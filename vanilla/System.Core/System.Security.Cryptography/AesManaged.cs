namespace System.Security.Cryptography;

/// <summary>Provides a managed implementation of the Advanced Encryption Standard (AES) symmetric algorithm. </summary>
public sealed class AesManaged : Aes
{
	private RijndaelManaged m_rijndael;

	/// <summary>Gets or sets the number of bits to use as feedback. </summary>
	/// <returns>The feedback size, in bits.</returns>
	public override int FeedbackSize
	{
		get
		{
			return m_rijndael.FeedbackSize;
		}
		set
		{
			m_rijndael.FeedbackSize = value;
		}
	}

	/// <summary>Gets or sets the initialization vector (IV) to use for the symmetric algorithm. </summary>
	/// <returns>The initialization vector to use for the symmetric algorithm</returns>
	public override byte[] IV
	{
		get
		{
			return m_rijndael.IV;
		}
		set
		{
			m_rijndael.IV = value;
		}
	}

	/// <summary>Gets or sets the secret key used for the symmetric algorithm.</summary>
	/// <returns>The key for the symmetric algorithm.</returns>
	public override byte[] Key
	{
		get
		{
			return m_rijndael.Key;
		}
		set
		{
			m_rijndael.Key = value;
		}
	}

	/// <summary>Gets or sets the size, in bits, of the secret key used for the symmetric algorithm. </summary>
	/// <returns>The size, in bits, of the key used by the symmetric algorithm.</returns>
	public override int KeySize
	{
		get
		{
			return m_rijndael.KeySize;
		}
		set
		{
			m_rijndael.KeySize = value;
		}
	}

	/// <summary>Gets or sets the mode for operation of the symmetric algorithm.</summary>
	/// <returns>One of the enumeration values that specifies the block cipher mode to use for encryption. The default is <see cref="F:System.Security.Cryptography.CipherMode.CBC" />.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">
	///   <see cref="P:System.Security.Cryptography.AesManaged.Mode" /> is set to <see cref="F:System.Security.Cryptography.CipherMode.CFB" /> or <see cref="F:System.Security.Cryptography.CipherMode.OFB" />.</exception>
	public override CipherMode Mode
	{
		get
		{
			return m_rijndael.Mode;
		}
		set
		{
			if (value == CipherMode.CFB || value == CipherMode.OFB)
			{
				throw new CryptographicException(global::SR.GetString("The specified cipher mode is not valid for this algorithm."));
			}
			m_rijndael.Mode = value;
		}
	}

	/// <summary>Gets or sets the padding mode used in the symmetric algorithm. </summary>
	/// <returns>One of the enumeration values that specifies the type of padding to apply. The default is <see cref="F:System.Security.Cryptography.PaddingMode.PKCS7" />.</returns>
	public override PaddingMode Padding
	{
		get
		{
			return m_rijndael.Padding;
		}
		set
		{
			m_rijndael.Padding = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.AesManaged" /> class. </summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The Windows security policy setting for FIPS is enabled.</exception>
	/// <exception cref="T:System.InvalidOperationException">This implementation is not part of the Windows Platform FIPS-validated cryptographic algorithms.</exception>
	public AesManaged()
	{
		if (CryptoConfig.AllowOnlyFipsAlgorithms)
		{
			throw new InvalidOperationException(global::SR.GetString("This implementation is not part of the Windows Platform FIPS validated cryptographic algorithms."));
		}
		m_rijndael = new RijndaelManaged();
		m_rijndael.BlockSize = BlockSize;
		m_rijndael.KeySize = KeySize;
	}

	/// <summary>Creates a symmetric decryptor object using the current key and initialization vector (IV).</summary>
	/// <returns>A symmetric decryptor object.</returns>
	public override ICryptoTransform CreateDecryptor()
	{
		return m_rijndael.CreateDecryptor();
	}

	/// <summary>Creates a symmetric decryptor object using the specified key and initialization vector (IV).</summary>
	/// <returns>A symmetric decryptor object.</returns>
	/// <param name="key">The secret key to use for the symmetric algorithm.</param>
	/// <param name="iv">The initialization vector to use for the symmetric algorithm.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> or <paramref name="iv" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="key" /> is invalid.</exception>
	public override ICryptoTransform CreateDecryptor(byte[] key, byte[] iv)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (!ValidKeySize(key.Length * 8))
		{
			throw new ArgumentException(global::SR.GetString("The specified key is not a valid size for this algorithm."), "key");
		}
		if (iv != null && iv.Length * 8 != BlockSizeValue)
		{
			throw new ArgumentException(global::SR.GetString("The specified initialization vector (IV) does not match the block size for this algorithm."), "iv");
		}
		return m_rijndael.CreateDecryptor(key, iv);
	}

	/// <summary>Creates a symmetric encryptor object using the current key and initialization vector (IV).</summary>
	/// <returns>A symmetric encryptor object.</returns>
	public override ICryptoTransform CreateEncryptor()
	{
		return m_rijndael.CreateEncryptor();
	}

	/// <summary>Creates a symmetric encryptor object using the specified key and initialization vector (IV).</summary>
	/// <returns>A symmetric encryptor object.</returns>
	/// <param name="key">The secret key to use for the symmetric algorithm.</param>
	/// <param name="iv">The initialization vector to use for the symmetric algorithm.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> or <paramref name="iv" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="key" /> is invalid.</exception>
	public override ICryptoTransform CreateEncryptor(byte[] key, byte[] iv)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (!ValidKeySize(key.Length * 8))
		{
			throw new ArgumentException(global::SR.GetString("The specified key is not a valid size for this algorithm."), "key");
		}
		if (iv != null && iv.Length * 8 != BlockSizeValue)
		{
			throw new ArgumentException(global::SR.GetString("The specified initialization vector (IV) does not match the block size for this algorithm."), "iv");
		}
		return m_rijndael.CreateEncryptor(key, iv);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				((IDisposable)m_rijndael).Dispose();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	/// <summary>Generates a random initialization vector (IV) to use for the symmetric algorithm.</summary>
	public override void GenerateIV()
	{
		m_rijndael.GenerateIV();
	}

	/// <summary>Generates a random key to use for the symmetric algorithm. </summary>
	public override void GenerateKey()
	{
		m_rijndael.GenerateKey();
	}
}
