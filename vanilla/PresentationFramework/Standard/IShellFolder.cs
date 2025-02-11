using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Standard;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("000214E6-0000-0000-C000-000000000046")]
internal interface IShellFolder
{
	void ParseDisplayName([In] nint hwnd, [In] IBindCtx pbc, [In][MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, [In][Out] ref int pchEaten, out nint ppidl, [In][Out] ref uint pdwAttributes);

	IEnumIDList EnumObjects([In] nint hwnd, [In] SHCONTF grfFlags);

	[return: MarshalAs(UnmanagedType.Interface)]
	object BindToObject([In] nint pidl, [In] IBindCtx pbc, [In] ref Guid riid);

	[return: MarshalAs(UnmanagedType.Interface)]
	object BindToStorage([In] nint pidl, [In] IBindCtx pbc, [In] ref Guid riid);

	[PreserveSig]
	HRESULT CompareIDs([In] nint lParam, [In] nint pidl1, [In] nint pidl2);

	[return: MarshalAs(UnmanagedType.Interface)]
	object CreateViewObject([In] nint hwndOwner, [In] ref Guid riid);

	void GetAttributesOf([In] uint cidl, [In] nint apidl, [In][Out] ref SFGAO rgfInOut);

	[return: MarshalAs(UnmanagedType.Interface)]
	object GetUIObjectOf([In] nint hwndOwner, [In] uint cidl, [In][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.SysInt, SizeParamIndex = 2)] nint apidl, [In] ref Guid riid, [In][Out] ref uint rgfReserved);

	void GetDisplayNameOf([In] nint pidl, [In] SHGDN uFlags, out nint pName);

	void SetNameOf([In] nint hwnd, [In] nint pidl, [In][MarshalAs(UnmanagedType.LPWStr)] string pszName, [In] SHGDN uFlags, out nint ppidlOut);
}
