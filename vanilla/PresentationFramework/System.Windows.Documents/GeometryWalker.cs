using System.Collections.Generic;
using System.Windows.Media;

namespace System.Windows.Documents;

internal sealed class GeometryWalker : CapacityStreamGeometryContext
{
	private FixedSOMPageConstructor _pageConstructor;

	private Matrix _transform;

	private bool _stroke;

	private bool _fill;

	private Point _startPoint;

	private Point _lastPoint;

	private bool _isClosed;

	private bool _isFilled;

	private double _xMin;

	private double _xMax;

	private double _yMin;

	private double _yMax;

	private bool _needClose;

	public GeometryWalker(FixedSOMPageConstructor pageConstructor)
	{
		_pageConstructor = pageConstructor;
	}

	public void FindLines(StreamGeometry geometry, bool stroke, bool fill, Matrix trans)
	{
		_transform = trans;
		_fill = fill;
		_stroke = stroke;
		PathGeometry.ParsePathGeometryData(geometry.GetPathGeometryData(), this);
		CheckCloseFigure();
	}

	private void CheckCloseFigure()
	{
		if (_needClose)
		{
			if (_stroke && _isClosed)
			{
				_pageConstructor._AddLine(_startPoint, _lastPoint, _transform);
			}
			if (_fill && _isFilled)
			{
				_pageConstructor._ProcessFilledRect(_transform, new Rect(_xMin, _yMin, _xMax - _xMin, _yMax - _yMin));
			}
			_needClose = false;
		}
	}

	private void GatherBounds(Point p)
	{
		if (p.X < _xMin)
		{
			_xMin = p.X;
		}
		else if (p.X > _xMax)
		{
			_xMax = p.X;
		}
		if (p.Y < _yMin)
		{
			_yMin = p.Y;
		}
		else if (p.Y > _yMax)
		{
			_yMax = p.Y;
		}
	}

	public override void BeginFigure(Point startPoint, bool isFilled, bool isClosed)
	{
		CheckCloseFigure();
		_startPoint = startPoint;
		_lastPoint = startPoint;
		_isClosed = isClosed;
		_isFilled = isFilled;
		if (_isFilled && _fill)
		{
			_xMin = (_xMax = startPoint.X);
			_yMin = (_yMax = startPoint.Y);
		}
	}

	public override void LineTo(Point point, bool isStroked, bool isSmoothJoin)
	{
		if (isStroked && _stroke)
		{
			_pageConstructor._AddLine(_lastPoint, point, _transform);
		}
		if (_isFilled && _fill)
		{
			GatherBounds(point);
		}
		_lastPoint = point;
	}

	public override void QuadraticBezierTo(Point point1, Point point2, bool isStroked, bool isSmoothJoin)
	{
		_lastPoint = point2;
		_fill = false;
	}

	public override void BezierTo(Point point1, Point point2, Point point3, bool isStroked, bool isSmoothJoin)
	{
		_lastPoint = point3;
		_fill = false;
	}

	public override void PolyLineTo(IList<Point> points, bool isStroked, bool isSmoothJoin)
	{
		if (isStroked && _stroke)
		{
			for (int i = 0; i < points.Count; i++)
			{
				_pageConstructor._AddLine(_lastPoint, points[i], _transform);
				_lastPoint = points[i];
			}
		}
		else
		{
			_lastPoint = points[points.Count - 1];
		}
		if (_isFilled && _fill)
		{
			for (int j = 0; j < points.Count; j++)
			{
				GatherBounds(points[j]);
			}
		}
	}

	public override void PolyQuadraticBezierTo(IList<Point> points, bool isStroked, bool isSmoothJoin)
	{
		_lastPoint = points[points.Count - 1];
		_fill = false;
	}

	public override void PolyBezierTo(IList<Point> points, bool isStroked, bool isSmoothJoin)
	{
		_lastPoint = points[points.Count - 1];
		_fill = false;
	}

	public override void ArcTo(Point point, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection, bool isStroked, bool isSmoothJoin)
	{
		_lastPoint = point;
		_fill = false;
	}

	internal override void SetClosedState(bool closed)
	{
	}

	internal override void SetFigureCount(int figureCount)
	{
	}

	internal override void SetSegmentCount(int segmentCount)
	{
		if (segmentCount != 0)
		{
			_needClose = true;
		}
	}
}
