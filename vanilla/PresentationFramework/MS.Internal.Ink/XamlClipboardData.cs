using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Xml;

namespace MS.Internal.Ink;

internal class XamlClipboardData : ElementsClipboardData
{
	internal XamlClipboardData()
	{
	}

	internal XamlClipboardData(UIElement[] elements)
		: base(elements)
	{
	}

	internal override bool CanPaste(IDataObject dataObject)
	{
		return dataObject.GetDataPresent(DataFormats.Xaml, autoConvert: false);
	}

	protected override bool CanCopy()
	{
		if (base.Elements != null)
		{
			return base.Elements.Count != 0;
		}
		return false;
	}

	protected override void DoCopy(IDataObject dataObject)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (UIElement element in base.Elements)
		{
			string value = XamlWriter.Save(element);
			stringBuilder.Append(value);
		}
		dataObject.SetData(DataFormats.Xaml, stringBuilder.ToString());
	}

	protected override void DoPaste(IDataObject dataObject)
	{
		base.ElementList = new List<UIElement>();
		string text = dataObject.GetData(DataFormats.Xaml) as string;
		if (!string.IsNullOrEmpty(text) && XamlReader.Load(new XmlTextReader(new StringReader(text)), useRestrictiveXamlReader: true) is UIElement item)
		{
			base.ElementList.Add(item);
		}
	}
}
