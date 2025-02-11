using System;
using System.Runtime.InteropServices;
using MS.Internal.Interop;

namespace MS.Internal.AppModel;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("6332debf-87b5-4670-90c0-5e57b408a49e")]
internal interface ICustomDestinationList
{
	void SetAppID([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

	[return: MarshalAs(UnmanagedType.Interface)]
	object BeginList(out uint pcMaxSlots, [In] ref Guid riid);

	[PreserveSig]
	MS.Internal.Interop.HRESULT AppendCategory([MarshalAs(UnmanagedType.LPWStr)] string pszCategory, IObjectArray poa);

	void AppendKnownCategory(KDC category);

	[PreserveSig]
	MS.Internal.Interop.HRESULT AddUserTasks(IObjectArray poa);

	void CommitList();

	[return: MarshalAs(UnmanagedType.Interface)]
	object GetRemovedDestinations([In] ref Guid riid);

	void DeleteList([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

	void AbortList();
}
