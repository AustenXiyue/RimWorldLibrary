using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Runtime.Serialization.Json;

/// <summary>Specifies the interface for initializing a JavaScript Object Notation (JSON) writer when reusing them to write to a particular output stream.</summary>
[TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public interface IXmlJsonWriterInitializer
{
	/// <summary>Initializes (or reinitializes) a JavaScript Object Notation (JSON) writer to a specified output stream with specified character encoding.</summary>
	/// <param name="stream">The output <see cref="T:System.IO.Stream" /> to which the writer writes. </param>
	/// <param name="encoding">The <see cref="T:System.Text.Encoding" /> that specifies the character encoding of the output stream.</param>
	/// <param name="ownsStream">If true, the output stream is closed by the writer when done; otherwise false.</param>
	void SetOutput(Stream stream, Encoding encoding, bool ownsStream);
}
