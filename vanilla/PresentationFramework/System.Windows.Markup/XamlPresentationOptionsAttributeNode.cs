namespace System.Windows.Markup;

internal class XamlPresentationOptionsAttributeNode : XamlAttributeNode
{
	private string _name;

	internal string Name => _name;

	internal XamlPresentationOptionsAttributeNode(int lineNumber, int linePosition, int depth, string name, string value)
		: base(XamlNodeType.PresentationOptionsAttribute, lineNumber, linePosition, depth, value)
	{
		_name = name;
	}
}
