using System.ComponentModel;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.PresentationFramework;

namespace System.Windows.Shapes;

/// <summary>Provides a base class for shape elements, such as <see cref="T:System.Windows.Shapes.Ellipse" />, <see cref="T:System.Windows.Shapes.Polygon" />, and <see cref="T:System.Windows.Shapes.Rectangle" />.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public abstract class Shape : FrameworkElement
{
	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Shape.Stretch" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Shape.Stretch" /> dependency property.</returns>
	public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(Shape), new FrameworkPropertyMetadata(Stretch.None, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Shape.Fill" /> dependency property. This field is read-only.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Shape.Fill" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(Shape), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Shape.Stroke" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Shape.Stroke" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke", typeof(Brush), typeof(Shape), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender, OnPenChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Shape.StrokeThickness" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Shape.StrokeThickness" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), typeof(Shape), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Shape.StrokeStartLineCap" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Shape.StrokeStartLineCap" /> dependency property.</returns>
	public static readonly DependencyProperty StrokeStartLineCapProperty = DependencyProperty.Register("StrokeStartLineCap", typeof(PenLineCap), typeof(Shape), new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged), System.Windows.Media.ValidateEnums.IsPenLineCapValid);

	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Shape.StrokeEndLineCap" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Shape.StrokeEndLineCap" /> dependency property.</returns>
	public static readonly DependencyProperty StrokeEndLineCapProperty = DependencyProperty.Register("StrokeEndLineCap", typeof(PenLineCap), typeof(Shape), new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged), System.Windows.Media.ValidateEnums.IsPenLineCapValid);

	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Shape.StrokeDashCap" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Shape.StrokeDashCap" /> dependency property.</returns>
	public static readonly DependencyProperty StrokeDashCapProperty = DependencyProperty.Register("StrokeDashCap", typeof(PenLineCap), typeof(Shape), new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged), System.Windows.Media.ValidateEnums.IsPenLineCapValid);

	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Shape.StrokeLineJoin" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Shape.StrokeLineJoin" /> dependency property.</returns>
	public static readonly DependencyProperty StrokeLineJoinProperty = DependencyProperty.Register("StrokeLineJoin", typeof(PenLineJoin), typeof(Shape), new FrameworkPropertyMetadata(PenLineJoin.Miter, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged), System.Windows.Media.ValidateEnums.IsPenLineJoinValid);

	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Shape.StrokeMiterLimit" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Shape.StrokeMiterLimit" /> dependency property.</returns>
	public static readonly DependencyProperty StrokeMiterLimitProperty = DependencyProperty.Register("StrokeMiterLimit", typeof(double), typeof(Shape), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Shape.StrokeDashOffset" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Shape.StrokeDashOffset" /> dependency property.</returns>
	public static readonly DependencyProperty StrokeDashOffsetProperty = DependencyProperty.Register("StrokeDashOffset", typeof(double), typeof(Shape), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Shape.StrokeDashArray" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Shape.StrokeDashArray" /> dependency property.</returns>
	public static readonly DependencyProperty StrokeDashArrayProperty = DependencyProperty.Register("StrokeDashArray", typeof(DoubleCollection), typeof(Shape), new FrameworkPropertyMetadata(new FreezableDefaultValueFactory(DoubleCollection.Empty), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged));

	private Pen _pen;

	private Geometry _renderedGeometry = Geometry.Empty;

	private static UncommonField<BoxedMatrix> StretchMatrixField = new UncommonField<BoxedMatrix>(null);

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Stretch" /> enumeration value that describes how the shape fills its allocated space.</summary>
	/// <returns>One of the <see cref="T:System.Windows.Media.Stretch" /> enumeration values.</returns>
	public Stretch Stretch
	{
		get
		{
			return (Stretch)GetValue(StretchProperty);
		}
		set
		{
			SetValue(StretchProperty, value);
		}
	}

	/// <summary>Gets a value that represents the final rendered <see cref="T:System.Windows.Media.Geometry" /> of a <see cref="T:System.Windows.Shapes.Shape" />.</summary>
	/// <returns>The final rendered <see cref="T:System.Windows.Media.Geometry" /> of a <see cref="T:System.Windows.Shapes.Shape" />.</returns>
	public virtual Geometry RenderedGeometry
	{
		get
		{
			EnsureRenderedGeometry();
			Geometry geometry = _renderedGeometry.CloneCurrentValue();
			if (geometry == null || geometry == Geometry.Empty)
			{
				return Geometry.Empty;
			}
			if (geometry == _renderedGeometry)
			{
				geometry = geometry.Clone();
				geometry.Freeze();
			}
			return geometry;
		}
	}

	/// <summary>Gets a value that represents a <see cref="T:System.Windows.Media.Transform" /> that is applied to the geometry of a <see cref="T:System.Windows.Shapes.Shape" /> prior to when it is drawn.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Transform" /> that is applied to the geometry of a <see cref="T:System.Windows.Shapes.Shape" /> prior to when it is drawn.</returns>
	public virtual Transform GeometryTransform
	{
		get
		{
			BoxedMatrix value = StretchMatrixField.GetValue(this);
			if (value == null)
			{
				return Transform.Identity;
			}
			return new MatrixTransform(value.Value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> that specifies how the shape's interior is painted. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Brush" /> that describes how the shape's interior is painted. The default is null.</returns>
	public Brush Fill
	{
		get
		{
			return (Brush)GetValue(FillProperty);
		}
		set
		{
			SetValue(FillProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> that specifies how the <see cref="T:System.Windows.Shapes.Shape" /> outline is painted. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Brush" /> that specifies how the <see cref="T:System.Windows.Shapes.Shape" /> outline is painted. The default is null.</returns>
	public Brush Stroke
	{
		get
		{
			return (Brush)GetValue(StrokeProperty);
		}
		set
		{
			SetValue(StrokeProperty, value);
		}
	}

	/// <summary>Gets or sets the width of the <see cref="T:System.Windows.Shapes.Shape" /> outline. </summary>
	/// <returns>The width of the <see cref="T:System.Windows.Shapes.Shape" /> outline.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double StrokeThickness
	{
		get
		{
			return (double)GetValue(StrokeThicknessProperty);
		}
		set
		{
			SetValue(StrokeThicknessProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.PenLineCap" /> enumeration value that describes the <see cref="T:System.Windows.Shapes.Shape" /> at the start of a <see cref="P:System.Windows.Shapes.Shape.Stroke" />. </summary>
	/// <returns>One of the <see cref="T:System.Windows.Media.PenLineCap" /> enumeration values. The default is <see cref="F:System.Windows.Media.PenLineCap.Flat" />.</returns>
	public PenLineCap StrokeStartLineCap
	{
		get
		{
			return (PenLineCap)GetValue(StrokeStartLineCapProperty);
		}
		set
		{
			SetValue(StrokeStartLineCapProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.PenLineCap" /> enumeration value that describes the <see cref="T:System.Windows.Shapes.Shape" /> at the end of a line. </summary>
	/// <returns>One of the enumeration values for <see cref="T:System.Windows.Media.PenLineCap" />. The default is <see cref="F:System.Windows.Media.PenLineCap.Flat" />.</returns>
	public PenLineCap StrokeEndLineCap
	{
		get
		{
			return (PenLineCap)GetValue(StrokeEndLineCapProperty);
		}
		set
		{
			SetValue(StrokeEndLineCapProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.PenLineCap" /> enumeration value that specifies how the ends of a dash are drawn. </summary>
	/// <returns>One of the enumeration values for <see cref="T:System.Windows.Media.PenLineCap" />. The default is <see cref="F:System.Windows.Media.PenLineCap.Flat" />. </returns>
	public PenLineCap StrokeDashCap
	{
		get
		{
			return (PenLineCap)GetValue(StrokeDashCapProperty);
		}
		set
		{
			SetValue(StrokeDashCapProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.PenLineJoin" /> enumeration value that specifies the type of join that is used at the vertices of a <see cref="T:System.Windows.Shapes.Shape" />.</summary>
	/// <returns>One of the enumeration values for <see cref="T:System.Windows.Media.PenLineJoin" /></returns>
	public PenLineJoin StrokeLineJoin
	{
		get
		{
			return (PenLineJoin)GetValue(StrokeLineJoinProperty);
		}
		set
		{
			SetValue(StrokeLineJoinProperty, value);
		}
	}

	/// <summary>Gets or sets a limit on the ratio of the miter length to half the <see cref="P:System.Windows.Shapes.Shape.StrokeThickness" /> of a <see cref="T:System.Windows.Shapes.Shape" /> element. </summary>
	/// <returns>The limit on the ratio of the miter length to the <see cref="P:System.Windows.Shapes.Shape.StrokeThickness" /> of a <see cref="T:System.Windows.Shapes.Shape" /> element. This value is always a positive number that is greater than or equal to 1.</returns>
	public double StrokeMiterLimit
	{
		get
		{
			return (double)GetValue(StrokeMiterLimitProperty);
		}
		set
		{
			SetValue(StrokeMiterLimitProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Double" /> that specifies the distance within the dash pattern where a dash begins.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the distance within the dash pattern where a dash begins.</returns>
	public double StrokeDashOffset
	{
		get
		{
			return (double)GetValue(StrokeDashOffsetProperty);
		}
		set
		{
			SetValue(StrokeDashOffsetProperty, value);
		}
	}

	/// <summary>Gets or sets a collection of <see cref="T:System.Double" /> values that indicate the pattern of dashes and gaps that is used to outline shapes. </summary>
	/// <returns>A collection of <see cref="T:System.Double" /> values that specify the pattern of dashes and gaps. </returns>
	public DoubleCollection StrokeDashArray
	{
		get
		{
			return (DoubleCollection)GetValue(StrokeDashArrayProperty);
		}
		set
		{
			SetValue(StrokeDashArrayProperty, value);
		}
	}

	/// <summary>Gets a value that represents the <see cref="T:System.Windows.Media.Geometry" /> of the <see cref="T:System.Windows.Shapes.Shape" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Geometry" /> of the <see cref="T:System.Windows.Shapes.Shape" />.</returns>
	protected abstract Geometry DefiningGeometry { get; }

	internal bool IsPenNoOp
	{
		get
		{
			double strokeThickness = StrokeThickness;
			if (Stroke != null && !double.IsNaN(strokeThickness))
			{
				return DoubleUtil.IsZero(strokeThickness);
			}
			return true;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shapes.Shape" /> class.</summary>
	protected Shape()
	{
	}

	private static void OnPenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Shape)d)._pen = null;
	}

	/// <summary>Measures a <see cref="T:System.Windows.Shapes.Shape" /> during the first layout pass prior to arranging it.</summary>
	/// <returns>The maximum <see cref="T:System.Windows.Size" /> for the <see cref="T:System.Windows.Shapes.Shape" />.</returns>
	/// <param name="constraint">A maximum <see cref="T:System.Windows.Size" /> to not exceed.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		CacheDefiningGeometry();
		Stretch stretch = Stretch;
		Size size = ((stretch != 0) ? GetStretchedRenderSize(stretch, GetStrokeThickness(), constraint, GetDefiningGeometryBounds()) : GetNaturalSize());
		if (SizeIsInvalidOrEmpty(size))
		{
			size = new Size(0.0, 0.0);
			_renderedGeometry = Geometry.Empty;
		}
		return size;
	}

	/// <summary>Arranges a <see cref="T:System.Windows.Shapes.Shape" /> by evaluating its <see cref="P:System.Windows.Shapes.Shape.RenderedGeometry" /> and <see cref="P:System.Windows.Shapes.Shape.Stretch" /> properties.</summary>
	/// <returns>The final size of the arranged <see cref="T:System.Windows.Shapes.Shape" /> element.</returns>
	/// <param name="finalSize">The final evaluated size of the <see cref="T:System.Windows.Shapes.Shape" />.</param>
	protected override Size ArrangeOverride(Size finalSize)
	{
		Stretch stretch = Stretch;
		Size size;
		if (stretch == Stretch.None)
		{
			StretchMatrixField.ClearValue(this);
			ResetRenderedGeometry();
			size = finalSize;
		}
		else
		{
			size = GetStretchedRenderSizeAndSetStretchMatrix(stretch, GetStrokeThickness(), finalSize, GetDefiningGeometryBounds());
		}
		if (SizeIsInvalidOrEmpty(size))
		{
			size = new Size(0.0, 0.0);
			_renderedGeometry = Geometry.Empty;
		}
		return size;
	}

	/// <summary>Provides a means to change the default appearance of a <see cref="T:System.Windows.Shapes.Shape" /> element.</summary>
	/// <param name="drawingContext">A <see cref="T:System.Windows.Media.DrawingContext" /> object that is drawn during the rendering pass of this <see cref="T:System.Windows.Shapes.Shape" />.</param>
	protected override void OnRender(DrawingContext drawingContext)
	{
		EnsureRenderedGeometry();
		if (_renderedGeometry != Geometry.Empty)
		{
			drawingContext.DrawGeometry(Fill, GetPen(), _renderedGeometry);
		}
	}

	internal bool SizeIsInvalidOrEmpty(Size size)
	{
		if (!double.IsNaN(size.Width) && !double.IsNaN(size.Height))
		{
			return size.IsEmpty;
		}
		return true;
	}

	internal double GetStrokeThickness()
	{
		if (IsPenNoOp)
		{
			return 0.0;
		}
		return Math.Abs(StrokeThickness);
	}

	internal Pen GetPen()
	{
		if (IsPenNoOp)
		{
			return null;
		}
		if (_pen == null)
		{
			double num = 0.0;
			num = Math.Abs(StrokeThickness);
			_pen = new Pen();
			_pen.CanBeInheritanceContext = false;
			_pen.Thickness = num;
			_pen.Brush = Stroke;
			_pen.StartLineCap = StrokeStartLineCap;
			_pen.EndLineCap = StrokeEndLineCap;
			_pen.DashCap = StrokeDashCap;
			_pen.LineJoin = StrokeLineJoin;
			_pen.MiterLimit = StrokeMiterLimit;
			DoubleCollection doubleCollection = null;
			if (GetValueSource(StrokeDashArrayProperty, null, out var hasModifiers) != BaseValueSourceInternal.Default || hasModifiers)
			{
				doubleCollection = StrokeDashArray;
			}
			double strokeDashOffset = StrokeDashOffset;
			if (doubleCollection != null || strokeDashOffset != 0.0)
			{
				_pen.DashStyle = new DashStyle(doubleCollection, strokeDashOffset);
			}
		}
		return _pen;
	}

	internal static bool IsDoubleFiniteNonNegative(object o)
	{
		double num = (double)o;
		if (!double.IsInfinity(num) && !double.IsNaN(num))
		{
			return !(num < 0.0);
		}
		return false;
	}

	internal static bool IsDoubleFinite(object o)
	{
		double d = (double)o;
		if (!double.IsInfinity(d))
		{
			return !double.IsNaN(d);
		}
		return false;
	}

	internal static bool IsDoubleFiniteOrNaN(object o)
	{
		return !double.IsInfinity((double)o);
	}

	internal virtual void CacheDefiningGeometry()
	{
	}

	internal Size GetStretchedRenderSize(Stretch mode, double strokeThickness, Size availableSize, Rect geometryBounds)
	{
		GetStretchMetrics(mode, strokeThickness, availableSize, geometryBounds, out var _, out var _, out var _, out var _, out var stretchedSize);
		return stretchedSize;
	}

	internal Size GetStretchedRenderSizeAndSetStretchMatrix(Stretch mode, double strokeThickness, Size availableSize, Rect geometryBounds)
	{
		GetStretchMetrics(mode, strokeThickness, availableSize, geometryBounds, out var xScale, out var yScale, out var dX, out var dY, out var stretchedSize);
		Matrix identity = Matrix.Identity;
		identity.ScaleAt(xScale, yScale, geometryBounds.Location.X, geometryBounds.Location.Y);
		identity.Translate(dX, dY);
		StretchMatrixField.SetValue(this, new BoxedMatrix(identity));
		ResetRenderedGeometry();
		return stretchedSize;
	}

	internal void ResetRenderedGeometry()
	{
		_renderedGeometry = null;
	}

	internal void GetStretchMetrics(Stretch mode, double strokeThickness, Size availableSize, Rect geometryBounds, out double xScale, out double yScale, out double dX, out double dY, out Size stretchedSize)
	{
		if (!geometryBounds.IsEmpty)
		{
			double num = strokeThickness / 2.0;
			bool flag = false;
			xScale = Math.Max(availableSize.Width - strokeThickness, 0.0);
			yScale = Math.Max(availableSize.Height - strokeThickness, 0.0);
			dX = num - geometryBounds.Left;
			dY = num - geometryBounds.Top;
			if (geometryBounds.Width > xScale * double.Epsilon)
			{
				xScale /= geometryBounds.Width;
			}
			else
			{
				xScale = 1.0;
				if (geometryBounds.Width == 0.0)
				{
					flag = true;
				}
			}
			if (geometryBounds.Height > yScale * double.Epsilon)
			{
				yScale /= geometryBounds.Height;
			}
			else
			{
				yScale = 1.0;
				if (geometryBounds.Height == 0.0)
				{
					flag = true;
				}
			}
			if (mode != Stretch.Fill && !flag)
			{
				if (mode == Stretch.Uniform)
				{
					if (yScale > xScale)
					{
						yScale = xScale;
					}
					else
					{
						xScale = yScale;
					}
				}
				else if (xScale > yScale)
				{
					yScale = xScale;
				}
				else
				{
					xScale = yScale;
				}
			}
			stretchedSize = new Size(geometryBounds.Width * xScale + strokeThickness, geometryBounds.Height * yScale + strokeThickness);
		}
		else
		{
			xScale = (yScale = 1.0);
			dX = (dY = 0.0);
			stretchedSize = new Size(0.0, 0.0);
		}
	}

	internal virtual Size GetNaturalSize()
	{
		Geometry definingGeometry = DefiningGeometry;
		Pen pen = GetPen();
		DashStyle dashStyle = null;
		if (pen != null)
		{
			dashStyle = pen.DashStyle;
			if (dashStyle != null)
			{
				pen.DashStyle = null;
			}
		}
		Rect renderBounds = definingGeometry.GetRenderBounds(pen);
		if (dashStyle != null)
		{
			pen.DashStyle = dashStyle;
		}
		return new Size(Math.Max(renderBounds.Right, 0.0), Math.Max(renderBounds.Bottom, 0.0));
	}

	internal virtual Rect GetDefiningGeometryBounds()
	{
		return DefiningGeometry.Bounds;
	}

	internal void EnsureRenderedGeometry()
	{
		if (_renderedGeometry != null)
		{
			return;
		}
		_renderedGeometry = DefiningGeometry;
		if (Stretch != 0)
		{
			Geometry geometry = _renderedGeometry.CloneCurrentValue();
			if (_renderedGeometry == geometry)
			{
				_renderedGeometry = geometry.Clone();
			}
			else
			{
				_renderedGeometry = geometry;
			}
			Transform transform = _renderedGeometry.Transform;
			Matrix matrix = StretchMatrixField.GetValue(this)?.Value ?? Matrix.Identity;
			if (transform == null || transform.IsIdentity)
			{
				_renderedGeometry.Transform = new MatrixTransform(matrix);
			}
			else
			{
				_renderedGeometry.Transform = new MatrixTransform(transform.Value * matrix);
			}
		}
	}
}
