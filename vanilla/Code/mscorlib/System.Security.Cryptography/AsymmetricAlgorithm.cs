using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Represents the abstract base class from which all implementations of asymmetric algorithms must inherit.</summary>
[ComVisible(true)]
public abstract class AsymmetricAlgorithm : IDisposable
{
	/// <summary>Represents the size, in bits, of the key modulus used by the asymmetric algorithm.</summary>
	protected int KeySizeValue;

	/// <summary>Specifies the key sizes that are supported by the asymmetric algorithm.</summary>
	protected KeySizes[] LegalKeySizesValue;

	/// <summary>Gets or sets the size, in bits, of the key modulus used by the asymmetric algorithm.</summary>
	/// <returns>The size, in bits, of the key modulus used by the asymmetric algorithm.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The key modulus size is invalid. </exception>
	public virtual int KeySize
	{
		get
		{
			return KeySizeValue;
		}
		set
		{
			for (int i = 0; i < LegalKeySizesValue.Length; i++)
			{
				if (LegalKeySizesValue[i].SkipSize == 0)
				{
					if (LegalKeySizesValue[i].MinSize == value)
					{
						KeySizeValue = value;
						return;
					}
					continue;
				}
				for (int j = LegalKeySizesValue[i].MinSize; j <= LegalKeySizesValue[i].MaxSize; j += LegalKeySizesValue[i].SkipSize)
				{
					if (j == value)
					{
						KeySizeValue = value;
						return;
					}
				}
			}
			throw new CryptographicException(Environment.GetResourceString("Specified key is not a valid size for this algorithm."));
		}
	}

	/// <summary>Gets the key sizes that are supported by the asymmetric algorithm.</summary>
	/// <returns>An array that contains the key sizes supported by the asymmetric algorithm.</returns>
	public virtual KeySizes[] LegalKeySizes => (KeySizes[])LegalKeySizesValue.Clone();

	/// <summary>Gets the name of the signature algorithm.</summary>
	/// <returns>The name of the signature algorithm.</returns>
	public virtual string SignatureAlgorithm
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>When overridden in a derived class, gets the name of the key exchange algorithm.</summary>
	/// <returns>The name of the key exchange algorithm.</returns>
	public virtual string KeyExchangeAlgorithm
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" /> class.</summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The implementation of the derived class is not valid. </exception>
	protected AsymmetricAlgorithm()
	{
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" /> class.</summary>
	public void Dispose()
	{
		Clear();
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" /> class.</summary>
	public void Clear()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" /> class and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
	}

	/// <summary>Creates a default cryptographic object used to perform the asymmetric algorithm.</summary>
	/// <returns>A new <see cref="T:System.Security.Cryptography.RSACryptoServiceProvider" /> instance, unless the default settings have been changed with the &lt;cryptoClass&gt; element.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static AsymmetricAlgorithm Create()
	{
		return Create("System.Security.Cryptography.AsymmetricAlgorithm");
	}

	/// <summary>Creates an instance of the specified implementation of an asymmetric algorithm.</summary>
	/// <returns>A new instance of the specified asymmetric algorithm implementation.</returns>
	/// <param name="algName">The asymmetric algorithm implementation to use. The following table shows the valid values for the <paramref name="algName" /> parameter and the algorithms they map to.Parameter valueImplements System.Security.Cryptography.AsymmetricAlgorithm<see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" />RSA<see cref="T:System.Security.Cryptography.RSA" />System.Security.Cryptography.RSA<see cref="T:System.Security.Cryptography.RSA" />DSA<see cref="T:System.Security.Cryptography.DSA" />System.Security.Cryptography.DSA<see cref="T:System.Security.Cryptography.DSA" />ECDsa<see cref="T:System.Security.Cryptography.ECDsa" />ECDsaCng<see cref="T:System.Security.Cryptography.ECDsaCng" />System.Security.Cryptography.ECDsaCng<see cref="T:System.Security.Cryptography.ECDsaCng" />ECDH<see cref="T:System.Security.Cryptography.ECDiffieHellman" />ECDiffieHellman<see cref="T:System.Security.Cryptography.ECDiffieHellman" />ECDiffieHellmanCng<see cref="T:System.Security.Cryptography.ECDiffieHellmanCng" />System.Security.Cryptography.ECDiffieHellmanCng<see cref="T:System.Security.Cryptography.ECDiffieHellmanCng" /></param>
	public static AsymmetricAlgorithm Create(string algName)
	{
		return (AsymmetricAlgorithm)CryptoConfig.CreateFromName(algName);
	}

	/// <summary>When overridden in a derived class, reconstructs an <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" /> object from an XML string.</summary>
	/// <param name="xmlString">The XML string to use to reconstruct the <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" /> object. </param>
	public virtual void FromXmlString(string xmlString)
	{
		throw new NotImplementedException();
	}

	/// <summary>When overridden in a derived class, creates and returns an XML string representation of the current <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" /> object.</summary>
	/// <returns>An XML string encoding of the current <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" /> object.</returns>
	/// <param name="includePrivateParameters">true to include private parameters; otherwise, false. </param>
	public virtual string ToXmlString(bool includePrivateParameters)
	{
		throw new NotImplementedException();
	}
}
