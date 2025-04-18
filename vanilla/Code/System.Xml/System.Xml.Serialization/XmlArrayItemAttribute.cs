using System.Xml.Schema;

namespace System.Xml.Serialization;

/// <summary>Represents an attribute that specifies the derived types that the <see cref="T:System.Xml.Serialization.XmlSerializer" /> can place in a serialized array.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true)]
public class XmlArrayItemAttribute : Attribute
{
	private string elementName;

	private Type type;

	private string ns;

	private string dataType;

	private bool nullable;

	private bool nullableSpecified;

	private XmlSchemaForm form;

	private int nestingLevel;

	/// <summary>Gets or sets the type allowed in an array.</summary>
	/// <returns>A <see cref="T:System.Type" /> that is allowed in the array.</returns>
	public Type Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}

	/// <summary>Gets or sets the name of the generated XML element.</summary>
	/// <returns>The name of the generated XML element. The default is the member identifier.</returns>
	public string ElementName
	{
		get
		{
			if (elementName != null)
			{
				return elementName;
			}
			return string.Empty;
		}
		set
		{
			elementName = value;
		}
	}

	/// <summary>Gets or sets the namespace of the generated XML element.</summary>
	/// <returns>The namespace of the generated XML element.</returns>
	public string Namespace
	{
		get
		{
			return ns;
		}
		set
		{
			ns = value;
		}
	}

	/// <summary>Gets or sets the level in a hierarchy of XML elements that the <see cref="T:System.Xml.Serialization.XmlArrayItemAttribute" /> affects.</summary>
	/// <returns>The zero-based index of a set of indexes in an array of arrays.</returns>
	public int NestingLevel
	{
		get
		{
			return nestingLevel;
		}
		set
		{
			nestingLevel = value;
		}
	}

	/// <summary>Gets or sets the XML data type of the generated XML element.</summary>
	/// <returns>An XML schema definition (XSD) data type, as defined by the World Wide Web Consortium (www.w3.org) document "XML Schema Part 2: DataTypes".</returns>
	public string DataType
	{
		get
		{
			if (dataType != null)
			{
				return dataType;
			}
			return string.Empty;
		}
		set
		{
			dataType = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Xml.Serialization.XmlSerializer" /> must serialize a member as an empty XML tag with the xsi:nil attribute set to true.</summary>
	/// <returns>true if the <see cref="T:System.Xml.Serialization.XmlSerializer" /> generates the xsi:nil attribute; otherwise, false, and no instance is generated. The default is true.</returns>
	public bool IsNullable
	{
		get
		{
			return nullable;
		}
		set
		{
			nullable = value;
			nullableSpecified = true;
		}
	}

	internal bool IsNullableSpecified => nullableSpecified;

	/// <summary>Gets or sets a value that indicates whether the name of the generated XML element is qualified.</summary>
	/// <returns>One of the <see cref="T:System.Xml.Schema.XmlSchemaForm" /> values. The default is XmlSchemaForm.None.</returns>
	/// <exception cref="T:System.Exception">The <see cref="P:System.Xml.Serialization.XmlArrayItemAttribute.Form" /> property is set to XmlSchemaForm.Unqualified and a <see cref="P:System.Xml.Serialization.XmlArrayItemAttribute.Namespace" /> value is specified. </exception>
	public XmlSchemaForm Form
	{
		get
		{
			return form;
		}
		set
		{
			form = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlArrayItemAttribute" /> class.</summary>
	public XmlArrayItemAttribute()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlArrayItemAttribute" /> class and specifies the name of the XML element generated in the XML document.</summary>
	/// <param name="elementName">The name of the XML element. </param>
	public XmlArrayItemAttribute(string elementName)
	{
		this.elementName = elementName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlArrayItemAttribute" /> class and specifies the <see cref="T:System.Type" /> that can be inserted into the serialized array.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> of the object to serialize. </param>
	public XmlArrayItemAttribute(Type type)
	{
		this.type = type;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlArrayItemAttribute" /> class and specifies the name of the XML element generated in the XML document and the <see cref="T:System.Type" /> that can be inserted into the generated XML document.</summary>
	/// <param name="elementName">The name of the XML element. </param>
	/// <param name="type">The <see cref="T:System.Type" /> of the object to serialize. </param>
	public XmlArrayItemAttribute(string elementName, Type type)
	{
		this.elementName = elementName;
		this.type = type;
	}
}
