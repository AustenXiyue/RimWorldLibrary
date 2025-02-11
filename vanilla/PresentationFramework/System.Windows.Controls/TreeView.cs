using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Data;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a control that displays hierarchical data in a tree structure that has items that can expand and collapse. </summary>
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(TreeViewItem))]
public class TreeView : ItemsControl
{
	private enum Bits
	{
		IsSelectionChangeActive = 1
	}

	private static readonly DependencyPropertyKey SelectedItemPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TreeView.SelectedItem" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TreeView.SelectedItem" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedItemProperty;

	private static readonly DependencyPropertyKey SelectedValuePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TreeView.SelectedValue" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="F:System.Windows.Controls.TreeView.SelectedValuePathProperty" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedValueProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TreeView.SelectedValuePath" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TreeView.SelectedValuePath" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedValuePathProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.TreeView.SelectedItemChanged" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.TreeView.SelectedItemChanged" /> routed event.</returns>
	public static readonly RoutedEvent SelectedItemChangedEvent;

	private static DependencyObjectType _dType;

	private BitVector32 _bits = new BitVector32(0);

	private TreeViewItem _selectedContainer;

	private static readonly BindingExpressionUncommonField SelectedValuePathBindingExpression;

	private EventHandler _focusEnterMainFocusScopeEventHandler;

	/// <summary>Gets the selected item in a <see cref="T:System.Windows.Controls.TreeView" />.  </summary>
	/// <returns>The selected object in the <see cref="T:System.Windows.Controls.TreeView" />, or null if no item is selected. The default value is null.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	[ReadOnly(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public object SelectedItem => GetValue(SelectedItemProperty);

	/// <summary>Gets the value of the property that is the specified by <see cref="P:System.Windows.Controls.TreeView.SelectedValuePath" /> for the <see cref="P:System.Windows.Controls.TreeView.SelectedItem" />.   </summary>
	/// <returns>The value of the property that is specified by the <see cref="P:System.Windows.Controls.TreeView.SelectedValuePath" /> for the <see cref="P:System.Windows.Controls.TreeView.SelectedItem" />, or null if no item is selected. The default value is null.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	[ReadOnly(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public object SelectedValue => GetValue(SelectedValueProperty);

	/// <summary>Gets or sets the path that is used to get the <see cref="P:System.Windows.Controls.TreeView.SelectedValue" /> of the <see cref="P:System.Windows.Controls.TreeView.SelectedItem" /> in a <see cref="T:System.Windows.Controls.TreeView" />.  </summary>
	/// <returns>A string that contains the path that is used to get the <see cref="P:System.Windows.Controls.TreeView.SelectedValue" />. The default value is String.Empty.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public string SelectedValuePath
	{
		get
		{
			return (string)GetValue(SelectedValuePathProperty);
		}
		set
		{
			SetValue(SelectedValuePathProperty, value);
		}
	}

	internal bool IsSelectionChangeActive
	{
		get
		{
			return _bits[1];
		}
		set
		{
			_bits[1] = value;
		}
	}

	internal bool IsSelectedContainerHookedUp
	{
		get
		{
			if (_selectedContainer != null)
			{
				return _selectedContainer.ParentTreeView == this;
			}
			return false;
		}
	}

	internal TreeViewItem SelectedContainer => _selectedContainer;

	/// <summary>Gets whether the <see cref="T:System.Windows.Controls.TreeView" /> can scroll.</summary>
	/// <returns>Always returns true because the control has a <see cref="T:System.Windows.Controls.ScrollViewer" /> in its style.</returns>
	protected internal override bool HandlesScrolling => true;

	private static bool IsControlKeyDown => (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

	private static bool IsShiftKeyDown => (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.TreeView.SelectedItem" /> changes.</summary>
	[Category("Behavior")]
	public event RoutedPropertyChangedEventHandler<object> SelectedItemChanged
	{
		add
		{
			AddHandler(SelectedItemChangedEvent, value);
		}
		remove
		{
			RemoveHandler(SelectedItemChangedEvent, value);
		}
	}

	static TreeView()
	{
		SelectedItemPropertyKey = DependencyProperty.RegisterReadOnly("SelectedItem", typeof(object), typeof(TreeView), new FrameworkPropertyMetadata((object)null));
		SelectedItemProperty = SelectedItemPropertyKey.DependencyProperty;
		SelectedValuePropertyKey = DependencyProperty.RegisterReadOnly("SelectedValue", typeof(object), typeof(TreeView), new FrameworkPropertyMetadata((object)null));
		SelectedValueProperty = SelectedValuePropertyKey.DependencyProperty;
		SelectedValuePathProperty = DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(TreeView), new FrameworkPropertyMetadata(string.Empty, OnSelectedValuePathChanged));
		SelectedItemChangedEvent = EventManager.RegisterRoutedEvent("SelectedItemChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(TreeView));
		SelectedValuePathBindingExpression = new BindingExpressionUncommonField();
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeView), new FrameworkPropertyMetadata(typeof(TreeView)));
		VirtualizingPanel.IsVirtualizingProperty.OverrideMetadata(typeof(TreeView), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(TreeView));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(TreeView), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(TreeView), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
		VirtualizingPanel.ScrollUnitProperty.OverrideMetadata(typeof(TreeView), new FrameworkPropertyMetadata(ScrollUnit.Pixel));
		ControlsTraceLogger.AddControl(TelemetryControls.TreeView);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.TreeView" /> class.</summary>
	public TreeView()
	{
		_focusEnterMainFocusScopeEventHandler = OnFocusEnterMainFocusScope;
		KeyboardNavigation.Current.FocusEnterMainFocusScope += _focusEnterMainFocusScopeEventHandler;
	}

	private void SetSelectedItem(object data)
	{
		if (SelectedItem != data)
		{
			SetValue(SelectedItemPropertyKey, data);
		}
	}

	private void SetSelectedValue(object data)
	{
		if (SelectedValue != data)
		{
			SetValue(SelectedValuePropertyKey, data);
		}
	}

	private static void OnSelectedValuePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TreeView treeView = (TreeView)d;
		SelectedValuePathBindingExpression.ClearValue(treeView);
		treeView.UpdateSelectedValue(treeView.SelectedItem);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.TreeView.SelectedItemChanged" /> event when the <see cref="P:System.Windows.Controls.TreeView.SelectedItem" /> property value changes.</summary>
	/// <param name="e">Provides the item that was previously selected and the item that is currently selected for the <see cref="E:System.Windows.Controls.TreeView.SelectedItemChanged" /> event.</param>
	protected virtual void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
	{
		RaiseEvent(e);
	}

	internal void ChangeSelection(object data, TreeViewItem container, bool selected)
	{
		if (IsSelectionChangeActive)
		{
			return;
		}
		object oldValue = null;
		object newValue = null;
		bool flag = false;
		TreeViewItem selectedContainer = _selectedContainer;
		IsSelectionChangeActive = true;
		try
		{
			if (selected)
			{
				if (container != _selectedContainer)
				{
					oldValue = SelectedItem;
					newValue = data;
					if (_selectedContainer != null)
					{
						_selectedContainer.IsSelected = false;
						_selectedContainer.UpdateContainsSelection(selected: false);
					}
					_selectedContainer = container;
					_selectedContainer.UpdateContainsSelection(selected: true);
					SetSelectedItem(data);
					UpdateSelectedValue(data);
					flag = true;
				}
			}
			else if (container == _selectedContainer)
			{
				_selectedContainer.UpdateContainsSelection(selected: false);
				_selectedContainer = null;
				SetSelectedItem(null);
				UpdateSelectedValue(null);
				oldValue = data;
				flag = true;
			}
			if (container.IsSelected != selected)
			{
				container.IsSelected = selected;
			}
		}
		finally
		{
			IsSelectionChangeActive = false;
		}
		if (flag)
		{
			if (_selectedContainer != null && AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected) && UIElementAutomationPeer.CreatePeerForElement(_selectedContainer) is TreeViewItemAutomationPeer treeViewItemAutomationPeer)
			{
				treeViewItemAutomationPeer.RaiseAutomationSelectionEvent(AutomationEvents.SelectionItemPatternOnElementSelected);
			}
			if (selectedContainer != null && AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection) && UIElementAutomationPeer.CreatePeerForElement(selectedContainer) is TreeViewItemAutomationPeer treeViewItemAutomationPeer2)
			{
				treeViewItemAutomationPeer2.RaiseAutomationSelectionEvent(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection);
			}
			RoutedPropertyChangedEventArgs<object> e = new RoutedPropertyChangedEventArgs<object>(oldValue, newValue, SelectedItemChangedEvent);
			OnSelectedItemChanged(e);
		}
	}

	private void UpdateSelectedValue(object selectedItem)
	{
		BindingExpression bindingExpression = PrepareSelectedValuePathBindingExpression(selectedItem);
		if (bindingExpression != null)
		{
			bindingExpression.Activate(selectedItem);
			object value = bindingExpression.Value;
			bindingExpression.Deactivate();
			SetValue(SelectedValuePropertyKey, value);
		}
		else
		{
			ClearValue(SelectedValuePropertyKey);
		}
	}

	private BindingExpression PrepareSelectedValuePathBindingExpression(object item)
	{
		if (item == null)
		{
			return null;
		}
		bool flag = SystemXmlHelper.IsXmlNode(item);
		BindingExpression bindingExpression = SelectedValuePathBindingExpression.GetValue(this);
		if (bindingExpression != null)
		{
			Binding parentBinding = bindingExpression.ParentBinding;
			if (parentBinding.XPath != null != flag)
			{
				bindingExpression = null;
			}
		}
		if (bindingExpression == null)
		{
			Binding parentBinding = new Binding();
			parentBinding.Source = null;
			if (flag)
			{
				parentBinding.XPath = SelectedValuePath;
				parentBinding.Path = new PropertyPath("/InnerText");
			}
			else
			{
				parentBinding.Path = new PropertyPath(SelectedValuePath);
			}
			bindingExpression = (BindingExpression)BindingExpressionBase.CreateUntargetedBindingExpression(this, parentBinding);
			SelectedValuePathBindingExpression.SetValue(this, bindingExpression);
		}
		return bindingExpression;
	}

	internal void HandleSelectionAndCollapsed(TreeViewItem collapsed)
	{
		if (_selectedContainer == null || _selectedContainer == collapsed)
		{
			return;
		}
		TreeViewItem treeViewItem = _selectedContainer;
		do
		{
			treeViewItem = treeViewItem.ParentTreeViewItem;
			if (treeViewItem == collapsed)
			{
				TreeViewItem selectedContainer = _selectedContainer;
				ChangeSelection(collapsed.ParentItemsControl.ItemContainerGenerator.ItemFromContainer(collapsed), collapsed, selected: true);
				if (selectedContainer.IsKeyboardFocusWithin)
				{
					_selectedContainer.Focus();
				}
				break;
			}
		}
		while (treeViewItem != null);
	}

	internal void HandleMouseButtonDown()
	{
		if (base.IsKeyboardFocusWithin)
		{
			return;
		}
		if (_selectedContainer != null)
		{
			if (!_selectedContainer.IsKeyboardFocused)
			{
				_selectedContainer.Focus();
			}
		}
		else
		{
			Focus();
		}
	}

	/// <summary>Determines whether the specified item is its own container or can be its own container.</summary>
	/// <returns>true if <paramref name="item" /> is a <see cref="T:System.Windows.Controls.TreeViewItem" />; otherwise, false.</returns>
	/// <param name="item">The object to evaluate.</param>
	protected override bool IsItemItsOwnContainerOverride(object item)
	{
		return item is TreeViewItem;
	}

	/// <summary>Creates a <see cref="T:System.Windows.Controls.TreeViewItem" /> to use to display content.</summary>
	/// <returns>A new <see cref="T:System.Windows.Controls.TreeViewItem" /> to use as a container for content.</returns>
	protected override DependencyObject GetContainerForItemOverride()
	{
		return new TreeViewItem();
	}

	/// <summary>Provides class handling for an <see cref="E:System.Windows.Controls.ItemContainerGenerator.ItemsChanged" /> event that occurs when there is a change in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Remove:
		case NotifyCollectionChangedAction.Reset:
			if (SelectedItem != null && !IsSelectedContainerHookedUp)
			{
				SelectFirstItem();
			}
			break;
		case NotifyCollectionChangedAction.Replace:
		{
			object selectedItem = SelectedItem;
			if (selectedItem != null && selectedItem.Equals(e.OldItems[0]))
			{
				ChangeSelection(selectedItem, _selectedContainer, selected: false);
			}
			break;
		}
		default:
			throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, e.Action));
		case NotifyCollectionChangedAction.Add:
		case NotifyCollectionChangedAction.Move:
			break;
		}
	}

	private void SelectFirstItem()
	{
		object item;
		TreeViewItem container;
		bool firstItem = GetFirstItem(out item, out container);
		if (!firstItem)
		{
			item = SelectedItem;
			container = _selectedContainer;
		}
		ChangeSelection(item, container, firstItem);
	}

	private bool GetFirstItem(out object item, out TreeViewItem container)
	{
		if (base.HasItems)
		{
			item = base.Items[0];
			container = base.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
			if (item != null)
			{
				return container != null;
			}
			return false;
		}
		item = null;
		container = null;
		return false;
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.KeyDown" /> event for a <see cref="T:System.Windows.Controls.TreeView" />.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.Handled)
		{
			return;
		}
		if (IsControlKeyDown)
		{
			Key key = e.Key;
			if ((uint)(key - 19) <= 7u && HandleScrollKeys(e.Key))
			{
				e.Handled = true;
			}
			return;
		}
		switch (e.Key)
		{
		case Key.Up:
		case Key.Down:
			if (_selectedContainer == null && FocusFirstItem())
			{
				e.Handled = true;
			}
			break;
		case Key.Home:
			if (FocusFirstItem())
			{
				e.Handled = true;
			}
			break;
		case Key.End:
			if (FocusLastItem())
			{
				e.Handled = true;
			}
			break;
		case Key.Prior:
		case Key.Next:
			if (_selectedContainer == null)
			{
				if (FocusFirstItem())
				{
					e.Handled = true;
				}
			}
			else if (HandleScrollByPage(e))
			{
				e.Handled = true;
			}
			break;
		case Key.Tab:
			if (IsShiftKeyDown && base.IsKeyboardFocusWithin && MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous)))
			{
				e.Handled = true;
			}
			break;
		case Key.Multiply:
			if (ExpandSubtree(_selectedContainer))
			{
				e.Handled = true;
			}
			break;
		}
	}

	private bool FocusFirstItem()
	{
		FrameworkElement container;
		return NavigateToStartInternal(new ItemNavigateArgs(Keyboard.PrimaryDevice, Keyboard.Modifiers), shouldFocus: true, out container);
	}

	private bool FocusLastItem()
	{
		FrameworkElement container;
		return NavigateToEndInternal(new ItemNavigateArgs(Keyboard.PrimaryDevice, Keyboard.Modifiers), shouldFocus: true, out container);
	}

	private bool HandleScrollKeys(Key key)
	{
		ScrollViewer scrollHost = base.ScrollHost;
		if (scrollHost != null)
		{
			bool flag = base.FlowDirection == FlowDirection.RightToLeft;
			switch (key)
			{
			case Key.Up:
				scrollHost.LineUp();
				return true;
			case Key.Down:
				scrollHost.LineDown();
				return true;
			case Key.Left:
				if (flag)
				{
					scrollHost.LineRight();
				}
				else
				{
					scrollHost.LineLeft();
				}
				return true;
			case Key.Right:
				if (flag)
				{
					scrollHost.LineLeft();
				}
				else
				{
					scrollHost.LineRight();
				}
				return true;
			case Key.Home:
				scrollHost.ScrollToTop();
				return true;
			case Key.End:
				scrollHost.ScrollToBottom();
				return true;
			case Key.Prior:
				if (DoubleUtil.GreaterThan(scrollHost.ExtentHeight, scrollHost.ViewportHeight))
				{
					scrollHost.PageUp();
				}
				else
				{
					scrollHost.PageLeft();
				}
				return true;
			case Key.Next:
				if (DoubleUtil.GreaterThan(scrollHost.ExtentHeight, scrollHost.ViewportHeight))
				{
					scrollHost.PageDown();
				}
				else
				{
					scrollHost.PageRight();
				}
				return true;
			}
		}
		return false;
	}

	private bool HandleScrollByPage(KeyEventArgs e)
	{
		_ = Keyboard.FocusedElement;
		ItemInfo startingInfo = ItemsControl.ItemsControlFromItemContainer(_selectedContainer)?.ItemInfoFromContainer(_selectedContainer);
		FrameworkElement frameworkElement = _selectedContainer.HeaderElement;
		if (frameworkElement == null)
		{
			frameworkElement = _selectedContainer;
		}
		return NavigateByPage(startingInfo, frameworkElement, (e.Key == Key.Prior) ? FocusNavigationDirection.Up : FocusNavigationDirection.Down, new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
	}

	/// <summary>Expands the specified <see cref="T:System.Windows.Controls.TreeViewItem" /> control and all its child <see cref="T:System.Windows.Controls.TreeViewItem" /> elements.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Controls.TreeViewItem" /> and all its child elements were expanded; otherwise, false.</returns>
	/// <param name="container">The <see cref="T:System.Windows.Controls.TreeViewItem" /> to expand.</param>
	protected virtual bool ExpandSubtree(TreeViewItem container)
	{
		if (container != null)
		{
			container.ExpandSubtree();
			return true;
		}
		return false;
	}

	/// <summary>Provides class handling for an <see cref="E:System.Windows.ContentElement.IsKeyboardFocusWithinChanged" /> event when the keyboard focus changes for a <see cref="T:System.Windows.Controls.TreeView" />.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnIsKeyboardFocusWithinChanged(e);
		bool flag = false;
		bool isKeyboardFocusWithin = base.IsKeyboardFocusWithin;
		if (isKeyboardFocusWithin)
		{
			flag = true;
		}
		else if (Keyboard.FocusedElement is DependencyObject element && KeyboardNavigation.GetVisualRoot(this) is UIElement { IsKeyboardFocusWithin: not false } uIElement && FocusManager.GetFocusScope(element) != uIElement)
		{
			flag = true;
		}
		if ((bool)GetValue(Selector.IsSelectionActiveProperty) != flag)
		{
			SetValue(Selector.IsSelectionActivePropertyKey, BooleanBoxes.Box(flag));
		}
		if (isKeyboardFocusWithin && base.IsKeyboardFocused && _selectedContainer != null && !_selectedContainer.IsKeyboardFocusWithin)
		{
			_selectedContainer.Focus();
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.UIElement.GotFocus" /> routed event. </summary>
	/// <param name="e">The data for the event. </param>
	protected override void OnGotFocus(RoutedEventArgs e)
	{
		base.OnGotFocus(e);
		if (base.IsKeyboardFocusWithin && base.IsKeyboardFocused && _selectedContainer != null && !_selectedContainer.IsKeyboardFocusWithin)
		{
			_selectedContainer.Focus();
		}
	}

	private void OnFocusEnterMainFocusScope(object sender, EventArgs e)
	{
		if (!base.IsKeyboardFocusWithin)
		{
			ClearValue(Selector.IsSelectionActivePropertyKey);
		}
	}

	private static DependencyObject FindParent(DependencyObject o)
	{
		Visual visual = o as Visual;
		ContentElement contentElement = ((visual == null) ? (o as ContentElement) : null);
		if (contentElement != null)
		{
			o = ContentOperations.GetParent(contentElement);
			if (o != null)
			{
				return o;
			}
			if (contentElement is FrameworkContentElement frameworkContentElement)
			{
				return frameworkContentElement.Parent;
			}
		}
		else if (visual != null)
		{
			return VisualTreeHelper.GetParent(visual);
		}
		return null;
	}

	/// <summary>Defines an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.TreeView" /> control.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.TreeViewAutomationPeer" /> for the <see cref="T:System.Windows.Controls.TreeView" /> control.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new TreeViewAutomationPeer(this);
	}
}
