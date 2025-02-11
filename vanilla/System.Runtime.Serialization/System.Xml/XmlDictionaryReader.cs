using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace System.Xml;

/// <summary>An abstract class that the Windows Communication Foundation (WCF) derives from <see cref="T:System.Xml.XmlReader" /> to do serialization and deserialization.</summary>
public abstract class XmlDictionaryReader : XmlReader
{
	private class XmlWrappedReader : XmlDictionaryReader, IXmlLineInfo
	{
		private XmlReader reader;

		private XmlNamespaceManager nsMgr;

		public override int AttributeCount => reader.AttributeCount;

		public override string BaseURI => reader.BaseURI;

		public override bool CanReadBinaryContent => reader.CanReadBinaryContent;

		public override bool CanReadValueChunk => reader.CanReadValueChunk;

		public override int Depth => reader.Depth;

		public override bool EOF => reader.EOF;

		public override bool HasValue => reader.HasValue;

		public override bool IsDefault => reader.IsDefault;

		public override bool IsEmptyElement => reader.IsEmptyElement;

		public override string LocalName => reader.LocalName;

		public override string Name => reader.Name;

		public override string NamespaceURI => reader.NamespaceURI;

		public override XmlNameTable NameTable => reader.NameTable;

		public override XmlNodeType NodeType => reader.NodeType;

		public override string Prefix => reader.Prefix;

		public override char QuoteChar => reader.QuoteChar;

		public override ReadState ReadState => reader.ReadState;

		public override string this[int index] => reader[index];

		public override string this[string name] => reader[name];

		public override string this[string name, string namespaceUri] => reader[name, namespaceUri];

		public override string Value => reader.Value;

		public override string XmlLang => reader.XmlLang;

		public override XmlSpace XmlSpace => reader.XmlSpace;

		public override Type ValueType => reader.ValueType;

		public int LineNumber
		{
			get
			{
				if (!(reader is IXmlLineInfo xmlLineInfo))
				{
					return 1;
				}
				return xmlLineInfo.LineNumber;
			}
		}

		public int LinePosition
		{
			get
			{
				if (!(reader is IXmlLineInfo xmlLineInfo))
				{
					return 1;
				}
				return xmlLineInfo.LinePosition;
			}
		}

		public XmlWrappedReader(XmlReader reader, XmlNamespaceManager nsMgr)
		{
			this.reader = reader;
			this.nsMgr = nsMgr;
		}

		public override void Close()
		{
			reader.Close();
			nsMgr = null;
		}

		public override string GetAttribute(int index)
		{
			return reader.GetAttribute(index);
		}

		public override string GetAttribute(string name)
		{
			return reader.GetAttribute(name);
		}

		public override string GetAttribute(string name, string namespaceUri)
		{
			return reader.GetAttribute(name, namespaceUri);
		}

		public override bool IsStartElement(string name)
		{
			return reader.IsStartElement(name);
		}

		public override bool IsStartElement(string localName, string namespaceUri)
		{
			return reader.IsStartElement(localName, namespaceUri);
		}

		public override string LookupNamespace(string namespaceUri)
		{
			return reader.LookupNamespace(namespaceUri);
		}

		public override void MoveToAttribute(int index)
		{
			reader.MoveToAttribute(index);
		}

		public override bool MoveToAttribute(string name)
		{
			return reader.MoveToAttribute(name);
		}

		public override bool MoveToAttribute(string name, string namespaceUri)
		{
			return reader.MoveToAttribute(name, namespaceUri);
		}

		public override bool MoveToElement()
		{
			return reader.MoveToElement();
		}

		public override bool MoveToFirstAttribute()
		{
			return reader.MoveToFirstAttribute();
		}

		public override bool MoveToNextAttribute()
		{
			return reader.MoveToNextAttribute();
		}

		public override bool Read()
		{
			return reader.Read();
		}

		public override bool ReadAttributeValue()
		{
			return reader.ReadAttributeValue();
		}

		public override string ReadElementString(string name)
		{
			return reader.ReadElementString(name);
		}

		public override string ReadElementString(string localName, string namespaceUri)
		{
			return reader.ReadElementString(localName, namespaceUri);
		}

		public override string ReadInnerXml()
		{
			return reader.ReadInnerXml();
		}

		public override string ReadOuterXml()
		{
			return reader.ReadOuterXml();
		}

		public override void ReadStartElement(string name)
		{
			reader.ReadStartElement(name);
		}

		public override void ReadStartElement(string localName, string namespaceUri)
		{
			reader.ReadStartElement(localName, namespaceUri);
		}

		public override void ReadEndElement()
		{
			reader.ReadEndElement();
		}

		public override string ReadString()
		{
			return reader.ReadString();
		}

		public override void ResolveEntity()
		{
			reader.ResolveEntity();
		}

		public override int ReadElementContentAsBase64(byte[] buffer, int offset, int count)
		{
			return reader.ReadElementContentAsBase64(buffer, offset, count);
		}

		public override int ReadContentAsBase64(byte[] buffer, int offset, int count)
		{
			return reader.ReadContentAsBase64(buffer, offset, count);
		}

		public override int ReadElementContentAsBinHex(byte[] buffer, int offset, int count)
		{
			return reader.ReadElementContentAsBinHex(buffer, offset, count);
		}

		public override int ReadContentAsBinHex(byte[] buffer, int offset, int count)
		{
			return reader.ReadContentAsBinHex(buffer, offset, count);
		}

		public override int ReadValueChunk(char[] chars, int offset, int count)
		{
			return reader.ReadValueChunk(chars, offset, count);
		}

		public override bool ReadContentAsBoolean()
		{
			return reader.ReadContentAsBoolean();
		}

		public override DateTime ReadContentAsDateTime()
		{
			return reader.ReadContentAsDateTime();
		}

		public override decimal ReadContentAsDecimal()
		{
			return (decimal)reader.ReadContentAs(typeof(decimal), null);
		}

		public override double ReadContentAsDouble()
		{
			return reader.ReadContentAsDouble();
		}

		public override int ReadContentAsInt()
		{
			return reader.ReadContentAsInt();
		}

		public override long ReadContentAsLong()
		{
			return reader.ReadContentAsLong();
		}

		public override float ReadContentAsFloat()
		{
			return reader.ReadContentAsFloat();
		}

		public override string ReadContentAsString()
		{
			return reader.ReadContentAsString();
		}

		public override object ReadContentAs(Type type, IXmlNamespaceResolver namespaceResolver)
		{
			return reader.ReadContentAs(type, namespaceResolver);
		}

		public bool HasLineInfo()
		{
			if (!(reader is IXmlLineInfo xmlLineInfo))
			{
				return false;
			}
			return xmlLineInfo.HasLineInfo();
		}
	}

	internal const int MaxInitialArrayLength = 65535;

	/// <summary>This property always returns false. Its derived classes can override to return true if they support canonicalization.</summary>
	/// <returns>Returns false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool CanCanonicalize => false;

	/// <summary>Gets the quota values that apply to the current instance of this class.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> that applies to the current instance of this class. </returns>
	/// <filterpriority>2</filterpriority>
	public virtual XmlDictionaryReaderQuotas Quotas => XmlDictionaryReaderQuotas.Max;

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> from an existing <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="reader">An instance of <see cref="T:System.Xml.XmlReader" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="reader" /> is null.</exception>
	public static XmlDictionaryReader CreateDictionaryReader(XmlReader reader)
	{
		if (reader == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
		}
		XmlDictionaryReader xmlDictionaryReader = reader as XmlDictionaryReader;
		if (xmlDictionaryReader == null)
		{
			xmlDictionaryReader = new XmlWrappedReader(reader, null);
		}
		return xmlDictionaryReader;
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that can read .NET Binary XML Format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="quotas">The quotas that apply to this operation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	public static XmlDictionaryReader CreateBinaryReader(byte[] buffer, XmlDictionaryReaderQuotas quotas)
	{
		if (buffer == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
		}
		return CreateBinaryReader(buffer, 0, buffer.Length, quotas);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that can read .NET Binary XML Format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="offset">The starting position from which to read in <paramref name="buffer" />.</param>
	/// <param name="count">The number of bytes that can be read from <paramref name="buffer" />.</param>
	/// <param name="quotas">The quotas that apply to this operation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than zero or greater than the buffer length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero or greater than the buffer length minus the offset.</exception>
	public static XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count, XmlDictionaryReaderQuotas quotas)
	{
		return CreateBinaryReader(buffer, offset, count, null, quotas);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that can read .NET Binary XML Format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="offset">The starting position from which to read in <paramref name="buffer" />.</param>
	/// <param name="count">The number of bytes that can be read from <paramref name="buffer" />.</param>
	/// <param name="dictionary">
	///   <see cref="T:System.Xml.XmlDictionary" /> to use.</param>
	/// <param name="quotas">The quotas that apply to this operation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than zero or greater than the buffer length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero or greater than the buffer length minus the offset.</exception>
	public static XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas)
	{
		return CreateBinaryReader(buffer, offset, count, dictionary, quotas, null);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that can read .NET Binary XML Format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="offset">The starting position from which to read in <paramref name="buffer" />.</param>
	/// <param name="count">The number of bytes that can be read from <paramref name="buffer" />.</param>
	/// <param name="dictionary">The <see cref="T:System.Xml.XmlDictionary" /> to use.</param>
	/// <param name="quotas">The <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> to apply.</param>
	/// <param name="session">The <see cref="T:System.Xml.XmlBinaryReaderSession" /> to use.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than zero or greater than the buffer length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero or greater than the buffer length minus the offset.</exception>
	public static XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas, XmlBinaryReaderSession session)
	{
		return CreateBinaryReader(buffer, offset, count, dictionary, quotas, session, null);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that can read .NET Binary XML Format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="offset">The starting position from which to read in <paramref name="buffer" />.</param>
	/// <param name="count">The number of bytes that can be read from <paramref name="buffer" />.</param>
	/// <param name="dictionary">The <see cref="T:System.Xml.XmlDictionary" /> to use.</param>
	/// <param name="quotas">The <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> to apply.</param>
	/// <param name="session">The <see cref="T:System.Xml.XmlBinaryReaderSession" /> to use.</param>
	/// <param name="onClose">Delegate to be called when the reader is closed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than zero or greater than the buffer length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero or greater than the buffer length minus the offset.</exception>
	public static XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas, XmlBinaryReaderSession session, OnXmlDictionaryReaderClose onClose)
	{
		XmlBinaryReader xmlBinaryReader = new XmlBinaryReader();
		xmlBinaryReader.SetInput(buffer, offset, count, dictionary, quotas, session, onClose);
		return xmlBinaryReader;
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that can read .NET Binary XML Format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="stream">The stream from which to read.</param>
	/// <param name="quotas">The quotas that apply to this operation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.</exception>
	public static XmlDictionaryReader CreateBinaryReader(Stream stream, XmlDictionaryReaderQuotas quotas)
	{
		return CreateBinaryReader(stream, null, quotas);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that can read .NET Binary XML Format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="stream">The stream from which to read.</param>
	/// <param name="dictionary">
	///   <see cref="T:System.Xml.XmlDictionary" /> to use.</param>
	/// <param name="quotas">The quotas that apply to this operation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="quotas" /> is null.</exception>
	public static XmlDictionaryReader CreateBinaryReader(Stream stream, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas)
	{
		return CreateBinaryReader(stream, dictionary, quotas, null);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that can read .NET Binary XML Format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="stream">The stream from which to read.</param>
	/// <param name="dictionary">
	///   <see cref="T:System.Xml.XmlDictionary" /> to use.</param>
	/// <param name="quotas">The quotas that apply to this operation.</param>
	/// <param name="session">
	///   <see cref="T:System.Xml.XmlBinaryReaderSession" /> to use.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.</exception>
	public static XmlDictionaryReader CreateBinaryReader(Stream stream, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas, XmlBinaryReaderSession session)
	{
		return CreateBinaryReader(stream, dictionary, quotas, session, null);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that can read .NET Binary XML Format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="stream">The stream from which to read.</param>
	/// <param name="dictionary">
	///   <see cref="T:System.Xml.XmlDictionary" /> to use.</param>
	/// <param name="quotas">
	///   <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> to apply.</param>
	/// <param name="session">
	///   <see cref="T:System.Xml.XmlBinaryReaderSession" /> to use.</param>
	/// <param name="onClose">Delegate to be called when the reader is closed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.</exception>
	public static XmlDictionaryReader CreateBinaryReader(Stream stream, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas, XmlBinaryReaderSession session, OnXmlDictionaryReaderClose onClose)
	{
		XmlBinaryReader xmlBinaryReader = new XmlBinaryReader();
		xmlBinaryReader.SetInput(stream, dictionary, quotas, session, onClose);
		return xmlBinaryReader;
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="quotas">The quotas applied to the reader.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	public static XmlDictionaryReader CreateTextReader(byte[] buffer, XmlDictionaryReaderQuotas quotas)
	{
		if (buffer == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
		}
		return CreateTextReader(buffer, 0, buffer.Length, quotas);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="offset">The starting position from which to read in <paramref name="buffer" />.</param>
	/// <param name="count">The number of bytes that can be read from <paramref name="buffer" />.</param>
	/// <param name="quotas">The quotas applied to the reader.</param>
	public static XmlDictionaryReader CreateTextReader(byte[] buffer, int offset, int count, XmlDictionaryReaderQuotas quotas)
	{
		return CreateTextReader(buffer, offset, count, null, quotas, null);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="offset">The starting position from which to read in <paramref name="buffer" />.</param>
	/// <param name="count">The number of bytes that can be read from <paramref name="buffer" />.</param>
	/// <param name="encoding">The <see cref="T:System.Text.Encoding" /> object that specifies the encoding properties to apply.</param>
	/// <param name="quotas">The <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> to apply.</param>
	/// <param name="onClose">The delegate to be called when the reader is closed.</param>
	public static XmlDictionaryReader CreateTextReader(byte[] buffer, int offset, int count, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose)
	{
		XmlUTF8TextReader xmlUTF8TextReader = new XmlUTF8TextReader();
		xmlUTF8TextReader.SetInput(buffer, offset, count, encoding, quotas, onClose);
		return xmlUTF8TextReader;
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="stream">The stream from which to read.</param>
	/// <param name="quotas">The quotas applied to the reader.</param>
	public static XmlDictionaryReader CreateTextReader(Stream stream, XmlDictionaryReaderQuotas quotas)
	{
		return CreateTextReader(stream, null, quotas, null);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="stream">The stream from which to read.</param>
	/// <param name="encoding">The <see cref="T:System.Text.Encoding" /> object that specifies the encoding properties to apply.</param>
	/// <param name="quotas">The <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> to apply.</param>
	/// <param name="onClose">The delegate to be called when the reader is closed.</param>
	public static XmlDictionaryReader CreateTextReader(Stream stream, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose)
	{
		XmlUTF8TextReader xmlUTF8TextReader = new XmlUTF8TextReader();
		xmlUTF8TextReader.SetInput(stream, encoding, quotas, onClose);
		return xmlUTF8TextReader;
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that reads XML in the MTOM format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="stream">The stream from which to read.</param>
	/// <param name="encoding">The possible character encoding of the stream.</param>
	/// <param name="quotas">The quotas to apply to this reader.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="encoding" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	public static XmlDictionaryReader CreateMtomReader(Stream stream, Encoding encoding, XmlDictionaryReaderQuotas quotas)
	{
		if (encoding == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoding");
		}
		return CreateMtomReader(stream, new Encoding[1] { encoding }, quotas);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that reads XML in the MTOM format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="stream">The stream from which to read.</param>
	/// <param name="encodings">The possible character encodings of the stream.</param>
	/// <param name="quotas">The quotas to apply to this reader.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="encoding" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	public static XmlDictionaryReader CreateMtomReader(Stream stream, Encoding[] encodings, XmlDictionaryReaderQuotas quotas)
	{
		return CreateMtomReader(stream, encodings, null, quotas);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that reads XML in the MTOM format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="stream">The stream from which to read.</param>
	/// <param name="encodings">The possible character encodings of the stream.</param>
	/// <param name="contentType">The Content-Type MIME type of the message.</param>
	/// <param name="quotas">The quotas to apply to this reader.</param>
	/// <filterpriority>2</filterpriority>
	public static XmlDictionaryReader CreateMtomReader(Stream stream, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas)
	{
		return CreateMtomReader(stream, encodings, contentType, quotas, int.MaxValue, null);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that reads XML in the MTOM format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="stream">The stream from which to read.</param>
	/// <param name="encodings">The possible character encodings of the stream.</param>
	/// <param name="contentType">The Content-Type MIME type of the message.</param>
	/// <param name="quotas">The MIME type of the message.</param>
	/// <param name="maxBufferSize">The <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> to apply to the reader.</param>
	/// <param name="onClose">The delegate to be called when the reader is closed.</param>
	/// <filterpriority>2</filterpriority>
	public static XmlDictionaryReader CreateMtomReader(Stream stream, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas, int maxBufferSize, OnXmlDictionaryReaderClose onClose)
	{
		XmlMtomReader xmlMtomReader = new XmlMtomReader();
		xmlMtomReader.SetInput(stream, encodings, contentType, quotas, maxBufferSize, onClose);
		return xmlMtomReader;
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that reads XML in the MTOM format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="offset">The starting position from which to read in <paramref name="buffer" />.</param>
	/// <param name="count">The number of bytes that can be read from <paramref name="buffer" />.</param>
	/// <param name="encoding">The possible character encoding of the input.</param>
	/// <param name="quotas">The quotas to apply to this reader.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="encoding" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	public static XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, Encoding encoding, XmlDictionaryReaderQuotas quotas)
	{
		if (encoding == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoding");
		}
		return CreateMtomReader(buffer, offset, count, new Encoding[1] { encoding }, quotas);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that reads XML in the MTOM format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="offset">The starting position from which to read in <paramref name="buffer" />.</param>
	/// <param name="count">The number of bytes that can be read from <paramref name="buffer" />.</param>
	/// <param name="encodings">The possible character encodings of the input.</param>
	/// <param name="quotas">The quotas to apply to this reader.</param>
	/// <filterpriority>2</filterpriority>
	public static XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, Encoding[] encodings, XmlDictionaryReaderQuotas quotas)
	{
		return CreateMtomReader(buffer, offset, count, encodings, null, quotas);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that reads XML in the MTOM format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="offset">The starting position from which to read in <paramref name="buffer" />.</param>
	/// <param name="count">The number of bytes that can be read from <paramref name="buffer" />.</param>
	/// <param name="encodings">The possible character encodings of the input.</param>
	/// <param name="contentType">The Content-Type MIME type of the message.</param>
	/// <param name="quotas">The quotas to apply to this reader.</param>
	/// <filterpriority>2</filterpriority>
	public static XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas)
	{
		return CreateMtomReader(buffer, offset, count, encodings, contentType, quotas, int.MaxValue, null);
	}

	/// <summary>Creates an instance of <see cref="T:System.Xml.XmlDictionaryReader" /> that reads XML in the MTOM format.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReader" />.</returns>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="offset">The starting position from which to read in <paramref name="buffer" />.</param>
	/// <param name="count">The number of bytes that can be read from <paramref name="buffer" />.</param>
	/// <param name="encodings">The possible character encodings of the input.</param>
	/// <param name="contentType">The Content-Type MIME type of the message.</param>
	/// <param name="quotas">The <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> to apply to the reader.</param>
	/// <param name="maxBufferSize">The maximum allowed size of the buffer.</param>
	/// <param name="onClose">The delegate to be called when the reader is closed.</param>
	/// <filterpriority>2</filterpriority>
	public static XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas, int maxBufferSize, OnXmlDictionaryReaderClose onClose)
	{
		XmlMtomReader xmlMtomReader = new XmlMtomReader();
		xmlMtomReader.SetInput(buffer, offset, count, encodings, contentType, quotas, maxBufferSize, onClose);
		return xmlMtomReader;
	}

	/// <summary>This method is not yet implemented.</summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="includeComments">Determines whether comments are included.</param>
	/// <param name="inclusivePrefixes">The prefixes to be included.</param>
	/// <exception cref="T:System.NotSupportedException">Always.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual void StartCanonicalization(Stream stream, bool includeComments, string[] inclusivePrefixes)
	{
		throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
	}

	/// <summary>This method is not yet implemented.</summary>
	/// <exception cref="T:System.NotSupportedException">Always.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual void EndCanonicalization()
	{
		throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
	}

	/// <summary>Tests whether the current content node is a start element or an empty element.</summary>
	public virtual void MoveToStartElement()
	{
		if (!IsStartElement())
		{
			XmlExceptionHelper.ThrowStartElementExpected(this);
		}
	}

	/// <summary>Tests whether the current content node is a start element or an empty element and if the <see cref="P:System.Xml.XmlReader.Name" /> property of the element matches the given argument.</summary>
	/// <param name="name">The <see cref="P:System.Xml.XmlReader.Name" /> property of the element.</param>
	public virtual void MoveToStartElement(string name)
	{
		if (!IsStartElement(name))
		{
			XmlExceptionHelper.ThrowStartElementExpected(this, name);
		}
	}

	/// <summary>Tests whether the current content node is a start element or an empty element and if the <see cref="P:System.Xml.XmlReader.LocalName" /> and <see cref="P:System.Xml.XmlReader.NamespaceURI" /> properties of the element matches the given arguments.</summary>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual void MoveToStartElement(string localName, string namespaceUri)
	{
		if (!IsStartElement(localName, namespaceUri))
		{
			XmlExceptionHelper.ThrowStartElementExpected(this, localName, namespaceUri);
		}
	}

	/// <summary>Tests whether the current content node is a start element or an empty element and if the <see cref="P:System.Xml.XmlReader.LocalName" /> and <see cref="P:System.Xml.XmlReader.NamespaceURI" /> properties of the element matches the given argument.</summary>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual void MoveToStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		if (!IsStartElement(localName, namespaceUri))
		{
			XmlExceptionHelper.ThrowStartElementExpected(this, localName, namespaceUri);
		}
	}

	/// <summary>Checks whether the parameter, <paramref name="localName" />, is the local name of the current node.</summary>
	/// <returns>true if <paramref name="localName" /> matches local name of the current node; otherwise false.</returns>
	/// <param name="localName">The local name of the current node.</param>
	public virtual bool IsLocalName(string localName)
	{
		return LocalName == localName;
	}

	/// <summary>Checks whether the parameter, <paramref name="localName" />, is the local name of the current node.</summary>
	/// <returns>true if <paramref name="localName" /> matches local name of the current node; otherwise false.</returns>
	/// <param name="localName">An <see cref="T:System.Xml.XmlDictionaryString" /> that represents the local name of the current node.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="localName" /> is null.</exception>
	public virtual bool IsLocalName(XmlDictionaryString localName)
	{
		if (localName == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
		}
		return IsLocalName(localName.Value);
	}

	/// <summary>Checks whether the parameter, <paramref name="namespaceUri" />, is the namespace of the current node.</summary>
	/// <returns>true if <paramref name="namespaceUri" /> matches namespace of the current node; otherwise false.</returns>
	/// <param name="namespaceUri">The namespace of current node.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="namespaceUri" /> is null.</exception>
	public virtual bool IsNamespaceUri(string namespaceUri)
	{
		if (namespaceUri == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
		}
		return NamespaceURI == namespaceUri;
	}

	/// <summary>Checks whether the parameter, <paramref name="namespaceUri" />, is the namespace of the current node.</summary>
	/// <returns>true if <paramref name="namespaceUri" /> matches namespace of the current node; otherwise false.</returns>
	/// <param name="namespaceUri">Namespace of current node.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="namespaceUri" /> is null.</exception>
	public virtual bool IsNamespaceUri(XmlDictionaryString namespaceUri)
	{
		if (namespaceUri == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
		}
		return IsNamespaceUri(namespaceUri.Value);
	}

	/// <summary>Checks whether the current node is an element and advances the reader to the next node.</summary>
	/// <exception cref="T:System.Xml.XmlException">
	///   <see cref="M:System.Xml.XmlDictionaryReader.IsStartElement(System.Xml.XmlDictionaryString,System.Xml.XmlDictionaryString)" /> returns false.</exception>
	public virtual void ReadFullStartElement()
	{
		MoveToStartElement();
		if (IsEmptyElement)
		{
			XmlExceptionHelper.ThrowFullStartElementExpected(this);
		}
		Read();
	}

	/// <summary>Checks whether the current node is an element with the given <paramref name="name" /> and advances the reader to the next node.</summary>
	/// <param name="name">The qualified name of the element.</param>
	/// <exception cref="T:System.Xml.XmlException">
	///   <see cref="M:System.Xml.XmlDictionaryReader.IsStartElement(System.Xml.XmlDictionaryString,System.Xml.XmlDictionaryString)" /> returns false.</exception>
	public virtual void ReadFullStartElement(string name)
	{
		MoveToStartElement(name);
		if (IsEmptyElement)
		{
			XmlExceptionHelper.ThrowFullStartElementExpected(this, name);
		}
		Read();
	}

	/// <summary>Checks whether the current node is an element with the given <paramref name="localName" /> and <paramref name="namespaceUri" /> and advances the reader to the next node.</summary>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <exception cref="T:System.Xml.XmlException">
	///   <see cref="M:System.Xml.XmlDictionaryReader.IsStartElement(System.Xml.XmlDictionaryString,System.Xml.XmlDictionaryString)" /> returns false.</exception>
	public virtual void ReadFullStartElement(string localName, string namespaceUri)
	{
		MoveToStartElement(localName, namespaceUri);
		if (IsEmptyElement)
		{
			XmlExceptionHelper.ThrowFullStartElementExpected(this, localName, namespaceUri);
		}
		Read();
	}

	/// <summary>Checks whether the current node is an element with the given <paramref name="localName" /> and <paramref name="namespaceUri" /> and advances the reader to the next node.</summary>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <exception cref="T:System.Xml.XmlException">
	///   <see cref="M:System.Xml.XmlDictionaryReader.IsStartElement(System.Xml.XmlDictionaryString,System.Xml.XmlDictionaryString)" /> returns false.</exception>
	public virtual void ReadFullStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		MoveToStartElement(localName, namespaceUri);
		if (IsEmptyElement)
		{
			XmlExceptionHelper.ThrowFullStartElementExpected(this, localName, namespaceUri);
		}
		Read();
	}

	/// <summary>Checks whether the current node is an element with the given <paramref name="localName" /> and <paramref name="namespaceUri" /> and advances the reader to the next node.</summary>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual void ReadStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		MoveToStartElement(localName, namespaceUri);
		Read();
	}

	/// <summary>Tests whether the first tag is a start tag or empty element tag and if the local name and namespace URI match those of the current node.</summary>
	/// <returns>true if the first tag in the array is a start tag or empty element tag and matches <paramref name="localName" /> and <paramref name="namespaceUri" />; otherwise false.</returns>
	/// <param name="localName">An <see cref="T:System.Xml.XmlDictionaryString" /> that represents the local name of the attribute.</param>
	/// <param name="namespaceUri">An <see cref="T:System.Xml.XmlDictionaryString" /> that represents the namespace of the attribute.</param>
	public virtual bool IsStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		return IsStartElement(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri));
	}

	/// <summary>Gets the index of the local name of the current node within an array of names.</summary>
	/// <returns>The index of the local name of the current node within an array of names.</returns>
	/// <param name="localNames">The string array of local names to be searched.</param>
	/// <param name="namespaceUri">The namespace of current node.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="localNames" /> or any of the names in the array is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="namespaceUri" /> is null.</exception>
	public virtual int IndexOfLocalName(string[] localNames, string namespaceUri)
	{
		if (localNames == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localNames");
		}
		if (namespaceUri == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
		}
		if (NamespaceURI == namespaceUri)
		{
			string localName = LocalName;
			for (int i = 0; i < localNames.Length; i++)
			{
				string text = localNames[i];
				if (text == null)
				{
					throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "localNames[{0}]", i));
				}
				if (localName == text)
				{
					return i;
				}
			}
		}
		return -1;
	}

	/// <summary>Gets the index of the local name of the current node within an array of names.</summary>
	/// <returns>The index of the local name of the current node within an array of names.</returns>
	/// <param name="localNames">The <see cref="T:System.Xml.XmlDictionaryString" /> array of local names to be searched.</param>
	/// <param name="namespaceUri">The namespace of current node.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="localNames" /> or any of the names in the array is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="namespaceUri" /> is null.</exception>
	public virtual int IndexOfLocalName(XmlDictionaryString[] localNames, XmlDictionaryString namespaceUri)
	{
		if (localNames == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localNames");
		}
		if (namespaceUri == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
		}
		if (NamespaceURI == namespaceUri.Value)
		{
			string localName = LocalName;
			for (int i = 0; i < localNames.Length; i++)
			{
				XmlDictionaryString xmlDictionaryString = localNames[i];
				if (xmlDictionaryString == null)
				{
					throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "localNames[{0}]", i));
				}
				if (localName == xmlDictionaryString.Value)
				{
					return i;
				}
			}
		}
		return -1;
	}

	/// <summary>When overridden in a derived class, gets the value of an attribute.</summary>
	/// <returns>The value of the attribute.</returns>
	/// <param name="localName">An <see cref="T:System.Xml.XmlDictionaryString" /> that represents the local name of the attribute.</param>
	/// <param name="namespaceUri">An <see cref="T:System.Xml.XmlDictionaryString" /> that represents the namespace of the attribute.</param>
	public virtual string GetAttribute(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		return GetAttribute(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri));
	}

	/// <summary>Not implemented in this class (it always returns false). May be overridden in derived classes.</summary>
	/// <returns>false, unless overridden in a derived class.</returns>
	/// <param name="length">Returns 0, unless overridden in a derived class.</param>
	/// <filterpriority>2</filterpriority>
	public virtual bool TryGetBase64ContentLength(out int length)
	{
		length = 0;
		return false;
	}

	/// <summary>Not implemented.</summary>
	/// <returns>Not implemented.</returns>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="offset">The starting position from which to read in <paramref name="buffer" />.</param>
	/// <param name="count">The number of bytes that can be read from <paramref name="buffer" />.</param>
	/// <exception cref="T:System.NotSupportedException">Always.</exception>
	public virtual int ReadValueAsBase64(byte[] buffer, int offset, int count)
	{
		throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
	}

	/// <summary>Reads the content and returns the Base64 decoded binary bytes.</summary>
	/// <returns>A byte array that contains the Base64 decoded binary bytes.</returns>
	/// <exception cref="T:System.Xml.XmlException">The array size is greater than the MaxArrayLength quota for this reader.</exception>
	public virtual byte[] ReadContentAsBase64()
	{
		return ReadContentAsBase64(Quotas.MaxArrayLength, 65535);
	}

	internal byte[] ReadContentAsBase64(int maxByteArrayContentLength, int maxInitialCount)
	{
		if (TryGetBase64ContentLength(out var length))
		{
			if (length > maxByteArrayContentLength)
			{
				XmlExceptionHelper.ThrowMaxArrayLengthExceeded(this, maxByteArrayContentLength);
			}
			if (length <= maxInitialCount)
			{
				byte[] array = new byte[length];
				int num;
				for (int i = 0; i < length; i += num)
				{
					num = ReadContentAsBase64(array, i, length - i);
					if (num == 0)
					{
						XmlExceptionHelper.ThrowBase64DataExpected(this);
					}
				}
				return array;
			}
		}
		return ReadContentAsBytes(base64: true, maxByteArrayContentLength);
	}

	/// <summary>Converts a node's content to a string.</summary>
	/// <returns>The node content in a string representation.</returns>
	public override string ReadContentAsString()
	{
		return ReadContentAsString(Quotas.MaxStringContentLength);
	}

	/// <summary>Converts a node's content to a string.</summary>
	/// <returns>Node content in string representation.</returns>
	/// <param name="maxStringContentLength">The maximum string length.</param>
	protected string ReadContentAsString(int maxStringContentLength)
	{
		StringBuilder stringBuilder = null;
		string text = string.Empty;
		bool flag = false;
		while (true)
		{
			switch (NodeType)
			{
			case XmlNodeType.Attribute:
				text = Value;
				break;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
			{
				string value = Value;
				if (text.Length == 0)
				{
					text = value;
					break;
				}
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(text);
				}
				if (stringBuilder.Length > maxStringContentLength - value.Length)
				{
					XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, maxStringContentLength);
				}
				stringBuilder.Append(value);
				break;
			}
			case XmlNodeType.EntityReference:
				if (CanResolveEntity)
				{
					ResolveEntity();
					break;
				}
				goto default;
			default:
				flag = true;
				break;
			case XmlNodeType.ProcessingInstruction:
			case XmlNodeType.Comment:
			case XmlNodeType.EndEntity:
				break;
			}
			if (flag)
			{
				break;
			}
			if (AttributeCount != 0)
			{
				ReadAttributeValue();
			}
			else
			{
				Read();
			}
		}
		if (stringBuilder != null)
		{
			text = stringBuilder.ToString();
		}
		if (text.Length > maxStringContentLength)
		{
			XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, maxStringContentLength);
		}
		return text;
	}

	/// <summary>Reads the contents of the current node into a string.</summary>
	/// <returns>A string that contains the contents of the current node.</returns>
	/// <exception cref="T:System.InvalidOperationException">Unable to read the contents of the current node.</exception>
	/// <exception cref="T:System.Xml.XmlException">Maximum allowed string length exceeded.</exception>
	public override string ReadString()
	{
		return ReadString(Quotas.MaxStringContentLength);
	}

	/// <summary>Reads the contents of the current node into a string with a given maximum length.</summary>
	/// <returns>A string that contains the contents of the current node.</returns>
	/// <param name="maxStringContentLength">Maximum allowed string length.</param>
	/// <exception cref="T:System.InvalidOperationException">Unable to read the contents of the current node.</exception>
	/// <exception cref="T:System.Xml.XmlException">Maximum allowed string length exceeded.</exception>
	protected string ReadString(int maxStringContentLength)
	{
		if (ReadState != ReadState.Interactive)
		{
			return string.Empty;
		}
		if (NodeType != XmlNodeType.Element)
		{
			MoveToElement();
		}
		if (NodeType == XmlNodeType.Element)
		{
			if (IsEmptyElement)
			{
				return string.Empty;
			}
			if (!Read())
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("The reader cannot be advanced.")));
			}
			if (NodeType == XmlNodeType.EndElement)
			{
				return string.Empty;
			}
		}
		StringBuilder stringBuilder = null;
		string text = string.Empty;
		while (IsTextNode(NodeType))
		{
			string value = Value;
			if (text.Length == 0)
			{
				text = value;
			}
			else
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(text);
				}
				if (stringBuilder.Length > maxStringContentLength - value.Length)
				{
					XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, maxStringContentLength);
				}
				stringBuilder.Append(value);
			}
			if (!Read())
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("The reader cannot be advanced.")));
			}
		}
		if (stringBuilder != null)
		{
			text = stringBuilder.ToString();
		}
		if (text.Length > maxStringContentLength)
		{
			XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, maxStringContentLength);
		}
		return text;
	}

	/// <summary>Reads the content and returns the BinHex decoded binary bytes.</summary>
	/// <returns>A byte array that contains the BinHex decoded binary bytes.</returns>
	/// <exception cref="T:System.Xml.XmlException">The array size is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	public virtual byte[] ReadContentAsBinHex()
	{
		return ReadContentAsBinHex(Quotas.MaxArrayLength);
	}

	/// <summary>Reads the content and returns the BinHex decoded binary bytes.</summary>
	/// <returns>A byte array that contains the BinHex decoded binary bytes.</returns>
	/// <param name="maxByteArrayContentLength">The maximum array length.</param>
	/// <exception cref="T:System.Xml.XmlException">The array size is greater than <paramref name="maxByteArrayContentLength" />.</exception>
	protected byte[] ReadContentAsBinHex(int maxByteArrayContentLength)
	{
		return ReadContentAsBytes(base64: false, maxByteArrayContentLength);
	}

	private byte[] ReadContentAsBytes(bool base64, int maxByteArrayContentLength)
	{
		byte[][] array = new byte[32][];
		int num = 384;
		int num2 = 0;
		int num3 = 0;
		byte[] array2;
		while (true)
		{
			array2 = new byte[num];
			array[num2++] = array2;
			int i;
			int num4;
			for (i = 0; i < array2.Length; i += num4)
			{
				num4 = ((!base64) ? ReadContentAsBinHex(array2, i, array2.Length - i) : ReadContentAsBase64(array2, i, array2.Length - i));
				if (num4 == 0)
				{
					break;
				}
			}
			if (num3 > maxByteArrayContentLength - i)
			{
				XmlExceptionHelper.ThrowMaxArrayLengthExceeded(this, maxByteArrayContentLength);
			}
			num3 += i;
			if (i < array2.Length)
			{
				break;
			}
			num *= 2;
		}
		array2 = new byte[num3];
		int num5 = 0;
		for (int j = 0; j < num2 - 1; j++)
		{
			Buffer.BlockCopy(array[j], 0, array2, num5, array[j].Length);
			num5 += array[j].Length;
		}
		Buffer.BlockCopy(array[num2 - 1], 0, array2, num5, num3 - num5);
		return array2;
	}

	/// <summary>Tests whether the current node is a text node.</summary>
	/// <returns>true if the node type is <see cref="F:System.Xml.XmlNodetype.Text" />, <see cref="F:System.Xml.XmlNodetype.Whitespace" />, <see cref="F:System.Xml.XmlNodetype.SignificantWhitespace" />, <see cref="F:System.Xml.XmlNodetype.CDATA" />, or <see cref="F:System.Xml.XmlNodetype.Attribute" />; otherwise false.</returns>
	/// <param name="nodeType">Type of the node being tested.</param>
	protected bool IsTextNode(XmlNodeType nodeType)
	{
		if (nodeType != XmlNodeType.Text && nodeType != XmlNodeType.Whitespace && nodeType != XmlNodeType.SignificantWhitespace && nodeType != XmlNodeType.CDATA)
		{
			return nodeType == XmlNodeType.Attribute;
		}
		return true;
	}

	/// <summary>Reads the content into a char array.</summary>
	/// <returns>Number of characters read.</returns>
	/// <param name="chars">The array into which the characters are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of characters to put in the array.</param>
	public virtual int ReadContentAsChars(char[] chars, int offset, int count)
	{
		int num = 0;
		while (true)
		{
			XmlNodeType nodeType = NodeType;
			if (nodeType == XmlNodeType.Element || nodeType == XmlNodeType.EndElement)
			{
				break;
			}
			if (IsTextNode(nodeType))
			{
				num = ReadValueChunk(chars, offset, count);
				if (num > 0 || nodeType == XmlNodeType.Attribute || !Read())
				{
					break;
				}
			}
			else if (!Read())
			{
				break;
			}
		}
		return num;
	}

	/// <summary>Converts a node's content to a specified type.</summary>
	/// <returns>The concatenated text content or attribute value converted to the requested type.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> of the value to be returned.</param>
	/// <param name="namespaceResolver">An <see cref="T:System.Xml.IXmlNamespaceResolver" /> object that is used to resolve any namespace prefixes related to type conversion. For example, this can be used when converting an <see cref="T:System.Xml.XmlQualifiedName" /> object to an xs:string. This value can be a null reference.</param>
	/// <filterpriority>2</filterpriority>
	public override object ReadContentAs(Type type, IXmlNamespaceResolver namespaceResolver)
	{
		if (type == typeof(Guid[]))
		{
			string[] array = (string[])ReadContentAs(typeof(string[]), namespaceResolver);
			Guid[] array2 = new Guid[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = XmlConverter.ToGuid(array[i]);
			}
			return array2;
		}
		if (type == typeof(UniqueId[]))
		{
			string[] array3 = (string[])ReadContentAs(typeof(string[]), namespaceResolver);
			UniqueId[] array4 = new UniqueId[array3.Length];
			for (int j = 0; j < array3.Length; j++)
			{
				array4[j] = XmlConverter.ToUniqueId(array3[j]);
			}
			return array4;
		}
		return base.ReadContentAs(type, namespaceResolver);
	}

	/// <summary>Converts a node's content to a string.</summary>
	/// <returns>The node content in a string representation.</returns>
	/// <param name="strings">The array of strings to match content against.</param>
	/// <param name="index">The index of the entry in <paramref name="strings" /> that matches the content.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="strings" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">An entry in<paramref name=" strings" /> is null.</exception>
	public virtual string ReadContentAsString(string[] strings, out int index)
	{
		if (strings == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("strings");
		}
		string text = ReadContentAsString();
		index = -1;
		for (int i = 0; i < strings.Length; i++)
		{
			string text2 = strings[i];
			if (text2 == null)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "strings[{0}]", i));
			}
			if (text2 == text)
			{
				index = i;
				return text2;
			}
		}
		return text;
	}

	/// <summary>Converts a node's content to a string.</summary>
	/// <returns>The node content in a string representation.</returns>
	/// <param name="strings">The array of <see cref="T:System.Xml.XmlDictionaryString" /> objects to match content against.</param>
	/// <param name="index">The index of the entry in <paramref name="strings" /> that matches the content.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="strings" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">An entry in<paramref name=" strings" /> is null.</exception>
	public virtual string ReadContentAsString(XmlDictionaryString[] strings, out int index)
	{
		if (strings == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("strings");
		}
		string text = ReadContentAsString();
		index = -1;
		for (int i = 0; i < strings.Length; i++)
		{
			XmlDictionaryString xmlDictionaryString = strings[i];
			if (xmlDictionaryString == null)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "strings[{0}]", i));
			}
			if (xmlDictionaryString.Value == text)
			{
				index = i;
				return xmlDictionaryString.Value;
			}
		}
		return text;
	}

	/// <summary>Converts a node's content to decimal.</summary>
	/// <returns>The decimal representation of node's content.</returns>
	public override decimal ReadContentAsDecimal()
	{
		return XmlConverter.ToDecimal(ReadContentAsString());
	}

	/// <summary>Converts a node's content to float.</summary>
	/// <returns>The float representation of node's content.</returns>
	public override float ReadContentAsFloat()
	{
		return XmlConverter.ToSingle(ReadContentAsString());
	}

	/// <summary>Converts a node's content to a unique identifier.</summary>
	/// <returns>The node's content represented as a unique identifier.</returns>
	public virtual UniqueId ReadContentAsUniqueId()
	{
		return XmlConverter.ToUniqueId(ReadContentAsString());
	}

	/// <summary>Converts a node's content to guid.</summary>
	/// <returns>The guid representation of node's content.</returns>
	public virtual Guid ReadContentAsGuid()
	{
		return XmlConverter.ToGuid(ReadContentAsString());
	}

	/// <summary>Converts a node's content to <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>
	///   <see cref="T:System.TimeSpan" /> representation of node's content.</returns>
	public virtual TimeSpan ReadContentAsTimeSpan()
	{
		return XmlConverter.ToTimeSpan(ReadContentAsString());
	}

	/// <summary>Converts a node's content to a qualified name representation.</summary>
	/// <param name="localName">The <see cref="P:System.Xml.XmlReader.LocalName" /> part of the qualified name (out parameter).</param>
	/// <param name="namespaceUri">The <see cref="P:System.Xml.XmlReader.NamespaceURI" /> part of the qualified name (out parameter).</param>
	public virtual void ReadContentAsQualifiedName(out string localName, out string namespaceUri)
	{
		XmlConverter.ToQualifiedName(ReadContentAsString(), out var prefix, out localName);
		namespaceUri = LookupNamespace(prefix);
		if (namespaceUri == null)
		{
			XmlExceptionHelper.ThrowUndefinedPrefix(this, prefix);
		}
	}

	/// <summary>Converts an element's content to a <see cref="T:System.String" />.</summary>
	/// <returns>The node's content represented as a <see cref="T:System.String" />.</returns>
	public override string ReadElementContentAsString()
	{
		string result;
		if (IsStartElement() && IsEmptyElement)
		{
			Read();
			result = string.Empty;
		}
		else
		{
			ReadStartElement();
			result = ReadContentAsString();
			ReadEndElement();
		}
		return result;
	}

	/// <summary>Converts an element's content to a <see cref="T:System.Boolean" />.</summary>
	/// <returns>The node's content represented as a <see cref="T:System.Boolean" />.</returns>
	public override bool ReadElementContentAsBoolean()
	{
		bool result;
		if (IsStartElement() && IsEmptyElement)
		{
			Read();
			result = XmlConverter.ToBoolean(string.Empty);
		}
		else
		{
			ReadStartElement();
			result = ReadContentAsBoolean();
			ReadEndElement();
		}
		return result;
	}

	/// <summary>Converts an element's content to an integer (<see cref="T:System.Int32" />).</summary>
	/// <returns>The node's content represented as an integer (<see cref="T:System.Int32" />).</returns>
	public override int ReadElementContentAsInt()
	{
		int result;
		if (IsStartElement() && IsEmptyElement)
		{
			Read();
			result = XmlConverter.ToInt32(string.Empty);
		}
		else
		{
			ReadStartElement();
			result = ReadContentAsInt();
			ReadEndElement();
		}
		return result;
	}

	/// <summary>Converts an element's content to a long integer (<see cref="T:System.Int64" />).</summary>
	/// <returns>The node's content represented as a long integer (<see cref="T:System.Int64" />).</returns>
	public override long ReadElementContentAsLong()
	{
		long result;
		if (IsStartElement() && IsEmptyElement)
		{
			Read();
			result = XmlConverter.ToInt64(string.Empty);
		}
		else
		{
			ReadStartElement();
			result = ReadContentAsLong();
			ReadEndElement();
		}
		return result;
	}

	/// <summary>Converts an element's content to a floating point number (<see cref="T:System.Single" />).</summary>
	/// <returns>The node's content represented as a floating point number (<see cref="T:System.Single" />).</returns>
	public override float ReadElementContentAsFloat()
	{
		float result;
		if (IsStartElement() && IsEmptyElement)
		{
			Read();
			result = XmlConverter.ToSingle(string.Empty);
		}
		else
		{
			ReadStartElement();
			result = ReadContentAsFloat();
			ReadEndElement();
		}
		return result;
	}

	/// <summary>Converts an element's content to a <see cref="T:System.Double" />.</summary>
	/// <returns>The node's content represented as a <see cref="T:System.Double" />.</returns>
	public override double ReadElementContentAsDouble()
	{
		double result;
		if (IsStartElement() && IsEmptyElement)
		{
			Read();
			result = XmlConverter.ToDouble(string.Empty);
		}
		else
		{
			ReadStartElement();
			result = ReadContentAsDouble();
			ReadEndElement();
		}
		return result;
	}

	/// <summary>Converts an element's content to a <see cref="T:System.Decimal" />.</summary>
	/// <returns>The node's content represented as a <see cref="T:System.Decimal" />.</returns>
	public override decimal ReadElementContentAsDecimal()
	{
		decimal result;
		if (IsStartElement() && IsEmptyElement)
		{
			Read();
			result = XmlConverter.ToDecimal(string.Empty);
		}
		else
		{
			ReadStartElement();
			result = ReadContentAsDecimal();
			ReadEndElement();
		}
		return result;
	}

	/// <summary>Converts an element's content to a <see cref="T:System.DateTime" />.</summary>
	/// <returns>The node's content represented as a <see cref="T:System.DateTime" />.</returns>
	/// <exception cref="T:System.ArgumentException">The element is not in valid format.</exception>
	/// <exception cref="T:System.FormatException">The element is not in valid format.</exception>
	public override DateTime ReadElementContentAsDateTime()
	{
		DateTime result;
		if (IsStartElement() && IsEmptyElement)
		{
			Read();
			try
			{
				result = DateTime.Parse(string.Empty, NumberFormatInfo.InvariantInfo);
			}
			catch (ArgumentException exception)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "DateTime", exception));
			}
			catch (FormatException exception2)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "DateTime", exception2));
			}
		}
		else
		{
			ReadStartElement();
			result = ReadContentAsDateTime();
			ReadEndElement();
		}
		return result;
	}

	/// <summary>Converts an element's content to a unique identifier.</summary>
	/// <returns>The node's content represented as a unique identifier.</returns>
	/// <exception cref="T:System.ArgumentException">The element is not in valid format.</exception>
	/// <exception cref="T:System.FormatException">The element is not in valid format.</exception>
	public virtual UniqueId ReadElementContentAsUniqueId()
	{
		UniqueId result;
		if (IsStartElement() && IsEmptyElement)
		{
			Read();
			try
			{
				result = new UniqueId(string.Empty);
			}
			catch (ArgumentException exception)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "UniqueId", exception));
			}
			catch (FormatException exception2)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "UniqueId", exception2));
			}
		}
		else
		{
			ReadStartElement();
			result = ReadContentAsUniqueId();
			ReadEndElement();
		}
		return result;
	}

	/// <summary>Converts an element's content to a <see cref="T:System.Guid" />.</summary>
	/// <returns>The node's content represented as a <see cref="T:System.Guid" />.</returns>
	/// <exception cref="T:System.ArgumentException">The element is not in valid format.</exception>
	/// <exception cref="T:System.FormatException">The element is not in valid format.</exception>
	public virtual Guid ReadElementContentAsGuid()
	{
		Guid result;
		if (IsStartElement() && IsEmptyElement)
		{
			Read();
			try
			{
				result = Guid.Empty;
			}
			catch (ArgumentException exception)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "Guid", exception));
			}
			catch (FormatException exception2)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "Guid", exception2));
			}
			catch (OverflowException exception3)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "Guid", exception3));
			}
		}
		else
		{
			ReadStartElement();
			result = ReadContentAsGuid();
			ReadEndElement();
		}
		return result;
	}

	/// <summary>Converts an element's content to a <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>The node's content represented as a <see cref="T:System.TimeSpan" />.</returns>
	public virtual TimeSpan ReadElementContentAsTimeSpan()
	{
		TimeSpan result;
		if (IsStartElement() && IsEmptyElement)
		{
			Read();
			result = XmlConverter.ToTimeSpan(string.Empty);
		}
		else
		{
			ReadStartElement();
			result = ReadContentAsTimeSpan();
			ReadEndElement();
		}
		return result;
	}

	/// <summary>Converts a node's content to a array of Base64 bytes.</summary>
	/// <returns>The node's content represented as an array of Base64 bytes.</returns>
	public virtual byte[] ReadElementContentAsBase64()
	{
		byte[] result;
		if (IsStartElement() && IsEmptyElement)
		{
			Read();
			result = new byte[0];
		}
		else
		{
			ReadStartElement();
			result = ReadContentAsBase64();
			ReadEndElement();
		}
		return result;
	}

	/// <summary>Converts a node's content to an array of BinHex bytes.</summary>
	/// <returns>The node's content represented as an array of BinHex bytes.</returns>
	public virtual byte[] ReadElementContentAsBinHex()
	{
		byte[] result;
		if (IsStartElement() && IsEmptyElement)
		{
			Read();
			result = new byte[0];
		}
		else
		{
			ReadStartElement();
			result = ReadContentAsBinHex();
			ReadEndElement();
		}
		return result;
	}

	/// <summary>Gets non-atomized names.</summary>
	/// <param name="localName">The local name.</param>
	/// <param name="namespaceUri">The namespace for the local <paramref name="localName" />.</param>
	public virtual void GetNonAtomizedNames(out string localName, out string namespaceUri)
	{
		localName = LocalName;
		namespaceUri = NamespaceURI;
	}

	/// <summary>Not implemented in this class (it always returns false). May be overridden in derived classes.</summary>
	/// <returns>false, unless overridden in a derived class.</returns>
	/// <param name="localName">Returns null, unless overridden in a derived class. .</param>
	public virtual bool TryGetLocalNameAsDictionaryString(out XmlDictionaryString localName)
	{
		localName = null;
		return false;
	}

	/// <summary>Not implemented in this class (it always returns false). May be overridden in derived classes.</summary>
	/// <returns>false, unless overridden in a derived class.</returns>
	/// <param name="namespaceUri">Returns null, unless overridden in a derived class.</param>
	public virtual bool TryGetNamespaceUriAsDictionaryString(out XmlDictionaryString namespaceUri)
	{
		namespaceUri = null;
		return false;
	}

	/// <summary>Not implemented in this class (it always returns false). May be overridden in derived classes.</summary>
	/// <returns>false, unless overridden in a derived class.</returns>
	/// <param name="value">Returns null, unless overridden in a derived class.</param>
	public virtual bool TryGetValueAsDictionaryString(out XmlDictionaryString value)
	{
		value = null;
		return false;
	}

	private void CheckArray(Array array, int offset, int count)
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

	/// <summary>Checks whether the reader is positioned at the start of an array. This class returns false, but derived classes that have the concept of arrays might return true.</summary>
	/// <returns>true if the reader is positioned at the start of an array node; otherwise false.</returns>
	/// <param name="type">Type of the node, if a valid node; otherwise null.</param>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsStartArray(out Type type)
	{
		type = null;
		return false;
	}

	/// <summary>Not implemented in this class (it always returns false). May be overridden in derived classes.</summary>
	/// <returns>false, unless overridden in a derived class.</returns>
	/// <param name="count">Returns 0, unless overridden in a derived class.</param>
	/// <filterpriority>2</filterpriority>
	public virtual bool TryGetArrayLength(out int count)
	{
		count = 0;
		return false;
	}

	/// <summary>Reads repeated occurrences of <see cref="T:System.Boolean" /> nodes into a typed array.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> array of the <see cref="T:System.Boolean" /> nodes.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual bool[] ReadBooleanArray(string localName, string namespaceUri)
	{
		return BooleanArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads repeated occurrences of <see cref="T:System.Boolean" /> nodes into a typed array.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> array of the <see cref="T:System.Boolean" /> nodes.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual bool[] ReadBooleanArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		return BooleanArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads repeated occurrences of <see cref="T:System.Boolean" /> nodes into a typed array.</summary>
	/// <returns>The number of nodes put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The local name of the element.</param>
	/// <param name="array">The array into which the nodes are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(string localName, string namespaceUri, bool[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		int i;
		for (i = 0; i < count; i++)
		{
			if (!IsStartElement(localName, namespaceUri))
			{
				break;
			}
			array[offset + i] = ReadElementContentAsBoolean();
		}
		return i;
	}

	/// <summary>Reads repeated occurrences of <see cref="T:System.Boolean" /> nodes into a typed array.</summary>
	/// <returns>The number of nodes put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the nodes are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, bool[] array, int offset, int count)
	{
		return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Reads the contents of a series of nodes with the given <paramref name="localName" /> and <paramref name="namespaceUri" /> into an array of short integers (<see cref="T:System.Int16" />).</summary>
	/// <returns>An array of short integers (<see cref="T:System.Int16" />).</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual short[] ReadInt16Array(string localName, string namespaceUri)
	{
		return Int16ArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads the contents of a series of nodes with the given <paramref name="localName" /> and <paramref name="namespaceUri" /> into an array of short integers (<see cref="T:System.Int16" />).</summary>
	/// <returns>An array of short integers (<see cref="T:System.Int16" />).</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual short[] ReadInt16Array(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		return Int16ArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads repeated occurrences of short integers into a typed array.</summary>
	/// <returns>The number of integers put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the integers are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of integers to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(string localName, string namespaceUri, short[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		int i;
		for (i = 0; i < count; i++)
		{
			if (!IsStartElement(localName, namespaceUri))
			{
				break;
			}
			int num = ReadElementContentAsInt();
			if (num < -32768 || num > 32767)
			{
				XmlExceptionHelper.ThrowConversionOverflow(this, num.ToString(NumberFormatInfo.CurrentInfo), "Int16");
			}
			array[offset + i] = (short)num;
		}
		return i;
	}

	/// <summary>Reads repeated occurrences of short integers into a typed array.</summary>
	/// <returns>The number of integers put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the integers are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of integers to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, short[] array, int offset, int count)
	{
		return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Reads the contents of a series of nodes with the given <paramref name="localName" /> and <paramref name="namespaceUri" /> into an array of integers (<see cref="T:System.Int32" />).</summary>
	/// <returns>An array of integers (<see cref="T:System.Int32" />).</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual int[] ReadInt32Array(string localName, string namespaceUri)
	{
		return Int32ArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads the contents of a series of nodes with the given <paramref name="localName" /> and <paramref name="namespaceUri" /> into an array of integers (<see cref="T:System.Int32" />).</summary>
	/// <returns>An array of integers (<see cref="T:System.Int32" />).</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual int[] ReadInt32Array(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		return Int32ArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads repeated occurrences of integers into a typed array.</summary>
	/// <returns>The number of integers put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the integers are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of integers to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(string localName, string namespaceUri, int[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		int i;
		for (i = 0; i < count; i++)
		{
			if (!IsStartElement(localName, namespaceUri))
			{
				break;
			}
			array[offset + i] = ReadElementContentAsInt();
		}
		return i;
	}

	/// <summary>Reads repeated occurrences of integers into a typed array.</summary>
	/// <returns>The number of integers put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the integers are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of integers to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, int[] array, int offset, int count)
	{
		return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Reads the contents of a series of nodes with the given <paramref name="localName" /> and <paramref name="namespaceUri" /> into an array of long integers (<see cref="T:System.Int64" />).</summary>
	/// <returns>An array of long integers (<see cref="T:System.Int64" />).</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual long[] ReadInt64Array(string localName, string namespaceUri)
	{
		return Int64ArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads the contents of a series of nodes with the given <paramref name="localName" /> and <paramref name="namespaceUri" /> into an array of long integers (<see cref="T:System.Int64" />).</summary>
	/// <returns>An array of long integers (<see cref="T:System.Int64" />).</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual long[] ReadInt64Array(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		return Int64ArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads repeated occurrences of long integers into a typed array.</summary>
	/// <returns>The number of integers put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the integers are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of integers to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(string localName, string namespaceUri, long[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		int i;
		for (i = 0; i < count; i++)
		{
			if (!IsStartElement(localName, namespaceUri))
			{
				break;
			}
			array[offset + i] = ReadElementContentAsLong();
		}
		return i;
	}

	/// <summary>Reads repeated occurrences of long integers into a typed array.</summary>
	/// <returns>The number of integers put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the integers are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of integers to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, long[] array, int offset, int count)
	{
		return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Reads the contents of a series of nodes with the given <paramref name="localName" /> and <paramref name="namespaceUri" /> into an array of float numbers (<see cref="T:System.Single" />).</summary>
	/// <returns>An array of float numbers (<see cref="T:System.Single" />).</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual float[] ReadSingleArray(string localName, string namespaceUri)
	{
		return SingleArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads the contents of a series of nodes with the given <paramref name="localName" /> and <paramref name="namespaceUri" /> into an array of float numbers (<see cref="T:System.Single" />).</summary>
	/// <returns>An array of float numbers (<see cref="T:System.Single" />).</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual float[] ReadSingleArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		return SingleArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads repeated occurrences of float numbers into a typed array.</summary>
	/// <returns>The umber of float numbers put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the float numbers are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of float numbers to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(string localName, string namespaceUri, float[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		int i;
		for (i = 0; i < count; i++)
		{
			if (!IsStartElement(localName, namespaceUri))
			{
				break;
			}
			array[offset + i] = ReadElementContentAsFloat();
		}
		return i;
	}

	/// <summary>Reads repeated occurrences of float numbers into a typed array.</summary>
	/// <returns>The number of float numbers put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the float numbers are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of float numbers to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, float[] array, int offset, int count)
	{
		return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Converts a node's content to a <see cref="T:System.DateTime" /> array.</summary>
	/// <returns>The node's content represented as a <see cref="T:System.Double" /> array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual double[] ReadDoubleArray(string localName, string namespaceUri)
	{
		return DoubleArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Converts a node's content to a <see cref="T:System.DateTime" /> array.</summary>
	/// <returns>The node's content represented as a <see cref="T:System.Double" /> array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual double[] ReadDoubleArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		return DoubleArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads repeated occurrences of <see cref="T:System.Double" /> nodes into a typed array.</summary>
	/// <returns>The number of nodes put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the nodes are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(string localName, string namespaceUri, double[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		int i;
		for (i = 0; i < count; i++)
		{
			if (!IsStartElement(localName, namespaceUri))
			{
				break;
			}
			array[offset + i] = ReadElementContentAsDouble();
		}
		return i;
	}

	/// <summary>Reads repeated occurrences of <see cref="T:System.Double" /> nodes type into a typed array.</summary>
	/// <returns>The number of nodes put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the nodes are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, double[] array, int offset, int count)
	{
		return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Converts a node's content to a <see cref="T:System.DateTime" /> array.</summary>
	/// <returns>The node's content represented as a <see cref="T:System.Decimal" /> array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual decimal[] ReadDecimalArray(string localName, string namespaceUri)
	{
		return DecimalArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Converts a node's content to a <see cref="T:System.DateTime" /> array.</summary>
	/// <returns>The node's content represented as a <see cref="T:System.Decimal" /> array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual decimal[] ReadDecimalArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		return DecimalArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads repeated occurrences of <see cref="T:System.Decimal" /> nodes into a typed array.</summary>
	/// <returns>The number of nodes put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the nodes are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(string localName, string namespaceUri, decimal[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		int i;
		for (i = 0; i < count; i++)
		{
			if (!IsStartElement(localName, namespaceUri))
			{
				break;
			}
			array[offset + i] = ReadElementContentAsDecimal();
		}
		return i;
	}

	/// <summary>Reads repeated occurrences of <see cref="T:System.Decimal" /> nodes into a typed array.</summary>
	/// <returns>The number of nodes put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the nodes are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, decimal[] array, int offset, int count)
	{
		return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Converts a node's content to a <see cref="T:System.DateTime" /> array.</summary>
	/// <returns>The node's content represented as a <see cref="T:System.DateTime" /> array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual DateTime[] ReadDateTimeArray(string localName, string namespaceUri)
	{
		return DateTimeArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Converts a node's content to a <see cref="T:System.DateTime" /> array.</summary>
	/// <returns>The node's content represented as a <see cref="T:System.DateTime" /> array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual DateTime[] ReadDateTimeArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		return DateTimeArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads repeated occurrences of <see cref="T:System.DateTime" /> nodes into a typed array.</summary>
	/// <returns>The number of nodes put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the nodes are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(string localName, string namespaceUri, DateTime[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		int i;
		for (i = 0; i < count; i++)
		{
			if (!IsStartElement(localName, namespaceUri))
			{
				break;
			}
			array[offset + i] = ReadElementContentAsDateTime();
		}
		return i;
	}

	/// <summary>Reads repeated occurrences of <see cref="T:System.DateTime" /> nodes into a typed array.</summary>
	/// <returns>The number of nodes put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the nodes are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, DateTime[] array, int offset, int count)
	{
		return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Reads the contents of a series of nodes with the given <paramref name="localName" /> and <paramref name="namespaceUri" /> into an array of <see cref="T:System.Guid" />.</summary>
	/// <returns>An array of <see cref="T:System.Guid" />.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual Guid[] ReadGuidArray(string localName, string namespaceUri)
	{
		return GuidArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads the contents of a series of nodes with the given <paramref name="localName" /> and <paramref name="namespaceUri" /> into an array of <see cref="T:System.Guid" />.</summary>
	/// <returns>An array of <see cref="T:System.Guid" />.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual Guid[] ReadGuidArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		return GuidArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads repeated occurrences of <see cref="T:System.Guid" /> nodes into a typed array.</summary>
	/// <returns>The number of nodes put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the nodes are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(string localName, string namespaceUri, Guid[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		int i;
		for (i = 0; i < count; i++)
		{
			if (!IsStartElement(localName, namespaceUri))
			{
				break;
			}
			array[offset + i] = ReadElementContentAsGuid();
		}
		return i;
	}

	/// <summary>Reads repeated occurrences of <see cref="T:System.Guid" /> nodes into a typed array.</summary>
	/// <returns>The number of nodes put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the nodes are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, Guid[] array, int offset, int count)
	{
		return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Reads the contents of a series of nodes with the given <paramref name="localName" /> and <paramref name="namespaceUri" /> into a <see cref="T:System.TimeSpan" /> array.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual TimeSpan[] ReadTimeSpanArray(string localName, string namespaceUri)
	{
		return TimeSpanArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads the contents of a series of nodes with the given <paramref name="localName" /> and <paramref name="namespaceUri" /> into a <see cref="T:System.TimeSpan" /> array.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	public virtual TimeSpan[] ReadTimeSpanArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
	{
		return TimeSpanArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
	}

	/// <summary>Reads repeated occurrences of <see cref="T:System.TimeSpan" /> nodes into a typed array.</summary>
	/// <returns>The number of nodes put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the nodes are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(string localName, string namespaceUri, TimeSpan[] array, int offset, int count)
	{
		CheckArray(array, offset, count);
		int i;
		for (i = 0; i < count; i++)
		{
			if (!IsStartElement(localName, namespaceUri))
			{
				break;
			}
			array[offset + i] = ReadElementContentAsTimeSpan();
		}
		return i;
	}

	/// <summary>Reads repeated occurrences of <see cref="T:System.TimeSpan" /> nodes into a typed array.</summary>
	/// <returns>The number of nodes put in the array.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceUri">The namespace URI of the element.</param>
	/// <param name="array">The array into which the nodes are put.</param>
	/// <param name="offset">The starting index in the array.</param>
	/// <param name="count">The number of nodes to put in the array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is &lt; 0 or &gt; <paramref name="array" /> length.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is &lt; 0 or &gt; <paramref name="array" /> length minus <paramref name="offset" />.</exception>
	public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, TimeSpan[] array, int offset, int count)
	{
		return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
	}

	/// <summary>Creates an instance of this class.  Invoked only by its derived classes.</summary>
	protected XmlDictionaryReader()
	{
	}
}
