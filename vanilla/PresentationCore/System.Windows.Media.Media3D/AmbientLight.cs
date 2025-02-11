using System.Windows.Media.Composition;

namespace System.Windows.Media.Media3D;

/// <summary> Light object that applies light to objects uniformly, regardless of their shape. </summary>
public sealed class AmbientLight : Light
{
	internal DUCE.MultiChannelResource _duceResource;

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.AmbientLight" /> class. </summary>
	public AmbientLight()
		: this(Colors.White)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.AmbientLight" /> class with the specified color.</summary>
	/// <param name="ambientColor">Color of the new light.</param>
	public AmbientLight(Color ambientColor)
	{
		base.Color = ambientColor;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.AmbientLight" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new AmbientLight Clone()
	{
		return (AmbientLight)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.AmbientLight" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new AmbientLight CloneCurrentValue()
	{
		return (AmbientLight)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new AmbientLight();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform3D transform = base.Transform;
			DUCE.ResourceHandle htransform = ((transform != null && transform != Transform3D.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(Light.ColorProperty, channel);
			DUCE.MILCMD_AMBIENTLIGHT mILCMD_AMBIENTLIGHT = default(DUCE.MILCMD_AMBIENTLIGHT);
			mILCMD_AMBIENTLIGHT.Type = MILCMD.MilCmdAmbientLight;
			mILCMD_AMBIENTLIGHT.Handle = _duceResource.GetHandle(channel);
			mILCMD_AMBIENTLIGHT.htransform = htransform;
			if (animationResourceHandle.IsNull)
			{
				mILCMD_AMBIENTLIGHT.color = CompositionResourceManager.ColorToMilColorF(base.Color);
			}
			mILCMD_AMBIENTLIGHT.hColorAnimations = animationResourceHandle;
			channel.SendCommand((byte*)(&mILCMD_AMBIENTLIGHT), sizeof(DUCE.MILCMD_AMBIENTLIGHT));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_AMBIENTLIGHT))
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
