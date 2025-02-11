namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="E:System.Windows.Input.Touch.FrameReported" /> event.</summary>
public sealed class TouchFrameEventArgs : EventArgs
{
	/// <summary>Gets the time stamp for this event.</summary>
	/// <returns>The time stamp for this event.</returns>
	public int Timestamp { get; private set; }

	internal TouchFrameEventArgs(int timestamp)
	{
		Timestamp = timestamp;
	}

	/// <summary>Returns a collection that contains the current touch point for each active touch device relative to the specified element.</summary>
	/// <returns>A collection that contains the current <see cref="T:System.Windows.Input.TouchPoint" /> for each active <see cref="T:System.Windows.Input.TouchDevice" />.</returns>
	/// <param name="relativeTo">The element that defines the coordinate space. To use WPF absolute coordinates, specify <paramref name="relativeTo" /> as null.</param>
	public TouchPointCollection GetTouchPoints(IInputElement relativeTo)
	{
		return TouchDevice.GetTouchPoints(relativeTo);
	}

	/// <summary>Returns the current touch point of the primary touch device relative to the specified element.</summary>
	/// <returns>The current position of the primary <see cref="T:System.Windows.Input.TouchDevice" /> relative to the specified element; or null if the primary <see cref="T:System.Windows.Input.TouchDevice" /> is not active.</returns>
	/// <param name="relativeTo">The element that defines the coordinate space. To use WPF absolute coordinates, specify <paramref name="relativeTo" /> as null.</param>
	public TouchPoint GetPrimaryTouchPoint(IInputElement relativeTo)
	{
		return TouchDevice.GetPrimaryTouchPoint(relativeTo);
	}

	/// <summary>This member is not implemented.</summary>
	public void SuspendMousePromotionUntilTouchUp()
	{
	}
}
