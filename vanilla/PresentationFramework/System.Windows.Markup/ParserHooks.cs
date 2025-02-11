namespace System.Windows.Markup;

internal abstract class ParserHooks
{
	internal virtual ParserAction LoadNode(XamlNode tokenNode)
	{
		return ParserAction.Normal;
	}
}
