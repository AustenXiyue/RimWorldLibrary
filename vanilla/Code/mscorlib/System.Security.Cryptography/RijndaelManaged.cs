using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Accesses the managed version of the <see cref="T:System.Security.Cryptography.Rijndael" /> algorithm. This class cannot be inherited.</summary>
[ComVisible(true)]
public sealed class RijndaelManaged : Rijndael
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RijndaelManaged" /> class.</summary>
	/// <exception cref="T:System.InvalidOperationException">This class is not compliant with the FIPS algorithm.</exception>
	public RijndaelManaged()
	{
		if (CryptoConfig.AllowOnlyFipsAlgorithms)
		{
			throw new InvalidOperationException(Environment.GetResourceString("This implementation is not part of the Windows Platform FIPS validated cryptographic algorithms."));
		}
	}

	/// <summary>Creates a symmetric <see cref="T:System.Security.Cryptography.Rijndael" /> encryptor object with the specified <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key" /> and initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />).</summary>
	/// <returns>A symmetric <see cref="T:System.Security.Cryptography.Rijndael" /> encryptor object.</returns>
	/// <param name="rgbKey">The secret key to be used for the symmetric algorithm. The key size must be 128, 192, or 256 bits.</param>
	/// <param name="rgbIV">The IV to be used for the symmetric algorithm. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rgbKey" /> parameter is null.-or-The <paramref name="rgbIV" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The value of the <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Mode" /> property is not <see cref="F:System.Security.Cryptography.CipherMode.ECB" />, <see cref="F:System.Security.Cryptography.CipherMode.CBC" />, or <see cref="F:System.Security.Cryptography.CipherMode.CFB" />.</exception>
	public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
	{
		return NewEncryptor(rgbKey, ModeValue, rgbIV, FeedbackSizeValue, RijndaelManagedTransformMode.Encrypt);
	}

	/// <summary>Creates a symmetric <see cref="T:System.Security.Cryptography.Rijndael" /> decryptor object with the specified <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key" /> and initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />).</summary>
	/// <returns>A symmetric <see cref="T:System.Security.Cryptography.Rijndael" /> decryptor object.</returns>
	/// <param name="rgbKey">The secret key to be used for the symmetric algorithm. The key size must be 128, 192, or 256 bits.</param>
	/// <param name="rgbIV">The IV to be used for the symmetric algorithm. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rgbKey" /> parameter is null.-or-The <paramref name="rgbIV" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The value of the <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Mode" /> property is not <see cref="F:System.Security.Cryptography.CipherMode.ECB" />, <see cref="F:System.Security.Cryptography.CipherMode.CBC" />, or <see cref="F:System.Security.Cryptography.CipherMode.CFB" />.</exception>
	public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
	{
		return NewEncryptor(rgbKey, ModeValue, rgbIV, FeedbackSizeValue, RijndaelManagedTransformMode.Decrypt);
	}

	/// <summary>Generates a random <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key" /> to be used for the algorithm.</summary>
	public override void GenerateKey()
	{
		KeyValue = Utils.GenerateRandom(KeySizeValue / 8);
	}

	/// <summary>Generates a random initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />) to be used for the algorithm.</summary>
	public override void GenerateIV()
	{
		IVValue = Utils.GenerateRandom(BlockSizeValue / 8);
	}

	private ICryptoTransform NewEncryptor(byte[] rgbKey, CipherMode mode, byte[] rgbIV, int feedbackSize, RijndaelManagedTransformMode encryptMode)
	{
		if (rgbKey == null)
		{
			rgbKey = Utils.GenerateRandom(KeySizeValue / 8);
		}
		if (rgbIV == null)
		{
			rgbIV = Utils.GenerateRandom(BlockSizeValue / 8);
		}
		return new RijndaelManagedTransform(rgbKey, mode, rgbIV, BlockSizeValue, feedbackSize, PaddingValue, encryptMode);
	}
}
