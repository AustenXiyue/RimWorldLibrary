using System;
using System.Text;
using MS.Internal.Interop;

namespace MS.Internal.AppModel;

internal static class ShellUtil
{
	public static string GetPathFromShellItem(IShellItem item)
	{
		return item.GetDisplayName(SIGDN.DESKTOPABSOLUTEPARSING);
	}

	public static string GetPathForKnownFolder(Guid knownFolder)
	{
		if (knownFolder == default(Guid))
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder(260);
		if (!NativeMethods2.SHGetFolderPathEx(ref knownFolder, KF_FLAG.DEFAULT, IntPtr.Zero, stringBuilder, (uint)stringBuilder.Capacity).Succeeded)
		{
			return null;
		}
		return stringBuilder.ToString();
	}

	public static IShellItem2 GetShellItemForPath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}
		Guid riid = new Guid("7e9fb0d3-919f-4307-ab2e-9b1860310c93");
		object ppv;
		MS.Internal.Interop.HRESULT hRESULT = NativeMethods2.SHCreateItemFromParsingName(path, null, ref riid, out ppv);
		if (hRESULT == (MS.Internal.Interop.HRESULT)Win32Error.ERROR_FILE_NOT_FOUND || hRESULT == (MS.Internal.Interop.HRESULT)Win32Error.ERROR_PATH_NOT_FOUND)
		{
			hRESULT = MS.Internal.Interop.HRESULT.S_OK;
			ppv = null;
		}
		hRESULT.ThrowIfFailed();
		return (IShellItem2)ppv;
	}
}
