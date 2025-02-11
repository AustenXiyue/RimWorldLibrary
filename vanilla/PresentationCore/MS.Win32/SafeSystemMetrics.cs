using MS.Internal.Interop;
using MS.Internal.PresentationCore;

namespace MS.Win32;

[FriendAccessAllowed]
internal sealed class SafeSystemMetrics
{
	internal static int DoubleClickDeltaX => MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.CXDOUBLECLK);

	internal static int DoubleClickDeltaY => MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.CYDOUBLECLK);

	internal static int DragDeltaX => MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.CXDRAG);

	internal static int DragDeltaY => MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.CYDRAG);

	internal static bool IsImmEnabled => MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.IMMENABLED) != 0;

	private SafeSystemMetrics()
	{
	}
}
