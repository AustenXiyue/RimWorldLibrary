using System.Security;

namespace System.Text;

/// <summary>Represents a UTF-32 encoding of Unicode characters.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
public sealed class UTF32Encoding : Encoding
{
	[Serializable]
	internal class UTF32Decoder : DecoderNLS
	{
		internal int iChar;

		internal int readByteCount;

		internal override bool HasState => readByteCount != 0;

		public UTF32Decoder(UTF32Encoding encoding)
			: base(encoding)
		{
		}

		public override void Reset()
		{
			iChar = 0;
			readByteCount = 0;
			if (m_fallbackBuffer != null)
			{
				m_fallbackBuffer.Reset();
			}
		}
	}

	private bool emitUTF32ByteOrderMark;

	private bool isThrowException;

	private bool bigEndian;

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.UTF32Encoding" /> class.</summary>
	public UTF32Encoding()
		: this(bigEndian: false, byteOrderMark: true, throwOnInvalidCharacters: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.UTF32Encoding" /> class. Parameters specify whether to use the big endian byte order and whether to provide a Unicode byte order mark.</summary>
	/// <param name="bigEndian">true to use the big endian byte order (most significant byte first), or false to use the little endian byte order (least significant byte first). </param>
	/// <param name="byteOrderMark">true to specify that a Unicode byte order mark is provided; otherwise, false. </param>
	public UTF32Encoding(bool bigEndian, bool byteOrderMark)
		: this(bigEndian, byteOrderMark, throwOnInvalidCharacters: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.UTF32Encoding" /> class. Parameters specify whether to use the big endian byte order, whether to provide a Unicode byte order mark, and whether to throw an exception when an invalid encoding is detected.</summary>
	/// <param name="bigEndian">true to use the big endian byte order (most significant byte first), or false to use the little endian byte order (least significant byte first). </param>
	/// <param name="byteOrderMark">true to specify that a Unicode byte order mark is provided; otherwise, false. </param>
	/// <param name="throwOnInvalidCharacters">true to specify that an exception should be thrown when an invalid encoding is detected; otherwise, false. </param>
	public UTF32Encoding(bool bigEndian, bool byteOrderMark, bool throwOnInvalidCharacters)
		: base(bigEndian ? 12001 : 12000)
	{
		this.bigEndian = bigEndian;
		emitUTF32ByteOrderMark = byteOrderMark;
		isThrowException = throwOnInvalidCharacters;
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
	/// <param name="s">The <see cref="T:System.String" /> containing the set of characters to encode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="s" /> contains an invalid sequence of characters. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe override int GetByteCount(string s)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		fixed (char* chars = s)
		{
			return GetByteCount(chars, s.Length, null);
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
	[SecurityCritical]
	[CLSCompliant(false)]
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
	/// <returns>The actual number of bytes written at the location indicated by the <paramref name="bytes" /> parameter.</returns>
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
	[CLSCompliant(false)]
	[SecurityCritical]
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
	[SecurityCritical]
	[CLSCompliant(false)]
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
	internal unsafe override int GetByteCount(char* chars, int count, EncoderNLS encoder)
	{
		char* ptr = chars + count;
		char* charStart = chars;
		int num = 0;
		char c = '\0';
		EncoderFallbackBuffer encoderFallbackBuffer = null;
		if (encoder != null)
		{
			c = encoder.charLeftOver;
			encoderFallbackBuffer = encoder.FallbackBuffer;
			if (encoderFallbackBuffer.Remaining > 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Must complete Convert() operation or call Encoder.Reset() before calling GetBytes() or GetByteCount(). Encoder '{0}' fallback '{1}'.", EncodingName, encoder.Fallback.GetType()));
			}
		}
		else
		{
			encoderFallbackBuffer = encoderFallback.CreateFallbackBuffer();
		}
		encoderFallbackBuffer.InternalInitialize(charStart, ptr, encoder, setEncoder: false);
		while (true)
		{
			char c2;
			if ((c2 = encoderFallbackBuffer.InternalGetNextChar()) != 0 || chars < ptr)
			{
				if (c2 == '\0')
				{
					c2 = *chars;
					chars++;
				}
				if (c != 0)
				{
					if (char.IsLowSurrogate(c2))
					{
						c = '\0';
						num += 4;
					}
					else
					{
						chars--;
						encoderFallbackBuffer.InternalFallback(c, ref chars);
						c = '\0';
					}
				}
				else if (char.IsHighSurrogate(c2))
				{
					c = c2;
				}
				else if (char.IsLowSurrogate(c2))
				{
					encoderFallbackBuffer.InternalFallback(c2, ref chars);
				}
				else
				{
					num += 4;
				}
			}
			else
			{
				if ((encoder != null && !encoder.MustFlush) || c <= '\0')
				{
					break;
				}
				encoderFallbackBuffer.InternalFallback(c, ref chars);
				c = '\0';
			}
		}
		if (num < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Too many characters. The resulting number of bytes is larger than what can be returned as an int."));
		}
		return num;
	}

	[SecurityCritical]
	internal unsafe override int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS encoder)
	{
		char* ptr = chars;
		char* ptr2 = chars + charCount;
		byte* ptr3 = bytes;
		byte* ptr4 = bytes + byteCount;
		char c = '\0';
		EncoderFallbackBuffer encoderFallbackBuffer = null;
		if (encoder != null)
		{
			c = encoder.charLeftOver;
			encoderFallbackBuffer = encoder.FallbackBuffer;
			if (encoder.m_throwOnOverflow && encoderFallbackBuffer.Remaining > 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Must complete Convert() operation or call Encoder.Reset() before calling GetBytes() or GetByteCount(). Encoder '{0}' fallback '{1}'.", EncodingName, encoder.Fallback.GetType()));
			}
		}
		else
		{
			encoderFallbackBuffer = encoderFallback.CreateFallbackBuffer();
		}
		encoderFallbackBuffer.InternalInitialize(ptr, ptr2, encoder, setEncoder: true);
		while (true)
		{
			char c2;
			if ((c2 = encoderFallbackBuffer.InternalGetNextChar()) != 0 || chars < ptr2)
			{
				if (c2 == '\0')
				{
					c2 = *chars;
					chars++;
				}
				if (c != 0)
				{
					if (!char.IsLowSurrogate(c2))
					{
						chars--;
						encoderFallbackBuffer.InternalFallback(c, ref chars);
						c = '\0';
						continue;
					}
					uint surrogate = GetSurrogate(c, c2);
					c = '\0';
					if (bytes + 3 < ptr4)
					{
						if (bigEndian)
						{
							*(bytes++) = 0;
							*(bytes++) = (byte)(surrogate >> 16);
							*(bytes++) = (byte)(surrogate >> 8);
							*(bytes++) = (byte)surrogate;
						}
						else
						{
							*(bytes++) = (byte)surrogate;
							*(bytes++) = (byte)(surrogate >> 8);
							*(bytes++) = (byte)(surrogate >> 16);
							*(bytes++) = 0;
						}
						continue;
					}
					if (encoderFallbackBuffer.bFallingBack)
					{
						encoderFallbackBuffer.MovePrevious();
						encoderFallbackBuffer.MovePrevious();
					}
					else
					{
						chars -= 2;
					}
					ThrowBytesOverflow(encoder, bytes == ptr3);
					c = '\0';
				}
				else
				{
					if (char.IsHighSurrogate(c2))
					{
						c = c2;
						continue;
					}
					if (char.IsLowSurrogate(c2))
					{
						encoderFallbackBuffer.InternalFallback(c2, ref chars);
						continue;
					}
					if (bytes + 3 < ptr4)
					{
						if (bigEndian)
						{
							*(bytes++) = 0;
							*(bytes++) = 0;
							*(bytes++) = (byte)((uint)c2 >> 8);
							*(bytes++) = (byte)c2;
						}
						else
						{
							*(bytes++) = (byte)c2;
							*(bytes++) = (byte)((uint)c2 >> 8);
							*(bytes++) = 0;
							*(bytes++) = 0;
						}
						continue;
					}
					if (encoderFallbackBuffer.bFallingBack)
					{
						encoderFallbackBuffer.MovePrevious();
					}
					else
					{
						chars--;
					}
					ThrowBytesOverflow(encoder, bytes == ptr3);
				}
			}
			if ((encoder != null && !encoder.MustFlush) || c <= '\0')
			{
				break;
			}
			encoderFallbackBuffer.InternalFallback(c, ref chars);
			c = '\0';
		}
		if (encoder != null)
		{
			encoder.charLeftOver = c;
			encoder.m_charsUsed = (int)(chars - ptr);
		}
		return (int)(bytes - ptr3);
	}

	[SecurityCritical]
	internal unsafe override int GetCharCount(byte* bytes, int count, DecoderNLS baseDecoder)
	{
		UTF32Decoder uTF32Decoder = (UTF32Decoder)baseDecoder;
		int num = 0;
		byte* ptr = bytes + count;
		byte* byteStart = bytes;
		int num2 = 0;
		uint num3 = 0u;
		DecoderFallbackBuffer decoderFallbackBuffer = null;
		if (uTF32Decoder != null)
		{
			num2 = uTF32Decoder.readByteCount;
			num3 = (uint)uTF32Decoder.iChar;
			decoderFallbackBuffer = uTF32Decoder.FallbackBuffer;
		}
		else
		{
			decoderFallbackBuffer = decoderFallback.CreateFallbackBuffer();
		}
		decoderFallbackBuffer.InternalInitialize(byteStart, null);
		while (bytes < ptr && num >= 0)
		{
			if (bigEndian)
			{
				num3 <<= 8;
				num3 += *(bytes++);
			}
			else
			{
				num3 >>= 8;
				num3 += (uint)(*(bytes++) << 24);
			}
			num2++;
			if (num2 < 4)
			{
				continue;
			}
			num2 = 0;
			if (num3 > 1114111 || (num3 >= 55296 && num3 <= 57343))
			{
				byte[] bytes2 = ((!bigEndian) ? new byte[4]
				{
					(byte)num3,
					(byte)(num3 >> 8),
					(byte)(num3 >> 16),
					(byte)(num3 >> 24)
				} : new byte[4]
				{
					(byte)(num3 >> 24),
					(byte)(num3 >> 16),
					(byte)(num3 >> 8),
					(byte)num3
				});
				num += decoderFallbackBuffer.InternalFallback(bytes2, bytes);
				num3 = 0u;
			}
			else
			{
				if (num3 >= 65536)
				{
					num++;
				}
				num++;
				num3 = 0u;
			}
		}
		if (num2 > 0 && (uTF32Decoder == null || uTF32Decoder.MustFlush))
		{
			byte[] array = new byte[num2];
			if (bigEndian)
			{
				while (num2 > 0)
				{
					array[--num2] = (byte)num3;
					num3 >>= 8;
				}
			}
			else
			{
				while (num2 > 0)
				{
					array[--num2] = (byte)(num3 >> 24);
					num3 <<= 8;
				}
			}
			num += decoderFallbackBuffer.InternalFallback(array, bytes);
		}
		if (num < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Too many characters. The resulting number of bytes is larger than what can be returned as an int."));
		}
		return num;
	}

	[SecurityCritical]
	internal unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount, DecoderNLS baseDecoder)
	{
		UTF32Decoder uTF32Decoder = (UTF32Decoder)baseDecoder;
		char* ptr = chars;
		char* ptr2 = chars + charCount;
		byte* ptr3 = bytes;
		byte* ptr4 = bytes + byteCount;
		int num = 0;
		uint num2 = 0u;
		DecoderFallbackBuffer decoderFallbackBuffer = null;
		if (uTF32Decoder != null)
		{
			num = uTF32Decoder.readByteCount;
			num2 = (uint)uTF32Decoder.iChar;
			decoderFallbackBuffer = baseDecoder.FallbackBuffer;
		}
		else
		{
			decoderFallbackBuffer = decoderFallback.CreateFallbackBuffer();
		}
		decoderFallbackBuffer.InternalInitialize(bytes, chars + charCount);
		while (bytes < ptr4)
		{
			if (bigEndian)
			{
				num2 <<= 8;
				num2 += *(bytes++);
			}
			else
			{
				num2 >>= 8;
				num2 += (uint)(*(bytes++) << 24);
			}
			num++;
			if (num < 4)
			{
				continue;
			}
			num = 0;
			if (num2 > 1114111 || (num2 >= 55296 && num2 <= 57343))
			{
				byte[] bytes2 = ((!bigEndian) ? new byte[4]
				{
					(byte)num2,
					(byte)(num2 >> 8),
					(byte)(num2 >> 16),
					(byte)(num2 >> 24)
				} : new byte[4]
				{
					(byte)(num2 >> 24),
					(byte)(num2 >> 16),
					(byte)(num2 >> 8),
					(byte)num2
				});
				if (!decoderFallbackBuffer.InternalFallback(bytes2, bytes, ref chars))
				{
					bytes -= 4;
					num2 = 0u;
					decoderFallbackBuffer.InternalReset();
					ThrowCharsOverflow(uTF32Decoder, chars == ptr);
					break;
				}
				num2 = 0u;
				continue;
			}
			if (num2 >= 65536)
			{
				if (chars >= ptr2 - 1)
				{
					bytes -= 4;
					num2 = 0u;
					ThrowCharsOverflow(uTF32Decoder, chars == ptr);
					break;
				}
				*(chars++) = GetHighSurrogate(num2);
				num2 = GetLowSurrogate(num2);
			}
			else if (chars >= ptr2)
			{
				bytes -= 4;
				num2 = 0u;
				ThrowCharsOverflow(uTF32Decoder, chars == ptr);
				break;
			}
			*(chars++) = (char)num2;
			num2 = 0u;
		}
		if (num > 0 && (uTF32Decoder == null || uTF32Decoder.MustFlush))
		{
			byte[] array = new byte[num];
			int num3 = num;
			if (bigEndian)
			{
				while (num3 > 0)
				{
					array[--num3] = (byte)num2;
					num2 >>= 8;
				}
			}
			else
			{
				while (num3 > 0)
				{
					array[--num3] = (byte)(num2 >> 24);
					num2 <<= 8;
				}
			}
			if (!decoderFallbackBuffer.InternalFallback(array, bytes, ref chars))
			{
				decoderFallbackBuffer.InternalReset();
				ThrowCharsOverflow(uTF32Decoder, chars == ptr);
			}
			else
			{
				num = 0;
				num2 = 0u;
			}
		}
		if (uTF32Decoder != null)
		{
			uTF32Decoder.iChar = (int)num2;
			uTF32Decoder.readByteCount = num;
			uTF32Decoder.m_bytesUsed = (int)(bytes - ptr3);
		}
		return (int)(chars - ptr);
	}

	private uint GetSurrogate(char cHigh, char cLow)
	{
		return (uint)((cHigh - 55296) * 1024 + (cLow - 56320) + 65536);
	}

	private char GetHighSurrogate(uint iChar)
	{
		return (char)((iChar - 65536) / 1024 + 55296);
	}

	private char GetLowSurrogate(uint iChar)
	{
		return (char)((iChar - 65536) % 1024 + 56320);
	}

	/// <summary>Obtains a decoder that converts a UTF-32 encoded sequence of bytes into a sequence of Unicode characters.</summary>
	/// <returns>A <see cref="T:System.Text.Decoder" /> that converts a UTF-32 encoded sequence of bytes into a sequence of Unicode characters.</returns>
	/// <filterpriority>1</filterpriority>
	public override Decoder GetDecoder()
	{
		return new UTF32Decoder(this);
	}

	/// <summary>Obtains an encoder that converts a sequence of Unicode characters into a UTF-32 encoded sequence of bytes.</summary>
	/// <returns>A <see cref="T:System.Text.Encoder" /> that converts a sequence of Unicode characters into a UTF-32 encoded sequence of bytes.</returns>
	/// <filterpriority>1</filterpriority>
	public override Encoder GetEncoder()
	{
		return new EncoderNLS(this);
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
		num *= 4;
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
		int num = byteCount / 2 + 2;
		if (base.DecoderFallback.MaxCharCount > 2)
		{
			num *= base.DecoderFallback.MaxCharCount;
			num /= 2;
		}
		if (num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("byteCount", Environment.GetResourceString("Too many bytes. The resulting number of chars is larger than what can be returned as an int."));
		}
		return num;
	}

	/// <summary>Returns a Unicode byte order mark encoded in UTF-32 format, if the constructor for this instance requests a byte order mark.</summary>
	/// <returns>A byte array containing the Unicode byte order mark, if the constructor for this instance requests a byte order mark. Otherwise, this method returns a byte array of length zero.</returns>
	/// <filterpriority>1</filterpriority>
	public override byte[] GetPreamble()
	{
		if (emitUTF32ByteOrderMark)
		{
			if (!bigEndian)
			{
				return new byte[4] { 255, 254, 0, 0 };
			}
			return new byte[4] { 0, 0, 254, 255 };
		}
		return EmptyArray<byte>.Value;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Text.UTF32Encoding" /> object.</summary>
	/// <returns>true if <paramref name="value" /> is an instance of <see cref="T:System.Text.UTF32Encoding" /> and is equal to the current object; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to compare with the current object. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object value)
	{
		if (value is UTF32Encoding uTF32Encoding)
		{
			if (emitUTF32ByteOrderMark == uTF32Encoding.emitUTF32ByteOrderMark && bigEndian == uTF32Encoding.bigEndian && base.EncoderFallback.Equals(uTF32Encoding.EncoderFallback))
			{
				return base.DecoderFallback.Equals(uTF32Encoding.DecoderFallback);
			}
			return false;
		}
		return false;
	}

	/// <summary>Returns the hash code for the current instance.</summary>
	/// <returns>The hash code for the current <see cref="T:System.Text.UTF32Encoding" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	public override int GetHashCode()
	{
		return base.EncoderFallback.GetHashCode() + base.DecoderFallback.GetHashCode() + CodePage + (emitUTF32ByteOrderMark ? 4 : 0) + (bigEndian ? 8 : 0);
	}
}
