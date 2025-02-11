namespace System.Windows.Input;

/// <summary>Provides data for various events that report changes to the mouse wheel delta value of a mouse device.</summary>
public class MouseWheelEventArgs : MouseEventArgs
{
	private static int _delta;

	/// <summary>Gets a value that indicates the amount that the mouse wheel has changed.</summary>
	/// <returns>The amount the wheel has changed. This value is positive if the mouse wheel is rotated in an upward direction (away from the user) or negative if the mouse wheel is rotated in a downward direction (toward the user).  </returns>
	public int Delta => _delta;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.MouseWheelEventArgs" /> class. </summary>
	/// <param name="mouse">The mouse device associated with this event.</param>
	/// <param name="timestamp">The time when the input occurred.</param>
	/// <param name="delta">The amount the wheel has changed.</param>
	public MouseWheelEventArgs(MouseDevice mouse, int timestamp, int delta)
		: base(mouse, timestamp)
	{
		_delta = delta;
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((MouseWheelEventHandler)genericHandler)(genericTarget, this);
	}
}
