using System.Collections.Generic;

namespace System.Windows.Documents;

internal sealed class FixedSOMLineCollection
{
	private List<FixedSOMLineRanges> _horizontals;

	private List<FixedSOMLineRanges> _verticals;

	private const double _fudgeFactor = 0.1;

	public List<FixedSOMLineRanges> HorizontalLines => _horizontals;

	public List<FixedSOMLineRanges> VerticalLines => _verticals;

	public FixedSOMLineCollection()
	{
		_verticals = new List<FixedSOMLineRanges>();
		_horizontals = new List<FixedSOMLineRanges>();
	}

	public bool IsVerticallySeparated(double left, double top, double right, double bottom)
	{
		return _IsSeparated(_verticals, left, top, right, bottom);
	}

	public bool IsHorizontallySeparated(double left, double top, double right, double bottom)
	{
		return _IsSeparated(_horizontals, top, left, bottom, right);
	}

	public void AddVertical(Point point1, Point point2)
	{
		_AddLineToRanges(_verticals, point1.X, point1.Y, point2.Y);
	}

	public void AddHorizontal(Point point1, Point point2)
	{
		_AddLineToRanges(_horizontals, point1.Y, point1.X, point2.X);
	}

	private void _AddLineToRanges(List<FixedSOMLineRanges> ranges, double line, double start, double end)
	{
		if (start > end)
		{
			double num = start;
			start = end;
			end = num;
		}
		double num2 = 0.5 * FixedSOMLineRanges.MinLineSeparation;
		FixedSOMLineRanges fixedSOMLineRanges;
		for (int i = 0; i < ranges.Count; i++)
		{
			if (line < ranges[i].Line - num2)
			{
				fixedSOMLineRanges = new FixedSOMLineRanges();
				fixedSOMLineRanges.Line = line;
				fixedSOMLineRanges.AddRange(start, end);
				ranges.Insert(i, fixedSOMLineRanges);
				return;
			}
			if (line < ranges[i].Line + num2)
			{
				ranges[i].AddRange(start, end);
				return;
			}
		}
		fixedSOMLineRanges = new FixedSOMLineRanges();
		fixedSOMLineRanges.Line = line;
		fixedSOMLineRanges.AddRange(start, end);
		ranges.Add(fixedSOMLineRanges);
	}

	private bool _IsSeparated(List<FixedSOMLineRanges> lines, double parallelLowEnd, double perpLowEnd, double parallelHighEnd, double perpHighEnd)
	{
		int num = 0;
		int num2 = lines.Count;
		if (num2 == 0)
		{
			return false;
		}
		int num3 = 0;
		while (num2 > num)
		{
			num3 = num + num2 >> 1;
			if (lines[num3].Line < parallelLowEnd)
			{
				num = num3 + 1;
				continue;
			}
			if (lines[num3].Line <= parallelHighEnd)
			{
				break;
			}
			num2 = num3;
		}
		if (lines[num3].Line >= parallelLowEnd && lines[num3].Line <= parallelHighEnd)
		{
			do
			{
				num3--;
			}
			while (num3 >= 0 && lines[num3].Line >= parallelLowEnd);
			for (num3++; num3 < lines.Count && lines[num3].Line <= parallelHighEnd; num3++)
			{
				double num4 = (perpHighEnd - perpLowEnd) * 0.1;
				int lineAt = lines[num3].GetLineAt(perpLowEnd + num4);
				if (lineAt >= 0 && lines[num3].End[lineAt] >= perpHighEnd - num4)
				{
					return true;
				}
			}
		}
		return false;
	}
}
