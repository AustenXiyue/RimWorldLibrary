using System.Windows.Media;
using MS.Internal.PresentationFramework;

namespace System.Windows.Shapes;

/// <summary>Draws a series of connected lines and curves. </summary>
public sealed class Path : Shape
{
	/// <summary>Identifies the <see cref="P:System.Windows.Shapes.Path.Data" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shapes.Path.Data" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(Geometry), typeof(Path), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), null);

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Geometry" /> that specifies the shape to be drawn.  </summary>
	/// <returns>A description of the shape to be drawn. </returns>
	public Geometry Data
	{
		get
		{
			return (Geometry)GetValue(DataProperty);
		}
		set
		{
			SetValue(DataProperty, value);
		}
	}

	protected override Geometry DefiningGeometry
	{
		get
		{
			Geometry geometry = Data;
			if (geometry == null)
			{
				geometry = Geometry.Empty;
			}
			return geometry;
		}
	}

	internal override int EffectiveValuesInitialSize => 13;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shapes.Path" /> class.</summary>
	public Path()
	{
	}
}
