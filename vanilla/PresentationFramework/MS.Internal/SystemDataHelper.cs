using System;
using System.ComponentModel;
using System.Reflection;

namespace MS.Internal;

internal static class SystemDataHelper
{
	internal static bool IsDataView(IBindingList list)
	{
		return AssemblyHelper.ExtensionsForSystemData()?.IsDataView(list) ?? false;
	}

	internal static bool IsDataRowView(object item)
	{
		return AssemblyHelper.ExtensionsForSystemData()?.IsDataRowView(item) ?? false;
	}

	internal static bool IsSqlNull(object value)
	{
		return AssemblyHelper.ExtensionsForSystemData()?.IsSqlNull(value) ?? false;
	}

	internal static bool IsSqlNullableType(Type type)
	{
		return AssemblyHelper.ExtensionsForSystemData()?.IsSqlNullableType(type) ?? false;
	}

	internal static bool IsDataSetCollectionProperty(PropertyDescriptor pd)
	{
		return AssemblyHelper.ExtensionsForSystemData()?.IsDataSetCollectionProperty(pd) ?? false;
	}

	internal static object GetValue(object item, PropertyDescriptor pd, bool useFollowParent)
	{
		return AssemblyHelper.ExtensionsForSystemData()?.GetValue(item, pd, useFollowParent);
	}

	internal static bool DetermineWhetherDBNullIsValid(object item, string columnName, object arg)
	{
		return AssemblyHelper.ExtensionsForSystemData()?.DetermineWhetherDBNullIsValid(item, columnName, arg) ?? false;
	}

	internal static object NullValueForSqlNullableType(Type type)
	{
		FieldInfo field = type.GetField("Null", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		if (field != null)
		{
			return field.GetValue(null);
		}
		PropertyInfo property = type.GetProperty("Null", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		if (property != null)
		{
			return property.GetValue(null, null);
		}
		return null;
	}
}
