using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents a control that can be dragged by the user. </summary>
[DefaultEvent("DragDelta")]
[Localizability(LocalizationCategory.NeverLocalize)]
public class Thumb : Control
{
	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Primitives.Thumb.DragStarted" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Primitives.Thumb.DragStarted" /> routed event.</returns>
	public static readonly RoutedEvent DragStartedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Primitives.Thumb.DragDelta" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Primitives.Thumb.DragDelta" /> routed event.</returns>
	public static readonly RoutedEvent DragDeltaEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Primitives.Thumb.DragCompleted" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Primitives.Thumb.DragCompleted" /> routed event.</returns>
	public static readonly RoutedEvent DragCompletedEvent;

	private static readonly DependencyPropertyKey IsDraggingPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Thumb.IsDragging" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Thumb.IsDragging" /> dependency property.</returns>
	public static readonly DependencyProperty IsDraggingProperty;

	private Point _originThumbPoint;

	private Point _originScreenCoordPosition;

	private Point _previousScreenCoordPosition;

	private static DependencyObjectType _dType;

	/// <summary>Gets whether the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control has logical focus and mouse capture and the left mouse button is pressed.   </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control has focus and mouse capture; otherwise false. The default value is false.</returns>
	[Bindable(true)]
	[Browsable(false)]
	[Category("Appearance")]
	public bool IsDragging
	{
		get
		{
			return (bool)GetValue(IsDraggingProperty);
		}
		protected set
		{
			SetValue(IsDraggingPropertyKey, BooleanBoxes.Box(value));
		}
	}

	internal override int EffectiveValuesInitialSize => 19;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Occurs when a <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control receives logical focus and mouse capture.</summary>
	[Category("Behavior")]
	public event DragStartedEventHandler DragStarted
	{
		add
		{
			AddHandler(DragStartedEvent, value);
		}
		remove
		{
			RemoveHandler(DragStartedEvent, value);
		}
	}

	/// <summary>Occurs one or more times as the mouse changes position when a <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control has logical focus and mouse capture. </summary>
	[Category("Behavior")]
	public event DragDeltaEventHandler DragDelta
	{
		add
		{
			AddHandler(DragDeltaEvent, value);
		}
		remove
		{
			RemoveHandler(DragDeltaEvent, value);
		}
	}

	/// <summary>Occurs when the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control loses mouse capture.</summary>
	[Category("Behavior")]
	public event DragCompletedEventHandler DragCompleted
	{
		add
		{
			AddHandler(DragCompletedEvent, value);
		}
		remove
		{
			RemoveHandler(DragCompletedEvent, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> class.</summary>
	public Thumb()
	{
	}

	static Thumb()
	{
		DragStartedEvent = EventManager.RegisterRoutedEvent("DragStarted", RoutingStrategy.Bubble, typeof(DragStartedEventHandler), typeof(Thumb));
		DragDeltaEvent = EventManager.RegisterRoutedEvent("DragDelta", RoutingStrategy.Bubble, typeof(DragDeltaEventHandler), typeof(Thumb));
		DragCompletedEvent = EventManager.RegisterRoutedEvent("DragCompleted", RoutingStrategy.Bubble, typeof(DragCompletedEventHandler), typeof(Thumb));
		IsDraggingPropertyKey = DependencyProperty.RegisterReadOnly("IsDragging", typeof(bool), typeof(Thumb), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, OnIsDraggingPropertyChanged));
		IsDraggingProperty = IsDraggingPropertyKey.DependencyProperty;
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Thumb), new FrameworkPropertyMetadata(typeof(Thumb)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(Thumb));
		UIElement.FocusableProperty.OverrideMetadata(typeof(Thumb), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		EventManager.RegisterClassHandler(typeof(Thumb), Mouse.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(Thumb), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		UIElement.IsMouseOverPropertyKey.OverrideMetadata(typeof(Thumb), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
	}

	private static void OnIsDraggingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Thumb obj = (Thumb)d;
		obj.OnDraggingChanged(e);
		obj.UpdateVisualState();
	}

	/// <summary>Cancels a drag operation for the <see cref="T:System.Windows.Controls.Primitives.Thumb" />.</summary>
	public void CancelDrag()
	{
		if (IsDragging)
		{
			if (base.IsMouseCaptured)
			{
				ReleaseMouseCapture();
			}
			ClearValue(IsDraggingPropertyKey);
			RaiseEvent(new DragCompletedEventArgs(_previousScreenCoordPosition.X - _originScreenCoordPosition.X, _previousScreenCoordPosition.Y - _originScreenCoordPosition.Y, canceled: true));
		}
	}

	/// <summary>Responds to a change in the value of the <see cref="P:System.Windows.Controls.Primitives.Thumb.IsDragging" /> property. </summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnDraggingChanged(DependencyPropertyChangedEventArgs e)
	{
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (!base.IsEnabled)
		{
			VisualStateManager.GoToState(this, "Disabled", useTransitions);
		}
		else if (IsDragging)
		{
			VisualStateManager.GoToState(this, "Pressed", useTransitions);
		}
		else if (base.IsMouseOver)
		{
			VisualStateManager.GoToState(this, "MouseOver", useTransitions);
		}
		else
		{
			VisualStateManager.GoToState(this, "Normal", useTransitions);
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

	/// <summary>Creates an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.ThumbAutomationPeer" /> for the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new ThumbAutomationPeer(this);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.ContentElement.MouseLeftButtonDown" /> event. </summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		if (!IsDragging)
		{
			e.Handled = true;
			Focus();
			CaptureMouse();
			SetValue(IsDraggingPropertyKey, value: true);
			_originThumbPoint = e.GetPosition(this);
			_previousScreenCoordPosition = (_originScreenCoordPosition = SafeSecurityHelper.ClientToScreen(this, _originThumbPoint));
			bool flag = true;
			try
			{
				RaiseEvent(new DragStartedEventArgs(_originThumbPoint.X, _originThumbPoint.Y));
				flag = false;
			}
			finally
			{
				if (flag)
				{
					CancelDrag();
				}
			}
		}
		base.OnMouseLeftButtonDown(e);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.ContentElement.MouseLeftButtonUp" /> event. </summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		if (base.IsMouseCaptured && IsDragging)
		{
			e.Handled = true;
			ClearValue(IsDraggingPropertyKey);
			ReleaseMouseCapture();
			Point point = SafeSecurityHelper.ClientToScreen(this, e.MouseDevice.GetPosition(this));
			RaiseEvent(new DragCompletedEventArgs(point.X - _originScreenCoordPosition.X, point.Y - _originScreenCoordPosition.Y, canceled: false));
		}
		base.OnMouseLeftButtonUp(e);
	}

	private static void OnLostMouseCapture(object sender, MouseEventArgs e)
	{
		Thumb thumb = (Thumb)sender;
		if (Mouse.Captured != thumb)
		{
			thumb.CancelDrag();
		}
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.MouseMove" /> event. </summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (!IsDragging)
		{
			return;
		}
		if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
		{
			Point position = e.GetPosition(this);
			Point point = SafeSecurityHelper.ClientToScreen(this, position);
			if (point != _previousScreenCoordPosition)
			{
				_previousScreenCoordPosition = point;
				e.Handled = true;
				RaiseEvent(new DragDeltaEventArgs(position.X - _originThumbPoint.X, position.Y - _originThumbPoint.Y));
			}
		}
		else
		{
			if (e.MouseDevice.Captured == this)
			{
				ReleaseMouseCapture();
			}
			ClearValue(IsDraggingPropertyKey);
			_originThumbPoint.X = 0.0;
			_originThumbPoint.Y = 0.0;
		}
	}
}
