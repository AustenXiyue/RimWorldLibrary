using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity;

namespace System.Net.Sockets;

/// <summary>Represents an asynchronous socket operation.</summary>
public class SocketAsyncEventArgs : EventArgs, IDisposable
{
	private bool disposed;

	internal volatile int in_progress;

	internal EndPoint remote_ep;

	internal Socket current_socket;

	internal SocketAsyncResult socket_async_result = new SocketAsyncResult();

	internal IList<ArraySegment<byte>> m_BufferList;

	/// <summary>Gets the exception in the case of a connection failure when a <see cref="T:System.Net.DnsEndPoint" /> was used.</summary>
	/// <returns>An <see cref="T:System.Exception" /> that indicates the cause of the connection error when a <see cref="T:System.Net.DnsEndPoint" /> was specified for the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint" /> property.</returns>
	public Exception ConnectByNameError { get; internal set; }

	/// <summary>Gets or sets the socket to use or the socket created for accepting a connection with an asynchronous socket method.</summary>
	/// <returns>The <see cref="T:System.Net.Sockets.Socket" /> to use or the socket created for accepting a connection with an asynchronous socket method.</returns>
	public Socket AcceptSocket { get; set; }

	/// <summary>Gets the data buffer to use with an asynchronous socket method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array that represents the data buffer to use with an asynchronous socket method.</returns>
	public byte[] Buffer { get; private set; }

	/// <summary>Gets or sets an array of data buffers to use with an asynchronous socket method.</summary>
	/// <returns>An <see cref="T:System.Collections.IList" /> that represents an array of data buffers to use with an asynchronous socket method.</returns>
	/// <exception cref="T:System.ArgumentException">There are ambiguous buffers specified on a set operation. This exception occurs if the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer" /> property has been set to a non-null value and an attempt was made to set the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.BufferList" /> property to a non-null value.</exception>
	public IList<ArraySegment<byte>> BufferList
	{
		get
		{
			return m_BufferList;
		}
		set
		{
			if (Buffer != null && value != null)
			{
				throw new ArgumentException("Buffer and BufferList properties cannot both be non-null.");
			}
			m_BufferList = value;
		}
	}

	/// <summary>Gets the number of bytes transferred in the socket operation.</summary>
	/// <returns>An <see cref="T:System.Int32" /> that contains the number of bytes transferred in the socket operation.</returns>
	public int BytesTransferred { get; internal set; }

	/// <summary>Gets the maximum amount of data, in bytes, to send or receive in an asynchronous operation.</summary>
	/// <returns>An <see cref="T:System.Int32" /> that contains the maximum amount of data, in bytes, to send or receive.</returns>
	public int Count { get; internal set; }

	/// <summary>Gets or sets a value that specifies if socket can be reused after a disconnect operation.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> that specifies if socket can be reused after a disconnect operation.</returns>
	public bool DisconnectReuseSocket { get; set; }

	/// <summary>Gets the type of socket operation most recently performed with this context object.</summary>
	/// <returns>A <see cref="T:System.Net.Sockets.SocketAsyncOperation" /> instance that indicates the type of socket operation most recently performed with this context object.</returns>
	public SocketAsyncOperation LastOperation { get; private set; }

	/// <summary>Gets the offset, in bytes, into the data buffer referenced by the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer" /> property.</summary>
	/// <returns>An <see cref="T:System.Int32" /> that contains the offset, in bytes, into the data buffer referenced by the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer" /> property.</returns>
	public int Offset { get; private set; }

	/// <summary>Gets or sets the remote IP endpoint for an asynchronous operation.</summary>
	/// <returns>An <see cref="T:System.Net.EndPoint" /> that represents the remote IP endpoint for an asynchronous operation.</returns>
	public EndPoint RemoteEndPoint
	{
		get
		{
			return remote_ep;
		}
		set
		{
			remote_ep = value;
		}
	}

	/// <summary>Gets the IP address and interface of a received packet.</summary>
	/// <returns>An <see cref="T:System.Net.Sockets.IPPacketInformation" /> instance that contains the destination IP address and interface of a received packet.</returns>
	public IPPacketInformation ReceiveMessageFromPacketInfo { get; private set; }

	/// <summary>Gets or sets an array of buffers to be sent for an asynchronous operation used by the <see cref="M:System.Net.Sockets.Socket.SendPacketsAsync(System.Net.Sockets.SocketAsyncEventArgs)" /> method.</summary>
	/// <returns>An array of <see cref="T:System.Net.Sockets.SendPacketsElement" /> objects that represent an array of buffers to be sent.</returns>
	public SendPacketsElement[] SendPacketsElements { get; set; }

	/// <summary>Gets or sets a bitwise combination of <see cref="T:System.Net.Sockets.TransmitFileOptions" /> values for an asynchronous operation used by the <see cref="M:System.Net.Sockets.Socket.SendPacketsAsync(System.Net.Sockets.SocketAsyncEventArgs)" /> method.</summary>
	/// <returns>A <see cref="T:System.Net.Sockets.TransmitFileOptions" /> that contains a bitwise combination of values that are used with an asynchronous operation.</returns>
	public TransmitFileOptions SendPacketsFlags { get; set; }

	/// <summary>Gets or sets the size, in bytes, of the data block used in the send operation.</summary>
	/// <returns>An <see cref="T:System.Int32" /> that contains the size, in bytes, of the data block used in the send operation.</returns>
	[System.MonoTODO("unused property")]
	public int SendPacketsSendSize { get; set; }

	/// <summary>Gets or sets the result of the asynchronous socket operation.</summary>
	/// <returns>A <see cref="T:System.Net.Sockets.SocketError" /> that represents the result of the asynchronous socket operation.</returns>
	public SocketError SocketError { get; set; }

	/// <summary>Gets the results of an asynchronous socket operation or sets the behavior of an asynchronous operation.</summary>
	/// <returns>A <see cref="T:System.Net.Sockets.SocketFlags" /> that represents the results of an asynchronous socket operation.</returns>
	public SocketFlags SocketFlags { get; set; }

	/// <summary>Gets or sets a user or application object associated with this asynchronous socket operation.</summary>
	/// <returns>An object that represents the user or application object associated with this asynchronous socket operation.</returns>
	public object UserToken { get; set; }

	/// <summary>The created and connected <see cref="T:System.Net.Sockets.Socket" /> object after successful completion of the <see cref="Overload:System.Net.Sockets.Socket.ConnectAsync" /> method.</summary>
	/// <returns>The connected <see cref="T:System.Net.Sockets.Socket" /> object.</returns>
	public Socket ConnectSocket
	{
		get
		{
			SocketError socketError = SocketError;
			if (socketError == SocketError.AccessDenied)
			{
				return null;
			}
			return current_socket;
		}
	}

	internal bool PolicyRestricted { get; private set; }

	/// <summary>Gets or sets the protocol to use to download the socket client access policy file. </summary>
	/// <returns>Returns <see cref="T:System.Net.Sockets.SocketClientAccessPolicyProtocol" />.The protocol to use to download the socket client access policy file.</returns>
	public SocketClientAccessPolicyProtocol SocketClientAccessPolicyProtocol
	{
		[CompilerGenerated]
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(SocketClientAccessPolicyProtocol);
		}
		[CompilerGenerated]
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>The event used to complete an asynchronous operation.</summary>
	public event EventHandler<SocketAsyncEventArgs> Completed;

	internal SocketAsyncEventArgs(bool policy)
		: this()
	{
		PolicyRestricted = policy;
	}

	/// <summary>Creates an empty <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> instance.</summary>
	/// <exception cref="T:System.NotSupportedException">The platform is not supported. </exception>
	public SocketAsyncEventArgs()
	{
		SendPacketsSendSize = -1;
	}

	/// <summary>Frees resources used by the <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> class.</summary>
	~SocketAsyncEventArgs()
	{
		Dispose(disposing: false);
	}

	private void Dispose(bool disposing)
	{
		disposed = true;
		if (disposing)
		{
			_ = in_progress;
		}
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> instance and optionally disposes of the managed resources.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	internal void SetLastOperation(SocketAsyncOperation op)
	{
		if (disposed)
		{
			throw new ObjectDisposedException("System.Net.Sockets.SocketAsyncEventArgs");
		}
		if (Interlocked.Exchange(ref in_progress, 1) != 0)
		{
			throw new InvalidOperationException("Operation already in progress");
		}
		LastOperation = op;
	}

	internal void Complete()
	{
		OnCompleted(this);
	}

	/// <summary>Represents a method that is called when an asynchronous operation completes.</summary>
	/// <param name="e">The event that is signaled.</param>
	protected virtual void OnCompleted(SocketAsyncEventArgs e)
	{
		e?.Completed?.Invoke(e.current_socket, e);
	}

	/// <summary>Sets the data buffer to use with an asynchronous socket method.</summary>
	/// <param name="offset">The offset, in bytes, in the data buffer where the operation starts.</param>
	/// <param name="count">The maximum amount of data, in bytes, to send or receive in the buffer.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">An argument was out of range. This exception occurs if the <paramref name="offset" /> parameter is less than zero or greater than the length of the array in the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer" /> property. This exception also occurs if the <paramref name="count" /> parameter is less than zero or greater than the length of the array in the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer" /> property minus the <paramref name="offset" /> parameter.</exception>
	public void SetBuffer(int offset, int count)
	{
		SetBuffer(Buffer, offset, count);
	}

	/// <summary>Sets the data buffer to use with an asynchronous socket method.</summary>
	/// <param name="buffer">The data buffer to use with an asynchronous socket method.</param>
	/// <param name="offset">The offset, in bytes, in the data buffer where the operation starts.</param>
	/// <param name="count">The maximum amount of data, in bytes, to send or receive in the buffer.</param>
	/// <exception cref="T:System.ArgumentException">There are ambiguous buffers specified. This exception occurs if the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer" /> property is also not null and the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.BufferList" /> property is also not null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">An argument was out of range. This exception occurs if the <paramref name="offset" /> parameter is less than zero or greater than the length of the array in the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer" /> property. This exception also occurs if the <paramref name="count" /> parameter is less than zero or greater than the length of the array in the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer" /> property minus the <paramref name="offset" /> parameter.</exception>
	public void SetBuffer(byte[] buffer, int offset, int count)
	{
		if (buffer != null)
		{
			if (BufferList != null)
			{
				throw new ArgumentException("Buffer and BufferList properties cannot both be non-null.");
			}
			int num = buffer.Length;
			if (offset < 0 || (offset != 0 && offset >= num))
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (count < 0 || count > num - offset)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			Count = count;
			Offset = offset;
		}
		Buffer = buffer;
	}

	internal void StartOperationCommon(Socket socket)
	{
		current_socket = socket;
	}

	internal void StartOperationWrapperConnect(MultipleConnectAsync args)
	{
		SetLastOperation(SocketAsyncOperation.Connect);
	}

	internal void FinishConnectByNameSyncFailure(Exception exception, int bytesTransferred, SocketFlags flags)
	{
		throw new NotImplementedException();
	}

	internal void FinishOperationAsyncFailure(Exception exception, int bytesTransferred, SocketFlags flags)
	{
		throw new NotImplementedException();
	}

	internal void FinishWrapperConnectSuccess(Socket connectSocket, int bytesTransferred, SocketFlags flags)
	{
		SetResults(SocketError.Success, bytesTransferred, flags);
		current_socket = connectSocket;
		OnCompleted(this);
	}

	internal void SetResults(SocketError socketError, int bytesTransferred, SocketFlags flags)
	{
		SocketError = socketError;
		BytesTransferred = bytesTransferred;
		SocketFlags = flags;
	}
}
