using System.ComponentModel;
using System.Threading;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.AppModel;
using MS.Internal.Commands;
using MS.Internal.PresentationFramework;

namespace System.Windows.Documents;

/// <summary>An inline-level flow content element that provides facilities for hosting hyperlinks within flow content.</summary>
[TextElementEditingBehavior(IsMergeable = false, IsTypographicOnly = false)]
public class Hyperlink : Span, ICommandSource, IUriContext
{
	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Hyperlink.Command" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Hyperlink.Command" /> dependency property.</returns>
	public static readonly DependencyProperty CommandProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Hyperlink.CommandParameter" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Hyperlink.CommandParameter" /> dependency property.</returns>
	public static readonly DependencyProperty CommandParameterProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Hyperlink.CommandTarget" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Hyperlink.CommandTarget" /> dependency property.</returns>
	public static readonly DependencyProperty CommandTargetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Hyperlink.NavigateUri" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Hyperlink.NavigateUri" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty NavigateUriProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Hyperlink.TargetName" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Hyperlink.TargetName" /> dependency property.</returns>
	public static readonly DependencyProperty TargetNameProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Documents.Hyperlink.RequestNavigate" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Documents.Hyperlink.RequestNavigate" /> routed event.</returns>
	public static readonly RoutedEvent RequestNavigateEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Documents.Hyperlink.Click" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Documents.Hyperlink.Click" /> routed event.</returns>
	public static readonly RoutedEvent ClickEvent;

	internal static readonly RoutedEvent RequestSetStatusBarEvent;

	[ThreadStatic]
	private static MS.Internal.SecurityCriticalDataForSet<Uri> s_cachedNavigateUri;

	[ThreadStatic]
	private static MS.Internal.SecurityCriticalDataForSet<int?> s_criticalNavigateUriProtectee;

	private static MS.Internal.SecurityCriticalDataForSet<bool?> s_shouldPreventUriSpoofing;

	private bool _canExecute = true;

	private static readonly DependencyProperty IsHyperlinkPressedProperty;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets a command to associate with the <see cref="T:System.Windows.Documents.Hyperlink" />.  </summary>
	/// <returns>A command to associate with the <see cref="T:System.Windows.Documents.Hyperlink" />. The default is null.</returns>
	[Bindable(true)]
	[Category("Action")]
	[Localizability(LocalizationCategory.NeverLocalize)]
	public ICommand Command
	{
		get
		{
			return (ICommand)GetValue(CommandProperty);
		}
		set
		{
			SetValue(CommandProperty, value);
		}
	}

	private bool CanExecute
	{
		get
		{
			return _canExecute;
		}
		set
		{
			if (_canExecute != value)
			{
				_canExecute = value;
				CoerceValue(ContentElement.IsEnabledProperty);
			}
		}
	}

	private bool IsEditable
	{
		get
		{
			if (base.TextContainer.TextSelection != null)
			{
				return !base.TextContainer.TextSelection.TextEditor.IsReadOnly;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether or not the <see cref="T:System.Windows.Documents.Hyperlink" /> is enabled.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Documents.Hyperlink" /> is enabled; otherwise, false.</returns>
	protected override bool IsEnabledCore
	{
		get
		{
			if (base.IsEnabledCore)
			{
				return CanExecute;
			}
			return false;
		}
	}

	/// <summary>Gets or sets command parameters associated with the command specified by the <see cref="P:System.Windows.Documents.Hyperlink.Command" /> property.  </summary>
	/// <returns>An object specifying parameters for the command specified by the <see cref="P:System.Windows.Documents.Hyperlink.Command" /> property. The default is null.</returns>
	[Bindable(true)]
	[Category("Action")]
	[Localizability(LocalizationCategory.NeverLocalize)]
	public object CommandParameter
	{
		get
		{
			return GetValue(CommandParameterProperty);
		}
		set
		{
			SetValue(CommandParameterProperty, value);
		}
	}

	/// <summary>Gets or sets a target element on which to execute the command specified by the <see cref="P:System.Windows.Documents.Hyperlink.Command" /> property.  </summary>
	/// <returns>A target element on which to execute the command specified by the <see cref="P:System.Windows.Documents.Hyperlink.Command" /> property. The default is null.</returns>
	[Bindable(true)]
	[Category("Action")]
	public IInputElement CommandTarget
	{
		get
		{
			return (IInputElement)GetValue(CommandTargetProperty);
		}
		set
		{
			SetValue(CommandTargetProperty, value);
		}
	}

	/// <summary>Gets or sets a URI to navigate to when the <see cref="T:System.Windows.Documents.Hyperlink" /> is activated. </summary>
	/// <returns>The <see cref="T:System.Uri" /> to navigate to when the <see cref="T:System.Windows.Documents.Hyperlink" /> is activated. The default is null.Note<see cref="T:System.Windows.Documents.Hyperlink" /> can navigate to the value of the <see cref="P:System.Windows.Documents.Hyperlink.NavigateUri" /> property only if either the direct or indirect parent of a <see cref="T:System.Windows.Documents.Hyperlink" /> is a navigation host, including <see cref="T:System.Windows.Navigation.NavigationWindow" />, <see cref="T:System.Windows.Controls.Frame" />, or any browser that can host XBAPs (which includes Internet Explorer 7, Microsoft Internet Explorer 6, and Firefox 2.0+). For more information, see the Navigation Hosts section in Navigation Overview.</returns>
	[Bindable(true)]
	[CustomCategory("Navigation")]
	[Localizability(LocalizationCategory.Hyperlink)]
	public Uri NavigateUri
	{
		get
		{
			return (Uri)GetValue(NavigateUriProperty);
		}
		set
		{
			SetValue(NavigateUriProperty, value);
		}
	}

	/// <summary>Gets or sets the name of a target window or frame for the <see cref="T:System.Windows.Documents.Hyperlink" />.  </summary>
	/// <returns>A string specifying the name of a target window or frame for the <see cref="T:System.Windows.Documents.Hyperlink" />.</returns>
	[Bindable(true)]
	[CustomCategory("Navigation")]
	[Localizability(LocalizationCategory.None, Modifiability = Modifiability.Unmodifiable)]
	public string TargetName
	{
		get
		{
			return (string)GetValue(TargetNameProperty);
		}
		set
		{
			SetValue(TargetNameProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 19;

	/// <summary>Gets or sets a base URI for the <see cref="T:System.Windows.Documents.Hyperlink" />.</summary>
	/// <returns>A base URI for the <see cref="T:System.Windows.Documents.Hyperlink" />.</returns>
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

	/// <summary>Gets or sets a base URI for the <see cref="T:System.Windows.Documents.Hyperlink" />.</summary>
	/// <returns>A base URI for the <see cref="T:System.Windows.Documents.Hyperlink" />.</returns>
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

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	internal string Text => TextRangeBase.GetTextInternal(base.ContentStart, base.ContentEnd);

	private static bool ShouldPreventUriSpoofing
	{
		get
		{
			if (!s_shouldPreventUriSpoofing.Value.HasValue)
			{
				s_shouldPreventUriSpoofing.Value = false;
			}
			return s_shouldPreventUriSpoofing.Value.Value;
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Occurs when navigation events are requested.</summary>
	public event RequestNavigateEventHandler RequestNavigate
	{
		add
		{
			AddHandler(RequestNavigateEvent, value);
		}
		remove
		{
			RemoveHandler(RequestNavigateEvent, value);
		}
	}

	/// <summary>Occurs when the left mouse button is clicked on a <see cref="T:System.Windows.Documents.Hyperlink" />.</summary>
	[Category("Behavior")]
	public event RoutedEventHandler Click
	{
		add
		{
			AddHandler(ClickEvent, value);
		}
		remove
		{
			RemoveHandler(ClickEvent, value);
		}
	}

	static Hyperlink()
	{
		CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(Hyperlink), new FrameworkPropertyMetadata(null, OnCommandChanged));
		CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(Hyperlink), new FrameworkPropertyMetadata(null, OnCommandParameterChanged));
		CommandTargetProperty = DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(Hyperlink), new FrameworkPropertyMetadata((object)null));
		NavigateUriProperty = DependencyProperty.Register("NavigateUri", typeof(Uri), typeof(Hyperlink), new FrameworkPropertyMetadata(null, OnNavigateUriChanged, CoerceNavigateUri));
		TargetNameProperty = DependencyProperty.Register("TargetName", typeof(string), typeof(Hyperlink), new FrameworkPropertyMetadata(string.Empty));
		RequestNavigateEvent = EventManager.RegisterRoutedEvent("RequestNavigate", RoutingStrategy.Bubble, typeof(RequestNavigateEventHandler), typeof(Hyperlink));
		ClickEvent = ButtonBase.ClickEvent.AddOwner(typeof(Hyperlink));
		RequestSetStatusBarEvent = EventManager.RegisterRoutedEvent("RequestSetStatusBar", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Hyperlink));
		IsHyperlinkPressedProperty = DependencyProperty.Register("IsHyperlinkPressed", typeof(bool), typeof(Hyperlink), new FrameworkPropertyMetadata(false));
		FrameworkContentElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Hyperlink), new FrameworkPropertyMetadata(typeof(Hyperlink)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(Hyperlink));
		ContentElement.FocusableProperty.OverrideMetadata(typeof(Hyperlink), new FrameworkPropertyMetadata(true));
		EventManager.RegisterClassHandler(typeof(Hyperlink), Mouse.QueryCursorEvent, new QueryCursorEventHandler(OnQueryCursor));
	}

	/// <summary>Initializes a new, default instance of the <see cref="T:System.Windows.Documents.Hyperlink" /> class.</summary>
	public Hyperlink()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Hyperlink" /> class, taking a specified <see cref="T:System.Windows.Documents.Inline" /> object as the initial contents of the new <see cref="T:System.Windows.Documents.Hyperlink" />.</summary>
	/// <param name="childInline">An <see cref="T:System.Windows.Documents.Inline" /> object specifying the initial contents of the new <see cref="T:System.Windows.Documents.Hyperlink" />.</param>
	public Hyperlink(Inline childInline)
		: base(childInline)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Hyperlink" /> class, taking a specified <see cref="T:System.Windows.Documents.Inline" /> object as the initial contents of the new <see cref="T:System.Windows.Documents.Hyperlink" />, and a <see cref="T:System.Windows.Documents.TextPointer" /> specifying an insertion position for the new <see cref="T:System.Windows.Documents.Hyperlink" />.</summary>
	/// <param name="childInline">An <see cref="T:System.Windows.Documents.Inline" /> object specifying the initial contents of the new <see cref="T:System.Windows.Documents.Hyperlink" />.  This parameter may be null, in which case no <see cref="T:System.Windows.Documents.Inline" /> is inserted.</param>
	/// <param name="insertionPosition">A <see cref="T:System.Windows.Documents.TextPointer" /> specifying an insertion position at which to insert the <see cref="T:System.Windows.Documents.Hyperlink" /> element after it is created, or null for no automatic insertion.</param>
	public Hyperlink(Inline childInline, TextPointer insertionPosition)
		: base(childInline, insertionPosition)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Hyperlink" /> class, taking two <see cref="T:System.Windows.Documents.TextPointer" /> objects that indicate the beginning and end of a selection of content to be contained by the new <see cref="T:System.Windows.Documents.Hyperlink" />.</summary>
	/// <param name="start">A <see cref="T:System.Windows.Documents.TextPointer" /> indicating the beginning of a selection of content to be contained by the new <see cref="T:System.Windows.Documents.Hyperlink" />.</param>
	/// <param name="end">A <see cref="T:System.Windows.Documents.TextPointer" /> indicating the end of a selection of content to be contained by the new <see cref="T:System.Windows.Documents.Hyperlink" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="start" /> or <paramref name="end" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="start" /> and <paramref name="end" /> do not resolve to a range of content suitable for enclosure by a <see cref="T:System.Windows.Documents.Span" /> element; for example, if <paramref name="start" /> and <paramref name="end" /> indicate positions in different paragraphs.</exception>
	public Hyperlink(TextPointer start, TextPointer end)
		: base(start, end)
	{
		TextPointer textPointer = base.ContentStart.CreatePointer();
		TextPointer contentEnd = base.ContentEnd;
		while (textPointer.CompareTo(contentEnd) < 0)
		{
			if (textPointer.GetAdjacentElement(LogicalDirection.Forward) is Hyperlink hyperlink)
			{
				hyperlink.Reposition(null, null);
			}
			else
			{
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
			}
		}
	}

	/// <summary>Simulates the act of a user clicking the <see cref="T:System.Windows.Documents.Hyperlink" />.</summary>
	public void DoClick()
	{
		DoNonUserInitiatedNavigation(this);
	}

	private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Hyperlink)d).OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue);
	}

	private void OnCommandChanged(ICommand oldCommand, ICommand newCommand)
	{
		if (oldCommand != null)
		{
			UnhookCommand(oldCommand);
		}
		if (newCommand != null)
		{
			HookCommand(newCommand);
		}
	}

	private void UnhookCommand(ICommand command)
	{
		CanExecuteChangedEventManager.RemoveHandler(command, OnCanExecuteChanged);
		UpdateCanExecute();
	}

	private void HookCommand(ICommand command)
	{
		CanExecuteChangedEventManager.AddHandler(command, OnCanExecuteChanged);
		UpdateCanExecute();
	}

	private void OnCanExecuteChanged(object sender, EventArgs e)
	{
		UpdateCanExecute();
	}

	private void UpdateCanExecute()
	{
		if (Command != null)
		{
			CanExecute = CommandHelpers.CanExecuteCommandSource(this);
		}
		else
		{
			CanExecute = true;
		}
	}

	private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Hyperlink)d).UpdateCanExecute();
	}

	internal static object CoerceNavigateUri(DependencyObject d, object value)
	{
		if (s_criticalNavigateUriProtectee.Value == d.GetHashCode() && ShouldPreventUriSpoofing)
		{
			value = DependencyProperty.UnsetValue;
		}
		return value;
	}

	/// <summary>Handles the <see cref="E:System.Windows.ContentElement.MouseLeftButtonDown" /> routed event.</summary>
	/// <param name="e">Arguments associated with the <see cref="E:System.Windows.ContentElement.MouseLeftButtonDown" /> event.</param>
	protected internal override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonDown(e);
		if (base.IsEnabled && (!IsEditable || (Keyboard.Modifiers & ModifierKeys.Control) != 0))
		{
			OnMouseLeftButtonDown(this, e);
		}
	}

	/// <summary>Handles the <see cref="E:System.Windows.ContentElement.MouseLeftButtonUp" /> routed event.</summary>
	/// <param name="e">Arguments associated with the <see cref="E:System.Windows.ContentElement.MouseLeftButtonUp" /> event.</param>
	protected internal override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonUp(e);
		OnMouseLeftButtonUp(this, e);
	}

	private static void CacheNavigateUri(DependencyObject d, Uri targetUri)
	{
		d.VerifyAccess();
		s_cachedNavigateUri.Value = targetUri;
	}

	private static void NavigateToUri(IInputElement sourceElement, Uri targetUri, string targetWindow)
	{
		DependencyObject dependencyObject = (DependencyObject)sourceElement;
		dependencyObject.VerifyAccess();
		Uri value = s_cachedNavigateUri.Value;
		if (value == null || value.Equals(targetUri) || !ShouldPreventUriSpoofing)
		{
			if (!(sourceElement is Hyperlink))
			{
				targetUri = FixedPage.GetLinkUri(sourceElement, targetUri);
			}
			RequestNavigateEventArgs requestNavigateEventArgs = new RequestNavigateEventArgs(targetUri, targetWindow);
			requestNavigateEventArgs.Source = sourceElement;
			sourceElement.RaiseEvent(requestNavigateEventArgs);
			if (requestNavigateEventArgs.Handled)
			{
				dependencyObject.Dispatcher.BeginInvoke(DispatcherPriority.Send, new SendOrPostCallback(ClearStatusBarAndCachedUri), sourceElement);
			}
		}
	}

	private static void UpdateStatusBar(object sender)
	{
		IInputElement inputElement = (IInputElement)sender;
		DependencyObject dependencyObject = (DependencyObject)sender;
		Uri targetUri = (Uri)dependencyObject.GetValue(GetNavigateUriProperty(inputElement));
		s_criticalNavigateUriProtectee.Value = dependencyObject.GetHashCode();
		CacheNavigateUri(dependencyObject, targetUri);
		RequestSetStatusBarEventArgs e = new RequestSetStatusBarEventArgs(targetUri);
		inputElement.RaiseEvent(e);
	}

	private static DependencyProperty GetNavigateUriProperty(object element)
	{
		if (element is Hyperlink)
		{
			return NavigateUriProperty;
		}
		return FixedPage.NavigateUriProperty;
	}

	private static void ClearStatusBarAndCachedUri(object sender)
	{
		((IInputElement)sender).RaiseEvent(RequestSetStatusBarEventArgs.Clear);
		CacheNavigateUri((DependencyObject)sender, null);
		s_criticalNavigateUriProtectee.Value = null;
	}

	/// <summary>Handles the <see cref="E:System.Windows.Documents.Hyperlink.Click" /> routed event.</summary>
	protected virtual void OnClick()
	{
		if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
		{
			ContentElementAutomationPeer.CreatePeerForElement(this)?.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
		}
		DoNavigation(this);
		RaiseEvent(new RoutedEventArgs(ClickEvent, this));
		CommandHelpers.ExecuteCommandSource(this);
	}

	/// <summary>Handles the <see cref="E:System.Windows.ContentElement.KeyDown" /> routed event.</summary>
	/// <param name="e">Arguments associated with the <see cref="E:System.Windows.ContentElement.KeyDown" /> event.</param>
	protected internal override void OnKeyDown(KeyEventArgs e)
	{
		if (!e.Handled && e.Key == Key.Return)
		{
			OnKeyDown(this, e);
		}
		else
		{
			base.OnKeyDown(e);
		}
	}

	/// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Documents.Hyperlink" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Documents.Hyperlink" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new HyperlinkAutomationPeer(this);
	}

	private static void OnQueryCursor(object sender, QueryCursorEventArgs e)
	{
		Hyperlink hyperlink = (Hyperlink)sender;
		if (hyperlink.IsEnabled && hyperlink.IsEditable && (Keyboard.Modifiers & ModifierKeys.Control) == 0)
		{
			e.Cursor = hyperlink.TextContainer.TextSelection.TextEditor._cursor;
			e.Handled = true;
		}
	}

	internal static void OnNavigateUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is IInputElement upNavigationEventHandlers && (Uri)e.NewValue != null)
		{
			if (d is FrameworkElement frameworkElement && (frameworkElement is Path || frameworkElement is Canvas || frameworkElement is Glyphs || frameworkElement is FixedPage))
			{
				SetUpNavigationEventHandlers(upNavigationEventHandlers);
				frameworkElement.Cursor = Cursors.Hand;
			}
			else if (d is FrameworkContentElement frameworkContentElement && frameworkContentElement is Hyperlink)
			{
				SetUpNavigationEventHandlers(upNavigationEventHandlers);
			}
		}
	}

	private static void SetUpNavigationEventHandlers(IInputElement element)
	{
		if (!(element is Hyperlink))
		{
			SetUpEventHandler(element, UIElement.KeyDownEvent, new KeyEventHandler(OnKeyDown));
			SetUpEventHandler(element, UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown));
			SetUpEventHandler(element, UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseLeftButtonUp));
		}
		SetUpEventHandler(element, UIElement.MouseEnterEvent, new MouseEventHandler(OnMouseEnter));
		SetUpEventHandler(element, UIElement.MouseLeaveEvent, new MouseEventHandler(OnMouseLeave));
	}

	private static void SetUpEventHandler(IInputElement element, RoutedEvent routedEvent, Delegate handler)
	{
		element.RemoveHandler(routedEvent, handler);
		element.AddHandler(routedEvent, handler);
	}

	private static void OnKeyDown(object sender, KeyEventArgs e)
	{
		if (!e.Handled && e.Key == Key.Return)
		{
			CacheNavigateUri((DependencyObject)sender, null);
			if (e.UserInitiated)
			{
				DoUserInitiatedNavigation(sender);
			}
			else
			{
				DoNonUserInitiatedNavigation(sender);
			}
			e.Handled = true;
		}
	}

	private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		IInputElement inputElement = (IInputElement)sender;
		DependencyObject dependencyObject = (DependencyObject)sender;
		inputElement.Focus();
		if (e.ButtonState == MouseButtonState.Pressed)
		{
			Mouse.Capture(inputElement);
			if (inputElement.IsMouseCaptured)
			{
				if (e.ButtonState == MouseButtonState.Pressed)
				{
					dependencyObject.SetValue(IsHyperlinkPressedProperty, value: true);
				}
				else
				{
					inputElement.ReleaseMouseCapture();
				}
			}
		}
		e.Handled = true;
	}

	private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		IInputElement inputElement = (IInputElement)sender;
		DependencyObject dependencyObject = (DependencyObject)sender;
		if (inputElement.IsMouseCaptured)
		{
			inputElement.ReleaseMouseCapture();
		}
		if ((bool)dependencyObject.GetValue(IsHyperlinkPressedProperty))
		{
			dependencyObject.SetValue(IsHyperlinkPressedProperty, value: false);
			if (inputElement.IsMouseOver)
			{
				if (e.UserInitiated)
				{
					DoUserInitiatedNavigation(sender);
				}
				else
				{
					DoNonUserInitiatedNavigation(sender);
				}
			}
		}
		e.Handled = true;
	}

	private static void OnMouseEnter(object sender, MouseEventArgs e)
	{
		UpdateStatusBar(sender);
	}

	private static void OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (!((IInputElement)sender).IsMouseOver)
		{
			ClearStatusBarAndCachedUri(sender);
		}
	}

	private static void DoUserInitiatedNavigation(object sender)
	{
		DispatchNavigation(sender);
	}

	private static void DoNonUserInitiatedNavigation(object sender)
	{
		CacheNavigateUri((DependencyObject)sender, null);
		DispatchNavigation(sender);
	}

	private static void DispatchNavigation(object sender)
	{
		if (sender is Hyperlink hyperlink)
		{
			hyperlink.OnClick();
		}
		else
		{
			DoNavigation(sender);
		}
	}

	private static void DoNavigation(object sender)
	{
		IInputElement element = (IInputElement)sender;
		DependencyObject obj = (DependencyObject)sender;
		Uri targetUri = (Uri)obj.GetValue(GetNavigateUriProperty(element));
		string targetWindow = (string)obj.GetValue(TargetNameProperty);
		RaiseNavigate(element, targetUri, targetWindow);
	}

	internal static void RaiseNavigate(IInputElement element, Uri targetUri, string targetWindow)
	{
		if (targetUri != null)
		{
			NavigateToUri(element, targetUri, targetWindow);
		}
	}
}
