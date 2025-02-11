using System.Collections;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Net;
using System.Net.Cache;
using System.Security;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.AppModel;
using MS.Internal.Navigation;
using MS.Internal.Utility;
using MS.Utility;

namespace System.Windows.Navigation;

/// <summary>Contains methods, properties, and events to support navigation.</summary>
public sealed class NavigationService : IContentContainer
{
	internal static readonly DependencyProperty NavigationServiceProperty = DependencyProperty.RegisterAttached("NavigationService", typeof(NavigationService), typeof(NavigationService), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

	private NavigatingCancelEventHandler _navigating;

	private NavigatedEventHandler _navigated;

	private NavigationProgressEventHandler _navigationProgress;

	private LoadCompletedEventHandler _loadCompleted;

	private FragmentNavigationEventHandler _fragmentNavigation;

	private NavigationStoppedEventHandler _stopped;

	private object _bp;

	private uint _contentId;

	private Uri _currentSource;

	private Uri _currentCleanSource;

	private JournalEntryGroupState _journalEntryGroupState;

	private bool _doNotJournalCurrentContent;

	private bool _cancelContentRenderedHandling;

	private CustomContentState _customContentStateToSave;

	private CustomJournalStateInternal _rootViewerStateToSave;

	private WebRequest _request;

	private object _navState;

	private WebResponse _webResponse;

	private XamlReader _asyncObjectConverter;

	private bool _isNavInitiator;

	private bool _isNavInitiatorValid;

	private bool _allowWindowNavigation;

	private Guid _guidId = Guid.Empty;

	private INavigator _navigatorHost;

	private INavigatorImpl _navigatorHostImpl;

	private JournalNavigationScope _journalScope;

	private ArrayList _childNavigationServices = new ArrayList(2);

	private NavigationService _parentNavigationService;

	private bool _disposed;

	private FinishEventHandler _finishHandler;

	private NavigationStatus _navStatus;

	private ArrayList _pendingNavigationList = new ArrayList(2);

	private ArrayList _recursiveNavigateList = new ArrayList(2);

	private NavigateQueueItem _navigateQueueItem;

	private long _bytesRead;

	private long _maxBytes;

	private Visual _oldRootVisual;

	private const int _noParentPage = -1;

	private WebBrowser _webBrowser;

	/// <summary>Gets or sets the uniform resource identifier (URI) of the current content, or the URI of new content that is currently being navigated to.</summary>
	/// <returns>A <see cref="T:System.Uri" /> that contains the URI for the current content, or the content that is currently being navigated to.</returns>
	public Uri Source
	{
		get
		{
			if (IsDisposed)
			{
				return null;
			}
			if (_recursiveNavigateList.Count > 0)
			{
				return MS.Internal.Utility.BindUriHelper.GetUriRelativeToPackAppBase((_recursiveNavigateList[_recursiveNavigateList.Count - 1] as NavigateQueueItem).Source);
			}
			if (_navigateQueueItem != null)
			{
				return MS.Internal.Utility.BindUriHelper.GetUriRelativeToPackAppBase(_navigateQueueItem.Source);
			}
			return _currentCleanSource;
		}
		set
		{
			Navigate(value);
		}
	}

	/// <summary>Gets the uniform resource identifier (URI) of the content that was last navigated to.</summary>
	/// <returns>A <see cref="T:System.Uri" /> for the content that was last navigated to, if navigated to by using a URI; otherwise, null.</returns>
	public Uri CurrentSource
	{
		get
		{
			if (IsDisposed)
			{
				return null;
			}
			return _currentCleanSource;
		}
	}

	/// <summary>Gets or sets a reference to the object that contains the current content.</summary>
	/// <returns>An object that is a reference to the object that contains the current content.</returns>
	public object Content
	{
		get
		{
			if (IsDisposed)
			{
				return null;
			}
			return _bp;
		}
		set
		{
			Navigate(value);
		}
	}

	/// <summary>Gets a value that indicates whether there is at least one entry in forward navigation history.</summary>
	/// <returns>true if there is at least one entry in forward navigation history; otherwise, false.</returns>
	public bool CanGoForward
	{
		get
		{
			if (JournalScope != null)
			{
				return JournalScope.CanGoForward;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether there is at least one entry in back navigation history.</summary>
	/// <returns>true if there is at least one entry in back navigation history; otherwise, false.</returns>
	public bool CanGoBack
	{
		get
		{
			if (JournalScope != null)
			{
				return JournalScope.CanGoBack;
			}
			return false;
		}
	}

	internal Application Application => Application.Current;

	internal bool AllowWindowNavigation
	{
		private get
		{
			return _allowWindowNavigation;
		}
		set
		{
			_allowWindowNavigation = value;
		}
	}

	internal long BytesRead
	{
		get
		{
			return _bytesRead;
		}
		set
		{
			_bytesRead = value;
		}
	}

	internal long MaxBytes
	{
		get
		{
			return _maxBytes;
		}
		set
		{
			_maxBytes = value;
		}
	}

	internal uint ContentId => _contentId;

	internal Guid GuidId
	{
		get
		{
			return _guidId;
		}
		set
		{
			_guidId = value;
		}
	}

	internal NavigationService ParentNavigationService => _parentNavigationService;

	internal bool CanReloadFromUri
	{
		get
		{
			if (!(_currentCleanSource == null) && !MS.Internal.Utility.BindUriHelper.StartWithFragment(_currentCleanSource))
			{
				return !MS.Internal.Utility.BindUriHelper.StartWithFragment(MS.Internal.Utility.BindUriHelper.GetUriRelativeToPackAppBase(_currentCleanSource));
			}
			return false;
		}
	}

	internal ArrayList ChildNavigationServices => _childNavigationServices;

	private FinishEventHandler FinishHandler
	{
		get
		{
			if (_finishHandler == null)
			{
				_finishHandler = HandleFinish;
			}
			return _finishHandler;
		}
	}

	private bool IsTopLevelContainer
	{
		get
		{
			if (!(INavigatorHost is NavigationWindow))
			{
				if (Application != null && Application.CheckAccess())
				{
					return Application.NavService == this;
				}
				return false;
			}
			return true;
		}
	}

	private bool IsJournalLevelContainer
	{
		get
		{
			JournalNavigationScope journalScope = JournalScope;
			if (journalScope != null)
			{
				return journalScope.RootNavigationService == this;
			}
			return false;
		}
	}

	private bool SandboxExternalContent
	{
		get
		{
			if (!(INavigatorHost is DependencyObject dependencyObject))
			{
				return false;
			}
			return (bool)dependencyObject.GetValue(Frame.SandboxExternalContentProperty);
		}
	}

	internal INavigator INavigatorHost
	{
		get
		{
			return _navigatorHost;
		}
		set
		{
			RequestNavigateEventHandler handler = OnRequestNavigate;
			if (_navigatorHost != null)
			{
				if (_navigatorHost is IInputElement inputElement)
				{
					inputElement.RemoveHandler(Hyperlink.RequestNavigateEvent, handler);
				}
				if (_navigatorHost is IDownloader downloader)
				{
					downloader.ContentRendered -= ContentRenderedHandler;
				}
			}
			if (value != null)
			{
				if (value is IInputElement inputElement2)
				{
					inputElement2.AddHandler(Hyperlink.RequestNavigateEvent, handler);
				}
				if (value is IDownloader downloader2)
				{
					downloader2.ContentRendered += ContentRenderedHandler;
				}
			}
			_navigatorHost = value;
			_navigatorHostImpl = value as INavigatorImpl;
		}
	}

	internal NavigationStatus NavStatus
	{
		get
		{
			return _navStatus;
		}
		set
		{
			_navStatus = value;
		}
	}

	internal ArrayList PendingNavigationList => _pendingNavigationList;

	internal WebBrowser WebBrowser => _webBrowser;

	internal bool IsDisposed
	{
		get
		{
			bool flag = false;
			if (Application != null && Application.CheckAccess() && Application.IsShuttingDown)
			{
				flag = true;
			}
			return _disposed || flag;
		}
	}

	internal bool IsUnsafe { get; set; }

	private JournalNavigationScope JournalScope
	{
		get
		{
			if (_journalScope == null && _navigatorHost != null)
			{
				_journalScope = _navigatorHost.GetJournal(create: false);
			}
			return _journalScope;
		}
	}

	private bool IsNavigationInitiator
	{
		get
		{
			if (!_isNavInitiatorValid)
			{
				_isNavInitiator = IsTopLevelContainer;
				if (_parentNavigationService != null)
				{
					if (!_parentNavigationService.PendingNavigationList.Contains(this))
					{
						_isNavInitiator = true;
					}
				}
				else if (IsJournalLevelContainer)
				{
					_isNavInitiator = true;
				}
				_isNavInitiatorValid = true;
			}
			return _isNavInitiator;
		}
	}

	/// <summary>Occurs when an error occurs while navigating to the requested content.</summary>
	public event NavigationFailedEventHandler NavigationFailed;

	/// <summary>Occurs when a new navigation is requested.</summary>
	public event NavigatingCancelEventHandler Navigating
	{
		add
		{
			_navigating = (NavigatingCancelEventHandler)Delegate.Combine(_navigating, value);
		}
		remove
		{
			_navigating = (NavigatingCancelEventHandler)Delegate.Remove(_navigating, value);
		}
	}

	/// <summary>Occurs when the content that is being navigated to has been found, and is available from the <see cref="P:System.Windows.Navigation.NavigationService.Content" /> property, although it may not have completed loading.</summary>
	public event NavigatedEventHandler Navigated
	{
		add
		{
			_navigated = (NavigatedEventHandler)Delegate.Combine(_navigated, value);
		}
		remove
		{
			_navigated = (NavigatedEventHandler)Delegate.Remove(_navigated, value);
		}
	}

	/// <summary>Occurs periodically during a download to provide navigation progress information.</summary>
	public event NavigationProgressEventHandler NavigationProgress
	{
		add
		{
			_navigationProgress = (NavigationProgressEventHandler)Delegate.Combine(_navigationProgress, value);
		}
		remove
		{
			_navigationProgress = (NavigationProgressEventHandler)Delegate.Remove(_navigationProgress, value);
		}
	}

	/// <summary>Occurs when content that was navigated to has been loaded, parsed, and has begun rendering.</summary>
	public event LoadCompletedEventHandler LoadCompleted
	{
		add
		{
			_loadCompleted = (LoadCompletedEventHandler)Delegate.Combine(_loadCompleted, value);
		}
		remove
		{
			_loadCompleted = (LoadCompletedEventHandler)Delegate.Remove(_loadCompleted, value);
		}
	}

	/// <summary>Occurs when navigation to a content fragment begins, which occurs immediately, if the desired fragment is in the current content, or after the source XAML content has been loaded, if the desired fragment is in different content.</summary>
	public event FragmentNavigationEventHandler FragmentNavigation
	{
		add
		{
			_fragmentNavigation = (FragmentNavigationEventHandler)Delegate.Combine(_fragmentNavigation, value);
		}
		remove
		{
			_fragmentNavigation = (FragmentNavigationEventHandler)Delegate.Remove(_fragmentNavigation, value);
		}
	}

	/// <summary>Occurs when the <see cref="M:System.Windows.Navigation.NavigationService.StopLoading" /> method is called, or when a new navigation is requested while a current navigation is in progress.</summary>
	public event NavigationStoppedEventHandler NavigationStopped
	{
		add
		{
			_stopped = (NavigationStoppedEventHandler)Delegate.Combine(_stopped, value);
		}
		remove
		{
			_stopped = (NavigationStoppedEventHandler)Delegate.Remove(_stopped, value);
		}
	}

	internal event BPReadyEventHandler BPReady;

	internal event BPReadyEventHandler PreBPReady;

	internal NavigationService(INavigator nav)
	{
		INavigatorHost = nav;
		if (!(nav is NavigationWindow))
		{
			GuidId = Guid.NewGuid();
		}
	}

	private void ResetPendingNavigationState(NavigationStatus newState)
	{
		JournalNavigationScope journalScope = JournalScope;
		if (journalScope != null && journalScope.RootNavigationService != this)
		{
			journalScope.RootNavigationService.BytesRead -= _bytesRead;
			journalScope.RootNavigationService.MaxBytes -= _maxBytes;
		}
		_navStatus = newState;
		_bytesRead = 0L;
		_maxBytes = 0L;
		_navigateQueueItem = null;
		_request = null;
	}

	private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
	{
		e.Handled = true;
		string target = e.Target;
		Uri bpu = e.Uri;
		if (bpu != null && !bpu.IsAbsoluteUri)
		{
			DependencyObject dependencyObject = e.OriginalSource as DependencyObject;
			if (!(dependencyObject is IUriContext uriContext))
			{
				throw new Exception(SR.Format(SR.MustImplementIUriContext, typeof(IUriContext)));
			}
			bpu = MS.Internal.Utility.BindUriHelper.GetUriToNavigate(dependencyObject, uriContext.BaseUri, e.Uri);
		}
		INavigatorBase navigator = null;
		bool flag = true;
		if (!string.IsNullOrEmpty(target))
		{
			navigator = FindTarget(target);
			if (navigator == null && JournalScope != null)
			{
				navigator = JournalScope.FindTarget(target);
			}
			if (navigator == null)
			{
				NavigationWindow navigationWindow = FindNavigationWindow();
				if (navigationWindow != null)
				{
					navigator = FindTargetInNavigationWindow(navigationWindow, target);
				}
				if (navigator == null)
				{
					navigator = FindTargetInApplication(target);
					if (navigator != null)
					{
						flag = ((DispatcherObject)navigator).CheckAccess();
					}
				}
			}
		}
		else
		{
			navigator = INavigatorHost;
		}
		if (navigator != null)
		{
			if (flag)
			{
				navigator.Navigate(bpu);
				return;
			}
			((DispatcherObject)navigator).Dispatcher.BeginInvoke(DispatcherPriority.Send, (DispatcherOperationCallback)((object unused) => navigator.Navigate(bpu)), null);
			return;
		}
		throw new ArgumentException(SR.HyperLinkTargetNotFound);
	}

	private static bool IsSameUri(Uri baseUri, Uri a, Uri b, bool withFragment)
	{
		if ((object)a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		Uri resolvedUri = MS.Internal.Utility.BindUriHelper.GetResolvedUri(baseUri, a);
		Uri resolvedUri2 = MS.Internal.Utility.BindUriHelper.GetResolvedUri(baseUri, b);
		bool flag = resolvedUri.Equals(resolvedUri2);
		if (flag && withFragment)
		{
			flag = flag && string.Compare(resolvedUri.Fragment, resolvedUri2.Fragment, StringComparison.OrdinalIgnoreCase) == 0;
		}
		return flag;
	}

	private void NavigateToFragmentOrCustomContentState(Uri uri, object navState)
	{
		NavigateInfo navigateInfo = navState as NavigateInfo;
		JournalEntry journalEntry = null;
		if (navigateInfo != null)
		{
			journalEntry = navigateInfo.JournalEntry;
		}
		NavigationMode navigationMode = navigateInfo?.NavigationMode ?? NavigationMode.New;
		CustomJournalStateInternal rootViewerState = GetRootViewerState(JournalReason.FragmentNavigation);
		string elementId = ((uri != null) ? MS.Internal.Utility.BindUriHelper.GetFragment(uri) : null);
		bool flag = journalEntry != null && journalEntry.CustomContentState != null;
		bool flag2 = NavigateToFragment(elementId, !flag);
		if (navigationMode == NavigationMode.Back || navigationMode == NavigationMode.Forward || (flag2 && !IsSameUri(null, _currentSource, uri, withFragment: true)))
		{
			try
			{
				_rootViewerStateToSave = rootViewerState;
				UpdateJournal(navigationMode, JournalReason.FragmentNavigation, journalEntry);
			}
			finally
			{
				_rootViewerStateToSave = null;
			}
			Uri resolvedUri = MS.Internal.Utility.BindUriHelper.GetResolvedUri(_currentSource, uri);
			_currentSource = resolvedUri;
			_currentCleanSource = MS.Internal.Utility.BindUriHelper.GetUriRelativeToPackAppBase(uri);
		}
		_navStatus = NavigationStatus.Navigated;
		HandleNavigated(navState, navigatedToNewContent: false);
	}

	private bool NavigateToFragment(string elementId, bool scrollToTopOnEmptyFragment)
	{
		if (FireFragmentNavigation(elementId))
		{
			return true;
		}
		if (string.IsNullOrEmpty(elementId))
		{
			if (!scrollToTopOnEmptyFragment)
			{
				return false;
			}
			ScrollContentToTop();
			return true;
		}
		DependencyObject dependencyObject = LogicalTreeHelper.FindLogicalNode((DependencyObject)_bp, elementId);
		BringIntoView(dependencyObject);
		return dependencyObject != null;
	}

	private void ScrollContentToTop()
	{
		if (_bp != null)
		{
			if (_bp is FrameworkElement { LogicalChildren: { } logicalChildren } && logicalChildren.MoveNext() && logicalChildren.Current is ScrollViewer scrollViewer)
			{
				scrollViewer.ScrollToTop();
			}
			else if (_bp is IInputElement target && ScrollBar.ScrollToTopCommand.CanExecute(null, target))
			{
				ScrollBar.ScrollToTopCommand.Execute(null, target);
			}
			else
			{
				BringIntoView(_bp as DependencyObject);
			}
		}
	}

	private static void BringIntoView(DependencyObject elem)
	{
		if (elem is FrameworkElement frameworkElement)
		{
			frameworkElement.BringIntoView();
		}
		else if (elem is FrameworkContentElement frameworkContentElement)
		{
			frameworkContentElement.BringIntoView();
		}
	}

	private JournalNavigationScope EnsureJournal()
	{
		if (_journalScope == null && _navigatorHost != null)
		{
			_journalScope = _navigatorHost.GetJournal(create: true);
		}
		return _journalScope;
	}

	private bool IsConsistent(NavigateInfo navInfo)
	{
		if (navInfo != null)
		{
			if (navInfo.IsConsistent)
			{
				if (navInfo.JournalEntry != null)
				{
					return navInfo.JournalEntry.NavigationServiceId == _guidId;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	private bool IsJournalNavigation(NavigateInfo navInfo)
	{
		if (navInfo != null)
		{
			if (navInfo.NavigationMode != NavigationMode.Back)
			{
				return navInfo.NavigationMode == NavigationMode.Forward;
			}
			return true;
		}
		return false;
	}

	private CustomJournalStateInternal GetRootViewerState(JournalReason journalReason)
	{
		if (_navigatorHostImpl != null && !(_bp is Visual) && _navigatorHostImpl.FindRootViewer() is IJournalState journalState)
		{
			return journalState.GetJournalState(journalReason);
		}
		return null;
	}

	private bool RestoreRootViewerState(CustomJournalStateInternal rvs)
	{
		Visual visual = _navigatorHostImpl.FindRootViewer();
		if (visual == null)
		{
			return false;
		}
		if (visual is IJournalState journalState)
		{
			journalState.RestoreJournalState(rvs);
		}
		return true;
	}

	internal static INavigatorBase FindTargetInApplication(string targetName)
	{
		if (Application.Current == null)
		{
			return null;
		}
		INavigatorBase navigatorBase = FindTargetInWindowCollection(Application.Current.WindowsInternal.Clone(), targetName);
		if (navigatorBase == null)
		{
			navigatorBase = FindTargetInWindowCollection(Application.Current.NonAppWindowsInternal.Clone(), targetName);
		}
		return navigatorBase;
	}

	private static INavigatorBase FindTargetInWindowCollection(WindowCollection wc, string targetName)
	{
		INavigatorBase navigatorBase = null;
		NavigationWindow nw = null;
		for (int i = 0; i < wc.Count; i++)
		{
			nw = wc[i] as NavigationWindow;
			if (nw != null)
			{
				navigatorBase = ((!nw.CheckAccess()) ? ((INavigator)nw.Dispatcher.Invoke(DispatcherPriority.Send, (DispatcherOperationCallback)((object unused) => FindTargetInNavigationWindow(nw, targetName)), null)) : FindTargetInNavigationWindow(nw, targetName));
				if (navigatorBase != null)
				{
					return navigatorBase;
				}
			}
		}
		return null;
	}

	private static INavigatorBase FindTargetInNavigationWindow(NavigationWindow navigationWindow, string navigatorId)
	{
		return navigationWindow?.NavigationService.FindTarget(navigatorId);
	}

	internal void InvalidateJournalNavigationScope()
	{
		if (_journalScope != null && _journalScope.Journal.HasUncommittedNavigation)
		{
			throw new InvalidOperationException(SR.InvalidOperation_CantChangeJournalOwnership);
		}
		_journalScope = null;
		for (int num = ChildNavigationServices.Count - 1; num >= 0; num--)
		{
			((NavigationService)ChildNavigationServices[num]).InvalidateJournalNavigationScope();
		}
	}

	internal void OnParentNavigationServiceChanged()
	{
		NavigationService parentNavigationService = _parentNavigationService;
		NavigationService navigationService = ((DependencyObject)INavigatorHost).GetValue(NavigationServiceProperty) as NavigationService;
		if (navigationService != parentNavigationService)
		{
			parentNavigationService?.RemoveChild(this);
			navigationService?.AddChild(this);
		}
	}

	internal void AddChild(NavigationService ncChild)
	{
		if (ncChild == this)
		{
			throw new Exception(SR.Format(SR.LoopDetected, _currentCleanSource));
		}
		Invariant.Assert(ncChild.ParentNavigationService == null);
		Invariant.Assert(ncChild.JournalScope == null || ncChild.IsJournalLevelContainer, "Parentless NavigationService has a reference to a JournalNavigationScope its host navigator doesn't own.");
		ChildNavigationServices.Add(ncChild);
		ncChild._parentNavigationService = this;
		if (JournalScope != null)
		{
			JournalScope.Journal.UpdateView();
		}
		if (NavStatus == NavigationStatus.Stopped)
		{
			ncChild.INavigatorHost.StopLoading();
		}
		else if (ncChild.NavStatus != 0 && ncChild.NavStatus != NavigationStatus.Stopped && NavStatus != 0 && NavStatus != NavigationStatus.Stopped)
		{
			PendingNavigationList.Add(ncChild);
		}
	}

	internal void RemoveChild(NavigationService ncChild)
	{
		ChildNavigationServices.Remove(ncChild);
		ncChild._parentNavigationService = null;
		if (!ncChild.IsJournalLevelContainer)
		{
			ncChild.InvalidateJournalNavigationScope();
		}
		if (JournalScope != null)
		{
			JournalScope.Journal.UpdateView();
		}
		if (PendingNavigationList.Contains(ncChild))
		{
			PendingNavigationList.Remove(ncChild);
			HandleLoadCompleted(null);
		}
	}

	internal NavigationService FindTarget(Guid navigationServiceId)
	{
		if (GuidId == navigationServiceId)
		{
			return this;
		}
		NavigationService navigationService = null;
		foreach (NavigationService childNavigationService in ChildNavigationServices)
		{
			navigationService = childNavigationService.FindTarget(navigationServiceId);
			if (navigationService != null)
			{
				return navigationService;
			}
		}
		return null;
	}

	internal INavigatorBase FindTarget(string name)
	{
		FrameworkElement frameworkElement = INavigatorHost as FrameworkElement;
		if (string.Compare(name, frameworkElement.Name, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return INavigatorHost;
		}
		INavigatorBase navigatorBase = null;
		foreach (NavigationService childNavigationService in ChildNavigationServices)
		{
			navigatorBase = childNavigationService.FindTarget(name);
			if (navigatorBase != null)
			{
				return navigatorBase;
			}
		}
		return navigatorBase;
	}

	internal bool IsContentKeepAlive()
	{
		bool flag = true;
		if (_bp is DependencyObject dependencyObject)
		{
			flag = JournalEntry.GetKeepAlive(dependencyObject);
			if (!flag)
			{
				PageFunctionBase obj = dependencyObject as PageFunctionBase;
				bool flag2 = !CanReloadFromUri;
				if (obj == null && flag2)
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	private void SetBaseUri(DependencyObject dobj, Uri fullUri)
	{
		Invariant.Assert(dobj != null && !dobj.IsSealed);
		if ((Uri)dobj.GetValue(BaseUriHelper.BaseUriProperty) == null && fullUri != null)
		{
			dobj.SetValue(BaseUriHelper.BaseUriProperty, fullUri);
		}
	}

	private bool UnhookOldTree(object oldTree)
	{
		DependencyObject dependencyObject = oldTree as DependencyObject;
		if (dependencyObject != null && !dependencyObject.IsSealed)
		{
			dependencyObject.SetValue(NavigationServiceProperty, null);
		}
		if (oldTree is IInputElement { IsKeyboardFocusWithin: not false })
		{
			if (dependencyObject != null && JournalScope != null)
			{
				DependencyObject dependencyObject2 = (DependencyObject)INavigatorHost;
				if (!(bool)dependencyObject2.GetValue(FocusManager.IsFocusScopeProperty))
				{
					dependencyObject2 = FocusManager.GetFocusScope(dependencyObject2);
				}
				FocusManager.SetFocusedElement(dependencyObject2, null);
			}
			Keyboard.PrimaryDevice.Focus(null);
		}
		if (oldTree is PageFunctionBase pageFunctionBase)
		{
			pageFunctionBase.FinishHandler = null;
		}
		bool result = true;
		if (IsContentKeepAlive())
		{
			result = false;
		}
		return result;
	}

	private bool HookupNewTree(object newTree, NavigateInfo navInfo, Uri newUri)
	{
		if (newTree != null && IsJournalNavigation(navInfo))
		{
			navInfo.JournalEntry.RestoreState(newTree);
		}
		PageFunctionReturnInfo pageFunctionReturnInfo = navInfo as PageFunctionReturnInfo;
		PageFunctionBase pageFunctionBase = pageFunctionReturnInfo?.FinishingChildPageFunction;
		if (pageFunctionBase != null)
		{
			object returnEventArgs = pageFunctionReturnInfo?.ReturnEventArgs;
			if (newTree != null)
			{
				FireChildPageFunctionReturnEvent(newTree, pageFunctionBase, returnEventArgs);
				if (_navigateQueueItem != null)
				{
					if (pageFunctionReturnInfo.JournalEntry != null)
					{
						pageFunctionReturnInfo.JournalEntry.SaveState(newTree);
					}
					return false;
				}
			}
		}
		if (IsPageFunction(newTree))
		{
			SetupPageFunctionHandlers(newTree);
			if ((navInfo == null || navInfo.NavigationMode == NavigationMode.New) && !_doNotJournalCurrentContent)
			{
				PageFunctionBase pageFunctionBase2 = (PageFunctionBase)newTree;
				if (!pageFunctionBase2._Resume && pageFunctionBase2.ParentPageFunctionId == Guid.Empty && _bp is PageFunctionBase)
				{
					pageFunctionBase2.ParentPageFunctionId = ((PageFunctionBase)_bp).PageFunctionId;
				}
			}
		}
		if (newTree is DependencyObject { IsSealed: false } dependencyObject)
		{
			dependencyObject.SetValue(NavigationServiceProperty, this);
			if (newUri != null && !MS.Internal.Utility.BindUriHelper.StartWithFragment(newUri))
			{
				SetBaseUri(dependencyObject, newUri);
			}
		}
		_webBrowser = newTree as WebBrowser;
		return true;
	}

	private bool OnBeforeSwitchContent(object newBP, NavigateInfo navInfo, Uri newUri)
	{
		if (newBP != null && !HookupNewTree(newBP, navInfo, newUri))
		{
			return false;
		}
		if (navInfo == null)
		{
			UpdateJournal(NavigationMode.New, JournalReason.NewContentNavigation, null);
		}
		else if (navInfo.NavigationMode != NavigationMode.Refresh)
		{
			UpdateJournal(navInfo.NavigationMode, JournalReason.NewContentNavigation, navInfo.JournalEntry);
		}
		if (_navigateQueueItem != null)
		{
			return false;
		}
		if (UnhookOldTree(_bp))
		{
			DisposeTreeQueueItem @object = new DisposeTreeQueueItem(_bp);
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(@object.Dispatch), null);
		}
		return true;
	}

	internal void VisualTreeAvailable(Visual v)
	{
		if (v != _oldRootVisual)
		{
			if (_oldRootVisual != null)
			{
				_oldRootVisual.SetValue(NavigationServiceProperty, null);
			}
			v?.SetValue(NavigationServiceProperty, this);
			_oldRootVisual = v;
		}
	}

	void IContentContainer.OnContentReady(ContentType contentType, object bp, Uri bpu, object navState)
	{
		Invariant.Assert(bpu == null || bpu.IsAbsoluteUri, "Content URI must be absolute.");
		if (IsDisposed)
		{
			return;
		}
		if (!IsValidRootElement(bp))
		{
			throw new InvalidOperationException(SR.Format(SR.WrongNavigateRootElement, bp.ToString()));
		}
		ResetPendingNavigationState(NavigationStatus.Navigated);
		NavigateInfo navigateInfo = navState as NavigateInfo;
		NavigationMode navigationMode = navigateInfo?.NavigationMode ?? NavigationMode.New;
		if (bpu == null)
		{
			bpu = navigateInfo?.Source;
		}
		Uri uriRelativeToPackAppBase = MS.Internal.Utility.BindUriHelper.GetUriRelativeToPackAppBase(bpu);
		if (this.PreBPReady != null)
		{
			BPReadyEventArgs bPReadyEventArgs = new BPReadyEventArgs(bp, bpu);
			this.PreBPReady(this, bPReadyEventArgs);
			if (bPReadyEventArgs.Cancel)
			{
				_navStatus = NavigationStatus.Idle;
				return;
			}
		}
		bool flag = false;
		if (bp == _bp)
		{
			flag = true;
			_bp = null;
			if (this.BPReady != null)
			{
				this.BPReady(this, new BPReadyEventArgs(null, null));
			}
		}
		else
		{
			if (!OnBeforeSwitchContent(bp, navigateInfo, bpu))
			{
				return;
			}
			if (navigationMode != NavigationMode.Refresh)
			{
				if (navigateInfo == null || navigateInfo.JournalEntry == null)
				{
					_contentId++;
					_journalEntryGroupState = null;
				}
				else
				{
					_contentId = navigateInfo.JournalEntry.ContentId;
					_journalEntryGroupState = navigateInfo.JournalEntry.JEGroupState;
				}
				_currentSource = bpu;
				_currentCleanSource = uriRelativeToPackAppBase;
			}
		}
		_bp = bp;
		if (this.BPReady != null)
		{
			this.BPReady(this, new BPReadyEventArgs(_bp, bpu));
		}
		HandleNavigated(navState, !flag);
	}

	void IContentContainer.OnNavigationProgress(Uri sourceUri, long bytesRead, long maxBytes)
	{
		if (!IsDisposed && sourceUri.Equals(Source))
		{
			NavigationService navigationService = null;
			if (JournalScope != null && JournalScope.RootNavigationService != this)
			{
				navigationService = JournalScope.RootNavigationService;
				navigationService.BytesRead += bytesRead - _bytesRead;
				navigationService.MaxBytes += maxBytes - _maxBytes;
			}
			_bytesRead = bytesRead;
			_maxBytes = maxBytes;
			FireNavigationProgress(sourceUri);
			navigationService?.FireNavigationProgress(sourceUri);
		}
	}

	void IContentContainer.OnStreamClosed(Uri sourceUri)
	{
		if (sourceUri.Equals(Source))
		{
			_asyncObjectConverter = null;
			HandleLoadCompleted(null);
		}
	}

	/// <summary>Gets a reference to the <see cref="T:System.Windows.Navigation.NavigationService" /> for the navigator whose content contains the specified <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>A reference to the <see cref="T:System.Windows.Navigation.NavigationService" /> for the navigator whose content contains the specified <see cref="T:System.Windows.DependencyObject" />; can be null in some cases (see Remarks).</returns>
	/// <param name="dependencyObject">The <see cref="T:System.Windows.DependencyObject" /> in content that is hosted by a navigator.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="dependencyObject" /> parameter is null.</exception>
	public static NavigationService GetNavigationService(DependencyObject dependencyObject)
	{
		if (dependencyObject == null)
		{
			throw new ArgumentNullException("dependencyObject");
		}
		return dependencyObject.GetValue(NavigationServiceProperty) as NavigationService;
	}

	/// <summary>Adds an entry to back navigation history that contains a <see cref="T:System.Windows.Navigation.CustomContentState" /> object.</summary>
	/// <param name="state">A <see cref="T:System.Windows.Navigation.CustomContentState" /> object that represents application-defined state that is associated with a specific piece of content.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="state" /> is null, and a <see cref="T:System.Windows.Navigation.CustomContentState" /> object isn't returned from <see cref="M:System.Windows.Navigation.IProvideCustomContentState.GetContentState" />.</exception>
	public void AddBackEntry(CustomContentState state)
	{
		if (!IsDisposed)
		{
			if (_bp == null)
			{
				throw new InvalidOperationException(SR.InvalidOperation_AddBackEntryNoContent);
			}
			_customContentStateToSave = state;
			JournalEntry journalEntry = UpdateJournal(NavigationMode.New, JournalReason.AddBackEntry, null);
			_customContentStateToSave = null;
			if (journalEntry != null && journalEntry.CustomContentState == null)
			{
				RemoveBackEntry();
				throw new InvalidOperationException(SR.Format(SR.InvalidOperation_MustImplementIPCCSOrHandleNavigating, (_bp != null) ? _bp.GetType().ToString() : "null"));
			}
		}
	}

	/// <summary>Removes the most recent journal entry from back history.</summary>
	/// <returns>The most recent <see cref="T:System.Windows.Navigation.JournalEntry" /> in back navigation history, if there is one.</returns>
	public JournalEntry RemoveBackEntry()
	{
		if (IsDisposed)
		{
			return null;
		}
		if (JournalScope == null)
		{
			return null;
		}
		return JournalScope.RemoveBackEntry();
	}

	/// <summary>Navigate asynchronously to content that is specified by a uniform resource identifier (URI).</summary>
	/// <returns>true if a navigation is not canceled; otherwise, false.</returns>
	/// <param name="source">A <see cref="T:System.Uri" /> object initialized with the URI for the desired content.</param>
	public bool Navigate(Uri source)
	{
		return Navigate(source, null, sandboxExternalContent: false, navigateOnSourceChanged: false);
	}

	/// <summary>Navigate asynchronously to content that is contained by an object.</summary>
	/// <returns>true if a navigation is not canceled; otherwise, false.</returns>
	/// <param name="root">An object that contains the content to navigate to.</param>
	public bool Navigate(object root)
	{
		return Navigate(root, null);
	}

	/// <summary>Navigate asynchronously to source content located at a uniform resource identifier (URI), and pass an object that contains data to be used for processing during navigation.</summary>
	/// <returns>true if a navigation is not canceled; otherwise, false.</returns>
	/// <param name="source">A <see cref="T:System.Uri" /> object initialized with the URI for the desired content.</param>
	/// <param name="navigationState">An object that contains data to be used for processing during navigation.</param>
	public bool Navigate(Uri source, object navigationState)
	{
		return Navigate(source, navigationState, sandboxExternalContent: false, navigateOnSourceChanged: false);
	}

	/// <summary>Navigate asynchronously to source content located at a uniform resource identifier (URI), pass an object containing navigation state for processing during navigation, and sandbox the content.</summary>
	/// <returns>true if a navigation is not canceled; otherwise, false.</returns>
	/// <param name="source">A <see cref="T:System.Uri" /> object initialized with the URI for the desired content.</param>
	/// <param name="navigationState">An object that contains data to be used for processing during navigation.</param>
	/// <param name="sandboxExternalContent">Download content into a partial trust security sandbox (with the default Internet zone set of permissions, if true. The default is false.</param>
	public bool Navigate(Uri source, object navigationState, bool sandboxExternalContent)
	{
		return Navigate(source, navigationState, sandboxExternalContent, navigateOnSourceChanged: false);
	}

	internal bool Navigate(Uri source, object navigationState, bool sandboxExternalContent, bool navigateOnSourceChanged)
	{
		if (IsDisposed)
		{
			return false;
		}
		NavigateInfo navigateInfo = navigationState as NavigateInfo;
		if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordHosting, EventTrace.Level.Info))
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.Wpf_NavigationStart, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordHosting, EventTrace.Level.Info, (navigateInfo != null) ? navigateInfo.NavigationMode.ToString() : NavigationMode.New.ToString(), (source != null) ? ("\"" + source.ToString() + "\"") : "(null)");
		}
		Invariant.Assert(IsConsistent(navigateInfo));
		WebRequest webRequest = null;
		bool flag = false;
		Uri uri = null;
		if (source != null)
		{
			if (MS.Internal.Utility.BindUriHelper.StartWithFragment(source) || MS.Internal.Utility.BindUriHelper.StartWithFragment(MS.Internal.Utility.BindUriHelper.GetUriRelativeToPackAppBase(source)))
			{
				uri = MS.Internal.Utility.BindUriHelper.GetResolvedUri(_currentSource, source);
				flag = true;
			}
			else
			{
				uri = MS.Internal.Utility.BindUriHelper.GetResolvedUri(source);
				flag = (navigateInfo == null || navigateInfo.JournalEntry == null || navigateInfo.JournalEntry.ContentId == _contentId) && IsSameUri(null, uri, _currentSource, withFragment: false);
			}
			if (navigateInfo != null && navigateInfo.NavigationMode == NavigationMode.Refresh)
			{
				flag = false;
			}
			if (!flag)
			{
				webRequest = CreateWebRequest(uri, navigateInfo);
				if (webRequest == null)
				{
					return false;
				}
			}
		}
		if (!HandleNavigating(uri, null, navigationState, webRequest, navigateOnSourceChanged))
		{
			return false;
		}
		if (source == null && _bp == null)
		{
			ResetPendingNavigationState(NavigationStatus.Idle);
			return true;
		}
		if (flag)
		{
			NavigateToFragmentOrCustomContentState(uri, navigationState);
			return true;
		}
		_navigateQueueItem.PostNavigation();
		return true;
	}

	private void InformBrowserAboutStoppedNavigation()
	{
		if (Application != null && Application.CheckAccess())
		{
			Application.PerformNavigationStateChangeTasks(isNavigationInitiator: true, playNavigatingSound: false, Application.NavigationStateChange.Stopped);
		}
	}

	/// <summary>Navigate asynchronously to content that is contained by an object, and pass an object that contains data to be used for processing during navigation.</summary>
	/// <returns>true if a navigation is not canceled; otherwise, false.</returns>
	/// <param name="root">An object that contains the content to navigate to.</param>
	/// <param name="navigationState">An object that contains data to be used for processing during navigation.</param>
	public bool Navigate(object root, object navigationState)
	{
		if (IsDisposed)
		{
			return false;
		}
		NavigateInfo navigateInfo = navigationState as NavigateInfo;
		if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordHosting, EventTrace.Level.Info))
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.Wpf_NavigationStart, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordHosting, EventTrace.Level.Info, (navigateInfo != null) ? navigateInfo.NavigationMode.ToString() : NavigationMode.New.ToString(), (root != null) ? root.ToString() : "(null)");
		}
		Invariant.Assert(IsConsistent(navigateInfo));
		if (navigateInfo == null && root is PageFunctionBase pageFunctionBase && (pageFunctionBase._Resume || pageFunctionBase._Saver != null))
		{
			throw new InvalidOperationException(SR.InvalidOperation_CannotReenterPageFunction);
		}
		Uri uri = navigateInfo?.Source;
		if (!HandleNavigating(uri, root, navigationState, null, navigateOnSourceChanged: false))
		{
			return false;
		}
		if (root == _bp && (navigateInfo == null || navigateInfo.NavigationMode != NavigationMode.Refresh))
		{
			NavigateToFragmentOrCustomContentState(uri, navigationState);
			if (IsJournalNavigation(navigateInfo))
			{
				_journalEntryGroupState = navigateInfo.JournalEntry.JEGroupState;
				_contentId = _journalEntryGroupState.ContentId;
				_journalScope.Journal.UpdateView();
			}
			return true;
		}
		_navigateQueueItem.PostNavigation();
		return true;
	}

	/// <summary>Navigate to the most recent entry in forward navigation history, if there is one.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Windows.Navigation.NavigationService.GoForward" /> is called when there are no entries in forward navigation history.</exception>
	public void GoForward()
	{
		if (JournalScope == null)
		{
			throw new InvalidOperationException(SR.NoForwardEntry);
		}
		JournalScope.GoForward();
	}

	/// <summary>Navigates to the most recent entry in back navigation history, if there is one.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Windows.Navigation.NavigationService.GoBack" /> is called when there are no entries in back navigation history.</exception>
	public void GoBack()
	{
		if (JournalScope == null)
		{
			throw new InvalidOperationException(SR.NoBackEntry);
		}
		JournalScope.GoBack();
	}

	/// <summary>Stops further downloading of content for the current navigation request.</summary>
	public void StopLoading()
	{
		DoStopLoading(clearRecursiveNavigations: true, fireEvents: true);
	}

	private void DoStopLoading(bool clearRecursiveNavigations, bool fireEvents)
	{
		bool flag = false;
		object navState = null;
		if (_asyncObjectConverter != null)
		{
			_asyncObjectConverter.CancelAsync();
			_asyncObjectConverter = null;
			Invariant.Assert(_webResponse != null);
			_webResponse.Close();
			_webResponse = null;
		}
		else if (_navStatus != NavigationStatus.Navigating && _webResponse != null)
		{
			_webResponse.Close();
			_webResponse = null;
		}
		_navStatus = NavigationStatus.Stopped;
		if (_navigateQueueItem != null)
		{
			_navigateQueueItem.Stop();
			if (JournalScope != null && clearRecursiveNavigations)
			{
				JournalScope.AbortJournalNavigation();
			}
			if (_request != null)
			{
				try
				{
					WebRequest request = _request;
					_request = null;
					request.Abort();
				}
				catch (NotSupportedException)
				{
				}
				catch (NotImplementedException)
				{
				}
			}
			navState = _navigateQueueItem.NavState;
			ResetPendingNavigationState(NavigationStatus.Stopped);
			flag = true;
		}
		if (clearRecursiveNavigations && _recursiveNavigateList.Count > 0)
		{
			_recursiveNavigateList.Clear();
			flag = true;
		}
		if (_navigatorHostImpl != null)
		{
			_navigatorHostImpl.OnSourceUpdatedFromNavService(journalOrCancel: true);
		}
		bool fireEvents2 = false;
		try
		{
			if (fireEvents && flag)
			{
				FireNavigationStopped(navState);
			}
			fireEvents2 = true;
		}
		finally
		{
			int i = 0;
			try
			{
				for (; i < _childNavigationServices.Count; i++)
				{
					((NavigationService)_childNavigationServices[i]).DoStopLoading(clearRecursiveNavigations: true, fireEvents2);
				}
			}
			finally
			{
				if (++i < _childNavigationServices.Count)
				{
					for (; i < _childNavigationServices.Count; i++)
					{
						((NavigationService)_childNavigationServices[i]).DoStopLoading(clearRecursiveNavigations: true, fireEvents: false);
					}
				}
				PendingNavigationList.Clear();
				if (_parentNavigationService != null && _parentNavigationService.PendingNavigationList.Contains(this))
				{
					_parentNavigationService.PendingNavigationList.Remove(this);
					if (fireEvents)
					{
						_parentNavigationService.HandleLoadCompleted(null);
					}
				}
			}
		}
	}

	/// <summary>Reloads the current content.</summary>
	public void Refresh()
	{
		if (!IsDisposed)
		{
			if (CanReloadFromUri)
			{
				Navigate(_currentSource, new NavigateInfo(_currentSource, NavigationMode.Refresh));
			}
			else if (_bp != null)
			{
				Navigate(_bp, new NavigateInfo(_currentSource, NavigationMode.Refresh));
			}
		}
	}

	private bool FireNavigating(Uri source, object bp, object navState, WebRequest request)
	{
		NavigateInfo navigateInfo = navState as NavigateInfo;
		Uri uriRelativeToPackAppBase = MS.Internal.Utility.BindUriHelper.GetUriRelativeToPackAppBase(source);
		if (bp != null && navigateInfo != null && !(navigateInfo is PageFunctionReturnInfo) && (!(bp is PageFunctionBase) || !(bp as PageFunctionBase)._Resume) && navigateInfo.Source != null && navigateInfo.NavigationMode == NavigationMode.New)
		{
			return _navigateQueueItem == null;
		}
		CustomContentState customContentState = ((navigateInfo != null && navigateInfo.JournalEntry != null) ? navigateInfo.JournalEntry.CustomContentState : null);
		object extraData = ((navigateInfo == null) ? navState : null);
		NavigatingCancelEventArgs navigatingCancelEventArgs = new NavigatingCancelEventArgs(uriRelativeToPackAppBase, bp, customContentState, extraData, navigateInfo?.NavigationMode ?? NavigationMode.New, request, INavigatorHost, IsNavigationInitiator);
		if (_navigating != null)
		{
			_navigating(INavigatorHost, navigatingCancelEventArgs);
		}
		if (!navigatingCancelEventArgs.Cancel && Application != null && Application.CheckAccess())
		{
			Application.FireNavigating(navigatingCancelEventArgs, _bp == null);
		}
		_customContentStateToSave = navigatingCancelEventArgs.ContentStateToSave;
		if (navigatingCancelEventArgs.Cancel && JournalScope != null)
		{
			JournalScope.AbortJournalNavigation();
		}
		if (!navigatingCancelEventArgs.Cancel)
		{
			return !IsDisposed;
		}
		return false;
	}

	private bool HandleNavigating(Uri source, object content, object navState, WebRequest newRequest, bool navigateOnSourceChanged)
	{
		NavigateInfo navigateInfo = navState as NavigateInfo;
		if (navigateInfo != null && source == null)
		{
			source = navigateInfo.Source;
		}
		NavigateQueueItem navigateQueueItem = new NavigateQueueItem(source, content, navigateInfo?.NavigationMode ?? NavigationMode.New, navState, this);
		_recursiveNavigateList.Add(navigateQueueItem);
		_isNavInitiatorValid = false;
		if (_navigatorHostImpl != null && !navigateOnSourceChanged)
		{
			_navigatorHostImpl.OnSourceUpdatedFromNavService(IsJournalNavigation(navigateInfo));
		}
		bool flag = false;
		try
		{
			flag = FireNavigating(source, content, navState, newRequest);
		}
		catch
		{
			CleanupAfterNavigationCancelled(navigateQueueItem);
			throw;
		}
		if (flag)
		{
			DoStopLoading(clearRecursiveNavigations: false, fireEvents: true);
			if (!_recursiveNavigateList.Contains(navigateQueueItem))
			{
				return false;
			}
			_recursiveNavigateList.Clear();
			_navigateQueueItem = navigateQueueItem;
			_request = newRequest;
			_navStatus = NavigationStatus.Navigating;
		}
		else
		{
			CleanupAfterNavigationCancelled(navigateQueueItem);
		}
		return flag;
	}

	private void CleanupAfterNavigationCancelled(NavigateQueueItem localNavigateQueueItem)
	{
		if (JournalScope != null)
		{
			JournalScope.AbortJournalNavigation();
		}
		_recursiveNavigateList.Remove(localNavigateQueueItem);
		if (_navigatorHostImpl != null)
		{
			_navigatorHostImpl.OnSourceUpdatedFromNavService(journalOrCancel: true);
		}
		InformBrowserAboutStoppedNavigation();
	}

	private void FireNavigated(object navState)
	{
		object extraData = ((navState is NavigateInfo) ? null : navState);
		try
		{
			NavigationEventArgs e = new NavigationEventArgs(CurrentSource, Content, extraData, _webResponse, INavigatorHost, IsNavigationInitiator);
			if (_navigated != null)
			{
				_navigated(INavigatorHost, e);
			}
			if (Application != null && Application.CheckAccess())
			{
				Application.FireNavigated(e);
			}
		}
		catch
		{
			DoStopLoading(clearRecursiveNavigations: true, fireEvents: false);
			throw;
		}
	}

	private void HandleNavigated(object navState, bool navigatedToNewContent)
	{
		BrowserInteropHelper.IsInitialViewerNavigation = false;
		NavigateInfo navigateInfo = navState as NavigateInfo;
		bool flag = false;
		if (navigatedToNewContent && _currentSource != null)
		{
			flag = !string.IsNullOrEmpty(MS.Internal.Utility.BindUriHelper.GetFragment(_currentSource));
		}
		if (navigateInfo != null && navigateInfo.JournalEntry != null)
		{
			JournalEntry journalEntry = navigateInfo.JournalEntry;
			if (journalEntry.CustomContentState != null)
			{
				journalEntry.CustomContentState.Replay(this, navigateInfo.NavigationMode);
				journalEntry.CustomContentState = null;
				if (_navStatus != NavigationStatus.Navigated)
				{
					return;
				}
			}
			if (journalEntry.RootViewerState != null && _navigatorHostImpl != null)
			{
				if (!navigatedToNewContent)
				{
					RestoreRootViewerState(journalEntry.RootViewerState);
					journalEntry.RootViewerState = null;
				}
				else
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			if (_bp is FrameworkContentElement frameworkContentElement)
			{
				frameworkContentElement.Loaded += OnContentLoaded;
			}
			else if (_bp is FrameworkElement frameworkElement)
			{
				frameworkElement.Loaded += OnContentLoaded;
			}
			_cancelContentRenderedHandling = false;
		}
		if (JournalScope != null)
		{
			NavigateQueueItem navigateQueueItem = _navigateQueueItem;
			JournalScope.Journal.UpdateView();
			if (_navigateQueueItem != navigateQueueItem)
			{
				return;
			}
		}
		ResetPendingNavigationState(NavigationStatus.Navigated);
		FireNavigated(navState);
		if (navigatedToNewContent && IsPageFunction(_bp))
		{
			HandlePageFunction(navigateInfo);
		}
		HandleLoadCompleted(navState);
	}

	private void FireNavigationProgress(Uri source)
	{
		if (INavigatorHost is UIElement element)
		{
			AutomationPeer automationPeer = UIElementAutomationPeer.FromElement(element);
			if (automationPeer != null)
			{
				NavigationWindowAutomationPeer.RaiseAsyncContentLoadedEvent(automationPeer, BytesRead, MaxBytes);
			}
		}
		NavigationProgressEventArgs e = new NavigationProgressEventArgs(source, BytesRead, MaxBytes, INavigatorHost);
		try
		{
			if (_navigationProgress != null)
			{
				_navigationProgress(INavigatorHost, e);
			}
			if (Application != null && Application.CheckAccess())
			{
				Application.FireNavigationProgress(e);
			}
		}
		catch
		{
			DoStopLoading(clearRecursiveNavigations: true, fireEvents: false);
			throw;
		}
	}

	private void FireLoadCompleted(bool isNavInitiator, object navState)
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordHosting, EventTrace.Level.Info, EventTrace.Event.Wpf_NavigationEnd);
		object extraData = ((navState is NavigateInfo) ? null : navState);
		NavigationEventArgs e = new NavigationEventArgs(CurrentSource, Content, extraData, _webResponse, INavigatorHost, isNavInitiator);
		try
		{
			if (_loadCompleted != null)
			{
				_loadCompleted(INavigatorHost, e);
			}
			if (Application != null && Application.CheckAccess())
			{
				Application.FireLoadCompleted(e);
			}
		}
		catch
		{
			DoStopLoading(clearRecursiveNavigations: true, fireEvents: false);
			throw;
		}
	}

	private bool FireFragmentNavigation(string fragment)
	{
		if (string.IsNullOrEmpty(fragment))
		{
			return false;
		}
		FragmentNavigationEventArgs fragmentNavigationEventArgs = new FragmentNavigationEventArgs(fragment, INavigatorHost);
		try
		{
			if (_fragmentNavigation != null)
			{
				_fragmentNavigation(this, fragmentNavigationEventArgs);
			}
			if (Application != null && Application.CheckAccess())
			{
				Application.FireFragmentNavigation(fragmentNavigationEventArgs);
			}
		}
		catch
		{
			DoStopLoading(clearRecursiveNavigations: true, fireEvents: false);
			throw;
		}
		return fragmentNavigationEventArgs.Handled;
	}

	private void HandleLoadCompleted(object navState)
	{
		if (navState != null)
		{
			_navState = navState;
		}
		if (_asyncObjectConverter == null && PendingNavigationList.Count == 0 && _navStatus == NavigationStatus.Navigated)
		{
			NavigationService parentNavigationService = ParentNavigationService;
			_navStatus = NavigationStatus.Idle;
			bool isNavigationInitiator = IsNavigationInitiator;
			FireLoadCompleted(isNavigationInitiator, _navState);
			_navState = null;
			if (_webResponse != null)
			{
				_webResponse.Close();
				_webResponse = null;
			}
			if (!isNavigationInitiator && parentNavigationService != null)
			{
				parentNavigationService.PendingNavigationList.Remove(this);
				parentNavigationService.HandleLoadCompleted(null);
			}
		}
	}

	private void FireNavigationStopped(object navState)
	{
		object extraData = ((navState is NavigateInfo) ? null : navState);
		NavigationEventArgs e = new NavigationEventArgs(Source, Content, extraData, null, INavigatorHost, IsNavigationInitiator);
		if (_stopped != null)
		{
			_stopped(INavigatorHost, e);
		}
		if (Application != null && Application.CheckAccess())
		{
			Application.FireNavigationStopped(e);
		}
	}

	private void OnContentLoaded(object sender, RoutedEventArgs args)
	{
		if (_bp is FrameworkContentElement frameworkContentElement)
		{
			frameworkContentElement.Loaded -= OnContentLoaded;
		}
		else
		{
			((FrameworkElement)_bp).Loaded -= OnContentLoaded;
		}
		OnFirstContentLayout();
		_cancelContentRenderedHandling = true;
	}

	private void ContentRenderedHandler(object sender, EventArgs args)
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordHosting, EventTrace.Level.Info, EventTrace.Event.Wpf_NavigationContentRendered);
		if (_cancelContentRenderedHandling)
		{
			_cancelContentRenderedHandling = false;
		}
		else
		{
			OnFirstContentLayout();
		}
	}

	private void OnFirstContentLayout()
	{
		if (CurrentSource != null)
		{
			string fragment = MS.Internal.Utility.BindUriHelper.GetFragment(CurrentSource);
			if (!string.IsNullOrEmpty(fragment))
			{
				NavigateToFragment(fragment, scrollToTopOnEmptyFragment: false);
			}
		}
		if (_journalScope != null)
		{
			JournalEntry currentEntry = _journalScope.Journal.CurrentEntry;
			if (currentEntry != null && currentEntry.RootViewerState != null)
			{
				RestoreRootViewerState(currentEntry.RootViewerState);
				currentEntry.RootViewerState = null;
			}
		}
	}

	internal void DoNavigate(Uri source, NavigationMode f, object navState)
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordHosting, EventTrace.Level.Info, EventTrace.Event.Wpf_NavigationAsyncWorkItem);
		if (IsDisposed)
		{
			return;
		}
		WebResponse webResponse = null;
		try
		{
			if (_request is PackWebRequest)
			{
				webResponse = WpfWebRequestHelper.GetResponse(_request);
				if (webResponse == null)
				{
					Uri uriRelativeToPackAppBase = MS.Internal.Utility.BindUriHelper.GetUriRelativeToPackAppBase(_request.RequestUri);
					throw new Exception(SR.Format(SR.GetResponseFailed, uriRelativeToPackAppBase.ToString()));
				}
				GetObjectFromResponse(_request, webResponse, source, navState);
			}
			else
			{
				RequestState state = new RequestState(_request, source, navState, Dispatcher.CurrentDispatcher);
				_request.BeginGetResponse(HandleWebResponseOnRightDispatcher, state);
			}
		}
		catch (WebException e)
		{
			object extraData = ((navState is NavigateInfo) ? null : navState);
			if (!FireNavigationFailed(new NavigationFailedEventArgs(source, extraData, INavigatorHost, _request, webResponse, e)))
			{
				throw;
			}
		}
		catch (IOException e2)
		{
			object extraData2 = ((navState is NavigateInfo) ? null : navState);
			if (!FireNavigationFailed(new NavigationFailedEventArgs(source, extraData2, INavigatorHost, _request, webResponse, e2)))
			{
				throw;
			}
		}
	}

	private bool FireNavigationFailed(NavigationFailedEventArgs e)
	{
		_navStatus = NavigationStatus.NavigationFailed;
		try
		{
			if (this.NavigationFailed != null)
			{
				this.NavigationFailed(INavigatorHost, e);
			}
			if (!e.Handled)
			{
				NavigationWindow navigationWindow = FindNavigationWindow();
				if (navigationWindow != null && navigationWindow.NavigationService != this)
				{
					navigationWindow.NavigationService.FireNavigationFailed(e);
				}
			}
			if (!e.Handled && Application != null && Application.CheckAccess())
			{
				Application.FireNavigationFailed(e);
			}
		}
		finally
		{
			if (_navStatus == NavigationStatus.NavigationFailed)
			{
				DoStopLoading(clearRecursiveNavigations: true, fireEvents: false);
			}
		}
		return e.Handled;
	}

	private WebRequest CreateWebRequest(Uri resolvedDestinationUri, NavigateInfo navInfo)
	{
		WebRequest webRequest = null;
		try
		{
			webRequest = PackWebRequestFactory.CreateWebRequest(resolvedDestinationUri);
		}
		catch (NotSupportedException)
		{
			if (AppSecurityManager.SafeLaunchBrowserOnlyIfPossible(CurrentSource, resolvedDestinationUri, IsTopLevelContainer) == LaunchResult.NotLaunched)
			{
				throw;
			}
		}
		catch (SecurityException)
		{
			throw;
		}
		bool isRefresh = navInfo != null && navInfo.NavigationMode == NavigationMode.Refresh;
		WpfWebRequestHelper.ConfigCachePolicy(webRequest, isRefresh);
		return webRequest;
	}

	private void HandleWebResponseOnRightDispatcher(IAsyncResult ar)
	{
		if (IsDisposed)
		{
			return;
		}
		Dispatcher callbackDispatcher = ((RequestState)ar.AsyncState).CallbackDispatcher;
		if (Dispatcher.CurrentDispatcher != callbackDispatcher)
		{
			callbackDispatcher.BeginInvoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
			{
				HandleWebResponse(ar);
				return (object)null;
			}, null);
		}
		else
		{
			callbackDispatcher.Invoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate
			{
				HandleWebResponse(ar);
				return (object)null;
			}, null);
		}
	}

	private void HandleWebResponse(IAsyncResult ar)
	{
		if (IsDisposed)
		{
			return;
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordHosting, EventTrace.Level.Info, EventTrace.Event.Wpf_NavigationWebResponseReceived);
		RequestState requestState = (RequestState)ar.AsyncState;
		if (requestState.Request != _request)
		{
			return;
		}
		WebResponse response = null;
		try
		{
			try
			{
				response = WpfWebRequestHelper.EndGetResponse(_request, ar);
			}
			catch
			{
				throw;
			}
			GetObjectFromResponse(_request, response, requestState.Source, requestState.NavState);
		}
		catch (WebException e)
		{
			object extraData = ((requestState.NavState is NavigateInfo) ? null : requestState.NavState);
			if (!FireNavigationFailed(new NavigationFailedEventArgs(requestState.Source, extraData, INavigatorHost, _request, response, e)))
			{
				throw;
			}
		}
		catch (IOException e2)
		{
			object extraData2 = ((requestState.NavState is NavigateInfo) ? null : requestState.NavState);
			if (!FireNavigationFailed(new NavigationFailedEventArgs(requestState.Source, extraData2, INavigatorHost, _request, response, e2)))
			{
				throw;
			}
		}
	}

	private void GetObjectFromResponse(WebRequest request, WebResponse response, Uri destinationUri, object navState)
	{
		bool flag = false;
		ContentType contentType = WpfWebRequestHelper.GetContentType(response);
		try
		{
			Stream responseStream = response.GetResponseStream();
			if (responseStream == null)
			{
				Uri uriRelativeToPackAppBase = MS.Internal.Utility.BindUriHelper.GetUriRelativeToPackAppBase(_request.RequestUri);
				throw new Exception(SR.Format(SR.GetStreamFailed, uriRelativeToPackAppBase.ToString()));
			}
			long contentLength = response.ContentLength;
			Uri uriRelativeToPackAppBase2 = MS.Internal.Utility.BindUriHelper.GetUriRelativeToPackAppBase(destinationUri);
			NavigateInfo navInfo = navState as NavigateInfo;
			bool sandboxExternalContent = SandboxExternalContent && !BaseUriHelper.IsPackApplicationUri(destinationUri) && MimeTypeMapper.XamlMime.AreTypeAndSubTypeEqual(contentType);
			BindStream s = new BindStream(responseStream, contentLength, uriRelativeToPackAppBase2, this, Dispatcher.CurrentDispatcher);
			Invariant.Assert(_webResponse == null && _asyncObjectConverter == null);
			_webResponse = response;
			_asyncObjectConverter = null;
			bool canUseTopLevelBrowser = false;
			object objectAndCloseStreamCore = MimeObjectFactory.GetObjectAndCloseStreamCore(s, contentType, destinationUri, canUseTopLevelBrowser, sandboxExternalContent, allowAsync: true, IsJournalNavigation(navInfo), out _asyncObjectConverter, IsUnsafe);
			if (objectAndCloseStreamCore != null)
			{
				if (_request == request)
				{
					((IContentContainer)this).OnContentReady(contentType, objectAndCloseStreamCore, destinationUri, navState);
					flag = true;
				}
				return;
			}
			try
			{
				if (!IsTopLevelContainer || BrowserInteropHelper.IsInitialViewerNavigation)
				{
					throw new InvalidOperationException(SR.FailedToConvertResource);
				}
				DelegateToBrowser(response is PackWebResponse, destinationUri);
			}
			finally
			{
				DrainResponseStreamForPartialCacheFileBug(responseStream);
				responseStream.Close();
				ResetPendingNavigationState(_navStatus);
			}
		}
		finally
		{
			if (!flag)
			{
				response.Close();
				_webResponse = null;
				if (_asyncObjectConverter != null)
				{
					_asyncObjectConverter.CancelAsync();
					_asyncObjectConverter = null;
				}
			}
		}
	}

	private void DelegateToBrowser(bool isPack, Uri destinationUri)
	{
		try
		{
			if (isPack)
			{
				destinationUri = BaseUriHelper.ConvertPackUriToAbsoluteExternallyVisibleUri(destinationUri);
			}
			if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordHosting, EventTrace.Level.Info))
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.Wpf_NavigationLaunchBrowser, EventTrace.Keyword.KeywordHosting, EventTrace.Level.Info, destinationUri.ToString());
			}
			AppSecurityManager.SafeLaunchBrowserDemandWhenUnsafe(CurrentSource, destinationUri, IsTopLevelContainer);
		}
		finally
		{
			InformBrowserAboutStoppedNavigation();
		}
	}

	private void DrainResponseStreamForPartialCacheFileBug(Stream s)
	{
		if (_request is HttpWebRequest && HttpWebRequest.DefaultCachePolicy != null && HttpWebRequest.DefaultCachePolicy is HttpRequestCachePolicy)
		{
			StreamReader streamReader = new StreamReader(s);
			streamReader.ReadToEnd();
			streamReader.Close();
		}
	}

	internal void DoNavigate(object bp, NavigationMode navFlags, object navState)
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordHosting, EventTrace.Level.Info, EventTrace.Event.Wpf_NavigationAsyncWorkItem);
		if (!IsDisposed)
		{
			NavigateInfo navigateInfo = navState as NavigateInfo;
			Invariant.Assert((navFlags != NavigationMode.Refresh) ^ (bp == _bp), "Navigating to the same object should be handled as fragment navigation, except for Refresh.");
			Uri orgUri = navigateInfo?.Source;
			Uri resolvedUri = MS.Internal.Utility.BindUriHelper.GetResolvedUri(null, orgUri);
			((IContentContainer)this).OnContentReady((ContentType)null, bp, resolvedUri, navState);
		}
	}

	private JournalEntry UpdateJournal(NavigationMode navigationMode, JournalReason journalReason, JournalEntry destinationJournalEntry)
	{
		JournalEntry journalEntry = null;
		if (!_doNotJournalCurrentContent)
		{
			journalEntry = MakeJournalEntry(journalReason);
		}
		if (journalEntry == null)
		{
			_doNotJournalCurrentContent = false;
			if ((navigationMode == NavigationMode.Back || navigationMode == NavigationMode.Forward) && JournalScope != null)
			{
				JournalScope.Journal.CommitJournalNavigation(destinationJournalEntry);
			}
			return null;
		}
		JournalNavigationScope journalNavigationScope = EnsureJournal();
		if (journalNavigationScope == null)
		{
			return null;
		}
		if (_bp is PageFunctionBase pageFunctionBase && navigationMode == NavigationMode.New && pageFunctionBase.Content == null)
		{
			journalEntry.EntryType = JournalEntryType.UiLess;
		}
		journalNavigationScope.Journal.UpdateCurrentEntry(journalEntry);
		if (navigationMode == NavigationMode.New)
		{
			journalNavigationScope.Journal.RecordNewNavigation();
		}
		else
		{
			journalNavigationScope.Journal.CommitJournalNavigation(destinationJournalEntry);
		}
		_customContentStateToSave = null;
		return journalEntry;
	}

	internal JournalEntry MakeJournalEntry(JournalReason journalReason)
	{
		if (_bp == null)
		{
			return null;
		}
		if (_journalEntryGroupState == null)
		{
			_journalEntryGroupState = new JournalEntryGroupState(_guidId, _contentId);
		}
		bool flag = IsContentKeepAlive();
		JournalEntry journalEntry;
		if (_bp is PageFunctionBase pageFunctionBase)
		{
			if (flag)
			{
				journalEntry = new JournalEntryPageFunctionKeepAlive(_journalEntryGroupState, pageFunctionBase);
			}
			else
			{
				Uri uri = pageFunctionBase.GetValue(BaseUriHelper.BaseUriProperty) as Uri;
				if (uri != null)
				{
					Invariant.Assert(uri.IsAbsoluteUri, "BaseUri for root element should be absolute.");
					journalEntry = new JournalEntryPageFunctionUri(markupUri: (!(_currentCleanSource != null) || MS.Internal.Utility.BindUriHelper.StartWithFragment(_currentCleanSource)) ? uri : _currentSource, jeGroupState: _journalEntryGroupState, pageFunction: pageFunctionBase);
				}
				else
				{
					journalEntry = new JournalEntryPageFunctionType(_journalEntryGroupState, pageFunctionBase);
				}
			}
			journalEntry.Source = _currentCleanSource;
		}
		else
		{
			journalEntry = ((!flag) ? ((JournalEntry)new JournalEntryUri(_journalEntryGroupState, _currentCleanSource)) : ((JournalEntry)new JournalEntryKeepAlive(_journalEntryGroupState, _currentCleanSource, _bp)));
		}
		CustomContentState customContentState = _customContentStateToSave;
		if (customContentState == null && _bp is IProvideCustomContentState provideCustomContentState)
		{
			customContentState = provideCustomContentState.GetContentState();
		}
		if (customContentState != null)
		{
			Type type = customContentState.GetType();
			if (!type.IsSerializable)
			{
				throw new SystemException(SR.Format(SR.CustomContentStateMustBeSerializable, type));
			}
			journalEntry.CustomContentState = customContentState;
		}
		if (_rootViewerStateToSave != null)
		{
			journalEntry.RootViewerState = _rootViewerStateToSave;
			_rootViewerStateToSave = null;
		}
		else
		{
			journalEntry.RootViewerState = GetRootViewerState(journalReason);
		}
		string text = null;
		if (journalEntry.CustomContentState != null)
		{
			text = journalEntry.CustomContentState.JournalEntryName;
		}
		if (string.IsNullOrEmpty(text))
		{
			if (_bp is DependencyObject dependencyObject)
			{
				text = (string)dependencyObject.GetValue(JournalEntry.NameProperty);
				if (string.IsNullOrEmpty(text) && dependencyObject is Page)
				{
					text = (dependencyObject as Page).Title;
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				if (_currentSource != null)
				{
					string fragment = MS.Internal.Utility.BindUriHelper.GetFragment(_currentSource);
					if (!string.IsNullOrEmpty(fragment))
					{
						text = text + "#" + fragment;
					}
				}
			}
			else
			{
				NavigationWindow navigationWindow = ((JournalScope == null) ? null : (JournalScope.NavigatorHost as NavigationWindow));
				text = ((navigationWindow != null && this == navigationWindow.NavigationService && !string.IsNullOrEmpty(navigationWindow.Title)) ? ((!(CurrentSource != null)) ? navigationWindow.Title : string.Format(CultureInfo.CurrentCulture, "{0} ({1})", navigationWindow.Title, JournalEntry.GetDisplayName(_currentSource, SiteOfOriginContainer.SiteOfOrigin))) : ((!(CurrentSource != null)) ? SR.Untitled : JournalEntry.GetDisplayName(_currentSource, SiteOfOriginContainer.SiteOfOrigin)));
			}
		}
		journalEntry.Name = text;
		if (journalReason == JournalReason.NewContentNavigation)
		{
			journalEntry.SaveState(_bp);
		}
		return journalEntry;
	}

	internal void RequestCustomContentStateOnAppShutdown()
	{
		_isNavInitiator = false;
		_isNavInitiatorValid = true;
		FireNavigating(null, null, null, null);
	}

	internal void Dispose()
	{
		_disposed = true;
		StopLoading();
		foreach (NavigationService childNavigationService in ChildNavigationServices)
		{
			childNavigationService.Dispose();
		}
		_journalScope = null;
		_bp = null;
		_currentSource = null;
		_currentCleanSource = null;
		_oldRootVisual = null;
		_childNavigationServices.Clear();
		_parentNavigationService = null;
		_webBrowser = null;
	}

	private NavigationWindow FindNavigationWindow()
	{
		NavigationService navigationService = this;
		while (navigationService != null && navigationService.INavigatorHost != null)
		{
			if (navigationService.INavigatorHost is NavigationWindow result)
			{
				return result;
			}
			navigationService = navigationService.ParentNavigationService;
		}
		return null;
	}

	internal static bool IsPageFunction(object content)
	{
		return content is PageFunctionBase;
	}

	private void SetupPageFunctionHandlers(object bp)
	{
		PageFunctionBase pageFunctionBase = bp as PageFunctionBase;
		if (bp != null)
		{
			pageFunctionBase.FinishHandler = FinishHandler;
			new ReturnEventSaver()._Detach(pageFunctionBase);
		}
	}

	private void HandlePageFunction(NavigateInfo navInfo)
	{
		PageFunctionBase pageFunctionBase = (PageFunctionBase)_bp;
		if (IsJournalNavigation(navInfo))
		{
			pageFunctionBase._Resume = true;
		}
		if (!pageFunctionBase._Resume)
		{
			pageFunctionBase.CallStart();
		}
	}

	private void HandleFinish(PageFunctionBase endingPF, object ReturnEventArgs)
	{
		if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordHosting, EventTrace.Level.Info))
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.Wpf_NavigationPageFunctionReturn, EventTrace.Keyword.KeywordHosting, EventTrace.Level.Info, endingPF.ToString());
		}
		if (JournalScope == null)
		{
			throw new InvalidOperationException(SR.WindowAlreadyClosed);
		}
		Journal journal = JournalScope.Journal;
		PageFunctionBase pageFunctionBase = null;
		int parentPageJournalIndex = JournalEntryPageFunction.GetParentPageJournalIndex(this, journal, endingPF);
		if (endingPF.RemoveFromJournal)
		{
			DoRemoveFromJournal(endingPF, parentPageJournalIndex);
		}
		if (parentPageJournalIndex != -1 && journal[parentPageJournalIndex] is JournalEntryPageFunction journalEntryPageFunction)
		{
			pageFunctionBase = journalEntryPageFunction.ResumePageFunction();
			pageFunctionBase.FinishHandler = FinishHandler;
			FireChildPageFunctionReturnEvent(pageFunctionBase, endingPF, ReturnEventArgs);
		}
		if (_navigateQueueItem == null)
		{
			if (parentPageJournalIndex != -1 && parentPageJournalIndex < journal.TotalCount && !IsDisposed)
			{
				NavigateToParentPage(endingPF, pageFunctionBase, ReturnEventArgs, parentPageJournalIndex);
			}
			return;
		}
		if (parentPageJournalIndex < journal.TotalCount)
		{
			((JournalEntryPageFunction)journal[parentPageJournalIndex]).SaveState(pageFunctionBase);
		}
		pageFunctionBase.FinishHandler = null;
	}

	private void FireChildPageFunctionReturnEvent(object parentElem, PageFunctionBase childPF, object ReturnEventArgs)
	{
		ReturnEventSaver saver = childPF._Saver;
		if (saver == null)
		{
			return;
		}
		saver._Attach(parentElem, childPF);
		Window window = null;
		DependencyObject dependencyObject = parentElem as DependencyObject;
		if (dependencyObject != null && !dependencyObject.IsSealed)
		{
			dependencyObject.SetValue(NavigationServiceProperty, this);
			if (INavigatorHost is DependencyObject dependencyObject2 && (window = Window.GetWindow(dependencyObject2)) != null)
			{
				dependencyObject.SetValue(Window.IWindowServiceProperty, window);
			}
		}
		try
		{
			childPF._OnFinish(ReturnEventArgs);
		}
		catch
		{
			DoStopLoading(clearRecursiveNavigations: true, fireEvents: false);
			throw;
		}
		finally
		{
			saver._Detach(childPF);
			if (dependencyObject != null && !dependencyObject.IsSealed)
			{
				dependencyObject.ClearValue(NavigationServiceProperty);
				if (window != null)
				{
					dependencyObject.ClearValue(Window.IWindowServiceProperty);
				}
			}
		}
	}

	private void DoRemoveFromJournal(PageFunctionBase finishingChildPageFunction, int parentEntryIndex)
	{
		if (!finishingChildPageFunction.RemoveFromJournal)
		{
			return;
		}
		bool flag = false;
		Journal journal = JournalScope.Journal;
		int num = parentEntryIndex + 1;
		while (num < journal.TotalCount)
		{
			if (!flag)
			{
				flag = journal[num] is JournalEntryPageFunction journalEntryPageFunction && journalEntryPageFunction.PageFunctionId == finishingChildPageFunction.PageFunctionId;
			}
			if (flag)
			{
				journal.RemoveEntryInternal(num);
			}
			else
			{
				num++;
			}
		}
		if (flag)
		{
			journal.UpdateView();
		}
		else if (_bp == finishingChildPageFunction)
		{
			journal.ClearForwardStack();
		}
		_doNotJournalCurrentContent = true;
	}

	private void NavigateToParentPage(PageFunctionBase finishingChildPageFunction, PageFunctionBase parentPF, object returnEventArgs, int parentIndex)
	{
		JournalEntry journalEntry = JournalScope.Journal[parentIndex];
		if (parentPF != null)
		{
			if (journalEntry.EntryType == JournalEntryType.UiLess)
			{
				throw new InvalidOperationException(SR.UiLessPageFunctionNotCallingOnReturn);
			}
			NavigateInfo navigationState = (finishingChildPageFunction.RemoveFromJournal ? new NavigateInfo(journalEntry.Source, NavigationMode.Back, journalEntry) : new NavigateInfo(journalEntry.Source, NavigationMode.New));
			Navigate(parentPF, navigationState);
			return;
		}
		PageFunctionReturnInfo navigationState2 = (finishingChildPageFunction.RemoveFromJournal ? new PageFunctionReturnInfo(finishingChildPageFunction, journalEntry.Source, NavigationMode.Back, journalEntry, returnEventArgs) : new PageFunctionReturnInfo(finishingChildPageFunction, journalEntry.Source, NavigationMode.New, null, returnEventArgs));
		if (journalEntry is JournalEntryUri)
		{
			Navigate(journalEntry.Source, navigationState2);
		}
		else if (journalEntry is JournalEntryKeepAlive)
		{
			object keepAliveRoot = ((JournalEntryKeepAlive)journalEntry).KeepAliveRoot;
			Navigate(keepAliveRoot, navigationState2);
		}
	}

	private bool IsValidRootElement(object bp)
	{
		bool result = true;
		if (!AllowWindowNavigation && bp != null && bp is Window)
		{
			result = false;
		}
		return result;
	}
}
