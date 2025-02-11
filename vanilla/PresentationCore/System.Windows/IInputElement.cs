using System.Windows.Input;

namespace System.Windows;

/// <summary>Establishes the common events and also the event-related properties and methods for basic input processing by Windows Presentation Foundation (WPF) elements.</summary>
public interface IInputElement
{
	/// <summary>Gets a value that indicates whether the mouse pointer is located over this element (including visual children elements that are inside its bounds). </summary>
	/// <returns>true if the mouse pointer is over the element or its child elements; otherwise, false.</returns>
	bool IsMouseOver { get; }

	/// <summary>Gets a value that indicates whether the mouse pointer is over this element in the strictest hit testing sense. </summary>
	/// <returns>true if the mouse pointer is over this element; otherwise, false.</returns>
	bool IsMouseDirectlyOver { get; }

	/// <summary>Gets a value that indicates whether the mouse is captured to this element. </summary>
	/// <returns>true if the element has mouse capture; otherwise, false.</returns>
	bool IsMouseCaptured { get; }

	/// <summary>Gets a value that indicates whether the stylus is located over this element (or over visual child elements that are inside its bounds). </summary>
	/// <returns>true if the stylus cursor is over the element or its child elements; otherwise, false.</returns>
	bool IsStylusOver { get; }

	/// <summary>Gets a value that indicates whether the stylus is over this element in the strictest hit testing sense. </summary>
	/// <returns>true if the stylus is over the element; otherwise, false.</returns>
	bool IsStylusDirectlyOver { get; }

	/// <summary>Gets a value that indicates whether the stylus is captured to this element. </summary>
	/// <returns>true if the element has stylus capture; otherwise, false.</returns>
	bool IsStylusCaptured { get; }

	/// <summary>Gets a value that indicates whether keyboard focus is anywhere inside the element bounds, including if keyboard focus is inside the bounds of any visual child elements. </summary>
	/// <returns>true if keyboard focus is on the element or its child elements; otherwise, false.</returns>
	bool IsKeyboardFocusWithin { get; }

	/// <summary>Gets a value that indicates whether this element has keyboard focus. </summary>
	/// <returns>true if this element has keyboard focus; otherwise, false.</returns>
	bool IsKeyboardFocused { get; }

	/// <summary>Gets a value that indicates whether this element is enabled in the user interface (UI). </summary>
	/// <returns>true if the element is enabled; otherwise, false.     </returns>
	bool IsEnabled { get; }

	/// <summary>Gets or sets a value that indicates whether focus can be set to this element.</summary>
	/// <returns>true if the element can have focus set to it; otherwise, false.</returns>
	bool Focusable { get; set; }

	/// <summary>Occurs when the left mouse button is pressed while the mouse pointer is over the element. </summary>
	event MouseButtonEventHandler PreviewMouseLeftButtonDown;

	/// <summary>Occurs when the left mouse button is pressed while the mouse pointer is over the element. </summary>
	event MouseButtonEventHandler MouseLeftButtonDown;

	/// <summary>Occurs when the left mouse button is released while the mouse pointer is over the element. </summary>
	event MouseButtonEventHandler PreviewMouseLeftButtonUp;

	/// <summary>Occurs when the left mouse button is released while the mouse pointer is over the element. </summary>
	event MouseButtonEventHandler MouseLeftButtonUp;

	/// <summary>Occurs when the right mouse button is pressed while the mouse pointer is over the element. </summary>
	event MouseButtonEventHandler PreviewMouseRightButtonDown;

	/// <summary>Occurs when the right mouse button is pressed while the mouse pointer is over the element. </summary>
	event MouseButtonEventHandler MouseRightButtonDown;

	/// <summary>Occurs when the right mouse button is released while the mouse pointer is over the element. </summary>
	event MouseButtonEventHandler PreviewMouseRightButtonUp;

	/// <summary>Occurs when the right mouse button is released while the mouse pointer is over the element. </summary>
	event MouseButtonEventHandler MouseRightButtonUp;

	/// <summary>Occurs when the mouse pointer moves while the mouse pointer is over the element. </summary>
	event MouseEventHandler PreviewMouseMove;

	/// <summary>Occurs when the mouse pointer moves while the mouse pointer is over the element. </summary>
	event MouseEventHandler MouseMove;

	/// <summary>Occurs when the mouse wheel moves while the mouse pointer is over this element. </summary>
	event MouseWheelEventHandler PreviewMouseWheel;

	/// <summary>Occurs when the mouse wheel moves while the mouse pointer is over this element. </summary>
	event MouseWheelEventHandler MouseWheel;

	/// <summary>Occurs when the mouse pointer enters the bounds of this element. </summary>
	event MouseEventHandler MouseEnter;

	/// <summary>Occurs when the mouse pointer leaves the bounds of this element. </summary>
	event MouseEventHandler MouseLeave;

	/// <summary>Occurs when the element captures the mouse. </summary>
	event MouseEventHandler GotMouseCapture;

	/// <summary>Occurs when this element loses mouse capture. </summary>
	event MouseEventHandler LostMouseCapture;

	/// <summary>Occurs when the stylus touches the digitizer while over this element. </summary>
	event StylusDownEventHandler PreviewStylusDown;

	/// <summary>Occurs when the stylus touches the digitizer while over this element. </summary>
	event StylusDownEventHandler StylusDown;

	/// <summary>Occurs when the stylus is raised off the digitizer while over this element. </summary>
	event StylusEventHandler PreviewStylusUp;

	/// <summary>Occurs when the stylus is raised off the digitizer while over this element. </summary>
	event StylusEventHandler StylusUp;

	/// <summary>Occurs when the stylus moves while the stylus is over the element. </summary>
	event StylusEventHandler PreviewStylusMove;

	/// <summary>Occurs when the stylus cursor moves over the element. </summary>
	event StylusEventHandler StylusMove;

	/// <summary>Occurs when the stylus moves over an element, but without touching the digitizer. </summary>
	event StylusEventHandler PreviewStylusInAirMove;

	/// <summary>Occurs when the stylus moves over an element, but without touching the digitizer. </summary>
	event StylusEventHandler StylusInAirMove;

	/// <summary>Occurs when the stylus cursor enters the bounds of the element. </summary>
	event StylusEventHandler StylusEnter;

	/// <summary>Occurs when the stylus cursor leaves the bounds of the element. </summary>
	event StylusEventHandler StylusLeave;

	/// <summary>Occurs when the stylus is close enough to the digitizer to be detected. </summary>
	event StylusEventHandler PreviewStylusInRange;

	/// <summary>Occurs when the stylus is close enough to the digitizer to be detected. </summary>
	event StylusEventHandler StylusInRange;

	/// <summary>Occurs when the stylus is too far from the digitizer to be detected. </summary>
	event StylusEventHandler PreviewStylusOutOfRange;

	/// <summary>Occurs when the stylus is too far from the digitizer to be detected. </summary>
	event StylusEventHandler StylusOutOfRange;

	/// <summary>Occurs when one of several stylus gestures are detected, for example, <see cref="F:System.Windows.Input.SystemGesture.Tap" /> or <see cref="F:System.Windows.Input.SystemGesture.Drag" />.</summary>
	event StylusSystemGestureEventHandler PreviewStylusSystemGesture;

	/// <summary>Occurs when one of several stylus gestures are detected, for example, <see cref="F:System.Windows.Input.SystemGesture.Tap" /> or <see cref="F:System.Windows.Input.SystemGesture.Drag" />. </summary>
	event StylusSystemGestureEventHandler StylusSystemGesture;

	/// <summary>Occurs when the stylus button is pressed while the stylus is over this element. </summary>
	event StylusButtonEventHandler StylusButtonDown;

	/// <summary>Occurs when the stylus button is pressed down while the stylus is over this element. </summary>
	event StylusButtonEventHandler PreviewStylusButtonDown;

	/// <summary>Occurs when the stylus button is released while the stylus is over this element. </summary>
	event StylusButtonEventHandler PreviewStylusButtonUp;

	/// <summary>Occurs when the stylus button is released while the stylus is over this element. </summary>
	event StylusButtonEventHandler StylusButtonUp;

	/// <summary>Occurs when the element captures the stylus. </summary>
	event StylusEventHandler GotStylusCapture;

	/// <summary>Occurs when this element loses stylus capture. </summary>
	event StylusEventHandler LostStylusCapture;

	/// <summary>Occurs when a key is pressed while the keyboard is focused on this element. </summary>
	event KeyEventHandler PreviewKeyDown;

	/// <summary>Occurs when a key is pressed while the keyboard is focused on this element. </summary>
	event KeyEventHandler KeyDown;

	/// <summary>Occurs when a key is released while the keyboard is focused on this element. </summary>
	event KeyEventHandler PreviewKeyUp;

	/// <summary>Occurs when a key is released while the keyboard is focused on this element. </summary>
	event KeyEventHandler KeyUp;

	/// <summary>Occurs when the keyboard is focused on this element. </summary>
	event KeyboardFocusChangedEventHandler PreviewGotKeyboardFocus;

	/// <summary>Occurs when the keyboard is focused on this element.</summary>
	event KeyboardFocusChangedEventHandler GotKeyboardFocus;

	/// <summary>Occurs when the keyboard is no longer focused on this element. </summary>
	event KeyboardFocusChangedEventHandler PreviewLostKeyboardFocus;

	/// <summary>Occurs when the keyboard is no longer focused on this element. </summary>
	event KeyboardFocusChangedEventHandler LostKeyboardFocus;

	/// <summary>Occurs when this element gets text in a device-independent manner. </summary>
	event TextCompositionEventHandler PreviewTextInput;

	/// <summary>Occurs when this element gets text in a device-independent manner. </summary>
	event TextCompositionEventHandler TextInput;

	/// <summary>Raises the routed event that is specified by the <see cref="P:System.Windows.RoutedEventArgs.RoutedEvent" /> property within the provided <see cref="T:System.Windows.RoutedEventArgs" />.</summary>
	/// <param name="e">An instance of the <see cref="T:System.Windows.RoutedEventArgs" /> class that contains the identifier for the event to raise. </param>
	void RaiseEvent(RoutedEventArgs e);

	/// <summary>Adds a routed event handler for a specific routed event to an element. </summary>
	/// <param name="routedEvent">The identifier for the routed event that is being handled.</param>
	/// <param name="handler">A reference to the handler implementation.</param>
	void AddHandler(RoutedEvent routedEvent, Delegate handler);

	/// <summary>Removes all instances of the specified routed event handler from this element. </summary>
	/// <param name="routedEvent">Identifier of the routed event for which the handler is attached.</param>
	/// <param name="handler">The specific handler implementation to remove from this element's event handler collection.</param>
	void RemoveHandler(RoutedEvent routedEvent, Delegate handler);

	/// <summary>Attempts to force capture of the mouse to this element. </summary>
	/// <returns>true if the mouse is successfully captured; otherwise, false.</returns>
	bool CaptureMouse();

	/// <summary>Releases the mouse capture, if this element holds the capture. </summary>
	void ReleaseMouseCapture();

	/// <summary>Attempts to force capture of the stylus to this element. </summary>
	/// <returns>true if the stylus is successfully captured; otherwise, false.</returns>
	bool CaptureStylus();

	/// <summary>Releases the stylus capture, if this element holds the capture. </summary>
	void ReleaseStylusCapture();

	/// <summary>Attempts to focus the keyboard on this element. </summary>
	/// <returns>true if keyboard focus is moved to this element or already was on this element; otherwise, false.</returns>
	bool Focus();
}
