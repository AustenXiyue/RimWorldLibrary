namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.KeyAgreeKeyChoice" /> enumeration defines the type of key used in a key agreement protocol.</summary>
public enum KeyAgreeKeyChoice
{
	/// <summary>The key agreement key type is unknown.</summary>
	Unknown,
	/// <summary>The key agreement key is ephemeral, existing only for the duration of the key agreement protocol.</summary>
	EphemeralKey,
	/// <summary>The key agreement key is static, existing for an extended period of time.</summary>
	StaticKey
}
