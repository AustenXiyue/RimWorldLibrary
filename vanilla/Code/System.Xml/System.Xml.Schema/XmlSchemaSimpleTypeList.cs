using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the list element from XML Schema as specified by the World Wide Web Consortium (W3C). This class can be used to define a simpleType element as a list of values of a specified data type.</summary>
public class XmlSchemaSimpleTypeList : XmlSchemaSimpleTypeContent
{
	private XmlQualifiedName itemTypeName = XmlQualifiedName.Empty;

	private XmlSchemaSimpleType itemType;

	private XmlSchemaSimpleType baseItemType;

	/// <summary>Gets or sets the name of a built-in data type or simpleType element defined in this schema (or another schema indicated by the specified namespace).</summary>
	/// <returns>The type name of the simple type list.</returns>
	[XmlAttribute("itemType")]
	public XmlQualifiedName ItemTypeName
	{
		get
		{
			return itemTypeName;
		}
		set
		{
			itemTypeName = ((value == null) ? XmlQualifiedName.Empty : value);
		}
	}

	/// <summary>Gets or sets the simpleType element that is derived from the type specified by the base value.</summary>
	/// <returns>The item type for the simple type element.</returns>
	[XmlElement("simpleType", typeof(XmlSchemaSimpleType))]
	public XmlSchemaSimpleType ItemType
	{
		get
		{
			return itemType;
		}
		set
		{
			itemType = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Xml.Schema.XmlSchemaSimpleType" /> representing the type of the simpleType element based on the <see cref="P:System.Xml.Schema.XmlSchemaSimpleTypeList.ItemType" /> and <see cref="P:System.Xml.Schema.XmlSchemaSimpleTypeList.ItemTypeName" /> values of the simple type.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchemaSimpleType" /> representing the type of the simpleType element.</returns>
	[XmlIgnore]
	public XmlSchemaSimpleType BaseItemType
	{
		get
		{
			return baseItemType;
		}
		set
		{
			baseItemType = value;
		}
	}

	internal override XmlSchemaObject Clone()
	{
		XmlSchemaSimpleTypeList obj = (XmlSchemaSimpleTypeList)MemberwiseClone();
		obj.ItemTypeName = itemTypeName.Clone();
		return obj;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaSimpleTypeList" /> class.</summary>
	public XmlSchemaSimpleTypeList()
	{
	}
}
