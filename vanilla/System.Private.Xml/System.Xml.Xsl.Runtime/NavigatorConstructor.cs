using System.Xml.XPath;

namespace System.Xml.Xsl.Runtime;

internal sealed class NavigatorConstructor
{
	private object _cache;

	public XPathNavigator GetNavigator(XmlEventCache events, XmlNameTable nameTable)
	{
		if (_cache == null)
		{
			XPathDocument xPathDocument = new XPathDocument(nameTable);
			XmlRawWriter xmlRawWriter = xPathDocument.LoadFromWriter((XPathDocument.LoadFlags)(1 | ((!events.HasRootNode) ? 2 : 0)), events.BaseUri);
			events.EventsToWriter(xmlRawWriter);
			xmlRawWriter.Close();
			_cache = xPathDocument;
		}
		return ((XPathDocument)_cache).CreateNavigator();
	}

	public XPathNavigator GetNavigator(string text, string baseUri, XmlNameTable nameTable)
	{
		if (_cache == null)
		{
			XPathDocument xPathDocument = new XPathDocument(nameTable);
			XmlRawWriter xmlRawWriter = xPathDocument.LoadFromWriter(XPathDocument.LoadFlags.AtomizeNames, baseUri);
			xmlRawWriter.WriteString(text);
			xmlRawWriter.Close();
			_cache = xPathDocument;
		}
		return ((XPathDocument)_cache).CreateNavigator();
	}
}
