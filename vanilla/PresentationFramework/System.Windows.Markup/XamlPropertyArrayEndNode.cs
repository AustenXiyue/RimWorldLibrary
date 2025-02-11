namespace System.Windows.Markup;

internal class XamlPropertyArrayEndNode : XamlPropertyComplexEndNode
{
	internal XamlPropertyArrayEndNode(int lineNumber, int linePosition, int depth)
		: base(XamlNodeType.PropertyArrayEnd, lineNumber, linePosition, depth)
	{
	}
}
