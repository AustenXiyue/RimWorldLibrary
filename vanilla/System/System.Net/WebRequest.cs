using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Net.Cache;
using System.Net.Configuration;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net;

/// <summary>Makes a request to a Uniform Resource Identifier (URI). This is an abstract class.</summary>
[Serializable]
public abstract class WebRequest : MarshalByRefObject, ISerializable
{
	internal class DesignerWebRequestCreate : IWebRequestCreate
	{
		public WebRequest Create(Uri uri)
		{
			return WebRequest.Create(uri);
		}
	}

	internal class WebProxyWrapperOpaque : IAutoWebProxy, IWebProxy
	{
		protected readonly WebProxy webProxy;

		public ICredentials Credentials
		{
			get
			{
				return webProxy.Credentials;
			}
			set
			{
				webProxy.Credentials = value;
			}
		}

		internal WebProxyWrapperOpaque(WebProxy webProxy)
		{
			this.webProxy = webProxy;
		}

		public Uri GetProxy(Uri destination)
		{
			return webProxy.GetProxy(destination);
		}

		public bool IsBypassed(Uri host)
		{
			return webProxy.IsBypassed(host);
		}

		public ProxyChain GetProxies(Uri destination)
		{
			return ((IAutoWebProxy)webProxy).GetProxies(destination);
		}
	}

	internal class WebProxyWrapper : WebProxyWrapperOpaque
	{
		internal WebProxy WebProxy => webProxy;

		internal WebProxyWrapper(WebProxy webProxy)
			: base(webProxy)
		{
		}
	}

	internal const int DefaultTimeout = 100000;

	private static volatile ArrayList s_PrefixList;

	private static object s_InternalSyncObject;

	private static TimerThread.Queue s_DefaultTimerQueue = TimerThread.CreateQueue(100000);

	private AuthenticationLevel m_AuthenticationLevel;

	private TokenImpersonationLevel m_ImpersonationLevel;

	private RequestCachePolicy m_CachePolicy;

	private RequestCacheProtocol m_CacheProtocol;

	private RequestCacheBinding m_CacheBinding;

	private static DesignerWebRequestCreate webRequestCreate = new DesignerWebRequestCreate();

	private static volatile IWebProxy s_DefaultWebProxy;

	private static volatile bool s_DefaultWebProxyInitialized;

	/// <summary>When overridden in a descendant class, gets the factory object derived from the <see cref="T:System.Net.IWebRequestCreate" /> class used to create the <see cref="T:System.Net.WebRequest" /> instantiated for making the request to the specified URI.</summary>
	/// <returns>Returns <see cref="T:System.Net.IWebRequestCreate" />.The derived <see cref="T:System.Net.WebRequest" /> type returned by the <see cref="M:System.Net.IWebRequestCreate.Create(System.Uri)" /> method.</returns>
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual IWebRequestCreate CreatorInstance => webRequestCreate;

	private static object InternalSyncObject
	{
		get
		{
			if (s_InternalSyncObject == null)
			{
				object value = new object();
				Interlocked.CompareExchange(ref s_InternalSyncObject, value, null);
			}
			return s_InternalSyncObject;
		}
	}

	internal static TimerThread.Queue DefaultTimerQueue => s_DefaultTimerQueue;

	internal static ArrayList PrefixList
	{
		get
		{
			if (s_PrefixList == null)
			{
				lock (InternalSyncObject)
				{
					if (s_PrefixList == null)
					{
						s_PrefixList = PopulatePrefixList();
					}
				}
			}
			return s_PrefixList;
		}
		set
		{
			s_PrefixList = value;
		}
	}

	/// <summary>Gets or sets the default cache policy for this request.</summary>
	/// <returns>A <see cref="T:System.Net.Cache.HttpRequestCachePolicy" /> that specifies the cache policy in effect for this request when no other policy is applicable.</returns>
	public static RequestCachePolicy DefaultCachePolicy
	{
		get
		{
			return RequestCacheManager.GetBinding(string.Empty).Policy;
		}
		set
		{
			RequestCacheBinding binding = RequestCacheManager.GetBinding(string.Empty);
			RequestCacheManager.SetBinding(string.Empty, new RequestCacheBinding(binding.Cache, binding.Validator, value));
		}
	}

	/// <summary>Gets or sets the cache policy for this request.</summary>
	/// <returns>A <see cref="T:System.Net.Cache.RequestCachePolicy" /> object that defines a cache policy.</returns>
	public virtual RequestCachePolicy CachePolicy
	{
		get
		{
			return m_CachePolicy;
		}
		set
		{
			InternalSetCachePolicy(value);
		}
	}

	/// <summary>When overridden in a descendant class, gets or sets the protocol method to use in this request.</summary>
	/// <returns>The protocol method to use in this request.</returns>
	/// <exception cref="T:System.NotImplementedException">If the property is not overridden in a descendant class, any attempt is made to get or set the property. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual string Method
	{
		get
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
	}

	/// <summary>When overridden in a descendant class, gets the URI of the Internet resource associated with the request.</summary>
	/// <returns>A <see cref="T:System.Uri" /> representing the resource associated with the request </returns>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to get or set the property, when the property is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual Uri RequestUri
	{
		get
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
	}

	/// <summary>When overridden in a descendant class, gets or sets the name of the connection group for the request.</summary>
	/// <returns>The name of the connection group for the request.</returns>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to get or set the property, when the property is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual string ConnectionGroupName
	{
		get
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
	}

	/// <summary>When overridden in a descendant class, gets or sets the collection of header name/value pairs associated with the request.</summary>
	/// <returns>A <see cref="T:System.Net.WebHeaderCollection" /> containing the header name/value pairs associated with this request.</returns>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to get or set the property, when the property is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual WebHeaderCollection Headers
	{
		get
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
	}

	/// <summary>When overridden in a descendant class, gets or sets the content length of the request data being sent.</summary>
	/// <returns>The number of bytes of request data being sent.</returns>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to get or set the property, when the property is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual long ContentLength
	{
		get
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
	}

	/// <summary>When overridden in a descendant class, gets or sets the content type of the request data being sent.</summary>
	/// <returns>The content type of the request data.</returns>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to get or set the property, when the property is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual string ContentType
	{
		get
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
	}

	/// <summary>When overridden in a descendant class, gets or sets the network credentials used for authenticating the request with the Internet resource.</summary>
	/// <returns>An <see cref="T:System.Net.ICredentials" /> containing the authentication credentials associated with the request. The default is null.</returns>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to get or set the property, when the property is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual ICredentials Credentials
	{
		get
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
	}

	/// <summary>When overridden in a descendant class, gets or sets a <see cref="T:System.Boolean" /> value that controls whether <see cref="P:System.Net.CredentialCache.DefaultCredentials" /> are sent with requests.</summary>
	/// <returns>true if the default credentials are used; otherwise false. The default value is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">You attempted to set this property after the request was sent.</exception>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to access the property, when the property is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual bool UseDefaultCredentials
	{
		get
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
	}

	/// <summary>When overridden in a descendant class, gets or sets the network proxy to use to access this Internet resource.</summary>
	/// <returns>The <see cref="T:System.Net.IWebProxy" /> to use to access the Internet resource.</returns>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to get or set the property, when the property is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual IWebProxy Proxy
	{
		get
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
	}

	/// <summary>When overridden in a descendant class, indicates whether to pre-authenticate the request.</summary>
	/// <returns>true to pre-authenticate; otherwise, false.</returns>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to get or set the property, when the property is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual bool PreAuthenticate
	{
		get
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
	}

	/// <summary>Gets or sets the length of time, in milliseconds, before the request times out.</summary>
	/// <returns>The length of time, in milliseconds, until the request times out, or the value <see cref="F:System.Threading.Timeout.Infinite" /> to indicate that the request does not time out. The default value is defined by the descendant class.</returns>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to get or set the property, when the property is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual int Timeout
	{
		get
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
	}

	internal RequestCacheProtocol CacheProtocol
	{
		get
		{
			return m_CacheProtocol;
		}
		set
		{
			m_CacheProtocol = value;
		}
	}

	/// <summary>Gets or sets values indicating the level of authentication and impersonation used for this request.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.Net.Security.AuthenticationLevel" /> values. The default value is <see cref="F:System.Net.Security.AuthenticationLevel.MutualAuthRequested" />.In mutual authentication, both the client and server present credentials to establish their identity. The <see cref="F:System.Net.Security.AuthenticationLevel.MutualAuthRequired" /> and <see cref="F:System.Net.Security.AuthenticationLevel.MutualAuthRequested" /> values are relevant for Kerberos authentication. Kerberos authentication can be supported directly, or can be used if the Negotiate security protocol is used to select the actual security protocol. For more information about authentication protocols, see Internet Authentication.To determine whether mutual authentication occurred, check the <see cref="P:System.Net.WebResponse.IsMutuallyAuthenticated" /> property. If you specify the <see cref="F:System.Net.Security.AuthenticationLevel.MutualAuthRequired" /> authentication flag value and mutual authentication does not occur, your application will receive an <see cref="T:System.IO.IOException" /> with a <see cref="T:System.Net.ProtocolViolationException" /> inner exception indicating that mutual authentication failed.</returns>
	public AuthenticationLevel AuthenticationLevel
	{
		get
		{
			return m_AuthenticationLevel;
		}
		set
		{
			m_AuthenticationLevel = value;
		}
	}

	/// <summary>Gets or sets the impersonation level for the current request.</summary>
	/// <returns>A <see cref="T:System.Security.Principal.TokenImpersonationLevel" /> value.</returns>
	public TokenImpersonationLevel ImpersonationLevel
	{
		get
		{
			return m_ImpersonationLevel;
		}
		set
		{
			m_ImpersonationLevel = value;
		}
	}

	internal static IWebProxy InternalDefaultWebProxy
	{
		get
		{
			if (!s_DefaultWebProxyInitialized)
			{
				lock (InternalSyncObject)
				{
					if (!s_DefaultWebProxyInitialized)
					{
						DefaultProxySectionInternal section = DefaultProxySectionInternal.GetSection();
						if (section != null)
						{
							s_DefaultWebProxy = section.WebProxy;
						}
						s_DefaultWebProxyInitialized = true;
					}
				}
			}
			return s_DefaultWebProxy;
		}
		set
		{
			if (!s_DefaultWebProxyInitialized)
			{
				lock (InternalSyncObject)
				{
					s_DefaultWebProxy = value;
					s_DefaultWebProxyInitialized = true;
					return;
				}
			}
			s_DefaultWebProxy = value;
		}
	}

	/// <summary>Gets or sets the global HTTP proxy.</summary>
	/// <returns>An <see cref="T:System.Net.IWebProxy" /> used by every call to instances of <see cref="T:System.Net.WebRequest" />.</returns>
	public static IWebProxy DefaultWebProxy
	{
		get
		{
			return InternalDefaultWebProxy;
		}
		set
		{
			InternalDefaultWebProxy = value;
		}
	}

	/// <summary>Register an <see cref="T:System.Net.IWebRequestCreate" /> object. </summary>
	/// <param name="creator">The <see cref="T:System.Net.IWebRequestCreate" /> object to register.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	public static void RegisterPortableWebRequestCreator(IWebRequestCreate creator)
	{
	}

	private static WebRequest Create(Uri requestUri, bool useUriBase)
	{
		_ = Logging.On;
		WebRequestPrefixElement webRequestPrefixElement = null;
		bool flag = false;
		string text = (useUriBase ? (requestUri.Scheme + ":") : requestUri.AbsoluteUri);
		int length = text.Length;
		ArrayList prefixList = PrefixList;
		for (int i = 0; i < prefixList.Count; i++)
		{
			webRequestPrefixElement = (WebRequestPrefixElement)prefixList[i];
			if (length >= webRequestPrefixElement.Prefix.Length && string.Compare(webRequestPrefixElement.Prefix, 0, text, 0, webRequestPrefixElement.Prefix.Length, StringComparison.OrdinalIgnoreCase) == 0)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			WebRequest result = webRequestPrefixElement.Creator.Create(requestUri);
			_ = Logging.On;
			return result;
		}
		_ = Logging.On;
		throw new NotSupportedException(global::SR.GetString("The URI prefix is not recognized."));
	}

	/// <summary>Initializes a new <see cref="T:System.Net.WebRequest" /> instance for the specified URI scheme.</summary>
	/// <returns>A <see cref="T:System.Net.WebRequest" /> descendant for the specific URI scheme.</returns>
	/// <param name="requestUriString">The URI that identifies the Internet resource. </param>
	/// <exception cref="T:System.NotSupportedException">The request scheme specified in <paramref name="requestUriString" /> has not been registered. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="requestUriString" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have permission to connect to the requested URI or a URI that the request is redirected to. </exception>
	/// <exception cref="T:System.UriFormatException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead.The URI specified in <paramref name="requestUriString" /> is not a valid URI. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static WebRequest Create(string requestUriString)
	{
		if (requestUriString == null)
		{
			throw new ArgumentNullException("requestUriString");
		}
		return Create(new Uri(requestUriString), useUriBase: false);
	}

	/// <summary>Initializes a new <see cref="T:System.Net.WebRequest" /> instance for the specified URI scheme.</summary>
	/// <returns>A <see cref="T:System.Net.WebRequest" /> descendant for the specified URI scheme.</returns>
	/// <param name="requestUri">A <see cref="T:System.Uri" /> containing the URI of the requested resource. </param>
	/// <exception cref="T:System.NotSupportedException">The request scheme specified in <paramref name="requestUri" /> is not registered. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="requestUri" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have permission to connect to the requested URI or a URI that the request is redirected to. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static WebRequest Create(Uri requestUri)
	{
		if (requestUri == null)
		{
			throw new ArgumentNullException("requestUri");
		}
		return Create(requestUri, useUriBase: false);
	}

	/// <summary>Initializes a new <see cref="T:System.Net.WebRequest" /> instance for the specified URI scheme.</summary>
	/// <returns>A <see cref="T:System.Net.WebRequest" /> descendant for the specified URI scheme.</returns>
	/// <param name="requestUri">A <see cref="T:System.Uri" /> containing the URI of the requested resource. </param>
	/// <exception cref="T:System.NotSupportedException">The request scheme specified in <paramref name="requestUri" /> is not registered. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="requestUri" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have permission to connect to the requested URI or a URI that the request is redirected to. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static WebRequest CreateDefault(Uri requestUri)
	{
		if (requestUri == null)
		{
			throw new ArgumentNullException("requestUri");
		}
		return Create(requestUri, useUriBase: true);
	}

	/// <summary>Initializes a new <see cref="T:System.Net.HttpWebRequest" /> instance for the specified URI string.</summary>
	/// <returns>Returns <see cref="T:System.Net.HttpWebRequest" />.An <see cref="T:System.Net.HttpWebRequest" />  instance for the specific URI string.</returns>
	/// <param name="requestUriString">A URI string that identifies the Internet resource. </param>
	/// <exception cref="T:System.NotSupportedException">The request scheme specified in <paramref name="requestUriString" /> is the http or https scheme. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="requestUriString" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have permission to connect to the requested URI or a URI that the request is redirected to. </exception>
	/// <exception cref="T:System.UriFormatException">The URI specified in <paramref name="requestUriString" /> is not a valid URI. </exception>
	public static HttpWebRequest CreateHttp(string requestUriString)
	{
		if (requestUriString == null)
		{
			throw new ArgumentNullException("requestUriString");
		}
		return CreateHttp(new Uri(requestUriString));
	}

	/// <summary>Initializes a new <see cref="T:System.Net.HttpWebRequest" /> instance for the specified URI.</summary>
	/// <returns>Returns <see cref="T:System.Net.HttpWebRequest" />.An <see cref="T:System.Net.HttpWebRequest" /> instance for the specific URI string.</returns>
	/// <param name="requestUri">A URI that identifies the Internet resource.</param>
	/// <exception cref="T:System.NotSupportedException">The request scheme specified in <paramref name="requestUri" /> is the http or https scheme. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="requestUri" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have permission to connect to the requested URI or a URI that the request is redirected to. </exception>
	/// <exception cref="T:System.UriFormatException">The URI specified in <paramref name="requestUri" /> is not a valid URI. </exception>
	public static HttpWebRequest CreateHttp(Uri requestUri)
	{
		if (requestUri == null)
		{
			throw new ArgumentNullException("requestUri");
		}
		if (requestUri.Scheme != Uri.UriSchemeHttp && requestUri.Scheme != Uri.UriSchemeHttps)
		{
			throw new NotSupportedException(global::SR.GetString("The URI prefix is not recognized."));
		}
		return (HttpWebRequest)CreateDefault(requestUri);
	}

	/// <summary>Registers a <see cref="T:System.Net.WebRequest" /> descendant for the specified URI.</summary>
	/// <returns>true if registration is successful; otherwise, false.</returns>
	/// <param name="prefix">The complete URI or URI prefix that the <see cref="T:System.Net.WebRequest" /> descendant services. </param>
	/// <param name="creator">The create method that the <see cref="T:System.Net.WebRequest" /> calls to create the <see cref="T:System.Net.WebRequest" /> descendant. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="prefix" /> is null-or- <paramref name="creator" /> is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static bool RegisterPrefix(string prefix, IWebRequestCreate creator)
	{
		bool flag = false;
		if (prefix == null)
		{
			throw new ArgumentNullException("prefix");
		}
		if (creator == null)
		{
			throw new ArgumentNullException("creator");
		}
		lock (InternalSyncObject)
		{
			ArrayList arrayList = (ArrayList)PrefixList.Clone();
			if (Uri.TryCreate(prefix, UriKind.Absolute, out var result))
			{
				string text = result.AbsoluteUri;
				if (!prefix.EndsWith("/", StringComparison.Ordinal) && result.GetComponents(UriComponents.PathAndQuery | UriComponents.Fragment, UriFormat.UriEscaped).Equals("/"))
				{
					text = text.Substring(0, text.Length - 1);
				}
				prefix = text;
			}
			int i;
			for (i = 0; i < arrayList.Count; i++)
			{
				WebRequestPrefixElement webRequestPrefixElement = (WebRequestPrefixElement)arrayList[i];
				if (prefix.Length > webRequestPrefixElement.Prefix.Length)
				{
					break;
				}
				if (prefix.Length == webRequestPrefixElement.Prefix.Length && string.Compare(webRequestPrefixElement.Prefix, prefix, StringComparison.OrdinalIgnoreCase) == 0)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				arrayList.Insert(i, new WebRequestPrefixElement(prefix, creator));
				PrefixList = arrayList;
			}
		}
		return !flag;
	}

	private static ArrayList PopulatePrefixList()
	{
		ArrayList arrayList = new ArrayList();
		if (ConfigurationManager.GetSection("system.net/webRequestModules") is WebRequestModulesSection webRequestModulesSection)
		{
			foreach (WebRequestModuleElement webRequestModule in webRequestModulesSection.WebRequestModules)
			{
				arrayList.Add(new WebRequestPrefixElement(webRequestModule.Prefix, webRequestModule.Type));
			}
		}
		return arrayList;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebRequest" /> class.</summary>
	protected WebRequest()
	{
		m_ImpersonationLevel = TokenImpersonationLevel.Delegation;
		m_AuthenticationLevel = AuthenticationLevel.MutualAuthRequested;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebRequest" /> class from the specified instances of the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> classes.</summary>
	/// <param name="serializationInfo">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that contains the information required to serialize the new <see cref="T:System.Net.WebRequest" /> instance. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that indicates the source of the serialized stream associated with the new <see cref="T:System.Net.WebRequest" /> instance. </param>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to access the constructor, when the constructor is not overridden in a descendant class. </exception>
	protected WebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
	}

	/// <summary>When overridden in a descendant class, populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> instance with the data needed to serialize the <see cref="T:System.Net.WebRequest" />.</summary>
	/// <param name="serializationInfo">A <see cref="T:System.Runtime.Serialization.SerializationInfo" />, which holds the serialized data for the <see cref="T:System.Net.WebRequest" />. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains the destination of the serialized stream associated with the new <see cref="T:System.Net.WebRequest" />. </param>
	/// <exception cref="T:System.NotImplementedException">An attempt is made to serialize the object, when the interface is not overridden in a descendant class. </exception>
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter, SerializationFormatter = true)]
	void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		GetObjectData(serializationInfo, streamingContext);
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.</summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that specifies the destination for this serialization.</param>
	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	protected virtual void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
	}

	private void InternalSetCachePolicy(RequestCachePolicy policy)
	{
		if (m_CacheBinding != null && m_CacheBinding.Cache != null && m_CacheBinding.Validator != null && CacheProtocol == null && policy != null && policy.Level != RequestCacheLevel.BypassCache)
		{
			CacheProtocol = new RequestCacheProtocol(m_CacheBinding.Cache, m_CacheBinding.Validator.CreateValidator());
		}
		m_CachePolicy = policy;
	}

	/// <summary>When overridden in a descendant class, returns a <see cref="T:System.IO.Stream" /> for writing data to the Internet resource.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> for writing data to the Internet resource.</returns>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to access the method, when the method is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual Stream GetRequestStream()
	{
		throw ExceptionHelper.MethodNotImplementedException;
	}

	/// <summary>When overridden in a descendant class, returns a response to an Internet request.</summary>
	/// <returns>A <see cref="T:System.Net.WebResponse" /> containing the response to the Internet request.</returns>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to access the method, when the method is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual WebResponse GetResponse()
	{
		throw ExceptionHelper.MethodNotImplementedException;
	}

	/// <summary>When overridden in a descendant class, begins an asynchronous request for an Internet resource.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous request.</returns>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object containing state information for this asynchronous request. </param>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to access the method, when the method is not overridden in a descendant class. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
	{
		throw ExceptionHelper.MethodNotImplementedException;
	}

	/// <summary>When overridden in a descendant class, returns a <see cref="T:System.Net.WebResponse" />.</summary>
	/// <returns>A <see cref="T:System.Net.WebResponse" /> that contains a response to the Internet request.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> that references a pending request for a response. </param>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to access the method, when the method is not overridden in a descendant class. </exception>
	public virtual WebResponse EndGetResponse(IAsyncResult asyncResult)
	{
		throw ExceptionHelper.MethodNotImplementedException;
	}

	/// <summary>When overridden in a descendant class, provides an asynchronous version of the <see cref="M:System.Net.WebRequest.GetRequestStream" /> method.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous request.</returns>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object containing state information for this asynchronous request. </param>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to access the method, when the method is not overridden in a descendant class. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
	{
		throw ExceptionHelper.MethodNotImplementedException;
	}

	/// <summary>When overridden in a descendant class, returns a <see cref="T:System.IO.Stream" /> for writing data to the Internet resource.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> to write data to.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> that references a pending request for a stream. </param>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to access the method, when the method is not overridden in a descendant class. </exception>
	public virtual Stream EndGetRequestStream(IAsyncResult asyncResult)
	{
		throw ExceptionHelper.MethodNotImplementedException;
	}

	/// <summary>When overridden in a descendant class, returns a <see cref="T:System.IO.Stream" /> for writing data to the Internet resource as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation.</returns>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual Task<Stream> GetRequestStreamAsync()
	{
		IWebProxy webProxy = null;
		try
		{
			webProxy = Proxy;
		}
		catch (NotImplementedException)
		{
		}
		if (ExecutionContext.IsFlowSuppressed() && (UseDefaultCredentials || Credentials != null || (webProxy != null && webProxy.Credentials != null)))
		{
			WindowsIdentity currentUser = SafeCaptureIdenity();
			return Task.Run(delegate
			{
				using (currentUser)
				{
					using (currentUser.Impersonate())
					{
						return Task<Stream>.Factory.FromAsync(BeginGetRequestStream, EndGetRequestStream, null);
					}
				}
			});
		}
		return Task.Run(() => Task<Stream>.Factory.FromAsync(BeginGetRequestStream, EndGetRequestStream, null));
	}

	/// <summary>When overridden in a descendant class, returns a response to an Internet request as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation.</returns>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual Task<WebResponse> GetResponseAsync()
	{
		IWebProxy webProxy = null;
		try
		{
			webProxy = Proxy;
		}
		catch (NotImplementedException)
		{
		}
		if (ExecutionContext.IsFlowSuppressed() && (UseDefaultCredentials || Credentials != null || (webProxy != null && webProxy.Credentials != null)))
		{
			WindowsIdentity currentUser = SafeCaptureIdenity();
			return Task.Run(delegate
			{
				using (currentUser)
				{
					using (currentUser.Impersonate())
					{
						return Task<WebResponse>.Factory.FromAsync(BeginGetResponse, EndGetResponse, null);
					}
				}
			});
		}
		return Task.Run(() => Task<WebResponse>.Factory.FromAsync(BeginGetResponse, EndGetResponse, null));
	}

	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.ControlPrincipal)]
	private WindowsIdentity SafeCaptureIdenity()
	{
		return WindowsIdentity.GetCurrent();
	}

	/// <summary>Aborts the Request </summary>
	/// <exception cref="T:System.NotImplementedException">Any attempt is made to access the method, when the method is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual void Abort()
	{
		throw ExceptionHelper.MethodNotImplementedException;
	}

	internal virtual void RequestCallback(object obj)
	{
		throw ExceptionHelper.MethodNotImplementedException;
	}

	/// <summary>Returns a proxy configured with the Internet Explorer settings of the currently impersonated user.</summary>
	/// <returns>An <see cref="T:System.Net.IWebProxy" /> used by every call to instances of <see cref="T:System.Net.WebRequest" />.</returns>
	public static IWebProxy GetSystemWebProxy()
	{
		return InternalGetSystemWebProxy();
	}

	internal static IWebProxy InternalGetSystemWebProxy()
	{
		return WebProxy.CreateDefaultProxy();
	}

	internal void SetupCacheProtocol(Uri uri)
	{
		m_CacheBinding = RequestCacheManager.GetBinding(uri.Scheme);
		InternalSetCachePolicy(m_CacheBinding.Policy);
		if (m_CachePolicy == null)
		{
			InternalSetCachePolicy(DefaultCachePolicy);
		}
	}
}
