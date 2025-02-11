namespace System.Windows.Input;

/// <summary>Represents a single touch point from a multitouch message source.</summary>
public class TouchPoint : IEquatable<TouchPoint>
{
	/// <summary>Gets the touch device that generated this <see cref="T:System.Windows.Input.TouchPoint" />.</summary>
	/// <returns>The touch device that generated this <see cref="T:System.Windows.Input.TouchPoint" />.</returns>
	public TouchDevice TouchDevice { get; private set; }

	/// <summary>Gets the location of the touch point.</summary>
	/// <returns>The location of the touch point.</returns>
	public Point Position { get; private set; }

	/// <summary>Gets the bounds of the area that the finger has in contact with the screen. </summary>
	/// <returns>The bounds of the area that the finger has in contact with the screen. </returns>
	public Rect Bounds { get; private set; }

	/// <summary>Gets the size of the <see cref="P:System.Windows.Input.TouchPoint.Bounds" /> property.</summary>
	/// <returns>The size of the <see cref="P:System.Windows.Input.TouchPoint.Bounds" /> property.</returns>
	public Size Size => Bounds.Size;

	/// <summary>Gets the last action that occurred at this location.</summary>
	/// <returns>The last action that occurred at this location.</returns>
	public TouchAction Action { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.TouchPoint" /> class. </summary>
	/// <param name="device">The touch device that generated this <see cref="T:System.Windows.Input.TouchPoint" />.</param>
	/// <param name="position">The location of the touch point.</param>
	/// <param name="bounds">The bounds of the area that the finger has in contact with the screen. </param>
	/// <param name="action">The last action that occurred by this device at this location.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="device" /> is null.</exception>
	public TouchPoint(TouchDevice device, Point position, Rect bounds, TouchAction action)
	{
		if (device == null)
		{
			throw new ArgumentNullException("device");
		}
		TouchDevice = device;
		Position = position;
		Bounds = bounds;
		Action = action;
	}

	bool IEquatable<TouchPoint>.Equals(TouchPoint other)
	{
		if (other != null)
		{
			if (other.TouchDevice == TouchDevice && other.Position == Position && other.Bounds == Bounds)
			{
				return other.Action == Action;
			}
			return false;
		}
		return false;
	}
}
