using System.Windows.Media.Composition;

namespace System.Windows.Media.Effects;

internal sealed class ImplicitInputBrush : Brush
{
	internal DUCE.MultiChannelResource _duceResource;

	public new ImplicitInputBrush Clone()
	{
		return (ImplicitInputBrush)base.Clone();
	}

	public new ImplicitInputBrush CloneCurrentValue()
	{
		return (ImplicitInputBrush)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new ImplicitInputBrush();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform transform = base.Transform;
			Transform relativeTransform = base.RelativeTransform;
			DUCE.ResourceHandle hTransform = ((transform != null && transform != Transform.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle hRelativeTransform = ((relativeTransform != null && relativeTransform != Transform.Identity) ? ((DUCE.IResource)relativeTransform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(Brush.OpacityProperty, channel);
			DUCE.MILCMD_IMPLICITINPUTBRUSH mILCMD_IMPLICITINPUTBRUSH = default(DUCE.MILCMD_IMPLICITINPUTBRUSH);
			mILCMD_IMPLICITINPUTBRUSH.Type = MILCMD.MilCmdImplicitInputBrush;
			mILCMD_IMPLICITINPUTBRUSH.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_IMPLICITINPUTBRUSH.Opacity = base.Opacity;
			}
			mILCMD_IMPLICITINPUTBRUSH.hOpacityAnimations = animationResourceHandle;
			mILCMD_IMPLICITINPUTBRUSH.hTransform = hTransform;
			mILCMD_IMPLICITINPUTBRUSH.hRelativeTransform = hRelativeTransform;
			channel.SendCommand((byte*)(&mILCMD_IMPLICITINPUTBRUSH), sizeof(DUCE.MILCMD_IMPLICITINPUTBRUSH));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_IMPLICITINPUTBRUSH))
		{
			((DUCE.IResource)base.Transform)?.AddRefOnChannel(channel);
			((DUCE.IResource)base.RelativeTransform)?.AddRefOnChannel(channel);
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
			((DUCE.IResource)base.RelativeTransform)?.ReleaseOnChannel(channel);
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
