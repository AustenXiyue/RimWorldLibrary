using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the union element for simple types from XML Schema as specified by the World Wide Web Consortium (W3C). A union datatype can be used to specify the content of a simpleType. The value of the simpleType element must be any one of a set of alternative datatypes specified in the union. Union types are always derived types and must comprise at least two alternative datatypes.</summary>
public class XmlSchemaSimpleTypeUnion : XmlSchemaSimpleTypeContent
{
	private XmlSchemaObjectCollection baseTypes = new XmlSchemaObjectCollection();

	private XmlQualifiedName[] memberTypes;

	private XmlSchemaSimpleType[] baseMemberTypes;

	/// <summary>Gets the collection of base types.</summary>
	/// <returns>The collection of simple type base values.</returns>
	[XmlElement("simpleType", typeof(XmlSchemaSimpleType))]
	public XmlSchemaObjectCollection BaseTypes => baseTypes;

	/// <summary>Gets or sets the array of qualified member names of built-in data types or simpleType elements defined in this schema (or another schema indicated by the specified namespace).</summary>
	/// <returns>An array that consists of a list of members of built-in data types or simple types.</returns>
	[XmlAttribute("memberTypes")]
	public XmlQualifiedName[] MemberTypes
	{
		get
		{
			return memberTypes;
		}
		set
		{
			memberTypes = value;
		}
	}

	/// <summary>Gets an array of <see cref="T:System.Xml.Schema.XmlSchemaSimpleType" /> objects representing the type of the simpleType element based on the <see cref="P:System.Xml.Schema.XmlSchemaSimpleTypeUnion.BaseTypes" /> and <see cref="P:System.Xml.Schema.XmlSchemaSimpleTypeUnion.MemberTypes" /> values of the simple type.</summary>
	/// <returns>An array of <see cref="T:System.Xml.Schema.XmlSchemaSimpleType" /> objects representing the type of the simpleType element.</returns>
	[XmlIgnore]
	public XmlSchemaSimpleType[] BaseMemberTypes => baseMemberTypes;

	internal void SetBaseMemberTypes(XmlSchemaSimpleType[] baseMemberTypes)
	{
		this.baseMemberTypes = baseMemberTypes;
	}

	internal override XmlSchemaObject Clone()
	{
		if (memberTypes != null && memberTypes.Length != 0)
		{
			XmlSchemaSimpleTypeUnion xmlSchemaSimpleTypeUnion = (XmlSchemaSimpleTypeUnion)MemberwiseClone();
			XmlQualifiedName[] array = new XmlQualifiedName[memberTypes.Length];
			for (int i = 0; i < memberTypes.Length; i++)
			{
				array[i] = memberTypes[i].Clone();
			}
			xmlSchemaSimpleTypeUnion.MemberTypes = array;
			return xmlSchemaSimpleTypeUnion;
		}
		return this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaSimpleTypeUnion" /> class.</summary>
	public XmlSchemaSimpleTypeUnion()
	{
	}
}
