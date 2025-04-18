using System;
using System.Reflection;
using System.Reflection.Emit;

namespace HarmonyLib;

public class DelegateTypeFactory
{
	private readonly ModuleBuilder module;

	private static int counter;

	public DelegateTypeFactory()
	{
		counter++;
		string name = $"HarmonyDTFAssembly{counter}";
		AssemblyBuilder assemblyBuilder = PatchTools.DefineDynamicAssembly(name);
		module = assemblyBuilder.DefineDynamicModule($"HarmonyDTFModule{counter}");
	}

	public Type CreateDelegateType(MethodInfo method)
	{
		TypeAttributes attr = TypeAttributes.Public | TypeAttributes.Sealed;
		TypeBuilder typeBuilder = module.DefineType($"HarmonyDTFType{counter}", attr, typeof(MulticastDelegate));
		ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName, CallingConventions.Standard, new Type[2]
		{
			typeof(object),
			typeof(IntPtr)
		});
		constructorBuilder.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
		ParameterInfo[] parameters = method.GetParameters();
		MethodBuilder methodBuilder = typeBuilder.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, method.ReturnType, parameters.Types());
		methodBuilder.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
		for (int i = 0; i < parameters.Length; i++)
		{
			methodBuilder.DefineParameter(i + 1, ParameterAttributes.None, parameters[i].Name);
		}
		return typeBuilder.CreateType();
	}
}
