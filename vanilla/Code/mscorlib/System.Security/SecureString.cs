using System.Runtime.ExceptionServices;

namespace System.Security;

/// <summary>Represents text that should be kept confidential. The text is encrypted for privacy when being used, and deleted from computer memory when no longer needed. This class cannot be inherited.</summary>
[MonoTODO("work in progress - encryption is missing")]
public sealed class SecureString : IDisposable
{
	private const int BlockSize = 16;

	private const int MaxSize = 65536;

	private int length;

	private bool disposed;

	private bool read_only;

	private byte[] data;

	/// <summary>Gets the number of characters in the current secure string.</summary>
	/// <returns>The number of <see cref="T:System.Char" /> objects in this secure string.</returns>
	/// <exception cref="T:System.ObjectDisposedException">This secure string has already been disposed.</exception>
	public int Length
	{
		get
		{
			if (disposed)
			{
				throw new ObjectDisposedException("SecureString");
			}
			return length;
		}
	}

	static SecureString()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.SecureString" /> class.</summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error occurred while encrypting or decrypting the value of this instance.</exception>
	/// <exception cref="T:System.NotSupportedException">This operation is not supported on this platform.</exception>
	public SecureString()
	{
		Alloc(8, realloc: false);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.SecureString" /> class from a subarray of <see cref="T:System.Char" /> objects.</summary>
	/// <param name="value">A pointer to an array of <see cref="T:System.Char" /> objects.</param>
	/// <param name="length">The number of elements of <paramref name="value" /> to include in the new instance.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="length" /> is less than zero or greater than 65536.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error occurred while encrypting or decrypting the value of this secure string.</exception>
	/// <exception cref="T:System.NotSupportedException">This operation is not supported on this platform.</exception>
	[CLSCompliant(false)]
	public unsafe SecureString(char* value, int length)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (length < 0 || length > 65536)
		{
			throw new ArgumentOutOfRangeException("length", "< 0 || > 65536");
		}
		this.length = length;
		Alloc(length, realloc: false);
		int num = 0;
		for (int i = 0; i < length; i++)
		{
			char c = *(value++);
			data[num++] = (byte)((int)c >> 8);
			data[num++] = (byte)c;
		}
		Encrypt();
	}

	/// <summary>Appends a character to the end of the current secure string.</summary>
	/// <param name="c">A character to append to this secure string.</param>
	/// <exception cref="T:System.ObjectDisposedException">This secure string has already been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">This secure string is read-only.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Performing this operation would make the length of this secure string greater than 65536 characters.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error occurred while encrypting or decrypting the value of this secure string.</exception>
	[HandleProcessCorruptedStateExceptions]
	public void AppendChar(char c)
	{
		if (disposed)
		{
			throw new ObjectDisposedException("SecureString");
		}
		if (read_only)
		{
			throw new InvalidOperationException(Locale.GetText("SecureString is read-only."));
		}
		if (length == 65536)
		{
			throw new ArgumentOutOfRangeException("length", "> 65536");
		}
		try
		{
			Decrypt();
			int num = length * 2;
			Alloc(++length, realloc: true);
			data[num++] = (byte)((int)c >> 8);
			data[num++] = (byte)c;
		}
		finally
		{
			Encrypt();
		}
	}

	/// <summary>Deletes the value of the current secure string.</summary>
	/// <exception cref="T:System.ObjectDisposedException">This secure string has already been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">This secure string is read-only.</exception>
	public void Clear()
	{
		if (disposed)
		{
			throw new ObjectDisposedException("SecureString");
		}
		if (read_only)
		{
			throw new InvalidOperationException(Locale.GetText("SecureString is read-only."));
		}
		Array.Clear(data, 0, data.Length);
		length = 0;
	}

	/// <summary>Creates a copy of the current secure string.</summary>
	/// <returns>A duplicate of this secure string.</returns>
	/// <exception cref="T:System.ObjectDisposedException">This secure string has already been disposed.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error occurred while encrypting or decrypting the value of this secure string.</exception>
	public SecureString Copy()
	{
		return new SecureString
		{
			data = (byte[])data.Clone(),
			length = length
		};
	}

	/// <summary>Releases all resources used by the current <see cref="T:System.Security.SecureString" /> object.</summary>
	[SecuritySafeCritical]
	public void Dispose()
	{
		disposed = true;
		if (data != null)
		{
			Array.Clear(data, 0, data.Length);
			data = null;
		}
		length = 0;
	}

	/// <summary>Inserts a character in this secure string at the specified index position.</summary>
	/// <param name="index">The index position where parameter <paramref name="c" /> is inserted.</param>
	/// <param name="c">The character to insert.</param>
	/// <exception cref="T:System.ObjectDisposedException">This secure string has already been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">This secure string is read-only.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero, or greater than the length of this secure string.-or-Performing this operation would make the length of this secure string greater than 65536 characters.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error occurred while encrypting or decrypting the value of this secure string.</exception>
	[HandleProcessCorruptedStateExceptions]
	public void InsertAt(int index, char c)
	{
		if (disposed)
		{
			throw new ObjectDisposedException("SecureString");
		}
		if (read_only)
		{
			throw new InvalidOperationException(Locale.GetText("SecureString is read-only."));
		}
		if (index < 0 || index > length)
		{
			throw new ArgumentOutOfRangeException("index", "< 0 || > length");
		}
		if (length >= 65536)
		{
			string text = Locale.GetText("Maximum string size is '{0}'.", 65536);
			throw new ArgumentOutOfRangeException("index", text);
		}
		try
		{
			Decrypt();
			Alloc(++length, realloc: true);
			int num = index * 2;
			Buffer.BlockCopy(data, num, data, num + 2, data.Length - num - 2);
			data[num++] = (byte)((int)c >> 8);
			data[num] = (byte)c;
		}
		finally
		{
			Encrypt();
		}
	}

	/// <summary>Indicates whether this secure string is marked read-only.</summary>
	/// <returns>true if this secure string is marked read-only; otherwise, false.</returns>
	/// <exception cref="T:System.ObjectDisposedException">This secure string has already been disposed.</exception>
	public bool IsReadOnly()
	{
		if (disposed)
		{
			throw new ObjectDisposedException("SecureString");
		}
		return read_only;
	}

	/// <summary>Makes the text value of this secure string read-only.   </summary>
	/// <exception cref="T:System.ObjectDisposedException">This secure string has already been disposed.</exception>
	public void MakeReadOnly()
	{
		read_only = true;
	}

	/// <summary>Removes the character at the specified index position from this secure string.</summary>
	/// <param name="index">The index position of a character in this secure string.</param>
	/// <exception cref="T:System.ObjectDisposedException">This secure string has already been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">This secure string is read-only.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero, or greater than or equal to the length of this secure string.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error occurred while encrypting or decrypting the value of this secure string.</exception>
	[HandleProcessCorruptedStateExceptions]
	public void RemoveAt(int index)
	{
		if (disposed)
		{
			throw new ObjectDisposedException("SecureString");
		}
		if (read_only)
		{
			throw new InvalidOperationException(Locale.GetText("SecureString is read-only."));
		}
		if (index < 0 || index >= length)
		{
			throw new ArgumentOutOfRangeException("index", "< 0 || > length");
		}
		try
		{
			Decrypt();
			Buffer.BlockCopy(data, index * 2 + 2, data, index * 2, data.Length - index * 2 - 2);
			Alloc(--length, realloc: true);
		}
		finally
		{
			Encrypt();
		}
	}

	/// <summary>Replaces the existing character at the specified index position with another character.</summary>
	/// <param name="index">The index position of an existing character in this secure string</param>
	/// <param name="c">A character that replaces the existing character.</param>
	/// <exception cref="T:System.ObjectDisposedException">This secure string has already been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">This secure string is read-only.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero, or greater than or equal to the length of this secure string.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error occurred while encrypting or decrypting the value of this secure string.</exception>
	[HandleProcessCorruptedStateExceptions]
	public void SetAt(int index, char c)
	{
		if (disposed)
		{
			throw new ObjectDisposedException("SecureString");
		}
		if (read_only)
		{
			throw new InvalidOperationException(Locale.GetText("SecureString is read-only."));
		}
		if (index < 0 || index >= length)
		{
			throw new ArgumentOutOfRangeException("index", "< 0 || > length");
		}
		try
		{
			Decrypt();
			int num = index * 2;
			data[num++] = (byte)((int)c >> 8);
			data[num] = (byte)c;
		}
		finally
		{
			Encrypt();
		}
	}

	private void Encrypt()
	{
		if (data != null)
		{
			_ = data.LongLength;
		}
	}

	private void Decrypt()
	{
		if (data != null)
		{
			_ = data.LongLength;
		}
	}

	private void Alloc(int length, bool realloc)
	{
		if (length < 0 || length > 65536)
		{
			throw new ArgumentOutOfRangeException("length", "< 0 || > 65536");
		}
		int num = (length >> 3) + (((length & 7) != 0) ? 1 : 0) << 4;
		if (!realloc || data == null || num != data.Length)
		{
			if (realloc)
			{
				byte[] array = new byte[num];
				Array.Copy(data, 0, array, 0, Math.Min(data.Length, array.Length));
				Array.Clear(data, 0, data.Length);
				data = array;
			}
			else
			{
				data = new byte[num];
			}
		}
	}

	internal byte[] GetBuffer()
	{
		byte[] array = new byte[length << 1];
		try
		{
			Decrypt();
			Buffer.BlockCopy(data, 0, array, 0, array.Length);
			return array;
		}
		finally
		{
			Encrypt();
		}
	}
}
