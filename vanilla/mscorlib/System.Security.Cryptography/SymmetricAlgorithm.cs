using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Represents the abstract base class from which all implementations of symmetric algorithms must inherit.</summary>
[ComVisible(true)]
public abstract class SymmetricAlgorithm : IDisposable
{
	/// <summary>Represents the block size, in bits, of the cryptographic operation.</summary>
	protected int BlockSizeValue;

	/// <summary>Represents the feedback size, in bits, of the cryptographic operation.</summary>
	protected int FeedbackSizeValue;

	/// <summary>Represents the initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />) for the symmetric algorithm.</summary>
	protected byte[] IVValue;

	/// <summary>Represents the secret key for the symmetric algorithm.</summary>
	protected byte[] KeyValue;

	/// <summary>Specifies the block sizes, in bits, that are supported by the symmetric algorithm.</summary>
	protected KeySizes[] LegalBlockSizesValue;

	/// <summary>Specifies the key sizes, in bits, that are supported by the symmetric algorithm.</summary>
	protected KeySizes[] LegalKeySizesValue;

	/// <summary>Represents the size, in bits, of the secret key used by the symmetric algorithm.</summary>
	protected int KeySizeValue;

	/// <summary>Represents the cipher mode used in the symmetric algorithm.</summary>
	protected CipherMode ModeValue;

	/// <summary>Represents the padding mode used in the symmetric algorithm.</summary>
	protected PaddingMode PaddingValue;

	/// <summary>Gets or sets the block size, in bits, of the cryptographic operation.</summary>
	/// <returns>The block size, in bits.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The block size is invalid. </exception>
	public virtual int BlockSize
	{
		get
		{
			return BlockSizeValue;
		}
		set
		{
			for (int i = 0; i < LegalBlockSizesValue.Length; i++)
			{
				if (LegalBlockSizesValue[i].SkipSize == 0)
				{
					if (LegalBlockSizesValue[i].MinSize == value)
					{
						BlockSizeValue = value;
						IVValue = null;
						return;
					}
					continue;
				}
				for (int j = LegalBlockSizesValue[i].MinSize; j <= LegalBlockSizesValue[i].MaxSize; j += LegalBlockSizesValue[i].SkipSize)
				{
					if (j == value)
					{
						if (BlockSizeValue != value)
						{
							BlockSizeValue = value;
							IVValue = null;
						}
						return;
					}
				}
			}
			throw new CryptographicException(Environment.GetResourceString("Specified block size is not valid for this algorithm."));
		}
	}

	/// <summary>Gets or sets the feedback size, in bits, of the cryptographic operation.</summary>
	/// <returns>The feedback size in bits.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The feedback size is larger than the block size. </exception>
	public virtual int FeedbackSize
	{
		get
		{
			return FeedbackSizeValue;
		}
		set
		{
			if (value <= 0 || value > BlockSizeValue || value % 8 != 0)
			{
				throw new CryptographicException(Environment.GetResourceString("Specified feedback size is invalid."));
			}
			FeedbackSizeValue = value;
		}
	}

	/// <summary>Gets or sets the initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />) for the symmetric algorithm.</summary>
	/// <returns>The initialization vector.</returns>
	/// <exception cref="T:System.ArgumentNullException">An attempt was made to set the initialization vector to null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An attempt was made to set the initialization vector to an invalid size. </exception>
	public virtual byte[] IV
	{
		get
		{
			if (IVValue == null)
			{
				GenerateIV();
			}
			return (byte[])IVValue.Clone();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Length != BlockSizeValue / 8)
			{
				throw new CryptographicException(Environment.GetResourceString("Specified initialization vector (IV) does not match the block size for this algorithm."));
			}
			IVValue = (byte[])value.Clone();
		}
	}

	/// <summary>Gets or sets the secret key for the symmetric algorithm.</summary>
	/// <returns>The secret key to use for the symmetric algorithm.</returns>
	/// <exception cref="T:System.ArgumentNullException">An attempt was made to set the key to null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The key size is invalid.</exception>
	public virtual byte[] Key
	{
		get
		{
			if (KeyValue == null)
			{
				GenerateKey();
			}
			return (byte[])KeyValue.Clone();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!ValidKeySize(value.Length * 8))
			{
				throw new CryptographicException(Environment.GetResourceString("Specified key is not a valid size for this algorithm."));
			}
			KeyValue = (byte[])value.Clone();
			KeySizeValue = value.Length * 8;
		}
	}

	/// <summary>Gets the block sizes, in bits, that are supported by the symmetric algorithm.</summary>
	/// <returns>An array that contains the block sizes supported by the algorithm.</returns>
	public virtual KeySizes[] LegalBlockSizes => (KeySizes[])LegalBlockSizesValue.Clone();

	/// <summary>Gets the key sizes, in bits, that are supported by the symmetric algorithm.</summary>
	/// <returns>An array that contains the key sizes supported by the algorithm.</returns>
	public virtual KeySizes[] LegalKeySizes => (KeySizes[])LegalKeySizesValue.Clone();

	/// <summary>Gets or sets the size, in bits, of the secret key used by the symmetric algorithm.</summary>
	/// <returns>The size, in bits, of the secret key used by the symmetric algorithm.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The key size is not valid. </exception>
	public virtual int KeySize
	{
		get
		{
			return KeySizeValue;
		}
		set
		{
			if (!ValidKeySize(value))
			{
				throw new CryptographicException(Environment.GetResourceString("Specified key is not a valid size for this algorithm."));
			}
			KeySizeValue = value;
			KeyValue = null;
		}
	}

	/// <summary>Gets or sets the mode for operation of the symmetric algorithm.</summary>
	/// <returns>The mode for operation of the symmetric algorithm. The default is <see cref="F:System.Security.Cryptography.CipherMode.CBC" />.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The cipher mode is not one of the <see cref="T:System.Security.Cryptography.CipherMode" /> values. </exception>
	public virtual CipherMode Mode
	{
		get
		{
			return ModeValue;
		}
		set
		{
			if (value < CipherMode.CBC || CipherMode.CFB < value)
			{
				throw new CryptographicException(Environment.GetResourceString("Specified cipher mode is not valid for this algorithm."));
			}
			ModeValue = value;
		}
	}

	/// <summary>Gets or sets the padding mode used in the symmetric algorithm.</summary>
	/// <returns>The padding mode used in the symmetric algorithm. The default is <see cref="F:System.Security.Cryptography.PaddingMode.PKCS7" />.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The padding mode is not one of the <see cref="T:System.Security.Cryptography.PaddingMode" /> values. </exception>
	public virtual PaddingMode Padding
	{
		get
		{
			return PaddingValue;
		}
		set
		{
			if (value < PaddingMode.None || PaddingMode.ISO10126 < value)
			{
				throw new CryptographicException(Environment.GetResourceString("Specified padding mode is not valid for this algorithm."));
			}
			PaddingValue = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.SymmetricAlgorithm" /> class.</summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The implementation of the class derived from the symmetric algorithm is not valid.</exception>
	protected SymmetricAlgorithm()
	{
		ModeValue = CipherMode.CBC;
		PaddingValue = PaddingMode.PKCS7;
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Security.Cryptography.SymmetricAlgorithm" /> class.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Security.Cryptography.SymmetricAlgorithm" /> class.</summary>
	public void Clear()
	{
		((IDisposable)this).Dispose();
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.SymmetricAlgorithm" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (KeyValue != null)
			{
				Array.Clear(KeyValue, 0, KeyValue.Length);
				KeyValue = null;
			}
			if (IVValue != null)
			{
				Array.Clear(IVValue, 0, IVValue.Length);
				IVValue = null;
			}
		}
	}

	/// <summary>Determines whether the specified key size is valid for the current algorithm.</summary>
	/// <returns>true if the specified key size is valid for the current algorithm; otherwise, false.</returns>
	/// <param name="bitLength">The length, in bits, to check for a valid key size. </param>
	public bool ValidKeySize(int bitLength)
	{
		KeySizes[] legalKeySizes = LegalKeySizes;
		if (legalKeySizes == null)
		{
			return false;
		}
		for (int i = 0; i < legalKeySizes.Length; i++)
		{
			if (legalKeySizes[i].SkipSize == 0)
			{
				if (legalKeySizes[i].MinSize == bitLength)
				{
					return true;
				}
				continue;
			}
			for (int j = legalKeySizes[i].MinSize; j <= legalKeySizes[i].MaxSize; j += legalKeySizes[i].SkipSize)
			{
				if (j == bitLength)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>Creates a default cryptographic object used to perform the symmetric algorithm.</summary>
	/// <returns>A default cryptographic object used to perform the symmetric algorithm.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static SymmetricAlgorithm Create()
	{
		return Create("System.Security.Cryptography.SymmetricAlgorithm");
	}

	/// <summary>Creates the specified cryptographic object used to perform the symmetric algorithm.</summary>
	/// <returns>A cryptographic object used to perform the symmetric algorithm.</returns>
	/// <param name="algName">The name of the specific implementation of the <see cref="T:System.Security.Cryptography.SymmetricAlgorithm" /> class to use. </param>
	public static SymmetricAlgorithm Create(string algName)
	{
		return (SymmetricAlgorithm)CryptoConfig.CreateFromName(algName);
	}

	/// <summary>Creates a symmetric encryptor object with the current <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key" /> property and initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />).</summary>
	/// <returns>A symmetric encryptor object.</returns>
	public virtual ICryptoTransform CreateEncryptor()
	{
		return CreateEncryptor(Key, IV);
	}

	/// <summary>When overridden in a derived class, creates a symmetric encryptor object with the specified <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key" /> property and initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />).</summary>
	/// <returns>A symmetric encryptor object.</returns>
	/// <param name="rgbKey">The secret key to use for the symmetric algorithm. </param>
	/// <param name="rgbIV">The initialization vector to use for the symmetric algorithm. </param>
	public abstract ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV);

	/// <summary>Creates a symmetric decryptor object with the current <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key" /> property and initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />).</summary>
	/// <returns>A symmetric decryptor object.</returns>
	public virtual ICryptoTransform CreateDecryptor()
	{
		return CreateDecryptor(Key, IV);
	}

	/// <summary>When overridden in a derived class, creates a symmetric decryptor object with the specified <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key" /> property and initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />).</summary>
	/// <returns>A symmetric decryptor object.</returns>
	/// <param name="rgbKey">The secret key to use for the symmetric algorithm. </param>
	/// <param name="rgbIV">The initialization vector to use for the symmetric algorithm. </param>
	public abstract ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV);

	/// <summary>When overridden in a derived class, generates a random key (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key" />) to use for the algorithm. </summary>
	public abstract void GenerateKey();

	/// <summary>When overridden in a derived class, generates a random initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />) to use for the algorithm.</summary>
	public abstract void GenerateIV();
}
