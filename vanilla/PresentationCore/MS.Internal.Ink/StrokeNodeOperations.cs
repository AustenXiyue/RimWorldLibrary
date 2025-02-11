using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;

namespace MS.Internal.Ink;

internal class StrokeNodeOperations
{
	internal enum HitResult
	{
		Hit,
		Left,
		Right,
		InFront,
		Behind
	}

	private Rect _shapeBounds = Rect.Empty;

	protected Vector[] _vertices;

	internal virtual bool IsNodeShapeEllipse => false;

	internal static StrokeNodeOperations CreateInstance(StylusShape nodeShape)
	{
		if (nodeShape == null)
		{
			throw new ArgumentNullException("nodeShape");
		}
		if (nodeShape.IsEllipse)
		{
			return new EllipticalNodeOperations(nodeShape);
		}
		return new StrokeNodeOperations(nodeShape);
	}

	internal StrokeNodeOperations(StylusShape nodeShape)
	{
		_vertices = nodeShape.GetVerticesAsVectors();
	}

	internal Rect GetNodeBounds(in StrokeNodeData node)
	{
		if (_shapeBounds.IsEmpty)
		{
			int i;
			for (i = 0; i + 1 < _vertices.Length; i += 2)
			{
				_shapeBounds.Union(new Rect((Point)_vertices[i], (Point)_vertices[i + 1]));
			}
			if (i < _vertices.Length)
			{
				_shapeBounds.Union((Point)_vertices[i]);
			}
		}
		Rect result = _shapeBounds;
		double num = node.PressureFactor;
		if (!DoubleUtil.AreClose(num, 1.0))
		{
			result = new Rect(_shapeBounds.X * num, _shapeBounds.Y * num, _shapeBounds.Width * num, _shapeBounds.Height * num);
		}
		result.Location += (Vector)node.Position;
		return result;
	}

	internal void GetNodeContourPoints(in StrokeNodeData node, List<Point> pointBuffer)
	{
		double num = node.PressureFactor;
		if (DoubleUtil.AreClose(num, 1.0))
		{
			for (int i = 0; i < _vertices.Length; i++)
			{
				pointBuffer.Add(node.Position + _vertices[i]);
			}
		}
		else
		{
			for (int j = 0; j < _vertices.Length; j++)
			{
				pointBuffer.Add(node.Position + _vertices[j] * num);
			}
		}
	}

	internal virtual IEnumerable<ContourSegment> GetContourSegments(StrokeNodeData node, Quad quad)
	{
		int i;
		if (quad.IsEmpty)
		{
			Point begin = node.Position + _vertices[_vertices.Length - 1] * node.PressureFactor;
			for (i = 0; i < _vertices.Length; i++)
			{
				Point nextVertex = node.Position + _vertices[i] * node.PressureFactor;
				yield return new ContourSegment(begin, nextVertex);
				begin = nextVertex;
			}
			yield break;
		}
		yield return new ContourSegment(quad.A, quad.B);
		i = 0;
		for (int count = _vertices.Length; i < count; i++)
		{
			Point point = node.Position + _vertices[i] * node.PressureFactor;
			if (!(point == quad.B))
			{
				continue;
			}
			for (int j = 0; j < count; j++)
			{
				if (!(point != quad.C))
				{
					break;
				}
				i = (i + 1) % count;
				Point nextVertex = node.Position + _vertices[i] * node.PressureFactor;
				yield return new ContourSegment(point, nextVertex);
				point = nextVertex;
			}
			break;
		}
		yield return new ContourSegment(quad.C, quad.D);
		yield return new ContourSegment(quad.D, quad.A);
	}

	internal virtual IEnumerable<ContourSegment> GetNonBezierContourSegments(StrokeNodeData beginNode, StrokeNodeData endNode)
	{
		Quad quad = (beginNode.IsEmpty ? Quad.Empty : GetConnectingQuad(in beginNode, in endNode));
		return GetContourSegments(endNode, quad);
	}

	internal virtual Quad GetConnectingQuad(in StrokeNodeData beginNode, in StrokeNodeData endNode)
	{
		if (beginNode.IsEmpty || endNode.IsEmpty || DoubleUtil.AreClose(beginNode.Position, endNode.Position))
		{
			return Quad.Empty;
		}
		Quad empty = Quad.Empty;
		bool flag = false;
		bool flag2 = false;
		Vector vector = endNode.Position - beginNode.Position;
		double num = endNode.PressureFactor - beginNode.PressureFactor;
		int num2 = _vertices.Length;
		int num3 = 0;
		int num4 = num2 - 1;
		while (num3 < num2)
		{
			Vector vector2 = vector + _vertices[num3] * num;
			if (num != 0.0 && vector2.X == 0.0 && vector2.Y == 0.0)
			{
				return Quad.Empty;
			}
			if (WhereIsVectorAboutVector(vector2, _vertices[(num3 + 1) % num2] - _vertices[num3]) == HitResult.Left)
			{
				if (!flag)
				{
					HitResult hitResult = WhereIsVectorAboutVector(_vertices[num3] - _vertices[num4], vector2);
					if (HitResult.Right != hitResult)
					{
						flag = true;
						empty.A = beginNode.Position + _vertices[num3] * beginNode.PressureFactor;
						empty.B = endNode.Position + _vertices[num3] * endNode.PressureFactor;
						if (flag2)
						{
							break;
						}
					}
				}
			}
			else if (!flag2)
			{
				HitResult hitResult2 = WhereIsVectorAboutVector(_vertices[num3] - _vertices[num4], vector2);
				if (HitResult.Right == hitResult2)
				{
					flag2 = true;
					empty.C = endNode.Position + _vertices[num3] * endNode.PressureFactor;
					empty.D = beginNode.Position + _vertices[num3] * beginNode.PressureFactor;
					if (flag)
					{
						break;
					}
				}
			}
			num3++;
			num4 = (num4 + 1) % num2;
		}
		if (!flag || !flag2 || (num != 0.0 && Vector.Determinant(empty.B - empty.A, empty.D - empty.A) == 0.0))
		{
			return Quad.Empty;
		}
		return empty;
	}

	internal virtual bool HitTest(in StrokeNodeData beginNode, in StrokeNodeData endNode, Quad quad, Point hitBeginPoint, Point hitEndPoint)
	{
		if (quad.IsEmpty)
		{
			Point position;
			double num;
			if (beginNode.IsEmpty || endNode.PressureFactor > beginNode.PressureFactor)
			{
				position = endNode.Position;
				num = endNode.PressureFactor;
			}
			else
			{
				position = beginNode.Position;
				num = beginNode.PressureFactor;
			}
			Vector hitBegin = hitBeginPoint - position;
			Vector hitEnd = hitEndPoint - position;
			if (num != 1.0)
			{
				hitBegin /= num;
				hitEnd /= num;
			}
			return HitTestPolygonSegment(_vertices, hitBegin, hitEnd);
		}
		Vector hitBegin2 = hitBeginPoint - beginNode.Position;
		Vector hitEnd2 = hitEndPoint - beginNode.Position;
		HitResult hitResult = WhereIsSegmentAboutSegment(hitBegin2, hitEnd2, quad.C - beginNode.Position, quad.D - beginNode.Position);
		if (HitResult.Left == hitResult)
		{
			return false;
		}
		HitResult hitResult2 = hitResult;
		HitResult prevHitResult = hitResult;
		double num2 = beginNode.PressureFactor;
		int num3 = 0;
		int num4 = _vertices.Length;
		Vector vector = default(Vector);
		for (num3 = 0; num3 < num4; num3++)
		{
			vector = _vertices[num3] * num2;
			if (beginNode.Position + vector == quad.D)
			{
				break;
			}
		}
		for (int i = 0; i < 2; i++)
		{
			Point point = ((i == 0) ? beginNode.Position : endNode.Position);
			Point point2 = ((i == 0) ? quad.A : quad.C);
			num4 = _vertices.Length;
			while (point + vector != point2 && num4 != 0)
			{
				num3 = (num3 + 1) % _vertices.Length;
				Vector vector2 = ((num2 == 1.0) ? _vertices[num3] : (_vertices[num3] * num2));
				hitResult = WhereIsSegmentAboutSegment(hitBegin2, hitEnd2, vector, vector2);
				if (hitResult == HitResult.Hit)
				{
					return true;
				}
				if (IsOutside(hitResult, prevHitResult))
				{
					return false;
				}
				prevHitResult = hitResult;
				vector = vector2;
				num4--;
			}
			if (i == 0)
			{
				num2 = endNode.PressureFactor;
				Vector vector3 = endNode.Position - beginNode.Position;
				vector -= vector3;
				hitBegin2 -= vector3;
				hitEnd2 -= vector3;
				num4 = _vertices.Length;
				while (endNode.Position + _vertices[num3] * num2 != quad.B && num4 != 0)
				{
					num3 = (num3 + 1) % _vertices.Length;
					num4--;
				}
				num3--;
			}
		}
		return !IsOutside(hitResult2, hitResult);
	}

	internal virtual bool HitTest(in StrokeNodeData beginNode, in StrokeNodeData endNode, Quad quad, IEnumerable<ContourSegment> hitContour)
	{
		if (quad.IsEmpty)
		{
			return HitTestPolygonContourSegments(hitContour, in beginNode, in endNode);
		}
		return HitTestInkContour(hitContour, quad, in beginNode, in endNode);
	}

	internal virtual StrokeFIndices CutTest(in StrokeNodeData beginNode, in StrokeNodeData endNode, Quad quad, Point hitBeginPoint, Point hitEndPoint)
	{
		StrokeFIndices empty = StrokeFIndices.Empty;
		for (int i = (beginNode.IsEmpty ? 1 : 0); i < 2; i++)
		{
			Point point = ((i == 0) ? beginNode.Position : endNode.Position);
			double num = ((i == 0) ? beginNode.PressureFactor : endNode.PressureFactor);
			Vector hitBegin = hitBeginPoint - point;
			Vector hitEnd = hitEndPoint - point;
			if (num != 1.0)
			{
				hitBegin /= num;
				hitEnd /= num;
			}
			if (!HitTestPolygonSegment(_vertices, hitBegin, hitEnd))
			{
				continue;
			}
			if (i == 0)
			{
				empty.BeginFIndex = StrokeFIndices.BeforeFirst;
				empty.EndFIndex = 0.0;
				continue;
			}
			empty.EndFIndex = StrokeFIndices.AfterLast;
			if (beginNode.IsEmpty)
			{
				empty.BeginFIndex = StrokeFIndices.BeforeFirst;
			}
			else if (empty.BeginFIndex != StrokeFIndices.BeforeFirst)
			{
				empty.BeginFIndex = 1.0;
			}
		}
		if (empty.IsFull)
		{
			return empty;
		}
		if (empty.IsEmpty && (quad.IsEmpty || !HitTestQuadSegment(quad, hitBeginPoint, hitEndPoint)))
		{
			return empty;
		}
		if (empty.BeginFIndex != StrokeFIndices.BeforeFirst)
		{
			empty.BeginFIndex = ClipTest((endNode.Position - beginNode.Position) / beginNode.PressureFactor, endNode.PressureFactor / beginNode.PressureFactor - 1f, (hitBeginPoint - beginNode.Position) / beginNode.PressureFactor, (hitEndPoint - beginNode.Position) / beginNode.PressureFactor);
		}
		if (empty.EndFIndex != StrokeFIndices.AfterLast)
		{
			empty.EndFIndex = 1.0 - ClipTest((beginNode.Position - endNode.Position) / endNode.PressureFactor, beginNode.PressureFactor / endNode.PressureFactor - 1f, (hitBeginPoint - endNode.Position) / endNode.PressureFactor, (hitEndPoint - endNode.Position) / endNode.PressureFactor);
		}
		if (IsInvalidCutTestResult(empty))
		{
			return StrokeFIndices.Empty;
		}
		return empty;
	}

	internal virtual StrokeFIndices CutTest(in StrokeNodeData beginNode, in StrokeNodeData endNode, Quad quad, IEnumerable<ContourSegment> hitContour)
	{
		if (beginNode.IsEmpty)
		{
			if (HitTest(in beginNode, in endNode, quad, hitContour))
			{
				return StrokeFIndices.Full;
			}
			return StrokeFIndices.Empty;
		}
		StrokeFIndices result = StrokeFIndices.Empty;
		bool flag = true;
		Vector spineVector = (endNode.Position - beginNode.Position) / beginNode.PressureFactor;
		Vector spineVector2 = (beginNode.Position - endNode.Position) / endNode.PressureFactor;
		double pressureDelta = endNode.PressureFactor / beginNode.PressureFactor - 1f;
		double pressureDelta2 = beginNode.PressureFactor / endNode.PressureFactor - 1f;
		foreach (ContourSegment item in hitContour)
		{
			ContourSegment hitSegment = item;
			bool flag2 = HitTestStrokeNodes(in hitSegment, in beginNode, in endNode, ref result);
			if (result.IsFull)
			{
				return result;
			}
			if (!flag2)
			{
				if (!quad.IsEmpty)
				{
					flag2 = (hitSegment.IsArc ? HitTestQuadCircle(quad, hitSegment.Begin + hitSegment.Radius, hitSegment.Radius) : HitTestQuadSegment(quad, hitSegment.Begin, hitSegment.End));
				}
				if (!flag2)
				{
					if (flag)
					{
						flag = (hitSegment.IsArc ? (WhereIsVectorAboutArc(endNode.Position - hitSegment.Begin - hitSegment.Radius, -hitSegment.Radius, hitSegment.Vector - hitSegment.Radius) != HitResult.Hit) : (WhereIsVectorAboutVector(endNode.Position - hitSegment.Begin, hitSegment.Vector) == HitResult.Right));
					}
					continue;
				}
			}
			flag = false;
			if (!DoubleUtil.AreClose(result.BeginFIndex, StrokeFIndices.BeforeFirst))
			{
				double num = CalculateClipLocation(in hitSegment, in beginNode, spineVector, pressureDelta);
				if (num != StrokeFIndices.BeforeFirst && result.BeginFIndex > num)
				{
					result.BeginFIndex = num;
				}
			}
			if (DoubleUtil.AreClose(result.EndFIndex, StrokeFIndices.AfterLast))
			{
				continue;
			}
			double num2 = CalculateClipLocation(in hitSegment, in endNode, spineVector2, pressureDelta2);
			if (num2 != StrokeFIndices.BeforeFirst)
			{
				num2 = 1.0 - num2;
				if (result.EndFIndex < num2)
				{
					result.EndFIndex = num2;
				}
			}
		}
		if (DoubleUtil.AreClose(result.BeginFIndex, StrokeFIndices.AfterLast))
		{
			if (!DoubleUtil.AreClose(result.EndFIndex, StrokeFIndices.BeforeFirst))
			{
				result.BeginFIndex = StrokeFIndices.BeforeFirst;
			}
		}
		else if (DoubleUtil.AreClose(result.EndFIndex, StrokeFIndices.BeforeFirst))
		{
			result.EndFIndex = StrokeFIndices.AfterLast;
		}
		if (IsInvalidCutTestResult(result))
		{
			return StrokeFIndices.Empty;
		}
		if (!(result.IsEmpty && flag))
		{
			return result;
		}
		return StrokeFIndices.Full;
	}

	private double ClipTest(Vector spineVector, double pressureDelta, Vector hitBegin, Vector hitEnd)
	{
		double num = StrokeFIndices.AfterLast;
		Vector vector = hitEnd - hitBegin;
		Vector vector2 = _vertices[_vertices.Length - 1];
		Vector vector3 = spineVector + vector2 * pressureDelta;
		bool flag = false;
		int i = 0;
		for (int num2 = _vertices.Length; i < num2 || (i == num2 && flag); i++)
		{
			Vector vector4 = _vertices[i % num2];
			Vector vector5 = vector4 - vector2;
			Vector vector6 = spineVector + vector4 * pressureDelta;
			if ((DoubleUtil.IsZero(vector3.X) && DoubleUtil.IsZero(vector3.Y)) || (!flag && HitResult.Left != WhereIsVectorAboutVector(vector3, vector5)))
			{
				vector2 = vector4;
				vector3 = vector6;
				continue;
			}
			flag = false;
			HitResult hitResult = HitResult.Left;
			int num3 = 0;
			for (int j = 0; j < 2; j++)
			{
				Vector vector7 = ((j == 0) ? hitBegin : hitEnd) - vector2;
				hitResult = WhereIsVectorAboutVector(vector7, vector3);
				switch (hitResult)
				{
				case HitResult.Hit:
				{
					double num4 = ((Math.Abs(vector3.X) < Math.Abs(vector3.Y)) ? (vector7.Y / vector3.Y) : (vector7.X / vector3.X));
					if (num > num4 && DoubleUtil.IsBetweenZeroAndOne(num4))
					{
						num = num4;
					}
					break;
				}
				case HitResult.Right:
					num3++;
					if (HitResult.Left == WhereIsVectorAboutVector(vector7 - vector5, vector6))
					{
						double positionBetweenLines = GetPositionBetweenLines(vector5, vector3, vector7);
						if (num > positionBetweenLines && DoubleUtil.IsBetweenZeroAndOne(positionBetweenLines))
						{
							num = positionBetweenLines;
						}
					}
					else
					{
						flag = true;
					}
					break;
				default:
					num3--;
					break;
				}
			}
			if (num3 == 0)
			{
				if (hitResult == HitResult.Hit)
				{
					break;
				}
				double num5 = 0.0 - Vector.Determinant(vector3, vector);
				if (!DoubleUtil.IsZero(num5))
				{
					double num6 = Vector.Determinant(vector, hitBegin - vector2) / num5;
					if (num > num6 && DoubleUtil.IsBetweenZeroAndOne(num6))
					{
						num = num6;
					}
				}
			}
			vector2 = vector4;
			vector3 = vector6;
		}
		return AdjustFIndex(num);
	}

	private double ClipTestArc(Vector spineVector, double pressureDelta, Vector hitCenter, Vector hitRadius)
	{
		throw new NotImplementedException();
	}

	internal Vector[] GetVertices()
	{
		return _vertices;
	}

	private bool HitTestPolygonContourSegments(IEnumerable<ContourSegment> hitContour, in StrokeNodeData beginNode, in StrokeNodeData endNode)
	{
		bool flag = false;
		bool flag2 = true;
		Point position;
		double num;
		if (beginNode.IsEmpty || endNode.PressureFactor > beginNode.PressureFactor)
		{
			position = endNode.Position;
			num = endNode.PressureFactor;
		}
		else
		{
			position = beginNode.Position;
			num = beginNode.PressureFactor;
		}
		foreach (ContourSegment item in hitContour)
		{
			if (item.IsArc)
			{
				Vector center = item.Begin + item.Radius - position;
				Vector radius = item.Radius;
				if (!DoubleUtil.AreClose(num, 1.0))
				{
					center /= num;
					radius /= num;
				}
				if (HitTestPolygonCircle(_vertices, center, radius))
				{
					flag = true;
					break;
				}
				if (flag2 && WhereIsVectorAboutArc(position - item.Begin - item.Radius, -item.Radius, item.Vector - item.Radius) == HitResult.Hit)
				{
					flag2 = false;
				}
			}
			else
			{
				Vector vector = item.Begin - position;
				Vector hitEnd = vector + item.Vector;
				if (!DoubleUtil.AreClose(num, 1.0))
				{
					vector /= num;
					hitEnd /= num;
				}
				if (HitTestPolygonSegment(_vertices, vector, hitEnd))
				{
					flag = true;
					break;
				}
				if (flag2 && WhereIsVectorAboutVector(position - item.Begin, item.Vector) != HitResult.Right)
				{
					flag2 = false;
				}
			}
		}
		return flag2 || flag;
	}

	private bool HitTestInkContour(IEnumerable<ContourSegment> hitContour, Quad quad, in StrokeNodeData beginNode, in StrokeNodeData endNode)
	{
		bool flag = false;
		bool flag2 = true;
		foreach (ContourSegment item in hitContour)
		{
			Vector vector;
			Vector vector2;
			HitResult hitResult;
			if (item.IsArc)
			{
				vector = item.Begin + item.Radius - beginNode.Position;
				vector2 = item.Radius;
				hitResult = WhereIsCircleAboutSegment(vector, vector2, quad.C - beginNode.Position, quad.D - beginNode.Position);
			}
			else
			{
				vector = item.Begin - beginNode.Position;
				vector2 = vector + item.Vector;
				hitResult = WhereIsSegmentAboutSegment(vector, vector2, quad.C - beginNode.Position, quad.D - beginNode.Position);
			}
			if (HitResult.Left == hitResult)
			{
				if (flag2)
				{
					flag2 = (item.IsArc ? (WhereIsVectorAboutArc(-vector, -item.Radius, item.Vector - item.Radius) != HitResult.Hit) : (WhereIsVectorAboutVector(-vector, item.Vector) == HitResult.Right));
				}
				continue;
			}
			HitResult hitResult2 = hitResult;
			HitResult prevHitResult = hitResult;
			double num = beginNode.PressureFactor;
			int num2 = 0;
			int num3 = _vertices.Length;
			Vector vector3 = default(Vector);
			for (num2 = 0; num2 < num3; num2++)
			{
				vector3 = _vertices[num2] * num;
				if (DoubleUtil.AreClose(beginNode.Position + vector3, quad.D))
				{
					break;
				}
			}
			int i;
			for (i = 0; i < 2; i++)
			{
				num3 = _vertices.Length;
				Point point = ((i == 0) ? beginNode.Position : endNode.Position);
				Point point2 = ((i == 0) ? quad.A : quad.C);
				while (point + vector3 != point2 && num3 != 0)
				{
					num2 = (num2 + 1) % _vertices.Length;
					Vector vector4 = _vertices[num2] * num;
					hitResult = (item.IsArc ? WhereIsCircleAboutSegment(vector, vector2, vector3, vector4) : WhereIsSegmentAboutSegment(vector, vector2, vector3, vector4));
					if (hitResult == HitResult.Hit)
					{
						return true;
					}
					if (IsOutside(hitResult, prevHitResult))
					{
						i = 3;
						break;
					}
					prevHitResult = hitResult;
					vector3 = vector4;
					num3--;
				}
				if (i == 0)
				{
					num = endNode.PressureFactor;
					Vector vector5 = endNode.Position - beginNode.Position;
					vector3 -= vector5;
					vector -= vector5;
					if (!item.IsArc)
					{
						vector2 -= vector5;
					}
					num3 = _vertices.Length;
					while (!DoubleUtil.AreClose(endNode.Position + _vertices[num2] * num, quad.B) && num3 != 0)
					{
						num2 = (num2 + 1) % _vertices.Length;
						num3--;
					}
					num2--;
				}
			}
			if (i == 2 && !IsOutside(hitResult2, hitResult))
			{
				flag = true;
				break;
			}
			if (flag2)
			{
				flag2 = (item.IsArc ? (WhereIsVectorAboutArc(-vector, -item.Radius, item.Vector - item.Radius) != HitResult.Hit) : (WhereIsVectorAboutVector(-vector, item.Vector) == HitResult.Right));
			}
		}
		return flag || flag2;
	}

	private bool HitTestStrokeNodes(in ContourSegment hitSegment, in StrokeNodeData beginNode, in StrokeNodeData endNode, ref StrokeFIndices result)
	{
		bool flag = false;
		for (int i = 0; i < 2; i++)
		{
			Point position;
			double num;
			if (i == 0)
			{
				if (flag && DoubleUtil.AreClose(result.BeginFIndex, StrokeFIndices.BeforeFirst))
				{
					continue;
				}
				position = beginNode.Position;
				num = beginNode.PressureFactor;
			}
			else
			{
				if (flag && DoubleUtil.AreClose(result.EndFIndex, StrokeFIndices.AfterLast))
				{
					continue;
				}
				position = endNode.Position;
				num = endNode.PressureFactor;
			}
			Vector vector;
			Vector vector2;
			if (hitSegment.IsArc)
			{
				vector = hitSegment.Begin - position + hitSegment.Radius;
				vector2 = hitSegment.Radius;
			}
			else
			{
				vector = hitSegment.Begin - position;
				vector2 = vector + hitSegment.Vector;
			}
			if (num != 1.0)
			{
				vector /= num;
				vector2 /= num;
			}
			if (!(hitSegment.IsArc ? HitTestPolygonCircle(_vertices, vector, vector2) : HitTestPolygonSegment(_vertices, vector, vector2)))
			{
				continue;
			}
			flag = true;
			if (i == 0)
			{
				result.BeginFIndex = StrokeFIndices.BeforeFirst;
				if (DoubleUtil.AreClose(result.EndFIndex, StrokeFIndices.AfterLast))
				{
					break;
				}
				continue;
			}
			result.EndFIndex = StrokeFIndices.AfterLast;
			if (beginNode.IsEmpty)
			{
				result.BeginFIndex = StrokeFIndices.BeforeFirst;
				break;
			}
			if (DoubleUtil.AreClose(result.BeginFIndex, StrokeFIndices.BeforeFirst))
			{
				break;
			}
		}
		return flag;
	}

	private double CalculateClipLocation(in ContourSegment hitSegment, in StrokeNodeData beginNode, Vector spineVector, double pressureDelta)
	{
		double num = StrokeFIndices.BeforeFirst;
		if (hitSegment.IsArc || WhereIsVectorAboutVector(beginNode.Position - hitSegment.Begin, hitSegment.Vector) == HitResult.Left)
		{
			num = (hitSegment.IsArc ? ClipTestArc(spineVector, pressureDelta, (hitSegment.Begin + hitSegment.Radius - beginNode.Position) / beginNode.PressureFactor, hitSegment.Radius / beginNode.PressureFactor) : ClipTest(spineVector, pressureDelta, (hitSegment.Begin - beginNode.Position) / beginNode.PressureFactor, (hitSegment.End - beginNode.Position) / beginNode.PressureFactor));
			if (num == StrokeFIndices.AfterLast)
			{
				num = StrokeFIndices.BeforeFirst;
			}
		}
		return num;
	}

	protected bool IsInvalidCutTestResult(StrokeFIndices result)
	{
		if (DoubleUtil.AreClose(result.BeginFIndex, result.EndFIndex) || (DoubleUtil.AreClose(result.BeginFIndex, StrokeFIndices.BeforeFirst) && result.EndFIndex < 0.0) || (result.BeginFIndex > 1.0 && DoubleUtil.AreClose(result.EndFIndex, StrokeFIndices.AfterLast)))
		{
			return true;
		}
		return false;
	}

	internal static bool HitTestPolygonSegment(Vector[] vertices, Vector hitBegin, Vector hitEnd)
	{
		HitResult hitResult = HitResult.Right;
		HitResult hitResult2 = HitResult.Right;
		HitResult prevHitResult = HitResult.Right;
		int num = vertices.Length;
		Vector orgBegin = vertices[num - 1];
		for (int i = 0; i < num; i++)
		{
			Vector vector = vertices[i];
			hitResult = WhereIsSegmentAboutSegment(hitBegin, hitEnd, orgBegin, vector);
			if (hitResult == HitResult.Hit)
			{
				return true;
			}
			if (IsOutside(hitResult, prevHitResult))
			{
				return false;
			}
			if (i == 0)
			{
				hitResult2 = hitResult;
			}
			prevHitResult = hitResult;
			orgBegin = vector;
		}
		return !IsOutside(hitResult2, hitResult);
	}

	internal static bool HitTestQuadSegment(Quad quad, Point hitBegin, Point hitEnd)
	{
		HitResult hitResult = HitResult.Right;
		HitResult hitResult2 = HitResult.Right;
		HitResult prevHitResult = HitResult.Right;
		int num = 4;
		Vector hitBegin2 = new Vector(0.0, 0.0);
		Vector hitEnd2 = hitEnd - hitBegin;
		Vector orgBegin = quad[num - 1] - hitBegin;
		for (int i = 0; i < num; i++)
		{
			Vector vector = quad[i] - hitBegin;
			hitResult = WhereIsSegmentAboutSegment(hitBegin2, hitEnd2, orgBegin, vector);
			if (hitResult == HitResult.Hit)
			{
				return true;
			}
			if (IsOutside(hitResult, prevHitResult))
			{
				return false;
			}
			if (i == 0)
			{
				hitResult2 = hitResult;
			}
			prevHitResult = hitResult;
			orgBegin = vector;
		}
		return !IsOutside(hitResult2, hitResult);
	}

	internal static bool HitTestPolygonCircle(Vector[] vertices, Vector center, Vector radius)
	{
		throw new NotImplementedException();
	}

	internal static bool HitTestQuadCircle(Quad quad, Point center, Vector radius)
	{
		throw new NotImplementedException();
	}

	internal static HitResult WhereIsSegmentAboutSegment(Vector hitBegin, Vector hitEnd, Vector orgBegin, Vector orgEnd)
	{
		if (hitEnd == hitBegin)
		{
			return WhereIsCircleAboutSegment(hitBegin, new Vector(0.0, 0.0), orgBegin, orgEnd);
		}
		HitResult result = HitResult.Right;
		Vector vector = orgEnd - orgBegin;
		Vector vector2 = orgBegin - hitBegin;
		Vector vector3 = hitEnd - hitBegin;
		double num = Vector.Determinant(vector, vector3);
		if (DoubleUtil.IsZero(num))
		{
			if (DoubleUtil.IsZero(Vector.Determinant(vector3, vector2)) || DoubleUtil.GreaterThan(Vector.Determinant(vector, vector2), 0.0))
			{
				result = HitResult.Left;
			}
		}
		else
		{
			double num2 = AdjustFIndex(Vector.Determinant(vector, vector2) / num);
			if (num2 > 0.0 && num2 < 1.0)
			{
				double num3 = AdjustFIndex(Vector.Determinant(vector3, vector2) / num);
				result = ((!(num3 > 0.0) || !(num3 < 1.0)) ? ((0.0 < num3) ? HitResult.InFront : HitResult.Behind) : HitResult.Hit);
			}
			else if (WhereIsVectorAboutVector(hitBegin - orgBegin, vector) == HitResult.Left || WhereIsVectorAboutVector(hitEnd - orgBegin, vector) == HitResult.Left)
			{
				result = HitResult.Left;
			}
		}
		return result;
	}

	internal static HitResult WhereIsCircleAboutSegment(Vector center, Vector radius, Vector segBegin, Vector segEnd)
	{
		segBegin -= center;
		segEnd -= center;
		double lengthSquared = radius.LengthSquared;
		double lengthSquared2 = GetNearest(segBegin, segEnd).LengthSquared;
		if (lengthSquared > lengthSquared2)
		{
			return HitResult.Hit;
		}
		Vector vector = segEnd - segBegin;
		HitResult hitResult = HitResult.Right;
		HitResult hitResult2 = WhereIsVectorAboutVector(-segBegin, vector);
		if (hitResult2 == HitResult.Hit)
		{
			return DoubleUtil.LessThan(segBegin.LengthSquared, segEnd.LengthSquared) ? HitResult.InFront : HitResult.Behind;
		}
		double projectionFIndex = GetProjectionFIndex(segBegin, segEnd);
		lengthSquared2 = (segBegin + vector * projectionFIndex).LengthSquared;
		if (lengthSquared <= lengthSquared2)
		{
			return hitResult2;
		}
		return (projectionFIndex > 0.0) ? HitResult.InFront : HitResult.Behind;
	}

	internal static HitResult WhereIsVectorAboutVector(Vector vector1, Vector vector2)
	{
		double num = Vector.Determinant(vector1, vector2);
		if (DoubleUtil.IsZero(num))
		{
			return HitResult.Hit;
		}
		if (!(0.0 < num))
		{
			return HitResult.Right;
		}
		return HitResult.Left;
	}

	internal static HitResult WhereIsVectorAboutArc(Vector hitVector, Vector arcBegin, Vector arcEnd)
	{
		if (arcBegin == arcEnd)
		{
			return HitResult.Hit;
		}
		if (HitResult.Right == WhereIsVectorAboutVector(arcEnd, arcBegin))
		{
			if (HitResult.Left != WhereIsVectorAboutVector(hitVector, arcBegin) && HitResult.Right != WhereIsVectorAboutVector(hitVector, arcEnd))
			{
				return HitResult.Hit;
			}
		}
		else if (HitResult.Left != WhereIsVectorAboutVector(hitVector, arcBegin) || HitResult.Right != WhereIsVectorAboutVector(hitVector, arcEnd))
		{
			return HitResult.Hit;
		}
		if (WhereIsVectorAboutVector(hitVector - arcBegin, TurnLeft(arcBegin)) != HitResult.Left || WhereIsVectorAboutVector(hitVector - arcEnd, TurnRight(arcEnd)) != HitResult.Right)
		{
			return HitResult.Left;
		}
		return HitResult.Right;
	}

	internal static Vector TurnLeft(Vector vector)
	{
		throw new NotImplementedException();
	}

	internal static Vector TurnRight(Vector vector)
	{
		throw new NotImplementedException();
	}

	internal static bool IsOutside(HitResult hitResult, HitResult prevHitResult)
	{
		if (HitResult.Left != hitResult)
		{
			if (HitResult.Behind == hitResult)
			{
				return HitResult.InFront == prevHitResult;
			}
			return false;
		}
		return true;
	}

	internal static double GetPositionBetweenLines(Vector linesVector, Vector nextLine, Vector hitPoint)
	{
		Vector projection = GetProjection(-hitPoint, linesVector - hitPoint);
		hitPoint = nextLine - hitPoint;
		Vector projection2 = GetProjection(hitPoint, hitPoint + linesVector);
		Vector vector = projection - projection2;
		return Math.Sqrt(projection.LengthSquared / vector.LengthSquared);
	}

	internal static double GetProjectionFIndex(Vector begin, Vector end)
	{
		Vector vector = end - begin;
		double lengthSquared = vector.LengthSquared;
		if (DoubleUtil.IsZero(lengthSquared))
		{
			return 0.0;
		}
		return AdjustFIndex((0.0 - begin * vector) / lengthSquared);
	}

	internal static Vector GetProjection(Vector begin, Vector end)
	{
		double projectionFIndex = GetProjectionFIndex(begin, end);
		return begin + (end - begin) * projectionFIndex;
	}

	internal static Vector GetNearest(Vector begin, Vector end)
	{
		double projectionFIndex = GetProjectionFIndex(begin, end);
		if (projectionFIndex <= 0.0)
		{
			return begin;
		}
		if (projectionFIndex >= 1.0)
		{
			return end;
		}
		return begin + (end - begin) * projectionFIndex;
	}

	internal static double AdjustFIndex(double findex)
	{
		if (!DoubleUtil.IsZero(findex))
		{
			if (!DoubleUtil.IsOne(findex))
			{
				return findex;
			}
			return 1.0;
		}
		return 0.0;
	}
}
