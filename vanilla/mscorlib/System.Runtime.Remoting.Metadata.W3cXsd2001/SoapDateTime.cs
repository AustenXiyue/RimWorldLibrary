using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001;

/// <summary>Provides static methods for the serialization and deserialization of <see cref="T:System.DateTime" /> to a string that is formatted as XSD dateTime. </summary>
[ComVisible(true)]
public sealed class SoapDateTime
{
	private static readonly string[] _datetimeFormats = new string[24]
	{
		"yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.f", "yyyy-MM-ddTHH:mm:ss.ff", "yyyy-MM-ddTHH:mm:ss.fff", "yyyy-MM-ddTHH:mm:ss.ffff", "yyyy-MM-ddTHH:mm:ss.fffff", "yyyy-MM-ddTHH:mm:ss.ffffff", "yyyy-MM-ddTHH:mm:ss.fffffff", "yyyy-MM-ddTHH:mm:sszzz", "yyyy-MM-ddTHH:mm:ss.fzzz",
		"yyyy-MM-ddTHH:mm:ss.ffzzz", "yyyy-MM-ddTHH:mm:ss.fffzzz", "yyyy-MM-ddTHH:mm:ss.ffffzzz", "yyyy-MM-ddTHH:mm:ss.fffffzzz", "yyyy-MM-ddTHH:mm:ss.ffffffzzz", "yyyy-MM-ddTHH:mm:ss.fffffffzzz", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ss.fZ", "yyyy-MM-ddTHH:mm:ss.ffZ", "yyyy-MM-ddTHH:mm:ss.fffZ",
		"yyyy-MM-ddTHH:mm:ss.ffffZ", "yyyy-MM-ddTHH:mm:ss.fffffZ", "yyyy-MM-ddTHH:mm:ss.ffffffZ", "yyyy-MM-ddTHH:mm:ss.fffffffZ"
	};

	/// <summary>Gets the XML Schema definition language (XSD) of the current SOAP type.</summary>
	/// <returns>A <see cref="T:System.String" /> that indicates the XSD of the current SOAP type.</returns>
	public static string XsdType => "dateTime";

	/// <summary>Creates an instance of <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDateTime" />.</summary>
	public SoapDateTime()
	{
	}

	/// <summary>Converts the specified <see cref="T:System.String" /> into a <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> object obtained from <paramref name="value" />.</returns>
	/// <param name="value">The String to convert. </param>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">One of the following: <paramref name="value" /> is an empty string.<paramref name="value" /> is null reference.<paramref name="value" /> does not contain a date and time that corresponds to any of the recognized format patterns. </exception>
	public static DateTime Parse(string value)
	{
		return DateTime.ParseExact(value, _datetimeFormats, null, DateTimeStyles.None);
	}

	/// <summary>Returns the specified <see cref="T:System.DateTime" /> object as a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> representation of <paramref name="value" /> in the format "yyyy-MM-dd'T'HH:mm:ss.fffffffzzz".</returns>
	/// <param name="value">The <see cref="T:System.DateTime" /> object to convert. </param>
	public static string ToString(DateTime value)
	{
		return value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture);
	}
}
