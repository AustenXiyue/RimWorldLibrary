using System.Collections.Generic;
using MS.Internal.Media3D;

namespace System.Windows.Media.Media3D;

/// <summary>Specifies the parameters of a hit test along a ray.</summary>
public sealed class RayHitTestParameters : HitTestParameters3D
{
	private readonly Point3D _origin;

	private readonly Vector3D _direction;

	private readonly List<RayHitTestResult> results = new List<RayHitTestResult>();

	private bool _isRay;

	/// <summary> Gets the origin of the ray along which to hit test. </summary>
	/// <returns>Origin of the ray along which to hit test.</returns>
	public Point3D Origin => _origin;

	/// <summary> Gets or sets a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> that indicates the direction (from its origin) of the ray along which to hit test. </summary>
	/// <returns>Vector3D that indicates the direction of the ray along which to hit test.</returns>
	public Vector3D Direction => _direction;

	internal bool IsRay => _isRay;

	/// <summary>Creates an instance of the object that specifies the origin and direction of the ray along which to hit test.</summary>
	/// <param name="origin">Point3D at which the ray originates.</param>
	/// <param name="direction">Vector3D that indicates the direction of the ray.</param>
	public RayHitTestParameters(Point3D origin, Vector3D direction)
	{
		_origin = origin;
		_direction = direction;
		_isRay = true;
	}

	internal void ReportResult(MeshGeometry3D meshHit, Point3D pointHit, double distanceToRayOrigin, int vertexIndex1, int vertexIndex2, int vertexIndex3, Point barycentric)
	{
		results.Add(new RayMeshGeometry3DHitTestResult(CurrentVisual, CurrentModel, meshHit, pointHit, distanceToRayOrigin, vertexIndex1, vertexIndex2, vertexIndex3, barycentric));
	}

	internal HitTestResultBehavior RaiseCallback(HitTestResultCallback resultCallback, HitTestFilterCallback filterCallback, HitTestResultBehavior lastResult)
	{
		return RaiseCallback(resultCallback, filterCallback, lastResult, 0.0);
	}

	internal HitTestResultBehavior RaiseCallback(HitTestResultCallback resultCallback, HitTestFilterCallback filterCallback, HitTestResultBehavior lastResult, double distanceAdjustment)
	{
		results.Sort(RayHitTestResult.CompareByDistanceToRayOrigin);
		int i = 0;
		for (int count = results.Count; i < count; i++)
		{
			RayHitTestResult rayHitTestResult = results[i];
			rayHitTestResult.SetDistanceToRayOrigin(rayHitTestResult.DistanceToRayOrigin + distanceAdjustment);
			if (rayHitTestResult.VisualHit is Viewport2DVisual3D { Visual: { } visual } && Viewport2DVisual3D.GetIntersectionInfo(rayHitTestResult, out var outputPoint))
			{
				Point inPoint = Viewport2DVisual3D.TextureCoordsToVisualCoords(outputPoint, visual);
				GeneralTransform inverse = visual.TransformToOuterSpace().Inverse;
				if (inverse != null && inverse.TryTransform(inPoint, out var result) && visual.HitTestPoint(filterCallback, resultCallback, new PointHitTestParameters(result)) == HitTestResultBehavior.Stop)
				{
					return HitTestResultBehavior.Stop;
				}
			}
			if (resultCallback(results[i]) == HitTestResultBehavior.Stop)
			{
				return HitTestResultBehavior.Stop;
			}
		}
		return lastResult;
	}

	internal void GetLocalLine(out Point3D origin, out Vector3D direction)
	{
		origin = _origin;
		direction = _direction;
		bool isRay = true;
		if (base.HasWorldTransformMatrix)
		{
			LineUtil.Transform(base.WorldTransformMatrix, ref origin, ref direction, out isRay);
		}
		_isRay &= isRay;
	}

	internal void ClearResults()
	{
		if (results != null)
		{
			results.Clear();
		}
	}
}
