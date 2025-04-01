using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace System.Net;

/// <summary>Describes an incoming HTTP request to an <see cref="T:System.Net.HttpListener" /> object. This class cannot be inherited.</summary>
public sealed class HttpListenerRequest
{
	private class Context : TransportContext
	{
		public override ChannelBinding GetChannelBinding(ChannelBindingKind kind)
		{
			throw new NotImplementedException();
		}
	}

	private delegate X509Certificate2 GCCDelegate();

	private string[] accept_types;

	private Encoding content_encoding;

	private long content_length;

	private bool cl_set;

	private CookieCollection cookies;

	private WebHeaderCollection headers;

	private string method;

	private Stream input_stream;

	private Version version;

	private NameValueCollection query_string;

	private string raw_url;

	private Uri url;

	private Uri referrer;

	private string[] user_languages;

	private HttpListenerContext context;

	private bool is_chunked;

	private bool ka_set;

	private bool keep_alive;

	private GCCDelegate gcc_delegate;

	private static byte[] _100continue = Encoding.ASCII.GetBytes("HTTP/1.1 100 Continue\r\n\r\n");

	private static char[] separators = new char[1] { ' ' };

	/// <summary>Gets the MIME types accepted by the client. </summary>
	/// <returns>A <see cref="T:System.String" /> array that contains the type names specified in the request's Accept header or null if the client request did not include an Accept header.</returns>
	public string[] AcceptTypes => accept_types;

	/// <summary>Gets an error code that identifies a problem with the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> provided by the client.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that contains a Windows error code.</returns>
	/// <exception cref="T:System.InvalidOperationException">The client certificate has not been initialized yet by a call to the <see cref="M:System.Net.HttpListenerRequest.BeginGetClientCertificate(System.AsyncCallback,System.Object)" /> or <see cref="M:System.Net.HttpListenerRequest.GetClientCertificate" /> methods-or - The operation is still in progress.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public int ClientCertificateError
	{
		get
		{
			HttpConnection connection = context.Connection;
			if (connection.ClientCertificate == null)
			{
				throw new InvalidOperationException("No client certificate");
			}
			int[] clientCertificateErrors = connection.ClientCertificateErrors;
			if (clientCertificateErrors != null && clientCertificateErrors.Length != 0)
			{
				return clientCertificateErrors[0];
			}
			return 0;
		}
	}

	/// <summary>Gets the content encoding that can be used with data sent with the request</summary>
	/// <returns>An <see cref="T:System.Text.Encoding" /> object suitable for use with the data in the <see cref="P:System.Net.HttpListenerRequest.InputStream" /> property.</returns>
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
	}

	/// <summary>Gets the length of the body data included in the request.</summary>
	/// <returns>The value from the request's Content-Length header. This value is -1 if the content length is not known.</returns>
	public long ContentLength64
	{
		get
		{
			if (!is_chunked)
			{
				return content_length;
			}
			return -1L;
		}
	}

	/// <summary>Gets the MIME type of the body data included in the request.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the text of the request's Content-Type header.</returns>
	public string ContentType => headers["content-type"];

	/// <summary>Gets the cookies sent with the request.</summary>
	/// <returns>A <see cref="T:System.Net.CookieCollection" /> that contains cookies that accompany the request. This property returns an empty collection if the request does not contain cookies.</returns>
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
	}

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the request has associated body data.</summary>
	/// <returns>true if the request has associated body data; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public bool HasEntityBody
	{
		get
		{
			if (content_length <= 0)
			{
				return is_chunked;
			}
			return true;
		}
	}

	/// <summary>Gets the collection of header name/value pairs sent in the request.</summary>
	/// <returns>A <see cref="T:System.Net.WebHeaderCollection" /> that contains the HTTP headers included in the request.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public NameValueCollection Headers => headers;

	/// <summary>Gets the HTTP method specified by the client. </summary>
	/// <returns>A <see cref="T:System.String" /> that contains the method used in the request.</returns>
	public string HttpMethod => method;

	/// <summary>Gets a stream that contains the body data sent by the client.</summary>
	/// <returns>A readable <see cref="T:System.IO.Stream" /> object that contains the bytes sent by the client in the body of the request. This property returns <see cref="F:System.IO.Stream.Null" /> if no data is sent with the request.</returns>
	public Stream InputStream
	{
		get
		{
			if (input_stream == null)
			{
				if (is_chunked || content_length > 0)
				{
					input_stream = context.Connection.GetRequestStream(is_chunked, content_length);
				}
				else
				{
					input_stream = Stream.Null;
				}
			}
			return input_stream;
		}
	}

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the client sending this request is authenticated.</summary>
	/// <returns>true if the client was authenticated; otherwise, false.</returns>
	[System.MonoTODO("Always returns false")]
	public bool IsAuthenticated => false;

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the request is sent from the local computer.</summary>
	/// <returns>true if the request originated on the same computer as the <see cref="T:System.Net.HttpListener" /> object that provided the request; otherwise, false.</returns>
	public bool IsLocal => LocalEndPoint.Address.Equals(RemoteEndPoint.Address);

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the TCP connection used to send the request is using the Secure Sockets Layer (SSL) protocol.</summary>
	/// <returns>true if the TCP connection is using SSL; otherwise, false.</returns>
	public bool IsSecureConnection => context.Connection.IsSecure;

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the client requests a persistent connection.</summary>
	/// <returns>true if the connection should be kept open; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool KeepAlive
	{
		get
		{
			if (ka_set)
			{
				return keep_alive;
			}
			ka_set = true;
			string text = headers["Connection"];
			if (!string.IsNullOrEmpty(text))
			{
				keep_alive = string.Compare(text, "keep-alive", StringComparison.OrdinalIgnoreCase) == 0;
			}
			else if (version == HttpVersion.Version11)
			{
				keep_alive = true;
			}
			else
			{
				text = headers["keep-alive"];
				if (!string.IsNullOrEmpty(text))
				{
					keep_alive = string.Compare(text, "closed", StringComparison.OrdinalIgnoreCase) != 0;
				}
			}
			return keep_alive;
		}
	}

	/// <summary>Get the server IP address and port number to which the request is directed.</summary>
	/// <returns>An <see cref="T:System.Net.IPEndPoint" /> that represents the IP address that the request is sent to.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public IPEndPoint LocalEndPoint => context.Connection.LocalEndPoint;

	/// <summary>Gets the HTTP version used by the requesting client.</summary>
	/// <returns>A <see cref="T:System.Version" /> that identifies the client's version of HTTP.</returns>
	public Version ProtocolVersion => version;

	/// <summary>Gets the query string included in the request.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.NameValueCollection" /> object that contains the query data included in the request <see cref="P:System.Net.HttpListenerRequest.Url" />.</returns>
	public NameValueCollection QueryString => query_string;

	/// <summary>Gets the URL information (without the host and port) requested by the client.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the raw URL for this request.</returns>
	public string RawUrl => raw_url;

	/// <summary>Gets the client IP address and port number from which the request originated.</summary>
	/// <returns>An <see cref="T:System.Net.IPEndPoint" /> that represents the IP address and port number from which the request originated.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public IPEndPoint RemoteEndPoint => context.Connection.RemoteEndPoint;

	/// <summary>Gets the request identifier of the incoming HTTP request.</summary>
	/// <returns>A <see cref="T:System.Guid" /> object that contains the identifier of the HTTP request.</returns>
	[System.MonoTODO("Always returns Guid.Empty")]
	public Guid RequestTraceIdentifier => Guid.Empty;

	/// <summary>Gets the <see cref="T:System.Uri" /> object requested by the client.</summary>
	/// <returns>A <see cref="T:System.Uri" /> object that identifies the resource requested by the client.</returns>
	public Uri Url => url;

	/// <summary>Gets the Uniform Resource Identifier (URI) of the resource that referred the client to the server.</summary>
	/// <returns>A <see cref="T:System.Uri" /> object that contains the text of the request's <see cref="F:System.Net.HttpRequestHeader.Referer" /> header, or null if the header was not included in the request.</returns>
	public Uri UrlReferrer => referrer;

	/// <summary>Gets the user agent presented by the client.</summary>
	/// <returns>A <see cref="T:System.String" /> object that contains the text of the request's User-Agent header.</returns>
	public string UserAgent => headers["user-agent"];

	/// <summary>Gets the server IP address and port number to which the request is directed.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the host address information.</returns>
	public string UserHostAddress => LocalEndPoint.ToString();

	/// <summary>Gets the DNS name and, if provided, the port number specified by the client.</summary>
	/// <returns>A <see cref="T:System.String" /> value that contains the text of the request's Host header.</returns>
	public string UserHostName => headers["host"];

	/// <summary>Gets the natural languages that are preferred for the response.</summary>
	/// <returns>A <see cref="T:System.String" /> array that contains the languages specified in the request's <see cref="F:System.Net.HttpRequestHeader.AcceptLanguage" /> header or null if the client request did not include an <see cref="F:System.Net.HttpRequestHeader.AcceptLanguage" /> header.</returns>
	public string[] UserLanguages => user_languages;

	/// <summary>Gets the Service Provider Name (SPN) that the client sent on the request.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the SPN the client sent on the request. </returns>
	[System.MonoTODO]
	public string ServiceName => null;

	/// <summary>Gets the <see cref="T:System.Net.TransportContext" /> for the client request.</summary>
	/// <returns>A <see cref="T:System.Net.TransportContext" /> object for the client request.</returns>
	public TransportContext TransportContext => new Context();

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the TCP connection was  a WebSocket request.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the TCP connection is a WebSocket request; otherwise, false.</returns>
	[System.MonoTODO]
	public bool IsWebSocketRequest => false;

	internal HttpListenerRequest(HttpListenerContext context)
	{
		this.context = context;
		headers = new WebHeaderCollection();
		version = HttpVersion.Version10;
	}

	internal void SetRequestLine(string req)
	{
		string[] array = req.Split(separators, 3);
		if (array.Length != 3)
		{
			context.ErrorMessage = "Invalid request line (parts).";
			return;
		}
		method = array[0];
		string text = method;
		foreach (char c in text)
		{
			int num = c;
			if ((num < 65 || num > 90) && (num <= 32 || c >= '\u007f' || c == '(' || c == ')' || c == '<' || c == '<' || c == '>' || c == '@' || c == ',' || c == ';' || c == ':' || c == '\\' || c == '"' || c == '/' || c == '[' || c == ']' || c == '?' || c == '=' || c == '{' || c == '}'))
			{
				context.ErrorMessage = "(Invalid verb)";
				return;
			}
		}
		raw_url = array[1];
		if (array[2].Length != 8 || !array[2].StartsWith("HTTP/"))
		{
			context.ErrorMessage = "Invalid request line (version).";
			return;
		}
		try
		{
			version = new Version(array[2].Substring(5));
			if (version.Major < 1)
			{
				throw new Exception();
			}
		}
		catch
		{
			context.ErrorMessage = "Invalid request line (version).";
		}
	}

	private void CreateQueryString(string query)
	{
		if (query == null || query.Length == 0)
		{
			query_string = new NameValueCollection(1);
			return;
		}
		query_string = new NameValueCollection();
		if (query[0] == '?')
		{
			query = query.Substring(1);
		}
		string[] array = query.Split('&');
		foreach (string text in array)
		{
			int num = text.IndexOf('=');
			if (num == -1)
			{
				query_string.Add(null, WebUtility.UrlDecode(text));
				continue;
			}
			string name = WebUtility.UrlDecode(text.Substring(0, num));
			string value = WebUtility.UrlDecode(text.Substring(num + 1));
			query_string.Add(name, value);
		}
	}

	private static bool MaybeUri(string s)
	{
		int num = s.IndexOf(':');
		if (num == -1)
		{
			return false;
		}
		if (num >= 10)
		{
			return false;
		}
		return IsPredefinedScheme(s.Substring(0, num));
	}

	private static bool IsPredefinedScheme(string scheme)
	{
		if (scheme == null || scheme.Length < 3)
		{
			return false;
		}
		char c = scheme[0];
		if (c == 'h')
		{
			if (!(scheme == "http"))
			{
				return scheme == "https";
			}
			return true;
		}
		if (c == 'f')
		{
			if (!(scheme == "file"))
			{
				return scheme == "ftp";
			}
			return true;
		}
		if (c == 'n')
		{
			c = scheme[1];
			if (c == 'e')
			{
				if (!(scheme == "news") && !(scheme == "net.pipe"))
				{
					return scheme == "net.tcp";
				}
				return true;
			}
			if (scheme == "nntp")
			{
				return true;
			}
			return false;
		}
		if ((c == 'g' && scheme == "gopher") || (c == 'm' && scheme == "mailto"))
		{
			return true;
		}
		return false;
	}

	internal void FinishInitialization()
	{
		string text = UserHostName;
		if (version > HttpVersion.Version10 && (text == null || text.Length == 0))
		{
			context.ErrorMessage = "Invalid host name";
			return;
		}
		Uri result = null;
		string text2 = ((!MaybeUri(raw_url.ToLowerInvariant()) || !Uri.TryCreate(raw_url, UriKind.Absolute, out result)) ? raw_url : result.PathAndQuery);
		if (text == null || text.Length == 0)
		{
			text = UserHostAddress;
		}
		if (result != null)
		{
			text = result.Host;
		}
		int num = text.IndexOf(':');
		if (num >= 0)
		{
			text = text.Substring(0, num);
		}
		string text3 = string.Format("{0}://{1}:{2}", IsSecureConnection ? "https" : "http", text, LocalEndPoint.Port);
		if (!Uri.TryCreate(text3 + text2, UriKind.Absolute, out url))
		{
			context.ErrorMessage = WebUtility.HtmlEncode("Invalid url: " + text3 + text2);
			return;
		}
		CreateQueryString(url.Query);
		url = HttpListenerRequestUriBuilder.GetRequestUri(raw_url, url.Scheme, url.Authority, url.LocalPath, url.Query);
		if (version >= HttpVersion.Version11)
		{
			string text4 = Headers["Transfer-Encoding"];
			is_chunked = text4 != null && string.Compare(text4, "chunked", StringComparison.OrdinalIgnoreCase) == 0;
			if (text4 != null && !is_chunked)
			{
				context.Connection.SendError(null, 501);
				return;
			}
		}
		if (!is_chunked && !cl_set && (string.Compare(method, "POST", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(method, "PUT", StringComparison.OrdinalIgnoreCase) == 0))
		{
			context.Connection.SendError(null, 411);
		}
		else if (string.Compare(Headers["Expect"], "100-continue", StringComparison.OrdinalIgnoreCase) == 0)
		{
			context.Connection.GetResponseStream().InternalWrite(_100continue, 0, _100continue.Length);
		}
	}

	internal static string Unquote(string str)
	{
		int num = str.IndexOf('"');
		int num2 = str.LastIndexOf('"');
		if (num >= 0 && num2 >= 0)
		{
			str = str.Substring(num + 1, num2 - 1);
		}
		return str.Trim();
	}

	internal void AddHeader(string header)
	{
		int num = header.IndexOf(':');
		if (num == -1 || num == 0)
		{
			context.ErrorMessage = "Bad Request";
			context.ErrorStatus = 400;
			return;
		}
		string text = header.Substring(0, num).Trim();
		string text2 = header.Substring(num + 1).Trim();
		string text3 = text.ToLower(CultureInfo.InvariantCulture);
		headers.SetInternal(text, text2);
		switch (text3)
		{
		case "accept-language":
			user_languages = text2.Split(',');
			break;
		case "accept":
			accept_types = text2.Split(',');
			break;
		case "content-length":
			try
			{
				content_length = long.Parse(text2.Trim());
				if (content_length < 0)
				{
					context.ErrorMessage = "Invalid Content-Length.";
				}
				cl_set = true;
				break;
			}
			catch
			{
				context.ErrorMessage = "Invalid Content-Length.";
				break;
			}
		case "referer":
			try
			{
				referrer = new Uri(text2);
				break;
			}
			catch
			{
				referrer = new Uri("http://someone.is.screwing.with.the.headers.com/");
				break;
			}
		case "cookie":
		{
			if (cookies == null)
			{
				cookies = new CookieCollection();
			}
			string[] array = text2.Split(',', ';');
			Cookie cookie = null;
			int num2 = 0;
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string text4 = array2[i].Trim();
				if (text4.Length == 0)
				{
					continue;
				}
				if (text4.StartsWith("$Version"))
				{
					num2 = int.Parse(Unquote(text4.Substring(text4.IndexOf('=') + 1)));
					continue;
				}
				if (text4.StartsWith("$Path"))
				{
					if (cookie != null)
					{
						cookie.Path = text4.Substring(text4.IndexOf('=') + 1).Trim();
					}
					continue;
				}
				if (text4.StartsWith("$Domain"))
				{
					if (cookie != null)
					{
						cookie.Domain = text4.Substring(text4.IndexOf('=') + 1).Trim();
					}
					continue;
				}
				if (text4.StartsWith("$Port"))
				{
					if (cookie != null)
					{
						cookie.Port = text4.Substring(text4.IndexOf('=') + 1).Trim();
					}
					continue;
				}
				if (cookie != null)
				{
					cookies.Add(cookie);
				}
				try
				{
					cookie = new Cookie();
					int num3 = text4.IndexOf('=');
					if (num3 > 0)
					{
						cookie.Name = text4.Substring(0, num3).Trim();
						cookie.Value = text4.Substring(num3 + 1).Trim();
					}
					else
					{
						cookie.Name = text4.Trim();
						cookie.Value = string.Empty;
					}
					cookie.Version = num2;
				}
				catch (CookieException)
				{
					cookie = null;
				}
			}
			if (cookie != null)
			{
				cookies.Add(cookie);
			}
			break;
		}
		}
	}

	internal bool FlushInput()
	{
		if (!HasEntityBody)
		{
			return true;
		}
		int num = 2048;
		if (content_length > 0)
		{
			num = (int)Math.Min(content_length, num);
		}
		byte[] buffer = new byte[num];
		while (true)
		{
			try
			{
				IAsyncResult asyncResult = InputStream.BeginRead(buffer, 0, num, null, null);
				if (!asyncResult.IsCompleted && !asyncResult.AsyncWaitHandle.WaitOne(1000))
				{
					return false;
				}
				if (InputStream.EndRead(asyncResult) <= 0)
				{
					return true;
				}
			}
			catch (ObjectDisposedException)
			{
				input_stream = null;
				return true;
			}
			catch
			{
				return false;
			}
		}
	}

	/// <summary>Begins an asynchronous request for the client's X.509 v.3 certificate.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that indicates the status of the operation.</returns>
	/// <param name="requestCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete.</param>
	/// <param name="state">A user-defined object that contains information about the operation. This object is passed to the callback delegate when the operation completes.</param>
	public IAsyncResult BeginGetClientCertificate(AsyncCallback requestCallback, object state)
	{
		if (gcc_delegate == null)
		{
			gcc_delegate = GetClientCertificate;
		}
		return gcc_delegate.BeginInvoke(requestCallback, state);
	}

	/// <summary>Ends an asynchronous request for the client's X.509 v.3 certificate.</summary>
	/// <returns>The <see cref="T:System.IAsyncResult" /> object that is returned when the operation started.</returns>
	/// <param name="asyncResult">The pending request for the certificate.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not obtained by calling <see cref="M:System.Net.HttpListenerRequest.BeginGetClientCertificate(System.AsyncCallback,System.Object)" /><paramref name="e." /></exception>
	/// <exception cref="T:System.InvalidOperationException">This method was already called for the operation identified by <paramref name="asyncResult" />. </exception>
	public X509Certificate2 EndGetClientCertificate(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (gcc_delegate == null)
		{
			throw new InvalidOperationException();
		}
		return gcc_delegate.EndInvoke(asyncResult);
	}

	/// <summary>Retrieves the client's X.509 v.3 certificate.</summary>
	/// <returns>A <see cref="N:System.Security.Cryptography.X509Certificates" /> object that contains the client's X.509 v.3 certificate.</returns>
	/// <exception cref="T:System.InvalidOperationException">A call to this method to retrieve the client's X.509 v.3 certificate is in progress and therefore another call to this method cannot be made.</exception>
	public X509Certificate2 GetClientCertificate()
	{
		return context.Connection.ClientCertificate;
	}

	/// <summary>Retrieves the client's X.509 v.3 certificate as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="N:System.Security.Cryptography.X509Certificates" /> object that contains the client's X.509 v.3 certificate.</returns>
	public Task<X509Certificate2> GetClientCertificateAsync()
	{
		return Task<X509Certificate2>.Factory.FromAsync(BeginGetClientCertificate, EndGetClientCertificate, null);
	}

	internal HttpListenerRequest()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
