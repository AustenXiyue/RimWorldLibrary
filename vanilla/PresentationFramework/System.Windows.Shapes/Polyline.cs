using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Shapes;

/// <summary>Draws a series of connected straight lines. </summary>
public sealed class Polyline : Shape
{
	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Polyline.Points" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Polyline.Points" /> dependency property.</returns>
	public static readonly DependencyProperty PointsProperty = DependencyProperty.Register("Points", typeof(PointCollection), typeof(Polyline), new FrameworkPropertyMetadata(new FreezableDefaultValueFactory(PointCollection.Empty), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Polyline.FillRule" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Polyline.FillRule" /> dependency property.</returns>
	public static readonly DependencyProperty FillRuleProperty = DependencyProperty.Register("FillRule", typeof(FillRule), typeof(Polyline), new FrameworkPropertyMetadata(FillRule.EvenOdd, FrameworkPropertyMetadataOptions.AffectsRender), System.Windows.Media.ValidateEnums.IsFillRuleValid);

	private Geometry _polylineGeometry;

	/// <summary>Gets or sets a collection that contains the vertex points of the <see cref="T:System.Windows.Shapes.Polyline" />.  </summary>
	/// <returns>A collection of <see cref="T:System.Windows.Point" /> structures that describe the vertex points of the <see cref="T:System.Windows.Shapes.Polyline" />. The default is a null  reference (Nothing in Visual Basic).</returns>
	public PointCollection Points
	{
		get
		{
			return (PointCollection)GetValue(PointsProperty);
		}
		set
		{
			SetValue(PointsProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.FillRule" /> enumeration that specifies how the interior fill of the shape is determined.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.Media.FillRule" /> enumeration values. The default is <see cref="F:System.Windows.Media.FillRule.EvenOdd" />.</returns>
	public FillRule FillRule
	{
		get
		{
			return (FillRule)GetValue(FillRuleProperty);
		}
		set
		{
			SetValue(FillRuleProperty, value);
		}
	}

	protected override Geometry DefiningGeometry => _polylineGeometry;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shapes.Polyline" /> class.</summary>
	public Polyline()
	{
	}

	internal override void CacheDefiningGeometry()
	{
		PointCollection points = Points;
		PathFigure pathFigure = new PathFigure();
		if (points == null)
		{
			_polylineGeometry = Geometry.Empty;
			return;
		}
		if (points.Count > 0)
		{
			pathFigure.StartPoint = points[0];
			if (points.Count > 1)
			{
				Point[] array = new Point[points.Count - 1];
				for (int i = 1; i < points.Count; i++)
				{
					array[i - 1] = points[i];
				}
				pathFigure.Segments.Add(new PolyLineSegment(array, isStroked: true));
			}
		}
		PathGeometry pathGeometry = new PathGeometry();
		pathGeometry.Figures.Add(pathFigure);
		pathGeometry.FillRule = FillRule;
		if (pathGeometry.Bounds == Rect.Empty)
		{
			_polylineGeometry = Geometry.Empty;
		}
		else
		{
			_polylineGeometry = pathGeometry;
		}
	}
}
