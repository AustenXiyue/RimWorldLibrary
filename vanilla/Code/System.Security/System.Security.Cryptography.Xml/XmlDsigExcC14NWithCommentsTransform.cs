namespace System.Security.Cryptography.Xml;

/// <summary>Represents the exclusive C14N XML canonicalization transform for a digital signature as defined by the World Wide Web Consortium (W3C), with comments.</summary>
public class XmlDsigExcC14NWithCommentsTransform : XmlDsigExcC14NTransform
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NWithCommentsTransform" /> class. </summary>
	public XmlDsigExcC14NWithCommentsTransform()
		: base(includeComments: true)
	{
		base.Algorithm = "http://www.w3.org/2001/10/xml-exc-c14n#WithComments";
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NWithCommentsTransform" /> class specifying a list of namespace prefixes to canonicalize using the standard canonicalization algorithm.</summary>
	/// <param name="inclusiveNamespacesPrefixList">The namespace prefixes to canonicalize using the standard canonicalization algorithm.</param>
	public XmlDsigExcC14NWithCommentsTransform(string inclusiveNamespacesPrefixList)
		: base(includeComments: true, inclusiveNamespacesPrefixList)
	{
		base.Algorithm = "http://www.w3.org/2001/10/xml-exc-c14n#WithComments";
	}
}
