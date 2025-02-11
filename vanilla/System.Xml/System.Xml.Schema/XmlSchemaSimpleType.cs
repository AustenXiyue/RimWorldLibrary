using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the simpleType element for simple content from XML Schema as specified by the World Wide Web Consortium (W3C). This class defines a simple type. Simple types can specify information and constraints for the value of attributes or elements with text-only content.</summary>
public class XmlSchemaSimpleType : XmlSchemaType
{
	private XmlSchemaSimpleTypeContent content;

	/// <summary>Gets or sets one of <see cref="T:System.Xml.Schema.XmlSchemaSimpleTypeUnion" />, <see cref="T:System.Xml.Schema.XmlSchemaSimpleTypeList" />, or <see cref="T:System.Xml.Schema.XmlSchemaSimpleTypeRestriction" />.</summary>
	/// <returns>One of XmlSchemaSimpleTypeUnion, XmlSchemaSimpleTypeList, or XmlSchemaSimpleTypeRestriction.</returns>
	[XmlElement("restriction", typeof(XmlSchemaSimpleTypeRestriction))]
	[XmlElement("union", typeof(XmlSchemaSimpleTypeUnion))]
	[XmlElement("list", typeof(XmlSchemaSimpleTypeList))]
	public XmlSchemaSimpleTypeContent Content
	{
		get
		{
			return content;
		}
		set
		{
			content = value;
		}
	}

	internal override XmlQualifiedName DerivedFrom
	{
		get
		{
			if (content == null)
			{
				return XmlQualifiedName.Empty;
			}
			if (content is XmlSchemaSimpleTypeRestriction)
			{
				return ((XmlSchemaSimpleTypeRestriction)content).BaseTypeName;
			}
			return XmlQualifiedName.Empty;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaSimpleType" /> class.</summary>
	public XmlSchemaSimpleType()
	{
	}

	internal override XmlSchemaObject Clone()
	{
		XmlSchemaSimpleType xmlSchemaSimpleType = (XmlSchemaSimpleType)MemberwiseClone();
		if (content != null)
		{
			xmlSchemaSimpleType.Content = (XmlSchemaSimpleTypeContent)content.Clone();
		}
		return xmlSchemaSimpleType;
	}
}
