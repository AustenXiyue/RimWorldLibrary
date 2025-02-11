using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace MS.Internal;

internal static class SecurityHelper
{
	internal static Uri GetBaseDirectory(AppDomain domain)
	{
		return new Uri(domain.BaseDirectory);
	}

	internal static int MapUrlToZoneWrapper(Uri uri)
	{
		int pdwZone = 0;
		int num = 0;
		object ppISecurityManager = null;
		num = MS.Win32.UnsafeNativeMethods.CoInternetCreateSecurityManager(null, out ppISecurityManager, 0);
		if (MS.Win32.NativeMethods.Failed(num))
		{
			throw new Win32Exception(num);
		}
		MS.Win32.UnsafeNativeMethods.IInternetSecurityManager internetSecurityManager = (MS.Win32.UnsafeNativeMethods.IInternetSecurityManager)ppISecurityManager;
		string pwszUrl = BindUriHelper.UriToString(uri);
		if (uri.IsFile)
		{
			internetSecurityManager.MapUrlToZone(pwszUrl, out pdwZone, 1);
		}
		else
		{
			internetSecurityManager.MapUrlToZone(pwszUrl, out pdwZone, 0);
		}
		if (pdwZone < 0)
		{
			throw new SecurityException(SR.Invalid_URI);
		}
		internetSecurityManager = null;
		ppISecurityManager = null;
		return pdwZone;
	}

	internal static Exception GetExceptionForHR(int hr)
	{
		return Marshal.GetExceptionForHR(hr, new IntPtr(-1));
	}

	internal static void ThrowExceptionForHR(int hr)
	{
		Marshal.ThrowExceptionForHR(hr, new IntPtr(-1));
	}

	internal static int GetHRForException(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		int hRForException = Marshal.GetHRForException(exception);
		Marshal.GetHRForException(new Exception());
		return hRForException;
	}

	internal static bool AreStringTypesEqual(string m1, string m2)
	{
		return string.Equals(m1, m2, StringComparison.OrdinalIgnoreCase);
	}
}
