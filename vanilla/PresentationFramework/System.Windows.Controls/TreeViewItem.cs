using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Implements a selectable item in a <see cref="T:System.Windows.Controls.TreeView" /> control.</summary>
[TemplatePart(Name = "PART_Header", Type = typeof(FrameworkElement))]
[TemplatePart(Name = "ItemsHost", Type = typeof(ItemsPresenter))]
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(TreeViewItem))]
public class TreeViewItem : HeaderedItemsControl, IHierarchicalVirtualizationAndScrollInfo
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TreeViewItem.IsExpanded" /> dependency property. </summary>
	public static readonly DependencyProperty IsExpandedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TreeViewItem.IsSelected" /> dependency property. </summary>
	public static readonly DependencyProperty IsSelectedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TreeViewItem.IsSelectionActive" /> dependency property. </summary>
	public static readonly DependencyProperty IsSelectionActiveProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.TreeViewItem.Expanded" /> routed event. </summary>
	public static readonly RoutedEvent ExpandedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.TreeViewItem.Collapsed" /> routed event. </summary>
	public static readonly RoutedEvent CollapsedEvent;

	/// <summary>Identified by the <see cref="E:System.Windows.Controls.TreeViewItem.Selected" /> routed event. </summary>
	public static readonly RoutedEvent SelectedEvent;

	/// <summary>Identified by the <see cref="E:System.Windows.Controls.TreeViewItem.Unselected" /> routed event. </summary>
	public static readonly RoutedEvent UnselectedEvent;

	private static DependencyObjectType _dType;

	private const string HeaderPartName = "PART_Header";

	private const string ItemsHostPartName = "ItemsHost";

	/// <summary>Gets or sets whether the nested items in a <see cref="T:System.Windows.Controls.TreeViewItem" /> are expanded or collapsed.  </summary>
	/// <returns>true if the nested items of a <see cref="T:System.Windows.Controls.TreeViewItem" /> are visible; otherwise, false. The default is false.</returns>
	public bool IsExpanded
	{
		get
		{
			return (bool)GetValue(IsExpandedProperty);
		}
		set
		{
			SetValue(IsExpandedProperty, value);
		}
	}

	private bool CanExpand => base.HasItems;

	/// <summary>Gets or sets whether a <see cref="T:System.Windows.Controls.TreeViewItem" /> control is selected.  </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.TreeViewItem" /> is selected; otherwise, false. The default is false.</returns>
	public bool IsSelected
	{
		get
		{
			return (bool)GetValue(IsSelectedProperty);
		}
		set
		{
			SetValue(IsSelectedProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.TreeViewItem" /> has keyboard focus.  </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.TreeViewItem" /> has keyboard focus; otherwise, false. The default is false.</returns>
	[Browsable(false)]
	[Category("Appearance")]
	[ReadOnly(true)]
	public bool IsSelectionActive => (bool)GetValue(IsSelectionActiveProperty);

	/// <summary>Gets or sets an object that represents the viewport and cache sizes of the <see cref="T:System.Windows.Controls.TreeViewItem" />.</summary>
	/// <returns>An object that represents the viewport and cache sizes of the <see cref="T:System.Windows.Controls.TreeViewItem" />.</returns>
	HierarchicalVirtualizationConstraints IHierarchicalVirtualizationAndScrollInfo.Constraints
	{
		get
		{
			return GroupItem.HierarchicalVirtualizationConstraintsField.GetValue(this);
		}
		set
		{
			if (value.CacheLengthUnit == VirtualizationCacheLengthUnit.Page)
			{
				throw new InvalidOperationException(SR.PageCacheSizeNotAllowed);
			}
			GroupItem.HierarchicalVirtualizationConstraintsField.SetValue(this, value);
		}
	}

	/// <summary>Gets an object that represents the desired size of the <see cref="P:System.Windows.Controls.HeaderedItemsControl.Header" />, in pixels and in logical units.</summary>
	/// <returns>An object that represents the desired size of the <see cref="P:System.Windows.Controls.HeaderedItemsControl.Header" />, in pixels and in logical units.</returns>
	HierarchicalVirtualizationHeaderDesiredSizes IHierarchicalVirtualizationAndScrollInfo.HeaderDesiredSizes
	{
		get
		{
			FrameworkElement headerElement = HeaderElement;
			Size headerSize = ((base.IsVisible && headerElement != null) ? headerElement.DesiredSize : default(Size));
			Helper.ApplyCorrectionFactorToPixelHeaderSize(ParentTreeView, this, base.ItemsHost, ref headerSize);
			return new HierarchicalVirtualizationHeaderDesiredSizes(new Size(DoubleUtil.GreaterThanZero(headerSize.Width) ? 1 : 0, DoubleUtil.GreaterThanZero(headerSize.Height) ? 1 : 0), headerSize);
		}
	}

	/// <summary>Gets or sets an object that represents the desired size of the control's items, in pixels and in logical units.</summary>
	/// <returns>An object that represents the desired size of the control's items, in pixels and in logical units.</returns>
	HierarchicalVirtualizationItemDesiredSizes IHierarchicalVirtualizationAndScrollInfo.ItemDesiredSizes
	{
		get
		{
			return Helper.ApplyCorrectionFactorToItemDesiredSizes(this, base.ItemsHost);
		}
		set
		{
			GroupItem.HierarchicalVirtualizationItemDesiredSizesField.SetValue(this, value);
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Controls.Panel" /> that displays the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> of the <see cref="T:System.Windows.Controls.TreeViewItem" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.Panel" /> that displays the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> of the <see cref="T:System.Windows.Controls.TreeViewItem" />.</returns>
	Panel IHierarchicalVirtualizationAndScrollInfo.ItemsHost => base.ItemsHost;

	/// <summary>Gets or sets a value that indicates whether the owning <see cref="T:System.Windows.Controls.ItemsControl" /> should virtualize its items.</summary>
	/// <returns>true if the owning <see cref="T:System.Windows.Controls.ItemsControl" /> should virtualize its items; otherwise, false.</returns>
	bool IHierarchicalVirtualizationAndScrollInfo.MustDisableVirtualization
	{
		get
		{
			return GroupItem.MustDisableVirtualizationField.GetValue(this);
		}
		set
		{
			GroupItem.MustDisableVirtualizationField.SetValue(this, value);
		}
	}

	/// <summary>Gets a value that indicates whether the control's layout pass occurs at a lower priority.</summary>
	/// <returns>true if the control's layout pass occurs at a lower priority; otherwise, false.</returns>
	bool IHierarchicalVirtualizationAndScrollInfo.InBackgroundLayout
	{
		get
		{
			return GroupItem.InBackgroundLayoutField.GetValue(this);
		}
		set
		{
			GroupItem.InBackgroundLayoutField.SetValue(this, value);
		}
	}

	internal TreeView ParentTreeView
	{
		get
		{
			for (ItemsControl itemsControl = ParentItemsControl; itemsControl != null; itemsControl = ItemsControl.ItemsControlFromItemContainer(itemsControl))
			{
				if (itemsControl is TreeView result)
				{
					return result;
				}
			}
			return null;
		}
	}

	internal TreeViewItem ParentTreeViewItem => ParentItemsControl as TreeViewItem;

	internal ItemsControl ParentItemsControl => ItemsControl.ItemsControlFromItemContainer(this);

	private bool ContainsSelection
	{
		get
		{
			return ReadControlFlag(ControlBoolFlags.ContainsSelection);
		}
		set
		{
			WriteControlFlag(ControlBoolFlags.ContainsSelection, value);
		}
	}

	private static bool IsControlKeyDown => (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

	private bool CanExpandOnInput
	{
		get
		{
			if (CanExpand)
			{
				return base.IsEnabled;
			}
			return false;
		}
	}

	internal FrameworkElement HeaderElement => GetTemplateChild("PART_Header") as FrameworkElement;

	private ItemsPresenter ItemsHostPresenter => GetTemplateChild("ItemsHost") as ItemsPresenter;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.TreeViewItem.IsExpanded" /> property changes from false to true.</summary>
	[Category("Behavior")]
	public event RoutedEventHandler Expanded
	{
		add
		{
			AddHandler(ExpandedEvent, value);
		}
		remove
		{
			RemoveHandler(ExpandedEvent, value);
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.TreeViewItem.IsExpanded" /> property changes from true to false.</summary>
	[Category("Behavior")]
	public event RoutedEventHandler Collapsed
	{
		add
		{
			AddHandler(CollapsedEvent, value);
		}
		remove
		{
			RemoveHandler(CollapsedEvent, value);
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.TreeViewItem.IsSelected" /> property of a <see cref="T:System.Windows.Controls.TreeViewItem" /> changes from false to true.</summary>
	[Category("Behavior")]
	public event RoutedEventHandler Selected
	{
		add
		{
			AddHandler(SelectedEvent, value);
		}
		remove
		{
			RemoveHandler(SelectedEvent, value);
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.TreeViewItem.IsSelected" /> property of a <see cref="T:System.Windows.Controls.TreeViewItem" /> changes from true to false.</summary>
	[Category("Behavior")]
	public event RoutedEventHandler Unselected
	{
		add
		{
			AddHandler(UnselectedEvent, value);
		}
		remove
		{
			RemoveHandler(UnselectedEvent, value);
		}
	}

	static TreeViewItem()
	{
		IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(TreeViewItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, OnIsExpandedChanged));
		IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(TreeViewItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectedChanged));
		IsSelectionActiveProperty = Selector.IsSelectionActiveProperty.AddOwner(typeof(TreeViewItem));
		ExpandedEvent = EventManager.RegisterRoutedEvent("Expanded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeViewItem));
		CollapsedEvent = EventManager.RegisterRoutedEvent("Collapsed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeViewItem));
		SelectedEvent = EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeViewItem));
		UnselectedEvent = EventManager.RegisterRoutedEvent("Unselected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeViewItem));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeViewItem), new FrameworkPropertyMetadata(typeof(TreeViewItem)));
		VirtualizingPanel.IsVirtualizingProperty.OverrideMetadata(typeof(TreeViewItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(TreeViewItem));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(TreeViewItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue));
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(TreeViewItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
		Control.IsTabStopProperty.OverrideMetadata(typeof(TreeViewItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		UIElement.IsMouseOverPropertyKey.OverrideMetadata(typeof(TreeViewItem), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(TreeViewItem), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		Selector.IsSelectionActivePropertyKey.OverrideMetadata(typeof(TreeViewItem), new FrameworkPropertyMetadata(Control.OnVisualStatePropertyChanged));
		EventManager.RegisterClassHandler(typeof(TreeViewItem), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(OnRequestBringIntoView));
		EventManager.RegisterClassHandler(typeof(TreeViewItem), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseButtonDown), handledEventsToo: true);
		AutomationProperties.IsOffscreenBehaviorProperty.OverrideMetadata(typeof(TreeViewItem), new FrameworkPropertyMetadata(IsOffscreenBehavior.FromClip));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.TreeViewItem" /> class.</summary>
	public TreeViewItem()
	{
	}

	private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TreeViewItem treeViewItem = (TreeViewItem)d;
		bool flag = (bool)e.NewValue;
		TreeView parentTreeView = treeViewItem.ParentTreeView;
		if (parentTreeView != null && !flag)
		{
			parentTreeView.HandleSelectionAndCollapsed(treeViewItem);
		}
		ItemsPresenter itemsHostPresenter = treeViewItem.ItemsHostPresenter;
		if (itemsHostPresenter != null)
		{
			treeViewItem.InvalidateMeasure();
			Helper.InvalidateMeasureOnPath(itemsHostPresenter, treeViewItem, duringMeasure: false);
		}
		if (UIElementAutomationPeer.FromElement(treeViewItem) is TreeViewItemAutomationPeer treeViewItemAutomationPeer)
		{
			treeViewItemAutomationPeer.RaiseExpandCollapseAutomationEvent((bool)e.OldValue, flag);
		}
		if (flag)
		{
			treeViewItem.OnExpanded(new RoutedEventArgs(ExpandedEvent, treeViewItem));
		}
		else
		{
			treeViewItem.OnCollapsed(new RoutedEventArgs(CollapsedEvent, treeViewItem));
		}
		treeViewItem.UpdateVisualState();
	}

	private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TreeViewItem treeViewItem = (TreeViewItem)d;
		bool flag = (bool)e.NewValue;
		treeViewItem.Select(flag);
		if (UIElementAutomationPeer.FromElement(treeViewItem) is TreeViewItemAutomationPeer treeViewItemAutomationPeer)
		{
			treeViewItemAutomationPeer.RaiseAutomationIsSelectedChanged(flag);
		}
		if (flag)
		{
			treeViewItem.OnSelected(new RoutedEventArgs(SelectedEvent, treeViewItem));
		}
		else
		{
			treeViewItem.OnUnselected(new RoutedEventArgs(UnselectedEvent, treeViewItem));
		}
		treeViewItem.UpdateVisualState();
	}

	/// <summary>Raises an <see cref="E:System.Windows.Controls.TreeViewItem.Expanded" /> event when the <see cref="P:System.Windows.Controls.TreeViewItem.IsExpanded" /> property changes from false to true.</summary>
	/// <param name="e">The event arguments.</param>
	protected virtual void OnExpanded(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Raises a <see cref="E:System.Windows.Controls.TreeViewItem.Collapsed" /> event when the <see cref="P:System.Windows.Controls.TreeViewItem.IsExpanded" /> property changes from true to false. </summary>
	/// <param name="e">The event arguments.</param>
	protected virtual void OnCollapsed(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.TreeViewItem.Selected" /> routed event when the <see cref="P:System.Windows.Controls.TreeViewItem.IsSelected" /> property changes from false to true.</summary>
	/// <param name="e">The event arguments.</param>
	protected virtual void OnSelected(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.TreeViewItem.Unselected" /> routed event when the <see cref="P:System.Windows.Controls.TreeViewItem.IsSelected" /> property changes from true to false.</summary>
	/// <param name="e">The event arguments.</param>
	protected virtual void OnUnselected(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Expands the <see cref="T:System.Windows.Controls.TreeViewItem" /> control and all its child <see cref="T:System.Windows.Controls.TreeViewItem" /> elements.</summary>
	public void ExpandSubtree()
	{
		ExpandRecursive(this);
	}

	/// <summary>Arranges the content of the <see cref="T:System.Windows.Controls.TreeViewItem" />.</summary>
	/// <returns>The actual size used by the <see cref="T:System.Windows.Controls.TreeViewItem" />.</returns>
	/// <param name="arrangeSize">The final area within the parent that the <see cref="T:System.Windows.Controls.TreeViewItem" /> should use to arrange itself and its children.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		arrangeSize = base.ArrangeOverride(arrangeSize);
		Helper.ComputeCorrectionFactor(ParentTreeView, this, base.ItemsHost, HeaderElement);
		return arrangeSize;
	}

	/// <summary>Responds to a change in the visual parent of a <see cref="T:System.Windows.Controls.TreeViewItem" />.</summary>
	/// <param name="oldParent">The previous visual parent.</param>
	protected internal override void OnVisualParentChanged(DependencyObject oldParent)
	{
		if (VisualTreeHelper.GetParent(this) != null && IsSelected)
		{
			Select(selected: true);
		}
		base.OnVisualParentChanged(oldParent);
	}

	private void Select(bool selected)
	{
		TreeView parentTreeView = ParentTreeView;
		ItemsControl parentItemsControl = ParentItemsControl;
		if (parentTreeView != null && parentItemsControl != null && !parentTreeView.IsSelectionChangeActive)
		{
			object itemOrContainerFromContainer = parentItemsControl.GetItemOrContainerFromContainer(this);
			parentTreeView.ChangeSelection(itemOrContainerFromContainer, this, selected);
			if (selected && parentTreeView.IsKeyboardFocusWithin && !base.IsKeyboardFocusWithin)
			{
				Focus();
			}
		}
	}

	internal void UpdateContainsSelection(bool selected)
	{
		for (TreeViewItem parentTreeViewItem = ParentTreeViewItem; parentTreeViewItem != null; parentTreeViewItem = parentTreeViewItem.ParentTreeViewItem)
		{
			parentTreeViewItem.ContainsSelection = selected;
		}
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.GotFocus" /> event. </summary>
	/// <param name="e">The event data.</param>
	protected override void OnGotFocus(RoutedEventArgs e)
	{
		Select(selected: true);
		base.OnGotFocus(e);
	}

	/// <summary>Provides class handling for a <see cref="E:System.Windows.UIElement.MouseLeftButtonDown" /> event.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		if (!e.Handled && base.IsEnabled)
		{
			bool isFocused = base.IsFocused;
			if (Focus())
			{
				if (isFocused && !IsSelected)
				{
					Select(selected: true);
				}
				e.Handled = true;
			}
			if (e.ClickCount % 2 == 0)
			{
				SetCurrentValueInternal(IsExpandedProperty, BooleanBoxes.Box(!IsExpanded));
				e.Handled = true;
			}
		}
		base.OnMouseLeftButtonDown(e);
	}

	/// <summary>Provides class handling for a <see cref="E:System.Windows.UIElement.KeyDown" /> event.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.Handled)
		{
			return;
		}
		switch (e.Key)
		{
		case Key.Add:
			if (CanExpandOnInput && !IsExpanded)
			{
				SetCurrentValueInternal(IsExpandedProperty, BooleanBoxes.TrueBox);
				e.Handled = true;
			}
			break;
		case Key.Subtract:
			if (CanExpandOnInput && IsExpanded)
			{
				SetCurrentValueInternal(IsExpandedProperty, BooleanBoxes.FalseBox);
				e.Handled = true;
			}
			break;
		case Key.Left:
		case Key.Right:
			if (LogicalLeft(e.Key))
			{
				if (!IsControlKeyDown && CanExpandOnInput && IsExpanded)
				{
					if (base.IsFocused)
					{
						SetCurrentValueInternal(IsExpandedProperty, BooleanBoxes.FalseBox);
					}
					else
					{
						Focus();
					}
					e.Handled = true;
				}
			}
			else if (!IsControlKeyDown && CanExpandOnInput)
			{
				if (!IsExpanded)
				{
					SetCurrentValueInternal(IsExpandedProperty, BooleanBoxes.TrueBox);
					e.Handled = true;
				}
				else if (HandleDownKey(e))
				{
					e.Handled = true;
				}
			}
			break;
		case Key.Down:
			if (!IsControlKeyDown && HandleDownKey(e))
			{
				e.Handled = true;
			}
			break;
		case Key.Up:
			if (!IsControlKeyDown && HandleUpKey(e))
			{
				e.Handled = true;
			}
			break;
		}
	}

	private bool LogicalLeft(Key key)
	{
		bool flag = base.FlowDirection == FlowDirection.RightToLeft;
		if (flag || key != Key.Left)
		{
			if (flag)
			{
				return key == Key.Right;
			}
			return false;
		}
		return true;
	}

	internal bool HandleUpKey(KeyEventArgs e)
	{
		return HandleUpDownKey(up: true, e);
	}

	internal bool HandleDownKey(KeyEventArgs e)
	{
		return HandleUpDownKey(up: false, e);
	}

	private bool HandleUpDownKey(bool up, KeyEventArgs e)
	{
		FocusNavigationDirection direction = (up ? FocusNavigationDirection.Up : FocusNavigationDirection.Down);
		if (AllowHandleKeyEvent(direction))
		{
			TreeView parentTreeView = ParentTreeView;
			_ = Keyboard.FocusedElement;
			if (parentTreeView != null)
			{
				FrameworkElement frameworkElement = HeaderElement;
				if (frameworkElement == null)
				{
					frameworkElement = this;
				}
				ItemInfo startingInfo = ItemsControl.ItemsControlFromItemContainer(this)?.ItemInfoFromContainer(this);
				return parentTreeView.NavigateByLine(startingInfo, frameworkElement, direction, new ItemNavigateArgs(e.Device, Keyboard.Modifiers));
			}
		}
		return false;
	}

	private bool AllowHandleKeyEvent(FocusNavigationDirection direction)
	{
		if (!IsSelected)
		{
			return false;
		}
		if (Keyboard.FocusedElement is DependencyObject dependencyObject)
		{
			DependencyObject dependencyObject2 = UIElementHelper.PredictFocus(dependencyObject, direction);
			if (dependencyObject2 != dependencyObject)
			{
				while (dependencyObject2 != null)
				{
					TreeViewItem treeViewItem = dependencyObject2 as TreeViewItem;
					if (treeViewItem == this)
					{
						return false;
					}
					if (treeViewItem != null || dependencyObject2 is TreeView)
					{
						return true;
					}
					dependencyObject2 = KeyboardNavigation.GetParent(dependencyObject2);
				}
			}
		}
		return true;
	}

	private static void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
	{
		((TreeViewItem)sender).ParentTreeView?.HandleMouseButtonDown();
	}

	private static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
	{
		if (e.TargetObject == sender)
		{
			((TreeViewItem)sender).HandleBringIntoView(e);
		}
	}

	private void HandleBringIntoView(RequestBringIntoViewEventArgs e)
	{
		for (TreeViewItem parentTreeViewItem = ParentTreeViewItem; parentTreeViewItem != null; parentTreeViewItem = parentTreeViewItem.ParentTreeViewItem)
		{
			if (!parentTreeViewItem.IsExpanded)
			{
				parentTreeViewItem.SetCurrentValueInternal(IsExpandedProperty, BooleanBoxes.TrueBox);
			}
		}
		if (e.TargetRect.IsEmpty)
		{
			FrameworkElement headerElement = HeaderElement;
			if (headerElement != null)
			{
				e.Handled = true;
				headerElement.BringIntoView();
			}
			else
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(BringItemIntoView), null);
			}
		}
	}

	private object BringItemIntoView(object args)
	{
		HeaderElement?.BringIntoView();
		return null;
	}

	internal FrameworkElement TryGetHeaderElement()
	{
		FrameworkElement headerElement = HeaderElement;
		if (headerElement != null)
		{
			return headerElement;
		}
		FrameworkTemplate templateInternal = TemplateInternal;
		if (templateInternal == null)
		{
			return this;
		}
		int num = StyleHelper.QueryChildIndexFromChildName("PART_Header", templateInternal.ChildIndexFromChildName);
		if (num < 0)
		{
			ToggleButton toggleButton = Helper.FindTemplatedDescendant<ToggleButton>(this, this);
			if (toggleButton != null && VisualTreeHelper.GetParent(toggleButton) is FrameworkElement reference)
			{
				int childrenCount = VisualTreeHelper.GetChildrenCount(reference);
				for (num = 0; num < childrenCount - 1; num++)
				{
					if (VisualTreeHelper.GetChild(reference, num) == toggleButton)
					{
						if (!(VisualTreeHelper.GetChild(reference, num + 1) is FrameworkElement result))
						{
							break;
						}
						return result;
					}
				}
			}
		}
		return this;
	}

	/// <summary>Determines whether an object is a <see cref="T:System.Windows.Controls.TreeViewItem" />.</summary>
	/// <returns>true if <paramref name="item" /> is a <see cref="T:System.Windows.Controls.TreeViewItem" />; otherwise, false.</returns>
	/// <param name="item">The object to evaluate.</param>
	protected override bool IsItemItsOwnContainerOverride(object item)
	{
		return item is TreeViewItem;
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Controls.TreeViewItem" /> to use to display the object.</summary>
	/// <returns>A new <see cref="T:System.Windows.Controls.TreeViewItem" /> to use to display the object.</returns>
	protected override DependencyObject GetContainerForItemOverride()
	{
		return new TreeViewItem();
	}

	internal void PrepareItemContainer(object item, ItemsControl parentItemsControl)
	{
		Helper.ClearVirtualizingElement(this);
		IsVirtualizingPropagationHelper(parentItemsControl, this);
		if (VirtualizingPanel.GetIsVirtualizing(parentItemsControl))
		{
			Helper.SetItemValuesOnContainer(parentItemsControl, this, item);
		}
	}

	internal void ClearItemContainer(object item, ItemsControl parentItemsControl)
	{
		if (VirtualizingPanel.GetIsVirtualizing(parentItemsControl))
		{
			Helper.StoreItemValues(parentItemsControl, this, item);
			if (base.ItemsHost is VirtualizingPanel virtualizingPanel)
			{
				virtualizingPanel.OnClearChildrenInternal();
			}
			base.ItemContainerGenerator.RemoveAllInternal(saveRecycleQueue: true);
		}
		ContainsSelection = false;
	}

	internal static void IsVirtualizingPropagationHelper(DependencyObject parent, DependencyObject element)
	{
		SynchronizeValue(VirtualizingPanel.IsVirtualizingProperty, parent, element);
		SynchronizeValue(VirtualizingPanel.IsVirtualizingWhenGroupingProperty, parent, element);
		SynchronizeValue(VirtualizingPanel.VirtualizationModeProperty, parent, element);
		SynchronizeValue(VirtualizingPanel.ScrollUnitProperty, parent, element);
	}

	internal static void SynchronizeValue(DependencyProperty dp, DependencyObject parent, DependencyObject child)
	{
		object value = parent.GetValue(dp);
		child.SetValue(dp, value);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.Controls.ItemContainerGenerator.ItemsChanged" /> event that occurs when there is a change in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Remove:
		case NotifyCollectionChangedAction.Reset:
			if (ContainsSelection)
			{
				TreeView parentTreeView2 = ParentTreeView;
				if (parentTreeView2 != null && !parentTreeView2.IsSelectedContainerHookedUp)
				{
					ContainsSelection = false;
					Select(selected: true);
				}
			}
			break;
		case NotifyCollectionChangedAction.Replace:
		{
			if (!ContainsSelection)
			{
				break;
			}
			TreeView parentTreeView = ParentTreeView;
			if (parentTreeView != null)
			{
				object selectedItem = parentTreeView.SelectedItem;
				if (selectedItem != null && selectedItem.Equals(e.OldItems[0]))
				{
					parentTreeView.ChangeSelection(selectedItem, parentTreeView.SelectedContainer, selected: false);
				}
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

	private static void ExpandRecursive(TreeViewItem item)
	{
		if (item == null)
		{
			return;
		}
		if (!item.IsExpanded)
		{
			item.SetCurrentValueInternal(IsExpandedProperty, BooleanBoxes.TrueBox);
		}
		item.ApplyTemplate();
		ItemsPresenter itemsPresenter = (ItemsPresenter)item.Template.FindName("ItemsHost", item);
		if (itemsPresenter != null)
		{
			itemsPresenter.ApplyTemplate();
		}
		else
		{
			item.UpdateLayout();
		}
		VirtualizingPanel virtualizingPanel = item.ItemsHost as VirtualizingPanel;
		item.ItemsHost.EnsureGenerator();
		int i = 0;
		for (int count = item.Items.Count; i < count; i++)
		{
			TreeViewItem treeViewItem;
			if (virtualizingPanel != null)
			{
				virtualizingPanel.BringIndexIntoView(i);
				treeViewItem = (TreeViewItem)item.ItemContainerGenerator.ContainerFromIndex(i);
			}
			else
			{
				treeViewItem = (TreeViewItem)item.ItemContainerGenerator.ContainerFromIndex(i);
				treeViewItem.BringIntoView();
			}
			if (treeViewItem != null)
			{
				ExpandRecursive(treeViewItem);
			}
		}
	}

	/// <summary>Defines an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.TreeViewItem" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.TreeViewItemAutomationPeer" /> object for the <see cref="T:System.Windows.Controls.TreeViewItem" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new TreeViewItemAutomationPeer(this);
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (!base.IsEnabled)
		{
			VisualStates.GoToState(this, useTransitions, "Disabled", "Normal");
		}
		else if (base.IsMouseOver)
		{
			VisualStates.GoToState(this, useTransitions, "MouseOver", "Normal");
		}
		else
		{
			VisualStates.GoToState(this, useTransitions, "Normal");
		}
		if (base.IsKeyboardFocused)
		{
			VisualStates.GoToState(this, useTransitions, "Focused", "Unfocused");
		}
		else
		{
			VisualStates.GoToState(this, useTransitions, "Unfocused");
		}
		if (IsExpanded)
		{
			VisualStates.GoToState(this, useTransitions, "Expanded");
		}
		else
		{
			VisualStates.GoToState(this, useTransitions, "Collapsed");
		}
		if (base.HasItems)
		{
			VisualStates.GoToState(this, useTransitions, "HasItems");
		}
		else
		{
			VisualStates.GoToState(this, useTransitions, "NoItems");
		}
		if (IsSelected)
		{
			if (IsSelectionActive)
			{
				VisualStates.GoToState(this, useTransitions, "Selected");
			}
			else
			{
				VisualStates.GoToState(this, useTransitions, "SelectedInactive", "Selected");
			}
		}
		else
		{
			VisualStates.GoToState(this, useTransitions, "Unselected");
		}
		base.ChangeVisualState(useTransitions);
	}
}
