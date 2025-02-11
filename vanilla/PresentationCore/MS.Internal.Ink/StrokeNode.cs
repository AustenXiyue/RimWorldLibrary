using System;
using System.Collections.Generic;
using System.Windows;

namespace MS.Internal.Ink;

internal struct StrokeNode
{
	private StrokeNodeOperations _operations;

	private int _index;

	private StrokeNodeData _thisNode;

	private StrokeNodeData _lastNode;

	private bool _isQuadCached;

	private Quad _connectingQuad;

	private bool _isLastNode;

	internal Point Position => _thisNode.Position;

	internal Point PreviousPosition => _lastNode.Position;

	internal float PressureFactor => _thisNode.PressureFactor;

	internal float PreviousPressureFactor => _lastNode.PressureFactor;

	internal bool IsEllipse
	{
		get
		{
			if (IsValid)
			{
				return _operations.IsNodeShapeEllipse;
			}
			return false;
		}
	}

	internal bool IsLastNode => _isLastNode;

	internal int Index => _index;

	internal bool IsValid => _operations != null;

	private Quad ConnectingQuad
	{
		get
		{
			if (!_isQuadCached)
			{
				_connectingQuad = _operations.GetConnectingQuad(in _lastNode, in _thisNode);
				_isQuadCached = true;
			}
			return _connectingQuad;
		}
	}

	internal StrokeNode(StrokeNodeOperations operations, int index, in StrokeNodeData nodeData, in StrokeNodeData lastNodeData, bool isLastNode)
	{
		_operations = operations;
		_index = index;
		_thisNode = nodeData;
		_lastNode = lastNodeData;
		_isQuadCached = lastNodeData.IsEmpty;
		_connectingQuad = Quad.Empty;
		_isLastNode = isLastNode;
	}

	internal Rect GetBounds()
	{
		if (!IsValid)
		{
			return Rect.Empty;
		}
		return _operations.GetNodeBounds(in _thisNode);
	}

	internal Rect GetBoundsConnected()
	{
		if (!IsValid)
		{
			return Rect.Empty;
		}
		return Rect.Union(_operations.GetNodeBounds(in _thisNode), ConnectingQuad.Bounds);
	}

	internal void GetContourPoints(List<Point> pointBuffer)
	{
		if (IsValid)
		{
			_operations.GetNodeContourPoints(in _thisNode, pointBuffer);
		}
	}

	internal void GetPreviousContourPoints(List<Point> pointBuffer)
	{
		if (IsValid)
		{
			_operations.GetNodeContourPoints(in _lastNode, pointBuffer);
		}
	}

	internal Quad GetConnectingQuad()
	{
		if (IsValid)
		{
			return ConnectingQuad;
		}
		return Quad.Empty;
	}

	internal void GetPointsAtStartOfSegment(List<Point> abPoints, List<Point> dcPoints)
	{
		if (!IsValid)
		{
			return;
		}
		Quad connectingQuad = ConnectingQuad;
		if (IsEllipse)
		{
			Rect nodeBounds = _operations.GetNodeBounds(in _lastNode);
			abPoints.Add(connectingQuad.D);
			abPoints.Add(StrokeRenderer.ArcToMarker);
			abPoints.Add(new Point(nodeBounds.Width, nodeBounds.Height));
			abPoints.Add(connectingQuad.A);
			dcPoints.Add(connectingQuad.D);
			return;
		}
		Rect nodeBounds2 = _operations.GetNodeBounds(in _thisNode);
		Vector[] vertices = _operations.GetVertices();
		double num = _lastNode.PressureFactor;
		int num2 = vertices.Length * 2;
		int i = 0;
		bool flag = true;
		for (; i < num2; i++)
		{
			if (_lastNode.Position + vertices[i % vertices.Length] * num == connectingQuad.D)
			{
				if (!nodeBounds2.Contains(connectingQuad.D))
				{
					flag = false;
					abPoints.Add(connectingQuad.D);
					dcPoints.Add(connectingQuad.D);
				}
				break;
			}
		}
		if (i == num2)
		{
			return;
		}
		i++;
		int num3 = 0;
		while (i < num2 && num3 < vertices.Length)
		{
			Point point = _lastNode.Position + vertices[i % vertices.Length] * num;
			if (!nodeBounds2.Contains(point))
			{
				abPoints.Add(point);
			}
			if (flag)
			{
				flag = false;
				dcPoints.Add(point);
			}
			if (!(point == connectingQuad.A))
			{
				i++;
				num3++;
				continue;
			}
			break;
		}
	}

	internal void GetPointsAtEndOfSegment(List<Point> abPoints, List<Point> dcPoints)
	{
		if (!IsValid)
		{
			return;
		}
		Quad connectingQuad = ConnectingQuad;
		if (IsEllipse)
		{
			Rect bounds = GetBounds();
			abPoints.Add(connectingQuad.B);
			abPoints.Add(StrokeRenderer.ArcToMarker);
			abPoints.Add(new Point(bounds.Width, bounds.Height));
			abPoints.Add(connectingQuad.C);
			return;
		}
		double num = _thisNode.PressureFactor;
		Vector[] vertices = _operations.GetVertices();
		int num2 = vertices.Length * 2;
		int i;
		for (i = 0; i < num2; i++)
		{
			if (_thisNode.Position + vertices[i % vertices.Length] * num == connectingQuad.B)
			{
				abPoints.Add(connectingQuad.B);
				break;
			}
		}
		if (i == num2)
		{
			return;
		}
		i++;
		int num3 = 0;
		while (i < num2 && num3 < vertices.Length)
		{
			Point point = _thisNode.Position + vertices[i % vertices.Length] * num;
			if (point == connectingQuad.C)
			{
				break;
			}
			abPoints.Add(point);
			i++;
			num3++;
		}
		dcPoints.Add(connectingQuad.C);
	}

	internal void GetPointsAtMiddleSegment(StrokeNode previous, double angleBetweenNodes, List<Point> abPoints, List<Point> dcPoints, out bool missingIntersection)
	{
		missingIntersection = false;
		if (!IsValid || !previous.IsValid)
		{
			return;
		}
		Quad connectingQuad = previous.ConnectingQuad;
		if (connectingQuad.IsEmpty)
		{
			return;
		}
		Quad connectingQuad2 = ConnectingQuad;
		if (connectingQuad2.IsEmpty)
		{
			return;
		}
		if (IsEllipse)
		{
			Rect nodeBounds = _operations.GetNodeBounds(in previous._lastNode);
			Rect nodeBounds2 = _operations.GetNodeBounds(in _lastNode);
			if (angleBetweenNodes == 0.0 || (connectingQuad.B == connectingQuad2.A && connectingQuad.C == connectingQuad2.D))
			{
				abPoints.Add(connectingQuad.B);
				dcPoints.Add(connectingQuad.C);
				return;
			}
			if (angleBetweenNodes > 0.0)
			{
				if (connectingQuad.B == connectingQuad2.A)
				{
					abPoints.Add(connectingQuad.B);
				}
				else
				{
					Point intersection = GetIntersection(connectingQuad.A, connectingQuad.B, connectingQuad2.A, connectingQuad2.B);
					Rect rect = Rect.Union(nodeBounds, nodeBounds2);
					rect.Inflate(1.0, 1.0);
					if (!rect.Contains(intersection))
					{
						missingIntersection = true;
						return;
					}
					abPoints.Add(intersection);
				}
				if (connectingQuad.C == connectingQuad2.D)
				{
					dcPoints.Add(connectingQuad.C);
					return;
				}
				dcPoints.Add(connectingQuad.C);
				dcPoints.Add(new Point(nodeBounds2.Width, nodeBounds2.Height));
				dcPoints.Add(StrokeRenderer.ArcToMarker);
				dcPoints.Add(connectingQuad2.D);
				return;
			}
			if (connectingQuad.C == connectingQuad2.D)
			{
				dcPoints.Add(connectingQuad.C);
			}
			else
			{
				Point intersection2 = GetIntersection(connectingQuad.D, connectingQuad.C, connectingQuad2.D, connectingQuad2.C);
				Rect rect2 = Rect.Union(nodeBounds, nodeBounds2);
				rect2.Inflate(1.0, 1.0);
				if (!rect2.Contains(intersection2))
				{
					missingIntersection = true;
					return;
				}
				dcPoints.Add(intersection2);
			}
			if (connectingQuad.B == connectingQuad2.A)
			{
				abPoints.Add(connectingQuad.B);
				return;
			}
			abPoints.Add(connectingQuad.B);
			abPoints.Add(StrokeRenderer.ArcToMarker);
			abPoints.Add(new Point(nodeBounds2.Width, nodeBounds2.Height));
			abPoints.Add(connectingQuad2.A);
			return;
		}
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		int num4 = -1;
		Vector[] vertices = _operations.GetVertices();
		double num5 = _lastNode.PressureFactor;
		for (int i = 0; i < vertices.Length; i++)
		{
			Point point = _lastNode.Position + vertices[i % vertices.Length] * num5;
			if (point == connectingQuad2.A)
			{
				num = i;
			}
			if (point == connectingQuad.B)
			{
				num2 = i;
			}
			if (point == connectingQuad.C)
			{
				num3 = i;
			}
			if (point == connectingQuad2.D)
			{
				num4 = i;
			}
		}
		if (num == -1 || num2 == -1 || num3 == -1 || num4 == -1)
		{
			return;
		}
		Rect nodeBounds3 = _operations.GetNodeBounds(in _thisNode);
		if (num == num2)
		{
			if (!nodeBounds3.Contains(connectingQuad.B))
			{
				abPoints.Add(connectingQuad.B);
			}
		}
		else if ((num == 0 && num2 == 3) || ((num != 3 || num2 != 0) && num > num2))
		{
			if (!nodeBounds3.Contains(connectingQuad.B))
			{
				abPoints.Add(connectingQuad.B);
			}
			if (!nodeBounds3.Contains(connectingQuad2.A))
			{
				abPoints.Add(connectingQuad2.A);
			}
		}
		else
		{
			Point intersection3 = GetIntersection(connectingQuad.A, connectingQuad.B, connectingQuad2.A, connectingQuad2.B);
			Rect rect3 = Rect.Union(_operations.GetNodeBounds(in previous._lastNode), _operations.GetNodeBounds(in _lastNode));
			rect3.Inflate(1.0, 1.0);
			if (!rect3.Contains(intersection3))
			{
				missingIntersection = true;
				return;
			}
			abPoints.Add(intersection3);
		}
		if (num3 == num4)
		{
			if (!nodeBounds3.Contains(connectingQuad.C))
			{
				dcPoints.Add(connectingQuad.C);
			}
		}
		else if ((num3 == 0 && num4 == 3) || ((num3 != 3 || num4 != 0) && num3 > num4))
		{
			if (!nodeBounds3.Contains(connectingQuad.C))
			{
				dcPoints.Add(connectingQuad.C);
			}
			if (!nodeBounds3.Contains(connectingQuad2.D))
			{
				dcPoints.Add(connectingQuad2.D);
			}
		}
		else
		{
			Point intersection4 = GetIntersection(connectingQuad.D, connectingQuad.C, connectingQuad2.D, connectingQuad2.C);
			Rect rect4 = Rect.Union(_operations.GetNodeBounds(in previous._lastNode), _operations.GetNodeBounds(in _lastNode));
			rect4.Inflate(1.0, 1.0);
			if (rect4.Contains(intersection4))
			{
				dcPoints.Add(intersection4);
			}
			else
			{
				missingIntersection = true;
			}
		}
	}

	internal static Point GetIntersection(Point line1Start, Point line1End, Point line2Start, Point line2End)
	{
		double num = line1End.Y - line1Start.Y;
		double num2 = line1Start.X - line1End.X;
		double num3 = line1End.X * line1Start.Y - line1Start.X * line1End.Y;
		double num4 = line2End.Y - line2Start.Y;
		double num5 = line2Start.X - line2End.X;
		double num6 = line2End.X * line2Start.Y - line2Start.X * line2End.Y;
		double num7 = num * num5 - num4 * num2;
		if (num7 != 0.0)
		{
			double num8 = (num2 * num6 - num5 * num3) / num7;
			double num9 = (num4 * num3 - num * num6) / num7;
			double num10;
			double num11;
			if (line1Start.X < line1End.X)
			{
				num10 = Math.Floor(line1Start.X);
				num11 = Math.Ceiling(line1End.X);
			}
			else
			{
				num10 = Math.Floor(line1End.X);
				num11 = Math.Ceiling(line1Start.X);
			}
			double num12;
			double num13;
			if (line2Start.X < line2End.X)
			{
				num12 = Math.Floor(line2Start.X);
				num13 = Math.Ceiling(line2End.X);
			}
			else
			{
				num12 = Math.Floor(line2End.X);
				num13 = Math.Ceiling(line2Start.X);
			}
			double num14;
			double num15;
			if (line1Start.Y < line1End.Y)
			{
				num14 = Math.Floor(line1Start.Y);
				num15 = Math.Ceiling(line1End.Y);
			}
			else
			{
				num14 = Math.Floor(line1End.Y);
				num15 = Math.Ceiling(line1Start.Y);
			}
			double num16;
			double num17;
			if (line2Start.Y < line2End.Y)
			{
				num16 = Math.Floor(line2Start.Y);
				num17 = Math.Ceiling(line2End.Y);
			}
			else
			{
				num16 = Math.Floor(line2End.Y);
				num17 = Math.Ceiling(line2Start.Y);
			}
			if (num10 <= num8 && num8 <= num11 && num14 <= num9 && num9 <= num15 && num12 <= num8 && num8 <= num13 && num16 <= num9 && num9 <= num17)
			{
				return new Point(num8, num9);
			}
		}
		if ((long)line1End.X == (long)line2Start.X && (long)line1End.Y == (long)line2Start.Y)
		{
			return new Point(line1End.X, line1End.Y);
		}
		return new Point(double.NaN, double.NaN);
	}

	internal bool HitTest(StrokeNode hitNode)
	{
		if (!IsValid || !hitNode.IsValid)
		{
			return false;
		}
		IEnumerable<ContourSegment> contourSegments = hitNode.GetContourSegments();
		return _operations.HitTest(in _lastNode, in _thisNode, ConnectingQuad, contourSegments);
	}

	internal StrokeFIndices CutTest(StrokeNode hitNode)
	{
		if (!IsValid || !hitNode.IsValid)
		{
			return StrokeFIndices.Empty;
		}
		IEnumerable<ContourSegment> contourSegments = hitNode.GetContourSegments();
		StrokeFIndices strokeFIndices = _operations.CutTest(in _lastNode, in _thisNode, ConnectingQuad, contourSegments);
		if (_index != 0)
		{
			return BindFIndices(strokeFIndices);
		}
		return strokeFIndices;
	}

	internal StrokeFIndices CutTest(Point begin, Point end)
	{
		if (!IsValid)
		{
			return StrokeFIndices.Empty;
		}
		StrokeFIndices fragment = _operations.CutTest(in _lastNode, in _thisNode, ConnectingQuad, begin, end);
		return BindFIndicesForLassoHitTest(fragment);
	}

	private StrokeFIndices BindFIndices(StrokeFIndices fragment)
	{
		if (!fragment.IsEmpty)
		{
			if (!DoubleUtil.AreClose(fragment.BeginFIndex, StrokeFIndices.BeforeFirst))
			{
				fragment.BeginFIndex += _index - 1;
			}
			if (!DoubleUtil.AreClose(fragment.EndFIndex, StrokeFIndices.AfterLast))
			{
				fragment.EndFIndex += _index - 1;
			}
		}
		return fragment;
	}

	private StrokeFIndices BindFIndicesForLassoHitTest(StrokeFIndices fragment)
	{
		if (!fragment.IsEmpty)
		{
			if (DoubleUtil.AreClose(fragment.BeginFIndex, StrokeFIndices.BeforeFirst))
			{
				fragment.BeginFIndex = ((_index == 0) ? StrokeFIndices.BeforeFirst : ((double)(_index - 1)));
			}
			else
			{
				fragment.BeginFIndex += _index - 1;
			}
			if (DoubleUtil.AreClose(fragment.EndFIndex, StrokeFIndices.AfterLast))
			{
				fragment.EndFIndex = (_isLastNode ? StrokeFIndices.AfterLast : ((double)_index));
			}
			else
			{
				fragment.EndFIndex += _index - 1;
			}
		}
		return fragment;
	}

	private IEnumerable<ContourSegment> GetContourSegments()
	{
		if (IsEllipse)
		{
			return _operations.GetNonBezierContourSegments(_lastNode, _thisNode);
		}
		return _operations.GetContourSegments(_thisNode, ConnectingQuad);
	}

	internal Point GetPointAt(double findex)
	{
		if (_lastNode.IsEmpty)
		{
			return _thisNode.Position;
		}
		if (DoubleUtil.AreClose(findex, _index))
		{
			return _thisNode.Position;
		}
		double num = Math.Floor(findex);
		findex -= num;
		double num2 = (_thisNode.Position.X - _lastNode.Position.X) * findex;
		double num3 = (_thisNode.Position.Y - _lastNode.Position.Y) * findex;
		return new Point(_lastNode.Position.X + num2, _lastNode.Position.Y + num3);
	}
}
