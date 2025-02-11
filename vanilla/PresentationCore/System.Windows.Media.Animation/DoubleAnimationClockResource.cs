using System.Windows.Media.Composition;

namespace System.Windows.Media.Animation;

internal class DoubleAnimationClockResource : AnimationClockResource, DUCE.IResource
{
	private double _baseValue;

	public double BaseValue => _baseValue;

	public double CurrentValue
	{
		get
		{
			if (_animationClock != null)
			{
				return ((DoubleAnimationBase)_animationClock.Timeline).GetCurrentValue(_baseValue, _baseValue, _animationClock);
			}
			return _baseValue;
		}
	}

	protected override DUCE.ResourceType ResourceType => DUCE.ResourceType.TYPE_DOUBLERESOURCE;

	public DoubleAnimationClockResource(double baseValue, AnimationClock animationClock)
		: base(animationClock)
	{
		_baseValue = baseValue;
	}

	protected unsafe override void UpdateResource(DUCE.ResourceHandle handle, DUCE.Channel channel)
	{
		DUCE.MILCMD_DOUBLERESOURCE mILCMD_DOUBLERESOURCE = default(DUCE.MILCMD_DOUBLERESOURCE);
		mILCMD_DOUBLERESOURCE.Type = MILCMD.MilCmdDoubleResource;
		mILCMD_DOUBLERESOURCE.Handle = handle;
		mILCMD_DOUBLERESOURCE.Value = CurrentValue;
		channel.SendCommand((byte*)(&mILCMD_DOUBLERESOURCE), sizeof(DUCE.MILCMD_DOUBLERESOURCE));
		base.IsResourceInvalid = false;
	}
}
