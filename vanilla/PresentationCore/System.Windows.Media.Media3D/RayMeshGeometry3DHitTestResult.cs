namespace System.Windows.Media.Media3D;

/// <summary>Represents an intersection between a ray hit test and a <see cref="T:System.Windows.Media.Media3D.MeshGeometry3D" />.</summary>
public sealed class RayMeshGeometry3DHitTestResult : RayHitTestResult
{
	private double _distanceToRayOrigin;

	private readonly int _vertexIndex1;

	private readonly int _vertexIndex2;

	private readonly int _vertexIndex3;

	private readonly Point _barycentricCoordinate;

	private readonly MeshGeometry3D _meshHit;

	private readonly Point3D _pointHit;

	/// <summary>Gets the point at which the mesh was intersected by the ray along which the hit test was performed.</summary>
	/// <returns>Point3D at which the mesh was intersected.</returns>
	public override Point3D PointHit => _pointHit;

	/// <summary>Gets the distance between the point of intersection and the ray's origin in the coordinate space of <see cref="T:System.Windows.Media.Media3D.Visual3D" /> which initiated the hit test.</summary>
	/// <returns>Double that indicates the distance between the point of intersection and the ray's origin in the coordinate space of <see cref="T:System.Windows.Media.Media3D.Visual3D" /> which initiated the hit test.</returns>
	public override double DistanceToRayOrigin => _distanceToRayOrigin;

	/// <summary>First vertex of the mesh triangle intersected by the ray.</summary>
	/// <returns>The index of the first vertex. </returns>
	public int VertexIndex1 => _vertexIndex1;

	/// <summary>Second vertex of the mesh triangle intersected by the ray.</summary>
	/// <returns>The index of the second vertex.</returns>
	public int VertexIndex2 => _vertexIndex2;

	/// <summary>Third vertex of the mesh triangle intersected by the ray.</summary>
	/// <returns>The index of the third vertex.</returns>
	public int VertexIndex3 => _vertexIndex3;

	/// <summary>Relative contribution of the first vertex of a mesh triangle to the point at which that triangle was intersected by the hit test, expressed as a value between zero and 1.</summary>
	/// <returns>The weighting of the first vertex. </returns>
	public double VertexWeight1 => 1.0 - VertexWeight2 - VertexWeight3;

	/// <summary>Relative contribution of the second vertex of a mesh triangle to the point at which that triangle was intersected by the hit test, expressed as a value between zero and 1.</summary>
	/// <returns>The weighting of the second vertex.</returns>
	public double VertexWeight2 => _barycentricCoordinate.X;

	/// <summary>Relative contribution of the third vertex of a mesh triangle to the point at which that triangle was intersected by the hit test, expressed as a value between zero and 1.</summary>
	/// <returns>The weighting of the third vertex.</returns>
	public double VertexWeight3 => _barycentricCoordinate.Y;

	/// <summary>Gets the <see cref="T:System.Windows.Media.Media3D.MeshGeometry3D" /> intersected by this hit test.</summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.MeshGeometry3D" /> intersected by the ray.</returns>
	public MeshGeometry3D MeshHit => _meshHit;

	internal RayMeshGeometry3DHitTestResult(Visual3D visualHit, Model3D modelHit, MeshGeometry3D meshHit, Point3D pointHit, double distanceToRayOrigin, int vertexIndex1, int vertexIndex2, int vertexIndex3, Point barycentricCoordinate)
		: base(visualHit, modelHit)
	{
		_meshHit = meshHit;
		_pointHit = pointHit;
		_distanceToRayOrigin = distanceToRayOrigin;
		_vertexIndex1 = vertexIndex1;
		_vertexIndex2 = vertexIndex2;
		_vertexIndex3 = vertexIndex3;
		_barycentricCoordinate = barycentricCoordinate;
	}

	internal override void SetDistanceToRayOrigin(double distance)
	{
		_distanceToRayOrigin = distance;
	}
}
