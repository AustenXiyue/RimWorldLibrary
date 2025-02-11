using System.Collections.Generic;

namespace System.Windows.Documents;

internal class FixedSOMLineRanges
{
	private double _line;

	private List<double> _start;

	private List<double> _end;

	private const double _minLineSeparation = 3.0;

	public double Line
	{
		get
		{
			return _line;
		}
		set
		{
			_line = value;
		}
	}

	public List<double> Start
	{
		get
		{
			if (_start == null)
			{
				_start = new List<double>();
			}
			return _start;
		}
	}

	public List<double> End
	{
		get
		{
			if (_end == null)
			{
				_end = new List<double>();
			}
			return _end;
		}
	}

	public int Count => Start.Count;

	public static double MinLineSeparation => 3.0;

	public void AddRange(double start, double end)
	{
		int num = 0;
		while (num < Start.Count)
		{
			if (start > End[num] + 3.0)
			{
				num++;
				continue;
			}
			if (end + 3.0 < Start[num])
			{
				Start.Insert(num, start);
				End.Insert(num, end);
				return;
			}
			if (Start[num] < start)
			{
				start = Start[num];
			}
			if (End[num] > end)
			{
				end = End[num];
			}
			Start.RemoveAt(num);
			End.RemoveAt(num);
		}
		Start.Add(start);
		End.Add(end);
	}

	public int GetLineAt(double line)
	{
		int num = 0;
		int num2 = Start.Count - 1;
		while (num2 > num)
		{
			int num3 = num + num2 >> 1;
			if (line > End[num3])
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3;
			}
		}
		if (num == num2 && line <= End[num] && line >= Start[num])
		{
			return num;
		}
		return -1;
	}
}
