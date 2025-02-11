using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;
using MS.Internal.WindowsBase;

namespace System.Windows;

internal static class AccessibilitySwitches
{
	private const int EventId = 1023;

	private const string EventSource = ".NET Runtime";

	private static int s_DefaultsSet;

	private static int s_SwitchesVerified;

	internal const string UseLegacyAccessibilityFeaturesSwitchName = "Switch.UseLegacyAccessibilityFeatures";

	private static int _useLegacyAccessibilityFeatures;

	internal const string UseLegacyAccessibilityFeatures2SwitchName = "Switch.UseLegacyAccessibilityFeatures.2";

	private static int _useLegacyAccessibilityFeatures2;

	internal const string UseLegacyAccessibilityFeatures3SwitchName = "Switch.UseLegacyAccessibilityFeatures.3";

	private static int _useLegacyAccessibilityFeatures3;

	internal const string UseLegacyToolTipDisplaySwitchName = "Switch.UseLegacyToolTipDisplay";

	private static int _UseLegacyToolTipDisplay;

	internal const string ItemsControlDoesNotSupportAutomationSwitchName = "Switch.System.Windows.Controls.ItemsControlDoesNotSupportAutomation";

	private static int _ItemsControlDoesNotSupportAutomation;

	public static bool UseNetFx47CompatibleAccessibilityFeatures
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.UseLegacyAccessibilityFeatures", ref _useLegacyAccessibilityFeatures);
		}
	}

	public static bool UseNetFx471CompatibleAccessibilityFeatures
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.UseLegacyAccessibilityFeatures.2", ref _useLegacyAccessibilityFeatures2);
		}
	}

	public static bool UseNetFx472CompatibleAccessibilityFeatures
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.UseLegacyAccessibilityFeatures.3", ref _useLegacyAccessibilityFeatures3);
		}
	}

	public static bool UseLegacyToolTipDisplay
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.UseLegacyToolTipDisplay", ref _UseLegacyToolTipDisplay);
		}
	}

	public static bool ItemsControlDoesNotSupportAutomation
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Controls.ItemsControlDoesNotSupportAutomation", ref _ItemsControlDoesNotSupportAutomation);
		}
	}

	internal static void SetSwitchDefaults(string platformIdentifier, int targetFrameworkVersion)
	{
		if (!(platformIdentifier == ".NETFramework"))
		{
			if (platformIdentifier == ".NETCoreApp")
			{
				LocalAppContext.DefineSwitchDefault("Switch.UseLegacyAccessibilityFeatures", initialValue: false);
				LocalAppContext.DefineSwitchDefault("Switch.UseLegacyAccessibilityFeatures.2", initialValue: false);
				LocalAppContext.DefineSwitchDefault("Switch.UseLegacyAccessibilityFeatures.3", initialValue: false);
				LocalAppContext.DefineSwitchDefault("Switch.UseLegacyToolTipDisplay", initialValue: false);
				LocalAppContext.DefineSwitchDefault("Switch.System.Windows.Controls.ItemsControlDoesNotSupportAutomation", initialValue: false);
			}
		}
		else if (Interlocked.CompareExchange(ref s_DefaultsSet, 1, 0) == 0)
		{
			if (targetFrameworkVersion <= 40700)
			{
				LocalAppContext.DefineSwitchDefault("Switch.UseLegacyAccessibilityFeatures", initialValue: true);
			}
			if (targetFrameworkVersion <= 40701)
			{
				LocalAppContext.DefineSwitchDefault("Switch.UseLegacyAccessibilityFeatures.2", initialValue: true);
			}
			if (targetFrameworkVersion <= 40702)
			{
				LocalAppContext.DefineSwitchDefault("Switch.UseLegacyAccessibilityFeatures.3", initialValue: true);
				LocalAppContext.DefineSwitchDefault("Switch.UseLegacyToolTipDisplay", initialValue: true);
				LocalAppContext.DefineSwitchDefault("Switch.System.Windows.Controls.ItemsControlDoesNotSupportAutomation", initialValue: true);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void VerifySwitches(Dispatcher dispatcher)
	{
		if (Interlocked.CompareExchange(ref s_SwitchesVerified, 1, 0) != 0)
		{
			return;
		}
		IEnumerable<bool> enumerable = from x in (from x in typeof(AccessibilitySwitches).GetProperties(BindingFlags.Static | BindingFlags.Public)
				where x.Name.EndsWith("CompatibleAccessibilityFeatures")
				select x).OrderBy<PropertyInfo, string>((PropertyInfo x) => x.Name.Remove(x.Name.IndexOf("CompatibleAccessibilityFeatures", 0)), StringComparer.OrdinalIgnoreCase)
			select (bool)x.GetValue(null);
		bool? flag = null;
		bool flag2 = false;
		foreach (bool item in enumerable)
		{
			if (flag2 = !item && flag == true)
			{
				break;
			}
			flag = item;
		}
		if (flag2)
		{
			DispatchOnError(dispatcher, SR.CombinationOfAccessibilitySwitchesNotSupported);
		}
		VerifyDependencies(dispatcher);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void VerifyDependencies(Dispatcher dispatcher)
	{
		if (!UseLegacyToolTipDisplay && UseNetFx472CompatibleAccessibilityFeatures)
		{
			DispatchOnError(dispatcher, string.Format(SR.AccessibilitySwitchDependencyNotSatisfied, "Switch.UseLegacyToolTipDisplay", "Switch.UseLegacyAccessibilityFeatures.3", 3));
		}
		if (!ItemsControlDoesNotSupportAutomation && UseNetFx472CompatibleAccessibilityFeatures)
		{
			DispatchOnError(dispatcher, string.Format(SR.AccessibilitySwitchDependencyNotSatisfied, "Switch.System.Windows.Controls.ItemsControlDoesNotSupportAutomation", "Switch.UseLegacyAccessibilityFeatures.3", 3));
		}
	}

	private static void DispatchOnError(Dispatcher dispatcher, string message)
	{
		dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)delegate
		{
			WriteEventAndThrow(message);
		});
	}

	private static void WriteEventAndThrow(string message)
	{
		NotSupportedException ex = new NotSupportedException(message);
		if (EventLog.SourceExists(".NET Runtime"))
		{
			EventLog.WriteEntry(".NET Runtime", Process.GetCurrentProcess().ProcessName + Environment.NewLine + ex.ToString(), EventLogEntryType.Error, 1023);
		}
		throw ex;
	}
}
