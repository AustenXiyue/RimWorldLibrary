namespace System.Xml.Schema;

/// <summary>Represents the minLength element from XML Schema as specified by the World Wide Web Consortium (W3C). This class can be used to specify a restriction on the minimum length of the data value of a simpleType element. The length must be greater than the value of the minLength element.</summary>
public class XmlSchemaMinLengthFacet : XmlSchemaNumericFacet
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaMinLengthFacet" /> class.</summary>
	public XmlSchemaMinLengthFacet()
	{
		base.FacetType = FacetType.MinLength;
	}
}
