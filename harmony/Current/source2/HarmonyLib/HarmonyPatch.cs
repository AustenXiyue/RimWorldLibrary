using System;
using System.Collections.Generic;

namespace HarmonyLib;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = true)]
public class HarmonyPatch : HarmonyAttribute
{
	public HarmonyPatch()
	{
	}

	public HarmonyPatch(Type declaringType)
	{
		info.declaringType = declaringType;
	}

	public HarmonyPatch(Type declaringType, Type[] argumentTypes)
	{
		info.declaringType = declaringType;
		info.argumentTypes = argumentTypes;
	}

	public HarmonyPatch(Type declaringType, string methodName)
	{
		info.declaringType = declaringType;
		info.methodName = methodName;
	}

	public HarmonyPatch(Type declaringType, string methodName, params Type[] argumentTypes)
	{
		info.declaringType = declaringType;
		info.methodName = methodName;
		info.argumentTypes = argumentTypes;
	}

	public HarmonyPatch(Type declaringType, string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations)
	{
		info.declaringType = declaringType;
		info.methodName = methodName;
		ParseSpecialArguments(argumentTypes, argumentVariations);
	}

	public HarmonyPatch(Type declaringType, MethodType methodType)
	{
		info.declaringType = declaringType;
		info.methodType = methodType;
	}

	public HarmonyPatch(Type declaringType, MethodType methodType, params Type[] argumentTypes)
	{
		info.declaringType = declaringType;
		info.methodType = methodType;
		info.argumentTypes = argumentTypes;
	}

	public HarmonyPatch(Type declaringType, MethodType methodType, Type[] argumentTypes, ArgumentType[] argumentVariations)
	{
		info.declaringType = declaringType;
		info.methodType = methodType;
		ParseSpecialArguments(argumentTypes, argumentVariations);
	}

	public HarmonyPatch(Type declaringType, string methodName, MethodType methodType)
	{
		info.declaringType = declaringType;
		info.methodName = methodName;
		info.methodType = methodType;
	}

	public HarmonyPatch(string methodName)
	{
		info.methodName = methodName;
	}

	public HarmonyPatch(string methodName, params Type[] argumentTypes)
	{
		info.methodName = methodName;
		info.argumentTypes = argumentTypes;
	}

	public HarmonyPatch(string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations)
	{
		info.methodName = methodName;
		ParseSpecialArguments(argumentTypes, argumentVariations);
	}

	public HarmonyPatch(string methodName, MethodType methodType)
	{
		info.methodName = methodName;
		info.methodType = methodType;
	}

	public HarmonyPatch(MethodType methodType)
	{
		info.methodType = methodType;
	}

	public HarmonyPatch(MethodType methodType, params Type[] argumentTypes)
	{
		info.methodType = methodType;
		info.argumentTypes = argumentTypes;
	}

	public HarmonyPatch(MethodType methodType, Type[] argumentTypes, ArgumentType[] argumentVariations)
	{
		info.methodType = methodType;
		ParseSpecialArguments(argumentTypes, argumentVariations);
	}

	public HarmonyPatch(Type[] argumentTypes)
	{
		info.argumentTypes = argumentTypes;
	}

	public HarmonyPatch(Type[] argumentTypes, ArgumentType[] argumentVariations)
	{
		ParseSpecialArguments(argumentTypes, argumentVariations);
	}

	public HarmonyPatch(string typeName, string methodName, MethodType methodType = MethodType.Normal)
	{
		info.declaringType = AccessTools.TypeByName(typeName);
		info.methodName = methodName;
		info.methodType = methodType;
	}

	private void ParseSpecialArguments(Type[] argumentTypes, ArgumentType[] argumentVariations)
	{
		if (argumentVariations == null || argumentVariations.Length == 0)
		{
			info.argumentTypes = argumentTypes;
			return;
		}
		if (argumentTypes.Length < argumentVariations.Length)
		{
			throw new ArgumentException("argumentVariations contains more elements than argumentTypes", "argumentVariations");
		}
		List<Type> list = new List<Type>();
		for (int i = 0; i < argumentTypes.Length; i++)
		{
			Type type = argumentTypes[i];
			switch (argumentVariations[i])
			{
			case ArgumentType.Ref:
			case ArgumentType.Out:
				type = type.MakeByRefType();
				break;
			case ArgumentType.Pointer:
				type = type.MakePointerType();
				break;
			}
			list.Add(type);
		}
		info.argumentTypes = list.ToArray();
	}
}
