using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Defines a wrapper object to access the cryptographic service provider (CSP) version of the <see cref="T:System.Security.Cryptography.TripleDES" /> algorithm. This class cannot be inherited.</summary>
[ComVisible(true)]
public sealed class TripleDESCryptoServiceProvider : TripleDES
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.TripleDESCryptoServiceProvider" /> class.</summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="T:System.Security.Cryptography.TripleDES" /> cryptographic service provider is not available. </exception>
	[SecuritySafeCritical]
	public TripleDESCryptoServiceProvider()
	{
		if (!Utils.HasAlgorithm(26115, 0))
		{
			throw new CryptographicException(Environment.GetResourceString("Cryptographic service provider (CSP) could not be found for this algorithm."));
		}
		FeedbackSizeValue = 8;
	}

	/// <summary>Creates a symmetric <see cref="T:System.Security.Cryptography.TripleDES" /> encryptor object with the specified key (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key" />) and initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />).</summary>
	/// <returns>A symmetric <see cref="T:System.Security.Cryptography.TripleDES" /> encryptor object.</returns>
	/// <param name="rgbKey">The secret key to use for the symmetric algorithm. </param>
	/// <param name="rgbIV">The initialization vector to use for the symmetric algorithm. NoteThe initialization vector must be 8 bytes long. If it is longer than 8 bytes, it is truncated and an exception is not thrown. Before you call <see cref="M:System.Security.Cryptography.TripleDESCryptoServiceProvider.CreateEncryptor(System.Byte[],System.Byte[])" />, check the length of the initialization vector and throw an exception if it is too long.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The value of the <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Mode" /> property is <see cref="F:System.Security.Cryptography.CipherMode.OFB" />.-or-The value of the <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Mode" /> property is <see cref="F:System.Security.Cryptography.CipherMode.CFB" /> and the value of the <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.FeedbackSize" /> property is not 8.-or-An invalid key size was used.-or-The algorithm key size was not available.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
	{
		if (TripleDES.IsWeakKey(rgbKey))
		{
			throw new CryptographicException(Environment.GetResourceString("Specified key is a known weak key for '{0}' and cannot be used."), "TripleDES");
		}
		return new TripleDESTransform(this, encryption: true, rgbKey, rgbIV);
	}

	/// <summary>Creates a symmetric <see cref="T:System.Security.Cryptography.TripleDES" /> decryptor object with the specified key (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key" />) and initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />).</summary>
	/// <returns>A symmetric <see cref="T:System.Security.Cryptography.TripleDES" /> decryptor object.</returns>
	/// <param name="rgbKey">The secret key to use for the symmetric algorithm. </param>
	/// <param name="rgbIV">The initialization vector to use for the symmetric algorithm. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The value of the <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Mode" /> property is <see cref="F:System.Security.Cryptography.CipherMode.OFB" />.-or-The value of the <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Mode" /> property is <see cref="F:System.Security.Cryptography.CipherMode.CFB" /> and the value of the <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.FeedbackSize" /> property is not 8.-or-An invalid key size was used.-or-The algorithm key size was not available.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
	{
		if (TripleDES.IsWeakKey(rgbKey))
		{
			throw new CryptographicException(Environment.GetResourceString("Specified key is a known weak key for '{0}' and cannot be used."), "TripleDES");
		}
		return new TripleDESTransform(this, encryption: false, rgbKey, rgbIV);
	}

	/// <summary>Generates a random <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key" /> to be used for the algorithm.</summary>
	public override void GenerateKey()
	{
		KeyValue = new byte[KeySizeValue / 8];
		Utils.StaticRandomNumberGenerator.GetBytes(KeyValue);
		while (TripleDES.IsWeakKey(KeyValue))
		{
			Utils.StaticRandomNumberGenerator.GetBytes(KeyValue);
		}
	}

	/// <summary>Generates a random initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV" />) to use for the algorithm.</summary>
	public override void GenerateIV()
	{
		IVValue = new byte[8];
		Utils.StaticRandomNumberGenerator.GetBytes(IVValue);
	}
}
