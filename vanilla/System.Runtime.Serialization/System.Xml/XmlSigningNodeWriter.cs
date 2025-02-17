using System.IO;
using System.Text;

namespace System.Xml;

internal class XmlSigningNodeWriter : XmlNodeWriter
{
	private XmlNodeWriter writer;

	private XmlCanonicalWriter signingWriter;

	private byte[] chars;

	private byte[] base64Chars;

	private bool text;

	public XmlNodeWriter NodeWriter
	{
		get
		{
			return writer;
		}
		set
		{
			writer = value;
		}
	}

	public XmlCanonicalWriter CanonicalWriter => signingWriter;

	public XmlSigningNodeWriter(bool text)
	{
		this.text = text;
	}

	public void SetOutput(XmlNodeWriter writer, Stream stream, bool includeComments, string[] inclusivePrefixes)
	{
		this.writer = writer;
		if (signingWriter == null)
		{
			signingWriter = new XmlCanonicalWriter();
		}
		signingWriter.SetOutput(stream, includeComments, inclusivePrefixes);
		chars = new byte[64];
		base64Chars = null;
	}

	public override void Flush()
	{
		writer.Flush();
		signingWriter.Flush();
	}

	public override void Close()
	{
		writer.Close();
		signingWriter.Close();
	}

	public override void WriteDeclaration()
	{
		writer.WriteDeclaration();
		signingWriter.WriteDeclaration();
	}

	public override void WriteComment(string text)
	{
		writer.WriteComment(text);
		signingWriter.WriteComment(text);
	}

	public override void WriteCData(string text)
	{
		writer.WriteCData(text);
		signingWriter.WriteEscapedText(text);
	}

	public override void WriteStartElement(string prefix, string localName)
	{
		writer.WriteStartElement(prefix, localName);
		signingWriter.WriteStartElement(prefix, localName);
	}

	public override void WriteStartElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
	{
		writer.WriteStartElement(prefixBuffer, prefixOffset, prefixLength, localNameBuffer, localNameOffset, localNameLength);
		signingWriter.WriteStartElement(prefixBuffer, prefixOffset, prefixLength, localNameBuffer, localNameOffset, localNameLength);
	}

	public override void WriteStartElement(string prefix, XmlDictionaryString localName)
	{
		writer.WriteStartElement(prefix, localName);
		signingWriter.WriteStartElement(prefix, localName.Value);
	}

	public override void WriteEndStartElement(bool isEmpty)
	{
		writer.WriteEndStartElement(isEmpty);
		signingWriter.WriteEndStartElement(isEmpty);
	}

	public override void WriteEndElement(string prefix, string localName)
	{
		writer.WriteEndElement(prefix, localName);
		signingWriter.WriteEndElement(prefix, localName);
	}

	public override void WriteXmlnsAttribute(string prefix, string ns)
	{
		writer.WriteXmlnsAttribute(prefix, ns);
		signingWriter.WriteXmlnsAttribute(prefix, ns);
	}

	public override void WriteXmlnsAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] nsBuffer, int nsOffset, int nsLength)
	{
		writer.WriteXmlnsAttribute(prefixBuffer, prefixOffset, prefixLength, nsBuffer, nsOffset, nsLength);
		signingWriter.WriteXmlnsAttribute(prefixBuffer, prefixOffset, prefixLength, nsBuffer, nsOffset, nsLength);
	}

	public override void WriteXmlnsAttribute(string prefix, XmlDictionaryString ns)
	{
		writer.WriteXmlnsAttribute(prefix, ns);
		signingWriter.WriteXmlnsAttribute(prefix, ns.Value);
	}

	public override void WriteStartAttribute(string prefix, string localName)
	{
		writer.WriteStartAttribute(prefix, localName);
		signingWriter.WriteStartAttribute(prefix, localName);
	}

	public override void WriteStartAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
	{
		writer.WriteStartAttribute(prefixBuffer, prefixOffset, prefixLength, localNameBuffer, localNameOffset, localNameLength);
		signingWriter.WriteStartAttribute(prefixBuffer, prefixOffset, prefixLength, localNameBuffer, localNameOffset, localNameLength);
	}

	public override void WriteStartAttribute(string prefix, XmlDictionaryString localName)
	{
		writer.WriteStartAttribute(prefix, localName);
		signingWriter.WriteStartAttribute(prefix, localName.Value);
	}

	public override void WriteEndAttribute()
	{
		writer.WriteEndAttribute();
		signingWriter.WriteEndAttribute();
	}

	public override void WriteCharEntity(int ch)
	{
		writer.WriteCharEntity(ch);
		signingWriter.WriteCharEntity(ch);
	}

	public override void WriteEscapedText(string value)
	{
		writer.WriteEscapedText(value);
		signingWriter.WriteEscapedText(value);
	}

	public override void WriteEscapedText(char[] chars, int offset, int count)
	{
		writer.WriteEscapedText(chars, offset, count);
		signingWriter.WriteEscapedText(chars, offset, count);
	}

	public override void WriteEscapedText(XmlDictionaryString value)
	{
		writer.WriteEscapedText(value);
		signingWriter.WriteEscapedText(value.Value);
	}

	public override void WriteEscapedText(byte[] chars, int offset, int count)
	{
		writer.WriteEscapedText(chars, offset, count);
		signingWriter.WriteEscapedText(chars, offset, count);
	}

	public override void WriteText(string value)
	{
		writer.WriteText(value);
		signingWriter.WriteText(value);
	}

	public override void WriteText(char[] chars, int offset, int count)
	{
		writer.WriteText(chars, offset, count);
		signingWriter.WriteText(chars, offset, count);
	}

	public override void WriteText(byte[] chars, int offset, int count)
	{
		writer.WriteText(chars, offset, count);
		signingWriter.WriteText(chars, offset, count);
	}

	public override void WriteText(XmlDictionaryString value)
	{
		writer.WriteText(value);
		signingWriter.WriteText(value.Value);
	}

	public override void WriteInt32Text(int value)
	{
		int count = XmlConverter.ToChars(value, chars, 0);
		if (text)
		{
			writer.WriteText(chars, 0, count);
		}
		else
		{
			writer.WriteInt32Text(value);
		}
		signingWriter.WriteText(chars, 0, count);
	}

	public override void WriteInt64Text(long value)
	{
		int count = XmlConverter.ToChars(value, chars, 0);
		if (text)
		{
			writer.WriteText(chars, 0, count);
		}
		else
		{
			writer.WriteInt64Text(value);
		}
		signingWriter.WriteText(chars, 0, count);
	}

	public override void WriteBoolText(bool value)
	{
		int count = XmlConverter.ToChars(value, chars, 0);
		if (text)
		{
			writer.WriteText(chars, 0, count);
		}
		else
		{
			writer.WriteBoolText(value);
		}
		signingWriter.WriteText(chars, 0, count);
	}

	public override void WriteUInt64Text(ulong value)
	{
		int count = XmlConverter.ToChars(value, chars, 0);
		if (text)
		{
			writer.WriteText(chars, 0, count);
		}
		else
		{
			writer.WriteUInt64Text(value);
		}
		signingWriter.WriteText(chars, 0, count);
	}

	public override void WriteFloatText(float value)
	{
		int count = XmlConverter.ToChars(value, chars, 0);
		if (text)
		{
			writer.WriteText(chars, 0, count);
		}
		else
		{
			writer.WriteFloatText(value);
		}
		signingWriter.WriteText(chars, 0, count);
	}

	public override void WriteDoubleText(double value)
	{
		int count = XmlConverter.ToChars(value, chars, 0);
		if (text)
		{
			writer.WriteText(chars, 0, count);
		}
		else
		{
			writer.WriteDoubleText(value);
		}
		signingWriter.WriteText(chars, 0, count);
	}

	public override void WriteDecimalText(decimal value)
	{
		int count = XmlConverter.ToChars(value, chars, 0);
		if (text)
		{
			writer.WriteText(chars, 0, count);
		}
		else
		{
			writer.WriteDecimalText(value);
		}
		signingWriter.WriteText(chars, 0, count);
	}

	public override void WriteDateTimeText(DateTime value)
	{
		int count = XmlConverter.ToChars(value, chars, 0);
		if (text)
		{
			writer.WriteText(chars, 0, count);
		}
		else
		{
			writer.WriteDateTimeText(value);
		}
		signingWriter.WriteText(chars, 0, count);
	}

	public override void WriteUniqueIdText(UniqueId value)
	{
		string value2 = XmlConverter.ToString(value);
		if (text)
		{
			writer.WriteText(value2);
		}
		else
		{
			writer.WriteUniqueIdText(value);
		}
		signingWriter.WriteText(value2);
	}

	public override void WriteTimeSpanText(TimeSpan value)
	{
		string value2 = XmlConverter.ToString(value);
		if (text)
		{
			writer.WriteText(value2);
		}
		else
		{
			writer.WriteTimeSpanText(value);
		}
		signingWriter.WriteText(value2);
	}

	public override void WriteGuidText(Guid value)
	{
		string value2 = XmlConverter.ToString(value);
		if (text)
		{
			writer.WriteText(value2);
		}
		else
		{
			writer.WriteGuidText(value);
		}
		signingWriter.WriteText(value2);
	}

	public override void WriteStartListText()
	{
		writer.WriteStartListText();
	}

	public override void WriteListSeparator()
	{
		writer.WriteListSeparator();
		signingWriter.WriteText(32);
	}

	public override void WriteEndListText()
	{
		writer.WriteEndListText();
	}

	public override void WriteBase64Text(byte[] trailBytes, int trailByteCount, byte[] buffer, int offset, int count)
	{
		if (trailByteCount > 0)
		{
			WriteBase64Text(trailBytes, 0, trailByteCount);
		}
		WriteBase64Text(buffer, offset, count);
		if (!text)
		{
			writer.WriteBase64Text(trailBytes, trailByteCount, buffer, offset, count);
		}
	}

	private void WriteBase64Text(byte[] buffer, int offset, int count)
	{
		if (base64Chars == null)
		{
			base64Chars = new byte[512];
		}
		Base64Encoding base64Encoding = XmlConverter.Base64Encoding;
		while (count >= 3)
		{
			int num = Math.Min(base64Chars.Length / 4 * 3, count - count % 3);
			int count2 = num / 3 * 4;
			base64Encoding.GetChars(buffer, offset, num, base64Chars, 0);
			signingWriter.WriteText(base64Chars, 0, count2);
			if (text)
			{
				writer.WriteText(base64Chars, 0, count2);
			}
			offset += num;
			count -= num;
		}
		if (count > 0)
		{
			base64Encoding.GetChars(buffer, offset, count, base64Chars, 0);
			signingWriter.WriteText(base64Chars, 0, 4);
			if (text)
			{
				writer.WriteText(base64Chars, 0, 4);
			}
		}
	}

	public override void WriteQualifiedName(string prefix, XmlDictionaryString localName)
	{
		writer.WriteQualifiedName(prefix, localName);
		if (prefix.Length != 0)
		{
			signingWriter.WriteText(prefix);
			signingWriter.WriteText(":");
		}
		signingWriter.WriteText(localName.Value);
	}
}
