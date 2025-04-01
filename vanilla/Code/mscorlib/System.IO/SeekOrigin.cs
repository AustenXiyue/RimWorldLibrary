using System.Runtime.InteropServices;

namespace System.IO;

/// <summary>Specifies the position in a stream to use for seeking.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public enum SeekOrigin
{
	/// <summary>Specifies the beginning of a stream.</summary>
	Begin,
	/// <summary>Specifies the current position within a stream.</summary>
	Current,
	/// <summary>Specifies the end of a stream.</summary>
	End
}
