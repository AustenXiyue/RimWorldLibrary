using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using MS.Utility;

namespace System.Windows.Media;

/// <summary>Describes visual content using draw, push, and pop commands. </summary>
public abstract class DrawingContext : DispatcherObject, IDisposable
{
	internal DrawingContext()
	{
	}

	/// <summary>Draws formatted text at the specified location. </summary>
	/// <param name="formattedText">The formatted text to be drawn.</param>
	/// <param name="origin">The location where the text is to be drawn.</param>
	/// <exception cref="T:System.ObjectDisposedException">The object has already been closed or disposed.</exception>
	public void DrawText(FormattedText formattedText, Point origin)
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose, EventTrace.Event.WClientStringBegin, "DrawingContext.DrawText Start");
		VerifyApiNonstructuralChange();
		if (formattedText != null)
		{
			formattedText.Draw(this, origin);
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose, EventTrace.Event.WClientStringEnd, "DrawingContext.DrawText End");
		}
	}

	/// <summary>Closes the <see cref="T:System.Windows.Media.DrawingContext" /> and flushes the content. Afterward, the <see cref="T:System.Windows.Media.DrawingContext" /> cannot be modified.</summary>
	/// <exception cref="T:System.ObjectDisposedException">This object has already been closed or disposed.</exception>
	public abstract void Close();

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. </summary>
	void IDisposable.Dispose()
	{
		VerifyAccess();
		DisposeCore();
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Windows.Media.DrawingContext" />. </summary>
	/// <exception cref="T:System.ObjectDisposedException">The object has already been closed or disposed.</exception>
	protected abstract void DisposeCore();

	/// <summary>This member supports the WPF infrastructure and is not intended to be used directly from your code. </summary>
	protected virtual void VerifyApiNonstructuralChange()
	{
		VerifyAccess();
	}

	/// <summary>Draws a line between the specified points using the specified <see cref="T:System.Windows.Media.Pen" />. </summary>
	/// <param name="pen">The pen with which to stroke the line.</param>
	/// <param name="point0">The start point of the line.</param>
	/// <param name="point1">The end point of the line.</param>
	public abstract void DrawLine(Pen pen, Point point0, Point point1);

	/// <summary>Draws a line between the specified points using the specified <see cref="T:System.Windows.Media.Pen" /> and applies the specified animation clocks. </summary>
	/// <param name="pen">The pen to stroke the line.</param>
	/// <param name="point0">The start point of the line.</param>
	/// <param name="point0Animations">The clock with which to animate the start point of the line, or null for no animation. This clock must be created from an <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> that can animate <see cref="T:System.Windows.Point" /> objects.</param>
	/// <param name="point1">The end point of the line.</param>
	/// <param name="point1Animations">The clock with which to animate the end point of the line, or null for no animation. This clock must be created from an <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> that can animate <see cref="T:System.Windows.Point" /> objects.</param>
	public abstract void DrawLine(Pen pen, Point point0, AnimationClock point0Animations, Point point1, AnimationClock point1Animations);

	/// <summary>Draws a rectangle with the specified <see cref="T:System.Windows.Media.Brush" /> and <see cref="T:System.Windows.Media.Pen" />. The pen and the brush can be null. </summary>
	/// <param name="brush">The brush with which to fill the rectangle.  This is optional, and can be null. If the brush is null, no fill is drawn.</param>
	/// <param name="pen">The pen with which to stroke the rectangle.  This is optional, and can be null. If the pen is null, no stroke is drawn.</param>
	/// <param name="rectangle">The rectangle to draw.</param>
	public abstract void DrawRectangle(Brush brush, Pen pen, Rect rectangle);

	/// <summary>Draws a rectangle with the specified <see cref="T:System.Windows.Media.Brush" /> and <see cref="T:System.Windows.Media.Pen" /> and applies the specified animation clocks. </summary>
	/// <param name="brush">The brush with which to fill the rectangle.  This is optional, and can be null. If the brush is null, no fill is drawn.</param>
	/// <param name="pen">The pen with which to stroke the rectangle.  This is optional, and can be null. If the pen is null, no stroke is drawn.</param>
	/// <param name="rectangle">The rectangle to draw.</param>
	/// <param name="rectangleAnimations">The clock with which to animate the rectangle's size and dimensions, or null for no animation. This clock must be created from an <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> that can animate <see cref="T:System.Windows.Rect" /> objects.</param>
	public abstract void DrawRectangle(Brush brush, Pen pen, Rect rectangle, AnimationClock rectangleAnimations);

	/// <summary>Draws a rounded rectangle with the specified <see cref="T:System.Windows.Media.Brush" /> and <see cref="T:System.Windows.Media.Pen" />. </summary>
	/// <param name="brush">The brush used to fill the rectangle.</param>
	/// <param name="pen">The pen used to stroke the rectangle.</param>
	/// <param name="rectangle">The rectangle to draw.</param>
	/// <param name="radiusX">The radius in the X dimension of the rounded corners.  This value will be clamped to the range of 0 to <see cref="P:System.Windows.Rect.Width" />/2.</param>
	/// <param name="radiusY">The radius in the Y dimension of the rounded corners.  This value will be clamped to a value between 0 to <see cref="P:System.Windows.Rect.Height" />/2.</param>
	public abstract void DrawRoundedRectangle(Brush brush, Pen pen, Rect rectangle, double radiusX, double radiusY);

	/// <summary>Draws a rounded rectangle with the specified <see cref="T:System.Windows.Media.Brush" /> and <see cref="T:System.Windows.Media.Pen" /> and applies the specified animation clocks. </summary>
	/// <param name="brush">The brush used to fill the rectangle, or null for no fill.</param>
	/// <param name="pen">The pen used to stroke the rectangle, or null for no stroke.</param>
	/// <param name="rectangle">The rectangle to draw.</param>
	/// <param name="rectangleAnimations">The clock with which to animate the rectangle's size and dimensions, or null for no animation. This clock must be created from an <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> that can animate <see cref="T:System.Windows.Rect" /> objects.</param>
	/// <param name="radiusX">The radius in the X dimension of the rounded corners.  This value will be clamped to the range of 0 to <see cref="P:System.Windows.Rect.Width" />/2</param>
	/// <param name="radiusXAnimations">The clock with which to animate the rectangle's <paramref name="radiusX" /> value, or null for no animation. This clock must be created from an <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> that can animate <see cref="T:System.Double" /> values. </param>
	/// <param name="radiusY">The radius in the Y dimension of the rounded corners.  This value will be clamped to a value between 0 to <see cref="P:System.Windows.Rect.Height" />/2.</param>
	/// <param name="radiusYAnimations">The clock with which to animate the rectangle's <paramref name="radiusY" /> value, or null for no animation. This clock must be created from an <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> that can animate <see cref="T:System.Double" /> values.</param>
	public abstract void DrawRoundedRectangle(Brush brush, Pen pen, Rect rectangle, AnimationClock rectangleAnimations, double radiusX, AnimationClock radiusXAnimations, double radiusY, AnimationClock radiusYAnimations);

	/// <summary>Draws an ellipse with the specified <see cref="T:System.Windows.Media.Brush" /> and <see cref="T:System.Windows.Media.Pen" />. </summary>
	/// <param name="brush">The brush with which to fill the ellipse.  This is optional, and can be null. If the brush is null, no fill is drawn.</param>
	/// <param name="pen">The pen with which to stroke the ellipse.  This is optional, and can be null. If the pen is null, no stroke is drawn.</param>
	/// <param name="center">The location of the center of the ellipse.</param>
	/// <param name="radiusX">The horizontal radius of the ellipse.</param>
	/// <param name="radiusY">The vertical radius of the ellipse.</param>
	public abstract void DrawEllipse(Brush brush, Pen pen, Point center, double radiusX, double radiusY);

	/// <summary>Draws an ellipse with the specified <see cref="T:System.Windows.Media.Brush" /> and <see cref="T:System.Windows.Media.Pen" /> and applies the specified animation clocks. </summary>
	/// <param name="brush">The brush with which to fill the ellipse.  This is optional, and can be null. If the brush is null, no fill is drawn.</param>
	/// <param name="pen">The pen with which to stroke the ellipse.  This is optional, and can be null. If the pen is null, no stroke is drawn.</param>
	/// <param name="center">The location of the center of the ellipse.</param>
	/// <param name="centerAnimations">The clock with which to animate the ellipse's center position, or null for no animation. This clock must be created from an <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> that can animate <see cref="T:System.Windows.Point" /> objects.</param>
	/// <param name="radiusX">The horizontal radius of the ellipse.</param>
	/// <param name="radiusXAnimations">The clock with which to animate the ellipse's x-radius, or null for no animation. This clock must be created from an <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> that can animate <see cref="T:System.Double" /> objects.</param>
	/// <param name="radiusY">The vertical radius of the ellipse.</param>
	/// <param name="radiusYAnimations">The clock with which to animate the ellipse's y-radius, or null for no animation. This clock must be created from an <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> that can animate <see cref="T:System.Double" /> objects.</param>
	public abstract void DrawEllipse(Brush brush, Pen pen, Point center, AnimationClock centerAnimations, double radiusX, AnimationClock radiusXAnimations, double radiusY, AnimationClock radiusYAnimations);

	/// <summary>Draws the specified <see cref="T:System.Windows.Media.Geometry" /> using the specified <see cref="T:System.Windows.Media.Brush" /> and <see cref="T:System.Windows.Media.Pen" />. </summary>
	/// <param name="brush">The <see cref="T:System.Windows.Media.Brush" /> with which to fill the <see cref="T:System.Windows.Media.Geometry" />. This is optional, and can be null. If the brush is null, no fill is drawn.</param>
	/// <param name="pen">The <see cref="T:System.Windows.Media.Pen" /> with which to stroke the <see cref="T:System.Windows.Media.Geometry" />. This is optional, and can be null. If the pen is null, no stroke is drawn.</param>
	/// <param name="geometry">The <see cref="T:System.Windows.Media.Geometry" /> to draw.</param>
	public abstract void DrawGeometry(Brush brush, Pen pen, Geometry geometry);

	/// <summary>Draws an image into the region defined by the specified <see cref="T:System.Windows.Rect" />. </summary>
	/// <param name="imageSource">The image to draw.</param>
	/// <param name="rectangle">The region in which to draw <paramref name="bitmapSource" />.</param>
	public abstract void DrawImage(ImageSource imageSource, Rect rectangle);

	/// <summary>Draws an image into the region defined by the specified <see cref="T:System.Windows.Rect" /> and applies the specified animation clock.  </summary>
	/// <param name="imageSource">The image to draw.</param>
	/// <param name="rectangle">The region in which to draw <paramref name="bitmapSource" />.</param>
	/// <param name="rectangleAnimations">The clock with which to animate the rectangle's size and dimensions, or null for no animation. This clock must be created from an <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> that can animate <see cref="T:System.Windows.Rect" /> objects.</param>
	public abstract void DrawImage(ImageSource imageSource, Rect rectangle, AnimationClock rectangleAnimations);

	/// <summary>Draws the specified text. </summary>
	/// <param name="foregroundBrush">The brush used to paint the text.</param>
	/// <param name="glyphRun">The text to draw.</param>
	public abstract void DrawGlyphRun(Brush foregroundBrush, GlyphRun glyphRun);

	/// <summary>Draws the specified <see cref="T:System.Windows.Media.Drawing" /> object.</summary>
	/// <param name="drawing">The drawing to append.</param>
	public abstract void DrawDrawing(Drawing drawing);

	/// <summary>Draws a video into the specified region.</summary>
	/// <param name="player">The media to draw.</param>
	/// <param name="rectangle">The region in which to draw <paramref name="player" />.</param>
	public abstract void DrawVideo(MediaPlayer player, Rect rectangle);

	/// <summary>Draws a video into the specified region and applies the specified animation clock. </summary>
	/// <param name="player">The media to draw.</param>
	/// <param name="rectangle">The area in which to draw the media.</param>
	/// <param name="rectangleAnimations">The clock with which to animate the rectangle's size and dimensions, or null for no animation. This clock must be created from an <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> that can animate <see cref="T:System.Windows.Rect" /> objects.</param>
	public abstract void DrawVideo(MediaPlayer player, Rect rectangle, AnimationClock rectangleAnimations);

	/// <summary>Pushes the specified clip region onto the drawing context.  </summary>
	/// <param name="clipGeometry">The clip region to apply to subsequent drawing commands.</param>
	public abstract void PushClip(Geometry clipGeometry);

	/// <summary>Pushes the specified opacity mask onto the drawing context. </summary>
	/// <param name="opacityMask">The opacity mask to apply to subsequent drawings. The alpha values of this brush determine the opacity of the drawing to which it is applied.</param>
	public abstract void PushOpacityMask(Brush opacityMask);

	/// <summary>Pushes the specified opacity setting onto the drawing context. </summary>
	/// <param name="opacity">The opacity factor to apply to subsequent drawing commands. This factor is cumulative with previous <see cref="M:System.Windows.Media.DrawingContext.PushOpacity(System.Double)" /> operations.</param>
	public abstract void PushOpacity(double opacity);

	/// <summary>Pushes the specified opacity setting onto the drawing context and applies the specified animation clock. </summary>
	/// <param name="opacity">The opacity factor to apply to subsequent drawing commands. This factor is cumulative with previous <see cref="M:System.Windows.Media.DrawingContext.PushOpacity(System.Double)" /> operations.</param>
	/// <param name="opacityAnimations">The clock with which to animate the opacity value, or null for no animation. This clock must be created from an <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> that can animate <see cref="T:System.Double" /> values.</param>
	public abstract void PushOpacity(double opacity, AnimationClock opacityAnimations);

	/// <summary>Pushes the specified <see cref="T:System.Windows.Media.Transform" /> onto the drawing context. </summary>
	/// <param name="transform">The transform to apply to subsequent drawing commands.</param>
	public abstract void PushTransform(Transform transform);

	/// <summary>Pushes the specified <see cref="T:System.Windows.Media.GuidelineSet" /> onto the drawing context. </summary>
	/// <param name="guidelines">The guideline set to apply to subsequent drawing commands.</param>
	public abstract void PushGuidelineSet(GuidelineSet guidelines);

	internal abstract void PushGuidelineY1(double coordinate);

	internal abstract void PushGuidelineY2(double leadingCoordinate, double offsetToDrivenCoordinate);

	/// <summary>Pushes the specified <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> onto the drawing context. </summary>
	/// <param name="effect">The effect to apply to subsequent drawings.</param>
	/// <param name="effectInput">The area to which the effect is applied, or null if the effect is to be applied to the entire area of subsequent drawings.</param>
	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	public abstract void PushEffect(BitmapEffect effect, BitmapEffectInput effectInput);

	/// <summary>Pops the last opacity mask, opacity, clip, effect, or transform operation that was pushed onto the drawing context. </summary>
	public abstract void Pop();
}
