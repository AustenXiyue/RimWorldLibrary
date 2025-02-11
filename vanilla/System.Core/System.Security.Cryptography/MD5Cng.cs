namespace System.Security.Cryptography;

/// <summary>Provides a CNG (Cryptography Next Generation) implementation of the MD5 (Message Digest 5) 128-bit hashing algorithm.</summary>
public sealed class MD5Cng : MD5
{
	private static byte[] Empty = new byte[0];

	private MD5 hash;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.MD5Cng" /> class. </summary>
	/// <exception cref="T:System.InvalidOperationException">This implementation is not part of the Windows Platform FIPS-validated cryptographic algorithms.</exception>
	[SecurityCritical]
	public MD5Cng()
	{
		hash = new MD5CryptoServiceProvider();
	}

	/// <summary>Initializes, or re-initializes, the instance of the hash algorithm. </summary>
	[SecurityCritical]
	public override void Initialize()
	{
		hash.Initialize();
	}

	[SecurityCritical]
	protected override void HashCore(byte[] array, int ibStart, int cbSize)
	{
		hash.TransformBlock(array, ibStart, cbSize, null, 0);
	}

	[SecurityCritical]
	protected override byte[] HashFinal()
	{
		hash.TransformFinalBlock(Empty, 0, 0);
		HashValue = hash.Hash;
		return HashValue;
	}

	[SecurityCritical]
	protected override void Dispose(bool disposing)
	{
		((IDisposable)hash).Dispose();
		base.Dispose(disposing);
	}
}
