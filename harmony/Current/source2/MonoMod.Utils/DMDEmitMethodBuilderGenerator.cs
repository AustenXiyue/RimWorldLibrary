using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Mono.Cecil;
using MonoMod.Logs;

namespace MonoMod.Utils;

internal sealed class DMDEmitMethodBuilderGenerator : DMDGenerator<DMDEmitMethodBuilderGenerator>
{
	private static readonly bool _MBCanRunAndCollect = Enum.IsDefined(typeof(AssemblyBuilderAccess), "RunAndCollect");

	protected override MethodInfo GenerateCore(DynamicMethodDefinition dmd, object? context)
	{
		TypeBuilder typeBuilder = context as TypeBuilder;
		MethodBuilder methodBuilder = GenerateMethodBuilder(dmd, typeBuilder);
		typeBuilder = (TypeBuilder)methodBuilder.DeclaringType;
		Type type = typeBuilder.CreateType();
		if (!string.IsNullOrEmpty(Switches.TryGetSwitchValue("DMDDumpTo", out object value) ? (value as string) : null))
		{
			string fullyQualifiedName = methodBuilder.Module.FullyQualifiedName;
			string fileName = Path.GetFileName(fullyQualifiedName);
			string directoryName = Path.GetDirectoryName(fullyQualifiedName);
			if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			if (File.Exists(fullyQualifiedName))
			{
				File.Delete(fullyQualifiedName);
			}
			((AssemblyBuilder)typeBuilder.Assembly).Save(fileName);
		}
		return type.GetMethod(methodBuilder.Name, BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static MethodBuilder GenerateMethodBuilder(DynamicMethodDefinition dmd, TypeBuilder? typeBuilder)
	{
		Helpers.ThrowIfArgumentNull(dmd, "dmd");
		MethodBase originalMethod = dmd.OriginalMethod;
		MethodDefinition definition = dmd.Definition;
		if (typeBuilder == null)
		{
			string text = (Switches.TryGetSwitchValue("DMDDumpTo", out object value) ? (value as string) : null);
			text = ((!string.IsNullOrEmpty(text)) ? Path.GetFullPath(text) : null);
			bool flag = string.IsNullOrEmpty(text) && _MBCanRunAndCollect;
			AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName
			{
				Name = dmd.GetDumpName("MethodBuilder")
			}, flag ? AssemblyBuilderAccess.RunAndCollect : AssemblyBuilderAccess.RunAndSave, text);
			assemblyBuilder.SetCustomAttribute(new CustomAttributeBuilder(DynamicMethodDefinition.c_UnverifiableCodeAttribute, new object[0]));
			if (dmd.Debug)
			{
				assemblyBuilder.SetCustomAttribute(new CustomAttributeBuilder(DynamicMethodDefinition.c_DebuggableAttribute, new object[1] { DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations }));
			}
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyBuilder.GetName().Name + ".dll", assemblyBuilder.GetName().Name + ".dll", dmd.Debug);
			FormatInterpolatedStringHandler handler = new FormatInterpolatedStringHandler(6, 2);
			handler.AppendLiteral("DMD<");
			handler.AppendFormatted(originalMethod);
			handler.AppendLiteral(">?");
			handler.AppendFormatted(dmd.GetHashCode());
			typeBuilder = moduleBuilder.DefineType(DebugFormatter.Format(ref handler), System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Abstract | System.Reflection.TypeAttributes.Sealed);
		}
		Type[] array;
		Type[][] array2;
		Type[][] array3;
		if (originalMethod != null)
		{
			ParameterInfo[] parameters = originalMethod.GetParameters();
			int num = 0;
			if (!originalMethod.IsStatic)
			{
				num++;
				array = new Type[parameters.Length + 1];
				array2 = new Type[parameters.Length + 1][];
				array3 = new Type[parameters.Length + 1][];
				array[0] = originalMethod.GetThisParamType();
				array2[0] = Type.EmptyTypes;
				array3[0] = Type.EmptyTypes;
			}
			else
			{
				array = new Type[parameters.Length];
				array2 = new Type[parameters.Length][];
				array3 = new Type[parameters.Length][];
			}
			for (int i = 0; i < parameters.Length; i++)
			{
				array[i + num] = parameters[i].ParameterType;
				array2[i + num] = parameters[i].GetRequiredCustomModifiers();
				array3[i + num] = parameters[i].GetOptionalCustomModifiers();
			}
		}
		else
		{
			int num2 = 0;
			if (definition.HasThis)
			{
				num2++;
				array = new Type[definition.Parameters.Count + 1];
				array2 = new Type[definition.Parameters.Count + 1][];
				array3 = new Type[definition.Parameters.Count + 1][];
				Type type = definition.DeclaringType.ResolveReflection();
				if (type.IsValueType)
				{
					type = type.MakeByRefType();
				}
				array[0] = type;
				array2[0] = Type.EmptyTypes;
				array3[0] = Type.EmptyTypes;
			}
			else
			{
				array = new Type[definition.Parameters.Count];
				array2 = new Type[definition.Parameters.Count][];
				array3 = new Type[definition.Parameters.Count][];
			}
			List<Type> modReq = new List<Type>();
			List<Type> modOpt = new List<Type>();
			for (int j = 0; j < definition.Parameters.Count; j++)
			{
				_DMDEmit.ResolveWithModifiers(definition.Parameters[j].ParameterType, out Type type2, out Type[] typeModReq, out Type[] typeModOpt, modReq, modOpt);
				array[j + num2] = type2;
				array2[j + num2] = typeModReq;
				array3[j + num2] = typeModOpt;
			}
		}
		_DMDEmit.ResolveWithModifiers(definition.ReturnType, out Type type3, out Type[] typeModReq2, out Type[] typeModOpt2);
		MethodBuilder methodBuilder = typeBuilder.DefineMethod(dmd.Name ?? (originalMethod?.Name ?? definition.Name).Replace('.', '_'), System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static | System.Reflection.MethodAttributes.HideBySig, CallingConventions.Standard, type3, typeModReq2, typeModOpt2, array, array2, array3);
		ILGenerator iLGenerator = methodBuilder.GetILGenerator();
		_DMDEmit.Generate(dmd, methodBuilder, iLGenerator);
		return methodBuilder;
	}
}
