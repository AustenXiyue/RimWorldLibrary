namespace System.Windows.Media;

/// <summary>Provides information about the intersection between the geometries in the <see cref="T:System.Windows.Media.GeometryHitTestParameters" /> and the visual which was hit.</summary>
public enum IntersectionDetail
{
	/// <summary>The <see cref="T:System.Windows.Media.IntersectionDetail" /> value is not calculated.</summary>
	NotCalculated,
	/// <summary>The <see cref="T:System.Windows.Media.Geometry" /> hit test parameter and the target visual, or geometry, do not intersect.</summary>
	Empty,
	/// <summary>The target visual, or geometry, is fully inside the <see cref="T:System.Windows.Media.Geometry" /> hit test parameter.</summary>
	FullyInside,
	/// <summary>The <see cref="T:System.Windows.Media.Geometry" /> hit test parameter is fully contained within the boundary of the target visual or geometry.</summary>
	FullyContains,
	/// <summary>The <see cref="T:System.Windows.Media.Geometry" /> hit test parameter and the target visual, or geometry, intersect. This means that the two elements overlap, but neither element contains the other.</summary>
	Intersects
}
