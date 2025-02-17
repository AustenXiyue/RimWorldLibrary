using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using MonoMod.Utils;

namespace MonoMod;

internal static class Switches
{
	private delegate bool TryGetSwitchFunc(string @switch, out bool isEnabled);

	private static readonly ConcurrentDictionary<string, object?> switchValues;

	private const string Prefix = "MONOMOD_";

	public const string RunningOnWine = "RunningOnWine";

	public const string DebugClr = "DebugClr";

	public const string JitPath = "JitPath";

	public const string LogRecordHoles = "LogRecordHoles";

	public const string LogInMemory = "LogInMemory";

	public const string LogSpam = "LogSpam";

	public const string LogReplayQueueLength = "LogReplayQueueLength";

	public const string LogToFile = "LogToFile";

	public const string LogToFileFilter = "LogToFileFilter";

	public const string DMDType = "DMDType";

	public const string DMDDebug = "DMDDebug";

	public const string DMDDumpTo = "DMDDumpTo";

	private static readonly Type? tAppContext;

	private static readonly Func<string, object?> dGetData;

	private static readonly MethodInfo? miTryGetSwitch;

	private static readonly TryGetSwitchFunc? dTryGetSwitch;

	static Switches()
	{
		switchValues = new ConcurrentDictionary<string, object>();
		tAppContext = typeof(AppDomain).Assembly.GetType("System.AppContext");
		dGetData = MakeGetDataDelegate();
		miTryGetSwitch = tAppContext?.GetMethod("TryGetSwitch", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
		{
			typeof(string),
			typeof(bool).MakeByRefType()
		}, null);
		dTryGetSwitch = miTryGetSwitch?.TryCreateDelegate<TryGetSwitchFunc>();
		foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
		{
			string text = (string)environmentVariable.Key;
			if (text.StartsWith("MONOMOD_", StringComparison.Ordinal) && environmentVariable.Value != null)
			{
				string key = text.Substring("MONOMOD_".Length);
				switchValues.TryAdd(key, BestEffortParseEnvVar((string)environmentVariable.Value));
			}
		}
	}

	private static object? BestEffortParseEnvVar(string value)
	{
		if (value.Length == 0)
		{
			return null;
		}
		if (int.TryParse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var result))
		{
			return result;
		}
		if (long.TryParse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var result2))
		{
			return result2;
		}
		if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
		{
			return result;
		}
		if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result2))
		{
			return result2;
		}
		bool flag;
		switch (value[0])
		{
		case 'F':
		case 'N':
		case 'T':
		case 'Y':
		case 'f':
		case 'n':
		case 't':
		case 'y':
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			if (bool.TryParse(value, out var result3))
			{
				return result3;
			}
			if (value.Equals("yes", StringComparison.OrdinalIgnoreCase) || value.Equals("y", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (value.Equals("no", StringComparison.OrdinalIgnoreCase) || value.Equals("n", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
		}
		return value;
	}

	public static void SetSwitchValue(string @switch, object? value)
	{
		switchValues[@switch] = value;
	}

	public static void ClearSwitchValue(string @switch)
	{
		switchValues.TryRemove(@switch, out object _);
	}

	private static Func<string, object?> MakeGetDataDelegate()
	{
		Func<string, object> func = (tAppContext?.GetMethod("GetData", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(string) }, null))?.TryCreateDelegate<Func<string, object>>();
		if (func != null)
		{
			try
			{
				func("MonoMod.LogToFile");
			}
			catch
			{
				func = null;
			}
		}
		if (func == null)
		{
			func = AppDomain.CurrentDomain.GetData;
		}
		return func;
	}

	public static bool TryGetSwitchValue(string @switch, out object? value)
	{
		if (switchValues.TryGetValue(@switch, out value))
		{
			return true;
		}
		if (dGetData != null || dTryGetSwitch != null)
		{
			string text = "MonoMod." + @switch;
			object obj = dGetData?.Invoke(text);
			if (obj != null)
			{
				value = obj;
				return true;
			}
			TryGetSwitchFunc tryGetSwitchFunc = dTryGetSwitch;
			if (tryGetSwitchFunc != null && tryGetSwitchFunc(text, out var isEnabled))
			{
				value = isEnabled;
				return true;
			}
		}
		value = null;
		return false;
	}

	public static bool TryGetSwitchEnabled(string @switch, out bool isEnabled)
	{
		if (switchValues.TryGetValue(@switch, out object value) && value != null && TryProcessBoolData(value, out isEnabled))
		{
			return true;
		}
		if (dGetData != null || dTryGetSwitch != null)
		{
			string text = "MonoMod." + @switch;
			TryGetSwitchFunc tryGetSwitchFunc = dTryGetSwitch;
			if (tryGetSwitchFunc != null && tryGetSwitchFunc(text, out isEnabled))
			{
				return true;
			}
			object obj = dGetData?.Invoke(text);
			if (obj != null && TryProcessBoolData(obj, out isEnabled))
			{
				return true;
			}
		}
		isEnabled = false;
		return false;
	}

	private static bool TryProcessBoolData(object data, out bool boolVal)
	{
		if (!(data is bool flag))
		{
			if (!(data is int num))
			{
				if (!(data is long num2))
				{
					IConvertible convertible;
					if (!(data is string value))
					{
						convertible = data as IConvertible;
						if (convertible == null)
						{
							boolVal = false;
							return false;
						}
					}
					else
					{
						if (bool.TryParse(value, out boolVal))
						{
							return true;
						}
						convertible = (IConvertible)data;
					}
					IConvertible convertible2 = convertible;
					boolVal = convertible2.ToBoolean(CultureInfo.CurrentCulture);
					return true;
				}
				long num3 = num2;
				boolVal = num3 != 0;
				return true;
			}
			int num4 = num;
			boolVal = num4 != 0;
			return true;
		}
		bool flag2 = flag;
		boolVal = flag2;
		return true;
	}
}
