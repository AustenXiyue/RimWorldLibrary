using Mono.Security;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> class represents the CMS/PKCS #7 ContentInfo data structure as defined in the CMS/PKCS #7 standards document. This data structure is the basis for all CMS/PKCS #7 messages.</summary>
public sealed class ContentInfo
{
	private Oid _oid;

	private byte[] _content;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.ContentInfo.Content" /> property  retrieves the content of the CMS/PKCS #7 message.</summary>
	/// <returns>An array of byte values that represents the content data.</returns>
	public byte[] Content => (byte[])_content.Clone();

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.ContentInfo.ContentType" /> property  retrieves the <see cref="T:System.Security.Cryptography.Oid" />   object that contains the <paramref name="object identifier" /> (OID)  of the content type of the inner content of the CMS/PKCS #7 message.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.Oid" />  object that contains the OID value that represents the content type.</returns>
	public Oid ContentType => _oid;

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.ContentInfo.#ctor(System.Byte[])" /> constructor  creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> class by using an array of byte values as the data and a default <paramref name="object identifier" /> (OID) that represents the content type.</summary>
	/// <param name="content">An array of byte values that represents the data from which to create the <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference  was passed to a method that does not accept it as a valid argument. </exception>
	public ContentInfo(byte[] content)
		: this(new Oid("1.2.840.113549.1.7.1"), content)
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.ContentInfo.#ctor(System.Security.Cryptography.Oid,System.Byte[])" />  constructor  creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> class by using the specified content type and an array of byte values as the data.</summary>
	/// <param name="contentType">An <see cref="T:System.Security.Cryptography.Oid" /> object that contains an <paramref name="object identifier" /> (OID) that specifies the content type of the content. This can be data, digestedData, encryptedData, envelopedData, hashedData, signedAndEnvelopedData, or signedData.  For more information, see  Remarks.</param>
	/// <param name="content">An array of byte values that represents the data from which to create the <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference  was passed to a method that does not accept it as a valid argument. </exception>
	public ContentInfo(Oid contentType, byte[] content)
	{
		if (contentType == null)
		{
			throw new ArgumentNullException("contentType");
		}
		if (content == null)
		{
			throw new ArgumentNullException("content");
		}
		_oid = contentType;
		_content = content;
	}

	~ContentInfo()
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.ContentInfo.GetContentType(System.Byte[])" /> static method  retrieves the outer content type of the encoded <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> message represented by an array of byte values.</summary>
	/// <returns>If the method succeeds, the method returns an <see cref="T:System.Security.Cryptography.Oid" /> object that contains the outer content type of the specified encoded <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> message.If the method fails, it throws an exception.</returns>
	/// <param name="encodedMessage">An array of byte values that represents the encoded <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> message from which to retrieve the outer content type.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference  was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error occurred during a cryptographic operation.</exception>
	[System.MonoTODO("MS is stricter than us about the content structure")]
	public static Oid GetContentType(byte[] encodedMessage)
	{
		if (encodedMessage == null)
		{
			throw new ArgumentNullException("algorithm");
		}
		try
		{
			PKCS7.ContentInfo contentInfo = new PKCS7.ContentInfo(encodedMessage);
			switch (contentInfo.ContentType)
			{
			case "1.2.840.113549.1.7.1":
			case "1.2.840.113549.1.7.2":
			case "1.2.840.113549.1.7.3":
			case "1.2.840.113549.1.7.5":
			case "1.2.840.113549.1.7.6":
				return new Oid(contentInfo.ContentType);
			default:
				throw new CryptographicException(string.Format(global::Locale.GetText("Bad ASN1 - invalid OID '{0}'"), contentInfo.ContentType));
			}
		}
		catch (Exception inner)
		{
			throw new CryptographicException(global::Locale.GetText("Bad ASN1 - invalid structure"), inner);
		}
	}
}
