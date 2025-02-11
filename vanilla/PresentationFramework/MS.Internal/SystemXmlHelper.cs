using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace MS.Internal;

internal static class SystemXmlHelper
{
	internal static bool IsXmlNode(object item)
	{
		return AssemblyHelper.ExtensionsForSystemXml()?.IsXmlNode(item) ?? false;
	}

	internal static bool IsXmlNamespaceManager(object item)
	{
		return AssemblyHelper.ExtensionsForSystemXml()?.IsXmlNamespaceManager(item) ?? false;
	}

	internal static bool TryGetValueFromXmlNode(object item, string name, out object value)
	{
		SystemXmlExtensionMethods systemXmlExtensionMethods = AssemblyHelper.ExtensionsForSystemXml();
		if (systemXmlExtensionMethods != null)
		{
			return systemXmlExtensionMethods.TryGetValueFromXmlNode(item, name, out value);
		}
		value = null;
		return false;
	}

	internal static IComparer PrepareXmlComparer(IEnumerable collection, SortDescriptionCollection sort, CultureInfo culture)
	{
		return AssemblyHelper.ExtensionsForSystemXml()?.PrepareXmlComparer(collection, sort, culture);
	}

	internal static bool IsEmptyXmlDataCollection(object parent)
	{
		return AssemblyHelper.ExtensionsForSystemXml()?.IsEmptyXmlDataCollection(parent) ?? false;
	}

	internal static string GetXmlTagName(object item, DependencyObject target)
	{
		return AssemblyHelper.ExtensionsForSystemXml()?.GetXmlTagName(item, target);
	}

	internal static object FindXmlNodeWithInnerText(IEnumerable items, object innerText, out int index)
	{
		index = -1;
		SystemXmlExtensionMethods systemXmlExtensionMethods = AssemblyHelper.ExtensionsForSystemXml();
		if (systemXmlExtensionMethods == null)
		{
			return DependencyProperty.UnsetValue;
		}
		return systemXmlExtensionMethods.FindXmlNodeWithInnerText(items, innerText, out index);
	}

	internal static object GetInnerText(object item)
	{
		return AssemblyHelper.ExtensionsForSystemXml()?.GetInnerText(item);
	}
}
