using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace MonoMod.Utils.Cil;

internal abstract class ILGeneratorShim
{
	internal static class ILGeneratorBuilder
	{
		public const string Namespace = "MonoMod.Utils.Cil";

		public const string Name = "ILGeneratorProxy";

		public const string FullName = "MonoMod.Utils.Cil.ILGeneratorProxy";

		public const string TargetName = "Target";

		private static Type? ProxyType;

		public static Type GenerateProxy()
		{
			if (ProxyType != null)
			{
				return ProxyType;
			}
			Type typeFromHandle = typeof(ILGenerator);
			Type typeFromHandle2 = typeof(ILGeneratorShim);
			Assembly assembly;
			using (ModuleDefinition moduleDefinition = ModuleDefinition.CreateModule("MonoMod.Utils.Cil.ILGeneratorProxy", new ModuleParameters
			{
				Kind = ModuleKind.Dll,
				ReflectionImporterProvider = MMReflectionImporter.Provider
			}))
			{
				CustomAttribute customAttribute = new CustomAttribute(moduleDefinition.ImportReference(DynamicMethodDefinition.c_IgnoresAccessChecksToAttribute));
				customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(moduleDefinition.TypeSystem.String, typeof(ILGeneratorShim).Assembly.GetName().Name));
				moduleDefinition.Assembly.CustomAttributes.Add(customAttribute);
				TypeDefinition typeDefinition = new TypeDefinition("MonoMod.Utils.Cil", "ILGeneratorProxy", Mono.Cecil.TypeAttributes.Public)
				{
					BaseType = moduleDefinition.ImportReference(typeFromHandle)
				};
				moduleDefinition.Types.Add(typeDefinition);
				TypeReference constraintType = moduleDefinition.ImportReference(typeFromHandle2);
				GenericParameter genericParameter = new GenericParameter("TTarget", typeDefinition);
				genericParameter.Constraints.Add(new GenericParameterConstraint(constraintType));
				typeDefinition.GenericParameters.Add(genericParameter);
				FieldDefinition item = new FieldDefinition("Target", Mono.Cecil.FieldAttributes.Public, genericParameter);
				typeDefinition.Fields.Add(item);
				GenericInstanceType genericInstanceType = new GenericInstanceType(typeDefinition);
				genericInstanceType.GenericArguments.Add(genericParameter);
				FieldReference field = new FieldReference("Target", genericParameter, genericInstanceType);
				MethodDefinition methodDefinition = new MethodDefinition(".ctor", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.SpecialName | Mono.Cecil.MethodAttributes.RTSpecialName, moduleDefinition.TypeSystem.Void);
				methodDefinition.Parameters.Add(new ParameterDefinition(genericParameter));
				typeDefinition.Methods.Add(methodDefinition);
				ILProcessor iLProcessor = methodDefinition.Body.GetILProcessor();
				iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
				iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_1);
				iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Stfld, field);
				iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ret);
				MethodInfo[] methods = typeFromHandle.GetMethods(BindingFlags.Instance | BindingFlags.Public);
				foreach (MethodInfo methodInfo in methods)
				{
					MethodInfo method = typeFromHandle2.GetMethod(methodInfo.Name, (from p in methodInfo.GetParameters()
						select p.ParameterType).ToArray());
					if (method == null)
					{
						continue;
					}
					MethodDefinition methodDefinition2 = new MethodDefinition(methodInfo.Name, Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Virtual | Mono.Cecil.MethodAttributes.HideBySig, moduleDefinition.ImportReference(methodInfo.ReturnType))
					{
						HasThis = true
					};
					ParameterInfo[] parameters = methodInfo.GetParameters();
					foreach (ParameterInfo parameterInfo in parameters)
					{
						methodDefinition2.Parameters.Add(new ParameterDefinition(moduleDefinition.ImportReference(parameterInfo.ParameterType)));
					}
					typeDefinition.Methods.Add(methodDefinition2);
					iLProcessor = methodDefinition2.Body.GetILProcessor();
					iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
					iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ldfld, field);
					foreach (ParameterDefinition parameter in methodDefinition2.Parameters)
					{
						iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ldarg, parameter);
					}
					iLProcessor.Emit(method.IsVirtual ? Mono.Cecil.Cil.OpCodes.Callvirt : Mono.Cecil.Cil.OpCodes.Call, iLProcessor.Body.Method.Module.ImportReference(method));
					iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ret);
				}
				assembly = ReflectionHelper.Load(moduleDefinition);
				assembly.SetMonoCorlibInternal(value: true);
			}
			ResolveEventHandler value = (object asmSender, ResolveEventArgs asmArgs) => (new AssemblyName(asmArgs.Name).Name == typeof(ILGeneratorBuilder).Assembly.GetName().Name) ? typeof(ILGeneratorBuilder).Assembly : null;
			AppDomain.CurrentDomain.AssemblyResolve += value;
			try
			{
				ProxyType = assembly.GetType("MonoMod.Utils.Cil.ILGeneratorProxy");
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= value;
			}
			if (ProxyType == null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("Couldn't find ILGeneratorShim proxy \"").Append("MonoMod.Utils.Cil.ILGeneratorProxy").Append("\" in autogenerated \"")
					.Append(assembly.FullName)
					.AppendLine("\"");
				Type[] types;
				Exception[] array;
				try
				{
					types = assembly.GetTypes();
					array = null;
				}
				catch (ReflectionTypeLoadException ex)
				{
					types = ex.Types;
					array = new Exception[ex.LoaderExceptions.Length + 1];
					array[0] = ex;
					for (int k = 0; k < ex.LoaderExceptions.Length; k++)
					{
						array[k + 1] = ex.LoaderExceptions[k];
					}
				}
				stringBuilder.AppendLine("Listing all types in autogenerated assembly:");
				Type[] array2 = types;
				for (int i = 0; i < array2.Length; i++)
				{
					stringBuilder.AppendLine(array2[i]?.FullName ?? "<NULL>");
				}
				if (array != null && array.Length != 0)
				{
					stringBuilder.AppendLine("Listing all exceptions:");
					for (int l = 0; l < array.Length; l++)
					{
						stringBuilder.Append('#').Append(l).Append(": ")
							.AppendLine(array[l]?.ToString() ?? "NULL");
					}
				}
				throw new InvalidOperationException(stringBuilder.ToString());
			}
			return ProxyType;
		}
	}

	public abstract int ILOffset { get; }

	public static Type GenericProxyType => ILGeneratorBuilder.GenerateProxy();

	public abstract void BeginCatchBlock(Type exceptionType);

	public abstract void BeginExceptFilterBlock();

	public abstract Label BeginExceptionBlock();

	public abstract void BeginFaultBlock();

	public abstract void BeginFinallyBlock();

	public abstract void BeginScope();

	public abstract LocalBuilder DeclareLocal(Type localType);

	public abstract LocalBuilder DeclareLocal(Type localType, bool pinned);

	public abstract Label DefineLabel();

	public abstract void Emit(System.Reflection.Emit.OpCode opcode);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, byte arg);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, double arg);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, short arg);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, int arg);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, long arg);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, ConstructorInfo con);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, Label label);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, Label[] labels);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, LocalBuilder local);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, SignatureHelper signature);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, FieldInfo field);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, MethodInfo meth);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, sbyte arg);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, float arg);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, string str);

	public abstract void Emit(System.Reflection.Emit.OpCode opcode, Type cls);

	public abstract void EmitCall(System.Reflection.Emit.OpCode opcode, MethodInfo methodInfo, Type[]? optionalParameterTypes);

	public abstract void EmitCalli(System.Reflection.Emit.OpCode opcode, CallingConventions callingConvention, Type? returnType, Type[]? parameterTypes, Type[]? optionalParameterTypes);

	public abstract void EmitCalli(System.Reflection.Emit.OpCode opcode, CallingConvention unmanagedCallConv, Type? returnType, Type[]? parameterTypes);

	public abstract void EmitWriteLine(LocalBuilder localBuilder);

	public abstract void EmitWriteLine(FieldInfo fld);

	public abstract void EmitWriteLine(string value);

	public abstract void EndExceptionBlock();

	public abstract void EndScope();

	public abstract void MarkLabel(Label loc);

	public abstract void ThrowException(Type excType);

	public abstract void UsingNamespace(string usingNamespace);

	public ILGenerator GetProxy()
	{
		return (ILGenerator)ILGeneratorBuilder.GenerateProxy().MakeGenericType(GetType()).GetConstructors()[0].Invoke(new object[1] { this });
	}

	public static Type GetProxyType<TShim>() where TShim : ILGeneratorShim
	{
		return GetProxyType(typeof(TShim));
	}

	public static Type GetProxyType(Type tShim)
	{
		return GenericProxyType.MakeGenericType(tShim);
	}
}
