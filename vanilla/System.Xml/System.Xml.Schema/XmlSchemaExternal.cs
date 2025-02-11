using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>An abstract class. Provides information about the included schema.</summary>
public abstract class XmlSchemaExternal : XmlSchemaObject
{
	private string location;

	private Uri baseUri;

	private XmlSchema schema;

	private string id;

	private XmlAttribute[] moreAttributes;

	private Compositor compositor;

	/// <summary>Gets or sets the Uniform Resource Identifier (URI) location for the schema, which tells the schema processor where the schema physically resides.</summary>
	/// <returns>The URI location for the schema.Optional for imported schemas.</returns>
	[XmlAttribute("schemaLocation", DataType = "anyURI")]
	public string SchemaLocation
	{
		get
		{
			return location;
		}
		set
		{
			location = value;
		}
	}

	/// <summary>Gets or sets the XmlSchema for the referenced schema.</summary>
	/// <returns>The XmlSchema for the referenced schema.</returns>
	[XmlIgnore]
	public XmlSchema Schema
	{
		get
		{
			return schema;
		}
		set
		{
			schema = value;
		}
	}

	/// <summary>Gets or sets the string id.</summary>
	/// <returns>The string id. The default is String.Empty.Optional.</returns>
	[XmlAttribute("id", DataType = "ID")]
	public string Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	/// <summary>Gets and sets the qualified attributes, which do not belong to the schema target namespace.</summary>
	/// <returns>Qualified attributes that belong to another target namespace.</returns>
	[XmlAnyAttribute]
	public XmlAttribute[] UnhandledAttributes
	{
		get
		{
			return moreAttributes;
		}
		set
		{
			moreAttributes = value;
		}
	}

	[XmlIgnore]
	internal Uri BaseUri
	{
		get
		{
			return baseUri;
		}
		set
		{
			baseUri = value;
		}
	}

	[XmlIgnore]
	internal override string IdAttribute
	{
		get
		{
			return Id;
		}
		set
		{
			Id = value;
		}
	}

	internal Compositor Compositor
	{
		get
		{
			return compositor;
		}
		set
		{
			compositor = value;
		}
	}

	internal override void SetUnhandledAttributes(XmlAttribute[] moreAttributes)
	{
		this.moreAttributes = moreAttributes;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaExternal" /> class.</summary>
	protected XmlSchemaExternal()
	{
	}
}
