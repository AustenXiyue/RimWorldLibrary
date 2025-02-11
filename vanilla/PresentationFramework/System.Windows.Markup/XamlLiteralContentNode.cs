using System.Diagnostics;

namespace System.Windows.Markup;

[DebuggerDisplay("Cont:{_content}")]
internal class XamlLiteralContentNode : XamlNode
{
	private string _content;

	internal string Content => _content;

	internal XamlLiteralContentNode(int lineNumber, int linePosition, int depth, string content)
		: base(XamlNodeType.LiteralContent, lineNumber, linePosition, depth)
	{
		_content = content;
	}
}
