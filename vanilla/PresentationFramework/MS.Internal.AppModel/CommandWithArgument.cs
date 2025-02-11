using System.Windows;
using System.Windows.Input;

namespace MS.Internal.AppModel;

internal class CommandWithArgument
{
	private object _argument;

	private MS.Internal.SecurityCriticalDataForSet<RoutedCommand> _command;

	public RoutedCommand Command => _command.Value;

	public CommandWithArgument(RoutedCommand command)
		: this(command, null)
	{
	}

	public CommandWithArgument(RoutedCommand command, object argument)
	{
		_command = new MS.Internal.SecurityCriticalDataForSet<RoutedCommand>(command);
		_argument = argument;
	}

	public bool Execute(IInputElement target, object argument)
	{
		if (argument == null)
		{
			argument = _argument;
		}
		if (_command.Value is ISecureCommand)
		{
			if (_command.Value.CriticalCanExecute(argument, target, trusted: true, out var _))
			{
				_command.Value.ExecuteCore(argument, target, userInitiated: true);
				return true;
			}
			return false;
		}
		if (_command.Value.CanExecute(argument, target))
		{
			_command.Value.Execute(argument, target);
			return true;
		}
		return false;
	}

	public bool QueryEnabled(IInputElement target, object argument)
	{
		if (argument == null)
		{
			argument = _argument;
		}
		bool continueRouting;
		if (_command.Value is ISecureCommand)
		{
			return _command.Value.CriticalCanExecute(argument, target, trusted: true, out continueRouting);
		}
		return _command.Value.CanExecute(argument, target);
	}
}
