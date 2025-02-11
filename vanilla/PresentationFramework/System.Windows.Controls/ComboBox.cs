using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Documents;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;
using MS.Win32;

namespace System.Windows.Controls;

/// <summary>Represents a selection control with a drop-down list that can be shown or hidden by clicking the arrow on the control. </summary>
[Localizability(LocalizationCategory.ComboBox)]
[TemplatePart(Name = "PART_EditableTextBox", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ComboBoxItem))]
public class ComboBox : Selector
{
	private enum CacheBits
	{
		IsMouseOverItemsHost = 1,
		HasMouseEnteredItemsHost = 2,
		IsContextMenuOpen = 4,
		UpdatingText = 8,
		UpdatingSelectedItem = 0x10,
		IsWaitingForTextComposition = 0x20
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ComboBox.MaxDropDownHeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ComboBox.MaxDropDownHeight" /> dependency property.</returns>
	public static readonly DependencyProperty MaxDropDownHeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ComboBox.IsDropDownOpen" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ComboBox.IsDropDownOpen" /> dependency property.</returns>
	public static readonly DependencyProperty IsDropDownOpenProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ComboBox.ShouldPreserveUserEnteredPrefix" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ComboBox.ShouldPreserveUserEnteredPrefix" /> dependency property.</returns>
	public static readonly DependencyProperty ShouldPreserveUserEnteredPrefixProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ComboBox.IsEditable" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ComboBox.IsEditable" /> dependency property.</returns>
	public static readonly DependencyProperty IsEditableProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ComboBox.Text" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ComboBox.Text" /> dependency property.</returns>
	public static readonly DependencyProperty TextProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ComboBox.IsReadOnly" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ComboBox.IsReadOnly" /> dependency property.</returns>
	public static readonly DependencyProperty IsReadOnlyProperty;

	private static readonly DependencyPropertyKey SelectionBoxItemPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ComboBox.SelectionBoxItem" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ComboBox.SelectionBoxItem" /> dependency property.</returns>
	public static readonly DependencyProperty SelectionBoxItemProperty;

	private static readonly DependencyPropertyKey SelectionBoxItemTemplatePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ComboBox.SelectionBoxItemTemplate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ComboBox.SelectionBoxItemTemplate" /> dependency property.</returns>
	public static readonly DependencyProperty SelectionBoxItemTemplateProperty;

	private static readonly DependencyPropertyKey SelectionBoxItemStringFormatPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ComboBox.SelectionBoxItemStringFormat" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ComboBox.SelectionBoxItemStringFormat" /> dependency property.</returns>
	public static readonly DependencyProperty SelectionBoxItemStringFormatProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ComboBox.StaysOpenOnEdit" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ComboBox.StaysOpenOnEdit" /> dependency property.</returns>
	public static readonly DependencyProperty StaysOpenOnEditProperty;

	private static readonly DependencyPropertyKey IsSelectionBoxHighlightedPropertyKey;

	private static readonly DependencyProperty IsSelectionBoxHighlightedProperty;

	private static readonly EventPrivateKey DropDownOpenedKey;

	private static readonly EventPrivateKey DropDownClosedKey;

	private const string EditableTextBoxTemplateName = "PART_EditableTextBox";

	private const string PopupTemplateName = "PART_Popup";

	private TextBox _editableTextBoxSite;

	private Popup _dropDownPopup;

	private int _textBoxSelectionStart;

	private BitVector32 _cacheValid = new BitVector32(0);

	private ItemInfo _highlightedInfo;

	private DispatcherTimer _autoScrollTimer;

	private UIElement _clonedElement;

	private DispatcherOperation _updateTextBoxOperation;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets the maximum height for a combo box drop-down. </summary>
	/// <returns>A double that represents the height that is retrieved or the height to set. The default value as defined to the property system is a calculated value based on taking a one-third fraction of the system max screen height parameters, but this default is potentially overridden by various control templates.</returns>
	[Bindable(true)]
	[Category("Layout")]
	[TypeConverter(typeof(LengthConverter))]
	public double MaxDropDownHeight
	{
		get
		{
			return (double)GetValue(MaxDropDownHeightProperty);
		}
		set
		{
			SetValue(MaxDropDownHeightProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the drop-down for a combo box is currently open.  </summary>
	/// <returns>true if the drop-down is open; otherwise, false. The default is false.</returns>
	[Bindable(true)]
	[Browsable(false)]
	[Category("Appearance")]
	public bool IsDropDownOpen
	{
		get
		{
			return (bool)GetValue(IsDropDownOpenProperty);
		}
		set
		{
			SetValue(IsDropDownOpenProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.ComboBox" /> keeps the user's input or replaces the input with a matching item.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.ComboBox" /> keeps the user's input; false if the <see cref="T:System.Windows.Controls.ComboBox" /> replaces the input with a matching item The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public bool ShouldPreserveUserEnteredPrefix
	{
		get
		{
			return (bool)GetValue(ShouldPreserveUserEnteredPrefixProperty);
		}
		set
		{
			SetValue(ShouldPreserveUserEnteredPrefixProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a value that enables or disables editing of the text in text box of the <see cref="T:System.Windows.Controls.ComboBox" />. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.ComboBox" /> can be edited; otherwise false. The default is false.</returns>
	public bool IsEditable
	{
		get
		{
			return (bool)GetValue(IsEditableProperty);
		}
		set
		{
			SetValue(IsEditableProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets the text of the currently selected item. </summary>
	/// <returns>The string of the currently selected item. The default is an empty string ("").</returns>
	public string Text
	{
		get
		{
			return (string)GetValue(TextProperty);
		}
		set
		{
			SetValue(TextProperty, value);
		}
	}

	/// <summary>Gets or sets a value that enables selection-only mode, in which the contents of the combo box are selectable but not editable. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.ComboBox" /> is read-only; otherwise, false. The default is false.</returns>
	public bool IsReadOnly
	{
		get
		{
			return (bool)GetValue(IsReadOnlyProperty);
		}
		set
		{
			SetValue(IsReadOnlyProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets the item that is displayed in the selection box. </summary>
	/// <returns>The selected item.</returns>
	public object SelectionBoxItem
	{
		get
		{
			return GetValue(SelectionBoxItemProperty);
		}
		private set
		{
			SetValue(SelectionBoxItemPropertyKey, value);
		}
	}

	/// <summary>Gets the item template of the selection box content. </summary>
	/// <returns>An item template.</returns>
	public DataTemplate SelectionBoxItemTemplate
	{
		get
		{
			return (DataTemplate)GetValue(SelectionBoxItemTemplateProperty);
		}
		private set
		{
			SetValue(SelectionBoxItemTemplatePropertyKey, value);
		}
	}

	/// <summary>Gets a composite string that specifies how to format the selected item in the selection box if it is displayed as a string.</summary>
	/// <returns>A composite string that specifies how to format the selected item in the selection box if it is displayed as a string.</returns>
	public string SelectionBoxItemStringFormat
	{
		get
		{
			return (string)GetValue(SelectionBoxItemStringFormatProperty);
		}
		private set
		{
			SetValue(SelectionBoxItemStringFormatPropertyKey, value);
		}
	}

	/// <summary>Gets or sets whether a <see cref="T:System.Windows.Controls.ComboBox" /> that is open and displays a drop-down control will remain open when a user clicks the <see cref="T:System.Windows.Controls.TextBox" />. </summary>
	/// <returns>true to keep the drop-down control open when the user clicks on the text area to start editing; otherwise, false. The default is false.</returns>
	public bool StaysOpenOnEdit
	{
		get
		{
			return (bool)GetValue(StaysOpenOnEditProperty);
		}
		set
		{
			SetValue(StaysOpenOnEditProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets whether the <see cref="P:System.Windows.Controls.ComboBox.SelectionBoxItem" /> is highlighted.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.ComboBox.SelectionBoxItem" /> is highlighted; otherwise, false.</returns>
	public bool IsSelectionBoxHighlighted => (bool)GetValue(IsSelectionBoxHighlightedProperty);

	/// <summary>Gets a value that indicates whether a combo box supports scrolling.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.ComboBox" /> supports scrolling; otherwise, false. The default is true.</returns>
	protected internal override bool HandlesScrolling => true;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.ComboBox" /> has focus. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.ComboBox" /> has focus; otherwise, false.</returns>
	protected override bool HasEffectiveKeyboardFocus
	{
		protected internal get
		{
			if (IsEditable && EditableTextBoxSite != null)
			{
				return EditableTextBoxSite.HasEffectiveKeyboardFocus;
			}
			return base.HasEffectiveKeyboardFocus;
		}
	}

	internal TextBox EditableTextBoxSite
	{
		get
		{
			return _editableTextBoxSite;
		}
		set
		{
			_editableTextBoxSite = value;
		}
	}

	private bool HasCapture => Mouse.Captured == this;

	private bool IsItemsHostVisible
	{
		get
		{
			Panel itemsHost = base.ItemsHost;
			if (itemsHost != null && PresentationSource.CriticalFromVisual(itemsHost) is HwndSource { IsDisposed: false, RootVisual: not null } hwndSource)
			{
				return hwndSource.RootVisual.IsAncestorOf(itemsHost);
			}
			return false;
		}
	}

	private ItemInfo HighlightedInfo
	{
		get
		{
			return _highlightedInfo;
		}
		set
		{
			((_highlightedInfo != null) ? (_highlightedInfo.Container as ComboBoxItem) : null)?.SetIsHighlighted(isHighlighted: false);
			_highlightedInfo = value;
			((_highlightedInfo != null) ? (_highlightedInfo.Container as ComboBoxItem) : null)?.SetIsHighlighted(isHighlighted: true);
			CoerceValue(IsSelectionBoxHighlightedProperty);
		}
	}

	private ComboBoxItem HighlightedElement
	{
		get
		{
			if (!(_highlightedInfo == null))
			{
				return _highlightedInfo.Container as ComboBoxItem;
			}
			return null;
		}
	}

	private bool IsMouseOverItemsHost
	{
		get
		{
			return _cacheValid[1];
		}
		set
		{
			_cacheValid[1] = value;
		}
	}

	private bool HasMouseEnteredItemsHost
	{
		get
		{
			return _cacheValid[2];
		}
		set
		{
			_cacheValid[2] = value;
		}
	}

	private bool IsContextMenuOpen
	{
		get
		{
			return _cacheValid[4];
		}
		set
		{
			_cacheValid[4] = value;
		}
	}

	private bool UpdatingText
	{
		get
		{
			return _cacheValid[8];
		}
		set
		{
			_cacheValid[8] = value;
		}
	}

	private bool UpdatingSelectedItem
	{
		get
		{
			return _cacheValid[16];
		}
		set
		{
			_cacheValid[16] = value;
		}
	}

	private bool IsWaitingForTextComposition
	{
		get
		{
			return _cacheValid[32];
		}
		set
		{
			_cacheValid[32] = value;
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Occurs when the drop-down list of the combo box opens. </summary>
	public event EventHandler DropDownOpened
	{
		add
		{
			EventHandlersStoreAdd(DropDownOpenedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(DropDownOpenedKey, value);
		}
	}

	/// <summary>Occurs when the drop-down list of the combo box closes. </summary>
	public event EventHandler DropDownClosed
	{
		add
		{
			EventHandlersStoreAdd(DropDownClosedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(DropDownClosedKey, value);
		}
	}

	static ComboBox()
	{
		MaxDropDownHeightProperty = DependencyProperty.Register("MaxDropDownHeight", typeof(double), typeof(ComboBox), new FrameworkPropertyMetadata(SystemParameters.PrimaryScreenHeight / 3.0, Control.OnVisualStatePropertyChanged));
		IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(ComboBox), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDropDownOpenChanged, CoerceIsDropDownOpen));
		ShouldPreserveUserEnteredPrefixProperty = DependencyProperty.Register("ShouldPreserveUserEnteredPrefix", typeof(bool), typeof(ComboBox), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(ComboBox), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, OnIsEditableChanged));
		TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ComboBox), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnTextChanged));
		IsReadOnlyProperty = TextBoxBase.IsReadOnlyProperty.AddOwner(typeof(ComboBox));
		SelectionBoxItemPropertyKey = DependencyProperty.RegisterReadOnly("SelectionBoxItem", typeof(object), typeof(ComboBox), new FrameworkPropertyMetadata(string.Empty));
		SelectionBoxItemProperty = SelectionBoxItemPropertyKey.DependencyProperty;
		SelectionBoxItemTemplatePropertyKey = DependencyProperty.RegisterReadOnly("SelectionBoxItemTemplate", typeof(DataTemplate), typeof(ComboBox), new FrameworkPropertyMetadata((object)null));
		SelectionBoxItemTemplateProperty = SelectionBoxItemTemplatePropertyKey.DependencyProperty;
		SelectionBoxItemStringFormatPropertyKey = DependencyProperty.RegisterReadOnly("SelectionBoxItemStringFormat", typeof(string), typeof(ComboBox), new FrameworkPropertyMetadata((object)null));
		SelectionBoxItemStringFormatProperty = SelectionBoxItemStringFormatPropertyKey.DependencyProperty;
		StaysOpenOnEditProperty = DependencyProperty.Register("StaysOpenOnEdit", typeof(bool), typeof(ComboBox), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsSelectionBoxHighlightedPropertyKey = DependencyProperty.RegisterReadOnly("IsSelectionBoxHighlighted", typeof(bool), typeof(ComboBox), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, null, CoerceIsSelectionBoxHighlighted));
		IsSelectionBoxHighlightedProperty = IsSelectionBoxHighlightedPropertyKey.DependencyProperty;
		DropDownOpenedKey = new EventPrivateKey();
		DropDownClosedKey = new EventPrivateKey();
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(ComboBox), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
		KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(ComboBox), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(ComboBox), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
		ToolTipService.IsEnabledProperty.OverrideMetadata(typeof(ComboBox), new FrameworkPropertyMetadata(null, CoerceToolTipIsEnabled));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBox), new FrameworkPropertyMetadata(typeof(ComboBox)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(ComboBox));
		ItemsControl.IsTextSearchEnabledProperty.OverrideMetadata(typeof(ComboBox), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		EventManager.RegisterClassHandler(typeof(ComboBox), Mouse.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));
		EventManager.RegisterClassHandler(typeof(ComboBox), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseButtonDown), handledEventsToo: true);
		EventManager.RegisterClassHandler(typeof(ComboBox), Mouse.MouseMoveEvent, new MouseEventHandler(OnMouseMove));
		EventManager.RegisterClassHandler(typeof(ComboBox), Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseButtonDown));
		EventManager.RegisterClassHandler(typeof(ComboBox), Mouse.MouseWheelEvent, new MouseWheelEventHandler(OnMouseWheel), handledEventsToo: true);
		EventManager.RegisterClassHandler(typeof(ComboBox), UIElement.GotFocusEvent, new RoutedEventHandler(OnGotFocus));
		EventManager.RegisterClassHandler(typeof(ComboBox), ContextMenuService.ContextMenuOpeningEvent, new ContextMenuEventHandler(OnContextMenuOpen), handledEventsToo: true);
		EventManager.RegisterClassHandler(typeof(ComboBox), ContextMenuService.ContextMenuClosingEvent, new ContextMenuEventHandler(OnContextMenuClose), handledEventsToo: true);
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(ComboBox), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		UIElement.IsMouseOverPropertyKey.OverrideMetadata(typeof(ComboBox), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		Selector.IsSelectionActivePropertyKey.OverrideMetadata(typeof(ComboBox), new FrameworkPropertyMetadata(Control.OnVisualStatePropertyChanged));
		ControlsTraceLogger.AddControl(TelemetryControls.ComboBox);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ComboBox" /> class. </summary>
	public ComboBox()
	{
		Initialize();
	}

	private static object CoerceIsDropDownOpen(DependencyObject d, object value)
	{
		if ((bool)value)
		{
			ComboBox comboBox = (ComboBox)d;
			if (!comboBox.IsLoaded)
			{
				comboBox.RegisterToOpenOnLoad();
				return BooleanBoxes.FalseBox;
			}
		}
		return value;
	}

	private static object CoerceToolTipIsEnabled(DependencyObject d, object value)
	{
		if (!((ComboBox)d).IsDropDownOpen)
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
			CoerceValue(IsDropDownOpenProperty);
			return (object)null;
		}, null);
	}

	/// <summary>Reports when a combo box's popup opens. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Controls.ComboBox.DropDownOpened" /> event.</param>
	protected virtual void OnDropDownOpened(EventArgs e)
	{
		RaiseClrEvent(DropDownOpenedKey, e);
	}

	/// <summary>Reports when a combo box's popup closes. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Controls.ComboBox.DropDownClosed" /> event.</param>
	protected virtual void OnDropDownClosed(EventArgs e)
	{
		RaiseClrEvent(DropDownClosedKey, e);
	}

	private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ComboBox comboBox = (ComboBox)d;
		comboBox.HasMouseEnteredItemsHost = false;
		bool flag = (bool)e.NewValue;
		bool oldValue = !flag;
		if (UIElementAutomationPeer.FromElement(comboBox) is ComboBoxAutomationPeer comboBoxAutomationPeer)
		{
			comboBoxAutomationPeer.RaiseExpandCollapseAutomationEvent(oldValue, flag);
		}
		if (flag)
		{
			Mouse.Capture(comboBox, CaptureMode.SubTree);
			if (comboBox.IsEditable && comboBox.EditableTextBoxSite != null)
			{
				comboBox.EditableTextBoxSite.SelectAll();
			}
			if (comboBox._clonedElement != null && VisualTreeHelper.GetParent(comboBox._clonedElement) == null)
			{
				comboBox.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (DispatcherOperationCallback)delegate(object arg)
				{
					ComboBox comboBox2 = (ComboBox)arg;
					comboBox2.UpdateSelectionBoxItem();
					if (comboBox2._clonedElement != null)
					{
						comboBox2._clonedElement.CoerceValue(FrameworkElement.FlowDirectionProperty);
					}
					return (object)null;
				}, comboBox);
			}
			comboBox.Dispatcher.BeginInvoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate(object arg)
			{
				ComboBox comboBox3 = (ComboBox)arg;
				if (comboBox3.IsItemsHostVisible)
				{
					comboBox3.NavigateToItem(comboBox3.InternalSelectedInfo, ItemNavigateArgs.Empty, alwaysAtTopOfViewport: true);
				}
				return (object)null;
			}, comboBox);
			comboBox.OnDropDownOpened(EventArgs.Empty);
		}
		else
		{
			if (comboBox.IsKeyboardFocusWithin)
			{
				if (comboBox.IsEditable)
				{
					if (comboBox.EditableTextBoxSite != null && !comboBox.EditableTextBoxSite.IsKeyboardFocusWithin)
					{
						comboBox.Focus();
					}
				}
				else
				{
					comboBox.Focus();
				}
			}
			comboBox.HighlightedInfo = null;
			if (comboBox.HasCapture)
			{
				Mouse.Capture(null);
			}
			if (comboBox._dropDownPopup == null)
			{
				comboBox.OnDropDownClosed(EventArgs.Empty);
			}
		}
		comboBox.CoerceValue(IsSelectionBoxHighlightedProperty);
		comboBox.CoerceValue(ToolTipService.IsEnabledProperty);
		comboBox.UpdateVisualState();
	}

	private void OnPopupClosed(object source, EventArgs e)
	{
		OnDropDownClosed(EventArgs.Empty);
	}

	private static void OnIsEditableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ComboBox obj = d as ComboBox;
		obj.Update();
		obj.UpdateVisualState();
	}

	private static object CoerceIsSelectionBoxHighlighted(object o, object value)
	{
		ComboBox comboBox = (ComboBox)o;
		ComboBoxItem highlightedElement;
		return (!comboBox.IsDropDownOpen && comboBox.IsKeyboardFocusWithin) || ((highlightedElement = comboBox.HighlightedElement) != null && highlightedElement.Content == comboBox._clonedElement);
	}

	/// <summary>Responds to a <see cref="T:System.Windows.Controls.ComboBox" /> selection change by raising a <see cref="E:System.Windows.Controls.Primitives.Selector.SelectionChanged" /> event. </summary>
	/// <param name="e">Provides data for <see cref="T:System.Windows.Controls.SelectionChangedEventArgs" />. </param>
	protected override void OnSelectionChanged(SelectionChangedEventArgs e)
	{
		base.OnSelectionChanged(e);
		SelectedItemUpdated();
		if (IsDropDownOpen)
		{
			ItemInfo internalSelectedInfo = base.InternalSelectedInfo;
			if (internalSelectedInfo != null)
			{
				NavigateToItem(internalSelectedInfo, ItemNavigateArgs.Empty);
			}
		}
		if ((AutomationPeer.ListenerExists(AutomationEvents.SelectionPatternOnInvalidated) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementAddedToSelection) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection)) && UIElementAutomationPeer.CreatePeerForElement(this) is ComboBoxAutomationPeer comboBoxAutomationPeer)
		{
			comboBoxAutomationPeer.RaiseSelectionEvents(e);
		}
	}

	internal void SelectedItemUpdated()
	{
		try
		{
			UpdatingSelectedItem = true;
			if (!UpdatingText)
			{
				string primaryTextFromItem = TextSearch.GetPrimaryTextFromItem(this, base.InternalSelectedItem);
				if (Text != primaryTextFromItem)
				{
					SetCurrentValueInternal(TextProperty, primaryTextFromItem);
				}
			}
			Update();
		}
		finally
		{
			UpdatingSelectedItem = false;
		}
	}

	private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ComboBox obj = (ComboBox)d;
		if (UIElementAutomationPeer.FromElement(obj) is ComboBoxAutomationPeer comboBoxAutomationPeer)
		{
			comboBoxAutomationPeer.RaiseValuePropertyChangedEvent((string)e.OldValue, (string)e.NewValue);
		}
		obj.TextUpdated((string)e.NewValue, textBoxUpdated: false);
	}

	private void OnEditableTextBoxTextChanged(object sender, TextChangedEventArgs e)
	{
		if (IsEditable)
		{
			TextUpdated(EditableTextBoxSite.Text, textBoxUpdated: true);
		}
	}

	private void OnEditableTextBoxSelectionChanged(object sender, RoutedEventArgs e)
	{
		if (!Helper.IsComposing(EditableTextBoxSite))
		{
			_textBoxSelectionStart = EditableTextBoxSite.SelectionStart;
		}
	}

	private void OnEditableTextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
	{
		if (IsWaitingForTextComposition && e.TextComposition.Source == EditableTextBoxSite && e.TextComposition.Stage == TextCompositionStage.Done)
		{
			IsWaitingForTextComposition = false;
			TextUpdated(EditableTextBoxSite.Text, textBoxUpdated: true);
			EditableTextBoxSite.RaiseCourtesyTextChangedEvent();
		}
	}

	private void TextUpdated(string newText, bool textBoxUpdated)
	{
		if (UpdatingText || UpdatingSelectedItem)
		{
			return;
		}
		if (Helper.IsComposing(EditableTextBoxSite))
		{
			IsWaitingForTextComposition = true;
			return;
		}
		try
		{
			UpdatingText = true;
			if (base.IsTextSearchEnabled)
			{
				if (_updateTextBoxOperation != null)
				{
					_updateTextBoxOperation.Abort();
					_updateTextBoxOperation = null;
				}
				MatchedTextInfo matchedTextInfo = TextSearch.FindMatchingPrefix(this, newText);
				int num = matchedTextInfo.MatchedItemIndex;
				if (num >= 0)
				{
					if (textBoxUpdated)
					{
						int selectionStart = EditableTextBoxSite.SelectionStart;
						if (selectionStart == newText.Length && selectionStart > _textBoxSelectionStart)
						{
							string text = matchedTextInfo.MatchedText;
							if (ShouldPreserveUserEnteredPrefix)
							{
								text = string.Concat(newText, text.AsSpan(matchedTextInfo.MatchedPrefixLength));
							}
							UndoManager undoManager = EditableTextBoxSite.TextContainer.UndoManager;
							if (undoManager != null && undoManager.OpenedUnit != null && undoManager.OpenedUnit.GetType() != typeof(TextParentUndoUnit))
							{
								_updateTextBoxOperation = base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(UpdateTextBoxCallback), new object[2] { text, matchedTextInfo });
							}
							else
							{
								UpdateTextBox(text, matchedTextInfo);
							}
							newText = text;
						}
					}
					else
					{
						string matchedText = matchedTextInfo.MatchedText;
						if (!string.Equals(newText, matchedText, StringComparison.CurrentCulture))
						{
							num = -1;
						}
					}
				}
				if (num != base.SelectedIndex)
				{
					SetCurrentValueInternal(Selector.SelectedIndexProperty, num);
				}
			}
			if (textBoxUpdated)
			{
				SetCurrentValueInternal(TextProperty, newText);
			}
			else if (EditableTextBoxSite != null)
			{
				EditableTextBoxSite.Text = newText;
			}
		}
		finally
		{
			UpdatingText = false;
		}
	}

	private object UpdateTextBoxCallback(object arg)
	{
		_updateTextBoxOperation = null;
		object[] obj = (object[])arg;
		string matchedText = (string)obj[0];
		MatchedTextInfo matchedTextInfo = (MatchedTextInfo)obj[1];
		try
		{
			UpdatingText = true;
			UpdateTextBox(matchedText, matchedTextInfo);
		}
		finally
		{
			UpdatingText = false;
		}
		return null;
	}

	private void UpdateTextBox(string matchedText, MatchedTextInfo matchedTextInfo)
	{
		EditableTextBoxSite.Text = matchedText;
		EditableTextBoxSite.SelectionStart = matchedText.Length - matchedTextInfo.TextExcludingPrefixLength;
		EditableTextBoxSite.SelectionLength = matchedTextInfo.TextExcludingPrefixLength;
	}

	private void Update()
	{
		if (IsEditable)
		{
			UpdateEditableTextBox();
		}
		else
		{
			UpdateSelectionBoxItem();
		}
	}

	private void UpdateEditableTextBox()
	{
		if (UpdatingText)
		{
			return;
		}
		try
		{
			UpdatingText = true;
			string text = Text;
			if (EditableTextBoxSite != null && EditableTextBoxSite.Text != text)
			{
				EditableTextBoxSite.Text = text;
				EditableTextBoxSite.SelectAll();
			}
		}
		finally
		{
			UpdatingText = false;
		}
	}

	private void UpdateSelectionBoxItem()
	{
		object obj = base.InternalSelectedItem;
		DataTemplate dataTemplate = base.ItemTemplate;
		string text = base.ItemStringFormat;
		if (obj is ContentControl contentControl)
		{
			obj = contentControl.Content;
			dataTemplate = contentControl.ContentTemplate;
			text = contentControl.ContentStringFormat;
		}
		if (_clonedElement != null)
		{
			_clonedElement.LayoutUpdated -= CloneLayoutUpdated;
			_clonedElement = null;
		}
		if (dataTemplate == null && base.ItemTemplateSelector == null && text == null && obj is DependencyObject dependencyObject)
		{
			_clonedElement = dependencyObject as UIElement;
			if (_clonedElement != null)
			{
				VisualBrush visualBrush = new VisualBrush(_clonedElement);
				visualBrush.Stretch = Stretch.None;
				visualBrush.ViewboxUnits = BrushMappingMode.Absolute;
				visualBrush.Viewbox = new Rect(_clonedElement.RenderSize);
				visualBrush.ViewportUnits = BrushMappingMode.Absolute;
				visualBrush.Viewport = new Rect(_clonedElement.RenderSize);
				DependencyObject parent = VisualTreeHelper.GetParent(_clonedElement);
				FlowDirection flowDirection = ((parent != null) ? ((FlowDirection)parent.GetValue(FrameworkElement.FlowDirectionProperty)) : FlowDirection.LeftToRight);
				if (base.FlowDirection != flowDirection)
				{
					visualBrush.Transform = new MatrixTransform(new Matrix(-1.0, 0.0, 0.0, 1.0, _clonedElement.RenderSize.Width, 0.0));
				}
				Rectangle obj2 = new Rectangle
				{
					Fill = visualBrush,
					Width = _clonedElement.RenderSize.Width,
					Height = _clonedElement.RenderSize.Height
				};
				_clonedElement.LayoutUpdated += CloneLayoutUpdated;
				obj = obj2;
				dataTemplate = null;
			}
			else
			{
				obj = ExtractString(dependencyObject);
				dataTemplate = ContentPresenter.StringContentTemplate;
			}
		}
		if (obj == null)
		{
			obj = string.Empty;
			dataTemplate = ContentPresenter.StringContentTemplate;
		}
		SelectionBoxItem = obj;
		SelectionBoxItemTemplate = dataTemplate;
		SelectionBoxItemStringFormat = text;
	}

	private void CloneLayoutUpdated(object sender, EventArgs e)
	{
		Rectangle obj = (Rectangle)SelectionBoxItem;
		obj.Width = _clonedElement.RenderSize.Width;
		obj.Height = _clonedElement.RenderSize.Height;
		VisualBrush obj2 = (VisualBrush)obj.Fill;
		obj2.Viewbox = new Rect(_clonedElement.RenderSize);
		obj2.Viewport = new Rect(_clonedElement.RenderSize);
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (!base.IsEnabled)
		{
			VisualStateManager.GoToState(this, "Disabled", useTransitions);
		}
		else if (base.IsMouseOver)
		{
			VisualStateManager.GoToState(this, "MouseOver", useTransitions);
		}
		else
		{
			VisualStateManager.GoToState(this, "Normal", useTransitions);
		}
		if (!Selector.GetIsSelectionActive(this))
		{
			VisualStateManager.GoToState(this, "Unfocused", useTransitions);
		}
		else if (IsDropDownOpen)
		{
			VisualStateManager.GoToState(this, "FocusedDropDown", useTransitions);
		}
		else
		{
			VisualStateManager.GoToState(this, "Focused", useTransitions);
		}
		if (IsEditable)
		{
			VisualStateManager.GoToState(this, "Editable", useTransitions);
		}
		else
		{
			VisualStateManager.GoToState(this, "Uneditable", useTransitions);
		}
		base.ChangeVisualState(useTransitions);
	}

	/// <summary>Prepares the specified element to display the specified item. </summary>
	/// <param name="element">Element used to display the specified item.</param>
	/// <param name="item">Specified item.</param>
	protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		base.PrepareContainerForItemOverride(element, item);
		if (item is Separator)
		{
			Separator.PrepareContainer(element as Control);
		}
	}

	internal override void AdjustItemInfoOverride(NotifyCollectionChangedEventArgs e)
	{
		AdjustItemInfo(e, _highlightedInfo);
		base.AdjustItemInfoOverride(e);
	}

	private static void OnGotFocus(object sender, RoutedEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		if (!e.Handled && comboBox.IsEditable && comboBox.EditableTextBoxSite != null)
		{
			if (e.OriginalSource == comboBox)
			{
				comboBox.EditableTextBoxSite.Focus();
				e.Handled = true;
			}
			else if (e.OriginalSource == comboBox.EditableTextBoxSite)
			{
				comboBox.EditableTextBoxSite.SelectAll();
			}
		}
	}

	internal override bool FocusItem(ItemInfo info, ItemNavigateArgs itemNavigateArgs)
	{
		bool result = false;
		if (!IsEditable)
		{
			result = base.FocusItem(info, itemNavigateArgs);
		}
		ComboBoxItem comboBoxItem = info.Container as ComboBoxItem;
		HighlightedInfo = ((comboBoxItem != null) ? info : null);
		if ((IsEditable || !IsDropDownOpen) && itemNavigateArgs.DeviceUsed is KeyboardDevice)
		{
			int num = info.Index;
			if (num < 0)
			{
				num = base.Items.IndexOf(info.Item);
			}
			SetCurrentValueInternal(Selector.SelectedIndexProperty, num);
			result = true;
		}
		return result;
	}

	/// <summary>Reports that the <see cref="P:System.Windows.ContentElement.IsKeyboardFocusWithin" /> property changed. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.IsKeyboardFocusWithinChanged" /> event.</param>
	protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnIsKeyboardFocusWithinChanged(e);
		if (IsDropDownOpen && !base.IsKeyboardFocusWithin && (!(Keyboard.FocusedElement is DependencyObject container) || (!IsContextMenuOpen && ItemsControl.ItemsControlFromItemContainer(container) != this)))
		{
			Close();
		}
		CoerceValue(IsSelectionBoxHighlightedProperty);
	}

	private static void OnMouseWheel(object sender, MouseWheelEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		if (comboBox.IsKeyboardFocusWithin)
		{
			if (!comboBox.IsDropDownOpen)
			{
				if (e.Delta < 0)
				{
					comboBox.SelectNext();
				}
				else
				{
					comboBox.SelectPrev();
				}
			}
			e.Handled = true;
		}
		else if (comboBox.IsDropDownOpen)
		{
			e.Handled = true;
		}
	}

	private static void OnContextMenuOpen(object sender, ContextMenuEventArgs e)
	{
		((ComboBox)sender).IsContextMenuOpen = true;
	}

	private static void OnContextMenuClose(object sender, ContextMenuEventArgs e)
	{
		((ComboBox)sender).IsContextMenuOpen = false;
	}

	/// <summary>Called when the <see cref="P:System.Windows.UIElement.IsMouseCaptured" /> property changes. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.IsMouseCapturedChanged" /> event.</param>
	protected override void OnIsMouseCapturedChanged(DependencyPropertyChangedEventArgs e)
	{
		if (base.IsMouseCaptured)
		{
			if (_autoScrollTimer == null)
			{
				_autoScrollTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
				_autoScrollTimer.Interval = ItemsControl.AutoScrollTimeout;
				_autoScrollTimer.Tick += OnAutoScrollTimeout;
				_autoScrollTimer.Start();
			}
		}
		else if (_autoScrollTimer != null)
		{
			_autoScrollTimer.Stop();
			_autoScrollTimer = null;
		}
		base.OnIsMouseCapturedChanged(e);
	}

	internal void NotifyComboBoxItemMouseDown(ComboBoxItem comboBoxItem)
	{
	}

	internal void NotifyComboBoxItemMouseUp(ComboBoxItem comboBoxItem)
	{
		object obj = base.ItemContainerGenerator.ItemFromContainer(comboBoxItem);
		if (obj != null)
		{
			base.SelectionChange.SelectJustThisItem(NewItemInfo(obj, comboBoxItem), assumeInItemsCollection: true);
		}
		Close();
	}

	internal void NotifyComboBoxItemEnter(ComboBoxItem item)
	{
		if (IsDropDownOpen && Mouse.Captured == this && DidMouseMove())
		{
			HighlightedInfo = ItemInfoFromContainer(item);
			if (!IsEditable && !item.IsKeyboardFocusWithin)
			{
				item.Focus();
			}
		}
	}

	/// <summary>Determines if the specified item is (or is eligible to be) its own ItemContainer. </summary>
	/// <returns>true if the item is its own ItemContainer; otherwise, false.</returns>
	/// <param name="item">Specified item.</param>
	protected override bool IsItemItsOwnContainerOverride(object item)
	{
		return item is ComboBoxItem;
	}

	/// <summary>Creates or identifies the element used to display the specified item.</summary>
	/// <returns>The element used to display the specified item.</returns>
	protected override DependencyObject GetContainerForItemOverride()
	{
		return new ComboBoxItem();
	}

	private void Initialize()
	{
		base.CanSelectMultiple = false;
	}

	/// <summary>Invoked when a <see cref="E:System.Windows.Input.Keyboard.PreviewKeyDown" /> attached routed event occurs.</summary>
	/// <param name="e">Event data.</param>
	protected override void OnPreviewKeyDown(KeyEventArgs e)
	{
		if (IsEditable && e.OriginalSource == EditableTextBoxSite)
		{
			KeyDownHandler(e);
		}
	}

	/// <summary>Invoked when a <see cref="E:System.Windows.Input.Keyboard.KeyDown" /> attached routed event occurs.</summary>
	/// <param name="e">Event data.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		KeyDownHandler(e);
	}

	private void KeyDownHandler(KeyEventArgs e)
	{
		bool flag = false;
		Key key = e.Key;
		if (key == Key.System)
		{
			key = e.SystemKey;
		}
		bool flag2 = base.FlowDirection == FlowDirection.RightToLeft;
		switch (key)
		{
		case Key.Up:
			flag = true;
			if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
			{
				KeyboardToggleDropDown(commitSelection: true);
			}
			else if (IsItemsHostVisible)
			{
				NavigateByLine(HighlightedInfo, FocusNavigationDirection.Up, new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
			}
			else
			{
				SelectPrev();
			}
			break;
		case Key.Down:
			flag = true;
			if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
			{
				KeyboardToggleDropDown(commitSelection: true);
			}
			else if (IsItemsHostVisible)
			{
				NavigateByLine(HighlightedInfo, FocusNavigationDirection.Down, new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
			}
			else
			{
				SelectNext();
			}
			break;
		case Key.F4:
			if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == 0)
			{
				KeyboardToggleDropDown(commitSelection: true);
				flag = true;
			}
			break;
		case Key.Escape:
			if (IsDropDownOpen)
			{
				KeyboardCloseDropDown(commitSelection: false);
				flag = true;
			}
			break;
		case Key.Return:
			if (IsDropDownOpen)
			{
				KeyboardCloseDropDown(commitSelection: true);
				flag = true;
			}
			break;
		case Key.Home:
			if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) != ModifierKeys.Alt && !IsEditable)
			{
				if (IsItemsHostVisible)
				{
					NavigateToStart(new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
				}
				else
				{
					SelectFirst();
				}
				flag = true;
			}
			break;
		case Key.End:
			if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) != ModifierKeys.Alt && !IsEditable)
			{
				if (IsItemsHostVisible)
				{
					NavigateToEnd(new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
				}
				else
				{
					SelectLast();
				}
				flag = true;
			}
			break;
		case Key.Right:
			if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) != ModifierKeys.Alt && !IsEditable)
			{
				if (IsItemsHostVisible)
				{
					NavigateByLine(HighlightedInfo, FocusNavigationDirection.Right, new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
				}
				else if (!flag2)
				{
					SelectNext();
				}
				else
				{
					SelectPrev();
				}
				flag = true;
			}
			break;
		case Key.Left:
			if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) != ModifierKeys.Alt && !IsEditable)
			{
				if (IsItemsHostVisible)
				{
					NavigateByLine(HighlightedInfo, FocusNavigationDirection.Left, new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
				}
				else if (!flag2)
				{
					SelectPrev();
				}
				else
				{
					SelectNext();
				}
				flag = true;
			}
			break;
		case Key.Prior:
			if (IsItemsHostVisible)
			{
				NavigateByPage(HighlightedInfo, FocusNavigationDirection.Up, new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
				flag = true;
			}
			break;
		case Key.Next:
			if (IsItemsHostVisible)
			{
				NavigateByPage(HighlightedInfo, FocusNavigationDirection.Down, new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
				flag = true;
			}
			break;
		case Key.Oem5:
			if (Keyboard.Modifiers == ModifierKeys.Control)
			{
				NavigateToItem(base.InternalSelectedInfo, ItemNavigateArgs.Empty);
				flag = true;
			}
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			e.Handled = true;
		}
	}

	private void SelectPrev()
	{
		if (!base.Items.IsEmpty)
		{
			int internalSelectedIndex = base.InternalSelectedIndex;
			if (internalSelectedIndex > 0)
			{
				SelectItemHelper(internalSelectedIndex - 1, -1, -1);
			}
		}
	}

	private void SelectNext()
	{
		int count = base.Items.Count;
		if (count > 0)
		{
			int internalSelectedIndex = base.InternalSelectedIndex;
			if (internalSelectedIndex < count - 1)
			{
				SelectItemHelper(internalSelectedIndex + 1, 1, count);
			}
		}
	}

	private void SelectFirst()
	{
		SelectItemHelper(0, 1, base.Items.Count);
	}

	private void SelectLast()
	{
		SelectItemHelper(base.Items.Count - 1, -1, -1);
	}

	private void SelectItemHelper(int startIndex, int increment, int stopIndex)
	{
		for (int i = startIndex; i != stopIndex; i += increment)
		{
			object obj = base.Items[i];
			DependencyObject dependencyObject = base.ItemContainerGenerator.ContainerFromIndex(i);
			if (IsSelectableHelper(obj) && IsSelectableHelper(dependencyObject))
			{
				base.SelectionChange.SelectJustThisItem(NewItemInfo(obj, dependencyObject, i), assumeInItemsCollection: true);
				break;
			}
		}
	}

	private bool IsSelectableHelper(object o)
	{
		if (!(o is DependencyObject dependencyObject))
		{
			return true;
		}
		return (bool)dependencyObject.GetValue(UIElement.IsEnabledProperty);
	}

	private static string ExtractString(DependencyObject d)
	{
		string text = string.Empty;
		if (d is TextBlock textBlock)
		{
			text = textBlock.Text;
		}
		else if (d is Visual reference)
		{
			int childrenCount = VisualTreeHelper.GetChildrenCount(reference);
			for (int i = 0; i < childrenCount; i++)
			{
				text += ExtractString(VisualTreeHelper.GetChild(reference, i));
			}
		}
		else if (d is TextElement textElement)
		{
			text += TextRangeBase.GetTextInternal(textElement.ContentStart, textElement.ContentEnd);
		}
		return text;
	}

	/// <summary>Called when <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" /> is called.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		if (_dropDownPopup != null)
		{
			_dropDownPopup.Closed -= OnPopupClosed;
		}
		EditableTextBoxSite = GetTemplateChild("PART_EditableTextBox") as TextBox;
		_dropDownPopup = GetTemplateChild("PART_Popup") as Popup;
		if (EditableTextBoxSite != null)
		{
			EditableTextBoxSite.TextChanged += OnEditableTextBoxTextChanged;
			EditableTextBoxSite.SelectionChanged += OnEditableTextBoxSelectionChanged;
			EditableTextBoxSite.PreviewTextInput += OnEditableTextBoxPreviewTextInput;
		}
		if (_dropDownPopup != null)
		{
			_dropDownPopup.Closed += OnPopupClosed;
		}
		Update();
	}

	internal override void OnTemplateChangedInternal(FrameworkTemplate oldTemplate, FrameworkTemplate newTemplate)
	{
		base.OnTemplateChangedInternal(oldTemplate, newTemplate);
		if (EditableTextBoxSite != null)
		{
			EditableTextBoxSite.TextChanged -= OnEditableTextBoxTextChanged;
			EditableTextBoxSite.SelectionChanged -= OnEditableTextBoxSelectionChanged;
			EditableTextBoxSite.PreviewTextInput -= OnEditableTextBoxPreviewTextInput;
		}
	}

	private static void OnLostMouseCapture(object sender, MouseEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		if (Mouse.Captured == comboBox)
		{
			return;
		}
		if (e.OriginalSource == comboBox)
		{
			if (Mouse.Captured == null || !MenuBase.IsDescendant(comboBox, Mouse.Captured as DependencyObject))
			{
				comboBox.Close();
			}
		}
		else if (MenuBase.IsDescendant(comboBox, e.OriginalSource as DependencyObject))
		{
			if (comboBox.IsDropDownOpen && Mouse.Captured == null && SafeNativeMethods.GetCapture() == IntPtr.Zero)
			{
				Mouse.Capture(comboBox, CaptureMode.SubTree);
				e.Handled = true;
			}
		}
		else
		{
			comboBox.Close();
		}
	}

	private static void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		if (!comboBox.IsContextMenuOpen && !comboBox.IsKeyboardFocusWithin)
		{
			comboBox.Focus();
		}
		e.Handled = true;
		if (Mouse.Captured == comboBox && e.OriginalSource == comboBox)
		{
			comboBox.Close();
		}
	}

	private static void OnPreviewMouseButtonDown(object sender, MouseButtonEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		if (!comboBox.IsEditable)
		{
			return;
		}
		Visual visual = e.OriginalSource as Visual;
		Visual editableTextBoxSite = comboBox.EditableTextBoxSite;
		if (visual != null && editableTextBoxSite != null && editableTextBoxSite.IsAncestorOf(visual))
		{
			if (comboBox.IsDropDownOpen && !comboBox.StaysOpenOnEdit)
			{
				comboBox.Close();
			}
			else if (!comboBox.IsContextMenuOpen && !comboBox.IsKeyboardFocusWithin)
			{
				comboBox.Focus();
				e.Handled = true;
			}
		}
	}

	/// <summary>Called to report that the left mouse button was released. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.MouseLeftButtonUp" /> event.</param>
	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		if (HasMouseEnteredItemsHost && !IsMouseOverItemsHost && IsDropDownOpen)
		{
			Close();
			e.Handled = true;
		}
		base.OnMouseLeftButtonUp(e);
	}

	private static void OnMouseMove(object sender, MouseEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		if (comboBox.IsDropDownOpen)
		{
			bool flag = comboBox.ItemsHost != null && comboBox.ItemsHost.IsMouseOver;
			if (flag && !comboBox.HasMouseEnteredItemsHost)
			{
				comboBox.SetInitialMousePosition();
			}
			comboBox.IsMouseOverItemsHost = flag;
			comboBox.HasMouseEnteredItemsHost |= flag;
		}
		if (Mouse.LeftButton == MouseButtonState.Pressed && comboBox.HasMouseEnteredItemsHost && Mouse.Captured == comboBox)
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed)
			{
				comboBox.DoAutoScroll(comboBox.HighlightedInfo);
			}
			else
			{
				comboBox.ReleaseMouseCapture();
				comboBox.ResetLastMousePosition();
			}
			e.Handled = true;
		}
	}

	private void KeyboardToggleDropDown(bool commitSelection)
	{
		KeyboardToggleDropDown(!IsDropDownOpen, commitSelection);
	}

	private void KeyboardCloseDropDown(bool commitSelection)
	{
		KeyboardToggleDropDown(openDropDown: false, commitSelection);
	}

	private void KeyboardToggleDropDown(bool openDropDown, bool commitSelection)
	{
		ItemInfo itemInfo = null;
		if (commitSelection)
		{
			itemInfo = HighlightedInfo;
		}
		SetCurrentValueInternal(IsDropDownOpenProperty, BooleanBoxes.Box(openDropDown));
		if (!openDropDown && commitSelection && itemInfo != null)
		{
			base.SelectionChange.SelectJustThisItem(itemInfo, assumeInItemsCollection: true);
		}
	}

	private void CommitSelection()
	{
		ItemInfo highlightedInfo = HighlightedInfo;
		if (highlightedInfo != null)
		{
			base.SelectionChange.SelectJustThisItem(highlightedInfo, assumeInItemsCollection: true);
		}
	}

	private void OnAutoScrollTimeout(object sender, EventArgs e)
	{
		if (Mouse.LeftButton == MouseButtonState.Pressed && HasMouseEnteredItemsHost)
		{
			DoAutoScroll(HighlightedInfo);
		}
	}

	private void Close()
	{
		if (IsDropDownOpen)
		{
			SetCurrentValueInternal(IsDropDownOpenProperty, false);
		}
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.ComboBoxAutomationPeer" /> implementation for this control, as part of the WPF infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new ComboBoxAutomationPeer(this);
	}
}
