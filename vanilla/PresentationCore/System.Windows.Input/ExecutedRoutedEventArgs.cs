namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="E:System.Windows.Input.CommandManager.Executed" /> and <see cref="E:System.Windows.Input.CommandManager.PreviewExecuted" /> routed events.</summary>
public sealed class ExecutedRoutedEventArgs : RoutedEventArgs
{
	private ICommand _command;

	private object _parameter;

	/// <summary>Gets the command that was invoked.</summary>
	/// <returns>The command associated with this event.  </returns>
	public ICommand Command => _command;

	/// <summary>Gets data parameter of the command.</summary>
	/// <returns>The command-specific data. The default value is null.</returns>
	public object Parameter => _parameter;

	internal ExecutedRoutedEventArgs(ICommand command, object parameter)
	{
		if (command == null)
		{
			throw new ArgumentNullException("command");
		}
		_command = command;
		_parameter = parameter;
	}

	protected override void InvokeEventHandler(Delegate genericHandler, object target)
	{
		((ExecutedRoutedEventHandler)genericHandler)(target as DependencyObject, this);
	}
}
