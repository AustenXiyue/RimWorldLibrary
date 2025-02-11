namespace System.Windows;

/// <summary>Specifies special display options for a message box.</summary>
[Flags]
public enum MessageBoxOptions
{
	/// <summary>No options are set.</summary>
	None = 0,
	/// <summary>The message box is displayed on the currently active desktop even if a user is not logged on to the computer. Specifies that the message box is displayed from a Microsoft .NET Framework windows service application in order to notify the user of an event. </summary>
	ServiceNotification = 0x200000,
	/// <summary>The message box is displayed on the default desktop of the interactive window station. Specifies that the message box is displayed from a Microsoft .NET Framework windows service application in order to notify the user of an event. </summary>
	DefaultDesktopOnly = 0x20000,
	/// <summary>The message box text and title bar caption are right-aligned.</summary>
	RightAlign = 0x80000,
	/// <summary>All text, buttons, icons, and title bars are displayed right-to-left.</summary>
	RtlReading = 0x100000
}
