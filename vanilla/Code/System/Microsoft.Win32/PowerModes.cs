namespace Microsoft.Win32;

/// <summary>Defines identifiers for power mode events reported by the operating system.</summary>
public enum PowerModes
{
	/// <summary>The operating system is about to resume from a suspended state.</summary>
	Resume = 1,
	/// <summary>A power mode status notification event has been raised by the operating system. This might indicate a weak or charging battery, a transition between AC power and battery, or another change in the status of the system power supply.</summary>
	StatusChange,
	/// <summary>The operating system is about to be suspended.</summary>
	Suspend
}
