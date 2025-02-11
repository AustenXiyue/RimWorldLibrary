using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.AppModel;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;
using MS.Internal.Utility;

namespace System.Windows.Controls;

/// <summary>Frame is a content control that supports navigation.</summary>
[DefaultProperty("Source")]
[DefaultEvent("Navigated")]
[Localizability(LocalizationCategory.Ignore)]
[ContentProperty]
[TemplatePart(Name = "PART_FrameCP", Type = typeof(ContentPresenter))]
public class Frame : ContentControl, INavigator, INavigatorBase, INavigatorImpl, IJournalNavigationScopeHost, IDownloader, IJournalState, IAddChild, IUriContext
{
	[Serializable]
	private class FramePersistState : CustomJournalStateInternal
	{
		internal JournalEntry JournalEntry;

		internal Guid NavSvcGuid;

		internal JournalOwnership JournalOwnership;

		internal Journal Journal;

		internal override void PrepareForSerialization()
		{
			if (JournalEntry != null && JournalEntry.IsAlive())
			{
				JournalEntry = null;
			}
			if (Journal != null)
			{
				Journal.PruneKeepAliveEntries();
			}
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Frame.Source" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Frame.Source" /> dependency property.</returns>
	public static readonly DependencyProperty SourceProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Frame.CanGoBack" /> dependency property.</summary>
	public static readonly DependencyProperty CanGoBackProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Frame.CanGoForward" /> dependency property.</summary>
	public static readonly DependencyProperty CanGoForwardProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Frame.BackStack" /> dependency property.</summary>
	public static readonly DependencyProperty BackStackProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Frame.ForwardStack" /> dependency property.</summary>
	public static readonly DependencyProperty ForwardStackProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Frame.NavigationUIVisibility" /> dependency property.</summary>
	public static readonly DependencyProperty NavigationUIVisibilityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Frame.SandboxExternalContent" /> dependency property.</summary>
	public static readonly DependencyProperty SandboxExternalContentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Frame.JournalOwnership" /> dependency property.</summary>
	public static readonly DependencyProperty JournalOwnershipProperty;

	private bool _postContentRenderedFromLoadedHandler;

	private DispatcherOperation _contentRenderedCallback;

	private NavigationService _navigationService;

	private bool _sourceUpdatedFromNavService;

	private JournalOwnership _journalOwnership;

	private JournalNavigationScope _ownJournalScope;

	private List<CommandBinding> _commandBindings;

	private static DependencyObjectType _dType;

	/// <summary>For a description of this member, see <see cref="P:System.Windows.Markup.IUriContext.BaseUri" />.</summary>
	/// <returns>The base URI of the current context.</returns>
	Uri IUriContext.BaseUri
	{
		get
		{
			return BaseUri;
		}
		set
		{
			BaseUri = value;
		}
	}

	/// <summary>Gets or sets the base uniform resource identifier (URI) for a <see cref="T:System.Windows.Controls.Frame" />.</summary>
	/// <returns>The base uniform resource identifier (URI) of the <see cref="T:System.Windows.Controls.Frame" /> control.</returns>
	protected virtual Uri BaseUri
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

	NavigationService IDownloader.Downloader => _navigationService;

	/// <summary>Gets or sets the uniform resource identifier (URI) of the current content, or the URI of new content that is currently being navigated to. </summary>
	/// <returns>A <see cref="T:System.Uri" /> that contains the URI for the current content, or the content that is currently being navigated to.</returns>
	[Bindable(true)]
	[CustomCategory("Navigation")]
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

	/// <summary>Gets or sets when the <see cref="T:System.Windows.Controls.Frame" /> can show its navigation UI. </summary>
	/// <returns>A <see cref="T:System.Windows.Navigation.NavigationUIVisibility" /> value that specifies when the <see cref="T:System.Windows.Controls.Frame" /> can show its navigation UI. The default value is <see cref="F:System.Windows.Navigation.NavigationUIVisibility.Automatic" />.</returns>
	public NavigationUIVisibility NavigationUIVisibility
	{
		get
		{
			return (NavigationUIVisibility)GetValue(NavigationUIVisibilityProperty);
		}
		set
		{
			SetValue(NavigationUIVisibilityProperty, value);
		}
	}

	/// <summary>Gets or sets whether a <see cref="T:System.Windows.Controls.Frame" /> isolates external Extensible Application Markup Language (XAML) content within a partial trust security sandbox (with the default Internet permission set). </summary>
	/// <returns>true if content is isolated within a partial trust security sandbox; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.Security.SecurityException">
	///   <see cref="P:System.Windows.Controls.Frame.SandboxExternalContent" /> is set when an application is executing in partial trust.</exception>
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

	/// <summary>Gets or sets whether a <see cref="T:System.Windows.Controls.Frame" /> is responsible for managing its own navigation history, or yields navigation history management to a parent navigator (<see cref="T:System.Windows.Navigation.NavigationWindow" />, <see cref="T:System.Windows.Controls.Frame" />).</summary>
	/// <returns>A <see cref="T:System.Windows.Navigation.JournalOwnership" /> value that specifies whether <see cref="T:System.Windows.Controls.Frame" /> manages its own journal. The default value is <see cref="F:System.Windows.Navigation.JournalOwnership.Automatic" />.</returns>
	public JournalOwnership JournalOwnership
	{
		get
		{
			return _journalOwnership;
		}
		set
		{
			if (value != _journalOwnership)
			{
				SetValue(JournalOwnershipProperty, value);
			}
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Navigation.NavigationService" /> that is used by this <see cref="T:System.Windows.Controls.Frame" /> to provide navigation services.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.Frame" /> object that represents the <see cref="T:System.Windows.Navigation.NavigationService" /> used by this <see cref="T:System.Windows.Controls.Frame" />, if one is available. Otherwise, null is returned.</returns>
	public NavigationService NavigationService
	{
		get
		{
			VerifyAccess();
			return _navigationService;
		}
	}

	/// <summary>Gets a value that indicates whether there is at least one entry in forward navigation history. </summary>
	/// <returns>true if there is at least one entry in forward navigation history; false if there are no entries in forward navigation history or the <see cref="T:System.Windows.Controls.Frame" /> does not own its own navigation history.</returns>
	public bool CanGoForward
	{
		get
		{
			if (_ownJournalScope != null)
			{
				return _ownJournalScope.CanGoForward;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether there is at least one entry in back navigation history. </summary>
	/// <returns>true if there is at least one entry in back navigation history; false if there are no entries in back navigation history or the <see cref="T:System.Windows.Controls.Frame" /> does not own its own navigation history.</returns>
	public bool CanGoBack
	{
		get
		{
			if (_ownJournalScope != null)
			{
				return _ownJournalScope.CanGoBack;
			}
			return false;
		}
	}

	/// <summary>Gets the uniform resource identifier (URI) of the content that was last navigated to.</summary>
	/// <returns>A <see cref="T:System.Uri" /> for the content that was last navigated to, if navigated to by using a URI; otherwise, null.</returns>
	public Uri CurrentSource => _navigationService.CurrentSource;

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerable" /> that you use to enumerate the entries in back navigation history for a <see cref="T:System.Windows.Controls.Frame" />. </summary>
	/// <returns>
	///   <see cref="T:System.Collections.IEnumerable" /> if at least one entry has been added to back navigation history. If there are not entries, or the <see cref="T:System.Windows.Controls.Frame" /> does not own its own navigation history, <see cref="P:System.Windows.Controls.Frame.BackStack" /> is null.</returns>
	public IEnumerable BackStack
	{
		get
		{
			if (_ownJournalScope != null)
			{
				return _ownJournalScope.BackStack;
			}
			return null;
		}
	}

	/// <summary>Gets an <see cref="T:System.Collections.IEnumerable" /> that you use to enumerate the entries in forward navigation history for a <see cref="T:System.Windows.Controls.Frame" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerable" /> object if at least one entry has been added to forward navigation history, or null if there are no entries or the <see cref="T:System.Windows.Controls.Frame" /> does not own its own navigation history.</returns>
	public IEnumerable ForwardStack
	{
		get
		{
			if (_ownJournalScope != null)
			{
				return _ownJournalScope.ForwardStack;
			}
			return null;
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Occurs after <see cref="T:System.Windows.Controls.Frame" /> content has been rendered.</summary>
	public event EventHandler ContentRendered;

	/// <summary>Occurs when a new navigation is requested.</summary>
	public event NavigatingCancelEventHandler Navigating
	{
		add
		{
			_navigationService.Navigating += value;
		}
		remove
		{
			_navigationService.Navigating -= value;
		}
	}

	/// <summary>Occurs periodically during a download to provide navigation progress information.</summary>
	public event NavigationProgressEventHandler NavigationProgress
	{
		add
		{
			_navigationService.NavigationProgress += value;
		}
		remove
		{
			_navigationService.NavigationProgress -= value;
		}
	}

	/// <summary>Occurs when an error is raised while navigating to the requested content.</summary>
	public event NavigationFailedEventHandler NavigationFailed
	{
		add
		{
			_navigationService.NavigationFailed += value;
		}
		remove
		{
			_navigationService.NavigationFailed -= value;
		}
	}

	/// <summary>Occurs when the content that is being navigated to has been found, and is available from the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property, although it may not have completed loading.</summary>
	public event NavigatedEventHandler Navigated
	{
		add
		{
			_navigationService.Navigated += value;
		}
		remove
		{
			_navigationService.Navigated -= value;
		}
	}

	/// <summary>Occurs when content that was navigated to has been loaded, parsed, and has begun rendering.</summary>
	public event LoadCompletedEventHandler LoadCompleted
	{
		add
		{
			_navigationService.LoadCompleted += value;
		}
		remove
		{
			_navigationService.LoadCompleted -= value;
		}
	}

	/// <summary>Occurs when the <see cref="M:System.Windows.Controls.Frame.StopLoading" /> method is called, or when a new navigation is requested while a current navigation is in progress.</summary>
	public event NavigationStoppedEventHandler NavigationStopped
	{
		add
		{
			_navigationService.NavigationStopped += value;
		}
		remove
		{
			_navigationService.NavigationStopped -= value;
		}
	}

	/// <summary>Occurs when navigation to a content fragment begins, which occurs immediately, if the desired fragment is in the current content, or after the source XAML content has been loaded, if the desired fragment is in different content.</summary>
	public event FragmentNavigationEventHandler FragmentNavigation
	{
		add
		{
			_navigationService.FragmentNavigation += value;
		}
		remove
		{
			_navigationService.FragmentNavigation -= value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Frame" /> class.</summary>
	public Frame()
	{
		Init();
	}

	static Frame()
	{
		SourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(Frame), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Journal, OnSourcePropertyChanged, CoerceSource));
		CanGoBackProperty = JournalNavigationScope.CanGoBackProperty.AddOwner(typeof(Frame));
		CanGoForwardProperty = JournalNavigationScope.CanGoForwardProperty.AddOwner(typeof(Frame));
		BackStackProperty = JournalNavigationScope.BackStackProperty.AddOwner(typeof(Frame));
		ForwardStackProperty = JournalNavigationScope.ForwardStackProperty.AddOwner(typeof(Frame));
		NavigationUIVisibilityProperty = DependencyProperty.Register("NavigationUIVisibility", typeof(NavigationUIVisibility), typeof(Frame), new PropertyMetadata(NavigationUIVisibility.Automatic));
		SandboxExternalContentProperty = DependencyProperty.Register("SandboxExternalContent", typeof(bool), typeof(Frame), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, OnSandboxExternalContentPropertyChanged, CoerceSandBoxExternalContentValue));
		JournalOwnershipProperty = DependencyProperty.Register("JournalOwnership", typeof(JournalOwnership), typeof(Frame), new FrameworkPropertyMetadata(JournalOwnership.Automatic, OnJournalOwnershipPropertyChanged, CoerceJournalOwnership), ValidateJournalOwnershipValue);
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Frame), new FrameworkPropertyMetadata(typeof(Frame)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(Frame));
		ContentControl.ContentProperty.OverrideMetadata(typeof(Frame), new FrameworkPropertyMetadata(null, CoerceContent));
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(Frame), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
		KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(Frame), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
		NavigationService.NavigationServiceProperty.OverrideMetadata(typeof(Frame), new FrameworkPropertyMetadata(OnParentNavigationServiceChanged));
		ControlsTraceLogger.AddControl(TelemetryControls.Frame);
	}

	private static object CoerceContent(DependencyObject d, object value)
	{
		Frame frame = (Frame)d;
		if (frame._navigationService.Content == value)
		{
			return value;
		}
		frame.Navigate(value);
		return DependencyProperty.UnsetValue;
	}

	private void Init()
	{
		base.InheritanceBehavior = InheritanceBehavior.SkipToAppNow;
		base.ContentIsNotLogical = true;
		_navigationService = new NavigationService(this);
		_navigationService.BPReady += _OnBPReady;
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.Frame.ContentRendered" /> event.</summary>
	/// <param name="args">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	protected virtual void OnContentRendered(EventArgs args)
	{
		if (base.Content is DependencyObject element)
		{
			FocusManager.GetFocusedElement(element)?.Focus();
		}
		if (this.ContentRendered != null)
		{
			this.ContentRendered(this, args);
		}
	}

	private static object CoerceSource(DependencyObject d, object value)
	{
		Frame frame = (Frame)d;
		if (frame._sourceUpdatedFromNavService)
		{
			Invariant.Assert(frame._navigationService != null, "_navigationService should never be null here");
			return frame._navigationService.Source;
		}
		return value;
	}

	private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Frame frame = (Frame)d;
		if (!frame._sourceUpdatedFromNavService)
		{
			Uri uriToNavigate = MS.Internal.Utility.BindUriHelper.GetUriToNavigate(frame, ((IUriContext)frame).BaseUri, (Uri)e.NewValue);
			frame._navigationService.Navigate(uriToNavigate, null, sandboxExternalContent: false, navigateOnSourceChanged: true);
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

	private static void OnSandboxExternalContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Frame frame = (Frame)d;
		if ((bool)e.NewValue && !(bool)e.OldValue)
		{
			frame.NavigationService.Refresh();
		}
	}

	private static object CoerceSandBoxExternalContentValue(DependencyObject d, object value)
	{
		return (bool)value;
	}

	private static bool ValidateJournalOwnershipValue(object value)
	{
		JournalOwnership journalOwnership = (JournalOwnership)value;
		if (journalOwnership != 0 && journalOwnership != JournalOwnership.UsesParentJournal)
		{
			return journalOwnership == JournalOwnership.OwnsJournal;
		}
		return true;
	}

	private static void OnJournalOwnershipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Frame)d).OnJournalOwnershipPropertyChanged((JournalOwnership)e.NewValue);
	}

	private void OnJournalOwnershipPropertyChanged(JournalOwnership newValue)
	{
		switch (_journalOwnership)
		{
		case JournalOwnership.Automatic:
			switch (newValue)
			{
			case JournalOwnership.OwnsJournal:
				SwitchToOwnJournal();
				break;
			case JournalOwnership.UsesParentJournal:
				SwitchToParentJournal();
				break;
			}
			break;
		case JournalOwnership.OwnsJournal:
			if (newValue != 0 && newValue == JournalOwnership.UsesParentJournal)
			{
				SwitchToParentJournal();
			}
			break;
		case JournalOwnership.UsesParentJournal:
			switch (newValue)
			{
			case JournalOwnership.Automatic:
				_navigationService.InvalidateJournalNavigationScope();
				break;
			case JournalOwnership.OwnsJournal:
				SwitchToOwnJournal();
				break;
			}
			break;
		}
		_journalOwnership = newValue;
	}

	private static object CoerceJournalOwnership(DependencyObject d, object newValue)
	{
		if (((Frame)d)._journalOwnership == JournalOwnership.OwnsJournal && (JournalOwnership)newValue == JournalOwnership.Automatic)
		{
			return JournalOwnership.OwnsJournal;
		}
		return newValue;
	}

	/// <summary>Creates and returns a <see cref="T:System.Windows.Automation.Peers.NavigationWindowAutomationPeer" /> object for this <see cref="T:System.Windows.Controls.Frame" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.NavigationWindowAutomationPeer" /> object for this <see cref="T:System.Windows.Controls.Frame" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new FrameAutomationPeer(this);
	}

	/// <summary>Adds a child object. </summary>
	/// <param name="value">The child object to add.</param>
	protected override void AddChild(object value)
	{
		throw new InvalidOperationException(SR.FrameNoAddChild);
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	protected override void AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	private void _OnBPReady(object o, BPReadyEventArgs e)
	{
		SetCurrentValueInternal(ContentControl.ContentProperty, e.Content);
		InvalidateMeasure();
		if (base.IsLoaded)
		{
			PostContentRendered();
		}
		else if (!_postContentRenderedFromLoadedHandler)
		{
			base.Loaded += LoadedHandler;
			_postContentRenderedFromLoadedHandler = true;
		}
	}

	private void LoadedHandler(object sender, RoutedEventArgs args)
	{
		if (_postContentRenderedFromLoadedHandler)
		{
			PostContentRendered();
			_postContentRenderedFromLoadedHandler = false;
			base.Loaded -= LoadedHandler;
		}
	}

	private void PostContentRendered()
	{
		if (_contentRenderedCallback != null)
		{
			_contentRenderedCallback.Abort();
		}
		_contentRenderedCallback = base.Dispatcher.BeginInvoke(DispatcherPriority.Input, (DispatcherOperationCallback)delegate(object arg)
		{
			Frame obj = (Frame)arg;
			obj._contentRenderedCallback = null;
			obj.OnContentRendered(EventArgs.Empty);
			return (object)null;
		}, this);
	}

	private void OnQueryGoBack(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = _ownJournalScope.CanGoBack;
		e.Handled = true;
	}

	private void OnGoBack(object sender, ExecutedRoutedEventArgs e)
	{
		_ownJournalScope.GoBack();
		e.Handled = true;
	}

	private void OnQueryGoForward(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = _ownJournalScope.CanGoForward;
		e.Handled = true;
	}

	private void OnGoForward(object sender, ExecutedRoutedEventArgs e)
	{
		_ownJournalScope.GoForward();
		e.Handled = true;
	}

	private void OnNavigateJournal(object sender, ExecutedRoutedEventArgs e)
	{
		if (e.Parameter is FrameworkElement { DataContext: JournalEntry dataContext } && _ownJournalScope.NavigateToEntry(dataContext))
		{
			e.Handled = true;
		}
	}

	private void OnQueryRefresh(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = base.Content != null;
	}

	private void OnRefresh(object sender, ExecutedRoutedEventArgs e)
	{
		_navigationService.Refresh();
		e.Handled = true;
	}

	private void OnBrowseStop(object sender, ExecutedRoutedEventArgs e)
	{
		_ownJournalScope.StopLoading();
		e.Handled = true;
	}

	internal override object AdjustEventSource(RoutedEventArgs e)
	{
		e.Source = this;
		return this;
	}

	internal override string GetPlainText()
	{
		if (Source != null)
		{
			return Source.ToString();
		}
		return string.Empty;
	}

	/// <summary>Allows derived classes to determine the serialization behavior of the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property.</summary>
	/// <returns>true if the content should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool ShouldSerializeContent()
	{
		Invariant.Assert(_navigationService != null, "_navigationService should never be null here");
		if (!_navigationService.CanReloadFromUri)
		{
			return base.Content != null;
		}
		return false;
	}

	private static void OnParentNavigationServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Frame)d).NavigationService.OnParentNavigationServiceChanged();
	}

	/// <summary>Called when the template generation for the visual tree is created.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		Visual templateChild = TemplateChild;
		if (templateChild != null)
		{
			NavigationService.VisualTreeAvailable(templateChild);
		}
	}

	Visual INavigatorImpl.FindRootViewer()
	{
		return NavigationHelper.FindRootViewer(this, "PART_FrameCP");
	}

	JournalNavigationScope INavigator.GetJournal(bool create)
	{
		return GetJournal(create);
	}

	private JournalNavigationScope GetJournal(bool create)
	{
		Invariant.Assert((_ownJournalScope != null) ^ (_journalOwnership != JournalOwnership.OwnsJournal));
		if (_ownJournalScope != null)
		{
			return _ownJournalScope;
		}
		JournalNavigationScope parentJournal = GetParentJournal(create);
		if (parentJournal != null)
		{
			SetCurrentValueInternal(JournalOwnershipProperty, JournalOwnership.UsesParentJournal);
			return parentJournal;
		}
		if (create && _journalOwnership == JournalOwnership.Automatic)
		{
			SetCurrentValueInternal(JournalOwnershipProperty, JournalOwnership.OwnsJournal);
		}
		return _ownJournalScope;
	}

	/// <summary>Adds an entry to back navigation history that contains a <see cref="T:System.Windows.Navigation.CustomContentState" /> object.</summary>
	/// <param name="state">A <see cref="T:System.Windows.Navigation.CustomContentState" /> object that represents application-defined state that is associated with a specific piece of content.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="state" /> is null, and a <see cref="T:System.Windows.Navigation.CustomContentState" /> object is not returned from <see cref="M:System.Windows.Navigation.IProvideCustomContentState.GetContentState" />.</exception>
	public void AddBackEntry(CustomContentState state)
	{
		VerifyAccess();
		_navigationService.AddBackEntry(state);
	}

	/// <summary>Removes the most recent journal entry from back history.</summary>
	/// <returns>The most recent <see cref="T:System.Windows.Navigation.JournalEntry" /> in back navigation history, if there is one.</returns>
	public JournalEntry RemoveBackEntry()
	{
		if (_ownJournalScope == null)
		{
			throw new InvalidOperationException(SR.InvalidOperation_NoJournal);
		}
		return _ownJournalScope.RemoveBackEntry();
	}

	/// <summary>Navigates asynchronously to content that is specified by a uniform resource identifier (URI).</summary>
	/// <returns>true if navigation is not canceled; otherwise, false.</returns>
	/// <param name="source">A <see cref="T:System.Uri" /> object initialized with the URI for the desired content.</param>
	public bool Navigate(Uri source)
	{
		VerifyAccess();
		return _navigationService.Navigate(source);
	}

	/// <summary>Navigates asynchronously to source content located at a uniform resource identifier (URI), and passes an object that contains data to be used for processing during navigation.</summary>
	/// <returns>true if navigation is not canceled; otherwise, false.</returns>
	/// <param name="source">A <see cref="T:System.Uri" /> object initialized with the URI for the desired content.</param>
	/// <param name="extraData">A <see cref="T:System.Object" /> that contains data to be used for processing during navigation.</param>
	public bool Navigate(Uri source, object extraData)
	{
		VerifyAccess();
		return _navigationService.Navigate(source, extraData);
	}

	/// <summary>Navigates asynchronously to content that is contained by an object.</summary>
	/// <returns>true if navigation is not canceled; otherwise, false.</returns>
	/// <param name="content">An <see cref="T:System.Object" /> that contains the content to navigate to.</param>
	public bool Navigate(object content)
	{
		VerifyAccess();
		return _navigationService.Navigate(content);
	}

	/// <summary>Navigates asynchronously to content that is contained by an object, and passes an object that contains data to be used for processing during navigation.</summary>
	/// <returns>true if navigation is not canceled; otherwise, false.</returns>
	/// <param name="content">An <see cref="T:System.Object" /> that contains the content to navigate to.</param>
	/// <param name="extraData">A <see cref="T:System.Object" /> that contains data to be used for processing during navigation.</param>
	public bool Navigate(object content, object extraData)
	{
		VerifyAccess();
		return _navigationService.Navigate(content, extraData);
	}

	/// <summary>Navigates to the most recent item in forward navigation history, if a <see cref="T:System.Windows.Controls.Frame" /> manages its own navigation history.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Windows.Controls.Frame.GoForward" /> is called when there are no entries in back navigation history.</exception>
	public void GoForward()
	{
		if (_ownJournalScope == null)
		{
			throw new InvalidOperationException(SR.InvalidOperation_NoJournal);
		}
		_ownJournalScope.GoForward();
	}

	/// <summary>Navigates to the most recent item in back navigation history, if a <see cref="T:System.Windows.Controls.Frame" /> manages its own navigation history.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Windows.Controls.Frame.GoBack" /> is called when there are no entries in back navigation history.</exception>
	public void GoBack()
	{
		if (_ownJournalScope == null)
		{
			throw new InvalidOperationException(SR.InvalidOperation_NoJournal);
		}
		_ownJournalScope.GoBack();
	}

	/// <summary>Stops further downloading of content for the current navigation request.</summary>
	public void StopLoading()
	{
		VerifyAccess();
		_navigationService.StopLoading();
	}

	/// <summary>Reloads the current content.</summary>
	public void Refresh()
	{
		VerifyAccess();
		_navigationService.Refresh();
	}

	void IJournalNavigationScopeHost.VerifyContextAndObjectState()
	{
		VerifyAccess();
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

	CustomJournalStateInternal IJournalState.GetJournalState(JournalReason journalReason)
	{
		if (journalReason != JournalReason.NewContentNavigation)
		{
			return null;
		}
		FramePersistState framePersistState = new FramePersistState();
		framePersistState.JournalEntry = _navigationService.MakeJournalEntry(JournalReason.NewContentNavigation);
		framePersistState.NavSvcGuid = _navigationService.GuidId;
		framePersistState.JournalOwnership = _journalOwnership;
		if (_ownJournalScope != null)
		{
			framePersistState.Journal = _ownJournalScope.Journal;
		}
		return framePersistState;
	}

	void IJournalState.RestoreJournalState(CustomJournalStateInternal cjs)
	{
		FramePersistState framePersistState = (FramePersistState)cjs;
		_navigationService.GuidId = framePersistState.NavSvcGuid;
		JournalOwnership = framePersistState.JournalOwnership;
		if (_journalOwnership == JournalOwnership.OwnsJournal)
		{
			Invariant.Assert(framePersistState.Journal != null);
			_ownJournalScope.Journal = framePersistState.Journal;
		}
		if (framePersistState.JournalEntry != null)
		{
			framePersistState.JournalEntry.Navigate(this, NavigationMode.Back);
		}
	}

	internal override void OnPreApplyTemplate()
	{
		base.OnPreApplyTemplate();
		if (_ownJournalScope != null)
		{
			_ownJournalScope.EnsureJournal();
		}
	}

	internal override void OnThemeChanged()
	{
		if (!base.HasTemplateGeneratedSubTree && base.Content is DependencyObject d)
		{
			Helper.DowncastToFEorFCE(d, out var fe, out var fce, throwIfNeither: false);
			if (fe != null || fce != null)
			{
				TreeWalkHelper.InvalidateOnResourcesChange(fe, fce, ResourcesChangeInfo.ThemeChangeInfo);
			}
		}
	}

	private JournalNavigationScope GetParentJournal(bool create)
	{
		JournalNavigationScope result = null;
		NavigationService parentNavigationService = _navigationService.ParentNavigationService;
		if (parentNavigationService != null)
		{
			result = parentNavigationService.INavigatorHost.GetJournal(create);
		}
		return result;
	}

	private void SwitchToOwnJournal()
	{
		if (_ownJournalScope == null)
		{
			GetParentJournal(create: false)?.Journal.RemoveEntries(_navigationService.GuidId);
			_ownJournalScope = new JournalNavigationScope(this);
			_navigationService.InvalidateJournalNavigationScope();
			if (base.IsLoaded)
			{
				_ownJournalScope.EnsureJournal();
			}
			AddCommandBinding(new CommandBinding(NavigationCommands.BrowseBack, OnGoBack, OnQueryGoBack));
			AddCommandBinding(new CommandBinding(NavigationCommands.BrowseForward, OnGoForward, OnQueryGoForward));
			AddCommandBinding(new CommandBinding(NavigationCommands.NavigateJournal, OnNavigateJournal));
			AddCommandBinding(new CommandBinding(NavigationCommands.Refresh, OnRefresh, OnQueryRefresh));
			AddCommandBinding(new CommandBinding(NavigationCommands.BrowseStop, OnBrowseStop));
		}
		_journalOwnership = JournalOwnership.OwnsJournal;
	}

	private void SwitchToParentJournal()
	{
		if (_ownJournalScope != null)
		{
			_ownJournalScope = null;
			_navigationService.InvalidateJournalNavigationScope();
			JournalNavigationScope.ClearDPValues(this);
			foreach (CommandBinding commandBinding in _commandBindings)
			{
				base.CommandBindings.Remove(commandBinding);
			}
			_commandBindings = null;
		}
		_journalOwnership = JournalOwnership.UsesParentJournal;
	}

	private void AddCommandBinding(CommandBinding b)
	{
		base.CommandBindings.Add(b);
		if (_commandBindings == null)
		{
			_commandBindings = new List<CommandBinding>(6);
		}
		_commandBindings.Add(b);
	}
}
