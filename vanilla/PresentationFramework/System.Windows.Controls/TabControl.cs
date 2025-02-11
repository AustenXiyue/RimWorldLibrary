using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a control that contains multiple items that share the same space on the screen. </summary>
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(TabItem))]
[TemplatePart(Name = "PART_SelectedContentHost", Type = typeof(ContentPresenter))]
public class TabControl : Selector
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TabControl.TabStripPlacement" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TabControl.TabStripPlacement" /> dependency property.</returns>
	public static readonly DependencyProperty TabStripPlacementProperty;

	private static readonly DependencyPropertyKey SelectedContentPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TabControl.SelectedContent" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TabControl.SelectedContent" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedContentProperty;

	private static readonly DependencyPropertyKey SelectedContentTemplatePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TabControl.SelectedContentTemplate" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TabControl.SelectedContentTemplate" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedContentTemplateProperty;

	private static readonly DependencyPropertyKey SelectedContentTemplateSelectorPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TabControl.SelectedContentTemplateSelector" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TabControl.SelectedContentTemplateSelector" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedContentTemplateSelectorProperty;

	private static readonly DependencyPropertyKey SelectedContentStringFormatPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TabControl.SelectedContentStringFormat" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TabControl.SelectedContentStringFormat" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedContentStringFormatProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TabControl.ContentTemplate" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TabControl.ContentTemplate" /> dependency property.</returns>
	public static readonly DependencyProperty ContentTemplateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TabControl.ContentTemplateSelector" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TabControl.ContentTemplateSelector" /> dependency property.</returns>
	public static readonly DependencyProperty ContentTemplateSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TabControl.ContentStringFormat" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TabControl.ContentStringFormat" /> dependency property.</returns>
	public static readonly DependencyProperty ContentStringFormatProperty;

	private const string SelectedContentHostTemplateName = "PART_SelectedContentHost";

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets how tab headers align relative to the tab content. </summary>
	/// <returns>The alignment of tab headers relative to tab content. The default is <see cref="F:System.Windows.Controls.Dock.Top" />.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public Dock TabStripPlacement
	{
		get
		{
			return (Dock)GetValue(TabStripPlacementProperty);
		}
		set
		{
			SetValue(TabStripPlacementProperty, value);
		}
	}

	/// <summary>Gets the content of the currently selected <see cref="T:System.Windows.Controls.TabItem" />.</summary>
	/// <returns>The content of the currently selected <see cref="T:System.Windows.Controls.TabItem" />. The default is null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public object SelectedContent
	{
		get
		{
			return GetValue(SelectedContentProperty);
		}
		internal set
		{
			SetValue(SelectedContentPropertyKey, value);
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.DataTemplate" /> of the currently selected item.</summary>
	/// <returns>The <see cref="T:System.Windows.DataTemplate" /> of the selected item.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DataTemplate SelectedContentTemplate
	{
		get
		{
			return (DataTemplate)GetValue(SelectedContentTemplateProperty);
		}
		internal set
		{
			SetValue(SelectedContentTemplatePropertyKey, value);
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Controls.DataTemplateSelector" /> of the currently selected item. </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.DataTemplateSelector" /> of the currently selected item. The default is null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DataTemplateSelector SelectedContentTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(SelectedContentTemplateSelectorProperty);
		}
		internal set
		{
			SetValue(SelectedContentTemplateSelectorPropertyKey, value);
		}
	}

	/// <summary>Gets a composite string that specifies how to format the content of the currently selected <see cref="T:System.Windows.Controls.TabItem" /> if it is displayed as a string.</summary>
	/// <returns>A composite string that specifies how to format the content of the currently selected <see cref="T:System.Windows.Controls.TabItem" /> if it is displayed as a string.</returns>
	public string SelectedContentStringFormat
	{
		get
		{
			return (string)GetValue(SelectedContentStringFormatProperty);
		}
		internal set
		{
			SetValue(SelectedContentStringFormatPropertyKey, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.DataTemplate" /> to apply to any <see cref="T:System.Windows.Controls.TabItem" /> that does not have a <see cref="P:System.Windows.Controls.ContentControl.ContentTemplate" /> or <see cref="P:System.Windows.Controls.ContentControl.ContentTemplateSelector" /> property defined. </summary>
	/// <returns>The <see cref="T:System.Windows.DataTemplate" /> to apply to any <see cref="T:System.Windows.Controls.TabItem" /> that does not have a <see cref="P:System.Windows.Controls.ContentControl.ContentTemplate" /> or <see cref="P:System.Windows.Controls.ContentControl.ContentTemplateSelector" /> property defined. The default is null.</returns>
	public DataTemplate ContentTemplate
	{
		get
		{
			return (DataTemplate)GetValue(ContentTemplateProperty);
		}
		set
		{
			SetValue(ContentTemplateProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Controls.DataTemplateSelector" /> that provides custom logic for choosing the template that is used to display the content of the control.</summary>
	/// <returns>A <see cref="P:System.Windows.Controls.TabControl.ContentTemplateSelector" />. The default is null.</returns>
	public DataTemplateSelector ContentTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(ContentTemplateSelectorProperty);
		}
		set
		{
			SetValue(ContentTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets a composite string that specifies how to format the contents of the <see cref="T:System.Windows.Controls.TabItem" /> objects if they are displayed as strings.</summary>
	/// <returns>A composite string that specifies how to format the contents of the <see cref="T:System.Windows.Controls.TabItem" /> objects if they are displayed as strings.</returns>
	public string ContentStringFormat
	{
		get
		{
			return (string)GetValue(ContentStringFormatProperty);
		}
		set
		{
			SetValue(ContentStringFormatProperty, value);
		}
	}

	internal ContentPresenter SelectedContentPresenter => GetTemplateChild("PART_SelectedContentHost") as ContentPresenter;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static TabControl()
	{
		TabStripPlacementProperty = DependencyProperty.Register("TabStripPlacement", typeof(Dock), typeof(TabControl), new FrameworkPropertyMetadata(Dock.Top, OnTabStripPlacementPropertyChanged), DockPanel.IsValidDock);
		SelectedContentPropertyKey = DependencyProperty.RegisterReadOnly("SelectedContent", typeof(object), typeof(TabControl), new FrameworkPropertyMetadata((object)null));
		SelectedContentProperty = SelectedContentPropertyKey.DependencyProperty;
		SelectedContentTemplatePropertyKey = DependencyProperty.RegisterReadOnly("SelectedContentTemplate", typeof(DataTemplate), typeof(TabControl), new FrameworkPropertyMetadata((object)null));
		SelectedContentTemplateProperty = SelectedContentTemplatePropertyKey.DependencyProperty;
		SelectedContentTemplateSelectorPropertyKey = DependencyProperty.RegisterReadOnly("SelectedContentTemplateSelector", typeof(DataTemplateSelector), typeof(TabControl), new FrameworkPropertyMetadata((object)null));
		SelectedContentTemplateSelectorProperty = SelectedContentTemplateSelectorPropertyKey.DependencyProperty;
		SelectedContentStringFormatPropertyKey = DependencyProperty.RegisterReadOnly("SelectedContentStringFormat", typeof(string), typeof(TabControl), new FrameworkPropertyMetadata((object)null));
		SelectedContentStringFormatProperty = SelectedContentStringFormatPropertyKey.DependencyProperty;
		ContentTemplateProperty = DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(TabControl), new FrameworkPropertyMetadata((object)null));
		ContentTemplateSelectorProperty = DependencyProperty.Register("ContentTemplateSelector", typeof(DataTemplateSelector), typeof(TabControl), new FrameworkPropertyMetadata((object)null));
		ContentStringFormatProperty = DependencyProperty.Register("ContentStringFormat", typeof(string), typeof(TabControl), new FrameworkPropertyMetadata((object)null));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TabControl), new FrameworkPropertyMetadata(typeof(TabControl)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(TabControl));
		Control.IsTabStopProperty.OverrideMetadata(typeof(TabControl), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(TabControl), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(TabControl), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		ControlsTraceLogger.AddControl(TelemetryControls.TabControl);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.TabControl" />.class. </summary>
	public TabControl()
	{
	}

	private static void OnTabStripPlacementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TabControl tabControl = (TabControl)d;
		ItemCollection items = tabControl.Items;
		for (int i = 0; i < items.Count; i++)
		{
			if (tabControl.ItemContainerGenerator.ContainerFromIndex(i) is TabItem tabItem)
			{
				tabItem.CoerceValue(TabItem.TabStripPlacementProperty);
			}
		}
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (!base.IsEnabled)
		{
			VisualStates.GoToState(this, useTransitions, "Disabled", "Normal");
		}
		else
		{
			VisualStateManager.GoToState(this, "Normal", useTransitions);
		}
		base.ChangeVisualState(useTransitions);
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.TabControlAutomationPeer" /> implementation for this control, as part of the WPF automation infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new TabControlAutomationPeer(this);
	}

	/// <summary>Called when <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> is set to true.</summary>
	/// <param name="e">Provides data for the <see cref="E:System.Windows.FrameworkElement.Initialized" /> event.</param>
	protected override void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);
		base.CanSelectMultiple = false;
		base.ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
	}

	/// <summary>Called when <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" /> is called.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		UpdateSelectedContent();
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.Primitives.Selector.SelectionChanged" /> routed event. </summary>
	/// <param name="e">Provides data for <see cref="T:System.Windows.Controls.SelectionChangedEventArgs" />. </param>
	protected override void OnSelectionChanged(SelectionChangedEventArgs e)
	{
		if (FrameworkAppContextSwitches.SelectionPropertiesCanLagBehindSelectionChangedEvent)
		{
			base.OnSelectionChanged(e);
			if (base.IsKeyboardFocusWithin)
			{
				GetSelectedTabItem()?.SetFocus();
			}
			UpdateSelectedContent();
		}
		else
		{
			bool isKeyboardFocusWithin = base.IsKeyboardFocusWithin;
			UpdateSelectedContent();
			if (isKeyboardFocusWithin)
			{
				GetSelectedTabItem()?.SetFocus();
			}
			base.OnSelectionChanged(e);
		}
		if ((AutomationPeer.ListenerExists(AutomationEvents.SelectionPatternOnInvalidated) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementAddedToSelection) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection)) && UIElementAutomationPeer.CreatePeerForElement(this) is TabControlAutomationPeer tabControlAutomationPeer)
		{
			tabControlAutomationPeer.RaiseSelectionEvents(e);
		}
	}

	/// <summary>Called to update the current selection when items change.</summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Controls.ItemContainerGenerator.ItemsChanged" /> event.</param>
	protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
	{
		base.OnItemsChanged(e);
		if (e.Action == NotifyCollectionChangedAction.Remove && base.SelectedIndex == -1)
		{
			int num = e.OldStartingIndex + 1;
			if (num > base.Items.Count)
			{
				num = 0;
			}
			FindNextTabItem(num, -1)?.SetCurrentValueInternal(TabItem.IsSelectedProperty, BooleanBoxes.TrueBox);
		}
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.ContentElement.KeyDown" /> routed event that occurs when the user presses a key.</summary>
	/// <param name="e">Provides data for <see cref="T:System.Windows.Input.KeyEventArgs" />.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		TabItem tabItem = null;
		int direction = 0;
		int startIndex = -1;
		switch (e.Key)
		{
		case Key.Tab:
			if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
			{
				startIndex = base.ItemContainerGenerator.IndexFromContainer(base.ItemContainerGenerator.ContainerFromItem(base.SelectedItem));
				direction = (((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift) ? 1 : (-1));
			}
			break;
		case Key.Home:
			direction = 1;
			startIndex = -1;
			break;
		case Key.End:
			direction = -1;
			startIndex = base.Items.Count;
			break;
		}
		tabItem = FindNextTabItem(startIndex, direction);
		if (tabItem != null && tabItem != base.SelectedItem)
		{
			e.Handled = tabItem.SetFocus();
		}
		if (!e.Handled)
		{
			base.OnKeyDown(e);
		}
	}

	private TabItem FindNextTabItem(int startIndex, int direction)
	{
		TabItem result = null;
		if (direction != 0)
		{
			int num = startIndex;
			for (int i = 0; i < base.Items.Count; i++)
			{
				num += direction;
				if (num >= base.Items.Count)
				{
					num = 0;
				}
				else if (num < 0)
				{
					num = base.Items.Count - 1;
				}
				if (base.ItemContainerGenerator.ContainerFromIndex(num) is TabItem { IsEnabled: not false, Visibility: Visibility.Visible } tabItem)
				{
					result = tabItem;
					break;
				}
			}
		}
		return result;
	}

	/// <summary>Determines if the specified item is (or is eligible to be) its own ItemContainer. </summary>
	/// <returns>Returns true if the item is its own ItemContainer; otherwise, false.</returns>
	/// <param name="item">Specified item.</param>
	protected override bool IsItemItsOwnContainerOverride(object item)
	{
		return item is TabItem;
	}

	/// <summary>Creates or identifies the element used to display the specified item.</summary>
	/// <returns>The element used to display the specified item.</returns>
	protected override DependencyObject GetContainerForItemOverride()
	{
		return new TabItem();
	}

	private void OnGeneratorStatusChanged(object sender, EventArgs e)
	{
		if (base.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
		{
			if (base.HasItems && _selectedItems.Count == 0)
			{
				SetCurrentValueInternal(Selector.SelectedIndexProperty, 0);
			}
			UpdateSelectedContent();
		}
	}

	private TabItem GetSelectedTabItem()
	{
		object selectedItem = base.SelectedItem;
		if (selectedItem != null)
		{
			TabItem tabItem = selectedItem as TabItem;
			if (tabItem == null)
			{
				tabItem = base.ItemContainerGenerator.ContainerFromIndex(base.SelectedIndex) as TabItem;
				if (tabItem == null || !ItemsControl.EqualsEx(selectedItem, base.ItemContainerGenerator.ItemFromContainer(tabItem)))
				{
					tabItem = base.ItemContainerGenerator.ContainerFromItem(selectedItem) as TabItem;
				}
			}
			return tabItem;
		}
		return null;
	}

	private void UpdateSelectedContent()
	{
		if (base.SelectedIndex < 0)
		{
			SelectedContent = null;
			SelectedContentTemplate = null;
			SelectedContentTemplateSelector = null;
			SelectedContentStringFormat = null;
			return;
		}
		TabItem selectedTabItem = GetSelectedTabItem();
		if (selectedTabItem != null)
		{
			if (VisualTreeHelper.GetParent(selectedTabItem) is FrameworkElement frameworkElement)
			{
				KeyboardNavigation.SetTabOnceActiveElement(frameworkElement, selectedTabItem);
				KeyboardNavigation.SetTabOnceActiveElement(this, frameworkElement);
			}
			SelectedContent = selectedTabItem.Content;
			ContentPresenter selectedContentPresenter = SelectedContentPresenter;
			if (selectedContentPresenter != null)
			{
				selectedContentPresenter.HorizontalAlignment = selectedTabItem.HorizontalContentAlignment;
				selectedContentPresenter.VerticalAlignment = selectedTabItem.VerticalContentAlignment;
			}
			if (selectedTabItem.ContentTemplate != null || selectedTabItem.ContentTemplateSelector != null || selectedTabItem.ContentStringFormat != null)
			{
				SelectedContentTemplate = selectedTabItem.ContentTemplate;
				SelectedContentTemplateSelector = selectedTabItem.ContentTemplateSelector;
				SelectedContentStringFormat = selectedTabItem.ContentStringFormat;
			}
			else
			{
				SelectedContentTemplate = ContentTemplate;
				SelectedContentTemplateSelector = ContentTemplateSelector;
				SelectedContentStringFormat = ContentStringFormat;
			}
		}
	}
}
