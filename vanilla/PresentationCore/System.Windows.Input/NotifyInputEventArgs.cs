namespace System.Windows.Input;

/// <summary>Provides data for raw input being processed by the <see cref="P:System.Windows.Input.NotifyInputEventArgs.InputManager" />. </summary>
public class NotifyInputEventArgs : EventArgs
{
	private StagingAreaInputItem _input;

	private InputManager _inputManager;

	/// <summary>Gets the staging area input item being processed by the input manager. </summary>
	/// <returns>The staging area.</returns>
	public StagingAreaInputItem StagingItem => _input;

	/// <summary>Gets the input manager processing the input event. </summary>
	/// <returns>The input manager.</returns>
	public InputManager InputManager => _inputManager;

	internal InputManager UnsecureInputManager => _inputManager;

	internal NotifyInputEventArgs()
	{
	}

	internal virtual void Reset(StagingAreaInputItem input, InputManager inputManager)
	{
		_input = input;
		_inputManager = inputManager;
	}
}
