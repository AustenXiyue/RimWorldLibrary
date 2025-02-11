using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

internal static class HandoffBehaviorEnum
{
	[FriendAccessAllowed]
	internal static bool IsDefined(HandoffBehavior handoffBehavior)
	{
		if (handoffBehavior < HandoffBehavior.SnapshotAndReplace || handoffBehavior > HandoffBehavior.Compose)
		{
			return false;
		}
		return true;
	}
}
