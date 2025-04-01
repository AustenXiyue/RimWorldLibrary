using System.IO;
using System.Text;

namespace System.Xml;

/// <summary>When implemented by an MTOM writer, this interface ensures initialization for an MTOM writer.</summary>
/// <filterpriority>2</filterpriority>
public interface IXmlMtomWriterInitializer
{
	/// <summary>When implemented by an MTOM writer, initializes an MTOM writer.</summary>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="encoding">The character encoding of the stream.</param>
	/// <param name="maxSizeInBytes">The maximum number of bytes that are buffered in the writer.</param>
	/// <param name="startInfo">An attribute in the ContentType SOAP header, set to "Application/soap+xml".</param>
	/// <param name="boundary">The MIME boundary string.</param>
	/// <param name="startUri">The URI for MIME section.</param>
	/// <param name="writeMessageHeaders">If true, write message headers.</param>
	/// <param name="ownsStream">If true, the stream is closed by the writer when done; otherwise false.</param>
	void SetOutput(Stream stream, Encoding encoding, int maxSizeInBytes, string startInfo, string boundary, string startUri, bool writeMessageHeaders, bool ownsStream);
}
