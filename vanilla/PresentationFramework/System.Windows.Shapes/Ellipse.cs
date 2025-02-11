using System.Windows.Media;

namespace System.Windows.Shapes;

/// <summary>Draws an ellipse. </summary>
public sealed class Ellipse : Shape
{
	private Rect _rect = Rect.Empty;

	/// <summary>Gets the final rendered <see cref="T:System.Windows.Media.Geometry" /> of an <see cref="T:System.Windows.Shapes.Ellipse" />.</summary>
	/// <returns>The final rendered <see cref="T:System.Windows.Media.Geometry" /> of an <see cref="T:System.Windows.Shapes.Ellipse" />.</returns>
	public override Geometry RenderedGeometry => DefiningGeometry;

	/// <summary>Gets the value of any <see cref="P:System.Windows.Media.Transform.Identity" /> transforms that are applied to the <see cref="T:System.Windows.Media.Geometry" /> of an <see cref="T:System.Windows.Shapes.Ellipse" /> before it is rendered.</summary>
	/// <returns>The value of any <see cref="P:System.Windows.Media.Transform.Identity" /> transforms that are applied to the <see cref="T:System.Windows.Media.Geometry" /> of an <see cref="T:System.Windows.Shapes.Ellipse" /> before it is rendered.</returns>
	public override Transform GeometryTransform => Transform.Identity;

	protected override Geometry DefiningGeometry
	{
		get
		{
			if (_rect.IsEmpty)
			{
				return Geometry.Empty;
			}
			return new EllipseGeometry(_rect);
		}
	}

	internal override int EffectiveValuesInitialSize => 13;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shapes.Ellipse" /> class.</summary>
	public Ellipse()
	{
	}

	static Ellipse()
	{
		Shape.StretchProperty.OverrideMetadata(typeof(Ellipse), new FrameworkPropertyMetadata(Stretch.Fill));
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
		if (!_rect.IsEmpty)
		{
			Pen pen = GetPen();
			drawingContext.DrawGeometry(base.Fill, pen, new EllipseGeometry(_rect));
		}
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
