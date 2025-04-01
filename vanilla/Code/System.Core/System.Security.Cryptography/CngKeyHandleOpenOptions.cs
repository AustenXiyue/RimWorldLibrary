namespace System.Security.Cryptography;

/// <summary>Specifies options for opening key handles.</summary>
[Flags]
public enum CngKeyHandleOpenOptions
{
	/// <summary>The key handle being opened does not specify an ephemeral key.</summary>
	None = 0,
	/// <summary>The key handle being opened specifies an ephemeral key.</summary>
	EphemeralKey = 1
}
