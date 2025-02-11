using System.Runtime.InteropServices;

namespace MS.Internal.Printing;

internal static class UnsafeNativeMethods
{
	[DllImport("comdlg32.dll", CharSet = CharSet.Auto)]
	internal static extern int PrintDlgEx(nint pdex);

	[DllImport("kernel32.dll")]
	internal static extern nint GlobalFree(nint hMem);

	[DllImport("kernel32.dll")]
	internal static extern nint GlobalLock(nint hMem);

	[DllImport("kernel32.dll")]
	internal static extern bool GlobalUnlock(nint hMem);
}
