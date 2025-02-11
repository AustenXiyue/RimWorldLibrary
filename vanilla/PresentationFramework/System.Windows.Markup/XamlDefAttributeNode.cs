namespace System.Windows.Markup;

internal class XamlDefAttributeNode : XamlAttributeNode
{
	private BamlAttributeUsage _attributeUsage;

	private string _name;

	internal string Name => _name;

	internal BamlAttributeUsage AttributeUsage => _attributeUsage;

	internal XamlDefAttributeNode(int lineNumber, int linePosition, int depth, string name, string value)
		: base(XamlNodeType.DefAttribute, lineNumber, linePosition, depth, value)
	{
		_attributeUsage = BamlAttributeUsage.Default;
		_name = name;
	}

	internal XamlDefAttributeNode(int lineNumber, int linePosition, int depth, string name, string value, BamlAttributeUsage bamlAttributeUsage)
		: base(XamlNodeType.DefAttribute, lineNumber, linePosition, depth, value)
	{
		_attributeUsage = bamlAttributeUsage;
		_name = name;
	}
}
