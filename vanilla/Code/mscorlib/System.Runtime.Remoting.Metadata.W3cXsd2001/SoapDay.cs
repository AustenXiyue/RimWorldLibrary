using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001;

/// <summary>Wraps an XSD gDay type. </summary>
[Serializable]
[ComVisible(true)]
public sealed class SoapDay : ISoapXsd
{
	private static readonly string[] _datetimeFormats = new string[2] { "---dd", "---ddzzz" };

	private DateTime _value;

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
	public static string XsdType => "gDay";

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDay" /> class.</summary>
	public SoapDay()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDay" /> class with a specified <see cref="T:System.DateTime" /> object.</summary>
	/// <param name="value">A <see cref="T:System.DateTime" /> object to initialize the current instance. </param>
	public SoapDay(DateTime value)
	{
		_value = value;
	}

	/// <summary>Returns the XML Schema definition language (XSD) of the current SOAP type.</summary>
	/// <returns>A <see cref="T:System.String" /> that indicates the XSD of the current SOAP type.</returns>
	public string GetXsdType()
	{
		return XsdType;
	}

	/// <summary>Converts the specified <see cref="T:System.String" /> into a <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDay" /> object.</summary>
	/// <returns>A <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDay" /> object that is obtained from <paramref name="value" />.</returns>
	/// <param name="value">The <see cref="T:System.String" /> to convert. </param>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">
	///   <paramref name="value" /> does not contain a date and time that corresponds to any of the recognized format patterns. </exception>
	public static SoapDay Parse(string value)
	{
		return new SoapDay(DateTime.ParseExact(value, _datetimeFormats, null, DateTimeStyles.None));
	}

	/// <summary>Returns <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDay.Value" /> as a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> obtained from <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDay.Value" /> in the format "---dd".</returns>
	public override string ToString()
	{
		return _value.ToString("---dd", CultureInfo.InvariantCulture);
	}
}
