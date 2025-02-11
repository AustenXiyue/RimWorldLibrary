namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> enumeration defines the type of subject identifier.</summary>
public enum SubjectIdentifierType
{
	/// <summary>The type of subject identifier is unknown.</summary>
	Unknown,
	/// <summary>The subject is identified by the certificate issuer and serial number.</summary>
	IssuerAndSerialNumber,
	/// <summary>The subject is identified by the hash of the subject's public key. The hash algorithm used is determined by the signature algorithm suite in the subject's certificate.</summary>
	SubjectKeyIdentifier,
	/// <summary>The subject is identified as taking part in an integrity check operation that uses only a hashing algorithm.</summary>
	NoSignature
}
