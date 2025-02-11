using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System;

internal class LocalAppContext
{
	private static Dictionary<string, bool> s_switchMap;

	private static readonly object s_syncLock;

	private static bool DisableCaching { get; set; }

	static LocalAppContext()
	{
		s_switchMap = new Dictionary<string, bool>();
		s_syncLock = new object();
		AppContextDefaultValues.PopulateDefaultValues();
		DisableCaching = IsSwitchEnabled("TestSwitch.LocalAppContext.DisableCaching");
	}

	public static bool IsSwitchEnabled(string switchName)
	{
		if (AppContext.TryGetSwitch(switchName, out var isEnabled))
		{
			return isEnabled;
		}
		return IsSwitchEnabledLocal(switchName);
	}

	private static bool IsSwitchEnabledLocal(string switchName)
	{
		bool flag;
		bool value;
		lock (s_switchMap)
		{
			flag = s_switchMap.TryGetValue(switchName, out value);
		}
		if (flag)
		{
			return value;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool GetCachedSwitchValue(string switchName, ref int switchValue)
	{
		if (switchValue < 0)
		{
			return false;
		}
		if (switchValue > 0)
		{
			return true;
		}
		return GetCachedSwitchValueInternal(switchName, ref switchValue);
	}

	private static bool GetCachedSwitchValueInternal(string switchName, ref int switchValue)
	{
		if (DisableCaching)
		{
			return IsSwitchEnabled(switchName);
		}
		bool flag = IsSwitchEnabled(switchName);
		switchValue = (flag ? 1 : (-1));
		return flag;
	}

	internal static void DefineSwitchDefault(string switchName, bool initialValue)
	{
		s_switchMap[switchName] = initialValue;
	}
}
