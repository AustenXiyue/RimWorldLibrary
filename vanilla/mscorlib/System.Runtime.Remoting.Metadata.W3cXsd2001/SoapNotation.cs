using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001;

/// <summary>Wraps an XML NOTATION attribute type.</summary>
[Serializable]
[ComVisible(true)]
public sealed class SoapNotation : ISoapXsd
{
	private string _value;

	/// <summary>Gets or sets an XML NOTATION attribute.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains an XML NOTATION attribute.</returns>
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
	public static string XsdType => "NOTATION";

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNotation" /> class.</summary>
	public SoapNotation()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNotation" /> class with an XML NOTATION attribute.</summary>
	/// <param name="value">A <see cref="T:System.String" /> that contains an XML NOTATION attribute. </param>
	public SoapNotation(string value)
	{
		_value = value;
	}

	/// <summary>Returns the XML Schema definition language (XSD) of the current SOAP type.</summary>
	/// <returns>A <see cref="T:System.String" /> that indicates the XSD of the current SOAP type.</returns>
	public string GetXsdType()
	{
		return XsdType;
	}

	/// <summary>Converts the specified <see cref="T:System.String" /> into a <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNotation" /> object.</summary>
	/// <returns>A <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNotation" /> object that is obtained from <paramref name="value" />.</returns>
	/// <param name="value">The String to convert. </param>
	public static SoapNotation Parse(string value)
	{
		return new SoapNotation(value);
	}

	/// <summary>Returns <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNotation.Value" /> as a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that is obtained from <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNotation.Value" />.</returns>
	public override string ToString()
	{
		return _value;
	}
}
