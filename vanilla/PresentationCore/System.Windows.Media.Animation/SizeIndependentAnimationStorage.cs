using System.Windows.Media.Composition;

namespace System.Windows.Media.Animation;

internal class SizeIndependentAnimationStorage : IndependentAnimationStorage
{
	protected override DUCE.ResourceType ResourceType => DUCE.ResourceType.TYPE_SIZERESOURCE;

	protected unsafe override void UpdateResourceCore(DUCE.Channel channel)
	{
		DependencyObject dependencyObject = (DependencyObject)_dependencyObject.Target;
		if (dependencyObject != null)
		{
			Size value = (Size)dependencyObject.GetValue(_dependencyProperty);
			DUCE.MILCMD_SIZERESOURCE mILCMD_SIZERESOURCE = default(DUCE.MILCMD_SIZERESOURCE);
			mILCMD_SIZERESOURCE.Type = MILCMD.MilCmdSizeResource;
			mILCMD_SIZERESOURCE.Handle = _duceResource.GetHandle(channel);
			mILCMD_SIZERESOURCE.Value = value;
			channel.SendCommand((byte*)(&mILCMD_SIZERESOURCE), sizeof(DUCE.MILCMD_SIZERESOURCE));
		}
	}
}
