using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Defines a wrapper object to access the cryptographic service provider (CSP) implementation of the <see cref="T:System.Security.Cryptography.RC2" /> algorithm. This class cannot be inherited.</summary>
[ComVisible(true)]
public sealed class RC2CryptoServiceProvider : RC2
{
	private bool m_use40bitSalt;

	private static KeySizes[] s_legalKeySizes = new KeySizes[1]
	{
		new KeySizes(40, 128, 8)
	};

	/// <summary>Gets or sets the effective size, in bits, of the secret key used by the <see cref="T:System.Security.Cryptography.RC2" /> algorithm.</summary>
	/// <returns>The effective key size, in bits, used by the <see cref="T:System.Security.Cryptography.RC2" /> algorithm.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException">The <see cref="P:System.Security.Cryptography.RC2CryptoServiceProvider.EffectiveKeySize" /> property was set to a value other than the <see cref="F:System.Security.Cryptography.SymmetricAlgorithm.KeySizeValue" /> property. </exception>
	public override int EffectiveKeySize
	{
		get
		{
			return KeySizeValue;
		}
		set
		{
			if (value != KeySizeValue)
			{
				throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("EffectiveKeySize must be the same as KeySize in this implementation."));
			}
		}
	}

	/// <summary>Gets or sets a value that determines whether to create a key with an 11-byte-long, zero-value salt. </summary>
	/// <returns>true if the key should be created with an 11-byte-long, zero-value salt; otherwise, false. The default is false.</returns>
	[ComVisible(false)]
	public bool UseSalt
	{
		get
		{
			return m_use40bitSalt;
		}
		set
		{
			m_use40bitSalt = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RC2CryptoServiceProvider" /> class.</summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
	/// <exception cref="T:System.InvalidOperationException">A non-compliant FIPS algorithm was found.</exception>
	[SecuritySafeCritical]
	public RC2CryptoServiceProvider()
	{
		if (CryptoConfig.AllowOnlyFipsAlgorithms)
		{
			throw new InvalidOperationException(Environment.GetResourceString("This implementation is not part of the Windows Platform FIPS validated cryptographic algorithms."));
		}
		if (!Utils.HasAlgorithm(26114, 0))
		{
			throw new CryptographicException(Environment.GetResourceString("Cryptographic service provider (CSP) could not be found for this algorithm."));
		}
		LegalKeySizesValue = s_legalKeySizes;
		FeedbackSizeValue = 8;
	}

	/// <summary>Creates a symmetric <see cref="T:System.Security.Cryptography.RC2" /> encryptor object with the specified key (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key" />) and initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />).</summary>
	/// <returns>A symmetric <see cref="T:System.Security.Cryptography.RC2" /> encryptor object.</returns>
	/// <param name="rgbKey">The secret key to use for the symmetric algorithm. </param>
	/// <param name="rgbIV">The initialization vector to use for the symmetric algorithm. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An <see cref="F:System.Security.Cryptography.CipherMode.OFB" /> cipher mode was used.-or-A <see cref="F:System.Security.Cryptography.CipherMode.CFB" /> cipher mode with a feedback size other than 8 bits was used.-or-An invalid key size was used.-or-The algorithm key size was not available.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
	{
		if (m_use40bitSalt)
		{
			throw new NotImplementedException("UseSalt=true is not implemented on Mono yet");
		}
		return new RC2Transform(this, encryption: true, rgbKey, rgbIV);
	}

	/// <summary>Creates a symmetric <see cref="T:System.Security.Cryptography.RC2" /> decryptor object with the specified key (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key" />)and initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />).</summary>
	/// <returns>A symmetric <see cref="T:System.Security.Cryptography.RC2" /> decryptor object.</returns>
	/// <param name="rgbKey">The secret key to use for the symmetric algorithm. </param>
	/// <param name="rgbIV">The initialization vector to use for the symmetric algorithm. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An <see cref="F:System.Security.Cryptography.CipherMode.OFB" /> cipher mode was used.-or-A <see cref="F:System.Security.Cryptography.CipherMode.CFB" /> cipher mode with a feedback size other than 8 bits was used.-or-An invalid key size was used.-or-The algorithm key size was not available.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
	{
		if (m_use40bitSalt)
		{
			throw new NotImplementedException("UseSalt=true is not implemented on Mono yet");
		}
		return new RC2Transform(this, encryption: false, rgbKey, rgbIV);
	}

	/// <summary>Generates a random key (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key" />) to be used for the algorithm.</summary>
	public override void GenerateKey()
	{
		KeyValue = new byte[KeySizeValue / 8];
		Utils.StaticRandomNumberGenerator.GetBytes(KeyValue);
	}

	/// <summary>Generates a random initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />) to use for the algorithm.</summary>
	public override void GenerateIV()
	{
		IVValue = new byte[8];
		Utils.StaticRandomNumberGenerator.GetBytes(IVValue);
	}
}
