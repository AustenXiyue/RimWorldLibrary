using System.Windows.Media.Composition;

namespace System.Windows.Media.Animation;

internal class ColorIndependentAnimationStorage : IndependentAnimationStorage
{
	protected override DUCE.ResourceType ResourceType => DUCE.ResourceType.TYPE_COLORRESOURCE;

	protected unsafe override void UpdateResourceCore(DUCE.Channel channel)
	{
		DependencyObject dependencyObject = (DependencyObject)_dependencyObject.Target;
		if (dependencyObject != null)
		{
			Color c = (Color)dependencyObject.GetValue(_dependencyProperty);
			DUCE.MILCMD_COLORRESOURCE mILCMD_COLORRESOURCE = default(DUCE.MILCMD_COLORRESOURCE);
			mILCMD_COLORRESOURCE.Type = MILCMD.MilCmdColorResource;
			mILCMD_COLORRESOURCE.Handle = _duceResource.GetHandle(channel);
			mILCMD_COLORRESOURCE.Value = CompositionResourceManager.ColorToMilColorF(c);
			channel.SendCommand((byte*)(&mILCMD_COLORRESOURCE), sizeof(DUCE.MILCMD_COLORRESOURCE));
		}
	}
}
