using System.Collections.Specialized;
using System.Configuration;
using Microsoft.Win32;
using MS.Internal.WindowsBase;

namespace System.Windows;

/// <summary>Contains properties that specify how an application should behave relative to new WPF features that are in the WindowsBase assembly.</summary>
public static class BaseCompatibilityPreferences
{
	public enum HandleDispatcherRequestProcessingFailureOptions
	{
		Continue,
		Throw,
		Reset
	}

	private static bool _reuseDispatcherSynchronizationContextInstance;

	private static bool _flowDispatcherSynchronizationContextPriority;

	private static bool _inlineDispatcherSynchronizationContextSend;

	private static bool _matchPackageSignatureMethodToPackagePartDigestMethod;

	private const string WpfPackagingKey = "HKEY_CURRENT_USER\\Software\\Microsoft\\Avalon.Packaging\\";

	private const string WpfPackagingSubKeyPath = "Software\\Microsoft\\Avalon.Packaging\\";

	private const string MatchPackageSignatureMethodToPackagePartDigestMethodValue = "MatchPackageSignatureMethodToPackagePartDigestMethod";

	private static HandleDispatcherRequestProcessingFailureOptions _handleDispatcherRequestProcessingFailure;

	private static bool _isSealed;

	private static readonly object _lockObject;

	/// <summary>Gets or sets a value that indicates whether a single <see cref="T:System.Windows.Threading.DispatcherSynchronizationContext" /> is used for multiple dispatcher operations.</summary>
	/// <returns>true if a single <see cref="T:System.Windows.Threading.DispatcherSynchronizationContext" /> is used for multiple dispatcher operations; otherwise, false.</returns>
	public static bool ReuseDispatcherSynchronizationContextInstance
	{
		get
		{
			return _reuseDispatcherSynchronizationContextInstance;
		}
		set
		{
			lock (_lockObject)
			{
				if (_isSealed)
				{
					throw new InvalidOperationException(SR.Format(SR.CompatibilityPreferencesSealed, "ReuseDispatcherSynchronizationContextInstance", "BaseCompatibilityPreferences"));
				}
				_reuseDispatcherSynchronizationContextInstance = value;
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether information about the <see cref="P:System.Windows.Threading.DispatcherOperation.Priority" /> is saved to the <see cref="T:System.Windows.Threading.DispatcherSynchronizationContext" />.</summary>
	/// <returns>true if information about the <see cref="P:System.Windows.Threading.DispatcherOperation.Priority" /> is saved to the <see cref="T:System.Windows.Threading.DispatcherSynchronizationContext" />; otherwise, false.</returns>
	public static bool FlowDispatcherSynchronizationContextPriority
	{
		get
		{
			return _flowDispatcherSynchronizationContextPriority;
		}
		set
		{
			lock (_lockObject)
			{
				if (_isSealed)
				{
					throw new InvalidOperationException(SR.Format(SR.CompatibilityPreferencesSealed, "FlowDispatcherSynchronizationContextPriority", "BaseCompatibilityPreferences"));
				}
				_flowDispatcherSynchronizationContextPriority = value;
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="M:System.Windows.Threading.DispatcherSynchronizationContext.Send(System.Threading.SendOrPostCallback,System.Object)" /> method puts the delegates on the dispatcher queue or if the delegate is directly invoked.</summary>
	/// <returns>true if the <see cref="M:System.Windows.Threading.DispatcherSynchronizationContext.Send(System.Threading.SendOrPostCallback,System.Object)" /> method puts the delegates on the dispatcher queue or if the delegate is directly invoked; otherwise, false.</returns>
	public static bool InlineDispatcherSynchronizationContextSend
	{
		get
		{
			return _inlineDispatcherSynchronizationContextSend;
		}
		set
		{
			lock (_lockObject)
			{
				if (_isSealed)
				{
					throw new InvalidOperationException(SR.Format(SR.CompatibilityPreferencesSealed, "InlineDispatcherSynchronizationContextSend", "BaseCompatibilityPreferences"));
				}
				_inlineDispatcherSynchronizationContextSend = value;
			}
		}
	}

	internal static bool MatchPackageSignatureMethodToPackagePartDigestMethod
	{
		get
		{
			return _matchPackageSignatureMethodToPackagePartDigestMethod;
		}
		set
		{
			_matchPackageSignatureMethodToPackagePartDigestMethod = value;
		}
	}

	public static HandleDispatcherRequestProcessingFailureOptions HandleDispatcherRequestProcessingFailure
	{
		get
		{
			return _handleDispatcherRequestProcessingFailure;
		}
		set
		{
			_handleDispatcherRequestProcessingFailure = value;
		}
	}

	static BaseCompatibilityPreferences()
	{
		_reuseDispatcherSynchronizationContextInstance = false;
		_flowDispatcherSynchronizationContextPriority = true;
		_inlineDispatcherSynchronizationContextSend = true;
		_matchPackageSignatureMethodToPackagePartDigestMethod = true;
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
			SetHandleDispatcherRequestProcessingFailureFromAppSettings(nameValueCollection);
		}
		SetMatchPackageSignatureMethodToPackagePartDigestMethod(nameValueCollection);
	}

	internal static bool GetReuseDispatcherSynchronizationContextInstance()
	{
		Seal();
		return ReuseDispatcherSynchronizationContextInstance;
	}

	internal static bool GetFlowDispatcherSynchronizationContextPriority()
	{
		Seal();
		return FlowDispatcherSynchronizationContextPriority;
	}

	internal static bool GetInlineDispatcherSynchronizationContextSend()
	{
		Seal();
		return InlineDispatcherSynchronizationContextSend;
	}

	private static void SetMatchPackageSignatureMethodToPackagePartDigestMethod(NameValueCollection appSettings)
	{
		if (appSettings == null || !SetMatchPackageSignatureMethodToPackagePartDigestMethodFromAppSettings(appSettings))
		{
			SetMatchPackageSignatureMethodToPackagePartDigestMethodFromRegistry();
		}
	}

	private static bool SetMatchPackageSignatureMethodToPackagePartDigestMethodFromAppSettings(NameValueCollection appSettings)
	{
		if (bool.TryParse(appSettings["MatchPackageSignatureMethodToPackagePartDigestMethod"], out var result))
		{
			MatchPackageSignatureMethodToPackagePartDigestMethod = result;
			return true;
		}
		return false;
	}

	private static void SetMatchPackageSignatureMethodToPackagePartDigestMethodFromRegistry()
	{
		try
		{
			using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Avalon.Packaging\\", RegistryKeyPermissionCheck.ReadSubTree);
			if (registryKey != null && registryKey.GetValueKind("MatchPackageSignatureMethodToPackagePartDigestMethod") == RegistryValueKind.DWord)
			{
				object value = registryKey.GetValue("MatchPackageSignatureMethodToPackagePartDigestMethod");
				if (value != null)
				{
					MatchPackageSignatureMethodToPackagePartDigestMethod = (int)value == 1;
				}
			}
		}
		catch
		{
		}
	}

	private static void SetHandleDispatcherRequestProcessingFailureFromAppSettings(NameValueCollection appSettings)
	{
		if (Enum.TryParse<HandleDispatcherRequestProcessingFailureOptions>(appSettings["HandleDispatcherRequestProcessingFailure"], ignoreCase: true, out var result))
		{
			HandleDispatcherRequestProcessingFailure = result;
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
