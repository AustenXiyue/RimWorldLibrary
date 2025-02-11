using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Creates Optimal Asymmetric Encryption Padding (OAEP) key exchange data using <see cref="T:System.Security.Cryptography.RSA" />.</summary>
[ComVisible(true)]
public class RSAOAEPKeyExchangeFormatter : AsymmetricKeyExchangeFormatter
{
	private byte[] ParameterValue;

	private RSA _rsaKey;

	private bool? _rsaOverridesEncrypt;

	private RandomNumberGenerator RngValue;

	/// <summary>Gets or sets the parameter used to create padding in the key exchange creation process.</summary>
	/// <returns>The parameter value.</returns>
	public byte[] Parameter
	{
		get
		{
			if (ParameterValue != null)
			{
				return (byte[])ParameterValue.Clone();
			}
			return null;
		}
		set
		{
			if (value != null)
			{
				ParameterValue = (byte[])value.Clone();
			}
			else
			{
				ParameterValue = null;
			}
		}
	}

	/// <summary>Gets the parameters for the Optimal Asymmetric Encryption Padding (OAEP) key exchange.</summary>
	/// <returns>An XML string containing the parameters of the OAEP key exchange operation.</returns>
	public override string Parameters => null;

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

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RSAOAEPKeyExchangeFormatter" /> class.</summary>
	public RSAOAEPKeyExchangeFormatter()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RSAOAEPKeyExchangeFormatter" /> class with the specified key.</summary>
	/// <param name="key">The instance of the <see cref="T:System.Security.Cryptography.RSA" /> algorithm that holds the public key. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key " />is null.</exception>
	public RSAOAEPKeyExchangeFormatter(AsymmetricAlgorithm key)
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
	/// <exception cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException">The key is missing.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public override byte[] CreateKeyExchange(byte[] rgbData)
	{
		if (_rsaKey == null)
		{
			throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("No asymmetric key object has been associated with this formatter object."));
		}
		if (OverridesEncrypt)
		{
			return _rsaKey.Encrypt(rgbData, RSAEncryptionPadding.OaepSHA1);
		}
		return Utils.RsaOaepEncrypt(_rsaKey, SHA1.Create(), new PKCS1MaskGenerationMethod(), RandomNumberGenerator.Create(), rgbData);
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
