using System.Windows.Media.Composition;

namespace System.Windows.Media.Animation;

internal class PointAnimationClockResource : AnimationClockResource, DUCE.IResource
{
	private Point _baseValue;

	public Point BaseValue => _baseValue;

	public Point CurrentValue
	{
		get
		{
			if (_animationClock != null)
			{
				return ((PointAnimationBase)_animationClock.Timeline).GetCurrentValue(_baseValue, _baseValue, _animationClock);
			}
			return _baseValue;
		}
	}

	protected override DUCE.ResourceType ResourceType => DUCE.ResourceType.TYPE_POINTRESOURCE;

	public PointAnimationClockResource(Point baseValue, AnimationClock animationClock)
		: base(animationClock)
	{
		_baseValue = baseValue;
	}

	protected unsafe override void UpdateResource(DUCE.ResourceHandle handle, DUCE.Channel channel)
	{
		DUCE.MILCMD_POINTRESOURCE mILCMD_POINTRESOURCE = default(DUCE.MILCMD_POINTRESOURCE);
		mILCMD_POINTRESOURCE.Type = MILCMD.MilCmdPointResource;
		mILCMD_POINTRESOURCE.Handle = handle;
		mILCMD_POINTRESOURCE.Value = CurrentValue;
		channel.SendCommand((byte*)(&mILCMD_POINTRESOURCE), sizeof(DUCE.MILCMD_POINTRESOURCE));
		base.IsResourceInvalid = false;
	}
}
