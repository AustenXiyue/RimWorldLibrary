using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the element element from XML Schema as specified by the World Wide Web Consortium (W3C). This class is the base class for all particle types and is used to describe an element in an XML document.</summary>
public class XmlSchemaElement : XmlSchemaParticle
{
	private bool isAbstract;

	private bool hasAbstractAttribute;

	private bool isNillable;

	private bool hasNillableAttribute;

	private bool isLocalTypeDerivationChecked;

	private XmlSchemaDerivationMethod block = XmlSchemaDerivationMethod.None;

	private XmlSchemaDerivationMethod final = XmlSchemaDerivationMethod.None;

	private XmlSchemaForm form;

	private string defaultValue;

	private string fixedValue;

	private string name;

	private XmlQualifiedName refName = XmlQualifiedName.Empty;

	private XmlQualifiedName substitutionGroup = XmlQualifiedName.Empty;

	private XmlQualifiedName typeName = XmlQualifiedName.Empty;

	private XmlSchemaType type;

	private XmlQualifiedName qualifiedName = XmlQualifiedName.Empty;

	private XmlSchemaType elementType;

	private XmlSchemaDerivationMethod blockResolved;

	private XmlSchemaDerivationMethod finalResolved;

	private XmlSchemaObjectCollection constraints;

	private SchemaElementDecl elementDecl;

	/// <summary>Gets or sets information to indicate if the element can be used in an instance document.</summary>
	/// <returns>If true, the element cannot appear in the instance document. The default is false.Optional.</returns>
	[XmlAttribute("abstract")]
	[DefaultValue(false)]
	public bool IsAbstract
	{
		get
		{
			return isAbstract;
		}
		set
		{
			isAbstract = value;
			hasAbstractAttribute = true;
		}
	}

	/// <summary>Gets or sets a Block derivation.</summary>
	/// <returns>The attribute used to block a type derivation. Default value is XmlSchemaDerivationMethod.None.Optional.</returns>
	[DefaultValue(XmlSchemaDerivationMethod.None)]
	[XmlAttribute("block")]
	public XmlSchemaDerivationMethod Block
	{
		get
		{
			return block;
		}
		set
		{
			block = value;
		}
	}

	/// <summary>Gets or sets the default value of the element if its content is a simple type or content of the element is textOnly.</summary>
	/// <returns>The default value for the element. The default is a null reference.Optional.</returns>
	[DefaultValue(null)]
	[XmlAttribute("default")]
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

	/// <summary>Gets or sets the Final property to indicate that no further derivations are allowed.</summary>
	/// <returns>The Final property. The default is XmlSchemaDerivationMethod.None.Optional.</returns>
	[XmlAttribute("final")]
	[DefaultValue(XmlSchemaDerivationMethod.None)]
	public XmlSchemaDerivationMethod Final
	{
		get
		{
			return final;
		}
		set
		{
			final = value;
		}
	}

	/// <summary>Gets or sets the fixed value.</summary>
	/// <returns>The fixed value that is predetermined and unchangeable. The default is a null reference.Optional.</returns>
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

	/// <summary>Gets or sets the form for the element.</summary>
	/// <returns>The form for the element. The default is the <see cref="P:System.Xml.Schema.XmlSchema.ElementFormDefault" /> value.Optional.</returns>
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

	/// <summary>Gets or sets the name of the element.</summary>
	/// <returns>The name of the element. The default is String.Empty.</returns>
	[XmlAttribute("name")]
	[DefaultValue("")]
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

	/// <summary>Gets or sets information that indicates if xsi:nil can occur in the instance data. Indicates if an explicit nil value can be assigned to the element.</summary>
	/// <returns>If nillable is true, this enables an instance of the element to have the nil attribute set to true. The nil attribute is defined as part of the XML Schema namespace for instances. The default is false.Optional.</returns>
	[DefaultValue(false)]
	[XmlAttribute("nillable")]
	public bool IsNillable
	{
		get
		{
			return isNillable;
		}
		set
		{
			isNillable = value;
			hasNillableAttribute = true;
		}
	}

	[XmlIgnore]
	internal bool HasNillableAttribute => hasNillableAttribute;

	[XmlIgnore]
	internal bool HasAbstractAttribute => hasAbstractAttribute;

	/// <summary>Gets or sets the reference name of an element declared in this schema (or another schema indicated by the specified namespace).</summary>
	/// <returns>The reference name of the element.</returns>
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

	/// <summary>Gets or sets the name of an element that is being substituted by this element.</summary>
	/// <returns>The qualified name of an element that is being substituted by this element.Optional.</returns>
	[XmlAttribute("substitutionGroup")]
	public XmlQualifiedName SubstitutionGroup
	{
		get
		{
			return substitutionGroup;
		}
		set
		{
			substitutionGroup = ((value == null) ? XmlQualifiedName.Empty : value);
		}
	}

	/// <summary>Gets or sets the name of a built-in data type defined in this schema or another schema indicated by the specified namespace.</summary>
	/// <returns>The name of the built-in data type.</returns>
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

	/// <summary>Gets or sets the type of the element. This can either be a complex type or a simple type.</summary>
	/// <returns>The type of the element.</returns>
	[XmlElement("complexType", typeof(XmlSchemaComplexType))]
	[XmlElement("simpleType", typeof(XmlSchemaSimpleType))]
	public XmlSchemaType SchemaType
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

	/// <summary>Gets the collection of constraints on the element.</summary>
	/// <returns>The collection of constraints.</returns>
	[XmlElement("key", typeof(XmlSchemaKey))]
	[XmlElement("keyref", typeof(XmlSchemaKeyref))]
	[XmlElement("unique", typeof(XmlSchemaUnique))]
	public XmlSchemaObjectCollection Constraints
	{
		get
		{
			if (constraints == null)
			{
				constraints = new XmlSchemaObjectCollection();
			}
			return constraints;
		}
	}

	/// <summary>Gets the actual qualified name for the given element. </summary>
	/// <returns>The qualified name of the element. The post-compilation value of the QualifiedName property.</returns>
	[XmlIgnore]
	public XmlQualifiedName QualifiedName => qualifiedName;

	/// <summary>Gets a common language runtime (CLR) object based on the <see cref="T:System.Xml.Schema.XmlSchemaElement" /> or <see cref="T:System.Xml.Schema.XmlSchemaElement" /> of the element, which holds the post-compilation value of the ElementType property.</summary>
	/// <returns>The common language runtime object. The post-compilation value of the ElementType property.</returns>
	[XmlIgnore]
	[Obsolete("This property has been deprecated. Please use ElementSchemaType property that returns a strongly typed element type. http://go.microsoft.com/fwlink/?linkid=14202")]
	public object ElementType
	{
		get
		{
			if (elementType == null)
			{
				return null;
			}
			if (elementType.QualifiedName.Namespace == "http://www.w3.org/2001/XMLSchema")
			{
				return elementType.Datatype;
			}
			return elementType;
		}
	}

	/// <summary>Gets an <see cref="T:System.Xml.Schema.XmlSchemaType" /> object representing the type of the element based on the <see cref="P:System.Xml.Schema.XmlSchemaElement.SchemaType" /> or <see cref="P:System.Xml.Schema.XmlSchemaElement.SchemaTypeName" /> values of the element.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaType" /> object.</returns>
	[XmlIgnore]
	public XmlSchemaType ElementSchemaType => elementType;

	/// <summary>Gets the post-compilation value of the Block property.</summary>
	/// <returns>The post-compilation value of the Block property. The default is the BlockDefault value on the schema element.</returns>
	[XmlIgnore]
	public XmlSchemaDerivationMethod BlockResolved => blockResolved;

	/// <summary>Gets the post-compilation value of the Final property.</summary>
	/// <returns>The post-compilation value of the Final property. Default value is the FinalDefault value on the schema element.</returns>
	[XmlIgnore]
	public XmlSchemaDerivationMethod FinalResolved => finalResolved;

	[XmlIgnore]
	internal bool HasDefault
	{
		get
		{
			if (defaultValue != null)
			{
				return defaultValue.Length > 0;
			}
			return false;
		}
	}

	internal bool HasConstraints
	{
		get
		{
			if (constraints != null)
			{
				return constraints.Count > 0;
			}
			return false;
		}
	}

	internal bool IsLocalTypeDerivationChecked
	{
		get
		{
			return isLocalTypeDerivationChecked;
		}
		set
		{
			isLocalTypeDerivationChecked = value;
		}
	}

	internal SchemaElementDecl ElementDecl
	{
		get
		{
			return elementDecl;
		}
		set
		{
			elementDecl = value;
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

	[XmlIgnore]
	internal override string NameString => qualifiedName.ToString();

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

	internal void SetElementType(XmlSchemaType value)
	{
		elementType = value;
	}

	internal void SetBlockResolved(XmlSchemaDerivationMethod value)
	{
		blockResolved = value;
	}

	internal void SetFinalResolved(XmlSchemaDerivationMethod value)
	{
		finalResolved = value;
	}

	internal override XmlSchemaObject Clone()
	{
		return Clone(null);
	}

	internal XmlSchemaObject Clone(XmlSchema parentSchema)
	{
		XmlSchemaElement xmlSchemaElement = (XmlSchemaElement)MemberwiseClone();
		xmlSchemaElement.refName = refName.Clone();
		xmlSchemaElement.substitutionGroup = substitutionGroup.Clone();
		xmlSchemaElement.typeName = typeName.Clone();
		xmlSchemaElement.qualifiedName = qualifiedName.Clone();
		if (type is XmlSchemaComplexType xmlSchemaComplexType && xmlSchemaComplexType.QualifiedName.IsEmpty)
		{
			xmlSchemaElement.type = (XmlSchemaType)xmlSchemaComplexType.Clone(parentSchema);
		}
		xmlSchemaElement.constraints = null;
		return xmlSchemaElement;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaElement" /> class.</summary>
	public XmlSchemaElement()
	{
	}
}
