using System.IO;
using System.Text;

namespace System.Xml;

/// <summary>Specifies implementation requirements for XML text readers that derive from this interface.</summary>
/// <filterpriority>2</filterpriority>
public interface IXmlTextReaderInitializer
{
	/// <summary>Specifies initialization requirements for XML text readers that read a buffer.</summary>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="offset">The starting position from which to read in <paramref name="buffer" />.</param>
	/// <param name="count">The number of bytes that can be read from <paramref name="buffer" />.</param>
	/// <param name="encoding">The character encoding of the stream.</param>
	/// <param name="quotas">The <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> to apply.</param>
	/// <param name="onClose">The delegate to be called when the reader is closed.</param>
	void SetInput(byte[] buffer, int offset, int count, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose);

	/// <summary>Specifies initialization requirements for XML text readers that read a stream.</summary>
	/// <param name="stream">The stream from which to read.</param>
	/// <param name="encoding">The character encoding of the stream.</param>
	/// <param name="quotas">The <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> to apply.</param>
	/// <param name="onClose">The delegate to be called when the reader is closed.</param>
	void SetInput(Stream stream, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose);
}
