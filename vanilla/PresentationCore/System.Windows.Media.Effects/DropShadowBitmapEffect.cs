using System.Runtime.InteropServices;
using System.Windows.Media.Animation;

namespace System.Windows.Media.Effects;

/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Applies a shadow behind a visual object at a slight offset. The offset is determined by mimicking a casting shadow from an imaginary light source.</summary>
public sealed class DropShadowBitmapEffect : BitmapEffect
{
	private DropShadowEffect _imageEffectEmulation;

	private const double _MAX_EMULATED_BLUR_RADIUS = 25.0;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Identifies the <see cref="P:System.Windows.Media.Effects.DropShadowBitmapEffect.ShadowDepth" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.DropShadowBitmapEffect.ShadowDepth" /> dependency property.</returns>
	public static readonly DependencyProperty ShadowDepthProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Identifies the <see cref="P:System.Windows.Media.Effects.DropShadowBitmapEffect.Color" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.DropShadowBitmapEffect.Color" /> dependency property.</returns>
	public static readonly DependencyProperty ColorProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Identifies the <see cref="P:System.Windows.Media.Effects.DropShadowBitmapEffect.Direction" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.DropShadowBitmapEffect.Direction" /> dependency property.</returns>
	public static readonly DependencyProperty DirectionProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Identifies the <see cref="P:System.Windows.Media.Effects.DropShadowBitmapEffect.Noise" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.DropShadowBitmapEffect.Noise" /> dependency property.</returns>
	public static readonly DependencyProperty NoiseProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Identifies the <see cref="P:System.Windows.Media.Effects.DropShadowBitmapEffect.Opacity" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.DropShadowBitmapEffect.Opacity" /> dependency property.</returns>
	public static readonly DependencyProperty OpacityProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Identifies the <see cref="P:System.Windows.Media.Effects.DropShadowBitmapEffect.Softness" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.DropShadowBitmapEffect.Softness" /> dependency property.</returns>
	public static readonly DependencyProperty SoftnessProperty;

	internal const double c_ShadowDepth = 5.0;

	internal static Color s_Color;

	internal const double c_Direction = 315.0;

	internal const double c_Noise = 0.0;

	internal const double c_Opacity = 1.0;

	internal const double c_Softness = 0.5;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Gets or sets the distance between the object and the shadow that it casts. </summary>
	/// <returns>The distance between the plane of the object casting the shadow and the shadow plane measured in device-independent units (1/96th inch per unit). The valid range of values is from 0 through 300. The default is 5.</returns>
	public double ShadowDepth
	{
		get
		{
			return (double)GetValue(ShadowDepthProperty);
		}
		set
		{
			SetValueInternal(ShadowDepthProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Gets or sets the color of the shadow.  </summary>
	/// <returns>The color of the shadow. The default value is FF000000 (black). </returns>
	public Color Color
	{
		get
		{
			return (Color)GetValue(ColorProperty);
		}
		set
		{
			SetValueInternal(ColorProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Gets or sets the angle at which the shadow is cast. </summary>
	/// <returns>The angle at which the shadow is cast. The valid range of values is from 0 through 360. The value 0 puts the direction immediately to the right of the object. Subsequent values move the direction around the object in a counter-clockwise direction. For example, a value of 90 indicates the shadow is cast directly upward from the object; a value of 180 is cast directly to the left of the object, and so on. The default value is 315.</returns>
	public double Direction
	{
		get
		{
			return (double)GetValue(DirectionProperty);
		}
		set
		{
			SetValueInternal(DirectionProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Gets or sets the graininess, or "noise level," of the shadow. </summary>
	/// <returns>The noise level of the shadow. The valid range of values is from 0 through 1. A value of 0 indicates no noise and 1 indicates maximum noise. A value of 0.5 indicates 50 percent noise, a value of 0.75 indicates 75 percent noise, and so on. The default value is 0.</returns>
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

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Gets or sets the degree of opacity of the shadow. </summary>
	/// <returns>The degree of opacity. The valid range of values is from 0 through 1. A value of 0 indicates that the shadow is completely transparent, and a value of 1 indicates that the shadow is completely opaque. A value of 0.5 indicates the shadow is 50 percent opaque, a value of 0.725 indicates the shadow is 72.5 percent opaque, and so on. Values less than 0 are treated as 0, while values greater than 1 are treated as 1. The default is 1.</returns>
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

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Gets or sets the softness of the shadow. </summary>
	/// <returns>The shadow's softness. The valid range of values is from 0 through 1. A value of 0.0 indicates no softness (a sharply defined shadow) and 1.0 indicates maximum softness (a very diffused shadow). A value of 0.5 indicates 50 percent softness, a value of 0.75 indicates 75 percent softness, and so on. The default is 0.5. </returns>
	public double Softness
	{
		get
		{
			return (double)GetValue(SoftnessProperty);
		}
		set
		{
			SetValueInternal(SoftnessProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.DropShadowBitmapEffect" /> class.</summary>
	public DropShadowBitmapEffect()
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

	internal override bool CanBeEmulatedUsingEffectPipeline()
	{
		return Noise == 0.0;
	}

	internal override Effect GetEmulatingEffect()
	{
		if (_imageEffectEmulation != null && _imageEffectEmulation.IsFrozen)
		{
			return _imageEffectEmulation;
		}
		if (_imageEffectEmulation == null)
		{
			_imageEffectEmulation = new DropShadowEffect();
		}
		Color color = Color;
		if (_imageEffectEmulation.Color != color)
		{
			_imageEffectEmulation.Color = color;
		}
		double shadowDepth = ShadowDepth;
		if (_imageEffectEmulation.ShadowDepth != shadowDepth)
		{
			if (shadowDepth >= 50.0)
			{
				_imageEffectEmulation.ShadowDepth = 50.0;
			}
			else if (shadowDepth < 0.0)
			{
				_imageEffectEmulation.ShadowDepth = 0.0;
			}
			else
			{
				_imageEffectEmulation.ShadowDepth = shadowDepth;
			}
		}
		double direction = Direction;
		if (_imageEffectEmulation.Direction != direction)
		{
			_imageEffectEmulation.Direction = direction;
		}
		double opacity = Opacity;
		if (_imageEffectEmulation.Opacity != opacity)
		{
			if (opacity >= 1.0)
			{
				_imageEffectEmulation.Opacity = 1.0;
			}
			else if (opacity <= 0.0)
			{
				_imageEffectEmulation.Opacity = 0.0;
			}
			else
			{
				_imageEffectEmulation.Opacity = opacity;
			}
		}
		double softness = Softness;
		if (_imageEffectEmulation.BlurRadius / 25.0 != softness)
		{
			if (softness >= 1.0)
			{
				_imageEffectEmulation.BlurRadius = 25.0;
			}
			else if (softness <= 0.0)
			{
				_imageEffectEmulation.BlurRadius = 0.0;
			}
			else
			{
				_imageEffectEmulation.BlurRadius = 25.0 * softness;
			}
		}
		_imageEffectEmulation.RenderingBias = RenderingBias.Performance;
		if (base.IsFrozen)
		{
			_imageEffectEmulation.Freeze();
		}
		return _imageEffectEmulation;
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.DropShadowBitmapEffect" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DropShadowBitmapEffect Clone()
	{
		return (DropShadowBitmapEffect)base.Clone();
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.DropShadowEffect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.DropShadowBitmapEffect" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DropShadowBitmapEffect CloneCurrentValue()
	{
		return (DropShadowBitmapEffect)base.CloneCurrentValue();
	}

	private static void ShadowDepthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DropShadowBitmapEffect)d).PropertyChanged(ShadowDepthProperty);
	}

	private static void ColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DropShadowBitmapEffect)d).PropertyChanged(ColorProperty);
	}

	private static void DirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DropShadowBitmapEffect)d).PropertyChanged(DirectionProperty);
	}

	private static void NoisePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DropShadowBitmapEffect)d).PropertyChanged(NoiseProperty);
	}

	private static void OpacityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DropShadowBitmapEffect)d).PropertyChanged(OpacityProperty);
	}

	private static void SoftnessPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DropShadowBitmapEffect)d).PropertyChanged(SoftnessProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new DropShadowBitmapEffect();
	}

	static DropShadowBitmapEffect()
	{
		s_Color = Colors.Black;
		Type typeFromHandle = typeof(DropShadowBitmapEffect);
		ShadowDepthProperty = Animatable.RegisterProperty("ShadowDepth", typeof(double), typeFromHandle, 5.0, ShadowDepthPropertyChanged, null, isIndependentlyAnimated: true, null);
		ColorProperty = Animatable.RegisterProperty("Color", typeof(Color), typeFromHandle, Colors.Black, ColorPropertyChanged, null, isIndependentlyAnimated: true, null);
		DirectionProperty = Animatable.RegisterProperty("Direction", typeof(double), typeFromHandle, 315.0, DirectionPropertyChanged, null, isIndependentlyAnimated: true, null);
		NoiseProperty = Animatable.RegisterProperty("Noise", typeof(double), typeFromHandle, 0.0, NoisePropertyChanged, null, isIndependentlyAnimated: true, null);
		OpacityProperty = Animatable.RegisterProperty("Opacity", typeof(double), typeFromHandle, 1.0, OpacityPropertyChanged, null, isIndependentlyAnimated: true, null);
		SoftnessProperty = Animatable.RegisterProperty("Softness", typeof(double), typeFromHandle, 0.5, SoftnessPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
