using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using Mono.Security;

namespace System.IO;

/// <summary>Writes primitive types in binary to a stream and supports writing strings in a specific encoding.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class BinaryWriter : IDisposable
{
	/// <summary>Specifies a <see cref="T:System.IO.BinaryWriter" /> with no backing store.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly BinaryWriter Null = new BinaryWriter();

	/// <summary>Holds the underlying stream.</summary>
	protected Stream OutStream;

	private byte[] _buffer;

	private Encoding _encoding;

	private Encoder _encoder;

	[OptionalField]
	private bool _leaveOpen;

	[OptionalField]
	private char[] _tmpOneCharBuffer;

	private byte[] _largeByteBuffer;

	private int _maxChars;

	private const int LargeByteBufferSize = 256;

	/// <summary>Gets the underlying stream of the <see cref="T:System.IO.BinaryWriter" />.</summary>
	/// <returns>The underlying stream associated with the BinaryWriter.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual Stream BaseStream
	{
		get
		{
			Flush();
			return OutStream;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.BinaryWriter" /> class that writes to a stream.</summary>
	protected BinaryWriter()
	{
		OutStream = Stream.Null;
		_buffer = new byte[16];
		_encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
		_encoder = _encoding.GetEncoder();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.BinaryWriter" /> class based on the specified stream and using UTF-8 encoding.</summary>
	/// <param name="output">The output stream. </param>
	/// <exception cref="T:System.ArgumentException">The stream does not support writing or is already closed. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="output" /> is null. </exception>
	public BinaryWriter(Stream output)
		: this(output, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true), leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.BinaryWriter" /> class based on the specified stream and character encoding.</summary>
	/// <param name="output">The output stream. </param>
	/// <param name="encoding">The character encoding to use. </param>
	/// <exception cref="T:System.ArgumentException">The stream does not support writing or is already closed. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="output" /> or <paramref name="encoding" /> is null. </exception>
	public BinaryWriter(Stream output, Encoding encoding)
		: this(output, encoding, leaveOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.BinaryWriter" /> class based on the specified stream and character encoding, and optionally leaves the stream open.</summary>
	/// <param name="output">The output stream.</param>
	/// <param name="encoding">The character encoding to use.</param>
	/// <param name="leaveOpen">true to leave the stream open after the <see cref="T:System.IO.BinaryWriter" /> object is disposed; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentException">The stream does not support writing or is already closed. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="output" /> or <paramref name="encoding" /> is null. </exception>
	public BinaryWriter(Stream output, Encoding encoding, bool leaveOpen)
	{
		if (output == null)
		{
			throw new ArgumentNullException("output");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (!output.CanWrite)
		{
			throw new ArgumentException(Environment.GetResourceString("Stream was not writable."));
		}
		OutStream = output;
		_buffer = new byte[16];
		_encoding = encoding;
		_encoder = _encoding.GetEncoder();
		_leaveOpen = leaveOpen;
	}

	/// <summary>Closes the current <see cref="T:System.IO.BinaryWriter" /> and the underlying stream.</summary>
	/// <filterpriority>1</filterpriority>
	public virtual void Close()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.BinaryWriter" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_leaveOpen)
			{
				OutStream.Flush();
			}
			else
			{
				OutStream.Close();
			}
		}
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.IO.BinaryWriter" /> class.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Clears all buffers for the current writer and causes any buffered data to be written to the underlying device.</summary>
	/// <filterpriority>1</filterpriority>
	public virtual void Flush()
	{
		OutStream.Flush();
	}

	/// <summary>Sets the position within the current stream.</summary>
	/// <returns>The position with the current stream.</returns>
	/// <param name="offset">A byte offset relative to <paramref name="origin" />. </param>
	/// <param name="origin">A field of <see cref="T:System.IO.SeekOrigin" /> indicating the reference point from which the new position is to be obtained. </param>
	/// <exception cref="T:System.IO.IOException">The file pointer was moved to an invalid location. </exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.IO.SeekOrigin" /> value is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual long Seek(int offset, SeekOrigin origin)
	{
		return OutStream.Seek(offset, origin);
	}

	/// <summary>Writes a one-byte Boolean value to the current stream, with 0 representing false and 1 representing true.</summary>
	/// <param name="value">The Boolean value to write (0 or 1). </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(bool value)
	{
		_buffer[0] = (byte)(value ? 1u : 0u);
		OutStream.Write(_buffer, 0, 1);
	}

	/// <summary>Writes an unsigned byte to the current stream and advances the stream position by one byte.</summary>
	/// <param name="value">The unsigned byte to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(byte value)
	{
		OutStream.WriteByte(value);
	}

	/// <summary>Writes a signed byte to the current stream and advances the stream position by one byte.</summary>
	/// <param name="value">The signed byte to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public virtual void Write(sbyte value)
	{
		OutStream.WriteByte((byte)value);
	}

	/// <summary>Writes a byte array to the underlying stream.</summary>
	/// <param name="buffer">A byte array containing the data to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(byte[] buffer)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		OutStream.Write(buffer, 0, buffer.Length);
	}

	/// <summary>Writes a region of a byte array to the current stream.</summary>
	/// <param name="buffer">A byte array containing the data to write. </param>
	/// <param name="index">The starting point in <paramref name="buffer" /> at which to begin writing. </param>
	/// <param name="count">The number of bytes to write. </param>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(byte[] buffer, int index, int count)
	{
		OutStream.Write(buffer, index, count);
	}

	/// <summary>Writes a Unicode character to the current stream and advances the current position of the stream in accordance with the Encoding used and the specific characters being written to the stream.</summary>
	/// <param name="ch">The non-surrogate, Unicode character to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="ch" /> is a single surrogate character.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe virtual void Write(char ch)
	{
		if (char.IsSurrogate(ch))
		{
			throw new ArgumentException(Environment.GetResourceString("Unicode surrogate characters must be written out as pairs together in the same call, not individually. Consider passing in a character array instead."));
		}
		int num = 0;
		fixed (byte* buffer = _buffer)
		{
			num = _encoder.GetBytes(&ch, 1, buffer, _buffer.Length, flush: true);
		}
		OutStream.Write(_buffer, 0, num);
	}

	/// <summary>Writes a character array to the current stream and advances the current position of the stream in accordance with the Encoding used and the specific characters being written to the stream.</summary>
	/// <param name="chars">A character array containing the data to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(char[] chars)
	{
		if (chars == null)
		{
			throw new ArgumentNullException("chars");
		}
		byte[] bytes = _encoding.GetBytes(chars, 0, chars.Length);
		OutStream.Write(bytes, 0, bytes.Length);
	}

	/// <summary>Writes a section of a character array to the current stream, and advances the current position of the stream in accordance with the Encoding used and perhaps the specific characters being written to the stream.</summary>
	/// <param name="chars">A character array containing the data to write. </param>
	/// <param name="index">The starting point in <paramref name="chars" /> from which to begin writing. </param>
	/// <param name="count">The number of characters to write. </param>
	/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(char[] chars, int index, int count)
	{
		byte[] bytes = _encoding.GetBytes(chars, index, count);
		OutStream.Write(bytes, 0, bytes.Length);
	}

	/// <summary>Writes an eight-byte floating-point value to the current stream and advances the stream position by eight bytes.</summary>
	/// <param name="value">The eight-byte floating-point value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public virtual void Write(double value)
	{
		OutStream.Write(BitConverterLE.GetBytes(value), 0, 8);
	}

	/// <summary>Writes a decimal value to the current stream and advances the stream position by sixteen bytes.</summary>
	/// <param name="value">The decimal value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(decimal value)
	{
		decimal.GetBytes(value, _buffer);
		OutStream.Write(_buffer, 0, 16);
	}

	/// <summary>Writes a two-byte signed integer to the current stream and advances the stream position by two bytes.</summary>
	/// <param name="value">The two-byte signed integer to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(short value)
	{
		_buffer[0] = (byte)value;
		_buffer[1] = (byte)(value >> 8);
		OutStream.Write(_buffer, 0, 2);
	}

	/// <summary>Writes a two-byte unsigned integer to the current stream and advances the stream position by two bytes.</summary>
	/// <param name="value">The two-byte unsigned integer to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public virtual void Write(ushort value)
	{
		_buffer[0] = (byte)value;
		_buffer[1] = (byte)(value >> 8);
		OutStream.Write(_buffer, 0, 2);
	}

	/// <summary>Writes a four-byte signed integer to the current stream and advances the stream position by four bytes.</summary>
	/// <param name="value">The four-byte signed integer to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(int value)
	{
		_buffer[0] = (byte)value;
		_buffer[1] = (byte)(value >> 8);
		_buffer[2] = (byte)(value >> 16);
		_buffer[3] = (byte)(value >> 24);
		OutStream.Write(_buffer, 0, 4);
	}

	/// <summary>Writes a four-byte unsigned integer to the current stream and advances the stream position by four bytes.</summary>
	/// <param name="value">The four-byte unsigned integer to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public virtual void Write(uint value)
	{
		_buffer[0] = (byte)value;
		_buffer[1] = (byte)(value >> 8);
		_buffer[2] = (byte)(value >> 16);
		_buffer[3] = (byte)(value >> 24);
		OutStream.Write(_buffer, 0, 4);
	}

	/// <summary>Writes an eight-byte signed integer to the current stream and advances the stream position by eight bytes.</summary>
	/// <param name="value">The eight-byte signed integer to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Write(long value)
	{
		_buffer[0] = (byte)value;
		_buffer[1] = (byte)(value >> 8);
		_buffer[2] = (byte)(value >> 16);
		_buffer[3] = (byte)(value >> 24);
		_buffer[4] = (byte)(value >> 32);
		_buffer[5] = (byte)(value >> 40);
		_buffer[6] = (byte)(value >> 48);
		_buffer[7] = (byte)(value >> 56);
		OutStream.Write(_buffer, 0, 8);
	}

	/// <summary>Writes an eight-byte unsigned integer to the current stream and advances the stream position by eight bytes.</summary>
	/// <param name="value">The eight-byte unsigned integer to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public virtual void Write(ulong value)
	{
		_buffer[0] = (byte)value;
		_buffer[1] = (byte)(value >> 8);
		_buffer[2] = (byte)(value >> 16);
		_buffer[3] = (byte)(value >> 24);
		_buffer[4] = (byte)(value >> 32);
		_buffer[5] = (byte)(value >> 40);
		_buffer[6] = (byte)(value >> 48);
		_buffer[7] = (byte)(value >> 56);
		OutStream.Write(_buffer, 0, 8);
	}

	/// <summary>Writes a four-byte floating-point value to the current stream and advances the stream position by four bytes.</summary>
	/// <param name="value">The four-byte floating-point value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public virtual void Write(float value)
	{
		OutStream.Write(BitConverterLE.GetBytes(value), 0, 4);
	}

	/// <summary>Writes a length-prefixed string to this stream in the current encoding of the <see cref="T:System.IO.BinaryWriter" />, and advances the current position of the stream in accordance with the encoding used and the specific characters being written to the stream.</summary>
	/// <param name="value">The value to write. </param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe virtual void Write(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		int byteCount = _encoding.GetByteCount(value);
		Write7BitEncodedInt(byteCount);
		if (_largeByteBuffer == null)
		{
			_largeByteBuffer = new byte[256];
			_maxChars = _largeByteBuffer.Length / _encoding.GetMaxByteCount(1);
		}
		if (byteCount <= _largeByteBuffer.Length)
		{
			_encoding.GetBytes(value, 0, value.Length, _largeByteBuffer, 0);
			OutStream.Write(_largeByteBuffer, 0, byteCount);
			return;
		}
		int num = 0;
		int num2 = value.Length;
		while (num2 > 0)
		{
			int num3 = ((num2 > _maxChars) ? _maxChars : num2);
			if (num < 0 || num3 < 0 || checked(num + num3) > value.Length)
			{
				throw new ArgumentOutOfRangeException("charCount");
			}
			int bytes;
			fixed (char* ptr = value)
			{
				fixed (byte* largeByteBuffer = _largeByteBuffer)
				{
					bytes = _encoder.GetBytes((char*)checked(unchecked((nuint)ptr) + unchecked((nuint)checked(unchecked((nint)num) * (nint)2))), num3, largeByteBuffer, _largeByteBuffer.Length, num3 == num2);
				}
			}
			OutStream.Write(_largeByteBuffer, 0, bytes);
			num += num3;
			num2 -= num3;
		}
	}

	/// <summary>Writes a 32-bit integer in a compressed format.</summary>
	/// <param name="value">The 32-bit integer to be written. </param>
	/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
	/// <exception cref="T:System.IO.IOException">The stream is closed. </exception>
	protected void Write7BitEncodedInt(int value)
	{
		uint num;
		for (num = (uint)value; num >= 128; num >>= 7)
		{
			Write((byte)(num | 0x80));
		}
		Write((byte)num);
	}
}
