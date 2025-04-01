using System.Runtime.InteropServices;
using System.Security;

namespace System.Text;

/// <summary>Represents an ASCII character encoding of Unicode characters.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public class ASCIIEncoding : Encoding
{
	/// <summary>Gets a value indicating whether the current encoding uses single-byte code points.</summary>
	/// <returns>This property is always true.</returns>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public override bool IsSingleByte => true;

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.ASCIIEncoding" /> class.</summary>
	public ASCIIEncoding()
		: base(20127)
	{
	}

	internal override void SetDefaultFallbacks()
	{
		encoderFallback = EncoderFallback.ReplacementFallback;
		decoderFallback = DecoderFallback.ReplacementFallback;
	}

	/// <summary>Calculates the number of bytes produced by encoding a set of characters from the specified character array.</summary>
	/// <returns>The number of bytes produced by encoding the specified characters.</returns>
	/// <param name="chars">The character array containing the set of characters to encode.</param>
	/// <param name="index">The index of the first character to encode.</param>
	/// <param name="count">The number of characters to encode.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="chars" />.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
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
	/// <param name="chars">The <see cref="T:System.String" /> containing the set of characters to encode.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe override int GetByteCount(string chars)
	{
		if (chars == null)
		{
			throw new ArgumentNullException("chars");
		}
		fixed (char* chars2 = chars)
		{
			return GetByteCount(chars2, chars.Length, null);
		}
	}

	/// <summary>Calculates the number of bytes produced by encoding a set of characters starting at the specified character pointer.</summary>
	/// <returns>The number of bytes produced by encoding the specified characters.</returns>
	/// <param name="chars">A pointer to the first character to encode.</param>
	/// <param name="count">The number of characters to encode.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
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
	/// <param name="chars">The <see cref="T:System.String" /> containing the set of characters to encode.</param>
	/// <param name="charIndex">The index of the first character to encode.</param>
	/// <param name="charCount">The number of characters to encode.</param>
	/// <param name="bytes">The byte array to contain the resulting sequence of bytes.</param>
	/// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null.-or- <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charIndex" /> or <paramref name="charCount" /> or <paramref name="byteIndex" /> is less than zero.-or- <paramref name="charIndex" /> and <paramref name="charCount" /> do not denote a valid range in <paramref name="chars" />.-or- <paramref name="byteIndex" /> is not a valid index in <paramref name="bytes" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="bytes" /> does not have enough capacity from <paramref name="byteIndex" /> to the end of the array to accommodate the resulting bytes. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe override int GetBytes(string chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
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
			throw new ArgumentOutOfRangeException("chars", Environment.GetResourceString("Index and count must refer to a location within the string."));
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
		fixed (char* ptr = chars)
		{
			fixed (byte* ptr2 = bytes)
			{
				return GetBytes(ptr + charIndex, charCount, ptr2 + byteIndex, byteCount, null);
			}
		}
	}

	/// <summary>Encodes a set of characters from the specified character array into the specified byte array.</summary>
	/// <returns>The actual number of bytes written into <paramref name="bytes" />.</returns>
	/// <param name="chars">The character array containing the set of characters to encode.</param>
	/// <param name="charIndex">The index of the first character to encode.</param>
	/// <param name="charCount">The number of characters to encode.</param>
	/// <param name="bytes">The byte array to contain the resulting sequence of bytes.</param>
	/// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null.-or- <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charIndex" /> or <paramref name="charCount" /> or <paramref name="byteIndex" /> is less than zero.-or- <paramref name="charIndex" /> and <paramref name="charCount" /> do not denote a valid range in <paramref name="chars" />.-or- <paramref name="byteIndex" /> is not a valid index in <paramref name="bytes" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="bytes" /> does not have enough capacity from <paramref name="byteIndex" /> to the end of the array to accommodate the resulting bytes. </exception>
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
	/// <param name="chars">A pointer to the first character to encode.</param>
	/// <param name="charCount">The number of characters to encode.</param>
	/// <param name="bytes">A pointer to the location at which to start writing the resulting sequence of bytes.</param>
	/// <param name="byteCount">The maximum number of bytes to write.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null.-or- <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charCount" /> or <paramref name="byteCount" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="byteCount" /> is less than the resulting number of bytes. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	[ComVisible(false)]
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
	/// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
	/// <param name="index">The index of the first byte to decode.</param>
	/// <param name="count">The number of bytes to decode.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="bytes" />.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
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
	/// <param name="bytes">A pointer to the first byte to decode.</param>
	/// <param name="count">The number of bytes to decode.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	[SecurityCritical]
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
	/// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
	/// <param name="byteIndex">The index of the first byte to decode.</param>
	/// <param name="byteCount">The number of bytes to decode.</param>
	/// <param name="chars">The character array to contain the resulting set of characters.</param>
	/// <param name="charIndex">The index at which to start writing the resulting set of characters.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null.-or- <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="byteIndex" /> or <paramref name="byteCount" /> or <paramref name="charIndex" /> is less than zero.-or- <paramref name="byteindex" /> and <paramref name="byteCount" /> do not denote a valid range in <paramref name="bytes" />.-or- <paramref name="charIndex" /> is not a valid index in <paramref name="chars" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="chars" /> does not have enough capacity from <paramref name="charIndex" /> to the end of the array to accommodate the resulting characters. </exception>
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
	/// <param name="bytes">A pointer to the first byte to decode.</param>
	/// <param name="byteCount">The number of bytes to decode.</param>
	/// <param name="chars">A pointer to the location at which to start writing the resulting set of characters.</param>
	/// <param name="charCount">The maximum number of characters to write.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null.-or- <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="byteCount" /> or <paramref name="charCount" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="charCount" /> is less than the resulting number of characters. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
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
	/// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
	/// <param name="byteIndex">The index of the first byte to decode.</param>
	/// <param name="byteCount">The number of bytes to decode.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="bytes" />. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe override string GetString(byte[] bytes, int byteIndex, int byteCount)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes", Environment.GetResourceString("Array cannot be null."));
		}
		if (byteIndex < 0 || byteCount < 0)
		{
			throw new ArgumentOutOfRangeException((byteIndex < 0) ? "byteIndex" : "byteCount", Environment.GetResourceString("Non-negative number required."));
		}
		if (bytes.Length - byteIndex < byteCount)
		{
			throw new ArgumentOutOfRangeException("bytes", Environment.GetResourceString("Index and count must refer to a location within the buffer."));
		}
		if (bytes.Length == 0)
		{
			return string.Empty;
		}
		fixed (byte* ptr = bytes)
		{
			return string.CreateStringFromEncoding(ptr + byteIndex, byteCount, this);
		}
	}

	[SecurityCritical]
	internal unsafe override int GetByteCount(char* chars, int charCount, EncoderNLS encoder)
	{
		char c = '\0';
		EncoderReplacementFallback encoderReplacementFallback = null;
		char* ptr = chars + charCount;
		EncoderFallbackBuffer encoderFallbackBuffer = null;
		if (encoder != null)
		{
			c = encoder.charLeftOver;
			encoderReplacementFallback = encoder.Fallback as EncoderReplacementFallback;
			if (encoder.InternalHasFallbackBuffer)
			{
				encoderFallbackBuffer = encoder.FallbackBuffer;
				if (encoderFallbackBuffer.Remaining > 0 && encoder.m_throwOnOverflow)
				{
					throw new ArgumentException(Environment.GetResourceString("Must complete Convert() operation or call Encoder.Reset() before calling GetBytes() or GetByteCount(). Encoder '{0}' fallback '{1}'.", EncodingName, encoder.Fallback.GetType()));
				}
				encoderFallbackBuffer.InternalInitialize(chars, ptr, encoder, setEncoder: false);
			}
		}
		else
		{
			encoderReplacementFallback = base.EncoderFallback as EncoderReplacementFallback;
		}
		if (encoderReplacementFallback != null && encoderReplacementFallback.MaxCharCount == 1)
		{
			if (c > '\0')
			{
				charCount++;
			}
			return charCount;
		}
		int num = 0;
		if (c > '\0')
		{
			encoderFallbackBuffer = encoder.FallbackBuffer;
			encoderFallbackBuffer.InternalInitialize(chars, ptr, encoder, setEncoder: false);
			encoderFallbackBuffer.InternalFallback(c, ref chars);
		}
		while (true)
		{
			char num2 = encoderFallbackBuffer?.InternalGetNextChar() ?? '\0';
			char c2 = num2;
			if (num2 == '\0' && chars >= ptr)
			{
				break;
			}
			if (c2 == '\0')
			{
				c2 = *chars;
				chars++;
			}
			if (c2 > '\u007f')
			{
				if (encoderFallbackBuffer == null)
				{
					encoderFallbackBuffer = ((encoder != null) ? encoder.FallbackBuffer : encoderFallback.CreateFallbackBuffer());
					encoderFallbackBuffer.InternalInitialize(ptr - charCount, ptr, encoder, setEncoder: false);
				}
				encoderFallbackBuffer.InternalFallback(c2, ref chars);
			}
			else
			{
				num++;
			}
		}
		return num;
	}

	[SecurityCritical]
	internal unsafe override int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS encoder)
	{
		char c = '\0';
		EncoderReplacementFallback encoderReplacementFallback = null;
		EncoderFallbackBuffer encoderFallbackBuffer = null;
		char* ptr = chars + charCount;
		byte* ptr2 = bytes;
		char* ptr3 = chars;
		if (encoder != null)
		{
			c = encoder.charLeftOver;
			encoderReplacementFallback = encoder.Fallback as EncoderReplacementFallback;
			if (encoder.InternalHasFallbackBuffer)
			{
				encoderFallbackBuffer = encoder.FallbackBuffer;
				if (encoderFallbackBuffer.Remaining > 0 && encoder.m_throwOnOverflow)
				{
					throw new ArgumentException(Environment.GetResourceString("Must complete Convert() operation or call Encoder.Reset() before calling GetBytes() or GetByteCount(). Encoder '{0}' fallback '{1}'.", EncodingName, encoder.Fallback.GetType()));
				}
				encoderFallbackBuffer.InternalInitialize(ptr3, ptr, encoder, setEncoder: true);
			}
		}
		else
		{
			encoderReplacementFallback = base.EncoderFallback as EncoderReplacementFallback;
		}
		if (encoderReplacementFallback != null && encoderReplacementFallback.MaxCharCount == 1)
		{
			char c2 = encoderReplacementFallback.DefaultString[0];
			if (c2 <= '\u007f')
			{
				if (c > '\0')
				{
					if (byteCount == 0)
					{
						ThrowBytesOverflow(encoder, nothingEncoded: true);
					}
					*(bytes++) = (byte)c2;
					byteCount--;
				}
				if (byteCount < charCount)
				{
					ThrowBytesOverflow(encoder, byteCount < 1);
					ptr = chars + byteCount;
				}
				while (chars < ptr)
				{
					char c3 = *(chars++);
					if (c3 >= '\u0080')
					{
						*(bytes++) = (byte)c2;
					}
					else
					{
						*(bytes++) = (byte)c3;
					}
				}
				if (encoder != null)
				{
					encoder.charLeftOver = '\0';
					encoder.m_charsUsed = (int)(chars - ptr3);
				}
				return (int)(bytes - ptr2);
			}
		}
		byte* ptr4 = bytes + byteCount;
		if (c > '\0')
		{
			encoderFallbackBuffer = encoder.FallbackBuffer;
			encoderFallbackBuffer.InternalInitialize(chars, ptr, encoder, setEncoder: true);
			encoderFallbackBuffer.InternalFallback(c, ref chars);
		}
		while (true)
		{
			char num = encoderFallbackBuffer?.InternalGetNextChar() ?? '\0';
			char c4 = num;
			if (num == '\0' && chars >= ptr)
			{
				break;
			}
			if (c4 == '\0')
			{
				c4 = *chars;
				chars++;
			}
			if (c4 > '\u007f')
			{
				if (encoderFallbackBuffer == null)
				{
					encoderFallbackBuffer = ((encoder != null) ? encoder.FallbackBuffer : encoderFallback.CreateFallbackBuffer());
					encoderFallbackBuffer.InternalInitialize(ptr - charCount, ptr, encoder, setEncoder: true);
				}
				encoderFallbackBuffer.InternalFallback(c4, ref chars);
				continue;
			}
			if (bytes >= ptr4)
			{
				if (encoderFallbackBuffer == null || !encoderFallbackBuffer.bFallingBack)
				{
					chars--;
				}
				else
				{
					encoderFallbackBuffer.MovePrevious();
				}
				ThrowBytesOverflow(encoder, bytes == ptr2);
				break;
			}
			*bytes = (byte)c4;
			bytes++;
		}
		if (encoder != null)
		{
			if (encoderFallbackBuffer != null && !encoderFallbackBuffer.bUsedEncoder)
			{
				encoder.charLeftOver = '\0';
			}
			encoder.m_charsUsed = (int)(chars - ptr3);
		}
		return (int)(bytes - ptr2);
	}

	[SecurityCritical]
	internal unsafe override int GetCharCount(byte* bytes, int count, DecoderNLS decoder)
	{
		DecoderReplacementFallback decoderReplacementFallback = null;
		decoderReplacementFallback = ((decoder != null) ? (decoder.Fallback as DecoderReplacementFallback) : (base.DecoderFallback as DecoderReplacementFallback));
		if (decoderReplacementFallback != null && decoderReplacementFallback.MaxCharCount == 1)
		{
			return count;
		}
		DecoderFallbackBuffer decoderFallbackBuffer = null;
		int num = count;
		byte[] array = new byte[1];
		byte* ptr = bytes + count;
		while (bytes < ptr)
		{
			byte b = *bytes;
			bytes++;
			if (b >= 128)
			{
				if (decoderFallbackBuffer == null)
				{
					decoderFallbackBuffer = ((decoder != null) ? decoder.FallbackBuffer : base.DecoderFallback.CreateFallbackBuffer());
					decoderFallbackBuffer.InternalInitialize(ptr - count, null);
				}
				array[0] = b;
				num--;
				num += decoderFallbackBuffer.InternalFallback(array, bytes);
			}
		}
		return num;
	}

	[SecurityCritical]
	internal unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount, DecoderNLS decoder)
	{
		byte* ptr = bytes + byteCount;
		byte* ptr2 = bytes;
		char* ptr3 = chars;
		DecoderReplacementFallback decoderReplacementFallback = null;
		decoderReplacementFallback = ((decoder != null) ? (decoder.Fallback as DecoderReplacementFallback) : (base.DecoderFallback as DecoderReplacementFallback));
		if (decoderReplacementFallback != null && decoderReplacementFallback.MaxCharCount == 1)
		{
			char c = decoderReplacementFallback.DefaultString[0];
			if (charCount < byteCount)
			{
				ThrowCharsOverflow(decoder, charCount < 1);
				ptr = bytes + charCount;
			}
			while (bytes < ptr)
			{
				byte b = *(bytes++);
				if (b >= 128)
				{
					*(chars++) = c;
				}
				else
				{
					*(chars++) = (char)b;
				}
			}
			if (decoder != null)
			{
				decoder.m_bytesUsed = (int)(bytes - ptr2);
			}
			return (int)(chars - ptr3);
		}
		DecoderFallbackBuffer decoderFallbackBuffer = null;
		byte[] array = new byte[1];
		char* ptr4 = chars + charCount;
		while (bytes < ptr)
		{
			byte b2 = *bytes;
			bytes++;
			if (b2 >= 128)
			{
				if (decoderFallbackBuffer == null)
				{
					decoderFallbackBuffer = ((decoder != null) ? decoder.FallbackBuffer : base.DecoderFallback.CreateFallbackBuffer());
					decoderFallbackBuffer.InternalInitialize(ptr - byteCount, ptr4);
				}
				array[0] = b2;
				if (!decoderFallbackBuffer.InternalFallback(array, bytes, ref chars))
				{
					bytes--;
					decoderFallbackBuffer.InternalReset();
					ThrowCharsOverflow(decoder, chars == ptr3);
					break;
				}
			}
			else
			{
				if (chars >= ptr4)
				{
					bytes--;
					ThrowCharsOverflow(decoder, chars == ptr3);
					break;
				}
				*chars = (char)b2;
				chars++;
			}
		}
		if (decoder != null)
		{
			decoder.m_bytesUsed = (int)(bytes - ptr2);
		}
		return (int)(chars - ptr3);
	}

	/// <summary>Calculates the maximum number of bytes produced by encoding the specified number of characters.</summary>
	/// <returns>The maximum number of bytes produced by encoding the specified number of characters.</returns>
	/// <param name="charCount">The number of characters to encode.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charCount" /> is less than zero.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
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
		if (num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("charCount", Environment.GetResourceString("Too many characters. The resulting number of bytes is larger than what can be returned as an int."));
		}
		return (int)num;
	}

	/// <summary>Calculates the maximum number of characters produced by decoding the specified number of bytes.</summary>
	/// <returns>The maximum number of characters produced by decoding the specified number of bytes.</returns>
	/// <param name="byteCount">The number of bytes to decode.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="byteCount" /> is less than zero.-or- The resulting number of bytes is greater than the maximum number that can be returned as an integer. </exception>
	/// <filterpriority>1</filterpriority>
	public override int GetMaxCharCount(int byteCount)
	{
		if (byteCount < 0)
		{
			throw new ArgumentOutOfRangeException("byteCount", Environment.GetResourceString("Non-negative number required."));
		}
		long num = byteCount;
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

	/// <summary>Obtains a decoder that converts an ASCII encoded sequence of bytes into a sequence of Unicode characters.</summary>
	/// <returns>A <see cref="T:System.Text.Decoder" /> that converts an ASCII encoded sequence of bytes into a sequence of Unicode characters.</returns>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	public override Decoder GetDecoder()
	{
		return new DecoderNLS(this);
	}

	/// <summary>Obtains an encoder that converts a sequence of Unicode characters into an ASCII encoded sequence of bytes.</summary>
	/// <returns>An <see cref="T:System.Text.Encoder" /> that converts a sequence of Unicode characters into an ASCII encoded sequence of bytes.</returns>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	public override Encoder GetEncoder()
	{
		return new EncoderNLS(this);
	}
}
