namespace System.Windows.Input;

/// <summary>Represents the mouse device to a specific thread.</summary>
public static class Mouse
{
	/// <summary>Identifies the <see cref="E:System.Windows.Input.Mouse.PreviewMouseMove" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseMove" /> attached event.</returns>
	public static readonly RoutedEvent PreviewMouseMoveEvent = EventManager.RegisterRoutedEvent("PreviewMouseMove", RoutingStrategy.Tunnel, typeof(MouseEventHandler), typeof(Mouse));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Mouse.MouseMove" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Mouse.MouseMove" /> attached event.</returns>
	public static readonly RoutedEvent MouseMoveEvent = EventManager.RegisterRoutedEvent("MouseMove", RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(Mouse));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Mouse.PreviewMouseDownOutsideCapturedElement" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseDownOutsideCapturedElement" /> attached event.</returns>
	public static readonly RoutedEvent PreviewMouseDownOutsideCapturedElementEvent = EventManager.RegisterRoutedEvent("PreviewMouseDownOutsideCapturedElement", RoutingStrategy.Tunnel, typeof(MouseButtonEventHandler), typeof(Mouse));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Mouse.PreviewMouseUpOutsideCapturedElement" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseUpOutsideCapturedElement" /> attached event.</returns>
	public static readonly RoutedEvent PreviewMouseUpOutsideCapturedElementEvent = EventManager.RegisterRoutedEvent("PreviewMouseUpOutsideCapturedElement", RoutingStrategy.Tunnel, typeof(MouseButtonEventHandler), typeof(Mouse));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Mouse.PreviewMouseDown" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseDown" /> attached event.</returns>
	public static readonly RoutedEvent PreviewMouseDownEvent = EventManager.RegisterRoutedEvent("PreviewMouseDown", RoutingStrategy.Tunnel, typeof(MouseButtonEventHandler), typeof(Mouse));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Mouse.MouseDown" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Mouse.MouseDown" /> attached event.</returns>
	public static readonly RoutedEvent MouseDownEvent = EventManager.RegisterRoutedEvent("MouseDown", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(Mouse));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Mouse.PreviewMouseUp" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseUp" /> attached event</returns>
	public static readonly RoutedEvent PreviewMouseUpEvent = EventManager.RegisterRoutedEvent("PreviewMouseUp", RoutingStrategy.Tunnel, typeof(MouseButtonEventHandler), typeof(Mouse));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Mouse.MouseUp" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Mouse.MouseUp" /> attached event.</returns>
	public static readonly RoutedEvent MouseUpEvent = EventManager.RegisterRoutedEvent("MouseUp", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(Mouse));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Mouse.PreviewMouseWheel" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseWheel" /> attached event.</returns>
	public static readonly RoutedEvent PreviewMouseWheelEvent = EventManager.RegisterRoutedEvent("PreviewMouseWheel", RoutingStrategy.Tunnel, typeof(MouseWheelEventHandler), typeof(Mouse));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Mouse.MouseWheel" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Mouse.MouseWheel" /> attached event.</returns>
	public static readonly RoutedEvent MouseWheelEvent = EventManager.RegisterRoutedEvent("MouseWheel", RoutingStrategy.Bubble, typeof(MouseWheelEventHandler), typeof(Mouse));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Mouse.MouseEnter" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Mouse.MouseEnter" /> attached event.</returns>
	public static readonly RoutedEvent MouseEnterEvent = EventManager.RegisterRoutedEvent("MouseEnter", RoutingStrategy.Direct, typeof(MouseEventHandler), typeof(Mouse));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Mouse.MouseLeave" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Mouse.MouseLeave" /> attached event.</returns>
	public static readonly RoutedEvent MouseLeaveEvent = EventManager.RegisterRoutedEvent("MouseLeave", RoutingStrategy.Direct, typeof(MouseEventHandler), typeof(Mouse));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Mouse.GotMouseCapture" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Mouse.GotMouseCapture" /> attached event.</returns>
	public static readonly RoutedEvent GotMouseCaptureEvent = EventManager.RegisterRoutedEvent("GotMouseCapture", RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(Mouse));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Mouse.LostMouseCapture" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Mouse.LostMouseCapture" /> attached event.</returns>
	public static readonly RoutedEvent LostMouseCaptureEvent = EventManager.RegisterRoutedEvent("LostMouseCapture", RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(Mouse));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Mouse.QueryCursor" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Mouse.QueryCursor" /> attached event.</returns>
	public static readonly RoutedEvent QueryCursorEvent = EventManager.RegisterRoutedEvent("QueryCursor", RoutingStrategy.Bubble, typeof(QueryCursorEventHandler), typeof(Mouse));

	/// <summary>Represents the number of units the mouse wheel is rotated to scroll one line. </summary>
	/// <returns>The units in one scroll line.</returns>
	public const int MouseWheelDeltaForOneLine = 120;

	/// <summary>Gets the element the mouse pointer is directly over.</summary>
	/// <returns>The element the mouse pointer is over.</returns>
	public static IInputElement DirectlyOver => PrimaryDevice.DirectlyOver;

	/// <summary>Gets the element that has captured the mouse. </summary>
	/// <returns>The element captured by the mouse.</returns>
	public static IInputElement Captured => PrimaryDevice.Captured;

	internal static CaptureMode CapturedMode => PrimaryDevice.CapturedMode;

	/// <summary>Gets or sets the cursor for the entire application. </summary>
	/// <returns>The override cursor or null if the <see cref="P:System.Windows.Input.Mouse.OverrideCursor" /> is not set.</returns>
	public static Cursor OverrideCursor
	{
		get
		{
			return PrimaryDevice.OverrideCursor;
		}
		set
		{
			PrimaryDevice.OverrideCursor = value;
		}
	}

	/// <summary>Gets the state of the left button of the mouse.</summary>
	/// <returns>The state of the left mouse button.</returns>
	public static MouseButtonState LeftButton => PrimaryDevice.LeftButton;

	/// <summary> Gets the state of the right button. </summary>
	/// <returns>The state of the right mouse button.</returns>
	public static MouseButtonState RightButton => PrimaryDevice.RightButton;

	/// <summary> Gets the state of the middle button of the mouse.</summary>
	/// <returns>The state of the middle mouse button.</returns>
	public static MouseButtonState MiddleButton => PrimaryDevice.MiddleButton;

	/// <summary> Gets the state of the first extended button. </summary>
	/// <returns>The state of the first extended mouse button.</returns>
	public static MouseButtonState XButton1 => PrimaryDevice.XButton1;

	/// <summary>Gets the state of the second extended button. </summary>
	/// <returns>The state of the second extended mouse button.</returns>
	public static MouseButtonState XButton2 => PrimaryDevice.XButton2;

	/// <summary>Gets the primary mouse device. </summary>
	/// <returns>The device.</returns>
	public static MouseDevice PrimaryDevice => InputManager.UnsecureCurrent.PrimaryMouseDevice;

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseMove" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void AddPreviewMouseMoveHandler(DependencyObject element, MouseEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewMouseMoveEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseMove" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void RemovePreviewMouseMoveHandler(DependencyObject element, MouseEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewMouseMoveEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Mouse.MouseMove" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void AddMouseMoveHandler(DependencyObject element, MouseEventHandler handler)
	{
		UIElement.AddHandler(element, MouseMoveEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Mouse.MouseMove" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void RemoveMouseMoveHandler(DependencyObject element, MouseEventHandler handler)
	{
		UIElement.RemoveHandler(element, MouseMoveEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseDownOutsideCapturedElement" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void AddPreviewMouseDownOutsideCapturedElementHandler(DependencyObject element, MouseButtonEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewMouseDownOutsideCapturedElementEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseDownOutsideCapturedElement" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void RemovePreviewMouseDownOutsideCapturedElementHandler(DependencyObject element, MouseButtonEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewMouseDownOutsideCapturedElementEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseUpOutsideCapturedElement" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void AddPreviewMouseUpOutsideCapturedElementHandler(DependencyObject element, MouseButtonEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewMouseUpOutsideCapturedElementEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseUpOutsideCapturedElement" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void RemovePreviewMouseUpOutsideCapturedElementHandler(DependencyObject element, MouseButtonEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewMouseUpOutsideCapturedElementEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseDown" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void AddPreviewMouseDownHandler(DependencyObject element, MouseButtonEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewMouseDownEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseDown" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void RemovePreviewMouseDownHandler(DependencyObject element, MouseButtonEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewMouseDownEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Mouse.MouseDown" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void AddMouseDownHandler(DependencyObject element, MouseButtonEventHandler handler)
	{
		UIElement.AddHandler(element, MouseDownEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Mouse.MouseDown" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void RemoveMouseDownHandler(DependencyObject element, MouseButtonEventHandler handler)
	{
		UIElement.RemoveHandler(element, MouseDownEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseUp" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void AddPreviewMouseUpHandler(DependencyObject element, MouseButtonEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewMouseUpEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseUp" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void RemovePreviewMouseUpHandler(DependencyObject element, MouseButtonEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewMouseUpEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Mouse.MouseUp" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void AddMouseUpHandler(DependencyObject element, MouseButtonEventHandler handler)
	{
		UIElement.AddHandler(element, MouseUpEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Mouse.MouseUp" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void RemoveMouseUpHandler(DependencyObject element, MouseButtonEventHandler handler)
	{
		UIElement.RemoveHandler(element, MouseUpEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseWheel" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void AddPreviewMouseWheelHandler(DependencyObject element, MouseWheelEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewMouseWheelEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Mouse.PreviewMouseWheel" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void RemovePreviewMouseWheelHandler(DependencyObject element, MouseWheelEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewMouseWheelEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Mouse.MouseWheel" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void AddMouseWheelHandler(DependencyObject element, MouseWheelEventHandler handler)
	{
		UIElement.AddHandler(element, MouseWheelEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Mouse.MouseWheel" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void RemoveMouseWheelHandler(DependencyObject element, MouseWheelEventHandler handler)
	{
		UIElement.RemoveHandler(element, MouseWheelEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Mouse.MouseEnter" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void AddMouseEnterHandler(DependencyObject element, MouseEventHandler handler)
	{
		UIElement.AddHandler(element, MouseEnterEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Mouse.MouseEnter" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void RemoveMouseEnterHandler(DependencyObject element, MouseEventHandler handler)
	{
		UIElement.RemoveHandler(element, MouseEnterEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Mouse.MouseLeave" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void AddMouseLeaveHandler(DependencyObject element, MouseEventHandler handler)
	{
		UIElement.AddHandler(element, MouseLeaveEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Mouse.MouseLeave" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void RemoveMouseLeaveHandler(DependencyObject element, MouseEventHandler handler)
	{
		UIElement.RemoveHandler(element, MouseLeaveEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Mouse.GotMouseCapture" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void AddGotMouseCaptureHandler(DependencyObject element, MouseEventHandler handler)
	{
		UIElement.AddHandler(element, GotMouseCaptureEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Mouse.GotMouseCapture" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void RemoveGotMouseCaptureHandler(DependencyObject element, MouseEventHandler handler)
	{
		UIElement.RemoveHandler(element, GotMouseCaptureEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Mouse.LostMouseCapture" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void AddLostMouseCaptureHandler(DependencyObject element, MouseEventHandler handler)
	{
		UIElement.AddHandler(element, LostMouseCaptureEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Mouse.LostMouseCapture" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void RemoveLostMouseCaptureHandler(DependencyObject element, MouseEventHandler handler)
	{
		UIElement.RemoveHandler(element, LostMouseCaptureEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Mouse.QueryCursor" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void AddQueryCursorHandler(DependencyObject element, QueryCursorEventHandler handler)
	{
		UIElement.AddHandler(element, QueryCursorEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Mouse.QueryCursor" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler.</param>
	public static void RemoveQueryCursorHandler(DependencyObject element, QueryCursorEventHandler handler)
	{
		UIElement.RemoveHandler(element, QueryCursorEvent, handler);
	}

	/// <summary>Captures mouse input to the specified element. </summary>
	/// <returns>true if the element was able to capture the mouse; otherwise, false.</returns>
	/// <param name="element">The element to capture the mouse.</param>
	public static bool Capture(IInputElement element)
	{
		return PrimaryDevice.Capture(element);
	}

	/// <summary>Captures mouse input to the specified element using the specified <see cref="T:System.Windows.Input.CaptureMode" />.</summary>
	/// <returns>true if the element was able to capture the mouse; otherwise, false.</returns>
	/// <param name="element">The element to capture the mouse.</param>
	/// <param name="captureMode">The capture policy to use.</param>
	public static bool Capture(IInputElement element, CaptureMode captureMode)
	{
		return PrimaryDevice.Capture(element, captureMode);
	}

	/// <summary>Retrieves up to 64 previous coordinates of the mouse pointer since the last mouse move event. </summary>
	/// <returns>The number of points returned.</returns>
	/// <param name="relativeTo">The elements <paramref name="points" /> are in relation to.</param>
	/// <param name="points">An array of objects.</param>
	public static int GetIntermediatePoints(IInputElement relativeTo, Point[] points)
	{
		if (PrimaryDevice.IsActive && relativeTo != null)
		{
			PresentationSource presentationSource = PresentationSource.FromDependencyObject(InputElement.GetContainingVisual(relativeTo as DependencyObject));
			if (presentationSource != null && presentationSource.GetInputProvider(typeof(MouseDevice)) is IMouseInputProvider mouseInputProvider)
			{
				return mouseInputProvider.GetIntermediatePoints(relativeTo, points);
			}
		}
		return -1;
	}

	/// <summary>Sets the mouse pointer to the specified <see cref="T:System.Windows.Input.Cursor" />. </summary>
	/// <returns>true, if the cursor was set; otherwise, false.</returns>
	/// <param name="cursor">The cursor to set the mouse pointer to.</param>
	public static bool SetCursor(Cursor cursor)
	{
		return PrimaryDevice.SetCursor(cursor);
	}

	/// <summary>Gets the position of the mouse relative to a specified element. </summary>
	/// <returns>The position of the mouse relative to the parameter <paramref name="relativeTo" />.</returns>
	/// <param name="relativeTo">The coordinate space in which to calculate the position of the mouse.</param>
	public static Point GetPosition(IInputElement relativeTo)
	{
		return PrimaryDevice.GetPosition(relativeTo);
	}

	/// <summary>Forces the mouse to resynchronize. </summary>
	public static void Synchronize()
	{
		PrimaryDevice.Synchronize();
	}

	/// <summary> Forces the mouse cursor to be updated. </summary>
	public static void UpdateCursor()
	{
		PrimaryDevice.UpdateCursor();
	}
}
