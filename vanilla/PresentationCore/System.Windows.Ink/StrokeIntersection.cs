using System.Collections.Generic;
using MS.Internal.Ink;

namespace System.Windows.Ink;

internal struct StrokeIntersection
{
	private static StrokeIntersection s_empty = new StrokeIntersection(AfterLast, AfterLast, BeforeFirst, BeforeFirst);

	private static StrokeIntersection s_full = new StrokeIntersection(BeforeFirst, BeforeFirst, AfterLast, AfterLast);

	private StrokeFIndices _hitSegment;

	private StrokeFIndices _inSegment;

	internal static double BeforeFirst => StrokeFIndices.BeforeFirst;

	internal static double AfterLast => StrokeFIndices.AfterLast;

	internal double HitBegin
	{
		set
		{
			_hitSegment.BeginFIndex = value;
		}
	}

	internal double HitEnd
	{
		get
		{
			return _hitSegment.EndFIndex;
		}
		set
		{
			_hitSegment.EndFIndex = value;
		}
	}

	internal double InBegin
	{
		get
		{
			return _inSegment.BeginFIndex;
		}
		set
		{
			_inSegment.BeginFIndex = value;
		}
	}

	internal double InEnd
	{
		get
		{
			return _inSegment.EndFIndex;
		}
		set
		{
			_inSegment.EndFIndex = value;
		}
	}

	internal static StrokeIntersection Full => s_full;

	internal bool IsEmpty => _hitSegment.IsEmpty;

	internal StrokeFIndices HitSegment => _hitSegment;

	internal StrokeFIndices InSegment => _inSegment;

	internal StrokeIntersection(double hitBegin, double inBegin, double inEnd, double hitEnd)
	{
		_hitSegment = new StrokeFIndices(hitBegin, hitEnd);
		_inSegment = new StrokeFIndices(inBegin, inEnd);
	}

	public override string ToString()
	{
		return "{" + StrokeFIndices.GetStringRepresentation(_hitSegment.BeginFIndex) + "," + StrokeFIndices.GetStringRepresentation(_inSegment.BeginFIndex) + "," + StrokeFIndices.GetStringRepresentation(_inSegment.EndFIndex) + "," + StrokeFIndices.GetStringRepresentation(_hitSegment.EndFIndex) + "}";
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		return (StrokeIntersection)obj == this;
	}

	public override int GetHashCode()
	{
		return _hitSegment.GetHashCode() ^ _inSegment.GetHashCode();
	}

	public static bool operator ==(StrokeIntersection left, StrokeIntersection right)
	{
		if (left._hitSegment == right._hitSegment)
		{
			return left._inSegment == right._inSegment;
		}
		return false;
	}

	public static bool operator !=(StrokeIntersection left, StrokeIntersection right)
	{
		return !(left == right);
	}

	internal static StrokeFIndices[] GetInSegments(StrokeIntersection[] intersections)
	{
		List<StrokeFIndices> list = new List<StrokeFIndices>(intersections.Length);
		for (int i = 0; i < intersections.Length; i++)
		{
			if (!intersections[i].InSegment.IsEmpty)
			{
				if (list.Count > 0 && list[list.Count - 1].EndFIndex >= intersections[i].InSegment.BeginFIndex)
				{
					StrokeFIndices value = list[list.Count - 1];
					value.EndFIndex = intersections[i].InSegment.EndFIndex;
					list[list.Count - 1] = value;
				}
				else
				{
					list.Add(intersections[i].InSegment);
				}
			}
		}
		return list.ToArray();
	}

	internal static StrokeFIndices[] GetHitSegments(StrokeIntersection[] intersections)
	{
		List<StrokeFIndices> list = new List<StrokeFIndices>(intersections.Length);
		for (int i = 0; i < intersections.Length; i++)
		{
			if (!intersections[i].HitSegment.IsEmpty)
			{
				if (list.Count > 0 && list[list.Count - 1].EndFIndex >= intersections[i].HitSegment.BeginFIndex)
				{
					StrokeFIndices value = list[list.Count - 1];
					value.EndFIndex = intersections[i].HitSegment.EndFIndex;
					list[list.Count - 1] = value;
				}
				else
				{
					list.Add(intersections[i].HitSegment);
				}
			}
		}
		return list.ToArray();
	}
}
