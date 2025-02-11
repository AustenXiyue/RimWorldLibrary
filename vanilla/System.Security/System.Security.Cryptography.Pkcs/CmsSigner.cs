using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> class provides signing functionality.</summary>
public sealed class CmsSigner
{
	private SubjectIdentifierType _signer;

	private X509Certificate2 _certificate;

	private X509Certificate2Collection _coll;

	private Oid _digest;

	private X509IncludeOption _options;

	private CryptographicAttributeObjectCollection _signed;

	private CryptographicAttributeObjectCollection _unsigned;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.SignedAttributes" /> property retrieves the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection of signed attributes to be associated with the resulting <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> content. Signed attributes are signed along with the specified content.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection that represents the signed attributes. If there are no signed attributes, the property is an empty collection.</returns>
	public CryptographicAttributeObjectCollection SignedAttributes => _signed;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.Certificate" /> property sets or retrieves the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object that represents the signing certificate.</summary>
	/// <returns>An  <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object that represents the signing certificate.</returns>
	public X509Certificate2 Certificate
	{
		get
		{
			return _certificate;
		}
		set
		{
			_certificate = value;
		}
	}

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.Certificates" /> property retrieves the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> collection that contains certificates associated with the message to be signed.  </summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> collection that represents the collection of  certificates associated with the message to be signed.</returns>
	public X509Certificate2Collection Certificates => _coll;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.DigestAlgorithm" /> property sets or retrieves the <see cref="T:System.Security.Cryptography.Oid" /> that represents the hash algorithm used with the signature.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.Oid" /> object that represents the hash algorithm used with the signature.</returns>
	public Oid DigestAlgorithm
	{
		get
		{
			return _digest;
		}
		set
		{
			_digest = value;
		}
	}

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.IncludeOption" /> property sets or retrieves the option that controls whether the root and entire chain associated with the signing certificate are included with the created CMS/PKCS #7 message.</summary>
	/// <returns>A member of the <see cref="T:System.Security.Cryptography.X509Certificates.X509IncludeOption" /> enumeration that specifies how much of the X509 certificate chain should be included in the <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> object. The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.IncludeOption" /> property can be one of the following <see cref="T:System.Security.Cryptography.X509Certificates.X509IncludeOption" /> members.NameValueMeaning<see cref="F:System.Security.Cryptography.X509Certificates.X509IncludeOption.None" />0The certificate chain is not included.<see cref="F:System.Security.Cryptography.X509Certificates.X509IncludeOption.ExcludeRoot" />1The certificate chain, except for the root certificate, is included.<see cref="F:System.Security.Cryptography.X509Certificates.X509IncludeOption.EndCertOnly" />2Only the end certificate is included.<see cref="F:System.Security.Cryptography.X509Certificates.X509IncludeOption.WholeChain" />3The certificate chain, including the root certificate, is included.</returns>
	/// <exception cref="T:System.ArgumentException">One of the arguments provided to a method was not valid.</exception>
	public X509IncludeOption IncludeOption
	{
		get
		{
			return _options;
		}
		set
		{
			_options = value;
		}
	}

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.SignerIdentifierType" /> property sets or retrieves the type of the identifier of the signer.</summary>
	/// <returns>A member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> enumeration that specifies the type of the identifier of the signer.</returns>
	/// <exception cref="T:System.ArgumentException">One of the arguments provided to a method was not valid.</exception>
	public SubjectIdentifierType SignerIdentifierType
	{
		get
		{
			return _signer;
		}
		set
		{
			if (value == SubjectIdentifierType.Unknown)
			{
				throw new ArgumentException("value");
			}
			_signer = value;
		}
	}

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.UnsignedAttributes" /> property retrieves the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection of unsigned PKCS #9 attributes to be associated with the resulting <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> content. Unsigned attributes can be modified without invalidating the signature.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection that represents the unsigned attributes. If there are no unsigned attributes, the property is an empty collection.</returns>
	public CryptographicAttributeObjectCollection UnsignedAttributes => _unsigned;

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.CmsSigner.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> class by using a default subject identifier type.</summary>
	public CmsSigner()
	{
		_signer = SubjectIdentifierType.IssuerAndSerialNumber;
		_digest = new Oid("1.3.14.3.2.26");
		_options = X509IncludeOption.ExcludeRoot;
		_signed = new CryptographicAttributeObjectCollection();
		_unsigned = new CryptographicAttributeObjectCollection();
		_coll = new X509Certificate2Collection();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.CmsSigner.#ctor(System.Security.Cryptography.Pkcs.SubjectIdentifierType)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> class with the specified subject identifier type.</summary>
	/// <param name="signerIdentifierType">A member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> enumeration that specifies the signer identifier type.</param>
	public CmsSigner(SubjectIdentifierType signerIdentifierType)
		: this()
	{
		if (signerIdentifierType == SubjectIdentifierType.Unknown)
		{
			_signer = SubjectIdentifierType.IssuerAndSerialNumber;
		}
		else
		{
			_signer = signerIdentifierType;
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.CmsSigner.#ctor(System.Security.Cryptography.Pkcs.SubjectIdentifierType,System.Security.Cryptography.X509Certificates.X509Certificate2)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> class with the specified signer identifier type and signing certificate.</summary>
	/// <param name="signerIdentifierType">A member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> enumeration that specifies the signer identifier type.</param>
	/// <param name="certificate">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object that represents the signing certificate.</param>
	public CmsSigner(SubjectIdentifierType signerIdentifierType, X509Certificate2 certificate)
		: this(signerIdentifierType)
	{
		_certificate = certificate;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.CmsSigner.#ctor(System.Security.Cryptography.X509Certificates.X509Certificate2)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> class with the specified signing certificate.</summary>
	/// <param name="certificate">An    <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object that represents the signing certificate.</param>
	public CmsSigner(X509Certificate2 certificate)
		: this()
	{
		_certificate = certificate;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.CmsSigner.#ctor(System.Security.Cryptography.CspParameters)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> class with the specified cryptographic service provider (CSP) parameters. <see cref="M:System.Security.Cryptography.Pkcs.CmsSigner.#ctor(System.Security.Cryptography.CspParameters)" /> is useful when you know the specific CSP and private key to use for signing.</summary>
	/// <param name="parameters">A <see cref="T:System.Security.Cryptography.CspParameters" />  object that represents the set of CSP parameters to use.</param>
	[System.MonoTODO]
	public CmsSigner(CspParameters parameters)
		: this()
	{
	}
}
