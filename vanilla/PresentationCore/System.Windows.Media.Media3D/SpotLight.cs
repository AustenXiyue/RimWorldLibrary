using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media.Media3D;

/// <summary>Light object that projects its effect in a cone-shaped area along a specified direction.</summary>
public sealed class SpotLight : PointLightBase
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.SpotLight.Direction" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.SpotLight.Direction" /> dependency property. </returns>
	public static readonly DependencyProperty DirectionProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.SpotLight.OuterConeAngle" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.SpotLight.OuterConeAngle" /> dependency property.</returns>
	public static readonly DependencyProperty OuterConeAngleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.SpotLight.InnerConeAngle" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.SpotLight.InnerConeAngle" /> dependency property.</returns>
	public static readonly DependencyProperty InnerConeAngleProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Vector3D s_Direction;

	internal const double c_OuterConeAngle = 90.0;

	internal const double c_InnerConeAngle = 180.0;

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> that specifies the direction in which the <see cref="T:System.Windows.Media.Media3D.SpotLight" /> projects its light.  </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Vector3D" /> that specifies the direction of the Spotlight's projection. The default value is 0,0,-1.</returns>
	public Vector3D Direction
	{
		get
		{
			return (Vector3D)GetValue(DirectionProperty);
		}
		set
		{
			SetValueInternal(DirectionProperty, value);
		}
	}

	/// <summary> Gets or sets an angle that specifies the proportion of a <see cref="T:System.Windows.Media.Media3D.SpotLight" />'s cone-shaped projection outside which the light does not illuminate objects in the scene.  </summary>
	/// <returns>The angle in degrees that specifies the proportion of a <see cref="T:System.Windows.Media.Media3D.SpotLight" />'s cone-shaped projection outside which the light does not illuminate objects in the scene. The default value is 90.</returns>
	public double OuterConeAngle
	{
		get
		{
			return (double)GetValue(OuterConeAngleProperty);
		}
		set
		{
			SetValueInternal(OuterConeAngleProperty, value);
		}
	}

	/// <summary> Gets or sets an angle that specifies the proportion of a <see cref="T:System.Windows.Media.Media3D.SpotLight" />'s cone-shaped projection in which the light fully illuminates objects in the scene.  </summary>
	/// <returns>The angle in degrees that specifies the proportion of a <see cref="T:System.Windows.Media.Media3D.SpotLight" />'s cone-shaped projection in which the light fully illuminates objects in the scene. The default value is 180.</returns>
	public double InnerConeAngle
	{
		get
		{
			return (double)GetValue(InnerConeAngleProperty);
		}
		set
		{
			SetValueInternal(InnerConeAngleProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.SpotLight" /> class using the specified color, position, direction, and cone angles. </summary>
	/// <param name="diffuseColor">Diffuse color of the new <see cref="T:System.Windows.Media.Media3D.SpotLight" />.</param>
	/// <param name="position">Position of the new <see cref="T:System.Windows.Media.Media3D.SpotLight" />.</param>
	/// <param name="direction">Direction of the new <see cref="T:System.Windows.Media.Media3D.SpotLight" />.</param>
	/// <param name="outerConeAngle">Angle that defines a cone outside which the light does not illuminate objects in the scene.</param>
	/// <param name="innerConeAngle">Angle that defines a cone within which the light fully illuminates objects in the scene.</param>
	public SpotLight(Color diffuseColor, Point3D position, Vector3D direction, double outerConeAngle, double innerConeAngle)
		: this()
	{
		base.Color = diffuseColor;
		base.Position = position;
		Direction = direction;
		OuterConeAngle = outerConeAngle;
		InnerConeAngle = innerConeAngle;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.SpotLight" /> class. </summary>
	public SpotLight()
	{
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.SpotLight" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new SpotLight Clone()
	{
		return (SpotLight)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.SpotLight" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new SpotLight CloneCurrentValue()
	{
		return (SpotLight)base.CloneCurrentValue();
	}

	private static void DirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((SpotLight)d).PropertyChanged(DirectionProperty);
	}

	private static void OuterConeAnglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((SpotLight)d).PropertyChanged(OuterConeAngleProperty);
	}

	private static void InnerConeAnglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((SpotLight)d).PropertyChanged(InnerConeAngleProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new SpotLight();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform3D transform = base.Transform;
			DUCE.ResourceHandle htransform = ((transform != null && transform != Transform3D.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(Light.ColorProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(PointLightBase.PositionProperty, channel);
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(PointLightBase.RangeProperty, channel);
			DUCE.ResourceHandle animationResourceHandle4 = GetAnimationResourceHandle(PointLightBase.ConstantAttenuationProperty, channel);
			DUCE.ResourceHandle animationResourceHandle5 = GetAnimationResourceHandle(PointLightBase.LinearAttenuationProperty, channel);
			DUCE.ResourceHandle animationResourceHandle6 = GetAnimationResourceHandle(PointLightBase.QuadraticAttenuationProperty, channel);
			DUCE.ResourceHandle animationResourceHandle7 = GetAnimationResourceHandle(DirectionProperty, channel);
			DUCE.ResourceHandle animationResourceHandle8 = GetAnimationResourceHandle(OuterConeAngleProperty, channel);
			DUCE.ResourceHandle animationResourceHandle9 = GetAnimationResourceHandle(InnerConeAngleProperty, channel);
			DUCE.MILCMD_SPOTLIGHT mILCMD_SPOTLIGHT = default(DUCE.MILCMD_SPOTLIGHT);
			mILCMD_SPOTLIGHT.Type = MILCMD.MilCmdSpotLight;
			mILCMD_SPOTLIGHT.Handle = _duceResource.GetHandle(channel);
			mILCMD_SPOTLIGHT.htransform = htransform;
			if (animationResourceHandle.IsNull)
			{
				mILCMD_SPOTLIGHT.color = CompositionResourceManager.ColorToMilColorF(base.Color);
			}
			mILCMD_SPOTLIGHT.hColorAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_SPOTLIGHT.position = CompositionResourceManager.Point3DToMilPoint3F(base.Position);
			}
			mILCMD_SPOTLIGHT.hPositionAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_SPOTLIGHT.range = base.Range;
			}
			mILCMD_SPOTLIGHT.hRangeAnimations = animationResourceHandle3;
			if (animationResourceHandle4.IsNull)
			{
				mILCMD_SPOTLIGHT.constantAttenuation = base.ConstantAttenuation;
			}
			mILCMD_SPOTLIGHT.hConstantAttenuationAnimations = animationResourceHandle4;
			if (animationResourceHandle5.IsNull)
			{
				mILCMD_SPOTLIGHT.linearAttenuation = base.LinearAttenuation;
			}
			mILCMD_SPOTLIGHT.hLinearAttenuationAnimations = animationResourceHandle5;
			if (animationResourceHandle6.IsNull)
			{
				mILCMD_SPOTLIGHT.quadraticAttenuation = base.QuadraticAttenuation;
			}
			mILCMD_SPOTLIGHT.hQuadraticAttenuationAnimations = animationResourceHandle6;
			if (animationResourceHandle7.IsNull)
			{
				mILCMD_SPOTLIGHT.direction = CompositionResourceManager.Vector3DToMilPoint3F(Direction);
			}
			mILCMD_SPOTLIGHT.hDirectionAnimations = animationResourceHandle7;
			if (animationResourceHandle8.IsNull)
			{
				mILCMD_SPOTLIGHT.outerConeAngle = OuterConeAngle;
			}
			mILCMD_SPOTLIGHT.hOuterConeAngleAnimations = animationResourceHandle8;
			if (animationResourceHandle9.IsNull)
			{
				mILCMD_SPOTLIGHT.innerConeAngle = InnerConeAngle;
			}
			mILCMD_SPOTLIGHT.hInnerConeAngleAnimations = animationResourceHandle9;
			channel.SendCommand((byte*)(&mILCMD_SPOTLIGHT), sizeof(DUCE.MILCMD_SPOTLIGHT));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_SPOTLIGHT))
		{
			((DUCE.IResource)base.Transform)?.AddRefOnChannel(channel);
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			((DUCE.IResource)base.Transform)?.ReleaseOnChannel(channel);
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

	static SpotLight()
	{
		s_Direction = new Vector3D(0.0, 0.0, -1.0);
		Type typeFromHandle = typeof(SpotLight);
		DirectionProperty = Animatable.RegisterProperty("Direction", typeof(Vector3D), typeFromHandle, new Vector3D(0.0, 0.0, -1.0), DirectionPropertyChanged, null, isIndependentlyAnimated: true, null);
		OuterConeAngleProperty = Animatable.RegisterProperty("OuterConeAngle", typeof(double), typeFromHandle, 90.0, OuterConeAnglePropertyChanged, null, isIndependentlyAnimated: true, null);
		InnerConeAngleProperty = Animatable.RegisterProperty("InnerConeAngle", typeof(double), typeFromHandle, 180.0, InnerConeAnglePropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
