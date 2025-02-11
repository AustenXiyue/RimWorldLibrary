using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

[FriendAccessAllowed]
[Flags]
internal enum SubtreeFlag
{
	Reset = 1,
	ProcessRoot = 2,
	SkipSubtree = 4
}
