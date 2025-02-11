using System.Runtime.InteropServices;
using System.Windows.Media.Animation;

namespace System.Windows.Media.Effects;

/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Simulates looking at an object through an out-of-focus lens.</summary>
public sealed class BlurBitmapEffect : BitmapEffect
{
	private BlurEffect _imageEffectEmulation;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Identifies the <see cref="P:System.Windows.Media.Effects.BlurBitmapEffect.Radius" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.BlurBitmapEffect.KernelType" /> dependency property.</returns>
	public static readonly DependencyProperty RadiusProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Identifies the <see cref="P:System.Windows.Media.Effects.BlurBitmapEffect.KernelType" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.BlurBitmapEffect.KernelType" /> dependency property.</returns>
	public static readonly DependencyProperty KernelTypeProperty;

	internal const double c_Radius = 5.0;

	internal const KernelType c_KernelType = KernelType.Gaussian;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Gets or sets the radius used in the blur kernel. A larger radius implies more blurring.  </summary>
	/// <returns>The radius used in the blur kernel, in DIU (1/96 of an inch). The default value is 5.</returns>
	public double Radius
	{
		get
		{
			return (double)GetValue(RadiusProperty);
		}
		set
		{
			SetValueInternal(RadiusProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Gets or sets the type of blur kernel to use for the <see cref="T:System.Windows.Media.Effects.BlurBitmapEffect" />.  </summary>
	/// <returns>The type of blur kernel. The default value is <see cref="F:System.Windows.Media.Effects.KernelType.Gaussian" />.</returns>
	public KernelType KernelType
	{
		get
		{
			return (KernelType)GetValue(KernelTypeProperty);
		}
		set
		{
			SetValueInternal(KernelTypeProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.BlurBitmapEffect" /> class.</summary>
	public BlurBitmapEffect()
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
		return Radius <= 100.0;
	}

	internal override Effect GetEmulatingEffect()
	{
		if (_imageEffectEmulation != null && _imageEffectEmulation.IsFrozen)
		{
			return _imageEffectEmulation;
		}
		if (_imageEffectEmulation == null)
		{
			_imageEffectEmulation = new BlurEffect();
		}
		double radius = Radius;
		if (_imageEffectEmulation.Radius != radius)
		{
			_imageEffectEmulation.Radius = radius;
		}
		KernelType kernelType = KernelType;
		if (_imageEffectEmulation.KernelType != kernelType)
		{
			_imageEffectEmulation.KernelType = kernelType;
		}
		_imageEffectEmulation.RenderingBias = RenderingBias.Performance;
		if (base.IsFrozen)
		{
			_imageEffectEmulation.Freeze();
		}
		return _imageEffectEmulation;
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.BlurBitmapEffect" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new BlurBitmapEffect Clone()
	{
		return (BlurBitmapEffect)base.Clone();
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.BlurEffect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.BlurBitmapEffect" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new BlurBitmapEffect CloneCurrentValue()
	{
		return (BlurBitmapEffect)base.CloneCurrentValue();
	}

	private static void RadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((BlurBitmapEffect)d).PropertyChanged(RadiusProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new BlurBitmapEffect();
	}

	static BlurBitmapEffect()
	{
		Type typeFromHandle = typeof(BlurBitmapEffect);
		RadiusProperty = Animatable.RegisterProperty("Radius", typeof(double), typeFromHandle, 5.0, RadiusPropertyChanged, null, isIndependentlyAnimated: true, null);
		KernelTypeProperty = Animatable.RegisterProperty("KernelType", typeof(KernelType), typeFromHandle, KernelType.Gaussian, null, ValidateEnums.IsKernelTypeValid, isIndependentlyAnimated: false, null);
	}
}
