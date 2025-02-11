using System.Windows.Markup;
using System.Windows.Media.Animation;
using MS.Internal;

namespace System.Windows.Media;

/// <summary> An abstract class that describes a gradient, composed of gradient stops. Classes that inherit from <see cref="T:System.Windows.Media.GradientBrush" /> describe different ways of interpreting gradient stops. </summary>
[ContentProperty("GradientStops")]
public abstract class GradientBrush : Brush
{
	/// <summary> Identifies the <see cref="P:System.Windows.Media.GradientBrush.ColorInterpolationMode" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.GradientBrush.ColorInterpolationMode" /> dependency property.</returns>
	public static readonly DependencyProperty ColorInterpolationModeProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.GradientBrush.MappingMode" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.GradientBrush.MappingMode" /> dependency property.</returns>
	public static readonly DependencyProperty MappingModeProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.GradientBrush.SpreadMethod" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.GradientBrush.SpreadMethod" /> dependency property.</returns>
	public static readonly DependencyProperty SpreadMethodProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.GradientBrush.GradientStops" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.GradientBrush.GradientStops" /> dependency property .</returns>
	public static readonly DependencyProperty GradientStopsProperty;

	internal const ColorInterpolationMode c_ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;

	internal const BrushMappingMode c_MappingMode = BrushMappingMode.RelativeToBoundingBox;

	internal const GradientSpreadMethod c_SpreadMethod = GradientSpreadMethod.Pad;

	internal static GradientStopCollection s_GradientStops;

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.ColorInterpolationMode" /> enumeration that specifies how the gradient's colors are interpolated.   </summary>
	/// <returns>Specifies how the colors in a gradient are interpolated. The default is <see cref="F:System.Windows.Media.ColorInterpolationMode.SRgbLinearInterpolation" />. </returns>
	public ColorInterpolationMode ColorInterpolationMode
	{
		get
		{
			return (ColorInterpolationMode)GetValue(ColorInterpolationModeProperty);
		}
		set
		{
			SetValueInternal(ColorInterpolationModeProperty, value);
		}
	}

	/// <summary> Gets or sets a <see cref="T:System.Windows.Media.BrushMappingMode" /> enumeration that specifies whether the gradient brush's positioning coordinates are absolute or relative to the output area.   </summary>
	/// <returns>A <see cref="T:System.Windows.Media.BrushMappingMode" /> value that specifies how the gradient brush's positioning coordinates are interpreted. The default is <see cref="F:System.Windows.Media.BrushMappingMode.RelativeToBoundingBox" />.</returns>
	public BrushMappingMode MappingMode
	{
		get
		{
			return (BrushMappingMode)GetValue(MappingModeProperty);
		}
		set
		{
			SetValueInternal(MappingModeProperty, value);
		}
	}

	/// <summary> Gets or sets the type of spread method that specifies how to draw a gradient that starts or ends inside the bounds of the object to be painted. </summary>
	/// <returns>The type of spread method used to paint the gradient. The default is <see cref="F:System.Windows.Media.GradientSpreadMethod.Pad" />.</returns>
	public GradientSpreadMethod SpreadMethod
	{
		get
		{
			return (GradientSpreadMethod)GetValue(SpreadMethodProperty);
		}
		set
		{
			SetValueInternal(SpreadMethodProperty, value);
		}
	}

	/// <summary>Gets or sets the brush's gradient stops. </summary>
	/// <returns>A collection of the <see cref="T:System.Windows.Media.GradientStop" /> objects associated with the brush, each of which specifies a color and an offset along the brush's gradient axis. The default is an empty <see cref="T:System.Windows.Media.GradientStopCollection" />. </returns>
	public GradientStopCollection GradientStops
	{
		get
		{
			return (GradientStopCollection)GetValue(GradientStopsProperty);
		}
		set
		{
			SetValueInternal(GradientStopsProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GradientBrush" /> class. </summary>
	protected GradientBrush()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GradientBrush" /> class with the specified <see cref="T:System.Windows.Media.GradientStopCollection" />. </summary>
	/// <param name="gradientStopCollection">The <see cref="T:System.Windows.Media.GradientStopCollection" /> used to specify the location and color of the transition points in a gradient.</param>
	protected GradientBrush(GradientStopCollection gradientStopCollection)
	{
		GradientStops = gradientStopCollection;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GradientBrush" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new GradientBrush Clone()
	{
		return (GradientBrush)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GradientBrush" /> object, making deep copies of this object's current values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new GradientBrush CloneCurrentValue()
	{
		return (GradientBrush)base.CloneCurrentValue();
	}

	private static void ColorInterpolationModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GradientBrush)d).PropertyChanged(ColorInterpolationModeProperty);
	}

	private static void MappingModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GradientBrush)d).PropertyChanged(MappingModeProperty);
	}

	private static void SpreadMethodPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GradientBrush)d).PropertyChanged(SpreadMethodProperty);
	}

	private static void GradientStopsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GradientBrush)d).PropertyChanged(GradientStopsProperty);
	}

	static GradientBrush()
	{
		s_GradientStops = GradientStopCollection.Empty;
		Type typeFromHandle = typeof(GradientBrush);
		ColorInterpolationModeProperty = Animatable.RegisterProperty("ColorInterpolationMode", typeof(ColorInterpolationMode), typeFromHandle, ColorInterpolationMode.SRgbLinearInterpolation, ColorInterpolationModePropertyChanged, ValidateEnums.IsColorInterpolationModeValid, isIndependentlyAnimated: false, null);
		MappingModeProperty = Animatable.RegisterProperty("MappingMode", typeof(BrushMappingMode), typeFromHandle, BrushMappingMode.RelativeToBoundingBox, MappingModePropertyChanged, ValidateEnums.IsBrushMappingModeValid, isIndependentlyAnimated: false, null);
		SpreadMethodProperty = Animatable.RegisterProperty("SpreadMethod", typeof(GradientSpreadMethod), typeFromHandle, GradientSpreadMethod.Pad, SpreadMethodPropertyChanged, ValidateEnums.IsGradientSpreadMethodValid, isIndependentlyAnimated: false, null);
		GradientStopsProperty = Animatable.RegisterProperty("GradientStops", typeof(GradientStopCollection), typeFromHandle, new FreezableDefaultValueFactory(GradientStopCollection.Empty), GradientStopsPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
