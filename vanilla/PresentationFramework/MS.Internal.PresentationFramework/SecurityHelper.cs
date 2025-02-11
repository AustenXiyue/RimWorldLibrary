using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace MS.Internal.PresentationFramework;

internal static class SecurityHelper
{
	internal static Exception GetExceptionForHR(int hr)
	{
		return Marshal.GetExceptionForHR(hr, new IntPtr(-1));
	}

	internal static void ShowMessageBoxHelper(Window parent, string text, string title, MessageBoxButton buttons, MessageBoxImage image)
	{
		if (parent != null)
		{
			MessageBox.Show(parent, text, title, buttons, image);
		}
		else
		{
			MessageBox.Show(text, title, buttons, image);
		}
	}

	internal static void ShowMessageBoxHelper(nint parentHwnd, string text, string title, MessageBoxButton buttons, MessageBoxImage image)
	{
		MessageBox.ShowCore(parentHwnd, text, title, buttons, image, MessageBoxResult.None, MessageBoxOptions.None);
	}

	internal static bool AreStringTypesEqual(string m1, string m2)
	{
		return string.Equals(m1, m2, StringComparison.OrdinalIgnoreCase);
	}
}
