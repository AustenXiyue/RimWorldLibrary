using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Provides a container for a group of commands or controls.  </summary>
[TemplatePart(Name = "PART_ToolBarPanel", Type = typeof(ToolBarPanel))]
[TemplatePart(Name = "PART_ToolBarOverflowPanel", Type = typeof(ToolBarOverflowPanel))]
public class ToolBar : HeaderedItemsControl
{
	private static readonly DependencyPropertyKey OrientationPropertyKey;

	/// <summary>Identifies the <see cref="T:System.Windows.Controls.Orientation" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="T:System.Windows.Controls.Orientation" /> dependency property.</returns>
	public static readonly DependencyProperty OrientationProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolBar.Band" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolBar.Band" /> dependency property.</returns>
	public static readonly DependencyProperty BandProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolBar.BandIndex" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolBar.BandIndex" /> dependency property.</returns>
	public static readonly DependencyProperty BandIndexProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolBar.IsOverflowOpen" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolBar.IsOverflowOpen" /> dependency property.</returns>
	public static readonly DependencyProperty IsOverflowOpenProperty;

	internal static readonly DependencyPropertyKey HasOverflowItemsPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolBar.HasOverflowItems" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolBar.HasOverflowItems" /> dependency property.</returns>
	public static readonly DependencyProperty HasOverflowItemsProperty;

	internal static readonly DependencyPropertyKey IsOverflowItemPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolBar.IsOverflowItem" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolBar.IsOverflowItem" /> attached property.</returns>
	public static readonly DependencyProperty IsOverflowItemProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Controls.ToolBar.OverflowMode" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolBar.OverflowMode" /> attached property.</returns>
	public static readonly DependencyProperty OverflowModeProperty;

	private ToolBarPanel _toolBarPanel;

	private ToolBarOverflowPanel _toolBarOverflowPanel;

	private const string ToolBarPanelTemplateName = "PART_ToolBarPanel";

	private const string ToolBarOverflowPanelTemplateName = "PART_ToolBarOverflowPanel";

	private double _minLength;

	private double _maxLength;

	private static DependencyObjectType _dType;

	/// <summary> Gets the orientation of the <see cref="T:System.Windows.Controls.ToolBar" />.  </summary>
	/// <returns>The toolbar orientation. The default is <see cref="F:System.Windows.Controls.Orientation.Horizontal" />.</returns>
	public Orientation Orientation => (Orientation)GetValue(OrientationProperty);

	/// <summary>Gets or sets a value that indicates where the toolbar should be located in the <see cref="T:System.Windows.Controls.ToolBarTray" />.  </summary>
	/// <returns>The band of the <see cref="T:System.Windows.Controls.ToolBarTray" /> in which the toolbar is positioned. The default is 0.</returns>
	public int Band
	{
		get
		{
			return (int)GetValue(BandProperty);
		}
		set
		{
			SetValue(BandProperty, value);
		}
	}

	/// <summary>Gets or sets the band index number that indicates the position of the toolbar on the band.  </summary>
	/// <returns>The position of a toolbar on the band of a <see cref="T:System.Windows.Controls.ToolBarTray" />.</returns>
	public int BandIndex
	{
		get
		{
			return (int)GetValue(BandIndexProperty);
		}
		set
		{
			SetValue(BandIndexProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.ToolBar" /> overflow area is currently visible.  </summary>
	/// <returns>true if the overflow area is visible; otherwise, false.</returns>
	[Bindable(true)]
	[Browsable(false)]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsOverflowOpen
	{
		get
		{
			return (bool)GetValue(IsOverflowOpenProperty);
		}
		set
		{
			SetValue(IsOverflowOpenProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets a value that indicates whether the toolbar has items that are not visible.  </summary>
	/// <returns>true if there are items on the toolbar that are not visible; otherwise, false. The default is false.</returns>
	public bool HasOverflowItems => (bool)GetValue(HasOverflowItemsProperty);

	internal ToolBarPanel ToolBarPanel
	{
		get
		{
			if (_toolBarPanel == null)
			{
				_toolBarPanel = FindToolBarPanel();
			}
			return _toolBarPanel;
		}
	}

	internal ToolBarOverflowPanel ToolBarOverflowPanel
	{
		get
		{
			if (_toolBarOverflowPanel == null)
			{
				_toolBarOverflowPanel = FindToolBarOverflowPanel();
			}
			return _toolBarOverflowPanel;
		}
	}

	private ToolBarTray ToolBarTray => base.Parent as ToolBarTray;

	internal double MinLength => _minLength;

	internal double MaxLength => _maxLength;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Gets the <see cref="T:System.Windows.Style" /> applied to buttons on a toolbar.</summary>
	/// <returns>A resource key that represents the default style for buttons on the toolbar.</returns>
	public static ResourceKey ButtonStyleKey => SystemResourceKey.ToolBarButtonStyleKey;

	/// <summary>Gets the <see cref="T:System.Windows.Style" /> applied to <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> controls on a <see cref="T:System.Windows.Controls.ToolBar" />.</summary>
	/// <returns>A resource key that represents the default style for toggle buttons on the toolbar.</returns>
	public static ResourceKey ToggleButtonStyleKey => SystemResourceKey.ToolBarToggleButtonStyleKey;

	/// <summary>Gets the <see cref="T:System.Windows.Style" /> applied to separators on a <see cref="T:System.Windows.Controls.ToolBar" />.</summary>
	/// <returns>A resource key that represents the default style for separators on the toolbar.</returns>
	public static ResourceKey SeparatorStyleKey => SystemResourceKey.ToolBarSeparatorStyleKey;

	/// <summary>Gets the <see cref="T:System.Windows.Style" /> applied to check boxes on a <see cref="T:System.Windows.Controls.ToolBar" />.</summary>
	/// <returns>A resource key that represents the default style for check boxes on the <see cref="T:System.Windows.Controls.ToolBar" />.</returns>
	public static ResourceKey CheckBoxStyleKey => SystemResourceKey.ToolBarCheckBoxStyleKey;

	/// <summary>Gets the <see cref="T:System.Windows.Style" /> applied to radio buttons on a toolbar.</summary>
	/// <returns>A resource key that represents the default style for radio buttons on the toolbar.</returns>
	public static ResourceKey RadioButtonStyleKey => SystemResourceKey.ToolBarRadioButtonStyleKey;

	/// <summary>Gets the <see cref="T:System.Windows.Style" /> applied to combo boxes on a <see cref="T:System.Windows.Controls.ToolBar" />.</summary>
	/// <returns>A resource key that represents the default style for combo boxes on the toolbar.</returns>
	public static ResourceKey ComboBoxStyleKey => SystemResourceKey.ToolBarComboBoxStyleKey;

	/// <summary>Gets the <see cref="T:System.Windows.Style" /> applied to text boxes on a <see cref="T:System.Windows.Controls.ToolBar" />.</summary>
	/// <returns>A resource key that represents the default style for text boxes on the toolbar</returns>
	public static ResourceKey TextBoxStyleKey => SystemResourceKey.ToolBarTextBoxStyleKey;

	/// <summary>Gets the <see cref="T:System.Windows.Style" /> applied to menus on a <see cref="T:System.Windows.Controls.ToolBar" />.</summary>
	/// <returns>A resource key that represents the default style for menus on the toolbar.</returns>
	public static ResourceKey MenuStyleKey => SystemResourceKey.ToolBarMenuStyleKey;

	static ToolBar()
	{
		OrientationPropertyKey = DependencyProperty.RegisterAttachedReadOnly("Orientation", typeof(Orientation), typeof(ToolBar), new FrameworkPropertyMetadata(Orientation.Horizontal, null, CoerceOrientation));
		OrientationProperty = OrientationPropertyKey.DependencyProperty;
		BandProperty = DependencyProperty.Register("Band", typeof(int), typeof(ToolBar), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
		BandIndexProperty = DependencyProperty.Register("BandIndex", typeof(int), typeof(ToolBar), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
		IsOverflowOpenProperty = DependencyProperty.Register("IsOverflowOpen", typeof(bool), typeof(ToolBar), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnOverflowOpenChanged, CoerceIsOverflowOpen));
		HasOverflowItemsPropertyKey = DependencyProperty.RegisterReadOnly("HasOverflowItems", typeof(bool), typeof(ToolBar), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		HasOverflowItemsProperty = HasOverflowItemsPropertyKey.DependencyProperty;
		IsOverflowItemPropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsOverflowItem", typeof(bool), typeof(ToolBar), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsOverflowItemProperty = IsOverflowItemPropertyKey.DependencyProperty;
		OverflowModeProperty = DependencyProperty.RegisterAttached("OverflowMode", typeof(OverflowMode), typeof(ToolBar), new FrameworkPropertyMetadata(OverflowMode.AsNeeded, OnOverflowModeChanged), IsValidOverflowMode);
		ToolTipService.IsEnabledProperty.OverrideMetadata(typeof(ToolBar), new FrameworkPropertyMetadata(null, CoerceToolTipIsEnabled));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolBar), new FrameworkPropertyMetadata(typeof(ToolBar)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(ToolBar));
		Control.IsTabStopProperty.OverrideMetadata(typeof(ToolBar), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		UIElement.FocusableProperty.OverrideMetadata(typeof(ToolBar), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(ToolBar), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(ToolBar), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
		KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(ToolBar), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
		FocusManager.IsFocusScopeProperty.OverrideMetadata(typeof(ToolBar), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		EventManager.RegisterClassHandler(typeof(ToolBar), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseButtonDown), handledEventsToo: true);
		EventManager.RegisterClassHandler(typeof(ToolBar), ButtonBase.ClickEvent, new RoutedEventHandler(_OnClick));
		ControlsTraceLogger.AddControl(TelemetryControls.ToolBar);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ToolBar" /> class. </summary>
	public ToolBar()
	{
	}

	private static object CoerceOrientation(DependencyObject d, object value)
	{
		ToolBarTray toolBarTray = ((ToolBar)d).ToolBarTray;
		if (toolBarTray == null)
		{
			return value;
		}
		return toolBarTray.Orientation;
	}

	private static object CoerceIsOverflowOpen(DependencyObject d, object value)
	{
		if ((bool)value)
		{
			ToolBar toolBar = (ToolBar)d;
			if (!toolBar.IsLoaded)
			{
				toolBar.RegisterToOpenOnLoad();
				return BooleanBoxes.FalseBox;
			}
		}
		return value;
	}

	private static object CoerceToolTipIsEnabled(DependencyObject d, object value)
	{
		if (!((ToolBar)d).IsOverflowOpen)
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
			CoerceValue(IsOverflowOpenProperty);
			return (object)null;
		}, null);
	}

	private static void OnOverflowOpenChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
	{
		ToolBar toolBar = (ToolBar)element;
		if ((bool)e.NewValue)
		{
			Mouse.Capture(toolBar, CaptureMode.SubTree);
			toolBar.SetFocusOnToolBarOverflowPanel();
		}
		else
		{
			ToolBarOverflowPanel toolBarOverflowPanel = toolBar.ToolBarOverflowPanel;
			if (toolBarOverflowPanel != null && toolBarOverflowPanel.IsKeyboardFocusWithin)
			{
				Keyboard.Focus(null);
			}
			if (Mouse.Captured == toolBar)
			{
				Mouse.Capture(null);
			}
		}
		toolBar.CoerceValue(ToolTipService.IsEnabledProperty);
	}

	private void SetFocusOnToolBarOverflowPanel()
	{
		base.Dispatcher.BeginInvoke(DispatcherPriority.Input, (DispatcherOperationCallback)delegate
		{
			if (ToolBarOverflowPanel != null)
			{
				if (KeyboardNavigation.IsKeyboardMostRecentInputDevice())
				{
					ToolBarOverflowPanel.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
				}
				else
				{
					ToolBarOverflowPanel.Focus();
				}
			}
			return (object)null;
		}, null);
	}

	internal static void SetIsOverflowItem(DependencyObject element, object value)
	{
		element.SetValue(IsOverflowItemPropertyKey, value);
	}

	/// <summary> Reads the value of the <see cref="P:System.Windows.Controls.ToolBar.IsOverflowItem" /> property from the specified element. </summary>
	/// <returns>The value of the property.</returns>
	/// <param name="element">The element from which to read the property.</param>
	public static bool GetIsOverflowItem(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsOverflowItemProperty);
	}

	private static void OnOverflowModeChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
	{
		if (ItemsControl.ItemsControlFromItemContainer(element) is ToolBar toolBar)
		{
			toolBar.InvalidateLayout();
		}
	}

	private void InvalidateLayout()
	{
		_minLength = 0.0;
		_maxLength = 0.0;
		InvalidateMeasure();
		ToolBarPanel?.InvalidateMeasure();
	}

	private static bool IsValidOverflowMode(object o)
	{
		OverflowMode overflowMode = (OverflowMode)o;
		if (overflowMode != 0 && overflowMode != OverflowMode.Always)
		{
			return overflowMode == OverflowMode.Never;
		}
		return true;
	}

	/// <summary>Writes the value of the <see cref="P:System.Windows.Controls.ToolBar.OverflowMode" /> property to the specified element. </summary>
	/// <param name="element">The element to write the property to.</param>
	/// <param name="mode">The property value to set.</param>
	public static void SetOverflowMode(DependencyObject element, OverflowMode mode)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(OverflowModeProperty, mode);
	}

	/// <summary>Reads the value of the <see cref="P:System.Windows.Controls.ToolBar.OverflowMode" /> property from the specified element. </summary>
	/// <returns>The value of the property.</returns>
	/// <param name="element">The element from which to read the property.</param>
	[AttachedPropertyBrowsableForChildren(IncludeDescendants = true)]
	public static OverflowMode GetOverflowMode(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (OverflowMode)element.GetValue(OverflowModeProperty);
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.ToolBarAutomationPeer" /> implementation for this control, as part of the WPF infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new ToolBarAutomationPeer(this);
	}

	/// <summary>Prepares the specified element to display the specified item. </summary>
	/// <param name="element">The element that will display the item.</param>
	/// <param name="item">The item to display.</param>
	protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		base.PrepareContainerForItemOverride(element, item);
		if (!(element is FrameworkElement frameworkElement))
		{
			return;
		}
		Type type = frameworkElement.GetType();
		ResourceKey resourceKey = null;
		if (type == typeof(Button))
		{
			resourceKey = ButtonStyleKey;
		}
		else if (type == typeof(ToggleButton))
		{
			resourceKey = ToggleButtonStyleKey;
		}
		else if (type == typeof(Separator))
		{
			resourceKey = SeparatorStyleKey;
		}
		else if (type == typeof(CheckBox))
		{
			resourceKey = CheckBoxStyleKey;
		}
		else if (type == typeof(RadioButton))
		{
			resourceKey = RadioButtonStyleKey;
		}
		else if (type == typeof(ComboBox))
		{
			resourceKey = ComboBoxStyleKey;
		}
		else if (type == typeof(TextBox))
		{
			resourceKey = TextBoxStyleKey;
		}
		else if (type == typeof(Menu))
		{
			resourceKey = MenuStyleKey;
		}
		if (resourceKey != null)
		{
			if (frameworkElement.GetValueSource(FrameworkElement.StyleProperty, null, out var _) <= BaseValueSourceInternal.ImplicitReference)
			{
				frameworkElement.SetResourceReference(FrameworkElement.StyleProperty, resourceKey);
			}
			frameworkElement.DefaultStyleKey = resourceKey;
		}
	}

	internal override void OnTemplateChangedInternal(FrameworkTemplate oldTemplate, FrameworkTemplate newTemplate)
	{
		_toolBarPanel = null;
		_toolBarOverflowPanel = null;
		base.OnTemplateChangedInternal(oldTemplate, newTemplate);
	}

	/// <summary> Called when the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> property changes. </summary>
	/// <param name="e">The arguments for the <see cref="E:System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged" /> event.</param>
	protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
	{
		InvalidateLayout();
		base.OnItemsChanged(e);
	}

	/// <summary> Remeasures a <see cref="T:System.Windows.Controls.ToolBar" />. </summary>
	/// <returns>The size of the <see cref="T:System.Windows.Controls.ToolBar" />.</returns>
	/// <param name="constraint">The measurement constraints. A <see cref="T:System.Windows.Controls.ToolBar" /> cannot return a size larger than the constraint.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		Size result = base.MeasureOverride(constraint);
		ToolBarPanel toolBarPanel = ToolBarPanel;
		if (toolBarPanel != null)
		{
			Thickness margin = toolBarPanel.Margin;
			double num = ((toolBarPanel.Orientation != 0) ? Math.Max(0.0, result.Height - toolBarPanel.DesiredSize.Height + margin.Top + margin.Bottom) : Math.Max(0.0, result.Width - toolBarPanel.DesiredSize.Width + margin.Left + margin.Right));
			_minLength = toolBarPanel.MinLength + num;
			_maxLength = toolBarPanel.MaxLength + num;
		}
		return result;
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.LostMouseCapture" /> routed event that occurs when the <see cref="T:System.Windows.Controls.ToolBar" /> loses mouse capture. </summary>
	/// <param name="e">The arguments for the <see cref="E:System.Windows.UIElement.LostMouseCapture" /> event.</param>
	protected override void OnLostMouseCapture(MouseEventArgs e)
	{
		base.OnLostMouseCapture(e);
		if (Mouse.Captured == null)
		{
			Close();
		}
	}

	private ToolBarPanel FindToolBarPanel()
	{
		DependencyObject templateChild = GetTemplateChild("PART_ToolBarPanel");
		ToolBarPanel toolBarPanel = templateChild as ToolBarPanel;
		if (templateChild != null && toolBarPanel == null)
		{
			throw new NotSupportedException(SR.Format(SR.ToolBar_InvalidStyle_ToolBarPanel, templateChild.GetType()));
		}
		return toolBarPanel;
	}

	private ToolBarOverflowPanel FindToolBarOverflowPanel()
	{
		DependencyObject templateChild = GetTemplateChild("PART_ToolBarOverflowPanel");
		ToolBarOverflowPanel toolBarOverflowPanel = templateChild as ToolBarOverflowPanel;
		if (templateChild != null && toolBarOverflowPanel == null)
		{
			throw new NotSupportedException(SR.Format(SR.ToolBar_InvalidStyle_ToolBarOverflowPanel, templateChild.GetType()));
		}
		return toolBarOverflowPanel;
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.KeyDown" /> routed event that occurs when a key is pressed on an item in the <see cref="T:System.Windows.Controls.ToolBar" />. </summary>
	/// <param name="e">The arguments for the <see cref="E:System.Windows.UIElement.KeyDown" /> event.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		UIElement uIElement = null;
		if (e.Source is UIElement uIElement2 && ItemsControl.ItemsControlFromItemContainer(uIElement2) == this && VisualTreeHelper.GetParent(uIElement2) is Panel reference)
		{
			switch (e.Key)
			{
			case Key.Home:
				uIElement = VisualTreeHelper.GetChild(reference, 0) as UIElement;
				break;
			case Key.End:
				uIElement = VisualTreeHelper.GetChild(reference, VisualTreeHelper.GetChildrenCount(reference) - 1) as UIElement;
				break;
			case Key.Escape:
			{
				ToolBarOverflowPanel toolBarOverflowPanel = ToolBarOverflowPanel;
				if (toolBarOverflowPanel != null && toolBarOverflowPanel.IsKeyboardFocusWithin)
				{
					MoveFocus(new TraversalRequest(FocusNavigationDirection.Last));
				}
				else
				{
					Keyboard.Focus(null);
				}
				Close();
				break;
			}
			}
			if (uIElement != null && uIElement.Focus())
			{
				e.Handled = true;
			}
		}
		if (!e.Handled)
		{
			base.OnKeyDown(e);
		}
	}

	private static void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
	{
		ToolBar toolBar = (ToolBar)sender;
		if (!e.Handled)
		{
			toolBar.Close();
			e.Handled = true;
		}
	}

	private static void _OnClick(object e, RoutedEventArgs args)
	{
		ToolBar toolBar = (ToolBar)e;
		ButtonBase buttonBase = args.OriginalSource as ButtonBase;
		if (toolBar.IsOverflowOpen && buttonBase != null && buttonBase.Parent == toolBar)
		{
			toolBar.Close();
		}
	}

	internal override void OnAncestorChanged()
	{
		CoerceValue(OrientationProperty);
	}

	private void Close()
	{
		SetCurrentValueInternal(IsOverflowOpenProperty, BooleanBoxes.FalseBox);
	}
}
