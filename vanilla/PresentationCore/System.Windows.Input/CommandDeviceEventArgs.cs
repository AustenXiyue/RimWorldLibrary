namespace System.Windows.Input;

internal class CommandDeviceEventArgs : InputEventArgs
{
	private ICommand _command;

	internal ICommand Command => _command;

	internal CommandDeviceEventArgs(CommandDevice commandDevice, int timestamp, ICommand command)
		: base(commandDevice, timestamp)
	{
		if (command == null)
		{
			throw new ArgumentNullException("command");
		}
		_command = command;
	}

	protected override void InvokeEventHandler(Delegate execHandler, object target)
	{
		((CommandDeviceEventHandler)execHandler)(target, this);
	}
}
