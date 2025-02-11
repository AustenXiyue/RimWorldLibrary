using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using MS.Internal.Documents.Application;
using MS.Internal.Utility;
using MS.Win32;

namespace MS.Internal.AppModel;

internal static class AppSecurityManager
{
	[ComImport]
	[ComVisible(false)]
	[Guid("7b8a2d94-0ac9-11d1-896c-00c04Fb6bfc4")]
	internal class InternetSecurityManager
	{
	}

	private const string RefererHeader = "Referer: ";

	private const string BrowserOpenCommandLookupKey = "htmlfile\\shell\\open\\command";

	private static readonly object _lockObj = new object();

	private static MS.Win32.UnsafeNativeMethods.IInternetSecurityManager _secMgr;

	private static SecurityMgrSite _secMgrSite;

	internal static void SafeLaunchBrowserDemandWhenUnsafe(Uri originatingUri, Uri destinationUri, bool fIsTopLevel)
	{
		if (SafeLaunchBrowserOnlyIfPossible(originatingUri, destinationUri, fIsTopLevel) == LaunchResult.NotLaunched)
		{
			UnsafeLaunchBrowser(destinationUri);
		}
	}

	internal static LaunchResult SafeLaunchBrowserOnlyIfPossible(Uri originatingUri, Uri destinationUri, bool fIsTopLevel)
	{
		return SafeLaunchBrowserOnlyIfPossible(originatingUri, destinationUri, null, fIsTopLevel);
	}

	internal static LaunchResult SafeLaunchBrowserOnlyIfPossible(Uri originatingUri, Uri destinationUri, string targetName, bool fIsTopLevel)
	{
		LaunchResult result = LaunchResult.NotLaunched;
		bool flag = (object)destinationUri.Scheme == Uri.UriSchemeHttp || (object)destinationUri.Scheme == Uri.UriSchemeHttps || destinationUri.IsFile;
		bool flag2 = string.Compare(destinationUri.Scheme, Uri.UriSchemeMailto, StringComparison.OrdinalIgnoreCase) == 0;
		if (!BrowserInteropHelper.IsInitialViewerNavigation && ((fIsTopLevel && flag) || flag2) && !flag && flag2)
		{
			MS.Win32.UnsafeNativeMethods.ShellExecute(new HandleRef(null, IntPtr.Zero), null, MS.Internal.Utility.BindUriHelper.UriToString(destinationUri), null, null, 0);
			result = LaunchResult.Launched;
		}
		return result;
	}

	internal static void UnsafeLaunchBrowser(Uri uri, string targetFrame = null)
	{
		ShellExecuteDefaultBrowser(uri);
	}

	internal static void ShellExecuteDefaultBrowser(Uri uri)
	{
		MS.Win32.UnsafeNativeMethods.ShellExecuteInfo shellExecuteInfo = new MS.Win32.UnsafeNativeMethods.ShellExecuteInfo();
		shellExecuteInfo.cbSize = Marshal.SizeOf(shellExecuteInfo);
		shellExecuteInfo.fMask = MS.Win32.UnsafeNativeMethods.ShellExecuteFlags.SEE_MASK_FLAG_DDEWAIT;
		if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
		{
			shellExecuteInfo.fMask |= MS.Win32.UnsafeNativeMethods.ShellExecuteFlags.SEE_MASK_CLASSNAME;
			shellExecuteInfo.lpClass = ".htm";
		}
		shellExecuteInfo.lpFile = uri.ToString();
		if (!MS.Win32.UnsafeNativeMethods.ShellExecuteEx(shellExecuteInfo))
		{
			throw new InvalidOperationException(SR.FailToLaunchDefaultBrowser, new Win32Exception());
		}
	}

	private static string GetHeaders(Uri destinationUri)
	{
		string text = MS.Internal.Utility.BindUriHelper.GetReferer(destinationUri);
		if (!string.IsNullOrEmpty(text))
		{
			text = "Referer: " + text + "\r\n";
		}
		return text;
	}

	private static LaunchResult CanNavigateToUrlWithZoneCheck(Uri originatingUri, Uri destinationUri)
	{
		int num = 0;
		int num2 = 3;
		bool flag = true;
		EnsureSecurityManager();
		flag = MS.Win32.UnsafeNativeMethods.CoInternetIsFeatureEnabled(1, 2) != 1;
		num = MapUrlToZone(destinationUri);
		Uri uri = null;
		if (Application.Current.MimeType != MimeType.Document)
		{
			uri = BrowserInteropHelper.Source;
		}
		else if (destinationUri.IsFile && Path.GetExtension(destinationUri.LocalPath).Equals(DocumentStream.XpsFileExtension, StringComparison.OrdinalIgnoreCase))
		{
			num = 3;
		}
		if (uri != null)
		{
			num2 = MapUrlToZone(uri);
			if ((!flag && ((num2 != 3 && num2 != 4) || num != 0)) || (flag && (num2 == num || (num2 <= 4 && num <= 4 && (num2 < num || ((num2 == 2 || num2 == 1) && (num == 2 || num == 1)))))))
			{
				return LaunchResult.Launched;
			}
			return CheckBlockNavigation(uri, destinationUri, flag);
		}
		return LaunchResult.Launched;
	}

	private static LaunchResult CheckBlockNavigation(Uri originatingUri, Uri destinationUri, bool fEnabled)
	{
		if (fEnabled)
		{
			if (MS.Win32.UnsafeNativeMethods.CoInternetIsFeatureZoneElevationEnabled(MS.Internal.Utility.BindUriHelper.UriToString(originatingUri), MS.Internal.Utility.BindUriHelper.UriToString(destinationUri), _secMgr, 2) == 1)
			{
				return LaunchResult.Launched;
			}
			if (IsZoneElevationSettingPrompt(destinationUri))
			{
				return LaunchResult.NotLaunchedDueToPrompt;
			}
			return LaunchResult.NotLaunched;
		}
		return LaunchResult.Launched;
	}

	private unsafe static bool IsZoneElevationSettingPrompt(Uri target)
	{
		Invariant.Assert(_secMgr != null);
		int num = 3;
		string pwszUrl = MS.Internal.Utility.BindUriHelper.UriToString(target);
		_secMgr.ProcessUrlAction(pwszUrl, 8449, (byte*)(&num), Marshal.SizeOf(typeof(int)), null, 0, 1, 0);
		return num == 1;
	}

	private static void EnsureSecurityManager()
	{
		if (_secMgr != null)
		{
			return;
		}
		lock (_lockObj)
		{
			if (_secMgr == null)
			{
				_secMgr = (MS.Win32.UnsafeNativeMethods.IInternetSecurityManager)new InternetSecurityManager();
				_secMgrSite = new SecurityMgrSite();
				_secMgr.SetSecuritySite(_secMgrSite);
			}
		}
	}

	internal static void ClearSecurityManager()
	{
		if (_secMgr == null)
		{
			return;
		}
		lock (_lockObj)
		{
			if (_secMgr != null)
			{
				_secMgr.SetSecuritySite(null);
				_secMgrSite = null;
				_secMgr = null;
			}
		}
	}

	internal static int MapUrlToZone(Uri url)
	{
		EnsureSecurityManager();
		_secMgr.MapUrlToZone(MS.Internal.Utility.BindUriHelper.UriToString(url), out var pdwZone, 0);
		return pdwZone;
	}
}
