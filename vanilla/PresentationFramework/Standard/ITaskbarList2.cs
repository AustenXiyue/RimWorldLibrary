using System.Runtime.InteropServices;

namespace Standard;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("602D4995-B13A-429b-A66E-1935E44F4317")]
internal interface ITaskbarList2 : ITaskbarList
{
	new void HrInit();

	new void AddTab(nint hwnd);

	new void DeleteTab(nint hwnd);

	new void ActivateTab(nint hwnd);

	new void SetActiveAlt(nint hwnd);

	void MarkFullscreenWindow(nint hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
}
