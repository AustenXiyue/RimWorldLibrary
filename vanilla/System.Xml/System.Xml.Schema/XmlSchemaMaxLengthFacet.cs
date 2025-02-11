namespace System.Xml.Schema;

/// <summary>Represents the maxLength element from XML Schema as specified by the World Wide Web Consortium (W3C). This class can be used to specify a restriction on the maximum length of the data value of a simpleType element. The length must be less than the value of the maxLength element.</summary>
public class XmlSchemaMaxLengthFacet : XmlSchemaNumericFacet
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaMaxLengthFacet" /> class.</summary>
	public XmlSchemaMaxLengthFacet()
	{
		base.FacetType = FacetType.MaxLength;
	}
}
