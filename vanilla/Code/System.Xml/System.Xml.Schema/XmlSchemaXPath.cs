using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the World Wide Web Consortium (W3C) selector element.</summary>
public class XmlSchemaXPath : XmlSchemaAnnotated
{
	private string xpath;

	/// <summary>Gets or sets the attribute for the XPath expression.</summary>
	/// <returns>The string attribute value for the XPath expression.</returns>
	[DefaultValue("")]
	[XmlAttribute("xpath")]
	public string XPath
	{
		get
		{
			return xpath;
		}
		set
		{
			xpath = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaXPath" /> class.</summary>
	public XmlSchemaXPath()
	{
	}
}
