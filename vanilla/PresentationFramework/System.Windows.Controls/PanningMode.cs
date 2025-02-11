namespace System.Windows.Controls;

/// <summary>Specifies how <see cref="T:System.Windows.Controls.ScrollViewer" /> reacts to touch manipulation.  </summary>
public enum PanningMode
{
	/// <summary>The <see cref="T:System.Windows.Controls.ScrollViewer" /> does not respond to touch input.</summary>
	None,
	/// <summary>The <see cref="T:System.Windows.Controls.ScrollViewer" /> scrolls horizontally only.</summary>
	HorizontalOnly,
	/// <summary>The <see cref="T:System.Windows.Controls.ScrollViewer" /> scrolls vertically only.</summary>
	VerticalOnly,
	/// <summary>The <see cref="T:System.Windows.Controls.ScrollViewer" /> scrolls horizontally and vertically.</summary>
	Both,
	/// <summary>The <see cref="T:System.Windows.Controls.ScrollViewer" /> scrolls when the user moves a finger horizontally first.  If the user moves the vertically first, the movement is treated as mouse events.  After the <see cref="T:System.Windows.Controls.ScrollViewer" /> begins to scroll, it will scroll horizontally and vertically.</summary>
	HorizontalFirst,
	/// <summary>The <see cref="T:System.Windows.Controls.ScrollViewer" /> scrolls when the user moves a finger vertically first.  If the user moves the horizontally first, the movement is treated as mouse events.  After the <see cref="T:System.Windows.Controls.ScrollViewer" /> begins to scroll, it will scroll horizontally and vertically.</summary>
	VerticalFirst
}
