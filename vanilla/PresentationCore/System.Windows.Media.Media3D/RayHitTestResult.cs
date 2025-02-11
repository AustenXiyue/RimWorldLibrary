namespace System.Windows.Media.Media3D;

/// <summary>Represents the result of a hit test along a ray.</summary>
public abstract class RayHitTestResult : HitTestResult
{
	private readonly Model3D _modelHit;

	/// <summary>Gets the Visual intersected by the ray along which the hit test was performed.</summary>
	/// <returns>Visual3D intersected by the ray.</returns>
	public new Visual3D VisualHit => (Visual3D)base.VisualHit;

	/// <summary>Gets the Model3D intersected by the ray along which the hit test was performed.</summary>
	/// <returns>Model3D intersected by the ray.</returns>
	public Model3D ModelHit => _modelHit;

	/// <summary>Gets the Point3D at the intersection between the ray along which the hit test was performed and the hit object.</summary>
	/// <returns>Point3D at which the hit object was intersected by the ray.</returns>
	public abstract Point3D PointHit { get; }

	/// <summary>Gets the distance between the hit intersection and the inner coordinate space of the <see cref="T:System.Windows.Media.Media3D.Visual3D" /> which initiated the hit test.</summary>
	/// <returns>Double that indicates the distance between the hit intersection and the inner coordinate space of the <see cref="T:System.Windows.Media.Media3D.Visual3D" /> which initiated the hit test.</returns>
	public abstract double DistanceToRayOrigin { get; }

	internal RayHitTestResult(Visual3D visualHit, Model3D modelHit)
		: base(visualHit)
	{
		_modelHit = modelHit;
	}

	internal abstract void SetDistanceToRayOrigin(double distance);

	internal static int CompareByDistanceToRayOrigin(RayHitTestResult x, RayHitTestResult y)
	{
		return Math.Sign(x.DistanceToRayOrigin - y.DistanceToRayOrigin);
	}
}
