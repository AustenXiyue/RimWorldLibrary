using System.Windows.Media.Composition;

namespace System.Windows.Media.Animation;

internal class SizeAnimationClockResource : AnimationClockResource, DUCE.IResource
{
	private Size _baseValue;

	public Size BaseValue => _baseValue;

	public Size CurrentValue
	{
		get
		{
			if (_animationClock != null)
			{
				return ((SizeAnimationBase)_animationClock.Timeline).GetCurrentValue(_baseValue, _baseValue, _animationClock);
			}
			return _baseValue;
		}
	}

	protected override DUCE.ResourceType ResourceType => DUCE.ResourceType.TYPE_SIZERESOURCE;

	public SizeAnimationClockResource(Size baseValue, AnimationClock animationClock)
		: base(animationClock)
	{
		_baseValue = baseValue;
	}

	protected unsafe override void UpdateResource(DUCE.ResourceHandle handle, DUCE.Channel channel)
	{
		DUCE.MILCMD_SIZERESOURCE mILCMD_SIZERESOURCE = default(DUCE.MILCMD_SIZERESOURCE);
		mILCMD_SIZERESOURCE.Type = MILCMD.MilCmdSizeResource;
		mILCMD_SIZERESOURCE.Handle = handle;
		mILCMD_SIZERESOURCE.Value = CurrentValue;
		channel.SendCommand((byte*)(&mILCMD_SIZERESOURCE), sizeof(DUCE.MILCMD_SIZERESOURCE));
		base.IsResourceInvalid = false;
	}
}
