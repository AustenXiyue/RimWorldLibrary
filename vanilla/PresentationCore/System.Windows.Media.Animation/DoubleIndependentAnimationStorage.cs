using System.Windows.Media.Composition;

namespace System.Windows.Media.Animation;

internal class DoubleIndependentAnimationStorage : IndependentAnimationStorage
{
	protected override DUCE.ResourceType ResourceType => DUCE.ResourceType.TYPE_DOUBLERESOURCE;

	protected unsafe override void UpdateResourceCore(DUCE.Channel channel)
	{
		DependencyObject dependencyObject = (DependencyObject)_dependencyObject.Target;
		if (dependencyObject != null)
		{
			double value = (double)dependencyObject.GetValue(_dependencyProperty);
			DUCE.MILCMD_DOUBLERESOURCE mILCMD_DOUBLERESOURCE = default(DUCE.MILCMD_DOUBLERESOURCE);
			mILCMD_DOUBLERESOURCE.Type = MILCMD.MilCmdDoubleResource;
			mILCMD_DOUBLERESOURCE.Handle = _duceResource.GetHandle(channel);
			mILCMD_DOUBLERESOURCE.Value = value;
			channel.SendCommand((byte*)(&mILCMD_DOUBLERESOURCE), sizeof(DUCE.MILCMD_DOUBLERESOURCE));
		}
	}
}
