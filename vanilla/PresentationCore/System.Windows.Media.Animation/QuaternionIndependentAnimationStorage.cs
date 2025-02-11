using System.Windows.Media.Composition;
using System.Windows.Media.Media3D;

namespace System.Windows.Media.Animation;

internal class QuaternionIndependentAnimationStorage : IndependentAnimationStorage
{
	protected override DUCE.ResourceType ResourceType => DUCE.ResourceType.TYPE_QUATERNIONRESOURCE;

	protected unsafe override void UpdateResourceCore(DUCE.Channel channel)
	{
		DependencyObject dependencyObject = (DependencyObject)_dependencyObject.Target;
		if (dependencyObject != null)
		{
			Quaternion q = (Quaternion)dependencyObject.GetValue(_dependencyProperty);
			DUCE.MILCMD_QUATERNIONRESOURCE mILCMD_QUATERNIONRESOURCE = default(DUCE.MILCMD_QUATERNIONRESOURCE);
			mILCMD_QUATERNIONRESOURCE.Type = MILCMD.MilCmdQuaternionResource;
			mILCMD_QUATERNIONRESOURCE.Handle = _duceResource.GetHandle(channel);
			mILCMD_QUATERNIONRESOURCE.Value = CompositionResourceManager.QuaternionToMilQuaternionF(q);
			channel.SendCommand((byte*)(&mILCMD_QUATERNIONRESOURCE), sizeof(DUCE.MILCMD_QUATERNIONRESOURCE));
		}
	}
}
