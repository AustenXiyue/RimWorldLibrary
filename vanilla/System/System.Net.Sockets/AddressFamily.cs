namespace System.Net.Sockets;

/// <summary>Specifies the addressing scheme that an instance of the <see cref="T:System.Net.Sockets.Socket" /> class can use.</summary>
public enum AddressFamily
{
	/// <summary>Unknown address family.</summary>
	Unknown = -1,
	/// <summary>Unspecified address family.</summary>
	Unspecified = 0,
	/// <summary>Unix local to host address.</summary>
	Unix = 1,
	/// <summary>Address for IP version 4.</summary>
	InterNetwork = 2,
	/// <summary>ARPANET IMP address.</summary>
	ImpLink = 3,
	/// <summary>Address for PUP protocols.</summary>
	Pup = 4,
	/// <summary>Address for MIT CHAOS protocols.</summary>
	Chaos = 5,
	/// <summary>Address for Xerox NS protocols.</summary>
	NS = 6,
	/// <summary>IPX or SPX address.</summary>
	Ipx = 6,
	/// <summary>Address for ISO protocols.</summary>
	Iso = 7,
	/// <summary>Address for OSI protocols.</summary>
	Osi = 7,
	/// <summary>European Computer Manufacturers Association (ECMA) address.</summary>
	Ecma = 8,
	/// <summary>Address for Datakit protocols.</summary>
	DataKit = 9,
	/// <summary>Addresses for CCITT protocols, such as X.25.</summary>
	Ccitt = 10,
	/// <summary>IBM SNA address.</summary>
	Sna = 11,
	/// <summary>DECnet address.</summary>
	DecNet = 12,
	/// <summary>Direct data-link interface address.</summary>
	DataLink = 13,
	/// <summary>LAT address.</summary>
	Lat = 14,
	/// <summary>NSC Hyperchannel address.</summary>
	HyperChannel = 15,
	/// <summary>AppleTalk address.</summary>
	AppleTalk = 16,
	/// <summary>NetBios address.</summary>
	NetBios = 17,
	/// <summary>VoiceView address.</summary>
	VoiceView = 18,
	/// <summary>FireFox address.</summary>
	FireFox = 19,
	/// <summary>Banyan address.</summary>
	Banyan = 21,
	/// <summary>Native ATM services address.</summary>
	Atm = 22,
	/// <summary>Address for IP version 6.</summary>
	InterNetworkV6 = 23,
	/// <summary>Address for Microsoft cluster products.</summary>
	Cluster = 24,
	/// <summary>IEEE 1284.4 workgroup address.</summary>
	Ieee12844 = 25,
	/// <summary>IrDA address.</summary>
	Irda = 26,
	/// <summary>Address for Network Designers OSI gateway-enabled protocols.</summary>
	NetworkDesigners = 28,
	/// <summary>MAX address.</summary>
	Max = 29
}
