using System.Runtime.InteropServices;
using MS.Internal.Interop;
using MS.Win32;

namespace MS.Internal.AppModel;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
internal interface ITaskbarList3 : ITaskbarList2, ITaskbarList
{
	new void HrInit();

	new void AddTab(nint hwnd);

	new void DeleteTab(nint hwnd);

	new void ActivateTab(nint hwnd);

	new void SetActiveAlt(nint hwnd);

	new void MarkFullscreenWindow(nint hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

	[PreserveSig]
	MS.Internal.Interop.HRESULT SetProgressValue(nint hwnd, ulong ullCompleted, ulong ullTotal);

	[PreserveSig]
	MS.Internal.Interop.HRESULT SetProgressState(nint hwnd, TBPF tbpFlags);

	[PreserveSig]
	MS.Internal.Interop.HRESULT RegisterTab(nint hwndTab, nint hwndMDI);

	[PreserveSig]
	MS.Internal.Interop.HRESULT UnregisterTab(nint hwndTab);

	[PreserveSig]
	MS.Internal.Interop.HRESULT SetTabOrder(nint hwndTab, nint hwndInsertBefore);

	[PreserveSig]
	MS.Internal.Interop.HRESULT SetTabActive(nint hwndTab, nint hwndMDI, uint dwReserved);

	[PreserveSig]
	MS.Internal.Interop.HRESULT ThumbBarAddButtons(nint hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] THUMBBUTTON[] pButtons);

	[PreserveSig]
	MS.Internal.Interop.HRESULT ThumbBarUpdateButtons(nint hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] THUMBBUTTON[] pButtons);

	[PreserveSig]
	MS.Internal.Interop.HRESULT ThumbBarSetImageList(nint hwnd, [MarshalAs(UnmanagedType.IUnknown)] object himl);

	[PreserveSig]
	MS.Internal.Interop.HRESULT SetOverlayIcon(nint hwnd, MS.Win32.NativeMethods.IconHandle hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);

	[PreserveSig]
	MS.Internal.Interop.HRESULT SetThumbnailTooltip(nint hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);

	[PreserveSig]
	MS.Internal.Interop.HRESULT SetThumbnailClip(nint hwnd, MS.Win32.NativeMethods.RefRECT prcClip);
}
