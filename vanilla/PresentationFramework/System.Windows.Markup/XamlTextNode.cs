using System.Diagnostics;

namespace System.Windows.Markup;

[DebuggerDisplay("Text:{_text}")]
internal class XamlTextNode : XamlNode
{
	private string _text;

	private Type _converterType;

	internal string Text => _text;

	internal Type ConverterType => _converterType;

	internal XamlTextNode(int lineNumber, int linePosition, int depth, string textContent, Type converterType)
		: base(XamlNodeType.Text, lineNumber, linePosition, depth)
	{
		_text = textContent;
		_converterType = converterType;
	}

	internal void UpdateText(string text)
	{
		_text = text;
	}
}
