using System.ComponentModel;

namespace MS.Internal;

internal static class SystemXmlLinqHelper
{
	internal static bool IsXElement(object item)
	{
		return AssemblyHelper.ExtensionsForSystemXmlLinq()?.IsXElement(item) ?? false;
	}

	internal static string GetXElementTagName(object item)
	{
		return AssemblyHelper.ExtensionsForSystemXmlLinq()?.GetXElementTagName(item);
	}

	internal static bool IsXLinqCollectionProperty(PropertyDescriptor pd)
	{
		return AssemblyHelper.ExtensionsForSystemXmlLinq()?.IsXLinqCollectionProperty(pd) ?? false;
	}

	internal static bool IsXLinqNonIdempotentProperty(PropertyDescriptor pd)
	{
		return AssemblyHelper.ExtensionsForSystemXmlLinq()?.IsXLinqNonIdempotentProperty(pd) ?? false;
	}
}
