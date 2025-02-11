using System.Runtime.InteropServices;

namespace Standard;

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
	HRESULT SetProgressValue(nint hwnd, ulong ullCompleted, ulong ullTotal);

	[PreserveSig]
	HRESULT SetProgressState(nint hwnd, TBPF tbpFlags);

	[PreserveSig]
	HRESULT RegisterTab(nint hwndTab, nint hwndMDI);

	[PreserveSig]
	HRESULT UnregisterTab(nint hwndTab);

	[PreserveSig]
	HRESULT SetTabOrder(nint hwndTab, nint hwndInsertBefore);

	[PreserveSig]
	HRESULT SetTabActive(nint hwndTab, nint hwndMDI, uint dwReserved);

	[PreserveSig]
	HRESULT ThumbBarAddButtons(nint hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] THUMBBUTTON[] pButtons);

	[PreserveSig]
	HRESULT ThumbBarUpdateButtons(nint hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] THUMBBUTTON[] pButtons);

	[PreserveSig]
	HRESULT ThumbBarSetImageList(nint hwnd, [MarshalAs(UnmanagedType.IUnknown)] object himl);

	[PreserveSig]
	HRESULT SetOverlayIcon(nint hwnd, nint hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);

	[PreserveSig]
	HRESULT SetThumbnailTooltip(nint hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);

	[PreserveSig]
	HRESULT SetThumbnailClip(nint hwnd, RefRECT prcClip);
}
