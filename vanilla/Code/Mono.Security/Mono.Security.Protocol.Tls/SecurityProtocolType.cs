using System;

namespace Mono.Security.Protocol.Tls;

[Serializable]
[Flags]
public enum SecurityProtocolType
{
	Default = -1073741824,
	Ssl2 = 0xC,
	Ssl3 = 0x30,
	Tls = 0xC0,
	Tls11 = 0x300,
	Tls12 = 0xC00
}
