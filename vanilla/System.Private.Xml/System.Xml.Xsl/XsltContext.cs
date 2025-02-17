using System.Xml.XPath;

namespace System.Xml.Xsl;

public abstract class XsltContext : XmlNamespaceManager
{
	public abstract bool Whitespace { get; }

	protected XsltContext(NameTable table)
		: base(table)
	{
	}

	protected XsltContext()
		: base(new NameTable())
	{
	}

	internal XsltContext(bool _)
	{
	}

	public abstract IXsltContextVariable ResolveVariable(string prefix, string name);

	public abstract IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes);

	public abstract bool PreserveWhitespace(XPathNavigator node);

	public abstract int CompareDocument(string baseUri, string nextbaseUri);
}
