using System.Collections.Generic;
using System.Windows.Media.Effects;

namespace System.Windows.Media;

internal class HitTestWithPointDrawingContextWalker : HitTestDrawingContextWalker
{
	private Point _point;

	private Stack<Point> _pointStack;

	private bool _currentLayerIsNoOp;

	private int _noOpLayerDepth;

	internal override bool IsHit => _contains;

	internal override IntersectionDetail IntersectionDetail
	{
		get
		{
			if (!_contains)
			{
				return IntersectionDetail.Empty;
			}
			return IntersectionDetail.FullyInside;
		}
	}

	private bool IsCurrentLayerNoOp
	{
		get
		{
			return _currentLayerIsNoOp;
		}
		set
		{
			if (value)
			{
				_currentLayerIsNoOp = true;
				_noOpLayerDepth++;
			}
			else
			{
				_currentLayerIsNoOp = false;
			}
		}
	}

	internal HitTestWithPointDrawingContextWalker(Point point)
	{
		_point = point;
	}

	public override void DrawGeometry(Brush brush, Pen pen, Geometry geometry)
	{
		if (!IsCurrentLayerNoOp && geometry != null && !geometry.IsEmpty())
		{
			if (brush != null)
			{
				_contains |= geometry.FillContains(_point);
			}
			if (pen != null && !_contains)
			{
				_contains |= geometry.StrokeContains(pen, _point);
			}
			if (_contains)
			{
				StopWalking();
			}
		}
	}

	public override void DrawGlyphRun(Brush foregroundBrush, GlyphRun glyphRun)
	{
		if (IsCurrentLayerNoOp || glyphRun == null)
		{
			return;
		}
		Rect rect = glyphRun.ComputeInkBoundingBox();
		if (!rect.IsEmpty)
		{
			rect.Offset((Vector)glyphRun.BaselineOrigin);
			_contains |= rect.Contains(_point);
			if (_contains)
			{
				StopWalking();
			}
		}
	}

	public override void PushClip(Geometry clipGeometry)
	{
		if (!IsPushNoOp())
		{
			PushPointStack(_point);
			if (clipGeometry != null && !clipGeometry.FillContains(_point))
			{
				IsCurrentLayerNoOp = true;
			}
		}
	}

	public override void PushOpacityMask(Brush brush)
	{
		if (!IsPushNoOp())
		{
			PushPointStack(_point);
		}
	}

	public override void PushOpacity(double opacity)
	{
		if (!IsPushNoOp())
		{
			PushPointStack(_point);
		}
	}

	public override void PushTransform(Transform transform)
	{
		if (IsPushNoOp())
		{
			return;
		}
		if (transform == null || transform.IsIdentity)
		{
			PushPointStack(_point);
			return;
		}
		Matrix value = transform.Value;
		if (value.HasInverse)
		{
			value.Invert();
			PushPointStack(_point * value);
		}
		else
		{
			IsCurrentLayerNoOp = true;
		}
	}

	public override void PushGuidelineSet(GuidelineSet guidelines)
	{
		if (!IsPushNoOp())
		{
			PushPointStack(_point);
		}
	}

	internal override void PushGuidelineY1(double coordinate)
	{
		if (!IsPushNoOp())
		{
			PushPointStack(_point);
		}
	}

	internal override void PushGuidelineY2(double leadingCoordinate, double offsetToDrivenCoordinate)
	{
		if (!IsPushNoOp())
		{
			PushPointStack(_point);
		}
	}

	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	public override void PushEffect(BitmapEffect effect, BitmapEffectInput effectInput)
	{
		if (!IsPushNoOp())
		{
			PushPointStack(_point);
		}
	}

	public override void Pop()
	{
		if (!IsPopNoOp())
		{
			PopPointStack();
		}
	}

	private void PushPointStack(Point point)
	{
		if (_pointStack == null)
		{
			_pointStack = new Stack<Point>(2);
		}
		_pointStack.Push(_point);
		_point = point;
	}

	private void PopPointStack()
	{
		_point = _pointStack.Pop();
	}

	private bool IsPushNoOp()
	{
		if (IsCurrentLayerNoOp)
		{
			_noOpLayerDepth++;
			return true;
		}
		return false;
	}

	private bool IsPopNoOp()
	{
		if (IsCurrentLayerNoOp)
		{
			_noOpLayerDepth--;
			if (_noOpLayerDepth == 0)
			{
				IsCurrentLayerNoOp = false;
			}
			return true;
		}
		return false;
	}
}
