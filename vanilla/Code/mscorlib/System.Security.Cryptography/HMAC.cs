using System.Runtime.InteropServices;

namespace System.Security.Cryptography;

/// <summary>Represents the abstract class from which all implementations of Hash-based Message Authentication Code (HMAC) must derive.</summary>
[ComVisible(true)]
public abstract class HMAC : KeyedHashAlgorithm
{
	private int blockSizeValue = 64;

	internal string m_hashName;

	internal HashAlgorithm m_hash1;

	internal HashAlgorithm m_hash2;

	private byte[] m_inner;

	private byte[] m_outer;

	private bool m_hashing;

	/// <summary>Gets or sets the block size to use in the hash value.</summary>
	/// <returns>The block size to use in the hash value.</returns>
	protected int BlockSizeValue
	{
		get
		{
			return blockSizeValue;
		}
		set
		{
			blockSizeValue = value;
		}
	}

	/// <summary>Gets or sets the key to use in the hash algorithm.</summary>
	/// <returns>The key to use in the hash algorithm.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An attempt is made to change the <see cref="P:System.Security.Cryptography.HMAC.Key" /> property after hashing has begun. </exception>
	public override byte[] Key
	{
		get
		{
			return (byte[])KeyValue.Clone();
		}
		set
		{
			if (m_hashing)
			{
				throw new CryptographicException(Environment.GetResourceString("Hash key cannot be changed after the first write to the stream."));
			}
			InitializeKey(value);
		}
	}

	/// <summary>Gets or sets the name of the hash algorithm to use for hashing.</summary>
	/// <returns>The name of the hash algorithm.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The current hash algorithm cannot be changed.</exception>
	public string HashName
	{
		get
		{
			return m_hashName;
		}
		set
		{
			if (m_hashing)
			{
				throw new CryptographicException(Environment.GetResourceString("Hash name cannot be changed after the first write to the stream."));
			}
			m_hashName = value;
			m_hash1 = HashAlgorithm.Create(m_hashName);
			m_hash2 = HashAlgorithm.Create(m_hashName);
		}
	}

	private void UpdateIOPadBuffers()
	{
		if (m_inner == null)
		{
			m_inner = new byte[BlockSizeValue];
		}
		if (m_outer == null)
		{
			m_outer = new byte[BlockSizeValue];
		}
		for (int i = 0; i < BlockSizeValue; i++)
		{
			m_inner[i] = 54;
			m_outer[i] = 92;
		}
		for (int i = 0; i < KeyValue.Length; i++)
		{
			m_inner[i] ^= KeyValue[i];
			m_outer[i] ^= KeyValue[i];
		}
	}

	internal void InitializeKey(byte[] key)
	{
		m_inner = null;
		m_outer = null;
		if (key.Length > BlockSizeValue)
		{
			KeyValue = m_hash1.ComputeHash(key);
		}
		else
		{
			KeyValue = (byte[])key.Clone();
		}
		UpdateIOPadBuffers();
	}

	/// <summary>Creates an instance of the default implementation of a Hash-based Message Authentication Code (HMAC).</summary>
	/// <returns>A new SHA-1 instance, unless the default settings have been changed by using the &lt;cryptoClass&gt; element.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public new static HMAC Create()
	{
		return Create("System.Security.Cryptography.HMAC");
	}

	/// <summary>Creates an instance of the specified implementation of a Hash-based Message Authentication Code (HMAC).</summary>
	/// <returns>A new instance of the specified HMAC implementation.</returns>
	/// <param name="algorithmName">The HMAC implementation to use. The following table shows the valid values for the <paramref name="algorithmName" /> parameter and the algorithms they map to.Parameter valueImplements System.Security.Cryptography.HMAC<see cref="T:System.Security.Cryptography.HMACSHA1" />System.Security.Cryptography.KeyedHashAlgorithm<see cref="T:System.Security.Cryptography.HMACSHA1" />HMACMD5<see cref="T:System.Security.Cryptography.HMACMD5" />System.Security.Cryptography.HMACMD5<see cref="T:System.Security.Cryptography.HMACMD5" />HMACRIPEMD160<see cref="T:System.Security.Cryptography.HMACRIPEMD160" />System.Security.Cryptography.HMACRIPEMD160<see cref="T:System.Security.Cryptography.HMACRIPEMD160" />HMACSHA1<see cref="T:System.Security.Cryptography.HMACSHA1" />System.Security.Cryptography.HMACSHA1<see cref="T:System.Security.Cryptography.HMACSHA1" />HMACSHA256<see cref="T:System.Security.Cryptography.HMACSHA256" />System.Security.Cryptography.HMACSHA256<see cref="T:System.Security.Cryptography.HMACSHA256" />HMACSHA384<see cref="T:System.Security.Cryptography.HMACSHA384" />System.Security.Cryptography.HMACSHA384<see cref="T:System.Security.Cryptography.HMACSHA384" />HMACSHA512<see cref="T:System.Security.Cryptography.HMACSHA512" />System.Security.Cryptography.HMACSHA512<see cref="T:System.Security.Cryptography.HMACSHA512" />MACTripleDES<see cref="T:System.Security.Cryptography.MACTripleDES" />System.Security.Cryptography.MACTripleDES<see cref="T:System.Security.Cryptography.MACTripleDES" /></param>
	public new static HMAC Create(string algorithmName)
	{
		return (HMAC)CryptoConfig.CreateFromName(algorithmName);
	}

	/// <summary>Initializes an instance of the default implementation of <see cref="T:System.Security.Cryptography.HMAC" />.</summary>
	public override void Initialize()
	{
		m_hash1.Initialize();
		m_hash2.Initialize();
		m_hashing = false;
	}

	/// <summary>When overridden in a derived class, routes data written to the object into the default <see cref="T:System.Security.Cryptography.HMAC" /> hash algorithm for computing the hash value.</summary>
	/// <param name="rgb">The input data. </param>
	/// <param name="ib">The offset into the byte array from which to begin using data. </param>
	/// <param name="cb">The number of bytes in the array to use as data. </param>
	protected override void HashCore(byte[] rgb, int ib, int cb)
	{
		if (!m_hashing)
		{
			m_hash1.TransformBlock(m_inner, 0, m_inner.Length, m_inner, 0);
			m_hashing = true;
		}
		m_hash1.TransformBlock(rgb, ib, cb, rgb, ib);
	}

	/// <summary>When overridden in a derived class, finalizes the hash computation after the last data is processed by the cryptographic stream object.</summary>
	/// <returns>The computed hash code in a byte array.</returns>
	protected override byte[] HashFinal()
	{
		if (!m_hashing)
		{
			m_hash1.TransformBlock(m_inner, 0, m_inner.Length, m_inner, 0);
			m_hashing = true;
		}
		m_hash1.TransformFinalBlock(EmptyArray<byte>.Value, 0, 0);
		byte[] hashValue = m_hash1.HashValue;
		m_hash2.TransformBlock(m_outer, 0, m_outer.Length, m_outer, 0);
		m_hash2.TransformBlock(hashValue, 0, hashValue.Length, hashValue, 0);
		m_hashing = false;
		m_hash2.TransformFinalBlock(EmptyArray<byte>.Value, 0, 0);
		return m_hash2.HashValue;
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.HMAC" /> class when a key change is legitimate and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (m_hash1 != null)
			{
				((IDisposable)m_hash1).Dispose();
			}
			if (m_hash2 != null)
			{
				((IDisposable)m_hash2).Dispose();
			}
			if (m_inner != null)
			{
				Array.Clear(m_inner, 0, m_inner.Length);
			}
			if (m_outer != null)
			{
				Array.Clear(m_outer, 0, m_outer.Length);
			}
		}
		base.Dispose(disposing);
	}

	internal static HashAlgorithm GetHashAlgorithmWithFipsFallback(Func<HashAlgorithm> createStandardHashAlgorithmCallback, Func<HashAlgorithm> createFipsHashAlgorithmCallback)
	{
		if (CryptoConfig.AllowOnlyFipsAlgorithms)
		{
			try
			{
				return createFipsHashAlgorithmCallback();
			}
			catch (PlatformNotSupportedException ex)
			{
				throw new InvalidOperationException(ex.Message, ex);
			}
		}
		return createStandardHashAlgorithmCallback();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.HMAC" /> class. </summary>
	protected HMAC()
	{
	}
}
