using Unity;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifier" /> class defines the type of the identifier of a subject, such as a <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> or a <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipient" />.  The subject can be identified by the certificate issuer and serial number or the subject key.</summary>
public sealed class SubjectIdentifier
{
	private SubjectIdentifierType _type;

	private object _value;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifier.Type" /> property retrieves the type of subject identifier. The subject can be identified by the certificate issuer and serial number or the subject key.</summary>
	/// <returns>A member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" />  enumeration that identifies the type of subject.</returns>
	public SubjectIdentifierType Type => _type;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifier.Value" /> property retrieves the value of the subject identifier. Use the <see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifier.Type" /> property to determine the type of subject identifier, and use the <see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifier.Value" /> property to retrieve the corresponding value.</summary>
	/// <returns>An <see cref="T:System.Object" /> object that represents the value of the subject identifier. This <see cref="T:System.Object" /> can be one of the following objects as determined by the <see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifier.Type" /> property.<see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifier.Type" /> propertyObjectIssuerAndSerialNumber<see cref="T:System.Security.Cryptography.Xml.X509IssuerSerial" />SubjectKeyIdentifier<see cref="T:System.String" /></returns>
	public object Value => _value;

	internal SubjectIdentifier(SubjectIdentifierType type, object value)
	{
		_type = type;
		_value = value;
	}

	internal SubjectIdentifier()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
