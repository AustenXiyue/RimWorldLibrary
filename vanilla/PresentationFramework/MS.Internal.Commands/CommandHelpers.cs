using System;
using System.Windows;
using System.Windows.Input;

namespace MS.Internal.Commands;

internal static class CommandHelpers
{
	internal static void RegisterCommandHandler(Type controlType, RoutedCommand command, ExecutedRoutedEventHandler executedRoutedEventHandler)
	{
		PrivateRegisterCommandHandler(controlType, command, executedRoutedEventHandler, (CanExecuteRoutedEventHandler)null, (InputGesture[])null);
	}

	internal static void RegisterCommandHandler(Type controlType, RoutedCommand command, ExecutedRoutedEventHandler executedRoutedEventHandler, InputGesture inputGesture)
	{
		PrivateRegisterCommandHandler(controlType, command, executedRoutedEventHandler, null, inputGesture);
	}

	internal static void RegisterCommandHandler(Type controlType, RoutedCommand command, ExecutedRoutedEventHandler executedRoutedEventHandler, Key key)
	{
		PrivateRegisterCommandHandler(controlType, command, executedRoutedEventHandler, null, new KeyGesture(key));
	}

	internal static void RegisterCommandHandler(Type controlType, RoutedCommand command, ExecutedRoutedEventHandler executedRoutedEventHandler, InputGesture inputGesture, InputGesture inputGesture2)
	{
		PrivateRegisterCommandHandler(controlType, command, executedRoutedEventHandler, null, inputGesture, inputGesture2);
	}

	internal static void RegisterCommandHandler(Type controlType, RoutedCommand command, ExecutedRoutedEventHandler executedRoutedEventHandler, CanExecuteRoutedEventHandler canExecuteRoutedEventHandler)
	{
		PrivateRegisterCommandHandler(controlType, command, executedRoutedEventHandler, canExecuteRoutedEventHandler, (InputGesture[])null);
	}

	internal static void RegisterCommandHandler(Type controlType, RoutedCommand command, ExecutedRoutedEventHandler executedRoutedEventHandler, CanExecuteRoutedEventHandler canExecuteRoutedEventHandler, InputGesture inputGesture)
	{
		PrivateRegisterCommandHandler(controlType, command, executedRoutedEventHandler, canExecuteRoutedEventHandler, inputGesture);
	}

	internal static void RegisterCommandHandler(Type controlType, RoutedCommand command, ExecutedRoutedEventHandler executedRoutedEventHandler, CanExecuteRoutedEventHandler canExecuteRoutedEventHandler, Key key)
	{
		PrivateRegisterCommandHandler(controlType, command, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(key));
	}

	internal static void RegisterCommandHandler(Type controlType, RoutedCommand command, ExecutedRoutedEventHandler executedRoutedEventHandler, CanExecuteRoutedEventHandler canExecuteRoutedEventHandler, InputGesture inputGesture, InputGesture inputGesture2)
	{
		PrivateRegisterCommandHandler(controlType, command, executedRoutedEventHandler, canExecuteRoutedEventHandler, inputGesture, inputGesture2);
	}

	internal static void RegisterCommandHandler(Type controlType, RoutedCommand command, ExecutedRoutedEventHandler executedRoutedEventHandler, CanExecuteRoutedEventHandler canExecuteRoutedEventHandler, InputGesture inputGesture, InputGesture inputGesture2, InputGesture inputGesture3, InputGesture inputGesture4)
	{
		PrivateRegisterCommandHandler(controlType, command, executedRoutedEventHandler, canExecuteRoutedEventHandler, inputGesture, inputGesture2, inputGesture3, inputGesture4);
	}

	internal static void RegisterCommandHandler(Type controlType, RoutedCommand command, Key key, ModifierKeys modifierKeys, ExecutedRoutedEventHandler executedRoutedEventHandler, CanExecuteRoutedEventHandler canExecuteRoutedEventHandler)
	{
		PrivateRegisterCommandHandler(controlType, command, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(key, modifierKeys));
	}

	internal static void RegisterCommandHandler(Type controlType, RoutedCommand command, ExecutedRoutedEventHandler executedRoutedEventHandler, string srid1, string srid2)
	{
		PrivateRegisterCommandHandler(controlType, command, executedRoutedEventHandler, null, KeyGesture.CreateFromResourceStrings(SR.GetResourceString(srid1), SR.GetResourceString(srid2)));
	}

	internal static void RegisterCommandHandler(Type controlType, RoutedCommand command, ExecutedRoutedEventHandler executedRoutedEventHandler, CanExecuteRoutedEventHandler canExecuteRoutedEventHandler, string srid1, string srid2)
	{
		PrivateRegisterCommandHandler(controlType, command, executedRoutedEventHandler, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings(SR.GetResourceString(srid1), SR.GetResourceString(srid2)));
	}

	private static void PrivateRegisterCommandHandler(Type controlType, RoutedCommand command, ExecutedRoutedEventHandler executedRoutedEventHandler, CanExecuteRoutedEventHandler canExecuteRoutedEventHandler, params InputGesture[] inputGestures)
	{
		CommandManager.RegisterClassCommandBinding(controlType, new CommandBinding(command, executedRoutedEventHandler, canExecuteRoutedEventHandler));
		if (inputGestures != null)
		{
			for (int i = 0; i < inputGestures.Length; i++)
			{
				CommandManager.RegisterClassInputBinding(controlType, new InputBinding(command, inputGestures[i]));
			}
		}
	}

	internal static bool CanExecuteCommandSource(ICommandSource commandSource)
	{
		ICommand command = commandSource.Command;
		if (command != null)
		{
			object commandParameter = commandSource.CommandParameter;
			IInputElement inputElement = commandSource.CommandTarget;
			if (command is RoutedCommand routedCommand)
			{
				if (inputElement == null)
				{
					inputElement = commandSource as IInputElement;
				}
				return routedCommand.CanExecute(commandParameter, inputElement);
			}
			return command.CanExecute(commandParameter);
		}
		return false;
	}

	internal static void ExecuteCommandSource(ICommandSource commandSource)
	{
		CriticalExecuteCommandSource(commandSource, userInitiated: false);
	}

	internal static void CriticalExecuteCommandSource(ICommandSource commandSource, bool userInitiated)
	{
		ICommand command = commandSource.Command;
		if (command == null)
		{
			return;
		}
		object commandParameter = commandSource.CommandParameter;
		IInputElement inputElement = commandSource.CommandTarget;
		if (command is RoutedCommand routedCommand)
		{
			if (inputElement == null)
			{
				inputElement = commandSource as IInputElement;
			}
			if (routedCommand.CanExecute(commandParameter, inputElement))
			{
				routedCommand.ExecuteCore(commandParameter, inputElement, userInitiated);
			}
		}
		else if (command.CanExecute(commandParameter))
		{
			command.Execute(commandParameter);
		}
	}

	internal static void ExecuteCommand(ICommand command, object parameter, IInputElement target)
	{
		if (command is RoutedCommand routedCommand)
		{
			if (routedCommand.CanExecute(parameter, target))
			{
				routedCommand.Execute(parameter, target);
			}
		}
		else if (command.CanExecute(parameter))
		{
			command.Execute(parameter);
		}
	}
}
