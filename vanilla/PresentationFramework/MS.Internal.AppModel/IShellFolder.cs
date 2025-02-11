using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using MS.Internal.Interop;

namespace MS.Internal.AppModel;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("000214E6-0000-0000-C000-000000000046")]
internal interface IShellFolder
{
	void ParseDisplayName(nint hwnd, IBindCtx pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, [In][Out] ref int pchEaten, out nint ppidl, [In][Out] ref uint pdwAttributes);

	IEnumIDList EnumObjects(nint hwnd, SHCONTF grfFlags);

	[return: MarshalAs(UnmanagedType.Interface)]
	object BindToObject(nint pidl, IBindCtx pbc, [In] ref Guid riid);

	[return: MarshalAs(UnmanagedType.Interface)]
	object BindToStorage(nint pidl, IBindCtx pbc, [In] ref Guid riid);

	[PreserveSig]
	MS.Internal.Interop.HRESULT CompareIDs(nint lParam, nint pidl1, nint pidl2);

	[return: MarshalAs(UnmanagedType.Interface)]
	object CreateViewObject(nint hwndOwner, [In] ref Guid riid);

	void GetAttributesOf(uint cidl, nint apidl, [In][Out] ref SFGAO rgfInOut);

	[return: MarshalAs(UnmanagedType.Interface)]
	object GetUIObjectOf(nint hwndOwner, uint cidl, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.SysInt, SizeParamIndex = 1)] nint apidl, [In] ref Guid riid, [In][Out] ref uint rgfReserved);

	void GetDisplayNameOf(nint pidl, SHGDN uFlags, out nint pName);

	void SetNameOf(nint hwnd, nint pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, SHGDN uFlags, out nint ppidlOut);
}
