using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Net;

/// <summary>Provides a file system implementation of the <see cref="T:System.Net.WebResponse" /> class.</summary>
[Serializable]
public class FileWebResponse : WebResponse, ISerializable, ICloseEx
{
	private const int DefaultFileStreamBufferSize = 8192;

	private const string DefaultFileContentType = "application/octet-stream";

	private bool m_closed;

	private long m_contentLength;

	private FileAccess m_fileAccess;

	private WebHeaderCollection m_headers;

	private Stream m_stream;

	private Uri m_uri;

	/// <summary>Gets the length of the content in the file system resource.</summary>
	/// <returns>The number of bytes returned from the file system resource.</returns>
	public override long ContentLength
	{
		get
		{
			CheckDisposed();
			return m_contentLength;
		}
	}

	/// <summary>Gets the content type of the file system resource.</summary>
	/// <returns>The value "binary/octet-stream".</returns>
	public override string ContentType
	{
		get
		{
			CheckDisposed();
			return "application/octet-stream";
		}
	}

	/// <summary>Gets a collection of header name/value pairs associated with the response.</summary>
	/// <returns>A <see cref="T:System.Net.WebHeaderCollection" /> that contains the header name/value pairs associated with the response.</returns>
	public override WebHeaderCollection Headers
	{
		get
		{
			CheckDisposed();
			return m_headers;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="P:System.Net.FileWebResponse.Headers" /> property is supported by the <see cref="T:System.Net.FileWebResponse" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the <see cref="P:System.Net.FileWebResponse.Headers" /> property is supported by the <see cref="T:System.Net.FileWebResponse" /> instance; otherwise, false.</returns>
	public override bool SupportsHeaders => true;

	/// <summary>Gets the URI of the file system resource that provided the response.</summary>
	/// <returns>A <see cref="T:System.Uri" /> that contains the URI of the file system resource that provided the response.</returns>
	public override Uri ResponseUri
	{
		get
		{
			CheckDisposed();
			return m_uri;
		}
	}

	internal FileWebResponse(FileWebRequest request, Uri uri, FileAccess access, bool asyncHint)
	{
		try
		{
			m_fileAccess = access;
			if (access == FileAccess.Write)
			{
				m_stream = Stream.Null;
			}
			else
			{
				m_stream = new FileWebStream(request, uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, asyncHint);
				m_contentLength = m_stream.Length;
			}
			m_headers = new WebHeaderCollection(WebHeaderCollectionType.FileWebResponse);
			m_headers.AddInternal("Content-Length", m_contentLength.ToString(NumberFormatInfo.InvariantInfo));
			m_headers.AddInternal("Content-Type", "application/octet-stream");
			m_uri = uri;
		}
		catch (Exception ex)
		{
			throw new WebException(ex.Message, ex, WebExceptionStatus.ConnectFailure, null);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.FileWebResponse" /> class from the specified instances of the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> classes.</summary>
	/// <param name="serializationInfo">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> instance that contains the information required to serialize the new <see cref="T:System.Net.FileWebResponse" /> instance. </param>
	/// <param name="streamingContext">An instance of the <see cref="T:System.Runtime.Serialization.StreamingContext" /> class that contains the source of the serialized stream associated with the new <see cref="T:System.Net.FileWebResponse" /> instance. </param>
	[Obsolete("Serialization is obsoleted for this type. http://go.microsoft.com/fwlink/?linkid=14202")]
	protected FileWebResponse(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
		m_headers = (WebHeaderCollection)serializationInfo.GetValue("headers", typeof(WebHeaderCollection));
		m_uri = (Uri)serializationInfo.GetValue("uri", typeof(Uri));
		m_contentLength = serializationInfo.GetInt64("contentLength");
		m_fileAccess = (FileAccess)serializationInfo.GetInt32("fileAccess");
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> instance with the data needed to serialize the <see cref="T:System.Net.FileWebResponse" />.</summary>
	/// <param name="serializationInfo">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> , which will hold the serialized data for the <see cref="T:System.Net.FileWebResponse" />. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> containing the destination of the serialized stream associated with the new <see cref="T:System.Net.FileWebResponse" />. </param>
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter, SerializationFormatter = true)]
	void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		GetObjectData(serializationInfo, streamingContext);
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.</summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that specifies the destination for this serialization.</param>
	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		serializationInfo.AddValue("headers", m_headers, typeof(WebHeaderCollection));
		serializationInfo.AddValue("uri", m_uri, typeof(Uri));
		serializationInfo.AddValue("contentLength", m_contentLength);
		serializationInfo.AddValue("fileAccess", m_fileAccess);
		base.GetObjectData(serializationInfo, streamingContext);
	}

	private void CheckDisposed()
	{
		if (m_closed)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
	}

	/// <summary>Closes the response stream.</summary>
	public override void Close()
	{
		((ICloseEx)this).CloseEx(CloseExState.Normal);
	}

	void ICloseEx.CloseEx(CloseExState closeState)
	{
		try
		{
			if (m_closed)
			{
				return;
			}
			m_closed = true;
			Stream stream = m_stream;
			if (stream != null)
			{
				if (stream is ICloseEx)
				{
					((ICloseEx)stream).CloseEx(closeState);
				}
				else
				{
					stream.Close();
				}
				m_stream = null;
			}
		}
		finally
		{
		}
	}

	/// <summary>Returns the data stream from the file system resource.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> for reading data from the file system resource.</returns>
	public override Stream GetResponseStream()
	{
		try
		{
			CheckDisposed();
		}
		finally
		{
		}
		return m_stream;
	}
}
