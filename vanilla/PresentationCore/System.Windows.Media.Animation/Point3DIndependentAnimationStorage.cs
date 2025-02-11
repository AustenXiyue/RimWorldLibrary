using System.Windows.Media.Composition;
using System.Windows.Media.Media3D;

namespace System.Windows.Media.Animation;

internal class Point3DIndependentAnimationStorage : IndependentAnimationStorage
{
	protected override DUCE.ResourceType ResourceType => DUCE.ResourceType.TYPE_POINT3DRESOURCE;

	protected unsafe override void UpdateResourceCore(DUCE.Channel channel)
	{
		DependencyObject dependencyObject = (DependencyObject)_dependencyObject.Target;
		if (dependencyObject != null)
		{
			Point3D p = (Point3D)dependencyObject.GetValue(_dependencyProperty);
			DUCE.MILCMD_POINT3DRESOURCE mILCMD_POINT3DRESOURCE = default(DUCE.MILCMD_POINT3DRESOURCE);
			mILCMD_POINT3DRESOURCE.Type = MILCMD.MilCmdPoint3DResource;
			mILCMD_POINT3DRESOURCE.Handle = _duceResource.GetHandle(channel);
			mILCMD_POINT3DRESOURCE.Value = CompositionResourceManager.Point3DToMilPoint3F(p);
			channel.SendCommand((byte*)(&mILCMD_POINT3DRESOURCE), sizeof(DUCE.MILCMD_POINT3DRESOURCE));
		}
	}
}
