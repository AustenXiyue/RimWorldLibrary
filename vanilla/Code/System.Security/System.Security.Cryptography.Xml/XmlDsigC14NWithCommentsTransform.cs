namespace System.Security.Cryptography.Xml;

/// <summary>Represents the C14N XML canonicalization transform for a digital signature as defined by the World Wide Web Consortium (W3C), with comments.</summary>
public class XmlDsigC14NWithCommentsTransform : XmlDsigC14NTransform
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.XmlDsigC14NWithCommentsTransform" /> class.</summary>
	public XmlDsigC14NWithCommentsTransform()
		: base(includeComments: true)
	{
		base.Algorithm = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments";
	}
}
