namespace System.Diagnostics.Eventing.Reader;

/// <summary>Specifies that a string contains a name of an event log or the file system path to an event log file.</summary>
/// <filterpriority>2</filterpriority>
public enum PathType
{
	/// <summary>A path parameter contains the file system path to an event log file.</summary>
	FilePath = 2,
	/// <summary>A path parameter contains the name of the event log.</summary>
	LogName = 1
}
