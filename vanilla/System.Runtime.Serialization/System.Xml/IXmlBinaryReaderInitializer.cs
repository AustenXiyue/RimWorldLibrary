using System.IO;

namespace System.Xml;

/// <summary>Provides methods for reinitializing a binary reader to read a new document.</summary>
/// <filterpriority>2</filterpriority>
public interface IXmlBinaryReaderInitializer
{
	/// <summary>Reinitializes the binary reader using the given input buffer.</summary>
	/// <param name="buffer">The buffer from which to read.</param>
	/// <param name="offset">Starting position from which to read in <paramref name="buffer" />.</param>
	/// <param name="count">Number of bytes that can be read from <paramref name="buffer" />.</param>
	/// <param name="dictionary">
	///   <see cref="T:System.Xml.XmlDictionary" /> to use.</param>
	/// <param name="quotas">
	///   <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> to apply.</param>
	/// <param name="session">
	///   <see cref="T:System.Xml.XmlBinaryReaderSession" /> to use.</param>
	/// <param name="onClose">Delegate to call when the reader is closed.</param>
	void SetInput(byte[] buffer, int offset, int count, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas, XmlBinaryReaderSession session, OnXmlDictionaryReaderClose onClose);

	/// <summary>Reinitializes the binary reader using the given input stream.</summary>
	/// <param name="stream">The stream from which to read.</param>
	/// <param name="dictionary">
	///   <see cref="T:System.Xml.XmlDictionary" /> to use.</param>
	/// <param name="quotas">
	///   <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> to apply.</param>
	/// <param name="session">
	///   <see cref="T:System.Xml.XmlBinaryReaderSession" /> to use.</param>
	/// <param name="onClose">Delegate to call when the reader is closed.</param>
	void SetInput(Stream stream, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas, XmlBinaryReaderSession session, OnXmlDictionaryReaderClose onClose);
}
