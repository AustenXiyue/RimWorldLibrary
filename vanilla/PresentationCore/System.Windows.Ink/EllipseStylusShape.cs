namespace System.Windows.Ink;

/// <summary>Represents a stylus tip shaped like an ellipse.</summary>
public sealed class EllipseStylusShape : StylusShape
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Ink.EllipseStylusShape" /> class with the specified width and height. </summary>
	public EllipseStylusShape(double width, double height)
		: this(width, height, 0.0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Ink.EllipseStylusShape" /> class with the specified width, height, and angle.</summary>
	/// <param name="rotation">The angle of the stylus shape.</param>
	public EllipseStylusShape(double width, double height, double rotation)
		: base(StylusTip.Ellipse, width, height, rotation)
	{
	}
}
