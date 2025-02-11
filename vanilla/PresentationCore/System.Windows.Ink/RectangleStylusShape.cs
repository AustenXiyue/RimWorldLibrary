namespace System.Windows.Ink;

/// <summary>Represents a rectangular stylus tip.</summary>
public sealed class RectangleStylusShape : StylusShape
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Ink.RectangleStylusShape" /> class with the specified width and height.</summary>
	public RectangleStylusShape(double width, double height)
		: this(width, height, 0.0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Ink.RectangleStylusShape" /> class with the specified width, height, and angle.</summary>
	public RectangleStylusShape(double width, double height, double rotation)
		: base(StylusTip.Rectangle, width, height, rotation)
	{
	}
}
