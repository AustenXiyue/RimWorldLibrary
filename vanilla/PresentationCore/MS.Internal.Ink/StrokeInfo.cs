using System;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;

namespace MS.Internal.Ink;

internal class StrokeInfo
{
	private Stroke _stroke;

	private Rect _bounds;

	private double _hitWeight;

	private bool _isHit;

	private bool _isDirty = true;

	private StylusPointCollection _stylusPoints;

	private double _totalWeight;

	private bool _totalWeightCached;

	internal Stroke Stroke => _stroke;

	internal Rect StrokeBounds => _bounds;

	internal bool IsDirty
	{
		get
		{
			return _isDirty;
		}
		set
		{
			_isDirty = value;
		}
	}

	internal bool IsHit
	{
		get
		{
			return _isHit;
		}
		set
		{
			_isHit = value;
		}
	}

	internal StylusPointCollection StylusPoints
	{
		get
		{
			if (_stylusPoints == null)
			{
				if (_stroke.DrawingAttributes.FitToCurve)
				{
					_stylusPoints = _stroke.GetBezierStylusPoints();
				}
				else
				{
					_stylusPoints = _stroke.StylusPoints;
				}
			}
			return _stylusPoints;
		}
	}

	internal double HitWeight
	{
		get
		{
			return _hitWeight;
		}
		set
		{
			if (DoubleUtil.GreaterThan(value, TotalWeight))
			{
				_hitWeight = TotalWeight;
			}
			else if (DoubleUtil.LessThan(value, 0.0))
			{
				_hitWeight = 0.0;
			}
			else
			{
				_hitWeight = value;
			}
		}
	}

	internal double TotalWeight
	{
		get
		{
			if (!_totalWeightCached)
			{
				_totalWeight = 0.0;
				for (int i = 0; i < StylusPoints.Count; i++)
				{
					_totalWeight += GetPointWeight(i);
				}
				_totalWeightCached = true;
			}
			return _totalWeight;
		}
	}

	internal StrokeInfo(Stroke stroke)
	{
		_stroke = stroke;
		_bounds = stroke.GetBounds();
		_stroke.DrawingAttributesChanged += OnStrokeDrawingAttributesChanged;
		_stroke.StylusPointsReplaced += OnStylusPointsReplaced;
		_stroke.StylusPoints.Changed += OnStylusPointsChanged;
		_stroke.DrawingAttributesReplaced += OnDrawingAttributesReplaced;
	}

	internal double GetPointWeight(int index)
	{
		StylusPointCollection stylusPoints = StylusPoints;
		DrawingAttributes drawingAttributes = Stroke.DrawingAttributes;
		double num = 0.0;
		num = ((index != 0) ? (num + Math.Sqrt(((Point)stylusPoints[index] - (Point)stylusPoints[index - 1]).LengthSquared) / 2.0) : (num + Math.Sqrt(drawingAttributes.Width * drawingAttributes.Width + drawingAttributes.Height * drawingAttributes.Height) / 2.0));
		if (index == stylusPoints.Count - 1)
		{
			return num + Math.Sqrt(drawingAttributes.Width * drawingAttributes.Width + drawingAttributes.Height * drawingAttributes.Height) / 2.0;
		}
		return num + Math.Sqrt(((Point)stylusPoints[index + 1] - (Point)stylusPoints[index]).LengthSquared) / 2.0;
	}

	internal void Detach()
	{
		if (_stroke != null)
		{
			_stroke.DrawingAttributesChanged -= OnStrokeDrawingAttributesChanged;
			_stroke.StylusPointsReplaced -= OnStylusPointsReplaced;
			_stroke.StylusPoints.Changed -= OnStylusPointsChanged;
			_stroke.DrawingAttributesReplaced -= OnDrawingAttributesReplaced;
			_stroke = null;
		}
	}

	private void OnStylusPointsChanged(object sender, EventArgs args)
	{
		Invalidate();
	}

	private void OnStylusPointsReplaced(object sender, StylusPointsReplacedEventArgs args)
	{
		Invalidate();
	}

	private void OnStrokeDrawingAttributesChanged(object sender, PropertyDataChangedEventArgs args)
	{
		if (DrawingAttributes.IsGeometricalDaGuid(args.PropertyGuid))
		{
			Invalidate();
		}
	}

	private void OnDrawingAttributesReplaced(object sender, DrawingAttributesReplacedEventArgs args)
	{
		if (!DrawingAttributes.GeometricallyEqual(args.NewDrawingAttributes, args.PreviousDrawingAttributes))
		{
			Invalidate();
		}
	}

	private void Invalidate()
	{
		_totalWeightCached = false;
		_stylusPoints = null;
		_hitWeight = 0.0;
		_isDirty = true;
		_bounds = _stroke.GetBounds();
	}
}
