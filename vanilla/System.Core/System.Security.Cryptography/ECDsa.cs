using System.IO;
using System.Security.Permissions;

namespace System.Security.Cryptography;

/// <summary>Provides an abstract base class that encapsulates the Elliptic Curve Digital Signature Algorithm (ECDSA).</summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public abstract class ECDsa : AsymmetricAlgorithm
{
	/// <summary>Gets the name of the key exchange algorithm.</summary>
	/// <returns>Always null.</returns>
	public override string KeyExchangeAlgorithm => null;

	/// <summary>Gets the name of the signature algorithm.</summary>
	/// <returns>The string "ECDsa".</returns>
	public override string SignatureAlgorithm => "ECDsa";

	/// <summary>Creates a new instance of the default implementation of the Elliptic Curve Digital Signature Algorithm (ECDSA).</summary>
	/// <returns>A new instance of the default implementation (<see cref="T:System.Security.Cryptography.ECDsaCng" />) of this class.</returns>
	public new static ECDsa Create()
	{
		throw new NotImplementedException();
	}

	/// <summary>Creates a new instance of the specified implementation of the Elliptic Curve Digital Signature Algorithm (ECDSA).</summary>
	/// <returns>A new instance of the specified implementation of this class. If the specified algorithm name does not map to an ECDSA implementation, this method returns null. </returns>
	/// <param name="algorithm">The name of an ECDSA implementation. The following strings all refer to the same implementation, which is the only implementation currently supported in the .NET Framework:- "ECDsa"- "ECDsaCng"- "System.Security.Cryptography.ECDsaCng"You can also provide the name of a custom ECDSA implementation.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="algorithm" /> parameter is null.</exception>
	public new static ECDsa Create(string algorithm)
	{
		if (algorithm == null)
		{
			throw new ArgumentNullException("algorithm");
		}
		return CryptoConfig.CreateFromName(algorithm) as ECDsa;
	}

	public static ECDsa Create(ECCurve curve)
	{
		ECDsa eCDsa = Create();
		if (eCDsa != null)
		{
			try
			{
				eCDsa.GenerateKey(curve);
			}
			catch
			{
				eCDsa.Dispose();
				throw;
			}
		}
		return eCDsa;
	}

	public static ECDsa Create(ECParameters parameters)
	{
		ECDsa eCDsa = Create();
		if (eCDsa != null)
		{
			try
			{
				eCDsa.ImportParameters(parameters);
			}
			catch
			{
				eCDsa.Dispose();
				throw;
			}
		}
		return eCDsa;
	}

	/// <summary>Generates a digital signature for the specified hash value. </summary>
	/// <returns>A digital signature that consists of the given hash value encrypted with the private key.</returns>
	/// <param name="hash">The hash value of the data that is being signed.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="hash" /> parameter is null.</exception>
	public abstract byte[] SignHash(byte[] hash);

	/// <summary>Verifies a digital signature against the specified hash value. </summary>
	/// <returns>true if the hash value equals the decrypted signature; otherwise, false.</returns>
	/// <param name="hash">The hash value of a block of data.</param>
	/// <param name="signature">The digital signature to be verified.</param>
	public abstract bool VerifyHash(byte[] hash, byte[] signature);

	protected virtual byte[] HashData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm)
	{
		throw DerivedClassMustOverride();
	}

	protected virtual byte[] HashData(Stream data, HashAlgorithmName hashAlgorithm)
	{
		throw DerivedClassMustOverride();
	}

	public virtual byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return SignData(data, 0, data.Length, hashAlgorithm);
	}

	public virtual byte[] SignData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (offset < 0 || offset > data.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (count < 0 || count > data.Length - offset)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (string.IsNullOrEmpty(hashAlgorithm.Name))
		{
			throw HashAlgorithmNameNullOrEmpty();
		}
		byte[] hash = HashData(data, offset, count, hashAlgorithm);
		return SignHash(hash);
	}

	public virtual byte[] SignData(Stream data, HashAlgorithmName hashAlgorithm)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (string.IsNullOrEmpty(hashAlgorithm.Name))
		{
			throw HashAlgorithmNameNullOrEmpty();
		}
		byte[] hash = HashData(data, hashAlgorithm);
		return SignHash(hash);
	}

	public bool VerifyData(byte[] data, byte[] signature, HashAlgorithmName hashAlgorithm)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return VerifyData(data, 0, data.Length, signature, hashAlgorithm);
	}

	public virtual bool VerifyData(byte[] data, int offset, int count, byte[] signature, HashAlgorithmName hashAlgorithm)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (offset < 0 || offset > data.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (count < 0 || count > data.Length - offset)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (signature == null)
		{
			throw new ArgumentNullException("signature");
		}
		if (string.IsNullOrEmpty(hashAlgorithm.Name))
		{
			throw HashAlgorithmNameNullOrEmpty();
		}
		byte[] hash = HashData(data, offset, count, hashAlgorithm);
		return VerifyHash(hash, signature);
	}

	public bool VerifyData(Stream data, byte[] signature, HashAlgorithmName hashAlgorithm)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (signature == null)
		{
			throw new ArgumentNullException("signature");
		}
		if (string.IsNullOrEmpty(hashAlgorithm.Name))
		{
			throw HashAlgorithmNameNullOrEmpty();
		}
		byte[] hash = HashData(data, hashAlgorithm);
		return VerifyHash(hash, signature);
	}

	public virtual ECParameters ExportParameters(bool includePrivateParameters)
	{
		throw new NotSupportedException(global::SR.GetString("Method not supported. Derived class must override."));
	}

	public virtual ECParameters ExportExplicitParameters(bool includePrivateParameters)
	{
		throw new NotSupportedException(global::SR.GetString("Method not supported. Derived class must override."));
	}

	public virtual void ImportParameters(ECParameters parameters)
	{
		throw new NotSupportedException(global::SR.GetString("Method not supported. Derived class must override."));
	}

	public virtual void GenerateKey(ECCurve curve)
	{
		throw new NotSupportedException(global::SR.GetString("Method not supported. Derived class must override."));
	}

	private static Exception DerivedClassMustOverride()
	{
		return new NotImplementedException(global::SR.GetString("Method not supported. Derived class must override."));
	}

	internal static Exception HashAlgorithmNameNullOrEmpty()
	{
		return new ArgumentException(global::SR.GetString("The hash algorithm name cannot be null or empty."), "hashAlgorithm");
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.ECDsa" /> class.</summary>
	protected ECDsa()
	{
	}
}
