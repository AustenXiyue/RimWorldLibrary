using Unity;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.PublicKeyInfo" /> class represents information associated with a public key.</summary>
public sealed class PublicKeyInfo
{
	private AlgorithmIdentifier _algorithm;

	private byte[] _key;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.PublicKeyInfo.Algorithm" /> property retrieves the algorithm identifier associated with the public key.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.Pkcs.AlgorithmIdentifier" />  object that represents the algorithm.</returns>
	public AlgorithmIdentifier Algorithm => _algorithm;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.PublicKeyInfo.KeyValue" /> property retrieves the value of the encoded public component of the public key pair.</summary>
	/// <returns>An array of byte values  that represents the encoded public component of the public key pair.</returns>
	public byte[] KeyValue => _key;

	internal PublicKeyInfo(AlgorithmIdentifier algorithm, byte[] key)
	{
		_algorithm = algorithm;
		_key = key;
	}

	internal PublicKeyInfo()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
