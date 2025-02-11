using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media.Effects;

/// <summary>A bitmap effect that blurs the target texture. </summary>
public sealed class BlurEffect : Effect
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Effects.BlurEffect.Radius" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.BlurEffect.Radius" /> dependency property.</returns>
	public static readonly DependencyProperty RadiusProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Effects.BlurEffect.KernelType" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.BlurEffect.KernelType" /> dependency property.</returns>
	public static readonly DependencyProperty KernelTypeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Effects.BlurEffect.RenderingBias" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.BlurEffect.RenderingBias" /> dependency property.</returns>
	public static readonly DependencyProperty RenderingBiasProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_Radius = 5.0;

	internal const KernelType c_KernelType = KernelType.Gaussian;

	internal const RenderingBias c_RenderingBias = RenderingBias.Performance;

	/// <summary>Gets or sets a value that indicates the radius of the blur effect's curve. </summary>
	/// <returns>The radius of the blur effect's curve. The default is 5.</returns>
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

	/// <summary>Gets or sets a value representing the curve that is used to calculate the blur.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Effects.KernelType" /> representing the curve that is used to calculate the blur. The default is <see cref="F:System.Windows.Media.Effects.KernelType.Gaussian" />.</returns>
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

	/// <summary>Gets or sets a value that indicates whether the system renders an effect with emphasis on speed or quality. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Effects.RenderingBias" /> value that indicates whether the system renders an effect with emphasis on speed or quality. The default is <see cref="F:System.Windows.Media.Effects.RenderingBias.Performance" />.</returns>
	public RenderingBias RenderingBias
	{
		get
		{
			return (RenderingBias)GetValue(RenderingBiasProperty);
		}
		set
		{
			SetValueInternal(RenderingBiasProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.BlurEffect" /> class. </summary>
	public BlurEffect()
	{
	}

	internal override Rect GetRenderBounds(Rect contentBounds)
	{
		Point point = default(Point);
		Point point2 = default(Point);
		double radius = Radius;
		point.X = contentBounds.TopLeft.X - radius;
		point.Y = contentBounds.TopLeft.Y - radius;
		point2.X = contentBounds.BottomRight.X + radius;
		point2.Y = contentBounds.BottomRight.Y + radius;
		return new Rect(point, point2);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.Effect" /> object, making deep copies of this object's values. When copying this object's dependency properties, this method copies resource references and data bindings (which may no longer resolve), but not animations or their current values.  </summary>
	/// <returns>A modifiable clone of this instance. The returned clone is effectively a deep copy of the current object. The clone's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false.</returns>
	public new BlurEffect Clone()
	{
		return (BlurEffect)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.Effect" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are copied.  </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new BlurEffect CloneCurrentValue()
	{
		return (BlurEffect)base.CloneCurrentValue();
	}

	private static void RadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((BlurEffect)d).PropertyChanged(RadiusProperty);
	}

	private static void KernelTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((BlurEffect)d).PropertyChanged(KernelTypeProperty);
	}

	private static void RenderingBiasPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((BlurEffect)d).PropertyChanged(RenderingBiasProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new BlurEffect();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(RadiusProperty, channel);
			DUCE.MILCMD_BLUREFFECT mILCMD_BLUREFFECT = default(DUCE.MILCMD_BLUREFFECT);
			mILCMD_BLUREFFECT.Type = MILCMD.MilCmdBlurEffect;
			mILCMD_BLUREFFECT.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_BLUREFFECT.Radius = Radius;
			}
			mILCMD_BLUREFFECT.hRadiusAnimations = animationResourceHandle;
			mILCMD_BLUREFFECT.KernelType = KernelType;
			mILCMD_BLUREFFECT.RenderingBias = RenderingBias;
			channel.SendCommand((byte*)(&mILCMD_BLUREFFECT), sizeof(DUCE.MILCMD_BLUREFFECT));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_BLUREFFECT))
		{
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			ReleaseOnChannelAnimations(channel);
		}
	}

	internal override DUCE.ResourceHandle GetHandleCore(DUCE.Channel channel)
	{
		return _duceResource.GetHandle(channel);
	}

	internal override int GetChannelCountCore()
	{
		return _duceResource.GetChannelCount();
	}

	internal override DUCE.Channel GetChannelCore(int index)
	{
		return _duceResource.GetChannel(index);
	}

	static BlurEffect()
	{
		Type typeFromHandle = typeof(BlurEffect);
		RadiusProperty = Animatable.RegisterProperty("Radius", typeof(double), typeFromHandle, 5.0, RadiusPropertyChanged, null, isIndependentlyAnimated: true, null);
		KernelTypeProperty = Animatable.RegisterProperty("KernelType", typeof(KernelType), typeFromHandle, KernelType.Gaussian, KernelTypePropertyChanged, ValidateEnums.IsKernelTypeValid, isIndependentlyAnimated: false, null);
		RenderingBiasProperty = Animatable.RegisterProperty("RenderingBias", typeof(RenderingBias), typeFromHandle, RenderingBias.Performance, RenderingBiasPropertyChanged, ValidateEnums.IsRenderingBiasValid, isIndependentlyAnimated: false, null);
	}
}
