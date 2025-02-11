namespace System.Windows.Input;

/// <summary>Provides data for touch input events.</summary>
public class TouchEventArgs : InputEventArgs
{
	/// <summary>Gets the touch that generated the event.</summary>
	/// <returns>The touch that generated the event.</returns>
	public TouchDevice TouchDevice => (TouchDevice)base.Device;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.TouchEventArgs" /> class. </summary>
	/// <param name="touchDevice">The input device to associate with this event.</param>
	/// <param name="timestamp">The time when the input occurred.</param>
	public TouchEventArgs(TouchDevice touchDevice, int timestamp)
		: base(touchDevice, timestamp)
	{
	}

	/// <summary>Returns the current position of the touch device relative to the specified element.</summary>
	/// <returns>The current position of the touch device relative to the specified element.</returns>
	/// <param name="relativeTo">The element that defines the coordinate space.</param>
	public TouchPoint GetTouchPoint(IInputElement relativeTo)
	{
		return TouchDevice.GetTouchPoint(relativeTo);
	}

	/// <summary>Returns all touch points that were collected between the most recent and previous touch events.</summary>
	/// <returns>All touch points that were collected between the most recent and previous touch events.</returns>
	/// <param name="relativeTo">The element that defines the coordinate space.</param>
	public TouchPointCollection GetIntermediateTouchPoints(IInputElement relativeTo)
	{
		return TouchDevice.GetIntermediateTouchPoints(relativeTo);
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target on which to call the handler.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((EventHandler<TouchEventArgs>)genericHandler)(genericTarget, this);
	}
}
