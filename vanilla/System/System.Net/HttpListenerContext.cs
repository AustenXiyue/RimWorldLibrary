using System.Net.WebSockets;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace System.Net;

/// <summary>Provides access to the request and response objects used by the <see cref="T:System.Net.HttpListener" /> class. This class cannot be inherited.</summary>
public sealed class HttpListenerContext
{
	private HttpListenerRequest request;

	private HttpListenerResponse response;

	private IPrincipal user;

	private HttpConnection cnc;

	private string error;

	private int err_status;

	internal HttpListener Listener;

	internal int ErrorStatus
	{
		get
		{
			return err_status;
		}
		set
		{
			err_status = value;
		}
	}

	internal string ErrorMessage
	{
		get
		{
			return error;
		}
		set
		{
			error = value;
		}
	}

	internal bool HaveError => error != null;

	internal HttpConnection Connection => cnc;

	/// <summary>Gets the <see cref="T:System.Net.HttpListenerRequest" /> that represents a client's request for a resource.</summary>
	/// <returns>An <see cref="T:System.Net.HttpListenerRequest" /> object that represents the client request.</returns>
	public HttpListenerRequest Request => request;

	/// <summary>Gets the <see cref="T:System.Net.HttpListenerResponse" /> object that will be sent to the client in response to the client's request. </summary>
	/// <returns>An <see cref="T:System.Net.HttpListenerResponse" /> object used to send a response back to the client.</returns>
	public HttpListenerResponse Response => response;

	/// <summary>Gets an object used to obtain identity, authentication information, and security roles for the client whose request is represented by this <see cref="T:System.Net.HttpListenerContext" /> object. </summary>
	/// <returns>An <see cref="T:System.Security.Principal.IPrincipal" /> object that describes the client, or null if the <see cref="T:System.Net.HttpListener" /> that supplied this <see cref="T:System.Net.HttpListenerContext" /> does not require authentication.</returns>
	public IPrincipal User => user;

	internal HttpListenerContext(HttpConnection cnc)
	{
		err_status = 400;
		base._002Ector();
		this.cnc = cnc;
		request = new HttpListenerRequest(this);
		response = new HttpListenerResponse(this);
	}

	internal void ParseAuthentication(AuthenticationSchemes expectedSchemes)
	{
		if (expectedSchemes == AuthenticationSchemes.Anonymous)
		{
			return;
		}
		string text = request.Headers["Authorization"];
		if (text != null && text.Length >= 2)
		{
			string[] array = text.Split(new char[1] { ' ' }, 2);
			if (string.Compare(array[0], "basic", ignoreCase: true) == 0)
			{
				user = ParseBasicAuthentication(array[1]);
			}
		}
	}

	internal IPrincipal ParseBasicAuthentication(string authData)
	{
		try
		{
			string text = null;
			string text2 = null;
			int num = -1;
			string @string = Encoding.Default.GetString(Convert.FromBase64String(authData));
			num = @string.IndexOf(':');
			text2 = @string.Substring(num + 1);
			@string = @string.Substring(0, num);
			num = @string.IndexOf('\\');
			text = ((num <= 0) ? @string : @string.Substring(num));
			return new GenericPrincipal(new HttpListenerBasicIdentity(text, text2), new string[0]);
		}
		catch (Exception)
		{
			return null;
		}
	}

	/// <summary>Accept a WebSocket connection as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns an <see cref="T:System.Net.WebSockets.HttpListenerWebSocketContext" /> object.</returns>
	/// <param name="subProtocol">The supported WebSocket sub-protocol.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="subProtocol" /> is an empty string-or- <paramref name="subProtocol" /> contains illegal characters.</exception>
	/// <exception cref="T:System.Net.WebSockets.WebSocketException">An error occurred when sending the response to complete the WebSocket handshake.</exception>
	[System.MonoTODO]
	public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol)
	{
		throw new NotImplementedException();
	}

	/// <summary>Accept a WebSocket connection specifying the supported WebSocket sub-protocol  and WebSocket keep-alive interval as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns an <see cref="T:System.Net.WebSockets.HttpListenerWebSocketContext" /> object.</returns>
	/// <param name="subProtocol">The supported WebSocket sub-protocol.</param>
	/// <param name="keepAliveInterval">The WebSocket protocol keep-alive interval in milliseconds.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="subProtocol" /> is an empty string-or- <paramref name="subProtocol" /> contains illegal characters.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="keepAliveInterval" /> is too small.</exception>
	/// <exception cref="T:System.Net.WebSockets.WebSocketException">An error occurred when sending the response to complete the WebSocket handshake.</exception>
	[System.MonoTODO]
	public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, TimeSpan keepAliveInterval)
	{
		throw new NotImplementedException();
	}

	/// <summary>Accept a WebSocket connection specifying the supported WebSocket sub-protocol, receive buffer size, and WebSocket keep-alive interval as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns an <see cref="T:System.Net.WebSockets.HttpListenerWebSocketContext" /> object.</returns>
	/// <param name="subProtocol">The supported WebSocket sub-protocol.</param>
	/// <param name="receiveBufferSize">The receive buffer size in bytes.</param>
	/// <param name="keepAliveInterval">The WebSocket protocol keep-alive interval in milliseconds.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="subProtocol" /> is an empty string-or- <paramref name="subProtocol" /> contains illegal characters.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="keepAliveInterval" /> is too small.-or- <paramref name="receiveBufferSize" /> is less than 16 bytes-or- <paramref name="receiveBufferSize" /> is greater than 64K bytes.</exception>
	/// <exception cref="T:System.Net.WebSockets.WebSocketException">An error occurred when sending the response to complete the WebSocket handshake.</exception>
	[System.MonoTODO]
	public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, int receiveBufferSize, TimeSpan keepAliveInterval)
	{
		throw new NotImplementedException();
	}

	/// <summary>Accept a WebSocket connection specifying the supported WebSocket sub-protocol, receive buffer size, WebSocket keep-alive interval, and the internal buffer as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns an <see cref="T:System.Net.WebSockets.HttpListenerWebSocketContext" /> object.</returns>
	/// <param name="subProtocol">The supported WebSocket sub-protocol.</param>
	/// <param name="receiveBufferSize">The receive buffer size in bytes.</param>
	/// <param name="keepAliveInterval">The WebSocket protocol keep-alive interval in milliseconds.</param>
	/// <param name="internalBuffer">An internal buffer to use for this operation.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="subProtocol" /> is an empty string-or- <paramref name="subProtocol" /> contains illegal characters.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="keepAliveInterval" /> is too small.-or- <paramref name="receiveBufferSize" /> is less than 16 bytes-or- <paramref name="receiveBufferSize" /> is greater than 64K bytes.</exception>
	/// <exception cref="T:System.Net.WebSockets.WebSocketException">An error occurred when sending the response to complete the WebSocket handshake.</exception>
	[System.MonoTODO]
	public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, int receiveBufferSize, TimeSpan keepAliveInterval, ArraySegment<byte> internalBuffer)
	{
		throw new NotImplementedException();
	}

	internal HttpListenerContext()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
