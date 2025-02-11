using System.Windows.Media.Composition;

namespace System.Windows.Media.Animation;

internal class RectIndependentAnimationStorage : IndependentAnimationStorage
{
	protected override DUCE.ResourceType ResourceType => DUCE.ResourceType.TYPE_RECTRESOURCE;

	protected unsafe override void UpdateResourceCore(DUCE.Channel channel)
	{
		DependencyObject dependencyObject = (DependencyObject)_dependencyObject.Target;
		if (dependencyObject != null)
		{
			Rect value = (Rect)dependencyObject.GetValue(_dependencyProperty);
			DUCE.MILCMD_RECTRESOURCE mILCMD_RECTRESOURCE = default(DUCE.MILCMD_RECTRESOURCE);
			mILCMD_RECTRESOURCE.Type = MILCMD.MilCmdRectResource;
			mILCMD_RECTRESOURCE.Handle = _duceResource.GetHandle(channel);
			mILCMD_RECTRESOURCE.Value = value;
			channel.SendCommand((byte*)(&mILCMD_RECTRESOURCE), sizeof(DUCE.MILCMD_RECTRESOURCE));
		}
	}
}
