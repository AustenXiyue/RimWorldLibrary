using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the attributeGroup element from the XML Schema as specified by the World Wide Web Consortium (W3C). AttributesGroups provides a mechanism to group a set of attribute declarations so that they can be incorporated as a group into complex type definitions.</summary>
public class XmlSchemaAttributeGroup : XmlSchemaAnnotated
{
	private string name;

	private XmlSchemaObjectCollection attributes = new XmlSchemaObjectCollection();

	private XmlSchemaAnyAttribute anyAttribute;

	private XmlQualifiedName qname = XmlQualifiedName.Empty;

	private XmlSchemaAttributeGroup redefined;

	private XmlSchemaObjectTable attributeUses;

	private XmlSchemaAnyAttribute attributeWildcard;

	private int selfReferenceCount;

	/// <summary>Gets or sets the name of the attribute group.</summary>
	/// <returns>The name of the attribute group.</returns>
	[XmlAttribute("name")]
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	/// <summary>Gets the collection of attributes for the attribute group. Contains XmlSchemaAttribute and XmlSchemaAttributeGroupRef elements.</summary>
	/// <returns>The collection of attributes for the attribute group.</returns>
	[XmlElement("attribute", typeof(XmlSchemaAttribute))]
	[XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroupRef))]
	public XmlSchemaObjectCollection Attributes => attributes;

	/// <summary>Gets or sets the <see cref="T:System.Xml.Schema.XmlSchemaAnyAttribute" /> component of the attribute group.</summary>
	/// <returns>The World Wide Web Consortium (W3C) anyAttribute element.</returns>
	[XmlElement("anyAttribute")]
	public XmlSchemaAnyAttribute AnyAttribute
	{
		get
		{
			return anyAttribute;
		}
		set
		{
			anyAttribute = value;
		}
	}

	/// <summary>Gets the qualified name of the attribute group.</summary>
	/// <returns>The qualified name of the attribute group.</returns>
	[XmlIgnore]
	public XmlQualifiedName QualifiedName => qname;

	[XmlIgnore]
	internal XmlSchemaObjectTable AttributeUses
	{
		get
		{
			if (attributeUses == null)
			{
				attributeUses = new XmlSchemaObjectTable();
			}
			return attributeUses;
		}
	}

	[XmlIgnore]
	internal XmlSchemaAnyAttribute AttributeWildcard
	{
		get
		{
			return attributeWildcard;
		}
		set
		{
			attributeWildcard = value;
		}
	}

	/// <summary>Gets the redefined attribute group property from the XML Schema.</summary>
	/// <returns>The redefined attribute group property.</returns>
	[XmlIgnore]
	public XmlSchemaAttributeGroup RedefinedAttributeGroup => redefined;

	[XmlIgnore]
	internal XmlSchemaAttributeGroup Redefined
	{
		get
		{
			return redefined;
		}
		set
		{
			redefined = value;
		}
	}

	[XmlIgnore]
	internal int SelfReferenceCount
	{
		get
		{
			return selfReferenceCount;
		}
		set
		{
			selfReferenceCount = value;
		}
	}

	[XmlIgnore]
	internal override string NameAttribute
	{
		get
		{
			return Name;
		}
		set
		{
			Name = value;
		}
	}

	internal void SetQualifiedName(XmlQualifiedName value)
	{
		qname = value;
	}

	internal override XmlSchemaObject Clone()
	{
		XmlSchemaAttributeGroup xmlSchemaAttributeGroup = (XmlSchemaAttributeGroup)MemberwiseClone();
		if (XmlSchemaComplexType.HasAttributeQNameRef(attributes))
		{
			xmlSchemaAttributeGroup.attributes = XmlSchemaComplexType.CloneAttributes(attributes);
			xmlSchemaAttributeGroup.attributeUses = null;
		}
		return xmlSchemaAttributeGroup;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaAttributeGroup" /> class.</summary>
	public XmlSchemaAttributeGroup()
	{
	}
}
