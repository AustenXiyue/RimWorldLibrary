using MS.Internal.WindowsBase;

namespace System.Windows;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal enum FullValueSource : short
{
	ValueSourceMask = 15,
	ModifiersMask = 112,
	IsExpression = 16,
	IsAnimated = 32,
	IsCoerced = 64,
	IsPotentiallyADeferredReference = 128,
	HasExpressionMarker = 256,
	IsCoercedWithCurrentValue = 512
}
