namespace System.Windows.Controls.Primitives;

/// <summary>Specifies the position of tick marks in a <see cref="T:System.Windows.Controls.Slider" /> control with respect to the <see cref="T:System.Windows.Controls.Primitives.Track" /> that the control implements.</summary>
public enum TickPlacement
{
	/// <summary>No tick marks appear. </summary>
	None,
	/// <summary>Tick marks appear above the <see cref="T:System.Windows.Controls.Primitives.Track" /> for a horizontal <see cref="T:System.Windows.Controls.Slider" />, or to the left of the <see cref="T:System.Windows.Controls.Primitives.Track" /> for a vertical <see cref="T:System.Windows.Controls.Slider" />. </summary>
	TopLeft,
	/// <summary>Tick marks appear below the <see cref="T:System.Windows.Controls.Primitives.Track" /> for a horizontal <see cref="T:System.Windows.Controls.Slider" />, or to the right of the <see cref="T:System.Windows.Controls.Primitives.Track" /> for a vertical <see cref="T:System.Windows.Controls.Slider" />. </summary>
	BottomRight,
	/// <summary>Tick marks appear above and below the <see cref="T:System.Windows.Controls.Primitives.Track" /> bar for a horizontal <see cref="T:System.Windows.Controls.Slider" />, or to the left and right of the <see cref="T:System.Windows.Controls.Primitives.Track" /> for a vertical <see cref="T:System.Windows.Controls.Slider" />.</summary>
	Both
}
