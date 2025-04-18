namespace System.Net.Sockets;

/// <summary>Specifies the protocols that the <see cref="T:System.Net.Sockets.Socket" /> class supports.</summary>
public enum ProtocolType
{
	/// <summary>Internet Protocol.</summary>
	IP = 0,
	/// <summary>IPv6 Hop by Hop Options header.</summary>
	IPv6HopByHopOptions = 0,
	/// <summary>Internet Control Message Protocol.</summary>
	Icmp = 1,
	/// <summary>Internet Group Management Protocol.</summary>
	Igmp = 2,
	/// <summary>Gateway To Gateway Protocol.</summary>
	Ggp = 3,
	/// <summary>Internet Protocol version 4.</summary>
	IPv4 = 4,
	/// <summary>Transmission Control Protocol.</summary>
	Tcp = 6,
	/// <summary>PARC Universal Packet Protocol.</summary>
	Pup = 12,
	/// <summary>User Datagram Protocol.</summary>
	Udp = 17,
	/// <summary>Internet Datagram Protocol.</summary>
	Idp = 22,
	/// <summary>Internet Protocol version 6 (IPv6). </summary>
	IPv6 = 41,
	/// <summary>IPv6 Routing header.</summary>
	IPv6RoutingHeader = 43,
	/// <summary>IPv6 Fragment header.</summary>
	IPv6FragmentHeader = 44,
	/// <summary>IPv6 Encapsulating Security Payload header.</summary>
	IPSecEncapsulatingSecurityPayload = 50,
	/// <summary>IPv6 Authentication header. For details, see RFC 2292 section 2.2.1, available at http://www.ietf.org.</summary>
	IPSecAuthenticationHeader = 51,
	/// <summary>Internet Control Message Protocol for IPv6.</summary>
	IcmpV6 = 58,
	/// <summary>IPv6 No next header.</summary>
	IPv6NoNextHeader = 59,
	/// <summary>IPv6 Destination Options header.</summary>
	IPv6DestinationOptions = 60,
	/// <summary>Net Disk Protocol (unofficial).</summary>
	ND = 77,
	/// <summary>Raw IP packet protocol.</summary>
	Raw = 255,
	/// <summary>Unspecified protocol.</summary>
	Unspecified = 0,
	/// <summary>Internet Packet Exchange Protocol.</summary>
	Ipx = 1000,
	/// <summary>Sequenced Packet Exchange protocol.</summary>
	Spx = 1256,
	/// <summary>Sequenced Packet Exchange version 2 protocol.</summary>
	SpxII = 1257,
	/// <summary>Unknown protocol.</summary>
	Unknown = -1
}
