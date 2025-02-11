using System;
using System.Runtime.InteropServices;

namespace MS.Win32;

internal static class LoadLibraryHelper
{
	private static bool IsKnowledgeBase2533623OrGreater()
	{
		bool result = false;
		nint hModule = IntPtr.Zero;
		if (MS.Win32.UnsafeNativeMethods.GetModuleHandleEx(MS.Win32.UnsafeNativeMethods.GetModuleHandleFlags.None, "kernel32.dll", out hModule) && hModule != IntPtr.Zero)
		{
			try
			{
				result = MS.Win32.UnsafeNativeMethods.GetProcAddressNoThrow(new HandleRef(null, hModule), "AddDllDirectoryName") != IntPtr.Zero;
			}
			finally
			{
				MS.Win32.UnsafeNativeMethods.FreeLibrary(hModule);
			}
		}
		return result;
	}

	internal static nint SecureLoadLibraryEx(string lpFileName, nint hFile, MS.Win32.UnsafeNativeMethods.LoadLibraryFlags dwFlags)
	{
		if (!IsKnowledgeBase2533623OrGreater() && (dwFlags & MS.Win32.UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_APPLICATION_DIR & MS.Win32.UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS & MS.Win32.UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR & MS.Win32.UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_SYSTEM32 & MS.Win32.UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_USER_DIRS) != 0)
		{
			dwFlags = (MS.Win32.UnsafeNativeMethods.LoadLibraryFlags)((uint)dwFlags & 0xFFFFE0FFu);
		}
		return MS.Win32.UnsafeNativeMethods.LoadLibraryEx(lpFileName, hFile, dwFlags);
	}
}
