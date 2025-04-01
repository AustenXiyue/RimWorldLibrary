namespace System.Xml.Schema;

/// <summary>Represents the maxExclusive element from XML Schema as specified by the World Wide Web Consortium (W3C). This class can be used to specify a restriction on the maximum value of a simpleType element. The element value must be less than the value of the maxExclusive element.</summary>
public class XmlSchemaMaxExclusiveFacet : XmlSchemaFacet
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaMaxExclusiveFacet" /> class.</summary>
	public XmlSchemaMaxExclusiveFacet()
	{
		base.FacetType = FacetType.MaxExclusive;
	}
}
