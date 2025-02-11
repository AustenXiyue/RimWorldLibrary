using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace MS.Internal.Ink;

internal class CuspData
{
	private struct CDataPoint
	{
		public Point Point;

		public int Index;

		public int TanPrev;

		public int TanNext;
	}

	private List<CDataPoint> _points;

	private List<double> _nodes;

	private double _dist;

	private List<int> _cusps = new List<int>();

	private double _span = 3.0;

	internal int Count => _points.Count;

	internal CuspData()
	{
	}

	internal void Analyze(StylusPointCollection stylusPoints, double rSpan)
	{
		if (stylusPoints == null || stylusPoints.Count == 0)
		{
			return;
		}
		_points = new List<CDataPoint>(stylusPoints.Count);
		_nodes = new List<double>(stylusPoints.Count);
		_nodes.Add(0.0);
		CDataPoint item = default(CDataPoint);
		item.Index = 0;
		Point point = (Point)stylusPoints[0];
		point.X *= 26.458333333333332;
		point.Y *= 26.458333333333332;
		item.Point = point;
		_points.Add(item);
		int num = 0;
		for (int i = 1; i < stylusPoints.Count; i++)
		{
			if (!DoubleUtil.AreClose(stylusPoints[i].X, stylusPoints[i - 1].X) || !DoubleUtil.AreClose(stylusPoints[i].Y, stylusPoints[i - 1].Y))
			{
				num++;
				CDataPoint item2 = default(CDataPoint);
				item2.Index = num;
				Point point2 = (Point)stylusPoints[i];
				point2.X *= 26.458333333333332;
				point2.Y *= 26.458333333333332;
				item2.Point = point2;
				_points.Insert(num, item2);
				_nodes.Insert(num, _nodes[num - 1] + (XY(num) - XY(num - 1)).Length);
			}
		}
		SetLinks(rSpan);
	}

	internal void SetTanLinks(double rError)
	{
		int count = Count;
		if (rError < 1.0)
		{
			rError = 1.0;
		}
		for (int i = 0; i < count; i++)
		{
			for (int j = i + 1; j < count; j++)
			{
				if (_nodes[j] - _nodes[i] >= rError)
				{
					CDataPoint value = _points[i];
					value.TanNext = j;
					_points[i] = value;
					CDataPoint value2 = _points[j];
					value2.TanPrev = i;
					_points[j] = value2;
					break;
				}
			}
			if (0 > _points[i].TanPrev)
			{
				int num = i - 1;
				while (0 <= num)
				{
					if (_nodes[i] - _nodes[num] >= rError)
					{
						CDataPoint value3 = _points[i];
						value3.TanPrev = num;
						_points[i] = value3;
						break;
					}
					num--;
				}
			}
			if (0 > _points[i].TanNext)
			{
				CDataPoint value4 = _points[i];
				value4.TanNext = count - 1;
				_points[i] = value4;
			}
			if (0 > _points[i].TanPrev)
			{
				CDataPoint value5 = _points[i];
				value5.TanPrev = 0;
				_points[i] = value5;
			}
		}
	}

	internal int GetNextCusp(int iCurrent)
	{
		int num = Count - 1;
		if (iCurrent < 0)
		{
			return 0;
		}
		if (iCurrent >= num)
		{
			return num;
		}
		int num2 = 0;
		int num3 = _cusps.Count;
		int num4 = (num2 + num3) / 2;
		while (num2 < num4)
		{
			if (_cusps[num4] <= iCurrent)
			{
				num2 = num4;
			}
			else
			{
				num3 = num4;
			}
			num4 = (num2 + num3) / 2;
		}
		return _cusps[num4 + 1];
	}

	internal Vector XY(int i)
	{
		return new Vector(_points[i].Point.X, _points[i].Point.Y);
	}

	internal double Node(int i)
	{
		return _nodes[i];
	}

	internal int GetPointIndex(int nodeIndex)
	{
		return _points[nodeIndex].Index;
	}

	internal double Distance()
	{
		return _dist;
	}

	internal bool Tangent(ref Vector ptT, int nAt, int nPrevCusp, int nNextCusp, bool bReverse, bool bIsCusp)
	{
		if (bIsCusp)
		{
			int num;
			int num2;
			if (bReverse)
			{
				num = _points[nAt].TanPrev;
				if (num < nPrevCusp || 0 > num)
				{
					num2 = nPrevCusp;
					num = (num2 + nAt) / 2;
				}
				else
				{
					num2 = _points[num].TanPrev;
					if (num2 < nPrevCusp)
					{
						num2 = nPrevCusp;
					}
				}
			}
			else
			{
				num = _points[nAt].TanNext;
				if (num > nNextCusp || 0 > num)
				{
					num2 = nNextCusp;
					num = (num2 + nAt) / 2;
				}
				else
				{
					num2 = _points[num].TanNext;
					if (num2 > nNextCusp)
					{
						num2 = nNextCusp;
					}
				}
			}
			ptT = XY(num) + 0.5 * XY(num2) - 1.5 * XY(nAt);
		}
		else
		{
			int num = nAt;
			int num2 = _points[nAt].TanPrev;
			int num3;
			if (num2 < nPrevCusp)
			{
				num3 = nPrevCusp;
				num2 = (num3 + num) / 2;
			}
			else
			{
				num3 = _points[num2].TanPrev;
				if (num3 < nPrevCusp)
				{
					num3 = nPrevCusp;
				}
			}
			nAt = _points[nAt].TanNext;
			if (nAt > nNextCusp)
			{
				nAt = nNextCusp;
			}
			ptT = XY(num) + XY(num2) + 0.5 * XY(num3) - 2.5 * XY(nAt);
		}
		if (DoubleUtil.IsZero(ptT.LengthSquared))
		{
			return false;
		}
		ptT.Normalize();
		return true;
	}

	private double GetCurvature(int iPrev, int iCurrent, int iNext)
	{
		Vector vector = XY(iCurrent) - XY(iPrev);
		Vector vector2 = XY(iNext) - XY(iCurrent);
		double num = vector.Length * vector2.Length;
		if (DoubleUtil.IsZero(num))
		{
			return 0.0;
		}
		return 1.0 - vector * vector2 / num;
	}

	private void FindAllCusps()
	{
		_cusps.Clear();
		if (1 > Count)
		{
			return;
		}
		_cusps.Add(0);
		int iPrev = 0;
		int iNext = 0;
		int iPrevCusp = 0;
		if (!FindNextAndPrev(0, iPrevCusp, ref iPrev, ref iNext))
		{
			if (Count == 0)
			{
				_cusps.Clear();
			}
			else if (1 < Count)
			{
				_cusps.Add(iNext);
			}
			return;
		}
		int num = iNext;
		double num2 = 0.0;
		while (FindNextAndPrev(num, iPrevCusp, ref iPrev, ref iNext))
		{
			num2 = GetCurvature(iPrev, num, iNext);
			if (0.8 < num2)
			{
				double num3 = num2;
				int num4 = num;
				int iNext2 = 0;
				int iPrev2 = 0;
				if (!FindNextAndPrev(iNext, iPrevCusp, ref iPrev2, ref iNext2))
				{
					break;
				}
				for (int i = iPrev + 1; i <= iNext2 && FindNextAndPrev(i, iPrevCusp, ref iPrev, ref iNext); i++)
				{
					num2 = GetCurvature(iPrev, i, iNext);
					if (num2 > num3)
					{
						num3 = num2;
						num4 = i;
					}
				}
				_cusps.Add(num4);
				num = iNext2 + 1;
				iPrevCusp = num4;
			}
			else
			{
				num = ((!(0.035 > num2)) ? (num + 1) : iNext);
			}
		}
		_cusps.Add(Count - 1);
	}

	private bool FindNextAndPrev(int iPoint, int iPrevCusp, ref int iPrev, ref int iNext)
	{
		bool result = true;
		if (iPoint >= Count)
		{
			result = false;
			iPoint = Count - 1;
		}
		iNext = checked(iPoint + 1);
		while (iNext < Count && !(_nodes[iNext] - _nodes[iPoint] >= _span))
		{
			iNext++;
		}
		if (iNext >= Count)
		{
			result = false;
			iNext = Count - 1;
		}
		iPrev = checked(iPoint - 1);
		while (iPrevCusp <= iPrev && !(_nodes[iPoint] - _nodes[iPrev] >= _span))
		{
			iPrev--;
		}
		if (iPrev < 0)
		{
			iPrev = 0;
		}
		return result;
	}

	private static void UpdateMinMax(double a, ref double rMin, ref double rMax)
	{
		rMin = Math.Min(rMin, a);
		rMax = Math.Max(a, rMax);
	}

	private void SetLinks(double rSpan)
	{
		int count = Count;
		if (2 <= count)
		{
			double rMin = XY(0).X;
			double rMin2 = XY(0).Y;
			double rMax = rMin;
			double rMax2 = rMin2;
			for (int i = 0; i < count; i++)
			{
				UpdateMinMax(XY(i).X, ref rMin, ref rMax);
				UpdateMinMax(XY(i).Y, ref rMin2, ref rMax2);
			}
			rMax -= rMin;
			rMax2 -= rMin2;
			_dist = Math.Abs(rMax) + Math.Abs(rMax2);
			if (!DoubleUtil.IsZero(rSpan))
			{
				_span = rSpan;
			}
			else if (0.0 < _dist)
			{
				_span = 0.75 * (_nodes[count - 1] * _nodes[count - 1]) / ((double)count * _dist);
			}
			if (_span < 1.0)
			{
				_span = 1.0;
			}
			FindAllCusps();
		}
	}
}
