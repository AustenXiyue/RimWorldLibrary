using Mono.Security;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9ContentType" /> class defines the type of the content of a CMS/PKCS #7 message.</summary>
public sealed class Pkcs9ContentType : Pkcs9AttributeObject
{
	internal const string oid = "1.2.840.113549.1.9.3";

	internal const string friendlyName = "Content Type";

	private Oid _contentType;

	private byte[] _encoded;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs9ContentType.ContentType" /> property gets an <see cref="T:System.Security.Cryptography.Oid" /> object that contains the content type.</summary>
	/// <returns>An  <see cref="T:System.Security.Cryptography.Oid" /> object that contains the content type.</returns>
	public Oid ContentType
	{
		get
		{
			if (_encoded != null)
			{
				Decode(_encoded);
			}
			return _contentType;
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9ContentType.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9ContentType" /> class.</summary>
	public Pkcs9ContentType()
	{
		((AsnEncodedData)this).Oid = new Oid("1.2.840.113549.1.9.3", "Content Type");
		_encoded = null;
	}

	internal Pkcs9ContentType(string contentType)
	{
		((AsnEncodedData)this).Oid = new Oid("1.2.840.113549.1.9.3", "Content Type");
		_contentType = new Oid(contentType);
		base.RawData = Encode();
		_encoded = null;
	}

	internal Pkcs9ContentType(byte[] encodedContentType)
	{
		if (encodedContentType == null)
		{
			throw new ArgumentNullException("encodedContentType");
		}
		((AsnEncodedData)this).Oid = new Oid("1.2.840.113549.1.9.3", "Content Type");
		base.RawData = encodedContentType;
		Decode(encodedContentType);
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
		if (attribute == null || attribute[0] != 6)
		{
			throw new CryptographicException(global::Locale.GetText("Expected an OID."));
		}
		ASN1 asn = new ASN1(attribute);
		_contentType = new Oid(ASN1Convert.ToOid(asn));
		_encoded = null;
	}

	internal byte[] Encode()
	{
		if (_contentType == null)
		{
			return null;
		}
		return ASN1Convert.FromOid(_contentType.Value).GetBytes();
	}
}
