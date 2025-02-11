namespace System.Windows.Markup;

internal class XamlElementEndNode : XamlNode
{
	internal XamlElementEndNode(int lineNumber, int linePosition, int depth)
		: this(XamlNodeType.ElementEnd, lineNumber, linePosition, depth)
	{
	}

	internal XamlElementEndNode(XamlNodeType tokenType, int lineNumber, int linePosition, int depth)
		: base(tokenType, lineNumber, linePosition, depth)
	{
	}
}
