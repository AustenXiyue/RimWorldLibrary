using System.IO;

namespace System.Xml;

/// <summary>Represents an interface that can be implemented by classes providing streams.</summary>
/// <filterpriority>2</filterpriority>
public interface IStreamProvider
{
	/// <summary>Gets a stream.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> object.</returns>
	Stream GetStream();

	/// <summary>Releases a stream to output.</summary>
	/// <param name="stream">The stream being released.</param>
	void ReleaseStream(Stream stream);
}
