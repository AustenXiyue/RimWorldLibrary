namespace System.Security.Cryptography;

/// <summary>Specifies the key export policies for a key. </summary>
[Flags]
public enum CngExportPolicies
{
	/// <summary>No export policies are established. Key export is allowed without restriction.</summary>
	None = 0,
	/// <summary>The private key can be exported multiple times.</summary>
	AllowExport = 1,
	/// <summary>The private key can be exported multiple times as plaintext.</summary>
	AllowPlaintextExport = 2,
	/// <summary>The private key can be exported one time for archiving purposes.</summary>
	AllowArchiving = 4,
	/// <summary>The private key can be exported one time as plaintext.</summary>
	AllowPlaintextArchiving = 8
}
