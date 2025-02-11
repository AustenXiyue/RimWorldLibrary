using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the extension element from XML Schema as specified by the World Wide Web Consortium (W3C). This class is for complex types with complex content model derived by extension. It extends the complex type by adding attributes or elements.</summary>
public class XmlSchemaComplexContentExtension : XmlSchemaContent
{
	private XmlSchemaParticle particle;

	private XmlSchemaObjectCollection attributes = new XmlSchemaObjectCollection();

	private XmlSchemaAnyAttribute anyAttribute;

	private XmlQualifiedName baseTypeName = XmlQualifiedName.Empty;

	/// <summary>Gets or sets the name of the complex type from which this type is derived by extension.</summary>
	/// <returns>The name of the complex type from which this type is derived by extension.</returns>
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

	/// <summary>Gets or sets one of the <see cref="T:System.Xml.Schema.XmlSchemaGroupRef" />, <see cref="T:System.Xml.Schema.XmlSchemaChoice" />, <see cref="T:System.Xml.Schema.XmlSchemaAll" />, or <see cref="T:System.Xml.Schema.XmlSchemaSequence" /> classes.</summary>
	/// <returns>One of the <see cref="T:System.Xml.Schema.XmlSchemaGroupRef" />, <see cref="T:System.Xml.Schema.XmlSchemaChoice" />, <see cref="T:System.Xml.Schema.XmlSchemaAll" />, or <see cref="T:System.Xml.Schema.XmlSchemaSequence" /> classes.</returns>
	[XmlElement("sequence", typeof(XmlSchemaSequence))]
	[XmlElement("choice", typeof(XmlSchemaChoice))]
	[XmlElement("group", typeof(XmlSchemaGroupRef))]
	[XmlElement("all", typeof(XmlSchemaAll))]
	public XmlSchemaParticle Particle
	{
		get
		{
			return particle;
		}
		set
		{
			particle = value;
		}
	}

	/// <summary>Gets the collection of attributes for the complex content. Contains <see cref="T:System.Xml.Schema.XmlSchemaAttribute" /> and <see cref="T:System.Xml.Schema.XmlSchemaAttributeGroupRef" /> elements.</summary>
	/// <returns>The collection of attributes for the complex content.</returns>
	[XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroupRef))]
	[XmlElement("attribute", typeof(XmlSchemaAttribute))]
	public XmlSchemaObjectCollection Attributes => attributes;

	/// <summary>Gets or sets the <see cref="T:System.Xml.Schema.XmlSchemaAnyAttribute" /> component of the complex content model.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchemaAnyAttribute" /> component of the complex content model.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaComplexContentExtension" /> class.</summary>
	public XmlSchemaComplexContentExtension()
	{
	}
}
