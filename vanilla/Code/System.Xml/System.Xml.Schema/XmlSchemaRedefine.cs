using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the redefine element from XML Schema as specified by the World Wide Web Consortium (W3C). This class can be used to allow simple and complex types, groups and attribute groups from external schema files to be redefined in the current schema. This class can also be used to provide versioning for the schema elements.</summary>
public class XmlSchemaRedefine : XmlSchemaExternal
{
	private XmlSchemaObjectCollection items = new XmlSchemaObjectCollection();

	private XmlSchemaObjectTable attributeGroups = new XmlSchemaObjectTable();

	private XmlSchemaObjectTable types = new XmlSchemaObjectTable();

	private XmlSchemaObjectTable groups = new XmlSchemaObjectTable();

	/// <summary>Gets the collection of the following classes: <see cref="T:System.Xml.Schema.XmlSchemaAnnotation" />, <see cref="T:System.Xml.Schema.XmlSchemaAttributeGroup" />, <see cref="T:System.Xml.Schema.XmlSchemaComplexType" />, <see cref="T:System.Xml.Schema.XmlSchemaSimpleType" />, and <see cref="T:System.Xml.Schema.XmlSchemaGroup" />.</summary>
	/// <returns>The elements contained within the redefine element.</returns>
	[XmlElement("group", typeof(XmlSchemaGroup))]
	[XmlElement("complexType", typeof(XmlSchemaComplexType))]
	[XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroup))]
	[XmlElement("annotation", typeof(XmlSchemaAnnotation))]
	[XmlElement("simpleType", typeof(XmlSchemaSimpleType))]
	public XmlSchemaObjectCollection Items => items;

	/// <summary>Gets the <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" /> , for all attributes in the schema, which holds the post-compilation value of the AttributeGroups property.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" /> for all attributes in the schema. The post-compilation value of the AttributeGroups property.</returns>
	[XmlIgnore]
	public XmlSchemaObjectTable AttributeGroups => attributeGroups;

	/// <summary>Gets the <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" />, for all simple and complex types in the schema, which holds the post-compilation value of the SchemaTypes property.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" /> for all schema types in the schema. The post-compilation value of the SchemaTypes property.</returns>
	[XmlIgnore]
	public XmlSchemaObjectTable SchemaTypes => types;

	/// <summary>Gets the <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" />, for all groups in the schema, which holds the post-compilation value of the Groups property.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" /> for all groups in the schema. The post-compilation value of the Groups property.</returns>
	[XmlIgnore]
	public XmlSchemaObjectTable Groups => groups;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaRedefine" /> class.</summary>
	public XmlSchemaRedefine()
	{
		base.Compositor = Compositor.Redefine;
	}

	internal override void AddAnnotation(XmlSchemaAnnotation annotation)
	{
		items.Add(annotation);
	}
}
