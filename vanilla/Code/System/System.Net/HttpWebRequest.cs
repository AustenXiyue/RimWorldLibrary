using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.Cache;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Mono.Security.Interface;
using Unity;

namespace System.Net;

/// <summary>Provides an HTTP-specific implementation of the <see cref="T:System.Net.WebRequest" /> class.</summary>
[Serializable]
public class HttpWebRequest : WebRequest, ISerializable
{
	private enum NtlmAuthState
	{
		None,
		Challenge,
		Response
	}

	private struct AuthorizationState
	{
		private readonly HttpWebRequest request;

		private readonly bool isProxy;

		private bool isCompleted;

		private NtlmAuthState ntlm_auth_state;

		public bool IsCompleted => isCompleted;

		public NtlmAuthState NtlmAuthState => ntlm_auth_state;

		public bool IsNtlmAuthenticated
		{
			get
			{
				if (isCompleted)
				{
					return ntlm_auth_state != NtlmAuthState.None;
				}
				return false;
			}
		}

		public AuthorizationState(HttpWebRequest request, bool isProxy)
		{
			this.request = request;
			this.isProxy = isProxy;
			isCompleted = false;
			ntlm_auth_state = NtlmAuthState.None;
		}

		public bool CheckAuthorization(WebResponse response, HttpStatusCode code)
		{
			isCompleted = false;
			if (code == HttpStatusCode.Unauthorized && request.credentials == null)
			{
				return false;
			}
			if (isProxy != (code == HttpStatusCode.ProxyAuthenticationRequired))
			{
				return false;
			}
			if (isProxy && (request.proxy == null || request.proxy.Credentials == null))
			{
				return false;
			}
			string[] values = response.Headers.GetValues(isProxy ? "Proxy-Authenticate" : "WWW-Authenticate");
			if (values == null || values.Length == 0)
			{
				return false;
			}
			ICredentials credentials = ((!isProxy) ? request.credentials : request.proxy.Credentials);
			Authorization authorization = null;
			string[] array = values;
			for (int i = 0; i < array.Length; i++)
			{
				authorization = AuthenticationManager.Authenticate(array[i], request, credentials);
				if (authorization != null)
				{
					break;
				}
			}
			if (authorization == null)
			{
				return false;
			}
			request.webHeaders[isProxy ? "Proxy-Authorization" : "Authorization"] = authorization.Message;
			isCompleted = authorization.Complete;
			if (authorization.ModuleAuthenticationType == "NTLM")
			{
				ntlm_auth_state++;
			}
			return true;
		}

		public void Reset()
		{
			isCompleted = false;
			ntlm_auth_state = NtlmAuthState.None;
			request.webHeaders.RemoveInternal(isProxy ? "Proxy-Authorization" : "Authorization");
		}

		public override string ToString()
		{
			return string.Format("{0}AuthState [{1}:{2}]", isProxy ? "Proxy" : "", isCompleted, ntlm_auth_state);
		}
	}

	private Uri requestUri;

	private Uri actualUri;

	private bool hostChanged;

	private bool allowAutoRedirect;

	private bool allowBuffering;

	private X509CertificateCollection certificates;

	private string connectionGroup;

	private bool haveContentLength;

	private long contentLength;

	private HttpContinueDelegate continueDelegate;

	private CookieContainer cookieContainer;

	private ICredentials credentials;

	private bool haveResponse;

	private bool haveRequest;

	private bool requestSent;

	private WebHeaderCollection webHeaders;

	private bool keepAlive;

	private int maxAutoRedirect;

	private string mediaType;

	private string method;

	private string initialMethod;

	private bool pipelined;

	private bool preAuthenticate;

	private bool usedPreAuth;

	private Version version;

	private bool force_version;

	private Version actualVersion;

	private IWebProxy proxy;

	private bool sendChunked;

	private ServicePoint servicePoint;

	private int timeout;

	private WebConnectionStream writeStream;

	private HttpWebResponse webResponse;

	private WebAsyncResult asyncWrite;

	private WebAsyncResult asyncRead;

	private EventHandler abortHandler;

	private int aborted;

	private bool gotRequestStream;

	private int redirects;

	private bool expectContinue;

	private byte[] bodyBuffer;

	private int bodyBufferLength;

	private bool getResponseCalled;

	private Exception saved_exc;

	private object locker;

	private bool finished_reading;

	internal WebConnection WebConnection;

	private DecompressionMethods auto_decomp;

	private int maxResponseHeadersLength;

	private static int defaultMaxResponseHeadersLength;

	private int readWriteTimeout;

	private MonoTlsProvider tlsProvider;

	private MonoTlsSettings tlsSettings;

	private ServerCertValidationCallback certValidationCallback;

	private AuthorizationState auth_state;

	private AuthorizationState proxy_auth_state;

	private string host;

	[NonSerialized]
	internal Action<Stream> ResendContentFactory;

	private bool unsafe_auth_blah;

	internal WebConnection StoredConnection;

	/// <summary>Gets or sets the value of the Accept HTTP header.</summary>
	/// <returns>The value of the Accept HTTP header. The default value is null.</returns>
	public string Accept
	{
		get
		{
			return webHeaders["Accept"];
		}
		set
		{
			CheckRequestStarted();
			SetSpecialHeaders("Accept", value);
		}
	}

	/// <summary>Gets the Uniform Resource Identifier (URI) of the Internet resource that actually responds to the request.</summary>
	/// <returns>A <see cref="T:System.Uri" /> that identifies the Internet resource that actually responds to the request. The default is the URI used by the <see cref="M:System.Net.WebRequest.Create(System.String)" /> method to initialize the request.</returns>
	public Uri Address
	{
		get
		{
			return actualUri;
		}
		internal set
		{
			actualUri = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the request should follow redirection responses.</summary>
	/// <returns>true if the request should automatically follow redirection responses from the Internet resource; otherwise, false. The default value is true.</returns>
	public virtual bool AllowAutoRedirect
	{
		get
		{
			return allowAutoRedirect;
		}
		set
		{
			allowAutoRedirect = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to buffer the data sent to the Internet resource.</summary>
	/// <returns>true to enable buffering of the data sent to the Internet resource; false to disable buffering. The default is true.</returns>
	public virtual bool AllowWriteStreamBuffering
	{
		get
		{
			return allowBuffering;
		}
		set
		{
			allowBuffering = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to buffer the received from the  Internet resource.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true to enable buffering of the data received from the Internet resource; false to disable buffering. The default is true.</returns>
	public virtual bool AllowReadStreamBuffering
	{
		get
		{
			return false;
		}
		set
		{
			if (value)
			{
				throw new InvalidOperationException();
			}
		}
	}

	/// <summary>Gets or sets the type of decompression that is used.</summary>
	/// <returns>A T:System.Net.DecompressionMethods object that indicates the type of decompression that is used. </returns>
	/// <exception cref="T:System.InvalidOperationException">The object's current state does not allow this property to be set.</exception>
	public DecompressionMethods AutomaticDecompression
	{
		get
		{
			return auto_decomp;
		}
		set
		{
			CheckRequestStarted();
			auto_decomp = value;
		}
	}

	internal bool InternalAllowBuffering
	{
		get
		{
			if (allowBuffering)
			{
				return MethodWithBuffer;
			}
			return false;
		}
	}

	private bool MethodWithBuffer
	{
		get
		{
			if (method != "HEAD" && method != "GET" && method != "MKCOL" && method != "CONNECT")
			{
				return method != "TRACE";
			}
			return false;
		}
	}

	internal MonoTlsProvider TlsProvider => tlsProvider;

	internal MonoTlsSettings TlsSettings => tlsSettings;

	/// <summary>Gets or sets the collection of security certificates that are associated with this request.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.X509Certificates.X509CertificateCollection" /> that contains the security certificates associated with this request.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value specified for a set operation is null. </exception>
	public X509CertificateCollection ClientCertificates
	{
		get
		{
			if (certificates == null)
			{
				certificates = new X509CertificateCollection();
			}
			return certificates;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			certificates = value;
		}
	}

	/// <summary>Gets or sets the value of the Connection HTTP header.</summary>
	/// <returns>The value of the Connection HTTP header. The default value is null.</returns>
	/// <exception cref="T:System.ArgumentException">The value of <see cref="P:System.Net.HttpWebRequest.Connection" /> is set to Keep-alive or Close. </exception>
	public string Connection
	{
		get
		{
			return webHeaders["Connection"];
		}
		set
		{
			CheckRequestStarted();
			if (string.IsNullOrEmpty(value))
			{
				webHeaders.RemoveInternal("Connection");
				return;
			}
			string text = value.ToLowerInvariant();
			if (text.Contains("keep-alive") || text.Contains("close"))
			{
				throw new ArgumentException("Keep-Alive and Close may not be set with this property");
			}
			if (keepAlive)
			{
				value += ", Keep-Alive";
			}
			webHeaders.CheckUpdate("Connection", value);
		}
	}

	/// <summary>Gets or sets the name of the connection group for the request.</summary>
	/// <returns>The name of the connection group for this request. The default value is null.</returns>
	public override string ConnectionGroupName
	{
		get
		{
			return connectionGroup;
		}
		set
		{
			connectionGroup = value;
		}
	}

	/// <summary>Gets or sets the Content-length HTTP header.</summary>
	/// <returns>The number of bytes of data to send to the Internet resource. The default is -1, which indicates the property has not been set and that there is no request data to send.</returns>
	/// <exception cref="T:System.InvalidOperationException">The request has been started by calling the <see cref="M:System.Net.HttpWebRequest.GetRequestStream" />, <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />, <see cref="M:System.Net.HttpWebRequest.GetResponse" />, or <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> method. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The new <see cref="P:System.Net.HttpWebRequest.ContentLength" /> value is less than 0. </exception>
	public override long ContentLength
	{
		get
		{
			return contentLength;
		}
		set
		{
			CheckRequestStarted();
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", "Content-Length must be >= 0");
			}
			contentLength = value;
			haveContentLength = true;
		}
	}

	internal long InternalContentLength
	{
		set
		{
			contentLength = value;
		}
	}

	internal bool ThrowOnError { get; set; }

	/// <summary>Gets or sets the value of the Content-type HTTP header.</summary>
	/// <returns>The value of the Content-type HTTP header. The default value is null.</returns>
	public override string ContentType
	{
		get
		{
			return webHeaders["Content-Type"];
		}
		set
		{
			SetSpecialHeaders("Content-Type", value);
		}
	}

	/// <summary>Gets or sets the delegate method called when an HTTP 100-continue response is received from the Internet resource.</summary>
	/// <returns>A delegate that implements the callback method that executes when an HTTP Continue response is returned from the Internet resource. The default value is null.</returns>
	public HttpContinueDelegate ContinueDelegate
	{
		get
		{
			return continueDelegate;
		}
		set
		{
			continueDelegate = value;
		}
	}

	/// <summary>Gets or sets the cookies associated with the request.</summary>
	/// <returns>A <see cref="T:System.Net.CookieContainer" /> that contains the cookies associated with this request.</returns>
	public virtual CookieContainer CookieContainer
	{
		get
		{
			return cookieContainer;
		}
		set
		{
			cookieContainer = value;
		}
	}

	/// <summary>Gets or sets authentication information for the request.</summary>
	/// <returns>An <see cref="T:System.Net.ICredentials" /> that contains the authentication credentials associated with the request. The default is null.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override ICredentials Credentials
	{
		get
		{
			return credentials;
		}
		set
		{
			credentials = value;
		}
	}

	/// <summary>Get or set the Date HTTP header value to use in an HTTP request.</summary>
	/// <returns>The Date header value in the HTTP request.</returns>
	public DateTime Date
	{
		get
		{
			string text = webHeaders["Date"];
			if (text == null)
			{
				return DateTime.MinValue;
			}
			return DateTime.ParseExact(text, "r", CultureInfo.InvariantCulture).ToLocalTime();
		}
		set
		{
			SetDateHeaderHelper("Date", value);
		}
	}

	/// <summary>Gets or sets the default cache policy for this request.</summary>
	/// <returns>A <see cref="T:System.Net.Cache.HttpRequestCachePolicy" /> that specifies the cache policy in effect for this request when no other policy is applicable.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO]
	public new static RequestCachePolicy DefaultCachePolicy
	{
		get
		{
			throw GetMustImplement();
		}
		set
		{
			throw GetMustImplement();
		}
	}

	/// <summary>Gets or sets the default maximum length of an HTTP error response.</summary>
	/// <returns>An integer that represents the default maximum length of an HTTP error response.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value is less than 0 and is not equal to -1. </exception>
	[System.MonoTODO]
	public static int DefaultMaximumErrorResponseLength
	{
		get
		{
			throw GetMustImplement();
		}
		set
		{
			throw GetMustImplement();
		}
	}

	/// <summary>Gets or sets the value of the Expect HTTP header.</summary>
	/// <returns>The contents of the Expect HTTP header. The default value is null.NoteThe value for this property is stored in <see cref="T:System.Net.WebHeaderCollection" />. If WebHeaderCollection is set, the property value is lost.</returns>
	/// <exception cref="T:System.ArgumentException">Expect is set to a string that contains "100-continue" as a substring. </exception>
	public string Expect
	{
		get
		{
			return webHeaders["Expect"];
		}
		set
		{
			CheckRequestStarted();
			string text = value;
			if (text != null)
			{
				text = text.Trim().ToLower();
			}
			if (text == null || text.Length == 0)
			{
				webHeaders.RemoveInternal("Expect");
				return;
			}
			if (text == "100-continue")
			{
				throw new ArgumentException("100-Continue cannot be set with this property.", "value");
			}
			webHeaders.CheckUpdate("Expect", value);
		}
	}

	/// <summary>Gets a value that indicates whether a response has been received from an Internet resource.</summary>
	/// <returns>true if a response has been received; otherwise, false.</returns>
	public virtual bool HaveResponse => haveResponse;

	/// <summary>Specifies a collection of the name/value pairs that make up the HTTP headers.</summary>
	/// <returns>A <see cref="T:System.Net.WebHeaderCollection" /> that contains the name/value pairs that make up the headers for the HTTP request.</returns>
	/// <exception cref="T:System.InvalidOperationException">The request has been started by calling the <see cref="M:System.Net.HttpWebRequest.GetRequestStream" />, <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />, <see cref="M:System.Net.HttpWebRequest.GetResponse" />, or <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> method. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override WebHeaderCollection Headers
	{
		get
		{
			return webHeaders;
		}
		set
		{
			CheckRequestStarted();
			WebHeaderCollection webHeaderCollection = new WebHeaderCollection(WebHeaderCollectionType.HttpWebRequest);
			string[] allKeys = value.AllKeys;
			foreach (string name in allKeys)
			{
				webHeaderCollection.Add(name, value[name]);
			}
			webHeaders = webHeaderCollection;
		}
	}

	/// <summary>Get or set the Host header value to use in an HTTP request independent from the request URI.</summary>
	/// <returns>The Host header value in the HTTP request.</returns>
	/// <exception cref="T:System.ArgumentNullException">The Host header cannot be set to null. </exception>
	/// <exception cref="T:System.ArgumentException">The Host header cannot be set to an invalid value. </exception>
	/// <exception cref="T:System.InvalidOperationException">The Host header cannot be set after the <see cref="T:System.Net.HttpWebRequest" /> has already started to be sent. </exception>
	public string Host
	{
		get
		{
			if (host == null)
			{
				return actualUri.Authority;
			}
			return host;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!CheckValidHost(actualUri.Scheme, value))
			{
				throw new ArgumentException("Invalid host: " + value);
			}
			host = value;
		}
	}

	/// <summary>Gets or sets the value of the If-Modified-Since HTTP header.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> that contains the contents of the If-Modified-Since HTTP header. The default value is the current date and time.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public DateTime IfModifiedSince
	{
		get
		{
			string text = webHeaders["If-Modified-Since"];
			if (text == null)
			{
				return DateTime.Now;
			}
			try
			{
				return MonoHttpDate.Parse(text);
			}
			catch (Exception)
			{
				return DateTime.Now;
			}
		}
		set
		{
			CheckRequestStarted();
			webHeaders.SetInternal("If-Modified-Since", value.ToUniversalTime().ToString("r", null));
		}
	}

	/// <summary>Gets or sets a value that indicates whether to make a persistent connection to the Internet resource.</summary>
	/// <returns>true if the request to the Internet resource should contain a Connection HTTP header with the value Keep-alive; otherwise, false. The default is true.</returns>
	public bool KeepAlive
	{
		get
		{
			return keepAlive;
		}
		set
		{
			keepAlive = value;
		}
	}

	/// <summary>Gets or sets the maximum number of redirects that the request follows.</summary>
	/// <returns>The maximum number of redirection responses that the request follows. The default value is 50.</returns>
	/// <exception cref="T:System.ArgumentException">The value is set to 0 or less. </exception>
	public int MaximumAutomaticRedirections
	{
		get
		{
			return maxAutoRedirect;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentException("Must be > 0", "value");
			}
			maxAutoRedirect = value;
		}
	}

	/// <summary>Gets or sets the maximum allowed length of the response headers.</summary>
	/// <returns>The length, in kilobytes (1024 bytes), of the response headers.</returns>
	/// <exception cref="T:System.InvalidOperationException">The property is set after the request has already been submitted. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value is less than 0 and is not equal to -1. </exception>
	[System.MonoTODO("Use this")]
	public int MaximumResponseHeadersLength
	{
		get
		{
			return maxResponseHeadersLength;
		}
		set
		{
			maxResponseHeadersLength = value;
		}
	}

	/// <summary>Gets or sets the default for the <see cref="P:System.Net.HttpWebRequest.MaximumResponseHeadersLength" /> property.</summary>
	/// <returns>The length, in kilobytes (1024 bytes), of the default maximum for response headers received. The default configuration file sets this value to 64 kilobytes.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value is not equal to -1 and is less than zero. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO("Use this")]
	public static int DefaultMaximumResponseHeadersLength
	{
		get
		{
			return defaultMaxResponseHeadersLength;
		}
		set
		{
			defaultMaxResponseHeadersLength = value;
		}
	}

	/// <summary>Gets or sets a time-out in milliseconds when writing to or reading from a stream.</summary>
	/// <returns>The number of milliseconds before the writing or reading times out. The default value is 300,000 milliseconds (5 minutes).</returns>
	/// <exception cref="T:System.InvalidOperationException">The request has already been sent. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for a set operation is less than or equal to zero and is not equal to <see cref="F:System.Threading.Timeout.Infinite" /></exception>
	public int ReadWriteTimeout
	{
		get
		{
			return readWriteTimeout;
		}
		set
		{
			if (requestSent)
			{
				throw new InvalidOperationException("The request has already been sent.");
			}
			if (value < -1)
			{
				throw new ArgumentOutOfRangeException("value", "Must be >= -1");
			}
			readWriteTimeout = value;
		}
	}

	/// <summary>Gets or sets a timeout, in seconds, to wait for the server status after 100-continue is received.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.The timeout, in seconds, to wait for the server status after 100-continue is received.</returns>
	[System.MonoTODO]
	public int ContinueTimeout
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the media type of the request.</summary>
	/// <returns>The media type of the request. The default value is null.</returns>
	public string MediaType
	{
		get
		{
			return mediaType;
		}
		set
		{
			mediaType = value;
		}
	}

	/// <summary>Gets or sets the method for the request.</summary>
	/// <returns>The request method to use to contact the Internet resource. The default value is GET.</returns>
	/// <exception cref="T:System.ArgumentException">No method is supplied.-or- The method string contains invalid characters. </exception>
	public override string Method
	{
		get
		{
			return method;
		}
		set
		{
			if (value == null || value.Trim() == "")
			{
				throw new ArgumentException("not a valid method");
			}
			method = value.ToUpperInvariant();
			if (method != "HEAD" && method != "GET" && method != "POST" && method != "PUT" && method != "DELETE" && method != "CONNECT" && method != "TRACE" && method != "MKCOL")
			{
				method = value;
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether to pipeline the request to the Internet resource.</summary>
	/// <returns>true if the request should be pipelined; otherwise, false. The default is true.</returns>
	public bool Pipelined
	{
		get
		{
			return pipelined;
		}
		set
		{
			pipelined = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to send an Authorization header with the request.</summary>
	/// <returns>true to send an  HTTP Authorization header with requests after authentication has taken place; otherwise, false. The default is false.</returns>
	public override bool PreAuthenticate
	{
		get
		{
			return preAuthenticate;
		}
		set
		{
			preAuthenticate = value;
		}
	}

	/// <summary>Gets or sets the version of HTTP to use for the request.</summary>
	/// <returns>The HTTP version to use for the request. The default is <see cref="F:System.Net.HttpVersion.Version11" />.</returns>
	/// <exception cref="T:System.ArgumentException">The HTTP version is set to a value other than 1.0 or 1.1. </exception>
	public Version ProtocolVersion
	{
		get
		{
			return version;
		}
		set
		{
			if (value != HttpVersion.Version10 && value != HttpVersion.Version11)
			{
				throw new ArgumentException("value");
			}
			force_version = true;
			version = value;
		}
	}

	/// <summary>Gets or sets proxy information for the request.</summary>
	/// <returns>The <see cref="T:System.Net.IWebProxy" /> object to use to proxy the request. The default value is set by calling the <see cref="P:System.Net.GlobalProxySelection.Select" /> property.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	///   <see cref="P:System.Net.HttpWebRequest.Proxy" /> is set to null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The request has been started by calling <see cref="M:System.Net.HttpWebRequest.GetRequestStream" />, <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />, <see cref="M:System.Net.HttpWebRequest.GetResponse" />, or <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have permission for the requested operation. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override IWebProxy Proxy
	{
		get
		{
			return proxy;
		}
		set
		{
			CheckRequestStarted();
			proxy = value;
			servicePoint = null;
			GetServicePoint();
		}
	}

	/// <summary>Gets or sets the value of the Referer HTTP header.</summary>
	/// <returns>The value of the Referer HTTP header. The default value is null.</returns>
	public string Referer
	{
		get
		{
			return webHeaders["Referer"];
		}
		set
		{
			CheckRequestStarted();
			if (value == null || value.Trim().Length == 0)
			{
				webHeaders.RemoveInternal("Referer");
			}
			else
			{
				webHeaders.SetInternal("Referer", value);
			}
		}
	}

	/// <summary>Gets the original Uniform Resource Identifier (URI) of the request.</summary>
	/// <returns>A <see cref="T:System.Uri" /> that contains the URI of the Internet resource passed to the <see cref="M:System.Net.WebRequest.Create(System.String)" /> method.</returns>
	public override Uri RequestUri => requestUri;

	/// <summary>Gets or sets a value that indicates whether to send data in segments to the Internet resource.</summary>
	/// <returns>true to send data to the Internet resource in segments; otherwise, false. The default value is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The request has been started by calling the <see cref="M:System.Net.HttpWebRequest.GetRequestStream" />, <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />, <see cref="M:System.Net.HttpWebRequest.GetResponse" />, or <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> method. </exception>
	public bool SendChunked
	{
		get
		{
			return sendChunked;
		}
		set
		{
			CheckRequestStarted();
			sendChunked = value;
		}
	}

	/// <summary>Gets the service point to use for the request.</summary>
	/// <returns>A <see cref="T:System.Net.ServicePoint" /> that represents the network connection to the Internet resource.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public ServicePoint ServicePoint => GetServicePoint();

	internal ServicePoint ServicePointNoLock => servicePoint;

	/// <summary>Gets a value that indicates whether the request provides support for a <see cref="T:System.Net.CookieContainer" />.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if a <see cref="T:System.Net.CookieContainer" /> is supported; otherwise, false. </returns>
	public virtual bool SupportsCookieContainer => true;

	/// <summary>Gets or sets the time-out value in milliseconds for the <see cref="M:System.Net.HttpWebRequest.GetResponse" /> and <see cref="M:System.Net.HttpWebRequest.GetRequestStream" /> methods.</summary>
	/// <returns>The number of milliseconds to wait before the request times out. The default value is 100,000 milliseconds (100 seconds).</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified is less than zero and is not <see cref="F:System.Threading.Timeout.Infinite" />.</exception>
	public override int Timeout
	{
		get
		{
			return timeout;
		}
		set
		{
			if (value < -1)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			timeout = value;
		}
	}

	/// <summary>Gets or sets the value of the Transfer-encoding HTTP header.</summary>
	/// <returns>The value of the Transfer-encoding HTTP header. The default value is null.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set when <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to the value "Chunked". </exception>
	public string TransferEncoding
	{
		get
		{
			return webHeaders["Transfer-Encoding"];
		}
		set
		{
			CheckRequestStarted();
			string text = value;
			if (text != null)
			{
				text = text.Trim().ToLower();
			}
			if (text == null || text.Length == 0)
			{
				webHeaders.RemoveInternal("Transfer-Encoding");
				return;
			}
			if (text == "chunked")
			{
				throw new ArgumentException("Chunked encoding must be set with the SendChunked property");
			}
			if (!sendChunked)
			{
				throw new ArgumentException("SendChunked must be True", "value");
			}
			webHeaders.CheckUpdate("Transfer-Encoding", value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that controls whether default credentials are sent with requests.</summary>
	/// <returns>true if the default credentials are used; otherwise false. The default value is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">You attempted to set this property after the request was sent.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="USERNAME" />
	/// </PermissionSet>
	public override bool UseDefaultCredentials
	{
		get
		{
			return CredentialCache.DefaultCredentials == Credentials;
		}
		set
		{
			Credentials = (value ? CredentialCache.DefaultCredentials : null);
		}
	}

	/// <summary>Gets or sets the value of the User-agent HTTP header.</summary>
	/// <returns>The value of the User-agent HTTP header. The default value is null.NoteThe value for this property is stored in <see cref="T:System.Net.WebHeaderCollection" />. If WebHeaderCollection is set, the property value is lost.</returns>
	public string UserAgent
	{
		get
		{
			return webHeaders["User-Agent"];
		}
		set
		{
			webHeaders.SetInternal("User-Agent", value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether to allow high-speed NTLM-authenticated connection sharing.</summary>
	/// <returns>true to keep the authenticated connection open; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool UnsafeAuthenticatedConnectionSharing
	{
		get
		{
			return unsafe_auth_blah;
		}
		set
		{
			unsafe_auth_blah = value;
		}
	}

	internal bool GotRequestStream => gotRequestStream;

	internal bool ExpectContinue
	{
		get
		{
			return expectContinue;
		}
		set
		{
			expectContinue = value;
		}
	}

	internal Uri AuthUri => actualUri;

	internal bool ProxyQuery
	{
		get
		{
			if (servicePoint.UsesProxy)
			{
				return !servicePoint.UseConnect;
			}
			return false;
		}
	}

	internal ServerCertValidationCallback ServerCertValidationCallback => certValidationCallback;

	/// <summary>Gets or sets a callback function to validate the server certificate.</summary>
	/// <returns>Returns <see cref="T:System.Net.Security.RemoteCertificateValidationCallback" />.A callback function to validate the server certificate.</returns>
	public RemoteCertificateValidationCallback ServerCertificateValidationCallback
	{
		get
		{
			if (certValidationCallback == null)
			{
				return null;
			}
			return certValidationCallback.ValidationCallback;
		}
		set
		{
			if (value == null)
			{
				certValidationCallback = null;
			}
			else
			{
				certValidationCallback = new ServerCertValidationCallback(value);
			}
		}
	}

	internal bool FinishedReading
	{
		get
		{
			return finished_reading;
		}
		set
		{
			finished_reading = value;
		}
	}

	internal bool Aborted => Interlocked.CompareExchange(ref aborted, 0, 0) == 1;

	internal bool ReuseConnection { get; set; }

	static HttpWebRequest()
	{
		defaultMaxResponseHeadersLength = 65536;
		if (ConfigurationSettings.GetConfig("system.net/settings") is NetConfig { MaxResponseHeadersLength: var num })
		{
			if (num != -1)
			{
				num *= 64;
			}
			defaultMaxResponseHeadersLength = num;
		}
	}

	internal HttpWebRequest(Uri uri)
	{
		allowAutoRedirect = true;
		allowBuffering = true;
		contentLength = -1L;
		keepAlive = true;
		maxAutoRedirect = 50;
		mediaType = string.Empty;
		method = "GET";
		initialMethod = "GET";
		pipelined = true;
		version = HttpVersion.Version11;
		timeout = 100000;
		locker = new object();
		readWriteTimeout = 300000;
		base._002Ector();
		requestUri = uri;
		actualUri = uri;
		proxy = WebRequest.InternalDefaultWebProxy;
		webHeaders = new WebHeaderCollection(WebHeaderCollectionType.HttpWebRequest);
		ThrowOnError = true;
		ResetAuthorization();
	}

	internal HttpWebRequest(Uri uri, MonoTlsProvider tlsProvider, MonoTlsSettings settings = null)
		: this(uri)
	{
		this.tlsProvider = tlsProvider;
		tlsSettings = settings;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.HttpWebRequest" /> class from the specified instances of the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> classes.</summary>
	/// <param name="serializationInfo">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the new <see cref="T:System.Net.HttpWebRequest" /> object. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that contains the source and destination of the serialized stream associated with the new <see cref="T:System.Net.HttpWebRequest" /> object. </param>
	[Obsolete("Serialization is obsoleted for this type", false)]
	protected HttpWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		allowAutoRedirect = true;
		allowBuffering = true;
		contentLength = -1L;
		keepAlive = true;
		maxAutoRedirect = 50;
		mediaType = string.Empty;
		method = "GET";
		initialMethod = "GET";
		pipelined = true;
		version = HttpVersion.Version11;
		timeout = 100000;
		locker = new object();
		readWriteTimeout = 300000;
		base._002Ector();
		requestUri = (Uri)serializationInfo.GetValue("requestUri", typeof(Uri));
		actualUri = (Uri)serializationInfo.GetValue("actualUri", typeof(Uri));
		allowAutoRedirect = serializationInfo.GetBoolean("allowAutoRedirect");
		allowBuffering = serializationInfo.GetBoolean("allowBuffering");
		certificates = (X509CertificateCollection)serializationInfo.GetValue("certificates", typeof(X509CertificateCollection));
		connectionGroup = serializationInfo.GetString("connectionGroup");
		contentLength = serializationInfo.GetInt64("contentLength");
		webHeaders = (WebHeaderCollection)serializationInfo.GetValue("webHeaders", typeof(WebHeaderCollection));
		keepAlive = serializationInfo.GetBoolean("keepAlive");
		maxAutoRedirect = serializationInfo.GetInt32("maxAutoRedirect");
		mediaType = serializationInfo.GetString("mediaType");
		method = serializationInfo.GetString("method");
		initialMethod = serializationInfo.GetString("initialMethod");
		pipelined = serializationInfo.GetBoolean("pipelined");
		version = (Version)serializationInfo.GetValue("version", typeof(Version));
		proxy = (IWebProxy)serializationInfo.GetValue("proxy", typeof(IWebProxy));
		sendChunked = serializationInfo.GetBoolean("sendChunked");
		timeout = serializationInfo.GetInt32("timeout");
		redirects = serializationInfo.GetInt32("redirects");
		host = serializationInfo.GetString("host");
		ResetAuthorization();
	}

	private void ResetAuthorization()
	{
		auth_state = new AuthorizationState(this, isProxy: false);
		proxy_auth_state = new AuthorizationState(this, isProxy: true);
	}

	private void SetSpecialHeaders(string HeaderName, string value)
	{
		value = WebHeaderCollection.CheckBadChars(value, isHeaderValue: true);
		webHeaders.RemoveInternal(HeaderName);
		if (value.Length != 0)
		{
			webHeaders.AddInternal(HeaderName, value);
		}
	}

	private static Exception GetMustImplement()
	{
		return new NotImplementedException();
	}

	private void SetDateHeaderHelper(string headerName, DateTime dateTime)
	{
		if (dateTime == DateTime.MinValue)
		{
			SetSpecialHeaders(headerName, null);
		}
		else
		{
			SetSpecialHeaders(headerName, HttpProtocolUtils.date2string(dateTime));
		}
	}

	private static bool CheckValidHost(string scheme, string val)
	{
		if (val.Length == 0)
		{
			return false;
		}
		if (val[0] == '.')
		{
			return false;
		}
		if (val.IndexOf('/') >= 0)
		{
			return false;
		}
		if (IPAddress.TryParse(val, out var _))
		{
			return true;
		}
		return Uri.IsWellFormedUriString(scheme + "://" + val + "/", UriKind.Absolute);
	}

	internal ServicePoint GetServicePoint()
	{
		lock (locker)
		{
			if (hostChanged || servicePoint == null)
			{
				servicePoint = ServicePointManager.FindServicePoint(actualUri, proxy);
				hostChanged = false;
			}
		}
		return servicePoint;
	}

	/// <summary>Adds a byte range header to a request for a specific range from the beginning or end of the requested data.</summary>
	/// <param name="range">The starting or ending point of the range. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(int range)
	{
		AddRange("bytes", (long)range);
	}

	/// <summary>Adds a byte range header to the request for a specified range.</summary>
	/// <param name="from">The position at which to start sending data. </param>
	/// <param name="to">The position at which to stop sending data. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="from" /> is greater than <paramref name="to" />-or- <paramref name="from" /> or <paramref name="to" /> is less than 0. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(int from, int to)
	{
		AddRange("bytes", (long)from, (long)to);
	}

	/// <summary>Adds a Range header to a request for a specific range from the beginning or end of the requested data.</summary>
	/// <param name="rangeSpecifier">The description of the range. </param>
	/// <param name="range">The starting or ending point of the range. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rangeSpecifier" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(string rangeSpecifier, int range)
	{
		AddRange(rangeSpecifier, (long)range);
	}

	/// <summary>Adds a range header to a request for a specified range.</summary>
	/// <param name="rangeSpecifier">The description of the range. </param>
	/// <param name="from">The position at which to start sending data. </param>
	/// <param name="to">The position at which to stop sending data. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rangeSpecifier" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="from" /> is greater than <paramref name="to" />-or- <paramref name="from" /> or <paramref name="to" /> is less than 0. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(string rangeSpecifier, int from, int to)
	{
		AddRange(rangeSpecifier, (long)from, (long)to);
	}

	/// <summary>Adds a byte range header to a request for a specific range from the beginning or end of the requested data.</summary>
	/// <param name="range">The starting or ending point of the range.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(long range)
	{
		AddRange("bytes", range);
	}

	/// <summary>Adds a byte range header to the request for a specified range.</summary>
	/// <param name="from">The position at which to start sending data.</param>
	/// <param name="to">The position at which to stop sending data.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="from" /> is greater than <paramref name="to" />-or- <paramref name="from" /> or <paramref name="to" /> is less than 0. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(long from, long to)
	{
		AddRange("bytes", from, to);
	}

	/// <summary>Adds a Range header to a request for a specific range from the beginning or end of the requested data.</summary>
	/// <param name="rangeSpecifier">The description of the range.</param>
	/// <param name="range">The starting or ending point of the range.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rangeSpecifier" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(string rangeSpecifier, long range)
	{
		if (rangeSpecifier == null)
		{
			throw new ArgumentNullException("rangeSpecifier");
		}
		if (!WebHeaderCollection.IsValidToken(rangeSpecifier))
		{
			throw new ArgumentException("Invalid range specifier", "rangeSpecifier");
		}
		string text = webHeaders["Range"];
		if (text == null)
		{
			text = rangeSpecifier + "=";
		}
		else
		{
			if (string.Compare(text.Substring(0, text.IndexOf('=')), rangeSpecifier, StringComparison.OrdinalIgnoreCase) != 0)
			{
				throw new InvalidOperationException("A different range specifier is already in use");
			}
			text += ",";
		}
		string text2 = range.ToString(CultureInfo.InvariantCulture);
		text = ((range >= 0) ? (text + text2 + "-") : (text + "0" + text2));
		webHeaders.ChangeInternal("Range", text);
	}

	/// <summary>Adds a range header to a request for a specified range.</summary>
	/// <param name="rangeSpecifier">The description of the range.</param>
	/// <param name="from">The position at which to start sending data.</param>
	/// <param name="to">The position at which to stop sending data.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rangeSpecifier" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="from" /> is greater than <paramref name="to" />-or- <paramref name="from" /> or <paramref name="to" /> is less than 0. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rangeSpecifier" /> is invalid. </exception>
	/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
	public void AddRange(string rangeSpecifier, long from, long to)
	{
		if (rangeSpecifier == null)
		{
			throw new ArgumentNullException("rangeSpecifier");
		}
		if (!WebHeaderCollection.IsValidToken(rangeSpecifier))
		{
			throw new ArgumentException("Invalid range specifier", "rangeSpecifier");
		}
		if (from > to || from < 0)
		{
			throw new ArgumentOutOfRangeException("from");
		}
		if (to < 0)
		{
			throw new ArgumentOutOfRangeException("to");
		}
		string text = webHeaders["Range"];
		text = ((text != null) ? (text + ",") : (rangeSpecifier + "="));
		text = $"{text}{from}-{to}";
		webHeaders.ChangeInternal("Range", text);
	}

	/// <summary>Begins an asynchronous request for a <see cref="T:System.IO.Stream" /> object to use to write data.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous request.</returns>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">The state object for this request. </param>
	/// <exception cref="T:System.Net.ProtocolViolationException">The <see cref="P:System.Net.HttpWebRequest.Method" /> property is GET or HEAD.-or- <see cref="P:System.Net.HttpWebRequest.KeepAlive" /> is true, <see cref="P:System.Net.HttpWebRequest.AllowWriteStreamBuffering" /> is false, <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is -1, <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false, and <see cref="P:System.Net.HttpWebRequest.Method" /> is POST or PUT. </exception>
	/// <exception cref="T:System.InvalidOperationException">The stream is being used by a previous call to <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />-or- <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to a value and <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false.-or- The thread pool is running out of threads. </exception>
	/// <exception cref="T:System.NotSupportedException">The request cache validator indicated that the response for this request can be served from the cache; however, requests that write data must not use the cache. This exception can occur if you are using a custom cache validator that is incorrectly implemented. </exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called. </exception>
	/// <exception cref="T:System.ObjectDisposedException">In a .NET Compact Framework application, a request stream with zero content length was not obtained and closed correctly. For more information about handling zero content length requests, see Network Programming in the .NET Compact Framework.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
	{
		if (Aborted)
		{
			throw new WebException("The request was canceled.", WebExceptionStatus.RequestCanceled);
		}
		bool flag = !(method == "GET") && !(method == "CONNECT") && !(method == "HEAD") && !(method == "TRACE");
		if (method == null || !flag)
		{
			throw new ProtocolViolationException("Cannot send data when method is: " + method);
		}
		if (contentLength == -1 && !sendChunked && !allowBuffering && KeepAlive)
		{
			throw new ProtocolViolationException("Content-Length not set");
		}
		string transferEncoding = TransferEncoding;
		if (!sendChunked && transferEncoding != null && transferEncoding.Trim() != "")
		{
			throw new ProtocolViolationException("SendChunked should be true.");
		}
		lock (locker)
		{
			if (getResponseCalled)
			{
				throw new InvalidOperationException("The operation cannot be performed once the request has been submitted.");
			}
			if (asyncWrite != null)
			{
				throw new InvalidOperationException("Cannot re-call start of asynchronous method while a previous call is still in progress.");
			}
			asyncWrite = new WebAsyncResult(this, callback, state);
			initialMethod = method;
			if (haveRequest && writeStream != null)
			{
				asyncWrite.SetCompleted(synch: true, writeStream);
				asyncWrite.DoCallback();
				return asyncWrite;
			}
			gotRequestStream = true;
			WebAsyncResult result = asyncWrite;
			if (!requestSent)
			{
				requestSent = true;
				redirects = 0;
				servicePoint = GetServicePoint();
				abortHandler = servicePoint.SendRequest(this, connectionGroup);
			}
			return result;
		}
	}

	/// <summary>Ends an asynchronous request for a <see cref="T:System.IO.Stream" /> object to use to write data.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> to use to write request data.</returns>
	/// <param name="asyncResult">The pending request for a stream. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.IO.IOException">The request did not complete, and no stream is available. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by the current instance from a call to <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />. </exception>
	/// <exception cref="T:System.InvalidOperationException">This method was called previously using <paramref name="asyncResult" />. </exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- An error occurred while processing the request. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override Stream EndGetRequestStream(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is WebAsyncResult webAsyncResult))
		{
			throw new ArgumentException("Invalid IAsyncResult");
		}
		asyncWrite = webAsyncResult;
		webAsyncResult.WaitUntilComplete();
		Exception exception = webAsyncResult.Exception;
		if (exception != null)
		{
			throw exception;
		}
		return webAsyncResult.WriteStream;
	}

	/// <summary>Gets a <see cref="T:System.IO.Stream" /> object to use to write request data.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> to use to write request data.</returns>
	/// <exception cref="T:System.Net.ProtocolViolationException">The <see cref="P:System.Net.HttpWebRequest.Method" /> property is GET or HEAD.-or- <see cref="P:System.Net.HttpWebRequest.KeepAlive" /> is true, <see cref="P:System.Net.HttpWebRequest.AllowWriteStreamBuffering" /> is false, <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is -1, <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false, and <see cref="P:System.Net.HttpWebRequest.Method" /> is POST or PUT. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="M:System.Net.HttpWebRequest.GetRequestStream" /> method is called more than once.-or- <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to a value and <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false. </exception>
	/// <exception cref="T:System.NotSupportedException">The request cache validator indicated that the response for this request can be served from the cache; however, requests that write data must not use the cache. This exception can occur if you are using a custom cache validator that is incorrectly implemented. </exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- The time-out period for the request expired.-or- An error occurred while processing the request. </exception>
	/// <exception cref="T:System.ObjectDisposedException">In a .NET Compact Framework application, a request stream with zero content length was not obtained and closed correctly. For more information about handling zero content length requests, see Network Programming in the .NET Compact Framework.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override Stream GetRequestStream()
	{
		IAsyncResult asyncResult = asyncWrite;
		if (asyncResult == null)
		{
			asyncResult = BeginGetRequestStream(null, null);
			asyncWrite = (WebAsyncResult)asyncResult;
		}
		if (!asyncResult.IsCompleted && !asyncResult.AsyncWaitHandle.WaitOne(timeout, exitContext: false))
		{
			Abort();
			throw new WebException("The request timed out", WebExceptionStatus.Timeout);
		}
		return EndGetRequestStream(asyncResult);
	}

	/// <summary>Gets a <see cref="T:System.IO.Stream" /> object to use to write request data and outputs the <see cref="T:System.Net.TransportContext" /> associated with the stream.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> to use to write request data.</returns>
	/// <param name="context">The <see cref="T:System.Net.TransportContext" /> for the <see cref="T:System.IO.Stream" />.</param>
	/// <exception cref="T:System.Exception">The <see cref="M:System.Net.HttpWebRequest.GetRequestStream" /> method was unable to obtain the <see cref="T:System.IO.Stream" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="M:System.Net.HttpWebRequest.GetRequestStream" /> method is called more than once.-or- <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to a value and <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false. </exception>
	/// <exception cref="T:System.NotSupportedException">The request cache validator indicated that the response for this request can be served from the cache; however, requests that write data must not use the cache. This exception can occur if you are using a custom cache validator that is incorrectly implemented. </exception>
	/// <exception cref="T:System.Net.ProtocolViolationException">The <see cref="P:System.Net.HttpWebRequest.Method" /> property is GET or HEAD.-or- <see cref="P:System.Net.HttpWebRequest.KeepAlive" /> is true, <see cref="P:System.Net.HttpWebRequest.AllowWriteStreamBuffering" /> is false, <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is -1, <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false, and <see cref="P:System.Net.HttpWebRequest.Method" /> is POST or PUT. </exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- The time-out period for the request expired.-or- An error occurred while processing the request. </exception>
	[System.MonoTODO]
	public Stream GetRequestStream(out TransportContext context)
	{
		throw new NotImplementedException();
	}

	private bool CheckIfForceWrite(SimpleAsyncResult result)
	{
		if (writeStream == null || writeStream.RequestWritten || !InternalAllowBuffering)
		{
			return false;
		}
		if (contentLength < 0 && writeStream.CanWrite && writeStream.WriteBufferLength < 0)
		{
			return false;
		}
		if (contentLength < 0 && writeStream.WriteBufferLength >= 0)
		{
			InternalContentLength = writeStream.WriteBufferLength;
		}
		if (writeStream.WriteBufferLength == contentLength || (contentLength == -1 && !writeStream.CanWrite))
		{
			return writeStream.WriteRequestAsync(result);
		}
		return false;
	}

	/// <summary>Begins an asynchronous request to an Internet resource.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous request for a response.</returns>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate </param>
	/// <param name="state">The state object for this request. </param>
	/// <exception cref="T:System.InvalidOperationException">The stream is already in use by a previous call to <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" />-or- <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to a value and <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false.-or- The thread pool is running out of threads. </exception>
	/// <exception cref="T:System.Net.ProtocolViolationException">
	///   <see cref="P:System.Net.HttpWebRequest.Method" /> is GET or HEAD, and either <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is greater than zero or <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is true.-or- <see cref="P:System.Net.HttpWebRequest.KeepAlive" /> is true, <see cref="P:System.Net.HttpWebRequest.AllowWriteStreamBuffering" /> is false, and either <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is -1, <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false and <see cref="P:System.Net.HttpWebRequest.Method" /> is POST or PUT.-or- The <see cref="T:System.Net.HttpWebRequest" /> has an entity body but the <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> method is called without calling the <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" /> method. -or- The <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is greater than zero, but the application does not write all of the promised data.</exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
	{
		if (Aborted)
		{
			throw new WebException("The request was canceled.", WebExceptionStatus.RequestCanceled);
		}
		if (method == null)
		{
			throw new ProtocolViolationException("Method is null.");
		}
		string transferEncoding = TransferEncoding;
		if (!sendChunked && transferEncoding != null && transferEncoding.Trim() != "")
		{
			throw new ProtocolViolationException("SendChunked should be true.");
		}
		Monitor.Enter(locker);
		getResponseCalled = true;
		if (asyncRead != null && !haveResponse)
		{
			Monitor.Exit(locker);
			throw new InvalidOperationException("Cannot re-call start of asynchronous method while a previous call is still in progress.");
		}
		asyncRead = new WebAsyncResult(this, callback, state);
		WebAsyncResult aread = asyncRead;
		initialMethod = method;
		SimpleAsyncResult.RunWithLock(locker, CheckIfForceWrite, delegate(SimpleAsyncResult inner)
		{
			bool completedSynchronouslyPeek = inner.CompletedSynchronouslyPeek;
			if (inner.GotException)
			{
				aread.SetCompleted(completedSynchronouslyPeek, inner.Exception);
				aread.DoCallback();
			}
			else
			{
				if (haveResponse)
				{
					Exception ex = saved_exc;
					if (webResponse != null)
					{
						if (ex == null)
						{
							aread.SetCompleted(completedSynchronouslyPeek, webResponse);
						}
						else
						{
							aread.SetCompleted(completedSynchronouslyPeek, ex);
						}
						aread.DoCallback();
						return;
					}
					if (ex != null)
					{
						aread.SetCompleted(completedSynchronouslyPeek, ex);
						aread.DoCallback();
						return;
					}
				}
				if (!requestSent)
				{
					try
					{
						requestSent = true;
						redirects = 0;
						servicePoint = GetServicePoint();
						abortHandler = servicePoint.SendRequest(this, connectionGroup);
					}
					catch (Exception e)
					{
						aread.SetCompleted(completedSynchronouslyPeek, e);
						aread.DoCallback();
					}
				}
			}
		});
		return aread;
	}

	/// <summary>Ends an asynchronous request to an Internet resource.</summary>
	/// <returns>A <see cref="T:System.Net.WebResponse" /> that contains the response from the Internet resource.</returns>
	/// <param name="asyncResult">The pending request for a response. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">This method was called previously using <paramref name="asyncResult." />-or- The <see cref="P:System.Net.HttpWebRequest.ContentLength" /> property is greater than 0 but the data has not been written to the request stream. </exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- An error occurred while processing the request. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by the current instance from a call to <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override WebResponse EndGetResponse(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is WebAsyncResult webAsyncResult))
		{
			throw new ArgumentException("Invalid IAsyncResult", "asyncResult");
		}
		if (!webAsyncResult.WaitUntilComplete(timeout, exitContext: false))
		{
			Abort();
			throw new WebException("The request timed out", WebExceptionStatus.Timeout);
		}
		if (webAsyncResult.GotException)
		{
			throw webAsyncResult.Exception;
		}
		return webAsyncResult.Response;
	}

	/// <summary>Ends an asynchronous request for a <see cref="T:System.IO.Stream" /> object to use to write data and outputs the <see cref="T:System.Net.TransportContext" /> associated with the stream.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> to use to write request data.</returns>
	/// <param name="asyncResult">The pending request for a stream.</param>
	/// <param name="context">The <see cref="T:System.Net.TransportContext" /> for the <see cref="T:System.IO.Stream" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by the current instance from a call to <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">This method was called previously using <paramref name="asyncResult" />. </exception>
	/// <exception cref="T:System.IO.IOException">The request did not complete, and no stream is available. </exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- An error occurred while processing the request. </exception>
	public Stream EndGetRequestStream(IAsyncResult asyncResult, out TransportContext context)
	{
		context = null;
		return EndGetRequestStream(asyncResult);
	}

	/// <summary>Returns a response from an Internet resource.</summary>
	/// <returns>A <see cref="T:System.Net.WebResponse" /> that contains the response from the Internet resource.</returns>
	/// <exception cref="T:System.InvalidOperationException">The stream is already in use by a previous call to <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" />.-or- <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to a value and <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false. </exception>
	/// <exception cref="T:System.Net.ProtocolViolationException">
	///   <see cref="P:System.Net.HttpWebRequest.Method" /> is GET or HEAD, and either <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is greater or equal to zero or <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is true.-or- <see cref="P:System.Net.HttpWebRequest.KeepAlive" /> is true, <see cref="P:System.Net.HttpWebRequest.AllowWriteStreamBuffering" /> is false, <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is -1, <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is false, and <see cref="P:System.Net.HttpWebRequest.Method" /> is POST or PUT. -or- The <see cref="T:System.Net.HttpWebRequest" /> has an entity body but the <see cref="M:System.Net.HttpWebRequest.GetResponse" /> method is called without calling the <see cref="M:System.Net.HttpWebRequest.GetRequestStream" /> method. -or- The <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is greater than zero, but the application does not write all of the promised data.</exception>
	/// <exception cref="T:System.NotSupportedException">The request cache validator indicated that the response for this request can be served from the cache; however, this request includes data to be sent to the server. Requests that send data must not use the cache. This exception can occur if you are using a custom cache validator that is incorrectly implemented. </exception>
	/// <exception cref="T:System.Net.WebException">
	///   <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- The time-out period for the request expired.-or- An error occurred while processing the request. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override WebResponse GetResponse()
	{
		WebAsyncResult asyncResult = (WebAsyncResult)BeginGetResponse(null, null);
		return EndGetResponse(asyncResult);
	}

	/// <summary>Cancels a request to an Internet resource.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override void Abort()
	{
		if (Interlocked.CompareExchange(ref aborted, 1, 0) == 1 || (haveResponse && finished_reading))
		{
			return;
		}
		haveResponse = true;
		if (abortHandler != null)
		{
			try
			{
				abortHandler(this, EventArgs.Empty);
			}
			catch (Exception)
			{
			}
			abortHandler = null;
		}
		if (asyncWrite != null)
		{
			WebAsyncResult webAsyncResult = asyncWrite;
			if (!webAsyncResult.IsCompleted)
			{
				try
				{
					WebException e = new WebException("Aborted.", WebExceptionStatus.RequestCanceled);
					webAsyncResult.SetCompleted(synch: false, e);
					webAsyncResult.DoCallback();
				}
				catch
				{
				}
			}
			asyncWrite = null;
		}
		if (asyncRead != null)
		{
			WebAsyncResult webAsyncResult2 = asyncRead;
			if (!webAsyncResult2.IsCompleted)
			{
				try
				{
					WebException e2 = new WebException("Aborted.", WebExceptionStatus.RequestCanceled);
					webAsyncResult2.SetCompleted(synch: false, e2);
					webAsyncResult2.DoCallback();
				}
				catch
				{
				}
			}
			asyncRead = null;
		}
		if (writeStream != null)
		{
			try
			{
				writeStream.Close();
				writeStream = null;
			}
			catch
			{
			}
		}
		if (webResponse == null)
		{
			return;
		}
		try
		{
			webResponse.Close();
			webResponse = null;
		}
		catch
		{
		}
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.</summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that specifies the destination for this serialization.</param>
	void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		GetObjectData(serializationInfo, streamingContext);
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data required to serialize the target object.</summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that specifies the destination for this serialization.</param>
	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		serializationInfo.AddValue("requestUri", requestUri, typeof(Uri));
		serializationInfo.AddValue("actualUri", actualUri, typeof(Uri));
		serializationInfo.AddValue("allowAutoRedirect", allowAutoRedirect);
		serializationInfo.AddValue("allowBuffering", allowBuffering);
		serializationInfo.AddValue("certificates", certificates, typeof(X509CertificateCollection));
		serializationInfo.AddValue("connectionGroup", connectionGroup);
		serializationInfo.AddValue("contentLength", contentLength);
		serializationInfo.AddValue("webHeaders", webHeaders, typeof(WebHeaderCollection));
		serializationInfo.AddValue("keepAlive", keepAlive);
		serializationInfo.AddValue("maxAutoRedirect", maxAutoRedirect);
		serializationInfo.AddValue("mediaType", mediaType);
		serializationInfo.AddValue("method", method);
		serializationInfo.AddValue("initialMethod", initialMethod);
		serializationInfo.AddValue("pipelined", pipelined);
		serializationInfo.AddValue("version", version, typeof(Version));
		serializationInfo.AddValue("proxy", proxy, typeof(IWebProxy));
		serializationInfo.AddValue("sendChunked", sendChunked);
		serializationInfo.AddValue("timeout", timeout);
		serializationInfo.AddValue("redirects", redirects);
		serializationInfo.AddValue("host", host);
	}

	private void CheckRequestStarted()
	{
		if (requestSent)
		{
			throw new InvalidOperationException("request started");
		}
	}

	internal void DoContinueDelegate(int statusCode, WebHeaderCollection headers)
	{
		if (continueDelegate != null)
		{
			continueDelegate(statusCode, headers);
		}
	}

	private void RewriteRedirectToGet()
	{
		method = "GET";
		webHeaders.RemoveInternal("Transfer-Encoding");
		sendChunked = false;
	}

	private bool Redirect(WebAsyncResult result, HttpStatusCode code, WebResponse response)
	{
		redirects++;
		Exception ex = null;
		string text = null;
		switch (code)
		{
		case HttpStatusCode.MultipleChoices:
			ex = new WebException("Ambiguous redirect.");
			break;
		case HttpStatusCode.MovedPermanently:
		case HttpStatusCode.Found:
			if (method == "POST")
			{
				RewriteRedirectToGet();
			}
			break;
		case HttpStatusCode.SeeOther:
			RewriteRedirectToGet();
			break;
		case HttpStatusCode.NotModified:
			return false;
		case HttpStatusCode.UseProxy:
			ex = new NotImplementedException("Proxy support not available.");
			break;
		default:
			ex = new ProtocolViolationException("Invalid status code: " + (int)code);
			break;
		case HttpStatusCode.TemporaryRedirect:
			break;
		}
		if (method != "GET" && !InternalAllowBuffering && (writeStream.WriteBufferLength > 0 || contentLength > 0))
		{
			ex = new WebException("The request requires buffering data to succeed.", null, WebExceptionStatus.ProtocolError, webResponse);
		}
		if (ex != null)
		{
			throw ex;
		}
		if (AllowWriteStreamBuffering || method == "GET")
		{
			contentLength = -1L;
		}
		text = webResponse.Headers["Location"];
		if (text == null)
		{
			throw new WebException("No Location header found for " + (int)code, WebExceptionStatus.ProtocolError);
		}
		Uri uri = actualUri;
		try
		{
			actualUri = new Uri(actualUri, text);
		}
		catch (Exception)
		{
			throw new WebException($"Invalid URL ({text}) for {(int)code}", WebExceptionStatus.ProtocolError);
		}
		hostChanged = actualUri.Scheme != uri.Scheme || Host != uri.Authority;
		return true;
	}

	private string GetHeaders()
	{
		bool flag = false;
		if (sendChunked)
		{
			flag = true;
			webHeaders.ChangeInternal("Transfer-Encoding", "chunked");
			webHeaders.RemoveInternal("Content-Length");
		}
		else if (contentLength != -1)
		{
			if (auth_state.NtlmAuthState == NtlmAuthState.Challenge || proxy_auth_state.NtlmAuthState == NtlmAuthState.Challenge)
			{
				if (haveContentLength || gotRequestStream || contentLength > 0)
				{
					webHeaders.SetInternal("Content-Length", "0");
				}
				else
				{
					webHeaders.RemoveInternal("Content-Length");
				}
			}
			else
			{
				if (contentLength > 0)
				{
					flag = true;
				}
				if (haveContentLength || gotRequestStream || contentLength > 0)
				{
					webHeaders.SetInternal("Content-Length", contentLength.ToString());
				}
			}
			webHeaders.RemoveInternal("Transfer-Encoding");
		}
		else
		{
			webHeaders.RemoveInternal("Content-Length");
		}
		if (actualVersion == HttpVersion.Version11 && flag && servicePoint.SendContinue)
		{
			webHeaders.ChangeInternal("Expect", "100-continue");
			expectContinue = true;
		}
		else
		{
			webHeaders.RemoveInternal("Expect");
			expectContinue = false;
		}
		bool proxyQuery = ProxyQuery;
		string name = (proxyQuery ? "Proxy-Connection" : "Connection");
		webHeaders.RemoveInternal((!proxyQuery) ? "Proxy-Connection" : "Connection");
		Version protocolVersion = servicePoint.ProtocolVersion;
		bool flag2 = protocolVersion == null || protocolVersion == HttpVersion.Version10;
		if (keepAlive && (version == HttpVersion.Version10 || flag2))
		{
			if (webHeaders[name] == null || webHeaders[name].IndexOf("keep-alive", StringComparison.OrdinalIgnoreCase) == -1)
			{
				webHeaders.ChangeInternal(name, "keep-alive");
			}
		}
		else if (!keepAlive && version == HttpVersion.Version11)
		{
			webHeaders.ChangeInternal(name, "close");
		}
		webHeaders.SetInternal("Host", Host);
		if (cookieContainer != null)
		{
			string cookieHeader = cookieContainer.GetCookieHeader(actualUri);
			if (cookieHeader != "")
			{
				webHeaders.ChangeInternal("Cookie", cookieHeader);
			}
			else
			{
				webHeaders.RemoveInternal("Cookie");
			}
		}
		string text = null;
		if ((auto_decomp & DecompressionMethods.GZip) != 0)
		{
			text = "gzip";
		}
		if ((auto_decomp & DecompressionMethods.Deflate) != 0)
		{
			text = ((text != null) ? "gzip, deflate" : "deflate");
		}
		if (text != null)
		{
			webHeaders.ChangeInternal("Accept-Encoding", text);
		}
		if (!usedPreAuth && preAuthenticate)
		{
			DoPreAuthenticate();
		}
		return webHeaders.ToString();
	}

	private void DoPreAuthenticate()
	{
		bool flag = proxy != null && !proxy.IsBypassed(actualUri);
		ICredentials credentials = ((!flag || this.credentials != null) ? this.credentials : proxy.Credentials);
		Authorization authorization = AuthenticationManager.PreAuthenticate(this, credentials);
		if (authorization != null)
		{
			webHeaders.RemoveInternal("Proxy-Authorization");
			webHeaders.RemoveInternal("Authorization");
			string name = ((flag && this.credentials == null) ? "Proxy-Authorization" : "Authorization");
			webHeaders[name] = authorization.Message;
			usedPreAuth = true;
		}
	}

	internal void SetWriteStreamError(WebExceptionStatus status, Exception exc)
	{
		if (Aborted)
		{
			return;
		}
		WebAsyncResult webAsyncResult = asyncWrite;
		if (webAsyncResult == null)
		{
			webAsyncResult = asyncRead;
		}
		if (webAsyncResult == null)
		{
			return;
		}
		WebException ex;
		if (exc == null)
		{
			ex = new WebException("Error: " + status, status);
		}
		else
		{
			ex = exc as WebException;
			if (ex == null)
			{
				ex = new WebException($"Error: {status} ({exc.Message})", status, WebExceptionInternalStatus.RequestFatal, exc);
			}
		}
		webAsyncResult.SetCompleted(synch: false, ex);
		webAsyncResult.DoCallback();
	}

	internal byte[] GetRequestHeaders()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (ProxyQuery ? $"{actualUri.Scheme}://{Host}{actualUri.PathAndQuery}" : actualUri.PathAndQuery);
		if (!force_version && servicePoint.ProtocolVersion != null && servicePoint.ProtocolVersion < version)
		{
			actualVersion = servicePoint.ProtocolVersion;
		}
		else
		{
			actualVersion = version;
		}
		stringBuilder.AppendFormat("{0} {1} HTTP/{2}.{3}\r\n", method, text, actualVersion.Major, actualVersion.Minor);
		stringBuilder.Append(GetHeaders());
		string s = stringBuilder.ToString();
		return Encoding.UTF8.GetBytes(s);
	}

	internal void SetWriteStream(WebConnectionStream stream)
	{
		if (Aborted)
		{
			return;
		}
		writeStream = stream;
		if (bodyBuffer != null)
		{
			webHeaders.RemoveInternal("Transfer-Encoding");
			contentLength = bodyBufferLength;
			writeStream.SendChunked = false;
		}
		writeStream.SetHeadersAsync(setInternalLength: false, delegate(SimpleAsyncResult result)
		{
			if (result.GotException)
			{
				SetWriteStreamError(result.Exception);
			}
			else
			{
				haveRequest = true;
				SetWriteStreamInner(delegate(SimpleAsyncResult inner)
				{
					if (inner.GotException)
					{
						SetWriteStreamError(inner.Exception);
					}
					else if (asyncWrite != null)
					{
						asyncWrite.SetCompleted(inner.CompletedSynchronouslyPeek, writeStream);
						asyncWrite.DoCallback();
						asyncWrite = null;
					}
				});
			}
		});
	}

	private void SetWriteStreamInner(SimpleAsyncCallback callback)
	{
		SimpleAsyncResult.Run(delegate(SimpleAsyncResult result)
		{
			if (bodyBuffer != null)
			{
				if (auth_state.NtlmAuthState != NtlmAuthState.Challenge && proxy_auth_state.NtlmAuthState != NtlmAuthState.Challenge)
				{
					writeStream.Write(bodyBuffer, 0, bodyBufferLength);
					bodyBuffer = null;
					writeStream.Close();
				}
			}
			else if (MethodWithBuffer && getResponseCalled && !writeStream.RequestWritten)
			{
				return writeStream.WriteRequestAsync(result);
			}
			return false;
		}, callback);
	}

	private void SetWriteStreamError(Exception exc)
	{
		if (exc is WebException ex)
		{
			SetWriteStreamError(ex.Status, ex);
		}
		else
		{
			SetWriteStreamError(WebExceptionStatus.SendFailure, exc);
		}
	}

	internal void SetResponseError(WebExceptionStatus status, Exception e, string where)
	{
		if (Aborted)
		{
			return;
		}
		lock (locker)
		{
			string message = $"Error getting response stream ({where}): {status}";
			WebAsyncResult webAsyncResult = asyncRead;
			if (webAsyncResult == null)
			{
				webAsyncResult = asyncWrite;
			}
			WebException e2 = ((!(e is WebException)) ? new WebException(message, e, status, null) : ((WebException)e));
			if (webAsyncResult != null)
			{
				if (!webAsyncResult.IsCompleted)
				{
					webAsyncResult.SetCompleted(synch: false, e2);
					webAsyncResult.DoCallback();
				}
				else if (webAsyncResult == asyncWrite)
				{
					saved_exc = e2;
				}
				haveResponse = true;
				asyncRead = null;
				asyncWrite = null;
			}
			else
			{
				haveResponse = true;
				saved_exc = e2;
			}
		}
	}

	private void CheckSendError(WebConnectionData data)
	{
		int statusCode = data.StatusCode;
		if (statusCode >= 400 && statusCode != 401 && statusCode != 407 && writeStream != null && asyncRead == null && !writeStream.CompleteRequestWritten)
		{
			saved_exc = new WebException(data.StatusDescription, null, WebExceptionStatus.ProtocolError, webResponse);
			if (allowBuffering || sendChunked || writeStream.totalWritten >= contentLength)
			{
				webResponse.ReadAll();
			}
			else
			{
				writeStream.IgnoreIOErrors = true;
			}
		}
	}

	private bool HandleNtlmAuth(WebAsyncResult r)
	{
		bool flag = webResponse.StatusCode == HttpStatusCode.ProxyAuthenticationRequired;
		if ((flag ? proxy_auth_state.NtlmAuthState : auth_state.NtlmAuthState) == NtlmAuthState.None)
		{
			return false;
		}
		if (webResponse.GetResponseStream() is WebConnectionStream { Connection: var connection })
		{
			connection.PriorityRequest = this;
			ICredentials credentials = ((!flag || proxy == null) ? this.credentials : proxy.Credentials);
			if (credentials != null)
			{
				connection.NtlmCredential = credentials.GetCredential(requestUri, "NTLM");
				connection.UnsafeAuthenticatedConnectionSharing = unsafe_auth_blah;
			}
		}
		r.Reset();
		finished_reading = false;
		haveResponse = false;
		webResponse.ReadAll();
		webResponse = null;
		return true;
	}

	internal void SetResponseData(WebConnectionData data)
	{
		lock (locker)
		{
			if (Aborted)
			{
				if (data.stream != null)
				{
					data.stream.Close();
				}
				return;
			}
			WebException ex = null;
			try
			{
				webResponse = new HttpWebResponse(actualUri, method, data, cookieContainer);
			}
			catch (Exception ex2)
			{
				ex = new WebException(ex2.Message, ex2, WebExceptionStatus.ProtocolError, null);
				if (data.stream != null)
				{
					data.stream.Close();
				}
			}
			if (ex == null && (method == "POST" || method == "PUT"))
			{
				CheckSendError(data);
				if (saved_exc != null)
				{
					ex = (WebException)saved_exc;
				}
			}
			WebAsyncResult webAsyncResult = asyncRead;
			bool flag = false;
			if (webAsyncResult == null && webResponse != null)
			{
				flag = true;
				webAsyncResult = new WebAsyncResult(null, null);
				webAsyncResult.SetCompleted(synch: false, webResponse);
			}
			if (webAsyncResult == null)
			{
				return;
			}
			if (ex != null)
			{
				haveResponse = true;
				if (!webAsyncResult.IsCompleted)
				{
					webAsyncResult.SetCompleted(synch: false, ex);
				}
				webAsyncResult.DoCallback();
				return;
			}
			bool flag2 = ProxyQuery && proxy != null && !proxy.IsBypassed(actualUri);
			try
			{
				if (!CheckFinalStatus(webAsyncResult))
				{
					if ((flag2 ? proxy_auth_state.IsNtlmAuthenticated : auth_state.IsNtlmAuthenticated) && webResponse != null && webResponse.StatusCode < HttpStatusCode.BadRequest && webResponse.GetResponseStream() is WebConnectionStream webConnectionStream)
					{
						webConnectionStream.Connection.NtlmAuthenticated = true;
					}
					if (writeStream != null)
					{
						writeStream.KillBuffer();
					}
					haveResponse = true;
					webAsyncResult.SetCompleted(synch: false, webResponse);
					webAsyncResult.DoCallback();
					return;
				}
				if (sendChunked)
				{
					sendChunked = false;
					webHeaders.RemoveInternal("Transfer-Encoding");
				}
				if (webResponse != null)
				{
					if (HandleNtlmAuth(webAsyncResult))
					{
						return;
					}
					webResponse.Close();
				}
				finished_reading = false;
				haveResponse = false;
				webResponse = null;
				webAsyncResult.Reset();
				servicePoint = GetServicePoint();
				abortHandler = servicePoint.SendRequest(this, connectionGroup);
			}
			catch (WebException e)
			{
				if (flag)
				{
					saved_exc = e;
					haveResponse = true;
				}
				webAsyncResult.SetCompleted(synch: false, e);
				webAsyncResult.DoCallback();
			}
			catch (Exception ex3)
			{
				ex = new WebException(ex3.Message, ex3, WebExceptionStatus.ProtocolError, null);
				if (flag)
				{
					saved_exc = ex;
					haveResponse = true;
				}
				webAsyncResult.SetCompleted(synch: false, ex);
				webAsyncResult.DoCallback();
			}
		}
	}

	private bool CheckAuthorization(WebResponse response, HttpStatusCode code)
	{
		if (code != HttpStatusCode.ProxyAuthenticationRequired)
		{
			return auth_state.CheckAuthorization(response, code);
		}
		return proxy_auth_state.CheckAuthorization(response, code);
	}

	private bool CheckFinalStatus(WebAsyncResult result)
	{
		if (result.GotException)
		{
			bodyBuffer = null;
			throw result.Exception;
		}
		Exception ex = result.Exception;
		HttpWebResponse response = result.Response;
		WebExceptionStatus status = WebExceptionStatus.ProtocolError;
		HttpStatusCode httpStatusCode = (HttpStatusCode)0;
		if (ex == null && webResponse != null)
		{
			httpStatusCode = webResponse.StatusCode;
			if (((!auth_state.IsCompleted && httpStatusCode == HttpStatusCode.Unauthorized && credentials != null) || (ProxyQuery && !proxy_auth_state.IsCompleted && httpStatusCode == HttpStatusCode.ProxyAuthenticationRequired)) && !usedPreAuth && CheckAuthorization(webResponse, httpStatusCode))
			{
				if (MethodWithBuffer)
				{
					if (AllowWriteStreamBuffering)
					{
						if (writeStream.WriteBufferLength > 0)
						{
							bodyBuffer = writeStream.WriteBuffer;
							bodyBufferLength = writeStream.WriteBufferLength;
						}
						return true;
					}
					if (ResendContentFactory != null)
					{
						using (MemoryStream memoryStream = new MemoryStream())
						{
							ResendContentFactory(memoryStream);
							bodyBuffer = memoryStream.ToArray();
							bodyBufferLength = bodyBuffer.Length;
						}
						return true;
					}
				}
				else if (method != "PUT" && method != "POST")
				{
					bodyBuffer = null;
					return true;
				}
				if (!ThrowOnError)
				{
					return false;
				}
				writeStream.InternalClose();
				writeStream = null;
				webResponse.Close();
				webResponse = null;
				bodyBuffer = null;
				throw new WebException("This request requires buffering of data for authentication or redirection to be sucessful.");
			}
			bodyBuffer = null;
			if (httpStatusCode >= HttpStatusCode.BadRequest)
			{
				ex = new WebException($"The remote server returned an error: ({(int)httpStatusCode}) {webResponse.StatusDescription}.", null, status, webResponse);
				webResponse.ReadAll();
			}
			else if (httpStatusCode == HttpStatusCode.NotModified && allowAutoRedirect)
			{
				ex = new WebException($"The remote server returned an error: ({(int)httpStatusCode}) {webResponse.StatusDescription}.", null, status, webResponse);
			}
			else if (httpStatusCode >= HttpStatusCode.MultipleChoices && allowAutoRedirect && redirects >= maxAutoRedirect)
			{
				ex = new WebException("Max. redirections exceeded.", null, status, webResponse);
				webResponse.ReadAll();
			}
		}
		bodyBuffer = null;
		if (ex == null)
		{
			bool flag = false;
			int num = (int)httpStatusCode;
			if (allowAutoRedirect && num >= 300)
			{
				flag = Redirect(result, httpStatusCode, webResponse);
				if (InternalAllowBuffering && writeStream.WriteBufferLength > 0)
				{
					bodyBuffer = writeStream.WriteBuffer;
					bodyBufferLength = writeStream.WriteBufferLength;
				}
				if (flag && !unsafe_auth_blah)
				{
					auth_state.Reset();
					proxy_auth_state.Reset();
				}
			}
			if (response != null && num >= 300 && num != 304)
			{
				response.ReadAll();
			}
			return flag;
		}
		if (!ThrowOnError)
		{
			return false;
		}
		if (writeStream != null)
		{
			writeStream.InternalClose();
			writeStream = null;
		}
		webResponse = null;
		throw ex;
	}

	internal static StringBuilder GenerateConnectionGroup(string connectionGroupName, bool unsafeConnectionGroup, bool isInternalGroup)
	{
		StringBuilder stringBuilder = new StringBuilder(connectionGroupName);
		stringBuilder.Append(unsafeConnectionGroup ? "U>" : "S>");
		if (isInternalGroup)
		{
			stringBuilder.Append("I>");
		}
		return stringBuilder;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.HttpWebRequest" /> class.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	public HttpWebRequest()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
