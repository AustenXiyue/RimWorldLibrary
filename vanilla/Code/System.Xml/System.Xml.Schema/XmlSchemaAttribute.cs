using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the attribute element from the XML Schema as specified by the World Wide Web Consortium (W3C). Attributes provide additional information for other document elements. The attribute tag is nested between the tags of a document's element for the schema. The XML document displays attributes as named items in the opening tag of an element.</summary>
public class XmlSchemaAttribute : XmlSchemaAnnotated
{
	private string defaultValue;

	private string fixedValue;

	private string name;

	private XmlSchemaForm form;

	private XmlSchemaUse use;

	private XmlQualifiedName refName = XmlQualifiedName.Empty;

	private XmlQualifiedName typeName = XmlQualifiedName.Empty;

	private XmlQualifiedName qualifiedName = XmlQualifiedName.Empty;

	private XmlSchemaSimpleType type;

	private XmlSchemaSimpleType attributeType;

	private SchemaAttDef attDef;

	/// <summary>Gets or sets the default value for the attribute.</summary>
	/// <returns>The default value for the attribute. The default is a null reference.Optional.</returns>
	[XmlAttribute("default")]
	[DefaultValue(null)]
	public string DefaultValue
	{
		get
		{
			return defaultValue;
		}
		set
		{
			defaultValue = value;
		}
	}

	/// <summary>Gets or sets the fixed value for the attribute.</summary>
	/// <returns>The fixed value for the attribute. The default is null.Optional.</returns>
	[XmlAttribute("fixed")]
	[DefaultValue(null)]
	public string FixedValue
	{
		get
		{
			return fixedValue;
		}
		set
		{
			fixedValue = value;
		}
	}

	/// <summary>Gets or sets the form for the attribute.</summary>
	/// <returns>One of the <see cref="T:System.Xml.Schema.XmlSchemaForm" /> values. The default is the value of the <see cref="P:System.Xml.Schema.XmlSchema.AttributeFormDefault" /> of the schema element containing the attribute.Optional.</returns>
	[XmlAttribute("form")]
	[DefaultValue(XmlSchemaForm.None)]
	public XmlSchemaForm Form
	{
		get
		{
			return form;
		}
		set
		{
			form = value;
		}
	}

	/// <summary>Gets or sets the name of the attribute.</summary>
	/// <returns>The name of the attribute.</returns>
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

	/// <summary>Gets or sets the name of an attribute declared in this schema (or another schema indicated by the specified namespace).</summary>
	/// <returns>The name of the attribute declared.</returns>
	[XmlAttribute("ref")]
	public XmlQualifiedName RefName
	{
		get
		{
			return refName;
		}
		set
		{
			refName = ((value == null) ? XmlQualifiedName.Empty : value);
		}
	}

	/// <summary>Gets or sets the name of the simple type defined in this schema (or another schema indicated by the specified namespace).</summary>
	/// <returns>The name of the simple type.</returns>
	[XmlAttribute("type")]
	public XmlQualifiedName SchemaTypeName
	{
		get
		{
			return typeName;
		}
		set
		{
			typeName = ((value == null) ? XmlQualifiedName.Empty : value);
		}
	}

	/// <summary>Gets or sets the attribute type to a simple type.</summary>
	/// <returns>The simple type defined in this schema.</returns>
	[XmlElement("simpleType")]
	public XmlSchemaSimpleType SchemaType
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}

	/// <summary>Gets or sets information about how the attribute is used.</summary>
	/// <returns>One of the following values: None, Prohibited, Optional, or Required. The default is Optional.Optional.</returns>
	[XmlAttribute("use")]
	[DefaultValue(XmlSchemaUse.None)]
	public XmlSchemaUse Use
	{
		get
		{
			return use;
		}
		set
		{
			use = value;
		}
	}

	/// <summary>Gets the qualified name for the attribute.</summary>
	/// <returns>The post-compilation value of the QualifiedName property.</returns>
	[XmlIgnore]
	public XmlQualifiedName QualifiedName => qualifiedName;

	/// <summary>Gets the common language runtime (CLR) object based on the <see cref="P:System.Xml.Schema.XmlSchemaAttribute.SchemaType" /> or <see cref="P:System.Xml.Schema.XmlSchemaAttribute.SchemaTypeName" /> of the attribute that holds the post-compilation value of the AttributeType property.</summary>
	/// <returns>The common runtime library (CLR) object that holds the post-compilation value of the AttributeType property.</returns>
	[Obsolete("This property has been deprecated. Please use AttributeSchemaType property that returns a strongly typed attribute type. http://go.microsoft.com/fwlink/?linkid=14202")]
	[XmlIgnore]
	public object AttributeType
	{
		get
		{
			if (attributeType == null)
			{
				return null;
			}
			if (attributeType.QualifiedName.Namespace == "http://www.w3.org/2001/XMLSchema")
			{
				return attributeType.Datatype;
			}
			return attributeType;
		}
	}

	/// <summary>Gets an <see cref="T:System.Xml.Schema.XmlSchemaSimpleType" /> object representing the type of the attribute based on the <see cref="P:System.Xml.Schema.XmlSchemaAttribute.SchemaType" /> or <see cref="P:System.Xml.Schema.XmlSchemaAttribute.SchemaTypeName" /> of the attribute.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaSimpleType" /> object.</returns>
	[XmlIgnore]
	public XmlSchemaSimpleType AttributeSchemaType => attributeType;

	[XmlIgnore]
	internal XmlSchemaDatatype Datatype
	{
		get
		{
			if (attributeType != null)
			{
				return attributeType.Datatype;
			}
			return null;
		}
	}

	internal SchemaAttDef AttDef
	{
		get
		{
			return attDef;
		}
		set
		{
			attDef = value;
		}
	}

	internal bool HasDefault => defaultValue != null;

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

	internal XmlReader Validate(XmlReader reader, XmlResolver resolver, XmlSchemaSet schemaSet, ValidationEventHandler valEventHandler)
	{
		if (schemaSet != null)
		{
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.ValidationType = ValidationType.Schema;
			xmlReaderSettings.Schemas = schemaSet;
			xmlReaderSettings.ValidationEventHandler += valEventHandler;
			return new XsdValidatingReader(reader, resolver, xmlReaderSettings, this);
		}
		return null;
	}

	internal void SetQualifiedName(XmlQualifiedName value)
	{
		qualifiedName = value;
	}

	internal void SetAttributeType(XmlSchemaSimpleType value)
	{
		attributeType = value;
	}

	internal override XmlSchemaObject Clone()
	{
		XmlSchemaAttribute obj = (XmlSchemaAttribute)MemberwiseClone();
		obj.refName = refName.Clone();
		obj.typeName = typeName.Clone();
		obj.qualifiedName = qualifiedName.Clone();
		return obj;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaAttribute" /> class.</summary>
	public XmlSchemaAttribute()
	{
	}
}
