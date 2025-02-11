using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001;

/// <summary>Wraps an XML ID attribute.</summary>
[Serializable]
[ComVisible(true)]
public sealed class SoapId : ISoapXsd
{
	private string _value;

	/// <summary>Gets or sets an XML ID attribute.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains an XML ID attribute.</returns>
	public string Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	/// <summary>Gets the XML Schema definition language (XSD) of the current SOAP type.</summary>
	/// <returns>A <see cref="T:System.String" /> that indicates the XSD of the current SOAP type.</returns>
	public static string XsdType => "ID";

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapId" /> class.</summary>
	public SoapId()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapId" /> class with an XML ID attribute.</summary>
	/// <param name="value">A <see cref="T:System.String" /> that contains an XML ID attribute. </param>
	public SoapId(string value)
	{
		_value = SoapHelper.Normalize(value);
	}

	/// <summary>Returns the XML Schema definition language (XSD) of the current SOAP type.</summary>
	/// <returns>A <see cref="T:System.String" /> that indicates the XSD of the current SOAP type.</returns>
	public string GetXsdType()
	{
		return XsdType;
	}

	/// <summary>Converts the specified <see cref="T:System.String" /> into a <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapId" /> object.</summary>
	/// <returns>A <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapId" /> object that is obtained from <paramref name="value" />.</returns>
	/// <param name="value">The String to convert. </param>
	public static SoapId Parse(string value)
	{
		return new SoapId(value);
	}

	/// <summary>Returns <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapId.Value" /> as a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that is obtained from <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapId.Value" />.</returns>
	public override string ToString()
	{
		return _value;
	}
}
