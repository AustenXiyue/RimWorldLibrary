namespace System.Net.WebSockets;

/// <summary>Represents well known WebSocket close codes as defined in section 11.7 of the WebSocket protocol spec.</summary>
public enum WebSocketCloseStatus
{
	/// <summary>(1000) The connection has closed after the request was fulfilled.</summary>
	NormalClosure = 1000,
	/// <summary>(1001) Indicates an endpoint is being removed. Either the server or client will become unavailable.</summary>
	EndpointUnavailable = 1001,
	/// <summary>(1002) The client or server is terminating the connection because of a protocol error.</summary>
	ProtocolError = 1002,
	/// <summary>(1003) The client or server is terminating the connection because it cannot accept the data type it received.</summary>
	InvalidMessageType = 1003,
	/// <summary>No error specified.</summary>
	Empty = 1005,
	/// <summary>(1007) The client or server is terminating the connection because it has received data inconsistent with the message type.</summary>
	InvalidPayloadData = 1007,
	/// <summary>(1008) The connection will be closed because an endpoint has received a message that violates its policy.</summary>
	PolicyViolation = 1008,
	/// <summary>(1004) Reserved for future use.</summary>
	MessageTooBig = 1009,
	/// <summary>(1010) The client is terminating the connection because it expected the server to negotiate an extension.</summary>
	MandatoryExtension = 1010,
	/// <summary>The connection will be closed by the server because of an error on the server.</summary>
	InternalServerError = 1011
}
