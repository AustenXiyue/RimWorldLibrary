namespace System.Security.Cryptography;

/// <summary>Defines a wrapper object to access the cryptographic service provider (CSP) implementation of the <see cref="T:System.Security.Cryptography.SHA512" /> algorithm. </summary>
public sealed class SHA512CryptoServiceProvider : SHA512
{
	private static byte[] Empty = new byte[0];

	private SHA512 hash;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.SHA512CryptoServiceProvider" /> class. </summary>
	[SecurityCritical]
	public SHA512CryptoServiceProvider()
	{
		hash = new SHA512Managed();
	}

	/// <summary>Initializes, or reinitializes, an instance of a hash algorithm.</summary>
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
