using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace System.Net.WebSockets;

/// <summary>Options to use with a  <see cref="T:System.Net.WebSockets.ClientWebSocket" /> object.</summary>
public sealed class ClientWebSocketOptions
{
	private bool _isReadOnly;

	private readonly List<string> _requestedSubProtocols;

	private readonly WebHeaderCollection _requestHeaders;

	private TimeSpan _keepAliveInterval = WebSocket.DefaultKeepAliveInterval;

	private bool _useDefaultCredentials;

	private ICredentials _credentials;

	private IWebProxy _proxy;

	private X509CertificateCollection _clientCertificates;

	private CookieContainer _cookies;

	private int _receiveBufferSize = 4096;

	private int _sendBufferSize = 4096;

	private ArraySegment<byte>? _buffer;

	internal WebHeaderCollection RequestHeaders => _requestHeaders;

	internal List<string> RequestedSubProtocols => _requestedSubProtocols;

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that indicates if default credentials should be used during WebSocket handshake.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if default credentials should be used during WebSocket handshake; otherwise false. The default is true.</returns>
	public bool UseDefaultCredentials
	{
		get
		{
			return _useDefaultCredentials;
		}
		set
		{
			ThrowIfReadOnly();
			_useDefaultCredentials = value;
		}
	}

	/// <summary>Gets or sets the credential information for the client.</summary>
	/// <returns>Returns <see cref="T:System.Net.ICredentials" />.The credential information for the client.</returns>
	public ICredentials Credentials
	{
		get
		{
			return _credentials;
		}
		set
		{
			ThrowIfReadOnly();
			_credentials = value;
		}
	}

	/// <summary>Gets or sets the proxy for WebSocket requests.</summary>
	/// <returns>Returns <see cref="T:System.Net.IWebProxy" />.The proxy for WebSocket requests.</returns>
	public IWebProxy Proxy
	{
		get
		{
			return _proxy;
		}
		set
		{
			ThrowIfReadOnly();
			_proxy = value;
		}
	}

	/// <summary>Gets or sets a collection of client side certificates.</summary>
	/// <returns>Returns <see cref="T:System.Security.Cryptography.X509Certificates.X509CertificateCollection" />.A collection of client side certificates.</returns>
	public X509CertificateCollection ClientCertificates
	{
		get
		{
			if (_clientCertificates == null)
			{
				_clientCertificates = new X509CertificateCollection();
			}
			return _clientCertificates;
		}
		set
		{
			ThrowIfReadOnly();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_clientCertificates = value;
		}
	}

	/// <summary>Gets or sets the cookies associated with the request.</summary>
	/// <returns>Returns <see cref="T:System.Net.CookieContainer" />.The cookies associated with the request.</returns>
	public CookieContainer Cookies
	{
		get
		{
			return _cookies;
		}
		set
		{
			ThrowIfReadOnly();
			_cookies = value;
		}
	}

	/// <summary>Gets or sets the WebSocket protocol keep-alive interval in milliseconds.</summary>
	/// <returns>Returns <see cref="T:System.TimeSpan" />.The WebSocket protocol keep-alive interval in milliseconds.</returns>
	public TimeSpan KeepAliveInterval
	{
		get
		{
			return _keepAliveInterval;
		}
		set
		{
			ThrowIfReadOnly();
			if (value != Timeout.InfiniteTimeSpan && value < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException("value", value, global::SR.Format("The argument must be a value greater than {0}.", Timeout.InfiniteTimeSpan.ToString()));
			}
			_keepAliveInterval = value;
		}
	}

	internal int ReceiveBufferSize => _receiveBufferSize;

	internal int SendBufferSize => _sendBufferSize;

	internal ArraySegment<byte>? Buffer => _buffer;

	internal ClientWebSocketOptions()
	{
		_requestedSubProtocols = new List<string>();
		_requestHeaders = new WebHeaderCollection();
	}

	/// <summary>Creates a HTTP request header and its value.</summary>
	/// <param name="headerName">The name of the HTTP header.</param>
	/// <param name="headerValue">The value of the HTTP header.</param>
	public void SetRequestHeader(string headerName, string headerValue)
	{
		ThrowIfReadOnly();
		_requestHeaders.Set(headerName, headerValue);
	}

	/// <summary>Adds a sub-protocol to be negotiated during the WebSocket connection handshake.</summary>
	/// <param name="subProtocol">The WebSocket sub-protocol to add.</param>
	public void AddSubProtocol(string subProtocol)
	{
		ThrowIfReadOnly();
		WebSocketValidate.ValidateSubprotocol(subProtocol);
		foreach (string requestedSubProtocol in _requestedSubProtocols)
		{
			if (string.Equals(requestedSubProtocol, subProtocol, StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException(global::SR.Format("Duplicate protocols are not allowed: '{0}'.", subProtocol), "subProtocol");
			}
		}
		_requestedSubProtocols.Add(subProtocol);
	}

	/// <summary>Sets the client buffer parameters.</summary>
	/// <param name="receiveBufferSize">The size, in bytes, of the client receive buffer.</param>
	/// <param name="sendBufferSize">The size, in bytes, of the client send buffer.</param>
	public void SetBuffer(int receiveBufferSize, int sendBufferSize)
	{
		ThrowIfReadOnly();
		if (receiveBufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("receiveBufferSize", receiveBufferSize, global::SR.Format("The argument must be a value greater than {0}.", 1));
		}
		if (sendBufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("sendBufferSize", sendBufferSize, global::SR.Format("The argument must be a value greater than {0}.", 1));
		}
		_receiveBufferSize = receiveBufferSize;
		_sendBufferSize = sendBufferSize;
		_buffer = null;
	}

	/// <summary>Sets client buffer parameters.</summary>
	/// <param name="receiveBufferSize">The size, in bytes, of the client receive buffer.</param>
	/// <param name="sendBufferSize">The size, in bytes, of the client send buffer.</param>
	/// <param name="buffer">The receive buffer to use.</param>
	public void SetBuffer(int receiveBufferSize, int sendBufferSize, ArraySegment<byte> buffer)
	{
		ThrowIfReadOnly();
		if (receiveBufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("receiveBufferSize", receiveBufferSize, global::SR.Format("The argument must be a value greater than {0}.", 1));
		}
		if (sendBufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("sendBufferSize", sendBufferSize, global::SR.Format("The argument must be a value greater than {0}.", 1));
		}
		WebSocketValidate.ValidateArraySegment(buffer, "buffer");
		if (buffer.Count == 0)
		{
			throw new ArgumentOutOfRangeException("buffer");
		}
		_receiveBufferSize = receiveBufferSize;
		_sendBufferSize = sendBufferSize;
		_buffer = buffer;
	}

	internal void SetToReadOnly()
	{
		_isReadOnly = true;
	}

	private void ThrowIfReadOnly()
	{
		if (_isReadOnly)
		{
			throw new InvalidOperationException("The WebSocket has already been started.");
		}
	}
}
