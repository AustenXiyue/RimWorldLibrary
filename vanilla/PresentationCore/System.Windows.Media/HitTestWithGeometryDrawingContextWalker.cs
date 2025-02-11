using System.Collections;

namespace System.Windows.Media;

internal class HitTestWithGeometryDrawingContextWalker : HitTestDrawingContextWalker
{
	private class ModifierNode
	{
	}

	private class TransformModifierNode : ModifierNode
	{
		public Transform _transform;

		public TransformModifierNode(Transform transform)
		{
			_transform = transform;
		}
	}

	private class ClipModifierNode : ModifierNode
	{
		public Geometry _clip;

		public ClipModifierNode(Geometry clip)
		{
			_clip = clip;
		}
	}

	private PathGeometry _geometry;

	private Stack _modifierStack;

	private Transform _currentTransform;

	private Geometry _currentClip;

	private IntersectionDetail _intersectionDetail;

	internal override bool IsHit
	{
		get
		{
			if (_intersectionDetail != IntersectionDetail.Empty)
			{
				return _intersectionDetail != IntersectionDetail.NotCalculated;
			}
			return false;
		}
	}

	internal override IntersectionDetail IntersectionDetail
	{
		get
		{
			if (_intersectionDetail == IntersectionDetail.NotCalculated)
			{
				return IntersectionDetail.Empty;
			}
			return _intersectionDetail;
		}
	}

	internal HitTestWithGeometryDrawingContextWalker(PathGeometry geometry)
	{
		_geometry = geometry;
		_currentTransform = null;
		_currentClip = null;
		_intersectionDetail = IntersectionDetail.NotCalculated;
	}

	public override void DrawGeometry(Brush brush, Pen pen, Geometry geometry)
	{
		if (geometry != null && !geometry.IsEmpty())
		{
			Geometry geometry2 = ((_currentTransform == null || _currentTransform.IsIdentity) ? geometry : geometry.GetTransformedCopy(_currentTransform));
			if (_currentClip != null)
			{
				geometry2 = Geometry.Combine(geometry2, _currentClip, GeometryCombineMode.Intersect, null);
			}
			if (brush != null)
			{
				AccumulateIntersectionDetail(geometry2.FillContainsWithDetail(_geometry));
			}
			if (pen != null && !_contains)
			{
				AccumulateIntersectionDetail(geometry2.StrokeContainsWithDetail(pen, _geometry));
			}
			if (_contains)
			{
				StopWalking();
			}
		}
	}

	public override void DrawGlyphRun(Brush foregroundBrush, GlyphRun glyphRun)
	{
		if (glyphRun != null)
		{
			Rect rect = glyphRun.ComputeInkBoundingBox();
			if (!rect.IsEmpty)
			{
				rect.Offset((Vector)glyphRun.BaselineOrigin);
				DrawGeometry(Brushes.Black, null, new RectangleGeometry(rect));
			}
		}
	}

	public override void PushClip(Geometry clipGeometry)
	{
		if (clipGeometry == null || (_currentClip != null && _currentClip.IsEmpty()))
		{
			clipGeometry = _currentClip;
		}
		else
		{
			if (_currentTransform != null && !_currentTransform.IsIdentity)
			{
				clipGeometry = clipGeometry.GetTransformedCopy(_currentTransform);
			}
			if (_currentClip != null)
			{
				clipGeometry = Geometry.Combine(_currentClip, clipGeometry, GeometryCombineMode.Intersect, null);
			}
		}
		PushModifierStack(new ClipModifierNode(_currentClip));
		_currentClip = clipGeometry;
	}

	public override void PushOpacityMask(Brush brush)
	{
		PushModifierStack(null);
	}

	public override void PushOpacity(double opacity)
	{
		PushModifierStack(null);
	}

	public override void PushTransform(Transform transform)
	{
		if (transform == null || transform.IsIdentity)
		{
			transform = _currentTransform;
		}
		else if (_currentTransform != null && !_currentTransform.IsIdentity)
		{
			transform = new MatrixTransform(transform.Value * _currentTransform.Value);
		}
		PushModifierStack(new TransformModifierNode(_currentTransform));
		_currentTransform = transform;
	}

	public override void PushGuidelineSet(GuidelineSet guidelines)
	{
		PushModifierStack(null);
	}

	internal override void PushGuidelineY1(double coordinate)
	{
		PushModifierStack(null);
	}

	internal override void PushGuidelineY2(double leadingCoordinate, double offsetToDrivenCoordinate)
	{
		PushModifierStack(null);
	}

	public override void Pop()
	{
		object obj = _modifierStack.Pop();
		if (obj is TransformModifierNode)
		{
			_currentTransform = ((TransformModifierNode)obj)._transform;
		}
		else if (obj is ClipModifierNode)
		{
			_currentClip = ((ClipModifierNode)obj)._clip;
		}
	}

	private void AccumulateIntersectionDetail(IntersectionDetail intersectionDetail)
	{
		if (_intersectionDetail == IntersectionDetail.NotCalculated)
		{
			_intersectionDetail = intersectionDetail;
		}
		else if (intersectionDetail == IntersectionDetail.FullyInside && _intersectionDetail != IntersectionDetail.FullyInside)
		{
			_intersectionDetail = IntersectionDetail.Intersects;
		}
		else if (intersectionDetail == IntersectionDetail.Empty && _intersectionDetail != IntersectionDetail.Empty)
		{
			_intersectionDetail = IntersectionDetail.Intersects;
		}
		else
		{
			_intersectionDetail = intersectionDetail;
		}
		if (_intersectionDetail == IntersectionDetail.FullyContains)
		{
			_contains = true;
		}
	}

	private void PushModifierStack(ModifierNode modifier)
	{
		if (_modifierStack == null)
		{
			_modifierStack = new Stack();
		}
		_modifierStack.Push(modifier);
	}
}
