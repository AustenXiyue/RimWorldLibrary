namespace System.Windows.Controls;

/// <summary>Specifies the editing mode for the <see cref="T:System.Windows.Controls.InkCanvas" /></summary>
public enum InkCanvasEditingMode
{
	/// <summary>Indicates that no action is taken when the pen sends data to the <see cref="T:System.Windows.Controls.InkCanvas" />.</summary>
	None,
	/// <summary>Indicates that ink appears on the <see cref="T:System.Windows.Controls.InkCanvas" /> when the pen sends data to it.</summary>
	Ink,
	/// <summary>Indicates that the <see cref="T:System.Windows.Controls.InkCanvas" /> responds to gestures, and does not receive ink.</summary>
	GestureOnly,
	/// <summary>Indicates that the <see cref="T:System.Windows.Controls.InkCanvas" /> responds to gestures, and receives ink.</summary>
	InkAndGesture,
	/// <summary>Indicates that the pen selects strokes and elements on the <see cref="T:System.Windows.Controls.InkCanvas" />. </summary>
	Select,
	/// <summary>Indicates that the pen erases part of a stroke when the pen intersects the stroke.</summary>
	EraseByPoint,
	/// <summary>Indicates that the pen erases an entire stroke when the pen intersects the stroke.</summary>
	EraseByStroke
}
