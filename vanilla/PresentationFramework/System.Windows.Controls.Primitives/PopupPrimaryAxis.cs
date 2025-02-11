namespace System.Windows.Controls.Primitives;

/// <summary>Describes the direction to move a <see cref="T:System.Windows.Controls.Primitives.Popup" /> control to increase the amount of the <see cref="T:System.Windows.Controls.Primitives.Popup" /> that is visible.</summary>
public enum PopupPrimaryAxis
{
	/// <summary>A <see cref="T:System.Windows.Controls.Primitives.Popup" /> control changes position according to default <see cref="T:System.Windows.Controls.Primitives.Popup" /> behavior. </summary>
	None,
	/// <summary>A <see cref="T:System.Windows.Controls.Primitives.Popup" /> control changes position by moving along the horizontal axis of the screen before moving along the vertical axis. </summary>
	Horizontal,
	/// <summary>A <see cref="T:System.Windows.Controls.Primitives.Popup" /> control changes position by moving along the vertical axis of the screen before moving along the horizontal axis.</summary>
	Vertical
}
