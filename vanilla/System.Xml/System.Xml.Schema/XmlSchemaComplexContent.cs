using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the complexContent element from XML Schema as specified by the World Wide Web Consortium (W3C). This class represents the complex content model for complex types. It contains extensions or restrictions on a complex type that has either only elements or mixed content.</summary>
public class XmlSchemaComplexContent : XmlSchemaContentModel
{
	private XmlSchemaContent content;

	private bool isMixed;

	private bool hasMixedAttribute;

	/// <summary>Gets or sets information that determines if the type has a mixed content model.</summary>
	/// <returns>If this property is true, character data is allowed to appear between the child elements of the complex type (mixed content model). The default is false.Optional.</returns>
	[XmlAttribute("mixed")]
	public bool IsMixed
	{
		get
		{
			return isMixed;
		}
		set
		{
			isMixed = value;
			hasMixedAttribute = true;
		}
	}

	/// <summary>Gets or sets the content.</summary>
	/// <returns>One of either the <see cref="T:System.Xml.Schema.XmlSchemaComplexContentRestriction" /> or <see cref="T:System.Xml.Schema.XmlSchemaComplexContentExtension" /> classes.</returns>
	[XmlElement("extension", typeof(XmlSchemaComplexContentExtension))]
	[XmlElement("restriction", typeof(XmlSchemaComplexContentRestriction))]
	public override XmlSchemaContent Content
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

	[XmlIgnore]
	internal bool HasMixedAttribute => hasMixedAttribute;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaComplexContent" /> class.</summary>
	public XmlSchemaComplexContent()
	{
	}
}
