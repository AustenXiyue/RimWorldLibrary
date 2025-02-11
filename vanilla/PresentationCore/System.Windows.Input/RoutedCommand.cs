using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Defines a command that implements <see cref="T:System.Windows.Input.ICommand" /> and is routed through the element tree.</summary>
[TypeConverter("System.Windows.Input.CommandConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
[ValueSerializer("System.Windows.Input.CommandValueSerializer, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
public class RoutedCommand : ICommand
{
	private enum PrivateFlags : byte
	{
		IsBlockedByRM = 1,
		AreInputGesturesDelayLoaded
	}

	private string _name;

	private MS.Internal.SecurityCriticalDataForSet<PrivateFlags> _flags;

	private Type _ownerType;

	private InputGestureCollection _inputGestureCollection;

	private byte _commandId;

	/// <summary>Gets the name of the command. </summary>
	/// <returns>The name of the command.</returns>
	public string Name => _name;

	/// <summary>Gets the type that is registered with the command.</summary>
	/// <returns>The type of the command owner.</returns>
	public Type OwnerType => _ownerType;

	internal byte CommandId => _commandId;

	/// <summary>Gets the collection of <see cref="T:System.Windows.Input.InputGesture" /> objects that are associated with this command.</summary>
	/// <returns>The input gestures.</returns>
	public InputGestureCollection InputGestures
	{
		get
		{
			if (InputGesturesInternal == null)
			{
				_inputGestureCollection = new InputGestureCollection();
			}
			return _inputGestureCollection;
		}
	}

	internal InputGestureCollection InputGesturesInternal
	{
		get
		{
			if (_inputGestureCollection == null && AreInputGesturesDelayLoaded)
			{
				_inputGestureCollection = GetInputGestures();
				AreInputGesturesDelayLoaded = false;
			}
			return _inputGestureCollection;
		}
	}

	internal bool IsBlockedByRM
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.IsBlockedByRM);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.IsBlockedByRM, value);
		}
	}

	internal bool AreInputGesturesDelayLoaded
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.AreInputGesturesDelayLoaded);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.AreInputGesturesDelayLoaded, value);
		}
	}

	/// <summary>Occurs when changes to the command source are detected by the command manager. These changes often affect whether the command should execute on the current command target.</summary>
	public event EventHandler CanExecuteChanged
	{
		add
		{
			CommandManager.RequerySuggested += value;
		}
		remove
		{
			CommandManager.RequerySuggested -= value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.RoutedCommand" /> class.</summary>
	public RoutedCommand()
	{
		_name = string.Empty;
		_ownerType = null;
		_inputGestureCollection = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.RoutedCommand" /> class with the specified name and owner type.</summary>
	/// <param name="name">Declared name for serialization.</param>
	/// <param name="ownerType">The type which is registering the command.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">the length of <paramref name="name" /> is zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="ownerType" /> is null.</exception>
	public RoutedCommand(string name, Type ownerType)
		: this(name, ownerType, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.RoutedCommand" /> class with the specified name, owner type, and collection of gestures.</summary>
	/// <param name="name">Declared name for serialization.</param>
	/// <param name="ownerType">The type that is registering the command.</param>
	/// <param name="inputGestures">Default input gestures associated with this command.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">the length of <paramref name="name" /> is zero- or -<paramref name="ownerType" /> is null.</exception>
	public RoutedCommand(string name, Type ownerType, InputGestureCollection inputGestures)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException(SR.StringEmpty, "name");
		}
		if (ownerType == null)
		{
			throw new ArgumentNullException("ownerType");
		}
		_name = name;
		_ownerType = ownerType;
		_inputGestureCollection = inputGestures;
	}

	internal RoutedCommand(string name, Type ownerType, byte commandId)
		: this(name, ownerType, null)
	{
		_commandId = commandId;
	}

	/// <summary>For a description of this members, see <see cref="M:System.Windows.Input.ICommand.Execute(System.Object)" />.</summary>
	/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
	void ICommand.Execute(object parameter)
	{
		Execute(parameter, FilterInputElement(Keyboard.FocusedElement));
	}

	/// <summary>For a description of this members, see <see cref="M:System.Windows.Input.ICommand.CanExecute(System.Object)" />.</summary>
	/// <returns>true if this command can be executed; otherwise, false.</returns>
	/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
	bool ICommand.CanExecute(object parameter)
	{
		bool continueRouting;
		return CanExecuteImpl(parameter, FilterInputElement(Keyboard.FocusedElement), trusted: false, out continueRouting);
	}

	/// <summary>Executes the <see cref="T:System.Windows.Input.RoutedCommand" /> on the current command target.</summary>
	/// <param name="parameter">User defined parameter to be passed to the handler.</param>
	/// <param name="target">Element at which to begin looking for command handlers.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="target" /> is not a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />.</exception>
	public void Execute(object parameter, IInputElement target)
	{
		if (target != null && !InputElement.IsValid(target))
		{
			throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, target.GetType()));
		}
		if (target == null)
		{
			target = FilterInputElement(Keyboard.FocusedElement);
		}
		ExecuteImpl(parameter, target, userInitiated: false);
	}

	/// <summary>Determines whether this <see cref="T:System.Windows.Input.RoutedCommand" /> can execute in its current state.</summary>
	/// <returns>true if the command can execute on the current command target; otherwise, false.</returns>
	/// <param name="parameter">A user defined data type.</param>
	/// <param name="target">The command target.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="target" /> is not a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />.</exception>
	public bool CanExecute(object parameter, IInputElement target)
	{
		bool continueRouting;
		return CriticalCanExecute(parameter, target, trusted: false, out continueRouting);
	}

	internal bool CriticalCanExecute(object parameter, IInputElement target, bool trusted, out bool continueRouting)
	{
		if (target != null && !InputElement.IsValid(target))
		{
			throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, target.GetType()));
		}
		if (target == null)
		{
			target = FilterInputElement(Keyboard.FocusedElement);
		}
		return CanExecuteImpl(parameter, target, trusted, out continueRouting);
	}

	private InputGestureCollection GetInputGestures()
	{
		if (OwnerType == typeof(ApplicationCommands))
		{
			return ApplicationCommands.LoadDefaultGestureFromResource(_commandId);
		}
		if (OwnerType == typeof(NavigationCommands))
		{
			return NavigationCommands.LoadDefaultGestureFromResource(_commandId);
		}
		if (OwnerType == typeof(MediaCommands))
		{
			return MediaCommands.LoadDefaultGestureFromResource(_commandId);
		}
		if (OwnerType == typeof(ComponentCommands))
		{
			return ComponentCommands.LoadDefaultGestureFromResource(_commandId);
		}
		return new InputGestureCollection();
	}

	private static IInputElement FilterInputElement(IInputElement elem)
	{
		if (elem != null && InputElement.IsValid(elem))
		{
			return elem;
		}
		return null;
	}

	private bool CanExecuteImpl(object parameter, IInputElement target, bool trusted, out bool continueRouting)
	{
		if (target != null && !IsBlockedByRM)
		{
			CanExecuteRoutedEventArgs canExecuteRoutedEventArgs = new CanExecuteRoutedEventArgs(this, parameter);
			canExecuteRoutedEventArgs.RoutedEvent = CommandManager.PreviewCanExecuteEvent;
			CriticalCanExecuteWrapper(parameter, target, trusted, canExecuteRoutedEventArgs);
			if (!canExecuteRoutedEventArgs.Handled)
			{
				canExecuteRoutedEventArgs.RoutedEvent = CommandManager.CanExecuteEvent;
				CriticalCanExecuteWrapper(parameter, target, trusted, canExecuteRoutedEventArgs);
			}
			continueRouting = canExecuteRoutedEventArgs.ContinueRouting;
			return canExecuteRoutedEventArgs.CanExecute;
		}
		continueRouting = false;
		return false;
	}

	private void CriticalCanExecuteWrapper(object parameter, IInputElement target, bool trusted, CanExecuteRoutedEventArgs args)
	{
		DependencyObject dependencyObject = (DependencyObject)target;
		if (dependencyObject is UIElement uIElement)
		{
			uIElement.RaiseEvent(args, trusted);
		}
		else if (dependencyObject is ContentElement contentElement)
		{
			contentElement.RaiseEvent(args, trusted);
		}
		else if (dependencyObject is UIElement3D uIElement3D)
		{
			uIElement3D.RaiseEvent(args, trusted);
		}
	}

	internal bool ExecuteCore(object parameter, IInputElement target, bool userInitiated)
	{
		if (target == null)
		{
			target = FilterInputElement(Keyboard.FocusedElement);
		}
		return ExecuteImpl(parameter, target, userInitiated);
	}

	private bool ExecuteImpl(object parameter, IInputElement target, bool userInitiated)
	{
		if (target != null && !IsBlockedByRM)
		{
			UIElement uIElement = target as UIElement;
			ContentElement contentElement = null;
			UIElement3D uIElement3D = null;
			ExecutedRoutedEventArgs executedRoutedEventArgs = new ExecutedRoutedEventArgs(this, parameter);
			executedRoutedEventArgs.RoutedEvent = CommandManager.PreviewExecutedEvent;
			if (uIElement != null)
			{
				uIElement.RaiseEvent(executedRoutedEventArgs, userInitiated);
			}
			else
			{
				contentElement = target as ContentElement;
				if (contentElement != null)
				{
					contentElement.RaiseEvent(executedRoutedEventArgs, userInitiated);
				}
				else
				{
					uIElement3D = target as UIElement3D;
					uIElement3D?.RaiseEvent(executedRoutedEventArgs, userInitiated);
				}
			}
			if (!executedRoutedEventArgs.Handled)
			{
				executedRoutedEventArgs.RoutedEvent = CommandManager.ExecutedEvent;
				if (uIElement != null)
				{
					uIElement.RaiseEvent(executedRoutedEventArgs, userInitiated);
				}
				else if (contentElement != null)
				{
					contentElement.RaiseEvent(executedRoutedEventArgs, userInitiated);
				}
				else
				{
					uIElement3D?.RaiseEvent(executedRoutedEventArgs, userInitiated);
				}
			}
			return executedRoutedEventArgs.Handled;
		}
		return false;
	}

	private void WritePrivateFlag(PrivateFlags bit, bool value)
	{
		if (value)
		{
			_flags.Value |= bit;
		}
		else
		{
			_flags.Value &= (PrivateFlags)(byte)(~(int)bit);
		}
	}

	private bool ReadPrivateFlag(PrivateFlags bit)
	{
		return (_flags.Value & bit) != 0;
	}
}
