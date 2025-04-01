using System.Runtime.CompilerServices;

namespace System.Security.Cryptography;

/// <summary>Represents the abstract base class from which all implementations of the Advanced Encryption Standard (AES) must inherit. </summary>
[TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089")]
public abstract class Aes : SymmetricAlgorithm
{
	private static KeySizes[] s_legalBlockSizes = new KeySizes[1]
	{
		new KeySizes(128, 128, 0)
	};

	private static KeySizes[] s_legalKeySizes = new KeySizes[1]
	{
		new KeySizes(128, 256, 64)
	};

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Aes" /> class. </summary>
	protected Aes()
	{
		LegalBlockSizesValue = s_legalBlockSizes;
		LegalKeySizesValue = s_legalKeySizes;
		BlockSizeValue = 128;
		FeedbackSizeValue = 8;
		KeySizeValue = 256;
		ModeValue = CipherMode.CBC;
	}

	/// <summary>Creates a cryptographic object that is used to perform the symmetric algorithm.</summary>
	/// <returns>A cryptographic object that is used to perform the symmetric algorithm.</returns>
	public new static Aes Create()
	{
		return Create("AES");
	}

	/// <summary>Creates a cryptographic object that specifies the implementation of AES to use to perform the symmetric algorithm.</summary>
	/// <returns>A cryptographic object that is used to perform the symmetric algorithm.</returns>
	/// <param name="algorithmName">The name of the specific implementation of AES to use.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="algorithmName" /> parameter is null.</exception>
	public new static Aes Create(string algorithmName)
	{
		if (algorithmName == null)
		{
			throw new ArgumentNullException("algorithmName");
		}
		return CryptoConfig.CreateFromName(algorithmName) as Aes;
	}
}
