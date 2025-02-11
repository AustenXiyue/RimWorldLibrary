using System.Collections.Generic;
using System.Windows.Media.Effects;
using MS.Internal;

namespace System.Windows.Media;

internal class BoundsDrawingContextWalker : DrawingContextWalker
{
	private enum PushType
	{
		Transform,
		Clip,
		Opacity,
		OpacityMask,
		Guidelines,
		BitmapEffect
	}

	private Rect _bounds;

	private Rect _clip;

	private bool _haveClip;

	private Matrix _transform;

	private Stack<PushType> _pushTypeStack;

	private Stack<Matrix> _transformStack;

	private Stack<Rect> _clipStack;

	public Rect Bounds => _bounds;

	public BoundsDrawingContextWalker()
	{
		_bounds = Rect.Empty;
		_transform = Matrix.Identity;
	}

	public override void DrawLine(Pen pen, Point point0, Point point1)
	{
		if (Pen.ContributesToBounds(pen))
		{
			Rect bounds = LineGeometry.GetBoundsHelper(pen, _transform, point0, point1, Matrix.Identity, Geometry.StandardFlatteningTolerance, ToleranceType.Absolute);
			AddTransformedBounds(ref bounds);
		}
	}

	public override void DrawRectangle(Brush brush, Pen pen, Rect rectangle)
	{
		if (brush != null || Pen.ContributesToBounds(pen))
		{
			Rect bounds = RectangleGeometry.GetBoundsHelper(pen, _transform, rectangle, 0.0, 0.0, Matrix.Identity, Geometry.StandardFlatteningTolerance, ToleranceType.Absolute);
			AddTransformedBounds(ref bounds);
		}
	}

	public override void DrawRoundedRectangle(Brush brush, Pen pen, Rect rectangle, double radiusX, double radiusY)
	{
		if (brush != null || Pen.ContributesToBounds(pen))
		{
			Rect bounds = RectangleGeometry.GetBoundsHelper(pen, _transform, rectangle, radiusX, radiusY, Matrix.Identity, Geometry.StandardFlatteningTolerance, ToleranceType.Absolute);
			AddTransformedBounds(ref bounds);
		}
	}

	public override void DrawEllipse(Brush brush, Pen pen, Point center, double radiusX, double radiusY)
	{
		if (brush != null || Pen.ContributesToBounds(pen))
		{
			Rect bounds = EllipseGeometry.GetBoundsHelper(pen, _transform, center, radiusX, radiusY, Matrix.Identity, Geometry.StandardFlatteningTolerance, ToleranceType.Absolute);
			AddTransformedBounds(ref bounds);
		}
	}

	public override void DrawGeometry(Brush brush, Pen pen, Geometry geometry)
	{
		if (geometry != null && (brush != null || Pen.ContributesToBounds(pen)))
		{
			Rect bounds = geometry.GetBoundsInternal(pen, _transform);
			AddTransformedBounds(ref bounds);
		}
	}

	public override void DrawImage(ImageSource imageSource, Rect rectangle)
	{
		if (imageSource != null)
		{
			AddBounds(ref rectangle);
		}
	}

	public override void DrawVideo(MediaPlayer video, Rect rectangle)
	{
		if (video != null)
		{
			AddBounds(ref rectangle);
		}
	}

	public override void DrawGlyphRun(Brush foregroundBrush, GlyphRun glyphRun)
	{
		if (foregroundBrush != null && glyphRun != null)
		{
			Rect bounds = glyphRun.ComputeInkBoundingBox();
			if (!bounds.IsEmpty)
			{
				bounds.Offset((Vector)glyphRun.BaselineOrigin);
				AddBounds(ref bounds);
			}
		}
	}

	public override void PushOpacityMask(Brush brush)
	{
		PushTypeStack(PushType.OpacityMask);
	}

	public override void PushClip(Geometry clipGeometry)
	{
		if (_haveClip)
		{
			if (_clipStack == null)
			{
				_clipStack = new Stack<Rect>(2);
			}
			_clipStack.Push(_clip);
		}
		PushTypeStack(PushType.Clip);
		if (clipGeometry != null)
		{
			if (!_haveClip)
			{
				_haveClip = true;
				_clip = clipGeometry.GetBoundsInternal(null, _transform);
			}
			else
			{
				_clip.Intersect(clipGeometry.GetBoundsInternal(null, _transform));
			}
		}
	}

	public override void PushOpacity(double opacity)
	{
		PushTypeStack(PushType.Opacity);
	}

	public override void PushTransform(Transform transform)
	{
		if (_transformStack == null)
		{
			_transformStack = new Stack<Matrix>(2);
		}
		_transformStack.Push(_transform);
		PushTypeStack(PushType.Transform);
		Matrix matrix = Matrix.Identity;
		if (transform != null && !transform.IsIdentity)
		{
			matrix = transform.Value;
		}
		_transform = matrix * _transform;
	}

	public override void PushGuidelineSet(GuidelineSet guidelines)
	{
		PushTypeStack(PushType.Guidelines);
	}

	internal override void PushGuidelineY1(double coordinate)
	{
		PushTypeStack(PushType.Guidelines);
	}

	internal override void PushGuidelineY2(double leadingCoordinate, double offsetToDrivenCoordinate)
	{
		PushTypeStack(PushType.Guidelines);
	}

	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	public override void PushEffect(BitmapEffect effect, BitmapEffectInput effectInput)
	{
		PushTypeStack(PushType.BitmapEffect);
	}

	public override void Pop()
	{
		switch (_pushTypeStack.Pop())
		{
		default:
			_ = 5;
			break;
		case PushType.Transform:
			_transform = _transformStack.Pop();
			break;
		case PushType.Clip:
			if (_clipStack != null && _clipStack.Count > 0)
			{
				_clip = _clipStack.Pop();
			}
			else
			{
				_haveClip = false;
			}
			break;
		}
	}

	private void AddBounds(ref Rect bounds)
	{
		if (!_transform.IsIdentity)
		{
			MatrixUtil.TransformRect(ref bounds, ref _transform);
		}
		AddTransformedBounds(ref bounds);
	}

	private void AddTransformedBounds(ref Rect bounds)
	{
		if (DoubleUtil.RectHasNaN(bounds))
		{
			bounds.X = double.NegativeInfinity;
			bounds.Y = double.NegativeInfinity;
			bounds.Width = double.PositiveInfinity;
			bounds.Height = double.PositiveInfinity;
		}
		if (_haveClip)
		{
			bounds.Intersect(_clip);
		}
		_bounds.Union(bounds);
	}

	private void PushTypeStack(PushType pushType)
	{
		if (_pushTypeStack == null)
		{
			_pushTypeStack = new Stack<PushType>(2);
		}
		_pushTypeStack.Push(pushType);
	}

	internal void ClearState()
	{
		_clip = Rect.Empty;
		_bounds = Rect.Empty;
		_haveClip = false;
		_transform = default(Matrix);
		_pushTypeStack = null;
		_transformStack = null;
		_clipStack = null;
	}
}
