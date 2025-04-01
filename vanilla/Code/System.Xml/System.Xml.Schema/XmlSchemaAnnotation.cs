using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the World Wide Web Consortium (W3C) annotation element.</summary>
public class XmlSchemaAnnotation : XmlSchemaObject
{
	private string id;

	private XmlSchemaObjectCollection items = new XmlSchemaObjectCollection();

	private XmlAttribute[] moreAttributes;

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

	/// <summary>Gets the Items collection that is used to store the appinfo and documentation child elements.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaObjectCollection" /> of appinfo and documentation child elements.</returns>
	[XmlElement("appinfo", typeof(XmlSchemaAppInfo))]
	[XmlElement("documentation", typeof(XmlSchemaDocumentation))]
	public XmlSchemaObjectCollection Items => items;

	/// <summary>Gets or sets the qualified attributes that do not belong to the schema's target namespace.</summary>
	/// <returns>An array of <see cref="T:System.Xml.XmlAttribute" /> objects that do not belong to the schema's target namespace.</returns>
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

	internal override void SetUnhandledAttributes(XmlAttribute[] moreAttributes)
	{
		this.moreAttributes = moreAttributes;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaAnnotation" /> class.</summary>
	public XmlSchemaAnnotation()
	{
	}
}
