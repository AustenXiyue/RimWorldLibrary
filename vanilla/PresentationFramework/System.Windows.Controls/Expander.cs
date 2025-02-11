using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents the control that displays a header that has a collapsible window that displays content.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public class Expander : HeaderedContentControl
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Expander.ExpandDirection" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Expander.ExpandDirection" /> dependency property.</returns>
	public static readonly DependencyProperty ExpandDirectionProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Expander.IsExpanded" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Expander.IsExpanded" /> dependency property.</returns>
	public static readonly DependencyProperty IsExpandedProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Expander.Expanded" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Expander.Expanded" /> routed event.</returns>
	public static readonly RoutedEvent ExpandedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Expander.Collapsed" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Expander.Collapsed" /> routed event.</returns>
	public static readonly RoutedEvent CollapsedEvent;

	private const string ExpanderToggleButtonTemplateName = "HeaderSite";

	private ToggleButton _expanderToggleButton;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets the direction in which the <see cref="T:System.Windows.Controls.Expander" /> content window opens.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.Controls.ExpandDirection" /> values that defines which direction the content window opens. The default is <see cref="F:System.Windows.Controls.ExpandDirection.Down" />. </returns>
	[Bindable(true)]
	[Category("Behavior")]
	public ExpandDirection ExpandDirection
	{
		get
		{
			return (ExpandDirection)GetValue(ExpandDirectionProperty);
		}
		set
		{
			SetValue(ExpandDirectionProperty, value);
		}
	}

	/// <summary>Gets or sets whether the <see cref="T:System.Windows.Controls.Expander" /> content window is visible.  </summary>
	/// <returns>true if the content window is expanded; otherwise, false. The default is false.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public bool IsExpanded
	{
		get
		{
			return (bool)GetValue(IsExpandedProperty);
		}
		set
		{
			SetValue(IsExpandedProperty, BooleanBoxes.Box(value));
		}
	}

	internal bool IsExpanderToggleButtonFocused => _expanderToggleButton?.IsKeyboardFocused ?? false;

	internal ToggleButton ExpanderToggleButton => _expanderToggleButton;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Occurs when the content window of an <see cref="T:System.Windows.Controls.Expander" /> control opens to display both its header and content. </summary>
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

	/// <summary>Occurs when the content window of an <see cref="T:System.Windows.Controls.Expander" /> control closes and only the <see cref="P:System.Windows.Controls.HeaderedContentControl.Header" /> is visible.</summary>
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

	static Expander()
	{
		ExpandDirectionProperty = DependencyProperty.Register("ExpandDirection", typeof(ExpandDirection), typeof(Expander), new FrameworkPropertyMetadata(ExpandDirection.Down, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, Control.OnVisualStatePropertyChanged), IsValidExpandDirection);
		IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(Expander), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnIsExpandedChanged));
		ExpandedEvent = EventManager.RegisterRoutedEvent("Expanded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Expander));
		CollapsedEvent = EventManager.RegisterRoutedEvent("Collapsed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Expander));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Expander), new FrameworkPropertyMetadata(typeof(Expander)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(Expander));
		Control.IsTabStopProperty.OverrideMetadata(typeof(Expander), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		UIElement.IsMouseOverPropertyKey.OverrideMetadata(typeof(Expander), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(Expander), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		ControlsTraceLogger.AddControl(TelemetryControls.Expander);
	}

	private static bool IsValidExpandDirection(object o)
	{
		ExpandDirection expandDirection = (ExpandDirection)o;
		if (expandDirection != 0 && expandDirection != ExpandDirection.Left && expandDirection != ExpandDirection.Right)
		{
			return expandDirection == ExpandDirection.Up;
		}
		return true;
	}

	private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Expander expander = (Expander)d;
		bool flag = (bool)e.NewValue;
		if (UIElementAutomationPeer.FromElement(expander) is ExpanderAutomationPeer expanderAutomationPeer)
		{
			expanderAutomationPeer.RaiseExpandCollapseAutomationEvent(!flag, flag);
		}
		if (flag)
		{
			expander.OnExpanded();
		}
		else
		{
			expander.OnCollapsed();
		}
		expander.UpdateVisualState();
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
		switch (ExpandDirection)
		{
		case ExpandDirection.Down:
			VisualStates.GoToState(this, useTransitions, "ExpandDown");
			break;
		case ExpandDirection.Up:
			VisualStates.GoToState(this, useTransitions, "ExpandUp");
			break;
		case ExpandDirection.Left:
			VisualStates.GoToState(this, useTransitions, "ExpandLeft");
			break;
		default:
			VisualStates.GoToState(this, useTransitions, "ExpandRight");
			break;
		}
		base.ChangeVisualState(useTransitions);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.Expander.Expanded" /> event when the <see cref="P:System.Windows.Controls.Expander.IsExpanded" /> property changes from false to true.</summary>
	protected virtual void OnExpanded()
	{
		RoutedEventArgs routedEventArgs = new RoutedEventArgs();
		routedEventArgs.RoutedEvent = ExpandedEvent;
		routedEventArgs.Source = this;
		RaiseEvent(routedEventArgs);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.Expander.Collapsed" /> event when the <see cref="P:System.Windows.Controls.Expander.IsExpanded" /> property changes from true to false.</summary>
	protected virtual void OnCollapsed()
	{
		RaiseEvent(new RoutedEventArgs(CollapsedEvent, this));
	}

	/// <summary>Creates the implementation of <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.Expander" /> control.</summary>
	/// <returns>A new <see cref="T:System.Windows.Automation.Peers.ExpanderAutomationPeer" /> for this <see cref="T:System.Windows.Controls.Expander" /> control.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new ExpanderAutomationPeer(this);
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		_expanderToggleButton = GetTemplateChild("HeaderSite") as ToggleButton;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Expander" /> class.</summary>
	public Expander()
	{
	}
}
