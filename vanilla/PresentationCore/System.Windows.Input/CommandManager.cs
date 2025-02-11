using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace System.Windows.Input;

/// <summary>Provides command related utility methods that register <see cref="T:System.Windows.Input.CommandBinding" /> and <see cref="T:System.Windows.Input.InputBinding" /> objects for class owners and commands, add and remove command event handlers, and provides services for querying the status of a command.</summary>
public sealed class CommandManager
{
	private class RequerySuggestedEventManager : WeakEventManager
	{
		private static RequerySuggestedEventManager CurrentManager
		{
			get
			{
				Type typeFromHandle = typeof(RequerySuggestedEventManager);
				RequerySuggestedEventManager requerySuggestedEventManager = (RequerySuggestedEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
				if (requerySuggestedEventManager == null)
				{
					requerySuggestedEventManager = new RequerySuggestedEventManager();
					WeakEventManager.SetCurrentManager(typeFromHandle, requerySuggestedEventManager);
				}
				return requerySuggestedEventManager;
			}
		}

		private RequerySuggestedEventManager()
		{
		}

		public static void AddHandler(CommandManager source, EventHandler handler)
		{
			if (handler != null)
			{
				CurrentManager.ProtectedAddHandler(source, handler);
			}
		}

		public static void RemoveHandler(CommandManager source, EventHandler handler)
		{
			if (handler != null)
			{
				CurrentManager.ProtectedRemoveHandler(source, handler);
			}
		}

		protected override ListenerList NewListenerList()
		{
			return new ListenerList();
		}

		protected override void StartListening(object source)
		{
			Current.PrivateRequerySuggested += OnRequerySuggested;
		}

		protected override void StopListening(object source)
		{
			Current.PrivateRequerySuggested -= OnRequerySuggested;
		}

		private void OnRequerySuggested(object sender, EventArgs args)
		{
			DeliverEvent(sender, args);
		}
	}

	/// <summary>Identifies the <see cref="E:System.Windows.Input.CommandManager.PreviewExecuted" /> attached event.</summary>
	public static readonly RoutedEvent PreviewExecutedEvent = EventManager.RegisterRoutedEvent("PreviewExecuted", RoutingStrategy.Tunnel, typeof(ExecutedRoutedEventHandler), typeof(CommandManager));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.CommandManager.Executed" /> attached event.</summary>
	public static readonly RoutedEvent ExecutedEvent = EventManager.RegisterRoutedEvent("Executed", RoutingStrategy.Bubble, typeof(ExecutedRoutedEventHandler), typeof(CommandManager));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.CommandManager.PreviewCanExecute" /> attached event.</summary>
	public static readonly RoutedEvent PreviewCanExecuteEvent = EventManager.RegisterRoutedEvent("PreviewCanExecute", RoutingStrategy.Tunnel, typeof(CanExecuteRoutedEventHandler), typeof(CommandManager));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.CommandManager.CanExecute" /> attached event.</summary>
	public static readonly RoutedEvent CanExecuteEvent = EventManager.RegisterRoutedEvent("CanExecute", RoutingStrategy.Bubble, typeof(CanExecuteRoutedEventHandler), typeof(CommandManager));

	[ThreadStatic]
	private static CommandManager _commandManager;

	private static HybridDictionary _classCommandBindings = new HybridDictionary();

	private static HybridDictionary _classInputBindings = new HybridDictionary();

	private DispatcherOperation _requerySuggestedOperation;

	private static CommandManager Current
	{
		get
		{
			if (_commandManager == null)
			{
				_commandManager = new CommandManager();
			}
			return _commandManager;
		}
	}

	/// <summary>Occurs when the <see cref="T:System.Windows.Input.CommandManager" /> detects conditions that might change the ability of a command to execute.</summary>
	public static event EventHandler RequerySuggested
	{
		add
		{
			RequerySuggestedEventManager.AddHandler(null, value);
		}
		remove
		{
			RequerySuggestedEventManager.RemoveHandler(null, value);
		}
	}

	private event EventHandler PrivateRequerySuggested;

	/// <summary>Attaches the specified <see cref="T:System.Windows.Input.ExecutedRoutedEventHandler" /> to the specified element.</summary>
	/// <param name="element">The element to attach <paramref name="handler" /> to.</param>
	/// <param name="handler">The can execute handler.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="handler" /> is null.</exception>
	public static void AddPreviewExecutedHandler(UIElement element, ExecutedRoutedEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		element.AddHandler(PreviewExecutedEvent, handler);
	}

	/// <summary>Detaches the specified <see cref="T:System.Windows.Input.ExecutedRoutedEventHandler" /> from the specified element.</summary>
	/// <param name="element">The element to remove <paramref name="handler" /> from.</param>
	/// <param name="handler">The executed handler.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="handler" /> is null.</exception>
	public static void RemovePreviewExecutedHandler(UIElement element, ExecutedRoutedEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		element.RemoveHandler(PreviewExecutedEvent, handler);
	}

	/// <summary>Attaches the specified <see cref="T:System.Windows.Input.ExecutedRoutedEventHandler" /> to the specified element. </summary>
	/// <param name="element">The element to attach <paramref name="handler" /> to.</param>
	/// <param name="handler">The executed handler.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="handler" /> is null.</exception>
	public static void AddExecutedHandler(UIElement element, ExecutedRoutedEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		element.AddHandler(ExecutedEvent, handler);
	}

	/// <summary>Detaches the specified <see cref="T:System.Windows.Input.ExecutedRoutedEventHandler" /> from the specified element.</summary>
	/// <param name="element">The element to remove <paramref name="handler" /> from.</param>
	/// <param name="handler">The executed handler.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="handler" /> is null.</exception>
	public static void RemoveExecutedHandler(UIElement element, ExecutedRoutedEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		element.RemoveHandler(ExecutedEvent, handler);
	}

	/// <summary>Attaches the specified <see cref="T:System.Windows.Input.CanExecuteRoutedEventHandler" /> to the specified element.</summary>
	/// <param name="element">The element to attach <paramref name="handler" /> to.</param>
	/// <param name="handler">The can execute handler.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="handler" /> is null.</exception>
	public static void AddPreviewCanExecuteHandler(UIElement element, CanExecuteRoutedEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		element.AddHandler(PreviewCanExecuteEvent, handler);
	}

	/// <summary>Detaches the specified <see cref="T:System.Windows.Input.CanExecuteRoutedEventHandler" /> from the specified element.</summary>
	/// <param name="element">The element to remove <paramref name="handler" /> from.</param>
	/// <param name="handler">The can execute handler.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="handler" /> is null.</exception>
	public static void RemovePreviewCanExecuteHandler(UIElement element, CanExecuteRoutedEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		element.RemoveHandler(PreviewCanExecuteEvent, handler);
	}

	/// <summary>Attaches the specified <see cref="T:System.Windows.Input.CanExecuteRoutedEventHandler" /> to the specified element.</summary>
	/// <param name="element">The element to attach <paramref name="handler" /> to.</param>
	/// <param name="handler">The can execute handler.  </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="handler" /> is null.</exception>
	public static void AddCanExecuteHandler(UIElement element, CanExecuteRoutedEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		element.AddHandler(CanExecuteEvent, handler);
	}

	/// <summary>Detaches the specified <see cref="T:System.Windows.Input.CanExecuteRoutedEventHandler" /> from the specified element.</summary>
	/// <param name="element">The element to remove <paramref name="handler" /> from.</param>
	/// <param name="handler">The can execute handler.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="handler" /> is null.</exception>
	public static void RemoveCanExecuteHandler(UIElement element, CanExecuteRoutedEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		element.RemoveHandler(CanExecuteEvent, handler);
	}

	/// <summary>Registers the specified <see cref="T:System.Windows.Input.InputBinding" /> with the specified type. </summary>
	/// <param name="type">The type to register <paramref name="inputBinding" /> with.</param>
	/// <param name="inputBinding">The input binding to register.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> or <paramref name="inputBinding" /> is null.</exception>
	public static void RegisterClassInputBinding(Type type, InputBinding inputBinding)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (inputBinding == null)
		{
			throw new ArgumentNullException("inputBinding");
		}
		lock (_classInputBindings.SyncRoot)
		{
			InputBindingCollection inputBindingCollection = _classInputBindings[type] as InputBindingCollection;
			if (inputBindingCollection == null)
			{
				inputBindingCollection = new InputBindingCollection();
				_classInputBindings[type] = inputBindingCollection;
			}
			inputBindingCollection.Add(inputBinding);
			if (!inputBinding.IsFrozen)
			{
				inputBinding.Freeze();
			}
		}
	}

	/// <summary>Registers a <see cref="T:System.Windows.Input.CommandBinding" /> with the specified type. </summary>
	/// <param name="type">The class with which to register <paramref name="commandBinding" />.</param>
	/// <param name="commandBinding">The command binding to register.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> or <paramref name="commandBinding" /> is null.</exception>
	public static void RegisterClassCommandBinding(Type type, CommandBinding commandBinding)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (commandBinding == null)
		{
			throw new ArgumentNullException("commandBinding");
		}
		lock (_classCommandBindings.SyncRoot)
		{
			CommandBindingCollection commandBindingCollection = _classCommandBindings[type] as CommandBindingCollection;
			if (commandBindingCollection == null)
			{
				commandBindingCollection = new CommandBindingCollection();
				_classCommandBindings[type] = commandBindingCollection;
			}
			commandBindingCollection.Add(commandBinding);
		}
	}

	/// <summary>Forces the <see cref="T:System.Windows.Input.CommandManager" /> to raise the <see cref="E:System.Windows.Input.CommandManager.RequerySuggested" /> event.</summary>
	public static void InvalidateRequerySuggested()
	{
		Current.RaiseRequerySuggested();
	}

	internal static void TranslateInput(IInputElement targetElement, InputEventArgs inputEventArgs)
	{
		if (targetElement == null || inputEventArgs == null)
		{
			return;
		}
		ICommand command = null;
		IInputElement inputElement = null;
		object parameter = null;
		DependencyObject dependencyObject = targetElement as DependencyObject;
		bool flag = dependencyObject is UIElement;
		bool flag2 = !flag && dependencyObject is ContentElement;
		bool flag3 = !flag && !flag2 && dependencyObject is UIElement3D;
		InputBindingCollection inputBindingCollection = null;
		if (flag)
		{
			inputBindingCollection = ((UIElement)targetElement).InputBindingsInternal;
		}
		else if (flag2)
		{
			inputBindingCollection = ((ContentElement)targetElement).InputBindingsInternal;
		}
		else if (flag3)
		{
			inputBindingCollection = ((UIElement3D)targetElement).InputBindingsInternal;
		}
		if (inputBindingCollection != null)
		{
			InputBinding inputBinding = inputBindingCollection.FindMatch(targetElement, inputEventArgs);
			if (inputBinding != null)
			{
				command = inputBinding.Command;
				inputElement = inputBinding.CommandTarget;
				parameter = inputBinding.CommandParameter;
			}
		}
		if (command == null)
		{
			lock (_classInputBindings.SyncRoot)
			{
				Type type = targetElement.GetType();
				while (type != null)
				{
					if (_classInputBindings[type] is InputBindingCollection inputBindingCollection2)
					{
						InputBinding inputBinding2 = inputBindingCollection2.FindMatch(targetElement, inputEventArgs);
						if (inputBinding2 != null)
						{
							command = inputBinding2.Command;
							inputElement = inputBinding2.CommandTarget;
							parameter = inputBinding2.CommandParameter;
							break;
						}
					}
					type = type.BaseType;
				}
			}
		}
		if (command == null)
		{
			CommandBindingCollection commandBindingCollection = null;
			if (flag)
			{
				commandBindingCollection = ((UIElement)targetElement).CommandBindingsInternal;
			}
			else if (flag2)
			{
				commandBindingCollection = ((ContentElement)targetElement).CommandBindingsInternal;
			}
			else if (flag3)
			{
				commandBindingCollection = ((UIElement3D)targetElement).CommandBindingsInternal;
			}
			if (commandBindingCollection != null)
			{
				command = commandBindingCollection.FindMatch(targetElement, inputEventArgs);
			}
		}
		if (command == null)
		{
			lock (_classCommandBindings.SyncRoot)
			{
				Type type2 = targetElement.GetType();
				while (type2 != null)
				{
					if (_classCommandBindings[type2] is CommandBindingCollection commandBindingCollection2)
					{
						command = commandBindingCollection2.FindMatch(targetElement, inputEventArgs);
						if (command != null)
						{
							break;
						}
					}
					type2 = type2.BaseType;
				}
			}
		}
		if (command == null || command == ApplicationCommands.NotACommand)
		{
			return;
		}
		if (inputElement == null)
		{
			inputElement = targetElement;
		}
		bool continueRouting = false;
		if (command is RoutedCommand routedCommand)
		{
			if (routedCommand.CriticalCanExecute(parameter, inputElement, inputEventArgs.UserInitiated, out continueRouting))
			{
				continueRouting = false;
				ExecuteCommand(routedCommand, parameter, inputElement, inputEventArgs);
			}
		}
		else if (command.CanExecute(parameter))
		{
			command.Execute(parameter);
		}
		inputEventArgs.Handled = !continueRouting;
	}

	private static bool ExecuteCommand(RoutedCommand routedCommand, object parameter, IInputElement target, InputEventArgs inputEventArgs)
	{
		return routedCommand.ExecuteCore(parameter, target, inputEventArgs.UserInitiated);
	}

	internal static void OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		if (sender == null || e == null || e.Command == null)
		{
			return;
		}
		FindCommandBinding(sender, e, e.Command, execute: false);
		if (!e.Handled && e.RoutedEvent == CanExecuteEvent && sender is DependencyObject dependencyObject && FocusManager.GetIsFocusScope(dependencyObject))
		{
			IInputElement parentScopeFocusedElement = GetParentScopeFocusedElement(dependencyObject);
			if (parentScopeFocusedElement != null)
			{
				TransferEvent(parentScopeFocusedElement, e);
			}
		}
	}

	private static bool CanExecuteCommandBinding(object sender, CanExecuteRoutedEventArgs e, CommandBinding commandBinding)
	{
		commandBinding.OnCanExecute(sender, e);
		if (!e.CanExecute)
		{
			return e.Handled;
		}
		return true;
	}

	internal static void OnExecuted(object sender, ExecutedRoutedEventArgs e)
	{
		if (sender == null || e == null || e.Command == null)
		{
			return;
		}
		FindCommandBinding(sender, e, e.Command, execute: true);
		if (!e.Handled && e.RoutedEvent == ExecutedEvent && sender is DependencyObject dependencyObject && FocusManager.GetIsFocusScope(dependencyObject))
		{
			IInputElement parentScopeFocusedElement = GetParentScopeFocusedElement(dependencyObject);
			if (parentScopeFocusedElement != null)
			{
				TransferEvent(parentScopeFocusedElement, e);
			}
		}
	}

	private static bool ExecuteCommandBinding(object sender, ExecutedRoutedEventArgs e, CommandBinding commandBinding)
	{
		commandBinding.OnExecuted(sender, e);
		return e.Handled;
	}

	internal static void OnCommandDevice(object sender, CommandDeviceEventArgs e)
	{
		if (sender == null || e == null || e.Command == null)
		{
			return;
		}
		CanExecuteRoutedEventArgs canExecuteRoutedEventArgs = new CanExecuteRoutedEventArgs(e.Command, null);
		canExecuteRoutedEventArgs.RoutedEvent = CanExecuteEvent;
		canExecuteRoutedEventArgs.Source = sender;
		OnCanExecute(sender, canExecuteRoutedEventArgs);
		if (canExecuteRoutedEventArgs.CanExecute)
		{
			ExecutedRoutedEventArgs executedRoutedEventArgs = new ExecutedRoutedEventArgs(e.Command, null);
			executedRoutedEventArgs.RoutedEvent = ExecutedEvent;
			executedRoutedEventArgs.Source = sender;
			OnExecuted(sender, executedRoutedEventArgs);
			if (executedRoutedEventArgs.Handled)
			{
				e.Handled = true;
			}
		}
	}

	private static void FindCommandBinding(object sender, RoutedEventArgs e, ICommand command, bool execute)
	{
		CommandBindingCollection commandBindingCollection = ((sender is UIElement uIElement) ? uIElement.CommandBindingsInternal : ((sender is ContentElement contentElement) ? contentElement.CommandBindingsInternal : ((!(sender is UIElement3D uIElement3D)) ? null : uIElement3D.CommandBindingsInternal)));
		CommandBindingCollection commandBindingCollection2 = commandBindingCollection;
		if (commandBindingCollection2 != null)
		{
			FindCommandBinding(commandBindingCollection2, sender, e, command, execute);
		}
		Type type = sender.GetType();
		(Type, CommandBinding)? tuple = null;
		List<(Type, CommandBinding)> list = null;
		lock (_classCommandBindings.SyncRoot)
		{
			Type type2 = type;
			while ((object)type2 != null)
			{
				if (_classCommandBindings[type2] is CommandBindingCollection commandBindingCollection3)
				{
					int index = 0;
					while (true)
					{
						CommandBinding commandBinding = commandBindingCollection3.FindMatch(command, ref index);
						if (commandBinding == null)
						{
							break;
						}
						if (!tuple.HasValue)
						{
							tuple = ValueTuple.Create(type2, commandBinding);
							continue;
						}
						if (list == null)
						{
							list = new List<(Type, CommandBinding)>(8) { tuple.Value };
						}
						list.Add((type2, commandBinding));
					}
				}
				type2 = type2.BaseType;
			}
		}
		if (list != null)
		{
			ExecutedRoutedEventArgs e2 = (execute ? ((ExecutedRoutedEventArgs)e) : null);
			CanExecuteRoutedEventArgs e3 = (execute ? null : ((CanExecuteRoutedEventArgs)e));
			for (int i = 0; i < list.Count; i++)
			{
				if ((execute && ExecuteCommandBinding(sender, e2, list[i].Item2)) || (!execute && CanExecuteCommandBinding(sender, e3, list[i].Item2)))
				{
					Type item = list[i].Item1;
					while (++i < list.Count && list[i].Item1 == item)
					{
					}
					i--;
				}
			}
		}
		else if (tuple.HasValue)
		{
			CommandBinding item2 = tuple.GetValueOrDefault().Item2;
			if (execute)
			{
				ExecuteCommandBinding(sender, (ExecutedRoutedEventArgs)e, item2);
			}
			else
			{
				CanExecuteCommandBinding(sender, (CanExecuteRoutedEventArgs)e, item2);
			}
		}
	}

	private static void FindCommandBinding(CommandBindingCollection commandBindings, object sender, RoutedEventArgs e, ICommand command, bool execute)
	{
		int index = 0;
		CommandBinding commandBinding;
		do
		{
			commandBinding = commandBindings.FindMatch(command, ref index);
		}
		while (!HandleCommandBinding(sender, e, commandBinding, execute));
	}

	private static bool HandleCommandBinding(object sender, RoutedEventArgs e, CommandBinding commandBinding, bool execute)
	{
		if (commandBinding != null && (!execute || !ExecuteCommandBinding(sender, (ExecutedRoutedEventArgs)e, commandBinding)))
		{
			if (!execute)
			{
				return CanExecuteCommandBinding(sender, (CanExecuteRoutedEventArgs)e, commandBinding);
			}
			return false;
		}
		return true;
	}

	private static void TransferEvent(IInputElement newSource, CanExecuteRoutedEventArgs e)
	{
		if (e.Command is RoutedCommand routedCommand)
		{
			try
			{
				e.CanExecute = routedCommand.CanExecute(e.Parameter, newSource);
			}
			finally
			{
				e.Handled = true;
			}
		}
	}

	private static void TransferEvent(IInputElement newSource, ExecutedRoutedEventArgs e)
	{
		if (e.Command is RoutedCommand routedCommand)
		{
			try
			{
				routedCommand.ExecuteCore(e.Parameter, newSource, e.UserInitiated);
			}
			finally
			{
				e.Handled = true;
			}
		}
	}

	private static IInputElement GetParentScopeFocusedElement(DependencyObject childScope)
	{
		DependencyObject parentScope = GetParentScope(childScope);
		if (parentScope != null)
		{
			IInputElement focusedElement = FocusManager.GetFocusedElement(parentScope);
			if (focusedElement != null && !ContainsElement(childScope, focusedElement as DependencyObject))
			{
				return focusedElement;
			}
		}
		return null;
	}

	private static DependencyObject GetParentScope(DependencyObject childScope)
	{
		DependencyObject dependencyObject = null;
		UIElement uIElement = childScope as UIElement;
		ContentElement contentElement = ((uIElement == null) ? (childScope as ContentElement) : null);
		UIElement3D uIElement3D = ((uIElement == null && contentElement == null) ? (childScope as UIElement3D) : null);
		if (uIElement != null)
		{
			dependencyObject = uIElement.GetUIParent(continuePastVisualTree: true);
		}
		else if (contentElement != null)
		{
			dependencyObject = contentElement.GetUIParent(continuePastVisualTree: true);
		}
		else if (uIElement3D != null)
		{
			dependencyObject = uIElement3D.GetUIParent(continuePastVisualTree: true);
		}
		if (dependencyObject != null)
		{
			return FocusManager.GetFocusScope(dependencyObject);
		}
		return null;
	}

	private static bool ContainsElement(DependencyObject scope, DependencyObject child)
	{
		if (child != null)
		{
			for (DependencyObject dependencyObject = FocusManager.GetFocusScope(child); dependencyObject != null; dependencyObject = GetParentScope(dependencyObject))
			{
				if (dependencyObject == scope)
				{
					return true;
				}
			}
		}
		return false;
	}

	private CommandManager()
	{
	}

	private void RaiseRequerySuggested()
	{
		if (_requerySuggestedOperation == null)
		{
			Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;
			if (currentDispatcher != null && !currentDispatcher.HasShutdownStarted && !currentDispatcher.HasShutdownFinished)
			{
				_requerySuggestedOperation = currentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(RaiseRequerySuggested), null);
			}
		}
	}

	private object RaiseRequerySuggested(object obj)
	{
		_requerySuggestedOperation = null;
		if (this.PrivateRequerySuggested != null)
		{
			this.PrivateRequerySuggested(null, EventArgs.Empty);
		}
		return null;
	}
}
