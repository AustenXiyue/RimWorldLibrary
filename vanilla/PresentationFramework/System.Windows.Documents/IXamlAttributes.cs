namespace System.Windows.Documents;

internal interface IXamlAttributes
{
	XamlToRtfError GetLength(ref int length);

	XamlToRtfError GetUri(int index, ref string uri);

	XamlToRtfError GetLocalName(int index, ref string localName);

	XamlToRtfError GetQName(int index, ref string qName);

	XamlToRtfError GetName(int index, ref string uri, ref string localName, ref string qName);

	XamlToRtfError GetIndexFromName(string uri, string localName, ref int index);

	XamlToRtfError GetIndexFromQName(string qName, ref int index);

	XamlToRtfError GetType(int index, ref string type);

	XamlToRtfError GetTypeFromName(string uri, string localName, ref string type);

	XamlToRtfError GetTypeFromQName(string qName, ref string type);

	XamlToRtfError GetValue(int index, ref string value);

	XamlToRtfError GetValueFromName(string uri, string localName, ref string value);

	XamlToRtfError GetValueFromQName(string qName, ref string value);
}
