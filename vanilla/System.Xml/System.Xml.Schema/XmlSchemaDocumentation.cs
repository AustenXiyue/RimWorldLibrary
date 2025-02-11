using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the documentation element from XML Schema as specified by the World Wide Web Consortium (W3C). This class specifies information to be read or used by humans within an annotation.</summary>
public class XmlSchemaDocumentation : XmlSchemaObject
{
	private string source;

	private string language;

	private XmlNode[] markup;

	private static XmlSchemaSimpleType languageType = DatatypeImplementation.GetSimpleTypeFromXsdType(new XmlQualifiedName("language", "http://www.w3.org/2001/XMLSchema"));

	/// <summary>Gets or sets the Uniform Resource Identifier (URI) source of the information.</summary>
	/// <returns>A URI reference. The default is String.Empty.Optional.</returns>
	[XmlAttribute("source", DataType = "anyURI")]
	public string Source
	{
		get
		{
			return source;
		}
		set
		{
			source = value;
		}
	}

	/// <summary>Gets or sets the xml:lang attribute. This serves as an indicator of the language used in the contents.</summary>
	/// <returns>The xml:lang attribute.Optional.</returns>
	[XmlAttribute("xml:lang")]
	public string Language
	{
		get
		{
			return language;
		}
		set
		{
			language = (string)languageType.Datatype.ParseValue(value, null, null);
		}
	}

	/// <summary>Gets or sets an array of XmlNodes that represents the documentation child nodes.</summary>
	/// <returns>The array that represents the documentation child nodes.</returns>
	[XmlText]
	[XmlAnyElement]
	public XmlNode[] Markup
	{
		get
		{
			return markup;
		}
		set
		{
			markup = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaDocumentation" /> class.</summary>
	public XmlSchemaDocumentation()
	{
	}
}
