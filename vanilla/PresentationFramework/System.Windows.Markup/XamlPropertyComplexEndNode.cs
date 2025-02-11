namespace System.Windows.Markup;

internal class XamlPropertyComplexEndNode : XamlNode
{
	internal XamlPropertyComplexEndNode(int lineNumber, int linePosition, int depth)
		: base(XamlNodeType.PropertyComplexEnd, lineNumber, linePosition, depth)
	{
	}

	internal XamlPropertyComplexEndNode(XamlNodeType token, int lineNumber, int linePosition, int depth)
		: base(token, lineNumber, linePosition, depth)
	{
	}
}
