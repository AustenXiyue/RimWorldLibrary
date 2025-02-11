using Mono.Security;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9MessageDigest" /> class defines the message digest of a CMS/PKCS #7 message.</summary>
public sealed class Pkcs9MessageDigest : Pkcs9AttributeObject
{
	internal const string oid = "1.2.840.113549.1.9.4";

	internal const string friendlyName = "Message Digest";

	private byte[] _messageDigest;

	private byte[] _encoded;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs9MessageDigest.MessageDigest" /> property retrieves the message digest.</summary>
	/// <returns>An array of byte values that contains the message digest.</returns>
	public byte[] MessageDigest
	{
		get
		{
			if (_encoded != null)
			{
				Decode(_encoded);
			}
			return _messageDigest;
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9MessageDigest.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9MessageDigest" /> class.</summary>
	public Pkcs9MessageDigest()
	{
		((AsnEncodedData)this).Oid = new Oid("1.2.840.113549.1.9.4", "Message Digest");
		_encoded = null;
	}

	internal Pkcs9MessageDigest(byte[] messageDigest, bool encoded)
	{
		if (messageDigest == null)
		{
			throw new ArgumentNullException("messageDigest");
		}
		if (encoded)
		{
			((AsnEncodedData)this).Oid = new Oid("1.2.840.113549.1.9.4", "Message Digest");
			base.RawData = messageDigest;
			Decode(messageDigest);
		}
		else
		{
			((AsnEncodedData)this).Oid = new Oid("1.2.840.113549.1.9.4", "Message Digest");
			_messageDigest = (byte[])_messageDigest.Clone();
			base.RawData = Encode();
		}
	}

	/// <summary>Copies information from an <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object.</summary>
	/// <param name="asnEncodedData">The <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object from which to copy information.</param>
	public override void CopyFrom(AsnEncodedData asnEncodedData)
	{
		base.CopyFrom(asnEncodedData);
		_encoded = asnEncodedData.RawData;
	}

	internal void Decode(byte[] attribute)
	{
		if (attribute == null || attribute[0] != 4)
		{
			throw new CryptographicException(global::Locale.GetText("Expected an OCTETSTRING."));
		}
		ASN1 aSN = new ASN1(attribute);
		_messageDigest = aSN.Value;
		_encoded = null;
	}

	internal byte[] Encode()
	{
		return new ASN1(4, _messageDigest).GetBytes();
	}
}
