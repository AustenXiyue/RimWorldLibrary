using System.Runtime.InteropServices;

namespace System.Net;

[StructLayout(LayoutKind.Sequential)]
internal class SecChannelBindings
{
	internal int dwInitiatorAddrType;

	internal int cbInitiatorLength;

	internal int dwInitiatorOffset;

	internal int dwAcceptorAddrType;

	internal int cbAcceptorLength;

	internal int dwAcceptorOffset;

	internal int cbApplicationDataLength;

	internal int dwApplicationDataOffset;
}
