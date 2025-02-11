namespace System.Net.Sockets;

/// <summary>Specifies the type of protocol that an instance of the <see cref="T:System.Net.Sockets.Socket" /> class can use.</summary>
public enum ProtocolFamily
{
	/// <summary>Unknown protocol.</summary>
	Unknown = -1,
	/// <summary>Unspecified protocol.</summary>
	Unspecified = 0,
	/// <summary>Unix local to host protocol.</summary>
	Unix = 1,
	/// <summary>IP version 4 protocol.</summary>
	InterNetwork = 2,
	/// <summary>ARPANET IMP protocol.</summary>
	ImpLink = 3,
	/// <summary>PUP protocol.</summary>
	Pup = 4,
	/// <summary>MIT CHAOS protocol.</summary>
	Chaos = 5,
	/// <summary>Xerox NS protocol.</summary>
	NS = 6,
	/// <summary>IPX or SPX protocol.</summary>
	Ipx = 6,
	/// <summary>ISO protocol.</summary>
	Iso = 7,
	/// <summary>OSI protocol.</summary>
	Osi = 7,
	/// <summary>European Computer Manufacturers Association (ECMA) protocol.</summary>
	Ecma = 8,
	/// <summary>DataKit protocol.</summary>
	DataKit = 9,
	/// <summary>CCITT protocol, such as X.25.</summary>
	Ccitt = 10,
	/// <summary>IBM SNA protocol.</summary>
	Sna = 11,
	/// <summary>DECNet protocol.</summary>
	DecNet = 12,
	/// <summary>Direct data link protocol.</summary>
	DataLink = 13,
	/// <summary>LAT protocol.</summary>
	Lat = 14,
	/// <summary>NSC HyperChannel protocol.</summary>
	HyperChannel = 15,
	/// <summary>AppleTalk protocol.</summary>
	AppleTalk = 16,
	/// <summary>NetBIOS protocol.</summary>
	NetBios = 17,
	/// <summary>VoiceView protocol.</summary>
	VoiceView = 18,
	/// <summary>FireFox protocol.</summary>
	FireFox = 19,
	/// <summary>Banyan protocol.</summary>
	Banyan = 21,
	/// <summary>Native ATM services protocol.</summary>
	Atm = 22,
	/// <summary>IP version 6 protocol.</summary>
	InterNetworkV6 = 23,
	/// <summary>Microsoft Cluster products protocol.</summary>
	Cluster = 24,
	/// <summary>IEEE 1284.4 workgroup protocol.</summary>
	Ieee12844 = 25,
	/// <summary>IrDA protocol.</summary>
	Irda = 26,
	/// <summary>Network Designers OSI gateway enabled protocol.</summary>
	NetworkDesigners = 28,
	/// <summary>MAX protocol.</summary>
	Max = 29
}
