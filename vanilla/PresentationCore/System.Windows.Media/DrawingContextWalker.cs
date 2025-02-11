using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace System.Windows.Media;

internal abstract class DrawingContextWalker : DrawingContext
{
	private bool _stopWalking;

	internal bool ShouldStopWalking
	{
		get
		{
			return _stopWalking;
		}
		set
		{
			_stopWalking = value;
		}
	}

	public sealed override void Close()
	{
	}

	protected override void DisposeCore()
	{
	}

	protected void StopWalking()
	{
		_stopWalking = true;
	}

	public override void DrawLine(Pen pen, Point point0, Point point1)
	{
	}

	public override void DrawLine(Pen pen, Point point0, AnimationClock point0Animations, Point point1, AnimationClock point1Animations)
	{
	}

	public override void DrawRectangle(Brush brush, Pen pen, Rect rectangle)
	{
	}

	public override void DrawRectangle(Brush brush, Pen pen, Rect rectangle, AnimationClock rectangleAnimations)
	{
	}

	public override void DrawRoundedRectangle(Brush brush, Pen pen, Rect rectangle, double radiusX, double radiusY)
	{
	}

	public override void DrawRoundedRectangle(Brush brush, Pen pen, Rect rectangle, AnimationClock rectangleAnimations, double radiusX, AnimationClock radiusXAnimations, double radiusY, AnimationClock radiusYAnimations)
	{
	}

	public override void DrawEllipse(Brush brush, Pen pen, Point center, double radiusX, double radiusY)
	{
	}

	public override void DrawEllipse(Brush brush, Pen pen, Point center, AnimationClock centerAnimations, double radiusX, AnimationClock radiusXAnimations, double radiusY, AnimationClock radiusYAnimations)
	{
	}

	public override void DrawGeometry(Brush brush, Pen pen, Geometry geometry)
	{
	}

	public override void DrawImage(ImageSource imageSource, Rect rectangle)
	{
	}

	public override void DrawImage(ImageSource imageSource, Rect rectangle, AnimationClock rectangleAnimations)
	{
	}

	public override void DrawGlyphRun(Brush foregroundBrush, GlyphRun glyphRun)
	{
	}

	public override void DrawDrawing(Drawing drawing)
	{
		drawing?.WalkCurrentValue(this);
	}

	public override void DrawVideo(MediaPlayer player, Rect rectangle)
	{
	}

	public override void DrawVideo(MediaPlayer player, Rect rectangle, AnimationClock rectangleAnimations)
	{
	}

	public override void PushClip(Geometry clipGeometry)
	{
	}

	public override void PushOpacityMask(Brush opacityMask)
	{
	}

	public override void PushOpacity(double opacity)
	{
	}

	public override void PushOpacity(double opacity, AnimationClock opacityAnimations)
	{
	}

	public override void PushTransform(Transform transform)
	{
	}

	public override void PushGuidelineSet(GuidelineSet guidelines)
	{
	}

	internal override void PushGuidelineY1(double coordinate)
	{
	}

	internal override void PushGuidelineY2(double leadingCoordinate, double offsetToDrivenCoordinate)
	{
	}

	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	public override void PushEffect(BitmapEffect effect, BitmapEffectInput effectInput)
	{
	}

	public override void Pop()
	{
	}
}
