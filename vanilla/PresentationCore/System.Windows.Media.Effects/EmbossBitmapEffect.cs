using System.Runtime.InteropServices;
using System.Windows.Media.Animation;

namespace System.Windows.Media.Effects;

/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Creates a bump mapping of the visual object to give the impression of depth and texture from an artificial light source. </summary>
public sealed class EmbossBitmapEffect : BitmapEffect
{
	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Identifies the <see cref="P:System.Windows.Media.Effects.EmbossBitmapEffect.LightAngle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.EmbossBitmapEffect.LightAngle" /> dependency property.</returns>
	public static readonly DependencyProperty LightAngleProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Identifies the <see cref="P:System.Windows.Media.Effects.EmbossBitmapEffect.Relief" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.EmbossBitmapEffect.Relief" /> dependency property.</returns>
	public static readonly DependencyProperty ReliefProperty;

	internal const double c_LightAngle = 45.0;

	internal const double c_Relief = 0.44;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Gets or sets the direction the artificial light is cast upon the embossed object.  </summary>
	/// <returns>The direction the artificial light is cast upon the embossed object. The valid range is from 0-360 (degrees) with 0 specifying the right-hand side of the object and successive values moving counter-clockwise around the object. The default value is 45.</returns>
	public double LightAngle
	{
		get
		{
			return (double)GetValue(LightAngleProperty);
		}
		set
		{
			SetValueInternal(LightAngleProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Gets or sets the amount of relief of the emboss.  </summary>
	/// <returns>The amount of relief of the emboss. The valid range of values is 0-1 with 0 having the least relief and 1 having the most. The default value is 0.44.</returns>
	public double Relief
	{
		get
		{
			return (double)GetValue(ReliefProperty);
		}
		set
		{
			SetValueInternal(ReliefProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.EmbossBitmapEffect" /> class.</summary>
	public EmbossBitmapEffect()
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

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.EmbossBitmapEffect" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new EmbossBitmapEffect Clone()
	{
		return (EmbossBitmapEffect)base.Clone();
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.EmbossBitmapEffect" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new EmbossBitmapEffect CloneCurrentValue()
	{
		return (EmbossBitmapEffect)base.CloneCurrentValue();
	}

	private static void LightAnglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((EmbossBitmapEffect)d).PropertyChanged(LightAngleProperty);
	}

	private static void ReliefPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((EmbossBitmapEffect)d).PropertyChanged(ReliefProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new EmbossBitmapEffect();
	}

	static EmbossBitmapEffect()
	{
		Type typeFromHandle = typeof(EmbossBitmapEffect);
		LightAngleProperty = Animatable.RegisterProperty("LightAngle", typeof(double), typeFromHandle, 45.0, LightAnglePropertyChanged, null, isIndependentlyAnimated: true, null);
		ReliefProperty = Animatable.RegisterProperty("Relief", typeof(double), typeFromHandle, 0.44, ReliefPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
