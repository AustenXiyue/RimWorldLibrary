namespace System.Windows.Markup;

internal class XamlKeyElementEndNode : XamlElementEndNode
{
	internal XamlKeyElementEndNode(int lineNumber, int linePosition, int depth)
		: base(XamlNodeType.KeyElementEnd, lineNumber, linePosition, depth)
	{
	}
}
