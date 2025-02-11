using MS.Internal.WindowsBase;

namespace System.Windows;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal delegate object GetReadOnlyValueCallback(DependencyObject d, out BaseValueSourceInternal source);
