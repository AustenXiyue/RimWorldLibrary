using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using MS.Internal.Interop;

namespace MS.Internal.AppModel;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("7e9fb0d3-919f-4307-ab2e-9b1860310c93")]
internal interface IShellItem2 : IShellItem
{
	[return: MarshalAs(UnmanagedType.Interface)]
	new object BindToHandler(IBindCtx pbc, [In] ref Guid bhid, [In] ref Guid riid);

	new IShellItem GetParent();

	[return: MarshalAs(UnmanagedType.LPWStr)]
	new string GetDisplayName(SIGDN sigdnName);

	new SFGAO GetAttributes(SFGAO sfgaoMask);

	new int Compare(IShellItem psi, SICHINT hint);

	[return: MarshalAs(UnmanagedType.Interface)]
	object GetPropertyStore(GPS flags, [In] ref Guid riid);

	[return: MarshalAs(UnmanagedType.Interface)]
	object GetPropertyStoreWithCreateObject(GPS flags, [MarshalAs(UnmanagedType.IUnknown)] object punkCreateObject, [In] ref Guid riid);

	[return: MarshalAs(UnmanagedType.Interface)]
	object GetPropertyStoreForKeys(nint rgKeys, uint cKeys, GPS flags, [In] ref Guid riid);

	[return: MarshalAs(UnmanagedType.Interface)]
	object GetPropertyDescriptionList(nint keyType, [In] ref Guid riid);

	void Update(IBindCtx pbc);

	void GetProperty(nint key, [In][Out] PROPVARIANT pv);

	Guid GetCLSID(nint key);

	FILETIME GetFileTime(nint key);

	int GetInt32(nint key);

	[return: MarshalAs(UnmanagedType.LPWStr)]
	string GetString(nint key);

	uint GetUInt32(nint key);

	ulong GetUInt64(nint key);

	[return: MarshalAs(UnmanagedType.Bool)]
	bool GetBool(nint key);
}
