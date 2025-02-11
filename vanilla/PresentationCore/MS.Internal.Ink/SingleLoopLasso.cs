using System.Collections.Generic;
using System.Windows;

namespace MS.Internal.Ink;

internal class SingleLoopLasso : Lasso
{
	private bool _hasLoop;

	private Rect _prevBounds = Rect.Empty;

	private static readonly double NoIntersection = StrokeFIndices.BeforeFirst;

	internal SingleLoopLasso()
	{
	}

	protected override bool Filter(Point point)
	{
		List<Point> pointsList = base.PointsList;
		if (pointsList.Count == 0)
		{
			return false;
		}
		if (_hasLoop || base.Filter(point))
		{
			return true;
		}
		double bIndex = 0.0;
		if (GetIntersectionWithExistingLasso(point, ref bIndex))
		{
			if (bIndex == (double)(pointsList.Count - 2))
			{
				return true;
			}
			int num = (int)bIndex;
			if (!DoubleUtil.AreClose(num, bIndex))
			{
				Point value = new Point(0.0, 0.0);
				value.X = pointsList[num].X + (bIndex - (double)num) * (pointsList[num + 1].X - pointsList[num].X);
				value.Y = pointsList[num].Y + (bIndex - (double)num) * (pointsList[num + 1].Y - pointsList[num].Y);
				pointsList[num] = value;
				base.IsIncrementalLassoDirty = true;
			}
			if (num > 0)
			{
				pointsList.RemoveRange(0, num);
				base.IsIncrementalLassoDirty = true;
			}
			if (base.IsIncrementalLassoDirty)
			{
				Rect empty = Rect.Empty;
				for (int i = 0; i < pointsList.Count; i++)
				{
					empty.Union(pointsList[i]);
				}
				base.Bounds = empty;
			}
			_hasLoop = true;
			return true;
		}
		return false;
	}

	protected override void AddPointImpl(Point point)
	{
		_prevBounds = base.Bounds;
		base.AddPointImpl(point);
	}

	private bool GetIntersectionWithExistingLasso(Point point, ref double bIndex)
	{
		List<Point> pointsList = base.PointsList;
		int count = pointsList.Count;
		Rect rect = new Rect(pointsList[count - 1], point);
		if (!_prevBounds.IntersectsWith(rect))
		{
			return false;
		}
		for (int i = 0; i < count - 2; i++)
		{
			if (new Rect(pointsList[i], pointsList[i + 1]).IntersectsWith(rect))
			{
				double num = FindIntersection(pointsList[count - 1] - pointsList[i], point - pointsList[i], new Vector(0.0, 0.0), pointsList[i + 1] - pointsList[i]);
				if (num >= 0.0 && num <= 1.0)
				{
					bIndex = (double)i + num;
					return true;
				}
			}
		}
		return false;
	}

	private static double FindIntersection(Vector hitBegin, Vector hitEnd, Vector orgBegin, Vector orgEnd)
	{
		Vector vector = orgEnd - orgBegin;
		Vector vector2 = orgBegin - hitBegin;
		Vector vector3 = hitEnd - hitBegin;
		double num = Vector.Determinant(vector, vector3);
		if (DoubleUtil.IsZero(num))
		{
			return NoIntersection;
		}
		double num2 = AdjustFIndex(Vector.Determinant(vector, vector2) / num);
		if (num2 >= 0.0 && num2 <= 1.0)
		{
			double num3 = AdjustFIndex(Vector.Determinant(vector3, vector2) / num);
			if (num3 >= 0.0 && num3 <= 1.0)
			{
				return num3;
			}
		}
		return NoIntersection;
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
