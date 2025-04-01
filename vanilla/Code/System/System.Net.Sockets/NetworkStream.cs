using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Sockets;

/// <summary>Provides the underlying stream of data for network access.</summary>
public class NetworkStream : Stream
{
	private Socket m_StreamSocket;

	private bool m_Readable;

	private bool m_Writeable;

	private bool m_OwnsSocket;

	private int m_CloseTimeout = -1;

	private volatile bool m_CleanedUp;

	private int m_CurrentReadTimeout = -1;

	private int m_CurrentWriteTimeout = -1;

	/// <summary>Gets the underlying <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>A <see cref="T:System.Net.Sockets.Socket" /> that represents the underlying network connection.</returns>
	protected Socket Socket => m_StreamSocket;

	internal Socket InternalSocket
	{
		get
		{
			Socket streamSocket = m_StreamSocket;
			if (m_CleanedUp || streamSocket == null)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
			return streamSocket;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Net.Sockets.NetworkStream" /> can be read.</summary>
	/// <returns>true to indicate that the <see cref="T:System.Net.Sockets.NetworkStream" /> can be read; otherwise, false. The default value is true.</returns>
	protected bool Readable
	{
		get
		{
			return m_Readable;
		}
		set
		{
			m_Readable = value;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Net.Sockets.NetworkStream" /> is writable.</summary>
	/// <returns>true if data can be written to the stream; otherwise, false. The default value is true.</returns>
	protected bool Writeable
	{
		get
		{
			return m_Writeable;
		}
		set
		{
			m_Writeable = value;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Net.Sockets.NetworkStream" /> supports reading.</summary>
	/// <returns>true if data can be read from the stream; otherwise, false. The default value is true.</returns>
	public override bool CanRead => m_Readable;

	/// <summary>Gets a value that indicates whether the stream supports seeking. This property is not currently supported.This property always returns false.</summary>
	/// <returns>false in all cases to indicate that <see cref="T:System.Net.Sockets.NetworkStream" /> cannot seek a specific location in the stream.</returns>
	public override bool CanSeek => false;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Net.Sockets.NetworkStream" /> supports writing.</summary>
	/// <returns>true if data can be written to the <see cref="T:System.Net.Sockets.NetworkStream" />; otherwise, false. The default value is true.</returns>
	public override bool CanWrite => m_Writeable;

	/// <summary>Indicates whether timeout properties are usable for <see cref="T:System.Net.Sockets.NetworkStream" />.</summary>
	/// <returns>true in all cases.</returns>
	public override bool CanTimeout => true;

	/// <summary>Gets or sets the amount of time that a read operation blocks waiting for data. </summary>
	/// <returns>A <see cref="T:System.Int32" /> that specifies the amount of time, in milliseconds, that will elapse before a read operation fails. The default value, <see cref="F:System.Threading.Timeout.Infinite" />, specifies that the read operation does not time out.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified is less than or equal to zero and is not <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override int ReadTimeout
	{
		get
		{
			int num = (int)m_StreamSocket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
			if (num == 0)
			{
				return -1;
			}
			return num;
		}
		set
		{
			if (value <= 0 && value != -1)
			{
				throw new ArgumentOutOfRangeException("value", global::SR.GetString("Timeout can be only be set to 'System.Threading.Timeout.Infinite' or a value > 0."));
			}
			SetSocketTimeoutOption(SocketShutdown.Receive, value, silent: false);
		}
	}

	/// <summary>Gets or sets the amount of time that a write operation blocks waiting for data. </summary>
	/// <returns>A <see cref="T:System.Int32" /> that specifies the amount of time, in milliseconds, that will elapse before a write operation fails. The default value, <see cref="F:System.Threading.Timeout.Infinite" />, specifies that the write operation does not time out.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified is less than or equal to zero and is not <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override int WriteTimeout
	{
		get
		{
			int num = (int)m_StreamSocket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
			if (num == 0)
			{
				return -1;
			}
			return num;
		}
		set
		{
			if (value <= 0 && value != -1)
			{
				throw new ArgumentOutOfRangeException("value", global::SR.GetString("Timeout can be only be set to 'System.Threading.Timeout.Infinite' or a value > 0."));
			}
			SetSocketTimeoutOption(SocketShutdown.Send, value, silent: false);
		}
	}

	/// <summary>Gets a value that indicates whether data is available on the <see cref="T:System.Net.Sockets.NetworkStream" /> to be read.</summary>
	/// <returns>true if data is available on the stream to be read; otherwise, false.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.NetworkStream" /> is closed. </exception>
	/// <exception cref="T:System.IO.IOException">The underlying <see cref="T:System.Net.Sockets.Socket" /> is closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">Use the <see cref="P:System.Net.Sockets.SocketException.ErrorCode" /> property to obtain the specific error code, and refer to the Windows Sockets version 2 API error code documentation in MSDN for a detailed description of the error. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual bool DataAvailable
	{
		get
		{
			if (m_CleanedUp)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
			Socket streamSocket = m_StreamSocket;
			if (streamSocket == null)
			{
				throw new IOException(global::SR.GetString("Unable to read data from the transport connection: {0}.", global::SR.GetString("The connection was closed")));
			}
			return streamSocket.Available != 0;
		}
	}

	/// <summary>Gets the length of the data available on the stream. This property is not currently supported and always throws a <see cref="T:System.NotSupportedException" />.</summary>
	/// <returns>The length of the data available on the stream.</returns>
	/// <exception cref="T:System.NotSupportedException">Any use of this property. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override long Length
	{
		get
		{
			throw new NotSupportedException(global::SR.GetString("This stream does not support seek operations."));
		}
	}

	/// <summary>Gets or sets the current position in the stream. This property is not currently supported and always throws a <see cref="T:System.NotSupportedException" />.</summary>
	/// <returns>The current position in the stream.</returns>
	/// <exception cref="T:System.NotSupportedException">Any use of this property. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override long Position
	{
		get
		{
			throw new NotSupportedException(global::SR.GetString("This stream does not support seek operations."));
		}
		set
		{
			throw new NotSupportedException(global::SR.GetString("This stream does not support seek operations."));
		}
	}

	internal bool Connected
	{
		get
		{
			Socket streamSocket = m_StreamSocket;
			if (!m_CleanedUp && streamSocket != null && streamSocket.Connected)
			{
				return true;
			}
			return false;
		}
	}

	internal NetworkStream()
	{
		m_OwnsSocket = true;
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Net.Sockets.NetworkStream" /> class for the specified <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <param name="socket">The <see cref="T:System.Net.Sockets.Socket" /> that the <see cref="T:System.Net.Sockets.NetworkStream" /> will use to send and receive data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="socket" /> parameter is null. </exception>
	/// <exception cref="T:System.IO.IOException">The <paramref name="socket" /> parameter is not connected.-or- The <see cref="P:System.Net.Sockets.Socket.SocketType" /> property of the <paramref name="socket" /> parameter is not <see cref="F:System.Net.Sockets.SocketType.Stream" />.-or- The <paramref name="socket" /> parameter is in a nonblocking state. </exception>
	public NetworkStream(Socket socket)
	{
		if (socket == null)
		{
			throw new ArgumentNullException("socket");
		}
		InitNetworkStream(socket, FileAccess.ReadWrite);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Sockets.NetworkStream" /> class for the specified <see cref="T:System.Net.Sockets.Socket" /> with the specified <see cref="T:System.Net.Sockets.Socket" /> ownership.</summary>
	/// <param name="socket">The <see cref="T:System.Net.Sockets.Socket" /> that the <see cref="T:System.Net.Sockets.NetworkStream" /> will use to send and receive data. </param>
	/// <param name="ownsSocket">Set to true to indicate that the <see cref="T:System.Net.Sockets.NetworkStream" /> will take ownership of the <see cref="T:System.Net.Sockets.Socket" />; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="socket" /> parameter is null. </exception>
	/// <exception cref="T:System.IO.IOException">The <paramref name="socket" /> parameter is not connected.-or- the value of the <see cref="P:System.Net.Sockets.Socket.SocketType" /> property of the <paramref name="socket" /> parameter is not <see cref="F:System.Net.Sockets.SocketType.Stream" />.-or- the <paramref name="socket" /> parameter is in a nonblocking state. </exception>
	public NetworkStream(Socket socket, bool ownsSocket)
	{
		if (socket == null)
		{
			throw new ArgumentNullException("socket");
		}
		InitNetworkStream(socket, FileAccess.ReadWrite);
		m_OwnsSocket = ownsSocket;
	}

	internal NetworkStream(NetworkStream networkStream, bool ownsSocket)
	{
		Socket socket = networkStream.Socket;
		if (socket == null)
		{
			throw new ArgumentNullException("networkStream");
		}
		InitNetworkStream(socket, FileAccess.ReadWrite);
		m_OwnsSocket = ownsSocket;
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Net.Sockets.NetworkStream" /> class for the specified <see cref="T:System.Net.Sockets.Socket" /> with the specified access rights.</summary>
	/// <param name="socket">The <see cref="T:System.Net.Sockets.Socket" /> that the <see cref="T:System.Net.Sockets.NetworkStream" /> will use to send and receive data. </param>
	/// <param name="access">A bitwise combination of the <see cref="T:System.IO.FileAccess" /> values that specify the type of access given to the <see cref="T:System.Net.Sockets.NetworkStream" /> over the provided <see cref="T:System.Net.Sockets.Socket" />. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="socket" /> parameter is null. </exception>
	/// <exception cref="T:System.IO.IOException">The <paramref name="socket" /> parameter is not connected.-or- the <see cref="P:System.Net.Sockets.Socket.SocketType" /> property of the <paramref name="socket" /> parameter is not <see cref="F:System.Net.Sockets.SocketType.Stream" />.-or- the <paramref name="socket" /> parameter is in a nonblocking state. </exception>
	public NetworkStream(Socket socket, FileAccess access)
	{
		if (socket == null)
		{
			throw new ArgumentNullException("socket");
		}
		InitNetworkStream(socket, access);
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Net.Sockets.NetworkStream" /> class for the specified <see cref="T:System.Net.Sockets.Socket" /> with the specified access rights and the specified <see cref="T:System.Net.Sockets.Socket" /> ownership.</summary>
	/// <param name="socket">The <see cref="T:System.Net.Sockets.Socket" /> that the <see cref="T:System.Net.Sockets.NetworkStream" /> will use to send and receive data. </param>
	/// <param name="access">A bitwise combination of the <see cref="T:System.IO.FileAccess" /> values that specifies the type of access given to the <see cref="T:System.Net.Sockets.NetworkStream" /> over the provided <see cref="T:System.Net.Sockets.Socket" />. </param>
	/// <param name="ownsSocket">Set to true to indicate that the <see cref="T:System.Net.Sockets.NetworkStream" /> will take ownership of the <see cref="T:System.Net.Sockets.Socket" />; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="socket" /> parameter is null. </exception>
	/// <exception cref="T:System.IO.IOException">The <paramref name="socket" /> parameter is not connected.-or- The <see cref="P:System.Net.Sockets.Socket.SocketType" /> property of the <paramref name="socket" /> parameter is not <see cref="F:System.Net.Sockets.SocketType.Stream" />.-or- The <paramref name="socket" /> parameter is in a nonblocking state. </exception>
	public NetworkStream(Socket socket, FileAccess access, bool ownsSocket)
	{
		if (socket == null)
		{
			throw new ArgumentNullException("socket");
		}
		InitNetworkStream(socket, access);
		m_OwnsSocket = ownsSocket;
	}

	internal void InternalAbortSocket()
	{
		if (!m_OwnsSocket)
		{
			throw new InvalidOperationException();
		}
		Socket streamSocket = m_StreamSocket;
		if (m_CleanedUp || streamSocket == null)
		{
			return;
		}
		try
		{
			streamSocket.Close(0);
		}
		catch (ObjectDisposedException)
		{
		}
	}

	internal void ConvertToNotSocketOwner()
	{
		m_OwnsSocket = false;
		GC.SuppressFinalize(this);
	}

	/// <summary>Sets the current position of the stream to the given value. This method is not currently supported and always throws a <see cref="T:System.NotSupportedException" />.</summary>
	/// <returns>The position in the stream.</returns>
	/// <param name="offset">This parameter is not used. </param>
	/// <param name="origin">This parameter is not used. </param>
	/// <exception cref="T:System.NotSupportedException">Any use of this property. </exception>
	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException(global::SR.GetString("This stream does not support seek operations."));
	}

	internal void InitNetworkStream(Socket socket, FileAccess Access)
	{
		if (!socket.Blocking)
		{
			throw new IOException(global::SR.GetString("The operation is not allowed on a non-blocking Socket."));
		}
		if (!socket.Connected)
		{
			throw new IOException(global::SR.GetString("The operation is not allowed on non-connected sockets."));
		}
		if (socket.SocketType != SocketType.Stream)
		{
			throw new IOException(global::SR.GetString("The operation is not allowed on non-stream oriented sockets."));
		}
		m_StreamSocket = socket;
		switch (Access)
		{
		case FileAccess.Read:
			m_Readable = true;
			break;
		case FileAccess.Write:
			m_Writeable = true;
			break;
		default:
			m_Readable = true;
			m_Writeable = true;
			break;
		}
	}

	internal bool PollRead()
	{
		if (m_CleanedUp)
		{
			return false;
		}
		return m_StreamSocket?.Poll(0, SelectMode.SelectRead) ?? false;
	}

	internal bool Poll(int microSeconds, SelectMode mode)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		Socket streamSocket = m_StreamSocket;
		if (streamSocket == null)
		{
			throw new IOException(global::SR.GetString("Unable to read data from the transport connection: {0}.", global::SR.GetString("The connection was closed")));
		}
		return streamSocket.Poll(microSeconds, mode);
	}

	/// <summary>Reads data from the <see cref="T:System.Net.Sockets.NetworkStream" />.</summary>
	/// <returns>The number of bytes read from the <see cref="T:System.Net.Sockets.NetworkStream" />.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the location in memory to store data read from the <see cref="T:System.Net.Sockets.NetworkStream" />. </param>
	/// <param name="offset">The location in <paramref name="buffer" /> to begin storing the data to. </param>
	/// <param name="size">The number of bytes to read from the <see cref="T:System.Net.Sockets.NetworkStream" />. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="offset" /> parameter is less than 0.-or- The <paramref name="offset" /> parameter is greater than the length of <paramref name="buffer" />.-or- The <paramref name="size" /> parameter is less than 0.-or- The <paramref name="size" /> parameter is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter. -or-An error occurred when accessing the socket. See the Remarks section for more information.</exception>
	/// <exception cref="T:System.IO.IOException">The underlying <see cref="T:System.Net.Sockets.Socket" /> is closed. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.NetworkStream" /> is closed.-or- There is a failure reading from the network. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override int Read([In][Out] byte[] buffer, int offset, int size)
	{
		bool canRead = CanRead;
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (!canRead)
		{
			throw new InvalidOperationException(global::SR.GetString("The stream does not support reading."));
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (size < 0 || size > buffer.Length - offset)
		{
			throw new ArgumentOutOfRangeException("size");
		}
		Socket streamSocket = m_StreamSocket;
		if (streamSocket == null)
		{
			throw new IOException(global::SR.GetString("Unable to read data from the transport connection: {0}.", global::SR.GetString("The connection was closed")));
		}
		try
		{
			return streamSocket.Receive(buffer, offset, size, SocketFlags.None);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			throw new IOException(global::SR.GetString("Unable to read data from the transport connection: {0}.", ex.Message), ex);
		}
	}

	/// <summary>Writes data to the <see cref="T:System.Net.Sockets.NetworkStream" />.</summary>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the data to write to the <see cref="T:System.Net.Sockets.NetworkStream" />. </param>
	/// <param name="offset">The location in <paramref name="buffer" /> from which to start writing data. </param>
	/// <param name="size">The number of bytes to write to the <see cref="T:System.Net.Sockets.NetworkStream" />. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="offset" /> parameter is less than 0.-or- The <paramref name="offset" /> parameter is greater than the length of <paramref name="buffer" />.-or- The <paramref name="size" /> parameter is less than 0.-or- The <paramref name="size" /> parameter is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter. </exception>
	/// <exception cref="T:System.IO.IOException">There was a failure while writing to the network. -or-An error occurred when accessing the socket. See the Remarks section for more information.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.NetworkStream" /> is closed.-or- There was a failure reading from the network. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Write(byte[] buffer, int offset, int size)
	{
		bool canWrite = CanWrite;
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (!canWrite)
		{
			throw new InvalidOperationException(global::SR.GetString("The stream does not support writing."));
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (size < 0 || size > buffer.Length - offset)
		{
			throw new ArgumentOutOfRangeException("size");
		}
		Socket streamSocket = m_StreamSocket;
		if (streamSocket == null)
		{
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", global::SR.GetString("The connection was closed")));
		}
		try
		{
			streamSocket.Send(buffer, offset, size, SocketFlags.None);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", ex.Message), ex);
		}
	}

	/// <summary>Closes the <see cref="T:System.Net.Sockets.NetworkStream" /> after waiting the specified time to allow data to be sent.</summary>
	/// <param name="timeout">A 32-bit signed integer that specifies the number of milliseconds to wait to send any remaining data before closing.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="timeout" /> parameter is less than -1.</exception>
	public void Close(int timeout)
	{
		if (timeout < -1)
		{
			throw new ArgumentOutOfRangeException("timeout");
		}
		m_CloseTimeout = timeout;
		Close();
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.Sockets.NetworkStream" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		bool cleanedUp = m_CleanedUp;
		m_CleanedUp = true;
		if (!cleanedUp && disposing && m_StreamSocket != null)
		{
			m_Readable = false;
			m_Writeable = false;
			if (m_OwnsSocket)
			{
				Socket streamSocket = m_StreamSocket;
				if (streamSocket != null)
				{
					streamSocket.InternalShutdown(SocketShutdown.Both);
					streamSocket.Close(m_CloseTimeout);
				}
			}
		}
		base.Dispose(disposing);
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Net.Sockets.NetworkStream" />.</summary>
	~NetworkStream()
	{
		Dispose(disposing: false);
	}

	/// <summary>Begins an asynchronous read from the <see cref="T:System.Net.Sockets.NetworkStream" />.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that represents the asynchronous call.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the location in memory to store data read from the <see cref="T:System.Net.Sockets.NetworkStream" />. </param>
	/// <param name="offset">The location in <paramref name="buffer" /> to begin storing the data. </param>
	/// <param name="size">The number of bytes to read from the <see cref="T:System.Net.Sockets.NetworkStream" />. </param>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate that is executed when <see cref="M:System.Net.Sockets.NetworkStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> completes. </param>
	/// <param name="state">An object that contains any additional user-defined data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="offset" /> parameter is less than 0.-or- The <paramref name="offset" /> parameter is greater than the length of the <paramref name="buffer" /> paramater.-or- The <paramref name="size" /> is less than 0.-or- The <paramref name="size" /> is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter.</exception>
	/// <exception cref="T:System.IO.IOException">The underlying <see cref="T:System.Net.Sockets.Socket" /> is closed.-or- There was a failure while reading from the network. -or-An error occurred when accessing the socket. See the Remarks section for more information.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.NetworkStream" /> is closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
	{
		bool canRead = CanRead;
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (!canRead)
		{
			throw new InvalidOperationException(global::SR.GetString("The stream does not support reading."));
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (size < 0 || size > buffer.Length - offset)
		{
			throw new ArgumentOutOfRangeException("size");
		}
		Socket streamSocket = m_StreamSocket;
		if (streamSocket == null)
		{
			throw new IOException(global::SR.GetString("Unable to read data from the transport connection: {0}.", global::SR.GetString("The connection was closed")));
		}
		try
		{
			return streamSocket.BeginReceive(buffer, offset, size, SocketFlags.None, callback, state);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			throw new IOException(global::SR.GetString("Unable to read data from the transport connection: {0}.", ex.Message), ex);
		}
	}

	internal virtual IAsyncResult UnsafeBeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
	{
		bool canRead = CanRead;
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (!canRead)
		{
			throw new InvalidOperationException(global::SR.GetString("The stream does not support reading."));
		}
		Socket streamSocket = m_StreamSocket;
		if (streamSocket == null)
		{
			throw new IOException(global::SR.GetString("Unable to read data from the transport connection: {0}.", global::SR.GetString("The connection was closed")));
		}
		try
		{
			return streamSocket.UnsafeBeginReceive(buffer, offset, size, SocketFlags.None, callback, state);
		}
		catch (Exception ex)
		{
			if (NclUtilities.IsFatal(ex))
			{
				throw;
			}
			throw new IOException(global::SR.GetString("Unable to read data from the transport connection: {0}.", ex.Message), ex);
		}
	}

	/// <summary>Handles the end of an asynchronous read.</summary>
	/// <returns>The number of bytes read from the <see cref="T:System.Net.Sockets.NetworkStream" />.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> that represents an asynchronous call. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="asyncResult" /> parameter is null. </exception>
	/// <exception cref="T:System.IO.IOException">The underlying <see cref="T:System.Net.Sockets.Socket" /> is closed.-or- An error occurred when accessing the socket. See the Remarks section for more information.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.NetworkStream" /> is closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override int EndRead(IAsyncResult asyncResult)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		Socket streamSocket = m_StreamSocket;
		if (streamSocket == null)
		{
			throw new IOException(global::SR.GetString("Unable to read data from the transport connection: {0}.", global::SR.GetString("The connection was closed")));
		}
		try
		{
			return streamSocket.EndReceive(asyncResult);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			throw new IOException(global::SR.GetString("Unable to read data from the transport connection: {0}.", ex.Message), ex);
		}
	}

	/// <summary>Begins an asynchronous write to a stream.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that represents the asynchronous call.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the data to write to the <see cref="T:System.Net.Sockets.NetworkStream" />. </param>
	/// <param name="offset">The location in <paramref name="buffer" /> to begin sending the data. </param>
	/// <param name="size">The number of bytes to write to the <see cref="T:System.Net.Sockets.NetworkStream" />. </param>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate that is executed when <see cref="M:System.Net.Sockets.NetworkStream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> completes. </param>
	/// <param name="state">An object that contains any additional user-defined data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="offset" /> parameter is less than 0.-or- The <paramref name="offset" /> parameter is greater than the length of <paramref name="buffer" />.-or- The <paramref name="size" /> parameter is less than 0.-or- The <paramref name="size" /> parameter is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter. </exception>
	/// <exception cref="T:System.IO.IOException">The underlying <see cref="T:System.Net.Sockets.Socket" /> is closed.-or- There was a failure while writing to the network. -or-An error occurred when accessing the socket. See the Remarks section for more information.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.NetworkStream" /> is closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
	{
		bool canWrite = CanWrite;
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (!canWrite)
		{
			throw new InvalidOperationException(global::SR.GetString("The stream does not support writing."));
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (size < 0 || size > buffer.Length - offset)
		{
			throw new ArgumentOutOfRangeException("size");
		}
		Socket streamSocket = m_StreamSocket;
		if (streamSocket == null)
		{
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", global::SR.GetString("The connection was closed")));
		}
		try
		{
			return streamSocket.BeginSend(buffer, offset, size, SocketFlags.None, callback, state);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", ex.Message), ex);
		}
	}

	internal virtual IAsyncResult UnsafeBeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
	{
		bool canWrite = CanWrite;
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (!canWrite)
		{
			throw new InvalidOperationException(global::SR.GetString("The stream does not support writing."));
		}
		Socket streamSocket = m_StreamSocket;
		if (streamSocket == null)
		{
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", global::SR.GetString("The connection was closed")));
		}
		try
		{
			return streamSocket.UnsafeBeginSend(buffer, offset, size, SocketFlags.None, callback, state);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", ex.Message), ex);
		}
	}

	/// <summary>Handles the end of an asynchronous write.</summary>
	/// <param name="asyncResult">The <see cref="T:System.IAsyncResult" /> that represents the asynchronous call. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="asyncResult" /> parameter is null. </exception>
	/// <exception cref="T:System.IO.IOException">The underlying <see cref="T:System.Net.Sockets.Socket" /> is closed.-or- An error occurred while writing to the network. -or-An error occurred when accessing the socket. See the Remarks section for more information.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.NetworkStream" /> is closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void EndWrite(IAsyncResult asyncResult)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		Socket streamSocket = m_StreamSocket;
		if (streamSocket == null)
		{
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", global::SR.GetString("The connection was closed")));
		}
		try
		{
			streamSocket.EndSend(asyncResult);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", ex.Message), ex);
		}
	}

	internal virtual void MultipleWrite(BufferOffsetSize[] buffers)
	{
		if (buffers == null)
		{
			throw new ArgumentNullException("buffers");
		}
		Socket streamSocket = m_StreamSocket;
		if (streamSocket == null)
		{
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", global::SR.GetString("The connection was closed")));
		}
		try
		{
			streamSocket.MultipleSend(buffers, SocketFlags.None);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", ex.Message), ex);
		}
	}

	internal virtual IAsyncResult BeginMultipleWrite(BufferOffsetSize[] buffers, AsyncCallback callback, object state)
	{
		if (buffers == null)
		{
			throw new ArgumentNullException("buffers");
		}
		Socket streamSocket = m_StreamSocket;
		if (streamSocket == null)
		{
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", global::SR.GetString("The connection was closed")));
		}
		try
		{
			return streamSocket.BeginMultipleSend(buffers, SocketFlags.None, callback, state);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", ex.Message), ex);
		}
	}

	internal virtual IAsyncResult UnsafeBeginMultipleWrite(BufferOffsetSize[] buffers, AsyncCallback callback, object state)
	{
		if (buffers == null)
		{
			throw new ArgumentNullException("buffers");
		}
		Socket streamSocket = m_StreamSocket;
		if (streamSocket == null)
		{
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", global::SR.GetString("The connection was closed")));
		}
		try
		{
			return streamSocket.UnsafeBeginMultipleSend(buffers, SocketFlags.None, callback, state);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", ex.Message), ex);
		}
	}

	internal virtual void EndMultipleWrite(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		Socket streamSocket = m_StreamSocket;
		if (streamSocket == null)
		{
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", global::SR.GetString("The connection was closed")));
		}
		try
		{
			streamSocket.EndMultipleSend(asyncResult);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			throw new IOException(global::SR.GetString("Unable to write data to the transport connection: {0}.", ex.Message), ex);
		}
	}

	/// <summary>Flushes data from the stream. This method is reserved for future use.</summary>
	public override void Flush()
	{
	}

	/// <summary>Flushes data from the stream as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	/// <param name="cancellationToken">A cancellation token used to propagate notification that this  operation should be canceled.</param>
	public override Task FlushAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	/// <summary>Sets the length of the stream. This method always throws a <see cref="T:System.NotSupportedException" />.</summary>
	/// <param name="value">This parameter is not used. </param>
	/// <exception cref="T:System.NotSupportedException">Any use of this property. </exception>
	public override void SetLength(long value)
	{
		throw new NotSupportedException(global::SR.GetString("This stream does not support seek operations."));
	}

	internal void SetSocketTimeoutOption(SocketShutdown mode, int timeout, bool silent)
	{
		if (timeout < 0)
		{
			timeout = 0;
		}
		Socket streamSocket = m_StreamSocket;
		if (streamSocket != null)
		{
			if ((mode == SocketShutdown.Send || mode == SocketShutdown.Both) && timeout != m_CurrentWriteTimeout)
			{
				streamSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout, silent);
				m_CurrentWriteTimeout = timeout;
			}
			if ((mode == SocketShutdown.Receive || mode == SocketShutdown.Both) && timeout != m_CurrentReadTimeout)
			{
				streamSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout, silent);
				m_CurrentReadTimeout = timeout;
			}
		}
	}
}
