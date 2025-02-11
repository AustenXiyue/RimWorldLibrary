using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Threading;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls.Primitives;

/// <summary>Base class for controls that can switch states, such as <see cref="T:System.Windows.Controls.CheckBox" />. </summary>
[DefaultEvent("Checked")]
public class ToggleButton : ButtonBase
{
	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Primitives.ToggleButton.Checked" />  routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Primitives.ToggleButton.Checked" /> routed event.</returns>
	public static readonly RoutedEvent CheckedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Primitives.ToggleButton.Unchecked" />  routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Primitives.ToggleButton.Unchecked" /> routed event.</returns>
	public static readonly RoutedEvent UncheckedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Primitives.ToggleButton.Indeterminate" />  routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Primitives.ToggleButton.Indeterminate" /> routed event.</returns>
	public static readonly RoutedEvent IndeterminateEvent;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.ToggleButton.IsChecked" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.ToggleButton.IsChecked" /> dependency property.</returns>
	public static readonly DependencyProperty IsCheckedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.ToggleButton.IsThreeState" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.ToggleButton.IsThreeState" /> dependency property.</returns>
	public static readonly DependencyProperty IsThreeStateProperty;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets whether the <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> is checked.   </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> is checked; false if the <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> is unchecked; otherwise null. The default is false.</returns>
	[Category("Appearance")]
	[TypeConverter(typeof(NullableBoolConverter))]
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
	public bool? IsChecked
	{
		get
		{
			object value = GetValue(IsCheckedProperty);
			if (value == null)
			{
				return null;
			}
			return (bool)value;
		}
		set
		{
			SetValue(IsCheckedProperty, value.HasValue ? BooleanBoxes.Box(value.Value) : null);
		}
	}

	/// <summary>Determines whether the control supports two or three states.   </summary>
	/// <returns>true if the control supports three states; otherwise, false. The default is false.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public bool IsThreeState
	{
		get
		{
			return (bool)GetValue(IsThreeStateProperty);
		}
		set
		{
			SetValue(IsThreeStateProperty, BooleanBoxes.Box(value));
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Occurs when a <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> is checked.</summary>
	[Category("Behavior")]
	public event RoutedEventHandler Checked
	{
		add
		{
			AddHandler(CheckedEvent, value);
		}
		remove
		{
			RemoveHandler(CheckedEvent, value);
		}
	}

	/// <summary>Occurs when a <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> is unchecked.</summary>
	[Category("Behavior")]
	public event RoutedEventHandler Unchecked
	{
		add
		{
			AddHandler(UncheckedEvent, value);
		}
		remove
		{
			RemoveHandler(UncheckedEvent, value);
		}
	}

	/// <summary>Occurs when the state of a <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> is neither on nor off.</summary>
	[Category("Behavior")]
	public event RoutedEventHandler Indeterminate
	{
		add
		{
			AddHandler(IndeterminateEvent, value);
		}
		remove
		{
			RemoveHandler(IndeterminateEvent, value);
		}
	}

	static ToggleButton()
	{
		CheckedEvent = EventManager.RegisterRoutedEvent("Checked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToggleButton));
		UncheckedEvent = EventManager.RegisterRoutedEvent("Unchecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToggleButton));
		IndeterminateEvent = EventManager.RegisterRoutedEvent("Indeterminate", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToggleButton));
		IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool?), typeof(ToggleButton), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnIsCheckedChanged));
		IsThreeStateProperty = DependencyProperty.Register("IsThreeState", typeof(bool), typeof(ToggleButton), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleButton), new FrameworkPropertyMetadata(typeof(ToggleButton)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(ToggleButton));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> class. </summary>
	public ToggleButton()
	{
	}

	private static object OnGetIsChecked(DependencyObject d)
	{
		return ((ToggleButton)d).IsChecked;
	}

	private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ToggleButton toggleButton = (ToggleButton)d;
		bool? oldValue = (bool?)e.OldValue;
		bool? flag = (bool?)e.NewValue;
		if (UIElementAutomationPeer.FromElement(toggleButton) is ToggleButtonAutomationPeer toggleButtonAutomationPeer)
		{
			toggleButtonAutomationPeer.RaiseToggleStatePropertyChangedEvent(oldValue, flag);
		}
		if (flag == true)
		{
			toggleButton.OnChecked(new RoutedEventArgs(CheckedEvent));
		}
		else if (flag == false)
		{
			toggleButton.OnUnchecked(new RoutedEventArgs(UncheckedEvent));
		}
		else
		{
			toggleButton.OnIndeterminate(new RoutedEventArgs(IndeterminateEvent));
		}
		toggleButton.UpdateVisualState();
	}

	/// <summary>Called when a <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> raises a <see cref="E:System.Windows.Controls.Primitives.ToggleButton.Checked" /> event.</summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Controls.Primitives.ToggleButton.Checked" /> event.</param>
	protected virtual void OnChecked(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Called when a <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> raises an <see cref="E:System.Windows.Controls.Primitives.ToggleButton.Unchecked" /> event.</summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Controls.Primitives.ToggleButton.Unchecked" /> event.</param>
	protected virtual void OnUnchecked(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Called when a <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> raises an <see cref="E:System.Windows.Controls.Primitives.ToggleButton.Indeterminate" /> event.</summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Controls.Primitives.ToggleButton.Indeterminate" /> event.</param>
	protected virtual void OnIndeterminate(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.ToggleButtonAutomationPeer" /> implementation for this control, as part of the WPF infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new ToggleButtonAutomationPeer(this);
	}

	/// <summary>Called when a control is clicked by the mouse or the keyboard. </summary>
	protected override void OnClick()
	{
		OnToggle();
		base.OnClick();
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		base.ChangeVisualState(useTransitions);
		bool? isChecked = IsChecked;
		if (isChecked == true)
		{
			VisualStateManager.GoToState(this, "Checked", useTransitions);
			return;
		}
		if (isChecked == false)
		{
			VisualStateManager.GoToState(this, "Unchecked", useTransitions);
			return;
		}
		VisualStates.GoToState(this, useTransitions, "Indeterminate", "Unchecked");
	}

	/// <summary>Returns the string representation of a <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> object. </summary>
	/// <returns>String representation of a <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> object.</returns>
	public override string ToString()
	{
		string text = GetType().ToString();
		string contentText = string.Empty;
		bool? isChecked = false;
		bool valuesDefined = false;
		if (CheckAccess())
		{
			contentText = GetPlainText();
			isChecked = IsChecked;
			valuesDefined = true;
		}
		else
		{
			base.Dispatcher.Invoke(DispatcherPriority.Send, new TimeSpan(0, 0, 0, 0, 20), (DispatcherOperationCallback)delegate
			{
				contentText = GetPlainText();
				isChecked = IsChecked;
				valuesDefined = true;
				return (object)null;
			}, null);
		}
		if (valuesDefined)
		{
			return SR.Format(SR.ToStringFormatString_ToggleButton, text, contentText, isChecked.HasValue ? isChecked.Value.ToString() : "null");
		}
		return text;
	}

	/// <summary>Called by the <see cref="M:System.Windows.Controls.Primitives.ToggleButton.OnClick" /> method to implement toggle behavior. </summary>
	protected internal virtual void OnToggle()
	{
		bool? flag = ((IsChecked != true) ? new bool?(IsChecked.HasValue) : (IsThreeState ? ((bool?)null) : new bool?(false)));
		SetCurrentValueInternal(IsCheckedProperty, flag);
	}
}
