using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Principal;
using Unity;

namespace System.Net.WebSockets;

/// <summary>Provides access to information received by the <see cref="T:System.Net.HttpListener" /> class when accepting WebSocket connections.</summary>
public class HttpListenerWebSocketContext : WebSocketContext
{
	private readonly Uri _requestUri;

	private readonly NameValueCollection _headers;

	private readonly CookieCollection _cookieCollection;

	private readonly IPrincipal _user;

	private readonly bool _isAuthenticated;

	private readonly bool _isLocal;

	private readonly bool _isSecureConnection;

	private readonly string _origin;

	private readonly IEnumerable<string> _secWebSocketProtocols;

	private readonly string _secWebSocketVersion;

	private readonly string _secWebSocketKey;

	private readonly WebSocket _webSocket;

	/// <summary>Gets the URI requested by the WebSocket client.</summary>
	/// <returns>Returns <see cref="T:System.Uri" />.The URI requested by the WebSocket client.</returns>
	public override Uri RequestUri => _requestUri;

	/// <summary>Gets the HTTP headers received by the <see cref="T:System.Net.HttpListener" /> object in the WebSocket opening handshake.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Specialized.NameValueCollection" />.The HTTP headers received by the <see cref="T:System.Net.HttpListener" /> object.</returns>
	public override NameValueCollection Headers => _headers;

	/// <summary>Gets the value of the Origin HTTP header included in the WebSocket opening handshake.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The value of the Origin HTTP header.</returns>
	public override string Origin => _origin;

	/// <summary>Gets the list of the Secure WebSocket protocols included in the WebSocket opening handshake.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.IEnumerable`1" />.The list of the Secure WebSocket protocols.</returns>
	public override IEnumerable<string> SecWebSocketProtocols => _secWebSocketProtocols;

	/// <summary>Gets the list of sub-protocols requested by the WebSocket client.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The list of sub-protocols requested by the WebSocket client.</returns>
	public override string SecWebSocketVersion => _secWebSocketVersion;

	/// <summary>Gets the value of the SecWebSocketKey HTTP header included in the WebSocket opening handshake.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The value of the SecWebSocketKey HTTP header.</returns>
	public override string SecWebSocketKey => _secWebSocketKey;

	/// <summary>Gets the cookies received by the <see cref="T:System.Net.HttpListener" /> object in the WebSocket opening handshake.</summary>
	/// <returns>Returns <see cref="T:System.Net.CookieCollection" />.The cookies received by the <see cref="T:System.Net.HttpListener" /> object.</returns>
	public override CookieCollection CookieCollection => _cookieCollection;

	/// <summary>Gets an object used to obtain identity, authentication information, and security roles for the WebSocket client.</summary>
	/// <returns>Returns <see cref="T:System.Security.Principal.IPrincipal" />.The identity, authentication information, and security roles for the WebSocket client.</returns>
	public override IPrincipal User => _user;

	/// <summary>Gets a value that indicates if the WebSocket client is authenticated.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the WebSocket client is authenticated; otherwise false.</returns>
	public override bool IsAuthenticated => _isAuthenticated;

	/// <summary>Gets a value that indicates if the WebSocket client connected from the local machine.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the WebSocket client connected from the local machine; otherwise false.</returns>
	public override bool IsLocal => _isLocal;

	/// <summary>Gets a value that indicates if the WebSocket connection is secured using Secure Sockets Layer (SSL).</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the WebSocket connection is secured using SSL; otherwise false.</returns>
	public override bool IsSecureConnection => _isSecureConnection;

	/// <summary>Gets the WebSocket instance used to send and receive data over the WebSocket connection.</summary>
	/// <returns>Returns <see cref="T:System.Net.WebSockets.WebSocket" />.The WebSocket instance.</returns>
	public override WebSocket WebSocket => _webSocket;

	internal HttpListenerWebSocketContext(Uri requestUri, NameValueCollection headers, CookieCollection cookieCollection, IPrincipal user, bool isAuthenticated, bool isLocal, bool isSecureConnection, string origin, IEnumerable<string> secWebSocketProtocols, string secWebSocketVersion, string secWebSocketKey, WebSocket webSocket)
	{
		_cookieCollection = new CookieCollection();
		_cookieCollection.Add(cookieCollection);
		_headers = new NameValueCollection(headers);
		_user = CopyPrincipal(user);
		_requestUri = requestUri;
		_isAuthenticated = isAuthenticated;
		_isLocal = isLocal;
		_isSecureConnection = isSecureConnection;
		_origin = origin;
		_secWebSocketProtocols = secWebSocketProtocols;
		_secWebSocketVersion = secWebSocketVersion;
		_secWebSocketKey = secWebSocketKey;
		_webSocket = webSocket;
	}

	private static IPrincipal CopyPrincipal(IPrincipal user)
	{
		if (user != null)
		{
			if (user is WindowsPrincipal)
			{
				throw new PlatformNotSupportedException();
			}
			if (user.Identity is HttpListenerBasicIdentity httpListenerBasicIdentity)
			{
				return new GenericPrincipal(new HttpListenerBasicIdentity(httpListenerBasicIdentity.Name, httpListenerBasicIdentity.Password), null);
			}
		}
		return null;
	}

	internal HttpListenerWebSocketContext()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
