using System.Windows.Media.Composition;
using System.Windows.Media.Media3D;

namespace System.Windows.Media.Animation;

internal class Vector3DIndependentAnimationStorage : IndependentAnimationStorage
{
	protected override DUCE.ResourceType ResourceType => DUCE.ResourceType.TYPE_VECTOR3DRESOURCE;

	protected unsafe override void UpdateResourceCore(DUCE.Channel channel)
	{
		DependencyObject dependencyObject = (DependencyObject)_dependencyObject.Target;
		if (dependencyObject != null)
		{
			Vector3D v = (Vector3D)dependencyObject.GetValue(_dependencyProperty);
			DUCE.MILCMD_VECTOR3DRESOURCE mILCMD_VECTOR3DRESOURCE = default(DUCE.MILCMD_VECTOR3DRESOURCE);
			mILCMD_VECTOR3DRESOURCE.Type = MILCMD.MilCmdVector3DResource;
			mILCMD_VECTOR3DRESOURCE.Handle = _duceResource.GetHandle(channel);
			mILCMD_VECTOR3DRESOURCE.Value = CompositionResourceManager.Vector3DToMilPoint3F(v);
			channel.SendCommand((byte*)(&mILCMD_VECTOR3DRESOURCE), sizeof(DUCE.MILCMD_VECTOR3DRESOURCE));
		}
	}
}
