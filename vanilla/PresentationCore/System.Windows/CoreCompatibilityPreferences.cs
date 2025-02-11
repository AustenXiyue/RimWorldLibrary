using System.Collections.Specialized;
using System.Configuration;
using MS.Internal.PresentationCore;

namespace System.Windows;

/// <summary>Contains properties that specify how an application should behave relative to WPF features that are in the PresentationCore assembly.</summary>
public static class CoreCompatibilityPreferences
{
	private static bool _isAltKeyRequiredInAccessKeyDefaultScope;

	private static bool _includeAllInkInBoundingBox;

	private static bool? _enableMultiMonitorDisplayClipping;

	private static bool _isSealed;

	private static readonly object _lockObject;

	internal static bool TargetsAtLeast_Desktop_V4_5 => true;

	/// <summary>Gets or sets a value that indicates whether the user needs to use the ALT key to invoke a shortcut.</summary>
	/// <returns>true if the user needs to use the ALT key to invoke a shortcut; otherwise, false.  The default is false.</returns>
	public static bool IsAltKeyRequiredInAccessKeyDefaultScope
	{
		get
		{
			return _isAltKeyRequiredInAccessKeyDefaultScope;
		}
		set
		{
			lock (_lockObject)
			{
				if (_isSealed)
				{
					throw new InvalidOperationException(SR.Format(SR.CompatibilityPreferencesSealed, "IsAltKeyRequiredInAccessKeyDefaultScope", "CoreCompatibilityPreferences"));
				}
				_isAltKeyRequiredInAccessKeyDefaultScope = value;
			}
		}
	}

	internal static bool IncludeAllInkInBoundingBox
	{
		get
		{
			return _includeAllInkInBoundingBox;
		}
		set
		{
			lock (_lockObject)
			{
				if (_isSealed)
				{
					throw new InvalidOperationException(SR.Format(SR.CompatibilityPreferencesSealed, "IncludeAllInkInBoundingBox", "CoreCompatibilityPreferences"));
				}
				_includeAllInkInBoundingBox = value;
			}
		}
	}

	public static bool? EnableMultiMonitorDisplayClipping
	{
		get
		{
			return GetEnableMultiMonitorDisplayClipping();
		}
		set
		{
			lock (_lockObject)
			{
				if (_isSealed)
				{
					throw new InvalidOperationException(SR.Format(SR.CompatibilityPreferencesSealed, "DisableMultimonDisplayClipping", "CoreCompatibilityPreferences"));
				}
				_enableMultiMonitorDisplayClipping = value;
			}
		}
	}

	static CoreCompatibilityPreferences()
	{
		_isAltKeyRequiredInAccessKeyDefaultScope = false;
		_includeAllInkInBoundingBox = true;
		_enableMultiMonitorDisplayClipping = null;
		_lockObject = new object();
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
			SetIncludeAllInkInBoundingBoxFromAppSettings(nameValueCollection);
			SetEnableMultiMonitorDisplayClippingFromAppSettings(nameValueCollection);
		}
	}

	internal static bool GetIsAltKeyRequiredInAccessKeyDefaultScope()
	{
		Seal();
		return IsAltKeyRequiredInAccessKeyDefaultScope;
	}

	internal static bool GetIncludeAllInkInBoundingBox()
	{
		Seal();
		return IncludeAllInkInBoundingBox;
	}

	private static void SetIncludeAllInkInBoundingBoxFromAppSettings(NameValueCollection appSettings)
	{
		if (bool.TryParse(appSettings["IncludeAllInkInBoundingBox"], out var result))
		{
			IncludeAllInkInBoundingBox = result;
		}
	}

	internal static bool? GetEnableMultiMonitorDisplayClipping()
	{
		Seal();
		return _enableMultiMonitorDisplayClipping;
	}

	private static void SetEnableMultiMonitorDisplayClippingFromAppSettings(NameValueCollection appSettings)
	{
		if (bool.TryParse(appSettings["EnableMultiMonitorDisplayClipping"], out var result))
		{
			EnableMultiMonitorDisplayClipping = result;
		}
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
