using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;

namespace MS.Internal.Ink;

internal class EllipticalNodeOperations : StrokeNodeOperations
{
	private double _radius;

	private Size _radii;

	private Matrix _transform;

	private Matrix _nodeShapeToCircle;

	private Matrix _circleToNodeShape;

	internal override bool IsNodeShapeEllipse => true;

	internal EllipticalNodeOperations(StylusShape nodeShape)
		: base(nodeShape)
	{
		_radii = new Size(nodeShape.Width * 0.5, nodeShape.Height * 0.5);
		_radius = Math.Max(_radii.Width, _radii.Height);
		_transform = nodeShape.Transform;
		_nodeShapeToCircle = _transform;
		_nodeShapeToCircle.Invert();
		if (DoubleUtil.AreClose(_radii.Width, _radii.Height))
		{
			_circleToNodeShape = _transform;
			return;
		}
		if (!DoubleUtil.IsZero(nodeShape.Rotation))
		{
			_nodeShapeToCircle.Rotate(0.0 - nodeShape.Rotation);
		}
		double scaleX;
		double scaleY;
		if (_radii.Width > _radii.Height)
		{
			scaleX = 1.0;
			scaleY = _radii.Width / _radii.Height;
		}
		else
		{
			scaleX = _radii.Height / _radii.Width;
			scaleY = 1.0;
		}
		_nodeShapeToCircle.Scale(scaleX, scaleY);
		_circleToNodeShape = _nodeShapeToCircle;
		_circleToNodeShape.Invert();
	}

	internal override Quad GetConnectingQuad(in StrokeNodeData beginNode, in StrokeNodeData endNode)
	{
		if (beginNode.IsEmpty || endNode.IsEmpty || DoubleUtil.AreClose(beginNode.Position, endNode.Position))
		{
			return Quad.Empty;
		}
		Vector vector = endNode.Position - beginNode.Position;
		if (!_nodeShapeToCircle.IsIdentity)
		{
			vector = _nodeShapeToCircle.Transform(vector);
		}
		double num = _radius * (double)beginNode.PressureFactor;
		double num2 = _radius * (double)endNode.PressureFactor;
		double lengthSquared = vector.LengthSquared;
		double num3 = num2 - num;
		double num4 = (DoubleUtil.IsZero(num3) ? 0.0 : (num3 * num3));
		if (DoubleUtil.LessThanOrClose(lengthSquared, num4))
		{
			return Quad.Empty;
		}
		double num5 = Math.Sqrt(lengthSquared);
		vector /= num5;
		Vector vector2 = vector;
		double y = vector2.Y;
		vector2.Y = 0.0 - vector2.X;
		vector2.X = y;
		double num6 = num4 / lengthSquared;
		Vector vector3;
		Vector vector4;
		if (DoubleUtil.IsZero(num6))
		{
			vector3 = vector2;
			vector4 = -vector2;
		}
		else
		{
			vector2 *= Math.Sqrt(1.0 - num6);
			vector *= Math.Sqrt(num6);
			if (beginNode.PressureFactor < endNode.PressureFactor)
			{
				vector = -vector;
			}
			vector3 = vector + vector2;
			vector4 = vector - vector2;
		}
		if (!_circleToNodeShape.IsIdentity)
		{
			vector3 = _circleToNodeShape.Transform(vector3);
			vector4 = _circleToNodeShape.Transform(vector4);
		}
		return new Quad(beginNode.Position + vector3 * num, endNode.Position + vector3 * num2, endNode.Position + vector4 * num2, beginNode.Position + vector4 * num);
	}

	internal override IEnumerable<ContourSegment> GetContourSegments(StrokeNodeData node, Quad quad)
	{
		if (quad.IsEmpty)
		{
			Point position = node.Position;
			position.X += _radius;
			yield return new ContourSegment(position, position, node.Position);
		}
		else if (_nodeShapeToCircle.IsIdentity)
		{
			yield return new ContourSegment(quad.A, quad.B);
			yield return new ContourSegment(quad.B, quad.C, node.Position);
			yield return new ContourSegment(quad.C, quad.D);
			yield return new ContourSegment(quad.D, quad.A);
		}
	}

	internal override IEnumerable<ContourSegment> GetNonBezierContourSegments(StrokeNodeData beginNode, StrokeNodeData endNode)
	{
		Quad quad = (beginNode.IsEmpty ? Quad.Empty : base.GetConnectingQuad(in beginNode, in endNode));
		return base.GetContourSegments(endNode, quad);
	}

	internal override bool HitTest(in StrokeNodeData beginNode, in StrokeNodeData endNode, Quad quad, Point hitBeginPoint, Point hitEndPoint)
	{
		StrokeNodeData strokeNodeData;
		StrokeNodeData strokeNodeData2;
		if (beginNode.IsEmpty || (quad.IsEmpty && endNode.PressureFactor > beginNode.PressureFactor))
		{
			strokeNodeData = endNode;
			strokeNodeData2 = StrokeNodeData.Empty;
		}
		else
		{
			strokeNodeData = beginNode;
			strokeNodeData2 = endNode;
		}
		Vector vector = hitBeginPoint - strokeNodeData.Position;
		Vector vector2 = hitEndPoint - strokeNodeData.Position;
		if (!_nodeShapeToCircle.IsIdentity)
		{
			vector = _nodeShapeToCircle.Transform(vector);
			vector2 = _nodeShapeToCircle.Transform(vector2);
		}
		bool result = false;
		double num = _radius * (double)strokeNodeData.PressureFactor;
		if (StrokeNodeOperations.GetNearest(vector, vector2).LengthSquared <= num * num)
		{
			result = true;
		}
		else if (!quad.IsEmpty)
		{
			Vector vector3 = strokeNodeData2.Position - strokeNodeData.Position;
			if (!_nodeShapeToCircle.IsIdentity)
			{
				vector3 = _nodeShapeToCircle.Transform(vector3);
			}
			double num2 = _radius * (double)strokeNodeData2.PressureFactor;
			if (StrokeNodeOperations.GetNearest(vector - vector3, vector2 - vector3).LengthSquared <= num2 * num2 || StrokeNodeOperations.HitTestQuadSegment(quad, hitBeginPoint, hitEndPoint))
			{
				result = true;
			}
		}
		return result;
	}

	internal override bool HitTest(in StrokeNodeData beginNode, in StrokeNodeData endNode, Quad quad, IEnumerable<ContourSegment> hitContour)
	{
		double num = 0.0;
		StrokeNodeData strokeNodeData;
		Vector vector;
		if (beginNode.IsEmpty || (quad.IsEmpty && endNode.PressureFactor > beginNode.PressureFactor))
		{
			strokeNodeData = endNode;
			StrokeNodeData empty = StrokeNodeData.Empty;
			vector = default(Vector);
		}
		else
		{
			strokeNodeData = beginNode;
			StrokeNodeData empty = endNode;
			num = _radius * (double)empty.PressureFactor;
			num *= num;
			vector = empty.Position - strokeNodeData.Position;
			if (!_nodeShapeToCircle.IsIdentity)
			{
				vector = _nodeShapeToCircle.Transform(vector);
			}
		}
		double num2 = _radius * (double)strokeNodeData.PressureFactor;
		num2 *= num2;
		bool flag = false;
		bool flag2 = true;
		foreach (ContourSegment item in hitContour)
		{
			if (!item.IsArc)
			{
				Vector vector2 = item.Begin - strokeNodeData.Position;
				Vector vector3 = vector2 + item.Vector;
				if (!_nodeShapeToCircle.IsIdentity)
				{
					vector2 = _nodeShapeToCircle.Transform(vector2);
					vector3 = _nodeShapeToCircle.Transform(vector3);
				}
				if (StrokeNodeOperations.GetNearest(vector2, vector3).LengthSquared <= num2)
				{
					flag = true;
					break;
				}
				if (!quad.IsEmpty && (StrokeNodeOperations.GetNearest(vector2 - vector, vector3 - vector).LengthSquared <= num || StrokeNodeOperations.HitTestQuadSegment(quad, item.Begin, item.End)))
				{
					flag = true;
					break;
				}
				if (flag2 && StrokeNodeOperations.WhereIsVectorAboutVector(endNode.Position - item.Begin, item.Vector) != HitResult.Right)
				{
					flag2 = false;
				}
			}
		}
		return flag || flag2;
	}

	internal override StrokeFIndices CutTest(in StrokeNodeData beginNode, in StrokeNodeData endNode, Quad quad, Point hitBeginPoint, Point hitEndPoint)
	{
		Vector vector = (beginNode.IsEmpty ? new Vector(0.0, 0.0) : (beginNode.Position - endNode.Position));
		Vector vector2 = hitBeginPoint - endNode.Position;
		Vector vector3 = hitEndPoint - endNode.Position;
		if (!_nodeShapeToCircle.IsIdentity)
		{
			vector = _nodeShapeToCircle.Transform(vector);
			vector2 = _nodeShapeToCircle.Transform(vector2);
			vector3 = _nodeShapeToCircle.Transform(vector3);
		}
		StrokeFIndices empty = StrokeFIndices.Empty;
		double num = 0.0;
		double num2 = _radius * (double)endNode.PressureFactor;
		if (StrokeNodeOperations.GetNearest(vector2, vector3).LengthSquared <= num2 * num2)
		{
			empty.EndFIndex = StrokeFIndices.AfterLast;
			empty.BeginFIndex = (beginNode.IsEmpty ? StrokeFIndices.BeforeFirst : 1.0);
		}
		if (!beginNode.IsEmpty)
		{
			num = _radius * (double)beginNode.PressureFactor;
			if (StrokeNodeOperations.GetNearest(vector2 - vector, vector3 - vector).LengthSquared <= num * num)
			{
				empty.BeginFIndex = StrokeFIndices.BeforeFirst;
				if (!DoubleUtil.AreClose(empty.EndFIndex, StrokeFIndices.AfterLast))
				{
					empty.EndFIndex = 0.0;
				}
			}
		}
		if (empty.IsFull || quad.IsEmpty || (empty.IsEmpty && !StrokeNodeOperations.HitTestQuadSegment(quad, hitBeginPoint, hitEndPoint)))
		{
			return empty;
		}
		if (!DoubleUtil.AreClose(empty.BeginFIndex, StrokeFIndices.BeforeFirst))
		{
			empty.BeginFIndex = ClipTest(-vector, num, num2, vector2 - vector, vector3 - vector);
		}
		if (!DoubleUtil.AreClose(empty.EndFIndex, StrokeFIndices.AfterLast))
		{
			empty.EndFIndex = 1.0 - ClipTest(vector, num2, num, vector2, vector3);
		}
		if (IsInvalidCutTestResult(empty))
		{
			return StrokeFIndices.Empty;
		}
		return empty;
	}

	internal override StrokeFIndices CutTest(in StrokeNodeData beginNode, in StrokeNodeData endNode, Quad quad, IEnumerable<ContourSegment> hitContour)
	{
		Vector vector = (beginNode.IsEmpty ? new Vector(0.0, 0.0) : (beginNode.Position - endNode.Position));
		if (!_nodeShapeToCircle.IsIdentity)
		{
			vector = _nodeShapeToCircle.Transform(vector);
		}
		double num = 0.0;
		double num2 = 0.0;
		double num3 = _radius * (double)endNode.PressureFactor;
		double num4 = num3 * num3;
		if (!beginNode.IsEmpty)
		{
			num = _radius * (double)beginNode.PressureFactor;
			num2 = num * num;
		}
		bool flag = true;
		StrokeFIndices result = StrokeFIndices.Empty;
		foreach (ContourSegment item in hitContour)
		{
			if (item.IsArc)
			{
				continue;
			}
			Vector vector2 = item.Begin - endNode.Position;
			Vector vector3 = vector2 + item.Vector;
			if (!_nodeShapeToCircle.IsIdentity)
			{
				vector2 = _nodeShapeToCircle.Transform(vector2);
				vector3 = _nodeShapeToCircle.Transform(vector3);
			}
			bool flag2 = false;
			if (StrokeNodeOperations.GetNearest(vector2, vector3).LengthSquared < num4)
			{
				flag2 = true;
				if (!DoubleUtil.AreClose(result.EndFIndex, StrokeFIndices.AfterLast))
				{
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
			}
			if (!beginNode.IsEmpty && (!flag2 || !DoubleUtil.AreClose(result.BeginFIndex, StrokeFIndices.BeforeFirst)) && StrokeNodeOperations.GetNearest(vector2 - vector, vector3 - vector).LengthSquared < num2)
			{
				flag2 = true;
				if (!DoubleUtil.AreClose(result.BeginFIndex, StrokeFIndices.BeforeFirst))
				{
					result.BeginFIndex = StrokeFIndices.BeforeFirst;
					if (DoubleUtil.AreClose(result.EndFIndex, StrokeFIndices.AfterLast))
					{
						break;
					}
				}
			}
			if (beginNode.IsEmpty || (!flag2 && (quad.IsEmpty || !StrokeNodeOperations.HitTestQuadSegment(quad, item.Begin, item.End))))
			{
				if (flag && StrokeNodeOperations.WhereIsVectorAboutVector(endNode.Position - item.Begin, item.Vector) != HitResult.Right)
				{
					flag = false;
				}
				continue;
			}
			flag = false;
			CalculateCutLocations(vector, vector2, vector3, num3, num, ref result);
			if (!result.IsFull)
			{
				continue;
			}
			break;
		}
		if (!result.IsFull)
		{
			if (flag)
			{
				result = StrokeFIndices.Full;
			}
			else if (DoubleUtil.AreClose(result.EndFIndex, StrokeFIndices.BeforeFirst) && !DoubleUtil.AreClose(result.BeginFIndex, StrokeFIndices.AfterLast))
			{
				result.EndFIndex = StrokeFIndices.AfterLast;
			}
			else if (DoubleUtil.AreClose(result.BeginFIndex, StrokeFIndices.AfterLast) && !DoubleUtil.AreClose(result.EndFIndex, StrokeFIndices.BeforeFirst))
			{
				result.BeginFIndex = StrokeFIndices.BeforeFirst;
			}
		}
		if (IsInvalidCutTestResult(result))
		{
			return StrokeFIndices.Empty;
		}
		return result;
	}

	private static double ClipTest(Vector spineVector, double beginRadius, double endRadius, Vector hitBegin, Vector hitEnd)
	{
		if (DoubleUtil.IsZero(spineVector.X) && DoubleUtil.IsZero(spineVector.Y))
		{
			Vector nearest = StrokeNodeOperations.GetNearest(hitBegin, hitEnd);
			double num = ((nearest.X == 0.0) ? Math.Abs(nearest.Y) : ((nearest.Y != 0.0) ? nearest.Length : Math.Abs(nearest.X)));
			return StrokeNodeOperations.AdjustFIndex((num - beginRadius) / (endRadius - beginRadius));
		}
		if (DoubleUtil.AreClose(hitBegin, hitEnd))
		{
			return ClipTest(spineVector, beginRadius, endRadius, hitBegin);
		}
		Vector vector = hitEnd - hitBegin;
		double num2;
		if (DoubleUtil.IsZero(Vector.Determinant(spineVector, vector)))
		{
			num2 = ClipTest(spineVector, beginRadius, endRadius, StrokeNodeOperations.GetNearest(hitBegin, hitEnd));
		}
		else
		{
			double projectionFIndex = StrokeNodeOperations.GetProjectionFIndex(hitBegin, hitEnd);
			Vector vector2 = hitBegin + vector * projectionFIndex;
			if (vector2.LengthSquared < beginRadius * beginRadius)
			{
				num2 = ClipTest(spineVector, beginRadius, endRadius, (0.0 > projectionFIndex) ? hitBegin : hitEnd);
			}
			else
			{
				Vector vector3 = spineVector + StrokeNodeOperations.GetProjection(-spineVector, vector2 - spineVector);
				if (DoubleUtil.IsZero(vector3.LengthSquared) || DoubleUtil.IsZero(endRadius - beginRadius + vector3.Length))
				{
					return 1.0;
				}
				num2 = (vector2.Length - beginRadius) / (endRadius - beginRadius + vector3.Length);
				Vector vector4 = spineVector * num2;
				double projectionFIndex2 = StrokeNodeOperations.GetProjectionFIndex(hitBegin - vector4, hitEnd - vector4);
				if (!DoubleUtil.IsBetweenZeroAndOne(projectionFIndex2))
				{
					num2 = ClipTest(spineVector, beginRadius, endRadius, (0.0 > projectionFIndex2) ? hitBegin : hitEnd);
				}
			}
		}
		return StrokeNodeOperations.AdjustFIndex(num2);
	}

	private static double ClipTest(Vector spine, double beginRadius, double endRadius, Vector hit)
	{
		double num = endRadius - beginRadius;
		double num2 = spine.X * spine.X + spine.Y * spine.Y - num * num;
		double num3 = -2.0 * (hit.X * spine.X + hit.Y * spine.Y + beginRadius * num);
		double num4 = hit.X * hit.X + hit.Y * hit.Y - beginRadius * beginRadius;
		if (DoubleUtil.IsZero(num2) || !DoubleUtil.GreaterThanOrClose(num3 * num3, 4.0 * num2 * num4))
		{
			return 1.0;
		}
		double num5 = Math.Sqrt(num3 * num3 - 4.0 * num2 * num4);
		double num6 = (0.0 - num3 + num5) / (2.0 * num2);
		double num7 = (0.0 - num3 - num5) / (2.0 * num2);
		double findex = ((DoubleUtil.IsBetweenZeroAndOne(num6) && DoubleUtil.IsBetweenZeroAndOne(num6)) ? Math.Min(num6, num7) : (DoubleUtil.IsBetweenZeroAndOne(num6) ? num6 : (DoubleUtil.IsBetweenZeroAndOne(num7) ? num7 : ((num6 > 1.0 && num7 > 1.0) ? 1.0 : ((!(num6 < 0.0) || !(num7 < 0.0)) ? ((Math.Abs(Math.Min(num6, num7) - 0.0) < Math.Abs(Math.Max(num6, num7) - 1.0)) ? 0.0 : 1.0) : 0.0)))));
		return StrokeNodeOperations.AdjustFIndex(findex);
	}

	private static HitResult WhereIsNodeAboutSegment(Vector spine, Vector segBegin, Vector segEnd)
	{
		HitResult result = HitResult.Right;
		Vector vector = segEnd - segBegin;
		if (StrokeNodeOperations.WhereIsVectorAboutVector(-segBegin, vector) == HitResult.Left && !DoubleUtil.IsZero(Vector.Determinant(spine, vector)))
		{
			result = HitResult.Left;
		}
		return result;
	}

	private void CalculateCutLocations(Vector spineVector, Vector hitBegin, Vector hitEnd, double endRadius, double beginRadius, ref StrokeFIndices result)
	{
		if (!DoubleUtil.AreClose(result.EndFIndex, StrokeFIndices.AfterLast) && WhereIsNodeAboutSegment(spineVector, hitBegin, hitEnd) == HitResult.Left)
		{
			double num = 1.0 - ClipTest(spineVector, endRadius, beginRadius, hitBegin, hitEnd);
			if (num > result.EndFIndex)
			{
				result.EndFIndex = num;
			}
		}
		if (DoubleUtil.AreClose(result.BeginFIndex, StrokeFIndices.BeforeFirst))
		{
			return;
		}
		hitBegin -= spineVector;
		hitEnd -= spineVector;
		if (WhereIsNodeAboutSegment(-spineVector, hitBegin, hitEnd) == HitResult.Left)
		{
			double num2 = ClipTest(-spineVector, beginRadius, endRadius, hitBegin, hitEnd);
			if (num2 < result.BeginFIndex)
			{
				result.BeginFIndex = num2;
			}
		}
	}
}
