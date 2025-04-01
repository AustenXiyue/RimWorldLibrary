using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography;

/// <summary>Creates a Digital Signature Algorithm (<see cref="T:System.Security.Cryptography.DSA" />) signature.</summary>
[ComVisible(true)]
public class DSASignatureFormatter : AsymmetricSignatureFormatter
{
	private DSA _dsaKey;

	private string _oid;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.DSASignatureFormatter" /> class.</summary>
	public DSASignatureFormatter()
	{
		_oid = CryptoConfig.MapNameToOID("SHA1", OidGroup.HashAlgorithm);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.DSASignatureFormatter" /> class with the specified key.</summary>
	/// <param name="key">The instance of the Digital Signature Algorithm (<see cref="T:System.Security.Cryptography.DSA" />) that holds the key. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public DSASignatureFormatter(AsymmetricAlgorithm key)
		: this()
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		_dsaKey = (DSA)key;
	}

	/// <summary>Specifies the key to be used for the Digital Signature Algorithm (<see cref="T:System.Security.Cryptography.DSA" />) signature formatter.</summary>
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

	/// <summary>Specifies the hash algorithm for the Digital Signature Algorithm (<see cref="T:System.Security.Cryptography.DSA" />) signature formatter.</summary>
	/// <param name="strName">The name of the hash algorithm to use for the signature formatter. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException">The <paramref name="strName" /> parameter does not map to the <see cref="T:System.Security.Cryptography.SHA1" /> hash algorithm. </exception>
	public override void SetHashAlgorithm(string strName)
	{
		if (CryptoConfig.MapNameToOID(strName, OidGroup.HashAlgorithm) != _oid)
		{
			throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("This operation is not supported for this class."));
		}
	}

	/// <summary>Creates the Digital Signature Algorithm (<see cref="T:System.Security.Cryptography.DSA" />) PKCS #1 signature for the specified data.</summary>
	/// <returns>The digital signature for the specified data.</returns>
	/// <param name="rgbHash">The data to be signed. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rgbHash" /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException">The OID is null.-or-The DSA key is null.</exception>
	public override byte[] CreateSignature(byte[] rgbHash)
	{
		if (rgbHash == null)
		{
			throw new ArgumentNullException("rgbHash");
		}
		if (_oid == null)
		{
			throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Required object identifier (OID) cannot be found."));
		}
		if (_dsaKey == null)
		{
			throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("No asymmetric key object has been associated with this formatter object."));
		}
		return _dsaKey.CreateSignature(rgbHash);
	}
}
