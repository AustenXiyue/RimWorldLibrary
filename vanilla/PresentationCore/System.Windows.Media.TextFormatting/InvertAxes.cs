namespace System.Windows.Media.TextFormatting;

/// <summary>Indicate the inversion of horizontal and vertical axes of the drawing surface.</summary>
[Flags]
public enum InvertAxes
{
	/// <summary>Drawing surface is not inverted in either axis.</summary>
	None = 0,
	/// <summary>Drawing surface is inverted in the horizontal axis.</summary>
	Horizontal = 1,
	/// <summary>Drawing surface is inverted in the vertical axis. </summary>
	Vertical = 2,
	/// <summary>Drawing surface is inverted in both axes.</summary>
	Both = 3
}
