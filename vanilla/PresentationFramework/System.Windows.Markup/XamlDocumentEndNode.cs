namespace System.Windows.Markup;

internal class XamlDocumentEndNode : XamlNode
{
	internal XamlDocumentEndNode(int lineNumber, int linePosition, int depth)
		: base(XamlNodeType.DocumentEnd, lineNumber, linePosition, depth)
	{
	}
}
