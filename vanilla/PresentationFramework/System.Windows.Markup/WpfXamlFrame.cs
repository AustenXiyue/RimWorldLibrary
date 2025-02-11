using System.Xaml;
using MS.Internal.Xaml.Context;

namespace System.Windows.Markup;

internal class WpfXamlFrame : MS.Internal.Xaml.Context.XamlFrame
{
	public bool FreezeFreezable { get; set; }

	public XamlMember Property { get; set; }

	public XamlType Type { get; set; }

	public object Instance { get; set; }

	public XmlnsDictionary XmlnsDictionary { get; set; }

	public bool? XmlSpace { get; set; }

	public override void Reset()
	{
		Type = null;
		Property = null;
		Instance = null;
		XmlnsDictionary = null;
		XmlSpace = null;
		if (FreezeFreezable)
		{
			FreezeFreezable = false;
		}
	}
}
