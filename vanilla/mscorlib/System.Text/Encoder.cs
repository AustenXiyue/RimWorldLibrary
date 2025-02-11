using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System.Text;

/// <summary>Converts a set of characters into a sequence of bytes.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public abstract class Encoder
{
	internal EncoderFallback m_fallback;

	[NonSerialized]
	internal EncoderFallbackBuffer m_fallbackBuffer;

	/// <summary>Gets or sets a <see cref="T:System.Text.EncoderFallback" /> object for the current <see cref="T:System.Text.Encoder" /> object.</summary>
	/// <returns>A <see cref="T:System.Text.EncoderFallback" /> object.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value in a set operation is null (Nothing).</exception>
	/// <exception cref="T:System.ArgumentException">A new value cannot be assigned in a set operation because the current <see cref="T:System.Text.EncoderFallbackBuffer" /> object contains data that has not been encoded yet. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoder.Fallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	public EncoderFallback Fallback
	{
		get
		{
			return m_fallback;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (m_fallbackBuffer != null && m_fallbackBuffer.Remaining > 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Cannot change fallback when buffer is not empty. Previous Convert() call left data in the fallback buffer."), "value");
			}
			m_fallback = value;
			m_fallbackBuffer = null;
		}
	}

	/// <summary>Gets the <see cref="T:System.Text.EncoderFallbackBuffer" /> object associated with the current <see cref="T:System.Text.Encoder" /> object.</summary>
	/// <returns>A <see cref="T:System.Text.EncoderFallbackBuffer" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	public EncoderFallbackBuffer FallbackBuffer
	{
		get
		{
			if (m_fallbackBuffer == null)
			{
				if (m_fallback != null)
				{
					m_fallbackBuffer = m_fallback.CreateFallbackBuffer();
				}
				else
				{
					m_fallbackBuffer = EncoderFallback.ReplacementFallback.CreateFallbackBuffer();
				}
			}
			return m_fallbackBuffer;
		}
	}

	internal bool InternalHasFallbackBuffer => m_fallbackBuffer != null;

	internal void SerializeEncoder(SerializationInfo info)
	{
		info.AddValue("m_fallback", m_fallback);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.Encoder" /> class.</summary>
	protected Encoder()
	{
	}

	/// <summary>When overridden in a derived class, sets the encoder back to its initial state.</summary>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public virtual void Reset()
	{
		char[] chars = new char[0];
		byte[] bytes = new byte[GetByteCount(chars, 0, 0, flush: true)];
		GetBytes(chars, 0, 0, bytes, 0, flush: true);
		if (m_fallbackBuffer != null)
		{
			m_fallbackBuffer.Reset();
		}
	}

	/// <summary>When overridden in a derived class, calculates the number of bytes produced by encoding a set of characters from the specified character array. A parameter indicates whether to clear the internal state of the encoder after the calculation.</summary>
	/// <returns>The number of bytes produced by encoding the specified characters and any characters in the internal buffer.</returns>
	/// <param name="chars">The character array containing the set of characters to encode. </param>
	/// <param name="index">The index of the first character to encode. </param>
	/// <param name="count">The number of characters to encode. </param>
	/// <param name="flush">true to simulate clearing the internal state of the encoder after the calculation; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="chars" />. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoder.Fallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>2</filterpriority>
	public abstract int GetByteCount(char[] chars, int index, int count, bool flush);

	/// <summary>When overridden in a derived class, calculates the number of bytes produced by encoding a set of characters starting at the specified character pointer. A parameter indicates whether to clear the internal state of the encoder after the calculation.</summary>
	/// <returns>The number of bytes produced by encoding the specified characters and any characters in the internal buffer.</returns>
	/// <param name="chars">A pointer to the first character to encode. </param>
	/// <param name="count">The number of characters to encode. </param>
	/// <param name="flush">true to simulate clearing the internal state of the encoder after the calculation; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null (Nothing in Visual Basic .NET). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoder.Fallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	[CLSCompliant(false)]
	[ComVisible(false)]
	public unsafe virtual int GetByteCount(char* chars, int count, bool flush)
	{
		if (chars == null)
		{
			throw new ArgumentNullException("chars", Environment.GetResourceString("Array cannot be null."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		char[] array = new char[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = chars[i];
		}
		return GetByteCount(array, 0, count, flush);
	}

	/// <summary>When overridden in a derived class, encodes a set of characters from the specified character array and any characters in the internal buffer into the specified byte array. A parameter indicates whether to clear the internal state of the encoder after the conversion.</summary>
	/// <returns>The actual number of bytes written into <paramref name="bytes" />.</returns>
	/// <param name="chars">The character array containing the set of characters to encode. </param>
	/// <param name="charIndex">The index of the first character to encode. </param>
	/// <param name="charCount">The number of characters to encode. </param>
	/// <param name="bytes">The byte array to contain the resulting sequence of bytes. </param>
	/// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes. </param>
	/// <param name="flush">true to clear the internal state of the encoder after the conversion; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null (Nothing).-or- <paramref name="bytes" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charIndex" /> or <paramref name="charCount" /> or <paramref name="byteIndex" /> is less than zero.-or- <paramref name="charIndex" /> and <paramref name="charCount" /> do not denote a valid range in <paramref name="chars" />.-or- <paramref name="byteIndex" /> is not a valid index in <paramref name="bytes" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="bytes" /> does not have enough capacity from <paramref name="byteIndex" /> to the end of the array to accommodate the resulting bytes. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoder.Fallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>2</filterpriority>
	public abstract int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush);

	/// <summary>When overridden in a derived class, encodes a set of characters starting at the specified character pointer and any characters in the internal buffer into a sequence of bytes that are stored starting at the specified byte pointer. A parameter indicates whether to clear the internal state of the encoder after the conversion.</summary>
	/// <returns>The actual number of bytes written at the location indicated by the <paramref name="bytes" /> parameter.</returns>
	/// <param name="chars">A pointer to the first character to encode. </param>
	/// <param name="charCount">The number of characters to encode. </param>
	/// <param name="bytes">A pointer to the location at which to start writing the resulting sequence of bytes. </param>
	/// <param name="byteCount">The maximum number of bytes to write. </param>
	/// <param name="flush">true to clear the internal state of the encoder after the conversion; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null (Nothing).-or- <paramref name="bytes" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charCount" /> or <paramref name="byteCount" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="byteCount" /> is less than the resulting number of bytes. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoder.Fallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	[CLSCompliant(false)]
	[SecurityCritical]
	public unsafe virtual int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, bool flush)
	{
		if (bytes == null || chars == null)
		{
			throw new ArgumentNullException((bytes == null) ? "bytes" : "chars", Environment.GetResourceString("Array cannot be null."));
		}
		if (charCount < 0 || byteCount < 0)
		{
			throw new ArgumentOutOfRangeException((charCount < 0) ? "charCount" : "byteCount", Environment.GetResourceString("Non-negative number required."));
		}
		char[] array = new char[charCount];
		for (int i = 0; i < charCount; i++)
		{
			array[i] = chars[i];
		}
		byte[] array2 = new byte[byteCount];
		int bytes2 = GetBytes(array, 0, charCount, array2, 0, flush);
		if (bytes2 < byteCount)
		{
			byteCount = bytes2;
		}
		for (int i = 0; i < byteCount; i++)
		{
			bytes[i] = array2[i];
		}
		return byteCount;
	}

	/// <summary>Converts an array of Unicode characters to an encoded byte sequence and stores the result in an array of bytes.</summary>
	/// <param name="chars">An array of characters to convert.</param>
	/// <param name="charIndex">The first element of <paramref name="chars" /> to convert.</param>
	/// <param name="charCount">The number of elements of <paramref name="chars" /> to convert.</param>
	/// <param name="bytes">An array where the converted bytes are stored.</param>
	/// <param name="byteIndex">The first element of <paramref name="bytes" /> in which data is stored.</param>
	/// <param name="byteCount">The maximum number of elements of <paramref name="bytes" /> to use in the conversion.</param>
	/// <param name="flush">true to indicate no further data is to be converted; otherwise, false.</param>
	/// <param name="charsUsed">When this method returns, contains the number of characters from <paramref name="chars" /> that were used in the conversion. This parameter is passed uninitialized.</param>
	/// <param name="bytesUsed">When this method returns, contains the number of bytes that were produced by the conversion. This parameter is passed uninitialized.</param>
	/// <param name="completed">When this method returns, contains true if all the characters specified by <paramref name="charCount" /> were converted; otherwise, false. This parameter is passed uninitialized.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> or <paramref name="bytes" /> is null (Nothing).</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charIndex" />, <paramref name="charCount" />, <paramref name="byteIndex" />, or <paramref name="byteCount" /> is less than zero.-or-The length of <paramref name="chars" /> - <paramref name="charIndex" /> is less than <paramref name="charCount" />.-or-The length of <paramref name="bytes" /> - <paramref name="byteIndex" /> is less than <paramref name="byteCount" />.</exception>
	/// <exception cref="T:System.ArgumentException">The output buffer is too small to contain any of the converted input. The output buffer should be greater than or equal to the size indicated by the <see cref="Overload:System.Text.Encoder.GetByteCount" /> method.</exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoder.Fallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public virtual void Convert(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
	{
		if (chars == null || bytes == null)
		{
			throw new ArgumentNullException((chars == null) ? "chars" : "bytes", Environment.GetResourceString("Array cannot be null."));
		}
		if (charIndex < 0 || charCount < 0)
		{
			throw new ArgumentOutOfRangeException((charIndex < 0) ? "charIndex" : "charCount", Environment.GetResourceString("Non-negative number required."));
		}
		if (byteIndex < 0 || byteCount < 0)
		{
			throw new ArgumentOutOfRangeException((byteIndex < 0) ? "byteIndex" : "byteCount", Environment.GetResourceString("Non-negative number required."));
		}
		if (chars.Length - charIndex < charCount)
		{
			throw new ArgumentOutOfRangeException("chars", Environment.GetResourceString("Index and count must refer to a location within the buffer."));
		}
		if (bytes.Length - byteIndex < byteCount)
		{
			throw new ArgumentOutOfRangeException("bytes", Environment.GetResourceString("Index and count must refer to a location within the buffer."));
		}
		for (charsUsed = charCount; charsUsed > 0; charsUsed /= 2)
		{
			if (GetByteCount(chars, charIndex, charsUsed, flush) <= byteCount)
			{
				bytesUsed = GetBytes(chars, charIndex, charsUsed, bytes, byteIndex, flush);
				completed = charsUsed == charCount && (m_fallbackBuffer == null || m_fallbackBuffer.Remaining == 0);
				return;
			}
			flush = false;
		}
		throw new ArgumentException(Environment.GetResourceString("Conversion buffer overflow."));
	}

	/// <summary>Converts a buffer of Unicode characters to an encoded byte sequence and stores the result in another buffer.</summary>
	/// <param name="chars">The address of a string of UTF-16 encoded characters to convert.</param>
	/// <param name="charCount">The number of characters in <paramref name="chars" /> to convert.</param>
	/// <param name="bytes">The address of a buffer to store the converted bytes.</param>
	/// <param name="byteCount">The maximum number of bytes in <paramref name="bytes" /> to use in the conversion.</param>
	/// <param name="flush">true to indicate no further data is to be converted; otherwise, false.</param>
	/// <param name="charsUsed">When this method returns, contains the number of characters from <paramref name="chars" /> that were used in the conversion. This parameter is passed uninitialized.</param>
	/// <param name="bytesUsed">When this method returns, contains the number of bytes that were used in the conversion. This parameter is passed uninitialized.</param>
	/// <param name="completed">When this method returns, contains true if all the characters specified by <paramref name="charCount" /> were converted; otherwise, false. This parameter is passed uninitialized.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> or <paramref name="bytes" /> is null (Nothing).</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charCount" /> or <paramref name="byteCount" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">The output buffer is too small to contain any of the converted input. The output buffer should be greater than or equal to the size indicated by the <see cref="Overload:System.Text.Encoder.GetByteCount" /> method.</exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoder.Fallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>2</filterpriority>
	[CLSCompliant(false)]
	[SecurityCritical]
	[ComVisible(false)]
	public unsafe virtual void Convert(char* chars, int charCount, byte* bytes, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
	{
		if (bytes == null || chars == null)
		{
			throw new ArgumentNullException((bytes == null) ? "bytes" : "chars", Environment.GetResourceString("Array cannot be null."));
		}
		if (charCount < 0 || byteCount < 0)
		{
			throw new ArgumentOutOfRangeException((charCount < 0) ? "charCount" : "byteCount", Environment.GetResourceString("Non-negative number required."));
		}
		for (charsUsed = charCount; charsUsed > 0; charsUsed /= 2)
		{
			if (GetByteCount(chars, charsUsed, flush) <= byteCount)
			{
				bytesUsed = GetBytes(chars, charsUsed, bytes, byteCount, flush);
				completed = charsUsed == charCount && (m_fallbackBuffer == null || m_fallbackBuffer.Remaining == 0);
				return;
			}
			flush = false;
		}
		throw new ArgumentException(Environment.GetResourceString("Conversion buffer overflow."));
	}
}
