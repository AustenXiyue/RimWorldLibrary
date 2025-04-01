namespace System.Xml.Schema;

/// <summary>Represents the length facet from XML Schema as specified by the World Wide Web Consortium (W3C). This class can be used to specify a restriction on the length of a simpleType element on the data type.</summary>
public class XmlSchemaLengthFacet : XmlSchemaNumericFacet
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaLengthFacet" /> class.</summary>
	public XmlSchemaLengthFacet()
	{
		base.FacetType = FacetType.Length;
	}
}
