namespace System.Windows.Media;

/// <summary>Returns the results of a hit test that uses a <see cref="T:System.Windows.Media.Geometry" /> as a hit test parameter.</summary>
public class GeometryHitTestResult : HitTestResult
{
	private IntersectionDetail _intersectionDetail;

	/// <summary>Gets the <see cref="T:System.Windows.Media.IntersectionDetail" /> value of the hit test.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.IntersectionDetail" /> value of the hit test.</returns>
	public IntersectionDetail IntersectionDetail => _intersectionDetail;

	/// <summary>Gets the visual object that is returned from a hit test result.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Visual" /> object that represents the hit test result.</returns>
	public new Visual VisualHit => (Visual)base.VisualHit;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GeometryHitTestResult" /> class by using a specified visual object and <see cref="T:System.Windows.Media.IntersectionDetail" /> value.</summary>
	/// <param name="visualHit">The visual object that is hit during a hit test.</param>
	/// <param name="intersectionDetail">Describes the intersection between a <see cref="T:System.Windows.Media.Geometry" /> and <paramref name="visualHit" />.</param>
	public GeometryHitTestResult(Visual visualHit, IntersectionDetail intersectionDetail)
		: base(visualHit)
	{
		_intersectionDetail = intersectionDetail;
	}
}
