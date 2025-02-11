using System.ComponentModel;
using System.Windows.Media;

namespace System.Windows.Shapes;

/// <summary>Draws a straight line between two points. </summary>
public sealed class Line : Shape
{
	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Line.X1" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Line.X1" /> dependency property.</returns>
	public static readonly DependencyProperty X1Property = DependencyProperty.Register("X1", typeof(double), typeof(Line), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), Shape.IsDoubleFinite);

	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Line.Y1" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Line.Y1" /> dependency property.</returns>
	public static readonly DependencyProperty Y1Property = DependencyProperty.Register("Y1", typeof(double), typeof(Line), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), Shape.IsDoubleFinite);

	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Line.X2" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Line.X2" /> dependency property.</returns>
	public static readonly DependencyProperty X2Property = DependencyProperty.Register("X2", typeof(double), typeof(Line), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), Shape.IsDoubleFinite);

	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Line.Y2" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Line.Y2" /> dependency property.</returns>
	public static readonly DependencyProperty Y2Property = DependencyProperty.Register("Y2", typeof(double), typeof(Line), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), Shape.IsDoubleFinite);

	private LineGeometry _lineGeometry;

	/// <summary>Gets or sets the x-coordinate of the <see cref="T:System.Windows.Shapes.Line" /> start point.  </summary>
	/// <returns>The x-coordinate for the start point of the line. The default is 0.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double X1
	{
		get
		{
			return (double)GetValue(X1Property);
		}
		set
		{
			SetValue(X1Property, value);
		}
	}

	/// <summary>Gets or sets the y-coordinate of the <see cref="T:System.Windows.Shapes.Line" /> start point.  </summary>
	/// <returns>The y-coordinate for the start point of the line. The default is 0.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double Y1
	{
		get
		{
			return (double)GetValue(Y1Property);
		}
		set
		{
			SetValue(Y1Property, value);
		}
	}

	/// <summary>Gets or sets the x-coordinate of the <see cref="T:System.Windows.Shapes.Line" /> end point.  </summary>
	/// <returns>The x-coordinate for the end point of the line. The default is 0.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double X2
	{
		get
		{
			return (double)GetValue(X2Property);
		}
		set
		{
			SetValue(X2Property, value);
		}
	}

	/// <summary>Gets or sets the y-coordinate of the <see cref="T:System.Windows.Shapes.Line" /> end point.  </summary>
	/// <returns>The y-coordinate for the end point of the line. The default is 0.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double Y2
	{
		get
		{
			return (double)GetValue(Y2Property);
		}
		set
		{
			SetValue(Y2Property, value);
		}
	}

	protected override Geometry DefiningGeometry => _lineGeometry;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shapes.Line" /> class. </summary>
	public Line()
	{
	}

	internal override void CacheDefiningGeometry()
	{
		Point startPoint = new Point(X1, Y1);
		Point endPoint = new Point(X2, Y2);
		_lineGeometry = new LineGeometry(startPoint, endPoint);
	}
}
