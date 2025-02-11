using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001;

/// <summary>Wraps an XSD anyURI type.</summary>
[Serializable]
[ComVisible(true)]
public sealed class SoapAnyUri : ISoapXsd
{
	private string _value;

	/// <summary>Gets or sets a URI.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains a URI.</returns>
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
	public static string XsdType => "anyUri";

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapAnyUri" /> class.</summary>
	public SoapAnyUri()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapAnyUri" /> class with the specified URI.</summary>
	/// <param name="value">A <see cref="T:System.String" /> that contains a URI. </param>
	public SoapAnyUri(string value)
	{
		_value = value;
	}

	/// <summary>Returns the XML Schema definition language (XSD) of the current SOAP type.</summary>
	/// <returns>A <see cref="T:System.String" /> that indicates the XSD of the current SOAP type.</returns>
	public string GetXsdType()
	{
		return XsdType;
	}

	/// <summary>Converts the specified <see cref="T:System.String" /> into a <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapAnyUri" /> object.</summary>
	/// <returns>A <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapAnyUri" /> object that is obtained from <paramref name="value" />.</returns>
	/// <param name="value">The <see cref="T:System.String" /> to convert. </param>
	public static SoapAnyUri Parse(string value)
	{
		return new SoapAnyUri(value);
	}

	/// <summary>Returns <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapAnyUri.Value" /> as a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that is obtained from <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapAnyUri.Value" />.</returns>
	public override string ToString()
	{
		return _value;
	}
}
