using System.Runtime.InteropServices;
using System.Windows.Media.Animation;

namespace System.Windows.Media.Effects;

/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Creates a bevel which raises the surface of the image according to a specified curve. </summary>
public sealed class BevelBitmapEffect : BitmapEffect
{
	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Identifies the <see cref="P:System.Windows.Media.Effects.BevelBitmapEffect.BevelWidth" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.BevelBitmapEffect.BevelWidth" /> dependency property.</returns>
	public static readonly DependencyProperty BevelWidthProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Identifies the <see cref="P:System.Windows.Media.Effects.BevelBitmapEffect.Relief" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.BevelBitmapEffect.Relief" /> dependency property.</returns>
	public static readonly DependencyProperty ReliefProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Identifies the <see cref="P:System.Windows.Media.Effects.BevelBitmapEffect.LightAngle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.BevelBitmapEffect.LightAngle" /> dependency property.</returns>
	public static readonly DependencyProperty LightAngleProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Identifies the <see cref="P:System.Windows.Media.Effects.BevelBitmapEffect.Smoothness" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.BevelBitmapEffect.Smoothness" /> dependency property.</returns>
	public static readonly DependencyProperty SmoothnessProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Identifies the <see cref="P:System.Windows.Media.Effects.BevelBitmapEffect.EdgeProfile" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.BevelBitmapEffect.EdgeProfile" /> dependency property.</returns>
	public static readonly DependencyProperty EdgeProfileProperty;

	internal const double c_BevelWidth = 5.0;

	internal const double c_Relief = 0.3;

	internal const double c_LightAngle = 135.0;

	internal const double c_Smoothness = 0.2;

	internal const EdgeProfile c_EdgeProfile = EdgeProfile.Linear;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Gets or sets the width of the bevel. </summary>
	/// <returns>The width of the bevel. The default value is 5.</returns>
	public double BevelWidth
	{
		get
		{
			return (double)GetValue(BevelWidthProperty);
		}
		set
		{
			SetValueInternal(BevelWidthProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Gets or sets the height of the relief of the bevel.  </summary>
	/// <returns>The height of the relief of the bevel. The valid range is between 0 and 1 with 1 having the most relief (darkest shadows). The default value is 0.3.</returns>
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

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Gets or sets the direction the "virtual light" is coming from that creates the shadows of the bevel. </summary>
	/// <returns>The direction of the virtual light source. The valid range is from 0-360 (degrees) with 0 specifying the right-hand side of the object and successive values moving counter-clockwise around the object. The shadows of the bevel are on the opposite side of where the light is cast. The default value is 135.</returns>
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

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Gets or sets how smooth the shadows of the bevel are.  </summary>
	/// <returns>Value indicating how smooth the bevel shadows are. The valid range is between 0 and 1 with 1 being the smoothest. The default value is 0.2.</returns>
	public double Smoothness
	{
		get
		{
			return (double)GetValue(SmoothnessProperty);
		}
		set
		{
			SetValueInternal(SmoothnessProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Gets or sets the curve of the bevel. </summary>
	/// <returns>The curve of the bevel. The default value is <see cref="F:System.Windows.Media.Effects.EdgeProfile.Linear" />.</returns>
	public EdgeProfile EdgeProfile
	{
		get
		{
			return (EdgeProfile)GetValue(EdgeProfileProperty);
		}
		set
		{
			SetValueInternal(EdgeProfileProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.BevelBitmapEffect" /> class.</summary>
	public BevelBitmapEffect()
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

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.BevelBitmapEffect" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new BevelBitmapEffect Clone()
	{
		return (BevelBitmapEffect)base.Clone();
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.BevelBitmapEffect" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new BevelBitmapEffect CloneCurrentValue()
	{
		return (BevelBitmapEffect)base.CloneCurrentValue();
	}

	private static void BevelWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((BevelBitmapEffect)d).PropertyChanged(BevelWidthProperty);
	}

	private static void ReliefPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((BevelBitmapEffect)d).PropertyChanged(ReliefProperty);
	}

	private static void LightAnglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((BevelBitmapEffect)d).PropertyChanged(LightAngleProperty);
	}

	private static void SmoothnessPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((BevelBitmapEffect)d).PropertyChanged(SmoothnessProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new BevelBitmapEffect();
	}

	static BevelBitmapEffect()
	{
		Type typeFromHandle = typeof(BevelBitmapEffect);
		BevelWidthProperty = Animatable.RegisterProperty("BevelWidth", typeof(double), typeFromHandle, 5.0, BevelWidthPropertyChanged, null, isIndependentlyAnimated: true, null);
		ReliefProperty = Animatable.RegisterProperty("Relief", typeof(double), typeFromHandle, 0.3, ReliefPropertyChanged, null, isIndependentlyAnimated: true, null);
		LightAngleProperty = Animatable.RegisterProperty("LightAngle", typeof(double), typeFromHandle, 135.0, LightAnglePropertyChanged, null, isIndependentlyAnimated: true, null);
		SmoothnessProperty = Animatable.RegisterProperty("Smoothness", typeof(double), typeFromHandle, 0.2, SmoothnessPropertyChanged, null, isIndependentlyAnimated: true, null);
		EdgeProfileProperty = Animatable.RegisterProperty("EdgeProfile", typeof(EdgeProfile), typeFromHandle, EdgeProfile.Linear, null, ValidateEnums.IsEdgeProfileValid, isIndependentlyAnimated: false, null);
	}
}
