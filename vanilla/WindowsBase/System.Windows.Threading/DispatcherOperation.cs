using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MS.Internal;
using MS.Internal.WindowsBase;

namespace System.Windows.Threading;

/// <summary>Represents an object that is used to interact with an operation that has been posted to the <see cref="T:System.Windows.Threading.Dispatcher" /> queue.</summary>
public class DispatcherOperation
{
	private class DispatcherOperationFrame : DispatcherFrame
	{
		private DispatcherOperation _operation;

		private Timer _waitTimer;

		public DispatcherOperationFrame(DispatcherOperation op, TimeSpan timeout)
			: base(exitWhenRequested: false)
		{
			_operation = op;
			_operation.Aborted += OnCompletedOrAborted;
			_operation.Completed += OnCompletedOrAborted;
			if (timeout.TotalMilliseconds > 0.0)
			{
				_waitTimer = new Timer(OnTimeout, null, timeout, TimeSpan.FromMilliseconds(-1.0));
			}
			if (_operation._status != 0)
			{
				Exit();
			}
		}

		private void OnCompletedOrAborted(object sender, EventArgs e)
		{
			Exit();
		}

		private void OnTimeout(object arg)
		{
			Exit();
		}

		private void Exit()
		{
			base.Continue = false;
			if (_waitTimer != null)
			{
				_waitTimer.Dispose();
			}
			_operation.Aborted -= OnCompletedOrAborted;
			_operation.Completed -= OnCompletedOrAborted;
		}
	}

	private class DispatcherOperationEvent
	{
		private DispatcherOperation _operation;

		private TimeSpan _timeout;

		private ManualResetEvent _event;

		private bool _eventClosed;

		private object DispatcherLock => _operation.DispatcherLock;

		public DispatcherOperationEvent(DispatcherOperation op, TimeSpan timeout)
		{
			_operation = op;
			_timeout = timeout;
			_event = new ManualResetEvent(initialState: false);
			_eventClosed = false;
			lock (DispatcherLock)
			{
				_operation.Aborted += OnCompletedOrAborted;
				_operation.Completed += OnCompletedOrAborted;
				if (_operation._status != 0 && _operation._status != DispatcherOperationStatus.Executing)
				{
					_event.Set();
				}
			}
		}

		private void OnCompletedOrAborted(object sender, EventArgs e)
		{
			lock (DispatcherLock)
			{
				if (!_eventClosed)
				{
					_event.Set();
				}
			}
		}

		public void WaitOne()
		{
			_event.WaitOne(_timeout, exitContext: false);
			lock (DispatcherLock)
			{
				if (!_eventClosed)
				{
					_operation.Aborted -= OnCompletedOrAborted;
					_operation.Completed -= OnCompletedOrAborted;
					_event.Close();
					_eventClosed = true;
				}
			}
		}
	}

	private CulturePreservingExecutionContext _executionContext;

	private static readonly ContextCallback _invokeInSecurityContext;

	private readonly Dispatcher _dispatcher;

	private DispatcherPriority _priority;

	internal readonly Delegate _method;

	private readonly object _args;

	private readonly int _numArgs;

	internal DispatcherOperationStatus _status;

	private object _result;

	private Exception _exception;

	internal PriorityItem<DispatcherOperation> _item;

	private EventHandler _aborted;

	private EventHandler _completed;

	internal readonly DispatcherOperationTaskSource _taskSource;

	private readonly bool _useAsyncSemantics;

	/// <summary>Gets the <see cref="T:System.Windows.Threading.Dispatcher" /> that the operation was posted to. </summary>
	/// <returns>The dispatcher.</returns>
	public Dispatcher Dispatcher => _dispatcher;

	/// <summary>Gets or sets the priority of the operation in the <see cref="T:System.Windows.Threading.Dispatcher" /> queue. </summary>
	/// <returns>The priority of the delegate on the queue.</returns>
	public DispatcherPriority Priority
	{
		get
		{
			return _priority;
		}
		set
		{
			Dispatcher.ValidatePriority(value, "value");
			if (value != _priority && _dispatcher.SetPriority(this, value))
			{
				_priority = value;
			}
		}
	}

	/// <summary>Gets the current status of the operation..</summary>
	/// <returns>The status of the operation.</returns>
	public DispatcherOperationStatus Status => _status;

	/// <summary>Gets a <see cref="T:System.Threading.Task" /> that represents the current operation.</summary>
	/// <returns>An object that represents the current operation.</returns>
	public Task Task => _taskSource.GetTask();

	internal string Name => _method.Method.DeclaringType?.ToString() + "." + _method.Method.Name;

	internal unsafe long Id
	{
		get
		{
			long result;
			fixed (DispatcherPriority* priority = &_priority)
			{
				result = (long)priority;
			}
			return result;
		}
	}

	/// <summary>Gets the result of the operation after it has completed. </summary>
	/// <returns>The result of the operation -or- null if the operation has not completed. </returns>
	public object Result
	{
		get
		{
			if (_useAsyncSemantics)
			{
				Wait();
				if (_status == DispatcherOperationStatus.Completed || _status == DispatcherOperationStatus.Aborted)
				{
					Task.GetAwaiter().GetResult();
				}
			}
			return _result;
		}
	}

	private object DispatcherLock => _dispatcher._instanceLock;

	/// <summary>Occurs when the operation is aborted.</summary>
	public event EventHandler Aborted
	{
		add
		{
			lock (DispatcherLock)
			{
				_aborted = (EventHandler)Delegate.Combine(_aborted, value);
			}
		}
		remove
		{
			lock (DispatcherLock)
			{
				_aborted = (EventHandler)Delegate.Remove(_aborted, value);
			}
		}
	}

	/// <summary>Occurs when the operation has completed. </summary>
	public event EventHandler Completed
	{
		add
		{
			lock (DispatcherLock)
			{
				_completed = (EventHandler)Delegate.Combine(_completed, value);
			}
		}
		remove
		{
			lock (DispatcherLock)
			{
				_completed = (EventHandler)Delegate.Remove(_completed, value);
			}
		}
	}

	static DispatcherOperation()
	{
		_invokeInSecurityContext = InvokeInSecurityContext;
	}

	internal DispatcherOperation(Dispatcher dispatcher, Delegate method, DispatcherPriority priority, object args, int numArgs, DispatcherOperationTaskSource taskSource, bool useAsyncSemantics)
	{
		_dispatcher = dispatcher;
		_method = method;
		_priority = priority;
		_numArgs = numArgs;
		_args = args;
		_executionContext = CulturePreservingExecutionContext.Capture();
		_taskSource = taskSource;
		_taskSource.Initialize(this);
		_useAsyncSemantics = useAsyncSemantics;
	}

	internal DispatcherOperation(Dispatcher dispatcher, Delegate method, DispatcherPriority priority, object args, int numArgs)
		: this(dispatcher, method, priority, args, numArgs, new DispatcherOperationTaskSource<object>(), useAsyncSemantics: false)
	{
	}

	internal DispatcherOperation(Dispatcher dispatcher, DispatcherPriority priority, Action action)
		: this(dispatcher, action, priority, null, 0, new DispatcherOperationTaskSource<object>(), useAsyncSemantics: true)
	{
	}

	internal DispatcherOperation(Dispatcher dispatcher, DispatcherPriority priority, Delegate method, object[] args)
		: this(dispatcher, method, priority, args, -1, new DispatcherOperationTaskSource<object>(), useAsyncSemantics: true)
	{
	}

	/// <summary>Returns an object that is notified when the asynchronous operation is finished.</summary>
	/// <returns>An object that is notified when the asynchronous operation is finished.</returns>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public TaskAwaiter GetAwaiter()
	{
		return Task.GetAwaiter();
	}

	/// <summary>Waits for the operation to complete</summary>
	/// <returns>The status of the operation. </returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Threading.DispatcherOperation.Status" /> is equal to <see cref="F:System.Windows.Threading.DispatcherOperationStatus.Executing" />.  This can occur when waiting for an operation that is already executing on the same thread.</exception>
	public DispatcherOperationStatus Wait()
	{
		return Wait(TimeSpan.FromMilliseconds(-1.0));
	}

	/// <summary>Waits for the operation to complete in the specified period of time.</summary>
	/// <returns>The status of the operation. </returns>
	/// <param name="timeout">The maximum period of time to wait.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Threading.DispatcherOperation.Status" /> is equal to <see cref="F:System.Windows.Threading.DispatcherOperationStatus.Executing" />.  This can occur when waiting for an operation that is already executing on the same thread.</exception>
	public DispatcherOperationStatus Wait(TimeSpan timeout)
	{
		if ((_status == DispatcherOperationStatus.Pending || _status == DispatcherOperationStatus.Executing) && timeout.TotalMilliseconds != 0.0)
		{
			if (_dispatcher.Thread == Thread.CurrentThread)
			{
				if (_status == DispatcherOperationStatus.Executing)
				{
					throw new InvalidOperationException(SR.ThreadMayNotWaitOnOperationsAlreadyExecutingOnTheSameThread);
				}
				Dispatcher.PushFrame(new DispatcherOperationFrame(this, timeout));
			}
			else
			{
				new DispatcherOperationEvent(this, timeout).WaitOne();
			}
		}
		if (_useAsyncSemantics && (_status == DispatcherOperationStatus.Completed || _status == DispatcherOperationStatus.Aborted))
		{
			Task.GetAwaiter().GetResult();
		}
		return _status;
	}

	/// <summary>Aborts the operation. </summary>
	/// <returns>true if the operation was aborted; otherwise, false.</returns>
	public bool Abort()
	{
		bool flag = false;
		if (_dispatcher != null)
		{
			flag = _dispatcher.Abort(this);
			if (flag)
			{
				_taskSource.SetCanceled();
				_aborted?.Invoke(this, EventArgs.Empty);
			}
		}
		return flag;
	}

	internal void Invoke()
	{
		_status = DispatcherOperationStatus.Executing;
		if (_executionContext != null)
		{
			CulturePreservingExecutionContext.Run(_executionContext, _invokeInSecurityContext, this);
			_executionContext.Dispose();
			_executionContext = null;
		}
		else
		{
			_invokeInSecurityContext(this);
		}
		EventHandler eventHandler;
		lock (DispatcherLock)
		{
			if (_exception is OperationCanceledException)
			{
				eventHandler = _aborted;
				_status = DispatcherOperationStatus.Aborted;
			}
			else
			{
				eventHandler = _completed;
				_status = DispatcherOperationStatus.Completed;
			}
		}
		eventHandler?.Invoke(this, EventArgs.Empty);
	}

	internal void InvokeCompletions()
	{
		switch (_status)
		{
		case DispatcherOperationStatus.Aborted:
			_taskSource.SetCanceled();
			break;
		case DispatcherOperationStatus.Completed:
			if (_exception != null)
			{
				_taskSource.SetException(_exception);
			}
			else
			{
				_taskSource.SetResult(_result);
			}
			break;
		default:
			Invariant.Assert(condition: false, "Operation should be either Aborted or Completed!");
			break;
		}
	}

	private static void InvokeInSecurityContext(object state)
	{
		((DispatcherOperation)state).InvokeImpl();
	}

	private void InvokeImpl()
	{
		SynchronizationContext current = SynchronizationContext.Current;
		try
		{
			DispatcherSynchronizationContext synchronizationContext = (BaseCompatibilityPreferences.GetReuseDispatcherSynchronizationContextInstance() ? Dispatcher._defaultDispatcherSynchronizationContext : ((!BaseCompatibilityPreferences.GetFlowDispatcherSynchronizationContextPriority()) ? new DispatcherSynchronizationContext(_dispatcher, DispatcherPriority.Normal) : new DispatcherSynchronizationContext(_dispatcher, _priority)));
			SynchronizationContext.SetSynchronizationContext(synchronizationContext);
			_dispatcher.PromoteTimers(Environment.TickCount);
			if (_useAsyncSemantics)
			{
				try
				{
					_result = InvokeDelegateCore();
					return;
				}
				catch (Exception exception)
				{
					_exception = exception;
					return;
				}
			}
			_result = _dispatcher.WrappedInvoke(_method, _args, _numArgs, null);
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(current);
		}
	}

	/// <summary>Begins the operation that is associated with this <see cref="T:System.Windows.Threading.DispatcherOperation" />.</summary>
	/// <returns>null in all cases.</returns>
	protected virtual object InvokeDelegateCore()
	{
		((Action)_method)();
		return null;
	}
}
/// <summary>Represents an object that is used to interact with an operation that has been posted to the <see cref="T:System.Windows.Threading.Dispatcher" /> queue and contains a <see cref="T:System.Threading.Tasks.Task`1" />. </summary>
/// <typeparam name="TResult">The type that is returned by the <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
public class DispatcherOperation<TResult> : DispatcherOperation
{
	/// <summary>Gets a <see cref="T:System.Threading.Task`1" /> that represents the current operation.</summary>
	/// <returns>An object that represents the current operation.</returns>
	public new Task<TResult> Task => (Task<TResult>)base.Task;

	/// <summary>Gets the result of the operation after it has completed.</summary>
	/// <returns>The result of the operation.</returns>
	public new TResult Result => (TResult)base.Result;

	internal DispatcherOperation(Dispatcher dispatcher, DispatcherPriority priority, Func<TResult> func)
		: base(dispatcher, func, priority, null, 0, new DispatcherOperationTaskSource<TResult>(), useAsyncSemantics: true)
	{
	}

	/// <summary>Returns an object that awaits an asynchronous operation that returns a result.</summary>
	/// <returns>An object that awaits an asynchronous operation that returns a result.</returns>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new TaskAwaiter<TResult> GetAwaiter()
	{
		return Task.GetAwaiter();
	}

	/// <summary>Begins the operation that is associated with this <see cref="T:System.Windows.Threading.DispatcherOperation" />.</summary>
	/// <returns>The result of the operation.</returns>
	protected override object InvokeDelegateCore()
	{
		return ((Func<TResult>)_method)();
	}
}
