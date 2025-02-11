using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MS.Internal.Ink;

internal class TextClipboardData : ElementsClipboardData
{
	private string _text;

	internal TextClipboardData()
		: this(null)
	{
	}

	internal TextClipboardData(string text)
	{
		_text = text;
	}

	internal override bool CanPaste(IDataObject dataObject)
	{
		if (!dataObject.GetDataPresent(DataFormats.UnicodeText, autoConvert: false) && !dataObject.GetDataPresent(DataFormats.Text, autoConvert: false))
		{
			return dataObject.GetDataPresent(DataFormats.OemText, autoConvert: false);
		}
		return true;
	}

	protected override bool CanCopy()
	{
		return !string.IsNullOrEmpty(_text);
	}

	protected override void DoCopy(IDataObject dataObject)
	{
		dataObject.SetData(DataFormats.UnicodeText, _text, autoConvert: true);
	}

	protected override void DoPaste(IDataObject dataObject)
	{
		base.ElementList = new List<UIElement>();
		string text = dataObject.GetData(DataFormats.UnicodeText, autoConvert: true) as string;
		if (string.IsNullOrEmpty(text))
		{
			text = dataObject.GetData(DataFormats.Text, autoConvert: true) as string;
		}
		if (!string.IsNullOrEmpty(text))
		{
			TextBox textBox = new TextBox();
			textBox.Text = text;
			textBox.TextWrapping = TextWrapping.Wrap;
			base.ElementList.Add(textBox);
		}
	}
}
