using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Decrypts the PKCS #1 key exchange data.</summary>
[ComVisible(true)]
public class RSAPKCS1KeyExchangeDeformatter : AsymmetricKeyExchangeDeformatter
{
	private RSA _rsaKey;

	private bool? _rsaOverridesDecrypt;

	private RandomNumberGenerator RngValue;

	/// <summary>Gets or sets the random number generator algorithm to use in the creation of the key exchange.</summary>
	/// <returns>The instance of a random number generator algorithm to use.</returns>
	public RandomNumberGenerator RNG
	{
		get
		{
			return RngValue;
		}
		set
		{
			RngValue = value;
		}
	}

	/// <summary>Gets the parameters for the PKCS #1 key exchange.</summary>
	/// <returns>An XML string containing the parameters of the PKCS #1 key exchange operation.</returns>
	public override string Parameters
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	private bool OverridesDecrypt
	{
		get
		{
			if (!_rsaOverridesDecrypt.HasValue)
			{
				_rsaOverridesDecrypt = Utils.DoesRsaKeyOverride(_rsaKey, "Decrypt", new Type[2]
				{
					typeof(byte[]),
					typeof(RSAEncryptionPadding)
				});
			}
			return _rsaOverridesDecrypt.Value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RSAPKCS1KeyExchangeDeformatter" /> class.</summary>
	public RSAPKCS1KeyExchangeDeformatter()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RSAPKCS1KeyExchangeDeformatter" /> class with the specified key.</summary>
	/// <param name="key">The instance of the <see cref="T:System.Security.Cryptography.RSA" /> algorithm that holds the private key. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public RSAPKCS1KeyExchangeDeformatter(AsymmetricAlgorithm key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		_rsaKey = (RSA)key;
	}

	/// <summary>Extracts secret information from the encrypted key exchange data.</summary>
	/// <returns>The secret information derived from the key exchange data.</returns>
	/// <param name="rgbIn">The key exchange data within which the secret information is hidden. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException">The key is missing.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override byte[] DecryptKeyExchange(byte[] rgbIn)
	{
		if (_rsaKey == null)
		{
			throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("No asymmetric key object has been associated with this formatter object."));
		}
		byte[] array;
		if (OverridesDecrypt)
		{
			array = _rsaKey.Decrypt(rgbIn, RSAEncryptionPadding.Pkcs1);
		}
		else
		{
			byte[] array2 = _rsaKey.DecryptValue(rgbIn);
			int i;
			for (i = 2; i < array2.Length && array2[i] != 0; i++)
			{
			}
			if (i >= array2.Length)
			{
				throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Error occurred while decoding PKCS1 padding."));
			}
			i++;
			array = new byte[array2.Length - i];
			Buffer.InternalBlockCopy(array2, i, array, 0, array.Length);
		}
		return array;
	}

	/// <summary>Sets the private key to use for decrypting the secret information.</summary>
	/// <param name="key">The instance of the <see cref="T:System.Security.Cryptography.RSA" /> algorithm that holds the private key. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public override void SetKey(AsymmetricAlgorithm key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		_rsaKey = (RSA)key;
		_rsaOverridesDecrypt = null;
	}
}
