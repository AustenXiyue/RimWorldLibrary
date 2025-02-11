using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace MS.Internal.Ink;

internal class LassoHelper
{
	private DrawingVisual _containerVisual;

	private Brush _brush;

	private Pen _pen;

	private bool _isActivated;

	private Point _firstLassoPoint;

	private Point _lastLassoPoint;

	private int _count;

	private List<Point> _lasso;

	private Rect _boundingBox;

	public const double MinDistanceSquared = 49.0;

	private const double DotRadius = 2.5;

	private const double DotCircumferenceThickness = 0.5;

	private const double ConnectLineThickness = 0.75;

	private const double ConnectLineOpacity = 0.75;

	private static readonly Color DotColor = Colors.Orange;

	private static readonly Color DotCircumferenceColor = Colors.White;

	public Visual Visual
	{
		get
		{
			EnsureVisual();
			return _containerVisual;
		}
	}

	public Point[] AddPoints(List<Point> points)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		EnsureReady();
		List<Point> list = new List<Point>();
		int count = points.Count;
		for (int i = 0; i < count; i++)
		{
			Point point = points[i];
			if (_count == 0)
			{
				AddLassoPoint(point);
				list.Add(point);
				_lasso.Add(point);
				_boundingBox.Union(point);
				_firstLassoPoint = point;
				_lastLassoPoint = point;
				_count++;
				continue;
			}
			Vector vector = point - _lastLassoPoint;
			double lengthSquared = vector.LengthSquared;
			if (DoubleUtil.AreClose(49.0, lengthSquared))
			{
				AddLassoPoint(point);
				list.Add(point);
				_lasso.Add(point);
				_boundingBox.Union(point);
				_lastLassoPoint = point;
				_count++;
			}
			else if (49.0 < lengthSquared)
			{
				double num = Math.Sqrt(49.0 / lengthSquared);
				Point lastLassoPoint = _lastLassoPoint;
				for (double num2 = num; num2 < 1.0; num2 += num)
				{
					Point point2 = lastLassoPoint + vector * num2;
					AddLassoPoint(point2);
					list.Add(point2);
					_lasso.Add(point2);
					_boundingBox.Union(point2);
					_lastLassoPoint = point2;
					_count++;
				}
			}
		}
		return list.ToArray();
	}

	private void AddLassoPoint(Point lassoPoint)
	{
		DrawingVisual drawingVisual = new DrawingVisual();
		DrawingContext drawingContext = null;
		try
		{
			drawingContext = drawingVisual.RenderOpen();
			drawingContext.DrawEllipse(_brush, _pen, lassoPoint, 2.5, 2.5);
		}
		finally
		{
			drawingContext?.Close();
		}
		_containerVisual.Children.Add(drawingVisual);
	}

	public bool ArePointsInLasso(Point[] points, int percentIntersect)
	{
		int num = points.Length * percentIntersect / 100;
		if (num == 0 || 50 <= points.Length * percentIntersect % 100)
		{
			num++;
		}
		int num2 = 0;
		foreach (Point point in points)
		{
			if (Contains(point))
			{
				num2++;
				if (num2 == num)
				{
					break;
				}
			}
		}
		return num2 == num;
	}

	private bool Contains(Point point)
	{
		if (!_boundingBox.Contains(point))
		{
			return false;
		}
		bool flag = false;
		int num = _lasso.Count;
		while (--num >= 0)
		{
			if (!DoubleUtil.AreClose(_lasso[num].Y, point.Y))
			{
				flag = point.Y < _lasso[num].Y;
				break;
			}
		}
		bool flag2 = false;
		bool flag3 = false;
		Point point2 = _lasso[_lasso.Count - 1];
		for (int i = 0; i < _lasso.Count; i++)
		{
			Point point3 = _lasso[i];
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
					if (i == 0 && DoubleUtil.AreClose(point.X, Math.Max(point2.X, point3.X)))
					{
						flag3 = true;
					}
				}
				else if (DoubleUtil.GreaterThanOrClose(point.X, Math.Min(point2.X, point3.X)))
				{
					Vector vector = point3 - point2;
					double value = point2.X + vector.X / vector.Y * (point.Y - point2.Y);
					if (DoubleUtil.GreaterThanOrClose(point.X, value))
					{
						flag2 = !flag2;
						if (i == 0 && DoubleUtil.AreClose(point.X, value))
						{
							flag3 = true;
						}
					}
				}
			}
			point2 = point3;
		}
		if (!flag2)
		{
			return false;
		}
		return !flag3;
	}

	private void EnsureVisual()
	{
		if (_containerVisual == null)
		{
			_containerVisual = new DrawingVisual();
		}
	}

	private void EnsureReady()
	{
		if (!_isActivated)
		{
			_isActivated = true;
			EnsureVisual();
			_brush = new SolidColorBrush(DotColor);
			_brush.Freeze();
			_pen = new Pen(new SolidColorBrush(DotCircumferenceColor), 0.5);
			_pen.LineJoin = PenLineJoin.Round;
			_pen.Freeze();
			_lasso = new List<Point>(100);
			_boundingBox = Rect.Empty;
			_count = 0;
		}
	}
}
