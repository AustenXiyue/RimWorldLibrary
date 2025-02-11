using System;
using System.Windows;
using MS.Win32;

namespace MS.Internal;

internal class SecurityMgrSite : MS.Win32.NativeMethods.IInternetSecurityMgrSite
{
	internal SecurityMgrSite()
	{
	}

	public void GetWindow(ref nint phwnd)
	{
		phwnd = IntPtr.Zero;
		if (Application.Current != null)
		{
			Window mainWindow = Application.Current.MainWindow;
			if (mainWindow != null)
			{
				phwnd = mainWindow.CriticalHandle;
			}
		}
	}

	public void EnableModeless(bool fEnable)
	{
	}
}
