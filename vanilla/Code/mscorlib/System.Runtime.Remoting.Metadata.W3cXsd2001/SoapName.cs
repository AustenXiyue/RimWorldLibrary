using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001;

/// <summary>Wraps an XML Name type.</summary>
[Serializable]
[ComVisible(true)]
public sealed class SoapName : ISoapXsd
{
	private string _value;

	/// <summary>Gets or sets an XML Name type.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains an XML Name type.</returns>
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
	public static string XsdType => "Name";

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapName" /> class.</summary>
	public SoapName()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapName" /> class with an XML Name type.</summary>
	/// <param name="value">A <see cref="T:System.String" /> that contains an XML Name type. </param>
	public SoapName(string value)
	{
		_value = SoapHelper.Normalize(value);
	}

	/// <summary>Returns the XML Schema definition language (XSD) of the current SOAP type.</summary>
	/// <returns>A <see cref="T:System.String" /> that indicates the XSD of the current SOAP type.</returns>
	public string GetXsdType()
	{
		return XsdType;
	}

	/// <summary>Converts the specified <see cref="T:System.String" /> into a <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapName" /> object.</summary>
	/// <returns>A <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapName" /> object that is obtained from <paramref name="value" />.</returns>
	/// <param name="value">The String to convert. </param>
	public static SoapName Parse(string value)
	{
		return new SoapName(value);
	}

	/// <summary>Returns <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapName.Value" /> as a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that is obtained from <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapName.Value" />.</returns>
	public override string ToString()
	{
		return _value;
	}
}
