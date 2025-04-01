namespace System.Net.WebSockets;

/// <summary>An instance of this class represents the result of performing a single ReceiveAsync operation on a WebSocket.</summary>
public class WebSocketReceiveResult
{
	/// <summary>Indicates the number of bytes that the WebSocket received.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.</returns>
	public int Count { get; }

	/// <summary>Indicates whether the message has been received completely.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.</returns>
	public bool EndOfMessage { get; }

	/// <summary>Indicates whether the current message is a UTF-8 message or a binary message.</summary>
	/// <returns>Returns <see cref="T:System.Net.WebSockets.WebSocketMessageType" />.</returns>
	public WebSocketMessageType MessageType { get; }

	/// <summary>Indicates the reason why the remote endpoint initiated the close handshake.</summary>
	/// <returns>Returns <see cref="T:System.Net.WebSockets.WebSocketCloseStatus" />.</returns>
	public WebSocketCloseStatus? CloseStatus { get; }

	/// <summary>Returns the optional description that describes why the close handshake has been initiated by the remote endpoint.</summary>
	/// <returns>Returns <see cref="T:System.String" />.</returns>
	public string CloseStatusDescription { get; }

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketReceiveResult" /> class.</summary>
	/// <param name="count">The number of bytes received.</param>
	/// <param name="messageType">The type of message that was received.</param>
	/// <param name="endOfMessage">Indicates whether this is the final message.</param>
	public WebSocketReceiveResult(int count, WebSocketMessageType messageType, bool endOfMessage)
		: this(count, messageType, endOfMessage, null, null)
	{
	}

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketReceiveResult" /> class.</summary>
	/// <param name="count">The number of bytes received.</param>
	/// <param name="messageType">The type of message that was received.</param>
	/// <param name="endOfMessage">Indicates whether this is the final message.</param>
	/// <param name="closeStatus">Indicates the <see cref="T:System.Net.WebSockets.WebSocketCloseStatus" /> of the connection.</param>
	/// <param name="closeStatusDescription">The description of <paramref name="closeStatus" />.</param>
	public WebSocketReceiveResult(int count, WebSocketMessageType messageType, bool endOfMessage, WebSocketCloseStatus? closeStatus, string closeStatusDescription)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		Count = count;
		EndOfMessage = endOfMessage;
		MessageType = messageType;
		CloseStatus = closeStatus;
		CloseStatusDescription = closeStatusDescription;
	}
}
