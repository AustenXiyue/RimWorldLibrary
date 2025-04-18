using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace HarmonyLib;

internal static class PatchArgumentExtensions
{
	private static HarmonyArgument[] AllHarmonyArguments(object[] attributes)
	{
		return (from attr in attributes
			select (attr.GetType().Name != "HarmonyArgument") ? null : AccessTools.MakeDeepCopy<HarmonyArgument>(attr) into harg
			where harg != null
			select harg).ToArray();
	}

	private static HarmonyArgument GetArgumentAttribute(this ParameterInfo parameter)
	{
		object[] customAttributes = parameter.GetCustomAttributes(inherit: false);
		return AllHarmonyArguments(customAttributes).FirstOrDefault();
	}

	private static HarmonyArgument[] GetArgumentAttributes(this MethodInfo method)
	{
		if ((object)method == null || method is DynamicMethod)
		{
			return null;
		}
		object[] customAttributes = method.GetCustomAttributes(inherit: false);
		return AllHarmonyArguments(customAttributes);
	}

	private static HarmonyArgument[] GetArgumentAttributes(this Type type)
	{
		object[] customAttributes = type.GetCustomAttributes(inherit: false);
		return AllHarmonyArguments(customAttributes);
	}

	private static string GetOriginalArgumentName(this ParameterInfo parameter, string[] originalParameterNames)
	{
		HarmonyArgument argumentAttribute = parameter.GetArgumentAttribute();
		if (argumentAttribute == null)
		{
			return null;
		}
		if (!string.IsNullOrEmpty(argumentAttribute.OriginalName))
		{
			return argumentAttribute.OriginalName;
		}
		if (argumentAttribute.Index >= 0 && argumentAttribute.Index < originalParameterNames.Length)
		{
			return originalParameterNames[argumentAttribute.Index];
		}
		return null;
	}

	private static string GetOriginalArgumentName(HarmonyArgument[] attributes, string name, string[] originalParameterNames)
	{
		if (((attributes != null && attributes.Length != 0) ? 1 : 0) <= (false ? 1 : 0))
		{
			return null;
		}
		HarmonyArgument harmonyArgument = attributes.SingleOrDefault((HarmonyArgument p) => p.NewName == name);
		if (harmonyArgument == null)
		{
			return null;
		}
		if (!string.IsNullOrEmpty(harmonyArgument.OriginalName))
		{
			return harmonyArgument.OriginalName;
		}
		if (originalParameterNames != null && harmonyArgument.Index >= 0 && harmonyArgument.Index < originalParameterNames.Length)
		{
			return originalParameterNames[harmonyArgument.Index];
		}
		return null;
	}

	private static string GetOriginalArgumentName(this MethodInfo method, string[] originalParameterNames, string name)
	{
		string originalArgumentName = GetOriginalArgumentName(((object)method != null) ? method.GetArgumentAttributes() : null, name, originalParameterNames);
		if (originalArgumentName != null)
		{
			return originalArgumentName;
		}
		object attributes;
		if ((object)method == null)
		{
			attributes = null;
		}
		else
		{
			Type declaringType = method.DeclaringType;
			attributes = (((object)declaringType != null) ? declaringType.GetArgumentAttributes() : null);
		}
		originalArgumentName = GetOriginalArgumentName((HarmonyArgument[])attributes, name, originalParameterNames);
		if (originalArgumentName != null)
		{
			return originalArgumentName;
		}
		return name;
	}

	internal static int GetArgumentIndex(this MethodInfo patch, string[] originalParameterNames, ParameterInfo patchParam)
	{
		if (patch is DynamicMethod)
		{
			return Array.IndexOf(originalParameterNames, patchParam.Name);
		}
		string originalArgumentName = patchParam.GetOriginalArgumentName(originalParameterNames);
		if (originalArgumentName != null)
		{
			return Array.IndexOf(originalParameterNames, originalArgumentName);
		}
		originalArgumentName = patch.GetOriginalArgumentName(originalParameterNames, patchParam.Name);
		if (originalArgumentName != null)
		{
			return Array.IndexOf(originalParameterNames, originalArgumentName);
		}
		return -1;
	}
}
