using System;
using System.Net;
using System.Runtime.InteropServices;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace MS.Internal.AppModel;

[FriendAccessAllowed]
internal class CustomCredentialPolicy : ICredentialPolicy
{
	[ComImport]
	[ComVisible(false)]
	[Guid("7b8a2d94-0ac9-11d1-896c-00c04Fb6bfc4")]
	private class InternetSecurityManager
	{
	}

	private static MS.Win32.UnsafeNativeMethods.IInternetSecurityManager _securityManager;

	private static object _lockObj;

	private static bool _initialized;

	static CustomCredentialPolicy()
	{
		_lockObj = new object();
		_initialized = false;
	}

	internal static void EnsureCustomCredentialPolicy()
	{
		if (_initialized)
		{
			return;
		}
		lock (_lockObj)
		{
			if (!_initialized)
			{
				if (AuthenticationManager.CredentialPolicy == null)
				{
					AuthenticationManager.CredentialPolicy = new CustomCredentialPolicy();
				}
				_initialized = true;
			}
		}
	}

	public bool ShouldSendCredential(Uri challengeUri, WebRequest request, NetworkCredential credential, IAuthenticationModule authenticationModule)
	{
		switch (MapUrlToZone(challengeUri))
		{
		case 0:
		case 1:
		case 2:
			return true;
		default:
			return !IsDefaultCredentials(credential);
		}
	}

	private bool IsDefaultCredentials(NetworkCredential credential)
	{
		return credential == CredentialCache.DefaultCredentials;
	}

	internal static int MapUrlToZone(Uri uri)
	{
		EnsureSecurityManager();
		_securityManager.MapUrlToZone(BindUriHelper.UriToString(uri), out var pdwZone, 0);
		return pdwZone;
	}

	private static void EnsureSecurityManager()
	{
		if (_securityManager != null)
		{
			return;
		}
		lock (_lockObj)
		{
			if (_securityManager == null)
			{
				_securityManager = (MS.Win32.UnsafeNativeMethods.IInternetSecurityManager)new InternetSecurityManager();
			}
		}
	}
}
