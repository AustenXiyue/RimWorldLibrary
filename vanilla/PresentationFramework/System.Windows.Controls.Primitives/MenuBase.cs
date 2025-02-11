using System.Collections.Specialized;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal.KnownBoxes;
using MS.Win32;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents a control that defines choices for users to select. </summary>
[Localizability(LocalizationCategory.Menu)]
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(MenuItem))]
public abstract class MenuBase : ItemsControl
{
	private enum MenuBaseFlags
	{
		IgnoreNextLeftRelease = 1,
		IgnoreNextRightRelease = 2,
		IsMenuMode = 4,
		OpenOnMouseEnter = 8,
		IsAcquireFocusMenuMode = 0x10
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.MenuBase.ItemContainerTemplateSelector" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.MenuBase.ItemContainerTemplateSelector" /> dependency property.</returns>
	public static readonly DependencyProperty ItemContainerTemplateSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.MenuBase.UsesItemContainerTemplate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.MenuBase.UsesItemContainerTemplate" /> dependency property.</returns>
	public static readonly DependencyProperty UsesItemContainerTemplateProperty;

	internal static readonly RoutedEvent IsSelectedChangedEvent;

	private object _currentItem;

	private static readonly EventPrivateKey InternalMenuModeChangedKey;

	private PresentationSource _pushedMenuMode;

	private MenuItem _currentSelection;

	private BitVector32 _bitFlags = new BitVector32(0);

	/// <summary>Gets or sets the custom logic for choosing a template used to display each item. </summary>
	/// <returns>A custom object that provides logic and returns an item container. </returns>
	public ItemContainerTemplateSelector ItemContainerTemplateSelector
	{
		get
		{
			return (ItemContainerTemplateSelector)GetValue(ItemContainerTemplateSelectorProperty);
		}
		set
		{
			SetValue(ItemContainerTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the menu selects different item containers, depending on the type of the item in the underlying collection or some other heuristic.</summary>
	/// <returns>true the menu selects different item containers; otherwise, false.The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public bool UsesItemContainerTemplate
	{
		get
		{
			return (bool)GetValue(UsesItemContainerTemplateProperty);
		}
		set
		{
			SetValue(UsesItemContainerTemplateProperty, value);
		}
	}

	internal MenuItem CurrentSelection
	{
		get
		{
			return _currentSelection;
		}
		set
		{
			bool flag = false;
			if (_currentSelection != null)
			{
				flag = _currentSelection.IsKeyboardFocused;
				_currentSelection.SetCurrentValueInternal(MenuItem.IsSelectedProperty, BooleanBoxes.FalseBox);
			}
			_currentSelection = value;
			if (_currentSelection != null)
			{
				_currentSelection.SetCurrentValueInternal(MenuItem.IsSelectedProperty, BooleanBoxes.TrueBox);
				if (flag)
				{
					_currentSelection.Focus();
				}
			}
		}
	}

	internal bool HasCapture => Mouse.Captured == this;

	internal bool IgnoreNextLeftRelease
	{
		get
		{
			return _bitFlags[1];
		}
		set
		{
			_bitFlags[1] = value;
		}
	}

	internal bool IgnoreNextRightRelease
	{
		get
		{
			return _bitFlags[2];
		}
		set
		{
			_bitFlags[2] = value;
		}
	}

	internal bool IsMenuMode
	{
		get
		{
			return _bitFlags[4];
		}
		set
		{
			bool flag = _bitFlags[4];
			if (flag == value)
			{
				return;
			}
			bool flag3 = (_bitFlags[4] = value);
			flag = flag3;
			if (flag)
			{
				if (!IsDescendant(this, Mouse.Captured as Visual) && !Mouse.Capture(this, CaptureMode.SubTree))
				{
					flag3 = (_bitFlags[4] = false);
					flag = flag3;
				}
				else
				{
					if (!HasPushedMenuMode)
					{
						PushMenuMode(isAcquireFocusMenuMode: false);
					}
					RaiseClrEvent(InternalMenuModeChangedKey, EventArgs.Empty);
				}
			}
			if (!flag)
			{
				if (CurrentSelection != null)
				{
					_ = CurrentSelection.IsSubmenuOpen;
					CurrentSelection.IsSubmenuOpen = false;
					CurrentSelection = null;
				}
				if (HasPushedMenuMode)
				{
					PopMenuMode();
				}
				if (!value)
				{
					RaiseClrEvent(InternalMenuModeChangedKey, EventArgs.Empty);
				}
				SetSuspendingPopupAnimation(this, null, suspend: false);
				if (HasCapture)
				{
					Mouse.Capture(null);
				}
				RestorePreviousFocus();
			}
			OpenOnMouseEnter = flag;
		}
	}

	internal bool OpenOnMouseEnter
	{
		get
		{
			return _bitFlags[8];
		}
		set
		{
			_bitFlags[8] = value;
		}
	}

	private bool HasPushedMenuMode => _pushedMenuMode != null;

	private bool IsAcquireFocusMenuMode
	{
		get
		{
			return _bitFlags[16];
		}
		set
		{
			_bitFlags[16] = value;
		}
	}

	internal event EventHandler InternalMenuModeChanged
	{
		add
		{
			EventHandlersStoreAdd(InternalMenuModeChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(InternalMenuModeChangedKey, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.MenuBase" /> class. </summary>
	protected MenuBase()
	{
	}

	static MenuBase()
	{
		ItemContainerTemplateSelectorProperty = DependencyProperty.Register("ItemContainerTemplateSelector", typeof(ItemContainerTemplateSelector), typeof(MenuBase), new FrameworkPropertyMetadata(new DefaultItemContainerTemplateSelector()));
		UsesItemContainerTemplateProperty = DependencyProperty.Register("UsesItemContainerTemplate", typeof(bool), typeof(MenuBase));
		IsSelectedChangedEvent = EventManager.RegisterRoutedEvent("IsSelectedChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(MenuBase));
		InternalMenuModeChangedKey = new EventPrivateKey();
		EventManager.RegisterClassHandler(typeof(MenuBase), MenuItem.PreviewClickEvent, new RoutedEventHandler(OnMenuItemPreviewClick));
		EventManager.RegisterClassHandler(typeof(MenuBase), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseButtonDown));
		EventManager.RegisterClassHandler(typeof(MenuBase), Mouse.MouseUpEvent, new MouseButtonEventHandler(OnMouseButtonUp));
		EventManager.RegisterClassHandler(typeof(MenuBase), Mouse.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));
		EventManager.RegisterClassHandler(typeof(MenuBase), IsSelectedChangedEvent, new RoutedPropertyChangedEventHandler<bool>(OnIsSelectedChanged));
		EventManager.RegisterClassHandler(typeof(MenuBase), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnPromotedMouseButton));
		EventManager.RegisterClassHandler(typeof(MenuBase), Mouse.MouseUpEvent, new MouseButtonEventHandler(OnPromotedMouseButton));
		EventManager.RegisterClassHandler(typeof(MenuBase), Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(OnClickThroughThunk));
		EventManager.RegisterClassHandler(typeof(MenuBase), Mouse.PreviewMouseUpOutsideCapturedElementEvent, new MouseButtonEventHandler(OnClickThroughThunk));
		EventManager.RegisterClassHandler(typeof(MenuBase), Keyboard.PreviewKeyboardInputProviderAcquireFocusEvent, new KeyboardInputProviderAcquireFocusEventHandler(OnPreviewKeyboardInputProviderAcquireFocus), handledEventsToo: true);
		EventManager.RegisterClassHandler(typeof(MenuBase), Keyboard.KeyboardInputProviderAcquireFocusEvent, new KeyboardInputProviderAcquireFocusEventHandler(OnKeyboardInputProviderAcquireFocus), handledEventsToo: true);
		FocusManager.IsFocusScopeProperty.OverrideMetadata(typeof(MenuBase), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		InputMethod.IsInputMethodSuspendedProperty.OverrideMetadata(typeof(MenuBase), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));
	}

	private static void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
	{
		((MenuBase)sender).HandleMouseButton(e);
	}

	private static void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
	{
		((MenuBase)sender).HandleMouseButton(e);
	}

	/// <summary>Called when a mouse button is pressed or released. </summary>
	/// <param name="e">The event data for a mouse event.</param>
	protected virtual void HandleMouseButton(MouseButtonEventArgs e)
	{
	}

	private static void OnClickThroughThunk(object sender, MouseButtonEventArgs e)
	{
		((MenuBase)sender).OnClickThrough(e);
	}

	private void OnClickThrough(MouseButtonEventArgs e)
	{
		if ((e.ChangedButton != 0 && e.ChangedButton != MouseButton.Right) || !HasCapture)
		{
			return;
		}
		bool flag = true;
		if (e.ButtonState == MouseButtonState.Released)
		{
			if (e.ChangedButton == MouseButton.Left && IgnoreNextLeftRelease)
			{
				IgnoreNextLeftRelease = false;
				flag = false;
			}
			else if (e.ChangedButton == MouseButton.Right && IgnoreNextRightRelease)
			{
				IgnoreNextRightRelease = false;
				flag = false;
			}
		}
		if (flag)
		{
			IsMenuMode = false;
		}
	}

	private static void OnPromotedMouseButton(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			e.Handled = true;
		}
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.MouseLeave" /> routed event that occurs when the mouse leaves the control.</summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.MouseLeave" /> event.</param>
	protected override void OnMouseLeave(MouseEventArgs e)
	{
		base.OnMouseLeave(e);
		if (!HasCapture && !base.IsMouseOver && CurrentSelection != null && !CurrentSelection.IsKeyboardFocused && !CurrentSelection.IsSubmenuOpen)
		{
			CurrentSelection = null;
		}
	}

	private static void OnPreviewKeyboardInputProviderAcquireFocus(object sender, KeyboardInputProviderAcquireFocusEventArgs e)
	{
		MenuBase menuBase = (MenuBase)sender;
		if (!menuBase.IsKeyboardFocusWithin && !menuBase.HasPushedMenuMode)
		{
			menuBase.PushMenuMode(isAcquireFocusMenuMode: true);
		}
	}

	private static void OnKeyboardInputProviderAcquireFocus(object sender, KeyboardInputProviderAcquireFocusEventArgs e)
	{
		MenuBase menuBase = (MenuBase)sender;
		if (!menuBase.IsKeyboardFocusWithin && !e.FocusAcquired && menuBase.IsAcquireFocusMenuMode)
		{
			menuBase.PopMenuMode();
		}
	}

	/// <summary>Responds to a change to the <see cref="P:System.Windows.UIElement.IsKeyboardFocusWithin" /> property. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.IsKeyboardFocusWithinChanged" /> event.</param>
	protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnIsKeyboardFocusWithinChanged(e);
		if (base.IsKeyboardFocusWithin)
		{
			if (!IsMenuMode)
			{
				IsMenuMode = true;
				OpenOnMouseEnter = false;
			}
			if (KeyboardNavigation.IsKeyboardMostRecentInputDevice())
			{
				KeyboardNavigation.EnableKeyboardCues(this, enable: true);
			}
		}
		else
		{
			KeyboardNavigation.EnableKeyboardCues(this, enable: false);
			if (IsMenuMode)
			{
				if (HasCapture)
				{
					IsMenuMode = false;
				}
			}
			else if (CurrentSelection != null)
			{
				CurrentSelection = null;
			}
		}
		InvokeMenuOpenedClosedAutomationEvent(base.IsKeyboardFocusWithin);
	}

	private void InvokeMenuOpenedClosedAutomationEvent(bool open)
	{
		AutomationEvents automationEvent = (open ? AutomationEvents.MenuOpened : AutomationEvents.MenuClosed);
		if (!AutomationPeer.ListenerExists(automationEvent))
		{
			return;
		}
		AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(this);
		if (peer == null)
		{
			return;
		}
		if (open)
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Input, (DispatcherOperationCallback)delegate
			{
				peer.RaiseAutomationEvent(automationEvent);
				return (object)null;
			}, null);
		}
		else
		{
			peer.RaiseAutomationEvent(automationEvent);
		}
	}

	private static void OnIsSelectedChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
	{
		if (!(e.OriginalSource is MenuItem menuItem))
		{
			return;
		}
		MenuBase menuBase = (MenuBase)sender;
		if (e.NewValue)
		{
			if (menuBase.CurrentSelection != menuItem && menuItem.LogicalParent == menuBase)
			{
				bool flag = false;
				if (menuBase.CurrentSelection != null)
				{
					flag = menuBase.CurrentSelection.IsSubmenuOpen;
					menuBase.CurrentSelection.SetCurrentValueInternal(MenuItem.IsSubmenuOpenProperty, BooleanBoxes.FalseBox);
				}
				menuBase.CurrentSelection = menuItem;
				if (menuBase.CurrentSelection != null && flag)
				{
					MenuItemRole role = menuBase.CurrentSelection.Role;
					if ((role == MenuItemRole.SubmenuHeader || role == MenuItemRole.TopLevelHeader) && menuBase.CurrentSelection.IsSubmenuOpen != flag)
					{
						menuBase.CurrentSelection.SetCurrentValueInternal(MenuItem.IsSubmenuOpenProperty, BooleanBoxes.Box(flag));
					}
				}
			}
		}
		else if (menuBase.CurrentSelection == menuItem)
		{
			menuBase.CurrentSelection = null;
		}
		e.Handled = true;
	}

	private bool IsDescendant(DependencyObject node)
	{
		return IsDescendant(this, node);
	}

	internal static bool IsDescendant(DependencyObject reference, DependencyObject node)
	{
		bool result = false;
		DependencyObject dependencyObject = node;
		while (dependencyObject != null)
		{
			if (dependencyObject == reference)
			{
				result = true;
				break;
			}
			if (dependencyObject is PopupRoot popupRoot)
			{
				Popup popup = popupRoot.Parent as Popup;
				dependencyObject = popup;
				if (popup != null)
				{
					dependencyObject = popup.Parent;
					if (dependencyObject == null)
					{
						dependencyObject = popup.PlacementTarget;
					}
				}
			}
			else
			{
				dependencyObject = PopupControlService.FindParent(dependencyObject);
			}
		}
		return result;
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.KeyDown" /> routed event that occurs when the user presses a key.</summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.KeyDown" /> event.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		switch (e.Key)
		{
		case Key.Escape:
			if (CurrentSelection != null && CurrentSelection.IsSubmenuOpen)
			{
				CurrentSelection.SetCurrentValueInternal(MenuItem.IsSubmenuOpenProperty, BooleanBoxes.FalseBox);
				OpenOnMouseEnter = false;
				e.Handled = true;
			}
			else
			{
				KeyboardLeaveMenuMode();
				e.Handled = true;
			}
			break;
		case Key.System:
			if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt || e.SystemKey == Key.F10)
			{
				KeyboardLeaveMenuMode();
				e.Handled = true;
			}
			break;
		}
	}

	/// <summary>Determines whether the specified item is, or is eligible to be, its own item container. </summary>
	/// <returns>true if the item is a <see cref="T:System.Windows.Controls.MenuItem" /> or a <see cref="T:System.Windows.Controls.Separator" />; otherwise, false.</returns>
	/// <param name="item">The item to check whether it is an item container.</param>
	protected override bool IsItemItsOwnContainerOverride(object item)
	{
		int num;
		if (!(item is MenuItem))
		{
			num = ((item is Separator) ? 1 : 0);
			if (num == 0)
			{
				_currentItem = item;
			}
		}
		else
		{
			num = 1;
		}
		return (byte)num != 0;
	}

	/// <summary>Creates or identifies the element used to display the specified item.</summary>
	/// <returns>The element used to display the specified item.</returns>
	protected override DependencyObject GetContainerForItemOverride()
	{
		object currentItem = _currentItem;
		_currentItem = null;
		if (UsesItemContainerTemplate)
		{
			DataTemplate dataTemplate = ItemContainerTemplateSelector.SelectTemplate(currentItem, this);
			if (dataTemplate != null)
			{
				object obj = dataTemplate.LoadContent();
				if (obj is MenuItem || obj is Separator)
				{
					return obj as DependencyObject;
				}
				throw new InvalidOperationException(SR.Format(SR.InvalidItemContainer, GetType().Name, typeof(MenuItem).Name, typeof(Separator).Name, obj));
			}
		}
		return new MenuItem();
	}

	private static void OnLostMouseCapture(object sender, MouseEventArgs e)
	{
		MenuBase menuBase = sender as MenuBase;
		if (Mouse.Captured == menuBase)
		{
			return;
		}
		if (e.OriginalSource == menuBase)
		{
			if (Mouse.Captured == null || !IsDescendant(menuBase, Mouse.Captured as DependencyObject))
			{
				menuBase.IsMenuMode = false;
			}
		}
		else if (IsDescendant(menuBase, e.OriginalSource as DependencyObject))
		{
			if (menuBase.IsMenuMode && Mouse.Captured == null && SafeNativeMethods.GetCapture() == IntPtr.Zero)
			{
				Mouse.Capture(menuBase, CaptureMode.SubTree);
				e.Handled = true;
			}
		}
		else
		{
			menuBase.IsMenuMode = false;
		}
	}

	private static void OnMenuItemPreviewClick(object sender, RoutedEventArgs e)
	{
		MenuBase menuBase = (MenuBase)sender;
		if (e.OriginalSource is MenuItem { StaysOpenOnClick: false, Role: var role } && (role == MenuItemRole.TopLevelItem || role == MenuItemRole.SubmenuItem))
		{
			menuBase.IsMenuMode = false;
			e.Handled = true;
		}
	}

	private void RestorePreviousFocus()
	{
		if (base.IsKeyboardFocusWithin)
		{
			nint focus = MS.Win32.UnsafeNativeMethods.GetFocus();
			if (((focus != IntPtr.Zero) ? HwndSource.CriticalFromHwnd(focus) : null) != null)
			{
				Keyboard.Focus(null);
			}
			else
			{
				Keyboard.ClearFocus();
			}
		}
	}

	internal static void SetSuspendingPopupAnimation(ItemsControl menu, MenuItem ignore, bool suspend)
	{
		if (menu == null)
		{
			return;
		}
		int count = menu.Items.Count;
		for (int i = 0; i < count; i++)
		{
			if (menu.ItemContainerGenerator.ContainerFromIndex(i) is MenuItem menuItem && menuItem != ignore && menuItem.IsSuspendingPopupAnimation != suspend)
			{
				menuItem.IsSuspendingPopupAnimation = suspend;
				if (!suspend)
				{
					SetSuspendingPopupAnimation(menuItem, null, suspend);
				}
			}
		}
	}

	internal void KeyboardLeaveMenuMode()
	{
		if (IsMenuMode)
		{
			IsMenuMode = false;
			return;
		}
		CurrentSelection = null;
		RestorePreviousFocus();
	}

	private void PushMenuMode(bool isAcquireFocusMenuMode)
	{
		_pushedMenuMode = PresentationSource.CriticalFromVisual(this);
		IsAcquireFocusMenuMode = isAcquireFocusMenuMode;
		InputManager.UnsecureCurrent.PushMenuMode(_pushedMenuMode);
	}

	private void PopMenuMode()
	{
		PresentationSource pushedMenuMode = _pushedMenuMode;
		_pushedMenuMode = null;
		IsAcquireFocusMenuMode = false;
		InputManager.UnsecureCurrent.PopMenuMode(pushedMenuMode);
	}
}
