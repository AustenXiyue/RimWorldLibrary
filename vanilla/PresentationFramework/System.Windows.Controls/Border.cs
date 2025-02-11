using System.Windows.Media;
using MS.Internal;
using MS.Internal.PresentationFramework;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Draws a border, background, or both around another element. </summary>
public class Border : Decorator
{
	private struct Radii
	{
		internal double LeftTop;

		internal double TopLeft;

		internal double TopRight;

		internal double RightTop;

		internal double RightBottom;

		internal double BottomRight;

		internal double BottomLeft;

		internal double LeftBottom;

		internal Radii(CornerRadius radii, Thickness borders, bool outer)
		{
			double num = 0.5 * borders.Left;
			double num2 = 0.5 * borders.Top;
			double num3 = 0.5 * borders.Right;
			double num4 = 0.5 * borders.Bottom;
			if (outer)
			{
				if (DoubleUtil.IsZero(radii.TopLeft))
				{
					LeftTop = (TopLeft = 0.0);
				}
				else
				{
					LeftTop = radii.TopLeft + num;
					TopLeft = radii.TopLeft + num2;
				}
				if (DoubleUtil.IsZero(radii.TopRight))
				{
					TopRight = (RightTop = 0.0);
				}
				else
				{
					TopRight = radii.TopRight + num2;
					RightTop = radii.TopRight + num3;
				}
				if (DoubleUtil.IsZero(radii.BottomRight))
				{
					RightBottom = (BottomRight = 0.0);
				}
				else
				{
					RightBottom = radii.BottomRight + num3;
					BottomRight = radii.BottomRight + num4;
				}
				if (DoubleUtil.IsZero(radii.BottomLeft))
				{
					BottomLeft = (LeftBottom = 0.0);
					return;
				}
				BottomLeft = radii.BottomLeft + num4;
				LeftBottom = radii.BottomLeft + num;
			}
			else
			{
				LeftTop = Math.Max(0.0, radii.TopLeft - num);
				TopLeft = Math.Max(0.0, radii.TopLeft - num2);
				TopRight = Math.Max(0.0, radii.TopRight - num2);
				RightTop = Math.Max(0.0, radii.TopRight - num3);
				RightBottom = Math.Max(0.0, radii.BottomRight - num3);
				BottomRight = Math.Max(0.0, radii.BottomRight - num4);
				BottomLeft = Math.Max(0.0, radii.BottomLeft - num4);
				LeftBottom = Math.Max(0.0, radii.BottomLeft - num);
			}
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Border.BorderThickness" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Border.BorderThickness" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty BorderThicknessProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Border.Padding" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Border.Padding" /> dependency property.</returns>
	public static readonly DependencyProperty PaddingProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Border.CornerRadius" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Border.CornerRadius" /> dependency property.</returns>
	public static readonly DependencyProperty CornerRadiusProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Border.BorderBrush" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Border.BorderBrush" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty BorderBrushProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Border.Background" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Border.Background" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty BackgroundProperty;

	private bool _useComplexRenderCodePath;

	private static readonly UncommonField<StreamGeometry> BorderGeometryField;

	private static readonly UncommonField<StreamGeometry> BackgroundGeometryField;

	private static readonly UncommonField<Pen> LeftPenField;

	private static readonly UncommonField<Pen> RightPenField;

	private static readonly UncommonField<Pen> TopPenField;

	private static readonly UncommonField<Pen> BottomPenField;

	/// <summary>Gets or sets the relative <see cref="T:System.Windows.Thickness" /> of a <see cref="T:System.Windows.Controls.Border" />.  </summary>
	/// <returns>The <see cref="T:System.Windows.Thickness" /> that describes the width of the boundaries of the <see cref="T:System.Windows.Controls.Border" />. This property has no default value.</returns>
	public Thickness BorderThickness
	{
		get
		{
			return (Thickness)GetValue(BorderThicknessProperty);
		}
		set
		{
			SetValue(BorderThicknessProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Thickness" /> value that describes the amount of space between a <see cref="T:System.Windows.Controls.Border" /> and its child element.  </summary>
	/// <returns>The <see cref="T:System.Windows.Thickness" /> that describes the amount of space between a <see cref="T:System.Windows.Controls.Border" /> and its single child element. This property has no default value.</returns>
	public Thickness Padding
	{
		get
		{
			return (Thickness)GetValue(PaddingProperty);
		}
		set
		{
			SetValue(PaddingProperty, value);
		}
	}

	/// <summary>Gets or sets a value that represents the degree to which the corners of a <see cref="T:System.Windows.Controls.Border" /> are rounded.  </summary>
	/// <returns>The <see cref="T:System.Windows.CornerRadius" /> that describes the degree to which corners are rounded. This property has no default value.</returns>
	public CornerRadius CornerRadius
	{
		get
		{
			return (CornerRadius)GetValue(CornerRadiusProperty);
		}
		set
		{
			SetValue(CornerRadiusProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> that draws the outer border color.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Brush" /> that draws the outer border color. This property has no default value.</returns>
	public Brush BorderBrush
	{
		get
		{
			return (Brush)GetValue(BorderBrushProperty);
		}
		set
		{
			SetValue(BorderBrushProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> that fills the area between the bounds of a <see cref="T:System.Windows.Controls.Border" />.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Brush" /> that draws the background. This property has no default value.</returns>
	public Brush Background
	{
		get
		{
			return (Brush)GetValue(BackgroundProperty);
		}
		set
		{
			SetValue(BackgroundProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 9;

	private StreamGeometry BorderGeometryCache
	{
		get
		{
			return BorderGeometryField.GetValue(this);
		}
		set
		{
			if (value == null)
			{
				BorderGeometryField.ClearValue(this);
			}
			else
			{
				BorderGeometryField.SetValue(this, value);
			}
		}
	}

	private StreamGeometry BackgroundGeometryCache
	{
		get
		{
			return BackgroundGeometryField.GetValue(this);
		}
		set
		{
			if (value == null)
			{
				BackgroundGeometryField.ClearValue(this);
			}
			else
			{
				BackgroundGeometryField.SetValue(this, value);
			}
		}
	}

	private Pen LeftPenCache
	{
		get
		{
			return LeftPenField.GetValue(this);
		}
		set
		{
			if (value == null)
			{
				LeftPenField.ClearValue(this);
			}
			else
			{
				LeftPenField.SetValue(this, value);
			}
		}
	}

	private Pen RightPenCache
	{
		get
		{
			return RightPenField.GetValue(this);
		}
		set
		{
			if (value == null)
			{
				RightPenField.ClearValue(this);
			}
			else
			{
				RightPenField.SetValue(this, value);
			}
		}
	}

	private Pen TopPenCache
	{
		get
		{
			return TopPenField.GetValue(this);
		}
		set
		{
			if (value == null)
			{
				TopPenField.ClearValue(this);
			}
			else
			{
				TopPenField.SetValue(this, value);
			}
		}
	}

	private Pen BottomPenCache
	{
		get
		{
			return BottomPenField.GetValue(this);
		}
		set
		{
			if (value == null)
			{
				BottomPenField.ClearValue(this);
			}
			else
			{
				BottomPenField.SetValue(this, value);
			}
		}
	}

	static Border()
	{
		BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(Border), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnClearPenCache), IsThicknessValid);
		PaddingProperty = DependencyProperty.Register("Padding", typeof(Thickness), typeof(Border), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), IsThicknessValid);
		CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(Border), new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), IsCornerRadiusValid);
		BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(Border), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender, OnClearPenCache));
		BackgroundProperty = Panel.BackgroundProperty.AddOwner(typeof(Border), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		BorderGeometryField = new UncommonField<StreamGeometry>();
		BackgroundGeometryField = new UncommonField<StreamGeometry>();
		LeftPenField = new UncommonField<Pen>();
		RightPenField = new UncommonField<Pen>();
		TopPenField = new UncommonField<Pen>();
		BottomPenField = new UncommonField<Pen>();
		ControlsTraceLogger.AddControl(TelemetryControls.Border);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Border" /> class. </summary>
	public Border()
	{
	}

	private static void OnClearPenCache(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Border obj = (Border)d;
		obj.LeftPenCache = null;
		obj.RightPenCache = null;
		obj.TopPenCache = null;
		obj.BottomPenCache = null;
	}

	private static bool IsThicknessValid(object value)
	{
		return ((Thickness)value).IsValid(allowNegative: false, allowNaN: false, allowPositiveInfinity: false, allowNegativeInfinity: false);
	}

	private static bool IsCornerRadiusValid(object value)
	{
		return ((CornerRadius)value).IsValid(allowNegative: false, allowNaN: false, allowPositiveInfinity: false, allowNegativeInfinity: false);
	}

	/// <summary>Measures the child elements of a <see cref="T:System.Windows.Controls.Border" /> before they are arranged during the <see cref="M:System.Windows.Controls.Border.ArrangeOverride(System.Windows.Size)" /> pass.</summary>
	/// <returns>The <see cref="T:System.Windows.Size" /> that represents the upper size limit of the element.</returns>
	/// <param name="constraint">An upper <see cref="T:System.Windows.Size" /> limit that cannot be exceeded.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		UIElement child = Child;
		Size result = default(Size);
		Thickness th = BorderThickness;
		if (base.UseLayoutRounding && !FrameworkAppContextSwitches.DoNotApplyLayoutRoundingToMarginsAndBorderThickness)
		{
			DpiScale dpi = GetDpi();
			th = new Thickness(UIElement.RoundLayoutValue(th.Left, dpi.DpiScaleX), UIElement.RoundLayoutValue(th.Top, dpi.DpiScaleY), UIElement.RoundLayoutValue(th.Right, dpi.DpiScaleX), UIElement.RoundLayoutValue(th.Bottom, dpi.DpiScaleY));
		}
		Size size = HelperCollapseThickness(th);
		Size size2 = HelperCollapseThickness(Padding);
		if (child != null)
		{
			Size size3 = new Size(size.Width + size2.Width, size.Height + size2.Height);
			Size availableSize = new Size(Math.Max(0.0, constraint.Width - size3.Width), Math.Max(0.0, constraint.Height - size3.Height));
			child.Measure(availableSize);
			Size desiredSize = child.DesiredSize;
			result.Width = desiredSize.Width + size3.Width;
			result.Height = desiredSize.Height + size3.Height;
		}
		else
		{
			result = new Size(size.Width + size2.Width, size.Height + size2.Height);
		}
		return result;
	}

	/// <summary>Arranges the contents of a <see cref="T:System.Windows.Controls.Border" /> element.</summary>
	/// <returns>The <see cref="T:System.Windows.Size" /> that represents the arranged size of this <see cref="T:System.Windows.Controls.Border" /> element and its child element.</returns>
	/// <param name="finalSize">The <see cref="T:System.Windows.Size" /> this element uses to arrange its child element.</param>
	protected override Size ArrangeOverride(Size finalSize)
	{
		Thickness thickness = BorderThickness;
		if (base.UseLayoutRounding && !FrameworkAppContextSwitches.DoNotApplyLayoutRoundingToMarginsAndBorderThickness)
		{
			DpiScale dpi = GetDpi();
			thickness = new Thickness(UIElement.RoundLayoutValue(thickness.Left, dpi.DpiScaleX), UIElement.RoundLayoutValue(thickness.Top, dpi.DpiScaleY), UIElement.RoundLayoutValue(thickness.Right, dpi.DpiScaleX), UIElement.RoundLayoutValue(thickness.Bottom, dpi.DpiScaleY));
		}
		Rect rect = new Rect(finalSize);
		Rect rect2 = HelperDeflateRect(rect, thickness);
		UIElement child = Child;
		if (child != null)
		{
			Rect finalRect = HelperDeflateRect(rect2, Padding);
			child.Arrange(finalRect);
		}
		CornerRadius cornerRadius = CornerRadius;
		Brush borderBrush = BorderBrush;
		bool flag = AreUniformCorners(cornerRadius);
		_useComplexRenderCodePath = !flag;
		if (!_useComplexRenderCodePath && borderBrush != null)
		{
			SolidColorBrush solidColorBrush = borderBrush as SolidColorBrush;
			bool isUniform = thickness.IsUniform;
			_useComplexRenderCodePath = solidColorBrush == null || (solidColorBrush.Color.A < byte.MaxValue && !isUniform) || (!DoubleUtil.IsZero(cornerRadius.TopLeft) && !isUniform);
		}
		if (_useComplexRenderCodePath)
		{
			Radii radii = new Radii(cornerRadius, thickness, outer: false);
			StreamGeometry streamGeometry = null;
			if (!DoubleUtil.IsZero(rect2.Width) && !DoubleUtil.IsZero(rect2.Height))
			{
				streamGeometry = new StreamGeometry();
				using (StreamGeometryContext ctx = streamGeometry.Open())
				{
					GenerateGeometry(ctx, rect2, radii);
				}
				streamGeometry.Freeze();
				BackgroundGeometryCache = streamGeometry;
			}
			else
			{
				BackgroundGeometryCache = null;
			}
			if (!DoubleUtil.IsZero(rect.Width) && !DoubleUtil.IsZero(rect.Height))
			{
				Radii radii2 = new Radii(cornerRadius, thickness, outer: true);
				StreamGeometry streamGeometry2 = new StreamGeometry();
				using (StreamGeometryContext ctx2 = streamGeometry2.Open())
				{
					GenerateGeometry(ctx2, rect, radii2);
					if (streamGeometry != null)
					{
						GenerateGeometry(ctx2, rect2, radii);
					}
				}
				streamGeometry2.Freeze();
				BorderGeometryCache = streamGeometry2;
			}
			else
			{
				BorderGeometryCache = null;
			}
		}
		else
		{
			BackgroundGeometryCache = null;
			BorderGeometryCache = null;
		}
		return finalSize;
	}

	/// <summary>Draws the contents of a <see cref="T:System.Windows.Media.DrawingContext" /> object during the render pass of a <see cref="T:System.Windows.Controls.Border" />. </summary>
	/// <param name="dc">The <see cref="T:System.Windows.Media.DrawingContext" /> that defines the object to be drawn.</param>
	protected override void OnRender(DrawingContext dc)
	{
		bool useLayoutRounding = base.UseLayoutRounding;
		DpiScale dpi = GetDpi();
		if (_useComplexRenderCodePath)
		{
			StreamGeometry borderGeometryCache = BorderGeometryCache;
			Brush borderBrush;
			if (borderGeometryCache != null && (borderBrush = BorderBrush) != null)
			{
				dc.DrawGeometry(borderBrush, null, borderGeometryCache);
			}
			StreamGeometry backgroundGeometryCache = BackgroundGeometryCache;
			if (backgroundGeometryCache != null && (borderBrush = Background) != null)
			{
				dc.DrawGeometry(borderBrush, null, backgroundGeometryCache);
			}
			return;
		}
		Thickness borderThickness = BorderThickness;
		CornerRadius cornerRadius = CornerRadius;
		double topLeft = cornerRadius.TopLeft;
		bool flag = !DoubleUtil.IsZero(topLeft);
		Brush borderBrush2;
		if (!borderThickness.IsZero && (borderBrush2 = BorderBrush) != null)
		{
			Pen pen = LeftPenCache;
			if (pen == null)
			{
				pen = new Pen();
				pen.Brush = borderBrush2;
				if (useLayoutRounding)
				{
					pen.Thickness = UIElement.RoundLayoutValue(borderThickness.Left, dpi.DpiScaleX);
				}
				else
				{
					pen.Thickness = borderThickness.Left;
				}
				if (borderBrush2.IsFrozen)
				{
					pen.Freeze();
				}
				LeftPenCache = pen;
			}
			if (borderThickness.IsUniform)
			{
				double num = pen.Thickness * 0.5;
				Rect rectangle = new Rect(new Point(num, num), new Point(base.RenderSize.Width - num, base.RenderSize.Height - num));
				if (flag)
				{
					dc.DrawRoundedRectangle(null, pen, rectangle, topLeft, topLeft);
				}
				else
				{
					dc.DrawRectangle(null, pen, rectangle);
				}
			}
			else
			{
				if (DoubleUtil.GreaterThanZero(borderThickness.Left))
				{
					double num = pen.Thickness * 0.5;
					dc.DrawLine(pen, new Point(num, 0.0), new Point(num, base.RenderSize.Height));
				}
				if (DoubleUtil.GreaterThanZero(borderThickness.Right))
				{
					pen = RightPenCache;
					if (pen == null)
					{
						pen = new Pen();
						pen.Brush = borderBrush2;
						if (useLayoutRounding)
						{
							pen.Thickness = UIElement.RoundLayoutValue(borderThickness.Right, dpi.DpiScaleX);
						}
						else
						{
							pen.Thickness = borderThickness.Right;
						}
						if (borderBrush2.IsFrozen)
						{
							pen.Freeze();
						}
						RightPenCache = pen;
					}
					double num = pen.Thickness * 0.5;
					dc.DrawLine(pen, new Point(base.RenderSize.Width - num, 0.0), new Point(base.RenderSize.Width - num, base.RenderSize.Height));
				}
				if (DoubleUtil.GreaterThanZero(borderThickness.Top))
				{
					pen = TopPenCache;
					if (pen == null)
					{
						pen = new Pen();
						pen.Brush = borderBrush2;
						if (useLayoutRounding)
						{
							pen.Thickness = UIElement.RoundLayoutValue(borderThickness.Top, dpi.DpiScaleY);
						}
						else
						{
							pen.Thickness = borderThickness.Top;
						}
						if (borderBrush2.IsFrozen)
						{
							pen.Freeze();
						}
						TopPenCache = pen;
					}
					double num = pen.Thickness * 0.5;
					dc.DrawLine(pen, new Point(0.0, num), new Point(base.RenderSize.Width, num));
				}
				if (DoubleUtil.GreaterThanZero(borderThickness.Bottom))
				{
					pen = BottomPenCache;
					if (pen == null)
					{
						pen = new Pen();
						pen.Brush = borderBrush2;
						if (useLayoutRounding)
						{
							pen.Thickness = UIElement.RoundLayoutValue(borderThickness.Bottom, dpi.DpiScaleY);
						}
						else
						{
							pen.Thickness = borderThickness.Bottom;
						}
						if (borderBrush2.IsFrozen)
						{
							pen.Freeze();
						}
						BottomPenCache = pen;
					}
					double num = pen.Thickness * 0.5;
					dc.DrawLine(pen, new Point(0.0, base.RenderSize.Height - num), new Point(base.RenderSize.Width, base.RenderSize.Height - num));
				}
			}
		}
		Brush background = Background;
		if (background == null)
		{
			return;
		}
		Point point;
		Point point2;
		if (useLayoutRounding)
		{
			point = new Point(UIElement.RoundLayoutValue(borderThickness.Left, dpi.DpiScaleX), UIElement.RoundLayoutValue(borderThickness.Top, dpi.DpiScaleY));
			point2 = ((!FrameworkAppContextSwitches.DoNotApplyLayoutRoundingToMarginsAndBorderThickness) ? new Point(base.RenderSize.Width - UIElement.RoundLayoutValue(borderThickness.Right, dpi.DpiScaleX), base.RenderSize.Height - UIElement.RoundLayoutValue(borderThickness.Bottom, dpi.DpiScaleY)) : new Point(UIElement.RoundLayoutValue(base.RenderSize.Width - borderThickness.Right, dpi.DpiScaleX), UIElement.RoundLayoutValue(base.RenderSize.Height - borderThickness.Bottom, dpi.DpiScaleY)));
		}
		else
		{
			point = new Point(borderThickness.Left, borderThickness.Top);
			point2 = new Point(base.RenderSize.Width - borderThickness.Right, base.RenderSize.Height - borderThickness.Bottom);
		}
		if (point2.X > point.X && point2.Y > point.Y)
		{
			if (flag)
			{
				double topLeft2 = new Radii(cornerRadius, borderThickness, outer: false).TopLeft;
				dc.DrawRoundedRectangle(background, null, new Rect(point, point2), topLeft2, topLeft2);
			}
			else
			{
				dc.DrawRectangle(background, null, new Rect(point, point2));
			}
		}
	}

	private static Size HelperCollapseThickness(Thickness th)
	{
		return new Size(th.Left + th.Right, th.Top + th.Bottom);
	}

	private static bool AreUniformCorners(CornerRadius borderRadii)
	{
		double topLeft = borderRadii.TopLeft;
		if (DoubleUtil.AreClose(topLeft, borderRadii.TopRight) && DoubleUtil.AreClose(topLeft, borderRadii.BottomLeft))
		{
			return DoubleUtil.AreClose(topLeft, borderRadii.BottomRight);
		}
		return false;
	}

	private static Rect HelperDeflateRect(Rect rt, Thickness thick)
	{
		return new Rect(rt.Left + thick.Left, rt.Top + thick.Top, Math.Max(0.0, rt.Width - thick.Left - thick.Right), Math.Max(0.0, rt.Height - thick.Top - thick.Bottom));
	}

	private static void GenerateGeometry(StreamGeometryContext ctx, Rect rect, Radii radii)
	{
		Point point = new Point(radii.LeftTop, 0.0);
		Point point2 = new Point(rect.Width - radii.RightTop, 0.0);
		Point point3 = new Point(rect.Width, radii.TopRight);
		Point point4 = new Point(rect.Width, rect.Height - radii.BottomRight);
		Point point5 = new Point(rect.Width - radii.RightBottom, rect.Height);
		Point point6 = new Point(radii.LeftBottom, rect.Height);
		Point point7 = new Point(0.0, rect.Height - radii.BottomLeft);
		Point point8 = new Point(0.0, radii.TopLeft);
		if (point.X > point2.X)
		{
			double x = (point.X = radii.LeftTop / (radii.LeftTop + radii.RightTop) * rect.Width);
			point2.X = x;
		}
		if (point3.Y > point4.Y)
		{
			double y = (point3.Y = radii.TopRight / (radii.TopRight + radii.BottomRight) * rect.Height);
			point4.Y = y;
		}
		if (point5.X < point6.X)
		{
			double x2 = (point5.X = radii.LeftBottom / (radii.LeftBottom + radii.RightBottom) * rect.Width);
			point6.X = x2;
		}
		if (point7.Y < point8.Y)
		{
			double y2 = (point7.Y = radii.TopLeft / (radii.TopLeft + radii.BottomLeft) * rect.Height);
			point8.Y = y2;
		}
		Vector vector = new Vector(rect.TopLeft.X, rect.TopLeft.Y);
		point += vector;
		point2 += vector;
		point3 += vector;
		point4 += vector;
		point5 += vector;
		point6 += vector;
		point7 += vector;
		point8 += vector;
		ctx.BeginFigure(point, isFilled: true, isClosed: true);
		ctx.LineTo(point2, isStroked: true, isSmoothJoin: false);
		double num5 = rect.TopRight.X - point2.X;
		double num6 = point3.Y - rect.TopRight.Y;
		if (!DoubleUtil.IsZero(num5) || !DoubleUtil.IsZero(num6))
		{
			ctx.ArcTo(point3, new Size(num5, num6), 0.0, isLargeArc: false, SweepDirection.Clockwise, isStroked: true, isSmoothJoin: false);
		}
		ctx.LineTo(point4, isStroked: true, isSmoothJoin: false);
		num5 = rect.BottomRight.X - point5.X;
		num6 = rect.BottomRight.Y - point4.Y;
		if (!DoubleUtil.IsZero(num5) || !DoubleUtil.IsZero(num6))
		{
			ctx.ArcTo(point5, new Size(num5, num6), 0.0, isLargeArc: false, SweepDirection.Clockwise, isStroked: true, isSmoothJoin: false);
		}
		ctx.LineTo(point6, isStroked: true, isSmoothJoin: false);
		num5 = point6.X - rect.BottomLeft.X;
		num6 = rect.BottomLeft.Y - point7.Y;
		if (!DoubleUtil.IsZero(num5) || !DoubleUtil.IsZero(num6))
		{
			ctx.ArcTo(point7, new Size(num5, num6), 0.0, isLargeArc: false, SweepDirection.Clockwise, isStroked: true, isSmoothJoin: false);
		}
		ctx.LineTo(point8, isStroked: true, isSmoothJoin: false);
		num5 = point.X - rect.TopLeft.X;
		num6 = point8.Y - rect.TopLeft.Y;
		if (!DoubleUtil.IsZero(num5) || !DoubleUtil.IsZero(num6))
		{
			ctx.ArcTo(point, new Size(num5, num6), 0.0, isLargeArc: false, SweepDirection.Clockwise, isStroked: true, isSmoothJoin: false);
		}
	}
}
