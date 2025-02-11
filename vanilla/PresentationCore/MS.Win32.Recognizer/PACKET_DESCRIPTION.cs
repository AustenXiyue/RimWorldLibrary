namespace MS.Win32.Recognizer;

internal struct PACKET_DESCRIPTION
{
	public uint cbPacketSize;

	public uint cPacketProperties;

	public nint pPacketProperties;

	public uint cButtons;

	public nint pguidButtons;
}
