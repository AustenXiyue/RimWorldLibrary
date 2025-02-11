namespace System.Windows.Input;

/// <summary>Provides access to general information about a tablet pen.</summary>
public static class Stylus
{
	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.PreviewStylusDown" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusDown" /> attached event.</returns>
	public static readonly RoutedEvent PreviewStylusDownEvent = EventManager.RegisterRoutedEvent("PreviewStylusDown", RoutingStrategy.Tunnel, typeof(StylusDownEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.StylusDown" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.StylusDown" /> attached event.</returns>
	public static readonly RoutedEvent StylusDownEvent = EventManager.RegisterRoutedEvent("StylusDown", RoutingStrategy.Bubble, typeof(StylusDownEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.PreviewStylusUp" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusUp" /> attached event.</returns>
	public static readonly RoutedEvent PreviewStylusUpEvent = EventManager.RegisterRoutedEvent("PreviewStylusUp", RoutingStrategy.Tunnel, typeof(StylusEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.StylusUp" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.StylusUp" /> attached event.</returns>
	public static readonly RoutedEvent StylusUpEvent = EventManager.RegisterRoutedEvent("StylusUp", RoutingStrategy.Bubble, typeof(StylusEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.PreviewStylusMove" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusMove" /> attached event.</returns>
	public static readonly RoutedEvent PreviewStylusMoveEvent = EventManager.RegisterRoutedEvent("PreviewStylusMove", RoutingStrategy.Tunnel, typeof(StylusEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.StylusMove" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.StylusMove" /> attached event.</returns>
	public static readonly RoutedEvent StylusMoveEvent = EventManager.RegisterRoutedEvent("StylusMove", RoutingStrategy.Bubble, typeof(StylusEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.PreviewStylusInAirMove" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusInAirMove" /> attached event.</returns>
	public static readonly RoutedEvent PreviewStylusInAirMoveEvent = EventManager.RegisterRoutedEvent("PreviewStylusInAirMove", RoutingStrategy.Tunnel, typeof(StylusEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.StylusInAirMove" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.StylusInAirMove" /> attached event.</returns>
	public static readonly RoutedEvent StylusInAirMoveEvent = EventManager.RegisterRoutedEvent("StylusInAirMove", RoutingStrategy.Bubble, typeof(StylusEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.StylusEnter" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.StylusEnter" /> attached event.</returns>
	public static readonly RoutedEvent StylusEnterEvent = EventManager.RegisterRoutedEvent("StylusEnter", RoutingStrategy.Direct, typeof(StylusEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.StylusLeave" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.StylusLeave" /> attached event.</returns>
	public static readonly RoutedEvent StylusLeaveEvent = EventManager.RegisterRoutedEvent("StylusLeave", RoutingStrategy.Direct, typeof(StylusEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.PreviewStylusInRange" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusInRange" /> attached event.</returns>
	public static readonly RoutedEvent PreviewStylusInRangeEvent = EventManager.RegisterRoutedEvent("PreviewStylusInRange", RoutingStrategy.Tunnel, typeof(StylusEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.StylusInRange" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.StylusInRange" /> attached event.</returns>
	public static readonly RoutedEvent StylusInRangeEvent = EventManager.RegisterRoutedEvent("StylusInRange", RoutingStrategy.Bubble, typeof(StylusEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.PreviewStylusOutOfRange" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusOutOfRange" /> attached event.</returns>
	public static readonly RoutedEvent PreviewStylusOutOfRangeEvent = EventManager.RegisterRoutedEvent("PreviewStylusOutOfRange", RoutingStrategy.Tunnel, typeof(StylusEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.StylusOutOfRange" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.StylusOutOfRange" /> attached event.</returns>
	public static readonly RoutedEvent StylusOutOfRangeEvent = EventManager.RegisterRoutedEvent("StylusOutOfRange", RoutingStrategy.Bubble, typeof(StylusEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.PreviewStylusSystemGesture" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusSystemGesture" /> attached event.</returns>
	public static readonly RoutedEvent PreviewStylusSystemGestureEvent = EventManager.RegisterRoutedEvent("PreviewStylusSystemGesture", RoutingStrategy.Tunnel, typeof(StylusSystemGestureEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.StylusSystemGesture" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.StylusSystemGesture" /> attached event.</returns>
	public static readonly RoutedEvent StylusSystemGestureEvent = EventManager.RegisterRoutedEvent("StylusSystemGesture", RoutingStrategy.Bubble, typeof(StylusSystemGestureEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.GotStylusCapture" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.GotStylusCapture" /> attached event.</returns>
	public static readonly RoutedEvent GotStylusCaptureEvent = EventManager.RegisterRoutedEvent("GotStylusCapture", RoutingStrategy.Bubble, typeof(StylusEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.LostStylusCapture" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.LostStylusCapture" />  attached event.</returns>
	public static readonly RoutedEvent LostStylusCaptureEvent = EventManager.RegisterRoutedEvent("LostStylusCapture", RoutingStrategy.Bubble, typeof(StylusEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.StylusButtonDown" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.StylusButtonDown" /> attached event.</returns>
	public static readonly RoutedEvent StylusButtonDownEvent = EventManager.RegisterRoutedEvent("StylusButtonDown", RoutingStrategy.Bubble, typeof(StylusButtonEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.StylusButtonUp" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.StylusButtonUp" /> attached event.</returns>
	public static readonly RoutedEvent StylusButtonUpEvent = EventManager.RegisterRoutedEvent("StylusButtonUp", RoutingStrategy.Bubble, typeof(StylusButtonEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.PreviewStylusButtonDown" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusButtonDown" /> attached event.</returns>
	public static readonly RoutedEvent PreviewStylusButtonDownEvent = EventManager.RegisterRoutedEvent("PreviewStylusButtonDown", RoutingStrategy.Tunnel, typeof(StylusButtonEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Stylus.PreviewStylusButtonUp" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusButtonUp" /> attached event.</returns>
	public static readonly RoutedEvent PreviewStylusButtonUpEvent = EventManager.RegisterRoutedEvent("PreviewStylusButtonUp", RoutingStrategy.Tunnel, typeof(StylusButtonEventHandler), typeof(Stylus));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.Stylus.IsPressAndHoldEnabled" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.Stylus.IsPressAndHoldEnabled" /> attached property.</returns>
	public static readonly DependencyProperty IsPressAndHoldEnabledProperty = DependencyProperty.RegisterAttached("IsPressAndHoldEnabled", typeof(bool), typeof(Stylus), new PropertyMetadata(true));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.Stylus.IsFlicksEnabled" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.Stylus.IsFlicksEnabled" /> attached property.</returns>
	public static readonly DependencyProperty IsFlicksEnabledProperty = DependencyProperty.RegisterAttached("IsFlicksEnabled", typeof(bool), typeof(Stylus), new PropertyMetadata(true));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.Stylus.IsTapFeedbackEnabled" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.Stylus.IsTapFeedbackEnabled" /> attached property.</returns>
	public static readonly DependencyProperty IsTapFeedbackEnabledProperty = DependencyProperty.RegisterAttached("IsTapFeedbackEnabled", typeof(bool), typeof(Stylus), new PropertyMetadata(true));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.Stylus.IsTouchFeedbackEnabled" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.Stylus.IsTouchFeedbackEnabled" /> attached property.</returns>
	public static readonly DependencyProperty IsTouchFeedbackEnabledProperty = DependencyProperty.RegisterAttached("IsTouchFeedbackEnabled", typeof(bool), typeof(Stylus), new PropertyMetadata(true));

	/// <summary>Gets the element that is directly beneath the stylus.</summary>
	/// <returns>The <see cref="T:System.Windows.IInputElement" /> that is directly beneath the stylus.</returns>
	public static IInputElement DirectlyOver => CurrentStylusDevice?.DirectlyOver;

	/// <summary>Gets the element to which the stylus is bound.</summary>
	/// <returns>The <see cref="T:System.Windows.IInputElement" /> to which the stylus is bound.</returns>
	public static IInputElement Captured => CurrentStylusDevice?.Captured ?? Mouse.Captured;

	/// <summary>Gets the stylus that represents the stylus currently in use.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.StylusDevice" /> that represents the stylus currently in use.</returns>
	public static StylusDevice CurrentStylusDevice => StylusLogic.CurrentStylusLogic?.CurrentStylusDevice?.StylusDevice;

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusDown" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddPreviewStylusDownHandler(DependencyObject element, StylusDownEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewStylusDownEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusDown" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemovePreviewStylusDownHandler(DependencyObject element, StylusDownEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewStylusDownEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.StylusDown" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddStylusDownHandler(DependencyObject element, StylusDownEventHandler handler)
	{
		UIElement.AddHandler(element, StylusDownEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.StylusDown" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemoveStylusDownHandler(DependencyObject element, StylusDownEventHandler handler)
	{
		UIElement.RemoveHandler(element, StylusDownEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusUp" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddPreviewStylusUpHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewStylusUpEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusUp" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemovePreviewStylusUpHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewStylusUpEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.StylusUp" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddStylusUpHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.AddHandler(element, StylusUpEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.StylusUp" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemoveStylusUpHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.RemoveHandler(element, StylusUpEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusMove" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddPreviewStylusMoveHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewStylusMoveEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusMove" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemovePreviewStylusMoveHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewStylusMoveEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.StylusMove" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddStylusMoveHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.AddHandler(element, StylusMoveEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.StylusMove" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemoveStylusMoveHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.RemoveHandler(element, StylusMoveEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusInAirMove" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddPreviewStylusInAirMoveHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewStylusInAirMoveEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusInAirMove" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemovePreviewStylusInAirMoveHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewStylusInAirMoveEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.StylusInAirMove" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddStylusInAirMoveHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.AddHandler(element, StylusInAirMoveEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.StylusInAirMove" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemoveStylusInAirMoveHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.RemoveHandler(element, StylusInAirMoveEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.StylusEnter" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddStylusEnterHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.AddHandler(element, StylusEnterEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.StylusEnter" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemoveStylusEnterHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.RemoveHandler(element, StylusEnterEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.StylusLeave" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddStylusLeaveHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.AddHandler(element, StylusLeaveEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.StylusLeave" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemoveStylusLeaveHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.RemoveHandler(element, StylusLeaveEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusInRange" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddPreviewStylusInRangeHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewStylusInRangeEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusInRange" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemovePreviewStylusInRangeHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewStylusInRangeEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.StylusInRange" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddStylusInRangeHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.AddHandler(element, StylusInRangeEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.StylusInRange" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemoveStylusInRangeHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.RemoveHandler(element, StylusInRangeEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusOutOfRange" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddPreviewStylusOutOfRangeHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewStylusOutOfRangeEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusOutOfRange" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemovePreviewStylusOutOfRangeHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewStylusOutOfRangeEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.StylusOutOfRange" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddStylusOutOfRangeHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.AddHandler(element, StylusOutOfRangeEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.StylusOutOfRange" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemoveStylusOutOfRangeHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.RemoveHandler(element, StylusOutOfRangeEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusSystemGesture" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddPreviewStylusSystemGestureHandler(DependencyObject element, StylusSystemGestureEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewStylusSystemGestureEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusSystemGesture" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemovePreviewStylusSystemGestureHandler(DependencyObject element, StylusSystemGestureEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewStylusSystemGestureEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.StylusSystemGesture" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddStylusSystemGestureHandler(DependencyObject element, StylusSystemGestureEventHandler handler)
	{
		UIElement.AddHandler(element, StylusSystemGestureEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.StylusSystemGesture" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemoveStylusSystemGestureHandler(DependencyObject element, StylusSystemGestureEventHandler handler)
	{
		UIElement.RemoveHandler(element, StylusSystemGestureEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.GotStylusCapture" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddGotStylusCaptureHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.AddHandler(element, GotStylusCaptureEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.GotStylusCapture" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemoveGotStylusCaptureHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.RemoveHandler(element, GotStylusCaptureEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.LostStylusCapture" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddLostStylusCaptureHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.AddHandler(element, LostStylusCaptureEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.LostStylusCapture" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemoveLostStylusCaptureHandler(DependencyObject element, StylusEventHandler handler)
	{
		UIElement.RemoveHandler(element, LostStylusCaptureEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.StylusButtonDown" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddStylusButtonDownHandler(DependencyObject element, StylusButtonEventHandler handler)
	{
		UIElement.AddHandler(element, StylusButtonDownEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.StylusButtonDown" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemoveStylusButtonDownHandler(DependencyObject element, StylusButtonEventHandler handler)
	{
		UIElement.RemoveHandler(element, StylusButtonDownEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.StylusButtonUp" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddStylusButtonUpHandler(DependencyObject element, StylusButtonEventHandler handler)
	{
		UIElement.AddHandler(element, StylusButtonUpEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.StylusButtonUp" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemoveStylusButtonUpHandler(DependencyObject element, StylusButtonEventHandler handler)
	{
		UIElement.RemoveHandler(element, StylusButtonUpEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusButtonDown" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddPreviewStylusButtonDownHandler(DependencyObject element, StylusButtonEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewStylusButtonDownEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusButtonDown" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemovePreviewStylusButtonDownHandler(DependencyObject element, StylusButtonEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewStylusButtonDownEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusButtonUp" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to add.</param>
	public static void AddPreviewStylusButtonUpHandler(DependencyObject element, StylusButtonEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewStylusButtonUpEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Stylus.PreviewStylusButtonUp" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to remove.</param>
	public static void RemovePreviewStylusButtonUpHandler(DependencyObject element, StylusButtonEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewStylusButtonUpEvent, handler);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Input.Stylus.IsPressAndHoldEnabled" /> attached property on the specified element.</summary>
	/// <returns>true if the specified element has press and hold enabled; otherwise, false;</returns>
	/// <param name="element">A <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> on which to determine whether press and hold is enabled.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetIsPressAndHoldEnabled(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		object value = element.GetValue(IsPressAndHoldEnabledProperty);
		if (value == null)
		{
			return false;
		}
		return (bool)value;
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.Stylus.IsPressAndHoldEnabled" /> attached property on the specified element.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> on which to enable press and hold.</param>
	/// <param name="enabled">true to enable press and hold; false to disable press and hold.</param>
	public static void SetIsPressAndHoldEnabled(DependencyObject element, bool enabled)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsPressAndHoldEnabledProperty, enabled);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Input.Stylus.IsFlicksEnabled" /> attached property on the specified element.</summary>
	/// <returns>true if the specified element has flicks enabled; otherwise, false.</returns>
	/// <param name="element">A <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> on which to determine whether flicks are enabled.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetIsFlicksEnabled(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		object value = element.GetValue(IsFlicksEnabledProperty);
		if (value == null)
		{
			return false;
		}
		return (bool)value;
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Input.Stylus.IsFlicksEnabled" /> attached property on the specified element.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> on which to enable flicks.</param>
	/// <param name="enabled">true to enable flicks; false to disable flicks.</param>
	public static void SetIsFlicksEnabled(DependencyObject element, bool enabled)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsFlicksEnabledProperty, enabled);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Input.Stylus.IsTapFeedbackEnabled" /> attached property on the specified element.</summary>
	/// <returns>true if the specified element has tap feedback enabled; otherwise, false.</returns>
	/// <param name="element">A <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> on which to determine whether tap feedback enabled.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetIsTapFeedbackEnabled(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		object value = element.GetValue(IsTapFeedbackEnabledProperty);
		if (value == null)
		{
			return false;
		}
		return (bool)value;
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.Stylus.IsTapFeedbackEnabled" /> attached property on the specified element.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> on which to enable tap feedback.</param>
	/// <param name="enabled">true to enable tap feedback; false to disable tap feedback.</param>
	public static void SetIsTapFeedbackEnabled(DependencyObject element, bool enabled)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsTapFeedbackEnabledProperty, enabled);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Input.Stylus.IsTouchFeedbackEnabled" /> attached property on the specified element.</summary>
	/// <returns>true if touch input feedback is enabled, otherwise false.</returns>
	/// <param name="element">A <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> on which to determine whether touch input feedback enabled.</param>
	public static bool GetIsTouchFeedbackEnabled(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		object value = element.GetValue(IsTouchFeedbackEnabledProperty);
		if (value == null)
		{
			return false;
		}
		return (bool)value;
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.Stylus.IsTouchFeedbackEnabled" /> attached property on the specified element.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> on which to enable tap feedback.</param>
	/// <param name="enabled">true to enable touch input feedback; false to disable touch input feedback.</param>
	public static void SetIsTouchFeedbackEnabled(DependencyObject element, bool enabled)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsTouchFeedbackEnabledProperty, enabled);
	}

	/// <summary>Captures the stylus to the specified element.</summary>
	/// <returns>true if the stylus is captured to <paramref name="element" />; otherwise, false.</returns>
	/// <param name="element">The element to capture the stylus to.</param>
	public static bool Capture(IInputElement element)
	{
		return Capture(element, CaptureMode.Element);
	}

	/// <summary>Captures the stylus to the specified element.</summary>
	/// <returns>true if the stylus is captured to <paramref name="element" />; otherwise, false.</returns>
	/// <param name="element">The element to capture the stylus to.</param>
	/// <param name="captureMode">One of the <see cref="T:System.Windows.Input.CaptureMode" /> values.</param>
	public static bool Capture(IInputElement element, CaptureMode captureMode)
	{
		return Mouse.Capture(element, captureMode);
	}

	/// <summary>Synchronizes the cursor and the user interface.</summary>
	public static void Synchronize()
	{
		CurrentStylusDevice?.Synchronize();
	}
}
