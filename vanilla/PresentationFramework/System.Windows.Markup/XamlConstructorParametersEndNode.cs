namespace System.Windows.Markup;

internal class XamlConstructorParametersEndNode : XamlNode
{
	internal XamlConstructorParametersEndNode(int lineNumber, int linePosition, int depth)
		: base(XamlNodeType.ConstructorParametersEnd, lineNumber, linePosition, depth)
	{
	}
}
