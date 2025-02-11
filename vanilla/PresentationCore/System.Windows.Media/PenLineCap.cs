namespace System.Windows.Media;

/// <summary>Describes the shape at the end of a line or segment. </summary>
public enum PenLineCap
{
	/// <summary>A cap that does not extend past the last point of the line. Comparable to no line cap.</summary>
	Flat,
	/// <summary>A rectangle that has a height equal to the line thickness and a length equal to half the line thickness. </summary>
	Square,
	/// <summary>A semicircle that has a diameter equal to the line thickness. </summary>
	Round,
	/// <summary>An isosceles right triangle whose base length is equal to the thickness of the line.     </summary>
	Triangle
}
