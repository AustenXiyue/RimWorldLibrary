namespace System.Windows.Input;

/// <summary>Provides data for keyboard-related events. </summary>
public class KeyboardEventArgs : InputEventArgs
{
	/// <summary>Gets the keyboard device associated with the input event. </summary>
	/// <returns>The logical keyboard device associated with the event.</returns>
	public KeyboardDevice KeyboardDevice => (KeyboardDevice)base.Device;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.KeyboardEventArgs" /> class. </summary>
	/// <param name="keyboard">The logical keyboard device associated with this event.</param>
	/// <param name="timestamp">The time when the input occurred.</param>
	public KeyboardEventArgs(KeyboardDevice keyboard, int timestamp)
		: base(keyboard, timestamp)
	{
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((KeyboardEventHandler)genericHandler)(genericTarget, this);
	}
}
