namespace System.Xml.Serialization;

/// <summary>Specifies that the public member value be serialized by the <see cref="T:System.Xml.Serialization.XmlSerializer" /> as an encoded SOAP XML element.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public class SoapElementAttribute : Attribute
{
	private string elementName;

	private string dataType;

	private bool nullable;

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

	/// <summary>Gets or sets the XML Schema definition language (XSD) data type of the generated XML element.</summary>
	/// <returns>One of the XML Schema data types.</returns>
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

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Xml.Serialization.XmlSerializer" /> must serialize a member that has the xsi:null attribute set to "1".</summary>
	/// <returns>true if the <see cref="T:System.Xml.Serialization.XmlSerializer" /> generates the xsi:null attribute; otherwise, false.</returns>
	public bool IsNullable
	{
		get
		{
			return nullable;
		}
		set
		{
			nullable = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.SoapElementAttribute" /> class.</summary>
	public SoapElementAttribute()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.SoapElementAttribute" /> class and specifies the name of the XML element.</summary>
	/// <param name="elementName">The XML element name of the serialized member. </param>
	public SoapElementAttribute(string elementName)
	{
		this.elementName = elementName;
	}
}
