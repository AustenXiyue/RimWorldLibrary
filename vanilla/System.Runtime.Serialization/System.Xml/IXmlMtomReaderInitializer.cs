using System.IO;
using System.Text;

namespace System.Xml;

/// <summary>Specifies implementation requirements for XML MTOM readers that derive from this interface.</summary>
/// <filterpriority>2</filterpriority>
public interface IXmlMtomReaderInitializer
{
	/// <summary>Specifies initialization requirements for XML MTOM readers that read a buffer.</summary>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="offset">The starting position from which to read in <paramref name="buffer" />.</param>
	/// <param name="count">The number of bytes that can be read from <paramref name="buffer" />.</param>
	/// <param name="encodings">The possible character encodings of the input.</param>
	/// <param name="contentType">The Content-Type of the message. Can be null if the MIME type is present in the document being read.</param>
	/// <param name="quotas">The <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> to apply to the reader.</param>
	/// <param name="maxBufferSize">The maximum allowed size of the buffer.</param>
	/// <param name="onClose">The delegate to use when an onClose event happens.</param>
	void SetInput(byte[] buffer, int offset, int count, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas, int maxBufferSize, OnXmlDictionaryReaderClose onClose);

	/// <summary>Specifies initialization requirements for XML MTOM readers that read a stream.</summary>
	/// <param name="stream">The stream from which to read.</param>
	/// <param name="encodings">The possible character encodings of the stream.</param>
	/// <param name="contentType">The Content-Type of the message. Can be null if the MIME type is present in the document being read.</param>
	/// <param name="quotas">The <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> to apply to the reader.</param>
	/// <param name="maxBufferSize">The maximum allowed size of the buffer.</param>
	/// <param name="onClose">The delegate to use when an onClose event happens.</param>
	void SetInput(Stream stream, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas, int maxBufferSize, OnXmlDictionaryReaderClose onClose);
}
