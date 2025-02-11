namespace System.Windows.Markup;

internal class XamlPropertyIListEndNode : XamlPropertyComplexEndNode
{
	internal XamlPropertyIListEndNode(int lineNumber, int linePosition, int depth)
		: base(XamlNodeType.PropertyIListEnd, lineNumber, linePosition, depth)
	{
	}
}
