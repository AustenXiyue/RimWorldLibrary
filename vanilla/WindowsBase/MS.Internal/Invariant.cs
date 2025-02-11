using System;
using System.Diagnostics;
using Microsoft.Win32;
using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal static class Invariant
{
	private static bool _strict;

	private const bool _strictDefaultValue = false;

	internal static bool Strict
	{
		get
		{
			return _strict;
		}
		set
		{
			_strict = value;
		}
	}

	private static bool IsDialogOverrideEnabled
	{
		get
		{
			bool flag = false;
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\.NETFramework");
			if (registryKey != null)
			{
				object value = registryKey.GetValue("DbgJITDebugLaunchSetting");
				string text = registryKey.GetValue("DbgManagedDebugger") as string;
				flag = value is int && ((int)value & 2) != 0;
				if (flag)
				{
					flag = text != null && text.Length > 0;
				}
			}
			return flag;
		}
	}

	static Invariant()
	{
		_strict = false;
	}

	internal static void Assert(bool condition)
	{
		if (!condition)
		{
			FailFast(null, null);
		}
	}

	internal static void Assert(bool condition, string invariantMessage)
	{
		if (!condition)
		{
			FailFast(invariantMessage, null);
		}
	}

	internal static void Assert(bool condition, string invariantMessage, string detailMessage)
	{
		if (!condition)
		{
			FailFast(invariantMessage, detailMessage);
		}
	}

	private static void FailFast(string message, string detailMessage)
	{
		if (IsDialogOverrideEnabled)
		{
			Debugger.Break();
		}
		Environment.FailFast(SR.InvariantFailure);
	}
}
