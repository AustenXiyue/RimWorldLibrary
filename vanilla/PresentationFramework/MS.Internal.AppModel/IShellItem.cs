using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using MS.Internal.Interop;

namespace MS.Internal.AppModel;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
internal interface IShellItem
{
	[return: MarshalAs(UnmanagedType.Interface)]
	object BindToHandler(IBindCtx pbc, [In] ref Guid bhid, [In] ref Guid riid);

	IShellItem GetParent();

	[return: MarshalAs(UnmanagedType.LPWStr)]
	string GetDisplayName(SIGDN sigdnName);

	uint GetAttributes(SFGAO sfgaoMask);

	int Compare(IShellItem psi, SICHINT hint);
}
