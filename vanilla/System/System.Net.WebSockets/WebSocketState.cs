namespace System.Net.WebSockets;

/// <summary> Defines the different states a WebSockets instance can be in.</summary>
public enum WebSocketState
{
	/// <summary>Reserved for future use.</summary>
	None,
	/// <summary>The connection is negotiating the handshake with the remote endpoint.</summary>
	Connecting,
	/// <summary>The initial state after the HTTP handshake has been completed.</summary>
	Open,
	/// <summary>A close message was sent to the remote endpoint.</summary>
	CloseSent,
	/// <summary>A close message was received from the remote endpoint.</summary>
	CloseReceived,
	/// <summary>Indicates the WebSocket close handshake completed gracefully.</summary>
	Closed,
	/// <summary>Reserved for future use.</summary>
	Aborted
}
