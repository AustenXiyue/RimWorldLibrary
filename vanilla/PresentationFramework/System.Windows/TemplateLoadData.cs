using System.Collections.Generic;
using System.Xaml;

namespace System.Windows;

internal class TemplateLoadData
{
	internal Dictionary<string, XamlType> _namedTypes;

	internal TemplateContent.StackOfFrames Stack { get; set; }

	internal Dictionary<string, XamlType> NamedTypes
	{
		get
		{
			if (_namedTypes == null)
			{
				_namedTypes = new Dictionary<string, XamlType>();
			}
			return _namedTypes;
		}
	}

	internal XamlReader Reader { get; set; }

	internal string RootName { get; set; }

	internal object RootObject { get; set; }

	internal TemplateContent.ServiceProviderWrapper ServiceProviderWrapper { get; set; }

	internal XamlObjectWriter ObjectWriter { get; set; }

	internal TemplateLoadData()
	{
	}
}
