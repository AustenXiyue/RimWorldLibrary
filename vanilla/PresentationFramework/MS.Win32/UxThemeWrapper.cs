using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using MS.Internal;

namespace MS.Win32;

internal static class UxThemeWrapper
{
	private class ThemeState
	{
		private bool _isActive;

		private string _themeName;

		private string _themeColor;

		public bool IsActive => _isActive;

		public string ThemeName => _themeName;

		public string ThemeColor => _themeColor;

		public ThemeState(bool isActive, string name, string color)
		{
			_isActive = isActive;
			_themeName = name;
			_themeColor = color;
		}
	}

	private static ThemeState _themeState;

	private static bool _isActive;

	private static string _themeName;

	private static string _themeColor;

	internal static bool IsActive => IsActiveCompatWrapper;

	internal static string ThemeName => ThemeNameCompatWrapper;

	internal static string ThemeColor => ThemeColorCompatWrapper;

	internal static string ThemedResourceName => ThemedResourceNameCompatWrapper;

	private static bool IsAppSupported => _themeName == null;

	private static bool IsActiveCompatWrapper
	{
		get
		{
			if (!IsAppSupported)
			{
				return _isActive;
			}
			return _themeState.IsActive;
		}
	}

	private static string ThemeNameCompatWrapper
	{
		get
		{
			if (IsAppSupported)
			{
				ThemeState themeState = EnsureThemeState(themeChanged: false);
				if (themeState.IsActive)
				{
					return themeState.ThemeName;
				}
				return "classic";
			}
			return _themeName;
		}
	}

	private static string ThemeColorCompatWrapper
	{
		get
		{
			if (IsAppSupported)
			{
				return EnsureThemeState(themeChanged: false).ThemeColor;
			}
			return _themeColor;
		}
	}

	private static string ThemedResourceNameCompatWrapper
	{
		get
		{
			if (IsAppSupported)
			{
				ThemeState themeState = EnsureThemeState(themeChanged: false);
				if (themeState.IsActive)
				{
					return "themes/" + themeState.ThemeName.ToLowerInvariant() + "." + themeState.ThemeColor.ToLowerInvariant();
				}
				return "themes/classic";
			}
			if (_isActive)
			{
				return "themes/" + _themeName.ToLowerInvariant() + "." + _themeColor.ToLowerInvariant();
			}
			return "themes/classic";
		}
	}

	static UxThemeWrapper()
	{
		_themeState = new ThemeState(!SystemParameters.HighContrast && SafeNativeMethods.IsUxThemeActive(), null, null);
	}

	private static ThemeState EnsureThemeState(bool themeChanged)
	{
		ThemeState themeState = _themeState;
		bool flag = !themeChanged;
		bool flag2 = true;
		while (flag2)
		{
			int num;
			string themeName;
			string themeColor;
			if (themeChanged)
			{
				if (!SystemParameters.HighContrast)
				{
					num = (SafeNativeMethods.IsUxThemeActive() ? 1 : 0);
					if (num != 0 && (flag || themeState.ThemeName != null))
					{
						GetThemeNameAndColor(out themeName, out themeColor);
						goto IL_0042;
					}
				}
				else
				{
					num = 0;
				}
				themeName = (themeColor = null);
				goto IL_0042;
			}
			ThemeState themeState2;
			if (themeState.IsActive && themeState.ThemeName == null)
			{
				GetThemeNameAndColor(out themeName, out themeColor);
				themeState2 = new ThemeState(themeState.IsActive, themeName, themeColor);
			}
			else
			{
				themeState2 = themeState;
				flag2 = false;
			}
			goto IL_007d;
			IL_007d:
			if (flag2)
			{
				ThemeState themeState3 = Interlocked.CompareExchange(ref _themeState, themeState2, themeState);
				if (themeState3 == themeState)
				{
					themeState = themeState2;
					flag2 = false;
				}
				else if (themeState3.IsActive == themeState2.IsActive && (!themeState2.IsActive || themeState2.ThemeName == null || themeState3.ThemeName != null))
				{
					themeState = themeState3;
					flag2 = false;
				}
				else
				{
					themeChanged = true;
					themeState = themeState3;
				}
			}
			continue;
			IL_0042:
			themeState2 = new ThemeState((byte)num != 0, themeName, themeColor);
			goto IL_007d;
		}
		return themeState;
	}

	private static void GetThemeNameAndColor(out string themeName, out string themeColor)
	{
		StringBuilder stringBuilder = new StringBuilder(260);
		StringBuilder stringBuilder2 = new StringBuilder(260);
		if (MS.Win32.UnsafeNativeMethods.GetCurrentThemeName(stringBuilder, stringBuilder.Capacity, stringBuilder2, stringBuilder2.Capacity, null, 0) == 0)
		{
			themeName = stringBuilder.ToString();
			themeName = Path.GetFileNameWithoutExtension(themeName);
			if (string.Compare(themeName, "aero", StringComparison.OrdinalIgnoreCase) == 0 && Utilities.IsOSWindows8OrNewer)
			{
				themeName = "Aero2";
			}
			themeColor = stringBuilder2.ToString();
		}
		else
		{
			themeName = (themeColor = string.Empty);
		}
	}

	internal static void OnThemeChanged()
	{
		RestoreSupportedState();
		EnsureThemeState(themeChanged: true);
	}

	private static void RestoreSupportedState()
	{
		_isActive = false;
		_themeName = null;
		_themeColor = null;
	}
}
