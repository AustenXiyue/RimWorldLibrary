namespace System.Xml.Schema;

/// <summary>Represents the post-schema-validation infoset of a validated XML node.</summary>
public class XmlSchemaInfo : IXmlSchemaInfo
{
	private bool isDefault;

	private bool isNil;

	private XmlSchemaElement schemaElement;

	private XmlSchemaAttribute schemaAttribute;

	private XmlSchemaType schemaType;

	private XmlSchemaSimpleType memberType;

	private XmlSchemaValidity validity;

	private XmlSchemaContentType contentType;

	/// <summary>Gets or sets the <see cref="T:System.Xml.Schema.XmlSchemaValidity" /> value of this validated XML node.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaValidity" /> value.</returns>
	public XmlSchemaValidity Validity
	{
		get
		{
			return validity;
		}
		set
		{
			validity = value;
		}
	}

	/// <summary>Gets or sets a value indicating if this validated XML node was set as the result of a default being applied during XML Schema Definition Language (XSD) schema validation.</summary>
	/// <returns>A bool value.</returns>
	public bool IsDefault
	{
		get
		{
			return isDefault;
		}
		set
		{
			isDefault = value;
		}
	}

	/// <summary>Gets or sets a value indicating if the value for this validated XML node is nil.</summary>
	/// <returns>A bool value.</returns>
	public bool IsNil
	{
		get
		{
			return isNil;
		}
		set
		{
			isNil = value;
		}
	}

	/// <summary>Gets or sets the dynamic schema type for this validated XML node.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaSimpleType" /> object.</returns>
	public XmlSchemaSimpleType MemberType
	{
		get
		{
			return memberType;
		}
		set
		{
			memberType = value;
		}
	}

	/// <summary>Gets or sets the static XML Schema Definition Language (XSD) schema type of this validated XML node.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaType" /> object.</returns>
	public XmlSchemaType SchemaType
	{
		get
		{
			return schemaType;
		}
		set
		{
			schemaType = value;
			if (schemaType != null)
			{
				contentType = schemaType.SchemaContentType;
			}
			else
			{
				contentType = XmlSchemaContentType.Empty;
			}
		}
	}

	/// <summary>Gets or sets the compiled <see cref="T:System.Xml.Schema.XmlSchemaElement" /> object that corresponds to this validated XML node.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaElement" /> object.</returns>
	public XmlSchemaElement SchemaElement
	{
		get
		{
			return schemaElement;
		}
		set
		{
			schemaElement = value;
			if (value != null)
			{
				schemaAttribute = null;
			}
		}
	}

	/// <summary>Gets or sets the compiled <see cref="T:System.Xml.Schema.XmlSchemaAttribute" /> object that corresponds to this validated XML node.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaAttribute" /> object.</returns>
	public XmlSchemaAttribute SchemaAttribute
	{
		get
		{
			return schemaAttribute;
		}
		set
		{
			schemaAttribute = value;
			if (value != null)
			{
				schemaElement = null;
			}
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Xml.Schema.XmlSchemaContentType" /> object that corresponds to the content type of this validated XML node.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaContentType" /> object.</returns>
	public XmlSchemaContentType ContentType
	{
		get
		{
			return contentType;
		}
		set
		{
			contentType = value;
		}
	}

	internal XmlSchemaType XmlType
	{
		get
		{
			if (memberType != null)
			{
				return memberType;
			}
			return schemaType;
		}
	}

	internal bool HasDefaultValue
	{
		get
		{
			if (schemaElement != null)
			{
				return schemaElement.ElementDecl.DefaultValueTyped != null;
			}
			return false;
		}
	}

	internal bool IsUnionType
	{
		get
		{
			if (schemaType == null || schemaType.Datatype == null)
			{
				return false;
			}
			return schemaType.Datatype.Variety == XmlSchemaDatatypeVariety.Union;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaInfo" /> class.</summary>
	public XmlSchemaInfo()
	{
		Clear();
	}

	internal XmlSchemaInfo(XmlSchemaValidity validity)
		: this()
	{
		this.validity = validity;
	}

	internal void Clear()
	{
		isNil = false;
		isDefault = false;
		schemaType = null;
		schemaElement = null;
		schemaAttribute = null;
		memberType = null;
		validity = XmlSchemaValidity.NotKnown;
		contentType = XmlSchemaContentType.Empty;
	}
}
