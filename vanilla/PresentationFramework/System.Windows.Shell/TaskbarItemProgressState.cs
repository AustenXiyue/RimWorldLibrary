namespace System.Windows.Shell;

/// <summary>Specifies the state of the progress indicator in the Windows taskbar.</summary>
public enum TaskbarItemProgressState
{
	/// <summary>No progress indicator is displayed in the taskbar button.</summary>
	None,
	/// <summary>A pulsing green indicator is displayed in the taskbar button.</summary>
	Indeterminate,
	/// <summary>A green progress indicator is displayed in the taskbar button.</summary>
	Normal,
	/// <summary>A red progress indicator is displayed in the taskbar button.</summary>
	Error,
	/// <summary>A yellow progress indicator is displayed in the taskbar button.</summary>
	Paused
}
