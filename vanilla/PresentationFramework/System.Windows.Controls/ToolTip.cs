using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Represents a control that creates a pop-up window that displays information for an element in the interface. </summary>
[DefaultEvent("Opened")]
[Localizability(LocalizationCategory.ToolTip)]
public class ToolTip : ContentControl
{
	internal static readonly DependencyProperty FromKeyboardProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTip.HorizontalOffset" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTip.HorizontalOffset" /> dependency property.</returns>
	public static readonly DependencyProperty HorizontalOffsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTip.VerticalOffset" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTip.VerticalOffset" /> dependency property.</returns>
	public static readonly DependencyProperty VerticalOffsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTip.IsOpen" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTip.IsOpen" /> dependency property.</returns>
	public static readonly DependencyProperty IsOpenProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTip.HasDropShadow" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTip.HasDropShadow" /> dependency property.</returns>
	public static readonly DependencyProperty HasDropShadowProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTip.PlacementTarget" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTip.PlacementTarget" /> dependency property.</returns>
	public static readonly DependencyProperty PlacementTargetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTip.PlacementRectangle" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTip.PlacementRectangle" /> dependency property.</returns>
	public static readonly DependencyProperty PlacementRectangleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTip.Placement" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTip.Placement" /> dependency property.</returns>
	public static readonly DependencyProperty PlacementProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTip.CustomPopupPlacementCallback" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTip.CustomPopupPlacementCallback" /> dependency property.</returns>
	public static readonly DependencyProperty CustomPopupPlacementCallbackProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTip.StaysOpen" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTip.StaysOpen" /> dependency property.</returns>
	public static readonly DependencyProperty StaysOpenProperty;

	public static readonly DependencyProperty ShowsToolTipOnKeyboardFocusProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.ToolTip.Opened" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.ToolTip.Opened" /> routed event.</returns>
	public static readonly RoutedEvent OpenedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.ToolTip.Closed" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.ToolTip.Closed" /> routed event.</returns>
	public static readonly RoutedEvent ClosedEvent;

	private Popup _parentPopup;

	private static DependencyObjectType _dType;

	[Bindable(true)]
	[Category("Behavior")]
	internal bool FromKeyboard
	{
		get
		{
			return (bool)GetValue(FromKeyboardProperty);
		}
		set
		{
			SetValue(FromKeyboardProperty, value);
		}
	}

	/// <summary>Get or sets the horizontal distance between the target origin and the popup alignment point. </summary>
	/// <returns>The horizontal distance between the target origin and the popup alignment point. For information about the target origin and popup alignment point, see Popup Placement Behavior. The default is 0.</returns>
	[TypeConverter(typeof(LengthConverter))]
	[Bindable(true)]
	[Category("Layout")]
	public double HorizontalOffset
	{
		get
		{
			return (double)GetValue(HorizontalOffsetProperty);
		}
		set
		{
			SetValue(HorizontalOffsetProperty, value);
		}
	}

	/// <summary>Get or sets the vertical distance between the target origin and the popup alignment point. </summary>
	/// <returns>The vertical distance between the target origin and the popup alignment point. For information about the target origin and popup alignment point, see Popup Placement Behavior. The default is 0.</returns>
	[TypeConverter(typeof(LengthConverter))]
	[Bindable(true)]
	[Category("Layout")]
	public double VerticalOffset
	{
		get
		{
			return (double)GetValue(VerticalOffsetProperty);
		}
		set
		{
			SetValue(VerticalOffsetProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a <see cref="T:System.Windows.Controls.ToolTip" /> is visible.  </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.ToolTip" /> is visible; otherwise, false. The default is false.</returns>
	[Bindable(true)]
	[Browsable(false)]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsOpen
	{
		get
		{
			return (bool)GetValue(IsOpenProperty);
		}
		set
		{
			SetValue(IsOpenProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the control has a drop shadow.  </summary>
	/// <returns>true if the control has a drop shadow; otherwise, false. The default is false.</returns>
	public bool HasDropShadow
	{
		get
		{
			return (bool)GetValue(HasDropShadowProperty);
		}
		set
		{
			SetValue(HasDropShadowProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.UIElement" /> relative to which the <see cref="T:System.Windows.Controls.ToolTip" /> is positioned when it opens.  </summary>
	/// <returns>The <see cref="T:System.Windows.UIElement" /> that is the logical parent of the <see cref="T:System.Windows.Controls.ToolTip" /> control. The default is null.</returns>
	[Bindable(true)]
	[Category("Layout")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public UIElement PlacementTarget
	{
		get
		{
			return (UIElement)GetValue(PlacementTargetProperty);
		}
		set
		{
			SetValue(PlacementTargetProperty, value);
		}
	}

	/// <summary>Gets or sets the rectangular area relative to which the <see cref="T:System.Windows.Controls.ToolTip" /> control is positioned when it opens.  </summary>
	/// <returns>The <see cref="T:System.Windows.Rect" /> structure that defines the rectangle that is used to position the <see cref="T:System.Windows.Controls.ToolTip" /> control. The default is <see cref="P:System.Windows.Rect.Empty" />.</returns>
	[Bindable(true)]
	[Category("Layout")]
	public Rect PlacementRectangle
	{
		get
		{
			return (Rect)GetValue(PlacementRectangleProperty);
		}
		set
		{
			SetValue(PlacementRectangleProperty, value);
		}
	}

	/// <summary>Gets or sets the orientation of the <see cref="T:System.Windows.Controls.ToolTip" /> control when it opens, and specifies how the <see cref="T:System.Windows.Controls.ToolTip" /> control behaves when it overlaps screen boundaries.   </summary>
	/// <returns>A <see cref="T:System.Windows.Controls.Primitives.PlacementMode" /> enumeration value that determines the orientation of the <see cref="T:System.Windows.Controls.ToolTip" /> control when it opens, and that specifies how the control interacts with screen boundaries. The default is <see cref="F:System.Windows.Controls.Primitives.PlacementMode.Mouse" />.</returns>
	[Bindable(true)]
	[Category("Layout")]
	public PlacementMode Placement
	{
		get
		{
			return (PlacementMode)GetValue(PlacementProperty);
		}
		set
		{
			SetValue(PlacementProperty, value);
		}
	}

	/// <summary>Gets or sets the delegate handler method to use to position the <see cref="T:System.Windows.Controls.ToolTip" />.  </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.Primitives.CustomPopupPlacementCallback" /> delegate method that provides placement information for the <see cref="T:System.Windows.Controls.ToolTip" />. The default is null.</returns>
	[Bindable(false)]
	[Category("Layout")]
	public CustomPopupPlacementCallback CustomPopupPlacementCallback
	{
		get
		{
			return (CustomPopupPlacementCallback)GetValue(CustomPopupPlacementCallbackProperty);
		}
		set
		{
			SetValue(CustomPopupPlacementCallbackProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether an open <see cref="T:System.Windows.Controls.ToolTip" /> remains open until the user clicks the mouse when the mouse is not over the <see cref="T:System.Windows.Controls.ToolTip" />.  </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.ToolTip" /> stays open until it is closed by the user clicking the mouse button outside the <see cref="T:System.Windows.Controls.ToolTip" />; otherwise, false. The default is true.</returns>
	/// <exception cref="T:System.NotSupportedException">Occurs when <see cref="P:System.Windows.Controls.ToolTip.StaysOpen" /> is set to false when a tooltip is not open.</exception>
	[Bindable(true)]
	[Category("Behavior")]
	public bool StaysOpen
	{
		get
		{
			return (bool)GetValue(StaysOpenProperty);
		}
		set
		{
			SetValue(StaysOpenProperty, value);
		}
	}

	[Bindable(true)]
	[Category("Behavior")]
	public bool? ShowsToolTipOnKeyboardFocus
	{
		get
		{
			return (bool?)GetValue(ShowsToolTipOnKeyboardFocusProperty);
		}
		set
		{
			SetValue(ShowsToolTipOnKeyboardFocusProperty, BooleanBoxes.Box(value));
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Occurs when a <see cref="T:System.Windows.Controls.ToolTip" /> becomes visible.</summary>
	public event RoutedEventHandler Opened
	{
		add
		{
			AddHandler(OpenedEvent, value);
		}
		remove
		{
			RemoveHandler(OpenedEvent, value);
		}
	}

	/// <summary>Occurs when a <see cref="T:System.Windows.Controls.ToolTip" /> is closed and is no longer visible. </summary>
	public event RoutedEventHandler Closed
	{
		add
		{
			AddHandler(ClosedEvent, value);
		}
		remove
		{
			RemoveHandler(ClosedEvent, value);
		}
	}

	static ToolTip()
	{
		FromKeyboardProperty = DependencyProperty.Register("FromKeyboard", typeof(bool), typeof(ToolTip), new FrameworkPropertyMetadata(false));
		HorizontalOffsetProperty = ToolTipService.HorizontalOffsetProperty.AddOwner(typeof(ToolTip), new FrameworkPropertyMetadata(null, CoerceHorizontalOffset));
		VerticalOffsetProperty = ToolTipService.VerticalOffsetProperty.AddOwner(typeof(ToolTip), new FrameworkPropertyMetadata(null, CoerceVerticalOffset));
		IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(ToolTip), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));
		HasDropShadowProperty = ToolTipService.HasDropShadowProperty.AddOwner(typeof(ToolTip), new FrameworkPropertyMetadata(null, CoerceHasDropShadow));
		PlacementTargetProperty = ToolTipService.PlacementTargetProperty.AddOwner(typeof(ToolTip), new FrameworkPropertyMetadata(null, CoercePlacementTarget));
		PlacementRectangleProperty = ToolTipService.PlacementRectangleProperty.AddOwner(typeof(ToolTip), new FrameworkPropertyMetadata(null, CoercePlacementRectangle));
		PlacementProperty = ToolTipService.PlacementProperty.AddOwner(typeof(ToolTip), new FrameworkPropertyMetadata(null, CoercePlacement));
		CustomPopupPlacementCallbackProperty = Popup.CustomPopupPlacementCallbackProperty.AddOwner(typeof(ToolTip));
		StaysOpenProperty = Popup.StaysOpenProperty.AddOwner(typeof(ToolTip));
		ShowsToolTipOnKeyboardFocusProperty = ToolTipService.ShowsToolTipOnKeyboardFocusProperty.AddOwner(typeof(ToolTip));
		OpenedEvent = EventManager.RegisterRoutedEvent("Opened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToolTip));
		ClosedEvent = EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToolTip));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolTip), new FrameworkPropertyMetadata(typeof(ToolTip)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(ToolTip));
		Control.BackgroundProperty.OverrideMetadata(typeof(ToolTip), new FrameworkPropertyMetadata(SystemColors.InfoBrush));
		UIElement.FocusableProperty.OverrideMetadata(typeof(ToolTip), new FrameworkPropertyMetadata(false));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ToolTip" /> class. </summary>
	public ToolTip()
	{
	}

	private static object CoerceHorizontalOffset(DependencyObject d, object value)
	{
		return PopupControlService.CoerceProperty(d, value, ToolTipService.HorizontalOffsetProperty);
	}

	private static object CoerceVerticalOffset(DependencyObject d, object value)
	{
		return PopupControlService.CoerceProperty(d, value, ToolTipService.VerticalOffsetProperty);
	}

	private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ToolTip toolTip = (ToolTip)d;
		if ((bool)e.NewValue)
		{
			if (toolTip._parentPopup == null)
			{
				toolTip.HookupParentPopup();
			}
		}
		else if (AutomationPeer.ListenerExists(AutomationEvents.ToolTipClosed))
		{
			UIElementAutomationPeer.CreatePeerForElement(toolTip)?.RaiseAutomationEvent(AutomationEvents.ToolTipClosed);
		}
		Control.OnVisualStatePropertyChanged(d, e);
	}

	private static object CoerceHasDropShadow(DependencyObject d, object value)
	{
		ToolTip toolTip = (ToolTip)d;
		if (toolTip._parentPopup == null || !toolTip._parentPopup.AllowsTransparency || !SystemParameters.DropShadow)
		{
			return BooleanBoxes.FalseBox;
		}
		return PopupControlService.CoerceProperty(d, value, ToolTipService.HasDropShadowProperty);
	}

	private static object CoercePlacementTarget(DependencyObject d, object value)
	{
		return PopupControlService.CoerceProperty(d, value, ToolTipService.PlacementTargetProperty);
	}

	private static object CoercePlacementRectangle(DependencyObject d, object value)
	{
		return PopupControlService.CoerceProperty(d, value, ToolTipService.PlacementRectangleProperty);
	}

	private static object CoercePlacement(DependencyObject d, object value)
	{
		return PopupControlService.CoerceProperty(d, value, ToolTipService.PlacementProperty);
	}

	/// <summary>Responds to the <see cref="E:System.Windows.Controls.ToolTip.Opened" /> event. </summary>
	/// <param name="e">The event information.</param>
	protected virtual void OnOpened(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Responds to the <see cref="E:System.Windows.Controls.ToolTip.Closed" /> event.</summary>
	/// <param name="e">The event information.</param>
	protected virtual void OnClosed(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (IsOpen)
		{
			VisualStateManager.GoToState(this, "Open", useTransitions);
		}
		else
		{
			VisualStateManager.GoToState(this, "Closed", useTransitions);
		}
		base.ChangeVisualState(useTransitions);
	}

	/// <summary>Creates the implementation of <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.ToolTip" /> control.</summary>
	/// <returns>A new <see cref="T:System.Windows.Automation.Peers.ToolTipAutomationPeer" /> for this <see cref="T:System.Windows.Controls.ToolTip" /> control.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new ToolTipAutomationPeer(this);
	}

	/// <summary>Responds to a change in the visual parent of a <see cref="T:System.Windows.Controls.ToolTip" />.</summary>
	/// <param name="oldParent">The previous visual parent.</param>
	protected internal override void OnVisualParentChanged(DependencyObject oldParent)
	{
		base.OnVisualParentChanged(oldParent);
		if (!Popup.IsRootedInPopup(_parentPopup, this))
		{
			throw new InvalidOperationException(SR.Format(SR.ElementMustBeInPopup, "ToolTip"));
		}
	}

	internal override void OnAncestorChanged()
	{
		base.OnAncestorChanged();
		if (!Popup.IsRootedInPopup(_parentPopup, this))
		{
			throw new InvalidOperationException(SR.Format(SR.ElementMustBeInPopup, "ToolTip"));
		}
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property changes. </summary>
	/// <param name="oldContent">The old value of the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property.</param>
	/// <param name="newContent">The new value of the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property.</param>
	protected override void OnContentChanged(object oldContent, object newContent)
	{
		PopupControlService current = PopupControlService.Current;
		if (this == current.CurrentToolTip && (bool)GetValue(PopupControlService.ServiceOwnedProperty) && newContent is ToolTip)
		{
			current.ReplaceCurrentToolTip();
		}
		else
		{
			base.OnContentChanged(oldContent, newContent);
		}
	}

	private void HookupParentPopup()
	{
		_parentPopup = new Popup();
		_parentPopup.AllowsTransparency = true;
		_parentPopup.HitTestable = !StaysOpen;
		CoerceValue(HasDropShadowProperty);
		_parentPopup.Opened += OnPopupOpened;
		_parentPopup.Closed += OnPopupClosed;
		_parentPopup.PopupCouldClose += OnPopupCouldClose;
		_parentPopup.SetResourceReference(Popup.PopupAnimationProperty, SystemParameters.ToolTipPopupAnimationKey);
		Popup.CreateRootPopupInternal(_parentPopup, this, bindTreatMousePlacementAsBottomProperty: true);
	}

	internal void ForceClose()
	{
		if (_parentPopup != null)
		{
			_parentPopup.ForceClose();
		}
	}

	private void OnPopupCouldClose(object sender, EventArgs e)
	{
		SetCurrentValueInternal(IsOpenProperty, BooleanBoxes.FalseBox);
	}

	private void OnPopupOpened(object source, EventArgs e)
	{
		if (AutomationPeer.ListenerExists(AutomationEvents.ToolTipOpened))
		{
			AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(this);
			if (peer != null)
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Input, (DispatcherOperationCallback)delegate
				{
					peer.RaiseAutomationEvent(AutomationEvents.ToolTipOpened);
					return (object)null;
				}, null);
			}
		}
		OnOpened(new RoutedEventArgs(OpenedEvent, this));
	}

	private void OnPopupClosed(object source, EventArgs e)
	{
		OnClosed(new RoutedEventArgs(ClosedEvent, this));
	}

	internal Rect GetScreenRect()
	{
		if (_parentPopup != null)
		{
			return _parentPopup.GetWindowRect();
		}
		return Rect.Empty;
	}
}
