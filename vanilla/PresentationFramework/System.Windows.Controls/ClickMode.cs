namespace System.Windows.Controls;

/// <summary>Specifies when the <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> event should be raised. </summary>
public enum ClickMode
{
	/// <summary>Specifies that the <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> event should be raised when a button is pressed and released.</summary>
	Release,
	/// <summary>Specifies that the <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> event should be raised as soon as a button is pressed.</summary>
	Press,
	/// <summary>Specifies that the <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> event should be raised when the mouse hovers over a control.</summary>
	Hover
}
