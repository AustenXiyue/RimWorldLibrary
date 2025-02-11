using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace MS.Internal.AppModel;

internal static class CookieHandler
{
	internal static void HandleWebRequest(WebRequest request)
	{
		if (!(request is HttpWebRequest httpWebRequest))
		{
			return;
		}
		try
		{
			string cookie = GetCookie(httpWebRequest.RequestUri, throwIfNoCookie: false);
			if (!string.IsNullOrEmpty(cookie))
			{
				if (httpWebRequest.CookieContainer == null)
				{
					httpWebRequest.CookieContainer = new CookieContainer();
				}
				httpWebRequest.CookieContainer.SetCookies(httpWebRequest.RequestUri, cookie.Replace(';', ','));
			}
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
		}
	}

	internal static void HandleWebResponse(WebResponse response)
	{
		if (!(response is HttpWebResponse httpWebResponse))
		{
			return;
		}
		WebHeaderCollection headers = httpWebResponse.Headers;
		for (int num = headers.Count - 1; num >= 0; num--)
		{
			if (string.Equals(headers.Keys[num], "Set-Cookie", StringComparison.OrdinalIgnoreCase))
			{
				string p3pHeader = httpWebResponse.Headers["P3P"];
				string[] values = headers.GetValues(num);
				foreach (string cookieData in values)
				{
					try
					{
						SetCookieUnsafe(httpWebResponse.ResponseUri, cookieData, p3pHeader);
					}
					catch (Exception ex)
					{
						if (CriticalExceptions.IsCriticalException(ex))
						{
							throw;
						}
					}
				}
				break;
			}
		}
	}

	[FriendAccessAllowed]
	internal static string GetCookie(Uri uri, bool throwIfNoCookie)
	{
		uint pchCookieData = 0u;
		string url = BindUriHelper.UriToString(uri);
		if (MS.Win32.UnsafeNativeMethods.InternetGetCookieEx(url, null, null, ref pchCookieData, 0u, IntPtr.Zero))
		{
			pchCookieData++;
			StringBuilder stringBuilder = new StringBuilder((int)pchCookieData);
			if (MS.Win32.UnsafeNativeMethods.InternetGetCookieEx(url, null, stringBuilder, ref pchCookieData, 0u, IntPtr.Zero))
			{
				return stringBuilder.ToString();
			}
		}
		if (!throwIfNoCookie && Marshal.GetLastWin32Error() == 259)
		{
			return null;
		}
		throw new Win32Exception();
	}

	[FriendAccessAllowed]
	internal static bool SetCookie(Uri uri, string cookieData)
	{
		return SetCookieUnsafe(uri, cookieData, null);
	}

	private static bool SetCookieUnsafe(Uri uri, string cookieData, string p3pHeader)
	{
		uint num = MS.Win32.UnsafeNativeMethods.InternetSetCookieEx(BindUriHelper.UriToString(uri), null, cookieData, 64u, p3pHeader);
		if (num == 0)
		{
			throw new Win32Exception();
		}
		return num != 5;
	}
}
