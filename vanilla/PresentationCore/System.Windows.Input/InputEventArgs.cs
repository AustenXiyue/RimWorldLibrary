using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Provides data for input related events. </summary>
[FriendAccessAllowed]
public class InputEventArgs : RoutedEventArgs
{
	private InputDevice _inputDevice;

	private int _timestamp;

	/// <summary>Gets the input device that initiated this event.</summary>
	/// <returns>The input device associated with this event.</returns>
	public InputDevice Device
	{
		get
		{
			return _inputDevice;
		}
		internal set
		{
			_inputDevice = value;
		}
	}

	/// <summary>Gets the time when this event occurred. </summary>
	/// <returns>The timestamp.</returns>
	public int Timestamp => _timestamp;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputEventArgs" /> class. </summary>
	/// <param name="inputDevice">The input device to associate with this event.</param>
	/// <param name="timestamp">The time when the input occurred.</param>
	public InputEventArgs(InputDevice inputDevice, int timestamp)
	{
		_inputDevice = inputDevice;
		_timestamp = timestamp;
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((InputEventHandler)genericHandler)(genericTarget, this);
	}
}
