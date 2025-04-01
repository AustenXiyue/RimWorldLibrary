using System.Security.Permissions;
using System.Threading.Tasks;

namespace System.Net.Sockets;

/// <summary>Listens for connections from TCP network clients.</summary>
public class TcpListener
{
	private IPEndPoint m_ServerSocketEP;

	private Socket m_ServerSocket;

	private bool m_Active;

	private bool m_ExclusiveAddressUse;

	/// <summary>Gets the underlying network <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>The underlying <see cref="T:System.Net.Sockets.Socket" />.</returns>
	public Socket Server => m_ServerSocket;

	/// <summary>Gets a value that indicates whether <see cref="T:System.Net.Sockets.TcpListener" /> is actively listening for client connections.</summary>
	/// <returns>true if <see cref="T:System.Net.Sockets.TcpListener" /> is actively listening; otherwise, false.</returns>
	protected bool Active => m_Active;

	/// <summary>Gets the underlying <see cref="T:System.Net.EndPoint" /> of the current <see cref="T:System.Net.Sockets.TcpListener" />.</summary>
	/// <returns>The <see cref="T:System.Net.EndPoint" /> to which the <see cref="T:System.Net.Sockets.Socket" /> is bound.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public EndPoint LocalEndpoint
	{
		get
		{
			if (!m_Active)
			{
				return m_ServerSocketEP;
			}
			return m_ServerSocket.LocalEndPoint;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Net.Sockets.TcpListener" /> allows only one underlying socket to listen to a specific port.</summary>
	/// <returns>true if the <see cref="T:System.Net.Sockets.TcpListener" /> allows only one <see cref="T:System.Net.Sockets.TcpListener" /> to listen to a specific port; otherwise, false. . The default is true for Windows Server 2003 and Windows XP Service Pack 2 and later, and false for all other versions.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.TcpListener" /> has been started. Call the <see cref="M:System.Net.Sockets.TcpListener.Stop" /> method and then set the <see cref="P:System.Net.Sockets.Socket.ExclusiveAddressUse" /> property.</exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the underlying socket.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool ExclusiveAddressUse
	{
		get
		{
			return m_ServerSocket.ExclusiveAddressUse;
		}
		set
		{
			if (m_Active)
			{
				throw new InvalidOperationException(global::SR.GetString("The TcpListener must not be listening before performing this operation."));
			}
			m_ServerSocket.ExclusiveAddressUse = value;
			m_ExclusiveAddressUse = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Sockets.TcpListener" /> class with the specified local endpoint.</summary>
	/// <param name="localEP">An <see cref="T:System.Net.IPEndPoint" /> that represents the local endpoint to which to bind the listener <see cref="T:System.Net.Sockets.Socket" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="localEP" /> is null. </exception>
	public TcpListener(IPEndPoint localEP)
	{
		_ = Logging.On;
		if (localEP == null)
		{
			throw new ArgumentNullException("localEP");
		}
		m_ServerSocketEP = localEP;
		m_ServerSocket = new Socket(m_ServerSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		_ = Logging.On;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Sockets.TcpListener" /> class that listens for incoming connection attempts on the specified local IP address and port number.</summary>
	/// <param name="localaddr">An <see cref="T:System.Net.IPAddress" /> that represents the local IP address. </param>
	/// <param name="port">The port on which to listen for incoming connection attempts. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="localaddr" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="port" /> is not between <see cref="F:System.Net.IPEndPoint.MinPort" /> and <see cref="F:System.Net.IPEndPoint.MaxPort" />. </exception>
	public TcpListener(IPAddress localaddr, int port)
	{
		_ = Logging.On;
		if (localaddr == null)
		{
			throw new ArgumentNullException("localaddr");
		}
		if (!ValidationHelper.ValidateTcpPort(port))
		{
			throw new ArgumentOutOfRangeException("port");
		}
		m_ServerSocketEP = new IPEndPoint(localaddr, port);
		m_ServerSocket = new Socket(m_ServerSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		_ = Logging.On;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Sockets.TcpListener" /> class that listens on the specified port.</summary>
	/// <param name="port">The port on which to listen for incoming connection attempts. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="port" /> is not between <see cref="F:System.Net.IPEndPoint.MinPort" /> and <see cref="F:System.Net.IPEndPoint.MaxPort" />. </exception>
	[Obsolete("This method has been deprecated. Please use TcpListener(IPAddress localaddr, int port) instead. http://go.microsoft.com/fwlink/?linkid=14202")]
	public TcpListener(int port)
	{
		if (!ValidationHelper.ValidateTcpPort(port))
		{
			throw new ArgumentOutOfRangeException("port");
		}
		m_ServerSocketEP = new IPEndPoint(IPAddress.Any, port);
		m_ServerSocket = new Socket(m_ServerSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
	}

	/// <summary>Creates a new <see cref="T:System.Net.Sockets.TcpListener" /> instance to listen on the specified port.</summary>
	/// <returns>Returns <see cref="T:System.Net.Sockets.TcpListener" />.A new <see cref="T:System.Net.Sockets.TcpListener" /> instance to listen on the specified port.</returns>
	/// <param name="port">The port on which to listen for incoming connection attempts.</param>
	public static TcpListener Create(int port)
	{
		_ = Logging.On;
		if (!ValidationHelper.ValidateTcpPort(port))
		{
			throw new ArgumentOutOfRangeException("port");
		}
		TcpListener tcpListener = new TcpListener(IPAddress.IPv6Any, port);
		tcpListener.Server.DualMode = true;
		_ = Logging.On;
		return tcpListener;
	}

	/// <summary>Enables or disables Network Address Translation (NAT) traversal on a <see cref="T:System.Net.Sockets.TcpListener" /> instance.</summary>
	/// <param name="allowed">A Boolean value that specifies whether to enable or disable NAT traversal.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="M:System.Net.Sockets.TcpListener.AllowNatTraversal(System.Boolean)" /> method was called after calling the <see cref="M:System.Net.Sockets.TcpListener.Start" /> method</exception>
	public void AllowNatTraversal(bool allowed)
	{
		if (m_Active)
		{
			throw new InvalidOperationException(global::SR.GetString("The TcpListener must not be listening before performing this operation."));
		}
		if (allowed)
		{
			m_ServerSocket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
		}
		else
		{
			m_ServerSocket.SetIPProtectionLevel(IPProtectionLevel.EdgeRestricted);
		}
	}

	/// <summary>Starts listening for incoming connection requests.</summary>
	/// <exception cref="T:System.Net.Sockets.SocketException">Use the <see cref="P:System.Net.Sockets.SocketException.ErrorCode" /> property to obtain the specific error code. When you have obtained this code, you can refer to the Windows Sockets version 2 API error code documentation in MSDN for a detailed description of the error. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Start()
	{
		Start(int.MaxValue);
	}

	/// <summary>Starts listening for incoming connection requests with a maximum number of pending connection.</summary>
	/// <param name="backlog">The maximum length of the pending connections queue.</param>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred while accessing the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The<paramref name=" backlog" /> parameter is less than zero or exceeds the maximum number of permitted connections.</exception>
	/// <exception cref="T:System.InvalidOperationException">The underlying <see cref="T:System.Net.Sockets.Socket" /> is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Start(int backlog)
	{
		if (backlog > int.MaxValue || backlog < 0)
		{
			throw new ArgumentOutOfRangeException("backlog");
		}
		_ = Logging.On;
		if (m_ServerSocket == null)
		{
			throw new InvalidOperationException(global::SR.GetString("The socket handle is not valid."));
		}
		if (m_Active)
		{
			_ = Logging.On;
			return;
		}
		m_ServerSocket.Bind(m_ServerSocketEP);
		try
		{
			m_ServerSocket.Listen(backlog);
		}
		catch (SocketException)
		{
			Stop();
			throw;
		}
		m_Active = true;
		_ = Logging.On;
	}

	/// <summary>Closes the listener.</summary>
	/// <exception cref="T:System.Net.Sockets.SocketException">Use the <see cref="P:System.Net.Sockets.SocketException.ErrorCode" /> property to obtain the specific error code. When you have obtained this code, you can refer to the Windows Sockets version 2 API error code documentation in MSDN for a detailed description of the error. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Stop()
	{
		_ = Logging.On;
		if (m_ServerSocket != null)
		{
			m_ServerSocket.Close();
			m_ServerSocket = null;
		}
		m_Active = false;
		m_ServerSocket = new Socket(m_ServerSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		if (m_ExclusiveAddressUse)
		{
			m_ServerSocket.ExclusiveAddressUse = true;
		}
		_ = Logging.On;
	}

	/// <summary>Determines if there are pending connection requests.</summary>
	/// <returns>true if connections are pending; otherwise, false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The listener has not been started with a call to <see cref="M:System.Net.Sockets.TcpListener.Start" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool Pending()
	{
		if (!m_Active)
		{
			throw new InvalidOperationException(global::SR.GetString("Not listening. You must call the Start() method before calling this method."));
		}
		return m_ServerSocket.Poll(0, SelectMode.SelectRead);
	}

	/// <summary>Accepts a pending connection request.</summary>
	/// <returns>A <see cref="T:System.Net.Sockets.Socket" /> used to send and receive data.</returns>
	/// <exception cref="T:System.InvalidOperationException">The listener has not been started with a call to <see cref="M:System.Net.Sockets.TcpListener.Start" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Socket AcceptSocket()
	{
		_ = Logging.On;
		if (!m_Active)
		{
			throw new InvalidOperationException(global::SR.GetString("Not listening. You must call the Start() method before calling this method."));
		}
		Socket result = m_ServerSocket.Accept();
		_ = Logging.On;
		return result;
	}

	/// <summary>Accepts a pending connection request.</summary>
	/// <returns>A <see cref="T:System.Net.Sockets.TcpClient" /> used to send and receive data.</returns>
	/// <exception cref="T:System.InvalidOperationException">The listener has not been started with a call to <see cref="M:System.Net.Sockets.TcpListener.Start" />. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">Use the <see cref="P:System.Net.Sockets.SocketException.ErrorCode" /> property to obtain the specific error code. When you have obtained this code, you can refer to the Windows Sockets version 2 API error code documentation in MSDN for a detailed description of the error. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public TcpClient AcceptTcpClient()
	{
		_ = Logging.On;
		if (!m_Active)
		{
			throw new InvalidOperationException(global::SR.GetString("Not listening. You must call the Start() method before calling this method."));
		}
		TcpClient result = new TcpClient(m_ServerSocket.Accept());
		_ = Logging.On;
		return result;
	}

	/// <summary>Begins an asynchronous operation to accept an incoming connection attempt.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous creation of the <see cref="T:System.Net.Sockets.Socket" />.</returns>
	/// <param name="callback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete.</param>
	/// <param name="state">A user-defined object containing information about the accept operation. This object is passed to the <paramref name="callback" /> delegate when the operation is complete.</param>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred while attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public IAsyncResult BeginAcceptSocket(AsyncCallback callback, object state)
	{
		_ = Logging.On;
		if (!m_Active)
		{
			throw new InvalidOperationException(global::SR.GetString("Not listening. You must call the Start() method before calling this method."));
		}
		IAsyncResult result = m_ServerSocket.BeginAccept(callback, state);
		_ = Logging.On;
		return result;
	}

	/// <summary>Asynchronously accepts an incoming connection attempt and creates a new <see cref="T:System.Net.Sockets.Socket" /> to handle remote host communication.</summary>
	/// <returns>A <see cref="T:System.Net.Sockets.Socket" />.The <see cref="T:System.Net.Sockets.Socket" /> used to send and receive data.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> returned by a call to the <see cref="M:System.Net.Sockets.TcpListener.BeginAcceptSocket(System.AsyncCallback,System.Object)" />  method.</param>
	/// <exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="asyncResult" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="asyncResult" /> parameter was not created by a call to the <see cref="M:System.Net.Sockets.TcpListener.BeginAcceptSocket(System.AsyncCallback,System.Object)" /> method. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="M:System.Net.Sockets.TcpListener.EndAcceptSocket(System.IAsyncResult)" /> method was previously called. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred while attempting to access the <see cref="T:System.Net.Sockets.Socket" />. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Socket EndAcceptSocket(IAsyncResult asyncResult)
	{
		_ = Logging.On;
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		Socket result = (((!(asyncResult is SocketAsyncResult socketAsyncResult)) ? null : socketAsyncResult.socket) ?? throw new ArgumentException(global::SR.GetString("The IAsyncResult object was not returned from the corresponding asynchronous method on this class."), "asyncResult")).EndAccept(asyncResult);
		_ = Logging.On;
		return result;
	}

	/// <summary>Begins an asynchronous operation to accept an incoming connection attempt.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous creation of the <see cref="T:System.Net.Sockets.TcpClient" />.</returns>
	/// <param name="callback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete.</param>
	/// <param name="state">A user-defined object containing information about the accept operation. This object is passed to the <paramref name="callback" /> delegate when the operation is complete.</param>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred while attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public IAsyncResult BeginAcceptTcpClient(AsyncCallback callback, object state)
	{
		_ = Logging.On;
		if (!m_Active)
		{
			throw new InvalidOperationException(global::SR.GetString("Not listening. You must call the Start() method before calling this method."));
		}
		IAsyncResult result = m_ServerSocket.BeginAccept(callback, state);
		_ = Logging.On;
		return result;
	}

	/// <summary>Asynchronously accepts an incoming connection attempt and creates a new <see cref="T:System.Net.Sockets.TcpClient" /> to handle remote host communication.</summary>
	/// <returns>A <see cref="T:System.Net.Sockets.TcpClient" />.The <see cref="T:System.Net.Sockets.TcpClient" /> used to send and receive data.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> returned by a call to the <see cref="M:System.Net.Sockets.TcpListener.BeginAcceptTcpClient(System.AsyncCallback,System.Object)" /> method.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public TcpClient EndAcceptTcpClient(IAsyncResult asyncResult)
	{
		_ = Logging.On;
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		Socket acceptedSocket = (((!(asyncResult is SocketAsyncResult socketAsyncResult)) ? null : socketAsyncResult.socket) ?? throw new ArgumentException(global::SR.GetString("The IAsyncResult object was not returned from the corresponding asynchronous method on this class."), "asyncResult")).EndAccept(asyncResult);
		_ = Logging.On;
		return new TcpClient(acceptedSocket);
	}

	/// <summary>Accepts a pending connection request as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Net.Sockets.Socket" /> used to send and receive data.</returns>
	/// <exception cref="T:System.InvalidOperationException">The listener has not been started with a call to <see cref="M:System.Net.Sockets.TcpListener.Start" />. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<Socket> AcceptSocketAsync()
	{
		return Task<Socket>.Factory.FromAsync(BeginAcceptSocket, EndAcceptSocket, null);
	}

	/// <summary>Accepts a pending connection request as an asynchronous operation. </summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Net.Sockets.TcpClient" /> used to send and receive data.</returns>
	/// <exception cref="T:System.InvalidOperationException">The listener has not been started with a call to <see cref="M:System.Net.Sockets.TcpListener.Start" />. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">Use the <see cref="P:System.Net.Sockets.SocketException.ErrorCode" /> property to obtain the specific error code. When you have obtained this code, you can refer to the Windows Sockets version 2 API error code documentation in MSDN for a detailed description of the error. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<TcpClient> AcceptTcpClientAsync()
	{
		return Task<TcpClient>.Factory.FromAsync(BeginAcceptTcpClient, EndAcceptTcpClient, null);
	}
}
