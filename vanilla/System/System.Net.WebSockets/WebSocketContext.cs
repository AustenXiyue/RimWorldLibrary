using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Principal;

namespace System.Net.WebSockets;

/// <summary>Used for accessing the information in the WebSocket handshake.</summary>
public abstract class WebSocketContext
{
	/// <summary>The URI requested by the WebSocket client.</summary>
	/// <returns>Returns <see cref="T:System.Uri" />.</returns>
	public abstract Uri RequestUri { get; }

	/// <summary>The HTTP headers that were sent to the server during the opening handshake.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Specialized.NameValueCollection" />.</returns>
	public abstract NameValueCollection Headers { get; }

	/// <summary>The value of the Origin HTTP header included in the opening handshake.</summary>
	/// <returns>Returns <see cref="T:System.String" />.</returns>
	public abstract string Origin { get; }

	/// <summary>The value of the SecWebSocketKey HTTP header included in the opening handshake.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.IEnumerable`1" />.</returns>
	public abstract IEnumerable<string> SecWebSocketProtocols { get; }

	/// <summary>The list of subprotocols requested by the WebSocket client.</summary>
	/// <returns>Returns <see cref="T:System.String" />.</returns>
	public abstract string SecWebSocketVersion { get; }

	/// <summary>The value of the SecWebSocketKey HTTP header included in the opening handshake.</summary>
	/// <returns>Returns <see cref="T:System.String" />.</returns>
	public abstract string SecWebSocketKey { get; }

	/// <summary>The cookies that were passed to the server during the opening handshake.</summary>
	/// <returns>Returns <see cref="T:System.Net.CookieCollection" />.</returns>
	public abstract CookieCollection CookieCollection { get; }

	/// <summary>An object used to obtain identity, authentication information, and security roles for the WebSocket client.</summary>
	/// <returns>Returns <see cref="T:System.Security.Principal.IPrincipal" />.</returns>
	public abstract IPrincipal User { get; }

	/// <summary>Whether the WebSocket client is authenticated.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.</returns>
	public abstract bool IsAuthenticated { get; }

	/// <summary>Whether the WebSocket client connected from the local machine.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.</returns>
	public abstract bool IsLocal { get; }

	/// <summary>Whether the WebSocket connection is secured using Secure Sockets Layer (SSL).</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.</returns>
	public abstract bool IsSecureConnection { get; }

	/// <summary>The WebSocket instance used to interact (send/receive/close/etc) with the WebSocket connection.</summary>
	/// <returns>Returns <see cref="T:System.Net.WebSockets.WebSocket" />.</returns>
	public abstract WebSocket WebSocket { get; }

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketContext" /> class.</summary>
	protected WebSocketContext()
	{
	}
}
