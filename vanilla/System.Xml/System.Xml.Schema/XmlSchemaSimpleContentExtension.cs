using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the extension element for simple content from XML Schema as specified by the World Wide Web Consortium (W3C). This class can be used to derive simple types by extension. Such derivations are used to extend the simple type content of the element by adding attributes.</summary>
public class XmlSchemaSimpleContentExtension : XmlSchemaContent
{
	private XmlSchemaObjectCollection attributes = new XmlSchemaObjectCollection();

	private XmlSchemaAnyAttribute anyAttribute;

	private XmlQualifiedName baseTypeName = XmlQualifiedName.Empty;

	/// <summary>Gets or sets the name of a built-in data type or simple type from which this type is extended.</summary>
	/// <returns>The base type name.</returns>
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

	/// <summary>Gets the collection of <see cref="T:System.Xml.Schema.XmlSchemaAttribute" /> and <see cref="T:System.Xml.Schema.XmlSchemaAttributeGroupRef" />.</summary>
	/// <returns>The collection of attributes for the simpleType element.</returns>
	[XmlElement("attribute", typeof(XmlSchemaAttribute))]
	[XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroupRef))]
	public XmlSchemaObjectCollection Attributes => attributes;

	/// <summary>Gets or sets the XmlSchemaAnyAttribute to be used for the attribute value.</summary>
	/// <returns>The XmlSchemaAnyAttribute.Optional.</returns>
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

	internal void SetAttributes(XmlSchemaObjectCollection newAttributes)
	{
		attributes = newAttributes;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaSimpleContentExtension" /> class.</summary>
	public XmlSchemaSimpleContentExtension()
	{
	}
}
