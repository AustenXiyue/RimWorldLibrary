namespace System.Windows.Documents;

internal interface IXamlContentHandler
{
	XamlToRtfError StartDocument();

	XamlToRtfError EndDocument();

	XamlToRtfError StartPrefixMapping(string prefix, string uri);

	XamlToRtfError StartElement(string nameSpaceUri, string localName, string qName, IXamlAttributes attributes);

	XamlToRtfError EndElement(string nameSpaceUri, string localName, string qName);

	XamlToRtfError Characters(string characters);

	XamlToRtfError IgnorableWhitespace(string characters);

	XamlToRtfError ProcessingInstruction(string target, string data);

	XamlToRtfError SkippedEntity(string name);
}
