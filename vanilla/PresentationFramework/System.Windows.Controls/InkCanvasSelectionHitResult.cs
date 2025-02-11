namespace System.Windows.Controls;

/// <summary>Identifies the various parts of a selection adorner on an <see cref="T:System.Windows.Controls.InkCanvas" />.</summary>
public enum InkCanvasSelectionHitResult
{
	/// <summary>No part of the selection adorner.</summary>
	None,
	/// <summary>The upper left handle of the selection adorner.</summary>
	TopLeft,
	/// <summary>The upper middle handle of the selection adorner.</summary>
	Top,
	/// <summary>The upper right handle of the selection adorner.</summary>
	TopRight,
	/// <summary>The middle handle on the right edge of the selection adorner.</summary>
	Right,
	/// <summary>The lower right handle of the selection adorner.</summary>
	BottomRight,
	/// <summary>The lower middle handle of the selection adorner.</summary>
	Bottom,
	/// <summary>The lower left handle of the selection adorner.</summary>
	BottomLeft,
	/// <summary>The middle handle on the left edge of the selection adorner.</summary>
	Left,
	/// <summary>The area within the bounds of the selection adorner.</summary>
	Selection
}
