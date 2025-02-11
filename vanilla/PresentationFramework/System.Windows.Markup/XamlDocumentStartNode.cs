namespace System.Windows.Markup;

internal class XamlDocumentStartNode : XamlNode
{
	internal XamlDocumentStartNode(int lineNumber, int linePosition, int depth)
		: base(XamlNodeType.DocumentStart, lineNumber, linePosition, depth)
	{
	}
}
