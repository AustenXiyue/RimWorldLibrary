using System.IO;

namespace System.Windows.Resources;

/// <summary>Stores information for a stream resource used in Windows Presentation Foundation (WPF), such as images.</summary>
public class StreamResourceInfo
{
	private string _contentType;

	private Stream _stream;

	/// <summary> Gets or sets the content type of a stream. </summary>
	/// <returns>The Multipurpose Internet Mail Extensions (MIME) content type.</returns>
	public string ContentType => _contentType;

	/// <summary> Gets or sets the actual stream of the resource. </summary>
	/// <returns>The stream for the resource.</returns>
	public Stream Stream => _stream;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Resources.StreamResourceInfo" /> class.</summary>
	public StreamResourceInfo()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Resources.StreamResourceInfo" /> class based on a provided stream.</summary>
	/// <param name="stream">The reference stream.</param>
	/// <param name="contentType">The Multipurpose Internet Mail Extensions (MIME)  content type of the stream.</param>
	public StreamResourceInfo(Stream stream, string contentType)
	{
		_stream = stream;
		_contentType = contentType;
	}
}
