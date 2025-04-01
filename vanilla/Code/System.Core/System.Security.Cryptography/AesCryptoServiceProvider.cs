using System.Security.Permissions;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography;

/// <summary>Performs symmetric encryption and decryption using the Cryptographic Application Programming Interfaces (CAPI) implementation of the Advanced Encryption Standard (AES) algorithm. </summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class AesCryptoServiceProvider : Aes
{
	public override byte[] IV
	{
		get
		{
			return base.IV;
		}
		set
		{
			base.IV = value;
		}
	}

	/// <summary>Gets or sets the symmetric key that is used for encryption and decryption.</summary>
	/// <returns>The symmetric key that is used for encryption and decryption.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value for the key is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The size of the key is invalid.</exception>
	public override byte[] Key
	{
		get
		{
			return base.Key;
		}
		set
		{
			base.Key = value;
		}
	}

	/// <summary>Gets or sets the size, in bits, of the secret key. </summary>
	/// <returns>The size, in bits, of the key.</returns>
	public override int KeySize
	{
		get
		{
			return base.KeySize;
		}
		set
		{
			base.KeySize = value;
		}
	}

	public override int FeedbackSize
	{
		get
		{
			return base.FeedbackSize;
		}
		set
		{
			base.FeedbackSize = value;
		}
	}

	public override CipherMode Mode
	{
		get
		{
			return base.Mode;
		}
		set
		{
			if (value == CipherMode.CTS)
			{
				throw new CryptographicException("CTS is not supported");
			}
			base.Mode = value;
		}
	}

	public override PaddingMode Padding
	{
		get
		{
			return base.Padding;
		}
		set
		{
			base.Padding = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.AesCryptoServiceProvider" /> class. </summary>
	/// <exception cref="T:System.PlatformNotSupportedException">There is no supported key size for the current platform.</exception>
	public AesCryptoServiceProvider()
	{
		FeedbackSizeValue = 8;
	}

	/// <summary>Generates a random initialization vector (IV) to use for the algorithm.</summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The initialization vector (IV) could not be generated. </exception>
	public override void GenerateIV()
	{
		IVValue = KeyBuilder.IV(BlockSizeValue >> 3);
	}

	/// <summary>Generates a random key to use for the algorithm. </summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The key could not be generated.</exception>
	public override void GenerateKey()
	{
		KeyValue = KeyBuilder.Key(KeySizeValue >> 3);
	}

	/// <summary>Creates a symmetric AES decryptor object using the specified key and initialization vector (IV).</summary>
	/// <returns>A symmetric AES decryptor object.</returns>
	/// <param name="key">The secret key to use for the symmetric algorithm.</param>
	/// <param name="iv">The initialization vector to use for the symmetric algorithm.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> or <paramref name="iv" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="key" /> is invalid.</exception>
	public override ICryptoTransform CreateDecryptor(byte[] key, byte[] iv)
	{
		if (Mode == CipherMode.CFB && FeedbackSize > 64)
		{
			throw new CryptographicException("CFB with Feedbaack > 64 bits");
		}
		return new AesTransform(this, encryption: false, key, iv);
	}

	/// <summary>Creates a symmetric encryptor object using the specified key and initialization vector (IV).</summary>
	/// <returns>A symmetric AES encryptor object.</returns>
	/// <param name="key">The secret key to use for the symmetric algorithm.</param>
	/// <param name="iv">The initialization vector to use for the symmetric algorithm.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="key" /> or <paramref name="iv" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="key" /> is invalid.</exception>
	public override ICryptoTransform CreateEncryptor(byte[] key, byte[] iv)
	{
		if (Mode == CipherMode.CFB && FeedbackSize > 64)
		{
			throw new CryptographicException("CFB with Feedbaack > 64 bits");
		}
		return new AesTransform(this, encryption: true, key, iv);
	}

	/// <summary>Creates a symmetric AES decryptor object using the current key and initialization vector (IV).</summary>
	/// <returns>A symmetric AES decryptor object.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The current key is invalid or missing.</exception>
	public override ICryptoTransform CreateDecryptor()
	{
		return CreateDecryptor(Key, IV);
	}

	/// <summary>Creates a symmetric AES encryptor object using the current key and initialization vector (IV).</summary>
	/// <returns>A symmetric AES encryptor object.</returns>
	public override ICryptoTransform CreateEncryptor()
	{
		return CreateEncryptor(Key, IV);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}
}
