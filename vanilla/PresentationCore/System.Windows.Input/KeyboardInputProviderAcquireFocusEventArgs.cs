namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="E:System.Windows.Input.Keyboard.KeyboardInputProviderAcquireFocus" /> event. </summary>
public class KeyboardInputProviderAcquireFocusEventArgs : KeyboardEventArgs
{
	private bool _focusAcquired;

	/// <summary>Gets a value that indicates whether interoperation focus was acquired. </summary>
	/// <returns>true if interoperation focus was acquired; otherwise, false.</returns>
	public bool FocusAcquired => _focusAcquired;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.KeyboardInputProviderAcquireFocusEventArgs" /> class. </summary>
	/// <param name="keyboard">The logical keyboard device associated with this event.</param>
	/// <param name="timestamp">The time when the input occurred.</param>
	/// <param name="focusAcquired">true to indicate that interoperation focus was acquired; otherwise, false. </param>
	public KeyboardInputProviderAcquireFocusEventArgs(KeyboardDevice keyboard, int timestamp, bool focusAcquired)
		: base(keyboard, timestamp)
	{
		_focusAcquired = focusAcquired;
	}

	/// <summary>Calls the type-specific handler on the target.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((KeyboardInputProviderAcquireFocusEventHandler)genericHandler)(genericTarget, this);
	}
}
