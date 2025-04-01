using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the attributeGroup element with the ref attribute from the XML Schema as specified by the World Wide Web Consortium (W3C). AttributesGroupRef is the reference for an attributeGroup, name property contains the attribute group being referenced. </summary>
public class XmlSchemaAttributeGroupRef : XmlSchemaAnnotated
{
	private XmlQualifiedName refName = XmlQualifiedName.Empty;

	/// <summary>Gets or sets the name of the referenced attributeGroup element.</summary>
	/// <returns>The name of the referenced attribute group. The value must be a QName.</returns>
	[XmlAttribute("ref")]
	public XmlQualifiedName RefName
	{
		get
		{
			return refName;
		}
		set
		{
			refName = ((value == null) ? XmlQualifiedName.Empty : value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaAttributeGroupRef" /> class.</summary>
	public XmlSchemaAttributeGroupRef()
	{
	}
}
