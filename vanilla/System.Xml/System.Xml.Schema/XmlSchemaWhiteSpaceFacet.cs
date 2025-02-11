namespace System.Xml.Schema;

/// <summary>Represents the World Wide Web Consortium (W3C) whiteSpace facet.</summary>
public class XmlSchemaWhiteSpaceFacet : XmlSchemaFacet
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaWhiteSpaceFacet" /> class.</summary>
	public XmlSchemaWhiteSpaceFacet()
	{
		base.FacetType = FacetType.Whitespace;
	}
}
