using System.Diagnostics;

namespace System.Windows.Markup;

[DebuggerDisplay("Xmlns:{_prefix)={_xmlNamespace}")]
internal class XamlXmlnsPropertyNode : XamlNode
{
	private string _prefix;

	private string _xmlNamespace;

	internal string Prefix => _prefix;

	internal string XmlNamespace => _xmlNamespace;

	internal XamlXmlnsPropertyNode(int lineNumber, int linePosition, int depth, string prefix, string xmlNamespace)
		: base(XamlNodeType.XmlnsProperty, lineNumber, linePosition, depth)
	{
		_prefix = prefix;
		_xmlNamespace = xmlNamespace;
	}
}
