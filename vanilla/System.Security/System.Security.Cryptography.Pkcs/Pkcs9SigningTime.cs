using System.Globalization;
using System.Text;
using Mono.Security;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9SigningTime" /> class defines the signing date and time of a signature. A <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9SigningTime" /> object can  be used as an authenticated attribute of a <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" />  object when an authenticated date and time are to accompany a digital signature.</summary>
public sealed class Pkcs9SigningTime : Pkcs9AttributeObject
{
	internal const string oid = "1.2.840.113549.1.9.5";

	internal const string friendlyName = "Signing Time";

	private DateTime _signingTime;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs9SigningTime.SigningTime" /> property retrieves a <see cref="T:System.DateTime" /> structure that represents the date and time that the message was signed.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> structure that contains the date and time the document was signed.</returns>
	public DateTime SigningTime => _signingTime;

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9SigningTime.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9SigningTime" /> class.</summary>
	public Pkcs9SigningTime()
	{
		((AsnEncodedData)this).Oid = new Oid("1.2.840.113549.1.9.5", "Signing Time");
		_signingTime = DateTime.Now;
		base.RawData = Encode();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9SigningTime.#ctor(System.DateTime)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9SigningTime" /> class by using the specified signing date and time.</summary>
	/// <param name="signingTime">A <see cref="T:System.DateTime" />  structure that represents the signing date and time of the signature.</param>
	public Pkcs9SigningTime(DateTime signingTime)
	{
		((AsnEncodedData)this).Oid = new Oid("1.2.840.113549.1.9.5", "Signing Time");
		_signingTime = signingTime;
		base.RawData = Encode();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9SigningTime.#ctor(System.Byte[])" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9SigningTime" /> class by using the specified array of byte values as the encoded signing date and time of the content of a CMS/PKCS #7 message.</summary>
	/// <param name="encodedSigningTime">An array of byte values that specifies the encoded signing date and time of the CMS/PKCS #7 message.</param>
	public Pkcs9SigningTime(byte[] encodedSigningTime)
	{
		if (encodedSigningTime == null)
		{
			throw new ArgumentNullException("encodedSigningTime");
		}
		((AsnEncodedData)this).Oid = new Oid("1.2.840.113549.1.9.5", "Signing Time");
		base.RawData = encodedSigningTime;
		Decode(encodedSigningTime);
	}

	/// <summary>Copies information from a <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object.</summary>
	/// <param name="asnEncodedData">The <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object from which to copy information.</param>
	public override void CopyFrom(AsnEncodedData asnEncodedData)
	{
		if (asnEncodedData == null)
		{
			throw new ArgumentNullException("asnEncodedData");
		}
		Decode(asnEncodedData.RawData);
		base.Oid = asnEncodedData.Oid;
		base.RawData = asnEncodedData.RawData;
	}

	internal void Decode(byte[] attribute)
	{
		if (attribute[0] != 23)
		{
			throw new CryptographicException(global::Locale.GetText("Only UTCTIME is supported."));
		}
		byte[] value = new ASN1(attribute).Value;
		string @string = Encoding.ASCII.GetString(value, 0, value.Length - 1);
		_signingTime = DateTime.ParseExact(@string, "yyMMddHHmmss", null);
	}

	internal byte[] Encode()
	{
		if (_signingTime.Year <= 1600)
		{
			throw new ArgumentOutOfRangeException("<= 1600");
		}
		if (_signingTime.Year < 1950 || _signingTime.Year >= 2050)
		{
			throw new CryptographicException("[1950,2049]");
		}
		string s = _signingTime.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture) + "Z";
		return new ASN1(23, Encoding.ASCII.GetBytes(s)).GetBytes();
	}
}
