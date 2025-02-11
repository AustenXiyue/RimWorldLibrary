using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Threading;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents an element that has a value within a specific range. </summary>
[DefaultEvent("ValueChanged")]
[DefaultProperty("Value")]
public abstract class RangeBase : Control
{
	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Primitives.RangeBase.ValueChanged" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Primitives.RangeBase.ValueChanged" /> routed event.</returns>
	public static readonly RoutedEvent ValueChangedEvent;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> dependency property.</returns>
	public static readonly DependencyProperty MinimumProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> dependency property.</returns>
	public static readonly DependencyProperty MaximumProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> dependency property.</returns>
	public static readonly DependencyProperty ValueProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.RangeBase.LargeChange" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.RangeBase.LargeChange" /> dependency property.</returns>
	public static readonly DependencyProperty LargeChangeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.RangeBase.SmallChange" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.RangeBase.SmallChange" /> dependency property.</returns>
	public static readonly DependencyProperty SmallChangeProperty;

	/// <summary>Gets or sets the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> possible <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> of the range element.  </summary>
	/// <returns>
	///   <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> possible <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> of the range element. The default is 0.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public double Minimum
	{
		get
		{
			return (double)GetValue(MinimumProperty);
		}
		set
		{
			SetValue(MinimumProperty, value);
		}
	}

	/// <summary>Gets or sets the highest possible <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> of the range element.  </summary>
	/// <returns>The highest possible <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> of the range element. The default is 1.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public double Maximum
	{
		get
		{
			return (double)GetValue(MaximumProperty);
		}
		set
		{
			SetValue(MaximumProperty, value);
		}
	}

	/// <summary>Gets or sets the current magnitude of the range control.  </summary>
	/// <returns>The current magnitude of the range control. The default is 0.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public double Value
	{
		get
		{
			return (double)GetValue(ValueProperty);
		}
		set
		{
			SetValue(ValueProperty, value);
		}
	}

	/// <summary>Gets or sets a value to be added to or subtracted from the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> of a <see cref="T:System.Windows.Controls.Primitives.RangeBase" /> control.  </summary>
	/// <returns>
	///   <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> to add to or subtract from the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> of the <see cref="T:System.Windows.Controls.Primitives.RangeBase" /> element. The default is 1.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public double LargeChange
	{
		get
		{
			return (double)GetValue(LargeChangeProperty);
		}
		set
		{
			SetValue(LargeChangeProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> to be added to or subtracted from the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> of a <see cref="T:System.Windows.Controls.Primitives.RangeBase" /> control.  </summary>
	/// <returns>
	///   <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> to add to or subtract from the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> of the <see cref="T:System.Windows.Controls.Primitives.RangeBase" /> element. The default is 0.1.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public double SmallChange
	{
		get
		{
			return (double)GetValue(SmallChangeProperty);
		}
		set
		{
			SetValue(SmallChangeProperty, value);
		}
	}

	/// <summary>Occurs when the range value changes. </summary>
	[Category("Behavior")]
	public event RoutedPropertyChangedEventHandler<double> ValueChanged
	{
		add
		{
			AddHandler(ValueChangedEvent, value);
		}
		remove
		{
			RemoveHandler(ValueChangedEvent, value);
		}
	}

	static RangeBase()
	{
		ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(RangeBase));
		MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(RangeBase), new FrameworkPropertyMetadata(0.0, OnMinimumChanged), IsValidDoubleValue);
		MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(RangeBase), new FrameworkPropertyMetadata(1.0, OnMaximumChanged, CoerceMaximum), IsValidDoubleValue);
		ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(RangeBase), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnValueChanged, ConstrainToRange), IsValidDoubleValue);
		LargeChangeProperty = DependencyProperty.Register("LargeChange", typeof(double), typeof(RangeBase), new FrameworkPropertyMetadata(1.0), IsValidChange);
		SmallChangeProperty = DependencyProperty.Register("SmallChange", typeof(double), typeof(RangeBase), new FrameworkPropertyMetadata(0.1), IsValidChange);
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(RangeBase), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		UIElement.IsMouseOverPropertyKey.OverrideMetadata(typeof(RangeBase), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.RangeBase" /> class. </summary>
	protected RangeBase()
	{
	}

	private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		RangeBase obj = (RangeBase)d;
		if (UIElementAutomationPeer.FromElement(obj) is RangeBaseAutomationPeer rangeBaseAutomationPeer)
		{
			rangeBaseAutomationPeer.RaiseMinimumPropertyChangedEvent((double)e.OldValue, (double)e.NewValue);
		}
		obj.CoerceValue(MaximumProperty);
		obj.CoerceValue(ValueProperty);
		obj.OnMinimumChanged((double)e.OldValue, (double)e.NewValue);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> property changes. </summary>
	/// <param name="oldMinimum">Old value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> property.</param>
	/// <param name="newMinimum">New value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> property.</param>
	protected virtual void OnMinimumChanged(double oldMinimum, double newMinimum)
	{
	}

	private static object CoerceMaximum(DependencyObject d, object value)
	{
		double minimum = ((RangeBase)d).Minimum;
		if ((double)value < minimum)
		{
			return minimum;
		}
		return value;
	}

	private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		RangeBase obj = (RangeBase)d;
		if (UIElementAutomationPeer.FromElement(obj) is RangeBaseAutomationPeer rangeBaseAutomationPeer)
		{
			rangeBaseAutomationPeer.RaiseMaximumPropertyChangedEvent((double)e.OldValue, (double)e.NewValue);
		}
		obj.CoerceValue(ValueProperty);
		obj.OnMaximumChanged((double)e.OldValue, (double)e.NewValue);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> property changes. </summary>
	/// <param name="oldMaximum">Old value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> property.</param>
	/// <param name="newMaximum">New value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> property.</param>
	protected virtual void OnMaximumChanged(double oldMaximum, double newMaximum)
	{
	}

	internal static object ConstrainToRange(DependencyObject d, object value)
	{
		RangeBase rangeBase = (RangeBase)d;
		double minimum = rangeBase.Minimum;
		double num = (double)value;
		if (num < minimum)
		{
			return minimum;
		}
		double maximum = rangeBase.Maximum;
		if (num > maximum)
		{
			return maximum;
		}
		return value;
	}

	private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		RangeBase obj = (RangeBase)d;
		if (UIElementAutomationPeer.FromElement(obj) is RangeBaseAutomationPeer rangeBaseAutomationPeer)
		{
			rangeBaseAutomationPeer.RaiseValuePropertyChangedEvent((double)e.OldValue, (double)e.NewValue);
		}
		obj.OnValueChanged((double)e.OldValue, (double)e.NewValue);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.Primitives.RangeBase.ValueChanged" /> routed event. </summary>
	/// <param name="oldValue">Old value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> property</param>
	/// <param name="newValue">New value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> property</param>
	protected virtual void OnValueChanged(double oldValue, double newValue)
	{
		RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs = new RoutedPropertyChangedEventArgs<double>(oldValue, newValue);
		routedPropertyChangedEventArgs.RoutedEvent = ValueChangedEvent;
		RaiseEvent(routedPropertyChangedEventArgs);
	}

	private static bool IsValidDoubleValue(object value)
	{
		double d = (double)value;
		if (!double.IsNaN(d))
		{
			return !double.IsInfinity(d);
		}
		return false;
	}

	private static bool IsValidChange(object value)
	{
		double num = (double)value;
		if (IsValidDoubleValue(value))
		{
			return num >= 0.0;
		}
		return false;
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
			VisualStateManager.GoToState(this, "Normal", useTransitions);
		}
		if (base.IsKeyboardFocused)
		{
			VisualStates.GoToState(this, useTransitions, "Focused", "Unfocused");
		}
		else
		{
			VisualStateManager.GoToState(this, "Unfocused", useTransitions);
		}
		base.ChangeVisualState(useTransitions);
	}

	/// <summary>Provides a string representation of a <see cref="T:System.Windows.Controls.Primitives.RangeBase" /> object. </summary>
	/// <returns>Returns the string representation of a <see cref="T:System.Windows.Controls.Primitives.RangeBase" /> object.</returns>
	public override string ToString()
	{
		string text = GetType().ToString();
		double min = double.NaN;
		double max = double.NaN;
		double val = double.NaN;
		bool valuesDefined = false;
		if (CheckAccess())
		{
			min = Minimum;
			max = Maximum;
			val = Value;
			valuesDefined = true;
		}
		else
		{
			base.Dispatcher.Invoke(DispatcherPriority.Send, new TimeSpan(0, 0, 0, 0, 20), (DispatcherOperationCallback)delegate
			{
				min = Minimum;
				max = Maximum;
				val = Value;
				valuesDefined = true;
				return (object)null;
			}, null);
		}
		if (valuesDefined)
		{
			return SR.Format(SR.ToStringFormatString_RangeBase, text, min, max, val);
		}
		return text;
	}
}
