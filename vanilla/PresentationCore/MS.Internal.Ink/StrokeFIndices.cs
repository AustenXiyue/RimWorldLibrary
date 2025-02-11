using System;
using System.Globalization;

namespace MS.Internal.Ink;

internal struct StrokeFIndices : IEquatable<StrokeFIndices>
{
	private static readonly StrokeFIndices s_empty = new StrokeFIndices(AfterLast, BeforeFirst);

	private static readonly StrokeFIndices s_full = new StrokeFIndices(BeforeFirst, AfterLast);

	private double _beginFIndex;

	private double _endFIndex;

	internal static double BeforeFirst => double.MinValue;

	internal static double AfterLast => double.MaxValue;

	internal double BeginFIndex
	{
		get
		{
			return _beginFIndex;
		}
		set
		{
			_beginFIndex = value;
		}
	}

	internal double EndFIndex
	{
		get
		{
			return _endFIndex;
		}
		set
		{
			_endFIndex = value;
		}
	}

	internal static StrokeFIndices Empty => s_empty;

	internal static StrokeFIndices Full => s_full;

	internal bool IsEmpty => DoubleUtil.GreaterThanOrClose(_beginFIndex, _endFIndex);

	internal bool IsFull
	{
		get
		{
			if (DoubleUtil.AreClose(_beginFIndex, BeforeFirst))
			{
				return DoubleUtil.AreClose(_endFIndex, AfterLast);
			}
			return false;
		}
	}

	internal StrokeFIndices(double beginFIndex, double endFIndex)
	{
		_beginFIndex = beginFIndex;
		_endFIndex = endFIndex;
	}

	public override string ToString()
	{
		return "{" + GetStringRepresentation(_beginFIndex) + "," + GetStringRepresentation(_endFIndex) + "}";
	}

	public bool Equals(StrokeFIndices strokeFIndices)
	{
		return strokeFIndices == this;
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		return (StrokeFIndices)obj == this;
	}

	public override int GetHashCode()
	{
		return _beginFIndex.GetHashCode() ^ _endFIndex.GetHashCode();
	}

	public static bool operator ==(StrokeFIndices sfiLeft, StrokeFIndices sfiRight)
	{
		if (DoubleUtil.AreClose(sfiLeft._beginFIndex, sfiRight._beginFIndex))
		{
			return DoubleUtil.AreClose(sfiLeft._endFIndex, sfiRight._endFIndex);
		}
		return false;
	}

	public static bool operator !=(StrokeFIndices sfiLeft, StrokeFIndices sfiRight)
	{
		return !(sfiLeft == sfiRight);
	}

	internal static string GetStringRepresentation(double fIndex)
	{
		if (DoubleUtil.AreClose(fIndex, BeforeFirst))
		{
			return "BeforeFirst";
		}
		if (DoubleUtil.AreClose(fIndex, AfterLast))
		{
			return "AfterLast";
		}
		return fIndex.ToString(CultureInfo.InvariantCulture);
	}

	internal int CompareTo(StrokeFIndices fIndices)
	{
		if (DoubleUtil.AreClose(BeginFIndex, fIndices.BeginFIndex))
		{
			return 0;
		}
		if (DoubleUtil.GreaterThan(BeginFIndex, fIndices.BeginFIndex))
		{
			return 1;
		}
		return -1;
	}
}
