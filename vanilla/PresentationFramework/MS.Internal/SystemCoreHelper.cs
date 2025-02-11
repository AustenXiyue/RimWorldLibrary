using System;

namespace MS.Internal;

internal static class SystemCoreHelper
{
	internal static bool IsIDynamicMetaObjectProvider(object item)
	{
		return AssemblyHelper.ExtensionsForSystemCore()?.IsIDynamicMetaObjectProvider(item) ?? false;
	}

	internal static object NewDynamicPropertyAccessor(Type ownerType, string propertyName)
	{
		return AssemblyHelper.ExtensionsForSystemCore()?.NewDynamicPropertyAccessor(ownerType, propertyName);
	}

	internal static object GetIndexerAccessor(int rank)
	{
		return AssemblyHelper.ExtensionsForSystemCore()?.GetIndexerAccessor(rank);
	}
}
