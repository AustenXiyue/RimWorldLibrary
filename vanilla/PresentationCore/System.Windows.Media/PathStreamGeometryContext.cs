using System.Collections.Generic;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

internal class PathStreamGeometryContext : CapacityStreamGeometryContext
{
	private PathGeometry _pathGeometry;

	private PathFigureCollection _figures;

	private PathFigure _currentFigure;

	private PathSegmentCollection _segments;

	private bool _currentIsClosed;

	private MIL_SEGMENT_TYPE _currentSegmentType;

	private PointCollection _currentSegmentPoints;

	private bool _currentSegmentIsStroked;

	private bool _currentSegmentIsSmoothJoin;

	private static FillRule s_defaultFillRule;

	private static bool s_defaultValueForPathFigureIsClosed;

	private static bool s_defaultValueForPathFigureIsFilled;

	private static Point s_defaultValueForPathFigureStartPoint;

	private static bool s_defaultValueForPathSegmentIsStroked;

	private static bool s_defaultValueForPathSegmentIsSmoothJoin;

	private static bool s_defaultValueForArcSegmentIsLargeArc;

	private static SweepDirection s_defaultValueForArcSegmentSweepDirection;

	private static double s_defaultValueForArcSegmentRotationAngle;

	static PathStreamGeometryContext()
	{
		s_defaultFillRule = (FillRule)PathGeometry.FillRuleProperty.GetDefaultValue(typeof(PathGeometry));
		s_defaultValueForPathFigureIsClosed = (bool)PathFigure.IsClosedProperty.GetDefaultValue(typeof(PathFigure));
		s_defaultValueForPathFigureIsFilled = (bool)PathFigure.IsFilledProperty.GetDefaultValue(typeof(PathFigure));
		s_defaultValueForPathFigureStartPoint = (Point)PathFigure.StartPointProperty.GetDefaultValue(typeof(PathFigure));
		s_defaultValueForPathSegmentIsStroked = (bool)PathSegment.IsStrokedProperty.GetDefaultValue(typeof(PathSegment));
		s_defaultValueForPathSegmentIsSmoothJoin = (bool)PathSegment.IsSmoothJoinProperty.GetDefaultValue(typeof(PathSegment));
		s_defaultValueForArcSegmentIsLargeArc = (bool)ArcSegment.IsLargeArcProperty.GetDefaultValue(typeof(ArcSegment));
		s_defaultValueForArcSegmentSweepDirection = (SweepDirection)ArcSegment.SweepDirectionProperty.GetDefaultValue(typeof(ArcSegment));
		s_defaultValueForArcSegmentRotationAngle = (double)ArcSegment.RotationAngleProperty.GetDefaultValue(typeof(ArcSegment));
	}

	internal PathStreamGeometryContext()
	{
		_pathGeometry = new PathGeometry();
	}

	internal PathStreamGeometryContext(FillRule fillRule, Transform transform)
	{
		_pathGeometry = new PathGeometry();
		if (fillRule != s_defaultFillRule)
		{
			_pathGeometry.FillRule = fillRule;
		}
		if (transform != null && !transform.IsIdentity)
		{
			_pathGeometry.Transform = transform.Clone();
		}
	}

	internal override void SetFigureCount(int figureCount)
	{
		_figures = new PathFigureCollection(figureCount);
		_pathGeometry.Figures = _figures;
	}

	internal override void SetSegmentCount(int segmentCount)
	{
		_segments = new PathSegmentCollection(segmentCount);
		_currentFigure.Segments = _segments;
	}

	internal override void SetClosedState(bool isClosed)
	{
		if (isClosed != _currentIsClosed)
		{
			_currentFigure.IsClosed = isClosed;
			_currentIsClosed = isClosed;
		}
	}

	public override void BeginFigure(Point startPoint, bool isFilled, bool isClosed)
	{
		if (_currentFigure == null && _figures == null)
		{
			_figures = new PathFigureCollection();
			_pathGeometry.Figures = _figures;
		}
		FinishSegment();
		_segments = null;
		_currentFigure = new PathFigure();
		_currentIsClosed = isClosed;
		if (startPoint != s_defaultValueForPathFigureStartPoint)
		{
			_currentFigure.StartPoint = startPoint;
		}
		if (isClosed != s_defaultValueForPathFigureIsClosed)
		{
			_currentFigure.IsClosed = isClosed;
		}
		if (isFilled != s_defaultValueForPathFigureIsFilled)
		{
			_currentFigure.IsFilled = isFilled;
		}
		_figures.Add(_currentFigure);
		_currentSegmentType = MIL_SEGMENT_TYPE.MilSegmentNone;
	}

	public override void LineTo(Point point, bool isStroked, bool isSmoothJoin)
	{
		PrepareToAddPoints(1, isStroked, isSmoothJoin, MIL_SEGMENT_TYPE.MilSegmentPolyLine);
		_currentSegmentPoints.Add(point);
	}

	public override void QuadraticBezierTo(Point point1, Point point2, bool isStroked, bool isSmoothJoin)
	{
		PrepareToAddPoints(2, isStroked, isSmoothJoin, MIL_SEGMENT_TYPE.MilSegmentPolyQuadraticBezier);
		_currentSegmentPoints.Add(point1);
		_currentSegmentPoints.Add(point2);
	}

	public override void BezierTo(Point point1, Point point2, Point point3, bool isStroked, bool isSmoothJoin)
	{
		PrepareToAddPoints(3, isStroked, isSmoothJoin, MIL_SEGMENT_TYPE.MilSegmentPolyBezier);
		_currentSegmentPoints.Add(point1);
		_currentSegmentPoints.Add(point2);
		_currentSegmentPoints.Add(point3);
	}

	public override void PolyLineTo(IList<Point> points, bool isStroked, bool isSmoothJoin)
	{
		GenericPolyTo(points, isStroked, isSmoothJoin, MIL_SEGMENT_TYPE.MilSegmentPolyLine);
	}

	public override void PolyQuadraticBezierTo(IList<Point> points, bool isStroked, bool isSmoothJoin)
	{
		GenericPolyTo(points, isStroked, isSmoothJoin, MIL_SEGMENT_TYPE.MilSegmentPolyQuadraticBezier);
	}

	public override void PolyBezierTo(IList<Point> points, bool isStroked, bool isSmoothJoin)
	{
		GenericPolyTo(points, isStroked, isSmoothJoin, MIL_SEGMENT_TYPE.MilSegmentPolyBezier);
	}

	public override void ArcTo(Point point, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection, bool isStroked, bool isSmoothJoin)
	{
		FinishSegment();
		if (_segments == null)
		{
			_segments = new PathSegmentCollection();
			_currentFigure.Segments = _segments;
		}
		ArcSegment arcSegment = new ArcSegment();
		arcSegment.Point = point;
		arcSegment.Size = size;
		if (isLargeArc != s_defaultValueForArcSegmentIsLargeArc)
		{
			arcSegment.IsLargeArc = isLargeArc;
		}
		if (sweepDirection != s_defaultValueForArcSegmentSweepDirection)
		{
			arcSegment.SweepDirection = sweepDirection;
		}
		if (rotationAngle != s_defaultValueForArcSegmentRotationAngle)
		{
			arcSegment.RotationAngle = rotationAngle;
		}
		if (isStroked != s_defaultValueForPathSegmentIsStroked)
		{
			arcSegment.IsStroked = isStroked;
		}
		if (isSmoothJoin != s_defaultValueForPathSegmentIsSmoothJoin)
		{
			arcSegment.IsSmoothJoin = isSmoothJoin;
		}
		_segments.Add(arcSegment);
		_currentSegmentType = MIL_SEGMENT_TYPE.MilSegmentArc;
	}

	public override void Close()
	{
	}

	internal PathGeometry GetPathGeometry()
	{
		FinishSegment();
		return _pathGeometry;
	}

	private void GenericPolyTo(IList<Point> points, bool isStroked, bool isSmoothJoin, MIL_SEGMENT_TYPE segmentType)
	{
		int count = points.Count;
		PrepareToAddPoints(count, isStroked, isSmoothJoin, segmentType);
		for (int i = 0; i < count; i++)
		{
			_currentSegmentPoints.Add(points[i]);
		}
	}

	private void PrepareToAddPoints(int count, bool isStroked, bool isSmoothJoin, MIL_SEGMENT_TYPE segmentType)
	{
		if (_currentSegmentType != segmentType || _currentSegmentIsStroked != isStroked || _currentSegmentIsSmoothJoin != isSmoothJoin)
		{
			FinishSegment();
			_currentSegmentType = segmentType;
			_currentSegmentIsStroked = isStroked;
			_currentSegmentIsSmoothJoin = isSmoothJoin;
		}
		if (_currentSegmentPoints == null)
		{
			_currentSegmentPoints = new PointCollection();
		}
	}

	private void FinishSegment()
	{
		if (_currentSegmentPoints != null)
		{
			int count = _currentSegmentPoints.Count;
			if (_segments == null)
			{
				_segments = new PathSegmentCollection();
				_currentFigure.Segments = _segments;
			}
			PathSegment pathSegment = _currentSegmentType switch
			{
				MIL_SEGMENT_TYPE.MilSegmentPolyLine => (count != 1) ? ((PathSegment)new PolyLineSegment
				{
					Points = _currentSegmentPoints
				}) : ((PathSegment)new LineSegment
				{
					Point = _currentSegmentPoints[0]
				}), 
				MIL_SEGMENT_TYPE.MilSegmentPolyBezier => (count != 3) ? ((PathSegment)new PolyBezierSegment
				{
					Points = _currentSegmentPoints
				}) : ((PathSegment)new BezierSegment
				{
					Point1 = _currentSegmentPoints[0],
					Point2 = _currentSegmentPoints[1],
					Point3 = _currentSegmentPoints[2]
				}), 
				MIL_SEGMENT_TYPE.MilSegmentPolyQuadraticBezier => (count != 2) ? ((PathSegment)new PolyQuadraticBezierSegment
				{
					Points = _currentSegmentPoints
				}) : ((PathSegment)new QuadraticBezierSegment
				{
					Point1 = _currentSegmentPoints[0],
					Point2 = _currentSegmentPoints[1]
				}), 
				_ => null, 
			};
			if (_currentSegmentIsStroked != s_defaultValueForPathSegmentIsStroked)
			{
				pathSegment.IsStroked = _currentSegmentIsStroked;
			}
			if (_currentSegmentIsSmoothJoin != s_defaultValueForPathSegmentIsSmoothJoin)
			{
				pathSegment.IsSmoothJoin = _currentSegmentIsSmoothJoin;
			}
			_segments.Add(pathSegment);
			_currentSegmentPoints = null;
			_currentSegmentType = MIL_SEGMENT_TYPE.MilSegmentNone;
		}
	}
}
