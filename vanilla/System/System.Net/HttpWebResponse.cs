using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Unity;

namespace System.Net;

/// <summary>Provides an HTTP-specific implementation of the <see cref="T:System.Net.WebResponse" /> class.</summary>
[Serializable]
public class HttpWebResponse : WebResponse, ISerializable, IDisposable
{
	private Uri uri;

	private WebHeaderCollection webHeaders;

	private CookieCollection cookieCollection;

	private string method;

	private Version version;

	private HttpStatusCode statusCode;

	private string statusDescription;

	private long contentLength;

	private string contentType;

	private CookieContainer cookie_container;

	private bool disposed;

	private Stream stream;

	/// <summary>Gets the character set of the response.</summary>
	/// <returns>A string that contains the character set of the response.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has been disposed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string CharacterSet
	{
		get
		{
			string text = ContentType;
			if (text == null)
			{
				return "ISO-8859-1";
			}
			string text2 = text.ToLower();
			int num = text2.IndexOf("charset=", StringComparison.Ordinal);
			if (num == -1)
			{
				return "ISO-8859-1";
			}
			num += 8;
			int num2 = text2.IndexOf(';', num);
			if (num2 != -1)
			{
				return text.Substring(num, num2 - num);
			}
			return text.Substring(num);
		}
	}

	/// <summary>Gets the method that is used to encode the body of the response.</summary>
	/// <returns>A string that describes the method that is used to encode the body of the response.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has been disposed. </exception>
	public string ContentEncoding
	{
		get
		{
			CheckDisposed();
			string text = webHeaders["Content-Encoding"];
			if (text == null)
			{
				return "";
			}
			return text;
		}
	}

	/// <summary>Gets the length of the content returned by the request.</summary>
	/// <returns>The number of bytes returned by the request. Content length does not include header information.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has been disposed. </exception>
	public override long ContentLength => contentLength;

	/// <summary>Gets the content type of the response.</summary>
	/// <returns>A string that contains the content type of the response.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has been disposed. </exception>
	public override string ContentType
	{
		get
		{
			CheckDisposed();
			if (contentType == null)
			{
				contentType = webHeaders["Content-Type"];
			}
			return contentType;
		}
	}

	/// <summary>Gets or sets the cookies that are associated with this response.</summary>
	/// <returns>A <see cref="T:System.Net.CookieCollection" /> that contains the cookies that are associated with this response.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has been disposed. </exception>
	public virtual CookieCollection Cookies
	{
		get
		{
			CheckDisposed();
			if (cookieCollection == null)
			{
				cookieCollection = new CookieCollection();
			}
			return cookieCollection;
		}
		set
		{
			CheckDisposed();
			cookieCollection = value;
		}
	}

	/// <summary>Gets the headers that are associated with this response from the server.</summary>
	/// <returns>A <see cref="T:System.Net.WebHeaderCollection" /> that contains the header information returned with the response.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has been disposed. </exception>
	public override WebHeaderCollection Headers => webHeaders;

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether both client and server were authenticated.</summary>
	/// <returns>true if mutual authentication occurred; otherwise, false.</returns>
	[System.MonoTODO]
	public override bool IsMutuallyAuthenticated
	{
		get
		{
			throw GetMustImplement();
		}
	}

	/// <summary>Gets the last date and time that the contents of the response were modified.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> that contains the date and time that the contents of the response were modified.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has been disposed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public DateTime LastModified
	{
		get
		{
			CheckDisposed();
			try
			{
				return MonoHttpDate.Parse(webHeaders["Last-Modified"]);
			}
			catch (Exception)
			{
				return DateTime.Now;
			}
		}
	}

	/// <summary>Gets the method that is used to return the response.</summary>
	/// <returns>A string that contains the HTTP method that is used to return the response.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has been disposed. </exception>
	public virtual string Method
	{
		get
		{
			CheckDisposed();
			return method;
		}
	}

	/// <summary>Gets the version of the HTTP protocol that is used in the response.</summary>
	/// <returns>A <see cref="T:System.Version" /> that contains the HTTP protocol version of the response.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has been disposed. </exception>
	public Version ProtocolVersion
	{
		get
		{
			CheckDisposed();
			return version;
		}
	}

	/// <summary>Gets the URI of the Internet resource that responded to the request.</summary>
	/// <returns>A <see cref="T:System.Uri" /> that contains the URI of the Internet resource that responded to the request.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has been disposed. </exception>
	public override Uri ResponseUri
	{
		get
		{
			CheckDisposed();
			return uri;
		}
	}

	/// <summary>Gets the name of the server that sent the response.</summary>
	/// <returns>A string that contains the name of the server that sent the response.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has been disposed. </exception>
	public string Server
	{
		get
		{
			CheckDisposed();
			return webHeaders["Server"] ?? "";
		}
	}

	/// <summary>Gets the status of the response.</summary>
	/// <returns>One of the <see cref="T:System.Net.HttpStatusCode" /> values.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has been disposed. </exception>
	public virtual HttpStatusCode StatusCode => statusCode;

	/// <summary>Gets the status description returned with the response.</summary>
	/// <returns>A string that describes the status of the response.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has been disposed. </exception>
	public virtual string StatusDescription
	{
		get
		{
			CheckDisposed();
			return statusDescription;
		}
	}

	/// <summary>Gets a value that indicates if headers are supported.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if headers are supported; otherwise, false.</returns>
	public override bool SupportsHeaders => true;

	internal HttpWebResponse(Uri uri, string method, WebConnectionData data, CookieContainer container)
	{
		this.uri = uri;
		this.method = method;
		webHeaders = data.Headers;
		version = data.Version;
		statusCode = (HttpStatusCode)data.StatusCode;
		statusDescription = data.StatusDescription;
		stream = data.stream;
		contentLength = -1L;
		try
		{
			string text = webHeaders["Content-Length"];
			if (string.IsNullOrEmpty(text) || !long.TryParse(text, out contentLength))
			{
				contentLength = -1L;
			}
		}
		catch (Exception)
		{
			contentLength = -1L;
		}
		if (container != null)
		{
			cookie_container = container;
			FillCookies();
		}
		string text2 = webHeaders["Content-Encoding"];
		if (text2 == "gzip" && (data.request.AutomaticDecompression & DecompressionMethods.GZip) != 0)
		{
			stream = new GZipStream(stream, CompressionMode.Decompress);
			webHeaders.Remove(HttpRequestHeader.ContentEncoding);
		}
		else if (text2 == "deflate" && (data.request.AutomaticDecompression & DecompressionMethods.Deflate) != 0)
		{
			stream = new DeflateStream(stream, CompressionMode.Decompress);
			webHeaders.Remove(HttpRequestHeader.ContentEncoding);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.HttpWebResponse" /> class from the specified <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> instances.</summary>
	/// <param name="serializationInfo">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that contains the information required to serialize the new <see cref="T:System.Net.HttpWebRequest" />. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains the source of the serialized stream that is associated with the new <see cref="T:System.Net.HttpWebRequest" />. </param>
	[Obsolete("Serialization is obsoleted for this type", false)]
	protected HttpWebResponse(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		uri = (Uri)serializationInfo.GetValue("uri", typeof(Uri));
		contentLength = serializationInfo.GetInt64("contentLength");
		contentType = serializationInfo.GetString("contentType");
		method = serializationInfo.GetString("method");
		statusDescription = serializationInfo.GetString("statusDescription");
		cookieCollection = (CookieCollection)serializationInfo.GetValue("cookieCollection", typeof(CookieCollection));
		version = (Version)serializationInfo.GetValue("version", typeof(Version));
		statusCode = (HttpStatusCode)serializationInfo.GetValue("statusCode", typeof(HttpStatusCode));
	}

	private static Exception GetMustImplement()
	{
		return new NotImplementedException();
	}

	/// <summary>Gets the contents of a header that was returned with the response.</summary>
	/// <returns>The contents of the specified header.</returns>
	/// <param name="headerName">The header value to return. </param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has been disposed. </exception>
	public string GetResponseHeader(string headerName)
	{
		CheckDisposed();
		string text = webHeaders[headerName];
		if (text == null)
		{
			return "";
		}
		return text;
	}

	internal void ReadAll()
	{
		if (!(stream is WebConnectionStream webConnectionStream))
		{
			return;
		}
		try
		{
			webConnectionStream.ReadAll();
		}
		catch
		{
		}
	}

	/// <summary>Gets the stream that is used to read the body of the response from the server.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> containing the body of the response.</returns>
	/// <exception cref="T:System.Net.ProtocolViolationException">There is no response stream. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has been disposed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override Stream GetResponseStream()
	{
		CheckDisposed();
		if (stream == null)
		{
			return Stream.Null;
		}
		if (string.Equals(method, "HEAD", StringComparison.OrdinalIgnoreCase))
		{
			return Stream.Null;
		}
		return stream;
	}

	/// <summary>Serializes this instance into the specified <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object.</summary>
	/// <param name="serializationInfo">The object into which this <see cref="T:System.Net.HttpWebResponse" /> will be serialized. </param>
	/// <param name="streamingContext">The destination of the serialization. </param>
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
		serializationInfo.AddValue("uri", uri);
		serializationInfo.AddValue("contentLength", contentLength);
		serializationInfo.AddValue("contentType", contentType);
		serializationInfo.AddValue("method", method);
		serializationInfo.AddValue("statusDescription", statusDescription);
		serializationInfo.AddValue("cookieCollection", cookieCollection);
		serializationInfo.AddValue("version", version);
		serializationInfo.AddValue("statusCode", statusCode);
	}

	/// <summary>Closes the response stream.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override void Close()
	{
		if (this.stream != null)
		{
			Stream stream = this.stream;
			this.stream = null;
			stream?.Close();
		}
	}

	void IDisposable.Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.HttpWebResponse" />, and optionally disposes of the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		disposed = true;
		base.Dispose(disposing: true);
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
	}

	private void FillCookies()
	{
		if (webHeaders == null)
		{
			return;
		}
		CookieCollection cookieCollection = null;
		try
		{
			string text = webHeaders.Get("Set-Cookie");
			if (text != null)
			{
				cookieCollection = cookie_container.CookieCutter(uri, "Set-Cookie", text, isThrow: false);
			}
		}
		catch
		{
		}
		try
		{
			string text = webHeaders.Get("Set-Cookie2");
			if (text != null)
			{
				CookieCollection cookieCollection2 = cookie_container.CookieCutter(uri, "Set-Cookie2", text, isThrow: false);
				if (cookieCollection != null && cookieCollection.Count != 0)
				{
					cookieCollection.Add(cookieCollection2);
				}
				else
				{
					cookieCollection = cookieCollection2;
				}
			}
		}
		catch
		{
		}
		this.cookieCollection = cookieCollection;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.HttpWebResponse" /> class.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	public HttpWebResponse()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
