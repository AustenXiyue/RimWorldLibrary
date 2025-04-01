namespace System.Xml.Schema;

/// <summary>Specifies a restriction on the number of digits that can be entered for the fraction value of a simpleType element. The value of fractionDigits must be a positive integer. Represents the World Wide Web Consortium (W3C) fractionDigits facet.</summary>
public class XmlSchemaFractionDigitsFacet : XmlSchemaNumericFacet
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaFractionDigitsFacet" /> class.</summary>
	public XmlSchemaFractionDigitsFacet()
	{
		base.FacetType = FacetType.FractionDigits;
	}
}
