using System.Xml;

namespace System.Windows.Markup;

internal class XamlDefTagNode : XamlAttributeNode
{
	internal XamlDefTagNode(int lineNumber, int linePosition, int depth, bool isEmptyElement, XmlReader xmlReader, string defTagName)
		: base(XamlNodeType.DefTag, lineNumber, linePosition, depth, defTagName)
	{
	}
}
