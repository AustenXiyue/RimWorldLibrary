namespace System.Xml.Serialization;

/// <summary>Controls the XML schema that is generated when the attribute target is serialized by the <see cref="T:System.Xml.Serialization.XmlSerializer" />.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface)]
public class XmlTypeAttribute : Attribute
{
	private bool includeInSchema = true;

	private bool anonymousType;

	private string ns;

	private string typeName;

	/// <summary>Gets or sets a value that determines whether the resulting schema type is an XSD anonymous type.</summary>
	/// <returns>true, if the resulting schema type is an XSD anonymous type; otherwise, false.</returns>
	public bool AnonymousType
	{
		get
		{
			return anonymousType;
		}
		set
		{
			anonymousType = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to include the type in XML schema documents.</summary>
	/// <returns>true to include the type in XML schema documents; otherwise, false.</returns>
	public bool IncludeInSchema
	{
		get
		{
			return includeInSchema;
		}
		set
		{
			includeInSchema = value;
		}
	}

	/// <summary>Gets or sets the name of the XML type.</summary>
	/// <returns>The name of the XML type.</returns>
	public string TypeName
	{
		get
		{
			if (typeName != null)
			{
				return typeName;
			}
			return string.Empty;
		}
		set
		{
			typeName = value;
		}
	}

	/// <summary>Gets or sets the namespace of the XML type.</summary>
	/// <returns>The namespace of the XML type.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlTypeAttribute" /> class.</summary>
	public XmlTypeAttribute()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlTypeAttribute" /> class and specifies the name of the XML type.</summary>
	/// <param name="typeName">The name of the XML type that the <see cref="T:System.Xml.Serialization.XmlSerializer" /> generates when it serializes the class instance (and recognizes when it deserializes the class instance). </param>
	public XmlTypeAttribute(string typeName)
	{
		this.typeName = typeName;
	}
}
