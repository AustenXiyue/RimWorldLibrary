namespace System.Xml.Serialization;

/// <summary>Represents certain attributes of a XSD &lt;part&gt; element in a WSDL document for generating classes from the document. </summary>
public class SoapSchemaMember
{
	private string memberName;

	private XmlQualifiedName type = XmlQualifiedName.Empty;

	/// <summary>Gets or sets a value that corresponds to the type attribute of the WSDL part element.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlQualifiedName" /> that corresponds to the XML type.</returns>
	public XmlQualifiedName MemberType
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

	/// <summary>Gets or sets a value that corresponds to the name attribute of the WSDL part element. </summary>
	/// <returns>The element name.</returns>
	public string MemberName
	{
		get
		{
			if (memberName != null)
			{
				return memberName;
			}
			return string.Empty;
		}
		set
		{
			memberName = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.SoapSchemaMember" /> class. </summary>
	public SoapSchemaMember()
	{
	}
}
