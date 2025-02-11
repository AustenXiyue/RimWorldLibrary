namespace System.Windows.Markup;

internal class XamlUnknownAttributeNode : XamlAttributeNode
{
	private string _xmlNamespace;

	private string _name;

	private BamlAttributeUsage _attributeUsage;

	internal string XmlNamespace => _xmlNamespace;

	internal string Name => _name;

	internal XamlUnknownAttributeNode(int lineNumber, int linePosition, int depth, string xmlNamespace, string name, string value, BamlAttributeUsage attributeUsage)
		: base(XamlNodeType.UnknownAttribute, lineNumber, linePosition, depth, value)
	{
		_xmlNamespace = xmlNamespace;
		_name = name;
		_attributeUsage = attributeUsage;
	}
}
