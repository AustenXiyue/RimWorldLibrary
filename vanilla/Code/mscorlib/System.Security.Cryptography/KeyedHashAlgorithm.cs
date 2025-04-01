using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Represents the abstract class from which all implementations of keyed hash algorithms must derive. </summary>
[ComVisible(true)]
public abstract class KeyedHashAlgorithm : HashAlgorithm
{
	/// <summary>The key to use in the hash algorithm.</summary>
	protected byte[] KeyValue;

	/// <summary>Gets or sets the key to use in the hash algorithm.</summary>
	/// <returns>The key to use in the hash algorithm.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An attempt was made to change the <see cref="P:System.Security.Cryptography.KeyedHashAlgorithm.Key" /> property after hashing has begun. </exception>
	public virtual byte[] Key
	{
		get
		{
			return (byte[])KeyValue.Clone();
		}
		set
		{
			if (State != 0)
			{
				throw new CryptographicException(Environment.GetResourceString("Hash key cannot be changed after the first write to the stream."));
			}
			KeyValue = (byte[])value.Clone();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.KeyedHashAlgorithm" /> class.</summary>
	protected KeyedHashAlgorithm()
	{
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.KeyedHashAlgorithm" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (KeyValue != null)
			{
				Array.Clear(KeyValue, 0, KeyValue.Length);
			}
			KeyValue = null;
		}
		base.Dispose(disposing);
	}

	/// <summary>Creates an instance of the default implementation of a keyed hash algorithm.</summary>
	/// <returns>A new <see cref="T:System.Security.Cryptography.HMACSHA1" /> instance, unless the default settings have been changed.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public new static KeyedHashAlgorithm Create()
	{
		return Create("System.Security.Cryptography.KeyedHashAlgorithm");
	}

	/// <summary>Creates an instance of the specified implementation of a keyed hash algorithm.</summary>
	/// <returns>A new instance of the specified keyed hash algorithm.</returns>
	/// <param name="algName">The keyed hash algorithm implementation to use. The following table shows the valid values for the <paramref name="algName" /> parameter and the algorithms they map to.Parameter valueImplements System.Security.Cryptography.HMAC<see cref="T:System.Security.Cryptography.HMACSHA1" />System.Security.Cryptography.KeyedHashAlgorithm<see cref="T:System.Security.Cryptography.HMACSHA1" />HMACMD5<see cref="T:System.Security.Cryptography.HMACMD5" />System.Security.Cryptography.HMACMD5<see cref="T:System.Security.Cryptography.HMACMD5" />HMACRIPEMD160<see cref="T:System.Security.Cryptography.HMACRIPEMD160" />System.Security.Cryptography.HMACRIPEMD160<see cref="T:System.Security.Cryptography.HMACRIPEMD160" />HMACSHA1<see cref="T:System.Security.Cryptography.HMACSHA1" />System.Security.Cryptography.HMACSHA1<see cref="T:System.Security.Cryptography.HMACSHA1" />HMACSHA256<see cref="T:System.Security.Cryptography.HMACSHA256" />System.Security.Cryptography.HMACSHA256<see cref="T:System.Security.Cryptography.HMACSHA256" />HMACSHA384<see cref="T:System.Security.Cryptography.HMACSHA384" />System.Security.Cryptography.HMACSHA384<see cref="T:System.Security.Cryptography.HMACSHA384" />HMACSHA512<see cref="T:System.Security.Cryptography.HMACSHA512" />System.Security.Cryptography.HMACSHA512<see cref="T:System.Security.Cryptography.HMACSHA512" />MACTripleDES<see cref="T:System.Security.Cryptography.MACTripleDES" />System.Security.Cryptography.MACTripleDES<see cref="T:System.Security.Cryptography.MACTripleDES" /></param>
	public new static KeyedHashAlgorithm Create(string algName)
	{
		return (KeyedHashAlgorithm)CryptoConfig.CreateFromName(algName);
	}
}
