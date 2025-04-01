using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography;

/// <summary>Verifies a Digital Signature Algorithm (<see cref="T:System.Security.Cryptography.DSA" />) PKCS#1 v1.5 signature.</summary>
[ComVisible(true)]
public class DSASignatureDeformatter : AsymmetricSignatureDeformatter
{
	private DSA _dsaKey;

	private string _oid;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.DSASignatureDeformatter" /> class.</summary>
	public DSASignatureDeformatter()
	{
		_oid = CryptoConfig.MapNameToOID("SHA1", OidGroup.HashAlgorithm);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.DSASignatureDeformatter" /> class with the specified key.</summary>
	/// <param name="key">The instance of Digital Signature Algorithm (<see cref="T:System.Security.Cryptography.DSA" />) that holds the key. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public DSASignatureDeformatter(AsymmetricAlgorithm key)
		: this()
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		_dsaKey = (DSA)key;
	}

	/// <summary>Specifies the key to be used for the Digital Signature Algorithm (<see cref="T:System.Security.Cryptography.DSA" />) signature deformatter.</summary>
	/// <param name="key">The instance of <see cref="T:System.Security.Cryptography.DSA" /> that holds the key. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public override void SetKey(AsymmetricAlgorithm key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		_dsaKey = (DSA)key;
	}

	/// <summary>Specifies the hash algorithm for the Digital Signature Algorithm (<see cref="T:System.Security.Cryptography.DSA" />) signature deformatter.</summary>
	/// <param name="strName">The name of the hash algorithm to use for the signature deformatter. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException">The <paramref name="strName" /> parameter does not map to the <see cref="T:System.Security.Cryptography.SHA1" /> hash algorithm. </exception>
	public override void SetHashAlgorithm(string strName)
	{
		if (CryptoConfig.MapNameToOID(strName, OidGroup.HashAlgorithm) != _oid)
		{
			throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("This operation is not supported for this class."));
		}
	}

	/// <summary>Verifies the Digital Signature Algorithm (<see cref="T:System.Security.Cryptography.DSA" />) signature on the data.</summary>
	/// <returns>true if the signature is valid for the data; otherwise, false.</returns>
	/// <param name="rgbHash">The data signed with <paramref name="rgbSignature" />. </param>
	/// <param name="rgbSignature">The signature to be verified for <paramref name="rgbHash" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rgbHash" /> is null.-or-<paramref name="rgbSignature" /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException">The DSA key is missing.</exception>
	public override bool VerifySignature(byte[] rgbHash, byte[] rgbSignature)
	{
		if (rgbHash == null)
		{
			throw new ArgumentNullException("rgbHash");
		}
		if (rgbSignature == null)
		{
			throw new ArgumentNullException("rgbSignature");
		}
		if (_dsaKey == null)
		{
			throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("No asymmetric key object has been associated with this formatter object."));
		}
		return _dsaKey.VerifySignature(rgbHash, rgbSignature);
	}
}
