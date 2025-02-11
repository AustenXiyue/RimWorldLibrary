using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace MS.Internal.Ink;

internal class Bezier
{
	private List<Point> _bezierControlPoints = new List<Point>();

	private int BezierPointCount => _bezierControlPoints.Count;

	internal bool ConstructBezierState(StylusPointCollection stylusPoints, double fitError)
	{
		if (stylusPoints == null || stylusPoints.Count == 0)
		{
			return false;
		}
		CuspData cuspData = new CuspData();
		cuspData.Analyze(stylusPoints, fitError);
		return ConstructFromData(cuspData, fitError);
	}

	internal List<Point> Flatten(double tolerance)
	{
		List<Point> list = new List<Point>();
		Vector bezierPoint = GetBezierPoint(0);
		list.Add(new Point(bezierPoint.X, bezierPoint.Y));
		int num = BezierPointCount - 4;
		if (0 <= num)
		{
			if (tolerance < 2.220446049250313E-16)
			{
				tolerance = 2.220446049250313E-16;
			}
			for (int i = 0; i <= num; i += 3)
			{
				FlattenSegment(i, tolerance, list);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Point value = list[j];
			value.X *= 24.0 / 635.0;
			value.Y *= 24.0 / 635.0;
			list[j] = value;
		}
		return list;
	}

	private bool ExtendingRange(double error, CuspData data, int from, int next_cusp, ref int to, ref bool cusp, ref bool done)
	{
		to++;
		cusp = true;
		done = to >= data.Count - 1;
		if (done)
		{
			to = data.Count - 1;
			cusp = true;
			return false;
		}
		cusp = to >= next_cusp;
		if (cusp)
		{
			to = next_cusp;
			return false;
		}
		int num = (to - from) / 4;
		int[] i = new int[5]
		{
			from,
			from + num,
			(to + from) / 2,
			to - num,
			to
		};
		return CoCubic(data, i, error);
	}

	private bool AddBezierSegment(CuspData data, int from, ref Vector tanStart, int to, ref Vector tanEnd)
	{
		switch (to - from)
		{
		case 1:
			AddLine(data, from, to);
			return true;
		case 2:
			AddParabola(data, from);
			return true;
		default:
			return AddLeastSquares(data, from, ref tanStart, to, ref tanEnd);
		}
	}

	private bool ConstructFromData(CuspData data, double fitError)
	{
		if (data.Count < 2)
		{
			return false;
		}
		AddBezierPoint(data.XY(0));
		if (data.Count == 3)
		{
			AddParabola(data, 0);
			return true;
		}
		if (data.Count == 2)
		{
			AddLine(data, 0, 1);
			return true;
		}
		if (2.220446049250313E-16 > fitError)
		{
			fitError = 0.029999999329447746 * (data.Distance() * (24.0 / 635.0));
		}
		data.SetTanLinks(0.5 * fitError);
		fitError *= fitError;
		bool done = false;
		int num = 0;
		int num2 = 0;
		int nPrevCusp = 0;
		bool cusp = true;
		Vector ptT = new Vector(0.0, 0.0);
		Vector ptT2 = new Vector(0.0, 0.0);
		int num3 = 0;
		while (!done)
		{
			if (cusp)
			{
				nPrevCusp = num2;
				num2 = data.GetNextCusp(num3);
				if (!data.Tangent(ref ptT2, num3, nPrevCusp, num2, bReverse: false, bIsCusp: true))
				{
					return false;
				}
			}
			else
			{
				ptT2.X = 0.0 - ptT.X;
				ptT2.Y = 0.0 - ptT.Y;
			}
			num = num3 + 3;
			while (ExtendingRange(fitError, data, num3, num2, ref num, ref cusp, ref done))
			{
			}
			if (!data.Tangent(ref ptT, num, nPrevCusp, num2, bReverse: true, cusp))
			{
				return false;
			}
			if (!AddBezierSegment(data, num3, ref ptT2, num, ref ptT))
			{
				return false;
			}
			num3 = num;
		}
		return true;
	}

	private void AddParabola(CuspData data, int from)
	{
		double num = (data.Node(from + 1) - data.Node(from)) / (data.Node(from + 2) - data.Node(from));
		double num2 = 1.0 - num;
		if (num < 0.001 || num2 < 0.001)
		{
			AddLine(data, from, from + 2);
			return;
		}
		double num3 = 1.0 / num;
		double num4 = 1.0 / num2;
		Vector vector = num3 * num4 * data.XY(from + 1);
		Vector point = 1.0 / 3.0 * (vector + (1.0 - num2 * num3) * data.XY(from) - num * num4 * data.XY(from + 1));
		AddBezierPoint(point);
		point = 1.0 / 3.0 * (vector - num2 * num3 * data.XY(from) + (1.0 - num * num4) * data.XY(from + 2));
		AddBezierPoint(point);
		AddSegmentPoint(data, from + 2);
	}

	private void AddLine(CuspData data, int from, int to)
	{
		AddBezierPoint((2.0 * data.XY(from) + data.XY(to)) * (1.0 / 3.0));
		AddBezierPoint((data.XY(from) + 2.0 * data.XY(to)) * (1.0 / 3.0));
		AddSegmentPoint(data, to);
	}

	private bool AddLeastSquares(CuspData data, int from, ref Vector V, int to, ref Vector W)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double num9 = 0.0;
		for (int i = checked(from + 1); i < to; i++)
		{
			double num10 = (data.Node(i) - data.Node(from)) / (data.Node(to) - data.Node(from));
			double num11 = num10 * num10;
			double num12 = 1.0 - num10;
			double num13 = num12 * num12;
			double num14 = num13 * num12;
			double num15 = 3.0 * num13 * num10;
			double num16 = 3.0 * num12 * num11;
			double num17 = num11 * num10;
			num += num15 * num15;
			num3 += num16 * num16;
			num2 += num15 * num16;
			num6 -= (num14 + num15) * num15;
			num7 -= (num16 + num17) * num15;
			num4 += num15 * (data.XY(i) * V);
			num8 -= (num14 + num15) * num16;
			num9 -= (num16 + num17) * num16;
			num5 += num16 * (data.XY(i) * W);
		}
		num2 *= V * W;
		num4 += V * data.XY(from) * num6 + V * data.XY(to) * num7;
		num5 += W * data.XY(from) * num8 + W * data.XY(to) * num9;
		double num18 = num4 * num3 - num5 * num2;
		double num19 = num5 * num - num4 * num2;
		double num20 = num * num3 - num2 * num2;
		bool flag = Math.Abs(num20) > Math.Abs(num18) * 2.220446049250313E-16 && Math.Abs(num20) > Math.Abs(num19) * 2.220446049250313E-16;
		if (flag)
		{
			num18 /= num20;
			num19 /= num20;
			flag = num18 > 1E-06 && num19 > 1E-06;
		}
		if (!flag)
		{
			num18 = (num19 = (data.Node(to) - data.Node(from)) / 3.0);
		}
		AddBezierPoint(data.XY(from) + num18 * V);
		AddBezierPoint(data.XY(to) + num19 * W);
		AddSegmentPoint(data, to);
		return true;
	}

	private static bool CoCubic(CuspData data, int[] i, double fitError)
	{
		double num = data.Node(i[4]) - data.Node(i[0]);
		double num2 = num / (data.Node(i[1]) - data.Node(i[0]));
		double num3 = num / (data.Node(i[2]) - data.Node(i[0]));
		double num4 = num / (data.Node(i[3]) - data.Node(i[0]));
		double num5 = num / (data.Node(i[2]) - data.Node(i[1]));
		double num6 = num / (data.Node(i[3]) - data.Node(i[1]));
		double num7 = num / (data.Node(i[4]) - data.Node(i[1]));
		double num8 = num / (data.Node(i[3]) - data.Node(i[2]));
		double num9 = num / (data.Node(i[4]) - data.Node(i[2]));
		double num10 = num / (data.Node(i[4]) - data.Node(i[3]));
		Vector vector = num2 * num3 * num4 * data.XY(i[0]) - num2 * num5 * num6 * num7 * data.XY(i[1]) + num3 * num5 * num8 * num9 * data.XY(i[2]) - num4 * num6 * num8 * num10 * data.XY(i[3]) + num7 * num9 * num10 * data.XY(i[4]);
		return vector * vector < fitError;
	}

	private void AddBezierPoint(Vector point)
	{
		_bezierControlPoints.Add((Point)point);
	}

	private void AddSegmentPoint(CuspData data, int index)
	{
		_bezierControlPoints.Add((Point)data.XY(index));
	}

	private Vector DeCasteljau(int iFirst, double t)
	{
		double num = 1.0 - t;
		Vector vector = num * GetBezierPoint(iFirst) + t * GetBezierPoint(iFirst + 1);
		Vector vector2 = num * GetBezierPoint(iFirst + 1) + t * GetBezierPoint(iFirst + 2);
		Vector vector3 = num * GetBezierPoint(iFirst + 2) + t * GetBezierPoint(iFirst + 3);
		vector = num * vector + t * vector2;
		vector2 = num * vector2 + t * vector3;
		return num * vector + t * vector2;
	}

	private void FlattenSegment(int iFirst, double tolerance, List<Point> points)
	{
		int num = 1;
		Vector[] array = new Vector[4];
		double num2 = 0.0;
		for (int i = checked(iFirst + 1); i <= checked(iFirst + 2); i++)
		{
			array[0] = (GetBezierPoint(i - 1) + GetBezierPoint(i + 1)) * 0.5 - GetBezierPoint(i);
			double length = array[0].Length;
			if (length > num2)
			{
				num2 = length;
			}
		}
		if (num2 <= 0.5 * tolerance)
		{
			Vector bezierPoint = GetBezierPoint(iFirst + 3);
			points.Add(new Point(bezierPoint.X, bezierPoint.Y));
			return;
		}
		num = (int)Math.Sqrt(num2 / tolerance) + 3;
		if (num > 1000)
		{
			num = 1000;
		}
		double num3 = 1.0 / (double)num;
		array[0] = GetBezierPoint(iFirst);
		for (int i = 1; i <= 3; i++)
		{
			array[i] = DeCasteljau(iFirst, (double)i * num3);
			points.Add(new Point(array[i].X, array[i].Y));
		}
		for (int i = 1; i <= 3; i++)
		{
			for (int j = 0; j <= 3 - i; j++)
			{
				array[j] = array[j + 1] - array[j];
			}
		}
		for (int i = 4; i <= num; i++)
		{
			for (int j = 1; j <= 3; j++)
			{
				array[j] += array[j - 1];
			}
			points.Add(new Point(array[3].X, array[3].Y));
		}
	}

	private Vector GetBezierPoint(int index)
	{
		return (Vector)_bezierControlPoints[index];
	}
}
