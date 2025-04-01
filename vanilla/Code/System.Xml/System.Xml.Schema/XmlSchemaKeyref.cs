using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>This class represents the keyref element from XMLSchema as specified by the World Wide Web Consortium (W3C).</summary>
public class XmlSchemaKeyref : XmlSchemaIdentityConstraint
{
	private XmlQualifiedName refer = XmlQualifiedName.Empty;

	/// <summary>Gets or sets the name of the key that this constraint refers to in another simple or complex type.</summary>
	/// <returns>The QName of the key that this constraint refers to.</returns>
	[XmlAttribute("refer")]
	public XmlQualifiedName Refer
	{
		get
		{
			return refer;
		}
		set
		{
			refer = ((value == null) ? XmlQualifiedName.Empty : value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaKeyref" /> class.</summary>
	public XmlSchemaKeyref()
	{
	}
}
