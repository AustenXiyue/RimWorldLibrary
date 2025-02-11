using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary> Represents a Windows menu control that enables you to hierarchically organize elements associated with commands and event handlers. </summary>
public class Menu : MenuBase
{
	/// <summary> Identifies the <see cref="P:System.Windows.Controls.Menu.IsMainMenu" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Menu.IsMainMenu" /> dependency property.</returns>
	public static readonly DependencyProperty IsMainMenuProperty;

	private KeyboardNavigation.EnterMenuModeEventHandler _enterMenuModeHandler;

	private static DependencyObjectType _dType;

	/// <summary> Gets or sets a value that indicates whether this <see cref="T:System.Windows.Controls.Menu" /> receives a main menu activation notification.  </summary>
	/// <returns>true if the menu receives a main menu activation notification; otherwise, false. The default is true.</returns>
	public bool IsMainMenu
	{
		get
		{
			return (bool)GetValue(IsMainMenuProperty);
		}
		set
		{
			SetValue(IsMainMenuProperty, BooleanBoxes.Box(value));
		}
	}

	internal override int EffectiveValuesInitialSize => 28;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Controls.Menu" /> class. </summary>
	public Menu()
	{
	}

	static Menu()
	{
		IsMainMenuProperty = DependencyProperty.Register("IsMainMenu", typeof(bool), typeof(Menu), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, OnIsMainMenuChanged));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Menu), new FrameworkPropertyMetadata(typeof(Menu)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(Menu));
		ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(Menu), new FrameworkPropertyMetadata(GetDefaultPanel()));
		Control.IsTabStopProperty.OverrideMetadata(typeof(Menu), new FrameworkPropertyMetadata(false));
		KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(Menu), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(Menu), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
		EventManager.RegisterClassHandler(typeof(Menu), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(OnAccessKeyPressed));
		ControlsTraceLogger.AddControl(TelemetryControls.Menu);
	}

	private static ItemsPanelTemplate GetDefaultPanel()
	{
		ItemsPanelTemplate itemsPanelTemplate = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(WrapPanel)));
		itemsPanelTemplate.Seal();
		return itemsPanelTemplate;
	}

	private static void OnIsMainMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Menu menu = d as Menu;
		if ((bool)e.NewValue)
		{
			menu.SetupMainMenu();
		}
		else
		{
			menu.CleanupMainMenu();
		}
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.MenuAutomationPeer" /> implementation for this control, as part of the WPF automation infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new MenuAutomationPeer(this);
	}

	/// <summary> Called when the <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> property is set to true. </summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
	protected override void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);
		if (IsMainMenu)
		{
			SetupMainMenu();
		}
	}

	private void SetupMainMenu()
	{
		if (_enterMenuModeHandler == null)
		{
			_enterMenuModeHandler = OnEnterMenuMode;
			KeyboardNavigation.Current.EnterMenuMode += _enterMenuModeHandler;
		}
	}

	private void CleanupMainMenu()
	{
		if (_enterMenuModeHandler != null)
		{
			KeyboardNavigation.Current.EnterMenuMode -= _enterMenuModeHandler;
		}
	}

	private static object OnGetIsMainMenu(DependencyObject d)
	{
		return BooleanBoxes.Box(((Menu)d).IsMainMenu);
	}

	/// <summary> Prepares the specified element to display the specified item. </summary>
	/// <param name="element">The element used to display the specified item.</param>
	/// <param name="item">The item to display.</param>
	protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		base.PrepareContainerForItemOverride(element, item);
		MenuItem.PrepareMenuItem(element, item);
	}

	/// <summary>Responds to the <see cref="E:System.Windows.ContentElement.KeyDown" /> event. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.KeyEventArgs" /> that contains the event data. </param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.Handled)
		{
			return;
		}
		switch (e.Key)
		{
		case Key.Up:
		case Key.Down:
			if (base.CurrentSelection != null)
			{
				Panel itemsHost2 = base.ItemsHost;
				if (itemsHost2 == null || !itemsHost2.HasLogicalOrientation || itemsHost2.LogicalOrientation != Orientation.Vertical)
				{
					base.CurrentSelection.OpenSubmenuWithKeyboard();
					e.Handled = true;
				}
			}
			break;
		case Key.Left:
		case Key.Right:
			if (base.CurrentSelection != null)
			{
				Panel itemsHost = base.ItemsHost;
				if (itemsHost != null && itemsHost.HasLogicalOrientation && itemsHost.LogicalOrientation == Orientation.Vertical)
				{
					base.CurrentSelection.OpenSubmenuWithKeyboard();
					e.Handled = true;
				}
			}
			break;
		}
	}

	/// <summary>Handles the <see cref="E:System.Windows.UIElement.TextInput" /> routed event that occurs when the menu receives text input from any device.</summary>
	/// <param name="e">The event data. </param>
	protected override void OnTextInput(TextCompositionEventArgs e)
	{
		base.OnTextInput(e);
		if (!e.Handled && e.UserInitiated && e.Text == " " && IsMainMenu && (base.CurrentSelection == null || !base.CurrentSelection.IsSubmenuOpen))
		{
			base.IsMenuMode = false;
			if (PresentationSource.CriticalFromVisual(this) is HwndSource hwndSource)
			{
				hwndSource.ShowSystemMenu();
				e.Handled = true;
			}
		}
	}

	/// <summary> Called when any mouse button is pressed or released. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. </param>
	protected override void HandleMouseButton(MouseButtonEventArgs e)
	{
		base.HandleMouseButton(e);
		if (!e.Handled && (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right) && base.IsMenuMode && e.OriginalSource is FrameworkElement frameworkElement && (frameworkElement == this || frameworkElement.TemplatedParent == this))
		{
			base.IsMenuMode = false;
			e.Handled = true;
		}
	}

	internal override bool FocusItem(ItemInfo info, ItemNavigateArgs itemNavigateArgs)
	{
		bool result = base.FocusItem(info, itemNavigateArgs);
		if (itemNavigateArgs.DeviceUsed is KeyboardDevice && info.Container is MenuItem { Role: MenuItemRole.TopLevelHeader, IsSubmenuOpen: not false } menuItem)
		{
			menuItem.NavigateToStart(itemNavigateArgs);
		}
		return result;
	}

	private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
	{
		if (!Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt))
		{
			e.Scope = sender;
			e.Handled = true;
		}
	}

	private bool OnEnterMenuMode(object sender, EventArgs e)
	{
		if (Mouse.Captured != null)
		{
			return false;
		}
		PresentationSource obj = sender as PresentationSource;
		PresentationSource presentationSource = null;
		presentationSource = PresentationSource.CriticalFromVisual(this);
		if (obj == presentationSource)
		{
			for (int i = 0; i < base.Items.Count; i++)
			{
				if (base.ItemContainerGenerator.ContainerFromIndex(i) is MenuItem menuItem && !(base.Items[i] is Separator) && menuItem.Focus())
				{
					return true;
				}
			}
		}
		return false;
	}
}
