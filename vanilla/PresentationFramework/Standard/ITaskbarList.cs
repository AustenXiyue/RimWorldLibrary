using System.Runtime.InteropServices;

namespace Standard;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("56FDF342-FD6D-11d0-958A-006097C9A090")]
internal interface ITaskbarList
{
	void HrInit();

	void AddTab(nint hwnd);

	void DeleteTab(nint hwnd);

	void ActivateTab(nint hwnd);

	void SetActiveAlt(nint hwnd);
}
