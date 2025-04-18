using System.Globalization;
using System.IO;
using System.Text;
using Unity;

namespace System.Net;

/// <summary>Represents a response to a request being handled by an <see cref="T:System.Net.HttpListener" /> object.</summary>
public sealed class HttpListenerResponse : IDisposable
{
	private bool disposed;

	private Encoding content_encoding;

	private long content_length;

	private bool cl_set;

	private string content_type;

	private CookieCollection cookies;

	private WebHeaderCollection headers;

	private bool keep_alive;

	private ResponseStream output_stream;

	private Version version;

	private string location;

	private int status_code;

	private string status_description;

	private bool chunked;

	private HttpListenerContext context;

	internal bool HeadersSent;

	internal object headers_lock;

	private bool force_close_chunked;

	private static string tspecials = "()<>@,;:\\\"/[]?={} \t";

	internal bool ForceCloseChunked => force_close_chunked;

	/// <summary>Gets or sets the <see cref="T:System.Text.Encoding" /> for this response's <see cref="P:System.Net.HttpListenerResponse.OutputStream" />.</summary>
	/// <returns>An <see cref="T:System.Text.Encoding" /> object suitable for use with the data in the <see cref="P:System.Net.HttpListenerResponse.OutputStream" /> property, or null if no encoding is specified.</returns>
	public Encoding ContentEncoding
	{
		get
		{
			if (content_encoding == null)
			{
				content_encoding = Encoding.Default;
			}
			return content_encoding;
		}
		set
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (HeadersSent)
			{
				throw new InvalidOperationException("Cannot be changed after headers are sent.");
			}
			content_encoding = value;
		}
	}

	/// <summary>Gets or sets the number of bytes in the body data included in the response.</summary>
	/// <returns>The value of the response's Content-Length header.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for a set operation is less than zero.</exception>
	/// <exception cref="T:System.InvalidOperationException">The response is already being sent.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object is closed.</exception>
	public long ContentLength64
	{
		get
		{
			return content_length;
		}
		set
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (HeadersSent)
			{
				throw new InvalidOperationException("Cannot be changed after headers are sent.");
			}
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("Must be >= 0", "value");
			}
			cl_set = true;
			content_length = value;
		}
	}

	/// <summary>Gets or sets the MIME type of the content returned.</summary>
	/// <returns>A <see cref="T:System.String" /> instance that contains the text of the response's Content-Type header.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value specified for a set operation is null.</exception>
	/// <exception cref="T:System.ArgumentException">The value specified for a set operation is an empty string ("").</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object is closed.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public string ContentType
	{
		get
		{
			return content_type;
		}
		set
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (HeadersSent)
			{
				throw new InvalidOperationException("Cannot be changed after headers are sent.");
			}
			content_type = value;
		}
	}

	/// <summary>Gets or sets the collection of cookies returned with the response.</summary>
	/// <returns>A <see cref="T:System.Net.CookieCollection" /> that contains cookies to accompany the response. The collection is empty if no cookies have been added to the response.</returns>
	public CookieCollection Cookies
	{
		get
		{
			if (cookies == null)
			{
				cookies = new CookieCollection();
			}
			return cookies;
		}
		set
		{
			cookies = value;
		}
	}

	/// <summary>Gets or sets the collection of header name/value pairs returned by the server.</summary>
	/// <returns>A <see cref="T:System.Net.WebHeaderCollection" /> instance that contains all the explicitly set HTTP headers to be included in the response.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.WebHeaderCollection" /> instance specified for a set operation is not valid for a response.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public WebHeaderCollection Headers
	{
		get
		{
			return headers;
		}
		set
		{
			headers = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the server requests a persistent connection.</summary>
	/// <returns>true if the server requests a persistent connection; otherwise, false. The default is true.</returns>
	/// <exception cref="T:System.ObjectDisposedException">This object is closed.</exception>
	public bool KeepAlive
	{
		get
		{
			return keep_alive;
		}
		set
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (HeadersSent)
			{
				throw new InvalidOperationException("Cannot be changed after headers are sent.");
			}
			keep_alive = value;
		}
	}

	/// <summary>Gets a <see cref="T:System.IO.Stream" /> object to which a response can be written.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> object to which a response can be written.</returns>
	/// <exception cref="T:System.ObjectDisposedException">This object is closed.</exception>
	public Stream OutputStream
	{
		get
		{
			if (output_stream == null)
			{
				output_stream = context.Connection.GetResponseStream();
			}
			return output_stream;
		}
	}

	/// <summary>Gets or sets the HTTP version used for the response.</summary>
	/// <returns>A <see cref="T:System.Version" /> object indicating the version of HTTP used when responding to the client. Note that this property is now obsolete.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value specified for a set operation is null.</exception>
	/// <exception cref="T:System.ArgumentException">The value specified for a set operation does not have its <see cref="P:System.Version.Major" /> property set to 1 or does not have its <see cref="P:System.Version.Minor" /> property set to either 0 or 1.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object is closed.</exception>
	public Version ProtocolVersion
	{
		get
		{
			return version;
		}
		set
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (HeadersSent)
			{
				throw new InvalidOperationException("Cannot be changed after headers are sent.");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Major != 1 || (value.Minor != 0 && value.Minor != 1))
			{
				throw new ArgumentException("Must be 1.0 or 1.1", "value");
			}
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			version = value;
		}
	}

	/// <summary>Gets or sets the value of the HTTP Location header in this response.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the absolute URL to be sent to the client in the Location header. </returns>
	/// <exception cref="T:System.ArgumentException">The value specified for a set operation is an empty string ("").</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object is closed.</exception>
	public string RedirectLocation
	{
		get
		{
			return location;
		}
		set
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (HeadersSent)
			{
				throw new InvalidOperationException("Cannot be changed after headers are sent.");
			}
			location = value;
		}
	}

	/// <summary>Gets or sets whether the response uses chunked transfer encoding.</summary>
	/// <returns>true if the response is set to use chunked transfer encoding; otherwise, false. The default is false.</returns>
	public bool SendChunked
	{
		get
		{
			return chunked;
		}
		set
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (HeadersSent)
			{
				throw new InvalidOperationException("Cannot be changed after headers are sent.");
			}
			chunked = value;
		}
	}

	/// <summary>Gets or sets the HTTP status code to be returned to the client.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that specifies the HTTP status code for the requested resource. The default is <see cref="F:System.Net.HttpStatusCode.OK" />, indicating that the server successfully processed the client's request and included the requested resource in the response body.</returns>
	/// <exception cref="T:System.ObjectDisposedException">This object is closed.</exception>
	/// <exception cref="T:System.Net.ProtocolViolationException">The value specified for a set operation is not valid. Valid values are between 100 and 999 inclusive.</exception>
	public int StatusCode
	{
		get
		{
			return status_code;
		}
		set
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			if (HeadersSent)
			{
				throw new InvalidOperationException("Cannot be changed after headers are sent.");
			}
			if (value < 100 || value > 999)
			{
				throw new ProtocolViolationException("StatusCode must be between 100 and 999.");
			}
			status_code = value;
			status_description = HttpStatusDescription.Get(value);
		}
	}

	/// <summary>Gets or sets a text description of the HTTP status code returned to the client.</summary>
	/// <returns>The text description of the HTTP status code returned to the client. The default is the RFC 2616 description for the <see cref="P:System.Net.HttpListenerResponse.StatusCode" /> property value, or an empty string ("") if an RFC 2616 description does not exist.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value specified for a set operation is null.</exception>
	/// <exception cref="T:System.ArgumentException">The value specified for a set operation contains non-printable characters.</exception>
	public string StatusDescription
	{
		get
		{
			return status_description;
		}
		set
		{
			status_description = value;
		}
	}

	internal HttpListenerResponse(HttpListenerContext context)
	{
		headers = new WebHeaderCollection();
		keep_alive = true;
		version = HttpVersion.Version11;
		status_code = 200;
		status_description = "OK";
		headers_lock = new object();
		base._002Ector();
		this.context = context;
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Net.HttpListenerResponse" />.</summary>
	void IDisposable.Dispose()
	{
		Close(force: true);
	}

	/// <summary>Closes the connection to the client without sending a response.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Abort()
	{
		if (!disposed)
		{
			Close(force: true);
		}
	}

	/// <summary>Adds the specified header and value to the HTTP headers for this response.</summary>
	/// <param name="name">The name of the HTTP header to set.</param>
	/// <param name="value">The value for the <paramref name="name" /> header.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null or an empty string ("").</exception>
	/// <exception cref="T:System.ArgumentException">You are not allowed to specify a value for the specified header.-or-<paramref name="name" /> or <paramref name="value" /> contains invalid characters.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of <paramref name="value" /> is greater than 65,535 characters.</exception>
	public void AddHeader(string name, string value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == "")
		{
			throw new ArgumentException("'name' cannot be empty", "name");
		}
		if (value.Length > 65535)
		{
			throw new ArgumentOutOfRangeException("value");
		}
		headers.Set(name, value);
	}

	/// <summary>Adds the specified <see cref="T:System.Net.Cookie" /> to the collection of cookies for this response.</summary>
	/// <param name="cookie">The <see cref="T:System.Net.Cookie" /> to add to the collection to be sent with this response</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="cookie" /> is null.</exception>
	public void AppendCookie(Cookie cookie)
	{
		if (cookie == null)
		{
			throw new ArgumentNullException("cookie");
		}
		Cookies.Add(cookie);
	}

	/// <summary>Appends a value to the specified HTTP header to be sent with this response.</summary>
	/// <param name="name">The name of the HTTP header to append <paramref name="value" /> to.</param>
	/// <param name="value">The value to append to the <paramref name="name" /> header.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is null or an empty string ("").-or-You are not allowed to specify a value for the specified header.-or-<paramref name="name" /> or <paramref name="value" /> contains invalid characters.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of <paramref name="value" /> is greater than 65,535 characters.</exception>
	public void AppendHeader(string name, string value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name == "")
		{
			throw new ArgumentException("'name' cannot be empty", "name");
		}
		if (value.Length > 65535)
		{
			throw new ArgumentOutOfRangeException("value");
		}
		headers.Add(name, value);
	}

	private void Close(bool force)
	{
		disposed = true;
		context.Connection.Close(force);
	}

	/// <summary>Sends the response to the client and releases the resources held by this <see cref="T:System.Net.HttpListenerResponse" /> instance.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Close()
	{
		if (!disposed)
		{
			Close(force: false);
		}
	}

	/// <summary>Returns the specified byte array to the client and releases the resources held by this <see cref="T:System.Net.HttpListenerResponse" /> instance.</summary>
	/// <param name="responseEntity">A <see cref="T:System.Byte" /> array that contains the response to send to the client.</param>
	/// <param name="willBlock">true to block execution while flushing the stream to the client; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="responseEntity" /> is null.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object is closed.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Close(byte[] responseEntity, bool willBlock)
	{
		if (!disposed)
		{
			if (responseEntity == null)
			{
				throw new ArgumentNullException("responseEntity");
			}
			ContentLength64 = responseEntity.Length;
			OutputStream.Write(responseEntity, 0, (int)content_length);
			Close(force: false);
		}
	}

	/// <summary>Copies properties from the specified <see cref="T:System.Net.HttpListenerResponse" /> to this response.</summary>
	/// <param name="templateResponse">The <see cref="T:System.Net.HttpListenerResponse" /> instance to copy.</param>
	public void CopyFrom(HttpListenerResponse templateResponse)
	{
		headers.Clear();
		headers.Add(templateResponse.headers);
		content_length = templateResponse.content_length;
		status_code = templateResponse.status_code;
		status_description = templateResponse.status_description;
		keep_alive = templateResponse.keep_alive;
		version = templateResponse.version;
	}

	/// <summary>Configures the response to redirect the client to the specified URL.</summary>
	/// <param name="url">The URL that the client should use to locate the requested resource.</param>
	public void Redirect(string url)
	{
		StatusCode = 302;
		location = url;
	}

	private bool FindCookie(Cookie cookie)
	{
		string name = cookie.Name;
		string domain = cookie.Domain;
		string path = cookie.Path;
		foreach (Cookie cookie2 in cookies)
		{
			if (!(name != cookie2.Name) && !(domain != cookie2.Domain) && path == cookie2.Path)
			{
				return true;
			}
		}
		return false;
	}

	internal void SendHeaders(bool closing, MemoryStream ms)
	{
		Encoding @default = content_encoding;
		if (@default == null)
		{
			@default = Encoding.Default;
		}
		if (content_type != null)
		{
			if (content_encoding != null && content_type.IndexOf("charset=", StringComparison.Ordinal) == -1)
			{
				string webName = content_encoding.WebName;
				headers.SetInternal("Content-Type", content_type + "; charset=" + webName);
			}
			else
			{
				headers.SetInternal("Content-Type", content_type);
			}
		}
		if (headers["Server"] == null)
		{
			headers.SetInternal("Server", "Mono-HTTPAPI/1.0");
		}
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		if (headers["Date"] == null)
		{
			headers.SetInternal("Date", DateTime.UtcNow.ToString("r", invariantCulture));
		}
		if (!chunked)
		{
			if (!cl_set && closing)
			{
				cl_set = true;
				content_length = 0L;
			}
			if (cl_set)
			{
				headers.SetInternal("Content-Length", content_length.ToString(invariantCulture));
			}
		}
		Version protocolVersion = context.Request.ProtocolVersion;
		if (!cl_set && !chunked && protocolVersion >= HttpVersion.Version11)
		{
			chunked = true;
		}
		bool flag = status_code == 400 || status_code == 408 || status_code == 411 || status_code == 413 || status_code == 414 || status_code == 500 || status_code == 503;
		if (!flag)
		{
			flag = !context.Request.KeepAlive;
		}
		if (!keep_alive || flag)
		{
			headers.SetInternal("Connection", "close");
			flag = true;
		}
		if (chunked)
		{
			headers.SetInternal("Transfer-Encoding", "chunked");
		}
		int reuses = context.Connection.Reuses;
		if (reuses >= 100)
		{
			force_close_chunked = true;
			if (!flag)
			{
				headers.SetInternal("Connection", "close");
				flag = true;
			}
		}
		if (!flag)
		{
			headers.SetInternal("Keep-Alive", $"timeout=15,max={100 - reuses}");
			if (context.Request.ProtocolVersion <= HttpVersion.Version10)
			{
				headers.SetInternal("Connection", "keep-alive");
			}
		}
		if (location != null)
		{
			headers.SetInternal("Location", location);
		}
		if (cookies != null)
		{
			foreach (Cookie cookie in cookies)
			{
				headers.SetInternal("Set-Cookie", CookieToClientString(cookie));
			}
		}
		StreamWriter streamWriter = new StreamWriter(ms, @default, 256);
		streamWriter.Write("HTTP/{0} {1} {2}\r\n", version, status_code, status_description);
		string value = FormatHeaders(headers);
		streamWriter.Write(value);
		streamWriter.Flush();
		int num = @default.GetPreamble().Length;
		if (output_stream == null)
		{
			output_stream = context.Connection.GetResponseStream();
		}
		ms.Position = num;
		HeadersSent = true;
	}

	private static string FormatHeaders(WebHeaderCollection headers)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < headers.Count; i++)
		{
			string key = headers.GetKey(i);
			if (WebHeaderCollection.AllowMultiValues(key))
			{
				string[] values = headers.GetValues(i);
				foreach (string value in values)
				{
					stringBuilder.Append(key).Append(": ").Append(value)
						.Append("\r\n");
				}
			}
			else
			{
				stringBuilder.Append(key).Append(": ").Append(headers.Get(i))
					.Append("\r\n");
			}
		}
		return stringBuilder.Append("\r\n").ToString();
	}

	private static string CookieToClientString(Cookie cookie)
	{
		if (cookie.Name.Length == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(64);
		if (cookie.Version > 0)
		{
			stringBuilder.Append("Version=").Append(cookie.Version).Append(";");
		}
		stringBuilder.Append(cookie.Name).Append("=").Append(cookie.Value);
		if (cookie.Path != null && cookie.Path.Length != 0)
		{
			stringBuilder.Append(";Path=").Append(QuotedString(cookie, cookie.Path));
		}
		if (cookie.Domain != null && cookie.Domain.Length != 0)
		{
			stringBuilder.Append(";Domain=").Append(QuotedString(cookie, cookie.Domain));
		}
		if (cookie.Port != null && cookie.Port.Length != 0)
		{
			stringBuilder.Append(";Port=").Append(cookie.Port);
		}
		return stringBuilder.ToString();
	}

	private static string QuotedString(Cookie cookie, string value)
	{
		if (cookie.Version == 0 || IsToken(value))
		{
			return value;
		}
		return "\"" + value.Replace("\"", "\\\"") + "\"";
	}

	private static bool IsToken(string value)
	{
		int length = value.Length;
		for (int i = 0; i < length; i++)
		{
			char c = value[i];
			if (c < ' ' || c >= '\u007f' || tspecials.IndexOf(c) != -1)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Adds or updates a <see cref="T:System.Net.Cookie" /> in the collection of cookies sent with this response. </summary>
	/// <param name="cookie">A <see cref="T:System.Net.Cookie" /> for this response.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="cookie" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The cookie already exists in the collection and could not be replaced.</exception>
	public void SetCookie(Cookie cookie)
	{
		if (cookie == null)
		{
			throw new ArgumentNullException("cookie");
		}
		if (cookies != null)
		{
			if (FindCookie(cookie))
			{
				throw new ArgumentException("The cookie already exists.");
			}
		}
		else
		{
			cookies = new CookieCollection();
		}
		cookies.Add(cookie);
	}

	internal HttpListenerResponse()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
