namespace System.IO.Pipes;

/// <summary>Specifies the transmission mode of the pipe.</summary>
[Serializable]
public enum PipeTransmissionMode
{
	/// <summary>Indicates that data in the pipe is transmitted and read as a stream of bytes.</summary>
	Byte,
	/// <summary>Indicates that data in the pipe is transmitted and read as a stream of messages.</summary>
	Message
}
