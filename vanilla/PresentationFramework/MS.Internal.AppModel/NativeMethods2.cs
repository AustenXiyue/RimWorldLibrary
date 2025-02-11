using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using MS.Internal.Interop;

namespace MS.Internal.AppModel;

internal static class NativeMethods2
{
	[DllImport("shell32.dll", EntryPoint = "SHAddToRecentDocs")]
	private static extern void SHAddToRecentDocsString(SHARD uFlags, [MarshalAs(UnmanagedType.LPWStr)] string pv);

	[DllImport("shell32.dll", EntryPoint = "SHAddToRecentDocs")]
	private static extern void SHAddToRecentDocs_ShellLink(SHARD uFlags, IShellLinkW pv);

	internal static void SHAddToRecentDocs(string path)
	{
		SHAddToRecentDocsString(SHARD.PATHW, path);
	}

	internal static void SHAddToRecentDocs(IShellLinkW shellLink)
	{
		SHAddToRecentDocs_ShellLink(SHARD.LINK, shellLink);
	}

	[DllImport("shell32.dll")]
	internal static extern MS.Internal.Interop.HRESULT SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IBindCtx pbc, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);

	[DllImport("shell32.dll")]
	internal static extern MS.Internal.Interop.HRESULT SHGetFolderPathEx([In] ref Guid rfid, KF_FLAG dwFlags, [Optional][In] nint hToken, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszPath, uint cchPath);

	[DllImport("shell32.dll", PreserveSig = false)]
	internal static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

	[DllImport("shell32.dll")]
	internal static extern MS.Internal.Interop.HRESULT GetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] out string AppID);
}
