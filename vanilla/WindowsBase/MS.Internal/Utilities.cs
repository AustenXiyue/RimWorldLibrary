using System;
using System.Runtime.InteropServices;
using MS.Win32;

namespace MS.Internal;

internal static class Utilities
{
	private static readonly Version _osVersion = Environment.OSVersion.Version;

	internal static bool IsOSVistaOrNewer => _osVersion >= new Version(6, 0);

	internal static bool IsOSWindows7OrNewer => _osVersion >= new Version(6, 1);

	internal static bool IsOSWindows8OrNewer => _osVersion >= new Version(6, 2);

	internal static bool IsCompositionEnabled
	{
		get
		{
			if (!IsOSVistaOrNewer)
			{
				return false;
			}
			int enabled = 0;
			UnsafeNativeMethods.HRESULT.Check(UnsafeNativeMethods.DwmIsCompositionEnabled(out enabled));
			return enabled != 0;
		}
	}

	internal static void SafeDispose<T>(ref T disposable) where T : IDisposable
	{
		IDisposable disposable2 = disposable;
		disposable = default(T);
		disposable2?.Dispose();
	}

	internal static void SafeRelease<T>(ref T comObject) where T : class
	{
		T val = comObject;
		comObject = null;
		if (val != null)
		{
			Marshal.ReleaseComObject(val);
		}
	}
}
