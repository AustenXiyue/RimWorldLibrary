using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the restriction element for simple types from XML Schema as specified by the World Wide Web Consortium (W3C). This class can be used restricting simpleType element.</summary>
public class XmlSchemaSimpleTypeRestriction : XmlSchemaSimpleTypeContent
{
	private XmlQualifiedName baseTypeName = XmlQualifiedName.Empty;

	private XmlSchemaSimpleType baseType;

	private XmlSchemaObjectCollection facets = new XmlSchemaObjectCollection();

	/// <summary>Gets or sets the name of the qualified base type.</summary>
	/// <returns>The qualified name of the simple type restriction base type.</returns>
	[XmlAttribute("base")]
	public XmlQualifiedName BaseTypeName
	{
		get
		{
			return baseTypeName;
		}
		set
		{
			baseTypeName = ((value == null) ? XmlQualifiedName.Empty : value);
		}
	}

	/// <summary>Gets or sets information on the base type.</summary>
	/// <returns>The base type for the simpleType element.</returns>
	[XmlElement("simpleType", typeof(XmlSchemaSimpleType))]
	public XmlSchemaSimpleType BaseType
	{
		get
		{
			return baseType;
		}
		set
		{
			baseType = value;
		}
	}

	/// <summary>Gets or sets an Xml Schema facet. </summary>
	/// <returns>One of the following facet classes:<see cref="T:System.Xml.Schema.XmlSchemaLengthFacet" />, <see cref="T:System.Xml.Schema.XmlSchemaMinLengthFacet" />, <see cref="T:System.Xml.Schema.XmlSchemaMaxLengthFacet" />, <see cref="T:System.Xml.Schema.XmlSchemaPatternFacet" />, <see cref="T:System.Xml.Schema.XmlSchemaEnumerationFacet" />, <see cref="T:System.Xml.Schema.XmlSchemaMaxInclusiveFacet" />, <see cref="T:System.Xml.Schema.XmlSchemaMaxExclusiveFacet" />, <see cref="T:System.Xml.Schema.XmlSchemaMinInclusiveFacet" />, <see cref="T:System.Xml.Schema.XmlSchemaMinExclusiveFacet" />, <see cref="T:System.Xml.Schema.XmlSchemaFractionDigitsFacet" />, <see cref="T:System.Xml.Schema.XmlSchemaTotalDigitsFacet" />, <see cref="T:System.Xml.Schema.XmlSchemaWhiteSpaceFacet" />.</returns>
	[XmlElement("minLength", typeof(XmlSchemaMinLengthFacet))]
	[XmlElement("whiteSpace", typeof(XmlSchemaWhiteSpaceFacet))]
	[XmlElement("fractionDigits", typeof(XmlSchemaFractionDigitsFacet))]
	[XmlElement("length", typeof(XmlSchemaLengthFacet))]
	[XmlElement("totalDigits", typeof(XmlSchemaTotalDigitsFacet))]
	[XmlElement("minInclusive", typeof(XmlSchemaMinInclusiveFacet))]
	[XmlElement("enumeration", typeof(XmlSchemaEnumerationFacet))]
	[XmlElement("maxInclusive", typeof(XmlSchemaMaxInclusiveFacet))]
	[XmlElement("maxExclusive", typeof(XmlSchemaMaxExclusiveFacet))]
	[XmlElement("maxLength", typeof(XmlSchemaMaxLengthFacet))]
	[XmlElement("minExclusive", typeof(XmlSchemaMinExclusiveFacet))]
	[XmlElement("pattern", typeof(XmlSchemaPatternFacet))]
	public XmlSchemaObjectCollection Facets => facets;

	internal override XmlSchemaObject Clone()
	{
		XmlSchemaSimpleTypeRestriction obj = (XmlSchemaSimpleTypeRestriction)MemberwiseClone();
		obj.BaseTypeName = baseTypeName.Clone();
		return obj;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaSimpleTypeRestriction" /> class.</summary>
	public XmlSchemaSimpleTypeRestriction()
	{
	}
}
