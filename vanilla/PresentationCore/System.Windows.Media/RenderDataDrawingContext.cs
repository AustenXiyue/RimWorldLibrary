using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Media.Effects;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

internal class RenderDataDrawingContext : DrawingContext, IDisposable
{
	private RenderData _renderData;

	private bool _disposed;

	private int _stackDepth;

	internal RenderDataDrawingContext()
	{
	}

	internal RenderData GetRenderData()
	{
		return _renderData;
	}

	public override void Close()
	{
		VerifyApiNonstructuralChange();
		((IDisposable)this).Dispose();
	}

	protected override void DisposeCore()
	{
		if (!_disposed)
		{
			EnsureCorrectNesting();
			CloseCore(_renderData);
			_disposed = true;
		}
	}

	protected virtual void CloseCore(RenderData renderData)
	{
	}

	private void EnsureRenderData()
	{
		if (_renderData == null)
		{
			_renderData = new RenderData();
		}
	}

	protected override void VerifyApiNonstructuralChange()
	{
		base.VerifyApiNonstructuralChange();
		if (_disposed)
		{
			throw new ObjectDisposedException("RenderDataDrawingContext");
		}
	}

	private void EnsureCorrectNesting()
	{
		if (_renderData != null && _stackDepth > 0)
		{
			int stackDepth = _stackDepth;
			for (int i = 0; i < stackDepth; i++)
			{
				Pop();
			}
		}
		_stackDepth = 0;
	}

	public unsafe override void DrawLine(Pen pen, Point point0, Point point1)
	{
		VerifyApiNonstructuralChange();
		if (pen != null)
		{
			EnsureRenderData();
			MILCMD_DRAW_LINE mILCMD_DRAW_LINE = new MILCMD_DRAW_LINE(_renderData.AddDependentResource(pen), point0, point1);
			_renderData.WriteDataRecord(MILCMD.MilDrawLine, (byte*)(&mILCMD_DRAW_LINE), 40);
		}
	}

	public unsafe override void DrawLine(Pen pen, Point point0, AnimationClock point0Animations, Point point1, AnimationClock point1Animations)
	{
		VerifyApiNonstructuralChange();
		if (pen != null)
		{
			EnsureRenderData();
			uint num = 0u;
			uint num2 = 0u;
			num = UseAnimations(point0, point0Animations);
			num2 = UseAnimations(point1, point1Animations);
			MILCMD_DRAW_LINE_ANIMATE mILCMD_DRAW_LINE_ANIMATE = new MILCMD_DRAW_LINE_ANIMATE(_renderData.AddDependentResource(pen), point0, num, point1, num2);
			_renderData.WriteDataRecord(MILCMD.MilDrawLineAnimate, (byte*)(&mILCMD_DRAW_LINE_ANIMATE), 48);
		}
	}

	public unsafe override void DrawRectangle(Brush brush, Pen pen, Rect rectangle)
	{
		VerifyApiNonstructuralChange();
		if (brush != null || pen != null)
		{
			EnsureRenderData();
			MILCMD_DRAW_RECTANGLE mILCMD_DRAW_RECTANGLE = new MILCMD_DRAW_RECTANGLE(_renderData.AddDependentResource(brush), _renderData.AddDependentResource(pen), rectangle);
			_renderData.WriteDataRecord(MILCMD.MilDrawRectangle, (byte*)(&mILCMD_DRAW_RECTANGLE), 40);
		}
	}

	public unsafe override void DrawRectangle(Brush brush, Pen pen, Rect rectangle, AnimationClock rectangleAnimations)
	{
		VerifyApiNonstructuralChange();
		if (brush != null || pen != null)
		{
			EnsureRenderData();
			uint num = 0u;
			num = UseAnimations(rectangle, rectangleAnimations);
			MILCMD_DRAW_RECTANGLE_ANIMATE mILCMD_DRAW_RECTANGLE_ANIMATE = new MILCMD_DRAW_RECTANGLE_ANIMATE(_renderData.AddDependentResource(brush), _renderData.AddDependentResource(pen), rectangle, num);
			_renderData.WriteDataRecord(MILCMD.MilDrawRectangleAnimate, (byte*)(&mILCMD_DRAW_RECTANGLE_ANIMATE), 48);
		}
	}

	public unsafe override void DrawRoundedRectangle(Brush brush, Pen pen, Rect rectangle, double radiusX, double radiusY)
	{
		VerifyApiNonstructuralChange();
		if (brush != null || pen != null)
		{
			EnsureRenderData();
			MILCMD_DRAW_ROUNDED_RECTANGLE mILCMD_DRAW_ROUNDED_RECTANGLE = new MILCMD_DRAW_ROUNDED_RECTANGLE(_renderData.AddDependentResource(brush), _renderData.AddDependentResource(pen), rectangle, radiusX, radiusY);
			_renderData.WriteDataRecord(MILCMD.MilDrawRoundedRectangle, (byte*)(&mILCMD_DRAW_ROUNDED_RECTANGLE), 56);
		}
	}

	public unsafe override void DrawRoundedRectangle(Brush brush, Pen pen, Rect rectangle, AnimationClock rectangleAnimations, double radiusX, AnimationClock radiusXAnimations, double radiusY, AnimationClock radiusYAnimations)
	{
		VerifyApiNonstructuralChange();
		if (brush != null || pen != null)
		{
			EnsureRenderData();
			uint num = 0u;
			uint num2 = 0u;
			uint num3 = 0u;
			num = UseAnimations(rectangle, rectangleAnimations);
			num2 = UseAnimations(radiusX, radiusXAnimations);
			num3 = UseAnimations(radiusY, radiusYAnimations);
			MILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE = new MILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE(_renderData.AddDependentResource(brush), _renderData.AddDependentResource(pen), rectangle, num, radiusX, num2, radiusY, num3);
			_renderData.WriteDataRecord(MILCMD.MilDrawRoundedRectangleAnimate, (byte*)(&mILCMD_DRAW_ROUNDED_RECTANGLE_ANIMATE), 72);
		}
	}

	public unsafe override void DrawEllipse(Brush brush, Pen pen, Point center, double radiusX, double radiusY)
	{
		VerifyApiNonstructuralChange();
		if (brush != null || pen != null)
		{
			EnsureRenderData();
			MILCMD_DRAW_ELLIPSE mILCMD_DRAW_ELLIPSE = new MILCMD_DRAW_ELLIPSE(_renderData.AddDependentResource(brush), _renderData.AddDependentResource(pen), center, radiusX, radiusY);
			_renderData.WriteDataRecord(MILCMD.MilDrawEllipse, (byte*)(&mILCMD_DRAW_ELLIPSE), 40);
		}
	}

	public unsafe override void DrawEllipse(Brush brush, Pen pen, Point center, AnimationClock centerAnimations, double radiusX, AnimationClock radiusXAnimations, double radiusY, AnimationClock radiusYAnimations)
	{
		VerifyApiNonstructuralChange();
		if (brush != null || pen != null)
		{
			EnsureRenderData();
			uint num = 0u;
			uint num2 = 0u;
			uint num3 = 0u;
			num = UseAnimations(center, centerAnimations);
			num2 = UseAnimations(radiusX, radiusXAnimations);
			num3 = UseAnimations(radiusY, radiusYAnimations);
			MILCMD_DRAW_ELLIPSE_ANIMATE mILCMD_DRAW_ELLIPSE_ANIMATE = new MILCMD_DRAW_ELLIPSE_ANIMATE(_renderData.AddDependentResource(brush), _renderData.AddDependentResource(pen), center, num, radiusX, num2, radiusY, num3);
			_renderData.WriteDataRecord(MILCMD.MilDrawEllipseAnimate, (byte*)(&mILCMD_DRAW_ELLIPSE_ANIMATE), 56);
		}
	}

	public unsafe override void DrawGeometry(Brush brush, Pen pen, Geometry geometry)
	{
		VerifyApiNonstructuralChange();
		if ((brush != null || pen != null) && geometry != null)
		{
			EnsureRenderData();
			MILCMD_DRAW_GEOMETRY mILCMD_DRAW_GEOMETRY = new MILCMD_DRAW_GEOMETRY(_renderData.AddDependentResource(brush), _renderData.AddDependentResource(pen), _renderData.AddDependentResource(geometry));
			_renderData.WriteDataRecord(MILCMD.MilDrawGeometry, (byte*)(&mILCMD_DRAW_GEOMETRY), 16);
		}
	}

	public unsafe override void DrawImage(ImageSource imageSource, Rect rectangle)
	{
		VerifyApiNonstructuralChange();
		if (imageSource != null)
		{
			EnsureRenderData();
			MILCMD_DRAW_IMAGE mILCMD_DRAW_IMAGE = new MILCMD_DRAW_IMAGE(_renderData.AddDependentResource(imageSource), rectangle);
			_renderData.WriteDataRecord(MILCMD.MilDrawImage, (byte*)(&mILCMD_DRAW_IMAGE), 40);
		}
	}

	public unsafe override void DrawImage(ImageSource imageSource, Rect rectangle, AnimationClock rectangleAnimations)
	{
		VerifyApiNonstructuralChange();
		if (imageSource != null)
		{
			EnsureRenderData();
			uint num = 0u;
			num = UseAnimations(rectangle, rectangleAnimations);
			MILCMD_DRAW_IMAGE_ANIMATE mILCMD_DRAW_IMAGE_ANIMATE = new MILCMD_DRAW_IMAGE_ANIMATE(_renderData.AddDependentResource(imageSource), rectangle, num);
			_renderData.WriteDataRecord(MILCMD.MilDrawImageAnimate, (byte*)(&mILCMD_DRAW_IMAGE_ANIMATE), 40);
		}
	}

	public unsafe override void DrawGlyphRun(Brush foregroundBrush, GlyphRun glyphRun)
	{
		VerifyApiNonstructuralChange();
		if (foregroundBrush != null && glyphRun != null)
		{
			EnsureRenderData();
			MILCMD_DRAW_GLYPH_RUN mILCMD_DRAW_GLYPH_RUN = new MILCMD_DRAW_GLYPH_RUN(_renderData.AddDependentResource(foregroundBrush), _renderData.AddDependentResource(glyphRun));
			_renderData.WriteDataRecord(MILCMD.MilDrawGlyphRun, (byte*)(&mILCMD_DRAW_GLYPH_RUN), 8);
		}
	}

	public unsafe override void DrawDrawing(Drawing drawing)
	{
		VerifyApiNonstructuralChange();
		if (drawing != null)
		{
			EnsureRenderData();
			MILCMD_DRAW_DRAWING mILCMD_DRAW_DRAWING = new MILCMD_DRAW_DRAWING(_renderData.AddDependentResource(drawing));
			_renderData.WriteDataRecord(MILCMD.MilDrawDrawing, (byte*)(&mILCMD_DRAW_DRAWING), 8);
		}
	}

	public unsafe override void DrawVideo(MediaPlayer player, Rect rectangle)
	{
		VerifyApiNonstructuralChange();
		if (player != null)
		{
			EnsureRenderData();
			MILCMD_DRAW_VIDEO mILCMD_DRAW_VIDEO = new MILCMD_DRAW_VIDEO(_renderData.AddDependentResource(player), rectangle);
			_renderData.WriteDataRecord(MILCMD.MilDrawVideo, (byte*)(&mILCMD_DRAW_VIDEO), 40);
		}
	}

	public unsafe override void DrawVideo(MediaPlayer player, Rect rectangle, AnimationClock rectangleAnimations)
	{
		VerifyApiNonstructuralChange();
		if (player != null)
		{
			EnsureRenderData();
			uint num = 0u;
			num = UseAnimations(rectangle, rectangleAnimations);
			MILCMD_DRAW_VIDEO_ANIMATE mILCMD_DRAW_VIDEO_ANIMATE = new MILCMD_DRAW_VIDEO_ANIMATE(_renderData.AddDependentResource(player), rectangle, num);
			_renderData.WriteDataRecord(MILCMD.MilDrawVideoAnimate, (byte*)(&mILCMD_DRAW_VIDEO_ANIMATE), 40);
		}
	}

	public unsafe override void PushClip(Geometry clipGeometry)
	{
		VerifyApiNonstructuralChange();
		EnsureRenderData();
		MILCMD_PUSH_CLIP mILCMD_PUSH_CLIP = new MILCMD_PUSH_CLIP(_renderData.AddDependentResource(clipGeometry));
		_renderData.WriteDataRecord(MILCMD.MilPushClip, (byte*)(&mILCMD_PUSH_CLIP), 8);
		_stackDepth++;
	}

	public unsafe override void PushOpacityMask(Brush opacityMask)
	{
		VerifyApiNonstructuralChange();
		EnsureRenderData();
		MILCMD_PUSH_OPACITY_MASK mILCMD_PUSH_OPACITY_MASK = new MILCMD_PUSH_OPACITY_MASK(_renderData.AddDependentResource(opacityMask));
		_renderData.WriteDataRecord(MILCMD.MilPushOpacityMask, (byte*)(&mILCMD_PUSH_OPACITY_MASK), 24);
		_stackDepth++;
	}

	public unsafe override void PushOpacity(double opacity)
	{
		VerifyApiNonstructuralChange();
		EnsureRenderData();
		MILCMD_PUSH_OPACITY mILCMD_PUSH_OPACITY = new MILCMD_PUSH_OPACITY(opacity);
		_renderData.WriteDataRecord(MILCMD.MilPushOpacity, (byte*)(&mILCMD_PUSH_OPACITY), 8);
		_stackDepth++;
	}

	public unsafe override void PushOpacity(double opacity, AnimationClock opacityAnimations)
	{
		VerifyApiNonstructuralChange();
		EnsureRenderData();
		uint num = 0u;
		num = UseAnimations(opacity, opacityAnimations);
		MILCMD_PUSH_OPACITY_ANIMATE mILCMD_PUSH_OPACITY_ANIMATE = new MILCMD_PUSH_OPACITY_ANIMATE(opacity, num);
		_renderData.WriteDataRecord(MILCMD.MilPushOpacityAnimate, (byte*)(&mILCMD_PUSH_OPACITY_ANIMATE), 16);
		_stackDepth++;
	}

	public unsafe override void PushTransform(Transform transform)
	{
		VerifyApiNonstructuralChange();
		EnsureRenderData();
		MILCMD_PUSH_TRANSFORM mILCMD_PUSH_TRANSFORM = new MILCMD_PUSH_TRANSFORM(_renderData.AddDependentResource(transform));
		_renderData.WriteDataRecord(MILCMD.MilPushTransform, (byte*)(&mILCMD_PUSH_TRANSFORM), 8);
		_stackDepth++;
	}

	public unsafe override void PushGuidelineSet(GuidelineSet guidelines)
	{
		VerifyApiNonstructuralChange();
		EnsureRenderData();
		if (guidelines != null && guidelines.IsFrozen && guidelines.IsDynamic)
		{
			DoubleCollection guidelinesX = guidelines.GuidelinesX;
			DoubleCollection guidelinesY = guidelines.GuidelinesY;
			int num = guidelinesX?.Count ?? 0;
			int num2 = guidelinesY?.Count ?? 0;
			if (num == 0 && (num2 == 1 || num2 == 2))
			{
				if (num2 == 1)
				{
					MILCMD_PUSH_GUIDELINE_Y1 mILCMD_PUSH_GUIDELINE_Y = new MILCMD_PUSH_GUIDELINE_Y1(guidelinesY[0]);
					_renderData.WriteDataRecord(MILCMD.MilPushGuidelineY1, (byte*)(&mILCMD_PUSH_GUIDELINE_Y), sizeof(MILCMD_PUSH_GUIDELINE_Y1));
				}
				else
				{
					MILCMD_PUSH_GUIDELINE_Y2 mILCMD_PUSH_GUIDELINE_Y2 = new MILCMD_PUSH_GUIDELINE_Y2(guidelinesY[0], guidelinesY[1] - guidelinesY[0]);
					_renderData.WriteDataRecord(MILCMD.MilPushGuidelineY2, (byte*)(&mILCMD_PUSH_GUIDELINE_Y2), sizeof(MILCMD_PUSH_GUIDELINE_Y2));
				}
			}
		}
		else
		{
			MILCMD_PUSH_GUIDELINE_SET mILCMD_PUSH_GUIDELINE_SET = new MILCMD_PUSH_GUIDELINE_SET(_renderData.AddDependentResource(guidelines));
			_renderData.WriteDataRecord(MILCMD.MilPushGuidelineSet, (byte*)(&mILCMD_PUSH_GUIDELINE_SET), 8);
		}
		_stackDepth++;
	}

	internal unsafe override void PushGuidelineY1(double coordinate)
	{
		VerifyApiNonstructuralChange();
		EnsureRenderData();
		MILCMD_PUSH_GUIDELINE_Y1 mILCMD_PUSH_GUIDELINE_Y = new MILCMD_PUSH_GUIDELINE_Y1(coordinate);
		_renderData.WriteDataRecord(MILCMD.MilPushGuidelineY1, (byte*)(&mILCMD_PUSH_GUIDELINE_Y), 8);
		_stackDepth++;
	}

	internal unsafe override void PushGuidelineY2(double leadingCoordinate, double offsetToDrivenCoordinate)
	{
		VerifyApiNonstructuralChange();
		EnsureRenderData();
		MILCMD_PUSH_GUIDELINE_Y2 mILCMD_PUSH_GUIDELINE_Y = new MILCMD_PUSH_GUIDELINE_Y2(leadingCoordinate, offsetToDrivenCoordinate);
		_renderData.WriteDataRecord(MILCMD.MilPushGuidelineY2, (byte*)(&mILCMD_PUSH_GUIDELINE_Y), 16);
		_stackDepth++;
	}

	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	public unsafe override void PushEffect(BitmapEffect effect, BitmapEffectInput effectInput)
	{
		VerifyApiNonstructuralChange();
		EnsureRenderData();
		MILCMD_PUSH_EFFECT mILCMD_PUSH_EFFECT = new MILCMD_PUSH_EFFECT(_renderData.AddDependentResource(effect), _renderData.AddDependentResource(effectInput));
		_renderData.WriteDataRecord(MILCMD.MilPushEffect, (byte*)(&mILCMD_PUSH_EFFECT), 8);
		_stackDepth++;
		if (_renderData.BitmapEffectStackDepth == 0)
		{
			_renderData.BeginTopLevelBitmapEffect(_stackDepth);
		}
	}

	public unsafe override void Pop()
	{
		VerifyApiNonstructuralChange();
		if (_stackDepth <= 0)
		{
			throw new InvalidOperationException(SR.DrawingContext_TooManyPops);
		}
		EnsureRenderData();
		MILCMD_POP mILCMD_POP = default(MILCMD_POP);
		_renderData.WriteDataRecord(MILCMD.MilPop, (byte*)(&mILCMD_POP), 0);
		_stackDepth--;
		if (_renderData.BitmapEffectStackDepth == _stackDepth + 1)
		{
			_renderData.EndTopLevelBitmapEffect();
		}
	}

	private uint UseAnimations(double baseValue, AnimationClock animations)
	{
		if (animations == null)
		{
			return 0u;
		}
		return _renderData.AddDependentResource(new DoubleAnimationClockResource(baseValue, animations));
	}

	private uint UseAnimations(Point baseValue, AnimationClock animations)
	{
		if (animations == null)
		{
			return 0u;
		}
		return _renderData.AddDependentResource(new PointAnimationClockResource(baseValue, animations));
	}

	private uint UseAnimations(Size baseValue, AnimationClock animations)
	{
		if (animations == null)
		{
			return 0u;
		}
		return _renderData.AddDependentResource(new SizeAnimationClockResource(baseValue, animations));
	}

	private uint UseAnimations(Rect baseValue, AnimationClock animations)
	{
		if (animations == null)
		{
			return 0u;
		}
		return _renderData.AddDependentResource(new RectAnimationClockResource(baseValue, animations));
	}
}
