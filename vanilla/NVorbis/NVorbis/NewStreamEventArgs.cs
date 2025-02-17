using System;

namespace NVorbis;

[Serializable]
public class NewStreamEventArgs : EventArgs
{
	public IPacketProvider PacketProvider { get; private set; }

	public bool IgnoreStream { get; set; }

	public NewStreamEventArgs(IPacketProvider packetProvider)
	{
		if (packetProvider == null)
		{
			throw new ArgumentNullException("packetProvider");
		}
		PacketProvider = packetProvider;
	}
}
