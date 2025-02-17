using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace System.Runtime.Serialization.Json;

internal class XmlJsonReader : XmlBaseReader, IXmlJsonReaderInitializer
{
	private enum JsonComplexTextMode
	{
		QuotedText,
		NumericalText,
		None
	}

	private static class CharType
	{
		public const byte FirstName = 1;

		public const byte Name = 2;

		public const byte None = 0;
	}

	private const int MaxTextChunk = 2048;

	private static byte[] charType = new byte[256]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 2, 2, 0, 2, 2,
		2, 2, 2, 2, 2, 2, 2, 2, 0, 0,
		0, 0, 0, 0, 0, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 0, 0, 0, 0, 3, 0, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 0, 0, 0, 0, 0, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3
	};

	private bool buffered;

	private byte[] charactersToSkipOnNextRead;

	private JsonComplexTextMode complexTextMode = JsonComplexTextMode.None;

	private bool expectingFirstElementInNonPrimitiveChild;

	private int maxBytesPerRead;

	private OnXmlDictionaryReaderClose onReaderClose;

	private bool readServerTypeElement;

	private int scopeDepth;

	private JsonNodeType[] scopes;

	public override bool CanCanonicalize => false;

	public override string Value
	{
		get
		{
			if (IsAttributeValue && !IsLocalName("type"))
			{
				return UnescapeJsonString(base.Value);
			}
			return base.Value;
		}
	}

	private bool IsAttributeValue
	{
		get
		{
			if (base.Node.NodeType != XmlNodeType.Attribute)
			{
				return base.Node is XmlAttributeTextNode;
			}
			return true;
		}
	}

	private bool IsReadingCollection
	{
		get
		{
			if (scopeDepth > 0)
			{
				return scopes[scopeDepth] == JsonNodeType.Collection;
			}
			return false;
		}
	}

	private bool IsReadingComplexText
	{
		get
		{
			if (!base.Node.IsAtomicValue)
			{
				return base.Node.NodeType == XmlNodeType.Text;
			}
			return false;
		}
	}

	public override void Close()
	{
		base.Close();
		OnXmlDictionaryReaderClose onXmlDictionaryReaderClose = onReaderClose;
		onReaderClose = null;
		ResetState();
		if (onXmlDictionaryReaderClose == null)
		{
			return;
		}
		try
		{
			onXmlDictionaryReaderClose(this);
		}
		catch (Exception ex)
		{
			if (Fx.IsFatal(ex))
			{
				throw;
			}
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(ex);
		}
	}

	public override void EndCanonicalization()
	{
		throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
	}

	public override string GetAttribute(int index)
	{
		return UnescapeJsonString(base.GetAttribute(index));
	}

	public override string GetAttribute(string localName, string namespaceUri)
	{
		if (localName != "type")
		{
			return UnescapeJsonString(base.GetAttribute(localName, namespaceUri));
		}
		return base.GetAttribute(localName, namespaceUri);
	}

	public override string GetAttribute(string name)
	{
		if (name != "type")
		{
			return UnescapeJsonString(base.GetAttribute(name));
		}
		return base.GetAttribute(name);
	}

	public override string GetAttribute(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		if (XmlDictionaryString.GetString(localName) != "type")
		{
			return UnescapeJsonString(base.GetAttribute(localName, namespaceUri));
		}
		return base.GetAttribute(localName, namespaceUri);
	}

	public override bool Read()
	{
		if (base.Node.CanMoveToElement)
		{
			MoveToElement();
		}
		if (base.Node.ReadState == ReadState.Closed)
		{
			return false;
		}
		if (base.Node.ExitScope)
		{
			ExitScope();
		}
		if (!buffered)
		{
			base.BufferReader.SetWindow(base.ElementNode.BufferOffset, maxBytesPerRead);
		}
		byte ch;
		if (!IsReadingComplexText)
		{
			SkipWhitespaceInBufferReader();
			if (TryGetByte(out ch) && (charactersToSkipOnNextRead[0] == ch || charactersToSkipOnNextRead[1] == ch))
			{
				base.BufferReader.SkipByte();
				charactersToSkipOnNextRead[0] = 0;
				charactersToSkipOnNextRead[1] = 0;
			}
			SkipWhitespaceInBufferReader();
			if (TryGetByte(out ch) && ch == 93 && IsReadingCollection)
			{
				base.BufferReader.SkipByte();
				SkipWhitespaceInBufferReader();
				ExitJsonScope();
			}
			if (base.BufferReader.EndOfFile)
			{
				if (scopeDepth > 0)
				{
					MoveToEndElement();
					return true;
				}
				MoveToEndOfFile();
				return false;
			}
		}
		ch = base.BufferReader.GetByte();
		if (scopeDepth == 0)
		{
			ReadNonExistentElementName(StringHandleConstStringType.Root);
		}
		else if (IsReadingComplexText)
		{
			switch (complexTextMode)
			{
			case JsonComplexTextMode.NumericalText:
				ReadNumericalText();
				break;
			case JsonComplexTextMode.QuotedText:
				if (ch == 92)
				{
					ReadEscapedCharacter(moveToText: true);
				}
				else
				{
					ReadQuotedText(moveToText: true);
				}
				break;
			case JsonComplexTextMode.None:
				XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString("Encountered an unexpected character '{0}' in JSON.", (char)ch)));
				break;
			}
		}
		else if (IsReadingCollection)
		{
			ReadNonExistentElementName(StringHandleConstStringType.Item);
		}
		else if (ch == 93)
		{
			base.BufferReader.SkipByte();
			MoveToEndElement();
			ExitJsonScope();
		}
		else if (ch == 123)
		{
			base.BufferReader.SkipByte();
			SkipWhitespaceInBufferReader();
			ch = base.BufferReader.GetByte();
			if (ch == 125)
			{
				base.BufferReader.SkipByte();
				SkipWhitespaceInBufferReader();
				if (TryGetByte(out ch))
				{
					if (ch == 44)
					{
						base.BufferReader.SkipByte();
					}
				}
				else
				{
					charactersToSkipOnNextRead[0] = 44;
				}
				MoveToEndElement();
			}
			else
			{
				EnterJsonScope(JsonNodeType.Object);
				ParseStartElement();
			}
		}
		else if (ch == 125)
		{
			base.BufferReader.SkipByte();
			if (expectingFirstElementInNonPrimitiveChild)
			{
				SkipWhitespaceInBufferReader();
				ch = base.BufferReader.GetByte();
				if (ch == 44 || ch == 125)
				{
					base.BufferReader.SkipByte();
				}
				else
				{
					XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString("Encountered an unexpected character '{0}' in JSON.", (char)ch)));
				}
				expectingFirstElementInNonPrimitiveChild = false;
			}
			MoveToEndElement();
		}
		else if (ch == 44)
		{
			base.BufferReader.SkipByte();
			MoveToEndElement();
		}
		else if (ch == 34)
		{
			if (readServerTypeElement)
			{
				readServerTypeElement = false;
				EnterJsonScope(JsonNodeType.Object);
				ParseStartElement();
			}
			else if (base.Node.NodeType == XmlNodeType.Element)
			{
				if (expectingFirstElementInNonPrimitiveChild)
				{
					EnterJsonScope(JsonNodeType.Object);
					ParseStartElement();
				}
				else
				{
					base.BufferReader.SkipByte();
					ReadQuotedText(moveToText: true);
				}
			}
			else if (base.Node.NodeType == XmlNodeType.EndElement)
			{
				EnterJsonScope(JsonNodeType.Element);
				ParseStartElement();
			}
			else
			{
				XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString("Encountered an unexpected character '{0}' in JSON.", '"')));
			}
		}
		else if (ch == 102)
		{
			int offset;
			byte[] buffer = base.BufferReader.GetBuffer(5, out offset);
			if (buffer[offset + 1] != 97 || buffer[offset + 2] != 108 || buffer[offset + 3] != 115 || buffer[offset + 4] != 101)
			{
				XmlExceptionHelper.ThrowTokenExpected(this, "false", Encoding.UTF8.GetString(buffer, offset, 5));
			}
			base.BufferReader.Advance(5);
			if (TryGetByte(out ch) && !IsWhitespace(ch) && ch != 44 && ch != 125 && ch != 93)
			{
				string @string = Encoding.UTF8.GetString(buffer, offset, 4);
				char c = (char)ch;
				XmlExceptionHelper.ThrowTokenExpected(this, "false", @string + c);
			}
			MoveToAtomicText().Value.SetValue(ValueHandleType.UTF8, offset, 5);
		}
		else if (ch == 116)
		{
			int offset2;
			byte[] buffer2 = base.BufferReader.GetBuffer(4, out offset2);
			if (buffer2[offset2 + 1] != 114 || buffer2[offset2 + 2] != 117 || buffer2[offset2 + 3] != 101)
			{
				XmlExceptionHelper.ThrowTokenExpected(this, "true", Encoding.UTF8.GetString(buffer2, offset2, 4));
			}
			base.BufferReader.Advance(4);
			if (TryGetByte(out ch) && !IsWhitespace(ch) && ch != 44 && ch != 125 && ch != 93)
			{
				string string2 = Encoding.UTF8.GetString(buffer2, offset2, 4);
				char c = (char)ch;
				XmlExceptionHelper.ThrowTokenExpected(this, "true", string2 + c);
			}
			MoveToAtomicText().Value.SetValue(ValueHandleType.UTF8, offset2, 4);
		}
		else if (ch == 110)
		{
			int offset3;
			byte[] buffer3 = base.BufferReader.GetBuffer(4, out offset3);
			if (buffer3[offset3 + 1] != 117 || buffer3[offset3 + 2] != 108 || buffer3[offset3 + 3] != 108)
			{
				XmlExceptionHelper.ThrowTokenExpected(this, "null", Encoding.UTF8.GetString(buffer3, offset3, 4));
			}
			base.BufferReader.Advance(4);
			SkipWhitespaceInBufferReader();
			if (TryGetByte(out ch))
			{
				switch (ch)
				{
				case 44:
				case 125:
					base.BufferReader.SkipByte();
					break;
				default:
				{
					string string3 = Encoding.UTF8.GetString(buffer3, offset3, 4);
					char c = (char)ch;
					XmlExceptionHelper.ThrowTokenExpected(this, "null", string3 + c);
					break;
				}
				case 93:
					break;
				}
			}
			else
			{
				charactersToSkipOnNextRead[0] = 44;
				charactersToSkipOnNextRead[1] = 125;
			}
			MoveToEndElement();
		}
		else if (ch == 45 || (48 <= ch && ch <= 57) || ch == 73 || ch == 78)
		{
			ReadNumericalText();
		}
		else
		{
			XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString("Encountered an unexpected character '{0}' in JSON.", (char)ch)));
		}
		return true;
	}

	public override decimal ReadContentAsDecimal()
	{
		string s = ReadContentAsString();
		try
		{
			return decimal.Parse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
		}
		catch (ArgumentException exception)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "decimal", exception));
		}
		catch (FormatException exception2)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "decimal", exception2));
		}
		catch (OverflowException exception3)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "decimal", exception3));
		}
	}

	public override int ReadContentAsInt()
	{
		return ParseInt(ReadContentAsString(), NumberStyles.Float);
	}

	public override long ReadContentAsLong()
	{
		string s = ReadContentAsString();
		try
		{
			return long.Parse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
		}
		catch (ArgumentException exception)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "Int64", exception));
		}
		catch (FormatException exception2)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "Int64", exception2));
		}
		catch (OverflowException exception3)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "Int64", exception3));
		}
	}

	public override int ReadValueAsBase64(byte[] buffer, int offset, int count)
	{
		if (IsAttributeValue)
		{
			if (buffer == null)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
			}
			if (offset < 0)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString("The value of this argument must be non-negative.")));
			}
			if (offset > buffer.Length)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString("The specified offset exceeds the buffer size ({0} bytes).", buffer.Length)));
			}
			if (count < 0)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString("The value of this argument must be non-negative.")));
			}
			if (count > buffer.Length - offset)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString("The specified size exceeds the remaining buffer space ({0} bytes).", buffer.Length - offset)));
			}
			return 0;
		}
		return base.ReadValueAsBase64(buffer, offset, count);
	}

	public override int ReadValueChunk(char[] chars, int offset, int count)
	{
		if (IsAttributeValue)
		{
			if (chars == null)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
			}
			if (offset < 0)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString("The value of this argument must be non-negative.")));
			}
			if (offset > chars.Length)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString("The specified offset exceeds the buffer size ({0} bytes).", chars.Length)));
			}
			if (count < 0)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString("The value of this argument must be non-negative.")));
			}
			if (count > chars.Length - offset)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString("The specified size exceeds the remaining buffer space ({0} bytes).", chars.Length - offset)));
			}
			string text = UnescapeJsonString(base.Node.ValueAsString);
			int num = Math.Min(count, text.Length);
			if (num > 0)
			{
				text.CopyTo(0, chars, offset, num);
				if (base.Node.QNameType == QNameType.Xmlns)
				{
					base.Node.Namespace.Uri.SetValue(0, 0);
				}
				else
				{
					base.Node.Value.SetValue(ValueHandleType.UTF8, 0, 0);
				}
			}
			return num;
		}
		return base.ReadValueChunk(chars, offset, count);
	}

	public void SetInput(byte[] buffer, int offset, int count, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose)
	{
		if (buffer == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
		}
		if (offset < 0)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString("The value of this argument must be non-negative.")));
		}
		if (offset > buffer.Length)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString("On JSON writer, offset exceeded buffer size {0}.", buffer.Length)));
		}
		if (count < 0)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString("The value of this argument must be non-negative.")));
		}
		if (count > buffer.Length - offset)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString("JSON size exceeded remaining buffer space, by {0} byte(s).", buffer.Length - offset)));
		}
		MoveToInitial(quotas, onClose);
		ArraySegment<byte> arraySegment = JsonEncodingStreamWrapper.ProcessBuffer(buffer, offset, count, encoding);
		base.BufferReader.SetBuffer(arraySegment.Array, arraySegment.Offset, arraySegment.Count, null, null);
		buffered = true;
		ResetState();
	}

	public void SetInput(Stream stream, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose)
	{
		if (stream == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
		}
		MoveToInitial(quotas, onClose);
		stream = new JsonEncodingStreamWrapper(stream, encoding, isReader: true);
		base.BufferReader.SetBuffer(stream, null, null);
		buffered = false;
		ResetState();
	}

	public override void StartCanonicalization(Stream stream, bool includeComments, string[] inclusivePrefixes)
	{
		throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
	}

	internal static void CheckArray(Array array, int offset, int count)
	{
		if (array == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("array"));
		}
		if (offset < 0)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString("The value of this argument must be non-negative.")));
		}
		if (offset > array.Length)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString("The specified offset exceeds the buffer size ({0} bytes).", array.Length)));
		}
		if (count < 0)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString("The value of this argument must be non-negative.")));
		}
		if (count > array.Length - offset)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString("The specified size exceeds the remaining buffer space ({0} bytes).", array.Length - offset)));
		}
	}

	protected override XmlSigningNodeWriter CreateSigningNodeWriter()
	{
		throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString("Method {0} is not supported in JSON.", "CreateSigningNodeWriter")));
	}

	private static int BreakText(byte[] buffer, int offset, int length)
	{
		if (length > 0 && (buffer[offset + length - 1] & 0x80) == 128)
		{
			int num = length;
			do
			{
				length--;
			}
			while (length > 0 && (buffer[offset + length] & 0xC0) != 192);
			if (length == 0)
			{
				return num;
			}
			byte b = (byte)(buffer[offset + length] << 2);
			int num2 = 2;
			while ((b & 0x80) == 128)
			{
				b <<= 1;
				num2++;
				if (num2 > 4)
				{
					return num;
				}
			}
			if (length + num2 == num)
			{
				return num;
			}
			if (length == 0)
			{
				return num;
			}
		}
		return length;
	}

	private static int ComputeNumericalTextLength(byte[] buffer, int offset, int offsetMax)
	{
		int num = offset;
		while (offset < offsetMax)
		{
			byte b = buffer[offset];
			if (b == 44 || b == 125 || b == 93 || IsWhitespace(b))
			{
				break;
			}
			offset++;
		}
		return offset - num;
	}

	private static int ComputeQuotedTextLengthUntilEndQuote(byte[] buffer, int offset, int offsetMax, out bool escaped)
	{
		int num = offset;
		escaped = false;
		for (; offset < offsetMax; offset++)
		{
			byte b = buffer[offset];
			if (b < 32)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString("Encountered an invalid character '{0}'.", (char)b)));
			}
			switch (b)
			{
			case 92:
			case 239:
				escaped = true;
				break;
			default:
				continue;
			case 34:
				break;
			}
			break;
		}
		return offset - num;
	}

	private static bool IsWhitespace(byte ch)
	{
		if (ch != 32 && ch != 9 && ch != 10)
		{
			return ch == 13;
		}
		return true;
	}

	private static char ParseChar(string value, NumberStyles style)
	{
		int num = ParseInt(value, style);
		try
		{
			return Convert.ToChar(num);
		}
		catch (OverflowException exception)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "char", exception));
		}
	}

	private static int ParseInt(string value, NumberStyles style)
	{
		try
		{
			return int.Parse(value, style, NumberFormatInfo.InvariantInfo);
		}
		catch (ArgumentException exception)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Int32", exception));
		}
		catch (FormatException exception2)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Int32", exception2));
		}
		catch (OverflowException exception3)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Int32", exception3));
		}
	}

	private void BufferElement()
	{
		int offset = base.BufferReader.Offset;
		bool flag = false;
		byte b = 0;
		while (!flag)
		{
			int offset2;
			int offsetMax;
			byte[] buffer = base.BufferReader.GetBuffer(128, out offset2, out offsetMax);
			if (offset2 + 128 != offsetMax)
			{
				break;
			}
			for (int i = offset2; i < offsetMax; i++)
			{
				if (flag)
				{
					break;
				}
				byte b2 = buffer[i];
				if (b2 == 92)
				{
					i++;
					if (i >= offsetMax)
					{
						break;
					}
				}
				else if (b == 0)
				{
					if (b2 == 39 || b2 == 34)
					{
						b = b2;
					}
					if (b2 == 58)
					{
						flag = true;
					}
				}
				else if (b2 == b)
				{
					b = 0;
				}
			}
			base.BufferReader.Advance(128);
		}
		base.BufferReader.Offset = offset;
	}

	private void EnterJsonScope(JsonNodeType currentNodeType)
	{
		scopeDepth++;
		if (scopes == null)
		{
			scopes = new JsonNodeType[4];
		}
		else if (scopes.Length == scopeDepth)
		{
			JsonNodeType[] destinationArray = new JsonNodeType[scopeDepth * 2];
			Array.Copy(scopes, destinationArray, scopeDepth);
			scopes = destinationArray;
		}
		scopes[scopeDepth] = currentNodeType;
	}

	private JsonNodeType ExitJsonScope()
	{
		JsonNodeType result = scopes[scopeDepth];
		scopes[scopeDepth] = JsonNodeType.None;
		scopeDepth--;
		return result;
	}

	private new void MoveToEndElement()
	{
		ExitJsonScope();
		base.MoveToEndElement();
	}

	private void MoveToInitial(XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose)
	{
		MoveToInitial(quotas);
		maxBytesPerRead = quotas.MaxBytesPerRead;
		onReaderClose = onClose;
	}

	private void ParseAndSetLocalName()
	{
		XmlElementNode xmlElementNode = EnterScope();
		xmlElementNode.NameOffset = base.BufferReader.Offset;
		do
		{
			if (base.BufferReader.GetByte() == 92)
			{
				ReadEscapedCharacter(moveToText: false);
			}
			else
			{
				ReadQuotedText(moveToText: false);
			}
		}
		while (complexTextMode == JsonComplexTextMode.QuotedText);
		int num = base.BufferReader.Offset - 1;
		xmlElementNode.LocalName.SetValue(xmlElementNode.NameOffset, num - xmlElementNode.NameOffset);
		xmlElementNode.NameLength = num - xmlElementNode.NameOffset;
		xmlElementNode.Namespace.Uri.SetValue(xmlElementNode.NameOffset, 0);
		xmlElementNode.Prefix.SetValue(PrefixHandleType.Empty);
		xmlElementNode.IsEmptyElement = false;
		xmlElementNode.ExitScope = false;
		xmlElementNode.BufferOffset = num;
		int @byte = base.BufferReader.GetByte(xmlElementNode.NameOffset);
		if ((charType[@byte] & 1) == 0)
		{
			SetJsonNameWithMapping(xmlElementNode);
			return;
		}
		int num2 = 0;
		int num3 = xmlElementNode.NameOffset;
		while (num2 < xmlElementNode.NameLength)
		{
			@byte = base.BufferReader.GetByte(num3);
			if ((charType[@byte] & 2) == 0 || @byte >= 128)
			{
				SetJsonNameWithMapping(xmlElementNode);
				break;
			}
			num2++;
			num3++;
		}
	}

	private void ParseStartElement()
	{
		if (!buffered)
		{
			BufferElement();
		}
		expectingFirstElementInNonPrimitiveChild = false;
		byte @byte = base.BufferReader.GetByte();
		if (@byte == 34)
		{
			base.BufferReader.SkipByte();
			ParseAndSetLocalName();
			SkipWhitespaceInBufferReader();
			SkipExpectedByteInBufferReader(58);
			SkipWhitespaceInBufferReader();
			if (base.BufferReader.GetByte() == 123)
			{
				base.BufferReader.SkipByte();
				expectingFirstElementInNonPrimitiveChild = true;
			}
			ReadAttributes();
		}
		else
		{
			XmlExceptionHelper.ThrowTokenExpected(this, "\"", (char)@byte);
		}
	}

	private void ReadAttributes()
	{
		XmlAttributeNode xmlAttributeNode = AddAttribute();
		xmlAttributeNode.LocalName.SetConstantValue(StringHandleConstStringType.Type);
		xmlAttributeNode.Namespace.Uri.SetValue(0, 0);
		xmlAttributeNode.Prefix.SetValue(PrefixHandleType.Empty);
		SkipWhitespaceInBufferReader();
		byte @byte = base.BufferReader.GetByte();
		switch (@byte)
		{
		case 34:
			if (!expectingFirstElementInNonPrimitiveChild)
			{
				xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.String);
				return;
			}
			xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.Object);
			ReadServerTypeAttribute(consumedObjectChar: true);
			return;
		case 110:
			xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.Null);
			return;
		case 102:
		case 116:
			xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.Boolean);
			return;
		case 123:
			xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.Object);
			ReadServerTypeAttribute(consumedObjectChar: false);
			return;
		case 125:
			if (expectingFirstElementInNonPrimitiveChild)
			{
				xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.Object);
				return;
			}
			XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString("Encountered an unexpected character '{0}' in JSON.", (char)@byte)));
			return;
		case 91:
			xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.Array);
			base.BufferReader.SkipByte();
			EnterJsonScope(JsonNodeType.Collection);
			return;
		}
		switch (@byte)
		{
		default:
			if (@byte != 78 && @byte != 73)
			{
				XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString("Encountered an unexpected character '{0}' in JSON.", (char)@byte)));
				break;
			}
			goto case 45;
		case 45:
		case 48:
		case 49:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 56:
		case 57:
			xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.Number);
			break;
		}
	}

	private void ReadEscapedCharacter(bool moveToText)
	{
		base.BufferReader.SkipByte();
		char c = (char)base.BufferReader.GetByte();
		switch (c)
		{
		case 'u':
		{
			base.BufferReader.SkipByte();
			int offset;
			byte[] buffer = base.BufferReader.GetBuffer(5, out offset);
			string @string = Encoding.UTF8.GetString(buffer, offset, 4);
			base.BufferReader.Advance(4);
			int num = ParseChar(@string, NumberStyles.HexNumber);
			if (char.IsHighSurrogate((char)num) && base.BufferReader.GetByte() == 92)
			{
				base.BufferReader.SkipByte();
				SkipExpectedByteInBufferReader(117);
				buffer = base.BufferReader.GetBuffer(5, out offset);
				@string = Encoding.UTF8.GetString(buffer, offset, 4);
				base.BufferReader.Advance(4);
				char c2 = ParseChar(@string, NumberStyles.HexNumber);
				if (!char.IsLowSurrogate(c2))
				{
					XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString("Low surrogate char '0x{0}' not valid. Low surrogate chars range from 0xDC00 to 0xDFFF.", @string)));
				}
				num = new SurrogateChar(c2, (char)num).Char;
			}
			if (buffer[offset + 4] == 34)
			{
				base.BufferReader.SkipByte();
				if (moveToText)
				{
					MoveToAtomicText().Value.SetCharValue(num);
				}
				complexTextMode = JsonComplexTextMode.None;
			}
			else
			{
				if (moveToText)
				{
					MoveToComplexText().Value.SetCharValue(num);
				}
				complexTextMode = JsonComplexTextMode.QuotedText;
			}
			return;
		}
		case 'b':
			c = '\b';
			break;
		case 'f':
			c = '\f';
			break;
		case 'n':
			c = '\n';
			break;
		case 'r':
			c = '\r';
			break;
		case 't':
			c = '\t';
			break;
		default:
			XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString("Encountered an unexpected character '{0}' in JSON.", c)));
			break;
		case '"':
		case '/':
		case '\\':
			break;
		}
		base.BufferReader.SkipByte();
		if (base.BufferReader.GetByte() == 34)
		{
			base.BufferReader.SkipByte();
			if (moveToText)
			{
				MoveToAtomicText().Value.SetCharValue(c);
			}
			complexTextMode = JsonComplexTextMode.None;
		}
		else
		{
			if (moveToText)
			{
				MoveToComplexText().Value.SetCharValue(c);
			}
			complexTextMode = JsonComplexTextMode.QuotedText;
		}
	}

	private void ReadNonExistentElementName(StringHandleConstStringType elementName)
	{
		EnterJsonScope(JsonNodeType.Object);
		XmlElementNode xmlElementNode = EnterScope();
		xmlElementNode.LocalName.SetConstantValue(elementName);
		xmlElementNode.Namespace.Uri.SetValue(xmlElementNode.NameOffset, 0);
		xmlElementNode.Prefix.SetValue(PrefixHandleType.Empty);
		xmlElementNode.BufferOffset = base.BufferReader.Offset;
		xmlElementNode.IsEmptyElement = false;
		xmlElementNode.ExitScope = false;
		ReadAttributes();
	}

	private int ReadNonFFFE()
	{
		int offset;
		byte[] buffer = base.BufferReader.GetBuffer(3, out offset);
		if (buffer[offset + 1] == 191 && (buffer[offset + 2] == 190 || buffer[offset + 2] == 191))
		{
			XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString("FFFE in JSON is invalid.")));
		}
		return 3;
	}

	private void ReadNumericalText()
	{
		int num;
		int offset;
		int offsetMax;
		if (buffered)
		{
			num = ComputeNumericalTextLength(base.BufferReader.GetBuffer(out offset, out offsetMax), offset, offsetMax);
		}
		else
		{
			byte[] buffer = base.BufferReader.GetBuffer(2048, out offset, out offsetMax);
			num = BreakText(length: ComputeNumericalTextLength(buffer, offset, offsetMax), buffer: buffer, offset: offset);
		}
		base.BufferReader.Advance(num);
		if (offset <= offsetMax - num)
		{
			MoveToAtomicText().Value.SetValue(ValueHandleType.UTF8, offset, num);
			complexTextMode = JsonComplexTextMode.None;
		}
		else
		{
			MoveToComplexText().Value.SetValue(ValueHandleType.UTF8, offset, num);
			complexTextMode = JsonComplexTextMode.NumericalText;
		}
	}

	private void ReadQuotedText(bool moveToText)
	{
		int offset;
		int offsetMax;
		bool escaped;
		bool flag;
		int num;
		if (buffered)
		{
			num = ComputeQuotedTextLengthUntilEndQuote(base.BufferReader.GetBuffer(out offset, out offsetMax), offset, offsetMax, out escaped);
			flag = offset < offsetMax - num;
		}
		else
		{
			byte[] buffer = base.BufferReader.GetBuffer(2048, out offset, out offsetMax);
			num = ComputeQuotedTextLengthUntilEndQuote(buffer, offset, offsetMax, out escaped);
			flag = offset < offsetMax - num;
			num = BreakText(buffer, offset, num);
		}
		if (escaped && base.BufferReader.GetByte() == 239)
		{
			offset = base.BufferReader.Offset;
			num = ReadNonFFFE();
		}
		base.BufferReader.Advance(num);
		if (!escaped && flag)
		{
			if (moveToText)
			{
				MoveToAtomicText().Value.SetValue(ValueHandleType.UTF8, offset, num);
			}
			SkipExpectedByteInBufferReader(34);
			complexTextMode = JsonComplexTextMode.None;
		}
		else if (num == 0 && escaped)
		{
			ReadEscapedCharacter(moveToText);
		}
		else
		{
			if (moveToText)
			{
				MoveToComplexText().Value.SetValue(ValueHandleType.UTF8, offset, num);
			}
			complexTextMode = JsonComplexTextMode.QuotedText;
		}
	}

	private void ReadServerTypeAttribute(bool consumedObjectChar)
	{
		if (!consumedObjectChar)
		{
			SkipExpectedByteInBufferReader(123);
			SkipWhitespaceInBufferReader();
			byte @byte = base.BufferReader.GetByte();
			if (@byte != 34 && @byte != 125)
			{
				XmlExceptionHelper.ThrowTokenExpected(this, "\"", (char)@byte);
			}
		}
		else
		{
			SkipWhitespaceInBufferReader();
		}
		byte[] buffer = base.BufferReader.GetBuffer(8, out var offset, out var offsetMax);
		if (offset + 8 <= offsetMax && buffer[offset] == 34 && buffer[offset + 1] == 95 && buffer[offset + 2] == 95 && buffer[offset + 3] == 116 && buffer[offset + 4] == 121 && buffer[offset + 5] == 112 && buffer[offset + 6] == 101 && buffer[offset + 7] == 34)
		{
			XmlAttributeNode xmlAttributeNode = AddAttribute();
			xmlAttributeNode.LocalName.SetValue(offset + 1, 6);
			xmlAttributeNode.Namespace.Uri.SetValue(0, 0);
			xmlAttributeNode.Prefix.SetValue(PrefixHandleType.Empty);
			base.BufferReader.Advance(8);
			if (!buffered)
			{
				BufferElement();
			}
			SkipWhitespaceInBufferReader();
			SkipExpectedByteInBufferReader(58);
			SkipWhitespaceInBufferReader();
			SkipExpectedByteInBufferReader(34);
			buffer = base.BufferReader.GetBuffer(out offset, out offsetMax);
			do
			{
				if (base.BufferReader.GetByte() == 92)
				{
					ReadEscapedCharacter(moveToText: false);
				}
				else
				{
					ReadQuotedText(moveToText: false);
				}
			}
			while (complexTextMode == JsonComplexTextMode.QuotedText);
			xmlAttributeNode.Value.SetValue(ValueHandleType.UTF8, offset, base.BufferReader.Offset - 1 - offset);
			SkipWhitespaceInBufferReader();
			if (base.BufferReader.GetByte() == 44)
			{
				base.BufferReader.SkipByte();
				readServerTypeElement = true;
			}
		}
		if (base.BufferReader.GetByte() == 125)
		{
			base.BufferReader.SkipByte();
			readServerTypeElement = false;
			expectingFirstElementInNonPrimitiveChild = false;
		}
		else
		{
			readServerTypeElement = true;
		}
	}

	private void ResetState()
	{
		complexTextMode = JsonComplexTextMode.None;
		expectingFirstElementInNonPrimitiveChild = false;
		charactersToSkipOnNextRead = new byte[2];
		scopeDepth = 0;
		if (scopes != null && scopes.Length > 25)
		{
			scopes = null;
		}
	}

	private void SetJsonNameWithMapping(XmlElementNode elementNode)
	{
		Namespace @namespace = AddNamespace();
		@namespace.Prefix.SetValue(PrefixHandleType.A);
		@namespace.Uri.SetConstantValue(StringHandleConstStringType.Item);
		AddXmlnsAttribute(@namespace);
		XmlAttributeNode xmlAttributeNode = AddAttribute();
		xmlAttributeNode.LocalName.SetConstantValue(StringHandleConstStringType.Item);
		xmlAttributeNode.Namespace.Uri.SetValue(0, 0);
		xmlAttributeNode.Prefix.SetValue(PrefixHandleType.Empty);
		xmlAttributeNode.Value.SetValue(ValueHandleType.UTF8, elementNode.NameOffset, elementNode.NameLength);
		elementNode.NameLength = 0;
		elementNode.Prefix.SetValue(PrefixHandleType.A);
		elementNode.LocalName.SetConstantValue(StringHandleConstStringType.Item);
		elementNode.Namespace = @namespace;
	}

	private void SkipExpectedByteInBufferReader(byte characterToSkip)
	{
		if (base.BufferReader.GetByte() != characterToSkip)
		{
			char c = (char)characterToSkip;
			XmlExceptionHelper.ThrowTokenExpected(this, c.ToString(), (char)base.BufferReader.GetByte());
		}
		base.BufferReader.SkipByte();
	}

	private void SkipWhitespaceInBufferReader()
	{
		byte ch;
		while (TryGetByte(out ch) && IsWhitespace(ch))
		{
			base.BufferReader.SkipByte();
		}
	}

	private bool TryGetByte(out byte ch)
	{
		int offset;
		int offsetMax;
		byte[] buffer = base.BufferReader.GetBuffer(1, out offset, out offsetMax);
		if (offset < offsetMax)
		{
			ch = buffer[offset];
			return true;
		}
		ch = 0;
		return false;
	}

	private string UnescapeJsonString(string val)
	{
		if (val == null)
		{
			return null;
		}
		StringBuilder stringBuilder = null;
		int startIndex = 0;
		int num = 0;
		for (int i = 0; i < val.Length; i++)
		{
			if (val[i] == '\\')
			{
				i++;
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder();
				}
				stringBuilder.Append(val, startIndex, num);
				if (i >= val.Length)
				{
					XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString("Encountered an unexpected character '{0}' in JSON.", val[i])));
				}
				switch (val[i])
				{
				case '"':
				case '\'':
				case '/':
				case '\\':
					stringBuilder.Append(val[i]);
					break;
				case 'b':
					stringBuilder.Append('\b');
					break;
				case 'f':
					stringBuilder.Append('\f');
					break;
				case 'n':
					stringBuilder.Append('\n');
					break;
				case 'r':
					stringBuilder.Append('\r');
					break;
				case 't':
					stringBuilder.Append('\t');
					break;
				case 'u':
					if (i + 3 >= val.Length)
					{
						XmlExceptionHelper.ThrowXmlException(this, new XmlException(SR.GetString("Encountered an unexpected character '{0}' in JSON.", val[i])));
					}
					stringBuilder.Append(ParseChar(val.Substring(i + 1, 4), NumberStyles.HexNumber));
					i += 4;
					break;
				}
				startIndex = i + 1;
				num = 0;
			}
			else
			{
				num++;
			}
		}
		if (stringBuilder == null)
		{
			return val;
		}
		if (num > 0)
		{
			stringBuilder.Append(val, startIndex, num);
		}
		return stringBuilder.ToString();
	}
}
