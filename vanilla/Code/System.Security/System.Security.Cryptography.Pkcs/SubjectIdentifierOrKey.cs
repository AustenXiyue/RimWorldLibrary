using Unity;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierOrKey" /> class defines the type of the identifier of a subject, such as a <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> or a <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipient" />.  The subject can be identified by the certificate issuer and serial number, the hash of the subject key, or the subject key. </summary>
public sealed class SubjectIdentifierOrKey
{
	private SubjectIdentifierOrKeyType _type;

	private object _value;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifierOrKey.Type" /> property retrieves the type of subject identifier or key. The subject can be identified by the certificate issuer and serial number, the hash of the subject key, or the subject key.</summary>
	/// <returns>A member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierOrKeyType" />  enumeration that specifies the type of subject identifier.</returns>
	public SubjectIdentifierOrKeyType Type => _type;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifierOrKey.Value" /> property retrieves the value of the subject identifier or  key. Use the <see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifierOrKey.Type" /> property to determine the type of subject identifier or key, and use the <see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifierOrKey.Value" /> property to retrieve the corresponding value.</summary>
	/// <returns>An <see cref="T:System.Object" /> object that represents the value of the subject identifier or key. This <see cref="T:System.Object" /> can be one of the following objects as determined by the <see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifierOrKey.Type" /> property.<see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifierOrKey.Type" /> propertyObjectIssuerAndSerialNumber<see cref="T:System.Security.Cryptography.Xml.X509IssuerSerial" />SubjectKeyIdentifier<see cref="T:System.String" />PublicKeyInfo<see cref="T:System.Security.Cryptography.Pkcs.PublicKeyInfo" /></returns>
	public object Value => _value;

	internal SubjectIdentifierOrKey(SubjectIdentifierOrKeyType type, object value)
	{
		_type = type;
		_value = value;
	}

	internal SubjectIdentifierOrKey()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
