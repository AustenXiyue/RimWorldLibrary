namespace System.Security.Cryptography;

/// <summary>Provides a Cryptography Next Generation (CNG) implementation of the Secure Hash Algorithm (SHA).</summary>
public sealed class SHA1Cng : SHA1
{
	private static byte[] Empty = new byte[0];

	private SHA1 hash;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.SHA1Cng" /> class. </summary>
	[SecurityCritical]
	public SHA1Cng()
	{
		hash = new SHA1Managed();
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
