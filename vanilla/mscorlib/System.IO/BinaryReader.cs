using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Mono.Security;

namespace System.IO;

/// <summary>Reads primitive data types as binary values in a specific encoding.</summary>
/// <filterpriority>2</filterpriority>
[ComVisible(true)]
public class BinaryReader : IDisposable
{
	private const int MaxCharBytesSize = 128;

	private Stream m_stream;

	private byte[] m_buffer;

	private Decoder m_decoder;

	private byte[] m_charBytes;

	private char[] m_singleChar;

	private char[] m_charBuffer;

	private int m_maxCharsSize;

	private bool m_2BytesPerChar;

	private bool m_isMemoryStream;

	private bool m_leaveOpen;

	/// <summary>Exposes access to the underlying stream of the <see cref="T:System.IO.BinaryReader" />.</summary>
	/// <returns>The underlying stream associated with the BinaryReader.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual Stream BaseStream => m_stream;

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.BinaryReader" /> class based on the specified stream and using UTF-8 encoding.</summary>
	/// <param name="input">The input stream. </param>
	/// <exception cref="T:System.ArgumentException">The stream does not support reading, is null, or is already closed. </exception>
	public BinaryReader(Stream input)
		: this(input, new UTF8Encoding(), leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.BinaryReader" /> class based on the specified stream and character encoding.</summary>
	/// <param name="input">The input stream. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <exception cref="T:System.ArgumentException">The stream does not support reading, is null, or is already closed. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="encoding" /> is null. </exception>
	public BinaryReader(Stream input, Encoding encoding)
		: this(input, encoding, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.BinaryReader" /> class based on the specified stream and character encoding, and optionally leaves the stream open.</summary>
	/// <param name="input">The input stream.</param>
	/// <param name="encoding">The character encoding to use.</param>
	/// <param name="leaveOpen">true to leave the stream open after the <see cref="T:System.IO.BinaryReader" /> object is disposed; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentException">The stream does not support reading, is null, or is already closed. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="encoding" /> or <paramref name="input" /> is null. </exception>
	public BinaryReader(Stream input, Encoding encoding, bool leaveOpen)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (!input.CanRead)
		{
			throw new ArgumentException(Environment.GetResourceString("Stream was not readable."));
		}
		m_stream = input;
		m_decoder = encoding.GetDecoder();
		m_maxCharsSize = encoding.GetMaxCharCount(128);
		int num = encoding.GetMaxByteCount(1);
		if (num < 16)
		{
			num = 16;
		}
		m_buffer = new byte[num];
		m_2BytesPerChar = encoding is UnicodeEncoding;
		m_isMemoryStream = m_stream.GetType() == typeof(MemoryStream);
		m_leaveOpen = leaveOpen;
	}

	/// <summary>Closes the current reader and the underlying stream.</summary>
	/// <filterpriority>2</filterpriority>
	public virtual void Close()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.BinaryReader" /> class and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Stream stream = m_stream;
			m_stream = null;
			if (stream != null && !m_leaveOpen)
			{
				stream.Close();
			}
		}
		m_stream = null;
		m_buffer = null;
		m_decoder = null;
		m_charBytes = null;
		m_singleChar = null;
		m_charBuffer = null;
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.IO.BinaryReader" /> class.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Returns the next available character and does not advance the byte or character position.</summary>
	/// <returns>The next available character, or -1 if no more characters are available or the stream does not support seeking.</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ArgumentException">The current character cannot be decoded into the internal character buffer by using the <see cref="T:System.Text.Encoding" /> selected for the stream.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual int PeekChar()
	{
		if (m_stream == null)
		{
			__Error.FileNotOpen();
		}
		if (!m_stream.CanSeek)
		{
			return -1;
		}
		long position = m_stream.Position;
		int result = Read();
		m_stream.Position = position;
		return result;
	}

	/// <summary>Reads characters from the underlying stream and advances the current position of the stream in accordance with the Encoding used and the specific character being read from the stream.</summary>
	/// <returns>The next character from the input stream, or -1 if no characters are currently available.</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual int Read()
	{
		if (m_stream == null)
		{
			__Error.FileNotOpen();
		}
		return InternalReadOneChar();
	}

	/// <summary>Reads a Boolean value from the current stream and advances the current position of the stream by one byte.</summary>
	/// <returns>true if the byte is nonzero; otherwise, false.</returns>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual bool ReadBoolean()
	{
		FillBuffer(1);
		return m_buffer[0] != 0;
	}

	/// <summary>Reads the next byte from the current stream and advances the current position of the stream by one byte.</summary>
	/// <returns>The next byte read from the current stream.</returns>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual byte ReadByte()
	{
		if (m_stream == null)
		{
			__Error.FileNotOpen();
		}
		int num = m_stream.ReadByte();
		if (num == -1)
		{
			__Error.EndOfFile();
		}
		return (byte)num;
	}

	/// <summary>Reads a signed byte from this stream and advances the current position of the stream by one byte.</summary>
	/// <returns>A signed byte read from the current stream.</returns>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>2</filterpriority>
	[CLSCompliant(false)]
	public virtual sbyte ReadSByte()
	{
		FillBuffer(1);
		return (sbyte)m_buffer[0];
	}

	/// <summary>Reads the next character from the current stream and advances the current position of the stream in accordance with the Encoding used and the specific character being read from the stream.</summary>
	/// <returns>A character read from the current stream.</returns>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ArgumentException">A surrogate character was read. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual char ReadChar()
	{
		int num = Read();
		if (num == -1)
		{
			__Error.EndOfFile();
		}
		return (char)num;
	}

	/// <summary>Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two bytes.</summary>
	/// <returns>A 2-byte signed integer read from the current stream.</returns>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual short ReadInt16()
	{
		FillBuffer(2);
		return (short)(m_buffer[0] | (m_buffer[1] << 8));
	}

	/// <summary>Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the position of the stream by two bytes.</summary>
	/// <returns>A 2-byte unsigned integer read from this stream.</returns>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>2</filterpriority>
	[CLSCompliant(false)]
	public virtual ushort ReadUInt16()
	{
		FillBuffer(2);
		return (ushort)(m_buffer[0] | (m_buffer[1] << 8));
	}

	/// <summary>Reads a 4-byte signed integer from the current stream and advances the current position of the stream by four bytes.</summary>
	/// <returns>A 4-byte signed integer read from the current stream.</returns>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual int ReadInt32()
	{
		if (m_isMemoryStream)
		{
			if (m_stream == null)
			{
				__Error.FileNotOpen();
			}
			return (m_stream as MemoryStream).InternalReadInt32();
		}
		FillBuffer(4);
		return m_buffer[0] | (m_buffer[1] << 8) | (m_buffer[2] << 16) | (m_buffer[3] << 24);
	}

	/// <summary>Reads a 4-byte unsigned integer from the current stream and advances the position of the stream by four bytes.</summary>
	/// <returns>A 4-byte unsigned integer read from this stream.</returns>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>2</filterpriority>
	[CLSCompliant(false)]
	public virtual uint ReadUInt32()
	{
		FillBuffer(4);
		return (uint)(m_buffer[0] | (m_buffer[1] << 8) | (m_buffer[2] << 16) | (m_buffer[3] << 24));
	}

	/// <summary>Reads an 8-byte signed integer from the current stream and advances the current position of the stream by eight bytes.</summary>
	/// <returns>An 8-byte signed integer read from the current stream.</returns>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual long ReadInt64()
	{
		FillBuffer(8);
		uint num = (uint)(m_buffer[0] | (m_buffer[1] << 8) | (m_buffer[2] << 16) | (m_buffer[3] << 24));
		return (long)(((ulong)(uint)(m_buffer[4] | (m_buffer[5] << 8) | (m_buffer[6] << 16) | (m_buffer[7] << 24)) << 32) | num);
	}

	/// <summary>Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight bytes.</summary>
	/// <returns>An 8-byte unsigned integer read from this stream.</returns>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>2</filterpriority>
	[CLSCompliant(false)]
	public virtual ulong ReadUInt64()
	{
		FillBuffer(8);
		uint num = (uint)(m_buffer[0] | (m_buffer[1] << 8) | (m_buffer[2] << 16) | (m_buffer[3] << 24));
		return ((ulong)(uint)(m_buffer[4] | (m_buffer[5] << 8) | (m_buffer[6] << 16) | (m_buffer[7] << 24)) << 32) | num;
	}

	/// <summary>Reads a 4-byte floating point value from the current stream and advances the current position of the stream by four bytes.</summary>
	/// <returns>A 4-byte floating point value read from the current stream.</returns>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public virtual float ReadSingle()
	{
		FillBuffer(4);
		return BitConverterLE.ToSingle(m_buffer, 0);
	}

	/// <summary>Reads an 8-byte floating point value from the current stream and advances the current position of the stream by eight bytes.</summary>
	/// <returns>An 8-byte floating point value read from the current stream.</returns>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public virtual double ReadDouble()
	{
		FillBuffer(8);
		return BitConverterLE.ToDouble(m_buffer, 0);
	}

	/// <summary>Reads a decimal value from the current stream and advances the current position of the stream by sixteen bytes.</summary>
	/// <returns>A decimal value read from the current stream.</returns>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual decimal ReadDecimal()
	{
		FillBuffer(16);
		try
		{
			return decimal.ToDecimal(m_buffer);
		}
		catch (ArgumentException innerException)
		{
			throw new IOException(Environment.GetResourceString("Decimal byte array constructor requires an array of length four containing valid decimal bytes."), innerException);
		}
	}

	/// <summary>Reads a string from the current stream. The string is prefixed with the length, encoded as an integer seven bits at a time.</summary>
	/// <returns>The string being read.</returns>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual string ReadString()
	{
		if (m_stream == null)
		{
			__Error.FileNotOpen();
		}
		int num = 0;
		int num2 = Read7BitEncodedInt();
		if (num2 < 0)
		{
			throw new IOException(Environment.GetResourceString("BinaryReader encountered an invalid string length of {0} characters.", num2));
		}
		if (num2 == 0)
		{
			return string.Empty;
		}
		if (m_charBytes == null)
		{
			m_charBytes = new byte[128];
		}
		if (m_charBuffer == null)
		{
			m_charBuffer = new char[m_maxCharsSize];
		}
		StringBuilder stringBuilder = null;
		do
		{
			int count = ((num2 - num > 128) ? 128 : (num2 - num));
			int num3 = m_stream.Read(m_charBytes, 0, count);
			if (num3 == 0)
			{
				__Error.EndOfFile();
			}
			int chars = m_decoder.GetChars(m_charBytes, 0, num3, m_charBuffer, 0);
			if (num == 0 && num3 == num2)
			{
				return new string(m_charBuffer, 0, chars);
			}
			if (stringBuilder == null)
			{
				stringBuilder = StringBuilderCache.Acquire(num2);
			}
			stringBuilder.Append(m_charBuffer, 0, chars);
			num += num3;
		}
		while (num < num2);
		return StringBuilderCache.GetStringAndRelease(stringBuilder);
	}

	/// <summary>Reads the specified number of characters from the stream, starting from a specified point in the character array.</summary>
	/// <returns>The total number of characters read into the buffer. This might be less than the number of characters requested if that many characters are not currently available, or it might be zero if the end of the stream is reached.</returns>
	/// <param name="buffer">The buffer to read data into. </param>
	/// <param name="index">The starting point in the buffer at which to begin reading into the buffer. </param>
	/// <param name="count">The number of characters to read. </param>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. -or-The number of decoded characters to read is greater than <paramref name="count" />. This can happen if a Unicode decoder returns fallback characters or a surrogate pair.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public virtual int Read(char[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", Environment.GetResourceString("Buffer cannot be null."));
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (buffer.Length - index < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		if (m_stream == null)
		{
			__Error.FileNotOpen();
		}
		return InternalReadChars(buffer, index, count);
	}

	[SecurityCritical]
	private unsafe int InternalReadChars(char[] buffer, int index, int count)
	{
		int num = 0;
		int num2 = count;
		if (m_charBytes == null)
		{
			m_charBytes = new byte[128];
		}
		while (num2 > 0)
		{
			int num3 = 0;
			num = num2;
			if (m_decoder is DecoderNLS { HasState: not false } && num > 1)
			{
				num--;
			}
			if (m_2BytesPerChar)
			{
				num <<= 1;
			}
			if (num > 128)
			{
				num = 128;
			}
			int num4 = 0;
			byte[] array = null;
			if (m_isMemoryStream)
			{
				MemoryStream obj = m_stream as MemoryStream;
				num4 = obj.InternalGetPosition();
				num = obj.InternalEmulateRead(num);
				array = obj.InternalGetBuffer();
			}
			else
			{
				num = m_stream.Read(m_charBytes, 0, num);
				array = m_charBytes;
			}
			if (num == 0)
			{
				return count - num2;
			}
			checked
			{
				if (num4 < 0 || num < 0 || num4 + num > array.Length)
				{
					throw new ArgumentOutOfRangeException("byteCount");
				}
				if (index < 0 || num2 < 0 || index + num2 > buffer.Length)
				{
					throw new ArgumentOutOfRangeException("charsRemaining");
				}
			}
			fixed (byte* ptr = array)
			{
				fixed (char* ptr2 = buffer)
				{
					num3 = m_decoder.GetChars((byte*)checked(unchecked((nuint)ptr) + unchecked((nuint)num4)), num, (char*)checked(unchecked((nuint)ptr2) + unchecked((nuint)checked(unchecked((nint)index) * (nint)2))), num2, flush: false);
				}
			}
			num2 -= num3;
			index += num3;
		}
		return count - num2;
	}

	private int InternalReadOneChar()
	{
		int num = 0;
		int num2 = 0;
		long num3 = 0L;
		if (m_stream.CanSeek)
		{
			num3 = m_stream.Position;
		}
		if (m_charBytes == null)
		{
			m_charBytes = new byte[128];
		}
		if (m_singleChar == null)
		{
			m_singleChar = new char[1];
		}
		while (num == 0)
		{
			num2 = ((!m_2BytesPerChar) ? 1 : 2);
			int num4 = m_stream.ReadByte();
			m_charBytes[0] = (byte)num4;
			if (num4 == -1)
			{
				num2 = 0;
			}
			if (num2 == 2)
			{
				num4 = m_stream.ReadByte();
				m_charBytes[1] = (byte)num4;
				if (num4 == -1)
				{
					num2 = 1;
				}
			}
			if (num2 == 0)
			{
				return -1;
			}
			try
			{
				num = m_decoder.GetChars(m_charBytes, 0, num2, m_singleChar, 0);
			}
			catch
			{
				if (m_stream.CanSeek)
				{
					m_stream.Seek(num3 - m_stream.Position, SeekOrigin.Current);
				}
				throw;
			}
		}
		if (num == 0)
		{
			return -1;
		}
		return m_singleChar[0];
	}

	/// <summary>Reads the specified number of characters from the current stream, returns the data in a character array, and advances the current position in accordance with the Encoding used and the specific character being read from the stream.</summary>
	/// <returns>A character array containing data read from the underlying stream. This might be less than the number of characters requested if the end of the stream is reached.</returns>
	/// <param name="count">The number of characters to read. </param>
	/// <exception cref="T:System.ArgumentException">The number of decoded characters to read is greater than <paramref name="count" />. This can happen if a Unicode decoder returns fallback characters or a surrogate pair.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is negative. </exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public virtual char[] ReadChars(int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (m_stream == null)
		{
			__Error.FileNotOpen();
		}
		if (count == 0)
		{
			return EmptyArray<char>.Value;
		}
		char[] array = new char[count];
		int num = InternalReadChars(array, 0, count);
		if (num != count)
		{
			char[] array2 = new char[num];
			Buffer.InternalBlockCopy(array, 0, array2, 0, 2 * num);
			array = array2;
		}
		return array;
	}

	/// <summary>Reads the specified number of bytes from the stream, starting from a specified point in the byte array. </summary>
	/// <returns>The number of bytes read into <paramref name="buffer" />. This might be less than the number of bytes requested if that many bytes are not available, or it might be zero if the end of the stream is reached.</returns>
	/// <param name="buffer">The buffer to read data into. </param>
	/// <param name="index">The starting point in the buffer at which to begin reading into the buffer. </param>
	/// <param name="count">The number of bytes to read. </param>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. -or-The number of decoded characters to read is greater than <paramref name="count" />. This can happen if a Unicode decoder returns fallback characters or a surrogate pair.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual int Read(byte[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", Environment.GetResourceString("Buffer cannot be null."));
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (buffer.Length - index < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		if (m_stream == null)
		{
			__Error.FileNotOpen();
		}
		return m_stream.Read(buffer, index, count);
	}

	/// <summary>Reads the specified number of bytes from the current stream into a byte array and advances the current position by that number of bytes.</summary>
	/// <returns>A byte array containing data read from the underlying stream. This might be less than the number of bytes requested if the end of the stream is reached.</returns>
	/// <param name="count">The number of bytes to read. This value must be 0 or a non-negative number or an exception will occur.</param>
	/// <exception cref="T:System.ArgumentException">The number of decoded characters to read is greater than <paramref name="count" />. This can happen if a Unicode decoder returns fallback characters or a surrogate pair.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is negative. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual byte[] ReadBytes(int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (m_stream == null)
		{
			__Error.FileNotOpen();
		}
		if (count == 0)
		{
			return EmptyArray<byte>.Value;
		}
		byte[] array = new byte[count];
		int num = 0;
		do
		{
			int num2 = m_stream.Read(array, num, count);
			if (num2 == 0)
			{
				break;
			}
			num += num2;
			count -= num2;
		}
		while (count > 0);
		if (num != array.Length)
		{
			byte[] array2 = new byte[num];
			Buffer.InternalBlockCopy(array, 0, array2, 0, num);
			array = array2;
		}
		return array;
	}

	/// <summary>Fills the internal buffer with the specified number of bytes read from the stream.</summary>
	/// <param name="numBytes">The number of bytes to be read. </param>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached before <paramref name="numBytes" /> could be read. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Requested <paramref name="numBytes" /> is larger than the internal buffer size.</exception>
	protected virtual void FillBuffer(int numBytes)
	{
		if (m_buffer != null && (numBytes < 0 || numBytes > m_buffer.Length))
		{
			throw new ArgumentOutOfRangeException("numBytes", Environment.GetResourceString("The number of bytes requested does not fit into BinaryReader's internal buffer."));
		}
		int num = 0;
		int num2 = 0;
		if (m_stream == null)
		{
			__Error.FileNotOpen();
		}
		if (numBytes == 1)
		{
			num2 = m_stream.ReadByte();
			if (num2 == -1)
			{
				__Error.EndOfFile();
			}
			m_buffer[0] = (byte)num2;
			return;
		}
		do
		{
			num2 = m_stream.Read(m_buffer, num, numBytes - num);
			if (num2 == 0)
			{
				__Error.EndOfFile();
			}
			num += num2;
		}
		while (num < numBytes);
	}

	/// <summary>Reads in a 32-bit integer in compressed format.</summary>
	/// <returns>A 32-bit integer in compressed format.</returns>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.FormatException">The stream is corrupted.</exception>
	protected internal int Read7BitEncodedInt()
	{
		int num = 0;
		int num2 = 0;
		byte b;
		do
		{
			if (num2 == 35)
			{
				throw new FormatException(Environment.GetResourceString("Too many bytes in what should have been a 7 bit encoded Int32."));
			}
			b = ReadByte();
			num |= (b & 0x7F) << num2;
			num2 += 7;
		}
		while ((b & 0x80) != 0);
		return num;
	}
}
