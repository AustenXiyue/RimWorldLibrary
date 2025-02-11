using MS.Internal.WindowsBase;

namespace System.Windows;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal enum RequestFlags
{
	FullyResolved = 0,
	AnimationBaseValue = 1,
	CoercionBaseValue = 2,
	DeferredReferences = 4,
	SkipDefault = 8,
	RawEntry = 0x10
}
