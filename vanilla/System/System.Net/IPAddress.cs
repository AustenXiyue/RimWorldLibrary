using System.Globalization;
using System.Net.Sockets;

namespace System.Net;

/// <summary>Provides an Internet Protocol (IP) address.</summary>
[Serializable]
public class IPAddress
{
	/// <summary>Provides an IP address that indicates that the server must listen for client activity on all network interfaces. This field is read-only.</summary>
	public static readonly IPAddress Any = new IPAddress(0);

	/// <summary>Provides the IP loopback address. This field is read-only.</summary>
	public static readonly IPAddress Loopback = new IPAddress(16777343);

	/// <summary>Provides the IP broadcast address. This field is read-only.</summary>
	public static readonly IPAddress Broadcast = new IPAddress(4294967295L);

	/// <summary>Provides an IP address that indicates that no network interface should be used. This field is read-only.</summary>
	public static readonly IPAddress None = Broadcast;

	internal const long LoopbackMask = 255L;

	internal long m_Address;

	[NonSerialized]
	internal string m_ToString;

	/// <summary>The <see cref="M:System.Net.Sockets.Socket.Bind(System.Net.EndPoint)" /> method uses the <see cref="F:System.Net.IPAddress.IPv6Any" /> field to indicate that a <see cref="T:System.Net.Sockets.Socket" /> must listen for client activity on all network interfaces.</summary>
	public static readonly IPAddress IPv6Any = new IPAddress(new byte[16], 0L);

	/// <summary>Provides the IP loopback address. This property is read-only.</summary>
	public static readonly IPAddress IPv6Loopback = new IPAddress(new byte[16]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 1
	}, 0L);

	/// <summary>Provides an IP address that indicates that no network interface should be used. This property is read-only.</summary>
	public static readonly IPAddress IPv6None = new IPAddress(new byte[16], 0L);

	private AddressFamily m_Family = AddressFamily.InterNetwork;

	private ushort[] m_Numbers = new ushort[8];

	private long m_ScopeId;

	private int m_HashCode;

	internal const int IPv4AddressBytes = 4;

	internal const int IPv6AddressBytes = 16;

	internal const int NumberOfLabels = 8;

	/// <summary>An Internet Protocol (IP) address.</summary>
	/// <returns>The long value of the IP address.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">The address family is <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" />. </exception>
	[Obsolete("This property has been deprecated. It is address family dependent. Please use IPAddress.Equals method to perform comparisons. http://go.microsoft.com/fwlink/?linkid=14202")]
	public long Address
	{
		get
		{
			if (m_Family == AddressFamily.InterNetworkV6)
			{
				throw new SocketException(SocketError.OperationNotSupported);
			}
			return m_Address;
		}
		set
		{
			if (m_Family == AddressFamily.InterNetworkV6)
			{
				throw new SocketException(SocketError.OperationNotSupported);
			}
			if (m_Address != value)
			{
				m_ToString = null;
				m_Address = value;
			}
		}
	}

	/// <summary>Gets the address family of the IP address.</summary>
	/// <returns>Returns <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork" /> for IPv4 or <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" /> for IPv6.</returns>
	public AddressFamily AddressFamily => m_Family;

	/// <summary>Gets or sets the IPv6 address scope identifier.</summary>
	/// <returns>A long integer that specifies the scope of the address.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">AddressFamily = InterNetwork. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="scopeId" /> &lt; 0- or -<paramref name="scopeId" /> &gt; 0x00000000FFFFFFFF  </exception>
	public long ScopeId
	{
		get
		{
			if (m_Family == AddressFamily.InterNetwork)
			{
				throw new SocketException(SocketError.OperationNotSupported);
			}
			return m_ScopeId;
		}
		set
		{
			if (m_Family == AddressFamily.InterNetwork)
			{
				throw new SocketException(SocketError.OperationNotSupported);
			}
			if (value < 0 || value > uint.MaxValue)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (m_ScopeId != value)
			{
				m_Address = value;
				m_ScopeId = value;
			}
		}
	}

	internal bool IsBroadcast
	{
		get
		{
			if (m_Family == AddressFamily.InterNetworkV6)
			{
				return false;
			}
			return m_Address == Broadcast.m_Address;
		}
	}

	/// <summary>Gets whether the address is an IPv6 multicast global address.</summary>
	/// <returns>true if the IP address is an IPv6 multicast global address; otherwise, false.</returns>
	public bool IsIPv6Multicast
	{
		get
		{
			if (m_Family == AddressFamily.InterNetworkV6)
			{
				return (m_Numbers[0] & 0xFF00) == 65280;
			}
			return false;
		}
	}

	/// <summary>Gets whether the address is an IPv6 link local address.</summary>
	/// <returns>true if the IP address is an IPv6 link local address; otherwise, false.</returns>
	public bool IsIPv6LinkLocal
	{
		get
		{
			if (m_Family == AddressFamily.InterNetworkV6)
			{
				return (m_Numbers[0] & 0xFFC0) == 65152;
			}
			return false;
		}
	}

	/// <summary>Gets whether the address is an IPv6 site local address.</summary>
	/// <returns>true if the IP address is an IPv6 site local address; otherwise, false.</returns>
	public bool IsIPv6SiteLocal
	{
		get
		{
			if (m_Family == AddressFamily.InterNetworkV6)
			{
				return (m_Numbers[0] & 0xFFC0) == 65216;
			}
			return false;
		}
	}

	/// <summary>Gets whether the address is an IPv6 Teredo address.</summary>
	/// <returns>true if the IP address is an IPv6 Teredo address; otherwise, false.</returns>
	public bool IsIPv6Teredo
	{
		get
		{
			if (m_Family == AddressFamily.InterNetworkV6 && m_Numbers[0] == 8193)
			{
				return m_Numbers[1] == 0;
			}
			return false;
		}
	}

	/// <summary>Gets whether the IP address is an IPv4-mapped IPv6 address.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the IP address is an IPv4-mapped IPv6 address; otherwise, false.</returns>
	public bool IsIPv4MappedToIPv6
	{
		get
		{
			if (AddressFamily != AddressFamily.InterNetworkV6)
			{
				return false;
			}
			for (int i = 0; i < 5; i++)
			{
				if (m_Numbers[i] != 0)
				{
					return false;
				}
			}
			return m_Numbers[5] == ushort.MaxValue;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.IPAddress" /> class with the address specified as an <see cref="T:System.Int64" />.</summary>
	/// <param name="newAddress">The long value of the IP address. For example, the value 0x2414188f in big-endian format would be the IP address "143.24.20.36". </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="newAddress" /> &lt; 0 or <paramref name="newAddress" /> &gt; 0x00000000FFFFFFFF </exception>
	public IPAddress(long newAddress)
	{
		if (newAddress < 0 || newAddress > uint.MaxValue)
		{
			throw new ArgumentOutOfRangeException("newAddress");
		}
		m_Address = newAddress;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.IPAddress" /> class with the address specified as a <see cref="T:System.Byte" /> array and the specified scope identifier.</summary>
	/// <param name="address">The byte array value of the IP address. </param>
	/// <param name="scopeid">The long value of the scope identifier. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="address" /> contains a bad IP address. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="scopeid" /> &lt; 0 or <paramref name="scopeid" /> &gt; 0x00000000FFFFFFFF </exception>
	public IPAddress(byte[] address, long scopeid)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (address.Length != 16)
		{
			throw new ArgumentException(global::SR.GetString("An invalid IP address was specified."), "address");
		}
		m_Family = AddressFamily.InterNetworkV6;
		for (int i = 0; i < 8; i++)
		{
			m_Numbers[i] = (ushort)(address[i * 2] * 256 + address[i * 2 + 1]);
		}
		if (scopeid < 0 || scopeid > uint.MaxValue)
		{
			throw new ArgumentOutOfRangeException("scopeid");
		}
		m_ScopeId = scopeid;
	}

	private IPAddress(ushort[] address, uint scopeid)
	{
		m_Family = AddressFamily.InterNetworkV6;
		m_Numbers = address;
		m_ScopeId = scopeid;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.IPAddress" /> class with the address specified as a <see cref="T:System.Byte" /> array.</summary>
	/// <param name="address">The byte array value of the IP address. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="address" /> contains a bad IP address. </exception>
	public IPAddress(byte[] address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (address.Length != 4 && address.Length != 16)
		{
			throw new ArgumentException(global::SR.GetString("An invalid IP address was specified."), "address");
		}
		if (address.Length == 4)
		{
			m_Family = AddressFamily.InterNetwork;
			m_Address = ((address[3] << 24) | (address[2] << 16) | (address[1] << 8) | address[0]) & 0xFFFFFFFFu;
			return;
		}
		m_Family = AddressFamily.InterNetworkV6;
		for (int i = 0; i < 8; i++)
		{
			m_Numbers[i] = (ushort)(address[i * 2] * 256 + address[i * 2 + 1]);
		}
	}

	internal IPAddress(int newAddress)
	{
		m_Address = newAddress & 0xFFFFFFFFu;
	}

	/// <summary>Determines whether a string is a valid IP address.</summary>
	/// <returns>true if <paramref name="ipString" /> is a valid IP address; otherwise, false.</returns>
	/// <param name="ipString">The string to validate.</param>
	/// <param name="address">The <see cref="T:System.Net.IPAddress" /> version of the string.</param>
	public static bool TryParse(string ipString, out IPAddress address)
	{
		address = InternalParse(ipString, tryParse: true);
		return address != null;
	}

	/// <summary>Converts an IP address string to an <see cref="T:System.Net.IPAddress" /> instance.</summary>
	/// <returns>An <see cref="T:System.Net.IPAddress" /> instance.</returns>
	/// <param name="ipString">A string that contains an IP address in dotted-quad notation for IPv4 and in colon-hexadecimal notation for IPv6. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="ipString" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="ipString" /> is not a valid IP address. </exception>
	public static IPAddress Parse(string ipString)
	{
		return InternalParse(ipString, tryParse: false);
	}

	private unsafe static IPAddress InternalParse(string ipString, bool tryParse)
	{
		if (ipString == null)
		{
			if (tryParse)
			{
				return null;
			}
			throw new ArgumentNullException("ipString");
		}
		if (ipString.IndexOf(':') != -1)
		{
			SocketException ex = null;
			int start = 0;
			if (ipString[0] != '[')
			{
				ipString += "]";
			}
			else
			{
				start = 1;
			}
			int end = ipString.Length;
			fixed (char* name = ipString)
			{
				if (IPv6AddressHelper.IsValidStrict(name, start, ref end) || end != ipString.Length)
				{
					ushort[] array = new ushort[8];
					string scopeId = null;
					fixed (ushort* numbers = array)
					{
						IPv6AddressHelper.Parse(ipString, numbers, 0, ref scopeId);
					}
					if (scopeId == null || scopeId.Length == 0)
					{
						return new IPAddress(array, 0u);
					}
					scopeId = scopeId.Substring(1);
					if (uint.TryParse(scopeId, NumberStyles.None, null, out var result))
					{
						return new IPAddress(array, result);
					}
					return new IPAddress(array, 0u);
				}
			}
			if (tryParse)
			{
				return null;
			}
			ex = new SocketException(SocketError.InvalidArgument);
			throw new FormatException(global::SR.GetString("An invalid IP address was specified."), ex);
		}
		int end2 = ipString.Length;
		long num;
		fixed (char* name2 = ipString)
		{
			num = IPv4AddressHelper.ParseNonCanonical(name2, 0, ref end2, notImplicitFile: true);
		}
		if (num == -1 || end2 != ipString.Length)
		{
			if (tryParse)
			{
				return null;
			}
			throw new FormatException(global::SR.GetString("An invalid IP address was specified."));
		}
		num = ((num & 0xFF) << 24) | (((num & 0xFF00) << 8) | (((num & 0xFF0000) >> 8) | ((num & 0xFF000000u) >> 24)));
		return new IPAddress(num);
	}

	/// <summary>Provides a copy of the <see cref="T:System.Net.IPAddress" /> as an array of bytes.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array.</returns>
	public byte[] GetAddressBytes()
	{
		byte[] array;
		if (m_Family != AddressFamily.InterNetworkV6)
		{
			array = new byte[4]
			{
				(byte)m_Address,
				(byte)(m_Address >> 8),
				(byte)(m_Address >> 16),
				(byte)(m_Address >> 24)
			};
		}
		else
		{
			array = new byte[16];
			int num = 0;
			for (int i = 0; i < 8; i++)
			{
				array[num++] = (byte)((m_Numbers[i] >> 8) & 0xFF);
				array[num++] = (byte)(m_Numbers[i] & 0xFF);
			}
		}
		return array;
	}

	/// <summary>Converts an Internet address to its standard notation.</summary>
	/// <returns>A string that contains the IP address in either IPv4 dotted-quad or in IPv6 colon-hexadecimal notation.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">The address family is <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" /> and the address is bad. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public unsafe override string ToString()
	{
		if (m_ToString == null)
		{
			if (m_Family == AddressFamily.InterNetworkV6)
			{
				m_ToString = new IPv6AddressFormatter(m_Numbers, ScopeId).ToString();
			}
			else
			{
				int num = 15;
				char* ptr = stackalloc char[15];
				int num2 = (int)((m_Address >> 24) & 0xFF);
				do
				{
					ptr[--num] = (char)(48 + num2 % 10);
					num2 /= 10;
				}
				while (num2 > 0);
				ptr[--num] = '.';
				num2 = (int)((m_Address >> 16) & 0xFF);
				do
				{
					ptr[--num] = (char)(48 + num2 % 10);
					num2 /= 10;
				}
				while (num2 > 0);
				ptr[--num] = '.';
				num2 = (int)((m_Address >> 8) & 0xFF);
				do
				{
					ptr[--num] = (char)(48 + num2 % 10);
					num2 /= 10;
				}
				while (num2 > 0);
				ptr[--num] = '.';
				num2 = (int)(m_Address & 0xFF);
				do
				{
					ptr[--num] = (char)(48 + num2 % 10);
					num2 /= 10;
				}
				while (num2 > 0);
				m_ToString = new string(ptr, num, 15 - num);
			}
		}
		return m_ToString;
	}

	/// <summary>Converts a long value from host byte order to network byte order.</summary>
	/// <returns>A long value, expressed in network byte order.</returns>
	/// <param name="host">The number to convert, expressed in host byte order. </param>
	public static long HostToNetworkOrder(long host)
	{
		return ((HostToNetworkOrder((int)host) & 0xFFFFFFFFu) << 32) | (HostToNetworkOrder((int)(host >> 32)) & 0xFFFFFFFFu);
	}

	/// <summary>Converts an integer value from host byte order to network byte order.</summary>
	/// <returns>An integer value, expressed in network byte order.</returns>
	/// <param name="host">The number to convert, expressed in host byte order. </param>
	public static int HostToNetworkOrder(int host)
	{
		return ((HostToNetworkOrder((short)host) & 0xFFFF) << 16) | (HostToNetworkOrder((short)(host >> 16)) & 0xFFFF);
	}

	/// <summary>Converts a short value from host byte order to network byte order.</summary>
	/// <returns>A short value, expressed in network byte order.</returns>
	/// <param name="host">The number to convert, expressed in host byte order. </param>
	public static short HostToNetworkOrder(short host)
	{
		return (short)(((host & 0xFF) << 8) | ((host >> 8) & 0xFF));
	}

	/// <summary>Converts a long value from network byte order to host byte order.</summary>
	/// <returns>A long value, expressed in host byte order.</returns>
	/// <param name="network">The number to convert, expressed in network byte order. </param>
	public static long NetworkToHostOrder(long network)
	{
		return HostToNetworkOrder(network);
	}

	/// <summary>Converts an integer value from network byte order to host byte order.</summary>
	/// <returns>An integer value, expressed in host byte order.</returns>
	/// <param name="network">The number to convert, expressed in network byte order. </param>
	public static int NetworkToHostOrder(int network)
	{
		return HostToNetworkOrder(network);
	}

	/// <summary>Converts a short value from network byte order to host byte order.</summary>
	/// <returns>A short value, expressed in host byte order.</returns>
	/// <param name="network">The number to convert, expressed in network byte order. </param>
	public static short NetworkToHostOrder(short network)
	{
		return HostToNetworkOrder(network);
	}

	/// <summary>Indicates whether the specified IP address is the loopback address.</summary>
	/// <returns>true if <paramref name="address" /> is the loopback address; otherwise, false.</returns>
	/// <param name="address">An IP address. </param>
	public static bool IsLoopback(IPAddress address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (address.m_Family == AddressFamily.InterNetworkV6)
		{
			return address.Equals(IPv6Loopback);
		}
		return (address.m_Address & 0xFF) == (Loopback.m_Address & 0xFF);
	}

	internal bool Equals(object comparandObj, bool compareScopeId)
	{
		if (!(comparandObj is IPAddress iPAddress))
		{
			return false;
		}
		if (m_Family != iPAddress.m_Family)
		{
			return false;
		}
		if (m_Family == AddressFamily.InterNetworkV6)
		{
			for (int i = 0; i < 8; i++)
			{
				if (iPAddress.m_Numbers[i] != m_Numbers[i])
				{
					return false;
				}
			}
			if (iPAddress.m_ScopeId == m_ScopeId)
			{
				return true;
			}
			if (!compareScopeId)
			{
				return true;
			}
			return false;
		}
		return iPAddress.m_Address == m_Address;
	}

	/// <summary>Compares two IP addresses.</summary>
	/// <returns>true if the two addresses are equal; otherwise, false.</returns>
	/// <param name="comparand">An <see cref="T:System.Net.IPAddress" /> instance to compare to the current instance. </param>
	public override bool Equals(object comparand)
	{
		return Equals(comparand, compareScopeId: true);
	}

	/// <summary>Returns a hash value for an IP address.</summary>
	/// <returns>An integer hash value.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override int GetHashCode()
	{
		if (m_Family == AddressFamily.InterNetworkV6)
		{
			if (m_HashCode == 0)
			{
				m_HashCode = StringComparer.InvariantCultureIgnoreCase.GetHashCode(ToString());
			}
			return m_HashCode;
		}
		return (int)m_Address;
	}

	internal IPAddress Snapshot()
	{
		return m_Family switch
		{
			AddressFamily.InterNetwork => new IPAddress(m_Address), 
			AddressFamily.InterNetworkV6 => new IPAddress(m_Numbers, (uint)m_ScopeId), 
			_ => throw new InternalException(), 
		};
	}

	/// <summary>Maps the <see cref="T:System.Net.IPAddress" /> object to an IPv6 address.</summary>
	/// <returns>Returns <see cref="T:System.Net.IPAddress" />.An IPv6 address.</returns>
	public IPAddress MapToIPv6()
	{
		if (AddressFamily == AddressFamily.InterNetworkV6)
		{
			return this;
		}
		return new IPAddress(new ushort[8]
		{
			0,
			0,
			0,
			0,
			0,
			65535,
			(ushort)(((m_Address & 0xFF00) >> 8) | ((m_Address & 0xFF) << 8)),
			(ushort)(((m_Address & 0xFF000000u) >> 24) | ((m_Address & 0xFF0000) >> 8))
		}, 0u);
	}

	/// <summary>Maps the <see cref="T:System.Net.IPAddress" /> object to an IPv4 address.</summary>
	/// <returns>Returns <see cref="T:System.Net.IPAddress" />.An IPv4 address.</returns>
	public IPAddress MapToIPv4()
	{
		if (AddressFamily == AddressFamily.InterNetwork)
		{
			return this;
		}
		return new IPAddress((uint)(((m_Numbers[6] & 0xFF00) >>> 8) | ((m_Numbers[6] & 0xFF) << 8) | ((((m_Numbers[7] & 0xFF00) >>> 8) | ((m_Numbers[7] & 0xFF) << 8)) << 16)));
	}
}
