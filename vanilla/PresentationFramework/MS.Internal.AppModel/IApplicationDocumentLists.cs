using System;
using System.Runtime.InteropServices;
using MS.Internal.Interop;

namespace MS.Internal.AppModel;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("3c594f9f-9f30-47a1-979a-c9e83d3d0a06")]
internal interface IApplicationDocumentLists
{
	void SetAppID([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

	[return: MarshalAs(UnmanagedType.IUnknown)]
	object GetList(ADLT listtype, uint cItemsDesired, [In] ref Guid riid);
}
