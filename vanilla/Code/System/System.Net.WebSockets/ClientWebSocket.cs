using System.Threading;
using System.Threading.Tasks;

namespace System.Net.WebSockets;

/// <summary>Provides a client for connecting to WebSocket services.</summary>
public sealed class ClientWebSocket : WebSocket
{
	private enum InternalState
	{
		Created,
		Connecting,
		Connected,
		Disposed
	}

	private readonly ClientWebSocketOptions _options;

	private WebSocketHandle _innerWebSocket;

	private int _state;

	/// <summary>Gets the WebSocket options for the <see cref="T:System.Net.WebSockets.ClientWebSocket" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.WebSockets.ClientWebSocketOptions" />.The WebSocket options for the <see cref="T:System.Net.WebSockets.ClientWebSocket" /> instance.</returns>
	public ClientWebSocketOptions Options => _options;

	/// <summary>Gets the reason why the close handshake was initiated on <see cref="T:System.Net.WebSockets.ClientWebSocket" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.WebSockets.WebSocketCloseStatus" />.The reason why the close handshake was initiated.</returns>
	public override WebSocketCloseStatus? CloseStatus
	{
		get
		{
			if (WebSocketHandle.IsValid(_innerWebSocket))
			{
				return _innerWebSocket.CloseStatus;
			}
			return null;
		}
	}

	/// <summary>Gets a description of the reason why the <see cref="T:System.Net.WebSockets.ClientWebSocket" /> instance was closed.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The description of the reason why the <see cref="T:System.Net.WebSockets.ClientWebSocket" /> instance was closed.</returns>
	public override string CloseStatusDescription
	{
		get
		{
			if (WebSocketHandle.IsValid(_innerWebSocket))
			{
				return _innerWebSocket.CloseStatusDescription;
			}
			return null;
		}
	}

	/// <summary>Gets the supported WebSocket sub-protocol for the <see cref="T:System.Net.WebSockets.ClientWebSocket" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The supported WebSocket sub-protocol.</returns>
	public override string SubProtocol
	{
		get
		{
			if (WebSocketHandle.IsValid(_innerWebSocket))
			{
				return _innerWebSocket.SubProtocol;
			}
			return null;
		}
	}

	/// <summary>Get the WebSocket state of the <see cref="T:System.Net.WebSockets.ClientWebSocket" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.WebSockets.WebSocketState" />.The WebSocket state of the <see cref="T:System.Net.WebSockets.ClientWebSocket" /> instance.</returns>
	public override WebSocketState State
	{
		get
		{
			if (WebSocketHandle.IsValid(_innerWebSocket))
			{
				return _innerWebSocket.State;
			}
			return (InternalState)_state switch
			{
				InternalState.Created => WebSocketState.None, 
				InternalState.Connecting => WebSocketState.Connecting, 
				_ => WebSocketState.Closed, 
			};
		}
	}

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.ClientWebSocket" /> class.</summary>
	public ClientWebSocket()
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(this, null, ".ctor");
		}
		WebSocketHandle.CheckPlatformSupport();
		_state = 0;
		_options = new ClientWebSocketOptions();
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Exit(this, null, ".ctor");
		}
	}

	/// <summary>Connect to a WebSocket server as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	/// <param name="uri">The URI of the WebSocket server to connect to.</param>
	/// <param name="cancellationToken">A cancellation token used to propagate notification that the  operation should be canceled.</param>
	public Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		if (!uri.IsAbsoluteUri)
		{
			throw new ArgumentException("This operation is not supported for a relative URI.", "uri");
		}
		if (uri.Scheme != "ws" && uri.Scheme != "wss")
		{
			throw new ArgumentException("Only Uris starting with 'ws://' or 'wss://' are supported.", "uri");
		}
		switch ((InternalState)Interlocked.CompareExchange(ref _state, 1, 0))
		{
		case InternalState.Disposed:
			throw new ObjectDisposedException(GetType().FullName);
		default:
			throw new InvalidOperationException("The WebSocket has already been started.");
		case InternalState.Created:
			_options.SetToReadOnly();
			return ConnectAsyncCore(uri, cancellationToken);
		}
	}

	private async Task ConnectAsyncCore(Uri uri, CancellationToken cancellationToken)
	{
		_innerWebSocket = WebSocketHandle.Create();
		try
		{
			if (Interlocked.CompareExchange(ref _state, 2, 1) != 1)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
			await _innerWebSocket.ConnectAsyncCore(uri, cancellationToken, _options).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception message)
		{
			if (NetEventSource.IsEnabled)
			{
				NetEventSource.Error(this, message, "ConnectAsyncCore");
			}
			throw;
		}
	}

	/// <summary>Send data on <see cref="T:System.Net.WebSockets.ClientWebSocket" /> as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	/// <param name="buffer">The buffer containing the message to be sent.</param>
	/// <param name="messageType">Specifies whether the buffer is clear text or in a binary format.</param>
	/// <param name="endOfMessage">Specifies whether this is the final asynchronous send. Set to true if this is the final send; false otherwise.</param>
	/// <param name="cancellationToken">A cancellation token used to propagate notification that this  operation should be canceled.</param>
	public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
	{
		ThrowIfNotConnected();
		return _innerWebSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
	}

	/// <summary>Receive data on <see cref="T:System.Net.WebSockets.ClientWebSocket" /> as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation.</returns>
	/// <param name="buffer">The buffer to receive the response.</param>
	/// <param name="cancellationToken">A cancellation token used to propagate notification that this  operation should be canceled.</param>
	public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
	{
		ThrowIfNotConnected();
		return _innerWebSocket.ReceiveAsync(buffer, cancellationToken);
	}

	/// <summary>Close the <see cref="T:System.Net.WebSockets.ClientWebSocket" /> instance as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	/// <param name="closeStatus">The WebSocket close status.</param>
	/// <param name="statusDescription">A description of the close status.</param>
	/// <param name="cancellationToken">A cancellation token used to propagate notification that this  operation should be canceled.</param>
	public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
	{
		ThrowIfNotConnected();
		return _innerWebSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
	}

	/// <summary>Close the output for the <see cref="T:System.Net.WebSockets.ClientWebSocket" /> instance as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	/// <param name="closeStatus">The WebSocket close status.</param>
	/// <param name="statusDescription">A description of the close status.</param>
	/// <param name="cancellationToken">A cancellation token used to propagate notification that this  operation should be canceled.</param>
	public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
	{
		ThrowIfNotConnected();
		return _innerWebSocket.CloseOutputAsync(closeStatus, statusDescription, cancellationToken);
	}

	/// <summary>Aborts the connection and cancels any pending IO operations.</summary>
	public override void Abort()
	{
		if (_state != 3)
		{
			if (WebSocketHandle.IsValid(_innerWebSocket))
			{
				_innerWebSocket.Abort();
			}
			Dispose();
		}
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.WebSockets.ClientWebSocket" /> instance.</summary>
	public override void Dispose()
	{
		if (Interlocked.Exchange(ref _state, 3) != 3 && WebSocketHandle.IsValid(_innerWebSocket))
		{
			_innerWebSocket.Dispose();
		}
	}

	private void ThrowIfNotConnected()
	{
		if (_state == 3)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (_state != 2)
		{
			throw new InvalidOperationException("The WebSocket is not connected.");
		}
	}
}
