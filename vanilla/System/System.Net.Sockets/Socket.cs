using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Configuration;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using Mono;

namespace System.Net.Sockets;

/// <summary>Implements the Berkeley sockets interface.</summary>
public class Socket : IDisposable
{
	private delegate void SendFileHandler(string fileName, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags);

	private sealed class SendFileAsyncResult : IAsyncResult
	{
		private IAsyncResult ares;

		private SendFileHandler d;

		public object AsyncState => ares.AsyncState;

		public WaitHandle AsyncWaitHandle => ares.AsyncWaitHandle;

		public bool CompletedSynchronously => ares.CompletedSynchronously;

		public bool IsCompleted => ares.IsCompleted;

		public SendFileHandler Delegate => d;

		public IAsyncResult Original => ares;

		public SendFileAsyncResult(SendFileHandler d, IAsyncResult ares)
		{
			this.d = d;
			this.ares = ares;
		}
	}

	private struct WSABUF
	{
		public int len;

		public IntPtr buf;
	}

	private static object s_InternalSyncObject;

	internal static volatile bool s_SupportsIPv4;

	internal static volatile bool s_SupportsIPv6;

	internal static volatile bool s_OSSupportsIPv6;

	internal static volatile bool s_Initialized;

	private static volatile bool s_LoggingEnabled;

	internal static volatile bool s_PerfCountersEnabled;

	internal const int DefaultCloseTimeout = -1;

	private const int SOCKET_CLOSED_CODE = 10004;

	private const string TIMEOUT_EXCEPTION_MSG = "A connection attempt failed because the connected party did not properly respondafter a period of time, or established connection failed because connected host has failed to respond";

	private bool is_closed;

	private bool is_listening;

	private bool useOverlappedIO;

	private int linger_timeout;

	private AddressFamily addressFamily;

	private SocketType socketType;

	private ProtocolType protocolType;

	internal SafeSocketHandle m_Handle;

	internal EndPoint seed_endpoint;

	internal SemaphoreSlim ReadSem = new SemaphoreSlim(1, 1);

	internal SemaphoreSlim WriteSem = new SemaphoreSlim(1, 1);

	internal bool is_blocking = true;

	internal bool is_bound;

	internal bool is_connected;

	private int m_IntCleanedUp;

	internal bool connect_in_progress;

	private static AsyncCallback AcceptAsyncCallback = delegate(IAsyncResult ares)
	{
		SocketAsyncEventArgs socketAsyncEventArgs = (SocketAsyncEventArgs)((SocketAsyncResult)ares).AsyncState;
		if (Interlocked.Exchange(ref socketAsyncEventArgs.in_progress, 0) != 1)
		{
			throw new InvalidOperationException("No operation in progress");
		}
		try
		{
			socketAsyncEventArgs.AcceptSocket = socketAsyncEventArgs.current_socket.EndAccept(ares);
		}
		catch (SocketException ex)
		{
			socketAsyncEventArgs.SocketError = ex.SocketErrorCode;
		}
		catch (ObjectDisposedException)
		{
			socketAsyncEventArgs.SocketError = SocketError.OperationAborted;
		}
		finally
		{
			if (socketAsyncEventArgs.AcceptSocket == null)
			{
				socketAsyncEventArgs.AcceptSocket = new Socket(socketAsyncEventArgs.current_socket.AddressFamily, socketAsyncEventArgs.current_socket.SocketType, socketAsyncEventArgs.current_socket.ProtocolType, null);
			}
			socketAsyncEventArgs.Complete();
		}
	};

	private static IOAsyncCallback BeginAcceptCallback = delegate(IOAsyncResult ares)
	{
		SocketAsyncResult socketAsyncResult = (SocketAsyncResult)ares;
		Socket socket = null;
		try
		{
			if (socketAsyncResult.AcceptSocket == null)
			{
				socket = socketAsyncResult.socket.Accept();
			}
			else
			{
				socket = socketAsyncResult.AcceptSocket;
				socketAsyncResult.socket.Accept(socket);
			}
		}
		catch (Exception e)
		{
			socketAsyncResult.Complete(e);
			return;
		}
		socketAsyncResult.Complete(socket);
	};

	private static IOAsyncCallback BeginAcceptReceiveCallback = delegate(IOAsyncResult ares)
	{
		SocketAsyncResult socketAsyncResult2 = (SocketAsyncResult)ares;
		Socket socket2 = null;
		try
		{
			if (socketAsyncResult2.AcceptSocket == null)
			{
				socket2 = socketAsyncResult2.socket.Accept();
			}
			else
			{
				socket2 = socketAsyncResult2.AcceptSocket;
				socketAsyncResult2.socket.Accept(socket2);
			}
		}
		catch (Exception e2)
		{
			socketAsyncResult2.Complete(e2);
			return;
		}
		int total = 0;
		if (socketAsyncResult2.Size > 0)
		{
			try
			{
				total = socket2.Receive(socketAsyncResult2.Buffer, socketAsyncResult2.Offset, socketAsyncResult2.Size, socketAsyncResult2.SockFlags, out var errorCode);
				if (errorCode != 0)
				{
					socketAsyncResult2.Complete(new SocketException((int)errorCode));
					return;
				}
			}
			catch (Exception e3)
			{
				socketAsyncResult2.Complete(e3);
				return;
			}
		}
		socketAsyncResult2.Complete(socket2, total);
	};

	private static AsyncCallback ConnectAsyncCallback = delegate(IAsyncResult ares)
	{
		SocketAsyncEventArgs socketAsyncEventArgs2 = (SocketAsyncEventArgs)((SocketAsyncResult)ares).AsyncState;
		if (Interlocked.Exchange(ref socketAsyncEventArgs2.in_progress, 0) != 1)
		{
			throw new InvalidOperationException("No operation in progress");
		}
		try
		{
			socketAsyncEventArgs2.current_socket.EndConnect(ares);
		}
		catch (SocketException ex3)
		{
			socketAsyncEventArgs2.SocketError = ex3.SocketErrorCode;
		}
		catch (ObjectDisposedException)
		{
			socketAsyncEventArgs2.SocketError = SocketError.OperationAborted;
		}
		finally
		{
			socketAsyncEventArgs2.Complete();
		}
	};

	private static IOAsyncCallback BeginConnectCallback = delegate(IOAsyncResult ares)
	{
		SocketAsyncResult socketAsyncResult3 = (SocketAsyncResult)ares;
		if (socketAsyncResult3.EndPoint == null)
		{
			socketAsyncResult3.Complete(new SocketException(10049));
			return;
		}
		try
		{
			int num = (int)socketAsyncResult3.socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Error);
			if (num == 0)
			{
				socketAsyncResult3.socket.seed_endpoint = socketAsyncResult3.EndPoint;
				socketAsyncResult3.socket.is_connected = true;
				socketAsyncResult3.socket.is_bound = true;
				socketAsyncResult3.socket.connect_in_progress = false;
				socketAsyncResult3.error = 0;
				socketAsyncResult3.Complete();
			}
			else if (socketAsyncResult3.Addresses == null)
			{
				socketAsyncResult3.socket.connect_in_progress = false;
				socketAsyncResult3.Complete(new SocketException(num));
			}
			else if (socketAsyncResult3.CurrentAddress >= socketAsyncResult3.Addresses.Length)
			{
				socketAsyncResult3.Complete(new SocketException(num));
			}
			else
			{
				BeginMConnect(socketAsyncResult3);
			}
		}
		catch (Exception e4)
		{
			socketAsyncResult3.socket.connect_in_progress = false;
			socketAsyncResult3.Complete(e4);
		}
	};

	private static AsyncCallback DisconnectAsyncCallback = delegate(IAsyncResult ares)
	{
		SocketAsyncEventArgs socketAsyncEventArgs3 = (SocketAsyncEventArgs)((SocketAsyncResult)ares).AsyncState;
		if (Interlocked.Exchange(ref socketAsyncEventArgs3.in_progress, 0) != 1)
		{
			throw new InvalidOperationException("No operation in progress");
		}
		try
		{
			socketAsyncEventArgs3.current_socket.EndDisconnect(ares);
		}
		catch (SocketException ex5)
		{
			socketAsyncEventArgs3.SocketError = ex5.SocketErrorCode;
		}
		catch (ObjectDisposedException)
		{
			socketAsyncEventArgs3.SocketError = SocketError.OperationAborted;
		}
		finally
		{
			socketAsyncEventArgs3.Complete();
		}
	};

	private static IOAsyncCallback BeginDisconnectCallback = delegate(IOAsyncResult ares)
	{
		SocketAsyncResult socketAsyncResult4 = (SocketAsyncResult)ares;
		try
		{
			socketAsyncResult4.socket.Disconnect(socketAsyncResult4.ReuseSocket);
		}
		catch (Exception e5)
		{
			socketAsyncResult4.Complete(e5);
			return;
		}
		socketAsyncResult4.Complete();
	};

	private static AsyncCallback ReceiveAsyncCallback = delegate(IAsyncResult ares)
	{
		SocketAsyncEventArgs socketAsyncEventArgs4 = (SocketAsyncEventArgs)((SocketAsyncResult)ares).AsyncState;
		if (Interlocked.Exchange(ref socketAsyncEventArgs4.in_progress, 0) != 1)
		{
			throw new InvalidOperationException("No operation in progress");
		}
		try
		{
			socketAsyncEventArgs4.BytesTransferred = socketAsyncEventArgs4.current_socket.EndReceive(ares);
		}
		catch (SocketException ex7)
		{
			socketAsyncEventArgs4.SocketError = ex7.SocketErrorCode;
		}
		catch (ObjectDisposedException)
		{
			socketAsyncEventArgs4.SocketError = SocketError.OperationAborted;
		}
		finally
		{
			socketAsyncEventArgs4.Complete();
		}
	};

	private unsafe static IOAsyncCallback BeginReceiveCallback = delegate(IOAsyncResult ares)
	{
		SocketAsyncResult socketAsyncResult5 = (SocketAsyncResult)ares;
		int total2 = 0;
		try
		{
			fixed (byte* buffer = socketAsyncResult5.Buffer)
			{
				total2 = Receive_internal(socketAsyncResult5.socket.m_Handle, buffer + socketAsyncResult5.Offset, socketAsyncResult5.Size, socketAsyncResult5.SockFlags, out socketAsyncResult5.error, socketAsyncResult5.socket.is_blocking);
			}
		}
		catch (Exception e6)
		{
			socketAsyncResult5.Complete(e6);
			return;
		}
		socketAsyncResult5.Complete(total2);
	};

	private static IOAsyncCallback BeginReceiveGenericCallback = delegate(IOAsyncResult ares)
	{
		SocketAsyncResult socketAsyncResult6 = (SocketAsyncResult)ares;
		int num2 = 0;
		try
		{
			num2 = socketAsyncResult6.socket.Receive(socketAsyncResult6.Buffers, socketAsyncResult6.SockFlags);
		}
		catch (Exception e7)
		{
			socketAsyncResult6.Complete(e7);
			return;
		}
		socketAsyncResult6.Complete(num2);
	};

	private static AsyncCallback ReceiveFromAsyncCallback = delegate(IAsyncResult ares)
	{
		SocketAsyncEventArgs socketAsyncEventArgs5 = (SocketAsyncEventArgs)((SocketAsyncResult)ares).AsyncState;
		if (Interlocked.Exchange(ref socketAsyncEventArgs5.in_progress, 0) != 1)
		{
			throw new InvalidOperationException("No operation in progress");
		}
		try
		{
			socketAsyncEventArgs5.BytesTransferred = socketAsyncEventArgs5.current_socket.EndReceiveFrom(ares, ref socketAsyncEventArgs5.remote_ep);
		}
		catch (SocketException ex9)
		{
			socketAsyncEventArgs5.SocketError = ex9.SocketErrorCode;
		}
		catch (ObjectDisposedException)
		{
			socketAsyncEventArgs5.SocketError = SocketError.OperationAborted;
		}
		finally
		{
			socketAsyncEventArgs5.Complete();
		}
	};

	private static IOAsyncCallback BeginReceiveFromCallback = delegate(IOAsyncResult ares)
	{
		SocketAsyncResult socketAsyncResult7 = (SocketAsyncResult)ares;
		int num3 = 0;
		try
		{
			num3 = socketAsyncResult7.socket.ReceiveFrom(socketAsyncResult7.Buffer, socketAsyncResult7.Offset, socketAsyncResult7.Size, socketAsyncResult7.SockFlags, ref socketAsyncResult7.EndPoint, out var errorCode2);
			if (errorCode2 != 0)
			{
				socketAsyncResult7.Complete(new SocketException(errorCode2));
				return;
			}
		}
		catch (Exception e8)
		{
			socketAsyncResult7.Complete(e8);
			return;
		}
		socketAsyncResult7.Complete(num3);
	};

	private static AsyncCallback SendAsyncCallback = delegate(IAsyncResult ares)
	{
		SocketAsyncEventArgs socketAsyncEventArgs6 = (SocketAsyncEventArgs)((SocketAsyncResult)ares).AsyncState;
		if (Interlocked.Exchange(ref socketAsyncEventArgs6.in_progress, 0) != 1)
		{
			throw new InvalidOperationException("No operation in progress");
		}
		try
		{
			socketAsyncEventArgs6.BytesTransferred = socketAsyncEventArgs6.current_socket.EndSend(ares);
		}
		catch (SocketException ex11)
		{
			socketAsyncEventArgs6.SocketError = ex11.SocketErrorCode;
		}
		catch (ObjectDisposedException)
		{
			socketAsyncEventArgs6.SocketError = SocketError.OperationAborted;
		}
		finally
		{
			socketAsyncEventArgs6.Complete();
		}
	};

	private static IOAsyncCallback BeginSendGenericCallback = delegate(IOAsyncResult ares)
	{
		SocketAsyncResult socketAsyncResult8 = (SocketAsyncResult)ares;
		int num4 = 0;
		try
		{
			num4 = socketAsyncResult8.socket.Send(socketAsyncResult8.Buffers, socketAsyncResult8.SockFlags);
		}
		catch (Exception e9)
		{
			socketAsyncResult8.Complete(e9);
			return;
		}
		socketAsyncResult8.Complete(num4);
	};

	private static AsyncCallback SendToAsyncCallback = delegate(IAsyncResult ares)
	{
		SocketAsyncEventArgs socketAsyncEventArgs7 = (SocketAsyncEventArgs)((SocketAsyncResult)ares).AsyncState;
		if (Interlocked.Exchange(ref socketAsyncEventArgs7.in_progress, 0) != 1)
		{
			throw new InvalidOperationException("No operation in progress");
		}
		try
		{
			socketAsyncEventArgs7.BytesTransferred = socketAsyncEventArgs7.current_socket.EndSendTo(ares);
		}
		catch (SocketException ex13)
		{
			socketAsyncEventArgs7.SocketError = ex13.SocketErrorCode;
		}
		catch (ObjectDisposedException)
		{
			socketAsyncEventArgs7.SocketError = SocketError.OperationAborted;
		}
		finally
		{
			socketAsyncEventArgs7.Complete();
		}
	};

	/// <summary>Gets a value indicating whether IPv4 support is available and enabled on the current host.</summary>
	/// <returns>true if the current host supports the IPv4 protocol; otherwise, false.</returns>
	[Obsolete("SupportsIPv4 is obsoleted for this type, please use OSSupportsIPv4 instead. http://go.microsoft.com/fwlink/?linkid=14202")]
	public static bool SupportsIPv4
	{
		get
		{
			InitializeSockets();
			return s_SupportsIPv4;
		}
	}

	/// <summary>Indicates whether the underlying operating system and network adaptors support Internet Protocol version 4 (IPv4).</summary>
	/// <returns>true if the operating system and network adaptors support the IPv4 protocol; otherwise, false.</returns>
	public static bool OSSupportsIPv4
	{
		get
		{
			InitializeSockets();
			return s_SupportsIPv4;
		}
	}

	/// <summary>Gets a value that indicates whether the Framework supports IPv6 for certain obsolete <see cref="T:System.Net.Dns" /> members.</summary>
	/// <returns>true if the Framework supports IPv6 for certain obsolete <see cref="T:System.Net.Dns" /> methods; otherwise, false.</returns>
	[Obsolete("SupportsIPv6 is obsoleted for this type, please use OSSupportsIPv6 instead. http://go.microsoft.com/fwlink/?linkid=14202")]
	public static bool SupportsIPv6
	{
		get
		{
			InitializeSockets();
			return s_SupportsIPv6;
		}
	}

	internal static bool LegacySupportsIPv6
	{
		get
		{
			InitializeSockets();
			return s_SupportsIPv6;
		}
	}

	/// <summary>Indicates whether the underlying operating system and network adaptors support Internet Protocol version 6 (IPv6).</summary>
	/// <returns>true if the operating system and network adaptors support the IPv6 protocol; otherwise, false.</returns>
	public static bool OSSupportsIPv6
	{
		get
		{
			InitializeSockets();
			return s_OSSupportsIPv6;
		}
	}

	/// <summary>Gets the operating system handle for the <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>An <see cref="T:System.IntPtr" /> that represents the operating system handle for the <see cref="T:System.Net.Sockets.Socket" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public IntPtr Handle => m_Handle.DangerousGetHandle();

	/// <summary>Specifies whether the socket should only use Overlapped I/O mode.</summary>
	/// <returns>true if the <see cref="T:System.Net.Sockets.Socket" /> uses only overlapped I/O; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The socket has been bound to a completion port.</exception>
	public bool UseOnlyOverlappedIO
	{
		get
		{
			return useOverlappedIO;
		}
		set
		{
			useOverlappedIO = value;
		}
	}

	/// <summary>Gets the address family of the <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>One of the <see cref="T:System.Net.Sockets.AddressFamily" /> values.</returns>
	public AddressFamily AddressFamily => addressFamily;

	/// <summary>Gets the type of the <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>One of the <see cref="T:System.Net.Sockets.SocketType" /> values.</returns>
	public SocketType SocketType => socketType;

	/// <summary>Gets the protocol type of the <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>One of the <see cref="T:System.Net.Sockets.ProtocolType" /> values.</returns>
	public ProtocolType ProtocolType => protocolType;

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Net.Sockets.Socket" /> allows only one process to bind to a port.</summary>
	/// <returns>true if the <see cref="T:System.Net.Sockets.Socket" /> allows only one socket to bind to a specific port; otherwise, false. The default is true for Windows Server 2003 and Windows XP Service Pack 2, and false for all other versions.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.Socket.Bind(System.Net.EndPoint)" /> has been called for this <see cref="T:System.Net.Sockets.Socket" />.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool ExclusiveAddressUse
	{
		get
		{
			if ((int)GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse) == 0)
			{
				return false;
			}
			return true;
		}
		set
		{
			if (IsBound)
			{
				throw new InvalidOperationException(global::SR.GetString("The socket must not be bound or connected."));
			}
			SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, value ? 1 : 0);
		}
	}

	/// <summary>Gets or sets a value that specifies the size of the receive buffer of the <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>An <see cref="T:System.Int32" /> that contains the size, in bytes, of the receive buffer. The default is 8192.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for a set operation is less than 0.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int ReceiveBufferSize
	{
		get
		{
			return (int)GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer);
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the size of the send buffer of the <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>An <see cref="T:System.Int32" /> that contains the size, in bytes, of the send buffer. The default is 8192.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for a set operation is less than 0.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int SendBufferSize
	{
		get
		{
			return (int)GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer);
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the amount of time after which a synchronous <see cref="Overload:System.Net.Sockets.Socket.Receive" /> call will time out.</summary>
	/// <returns>The time-out value, in milliseconds. The default value is 0, which indicates an infinite time-out period. Specifying -1 also indicates an infinite time-out period.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for a set operation is less than -1.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int ReceiveTimeout
	{
		get
		{
			return (int)GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
		}
		set
		{
			if (value < -1)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (value == -1)
			{
				value = 0;
			}
			SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the amount of time after which a synchronous <see cref="Overload:System.Net.Sockets.Socket.Send" /> call will time out.</summary>
	/// <returns>The time-out value, in milliseconds. If you set the property with a value between 1 and 499, the value will be changed to 500. The default value is 0, which indicates an infinite time-out period. Specifying -1 also indicates an infinite time-out period.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for a set operation is less than -1.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int SendTimeout
	{
		get
		{
			return (int)GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
		}
		set
		{
			if (value < -1)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (value == -1)
			{
				value = 0;
			}
			SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value);
		}
	}

	/// <summary>Gets or sets a value that specifies whether the <see cref="T:System.Net.Sockets.Socket" /> will delay closing a socket in an attempt to send all pending data.</summary>
	/// <returns>A <see cref="T:System.Net.Sockets.LingerOption" /> that specifies how to linger while closing a socket.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public LingerOption LingerState
	{
		get
		{
			return (LingerOption)GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger);
		}
		set
		{
			SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the Time To Live (TTL) value of Internet Protocol (IP) packets sent by the <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>The TTL value.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The TTL value can't be set to a negative number.</exception>
	/// <exception cref="T:System.NotSupportedException">This property can be set only for sockets in the <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork" /> or <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" /> families.</exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. This error is also returned when an attempt was made to set TTL to a value higher than 255.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public short Ttl
	{
		get
		{
			if (addressFamily == AddressFamily.InterNetwork)
			{
				return (short)(int)GetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress);
			}
			if (addressFamily == AddressFamily.InterNetworkV6)
			{
				return (short)(int)GetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.ReuseAddress);
			}
			throw new NotSupportedException(global::SR.GetString("This protocol version is not supported."));
		}
		set
		{
			if (value < 0 || value > 255)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (addressFamily == AddressFamily.InterNetwork)
			{
				SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, value);
				return;
			}
			if (addressFamily == AddressFamily.InterNetworkV6)
			{
				SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.ReuseAddress, value);
				return;
			}
			throw new NotSupportedException(global::SR.GetString("This protocol version is not supported."));
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Net.Sockets.Socket" /> allows Internet Protocol (IP) datagrams to be fragmented.</summary>
	/// <returns>true if the <see cref="T:System.Net.Sockets.Socket" /> allows datagram fragmentation; otherwise, false. The default is true.</returns>
	/// <exception cref="T:System.NotSupportedException">This property can be set only for sockets in the <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork" /> or <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" /> families. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool DontFragment
	{
		get
		{
			if (addressFamily == AddressFamily.InterNetwork)
			{
				if ((int)GetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment) == 0)
				{
					return false;
				}
				return true;
			}
			throw new NotSupportedException(global::SR.GetString("This protocol version is not supported."));
		}
		set
		{
			if (addressFamily == AddressFamily.InterNetwork)
			{
				SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment, value ? 1 : 0);
				return;
			}
			throw new NotSupportedException(global::SR.GetString("This protocol version is not supported."));
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Net.Sockets.Socket" /> is a dual-mode socket used for both IPv4 and IPv6.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the <see cref="T:System.Net.Sockets.Socket" /> is a  dual-mode socket; otherwise, false. The default is false.</returns>
	public bool DualMode
	{
		get
		{
			if (AddressFamily != AddressFamily.InterNetworkV6)
			{
				throw new NotSupportedException(global::SR.GetString("This protocol version is not supported."));
			}
			return (int)GetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only) == 0;
		}
		set
		{
			if (AddressFamily != AddressFamily.InterNetworkV6)
			{
				throw new NotSupportedException(global::SR.GetString("This protocol version is not supported."));
			}
			SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, (!value) ? 1 : 0);
		}
	}

	private bool IsDualMode
	{
		get
		{
			if (AddressFamily == AddressFamily.InterNetworkV6)
			{
				return DualMode;
			}
			return false;
		}
	}

	private static object InternalSyncObject
	{
		get
		{
			if (s_InternalSyncObject == null)
			{
				object value = new object();
				Interlocked.CompareExchange(ref s_InternalSyncObject, value, null);
			}
			return s_InternalSyncObject;
		}
	}

	internal bool CleanedUp => m_IntCleanedUp == 1;

	/// <summary>Gets the amount of data that has been received from the network and is available to be read.</summary>
	/// <returns>The number of bytes of data received from the network and available to be read.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public int Available
	{
		get
		{
			ThrowIfDisposedAndClosed();
			int error;
			int result = Available_internal(m_Handle, out error);
			if (error != 0)
			{
				throw new SocketException(error);
			}
			return result;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Net.Sockets.Socket" /> can send or receive broadcast packets.</summary>
	/// <returns>true if the <see cref="T:System.Net.Sockets.Socket" /> allows broadcast packets; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">This option is valid for a datagram socket only. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool EnableBroadcast
	{
		get
		{
			ThrowIfDisposedAndClosed();
			if (protocolType != ProtocolType.Udp)
			{
				throw new SocketException(10042);
			}
			return (int)GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast) != 0;
		}
		set
		{
			ThrowIfDisposedAndClosed();
			if (protocolType != ProtocolType.Udp)
			{
				throw new SocketException(10042);
			}
			SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, value ? 1 : 0);
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Net.Sockets.Socket" /> is bound to a specific local port.</summary>
	/// <returns>true if the <see cref="T:System.Net.Sockets.Socket" /> is bound to a local port; otherwise, false.</returns>
	public bool IsBound => is_bound;

	/// <summary>Gets or sets a value that specifies whether outgoing multicast packets are delivered to the sending application.</summary>
	/// <returns>true if the <see cref="T:System.Net.Sockets.Socket" /> receives outgoing multicast packets; otherwise, false.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool MulticastLoopback
	{
		get
		{
			ThrowIfDisposedAndClosed();
			if (protocolType == ProtocolType.Tcp)
			{
				throw new SocketException(10042);
			}
			return addressFamily switch
			{
				AddressFamily.InterNetwork => (int)GetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback) != 0, 
				AddressFamily.InterNetworkV6 => (int)GetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastLoopback) != 0, 
				_ => throw new NotSupportedException("This property is only valid for InterNetwork and InterNetworkV6 sockets"), 
			};
		}
		set
		{
			ThrowIfDisposedAndClosed();
			if (protocolType == ProtocolType.Tcp)
			{
				throw new SocketException(10042);
			}
			switch (addressFamily)
			{
			case AddressFamily.InterNetwork:
				SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, value ? 1 : 0);
				break;
			case AddressFamily.InterNetworkV6:
				SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastLoopback, value ? 1 : 0);
				break;
			default:
				throw new NotSupportedException("This property is only valid for InterNetwork and InterNetworkV6 sockets");
			}
		}
	}

	/// <summary>Gets the local endpoint.</summary>
	/// <returns>The <see cref="T:System.Net.EndPoint" /> that the <see cref="T:System.Net.Sockets.Socket" /> is using for communications.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public EndPoint LocalEndPoint
	{
		get
		{
			ThrowIfDisposedAndClosed();
			if (seed_endpoint == null)
			{
				return null;
			}
			int error;
			SocketAddress socketAddress = LocalEndPoint_internal(m_Handle, (int)addressFamily, out error);
			if (error != 0)
			{
				throw new SocketException(error);
			}
			return seed_endpoint.Create(socketAddress);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Net.Sockets.Socket" /> is in blocking mode.</summary>
	/// <returns>true if the <see cref="T:System.Net.Sockets.Socket" /> will block; otherwise, false. The default is true.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool Blocking
	{
		get
		{
			return is_blocking;
		}
		set
		{
			ThrowIfDisposedAndClosed();
			Blocking_internal(m_Handle, value, out var error);
			if (error != 0)
			{
				throw new SocketException(error);
			}
			is_blocking = value;
		}
	}

	/// <summary>Gets a value that indicates whether a <see cref="T:System.Net.Sockets.Socket" /> is connected to a remote host as of the last <see cref="Overload:System.Net.Sockets.Socket.Send" /> or <see cref="Overload:System.Net.Sockets.Socket.Receive" /> operation.</summary>
	/// <returns>true if the <see cref="T:System.Net.Sockets.Socket" /> was connected to a remote resource as of the most recent operation; otherwise, false.</returns>
	public bool Connected
	{
		get
		{
			return is_connected;
		}
		internal set
		{
			is_connected = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether the stream <see cref="T:System.Net.Sockets.Socket" /> is using the Nagle algorithm.</summary>
	/// <returns>false if the <see cref="T:System.Net.Sockets.Socket" /> uses the Nagle algorithm; otherwise, true. The default is false.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the <see cref="T:System.Net.Sockets.Socket" />. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool NoDelay
	{
		get
		{
			ThrowIfDisposedAndClosed();
			ThrowIfUdp();
			return (int)GetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug) != 0;
		}
		set
		{
			ThrowIfDisposedAndClosed();
			ThrowIfUdp();
			SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, value ? 1 : 0);
		}
	}

	/// <summary>Gets the remote endpoint.</summary>
	/// <returns>The <see cref="T:System.Net.EndPoint" /> with which the <see cref="T:System.Net.Sockets.Socket" /> is communicating.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public EndPoint RemoteEndPoint
	{
		get
		{
			ThrowIfDisposedAndClosed();
			if (!is_connected || seed_endpoint == null)
			{
				return null;
			}
			int error;
			SocketAddress socketAddress = RemoteEndPoint_internal(m_Handle, (int)addressFamily, out error);
			if (error != 0)
			{
				throw new SocketException(error);
			}
			return seed_endpoint.Create(socketAddress);
		}
	}

	internal static int FamilyHint
	{
		get
		{
			int num = 0;
			if (OSSupportsIPv4)
			{
				num = 1;
			}
			if (OSSupportsIPv6)
			{
				num = ((num == 0) ? 2 : 0);
			}
			return num;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Sockets.Socket" /> class using the specified socket type and protocol.</summary>
	/// <param name="socketType">One of the <see cref="T:System.Net.Sockets.SocketType" /> values.</param>
	/// <param name="protocolType">One of the <see cref="T:System.Net.Sockets.ProtocolType" /> values.</param>
	/// <exception cref="T:System.Net.Sockets.SocketException">The combination of  <paramref name="socketType" /> and <paramref name="protocolType" /> results in an invalid socket. </exception>
	public Socket(SocketType socketType, ProtocolType protocolType)
		: this(AddressFamily.InterNetworkV6, socketType, protocolType)
	{
		DualMode = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Sockets.Socket" /> class using the specified address family, socket type and protocol.</summary>
	/// <param name="addressFamily">One of the <see cref="T:System.Net.Sockets.AddressFamily" /> values. </param>
	/// <param name="socketType">One of the <see cref="T:System.Net.Sockets.SocketType" /> values. </param>
	/// <param name="protocolType">One of the <see cref="T:System.Net.Sockets.ProtocolType" /> values. </param>
	/// <exception cref="T:System.Net.Sockets.SocketException">The combination of <paramref name="addressFamily" />, <paramref name="socketType" />, and <paramref name="protocolType" /> results in an invalid socket. </exception>
	public Socket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
	{
		s_LoggingEnabled = Logging.On;
		_ = s_LoggingEnabled;
		InitializeSockets();
		m_Handle = new SafeSocketHandle(Socket_internal(addressFamily, socketType, protocolType, out var _), ownsHandle: true);
		if (m_Handle.IsInvalid)
		{
			throw new SocketException();
		}
		this.addressFamily = addressFamily;
		this.socketType = socketType;
		this.protocolType = protocolType;
		IPProtectionLevel iPProtectionLevel = SettingsSectionInternal.Section.IPProtectionLevel;
		if (iPProtectionLevel != IPProtectionLevel.Unspecified)
		{
			SetIPProtectionLevel(iPProtectionLevel);
		}
		SocketDefaults();
		_ = s_LoggingEnabled;
	}

	internal bool CanTryAddressFamily(AddressFamily family)
	{
		if (family != addressFamily)
		{
			if (family == AddressFamily.InterNetwork)
			{
				return IsDualMode;
			}
			return false;
		}
		return true;
	}

	/// <summary>Establishes a connection to a remote host. The host is specified by an array of IP addresses and a port number.</summary>
	/// <param name="addresses">The IP addresses of the remote host.</param>
	/// <param name="port">The port number of the remote host.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="addresses" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The port number is not valid.</exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.NotSupportedException">This method is valid for sockets in the <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork" /> or <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" /> families.</exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="address" /> is zero.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.Socket" /> is <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" />ing.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Connect(IPAddress[] addresses, int port)
	{
		_ = s_LoggingEnabled;
		if (CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (addresses == null)
		{
			throw new ArgumentNullException("addresses");
		}
		if (addresses.Length == 0)
		{
			throw new ArgumentException(global::SR.GetString("The number of specified IP addresses has to be greater than 0."), "addresses");
		}
		if (!ValidationHelper.ValidateTcpPort(port))
		{
			throw new ArgumentOutOfRangeException("port");
		}
		if (addressFamily != AddressFamily.InterNetwork && addressFamily != AddressFamily.InterNetworkV6)
		{
			throw new NotSupportedException(global::SR.GetString("This protocol version is not supported."));
		}
		Exception ex = null;
		foreach (IPAddress iPAddress in addresses)
		{
			if (!CanTryAddressFamily(iPAddress.AddressFamily))
			{
				continue;
			}
			try
			{
				Connect(new IPEndPoint(iPAddress, port));
				ex = null;
			}
			catch (Exception ex2)
			{
				if (NclUtilities.IsFatal(ex2))
				{
					throw;
				}
				ex = ex2;
				continue;
			}
			break;
		}
		if (ex != null)
		{
			throw ex;
		}
		if (!Connected)
		{
			throw new ArgumentException(global::SR.GetString("None of the discovered or specified addresses match the socket address family."), "addresses");
		}
		_ = s_LoggingEnabled;
	}

	/// <summary>Sends the specified number of bytes of data to a connected <see cref="T:System.Net.Sockets.Socket" />, using the specified <see cref="T:System.Net.Sockets.SocketFlags" />.</summary>
	/// <returns>The number of bytes sent to the <see cref="T:System.Net.Sockets.Socket" />.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the data to be sent. </param>
	/// <param name="size">The number of bytes to send. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="size" /> is less than 0 or exceeds the size of the buffer. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">
	///   <paramref name="socketFlags" /> is not a valid combination of values.-or- An operating system error occurs while accessing the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int Send(byte[] buffer, int size, SocketFlags socketFlags)
	{
		return Send(buffer, 0, size, socketFlags);
	}

	/// <summary>Sends data to a connected <see cref="T:System.Net.Sockets.Socket" /> using the specified <see cref="T:System.Net.Sockets.SocketFlags" />.</summary>
	/// <returns>The number of bytes sent to the <see cref="T:System.Net.Sockets.Socket" />.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the data to be sent. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int Send(byte[] buffer, SocketFlags socketFlags)
	{
		return Send(buffer, 0, (buffer != null) ? buffer.Length : 0, socketFlags);
	}

	/// <summary>Sends data to a connected <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>The number of bytes sent to the <see cref="T:System.Net.Sockets.Socket" />.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the data to be sent. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int Send(byte[] buffer)
	{
		return Send(buffer, 0, (buffer != null) ? buffer.Length : 0, SocketFlags.None);
	}

	/// <summary>Sends the set of buffers in the list to a connected <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>The number of bytes sent to the <see cref="T:System.Net.Sockets.Socket" />.</returns>
	/// <param name="buffers">A list of <see cref="T:System.ArraySegment`1" />s of type <see cref="T:System.Byte" /> that contains the data to be sent. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffers" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="buffers" /> is empty.</exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See remarks section below. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	public int Send(IList<ArraySegment<byte>> buffers)
	{
		return Send(buffers, SocketFlags.None);
	}

	/// <summary>Sends the set of buffers in the list to a connected <see cref="T:System.Net.Sockets.Socket" />, using the specified <see cref="T:System.Net.Sockets.SocketFlags" />.</summary>
	/// <returns>The number of bytes sent to the <see cref="T:System.Net.Sockets.Socket" />.</returns>
	/// <param name="buffers">A list of <see cref="T:System.ArraySegment`1" />s of type <see cref="T:System.Byte" /> that contains the data to be sent.</param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffers" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="buffers" /> is empty.</exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	public int Send(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags)
	{
		SocketError errorCode;
		int result = Send(buffers, socketFlags, out errorCode);
		if (errorCode != 0)
		{
			throw new SocketException(errorCode);
		}
		return result;
	}

	/// <summary>Sends the file <paramref name="fileName" /> to a connected <see cref="T:System.Net.Sockets.Socket" /> object with the <see cref="F:System.Net.Sockets.TransmitFileOptions.UseDefaultWorkerThread" /> transmit flag.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that contains the path and name of the file to be sent. This parameter can be null. </param>
	/// <exception cref="T:System.NotSupportedException">The socket is not connected to a remote host. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> object has been closed. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.Socket" /> object is not in blocking mode and cannot accept this synchronous call. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file <paramref name="fileName" /> was not found. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SendFile(string fileName)
	{
		SendFile(fileName, null, null, TransmitFileOptions.UseDefaultWorkerThread);
	}

	/// <summary>Sends the specified number of bytes of data to a connected <see cref="T:System.Net.Sockets.Socket" />, starting at the specified offset, and using the specified <see cref="T:System.Net.Sockets.SocketFlags" />.</summary>
	/// <returns>The number of bytes sent to the <see cref="T:System.Net.Sockets.Socket" />.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the data to be sent. </param>
	/// <param name="offset">The position in the data buffer at which to begin sending data. </param>
	/// <param name="size">The number of bytes to send. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or- <paramref name="offset" /> is greater than the length of <paramref name="buffer" />.-or- <paramref name="size" /> is less than 0.-or- <paramref name="size" /> is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">
	///   <paramref name="socketFlags" /> is not a valid combination of values.-or- An operating system error occurs while accessing the <see cref="T:System.Net.Sockets.Socket" />. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
	{
		SocketError errorCode;
		int result = Send(buffer, offset, size, socketFlags, out errorCode);
		if (errorCode != 0)
		{
			throw new SocketException(errorCode);
		}
		return result;
	}

	/// <summary>Sends the specified number of bytes of data to the specified endpoint using the specified <see cref="T:System.Net.Sockets.SocketFlags" />.</summary>
	/// <returns>The number of bytes sent.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the data to be sent. </param>
	/// <param name="size">The number of bytes to send. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <param name="remoteEP">The <see cref="T:System.Net.EndPoint" /> that represents the destination location for the data. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.-or- <paramref name="remoteEP" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The specified <paramref name="size" /> exceeds the size of <paramref name="buffer" />. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int SendTo(byte[] buffer, int size, SocketFlags socketFlags, EndPoint remoteEP)
	{
		return SendTo(buffer, 0, size, socketFlags, remoteEP);
	}

	/// <summary>Sends data to a specific endpoint using the specified <see cref="T:System.Net.Sockets.SocketFlags" />.</summary>
	/// <returns>The number of bytes sent.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the data to be sent. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <param name="remoteEP">The <see cref="T:System.Net.EndPoint" /> that represents the destination location for the data. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.-or- <paramref name="remoteEP" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int SendTo(byte[] buffer, SocketFlags socketFlags, EndPoint remoteEP)
	{
		return SendTo(buffer, 0, (buffer != null) ? buffer.Length : 0, socketFlags, remoteEP);
	}

	/// <summary>Sends data to the specified endpoint.</summary>
	/// <returns>The number of bytes sent.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the data to be sent. </param>
	/// <param name="remoteEP">The <see cref="T:System.Net.EndPoint" /> that represents the destination for the data. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.-or- <paramref name="remoteEP" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int SendTo(byte[] buffer, EndPoint remoteEP)
	{
		return SendTo(buffer, 0, (buffer != null) ? buffer.Length : 0, SocketFlags.None, remoteEP);
	}

	/// <summary>Receives the specified number of bytes of data from a bound <see cref="T:System.Net.Sockets.Socket" /> into a receive buffer, using the specified <see cref="T:System.Net.Sockets.SocketFlags" />.</summary>
	/// <returns>The number of bytes received.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the storage location for the received data. </param>
	/// <param name="size">The number of bytes to receive. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="size" /> exceeds the size of <paramref name="buffer" />. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller in the call stack does not have the required permissions. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int Receive(byte[] buffer, int size, SocketFlags socketFlags)
	{
		return Receive(buffer, 0, size, socketFlags);
	}

	/// <summary>Receives data from a bound <see cref="T:System.Net.Sockets.Socket" /> into a receive buffer, using the specified <see cref="T:System.Net.Sockets.SocketFlags" />.</summary>
	/// <returns>The number of bytes received.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the storage location for the received data. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller in the call stack does not have the required permissions. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int Receive(byte[] buffer, SocketFlags socketFlags)
	{
		return Receive(buffer, 0, (buffer != null) ? buffer.Length : 0, socketFlags);
	}

	/// <summary>Receives data from a bound <see cref="T:System.Net.Sockets.Socket" /> into a receive buffer.</summary>
	/// <returns>The number of bytes received.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the storage location for the received data. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller in the call stack does not have the required permissions. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int Receive(byte[] buffer)
	{
		return Receive(buffer, 0, (buffer != null) ? buffer.Length : 0, SocketFlags.None);
	}

	/// <summary>Receives the specified number of bytes from a bound <see cref="T:System.Net.Sockets.Socket" /> into the specified offset position of the receive buffer, using the specified <see cref="T:System.Net.Sockets.SocketFlags" />.</summary>
	/// <returns>The number of bytes received.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the storage location for received data. </param>
	/// <param name="offset">The location in <paramref name="buffer" /> to store the received data. </param>
	/// <param name="size">The number of bytes to receive. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or- <paramref name="offset" /> is greater than the length of <paramref name="buffer" />.-or- <paramref name="size" /> is less than 0.-or- <paramref name="size" /> is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">
	///   <paramref name="socketFlags" /> is not a valid combination of values.-or- The <see cref="P:System.Net.Sockets.Socket.LocalEndPoint" /> property was not set.-or- An operating system error occurs while accessing the <see cref="T:System.Net.Sockets.Socket" />. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller in the call stack does not have the required permissions. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags)
	{
		SocketError errorCode;
		int result = Receive(buffer, offset, size, socketFlags, out errorCode);
		if (errorCode != 0)
		{
			throw new SocketException(errorCode);
		}
		return result;
	}

	/// <summary>Receives data from a bound <see cref="T:System.Net.Sockets.Socket" /> into the list of receive buffers.</summary>
	/// <returns>The number of bytes received.</returns>
	/// <param name="buffers">A list of <see cref="T:System.ArraySegment`1" />s of type <see cref="T:System.Byte" /> that contains the received data.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred while attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	public int Receive(IList<ArraySegment<byte>> buffers)
	{
		return Receive(buffers, SocketFlags.None);
	}

	/// <summary>Receives data from a bound <see cref="T:System.Net.Sockets.Socket" /> into the list of receive buffers, using the specified <see cref="T:System.Net.Sockets.SocketFlags" />.</summary>
	/// <returns>The number of bytes received.</returns>
	/// <param name="buffers">A list of <see cref="T:System.ArraySegment`1" />s of type <see cref="T:System.Byte" /> that contains the received data.</param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffers" /> is null.-or-<paramref name="buffers" />.Count is zero.</exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred while attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	public int Receive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags)
	{
		SocketError errorCode;
		int result = Receive(buffers, socketFlags, out errorCode);
		if (errorCode != 0)
		{
			throw new SocketException(errorCode);
		}
		return result;
	}

	/// <summary>Receives the specified number of bytes into the data buffer, using the specified <see cref="T:System.Net.Sockets.SocketFlags" />, and stores the endpoint.</summary>
	/// <returns>The number of bytes received.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the storage location for received data. </param>
	/// <param name="size">The number of bytes to receive. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <param name="remoteEP">An <see cref="T:System.Net.EndPoint" />, passed by reference, that represents the remote server. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.-or- <paramref name="remoteEP" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="size" /> is less than 0.-or- <paramref name="size" /> is greater than the length of <paramref name="buffer" />. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">
	///   <paramref name="socketFlags" /> is not a valid combination of values.-or- The <see cref="P:System.Net.Sockets.Socket.LocalEndPoint" /> property was not set.-or- An operating system error occurs while accessing the <see cref="T:System.Net.Sockets.Socket" />. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller in the call stack does not have the required permissions. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int ReceiveFrom(byte[] buffer, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
	{
		return ReceiveFrom(buffer, 0, size, socketFlags, ref remoteEP);
	}

	/// <summary>Receives a datagram into the data buffer, using the specified <see cref="T:System.Net.Sockets.SocketFlags" />, and stores the endpoint.</summary>
	/// <returns>The number of bytes received.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the storage location for the received data. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <param name="remoteEP">An <see cref="T:System.Net.EndPoint" />, passed by reference, that represents the remote server. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.-or- <paramref name="remoteEP" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller in the call stack does not have the required permissions. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int ReceiveFrom(byte[] buffer, SocketFlags socketFlags, ref EndPoint remoteEP)
	{
		return ReceiveFrom(buffer, 0, (buffer != null) ? buffer.Length : 0, socketFlags, ref remoteEP);
	}

	/// <summary>Receives a datagram into the data buffer and stores the endpoint.</summary>
	/// <returns>The number of bytes received.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the storage location for received data. </param>
	/// <param name="remoteEP">An <see cref="T:System.Net.EndPoint" />, passed by reference, that represents the remote server. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.-or- <paramref name="remoteEP" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller in the call stack does not have the required permissions. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int ReceiveFrom(byte[] buffer, ref EndPoint remoteEP)
	{
		return ReceiveFrom(buffer, 0, (buffer != null) ? buffer.Length : 0, SocketFlags.None, ref remoteEP);
	}

	/// <summary>Sets low-level operating modes for the <see cref="T:System.Net.Sockets.Socket" /> using the <see cref="T:System.Net.Sockets.IOControlCode" /> enumeration to specify control codes.</summary>
	/// <returns>The number of bytes in the <paramref name="optionOutValue" /> parameter.</returns>
	/// <param name="ioControlCode">A <see cref="T:System.Net.Sockets.IOControlCode" /> value that specifies the control code of the operation to perform. </param>
	/// <param name="optionInValue">An array of type <see cref="T:System.Byte" /> that contains the input data required by the operation. </param>
	/// <param name="optionOutValue">An array of type <see cref="T:System.Byte" /> that contains the output data returned by the operation. </param>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to change the blocking mode without using the <see cref="P:System.Net.Sockets.Socket.Blocking" /> property. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int IOControl(IOControlCode ioControlCode, byte[] optionInValue, byte[] optionOutValue)
	{
		return IOControl((int)ioControlCode, optionInValue, optionOutValue);
	}

	/// <summary>Set the IP protection level on a socket.</summary>
	/// <param name="level">The IP protection level to set on this socket. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="level" /> parameter cannot be <see cref="F:System.Net.Sockets.IPProtectionLevel.Unspecified" />. The IP protection level cannot be set to unspecified.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Net.Sockets.AddressFamily" /> of the socket must be either <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" /> or <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork" />.</exception>
	public void SetIPProtectionLevel(IPProtectionLevel level)
	{
		if (level == IPProtectionLevel.Unspecified)
		{
			throw new ArgumentException(global::SR.GetString("The specified value is not valid."), "level");
		}
		if (addressFamily == AddressFamily.InterNetworkV6)
		{
			SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPProtectionLevel, (int)level);
			return;
		}
		if (addressFamily == AddressFamily.InterNetwork)
		{
			SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IPProtectionLevel, (int)level);
			return;
		}
		throw new NotSupportedException(global::SR.GetString("This protocol version is not supported."));
	}

	/// <summary>Sends the file <paramref name="fileName" /> to a connected <see cref="T:System.Net.Sockets.Socket" /> object using the <see cref="F:System.Net.Sockets.TransmitFileOptions.UseDefaultWorkerThread" /> flag.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that represents the asynchronous send.</returns>
	/// <param name="fileName">A string that contains the path and name of the file to send. This parameter can be null. </param>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object that contains state information for this request. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> object has been closed. </exception>
	/// <exception cref="T:System.NotSupportedException">The socket is not connected to a remote host. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file <paramref name="fileName" /> was not found. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See remarks section below. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public IAsyncResult BeginSendFile(string fileName, AsyncCallback callback, object state)
	{
		return BeginSendFile(fileName, null, null, TransmitFileOptions.UseDefaultWorkerThread, callback, state);
	}

	/// <summary>Begins an asynchronous request for a remote host connection. The host is specified by an <see cref="T:System.Net.IPAddress" /> and a port number.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous connection.</returns>
	/// <param name="address">The <see cref="T:System.Net.IPAddress" /> of the remote host.</param>
	/// <param name="port">The port number of the remote host.</param>
	/// <param name="requestCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the connect operation is complete. </param>
	/// <param name="state">A user-defined object that contains information about the connect operation. This object is passed to the <paramref name="requestCallback" /> delegate when the operation is complete.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Net.Sockets.Socket" /> is not in the socket family.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The port number is not valid.</exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="address" /> is zero.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.Socket" /> is <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" />ing.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public IAsyncResult BeginConnect(IPAddress address, int port, AsyncCallback requestCallback, object state)
	{
		_ = s_LoggingEnabled;
		if (CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (!ValidationHelper.ValidateTcpPort(port))
		{
			throw new ArgumentOutOfRangeException("port");
		}
		if (!CanTryAddressFamily(address.AddressFamily))
		{
			throw new NotSupportedException(global::SR.GetString("This protocol version is not supported."));
		}
		IAsyncResult result = BeginConnect(new IPEndPoint(address, port), requestCallback, state);
		_ = s_LoggingEnabled;
		return result;
	}

	/// <summary>Sends data asynchronously to a connected <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous send.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the data to send. </param>
	/// <param name="offset">The zero-based position in the <paramref name="buffer" /> parameter at which to begin sending data. </param>
	/// <param name="size">The number of bytes to send. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object that contains state information for this request. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See remarks section below. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or- <paramref name="offset" /> is less than the length of <paramref name="buffer" />.-or- <paramref name="size" /> is less than 0.-or- <paramref name="size" /> is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public IAsyncResult BeginSend(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
	{
		SocketError errorCode;
		IAsyncResult result = BeginSend(buffer, offset, size, socketFlags, out errorCode, callback, state);
		if (errorCode != 0 && errorCode != SocketError.IOPending)
		{
			throw new SocketException(errorCode);
		}
		return result;
	}

	/// <summary>Sends data asynchronously to a connected <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous send.</returns>
	/// <param name="buffers">An array of type <see cref="T:System.Byte" /> that contains the data to send. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object that contains state information for this request. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffers" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="buffers" /> is empty.</exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See remarks section below. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public IAsyncResult BeginSend(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
	{
		SocketError errorCode;
		IAsyncResult result = BeginSend(buffers, socketFlags, out errorCode, callback, state);
		if (errorCode != 0 && errorCode != SocketError.IOPending)
		{
			throw new SocketException(errorCode);
		}
		return result;
	}

	/// <summary>Ends a pending asynchronous send.</summary>
	/// <returns>If successful, the number of bytes sent to the <see cref="T:System.Net.Sockets.Socket" />; otherwise, an invalid <see cref="T:System.Net.Sockets.Socket" /> error.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> that stores state information for this asynchronous operation. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by a call to the <see cref="M:System.Net.Sockets.Socket.BeginSend(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)" /> method. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.Socket.EndSend(System.IAsyncResult)" /> was previously called for the asynchronous send. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int EndSend(IAsyncResult asyncResult)
	{
		SocketError errorCode;
		int result = EndSend(asyncResult, out errorCode);
		if (errorCode != 0)
		{
			throw new SocketException(errorCode);
		}
		return result;
	}

	/// <summary>Begins to asynchronously receive data from a connected <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous read.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the storage location for the received data. </param>
	/// <param name="offset">The zero-based position in the <paramref name="buffer" /> parameter at which to store the received data. </param>
	/// <param name="size">The number of bytes to receive. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <param name="callback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete. </param>
	/// <param name="state">A user-defined object that contains information about the receive operation. This object is passed to the <see cref="M:System.Net.Sockets.Socket.EndReceive(System.IAsyncResult)" /> delegate when the operation is complete.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or- <paramref name="offset" /> is greater than the length of <paramref name="buffer" />.-or- <paramref name="size" /> is less than 0.-or- <paramref name="size" /> is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
	{
		SocketError errorCode;
		IAsyncResult result = BeginReceive(buffer, offset, size, socketFlags, out errorCode, callback, state);
		if (errorCode != 0 && errorCode != SocketError.IOPending)
		{
			throw new SocketException(errorCode);
		}
		return result;
	}

	/// <summary>Begins to asynchronously receive data from a connected <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous read.</returns>
	/// <param name="buffers">An array of type <see cref="T:System.Byte" /> that is the storage location for the received data.</param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values.</param>
	/// <param name="callback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete.</param>
	/// <param name="state">A user-defined object that contains information about the receive operation. This object is passed to the <see cref="M:System.Net.Sockets.Socket.EndReceive(System.IAsyncResult)" /> delegate when the operation is complete.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public IAsyncResult BeginReceive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
	{
		SocketError errorCode;
		IAsyncResult result = BeginReceive(buffers, socketFlags, out errorCode, callback, state);
		if (errorCode != 0 && errorCode != SocketError.IOPending)
		{
			throw new SocketException(errorCode);
		}
		return result;
	}

	/// <summary>Ends a pending asynchronous read.</summary>
	/// <returns>The number of bytes received.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> that stores state information and any user defined data for this asynchronous operation. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by a call to the <see cref="M:System.Net.Sockets.Socket.BeginReceive(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)" /> method. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.Socket.EndReceive(System.IAsyncResult)" /> was previously called for the asynchronous read. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int EndReceive(IAsyncResult asyncResult)
	{
		SocketError errorCode;
		int result = EndReceive(asyncResult, out errorCode);
		if (errorCode != 0)
		{
			throw new SocketException(errorCode);
		}
		return result;
	}

	/// <summary>Begins an asynchronous operation to accept an incoming connection attempt and receives the first block of data sent by the client application.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous <see cref="T:System.Net.Sockets.Socket" /> creation.</returns>
	/// <param name="receiveSize">The number of bytes to accept from the sender. </param>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object that contains state information for this request. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> object has been closed. </exception>
	/// <exception cref="T:System.NotSupportedException">Windows NT is required for this method. </exception>
	/// <exception cref="T:System.InvalidOperationException">The accepting socket is not listening for connections. You must call <see cref="M:System.Net.Sockets.Socket.Bind(System.Net.EndPoint)" /> and <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" /> before calling <see cref="M:System.Net.Sockets.Socket.BeginAccept(System.AsyncCallback,System.Object)" />.-or- The accepted socket is bound. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="receiveSize" /> is less than 0. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public IAsyncResult BeginAccept(int receiveSize, AsyncCallback callback, object state)
	{
		return BeginAccept(null, receiveSize, callback, state);
	}

	/// <summary>Asynchronously accepts an incoming connection attempt and creates a new <see cref="T:System.Net.Sockets.Socket" /> object to handle remote host communication. This method returns a buffer that contains the initial data transferred.</summary>
	/// <returns>A <see cref="T:System.Net.Sockets.Socket" /> object to handle communication with the remote host.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the bytes transferred. </param>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> object that stores state information for this asynchronous operation as well as any user defined data. </param>
	/// <exception cref="T:System.NotSupportedException">Windows NT is required for this method. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> object has been closed. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is empty. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not created by a call to <see cref="M:System.Net.Sockets.Socket.BeginAccept(System.AsyncCallback,System.Object)" />. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.Socket.EndAccept(System.IAsyncResult)" /> method was previously called. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the <see cref="T:System.Net.Sockets.Socket" /> See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Socket EndAccept(out byte[] buffer, IAsyncResult asyncResult)
	{
		byte[] buffer2;
		int bytesTransferred;
		Socket result = EndAccept(out buffer2, out bytesTransferred, asyncResult);
		buffer = new byte[bytesTransferred];
		Array.Copy(buffer2, buffer, bytesTransferred);
		return result;
	}

	internal static void InitializeSockets()
	{
		if (s_Initialized)
		{
			return;
		}
		lock (InternalSyncObject)
		{
			if (!s_Initialized)
			{
				bool flag = true;
				bool num = IsProtocolSupported(NetworkInterfaceComponent.IPv4);
				flag = IsProtocolSupported(NetworkInterfaceComponent.IPv6);
				if (flag)
				{
					s_OSSupportsIPv6 = true;
					flag = SettingsSectionInternal.Section.Ipv6Enabled;
				}
				s_SupportsIPv4 = num;
				s_SupportsIPv6 = flag;
				s_Initialized = true;
			}
		}
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Net.Sockets.Socket" /> class.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Frees resources used by the <see cref="T:System.Net.Sockets.Socket" /> class.</summary>
	~Socket()
	{
		Dispose(disposing: false);
	}

	/// <summary>Begins an asynchronous request for a connection to a remote host.</summary>
	/// <returns>Returns true if the I/O operation is pending. The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will be raised upon completion of the operation. Returns false if the I/O operation completed synchronously. In this case, The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will not be raised and the <paramref name="e" /> object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation. </returns>
	/// <param name="socketType">One of the <see cref="T:System.Net.Sockets.SocketType" /> values.</param>
	/// <param name="protocolType">One of the <see cref="T:System.Net.Sockets.ProtocolType" /> values.</param>
	/// <param name="e">The <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object to use for this asynchronous socket operation.</param>
	/// <exception cref="T:System.ArgumentException">An argument is not valid. This exception occurs if multiple buffers are specified, the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.BufferList" /> property is not null. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="e" /> parameter cannot be null and the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint" /> cannot be null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.Socket" /> is listening or a socket operation was already in progress using the <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object specified in the <paramref name="e" /> parameter.</exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information.</exception>
	/// <exception cref="T:System.NotSupportedException">Windows XP or later is required for this method. This exception also occurs if the local endpoint and the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint" /> are not the same address family.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller higher in the call stack does not have permission for the requested operation.</exception>
	public static bool ConnectAsync(SocketType socketType, ProtocolType protocolType, SocketAsyncEventArgs e)
	{
		_ = s_LoggingEnabled;
		if (e.m_BufferList != null)
		{
			throw new ArgumentException(global::SR.GetString("Multiple buffers cannot be used with this method."), "BufferList");
		}
		if (e.RemoteEndPoint == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		EndPoint remoteEndPoint = e.RemoteEndPoint;
		bool result;
		if (remoteEndPoint is DnsEndPoint dnsEndPoint)
		{
			Socket socket = null;
			MultipleConnectAsync multipleConnectAsync = null;
			if (dnsEndPoint.AddressFamily == AddressFamily.Unspecified)
			{
				multipleConnectAsync = new MultipleSocketMultipleConnectAsync(socketType, protocolType);
			}
			else
			{
				socket = new Socket(dnsEndPoint.AddressFamily, socketType, protocolType);
				multipleConnectAsync = new SingleSocketMultipleConnectAsync(socket, userSocket: false);
			}
			e.StartOperationCommon(socket);
			e.StartOperationWrapperConnect(multipleConnectAsync);
			result = multipleConnectAsync.StartConnectAsync(e, dnsEndPoint);
		}
		else
		{
			result = new Socket(remoteEndPoint.AddressFamily, socketType, protocolType).ConnectAsync(e);
		}
		_ = s_LoggingEnabled;
		return result;
	}

	internal void InternalShutdown(SocketShutdown how)
	{
		if (is_connected && !CleanedUp)
		{
			Shutdown_internal(m_Handle, how, out var _);
		}
	}

	internal IAsyncResult UnsafeBeginConnect(EndPoint remoteEP, AsyncCallback callback, object state)
	{
		return BeginConnect(remoteEP, callback, state);
	}

	internal IAsyncResult UnsafeBeginSend(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
	{
		return BeginSend(buffer, offset, size, socketFlags, callback, state);
	}

	internal IAsyncResult UnsafeBeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
	{
		return BeginReceive(buffer, offset, size, socketFlags, callback, state);
	}

	internal IAsyncResult BeginMultipleSend(BufferOffsetSize[] buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
	{
		ArraySegment<byte>[] array = new ArraySegment<byte>[buffers.Length];
		for (int i = 0; i < buffers.Length; i++)
		{
			array[i] = new ArraySegment<byte>(buffers[i].Buffer, buffers[i].Offset, buffers[i].Size);
		}
		return BeginSend(array, socketFlags, callback, state);
	}

	internal IAsyncResult UnsafeBeginMultipleSend(BufferOffsetSize[] buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
	{
		return BeginMultipleSend(buffers, socketFlags, callback, state);
	}

	internal int EndMultipleSend(IAsyncResult asyncResult)
	{
		return EndSend(asyncResult);
	}

	internal void MultipleSend(BufferOffsetSize[] buffers, SocketFlags socketFlags)
	{
		ArraySegment<byte>[] array = new ArraySegment<byte>[buffers.Length];
		for (int i = 0; i < buffers.Length; i++)
		{
			array[i] = new ArraySegment<byte>(buffers[i].Buffer, buffers[i].Offset, buffers[i].Size);
		}
		Send(array, socketFlags);
	}

	internal void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue, bool silent)
	{
		if (CleanedUp && is_closed)
		{
			if (!silent)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
			return;
		}
		SetSocketOption_internal(m_Handle, optionLevel, optionName, null, null, optionValue, out var error);
		if (!silent && error != 0)
		{
			throw new SocketException(error);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Sockets.Socket" /> class using the specified value returned from <see cref="M:System.Net.Sockets.Socket.DuplicateAndClose(System.Int32)" />.</summary>
	/// <param name="socketInformation">The socket information returned by <see cref="M:System.Net.Sockets.Socket.DuplicateAndClose(System.Int32)" />.</param>
	public Socket(SocketInformation socketInformation)
	{
		is_listening = (socketInformation.Options & SocketInformationOptions.Listening) != 0;
		is_connected = (socketInformation.Options & SocketInformationOptions.Connected) != 0;
		is_blocking = (socketInformation.Options & SocketInformationOptions.NonBlocking) == 0;
		useOverlappedIO = (socketInformation.Options & SocketInformationOptions.UseOnlyOverlappedIO) != 0;
		IList list = DataConverter.Unpack("iiiil", socketInformation.ProtocolInformation, 0);
		addressFamily = (AddressFamily)(int)list[0];
		socketType = (SocketType)(int)list[1];
		protocolType = (ProtocolType)(int)list[2];
		is_bound = (int)list[3] != 0;
		m_Handle = new SafeSocketHandle((IntPtr)(long)list[4], ownsHandle: true);
		InitializeSockets();
		SocketDefaults();
	}

	internal Socket(AddressFamily family, SocketType type, ProtocolType proto, SafeSocketHandle safe_handle)
	{
		addressFamily = family;
		socketType = type;
		protocolType = proto;
		m_Handle = safe_handle;
		is_connected = true;
		InitializeSockets();
	}

	private void SocketDefaults()
	{
		try
		{
			if (addressFamily == AddressFamily.InterNetwork)
			{
				DontFragment = false;
				if (protocolType == ProtocolType.Tcp)
				{
					NoDelay = false;
				}
			}
			else if (addressFamily == AddressFamily.InterNetworkV6)
			{
				DualMode = true;
			}
		}
		catch (SocketException)
		{
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern IntPtr Socket_internal(AddressFamily family, SocketType type, ProtocolType proto, out int error);

	private static int Available_internal(SafeSocketHandle safeHandle, out int error)
	{
		bool success = false;
		try
		{
			safeHandle.DangerousAddRef(ref success);
			return Available_internal(safeHandle.DangerousGetHandle(), out error);
		}
		finally
		{
			if (success)
			{
				safeHandle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int Available_internal(IntPtr socket, out int error);

	private static SocketAddress LocalEndPoint_internal(SafeSocketHandle safeHandle, int family, out int error)
	{
		bool success = false;
		try
		{
			safeHandle.DangerousAddRef(ref success);
			return LocalEndPoint_internal(safeHandle.DangerousGetHandle(), family, out error);
		}
		finally
		{
			if (success)
			{
				safeHandle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern SocketAddress LocalEndPoint_internal(IntPtr socket, int family, out int error);

	private static void Blocking_internal(SafeSocketHandle safeHandle, bool block, out int error)
	{
		bool success = false;
		try
		{
			safeHandle.DangerousAddRef(ref success);
			Blocking_internal(safeHandle.DangerousGetHandle(), block, out error);
		}
		finally
		{
			if (success)
			{
				safeHandle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern void Blocking_internal(IntPtr socket, bool block, out int error);

	private static SocketAddress RemoteEndPoint_internal(SafeSocketHandle safeHandle, int family, out int error)
	{
		bool success = false;
		try
		{
			safeHandle.DangerousAddRef(ref success);
			return RemoteEndPoint_internal(safeHandle.DangerousGetHandle(), family, out error);
		}
		finally
		{
			if (success)
			{
				safeHandle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern SocketAddress RemoteEndPoint_internal(IntPtr socket, int family, out int error);

	/// <summary>Determines the status of one or more sockets.</summary>
	/// <param name="checkRead">An <see cref="T:System.Collections.IList" /> of <see cref="T:System.Net.Sockets.Socket" /> instances to check for readability. </param>
	/// <param name="checkWrite">An <see cref="T:System.Collections.IList" /> of <see cref="T:System.Net.Sockets.Socket" /> instances to check for writability. </param>
	/// <param name="checkError">An <see cref="T:System.Collections.IList" /> of <see cref="T:System.Net.Sockets.Socket" /> instances to check for errors. </param>
	/// <param name="microSeconds">The time-out value, in microseconds. A -1 value indicates an infinite time-out.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="checkRead" /> parameter is null or empty.-and- The <paramref name="checkWrite" /> parameter is null or empty -and- The <paramref name="checkError" /> parameter is null or empty. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	public static void Select(IList checkRead, IList checkWrite, IList checkError, int microSeconds)
	{
		List<Socket> list = new List<Socket>();
		AddSockets(list, checkRead, "checkRead");
		AddSockets(list, checkWrite, "checkWrite");
		AddSockets(list, checkError, "checkError");
		if (list.Count == 3)
		{
			throw new ArgumentNullException("checkRead, checkWrite, checkError", "All the lists are null or empty.");
		}
		Socket[] sockets = list.ToArray();
		Select_internal(ref sockets, microSeconds, out var error);
		if (error != 0)
		{
			throw new SocketException(error);
		}
		if (sockets == null)
		{
			checkRead?.Clear();
			checkWrite?.Clear();
			checkError?.Clear();
			return;
		}
		int num = 0;
		int num2 = sockets.Length;
		IList list2 = checkRead;
		int num3 = 0;
		for (int i = 0; i < num2; i++)
		{
			Socket socket = sockets[i];
			if (socket == null)
			{
				if (list2 != null)
				{
					int num4 = list2.Count - num3;
					for (int j = 0; j < num4; j++)
					{
						list2.RemoveAt(num3);
					}
				}
				list2 = ((num == 0) ? checkWrite : checkError);
				num3 = 0;
				num++;
			}
			else
			{
				if (num == 1 && list2 == checkWrite && !socket.is_connected && (int)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Error) == 0)
				{
					socket.is_connected = true;
				}
				while ((Socket)list2[num3] != socket)
				{
					list2.RemoveAt(num3);
				}
				num3++;
			}
		}
	}

	private static void AddSockets(List<Socket> sockets, IList list, string name)
	{
		if (list != null)
		{
			foreach (Socket item in list)
			{
				if (item == null)
				{
					throw new ArgumentNullException(name, "Contains a null element");
				}
				sockets.Add(item);
			}
		}
		sockets.Add(null);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Select_internal(ref Socket[] sockets, int microSeconds, out int error);

	/// <summary>Determines the status of the <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>The status of the <see cref="T:System.Net.Sockets.Socket" /> based on the polling mode value passed in the <paramref name="mode" /> parameter.Mode Return Value <see cref="F:System.Net.Sockets.SelectMode.SelectRead" />true if <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" /> has been called and a connection is pending; -or- true if data is available for reading; -or- true if the connection has been closed, reset, or terminated; otherwise, returns false. <see cref="F:System.Net.Sockets.SelectMode.SelectWrite" />true, if processing a <see cref="M:System.Net.Sockets.Socket.Connect(System.Net.EndPoint)" />, and the connection has succeeded; -or- true if data can be sent; otherwise, returns false. <see cref="F:System.Net.Sockets.SelectMode.SelectError" />true if processing a <see cref="M:System.Net.Sockets.Socket.Connect(System.Net.EndPoint)" /> that does not block, and the connection has failed; -or- true if <see cref="F:System.Net.Sockets.SocketOptionName.OutOfBandInline" /> is not set and out-of-band data is available; otherwise, returns false. </returns>
	/// <param name="microSeconds">The time to wait for a response, in microseconds. </param>
	/// <param name="mode">One of the <see cref="T:System.Net.Sockets.SelectMode" /> values. </param>
	/// <exception cref="T:System.NotSupportedException">The <paramref name="mode" /> parameter is not one of the <see cref="T:System.Net.Sockets.SelectMode" /> values. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See remarks below. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool Poll(int microSeconds, SelectMode mode)
	{
		ThrowIfDisposedAndClosed();
		if (mode != 0 && mode != SelectMode.SelectWrite && mode != SelectMode.SelectError)
		{
			throw new NotSupportedException("'mode' parameter is not valid.");
		}
		int error;
		bool flag = Poll_internal(m_Handle, mode, microSeconds, out error);
		if (error != 0)
		{
			throw new SocketException(error);
		}
		if (mode == SelectMode.SelectWrite && flag && !is_connected && (int)GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Error) == 0)
		{
			is_connected = true;
		}
		return flag;
	}

	private static bool Poll_internal(SafeSocketHandle safeHandle, SelectMode mode, int timeout, out int error)
	{
		bool success = false;
		try
		{
			safeHandle.DangerousAddRef(ref success);
			return Poll_internal(safeHandle.DangerousGetHandle(), mode, timeout, out error);
		}
		finally
		{
			if (success)
			{
				safeHandle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool Poll_internal(IntPtr socket, SelectMode mode, int timeout, out int error);

	/// <summary>Creates a new <see cref="T:System.Net.Sockets.Socket" /> for a newly created connection.</summary>
	/// <returns>A <see cref="T:System.Net.Sockets.Socket" /> for a newly created connection.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.InvalidOperationException">The accepting socket is not listening for connections. You must call <see cref="M:System.Net.Sockets.Socket.Bind(System.Net.EndPoint)" /> and <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" /> before calling <see cref="M:System.Net.Sockets.Socket.Accept" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Socket Accept()
	{
		ThrowIfDisposedAndClosed();
		int error = 0;
		SafeSocketHandle safe_handle = Accept_internal(m_Handle, out error, is_blocking);
		if (error != 0)
		{
			if (is_closed)
			{
				error = 10004;
			}
			throw new SocketException(error);
		}
		return new Socket(AddressFamily, SocketType, ProtocolType, safe_handle)
		{
			seed_endpoint = seed_endpoint,
			Blocking = Blocking
		};
	}

	internal void Accept(Socket acceptSocket)
	{
		ThrowIfDisposedAndClosed();
		int error = 0;
		SafeSocketHandle handle = Accept_internal(m_Handle, out error, is_blocking);
		if (error != 0)
		{
			if (is_closed)
			{
				error = 10004;
			}
			throw new SocketException(error);
		}
		acceptSocket.addressFamily = AddressFamily;
		acceptSocket.socketType = SocketType;
		acceptSocket.protocolType = ProtocolType;
		acceptSocket.m_Handle = handle;
		acceptSocket.is_connected = true;
		acceptSocket.seed_endpoint = seed_endpoint;
		acceptSocket.Blocking = Blocking;
	}

	/// <summary>Begins an asynchronous operation to accept an incoming connection attempt.</summary>
	/// <returns>Returns true if the I/O operation is pending. The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will be raised upon completion of the operation.Returns false if the I/O operation completed synchronously. The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will not be raised and the <paramref name="e" /> object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.</returns>
	/// <param name="e">The <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object to use for this asynchronous socket operation.</param>
	/// <exception cref="T:System.ArgumentException">An argument is not valid. This exception occurs if the buffer provided is not large enough. The buffer must be at least 2 * (sizeof(SOCKADDR_STORAGE + 16) bytes. This exception also occurs if multiple buffers are specified, the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.BufferList" /> property is not null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">An argument is out of range. The exception occurs if the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Count" /> is less than 0.</exception>
	/// <exception cref="T:System.InvalidOperationException">An invalid operation was requested. This exception occurs if the accepting <see cref="T:System.Net.Sockets.Socket" /> is not listening for connections or the accepted socket is bound. You must call the <see cref="M:System.Net.Sockets.Socket.Bind(System.Net.EndPoint)" /> and <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" /> method before calling the <see cref="M:System.Net.Sockets.Socket.AcceptAsync(System.Net.Sockets.SocketAsyncEventArgs)" /> method.This exception also occurs if the socket is already connected or a socket operation was already in progress using the specified <paramref name="e" /> parameter. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.NotSupportedException">Windows XP or later is required for this method.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	public bool AcceptAsync(SocketAsyncEventArgs e)
	{
		ThrowIfDisposedAndClosed();
		if (!is_bound)
		{
			throw new InvalidOperationException("You must call the Bind method before performing this operation.");
		}
		if (!is_listening)
		{
			throw new InvalidOperationException("You must call the Listen method before performing this operation.");
		}
		if (e.BufferList != null)
		{
			throw new ArgumentException("Multiple buffers cannot be used with this method.");
		}
		if (e.Count < 0)
		{
			throw new ArgumentOutOfRangeException("e.Count");
		}
		Socket acceptSocket = e.AcceptSocket;
		if (acceptSocket != null && (acceptSocket.is_bound || acceptSocket.is_connected))
		{
			throw new InvalidOperationException("AcceptSocket: The socket must not be bound or connected.");
		}
		InitSocketAsyncEventArgs(e, AcceptAsyncCallback, e, SocketOperation.Accept);
		QueueIOSelectorJob(ReadSem, e.socket_async_result.Handle, new IOSelectorJob(IOOperation.Read, BeginAcceptCallback, e.socket_async_result));
		return true;
	}

	/// <summary>Begins an asynchronous operation to accept an incoming connection attempt.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous <see cref="T:System.Net.Sockets.Socket" /> creation.</returns>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object that contains state information for this request. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> object has been closed. </exception>
	/// <exception cref="T:System.NotSupportedException">Windows NT is required for this method. </exception>
	/// <exception cref="T:System.InvalidOperationException">The accepting socket is not listening for connections. You must call <see cref="M:System.Net.Sockets.Socket.Bind(System.Net.EndPoint)" /> and <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" /> before calling <see cref="M:System.Net.Sockets.Socket.BeginAccept(System.AsyncCallback,System.Object)" />.-or- The accepted socket is bound. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="receiveSize" /> is less than 0. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public IAsyncResult BeginAccept(AsyncCallback callback, object state)
	{
		ThrowIfDisposedAndClosed();
		if (!is_bound || !is_listening)
		{
			throw new InvalidOperationException();
		}
		SocketAsyncResult socketAsyncResult = new SocketAsyncResult(this, callback, state, SocketOperation.Accept);
		QueueIOSelectorJob(ReadSem, socketAsyncResult.Handle, new IOSelectorJob(IOOperation.Read, BeginAcceptCallback, socketAsyncResult));
		return socketAsyncResult;
	}

	/// <summary>Begins an asynchronous operation to accept an incoming connection attempt from a specified socket and receives the first block of data sent by the client application.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that references the asynchronous <see cref="T:System.Net.Sockets.Socket" /> object creation.</returns>
	/// <param name="acceptSocket">The accepted <see cref="T:System.Net.Sockets.Socket" /> object. This value may be null. </param>
	/// <param name="receiveSize">The maximum number of bytes to receive. </param>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object that contains state information for this request. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> object has been closed. </exception>
	/// <exception cref="T:System.NotSupportedException">Windows NT is required for this method. </exception>
	/// <exception cref="T:System.InvalidOperationException">The accepting socket is not listening for connections. You must call <see cref="M:System.Net.Sockets.Socket.Bind(System.Net.EndPoint)" /> and <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" /> before calling <see cref="M:System.Net.Sockets.Socket.BeginAccept(System.AsyncCallback,System.Object)" />.-or- The accepted socket is bound. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="receiveSize" /> is less than 0. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public IAsyncResult BeginAccept(Socket acceptSocket, int receiveSize, AsyncCallback callback, object state)
	{
		ThrowIfDisposedAndClosed();
		if (receiveSize < 0)
		{
			throw new ArgumentOutOfRangeException("receiveSize", "receiveSize is less than zero");
		}
		if (acceptSocket != null)
		{
			ThrowIfDisposedAndClosed(acceptSocket);
			if (acceptSocket.IsBound)
			{
				throw new InvalidOperationException();
			}
			if (acceptSocket.ProtocolType != ProtocolType.Tcp)
			{
				throw new SocketException(10022);
			}
		}
		SocketAsyncResult socketAsyncResult = new SocketAsyncResult(this, callback, state, SocketOperation.AcceptReceive)
		{
			Buffer = new byte[receiveSize],
			Offset = 0,
			Size = receiveSize,
			SockFlags = SocketFlags.None,
			AcceptSocket = acceptSocket
		};
		QueueIOSelectorJob(ReadSem, socketAsyncResult.Handle, new IOSelectorJob(IOOperation.Read, BeginAcceptReceiveCallback, socketAsyncResult));
		return socketAsyncResult;
	}

	/// <summary>Asynchronously accepts an incoming connection attempt and creates a new <see cref="T:System.Net.Sockets.Socket" /> to handle remote host communication.</summary>
	/// <returns>A <see cref="T:System.Net.Sockets.Socket" /> to handle communication with the remote host.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> that stores state information for this asynchronous operation as well as any user defined data. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not created by a call to <see cref="M:System.Net.Sockets.Socket.BeginAccept(System.AsyncCallback,System.Object)" />. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.Socket.EndAccept(System.IAsyncResult)" /> method was previously called. </exception>
	/// <exception cref="T:System.NotSupportedException">Windows NT is required for this method. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Socket EndAccept(IAsyncResult asyncResult)
	{
		byte[] buffer;
		int bytesTransferred;
		return EndAccept(out buffer, out bytesTransferred, asyncResult);
	}

	/// <summary>Asynchronously accepts an incoming connection attempt and creates a new <see cref="T:System.Net.Sockets.Socket" /> object to handle remote host communication. This method returns a buffer that contains the initial data and the number of bytes transferred.</summary>
	/// <returns>A <see cref="T:System.Net.Sockets.Socket" /> object to handle communication with the remote host.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the bytes transferred. </param>
	/// <param name="bytesTransferred">The number of bytes transferred. </param>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> object that stores state information for this asynchronous operation as well as any user defined data. </param>
	/// <exception cref="T:System.NotSupportedException">Windows NT is required for this method. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> object has been closed. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is empty. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not created by a call to <see cref="M:System.Net.Sockets.Socket.BeginAccept(System.AsyncCallback,System.Object)" />. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.Socket.EndAccept(System.IAsyncResult)" /> method was previously called. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the <see cref="T:System.Net.Sockets.Socket" />. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Socket EndAccept(out byte[] buffer, out int bytesTransferred, IAsyncResult asyncResult)
	{
		ThrowIfDisposedAndClosed();
		SocketAsyncResult socketAsyncResult = ValidateEndIAsyncResult(asyncResult, "EndAccept", "asyncResult");
		if (!socketAsyncResult.IsCompleted)
		{
			socketAsyncResult.AsyncWaitHandle.WaitOne();
		}
		socketAsyncResult.CheckIfThrowDelayedException();
		buffer = socketAsyncResult.Buffer;
		bytesTransferred = socketAsyncResult.Total;
		return socketAsyncResult.AcceptedSocket;
	}

	private static SafeSocketHandle Accept_internal(SafeSocketHandle safeHandle, out int error, bool blocking)
	{
		try
		{
			safeHandle.RegisterForBlockingSyscall();
			return new SafeSocketHandle(Accept_internal(safeHandle.DangerousGetHandle(), out error, blocking), ownsHandle: true);
		}
		finally
		{
			safeHandle.UnRegisterForBlockingSyscall();
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr Accept_internal(IntPtr sock, out int error, bool blocking);

	/// <summary>Associates a <see cref="T:System.Net.Sockets.Socket" /> with a local endpoint.</summary>
	/// <param name="localEP">The local <see cref="T:System.Net.EndPoint" /> to associate with the <see cref="T:System.Net.Sockets.Socket" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="localEP" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller higher in the call stack does not have permission for the requested operation. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Bind(EndPoint localEP)
	{
		ThrowIfDisposedAndClosed();
		if (localEP == null)
		{
			throw new ArgumentNullException("localEP");
		}
		if (localEP is IPEndPoint input)
		{
			localEP = RemapIPEndPoint(input);
		}
		Bind_internal(m_Handle, localEP.Serialize(), out var error);
		if (error != 0)
		{
			throw new SocketException(error);
		}
		if (error == 0)
		{
			is_bound = true;
		}
		seed_endpoint = localEP;
	}

	private static void Bind_internal(SafeSocketHandle safeHandle, SocketAddress sa, out int error)
	{
		bool success = false;
		try
		{
			safeHandle.DangerousAddRef(ref success);
			Bind_internal(safeHandle.DangerousGetHandle(), sa, out error);
		}
		finally
		{
			if (success)
			{
				safeHandle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Bind_internal(IntPtr sock, SocketAddress sa, out int error);

	/// <summary>Places a <see cref="T:System.Net.Sockets.Socket" /> in a listening state.</summary>
	/// <param name="backlog">The maximum length of the pending connections queue. </param>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Listen(int backlog)
	{
		ThrowIfDisposedAndClosed();
		if (!is_bound)
		{
			throw new SocketException(10022);
		}
		Listen_internal(m_Handle, backlog, out var error);
		if (error != 0)
		{
			throw new SocketException(error);
		}
		is_listening = true;
	}

	private static void Listen_internal(SafeSocketHandle safeHandle, int backlog, out int error)
	{
		bool success = false;
		try
		{
			safeHandle.DangerousAddRef(ref success);
			Listen_internal(safeHandle.DangerousGetHandle(), backlog, out error);
		}
		finally
		{
			if (success)
			{
				safeHandle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Listen_internal(IntPtr sock, int backlog, out int error);

	/// <summary>Establishes a connection to a remote host. The host is specified by an IP address and a port number.</summary>
	/// <param name="address">The IP address of the remote host.</param>
	/// <param name="port">The port number of the remote host.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The port number is not valid.</exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.NotSupportedException">This method is valid for sockets in the <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork" /> or <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" /> families.</exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="address" /> is zero.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.Socket" /> is <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" />ing.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Connect(IPAddress address, int port)
	{
		Connect(new IPEndPoint(address, port));
	}

	/// <summary>Establishes a connection to a remote host. The host is specified by a host name and a port number.</summary>
	/// <param name="host">The name of the remote host.</param>
	/// <param name="port">The port number of the remote host.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="host" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The port number is not valid.</exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.NotSupportedException">This method is valid for sockets in the <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork" /> or <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" /> families.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.Socket" /> is <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" />ing.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Connect(string host, int port)
	{
		Connect(Dns.GetHostAddresses(host), port);
	}

	/// <summary>Establishes a connection to a remote host.</summary>
	/// <param name="remoteEP">An <see cref="T:System.Net.EndPoint" /> that represents the remote device. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="remoteEP" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller higher in the call stack does not have permission for the requested operation. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.Socket" /> is <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" />ing.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Connect(EndPoint remoteEP)
	{
		ThrowIfDisposedAndClosed();
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		IPEndPoint iPEndPoint = remoteEP as IPEndPoint;
		if (iPEndPoint != null && socketType != SocketType.Dgram && (iPEndPoint.Address.Equals(IPAddress.Any) || iPEndPoint.Address.Equals(IPAddress.IPv6Any)))
		{
			throw new SocketException(10049);
		}
		if (is_listening)
		{
			throw new InvalidOperationException();
		}
		if (iPEndPoint != null)
		{
			remoteEP = RemapIPEndPoint(iPEndPoint);
		}
		SocketAddress sa = remoteEP.Serialize();
		int error = 0;
		Connect_internal(m_Handle, sa, out error, is_blocking);
		if (error == 0 || error == 10035)
		{
			seed_endpoint = remoteEP;
		}
		if (error != 0)
		{
			if (is_closed)
			{
				error = 10004;
			}
			throw new SocketException(error);
		}
		is_connected = socketType != SocketType.Dgram || iPEndPoint == null || (!iPEndPoint.Address.Equals(IPAddress.Any) && !iPEndPoint.Address.Equals(IPAddress.IPv6Any));
		is_bound = true;
	}

	/// <summary>Begins an asynchronous request for a connection to a remote host.</summary>
	/// <returns>Returns true if the I/O operation is pending. The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will be raised upon completion of the operation. Returns false if the I/O operation completed synchronously. In this case, The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will not be raised and the <paramref name="e" /> object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation. </returns>
	/// <param name="e">The <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object to use for this asynchronous socket operation.</param>
	/// <exception cref="T:System.ArgumentException">An argument is not valid. This exception occurs if multiple buffers are specified, the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.BufferList" /> property is not null. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="e" /> parameter cannot be null and the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint" /> cannot be null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.Socket" /> is listening or a socket operation was already in progress using the <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object specified in the <paramref name="e" /> parameter.</exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information.</exception>
	/// <exception cref="T:System.NotSupportedException">Windows XP or later is required for this method. This exception also occurs if the local endpoint and the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint" /> are not the same address family.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller higher in the call stack does not have permission for the requested operation.</exception>
	public bool ConnectAsync(SocketAsyncEventArgs e)
	{
		ThrowIfDisposedAndClosed();
		if (is_listening)
		{
			throw new InvalidOperationException("You may not perform this operation after calling the Listen method.");
		}
		if (e.RemoteEndPoint == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		InitSocketAsyncEventArgs(e, null, e, SocketOperation.Connect);
		try
		{
			SocketAsyncResult socketAsyncResult;
			if (!GetCheckedIPs(e, out var addresses))
			{
				socketAsyncResult = (SocketAsyncResult)BeginConnect(e.RemoteEndPoint, ConnectAsyncCallback, e);
			}
			else
			{
				DnsEndPoint dnsEndPoint = (DnsEndPoint)e.RemoteEndPoint;
				socketAsyncResult = (SocketAsyncResult)BeginConnect(addresses, dnsEndPoint.Port, ConnectAsyncCallback, e);
			}
			if (socketAsyncResult.IsCompleted && socketAsyncResult.CompletedSynchronously)
			{
				socketAsyncResult.CheckIfThrowDelayedException();
				return false;
			}
		}
		catch (Exception e2)
		{
			e.socket_async_result.Complete(e2, synch: true);
			return false;
		}
		return true;
	}

	/// <summary>Cancels an asynchronous request for a remote host connection.</summary>
	/// <param name="e">The <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object used to request the connection to the remote host by calling one of the <see cref="M:System.Net.Sockets.Socket.ConnectAsync(System.Net.Sockets.SocketType,System.Net.Sockets.ProtocolType,System.Net.Sockets.SocketAsyncEventArgs)" /> methods.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="e" /> parameter cannot be null and the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint" /> cannot be null.</exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller higher in the call stack does not have permission for the requested operation.</exception>
	public static void CancelConnectAsync(SocketAsyncEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (e.in_progress != 0 && e.LastOperation == SocketAsyncOperation.Connect)
		{
			e.current_socket.Close();
		}
	}

	/// <summary>Begins an asynchronous request for a remote host connection. The host is specified by a host name and a port number.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous connection.</returns>
	/// <param name="host">The name of the remote host.</param>
	/// <param name="port">The port number of the remote host.</param>
	/// <param name="requestCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the connect operation is complete. </param>
	/// <param name="state">A user-defined object that contains information about the connect operation. This object is passed to the <paramref name="requestCallback" /> delegate when the operation is complete.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="host" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.NotSupportedException">This method is valid for sockets in the <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork" /> or <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" /> families.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The port number is not valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.Socket" /> is <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" />ing.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public IAsyncResult BeginConnect(string host, int port, AsyncCallback requestCallback, object state)
	{
		ThrowIfDisposedAndClosed();
		if (host == null)
		{
			throw new ArgumentNullException("host");
		}
		if (addressFamily != AddressFamily.InterNetwork && addressFamily != AddressFamily.InterNetworkV6)
		{
			throw new NotSupportedException("This method is valid only for sockets in the InterNetwork and InterNetworkV6 families");
		}
		if (port <= 0 || port > 65535)
		{
			throw new ArgumentOutOfRangeException("port", "Must be > 0 and < 65536");
		}
		if (is_listening)
		{
			throw new InvalidOperationException();
		}
		return BeginConnect(Dns.GetHostAddresses(host), port, requestCallback, state);
	}

	/// <summary>Begins an asynchronous request for a remote host connection.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous connection.</returns>
	/// <param name="remoteEP">An <see cref="T:System.Net.EndPoint" /> that represents the remote host. </param>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object that contains state information for this request. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="remoteEP" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller higher in the call stack does not have permission for the requested operation. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.Socket" /> is <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" />ing.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public IAsyncResult BeginConnect(EndPoint remoteEP, AsyncCallback callback, object state)
	{
		ThrowIfDisposedAndClosed();
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		if (is_listening)
		{
			throw new InvalidOperationException();
		}
		SocketAsyncResult obj = new SocketAsyncResult(this, callback, state, SocketOperation.Connect)
		{
			EndPoint = remoteEP
		};
		BeginSConnect(obj);
		return obj;
	}

	/// <summary>Begins an asynchronous request for a remote host connection. The host is specified by an <see cref="T:System.Net.IPAddress" /> array and a port number.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous connections.</returns>
	/// <param name="addresses">At least one <see cref="T:System.Net.IPAddress" />, designating the remote host.</param>
	/// <param name="port">The port number of the remote host.</param>
	/// <param name="requestCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the connect operation is complete. </param>
	/// <param name="state">A user-defined object that contains information about the connect operation. This object is passed to the <paramref name="requestCallback" /> delegate when the operation is complete.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="addresses" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.NotSupportedException">This method is valid for sockets that use <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork" /> or <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The port number is not valid.</exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="address" /> is zero.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.Socket" /> is <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" />ing.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public IAsyncResult BeginConnect(IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
	{
		ThrowIfDisposedAndClosed();
		if (addresses == null)
		{
			throw new ArgumentNullException("addresses");
		}
		if (addresses.Length == 0)
		{
			throw new ArgumentException("Empty addresses list");
		}
		if (AddressFamily != AddressFamily.InterNetwork && AddressFamily != AddressFamily.InterNetworkV6)
		{
			throw new NotSupportedException("This method is only valid for addresses in the InterNetwork or InterNetworkV6 families");
		}
		if (port <= 0 || port > 65535)
		{
			throw new ArgumentOutOfRangeException("port", "Must be > 0 and < 65536");
		}
		if (is_listening)
		{
			throw new InvalidOperationException();
		}
		SocketAsyncResult obj = new SocketAsyncResult(this, requestCallback, state, SocketOperation.Connect)
		{
			Addresses = addresses,
			Port = port
		};
		is_connected = false;
		BeginMConnect(obj);
		return obj;
	}

	private static void BeginMConnect(SocketAsyncResult sockares)
	{
		Exception ex = null;
		for (int i = sockares.CurrentAddress; i < sockares.Addresses.Length; i++)
		{
			try
			{
				sockares.CurrentAddress++;
				sockares.EndPoint = new IPEndPoint(sockares.Addresses[i], sockares.Port);
				BeginSConnect(sockares);
				return;
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
		}
		throw ex;
	}

	private static void BeginSConnect(SocketAsyncResult sockares)
	{
		EndPoint endPoint = sockares.EndPoint;
		if (endPoint is IPEndPoint)
		{
			IPEndPoint iPEndPoint = (IPEndPoint)endPoint;
			if (iPEndPoint.Address.Equals(IPAddress.Any) || iPEndPoint.Address.Equals(IPAddress.IPv6Any))
			{
				sockares.Complete(new SocketException(10049), synch: true);
				return;
			}
			endPoint = (sockares.EndPoint = sockares.socket.RemapIPEndPoint(iPEndPoint));
		}
		int error = 0;
		if (sockares.socket.connect_in_progress)
		{
			sockares.socket.connect_in_progress = false;
			sockares.socket.m_Handle.Dispose();
			sockares.socket.m_Handle = new SafeSocketHandle(sockares.socket.Socket_internal(sockares.socket.addressFamily, sockares.socket.socketType, sockares.socket.protocolType, out error), ownsHandle: true);
			if (error != 0)
			{
				throw new SocketException(error);
			}
		}
		bool num = sockares.socket.is_blocking;
		if (num)
		{
			sockares.socket.Blocking = false;
		}
		Connect_internal(sockares.socket.m_Handle, endPoint.Serialize(), out error, blocking: false);
		if (num)
		{
			sockares.socket.Blocking = true;
		}
		switch (error)
		{
		case 0:
			sockares.socket.is_connected = true;
			sockares.socket.is_bound = true;
			sockares.Complete(synch: true);
			break;
		default:
			sockares.socket.is_connected = false;
			sockares.socket.is_bound = false;
			sockares.Complete(new SocketException(error), synch: true);
			break;
		case 10035:
		case 10036:
			sockares.socket.is_connected = false;
			sockares.socket.is_bound = false;
			sockares.socket.connect_in_progress = true;
			IOSelector.Add(sockares.Handle, new IOSelectorJob(IOOperation.Write, BeginConnectCallback, sockares));
			break;
		}
	}

	/// <summary>Ends a pending asynchronous connection request.</summary>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> that stores state information and any user defined data for this asynchronous operation. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by a call to the <see cref="M:System.Net.Sockets.Socket.BeginConnect(System.Net.EndPoint,System.AsyncCallback,System.Object)" /> method. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.Socket.EndConnect(System.IAsyncResult)" /> was previously called for the asynchronous connection. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void EndConnect(IAsyncResult asyncResult)
	{
		ThrowIfDisposedAndClosed();
		SocketAsyncResult socketAsyncResult = ValidateEndIAsyncResult(asyncResult, "EndConnect", "asyncResult");
		if (!socketAsyncResult.IsCompleted)
		{
			socketAsyncResult.AsyncWaitHandle.WaitOne();
		}
		socketAsyncResult.CheckIfThrowDelayedException();
	}

	private static void Connect_internal(SafeSocketHandle safeHandle, SocketAddress sa, out int error, bool blocking)
	{
		try
		{
			safeHandle.RegisterForBlockingSyscall();
			Connect_internal(safeHandle.DangerousGetHandle(), sa, out error, blocking);
		}
		finally
		{
			safeHandle.UnRegisterForBlockingSyscall();
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Connect_internal(IntPtr sock, SocketAddress sa, out int error, bool blocking);

	private bool GetCheckedIPs(SocketAsyncEventArgs e, out IPAddress[] addresses)
	{
		addresses = null;
		if (e.RemoteEndPoint is DnsEndPoint dnsEndPoint)
		{
			if (dnsEndPoint.AddressFamily == AddressFamily.Unspecified)
			{
				addresses = Dns.GetHostAddresses(dnsEndPoint.Host);
			}
			else
			{
				IPAddress[] hostAddresses = Dns.GetHostAddresses(dnsEndPoint.Host);
				int num = 0;
				int[] array = new int[hostAddresses.Length];
				for (int i = 0; i < hostAddresses.Length; i++)
				{
					if (hostAddresses[i].AddressFamily == dnsEndPoint.AddressFamily)
					{
						array[num] = i;
						num++;
					}
				}
				addresses = new IPAddress[num];
				for (int j = 0; j < num; j++)
				{
					addresses[j] = hostAddresses[array[j]];
				}
			}
			return true;
		}
		e.ConnectByNameError = null;
		return false;
	}

	/// <summary>Closes the socket connection and allows reuse of the socket.</summary>
	/// <param name="reuseSocket">true if this socket can be reused after the current connection is closed; otherwise, false. </param>
	/// <exception cref="T:System.PlatformNotSupportedException">This method requires Windows 2000 or earlier, or the exception will be thrown.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> object has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Disconnect(bool reuseSocket)
	{
		ThrowIfDisposedAndClosed();
		int error = 0;
		Disconnect_internal(m_Handle, reuseSocket, out error);
		switch (error)
		{
		case 50:
			throw new PlatformNotSupportedException();
		default:
			throw new SocketException(error);
		case 0:
			is_connected = false;
			break;
		}
	}

	/// <summary>Begins an asynchronous request to disconnect from a remote endpoint.</summary>
	/// <returns>Returns true if the I/O operation is pending. The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will be raised upon completion of the operation. Returns false if the I/O operation completed synchronously. In this case, The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will not be raised and the <paramref name="e" /> object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.</returns>
	/// <param name="e">The <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object to use for this asynchronous socket operation.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="e" /> parameter cannot be null.</exception>
	/// <exception cref="T:System.InvalidOperationException">A socket operation was already in progress using the <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object specified in the <paramref name="e" /> parameter.</exception>
	/// <exception cref="T:System.NotSupportedException">Windows XP or later is required for this method.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. </exception>
	public bool DisconnectAsync(SocketAsyncEventArgs e)
	{
		ThrowIfDisposedAndClosed();
		InitSocketAsyncEventArgs(e, DisconnectAsyncCallback, e, SocketOperation.Disconnect);
		IOSelector.Add(e.socket_async_result.Handle, new IOSelectorJob(IOOperation.Write, BeginDisconnectCallback, e.socket_async_result));
		return true;
	}

	/// <summary>Begins an asynchronous request to disconnect from a remote endpoint.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that references the asynchronous operation.</returns>
	/// <param name="reuseSocket">true if this socket can be reused after the connection is closed; otherwise, false. </param>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object that contains state information for this request. </param>
	/// <exception cref="T:System.NotSupportedException">The operating system is Windows 2000 or earlier, and this method requires Windows XP. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> object has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public IAsyncResult BeginDisconnect(bool reuseSocket, AsyncCallback callback, object state)
	{
		ThrowIfDisposedAndClosed();
		SocketAsyncResult socketAsyncResult = new SocketAsyncResult(this, callback, state, SocketOperation.Disconnect)
		{
			ReuseSocket = reuseSocket
		};
		IOSelector.Add(socketAsyncResult.Handle, new IOSelectorJob(IOOperation.Write, BeginDisconnectCallback, socketAsyncResult));
		return socketAsyncResult;
	}

	/// <summary>Ends a pending asynchronous disconnect request.</summary>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> object that stores state information and any user-defined data for this asynchronous operation. </param>
	/// <exception cref="T:System.NotSupportedException">The operating system is Windows 2000 or earlier, and this method requires Windows XP. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> object has been closed. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by a call to the <see cref="M:System.Net.Sockets.Socket.BeginDisconnect(System.Boolean,System.AsyncCallback,System.Object)" /> method. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.Socket.EndDisconnect(System.IAsyncResult)" /> was previously called for the asynchronous connection. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.Net.WebException">The disconnect request has timed out. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void EndDisconnect(IAsyncResult asyncResult)
	{
		ThrowIfDisposedAndClosed();
		SocketAsyncResult socketAsyncResult = ValidateEndIAsyncResult(asyncResult, "EndDisconnect", "asyncResult");
		if (!socketAsyncResult.IsCompleted)
		{
			socketAsyncResult.AsyncWaitHandle.WaitOne();
		}
		socketAsyncResult.CheckIfThrowDelayedException();
	}

	private static void Disconnect_internal(SafeSocketHandle safeHandle, bool reuse, out int error)
	{
		bool success = false;
		try
		{
			safeHandle.DangerousAddRef(ref success);
			Disconnect_internal(safeHandle.DangerousGetHandle(), reuse, out error);
		}
		finally
		{
			if (success)
			{
				safeHandle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Disconnect_internal(IntPtr sock, bool reuse, out int error);

	/// <summary>Receives data from a bound <see cref="T:System.Net.Sockets.Socket" /> into a receive buffer, using the specified <see cref="T:System.Net.Sockets.SocketFlags" />.</summary>
	/// <returns>The number of bytes received.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the storage location for the received data.</param>
	/// <param name="offset">The position in the <paramref name="buffer" /> parameter to store the received data. </param>
	/// <param name="size">The number of bytes to receive. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values.</param>
	/// <param name="errorCode">A <see cref="T:System.Net.Sockets.SocketError" /> object that stores the socket error.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or- <paramref name="offset" /> is greater than the length of <paramref name="buffer" />.-or- <paramref name="size" /> is less than 0.-or- <paramref name="size" /> is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">
	///   <paramref name="socketFlags" /> is not a valid combination of values.-or- The <see cref="P:System.Net.Sockets.Socket.LocalEndPoint" /> property is not set.-or- An operating system error occurs while accessing the <see cref="T:System.Net.Sockets.Socket" />. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller in the call stack does not have the required permissions. </exception>
	public unsafe int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode)
	{
		ThrowIfDisposedAndClosed();
		ThrowIfBufferNull(buffer);
		ThrowIfBufferOutOfRange(buffer, offset, size);
		int result;
		int error;
		fixed (byte* ptr = buffer)
		{
			result = Receive_internal(m_Handle, ptr + offset, size, socketFlags, out error, is_blocking);
		}
		errorCode = (SocketError)error;
		if (errorCode != 0 && errorCode != SocketError.WouldBlock && errorCode != SocketError.InProgress)
		{
			is_connected = false;
			is_bound = false;
			return result;
		}
		is_connected = true;
		return result;
	}

	/// <summary>Receives data from a bound <see cref="T:System.Net.Sockets.Socket" /> into the list of receive buffers, using the specified <see cref="T:System.Net.Sockets.SocketFlags" />.</summary>
	/// <returns>The number of bytes received.</returns>
	/// <param name="buffers">A list of <see cref="T:System.ArraySegment`1" />s of type <see cref="T:System.Byte" /> that contains the received data.</param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values.</param>
	/// <param name="errorCode">A <see cref="T:System.Net.Sockets.SocketError" /> object that stores the socket error.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffers" /> is null.-or-<paramref name="buffers" />.Count is zero.</exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred while attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	[CLSCompliant(false)]
	public unsafe int Receive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode)
	{
		ThrowIfDisposedAndClosed();
		if (buffers == null || buffers.Count == 0)
		{
			throw new ArgumentNullException("buffers");
		}
		int count = buffers.Count;
		GCHandle[] array = new GCHandle[count];
		int result;
		int error;
		try
		{
			fixed (WSABUF* ptr = new WSABUF[count])
			{
				for (int i = 0; i < count; i++)
				{
					ArraySegment<byte> arraySegment = buffers[i];
					if (arraySegment.Offset < 0 || arraySegment.Count < 0 || arraySegment.Count > arraySegment.Array.Length - arraySegment.Offset)
					{
						throw new ArgumentOutOfRangeException("segment");
					}
					try
					{
					}
					finally
					{
						array[i] = GCHandle.Alloc(arraySegment.Array, GCHandleType.Pinned);
					}
					ptr[i].len = arraySegment.Count;
					ptr[i].buf = Marshal.UnsafeAddrOfPinnedArrayElement(arraySegment.Array, arraySegment.Offset);
				}
				result = Receive_internal(m_Handle, ptr, count, socketFlags, out error, is_blocking);
			}
		}
		finally
		{
			for (int j = 0; j < count; j++)
			{
				if (array[j].IsAllocated)
				{
					array[j].Free();
				}
			}
		}
		errorCode = (SocketError)error;
		return result;
	}

	/// <summary>Begins an asynchronous request to receive data from a connected <see cref="T:System.Net.Sockets.Socket" /> object.</summary>
	/// <returns>Returns true if the I/O operation is pending. The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will be raised upon completion of the operation. Returns false if the I/O operation completed synchronously. In this case, The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will not be raised and the <paramref name="e" /> object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.</returns>
	/// <param name="e">The <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object to use for this asynchronous socket operation.</param>
	/// <exception cref="T:System.ArgumentException">An argument was invalid. The <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer" /> or <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.BufferList" /> properties on the <paramref name="e" /> parameter must reference valid buffers. One or the other of these properties may be set, but not both at the same time.</exception>
	/// <exception cref="T:System.InvalidOperationException">A socket operation was already in progress using the <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object specified in the <paramref name="e" /> parameter.</exception>
	/// <exception cref="T:System.NotSupportedException">Windows XP or later is required for this method.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	public bool ReceiveAsync(SocketAsyncEventArgs e)
	{
		ThrowIfDisposedAndClosed();
		if (e.Buffer == null && e.BufferList == null)
		{
			throw new NullReferenceException("Either e.Buffer or e.BufferList must be valid buffers.");
		}
		if (e.Buffer == null)
		{
			InitSocketAsyncEventArgs(e, ReceiveAsyncCallback, e, SocketOperation.ReceiveGeneric);
			e.socket_async_result.Buffers = e.BufferList;
			QueueIOSelectorJob(ReadSem, e.socket_async_result.Handle, new IOSelectorJob(IOOperation.Read, BeginReceiveGenericCallback, e.socket_async_result));
		}
		else
		{
			InitSocketAsyncEventArgs(e, ReceiveAsyncCallback, e, SocketOperation.Receive);
			e.socket_async_result.Buffer = e.Buffer;
			e.socket_async_result.Offset = e.Offset;
			e.socket_async_result.Size = e.Count;
			QueueIOSelectorJob(ReadSem, e.socket_async_result.Handle, new IOSelectorJob(IOOperation.Read, BeginReceiveCallback, e.socket_async_result));
		}
		return true;
	}

	/// <summary>Begins to asynchronously receive data from a connected <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous read.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the storage location for the received data.</param>
	/// <param name="offset">The location in <paramref name="buffer" /> to store the received data. </param>
	/// <param name="size">The number of bytes to receive. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values.</param>
	/// <param name="errorCode">A <see cref="T:System.Net.Sockets.SocketError" /> object that stores the socket error.</param>
	/// <param name="callback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete.</param>
	/// <param name="state">A user-defined object that contains information about the receive operation. This object is passed to the <see cref="M:System.Net.Sockets.Socket.EndReceive(System.IAsyncResult)" /> delegate when the operation is complete.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or- <paramref name="offset" /> is greater than the length of <paramref name="buffer" />.-or- <paramref name="size" /> is less than 0.-or- <paramref name="size" /> is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter. </exception>
	public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
	{
		ThrowIfDisposedAndClosed();
		ThrowIfBufferNull(buffer);
		ThrowIfBufferOutOfRange(buffer, offset, size);
		errorCode = SocketError.Success;
		SocketAsyncResult socketAsyncResult = new SocketAsyncResult(this, callback, state, SocketOperation.Receive)
		{
			Buffer = buffer,
			Offset = offset,
			Size = size,
			SockFlags = socketFlags
		};
		QueueIOSelectorJob(ReadSem, socketAsyncResult.Handle, new IOSelectorJob(IOOperation.Read, BeginReceiveCallback, socketAsyncResult));
		return socketAsyncResult;
	}

	/// <summary>Begins to asynchronously receive data from a connected <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous read.</returns>
	/// <param name="buffers">An array of type <see cref="T:System.Byte" /> that is the storage location for the received data.</param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <param name="errorCode">A <see cref="T:System.Net.Sockets.SocketError" /> object that stores the socket error.</param>
	/// <param name="callback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete.</param>
	/// <param name="state">A user-defined object that contains information about the receive operation. This object is passed to the <see cref="M:System.Net.Sockets.Socket.EndReceive(System.IAsyncResult)" /> delegate when the operation is complete.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	[CLSCompliant(false)]
	public IAsyncResult BeginReceive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
	{
		ThrowIfDisposedAndClosed();
		if (buffers == null)
		{
			throw new ArgumentNullException("buffers");
		}
		errorCode = SocketError.Success;
		SocketAsyncResult socketAsyncResult = new SocketAsyncResult(this, callback, state, SocketOperation.ReceiveGeneric)
		{
			Buffers = buffers,
			SockFlags = socketFlags
		};
		QueueIOSelectorJob(ReadSem, socketAsyncResult.Handle, new IOSelectorJob(IOOperation.Read, BeginReceiveGenericCallback, socketAsyncResult));
		return socketAsyncResult;
	}

	/// <summary>Ends a pending asynchronous read.</summary>
	/// <returns>The number of bytes received.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> that stores state information and any user defined data for this asynchronous operation.</param>
	/// <param name="errorCode">A <see cref="T:System.Net.Sockets.SocketError" /> object that stores the socket error.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by a call to the <see cref="M:System.Net.Sockets.Socket.BeginReceive(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)" /> method. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.Socket.EndReceive(System.IAsyncResult)" /> was previously called for the asynchronous read. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	public int EndReceive(IAsyncResult asyncResult, out SocketError errorCode)
	{
		ThrowIfDisposedAndClosed();
		SocketAsyncResult socketAsyncResult = ValidateEndIAsyncResult(asyncResult, "EndReceive", "asyncResult");
		if (!socketAsyncResult.IsCompleted)
		{
			socketAsyncResult.AsyncWaitHandle.WaitOne();
		}
		errorCode = socketAsyncResult.ErrorCode;
		if (errorCode != 0 && errorCode != SocketError.WouldBlock && errorCode != SocketError.InProgress)
		{
			is_connected = false;
		}
		if (errorCode == SocketError.Success)
		{
			socketAsyncResult.CheckIfThrowDelayedException();
		}
		return socketAsyncResult.Total;
	}

	private unsafe static int Receive_internal(SafeSocketHandle safeHandle, WSABUF* bufarray, int count, SocketFlags flags, out int error, bool blocking)
	{
		try
		{
			safeHandle.RegisterForBlockingSyscall();
			return Receive_internal(safeHandle.DangerousGetHandle(), bufarray, count, flags, out error, blocking);
		}
		finally
		{
			safeHandle.UnRegisterForBlockingSyscall();
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern int Receive_internal(IntPtr sock, WSABUF* bufarray, int count, SocketFlags flags, out int error, bool blocking);

	private unsafe static int Receive_internal(SafeSocketHandle safeHandle, byte* buffer, int count, SocketFlags flags, out int error, bool blocking)
	{
		try
		{
			safeHandle.RegisterForBlockingSyscall();
			return Receive_internal(safeHandle.DangerousGetHandle(), buffer, count, flags, out error, blocking);
		}
		finally
		{
			safeHandle.UnRegisterForBlockingSyscall();
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern int Receive_internal(IntPtr sock, byte* buffer, int count, SocketFlags flags, out int error, bool blocking);

	/// <summary>Receives the specified number of bytes of data into the specified location of the data buffer, using the specified <see cref="T:System.Net.Sockets.SocketFlags" />, and stores the endpoint.</summary>
	/// <returns>The number of bytes received.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the storage location for received data. </param>
	/// <param name="offset">The position in the <paramref name="buffer" /> parameter to store the received data. </param>
	/// <param name="size">The number of bytes to receive. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <param name="remoteEP">An <see cref="T:System.Net.EndPoint" />, passed by reference, that represents the remote server. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.-or- <paramref name="remoteEP" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or- <paramref name="offset" /> is greater than the length of <paramref name="buffer" />.-or- <paramref name="size" /> is less than 0.-or- <paramref name="size" /> is greater than the length of the <paramref name="buffer" /> minus the value of the offset parameter. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">
	///   <paramref name="socketFlags" /> is not a valid combination of values.-or- The <see cref="P:System.Net.Sockets.Socket.LocalEndPoint" /> property was not set.-or- An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int ReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
	{
		ThrowIfDisposedAndClosed();
		ThrowIfBufferNull(buffer);
		ThrowIfBufferOutOfRange(buffer, offset, size);
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		SocketError errorCode;
		int result = ReceiveFrom(buffer, offset, size, socketFlags, ref remoteEP, out errorCode);
		if (errorCode != 0)
		{
			throw new SocketException(errorCode);
		}
		return result;
	}

	internal unsafe int ReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, out SocketError errorCode)
	{
		SocketAddress sockaddr = remoteEP.Serialize();
		int result;
		int error;
		fixed (byte* ptr = buffer)
		{
			result = ReceiveFrom_internal(m_Handle, ptr + offset, size, socketFlags, ref sockaddr, out error, is_blocking);
		}
		errorCode = (SocketError)error;
		if (errorCode != 0)
		{
			if (errorCode != SocketError.WouldBlock && errorCode != SocketError.InProgress)
			{
				is_connected = false;
			}
			else if (errorCode == SocketError.WouldBlock && is_blocking)
			{
				errorCode = SocketError.TimedOut;
			}
			return 0;
		}
		is_connected = true;
		is_bound = true;
		if (sockaddr != null)
		{
			remoteEP = remoteEP.Create(sockaddr);
		}
		seed_endpoint = remoteEP;
		return result;
	}

	/// <summary>Begins to asynchronously receive data from a specified network device.</summary>
	/// <returns>Returns true if the I/O operation is pending. The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will be raised upon completion of the operation. Returns false if the I/O operation completed synchronously. In this case, The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will not be raised and the <paramref name="e" /> object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.</returns>
	/// <param name="e">The <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object to use for this asynchronous socket operation.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint" /> cannot be null.</exception>
	/// <exception cref="T:System.InvalidOperationException">A socket operation was already in progress using the <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object specified in the <paramref name="e" /> parameter.</exception>
	/// <exception cref="T:System.NotSupportedException">Windows XP or later is required for this method.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. </exception>
	public bool ReceiveFromAsync(SocketAsyncEventArgs e)
	{
		ThrowIfDisposedAndClosed();
		if (e.BufferList != null)
		{
			throw new NotSupportedException("Mono doesn't support using BufferList at this point.");
		}
		if (e.RemoteEndPoint == null)
		{
			throw new ArgumentNullException("remoteEP", "Value cannot be null.");
		}
		InitSocketAsyncEventArgs(e, ReceiveFromAsyncCallback, e, SocketOperation.ReceiveFrom);
		e.socket_async_result.Buffer = e.Buffer;
		e.socket_async_result.Offset = e.Offset;
		e.socket_async_result.Size = e.Count;
		e.socket_async_result.EndPoint = e.RemoteEndPoint;
		e.socket_async_result.SockFlags = e.SocketFlags;
		QueueIOSelectorJob(ReadSem, e.socket_async_result.Handle, new IOSelectorJob(IOOperation.Read, BeginReceiveFromCallback, e.socket_async_result));
		return true;
	}

	/// <summary>Begins to asynchronously receive data from a specified network device.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous read.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the storage location for the received data. </param>
	/// <param name="offset">The zero-based position in the <paramref name="buffer" /> parameter at which to store the data. </param>
	/// <param name="size">The number of bytes to receive. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <param name="remoteEP">An <see cref="T:System.Net.EndPoint" /> that represents the source of the data. </param>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object that contains state information for this request. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.-or- <paramref name="remoteEP" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or- <paramref name="offset" /> is greater than the length of <paramref name="buffer" />.-or- <paramref name="size" /> is less than 0.-or- <paramref name="size" /> is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller higher in the call stack does not have permission for the requested operation. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public IAsyncResult BeginReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state)
	{
		ThrowIfDisposedAndClosed();
		ThrowIfBufferNull(buffer);
		ThrowIfBufferOutOfRange(buffer, offset, size);
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		SocketAsyncResult socketAsyncResult = new SocketAsyncResult(this, callback, state, SocketOperation.ReceiveFrom)
		{
			Buffer = buffer,
			Offset = offset,
			Size = size,
			SockFlags = socketFlags,
			EndPoint = remoteEP
		};
		QueueIOSelectorJob(ReadSem, socketAsyncResult.Handle, new IOSelectorJob(IOOperation.Read, BeginReceiveFromCallback, socketAsyncResult));
		return socketAsyncResult;
	}

	/// <summary>Ends a pending asynchronous read from a specific endpoint.</summary>
	/// <returns>If successful, the number of bytes received. If unsuccessful, returns 0.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> that stores state information and any user defined data for this asynchronous operation. </param>
	/// <param name="endPoint">The source <see cref="T:System.Net.EndPoint" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by a call to the <see cref="M:System.Net.Sockets.Socket.BeginReceiveFrom(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint@,System.AsyncCallback,System.Object)" /> method. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.Socket.EndReceiveFrom(System.IAsyncResult,System.Net.EndPoint@)" /> was previously called for the asynchronous read. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int EndReceiveFrom(IAsyncResult asyncResult, ref EndPoint endPoint)
	{
		ThrowIfDisposedAndClosed();
		if (endPoint == null)
		{
			throw new ArgumentNullException("endPoint");
		}
		SocketAsyncResult socketAsyncResult = ValidateEndIAsyncResult(asyncResult, "EndReceiveFrom", "asyncResult");
		if (!socketAsyncResult.IsCompleted)
		{
			socketAsyncResult.AsyncWaitHandle.WaitOne();
		}
		socketAsyncResult.CheckIfThrowDelayedException();
		endPoint = socketAsyncResult.EndPoint;
		return socketAsyncResult.Total;
	}

	private unsafe static int ReceiveFrom_internal(SafeSocketHandle safeHandle, byte* buffer, int count, SocketFlags flags, ref SocketAddress sockaddr, out int error, bool blocking)
	{
		try
		{
			safeHandle.RegisterForBlockingSyscall();
			return ReceiveFrom_internal(safeHandle.DangerousGetHandle(), buffer, count, flags, ref sockaddr, out error, blocking);
		}
		finally
		{
			safeHandle.UnRegisterForBlockingSyscall();
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern int ReceiveFrom_internal(IntPtr sock, byte* buffer, int count, SocketFlags flags, ref SocketAddress sockaddr, out int error, bool blocking);

	/// <summary>Receives the specified number of bytes of data into the specified location of the data buffer, using the specified <see cref="T:System.Net.Sockets.SocketFlags" />, and stores the endpoint and packet information.</summary>
	/// <returns>The number of bytes received.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the storage location for received data.</param>
	/// <param name="offset">The position in the <paramref name="buffer" /> parameter to store the received data.</param>
	/// <param name="size">The number of bytes to receive.</param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values.</param>
	/// <param name="remoteEP">An <see cref="T:System.Net.EndPoint" />, passed by reference, that represents the remote server.</param>
	/// <param name="ipPacketInformation">An <see cref="T:System.Net.Sockets.IPPacketInformation" /> holding address and interface information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.- or- <paramref name="remoteEP" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or- <paramref name="offset" /> is greater than the length of <paramref name="buffer" />.-or- <paramref name="size" /> is less than 0.-or- <paramref name="size" /> is greater than the length of the <paramref name="buffer" /> minus the value of the offset parameter. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">
	///   <paramref name="socketFlags" /> is not a valid combination of values.-or- The <see cref="P:System.Net.Sockets.Socket.LocalEndPoint" /> property was not set.-or- The .NET Framework is running on an AMD 64-bit processor.-or- An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.NotSupportedException">The operating system is Windows 2000 or earlier, and this method requires Windows XP.</exception>
	[System.MonoTODO("Not implemented")]
	public int ReceiveMessageFrom(byte[] buffer, int offset, int size, ref SocketFlags socketFlags, ref EndPoint remoteEP, out IPPacketInformation ipPacketInformation)
	{
		ThrowIfDisposedAndClosed();
		ThrowIfBufferNull(buffer);
		ThrowIfBufferOutOfRange(buffer, offset, size);
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		throw new NotImplementedException();
	}

	/// <summary>Begins to asynchronously receive the specified number of bytes of data into the specified location in the data buffer, using the specified <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.SocketFlags" />, and stores the endpoint and packet information.</summary>
	/// <returns>Returns true if the I/O operation is pending. The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will be raised upon completion of the operation. Returns false if the I/O operation completed synchronously. In this case, The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will not be raised and the <paramref name="e" /> object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.</returns>
	/// <param name="e">The <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object to use for this asynchronous socket operation.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint" /> cannot be null.</exception>
	/// <exception cref="T:System.NotSupportedException">Windows XP or later is required for this method.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. </exception>
	[System.MonoTODO("Not implemented")]
	public bool ReceiveMessageFromAsync(SocketAsyncEventArgs e)
	{
		ThrowIfDisposedAndClosed();
		throw new NotImplementedException();
	}

	/// <summary>Begins to asynchronously receive the specified number of bytes of data into the specified location of the data buffer, using the specified <see cref="T:System.Net.Sockets.SocketFlags" />, and stores the endpoint and packet information..</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous read.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that is the storage location for the received data. </param>
	/// <param name="offset">The zero-based position in the <paramref name="buffer" /> parameter at which to store the data.</param>
	/// <param name="size">The number of bytes to receive. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values.</param>
	/// <param name="remoteEP">An <see cref="T:System.Net.EndPoint" /> that represents the source of the data.</param>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate.</param>
	/// <param name="state">An object that contains state information for this request.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.-or- <paramref name="remoteEP" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or- <paramref name="offset" /> is greater than the length of <paramref name="buffer" />.-or- <paramref name="size" /> is less than 0.-or- <paramref name="size" /> is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.NotSupportedException">The operating system is Windows 2000 or earlier, and this method requires Windows XP.</exception>
	[System.MonoTODO]
	public IAsyncResult BeginReceiveMessageFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state)
	{
		ThrowIfDisposedAndClosed();
		ThrowIfBufferNull(buffer);
		ThrowIfBufferOutOfRange(buffer, offset, size);
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		throw new NotImplementedException();
	}

	/// <summary>Ends a pending asynchronous read from a specific endpoint. This method also reveals more information about the packet than <see cref="M:System.Net.Sockets.Socket.EndReceiveFrom(System.IAsyncResult,System.Net.EndPoint@)" />.</summary>
	/// <returns>If successful, the number of bytes received. If unsuccessful, returns 0.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> that stores state information and any user defined data for this asynchronous operation.</param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values for the received packet.</param>
	/// <param name="endPoint">The source <see cref="T:System.Net.EndPoint" />.</param>
	/// <param name="ipPacketInformation">The <see cref="T:System.Net.IPAddress" /> and interface of the received packet.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null-or- <paramref name="endPoint" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by a call to the <see cref="M:System.Net.Sockets.Socket.BeginReceiveMessageFrom(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint@,System.AsyncCallback,System.Object)" /> method. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.Socket.EndReceiveMessageFrom(System.IAsyncResult,System.Net.Sockets.SocketFlags@,System.Net.EndPoint@,System.Net.Sockets.IPPacketInformation@)" /> was previously called for the asynchronous read. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	[System.MonoTODO]
	public int EndReceiveMessageFrom(IAsyncResult asyncResult, ref SocketFlags socketFlags, ref EndPoint endPoint, out IPPacketInformation ipPacketInformation)
	{
		ThrowIfDisposedAndClosed();
		if (endPoint == null)
		{
			throw new ArgumentNullException("endPoint");
		}
		ValidateEndIAsyncResult(asyncResult, "EndReceiveMessageFrom", "asyncResult");
		throw new NotImplementedException();
	}

	/// <summary>Sends the specified number of bytes of data to a connected <see cref="T:System.Net.Sockets.Socket" />, starting at the specified offset, and using the specified <see cref="T:System.Net.Sockets.SocketFlags" /></summary>
	/// <returns>The number of bytes sent to the <see cref="T:System.Net.Sockets.Socket" />.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the data to be sent. </param>
	/// <param name="offset">The position in the data buffer at which to begin sending data. </param>
	/// <param name="size">The number of bytes to send. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <param name="errorCode">A <see cref="T:System.Net.Sockets.SocketError" /> object that stores the socket error.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or- <paramref name="offset" /> is greater than the length of <paramref name="buffer" />.-or- <paramref name="size" /> is less than 0.-or- <paramref name="size" /> is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">
	///   <paramref name="socketFlags" /> is not a valid combination of values.-or- An operating system error occurs while accessing the <see cref="T:System.Net.Sockets.Socket" />. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	public unsafe int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode)
	{
		ThrowIfDisposedAndClosed();
		ThrowIfBufferNull(buffer);
		ThrowIfBufferOutOfRange(buffer, offset, size);
		if (size == 0)
		{
			errorCode = SocketError.Success;
			return 0;
		}
		int num = 0;
		do
		{
			int error;
			fixed (byte* ptr = buffer)
			{
				num += Send_internal(m_Handle, ptr + (offset + num), size - num, socketFlags, out error, is_blocking);
			}
			errorCode = (SocketError)error;
			if (errorCode != 0 && errorCode != SocketError.WouldBlock && errorCode != SocketError.InProgress)
			{
				is_connected = false;
				is_bound = false;
				break;
			}
			is_connected = true;
		}
		while (num < size);
		return num;
	}

	/// <summary>Sends the set of buffers in the list to a connected <see cref="T:System.Net.Sockets.Socket" />, using the specified <see cref="T:System.Net.Sockets.SocketFlags" />.</summary>
	/// <returns>The number of bytes sent to the <see cref="T:System.Net.Sockets.Socket" />.</returns>
	/// <param name="buffers">A list of <see cref="T:System.ArraySegment`1" />s of type <see cref="T:System.Byte" /> that contains the data to be sent.</param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values.</param>
	/// <param name="errorCode">A <see cref="T:System.Net.Sockets.SocketError" /> object that stores the socket error.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffers" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="buffers" /> is empty.</exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	[CLSCompliant(false)]
	public unsafe int Send(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode)
	{
		ThrowIfDisposedAndClosed();
		if (buffers == null)
		{
			throw new ArgumentNullException("buffers");
		}
		if (buffers.Count == 0)
		{
			throw new ArgumentException("Buffer is empty", "buffers");
		}
		int count = buffers.Count;
		GCHandle[] array = new GCHandle[count];
		int result;
		int error;
		try
		{
			fixed (WSABUF* ptr = new WSABUF[count])
			{
				for (int i = 0; i < count; i++)
				{
					ArraySegment<byte> arraySegment = buffers[i];
					if (arraySegment.Offset < 0 || arraySegment.Count < 0 || arraySegment.Count > arraySegment.Array.Length - arraySegment.Offset)
					{
						throw new ArgumentOutOfRangeException("segment");
					}
					try
					{
					}
					finally
					{
						array[i] = GCHandle.Alloc(arraySegment.Array, GCHandleType.Pinned);
					}
					ptr[i].len = arraySegment.Count;
					ptr[i].buf = Marshal.UnsafeAddrOfPinnedArrayElement(arraySegment.Array, arraySegment.Offset);
				}
				result = Send_internal(m_Handle, ptr, count, socketFlags, out error, is_blocking);
			}
		}
		finally
		{
			for (int j = 0; j < count; j++)
			{
				if (array[j].IsAllocated)
				{
					array[j].Free();
				}
			}
		}
		errorCode = (SocketError)error;
		return result;
	}

	/// <summary>Sends data asynchronously to a connected <see cref="T:System.Net.Sockets.Socket" /> object.</summary>
	/// <returns>Returns true if the I/O operation is pending. The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will be raised upon completion of the operation. Returns false if the I/O operation completed synchronously. In this case, The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will not be raised and the <paramref name="e" /> object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.</returns>
	/// <param name="e">The <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object to use for this asynchronous socket operation.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer" /> or <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.BufferList" /> properties on the <paramref name="e" /> parameter must reference valid buffers. One or the other of these properties may be set, but not both at the same time.</exception>
	/// <exception cref="T:System.InvalidOperationException">A socket operation was already in progress using the <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object specified in the <paramref name="e" /> parameter.</exception>
	/// <exception cref="T:System.NotSupportedException">Windows XP or later is required for this method.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">The <see cref="T:System.Net.Sockets.Socket" /> is not yet connected or was not obtained via an <see cref="M:System.Net.Sockets.Socket.Accept" />, <see cref="M:System.Net.Sockets.Socket.AcceptAsync(System.Net.Sockets.SocketAsyncEventArgs)" />,or <see cref="Overload:System.Net.Sockets.Socket.BeginAccept" />, method.</exception>
	public bool SendAsync(SocketAsyncEventArgs e)
	{
		ThrowIfDisposedAndClosed();
		if (e.Buffer == null && e.BufferList == null)
		{
			throw new NullReferenceException("Either e.Buffer or e.BufferList must be valid buffers.");
		}
		if (e.Buffer == null)
		{
			InitSocketAsyncEventArgs(e, SendAsyncCallback, e, SocketOperation.SendGeneric);
			e.socket_async_result.Buffers = e.BufferList;
			QueueIOSelectorJob(WriteSem, e.socket_async_result.Handle, new IOSelectorJob(IOOperation.Write, BeginSendGenericCallback, e.socket_async_result));
		}
		else
		{
			InitSocketAsyncEventArgs(e, SendAsyncCallback, e, SocketOperation.Send);
			e.socket_async_result.Buffer = e.Buffer;
			e.socket_async_result.Offset = e.Offset;
			e.socket_async_result.Size = e.Count;
			QueueIOSelectorJob(WriteSem, e.socket_async_result.Handle, new IOSelectorJob(IOOperation.Write, delegate(IOAsyncResult s)
			{
				BeginSendCallback((SocketAsyncResult)s, 0);
			}, e.socket_async_result));
		}
		return true;
	}

	/// <summary>Sends data asynchronously to a connected <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous send.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the data to send. </param>
	/// <param name="offset">The zero-based position in the <paramref name="buffer" /> parameter at which to begin sending data. </param>
	/// <param name="size">The number of bytes to send. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <param name="errorCode">A <see cref="T:System.Net.Sockets.SocketError" /> object that stores the socket error.</param>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object that contains state information for this request. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See remarks section below. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or- <paramref name="offset" /> is less than the length of <paramref name="buffer" />.-or- <paramref name="size" /> is less than 0.-or- <paramref name="size" /> is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	public IAsyncResult BeginSend(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
	{
		ThrowIfDisposedAndClosed();
		ThrowIfBufferNull(buffer);
		ThrowIfBufferOutOfRange(buffer, offset, size);
		if (!is_connected)
		{
			errorCode = SocketError.NotConnected;
			return null;
		}
		errorCode = SocketError.Success;
		SocketAsyncResult socketAsyncResult = new SocketAsyncResult(this, callback, state, SocketOperation.Send)
		{
			Buffer = buffer,
			Offset = offset,
			Size = size,
			SockFlags = socketFlags
		};
		QueueIOSelectorJob(WriteSem, socketAsyncResult.Handle, new IOSelectorJob(IOOperation.Write, delegate(IOAsyncResult s)
		{
			BeginSendCallback((SocketAsyncResult)s, 0);
		}, socketAsyncResult));
		return socketAsyncResult;
	}

	private unsafe static void BeginSendCallback(SocketAsyncResult sockares, int sent_so_far)
	{
		int num = 0;
		try
		{
			fixed (byte* buffer = sockares.Buffer)
			{
				num = Send_internal(sockares.socket.m_Handle, buffer + sockares.Offset, sockares.Size, sockares.SockFlags, out sockares.error, blocking: false);
			}
		}
		catch (Exception e)
		{
			sockares.Complete(e);
			return;
		}
		if (sockares.error == 0)
		{
			sent_so_far += num;
			sockares.Offset += num;
			sockares.Size -= num;
			if (sockares.socket.CleanedUp)
			{
				sockares.Complete(sent_so_far);
				return;
			}
			if (sockares.Size > 0)
			{
				IOSelector.Add(sockares.Handle, new IOSelectorJob(IOOperation.Write, delegate(IOAsyncResult s)
				{
					BeginSendCallback((SocketAsyncResult)s, sent_so_far);
				}, sockares));
				return;
			}
			sockares.Total = sent_so_far;
		}
		sockares.Complete(sent_so_far);
	}

	/// <summary>Sends data asynchronously to a connected <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous send.</returns>
	/// <param name="buffers">An array of type <see cref="T:System.Byte" /> that contains the data to send. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values.</param>
	/// <param name="errorCode">A <see cref="T:System.Net.Sockets.SocketError" /> object that stores the socket error.</param>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object that contains state information for this request. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffers" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="buffers" /> is empty.</exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See remarks section below. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	[CLSCompliant(false)]
	public IAsyncResult BeginSend(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
	{
		ThrowIfDisposedAndClosed();
		if (buffers == null)
		{
			throw new ArgumentNullException("buffers");
		}
		if (!is_connected)
		{
			errorCode = SocketError.NotConnected;
			return null;
		}
		errorCode = SocketError.Success;
		SocketAsyncResult socketAsyncResult = new SocketAsyncResult(this, callback, state, SocketOperation.SendGeneric)
		{
			Buffers = buffers,
			SockFlags = socketFlags
		};
		QueueIOSelectorJob(WriteSem, socketAsyncResult.Handle, new IOSelectorJob(IOOperation.Write, BeginSendGenericCallback, socketAsyncResult));
		return socketAsyncResult;
	}

	/// <summary>Ends a pending asynchronous send.</summary>
	/// <returns>If successful, the number of bytes sent to the <see cref="T:System.Net.Sockets.Socket" />; otherwise, an invalid <see cref="T:System.Net.Sockets.Socket" /> error.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> that stores state information for this asynchronous operation.</param>
	/// <param name="errorCode">A <see cref="T:System.Net.Sockets.SocketError" /> object that stores the socket error.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by a call to the <see cref="M:System.Net.Sockets.Socket.BeginSend(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)" /> method. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.Socket.EndSend(System.IAsyncResult)" /> was previously called for the asynchronous send. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	public int EndSend(IAsyncResult asyncResult, out SocketError errorCode)
	{
		ThrowIfDisposedAndClosed();
		SocketAsyncResult socketAsyncResult = ValidateEndIAsyncResult(asyncResult, "EndSend", "asyncResult");
		if (!socketAsyncResult.IsCompleted)
		{
			socketAsyncResult.AsyncWaitHandle.WaitOne();
		}
		errorCode = socketAsyncResult.ErrorCode;
		if (errorCode != 0 && errorCode != SocketError.WouldBlock && errorCode != SocketError.InProgress)
		{
			is_connected = false;
		}
		if (errorCode == SocketError.Success)
		{
			socketAsyncResult.CheckIfThrowDelayedException();
		}
		return socketAsyncResult.Total;
	}

	private unsafe static int Send_internal(SafeSocketHandle safeHandle, WSABUF* bufarray, int count, SocketFlags flags, out int error, bool blocking)
	{
		try
		{
			safeHandle.RegisterForBlockingSyscall();
			return Send_internal(safeHandle.DangerousGetHandle(), bufarray, count, flags, out error, blocking);
		}
		finally
		{
			safeHandle.UnRegisterForBlockingSyscall();
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern int Send_internal(IntPtr sock, WSABUF* bufarray, int count, SocketFlags flags, out int error, bool blocking);

	private unsafe static int Send_internal(SafeSocketHandle safeHandle, byte* buffer, int count, SocketFlags flags, out int error, bool blocking)
	{
		try
		{
			safeHandle.RegisterForBlockingSyscall();
			return Send_internal(safeHandle.DangerousGetHandle(), buffer, count, flags, out error, blocking);
		}
		finally
		{
			safeHandle.UnRegisterForBlockingSyscall();
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern int Send_internal(IntPtr sock, byte* buffer, int count, SocketFlags flags, out int error, bool blocking);

	/// <summary>Sends the specified number of bytes of data to the specified endpoint, starting at the specified location in the buffer, and using the specified <see cref="T:System.Net.Sockets.SocketFlags" />.</summary>
	/// <returns>The number of bytes sent.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the data to be sent. </param>
	/// <param name="offset">The position in the data buffer at which to begin sending data. </param>
	/// <param name="size">The number of bytes to send. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <param name="remoteEP">The <see cref="T:System.Net.EndPoint" /> that represents the destination location for the data. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.-or- <paramref name="remoteEP" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or- <paramref name="offset" /> is greater than the length of <paramref name="buffer" />.-or- <paramref name="size" /> is less than 0.-or- <paramref name="size" /> is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">
	///   <paramref name="socketFlags" /> is not a valid combination of values.-or- An operating system error occurs while accessing the <see cref="T:System.Net.Sockets.Socket" />. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller in the call stack does not have the required permissions. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public unsafe int SendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP)
	{
		ThrowIfDisposedAndClosed();
		ThrowIfBufferNull(buffer);
		ThrowIfBufferOutOfRange(buffer, offset, size);
		if (remoteEP == null)
		{
			throw new ArgumentNullException("remoteEP");
		}
		int result;
		int error;
		fixed (byte* ptr = buffer)
		{
			result = SendTo_internal(m_Handle, ptr + offset, size, socketFlags, remoteEP.Serialize(), out error, is_blocking);
		}
		SocketError socketError = (SocketError)error;
		if (socketError != 0)
		{
			if (socketError != SocketError.WouldBlock && socketError != SocketError.InProgress)
			{
				is_connected = false;
			}
			throw new SocketException(error);
		}
		is_connected = true;
		is_bound = true;
		seed_endpoint = remoteEP;
		return result;
	}

	/// <summary>Sends data asynchronously to a specific remote host.</summary>
	/// <returns>Returns true if the I/O operation is pending. The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will be raised upon completion of the operation. Returns false if the I/O operation completed synchronously. In this case, The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will not be raised and the <paramref name="e" /> object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.</returns>
	/// <param name="e">The <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object to use for this asynchronous socket operation.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint" /> cannot be null.</exception>
	/// <exception cref="T:System.InvalidOperationException">A socket operation was already in progress using the <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object specified in the <paramref name="e" /> parameter.</exception>
	/// <exception cref="T:System.NotSupportedException">Windows XP or later is required for this method.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">The protocol specified is connection-oriented, but the <see cref="T:System.Net.Sockets.Socket" /> is not yet connected.</exception>
	public bool SendToAsync(SocketAsyncEventArgs e)
	{
		ThrowIfDisposedAndClosed();
		if (e.BufferList != null)
		{
			throw new NotSupportedException("Mono doesn't support using BufferList at this point.");
		}
		if (e.RemoteEndPoint == null)
		{
			throw new ArgumentNullException("remoteEP", "Value cannot be null.");
		}
		InitSocketAsyncEventArgs(e, SendToAsyncCallback, e, SocketOperation.SendTo);
		e.socket_async_result.Buffer = e.Buffer;
		e.socket_async_result.Offset = e.Offset;
		e.socket_async_result.Size = e.Count;
		e.socket_async_result.SockFlags = e.SocketFlags;
		e.socket_async_result.EndPoint = e.RemoteEndPoint;
		QueueIOSelectorJob(WriteSem, e.socket_async_result.Handle, new IOSelectorJob(IOOperation.Write, delegate(IOAsyncResult s)
		{
			BeginSendToCallback((SocketAsyncResult)s, 0);
		}, e.socket_async_result));
		return true;
	}

	/// <summary>Sends data asynchronously to a specific remote host.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous send.</returns>
	/// <param name="buffer">An array of type <see cref="T:System.Byte" /> that contains the data to send. </param>
	/// <param name="offset">The zero-based position in <paramref name="buffer" /> at which to begin sending data. </param>
	/// <param name="size">The number of bytes to send. </param>
	/// <param name="socketFlags">A bitwise combination of the <see cref="T:System.Net.Sockets.SocketFlags" /> values. </param>
	/// <param name="remoteEP">An <see cref="T:System.Net.EndPoint" /> that represents the remote device. </param>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object that contains state information for this request. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.-or- <paramref name="remoteEP" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is less than 0.-or- <paramref name="offset" /> is greater than the length of <paramref name="buffer" />.-or- <paramref name="size" /> is less than 0.-or- <paramref name="size" /> is greater than the length of <paramref name="buffer" /> minus the value of the <paramref name="offset" /> parameter. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller higher in the call stack does not have permission for the requested operation. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public IAsyncResult BeginSendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP, AsyncCallback callback, object state)
	{
		ThrowIfDisposedAndClosed();
		ThrowIfBufferNull(buffer);
		ThrowIfBufferOutOfRange(buffer, offset, size);
		SocketAsyncResult socketAsyncResult = new SocketAsyncResult(this, callback, state, SocketOperation.SendTo)
		{
			Buffer = buffer,
			Offset = offset,
			Size = size,
			SockFlags = socketFlags,
			EndPoint = remoteEP
		};
		QueueIOSelectorJob(WriteSem, socketAsyncResult.Handle, new IOSelectorJob(IOOperation.Write, delegate(IOAsyncResult s)
		{
			BeginSendToCallback((SocketAsyncResult)s, 0);
		}, socketAsyncResult));
		return socketAsyncResult;
	}

	private static void BeginSendToCallback(SocketAsyncResult sockares, int sent_so_far)
	{
		int num = 0;
		try
		{
			num = sockares.socket.SendTo(sockares.Buffer, sockares.Offset, sockares.Size, sockares.SockFlags, sockares.EndPoint);
			if (sockares.error == 0)
			{
				sent_so_far += num;
				sockares.Offset += num;
				sockares.Size -= num;
			}
			if (sockares.Size > 0)
			{
				IOSelector.Add(sockares.Handle, new IOSelectorJob(IOOperation.Write, delegate(IOAsyncResult s)
				{
					BeginSendToCallback((SocketAsyncResult)s, sent_so_far);
				}, sockares));
				return;
			}
			sockares.Total = sent_so_far;
		}
		catch (Exception e)
		{
			sockares.Complete(e);
			return;
		}
		sockares.Complete();
	}

	/// <summary>Ends a pending asynchronous send to a specific location.</summary>
	/// <returns>If successful, the number of bytes sent; otherwise, an invalid <see cref="T:System.Net.Sockets.Socket" /> error.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> that stores state information and any user defined data for this asynchronous operation. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by a call to the <see cref="M:System.Net.Sockets.Socket.BeginSendTo(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint,System.AsyncCallback,System.Object)" /> method. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.Socket.EndSendTo(System.IAsyncResult)" /> was previously called for the asynchronous send. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int EndSendTo(IAsyncResult asyncResult)
	{
		ThrowIfDisposedAndClosed();
		SocketAsyncResult socketAsyncResult = ValidateEndIAsyncResult(asyncResult, "EndSendTo", "result");
		if (!socketAsyncResult.IsCompleted)
		{
			socketAsyncResult.AsyncWaitHandle.WaitOne();
		}
		socketAsyncResult.CheckIfThrowDelayedException();
		return socketAsyncResult.Total;
	}

	private unsafe static int SendTo_internal(SafeSocketHandle safeHandle, byte* buffer, int count, SocketFlags flags, SocketAddress sa, out int error, bool blocking)
	{
		try
		{
			safeHandle.RegisterForBlockingSyscall();
			return SendTo_internal(safeHandle.DangerousGetHandle(), buffer, count, flags, sa, out error, blocking);
		}
		finally
		{
			safeHandle.UnRegisterForBlockingSyscall();
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern int SendTo_internal(IntPtr sock, byte* buffer, int count, SocketFlags flags, SocketAddress sa, out int error, bool blocking);

	/// <summary>Sends the file <paramref name="fileName" /> and buffers of data to a connected <see cref="T:System.Net.Sockets.Socket" /> object using the specified <see cref="T:System.Net.Sockets.TransmitFileOptions" /> value.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that contains the path and name of the file to be sent. This parameter can be null. </param>
	/// <param name="preBuffer">A <see cref="T:System.Byte" /> array that contains data to be sent before the file is sent. This parameter can be null. </param>
	/// <param name="postBuffer">A <see cref="T:System.Byte" /> array that contains data to be sent after the file is sent. This parameter can be null. </param>
	/// <param name="flags">One or more of <see cref="T:System.Net.Sockets.TransmitFileOptions" /> values. </param>
	/// <exception cref="T:System.NotSupportedException">The operating system is not Windows NT or later.- or - The socket is not connected to a remote host. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> object has been closed. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.Socket" /> object is not in blocking mode and cannot accept this synchronous call. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file <paramref name="fileName" /> was not found. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	public void SendFile(string fileName, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags)
	{
		ThrowIfDisposedAndClosed();
		if (!is_connected)
		{
			throw new NotSupportedException();
		}
		if (!is_blocking)
		{
			throw new InvalidOperationException();
		}
		int error = 0;
		if (!SendFile_internal(m_Handle, fileName, preBuffer, postBuffer, flags, out error, is_blocking) || error != 0)
		{
			SocketException ex = new SocketException(error);
			if (ex.ErrorCode == 2 || ex.ErrorCode == 3)
			{
				throw new FileNotFoundException();
			}
			throw ex;
		}
	}

	/// <summary>Sends a file and buffers of data asynchronously to a connected <see cref="T:System.Net.Sockets.Socket" /> object.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that represents the asynchronous operation.</returns>
	/// <param name="fileName">A string that contains the path and name of the file to be sent. This parameter can be null. </param>
	/// <param name="preBuffer">A <see cref="T:System.Byte" /> array that contains data to be sent before the file is sent. This parameter can be null. </param>
	/// <param name="postBuffer">A <see cref="T:System.Byte" /> array that contains data to be sent after the file is sent. This parameter can be null. </param>
	/// <param name="flags">A bitwise combination of <see cref="T:System.Net.Sockets.TransmitFileOptions" /> values. </param>
	/// <param name="callback">An <see cref="T:System.AsyncCallback" /> delegate to be invoked when this operation completes. This parameter can be null. </param>
	/// <param name="state">A user-defined object that contains state information for this request. This parameter can be null. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> object has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See remarks section below. </exception>
	/// <exception cref="T:System.NotSupportedException">The operating system is not Windows NT or later.- or - The socket is not connected to a remote host. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file <paramref name="fileName" /> was not found. </exception>
	public IAsyncResult BeginSendFile(string fileName, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags, AsyncCallback callback, object state)
	{
		ThrowIfDisposedAndClosed();
		if (!is_connected)
		{
			throw new NotSupportedException();
		}
		if (!File.Exists(fileName))
		{
			throw new FileNotFoundException();
		}
		SendFileHandler handler = SendFile;
		return new SendFileAsyncResult(handler, handler.BeginInvoke(fileName, preBuffer, postBuffer, flags, delegate(IAsyncResult ar)
		{
			callback(new SendFileAsyncResult(handler, ar));
		}, state));
	}

	/// <summary>Ends a pending asynchronous send of a file.</summary>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> object that stores state information for this asynchronous operation. </param>
	/// <exception cref="T:System.NotSupportedException">Windows NT is required for this method. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> object has been closed. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is empty. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by a call to the <see cref="M:System.Net.Sockets.Socket.BeginSendFile(System.String,System.AsyncCallback,System.Object)" /> method. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.Socket.EndSendFile(System.IAsyncResult)" /> was previously called for the asynchronous <see cref="M:System.Net.Sockets.Socket.BeginSendFile(System.String,System.AsyncCallback,System.Object)" />. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See remarks section below. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void EndSendFile(IAsyncResult asyncResult)
	{
		ThrowIfDisposedAndClosed();
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is SendFileAsyncResult sendFileAsyncResult))
		{
			throw new ArgumentException("Invalid IAsyncResult", "asyncResult");
		}
		sendFileAsyncResult.Delegate.EndInvoke(sendFileAsyncResult.Original);
	}

	private static bool SendFile_internal(SafeSocketHandle safeHandle, string filename, byte[] pre_buffer, byte[] post_buffer, TransmitFileOptions flags, out int error, bool blocking)
	{
		try
		{
			safeHandle.RegisterForBlockingSyscall();
			return SendFile_internal(safeHandle.DangerousGetHandle(), filename, pre_buffer, post_buffer, flags, out error, blocking);
		}
		finally
		{
			safeHandle.UnRegisterForBlockingSyscall();
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool SendFile_internal(IntPtr sock, string filename, byte[] pre_buffer, byte[] post_buffer, TransmitFileOptions flags, out int error, bool blocking);

	/// <summary>Sends a collection of files or in memory data buffers asynchronously to a connected <see cref="T:System.Net.Sockets.Socket" /> object.</summary>
	/// <returns>Returns true if the I/O operation is pending. The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will be raised upon completion of the operation. Returns false if the I/O operation completed synchronously. In this case, The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will not be raised and the <paramref name="e" /> object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.</returns>
	/// <param name="e">The <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object to use for this asynchronous socket operation.</param>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in the <see cref="P:System.Net.Sockets.SendPacketsElement.FilePath" /> property was not found. </exception>
	/// <exception cref="T:System.InvalidOperationException">A socket operation was already in progress using the <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object specified in the <paramref name="e" /> parameter.</exception>
	/// <exception cref="T:System.NotSupportedException">Windows XP or later is required for this method. This exception also occurs if the <see cref="T:System.Net.Sockets.Socket" /> is not connected to a remote host. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">A connectionless <see cref="T:System.Net.Sockets.Socket" /> is being used and the file being sent exceeds the maximum packet size of the underlying transport.</exception>
	[System.MonoTODO("Not implemented")]
	public bool SendPacketsAsync(SocketAsyncEventArgs e)
	{
		ThrowIfDisposedAndClosed();
		throw new NotImplementedException();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool Duplicate_internal(IntPtr handle, int targetProcessId, out IntPtr duplicateHandle, out MonoIOError error);

	/// <summary>Duplicates the socket reference for the target process, and closes the socket for this process.</summary>
	/// <returns>The socket reference to be passed to the target process.</returns>
	/// <param name="targetProcessId">The ID of the target process where a duplicate of the socket reference is created.</param>
	/// <exception cref="T:System.Net.Sockets.SocketException">
	///   <paramref name="targetProcessID" /> is not a valid process id.-or- Duplication of the socket reference failed. </exception>
	[System.MonoLimitation("We do not support passing sockets across processes, we merely allow this API to pass the socket across AppDomains")]
	public SocketInformation DuplicateAndClose(int targetProcessId)
	{
		SocketInformation result = default(SocketInformation);
		result.Options = (SocketInformationOptions)((int)((uint)((is_listening ? 4 : 0) | (is_connected ? 2 : 0)) | ((!is_blocking) ? 1u : 0u)) | (useOverlappedIO ? 8 : 0));
		if (!Duplicate_internal(Handle, targetProcessId, out var duplicateHandle, out var error))
		{
			throw MonoIO.GetException(error);
		}
		result.ProtocolInformation = DataConverter.Pack("iiiil", (int)addressFamily, (int)socketType, (int)protocolType, is_bound ? 1 : 0, (long)duplicateHandle);
		m_Handle = null;
		return result;
	}

	/// <summary>Returns the specified <see cref="T:System.Net.Sockets.Socket" /> option setting, represented as a byte array.</summary>
	/// <param name="optionLevel">One of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> values. </param>
	/// <param name="optionName">One of the <see cref="T:System.Net.Sockets.SocketOptionName" /> values. </param>
	/// <param name="optionValue">An array of type <see cref="T:System.Byte" /> that is to receive the option setting. </param>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. - or -In .NET Compact Framework applications, the Windows CE default buffer space is set to 32768 bytes. You can change the per socket buffer space by calling <see cref="Overload:System.Net.Sockets.Socket.SetSocketOption" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
	{
		ThrowIfDisposedAndClosed();
		if (optionValue == null)
		{
			throw new SocketException(10014, "Error trying to dereference an invalid pointer");
		}
		GetSocketOption_arr_internal(m_Handle, optionLevel, optionName, ref optionValue, out var error);
		if (error != 0)
		{
			throw new SocketException(error);
		}
	}

	/// <summary>Returns the value of the specified <see cref="T:System.Net.Sockets.Socket" /> option in an array.</summary>
	/// <returns>An array of type <see cref="T:System.Byte" /> that contains the value of the socket option.</returns>
	/// <param name="optionLevel">One of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> values. </param>
	/// <param name="optionName">One of the <see cref="T:System.Net.Sockets.SocketOptionName" /> values. </param>
	/// <param name="optionLength">The length, in bytes, of the expected return value. </param>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. - or -In .NET Compact Framework applications, the Windows CE default buffer space is set to 32768 bytes. You can change the per socket buffer space by calling <see cref="Overload:System.Net.Sockets.Socket.SetSocketOption" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionLength)
	{
		ThrowIfDisposedAndClosed();
		byte[] byte_val = new byte[optionLength];
		GetSocketOption_arr_internal(m_Handle, optionLevel, optionName, ref byte_val, out var error);
		if (error != 0)
		{
			throw new SocketException(error);
		}
		return byte_val;
	}

	/// <summary>Returns the value of a specified <see cref="T:System.Net.Sockets.Socket" /> option, represented as an object.</summary>
	/// <returns>An object that represents the value of the option. When the <paramref name="optionName" /> parameter is set to <see cref="F:System.Net.Sockets.SocketOptionName.Linger" /> the return value is an instance of the <see cref="T:System.Net.Sockets.LingerOption" /> class. When <paramref name="optionName" /> is set to <see cref="F:System.Net.Sockets.SocketOptionName.AddMembership" /> or <see cref="F:System.Net.Sockets.SocketOptionName.DropMembership" />, the return value is an instance of the <see cref="T:System.Net.Sockets.MulticastOption" /> class. When <paramref name="optionName" /> is any other value, the return value is an integer.</returns>
	/// <param name="optionLevel">One of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> values. </param>
	/// <param name="optionName">One of the <see cref="T:System.Net.Sockets.SocketOptionName" /> values. </param>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information.-or-<paramref name="optionName" /> was set to the unsupported value <see cref="F:System.Net.Sockets.SocketOptionName.MaxConnections" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public object GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName)
	{
		ThrowIfDisposedAndClosed();
		GetSocketOption_obj_internal(m_Handle, optionLevel, optionName, out var obj_val, out var error);
		if (error != 0)
		{
			throw new SocketException(error);
		}
		switch (optionName)
		{
		case SocketOptionName.Linger:
			return (LingerOption)obj_val;
		case SocketOptionName.AddMembership:
		case SocketOptionName.DropMembership:
			return (MulticastOption)obj_val;
		default:
			if (obj_val is int)
			{
				return (int)obj_val;
			}
			return obj_val;
		}
	}

	private static void GetSocketOption_arr_internal(SafeSocketHandle safeHandle, SocketOptionLevel level, SocketOptionName name, ref byte[] byte_val, out int error)
	{
		bool success = false;
		try
		{
			safeHandle.DangerousAddRef(ref success);
			GetSocketOption_arr_internal(safeHandle.DangerousGetHandle(), level, name, ref byte_val, out error);
		}
		finally
		{
			if (success)
			{
				safeHandle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void GetSocketOption_arr_internal(IntPtr socket, SocketOptionLevel level, SocketOptionName name, ref byte[] byte_val, out int error);

	private static void GetSocketOption_obj_internal(SafeSocketHandle safeHandle, SocketOptionLevel level, SocketOptionName name, out object obj_val, out int error)
	{
		bool success = false;
		try
		{
			safeHandle.DangerousAddRef(ref success);
			GetSocketOption_obj_internal(safeHandle.DangerousGetHandle(), level, name, out obj_val, out error);
		}
		finally
		{
			if (success)
			{
				safeHandle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void GetSocketOption_obj_internal(IntPtr socket, SocketOptionLevel level, SocketOptionName name, out object obj_val, out int error);

	/// <summary>Sets the specified <see cref="T:System.Net.Sockets.Socket" /> option to the specified value, represented as a byte array.</summary>
	/// <param name="optionLevel">One of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> values. </param>
	/// <param name="optionName">One of the <see cref="T:System.Net.Sockets.SocketOptionName" /> values. </param>
	/// <param name="optionValue">An array of type <see cref="T:System.Byte" /> that represents the value of the option. </param>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
	{
		ThrowIfDisposedAndClosed();
		if (optionValue == null)
		{
			throw new SocketException(10014, "Error trying to dereference an invalid pointer");
		}
		SetSocketOption_internal(m_Handle, optionLevel, optionName, null, optionValue, 0, out var error);
		switch (error)
		{
		case 10022:
			throw new ArgumentException();
		default:
			throw new SocketException(error);
		case 0:
			break;
		}
	}

	/// <summary>Sets the specified <see cref="T:System.Net.Sockets.Socket" /> option to the specified value, represented as an object.</summary>
	/// <param name="optionLevel">One of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> values. </param>
	/// <param name="optionName">One of the <see cref="T:System.Net.Sockets.SocketOptionName" /> values. </param>
	/// <param name="optionValue">A <see cref="T:System.Net.Sockets.LingerOption" /> or <see cref="T:System.Net.Sockets.MulticastOption" /> that contains the value of the option. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="optionValue" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
	{
		ThrowIfDisposedAndClosed();
		if (optionValue == null)
		{
			throw new ArgumentNullException("optionValue");
		}
		int error;
		if (optionLevel == SocketOptionLevel.Socket && optionName == SocketOptionName.Linger)
		{
			if (!(optionValue is LingerOption obj_val))
			{
				throw new ArgumentException("A 'LingerOption' value must be specified.", "optionValue");
			}
			SetSocketOption_internal(m_Handle, optionLevel, optionName, obj_val, null, 0, out error);
		}
		else if (optionLevel == SocketOptionLevel.IP && (optionName == SocketOptionName.AddMembership || optionName == SocketOptionName.DropMembership))
		{
			if (!(optionValue is MulticastOption obj_val2))
			{
				throw new ArgumentException("A 'MulticastOption' value must be specified.", "optionValue");
			}
			SetSocketOption_internal(m_Handle, optionLevel, optionName, obj_val2, null, 0, out error);
		}
		else
		{
			if (optionLevel != SocketOptionLevel.IPv6 || (optionName != SocketOptionName.AddMembership && optionName != SocketOptionName.DropMembership))
			{
				throw new ArgumentException("Invalid value specified.", "optionValue");
			}
			if (!(optionValue is IPv6MulticastOption obj_val3))
			{
				throw new ArgumentException("A 'IPv6MulticastOption' value must be specified.", "optionValue");
			}
			SetSocketOption_internal(m_Handle, optionLevel, optionName, obj_val3, null, 0, out error);
		}
		switch (error)
		{
		case 10022:
			throw new ArgumentException();
		default:
			throw new SocketException(error);
		case 0:
			break;
		}
	}

	/// <summary>Sets the specified <see cref="T:System.Net.Sockets.Socket" /> option to the specified <see cref="T:System.Boolean" /> value.</summary>
	/// <param name="optionLevel">One of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> values. </param>
	/// <param name="optionName">One of the <see cref="T:System.Net.Sockets.SocketOptionName" /> values. </param>
	/// <param name="optionValue">The value of the option, represented as a <see cref="T:System.Boolean" />. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> object has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue)
	{
		int optionValue2 = (optionValue ? 1 : 0);
		SetSocketOption(optionLevel, optionName, optionValue2);
	}

	/// <summary>Sets the specified <see cref="T:System.Net.Sockets.Socket" /> option to the specified integer value.</summary>
	/// <param name="optionLevel">One of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> values. </param>
	/// <param name="optionName">One of the <see cref="T:System.Net.Sockets.SocketOptionName" /> values. </param>
	/// <param name="optionValue">A value of the option. </param>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
	{
		ThrowIfDisposedAndClosed();
		SetSocketOption_internal(m_Handle, optionLevel, optionName, null, null, optionValue, out var error);
		switch (error)
		{
		case 10022:
			throw new ArgumentException();
		default:
			throw new SocketException(error);
		case 0:
			break;
		}
	}

	private static void SetSocketOption_internal(SafeSocketHandle safeHandle, SocketOptionLevel level, SocketOptionName name, object obj_val, byte[] byte_val, int int_val, out int error)
	{
		bool success = false;
		try
		{
			safeHandle.DangerousAddRef(ref success);
			SetSocketOption_internal(safeHandle.DangerousGetHandle(), level, name, obj_val, byte_val, int_val, out error);
		}
		finally
		{
			if (success)
			{
				safeHandle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void SetSocketOption_internal(IntPtr socket, SocketOptionLevel level, SocketOptionName name, object obj_val, byte[] byte_val, int int_val, out int error);

	/// <summary>Sets low-level operating modes for the <see cref="T:System.Net.Sockets.Socket" /> using numerical control codes.</summary>
	/// <returns>The number of bytes in the <paramref name="optionOutValue" /> parameter.</returns>
	/// <param name="ioControlCode">An <see cref="T:System.Int32" /> value that specifies the control code of the operation to perform. </param>
	/// <param name="optionInValue">A <see cref="T:System.Byte" /> array that contains the input data required by the operation. </param>
	/// <param name="optionOutValue">A <see cref="T:System.Byte" /> array that contains the output data returned by the operation. </param>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to change the blocking mode without using the <see cref="P:System.Net.Sockets.Socket.Blocking" /> property. </exception>
	/// <exception cref="T:System.Security.SecurityException">A caller in the call stack does not have the required permissions. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int IOControl(int ioControlCode, byte[] optionInValue, byte[] optionOutValue)
	{
		if (CleanedUp)
		{
			throw new ObjectDisposedException(GetType().ToString());
		}
		int error;
		int num = IOControl_internal(m_Handle, ioControlCode, optionInValue, optionOutValue, out error);
		if (error != 0)
		{
			throw new SocketException(error);
		}
		if (num == -1)
		{
			throw new InvalidOperationException("Must use Blocking property instead.");
		}
		return num;
	}

	private static int IOControl_internal(SafeSocketHandle safeHandle, int ioctl_code, byte[] input, byte[] output, out int error)
	{
		bool success = false;
		try
		{
			safeHandle.DangerousAddRef(ref success);
			return IOControl_internal(safeHandle.DangerousGetHandle(), ioctl_code, input, output, out error);
		}
		finally
		{
			if (success)
			{
				safeHandle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int IOControl_internal(IntPtr sock, int ioctl_code, byte[] input, byte[] output, out int error);

	/// <summary>Closes the <see cref="T:System.Net.Sockets.Socket" /> connection and releases all associated resources.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Close()
	{
		linger_timeout = 0;
		Dispose();
	}

	/// <summary>Closes the <see cref="T:System.Net.Sockets.Socket" /> connection and releases all associated resources with a specified timeout to allow queued data to be sent. </summary>
	/// <param name="timeout">Wait up to <paramref name="timeout" /> seconds to send any remaining data, then close the socket.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Close(int timeout)
	{
		linger_timeout = timeout;
		Dispose();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern void Close_internal(IntPtr socket, out int error);

	/// <summary>Disables sends and receives on a <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <param name="how">One of the <see cref="T:System.Net.Sockets.SocketShutdown" /> values that specifies the operation that will no longer be allowed. </param>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Shutdown(SocketShutdown how)
	{
		ThrowIfDisposedAndClosed();
		if (!is_connected)
		{
			throw new SocketException(10057);
		}
		Shutdown_internal(m_Handle, how, out var error);
		if (error != 0)
		{
			throw new SocketException(error);
		}
	}

	private static void Shutdown_internal(SafeSocketHandle safeHandle, SocketShutdown how, out int error)
	{
		bool success = false;
		try
		{
			safeHandle.DangerousAddRef(ref success);
			Shutdown_internal(safeHandle.DangerousGetHandle(), how, out error);
		}
		finally
		{
			if (success)
			{
				safeHandle.DangerousRelease();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern void Shutdown_internal(IntPtr socket, SocketShutdown how, out int error);

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.Sockets.Socket" />, and optionally disposes of the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (CleanedUp)
		{
			return;
		}
		m_IntCleanedUp = 1;
		bool flag = is_connected;
		is_connected = false;
		if (m_Handle != null)
		{
			is_closed = true;
			IntPtr handle = Handle;
			if (flag)
			{
				Linger(handle);
			}
			m_Handle.Dispose();
		}
	}

	private void Linger(IntPtr handle)
	{
		if (!is_connected || linger_timeout <= 0)
		{
			return;
		}
		Shutdown_internal(handle, SocketShutdown.Receive, out var error);
		if (error != 0)
		{
			return;
		}
		int num = linger_timeout / 1000;
		int num2 = linger_timeout % 1000;
		if (num2 > 0)
		{
			Poll_internal(handle, SelectMode.SelectRead, num2 * 1000, out error);
			if (error != 0)
			{
				return;
			}
		}
		if (num > 0)
		{
			LingerOption obj_val = new LingerOption(enable: true, num);
			SetSocketOption_internal(handle, SocketOptionLevel.Socket, SocketOptionName.Linger, obj_val, null, 0, out error);
		}
	}

	private void ThrowIfDisposedAndClosed(Socket socket)
	{
		if (socket.CleanedUp && socket.is_closed)
		{
			throw new ObjectDisposedException(socket.GetType().ToString());
		}
	}

	private void ThrowIfDisposedAndClosed()
	{
		if (CleanedUp && is_closed)
		{
			throw new ObjectDisposedException(GetType().ToString());
		}
	}

	private void ThrowIfBufferNull(byte[] buffer)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
	}

	private void ThrowIfBufferOutOfRange(byte[] buffer, int offset, int size)
	{
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "offset must be >= 0");
		}
		if (offset > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offset", "offset must be <= buffer.Length");
		}
		if (size < 0)
		{
			throw new ArgumentOutOfRangeException("size", "size must be >= 0");
		}
		if (size > buffer.Length - offset)
		{
			throw new ArgumentOutOfRangeException("size", "size must be <= buffer.Length - offset");
		}
	}

	private void ThrowIfUdp()
	{
		if (protocolType == ProtocolType.Udp)
		{
			throw new SocketException(10042);
		}
	}

	private SocketAsyncResult ValidateEndIAsyncResult(IAsyncResult ares, string methodName, string argName)
	{
		if (ares == null)
		{
			throw new ArgumentNullException(argName);
		}
		SocketAsyncResult obj = (ares as SocketAsyncResult) ?? throw new ArgumentException("Invalid IAsyncResult", argName);
		if (Interlocked.CompareExchange(ref obj.EndCalled, 1, 0) == 1)
		{
			throw new InvalidOperationException(methodName + " can only be called once per asynchronous operation");
		}
		return obj;
	}

	private void QueueIOSelectorJob(SemaphoreSlim sem, IntPtr handle, IOSelectorJob job)
	{
		Task task = sem.WaitAsync();
		if (task.IsCompleted)
		{
			if (CleanedUp)
			{
				job.MarkDisposed();
			}
			else
			{
				IOSelector.Add(handle, job);
			}
			return;
		}
		task.ContinueWith(delegate
		{
			if (CleanedUp)
			{
				job.MarkDisposed();
			}
			else
			{
				IOSelector.Add(handle, job);
			}
		});
	}

	private void InitSocketAsyncEventArgs(SocketAsyncEventArgs e, AsyncCallback callback, object state, SocketOperation operation)
	{
		e.socket_async_result.Init(this, callback, state, operation);
		if (e.AcceptSocket != null)
		{
			e.socket_async_result.AcceptSocket = e.AcceptSocket;
		}
		e.current_socket = this;
		e.SetLastOperation(SocketOperationToSocketAsyncOperation(operation));
		e.SocketError = SocketError.Success;
		e.BytesTransferred = 0;
	}

	private SocketAsyncOperation SocketOperationToSocketAsyncOperation(SocketOperation op)
	{
		switch (op)
		{
		case SocketOperation.Connect:
			return SocketAsyncOperation.Connect;
		case SocketOperation.Accept:
			return SocketAsyncOperation.Accept;
		case SocketOperation.Disconnect:
			return SocketAsyncOperation.Disconnect;
		case SocketOperation.Receive:
		case SocketOperation.ReceiveGeneric:
			return SocketAsyncOperation.Receive;
		case SocketOperation.ReceiveFrom:
			return SocketAsyncOperation.ReceiveFrom;
		case SocketOperation.Send:
		case SocketOperation.SendGeneric:
			return SocketAsyncOperation.Send;
		case SocketOperation.SendTo:
			return SocketAsyncOperation.SendTo;
		default:
			throw new NotImplementedException($"Operation {op} is not implemented");
		}
	}

	private IPEndPoint RemapIPEndPoint(IPEndPoint input)
	{
		if (IsDualMode && input.AddressFamily == AddressFamily.InterNetwork)
		{
			return new IPEndPoint(input.Address.MapToIPv6(), input.Port);
		}
		return input;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern void cancel_blocking_socket_operation(Thread thread);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool SupportsPortReuse(ProtocolType proto);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool IsProtocolSupported_internal(NetworkInterfaceComponent networkInterface);

	private static bool IsProtocolSupported(NetworkInterfaceComponent networkInterface)
	{
		return IsProtocolSupported_internal(networkInterface);
	}
}
