using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001;

/// <summary>Wraps an XSD hexBinary type.</summary>
[Serializable]
[ComVisible(true)]
public sealed class SoapHexBinary : ISoapXsd
{
	private byte[] _value;

	private StringBuilder sb = new StringBuilder();

	/// <summary>Gets or sets the hexadecimal representation of a number.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the hexadecimal representation of a number.</returns>
	public byte[] Value
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
	/// <returns>A <see cref="T:System.String" /> indicating the XSD of the current SOAP type.</returns>
	public static string XsdType => "hexBinary";

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary" /> class.</summary>
	public SoapHexBinary()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary" /> class.</summary>
	/// <param name="value">A <see cref="T:System.Byte" /> array that contains a hexadecimal number. </param>
	public SoapHexBinary(byte[] value)
	{
		_value = value;
	}

	/// <summary>Returns the XML Schema definition language (XSD) of the current SOAP type.</summary>
	/// <returns>A <see cref="T:System.String" /> that indicates the XSD of the current SOAP type.</returns>
	public string GetXsdType()
	{
		return XsdType;
	}

	/// <summary>Converts the specified <see cref="T:System.String" /> into a <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary" /> object.</summary>
	/// <returns>A <see cref="T:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary" /> object that is obtained from <paramref name="value" />.</returns>
	/// <param name="value">The String to convert. </param>
	public static SoapHexBinary Parse(string value)
	{
		return new SoapHexBinary(FromBinHexString(value));
	}

	internal static byte[] FromBinHexString(string value)
	{
		char[] array = value.ToCharArray();
		byte[] array2 = new byte[array.Length / 2 + array.Length % 2];
		int num = array.Length;
		if (num % 2 != 0)
		{
			throw CreateInvalidValueException(value);
		}
		int num2 = 0;
		for (int i = 0; i < num - 1; i += 2)
		{
			array2[num2] = FromHex(array[i], value);
			array2[num2] <<= 4;
			array2[num2] += FromHex(array[i + 1], value);
			num2++;
		}
		return array2;
	}

	private static byte FromHex(char hexDigit, string value)
	{
		try
		{
			return byte.Parse(hexDigit.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		}
		catch (FormatException)
		{
			throw CreateInvalidValueException(value);
		}
	}

	private static Exception CreateInvalidValueException(string value)
	{
		return new RemotingException(string.Format(CultureInfo.InvariantCulture, "Invalid value '{0}' for xsd:{1}.", value, XsdType));
	}

	/// <summary>Returns <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary.Value" /> as a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that is obtained from <see cref="P:System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary.Value" />.</returns>
	public override string ToString()
	{
		sb.Length = 0;
		byte[] value = _value;
		foreach (byte b in value)
		{
			sb.Append(b.ToString("X2"));
		}
		return sb.ToString();
	}
}
