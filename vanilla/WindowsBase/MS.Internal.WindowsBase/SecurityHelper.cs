using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace MS.Internal.WindowsBase;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal static class SecurityHelper
{
	internal static void RunClassConstructor(Type t)
	{
		RuntimeHelpers.RunClassConstructor(t.TypeHandle);
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

	internal static object ReadRegistryValue(RegistryKey baseRegistryKey, string keyName, string valueName)
	{
		object result = null;
		RegistryKey registryKey = baseRegistryKey.OpenSubKey(keyName);
		if (registryKey != null)
		{
			using (registryKey)
			{
				result = registryKey.GetValue(valueName);
			}
		}
		return result;
	}
}
