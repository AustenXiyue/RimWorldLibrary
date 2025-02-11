namespace System.Windows.Media;

/// <summary>Specifies a <see cref="T:System.Windows.Point" /> as the parameter to be used for hit testing of a visual object.</summary>
public class PointHitTestParameters : HitTestParameters
{
	private Point _hitPoint;

	/// <summary>Gets the <see cref="T:System.Windows.Point" /> against which to hit test. </summary>
	/// <returns>The <see cref="T:System.Windows.Point" /> against which to hit test.</returns>
	public Point HitPoint => _hitPoint;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.PointHitTestParameters" /> class, using a <see cref="T:System.Windows.Point" />.</summary>
	/// <param name="point">The parameter that is specified as the <see cref="T:System.Windows.Point" /> value.</param>
	public PointHitTestParameters(Point point)
	{
		_hitPoint = point;
	}

	internal void SetHitPoint(Point hitPoint)
	{
		_hitPoint = hitPoint;
	}
}
