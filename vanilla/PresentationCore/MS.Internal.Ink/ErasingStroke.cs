using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;

namespace MS.Internal.Ink;

internal class ErasingStroke
{
	private StrokeNodeIterator _nodeIterator;

	private List<StrokeNode> _erasingStrokeNodes;

	private Rect _bounds = Rect.Empty;

	private const double CollinearTolerance = 0.10000000149011612;

	internal Rect Bounds => _bounds;

	internal ErasingStroke(StylusShape erasingShape)
	{
		_nodeIterator = new StrokeNodeIterator(erasingShape);
	}

	internal ErasingStroke(StylusShape erasingShape, IEnumerable<Point> path)
		: this(erasingShape)
	{
		MoveTo(path);
	}

	internal void MoveTo(IEnumerable<Point> path)
	{
		Point[] pointArray = IEnumerablePointHelper.GetPointArray(path);
		if (_erasingStrokeNodes == null)
		{
			_erasingStrokeNodes = new List<StrokeNode>(pointArray.Length);
		}
		else
		{
			_erasingStrokeNodes.Clear();
		}
		_bounds = Rect.Empty;
		_nodeIterator = _nodeIterator.GetIteratorForNextSegment((pointArray.Length > 1) ? FilterPoints(pointArray) : pointArray);
		for (int i = 0; i < _nodeIterator.Count; i++)
		{
			StrokeNode item = _nodeIterator[i];
			_bounds.Union(item.GetBoundsConnected());
			_erasingStrokeNodes.Add(item);
		}
	}

	internal bool HitTest(StrokeNodeIterator iterator)
	{
		if (_erasingStrokeNodes == null || _erasingStrokeNodes.Count == 0)
		{
			return false;
		}
		Rect empty = Rect.Empty;
		for (int i = 0; i < iterator.Count; i++)
		{
			StrokeNode hitNode = iterator[i];
			Rect bounds = hitNode.GetBounds();
			empty.Union(bounds);
			if (!empty.IntersectsWith(_bounds))
			{
				continue;
			}
			foreach (StrokeNode erasingStrokeNode in _erasingStrokeNodes)
			{
				if (empty.IntersectsWith(erasingStrokeNode.GetBoundsConnected()) && erasingStrokeNode.HitTest(hitNode))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal bool EraseTest(StrokeNodeIterator iterator, List<StrokeIntersection> intersections)
	{
		intersections.Clear();
		List<StrokeFIndices> list = new List<StrokeFIndices>();
		if (_erasingStrokeNodes == null || _erasingStrokeNodes.Count == 0)
		{
			return false;
		}
		Rect rect = Rect.Empty;
		for (int i = 0; i < iterator.Count; i++)
		{
			StrokeNode strokeNode = iterator[i];
			Rect bounds = strokeNode.GetBounds();
			rect.Union(bounds);
			if (rect.IntersectsWith(_bounds))
			{
				int count = list.Count;
				foreach (StrokeNode erasingStrokeNode in _erasingStrokeNodes)
				{
					if (!rect.IntersectsWith(erasingStrokeNode.GetBoundsConnected()))
					{
						continue;
					}
					StrokeFIndices strokeFIndices = strokeNode.CutTest(erasingStrokeNode);
					if (strokeFIndices.IsEmpty)
					{
						continue;
					}
					bool flag = false;
					for (int j = count; j < list.Count; j++)
					{
						StrokeFIndices strokeFIndices2 = list[j];
						if (strokeFIndices.BeginFIndex < strokeFIndices2.EndFIndex)
						{
							if (!(strokeFIndices.EndFIndex > strokeFIndices2.BeginFIndex))
							{
								list.Insert(j, strokeFIndices);
								flag = true;
								break;
							}
							strokeFIndices = new StrokeFIndices(Math.Min(strokeFIndices2.BeginFIndex, strokeFIndices.BeginFIndex), Math.Max(strokeFIndices2.EndFIndex, strokeFIndices.EndFIndex));
							if (strokeFIndices.EndFIndex <= strokeFIndices2.EndFIndex || j + 1 == list.Count)
							{
								flag = true;
								list[j] = strokeFIndices;
								break;
							}
							list.RemoveAt(j);
							j--;
						}
					}
					if (!flag)
					{
						list.Add(strokeFIndices);
					}
					if (list[list.Count - 1].IsFull)
					{
						break;
					}
				}
				if (count > 0 && count < list.Count)
				{
					StrokeFIndices value = list[count - 1];
					if (DoubleUtil.AreClose(value.EndFIndex, StrokeFIndices.AfterLast))
					{
						if (DoubleUtil.AreClose(list[count].BeginFIndex, StrokeFIndices.BeforeFirst))
						{
							value.EndFIndex = list[count].EndFIndex;
							list[count - 1] = value;
							list.RemoveAt(count);
						}
						else
						{
							value.EndFIndex = strokeNode.Index;
							list[count - 1] = value;
						}
					}
				}
			}
			rect = bounds;
		}
		if (list.Count != 0)
		{
			foreach (StrokeFIndices item in list)
			{
				intersections.Add(new StrokeIntersection(item.BeginFIndex, StrokeFIndices.AfterLast, StrokeFIndices.BeforeFirst, item.EndFIndex));
			}
		}
		return list.Count != 0;
	}

	private Point[] FilterPoints(Point[] path)
	{
		List<Point> list = new List<Point>();
		Point point;
		Point point2;
		int num;
		if (_nodeIterator.Count == 0)
		{
			list.Add(path[0]);
			list.Add(path[1]);
			point = path[0];
			point2 = path[1];
			num = 2;
		}
		else
		{
			list.Add(path[0]);
			point = _nodeIterator[_nodeIterator.Count - 1].Position;
			point2 = path[0];
			num = 1;
		}
		while (num < path.Length)
		{
			if (DoubleUtil.AreClose(point2, path[num]))
			{
				num++;
				continue;
			}
			Vector vector = point - point2;
			Vector vector2 = path[num] - point2;
			double projectionFIndex = StrokeNodeOperations.GetProjectionFIndex(vector, vector2);
			if (DoubleUtil.IsBetweenZeroAndOne(projectionFIndex) && (vector + (vector2 - vector) * projectionFIndex).LengthSquared < 0.10000000149011612)
			{
				list[list.Count - 1] = path[num];
				point2 = path[num];
				num++;
			}
			else
			{
				list.Add(path[num]);
				point = point2;
				point2 = path[num];
				num++;
			}
		}
		return list.ToArray();
	}
}
