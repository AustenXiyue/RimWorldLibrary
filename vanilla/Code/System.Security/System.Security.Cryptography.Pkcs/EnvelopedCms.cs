using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using Mono.Security;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> class represents a CMS/PKCS #7 structure for enveloped data.</summary>
public sealed class EnvelopedCms
{
	private ContentInfo _content;

	private AlgorithmIdentifier _identifier;

	private X509Certificate2Collection _certs;

	private RecipientInfoCollection _recipients;

	private CryptographicAttributeObjectCollection _uattribs;

	private SubjectIdentifierType _idType;

	private int _version;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.EnvelopedCms.Certificates" /> property retrieves the set of certificates associated with the enveloped CMS/PKCS #7 message.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> collection that represents the X.509 certificates used with the enveloped CMS/PKCS #7 message. If no certificates exist, the property value is an empty collection.</returns>
	public X509Certificate2Collection Certificates => _certs;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.EnvelopedCms.ContentEncryptionAlgorithm" /> property retrieves the identifier of the algorithm used to encrypt the content.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.Pkcs.AlgorithmIdentifier" /> object that represents the algorithm identifier.</returns>
	public AlgorithmIdentifier ContentEncryptionAlgorithm
	{
		get
		{
			if (_identifier == null)
			{
				_identifier = new AlgorithmIdentifier();
			}
			return _identifier;
		}
	}

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.EnvelopedCms.ContentInfo" /> property retrieves the inner content information for the enveloped CMS/PKCS #7 message.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that represents the inner content information from the enveloped CMS/PKCS #7 message.</returns>
	public ContentInfo ContentInfo
	{
		get
		{
			if (_content == null)
			{
				Oid contentType = new Oid("1.2.840.113549.1.7.1");
				_content = new ContentInfo(contentType, new byte[0]);
			}
			return _content;
		}
	}

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.EnvelopedCms.RecipientInfos" /> property retrieves the recipient information associated with the enveloped CMS/PKCS #7 message.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfoCollection" /> collection that represents the recipient information. If no recipients exist, the property value is an empty collection.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public RecipientInfoCollection RecipientInfos => _recipients;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.EnvelopedCms.UnprotectedAttributes" /> property retrieves the unprotected (unencrypted) attributes associated with the enveloped CMS/PKCS #7 message. Unprotected attributes are not encrypted, and so do not have data confidentiality within an <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> object.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection that represents the unprotected attributes. If no unprotected attributes exist, the property value is an empty collection.</returns>
	public CryptographicAttributeObjectCollection UnprotectedAttributes => _uattribs;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.EnvelopedCms.Version" /> property retrieves the version of the enveloped CMS/PKCS #7 message.  </summary>
	/// <returns>An int value that represents the version of the enveloped CMS/PKCS #7 message.</returns>
	public int Version => _version;

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> class.</summary>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public EnvelopedCms()
	{
		_certs = new X509Certificate2Collection();
		_recipients = new RecipientInfoCollection();
		_uattribs = new CryptographicAttributeObjectCollection();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.#ctor(System.Security.Cryptography.Pkcs.ContentInfo)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> class by using the specified content information as the inner content type.</summary>
	/// <param name="contentInfo">An instance of the <see cref="P:System.Security.Cryptography.Pkcs.EnvelopedCms.ContentInfo" /> class that represents the content and its type.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public EnvelopedCms(ContentInfo contentInfo)
		: this()
	{
		if (contentInfo == null)
		{
			throw new ArgumentNullException("contentInfo");
		}
		_content = contentInfo;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.#ctor(System.Security.Cryptography.Pkcs.ContentInfo,System.Security.Cryptography.Pkcs.AlgorithmIdentifier)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> class by using the specified content information and encryption algorithm. The specified content information is to be used as the inner content type.</summary>
	/// <param name="contentInfo">A  <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that represents the content and its type.</param>
	/// <param name="encryptionAlgorithm">An <see cref="T:System.Security.Cryptography.Pkcs.AlgorithmIdentifier" /> object that specifies the encryption algorithm.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public EnvelopedCms(ContentInfo contentInfo, AlgorithmIdentifier encryptionAlgorithm)
		: this(contentInfo)
	{
		if (encryptionAlgorithm == null)
		{
			throw new ArgumentNullException("encryptionAlgorithm");
		}
		_identifier = encryptionAlgorithm;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.#ctor(System.Security.Cryptography.Pkcs.SubjectIdentifierType,System.Security.Cryptography.Pkcs.ContentInfo)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> class by using the specified subject identifier type and content information. The specified content information is to be used as the inner content type.</summary>
	/// <param name="recipientIdentifierType">A member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> enumeration that specifies the means of identifying the recipient.</param>
	/// <param name="contentInfo">A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that represents the content and its type.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public EnvelopedCms(SubjectIdentifierType recipientIdentifierType, ContentInfo contentInfo)
		: this(contentInfo)
	{
		_idType = recipientIdentifierType;
		if (_idType == SubjectIdentifierType.SubjectKeyIdentifier)
		{
			_version = 2;
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.#ctor(System.Security.Cryptography.Pkcs.SubjectIdentifierType,System.Security.Cryptography.Pkcs.ContentInfo,System.Security.Cryptography.Pkcs.AlgorithmIdentifier)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> class by using the specified subject identifier type, content information, and encryption algorithm. The specified content information is to be used as the inner content type.</summary>
	/// <param name="recipientIdentifierType">A member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> enumeration that specifies the means of identifying the recipient.</param>
	/// <param name="contentInfo">A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that represents the content and its type.</param>
	/// <param name="encryptionAlgorithm">An <see cref="T:System.Security.Cryptography.Pkcs.AlgorithmIdentifier" /> object that specifies the encryption algorithm.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	public EnvelopedCms(SubjectIdentifierType recipientIdentifierType, ContentInfo contentInfo, AlgorithmIdentifier encryptionAlgorithm)
		: this(contentInfo, encryptionAlgorithm)
	{
		_idType = recipientIdentifierType;
		if (_idType == SubjectIdentifierType.SubjectKeyIdentifier)
		{
			_version = 2;
		}
	}

	private X509IssuerSerial GetIssuerSerial(string issuer, byte[] serial)
	{
		X509IssuerSerial result = default(X509IssuerSerial);
		result.IssuerName = issuer;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (byte b in serial)
		{
			stringBuilder.Append(b.ToString("X2"));
		}
		result.SerialNumber = stringBuilder.ToString();
		return result;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decode(System.Byte[])" /> method decodes the specified enveloped CMS/PKCS #7 message and resets all member variables in the <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> object.</summary>
	/// <param name="encodedMessage">An array of byte values that represent the information to be decoded.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	[System.MonoTODO]
	public void Decode(byte[] encodedMessage)
	{
		if (encodedMessage == null)
		{
			throw new ArgumentNullException("encodedMessage");
		}
		PKCS7.ContentInfo contentInfo = new PKCS7.ContentInfo(encodedMessage);
		if (contentInfo.ContentType != "1.2.840.113549.1.7.3")
		{
			throw new Exception("");
		}
		PKCS7.EnvelopedData envelopedData = new PKCS7.EnvelopedData(contentInfo.Content);
		Oid contentType = new Oid(envelopedData.ContentInfo.ContentType);
		_content = new ContentInfo(contentType, new byte[0]);
		foreach (PKCS7.RecipientInfo recipientInfo in envelopedData.RecipientInfos)
		{
			AlgorithmIdentifier keyEncryptionAlgorithm = new AlgorithmIdentifier(new Oid(recipientInfo.Oid));
			SubjectIdentifier recipientIdentifier = null;
			if (recipientInfo.SubjectKeyIdentifier != null)
			{
				recipientIdentifier = new SubjectIdentifier(SubjectIdentifierType.SubjectKeyIdentifier, recipientInfo.SubjectKeyIdentifier);
			}
			else if (recipientInfo.Issuer != null && recipientInfo.Serial != null)
			{
				X509IssuerSerial issuerSerial = GetIssuerSerial(recipientInfo.Issuer, recipientInfo.Serial);
				recipientIdentifier = new SubjectIdentifier(SubjectIdentifierType.IssuerAndSerialNumber, issuerSerial);
			}
			KeyTransRecipientInfo ri = new KeyTransRecipientInfo(recipientInfo.Key, keyEncryptionAlgorithm, recipientIdentifier, recipientInfo.Version);
			_recipients.Add(ri);
		}
		_version = envelopedData.Version;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt" /> method decrypts the contents of the decoded enveloped CMS/PKCS #7 message. The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt" /> method searches the current user and computer My stores for the appropriate certificate and private key.</summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	///   <IPermission class="System.Security.Permissions.StorePermission, System.Security, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Flags="CreateStore, DeleteStore, OpenStore, EnumerateCertificates" />
	/// </PermissionSet>
	[System.MonoTODO]
	public void Decrypt()
	{
		throw new InvalidOperationException("not encrypted");
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt(System.Security.Cryptography.Pkcs.RecipientInfo)" /> method decrypts the contents of the decoded enveloped CMS/PKCS #7 message by using the private key associated with the certificate identified by the specified recipient information.</summary>
	/// <param name="recipientInfo">A <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> object that represents the recipient information that identifies the certificate associated with the private key to use for the decryption.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	[System.MonoTODO]
	public void Decrypt(RecipientInfo recipientInfo)
	{
		if (recipientInfo == null)
		{
			throw new ArgumentNullException("recipientInfo");
		}
		Decrypt();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt(System.Security.Cryptography.Pkcs.RecipientInfo,System.Security.Cryptography.X509Certificates.X509Certificate2Collection)" /> method decrypts the contents of the decoded enveloped CMS/PKCS #7 message by using the private key associated with the certificate identified by the specified recipient information and by using the specified certificate collection.  The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt(System.Security.Cryptography.Pkcs.RecipientInfo,System.Security.Cryptography.X509Certificates.X509Certificate2Collection)" /> method searches the specified certificate collection and the My certificate store for the proper certificate to use for the decryption.</summary>
	/// <param name="recipientInfo">A <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> object that represents the recipient information to use for the decryption.</param>
	/// <param name="extraStore">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> collection that represents additional certificates to use for the decryption. The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt(System.Security.Cryptography.Pkcs.RecipientInfo,System.Security.Cryptography.X509Certificates.X509Certificate2Collection)" /> method searches this certificate collection and the My certificate store for the proper certificate to use for the decryption.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	[System.MonoTODO]
	public void Decrypt(RecipientInfo recipientInfo, X509Certificate2Collection extraStore)
	{
		if (recipientInfo == null)
		{
			throw new ArgumentNullException("recipientInfo");
		}
		if (extraStore == null)
		{
			throw new ArgumentNullException("extraStore");
		}
		Decrypt();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt(System.Security.Cryptography.X509Certificates.X509Certificate2Collection)" /> method decrypts the contents of the decoded enveloped CMS/PKCS #7 message by using the specified certificate collection. The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt(System.Security.Cryptography.X509Certificates.X509Certificate2Collection)" /> method searches the specified certificate collection and the My certificate store for the proper certificate to use for the decryption.</summary>
	/// <param name="extraStore">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> collection that represents additional certificates to use for the decryption. The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt(System.Security.Cryptography.X509Certificates.X509Certificate2Collection)" /> method searches this certificate collection and the My certificate store for the proper certificate to use for the decryption.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	[System.MonoTODO]
	public void Decrypt(X509Certificate2Collection extraStore)
	{
		if (extraStore == null)
		{
			throw new ArgumentNullException("extraStore");
		}
		Decrypt();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Encode" /> method encodes the contents of the enveloped CMS/PKCS #7 message and returns it as an array of byte values. Encryption must be done before encoding.</summary>
	/// <returns>If the method succeeds, the method returns an array of byte values that represent the encoded information.If the method fails, it throws an exception.</returns>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO]
	public byte[] Encode()
	{
		throw new InvalidOperationException("not encrypted");
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Encrypt" /> method encrypts the contents of the CMS/PKCS #7 message.</summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.StorePermission, System.Security, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Flags="CreateStore, DeleteStore, OpenStore, EnumerateCertificates" />
	/// </PermissionSet>
	[System.MonoTODO]
	public void Encrypt()
	{
		if (_content == null || _content.Content == null || _content.Content.Length == 0)
		{
			throw new CryptographicException("no content to encrypt");
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Encrypt(System.Security.Cryptography.Pkcs.CmsRecipient)" /> method encrypts the contents of the CMS/PKCS #7 message by using the specified recipient information.</summary>
	/// <param name="recipient">A <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipient" /> object that represents the recipient information.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	[System.MonoTODO]
	public void Encrypt(CmsRecipient recipient)
	{
		if (recipient == null)
		{
			throw new ArgumentNullException("recipient");
		}
		Encrypt();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Encrypt(System.Security.Cryptography.Pkcs.CmsRecipientCollection)" /> method encrypts the contents of the CMS/PKCS #7 message by using the information for the specified list of recipients. The message is encrypted by using a message encryption key with a symmetric encryption algorithm such as triple DES. The message encryption key is then encrypted with the public key of each recipient.</summary>
	/// <param name="recipients">A <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipientCollection" /> collection that represents the information for the list of recipients.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	[System.MonoTODO]
	public void Encrypt(CmsRecipientCollection recipients)
	{
		if (recipients == null)
		{
			throw new ArgumentNullException("recipients");
		}
	}
}
