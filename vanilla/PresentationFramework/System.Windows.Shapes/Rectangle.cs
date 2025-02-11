using System.ComponentModel;
using System.Windows.Media;

namespace System.Windows.Shapes;

/// <summary>Draws a rectangle.</summary>
public sealed class Rectangle : Shape
{
	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Rectangle.RadiusX" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Rectangle.RadiusX" /> dependency property.</returns>
	public static readonly DependencyProperty RadiusXProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Rectangle.RadiusY" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Rectangle.RadiusY" /> dependency property.</returns>
	public static readonly DependencyProperty RadiusYProperty;

	private Rect _rect = Rect.Empty;

	/// <summary>Gets or sets the x-axis radius of the ellipse that is used to round the corners of the rectangle.  </summary>
	/// <returns>The x-axis radius of the ellipse that is used to round the corners of the rectangle.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double RadiusX
	{
		get
		{
			return (double)GetValue(RadiusXProperty);
		}
		set
		{
			SetValue(RadiusXProperty, value);
		}
	}

	/// <summary>Gets or sets the y-axis radius of the ellipse that is used to round the corners of the rectangle.  </summary>
	/// <returns>The y-axis radius of the ellipse that is used to round the corners of the rectangle. The default is 0.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double RadiusY
	{
		get
		{
			return (double)GetValue(RadiusYProperty);
		}
		set
		{
			SetValue(RadiusYProperty, value);
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.Geometry" /> object that represents the final rendered shape. </summary>
	/// <returns>The final rendered shape.</returns>
	public override Geometry RenderedGeometry => new RectangleGeometry(_rect, RadiusX, RadiusY);

	/// <summary>Gets the <see cref="T:System.Windows.Media.Transform" /> that is applied to this <see cref="T:System.Windows.Shapes.Rectangle" />. </summary>
	/// <returns>The transform that is applied to this <see cref="T:System.Windows.Shapes.Rectangle" />.</returns>
	public override Transform GeometryTransform => Transform.Identity;

	protected override Geometry DefiningGeometry => new RectangleGeometry(_rect, RadiusX, RadiusY);

	internal override int EffectiveValuesInitialSize => 19;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shapes.Rectangle" /> class.</summary>
	public Rectangle()
	{
	}

	static Rectangle()
	{
		RadiusXProperty = DependencyProperty.Register("RadiusX", typeof(double), typeof(Rectangle), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
		RadiusYProperty = DependencyProperty.Register("RadiusY", typeof(double), typeof(Rectangle), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
		Shape.StretchProperty.OverrideMetadata(typeof(Rectangle), new FrameworkPropertyMetadata(Stretch.Fill));
	}

	protected override Size MeasureOverride(Size constraint)
	{
		if (base.Stretch == Stretch.UniformToFill)
		{
			double width = constraint.Width;
			double height = constraint.Height;
			if (double.IsInfinity(width) && double.IsInfinity(height))
			{
				return GetNaturalSize();
			}
			width = ((!double.IsInfinity(width) && !double.IsInfinity(height)) ? Math.Max(width, height) : Math.Min(width, height));
			return new Size(width, width);
		}
		return GetNaturalSize();
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		double strokeThickness = GetStrokeThickness();
		double num = strokeThickness / 2.0;
		_rect = new Rect(num, num, Math.Max(0.0, finalSize.Width - strokeThickness), Math.Max(0.0, finalSize.Height - strokeThickness));
		switch (base.Stretch)
		{
		case Stretch.None:
		{
			ref Rect rect = ref _rect;
			double width = (_rect.Height = 0.0);
			rect.Width = width;
			break;
		}
		case Stretch.Uniform:
			if (_rect.Width > _rect.Height)
			{
				_rect.Width = _rect.Height;
			}
			else
			{
				_rect.Height = _rect.Width;
			}
			break;
		case Stretch.UniformToFill:
			if (_rect.Width < _rect.Height)
			{
				_rect.Width = _rect.Height;
			}
			else
			{
				_rect.Height = _rect.Width;
			}
			break;
		}
		ResetRenderedGeometry();
		return finalSize;
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		Pen pen = GetPen();
		drawingContext.DrawRoundedRectangle(base.Fill, pen, _rect, RadiusX, RadiusY);
	}

	internal override void CacheDefiningGeometry()
	{
		double num = GetStrokeThickness() / 2.0;
		_rect = new Rect(num, num, 0.0, 0.0);
	}

	internal override Size GetNaturalSize()
	{
		double strokeThickness = GetStrokeThickness();
		return new Size(strokeThickness, strokeThickness);
	}

	internal override Rect GetDefiningGeometryBounds()
	{
		return _rect;
	}
}
