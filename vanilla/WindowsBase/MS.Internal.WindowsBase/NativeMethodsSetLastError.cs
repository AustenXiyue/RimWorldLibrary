using System.Runtime.InteropServices;
using System.Text;
using MS.Win32;

namespace MS.Internal.WindowsBase;

internal static class NativeMethodsSetLastError
{
	private const string PresentationNativeDll = "PresentationNative_cor3.dll";

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "EnableWindowWrapper", ExactSpelling = true, SetLastError = true)]
	public static extern bool EnableWindow(HandleRef hWnd, bool enable);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "GetAncestorWrapper")]
	public static extern nint GetAncestor(nint hwnd, int gaFlags);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "GetKeyboardLayoutListWrapper", ExactSpelling = true, SetLastError = true)]
	public static extern int GetKeyboardLayoutList(int size, [Out][MarshalAs(UnmanagedType.LPArray)] nint[] hkls);

	[DllImport("PresentationNative_cor3.dll", EntryPoint = "GetParentWrapper", SetLastError = true)]
	public static extern nint GetParent(HandleRef hWnd);

	[DllImport("PresentationNative_cor3.dll", EntryPoint = "GetWindowWrapper", ExactSpelling = true, SetLastError = true)]
	public static extern nint GetWindow(nint hWnd, int uCmd);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLongWrapper", SetLastError = true)]
	public static extern int GetWindowLong(HandleRef hWnd, int nIndex);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLongWrapper", SetLastError = true)]
	public static extern int GetWindowLong(nint hWnd, int nIndex);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLongWrapper", SetLastError = true)]
	public static extern NativeMethods.WndProc GetWindowLongWndProc(HandleRef hWnd, int nIndex);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtrWrapper", SetLastError = true)]
	public static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtrWrapper", SetLastError = true)]
	public static extern nint GetWindowLongPtr(HandleRef hWnd, int nIndex);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtrWrapper", SetLastError = true)]
	public static extern NativeMethods.WndProc GetWindowLongPtrWndProc(HandleRef hWnd, int nIndex);

	[DllImport("PresentationNative_cor3.dll", BestFitMapping = false, CharSet = CharSet.Auto, EntryPoint = "GetWindowTextWrapper", SetLastError = true)]
	public static extern int GetWindowText(HandleRef hWnd, [Out] StringBuilder lpString, int nMaxCount);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowTextLengthWrapper", SetLastError = true)]
	public static extern int GetWindowTextLength(HandleRef hWnd);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "MapWindowPointsWrapper", ExactSpelling = true, SetLastError = true)]
	public static extern int MapWindowPoints(HandleRef hWndFrom, HandleRef hWndTo, [In][Out] ref NativeMethods.RECT rect, int cPoints);

	[DllImport("PresentationNative_cor3.dll", EntryPoint = "SetFocusWrapper", SetLastError = true)]
	public static extern nint SetFocus(HandleRef hWnd);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongWrapper")]
	public static extern int SetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongWrapper")]
	public static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongWrapper", SetLastError = true)]
	public static extern int SetWindowLongWndProc(HandleRef hWnd, int nIndex, NativeMethods.WndProc dwNewLong);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtrWrapper")]
	public static extern nint SetWindowLongPtr(HandleRef hWnd, int nIndex, nint dwNewLong);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtrWrapper")]
	public static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

	[DllImport("PresentationNative_cor3.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtrWrapper", SetLastError = true)]
	public static extern nint SetWindowLongPtrWndProc(HandleRef hWnd, int nIndex, NativeMethods.WndProc dwNewLong);
}
