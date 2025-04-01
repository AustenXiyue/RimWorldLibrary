using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Represents the base class from which all implementations of the <see cref="T:System.Security.Cryptography.Rijndael" /> symmetric encryption algorithm must inherit.</summary>
[ComVisible(true)]
public abstract class Rijndael : SymmetricAlgorithm
{
	private static KeySizes[] s_legalBlockSizes = new KeySizes[1]
	{
		new KeySizes(128, 256, 64)
	};

	private static KeySizes[] s_legalKeySizes = new KeySizes[1]
	{
		new KeySizes(128, 256, 64)
	};

	/// <summary>Initializes a new instance of <see cref="T:System.Security.Cryptography.Rijndael" />.</summary>
	protected Rijndael()
	{
		KeySizeValue = 256;
		BlockSizeValue = 128;
		FeedbackSizeValue = BlockSizeValue;
		LegalBlockSizesValue = s_legalBlockSizes;
		LegalKeySizesValue = s_legalKeySizes;
	}

	/// <summary>Creates a cryptographic object to perform the <see cref="T:System.Security.Cryptography.Rijndael" /> algorithm.</summary>
	/// <returns>A cryptographic object.</returns>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The algorithm was used with Federal Information Processing Standards (FIPS) mode enabled, but is not FIPS compatible.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public new static Rijndael Create()
	{
		return Create("System.Security.Cryptography.Rijndael");
	}

	/// <summary>Creates a cryptographic object to perform the specified implementation of the <see cref="T:System.Security.Cryptography.Rijndael" /> algorithm.</summary>
	/// <returns>A cryptographic object.</returns>
	/// <param name="algName">The name of the specific implementation of <see cref="T:System.Security.Cryptography.Rijndael" /> to create. </param>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The algorithm described by the <paramref name="algName" /> parameter was used with Federal Information Processing Standards (FIPS) mode enabled, but is not FIPS compatible.</exception>
	public new static Rijndael Create(string algName)
	{
		return (Rijndael)CryptoConfig.CreateFromName(algName);
	}
}
