namespace System.Windows.Markup;

internal class XamlUnknownTagEndNode : XamlNode
{
	private string _localName;

	private string _xmlNamespace;

	internal XamlUnknownTagEndNode(int lineNumber, int linePosition, int depth, string localName, string xmlNamespace)
		: base(XamlNodeType.UnknownTagEnd, lineNumber, linePosition, depth)
	{
		_localName = localName;
		_xmlNamespace = xmlNamespace;
	}
}
