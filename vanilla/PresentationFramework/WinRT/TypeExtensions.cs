using System;
using System.Reflection;

namespace WinRT;

internal static class TypeExtensions
{
	public static Type FindHelperType(this Type type)
	{
		if (typeof(Exception).IsAssignableFrom(type))
		{
			type = typeof(Exception);
		}
		Type type2 = Projections.FindCustomHelperTypeMapping(type);
		if ((object)type2 != null)
		{
			return type2;
		}
		string typeName = "ABI." + type.FullName;
		string typeName2 = "MS.Internal.WindowsRuntime.ABI." + type.FullName;
		if (type.FullName.StartsWith("MS.Internal.WindowsRuntime."))
		{
			typeName = "MS.Internal.WindowsRuntime.ABI." + RemoveNamespacePrefix(type.FullName);
		}
		return Type.GetType(typeName) ?? Type.GetType(typeName2);
	}

	public static Type GetHelperType(this Type type)
	{
		Type type2 = type.FindHelperType();
		if ((object)type2 != null)
		{
			return type2;
		}
		throw new InvalidOperationException("Target type is not a projected type: " + type.FullName + ".");
	}

	public static Type GetGuidType(this Type type)
	{
		if (!type.IsDelegate())
		{
			return type;
		}
		return type.GetHelperType();
	}

	public static Type FindVftblType(this Type helperType)
	{
		Type type = helperType.GetNestedType("Vftbl");
		if ((object)type == null)
		{
			return null;
		}
		if (helperType.IsGenericType && (object)type != null)
		{
			type = type.MakeGenericType(helperType.GetGenericArguments());
		}
		return type;
	}

	public static Type GetAbiType(this Type type)
	{
		return type.GetHelperType().GetMethod("GetAbi", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).ReturnType;
	}

	public static Type GetMarshalerType(this Type type)
	{
		return type.GetHelperType().GetMethod("CreateMarshaler", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).ReturnType;
	}

	public static bool IsDelegate(this Type type)
	{
		return typeof(Delegate).IsAssignableFrom(type);
	}

	public static string RemoveNamespacePrefix(string ns)
	{
		if (ns.StartsWith("MS.Internal.WindowsRuntime."))
		{
			return ns.Substring("MS.Internal.WindowsRuntime.".Length);
		}
		return ns;
	}
}
