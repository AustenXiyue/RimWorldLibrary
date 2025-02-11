using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Represents a pop-up menu that enables a control to expose functionality that is specific to the context of the control. </summary>
[DefaultEvent("Opened")]
public class ContextMenu : MenuBase
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContextMenu.HorizontalOffset" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenu.HorizontalOffset" /> dependency property.</returns>
	public static readonly DependencyProperty HorizontalOffsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContextMenu.VerticalOffset" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenu.VerticalOffset" /> dependency property.</returns>
	public static readonly DependencyProperty VerticalOffsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContextMenu.IsOpen" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenu.IsOpen" /> dependency property.</returns>
	public static readonly DependencyProperty IsOpenProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContextMenu.PlacementTarget" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenu.PlacementTarget" /> dependency property.</returns>
	public static readonly DependencyProperty PlacementTargetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContextMenu.PlacementRectangle" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenu.PlacementRectangle" /> dependency property.</returns>
	public static readonly DependencyProperty PlacementRectangleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContextMenu.Placement" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenu.Placement" /> dependency property.</returns>
	public static readonly DependencyProperty PlacementProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContextMenu.HasDropShadow" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenu.HasDropShadow" /> dependency property.</returns>
	public static readonly DependencyProperty HasDropShadowProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContextMenu.CustomPopupPlacementCallback" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenu.CustomPopupPlacementCallback" /> dependency property.</returns>
	public static readonly DependencyProperty CustomPopupPlacementCallbackProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContextMenu.StaysOpen" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenu.StaysOpen" /> dependency property.</returns>
	public static readonly DependencyProperty StaysOpenProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.ContextMenu.Opened" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.ContextMenu.Opened" /> routed event.</returns>
	public static readonly RoutedEvent OpenedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.ContextMenu.Closed" /> routed event. </summary>
	/// <returns>The identifier for the  <see cref="E:System.Windows.Controls.ContextMenu.Closed" /> routed event.</returns>
	public static readonly RoutedEvent ClosedEvent;

	private static readonly DependencyProperty InsideContextMenuProperty;

	private Popup _parentPopup;

	private WeakReference<IInputElement> _weakRefToPreviousFocus;

	private static DependencyObjectType _dType;

	/// <summary>Get or sets the horizontal distance between the target origin and the popup alignment point. </summary>
	/// <returns>The horizontal distance between the target origin and the popup alignment point. For information about the target origin and popup alignment point, see Popup Placement Behavior. The default is 0.</returns>
	[TypeConverter(typeof(LengthConverter))]
	[Bindable(true)]
	[Category("Layout")]
	public double HorizontalOffset
	{
		get
		{
			return (double)GetValue(HorizontalOffsetProperty);
		}
		set
		{
			SetValue(HorizontalOffsetProperty, value);
		}
	}

	/// <summary>Get or sets the vertical distance between the target origin and the popup alignment point. </summary>
	/// <returns>The vertical distance between the target origin and the popup alignment point. For information about the target origin and popup alignment point, see Popup Placement Behavior. The default is 0.</returns>
	[TypeConverter(typeof(LengthConverter))]
	[Bindable(true)]
	[Category("Layout")]
	public double VerticalOffset
	{
		get
		{
			return (double)GetValue(VerticalOffsetProperty);
		}
		set
		{
			SetValue(VerticalOffsetProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.ContextMenu" /> is visible.  </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.ContextMenu" /> is visible; otherwise, false. The default is false.</returns>
	[Bindable(true)]
	[Browsable(false)]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsOpen
	{
		get
		{
			return (bool)GetValue(IsOpenProperty);
		}
		set
		{
			SetValue(IsOpenProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.UIElement" /> relative to which the <see cref="T:System.Windows.Controls.ContextMenu" /> is positioned when it opens.  </summary>
	/// <returns>The element relative to which the <see cref="T:System.Windows.Controls.ContextMenu" /> is positioned when it opens. The default is null.</returns>
	[Bindable(true)]
	[Category("Layout")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public UIElement PlacementTarget
	{
		get
		{
			return (UIElement)GetValue(PlacementTargetProperty);
		}
		set
		{
			SetValue(PlacementTargetProperty, value);
		}
	}

	/// <summary>Gets or sets the area relative to which the context menu is positioned when it opens.  </summary>
	/// <returns>The area that defines the rectangle that is used to position the context menu. The default is <see cref="P:System.Windows.Rect.Empty" />.</returns>
	[Bindable(true)]
	[Category("Layout")]
	public Rect PlacementRectangle
	{
		get
		{
			return (Rect)GetValue(PlacementRectangleProperty);
		}
		set
		{
			SetValue(PlacementRectangleProperty, value);
		}
	}

	/// <summary> Gets or sets the <see cref="P:System.Windows.Controls.ContextMenu.Placement" /> property of a <see cref="T:System.Windows.Controls.ContextMenu" />.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.Controls.Primitives.PlacementMode" /> enumeration. The default is <see cref="F:System.Windows.Controls.Primitives.PlacementMode.MousePoint" />.</returns>
	[Bindable(true)]
	[Category("Layout")]
	public PlacementMode Placement
	{
		get
		{
			return (PlacementMode)GetValue(PlacementProperty);
		}
		set
		{
			SetValue(PlacementProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the context menu appears with a dropped shadow.  </summary>
	/// <returns>true if the context menu appears with a dropped shadow; otherwise, false. The default is false. </returns>
	public bool HasDropShadow
	{
		get
		{
			return (bool)GetValue(HasDropShadowProperty);
		}
		set
		{
			SetValue(HasDropShadowProperty, value);
		}
	}

	/// <summary> Gets or sets a callback that indicates where a <see cref="T:System.Windows.Controls.ContextMenu" /> should be placed on the screen.  </summary>
	/// <returns>A callback that specifies the location of the <see cref="T:System.Windows.Controls.ContextMenu" />.</returns>
	[Bindable(false)]
	[Category("Layout")]
	public CustomPopupPlacementCallback CustomPopupPlacementCallback
	{
		get
		{
			return (CustomPopupPlacementCallback)GetValue(CustomPopupPlacementCallbackProperty);
		}
		set
		{
			SetValue(CustomPopupPlacementCallbackProperty, value);
		}
	}

	/// <summary> Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.ContextMenu" /> should close automatically.  </summary>
	/// <returns>true if the menu should stay open until the <see cref="P:System.Windows.Controls.ContextMenu.IsOpen" /> property changes to false; otherwise, false. The default is false.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public bool StaysOpen
	{
		get
		{
			return (bool)GetValue(StaysOpenProperty);
		}
		set
		{
			SetValue(StaysOpenProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether the control supports scrolling.</summary>
	/// <returns>Always true.</returns>
	protected internal override bool HandlesScrolling => true;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Occurs when a particular instance of a context menu opens. </summary>
	public event RoutedEventHandler Opened
	{
		add
		{
			AddHandler(OpenedEvent, value);
		}
		remove
		{
			RemoveHandler(OpenedEvent, value);
		}
	}

	/// <summary>Occurs when a particular instance of a <see cref="T:System.Windows.Controls.ContextMenu" /> closes. </summary>
	public event RoutedEventHandler Closed
	{
		add
		{
			AddHandler(ClosedEvent, value);
		}
		remove
		{
			RemoveHandler(ClosedEvent, value);
		}
	}

	static ContextMenu()
	{
		HorizontalOffsetProperty = ContextMenuService.HorizontalOffsetProperty.AddOwner(typeof(ContextMenu), new FrameworkPropertyMetadata(null, CoerceHorizontalOffset));
		VerticalOffsetProperty = ContextMenuService.VerticalOffsetProperty.AddOwner(typeof(ContextMenu), new FrameworkPropertyMetadata(null, CoerceVerticalOffset));
		IsOpenProperty = Popup.IsOpenProperty.AddOwner(typeof(ContextMenu), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));
		PlacementTargetProperty = ContextMenuService.PlacementTargetProperty.AddOwner(typeof(ContextMenu), new FrameworkPropertyMetadata(null, CoercePlacementTarget));
		PlacementRectangleProperty = ContextMenuService.PlacementRectangleProperty.AddOwner(typeof(ContextMenu), new FrameworkPropertyMetadata(null, CoercePlacementRectangle));
		PlacementProperty = ContextMenuService.PlacementProperty.AddOwner(typeof(ContextMenu), new FrameworkPropertyMetadata(null, CoercePlacement));
		HasDropShadowProperty = ContextMenuService.HasDropShadowProperty.AddOwner(typeof(ContextMenu), new FrameworkPropertyMetadata(null, CoerceHasDropShadow));
		CustomPopupPlacementCallbackProperty = Popup.CustomPopupPlacementCallbackProperty.AddOwner(typeof(ContextMenu));
		StaysOpenProperty = Popup.StaysOpenProperty.AddOwner(typeof(ContextMenu));
		OpenedEvent = PopupControlService.ContextMenuOpenedEvent.AddOwner(typeof(ContextMenu));
		ClosedEvent = PopupControlService.ContextMenuClosedEvent.AddOwner(typeof(ContextMenu));
		InsideContextMenuProperty = MenuItem.InsideContextMenuProperty.AddOwner(typeof(ContextMenu), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));
		EventManager.RegisterClassHandler(typeof(ContextMenu), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(OnAccessKeyPressed));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ContextMenu), new FrameworkPropertyMetadata(typeof(ContextMenu)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(ContextMenu));
		Control.IsTabStopProperty.OverrideMetadata(typeof(ContextMenu), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(ContextMenu), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
		KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(ContextMenu), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(ContextMenu), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
		FrameworkElement.FocusVisualStyleProperty.OverrideMetadata(typeof(ContextMenu), new FrameworkPropertyMetadata((object)null));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ContextMenu" /> class.</summary>
	public ContextMenu()
	{
		Initialize();
	}

	private static object CoerceHorizontalOffset(DependencyObject d, object value)
	{
		return PopupControlService.CoerceProperty(d, value, ContextMenuService.HorizontalOffsetProperty);
	}

	private static object CoerceVerticalOffset(DependencyObject d, object value)
	{
		return PopupControlService.CoerceProperty(d, value, ContextMenuService.VerticalOffsetProperty);
	}

	private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ContextMenu contextMenu = (ContextMenu)d;
		if ((bool)e.NewValue)
		{
			if (contextMenu._parentPopup == null)
			{
				contextMenu.HookupParentPopup();
			}
			contextMenu._parentPopup.Unloaded += contextMenu.OnPopupUnloaded;
			contextMenu.SetValue(KeyboardNavigation.ShowKeyboardCuesProperty, KeyboardNavigation.IsKeyboardMostRecentInputDevice());
		}
		else
		{
			contextMenu.ClosingMenu();
		}
	}

	private static object CoercePlacementTarget(DependencyObject d, object value)
	{
		return PopupControlService.CoerceProperty(d, value, ContextMenuService.PlacementTargetProperty);
	}

	private static object CoercePlacementRectangle(DependencyObject d, object value)
	{
		return PopupControlService.CoerceProperty(d, value, ContextMenuService.PlacementRectangleProperty);
	}

	private static object CoercePlacement(DependencyObject d, object value)
	{
		return PopupControlService.CoerceProperty(d, value, ContextMenuService.PlacementProperty);
	}

	private static object CoerceHasDropShadow(DependencyObject d, object value)
	{
		ContextMenu contextMenu = (ContextMenu)d;
		if (contextMenu._parentPopup == null || !contextMenu._parentPopup.AllowsTransparency || !SystemParameters.DropShadow)
		{
			return BooleanBoxes.FalseBox;
		}
		return PopupControlService.CoerceProperty(d, value, ContextMenuService.HasDropShadowProperty);
	}

	/// <summary>Called when the <see cref="E:System.Windows.Controls.ContextMenu.Opened" /> event occurs. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Controls.ContextMenu.Opened" /> event.</param>
	protected virtual void OnOpened(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Called when the <see cref="E:System.Windows.Controls.ContextMenu.Closed" /> event occurs. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Controls.ContextMenu.Closed" /> event.</param>
	protected virtual void OnClosed(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Creates and returns a <see cref="T:System.Windows.Automation.Peers.ContextMenuAutomationPeer" /> object for this <see cref="T:System.Windows.Controls.ContextMenu" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.ContextMenuAutomationPeer" /> object for this <see cref="T:System.Windows.Controls.ContextMenu" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new ContextMenuAutomationPeer(this);
	}

	/// <summary> Prepares the specified element to display the specified item. </summary>
	/// <param name="element">Element used to display the specified item.</param>
	/// <param name="item">Specified item.</param>
	protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		base.PrepareContainerForItemOverride(element, item);
		MenuItem.PrepareMenuItem(element, item);
	}

	/// <summary>Called when a <see cref="E:System.Windows.ContentElement.KeyDown" /> event is raised by an object inside the <see cref="T:System.Windows.Controls.ContextMenu" />. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.KeyDown" /> event.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.Handled || !IsOpen)
		{
			return;
		}
		switch (e.Key)
		{
		case Key.Down:
			if (base.CurrentSelection == null)
			{
				NavigateToStart(new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
				e.Handled = true;
			}
			break;
		case Key.Up:
			if (base.CurrentSelection == null)
			{
				NavigateToEnd(new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
				e.Handled = true;
			}
			break;
		}
	}

	/// <summary>Responds to the <see cref="E:System.Windows.ContentElement.KeyUp" /> event.</summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.KeyUp" /> event.</param>
	protected override void OnKeyUp(KeyEventArgs e)
	{
		base.OnKeyUp(e);
		if (!e.Handled && IsOpen && e.Key == Key.Apps)
		{
			KeyboardLeaveMenuMode();
			e.Handled = true;
		}
	}

	private void Initialize()
	{
		MenuItem.SetInsideContextMenuProperty(this, value: true);
		base.InternalMenuModeChanged += OnIsMenuModeChanged;
	}

	private void HookupParentPopup()
	{
		_parentPopup = new Popup();
		_parentPopup.AllowsTransparency = true;
		CoerceValue(HasDropShadowProperty);
		_parentPopup.DropOpposite = false;
		_parentPopup.Opened += OnPopupOpened;
		_parentPopup.Closed += OnPopupClosed;
		_parentPopup.PopupCouldClose += OnPopupCouldClose;
		_parentPopup.SetResourceReference(Popup.PopupAnimationProperty, SystemParameters.MenuPopupAnimationKey);
		Popup.CreateRootPopup(_parentPopup, this);
	}

	private void OnPopupCouldClose(object sender, EventArgs e)
	{
		SetCurrentValueInternal(IsOpenProperty, BooleanBoxes.FalseBox);
	}

	private void OnPopupOpened(object source, EventArgs e)
	{
		if (base.CurrentSelection != null)
		{
			base.CurrentSelection = null;
		}
		base.IsMenuMode = true;
		if (Mouse.LeftButton == MouseButtonState.Pressed)
		{
			base.IgnoreNextLeftRelease = true;
		}
		if (Mouse.RightButton == MouseButtonState.Pressed)
		{
			base.IgnoreNextRightRelease = true;
		}
		OnOpened(new RoutedEventArgs(OpenedEvent, this));
	}

	private void OnPopupClosed(object source, EventArgs e)
	{
		base.IgnoreNextLeftRelease = false;
		base.IgnoreNextRightRelease = false;
		base.IsMenuMode = false;
		OnClosed(new RoutedEventArgs(ClosedEvent, this));
	}

	private void ClosingMenu()
	{
		if (_parentPopup == null)
		{
			return;
		}
		_parentPopup.Unloaded -= OnPopupUnloaded;
		base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate(object arg)
		{
			ContextMenu contextMenu = (ContextMenu)arg;
			if (!contextMenu.IsOpen)
			{
				FocusManager.SetFocusedElement(contextMenu, null);
			}
			return (object)null;
		}, this);
	}

	private void OnPopupUnloaded(object sender, RoutedEventArgs e)
	{
		if (!IsOpen)
		{
			return;
		}
		base.Dispatcher.BeginInvoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate(object arg)
		{
			ContextMenu contextMenu = (ContextMenu)arg;
			if (contextMenu.IsOpen)
			{
				contextMenu.SetCurrentValueInternal(IsOpenProperty, BooleanBoxes.FalseBox);
			}
			return (object)null;
		}, this);
	}

	private void OnIsMenuModeChanged(object sender, EventArgs e)
	{
		if (base.IsMenuMode)
		{
			if (Keyboard.FocusedElement != null)
			{
				_weakRefToPreviousFocus = new WeakReference<IInputElement>(Keyboard.FocusedElement);
			}
			Focus();
			return;
		}
		SetCurrentValueInternal(IsOpenProperty, BooleanBoxes.FalseBox);
		if (_weakRefToPreviousFocus != null)
		{
			if (_weakRefToPreviousFocus.TryGetTarget(out var target))
			{
				target.Focus();
			}
			_weakRefToPreviousFocus = null;
		}
	}

	/// <summary>Reports that the <see cref="P:System.Windows.UIElement.IsKeyboardFocusWithin" /> property changed.</summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.IsKeyboardFocusWithinChanged" /> event.</param>
	protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
	{
		if (!(bool)e.NewValue)
		{
			_weakRefToPreviousFocus = null;
		}
		base.OnIsKeyboardFocusWithinChanged(e);
	}

	internal override bool IgnoreModelParentBuildRoute(RoutedEventArgs e)
	{
		if (!(e is KeyEventArgs))
		{
			return e is FindToolTipEventArgs;
		}
		return true;
	}

	private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
	{
		e.Scope = sender;
		e.Handled = true;
	}

	/// <summary>Called when a context menu's visual parent changes. </summary>
	/// <param name="oldParent">The object that the context menu was previously attached to.</param>
	protected internal override void OnVisualParentChanged(DependencyObject oldParent)
	{
		base.OnVisualParentChanged(oldParent);
		if (!Popup.IsRootedInPopup(_parentPopup, this))
		{
			throw new InvalidOperationException(SR.Format(SR.ElementMustBeInPopup, "ContextMenu"));
		}
	}

	internal override void OnAncestorChanged()
	{
		base.OnAncestorChanged();
		if (!Popup.IsRootedInPopup(_parentPopup, this))
		{
			throw new InvalidOperationException(SR.Format(SR.ElementMustBeInPopup, "ContextMenu"));
		}
	}
}
