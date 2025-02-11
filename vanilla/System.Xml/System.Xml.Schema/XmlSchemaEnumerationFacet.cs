namespace System.Xml.Schema;

/// <summary>Represents the enumeration facet from XML Schema as specified by the World Wide Web Consortium (W3C). This class specifies a list of valid values for a simpleType element. Declaration is contained within a restriction declaration.</summary>
public class XmlSchemaEnumerationFacet : XmlSchemaFacet
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaEnumerationFacet" /> class.</summary>
	public XmlSchemaEnumerationFacet()
	{
		base.FacetType = FacetType.Enumeration;
	}
}
