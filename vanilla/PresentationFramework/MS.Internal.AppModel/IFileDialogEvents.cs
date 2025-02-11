using System.Runtime.InteropServices;
using MS.Internal.Interop;

namespace MS.Internal.AppModel;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("973510DB-7D7F-452B-8975-74A85828D354")]
internal interface IFileDialogEvents
{
	[PreserveSig]
	MS.Internal.Interop.HRESULT OnFileOk(IFileDialog pfd);

	[PreserveSig]
	MS.Internal.Interop.HRESULT OnFolderChanging(IFileDialog pfd, IShellItem psiFolder);

	[PreserveSig]
	MS.Internal.Interop.HRESULT OnFolderChange(IFileDialog pfd);

	[PreserveSig]
	MS.Internal.Interop.HRESULT OnSelectionChange(IFileDialog pfd);

	[PreserveSig]
	MS.Internal.Interop.HRESULT OnShareViolation(IFileDialog pfd, IShellItem psi, out FDESVR pResponse);

	[PreserveSig]
	MS.Internal.Interop.HRESULT OnTypeChange(IFileDialog pfd);

	[PreserveSig]
	MS.Internal.Interop.HRESULT OnOverwrite(IFileDialog pfd, IShellItem psi, out FDEOR pResponse);
}
