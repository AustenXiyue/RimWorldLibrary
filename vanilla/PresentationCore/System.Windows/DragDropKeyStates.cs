namespace System.Windows;

/// <summary>Specifies the current state of the modifier keys (SHIFT, CTRL, and ALT), as well as the state of the mouse buttons.</summary>
[Flags]
public enum DragDropKeyStates
{
	/// <summary>No modifier keys or mouse buttons are pressed.</summary>
	None = 0,
	/// <summary>The left mouse button is pressed.</summary>
	LeftMouseButton = 1,
	/// <summary>The right mouse button is pressed.</summary>
	RightMouseButton = 2,
	/// <summary>The shift (SHIFT) key is pressed.</summary>
	ShiftKey = 4,
	/// <summary>The control (CTRL) key is pressed.</summary>
	ControlKey = 8,
	/// <summary>The middle mouse button is pressed.</summary>
	MiddleMouseButton = 0x10,
	/// <summary>The ALT key is pressed.</summary>
	AltKey = 0x20
}
