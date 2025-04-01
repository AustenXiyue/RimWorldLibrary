namespace System.Xml.Schema;

/// <summary>Represents the minExclusive element from XML Schema as specified by the World Wide Web Consortium (W3C). This class can be used to specify a restriction on the minimum value of a simpleType element. The element value must be greater than the value of the minExclusive element.</summary>
public class XmlSchemaMinExclusiveFacet : XmlSchemaFacet
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaMinExclusiveFacet" /> class.</summary>
	public XmlSchemaMinExclusiveFacet()
	{
		base.FacetType = FacetType.MinExclusive;
	}
}
