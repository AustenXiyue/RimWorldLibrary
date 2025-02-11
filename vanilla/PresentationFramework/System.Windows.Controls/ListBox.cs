using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Commands;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Contains a list of selectable items. </summary>
[Localizability(LocalizationCategory.ListBox)]
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ListBoxItem))]
public class ListBox : Selector
{
	internal const string ListBoxSelectAllKey = "Ctrl+A";

	private static readonly bool OptOutOfGridColumnResizeUsingKeyboard;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ListBox.SelectionMode" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ListBox.SelectionMode" /> dependency property.</returns>
	public static readonly DependencyProperty SelectionModeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ListBox.SelectedItems" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ListBox.SelectedItems" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedItemsProperty;

	private ItemInfo _anchorItem;

	private WeakReference _lastActionItem;

	private DispatcherTimer _autoScrollTimer;

	private const double ColumnWidthStepSize = 10.0;

	private static RoutedUICommand SelectAllCommand;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets the selection behavior for a <see cref="T:System.Windows.Controls.ListBox" />.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.Controls.SelectionMode" /> values. The default is <see cref="F:System.Windows.Controls.SelectionMode.Single" /> selection. </returns>
	public SelectionMode SelectionMode
	{
		get
		{
			return (SelectionMode)GetValue(SelectionModeProperty);
		}
		set
		{
			SetValue(SelectionModeProperty, value);
		}
	}

	/// <summary>Gets the currently selected items.  </summary>
	/// <returns>Returns a collection of the currently selected items.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.ListBox.SelectionMode" /> property is set to <see cref="F:System.Windows.Controls.SelectionMode.Single" />.</exception>
	[Bindable(true)]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IList SelectedItems => base.SelectedItemsImpl;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.ListBox" /> supports scrolling.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.ListBox" /> supports scrolling; otherwise, false.</returns>
	protected internal override bool HandlesScrolling => true;

	/// <summary>Gets or sets the item that is initially selected when <see cref="P:System.Windows.Controls.ListBox.SelectionMode" /> is <see cref="F:System.Windows.Controls.SelectionMode.Extended" />.</summary>
	/// <returns>The item that is initially selected when <see cref="P:System.Windows.Controls.ListBox.SelectionMode" /> is <see cref="F:System.Windows.Controls.SelectionMode.Extended" />.</returns>
	protected object AnchorItem
	{
		get
		{
			return AnchorItemInternal;
		}
		set
		{
			if (value != null && value != DependencyProperty.UnsetValue)
			{
				ItemInfo itemInfo = NewItemInfo(value);
				if (!(itemInfo.Container is ListBoxItem lastActionItem))
				{
					throw new InvalidOperationException(SR.Format(SR.ListBoxInvalidAnchorItem, value));
				}
				AnchorItemInternal = itemInfo;
				LastActionItem = lastActionItem;
			}
			else
			{
				AnchorItemInternal = null;
				LastActionItem = null;
			}
		}
	}

	internal ItemInfo AnchorItemInternal
	{
		get
		{
			return _anchorItem;
		}
		set
		{
			_anchorItem = ((value != null) ? value.Clone() : null);
		}
	}

	internal ListBoxItem LastActionItem
	{
		get
		{
			return GetWeakReferenceTarget(ref _lastActionItem) as ListBoxItem;
		}
		set
		{
			_lastActionItem = new WeakReference(value);
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ListBox" /> class. </summary>
	public ListBox()
	{
		Initialize();
	}

	private void Initialize()
	{
		SelectionMode mode = (SelectionMode)SelectionModeProperty.GetDefaultValue(base.DependencyObjectType);
		ValidateSelectionMode(mode);
	}

	static ListBox()
	{
		SelectionModeProperty = DependencyProperty.Register("SelectionMode", typeof(SelectionMode), typeof(ListBox), new FrameworkPropertyMetadata(SelectionMode.Single, OnSelectionModeChanged), IsValidSelectionMode);
		SelectedItemsProperty = Selector.SelectedItemsImplProperty;
		SelectAllCommand = new RoutedUICommand(SR.ListBoxSelectAllText, "SelectAll", typeof(ListBox));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ListBox), new FrameworkPropertyMetadata(typeof(ListBox)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(ListBox));
		Control.IsTabStopProperty.OverrideMetadata(typeof(ListBox), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(ListBox), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(ListBox), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
		ItemsControl.IsTextSearchEnabledProperty.OverrideMetadata(typeof(ListBox), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		ItemsPanelTemplate itemsPanelTemplate = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(VirtualizingStackPanel)));
		itemsPanelTemplate.Seal();
		ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(ListBox), new FrameworkPropertyMetadata(itemsPanelTemplate));
		EventManager.RegisterClassHandler(typeof(ListBox), Mouse.MouseUpEvent, new MouseButtonEventHandler(OnMouseButtonUp), handledEventsToo: true);
		EventManager.RegisterClassHandler(typeof(ListBox), Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus));
		CommandHelpers.RegisterCommandHandler(typeof(ListBox), SelectAllCommand, OnSelectAll, OnQueryStatusSelectAll, KeyGesture.CreateFromResourceStrings("Ctrl+A", SR.ListBoxSelectAllKeyDisplayString));
		ControlsTraceLogger.AddControl(TelemetryControls.ListBox);
		AppContext.TryGetSwitch("System.Windows.Controls.OptOutOfGridColumnResizeUsingKeyboard", out OptOutOfGridColumnResizeUsingKeyboard);
	}

	/// <summary>Selects all the items in a <see cref="T:System.Windows.Controls.ListBox" />. </summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="P:System.Windows.Controls.ListBox.SelectionMode" /> property is set to <see cref="F:System.Windows.Controls.SelectionMode.Single" />.</exception>
	public void SelectAll()
	{
		if (base.CanSelectMultiple)
		{
			SelectAllImpl();
			return;
		}
		throw new NotSupportedException(SR.ListBoxSelectAllSelectionMode);
	}

	/// <summary>Clears all the selection in a <see cref="T:System.Windows.Controls.ListBox" />. </summary>
	public void UnselectAll()
	{
		UnselectAllImpl();
	}

	/// <summary>Causes the object to scroll into view. </summary>
	/// <param name="item">Object to scroll.</param>
	public void ScrollIntoView(object item)
	{
		if (base.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
		{
			OnBringItemIntoView(item);
		}
		else
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(base.OnBringItemIntoView), item);
		}
	}

	private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ListBox obj = (ListBox)d;
		obj.ValidateSelectionMode(obj.SelectionMode);
	}

	private static object OnGetSelectionMode(DependencyObject d)
	{
		return ((ListBox)d).SelectionMode;
	}

	private static bool IsValidSelectionMode(object o)
	{
		SelectionMode selectionMode = (SelectionMode)o;
		if (selectionMode != 0 && selectionMode != SelectionMode.Multiple)
		{
			return selectionMode == SelectionMode.Extended;
		}
		return true;
	}

	private void ValidateSelectionMode(SelectionMode mode)
	{
		base.CanSelectMultiple = mode != SelectionMode.Single;
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.ListBoxAutomationPeer" /> implementation for this control, as part of the WPF automation infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new ListBoxAutomationPeer(this);
	}

	/// <summary>Sets a collection of selected items. </summary>
	/// <returns>true if all items have been selected; otherwise, false.</returns>
	/// <param name="selectedItems">Collection of items to be selected.</param>
	protected bool SetSelectedItems(IEnumerable selectedItems)
	{
		return SetSelectedItemsImpl(selectedItems);
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
		AdjustItemInfo(e, _anchorItem);
		if (_anchorItem != null && _anchorItem.Index < 0)
		{
			_anchorItem = null;
		}
		base.AdjustItemInfoOverride(e);
	}

	internal override void AdjustItemInfosAfterGeneratorChangeOverride()
	{
		AdjustItemInfoAfterGeneratorChange(_anchorItem);
		base.AdjustItemInfosAfterGeneratorChangeOverride();
	}

	/// <summary>Responds to a list box selection change by raising a <see cref="E:System.Windows.Controls.Primitives.Selector.SelectionChanged" /> event. </summary>
	/// <param name="e">Provides data for <see cref="T:System.Windows.Controls.SelectionChangedEventArgs" />. </param>
	protected override void OnSelectionChanged(SelectionChangedEventArgs e)
	{
		base.OnSelectionChanged(e);
		if (SelectionMode == SelectionMode.Single)
		{
			ItemInfo internalSelectedInfo = base.InternalSelectedInfo;
			if (((internalSelectedInfo != null) ? (internalSelectedInfo.Container as ListBoxItem) : null) != null)
			{
				UpdateAnchorAndActionItem(internalSelectedInfo);
			}
		}
		if ((AutomationPeer.ListenerExists(AutomationEvents.SelectionPatternOnInvalidated) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementAddedToSelection) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection)) && UIElementAutomationPeer.CreatePeerForElement(this) is ListBoxAutomationPeer listBoxAutomationPeer)
		{
			listBoxAutomationPeer.RaiseSelectionEvents(e);
		}
	}

	/// <summary>Responds to the <see cref="E:System.Windows.UIElement.KeyDown" /> event. </summary>
	/// <param name="e">Provides data for <see cref="T:System.Windows.Input.KeyEventArgs" />.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		bool flag = true;
		Key key = e.Key;
		switch (key)
		{
		case Key.Divide:
		case Key.Oem2:
			if (Keyboard.Modifiers == ModifierKeys.Control && SelectionMode == SelectionMode.Extended)
			{
				SelectAll();
			}
			else
			{
				flag = false;
			}
			break;
		case Key.Oem5:
			if (Keyboard.Modifiers == ModifierKeys.Control && SelectionMode == SelectionMode.Extended)
			{
				ListBoxItem listBoxItem = ((base.FocusedInfo != null) ? (base.FocusedInfo.Container as ListBoxItem) : null);
				if (listBoxItem != null)
				{
					MakeSingleSelection(listBoxItem);
				}
			}
			else
			{
				flag = false;
			}
			break;
		case Key.Left:
		case Key.Up:
		case Key.Right:
		case Key.Down:
		{
			KeyboardNavigation.ShowFocusVisual();
			bool flag2 = base.ScrollHost != null;
			if (flag2)
			{
				flag2 = (key == Key.Down && base.IsLogicalHorizontal && DoubleUtil.GreaterThan(base.ScrollHost.ScrollableHeight, base.ScrollHost.VerticalOffset)) || (key == Key.Up && base.IsLogicalHorizontal && DoubleUtil.GreaterThanZero(base.ScrollHost.VerticalOffset)) || (key == Key.Right && base.IsLogicalVertical && DoubleUtil.GreaterThan(base.ScrollHost.ScrollableWidth, base.ScrollHost.HorizontalOffset)) || (key == Key.Left && base.IsLogicalVertical && DoubleUtil.GreaterThanZero(base.ScrollHost.HorizontalOffset));
			}
			if (flag2)
			{
				base.ScrollHost.ScrollInDirection(e);
			}
			else if ((base.ItemsHost != null && base.ItemsHost.IsKeyboardFocusWithin) || base.IsKeyboardFocused)
			{
				if (!NavigateByLine(KeyboardNavigation.KeyToTraversalDirection(key), new ItemNavigateArgs(e.Device, Keyboard.Modifiers)))
				{
					flag = false;
				}
			}
			else
			{
				flag = false;
			}
			break;
		}
		case Key.Home:
			NavigateToStart(new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
			break;
		case Key.End:
			NavigateToEnd(new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
			break;
		case Key.Return:
		case Key.Space:
		{
			if (e.Key == Key.Return && !(bool)GetValue(KeyboardNavigation.AcceptsReturnProperty))
			{
				flag = false;
				break;
			}
			ListBoxItem listBoxItem2 = e.OriginalSource as ListBoxItem;
			if ((Keyboard.Modifiers & (ModifierKeys.Alt | ModifierKeys.Control)) == ModifierKeys.Alt)
			{
				flag = false;
				break;
			}
			if (base.IsTextSearchEnabled && Keyboard.Modifiers == ModifierKeys.None)
			{
				TextSearch textSearch = TextSearch.EnsureInstance(this);
				if (textSearch != null && textSearch.GetCurrentPrefix() != string.Empty)
				{
					flag = false;
					break;
				}
			}
			if (listBoxItem2 != null && ItemsControl.ItemsControlFromItemContainer(listBoxItem2) == this)
			{
				switch (SelectionMode)
				{
				case SelectionMode.Single:
					if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
					{
						MakeToggleSelection(listBoxItem2);
					}
					else
					{
						MakeSingleSelection(listBoxItem2);
					}
					break;
				case SelectionMode.Multiple:
					MakeToggleSelection(listBoxItem2);
					break;
				case SelectionMode.Extended:
					if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == ModifierKeys.Control)
					{
						MakeToggleSelection(listBoxItem2);
					}
					else if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == ModifierKeys.Shift)
					{
						MakeAnchorSelection(listBoxItem2, clearCurrent: true);
					}
					else if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
					{
						MakeSingleSelection(listBoxItem2);
					}
					else
					{
						flag = false;
					}
					break;
				}
			}
			else
			{
				flag = false;
			}
			break;
		}
		case Key.Prior:
			NavigateByPage(FocusNavigationDirection.Up, new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
			break;
		case Key.Next:
			NavigateByPage(FocusNavigationDirection.Down, new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
			break;
		case Key.System:
		{
			if (OptOutOfGridColumnResizeUsingKeyboard)
			{
				flag = false;
				break;
			}
			Key systemKey = e.SystemKey;
			if (systemKey == Key.Left || systemKey == Key.Right)
			{
				if ((Keyboard.Modifiers & (ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Windows)) == ModifierKeys.Alt && e.OriginalSource is GridViewColumnHeader { Column: not null } gridViewColumnHeader)
				{
					double num = 0.0;
					if (e.SystemKey == Key.Left)
					{
						num = gridViewColumnHeader.Column.ActualWidth - 10.0;
					}
					else if (e.SystemKey == Key.Right)
					{
						num = gridViewColumnHeader.Column.ActualWidth + 10.0;
					}
					if (num > 0.0)
					{
						gridViewColumnHeader.UpdateColumnHeaderWidth(num);
					}
				}
			}
			else
			{
				flag = false;
			}
			break;
		}
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			e.Handled = true;
		}
		else
		{
			base.OnKeyDown(e);
		}
	}

	/// <summary>Called when a <see cref="T:System.Windows.Controls.ListBox" /> reports a mouse move. </summary>
	/// <param name="e">Provides data for <see cref="T:System.Windows.Input.MouseEventArgs" />.</param>
	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (e.OriginalSource == this && Mouse.Captured == this)
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed)
			{
				DoAutoScroll();
			}
			else
			{
				ReleaseMouseCapture();
				ResetLastMousePosition();
			}
		}
		base.OnMouseMove(e);
	}

	private static void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			ListBox obj = (ListBox)sender;
			obj.ReleaseMouseCapture();
			obj.ResetLastMousePosition();
		}
	}

	private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		ListBox listBox = (ListBox)sender;
		if (KeyboardNavigation.IsKeyboardMostRecentInputDevice() && e.NewFocus is ListBoxItem listBoxItem && ItemsControl.ItemsControlFromItemContainer(listBoxItem) == listBox)
		{
			DependencyObject dependencyObject = e.OldFocus as DependencyObject;
			Visual visual = dependencyObject as Visual;
			if (visual == null && dependencyObject is ContentElement ce)
			{
				visual = KeyboardNavigation.GetParentUIElementFromContentElement(ce);
			}
			if ((visual != null && listBox.IsAncestorOf(visual)) || dependencyObject == listBox)
			{
				listBox.LastActionItem = listBoxItem;
				listBox.MakeKeyboardSelection(listBoxItem);
			}
		}
	}

	/// <summary>Called when the <see cref="P:System.Windows.UIElement.IsMouseCaptured" /> property changes. </summary>
	/// <param name="e">Provides data for the <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" />.</param>
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

	/// <summary>Determines if the specified item is (or is eligible to be) its own ItemContainer. </summary>
	/// <returns>true if the item is its own ItemContainer; otherwise, false.</returns>
	/// <param name="item">Specified item.</param>
	protected override bool IsItemItsOwnContainerOverride(object item)
	{
		return item is ListBoxItem;
	}

	/// <summary>Creates or identifies the element used to display a specified item. </summary>
	/// <returns>The element used to display a specified item.</returns>
	protected override DependencyObject GetContainerForItemOverride()
	{
		return new ListBoxItem();
	}

	private static void OnQueryStatusSelectAll(object target, CanExecuteRoutedEventArgs args)
	{
		if ((target as ListBox).SelectionMode == SelectionMode.Extended)
		{
			args.CanExecute = true;
		}
	}

	private static void OnSelectAll(object target, ExecutedRoutedEventArgs args)
	{
		ListBox listBox = target as ListBox;
		if (listBox.SelectionMode == SelectionMode.Extended)
		{
			listBox.SelectAll();
		}
	}

	internal void NotifyListItemClicked(ListBoxItem item, MouseButton mouseButton)
	{
		if (mouseButton == MouseButton.Left && Mouse.Captured != this)
		{
			Mouse.Capture(this, CaptureMode.SubTree);
			SetInitialMousePosition();
		}
		switch (SelectionMode)
		{
		case SelectionMode.Single:
			if (!item.IsSelected)
			{
				item.SetCurrentValueInternal(Selector.IsSelectedProperty, BooleanBoxes.TrueBox);
			}
			else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
			{
				item.SetCurrentValueInternal(Selector.IsSelectedProperty, BooleanBoxes.FalseBox);
			}
			UpdateAnchorAndActionItem(ItemInfoFromContainer(item));
			break;
		case SelectionMode.Multiple:
			MakeToggleSelection(item);
			break;
		case SelectionMode.Extended:
			switch (mouseButton)
			{
			case MouseButton.Left:
				if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
				{
					MakeAnchorSelection(item, clearCurrent: false);
				}
				else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
				{
					MakeToggleSelection(item);
				}
				else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
				{
					MakeAnchorSelection(item, clearCurrent: true);
				}
				else
				{
					MakeSingleSelection(item);
				}
				break;
			case MouseButton.Right:
				if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == 0)
				{
					if (item.IsSelected)
					{
						UpdateAnchorAndActionItem(ItemInfoFromContainer(item));
					}
					else
					{
						MakeSingleSelection(item);
					}
				}
				break;
			}
			break;
		}
	}

	internal void NotifyListItemMouseDragged(ListBoxItem listItem)
	{
		if (Mouse.Captured == this && DidMouseMove())
		{
			NavigateToItem(ItemInfoFromContainer(listItem), new ItemNavigateArgs(Mouse.PrimaryDevice, Keyboard.Modifiers));
		}
	}

	private void UpdateAnchorAndActionItem(ItemInfo info)
	{
		object item = info.Item;
		ListBoxItem listBoxItem = info.Container as ListBoxItem;
		if (item == DependencyProperty.UnsetValue)
		{
			AnchorItemInternal = null;
			LastActionItem = null;
		}
		else
		{
			AnchorItemInternal = info;
			LastActionItem = listBoxItem;
		}
		KeyboardNavigation.SetTabOnceActiveElement(this, listBoxItem);
	}

	private void MakeSingleSelection(ListBoxItem listItem)
	{
		if (ItemsControl.ItemsControlFromItemContainer(listItem) == this)
		{
			ItemInfo info = ItemInfoFromContainer(listItem);
			base.SelectionChange.SelectJustThisItem(info, assumeInItemsCollection: true);
			listItem.Focus();
			UpdateAnchorAndActionItem(info);
		}
	}

	private void MakeToggleSelection(ListBoxItem item)
	{
		bool value = !item.IsSelected;
		item.SetCurrentValueInternal(Selector.IsSelectedProperty, BooleanBoxes.Box(value));
		UpdateAnchorAndActionItem(ItemInfoFromContainer(item));
	}

	private void MakeAnchorSelection(ListBoxItem actionItem, bool clearCurrent)
	{
		ItemInfo anchorItemInternal = AnchorItemInternal;
		if (anchorItemInternal == null)
		{
			if (_selectedItems.Count > 0)
			{
				AnchorItemInternal = _selectedItems[_selectedItems.Count - 1];
			}
			else
			{
				AnchorItemInternal = NewItemInfo(base.Items[0], null, 0);
			}
			if ((anchorItemInternal = AnchorItemInternal) == null)
			{
				return;
			}
		}
		int num = ElementIndex(actionItem);
		int num2 = AnchorItemInternal.Index;
		if (num > num2)
		{
			int num3 = num;
			num = num2;
			num2 = num3;
		}
		bool flag = false;
		if (!base.SelectionChange.IsActive)
		{
			flag = true;
			base.SelectionChange.Begin();
		}
		try
		{
			if (clearCurrent)
			{
				for (int i = 0; i < _selectedItems.Count; i++)
				{
					ItemInfo itemInfo = _selectedItems[i];
					int index = itemInfo.Index;
					if (index < num || num2 < index)
					{
						base.SelectionChange.Unselect(itemInfo);
					}
				}
			}
			IEnumerator enumerator = ((IEnumerable)base.Items).GetEnumerator();
			for (int j = 0; j <= num2; j++)
			{
				enumerator.MoveNext();
				if (j >= num)
				{
					base.SelectionChange.Select(NewItemInfo(enumerator.Current, null, j), assumeInItemsCollection: true);
				}
			}
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		finally
		{
			if (flag)
			{
				base.SelectionChange.End();
			}
		}
		LastActionItem = actionItem;
		GC.KeepAlive(anchorItemInternal);
	}

	private void MakeKeyboardSelection(ListBoxItem item)
	{
		if (item == null)
		{
			return;
		}
		switch (SelectionMode)
		{
		case SelectionMode.Single:
			if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
			{
				MakeSingleSelection(item);
			}
			break;
		case SelectionMode.Multiple:
			UpdateAnchorAndActionItem(ItemInfoFromContainer(item));
			break;
		case SelectionMode.Extended:
			if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
			{
				bool clearCurrent = (Keyboard.Modifiers & ModifierKeys.Control) == 0;
				MakeAnchorSelection(item, clearCurrent);
			}
			else if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
			{
				MakeSingleSelection(item);
			}
			break;
		}
	}

	private int ElementIndex(ListBoxItem listItem)
	{
		return base.ItemContainerGenerator.IndexFromContainer(listItem);
	}

	private ListBoxItem ElementAt(int index)
	{
		return base.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
	}

	private object GetWeakReferenceTarget(ref WeakReference weakReference)
	{
		if (weakReference != null)
		{
			return weakReference.Target;
		}
		return null;
	}

	private void OnAutoScrollTimeout(object sender, EventArgs e)
	{
		if (Mouse.LeftButton == MouseButtonState.Pressed)
		{
			DoAutoScroll();
		}
	}

	internal override bool FocusItem(ItemInfo info, ItemNavigateArgs itemNavigateArgs)
	{
		bool result = base.FocusItem(info, itemNavigateArgs);
		if (info.Container is ListBoxItem listBoxItem)
		{
			LastActionItem = listBoxItem;
			MakeKeyboardSelection(listBoxItem);
		}
		return result;
	}
}
