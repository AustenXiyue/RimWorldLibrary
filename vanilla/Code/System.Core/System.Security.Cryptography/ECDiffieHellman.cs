using System.Security.Permissions;

namespace System.Security.Cryptography;

/// <summary>Provides an abstract base class that Elliptic Curve Diffie-Hellman (ECDH) algorithm implementations can derive from. This class provides the basic set of operations that all ECDH implementations must support.</summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public abstract class ECDiffieHellman : AsymmetricAlgorithm
{
	/// <summary>Gets the name of the key exchange algorithm.</summary>
	/// <returns>The name of the key exchange algorithm. </returns>
	public override string KeyExchangeAlgorithm => "ECDiffieHellman";

	/// <summary>Gets the name of the signature algorithm.</summary>
	/// <returns>Always null.</returns>
	public override string SignatureAlgorithm => null;

	/// <summary>Gets the public key that is being used by the current Elliptic Curve Diffie-Hellman (ECDH) instance.</summary>
	/// <returns>The public part of the ECDH key pair that is being used by this <see cref="T:System.Security.Cryptography.ECDiffieHellman" /> instance.</returns>
	public abstract ECDiffieHellmanPublicKey PublicKey { get; }

	/// <summary>Creates a new instance of the default implementation of the Elliptic Curve Diffie-Hellman (ECDH) algorithm.</summary>
	/// <returns>A new instance of the default implementation of this class.</returns>
	public new static ECDiffieHellman Create()
	{
		throw new NotImplementedException();
	}

	/// <summary>Creates a new instance of the specified implementation of the Elliptic Curve Diffie-Hellman (ECDH) algorithm.</summary>
	/// <returns>A new instance of the specified implementation of this class. If the specified algorithm name does not map to an ECDH implementation, this method returns null.</returns>
	/// <param name="algorithm">The name of an implementation of the ECDH algorithm. The following strings all refer to the same implementation, which is the only implementation currently supported in the .NET Framework:- "ECDH"- "ECDiffieHellman"- "ECDiffieHellmanCng"- "System.Security.Cryptography.ECDiffieHellmanCng"You can also provide the name of a custom ECDH implementation.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="algorithm" /> parameter is null. </exception>
	public new static ECDiffieHellman Create(string algorithm)
	{
		if (algorithm == null)
		{
			throw new ArgumentNullException("algorithm");
		}
		return CryptoConfig.CreateFromName(algorithm) as ECDiffieHellman;
	}

	public static ECDiffieHellman Create(ECCurve curve)
	{
		ECDiffieHellman eCDiffieHellman = Create();
		if (eCDiffieHellman != null)
		{
			try
			{
				eCDiffieHellman.GenerateKey(curve);
			}
			catch
			{
				eCDiffieHellman.Dispose();
				throw;
			}
		}
		return eCDiffieHellman;
	}

	public static ECDiffieHellman Create(ECParameters parameters)
	{
		ECDiffieHellman eCDiffieHellman = Create();
		if (eCDiffieHellman != null)
		{
			try
			{
				eCDiffieHellman.ImportParameters(parameters);
			}
			catch
			{
				eCDiffieHellman.Dispose();
				throw;
			}
		}
		return eCDiffieHellman;
	}

	/// <summary>Derives bytes that can be used as a key, given another party's public key.</summary>
	/// <returns>The key material from the key exchange with the other party’s public key.</returns>
	/// <param name="otherPartyPublicKey">The other party's public key.</param>
	public virtual byte[] DeriveKeyMaterial(ECDiffieHellmanPublicKey otherPartyPublicKey)
	{
		throw DerivedClassMustOverride();
	}

	public byte[] DeriveKeyFromHash(ECDiffieHellmanPublicKey otherPartyPublicKey, HashAlgorithmName hashAlgorithm)
	{
		return DeriveKeyFromHash(otherPartyPublicKey, hashAlgorithm, null, null);
	}

	public virtual byte[] DeriveKeyFromHash(ECDiffieHellmanPublicKey otherPartyPublicKey, HashAlgorithmName hashAlgorithm, byte[] secretPrepend, byte[] secretAppend)
	{
		throw DerivedClassMustOverride();
	}

	public byte[] DeriveKeyFromHmac(ECDiffieHellmanPublicKey otherPartyPublicKey, HashAlgorithmName hashAlgorithm, byte[] hmacKey)
	{
		return DeriveKeyFromHmac(otherPartyPublicKey, hashAlgorithm, hmacKey, null, null);
	}

	public virtual byte[] DeriveKeyFromHmac(ECDiffieHellmanPublicKey otherPartyPublicKey, HashAlgorithmName hashAlgorithm, byte[] hmacKey, byte[] secretPrepend, byte[] secretAppend)
	{
		throw DerivedClassMustOverride();
	}

	public virtual byte[] DeriveKeyTls(ECDiffieHellmanPublicKey otherPartyPublicKey, byte[] prfLabel, byte[] prfSeed)
	{
		throw DerivedClassMustOverride();
	}

	private static Exception DerivedClassMustOverride()
	{
		return new NotImplementedException(global::SR.GetString("Method not supported. Derived class must override."));
	}

	public virtual ECParameters ExportParameters(bool includePrivateParameters)
	{
		throw DerivedClassMustOverride();
	}

	public virtual ECParameters ExportExplicitParameters(bool includePrivateParameters)
	{
		throw DerivedClassMustOverride();
	}

	public virtual void ImportParameters(ECParameters parameters)
	{
		throw DerivedClassMustOverride();
	}

	public virtual void GenerateKey(ECCurve curve)
	{
		throw new NotSupportedException(global::SR.GetString("Method not supported. Derived class must override."));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.ECDiffieHellman" /> class.</summary>
	protected ECDiffieHellman()
	{
	}
}
