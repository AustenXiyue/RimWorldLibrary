using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>The base class for any element that can contain annotation elements.</summary>
public class XmlSchemaAnnotated : XmlSchemaObject
{
	private string id;

	private XmlSchemaAnnotation annotation;

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

	/// <summary>Gets or sets the annotation property.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaAnnotation" /> representing the annotation property.</returns>
	[XmlElement("annotation", typeof(XmlSchemaAnnotation))]
	public XmlSchemaAnnotation Annotation
	{
		get
		{
			return annotation;
		}
		set
		{
			annotation = value;
		}
	}

	/// <summary>Gets or sets the qualified attributes that do not belong to the current schema's target namespace.</summary>
	/// <returns>An array of qualified <see cref="T:System.Xml.XmlAttribute" /> objects that do not belong to the schema's target namespace.</returns>
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

	internal override void AddAnnotation(XmlSchemaAnnotation annotation)
	{
		this.annotation = annotation;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaAnnotated" /> class.</summary>
	public XmlSchemaAnnotated()
	{
	}
}
