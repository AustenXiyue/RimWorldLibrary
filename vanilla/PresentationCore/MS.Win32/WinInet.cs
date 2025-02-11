using System;
using System.Runtime.InteropServices;

namespace MS.Win32;

internal static class WinInet
{
	internal static Uri InternetCacheFolder
	{
		get
		{
			MS.Win32.NativeMethods.InternetCacheConfigInfo pInternetCacheConfigInfo = default(MS.Win32.NativeMethods.InternetCacheConfigInfo);
			pInternetCacheConfigInfo.CachePath = new string(new char[260]);
			uint cbCacheConfigInfo = (pInternetCacheConfigInfo.dwStructSize = (uint)Marshal.SizeOf(pInternetCacheConfigInfo));
			if (!MS.Win32.UnsafeNativeMethods.GetUrlCacheConfigInfo(ref pInternetCacheConfigInfo, ref cbCacheConfigInfo, 260u))
			{
				int hRForLastWin32Error = Marshal.GetHRForLastWin32Error();
				if (hRForLastWin32Error != 0)
				{
					Marshal.ThrowExceptionForHR(hRForLastWin32Error);
				}
			}
			return new Uri(pInternetCacheConfigInfo.CachePath);
		}
	}
}
