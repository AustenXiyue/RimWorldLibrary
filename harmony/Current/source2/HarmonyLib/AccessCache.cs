using System;
using System.Collections.Generic;
using System.Reflection;

namespace HarmonyLib;

internal class AccessCache
{
	internal enum MemberType
	{
		Any,
		Static,
		Instance
	}

	private const BindingFlags BasicFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;

	private static readonly Dictionary<MemberType, BindingFlags> declaredOnlyBindingFlags = new Dictionary<MemberType, BindingFlags>
	{
		{
			MemberType.Any,
			BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty
		},
		{
			MemberType.Instance,
			BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty
		},
		{
			MemberType.Static,
			BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty
		}
	};

	private readonly Dictionary<Type, Dictionary<string, FieldInfo>> declaredFields = new Dictionary<Type, Dictionary<string, FieldInfo>>();

	private readonly Dictionary<Type, Dictionary<string, PropertyInfo>> declaredProperties = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

	private readonly Dictionary<Type, Dictionary<string, Dictionary<int, MethodBase>>> declaredMethods = new Dictionary<Type, Dictionary<string, Dictionary<int, MethodBase>>>();

	private readonly Dictionary<Type, Dictionary<string, FieldInfo>> inheritedFields = new Dictionary<Type, Dictionary<string, FieldInfo>>();

	private readonly Dictionary<Type, Dictionary<string, PropertyInfo>> inheritedProperties = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

	private readonly Dictionary<Type, Dictionary<string, Dictionary<int, MethodBase>>> inheritedMethods = new Dictionary<Type, Dictionary<string, Dictionary<int, MethodBase>>>();

	private static T Get<T>(Dictionary<Type, Dictionary<string, T>> dict, Type type, string name, Func<T> fetcher)
	{
		lock (dict)
		{
			if (!dict.TryGetValue(type, out var value))
			{
				value = (dict[type] = new Dictionary<string, T>());
			}
			if (!value.TryGetValue(name, out var value2))
			{
				value2 = (value[name] = fetcher());
			}
			return value2;
		}
	}

	private static T Get<T>(Dictionary<Type, Dictionary<string, Dictionary<int, T>>> dict, Type type, string name, Type[] arguments, Func<T> fetcher)
	{
		lock (dict)
		{
			if (!dict.TryGetValue(type, out var value))
			{
				value = (dict[type] = new Dictionary<string, Dictionary<int, T>>());
			}
			if (!value.TryGetValue(name, out var value2))
			{
				value2 = (value[name] = new Dictionary<int, T>());
			}
			int key = AccessTools.CombinedHashCode(arguments);
			if (!value2.TryGetValue(key, out var value3))
			{
				value3 = (value2[key] = fetcher());
			}
			return value3;
		}
	}

	internal FieldInfo GetFieldInfo(Type type, string name, MemberType memberType = MemberType.Any, bool declaredOnly = false)
	{
		FieldInfo fieldInfo = Get(declaredFields, type, name, () => type.GetField(name, declaredOnlyBindingFlags[memberType]));
		if ((object)fieldInfo == null && !declaredOnly)
		{
			fieldInfo = Get(inheritedFields, type, name, () => AccessTools.FindIncludingBaseTypes(type, (Type t) => t.GetField(name, AccessTools.all)));
		}
		return fieldInfo;
	}

	internal PropertyInfo GetPropertyInfo(Type type, string name, MemberType memberType = MemberType.Any, bool declaredOnly = false)
	{
		PropertyInfo propertyInfo = Get(declaredProperties, type, name, () => type.GetProperty(name, declaredOnlyBindingFlags[memberType]));
		if ((object)propertyInfo == null && !declaredOnly)
		{
			propertyInfo = Get(inheritedProperties, type, name, () => AccessTools.FindIncludingBaseTypes(type, (Type t) => t.GetProperty(name, AccessTools.all)));
		}
		return propertyInfo;
	}

	internal MethodBase GetMethodInfo(Type type, string name, Type[] arguments, MemberType memberType = MemberType.Any, bool declaredOnly = false)
	{
		MethodBase methodBase = Get(declaredMethods, type, name, arguments, () => type.GetMethod(name, declaredOnlyBindingFlags[memberType], null, arguments, null));
		if ((object)methodBase == null && !declaredOnly)
		{
			methodBase = Get(inheritedMethods, type, name, arguments, () => AccessTools.Method(type, name, arguments));
		}
		return methodBase;
	}
}
