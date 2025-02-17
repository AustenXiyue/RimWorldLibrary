using System;

namespace Mono.Unix.Native;

[Map]
[CLSCompliant(false)]
public enum UnixAddressFamily
{
	AF_UNSPEC = 0,
	AF_UNIX = 1,
	AF_INET = 2,
	AF_AX25 = 3,
	AF_IPX = 4,
	AF_APPLETALK = 5,
	AF_NETROM = 6,
	AF_BRIDGE = 7,
	AF_ATMPVC = 8,
	AF_X25 = 9,
	AF_INET6 = 10,
	AF_ROSE = 11,
	AF_DECnet = 12,
	AF_NETBEUI = 13,
	AF_SECURITY = 14,
	AF_KEY = 15,
	AF_NETLINK = 16,
	AF_PACKET = 17,
	AF_ASH = 18,
	AF_ECONET = 19,
	AF_ATMSVC = 20,
	AF_RDS = 21,
	AF_SNA = 22,
	AF_IRDA = 23,
	AF_PPPOX = 24,
	AF_WANPIPE = 25,
	AF_LLC = 26,
	AF_CAN = 29,
	AF_TIPC = 30,
	AF_BLUETOOTH = 31,
	AF_IUCV = 32,
	AF_RXRPC = 33,
	AF_ISDN = 34,
	AF_PHONET = 35,
	AF_IEEE802154 = 36,
	AF_CAIF = 37,
	AF_ALG = 38,
	AF_NFC = 39,
	AF_VSOCK = 40,
	Unknown = 65536
}
