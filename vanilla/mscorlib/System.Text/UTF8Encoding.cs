using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System.Text;

/// <summary>Represents a UTF-8 encoding of Unicode characters.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public class UTF8Encoding : Encoding
{
	[Serializable]
	internal class UTF8Encoder : EncoderNLS, ISerializable
	{
		internal int surrogateChar;

		internal override bool HasState => surrogateChar != 0;

		public UTF8Encoder(UTF8Encoding encoding)
			: base(encoding)
		{
		}

		internal UTF8Encoder(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			m_encoding = (Encoding)info.GetValue("encoding", typeof(Encoding));
			surrogateChar = (int)info.GetValue("surrogateChar", typeof(int));
			try
			{
				m_fallback = (EncoderFallback)info.GetValue("m_fallback", typeof(EncoderFallback));
			}
			catch (SerializationException)
			{
				m_fallback = null;
			}
		}

		[SecurityCritical]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("encoding", m_encoding);
			info.AddValue("surrogateChar", surrogateChar);
			info.AddValue("m_fallback", m_fallback);
			info.AddValue("storedSurrogate", surrogateChar > 0);
			info.AddValue("mustFlush", value: false);
		}

		public override void Reset()
		{
			surrogateChar = 0;
			if (m_fallbackBuffer != null)
			{
				m_fallbackBuffer.Reset();
			}
		}
	}

	[Serializable]
	internal class UTF8Decoder : DecoderNLS, ISerializable
	{
		internal int bits;

		internal override bool HasState => bits != 0;

		public UTF8Decoder(UTF8Encoding encoding)
			: base(encoding)
		{
		}

		internal UTF8Decoder(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			m_encoding = (Encoding)info.GetValue("encoding", typeof(Encoding));
			try
			{
				bits = (int)info.GetValue("wbits", typeof(int));
				m_fallback = (DecoderFallback)info.GetValue("m_fallback", typeof(DecoderFallback));
			}
			catch (SerializationException)
			{
				bits = 0;
				m_fallback = null;
			}
		}

		[SecurityCritical]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("encoding", m_encoding);
			info.AddValue("wbits", bits);
			info.AddValue("m_fallback", m_fallback);
			info.AddValue("bits", 0);
			info.AddValue("trailCount", 0);
			info.AddValue("isSurrogate", value: false);
			info.AddValue("byteSequence", 0);
		}

		public override void Reset()
		{
			bits = 0;
			if (m_fallbackBuffer != null)
			{
				m_fallbackBuffer.Reset();
			}
		}
	}

	private const int UTF8_CODEPAGE = 65001;

	private bool emitUTF8Identifier;

	private bool isThrowException;

	private const int FinalByte = 536870912;

	private const int SupplimentarySeq = 268435456;

	private const int ThreeByteSeq = 134217728;

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.UTF8Encoding" /> class.</summary>
	public UTF8Encoding()
		: this(encoderShouldEmitUTF8Identifier: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.UTF8Encoding" /> class. A parameter specifies whether to provide a Unicode byte order mark.</summary>
	/// <param name="encoderShouldEmitUTF8Identifier">true to specify that a Unicode byte order mark is provided; otherwise, false. </param>
	public UTF8Encoding(bool encoderShouldEmitUTF8Identifier)
		: this(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.UTF8Encoding" /> class. Parameters specify whether to provide a Unicode byte order mark and whether to throw an exception when an invalid encoding is detected.</summary>
	/// <param name="encoderShouldEmitUTF8Identifier">true to specify that a Unicode byte order mark is provided; otherwise, false. </param>
	/// <param name="throwOnInvalidBytes">true to specify that an exception be thrown when an invalid encoding is detected; otherwise, false. </param>
	public UTF8Encoding(bool encoderShouldEmitUTF8Identifier, bool throwOnInvalidBytes)
		: base(65001)
	{
		emitUTF8Identifier = encoderShouldEmitUTF8Identifier;
		isThrowException = throwOnInvalidBytes;
		if (isThrowException)
		{
			SetDefaultFallbacks();
		}
	}

	internal override void SetDefaultFallbacks()
	{
		if (isThrowException)
		{
			encoderFallback = EncoderFallback.ExceptionFallback;
			decoderFallback = DecoderFallback.ExceptionFallback;
		}
		else
		{
			encoderFallback = new EncoderReplacementFallback("\ufffd");
			decoderFallback = new DecoderReplacementFallback("\ufffd");
		}
	}

	/// <summary>Calculates the number of bytes produced by encoding a set of characters from the specified character array.</summary>
	/// <returns>The number of bytes produced by encoding the specified characters.</returns>
	/// <param name="chars">The character array containing the set of characters to encode. </param>
	/// <param name="index">The index of the first character to encode. </param>
	/// <param name="count">The number of characters to encode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="chars" />.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="chars" /> contains an invalid sequence of characters. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe override int GetByteCount(char[] chars, int index, int count)
	{
		if (chars == null)
		{
			throw new ArgumentNullException("chars", Environment.GetResourceString("Array cannot be null."));
		}
		if (index < 0 || count < 0)
		{
			throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
		}
		if (chars.Length - index < count)
		{
			throw new ArgumentOutOfRangeException("chars", Environment.GetResourceString("Index and count must refer to a location within the buffer."));
		}
		if (chars.Length == 0)
		{
			return 0;
		}
		fixed (char* ptr = chars)
		{
			return GetByteCount(ptr + index, count, null);
		}
	}

	/// <summary>Calculates the number of bytes produced by encoding the characters in the specified <see cref="T:System.String" />.</summary>
	/// <returns>The number of bytes produced by encoding the specified characters.</returns>
	/// <param name="chars">The <see cref="T:System.String" /> containing the set of characters to encode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="chars" /> contains an invalid sequence of characters. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe override int GetByteCount(string chars)
	{
		if (chars == null)
		{
			throw new ArgumentNullException("s");
		}
		fixed (char* chars2 = chars)
		{
			return GetByteCount(chars2, chars.Length, null);
		}
	}

	/// <summary>Calculates the number of bytes produced by encoding a set of characters starting at the specified character pointer.</summary>
	/// <returns>The number of bytes produced by encoding the specified characters.</returns>
	/// <param name="chars">A pointer to the first character to encode. </param>
	/// <param name="count">The number of characters to encode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="chars" /> contains an invalid sequence of characters. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	[ComVisible(false)]
	[SecurityCritical]
	public unsafe override int GetByteCount(char* chars, int count)
	{
		if (chars == null)
		{
			throw new ArgumentNullException("chars", Environment.GetResourceString("Array cannot be null."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		return GetByteCount(chars, count, null);
	}

	/// <summary>Encodes a set of characters from the specified <see cref="T:System.String" /> into the specified byte array.</summary>
	/// <returns>The actual number of bytes written into <paramref name="bytes" />.</returns>
	/// <param name="s">The <see cref="T:System.String" /> containing the set of characters to encode. </param>
	/// <param name="charIndex">The index of the first character to encode. </param>
	/// <param name="charCount">The number of characters to encode. </param>
	/// <param name="bytes">The byte array to contain the resulting sequence of bytes. </param>
	/// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null.-or- <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charIndex" /> or <paramref name="charCount" /> or <paramref name="byteIndex" /> is less than zero.-or- <paramref name="charIndex" /> and <paramref name="charCount" /> do not denote a valid range in <paramref name="chars" />.-or- <paramref name="byteIndex" /> is not a valid index in <paramref name="bytes" />. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="s" /> contains an invalid sequence of characters.-or- <paramref name="bytes" /> does not have enough capacity from <paramref name="byteIndex" /> to the end of the array to accommodate the resulting bytes. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe override int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
	{
		if (s == null || bytes == null)
		{
			throw new ArgumentNullException((s == null) ? "s" : "bytes", Environment.GetResourceString("Array cannot be null."));
		}
		if (charIndex < 0 || charCount < 0)
		{
			throw new ArgumentOutOfRangeException((charIndex < 0) ? "charIndex" : "charCount", Environment.GetResourceString("Non-negative number required."));
		}
		if (s.Length - charIndex < charCount)
		{
			throw new ArgumentOutOfRangeException("s", Environment.GetResourceString("Index and count must refer to a location within the string."));
		}
		if (byteIndex < 0 || byteIndex > bytes.Length)
		{
			throw new ArgumentOutOfRangeException("byteIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		int byteCount = bytes.Length - byteIndex;
		if (bytes.Length == 0)
		{
			bytes = new byte[1];
		}
		fixed (char* ptr = s)
		{
			fixed (byte* ptr2 = bytes)
			{
				return GetBytes(ptr + charIndex, charCount, ptr2 + byteIndex, byteCount, null);
			}
		}
	}

	/// <summary>Encodes a set of characters from the specified character array into the specified byte array.</summary>
	/// <returns>The actual number of bytes written into <paramref name="bytes" />.</returns>
	/// <param name="chars">The character array containing the set of characters to encode. </param>
	/// <param name="charIndex">The index of the first character to encode. </param>
	/// <param name="charCount">The number of characters to encode. </param>
	/// <param name="bytes">The byte array to contain the resulting sequence of bytes. </param>
	/// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null.-or- <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charIndex" /> or <paramref name="charCount" /> or <paramref name="byteIndex" /> is less than zero.-or- <paramref name="charIndex" /> and <paramref name="charCount" /> do not denote a valid range in <paramref name="chars" />.-or- <paramref name="byteIndex" /> is not a valid index in <paramref name="bytes" />. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="chars" /> contains an invalid sequence of characters.-or- <paramref name="bytes" /> does not have enough capacity from <paramref name="byteIndex" /> to the end of the array to accommodate the resulting bytes. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
	{
		if (chars == null || bytes == null)
		{
			throw new ArgumentNullException((chars == null) ? "chars" : "bytes", Environment.GetResourceString("Array cannot be null."));
		}
		if (charIndex < 0 || charCount < 0)
		{
			throw new ArgumentOutOfRangeException((charIndex < 0) ? "charIndex" : "charCount", Environment.GetResourceString("Non-negative number required."));
		}
		if (chars.Length - charIndex < charCount)
		{
			throw new ArgumentOutOfRangeException("chars", Environment.GetResourceString("Index and count must refer to a location within the buffer."));
		}
		if (byteIndex < 0 || byteIndex > bytes.Length)
		{
			throw new ArgumentOutOfRangeException("byteIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (chars.Length == 0)
		{
			return 0;
		}
		int byteCount = bytes.Length - byteIndex;
		if (bytes.Length == 0)
		{
			bytes = new byte[1];
		}
		fixed (char* ptr = chars)
		{
			fixed (byte* ptr2 = bytes)
			{
				return GetBytes(ptr + charIndex, charCount, ptr2 + byteIndex, byteCount, null);
			}
		}
	}

	/// <summary>Encodes a set of characters starting at the specified character pointer into a sequence of bytes that are stored starting at the specified byte pointer.</summary>
	/// <returns>The actual number of bytes written at the location indicated by <paramref name="bytes" />.</returns>
	/// <param name="chars">A pointer to the first character to encode. </param>
	/// <param name="charCount">The number of characters to encode. </param>
	/// <param name="bytes">A pointer to the location at which to start writing the resulting sequence of bytes. </param>
	/// <param name="byteCount">The maximum number of bytes to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null.-or- <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charCount" /> or <paramref name="byteCount" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="chars" /> contains an invalid sequence of characters.-or- <paramref name="byteCount" /> is less than the resulting number of bytes. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecurityCritical]
	[CLSCompliant(false)]
	[ComVisible(false)]
	public unsafe override int GetBytes(char* chars, int charCount, byte* bytes, int byteCount)
	{
		if (bytes == null || chars == null)
		{
			throw new ArgumentNullException((bytes == null) ? "bytes" : "chars", Environment.GetResourceString("Array cannot be null."));
		}
		if (charCount < 0 || byteCount < 0)
		{
			throw new ArgumentOutOfRangeException((charCount < 0) ? "charCount" : "byteCount", Environment.GetResourceString("Non-negative number required."));
		}
		return GetBytes(chars, charCount, bytes, byteCount, null);
	}

	/// <summary>Calculates the number of characters produced by decoding a sequence of bytes from the specified byte array.</summary>
	/// <returns>The number of characters produced by decoding the specified sequence of bytes.</returns>
	/// <param name="bytes">The byte array containing the sequence of bytes to decode. </param>
	/// <param name="index">The index of the first byte to decode. </param>
	/// <param name="count">The number of bytes to decode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="bytes" />.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="bytes" /> contains an invalid sequence of bytes. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe override int GetCharCount(byte[] bytes, int index, int count)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes", Environment.GetResourceString("Array cannot be null."));
		}
		if (index < 0 || count < 0)
		{
			throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
		}
		if (bytes.Length - index < count)
		{
			throw new ArgumentOutOfRangeException("bytes", Environment.GetResourceString("Index and count must refer to a location within the buffer."));
		}
		if (bytes.Length == 0)
		{
			return 0;
		}
		fixed (byte* ptr = bytes)
		{
			return GetCharCount(ptr + index, count, null);
		}
	}

	/// <summary>Calculates the number of characters produced by decoding a sequence of bytes starting at the specified byte pointer.</summary>
	/// <returns>The number of characters produced by decoding the specified sequence of bytes.</returns>
	/// <param name="bytes">A pointer to the first byte to decode. </param>
	/// <param name="count">The number of bytes to decode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="bytes" /> contains an invalid sequence of bytes. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	[CLSCompliant(false)]
	[SecurityCritical]
	public unsafe override int GetCharCount(byte* bytes, int count)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes", Environment.GetResourceString("Array cannot be null."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		return GetCharCount(bytes, count, null);
	}

	/// <summary>Decodes a sequence of bytes from the specified byte array into the specified character array.</summary>
	/// <returns>The actual number of characters written into <paramref name="chars" />.</returns>
	/// <param name="bytes">The byte array containing the sequence of bytes to decode. </param>
	/// <param name="byteIndex">The index of the first byte to decode. </param>
	/// <param name="byteCount">The number of bytes to decode. </param>
	/// <param name="chars">The character array to contain the resulting set of characters. </param>
	/// <param name="charIndex">The index at which to start writing the resulting set of characters. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null.-or- <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="byteIndex" /> or <paramref name="byteCount" /> or <paramref name="charIndex" /> is less than zero.-or- <paramref name="byteindex" /> and <paramref name="byteCount" /> do not denote a valid range in <paramref name="bytes" />.-or- <paramref name="charIndex" /> is not a valid index in <paramref name="chars" />. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="bytes" /> contains an invalid sequence of bytes.-or- <paramref name="chars" /> does not have enough capacity from <paramref name="charIndex" /> to the end of the array to accommodate the resulting characters. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
	{
		if (bytes == null || chars == null)
		{
			throw new ArgumentNullException((bytes == null) ? "bytes" : "chars", Environment.GetResourceString("Array cannot be null."));
		}
		if (byteIndex < 0 || byteCount < 0)
		{
			throw new ArgumentOutOfRangeException((byteIndex < 0) ? "byteIndex" : "byteCount", Environment.GetResourceString("Non-negative number required."));
		}
		if (bytes.Length - byteIndex < byteCount)
		{
			throw new ArgumentOutOfRangeException("bytes", Environment.GetResourceString("Index and count must refer to a location within the buffer."));
		}
		if (charIndex < 0 || charIndex > chars.Length)
		{
			throw new ArgumentOutOfRangeException("charIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (bytes.Length == 0)
		{
			return 0;
		}
		int charCount = chars.Length - charIndex;
		if (chars.Length == 0)
		{
			chars = new char[1];
		}
		fixed (byte* ptr = bytes)
		{
			fixed (char* ptr2 = chars)
			{
				return GetChars(ptr + byteIndex, byteCount, ptr2 + charIndex, charCount, null);
			}
		}
	}

	/// <summary>Decodes a sequence of bytes starting at the specified byte pointer into a set of characters that are stored starting at the specified character pointer.</summary>
	/// <returns>The actual number of characters written at the location indicated by <paramref name="chars" />.</returns>
	/// <param name="bytes">A pointer to the first byte to decode. </param>
	/// <param name="byteCount">The number of bytes to decode. </param>
	/// <param name="chars">A pointer to the location at which to start writing the resulting set of characters. </param>
	/// <param name="charCount">The maximum number of characters to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null.-or- <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="byteCount" /> or <paramref name="charCount" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="bytes" /> contains an invalid sequence of bytes.-or- <paramref name="charCount" /> is less than the resulting number of characters. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	[ComVisible(false)]
	[SecurityCritical]
	public unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount)
	{
		if (bytes == null || chars == null)
		{
			throw new ArgumentNullException((bytes == null) ? "bytes" : "chars", Environment.GetResourceString("Array cannot be null."));
		}
		if (charCount < 0 || byteCount < 0)
		{
			throw new ArgumentOutOfRangeException((charCount < 0) ? "charCount" : "byteCount", Environment.GetResourceString("Non-negative number required."));
		}
		return GetChars(bytes, byteCount, chars, charCount, null);
	}

	/// <summary>Decodes a range of bytes from a byte array into a string.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the results of decoding the specified sequence of bytes.</returns>
	/// <param name="bytes">The byte array containing the sequence of bytes to decode. </param>
	/// <param name="index">The index of the first byte to decode. </param>
	/// <param name="count">The number of bytes to decode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="bytes" />. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="bytes" /> contains an invalid sequence of bytes. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	[ComVisible(false)]
	public unsafe override string GetString(byte[] bytes, int index, int count)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes", Environment.GetResourceString("Array cannot be null."));
		}
		if (index < 0 || count < 0)
		{
			throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
		}
		if (bytes.Length - index < count)
		{
			throw new ArgumentOutOfRangeException("bytes", Environment.GetResourceString("Index and count must refer to a location within the buffer."));
		}
		if (bytes.Length == 0)
		{
			return string.Empty;
		}
		fixed (byte* ptr = bytes)
		{
			return string.CreateStringFromEncoding(ptr + index, count, this);
		}
	}

	[SecurityCritical]
	internal unsafe override int GetByteCount(char* chars, int count, EncoderNLS baseEncoder)
	{
		EncoderFallbackBuffer encoderFallbackBuffer = null;
		char* chars2 = chars;
		char* ptr = chars2 + count;
		int num = count;
		int num2 = 0;
		if (baseEncoder != null)
		{
			UTF8Encoder uTF8Encoder = (UTF8Encoder)baseEncoder;
			num2 = uTF8Encoder.surrogateChar;
			if (uTF8Encoder.InternalHasFallbackBuffer)
			{
				encoderFallbackBuffer = uTF8Encoder.FallbackBuffer;
				if (encoderFallbackBuffer.Remaining > 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Must complete Convert() operation or call Encoder.Reset() before calling GetBytes() or GetByteCount(). Encoder '{0}' fallback '{1}'.", EncodingName, uTF8Encoder.Fallback.GetType()));
				}
				encoderFallbackBuffer.InternalInitialize(chars, ptr, uTF8Encoder, setEncoder: false);
			}
		}
		while (true)
		{
			if (chars2 >= ptr)
			{
				if (num2 == 0)
				{
					num2 = encoderFallbackBuffer?.InternalGetNextChar() ?? '\0';
					if (num2 <= 0)
					{
						goto IL_00e6;
					}
					num++;
				}
				else
				{
					if (encoderFallbackBuffer == null || !encoderFallbackBuffer.bFallingBack)
					{
						goto IL_00e6;
					}
					num2 = encoderFallbackBuffer.InternalGetNextChar();
					num++;
					if (InRange(num2, 56320, 57343))
					{
						num2 = 65533;
						num++;
						goto IL_0165;
					}
					if (num2 <= 0)
					{
						num--;
						break;
					}
				}
			}
			else
			{
				if (num2 > 0)
				{
					char ch = *chars2;
					num++;
					if (InRange(ch, 56320, 57343))
					{
						num2 = 65533;
						chars2++;
					}
					goto IL_0165;
				}
				if (encoderFallbackBuffer != null)
				{
					num2 = encoderFallbackBuffer.InternalGetNextChar();
					if (num2 > 0)
					{
						num++;
						goto IL_0149;
					}
				}
				num2 = *chars2;
				chars2++;
			}
			goto IL_0149;
			IL_0149:
			if (InRange(num2, 55296, 56319))
			{
				num--;
				continue;
			}
			goto IL_0165;
			IL_0165:
			if (InRange(num2, 55296, 57343))
			{
				if (encoderFallbackBuffer == null)
				{
					encoderFallbackBuffer = ((baseEncoder != null) ? baseEncoder.FallbackBuffer : encoderFallback.CreateFallbackBuffer());
					encoderFallbackBuffer.InternalInitialize(chars, chars + count, baseEncoder, setEncoder: false);
				}
				encoderFallbackBuffer.InternalFallback((char)num2, ref chars2);
				num--;
				num2 = 0;
				continue;
			}
			if (num2 > 127)
			{
				if (num2 > 2047)
				{
					num++;
				}
				num++;
			}
			if (encoderFallbackBuffer != null && (num2 = encoderFallbackBuffer.InternalGetNextChar()) != 0)
			{
				num++;
				goto IL_0149;
			}
			int num3 = PtrDiff(ptr, chars2);
			if (num3 <= 13)
			{
				char* ptr2 = ptr;
				while (chars2 < ptr2)
				{
					num2 = *chars2;
					chars2++;
					if (num2 <= 127)
					{
						continue;
					}
					goto IL_0149;
				}
				break;
			}
			char* ptr3 = chars2 + num3 - 7;
			while (chars2 < ptr3)
			{
				num2 = *chars2;
				chars2++;
				if (num2 > 127)
				{
					if (num2 > 2047)
					{
						if ((num2 & 0xF800) == 55296)
						{
							goto IL_0395;
						}
						num++;
					}
					num++;
				}
				if (((int)chars2 & 2) != 0)
				{
					num2 = *chars2;
					chars2++;
					if (num2 > 127)
					{
						if (num2 > 2047)
						{
							if ((num2 & 0xF800) == 55296)
							{
								goto IL_0395;
							}
							num++;
						}
						num++;
					}
				}
				for (; chars2 < ptr3; chars2 += 4)
				{
					num2 = *(int*)chars2;
					int num4 = *(int*)(chars2 + 2);
					if (((num2 | num4) & -8323200) != 0)
					{
						if (((num2 | num4) & -134154240) != 0)
						{
							goto IL_0376;
						}
						if ((num2 & -8388608) != 0)
						{
							num++;
						}
						if ((num2 & 0xFF80) != 0)
						{
							num++;
						}
						if ((num4 & -8388608) != 0)
						{
							num++;
						}
						if ((num4 & 0xFF80) != 0)
						{
							num++;
						}
					}
					chars2 += 4;
					num2 = *(int*)chars2;
					num4 = *(int*)(chars2 + 2);
					if (((num2 | num4) & -8323200) == 0)
					{
						continue;
					}
					if (((num2 | num4) & -134154240) == 0)
					{
						if ((num2 & -8388608) != 0)
						{
							num++;
						}
						if ((num2 & 0xFF80) != 0)
						{
							num++;
						}
						if ((num4 & -8388608) != 0)
						{
							num++;
						}
						if ((num4 & 0xFF80) != 0)
						{
							num++;
						}
						continue;
					}
					goto IL_0376;
				}
				break;
				IL_0395:
				if (num2 > 2047)
				{
					if (InRange(num2, 55296, 57343))
					{
						int ch2 = *chars2;
						if (num2 > 56319 || !InRange(ch2, 56320, 57343))
						{
							chars2--;
							break;
						}
						chars2++;
					}
					num++;
				}
				num++;
				continue;
				IL_0376:
				num2 = (BitConverter.IsLittleEndian ? ((ushort)num2) : (num2 >>> 16));
				chars2++;
				if (num2 <= 127)
				{
					continue;
				}
				goto IL_0395;
			}
			num2 = 0;
			continue;
			IL_00e6:
			if (num2 <= 0 || (baseEncoder != null && !baseEncoder.MustFlush))
			{
				break;
			}
			num++;
			goto IL_0165;
		}
		return num;
	}

	[SecurityCritical]
	private unsafe static int PtrDiff(char* a, char* b)
	{
		return (int)((uint)((byte*)a - (byte*)b) >> 1);
	}

	[SecurityCritical]
	private unsafe static int PtrDiff(byte* a, byte* b)
	{
		return (int)(a - b);
	}

	private static bool InRange(int ch, int start, int end)
	{
		return (uint)(ch - start) <= (uint)(end - start);
	}

	[SecurityCritical]
	internal unsafe override int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS baseEncoder)
	{
		UTF8Encoder uTF8Encoder = null;
		EncoderFallbackBuffer encoderFallbackBuffer = null;
		char* chars2 = chars;
		byte* ptr = bytes;
		char* ptr2 = chars2 + charCount;
		byte* ptr3 = ptr + byteCount;
		int num = 0;
		if (baseEncoder != null)
		{
			uTF8Encoder = (UTF8Encoder)baseEncoder;
			num = uTF8Encoder.surrogateChar;
			if (uTF8Encoder.InternalHasFallbackBuffer)
			{
				encoderFallbackBuffer = uTF8Encoder.FallbackBuffer;
				if (encoderFallbackBuffer.Remaining > 0 && uTF8Encoder.m_throwOnOverflow)
				{
					throw new ArgumentException(Environment.GetResourceString("Must complete Convert() operation or call Encoder.Reset() before calling GetBytes() or GetByteCount(). Encoder '{0}' fallback '{1}'.", EncodingName, uTF8Encoder.Fallback.GetType()));
				}
				encoderFallbackBuffer.InternalInitialize(chars, ptr2, uTF8Encoder, setEncoder: true);
			}
		}
		while (true)
		{
			if (chars2 >= ptr2)
			{
				if (num == 0)
				{
					num = encoderFallbackBuffer?.InternalGetNextChar() ?? '\0';
					if (num <= 0)
					{
						goto IL_00ef;
					}
				}
				else
				{
					if (encoderFallbackBuffer == null || !encoderFallbackBuffer.bFallingBack)
					{
						goto IL_00ef;
					}
					int num2 = num;
					num = encoderFallbackBuffer.InternalGetNextChar();
					if (InRange(num, 56320, 57343))
					{
						num = num + (num2 << 10) + -56613888;
						goto IL_0167;
					}
					if (num <= 0)
					{
						break;
					}
				}
			}
			else
			{
				if (num > 0)
				{
					int num3 = *chars2;
					if (InRange(num3, 56320, 57343))
					{
						num = num3 + (num << 10) + -56613888;
						chars2++;
					}
					goto IL_0167;
				}
				if (encoderFallbackBuffer != null)
				{
					num = encoderFallbackBuffer.InternalGetNextChar();
					if (num > 0)
					{
						goto IL_0151;
					}
				}
				num = *chars2;
				chars2++;
			}
			goto IL_0151;
			IL_0167:
			if (InRange(num, 55296, 57343))
			{
				if (encoderFallbackBuffer == null)
				{
					encoderFallbackBuffer = ((baseEncoder != null) ? baseEncoder.FallbackBuffer : encoderFallback.CreateFallbackBuffer());
					encoderFallbackBuffer.InternalInitialize(chars, ptr2, baseEncoder, setEncoder: true);
				}
				encoderFallbackBuffer.InternalFallback((char)num, ref chars2);
				num = 0;
				continue;
			}
			int num4 = 1;
			if (num > 127)
			{
				if (num > 2047)
				{
					if (num > 65535)
					{
						num4++;
					}
					num4++;
				}
				num4++;
			}
			if (ptr > ptr3 - num4)
			{
				if (encoderFallbackBuffer != null && encoderFallbackBuffer.bFallingBack)
				{
					encoderFallbackBuffer.MovePrevious();
					if (num > 65535)
					{
						encoderFallbackBuffer.MovePrevious();
					}
				}
				else
				{
					chars2--;
					if (num > 65535)
					{
						chars2--;
					}
				}
				ThrowBytesOverflow(uTF8Encoder, ptr == bytes);
				num = 0;
				break;
			}
			if (num <= 127)
			{
				*ptr = (byte)num;
			}
			else
			{
				int num5;
				if (num <= 2047)
				{
					num5 = (byte)(-64 | (num >> 6));
				}
				else
				{
					if (num <= 65535)
					{
						num5 = (byte)(-32 | (num >> 12));
					}
					else
					{
						*ptr = (byte)(-16 | (num >> 18));
						ptr++;
						num5 = -128 | ((num >> 12) & 0x3F);
					}
					*ptr = (byte)num5;
					ptr++;
					num5 = -128 | ((num >> 6) & 0x3F);
				}
				*ptr = (byte)num5;
				ptr++;
				*ptr = (byte)(-128 | (num & 0x3F));
			}
			ptr++;
			if (encoderFallbackBuffer == null || (num = encoderFallbackBuffer.InternalGetNextChar()) == 0)
			{
				int num6 = PtrDiff(ptr2, chars2);
				int num7 = PtrDiff(ptr3, ptr);
				if (num6 <= 13)
				{
					if (num7 < num6)
					{
						num = 0;
						continue;
					}
					char* ptr4 = ptr2;
					while (chars2 < ptr4)
					{
						num = *chars2;
						chars2++;
						if (num <= 127)
						{
							*ptr = (byte)num;
							ptr++;
							continue;
						}
						goto IL_0151;
					}
					num = 0;
					break;
				}
				if (num7 < num6)
				{
					num6 = num7;
				}
				char* ptr5 = chars2 + num6 - 5;
				while (chars2 < ptr5)
				{
					num = *chars2;
					chars2++;
					if (num <= 127)
					{
						*ptr = (byte)num;
						ptr++;
						if (((int)chars2 & 2) != 0)
						{
							num = *chars2;
							chars2++;
							if (num > 127)
							{
								goto IL_0427;
							}
							*ptr = (byte)num;
							ptr++;
						}
						while (chars2 < ptr5)
						{
							num = *(int*)chars2;
							int num8 = *(int*)(chars2 + 2);
							if (((num | num8) & -8323200) == 0)
							{
								if (!BitConverter.IsLittleEndian)
								{
									*ptr = (byte)(num >> 16);
									ptr[1] = (byte)num;
									chars2 += 4;
									ptr[2] = (byte)(num8 >> 16);
									ptr[3] = (byte)num8;
									ptr += 4;
								}
								else
								{
									*ptr = (byte)num;
									ptr[1] = (byte)(num >> 16);
									chars2 += 4;
									ptr[2] = (byte)num8;
									ptr[3] = (byte)(num8 >> 16);
									ptr += 4;
								}
								continue;
							}
							goto IL_03fa;
						}
						continue;
					}
					goto IL_0427;
					IL_03fa:
					num = (BitConverter.IsLittleEndian ? ((ushort)num) : (num >>> 16));
					chars2++;
					if (num <= 127)
					{
						*ptr = (byte)num;
						ptr++;
						continue;
					}
					goto IL_0427;
					IL_0427:
					int num9;
					if (num <= 2047)
					{
						num9 = -64 | (num >> 6);
					}
					else
					{
						if (!InRange(num, 55296, 57343))
						{
							num9 = -32 | (num >> 12);
						}
						else
						{
							if (num > 56319)
							{
								chars2--;
								break;
							}
							num9 = *chars2;
							chars2++;
							if (!InRange(num9, 56320, 57343))
							{
								chars2 -= 2;
								break;
							}
							num = num9 + (num << 10) + -56613888;
							*ptr = (byte)(-16 | (num >> 18));
							ptr++;
							num9 = -128 | ((num >> 12) & 0x3F);
						}
						*ptr = (byte)num9;
						ptr5--;
						ptr++;
						num9 = -128 | ((num >> 6) & 0x3F);
					}
					*ptr = (byte)num9;
					ptr5--;
					ptr++;
					*ptr = (byte)(-128 | (num & 0x3F));
					ptr++;
				}
				num = 0;
				continue;
			}
			goto IL_0151;
			IL_00ef:
			if (num <= 0 || (uTF8Encoder != null && !uTF8Encoder.MustFlush))
			{
				break;
			}
			goto IL_0167;
			IL_0151:
			if (InRange(num, 55296, 56319))
			{
				continue;
			}
			goto IL_0167;
		}
		if (uTF8Encoder != null)
		{
			uTF8Encoder.surrogateChar = num;
			uTF8Encoder.m_charsUsed = (int)(chars2 - chars);
		}
		return (int)(ptr - bytes);
	}

	[SecurityCritical]
	internal unsafe override int GetCharCount(byte* bytes, int count, DecoderNLS baseDecoder)
	{
		byte* ptr = bytes;
		byte* ptr2 = ptr + count;
		int num = count;
		int num2 = 0;
		DecoderFallbackBuffer decoderFallbackBuffer = null;
		if (baseDecoder != null)
		{
			num2 = ((UTF8Decoder)baseDecoder).bits;
			num -= num2 >> 30;
		}
		while (ptr < ptr2)
		{
			if (num2 != 0)
			{
				int num3 = *ptr;
				ptr++;
				if ((num3 & -64) != 128)
				{
					ptr--;
					num += num2 >> 30;
				}
				else
				{
					num2 = (num2 << 6) | (num3 & 0x3F);
					if ((num2 & 0x20000000) != 0)
					{
						if ((num2 & 0x101F0000) == 268435456)
						{
							num--;
						}
						goto IL_017c;
					}
					if ((num2 & 0x10000000) != 0)
					{
						if ((num2 & 0x800000) != 0 || InRange(num2 & 0x1F0, 16, 256))
						{
							continue;
						}
					}
					else if ((num2 & 0x3E0) != 0 && (num2 & 0x3E0) != 864)
					{
						continue;
					}
				}
				goto IL_00c3;
			}
			num2 = *ptr;
			ptr++;
			goto IL_0106;
			IL_017c:
			int num4 = PtrDiff(ptr2, ptr);
			if (num4 <= 13)
			{
				byte* ptr3 = ptr2;
				while (ptr < ptr3)
				{
					num2 = *ptr;
					ptr++;
					if (num2 <= 127)
					{
						continue;
					}
					goto IL_0106;
				}
				num2 = 0;
				break;
			}
			byte* ptr4 = ptr + num4 - 7;
			while (true)
			{
				if (ptr < ptr4)
				{
					num2 = *ptr;
					ptr++;
					if (num2 > 127)
					{
						goto IL_026f;
					}
					if (((int)ptr & 1) != 0)
					{
						num2 = *ptr;
						ptr++;
						if (num2 > 127)
						{
							goto IL_026f;
						}
					}
					if (((int)ptr & 2) != 0)
					{
						num2 = *(ushort*)ptr;
						if ((num2 & 0x8080) != 0)
						{
							goto IL_024e;
						}
						ptr += 2;
					}
					while (ptr < ptr4)
					{
						num2 = *(int*)ptr;
						int num5 = *(int*)(ptr + 4);
						if (((num2 | num5) & -2139062144) == 0)
						{
							ptr += 8;
							if (ptr >= ptr4)
							{
								break;
							}
							num2 = *(int*)ptr;
							num5 = *(int*)(ptr + 4);
							if (((num2 | num5) & -2139062144) == 0)
							{
								ptr += 8;
								continue;
							}
						}
						goto IL_0238;
					}
				}
				num2 = 0;
				break;
				IL_024e:
				num2 = (BitConverter.IsLittleEndian ? (num2 & 0xFF) : (num2 >>> 8));
				ptr++;
				if (num2 <= 127)
				{
					continue;
				}
				goto IL_026f;
				IL_026f:
				int num6 = *ptr;
				ptr++;
				if ((num2 & 0x40) != 0 && (num6 & -64) == 128)
				{
					num6 &= 0x3F;
					if ((num2 & 0x20) != 0)
					{
						num6 |= (num2 & 0xF) << 6;
						if ((num2 & 0x10) != 0)
						{
							num2 = *ptr;
							if (InRange(num6 >> 4, 1, 16) && (num2 & -64) == 128)
							{
								num6 = (num6 << 6) | (num2 & 0x3F);
								num2 = ptr[1];
								if ((num2 & -64) == 128)
								{
									ptr += 2;
									num--;
									goto IL_0328;
								}
							}
						}
						else
						{
							num2 = *ptr;
							if ((num6 & 0x3E0) != 0 && (num6 & 0x3E0) != 864 && (num2 & -64) == 128)
							{
								ptr++;
								num--;
								goto IL_0328;
							}
						}
					}
					else if ((num2 & 0x1E) != 0)
					{
						goto IL_0328;
					}
				}
				ptr -= 2;
				num2 = 0;
				break;
				IL_0238:
				num2 = (BitConverter.IsLittleEndian ? (num2 & 0xFF) : (num2 >>> 16));
				goto IL_024e;
				IL_0328:
				num--;
			}
			continue;
			IL_00c3:
			if (decoderFallbackBuffer == null)
			{
				decoderFallbackBuffer = ((baseDecoder != null) ? baseDecoder.FallbackBuffer : decoderFallback.CreateFallbackBuffer());
				decoderFallbackBuffer.InternalInitialize(bytes, null);
			}
			num += FallbackInvalidByteSequence(ptr, num2, decoderFallbackBuffer);
			num2 = 0;
			continue;
			IL_0106:
			if (num2 > 127)
			{
				num--;
				if ((num2 & 0x40) != 0)
				{
					if ((num2 & 0x20) != 0)
					{
						if ((num2 & 0x10) == 0)
						{
							num2 = (num2 & 0xF) | 0x48228000;
							num--;
							continue;
						}
						num2 &= 0xF;
						if (num2 <= 4)
						{
							num2 |= 0x504D0C00;
							num--;
							continue;
						}
						num2 |= 0xF0;
					}
					else
					{
						num2 &= 0x1F;
						if (num2 > 1)
						{
							num2 |= 0x800000;
							continue;
						}
						num2 |= 0xC0;
					}
				}
				goto IL_00c3;
			}
			goto IL_017c;
		}
		if (num2 != 0)
		{
			num += num2 >> 30;
			if (baseDecoder == null || baseDecoder.MustFlush)
			{
				if (decoderFallbackBuffer == null)
				{
					decoderFallbackBuffer = ((baseDecoder != null) ? baseDecoder.FallbackBuffer : decoderFallback.CreateFallbackBuffer());
					decoderFallbackBuffer.InternalInitialize(bytes, null);
				}
				num += FallbackInvalidByteSequence(ptr, num2, decoderFallbackBuffer);
			}
		}
		return num;
	}

	[SecurityCritical]
	internal unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount, DecoderNLS baseDecoder)
	{
		byte* pSrc = bytes;
		char* pTarget = chars;
		byte* ptr = pSrc + byteCount;
		char* ptr2 = pTarget + charCount;
		int num = 0;
		DecoderFallbackBuffer decoderFallbackBuffer = null;
		if (baseDecoder != null)
		{
			num = ((UTF8Decoder)baseDecoder).bits;
		}
		while (pSrc < ptr)
		{
			if (num != 0)
			{
				int num2 = *pSrc;
				pSrc++;
				if ((num2 & -64) != 128)
				{
					pSrc--;
				}
				else
				{
					num = (num << 6) | (num2 & 0x3F);
					if ((num & 0x20000000) != 0)
					{
						if ((num & 0x101F0000) > 268435456 && pTarget < ptr2)
						{
							*pTarget = (char)(((num >> 10) & 0x7FF) + -10304);
							pTarget++;
							num = (num & 0x3FF) + 56320;
						}
						goto IL_01de;
					}
					if ((num & 0x10000000) != 0)
					{
						if ((num & 0x800000) != 0 || InRange(num & 0x1F0, 16, 256))
						{
							continue;
						}
					}
					else if ((num & 0x3E0) != 0 && (num & 0x3E0) != 864)
					{
						continue;
					}
				}
				goto IL_00f9;
			}
			num = *pSrc;
			pSrc++;
			goto IL_015d;
			IL_01de:
			if (pTarget >= ptr2)
			{
				num &= 0x1FFFFF;
				if (num > 127)
				{
					if (num > 2047)
					{
						if (num >= 56320 && num <= 57343)
						{
							pSrc--;
							pTarget--;
						}
						else if (num > 65535)
						{
							pSrc--;
						}
						pSrc--;
					}
					pSrc--;
				}
				pSrc--;
				ThrowCharsOverflow(baseDecoder, pTarget == chars);
				num = 0;
				break;
			}
			*pTarget = (char)num;
			pTarget++;
			int num3 = PtrDiff(ptr2, pTarget);
			int num4 = PtrDiff(ptr, pSrc);
			if (num4 <= 13)
			{
				if (num3 < num4)
				{
					num = 0;
					continue;
				}
				byte* ptr3 = ptr;
				while (pSrc < ptr3)
				{
					num = *pSrc;
					pSrc++;
					if (num <= 127)
					{
						*pTarget = (char)num;
						pTarget++;
						continue;
					}
					goto IL_015d;
				}
				num = 0;
				break;
			}
			if (num3 < num4)
			{
				num4 = num3;
			}
			char* ptr4 = pTarget + num4 - 7;
			while (true)
			{
				if (pTarget < ptr4)
				{
					num = *pSrc;
					pSrc++;
					if (num > 127)
					{
						goto IL_04d0;
					}
					*pTarget = (char)num;
					pTarget++;
					if (((int)pSrc & 1) != 0)
					{
						num = *pSrc;
						pSrc++;
						if (num > 127)
						{
							goto IL_04d0;
						}
						*pTarget = (char)num;
						pTarget++;
					}
					if (((int)pSrc & 2) != 0)
					{
						num = *(ushort*)pSrc;
						if ((num & 0x8080) != 0)
						{
							goto IL_049f;
						}
						if (!BitConverter.IsLittleEndian)
						{
							*pTarget = (char)((num >> 8) & 0x7F);
							pSrc += 2;
							pTarget[1] = (char)(num & 0x7F);
							pTarget += 2;
						}
						else
						{
							*pTarget = (char)(num & 0x7F);
							pSrc += 2;
							pTarget[1] = (char)((num >> 8) & 0x7F);
							pTarget += 2;
						}
					}
					while (pTarget < ptr4)
					{
						num = *(int*)pSrc;
						int num5 = *(int*)(pSrc + 4);
						if (((num | num5) & -2139062144) == 0)
						{
							if (!BitConverter.IsLittleEndian)
							{
								*pTarget = (char)((num >> 24) & 0x7F);
								pTarget[1] = (char)((num >> 16) & 0x7F);
								pTarget[2] = (char)((num >> 8) & 0x7F);
								pTarget[3] = (char)(num & 0x7F);
								pSrc += 8;
								pTarget[4] = (char)((num5 >> 24) & 0x7F);
								pTarget[5] = (char)((num5 >> 16) & 0x7F);
								pTarget[6] = (char)((num5 >> 8) & 0x7F);
								pTarget[7] = (char)(num5 & 0x7F);
								pTarget += 8;
							}
							else
							{
								*pTarget = (char)(num & 0x7F);
								pTarget[1] = (char)((num >> 8) & 0x7F);
								pTarget[2] = (char)((num >> 16) & 0x7F);
								pTarget[3] = (char)((num >> 24) & 0x7F);
								pSrc += 8;
								pTarget[4] = (char)(num5 & 0x7F);
								pTarget[5] = (char)((num5 >> 8) & 0x7F);
								pTarget[6] = (char)((num5 >> 16) & 0x7F);
								pTarget[7] = (char)((num5 >> 24) & 0x7F);
								pTarget += 8;
							}
							continue;
						}
						goto IL_0485;
					}
				}
				num = 0;
				break;
				IL_0485:
				num = (BitConverter.IsLittleEndian ? (num & 0xFF) : (num >>> 16));
				goto IL_049f;
				IL_05f3:
				*pTarget = (char)num;
				pTarget++;
				ptr4--;
				continue;
				IL_049f:
				num = (BitConverter.IsLittleEndian ? (num & 0xFF) : (num >>> 8));
				pSrc++;
				if (num <= 127)
				{
					*pTarget = (char)num;
					pTarget++;
					continue;
				}
				goto IL_04d0;
				IL_04d0:
				int num6 = *pSrc;
				pSrc++;
				if ((num & 0x40) != 0 && (num6 & -64) == 128)
				{
					num6 &= 0x3F;
					if ((num & 0x20) != 0)
					{
						num6 |= (num & 0xF) << 6;
						if ((num & 0x10) != 0)
						{
							num = *pSrc;
							if (InRange(num6 >> 4, 1, 16) && (num & -64) == 128)
							{
								num6 = (num6 << 6) | (num & 0x3F);
								num = pSrc[1];
								if ((num & -64) == 128)
								{
									pSrc += 2;
									num = (num6 << 6) | (num & 0x3F);
									*pTarget = (char)(((num >> 10) & 0x7FF) + -10304);
									pTarget++;
									num = (num & 0x3FF) + -9216;
									ptr4--;
									goto IL_05f3;
								}
							}
						}
						else
						{
							num = *pSrc;
							if ((num6 & 0x3E0) != 0 && (num6 & 0x3E0) != 864 && (num & -64) == 128)
							{
								pSrc++;
								num = (num6 << 6) | (num & 0x3F);
								ptr4--;
								goto IL_05f3;
							}
						}
					}
					else
					{
						num &= 0x1F;
						if (num > 1)
						{
							num = (num << 6) | num6;
							goto IL_05f3;
						}
					}
				}
				pSrc -= 2;
				num = 0;
				break;
			}
			continue;
			IL_015d:
			if (num > 127)
			{
				if ((num & 0x40) != 0)
				{
					if ((num & 0x20) != 0)
					{
						if ((num & 0x10) == 0)
						{
							num = (num & 0xF) | 0x48228000;
							continue;
						}
						num &= 0xF;
						if (num <= 4)
						{
							num |= 0x504D0C00;
							continue;
						}
						num |= 0xF0;
					}
					else
					{
						num &= 0x1F;
						if (num > 1)
						{
							num |= 0x800000;
							continue;
						}
						num |= 0xC0;
					}
				}
				goto IL_00f9;
			}
			goto IL_01de;
			IL_00f9:
			if (decoderFallbackBuffer == null)
			{
				decoderFallbackBuffer = ((baseDecoder != null) ? baseDecoder.FallbackBuffer : decoderFallback.CreateFallbackBuffer());
				decoderFallbackBuffer.InternalInitialize(bytes, ptr2);
			}
			if (!FallbackInvalidByteSequence(ref pSrc, num, decoderFallbackBuffer, ref pTarget))
			{
				decoderFallbackBuffer.InternalReset();
				ThrowCharsOverflow(baseDecoder, pTarget == chars);
				num = 0;
				break;
			}
			num = 0;
		}
		if (num != 0 && (baseDecoder == null || baseDecoder.MustFlush))
		{
			if (decoderFallbackBuffer == null)
			{
				decoderFallbackBuffer = ((baseDecoder != null) ? baseDecoder.FallbackBuffer : decoderFallback.CreateFallbackBuffer());
				decoderFallbackBuffer.InternalInitialize(bytes, ptr2);
			}
			if (!FallbackInvalidByteSequence(ref pSrc, num, decoderFallbackBuffer, ref pTarget))
			{
				decoderFallbackBuffer.InternalReset();
				ThrowCharsOverflow(baseDecoder, pTarget == chars);
			}
			num = 0;
		}
		if (baseDecoder != null)
		{
			((UTF8Decoder)baseDecoder).bits = num;
			baseDecoder.m_bytesUsed = (int)(pSrc - bytes);
		}
		return PtrDiff(pTarget, chars);
	}

	[SecurityCritical]
	private unsafe bool FallbackInvalidByteSequence(ref byte* pSrc, int ch, DecoderFallbackBuffer fallback, ref char* pTarget)
	{
		byte* pSrc2 = pSrc;
		byte[] bytesUnknown = GetBytesUnknown(ref pSrc2, ch);
		if (!fallback.InternalFallback(bytesUnknown, pSrc, ref pTarget))
		{
			pSrc = pSrc2;
			return false;
		}
		return true;
	}

	[SecurityCritical]
	private unsafe int FallbackInvalidByteSequence(byte* pSrc, int ch, DecoderFallbackBuffer fallback)
	{
		byte[] bytesUnknown = GetBytesUnknown(ref pSrc, ch);
		return fallback.InternalFallback(bytesUnknown, pSrc);
	}

	[SecurityCritical]
	private unsafe byte[] GetBytesUnknown(ref byte* pSrc, int ch)
	{
		byte[] array = null;
		if (ch < 256 && ch >= 0)
		{
			pSrc--;
			return new byte[1] { (byte)ch };
		}
		if ((ch & 0x18000000) == 0)
		{
			pSrc--;
			return new byte[1] { (byte)((ch & 0x1F) | 0xC0) };
		}
		if ((ch & 0x10000000) != 0)
		{
			if ((ch & 0x800000) != 0)
			{
				pSrc -= 3;
				return new byte[3]
				{
					(byte)(((ch >> 12) & 7) | 0xF0),
					(byte)(((ch >> 6) & 0x3F) | 0x80),
					(byte)((ch & 0x3F) | 0x80)
				};
			}
			if ((ch & 0x20000) != 0)
			{
				pSrc -= 2;
				return new byte[2]
				{
					(byte)(((ch >> 6) & 7) | 0xF0),
					(byte)((ch & 0x3F) | 0x80)
				};
			}
			pSrc--;
			return new byte[1] { (byte)((ch & 7) | 0xF0) };
		}
		if ((ch & 0x800000) != 0)
		{
			pSrc -= 2;
			return new byte[2]
			{
				(byte)(((ch >> 6) & 0xF) | 0xE0),
				(byte)((ch & 0x3F) | 0x80)
			};
		}
		pSrc--;
		return new byte[1] { (byte)((ch & 0xF) | 0xE0) };
	}

	/// <summary>Obtains a decoder that converts a UTF-8 encoded sequence of bytes into a sequence of Unicode characters.</summary>
	/// <returns>A <see cref="T:System.Text.Decoder" /> that converts a UTF-8 encoded sequence of bytes into a sequence of Unicode characters.</returns>
	/// <filterpriority>1</filterpriority>
	public override Decoder GetDecoder()
	{
		return new UTF8Decoder(this);
	}

	/// <summary>Obtains an encoder that converts a sequence of Unicode characters into a UTF-8 encoded sequence of bytes.</summary>
	/// <returns>A <see cref="T:System.Text.Encoder" /> that converts a sequence of Unicode characters into a UTF-8 encoded sequence of bytes.</returns>
	/// <filterpriority>1</filterpriority>
	public override Encoder GetEncoder()
	{
		return new UTF8Encoder(this);
	}

	/// <summary>Calculates the maximum number of bytes produced by encoding the specified number of characters.</summary>
	/// <returns>The maximum number of bytes produced by encoding the specified number of characters.</returns>
	/// <param name="charCount">The number of characters to encode. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charCount" /> is less than zero.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public override int GetMaxByteCount(int charCount)
	{
		if (charCount < 0)
		{
			throw new ArgumentOutOfRangeException("charCount", Environment.GetResourceString("Non-negative number required."));
		}
		long num = (long)charCount + 1L;
		if (base.EncoderFallback.MaxCharCount > 1)
		{
			num *= base.EncoderFallback.MaxCharCount;
		}
		num *= 3;
		if (num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("charCount", Environment.GetResourceString("Too many characters. The resulting number of bytes is larger than what can be returned as an int."));
		}
		return (int)num;
	}

	/// <summary>Calculates the maximum number of characters produced by decoding the specified number of bytes.</summary>
	/// <returns>The maximum number of characters produced by decoding the specified number of bytes.</returns>
	/// <param name="byteCount">The number of bytes to decode. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="byteCount" /> is less than zero.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public override int GetMaxCharCount(int byteCount)
	{
		if (byteCount < 0)
		{
			throw new ArgumentOutOfRangeException("byteCount", Environment.GetResourceString("Non-negative number required."));
		}
		long num = (long)byteCount + 1L;
		if (base.DecoderFallback.MaxCharCount > 1)
		{
			num *= base.DecoderFallback.MaxCharCount;
		}
		if (num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("byteCount", Environment.GetResourceString("Too many bytes. The resulting number of chars is larger than what can be returned as an int."));
		}
		return (int)num;
	}

	/// <summary>Returns a Unicode byte order mark encoded in UTF-8 format, if the constructor for this instance requests a byte order mark.</summary>
	/// <returns>A byte array containing the Unicode byte order mark, if the constructor for this instance requests a byte order mark. Otherwise, this method returns a byte array of length zero.</returns>
	/// <filterpriority>1</filterpriority>
	public override byte[] GetPreamble()
	{
		if (emitUTF8Identifier)
		{
			return new byte[3] { 239, 187, 191 };
		}
		return EmptyArray<byte>.Value;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Text.UTF8Encoding" /> object.</summary>
	/// <returns>true if <paramref name="value" /> is an instance of <see cref="T:System.Text.UTF8Encoding" /> and is equal to the current object; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to compare with the current instance. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object value)
	{
		if (value is UTF8Encoding uTF8Encoding)
		{
			if (emitUTF8Identifier == uTF8Encoding.emitUTF8Identifier && base.EncoderFallback.Equals(uTF8Encoding.EncoderFallback))
			{
				return base.DecoderFallback.Equals(uTF8Encoding.DecoderFallback);
			}
			return false;
		}
		return false;
	}

	/// <summary>Returns the hash code for the current instance.</summary>
	/// <returns>The hash code for the current instance.</returns>
	/// <filterpriority>1</filterpriority>
	public override int GetHashCode()
	{
		return base.EncoderFallback.GetHashCode() + base.DecoderFallback.GetHashCode() + 65001 + (emitUTF8Identifier ? 1 : 0);
	}
}
