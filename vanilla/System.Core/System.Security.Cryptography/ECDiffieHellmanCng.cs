using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using Unity;

namespace System.Security.Cryptography;

/// <summary>Provides a Cryptography Next Generation (CNG) implementation of the Elliptic Curve Diffie-Hellman (ECDH) algorithm. This class is used to perform cryptographic operations.</summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class ECDiffieHellmanCng : ECDiffieHellman
{
	/// <summary>Gets or sets the hash algorithm to use when generating key material.</summary>
	/// <returns>An object that specifies the hash algorithm.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value is null.</exception>
	public CngAlgorithm HashAlgorithm
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets or sets the Hash-based Message Authentication Code (HMAC) key to use when deriving key material.</summary>
	/// <returns>The Hash-based Message Authentication Code (HMAC) key to use when deriving key material.</returns>
	public byte[] HmacKey
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Specifies the <see cref="T:System.Security.Cryptography.CngKey" /> that is used by the current object for cryptographic operations.</summary>
	/// <returns>The key pair used by this object to perform cryptographic operations.</returns>
	public CngKey Key
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets or sets the key derivation function for the <see cref="T:System.Security.Cryptography.ECDiffieHellmanCng" /> class.</summary>
	/// <returns>One of the <see cref="T:System.Security.Cryptography.ECDiffieHellmanKeyDerivationFunction" /> enumeration values: <see cref="F:System.Security.Cryptography.ECDiffieHellmanKeyDerivationFunction.Hash" />, <see cref="F:System.Security.Cryptography.ECDiffieHellmanKeyDerivationFunction.Hmac" />, or <see cref="F:System.Security.Cryptography.ECDiffieHellmanKeyDerivationFunction.Tls" />. The default value is <see cref="F:System.Security.Cryptography.ECDiffieHellmanKeyDerivationFunction.Hash" />.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The enumeration value is out of range.</exception>
	public ECDiffieHellmanKeyDerivationFunction KeyDerivationFunction
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(ECDiffieHellmanKeyDerivationFunction);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets or sets the label value that is used for key derivation.</summary>
	/// <returns>The label value.</returns>
	public byte[] Label
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets the public key that can be used by another <see cref="T:System.Security.Cryptography.ECDiffieHellmanCng" /> object to generate a shared secret agreement.</summary>
	/// <returns>The public key that is associated with this instance of the <see cref="T:System.Security.Cryptography.ECDiffieHellmanCng" /> object.</returns>
	public override ECDiffieHellmanPublicKey PublicKey
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets or sets a value that will be appended to the secret agreement when generating key material.</summary>
	/// <returns>The value that is appended to the secret agreement.</returns>
	public byte[] SecretAppend
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets or sets a value that will be added to the beginning of the secret agreement when deriving key material.</summary>
	/// <returns>The value that is appended to the beginning of the secret agreement during key derivation.</returns>
	public byte[] SecretPrepend
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets or sets the seed value that will be used when deriving key material.</summary>
	/// <returns>The seed value.</returns>
	public byte[] Seed
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets a value that indicates whether the secret agreement is used as a Hash-based Message Authentication Code (HMAC) key to derive key material.</summary>
	/// <returns>true if the secret agreement is used as an HMAC key to derive key material; otherwise, false.</returns>
	public bool UseSecretAgreementAsHmacKey
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.ECDiffieHellmanCng" /> class with a random key pair.</summary>
	public ECDiffieHellmanCng()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.ECDiffieHellmanCng" /> class with a random key pair, using the specified key size.</summary>
	/// <param name="keySize">The size of the key. Valid key sizes are 256, 384, and 521 bits.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="keySize" /> specifies an invalid length.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">Cryptography Next Generation (CNG) classes are not supported on this system.</exception>
	public ECDiffieHellmanCng(int keySize)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.ECDiffieHellmanCng" /> class by using the specified <see cref="T:System.Security.Cryptography.CngKey" /> object.</summary>
	/// <param name="key">The key that will be used as input to the cryptographic operations performed by the current object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="key" /> does not specify an Elliptic Curve Diffie-Hellman (ECDH) algorithm group.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">Cryptography Next Generation (CNG) classes are not supported on this system.</exception>
	[SecuritySafeCritical]
	public ECDiffieHellmanCng(CngKey key)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	public ECDiffieHellmanCng(ECCurve curve)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Derives the key material that is generated from the secret agreement between two parties, given a <see cref="T:System.Security.Cryptography.CngKey" /> object that contains the second party's public key. </summary>
	/// <returns>A byte array that contains the key material. This information is generated from the secret agreement that is calculated from the current object's private key and the specified public key.</returns>
	/// <param name="otherPartyPublicKey">An object that contains the public part of the Elliptic Curve Diffie-Hellman (ECDH) key from the other party in the key exchange.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="otherPartyPublicKey" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="otherPartyPublicKey" /> is invalid. Either its <see cref="P:System.Security.Cryptography.CngKey.AlgorithmGroup" /> property does not specify <see cref="P:System.Security.Cryptography.CngAlgorithmGroup.ECDiffieHellman" /> or its key size does not match the key size of this instance.</exception>
	/// <exception cref="T:System.InvalidOperationException">This object's <see cref="P:System.Security.Cryptography.ECDiffieHellmanCng.KeyDerivationFunction" /> property specifies the <see cref="F:System.Security.Cryptography.ECDiffieHellmanKeyDerivationFunction.Tls" /> key derivation function, but either <see cref="P:System.Security.Cryptography.ECDiffieHellmanCng.Label" /> or <see cref="P:System.Security.Cryptography.ECDiffieHellmanCng.Seed" /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">All other errors.</exception>
	[SecuritySafeCritical]
	public byte[] DeriveKeyMaterial(CngKey otherPartyPublicKey)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Gets a handle to the secret agreement generated between two parties, given a <see cref="T:System.Security.Cryptography.CngKey" /> object that contains the second party's public key.</summary>
	/// <returns>A handle to the secret agreement. This information is calculated from the current object's private key and the specified public key.</returns>
	/// <param name="otherPartyPublicKey">An object that contains the public part of the Elliptic Curve Diffie-Hellman (ECDH) key from the other party in the key exchange.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="otherPartyPublicKey" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="otherPartyPublicKey" /> is not an ECDH key, or it is not the correct size.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">All other errors.</exception>
	[SecurityCritical]
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	public SafeNCryptSecretHandle DeriveSecretAgreementHandle(CngKey otherPartyPublicKey)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Gets a handle to the secret agreement generated between two parties, given an <see cref="T:System.Security.Cryptography.ECDiffieHellmanPublicKey" /> object that contains the second party's public key.</summary>
	/// <returns>A handle to the secret agreement. This information is calculated from the current object's private key and the specified public key.</returns>
	/// <param name="otherPartyPublicKey">The public key from the other party in the key exchange.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="otherPartyPublicKey" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="otherPartyPublicKey" /> is not an <see cref="T:System.Security.Cryptography.ECDiffieHellmanPublicKey" /> key. </exception>
	public SafeNCryptSecretHandle DeriveSecretAgreementHandle(ECDiffieHellmanPublicKey otherPartyPublicKey)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Deserializes the key information from an XML string by using the specified format.</summary>
	/// <param name="xml">The XML-based key information to be deserialized.</param>
	/// <param name="format">One of the enumeration values that specifies the format of the XML string. The only currently accepted format is <see cref="F:System.Security.Cryptography.ECKeyXmlFormat.Rfc4050" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xml" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="xml" /> is malformed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="format" /> specifies an invalid format. The only accepted value is <see cref="F:System.Security.Cryptography.ECKeyXmlFormat.Rfc4050" />.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">All other errors.</exception>
	public void FromXmlString(string xml, ECKeyXmlFormat format)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Serializes the key information to an XML string by using the specified format.</summary>
	/// <returns>A string object that contains the key information, serialized to an XML string, according to the requested format.</returns>
	/// <param name="format">One of the enumeration values that specifies the format of the XML string. The only currently accepted format is <see cref="F:System.Security.Cryptography.ECKeyXmlFormat.Rfc4050" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="format" /> specifies an invalid format. The only accepted value is <see cref="F:System.Security.Cryptography.ECKeyXmlFormat.Rfc4050" />.</exception>
	public string ToXmlString(ECKeyXmlFormat format)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}
}
