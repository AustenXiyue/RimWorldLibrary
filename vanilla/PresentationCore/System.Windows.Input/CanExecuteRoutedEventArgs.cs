namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="E:System.Windows.Input.CommandBinding.CanExecute" /> and <see cref="E:System.Windows.Input.CommandManager.PreviewCanExecute" /> routed events.</summary>
public sealed class CanExecuteRoutedEventArgs : RoutedEventArgs
{
	private ICommand _command;

	private object _parameter;

	private bool _canExecute;

	private bool _continueRouting;

	/// <summary>Gets the command associated with this event.</summary>
	/// <returns>The command. Unless the command is a custom command, this is generally a <see cref="T:System.Windows.Input.RoutedCommand" />. There is no default value.</returns>
	public ICommand Command => _command;

	/// <summary>Gets the command specific data.</summary>
	/// <returns>The command data.  The default value is null.</returns>
	public object Parameter => _parameter;

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Input.RoutedCommand" /> associated with this event can be executed on the command target.</summary>
	/// <returns>true if the event can be executed on the command target; otherwise, false.  The default value is false.</returns>
	public bool CanExecute
	{
		get
		{
			return _canExecute;
		}
		set
		{
			_canExecute = value;
		}
	}

	/// <summary>Determines whether the input routed event that invoked the command should continue to route through the element tree.</summary>
	/// <returns>true if the routed event should continue to route through element tree; otherwise, false.   The default value is false.</returns>
	public bool ContinueRouting
	{
		get
		{
			return _continueRouting;
		}
		set
		{
			_continueRouting = value;
		}
	}

	internal CanExecuteRoutedEventArgs(ICommand command, object parameter)
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
		((CanExecuteRoutedEventHandler)genericHandler)(target as DependencyObject, this);
	}
}
