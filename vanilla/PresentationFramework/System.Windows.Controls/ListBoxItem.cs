using System.ComponentModel;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Represents a selectable item in a <see cref="T:System.Windows.Controls.ListBox" />. </summary>
[DefaultEvent("Selected")]
public class ListBoxItem : ContentControl
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ListBoxItem.IsSelected" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ListBoxItem.IsSelected" /> dependency property.</returns>
	public static readonly DependencyProperty IsSelectedProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.ListBoxItem.Selected" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.ListBoxItem.Selected" /> routed event.</returns>
	public static readonly RoutedEvent SelectedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.ListBoxItem.Unselected" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.ListBoxItem.Unselected" /> routed event.</returns>
	public static readonly RoutedEvent UnselectedEvent;

	private DispatcherOperation parentNotifyDraggedOperation;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets a value that indicates whether a <see cref="T:System.Windows.Controls.ListBoxItem" /> is selected.  </summary>
	/// <returns>true if the item is selected; otherwise, false. The default is false.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public bool IsSelected
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

	private ListBox ParentListBox => ParentSelector as ListBox;

	internal Selector ParentSelector => ItemsControl.ItemsControlFromItemContainer(this) as Selector;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Occurs when a <see cref="T:System.Windows.Controls.ListBoxItem" /> is selected.</summary>
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

	/// <summary>Occurs when a <see cref="T:System.Windows.Controls.ListBoxItem" /> is unselected.</summary>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ListBoxItem" /> class. </summary>
	public ListBoxItem()
	{
	}

	static ListBoxItem()
	{
		IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(typeof(ListBoxItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnIsSelectedChanged));
		SelectedEvent = Selector.SelectedEvent.AddOwner(typeof(ListBoxItem));
		UnselectedEvent = Selector.UnselectedEvent.AddOwner(typeof(ListBoxItem));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ListBoxItem), new FrameworkPropertyMetadata(typeof(ListBoxItem)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(ListBoxItem));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(ListBoxItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(ListBoxItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(ListBoxItem), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		UIElement.IsMouseOverPropertyKey.OverrideMetadata(typeof(ListBoxItem), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		Selector.IsSelectionActivePropertyKey.OverrideMetadata(typeof(ListBoxItem), new FrameworkPropertyMetadata(Control.OnVisualStatePropertyChanged));
		AutomationProperties.IsOffscreenBehaviorProperty.OverrideMetadata(typeof(ListBoxItem), new FrameworkPropertyMetadata(IsOffscreenBehavior.FromClip));
	}

	private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ListBoxItem listBoxItem = d as ListBoxItem;
		bool flag = (bool)e.NewValue;
		listBoxItem.ParentSelector?.RaiseIsSelectedChangedAutomationEvent(listBoxItem, flag);
		if (flag)
		{
			listBoxItem.OnSelected(new RoutedEventArgs(Selector.SelectedEvent, listBoxItem));
		}
		else
		{
			listBoxItem.OnUnselected(new RoutedEventArgs(Selector.UnselectedEvent, listBoxItem));
		}
		listBoxItem.UpdateVisualState();
	}

	/// <summary>Called when the <see cref="T:System.Windows.Controls.ListBoxItem" /> is selected in a <see cref="T:System.Windows.Controls.ListBox" />.  </summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnSelected(RoutedEventArgs e)
	{
		HandleIsSelectedChanged(newValue: true, e);
	}

	/// <summary>Called when the <see cref="T:System.Windows.Controls.ListBoxItem" /> is unselected in a <see cref="T:System.Windows.Controls.ListBox" />. </summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnUnselected(RoutedEventArgs e)
	{
		HandleIsSelectedChanged(newValue: false, e);
	}

	private void HandleIsSelectedChanged(bool newValue, RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (!base.IsEnabled)
		{
			VisualStateManager.GoToState(this, (base.Content is Control) ? "Normal" : "Disabled", useTransitions);
		}
		else if (base.IsMouseOver)
		{
			VisualStateManager.GoToState(this, "MouseOver", useTransitions);
		}
		else
		{
			VisualStateManager.GoToState(this, "Normal", useTransitions);
		}
		if (IsSelected)
		{
			if (Selector.GetIsSelectionActive(this))
			{
				VisualStateManager.GoToState(this, "Selected", useTransitions);
			}
			else
			{
				VisualStates.GoToState(this, useTransitions, "SelectedUnfocused", "Selected");
			}
		}
		else
		{
			VisualStateManager.GoToState(this, "Unselected", useTransitions);
		}
		if (base.IsKeyboardFocused)
		{
			VisualStateManager.GoToState(this, "Focused", useTransitions);
		}
		else
		{
			VisualStateManager.GoToState(this, "Unfocused", useTransitions);
		}
		base.ChangeVisualState(useTransitions);
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.ListBoxItemAutomationPeer" /> implementation for this control, as part of the WPF automation infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new ListBoxItemWrapperAutomationPeer(this);
	}

	/// <summary>Called when the user presses the right mouse button over the <see cref="T:System.Windows.Controls.ListBoxItem" />.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		if (!e.Handled)
		{
			e.Handled = true;
			HandleMouseButtonDown(MouseButton.Left);
		}
		base.OnMouseLeftButtonDown(e);
	}

	/// <summary>Called when the user presses the right mouse button over a <see cref="T:System.Windows.Controls.ListBoxItem" />.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
	{
		if (!e.Handled)
		{
			e.Handled = true;
			HandleMouseButtonDown(MouseButton.Right);
		}
		base.OnMouseRightButtonDown(e);
	}

	private void HandleMouseButtonDown(MouseButton mouseButton)
	{
		if (Selector.UiGetIsSelectable(this) && Focus())
		{
			ParentListBox?.NotifyListItemClicked(this, mouseButton);
		}
	}

	/// <summary>Called when the mouse enters a <see cref="T:System.Windows.Controls.ListBoxItem" />. </summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseEnter(MouseEventArgs e)
	{
		if (parentNotifyDraggedOperation != null)
		{
			parentNotifyDraggedOperation.Abort();
			parentNotifyDraggedOperation = null;
		}
		if (base.IsMouseOver)
		{
			ListBox parentListBox = ParentListBox;
			if (parentListBox != null && Mouse.LeftButton == MouseButtonState.Pressed)
			{
				parentListBox.NotifyListItemMouseDragged(this);
			}
		}
		base.OnMouseEnter(e);
	}

	/// <summary>Called when the mouse leaves a <see cref="T:System.Windows.Controls.ListBoxItem" />. </summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseLeave(MouseEventArgs e)
	{
		if (parentNotifyDraggedOperation != null)
		{
			parentNotifyDraggedOperation.Abort();
			parentNotifyDraggedOperation = null;
		}
		base.OnMouseLeave(e);
	}

	/// <summary>Called when the visual parent of a list box item changes. </summary>
	/// <param name="oldParent">The previous <see cref="P:System.Windows.FrameworkElement.Parent" /> property of the <see cref="T:System.Windows.Controls.ListBoxItem" />.</param>
	protected internal override void OnVisualParentChanged(DependencyObject oldParent)
	{
		ItemsControl itemsControl = null;
		if (VisualTreeHelper.GetParent(this) == null && base.IsKeyboardFocusWithin)
		{
			itemsControl = ItemsControl.GetItemsOwner(oldParent);
		}
		base.OnVisualParentChanged(oldParent);
		itemsControl?.Focus();
	}
}
