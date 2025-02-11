using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;

namespace MS.Internal.Ink;

internal class Lasso
{
	private struct LassoCrossing : IComparable
	{
		internal StrokeFIndices FIndices;

		internal StrokeNode StartNode;

		internal StrokeNode EndNode;

		public static LassoCrossing EmptyCrossing
		{
			get
			{
				LassoCrossing result = default(LassoCrossing);
				result.FIndices = StrokeFIndices.Empty;
				return result;
			}
		}

		public bool IsEmpty => FIndices.IsEmpty;

		public LassoCrossing(StrokeFIndices newFIndices, StrokeNode strokeNode)
		{
			FIndices = newFIndices;
			StartNode = (EndNode = strokeNode);
		}

		public override string ToString()
		{
			return FIndices.ToString();
		}

		public int CompareTo(object obj)
		{
			LassoCrossing lassoCrossing = (LassoCrossing)obj;
			if (lassoCrossing.IsEmpty && IsEmpty)
			{
				return 0;
			}
			if (lassoCrossing.IsEmpty)
			{
				return 1;
			}
			if (IsEmpty)
			{
				return -1;
			}
			return FIndices.CompareTo(lassoCrossing.FIndices);
		}

		public bool Merge(LassoCrossing crossing)
		{
			if (crossing.IsEmpty)
			{
				return false;
			}
			if (FIndices.IsEmpty && !crossing.IsEmpty)
			{
				FIndices = crossing.FIndices;
				StartNode = crossing.StartNode;
				EndNode = crossing.EndNode;
				return true;
			}
			if (DoubleUtil.GreaterThanOrClose(crossing.FIndices.EndFIndex, FIndices.BeginFIndex) && DoubleUtil.GreaterThanOrClose(FIndices.EndFIndex, crossing.FIndices.BeginFIndex))
			{
				if (DoubleUtil.LessThan(crossing.FIndices.BeginFIndex, FIndices.BeginFIndex))
				{
					FIndices.BeginFIndex = crossing.FIndices.BeginFIndex;
					StartNode = crossing.StartNode;
				}
				if (DoubleUtil.GreaterThan(crossing.FIndices.EndFIndex, FIndices.EndFIndex))
				{
					FIndices.EndFIndex = crossing.FIndices.EndFIndex;
					EndNode = crossing.EndNode;
				}
				return true;
			}
			return false;
		}
	}

	private List<Point> _points;

	private Rect _bounds = Rect.Empty;

	private bool _incrementalLassoDirty;

	private const double MinDistance = 1.0;

	internal Rect Bounds
	{
		get
		{
			return _bounds;
		}
		set
		{
			_bounds = value;
		}
	}

	internal bool IsEmpty => _points.Count < 3;

	internal int PointCount => _points.Count;

	internal Point this[int index] => _points[index];

	internal bool IsIncrementalLassoDirty
	{
		get
		{
			return _incrementalLassoDirty;
		}
		set
		{
			_incrementalLassoDirty = value;
		}
	}

	protected List<Point> PointsList => _points;

	internal Lasso()
	{
		_points = new List<Point>();
	}

	internal void AddPoints(IEnumerable<Point> points)
	{
		foreach (Point point in points)
		{
			AddPoint(point);
		}
	}

	internal void AddPoint(Point point)
	{
		if (!Filter(point))
		{
			AddPointImpl(point);
		}
	}

	internal bool Contains(Point point)
	{
		if (!_bounds.Contains(point))
		{
			return false;
		}
		bool flag = false;
		int num = _points.Count;
		while (--num >= 0)
		{
			if (!DoubleUtil.AreClose(_points[num].Y, point.Y))
			{
				flag = point.Y < _points[num].Y;
				break;
			}
		}
		bool flag2 = false;
		Point point2 = _points[_points.Count - 1];
		for (int i = 0; i < _points.Count; i++)
		{
			Point point3 = _points[i];
			if (DoubleUtil.AreClose(point3.Y, point.Y))
			{
				if (DoubleUtil.AreClose(point3.X, point.X))
				{
					flag2 = true;
					break;
				}
				if (i != 0 && DoubleUtil.AreClose(point2.Y, point.Y) && DoubleUtil.GreaterThanOrClose(point.X, Math.Min(point2.X, point3.X)) && DoubleUtil.LessThanOrClose(point.X, Math.Max(point2.X, point3.X)))
				{
					flag2 = true;
					break;
				}
			}
			else if (flag != point.Y < point3.Y)
			{
				flag = !flag;
				if (DoubleUtil.GreaterThanOrClose(point.X, Math.Max(point2.X, point3.X)))
				{
					flag2 = !flag2;
				}
				else if (DoubleUtil.GreaterThanOrClose(point.X, Math.Min(point2.X, point3.X)))
				{
					Vector vector = point3 - point2;
					double value = point2.X + vector.X / vector.Y * (point.Y - point2.Y);
					if (DoubleUtil.GreaterThanOrClose(point.X, value))
					{
						flag2 = !flag2;
					}
				}
			}
			point2 = point3;
		}
		return flag2;
	}

	internal StrokeIntersection[] HitTest(StrokeNodeIterator iterator)
	{
		if (_points.Count < 3)
		{
			return Array.Empty<StrokeIntersection>();
		}
		Point point = default(Point);
		Point point2 = _points[_points.Count - 1];
		Rect rect = Rect.Empty;
		LassoCrossing item = LassoCrossing.EmptyCrossing;
		List<LassoCrossing> crossingList = new List<LassoCrossing>();
		for (int i = 0; i < iterator.Count; i++)
		{
			StrokeNode strokeNode = iterator[i];
			Rect bounds = strokeNode.GetBounds();
			rect.Union(bounds);
			if (rect.IntersectsWith(_bounds))
			{
				Point point3 = point2;
				foreach (Point point4 in _points)
				{
					Rect rect2 = new Rect(point3, point4);
					if (!rect.IntersectsWith(rect2))
					{
						point3 = point4;
						continue;
					}
					StrokeFIndices newFIndices = strokeNode.CutTest(point3, point4);
					point3 = point4;
					if (!newFIndices.IsEmpty)
					{
						LassoCrossing lassoCrossing = new LassoCrossing(newFIndices, strokeNode);
						if (!item.Merge(lassoCrossing))
						{
							crossingList.Add(item);
							item = lassoCrossing;
						}
					}
				}
			}
			rect = bounds;
			point = strokeNode.Position;
		}
		if (!item.IsEmpty)
		{
			crossingList.Add(item);
		}
		if (crossingList.Count == 0)
		{
			if (Contains(point))
			{
				return new StrokeIntersection[1] { StrokeIntersection.Full };
			}
			return Array.Empty<StrokeIntersection>();
		}
		SortAndMerge(ref crossingList);
		List<StrokeIntersection> list = new List<StrokeIntersection>();
		ProduceHitTestResults(crossingList, list);
		return list.ToArray();
	}

	private static void SortAndMerge(ref List<LassoCrossing> crossingList)
	{
		crossingList.Sort();
		List<LassoCrossing> list = new List<LassoCrossing>();
		LassoCrossing item = LassoCrossing.EmptyCrossing;
		foreach (LassoCrossing crossing in crossingList)
		{
			if (!item.Merge(crossing))
			{
				list.Add(item);
				item = crossing;
			}
		}
		if (!item.IsEmpty)
		{
			list.Add(item);
		}
		crossingList = list;
	}

	private bool SegmentWithinLasso(StrokeNode strokeNode, double fIndex)
	{
		if (DoubleUtil.AreClose(fIndex, StrokeFIndices.BeforeFirst))
		{
			return Contains(strokeNode.GetPointAt(0.0));
		}
		if (DoubleUtil.AreClose(fIndex, StrokeFIndices.AfterLast))
		{
			return Contains(strokeNode.Position);
		}
		return Contains(strokeNode.GetPointAt(fIndex));
	}

	private void ProduceHitTestResults(List<LassoCrossing> crossingList, List<StrokeIntersection> strokeIntersections)
	{
		bool flag = false;
		for (int i = 0; i <= crossingList.Count; i++)
		{
			bool flag2 = false;
			bool flag3 = true;
			StrokeIntersection item = default(StrokeIntersection);
			if (i == 0)
			{
				item.HitBegin = StrokeFIndices.BeforeFirst;
				item.InBegin = StrokeFIndices.BeforeFirst;
			}
			else
			{
				item.InBegin = crossingList[i - 1].FIndices.EndFIndex;
				item.HitBegin = crossingList[i - 1].FIndices.BeginFIndex;
				flag2 = SegmentWithinLasso(crossingList[i - 1].EndNode, item.InBegin);
			}
			if (i == crossingList.Count)
			{
				if (DoubleUtil.AreClose(item.InBegin, StrokeFIndices.AfterLast))
				{
					item.InEnd = StrokeFIndices.BeforeFirst;
				}
				else
				{
					item.InEnd = StrokeFIndices.AfterLast;
				}
				item.HitEnd = StrokeFIndices.AfterLast;
			}
			else
			{
				item.InEnd = crossingList[i].FIndices.BeginFIndex;
				if (DoubleUtil.AreClose(item.InEnd, StrokeFIndices.BeforeFirst))
				{
					item.InBegin = StrokeFIndices.AfterLast;
				}
				item.HitEnd = crossingList[i].FIndices.EndFIndex;
				flag2 = SegmentWithinLasso(crossingList[i].StartNode, item.InEnd);
				if (!flag2 && !SegmentWithinLasso(crossingList[i].EndNode, item.HitEnd))
				{
					flag2 = true;
					item.HitBegin = crossingList[i].FIndices.BeginFIndex;
					item.InBegin = StrokeFIndices.AfterLast;
					item.InEnd = StrokeFIndices.BeforeFirst;
					flag3 = false;
				}
			}
			if (flag2)
			{
				if (i > 0 && flag && flag3)
				{
					StrokeIntersection value = strokeIntersections[strokeIntersections.Count - 1];
					if (value.InSegment.IsEmpty)
					{
						value.InBegin = item.InBegin;
					}
					value.InEnd = item.InEnd;
					value.HitEnd = item.HitEnd;
					strokeIntersections[strokeIntersections.Count - 1] = value;
				}
				else
				{
					strokeIntersections.Add(item);
				}
				if (DoubleUtil.AreClose(item.HitEnd, StrokeFIndices.AfterLast))
				{
					break;
				}
			}
			flag = flag2;
		}
	}

	protected virtual bool Filter(Point point)
	{
		if (_points.Count == 0)
		{
			return false;
		}
		Point point2 = _points[_points.Count - 1];
		Vector vector = point - point2;
		if (Math.Abs(vector.X) < 1.0)
		{
			return Math.Abs(vector.Y) < 1.0;
		}
		return false;
	}

	protected virtual void AddPointImpl(Point point)
	{
		_points.Add(point);
		_bounds.Union(point);
	}
}
