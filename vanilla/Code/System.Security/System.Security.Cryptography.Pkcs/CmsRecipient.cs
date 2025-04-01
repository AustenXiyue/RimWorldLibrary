using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipient" /> class defines the recipient of a CMS/PKCS #7 message.</summary>
public sealed class CmsRecipient
{
	private SubjectIdentifierType _recipient;

	private X509Certificate2 _certificate;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsRecipient.Certificate" /> property retrieves the certificate associated with the recipient.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object that holds the certificate associated with the recipient.</returns>
	public X509Certificate2 Certificate => _certificate;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsRecipient.RecipientIdentifierType" /> property retrieves the type of the identifier of the recipient.</summary>
	/// <returns>A member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> enumeration that specifies the type of the identifier of the recipient.</returns>
	public SubjectIdentifierType RecipientIdentifierType => _recipient;

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.CmsRecipient.#ctor(System.Security.Cryptography.X509Certificates.X509Certificate2)" /> constructor constructs an instance of the <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipient" /> class by using the specified recipient certificate.</summary>
	/// <param name="certificate">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object that represents the recipient certificate.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	public CmsRecipient(X509Certificate2 certificate)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		_recipient = SubjectIdentifierType.IssuerAndSerialNumber;
		_certificate = certificate;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.CmsRecipient.#ctor(System.Security.Cryptography.Pkcs.SubjectIdentifierType,System.Security.Cryptography.X509Certificates.X509Certificate2)" /> constructor constructs an instance of the <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipient" /> class by using the specified recipient identifier type and recipient certificate.</summary>
	/// <param name="recipientIdentifierType">A member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> enumeration that specifies the type of the identifier of the recipient.</param>
	/// <param name="certificate">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object that represents the recipient certificate.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	public CmsRecipient(SubjectIdentifierType recipientIdentifierType, X509Certificate2 certificate)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		if (recipientIdentifierType == SubjectIdentifierType.Unknown)
		{
			_recipient = SubjectIdentifierType.IssuerAndSerialNumber;
		}
		else
		{
			_recipient = recipientIdentifierType;
		}
		_certificate = certificate;
	}
}
