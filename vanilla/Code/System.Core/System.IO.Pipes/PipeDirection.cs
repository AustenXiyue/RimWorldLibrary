namespace System.IO.Pipes;

/// <summary>Specifies the direction of the pipe.</summary>
[Serializable]
public enum PipeDirection
{
	/// <summary>Specifies that the pipe direction is in.</summary>
	In = 1,
	/// <summary>Specifies that the pipe direction is out.</summary>
	Out,
	/// <summary>Specifies that the pipe direction is two-way.</summary>
	InOut
}
