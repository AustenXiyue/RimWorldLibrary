using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media.Effects;

/// <summary>A bitmap effect that paints a drop shadow around the target texture. </summary>
public sealed class DropShadowEffect : Effect
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Effects.DropShadowEffect.ShadowDepth" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.DropShadowEffect.ShadowDepth" /> dependency property.</returns>
	public static readonly DependencyProperty ShadowDepthProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Effects.DropShadowEffect.Color" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.DropShadowEffect.Color" /> dependency property.</returns>
	public static readonly DependencyProperty ColorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Effects.DropShadowEffect.Direction" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.DropShadowEffect.Direction" /> dependency property.</returns>
	public static readonly DependencyProperty DirectionProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Effects.DropShadowEffect.Opacity" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.DropShadowEffect.Opacity" /> dependency property.</returns>
	public static readonly DependencyProperty OpacityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Effects.DropShadowEffect.BlurRadius" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.DropShadowEffect.BlurRadius" /> dependency property.</returns>
	public static readonly DependencyProperty BlurRadiusProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Effects.DropShadowEffect.RenderingBias" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.DropShadowEffect.RenderingBias" /> dependency property.</returns>
	public static readonly DependencyProperty RenderingBiasProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_ShadowDepth = 5.0;

	internal static Color s_Color;

	internal const double c_Direction = 315.0;

	internal const double c_Opacity = 1.0;

	internal const double c_BlurRadius = 5.0;

	internal const RenderingBias c_RenderingBias = RenderingBias.Performance;

	/// <summary>Gets or sets the distance of the drop shadow below the texture. </summary>
	/// <returns>The distance of the drop shadow below the texture. The default is 5.</returns>
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

	/// <summary>Gets or sets the color of the drop shadow. </summary>
	/// <returns>The color of the drop shadow. The default is <see cref="P:System.Windows.Media.Colors.Black" />.</returns>
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

	/// <summary>Gets or sets the direction of the drop shadow. </summary>
	/// <returns>The direction of the drop shadow, in degrees. The default is 315.</returns>
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

	/// <summary>Gets or sets the opacity of the drop shadow. </summary>
	/// <returns>The opacity of the drop shadow. The default is 1.</returns>
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

	/// <summary>Gets or sets a value that indicates the radius of the shadow's blur effect. </summary>
	/// <returns>A value that indicates the radius of the shadow's blur effect. The default is 5.</returns>
	public double BlurRadius
	{
		get
		{
			return (double)GetValue(BlurRadiusProperty);
		}
		set
		{
			SetValueInternal(BlurRadiusProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the system renders the drop shadow with emphasis on speed or quality. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Effects.RenderingBias" /> value that indicates whether the system renders the drop shadow with emphasis on speed or quality. The default is <see cref="F:System.Windows.Media.Effects.RenderingBias.Performance" />.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.DropShadowEffect" /> class. </summary>
	public DropShadowEffect()
	{
	}

	internal override Rect GetRenderBounds(Rect contentBounds)
	{
		Point point = default(Point);
		Point point2 = default(Point);
		double blurRadius = BlurRadius;
		point.X = contentBounds.TopLeft.X - blurRadius;
		point.Y = contentBounds.TopLeft.Y - blurRadius;
		point2.X = contentBounds.BottomRight.X + blurRadius;
		point2.Y = contentBounds.BottomRight.Y + blurRadius;
		double shadowDepth = ShadowDepth;
		double num = Math.PI / 180.0 * Direction;
		double num2 = shadowDepth * Math.Cos(num);
		double num3 = shadowDepth * Math.Sin(num);
		if (num2 >= 0.0)
		{
			point2.X += num2;
		}
		else
		{
			point.X += num2;
		}
		if (num3 >= 0.0)
		{
			point.Y -= num3;
		}
		else
		{
			point2.Y -= num3;
		}
		return new Rect(point, point2);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.Effect" /> object, making deep copies of this object's values. When copying this object's dependency properties, this method copies resource references and data bindings (which may no longer resolve), but not animations or their current values.</summary>
	/// <returns>A modifiable clone of this instance. The returned clone is effectively a deep copy of the current object. The clone's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false.</returns>
	public new DropShadowEffect Clone()
	{
		return (DropShadowEffect)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.Effect" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are copied.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DropShadowEffect CloneCurrentValue()
	{
		return (DropShadowEffect)base.CloneCurrentValue();
	}

	private static void ShadowDepthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DropShadowEffect)d).PropertyChanged(ShadowDepthProperty);
	}

	private static void ColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DropShadowEffect)d).PropertyChanged(ColorProperty);
	}

	private static void DirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DropShadowEffect)d).PropertyChanged(DirectionProperty);
	}

	private static void OpacityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DropShadowEffect)d).PropertyChanged(OpacityProperty);
	}

	private static void BlurRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DropShadowEffect)d).PropertyChanged(BlurRadiusProperty);
	}

	private static void RenderingBiasPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DropShadowEffect)d).PropertyChanged(RenderingBiasProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new DropShadowEffect();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(ShadowDepthProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(ColorProperty, channel);
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(DirectionProperty, channel);
			DUCE.ResourceHandle animationResourceHandle4 = GetAnimationResourceHandle(OpacityProperty, channel);
			DUCE.ResourceHandle animationResourceHandle5 = GetAnimationResourceHandle(BlurRadiusProperty, channel);
			DUCE.MILCMD_DROPSHADOWEFFECT mILCMD_DROPSHADOWEFFECT = default(DUCE.MILCMD_DROPSHADOWEFFECT);
			mILCMD_DROPSHADOWEFFECT.Type = MILCMD.MilCmdDropShadowEffect;
			mILCMD_DROPSHADOWEFFECT.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_DROPSHADOWEFFECT.ShadowDepth = ShadowDepth;
			}
			mILCMD_DROPSHADOWEFFECT.hShadowDepthAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_DROPSHADOWEFFECT.Color = CompositionResourceManager.ColorToMilColorF(Color);
			}
			mILCMD_DROPSHADOWEFFECT.hColorAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_DROPSHADOWEFFECT.Direction = Direction;
			}
			mILCMD_DROPSHADOWEFFECT.hDirectionAnimations = animationResourceHandle3;
			if (animationResourceHandle4.IsNull)
			{
				mILCMD_DROPSHADOWEFFECT.Opacity = Opacity;
			}
			mILCMD_DROPSHADOWEFFECT.hOpacityAnimations = animationResourceHandle4;
			if (animationResourceHandle5.IsNull)
			{
				mILCMD_DROPSHADOWEFFECT.BlurRadius = BlurRadius;
			}
			mILCMD_DROPSHADOWEFFECT.hBlurRadiusAnimations = animationResourceHandle5;
			mILCMD_DROPSHADOWEFFECT.RenderingBias = RenderingBias;
			channel.SendCommand((byte*)(&mILCMD_DROPSHADOWEFFECT), sizeof(DUCE.MILCMD_DROPSHADOWEFFECT));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_DROPSHADOWEFFECT))
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

	static DropShadowEffect()
	{
		s_Color = Colors.Black;
		Type typeFromHandle = typeof(DropShadowEffect);
		ShadowDepthProperty = Animatable.RegisterProperty("ShadowDepth", typeof(double), typeFromHandle, 5.0, ShadowDepthPropertyChanged, null, isIndependentlyAnimated: true, null);
		ColorProperty = Animatable.RegisterProperty("Color", typeof(Color), typeFromHandle, Colors.Black, ColorPropertyChanged, null, isIndependentlyAnimated: true, null);
		DirectionProperty = Animatable.RegisterProperty("Direction", typeof(double), typeFromHandle, 315.0, DirectionPropertyChanged, null, isIndependentlyAnimated: true, null);
		OpacityProperty = Animatable.RegisterProperty("Opacity", typeof(double), typeFromHandle, 1.0, OpacityPropertyChanged, null, isIndependentlyAnimated: true, null);
		BlurRadiusProperty = Animatable.RegisterProperty("BlurRadius", typeof(double), typeFromHandle, 5.0, BlurRadiusPropertyChanged, null, isIndependentlyAnimated: true, null);
		RenderingBiasProperty = Animatable.RegisterProperty("RenderingBias", typeof(RenderingBias), typeFromHandle, RenderingBias.Performance, RenderingBiasPropertyChanged, ValidateEnums.IsRenderingBiasValid, isIndependentlyAnimated: false, null);
	}
}
