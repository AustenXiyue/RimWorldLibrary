using System.Windows.Media.Composition;

namespace System.Windows.Media.Animation;

internal class PointIndependentAnimationStorage : IndependentAnimationStorage
{
	protected override DUCE.ResourceType ResourceType => DUCE.ResourceType.TYPE_POINTRESOURCE;

	protected unsafe override void UpdateResourceCore(DUCE.Channel channel)
	{
		DependencyObject dependencyObject = (DependencyObject)_dependencyObject.Target;
		if (dependencyObject != null)
		{
			Point value = (Point)dependencyObject.GetValue(_dependencyProperty);
			DUCE.MILCMD_POINTRESOURCE mILCMD_POINTRESOURCE = default(DUCE.MILCMD_POINTRESOURCE);
			mILCMD_POINTRESOURCE.Type = MILCMD.MilCmdPointResource;
			mILCMD_POINTRESOURCE.Handle = _duceResource.GetHandle(channel);
			mILCMD_POINTRESOURCE.Value = value;
			channel.SendCommand((byte*)(&mILCMD_POINTRESOURCE), sizeof(DUCE.MILCMD_POINTRESOURCE));
		}
	}
}
