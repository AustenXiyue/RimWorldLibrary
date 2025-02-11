using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace System.Windows.Media;

internal class DrawingContextDrawingContextWalker : DrawingContextWalker
{
	private DrawingContext _drawingContext;

	public DrawingContextDrawingContextWalker(DrawingContext drawingContext)
	{
		_drawingContext = drawingContext;
	}

	public override void DrawLine(Pen pen, Point point0, Point point1)
	{
		_drawingContext.DrawLine(pen, point0, point1);
	}

	public override void DrawLine(Pen pen, Point point0, AnimationClock point0Animations, Point point1, AnimationClock point1Animations)
	{
		_drawingContext.DrawLine(pen, point0, point0Animations, point1, point1Animations);
	}

	public override void DrawRectangle(Brush brush, Pen pen, Rect rectangle)
	{
		_drawingContext.DrawRectangle(brush, pen, rectangle);
	}

	public override void DrawRectangle(Brush brush, Pen pen, Rect rectangle, AnimationClock rectangleAnimations)
	{
		_drawingContext.DrawRectangle(brush, pen, rectangle, rectangleAnimations);
	}

	public override void DrawRoundedRectangle(Brush brush, Pen pen, Rect rectangle, double radiusX, double radiusY)
	{
		_drawingContext.DrawRoundedRectangle(brush, pen, rectangle, radiusX, radiusY);
	}

	public override void DrawRoundedRectangle(Brush brush, Pen pen, Rect rectangle, AnimationClock rectangleAnimations, double radiusX, AnimationClock radiusXAnimations, double radiusY, AnimationClock radiusYAnimations)
	{
		_drawingContext.DrawRoundedRectangle(brush, pen, rectangle, rectangleAnimations, radiusX, radiusXAnimations, radiusY, radiusYAnimations);
	}

	public override void DrawEllipse(Brush brush, Pen pen, Point center, double radiusX, double radiusY)
	{
		_drawingContext.DrawEllipse(brush, pen, center, radiusX, radiusY);
	}

	public override void DrawEllipse(Brush brush, Pen pen, Point center, AnimationClock centerAnimations, double radiusX, AnimationClock radiusXAnimations, double radiusY, AnimationClock radiusYAnimations)
	{
		_drawingContext.DrawEllipse(brush, pen, center, centerAnimations, radiusX, radiusXAnimations, radiusY, radiusYAnimations);
	}

	public override void DrawGeometry(Brush brush, Pen pen, Geometry geometry)
	{
		_drawingContext.DrawGeometry(brush, pen, geometry);
	}

	public override void DrawImage(ImageSource imageSource, Rect rectangle)
	{
		_drawingContext.DrawImage(imageSource, rectangle);
	}

	public override void DrawImage(ImageSource imageSource, Rect rectangle, AnimationClock rectangleAnimations)
	{
		_drawingContext.DrawImage(imageSource, rectangle, rectangleAnimations);
	}

	public override void DrawGlyphRun(Brush foregroundBrush, GlyphRun glyphRun)
	{
		_drawingContext.DrawGlyphRun(foregroundBrush, glyphRun);
	}

	public override void DrawDrawing(Drawing drawing)
	{
		_drawingContext.DrawDrawing(drawing);
	}

	public override void DrawVideo(MediaPlayer player, Rect rectangle)
	{
		_drawingContext.DrawVideo(player, rectangle);
	}

	public override void DrawVideo(MediaPlayer player, Rect rectangle, AnimationClock rectangleAnimations)
	{
		_drawingContext.DrawVideo(player, rectangle, rectangleAnimations);
	}

	public override void PushClip(Geometry clipGeometry)
	{
		_drawingContext.PushClip(clipGeometry);
	}

	public override void PushOpacityMask(Brush opacityMask)
	{
		_drawingContext.PushOpacityMask(opacityMask);
	}

	public override void PushOpacity(double opacity)
	{
		_drawingContext.PushOpacity(opacity);
	}

	public override void PushOpacity(double opacity, AnimationClock opacityAnimations)
	{
		_drawingContext.PushOpacity(opacity, opacityAnimations);
	}

	public override void PushTransform(Transform transform)
	{
		_drawingContext.PushTransform(transform);
	}

	public override void PushGuidelineSet(GuidelineSet guidelines)
	{
		_drawingContext.PushGuidelineSet(guidelines);
	}

	internal override void PushGuidelineY1(double coordinate)
	{
		_drawingContext.PushGuidelineY1(coordinate);
	}

	internal override void PushGuidelineY2(double leadingCoordinate, double offsetToDrivenCoordinate)
	{
		_drawingContext.PushGuidelineY2(leadingCoordinate, offsetToDrivenCoordinate);
	}

	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	public override void PushEffect(BitmapEffect effect, BitmapEffectInput effectInput)
	{
		_drawingContext.PushEffect(effect, effectInput);
	}

	public override void Pop()
	{
		_drawingContext.Pop();
	}
}
