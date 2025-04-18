using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Systems;

internal static class SystemVABI
{
	private static readonly ConditionalWeakTable<Type, StrongBox<bool>> SysVIsMemoryCache = new ConditionalWeakTable<Type, StrongBox<bool>>();

	public static TypeClassification ClassifyAMD64(Type type, bool isReturn)
	{
		int managedSize = type.GetManagedSize();
		if (managedSize > 16)
		{
			if (managedSize > 32)
			{
				if (!isReturn)
				{
					return TypeClassification.OnStack;
				}
				return TypeClassification.ByReference;
			}
			if (SysVIsMemoryCache.GetValue(type, (Type t) => new StrongBox<bool>(AnyFieldsNotFloat(t))).Value)
			{
				if (!isReturn)
				{
					return TypeClassification.OnStack;
				}
				return TypeClassification.ByReference;
			}
		}
		return TypeClassification.InRegister;
	}

	private static bool AnyFieldsNotFloat(Type type)
	{
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		for (int i = 0; i < fields.Length; i++)
		{
			Type fieldType = fields[i].FieldType;
			if ((object)fieldType != null && !fieldType.IsPrimitive && fieldType.IsValueType && AnyFieldsNotFloat(fieldType))
			{
				return true;
			}
			TypeCode typeCode = Type.GetTypeCode(fieldType);
			if (typeCode != TypeCode.Single && typeCode != TypeCode.Double)
			{
				return true;
			}
		}
		return false;
	}
}
