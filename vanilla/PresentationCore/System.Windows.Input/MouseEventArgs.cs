namespace System.Windows.Input;

/// <summary>Provides data for mouse related routed events that do not specifically involve mouse buttons or the mouse wheel, for example <see cref="E:System.Windows.UIElement.MouseMove" />.</summary>
public class MouseEventArgs : InputEventArgs
{
	private StylusDevice _stylusDevice;

	/// <summary>Gets the mouse device associated with this event. </summary>
	/// <returns>The mouse device associated with this event.  There is no default value.</returns>
	public MouseDevice MouseDevice => (MouseDevice)base.Device;

	/// <summary>Gets the stylus device associated with this event.</summary>
	/// <returns>The stylus associated with this event.  There is no default value.</returns>
	public StylusDevice StylusDevice => _stylusDevice;

	/// <summary>Gets the current state of the left mouse button.</summary>
	/// <returns>The current state of the left mouse button, which is either <see cref="F:System.Windows.Input.MouseButtonState.Pressed" /> or <see cref="F:System.Windows.Input.MouseButtonState.Released" />.  There is no default value.</returns>
	public MouseButtonState LeftButton => MouseDevice.LeftButton;

	/// <summary>Gets the current state of the right mouse button.</summary>
	/// <returns>The current state of the right mouse button, which is either <see cref="F:System.Windows.Input.MouseButtonState.Pressed" /> or <see cref="F:System.Windows.Input.MouseButtonState.Released" />.  There is no default value.</returns>
	public MouseButtonState RightButton => MouseDevice.RightButton;

	/// <summary>Gets the current state of the middle mouse button.</summary>
	/// <returns>The current state of the middle mouse button, which is either <see cref="F:System.Windows.Input.MouseButtonState.Pressed" /> or <see cref="F:System.Windows.Input.MouseButtonState.Released" />. There is no default value.</returns>
	public MouseButtonState MiddleButton => MouseDevice.MiddleButton;

	/// <summary>Gets the current state of the first extended mouse button.</summary>
	/// <returns>The current state of the first extended mouse button, which is either <see cref="F:System.Windows.Input.MouseButtonState.Pressed" /> or <see cref="F:System.Windows.Input.MouseButtonState.Released" />.  There is no default value.</returns>
	public MouseButtonState XButton1 => MouseDevice.XButton1;

	/// <summary>Gets the state of the second extended mouse button.</summary>
	/// <returns>The current state of the second extended mouse button, which is either <see cref="F:System.Windows.Input.MouseButtonState.Pressed" /> or <see cref="F:System.Windows.Input.MouseButtonState.Released" />.  There is no default value.</returns>
	public MouseButtonState XButton2 => MouseDevice.XButton2;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.MouseEventArgs" /> class using the specified <see cref="T:System.Windows.Input.MouseDevice" /> and timestamp </summary>
	/// <param name="mouse">The mouse device associated with this event.</param>
	/// <param name="timestamp">The time when the input occurred.</param>
	public MouseEventArgs(MouseDevice mouse, int timestamp)
		: base(mouse, timestamp)
	{
		if (mouse == null)
		{
			throw new ArgumentNullException("mouse");
		}
		_stylusDevice = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.MouseEventArgs" /> class using the specified <see cref="T:System.Windows.Input.MouseDevice" />, timestamp, and <see cref="T:System.Windows.Input.StylusDevice" />.</summary>
	/// <param name="mouse">The mouse device associated with this event.</param>
	/// <param name="timestamp">The time when the input occurred.</param>
	/// <param name="stylusDevice">The logical stylus device associated with this event.</param>
	public MouseEventArgs(MouseDevice mouse, int timestamp, StylusDevice stylusDevice)
		: base(mouse, timestamp)
	{
		if (mouse == null)
		{
			throw new ArgumentNullException("mouse");
		}
		_stylusDevice = stylusDevice;
	}

	/// <summary>Returns the position of the mouse pointer relative to the specified element.</summary>
	/// <returns>The x- and y-coordinates of the mouse pointer position relative to the specified object.</returns>
	/// <param name="relativeTo">The element to use as the frame of reference for calculating the position of the mouse pointer.</param>
	public Point GetPosition(IInputElement relativeTo)
	{
		return MouseDevice.GetPosition(relativeTo);
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((MouseEventHandler)genericHandler)(genericTarget, this);
	}
}
