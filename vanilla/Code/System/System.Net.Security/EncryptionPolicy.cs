namespace System.Net.Security;

/// <summary>The EncryptionPolicy to use. </summary>
public enum EncryptionPolicy
{
	/// <summary>Require encryption and never allow a NULL cipher.</summary>
	RequireEncryption,
	/// <summary>Prefer that full encryption be used, but allow a NULL cipher (no encryption) if the server agrees. </summary>
	AllowNoEncryption,
	/// <summary>Allow no encryption and request that a NULL cipher be used if the other endpoint can handle a NULL cipher.</summary>
	NoEncryption
}
