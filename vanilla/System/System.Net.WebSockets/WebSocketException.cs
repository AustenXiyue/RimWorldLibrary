using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Net.WebSockets;

/// <summary>Represents an exception that occurred when performing an operation on a WebSocket connection.</summary>
[Serializable]
public sealed class WebSocketException : Win32Exception
{
	private readonly WebSocketError _webSocketErrorCode;

	/// <summary>The native error code for the exception that occurred.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.</returns>
	public override int ErrorCode => base.NativeErrorCode;

	/// <summary>Returns a WebSocketError indicating the type of error that occurred.</summary>
	/// <returns>Returns <see cref="T:System.Net.WebSockets.WebSocketError" />.</returns>
	public WebSocketError WebSocketErrorCode => _webSocketErrorCode;

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketException" /> class.</summary>
	public WebSocketException()
		: this(Marshal.GetLastWin32Error())
	{
	}

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketException" /> class.</summary>
	/// <param name="error">The error from the WebSocketError enumeration.</param>
	public WebSocketException(WebSocketError error)
		: this(error, GetErrorMessage(error))
	{
	}

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketException" /> class.</summary>
	/// <param name="error">The error from the WebSocketError enumeration.</param>
	/// <param name="message">The description of the error.</param>
	public WebSocketException(WebSocketError error, string message)
		: base(message)
	{
		_webSocketErrorCode = error;
	}

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketException" /> class.</summary>
	/// <param name="error">The error from the WebSocketError enumeration.</param>
	/// <param name="innerException">Indicates the previous exception that led to the current exception.</param>
	public WebSocketException(WebSocketError error, Exception innerException)
		: this(error, GetErrorMessage(error), innerException)
	{
	}

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketException" /> class.</summary>
	/// <param name="error">The error from the WebSocketError enumeration.</param>
	/// <param name="message">The description of the error.</param>
	/// <param name="innerException">Indicates the previous exception that led to the current exception.</param>
	public WebSocketException(WebSocketError error, string message, Exception innerException)
		: base(message, innerException)
	{
		_webSocketErrorCode = error;
	}

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketException" /> class.</summary>
	/// <param name="nativeError">The native error code for the exception.</param>
	public WebSocketException(int nativeError)
		: base(nativeError)
	{
		_webSocketErrorCode = ((!Succeeded(nativeError)) ? WebSocketError.NativeError : WebSocketError.Success);
		SetErrorCodeOnError(nativeError);
	}

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketException" /> class.</summary>
	/// <param name="nativeError">The native error code for the exception.</param>
	/// <param name="message">The description of the error.</param>
	public WebSocketException(int nativeError, string message)
		: base(nativeError, message)
	{
		_webSocketErrorCode = ((!Succeeded(nativeError)) ? WebSocketError.NativeError : WebSocketError.Success);
		SetErrorCodeOnError(nativeError);
	}

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketException" /> class.</summary>
	/// <param name="nativeError">The native error code for the exception.</param>
	/// <param name="innerException">Indicates the previous exception that led to the current exception.</param>
	public WebSocketException(int nativeError, Exception innerException)
		: base("An internal WebSocket error occurred. Please see the innerException, if present, for more details.", innerException)
	{
		_webSocketErrorCode = ((!Succeeded(nativeError)) ? WebSocketError.NativeError : WebSocketError.Success);
		SetErrorCodeOnError(nativeError);
	}

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketException" /> class.</summary>
	/// <param name="error">The error from the WebSocketError enumeration.</param>
	/// <param name="nativeError">The native error code for the exception.</param>
	public WebSocketException(WebSocketError error, int nativeError)
		: this(error, nativeError, GetErrorMessage(error))
	{
	}

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketException" /> class.</summary>
	/// <param name="error">The error from the WebSocketError enumeration.</param>
	/// <param name="nativeError">The native error code for the exception.</param>
	/// <param name="message">The description of the error.</param>
	public WebSocketException(WebSocketError error, int nativeError, string message)
		: base(message)
	{
		_webSocketErrorCode = error;
		SetErrorCodeOnError(nativeError);
	}

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketException" /> class.</summary>
	/// <param name="error">The error from the WebSocketError enumeration.</param>
	/// <param name="nativeError">The native error code for the exception.</param>
	/// <param name="innerException">Indicates the previous exception that led to the current exception.</param>
	public WebSocketException(WebSocketError error, int nativeError, Exception innerException)
		: this(error, nativeError, GetErrorMessage(error), innerException)
	{
	}

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketException" /> class.</summary>
	/// <param name="error">The error from the WebSocketError enumeration.</param>
	/// <param name="nativeError">The native error code for the exception.</param>
	/// <param name="message">The description of the error.</param>
	/// <param name="innerException">Indicates the previous exception that led to the current exception.</param>
	public WebSocketException(WebSocketError error, int nativeError, string message, Exception innerException)
		: base(message, innerException)
	{
		_webSocketErrorCode = error;
		SetErrorCodeOnError(nativeError);
	}

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketException" /> class.</summary>
	/// <param name="message">The description of the error.</param>
	public WebSocketException(string message)
		: base(message)
	{
	}

	/// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketException" /> class.</summary>
	/// <param name="message">The description of the error.</param>
	/// <param name="innerException">Indicates the previous exception that led to the current exception.</param>
	public WebSocketException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>Sets the SerializationInfo object with the file name and line number where the exception occurred.</summary>
	/// <param name="info">A SerializationInfo object.</param>
	/// <param name="context">The contextual information about the source or destination.</param>
	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	private static string GetErrorMessage(WebSocketError error)
	{
		return error switch
		{
			WebSocketError.InvalidMessageType => global::SR.Format("The received  message type is invalid after calling {0}. {0} should only be used if no more data is expected from the remote endpoint. Use '{1}' instead to keep being able to receive data but close the output channel.", string.Format("{0}.{1}", "WebSocket", "CloseAsync"), string.Format("{0}.{1}", "WebSocket", "CloseOutputAsync")), 
			WebSocketError.Faulted => "An exception caused the WebSocket to enter the Aborted state. Please see the InnerException, if present, for more details.", 
			WebSocketError.NotAWebSocket => "A WebSocket operation was called on a request or response that is not a WebSocket.", 
			WebSocketError.UnsupportedVersion => "Unsupported WebSocket version.", 
			WebSocketError.UnsupportedProtocol => "The WebSocket request or response operation was called with unsupported protocol(s).", 
			WebSocketError.HeaderError => "The WebSocket request or response contained unsupported header(s).", 
			WebSocketError.ConnectionClosedPrematurely => "The remote party closed the WebSocket connection without completing the close handshake.", 
			WebSocketError.InvalidState => "The WebSocket instance cannot be used for communication because it has been transitioned into an invalid state.", 
			_ => "An internal WebSocket error occurred. Please see the innerException, if present, for more details.", 
		};
	}

	private void SetErrorCodeOnError(int nativeError)
	{
		if (!Succeeded(nativeError))
		{
			base.HResult = nativeError;
		}
	}

	private static bool Succeeded(int hr)
	{
		return hr >= 0;
	}
}
