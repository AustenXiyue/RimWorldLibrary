using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Threading;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents a control that raises its <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> event repeatedly from the time it is pressed until it is released. </summary>
public class RepeatButton : ButtonBase
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.RepeatButton.Delay" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.RepeatButton.Delay" /> dependency property.</returns>
	public static readonly DependencyProperty DelayProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.RepeatButton.Interval" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.RepeatButton.Interval" /> dependency property.</returns>
	public static readonly DependencyProperty IntervalProperty;

	private DispatcherTimer _timer;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets the amount of time, in milliseconds, the <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> waits while it is pressed before it starts repeating. The value must be non-negative.  </summary>
	/// <returns>The amount of time, in milliseconds, the <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> waits while it is pressed before it starts repeating. The default is the value of <see cref="P:System.Windows.SystemParameters.KeyboardDelay" />.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public int Delay
	{
		get
		{
			return (int)GetValue(DelayProperty);
		}
		set
		{
			SetValue(DelayProperty, value);
		}
	}

	/// <summary>Gets or sets the amount of time, in milliseconds, between repeats once repeating starts. The value must be non-negative.  </summary>
	/// <returns>The amount of time, in milliseconds, between repeats after repeating starts. The default is the value of <see cref="P:System.Windows.SystemParameters.KeyboardSpeed" />.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public int Interval
	{
		get
		{
			return (int)GetValue(IntervalProperty);
		}
		set
		{
			SetValue(IntervalProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 28;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static RepeatButton()
	{
		DelayProperty = DependencyProperty.Register("Delay", typeof(int), typeof(RepeatButton), new FrameworkPropertyMetadata(GetKeyboardDelay()), IsDelayValid);
		IntervalProperty = DependencyProperty.Register("Interval", typeof(int), typeof(RepeatButton), new FrameworkPropertyMetadata(GetKeyboardSpeed()), IsIntervalValid);
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RepeatButton), new FrameworkPropertyMetadata(typeof(RepeatButton)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(RepeatButton));
		ButtonBase.ClickModeProperty.OverrideMetadata(typeof(RepeatButton), new FrameworkPropertyMetadata(ClickMode.Press));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> class. </summary>
	public RepeatButton()
	{
	}

	private static bool IsDelayValid(object value)
	{
		return (int)value >= 0;
	}

	private static bool IsIntervalValid(object value)
	{
		return (int)value > 0;
	}

	private void StartTimer()
	{
		if (_timer == null)
		{
			_timer = new DispatcherTimer();
			_timer.Tick += OnTimeout;
		}
		else if (_timer.IsEnabled)
		{
			return;
		}
		_timer.Interval = TimeSpan.FromMilliseconds(Delay);
		_timer.Start();
	}

	private void StopTimer()
	{
		if (_timer != null)
		{
			_timer.Stop();
		}
	}

	private void OnTimeout(object sender, EventArgs e)
	{
		TimeSpan timeSpan = TimeSpan.FromMilliseconds(Interval);
		if (_timer.Interval != timeSpan)
		{
			_timer.Interval = timeSpan;
		}
		if (base.IsPressed)
		{
			OnClick();
		}
	}

	internal static int GetKeyboardDelay()
	{
		int num = SystemParameters.KeyboardDelay;
		if (num < 0 || num > 3)
		{
			num = 0;
		}
		return (num + 1) * 250;
	}

	internal static int GetKeyboardSpeed()
	{
		int num = SystemParameters.KeyboardSpeed;
		if (num < 0 || num > 31)
		{
			num = 31;
		}
		return (31 - num) * 367 / 31 + 33;
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.RepeatButtonAutomationPeer" /> implementation for this control, as part of the WPF infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new RepeatButtonAutomationPeer(this);
	}

	/// <summary>Raises an automation event and calls the base method to raise the <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> event. </summary>
	protected override void OnClick()
	{
		if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
		{
			UIElementAutomationPeer.CreatePeerForElement(this)?.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
		}
		base.OnClick();
	}

	/// <summary>Responds to the <see cref="E:System.Windows.UIElement.MouseLeftButtonDown" /> event. </summary>
	/// <param name="e">The event data for a <see cref="E:System.Windows.UIElement.MouseLeftButtonDown" /> event.</param>
	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonDown(e);
		if (base.IsPressed && base.ClickMode != ClickMode.Hover)
		{
			StartTimer();
		}
	}

	/// <summary>Responds to the <see cref="E:System.Windows.UIElement.MouseLeftButtonUp" /> event. </summary>
	/// <param name="e">The event data for a <see cref="E:System.Windows.ContentElement.MouseLeftButtonUp" /> event.</param>
	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonUp(e);
		if (base.ClickMode != ClickMode.Hover)
		{
			StopTimer();
		}
	}

	/// <summary>Called when a <see cref="T:System.Windows.Controls.Primitives.RepeatButton" /> loses mouse capture. </summary>
	/// <param name="e">The event data for a <see cref="E:System.Windows.UIElement.LostMouseCapture" /> event.</param>
	protected override void OnLostMouseCapture(MouseEventArgs e)
	{
		base.OnLostMouseCapture(e);
		StopTimer();
	}

	/// <summary>Reports when the mouse enters an element. </summary>
	/// <param name="e">The event data for a <see cref="E:System.Windows.UIElement.MouseEnter" /> event.</param>
	protected override void OnMouseEnter(MouseEventArgs e)
	{
		base.OnMouseEnter(e);
		if (HandleIsMouseOverChanged())
		{
			e.Handled = true;
		}
	}

	/// <summary>Reports when the mouse leaves an element. </summary>
	/// <param name="e">The event data for a <see cref="E:System.Windows.UIElement.MouseLeave" /> event.</param>
	protected override void OnMouseLeave(MouseEventArgs e)
	{
		base.OnMouseLeave(e);
		if (HandleIsMouseOverChanged())
		{
			e.Handled = true;
		}
	}

	private bool HandleIsMouseOverChanged()
	{
		if (base.ClickMode == ClickMode.Hover)
		{
			if (base.IsMouseOver)
			{
				StartTimer();
			}
			else
			{
				StopTimer();
			}
			return true;
		}
		return false;
	}

	/// <summary>Responds to the <see cref="E:System.Windows.UIElement.KeyDown" /> event. </summary>
	/// <param name="e">The event data for a <see cref="E:System.Windows.UIElement.KeyDown" /> event.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.Key == Key.Space && base.ClickMode != ClickMode.Hover)
		{
			StartTimer();
		}
	}

	/// <summary>Responds to the <see cref="E:System.Windows.UIElement.KeyUp" /> event. </summary>
	/// <param name="e">The event data for a <see cref="E:System.Windows.UIElement.KeyUp" /> event.</param>
	protected override void OnKeyUp(KeyEventArgs e)
	{
		if (e.Key == Key.Space && base.ClickMode != ClickMode.Hover)
		{
			StopTimer();
		}
		base.OnKeyUp(e);
	}
}
