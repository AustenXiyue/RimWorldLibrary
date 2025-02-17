using System;

namespace Mono.Unix.Native;

[Map]
[CLSCompliant(false)]
public enum UnixSocketType
{
	SOCK_STREAM = 1,
	SOCK_DGRAM = 2,
	SOCK_RAW = 3,
	SOCK_RDM = 4,
	SOCK_SEQPACKET = 5,
	SOCK_DCCP = 6,
	SOCK_PACKET = 10
}
