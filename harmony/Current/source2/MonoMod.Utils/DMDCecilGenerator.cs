using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace MonoMod.Utils;

internal sealed class DMDCecilGenerator : DMDGenerator<DMDCecilGenerator>
{
	protected override MethodInfo GenerateCore(DynamicMethodDefinition dmd, object? context)
	{
		MethodDefinition def = dmd.Definition ?? throw new InvalidOperationException();
		TypeDefinition typeDefinition = context as TypeDefinition;
		bool flag = false;
		ModuleDefinition module = typeDefinition?.Module;
		HashSet<string> hashSet = null;
		try
		{
			if (typeDefinition == null || module == null)
			{
				flag = true;
				string dumpName = dmd.GetDumpName("Cecil");
				module = ModuleDefinition.CreateModule(dumpName, new ModuleParameters
				{
					Kind = ModuleKind.Dll,
					ReflectionImporterProvider = MMReflectionImporter.ProviderNoDefault
				});
				hashSet = new HashSet<string>();
				module.Assembly.CustomAttributes.Add(new CustomAttribute(module.ImportReference(DynamicMethodDefinition.c_UnverifiableCodeAttribute)));
				if (dmd.Debug)
				{
					CustomAttribute customAttribute = new CustomAttribute(module.ImportReference(DynamicMethodDefinition.c_DebuggableAttribute));
					customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(module.ImportReference(typeof(DebuggableAttribute.DebuggingModes)), DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations));
					module.Assembly.CustomAttributes.Add(customAttribute);
				}
				typeDefinition = new TypeDefinition("", $"DMD<{dmd.OriginalMethod?.Name?.Replace('.', '_')}>?{GetHashCode()}", Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Abstract | Mono.Cecil.TypeAttributes.Sealed)
				{
					BaseType = module.TypeSystem.Object
				};
				module.Types.Add(typeDefinition);
			}
			MethodDefinition clone = null;
			new TypeReference("System.Runtime.CompilerServices", "IsVolatile", module, module.TypeSystem.CoreLibrary);
			Relinker relinker = delegate(IMetadataTokenProvider mtp, IGenericParameterProvider? ctx)
			{
				if (mtp == def)
				{
					return clone;
				}
				return (mtp is MethodReference methodReference && methodReference.FullName == def.FullName && methodReference.DeclaringType.FullName == def.DeclaringType.FullName && methodReference.DeclaringType.Scope.Name == def.DeclaringType.Scope.Name) ? clone : module.ImportReference(mtp);
			};
			clone = new MethodDefinition(dmd.Name ?? ("_" + def.Name.Replace('.', '_')), def.Attributes, module.TypeSystem.Void)
			{
				MethodReturnType = def.MethodReturnType,
				Attributes = (Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static | Mono.Cecil.MethodAttributes.HideBySig),
				ImplAttributes = Mono.Cecil.MethodImplAttributes.IL,
				DeclaringType = typeDefinition,
				NoInlining = true
			};
			foreach (ParameterDefinition parameter in def.Parameters)
			{
				clone.Parameters.Add(parameter.Clone().Relink(relinker, clone));
			}
			clone.ReturnType = def.ReturnType.Relink(relinker, clone);
			typeDefinition.Methods.Add(clone);
			clone.HasThis = def.HasThis;
			Mono.Cecil.Cil.MethodBody methodBody2 = (clone.Body = def.Body.Clone(clone));
			Mono.Cecil.Cil.MethodBody methodBody3 = methodBody2;
			foreach (VariableDefinition variable in clone.Body.Variables)
			{
				variable.VariableType = variable.VariableType.Relink(relinker, clone);
			}
			foreach (ExceptionHandler exceptionHandler in clone.Body.ExceptionHandlers)
			{
				if (exceptionHandler.CatchType != null)
				{
					exceptionHandler.CatchType = exceptionHandler.CatchType.Relink(relinker, clone);
				}
			}
			for (int i = 0; i < methodBody3.Instructions.Count; i++)
			{
				Instruction instruction = methodBody3.Instructions[i];
				object obj = instruction.Operand;
				if (obj is ParameterDefinition parameterDefinition)
				{
					obj = clone.Parameters[parameterDefinition.Index];
				}
				else if (obj is IMetadataTokenProvider mtp2)
				{
					obj = mtp2.Relink(relinker, clone);
				}
				_ = obj is DynamicMethodReference;
				if (hashSet != null && obj is MemberReference memberReference)
				{
					IMetadataScope metadataScope = (memberReference as TypeReference)?.Scope ?? memberReference.DeclaringType.Scope;
					if (!hashSet.Contains(metadataScope.Name))
					{
						CustomAttribute item = new CustomAttribute(module.ImportReference(DynamicMethodDefinition.c_IgnoresAccessChecksToAttribute))
						{
							ConstructorArguments = 
							{
								new CustomAttributeArgument(module.ImportReference(typeof(DebuggableAttribute.DebuggingModes)), metadataScope.Name)
							}
						};
						module.Assembly.CustomAttributes.Add(item);
						hashSet.Add(metadataScope.Name);
					}
				}
				instruction.Operand = obj;
			}
			clone.HasThis = false;
			if (def.HasThis)
			{
				TypeReference typeReference = def.DeclaringType;
				if (typeReference.IsValueType)
				{
					typeReference = new ByReferenceType(typeReference);
				}
				clone.Parameters.Insert(0, new ParameterDefinition("<>_this", Mono.Cecil.ParameterAttributes.None, typeReference.Relink(relinker, clone)));
			}
			object value;
			string text = (Switches.TryGetSwitchValue("DMDDumpTo", out value) ? (value as string) : null);
			if (!string.IsNullOrEmpty(text))
			{
				string fullPath = Path.GetFullPath(text);
				string path = module.Name + ".dll";
				string path2 = Path.Combine(fullPath, path);
				fullPath = Path.GetDirectoryName(path2);
				if (!string.IsNullOrEmpty(fullPath) && !Directory.Exists(fullPath))
				{
					Directory.CreateDirectory(fullPath);
				}
				if (File.Exists(path2))
				{
					File.Delete(path2);
				}
				using Stream stream = File.OpenWrite(path2);
				module.Write(stream);
			}
			return ReflectionHelper.Load(module).GetType(typeDefinition.FullName.Replace("+", "\\+", StringComparison.Ordinal), throwOnError: false, ignoreCase: false).GetMethod(clone.Name, BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Could not find generated method");
		}
		finally
		{
			if (flag)
			{
				module.Dispose();
			}
			module = null;
		}
	}
}
