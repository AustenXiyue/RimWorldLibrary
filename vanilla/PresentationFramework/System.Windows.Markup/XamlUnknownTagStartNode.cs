namespace System.Windows.Markup;

internal class XamlUnknownTagStartNode : XamlAttributeNode
{
	private string _xmlNamespace;

	internal string XmlNamespace => _xmlNamespace;

	internal XamlUnknownTagStartNode(int lineNumber, int linePosition, int depth, string xmlNamespace, string value)
		: base(XamlNodeType.UnknownTagStart, lineNumber, linePosition, depth, value)
	{
		_xmlNamespace = xmlNamespace;
	}
}
