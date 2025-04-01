namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierOrKeyType" /> enumeration defines how a subject is identified.</summary>
public enum SubjectIdentifierOrKeyType
{
	/// <summary>The type is unknown.</summary>
	Unknown,
	/// <summary>The subject is identified by the certificate issuer and serial number.</summary>
	IssuerAndSerialNumber,
	/// <summary>The subject is identified by the hash of the subject key.</summary>
	SubjectKeyIdentifier,
	/// <summary>The subject is identified by the public key.</summary>
	PublicKeyInfo
}
