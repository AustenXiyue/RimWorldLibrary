using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal.Commands;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Represents a selectable item inside a <see cref="T:System.Windows.Controls.Menu" />.</summary>
[DefaultEvent("Click")]
[Localizability(LocalizationCategory.Menu)]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(MenuItem))]
public class MenuItem : HeaderedItemsControl, ICommandSource
{
	[Flags]
	private enum BoolField
	{
		OpenedWithKeyboard = 1,
		IgnoreNextMouseLeave = 2,
		IgnoreMouseEvents = 4,
		MouseEnterOnMouseMove = 8,
		CanExecuteInvalid = 0x10
	}

	private static ComponentResourceKey _topLevelItemTemplateKey;

	private static ComponentResourceKey _topLevelHeaderTemplateKey;

	private static ComponentResourceKey _submenuItemTemplateKey;

	private static ComponentResourceKey _submenuHeaderTemplateKey;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.MenuItem.Click" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.MenuItem.Click" /> routed event.</returns>
	public static readonly RoutedEvent ClickEvent;

	internal static readonly RoutedEvent PreviewClickEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.MenuItem.Checked" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.MenuItem.Checked" /> routed event.</returns>
	public static readonly RoutedEvent CheckedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.MenuItem.Unchecked" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.MenuItem.Unchecked" /> routed event.</returns>
	public static readonly RoutedEvent UncheckedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.MenuItem.SubmenuOpened" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.MenuItem.SubmenuOpened" /> routed event.</returns>
	public static readonly RoutedEvent SubmenuOpenedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.MenuItem.SubmenuClosed" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.MenuItem.SubmenuClosed" /> routed event.</returns>
	public static readonly RoutedEvent SubmenuClosedEvent;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MenuItem.Command" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MenuItem.Command" /> dependency property.</returns>
	public static readonly DependencyProperty CommandProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MenuItem.CommandParameter" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MenuItem.CommandParameter" /> dependency property.</returns>
	public static readonly DependencyProperty CommandParameterProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MenuItem.CommandTarget" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MenuItem.CommandTarget" /> dependency property.</returns>
	public static readonly DependencyProperty CommandTargetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MenuItem.IsSubmenuOpen" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MenuItem.IsSubmenuOpen" /> dependency property.</returns>
	public static readonly DependencyProperty IsSubmenuOpenProperty;

	private static readonly DependencyPropertyKey RolePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MenuItem.Role" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="T:System.Windows.Controls.MenuItemRole" /> dependency property.</returns>
	public static readonly DependencyProperty RoleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MenuItem.IsCheckable" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MenuItem.IsCheckable" /> dependency property.</returns>
	public static readonly DependencyProperty IsCheckableProperty;

	private static readonly DependencyPropertyKey IsPressedPropertyKey;

	/// <summary> Identifies the <see cref="P:System.Windows.Controls.MenuItem.IsPressed" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MenuItem.IsPressed" /> dependency property.</returns>
	public static readonly DependencyProperty IsPressedProperty;

	private static readonly DependencyPropertyKey IsHighlightedPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MenuItem.IsHighlighted" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MenuItem.IsHighlighted" /> dependency property.</returns>
	public static readonly DependencyProperty IsHighlightedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MenuItem.IsChecked" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MenuItem.IsChecked" /> dependency property.</returns>
	public static readonly DependencyProperty IsCheckedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MenuItem.StaysOpenOnClick" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MenuItem.StaysOpenOnClick" /> dependency property.</returns>
	public static readonly DependencyProperty StaysOpenOnClickProperty;

	internal static readonly DependencyProperty IsSelectedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MenuItem.InputGestureText" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MenuItem.InputGestureText" /> dependency property.</returns>
	public static readonly DependencyProperty InputGestureTextProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MenuItem.Icon" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MenuItem.Icon" /> dependency property.</returns>
	public static readonly DependencyProperty IconProperty;

	private static readonly DependencyPropertyKey IsSuspendingPopupAnimationPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MenuItem.IsSuspendingPopupAnimation" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MenuItem.IsSuspendingPopupAnimation" /> dependency property.</returns>
	public static readonly DependencyProperty IsSuspendingPopupAnimationProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MenuItem.ItemContainerTemplateSelector" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MenuItem.ItemContainerTemplateSelector" /> dependency property.</returns>
	public static readonly DependencyProperty ItemContainerTemplateSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MenuItem.UsesItemContainerTemplate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MenuItem.UsesItemContainerTemplate" /> dependency property.</returns>
	public static readonly DependencyProperty UsesItemContainerTemplateProperty;

	private object _currentItem;

	internal static readonly DependencyProperty InsideContextMenuProperty;

	private static readonly DependencyProperty BooleanFieldStoreProperty;

	private const string PopupTemplateName = "PART_Popup";

	private MenuItem _currentSelection;

	private Popup _submenuPopup;

	private DispatcherTimer _openHierarchyTimer;

	private DispatcherTimer _closeHierarchyTimer;

	private bool _userInitiatedPress;

	private static DependencyObjectType _dType;

	/// <summary>Gets the resource key for a style applied to a <see cref="T:System.Windows.Controls.MenuItem" /> when it is a top-level <see cref="T:System.Windows.Controls.MenuItem" /> that can invoke commands.</summary>
	/// <returns>The resource key for a style applied to a <see cref="T:System.Windows.Controls.MenuItem" /> when it is a top-level <see cref="T:System.Windows.Controls.MenuItem" /> that can invoke commands.</returns>
	public static ResourceKey TopLevelItemTemplateKey
	{
		get
		{
			if (_topLevelItemTemplateKey == null)
			{
				_topLevelItemTemplateKey = new ComponentResourceKey(typeof(MenuItem), "TopLevelItemTemplateKey");
			}
			return _topLevelItemTemplateKey;
		}
	}

	/// <summary>Gets the resource key for a style applied to a <see cref="T:System.Windows.Controls.MenuItem" /> when the <see cref="T:System.Windows.Controls.MenuItem" /> is a header of a top-level menu.</summary>
	/// <returns>The resource key for a style applied to a <see cref="T:System.Windows.Controls.MenuItem" /> when the <see cref="T:System.Windows.Controls.MenuItem" /> is a header of a top-level menu.</returns>
	public static ResourceKey TopLevelHeaderTemplateKey
	{
		get
		{
			if (_topLevelHeaderTemplateKey == null)
			{
				_topLevelHeaderTemplateKey = new ComponentResourceKey(typeof(MenuItem), "TopLevelHeaderTemplateKey");
			}
			return _topLevelHeaderTemplateKey;
		}
	}

	/// <summary>Gets the resource key for a style applied to a <see cref="T:System.Windows.Controls.MenuItem" /> when the <see cref="T:System.Windows.Controls.MenuItem" /> is a submenu.</summary>
	/// <returns>The resource key for a style applied to a <see cref="T:System.Windows.Controls.MenuItem" /> when the <see cref="T:System.Windows.Controls.MenuItem" /> is a submenu.</returns>
	public static ResourceKey SubmenuItemTemplateKey
	{
		get
		{
			if (_submenuItemTemplateKey == null)
			{
				_submenuItemTemplateKey = new ComponentResourceKey(typeof(MenuItem), "SubmenuItemTemplateKey");
			}
			return _submenuItemTemplateKey;
		}
	}

	/// <summary>Gets the resource key for a style applied to a <see cref="T:System.Windows.Controls.MenuItem" /> when the <see cref="T:System.Windows.Controls.MenuItem" /> is a header of a submenu.</summary>
	/// <returns>The resource key for a style applied to a <see cref="T:System.Windows.Controls.MenuItem" /> when the <see cref="T:System.Windows.Controls.MenuItem" /> is a header of a submenu.</returns>
	public static ResourceKey SubmenuHeaderTemplateKey
	{
		get
		{
			if (_submenuHeaderTemplateKey == null)
			{
				_submenuHeaderTemplateKey = new ComponentResourceKey(typeof(MenuItem), "SubmenuHeaderTemplateKey");
			}
			return _submenuHeaderTemplateKey;
		}
	}

	/// <summary>Gets or sets the command associated with the menu item.  </summary>
	/// <returns>The command associated with the <see cref="T:System.Windows.Controls.MenuItem" />.  The default is null.</returns>
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

	/// <summary>Gets a value that indicates whether the <see cref="P:System.Windows.ContentElement.IsEnabled" /> property is true for the current menu item.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.MenuItem" /> can be selected; otherwise, false.</returns>
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

	/// <summary>Gets or sets the parameter to pass to the <see cref="P:System.Windows.Controls.MenuItem.Command" /> property of a <see cref="T:System.Windows.Controls.MenuItem" />.  </summary>
	/// <returns>The parameter to pass to the <see cref="P:System.Windows.Controls.MenuItem.Command" /> property of a <see cref="T:System.Windows.Controls.MenuItem" />. The default is null.</returns>
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

	/// <summary>Gets or sets the target element on which to raise the specified command.   </summary>
	/// <returns>The element on which to raise the specified command. The default is null.</returns>
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

	/// <summary>Gets or sets a value that indicates whether the submenu of the <see cref="T:System.Windows.Controls.MenuItem" /> is open.  </summary>
	/// <returns>true if the submenu of the <see cref="T:System.Windows.Controls.MenuItem" /> is open; otherwise, false. The default is false.</returns>
	[Bindable(true)]
	[Browsable(false)]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsSubmenuOpen
	{
		get
		{
			return (bool)GetValue(IsSubmenuOpenProperty);
		}
		set
		{
			SetValue(IsSubmenuOpenProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets a value that indicates the role of a <see cref="T:System.Windows.Controls.MenuItem" />.   </summary>
	/// <returns>One of the <see cref="T:System.Windows.Controls.MenuItemRole" /> values. The default is <see cref="F:System.Windows.Controls.MenuItemRole.TopLevelItem" />.</returns>
	[Category("Behavior")]
	public MenuItemRole Role => (MenuItemRole)GetValue(RoleProperty);

	/// <summary>Gets a value that indicates whether a <see cref="T:System.Windows.Controls.MenuItem" /> can be checked.   </summary>
	/// <returns>true if the menu item can be checked; otherwise, false.  The default is false.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public bool IsCheckable
	{
		get
		{
			return (bool)GetValue(IsCheckableProperty);
		}
		set
		{
			SetValue(IsCheckableProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether a <see cref="T:System.Windows.Controls.MenuItem" /> is pressed.  </summary>
	/// <returns>true if a <see cref="T:System.Windows.Controls.MenuItem" /> is pressed; otherwise, false. The default is false.</returns>
	[Browsable(false)]
	[Category("Appearance")]
	public bool IsPressed
	{
		get
		{
			return (bool)GetValue(IsPressedProperty);
		}
		protected set
		{
			SetValue(IsPressedPropertyKey, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets a value that indicates whether a <see cref="T:System.Windows.Controls.MenuItem" /> is highlighted.  </summary>
	/// <returns>true if a <see cref="T:System.Windows.Controls.MenuItem" /> is highlighted; otherwise, false. The default is false.</returns>
	[Browsable(false)]
	[Category("Appearance")]
	public bool IsHighlighted
	{
		get
		{
			return (bool)GetValue(IsHighlightedProperty);
		}
		protected set
		{
			SetValue(IsHighlightedPropertyKey, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.MenuItem" /> is checked.  </summary>
	/// <returns>true if a <see cref="T:System.Windows.Controls.MenuItem" /> is checked; otherwise, false. The default is false.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public bool IsChecked
	{
		get
		{
			return (bool)GetValue(IsCheckedProperty);
		}
		set
		{
			SetValue(IsCheckedProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a value that indicates that the submenu in which this <see cref="T:System.Windows.Controls.MenuItem" /> is located should not close when this item is clicked.   </summary>
	/// <returns>true if the submenu in which this <see cref="T:System.Windows.Controls.MenuItem" /> is located should not close when this item is clicked; otherwise, false. The default is false.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public bool StaysOpenOnClick
	{
		get
		{
			return (bool)GetValue(StaysOpenOnClickProperty);
		}
		set
		{
			SetValue(StaysOpenOnClickProperty, BooleanBoxes.Box(value));
		}
	}

	internal bool IsSelected
	{
		get
		{
			return (bool)GetValue(IsSelectedProperty);
		}
		set
		{
			SetValue(IsSelectedProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary> Sets the text describing an input gesture that will call the command tied to the specified item.  </summary>
	/// <returns>The text that describes the input gesture, such as Ctrl+C for the Copy command. The default is an empty string ("").</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	public string InputGestureText
	{
		get
		{
			return (string)GetValue(InputGestureTextProperty);
		}
		set
		{
			SetValue(InputGestureTextProperty, value);
		}
	}

	/// <summary>Gets or sets the icon that appears in a <see cref="T:System.Windows.Controls.MenuItem" />.  </summary>
	/// <returns>The icon that appears in a <see cref="T:System.Windows.Controls.MenuItem" />. The default value is null.</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	public object Icon
	{
		get
		{
			return GetValue(IconProperty);
		}
		set
		{
			SetValue(IconProperty, value);
		}
	}

	/// <summary>Gets whether a menu suspends animations on its <see cref="T:System.Windows.Controls.Primitives.Popup" /> control.  </summary>
	/// <returns>true if the menu should suspend animations on its popup; otherwise, false. The default is false.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsSuspendingPopupAnimation
	{
		get
		{
			return (bool)GetValue(IsSuspendingPopupAnimationProperty);
		}
		internal set
		{
			SetValue(IsSuspendingPopupAnimationPropertyKey, BooleanBoxes.Box(value));
		}
	}

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

	/// <summary>Gets whether the control supports scrolling.</summary>
	/// <returns>true in all cases.</returns>
	protected internal override bool HandlesScrolling => true;

	private bool IsInMenuMode
	{
		get
		{
			if (LogicalParent is MenuBase menuBase)
			{
				return menuBase.IsMenuMode;
			}
			return false;
		}
	}

	private bool OpenOnMouseEnter
	{
		get
		{
			if (LogicalParent is MenuBase menuBase)
			{
				return menuBase.OpenOnMouseEnter;
			}
			return false;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	private bool InsideContextMenu => (bool)GetValue(InsideContextMenuProperty);

	internal object LogicalParent
	{
		get
		{
			if (base.Parent != null)
			{
				return base.Parent;
			}
			return ItemsControl.ItemsControlFromItemContainer(this);
		}
	}

	private MenuItem CurrentSibling
	{
		get
		{
			object logicalParent = LogicalParent;
			MenuItem menuItem = logicalParent as MenuItem;
			MenuItem menuItem2 = null;
			if (menuItem != null)
			{
				menuItem2 = menuItem.CurrentSelection;
			}
			else if (logicalParent is MenuBase menuBase)
			{
				menuItem2 = menuBase.CurrentSelection;
			}
			if (menuItem2 == this)
			{
				menuItem2 = null;
			}
			return menuItem2;
		}
	}

	private bool IsMouseOverSibling
	{
		get
		{
			if (LogicalParent is FrameworkElement elem && IsMouseReallyOver(elem) && !base.IsMouseOver)
			{
				return true;
			}
			return false;
		}
	}

	private MenuItem CurrentSelection
	{
		get
		{
			return _currentSelection;
		}
		set
		{
			if (_currentSelection != null)
			{
				_currentSelection.SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.FalseBox);
			}
			_currentSelection = value;
			if (_currentSelection != null)
			{
				_currentSelection.SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.TrueBox);
			}
		}
	}

	internal override int EffectiveValuesInitialSize => 42;

	private bool CanExecute
	{
		get
		{
			return !ReadControlFlag(ControlBoolFlags.CommandDisabled);
		}
		set
		{
			if (value != CanExecute)
			{
				WriteControlFlag(ControlBoolFlags.CommandDisabled, !value);
				CoerceValue(UIElement.IsEnabledProperty);
			}
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Gets the resource key for a style applied to a <see cref="T:System.Windows.Controls.MenuItem" /> when the <see cref="T:System.Windows.Controls.MenuItem" /> is a <see cref="T:System.Windows.Controls.Separator" />.</summary>
	/// <returns>The resource key for a style applied to a <see cref="T:System.Windows.Controls.MenuItem" /> when the <see cref="T:System.Windows.Controls.MenuItem" /> is a <see cref="T:System.Windows.Controls.Separator" />.</returns>
	public static ResourceKey SeparatorStyleKey => SystemResourceKey.MenuItemSeparatorStyleKey;

	/// <summary>Occurs when a <see cref="T:System.Windows.Controls.MenuItem" /> is clicked. </summary>
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

	/// <summary>Occurs when a menu item is checked. </summary>
	[Category("Behavior")]
	public event RoutedEventHandler Checked
	{
		add
		{
			AddHandler(CheckedEvent, value);
		}
		remove
		{
			RemoveHandler(CheckedEvent, value);
		}
	}

	/// <summary>Occurs when a <see cref="T:System.Windows.Controls.MenuItem" /> is unchecked.</summary>
	[Category("Behavior")]
	public event RoutedEventHandler Unchecked
	{
		add
		{
			AddHandler(UncheckedEvent, value);
		}
		remove
		{
			RemoveHandler(UncheckedEvent, value);
		}
	}

	/// <summary>Occurs when the state of the <see cref="P:System.Windows.Controls.MenuItem.IsSubmenuOpen" /> property changes to true. </summary>
	[Category("Behavior")]
	public event RoutedEventHandler SubmenuOpened
	{
		add
		{
			AddHandler(SubmenuOpenedEvent, value);
		}
		remove
		{
			RemoveHandler(SubmenuOpenedEvent, value);
		}
	}

	/// <summary>Occurs when the state of the <see cref="P:System.Windows.Controls.MenuItem.IsSubmenuOpen" /> property changes to false. </summary>
	[Category("Behavior")]
	public event RoutedEventHandler SubmenuClosed
	{
		add
		{
			AddHandler(SubmenuClosedEvent, value);
		}
		remove
		{
			RemoveHandler(SubmenuClosedEvent, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.MenuItem" /> class. </summary>
	public MenuItem()
	{
	}

	static MenuItem()
	{
		ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuItem));
		PreviewClickEvent = EventManager.RegisterRoutedEvent("PreviewClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuItem));
		CheckedEvent = EventManager.RegisterRoutedEvent("Checked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuItem));
		UncheckedEvent = EventManager.RegisterRoutedEvent("Unchecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuItem));
		SubmenuOpenedEvent = EventManager.RegisterRoutedEvent("SubmenuOpened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuItem));
		SubmenuClosedEvent = EventManager.RegisterRoutedEvent("SubmenuClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuItem));
		CommandProperty = ButtonBase.CommandProperty.AddOwner(typeof(MenuItem), new FrameworkPropertyMetadata(null, OnCommandChanged));
		CommandParameterProperty = ButtonBase.CommandParameterProperty.AddOwner(typeof(MenuItem), new FrameworkPropertyMetadata(null, OnCommandParameterChanged));
		CommandTargetProperty = ButtonBase.CommandTargetProperty.AddOwner(typeof(MenuItem), new FrameworkPropertyMetadata((object)null));
		IsSubmenuOpenProperty = DependencyProperty.Register("IsSubmenuOpen", typeof(bool), typeof(MenuItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSubmenuOpenChanged, CoerceIsSubmenuOpen));
		RolePropertyKey = DependencyProperty.RegisterReadOnly("Role", typeof(MenuItemRole), typeof(MenuItem), new FrameworkPropertyMetadata(MenuItemRole.TopLevelItem));
		RoleProperty = RolePropertyKey.DependencyProperty;
		IsCheckableProperty = DependencyProperty.Register("IsCheckable", typeof(bool), typeof(MenuItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, OnIsCheckableChanged));
		IsPressedPropertyKey = DependencyProperty.RegisterReadOnly("IsPressed", typeof(bool), typeof(MenuItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsPressedProperty = IsPressedPropertyKey.DependencyProperty;
		IsHighlightedPropertyKey = DependencyProperty.RegisterReadOnly("IsHighlighted", typeof(bool), typeof(MenuItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsHighlightedProperty = IsHighlightedPropertyKey.DependencyProperty;
		IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(MenuItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnIsCheckedChanged));
		StaysOpenOnClickProperty = DependencyProperty.Register("StaysOpenOnClick", typeof(bool), typeof(MenuItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(typeof(MenuItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectedChanged));
		InputGestureTextProperty = DependencyProperty.Register("InputGestureText", typeof(string), typeof(MenuItem), new FrameworkPropertyMetadata(string.Empty, OnInputGestureTextChanged, CoerceInputGestureText));
		IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(MenuItem), new FrameworkPropertyMetadata((object)null));
		IsSuspendingPopupAnimationPropertyKey = DependencyProperty.RegisterReadOnly("IsSuspendingPopupAnimation", typeof(bool), typeof(MenuItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsSuspendingPopupAnimationProperty = IsSuspendingPopupAnimationPropertyKey.DependencyProperty;
		ItemContainerTemplateSelectorProperty = MenuBase.ItemContainerTemplateSelectorProperty.AddOwner(typeof(MenuItem), new FrameworkPropertyMetadata(new DefaultItemContainerTemplateSelector()));
		UsesItemContainerTemplateProperty = MenuBase.UsesItemContainerTemplateProperty.AddOwner(typeof(MenuItem));
		InsideContextMenuProperty = DependencyProperty.RegisterAttached("InsideContextMenu", typeof(bool), typeof(MenuItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits));
		BooleanFieldStoreProperty = DependencyProperty.RegisterAttached("BooleanFieldStore", typeof(BoolField), typeof(MenuItem), new FrameworkPropertyMetadata((BoolField)0));
		HeaderedItemsControl.HeaderProperty.OverrideMetadata(typeof(MenuItem), new FrameworkPropertyMetadata(null, CoerceHeader));
		EventManager.RegisterClassHandler(typeof(MenuItem), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(OnAccessKeyPressed));
		EventManager.RegisterClassHandler(typeof(MenuItem), MenuBase.IsSelectedChangedEvent, new RoutedPropertyChangedEventHandler<bool>(OnIsSelectedChanged));
		Control.ForegroundProperty.OverrideMetadata(typeof(MenuItem), new FrameworkPropertyMetadata(SystemColors.MenuTextBrush));
		Control.FontFamilyProperty.OverrideMetadata(typeof(MenuItem), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily));
		Control.FontSizeProperty.OverrideMetadata(typeof(MenuItem), new FrameworkPropertyMetadata(SystemFonts.MessageFontSize));
		Control.FontStyleProperty.OverrideMetadata(typeof(MenuItem), new FrameworkPropertyMetadata(SystemFonts.MessageFontStyle));
		Control.FontWeightProperty.OverrideMetadata(typeof(MenuItem), new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight));
		ToolTipService.IsEnabledProperty.OverrideMetadata(typeof(MenuItem), new FrameworkPropertyMetadata(null, CoerceToolTipIsEnabled));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuItem), new FrameworkPropertyMetadata(typeof(MenuItem)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(MenuItem));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(MenuItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
		FrameworkElement.FocusVisualStyleProperty.OverrideMetadata(typeof(MenuItem), new FrameworkPropertyMetadata((object)null));
		InputMethod.IsInputMethodSuspendedProperty.OverrideMetadata(typeof(MenuItem), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));
		AutomationProperties.IsOffscreenBehaviorProperty.OverrideMetadata(typeof(MenuItem), new FrameworkPropertyMetadata(IsOffscreenBehavior.FromClip));
	}

	private static object CoerceHeader(DependencyObject d, object value)
	{
		MenuItem menuItem = (MenuItem)d;
		if (value == null && !menuItem.HasNonDefaultValue(HeaderedItemsControl.HeaderProperty))
		{
			if (menuItem.Command is RoutedUICommand routedUICommand)
			{
				value = routedUICommand.Text;
			}
			return value;
		}
		if (value is RoutedUICommand routedUICommand2)
		{
			ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(menuItem);
			if (itemsControl != null && itemsControl.ItemContainerGenerator.ItemFromContainer(menuItem) == value)
			{
				return routedUICommand2.Text;
			}
		}
		return value;
	}

	private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((MenuItem)d).OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue);
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
		CoerceValue(HeaderedItemsControl.HeaderProperty);
		CoerceValue(InputGestureTextProperty);
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
		SetBoolField(this, BoolField.CanExecuteInvalid, value: false);
		if (Command != null)
		{
			if (ItemsControl.ItemsControlFromItemContainer(this) is MenuItem { IsSubmenuOpen: false })
			{
				CanExecute = true;
				SetBoolField(this, BoolField.CanExecuteInvalid, value: true);
			}
			else
			{
				CanExecute = CommandHelpers.CanExecuteCommandSource(this);
			}
		}
		else
		{
			CanExecute = true;
		}
	}

	private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((MenuItem)d).UpdateCanExecute();
	}

	private static object CoerceIsSubmenuOpen(DependencyObject d, object value)
	{
		if ((bool)value)
		{
			MenuItem menuItem = (MenuItem)d;
			if (!menuItem.IsLoaded)
			{
				menuItem.RegisterToOpenOnLoad();
				return BooleanBoxes.FalseBox;
			}
		}
		return value;
	}

	private static object CoerceToolTipIsEnabled(DependencyObject d, object value)
	{
		if (!((MenuItem)d).IsSubmenuOpen)
		{
			return value;
		}
		return BooleanBoxes.FalseBox;
	}

	private void RegisterToOpenOnLoad()
	{
		base.Loaded += OpenOnLoad;
	}

	private void OpenOnLoad(object sender, RoutedEventArgs e)
	{
		base.Dispatcher.BeginInvoke(DispatcherPriority.Input, (DispatcherOperationCallback)delegate
		{
			CoerceValue(IsSubmenuOpenProperty);
			return (object)null;
		}, null);
	}

	private static void OnIsSubmenuOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		MenuItem menuItem = (MenuItem)d;
		bool oldValue = (bool)e.OldValue;
		bool flag = (bool)e.NewValue;
		menuItem.StopTimer(ref menuItem._openHierarchyTimer);
		menuItem.StopTimer(ref menuItem._closeHierarchyTimer);
		if (UIElementAutomationPeer.FromElement(menuItem) is MenuItemAutomationPeer menuItemAutomationPeer)
		{
			menuItemAutomationPeer.ResetChildrenCache();
			menuItemAutomationPeer.RaiseExpandCollapseAutomationEvent(oldValue, flag);
		}
		if (flag)
		{
			CommandManager.InvalidateRequerySuggested();
			menuItem.SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.TrueBox);
			if (menuItem.Role == MenuItemRole.TopLevelHeader)
			{
				menuItem.SetMenuMode(menuMode: true);
			}
			menuItem.CurrentSelection = null;
			menuItem.NotifySiblingsToSuspendAnimation();
			for (int i = 0; i < menuItem.Items.Count; i++)
			{
				if (menuItem.ItemContainerGenerator.ContainerFromIndex(i) is MenuItem menuItem2 && GetBoolField(menuItem2, BoolField.CanExecuteInvalid))
				{
					menuItem2.UpdateCanExecute();
				}
			}
			menuItem.OnSubmenuOpened(new RoutedEventArgs(SubmenuOpenedEvent, menuItem));
			SetBoolField(menuItem, BoolField.IgnoreMouseEvents, value: true);
			SetBoolField(menuItem, BoolField.MouseEnterOnMouseMove, value: false);
			menuItem.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate
			{
				SetBoolField(menuItem, BoolField.IgnoreMouseEvents, value: false);
				return (object)null;
			}, null);
		}
		else
		{
			if (menuItem.CurrentSelection != null)
			{
				if (menuItem.CurrentSelection.IsKeyboardFocusWithin)
				{
					menuItem.Focus();
				}
				if (menuItem.CurrentSelection.IsSubmenuOpen)
				{
					menuItem.CurrentSelection.SetCurrentValueInternal(IsSubmenuOpenProperty, BooleanBoxes.FalseBox);
				}
			}
			else if (menuItem.IsKeyboardFocusWithin)
			{
				menuItem.Focus();
			}
			menuItem.CurrentSelection = null;
			if (menuItem.IsMouseOver && menuItem.Role == MenuItemRole.SubmenuHeader)
			{
				SetBoolField(menuItem, BoolField.IgnoreNextMouseLeave, value: true);
			}
			menuItem.NotifyChildrenToResumeAnimation();
			if (menuItem._submenuPopup == null)
			{
				menuItem.OnSubmenuClosed(new RoutedEventArgs(SubmenuClosedEvent, menuItem));
			}
		}
		menuItem.CoerceValue(ToolTipService.IsEnabledProperty);
	}

	private void OnPopupClosed(object source, EventArgs e)
	{
		OnSubmenuClosed(new RoutedEventArgs(SubmenuClosedEvent, this));
	}

	/// <summary>Called when the submenu of a <see cref="T:System.Windows.Controls.MenuItem" /> is opened. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Controls.MenuItem.SubmenuOpened" /> event.</param>
	protected virtual void OnSubmenuOpened(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Called when the submenu of a <see cref="T:System.Windows.Controls.MenuItem" /> is closed. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Controls.MenuItem.SubmenuClosed" /> event.</param>
	protected virtual void OnSubmenuClosed(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	private void UpdateRole()
	{
		SetValue(value: (!IsCheckable && base.HasItems) ? ((LogicalParent is Menu) ? MenuItemRole.TopLevelHeader : MenuItemRole.SubmenuHeader) : ((!(LogicalParent is Menu)) ? MenuItemRole.SubmenuItem : MenuItemRole.TopLevelItem), key: RolePropertyKey);
	}

	private static void OnIsCheckableChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
	{
		((MenuItem)target).UpdateRole();
	}

	private void UpdateIsPressed()
	{
		Rect rect = new Rect(default(Point), base.RenderSize);
		if (Mouse.LeftButton == MouseButtonState.Pressed && base.IsMouseOver && rect.Contains(Mouse.GetPosition(this)))
		{
			IsPressed = true;
		}
		else
		{
			ClearValue(IsPressedPropertyKey);
		}
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.MenuItem.IsChecked" /> property becomes true. This method raises the <see cref="E:System.Windows.Controls.MenuItem.Checked" /> routed event. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Controls.MenuItem.Checked" /> event.</param>
	protected virtual void OnChecked(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.MenuItem.IsChecked" /> property becomes false. This method raises the <see cref="E:System.Windows.Controls.MenuItem.Unchecked" /> routed event. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Controls.MenuItem.Unchecked" /> event.</param>
	protected virtual void OnUnchecked(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		MenuItem menuItem = (MenuItem)d;
		bool oldValue = (bool)e.OldValue;
		bool flag = (bool)e.NewValue;
		if (flag)
		{
			menuItem.OnChecked(new RoutedEventArgs(CheckedEvent));
		}
		else
		{
			menuItem.OnUnchecked(new RoutedEventArgs(UncheckedEvent));
		}
		if (UIElementAutomationPeer.FromElement(menuItem) is MenuItemAutomationPeer menuItemAutomationPeer)
		{
			menuItemAutomationPeer.RaiseToggleStatePropertyChangedEvent(oldValue, flag);
		}
	}

	private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		MenuItem menuItem = (MenuItem)d;
		menuItem.SetValue(IsHighlightedPropertyKey, e.NewValue);
		if ((bool)e.OldValue)
		{
			if (menuItem.IsSubmenuOpen)
			{
				menuItem.SetCurrentValueInternal(IsSubmenuOpenProperty, BooleanBoxes.FalseBox);
			}
			menuItem.StopTimer(ref menuItem._openHierarchyTimer);
			menuItem.StopTimer(ref menuItem._closeHierarchyTimer);
		}
		menuItem.RaiseEvent(new RoutedPropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue, MenuBase.IsSelectedChangedEvent));
	}

	private static void OnIsSelectedChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
	{
		if (sender == e.OriginalSource)
		{
			return;
		}
		MenuItem menuItem = (MenuItem)sender;
		if (!(e.OriginalSource is MenuItem menuItem2))
		{
			return;
		}
		if (e.NewValue)
		{
			if (menuItem.CurrentSelection == menuItem2)
			{
				menuItem.StopTimer(ref menuItem._closeHierarchyTimer);
			}
			if (menuItem.CurrentSelection != menuItem2 && menuItem2.LogicalParent == menuItem)
			{
				if (menuItem.CurrentSelection != null && menuItem.CurrentSelection.IsSubmenuOpen)
				{
					menuItem.CurrentSelection.SetCurrentValueInternal(IsSubmenuOpenProperty, BooleanBoxes.FalseBox);
				}
				menuItem.CurrentSelection = menuItem2;
			}
		}
		else if (menuItem.CurrentSelection == menuItem2)
		{
			menuItem.CurrentSelection = null;
		}
		e.Handled = true;
	}

	private static void OnInputGestureTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
	}

	private static object CoerceInputGestureText(DependencyObject d, object value)
	{
		MenuItem menuItem = (MenuItem)d;
		if (string.IsNullOrEmpty((string)value) && !menuItem.HasNonDefaultValue(InputGestureTextProperty) && menuItem.Command is RoutedCommand { InputGestures: { Count: >=1 } inputGestures })
		{
			for (int i = 0; i < inputGestures.Count; i++)
			{
				if (((IList)inputGestures)[i] is KeyGesture keyGesture)
				{
					return keyGesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture);
				}
			}
		}
		return value;
	}

	private void NotifySiblingsToSuspendAnimation()
	{
		if (IsSuspendingPopupAnimation)
		{
			return;
		}
		bool boolField = GetBoolField(this, BoolField.OpenedWithKeyboard);
		MenuItem ignore = (boolField ? null : this);
		MenuBase.SetSuspendingPopupAnimation(ItemsControl.ItemsControlFromItemContainer(this), ignore, suspend: true);
		if (!boolField)
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Input, (DispatcherOperationCallback)delegate(object arg)
			{
				((MenuItem)arg).IsSuspendingPopupAnimation = true;
				return (object)null;
			}, this);
		}
		else
		{
			SetBoolField(this, BoolField.OpenedWithKeyboard, value: false);
		}
	}

	private void NotifyChildrenToResumeAnimation()
	{
		MenuBase.SetSuspendingPopupAnimation(this, null, suspend: false);
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.MenuItemAutomationPeer" /> implementation for this control, as part of the WPF automation infrastructure.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.MenuItemAutomationPeer" /> for this <see cref="T:System.Windows.Controls.MenuItem" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new MenuItemAutomationPeer(this);
	}

	/// <summary>Called when the <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> property is set to true and raises an <see cref="E:System.Windows.FrameworkElement.Initialized" /> event. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.FrameworkElement.Initialized" /> event.</param>
	protected override void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);
		UpdateRole();
	}

	/// <summary> Prepares the specified element to display the specified item. </summary>
	/// <param name="element">Element used to display the specified item.</param>
	/// <param name="item">Specified item.</param>
	protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		base.PrepareContainerForItemOverride(element, item);
		PrepareMenuItem(element, item);
	}

	internal static void PrepareMenuItem(DependencyObject element, object item)
	{
		if (element is MenuItem menuItem)
		{
			if (item is ICommand command && !menuItem.HasNonDefaultValue(CommandProperty))
			{
				menuItem.Command = command;
			}
			if (GetBoolField(menuItem, BoolField.CanExecuteInvalid))
			{
				menuItem.UpdateCanExecute();
			}
		}
		else if (item is Separator separator)
		{
			if (separator.GetValueSource(FrameworkElement.StyleProperty, null, out var _) <= BaseValueSourceInternal.ImplicitReference)
			{
				separator.SetResourceReference(FrameworkElement.StyleProperty, SeparatorStyleKey);
			}
			separator.DefaultStyleKey = SeparatorStyleKey;
		}
	}

	/// <summary>Called when a <see cref="T:System.Windows.Controls.MenuItem" /> is clicked and raises a <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> event.</summary>
	protected virtual void OnClick()
	{
		OnClickImpl(userInitiated: false);
	}

	internal virtual void OnClickCore(bool userInitiated)
	{
		OnClick();
	}

	internal void OnClickImpl(bool userInitiated)
	{
		if (IsCheckable)
		{
			SetCurrentValueInternal(IsCheckedProperty, BooleanBoxes.Box(!IsChecked));
		}
		if (!base.IsKeyboardFocusWithin)
		{
			FocusOrSelect();
		}
		RaiseEvent(new RoutedEventArgs(PreviewClickEvent, this));
		if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
		{
			UIElementAutomationPeer.CreatePeerForElement(this)?.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
		}
		base.Dispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(InvokeClickAfterRender), userInitiated);
	}

	private object InvokeClickAfterRender(object arg)
	{
		bool userInitiated = (bool)arg;
		RaiseEvent(new RoutedEventArgs(ClickEvent, this));
		CommandHelpers.CriticalExecuteCommandSource(this, userInitiated);
		return null;
	}

	/// <summary>Called when the left mouse button is pressed. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.MouseLeftButtonDown" /> event.</param>
	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		if (!e.Handled)
		{
			HandleMouseDown(e);
			UpdateIsPressed();
			if (e.UserInitiated)
			{
				_userInitiatedPress = true;
			}
		}
		base.OnMouseLeftButtonDown(e);
	}

	/// <summary>Called when the right mouse button is pressed. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.MouseRightButtonDown" /> event.</param>
	protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
	{
		if (!e.Handled)
		{
			HandleMouseDown(e);
			if (e.UserInitiated)
			{
				_userInitiatedPress = true;
			}
		}
		base.OnMouseRightButtonDown(e);
	}

	/// <summary>Called when the left mouse button is released. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.MouseLeftButtonUp" /> event.</param>
	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		if (!e.Handled)
		{
			HandleMouseUp(e);
			UpdateIsPressed();
			_userInitiatedPress = false;
		}
		base.OnMouseLeftButtonUp(e);
	}

	/// <summary>Called when the right mouse button is released. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.MouseRightButtonUp" /> event.</param>
	protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
	{
		if (!e.Handled)
		{
			HandleMouseUp(e);
			_userInitiatedPress = false;
		}
		base.OnMouseRightButtonUp(e);
	}

	private void HandleMouseDown(MouseButtonEventArgs e)
	{
		Rect rect = new Rect(default(Point), base.RenderSize);
		if (rect.Contains(e.GetPosition(this)) && (e.ChangedButton == MouseButton.Left || (e.ChangedButton == MouseButton.Right && InsideContextMenu)))
		{
			MenuItemRole role = Role;
			if (role == MenuItemRole.TopLevelHeader || role == MenuItemRole.SubmenuHeader)
			{
				ClickHeader();
			}
		}
		e.Handled = true;
	}

	private void HandleMouseUp(MouseButtonEventArgs e)
	{
		Rect rect = new Rect(default(Point), base.RenderSize);
		if (rect.Contains(e.GetPosition(this)) && (e.ChangedButton == MouseButton.Left || (e.ChangedButton == MouseButton.Right && InsideContextMenu)))
		{
			MenuItemRole role = Role;
			if (role == MenuItemRole.TopLevelItem || role == MenuItemRole.SubmenuItem)
			{
				if (_userInitiatedPress)
				{
					ClickItem(e.UserInitiated);
				}
				else
				{
					ClickItem(userInitiated: false);
				}
			}
		}
		if (e.ChangedButton != MouseButton.Right || InsideContextMenu)
		{
			e.Handled = true;
		}
	}

	private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
	{
		MenuItem menuItem = sender as MenuItem;
		bool flag = false;
		if (e.Target == null)
		{
			if (Mouse.Captured == null || Mouse.Captured is MenuBase)
			{
				e.Target = menuItem;
				if (e.OriginalSource == menuItem && menuItem.IsSubmenuOpen)
				{
					flag = true;
				}
			}
			else
			{
				e.Handled = true;
			}
		}
		else if (e.Scope == null)
		{
			if (e.Target != menuItem && e.Target is MenuItem)
			{
				flag = true;
			}
			else
			{
				DependencyObject dependencyObject = e.Source as DependencyObject;
				while (dependencyObject != null && dependencyObject != menuItem)
				{
					if (dependencyObject is UIElement container && ItemsControl.ItemsControlFromItemContainer(container) == menuItem)
					{
						flag = true;
						break;
					}
					dependencyObject = FrameworkElement.GetFrameworkParent(dependencyObject);
				}
			}
		}
		if (flag)
		{
			e.Scope = menuItem;
			e.Handled = true;
		}
	}

	/// <summary>Called whenever the mouse leaves a <see cref="T:System.Windows.Controls.MenuItem" />. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Input.Mouse.MouseLeave" /> event.</param>
	protected override void OnMouseLeave(MouseEventArgs e)
	{
		base.OnMouseLeave(e);
		MenuItemRole role = Role;
		if (((role == MenuItemRole.TopLevelHeader || role == MenuItemRole.TopLevelItem) && IsInMenuMode) || role == MenuItemRole.SubmenuHeader || role == MenuItemRole.SubmenuItem)
		{
			MouseLeaveInMenuMode(role);
		}
		else if (base.IsMouseOver != IsSelected)
		{
			SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.Box(base.IsMouseOver));
		}
		UpdateIsPressed();
	}

	/// <summary>Called when the mouse is moved over a menu item.</summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Input.Mouse.MouseMove" /> event.</param>
	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (ItemsControl.ItemsControlFromItemContainer(this) is MenuItem element && GetBoolField(element, BoolField.MouseEnterOnMouseMove))
		{
			SetBoolField(element, BoolField.MouseEnterOnMouseMove, value: false);
			MouseEnterHelper();
		}
	}

	/// <summary>Called whenever the mouse enters a <see cref="T:System.Windows.Controls.MenuItem" />. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Input.Mouse.MouseEnter" /> event.</param>
	protected override void OnMouseEnter(MouseEventArgs e)
	{
		base.OnMouseEnter(e);
		MouseEnterHelper();
	}

	private void MouseEnterHelper()
	{
		ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(this);
		if (itemsControl == null || !GetBoolField(itemsControl, BoolField.IgnoreMouseEvents))
		{
			MenuItemRole role = Role;
			if (((role == MenuItemRole.TopLevelHeader || role == MenuItemRole.TopLevelItem) && OpenOnMouseEnter) || role == MenuItemRole.SubmenuHeader || role == MenuItemRole.SubmenuItem)
			{
				MouseEnterInMenuMode(role);
			}
			else if (base.IsMouseOver != IsSelected)
			{
				SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.Box(base.IsMouseOver));
			}
			UpdateIsPressed();
		}
		else if (itemsControl is MenuItem)
		{
			SetBoolField(itemsControl, BoolField.MouseEnterOnMouseMove, value: true);
		}
	}

	private void MouseEnterInMenuMode(MenuItemRole role)
	{
		switch (role)
		{
		case MenuItemRole.TopLevelItem:
		case MenuItemRole.TopLevelHeader:
			if (!IsSubmenuOpen)
			{
				OpenHierarchy(role);
			}
			break;
		case MenuItemRole.SubmenuItem:
		case MenuItemRole.SubmenuHeader:
		{
			MenuItem currentSibling = CurrentSibling;
			if (currentSibling == null || !currentSibling.IsSubmenuOpen)
			{
				if (!IsSubmenuOpen)
				{
					FocusOrSelect();
				}
				else
				{
					IsHighlighted = true;
				}
			}
			else
			{
				currentSibling.IsHighlighted = false;
				IsHighlighted = true;
			}
			if (!IsSelected || !IsSubmenuOpen)
			{
				SetTimerToOpenHierarchy();
			}
			break;
		}
		}
		StopTimer(ref _closeHierarchyTimer);
	}

	private void MouseLeaveInMenuMode(MenuItemRole role)
	{
		if (role == MenuItemRole.SubmenuHeader || role == MenuItemRole.SubmenuItem)
		{
			if (GetBoolField(this, BoolField.IgnoreNextMouseLeave))
			{
				SetBoolField(this, BoolField.IgnoreNextMouseLeave, value: false);
			}
			else if (!IsSubmenuOpen)
			{
				if (IsSelected)
				{
					SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.FalseBox);
				}
				else
				{
					IsHighlighted = false;
				}
				if (base.IsKeyboardFocusWithin)
				{
					ItemsControl.ItemsControlFromItemContainer(this)?.Focus();
				}
			}
			else if (IsMouseOverSibling)
			{
				SetTimerToCloseHierarchy();
			}
		}
		StopTimer(ref _openHierarchyTimer);
	}

	/// <summary>Announces that the keyboard is focused on this element. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.GotFocus" /> event.</param>
	protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		base.OnGotKeyboardFocus(e);
		if (!e.Handled && e.NewFocus == this)
		{
			SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.TrueBox);
		}
	}

	/// <summary>Called when the focus is no longer on or within a <see cref="T:System.Windows.Controls.MenuItem" />.</summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.ContentElement.IsKeyboardFocusWithinChanged" /> event.</param>
	protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnIsKeyboardFocusWithinChanged(e);
		if (base.IsKeyboardFocusWithin && !IsSelected)
		{
			SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.TrueBox);
		}
	}

	/// <summary> Responds to the <see cref="E:System.Windows.UIElement.KeyDown" /> event. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.KeyDown" /> event.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		bool flag = false;
		Key key = e.Key;
		MenuItemRole role = Role;
		if (base.FlowDirection == FlowDirection.RightToLeft)
		{
			switch (key)
			{
			case Key.Right:
				key = Key.Left;
				break;
			case Key.Left:
				key = Key.Right;
				break;
			}
		}
		switch (key)
		{
		case Key.Tab:
			if (role == MenuItemRole.SubmenuHeader && IsSubmenuOpen && CurrentSelection == null)
			{
				if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
				{
					NavigateToEnd(new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
				}
				else
				{
					NavigateToStart(new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
				}
				flag = true;
			}
			break;
		case Key.Right:
			if (role == MenuItemRole.SubmenuHeader && !IsSubmenuOpen)
			{
				OpenSubmenuWithKeyboard();
				flag = true;
			}
			break;
		case Key.Return:
			switch (role)
			{
			case MenuItemRole.TopLevelItem:
			case MenuItemRole.SubmenuItem:
				ClickItem(e.UserInitiated);
				flag = true;
				break;
			case MenuItemRole.TopLevelHeader:
				OpenSubmenuWithKeyboard();
				flag = true;
				break;
			case MenuItemRole.SubmenuHeader:
				if (!IsSubmenuOpen)
				{
					OpenSubmenuWithKeyboard();
					flag = true;
				}
				break;
			}
			break;
		case Key.Down:
			if (role == MenuItemRole.SubmenuHeader && IsSubmenuOpen && CurrentSelection == null)
			{
				NavigateToStart(new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
				flag = true;
			}
			break;
		case Key.Up:
			if (role == MenuItemRole.SubmenuHeader && IsSubmenuOpen && CurrentSelection == null)
			{
				NavigateToEnd(new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
				flag = true;
			}
			break;
		case Key.Escape:
		case Key.Left:
			if (role != MenuItemRole.TopLevelHeader && role != 0 && IsSubmenuOpen)
			{
				SetCurrentValueInternal(IsSubmenuOpenProperty, BooleanBoxes.FalseBox);
				flag = true;
			}
			break;
		}
		if (!flag)
		{
			ItemsControl parent = ItemsControl.ItemsControlFromItemContainer(this);
			if (parent != null && !GetBoolField(parent, BoolField.IgnoreMouseEvents))
			{
				SetBoolField(parent, BoolField.IgnoreMouseEvents, value: true);
				parent.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate
				{
					SetBoolField(parent, BoolField.IgnoreMouseEvents, value: false);
					return (object)null;
				}, null);
			}
			flag = MenuItemNavigate(e.Key, e.KeyboardDevice.Modifiers);
		}
		if (flag)
		{
			e.Handled = true;
		}
	}

	/// <summary>Responds when the <see cref="P:System.Windows.Controls.AccessText.AccessKey" /> for this control is invoked. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Input.AccessKeyManager.AccessKeyPressed" /> event.</param>
	protected override void OnAccessKey(AccessKeyEventArgs e)
	{
		base.OnAccessKey(e);
		if (!e.IsMultiple)
		{
			switch (Role)
			{
			case MenuItemRole.TopLevelItem:
			case MenuItemRole.SubmenuItem:
				ClickItem(e.UserInitiated);
				break;
			case MenuItemRole.TopLevelHeader:
			case MenuItemRole.SubmenuHeader:
				OpenSubmenuWithKeyboard();
				break;
			}
		}
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> property changes. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Controls.ItemContainerGenerator.ItemsChanged" /> event.</param>
	protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
	{
		UpdateRole();
		base.OnItemsChanged(e);
	}

	/// <summary> Determines if the specified item is (or is eligible to be) its own ItemContainer. </summary>
	/// <returns>true if the item is its own ItemContainer; otherwise, false.</returns>
	/// <param name="item">Specified item.</param>
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

	/// <summary>Used to determine whether to apply a style to the item container.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.MenuItem" /> is not a <see cref="T:System.Windows.Controls.Separator" />; otherwise, false.</returns>
	/// <param name="container">Container to which the style will be applied.</param>
	/// <param name="item">Item to which the container belongs.</param>
	protected override bool ShouldApplyItemContainerStyle(DependencyObject container, object item)
	{
		if (item is Separator)
		{
			return false;
		}
		return base.ShouldApplyItemContainerStyle(container, item);
	}

	/// <summary>Creates or identifies the element used to display a specified item.</summary>
	/// <returns>The element used to display a specified item.</returns>
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

	/// <summary>Called when the parent of the visual <see cref="T:System.Windows.Controls.MenuItem" /> changes. </summary>
	/// <param name="oldParent">Old value of the parent of the visual, or null if the visual did not have a parent.</param>
	protected internal override void OnVisualParentChanged(DependencyObject oldParent)
	{
		base.OnVisualParentChanged(oldParent);
		UpdateRole();
		DependencyObject parentInternal = VisualTreeHelper.GetParentInternal(this);
		if (base.Parent != null && parentInternal != null && base.Parent != parentInternal)
		{
			Binding binding = new Binding();
			binding.Path = new PropertyPath(DefinitionBase.PrivateSharedSizeScopeProperty);
			binding.Mode = BindingMode.OneWay;
			binding.Source = parentInternal;
			BindingOperations.SetBinding(this, DefinitionBase.PrivateSharedSizeScopeProperty, binding);
		}
		if (parentInternal == null)
		{
			BindingOperations.ClearBinding(this, DefinitionBase.PrivateSharedSizeScopeProperty);
		}
	}

	/// <summary>Called when the template's tree is generated.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		if (_submenuPopup != null)
		{
			_submenuPopup.Closed -= OnPopupClosed;
		}
		_submenuPopup = GetTemplateChild("PART_Popup") as Popup;
		if (_submenuPopup != null)
		{
			_submenuPopup.Closed += OnPopupClosed;
		}
	}

	private void SetMenuMode(bool menuMode)
	{
		if (LogicalParent is MenuBase menuBase && menuBase.IsMenuMode != menuMode)
		{
			menuBase.IsMenuMode = menuMode;
		}
	}

	internal static void SetInsideContextMenuProperty(UIElement element, bool value)
	{
		element.SetValue(InsideContextMenuProperty, BooleanBoxes.Box(value));
	}

	internal void ClickItem()
	{
		ClickItem(userInitiated: false);
	}

	private void ClickItem(bool userInitiated)
	{
		try
		{
			OnClickCore(userInitiated);
		}
		finally
		{
			if (Role == MenuItemRole.TopLevelItem && !StaysOpenOnClick)
			{
				SetMenuMode(menuMode: false);
			}
		}
	}

	internal void ClickHeader()
	{
		if (!base.IsKeyboardFocusWithin)
		{
			FocusOrSelect();
		}
		if (IsSubmenuOpen)
		{
			if (Role == MenuItemRole.TopLevelHeader)
			{
				SetMenuMode(menuMode: false);
			}
		}
		else
		{
			OpenMenu();
		}
	}

	internal bool OpenMenu()
	{
		if (!IsSubmenuOpen)
		{
			ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(this);
			if (itemsControl == null)
			{
				itemsControl = VisualTreeHelper.GetParent(this) as ItemsControl;
			}
			if (itemsControl != null && (itemsControl is MenuItem || itemsControl is MenuBase))
			{
				SetCurrentValueInternal(IsSubmenuOpenProperty, BooleanBoxes.TrueBox);
				return true;
			}
		}
		return false;
	}

	internal void OpenSubmenuWithKeyboard()
	{
		SetBoolField(this, BoolField.OpenedWithKeyboard, value: true);
		if (OpenMenu())
		{
			NavigateToStart(new ItemNavigateArgs(Keyboard.PrimaryDevice, Keyboard.Modifiers));
		}
	}

	private bool MenuItemNavigate(Key key, ModifierKeys modifiers)
	{
		if (key == Key.Left || key == Key.Right || key == Key.Up || key == Key.Down)
		{
			ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(this);
			if (itemsControl != null)
			{
				if (!itemsControl.HasItems)
				{
					return false;
				}
				if (itemsControl.Items.Count == 1 && !(itemsControl is Menu) && key == Key.Up && key == Key.Down)
				{
					return true;
				}
				object focusedElement = Keyboard.FocusedElement;
				itemsControl.NavigateByLine(itemsControl.FocusedInfo, KeyboardNavigation.KeyToTraversalDirection(key), new ItemNavigateArgs(Keyboard.PrimaryDevice, modifiers));
				object focusedElement2 = Keyboard.FocusedElement;
				if (focusedElement2 != focusedElement && focusedElement2 != this)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsMouseReallyOver(FrameworkElement elem)
	{
		bool isMouseOver = elem.IsMouseOver;
		if (isMouseOver && Mouse.Captured == elem && Mouse.DirectlyOver == elem)
		{
			return false;
		}
		return isMouseOver;
	}

	private void OpenHierarchy(MenuItemRole role)
	{
		FocusOrSelect();
		if (role == MenuItemRole.TopLevelHeader || role == MenuItemRole.SubmenuHeader)
		{
			OpenMenu();
		}
	}

	private void FocusOrSelect()
	{
		if (!base.IsKeyboardFocusWithin)
		{
			Focus();
		}
		if (!IsSelected)
		{
			SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.TrueBox);
		}
		if (IsSelected && !IsHighlighted)
		{
			IsHighlighted = true;
		}
	}

	private void SetTimerToOpenHierarchy()
	{
		if (_openHierarchyTimer == null)
		{
			_openHierarchyTimer = new DispatcherTimer(DispatcherPriority.Normal);
			_openHierarchyTimer.Tick += delegate
			{
				OpenHierarchy(Role);
				StopTimer(ref _openHierarchyTimer);
			};
		}
		else
		{
			_openHierarchyTimer.Stop();
		}
		StartTimer(_openHierarchyTimer);
	}

	private void SetTimerToCloseHierarchy()
	{
		if (_closeHierarchyTimer == null)
		{
			_closeHierarchyTimer = new DispatcherTimer(DispatcherPriority.Normal);
			_closeHierarchyTimer.Tick += delegate
			{
				SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.FalseBox);
				StopTimer(ref _closeHierarchyTimer);
			};
		}
		else
		{
			_closeHierarchyTimer.Stop();
		}
		StartTimer(_closeHierarchyTimer);
	}

	private void StopTimer(ref DispatcherTimer timer)
	{
		if (timer != null)
		{
			timer.Stop();
			timer = null;
		}
	}

	private void StartTimer(DispatcherTimer timer)
	{
		timer.Interval = TimeSpan.FromMilliseconds(SystemParameters.MenuShowDelay);
		timer.Start();
	}

	private static object OnCoerceAcceleratorKey(DependencyObject d, object value)
	{
		if (value == null)
		{
			string inputGestureText = ((MenuItem)d).InputGestureText;
			if (inputGestureText != string.Empty)
			{
				value = inputGestureText;
			}
		}
		return value;
	}

	private static bool GetBoolField(UIElement element, BoolField field)
	{
		return ((BoolField)element.GetValue(BooleanFieldStoreProperty) & field) != 0;
	}

	private static void SetBoolField(UIElement element, BoolField field, bool value)
	{
		if (value)
		{
			element.SetValue(BooleanFieldStoreProperty, (BoolField)element.GetValue(BooleanFieldStoreProperty) | field);
		}
		else
		{
			element.SetValue(BooleanFieldStoreProperty, (BoolField)element.GetValue(BooleanFieldStoreProperty) & ~field);
		}
	}
}
