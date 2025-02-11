namespace System.Windows.Markup;

internal class XamlEndAttributesNode : XamlNode
{
	private bool _compact;

	internal bool IsCompact => _compact;

	internal XamlEndAttributesNode(int lineNumber, int linePosition, int depth, bool compact)
		: base(XamlNodeType.EndAttributes, lineNumber, linePosition, depth)
	{
		_compact = compact;
	}
}
