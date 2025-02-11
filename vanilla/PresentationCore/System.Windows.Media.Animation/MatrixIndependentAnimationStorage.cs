using System.Windows.Media.Composition;

namespace System.Windows.Media.Animation;

internal class MatrixIndependentAnimationStorage : IndependentAnimationStorage
{
	protected override DUCE.ResourceType ResourceType => DUCE.ResourceType.TYPE_MATRIXRESOURCE;

	protected unsafe override void UpdateResourceCore(DUCE.Channel channel)
	{
		DependencyObject dependencyObject = (DependencyObject)_dependencyObject.Target;
		if (dependencyObject != null)
		{
			Matrix m = (Matrix)dependencyObject.GetValue(_dependencyProperty);
			DUCE.MILCMD_MATRIXRESOURCE mILCMD_MATRIXRESOURCE = default(DUCE.MILCMD_MATRIXRESOURCE);
			mILCMD_MATRIXRESOURCE.Type = MILCMD.MilCmdMatrixResource;
			mILCMD_MATRIXRESOURCE.Handle = _duceResource.GetHandle(channel);
			mILCMD_MATRIXRESOURCE.Value = CompositionResourceManager.MatrixToMilMatrix3x2D(m);
			channel.SendCommand((byte*)(&mILCMD_MATRIXRESOURCE), sizeof(DUCE.MILCMD_MATRIXRESOURCE));
		}
	}
}
