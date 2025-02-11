using MS.Internal.WindowsBase;

namespace System.Windows;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal abstract class DeferredReference
{
	internal abstract object GetValue(BaseValueSourceInternal valueSource);

	internal abstract Type GetValueType();
}
