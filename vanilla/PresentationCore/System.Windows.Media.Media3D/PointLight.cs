using System.Windows.Media.Composition;

namespace System.Windows.Media.Media3D;

/// <summary>Represents a light source that has a specified position in space and projects its light in all directions.</summary>
public sealed class PointLight : PointLightBase
{
	internal DUCE.MultiChannelResource _duceResource;

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.PointLight" /> class at the origin. </summary>
	public PointLight()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.PointLight" /> class at the specified position, using the specified color.</summary>
	/// <param name="diffuseColor">The diffuse color.</param>
	/// <param name="position">The position.</param>
	public PointLight(Color diffuseColor, Point3D position)
		: this()
	{
		base.Color = diffuseColor;
		base.Position = position;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.PointLight" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PointLight Clone()
	{
		return (PointLight)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.PointLight" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PointLight CloneCurrentValue()
	{
		return (PointLight)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new PointLight();
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
			DUCE.MILCMD_POINTLIGHT mILCMD_POINTLIGHT = default(DUCE.MILCMD_POINTLIGHT);
			mILCMD_POINTLIGHT.Type = MILCMD.MilCmdPointLight;
			mILCMD_POINTLIGHT.Handle = _duceResource.GetHandle(channel);
			mILCMD_POINTLIGHT.htransform = htransform;
			if (animationResourceHandle.IsNull)
			{
				mILCMD_POINTLIGHT.color = CompositionResourceManager.ColorToMilColorF(base.Color);
			}
			mILCMD_POINTLIGHT.hColorAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_POINTLIGHT.position = CompositionResourceManager.Point3DToMilPoint3F(base.Position);
			}
			mILCMD_POINTLIGHT.hPositionAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_POINTLIGHT.range = base.Range;
			}
			mILCMD_POINTLIGHT.hRangeAnimations = animationResourceHandle3;
			if (animationResourceHandle4.IsNull)
			{
				mILCMD_POINTLIGHT.constantAttenuation = base.ConstantAttenuation;
			}
			mILCMD_POINTLIGHT.hConstantAttenuationAnimations = animationResourceHandle4;
			if (animationResourceHandle5.IsNull)
			{
				mILCMD_POINTLIGHT.linearAttenuation = base.LinearAttenuation;
			}
			mILCMD_POINTLIGHT.hLinearAttenuationAnimations = animationResourceHandle5;
			if (animationResourceHandle6.IsNull)
			{
				mILCMD_POINTLIGHT.quadraticAttenuation = base.QuadraticAttenuation;
			}
			mILCMD_POINTLIGHT.hQuadraticAttenuationAnimations = animationResourceHandle6;
			channel.SendCommand((byte*)(&mILCMD_POINTLIGHT), sizeof(DUCE.MILCMD_POINTLIGHT));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_POINTLIGHT))
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
}
