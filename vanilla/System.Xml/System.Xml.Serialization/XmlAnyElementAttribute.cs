namespace System.Xml.Serialization;

/// <summary>Specifies that the member (a field that returns an array of <see cref="T:System.Xml.XmlElement" /> or <see cref="T:System.Xml.XmlNode" /> objects) contains objects that represent any XML element that has no corresponding member in the object being serialized or deserialized.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true)]
public class XmlAnyElementAttribute : Attribute
{
	private string name;

	private string ns;

	private int order = -1;

	private bool nsSpecified;

	/// <summary>Gets or sets the XML element name.</summary>
	/// <returns>The name of the XML element.</returns>
	/// <exception cref="T:System.InvalidOperationException">The element name of an array member does not match the element name specified by the <see cref="P:System.Xml.Serialization.XmlAnyElementAttribute.Name" /> property. </exception>
	public string Name
	{
		get
		{
			if (name != null)
			{
				return name;
			}
			return string.Empty;
		}
		set
		{
			name = value;
		}
	}

	/// <summary>Gets or sets the XML namespace generated in the XML document.</summary>
	/// <returns>An XML namespace.</returns>
	public string Namespace
	{
		get
		{
			return ns;
		}
		set
		{
			ns = value;
			nsSpecified = true;
		}
	}

	/// <summary>Gets or sets the explicit order in which the elements are serialized or deserialized.</summary>
	/// <returns>The order of the code generation.</returns>
	public int Order
	{
		get
		{
			return order;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException(Res.GetString("Negative values are prohibited."), "Order");
			}
			order = value;
		}
	}

	internal bool NamespaceSpecified => nsSpecified;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlAnyElementAttribute" /> class.</summary>
	public XmlAnyElementAttribute()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlAnyElementAttribute" /> class and specifies the XML element name generated in the XML document.</summary>
	/// <param name="name">The name of the XML element that the <see cref="T:System.Xml.Serialization.XmlSerializer" /> generates. </param>
	public XmlAnyElementAttribute(string name)
	{
		this.name = name;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlAnyElementAttribute" /> class and specifies the XML element name generated in the XML document and its XML namespace.</summary>
	/// <param name="name">The name of the XML element that the <see cref="T:System.Xml.Serialization.XmlSerializer" /> generates. </param>
	/// <param name="ns">The XML namespace of the XML element. </param>
	public XmlAnyElementAttribute(string name, string ns)
	{
		this.name = name;
		this.ns = ns;
		nsSpecified = true;
	}
}
