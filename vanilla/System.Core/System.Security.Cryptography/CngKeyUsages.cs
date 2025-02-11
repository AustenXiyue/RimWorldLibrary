namespace System.Security.Cryptography;

/// <summary>Specifies the cryptographic operations that a Cryptography Next Generation (CNG) key may be used with. </summary>
[Flags]
public enum CngKeyUsages
{
	/// <summary>No usage values are assigned to the key.</summary>
	None = 0,
	/// <summary>The key can be used for encryption and decryption.</summary>
	Decryption = 1,
	/// <summary>The key can be used for signing and verification.</summary>
	Signing = 2,
	/// <summary>The key can be used for secret agreement generation and key exchange.</summary>
	KeyAgreement = 4,
	/// <summary>The key can be used for all purposes.</summary>
	AllUsages = 0xFFFFFF
}
