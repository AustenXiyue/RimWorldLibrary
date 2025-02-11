namespace System.Windows.Media;

/// <summary>Represents the results of a hit test that uses a <see cref="T:System.Windows.Point" /> as a hit test parameter.</summary>
public class PointHitTestResult : HitTestResult
{
	private Point _pointHit;

	/// <summary>Gets the point value that is returned from a hit test result.</summary>
	/// <returns>A <see cref="T:System.Windows.Point" /> object that represents the hit test result.</returns>
	public Point PointHit => _pointHit;

	/// <summary>Gets the visual object that is returned from a hit test result.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Visual" /> object that represents the hit test result.</returns>
	public new Visual VisualHit => (Visual)base.VisualHit;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.PointHitTestResult" /> class.</summary>
	/// <param name="visualHit">The <see cref="T:System.Windows.Media.Visual" /> object that represents the hit test result.</param>
	/// <param name="pointHit">The <see cref="T:System.Windows.Point" /> object that represents the hit test result.</param>
	public PointHitTestResult(Visual visualHit, Point pointHit)
		: base(visualHit)
	{
		_pointHit = pointHit;
	}
}
