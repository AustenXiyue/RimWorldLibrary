using System.Collections.Specialized;
using System.Configuration;

namespace System.Windows;

/// <summary>Contains properties that specify how an application should behave relative to WPF features that are in the PresentationFramework assembly.</summary>
public static class FrameworkCompatibilityPreferences
{
	private static bool _targetsDesktop_V4_0;

	private static bool _areInactiveSelectionHighlightBrushKeysSupported;

	private static bool _keepTextBoxDisplaySynchronizedWithTextProperty;

	private static bool _useSetWindowPosForTopmostWindows;

	private static bool _vsp45Compat;

	private static string _scrollingTraceTarget;

	private static string _scrollingTraceFile;

	private static bool _shouldThrowOnCopyOrCutFailure;

	private static string _IMECompositionTraceTarget;

	private static string _IMECompositionTraceFile;

	private static bool _isSealed;

	private static readonly object _lockObject;

	internal static bool TargetsDesktop_V4_0 => _targetsDesktop_V4_0;

	/// <summary>Gets or sets a value that indicates whether the application should use the <see cref="P:System.Windows.SystemColors.InactiveSelectionHighlightBrush" /> and <see cref="P:System.Windows.SystemColors.InactiveSelectionHighlightTextBrush" /> properties for the colors of inactive selected items.</summary>
	/// <returns>true if the application should use the <see cref="P:System.Windows.SystemColors.InactiveSelectionHighlightBrush" /> and <see cref="P:System.Windows.SystemColors.InactiveSelectionHighlightTextBrush" /> properties for the colors of inactive selected items; otherwise, false</returns>
	public static bool AreInactiveSelectionHighlightBrushKeysSupported
	{
		get
		{
			return _areInactiveSelectionHighlightBrushKeysSupported;
		}
		set
		{
			lock (_lockObject)
			{
				if (_isSealed)
				{
					throw new InvalidOperationException(SR.Format(SR.CompatibilityPreferencesSealed, "AreInactiveSelectionHighlightBrushKeysSupported", "FrameworkCompatibilityPreferences"));
				}
				_areInactiveSelectionHighlightBrushKeysSupported = value;
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether a data-bound <see cref="T:System.Windows.Controls.TextBox" /> should display a string that is identical to the value of the source its <see cref="P:System.Windows.Controls.TextBox.Text" /> property</summary>
	/// <returns>true if a data-bound <see cref="T:System.Windows.Controls.TextBox" /> should display a string that is identical to the value of the source its <see cref="P:System.Windows.Controls.TextBox.Text" /> property; otherwise, false.</returns>
	public static bool KeepTextBoxDisplaySynchronizedWithTextProperty
	{
		get
		{
			return _keepTextBoxDisplaySynchronizedWithTextProperty;
		}
		set
		{
			lock (_lockObject)
			{
				if (_isSealed)
				{
					throw new InvalidOperationException(SR.Format(SR.CompatibilityPreferencesSealed, "AextBoxDisplaysText", "FrameworkCompatibilityPreferences"));
				}
				_keepTextBoxDisplaySynchronizedWithTextProperty = value;
			}
		}
	}

	internal static bool UseSetWindowPosForTopmostWindows
	{
		get
		{
			return _useSetWindowPosForTopmostWindows;
		}
		set
		{
			lock (_lockObject)
			{
				if (_isSealed)
				{
					throw new InvalidOperationException(SR.Format(SR.CompatibilityPreferencesSealed, "UseSetWindowPosForTopmostWindows", "FrameworkCompatibilityPreferences"));
				}
				_useSetWindowPosForTopmostWindows = value;
			}
		}
	}

	internal static bool VSP45Compat
	{
		get
		{
			return _vsp45Compat;
		}
		set
		{
			lock (_lockObject)
			{
				if (_isSealed)
				{
					throw new InvalidOperationException(SR.Format(SR.CompatibilityPreferencesSealed, "IsVirtualizingStackPanel_45Compatible", "FrameworkCompatibilityPreferences"));
				}
				_vsp45Compat = value;
			}
		}
	}

	public static bool ShouldThrowOnCopyOrCutFailure
	{
		get
		{
			return _shouldThrowOnCopyOrCutFailure;
		}
		set
		{
			if (_isSealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CompatibilityPreferencesSealed, "ShouldThrowOnCopyOrCutFailure", "FrameworkCompatibilityPreferences"));
			}
			_shouldThrowOnCopyOrCutFailure = value;
		}
	}

	static FrameworkCompatibilityPreferences()
	{
		_areInactiveSelectionHighlightBrushKeysSupported = true;
		_keepTextBoxDisplaySynchronizedWithTextProperty = true;
		_useSetWindowPosForTopmostWindows = false;
		_vsp45Compat = false;
		_shouldThrowOnCopyOrCutFailure = false;
		_lockObject = new object();
		_targetsDesktop_V4_0 = false;
		NameValueCollection nameValueCollection = null;
		try
		{
			nameValueCollection = ConfigurationManager.AppSettings;
		}
		catch (ConfigurationErrorsException)
		{
		}
		if (nameValueCollection != null)
		{
			SetUseSetWindowPosForTopmostWindowsFromAppSettings(nameValueCollection);
			SetVSP45CompatFromAppSettings(nameValueCollection);
			SetScrollingTraceFromAppSettings(nameValueCollection);
			SetShouldThrowOnCopyOrCutFailuresFromAppSettings(nameValueCollection);
			SetIMECompositionTraceFromAppSettings(nameValueCollection);
		}
	}

	internal static bool GetAreInactiveSelectionHighlightBrushKeysSupported()
	{
		Seal();
		return AreInactiveSelectionHighlightBrushKeysSupported;
	}

	internal static bool GetKeepTextBoxDisplaySynchronizedWithTextProperty()
	{
		Seal();
		return KeepTextBoxDisplaySynchronizedWithTextProperty;
	}

	internal static bool GetUseSetWindowPosForTopmostWindows()
	{
		Seal();
		return UseSetWindowPosForTopmostWindows;
	}

	private static void SetUseSetWindowPosForTopmostWindowsFromAppSettings(NameValueCollection appSettings)
	{
		if (bool.TryParse(appSettings["UseSetWindowPosForTopmostWindows"], out var result))
		{
			UseSetWindowPosForTopmostWindows = result;
		}
	}

	internal static bool GetVSP45Compat()
	{
		Seal();
		return VSP45Compat;
	}

	private static void SetVSP45CompatFromAppSettings(NameValueCollection appSettings)
	{
		if (bool.TryParse(appSettings["IsVirtualizingStackPanel_45Compatible"], out var result))
		{
			VSP45Compat = result;
		}
	}

	internal static string GetScrollingTraceTarget()
	{
		Seal();
		return _scrollingTraceTarget;
	}

	internal static string GetScrollingTraceFile()
	{
		Seal();
		return _scrollingTraceFile;
	}

	private static void SetScrollingTraceFromAppSettings(NameValueCollection appSettings)
	{
		_scrollingTraceTarget = appSettings["ScrollingTraceTarget"];
		_scrollingTraceFile = appSettings["ScrollingTraceFile"];
	}

	internal static bool GetShouldThrowOnCopyOrCutFailure()
	{
		Seal();
		return ShouldThrowOnCopyOrCutFailure;
	}

	private static void SetShouldThrowOnCopyOrCutFailuresFromAppSettings(NameValueCollection appSettings)
	{
		if (bool.TryParse(appSettings["ShouldThrowOnCopyOrCutFailure"], out var result))
		{
			ShouldThrowOnCopyOrCutFailure = result;
		}
	}

	internal static string GetIMECompositionTraceTarget()
	{
		Seal();
		return _IMECompositionTraceTarget;
	}

	internal static string GetIMECompositionTraceFile()
	{
		Seal();
		return _IMECompositionTraceFile;
	}

	private static void SetIMECompositionTraceFromAppSettings(NameValueCollection appSettings)
	{
		_IMECompositionTraceTarget = appSettings["IMECompositionTraceTarget"];
		_IMECompositionTraceFile = appSettings["IMECompositionTraceFile"];
	}

	private static void Seal()
	{
		if (!_isSealed)
		{
			lock (_lockObject)
			{
				_isSealed = true;
			}
		}
	}
}
