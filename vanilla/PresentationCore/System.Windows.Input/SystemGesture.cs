namespace System.Windows.Input;

/// <summary>Defines the available system gestures.</summary>
public enum SystemGesture
{
	/// <summary>No system gesture.</summary>
	None = 0,
	/// <summary>Maps to a left-click on a mouse. This can be used to choose a command from the menu or toolbar, take action if a command is chosen, set an insertion point, or show selection feedback.</summary>
	Tap = 16,
	/// <summary>Maps to a right-click on a mouse. This can be used to show a shortcut menu.</summary>
	RightTap = 18,
	/// <summary>Maps to a left drag on a mouse.</summary>
	Drag = 19,
	/// <summary>Maps to a right drag on a mouse. This can be used to drag an object or selection to a different area and is followed by the appearance of the shortcut menu which provides options for moving the object.</summary>
	RightDrag = 20,
	/// <summary>Indicates that press and hold has occurred.</summary>
	HoldEnter = 21,
	/// <summary>Not implemented.</summary>
	HoldLeave = 22,
	/// <summary>Maps to a mouse hover. This can be used to show ToolTip rollover effects, or other mouse hover behaviors.</summary>
	HoverEnter = 23,
	/// <summary>Maps to a mouse leaving a hover. This can be used to end ToolTip rollover effects or other mouse hover behaviors.</summary>
	HoverLeave = 24,
	/// <summary>Occurs with a short, quick stroke that translates into a specific command. The action taken by a flick is set system-wide. An application can listen for a <see cref="F:System.Windows.Input.SystemGesture.Flick" /> and prevent it from becoming one of the standard <see cref="T:System.Windows.Input.ApplicationCommands" /> by setting the <see cref="P:System.Windows.RoutedEventArgs.Handled" /> property to true in the <see cref="E:System.Windows.UIElement.StylusSystemGesture" /> event. Only Windows Vista supports flicks.</summary>
	Flick = 31,
	/// <summary>Maps to a double-click of a mouse. </summary>
	TwoFingerTap = 4352
}
