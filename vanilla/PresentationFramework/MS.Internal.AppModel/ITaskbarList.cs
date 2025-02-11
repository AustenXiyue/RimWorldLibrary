using System.Runtime.InteropServices;
using MS.Internal.Interop;

namespace MS.Internal.AppModel;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("56FDF342-FD6D-11d0-958A-006097C9A090")]
internal interface ITaskbarList
{
	[PreserveSig]
	MS.Internal.Interop.HRESULT HrInit();

	void AddTab(nint hwnd);

	void DeleteTab(nint hwnd);

	void ActivateTab(nint hwnd);

	void SetActiveAlt(nint hwnd);
}
