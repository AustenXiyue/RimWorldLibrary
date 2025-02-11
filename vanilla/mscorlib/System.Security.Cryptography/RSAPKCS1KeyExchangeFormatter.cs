using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Creates the PKCS#1 key exchange data using <see cref="T:System.Security.Cryptography.RSA" />.</summary>
[ComVisible(true)]
public class RSAPKCS1KeyExchangeFormatter : AsymmetricKeyExchangeFormatter
{
	private RandomNumberGenerator RngValue;

	private RSA _rsaKey;

	private bool? _rsaOverridesEncrypt;

	/// <summary>Gets the parameters for the PKCS #1 key exchange.</summary>
	/// <returns>An XML string containing the parameters of the PKCS #1 key exchange operation.</returns>
	public override string Parameters => "<enc:KeyEncryptionMethod enc:Algorithm=\"http://www.microsoft.com/xml/security/algorithm/PKCS1-v1.5-KeyEx\" xmlns:enc=\"http://www.microsoft.com/xml/security/encryption/v1.0\" />";

	/// <summary>Gets or sets the random number generator algorithm to use in the creation of the key exchange.</summary>
	/// <returns>The instance of a random number generator algorithm to use.</returns>
	public RandomNumberGenerator Rng
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

	private bool OverridesEncrypt
	{
		get
		{
			if (!_rsaOverridesEncrypt.HasValue)
			{
				_rsaOverridesEncrypt = Utils.DoesRsaKeyOverride(_rsaKey, "Encrypt", new Type[2]
				{
					typeof(byte[]),
					typeof(RSAEncryptionPadding)
				});
			}
			return _rsaOverridesEncrypt.Value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RSAPKCS1KeyExchangeFormatter" /> class.</summary>
	public RSAPKCS1KeyExchangeFormatter()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RSAPKCS1KeyExchangeFormatter" /> class with the specified key.</summary>
	/// <param name="key">The instance of the <see cref="T:System.Security.Cryptography.RSA" /> algorithm that holds the public key. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key " />is null.</exception>
	public RSAPKCS1KeyExchangeFormatter(AsymmetricAlgorithm key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		_rsaKey = (RSA)key;
	}

	/// <summary>Sets the public key to use for encrypting the key exchange data.</summary>
	/// <param name="key">The instance of the <see cref="T:System.Security.Cryptography.RSA" /> algorithm that holds the public key. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key " />is null.</exception>
	public override void SetKey(AsymmetricAlgorithm key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		_rsaKey = (RSA)key;
		_rsaOverridesEncrypt = null;
	}

	/// <summary>Creates the encrypted key exchange data from the specified input data.</summary>
	/// <returns>The encrypted key exchange data to be sent to the intended recipient.</returns>
	/// <param name="rgbData">The secret information to be passed in the key exchange. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">
	///   <paramref name="rgbData " />is too big.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException">The key is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override byte[] CreateKeyExchange(byte[] rgbData)
	{
		if (rgbData == null)
		{
			throw new ArgumentNullException("rgbData");
		}
		if (_rsaKey == null)
		{
			throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("No asymmetric key object has been associated with this formatter object."));
		}
		if (OverridesEncrypt)
		{
			return _rsaKey.Encrypt(rgbData, RSAEncryptionPadding.Pkcs1);
		}
		int num = _rsaKey.KeySize / 8;
		if (rgbData.Length + 11 > num)
		{
			throw new CryptographicException(Environment.GetResourceString("The data to be encrypted exceeds the maximum for this modulus of {0} bytes.", num - 11));
		}
		byte[] array = new byte[num];
		if (RngValue == null)
		{
			RngValue = RandomNumberGenerator.Create();
		}
		Rng.GetNonZeroBytes(array);
		array[0] = 0;
		array[1] = 2;
		array[num - rgbData.Length - 1] = 0;
		Buffer.InternalBlockCopy(rgbData, 0, array, num - rgbData.Length, rgbData.Length);
		return _rsaKey.EncryptValue(array);
	}

	/// <summary>Creates the encrypted key exchange data from the specified input data.</summary>
	/// <returns>The encrypted key exchange data to be sent to the intended recipient.</returns>
	/// <param name="rgbData">The secret information to be passed in the key exchange. </param>
	/// <param name="symAlgType">This parameter is not used in the current version. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override byte[] CreateKeyExchange(byte[] rgbData, Type symAlgType)
	{
		return CreateKeyExchange(rgbData);
	}
}
