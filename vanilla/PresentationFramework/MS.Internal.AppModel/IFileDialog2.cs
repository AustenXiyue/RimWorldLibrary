using System;
using System.Runtime.InteropServices;
using MS.Internal.Interop;

namespace MS.Internal.AppModel;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("61744fc7-85b5-4791-a9b0-272276309b13")]
internal interface IFileDialog2 : IFileDialog, IModalWindow
{
	[PreserveSig]
	new MS.Internal.Interop.HRESULT Show(nint parent);

	new void SetFileTypes(uint cFileTypes, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] COMDLG_FILTERSPEC[] rgFilterSpec);

	new void SetFileTypeIndex(uint iFileType);

	new uint GetFileTypeIndex();

	new uint Advise(IFileDialogEvents pfde);

	new void Unadvise(uint dwCookie);

	new void SetOptions(FOS fos);

	new FOS GetOptions();

	new void SetDefaultFolder(IShellItem psi);

	new void SetFolder(IShellItem psi);

	new IShellItem GetFolder();

	new IShellItem GetCurrentSelection();

	new void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);

	[return: MarshalAs(UnmanagedType.LPWStr)]
	new string GetFileName();

	new void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);

	new void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);

	new void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

	new IShellItem GetResult();

	new void AddPlace(IShellItem psi, FDAP alignment);

	new void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);

	new void Close([MarshalAs(UnmanagedType.Error)] int hr);

	new void SetClientGuid([In] ref Guid guid);

	new void ClearClientData();

	new void SetFilter([MarshalAs(UnmanagedType.Interface)] object pFilter);

	void SetCancelButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

	void SetNavigationRoot(IShellItem psi);
}
