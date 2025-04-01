using System;

namespace NVorbis;

[Serializable]
public class ParameterChangeEventArgs : EventArgs
{
	public DataPacket FirstPacket { get; private set; }

	public ParameterChangeEventArgs(DataPacket firstPacket)
	{
		FirstPacket = firstPacket;
	}
}
