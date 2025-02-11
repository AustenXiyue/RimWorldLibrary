namespace System.Windows.Threading;

/// <summary>Provides additional event information about <see cref="T:System.Windows.Threading.Dispatcher" /> processing.</summary>
public sealed class DispatcherHooks
{
	private readonly object _instanceLock = new object();

	private EventHandler _dispatcherInactive;

	private DispatcherHookEventHandler _operationPosted;

	private DispatcherHookEventHandler _operationStarted;

	private DispatcherHookEventHandler _operationCompleted;

	private DispatcherHookEventHandler _operationPriorityChanged;

	private DispatcherHookEventHandler _operationAborted;

	/// <summary>Occurs when the dispatcher has no more operations to process. </summary>
	public event EventHandler DispatcherInactive
	{
		add
		{
			lock (_instanceLock)
			{
				_dispatcherInactive = (EventHandler)Delegate.Combine(_dispatcherInactive, value);
			}
		}
		remove
		{
			lock (_instanceLock)
			{
				_dispatcherInactive = (EventHandler)Delegate.Remove(_dispatcherInactive, value);
			}
		}
	}

	/// <summary>Occurs when an operation is posted to the dispatcher. </summary>
	public event DispatcherHookEventHandler OperationPosted
	{
		add
		{
			lock (_instanceLock)
			{
				_operationPosted = (DispatcherHookEventHandler)Delegate.Combine(_operationPosted, value);
			}
		}
		remove
		{
			lock (_instanceLock)
			{
				_operationPosted = (DispatcherHookEventHandler)Delegate.Remove(_operationPosted, value);
			}
		}
	}

	/// <summary>Occurs when an operation is invoked.</summary>
	public event DispatcherHookEventHandler OperationStarted
	{
		add
		{
			lock (_instanceLock)
			{
				_operationStarted = (DispatcherHookEventHandler)Delegate.Combine(_operationStarted, value);
			}
		}
		remove
		{
			lock (_instanceLock)
			{
				_operationStarted = (DispatcherHookEventHandler)Delegate.Remove(_operationStarted, value);
			}
		}
	}

	/// <summary>Occurs when an operation completes. </summary>
	public event DispatcherHookEventHandler OperationCompleted
	{
		add
		{
			lock (_instanceLock)
			{
				_operationCompleted = (DispatcherHookEventHandler)Delegate.Combine(_operationCompleted, value);
			}
		}
		remove
		{
			lock (_instanceLock)
			{
				_operationCompleted = (DispatcherHookEventHandler)Delegate.Remove(_operationCompleted, value);
			}
		}
	}

	/// <summary>Occurs when the priority of an operation is changed. </summary>
	public event DispatcherHookEventHandler OperationPriorityChanged
	{
		add
		{
			lock (_instanceLock)
			{
				_operationPriorityChanged = (DispatcherHookEventHandler)Delegate.Combine(_operationPriorityChanged, value);
			}
		}
		remove
		{
			lock (_instanceLock)
			{
				_operationPriorityChanged = (DispatcherHookEventHandler)Delegate.Remove(_operationPriorityChanged, value);
			}
		}
	}

	/// <summary>Occurs when an operation is aborted. </summary>
	public event DispatcherHookEventHandler OperationAborted
	{
		add
		{
			lock (_instanceLock)
			{
				_operationAborted = (DispatcherHookEventHandler)Delegate.Combine(_operationAborted, value);
			}
		}
		remove
		{
			lock (_instanceLock)
			{
				_operationAborted = (DispatcherHookEventHandler)Delegate.Remove(_operationAborted, value);
			}
		}
	}

	internal DispatcherHooks()
	{
	}

	internal void RaiseDispatcherInactive(Dispatcher dispatcher)
	{
		_dispatcherInactive?.Invoke(dispatcher, EventArgs.Empty);
	}

	internal void RaiseOperationPosted(Dispatcher dispatcher, DispatcherOperation operation)
	{
		_operationPosted?.Invoke(dispatcher, new DispatcherHookEventArgs(operation));
	}

	internal void RaiseOperationStarted(Dispatcher dispatcher, DispatcherOperation operation)
	{
		_operationStarted?.Invoke(dispatcher, new DispatcherHookEventArgs(operation));
	}

	internal void RaiseOperationCompleted(Dispatcher dispatcher, DispatcherOperation operation)
	{
		_operationCompleted?.Invoke(dispatcher, new DispatcherHookEventArgs(operation));
	}

	internal void RaiseOperationPriorityChanged(Dispatcher dispatcher, DispatcherOperation operation)
	{
		_operationPriorityChanged?.Invoke(dispatcher, new DispatcherHookEventArgs(operation));
	}

	internal void RaiseOperationAborted(Dispatcher dispatcher, DispatcherOperation operation)
	{
		_operationAborted?.Invoke(dispatcher, new DispatcherHookEventArgs(operation));
	}
}
