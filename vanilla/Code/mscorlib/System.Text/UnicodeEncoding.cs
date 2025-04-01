using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System.Text;

/// <summary>Represents a UTF-16 encoding of Unicode characters. </summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public class UnicodeEncoding : Encoding
{
	[Serializable]
	private class Decoder : DecoderNLS, ISerializable
	{
		internal int lastByte = -1;

		internal char lastChar;

		internal override bool HasState
		{
			get
			{
				if (lastByte == -1)
				{
					return lastChar != '\0';
				}
				return true;
			}
		}

		public Decoder(UnicodeEncoding encoding)
			: base(encoding)
		{
		}

		internal Decoder(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			lastByte = (int)info.GetValue("lastByte", typeof(int));
			try
			{
				m_encoding = (Encoding)info.GetValue("m_encoding", typeof(Encoding));
				lastChar = (char)info.GetValue("lastChar", typeof(char));
				m_fallback = (DecoderFallback)info.GetValue("m_fallback", typeof(DecoderFallback));
			}
			catch (SerializationException)
			{
				bool bigEndian = (bool)info.GetValue("bigEndian", typeof(bool));
				m_encoding = new UnicodeEncoding(bigEndian, byteOrderMark: false);
			}
		}

		[SecurityCritical]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("m_encoding", m_encoding);
			info.AddValue("m_fallback", m_fallback);
			info.AddValue("lastChar", lastChar);
			info.AddValue("lastByte", lastByte);
			info.AddValue("bigEndian", ((UnicodeEncoding)m_encoding).bigEndian);
		}

		public override void Reset()
		{
			lastByte = -1;
			lastChar = '\0';
			if (m_fallbackBuffer != null)
			{
				m_fallbackBuffer.Reset();
			}
		}
	}

	[OptionalField(VersionAdded = 2)]
	internal bool isThrowException;

	internal bool bigEndian;

	internal bool byteOrderMark = true;

	/// <summary>Represents the Unicode version 2.0 character size in bytes. This field is a constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const int CharSize = 2;

	private static readonly ulong highLowPatternMask = (ulong)(-2882066263381583872L | (BitConverter.IsLittleEndian ? 288230376218820608L : 4398046512128L));

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.UnicodeEncoding" /> class.</summary>
	public UnicodeEncoding()
		: this(bigEndian: false, byteOrderMark: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.UnicodeEncoding" /> class. Parameters specify whether to use the big endian byte order and whether to provide a Unicode byte order mark.</summary>
	/// <param name="bigEndian">true to use the big endian byte order (most significant byte first), or false to use the little endian byte order (least significant byte first). </param>
	/// <param name="byteOrderMark">true to specify that a Unicode byte order mark is provided; otherwise, false. </param>
	public UnicodeEncoding(bool bigEndian, bool byteOrderMark)
		: this(bigEndian, byteOrderMark, throwOnInvalidBytes: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.UnicodeEncoding" /> class. Parameters specify whether to use the big endian byte order, whether to provide a Unicode byte order mark, and whether to throw an exception when an invalid encoding is detected.</summary>
	/// <param name="bigEndian">true to use the big endian byte order (most significant byte first); false to use the little endian byte order (least significant byte first). </param>
	/// <param name="byteOrderMark">true to specify that a Unicode byte order mark is provided; otherwise, false. </param>
	/// <param name="throwOnInvalidBytes">true to specify that an exception should be thrown when an invalid encoding is detected; otherwise, false. </param>
	public UnicodeEncoding(bool bigEndian, bool byteOrderMark, bool throwOnInvalidBytes)
		: base(bigEndian ? 1201 : 1200)
	{
		isThrowException = throwOnInvalidBytes;
		this.bigEndian = bigEndian;
		this.byteOrderMark = byteOrderMark;
		if (isThrowException)
		{
			SetDefaultFallbacks();
		}
	}

	[OnDeserializing]
	private void OnDeserializing(StreamingContext ctx)
	{
		isThrowException = false;
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
	///   <paramref name="chars" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="chars" />.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="chars" /> contains an invalid sequence of characters. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
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
	///   <paramref name="s" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="s" /> contains an invalid sequence of characters. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
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
	///   <paramref name="chars" /> is null (Nothing in Visual Basic .NET). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled and <paramref name="chars" /> contains an invalid sequence of characters. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecurityCritical]
	[CLSCompliant(false)]
	[ComVisible(false)]
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
	///   <paramref name="s" /> is null (Nothing).-or- <paramref name="bytes" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charIndex" /> or <paramref name="charCount" /> or <paramref name="byteIndex" /> is less than zero.-or- <paramref name="charIndex" /> and <paramref name="charCount" /> do not denote a valid range in <paramref name="chars" />.-or- <paramref name="byteIndex" /> is not a valid index in <paramref name="bytes" />. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="s" /> contains an invalid sequence of characters.-or- <paramref name="bytes" /> does not have enough capacity from <paramref name="byteIndex" /> to the end of the array to accommodate the resulting bytes. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
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
	///   <paramref name="chars" /> is null (Nothing).-or- <paramref name="bytes" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charIndex" /> or <paramref name="charCount" /> or <paramref name="byteIndex" /> is less than zero.-or- <paramref name="charIndex" /> and <paramref name="charCount" /> do not denote a valid range in <paramref name="chars" />.-or- <paramref name="byteIndex" /> is not a valid index in <paramref name="bytes" />. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="chars" /> contains an invalid sequence of characters.-or- <paramref name="bytes" /> does not have enough capacity from <paramref name="byteIndex" /> to the end of the array to accommodate the resulting bytes. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
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
	///   <paramref name="chars" /> is null (Nothing).-or- <paramref name="bytes" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charCount" /> or <paramref name="byteCount" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="chars" /> contains an invalid sequence of characters.-or- <paramref name="byteCount" /> is less than the resulting number of bytes. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	[SecurityCritical]
	[CLSCompliant(false)]
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
	///   <paramref name="bytes" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="bytes" />.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="bytes" /> contains an invalid sequence of bytes. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
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
	///   <paramref name="bytes" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="bytes" /> contains an invalid sequence of bytes. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecurityCritical]
	[CLSCompliant(false)]
	[ComVisible(false)]
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
	///   <paramref name="bytes" /> is null (Nothing).-or- <paramref name="chars" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="byteIndex" /> or <paramref name="byteCount" /> or <paramref name="charIndex" /> is less than zero.-or- <paramref name="byteindex" /> and <paramref name="byteCount" /> do not denote a valid range in <paramref name="bytes" />.-or- <paramref name="charIndex" /> is not a valid index in <paramref name="chars" />. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="bytes" /> contains an invalid sequence of bytes.-or- <paramref name="chars" /> does not have enough capacity from <paramref name="charIndex" /> to the end of the array to accommodate the resulting characters. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
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
	/// <returns>The actual number of characters written at the location indicated by the <paramref name="chars" /> parameter.</returns>
	/// <param name="bytes">A pointer to the first byte to decode. </param>
	/// <param name="byteCount">The number of bytes to decode. </param>
	/// <param name="chars">A pointer to the location at which to start writing the resulting set of characters. </param>
	/// <param name="charCount">The maximum number of characters to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null (Nothing).-or- <paramref name="chars" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="byteCount" /> or <paramref name="charCount" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="bytes" /> contains an invalid sequence of bytes.-or- <paramref name="charCount" /> is less than the resulting number of characters. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecurityCritical]
	[CLSCompliant(false)]
	[ComVisible(false)]
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
	/// <returns>A <see cref="T:System.String" /> object containing the results of decoding the specified sequence of bytes.</returns>
	/// <param name="bytes">The byte array containing the sequence of bytes to decode. </param>
	/// <param name="index">The index of the first byte to decode. </param>
	/// <param name="count">The number of bytes to decode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="bytes" />. </exception>
	/// <exception cref="T:System.ArgumentException">Error detection is enabled, and <paramref name="bytes" /> contains an invalid sequence of bytes. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
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
		int num = count << 1;
		if (num < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Too many characters. The resulting number of bytes is larger than what can be returned as an int."));
		}
		char* charStart = chars;
		char* ptr = chars + count;
		char c = '\0';
		bool flag = false;
		ulong* ptr2 = (ulong*)(ptr - 3);
		EncoderFallbackBuffer encoderFallbackBuffer = null;
		if (encoder != null)
		{
			c = encoder.charLeftOver;
			if (c > '\0')
			{
				num += 2;
			}
			if (encoder.InternalHasFallbackBuffer)
			{
				encoderFallbackBuffer = encoder.FallbackBuffer;
				if (encoderFallbackBuffer.Remaining > 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Must complete Convert() operation or call Encoder.Reset() before calling GetBytes() or GetByteCount(). Encoder '{0}' fallback '{1}'.", EncodingName, encoder.Fallback.GetType()));
				}
				encoderFallbackBuffer.InternalInitialize(charStart, ptr, encoder, setEncoder: false);
			}
		}
		while (true)
		{
			char num2 = encoderFallbackBuffer?.InternalGetNextChar() ?? '\0';
			char c2 = num2;
			if (num2 != 0 || chars < ptr)
			{
				if (c2 == '\0')
				{
					if ((bigEndian ^ BitConverter.IsLittleEndian) && c == '\0' && ((int)chars & 3) == 0)
					{
						ulong* ptr3;
						for (ptr3 = (ulong*)chars; ptr3 < ptr2; ptr3++)
						{
							if ((0x8000800080008000uL & *ptr3) != 0L)
							{
								ulong num3 = (0xF800F800F800F800uL & *ptr3) ^ 0xD800D800D800D800uL;
								if (((num3 & 0xFFFF000000000000uL) == 0L || (num3 & 0xFFFF00000000L) == 0L || (num3 & 0xFFFF0000u) == 0L || (num3 & 0xFFFF) == 0L) && ((0xFC00FC00FC00FC00uL & *ptr3) ^ highLowPatternMask) != 0L)
								{
									break;
								}
							}
						}
						chars = (char*)ptr3;
						if (chars >= ptr)
						{
							goto IL_0279;
						}
					}
					c2 = *chars;
					chars++;
				}
				else
				{
					num += 2;
				}
				if (c2 >= '\ud800' && c2 <= '\udfff')
				{
					if (c2 <= '\udbff')
					{
						if (c > '\0')
						{
							chars--;
							num -= 2;
							if (encoderFallbackBuffer == null)
							{
								encoderFallbackBuffer = ((encoder != null) ? encoder.FallbackBuffer : encoderFallback.CreateFallbackBuffer());
								encoderFallbackBuffer.InternalInitialize(charStart, ptr, encoder, setEncoder: false);
							}
							encoderFallbackBuffer.InternalFallback(c, ref chars);
							c = '\0';
						}
						else
						{
							c = c2;
						}
					}
					else if (c == '\0')
					{
						num -= 2;
						if (encoderFallbackBuffer == null)
						{
							encoderFallbackBuffer = ((encoder != null) ? encoder.FallbackBuffer : encoderFallback.CreateFallbackBuffer());
							encoderFallbackBuffer.InternalInitialize(charStart, ptr, encoder, setEncoder: false);
						}
						encoderFallbackBuffer.InternalFallback(c2, ref chars);
					}
					else
					{
						c = '\0';
					}
				}
				else if (c > '\0')
				{
					chars--;
					if (encoderFallbackBuffer == null)
					{
						encoderFallbackBuffer = ((encoder != null) ? encoder.FallbackBuffer : encoderFallback.CreateFallbackBuffer());
						encoderFallbackBuffer.InternalInitialize(charStart, ptr, encoder, setEncoder: false);
					}
					encoderFallbackBuffer.InternalFallback(c, ref chars);
					num -= 2;
					c = '\0';
				}
				continue;
			}
			goto IL_0279;
			IL_0279:
			if (c <= '\0')
			{
				break;
			}
			num -= 2;
			if (encoder != null && !encoder.MustFlush)
			{
				break;
			}
			if (flag)
			{
				throw new ArgumentException(Environment.GetResourceString("Recursive fallback not allowed for character \\\\u{0:X4}.", c), "chars");
			}
			if (encoderFallbackBuffer == null)
			{
				encoderFallbackBuffer = ((encoder != null) ? encoder.FallbackBuffer : encoderFallback.CreateFallbackBuffer());
				encoderFallbackBuffer.InternalInitialize(charStart, ptr, encoder, setEncoder: false);
			}
			encoderFallbackBuffer.InternalFallback(c, ref chars);
			c = '\0';
			flag = true;
		}
		return num;
	}

	[SecurityCritical]
	internal unsafe override int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS encoder)
	{
		char c = '\0';
		bool flag = false;
		byte* ptr = bytes + byteCount;
		char* ptr2 = chars + charCount;
		byte* ptr3 = bytes;
		char* ptr4 = chars;
		EncoderFallbackBuffer encoderFallbackBuffer = null;
		if (encoder != null)
		{
			c = encoder.charLeftOver;
			if (encoder.InternalHasFallbackBuffer)
			{
				encoderFallbackBuffer = encoder.FallbackBuffer;
				if (encoderFallbackBuffer.Remaining > 0 && encoder.m_throwOnOverflow)
				{
					throw new ArgumentException(Environment.GetResourceString("Must complete Convert() operation or call Encoder.Reset() before calling GetBytes() or GetByteCount(). Encoder '{0}' fallback '{1}'.", EncodingName, encoder.Fallback.GetType()));
				}
				encoderFallbackBuffer.InternalInitialize(ptr4, ptr2, encoder, setEncoder: false);
			}
		}
		while (true)
		{
			char num = encoderFallbackBuffer?.InternalGetNextChar() ?? '\0';
			char c2 = num;
			if (num != 0 || chars < ptr2)
			{
				if (c2 == '\0')
				{
					if ((bigEndian ^ BitConverter.IsLittleEndian) && ((int)chars & 3) == 0 && ((int)bytes & 3) == 0 && c == '\0')
					{
						ulong* ptr5 = (ulong*)(chars - 3 + ((ptr - bytes >> 1 < ptr2 - chars) ? (ptr - bytes >> 1) : (ptr2 - chars)));
						ulong* ptr6 = (ulong*)chars;
						ulong* ptr7 = (ulong*)bytes;
						while (ptr6 < ptr5)
						{
							if ((0x8000800080008000uL & *ptr6) != 0L)
							{
								ulong num2 = (0xF800F800F800F800uL & *ptr6) ^ 0xD800D800D800D800uL;
								if (((num2 & 0xFFFF000000000000uL) == 0L || (num2 & 0xFFFF00000000L) == 0L || (num2 & 0xFFFF0000u) == 0L || (num2 & 0xFFFF) == 0L) && ((0xFC00FC00FC00FC00uL & *ptr6) ^ highLowPatternMask) != 0L)
								{
									break;
								}
							}
							*ptr7 = *ptr6;
							ptr6++;
							ptr7++;
						}
						chars = (char*)ptr6;
						bytes = (byte*)ptr7;
						if (chars >= ptr2)
						{
							goto IL_047b;
						}
					}
					else if (c == '\0' && (bigEndian ^ BitConverter.IsLittleEndian) && ((int)chars & 3) != ((int)bytes & 3) && ((int)bytes & 1) == 0)
					{
						long num3 = ((ptr - bytes >> 1 < ptr2 - chars) ? (ptr - bytes >> 1) : (ptr2 - chars));
						char* ptr8 = (char*)bytes;
						char* ptr9 = chars + num3 - 1;
						while (chars < ptr9)
						{
							if (*chars >= '\ud800' && *chars <= '\udfff')
							{
								if (*chars >= '\udc00' || chars[1] < '\udc00' || chars[1] > '\udfff')
								{
									break;
								}
							}
							else if (chars[1] >= '\ud800' && chars[1] <= '\udfff')
							{
								*ptr8 = *chars;
								ptr8++;
								chars++;
								continue;
							}
							*ptr8 = *chars;
							ptr8[1] = chars[1];
							ptr8 += 2;
							chars += 2;
						}
						bytes = (byte*)ptr8;
						if (chars >= ptr2)
						{
							goto IL_047b;
						}
					}
					c2 = *chars;
					chars++;
				}
				if (c2 >= '\ud800' && c2 <= '\udfff')
				{
					if (c2 <= '\udbff')
					{
						if (c > '\0')
						{
							chars--;
							if (encoderFallbackBuffer == null)
							{
								encoderFallbackBuffer = ((encoder != null) ? encoder.FallbackBuffer : encoderFallback.CreateFallbackBuffer());
								encoderFallbackBuffer.InternalInitialize(ptr4, ptr2, encoder, setEncoder: true);
							}
							encoderFallbackBuffer.InternalFallback(c, ref chars);
							c = '\0';
						}
						else
						{
							c = c2;
						}
						continue;
					}
					if (c == '\0')
					{
						if (encoderFallbackBuffer == null)
						{
							encoderFallbackBuffer = ((encoder != null) ? encoder.FallbackBuffer : encoderFallback.CreateFallbackBuffer());
							encoderFallbackBuffer.InternalInitialize(ptr4, ptr2, encoder, setEncoder: true);
						}
						encoderFallbackBuffer.InternalFallback(c2, ref chars);
						continue;
					}
					if (bytes + 3 >= ptr)
					{
						if (encoderFallbackBuffer != null && encoderFallbackBuffer.bFallingBack)
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
						goto IL_047b;
					}
					if (bigEndian)
					{
						*(bytes++) = (byte)((int)c >> 8);
						*(bytes++) = (byte)c;
					}
					else
					{
						*(bytes++) = (byte)c;
						*(bytes++) = (byte)((int)c >> 8);
					}
					c = '\0';
				}
				else if (c > '\0')
				{
					chars--;
					if (encoderFallbackBuffer == null)
					{
						encoderFallbackBuffer = ((encoder != null) ? encoder.FallbackBuffer : encoderFallback.CreateFallbackBuffer());
						encoderFallbackBuffer.InternalInitialize(ptr4, ptr2, encoder, setEncoder: true);
					}
					encoderFallbackBuffer.InternalFallback(c, ref chars);
					c = '\0';
					continue;
				}
				if (bytes + 1 < ptr)
				{
					if (bigEndian)
					{
						*(bytes++) = (byte)((int)c2 >> 8);
						*(bytes++) = (byte)c2;
					}
					else
					{
						*(bytes++) = (byte)c2;
						*(bytes++) = (byte)((int)c2 >> 8);
					}
					continue;
				}
				if (encoderFallbackBuffer != null && encoderFallbackBuffer.bFallingBack)
				{
					encoderFallbackBuffer.MovePrevious();
				}
				else
				{
					chars--;
				}
				ThrowBytesOverflow(encoder, bytes == ptr3);
			}
			goto IL_047b;
			IL_047b:
			if (c <= '\0' || (encoder != null && !encoder.MustFlush))
			{
				break;
			}
			if (flag)
			{
				throw new ArgumentException(Environment.GetResourceString("Recursive fallback not allowed for character \\\\u{0:X4}.", c), "chars");
			}
			if (encoderFallbackBuffer == null)
			{
				encoderFallbackBuffer = ((encoder != null) ? encoder.FallbackBuffer : encoderFallback.CreateFallbackBuffer());
				encoderFallbackBuffer.InternalInitialize(ptr4, ptr2, encoder, setEncoder: true);
			}
			encoderFallbackBuffer.InternalFallback(c, ref chars);
			c = '\0';
			flag = true;
		}
		if (encoder != null)
		{
			encoder.charLeftOver = c;
			encoder.m_charsUsed = (int)(chars - ptr4);
		}
		return (int)(bytes - ptr3);
	}

	[SecurityCritical]
	internal unsafe override int GetCharCount(byte* bytes, int count, DecoderNLS baseDecoder)
	{
		Decoder decoder = (Decoder)baseDecoder;
		byte* ptr = bytes + count;
		byte* byteStart = bytes;
		int num = -1;
		char c = '\0';
		int num2 = count >> 1;
		ulong* ptr2 = (ulong*)(ptr - 7);
		DecoderFallbackBuffer decoderFallbackBuffer = null;
		if (decoder != null)
		{
			num = decoder.lastByte;
			c = decoder.lastChar;
			if (c > '\0')
			{
				num2++;
			}
			if (num >= 0 && (count & 1) == 1)
			{
				num2++;
			}
		}
		while (bytes < ptr)
		{
			if ((bigEndian ^ BitConverter.IsLittleEndian) && ((int)bytes & 3) == 0 && num == -1 && c == '\0')
			{
				ulong* ptr3;
				for (ptr3 = (ulong*)bytes; ptr3 < ptr2; ptr3++)
				{
					if ((0x8000800080008000uL & *ptr3) != 0L)
					{
						ulong num3 = (0xF800F800F800F800uL & *ptr3) ^ 0xD800D800D800D800uL;
						if (((num3 & 0xFFFF000000000000uL) == 0L || (num3 & 0xFFFF00000000L) == 0L || (num3 & 0xFFFF0000u) == 0L || (num3 & 0xFFFF) == 0L) && ((0xFC00FC00FC00FC00uL & *ptr3) ^ highLowPatternMask) != 0L)
						{
							break;
						}
					}
				}
				bytes = (byte*)ptr3;
				if (bytes >= ptr)
				{
					break;
				}
			}
			if (num < 0)
			{
				num = *(bytes++);
				if (bytes >= ptr)
				{
					break;
				}
			}
			char c2 = ((!bigEndian) ? ((char)((*(bytes++) << 8) | num)) : ((char)((num << 8) | *(bytes++))));
			num = -1;
			if (c2 >= '\ud800' && c2 <= '\udfff')
			{
				if (c2 <= '\udbff')
				{
					if (c > '\0')
					{
						num2--;
						byte[] array = null;
						array = ((!bigEndian) ? new byte[2]
						{
							(byte)c,
							(byte)((int)c >> 8)
						} : new byte[2]
						{
							(byte)((int)c >> 8),
							(byte)c
						});
						if (decoderFallbackBuffer == null)
						{
							decoderFallbackBuffer = ((decoder != null) ? decoder.FallbackBuffer : decoderFallback.CreateFallbackBuffer());
							decoderFallbackBuffer.InternalInitialize(byteStart, null);
						}
						num2 += decoderFallbackBuffer.InternalFallback(array, bytes);
					}
					c = c2;
				}
				else if (c == '\0')
				{
					num2--;
					byte[] array2 = null;
					array2 = ((!bigEndian) ? new byte[2]
					{
						(byte)c2,
						(byte)((int)c2 >> 8)
					} : new byte[2]
					{
						(byte)((int)c2 >> 8),
						(byte)c2
					});
					if (decoderFallbackBuffer == null)
					{
						decoderFallbackBuffer = ((decoder != null) ? decoder.FallbackBuffer : decoderFallback.CreateFallbackBuffer());
						decoderFallbackBuffer.InternalInitialize(byteStart, null);
					}
					num2 += decoderFallbackBuffer.InternalFallback(array2, bytes);
				}
				else
				{
					c = '\0';
				}
			}
			else if (c > '\0')
			{
				num2--;
				byte[] array3 = null;
				array3 = ((!bigEndian) ? new byte[2]
				{
					(byte)c,
					(byte)((int)c >> 8)
				} : new byte[2]
				{
					(byte)((int)c >> 8),
					(byte)c
				});
				if (decoderFallbackBuffer == null)
				{
					decoderFallbackBuffer = ((decoder != null) ? decoder.FallbackBuffer : decoderFallback.CreateFallbackBuffer());
					decoderFallbackBuffer.InternalInitialize(byteStart, null);
				}
				num2 += decoderFallbackBuffer.InternalFallback(array3, bytes);
				c = '\0';
			}
		}
		if (decoder == null || decoder.MustFlush)
		{
			if (c > '\0')
			{
				num2--;
				byte[] array4 = null;
				array4 = ((!bigEndian) ? new byte[2]
				{
					(byte)c,
					(byte)((int)c >> 8)
				} : new byte[2]
				{
					(byte)((int)c >> 8),
					(byte)c
				});
				if (decoderFallbackBuffer == null)
				{
					decoderFallbackBuffer = ((decoder != null) ? decoder.FallbackBuffer : decoderFallback.CreateFallbackBuffer());
					decoderFallbackBuffer.InternalInitialize(byteStart, null);
				}
				num2 += decoderFallbackBuffer.InternalFallback(array4, bytes);
				c = '\0';
			}
			if (num >= 0)
			{
				if (decoderFallbackBuffer == null)
				{
					decoderFallbackBuffer = ((decoder != null) ? decoder.FallbackBuffer : decoderFallback.CreateFallbackBuffer());
					decoderFallbackBuffer.InternalInitialize(byteStart, null);
				}
				num2 += decoderFallbackBuffer.InternalFallback(new byte[1] { (byte)num }, bytes);
				num = -1;
			}
		}
		if (c > '\0')
		{
			num2--;
		}
		return num2;
	}

	[SecurityCritical]
	internal unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount, DecoderNLS baseDecoder)
	{
		Decoder decoder = (Decoder)baseDecoder;
		int num = -1;
		char c = '\0';
		if (decoder != null)
		{
			num = decoder.lastByte;
			c = decoder.lastChar;
		}
		DecoderFallbackBuffer decoderFallbackBuffer = null;
		byte* ptr = bytes + byteCount;
		char* ptr2 = chars + charCount;
		byte* ptr3 = bytes;
		char* ptr4 = chars;
		while (bytes < ptr)
		{
			if ((bigEndian ^ BitConverter.IsLittleEndian) && ((int)chars & 3) == 0 && ((int)bytes & 3) == 0 && num == -1 && c == '\0')
			{
				ulong* ptr5 = (ulong*)(bytes - 7 + ((ptr - bytes >> 1 < ptr2 - chars) ? (ptr - bytes) : (ptr2 - chars << 1)));
				ulong* ptr6 = (ulong*)bytes;
				ulong* ptr7 = (ulong*)chars;
				while (ptr6 < ptr5)
				{
					if ((0x8000800080008000uL & *ptr6) != 0L)
					{
						ulong num2 = (0xF800F800F800F800uL & *ptr6) ^ 0xD800D800D800D800uL;
						if (((num2 & 0xFFFF000000000000uL) == 0L || (num2 & 0xFFFF00000000L) == 0L || (num2 & 0xFFFF0000u) == 0L || (num2 & 0xFFFF) == 0L) && ((0xFC00FC00FC00FC00uL & *ptr6) ^ highLowPatternMask) != 0L)
						{
							break;
						}
					}
					*ptr7 = *ptr6;
					ptr6++;
					ptr7++;
				}
				chars = (char*)ptr7;
				bytes = (byte*)ptr6;
				if (bytes >= ptr)
				{
					break;
				}
			}
			if (num < 0)
			{
				num = *(bytes++);
				continue;
			}
			char c2 = ((!bigEndian) ? ((char)((*(bytes++) << 8) | num)) : ((char)((num << 8) | *(bytes++))));
			num = -1;
			if (c2 >= '\ud800' && c2 <= '\udfff')
			{
				if (c2 <= '\udbff')
				{
					if (c > '\0')
					{
						byte[] array = null;
						array = ((!bigEndian) ? new byte[2]
						{
							(byte)c,
							(byte)((int)c >> 8)
						} : new byte[2]
						{
							(byte)((int)c >> 8),
							(byte)c
						});
						if (decoderFallbackBuffer == null)
						{
							decoderFallbackBuffer = ((decoder != null) ? decoder.FallbackBuffer : decoderFallback.CreateFallbackBuffer());
							decoderFallbackBuffer.InternalInitialize(ptr3, ptr2);
						}
						if (!decoderFallbackBuffer.InternalFallback(array, bytes, ref chars))
						{
							bytes -= 2;
							decoderFallbackBuffer.InternalReset();
							ThrowCharsOverflow(decoder, chars == ptr4);
							break;
						}
					}
					c = c2;
					continue;
				}
				if (c == '\0')
				{
					byte[] array2 = null;
					array2 = ((!bigEndian) ? new byte[2]
					{
						(byte)c2,
						(byte)((int)c2 >> 8)
					} : new byte[2]
					{
						(byte)((int)c2 >> 8),
						(byte)c2
					});
					if (decoderFallbackBuffer == null)
					{
						decoderFallbackBuffer = ((decoder != null) ? decoder.FallbackBuffer : decoderFallback.CreateFallbackBuffer());
						decoderFallbackBuffer.InternalInitialize(ptr3, ptr2);
					}
					if (!decoderFallbackBuffer.InternalFallback(array2, bytes, ref chars))
					{
						bytes -= 2;
						decoderFallbackBuffer.InternalReset();
						ThrowCharsOverflow(decoder, chars == ptr4);
						break;
					}
					continue;
				}
				if (chars >= ptr2 - 1)
				{
					bytes -= 2;
					ThrowCharsOverflow(decoder, chars == ptr4);
					break;
				}
				*(chars++) = c;
				c = '\0';
			}
			else if (c > '\0')
			{
				byte[] array3 = null;
				array3 = ((!bigEndian) ? new byte[2]
				{
					(byte)c,
					(byte)((int)c >> 8)
				} : new byte[2]
				{
					(byte)((int)c >> 8),
					(byte)c
				});
				if (decoderFallbackBuffer == null)
				{
					decoderFallbackBuffer = ((decoder != null) ? decoder.FallbackBuffer : decoderFallback.CreateFallbackBuffer());
					decoderFallbackBuffer.InternalInitialize(ptr3, ptr2);
				}
				if (!decoderFallbackBuffer.InternalFallback(array3, bytes, ref chars))
				{
					bytes -= 2;
					decoderFallbackBuffer.InternalReset();
					ThrowCharsOverflow(decoder, chars == ptr4);
					break;
				}
				c = '\0';
			}
			if (chars >= ptr2)
			{
				bytes -= 2;
				ThrowCharsOverflow(decoder, chars == ptr4);
				break;
			}
			*(chars++) = c2;
		}
		if (decoder == null || decoder.MustFlush)
		{
			if (c > '\0')
			{
				byte[] array4 = null;
				array4 = ((!bigEndian) ? new byte[2]
				{
					(byte)c,
					(byte)((int)c >> 8)
				} : new byte[2]
				{
					(byte)((int)c >> 8),
					(byte)c
				});
				if (decoderFallbackBuffer == null)
				{
					decoderFallbackBuffer = ((decoder != null) ? decoder.FallbackBuffer : decoderFallback.CreateFallbackBuffer());
					decoderFallbackBuffer.InternalInitialize(ptr3, ptr2);
				}
				if (!decoderFallbackBuffer.InternalFallback(array4, bytes, ref chars))
				{
					bytes -= 2;
					if (num >= 0)
					{
						bytes--;
					}
					decoderFallbackBuffer.InternalReset();
					ThrowCharsOverflow(decoder, chars == ptr4);
					bytes += 2;
					if (num >= 0)
					{
						bytes++;
					}
					goto IL_04a2;
				}
				c = '\0';
			}
			if (num >= 0)
			{
				if (decoderFallbackBuffer == null)
				{
					decoderFallbackBuffer = ((decoder != null) ? decoder.FallbackBuffer : decoderFallback.CreateFallbackBuffer());
					decoderFallbackBuffer.InternalInitialize(ptr3, ptr2);
				}
				if (!decoderFallbackBuffer.InternalFallback(new byte[1] { (byte)num }, bytes, ref chars))
				{
					bytes--;
					decoderFallbackBuffer.InternalReset();
					ThrowCharsOverflow(decoder, chars == ptr4);
					bytes++;
				}
				else
				{
					num = -1;
				}
			}
		}
		goto IL_04a2;
		IL_04a2:
		if (decoder != null)
		{
			decoder.m_bytesUsed = (int)(bytes - ptr3);
			decoder.lastChar = c;
			decoder.lastByte = num;
		}
		return (int)(chars - ptr4);
	}

	/// <summary>Obtains an encoder that converts a sequence of Unicode characters into a UTF-16 encoded sequence of bytes.</summary>
	/// <returns>A <see cref="T:System.Text.Encoder" /> object that converts a sequence of Unicode characters into a UTF-16 encoded sequence of bytes.</returns>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	public override Encoder GetEncoder()
	{
		return new EncoderNLS(this);
	}

	/// <summary>Obtains a decoder that converts a UTF-16 encoded sequence of bytes into a sequence of Unicode characters.</summary>
	/// <returns>A <see cref="T:System.Text.Decoder" /> that converts a UTF-16 encoded sequence of bytes into a sequence of Unicode characters.</returns>
	/// <filterpriority>1</filterpriority>
	public override System.Text.Decoder GetDecoder()
	{
		return new Decoder(this);
	}

	/// <summary>Returns a Unicode byte order mark encoded in UTF-16 format, if the constructor for this instance requests a byte order mark.</summary>
	/// <returns>A byte array containing the Unicode byte order mark, if the constructor for this instance requests a byte order mark. Otherwise, this method returns a byte array of length zero.</returns>
	/// <filterpriority>1</filterpriority>
	public override byte[] GetPreamble()
	{
		if (byteOrderMark)
		{
			if (!bigEndian)
			{
				return new byte[2] { 255, 254 };
			}
			return new byte[2] { 254, 255 };
		}
		return EmptyArray<byte>.Value;
	}

	/// <summary>Calculates the maximum number of bytes produced by encoding the specified number of characters.</summary>
	/// <returns>The maximum number of bytes produced by encoding the specified number of characters.</returns>
	/// <param name="charCount">The number of characters to encode. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charCount" /> is less than zero.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
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
		num <<= 1;
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
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public override int GetMaxCharCount(int byteCount)
	{
		if (byteCount < 0)
		{
			throw new ArgumentOutOfRangeException("byteCount", Environment.GetResourceString("Non-negative number required."));
		}
		long num = (long)(byteCount >> 1) + (long)(byteCount & 1) + 1;
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

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Text.UnicodeEncoding" /> object.</summary>
	/// <returns>true if <paramref name="value" /> is an instance of <see cref="T:System.Text.UnicodeEncoding" /> and is equal to the current object; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to compare with the current object. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object value)
	{
		if (value is UnicodeEncoding unicodeEncoding)
		{
			if (CodePage == unicodeEncoding.CodePage && byteOrderMark == unicodeEncoding.byteOrderMark && bigEndian == unicodeEncoding.bigEndian && base.EncoderFallback.Equals(unicodeEncoding.EncoderFallback))
			{
				return base.DecoderFallback.Equals(unicodeEncoding.DecoderFallback);
			}
			return false;
		}
		return false;
	}

	/// <summary>Returns the hash code for the current instance.</summary>
	/// <returns>The hash code for the current <see cref="T:System.Text.UnicodeEncoding" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	public override int GetHashCode()
	{
		return CodePage + base.EncoderFallback.GetHashCode() + base.DecoderFallback.GetHashCode() + (byteOrderMark ? 4 : 0) + (bigEndian ? 8 : 0);
	}
}
