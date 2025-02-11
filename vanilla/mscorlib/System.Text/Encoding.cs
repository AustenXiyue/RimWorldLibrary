using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;

namespace System.Text;

/// <summary>Represents a character encoding.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public abstract class Encoding : ICloneable
{
	[Serializable]
	internal class DefaultEncoder : Encoder, ISerializable, IObjectReference
	{
		private Encoding m_encoding;

		[NonSerialized]
		private bool m_hasInitializedEncoding;

		[NonSerialized]
		internal char charLeftOver;

		public DefaultEncoder(Encoding encoding)
		{
			m_encoding = encoding;
			m_hasInitializedEncoding = true;
		}

		internal DefaultEncoder(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			m_encoding = (Encoding)info.GetValue("encoding", typeof(Encoding));
			try
			{
				m_fallback = (EncoderFallback)info.GetValue("m_fallback", typeof(EncoderFallback));
				charLeftOver = (char)info.GetValue("charLeftOver", typeof(char));
			}
			catch (SerializationException)
			{
			}
		}

		[SecurityCritical]
		public object GetRealObject(StreamingContext context)
		{
			if (m_hasInitializedEncoding)
			{
				return this;
			}
			Encoder encoder = m_encoding.GetEncoder();
			if (m_fallback != null)
			{
				encoder.m_fallback = m_fallback;
			}
			if (charLeftOver != 0 && encoder is EncoderNLS encoderNLS)
			{
				encoderNLS.charLeftOver = charLeftOver;
			}
			return encoder;
		}

		[SecurityCritical]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("encoding", m_encoding);
		}

		public override int GetByteCount(char[] chars, int index, int count, bool flush)
		{
			return m_encoding.GetByteCount(chars, index, count);
		}

		[SecurityCritical]
		public unsafe override int GetByteCount(char* chars, int count, bool flush)
		{
			return m_encoding.GetByteCount(chars, count);
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush)
		{
			return m_encoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
		}

		[SecurityCritical]
		public unsafe override int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, bool flush)
		{
			return m_encoding.GetBytes(chars, charCount, bytes, byteCount);
		}
	}

	[Serializable]
	internal class DefaultDecoder : Decoder, ISerializable, IObjectReference
	{
		private Encoding m_encoding;

		[NonSerialized]
		private bool m_hasInitializedEncoding;

		public DefaultDecoder(Encoding encoding)
		{
			m_encoding = encoding;
			m_hasInitializedEncoding = true;
		}

		internal DefaultDecoder(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			m_encoding = (Encoding)info.GetValue("encoding", typeof(Encoding));
			try
			{
				m_fallback = (DecoderFallback)info.GetValue("m_fallback", typeof(DecoderFallback));
			}
			catch (SerializationException)
			{
				m_fallback = null;
			}
		}

		[SecurityCritical]
		public object GetRealObject(StreamingContext context)
		{
			if (m_hasInitializedEncoding)
			{
				return this;
			}
			Decoder decoder = m_encoding.GetDecoder();
			if (m_fallback != null)
			{
				decoder.m_fallback = m_fallback;
			}
			return decoder;
		}

		[SecurityCritical]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("encoding", m_encoding);
		}

		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			return GetCharCount(bytes, index, count, flush: false);
		}

		public override int GetCharCount(byte[] bytes, int index, int count, bool flush)
		{
			return m_encoding.GetCharCount(bytes, index, count);
		}

		[SecurityCritical]
		public unsafe override int GetCharCount(byte* bytes, int count, bool flush)
		{
			return m_encoding.GetCharCount(bytes, count);
		}

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			return GetChars(bytes, byteIndex, byteCount, chars, charIndex, flush: false);
		}

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, bool flush)
		{
			return m_encoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
		}

		[SecurityCritical]
		public unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount, bool flush)
		{
			return m_encoding.GetChars(bytes, byteCount, chars, charCount);
		}
	}

	internal class EncodingCharBuffer
	{
		[SecurityCritical]
		private unsafe char* chars;

		[SecurityCritical]
		private unsafe char* charStart;

		[SecurityCritical]
		private unsafe char* charEnd;

		private int charCountResult;

		private Encoding enc;

		private DecoderNLS decoder;

		[SecurityCritical]
		private unsafe byte* byteStart;

		[SecurityCritical]
		private unsafe byte* byteEnd;

		[SecurityCritical]
		private unsafe byte* bytes;

		private DecoderFallbackBuffer fallbackBuffer;

		internal unsafe bool MoreData
		{
			[SecurityCritical]
			get
			{
				return bytes < byteEnd;
			}
		}

		internal unsafe int BytesUsed
		{
			[SecurityCritical]
			get
			{
				return (int)(bytes - byteStart);
			}
		}

		internal int Count => charCountResult;

		[SecurityCritical]
		internal unsafe EncodingCharBuffer(Encoding enc, DecoderNLS decoder, char* charStart, int charCount, byte* byteStart, int byteCount)
		{
			this.enc = enc;
			this.decoder = decoder;
			chars = charStart;
			this.charStart = charStart;
			charEnd = charStart + charCount;
			this.byteStart = byteStart;
			bytes = byteStart;
			byteEnd = byteStart + byteCount;
			if (this.decoder == null)
			{
				fallbackBuffer = enc.DecoderFallback.CreateFallbackBuffer();
			}
			else
			{
				fallbackBuffer = this.decoder.FallbackBuffer;
			}
			fallbackBuffer.InternalInitialize(bytes, charEnd);
		}

		[SecurityCritical]
		internal unsafe bool AddChar(char ch, int numBytes)
		{
			if (chars != null)
			{
				if (chars >= charEnd)
				{
					bytes -= numBytes;
					enc.ThrowCharsOverflow(decoder, bytes <= byteStart);
					return false;
				}
				*(chars++) = ch;
			}
			charCountResult++;
			return true;
		}

		[SecurityCritical]
		internal bool AddChar(char ch)
		{
			return AddChar(ch, 1);
		}

		[SecurityCritical]
		internal unsafe bool AddChar(char ch1, char ch2, int numBytes)
		{
			if (chars >= charEnd - 1)
			{
				bytes -= numBytes;
				enc.ThrowCharsOverflow(decoder, bytes <= byteStart);
				return false;
			}
			if (AddChar(ch1, numBytes))
			{
				return AddChar(ch2, numBytes);
			}
			return false;
		}

		[SecurityCritical]
		internal unsafe void AdjustBytes(int count)
		{
			bytes += count;
		}

		[SecurityCritical]
		internal unsafe bool EvenMoreData(int count)
		{
			return bytes <= byteEnd - count;
		}

		[SecurityCritical]
		internal unsafe byte GetNextByte()
		{
			if (bytes >= byteEnd)
			{
				return 0;
			}
			return *(bytes++);
		}

		[SecurityCritical]
		internal bool Fallback(byte fallbackByte)
		{
			byte[] byteBuffer = new byte[1] { fallbackByte };
			return Fallback(byteBuffer);
		}

		[SecurityCritical]
		internal bool Fallback(byte byte1, byte byte2)
		{
			byte[] byteBuffer = new byte[2] { byte1, byte2 };
			return Fallback(byteBuffer);
		}

		[SecurityCritical]
		internal bool Fallback(byte byte1, byte byte2, byte byte3, byte byte4)
		{
			byte[] byteBuffer = new byte[4] { byte1, byte2, byte3, byte4 };
			return Fallback(byteBuffer);
		}

		[SecurityCritical]
		internal unsafe bool Fallback(byte[] byteBuffer)
		{
			if (chars != null)
			{
				char* ptr = chars;
				if (!fallbackBuffer.InternalFallback(byteBuffer, bytes, ref chars))
				{
					bytes -= byteBuffer.Length;
					fallbackBuffer.InternalReset();
					enc.ThrowCharsOverflow(decoder, chars == charStart);
					return false;
				}
				charCountResult += (int)(chars - ptr);
			}
			else
			{
				charCountResult += fallbackBuffer.InternalFallback(byteBuffer, bytes);
			}
			return true;
		}
	}

	internal class EncodingByteBuffer
	{
		[SecurityCritical]
		private unsafe byte* bytes;

		[SecurityCritical]
		private unsafe byte* byteStart;

		[SecurityCritical]
		private unsafe byte* byteEnd;

		[SecurityCritical]
		private unsafe char* chars;

		[SecurityCritical]
		private unsafe char* charStart;

		[SecurityCritical]
		private unsafe char* charEnd;

		private int byteCountResult;

		private Encoding enc;

		private EncoderNLS encoder;

		internal EncoderFallbackBuffer fallbackBuffer;

		internal unsafe bool MoreData
		{
			[SecurityCritical]
			get
			{
				if (fallbackBuffer.Remaining <= 0)
				{
					return chars < charEnd;
				}
				return true;
			}
		}

		internal unsafe int CharsUsed
		{
			[SecurityCritical]
			get
			{
				return (int)(chars - charStart);
			}
		}

		internal int Count => byteCountResult;

		[SecurityCritical]
		internal unsafe EncodingByteBuffer(Encoding inEncoding, EncoderNLS inEncoder, byte* inByteStart, int inByteCount, char* inCharStart, int inCharCount)
		{
			enc = inEncoding;
			encoder = inEncoder;
			charStart = inCharStart;
			chars = inCharStart;
			charEnd = inCharStart + inCharCount;
			bytes = inByteStart;
			byteStart = inByteStart;
			byteEnd = inByteStart + inByteCount;
			if (encoder == null)
			{
				fallbackBuffer = enc.EncoderFallback.CreateFallbackBuffer();
			}
			else
			{
				fallbackBuffer = encoder.FallbackBuffer;
				if (encoder.m_throwOnOverflow && encoder.InternalHasFallbackBuffer && fallbackBuffer.Remaining > 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Must complete Convert() operation or call Encoder.Reset() before calling GetBytes() or GetByteCount(). Encoder '{0}' fallback '{1}'.", encoder.Encoding.EncodingName, encoder.Fallback.GetType()));
				}
			}
			fallbackBuffer.InternalInitialize(chars, charEnd, encoder, bytes != null);
		}

		[SecurityCritical]
		internal unsafe bool AddByte(byte b, int moreBytesExpected)
		{
			if (bytes != null)
			{
				if (bytes >= byteEnd - moreBytesExpected)
				{
					MovePrevious(bThrow: true);
					return false;
				}
				*(bytes++) = b;
			}
			byteCountResult++;
			return true;
		}

		[SecurityCritical]
		internal bool AddByte(byte b1)
		{
			return AddByte(b1, 0);
		}

		[SecurityCritical]
		internal bool AddByte(byte b1, byte b2)
		{
			return AddByte(b1, b2, 0);
		}

		[SecurityCritical]
		internal bool AddByte(byte b1, byte b2, int moreBytesExpected)
		{
			if (AddByte(b1, 1 + moreBytesExpected))
			{
				return AddByte(b2, moreBytesExpected);
			}
			return false;
		}

		[SecurityCritical]
		internal bool AddByte(byte b1, byte b2, byte b3)
		{
			return AddByte(b1, b2, b3, 0);
		}

		[SecurityCritical]
		internal bool AddByte(byte b1, byte b2, byte b3, int moreBytesExpected)
		{
			if (AddByte(b1, 2 + moreBytesExpected) && AddByte(b2, 1 + moreBytesExpected))
			{
				return AddByte(b3, moreBytesExpected);
			}
			return false;
		}

		[SecurityCritical]
		internal bool AddByte(byte b1, byte b2, byte b3, byte b4)
		{
			if (AddByte(b1, 3) && AddByte(b2, 2) && AddByte(b3, 1))
			{
				return AddByte(b4, 0);
			}
			return false;
		}

		[SecurityCritical]
		internal unsafe void MovePrevious(bool bThrow)
		{
			if (fallbackBuffer.bFallingBack)
			{
				fallbackBuffer.MovePrevious();
			}
			else if (chars > charStart)
			{
				chars--;
			}
			if (bThrow)
			{
				enc.ThrowBytesOverflow(encoder, bytes == byteStart);
			}
		}

		[SecurityCritical]
		internal unsafe bool Fallback(char charFallback)
		{
			return fallbackBuffer.InternalFallback(charFallback, ref chars);
		}

		[SecurityCritical]
		internal unsafe char GetNextChar()
		{
			char c = fallbackBuffer.InternalGetNextChar();
			if (c == '\0' && chars < charEnd)
			{
				c = *(chars++);
			}
			return c;
		}
	}

	private static volatile Encoding defaultEncoding;

	private static volatile Encoding unicodeEncoding;

	private static volatile Encoding bigEndianUnicode;

	private static volatile Encoding utf7Encoding;

	private static volatile Encoding utf8Encoding;

	private static volatile Encoding utf32Encoding;

	private static volatile Encoding asciiEncoding;

	private static volatile Encoding latin1Encoding;

	private static volatile Hashtable encodings;

	private const int MIMECONTF_MAILNEWS = 1;

	private const int MIMECONTF_BROWSER = 2;

	private const int MIMECONTF_SAVABLE_MAILNEWS = 256;

	private const int MIMECONTF_SAVABLE_BROWSER = 512;

	private const int CodePageDefault = 0;

	private const int CodePageNoOEM = 1;

	private const int CodePageNoMac = 2;

	private const int CodePageNoThread = 3;

	private const int CodePageNoSymbol = 42;

	private const int CodePageUnicode = 1200;

	private const int CodePageBigEndian = 1201;

	private const int CodePageWindows1252 = 1252;

	private const int CodePageMacGB2312 = 10008;

	private const int CodePageGB2312 = 20936;

	private const int CodePageMacKorean = 10003;

	private const int CodePageDLLKorean = 20949;

	private const int ISO2022JP = 50220;

	private const int ISO2022JPESC = 50221;

	private const int ISO2022JPSISO = 50222;

	private const int ISOKorean = 50225;

	private const int ISOSimplifiedCN = 50227;

	private const int EUCJP = 51932;

	private const int ChineseHZ = 52936;

	private const int DuplicateEUCCN = 51936;

	private const int EUCCN = 936;

	private const int EUCKR = 51949;

	internal const int CodePageASCII = 20127;

	internal const int ISO_8859_1 = 28591;

	private const int ISCIIAssemese = 57006;

	private const int ISCIIBengali = 57003;

	private const int ISCIIDevanagari = 57002;

	private const int ISCIIGujarathi = 57010;

	private const int ISCIIKannada = 57008;

	private const int ISCIIMalayalam = 57009;

	private const int ISCIIOriya = 57007;

	private const int ISCIIPanjabi = 57011;

	private const int ISCIITamil = 57004;

	private const int ISCIITelugu = 57005;

	private const int GB18030 = 54936;

	private const int ISO_8859_8I = 38598;

	private const int ISO_8859_8_Visual = 28598;

	private const int ENC50229 = 50229;

	private const int CodePageUTF7 = 65000;

	private const int CodePageUTF8 = 65001;

	private const int CodePageUTF32 = 12000;

	private const int CodePageUTF32BE = 12001;

	internal int m_codePage;

	internal CodePageDataItem dataItem;

	[NonSerialized]
	internal bool m_deserializedFromEverett;

	[OptionalField(VersionAdded = 2)]
	private bool m_isReadOnly = true;

	[OptionalField(VersionAdded = 2)]
	internal EncoderFallback encoderFallback;

	[OptionalField(VersionAdded = 2)]
	internal DecoderFallback decoderFallback;

	private static object s_InternalSyncObject;

	private static object InternalSyncObject
	{
		get
		{
			if (s_InternalSyncObject == null)
			{
				object value = new object();
				Interlocked.CompareExchange<object>(ref s_InternalSyncObject, value, (object)null);
			}
			return s_InternalSyncObject;
		}
	}

	/// <summary>When overridden in a derived class, gets a name for the current encoding that can be used with mail agent body tags.</summary>
	/// <returns>A name for the current <see cref="T:System.Text.Encoding" /> that can be used with mail agent body tags.-or- An empty string (""), if the current <see cref="T:System.Text.Encoding" /> cannot be used.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual string BodyName
	{
		get
		{
			if (dataItem == null)
			{
				GetDataItem();
			}
			return dataItem.BodyName;
		}
	}

	/// <summary>When overridden in a derived class, gets the human-readable description of the current encoding.</summary>
	/// <returns>The human-readable description of the current <see cref="T:System.Text.Encoding" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual string EncodingName => Environment.GetResourceStringEncodingName(m_codePage);

	/// <summary>When overridden in a derived class, gets a name for the current encoding that can be used with mail agent header tags.</summary>
	/// <returns>A name for the current <see cref="T:System.Text.Encoding" /> to use with mail agent header tags.-or- An empty string (""), if the current <see cref="T:System.Text.Encoding" /> cannot be used.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual string HeaderName
	{
		get
		{
			if (dataItem == null)
			{
				GetDataItem();
			}
			return dataItem.HeaderName;
		}
	}

	/// <summary>When overridden in a derived class, gets the name registered with the Internet Assigned Numbers Authority (IANA) for the current encoding.</summary>
	/// <returns>The IANA name for the current <see cref="T:System.Text.Encoding" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual string WebName
	{
		get
		{
			if (dataItem == null)
			{
				GetDataItem();
			}
			return dataItem.WebName;
		}
	}

	/// <summary>When overridden in a derived class, gets the Windows operating system code page that most closely corresponds to the current encoding.</summary>
	/// <returns>The Windows operating system code page that most closely corresponds to the current <see cref="T:System.Text.Encoding" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual int WindowsCodePage
	{
		get
		{
			if (dataItem == null)
			{
				GetDataItem();
			}
			return dataItem.UIFamilyCodePage;
		}
	}

	/// <summary>When overridden in a derived class, gets a value indicating whether the current encoding can be used by browser clients for displaying content.</summary>
	/// <returns>true if the current <see cref="T:System.Text.Encoding" /> can be used by browser clients for displaying content; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsBrowserDisplay
	{
		get
		{
			if (dataItem == null)
			{
				GetDataItem();
			}
			return (dataItem.Flags & 2) != 0;
		}
	}

	/// <summary>When overridden in a derived class, gets a value indicating whether the current encoding can be used by browser clients for saving content.</summary>
	/// <returns>true if the current <see cref="T:System.Text.Encoding" /> can be used by browser clients for saving content; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsBrowserSave
	{
		get
		{
			if (dataItem == null)
			{
				GetDataItem();
			}
			return (dataItem.Flags & 0x200) != 0;
		}
	}

	/// <summary>When overridden in a derived class, gets a value indicating whether the current encoding can be used by mail and news clients for displaying content.</summary>
	/// <returns>true if the current <see cref="T:System.Text.Encoding" /> can be used by mail and news clients for displaying content; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsMailNewsDisplay
	{
		get
		{
			if (dataItem == null)
			{
				GetDataItem();
			}
			return (dataItem.Flags & 1) != 0;
		}
	}

	/// <summary>When overridden in a derived class, gets a value indicating whether the current encoding can be used by mail and news clients for saving content.</summary>
	/// <returns>true if the current <see cref="T:System.Text.Encoding" /> can be used by mail and news clients for saving content; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsMailNewsSave
	{
		get
		{
			if (dataItem == null)
			{
				GetDataItem();
			}
			return (dataItem.Flags & 0x100) != 0;
		}
	}

	/// <summary>When overridden in a derived class, gets a value indicating whether the current encoding uses single-byte code points.</summary>
	/// <returns>true if the current <see cref="T:System.Text.Encoding" /> uses single-byte code points; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public virtual bool IsSingleByte => false;

	/// <summary>Gets or sets the <see cref="T:System.Text.EncoderFallback" /> object for the current <see cref="T:System.Text.Encoding" /> object.</summary>
	/// <returns>The encoder fallback object for the current <see cref="T:System.Text.Encoding" /> object. </returns>
	/// <exception cref="T:System.ArgumentNullException">The value in a set operation is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">A value cannot be assigned in a set operation because the current <see cref="T:System.Text.Encoding" /> object is read-only.</exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public EncoderFallback EncoderFallback
	{
		get
		{
			return encoderFallback;
		}
		set
		{
			if (IsReadOnly)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Instance is read-only."));
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			encoderFallback = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Text.DecoderFallback" /> object for the current <see cref="T:System.Text.Encoding" /> object.</summary>
	/// <returns>The decoder fallback object for the current <see cref="T:System.Text.Encoding" /> object. </returns>
	/// <exception cref="T:System.ArgumentNullException">The value in a set operation is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">A value cannot be assigned in a set operation because the current <see cref="T:System.Text.Encoding" /> object is read-only.</exception>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public DecoderFallback DecoderFallback
	{
		get
		{
			return decoderFallback;
		}
		set
		{
			if (IsReadOnly)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Instance is read-only."));
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			decoderFallback = value;
		}
	}

	/// <summary>When overridden in a derived class, gets a value indicating whether the current encoding is read-only.</summary>
	/// <returns>true if the current <see cref="T:System.Text.Encoding" /> is read-only; otherwise, false. The default is true.</returns>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public bool IsReadOnly => m_isReadOnly;

	/// <summary>Gets an encoding for the ASCII (7-bit) character set.</summary>
	/// <returns>An  encoding for the ASCII (7-bit) character set.</returns>
	/// <filterpriority>1</filterpriority>
	public static Encoding ASCII
	{
		get
		{
			if (asciiEncoding == null)
			{
				asciiEncoding = new ASCIIEncoding();
			}
			return asciiEncoding;
		}
	}

	private static Encoding Latin1
	{
		get
		{
			if (latin1Encoding == null)
			{
				latin1Encoding = new Latin1Encoding();
			}
			return latin1Encoding;
		}
	}

	/// <summary>When overridden in a derived class, gets the code page identifier of the current <see cref="T:System.Text.Encoding" />.</summary>
	/// <returns>The code page identifier of the current <see cref="T:System.Text.Encoding" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual int CodePage => m_codePage;

	/// <summary>Gets an encoding for the operating system's current ANSI code page.</summary>
	/// <returns>An encoding for the operating system's current ANSI code page.</returns>
	/// <filterpriority>1</filterpriority>
	public static Encoding Default
	{
		[SecuritySafeCritical]
		get
		{
			if (defaultEncoding == null)
			{
				defaultEncoding = CreateDefaultEncoding();
			}
			return defaultEncoding;
		}
	}

	/// <summary>Gets an encoding for the UTF-16 format using the little endian byte order.</summary>
	/// <returns>An encoding for the UTF-16 format using the little endian byte order.</returns>
	/// <filterpriority>1</filterpriority>
	public static Encoding Unicode
	{
		get
		{
			if (unicodeEncoding == null)
			{
				unicodeEncoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: true);
			}
			return unicodeEncoding;
		}
	}

	/// <summary>Gets an encoding for the UTF-16 format that uses the big endian byte order.</summary>
	/// <returns>An encoding object for the UTF-16 format that uses the big endian byte order.</returns>
	/// <filterpriority>1</filterpriority>
	public static Encoding BigEndianUnicode
	{
		get
		{
			if (bigEndianUnicode == null)
			{
				bigEndianUnicode = new UnicodeEncoding(bigEndian: true, byteOrderMark: true);
			}
			return bigEndianUnicode;
		}
	}

	/// <summary>Gets an encoding for the UTF-7 format.</summary>
	/// <returns>An encoding for the UTF-7 format.</returns>
	/// <filterpriority>1</filterpriority>
	public static Encoding UTF7
	{
		get
		{
			if (utf7Encoding == null)
			{
				utf7Encoding = new UTF7Encoding();
			}
			return utf7Encoding;
		}
	}

	/// <summary>Gets an encoding for the UTF-8 format.</summary>
	/// <returns>An encoding for the UTF-8 format.</returns>
	/// <filterpriority>1</filterpriority>
	public static Encoding UTF8
	{
		get
		{
			if (utf8Encoding == null)
			{
				utf8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
			}
			return utf8Encoding;
		}
	}

	/// <summary>Gets an encoding for the UTF-32 format using the little endian byte order.</summary>
	/// <returns>An  encoding object for the UTF-32 format using the little endian byte order.</returns>
	/// <filterpriority>1</filterpriority>
	public static Encoding UTF32
	{
		get
		{
			if (utf32Encoding == null)
			{
				utf32Encoding = new UTF32Encoding(bigEndian: false, byteOrderMark: true);
			}
			return utf32Encoding;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.Encoding" /> class.</summary>
	protected Encoding()
		: this(0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.Encoding" /> class that corresponds to the specified code page.</summary>
	/// <param name="codePage">The code page identifier of the preferred encoding.-or- 0, to use the default encoding. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="codePage" /> is less than zero. </exception>
	protected Encoding(int codePage)
	{
		if (codePage < 0)
		{
			throw new ArgumentOutOfRangeException("codePage");
		}
		m_codePage = codePage;
		SetDefaultFallbacks();
	}

	protected Encoding(int codePage, EncoderFallback encoderFallback, DecoderFallback decoderFallback)
	{
		if (codePage < 0)
		{
			throw new ArgumentOutOfRangeException("codePage");
		}
		m_codePage = codePage;
		this.encoderFallback = encoderFallback ?? new InternalEncoderBestFitFallback(this);
		this.decoderFallback = decoderFallback ?? new InternalDecoderBestFitFallback(this);
	}

	internal virtual void SetDefaultFallbacks()
	{
		encoderFallback = new InternalEncoderBestFitFallback(this);
		decoderFallback = new InternalDecoderBestFitFallback(this);
	}

	internal void OnDeserializing()
	{
		encoderFallback = null;
		decoderFallback = null;
		m_isReadOnly = true;
	}

	internal void OnDeserialized()
	{
		if (encoderFallback == null || decoderFallback == null)
		{
			m_deserializedFromEverett = true;
			SetDefaultFallbacks();
		}
		dataItem = null;
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
	}

	[OnSerializing]
	private void OnSerializing(StreamingContext ctx)
	{
		dataItem = null;
	}

	internal void DeserializeEncoding(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		m_codePage = (int)info.GetValue("m_codePage", typeof(int));
		dataItem = null;
		try
		{
			m_isReadOnly = (bool)info.GetValue("m_isReadOnly", typeof(bool));
			encoderFallback = (EncoderFallback)info.GetValue("encoderFallback", typeof(EncoderFallback));
			decoderFallback = (DecoderFallback)info.GetValue("decoderFallback", typeof(DecoderFallback));
		}
		catch (SerializationException)
		{
			m_deserializedFromEverett = true;
			m_isReadOnly = true;
			SetDefaultFallbacks();
		}
	}

	internal void SerializeEncoding(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("m_isReadOnly", m_isReadOnly);
		info.AddValue("encoderFallback", EncoderFallback);
		info.AddValue("decoderFallback", DecoderFallback);
		info.AddValue("m_codePage", m_codePage);
		info.AddValue("dataItem", null);
		info.AddValue("Encoding+m_codePage", m_codePage);
		info.AddValue("Encoding+dataItem", null);
	}

	/// <summary>Converts an entire byte array from one encoding to another.</summary>
	/// <returns>An array of type <see cref="T:System.Byte" /> containing the results of converting <paramref name="bytes" /> from <paramref name="srcEncoding" /> to <paramref name="dstEncoding" />.</returns>
	/// <param name="srcEncoding">The encoding format of <paramref name="bytes" />. </param>
	/// <param name="dstEncoding">The target encoding format. </param>
	/// <param name="bytes">The bytes to convert. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="srcEncoding" /> is null.-or- <paramref name="dstEncoding" /> is null.-or- <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-srcEncoding.<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-dstEncoding.<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding, byte[] bytes)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		return Convert(srcEncoding, dstEncoding, bytes, 0, bytes.Length);
	}

	/// <summary>Converts a range of bytes in a byte array from one encoding to another.</summary>
	/// <returns>An array of type <see cref="T:System.Byte" /> containing the result of converting a range of bytes in <paramref name="bytes" /> from <paramref name="srcEncoding" /> to <paramref name="dstEncoding" />.</returns>
	/// <param name="srcEncoding">The encoding of the source array, <paramref name="bytes" />. </param>
	/// <param name="dstEncoding">The encoding of the output array. </param>
	/// <param name="bytes">The array of bytes to convert. </param>
	/// <param name="index">The index of the first element of <paramref name="bytes" /> to convert. </param>
	/// <param name="count">The number of bytes to convert. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="srcEncoding" /> is null.-or- <paramref name="dstEncoding" /> is null.-or- <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> and <paramref name="count" /> do not specify a valid range in the byte array. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-srcEncoding.<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-dstEncoding.<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding, byte[] bytes, int index, int count)
	{
		if (srcEncoding == null || dstEncoding == null)
		{
			throw new ArgumentNullException((srcEncoding == null) ? "srcEncoding" : "dstEncoding", Environment.GetResourceString("Array cannot be null."));
		}
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes", Environment.GetResourceString("Array cannot be null."));
		}
		return dstEncoding.GetBytes(srcEncoding.GetChars(bytes, index, count));
	}

	[SecurityCritical]
	public static void RegisterProvider(EncodingProvider provider)
	{
		EncodingProvider.AddProvider(provider);
	}

	/// <summary>Returns the encoding associated with the specified code page identifier.</summary>
	/// <returns>The encoding that is associated with the specified code page.</returns>
	/// <param name="codepage">The code page identifier of the preferred encoding. Possible values are listed in the Code Page column of the table that appears in the <see cref="T:System.Text.Encoding" /> class topic.-or- 0 (zero), to use the default encoding. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="codepage" /> is less than zero or greater than 65535. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="codepage" /> is not supported by the underlying platform. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="codepage" /> is not supported by the underlying platform. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static Encoding GetEncoding(int codepage)
	{
		Encoding encoding = EncodingProvider.GetEncodingFromProvider(codepage);
		if (encoding != null)
		{
			return encoding;
		}
		if (codepage < 0 || codepage > 65535)
		{
			throw new ArgumentOutOfRangeException("codepage", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 0, 65535));
		}
		if (encodings != null)
		{
			encoding = (Encoding)encodings[codepage];
		}
		if (encoding == null)
		{
			lock (InternalSyncObject)
			{
				if (encodings == null)
				{
					encodings = new Hashtable();
				}
				if ((encoding = (Encoding)encodings[codepage]) != null)
				{
					return encoding;
				}
				switch (codepage)
				{
				case 0:
					encoding = Default;
					break;
				case 1200:
					encoding = Unicode;
					break;
				case 1201:
					encoding = BigEndianUnicode;
					break;
				case 65000:
					encoding = UTF7;
					break;
				case 12000:
					encoding = UTF32;
					break;
				case 12001:
					encoding = new UTF32Encoding(bigEndian: true, byteOrderMark: true);
					break;
				case 65001:
					encoding = UTF8;
					break;
				case 1:
				case 2:
				case 3:
				case 42:
					throw new ArgumentException(Environment.GetResourceString("{0} is not a supported code page.", codepage), "codepage");
				case 20127:
					encoding = ASCII;
					break;
				case 28591:
					encoding = Latin1;
					break;
				default:
					if (EncodingTable.GetCodePageDataItem(codepage) == null)
					{
						throw new NotSupportedException(Environment.GetResourceString("No data is available for encoding {0}. For information on defining a custom encoding, see the documentation for the Encoding.RegisterProvider method.", codepage));
					}
					switch (codepage)
					{
					case 12000:
						encoding = UTF32;
						break;
					case 12001:
						encoding = new UTF32Encoding(bigEndian: true, byteOrderMark: true);
						break;
					default:
						encoding = (Encoding)EncodingHelper.InvokeI18N("GetEncoding", codepage);
						if (encoding == null)
						{
							throw new NotSupportedException($"Encoding {codepage} data could not be found. Make sure you have correct international codeset assembly installed and enabled.");
						}
						break;
					}
					break;
				}
				encodings.Add(codepage, encoding);
			}
		}
		return encoding;
	}

	/// <summary>Returns the encoding associated with the specified code page identifier. Parameters specify an error handler for characters that cannot be encoded and byte sequences that cannot be decoded.</summary>
	/// <returns>The encoding that is associated with the specified code page.</returns>
	/// <param name="codepage">The code page identifier of the preferred encoding. Possible values are listed in the Code Page column of the table that appears in the <see cref="T:System.Text.Encoding" /> class topic.-or- 0 (zero), to use the default encoding. </param>
	/// <param name="encoderFallback">An object that provides an error-handling procedure when a character cannot be encoded with the current encoding. </param>
	/// <param name="decoderFallback">An object that provides an error-handling procedure when a byte sequence cannot be decoded with the current encoding. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="codepage" /> is less than zero or greater than 65535. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="codepage" /> is not supported by the underlying platform. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="codepage" /> is not supported by the underlying platform. </exception>
	/// <filterpriority>1</filterpriority>
	public static Encoding GetEncoding(int codepage, EncoderFallback encoderFallback, DecoderFallback decoderFallback)
	{
		Encoding encodingFromProvider = EncodingProvider.GetEncodingFromProvider(codepage, encoderFallback, decoderFallback);
		if (encodingFromProvider != null)
		{
			return encodingFromProvider;
		}
		encodingFromProvider = GetEncoding(codepage);
		Encoding obj = (Encoding)encodingFromProvider.Clone();
		obj.EncoderFallback = encoderFallback;
		obj.DecoderFallback = decoderFallback;
		return obj;
	}

	/// <summary>Returns the encoding associated with the specified code page name.</summary>
	/// <returns>The encoding  associated with the specified code page.</returns>
	/// <param name="name">The code page name of the preferred encoding. Any value returned by the <see cref="P:System.Text.Encoding.WebName" /> property is valid. Possible values are listed in the Name column of the table that appears in the <see cref="T:System.Text.Encoding" /> class topic.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is not a valid code page name.-or- The code page indicated by <paramref name="name" /> is not supported by the underlying platform. </exception>
	/// <filterpriority>1</filterpriority>
	public static Encoding GetEncoding(string name)
	{
		Encoding encodingFromProvider = EncodingProvider.GetEncodingFromProvider(name);
		if (encodingFromProvider != null)
		{
			return encodingFromProvider;
		}
		return GetEncoding(EncodingTable.GetCodePageFromName(name));
	}

	/// <summary>Returns the encoding associated with the specified code page name. Parameters specify an error handler for characters that cannot be encoded and byte sequences that cannot be decoded.</summary>
	/// <returns>The encoding that is associated with the specified code page.</returns>
	/// <param name="name">The code page name of the preferred encoding. Any value returned by the <see cref="P:System.Text.Encoding.WebName" /> property is valid. Possible values are listed in the Name column of the table that appears in the <see cref="T:System.Text.Encoding" /> class topic.</param>
	/// <param name="encoderFallback">An object that provides an error-handling procedure when a character cannot be encoded with the current encoding. </param>
	/// <param name="decoderFallback">An object that provides an error-handling procedure when a byte sequence cannot be decoded with the current encoding. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is not a valid code page name.-or- The code page indicated by <paramref name="name" /> is not supported by the underlying platform. </exception>
	/// <filterpriority>1</filterpriority>
	public static Encoding GetEncoding(string name, EncoderFallback encoderFallback, DecoderFallback decoderFallback)
	{
		Encoding encodingFromProvider = EncodingProvider.GetEncodingFromProvider(name, encoderFallback, decoderFallback);
		if (encodingFromProvider != null)
		{
			return encodingFromProvider;
		}
		return GetEncoding(EncodingTable.GetCodePageFromName(name), encoderFallback, decoderFallback);
	}

	/// <summary>Returns an array that contains all encodings.</summary>
	/// <returns>An array that contains all encodings.</returns>
	/// <filterpriority>1</filterpriority>
	public static EncodingInfo[] GetEncodings()
	{
		return EncodingTable.GetEncodings();
	}

	/// <summary>When overridden in a derived class, returns a sequence of bytes that specifies the encoding used.</summary>
	/// <returns>A byte array containing a sequence of bytes that specifies the encoding used.-or- A byte array of length zero, if a preamble is not required.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual byte[] GetPreamble()
	{
		return EmptyArray<byte>.Value;
	}

	private void GetDataItem()
	{
		if (dataItem == null)
		{
			dataItem = EncodingTable.GetCodePageDataItem(m_codePage);
			if (dataItem == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("No data is available for encoding {0}. For information on defining a custom encoding, see the documentation for the Encoding.RegisterProvider method.", m_codePage));
			}
		}
	}

	/// <summary>When overridden in a derived class, creates a shallow copy of the current <see cref="T:System.Text.Encoding" /> object.</summary>
	/// <returns>A copy of the current <see cref="T:System.Text.Encoding" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public virtual object Clone()
	{
		Encoding obj = (Encoding)MemberwiseClone();
		obj.m_isReadOnly = false;
		return obj;
	}

	/// <summary>When overridden in a derived class, calculates the number of bytes produced by encoding all the characters in the specified character array.</summary>
	/// <returns>The number of bytes produced by encoding all the characters in the specified character array.</returns>
	/// <param name="chars">The character array containing the characters to encode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual int GetByteCount(char[] chars)
	{
		if (chars == null)
		{
			throw new ArgumentNullException("chars", Environment.GetResourceString("Array cannot be null."));
		}
		return GetByteCount(chars, 0, chars.Length);
	}

	/// <summary>When overridden in a derived class, calculates the number of bytes produced by encoding the characters in the specified string.</summary>
	/// <returns>The number of bytes produced by encoding the specified characters.</returns>
	/// <param name="s">The string containing the set of characters to encode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual int GetByteCount(string s)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		char[] array = s.ToCharArray();
		return GetByteCount(array, 0, array.Length);
	}

	/// <summary>When overridden in a derived class, calculates the number of bytes produced by encoding a set of characters from the specified character array.</summary>
	/// <returns>The number of bytes produced by encoding the specified characters.</returns>
	/// <param name="chars">The character array containing the set of characters to encode. </param>
	/// <param name="index">The index of the first character to encode. </param>
	/// <param name="count">The number of characters to encode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="chars" />. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public abstract int GetByteCount(char[] chars, int index, int count);

	/// <summary>When overridden in a derived class, calculates the number of bytes produced by encoding a set of characters starting at the specified character pointer.</summary>
	/// <returns>The number of bytes produced by encoding the specified characters.</returns>
	/// <param name="chars">A pointer to the first character to encode. </param>
	/// <param name="count">The number of characters to encode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecurityCritical]
	[ComVisible(false)]
	[CLSCompliant(false)]
	public unsafe virtual int GetByteCount(char* chars, int count)
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
		return GetByteCount(array, 0, count);
	}

	[SecurityCritical]
	internal unsafe virtual int GetByteCount(char* chars, int count, EncoderNLS encoder)
	{
		return GetByteCount(chars, count);
	}

	/// <summary>When overridden in a derived class, encodes all the characters in the specified character array into a sequence of bytes.</summary>
	/// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
	/// <param name="chars">The character array containing the characters to encode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual byte[] GetBytes(char[] chars)
	{
		if (chars == null)
		{
			throw new ArgumentNullException("chars", Environment.GetResourceString("Array cannot be null."));
		}
		return GetBytes(chars, 0, chars.Length);
	}

	/// <summary>When overridden in a derived class, encodes a set of characters from the specified character array into a sequence of bytes.</summary>
	/// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
	/// <param name="chars">The character array containing the set of characters to encode. </param>
	/// <param name="index">The index of the first character to encode. </param>
	/// <param name="count">The number of characters to encode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="chars" />. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual byte[] GetBytes(char[] chars, int index, int count)
	{
		byte[] array = new byte[GetByteCount(chars, index, count)];
		GetBytes(chars, index, count, array, 0);
		return array;
	}

	/// <summary>When overridden in a derived class, encodes a set of characters from the specified character array into the specified byte array.</summary>
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
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="bytes" /> does not have enough capacity from <paramref name="byteIndex" /> to the end of the array to accommodate the resulting bytes. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public abstract int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex);

	/// <summary>When overridden in a derived class, encodes all the characters in the specified string into a sequence of bytes.</summary>
	/// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
	/// <param name="s">The string containing the characters to encode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual byte[] GetBytes(string s)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s", Environment.GetResourceString("String reference not set to an instance of a String."));
		}
		byte[] array = new byte[GetByteCount(s)];
		GetBytes(s, 0, s.Length, array, 0);
		return array;
	}

	/// <summary>When overridden in a derived class, encodes a set of characters from the specified string into the specified byte array.</summary>
	/// <returns>The actual number of bytes written into <paramref name="bytes" />.</returns>
	/// <param name="s">The string containing the set of characters to encode. </param>
	/// <param name="charIndex">The index of the first character to encode. </param>
	/// <param name="charCount">The number of characters to encode. </param>
	/// <param name="bytes">The byte array to contain the resulting sequence of bytes. </param>
	/// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null.-or- <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charIndex" /> or <paramref name="charCount" /> or <paramref name="byteIndex" /> is less than zero.-or- <paramref name="charIndex" /> and <paramref name="charCount" /> do not denote a valid range in <paramref name="chars" />.-or- <paramref name="byteIndex" /> is not a valid index in <paramref name="bytes" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="bytes" /> does not have enough capacity from <paramref name="byteIndex" /> to the end of the array to accommodate the resulting bytes. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		return GetBytes(s.ToCharArray(), charIndex, charCount, bytes, byteIndex);
	}

	[SecurityCritical]
	internal unsafe virtual int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS encoder)
	{
		return GetBytes(chars, charCount, bytes, byteCount);
	}

	/// <summary>When overridden in a derived class, encodes a set of characters starting at the specified character pointer into a sequence of bytes that are stored starting at the specified byte pointer.</summary>
	/// <returns>The actual number of bytes written at the location indicated by the <paramref name="bytes" /> parameter.</returns>
	/// <param name="chars">A pointer to the first character to encode. </param>
	/// <param name="charCount">The number of characters to encode. </param>
	/// <param name="bytes">A pointer to the location at which to start writing the resulting sequence of bytes. </param>
	/// <param name="byteCount">The maximum number of bytes to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="chars" /> is null.-or- <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charCount" /> or <paramref name="byteCount" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="byteCount" /> is less than the resulting number of bytes. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecurityCritical]
	[ComVisible(false)]
	[CLSCompliant(false)]
	public unsafe virtual int GetBytes(char* chars, int charCount, byte* bytes, int byteCount)
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
		int bytes2 = GetBytes(array, 0, charCount, array2, 0);
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

	/// <summary>When overridden in a derived class, calculates the number of characters produced by decoding all the bytes in the specified byte array.</summary>
	/// <returns>The number of characters produced by decoding the specified sequence of bytes.</returns>
	/// <param name="bytes">The byte array containing the sequence of bytes to decode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual int GetCharCount(byte[] bytes)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes", Environment.GetResourceString("Array cannot be null."));
		}
		return GetCharCount(bytes, 0, bytes.Length);
	}

	/// <summary>When overridden in a derived class, calculates the number of characters produced by decoding a sequence of bytes from the specified byte array.</summary>
	/// <returns>The number of characters produced by decoding the specified sequence of bytes.</returns>
	/// <param name="bytes">The byte array containing the sequence of bytes to decode. </param>
	/// <param name="index">The index of the first byte to decode. </param>
	/// <param name="count">The number of bytes to decode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="bytes" />. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public abstract int GetCharCount(byte[] bytes, int index, int count);

	/// <summary>When overridden in a derived class, calculates the number of characters produced by decoding a sequence of bytes starting at the specified byte pointer.</summary>
	/// <returns>The number of characters produced by decoding the specified sequence of bytes.</returns>
	/// <param name="bytes">A pointer to the first byte to decode. </param>
	/// <param name="count">The number of bytes to decode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	[ComVisible(false)]
	[SecurityCritical]
	public unsafe virtual int GetCharCount(byte* bytes, int count)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes", Environment.GetResourceString("Array cannot be null."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		byte[] array = new byte[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = bytes[i];
		}
		return GetCharCount(array, 0, count);
	}

	[SecurityCritical]
	internal unsafe virtual int GetCharCount(byte* bytes, int count, DecoderNLS decoder)
	{
		return GetCharCount(bytes, count);
	}

	/// <summary>When overridden in a derived class, decodes all the bytes in the specified byte array into a set of characters.</summary>
	/// <returns>A character array containing the results of decoding the specified sequence of bytes.</returns>
	/// <param name="bytes">The byte array containing the sequence of bytes to decode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual char[] GetChars(byte[] bytes)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes", Environment.GetResourceString("Array cannot be null."));
		}
		return GetChars(bytes, 0, bytes.Length);
	}

	/// <summary>When overridden in a derived class, decodes a sequence of bytes from the specified byte array into a set of characters.</summary>
	/// <returns>A character array containing the results of decoding the specified sequence of bytes.</returns>
	/// <param name="bytes">The byte array containing the sequence of bytes to decode. </param>
	/// <param name="index">The index of the first byte to decode. </param>
	/// <param name="count">The number of bytes to decode. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="bytes" />. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual char[] GetChars(byte[] bytes, int index, int count)
	{
		char[] array = new char[GetCharCount(bytes, index, count)];
		GetChars(bytes, index, count, array, 0);
		return array;
	}

	/// <summary>When overridden in a derived class, decodes a sequence of bytes from the specified byte array into the specified character array.</summary>
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
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="chars" /> does not have enough capacity from <paramref name="charIndex" /> to the end of the array to accommodate the resulting characters. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public abstract int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex);

	/// <summary>When overridden in a derived class, decodes a sequence of bytes starting at the specified byte pointer into a set of characters that are stored starting at the specified character pointer.</summary>
	/// <returns>The actual number of characters written at the location indicated by the <paramref name="chars" /> parameter.</returns>
	/// <param name="bytes">A pointer to the first byte to decode. </param>
	/// <param name="byteCount">The number of bytes to decode. </param>
	/// <param name="chars">A pointer to the location at which to start writing the resulting set of characters. </param>
	/// <param name="charCount">The maximum number of characters to write. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null.-or- <paramref name="chars" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="byteCount" /> or <paramref name="charCount" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="charCount" /> is less than the resulting number of characters. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	[CLSCompliant(false)]
	[SecurityCritical]
	public unsafe virtual int GetChars(byte* bytes, int byteCount, char* chars, int charCount)
	{
		if (chars == null || bytes == null)
		{
			throw new ArgumentNullException((chars == null) ? "chars" : "bytes", Environment.GetResourceString("Array cannot be null."));
		}
		if (byteCount < 0 || charCount < 0)
		{
			throw new ArgumentOutOfRangeException((byteCount < 0) ? "byteCount" : "charCount", Environment.GetResourceString("Non-negative number required."));
		}
		byte[] array = new byte[byteCount];
		for (int i = 0; i < byteCount; i++)
		{
			array[i] = bytes[i];
		}
		char[] array2 = new char[charCount];
		int chars2 = GetChars(array, 0, byteCount, array2, 0);
		if (chars2 < charCount)
		{
			charCount = chars2;
		}
		for (int i = 0; i < charCount; i++)
		{
			chars[i] = array2[i];
		}
		return charCount;
	}

	[SecurityCritical]
	internal unsafe virtual int GetChars(byte* bytes, int byteCount, char* chars, int charCount, DecoderNLS decoder)
	{
		return GetChars(bytes, byteCount, chars, charCount);
	}

	[CLSCompliant(false)]
	[SecurityCritical]
	[ComVisible(false)]
	public unsafe string GetString(byte* bytes, int byteCount)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes", Environment.GetResourceString("Array cannot be null."));
		}
		if (byteCount < 0)
		{
			throw new ArgumentOutOfRangeException("byteCount", Environment.GetResourceString("Non-negative number required."));
		}
		return string.CreateStringFromEncoding(bytes, byteCount, this);
	}

	/// <summary>Gets a value indicating whether the current encoding is always normalized, using the default normalization form.</summary>
	/// <returns>true if the current <see cref="T:System.Text.Encoding" /> is always normalized; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public bool IsAlwaysNormalized()
	{
		return IsAlwaysNormalized(NormalizationForm.FormC);
	}

	/// <summary>When overridden in a derived class, gets a value indicating whether the current encoding is always normalized, using the specified normalization form.</summary>
	/// <returns>true if the current <see cref="T:System.Text.Encoding" /> object is always normalized using the specified <see cref="T:System.Text.NormalizationForm" /> value; otherwise, false. The default is false.</returns>
	/// <param name="form">One of the <see cref="T:System.Text.NormalizationForm" /> values. </param>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public virtual bool IsAlwaysNormalized(NormalizationForm form)
	{
		return false;
	}

	/// <summary>When overridden in a derived class, obtains a decoder that converts an encoded sequence of bytes into a sequence of characters.</summary>
	/// <returns>A <see cref="T:System.Text.Decoder" /> that converts an encoded sequence of bytes into a sequence of characters.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual Decoder GetDecoder()
	{
		return new DefaultDecoder(this);
	}

	[SecurityCritical]
	private static Encoding CreateDefaultEncoding()
	{
		Encoding encoding = EncodingHelper.GetDefaultEncoding();
		encoding.m_isReadOnly = true;
		return encoding;
	}

	internal void setReadOnly(bool value = true)
	{
		m_isReadOnly = value;
	}

	/// <summary>When overridden in a derived class, obtains an encoder that converts a sequence of Unicode characters into an encoded sequence of bytes.</summary>
	/// <returns>A <see cref="T:System.Text.Encoder" /> that converts a sequence of Unicode characters into an encoded sequence of bytes.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual Encoder GetEncoder()
	{
		return new DefaultEncoder(this);
	}

	/// <summary>When overridden in a derived class, calculates the maximum number of bytes produced by encoding the specified number of characters.</summary>
	/// <returns>The maximum number of bytes produced by encoding the specified number of characters.</returns>
	/// <param name="charCount">The number of characters to encode. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charCount" /> is less than zero. </exception>
	/// <exception cref="T:System.Text.EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public abstract int GetMaxByteCount(int charCount);

	/// <summary>When overridden in a derived class, calculates the maximum number of characters produced by decoding the specified number of bytes.</summary>
	/// <returns>The maximum number of characters produced by decoding the specified number of bytes.</returns>
	/// <param name="byteCount">The number of bytes to decode. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="byteCount" /> is less than zero. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public abstract int GetMaxCharCount(int byteCount);

	/// <summary>When overridden in a derived class, decodes all the bytes in the specified byte array into a string.</summary>
	/// <returns>A string that contains the results of decoding the specified sequence of bytes.</returns>
	/// <param name="bytes">The byte array containing the sequence of bytes to decode. </param>
	/// <exception cref="T:System.ArgumentException">The byte array contains invalid Unicode code points.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual string GetString(byte[] bytes)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes", Environment.GetResourceString("Array cannot be null."));
		}
		return GetString(bytes, 0, bytes.Length);
	}

	/// <summary>When overridden in a derived class, decodes a sequence of bytes from the specified byte array into a string.</summary>
	/// <returns>A string that contains the results of decoding the specified sequence of bytes.</returns>
	/// <param name="bytes">The byte array containing the sequence of bytes to decode. </param>
	/// <param name="index">The index of the first byte to decode. </param>
	/// <param name="count">The number of bytes to decode. </param>
	/// <exception cref="T:System.ArgumentException">The byte array contains invalid Unicode code points.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="bytes" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or- <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in <paramref name="bytes" />. </exception>
	/// <exception cref="T:System.Text.DecoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.DecoderFallback" /> is set to <see cref="T:System.Text.DecoderExceptionFallback" />.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual string GetString(byte[] bytes, int index, int count)
	{
		return new string(GetChars(bytes, index, count));
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current instance.</summary>
	/// <returns>true if <paramref name="value" /> is an instance of <see cref="T:System.Text.Encoding" /> and is equal to the current instance; otherwise, false. </returns>
	/// <param name="value">The <see cref="T:System.Object" /> to compare with the current instance. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object value)
	{
		if (value is Encoding encoding)
		{
			if (m_codePage == encoding.m_codePage && EncoderFallback.Equals(encoding.EncoderFallback))
			{
				return DecoderFallback.Equals(encoding.DecoderFallback);
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
		return m_codePage + EncoderFallback.GetHashCode() + DecoderFallback.GetHashCode();
	}

	internal virtual char[] GetBestFitUnicodeToBytesData()
	{
		return EmptyArray<char>.Value;
	}

	internal virtual char[] GetBestFitBytesToUnicodeData()
	{
		return EmptyArray<char>.Value;
	}

	internal void ThrowBytesOverflow()
	{
		throw new ArgumentException(Environment.GetResourceString("The output byte buffer is too small to contain the encoded data, encoding '{0}' fallback '{1}'.", EncodingName, EncoderFallback.GetType()), "bytes");
	}

	[SecurityCritical]
	internal void ThrowBytesOverflow(EncoderNLS encoder, bool nothingEncoded)
	{
		if ((encoder?.m_throwOnOverflow ?? true) || nothingEncoded)
		{
			if (encoder != null && encoder.InternalHasFallbackBuffer)
			{
				encoder.FallbackBuffer.InternalReset();
			}
			ThrowBytesOverflow();
		}
		encoder.ClearMustFlush();
	}

	internal void ThrowCharsOverflow()
	{
		throw new ArgumentException(Environment.GetResourceString("The output char buffer is too small to contain the decoded characters, encoding '{0}' fallback '{1}'.", EncodingName, DecoderFallback.GetType()), "chars");
	}

	[SecurityCritical]
	internal void ThrowCharsOverflow(DecoderNLS decoder, bool nothingDecoded)
	{
		if ((decoder?.m_throwOnOverflow ?? true) || nothingDecoded)
		{
			if (decoder != null && decoder.InternalHasFallbackBuffer)
			{
				decoder.FallbackBuffer.InternalReset();
			}
			ThrowCharsOverflow();
		}
		decoder.ClearMustFlush();
	}
}
