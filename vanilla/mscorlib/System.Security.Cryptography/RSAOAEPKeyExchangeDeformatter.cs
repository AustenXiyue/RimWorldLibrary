using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Decrypts Optimal Asymmetric Encryption Padding (OAEP) key exchange data.</summary>
[ComVisible(true)]
public class RSAOAEPKeyExchangeDeformatter : AsymmetricKeyExchangeDeformatter
{
	private RSA _rsaKey;

	private bool? _rsaOverridesDecrypt;

	/// <summary>Gets the parameters for the Optimal Asymmetric Encryption Padding (OAEP) key exchange.</summary>
	/// <returns>An XML string containing the parameters of the OAEP key exchange operation.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RSAOAEPKeyExchangeDeformatter" /> class.</summary>
	public RSAOAEPKeyExchangeDeformatter()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RSAOAEPKeyExchangeDeformatter" /> class with the specified key.</summary>
	/// <param name="key">The instance of the <see cref="T:System.Security.Cryptography.RSA" /> algorithm that holds the private key. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key " />is null.</exception>
	public RSAOAEPKeyExchangeDeformatter(AsymmetricAlgorithm key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		_rsaKey = (RSA)key;
	}

	/// <summary>Extracts secret information from the encrypted key exchange data.</summary>
	/// <returns>The secret information derived from the key exchange data.</returns>
	/// <param name="rgbData">The key exchange data within which the secret information is hidden. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The key exchange data verification has failed. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException">The key is missing.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public override byte[] DecryptKeyExchange(byte[] rgbData)
	{
		if (_rsaKey == null)
		{
			throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("No asymmetric key object has been associated with this formatter object."));
		}
		if (OverridesDecrypt)
		{
			return _rsaKey.Decrypt(rgbData, RSAEncryptionPadding.OaepSHA1);
		}
		return Utils.RsaOaepDecrypt(_rsaKey, SHA1.Create(), new PKCS1MaskGenerationMethod(), rgbData);
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
