using System;

namespace HarmonyLib;

[AttributeUsage(AttributeTargets.Delegate, AllowMultiple = true)]
public class HarmonyDelegate : HarmonyPatch
{
	public HarmonyDelegate(Type declaringType)
		: base(declaringType)
	{
	}

	public HarmonyDelegate(Type declaringType, Type[] argumentTypes)
		: base(declaringType, argumentTypes)
	{
	}

	public HarmonyDelegate(Type declaringType, string methodName)
		: base(declaringType, methodName)
	{
	}

	public HarmonyDelegate(Type declaringType, string methodName, params Type[] argumentTypes)
		: base(declaringType, methodName, argumentTypes)
	{
	}

	public HarmonyDelegate(Type declaringType, string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations)
		: base(declaringType, methodName, argumentTypes, argumentVariations)
	{
	}

	public HarmonyDelegate(Type declaringType, MethodDispatchType methodDispatchType)
		: base(declaringType, MethodType.Normal)
	{
		info.nonVirtualDelegate = methodDispatchType == MethodDispatchType.Call;
	}

	public HarmonyDelegate(Type declaringType, MethodDispatchType methodDispatchType, params Type[] argumentTypes)
		: base(declaringType, MethodType.Normal, argumentTypes)
	{
		info.nonVirtualDelegate = methodDispatchType == MethodDispatchType.Call;
	}

	public HarmonyDelegate(Type declaringType, MethodDispatchType methodDispatchType, Type[] argumentTypes, ArgumentType[] argumentVariations)
		: base(declaringType, MethodType.Normal, argumentTypes, argumentVariations)
	{
		info.nonVirtualDelegate = methodDispatchType == MethodDispatchType.Call;
	}

	public HarmonyDelegate(Type declaringType, string methodName, MethodDispatchType methodDispatchType)
		: base(declaringType, methodName, MethodType.Normal)
	{
		info.nonVirtualDelegate = methodDispatchType == MethodDispatchType.Call;
	}

	public HarmonyDelegate(string methodName)
		: base(methodName)
	{
	}

	public HarmonyDelegate(string methodName, params Type[] argumentTypes)
		: base(methodName, argumentTypes)
	{
	}

	public HarmonyDelegate(string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations)
		: base(methodName, argumentTypes, argumentVariations)
	{
	}

	public HarmonyDelegate(string methodName, MethodDispatchType methodDispatchType)
		: base(methodName, MethodType.Normal)
	{
		info.nonVirtualDelegate = methodDispatchType == MethodDispatchType.Call;
	}

	public HarmonyDelegate(MethodDispatchType methodDispatchType)
	{
		info.nonVirtualDelegate = methodDispatchType == MethodDispatchType.Call;
	}

	public HarmonyDelegate(MethodDispatchType methodDispatchType, params Type[] argumentTypes)
		: base(MethodType.Normal, argumentTypes)
	{
		info.nonVirtualDelegate = methodDispatchType == MethodDispatchType.Call;
	}

	public HarmonyDelegate(MethodDispatchType methodDispatchType, Type[] argumentTypes, ArgumentType[] argumentVariations)
		: base(MethodType.Normal, argumentTypes, argumentVariations)
	{
		info.nonVirtualDelegate = methodDispatchType == MethodDispatchType.Call;
	}

	public HarmonyDelegate(Type[] argumentTypes)
		: base(argumentTypes)
	{
	}

	public HarmonyDelegate(Type[] argumentTypes, ArgumentType[] argumentVariations)
		: base(argumentTypes, argumentVariations)
	{
	}
}
