using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Specifies the mode of a cryptographic stream.</summary>
[Serializable]
[ComVisible(true)]
public enum CryptoStreamMode
{
	/// <summary>Read access to a cryptographic stream.</summary>
	Read,
	/// <summary>Write access to a cryptographic stream.</summary>
	Write
}
