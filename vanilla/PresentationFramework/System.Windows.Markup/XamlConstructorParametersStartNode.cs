namespace System.Windows.Markup;

internal class XamlConstructorParametersStartNode : XamlNode
{
	internal XamlConstructorParametersStartNode(int lineNumber, int linePosition, int depth)
		: base(XamlNodeType.ConstructorParametersStart, lineNumber, linePosition, depth)
	{
	}
}
