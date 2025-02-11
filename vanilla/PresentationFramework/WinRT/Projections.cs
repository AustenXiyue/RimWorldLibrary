using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ABI.System;
using MS.Internal.WindowsRuntime.ABI.System.Collections.Generic;

namespace WinRT;

internal static class Projections
{
	private static readonly ReaderWriterLockSlim rwlock;

	private static readonly Dictionary<Type, Type> CustomTypeToHelperTypeMappings;

	private static readonly Dictionary<Type, Type> CustomAbiTypeToTypeMappings;

	private static readonly Dictionary<string, Type> CustomAbiTypeNameToTypeMappings;

	private static readonly Dictionary<Type, string> CustomTypeToAbiTypeNameMappings;

	private static readonly HashSet<string> ProjectedRuntimeClassNames;

	static Projections()
	{
		rwlock = new ReaderWriterLockSlim();
		CustomTypeToHelperTypeMappings = new Dictionary<Type, Type>();
		CustomAbiTypeToTypeMappings = new Dictionary<Type, Type>();
		CustomAbiTypeNameToTypeMappings = new Dictionary<string, Type>();
		CustomTypeToAbiTypeNameMappings = new Dictionary<Type, string>();
		ProjectedRuntimeClassNames = new HashSet<string>();
		RegisterCustomAbiTypeMappingNoLock(typeof(bool), typeof(ABI.System.Boolean), "Boolean");
		RegisterCustomAbiTypeMappingNoLock(typeof(char), typeof(ABI.System.Char), "Char");
		RegisterCustomAbiTypeMappingNoLock(typeof(System.Collections.Generic.IReadOnlyList<>), typeof(MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IReadOnlyList<>), "Windows.Foundation.Collections.IVectorView`1");
	}

	private static void RegisterCustomAbiTypeMappingNoLock(Type publicType, Type abiType, string winrtTypeName, bool isRuntimeClass = false)
	{
		CustomTypeToHelperTypeMappings.Add(publicType, abiType);
		CustomAbiTypeToTypeMappings.Add(abiType, publicType);
		CustomTypeToAbiTypeNameMappings.Add(publicType, winrtTypeName);
		CustomAbiTypeNameToTypeMappings.Add(winrtTypeName, publicType);
		if (isRuntimeClass)
		{
			ProjectedRuntimeClassNames.Add(winrtTypeName);
		}
	}

	public static Type FindCustomHelperTypeMapping(Type publicType)
	{
		rwlock.EnterReadLock();
		try
		{
			Type value;
			if (publicType.IsGenericType)
			{
				return CustomTypeToHelperTypeMappings.TryGetValue(publicType.GetGenericTypeDefinition(), out value) ? value.MakeGenericType(publicType.GetGenericArguments()) : null;
			}
			Type value2;
			return CustomTypeToHelperTypeMappings.TryGetValue(publicType, out value2) ? value2 : null;
		}
		finally
		{
			rwlock.ExitReadLock();
		}
	}

	public static Type FindCustomPublicTypeForAbiType(Type abiType)
	{
		rwlock.EnterReadLock();
		try
		{
			Type value;
			if (abiType.IsGenericType)
			{
				return CustomAbiTypeToTypeMappings.TryGetValue(abiType.GetGenericTypeDefinition(), out value) ? value.MakeGenericType(abiType.GetGenericArguments()) : null;
			}
			Type value2;
			return CustomAbiTypeToTypeMappings.TryGetValue(abiType, out value2) ? value2 : null;
		}
		finally
		{
			rwlock.ExitReadLock();
		}
	}

	public static Type FindCustomTypeForAbiTypeName(string abiTypeName)
	{
		rwlock.EnterReadLock();
		try
		{
			Type value;
			return CustomAbiTypeNameToTypeMappings.TryGetValue(abiTypeName, out value) ? value : null;
		}
		finally
		{
			rwlock.ExitReadLock();
		}
	}

	public static string FindCustomAbiTypeNameForType(Type type)
	{
		rwlock.EnterReadLock();
		try
		{
			string value;
			return CustomTypeToAbiTypeNameMappings.TryGetValue(type, out value) ? value : null;
		}
		finally
		{
			rwlock.ExitReadLock();
		}
	}

	public static bool IsTypeWindowsRuntimeType(Type type)
	{
		Type type2 = type;
		if (type2.IsArray)
		{
			type2 = type2.GetElementType();
		}
		return IsTypeWindowsRuntimeTypeNoArray(type2);
	}

	private static bool IsTypeWindowsRuntimeTypeNoArray(Type type)
	{
		if (type.IsConstructedGenericType)
		{
			if (IsTypeWindowsRuntimeTypeNoArray(type.GetGenericTypeDefinition()))
			{
				Type[] genericArguments = type.GetGenericArguments();
				for (int i = 0; i < genericArguments.Length; i++)
				{
					if (!IsTypeWindowsRuntimeTypeNoArray(genericArguments[i]))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
		if (!CustomTypeToAbiTypeNameMappings.ContainsKey(type) && !type.IsPrimitive && !(type == typeof(string)) && !(type == typeof(Guid)) && !(type == typeof(object)))
		{
			return type.GetCustomAttribute<WindowsRuntimeTypeAttribute>() != null;
		}
		return true;
	}

	public static bool TryGetCompatibleWindowsRuntimeTypeForVariantType(Type type, out Type compatibleType)
	{
		compatibleType = null;
		if (!type.IsConstructedGenericType)
		{
			throw new ArgumentException("type");
		}
		Type genericTypeDefinition = type.GetGenericTypeDefinition();
		if (!IsTypeWindowsRuntimeTypeNoArray(genericTypeDefinition))
		{
			return false;
		}
		Type[] genericArguments = genericTypeDefinition.GetGenericArguments();
		Type[] genericArguments2 = type.GetGenericArguments();
		Type[] array = new Type[genericArguments2.Length];
		for (int i = 0; i < genericArguments2.Length; i++)
		{
			if (!IsTypeWindowsRuntimeTypeNoArray(genericArguments2[i]))
			{
				if ((genericArguments[i].GenericParameterAttributes & GenericParameterAttributes.VarianceMask) != GenericParameterAttributes.Covariant || genericArguments2[i].IsValueType)
				{
					return false;
				}
				array[i] = typeof(object);
			}
			else
			{
				array[i] = genericArguments2[i];
			}
		}
		compatibleType = genericTypeDefinition.MakeGenericType(array);
		return true;
	}

	internal static bool TryGetDefaultInterfaceTypeForRuntimeClassType(Type runtimeClass, out Type defaultInterface)
	{
		defaultInterface = null;
		ProjectedRuntimeClassAttribute customAttribute = runtimeClass.GetCustomAttribute<ProjectedRuntimeClassAttribute>();
		if (customAttribute == null)
		{
			return false;
		}
		defaultInterface = runtimeClass.GetProperty(customAttribute.DefaultInterfaceProperty, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).PropertyType;
		return true;
	}

	internal static Type GetDefaultInterfaceTypeForRuntimeClassType(Type runtimeClass)
	{
		if (!TryGetDefaultInterfaceTypeForRuntimeClassType(runtimeClass, out var defaultInterface))
		{
			throw new ArgumentException("The provided type '" + runtimeClass.FullName + "' is not a WinRT projected runtime class.", "runtimeClass");
		}
		return defaultInterface;
	}

	internal static bool TryGetMarshalerTypeForProjectedRuntimeClass(IObjectReference objectReference, out Type type)
	{
		if (objectReference.TryAs(out ObjectReference<IInspectable.Vftbl> objRef) == 0)
		{
			rwlock.EnterReadLock();
			try
			{
				string runtimeClassName = ((IInspectable)objRef).GetRuntimeClassName(noThrow: true);
				if (runtimeClassName != null && ProjectedRuntimeClassNames.Contains(runtimeClassName))
				{
					type = CustomTypeToHelperTypeMappings[CustomAbiTypeNameToTypeMappings[runtimeClassName]];
					return true;
				}
			}
			finally
			{
				objRef.Dispose();
				rwlock.ExitReadLock();
			}
		}
		type = null;
		return false;
	}
}
