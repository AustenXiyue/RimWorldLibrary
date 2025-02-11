using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001;

/// <summary>Wraps an XSD nonNegativeInteger type.</summary>
[Serializable]
[ComVisible(true)]
public sealed class SoapNonNegativeInteger : ISoapXsd
{
	private decimal _value;

	/// <summary>Gets or sets the numeric value of the current instance.</summary>
	/// <returns>A <see cref="T:System.Decimal" /> that indicates the numeric value of the current instance.</returns>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">
	///   <paramref name="value" /> is less than 0. </exception>
	public decimal Value
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
	public static string XsdType => "nonNegativeInteger";

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNonNegativeInteger" /> class.</summary>
	public SoapNonNegativeInteger()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNonNegativeInteger" /> class with a <see cref="T:System.Decimal" /> value.</summary>
	/// <param name="value">A <see cref="T:System.Decimal" /> value to initialize the current instance. </param>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">
	///   <paramref name="value" /> is less than 0. </exception>
	public SoapNonNegativeInteger(decimal value)
	{
		if (value < 0m)
		{
			throw SoapHelper.GetException(this, "invalid " + value);
		}
		_value = value;
	}

	/// <summary>Returns the XML Schema definition language (XSD) of the current SOAP type.</summary>
	/// <returns>A <see cref="T:System.String" /> that indicates the XSD of the current SOAP type.</returns>
	public string GetXsdType()
	{
		return XsdType;
	}

	/// <summary>Converts the specified <see cref="T:System.String" /> into a <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNonNegativeInteger" /> object.</summary>
	/// <returns>A <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNonNegativeInteger" /> object that is obtained from <paramref name="value" />. </returns>
	/// <param name="value">The <see cref="T:System.String" />  to convert. </param>
	public static SoapNonNegativeInteger Parse(string value)
	{
		return new SoapNonNegativeInteger(decimal.Parse(value));
	}

	/// <summary>Returns <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNonNegativeInteger.Value" /> as a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that is obtained from <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNonNegativeInteger.Value" />.</returns>
	public override string ToString()
	{
		return _value.ToString();
	}
}
