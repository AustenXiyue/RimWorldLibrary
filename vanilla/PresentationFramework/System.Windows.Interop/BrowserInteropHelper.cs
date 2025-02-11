using System.Runtime.InteropServices;
using MS.Internal;
using MS.Internal.AppModel;
using MS.Internal.Interop;
using MS.Internal.PresentationFramework;
using MS.Win32;

namespace System.Windows.Interop;

/// <summary>A helper class that provides information about the browser environment in which a XAML browser application (XBAP) application is hosted.</summary>
public static class BrowserInteropHelper
{
	private static MS.Internal.SecurityCriticalDataForSet<HostingFlags> _hostingFlags;

	private static MS.Internal.SecurityCriticalDataForSet<bool> _isInitialViewerNavigation;

	private static MS.Internal.SecurityCriticalDataForSet<bool?> _isScriptInteropDisabled;

	private static MS.Internal.SecurityCriticalDataForSet<MS.Win32.UnsafeNativeMethods.IServiceProvider> _hostHtmlDocumentServiceProvider;

	private static MS.Internal.SecurityCriticalDataForSet<bool> _initializedHostScript;

	/// <summary>Returns a reference to an object that can be used to access the host browser via its OLE container interfaces (for example, IOleClientSite::GetContainer();).</summary>
	/// <returns>An object that can be cast to <see cref="T:Microsoft.VisualStudio.OLE.Interop.IOleClientSite" />.</returns>
	public static object ClientSite => null;

	/// <summary>Gets a script object that provides access to the HTML window object, custom script functions, and global variables for the HTML page, if the XAML browser application (XBAP) is hosted in a frame.</summary>
	/// <returns>A script object that provides access to the HTML window object, custom script functions, and global variables for the HTML page, if the XAML browser application (XBAP) is hosted in a frame; otherwise, null.</returns>
	public static dynamic HostScript => null;

	/// <summary>Gets a value that specifies whether the current Windows Presentation Foundation (WPF) application is browser hosted.</summary>
	/// <returns>true if the application is browser hosted; otherwise, false.</returns>
	public static bool IsBrowserHosted => false;

	internal static HostingFlags HostingFlags
	{
		get
		{
			return _hostingFlags.Value;
		}
		set
		{
			_hostingFlags.Value = value;
		}
	}

	/// <summary>Gets the uniform resource identifier (URI) for the location from which a XAML browser application (XBAP) application was launched.</summary>
	/// <returns>The <see cref="T:System.Uri" /> for the location from which a XAML browser application (XBAP) application was launched; otherwise, null.</returns>
	public static Uri Source => SiteOfOriginContainer.BrowserSource;

	internal static bool IsViewer
	{
		get
		{
			Application current = Application.Current;
			if (current != null)
			{
				return current.MimeType == MimeType.Markup;
			}
			return false;
		}
	}

	internal static bool IsHostedInIEorWebOC => (HostingFlags & HostingFlags.hfHostedInIEorWebOC) != 0;

	internal static bool IsInitialViewerNavigation
	{
		get
		{
			if (IsViewer)
			{
				return _isInitialViewerNavigation.Value;
			}
			return false;
		}
		set
		{
			_isInitialViewerNavigation.Value = value;
		}
	}

	internal static MS.Win32.UnsafeNativeMethods.IServiceProvider HostHtmlDocumentServiceProvider
	{
		get
		{
			Invariant.Assert(!_initializedHostScript.Value || _hostHtmlDocumentServiceProvider.Value != null);
			return _hostHtmlDocumentServiceProvider.Value;
		}
	}

	static BrowserInteropHelper()
	{
		IsInitialViewerNavigation = true;
	}

	private static void InitializeHostHtmlDocumentServiceProvider(DynamicScriptObject scriptObject)
	{
		if (IsHostedInIEorWebOC && scriptObject.ScriptObject is MS.Win32.UnsafeNativeMethods.IHTMLWindow4 && _hostHtmlDocumentServiceProvider.Value == null)
		{
			Invariant.Assert(scriptObject.TryFindMemberAndInvokeNonWrapped("document", 2, cacheDispId: true, null, out var result));
			_hostHtmlDocumentServiceProvider.Value = (MS.Win32.UnsafeNativeMethods.IServiceProvider)result;
			_initializedHostScript.Value = true;
		}
	}

	private static void HostFilterInput(ref MSG msg, ref bool handled)
	{
		WindowMessage message = (WindowMessage)msg.message;
		switch (message)
		{
		default:
			if (message < WindowMessage.WM_MOUSEMOVE || message > WindowMessage.WM_MOUSEHWHEEL)
			{
				break;
			}
			goto case WindowMessage.WM_INPUT;
		case WindowMessage.WM_INPUT:
		case WindowMessage.WM_KEYFIRST:
		case WindowMessage.WM_KEYUP:
		case WindowMessage.WM_CHAR:
		case WindowMessage.WM_DEADCHAR:
		case WindowMessage.WM_SYSKEYDOWN:
		case WindowMessage.WM_SYSKEYUP:
		case WindowMessage.WM_SYSCHAR:
		case WindowMessage.WM_SYSDEADCHAR:
		case WindowMessage.WM_KEYLAST:
		case (WindowMessage)265:
		case (WindowMessage)266:
		case (WindowMessage)267:
		case (WindowMessage)268:
		case WindowMessage.WM_IME_STARTCOMPOSITION:
		case WindowMessage.WM_IME_ENDCOMPOSITION:
		case WindowMessage.WM_IME_COMPOSITION:
			if (ForwardTranslateAccelerator(ref msg, appUnhandled: false) == 0)
			{
				handled = true;
			}
			break;
		}
	}

	internal static nint PostFilterInput(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		if (!handled && msg >= 256 && msg <= 271)
		{
			MSG pMsg = new MSG(hwnd, msg, wParam, lParam, SafeNativeMethods.GetMessageTime(), 0, 0);
			if (ForwardTranslateAccelerator(ref pMsg, appUnhandled: true) == 0)
			{
				handled = true;
			}
		}
		return IntPtr.Zero;
	}

	internal static void InitializeHostFilterInput()
	{
		ComponentDispatcher.ThreadFilterMessage += HostFilterInput;
	}

	private static void EnsureScriptInteropAllowed()
	{
		if (!_isScriptInteropDisabled.Value.HasValue)
		{
			_isScriptInteropDisabled.Value = SafeSecurityHelper.IsFeatureDisabled(SafeSecurityHelper.KeyToRead.ScriptInteropDisable);
		}
	}

	[DllImport("PresentationHost_cor3.dll")]
	private static extern int ForwardTranslateAccelerator(ref MSG pMsg, bool appUnhandled);
}
