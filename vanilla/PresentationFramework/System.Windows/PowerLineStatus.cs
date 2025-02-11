namespace System.Windows;

/// <summary>Indicates whether the system power is online, or that the system power status is unknown.</summary>
public enum PowerLineStatus
{
	/// <summary>The system power is not on.</summary>
	Offline = 0,
	/// <summary>The system power is on.</summary>
	Online = 1,
	/// <summary>The status of the system power cannot be determined.</summary>
	Unknown = 255
}
