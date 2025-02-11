using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.IO.Packaging;
using MS.Internal.Telemetry.PresentationFramework;
using MS.Internal.Utility;
using MS.Win32;

namespace System.Windows.Controls;

/// <summary>Hosts and navigates between HTML documents. Enables interoperability between WPF managed code and HTML script.</summary>
public sealed class WebBrowser : ActiveXHost
{
	internal class WebOCHostingAdaptor
	{
		protected WebBrowser _webBrowser;

		internal virtual object ObjectForScripting
		{
			get
			{
				return _webBrowser.ObjectForScripting;
			}
			set
			{
			}
		}

		internal WebOCHostingAdaptor(WebBrowser webBrowser)
		{
			_webBrowser = webBrowser;
		}

		internal virtual object CreateWebOC()
		{
			return Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("8856f961-340a-11d0-a96b-00c04fd705a2")));
		}

		internal virtual object CreateEventSink()
		{
			return new WebBrowserEvent(_webBrowser);
		}
	}

	internal bool _canGoBack;

	internal bool _canGoForward;

	internal const string AboutBlankUriString = "about:blank";

	private MS.Win32.UnsafeNativeMethods.IWebBrowser2 _axIWebBrowser2;

	private WebOCHostingAdaptor _hostingAdaptor;

	private ConnectionPointCookie _cookie;

	private object _objectForScripting;

	private Stream _documentStream;

	private MS.Internal.SecurityCriticalDataForSet<bool> _navigatingToAboutBlank;

	private MS.Internal.SecurityCriticalDataForSet<Guid> _lastNavigation;

	/// <summary>Gets or sets the <see cref="T:System.Uri" /> of the current document hosted in the <see cref="T:System.Windows.Controls.WebBrowser" />.</summary>
	/// <returns>The <see cref="T:System.Uri" /> for the current HTML document.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Windows.Controls.WebBrowser" /> instance is no longer valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
	/// <exception cref="T:System.Security.SecurityException">Navigation from an application that is running in partial trust to a <see cref="T:System.Uri" /> that is not located at the site of origin.</exception>
	public Uri Source
	{
		get
		{
			VerifyAccess();
			string text = AxIWebBrowser2.LocationURL;
			if (NavigatingToAboutBlank)
			{
				text = null;
			}
			if (!string.IsNullOrEmpty(text))
			{
				return new Uri(text);
			}
			return null;
		}
		set
		{
			VerifyAccess();
			Navigate(value);
		}
	}

	/// <summary>Gets a value that indicates whether there is a document to navigate back to.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> value that indicates whether there is a document to navigate back to.</returns>
	public bool CanGoBack
	{
		get
		{
			VerifyAccess();
			if (!base.IsDisposed)
			{
				return _canGoBack;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether there is a document to navigate forward to.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> value that indicates whether there is a document to navigate forward to.</returns>
	public bool CanGoForward
	{
		get
		{
			VerifyAccess();
			if (!base.IsDisposed)
			{
				return _canGoForward;
			}
			return false;
		}
	}

	/// <summary>Gets or sets an instance of a public class, implemented by the host application, that can be accessed by script from a hosted document.</summary>
	/// <returns>The <see cref="T:System.Object" /> that is an instance of a public class, implemented by the host application, that can be accessed by script from a hosted document.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Windows.Controls.WebBrowser.ObjectForScripting" /> is set with an instance of type that is not COMVisible.</exception>
	public object ObjectForScripting
	{
		get
		{
			VerifyAccess();
			return _objectForScripting;
		}
		set
		{
			VerifyAccess();
			if (value != null && !MarshalLocal.IsTypeVisibleFromCom(value.GetType()))
			{
				throw new ArgumentException(SR.NeedToBeComVisible);
			}
			_objectForScripting = value;
			_hostingAdaptor.ObjectForScripting = value;
		}
	}

	/// <summary>Gets the Document object that represents the hosted HTML page. </summary>
	/// <returns>A Document object.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Windows.Controls.WebBrowser" /> instance is no longer valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
	public object Document
	{
		get
		{
			VerifyAccess();
			return AxIWebBrowser2.Document;
		}
	}

	internal MS.Win32.UnsafeNativeMethods.IHTMLDocument2 NativeHTMLDocument => AxIWebBrowser2.Document as MS.Win32.UnsafeNativeMethods.IHTMLDocument2;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	internal MS.Win32.UnsafeNativeMethods.IWebBrowser2 AxIWebBrowser2
	{
		get
		{
			if (_axIWebBrowser2 == null)
			{
				if (base.IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				TransitionUpTo(ActiveXHelper.ActiveXState.Running);
			}
			if (_axIWebBrowser2 == null)
			{
				throw new InvalidOperationException(SR.WebBrowserNoCastToIWebBrowser2);
			}
			return _axIWebBrowser2;
		}
	}

	internal WebOCHostingAdaptor HostingAdaptor => _hostingAdaptor;

	internal Stream DocumentStream
	{
		get
		{
			return _documentStream;
		}
		set
		{
			_documentStream = value;
		}
	}

	internal bool NavigatingToAboutBlank
	{
		get
		{
			return _navigatingToAboutBlank.Value;
		}
		set
		{
			_navigatingToAboutBlank.Value = value;
		}
	}

	internal Guid LastNavigation
	{
		get
		{
			return _lastNavigation.Value;
		}
		set
		{
			_lastNavigation.Value = value;
		}
	}

	/// <summary>Occurs just before navigation to a document.</summary>
	public event NavigatingCancelEventHandler Navigating;

	/// <summary>Occurs when the document being navigated to is located and has started downloading.</summary>
	public event NavigatedEventHandler Navigated;

	/// <summary>Occurs when the document being navigated to has finished downloading.</summary>
	public event LoadCompletedEventHandler LoadCompleted;

	static WebBrowser()
	{
		TurnOnFeatureControlKeys();
		ControlsTraceLogger.AddControl(TelemetryControls.WebBrowser);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.WebBrowser" /> class.</summary>
	public WebBrowser()
		: base(new Guid("8856f961-340a-11d0-a96b-00c04fd705a2"), fTrusted: true)
	{
		_hostingAdaptor = new WebOCHostingAdaptor(this);
	}

	/// <summary>Navigate asynchronously to the document at the specified <see cref="T:System.Uri" />.</summary>
	/// <param name="source">The <see cref="T:System.Uri" /> to navigate to.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Windows.Controls.WebBrowser" /> instance is no longer valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
	/// <exception cref="T:System.Security.SecurityException">Navigation from an application that is running in partial trust to a <see cref="T:System.Uri" /> that is not located at the site of origin.</exception>
	public void Navigate(Uri source)
	{
		Navigate(source, null, null, null);
	}

	/// <summary>Navigates asynchronously to the document at the specified URL.</summary>
	/// <param name="source">The URL to navigate to.</param>
	public void Navigate(string source)
	{
		Navigate(source, null, null, null);
	}

	/// <summary>Navigate asynchronously to the document at the specified <see cref="T:System.Uri" /> and specify the target frame to load the document's content into. Additional HTTP POST data and HTTP headers can be sent to the server as part of the navigation request.</summary>
	/// <param name="source">The <see cref="T:System.Uri" /> to navigate to.</param>
	/// <param name="targetFrameName">The name of the frame to display the document's content in.</param>
	/// <param name="postData">HTTP POST data to send to the server when the source is requested.</param>
	/// <param name="additionalHeaders">HTTP headers to send to the server when the source is requested.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Windows.Controls.WebBrowser" /> instance is no longer valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
	/// <exception cref="T:System.Security.SecurityException">Navigation from an application that is running in partial trust:To a <see cref="T:System.Uri" /> that is not located at the site of origin, or <paramref name="targetFrameName" /> name is not null or empty.</exception>
	public void Navigate(Uri source, string targetFrameName, byte[] postData, string additionalHeaders)
	{
		object targetFrameName2 = targetFrameName;
		object postData2 = postData;
		object headers = additionalHeaders;
		DoNavigate(source, ref targetFrameName2, ref postData2, ref headers);
	}

	/// <summary>Navigates asynchronously to the document at the specified URL and specify the target frame to load the document's content into. Additional HTTP POST data and HTTP headers can be sent to the server as part of the navigation request.</summary>
	/// <param name="source">The URL to navigate to.</param>
	/// <param name="targetFrameName">The name of the frame to display the document's content in.</param>
	/// <param name="postData">HTTP POST data to send to the server when the source is requested.</param>
	/// <param name="additionalHeaders">HTTP headers to send to the server when the source is requested.</param>
	public void Navigate(string source, string targetFrameName, byte[] postData, string additionalHeaders)
	{
		object targetFrameName2 = targetFrameName;
		object postData2 = postData;
		object headers = additionalHeaders;
		Uri source2 = new Uri(source);
		DoNavigate(source2, ref targetFrameName2, ref postData2, ref headers, ignoreEscaping: true);
	}

	/// <summary>Navigate asynchronously to a <see cref="T:System.IO.Stream" /> that contains the content for a document.</summary>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> that contains the content for a document.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Windows.Controls.WebBrowser" /> instance is no longer valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
	public void NavigateToStream(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		DocumentStream = stream;
		Source = null;
	}

	/// <summary>Navigate asynchronously to a <see cref="T:System.String" /> that contains the content for a document.</summary>
	/// <param name="text">The <see cref="T:System.String" /> that contains the content for a document.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Windows.Controls.WebBrowser" /> instance is no longer valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
	public void NavigateToString(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			throw new ArgumentNullException("text");
		}
		MemoryStream memoryStream = new MemoryStream(text.Length);
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		streamWriter.Write(text);
		streamWriter.Flush();
		memoryStream.Position = 0L;
		NavigateToStream(memoryStream);
	}

	/// <summary>Navigate back to the previous document, if there is one.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Windows.Controls.WebBrowser" /> instance is no longer valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no document to navigate back to.</exception>
	public void GoBack()
	{
		VerifyAccess();
		AxIWebBrowser2.GoBack();
	}

	/// <summary>Navigate forward to the next HTML document, if there is one.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Windows.Controls.WebBrowser" /> instance is no longer valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no document to navigate forward to.</exception>
	public void GoForward()
	{
		VerifyAccess();
		AxIWebBrowser2.GoForward();
	}

	/// <summary>Reloads the current page.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Windows.Controls.WebBrowser" /> instance is no longer valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
	public void Refresh()
	{
		VerifyAccess();
		AxIWebBrowser2.Refresh();
	}

	/// <summary>Reloads the current page with optional cache validation.</summary>
	/// <param name="noCache">Specifies whether to refresh without cache validation.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Windows.Controls.WebBrowser" /> instance is no longer valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
	public void Refresh(bool noCache)
	{
		VerifyAccess();
		object level = (noCache ? 3 : 0);
		AxIWebBrowser2.Refresh2(ref level);
	}

	/// <summary>Executes a script function that is implemented by the currently loaded document.</summary>
	/// <returns>The object returned by the Active Scripting call.</returns>
	/// <param name="scriptName">The name of the script function to execute.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Windows.Controls.WebBrowser" /> instance is no longer valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">The script function does not exist.</exception>
	public object InvokeScript(string scriptName)
	{
		return InvokeScript(scriptName, null);
	}

	/// <summary>Executes a script function that is defined in the currently loaded document.</summary>
	/// <returns>The object returned by the Active Scripting call.</returns>
	/// <param name="scriptName">The name of the script function to execute.</param>
	/// <param name="args">The parameters to pass to the script function.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Windows.Controls.WebBrowser" /> instance is no longer valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">The script function does not exist.</exception>
	public object InvokeScript(string scriptName, params object[] args)
	{
		VerifyAccess();
		if (string.IsNullOrEmpty(scriptName))
		{
			throw new ArgumentNullException("scriptName");
		}
		MS.Win32.UnsafeNativeMethods.IDispatchEx dispatchEx = null;
		MS.Win32.UnsafeNativeMethods.IHTMLDocument2 nativeHTMLDocument = NativeHTMLDocument;
		if (nativeHTMLDocument != null)
		{
			dispatchEx = nativeHTMLDocument.GetScript() as MS.Win32.UnsafeNativeMethods.IDispatchEx;
		}
		object result = null;
		if (dispatchEx != null)
		{
			MS.Win32.NativeMethods.DISPPARAMS dISPPARAMS = new MS.Win32.NativeMethods.DISPPARAMS();
			dISPPARAMS.rgvarg = IntPtr.Zero;
			try
			{
				Guid riid = Guid.Empty;
				string[] rgszNames = new string[1] { scriptName };
				int[] array = new int[1] { -1 };
				dispatchEx.GetIDsOfNames(ref riid, rgszNames, 1, Thread.CurrentThread.CurrentCulture.LCID, array).ThrowIfFailed();
				if (args != null)
				{
					Array.Reverse(args);
				}
				dISPPARAMS.rgvarg = ((args == null) ? IntPtr.Zero : MS.Win32.UnsafeNativeMethods.ArrayToVARIANTHelper.ArrayToVARIANTVector(args));
				dISPPARAMS.cArgs = ((args != null) ? ((uint)args.Length) : 0u);
				dISPPARAMS.rgdispidNamedArgs = IntPtr.Zero;
				dISPPARAMS.cNamedArgs = 0u;
				dispatchEx.InvokeEx(array[0], Thread.CurrentThread.CurrentCulture.LCID, 1, dISPPARAMS, out result, new MS.Win32.NativeMethods.EXCEPINFO(), null).ThrowIfFailed();
				return result;
			}
			finally
			{
				if (dISPPARAMS.rgvarg != IntPtr.Zero)
				{
					MS.Win32.UnsafeNativeMethods.ArrayToVARIANTHelper.FreeVARIANTVector(dISPPARAMS.rgvarg, args.Length);
				}
			}
		}
		throw new InvalidOperationException(SR.CannotInvokeScript);
	}

	internal void OnNavigating(NavigatingCancelEventArgs e)
	{
		VerifyAccess();
		if (this.Navigating != null)
		{
			this.Navigating(this, e);
		}
	}

	internal void OnNavigated(NavigationEventArgs e)
	{
		VerifyAccess();
		if (this.Navigated != null)
		{
			this.Navigated(this, e);
		}
	}

	internal void OnLoadCompleted(NavigationEventArgs e)
	{
		VerifyAccess();
		if (this.LoadCompleted != null)
		{
			this.LoadCompleted(this, e);
		}
	}

	internal override object CreateActiveXObject(Guid clsid)
	{
		return _hostingAdaptor.CreateWebOC();
	}

	internal override void AttachInterfaces(object nativeActiveXObject)
	{
		_axIWebBrowser2 = (MS.Win32.UnsafeNativeMethods.IWebBrowser2)nativeActiveXObject;
	}

	internal override void DetachInterfaces()
	{
		_axIWebBrowser2 = null;
	}

	internal override void CreateSink()
	{
		_cookie = new ConnectionPointCookie(_axIWebBrowser2, _hostingAdaptor.CreateEventSink(), typeof(MS.Win32.UnsafeNativeMethods.DWebBrowserEvents2));
	}

	internal override void DetachSink()
	{
		if (_cookie != null)
		{
			_cookie.Disconnect();
			_cookie = null;
		}
	}

	internal override ActiveXSite CreateActiveXSite()
	{
		return new WebBrowserSite(this);
	}

	internal override DrawingGroup GetDrawing()
	{
		return base.GetDrawing();
	}

	internal void CleanInternalState()
	{
		NavigatingToAboutBlank = false;
		DocumentStream = null;
	}

	private void LoadedHandler(object sender, RoutedEventArgs args)
	{
		PresentationSource presentationSource = PresentationSource.CriticalFromVisual(this);
		if (presentationSource != null && presentationSource.RootVisual is PopupRoot)
		{
			throw new InvalidOperationException(SR.CannotBeInsidePopup);
		}
	}

	private static void TurnOnFeatureControlKeys()
	{
		Version version = Environment.OSVersion.Version;
		if (version.Major != 5 || version.Minor != 2 || version.MajorRevision != 0)
		{
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(0, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(1, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(2, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(3, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(4, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(5, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(6, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(7, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(8, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(9, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(10, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(11, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(12, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(13, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(14, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(15, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(16, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(17, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(18, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(20, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(22, 2, fEnable: true);
			MS.Win32.UnsafeNativeMethods.CoInternetSetFeatureEnabled(25, 2, fEnable: true);
		}
	}

	private void DoNavigate(Uri source, ref object targetFrameName, ref object postData, ref object headers, bool ignoreEscaping = false)
	{
		VerifyAccess();
		MS.Win32.NativeMethods.IOleCommandTarget obj = (MS.Win32.NativeMethods.IOleCommandTarget)AxIWebBrowser2;
		object obj2 = false;
		obj.Exec(null, 23, 0, new object[1] { obj2 }, 0);
		LastNavigation = Guid.NewGuid();
		if (source == null)
		{
			NavigatingToAboutBlank = true;
			source = new Uri("about:blank");
		}
		else
		{
			CleanInternalState();
		}
		if (!source.IsAbsoluteUri)
		{
			throw new ArgumentException(SR.AbsoluteUriOnly, "source");
		}
		if (PackUriHelper.IsPackUri(source))
		{
			source = BaseUriHelper.ConvertPackUriToAbsoluteExternallyVisibleUri(source);
		}
		object flags = null;
		object URL = (ignoreEscaping ? source.AbsoluteUri : MS.Internal.Utility.BindUriHelper.UriToString(source));
		try
		{
			AxIWebBrowser2.Navigate2(ref URL, ref flags, ref targetFrameName, ref postData, ref headers);
		}
		catch (COMException ex)
		{
			CleanInternalState();
			if (ex.ErrorCode != -2147023673)
			{
				throw;
			}
		}
	}

	private void SyncUIActiveState()
	{
		if (base.ActiveXState != ActiveXHelper.ActiveXState.UIActive && HasFocusWithinCore())
		{
			Invariant.Assert(base.ActiveXState == ActiveXHelper.ActiveXState.InPlaceActive);
			base.ActiveXState = ActiveXHelper.ActiveXState.UIActive;
		}
	}

	protected override bool TranslateAcceleratorCore(ref MSG msg, ModifierKeys modifiers)
	{
		SyncUIActiveState();
		Invariant.Assert(base.ActiveXState >= ActiveXHelper.ActiveXState.UIActive, "Should be at least UIActive when we are processing accelerator keys");
		return base.ActiveXInPlaceActiveObject.TranslateAccelerator(ref msg) == 0;
	}

	protected override bool TabIntoCore(TraversalRequest request)
	{
		Invariant.Assert(base.ActiveXState >= ActiveXHelper.ActiveXState.InPlaceActive, "Should be at least InPlaceActive when tabbed into");
		bool num = DoVerb(-4);
		if (num)
		{
			base.ActiveXState = ActiveXHelper.ActiveXState.UIActive;
		}
		return num;
	}
}
