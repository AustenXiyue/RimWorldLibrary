namespace System.Windows.Input;

/// <summary>Binds a <see cref="T:System.Windows.Input.RoutedCommand" /> to the event handlers that implement the command. </summary>
public class CommandBinding
{
	private ICommand _command;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Input.ICommand" /> associated with this <see cref="T:System.Windows.Input.CommandBinding" />. </summary>
	/// <returns>The command associated with this binding.</returns>
	[Localizability(LocalizationCategory.NeverLocalize)]
	public ICommand Command
	{
		get
		{
			return _command;
		}
		set
		{
			_command = value ?? throw new ArgumentNullException("value");
		}
	}

	/// <summary>Occurs when the command associated with this <see cref="T:System.Windows.Input.CommandBinding" /> executes.</summary>
	public event ExecutedRoutedEventHandler PreviewExecuted;

	/// <summary>Occurs when the command associated with this <see cref="T:System.Windows.Input.CommandBinding" /> executes.</summary>
	public event ExecutedRoutedEventHandler Executed;

	/// <summary>Occurs when the command associated with this <see cref="T:System.Windows.Input.CommandBinding" /> initiates a check to determine whether the command can be executed on the current command target.</summary>
	public event CanExecuteRoutedEventHandler PreviewCanExecute;

	/// <summary>Occurs when the command associated with this <see cref="T:System.Windows.Input.CommandBinding" /> initiates a check to determine whether the command can be executed on the command target.</summary>
	public event CanExecuteRoutedEventHandler CanExecute;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.CommandBinding" /> class.</summary>
	public CommandBinding()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.CommandBinding" /> class by using the specified <see cref="T:System.Windows.Input.ICommand" />.</summary>
	/// <param name="command">The command to base the new <see cref="T:System.Windows.Input.RoutedCommand" /> on.</param>
	public CommandBinding(ICommand command)
		: this(command, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.CommandBinding" /> class by using the specified <see cref="T:System.Windows.Input.ICommand" /> and the specified <see cref="E:System.Windows.Input.CommandBinding.Executed" /> event handler.</summary>
	/// <param name="command">The command to base the new <see cref="T:System.Windows.Input.RoutedCommand" /> on.</param>
	/// <param name="executed">The handler for the <see cref="E:System.Windows.Input.CommandBinding.Executed" /> event on the new <see cref="T:System.Windows.Input.RoutedCommand" />.</param>
	public CommandBinding(ICommand command, ExecutedRoutedEventHandler executed)
		: this(command, executed, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.CommandBinding" /> class by using the specified <see cref="T:System.Windows.Input.ICommand" /> and the specified <see cref="E:System.Windows.Input.CommandBinding.Executed" /> and <see cref="E:System.Windows.Input.CommandBinding.CanExecute" /> event handlers.</summary>
	/// <param name="command">The command to base the new <see cref="T:System.Windows.Input.RoutedCommand" /> on.</param>
	/// <param name="executed">The handler for the <see cref="E:System.Windows.Input.CommandBinding.Executed" /> event on the new <see cref="T:System.Windows.Input.RoutedCommand" />.</param>
	/// <param name="canExecute">The handler for the <see cref="E:System.Windows.Input.CommandBinding.CanExecute" /> event on the new <see cref="T:System.Windows.Input.RoutedCommand" />.</param>
	public CommandBinding(ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute)
	{
		_command = command ?? throw new ArgumentNullException("command");
		if (executed != null)
		{
			Executed += executed;
		}
		if (canExecute != null)
		{
			CanExecute += canExecute;
		}
	}

	internal void OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		if (e.Handled)
		{
			return;
		}
		if (e.RoutedEvent == CommandManager.CanExecuteEvent)
		{
			if (this.CanExecute == null)
			{
				if (!e.CanExecute && this.Executed != null)
				{
					e.CanExecute = true;
					e.Handled = true;
				}
			}
			else
			{
				this.CanExecute(sender, e);
				if (e.CanExecute)
				{
					e.Handled = true;
				}
			}
		}
		else if (this.PreviewCanExecute != null)
		{
			this.PreviewCanExecute(sender, e);
			if (e.CanExecute)
			{
				e.Handled = true;
			}
		}
	}

	private bool CheckCanExecute(object sender, ExecutedRoutedEventArgs e)
	{
		CanExecuteRoutedEventArgs canExecuteRoutedEventArgs = new CanExecuteRoutedEventArgs(e.Command, e.Parameter)
		{
			RoutedEvent = CommandManager.CanExecuteEvent,
			Source = e.OriginalSource
		};
		canExecuteRoutedEventArgs.OverrideSource(e.Source);
		OnCanExecute(sender, canExecuteRoutedEventArgs);
		return canExecuteRoutedEventArgs.CanExecute;
	}

	internal void OnExecuted(object sender, ExecutedRoutedEventArgs e)
	{
		if (e.Handled)
		{
			return;
		}
		if (e.RoutedEvent == CommandManager.ExecutedEvent)
		{
			if (this.Executed != null && CheckCanExecute(sender, e))
			{
				this.Executed(sender, e);
				e.Handled = true;
			}
		}
		else if (this.PreviewExecuted != null && CheckCanExecute(sender, e))
		{
			this.PreviewExecuted(sender, e);
			e.Handled = true;
		}
	}
}
