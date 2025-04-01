using System.IO;

namespace System.Xml;

/// <summary>Contains properties and methods that when implemented by a <see cref="T:System.Xml.XmlDictionaryWriter" />, allows processing of XML fragments.</summary>
/// <filterpriority>2</filterpriority>
public interface IFragmentCapableXmlDictionaryWriter
{
	/// <summary>Gets a value that indicates whether this <see cref="T:System.Xml.XmlDictionaryWriter" /> can process XML fragments. </summary>
	/// <returns>true if this <see cref="T:System.Xml.XmlDictionaryWriter" /> can process XML fragments; otherwise, false.</returns>
	bool CanFragment { get; }

	/// <summary>Starts the processing of an XML fragment.</summary>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="generateSelfContainedTextFragment">If true, any namespaces declared outside the fragment is declared again if used inside of it; if false the namespaces are not declared again.</param>
	void StartFragment(Stream stream, bool generateSelfContainedTextFragment);

	/// <summary>Ends the processing of an XML fragment.</summary>
	void EndFragment();

	/// <summary>Writes an XML fragment to the underlying stream of the writer.</summary>
	/// <param name="buffer">The buffer to write to.</param>
	/// <param name="offset">The starting position from which to write in <paramref name="buffer" />.</param>
	/// <param name="count">The number of bytes to be written to the <paramref name="buffer" />.</param>
	void WriteFragment(byte[] buffer, int offset, int count);
}
