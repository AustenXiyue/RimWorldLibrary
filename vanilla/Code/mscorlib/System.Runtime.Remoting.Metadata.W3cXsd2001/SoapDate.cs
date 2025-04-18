using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001;

/// <summary>Wraps an XSD date type.</summary>
[Serializable]
[ComVisible(true)]
public sealed class SoapDate : ISoapXsd
{
	private static readonly string[] _datetimeFormats = new string[6] { "yyyy-MM-dd", "'+'yyyy-MM-dd", "'-'yyyy-MM-dd", "yyyy-MM-ddzzz", "'+'yyyy-MM-ddzzz", "'-'yyyy-MM-ddzzz" };

	private int _sign;

	private DateTime _value;

	/// <summary>Gets or sets whether the date and time of the current instance is positive or negative.</summary>
	/// <returns>An integer that indicates whether <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDate.Value" /> is positive or negative.</returns>
	public int Sign
	{
		get
		{
			return _sign;
		}
		set
		{
			_sign = value;
		}
	}

	/// <summary>Gets or sets the date and time of the current instance.</summary>
	/// <returns>The <see cref="T:System.DateTime" /> object that contains the date and time of the current instance.</returns>
	public DateTime Value
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
	public static string XsdType => "date";

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDate" /> class.</summary>
	public SoapDate()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDate" /> class with a specified <see cref="T:System.DateTime" /> object.</summary>
	/// <param name="value">A <see cref="T:System.DateTime" /> object to initialize the current instance. </param>
	public SoapDate(DateTime value)
	{
		_value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDate" /> class with a specified <see cref="T:System.DateTime" /> object and an integer that indicates whether <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDate.Value" /> is a positive or negative value.</summary>
	/// <param name="value">A <see cref="T:System.DateTime" /> object to initialize the current instance. </param>
	/// <param name="sign">An integer that indicates whether <paramref name="value" /> is positive. </param>
	public SoapDate(DateTime value, int sign)
	{
		_value = value;
		_sign = sign;
	}

	/// <summary>Returns the XML Schema definition language (XSD) of the current SOAP type.</summary>
	/// <returns>A <see cref="T:System.String" /> that indicates the XSD of the current SOAP type.</returns>
	public string GetXsdType()
	{
		return XsdType;
	}

	/// <summary>Converts the specified <see cref="T:System.String" /> into a <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDate" /> object.</summary>
	/// <returns>A <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDate" /> object that is obtained from <paramref name="value" />.</returns>
	/// <param name="value">The <see cref="T:System.String" /> to convert. </param>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">
	///   <paramref name="value" /> does not contain a date and time that corresponds to any of the recognized format patterns. </exception>
	public static SoapDate Parse(string value)
	{
		SoapDate soapDate = new SoapDate(DateTime.ParseExact(value, _datetimeFormats, null, DateTimeStyles.None));
		if (value.StartsWith("-"))
		{
			soapDate.Sign = -1;
		}
		else
		{
			soapDate.Sign = 0;
		}
		return soapDate;
	}

	/// <summary>Returns <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDate.Value" /> as a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that is obtained from <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDate.Value" /> in the format "yyyy-MM-dd" or "'-'yyyy-MM-dd" if <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDate.Sign" /> is negative.</returns>
	public override string ToString()
	{
		if (_sign >= 0)
		{
			return _value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		}
		return _value.ToString("'-'yyyy-MM-dd", CultureInfo.InvariantCulture);
	}
}
