namespace System.Xml.Schema;

/// <summary>Represents the totalDigits facet from XML Schema as specified by the World Wide Web Consortium (W3C). This class can be used to specify a restriction on the number of digits that can be entered for the value of a simpleType element. That value of totalDigits must be a positive integer.</summary>
public class XmlSchemaTotalDigitsFacet : XmlSchemaNumericFacet
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaTotalDigitsFacet" /> class.</summary>
	public XmlSchemaTotalDigitsFacet()
	{
		base.FacetType = FacetType.TotalDigits;
	}
}
