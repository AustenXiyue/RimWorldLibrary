using System.Runtime.CompilerServices;

namespace System;

internal static class LocalAppContextSwitches
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool GetCachedSwitchValue(string switchName, ref int cachedSwitchValue)
	{
		if (cachedSwitchValue < 0)
		{
			return false;
		}
		if (cachedSwitchValue > 0)
		{
			return true;
		}
		return GetCachedSwitchValueInternal(switchName, ref cachedSwitchValue);
	}

	private static bool GetCachedSwitchValueInternal(string switchName, ref int cachedSwitchValue)
	{
		if (!AppContext.TryGetSwitch(switchName, out var isEnabled))
		{
			isEnabled = GetSwitchDefaultValue(switchName);
		}
		AppContext.TryGetSwitch("TestSwitch.LocalAppContext.DisableCaching", out var isEnabled2);
		if (!isEnabled2)
		{
			cachedSwitchValue = (isEnabled ? 1 : (-1));
		}
		return isEnabled;
	}

	private static bool GetSwitchDefaultValue(string switchName)
	{
		return switchName switch
		{
			"Switch.System.Runtime.Serialization.SerializationGuard" => true, 
			"System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization" => true, 
			"System.Xml.XmlResolver.IsNetworkingEnabledByDefault" => true, 
			_ => false, 
		};
	}
}
