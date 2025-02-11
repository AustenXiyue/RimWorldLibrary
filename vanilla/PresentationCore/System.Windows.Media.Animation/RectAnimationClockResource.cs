using System.Windows.Media.Composition;

namespace System.Windows.Media.Animation;

internal class RectAnimationClockResource : AnimationClockResource, DUCE.IResource
{
	private Rect _baseValue;

	public Rect BaseValue => _baseValue;

	public Rect CurrentValue
	{
		get
		{
			if (_animationClock != null)
			{
				return ((RectAnimationBase)_animationClock.Timeline).GetCurrentValue(_baseValue, _baseValue, _animationClock);
			}
			return _baseValue;
		}
	}

	protected override DUCE.ResourceType ResourceType => DUCE.ResourceType.TYPE_RECTRESOURCE;

	public RectAnimationClockResource(Rect baseValue, AnimationClock animationClock)
		: base(animationClock)
	{
		_baseValue = baseValue;
	}

	protected unsafe override void UpdateResource(DUCE.ResourceHandle handle, DUCE.Channel channel)
	{
		DUCE.MILCMD_RECTRESOURCE mILCMD_RECTRESOURCE = default(DUCE.MILCMD_RECTRESOURCE);
		mILCMD_RECTRESOURCE.Type = MILCMD.MilCmdRectResource;
		mILCMD_RECTRESOURCE.Handle = handle;
		mILCMD_RECTRESOURCE.Value = CurrentValue;
		channel.SendCommand((byte*)(&mILCMD_RECTRESOURCE), sizeof(DUCE.MILCMD_RECTRESOURCE));
		base.IsResourceInvalid = false;
	}
}
