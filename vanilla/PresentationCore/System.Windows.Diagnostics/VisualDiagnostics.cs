using System.Diagnostics;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Diagnostics;

public static class VisualDiagnostics
{
	private static class EnableHelper
	{
		private static readonly bool s_IsDevMode;

		private static readonly bool? s_IsEnableVisualTreeChangedAllowed;

		private static bool s_IsVisualTreeChangedEnabled;

		private static bool? s_AllowChangesDuringVisualTreeChanged;

		private const string c_enableVisualTreeNotificationsEnvironmentVariable = "ENABLE_XAML_DIAGNOSTICS_VISUAL_TREE_NOTIFICATIONS";

		private const string c_devmodeRegKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock";

		private const string c_devmodeRegKeyFullPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock";

		private const string c_devmodeValueName = "AllowDevelopmentWithoutDevLicense";

		internal static bool IsVisualTreeChangeEnabled
		{
			get
			{
				if (IsEnabled)
				{
					if (!s_IsVisualTreeChangedEnabled && !Debugger.IsAttached)
					{
						return s_isDebuggerCheckDisabledForTestPurposes;
					}
					return true;
				}
				return false;
			}
		}

		private static bool IsDevMode => s_IsDevMode;

		private static bool IsEnableVisualTreeChangedAllowed => s_IsEnableVisualTreeChangedAllowed ?? Debugger.IsAttached;

		static EnableHelper()
		{
			if (IsEnabled)
			{
				s_IsDevMode = GetDevModeFromRegistry();
				s_IsEnableVisualTreeChangedAllowed = PrecomputeIsEnableVisualTreeChangedAllowed();
			}
		}

		internal static void EnableVisualTreeChanged()
		{
			if (!IsEnableVisualTreeChangedAllowed)
			{
				throw new InvalidOperationException(SR.Format(SR.MethodCallNotAllowed, "EnableVisualTreeChanged"));
			}
			s_IsVisualTreeChangedEnabled = true;
		}

		internal static void DisableVisualTreeChanged()
		{
			s_IsVisualTreeChangedEnabled = false;
		}

		internal static bool AllowChangesDuringVisualTreeChanged(DependencyObject d)
		{
			if (!s_AllowChangesDuringVisualTreeChanged.HasValue)
			{
				if (IsChangePermitted(d))
				{
					return true;
				}
				s_AllowChangesDuringVisualTreeChanged = CoreAppContextSwitches.AllowChangesDuringVisualTreeChanged;
				_ = s_AllowChangesDuringVisualTreeChanged == true;
				return s_AllowChangesDuringVisualTreeChanged == true;
			}
			if (s_AllowChangesDuringVisualTreeChanged != true)
			{
				return IsChangePermitted(d);
			}
			return true;
		}

		private static bool IsChangePermitted(DependencyObject d)
		{
			if (s_ActiveHwndSource != null && d != null)
			{
				return s_ActiveHwndSource != PresentationSource.FromDependencyObject(d);
			}
			return false;
		}

		private static bool? PrecomputeIsEnableVisualTreeChangedAllowed()
		{
			if (!IsEnabled)
			{
				return false;
			}
			if (IsDevMode)
			{
				return true;
			}
			if (IsEnvironmentVariableSet(null, "ENABLE_XAML_DIAGNOSTICS_VISUAL_TREE_NOTIFICATIONS"))
			{
				return true;
			}
			return null;
		}

		private static bool GetDevModeFromRegistry()
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock");
			if (registryKey != null)
			{
				using (registryKey)
				{
					object value = registryKey.GetValue("AllowDevelopmentWithoutDevLicense");
					if (value is int)
					{
						return (int)value != 0;
					}
				}
			}
			return false;
		}
	}

	private static bool s_isDebuggerCheckDisabledForTestPurposes;

	private static readonly bool s_IsEnabled;

	private static bool s_HasVisualTreeChangedListeners;

	[ThreadStatic]
	private static bool s_IsVisualTreeChangedInProgress;

	[ThreadStatic]
	private static HwndSource s_ActiveHwndSource;

	internal static bool IsEnabled => s_IsEnabled;

	private static event EventHandler<VisualTreeChangeEventArgs> s_visualTreeChanged;

	public static event EventHandler<VisualTreeChangeEventArgs> VisualTreeChanged
	{
		add
		{
			if (EnableHelper.IsVisualTreeChangeEnabled)
			{
				s_visualTreeChanged += value;
				s_HasVisualTreeChangedListeners = true;
			}
		}
		remove
		{
			s_visualTreeChanged -= value;
		}
	}

	static VisualDiagnostics()
	{
		s_IsEnabled = !CoreAppContextSwitches.DisableDiagnostics;
	}

	public static void EnableVisualTreeChanged()
	{
		EnableHelper.EnableVisualTreeChanged();
	}

	public static void DisableVisualTreeChanged()
	{
		EnableHelper.DisableVisualTreeChanged();
	}

	public static XamlSourceInfo GetXamlSourceInfo(object obj)
	{
		return XamlSourceInfoHelper.GetXamlSourceInfo(obj);
	}

	internal static void OnVisualChildChanged(DependencyObject parent, DependencyObject child, bool isAdded)
	{
		EventHandler<VisualTreeChangeEventArgs> eventHandler = VisualDiagnostics.s_visualTreeChanged;
		if (eventHandler != null && EnableHelper.IsVisualTreeChangeEnabled)
		{
			int num;
			VisualTreeChangeType visualTreeChangeType;
			if (isAdded)
			{
				num = GetChildIndex(parent, child);
				visualTreeChangeType = VisualTreeChangeType.Add;
			}
			else
			{
				num = -1;
				visualTreeChangeType = VisualTreeChangeType.Remove;
			}
			RaiseVisualTreeChangedEvent(eventHandler, new VisualTreeChangeEventArgs(parent, child, num, visualTreeChangeType), visualTreeChangeType == VisualTreeChangeType.Add && num == 0 && VisualTreeHelper.GetParent(parent) == null);
		}
	}

	private static void RaiseVisualTreeChangedEvent(EventHandler<VisualTreeChangeEventArgs> visualTreeChanged, VisualTreeChangeEventArgs args, bool isPotentialOuterChange)
	{
		bool flag = s_IsVisualTreeChangedInProgress;
		HwndSource hwndSource = s_ActiveHwndSource;
		try
		{
			s_IsVisualTreeChangedInProgress = true;
			if (isPotentialOuterChange)
			{
				s_ActiveHwndSource = PresentationSource.FromDependencyObject(args.Parent) as HwndSource;
			}
			visualTreeChanged(null, args);
		}
		finally
		{
			s_IsVisualTreeChangedInProgress = flag;
			s_ActiveHwndSource = hwndSource;
		}
	}

	private static int GetChildIndex(DependencyObject parent, DependencyObject child)
	{
		int num = -1;
		if (child is Visual visual)
		{
			num = visual._parentIndex;
		}
		else if (child is Visual3D visual3D)
		{
			num = visual3D.ParentIndex;
		}
		if (num < 0)
		{
			int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childrenCount; i++)
			{
				if (VisualTreeHelper.GetChild(parent, i) == child)
				{
					num = i;
					break;
				}
			}
		}
		return num;
	}

	internal static void VerifyVisualTreeChange(DependencyObject d)
	{
		if (s_HasVisualTreeChangedListeners)
		{
			VerifyVisualTreeChangeCore(d);
		}
	}

	private static void VerifyVisualTreeChangeCore(DependencyObject d)
	{
		if (s_IsVisualTreeChangedInProgress && !EnableHelper.AllowChangesDuringVisualTreeChanged(d))
		{
			throw new InvalidOperationException(SR.Format(SR.ReentrantVisualTreeChangeError, "VisualTreeChanged"));
		}
	}

	internal static bool IsEnvironmentVariableSet(string value, string environmentVariable)
	{
		if (value != null)
		{
			return IsEnvironmentValueSet(value);
		}
		value = Environment.GetEnvironmentVariable(environmentVariable);
		return IsEnvironmentValueSet(value);
	}

	internal static bool IsEnvironmentValueSet(string value)
	{
		value = (value ?? string.Empty).Trim().ToLowerInvariant();
		if (!(value == string.Empty) && !(value == "0"))
		{
			return !(value == "false");
		}
		return false;
	}
}
