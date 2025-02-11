using System.Collections;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal.AppModel;
using MS.Internal.KnownBoxes;
using MS.Internal.Utility;

namespace System.Windows.Navigation;

/// <summary>Represents a window that supports content navigation.</summary>
[ContentProperty]
[TemplatePart(Name = "PART_NavWinCP", Type = typeof(ContentPresenter))]
public class NavigationWindow : Window, INavigator, INavigatorBase, INavigatorImpl, IDownloader, IJournalNavigationScopeHost, IUriContext
{
	/// <summary>Identifies the <see cref="P:System.Windows.Navigation.NavigationWindow.SandboxExternalContent" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Navigation.NavigationWindow.SandboxExternalContent" /> dependency property.</returns>
	public static readonly DependencyProperty SandboxExternalContentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Navigation.NavigationWindow.ShowsNavigationUI" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Navigation.NavigationWindow.ShowsNavigationUI" /> dependency property.</returns>
	public static readonly DependencyProperty ShowsNavigationUIProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Navigation.NavigationWindow.BackStack" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Navigation.NavigationWindow.BackStack" /> dependency property.</returns>
	public static readonly DependencyProperty BackStackProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Navigation.NavigationWindow.ForwardStack" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Navigation.NavigationWindow.ForwardStack" /> dependency property.</returns>
	public static readonly DependencyProperty ForwardStackProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Navigation.NavigationWindow.CanGoBack" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Navigation.NavigationWindow.CanGoBack" /> dependency property.</returns>
	public static readonly DependencyProperty CanGoBackProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Navigation.NavigationWindow.CanGoForward" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Navigation.NavigationWindow.CanGoForward" /> dependency property.</returns>
	public static readonly DependencyProperty CanGoForwardProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Navigation.NavigationWindow.Source" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Navigation.NavigationWindow.Source" /> dependency property.</returns>
	public static readonly DependencyProperty SourceProperty;

	private NavigationService _navigationService;

	private JournalNavigationScope _JNS;

	private bool _sourceUpdatedFromNavService;

	private bool _fFramelet;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets a value that indicates whether a <see cref="T:System.Windows.Navigation.NavigationWindow" /> isolates external Extensible Application Markup Language (XAML) content within a partial trust security sandbox (with default Internet zone permission set).  </summary>
	/// <returns>true if content is isolated within a partial trust security sandbox; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.Security.SecurityException">
	///   <see cref="P:System.Windows.Navigation.NavigationWindow.SandboxExternalContent" /> is set when an application is executing in partial trust.</exception>
	public bool SandboxExternalContent
	{
		get
		{
			return (bool)GetValue(SandboxExternalContentProperty);
		}
		set
		{
			bool value2 = value;
			SetValue(SandboxExternalContentProperty, value2);
		}
	}

	NavigationService IDownloader.Downloader => _navigationService;

	/// <summary>Gets or sets the base uniform resource identifier (URI) of the current context.</summary>
	/// <returns>The base URI of the current context.</returns>
	Uri IUriContext.BaseUri
	{
		get
		{
			return (Uri)GetValue(BaseUriHelper.BaseUriProperty);
		}
		set
		{
			SetValue(BaseUriHelper.BaseUriProperty, value);
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Navigation.NavigationService" /> that is used by this <see cref="T:System.Windows.Navigation.NavigationWindow" /> to provide navigation services to its content.</summary>
	/// <returns>The navigation service used by this <see cref="T:System.Windows.Navigation.NavigationWindow" />.</returns>
	public NavigationService NavigationService
	{
		get
		{
			VerifyContextAndObjectState();
			return _navigationService;
		}
	}

	/// <summary>Gets an <see cref="T:System.Collections.IEnumerable" /> that you use to enumerate the entries in back navigation history for a <see cref="T:System.Windows.Navigation.NavigationWindow" />.  </summary>
	/// <returns>
	///   <see cref="T:System.Collections.IEnumerable" /> if at least one entry has been added to back navigation history, or null if there are not entries or the <see cref="T:System.Windows.Navigation.NavigationWindow" /> does not own its own navigation history.</returns>
	public IEnumerable BackStack => _JNS.BackStack;

	/// <summary>Gets an <see cref="T:System.Collections.IEnumerable" /> that you use to enumerate the entries in back navigation history for a <see cref="T:System.Windows.Navigation.NavigationWindow" />.  </summary>
	/// <returns>
	///   <see cref="T:System.Collections.IEnumerable" /> if at least one entry has been added to forward navigation history, or null if there are no entries or the <see cref="T:System.Windows.Navigation.NavigationWindow" /> does not own its own navigation history.</returns>
	public IEnumerable ForwardStack => _JNS.ForwardStack;

	/// <summary>Gets or sets a value that indicates whether a <see cref="T:System.Windows.Navigation.NavigationWindow" /> shows its navigation UI.  </summary>
	/// <returns>true if the navigation UI is displayed; otherwise, false. The default is true.</returns>
	public bool ShowsNavigationUI
	{
		get
		{
			VerifyContextAndObjectState();
			return (bool)GetValue(ShowsNavigationUIProperty);
		}
		set
		{
			VerifyContextAndObjectState();
			SetValue(ShowsNavigationUIProperty, value);
		}
	}

	/// <summary>Gets or sets the uniform resource identifier (URI) of the current content, or the URI of new content that is currently being navigated to.  </summary>
	/// <returns>The URI for the current content, or the content that is currently being navigated to.</returns>
	[DefaultValue(null)]
	public Uri Source
	{
		get
		{
			return (Uri)GetValue(SourceProperty);
		}
		set
		{
			SetValue(SourceProperty, value);
		}
	}

	/// <summary>Gets the uniform resource identifier (URI) of the content that was last navigated to.</summary>
	/// <returns>The URI for the content that was last navigated to, if navigated to by using a URI; otherwise, null.</returns>
	public Uri CurrentSource
	{
		get
		{
			VerifyContextAndObjectState();
			if (_navigationService != null)
			{
				return _navigationService.CurrentSource;
			}
			return null;
		}
	}

	/// <summary>Gets a value that indicates whether there is at least one entry in forward navigation history.  </summary>
	/// <returns>true if there is at least one entry in forward navigation history; false if there are no entries in forward navigation history, or the <see cref="T:System.Windows.Navigation.NavigationWindow" /> does not own its own navigation history.</returns>
	public bool CanGoForward => _JNS.CanGoForward;

	/// <summary>Gets a value that indicates whether there is at least one entry in back navigation history.  </summary>
	/// <returns>true if there is at least one entry in back navigation history; false if there are no entries in back navigation history or the <see cref="T:System.Windows.Navigation.NavigationWindow" /> does not own its own navigation history.</returns>
	public bool CanGoBack => _JNS.CanGoBack;

	NavigationService IJournalNavigationScopeHost.NavigationService => _navigationService;

	internal Journal Journal => _JNS.Journal;

	internal JournalNavigationScope JournalNavigationScope => _JNS;

	private bool InAppShutdown => Application.IsShuttingDown;

	internal override int EffectiveValuesInitialSize => 42;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Occurs when a new navigation is requested.</summary>
	public event NavigatingCancelEventHandler Navigating
	{
		add
		{
			VerifyContextAndObjectState();
			NavigationService.Navigating += value;
		}
		remove
		{
			VerifyContextAndObjectState();
			NavigationService.Navigating -= value;
		}
	}

	/// <summary>Occurs periodically during a download to provide navigation progress information.</summary>
	public event NavigationProgressEventHandler NavigationProgress
	{
		add
		{
			VerifyContextAndObjectState();
			NavigationService.NavigationProgress += value;
		}
		remove
		{
			VerifyContextAndObjectState();
			NavigationService.NavigationProgress -= value;
		}
	}

	/// <summary>Occurs when an error is raised while navigating to the requested content.</summary>
	public event NavigationFailedEventHandler NavigationFailed
	{
		add
		{
			VerifyContextAndObjectState();
			NavigationService.NavigationFailed += value;
		}
		remove
		{
			VerifyContextAndObjectState();
			NavigationService.NavigationFailed -= value;
		}
	}

	/// <summary>Occurs when the content that is being navigated to has been found, and is available from the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property, although it may not have completed loading.</summary>
	public event NavigatedEventHandler Navigated
	{
		add
		{
			VerifyContextAndObjectState();
			NavigationService.Navigated += value;
		}
		remove
		{
			VerifyContextAndObjectState();
			NavigationService.Navigated -= value;
		}
	}

	/// <summary>Occurs when content that was navigated to has been loaded, parsed, and has begun rendering.</summary>
	public event LoadCompletedEventHandler LoadCompleted
	{
		add
		{
			VerifyContextAndObjectState();
			NavigationService.LoadCompleted += value;
		}
		remove
		{
			VerifyContextAndObjectState();
			NavigationService.LoadCompleted -= value;
		}
	}

	/// <summary>Occurs when the <see cref="M:System.Windows.Navigation.NavigationWindow.StopLoading" /> method is called, or when a new navigation is requested while a current navigation is in progress.</summary>
	public event NavigationStoppedEventHandler NavigationStopped
	{
		add
		{
			VerifyContextAndObjectState();
			NavigationService.NavigationStopped += value;
		}
		remove
		{
			VerifyContextAndObjectState();
			NavigationService.NavigationStopped -= value;
		}
	}

	/// <summary>Occurs when navigation to a content fragment begins, which occurs immediately, if the desired fragment is in the current content, or after the source XAML content has been loaded, if the desired fragment is in different content.</summary>
	public event FragmentNavigationEventHandler FragmentNavigation
	{
		add
		{
			VerifyContextAndObjectState();
			NavigationService.FragmentNavigation += value;
		}
		remove
		{
			VerifyContextAndObjectState();
			NavigationService.FragmentNavigation -= value;
		}
	}

	private static void OnSandboxExternalContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		NavigationWindow navigationWindow = (NavigationWindow)d;
		if ((bool)e.NewValue && !(bool)e.OldValue)
		{
			navigationWindow.NavigationService.Refresh();
		}
	}

	private static object CoerceSandBoxExternalContentValue(DependencyObject d, object value)
	{
		return (bool)value;
	}

	static NavigationWindow()
	{
		SandboxExternalContentProperty = Frame.SandboxExternalContentProperty.AddOwner(typeof(NavigationWindow));
		ShowsNavigationUIProperty = DependencyProperty.Register("ShowsNavigationUI", typeof(bool), typeof(NavigationWindow), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		BackStackProperty = JournalNavigationScope.BackStackProperty.AddOwner(typeof(NavigationWindow));
		ForwardStackProperty = JournalNavigationScope.ForwardStackProperty.AddOwner(typeof(NavigationWindow));
		CanGoBackProperty = JournalNavigationScope.CanGoBackProperty.AddOwner(typeof(NavigationWindow));
		CanGoForwardProperty = JournalNavigationScope.CanGoForwardProperty.AddOwner(typeof(NavigationWindow));
		SourceProperty = Frame.SourceProperty.AddOwner(typeof(NavigationWindow), new FrameworkPropertyMetadata(null, OnSourcePropertyChanged));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(NavigationWindow));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigationWindow), new FrameworkPropertyMetadata(typeof(NavigationWindow)));
		ContentControl.ContentProperty.OverrideMetadata(typeof(NavigationWindow), new FrameworkPropertyMetadata(null, CoerceContent));
		SandboxExternalContentProperty.OverrideMetadata(typeof(NavigationWindow), new FrameworkPropertyMetadata(OnSandboxExternalContentPropertyChanged, CoerceSandBoxExternalContentValue));
		CommandManager.RegisterClassCommandBinding(typeof(NavigationWindow), new CommandBinding(NavigationCommands.BrowseBack, OnGoBack, OnQueryGoBack));
		CommandManager.RegisterClassCommandBinding(typeof(NavigationWindow), new CommandBinding(NavigationCommands.BrowseForward, OnGoForward, OnQueryGoForward));
		CommandManager.RegisterClassCommandBinding(typeof(NavigationWindow), new CommandBinding(NavigationCommands.NavigateJournal, OnNavigateJournal));
		CommandManager.RegisterClassCommandBinding(typeof(NavigationWindow), new CommandBinding(NavigationCommands.Refresh, OnRefresh, OnQueryRefresh));
		CommandManager.RegisterClassCommandBinding(typeof(NavigationWindow), new CommandBinding(NavigationCommands.BrowseStop, OnBrowseStop, OnQueryBrowseStop));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Navigation.NavigationWindow" /> class. </summary>
	public NavigationWindow()
	{
		Initialize();
	}

	internal NavigationWindow(bool inRbw)
		: base(inRbw)
	{
		Initialize();
	}

	private void Initialize()
	{
		_navigationService = new NavigationService(this);
		_navigationService.BPReady += OnBPReady;
		_JNS = new JournalNavigationScope(this);
		_fFramelet = false;
	}

	/// <summary>Navigates asynchronously to content that is specified by a uniform resource identifier (URI).</summary>
	/// <returns>true if a navigation is not canceled; otherwise, false.</returns>
	/// <param name="source">A <see cref="T:System.Uri" /> object initialized with the URI for the desired content.</param>
	public bool Navigate(Uri source)
	{
		VerifyContextAndObjectState();
		return NavigationService.Navigate(source);
	}

	/// <summary>Navigates asynchronously to source content located at a uniform resource identifier (URI), and pass an object that contains data to be used for processing during navigation.</summary>
	/// <returns>true if a navigation is not canceled; otherwise, false.</returns>
	/// <param name="source">A <see cref="T:System.Uri" /> object initialized with the URI for the desired content.</param>
	/// <param name="extraData">A <see cref="T:System.Object" /> that contains data to be used for processing during navigation.</param>
	public bool Navigate(Uri source, object extraData)
	{
		VerifyContextAndObjectState();
		return NavigationService.Navigate(source, extraData);
	}

	/// <summary>Navigates asynchronously to content that is contained by an object.</summary>
	/// <returns>true if a navigation is not canceled; otherwise, false.</returns>
	/// <param name="content">An <see cref="T:System.Object" /> that contains the content to navigate to.</param>
	public bool Navigate(object content)
	{
		VerifyContextAndObjectState();
		return NavigationService.Navigate(content);
	}

	/// <summary>Navigates asynchronously to content that is contained by an object, and passes an object that contains data to be used for processing during navigation.</summary>
	/// <returns>true if a navigation is not canceled; otherwise, false.</returns>
	/// <param name="content">An <see cref="T:System.Object" /> that contains the content to navigate to.</param>
	/// <param name="extraData">A <see cref="T:System.Object" /> that contains data to be used for processing during navigation.</param>
	public bool Navigate(object content, object extraData)
	{
		VerifyContextAndObjectState();
		return NavigationService.Navigate(content, extraData);
	}

	JournalNavigationScope INavigator.GetJournal(bool create)
	{
		return _JNS;
	}

	/// <summary>Navigates to the most recent item in forward navigation history.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Windows.Navigation.NavigationWindow.GoForward" /> is called when there are no entries in forward navigation history.</exception>
	public void GoForward()
	{
		_JNS.GoForward();
	}

	/// <summary>Navigates to the most recent item in back navigation history.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Windows.Navigation.NavigationWindow.GoBack" /> is called when there are no entries in back navigation history.</exception>
	public void GoBack()
	{
		_JNS.GoBack();
	}

	/// <summary>Stops further downloading of content for the current navigation request.</summary>
	public void StopLoading()
	{
		VerifyContextAndObjectState();
		if (!InAppShutdown)
		{
			NavigationService.StopLoading();
		}
	}

	/// <summary>Reloads the current content.</summary>
	public void Refresh()
	{
		VerifyContextAndObjectState();
		if (!InAppShutdown)
		{
			NavigationService.Refresh();
		}
	}

	/// <summary>Adds an entry to back navigation history that contains a <see cref="T:System.Windows.Navigation.CustomContentState" /> object.</summary>
	/// <param name="state">A <see cref="T:System.Windows.Navigation.CustomContentState" /> object that represents application-defined state that is associated with a specific piece of content.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="state" /> is null, and a <see cref="T:System.Windows.Navigation.CustomContentState" /> object isn't returned from <see cref="M:System.Windows.Navigation.IProvideCustomContentState.GetContentState" />.</exception>
	public void AddBackEntry(CustomContentState state)
	{
		VerifyContextAndObjectState();
		NavigationService.AddBackEntry(state);
	}

	/// <summary>Removes the most recent journal entry from back history.</summary>
	/// <returns>The most recent <see cref="T:System.Windows.Navigation.JournalEntry" /> in back navigation history, if there is one.</returns>
	public JournalEntry RemoveBackEntry()
	{
		return _JNS.RemoveBackEntry();
	}

	/// <summary>Called when the template generation for the visual tree is created.</summary>
	public override void OnApplyTemplate()
	{
		VerifyContextAndObjectState();
		base.OnApplyTemplate();
		FrameworkElement frameworkElement = GetVisualChild(0) as FrameworkElement;
		if (_navigationService != null)
		{
			_navigationService.VisualTreeAvailable(frameworkElement);
		}
		if (frameworkElement != null && frameworkElement.Name == "NavigationBarRoot")
		{
			if (!_fFramelet)
			{
				_fFramelet = true;
			}
		}
		else if (_fFramelet)
		{
			_fFramelet = false;
		}
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool ShouldSerializeContent()
	{
		if (_navigationService != null)
		{
			return !_navigationService.CanReloadFromUri;
		}
		return true;
	}

	private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		NavigationWindow navigationWindow = (NavigationWindow)d;
		if (!navigationWindow._sourceUpdatedFromNavService)
		{
			Uri uriToNavigate = MS.Internal.Utility.BindUriHelper.GetUriToNavigate(navigationWindow, d.GetValue(BaseUriHelper.BaseUriProperty) as Uri, (Uri)e.NewValue);
			navigationWindow._navigationService.Navigate(uriToNavigate, null, sandboxExternalContent: false, navigateOnSourceChanged: true);
		}
	}

	void INavigatorImpl.OnSourceUpdatedFromNavService(bool journalOrCancel)
	{
		try
		{
			_sourceUpdatedFromNavService = true;
			SetCurrentValueInternal(SourceProperty, _navigationService.Source);
		}
		finally
		{
			_sourceUpdatedFromNavService = false;
		}
	}

	/// <summary>Creates and returns a <see cref="T:System.Windows.Automation.Peers.NavigationWindowAutomationPeer" /> object for this <see cref="T:System.Windows.Navigation.NavigationWindow" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.NavigationWindowAutomationPeer" /> object for this <see cref="T:System.Windows.Navigation.NavigationWindow" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new NavigationWindowAutomationPeer(this);
	}

	/// <summary>Adds a child object.</summary>
	/// <param name="value">The child object to add.</param>
	/// <exception cref="T:System.InvalidOperationException">when this method is called. This prevents content from being added to <see cref="T:System.Windows.Navigation.NavigationWindow" /> using XAML.</exception>
	protected override void AddChild(object value)
	{
		throw new InvalidOperationException(SR.NoAddChild);
	}

	/// <summary>Adds text to the object.</summary>
	/// <param name="text">The text to add to the object.</param>
	/// <exception cref="T:System.ArgumentException">if the text parameter value contains non-whitespace characters.</exception>
	protected override void AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Window.Closed" /> event.</summary>
	/// <param name="args">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	protected override void OnClosed(EventArgs args)
	{
		VerifyContextAndObjectState();
		base.OnClosed(args);
		if (_navigationService != null)
		{
			_navigationService.Dispose();
		}
	}

	internal override void OnPreApplyTemplate()
	{
		base.OnPreApplyTemplate();
		_JNS.EnsureJournal();
	}

	void IJournalNavigationScopeHost.VerifyContextAndObjectState()
	{
		VerifyContextAndObjectState();
	}

	void IJournalNavigationScopeHost.OnJournalAvailable()
	{
	}

	bool IJournalNavigationScopeHost.GoBackOverride()
	{
		return false;
	}

	bool IJournalNavigationScopeHost.GoForwardOverride()
	{
		return false;
	}

	Visual INavigatorImpl.FindRootViewer()
	{
		return NavigationHelper.FindRootViewer(this, "PART_NavWinCP");
	}

	private static object CoerceContent(DependencyObject d, object value)
	{
		NavigationWindow navigationWindow = (NavigationWindow)d;
		if (navigationWindow.NavigationService.Content == value)
		{
			return value;
		}
		navigationWindow.Navigate(value);
		return DependencyProperty.UnsetValue;
	}

	private void OnBPReady(object sender, BPReadyEventArgs e)
	{
		base.Content = e.Content;
	}

	private static void OnGoBack(object sender, ExecutedRoutedEventArgs args)
	{
		(sender as NavigationWindow).GoBack();
	}

	private static void OnQueryGoBack(object sender, CanExecuteRoutedEventArgs e)
	{
		NavigationWindow navigationWindow = sender as NavigationWindow;
		e.CanExecute = navigationWindow.CanGoBack;
		e.ContinueRouting = !navigationWindow.CanGoBack;
	}

	private static void OnGoForward(object sender, ExecutedRoutedEventArgs e)
	{
		(sender as NavigationWindow).GoForward();
	}

	private static void OnQueryGoForward(object sender, CanExecuteRoutedEventArgs e)
	{
		NavigationWindow navigationWindow = sender as NavigationWindow;
		e.CanExecute = navigationWindow.CanGoForward;
		e.ContinueRouting = !navigationWindow.CanGoForward;
	}

	private static void OnRefresh(object sender, ExecutedRoutedEventArgs e)
	{
		(sender as NavigationWindow).Refresh();
	}

	private static void OnQueryRefresh(object sender, CanExecuteRoutedEventArgs e)
	{
		NavigationWindow navigationWindow = sender as NavigationWindow;
		e.CanExecute = navigationWindow.Content != null;
	}

	private static void OnBrowseStop(object sender, ExecutedRoutedEventArgs e)
	{
		(sender as NavigationWindow).StopLoading();
	}

	private static void OnQueryBrowseStop(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = true;
	}

	private static void OnNavigateJournal(object sender, ExecutedRoutedEventArgs e)
	{
		NavigationWindow navigationWindow = sender as NavigationWindow;
		if (e.Parameter is FrameworkElement { DataContext: JournalEntry dataContext })
		{
			navigationWindow.JournalNavigationScope.NavigateToEntry(dataContext);
		}
	}
}
