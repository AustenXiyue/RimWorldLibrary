using System.Collections.Generic;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

internal class DrawingDrawingContext : DrawingContext
{
	protected Drawing _rootDrawing;

	protected DrawingGroup _currentDrawingGroup;

	private Stack<DrawingGroup> _previousDrawingGroupStack;

	private bool _disposed;

	private bool _canBeInheritanceContext = true;

	internal bool CanBeInheritanceContext
	{
		get
		{
			return _canBeInheritanceContext;
		}
		set
		{
			_canBeInheritanceContext = value;
		}
	}

	internal DrawingDrawingContext()
	{
	}

	public override void DrawLine(Pen pen, Point point0, Point point1)
	{
		DrawLine(pen, point0, null, point1, null);
	}

	public override void DrawLine(Pen pen, Point point0, AnimationClock point0Animations, Point point1, AnimationClock point1Animations)
	{
		VerifyApiNonstructuralChange();
		if (pen != null)
		{
			LineGeometry lineGeometry = new LineGeometry(point0, point1);
			lineGeometry.CanBeInheritanceContext = CanBeInheritanceContext;
			SetupNewFreezable(lineGeometry, point0Animations == null && point1Animations == null);
			if (point0Animations != null)
			{
				lineGeometry.ApplyAnimationClock(LineGeometry.StartPointProperty, point0Animations);
			}
			if (point1Animations != null)
			{
				lineGeometry.ApplyAnimationClock(LineGeometry.EndPointProperty, point1Animations);
			}
			AddNewGeometryDrawing(null, pen, lineGeometry);
		}
	}

	public override void DrawRectangle(Brush brush, Pen pen, Rect rectangle)
	{
		DrawRectangle(brush, pen, rectangle, null);
	}

	public override void DrawRectangle(Brush brush, Pen pen, Rect rectangle, AnimationClock rectangleAnimations)
	{
		VerifyApiNonstructuralChange();
		if (brush != null || pen != null)
		{
			RectangleGeometry rectangleGeometry = new RectangleGeometry(rectangle);
			rectangleGeometry.CanBeInheritanceContext = CanBeInheritanceContext;
			SetupNewFreezable(rectangleGeometry, rectangleAnimations == null);
			if (rectangleAnimations != null)
			{
				rectangleGeometry.ApplyAnimationClock(RectangleGeometry.RectProperty, rectangleAnimations);
			}
			AddNewGeometryDrawing(brush, pen, rectangleGeometry);
		}
	}

	public override void DrawRoundedRectangle(Brush brush, Pen pen, Rect rectangle, double radiusX, double radiusY)
	{
		DrawRoundedRectangle(brush, pen, rectangle, null, radiusX, null, radiusY, null);
	}

	public override void DrawRoundedRectangle(Brush brush, Pen pen, Rect rectangle, AnimationClock rectangleAnimations, double radiusX, AnimationClock radiusXAnimations, double radiusY, AnimationClock radiusYAnimations)
	{
		VerifyApiNonstructuralChange();
		if (brush != null || pen != null)
		{
			RectangleGeometry rectangleGeometry = new RectangleGeometry(rectangle, radiusX, radiusY);
			rectangleGeometry.CanBeInheritanceContext = CanBeInheritanceContext;
			SetupNewFreezable(rectangleGeometry, rectangleAnimations == null && radiusXAnimations == null && radiusYAnimations == null);
			if (rectangleAnimations != null)
			{
				rectangleGeometry.ApplyAnimationClock(RectangleGeometry.RectProperty, rectangleAnimations);
			}
			if (radiusXAnimations != null)
			{
				rectangleGeometry.ApplyAnimationClock(RectangleGeometry.RadiusXProperty, radiusXAnimations);
			}
			if (radiusYAnimations != null)
			{
				rectangleGeometry.ApplyAnimationClock(RectangleGeometry.RadiusYProperty, radiusYAnimations);
			}
			AddNewGeometryDrawing(brush, pen, rectangleGeometry);
		}
	}

	public override void DrawEllipse(Brush brush, Pen pen, Point center, double radiusX, double radiusY)
	{
		DrawEllipse(brush, pen, center, null, radiusX, null, radiusY, null);
	}

	public override void DrawEllipse(Brush brush, Pen pen, Point center, AnimationClock centerAnimations, double radiusX, AnimationClock radiusXAnimations, double radiusY, AnimationClock radiusYAnimations)
	{
		VerifyApiNonstructuralChange();
		if (brush != null || pen != null)
		{
			EllipseGeometry ellipseGeometry = new EllipseGeometry(center, radiusX, radiusY);
			ellipseGeometry.CanBeInheritanceContext = CanBeInheritanceContext;
			SetupNewFreezable(ellipseGeometry, centerAnimations == null && radiusXAnimations == null && radiusYAnimations == null);
			if (centerAnimations != null)
			{
				ellipseGeometry.ApplyAnimationClock(EllipseGeometry.CenterProperty, centerAnimations);
			}
			if (radiusXAnimations != null)
			{
				ellipseGeometry.ApplyAnimationClock(EllipseGeometry.RadiusXProperty, radiusXAnimations);
			}
			if (radiusYAnimations != null)
			{
				ellipseGeometry.ApplyAnimationClock(EllipseGeometry.RadiusYProperty, radiusYAnimations);
			}
			AddNewGeometryDrawing(brush, pen, ellipseGeometry);
		}
	}

	public override void DrawGeometry(Brush brush, Pen pen, Geometry geometry)
	{
		VerifyApiNonstructuralChange();
		if ((brush != null || pen != null) && geometry != null)
		{
			AddNewGeometryDrawing(brush, pen, geometry);
		}
	}

	public override void DrawImage(ImageSource imageSource, Rect rectangle)
	{
		DrawImage(imageSource, rectangle, null);
	}

	public override void DrawImage(ImageSource imageSource, Rect rectangle, AnimationClock rectangleAnimations)
	{
		VerifyApiNonstructuralChange();
		if (imageSource != null)
		{
			ImageDrawing imageDrawing = new ImageDrawing();
			imageDrawing.CanBeInheritanceContext = CanBeInheritanceContext;
			imageDrawing.ImageSource = imageSource;
			imageDrawing.Rect = rectangle;
			SetupNewFreezable(imageDrawing, rectangleAnimations == null && imageSource.IsFrozen);
			if (rectangleAnimations != null)
			{
				imageDrawing.ApplyAnimationClock(ImageDrawing.RectProperty, rectangleAnimations);
			}
			AddDrawing(imageDrawing);
		}
	}

	public override void DrawDrawing(Drawing drawing)
	{
		VerifyApiNonstructuralChange();
		if (drawing != null)
		{
			AddDrawing(drawing);
		}
	}

	public override void DrawVideo(MediaPlayer player, Rect rectangle)
	{
		DrawVideo(player, rectangle, null);
	}

	public override void DrawVideo(MediaPlayer player, Rect rectangle, AnimationClock rectangleAnimations)
	{
		VerifyApiNonstructuralChange();
		if (player != null)
		{
			VideoDrawing videoDrawing = new VideoDrawing();
			videoDrawing.CanBeInheritanceContext = CanBeInheritanceContext;
			videoDrawing.Player = player;
			videoDrawing.Rect = rectangle;
			SetupNewFreezable(videoDrawing, fFreeze: false);
			if (rectangleAnimations != null)
			{
				videoDrawing.ApplyAnimationClock(VideoDrawing.RectProperty, rectangleAnimations);
			}
			AddDrawing(videoDrawing);
		}
	}

	public override void PushClip(Geometry clipGeometry)
	{
		VerifyApiNonstructuralChange();
		PushNewDrawingGroup();
		_currentDrawingGroup.ClipGeometry = clipGeometry;
	}

	public override void PushOpacityMask(Brush brush)
	{
		VerifyApiNonstructuralChange();
		PushNewDrawingGroup();
		_currentDrawingGroup.OpacityMask = brush;
	}

	public override void PushOpacity(double opacity)
	{
		PushOpacity(opacity, null);
	}

	public override void PushOpacity(double opacity, AnimationClock opacityAnimations)
	{
		VerifyApiNonstructuralChange();
		PushNewDrawingGroup();
		_currentDrawingGroup.Opacity = opacity;
		if (opacityAnimations != null)
		{
			_currentDrawingGroup.ApplyAnimationClock(DrawingGroup.OpacityProperty, opacityAnimations);
		}
	}

	public override void PushTransform(Transform transform)
	{
		VerifyApiNonstructuralChange();
		PushNewDrawingGroup();
		_currentDrawingGroup.Transform = transform;
	}

	public override void PushGuidelineSet(GuidelineSet guidelines)
	{
		VerifyApiNonstructuralChange();
		PushNewDrawingGroup();
		_currentDrawingGroup.GuidelineSet = guidelines;
	}

	internal override void PushGuidelineY1(double coordinate)
	{
		VerifyApiNonstructuralChange();
		PushNewDrawingGroup();
		GuidelineSet guidelineSet = new GuidelineSet(null, new double[2] { coordinate, 0.0 }, isDynamic: true);
		guidelineSet.Freeze();
		_currentDrawingGroup.GuidelineSet = guidelineSet;
	}

	internal override void PushGuidelineY2(double leadingCoordinate, double offsetToDrivenCoordinate)
	{
		VerifyApiNonstructuralChange();
		PushNewDrawingGroup();
		GuidelineSet guidelineSet = new GuidelineSet(null, new double[2] { leadingCoordinate, offsetToDrivenCoordinate }, isDynamic: true);
		guidelineSet.Freeze();
		_currentDrawingGroup.GuidelineSet = guidelineSet;
	}

	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	public override void PushEffect(BitmapEffect effect, BitmapEffectInput effectInput)
	{
		VerifyApiNonstructuralChange();
		PushNewDrawingGroup();
		_currentDrawingGroup.BitmapEffect = effect;
		_currentDrawingGroup.BitmapEffectInput = ((effectInput != null) ? effectInput : new BitmapEffectInput());
	}

	public override void Pop()
	{
		VerifyApiNonstructuralChange();
		if (_previousDrawingGroupStack == null || _previousDrawingGroupStack.Count == 0)
		{
			throw new InvalidOperationException(SR.DrawingContext_TooManyPops);
		}
		_currentDrawingGroup = _previousDrawingGroupStack.Pop();
	}

	public override void DrawGlyphRun(Brush foregroundBrush, GlyphRun glyphRun)
	{
		VerifyApiNonstructuralChange();
		if (foregroundBrush != null && glyphRun != null)
		{
			GlyphRunDrawing glyphRunDrawing = new GlyphRunDrawing();
			glyphRunDrawing.CanBeInheritanceContext = CanBeInheritanceContext;
			glyphRunDrawing.ForegroundBrush = foregroundBrush;
			glyphRunDrawing.GlyphRun = glyphRun;
			SetupNewFreezable(glyphRunDrawing, foregroundBrush.IsFrozen);
			AddDrawing(glyphRunDrawing);
		}
	}

	public override void Close()
	{
		VerifyNotDisposed();
		((IDisposable)this).Dispose();
	}

	protected override void DisposeCore()
	{
		if (_disposed)
		{
			return;
		}
		if (_previousDrawingGroupStack != null)
		{
			int count = _previousDrawingGroupStack.Count;
			for (int i = 0; i < count; i++)
			{
				Pop();
			}
		}
		DrawingCollection drawingCollection;
		if (_currentDrawingGroup != null)
		{
			drawingCollection = _currentDrawingGroup.Children;
		}
		else
		{
			drawingCollection = new DrawingCollection();
			drawingCollection.CanBeInheritanceContext = CanBeInheritanceContext;
			if (_rootDrawing != null)
			{
				drawingCollection.Add(_rootDrawing);
			}
		}
		CloseCore(drawingCollection);
		_disposed = true;
	}

	protected virtual void CloseCore(DrawingCollection rootDrawingGroupChildren)
	{
	}

	protected override void VerifyApiNonstructuralChange()
	{
		base.VerifyApiNonstructuralChange();
		VerifyNotDisposed();
	}

	private void VerifyNotDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("DrawingDrawingContext");
		}
	}

	private Freezable SetupNewFreezable(Freezable newFreezable, bool fFreeze)
	{
		if (fFreeze)
		{
			newFreezable.Freeze();
		}
		return newFreezable;
	}

	private void AddNewGeometryDrawing(Brush brush, Pen pen, Geometry geometry)
	{
		GeometryDrawing geometryDrawing = new GeometryDrawing();
		geometryDrawing.CanBeInheritanceContext = CanBeInheritanceContext;
		geometryDrawing.Brush = brush;
		geometryDrawing.Pen = pen;
		geometryDrawing.Geometry = geometry;
		SetupNewFreezable(geometryDrawing, (brush == null || brush.IsFrozen) && (pen == null || pen.IsFrozen) && geometry.IsFrozen);
		AddDrawing(geometryDrawing);
	}

	private void PushNewDrawingGroup()
	{
		DrawingGroup drawingGroup = new DrawingGroup();
		drawingGroup.CanBeInheritanceContext = CanBeInheritanceContext;
		SetupNewFreezable(drawingGroup, fFreeze: false);
		AddDrawing(drawingGroup);
		if (_previousDrawingGroupStack == null)
		{
			_previousDrawingGroupStack = new Stack<DrawingGroup>(2);
		}
		_previousDrawingGroupStack.Push(_currentDrawingGroup);
		_currentDrawingGroup = drawingGroup;
	}

	private void AddDrawing(Drawing newDrawing)
	{
		if (_rootDrawing == null)
		{
			_rootDrawing = newDrawing;
		}
		else if (_currentDrawingGroup == null)
		{
			_currentDrawingGroup = new DrawingGroup();
			_currentDrawingGroup.CanBeInheritanceContext = CanBeInheritanceContext;
			SetupNewFreezable(_currentDrawingGroup, fFreeze: false);
			_currentDrawingGroup.Children.Add(_rootDrawing);
			_currentDrawingGroup.Children.Add(newDrawing);
			_rootDrawing = _currentDrawingGroup;
		}
		else
		{
			_currentDrawingGroup.Children.Add(newDrawing);
		}
	}
}
