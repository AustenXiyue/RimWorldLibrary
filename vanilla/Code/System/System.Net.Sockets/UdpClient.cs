using System.Security.Permissions;
using System.Threading.Tasks;

namespace System.Net.Sockets;

/// <summary>Provides User Datagram Protocol (UDP) network services.</summary>
public class UdpClient : IDisposable
{
	private const int MaxUDPSize = 65536;

	private Socket m_ClientSocket;

	private bool m_Active;

	private byte[] m_Buffer = new byte[65536];

	private AddressFamily m_Family = AddressFamily.InterNetwork;

	private bool m_CleanedUp;

	private bool m_IsBroadcast;

	/// <summary>Gets or sets the underlying network <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <returns>The underlying Network <see cref="T:System.Net.Sockets.Socket" />.</returns>
	public Socket Client
	{
		get
		{
			return m_ClientSocket;
		}
		set
		{
			m_ClientSocket = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether a default remote host has been established.</summary>
	/// <returns>true if a connection is active; otherwise, false.</returns>
	protected bool Active
	{
		get
		{
			return m_Active;
		}
		set
		{
			m_Active = value;
		}
	}

	/// <summary>Gets the amount of data received from the network that is available to read.</summary>
	/// <returns>The number of bytes of data received from the network.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred while attempting to access the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public int Available => m_ClientSocket.Available;

	/// <summary>Gets or sets a value that specifies the Time to Live (TTL) value of Internet Protocol (IP) packets sent by the <see cref="T:System.Net.Sockets.UdpClient" />.</summary>
	/// <returns>The TTL value.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public short Ttl
	{
		get
		{
			return m_ClientSocket.Ttl;
		}
		set
		{
			m_ClientSocket.Ttl = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Net.Sockets.UdpClient" /> allows Internet Protocol (IP) datagrams to be fragmented.</summary>
	/// <returns>true if the <see cref="T:System.Net.Sockets.UdpClient" /> allows datagram fragmentation; otherwise, false. The default is true.</returns>
	/// <exception cref="T:System.NotSupportedException">This property can be set only for sockets that use the <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork" /> flag or the <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" /> flag. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool DontFragment
	{
		get
		{
			return m_ClientSocket.DontFragment;
		}
		set
		{
			m_ClientSocket.DontFragment = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether outgoing multicast packets are delivered to the sending application.</summary>
	/// <returns>true if the <see cref="T:System.Net.Sockets.UdpClient" /> receives outgoing multicast packets; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool MulticastLoopback
	{
		get
		{
			return m_ClientSocket.MulticastLoopback;
		}
		set
		{
			m_ClientSocket.MulticastLoopback = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Net.Sockets.UdpClient" /> may send or receive broadcast packets.</summary>
	/// <returns>true if the <see cref="T:System.Net.Sockets.UdpClient" /> allows broadcast packets; otherwise, false. The default is false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool EnableBroadcast
	{
		get
		{
			return m_ClientSocket.EnableBroadcast;
		}
		set
		{
			m_ClientSocket.EnableBroadcast = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Net.Sockets.UdpClient" /> allows only one client to use a port.</summary>
	/// <returns>true if the <see cref="T:System.Net.Sockets.UdpClient" /> allows only one client to use a specific port; otherwise, false. The default is true for Windows Server 2003 and Windows XP Service Pack 2 and later, and false for all other versions.</returns>
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
			return m_ClientSocket.ExclusiveAddressUse;
		}
		set
		{
			m_ClientSocket.ExclusiveAddressUse = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Sockets.UdpClient" /> class.</summary>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	public UdpClient()
		: this(AddressFamily.InterNetwork)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Sockets.UdpClient" /> class.</summary>
	/// <param name="family">One of the <see cref="T:System.Net.Sockets.AddressFamily" /> values that specifies the addressing scheme of the socket. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="family" /> is not <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork" /> or <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" />. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	public UdpClient(AddressFamily family)
	{
		if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6)
		{
			throw new ArgumentException(global::SR.GetString("'{0}' Client can only accept InterNetwork or InterNetworkV6 addresses.", "UDP"), "family");
		}
		m_Family = family;
		createClientSocket();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Sockets.UdpClient" /> class and binds it to the local port number provided.</summary>
	/// <param name="port">The local port number from which you intend to communicate. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="port" /> parameter is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" /> or less than <see cref="F:System.Net.IPEndPoint.MinPort" />. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	public UdpClient(int port)
		: this(port, AddressFamily.InterNetwork)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Sockets.UdpClient" /> class and binds it to the local port number provided.</summary>
	/// <param name="port">The port on which to listen for incoming connection attempts. </param>
	/// <param name="family">One of the <see cref="T:System.Net.Sockets.AddressFamily" /> values that specifies the addressing scheme of the socket. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="family" /> is not <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork" /> or <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" />. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="port" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" /> or less than <see cref="F:System.Net.IPEndPoint.MinPort" />. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	public UdpClient(int port, AddressFamily family)
	{
		if (!ValidationHelper.ValidateTcpPort(port))
		{
			throw new ArgumentOutOfRangeException("port");
		}
		if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6)
		{
			throw new ArgumentException(global::SR.GetString("'{0}' Client can only accept InterNetwork or InterNetworkV6 addresses."), "family");
		}
		m_Family = family;
		IPEndPoint localEP = ((m_Family != AddressFamily.InterNetwork) ? new IPEndPoint(IPAddress.IPv6Any, port) : new IPEndPoint(IPAddress.Any, port));
		createClientSocket();
		Client.Bind(localEP);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Sockets.UdpClient" /> class and binds it to the specified local endpoint.</summary>
	/// <param name="localEP">An <see cref="T:System.Net.IPEndPoint" /> that respresents the local endpoint to which you bind the UDP connection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="localEP" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	public UdpClient(IPEndPoint localEP)
	{
		if (localEP == null)
		{
			throw new ArgumentNullException("localEP");
		}
		m_Family = localEP.AddressFamily;
		createClientSocket();
		Client.Bind(localEP);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Sockets.UdpClient" /> class and establishes a default remote host.</summary>
	/// <param name="hostname">The name of the remote DNS host to which you intend to connect. </param>
	/// <param name="port">The remote port number to which you intend to connect. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="hostname" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="port" /> is not between <see cref="F:System.Net.IPEndPoint.MinPort" /> and <see cref="F:System.Net.IPEndPoint.MaxPort" />. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	public UdpClient(string hostname, int port)
	{
		if (hostname == null)
		{
			throw new ArgumentNullException("hostname");
		}
		if (!ValidationHelper.ValidateTcpPort(port))
		{
			throw new ArgumentOutOfRangeException("port");
		}
		Connect(hostname, port);
	}

	/// <summary>Enables or disables Network Address Translation (NAT) traversal on a <see cref="T:System.Net.Sockets.UdpClient" /> instance.</summary>
	/// <param name="allowed">A Boolean value that specifies whether to enable or disable NAT traversal.</param>
	public void AllowNatTraversal(bool allowed)
	{
		if (allowed)
		{
			m_ClientSocket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
		}
		else
		{
			m_ClientSocket.SetIPProtectionLevel(IPProtectionLevel.EdgeRestricted);
		}
	}

	/// <summary>Closes the UDP connection.</summary>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Close()
	{
		Dispose(disposing: true);
	}

	private void FreeResources()
	{
		if (!m_CleanedUp)
		{
			Socket client = Client;
			if (client != null)
			{
				client.InternalShutdown(SocketShutdown.Both);
				client.Close();
				Client = null;
			}
			m_CleanedUp = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.Sockets.UdpClient" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			FreeResources();
			GC.SuppressFinalize(this);
		}
	}

	/// <summary>Establishes a default remote host using the specified host name and port number.</summary>
	/// <param name="hostname">The DNS name of the remote host to which you intend send data. </param>
	/// <param name="port">The port number on the remote host to which you intend to send data. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.UdpClient" /> is closed. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="port" /> is not between <see cref="F:System.Net.IPEndPoint.MinPort" /> and <see cref="F:System.Net.IPEndPoint.MaxPort" />. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Connect(string hostname, int port)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (hostname == null)
		{
			throw new ArgumentNullException("hostname");
		}
		if (!ValidationHelper.ValidateTcpPort(port))
		{
			throw new ArgumentOutOfRangeException("port");
		}
		IPAddress[] hostAddresses = Dns.GetHostAddresses(hostname);
		Exception ex = null;
		Socket socket = null;
		Socket socket2 = null;
		try
		{
			if (m_ClientSocket == null)
			{
				if (Socket.OSSupportsIPv4)
				{
					socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				}
				if (Socket.OSSupportsIPv6)
				{
					socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
				}
			}
			IPAddress[] array = hostAddresses;
			foreach (IPAddress iPAddress in array)
			{
				try
				{
					if (m_ClientSocket == null)
					{
						if (iPAddress.AddressFamily == AddressFamily.InterNetwork && socket2 != null)
						{
							socket2.Connect(iPAddress, port);
							m_ClientSocket = socket2;
							socket?.Close();
						}
						else if (socket != null)
						{
							socket.Connect(iPAddress, port);
							m_ClientSocket = socket;
							socket2?.Close();
						}
						m_Family = iPAddress.AddressFamily;
						m_Active = true;
						break;
					}
					if (iPAddress.AddressFamily == m_Family)
					{
						Connect(new IPEndPoint(iPAddress, port));
						m_Active = true;
						break;
					}
				}
				catch (Exception ex2)
				{
					if (NclUtilities.IsFatal(ex2))
					{
						throw;
					}
					ex = ex2;
				}
			}
		}
		catch (Exception ex3)
		{
			if (NclUtilities.IsFatal(ex3))
			{
				throw;
			}
			ex = ex3;
		}
		finally
		{
			if (!m_Active)
			{
				socket?.Close();
				socket2?.Close();
				if (ex != null)
				{
					throw ex;
				}
				throw new SocketException(SocketError.NotConnected);
			}
		}
	}

	/// <summary>Establishes a default remote host using the specified IP address and port number.</summary>
	/// <param name="addr">The <see cref="T:System.Net.IPAddress" /> of the remote host to which you intend to send data. </param>
	/// <param name="port">The port number to which you intend send data. </param>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="T:System.Net.Sockets.UdpClient" /> is closed. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="addr" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="port" /> is not between <see cref="F:System.Net.IPEndPoint.MinPort" /> and <see cref="F:System.Net.IPEndPoint.MaxPort" />. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Connect(IPAddress addr, int port)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (addr == null)
		{
			throw new ArgumentNullException("addr");
		}
		if (!ValidationHelper.ValidateTcpPort(port))
		{
			throw new ArgumentOutOfRangeException("port");
		}
		IPEndPoint endPoint = new IPEndPoint(addr, port);
		Connect(endPoint);
	}

	/// <summary>Establishes a default remote host using the specified network endpoint.</summary>
	/// <param name="endPoint">An <see cref="T:System.Net.IPEndPoint" /> that specifies the network endpoint to which you intend to send data. </param>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="endPoint" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.UdpClient" /> is closed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Connect(IPEndPoint endPoint)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (endPoint == null)
		{
			throw new ArgumentNullException("endPoint");
		}
		CheckForBroadcast(endPoint.Address);
		Client.Connect(endPoint);
		m_Active = true;
	}

	private void CheckForBroadcast(IPAddress ipAddress)
	{
		if (Client != null && !m_IsBroadcast && ipAddress.IsBroadcast)
		{
			m_IsBroadcast = true;
			Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
		}
	}

	/// <summary>Sends a UDP datagram to the host at the specified remote endpoint.</summary>
	/// <returns>The number of bytes sent.</returns>
	/// <param name="dgram">An array of type <see cref="T:System.Byte" /> that specifies the UDP datagram that you intend to send, represented as an array of bytes. </param>
	/// <param name="bytes">The number of bytes in the datagram. </param>
	/// <param name="endPoint">An <see cref="T:System.Net.IPEndPoint" /> that represents the host and port to which to send the datagram. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dgram" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="T:System.Net.Sockets.UdpClient" /> has already established a default remote host. </exception>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="T:System.Net.Sockets.UdpClient" /> is closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int Send(byte[] dgram, int bytes, IPEndPoint endPoint)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (dgram == null)
		{
			throw new ArgumentNullException("dgram");
		}
		if (m_Active && endPoint != null)
		{
			throw new InvalidOperationException(global::SR.GetString("Cannot send packets to an arbitrary host while connected."));
		}
		if (endPoint == null)
		{
			return Client.Send(dgram, 0, bytes, SocketFlags.None);
		}
		CheckForBroadcast(endPoint.Address);
		return Client.SendTo(dgram, 0, bytes, SocketFlags.None, endPoint);
	}

	/// <summary>Sends a UDP datagram to a specified port on a specified remote host.</summary>
	/// <returns>The number of bytes sent.</returns>
	/// <param name="dgram">An array of type <see cref="T:System.Byte" /> that specifies the UDP datagram that you intend to send represented as an array of bytes. </param>
	/// <param name="bytes">The number of bytes in the datagram. </param>
	/// <param name="hostname">The name of the remote host to which you intend to send the datagram. </param>
	/// <param name="port">The remote port number with which you intend to communicate. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dgram" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.UdpClient" /> has already established a default remote host. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.UdpClient" /> is closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int Send(byte[] dgram, int bytes, string hostname, int port)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (dgram == null)
		{
			throw new ArgumentNullException("dgram");
		}
		if (m_Active && (hostname != null || port != 0))
		{
			throw new InvalidOperationException(global::SR.GetString("Cannot send packets to an arbitrary host while connected."));
		}
		if (hostname == null || port == 0)
		{
			return Client.Send(dgram, 0, bytes, SocketFlags.None);
		}
		IPAddress[] hostAddresses = Dns.GetHostAddresses(hostname);
		int i;
		for (i = 0; i < hostAddresses.Length && hostAddresses[i].AddressFamily != m_Family; i++)
		{
		}
		if (hostAddresses.Length == 0 || i == hostAddresses.Length)
		{
			throw new ArgumentException(global::SR.GetString("None of the discovered or specified addresses match the socket address family."), "hostname");
		}
		CheckForBroadcast(hostAddresses[i]);
		IPEndPoint remoteEP = new IPEndPoint(hostAddresses[i], port);
		return Client.SendTo(dgram, 0, bytes, SocketFlags.None, remoteEP);
	}

	/// <summary>Sends a UDP datagram to a remote host.</summary>
	/// <returns>The number of bytes sent.</returns>
	/// <param name="dgram">An array of type <see cref="T:System.Byte" /> that specifies the UDP datagram that you intend to send represented as an array of bytes. </param>
	/// <param name="bytes">The number of bytes in the datagram. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dgram" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.UdpClient" /> has already established a default remote host. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.UdpClient" /> is closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int Send(byte[] dgram, int bytes)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (dgram == null)
		{
			throw new ArgumentNullException("dgram");
		}
		if (!m_Active)
		{
			throw new InvalidOperationException(global::SR.GetString("The operation is not allowed on non-connected sockets."));
		}
		return Client.Send(dgram, 0, bytes, SocketFlags.None);
	}

	/// <summary>Sends a datagram to a destination asynchronously. The destination is specified by a <see cref="T:System.Net.EndPoint" />.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that references the asynchronous send.</returns>
	/// <param name="datagram">A <see cref="T:System.Byte" /> array that contains the data to be sent.</param>
	/// <param name="bytes">The number of bytes to send.</param>
	/// <param name="endPoint">The <see cref="T:System.Net.EndPoint" /> that represents the destination for the data.</param>
	/// <param name="requestCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete. </param>
	/// <param name="state">A user-defined object that contains information about the send operation. This object is passed to the <paramref name="requestCallback" /> delegate when the operation is complete.</param>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public IAsyncResult BeginSend(byte[] datagram, int bytes, IPEndPoint endPoint, AsyncCallback requestCallback, object state)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (datagram == null)
		{
			throw new ArgumentNullException("datagram");
		}
		if (bytes > datagram.Length || bytes < 0)
		{
			throw new ArgumentOutOfRangeException("bytes");
		}
		if (m_Active && endPoint != null)
		{
			throw new InvalidOperationException(global::SR.GetString("Cannot send packets to an arbitrary host while connected."));
		}
		if (endPoint == null)
		{
			return Client.BeginSend(datagram, 0, bytes, SocketFlags.None, requestCallback, state);
		}
		CheckForBroadcast(endPoint.Address);
		return Client.BeginSendTo(datagram, 0, bytes, SocketFlags.None, endPoint, requestCallback, state);
	}

	/// <summary>Sends a datagram to a destination asynchronously. The destination is specified by the host name and port number.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that references the asynchronous send.</returns>
	/// <param name="datagram">A <see cref="T:System.Byte" /> array that contains the data to be sent.</param>
	/// <param name="bytes">The number of bytes to send.</param>
	/// <param name="hostname">The destination host.</param>
	/// <param name="port">The destination port number.</param>
	/// <param name="requestCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete. </param>
	/// <param name="state">A user-defined object that contains information about the send operation. This object is passed to the <paramref name="requestCallback" /> delegate when the operation is complete.</param>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public IAsyncResult BeginSend(byte[] datagram, int bytes, string hostname, int port, AsyncCallback requestCallback, object state)
	{
		if (m_Active && (hostname != null || port != 0))
		{
			throw new InvalidOperationException(global::SR.GetString("Cannot send packets to an arbitrary host while connected."));
		}
		IPEndPoint endPoint = null;
		if (hostname != null && port != 0)
		{
			IPAddress[] hostAddresses = Dns.GetHostAddresses(hostname);
			int i;
			for (i = 0; i < hostAddresses.Length && hostAddresses[i].AddressFamily != m_Family; i++)
			{
			}
			if (hostAddresses.Length == 0 || i == hostAddresses.Length)
			{
				throw new ArgumentException(global::SR.GetString("None of the discovered or specified addresses match the socket address family."), "hostname");
			}
			CheckForBroadcast(hostAddresses[i]);
			endPoint = new IPEndPoint(hostAddresses[i], port);
		}
		return BeginSend(datagram, bytes, endPoint, requestCallback, state);
	}

	/// <summary>Sends a datagram to a remote host asynchronously. The destination was specified previously by a call to <see cref="Overload:System.Net.Sockets.UdpClient.Connect" />.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that references the asynchronous send.</returns>
	/// <param name="datagram">A <see cref="T:System.Byte" /> array that contains the data to be sent.</param>
	/// <param name="bytes">The number of bytes to send.</param>
	/// <param name="requestCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete.</param>
	/// <param name="state">A user-defined object that contains information about the send operation. This object is passed to the <paramref name="requestCallback" /> delegate when the operation is complete.</param>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public IAsyncResult BeginSend(byte[] datagram, int bytes, AsyncCallback requestCallback, object state)
	{
		return BeginSend(datagram, bytes, null, requestCallback, state);
	}

	/// <summary>Ends a pending asynchronous send.</summary>
	/// <returns>If successful, the number of bytes sent to the <see cref="T:System.Net.Sockets.UdpClient" />.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> object returned by a call to <see cref="Overload:System.Net.Sockets.UdpClient.BeginSend" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by a call to the <see cref="M:System.Net.Sockets.Socket.BeginSend(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)" /> method. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.Socket.EndSend(System.IAsyncResult)" /> was previously called for the asynchronous read. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the underlying socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	public int EndSend(IAsyncResult asyncResult)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (m_Active)
		{
			return Client.EndSend(asyncResult);
		}
		return Client.EndSendTo(asyncResult);
	}

	/// <summary>Returns a UDP datagram that was sent by a remote host.</summary>
	/// <returns>An array of type <see cref="T:System.Byte" /> that contains datagram data.</returns>
	/// <param name="remoteEP">An <see cref="T:System.Net.IPEndPoint" /> that represents the remote host from which the data was sent. </param>
	/// <exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket" />  has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public byte[] Receive(ref IPEndPoint remoteEP)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		EndPoint remoteEP2 = ((m_Family != AddressFamily.InterNetwork) ? IPEndPoint.IPv6Any : IPEndPoint.Any);
		int num = Client.ReceiveFrom(m_Buffer, 65536, SocketFlags.None, ref remoteEP2);
		remoteEP = (IPEndPoint)remoteEP2;
		if (num < 65536)
		{
			byte[] array = new byte[num];
			Buffer.BlockCopy(m_Buffer, 0, array, 0, num);
			return array;
		}
		return m_Buffer;
	}

	/// <summary>Receives a datagram from a remote host asynchronously.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that references the asynchronous receive.</returns>
	/// <param name="requestCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete. </param>
	/// <param name="state">A user-defined object that contains information about the receive operation. This object is passed to the <paramref name="requestCallback" /> delegate when the operation is complete.</param>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public IAsyncResult BeginReceive(AsyncCallback requestCallback, object state)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		EndPoint remoteEP = ((m_Family != AddressFamily.InterNetwork) ? IPEndPoint.IPv6Any : IPEndPoint.Any);
		return Client.BeginReceiveFrom(m_Buffer, 0, 65536, SocketFlags.None, ref remoteEP, requestCallback, state);
	}

	/// <summary>Ends a pending asynchronous receive.</summary>
	/// <returns>If successful, the number of bytes received. If unsuccessful, this method returns 0.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> object returned by a call to <see cref="M:System.Net.Sockets.UdpClient.BeginReceive(System.AsyncCallback,System.Object)" />.</param>
	/// <param name="remoteEP">The specified remote endpoint.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not returned by a call to the <see cref="M:System.Net.Sockets.UdpClient.BeginReceive(System.AsyncCallback,System.Object)" /> method. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Net.Sockets.UdpClient.EndReceive(System.IAsyncResult,System.Net.IPEndPoint@)" /> was previously called for the asynchronous read. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the underlying <see cref="T:System.Net.Sockets.Socket" />. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	public byte[] EndReceive(IAsyncResult asyncResult, ref IPEndPoint remoteEP)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		EndPoint endPoint = ((m_Family != AddressFamily.InterNetwork) ? IPEndPoint.IPv6Any : IPEndPoint.Any);
		int num = Client.EndReceiveFrom(asyncResult, ref endPoint);
		remoteEP = (IPEndPoint)endPoint;
		if (num < 65536)
		{
			byte[] array = new byte[num];
			Buffer.BlockCopy(m_Buffer, 0, array, 0, num);
			return array;
		}
		return m_Buffer;
	}

	/// <summary>Adds a <see cref="T:System.Net.Sockets.UdpClient" /> to a multicast group.</summary>
	/// <param name="multicastAddr">The multicast <see cref="T:System.Net.IPAddress" /> of the group you want to join. </param>
	/// <exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ArgumentException">The IP address is not compatible with the <see cref="T:System.Net.Sockets.AddressFamily" /> value that defines the addressing scheme of the socket. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void JoinMulticastGroup(IPAddress multicastAddr)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (multicastAddr == null)
		{
			throw new ArgumentNullException("multicastAddr");
		}
		if (multicastAddr.AddressFamily != m_Family)
		{
			throw new ArgumentException(global::SR.GetString("Multicast family is not the same as the family of the '{0}' Client.", "UDP"), "multicastAddr");
		}
		if (m_Family == AddressFamily.InterNetwork)
		{
			MulticastOption optionValue = new MulticastOption(multicastAddr);
			Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
		}
		else
		{
			IPv6MulticastOption optionValue2 = new IPv6MulticastOption(multicastAddr);
			Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, optionValue2);
		}
	}

	/// <summary>Adds a <see cref="T:System.Net.Sockets.UdpClient" /> to a multicast group.</summary>
	/// <param name="multicastAddr">The multicast <see cref="T:System.Net.IPAddress" /> of the group you want to join.</param>
	/// <param name="localAddress">The local <see cref="T:System.Net.IPAddress" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void JoinMulticastGroup(IPAddress multicastAddr, IPAddress localAddress)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (m_Family != AddressFamily.InterNetwork)
		{
			throw new SocketException(SocketError.OperationNotSupported);
		}
		MulticastOption optionValue = new MulticastOption(multicastAddr, localAddress);
		Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
	}

	/// <summary>Adds a <see cref="T:System.Net.Sockets.UdpClient" /> to a multicast group.</summary>
	/// <param name="ifindex">The interface index associated with the local IP address on which to join the multicast group.</param>
	/// <param name="multicastAddr">The multicast <see cref="T:System.Net.IPAddress" /> of the group you want to join. </param>
	/// <exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void JoinMulticastGroup(int ifindex, IPAddress multicastAddr)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (multicastAddr == null)
		{
			throw new ArgumentNullException("multicastAddr");
		}
		if (ifindex < 0)
		{
			throw new ArgumentException(global::SR.GetString("The specified value cannot be negative."), "ifindex");
		}
		if (m_Family != AddressFamily.InterNetworkV6)
		{
			throw new SocketException(SocketError.OperationNotSupported);
		}
		IPv6MulticastOption optionValue = new IPv6MulticastOption(multicastAddr, ifindex);
		Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, optionValue);
	}

	/// <summary>Adds a <see cref="T:System.Net.Sockets.UdpClient" /> to a multicast group with the specified Time to Live (TTL).</summary>
	/// <param name="multicastAddr">The <see cref="T:System.Net.IPAddress" /> of the multicast group to join. </param>
	/// <param name="timeToLive">The Time to Live (TTL), measured in router hops. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The TTL provided is not between 0 and 255 </exception>
	/// <exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="multicastAddr" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The IP address is not compatible with the <see cref="T:System.Net.Sockets.AddressFamily" /> value that defines the addressing scheme of the socket. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void JoinMulticastGroup(IPAddress multicastAddr, int timeToLive)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (multicastAddr == null)
		{
			throw new ArgumentNullException("multicastAddr");
		}
		if (!ValidationHelper.ValidateRange(timeToLive, 0, 255))
		{
			throw new ArgumentOutOfRangeException("timeToLive");
		}
		JoinMulticastGroup(multicastAddr);
		Client.SetSocketOption((m_Family != AddressFamily.InterNetwork) ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, timeToLive);
	}

	/// <summary>Leaves a multicast group.</summary>
	/// <param name="multicastAddr">The <see cref="T:System.Net.IPAddress" /> of the multicast group to leave. </param>
	/// <exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ArgumentException">The IP address is not compatible with the <see cref="T:System.Net.Sockets.AddressFamily" /> value that defines the addressing scheme of the socket. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="multicastAddr" /> is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void DropMulticastGroup(IPAddress multicastAddr)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (multicastAddr == null)
		{
			throw new ArgumentNullException("multicastAddr");
		}
		if (multicastAddr.AddressFamily != m_Family)
		{
			throw new ArgumentException(global::SR.GetString("Multicast family is not the same as the family of the '{0}' Client.", "UDP"), "multicastAddr");
		}
		if (m_Family == AddressFamily.InterNetwork)
		{
			MulticastOption optionValue = new MulticastOption(multicastAddr);
			Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, optionValue);
		}
		else
		{
			IPv6MulticastOption optionValue2 = new IPv6MulticastOption(multicastAddr);
			Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, optionValue2);
		}
	}

	/// <summary>Leaves a multicast group.</summary>
	/// <param name="multicastAddr">The <see cref="T:System.Net.IPAddress" /> of the multicast group to leave. </param>
	/// <param name="ifindex">The local address of the multicast group to leave.</param>
	/// <exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	/// <exception cref="T:System.ArgumentException">The IP address is not compatible with the <see cref="T:System.Net.Sockets.AddressFamily" /> value that defines the addressing scheme of the socket. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="multicastAddr" /> is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void DropMulticastGroup(IPAddress multicastAddr, int ifindex)
	{
		if (m_CleanedUp)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
		if (multicastAddr == null)
		{
			throw new ArgumentNullException("multicastAddr");
		}
		if (ifindex < 0)
		{
			throw new ArgumentException(global::SR.GetString("The specified value cannot be negative."), "ifindex");
		}
		if (m_Family != AddressFamily.InterNetworkV6)
		{
			throw new SocketException(SocketError.OperationNotSupported);
		}
		IPv6MulticastOption optionValue = new IPv6MulticastOption(multicastAddr, ifindex);
		Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, optionValue);
	}

	/// <summary>Sends a UDP datagram asynchronously to a remote host.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="datagram">An array of type <see cref="T:System.Byte" /> that specifies the UDP datagram that you intend to send represented as an array of bytes.</param>
	/// <param name="bytes">The number of bytes in the datagram.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dgram" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.UdpClient" /> has already established a default remote host. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.UdpClient" /> is closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<int> SendAsync(byte[] datagram, int bytes)
	{
		return Task<int>.Factory.FromAsync(BeginSend, EndSend, datagram, bytes, null);
	}

	/// <summary>Sends a UDP datagram asynchronously to a remote host.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="datagram">An array of type <see cref="T:System.Byte" /> that specifies the UDP datagram that you intend to send represented as an array of bytes.</param>
	/// <param name="bytes">The number of bytes in the datagram.</param>
	/// <param name="endPoint">An <see cref="T:System.Net.IPEndPoint" /> that represents the host and port to which to send the datagram.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dgram" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="T:System.Net.Sockets.UdpClient" /> has already established a default remote host. </exception>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="T:System.Net.Sockets.UdpClient" /> is closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<int> SendAsync(byte[] datagram, int bytes, IPEndPoint endPoint)
	{
		return Task<int>.Factory.FromAsync(BeginSend, EndSend, datagram, bytes, endPoint, null);
	}

	/// <summary>Sends a UDP datagram asynchronously to a remote host.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="datagram">An array of type <see cref="T:System.Byte" /> that specifies the UDP datagram that you intend to send represented as an array of bytes.</param>
	/// <param name="bytes">The number of bytes in the datagram.</param>
	/// <param name="hostname">The name of the remote host to which you intend to send the datagram.</param>
	/// <param name="port">The remote port number with which you intend to communicate.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dgram" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.UdpClient" /> has already established a default remote host. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.UdpClient" /> is closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<int> SendAsync(byte[] datagram, int bytes, string hostname, int port)
	{
		return Task<int>.Factory.FromAsync((AsyncCallback callback, object state) => BeginSend(datagram, bytes, hostname, port, callback, state), EndSend, null);
	}

	/// <summary>Returns a UDP datagram asynchronously that was sent by a remote host.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket" />  has been closed. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<UdpReceiveResult> ReceiveAsync()
	{
		return Task<UdpReceiveResult>.Factory.FromAsync((AsyncCallback callback, object state) => BeginReceive(callback, state), delegate(IAsyncResult ar)
		{
			IPEndPoint remoteEP = null;
			return new UdpReceiveResult(EndReceive(ar, ref remoteEP), remoteEP);
		}, null);
	}

	private void createClientSocket()
	{
		Client = new Socket(m_Family, SocketType.Dgram, ProtocolType.Udp);
	}
}
