namespace System.Xml.Schema;

/// <summary>Represents the minInclusive element from XML Schema as specified by the World Wide Web Consortium (W3C). This class can be used to specify a restriction on the minimum value of a simpleType element. The element value must be greater than or equal to the value of the minInclusive element.</summary>
public class XmlSchemaMinInclusiveFacet : XmlSchemaFacet
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaMinInclusiveFacet" /> class.</summary>
	public XmlSchemaMinInclusiveFacet()
	{
		base.FacetType = FacetType.MinInclusive;
	}
}
