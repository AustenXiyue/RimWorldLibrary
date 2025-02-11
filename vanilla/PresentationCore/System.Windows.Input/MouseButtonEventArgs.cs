namespace System.Windows.Input;

/// <summary>Provides data for mouse button related events. </summary>
public class MouseButtonEventArgs : MouseEventArgs
{
	private MouseButton _button;

	private int _count;

	/// <summary>Gets the button associated with the event. </summary>
	/// <returns>The button which was pressed.</returns>
	public MouseButton ChangedButton => _button;

	/// <summary>Gets the state of the button associated with the event. </summary>
	/// <returns>The state the button is in.</returns>
	public MouseButtonState ButtonState
	{
		get
		{
			MouseButtonState result = MouseButtonState.Released;
			switch (_button)
			{
			case MouseButton.Left:
				result = base.MouseDevice.LeftButton;
				break;
			case MouseButton.Right:
				result = base.MouseDevice.RightButton;
				break;
			case MouseButton.Middle:
				result = base.MouseDevice.MiddleButton;
				break;
			case MouseButton.XButton1:
				result = base.MouseDevice.XButton1;
				break;
			case MouseButton.XButton2:
				result = base.MouseDevice.XButton2;
				break;
			}
			return result;
		}
	}

	/// <summary>Gets the number of times the button was clicked. </summary>
	/// <returns>The number of times the mouse button was clicked.</returns>
	public int ClickCount
	{
		get
		{
			return _count;
		}
		internal set
		{
			_count = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> class by using the specified <see cref="T:System.Windows.Input.MouseDevice" />, timestamp, and <see cref="T:System.Windows.Input.MouseButton" />. </summary>
	/// <param name="mouse">The logical Mouse device associated with this event.</param>
	/// <param name="timestamp">The time when the input occurred.</param>
	/// <param name="button">The mouse button whose state is being described.</param>
	public MouseButtonEventArgs(MouseDevice mouse, int timestamp, MouseButton button)
		: base(mouse, timestamp)
	{
		MouseButtonUtilities.Validate(button);
		_button = button;
		_count = 1;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> class by using the specified <see cref="T:System.Windows.Input.MouseDevice" />, timestamp, <see cref="T:System.Windows.Input.MouseButton" />, and <see cref="T:System.Windows.Input.StylusDevice" />.  .</summary>
	/// <param name="mouse">The logical mouse device associated with this event.</param>
	/// <param name="timestamp">The time the event occurred.</param>
	/// <param name="button">The button associated with this event.</param>
	/// <param name="stylusDevice">The stylus device associated with this event.</param>
	public MouseButtonEventArgs(MouseDevice mouse, int timestamp, MouseButton button, StylusDevice stylusDevice)
		: base(mouse, timestamp, stylusDevice)
	{
		MouseButtonUtilities.Validate(button);
		_button = button;
		_count = 1;
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((MouseButtonEventHandler)genericHandler)(genericTarget, this);
	}
}
