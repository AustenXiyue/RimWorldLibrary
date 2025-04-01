using System.Runtime.InteropServices;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography;

/// <summary>Verifies an <see cref="T:System.Security.Cryptography.RSA" /> PKCS #1 version 1.5 signature.</summary>
[ComVisible(true)]
public class RSAPKCS1SignatureDeformatter : AsymmetricSignatureDeformatter
{
	private RSA rsa;

	private string hashName;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RSAPKCS1SignatureDeformatter" /> class.</summary>
	public RSAPKCS1SignatureDeformatter()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RSAPKCS1SignatureDeformatter" /> class with the specified key.</summary>
	/// <param name="key">The instance of <see cref="T:System.Security.Cryptography.RSA" /> that holds the public key. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key " />is null.</exception>
	public RSAPKCS1SignatureDeformatter(AsymmetricAlgorithm key)
	{
		SetKey(key);
	}

	/// <summary>Sets the hash algorithm to use for verifying the signature.</summary>
	/// <param name="strName">The name of the hash algorithm to use for verifying the signature. </param>
	public override void SetHashAlgorithm(string strName)
	{
		if (strName == null)
		{
			throw new ArgumentNullException("strName");
		}
		hashName = strName;
	}

	/// <summary>Sets the public key to use for verifying the signature.</summary>
	/// <param name="key">The instance of <see cref="T:System.Security.Cryptography.RSA" /> that holds the public key. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key " />is null.</exception>
	public override void SetKey(AsymmetricAlgorithm key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		rsa = (RSA)key;
	}

	/// <summary>Verifies the <see cref="T:System.Security.Cryptography.RSA" /> PKCS#1 signature for the specified data.</summary>
	/// <returns>true if <paramref name="rgbSignature" /> matches the signature computed using the specified hash algorithm and key on <paramref name="rgbHash" />; otherwise, false.</returns>
	/// <param name="rgbHash">The data signed with <paramref name="rgbSignature" />. </param>
	/// <param name="rgbSignature">The signature to be verified for <paramref name="rgbHash" />. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rgbHash" /> parameter is null.-or- The <paramref name="rgbSignature" /> parameter is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override bool VerifySignature(byte[] rgbHash, byte[] rgbSignature)
	{
		if (rsa == null)
		{
			throw new CryptographicUnexpectedOperationException(Locale.GetText("No public key available."));
		}
		if (hashName == null)
		{
			throw new CryptographicUnexpectedOperationException(Locale.GetText("Missing hash algorithm."));
		}
		if (rgbHash == null)
		{
			throw new ArgumentNullException("rgbHash");
		}
		if (rgbSignature == null)
		{
			throw new ArgumentNullException("rgbSignature");
		}
		return PKCS1.Verify_v15(rsa, hashName, rgbHash, rgbSignature);
	}
}
