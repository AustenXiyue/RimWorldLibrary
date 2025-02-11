using System.Diagnostics;

namespace System.Windows.Markup;

[DebuggerDisplay("Attr:{_value}")]
internal class XamlAttributeNode : XamlNode
{
	private string _value;

	internal string Value => _value;

	internal XamlAttributeNode(XamlNodeType tokenType, int lineNumber, int linePosition, int depth, string value)
		: base(tokenType, lineNumber, linePosition, depth)
	{
		_value = value;
	}
}
