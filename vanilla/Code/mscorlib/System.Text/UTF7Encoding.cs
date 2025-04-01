using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System.Text;

/// <summary>Represents a UTF-7 encoding of Unicode characters.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public class UTF7Encoding : Encoding
{
	[Serializable]
	private class Decoder : DecoderNLS, ISerializable
	{
		internal int bits;

		internal int bitCount;

		internal bool firstByte;

		internal override bool HasState => bitCount != -1;

		public Decoder(UTF7Encoding encoding)
			: base(encoding)
		{
		}

		internal Decoder(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			bits = (int)info.GetValue("bits", typeof(int));
			bitCount = (int)info.GetValue("bitCount", typeof(int));
			firstByte = (bool)info.GetValue("firstByte", typeof(bool));
			m_encoding = (Encoding)info.GetValue("encoding", typeof(Encoding));
		}

		[SecurityCritical]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("encoding", m_encoding);
			info.AddValue("bits", bits);
			info.AddValue("bitCount", bitCount);
			info.AddValue("firstByte", firstByte);
		}

		public override void Reset()
		{
			bits = 0;
			bitCount = -1;
			firstByte = false;
			if (m_fallbackBuffer != null)
			{
				m_fallbackBuffer.Reset();
			}
		}
	}

	[Serializable]
	private class Encoder : EncoderNLS, ISerializable
	{
		internal int bits;

		internal int bitCount;

		internal override bool HasState
		{
			get
			{
				if (bits == 0)
				{
					return bitCount != -1;
				}
				return true;
			}
		}

		public Encoder(UTF7Encoding encoding)
			: base(encoding)
		{
		}

		internal Encoder(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			bits = (int)info.GetValue("bits", typeof(int));
			bitCount = (int)info.GetValue("bitCount", typeof(int));
			m_encoding = (Encoding)info.GetValue("encoding", typeof(Encoding));
		}

		[SecurityCritical]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("encoding", m_encoding);
			info.AddValue("bits", bits);
			info.AddValue("bitCount", bitCount);
		}

		public override void Reset()
		{
			bitCount = -1;
			bits = 0;
			if (m_fallbackBuffer != null)
			{
				m_fallbackBuffer.Reset();
			}
		}
	}

	[Serializable]
	internal sealed class DecoderUTF7Fallback : DecoderFallback
	{
		public override int MaxCharCount => 1;

		public override DecoderFallbackBuffer CreateFallbackBuffer()
		{
			return new DecoderUTF7FallbackBuffer(this);
		}

		public override bool Equals(object value)
		{
			if (value is DecoderUTF7Fallback)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return 984;
		}
	}

	internal sealed class DecoderUTF7FallbackBuffer : DecoderFallbackBuffer
	{
		private char cFallback;

		private int iCount = -1;

		private int iSize;

		public override int Remaining
		{
			get
			{
				if (iCount <= 0)
				{
					return 0;
				}
				return iCount;
			}
		}

		public DecoderUTF7FallbackBuffer(DecoderUTF7Fallback fallback)
		{
		}

		public override bool Fallback(byte[] bytesUnknown, int index)
		{
			cFallback = (char)bytesUnknown[0];
			if (cFallback == '\0')
			{
				return false;
			}
			iCount = (iSize = 1);
			return true;
		}

		public override char GetNextChar()
		{
			if (iCount-- > 0)
			{
				return cFallback;
			}
			return '\0';
		}

		public override bool MovePrevious()
		{
			if (iCount >= 0)
			{
				iCount++;
			}
			if (iCount >= 0)
			{
				return iCount <= iSize;
			}
			return false;
		}

		[SecuritySafeCritical]
		public unsafe override void Reset()
		{
			iCount = -1;
			byteStart = null;
		}

		[SecurityCritical]
		internal unsafe override int InternalFallback(byte[] bytes, byte* pBytes)
		{
			if (bytes.Length != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("String contains invalid Unicode code points."));
			}
			if (bytes[0] != 0)
			{
				return 1;
			}
			return 0;
		}
	}

	private const string base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

	private const string directChars = "\t\n\r '(),-./0123456789:?ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

	private const string optionalChars = "!\"#$%&*;<=>@[]^_`{|}";

	private byte[] base64Bytes;

	private sbyte[] base64Values;

	private bool[] directEncode;

	[OptionalField(VersionAdded = 2)]
	private bool m_allowOptionals;

	private const int UTF7_CODEPAGE = 65000;

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.UTF7Encoding" /> class.</summary>
	public UTF7Encoding()
		: this(allowOptionals: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.UTF7Encoding" /> class. A parameter specifies whether to allow optional characters.</summary>
	/// <param name="allowOptionals">true to specify that optional characters are allowed; otherwise, false. </param>
	public UTF7Encoding(bool allowOptionals)
		: base(65000)
	{
		m_allowOptionals = allowOptionals;
		MakeTables();
	}

	private void MakeTables()
	{
		base64Bytes = new byte[64];
		for (int i = 0; i < 64; i++)
		{
			base64Bytes[i] = (byte)"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"[i];
		}
		base64Values = new sbyte[128];
		for (int j = 0; j < 128; j++)
		{
			base64Values[j] = -1;
		}
		for (int k = 0; k < 64; k++)
		{
			base64Values[base64Bytes[k]] = (sbyte)k;
		}
		directEncode = new bool[128];
		int length = "\t\n\r '(),-./0123456789:?ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".Length;
		for (int l = 0; l < length; l++)
		{
			directEncode[(uint)"\t\n\r '(),-./0123456789:?ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"[l]] = true;
		}
		if (m_allowOptionals)
		{
			length = "!\"#$%&*;<=>@[]^_`{|}".Length;
			for (int m = 0; m < length; m++)
			{
				directEncode[(uint)"!\"#$%&*;<=>@[]^_`{|}"[m]] = true;
			}
		}
	}

	internal override void SetDefaultFallbacks()
	{
		encoderFallback = new EncoderReplacementFallback(string.Empty);
		decoderFallback = new DecoderUTF7Fallback();
	}

	[OnDeserializing]
	private void OnDeserializing(StreamingContext ctx)
	{
		OnDeserializing();
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext ctx)
	{
		OnDeserialized();
		if (m_deserializedFromEverett)
		{
			m_allowOptionals = directEncode[(uint)"!\"#$%&*;<=>@[]^_`{|}"[0]];
		}
		MakeTables();
	}

	/// <summary>Gets a value indicating whether the specified object is equal to the current <see cref="T:System.Text.UTF7Encoding" /> object.</summary>
	/// <returns>true if <paramref name="value" /> is a <see cref="T:System.Text.UTF7Encoding" /> object and is equal to the current <see cref="T:System.Text.UTF7Encoding" /> object; otherwise, false.</returns>
	/// <param name="value">An object to compare to the current <see cref="T:System.Text.UTF7Encoding" /> object.</param>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public override bool Equals(object value)
	{
		if (value is UTF7Encoding uTF7Encoding)
		{
			if (m_allowOptionals == uTF7Encoding.m_allowOptionals && base.EncoderFallback.Equals(uTF7Encoding.EncoderFallback))
			{
				return base.DecoderFallback.Equals(uTF7Encoding.DecoderFallback);
			}
			return false;
		}
		return false;
	}

	/// <summary>Returns the hash code for the current <see cref="T:System.Text.UTF7Encoding" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	public override int GetHashCode()
	{
		return CodePage + base.EncoderFallback.GetHashCode() + base.DecoderFallback.GetHashCode();
	}

	/// <summary>Calculates the number of bytes produced by encoding a set of characters from the specified character array.</summary>
	/// <returns>The number of bytes produced by encoding the specified characters.</returns>
	/// <param name="chars">The character array containing the set of characters to encode. </param>
	/// <param name="index">The index of the first character to encode. </param>
	/// <param name="count">The number of characters to encode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="chars" />.-or- The resulting number of bytes is greater than the maximum number that can be returned as an int. </exception>
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

	/// <summary>Calculates the number of bytes produced by encoding the characters in the specified <see cref="T:System.String" /> object.</summary>
	/// <returns>The number of bytes produced by encoding the specified characters.</returns>
	/// <param name="s">The <see cref="T:System.String" /> object containing the set of characters to encode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The resulting number of bytes is greater than the maximum number that can be returned as an int. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	[ComVisible(false)]
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
	///   <paramref name="count" /> is less than zero.-or- The resulting number of bytes is greater than the maximum number that can be returned as an int. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
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
	///   <paramref name="s" /> is null (Nothing).-or- <paramref name="bytes" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charIndex" /> or <paramref name="charCount" /> or <paramref name="byteIndex" /> is less than zero.-or- <paramref name="charIndex" /> and <paramref name="charCount" /> do not denote a valid range in <paramref name="chars" />.-or- <paramref name="byteIndex" /> is not a valid index in <paramref name="bytes" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="bytes" /> does not have enough capacity from <paramref name="byteIndex" /> to the end of the array to accommodate the resulting bytes. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
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
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="bytes" /> does not have enough capacity from <paramref name="byteIndex" /> to the end of the array to accommodate the resulting bytes. </exception>
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
	/// <returns>The actual number of bytes written at the location indicated by <paramref name="bytes" />.</returns>
	/// <param name="chars">A pointer to the first character to encode. </param>
	/// <param name="charCount">The number of characters to encode. </param>
	/// <param name="bytes">A pointer to the location at which to start writing the resulting sequence of bytes. </param>
	/// <param name="byteCount">The maximum number of bytes to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null (Nothing).-or- <paramref name="bytes" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charCount" /> or <paramref name="byteCount" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="byteCount" /> is less than the resulting number of bytes. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for fuller explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
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
	///   <paramref name="bytes" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="bytes" />.-or- The resulting number of characters is greater than the maximum number that can be returned as an int. </exception>
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
	///   <paramref name="bytes" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero.-or- The resulting number of characters is greater than the maximum number that can be returned as an int. </exception>
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
	/// <param name="bytes">A pointer to the first byte to decode. </param>
	/// <param name="byteCount">The number of bytes to decode. </param>
	/// <param name="chars">A pointer to the location at which to start writing the resulting set of characters. </param>
	/// <param name="charCount">The maximum number of characters to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null (Nothing).-or- <paramref name="chars" /> is null (Nothing). </exception>
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
	/// <param name="bytes">The byte array containing the sequence of bytes to decode. </param>
	/// <param name="index">The index of the first byte to decode. </param>
	/// <param name="count">The number of bytes to decode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null (Nothing). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="bytes" />. </exception>
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
	internal unsafe override int GetByteCount(char* chars, int count, EncoderNLS baseEncoder)
	{
		return GetBytes(chars, count, null, 0, baseEncoder);
	}

	[SecurityCritical]
	internal unsafe override int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS baseEncoder)
	{
		Encoder encoder = (Encoder)baseEncoder;
		int num = 0;
		int num2 = -1;
		EncodingByteBuffer encodingByteBuffer = new EncodingByteBuffer(this, encoder, bytes, byteCount, chars, charCount);
		if (encoder != null)
		{
			num = encoder.bits;
			num2 = encoder.bitCount;
			while (num2 >= 6)
			{
				num2 -= 6;
				if (!encodingByteBuffer.AddByte(base64Bytes[(num >> num2) & 0x3F]))
				{
					ThrowBytesOverflow(encoder, encodingByteBuffer.Count == 0);
				}
			}
		}
		while (encodingByteBuffer.MoreData)
		{
			char nextChar = encodingByteBuffer.GetNextChar();
			if (nextChar < '\u0080' && directEncode[(uint)nextChar])
			{
				if (num2 >= 0)
				{
					if (num2 > 0)
					{
						if (!encodingByteBuffer.AddByte(base64Bytes[(num << 6 - num2) & 0x3F]))
						{
							break;
						}
						num2 = 0;
					}
					if (!encodingByteBuffer.AddByte(45))
					{
						break;
					}
					num2 = -1;
				}
				if (!encodingByteBuffer.AddByte((byte)nextChar))
				{
					break;
				}
				continue;
			}
			if (num2 < 0 && nextChar == '+')
			{
				if (!encodingByteBuffer.AddByte((byte)43, (byte)45))
				{
					break;
				}
				continue;
			}
			if (num2 < 0)
			{
				if (!encodingByteBuffer.AddByte(43))
				{
					break;
				}
				num2 = 0;
			}
			num = (num << 16) | nextChar;
			num2 += 16;
			while (num2 >= 6)
			{
				num2 -= 6;
				if (!encodingByteBuffer.AddByte(base64Bytes[(num >> num2) & 0x3F]))
				{
					num2 += 6;
					nextChar = encodingByteBuffer.GetNextChar();
					break;
				}
			}
			if (num2 >= 6)
			{
				break;
			}
		}
		if (num2 >= 0 && (encoder == null || encoder.MustFlush))
		{
			if (num2 > 0 && encodingByteBuffer.AddByte(base64Bytes[(num << 6 - num2) & 0x3F]))
			{
				num2 = 0;
			}
			if (encodingByteBuffer.AddByte(45))
			{
				num = 0;
				num2 = -1;
			}
			else
			{
				encodingByteBuffer.GetNextChar();
			}
		}
		if (bytes != null && encoder != null)
		{
			encoder.bits = num;
			encoder.bitCount = num2;
			encoder.m_charsUsed = encodingByteBuffer.CharsUsed;
		}
		return encodingByteBuffer.Count;
	}

	[SecurityCritical]
	internal unsafe override int GetCharCount(byte* bytes, int count, DecoderNLS baseDecoder)
	{
		return GetChars(bytes, count, null, 0, baseDecoder);
	}

	[SecurityCritical]
	internal unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount, DecoderNLS baseDecoder)
	{
		Decoder decoder = (Decoder)baseDecoder;
		EncodingCharBuffer encodingCharBuffer = new EncodingCharBuffer(this, decoder, chars, charCount, bytes, byteCount);
		int num = 0;
		int num2 = -1;
		bool flag = false;
		if (decoder != null)
		{
			num = decoder.bits;
			num2 = decoder.bitCount;
			flag = decoder.firstByte;
		}
		if (num2 >= 16)
		{
			if (!encodingCharBuffer.AddChar((char)((num >> num2 - 16) & 0xFFFF)))
			{
				ThrowCharsOverflow(decoder, nothingDecoded: true);
			}
			num2 -= 16;
		}
		while (encodingCharBuffer.MoreData)
		{
			byte nextByte = encodingCharBuffer.GetNextByte();
			int num3;
			if (num2 >= 0)
			{
				sbyte b;
				if (nextByte < 128 && (b = base64Values[nextByte]) >= 0)
				{
					flag = false;
					num = (num << 6) | (byte)b;
					num2 += 6;
					if (num2 < 16)
					{
						continue;
					}
					num3 = (num >> num2 - 16) & 0xFFFF;
					num2 -= 16;
				}
				else
				{
					num2 = -1;
					if (nextByte != 45)
					{
						if (!encodingCharBuffer.Fallback(nextByte))
						{
							break;
						}
						continue;
					}
					if (!flag)
					{
						continue;
					}
					num3 = 43;
				}
			}
			else
			{
				if (nextByte == 43)
				{
					num2 = 0;
					flag = true;
					continue;
				}
				if (nextByte >= 128)
				{
					if (!encodingCharBuffer.Fallback(nextByte))
					{
						break;
					}
					continue;
				}
				num3 = nextByte;
			}
			if (num3 >= 0 && !encodingCharBuffer.AddChar((char)num3))
			{
				if (num2 >= 0)
				{
					encodingCharBuffer.AdjustBytes(1);
					num2 += 16;
				}
				break;
			}
		}
		if (chars != null && decoder != null)
		{
			if (decoder.MustFlush)
			{
				decoder.bits = 0;
				decoder.bitCount = -1;
				decoder.firstByte = false;
			}
			else
			{
				decoder.bits = num;
				decoder.bitCount = num2;
				decoder.firstByte = flag;
			}
			decoder.m_bytesUsed = encodingCharBuffer.BytesUsed;
		}
		return encodingCharBuffer.Count;
	}

	/// <summary>Obtains a decoder that converts a UTF-7 encoded sequence of bytes into a sequence of Unicode characters.</summary>
	/// <returns>A <see cref="T:System.Text.Decoder" /> that converts a UTF-7 encoded sequence of bytes into a sequence of Unicode characters.</returns>
	/// <filterpriority>1</filterpriority>
	public override System.Text.Decoder GetDecoder()
	{
		return new Decoder(this);
	}

	/// <summary>Obtains an encoder that converts a sequence of Unicode characters into a UTF-7 encoded sequence of bytes.</summary>
	/// <returns>A <see cref="T:System.Text.Encoder" /> that converts a sequence of Unicode characters into a UTF-7 encoded sequence of bytes.</returns>
	/// <filterpriority>1</filterpriority>
	public override System.Text.Encoder GetEncoder()
	{
		return new Encoder(this);
	}

	/// <summary>Calculates the maximum number of bytes produced by encoding the specified number of characters.</summary>
	/// <returns>The maximum number of bytes produced by encoding the specified number of characters.</returns>
	/// <param name="charCount">The number of characters to encode. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charCount" /> is less than zero.-or- The resulting number of bytes is greater than the maximum number that can be returned as an int. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public override int GetMaxByteCount(int charCount)
	{
		if (charCount < 0)
		{
			throw new ArgumentOutOfRangeException("charCount", Environment.GetResourceString("Non-negative number required."));
		}
		long num = (long)charCount * 3L + 2;
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
	///   <paramref name="byteCount" /> is less than zero.-or- The resulting number of characters is greater than the maximum number that can be returned as an int. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public override int GetMaxCharCount(int byteCount)
	{
		if (byteCount < 0)
		{
			throw new ArgumentOutOfRangeException("byteCount", Environment.GetResourceString("Non-negative number required."));
		}
		int num = byteCount;
		if (num == 0)
		{
			num = 1;
		}
		return num;
	}
}
