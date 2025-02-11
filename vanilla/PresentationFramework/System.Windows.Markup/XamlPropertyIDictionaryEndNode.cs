namespace System.Windows.Markup;

internal class XamlPropertyIDictionaryEndNode : XamlPropertyComplexEndNode
{
	internal XamlPropertyIDictionaryEndNode(int lineNumber, int linePosition, int depth)
		: base(XamlNodeType.PropertyIDictionaryEnd, lineNumber, linePosition, depth)
	{
	}
}
