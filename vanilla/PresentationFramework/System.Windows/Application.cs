using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Threading;
using Microsoft.Win32;
using MS.Internal;
using MS.Internal.AppModel;
using MS.Internal.Interop;
using MS.Internal.IO.Packaging;
using MS.Internal.Telemetry.PresentationFramework;
using MS.Internal.Utility;
using MS.Utility;
using MS.Win32;

namespace System.Windows;

/// <summary>Encapsulates a Windows Presentation Foundation (WPF) application. </summary>
public class Application : DispatcherObject, IHaveResources, IQueryAmbient
{
	internal enum NavigationStateChange : byte
	{
		Navigating,
		Completed,
		Stopped
	}

	private static object _globalLock;

	private static bool _isShuttingDown;

	private static bool _appCreatedInThisAppDomain;

	private static Application _appInstance;

	private static Assembly _resourceAssembly;

	[ThreadStatic]
	private static Stack<NestedBamlLoadInfo> s_NestedBamlLoadInfo;

	private Uri _startupUri;

	private Uri _applicationMarkupBaseUri;

	private HybridDictionary _htProps;

	private WindowCollection _appWindowList;

	private WindowCollection _nonAppWindowList;

	private Window _mainWindow;

	private ResourceDictionary _resources;

	private bool _ownDispatcherStarted;

	private NavigationService _navService;

	private MS.Internal.SecurityCriticalDataForSet<MimeType> _appMimeType;

	private IServiceProvider _serviceProvider;

	private bool _appIsShutdown;

	private int _exitCode;

	private ShutdownMode _shutdownMode;

	private HwndWrapper _parkingHwnd;

	private HwndWrapperHook _appFilterHook;

	private EventHandlerList _events;

	private bool _hasImplicitStylesInResources;

	private static readonly object EVENT_STARTUP;

	private static readonly object EVENT_EXIT;

	private static readonly object EVENT_SESSIONENDING;

	private const SafeNativeMethods.PlaySoundFlags PLAYSOUND_FLAGS = SafeNativeMethods.PlaySoundFlags.SND_ASYNC | SafeNativeMethods.PlaySoundFlags.SND_NODEFAULT | SafeNativeMethods.PlaySoundFlags.SND_NOSTOP | SafeNativeMethods.PlaySoundFlags.SND_FILENAME;

	private const string SYSTEM_SOUNDS_REGISTRY_LOCATION = "AppEvents\\Schemes\\Apps\\Explorer\\{0}\\.current\\";

	private const string SYSTEM_SOUNDS_REGISTRY_BASE = "HKEY_CURRENT_USER\\AppEvents\\Schemes\\Apps\\Explorer\\";

	private const string SOUND_NAVIGATING = "Navigating";

	private const string SOUND_COMPLETE_NAVIGATION = "ActivatingDocument";

	/// <summary>Gets the <see cref="T:System.Windows.Application" /> object for the current <see cref="T:System.AppDomain" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Application" /> object for the current <see cref="T:System.AppDomain" />.</returns>
	public static Application Current => _appInstance;

	/// <summary>Gets the instantiated windows in an application. </summary>
	/// <returns>A <see cref="T:System.Windows.WindowCollection" /> that contains references to all window objects in the current <see cref="T:System.AppDomain" />.</returns>
	public WindowCollection Windows
	{
		get
		{
			VerifyAccess();
			return WindowsInternal.Clone();
		}
	}

	/// <summary>Gets or sets the main window of the application.</summary>
	/// <returns>A <see cref="T:System.Windows.Window" /> that is designated as the main application window.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Application.MainWindow" /> is set from an application that's hosted in a browser, such as an XAML browser applications (XBAPs).</exception>
	public Window MainWindow
	{
		get
		{
			VerifyAccess();
			return _mainWindow;
		}
		set
		{
			VerifyAccess();
			if (value != _mainWindow)
			{
				_mainWindow = value;
			}
		}
	}

	/// <summary>Gets or sets the condition that causes the <see cref="M:System.Windows.Application.Shutdown" /> method to be called.</summary>
	/// <returns>A <see cref="T:System.Windows.ShutdownMode" /> enumeration value. The default value is <see cref="F:System.Windows.ShutdownMode.OnLastWindowClose" />.</returns>
	public ShutdownMode ShutdownMode
	{
		get
		{
			VerifyAccess();
			return _shutdownMode;
		}
		set
		{
			VerifyAccess();
			if (!IsValidShutdownMode(value))
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(ShutdownMode));
			}
			if (IsShuttingDown || _appIsShutdown)
			{
				throw new InvalidOperationException(SR.ShutdownModeWhenAppShutdown);
			}
			_shutdownMode = value;
		}
	}

	/// <summary>Gets or sets a collection of application-scope resources, such as styles and brushes.</summary>
	/// <returns>A <see cref="T:System.Windows.ResourceDictionary" /> object that contains zero or more application-scope resources.</returns>
	[Ambient]
	public ResourceDictionary Resources
	{
		get
		{
			bool flag = false;
			ResourceDictionary resources;
			lock (_globalLock)
			{
				if (_resources == null)
				{
					_resources = new ResourceDictionary();
					flag = true;
				}
				resources = _resources;
			}
			if (flag)
			{
				resources.AddOwner(this);
			}
			return resources;
		}
		set
		{
			bool flag = false;
			ResourceDictionary resources;
			lock (_globalLock)
			{
				resources = _resources;
				_resources = value;
			}
			resources?.RemoveOwner(this);
			if (value != null && !value.ContainsOwner(this))
			{
				value.AddOwner(this);
			}
			if (resources != value)
			{
				flag = true;
			}
			if (flag)
			{
				InvalidateResourceReferences(new ResourcesChangeInfo(resources, value));
			}
		}
	}

	ResourceDictionary IHaveResources.Resources
	{
		get
		{
			return Resources;
		}
		set
		{
			Resources = value;
		}
	}

	internal bool HasImplicitStylesInResources
	{
		get
		{
			return _hasImplicitStylesInResources;
		}
		set
		{
			_hasImplicitStylesInResources = value;
		}
	}

	/// <summary>Gets or sets a UI that is automatically shown when an application starts.</summary>
	/// <returns>A <see cref="T:System.Uri" /> that refers to the UI that automatically opens when an application starts.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	///   <see cref="P:System.Windows.Application.StartupUri" /> is set with a value of null.</exception>
	public Uri StartupUri
	{
		get
		{
			return _startupUri;
		}
		set
		{
			VerifyAccess();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_startupUri = value;
		}
	}

	/// <summary>Gets a collection of application-scope properties.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionary" /> that contains the application-scope properties.</returns>
	public IDictionary Properties
	{
		get
		{
			lock (_globalLock)
			{
				if (_htProps == null)
				{
					_htProps = new HybridDictionary(5);
				}
				return _htProps;
			}
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Reflection.Assembly" /> that provides the pack uniform resource identifiers (URIs) for resources in a WPF application.</summary>
	/// <returns>A reference to the <see cref="T:System.Reflection.Assembly" /> that provides the pack uniform resource identifiers (URIs) for resources in a WPF application.</returns>
	/// <exception cref="T:System.InvalidOperationException">A WPF application has an entry assembly, or <see cref="P:System.Windows.Application.ResourceAssembly" /> has already been set.</exception>
	public static Assembly ResourceAssembly
	{
		get
		{
			if (_resourceAssembly == null)
			{
				lock (_globalLock)
				{
					_resourceAssembly = Assembly.GetEntryAssembly();
				}
			}
			return _resourceAssembly;
		}
		set
		{
			lock (_globalLock)
			{
				if (_resourceAssembly != value)
				{
					if (!(_resourceAssembly == null) || !(Assembly.GetEntryAssembly() == null))
					{
						throw new InvalidOperationException(SR.Format(SR.PropertyIsImmutable, "ResourceAssembly", "Application"));
					}
					_resourceAssembly = value;
					BaseUriHelper.ResourceAssembly = value;
				}
			}
		}
	}

	internal WindowCollection WindowsInternal
	{
		get
		{
			lock (_globalLock)
			{
				if (_appWindowList == null)
				{
					_appWindowList = new WindowCollection();
				}
				return _appWindowList;
			}
		}
		private set
		{
			lock (_globalLock)
			{
				_appWindowList = value;
			}
		}
	}

	internal WindowCollection NonAppWindowsInternal
	{
		get
		{
			lock (_globalLock)
			{
				if (_nonAppWindowList == null)
				{
					_nonAppWindowList = new WindowCollection();
				}
				return _nonAppWindowList;
			}
		}
		private set
		{
			lock (_globalLock)
			{
				_nonAppWindowList = value;
			}
		}
	}

	internal MimeType MimeType
	{
		get
		{
			return _appMimeType.Value;
		}
		set
		{
			_appMimeType = new MS.Internal.SecurityCriticalDataForSet<MimeType>(value);
		}
	}

	internal IServiceProvider ServiceProvider
	{
		private get
		{
			VerifyAccess();
			if (_serviceProvider != null)
			{
				return _serviceProvider;
			}
			return null;
		}
		set
		{
			VerifyAccess();
			_serviceProvider = value;
		}
	}

	internal NavigationService NavService
	{
		get
		{
			VerifyAccess();
			return _navService;
		}
		set
		{
			VerifyAccess();
			_navService = value;
		}
	}

	internal static bool IsShuttingDown
	{
		get
		{
			if (_isShuttingDown)
			{
				return _isShuttingDown;
			}
			return false;
		}
		set
		{
			lock (_globalLock)
			{
				_isShuttingDown = value;
			}
		}
	}

	internal static bool IsApplicationObjectShuttingDown => _isShuttingDown;

	internal nint ParkingHwnd
	{
		get
		{
			if (_parkingHwnd != null)
			{
				return _parkingHwnd.Handle;
			}
			return IntPtr.Zero;
		}
	}

	internal Uri ApplicationMarkupBaseUri
	{
		get
		{
			if (_applicationMarkupBaseUri == null)
			{
				_applicationMarkupBaseUri = BaseUriHelper.BaseUri;
			}
			return _applicationMarkupBaseUri;
		}
		set
		{
			_applicationMarkupBaseUri = value;
		}
	}

	private EventHandlerList Events
	{
		get
		{
			if (_events == null)
			{
				_events = new EventHandlerList();
			}
			return _events;
		}
	}

	/// <summary>Occurs when the <see cref="M:System.Windows.Application.Run" /> method of the <see cref="T:System.Windows.Application" /> object is called.</summary>
	public event StartupEventHandler Startup
	{
		add
		{
			VerifyAccess();
			Events.AddHandler(EVENT_STARTUP, value);
		}
		remove
		{
			VerifyAccess();
			Events.RemoveHandler(EVENT_STARTUP, value);
		}
	}

	/// <summary>Occurs just before an application shuts down, and cannot be canceled.</summary>
	public event ExitEventHandler Exit
	{
		add
		{
			VerifyAccess();
			Events.AddHandler(EVENT_EXIT, value);
		}
		remove
		{
			VerifyAccess();
			Events.RemoveHandler(EVENT_EXIT, value);
		}
	}

	/// <summary>Occurs when an application becomes the foreground application.</summary>
	public event EventHandler Activated;

	/// <summary>Occurs when an application stops being the foreground application.</summary>
	public event EventHandler Deactivated;

	/// <summary>Occurs when the user ends the Windows session by logging off or shutting down the operating system.</summary>
	public event SessionEndingCancelEventHandler SessionEnding
	{
		add
		{
			VerifyAccess();
			Events.AddHandler(EVENT_SESSIONENDING, value);
		}
		remove
		{
			VerifyAccess();
			Events.RemoveHandler(EVENT_SESSIONENDING, value);
		}
	}

	/// <summary>Occurs when an exception is thrown by an application but not handled.</summary>
	public event DispatcherUnhandledExceptionEventHandler DispatcherUnhandledException
	{
		add
		{
			base.Dispatcher.Invoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate
			{
				base.Dispatcher.UnhandledException += value;
				return (object)null;
			}, null);
		}
		remove
		{
			base.Dispatcher.Invoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate
			{
				base.Dispatcher.UnhandledException -= value;
				return (object)null;
			}, null);
		}
	}

	/// <summary>Occurs when a new navigation is requested by a navigator in the application.</summary>
	public event NavigatingCancelEventHandler Navigating;

	/// <summary>Occurs when the content that is being navigated to by a navigator in the application has been found, although it may not have completed loading.</summary>
	public event NavigatedEventHandler Navigated;

	/// <summary>Occurs periodically during a download that is being managed by a navigator in the application to provide navigation progress information.</summary>
	public event NavigationProgressEventHandler NavigationProgress;

	/// <summary>Occurs when an error occurs while a navigator in the application is navigating to the requested content.</summary>
	public event NavigationFailedEventHandler NavigationFailed;

	/// <summary>Occurs when content that was navigated to by a navigator in the application has been loaded, parsed, and has begun rendering.</summary>
	public event LoadCompletedEventHandler LoadCompleted;

	/// <summary>Occurs when the StopLoading method of a navigator in the application is called, or when a new navigation is requested by a navigator while a current navigation is in progress.</summary>
	public event NavigationStoppedEventHandler NavigationStopped;

	/// <summary>Occurs when a navigator in the application begins navigation to a content fragment, Navigation occurs immediately if the desired fragment is in the current content, or after the source XAML content has been loaded if the desired fragment is in different content.</summary>
	public event FragmentNavigationEventHandler FragmentNavigation;

	static Application()
	{
		s_NestedBamlLoadInfo = null;
		EVENT_STARTUP = new object();
		EVENT_EXIT = new object();
		EVENT_SESSIONENDING = new object();
		ApplicationInit();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Application" /> class.</summary>
	/// <exception cref="T:System.InvalidOperationException">More than one instance of the <see cref="T:System.Windows.Application" /> class is created per <see cref="T:System.AppDomain" />.</exception>
	public Application()
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordGeneral | EventTrace.Keyword.KeywordPerf, EventTrace.Event.WClientAppCtor);
		lock (_globalLock)
		{
			if (_appCreatedInThisAppDomain)
			{
				throw new InvalidOperationException(SR.MultiSingleton);
			}
			_appInstance = this;
			IsShuttingDown = false;
			_appCreatedInThisAppDomain = true;
		}
		base.Dispatcher.BeginInvoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate
		{
			if (IsShuttingDown)
			{
				return (object)null;
			}
			StartupEventArgs startupEventArgs = new StartupEventArgs();
			OnStartup(startupEventArgs);
			if (startupEventArgs.PerformDefaultAction)
			{
				DoStartup();
			}
			return (object)null;
		}, null);
	}

	/// <summary>Starts a Windows Presentation Foundation (WPF) application.</summary>
	/// <returns>The <see cref="T:System.Int32" /> application exit code that is returned to the operating system when the application shuts down. By default, the exit code value is 0.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Windows.Application.Run" /> is called from a browser-hosted application (for example, an XAML browser application (XBAP)).</exception>
	public int Run()
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordGeneral | EventTrace.Keyword.KeywordPerf, EventTrace.Event.WClientAppRun);
		return Run(null);
	}

	/// <summary>Starts a Windows Presentation Foundation (WPF) application and opens the specified window.</summary>
	/// <returns>The <see cref="T:System.Int32" /> application exit code that is returned to the operating system when the application shuts down. By default, the exit code value is 0.</returns>
	/// <param name="window">A <see cref="T:System.Windows.Window" /> that opens automatically when an application starts.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Windows.Application.Run" /> is called from a browser-hosted application (for example, an XAML browser application (XBAP)).</exception>
	public int Run(Window window)
	{
		VerifyAccess();
		return RunInternal(window);
	}

	internal object GetService(Type serviceType)
	{
		VerifyAccess();
		object result = null;
		if (ServiceProvider != null)
		{
			result = ServiceProvider.GetService(serviceType);
		}
		return result;
	}

	/// <summary>Shuts down an application.</summary>
	public void Shutdown()
	{
		Shutdown(0);
	}

	/// <summary>Shuts down an application that returns the specified exit code to the operating system.</summary>
	/// <param name="exitCode">An integer exit code for an application. The default exit code is 0.</param>
	public void Shutdown(int exitCode)
	{
		CriticalShutdown(exitCode);
	}

	internal void CriticalShutdown(int exitCode)
	{
		VerifyAccess();
		if (!IsShuttingDown)
		{
			ControlsTraceLogger.LogUsedControlsDetails();
			SetExitCode(exitCode);
			IsShuttingDown = true;
			base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(ShutdownCallback), null);
		}
	}

	/// <summary>Searches for a user interface (UI) resource, such as a <see cref="T:System.Windows.Style" /> or <see cref="T:System.Windows.Media.Brush" />, with the specified key, and throws an exception if the requested resource is not found (see XAML Resources).</summary>
	/// <returns>The requested resource object. If the requested resource is not found, a <see cref="T:System.Windows.ResourceReferenceKeyNotFoundException" /> is thrown.</returns>
	/// <param name="resourceKey">The name of the resource to find.</param>
	/// <exception cref="T:System.Windows.ResourceReferenceKeyNotFoundException">The resource cannot be found.</exception>
	public object FindResource(object resourceKey)
	{
		ResourceDictionary resources = _resources;
		object obj = null;
		if (resources != null)
		{
			obj = resources[resourceKey];
		}
		if (obj == DependencyProperty.UnsetValue || obj == null)
		{
			obj = SystemResources.FindResourceInternal(resourceKey);
		}
		if (obj == null)
		{
			Helper.ResourceFailureThrow(resourceKey);
		}
		return obj;
	}

	/// <summary>Searches for the specified resource.</summary>
	/// <returns>The requested resource object. If the requested resource is not found, a null reference is returned.</returns>
	/// <param name="resourceKey">The name of the resource to find.</param>
	public object TryFindResource(object resourceKey)
	{
		ResourceDictionary resources = _resources;
		object obj = null;
		if (resources != null)
		{
			obj = resources[resourceKey];
		}
		if (obj == DependencyProperty.UnsetValue || obj == null)
		{
			obj = SystemResources.FindResourceInternal(resourceKey);
		}
		return obj;
	}

	internal object FindResourceInternal(object resourceKey)
	{
		return FindResourceInternal(resourceKey, allowDeferredResourceReference: false, mustReturnDeferredResourceReference: false);
	}

	internal object FindResourceInternal(object resourceKey, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference)
	{
		bool canCache;
		return _resources?.FetchResource(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference, out canCache);
	}

	/// <summary>Loads a XAML file that is located at the specified uniform resource identifier (URI) and converts it to an instance of the object that is specified by the root element of the XAML file.</summary>
	/// <param name="component">An object of the same type as the root element of the XAML file.</param>
	/// <param name="resourceLocator">A <see cref="T:System.Uri" /> that maps to a relative XAML file.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="component" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="resourceLocator" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Uri.OriginalString" /> property of the <paramref name="resourceLocator" /><see cref="T:System.Uri" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="resourceLocator" /> is an absolute URI.</exception>
	/// <exception cref="T:System.Exception">
	///   <paramref name="component" /> is of a type that does not match the root element of the XAML file.</exception>
	public static void LoadComponent(object component, Uri resourceLocator)
	{
		if (component == null)
		{
			throw new ArgumentNullException("component");
		}
		if (resourceLocator == null)
		{
			throw new ArgumentNullException("resourceLocator");
		}
		if (resourceLocator.OriginalString == null)
		{
			throw new ArgumentException(SR.Format(SR.ArgumentPropertyMustNotBeNull, "resourceLocator", "OriginalString"));
		}
		if (resourceLocator.IsAbsoluteUri)
		{
			throw new ArgumentException(SR.AbsoluteUriNotAllowed);
		}
		Uri uri = new Uri(BaseUriHelper.PackAppBaseUri, resourceLocator);
		ParserContext parserContext = new ParserContext();
		parserContext.BaseUri = uri;
		bool flag = true;
		Stream stream = null;
		if (IsComponentBeingLoadedFromOuterLoadBaml(uri))
		{
			NestedBamlLoadInfo nestedBamlLoadInfo = s_NestedBamlLoadInfo.Peek();
			stream = nestedBamlLoadInfo.BamlStream;
			stream.Seek(0L, SeekOrigin.Begin);
			parserContext.SkipJournaledProperties = nestedBamlLoadInfo.SkipJournaledProperties;
			nestedBamlLoadInfo.BamlUri = null;
			flag = false;
		}
		else
		{
			PackagePart resourceOrContentPart = GetResourceOrContentPart(resourceLocator);
			ContentType contentType = new ContentType(resourceOrContentPart.ContentType);
			stream = resourceOrContentPart.GetSeekableStream();
			flag = true;
			if (!MimeTypeMapper.BamlMime.AreTypeAndSubTypeEqual(contentType))
			{
				throw new Exception(SR.Format(SR.ContentTypeNotSupported, contentType));
			}
		}
		if (!(stream is IStreamInfo streamInfo) || streamInfo.Assembly != component.GetType().Assembly)
		{
			throw new Exception(SR.Format(SR.UriNotMatchWithRootType, component.GetType(), resourceLocator));
		}
		XamlReader.LoadBaml(stream, parserContext, component, flag);
	}

	/// <summary>Loads a XAML file that is located at the specified uniform resource identifier (URI), and converts it to an instance of the object that is specified by the root element of the XAML file.</summary>
	/// <returns>An instance of the root element specified by the XAML file loaded. </returns>
	/// <param name="resourceLocator">A <see cref="T:System.Uri" /> that maps to a relative XAML file.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="resourceLocator" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Uri.OriginalString" /> property of the <paramref name="resourceLocator" /><see cref="T:System.Uri" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="resourceLocator" /> is an absolute URI.</exception>
	/// <exception cref="T:System.Exception">The file is not a XAML file.</exception>
	public static object LoadComponent(Uri resourceLocator)
	{
		if (resourceLocator == null)
		{
			throw new ArgumentNullException("resourceLocator");
		}
		if (resourceLocator.OriginalString == null)
		{
			throw new ArgumentException(SR.Format(SR.ArgumentPropertyMustNotBeNull, "resourceLocator", "OriginalString"));
		}
		if (resourceLocator.IsAbsoluteUri)
		{
			throw new ArgumentException(SR.AbsoluteUriNotAllowed);
		}
		return LoadComponent(resourceLocator, bSkipJournaledProperties: false);
	}

	internal static object LoadComponent(Uri resourceLocator, bool bSkipJournaledProperties)
	{
		Uri resolvedUri = MS.Internal.Utility.BindUriHelper.GetResolvedUri(BaseUriHelper.PackAppBaseUri, resourceLocator);
		PackagePart resourceOrContentPart = GetResourceOrContentPart(resolvedUri);
		ContentType contentType = new ContentType(resourceOrContentPart.ContentType);
		Stream seekableStream = resourceOrContentPart.GetSeekableStream();
		ParserContext parserContext = new ParserContext();
		parserContext.BaseUri = resolvedUri;
		parserContext.SkipJournaledProperties = bSkipJournaledProperties;
		if (MimeTypeMapper.BamlMime.AreTypeAndSubTypeEqual(contentType))
		{
			return LoadBamlStreamWithSyncInfo(seekableStream, parserContext);
		}
		if (MimeTypeMapper.XamlMime.AreTypeAndSubTypeEqual(contentType))
		{
			return XamlReader.Load(seekableStream, parserContext);
		}
		throw new Exception(SR.Format(SR.ContentTypeNotSupported, contentType.ToString()));
	}

	internal static object LoadBamlStreamWithSyncInfo(Stream stream, ParserContext pc)
	{
		object obj = null;
		if (s_NestedBamlLoadInfo == null)
		{
			s_NestedBamlLoadInfo = new Stack<NestedBamlLoadInfo>();
		}
		NestedBamlLoadInfo item = new NestedBamlLoadInfo(pc.BaseUri, stream, pc.SkipJournaledProperties);
		s_NestedBamlLoadInfo.Push(item);
		try
		{
			return XamlReader.LoadBaml(stream, pc, null, closeStream: true);
		}
		finally
		{
			s_NestedBamlLoadInfo.Pop();
			if (s_NestedBamlLoadInfo.Count == 0)
			{
				s_NestedBamlLoadInfo = null;
			}
		}
	}

	/// <summary>Returns a resource stream for a resource data file that is located at the specified <see cref="T:System.Uri" /> (see WPF Application Resource, Content, and Data Files).</summary>
	/// <returns>A <see cref="T:System.Windows.Resources.StreamResourceInfo" /> that contains a resource stream for resource data file that is located at the specified <see cref="T:System.Uri" />. </returns>
	/// <param name="uriResource">The <see cref="T:System.Uri" /> that maps to an embedded resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Uri" /> that is passed to <see cref="M:System.Windows.Application.GetResourceStream(System.Uri)" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Uri.OriginalString" /> property of the <see cref="T:System.Uri" /> that is passed to <see cref="M:System.Windows.Application.GetResourceStream(System.Uri)" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Uri" /> that is passed to <see cref="M:System.Windows.Application.GetResourceStream(System.Uri)" /> is either not relative, or is absolute but not in the pack://application:,,,/ form.</exception>
	/// <exception cref="T:System.IO.IOException">The <see cref="T:System.Uri" /> that is passed to <see cref="M:System.Windows.Application.GetResourceStream(System.Uri)" /> cannot be found.</exception>
	public static StreamResourceInfo GetResourceStream(Uri uriResource)
	{
		if (uriResource == null)
		{
			throw new ArgumentNullException("uriResource");
		}
		if (uriResource.OriginalString == null)
		{
			throw new ArgumentException(SR.Format(SR.ArgumentPropertyMustNotBeNull, "uriResource", "OriginalString"));
		}
		if (uriResource.IsAbsoluteUri && !BaseUriHelper.IsPackApplicationUri(uriResource))
		{
			throw new ArgumentException(SR.NonPackAppAbsoluteUriNotAllowed);
		}
		if (GetResourceOrContentPart(uriResource) is ResourcePart resourcePart)
		{
			return new StreamResourceInfo(resourcePart.GetSeekableStream(), resourcePart.ContentType);
		}
		return null;
	}

	/// <summary>Returns a resource stream for a content data file that is located at the specified <see cref="T:System.Uri" /> (see WPF Application Resource, Content, and Data Files).</summary>
	/// <returns>A <see cref="T:System.Windows.Resources.StreamResourceInfo" /> that contains a content data file that is located at the specified <see cref="T:System.Uri" />. If a loose resource is not found, null is returned.</returns>
	/// <param name="uriContent">The relative <see cref="T:System.Uri" /> that maps to a loose resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Uri" /> that is passed to <see cref="M:System.Windows.Application.GetContentStream(System.Uri)" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Uri.OriginalString" /> property of the <see cref="T:System.Uri" /> that is passed to <see cref="M:System.Windows.Application.GetContentStream(System.Uri)" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Uri" /> that is passed to <see cref="M:System.Windows.Application.GetContentStream(System.Uri)" /> is an absolute <see cref="T:System.Uri" />.</exception>
	public static StreamResourceInfo GetContentStream(Uri uriContent)
	{
		if (uriContent == null)
		{
			throw new ArgumentNullException("uriContent");
		}
		if (uriContent.OriginalString == null)
		{
			throw new ArgumentException(SR.Format(SR.ArgumentPropertyMustNotBeNull, "uriContent", "OriginalString"));
		}
		if (uriContent.IsAbsoluteUri && !BaseUriHelper.IsPackApplicationUri(uriContent))
		{
			throw new ArgumentException(SR.NonPackAppAbsoluteUriNotAllowed);
		}
		if (GetResourceOrContentPart(uriContent) is ContentFilePart contentFilePart)
		{
			return new StreamResourceInfo(contentFilePart.GetSeekableStream(), contentFilePart.ContentType);
		}
		return null;
	}

	/// <summary>Returns a resource stream for a site-of-origin data file that is located at the specified <see cref="T:System.Uri" /> (see WPF Application Resource, Content, and Data Files).</summary>
	/// <returns>A <see cref="T:System.Windows.Resources.StreamResourceInfo" /> that contains a resource stream for a site-of-origin data file that is located at the specified <see cref="T:System.Uri" />. If the loose resource is not found, null is returned.</returns>
	/// <param name="uriRemote">The <see cref="T:System.Uri" /> that maps to a loose resource at the site of origin.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Uri" /> that is passed to <see cref="M:System.Windows.Application.GetRemoteStream(System.Uri)" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Uri.OriginalString" /> property of the <see cref="T:System.Uri" /> that is passed to <see cref="M:System.Windows.Application.GetRemoteStream(System.Uri)" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Uri" /> that is passed to <see cref="M:System.Windows.Application.GetRemoteStream(System.Uri)" /> is either not relative, or is absolute but not in the pack://siteoforigin:,,,/ form.</exception>
	public static StreamResourceInfo GetRemoteStream(Uri uriRemote)
	{
		SiteOfOriginPart siteOfOriginPart = null;
		if (uriRemote == null)
		{
			throw new ArgumentNullException("uriRemote");
		}
		if (uriRemote.OriginalString == null)
		{
			throw new ArgumentException(SR.Format(SR.ArgumentPropertyMustNotBeNull, "uriRemote", "OriginalString"));
		}
		if (uriRemote.IsAbsoluteUri && !BaseUriHelper.SiteOfOriginBaseUri.IsBaseOf(uriRemote))
		{
			throw new ArgumentException(SR.NonPackSooAbsoluteUriNotAllowed);
		}
		Uri resolvedUri = MS.Internal.Utility.BindUriHelper.GetResolvedUri(BaseUriHelper.SiteOfOriginBaseUri, uriRemote);
		Uri packageUri = System.IO.Packaging.PackUriHelper.GetPackageUri(resolvedUri);
		Uri partUri = System.IO.Packaging.PackUriHelper.GetPartUri(resolvedUri);
		SiteOfOriginContainer siteOfOriginContainer = (SiteOfOriginContainer)GetResourcePackage(packageUri);
		lock (siteOfOriginContainer)
		{
			siteOfOriginPart = siteOfOriginContainer.GetPart(partUri) as SiteOfOriginPart;
		}
		Stream stream = null;
		if (siteOfOriginPart != null)
		{
			try
			{
				stream = siteOfOriginPart.GetSeekableStream();
				if (stream == null)
				{
					siteOfOriginPart = null;
				}
			}
			catch (WebException)
			{
				siteOfOriginPart = null;
			}
		}
		if (stream != null)
		{
			return new StreamResourceInfo(stream, siteOfOriginPart.ContentType);
		}
		return null;
	}

	/// <summary>Retrieves a cookie for the location specified by a <see cref="T:System.Uri" />.</summary>
	/// <returns>A <see cref="T:System.String" /> value, if the cookie exists; otherwise, a <see cref="T:System.ComponentModel.Win32Exception" /> is thrown.</returns>
	/// <param name="uri">The <see cref="T:System.Uri" /> that specifies the location for which a cookie was created.</param>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A Win32 error is raised by the InternetGetCookie function (called by <see cref="M:System.Windows.Application.GetCookie(System.Uri)" />) if a problem occurs when attempting to retrieve the specified cookie.</exception>
	public static string GetCookie(Uri uri)
	{
		return CookieHandler.GetCookie(uri, throwIfNoCookie: true);
	}

	/// <summary>Creates a cookie for the location specified by a <see cref="T:System.Uri" />.</summary>
	/// <param name="uri">The <see cref="T:System.Uri" /> that specifies the location for which the cookie should be created.</param>
	/// <param name="value">The <see cref="T:System.String" /> that contains the cookie data.</param>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A Win32 error is raised by the InternetSetCookie function (called by <see cref="M:System.Windows.Application.SetCookie(System.Uri,System.String)" />) if a problem occurs when attempting to create the specified cookie.</exception>
	public static void SetCookie(Uri uri, string value)
	{
		CookieHandler.SetCookie(uri, value);
	}

	/// <summary>Queries for whether a specified ambient property is available in the current scope.</summary>
	/// <returns>true if the requested ambient property is available; otherwise, false.</returns>
	/// <param name="propertyName">The name of the requested ambient property.</param>
	bool IQueryAmbient.IsAmbientPropertyAvailable(string propertyName)
	{
		if (propertyName == "Resources")
		{
			return _resources != null;
		}
		return false;
	}

	/// <summary>Raises the <see cref="E:System.Windows.Application.Startup" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
	protected virtual void OnStartup(StartupEventArgs e)
	{
		VerifyAccess();
		((StartupEventHandler)Events[EVENT_STARTUP])?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Application.Exit" /> event.</summary>
	/// <param name="e">An <see cref="T:System.Windows.ExitEventArgs" /> that contains the event data.</param>
	protected virtual void OnExit(ExitEventArgs e)
	{
		VerifyAccess();
		((ExitEventHandler)Events[EVENT_EXIT])?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Application.Activated" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
	protected virtual void OnActivated(EventArgs e)
	{
		VerifyAccess();
		if (this.Activated != null)
		{
			this.Activated(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Application.Deactivated" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	protected virtual void OnDeactivated(EventArgs e)
	{
		VerifyAccess();
		if (this.Deactivated != null)
		{
			this.Deactivated(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Application.SessionEnding" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.SessionEndingCancelEventArgs" /> that contains the event data.</param>
	protected virtual void OnSessionEnding(SessionEndingCancelEventArgs e)
	{
		VerifyAccess();
		((SessionEndingCancelEventHandler)Events[EVENT_SESSIONENDING])?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Application.Navigating" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Navigation.NavigatingCancelEventArgs" /> that contains the event data.</param>
	protected virtual void OnNavigating(NavigatingCancelEventArgs e)
	{
		VerifyAccess();
		if (this.Navigating != null)
		{
			this.Navigating(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Application.Navigated" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Navigation.NavigationEventArgs" /> that contains the event data.</param>
	protected virtual void OnNavigated(NavigationEventArgs e)
	{
		VerifyAccess();
		if (this.Navigated != null)
		{
			this.Navigated(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Application.NavigationProgress" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Navigation.NavigationProgressEventArgs" /> that contains the event data.</param>
	protected virtual void OnNavigationProgress(NavigationProgressEventArgs e)
	{
		VerifyAccess();
		if (this.NavigationProgress != null)
		{
			this.NavigationProgress(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Application.NavigationFailed" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Navigation.NavigationFailedEventArgs" /> that contains the event data.</param>
	protected virtual void OnNavigationFailed(NavigationFailedEventArgs e)
	{
		VerifyAccess();
		if (this.NavigationFailed != null)
		{
			this.NavigationFailed(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Application.LoadCompleted" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Navigation.NavigationEventArgs" /> that contains the event data.</param>
	protected virtual void OnLoadCompleted(NavigationEventArgs e)
	{
		VerifyAccess();
		if (this.LoadCompleted != null)
		{
			this.LoadCompleted(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Application.NavigationStopped" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Navigation.NavigationEventArgs" /> that contains the event data. </param>
	protected virtual void OnNavigationStopped(NavigationEventArgs e)
	{
		VerifyAccess();
		if (this.NavigationStopped != null)
		{
			this.NavigationStopped(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Application.FragmentNavigation" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Navigation.FragmentNavigationEventArgs" /> that contains the event data.</param>
	protected virtual void OnFragmentNavigation(FragmentNavigationEventArgs e)
	{
		VerifyAccess();
		if (this.FragmentNavigation != null)
		{
			this.FragmentNavigation(this, e);
		}
	}

	internal virtual void PerformNavigationStateChangeTasks(bool isNavigationInitiator, bool playNavigatingSound, NavigationStateChange state)
	{
		if (!isNavigationInitiator)
		{
			return;
		}
		switch (state)
		{
		case NavigationStateChange.Navigating:
			if (playNavigatingSound)
			{
				PlaySound("Navigating");
			}
			break;
		case NavigationStateChange.Completed:
			PlaySound("ActivatingDocument");
			break;
		case NavigationStateChange.Stopped:
			break;
		}
	}

	internal void DoStartup()
	{
		if (!(StartupUri != null))
		{
			return;
		}
		if (!StartupUri.IsAbsoluteUri)
		{
			StartupUri = new Uri(ApplicationMarkupBaseUri, StartupUri);
		}
		if (BaseUriHelper.IsPackApplicationUri(StartupUri))
		{
			NavigatingCancelEventArgs navigatingCancelEventArgs = new NavigatingCancelEventArgs(MS.Internal.Utility.BindUriHelper.GetUriRelativeToPackAppBase(StartupUri), null, null, null, NavigationMode.New, null, null, isNavInitiator: true);
			FireNavigating(navigatingCancelEventArgs, isInitialNavigation: true);
			if (!navigatingCancelEventArgs.Cancel)
			{
				object root = LoadComponent(StartupUri, bSkipJournaledProperties: false);
				ConfigAppWindowAndRootElement(root, StartupUri);
			}
		}
		else
		{
			NavService = new NavigationService(null);
			NavService.AllowWindowNavigation = true;
			NavService.PreBPReady += OnPreBPReady;
			NavService.Navigate(StartupUri);
		}
	}

	internal virtual void DoShutdown()
	{
		while (WindowsInternal.Count > 0)
		{
			if (!WindowsInternal[0].IsDisposed)
			{
				WindowsInternal[0].InternalClose(shutdown: true, ignoreCancel: true);
			}
			else
			{
				WindowsInternal.RemoveAt(0);
			}
		}
		WindowsInternal = null;
		ExitEventArgs exitEventArgs = new ExitEventArgs(_exitCode);
		try
		{
			OnExit(exitEventArgs);
		}
		finally
		{
			SetExitCode(exitEventArgs._exitCode);
			lock (_globalLock)
			{
				_appInstance = null;
			}
			_mainWindow = null;
			_htProps = null;
			NonAppWindowsInternal = null;
			if (_parkingHwnd != null)
			{
				_parkingHwnd.Dispose();
			}
			if (_events != null)
			{
				_events.Dispose();
			}
			PreloadedPackages.Clear();
			AppSecurityManager.ClearSecurityManager();
			_appIsShutdown = true;
		}
	}

	internal int RunInternal(Window window)
	{
		VerifyAccess();
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordGeneral | EventTrace.Keyword.KeywordPerf, EventTrace.Event.WClientAppRun);
		if (_appIsShutdown)
		{
			throw new InvalidOperationException(SR.Format(SR.CannotCallRunMultipleTimes, GetType().FullName));
		}
		if (window != null)
		{
			if (!window.CheckAccess())
			{
				throw new ArgumentException(SR.Format(SR.WindowPassedShouldBeOnApplicationThread, window.GetType().FullName, GetType().FullName));
			}
			if (!WindowsInternal.HasItem(window))
			{
				WindowsInternal.Add(window);
			}
			if (MainWindow == null)
			{
				MainWindow = window;
			}
			if (window.Visibility != 0)
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate(object obj)
				{
					(obj as Window).Show();
					return (object)null;
				}, window);
			}
		}
		EnsureHwndSource();
		RunDispatcher(null);
		return _exitCode;
	}

	internal void InvalidateResourceReferences(ResourcesChangeInfo info)
	{
		InvalidateResourceReferenceOnWindowCollection(WindowsInternal.Clone(), info);
		InvalidateResourceReferenceOnWindowCollection(NonAppWindowsInternal.Clone(), info);
	}

	internal NavigationWindow GetAppWindow()
	{
		NavigationWindow navigationWindow = new NavigationWindow();
		new WindowInteropHelper(navigationWindow).EnsureHandle();
		return navigationWindow;
	}

	internal void FireNavigating(NavigatingCancelEventArgs e, bool isInitialNavigation)
	{
		PerformNavigationStateChangeTasks(e.IsNavigationInitiator, !isInitialNavigation, NavigationStateChange.Navigating);
		OnNavigating(e);
	}

	internal void FireNavigated(NavigationEventArgs e)
	{
		OnNavigated(e);
	}

	internal void FireNavigationProgress(NavigationProgressEventArgs e)
	{
		OnNavigationProgress(e);
	}

	internal void FireNavigationFailed(NavigationFailedEventArgs e)
	{
		PerformNavigationStateChangeTasks(isNavigationInitiator: true, playNavigatingSound: false, NavigationStateChange.Stopped);
		OnNavigationFailed(e);
	}

	internal void FireLoadCompleted(NavigationEventArgs e)
	{
		PerformNavigationStateChangeTasks(e.IsNavigationInitiator, playNavigatingSound: false, NavigationStateChange.Completed);
		OnLoadCompleted(e);
	}

	internal void FireNavigationStopped(NavigationEventArgs e)
	{
		PerformNavigationStateChangeTasks(e.IsNavigationInitiator, playNavigatingSound: false, NavigationStateChange.Stopped);
		OnNavigationStopped(e);
	}

	internal void FireFragmentNavigation(FragmentNavigationEventArgs e)
	{
		OnFragmentNavigation(e);
	}

	private static void ApplicationInit()
	{
		_globalLock = new object();
		PreloadedPackages.AddPackage(System.IO.Packaging.PackUriHelper.GetPackageUri(BaseUriHelper.PackAppBaseUri), new ResourceContainer(), threadSafe: true);
		MimeObjectFactory.RegisterCore(MimeTypeMapper.BamlMime, AppModelKnownContentFactory.BamlConverterCore);
		StreamToObjectFactoryDelegateCore method = AppModelKnownContentFactory.XamlConverterCore;
		MimeObjectFactory.RegisterCore(MimeTypeMapper.XamlMime, method);
		MimeObjectFactory.RegisterCore(MimeTypeMapper.FixedDocumentMime, method);
		MimeObjectFactory.RegisterCore(MimeTypeMapper.FixedDocumentSequenceMime, method);
		MimeObjectFactory.RegisterCore(MimeTypeMapper.FixedPageMime, method);
		MimeObjectFactory.RegisterCore(MimeTypeMapper.ResourceDictionaryMime, method);
		StreamToObjectFactoryDelegateCore method2 = AppModelKnownContentFactory.HtmlXappConverterCore;
		MimeObjectFactory.RegisterCore(MimeTypeMapper.HtmMime, method2);
		MimeObjectFactory.RegisterCore(MimeTypeMapper.HtmlMime, method2);
		MimeObjectFactory.RegisterCore(MimeTypeMapper.XbapMime, method2);
	}

	private static PackagePart GetResourceOrContentPart(Uri uri)
	{
		Uri resolvedUri = MS.Internal.Utility.BindUriHelper.GetResolvedUri(BaseUriHelper.PackAppBaseUri, uri);
		Uri packageUri = System.IO.Packaging.PackUriHelper.GetPackageUri(resolvedUri);
		Uri partUri = System.IO.Packaging.PackUriHelper.GetPartUri(resolvedUri);
		ResourceContainer resourceContainer = (ResourceContainer)GetResourcePackage(packageUri);
		PackagePart packagePart = null;
		lock (resourceContainer)
		{
			return resourceContainer.GetPart(partUri);
		}
	}

	private static Package GetResourcePackage(Uri packageUri)
	{
		Package package = PreloadedPackages.GetPackage(packageUri);
		if (package == null)
		{
			Uri uri = System.IO.Packaging.PackUriHelper.Create(packageUri);
			Invariant.Assert(uri == BaseUriHelper.PackAppBaseUri || uri == BaseUriHelper.SiteOfOriginBaseUri, "Unknown packageUri passed: " + packageUri);
			Invariant.Assert(IsApplicationObjectShuttingDown);
			throw new InvalidOperationException(SR.ApplicationShuttingDown);
		}
		return package;
	}

	private void EnsureHwndSource()
	{
		if (_parkingHwnd == null)
		{
			_appFilterHook = AppFilterMessage;
			HwndWrapperHook[] hooks = new HwndWrapperHook[1] { _appFilterHook };
			_parkingHwnd = new HwndWrapper(0, 0, 0, 0, 0, 0, 0, "", IntPtr.Zero, hooks);
		}
	}

	private nint AppFilterMessage(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		nint refInt = IntPtr.Zero;
		switch ((WindowMessage)msg)
		{
		case WindowMessage.WM_ACTIVATEAPP:
			handled = WmActivateApp(MS.Win32.NativeMethods.IntPtrToInt32(wParam));
			break;
		case WindowMessage.WM_QUERYENDSESSION:
			handled = WmQueryEndSession(lParam, ref refInt);
			break;
		default:
			handled = false;
			break;
		}
		return refInt;
	}

	private bool WmActivateApp(int wParam)
	{
		if (wParam != 0)
		{
			OnActivated(EventArgs.Empty);
		}
		else
		{
			OnDeactivated(EventArgs.Empty);
		}
		return false;
	}

	private bool WmQueryEndSession(nint lParam, ref nint refInt)
	{
		int num = MS.Win32.NativeMethods.IntPtrToInt32(lParam);
		bool flag = false;
		SessionEndingCancelEventArgs sessionEndingCancelEventArgs = new SessionEndingCancelEventArgs(((num & int.MinValue) == 0) ? ReasonSessionEnding.Shutdown : ReasonSessionEnding.Logoff);
		OnSessionEnding(sessionEndingCancelEventArgs);
		if (!sessionEndingCancelEventArgs.Cancel)
		{
			Shutdown();
			refInt = new IntPtr(1);
			return false;
		}
		refInt = IntPtr.Zero;
		return true;
	}

	private void InvalidateResourceReferenceOnWindowCollection(WindowCollection wc, ResourcesChangeInfo info)
	{
		bool hasImplicitStyles = info.IsResourceAddOperation && HasImplicitStylesInResources;
		for (int i = 0; i < wc.Count; i++)
		{
			if (wc[i].CheckAccess())
			{
				if (hasImplicitStyles)
				{
					wc[i].ShouldLookupImplicitStyles = true;
				}
				TreeWalkHelper.InvalidateOnResourcesChange(wc[i], null, info);
				continue;
			}
			wc[i].Dispatcher.BeginInvoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate(object obj)
			{
				object[] array = obj as object[];
				if (hasImplicitStyles)
				{
					((FrameworkElement)array[0]).ShouldLookupImplicitStyles = true;
				}
				TreeWalkHelper.InvalidateOnResourcesChange((FrameworkElement)array[0], null, (ResourcesChangeInfo)array[1]);
				return (object)null;
			}, new object[2]
			{
				wc[i],
				info
			});
		}
	}

	private void SetExitCode(int exitCode)
	{
		if (_exitCode != exitCode)
		{
			_exitCode = exitCode;
			Environment.ExitCode = exitCode;
		}
	}

	private object ShutdownCallback(object arg)
	{
		ShutdownImpl();
		return null;
	}

	private void ShutdownImpl()
	{
		try
		{
			DoShutdown();
		}
		finally
		{
			if (_ownDispatcherStarted)
			{
				base.Dispatcher.CriticalInvokeShutdown();
			}
			ServiceProvider = null;
		}
	}

	private static bool IsValidShutdownMode(ShutdownMode value)
	{
		if (value != ShutdownMode.OnExplicitShutdown && value != 0)
		{
			return value == ShutdownMode.OnMainWindowClose;
		}
		return true;
	}

	private void OnPreBPReady(object sender, BPReadyEventArgs e)
	{
		NavService.PreBPReady -= OnPreBPReady;
		NavService.AllowWindowNavigation = false;
		ConfigAppWindowAndRootElement(e.Content, e.Uri);
		NavService = null;
		e.Cancel = true;
	}

	private void ConfigAppWindowAndRootElement(object root, Uri uri)
	{
		if (!(root is Window window2))
		{
			NavigationWindow appWindow = GetAppWindow();
			appWindow.Navigate(root, new NavigateInfo(uri));
			base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate(object? window)
			{
				if (!((Window)window).IsDisposed)
				{
					((Window)window).Show();
				}
			}, appWindow);
		}
		else if (!window2.IsVisibilitySet && !window2.IsDisposed)
		{
			window2.Visibility = Visibility.Visible;
		}
	}

	private void PlaySound(string soundName)
	{
		string systemSound = GetSystemSound(soundName);
		if (!string.IsNullOrEmpty(systemSound))
		{
			MS.Win32.UnsafeNativeMethods.PlaySound(systemSound, IntPtr.Zero, SafeNativeMethods.PlaySoundFlags.SND_ASYNC | SafeNativeMethods.PlaySoundFlags.SND_NODEFAULT | SafeNativeMethods.PlaySoundFlags.SND_NOSTOP | SafeNativeMethods.PlaySoundFlags.SND_FILENAME);
		}
	}

	private string GetSystemSound(string soundName)
	{
		string result = null;
		string name = string.Format(CultureInfo.InvariantCulture, "AppEvents\\Schemes\\Apps\\Explorer\\{0}\\.current\\", soundName);
		try
		{
			using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(name);
			if (registryKey != null)
			{
				result = (string)registryKey.GetValue("");
			}
		}
		catch (IndexOutOfRangeException)
		{
		}
		return result;
	}

	private static bool IsComponentBeingLoadedFromOuterLoadBaml(Uri curComponentUri)
	{
		bool result = false;
		Invariant.Assert(curComponentUri != null, "curComponentUri should not be null");
		if (s_NestedBamlLoadInfo != null && s_NestedBamlLoadInfo.Count > 0)
		{
			NestedBamlLoadInfo nestedBamlLoadInfo = s_NestedBamlLoadInfo.Peek();
			if (nestedBamlLoadInfo != null && nestedBamlLoadInfo.BamlUri != null && nestedBamlLoadInfo.BamlStream != null && MS.Internal.Utility.BindUriHelper.DoSchemeAndHostMatch(nestedBamlLoadInfo.BamlUri, curComponentUri))
			{
				string localPath = nestedBamlLoadInfo.BamlUri.LocalPath;
				string localPath2 = curComponentUri.LocalPath;
				Invariant.Assert(localPath != null, "fileInBamlConvert should not be null");
				Invariant.Assert(localPath2 != null, "fileCurrent should not be null");
				if (string.Compare(localPath, localPath2, StringComparison.OrdinalIgnoreCase) == 0)
				{
					result = true;
				}
				else
				{
					string[] array = localPath.Split('/', '\\');
					string[] array2 = localPath2.Split('/', '\\');
					int num = array.Length;
					int num2 = array2.Length;
					Invariant.Assert(num >= 2 && num2 >= 2);
					int num3 = num - num2;
					if (Math.Abs(num3) == 1 && string.Compare(array[num - 1], array2[num2 - 1], StringComparison.OrdinalIgnoreCase) == 0)
					{
						result = BaseUriHelper.IsComponentEntryAssembly((num3 == 1) ? array[1] : array2[1]);
					}
				}
			}
		}
		return result;
	}

	private object RunDispatcher(object ignore)
	{
		if (_ownDispatcherStarted)
		{
			throw new InvalidOperationException(SR.ApplicationAlreadyRunning);
		}
		_ownDispatcherStarted = true;
		Dispatcher.Run();
		return null;
	}
}
