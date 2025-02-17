using System.Xml.Serialization;

namespace System.Xml.Schema;

public class XmlSchemaAnnotated : XmlSchemaObject
{
	private string _id;

	private XmlSchemaAnnotation _annotation;

	private XmlAttribute[] _moreAttributes;

	[XmlAttribute("id", DataType = "ID")]
	public string? Id
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
		}
	}

	[XmlElement("annotation", typeof(XmlSchemaAnnotation))]
	public XmlSchemaAnnotation? Annotation
	{
		get
		{
			return _annotation;
		}
		set
		{
			_annotation = value;
		}
	}

	[XmlAnyAttribute]
	public XmlAttribute[]? UnhandledAttributes
	{
		get
		{
			return _moreAttributes;
		}
		set
		{
			_moreAttributes = value;
		}
	}

	[XmlIgnore]
	internal override string? IdAttribute
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
		_moreAttributes = moreAttributes;
	}

	internal override void AddAnnotation(XmlSchemaAnnotation annotation)
	{
		_annotation = annotation;
	}
}
