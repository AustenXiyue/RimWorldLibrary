using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace MS.Internal;

internal abstract class SystemXmlExtensionMethods
{
	internal abstract bool IsXmlNode(object item);

	internal abstract bool IsXmlNamespaceManager(object item);

	internal abstract bool TryGetValueFromXmlNode(object item, string name, out object value);

	internal abstract IComparer PrepareXmlComparer(IEnumerable collection, SortDescriptionCollection sort, CultureInfo culture);

	internal abstract bool IsEmptyXmlDataCollection(object parent);

	internal abstract string GetXmlTagName(object item, DependencyObject target);

	internal abstract object FindXmlNodeWithInnerText(IEnumerable items, object innerText, out int index);

	internal abstract object GetInnerText(object item);
}
