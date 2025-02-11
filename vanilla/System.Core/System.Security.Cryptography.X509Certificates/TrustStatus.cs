namespace System.Security.Cryptography.X509Certificates;

/// <summary>Identifies the level of trustworthiness that is assigned to the signature for a manifest.</summary>
public enum TrustStatus
{
	/// <summary>The signature was created by an explicitly distrusted publisher.</summary>
	Untrusted,
	/// <summary>The identity is not known and the signature is invalid. Because there is no verified signature, an identity cannot be determined.</summary>
	UnknownIdentity,
	/// <summary>The identity is known and the signature is valid. A valid Authenticode signature provides an identity.</summary>
	KnownIdentity,
	/// <summary>The signature is valid and was created by an explicitly trusted publisher.</summary>
	Trusted
}
