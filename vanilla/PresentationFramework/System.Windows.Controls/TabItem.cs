using System.ComponentModel;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using MS.Internal;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Represents a selectable item inside a <see cref="T:System.Windows.Controls.TabControl" />. </summary>
[DefaultEvent("IsSelectedChanged")]
public class TabItem : HeaderedContentControl
{
	[Flags]
	private enum BoolField
	{
		SetFocusOnContent = 0x10,
		SettingFocus = 0x20,
		DefaultValue = 0
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TabItem.IsSelected" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TabItem.IsSelected" /> dependency property.</returns>
	public static readonly DependencyProperty IsSelectedProperty;

	private static readonly DependencyPropertyKey TabStripPlacementPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TabItem.TabStripPlacement" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TabItem.TabStripPlacement" /> dependency property.</returns>
	public static readonly DependencyProperty TabStripPlacementProperty;

	private BoolField _tabItemBoolFieldStore;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.TabItem" /> is selected.  </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.TabItem" /> is selected; otherwise, false. The default is false.</returns>
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

	/// <summary>Gets the tab strip placement.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.Controls.Dock" /> values. The default is <see cref="F:System.Windows.Controls.Dock.Top" />. </returns>
	public Dock TabStripPlacement => (Dock)GetValue(TabStripPlacementProperty);

	private TabControl TabControlParent => ItemsControl.ItemsControlFromItemContainer(this) as TabControl;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.TabItem" /> class. </summary>
	public TabItem()
	{
	}

	static TabItem()
	{
		IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(typeof(TabItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnIsSelectedChanged));
		TabStripPlacementPropertyKey = DependencyProperty.RegisterReadOnly("TabStripPlacement", typeof(Dock), typeof(TabItem), new FrameworkPropertyMetadata(Dock.Top, null, CoerceTabStripPlacement));
		TabStripPlacementProperty = TabStripPlacementPropertyKey.DependencyProperty;
		EventManager.RegisterClassHandler(typeof(TabItem), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(OnAccessKeyPressed));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TabItem), new FrameworkPropertyMetadata(typeof(TabItem)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(TabItem));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(TabItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(TabItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(TabItem), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		UIElement.IsMouseOverPropertyKey.OverrideMetadata(typeof(TabItem), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		AutomationProperties.IsOffscreenBehaviorProperty.OverrideMetadata(typeof(TabItem), new FrameworkPropertyMetadata(IsOffscreenBehavior.FromClip));
	}

	private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TabItem tabItem = d as TabItem;
		bool flag = (bool)e.NewValue;
		tabItem.TabControlParent?.RaiseIsSelectedChangedAutomationEvent(tabItem, flag);
		if (flag)
		{
			tabItem.OnSelected(new RoutedEventArgs(Selector.SelectedEvent, tabItem));
		}
		else
		{
			tabItem.OnUnselected(new RoutedEventArgs(Selector.UnselectedEvent, tabItem));
		}
		if (flag)
		{
			Binding binding = new Binding("Margin");
			binding.Source = tabItem;
			BindingOperations.SetBinding(tabItem, KeyboardNavigation.DirectionalNavigationMarginProperty, binding);
		}
		else
		{
			BindingOperations.ClearBinding(tabItem, KeyboardNavigation.DirectionalNavigationMarginProperty);
		}
		tabItem.UpdateVisualState();
	}

	/// <summary>Called to indicate that the <see cref="P:System.Windows.Controls.TabItem.IsSelected" /> property has changed to true. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Controls.Primitives.Selector.Selected" /> event.</param>
	protected virtual void OnSelected(RoutedEventArgs e)
	{
		HandleIsSelectedChanged(newValue: true, e);
	}

	/// <summary> Called to indicate that the <see cref="P:System.Windows.Controls.TabItem.IsSelected" /> property has changed to false. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Controls.Primitives.Selector.Unselected" /> event.</param>
	protected virtual void OnUnselected(RoutedEventArgs e)
	{
		HandleIsSelectedChanged(newValue: false, e);
	}

	private void HandleIsSelectedChanged(bool newValue, RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	private static object CoerceTabStripPlacement(DependencyObject d, object value)
	{
		TabControl tabControlParent = ((TabItem)d).TabControlParent;
		if (tabControlParent == null)
		{
			return value;
		}
		return tabControlParent.TabStripPlacement;
	}

	internal override void OnAncestorChanged()
	{
		CoerceValue(TabStripPlacementProperty);
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
		if (IsSelected)
		{
			VisualStates.GoToState(this, useTransitions, "Selected", "Unselected");
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

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.TabItemAutomationPeer" /> implementation for this control, as part of the WPF automation infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new TabItemWrapperAutomationPeer(this);
	}

	/// <summary> Responds to the <see cref="E:System.Windows.ContentElement.MouseLeftButtonDown" /> event. </summary>
	/// <param name="e">Provides data for <see cref="T:System.Windows.Input.MouseButtonEventArgs" />.</param>
	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		if ((e.Source == this || !IsSelected) && SetFocus())
		{
			e.Handled = true;
		}
		base.OnMouseLeftButtonDown(e);
	}

	/// <summary> Announces that the keyboard is focused on this element. </summary>
	/// <param name="e">Keyboard input event arguments.</param>
	protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		base.OnPreviewGotKeyboardFocus(e);
		if (e.Handled || e.NewFocus != this)
		{
			return;
		}
		if (FrameworkAppContextSwitches.SelectionPropertiesCanLagBehindSelectionChangedEvent)
		{
			if (IsSelected || TabControlParent == null)
			{
				return;
			}
			SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.TrueBox);
			if (e.OldFocus != Keyboard.FocusedElement)
			{
				e.Handled = true;
			}
			else
			{
				if (!GetBoolField(BoolField.SetFocusOnContent))
				{
					return;
				}
				TabControl tabControlParent = TabControlParent;
				if (tabControlParent == null)
				{
					return;
				}
				ContentPresenter selectedContentPresenter = tabControlParent.SelectedContentPresenter;
				if (selectedContentPresenter != null)
				{
					tabControlParent.UpdateLayout();
					if (selectedContentPresenter.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)))
					{
						e.Handled = true;
					}
				}
			}
			return;
		}
		if (!IsSelected && TabControlParent != null)
		{
			SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.TrueBox);
			if (e.OldFocus != Keyboard.FocusedElement)
			{
				e.Handled = true;
			}
		}
		if (e.Handled || !GetBoolField(BoolField.SetFocusOnContent))
		{
			return;
		}
		TabControl tabControlParent2 = TabControlParent;
		if (tabControlParent2 == null)
		{
			return;
		}
		ContentPresenter selectedContentPresenter2 = tabControlParent2.SelectedContentPresenter;
		if (selectedContentPresenter2 != null)
		{
			tabControlParent2.UpdateLayout();
			if (selectedContentPresenter2.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)))
			{
				e.Handled = true;
			}
		}
	}

	/// <summary> Responds when an <see cref="P:System.Windows.Controls.AccessText.AccessKey" /> for a <see cref="T:System.Windows.Controls.TabControl" /> is called. </summary>
	/// <param name="e">Provides data for <see cref="T:System.Windows.Input.AccessKeyEventArgs" />.</param>
	protected override void OnAccessKey(AccessKeyEventArgs e)
	{
		SetFocus();
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property changes. </summary>
	/// <param name="oldContent">Old value of the content property.</param>
	/// <param name="newContent">New value of the content property.</param>
	protected override void OnContentChanged(object oldContent, object newContent)
	{
		base.OnContentChanged(oldContent, newContent);
		if (!IsSelected)
		{
			return;
		}
		TabControl tabControlParent = TabControlParent;
		if (tabControlParent != null)
		{
			if (newContent == BindingExpressionBase.DisconnectedItem)
			{
				newContent = null;
			}
			tabControlParent.SelectedContent = newContent;
		}
	}

	/// <summary> Called when the <see cref="P:System.Windows.Controls.TabControl.ContentTemplate" /> property changes. </summary>
	/// <param name="oldContentTemplate">Old value of the content template property.</param>
	/// <param name="newContentTemplate">New value of the content template property.</param>
	protected override void OnContentTemplateChanged(DataTemplate oldContentTemplate, DataTemplate newContentTemplate)
	{
		base.OnContentTemplateChanged(oldContentTemplate, newContentTemplate);
		if (IsSelected)
		{
			TabControl tabControlParent = TabControlParent;
			if (tabControlParent != null)
			{
				tabControlParent.SelectedContentTemplate = newContentTemplate;
			}
		}
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.TabControl.ContentTemplateSelector" /> property changes. </summary>
	/// <param name="oldContentTemplateSelector">Old value of the content template selector.</param>
	/// <param name="newContentTemplateSelector">New value of the content template selector.</param>
	protected override void OnContentTemplateSelectorChanged(DataTemplateSelector oldContentTemplateSelector, DataTemplateSelector newContentTemplateSelector)
	{
		base.OnContentTemplateSelectorChanged(oldContentTemplateSelector, newContentTemplateSelector);
		if (IsSelected)
		{
			TabControl tabControlParent = TabControlParent;
			if (tabControlParent != null)
			{
				tabControlParent.SelectedContentTemplateSelector = newContentTemplateSelector;
			}
		}
	}

	private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
	{
		if (!e.Handled && e.Scope == null)
		{
			TabItem tabItem = sender as TabItem;
			if (e.Target == null)
			{
				e.Target = tabItem;
			}
			else if (!tabItem.IsSelected)
			{
				e.Scope = tabItem;
				e.Handled = true;
			}
		}
	}

	internal bool SetFocus()
	{
		bool result = false;
		if (!GetBoolField(BoolField.SettingFocus))
		{
			TabItem tabItem = Keyboard.FocusedElement as TabItem;
			bool flag = (FrameworkAppContextSwitches.SelectionPropertiesCanLagBehindSelectionChangedEvent || !base.IsKeyboardFocusWithin) && (tabItem == this || tabItem == null || tabItem.TabControlParent != TabControlParent);
			SetBoolField(BoolField.SettingFocus, value: true);
			SetBoolField(BoolField.SetFocusOnContent, flag);
			try
			{
				result = Focus() || flag;
			}
			finally
			{
				SetBoolField(BoolField.SettingFocus, value: false);
				SetBoolField(BoolField.SetFocusOnContent, value: false);
			}
		}
		return result;
	}

	private bool GetBoolField(BoolField field)
	{
		return (_tabItemBoolFieldStore & field) != 0;
	}

	private void SetBoolField(BoolField field, bool value)
	{
		if (value)
		{
			_tabItemBoolFieldStore |= field;
		}
		else
		{
			_tabItemBoolFieldStore &= ~field;
		}
	}
}
