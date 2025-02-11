using System.Runtime.InteropServices;
using System.Windows.Media.Animation;

namespace System.Windows.Media.Effects;

/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Creates a halo of color around objects or areas of color.</summary>
public sealed class OuterGlowBitmapEffect : BitmapEffect
{
	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Identifies the <see cref="P:System.Windows.Media.Effects.OuterGlowBitmapEffect.GlowColor" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.OuterGlowBitmapEffect.GlowColor" /> dependency property.</returns>
	public static readonly DependencyProperty GlowColorProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Identifies the <see cref="P:System.Windows.Media.Effects.OuterGlowBitmapEffect.GlowSize" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.OuterGlowBitmapEffect.GlowSize" /> dependency property.</returns>
	public static readonly DependencyProperty GlowSizeProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Identifies the <see cref="P:System.Windows.Media.Effects.OuterGlowBitmapEffect.Noise" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.OuterGlowBitmapEffect.Noise" /> dependency property.</returns>
	public static readonly DependencyProperty NoiseProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Identifies the <see cref="P:System.Windows.Media.Effects.OuterGlowBitmapEffect.Opacity" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.OuterGlowBitmapEffect.Opacity" /> dependency property.</returns>
	public static readonly DependencyProperty OpacityProperty;

	internal static Color s_GlowColor;

	internal const double c_GlowSize = 5.0;

	internal const double c_Noise = 0.0;

	internal const double c_Opacity = 1.0;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Gets or sets the color of the halo glow.  </summary>
	/// <returns>The color of the halo glow. The default is white. </returns>
	public Color GlowColor
	{
		get
		{
			return (Color)GetValue(GlowColorProperty);
		}
		set
		{
			SetValueInternal(GlowColorProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Gets or sets the thickness of the halo glow.  </summary>
	/// <returns>The thickness of the halo glow, in device-independent unit (1/96th inch). The valid range of values is from 1 through 199. The default is 20.</returns>
	public double GlowSize
	{
		get
		{
			return (double)GetValue(GlowSizeProperty);
		}
		set
		{
			SetValueInternal(GlowSizeProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Gets or sets the graininess of the halo glow.  </summary>
	/// <returns>The graininess (noise level) of the halo glow. The valid range of values is from 0.0 through 1.0, with 0.0 specifying no noise and 1.0 specifying maximum noise. A value of 0.5 indicates 50 percent noise, a value of 0.75 indicates 75 percent noise, and so on. The default value is 0.0.</returns>
	public double Noise
	{
		get
		{
			return (double)GetValue(NoiseProperty);
		}
		set
		{
			SetValueInternal(NoiseProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Gets or sets the degree of opacity of the halo glow.  </summary>
	/// <returns>The opacity level of the glow. A value of 0 indicates that the halo glow is completely transparent, while a value of 1 indicates that the glow is completely opaque. A value of 0.5 indicates the glow is 50 percent opaque, a value of 0.725 indicates the glow is 72.5 percent opaque, and so on. Values less than 0 are treated as 0, while values greater than 1 are treated as 1. The default is 1.</returns>
	public double Opacity
	{
		get
		{
			return (double)GetValue(OpacityProperty);
		}
		set
		{
			SetValueInternal(OpacityProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.OuterGlowBitmapEffect" /> class.</summary>
	public OuterGlowBitmapEffect()
	{
	}

	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	protected override SafeHandle CreateUnmanagedEffect()
	{
		return null;
	}

	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	protected override void UpdateUnmanagedPropertyState(SafeHandle unmanagedEffect)
	{
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.OuterGlowBitmapEffect" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new OuterGlowBitmapEffect Clone()
	{
		return (OuterGlowBitmapEffect)base.Clone();
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.OuterGlowBitmapEffect" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new OuterGlowBitmapEffect CloneCurrentValue()
	{
		return (OuterGlowBitmapEffect)base.CloneCurrentValue();
	}

	private static void GlowColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((OuterGlowBitmapEffect)d).PropertyChanged(GlowColorProperty);
	}

	private static void GlowSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((OuterGlowBitmapEffect)d).PropertyChanged(GlowSizeProperty);
	}

	private static void NoisePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((OuterGlowBitmapEffect)d).PropertyChanged(NoiseProperty);
	}

	private static void OpacityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((OuterGlowBitmapEffect)d).PropertyChanged(OpacityProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new OuterGlowBitmapEffect();
	}

	static OuterGlowBitmapEffect()
	{
		s_GlowColor = Colors.Gold;
		Type typeFromHandle = typeof(OuterGlowBitmapEffect);
		GlowColorProperty = Animatable.RegisterProperty("GlowColor", typeof(Color), typeFromHandle, Colors.Gold, GlowColorPropertyChanged, null, isIndependentlyAnimated: true, null);
		GlowSizeProperty = Animatable.RegisterProperty("GlowSize", typeof(double), typeFromHandle, 5.0, GlowSizePropertyChanged, null, isIndependentlyAnimated: true, null);
		NoiseProperty = Animatable.RegisterProperty("Noise", typeof(double), typeFromHandle, 0.0, NoisePropertyChanged, null, isIndependentlyAnimated: true, null);
		OpacityProperty = Animatable.RegisterProperty("Opacity", typeof(double), typeFromHandle, 1.0, OpacityPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
