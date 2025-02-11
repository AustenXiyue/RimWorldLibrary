using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Cryptography;

/// <summary>Implements password-based key derivation functionality, PBKDF2, by using a pseudo-random number generator based on <see cref="T:System.Security.Cryptography.HMACSHA1" />.</summary>
[ComVisible(true)]
public class Rfc2898DeriveBytes : DeriveBytes
{
	private byte[] m_buffer;

	private byte[] m_salt;

	private HMACSHA1 m_hmacsha1;

	private byte[] m_password;

	private uint m_iterations;

	private uint m_block;

	private int m_startIndex;

	private int m_endIndex;

	private const int BlockSize = 20;

	/// <summary>Gets or sets the number of iterations for the operation.</summary>
	/// <returns>The number of iterations for the operation.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The number of iterations is less than 1. </exception>
	public int IterationCount
	{
		get
		{
			return (int)m_iterations;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Positive number required."));
			}
			m_iterations = (uint)value;
			Initialize();
		}
	}

	/// <summary>Gets or sets the key salt value for the operation.</summary>
	/// <returns>The key salt value for the operation.</returns>
	/// <exception cref="T:System.ArgumentException">The specified salt size is smaller than 8 bytes. </exception>
	/// <exception cref="T:System.ArgumentNullException">The salt is null. </exception>
	public byte[] Salt
	{
		get
		{
			return (byte[])m_salt.Clone();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Length < 8)
			{
				throw new ArgumentException(Environment.GetResourceString("Salt is not at least eight bytes."));
			}
			m_salt = (byte[])value.Clone();
			Initialize();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes" /> class using the password and salt size to derive the key.</summary>
	/// <param name="password">The password used to derive the key. </param>
	/// <param name="saltSize">The size of the random salt that you want the class to generate. </param>
	/// <exception cref="T:System.ArgumentException">The specified salt size is smaller than 8 bytes. </exception>
	/// <exception cref="T:System.ArgumentNullException">The password or salt is null. </exception>
	public Rfc2898DeriveBytes(string password, int saltSize)
		: this(password, saltSize, 1000)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes" /> class using a password, a salt size, and number of iterations to derive the key.</summary>
	/// <param name="password">The password used to derive the key. </param>
	/// <param name="saltSize">The size of the random salt that you want the class to generate. </param>
	/// <param name="iterations">The number of iterations for the operation. </param>
	/// <exception cref="T:System.ArgumentException">The specified salt size is smaller than 8 bytes or the iteration count is less than 1. </exception>
	/// <exception cref="T:System.ArgumentNullException">The password or salt is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="iterations " />is out of range. This parameter requires a non-negative number.</exception>
	[SecuritySafeCritical]
	public Rfc2898DeriveBytes(string password, int saltSize, int iterations)
	{
		if (saltSize < 0)
		{
			throw new ArgumentOutOfRangeException("saltSize", Environment.GetResourceString("Non-negative number required."));
		}
		byte[] array = new byte[saltSize];
		Utils.StaticRandomNumberGenerator.GetBytes(array);
		Salt = array;
		IterationCount = iterations;
		m_password = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(password);
		m_hmacsha1 = new HMACSHA1(m_password);
		Initialize();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes" /> class using a password and salt to derive the key.</summary>
	/// <param name="password">The password used to derive the key. </param>
	/// <param name="salt">The key salt used to derive the key. </param>
	/// <exception cref="T:System.ArgumentException">The specified salt size is smaller than 8 bytes or the iteration count is less than 1. </exception>
	/// <exception cref="T:System.ArgumentNullException">The password or salt is null. </exception>
	public Rfc2898DeriveBytes(string password, byte[] salt)
		: this(password, salt, 1000)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes" /> class using a password, a salt, and number of iterations to derive the key.</summary>
	/// <param name="password">The password used to derive the key. </param>
	/// <param name="salt">The key salt used to derive the key. </param>
	/// <param name="iterations">The number of iterations for the operation. </param>
	/// <exception cref="T:System.ArgumentException">The specified salt size is smaller than 8 bytes or the iteration count is less than 1. </exception>
	/// <exception cref="T:System.ArgumentNullException">The password or salt is null. </exception>
	public Rfc2898DeriveBytes(string password, byte[] salt, int iterations)
		: this(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(password), salt, iterations)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes" /> class using a password, a salt, and number of iterations to derive the key.</summary>
	/// <param name="password">The password used to derive the key. </param>
	/// <param name="salt">The key salt used to derive the key.</param>
	/// <param name="iterations">The number of iterations for the operation. </param>
	/// <exception cref="T:System.ArgumentException">The specified salt size is smaller than 8 bytes or the iteration count is less than 1. </exception>
	/// <exception cref="T:System.ArgumentNullException">The password or salt is null. </exception>
	[SecuritySafeCritical]
	public Rfc2898DeriveBytes(byte[] password, byte[] salt, int iterations)
	{
		Salt = salt;
		IterationCount = iterations;
		m_password = password;
		m_hmacsha1 = new HMACSHA1(password);
		Initialize();
	}

	/// <summary>Returns the pseudo-random key for this object.</summary>
	/// <returns>A byte array filled with pseudo-random key bytes.</returns>
	/// <param name="cb">The number of pseudo-random key bytes to generate. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="cb " />is out of range. This parameter requires a non-negative number.</exception>
	public override byte[] GetBytes(int cb)
	{
		if (cb <= 0)
		{
			throw new ArgumentOutOfRangeException("cb", Environment.GetResourceString("Positive number required."));
		}
		byte[] array = new byte[cb];
		int i = 0;
		int num = m_endIndex - m_startIndex;
		if (num > 0)
		{
			if (cb < num)
			{
				Buffer.InternalBlockCopy(m_buffer, m_startIndex, array, 0, cb);
				m_startIndex += cb;
				return array;
			}
			Buffer.InternalBlockCopy(m_buffer, m_startIndex, array, 0, num);
			m_startIndex = (m_endIndex = 0);
			i += num;
		}
		for (; i < cb; i += 20)
		{
			byte[] src = Func();
			int num2 = cb - i;
			if (num2 > 20)
			{
				Buffer.InternalBlockCopy(src, 0, array, i, 20);
				continue;
			}
			Buffer.InternalBlockCopy(src, 0, array, i, num2);
			i += num2;
			Buffer.InternalBlockCopy(src, num2, m_buffer, m_startIndex, 20 - num2);
			m_endIndex += 20 - num2;
			return array;
		}
		return array;
	}

	/// <summary>Resets the state of the operation.</summary>
	public override void Reset()
	{
		Initialize();
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes" /> class and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			if (m_hmacsha1 != null)
			{
				((IDisposable)m_hmacsha1).Dispose();
			}
			if (m_buffer != null)
			{
				Array.Clear(m_buffer, 0, m_buffer.Length);
			}
			if (m_salt != null)
			{
				Array.Clear(m_salt, 0, m_salt.Length);
			}
		}
	}

	private void Initialize()
	{
		if (m_buffer != null)
		{
			Array.Clear(m_buffer, 0, m_buffer.Length);
		}
		m_buffer = new byte[20];
		m_block = 1u;
		m_startIndex = (m_endIndex = 0);
	}

	private byte[] Func()
	{
		byte[] array = Utils.Int(m_block);
		m_hmacsha1.TransformBlock(m_salt, 0, m_salt.Length, null, 0);
		m_hmacsha1.TransformBlock(array, 0, array.Length, null, 0);
		m_hmacsha1.TransformFinalBlock(EmptyArray<byte>.Value, 0, 0);
		byte[] hashValue = m_hmacsha1.HashValue;
		m_hmacsha1.Initialize();
		byte[] array2 = hashValue;
		for (int i = 2; i <= m_iterations; i++)
		{
			m_hmacsha1.TransformBlock(hashValue, 0, hashValue.Length, null, 0);
			m_hmacsha1.TransformFinalBlock(EmptyArray<byte>.Value, 0, 0);
			hashValue = m_hmacsha1.HashValue;
			for (int j = 0; j < 20; j++)
			{
				array2[j] ^= hashValue[j];
			}
			m_hmacsha1.Initialize();
		}
		m_block++;
		return array2;
	}

	[SecuritySafeCritical]
	public byte[] CryptDeriveKey(string algname, string alghashname, int keySize, byte[] rgbIV)
	{
		if (keySize < 0)
		{
			throw new CryptographicException(Environment.GetResourceString("Specified key is not a valid size for this algorithm."));
		}
		throw new NotSupportedException("CspParameters are not supported by Mono");
	}
}
