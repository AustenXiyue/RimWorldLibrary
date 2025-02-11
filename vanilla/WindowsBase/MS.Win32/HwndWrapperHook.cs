using MS.Internal.WindowsBase;

namespace MS.Win32;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal delegate nint HwndWrapperHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled);
