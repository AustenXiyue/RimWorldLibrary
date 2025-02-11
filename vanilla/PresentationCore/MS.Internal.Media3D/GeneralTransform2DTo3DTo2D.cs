using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MS.Internal.PresentationCore;

namespace MS.Internal.Media3D;

internal class GeneralTransform2DTo3DTo2D : GeneralTransform
{
	private struct Edge
	{
		public Point3D _start;

		public Point3D _end;

		public Edge(Point3D s, Point3D e)
		{
			_start = s;
			_end = e;
		}
	}

	private class EdgeInfo
	{
		public bool _hasFrontFace;

		public bool _hasBackFace;

		public Point _uv1;

		public Point _uv2;

		public int _numSharing;

		public EdgeInfo()
		{
			_hasFrontFace = (_hasBackFace = false);
			_numSharing = 0;
		}
	}

	private enum PolygonSide
	{
		FRONT,
		BACK
	}

	private bool _fInverse;

	private MeshGeometry3D _geometry;

	private Rect _visualBounds;

	private Rect _visualBrushBounds;

	private GeneralTransform _transform2D;

	private GeneralTransform _transform2DInverse;

	private Camera _camera;

	private Size _viewSize;

	private Rect3D _boundingRect;

	private Matrix3D _worldTransformation;

	private GeneralTransform3DTo2D _objectToViewport;

	private List<HitTestEdge> _validEdgesCache;

	private const double BUFFER_SIZE = 2.0;

	public override GeneralTransform Inverse
	{
		get
		{
			GeneralTransform2DTo3DTo2D obj = (GeneralTransform2DTo3DTo2D)Clone();
			obj.IsInverse = !IsInverse;
			return obj;
		}
	}

	internal override Transform AffineTransform
	{
		[FriendAccessAllowed]
		get
		{
			return null;
		}
	}

	internal bool IsInverse
	{
		get
		{
			return _fInverse;
		}
		set
		{
			_fInverse = value;
		}
	}

	internal GeneralTransform2DTo3DTo2D(Viewport2DVisual3D visual3D, Visual fromVisual)
	{
		IsInverse = false;
		_geometry = new MeshGeometry3D();
		_geometry.Positions = visual3D.InternalPositionsCache;
		_geometry.TextureCoordinates = visual3D.InternalTextureCoordinatesCache;
		_geometry.TriangleIndices = visual3D.InternalTriangleIndicesCache;
		_geometry.Freeze();
		Visual visual = visual3D.Visual;
		Visual visual2 = ((fromVisual == visual._parent) ? visual : fromVisual);
		_visualBrushBounds = visual.CalculateSubgraphRenderBoundsOuterSpace();
		_visualBounds = visual2.CalculateSubgraphRenderBoundsInnerSpace();
		GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
		generalTransformGroup.Children.Add(visual2.TransformToAncestor(visual));
		generalTransformGroup.Children.Add(visual.TransformToOuterSpace());
		generalTransformGroup.Freeze();
		_transform2D = generalTransformGroup;
		_transform2DInverse = _transform2D.Inverse;
		if (_transform2DInverse != null)
		{
			_transform2DInverse.Freeze();
		}
		Viewport3DVisual viewport3DVisual = (Viewport3DVisual)VisualTreeHelper.GetContainingVisual2D(visual3D);
		_camera = viewport3DVisual.Camera;
		if (_camera != null)
		{
			_camera = (Camera)viewport3DVisual.Camera.GetCurrentValueAsFrozen();
		}
		_viewSize = viewport3DVisual.Viewport.Size;
		_boundingRect = viewport3DVisual.ComputeSubgraphBounds3D();
		_objectToViewport = visual3D.TransformToAncestor(viewport3DVisual);
		if (_objectToViewport != null)
		{
			_objectToViewport.Freeze();
		}
		_worldTransformation = M3DUtil.GetWorldTransformationMatrix(visual3D);
		_validEdgesCache = null;
	}

	internal GeneralTransform2DTo3DTo2D()
	{
	}

	public override bool TryTransform(Point inPoint, out Point result)
	{
		if (IsInverse)
		{
			return TryInverseTransform(inPoint, out result);
		}
		return TryRegularTransform(inPoint, out result);
	}

	private bool TryInverseTransform(Point inPoint, out Point result)
	{
		bool foundIntersection = false;
		if (_camera != null)
		{
			double distanceAdjustment;
			RayHitTestParameters rayHitTestParameters = _camera.RayFromViewportPoint(inPoint, _viewSize, _boundingRect, out distanceAdjustment);
			rayHitTestParameters.PushVisualTransform(new MatrixTransform3D(_worldTransformation));
			Point pointHit = default(Point);
			_geometry.RayHitTest(rayHitTestParameters, FaceType.Front);
			rayHitTestParameters.RaiseCallback(delegate(HitTestResult rawresult)
			{
				if (rawresult is RayHitTestResult rayHitResult)
				{
					foundIntersection = Viewport2DVisual3D.GetIntersectionInfo(rayHitResult, out pointHit);
				}
				return HitTestResultBehavior.Stop;
			}, null, HitTestResultBehavior.Continue, distanceAdjustment);
			if (!foundIntersection)
			{
				foundIntersection = HandleOffMesh(inPoint, out pointHit);
			}
			result = Viewport2DVisual3D.TextureCoordsToVisualCoords(pointHit, _visualBrushBounds);
		}
		else
		{
			result = default(Point);
		}
		return foundIntersection;
	}

	private bool HandleOffMesh(Point mousePos, out Point outPoint)
	{
		Point[] array = new Point[4];
		if (_validEdgesCache == null)
		{
			array[0] = _transform2D.Transform(new Point(_visualBounds.Left, _visualBounds.Top));
			array[1] = _transform2D.Transform(new Point(_visualBounds.Right, _visualBounds.Top));
			array[2] = _transform2D.Transform(new Point(_visualBounds.Right, _visualBounds.Bottom));
			array[3] = _transform2D.Transform(new Point(_visualBounds.Left, _visualBounds.Bottom));
			Point[] array2 = new Point[4];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = Viewport2DVisual3D.VisualCoordsToTextureCoords(array[i], _visualBrushBounds);
			}
			_validEdgesCache = GrabValidEdges(array2);
		}
		return FindClosestIntersection(mousePos, _validEdgesCache, out outPoint);
	}

	private List<HitTestEdge> GrabValidEdges(Point[] visualTexCoordBounds)
	{
		List<HitTestEdge> list = new List<HitTestEdge>();
		Dictionary<Edge, EdgeInfo> dictionary = new Dictionary<Edge, EdgeInfo>();
		Point3DCollection positions = _geometry.Positions;
		PointCollection textureCoordinates = _geometry.TextureCoordinates;
		Int32Collection triangleIndices = _geometry.TriangleIndices;
		if (positions == null || textureCoordinates == null)
		{
			return new List<HitTestEdge>();
		}
		Matrix3D matrix3D = _worldTransformation * _camera.GetViewMatrix();
		try
		{
			matrix3D.Invert();
		}
		catch (InvalidOperationException)
		{
			return new List<HitTestEdge>();
		}
		Point3D camPosObjSpace = matrix3D.Transform(new Point3D(0.0, 0.0, 0.0));
		Rect empty = Rect.Empty;
		for (int i = 0; i < visualTexCoordBounds.Length; i++)
		{
			empty.Union(visualTexCoordBounds[i]);
		}
		Point3D[] array = new Point3D[3];
		Point[] array2 = new Point[3];
		if (triangleIndices == null || triangleIndices.Count == 0)
		{
			int count = textureCoordinates.Count;
			int count2 = positions.Count;
			count2 -= count2 % 3;
			for (int j = 0; j < count2; j += 3)
			{
				Rect empty2 = Rect.Empty;
				for (int k = 0; k < 3; k++)
				{
					array[k] = positions[j + k];
					if (j + k < count)
					{
						array2[k] = textureCoordinates[j + k];
					}
					else
					{
						array2[k] = new Point(0.0, 0.0);
					}
					empty2.Union(array2[k]);
				}
				if (empty.IntersectsWith(empty2))
				{
					ProcessTriangle(array, array2, visualTexCoordBounds, list, dictionary, camPosObjSpace);
				}
			}
		}
		else
		{
			int count3 = triangleIndices.Count;
			int count4 = positions.Count;
			int count5 = textureCoordinates.Count;
			int[] array3 = new int[3];
			for (int l = 2; l < count3; l += 3)
			{
				Rect empty3 = Rect.Empty;
				bool flag = true;
				bool flag2 = true;
				for (int m = 0; m < 3; m++)
				{
					array3[m] = triangleIndices[l - 2 + m];
					if (array3[m] < 0 || array3[m] >= count4)
					{
						flag2 = false;
						break;
					}
					if (array3[m] < 0 || array3[m] >= count5)
					{
						flag = false;
						break;
					}
					array[m] = positions[array3[m]];
					array2[m] = textureCoordinates[array3[m]];
					empty3.Union(array2[m]);
				}
				if (!flag2)
				{
					break;
				}
				if (flag && empty.IntersectsWith(empty3))
				{
					ProcessTriangle(array, array2, visualTexCoordBounds, list, dictionary, camPosObjSpace);
				}
			}
		}
		foreach (Edge key in dictionary.Keys)
		{
			EdgeInfo edgeInfo = dictionary[key];
			if (edgeInfo._hasFrontFace && edgeInfo._numSharing == 1)
			{
				HandleSilhouetteEdge(edgeInfo._uv1, edgeInfo._uv2, key._start, key._end, visualTexCoordBounds, list);
			}
		}
		if (_objectToViewport != null)
		{
			for (int n = 0; n < list.Count; n++)
			{
				list[n].Project(_objectToViewport);
			}
		}
		else
		{
			list = new List<HitTestEdge>();
		}
		return list;
	}

	private void ProcessTriangle(Point3D[] p, Point[] uv, Point[] visualTexCoordBounds, List<HitTestEdge> edgeList, Dictionary<Edge, EdgeInfo> adjInformation, Point3D camPosObjSpace)
	{
		Vector3D vector = Vector3D.CrossProduct(p[1] - p[0], p[2] - p[0]);
		Vector3D vector2 = camPosObjSpace - p[0];
		if (vector.X != 0.0 || vector.Y != 0.0 || vector.Z != 0.0)
		{
			if (Vector3D.DotProduct(vector, vector2) > 0.0)
			{
				ProcessTriangleEdges(p, uv, visualTexCoordBounds, PolygonSide.FRONT, edgeList, adjInformation);
				ProcessVisualBoundsIntersections(p, uv, visualTexCoordBounds, edgeList);
			}
			else
			{
				ProcessTriangleEdges(p, uv, visualTexCoordBounds, PolygonSide.BACK, edgeList, adjInformation);
			}
		}
	}

	private void ProcessVisualBoundsIntersections(Point3D[] p, Point[] uv, Point[] visualTexCoordBounds, List<HitTestEdge> edgeList)
	{
		List<Point3D> list = new List<Point3D>();
		List<Point> list2 = new List<Point>();
		for (int i = 0; i < visualTexCoordBounds.Length; i++)
		{
			Point point = visualTexCoordBounds[i];
			Point point2 = visualTexCoordBounds[(i + 1) % visualTexCoordBounds.Length];
			list.Clear();
			list2.Clear();
			bool flag = false;
			for (int j = 0; j < uv.Length; j++)
			{
				Point point3 = uv[j];
				Point point4 = uv[(j + 1) % uv.Length];
				Point3D point3D = p[j];
				Point3D point3D2 = p[(j + 1) % p.Length];
				if (Math.Max(point.X, point2.X) < Math.Min(point3.X, point4.X) || Math.Min(point.X, point2.X) > Math.Max(point3.X, point4.X) || Math.Max(point.Y, point2.Y) < Math.Min(point3.Y, point4.Y) || Math.Min(point.Y, point2.Y) > Math.Max(point3.Y, point4.Y))
				{
					continue;
				}
				bool coinc = false;
				Vector vector = point4 - point3;
				double num = IntersectRayLine(point3, vector, point, point2, out coinc);
				if (coinc)
				{
					HandleCoincidentLines(point, point2, point3D, point3D2, point3, point4, edgeList);
					flag = true;
					break;
				}
				if (num >= 0.0 && num <= 1.0)
				{
					Point point5 = point3 + vector * num;
					Point3D item = point3D + (point3D2 - point3D) * num;
					double length = (point - point2).Length;
					if ((point5 - point).Length < length && (point5 - point2).Length < length)
					{
						list.Add(item);
						list2.Add(point5);
					}
				}
			}
			if (flag)
			{
				continue;
			}
			Point3D inters3DPoint2;
			Point3D inters3DPoint3;
			if (list.Count >= 2)
			{
				edgeList.Add(new HitTestEdge(list[0], list[1], list2[0], list2[1]));
			}
			else if (list.Count == 1)
			{
				if (M3DUtil.IsPointInTriangle(point, uv, p, out var inters3DPoint))
				{
					edgeList.Add(new HitTestEdge(list[0], inters3DPoint, list2[0], point));
				}
				if (M3DUtil.IsPointInTriangle(point2, uv, p, out inters3DPoint))
				{
					edgeList.Add(new HitTestEdge(list[0], inters3DPoint, list2[0], point2));
				}
			}
			else if (M3DUtil.IsPointInTriangle(point, uv, p, out inters3DPoint2) && M3DUtil.IsPointInTriangle(point2, uv, p, out inters3DPoint3))
			{
				edgeList.Add(new HitTestEdge(inters3DPoint2, inters3DPoint3, point, point2));
			}
		}
	}

	private void HandleCoincidentLines(Point visUV1, Point visUV2, Point3D tri3D1, Point3D tri3D2, Point triUV1, Point triUV2, List<HitTestEdge> edgeList)
	{
		Point uv;
		Point3D p;
		Point uv2;
		Point3D p2;
		if (Math.Abs(visUV1.X - visUV2.X) > Math.Abs(visUV1.Y - visUV2.Y))
		{
			Point point;
			Point point2;
			if (visUV1.X <= visUV2.X)
			{
				point = visUV1;
				point2 = visUV2;
			}
			else
			{
				point = visUV2;
				point2 = visUV1;
			}
			Point point3;
			Point3D point3D;
			Point point4;
			Point3D point3D2;
			if (triUV1.X <= triUV2.X)
			{
				point3 = triUV1;
				point3D = tri3D1;
				point4 = triUV2;
				point3D2 = tri3D2;
			}
			else
			{
				point3 = triUV2;
				point3D = tri3D2;
				point4 = triUV1;
				point3D2 = tri3D1;
			}
			if (point.X < point3.X)
			{
				uv = point3;
				p = point3D;
			}
			else
			{
				uv = point;
				p = point3D + (point.X - point3.X) / (point4.X - point3.X) * (point3D2 - point3D);
			}
			if (point2.X > point4.X)
			{
				uv2 = point4;
				p2 = point3D2;
			}
			else
			{
				uv2 = point2;
				p2 = point3D + (point2.X - point3.X) / (point4.X - point3.X) * (point3D2 - point3D);
			}
		}
		else
		{
			Point point;
			Point point2;
			if (visUV1.Y <= visUV2.Y)
			{
				point = visUV1;
				point2 = visUV2;
			}
			else
			{
				point = visUV2;
				point2 = visUV1;
			}
			Point point3;
			Point3D point3D;
			Point point4;
			Point3D point3D2;
			if (triUV1.Y <= triUV2.Y)
			{
				point3 = triUV1;
				point3D = tri3D1;
				point4 = triUV2;
				point3D2 = tri3D2;
			}
			else
			{
				point3 = triUV2;
				point3D = tri3D2;
				point4 = triUV1;
				point3D2 = tri3D1;
			}
			if (point.Y < point3.Y)
			{
				uv = point3;
				p = point3D;
			}
			else
			{
				uv = point;
				p = point3D + (point.Y - point3.Y) / (point4.Y - point3.Y) * (point3D2 - point3D);
			}
			if (point2.Y > point4.Y)
			{
				uv2 = point4;
				p2 = point3D2;
			}
			else
			{
				uv2 = point2;
				p2 = point3D + (point2.Y - point3.Y) / (point4.Y - point3.Y) * (point3D2 - point3D);
			}
		}
		edgeList.Add(new HitTestEdge(p, p2, uv, uv2));
	}

	private double IntersectRayLine(Point o, Vector d, Point p1, Point p2, out bool coinc)
	{
		coinc = false;
		double num = p2.Y - p1.Y;
		double num2 = p2.X - p1.X;
		if (num2 == 0.0)
		{
			if (d.X == 0.0)
			{
				coinc = o.X == p1.X;
				return -1.0;
			}
			return (p2.X - o.X) / d.X;
		}
		double num3 = (o.X - p1.X) * num / num2 - o.Y + p1.Y;
		double num4 = d.Y - d.X * num / num2;
		if (num4 == 0.0)
		{
			double num5 = (0.0 - o.X) * num / num2 + o.Y;
			double num6 = (0.0 - p1.X) * num / num2 + p1.Y;
			coinc = num5 == num6;
			return -1.0;
		}
		return num3 / num4;
	}

	private void ProcessTriangleEdges(Point3D[] p, Point[] uv, Point[] visualTexCoordBounds, PolygonSide polygonSide, List<HitTestEdge> edgeList, Dictionary<Edge, EdgeInfo> adjInformation)
	{
		for (int i = 0; i < p.Length; i++)
		{
			Point3D point3D = p[i];
			Point3D point3D2 = p[(i + 1) % p.Length];
			Edge key;
			Point uv2;
			Point uv3;
			if (point3D.X < point3D2.X || (point3D.X == point3D2.X && point3D.Y < point3D2.Y) || (point3D.X == point3D2.X && point3D.Y == point3D2.Y && point3D.Z < point3D.Z))
			{
				key = new Edge(point3D, point3D2);
				uv2 = uv[i];
				uv3 = uv[(i + 1) % p.Length];
			}
			else
			{
				key = new Edge(point3D2, point3D);
				uv3 = uv[i];
				uv2 = uv[(i + 1) % p.Length];
			}
			EdgeInfo edgeInfo2 = (adjInformation.ContainsKey(key) ? adjInformation[key] : (adjInformation[key] = new EdgeInfo()));
			edgeInfo2._numSharing++;
			bool num = edgeInfo2._hasBackFace && edgeInfo2._hasFrontFace;
			if (polygonSide == PolygonSide.FRONT)
			{
				edgeInfo2._hasFrontFace = true;
				edgeInfo2._uv1 = uv2;
				edgeInfo2._uv2 = uv3;
			}
			else
			{
				edgeInfo2._hasBackFace = true;
			}
			if (!num && edgeInfo2._hasBackFace && edgeInfo2._hasFrontFace)
			{
				HandleSilhouetteEdge(edgeInfo2._uv1, edgeInfo2._uv2, key._start, key._end, visualTexCoordBounds, edgeList);
			}
		}
	}

	private void HandleSilhouetteEdge(Point uv1, Point uv2, Point3D p3D1, Point3D p3D2, Point[] bounds, List<HitTestEdge> edgeList)
	{
		List<Point3D> list = new List<Point3D>();
		List<Point> list2 = new List<Point>();
		Vector vector = uv2 - uv1;
		for (int i = 0; i < bounds.Length; i++)
		{
			Point point = bounds[i];
			Point point2 = bounds[(i + 1) % bounds.Length];
			if (Math.Max(point.X, point2.X) < Math.Min(uv1.X, uv2.X) || Math.Min(point.X, point2.X) > Math.Max(uv1.X, uv2.X) || Math.Max(point.Y, point2.Y) < Math.Min(uv1.Y, uv2.Y) || Math.Min(point.Y, point2.Y) > Math.Max(uv1.Y, uv2.Y))
			{
				continue;
			}
			bool coinc = false;
			double num = IntersectRayLine(uv1, vector, point, point2, out coinc);
			if (coinc)
			{
				return;
			}
			if (num >= 0.0 && num <= 1.0)
			{
				Point point3 = uv1 + vector * num;
				Point3D item = p3D1 + (p3D2 - p3D1) * num;
				double length = (point - point2).Length;
				if ((point3 - point).Length < length && (point3 - point2).Length < length)
				{
					list.Add(item);
					list2.Add(point3);
				}
			}
		}
		if (list.Count >= 2)
		{
			edgeList.Add(new HitTestEdge(list[0], list[1], list2[0], list2[1]));
		}
		else if (list.Count == 1)
		{
			if (IsPointInPolygon(bounds, uv1))
			{
				edgeList.Add(new HitTestEdge(list[0], p3D1, list2[0], uv1));
			}
			if (IsPointInPolygon(bounds, uv2))
			{
				edgeList.Add(new HitTestEdge(list[0], p3D2, list2[0], uv2));
			}
		}
		else if (IsPointInPolygon(bounds, uv1) && IsPointInPolygon(bounds, uv2))
		{
			edgeList.Add(new HitTestEdge(p3D1, p3D2, uv1, uv2));
		}
	}

	private bool IsPointInPolygon(Point[] polygon, Point p)
	{
		bool flag = false;
		for (int i = 0; i < polygon.Length; i++)
		{
			bool flag2 = Vector.CrossProduct(polygon[(i + 1) % polygon.Length] - polygon[i], polygon[i] - p) > 0.0;
			if (i == 0)
			{
				flag = flag2;
			}
			else if (flag != flag2)
			{
				return false;
			}
		}
		return true;
	}

	private bool FindClosestIntersection(Point mousePos, List<HitTestEdge> edges, out Point finalPoint)
	{
		bool result = false;
		double num = double.MaxValue;
		Point uv = default(Point);
		finalPoint = default(Point);
		int i = 0;
		for (int count = edges.Count; i < count; i++)
		{
			Vector vector = mousePos - edges[i]._p1Transformed;
			Vector vector2 = edges[i]._p2Transformed - edges[i]._p1Transformed;
			double num2 = vector2 * vector2;
			Point point;
			double length;
			if (num2 == 0.0)
			{
				point = edges[i]._p1Transformed;
				length = vector.Length;
			}
			else
			{
				double num3 = vector2 * vector;
				point = ((num3 < 0.0) ? edges[i]._p1Transformed : ((!(num3 > num2)) ? (edges[i]._p1Transformed + num3 / num2 * vector2) : edges[i]._p2Transformed));
				length = (mousePos - point).Length;
			}
			if (length < num)
			{
				num = length;
				uv = ((num2 == 0.0) ? edges[i]._uv1 : ((point - edges[i]._p1Transformed).Length / Math.Sqrt(num2) * (edges[i]._uv2 - edges[i]._uv1) + edges[i]._uv1));
			}
		}
		if (num != double.MaxValue)
		{
			Point point2 = Viewport2DVisual3D.TextureCoordsToVisualCoords(uv, _visualBrushBounds);
			if (_transform2DInverse != null)
			{
				Point point3 = _transform2DInverse.Transform(point2);
				if (point3.X <= _visualBounds.Left + 1.0)
				{
					point3.X -= 2.0;
				}
				if (point3.Y <= _visualBounds.Top + 1.0)
				{
					point3.Y -= 2.0;
				}
				if (point3.X >= _visualBounds.Right - 1.0)
				{
					point3.X += 2.0;
				}
				if (point3.Y >= _visualBounds.Bottom - 1.0)
				{
					point3.Y += 2.0;
				}
				Point pt = _transform2D.Transform(point3);
				finalPoint = Viewport2DVisual3D.VisualCoordsToTextureCoords(pt, _visualBrushBounds);
				result = true;
			}
		}
		return result;
	}

	private bool TryRegularTransform(Point inPoint, out Point result)
	{
		Point point = Viewport2DVisual3D.VisualCoordsToTextureCoords(inPoint, _visualBrushBounds);
		if (_objectToViewport != null && Viewport2DVisual3D.Get3DPointFor2DCoordinate(point, out var point3D, _geometry.Positions, _geometry.TextureCoordinates, _geometry.TriangleIndices))
		{
			return _objectToViewport.TryTransform(point3D, out result);
		}
		result = default(Point);
		return false;
	}

	public override Rect TransformBounds(Rect rect)
	{
		List<HitTestEdge> list = null;
		rect.Intersect(_visualBrushBounds);
		list = GrabValidEdges(new Point[4]
		{
			Viewport2DVisual3D.VisualCoordsToTextureCoords(rect.TopLeft, _visualBrushBounds),
			Viewport2DVisual3D.VisualCoordsToTextureCoords(rect.TopRight, _visualBrushBounds),
			Viewport2DVisual3D.VisualCoordsToTextureCoords(rect.BottomRight, _visualBrushBounds),
			Viewport2DVisual3D.VisualCoordsToTextureCoords(rect.BottomLeft, _visualBrushBounds)
		});
		Rect empty = Rect.Empty;
		if (list != null)
		{
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				empty.Union(list[i]._p1Transformed);
				empty.Union(list[i]._p2Transformed);
			}
		}
		return empty;
	}

	protected override Freezable CreateInstanceCore()
	{
		return new GeneralTransform2DTo3DTo2D();
	}

	protected override void CloneCore(Freezable sourceFreezable)
	{
		GeneralTransform2DTo3DTo2D transform = (GeneralTransform2DTo3DTo2D)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(transform);
	}

	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		GeneralTransform2DTo3DTo2D transform = (GeneralTransform2DTo3DTo2D)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(transform);
	}

	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		GeneralTransform2DTo3DTo2D transform = (GeneralTransform2DTo3DTo2D)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CopyCommon(transform);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		GeneralTransform2DTo3DTo2D transform = (GeneralTransform2DTo3DTo2D)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CopyCommon(transform);
	}

	private void CopyCommon(GeneralTransform2DTo3DTo2D transform)
	{
		_fInverse = transform._fInverse;
		_geometry = transform._geometry;
		_visualBounds = transform._visualBounds;
		_visualBrushBounds = transform._visualBrushBounds;
		_transform2D = transform._transform2D;
		_transform2DInverse = transform._transform2DInverse;
		_camera = transform._camera;
		_viewSize = transform._viewSize;
		_boundingRect = transform._boundingRect;
		_worldTransformation = transform._worldTransformation;
		_objectToViewport = transform._objectToViewport;
		_validEdgesCache = null;
	}
}
