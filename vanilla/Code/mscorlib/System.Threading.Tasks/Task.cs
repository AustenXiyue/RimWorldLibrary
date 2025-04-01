using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Security.Permissions;

namespace System.Threading.Tasks;

/// <summary>Represents an asynchronous operation that can return a value.</summary>
/// <typeparam name="TResult">The type of the result produced by this <see cref="T:System.Threading.Tasks.Task`1" />. </typeparam>
[DebuggerDisplay("Id = {Id}, Status = {Status}, Method = {DebuggerDisplayMethodDescription}, Result = {DebuggerDisplayResultDescription}")]
[DebuggerTypeProxy(typeof(SystemThreadingTasks_FutureDebugView<>))]
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public class Task<TResult> : Task
{
	internal TResult m_result;

	private static readonly TaskFactory<TResult> s_Factory = new TaskFactory<TResult>();

	internal static readonly Func<Task<Task>, Task<TResult>> TaskWhenAnyCast = (Task<Task> completed) => (Task<TResult>)completed.Result;

	private string DebuggerDisplayResultDescription
	{
		get
		{
			if (!base.IsRanToCompletion)
			{
				return Environment.GetResourceString("{Not yet computed}");
			}
			return string.Concat(m_result);
		}
	}

	private string DebuggerDisplayMethodDescription
	{
		get
		{
			Delegate @delegate = (Delegate)m_action;
			if ((object)@delegate == null)
			{
				return "{null}";
			}
			return @delegate.Method.ToString();
		}
	}

	/// <summary>Gets the result value of this <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
	/// <returns>The result value of this <see cref="T:System.Threading.Tasks.Task`1" />, which is the same type as the task's type parameter.</returns>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public TResult Result
	{
		get
		{
			if (!base.IsWaitNotificationEnabledOrNotRanToCompletion)
			{
				return m_result;
			}
			return GetResultCore(waitCompletionNotification: true);
		}
	}

	internal TResult ResultOnSuccess => m_result;

	/// <summary>Provides access to factory methods for creating <see cref="T:System.Threading.Tasks.Task`1" /> instances.</summary>
	/// <returns>A default instance of <see cref="T:System.Threading.Tasks.TaskFactory`1" />.</returns>
	public new static TaskFactory<TResult> Factory => s_Factory;

	internal Task()
	{
	}

	internal Task(object state, TaskCreationOptions options)
		: base(state, options, promiseStyle: true)
	{
	}

	internal Task(TResult result)
		: base(canceled: false, TaskCreationOptions.None, default(CancellationToken))
	{
		m_result = result;
	}

	internal Task(bool canceled, TResult result, TaskCreationOptions creationOptions, CancellationToken ct)
		: base(canceled, creationOptions, ct)
	{
		if (!canceled)
		{
			m_result = result;
		}
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified function.</summary>
	/// <param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Func<TResult> function)
		: this(function, (Task)null, default(CancellationToken), TaskCreationOptions.None, InternalTaskOptions.None, (TaskScheduler)null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified function.</summary>
	/// <param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to be assigned to this task.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Func<TResult> function, CancellationToken cancellationToken)
		: this(function, (Task)null, cancellationToken, TaskCreationOptions.None, InternalTaskOptions.None, (TaskScheduler)null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified function and creation options.</summary>
	/// <param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
	/// <param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Func<TResult> function, TaskCreationOptions creationOptions)
		: this(function, Task.InternalCurrentIfAttached(creationOptions), default(CancellationToken), creationOptions, InternalTaskOptions.None, (TaskScheduler)null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified function and creation options.</summary>
	/// <param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new task.</param>
	/// <param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
		: this(function, Task.InternalCurrentIfAttached(creationOptions), cancellationToken, creationOptions, InternalTaskOptions.None, (TaskScheduler)null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified function and state.</summary>
	/// <param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
	/// <param name="state">An object representing data to be used by the action.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Func<object, TResult> function, object state)
		: this((Delegate)function, state, (Task)null, default(CancellationToken), TaskCreationOptions.None, InternalTaskOptions.None, (TaskScheduler)null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified action, state, and options.</summary>
	/// <param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
	/// <param name="state">An object representing data to be used by the function.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to be assigned to the new task.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Func<object, TResult> function, object state, CancellationToken cancellationToken)
		: this((Delegate)function, state, (Task)null, cancellationToken, TaskCreationOptions.None, InternalTaskOptions.None, (TaskScheduler)null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified action, state, and options.</summary>
	/// <param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
	/// <param name="state">An object representing data to be used by the function.</param>
	/// <param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
		: this((Delegate)function, state, Task.InternalCurrentIfAttached(creationOptions), default(CancellationToken), creationOptions, InternalTaskOptions.None, (TaskScheduler)null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified action, state, and options.</summary>
	/// <param name="function">The delegate that represents the code to execute in the task. When the function has completed, the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return the result value of the function.</param>
	/// <param name="state">An object representing data to be used by the function.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to be assigned to the new task.</param>
	/// <param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Func<object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
		: this((Delegate)function, state, Task.InternalCurrentIfAttached(creationOptions), cancellationToken, creationOptions, InternalTaskOptions.None, (TaskScheduler)null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	internal Task(Func<TResult> valueSelector, Task parent, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler, ref StackCrawlMark stackMark)
		: this(valueSelector, parent, cancellationToken, creationOptions, internalOptions, scheduler)
	{
		PossiblyCaptureContext(ref stackMark);
	}

	internal Task(Func<TResult> valueSelector, Task parent, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler)
		: base(valueSelector, null, parent, cancellationToken, creationOptions, internalOptions, scheduler)
	{
		if ((internalOptions & InternalTaskOptions.SelfReplicating) != 0)
		{
			throw new ArgumentOutOfRangeException("creationOptions", Environment.GetResourceString("It is invalid to specify TaskCreationOptions.SelfReplicating for a Task<TResult>."));
		}
	}

	internal Task(Func<object, TResult> valueSelector, object state, Task parent, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler, ref StackCrawlMark stackMark)
		: this((Delegate)valueSelector, state, parent, cancellationToken, creationOptions, internalOptions, scheduler)
	{
		PossiblyCaptureContext(ref stackMark);
	}

	internal Task(Delegate valueSelector, object state, Task parent, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler)
		: base(valueSelector, state, parent, cancellationToken, creationOptions, internalOptions, scheduler)
	{
		if ((internalOptions & InternalTaskOptions.SelfReplicating) != 0)
		{
			throw new ArgumentOutOfRangeException("creationOptions", Environment.GetResourceString("It is invalid to specify TaskCreationOptions.SelfReplicating for a Task<TResult>."));
		}
	}

	internal static Task<TResult> StartNew(Task parent, Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler, ref StackCrawlMark stackMark)
	{
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if ((internalOptions & InternalTaskOptions.SelfReplicating) != 0)
		{
			throw new ArgumentOutOfRangeException("creationOptions", Environment.GetResourceString("It is invalid to specify TaskCreationOptions.SelfReplicating for a Task<TResult>."));
		}
		Task<TResult> task = new Task<TResult>(function, parent, cancellationToken, creationOptions, internalOptions | InternalTaskOptions.QueuedByRuntime, scheduler, ref stackMark);
		task.ScheduleAndStart(needsProtection: false);
		return task;
	}

	internal static Task<TResult> StartNew(Task parent, Func<object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler, ref StackCrawlMark stackMark)
	{
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if ((internalOptions & InternalTaskOptions.SelfReplicating) != 0)
		{
			throw new ArgumentOutOfRangeException("creationOptions", Environment.GetResourceString("It is invalid to specify TaskCreationOptions.SelfReplicating for a Task<TResult>."));
		}
		Task<TResult> task = new Task<TResult>(function, state, parent, cancellationToken, creationOptions, internalOptions | InternalTaskOptions.QueuedByRuntime, scheduler, ref stackMark);
		task.ScheduleAndStart(needsProtection: false);
		return task;
	}

	internal bool TrySetResult(TResult result)
	{
		if (base.IsCompleted)
		{
			return false;
		}
		if (AtomicStateUpdate(67108864, 90177536))
		{
			m_result = result;
			Interlocked.Exchange(ref m_stateFlags, m_stateFlags | 0x1000000);
			m_contingentProperties?.SetCompleted();
			FinishStageThree();
			return true;
		}
		return false;
	}

	internal void DangerousSetResult(TResult result)
	{
		if (m_parent != null)
		{
			TrySetResult(result);
			return;
		}
		m_result = result;
		m_stateFlags |= 16777216;
	}

	internal TResult GetResultCore(bool waitCompletionNotification)
	{
		if (!base.IsCompleted)
		{
			InternalWait(-1, default(CancellationToken));
		}
		if (waitCompletionNotification)
		{
			NotifyDebuggerOfWaitCompletionIfNecessary();
		}
		if (!base.IsRanToCompletion)
		{
			ThrowIfExceptional(includeTaskCanceledExceptions: true);
		}
		return m_result;
	}

	internal bool TrySetException(object exceptionObject)
	{
		bool result = false;
		EnsureContingentPropertiesInitialized(needsProtection: true);
		if (AtomicStateUpdate(67108864, 90177536))
		{
			AddException(exceptionObject);
			Finish(bUserDelegateExecuted: false);
			result = true;
		}
		return result;
	}

	internal bool TrySetCanceled(CancellationToken tokenToRecord)
	{
		return TrySetCanceled(tokenToRecord, null);
	}

	internal bool TrySetCanceled(CancellationToken tokenToRecord, object cancellationException)
	{
		bool result = false;
		if (AtomicStateUpdate(67108864, 90177536))
		{
			RecordInternalCancellationRequest(tokenToRecord, cancellationException);
			CancellationCleanupLogic();
			result = true;
		}
		return result;
	}

	internal override void InnerInvoke()
	{
		if (m_action is Func<TResult> func)
		{
			m_result = func();
		}
		else if (m_action is Func<object, TResult> func2)
		{
			m_result = func2(m_stateObject);
		}
	}

	/// <summary>Gets an awaiter used to await this <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
	/// <returns>An awaiter instance.</returns>
	public new TaskAwaiter<TResult> GetAwaiter()
	{
		return new TaskAwaiter<TResult>(this);
	}

	/// <summary>Configures an awaiter used to await this <see cref="T:System.Threading.Tasks.Task`1" />.</summary>
	/// <returns>An object used to await this task.</returns>
	/// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false.</param>
	public new ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
	{
		return new ConfiguredTaskAwaitable<TResult>(this, continueOnCapturedContext);
	}

	/// <summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task<TResult>> continuationAction)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, TaskScheduler.Current, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task<TResult>> continuationAction, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.-or-The <paramref name="scheduler" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task<TResult>> continuationAction, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, scheduler, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes according the condition specified in <paramref name="continuationOptions" />.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to according the condition specified in <paramref name="continuationOptions" />. When run, the delegate will be passed the completed task as an argument.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task<TResult>> continuationAction, TaskContinuationOptions continuationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, TaskScheduler.Current, default(CancellationToken), continuationOptions, ref stackMark);
	}

	/// <summary>Creates a continuation that executes according the condition specified in <paramref name="continuationOptions" />.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run according the condition specified in <paramref name="continuationOptions" />. When run, the delegate will be passed the completed task as an argument.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.-or-The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task<TResult>> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, scheduler, cancellationToken, continuationOptions, ref stackMark);
	}

	internal Task ContinueWith(Action<Task<TResult>> continuationAction, TaskScheduler scheduler, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, ref StackCrawlMark stackMark)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		Task.CreationOptionsFromContinuationOptions(continuationOptions, out var creationOptions, out var internalOptions);
		Task task = new ContinuationTaskFromResultTask<TResult>(this, continuationAction, null, creationOptions, internalOptions, ref stackMark);
		ContinueWithCore(task, scheduler, cancellationToken, continuationOptions);
		return task;
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation action.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task<TResult>, object> continuationAction, object state)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, state, TaskScheduler.Current, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation action.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task<TResult>, object> continuationAction, object state, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, state, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation action.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task<TResult>, object> continuationAction, object state, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, state, scheduler, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation action.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such  as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task<TResult>, object> continuationAction, object state, TaskContinuationOptions continuationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, state, TaskScheduler.Current, default(CancellationToken), continuationOptions, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation action.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as  well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its  execution.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task<TResult>, object> continuationAction, object state, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, state, scheduler, cancellationToken, continuationOptions, ref stackMark);
	}

	internal Task ContinueWith(Action<Task<TResult>, object> continuationAction, object state, TaskScheduler scheduler, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, ref StackCrawlMark stackMark)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		Task.CreationOptionsFromContinuationOptions(continuationOptions, out var creationOptions, out var internalOptions);
		Task task = new ContinuationTaskFromResultTask<TResult>(this, continuationAction, state, creationOptions, internalOptions, ref stackMark);
		ContinueWithCore(task, scheduler, cancellationToken, continuationOptions);
		return task;
	}

	/// <summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
	/// <typeparam name="TNewResult"> The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, TaskScheduler.Current, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new task.</param>
	/// <typeparam name="TNewResult"> The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
	/// <typeparam name="TNewResult"> The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.-or-The <paramref name="scheduler" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, scheduler, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes according the condition specified in <paramref name="continuationOptions" />.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run according the condition specified in <paramref name="continuationOptions" />.When run, the delegate will be passed the completed task as an argument.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <typeparam name="TNewResult"> The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction, TaskContinuationOptions continuationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, TaskScheduler.Current, default(CancellationToken), continuationOptions, ref stackMark);
	}

	/// <summary>Creates a continuation that executes according the condition specified in <paramref name="continuationOptions" />.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run according the condition specified in <paramref name="continuationOptions" />.When run, the delegate will be passed as an argument this completed task.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new task.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
	/// <typeparam name="TNewResult"> The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task`1" /> has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.-or-The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, scheduler, cancellationToken, continuationOptions, ref stackMark);
	}

	internal Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction, TaskScheduler scheduler, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, ref StackCrawlMark stackMark)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		Task.CreationOptionsFromContinuationOptions(continuationOptions, out var creationOptions, out var internalOptions);
		Task<TNewResult> task = new ContinuationResultTaskFromResultTask<TResult, TNewResult>(this, continuationFunction, null, creationOptions, internalOptions, ref stackMark);
		ContinueWithCore(task, scheduler, cancellationToken, continuationOptions);
		return task;
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation function.</param>
	/// <typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, object, TNewResult> continuationFunction, object state)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, state, TaskScheduler.Current, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation function.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new task.</param>
	/// <typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, object, TNewResult> continuationFunction, object state, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, state, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation function.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
	/// <typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, object, TNewResult> continuationFunction, object state, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, state, scheduler, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation function.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, object, TNewResult> continuationFunction, object state, TaskContinuationOptions continuationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, state, TaskScheduler.Current, default(CancellationToken), continuationOptions, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task`1" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task`1" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation function.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new task.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
	/// <typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The  <paramref name="continuationOptions" />  argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, object, TNewResult> continuationFunction, object state, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, state, scheduler, cancellationToken, continuationOptions, ref stackMark);
	}

	internal Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, object, TNewResult> continuationFunction, object state, TaskScheduler scheduler, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, ref StackCrawlMark stackMark)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		Task.CreationOptionsFromContinuationOptions(continuationOptions, out var creationOptions, out var internalOptions);
		Task<TNewResult> task = new ContinuationResultTaskFromResultTask<TResult, TNewResult>(this, continuationFunction, state, creationOptions, internalOptions, ref stackMark);
		ContinueWithCore(task, scheduler, cancellationToken, continuationOptions);
		return task;
	}
}
/// <summary>Represents an asynchronous operation.</summary>
[DebuggerTypeProxy(typeof(SystemThreadingTasks_TaskDebugView))]
[DebuggerDisplay("Id = {Id}, Status = {Status}, Method = {DebuggerDisplayMethodDescription}")]
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public class Task : IThreadPoolWorkItem, IAsyncResult, IDisposable
{
	internal class ContingentProperties
	{
		internal ExecutionContext m_capturedContext;

		internal volatile ManualResetEventSlim m_completionEvent;

		internal volatile TaskExceptionHolder m_exceptionsHolder;

		internal CancellationToken m_cancellationToken;

		internal Shared<CancellationTokenRegistration> m_cancellationRegistration;

		internal volatile int m_internalCancellationRequested;

		internal volatile int m_completionCountdown = 1;

		internal volatile List<Task> m_exceptionalChildren;

		internal void SetCompleted()
		{
			m_completionEvent?.Set();
		}

		internal void DeregisterCancellationCallback()
		{
			if (m_cancellationRegistration != null)
			{
				try
				{
					m_cancellationRegistration.Value.Dispose();
				}
				catch (ObjectDisposedException)
				{
				}
				m_cancellationRegistration = null;
			}
		}
	}

	private sealed class SetOnInvokeMres : ManualResetEventSlim, ITaskCompletionAction
	{
		internal SetOnInvokeMres()
			: base(initialState: false, 0)
		{
		}

		public void Invoke(Task completingTask)
		{
			Set();
		}
	}

	private sealed class SetOnCountdownMres : ManualResetEventSlim, ITaskCompletionAction
	{
		private int _count;

		internal SetOnCountdownMres(int count)
		{
			_count = count;
		}

		public void Invoke(Task completingTask)
		{
			if (Interlocked.Decrement(ref _count) == 0)
			{
				Set();
			}
		}
	}

	private sealed class DelayPromise : Task<VoidTaskResult>
	{
		internal readonly CancellationToken Token;

		internal CancellationTokenRegistration Registration;

		internal Timer Timer;

		internal DelayPromise(CancellationToken token)
		{
			Token = token;
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Required, base.Id, "Task.Delay", 0uL);
			}
			if (s_asyncDebuggingEnabled)
			{
				AddToActiveTasks(this);
			}
		}

		internal void Complete()
		{
			bool flag;
			if (Token.IsCancellationRequested)
			{
				flag = TrySetCanceled(Token);
			}
			else
			{
				if (AsyncCausalityTracer.LoggingOn)
				{
					AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, base.Id, AsyncCausalityStatus.Completed);
				}
				if (s_asyncDebuggingEnabled)
				{
					RemoveFromActiveTasks(base.Id);
				}
				flag = TrySetResult(default(VoidTaskResult));
			}
			if (flag)
			{
				if (Timer != null)
				{
					Timer.Dispose();
				}
				Registration.Dispose();
			}
		}
	}

	private sealed class WhenAllPromise : Task<VoidTaskResult>, ITaskCompletionAction
	{
		private readonly Task[] m_tasks;

		private int m_count;

		internal override bool ShouldNotifyDebuggerOfWaitCompletion
		{
			get
			{
				if (base.ShouldNotifyDebuggerOfWaitCompletion)
				{
					return AnyTaskRequiresNotifyDebuggerOfWaitCompletion(m_tasks);
				}
				return false;
			}
		}

		internal WhenAllPromise(Task[] tasks)
		{
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Required, base.Id, "Task.WhenAll", 0uL);
			}
			if (s_asyncDebuggingEnabled)
			{
				AddToActiveTasks(this);
			}
			m_tasks = tasks;
			m_count = tasks.Length;
			foreach (Task task in tasks)
			{
				if (task.IsCompleted)
				{
					Invoke(task);
				}
				else
				{
					task.AddCompletionAction(this);
				}
			}
		}

		public void Invoke(Task completedTask)
		{
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationRelation(CausalityTraceLevel.Important, base.Id, CausalityRelation.Join);
			}
			if (Interlocked.Decrement(ref m_count) != 0)
			{
				return;
			}
			List<ExceptionDispatchInfo> list = null;
			Task task = null;
			for (int i = 0; i < m_tasks.Length; i++)
			{
				Task task2 = m_tasks[i];
				if (task2.IsFaulted)
				{
					if (list == null)
					{
						list = new List<ExceptionDispatchInfo>();
					}
					list.AddRange(task2.GetExceptionDispatchInfos());
				}
				else if (task2.IsCanceled && task == null)
				{
					task = task2;
				}
				if (task2.IsWaitNotificationEnabled)
				{
					SetNotificationForWaitCompletion(enabled: true);
				}
				else
				{
					m_tasks[i] = null;
				}
			}
			if (list != null)
			{
				TrySetException(list);
				return;
			}
			if (task != null)
			{
				TrySetCanceled(task.CancellationToken, task.GetCancellationExceptionDispatchInfo());
				return;
			}
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, base.Id, AsyncCausalityStatus.Completed);
			}
			if (s_asyncDebuggingEnabled)
			{
				RemoveFromActiveTasks(base.Id);
			}
			TrySetResult(default(VoidTaskResult));
		}
	}

	private sealed class WhenAllPromise<T> : Task<T[]>, ITaskCompletionAction
	{
		private readonly Task<T>[] m_tasks;

		private int m_count;

		internal override bool ShouldNotifyDebuggerOfWaitCompletion
		{
			get
			{
				if (base.ShouldNotifyDebuggerOfWaitCompletion)
				{
					return AnyTaskRequiresNotifyDebuggerOfWaitCompletion(m_tasks);
				}
				return false;
			}
		}

		internal WhenAllPromise(Task<T>[] tasks)
		{
			m_tasks = tasks;
			m_count = tasks.Length;
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Required, base.Id, "Task.WhenAll", 0uL);
			}
			if (s_asyncDebuggingEnabled)
			{
				AddToActiveTasks(this);
			}
			foreach (Task<T> task in tasks)
			{
				if (task.IsCompleted)
				{
					Invoke(task);
				}
				else
				{
					task.AddCompletionAction(this);
				}
			}
		}

		public void Invoke(Task ignored)
		{
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationRelation(CausalityTraceLevel.Important, base.Id, CausalityRelation.Join);
			}
			if (Interlocked.Decrement(ref m_count) != 0)
			{
				return;
			}
			T[] array = new T[m_tasks.Length];
			List<ExceptionDispatchInfo> list = null;
			Task task = null;
			for (int i = 0; i < m_tasks.Length; i++)
			{
				Task<T> task2 = m_tasks[i];
				if (task2.IsFaulted)
				{
					if (list == null)
					{
						list = new List<ExceptionDispatchInfo>();
					}
					list.AddRange(task2.GetExceptionDispatchInfos());
				}
				else if (task2.IsCanceled)
				{
					if (task == null)
					{
						task = task2;
					}
				}
				else
				{
					array[i] = task2.GetResultCore(waitCompletionNotification: false);
				}
				if (task2.IsWaitNotificationEnabled)
				{
					SetNotificationForWaitCompletion(enabled: true);
				}
				else
				{
					m_tasks[i] = null;
				}
			}
			if (list != null)
			{
				TrySetException(list);
				return;
			}
			if (task != null)
			{
				TrySetCanceled(task.CancellationToken, task.GetCancellationExceptionDispatchInfo());
				return;
			}
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, base.Id, AsyncCausalityStatus.Completed);
			}
			if (s_asyncDebuggingEnabled)
			{
				RemoveFromActiveTasks(base.Id);
			}
			TrySetResult(array);
		}
	}

	[ThreadStatic]
	internal static Task t_currentTask;

	[ThreadStatic]
	private static StackGuard t_stackGuard;

	internal static int s_taskIdCounter;

	private static readonly TaskFactory s_factory = new TaskFactory();

	private volatile int m_taskId;

	internal object m_action;

	internal object m_stateObject;

	internal TaskScheduler m_taskScheduler;

	internal readonly Task m_parent;

	internal volatile int m_stateFlags;

	private const int OptionsMask = 65535;

	internal const int TASK_STATE_STARTED = 65536;

	internal const int TASK_STATE_DELEGATE_INVOKED = 131072;

	internal const int TASK_STATE_DISPOSED = 262144;

	internal const int TASK_STATE_EXCEPTIONOBSERVEDBYPARENT = 524288;

	internal const int TASK_STATE_CANCELLATIONACKNOWLEDGED = 1048576;

	internal const int TASK_STATE_FAULTED = 2097152;

	internal const int TASK_STATE_CANCELED = 4194304;

	internal const int TASK_STATE_WAITING_ON_CHILDREN = 8388608;

	internal const int TASK_STATE_RAN_TO_COMPLETION = 16777216;

	internal const int TASK_STATE_WAITINGFORACTIVATION = 33554432;

	internal const int TASK_STATE_COMPLETION_RESERVED = 67108864;

	internal const int TASK_STATE_THREAD_WAS_ABORTED = 134217728;

	internal const int TASK_STATE_WAIT_COMPLETION_NOTIFICATION = 268435456;

	internal const int TASK_STATE_EXECUTIONCONTEXT_IS_NULL = 536870912;

	internal const int TASK_STATE_TASKSCHEDULED_WAS_FIRED = 1073741824;

	private const int TASK_STATE_COMPLETED_MASK = 23068672;

	private const int CANCELLATION_REQUESTED = 1;

	private volatile object m_continuationObject;

	private static readonly object s_taskCompletionSentinel = new object();

	[FriendAccessAllowed]
	internal static bool s_asyncDebuggingEnabled;

	private static readonly Dictionary<int, Task> s_currentActiveTasks = new Dictionary<int, Task>();

	private static readonly object s_activeTasksLock = new object();

	internal volatile ContingentProperties m_contingentProperties;

	private static readonly Action<object> s_taskCancelCallback = TaskCancelCallback;

	private static readonly Func<ContingentProperties> s_createContingentProperties = () => new ContingentProperties();

	private static Task s_completedTask;

	private static readonly Predicate<Task> s_IsExceptionObservedByParentPredicate = (Task t) => t.IsExceptionObservedByParent;

	[SecurityCritical]
	private static ContextCallback s_ecCallback;

	private static readonly Predicate<object> s_IsTaskContinuationNullPredicate = (object tc) => tc == null;

	private string DebuggerDisplayMethodDescription
	{
		get
		{
			Delegate @delegate = (Delegate)m_action;
			if ((object)@delegate == null)
			{
				return "{null}";
			}
			return @delegate.Method.ToString();
		}
	}

	internal TaskCreationOptions Options => OptionsMethod(m_stateFlags);

	internal bool IsWaitNotificationEnabledOrNotRanToCompletion
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (m_stateFlags & 0x11000000) != 16777216;
		}
	}

	internal virtual bool ShouldNotifyDebuggerOfWaitCompletion => IsWaitNotificationEnabled;

	internal bool IsWaitNotificationEnabled => (m_stateFlags & 0x10000000) != 0;

	/// <summary>Gets a unique ID for this <see cref="T:System.Threading.Tasks.Task" /> instance.</summary>
	/// <returns>An integer that was assigned by the system to this task instance.</returns>
	public int Id
	{
		get
		{
			if (m_taskId == 0)
			{
				int value = NewId();
				Interlocked.CompareExchange(ref m_taskId, value, 0);
			}
			return m_taskId;
		}
	}

	/// <summary>Returns the unique ID of the currently executing <see cref="T:System.Threading.Tasks.Task" />.</summary>
	/// <returns>An integer that was assigned by the system to the currently-executing task.</returns>
	public static int? CurrentId => InternalCurrent?.Id;

	internal static Task InternalCurrent => t_currentTask;

	internal static StackGuard CurrentStackGuard
	{
		get
		{
			StackGuard stackGuard = t_stackGuard;
			if (stackGuard == null)
			{
				stackGuard = (t_stackGuard = new StackGuard());
			}
			return stackGuard;
		}
	}

	/// <summary>Gets the <see cref="T:System.AggregateException" /> that caused the <see cref="T:System.Threading.Tasks.Task" /> to end prematurely. If the <see cref="T:System.Threading.Tasks.Task" /> completed successfully or has not yet thrown any exceptions, this will return null.</summary>
	/// <returns>The <see cref="T:System.AggregateException" /> that caused the <see cref="T:System.Threading.Tasks.Task" /> to end prematurely.</returns>
	public AggregateException Exception
	{
		get
		{
			AggregateException result = null;
			if (IsFaulted)
			{
				result = GetExceptions(includeTaskCanceledExceptions: false);
			}
			return result;
		}
	}

	/// <summary>Gets the <see cref="T:System.Threading.Tasks.TaskStatus" /> of this task.</summary>
	/// <returns>The current <see cref="T:System.Threading.Tasks.TaskStatus" /> of this task instance.</returns>
	public TaskStatus Status
	{
		get
		{
			int stateFlags = m_stateFlags;
			if ((stateFlags & 0x200000) != 0)
			{
				return TaskStatus.Faulted;
			}
			if ((stateFlags & 0x400000) != 0)
			{
				return TaskStatus.Canceled;
			}
			if ((stateFlags & 0x1000000) != 0)
			{
				return TaskStatus.RanToCompletion;
			}
			if ((stateFlags & 0x800000) != 0)
			{
				return TaskStatus.WaitingForChildrenToComplete;
			}
			if ((stateFlags & 0x20000) != 0)
			{
				return TaskStatus.Running;
			}
			if ((stateFlags & 0x10000) != 0)
			{
				return TaskStatus.WaitingToRun;
			}
			if ((stateFlags & 0x2000000) != 0)
			{
				return TaskStatus.WaitingForActivation;
			}
			return TaskStatus.Created;
		}
	}

	/// <summary>Gets whether this <see cref="T:System.Threading.Tasks.Task" /> instance has completed execution due to being canceled.</summary>
	/// <returns>true if the task has completed due to being canceled; otherwise false.</returns>
	public bool IsCanceled => (m_stateFlags & 0x600000) == 4194304;

	internal bool IsCancellationRequested
	{
		get
		{
			ContingentProperties contingentProperties = m_contingentProperties;
			if (contingentProperties != null)
			{
				if (contingentProperties.m_internalCancellationRequested != 1)
				{
					return contingentProperties.m_cancellationToken.IsCancellationRequested;
				}
				return true;
			}
			return false;
		}
	}

	internal CancellationToken CancellationToken => m_contingentProperties?.m_cancellationToken ?? default(CancellationToken);

	internal bool IsCancellationAcknowledged => (m_stateFlags & 0x100000) != 0;

	/// <summary>Gets whether this <see cref="T:System.Threading.Tasks.Task" /> has completed.</summary>
	/// <returns>true if the task has completed; otherwise false.</returns>
	public bool IsCompleted => IsCompletedMethod(m_stateFlags);

	public bool IsCompletedSuccessfully => (m_stateFlags & 0x1600000) == 16777216;

	internal bool IsRanToCompletion => (m_stateFlags & 0x1600000) == 16777216;

	/// <summary>Gets the <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to create this task.</summary>
	/// <returns>The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to create this task.</returns>
	public TaskCreationOptions CreationOptions => Options & (TaskCreationOptions)(-65281);

	/// <summary>Gets a <see cref="T:System.Threading.WaitHandle" /> that can be used to wait for the task to complete.</summary>
	/// <returns>A <see cref="T:System.Threading.WaitHandle" /> that can be used to wait for the task to complete.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	WaitHandle IAsyncResult.AsyncWaitHandle
	{
		get
		{
			if ((m_stateFlags & 0x40000) != 0)
			{
				throw new ObjectDisposedException(null, Environment.GetResourceString("The task has been disposed."));
			}
			return CompletedEvent.WaitHandle;
		}
	}

	/// <summary>Gets the state object supplied when the <see cref="T:System.Threading.Tasks.Task" /> was created, or null if none was supplied.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the state data that was passed in to the task when it was created.</returns>
	public object AsyncState => m_stateObject;

	/// <summary>Gets an indication of whether the operation completed synchronously.</summary>
	/// <returns>true if the operation completed synchronously; otherwise, false.</returns>
	bool IAsyncResult.CompletedSynchronously => false;

	internal TaskScheduler ExecutingTaskScheduler => m_taskScheduler;

	/// <summary>Provides access to factory methods for creating <see cref="T:System.Threading.Tasks.Task" /> and <see cref="T:System.Threading.Tasks.Task`1" /> instances.</summary>
	/// <returns>The default <see cref="T:System.Threading.Tasks.TaskFactory" /> for the current task.</returns>
	public static TaskFactory Factory => s_factory;

	public static Task CompletedTask
	{
		get
		{
			Task task = s_completedTask;
			if (task == null)
			{
				task = (s_completedTask = new Task(canceled: false, (TaskCreationOptions)16384, default(CancellationToken)));
			}
			return task;
		}
	}

	internal ManualResetEventSlim CompletedEvent
	{
		get
		{
			ContingentProperties contingentProperties = EnsureContingentPropertiesInitialized(needsProtection: true);
			if (contingentProperties.m_completionEvent == null)
			{
				bool isCompleted = IsCompleted;
				ManualResetEventSlim manualResetEventSlim = new ManualResetEventSlim(isCompleted);
				if (Interlocked.CompareExchange(ref contingentProperties.m_completionEvent, manualResetEventSlim, null) != null)
				{
					manualResetEventSlim.Dispose();
				}
				else if (!isCompleted && IsCompleted)
				{
					manualResetEventSlim.Set();
				}
			}
			return contingentProperties.m_completionEvent;
		}
	}

	internal bool IsSelfReplicatingRoot => (Options & (TaskCreationOptions)2304) == (TaskCreationOptions)2048;

	internal bool IsChildReplica => (Options & (TaskCreationOptions)256) != 0;

	internal int ActiveChildCount
	{
		get
		{
			ContingentProperties contingentProperties = m_contingentProperties;
			if (contingentProperties == null)
			{
				return 0;
			}
			return contingentProperties.m_completionCountdown - 1;
		}
	}

	internal bool ExceptionRecorded
	{
		get
		{
			ContingentProperties contingentProperties = m_contingentProperties;
			if (contingentProperties != null && contingentProperties.m_exceptionsHolder != null)
			{
				return contingentProperties.m_exceptionsHolder.ContainsFaultList;
			}
			return false;
		}
	}

	/// <summary>Gets whether the <see cref="T:System.Threading.Tasks.Task" /> completed due to an unhandled exception.</summary>
	/// <returns>true if the task has thrown an unhandled exception; otherwise false.</returns>
	public bool IsFaulted => (m_stateFlags & 0x200000) != 0;

	internal ExecutionContext CapturedContext
	{
		get
		{
			if ((m_stateFlags & 0x20000000) == 536870912)
			{
				return null;
			}
			ContingentProperties contingentProperties = m_contingentProperties;
			if (contingentProperties != null && contingentProperties.m_capturedContext != null)
			{
				return contingentProperties.m_capturedContext;
			}
			return ExecutionContext.PreAllocatedDefault;
		}
		set
		{
			if (value == null)
			{
				m_stateFlags |= 536870912;
			}
			else if (!value.IsPreAllocatedDefault)
			{
				EnsureContingentPropertiesInitialized(needsProtection: false).m_capturedContext = value;
			}
		}
	}

	internal bool IsExceptionObservedByParent => (m_stateFlags & 0x80000) != 0;

	internal bool IsDelegateInvoked => (m_stateFlags & 0x20000) != 0;

	internal virtual object SavedStateForNextReplica
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	internal virtual object SavedStateFromPreviousReplica
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	internal virtual Task HandedOverChildReplica
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[FriendAccessAllowed]
	internal static bool AddToActiveTasks(Task task)
	{
		lock (s_activeTasksLock)
		{
			s_currentActiveTasks[task.Id] = task;
		}
		return true;
	}

	[FriendAccessAllowed]
	internal static void RemoveFromActiveTasks(int taskId)
	{
		lock (s_activeTasksLock)
		{
			s_currentActiveTasks.Remove(taskId);
		}
	}

	internal Task(bool canceled, TaskCreationOptions creationOptions, CancellationToken ct)
	{
		if (canceled)
		{
			m_stateFlags = (int)((TaskCreationOptions)5242880 | creationOptions);
			ContingentProperties contingentProperties = (m_contingentProperties = new ContingentProperties());
			contingentProperties.m_cancellationToken = ct;
			contingentProperties.m_internalCancellationRequested = 1;
		}
		else
		{
			m_stateFlags = (int)((TaskCreationOptions)16777216 | creationOptions);
		}
	}

	internal Task()
	{
		m_stateFlags = 33555456;
	}

	internal Task(object state, TaskCreationOptions creationOptions, bool promiseStyle)
	{
		if ((creationOptions & ~(TaskCreationOptions.AttachedToParent | TaskCreationOptions.RunContinuationsAsynchronously)) != 0)
		{
			throw new ArgumentOutOfRangeException("creationOptions");
		}
		if ((creationOptions & TaskCreationOptions.AttachedToParent) != 0)
		{
			m_parent = InternalCurrent;
		}
		TaskConstructorCore(null, state, default(CancellationToken), creationOptions, InternalTaskOptions.PromiseTask, null);
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action.</summary>
	/// <param name="action">The delegate that represents the code to execute in the task.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Action action)
		: this(action, null, null, default(CancellationToken), TaskCreationOptions.None, InternalTaskOptions.None, null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action and <see cref="T:System.Threading.CancellationToken" />.</summary>
	/// <param name="action">The delegate that represents the code to execute in the task.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that the new  task will observe.</param>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Action action, CancellationToken cancellationToken)
		: this(action, null, null, cancellationToken, TaskCreationOptions.None, InternalTaskOptions.None, null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action and creation options.</summary>
	/// <param name="action">The delegate that represents the code to execute in the task.</param>
	/// <param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Action action, TaskCreationOptions creationOptions)
		: this(action, null, InternalCurrentIfAttached(creationOptions), default(CancellationToken), creationOptions, InternalTaskOptions.None, null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action and creation options.</summary>
	/// <param name="action">The delegate that represents the code to execute in the task.</param>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that the new task will observe.</param>
	/// <param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
		: this(action, null, InternalCurrentIfAttached(creationOptions), cancellationToken, creationOptions, InternalTaskOptions.None, null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action and state.</summary>
	/// <param name="action">The delegate that represents the code to execute in the task.</param>
	/// <param name="state">An object representing data to be used by the action.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Action<object> action, object state)
		: this(action, state, null, default(CancellationToken), TaskCreationOptions.None, InternalTaskOptions.None, null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action, state, and options.</summary>
	/// <param name="action">The delegate that represents the code to execute in the task.</param>
	/// <param name="state">An object representing data to be used by the action.</param>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that that the new task will observe.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Action<object> action, object state, CancellationToken cancellationToken)
		: this(action, state, null, cancellationToken, TaskCreationOptions.None, InternalTaskOptions.None, null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action, state, and options.</summary>
	/// <param name="action">The delegate that represents the code to execute in the task.</param>
	/// <param name="state">An object representing data to be used by the action.</param>
	/// <param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Action<object> action, object state, TaskCreationOptions creationOptions)
		: this(action, state, InternalCurrentIfAttached(creationOptions), default(CancellationToken), creationOptions, InternalTaskOptions.None, null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	/// <summary>Initializes a new <see cref="T:System.Threading.Tasks.Task" /> with the specified action, state, and options.</summary>
	/// <param name="action">The delegate that represents the code to execute in the task.</param>
	/// <param name="state">An object representing data to be used by the action.</param>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that that the new task will observe..</param>
	/// <param name="creationOptions">The <see cref="T:System.Threading.Tasks.TaskCreationOptions" /> used to customize the task's behavior.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="action" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskCreationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task(Action<object> action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
		: this(action, state, InternalCurrentIfAttached(creationOptions), cancellationToken, creationOptions, InternalTaskOptions.None, null)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		PossiblyCaptureContext(ref stackMark);
	}

	internal Task(Action<object> action, object state, Task parent, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler, ref StackCrawlMark stackMark)
		: this(action, state, parent, cancellationToken, creationOptions, internalOptions, scheduler)
	{
		PossiblyCaptureContext(ref stackMark);
	}

	internal Task(Delegate action, object state, Task parent, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler)
	{
		if ((object)action == null)
		{
			throw new ArgumentNullException("action");
		}
		if ((creationOptions & TaskCreationOptions.AttachedToParent) != 0 || (internalOptions & InternalTaskOptions.SelfReplicating) != 0)
		{
			m_parent = parent;
		}
		TaskConstructorCore(action, state, cancellationToken, creationOptions, internalOptions, scheduler);
	}

	internal void TaskConstructorCore(object action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler)
	{
		m_action = action;
		m_stateObject = state;
		m_taskScheduler = scheduler;
		if ((creationOptions & ~(TaskCreationOptions.PreferFairness | TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent | TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler | TaskCreationOptions.RunContinuationsAsynchronously)) != 0)
		{
			throw new ArgumentOutOfRangeException("creationOptions");
		}
		if ((creationOptions & TaskCreationOptions.LongRunning) != 0 && (internalOptions & InternalTaskOptions.SelfReplicating) != 0)
		{
			throw new InvalidOperationException(Environment.GetResourceString("(Internal)An attempt was made to create a LongRunning SelfReplicating task."));
		}
		int num = (int)creationOptions | (int)internalOptions;
		if (m_action == null || (internalOptions & InternalTaskOptions.ContinuationTask) != 0)
		{
			num |= 0x2000000;
		}
		m_stateFlags = num;
		if (m_parent != null && (creationOptions & TaskCreationOptions.AttachedToParent) != 0 && (m_parent.CreationOptions & TaskCreationOptions.DenyChildAttach) == 0)
		{
			m_parent.AddNewChild();
		}
		if (cancellationToken.CanBeCanceled)
		{
			AssignCancellationToken(cancellationToken, null, null);
		}
	}

	private void AssignCancellationToken(CancellationToken cancellationToken, Task antecedent, TaskContinuation continuation)
	{
		ContingentProperties contingentProperties = EnsureContingentPropertiesInitialized(needsProtection: false);
		contingentProperties.m_cancellationToken = cancellationToken;
		try
		{
			if (AppContextSwitches.ThrowExceptionIfDisposedCancellationTokenSource)
			{
				cancellationToken.ThrowIfSourceDisposed();
			}
			if ((Options & (TaskCreationOptions)13312) == 0)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					InternalCancel(bCancelNonExecutingOnly: false);
					return;
				}
				CancellationTokenRegistration value = ((antecedent != null) ? cancellationToken.InternalRegisterWithoutEC(s_taskCancelCallback, new Tuple<Task, Task, TaskContinuation>(this, antecedent, continuation)) : cancellationToken.InternalRegisterWithoutEC(s_taskCancelCallback, this));
				contingentProperties.m_cancellationRegistration = new Shared<CancellationTokenRegistration>(value);
			}
		}
		catch
		{
			if (m_parent != null && (Options & TaskCreationOptions.AttachedToParent) != 0 && (m_parent.Options & TaskCreationOptions.DenyChildAttach) == 0)
			{
				m_parent.DisregardChild();
			}
			throw;
		}
	}

	private static void TaskCancelCallback(object o)
	{
		Task task = o as Task;
		if (task == null && o is Tuple<Task, Task, TaskContinuation> tuple)
		{
			task = tuple.Item1;
			Task item = tuple.Item2;
			TaskContinuation item2 = tuple.Item3;
			item.RemoveContinuation(item2);
		}
		task.InternalCancel(bCancelNonExecutingOnly: false);
	}

	[SecuritySafeCritical]
	internal void PossiblyCaptureContext(ref StackCrawlMark stackMark)
	{
		CapturedContext = ExecutionContext.Capture(ref stackMark, ExecutionContext.CaptureOptions.IgnoreSyncCtx | ExecutionContext.CaptureOptions.OptimizeDefaultCase);
	}

	internal static TaskCreationOptions OptionsMethod(int flags)
	{
		return (TaskCreationOptions)(flags & 0xFFFF);
	}

	internal bool AtomicStateUpdate(int newBits, int illegalBits)
	{
		SpinWait spinWait = default(SpinWait);
		while (true)
		{
			int stateFlags = m_stateFlags;
			if ((stateFlags & illegalBits) != 0)
			{
				return false;
			}
			if (Interlocked.CompareExchange(ref m_stateFlags, stateFlags | newBits, stateFlags) == stateFlags)
			{
				break;
			}
			spinWait.SpinOnce();
		}
		return true;
	}

	internal bool AtomicStateUpdate(int newBits, int illegalBits, ref int oldFlags)
	{
		SpinWait spinWait = default(SpinWait);
		while (true)
		{
			oldFlags = m_stateFlags;
			if ((oldFlags & illegalBits) != 0)
			{
				return false;
			}
			if (Interlocked.CompareExchange(ref m_stateFlags, oldFlags | newBits, oldFlags) == oldFlags)
			{
				break;
			}
			spinWait.SpinOnce();
		}
		return true;
	}

	internal void SetNotificationForWaitCompletion(bool enabled)
	{
		if (enabled)
		{
			AtomicStateUpdate(268435456, 90177536);
			return;
		}
		SpinWait spinWait = default(SpinWait);
		while (true)
		{
			int stateFlags = m_stateFlags;
			int value = stateFlags & -268435457;
			if (Interlocked.CompareExchange(ref m_stateFlags, value, stateFlags) != stateFlags)
			{
				spinWait.SpinOnce();
				continue;
			}
			break;
		}
	}

	internal bool NotifyDebuggerOfWaitCompletionIfNecessary()
	{
		if (IsWaitNotificationEnabled && ShouldNotifyDebuggerOfWaitCompletion)
		{
			NotifyDebuggerOfWaitCompletion();
			return true;
		}
		return false;
	}

	internal static bool AnyTaskRequiresNotifyDebuggerOfWaitCompletion(Task[] tasks)
	{
		foreach (Task task in tasks)
		{
			if (task != null && task.IsWaitNotificationEnabled && task.ShouldNotifyDebuggerOfWaitCompletion)
			{
				return true;
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	private void NotifyDebuggerOfWaitCompletion()
	{
		SetNotificationForWaitCompletion(enabled: false);
	}

	internal bool MarkStarted()
	{
		return AtomicStateUpdate(65536, 4259840);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool FireTaskScheduledIfNeeded(TaskScheduler ts)
	{
		return false;
	}

	internal void AddNewChild()
	{
		ContingentProperties contingentProperties = EnsureContingentPropertiesInitialized(needsProtection: true);
		if (contingentProperties.m_completionCountdown == 1 && !IsSelfReplicatingRoot)
		{
			contingentProperties.m_completionCountdown++;
		}
		else
		{
			Interlocked.Increment(ref contingentProperties.m_completionCountdown);
		}
	}

	internal void DisregardChild()
	{
		Interlocked.Decrement(ref EnsureContingentPropertiesInitialized(needsProtection: true).m_completionCountdown);
	}

	/// <summary>Starts the <see cref="T:System.Threading.Tasks.Task" />, scheduling it for execution to the current <see cref="T:System.Threading.Tasks.TaskScheduler" />.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> instance has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Threading.Tasks.Task" /> is not in a valid state to be started. It may have already been started, executed, or canceled, or it may have been created in a manner that doesn't support direct scheduling.</exception>
	public void Start()
	{
		Start(TaskScheduler.Current);
	}

	/// <summary>Starts the <see cref="T:System.Threading.Tasks.Task" />, scheduling it for execution to the specified <see cref="T:System.Threading.Tasks.TaskScheduler" />.</summary>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> with which to associate and execute this task.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> instance has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Threading.Tasks.Task" /> is not in a valid state to be started. It may have already been started, executed, or canceled, or it may have been created in a manner that doesn't support direct scheduling.</exception>
	public void Start(TaskScheduler scheduler)
	{
		int stateFlags = m_stateFlags;
		if (IsCompletedMethod(stateFlags))
		{
			throw new InvalidOperationException(Environment.GetResourceString("Start may not be called on a task that has completed."));
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		TaskCreationOptions num = OptionsMethod(stateFlags);
		if ((num & (TaskCreationOptions)1024) != 0)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Start may not be called on a promise-style task."));
		}
		if ((num & (TaskCreationOptions)512) != 0)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Start may not be called on a continuation task."));
		}
		if (Interlocked.CompareExchange(ref m_taskScheduler, scheduler, null) != null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Start may not be called on a task that was already started."));
		}
		ScheduleAndStart(needsProtection: true);
	}

	/// <summary>Runs the <see cref="T:System.Threading.Tasks.Task" /> synchronously on the current <see cref="T:System.Threading.Tasks.TaskScheduler" />.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> instance has been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Threading.Tasks.Task" /> is not in a valid state to be started. It may have already been started, executed, or canceled, or it may have been created in a manner that doesn't support direct scheduling.</exception>
	public void RunSynchronously()
	{
		InternalRunSynchronously(TaskScheduler.Current, waitForCompletion: true);
	}

	/// <summary>Runs the <see cref="T:System.Threading.Tasks.Task" /> synchronously on the <see cref="T:System.Threading.Tasks.TaskScheduler" /> provided.</summary>
	/// <param name="scheduler">The scheduler on which to attempt to run this task inline.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> instance has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Threading.Tasks.Task" /> is not in a valid state to be started. It may have already been started, executed, or canceled, or it may have been created in a manner that doesn't support direct scheduling.</exception>
	public void RunSynchronously(TaskScheduler scheduler)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		InternalRunSynchronously(scheduler, waitForCompletion: true);
	}

	[SecuritySafeCritical]
	internal void InternalRunSynchronously(TaskScheduler scheduler, bool waitForCompletion)
	{
		int stateFlags = m_stateFlags;
		TaskCreationOptions num = OptionsMethod(stateFlags);
		if ((num & (TaskCreationOptions)512) != 0)
		{
			throw new InvalidOperationException(Environment.GetResourceString("RunSynchronously may not be called on a continuation task."));
		}
		if ((num & (TaskCreationOptions)1024) != 0)
		{
			throw new InvalidOperationException(Environment.GetResourceString("RunSynchronously may not be called on a task not bound to a delegate, such as the task returned from an asynchronous method."));
		}
		if (IsCompletedMethod(stateFlags))
		{
			throw new InvalidOperationException(Environment.GetResourceString("RunSynchronously may not be called on a task that has already completed."));
		}
		if (Interlocked.CompareExchange(ref m_taskScheduler, scheduler, null) != null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("RunSynchronously may not be called on a task that was already started."));
		}
		if (MarkStarted())
		{
			bool flag = false;
			try
			{
				if (!scheduler.TryRunInline(this, taskWasPreviouslyQueued: false))
				{
					scheduler.InternalQueueTask(this);
					flag = true;
				}
				if (waitForCompletion && !IsCompleted)
				{
					SpinThenBlockingWait(-1, default(CancellationToken));
				}
				return;
			}
			catch (Exception ex)
			{
				if (!flag && !(ex is ThreadAbortException))
				{
					TaskSchedulerException ex2 = new TaskSchedulerException(ex);
					AddException(ex2);
					Finish(bUserDelegateExecuted: false);
					m_contingentProperties.m_exceptionsHolder.MarkAsHandled(calledFromFinalizer: false);
					throw ex2;
				}
				throw;
			}
		}
		throw new InvalidOperationException(Environment.GetResourceString("RunSynchronously may not be called on a task that has already completed."));
	}

	internal static Task InternalStartNew(Task creatingTask, Delegate action, object state, CancellationToken cancellationToken, TaskScheduler scheduler, TaskCreationOptions options, InternalTaskOptions internalOptions, ref StackCrawlMark stackMark)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		Task task = new Task(action, state, creatingTask, cancellationToken, options, internalOptions | InternalTaskOptions.QueuedByRuntime, scheduler);
		task.PossiblyCaptureContext(ref stackMark);
		task.ScheduleAndStart(needsProtection: false);
		return task;
	}

	internal static int NewId()
	{
		int num = 0;
		do
		{
			num = Interlocked.Increment(ref s_taskIdCounter);
		}
		while (num == 0);
		return num;
	}

	internal static Task InternalCurrentIfAttached(TaskCreationOptions creationOptions)
	{
		if ((creationOptions & TaskCreationOptions.AttachedToParent) == 0)
		{
			return null;
		}
		return InternalCurrent;
	}

	internal ContingentProperties EnsureContingentPropertiesInitialized(bool needsProtection)
	{
		ContingentProperties contingentProperties = m_contingentProperties;
		if (contingentProperties == null)
		{
			return EnsureContingentPropertiesInitializedCore(needsProtection);
		}
		return contingentProperties;
	}

	private ContingentProperties EnsureContingentPropertiesInitializedCore(bool needsProtection)
	{
		if (needsProtection)
		{
			return LazyInitializer.EnsureInitialized(ref m_contingentProperties, s_createContingentProperties);
		}
		return m_contingentProperties = new ContingentProperties();
	}

	private static bool IsCompletedMethod(int flags)
	{
		return (flags & 0x1600000) != 0;
	}

	private static ExecutionContext CopyExecutionContext(ExecutionContext capturedContext)
	{
		if (capturedContext == null)
		{
			return null;
		}
		if (capturedContext.IsPreAllocatedDefault)
		{
			return ExecutionContext.PreAllocatedDefault;
		}
		return capturedContext.CreateCopy();
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.Tasks.Task" /> class.</summary>
	/// <exception cref="T:System.InvalidOperationException">The exception that is thrown if the <see cref="T:System.Threading.Tasks.Task" /> is not in one of the final states: <see cref="F:System.Threading.Tasks.TaskStatus.RanToCompletion" />, <see cref="F:System.Threading.Tasks.TaskStatus.Faulted" />, or <see cref="F:System.Threading.Tasks.TaskStatus.Canceled" />.</exception>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Disposes the <see cref="T:System.Threading.Tasks.Task" />, releasing all of its unmanaged resources.</summary>
	/// <param name="disposing">A Boolean value that indicates whether this method is being called due to a call to <see cref="M:System.Threading.Tasks.Task.Dispose" />.</param>
	/// <exception cref="T:System.InvalidOperationException">The exception that is thrown if the <see cref="T:System.Threading.Tasks.Task" /> is not in one of the final states: <see cref="F:System.Threading.Tasks.TaskStatus.RanToCompletion" />, <see cref="F:System.Threading.Tasks.TaskStatus.Faulted" />, or <see cref="F:System.Threading.Tasks.TaskStatus.Canceled" />.</exception>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if ((Options & (TaskCreationOptions)16384) != 0)
			{
				return;
			}
			if (!IsCompleted)
			{
				throw new InvalidOperationException(Environment.GetResourceString("A task may only be disposed if it is in a completion state (RanToCompletion, Faulted or Canceled)."));
			}
			ContingentProperties contingentProperties = m_contingentProperties;
			if (contingentProperties != null)
			{
				ManualResetEventSlim completionEvent = contingentProperties.m_completionEvent;
				if (completionEvent != null)
				{
					contingentProperties.m_completionEvent = null;
					if (!completionEvent.IsSet)
					{
						completionEvent.Set();
					}
					completionEvent.Dispose();
				}
			}
		}
		m_stateFlags |= 262144;
	}

	[SecuritySafeCritical]
	internal void ScheduleAndStart(bool needsProtection)
	{
		if (needsProtection)
		{
			if (!MarkStarted())
			{
				return;
			}
		}
		else
		{
			m_stateFlags |= 65536;
		}
		if (s_asyncDebuggingEnabled)
		{
			AddToActiveTasks(this);
		}
		if (AsyncCausalityTracer.LoggingOn && (Options & (TaskCreationOptions)512) == 0)
		{
			AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Required, Id, "Task: " + ((Delegate)m_action).Method.Name, 0uL);
		}
		try
		{
			m_taskScheduler.InternalQueueTask(this);
		}
		catch (ThreadAbortException exceptionObject)
		{
			AddException(exceptionObject);
			FinishThreadAbortedTask(bTAEAddedToExceptionHolder: true, delegateRan: false);
		}
		catch (Exception innerException)
		{
			TaskSchedulerException ex = new TaskSchedulerException(innerException);
			AddException(ex);
			Finish(bUserDelegateExecuted: false);
			if ((Options & (TaskCreationOptions)512) == 0)
			{
				m_contingentProperties.m_exceptionsHolder.MarkAsHandled(calledFromFinalizer: false);
			}
			throw ex;
		}
	}

	internal void AddException(object exceptionObject)
	{
		AddException(exceptionObject, representsCancellation: false);
	}

	internal void AddException(object exceptionObject, bool representsCancellation)
	{
		ContingentProperties contingentProperties = EnsureContingentPropertiesInitialized(needsProtection: true);
		if (contingentProperties.m_exceptionsHolder == null)
		{
			TaskExceptionHolder taskExceptionHolder = new TaskExceptionHolder(this);
			if (Interlocked.CompareExchange(ref contingentProperties.m_exceptionsHolder, taskExceptionHolder, null) != null)
			{
				taskExceptionHolder.MarkAsHandled(calledFromFinalizer: false);
			}
		}
		lock (contingentProperties)
		{
			contingentProperties.m_exceptionsHolder.Add(exceptionObject, representsCancellation);
		}
	}

	private AggregateException GetExceptions(bool includeTaskCanceledExceptions)
	{
		Exception ex = null;
		if (includeTaskCanceledExceptions && IsCanceled)
		{
			ex = new TaskCanceledException(this);
		}
		if (ExceptionRecorded)
		{
			return m_contingentProperties.m_exceptionsHolder.CreateExceptionObject(calledFromFinalizer: false, ex);
		}
		if (ex != null)
		{
			return new AggregateException(ex);
		}
		return null;
	}

	internal ReadOnlyCollection<ExceptionDispatchInfo> GetExceptionDispatchInfos()
	{
		if (!IsFaulted || !ExceptionRecorded)
		{
			return new ReadOnlyCollection<ExceptionDispatchInfo>(new ExceptionDispatchInfo[0]);
		}
		return m_contingentProperties.m_exceptionsHolder.GetExceptionDispatchInfos();
	}

	internal ExceptionDispatchInfo GetCancellationExceptionDispatchInfo()
	{
		ContingentProperties contingentProperties = m_contingentProperties;
		if (contingentProperties == null)
		{
			return null;
		}
		return contingentProperties.m_exceptionsHolder?.GetCancellationExceptionDispatchInfo();
	}

	internal void ThrowIfExceptional(bool includeTaskCanceledExceptions)
	{
		Exception exceptions = GetExceptions(includeTaskCanceledExceptions);
		if (exceptions != null)
		{
			UpdateExceptionObservedStatus();
			throw exceptions;
		}
	}

	internal void UpdateExceptionObservedStatus()
	{
		if (m_parent != null && (Options & TaskCreationOptions.AttachedToParent) != 0 && (m_parent.CreationOptions & TaskCreationOptions.DenyChildAttach) == 0 && InternalCurrent == m_parent)
		{
			m_stateFlags |= 524288;
		}
	}

	internal void Finish(bool bUserDelegateExecuted)
	{
		if (!bUserDelegateExecuted)
		{
			FinishStageTwo();
			return;
		}
		ContingentProperties contingentProperties = m_contingentProperties;
		if (contingentProperties == null || (contingentProperties.m_completionCountdown == 1 && !IsSelfReplicatingRoot) || Interlocked.Decrement(ref contingentProperties.m_completionCountdown) == 0)
		{
			FinishStageTwo();
		}
		else
		{
			AtomicStateUpdate(8388608, 23068672);
		}
		List<Task> list = ((contingentProperties != null) ? contingentProperties.m_exceptionalChildren : null);
		if (list == null)
		{
			return;
		}
		lock (list)
		{
			list.RemoveAll(s_IsExceptionObservedByParentPredicate);
		}
	}

	internal void FinishStageTwo()
	{
		AddExceptionsFromChildren();
		int num;
		if (ExceptionRecorded)
		{
			num = 2097152;
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, Id, AsyncCausalityStatus.Error);
			}
			if (s_asyncDebuggingEnabled)
			{
				RemoveFromActiveTasks(Id);
			}
		}
		else if (IsCancellationRequested && IsCancellationAcknowledged)
		{
			num = 4194304;
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, Id, AsyncCausalityStatus.Canceled);
			}
			if (s_asyncDebuggingEnabled)
			{
				RemoveFromActiveTasks(Id);
			}
		}
		else
		{
			num = 16777216;
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, Id, AsyncCausalityStatus.Completed);
			}
			if (s_asyncDebuggingEnabled)
			{
				RemoveFromActiveTasks(Id);
			}
		}
		Interlocked.Exchange(ref m_stateFlags, m_stateFlags | num);
		ContingentProperties contingentProperties = m_contingentProperties;
		if (contingentProperties != null)
		{
			contingentProperties.SetCompleted();
			contingentProperties.DeregisterCancellationCallback();
		}
		FinishStageThree();
	}

	internal void FinishStageThree()
	{
		m_action = null;
		if (m_parent != null && (m_parent.CreationOptions & TaskCreationOptions.DenyChildAttach) == 0 && (m_stateFlags & 0xFFFF & 4) != 0)
		{
			m_parent.ProcessChildCompletion(this);
		}
		FinishContinuations();
	}

	internal void ProcessChildCompletion(Task childTask)
	{
		ContingentProperties contingentProperties = m_contingentProperties;
		if (childTask.IsFaulted && !childTask.IsExceptionObservedByParent)
		{
			if (contingentProperties.m_exceptionalChildren == null)
			{
				Interlocked.CompareExchange(ref contingentProperties.m_exceptionalChildren, new List<Task>(), null);
			}
			List<Task> exceptionalChildren = contingentProperties.m_exceptionalChildren;
			if (exceptionalChildren != null)
			{
				lock (exceptionalChildren)
				{
					exceptionalChildren.Add(childTask);
				}
			}
		}
		if (Interlocked.Decrement(ref contingentProperties.m_completionCountdown) == 0)
		{
			FinishStageTwo();
		}
	}

	internal void AddExceptionsFromChildren()
	{
		ContingentProperties contingentProperties = m_contingentProperties;
		List<Task> list = ((contingentProperties != null) ? contingentProperties.m_exceptionalChildren : null);
		if (list == null)
		{
			return;
		}
		lock (list)
		{
			foreach (Task item in list)
			{
				if (item.IsFaulted && !item.IsExceptionObservedByParent)
				{
					TaskExceptionHolder exceptionsHolder = item.m_contingentProperties.m_exceptionsHolder;
					AddException(exceptionsHolder.CreateExceptionObject(calledFromFinalizer: false, null));
				}
			}
		}
		contingentProperties.m_exceptionalChildren = null;
	}

	internal void FinishThreadAbortedTask(bool bTAEAddedToExceptionHolder, bool delegateRan)
	{
		if (bTAEAddedToExceptionHolder)
		{
			m_contingentProperties.m_exceptionsHolder.MarkAsHandled(calledFromFinalizer: false);
		}
		if (AtomicStateUpdate(134217728, 157286400))
		{
			Finish(delegateRan);
		}
	}

	private void Execute()
	{
		if (IsSelfReplicatingRoot)
		{
			ExecuteSelfReplicating(this);
			return;
		}
		try
		{
			InnerInvoke();
		}
		catch (ThreadAbortException unhandledException)
		{
			if (!IsChildReplica)
			{
				HandleException(unhandledException);
				FinishThreadAbortedTask(bTAEAddedToExceptionHolder: true, delegateRan: true);
			}
		}
		catch (Exception unhandledException2)
		{
			HandleException(unhandledException2);
		}
	}

	internal virtual bool ShouldReplicate()
	{
		return true;
	}

	internal virtual Task CreateReplicaTask(Action<object> taskReplicaDelegate, object stateObject, Task parentTask, TaskScheduler taskScheduler, TaskCreationOptions creationOptionsForReplica, InternalTaskOptions internalOptionsForReplica)
	{
		return new Task(taskReplicaDelegate, stateObject, parentTask, default(CancellationToken), creationOptionsForReplica, internalOptionsForReplica, parentTask.ExecutingTaskScheduler);
	}

	private static void ExecuteSelfReplicating(Task root)
	{
		TaskCreationOptions creationOptionsForReplicas = root.CreationOptions | TaskCreationOptions.AttachedToParent;
		InternalTaskOptions internalOptionsForReplicas = InternalTaskOptions.ChildReplica | InternalTaskOptions.SelfReplicating | InternalTaskOptions.QueuedByRuntime;
		bool replicasAreQuitting = false;
		Action<object> taskReplicaDelegate = null;
		taskReplicaDelegate = delegate
		{
			Task internalCurrent = InternalCurrent;
			Task task = internalCurrent.HandedOverChildReplica;
			if (task == null)
			{
				if (!root.ShouldReplicate() || Volatile.Read(ref replicasAreQuitting))
				{
					return;
				}
				ExecutionContext capturedContext = root.CapturedContext;
				task = root.CreateReplicaTask(taskReplicaDelegate, root.m_stateObject, root, root.ExecutingTaskScheduler, creationOptionsForReplicas, internalOptionsForReplicas);
				task.CapturedContext = CopyExecutionContext(capturedContext);
				task.ScheduleAndStart(needsProtection: false);
			}
			try
			{
				root.InnerInvokeWithArg(internalCurrent);
			}
			catch (Exception ex)
			{
				root.HandleException(ex);
				if (ex is ThreadAbortException)
				{
					internalCurrent.FinishThreadAbortedTask(bTAEAddedToExceptionHolder: false, delegateRan: true);
				}
			}
			object savedStateForNextReplica = internalCurrent.SavedStateForNextReplica;
			if (savedStateForNextReplica != null)
			{
				Task task2 = root.CreateReplicaTask(taskReplicaDelegate, root.m_stateObject, root, root.ExecutingTaskScheduler, creationOptionsForReplicas, internalOptionsForReplicas);
				ExecutionContext capturedContext2 = root.CapturedContext;
				task2.CapturedContext = CopyExecutionContext(capturedContext2);
				task2.HandedOverChildReplica = task;
				task2.SavedStateFromPreviousReplica = savedStateForNextReplica;
				task2.ScheduleAndStart(needsProtection: false);
				return;
			}
			replicasAreQuitting = true;
			try
			{
				task.InternalCancel(bCancelNonExecutingOnly: true);
			}
			catch (Exception unhandledException)
			{
				root.HandleException(unhandledException);
			}
		};
		taskReplicaDelegate(null);
	}

	[SecurityCritical]
	void IThreadPoolWorkItem.ExecuteWorkItem()
	{
		ExecuteEntry(bPreventDoubleExecution: false);
	}

	[SecurityCritical]
	void IThreadPoolWorkItem.MarkAborted(ThreadAbortException tae)
	{
		if (!IsCompleted)
		{
			HandleException(tae);
			FinishThreadAbortedTask(bTAEAddedToExceptionHolder: true, delegateRan: false);
		}
	}

	[SecuritySafeCritical]
	internal bool ExecuteEntry(bool bPreventDoubleExecution)
	{
		if (bPreventDoubleExecution || (Options & (TaskCreationOptions)2048) != 0)
		{
			int oldFlags = 0;
			if (!AtomicStateUpdate(131072, 23199744, ref oldFlags) && (oldFlags & 0x400000) == 0)
			{
				return false;
			}
		}
		else
		{
			m_stateFlags |= 131072;
		}
		if (!IsCancellationRequested && !IsCanceled)
		{
			ExecuteWithThreadLocal(ref t_currentTask);
		}
		else if (!IsCanceled && (Interlocked.Exchange(ref m_stateFlags, m_stateFlags | 0x400000) & 0x400000) == 0)
		{
			CancellationCleanupLogic();
		}
		return true;
	}

	[SecurityCritical]
	private void ExecuteWithThreadLocal(ref Task currentTaskSlot)
	{
		Task task = currentTaskSlot;
		try
		{
			currentTaskSlot = this;
			ExecutionContext capturedContext = CapturedContext;
			if (capturedContext == null)
			{
				Execute();
			}
			else
			{
				if (IsSelfReplicatingRoot || IsChildReplica)
				{
					CapturedContext = CopyExecutionContext(capturedContext);
				}
				ContextCallback callback = ExecutionContextCallback;
				ExecutionContext.Run(capturedContext, callback, this, preserveSyncCtx: true);
			}
			if (AsyncCausalityTracer.LoggingOn)
			{
				AsyncCausalityTracer.TraceSynchronousWorkCompletion(CausalityTraceLevel.Required, CausalitySynchronousWork.Execution);
			}
			Finish(bUserDelegateExecuted: true);
		}
		finally
		{
			currentTaskSlot = task;
		}
	}

	[SecurityCritical]
	private static void ExecutionContextCallback(object obj)
	{
		(obj as Task).Execute();
	}

	internal virtual void InnerInvoke()
	{
		if (m_action is Action action)
		{
			action();
		}
		else if (m_action is Action<object> action2)
		{
			action2(m_stateObject);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	internal void InnerInvokeWithArg(Task childTask)
	{
		InnerInvoke();
	}

	private void HandleException(Exception unhandledException)
	{
		if (unhandledException is OperationCanceledException ex && IsCancellationRequested && m_contingentProperties.m_cancellationToken == ex.CancellationToken)
		{
			SetCancellationAcknowledged();
			AddException(ex, representsCancellation: true);
		}
		else
		{
			AddException(unhandledException);
		}
	}

	/// <summary>Gets an awaiter used to await this <see cref="T:System.Threading.Tasks.Task" />.</summary>
	/// <returns>An awaiter instance.</returns>
	public TaskAwaiter GetAwaiter()
	{
		return new TaskAwaiter(this);
	}

	/// <summary>Configures an awaiter used to await this <see cref="T:System.Threading.Tasks.Task" />.</summary>
	/// <returns>An object used to await this task.</returns>
	/// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false.</param>
	public ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext)
	{
		return new ConfiguredTaskAwaitable(this, continueOnCapturedContext);
	}

	[SecurityCritical]
	internal void SetContinuationForAwait(Action continuationAction, bool continueOnCapturedContext, bool flowExecutionContext, ref StackCrawlMark stackMark)
	{
		TaskContinuation taskContinuation = null;
		if (continueOnCapturedContext)
		{
			SynchronizationContext currentNoFlow = SynchronizationContext.CurrentNoFlow;
			if (currentNoFlow != null && currentNoFlow.GetType() != typeof(SynchronizationContext))
			{
				taskContinuation = new SynchronizationContextAwaitTaskContinuation(currentNoFlow, continuationAction, flowExecutionContext, ref stackMark);
			}
			else
			{
				TaskScheduler internalCurrent = TaskScheduler.InternalCurrent;
				if (internalCurrent != null && internalCurrent != TaskScheduler.Default)
				{
					taskContinuation = new TaskSchedulerAwaitTaskContinuation(internalCurrent, continuationAction, flowExecutionContext, ref stackMark);
				}
			}
		}
		if (taskContinuation == null && flowExecutionContext)
		{
			taskContinuation = new AwaitTaskContinuation(continuationAction, flowExecutionContext: true, ref stackMark);
		}
		if (taskContinuation != null)
		{
			if (!AddTaskContinuation(taskContinuation, addBeforeOthers: false))
			{
				taskContinuation.Run(this, bCanInlineContinuationTask: false);
			}
		}
		else if (!AddTaskContinuation(continuationAction, addBeforeOthers: false))
		{
			AwaitTaskContinuation.UnsafeScheduleAction(continuationAction, this);
		}
	}

	/// <summary>Creates an awaitable task that asynchronously yields back to the current context when awaited.</summary>
	/// <returns>A context that, when awaited, will asynchronously transition back into the current context at the time of the await. If the current <see cref="T:System.Threading.SynchronizationContext" /> is non-null, it is treated as the current context. Otherwise, the task scheduler that is associated with the currently executing task is treated as the current context. </returns>
	public static YieldAwaitable Yield()
	{
		return default(YieldAwaitable);
	}

	/// <summary>Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	/// <exception cref="T:System.AggregateException">The <see cref="T:System.Threading.Tasks.Task" /> was canceled -or- an exception was thrown during the execution of the <see cref="T:System.Threading.Tasks.Task" />. If the task was canceled, the <see cref="T:System.AggregateException" /> contains an <see cref="T:System.OperationCanceledException" /> in its <see cref="P:System.AggregateException.InnerExceptions" /> collection.</exception>
	public void Wait()
	{
		Wait(-1, default(CancellationToken));
	}

	/// <summary>Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution within a specified time interval.</summary>
	/// <returns>true if the <see cref="T:System.Threading.Tasks.Task" /> completed execution within the allotted time; otherwise, false.</returns>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <exception cref="T:System.AggregateException">The <see cref="T:System.Threading.Tasks.Task" /> was canceled -or- an exception was thrown during the execution of the <see cref="T:System.Threading.Tasks.Task" />.</exception>
	public bool Wait(TimeSpan timeout)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout");
		}
		return Wait((int)num, default(CancellationToken));
	}

	/// <summary>Waits for the cancellable <see cref="T:System.Threading.Tasks.Task" /> to complete execution.</summary>
	/// <param name="cancellationToken">A <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> to observe while waiting for the task to complete.</param>
	/// <exception cref="T:System.OperationCanceledException">The <paramref name="cancellationToken" /> was canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	/// <exception cref="T:System.AggregateException">The <see cref="T:System.Threading.Tasks.Task" /> was canceled -or- an exception was thrown during the execution of the <see cref="T:System.Threading.Tasks.Task" />. If the task was canceled, the <see cref="T:System.AggregateException" /> contains an <see cref="T:System.OperationCanceledException" /> in its <see cref="P:System.AggregateException.InnerExceptions" /> collection.</exception>
	public void Wait(CancellationToken cancellationToken)
	{
		Wait(-1, cancellationToken);
	}

	/// <summary>Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution within a specified number of milliseconds.</summary>
	/// <returns>true if the <see cref="T:System.Threading.Tasks.Task" /> completed execution within the allotted time; otherwise, false.</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	/// <exception cref="T:System.AggregateException">The <see cref="T:System.Threading.Tasks.Task" /> was canceled -or- an exception was thrown during the execution of the <see cref="T:System.Threading.Tasks.Task" />. If the task was canceled, the <see cref="T:System.AggregateException" /> contains an <see cref="T:System.OperationCanceledException" /> in its <see cref="P:System.AggregateException.InnerExceptions" /> collection.</exception>
	public bool Wait(int millisecondsTimeout)
	{
		return Wait(millisecondsTimeout, default(CancellationToken));
	}

	/// <summary>Waits for the cancellable <see cref="T:System.Threading.Tasks.Task" /> to complete execution.</summary>
	/// <returns>true if the <see cref="T:System.Threading.Tasks.Task" /> completed execution within the allotted time; otherwise, false.</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
	/// <param name="cancellationToken">A <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> to observe while waiting for the task to complete.</param>
	/// <exception cref="T:System.OperationCanceledException">The <paramref name="cancellationToken" /> was canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	/// <exception cref="T:System.AggregateException">The <see cref="T:System.Threading.Tasks.Task" /> was canceled -or- an exception was thrown during the execution of the <see cref="T:System.Threading.Tasks.Task" />. If the task was canceled, the <see cref="T:System.AggregateException" /> contains an <see cref="T:System.OperationCanceledException" /> in its <see cref="P:System.AggregateException.InnerExceptions" /> collection.</exception>
	public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
	{
		if (millisecondsTimeout < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeout");
		}
		if (!IsWaitNotificationEnabledOrNotRanToCompletion)
		{
			return true;
		}
		if (!InternalWait(millisecondsTimeout, cancellationToken))
		{
			return false;
		}
		if (IsWaitNotificationEnabledOrNotRanToCompletion)
		{
			NotifyDebuggerOfWaitCompletionIfNecessary();
			if (IsCanceled)
			{
				cancellationToken.ThrowIfCancellationRequested();
			}
			ThrowIfExceptional(includeTaskCanceledExceptions: true);
		}
		return true;
	}

	private bool WrappedTryRunInline()
	{
		if (m_taskScheduler == null)
		{
			return false;
		}
		try
		{
			return m_taskScheduler.TryRunInline(this, taskWasPreviouslyQueued: true);
		}
		catch (Exception ex)
		{
			if (!(ex is ThreadAbortException))
			{
				throw new TaskSchedulerException(ex);
			}
			throw;
		}
	}

	[MethodImpl(MethodImplOptions.NoOptimization)]
	internal bool InternalWait(int millisecondsTimeout, CancellationToken cancellationToken)
	{
		bool flag = IsCompleted;
		if (!flag)
		{
			Debugger.NotifyOfCrossThreadDependency();
			flag = (millisecondsTimeout == -1 && !cancellationToken.CanBeCanceled && WrappedTryRunInline() && IsCompleted) || SpinThenBlockingWait(millisecondsTimeout, cancellationToken);
		}
		return flag;
	}

	private bool SpinThenBlockingWait(int millisecondsTimeout, CancellationToken cancellationToken)
	{
		bool flag = millisecondsTimeout == -1;
		uint num = ((!flag) ? ((uint)Environment.TickCount) : 0u);
		bool flag2 = SpinWait(millisecondsTimeout);
		if (!flag2)
		{
			SetOnInvokeMres setOnInvokeMres = new SetOnInvokeMres();
			try
			{
				AddCompletionAction(setOnInvokeMres, addBeforeOthers: true);
				if (flag)
				{
					flag2 = setOnInvokeMres.Wait(-1, cancellationToken);
				}
				else
				{
					uint num2 = (uint)Environment.TickCount - num;
					if (num2 < millisecondsTimeout)
					{
						flag2 = setOnInvokeMres.Wait((int)(millisecondsTimeout - num2), cancellationToken);
					}
				}
			}
			finally
			{
				if (!IsCompleted)
				{
					RemoveContinuation(setOnInvokeMres);
				}
			}
		}
		return flag2;
	}

	private bool SpinWait(int millisecondsTimeout)
	{
		if (IsCompleted)
		{
			return true;
		}
		if (millisecondsTimeout == 0)
		{
			return false;
		}
		int num = (PlatformHelper.IsSingleProcessor ? 1 : 10);
		for (int i = 0; i < num; i++)
		{
			if (IsCompleted)
			{
				return true;
			}
			if (i == num / 2)
			{
				Thread.Yield();
			}
			else
			{
				Thread.SpinWait(PlatformHelper.ProcessorCount * (4 << i));
			}
		}
		return IsCompleted;
	}

	[SecuritySafeCritical]
	internal bool InternalCancel(bool bCancelNonExecutingOnly)
	{
		bool flag = false;
		bool flag2 = false;
		TaskSchedulerException ex = null;
		if ((m_stateFlags & 0x10000) != 0)
		{
			TaskScheduler taskScheduler = m_taskScheduler;
			try
			{
				flag = taskScheduler?.TryDequeue(this) ?? false;
			}
			catch (Exception ex2)
			{
				if (!(ex2 is ThreadAbortException))
				{
					ex = new TaskSchedulerException(ex2);
				}
			}
			bool flag3 = (taskScheduler != null && taskScheduler.RequiresAtomicStartTransition) || (Options & (TaskCreationOptions)2048) != 0;
			if (!flag && bCancelNonExecutingOnly && flag3)
			{
				flag2 = AtomicStateUpdate(4194304, 4325376);
			}
		}
		if (!bCancelNonExecutingOnly || flag || flag2)
		{
			RecordInternalCancellationRequest();
			if (flag)
			{
				flag2 = AtomicStateUpdate(4194304, 4325376);
			}
			else if (!flag2 && (m_stateFlags & 0x10000) == 0)
			{
				flag2 = AtomicStateUpdate(4194304, 23265280);
			}
			if (flag2)
			{
				CancellationCleanupLogic();
			}
		}
		if (ex != null)
		{
			throw ex;
		}
		return flag2;
	}

	internal void RecordInternalCancellationRequest()
	{
		EnsureContingentPropertiesInitialized(needsProtection: true).m_internalCancellationRequested = 1;
	}

	internal void RecordInternalCancellationRequest(CancellationToken tokenToRecord)
	{
		RecordInternalCancellationRequest();
		if (tokenToRecord != default(CancellationToken))
		{
			m_contingentProperties.m_cancellationToken = tokenToRecord;
		}
	}

	internal void RecordInternalCancellationRequest(CancellationToken tokenToRecord, object cancellationException)
	{
		RecordInternalCancellationRequest(tokenToRecord);
		if (cancellationException != null)
		{
			AddException(cancellationException, representsCancellation: true);
		}
	}

	internal void CancellationCleanupLogic()
	{
		Interlocked.Exchange(ref m_stateFlags, m_stateFlags | 0x400000);
		ContingentProperties contingentProperties = m_contingentProperties;
		if (contingentProperties != null)
		{
			contingentProperties.SetCompleted();
			contingentProperties.DeregisterCancellationCallback();
		}
		if (AsyncCausalityTracer.LoggingOn)
		{
			AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, Id, AsyncCausalityStatus.Canceled);
		}
		if (s_asyncDebuggingEnabled)
		{
			RemoveFromActiveTasks(Id);
		}
		FinishStageThree();
	}

	private void SetCancellationAcknowledged()
	{
		m_stateFlags |= 1048576;
	}

	[SecuritySafeCritical]
	internal void FinishContinuations()
	{
		object obj = Interlocked.Exchange(ref m_continuationObject, s_taskCompletionSentinel);
		if (obj == null)
		{
			return;
		}
		if (AsyncCausalityTracer.LoggingOn)
		{
			AsyncCausalityTracer.TraceSynchronousWorkStart(CausalityTraceLevel.Required, Id, CausalitySynchronousWork.CompletionNotification);
		}
		bool flag = (m_stateFlags & 0x8000000) == 0 && Thread.CurrentThread.ThreadState != ThreadState.AbortRequested && (m_stateFlags & 0x40) == 0;
		if (obj is Action action)
		{
			AwaitTaskContinuation.RunOrScheduleAction(action, flag, ref t_currentTask);
			LogFinishCompletionNotification();
			return;
		}
		if (obj is ITaskCompletionAction taskCompletionAction)
		{
			if (flag)
			{
				taskCompletionAction.Invoke(this);
			}
			else
			{
				ThreadPool.UnsafeQueueCustomWorkItem(new CompletionActionInvoker(taskCompletionAction, this), forceGlobal: false);
			}
			LogFinishCompletionNotification();
			return;
		}
		if (obj is TaskContinuation taskContinuation)
		{
			taskContinuation.Run(this, flag);
			LogFinishCompletionNotification();
			return;
		}
		if (!(obj is List<object> list))
		{
			LogFinishCompletionNotification();
			return;
		}
		lock (list)
		{
		}
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (list[i] is StandardTaskContinuation standardTaskContinuation && (standardTaskContinuation.m_options & TaskContinuationOptions.ExecuteSynchronously) == 0)
			{
				list[i] = null;
				standardTaskContinuation.Run(this, flag);
			}
		}
		for (int j = 0; j < count; j++)
		{
			object obj2 = list[j];
			if (obj2 == null)
			{
				continue;
			}
			list[j] = null;
			if (obj2 is Action action2)
			{
				AwaitTaskContinuation.RunOrScheduleAction(action2, flag, ref t_currentTask);
				continue;
			}
			if (obj2 is TaskContinuation taskContinuation2)
			{
				taskContinuation2.Run(this, flag);
				continue;
			}
			ITaskCompletionAction taskCompletionAction2 = (ITaskCompletionAction)obj2;
			if (flag)
			{
				taskCompletionAction2.Invoke(this);
			}
			else
			{
				ThreadPool.UnsafeQueueCustomWorkItem(new CompletionActionInvoker(taskCompletionAction2, this), forceGlobal: false);
			}
		}
		LogFinishCompletionNotification();
	}

	private void LogFinishCompletionNotification()
	{
		if (AsyncCausalityTracer.LoggingOn)
		{
			AsyncCausalityTracer.TraceSynchronousWorkCompletion(CausalityTraceLevel.Required, CausalitySynchronousWork.CompletionNotification);
		}
	}

	/// <summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task> continuationAction)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, TaskScheduler.Current, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created the token has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task> continuationAction, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.-or-The <paramref name="scheduler" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task> continuationAction, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, scheduler, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes according to the specified <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run according to the specified <paramref name="continuationOptions" />. When run, the delegate will be passed the completed task as an argument.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task> continuationAction, TaskContinuationOptions continuationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, TaskScheduler.Current, default(CancellationToken), continuationOptions, ref stackMark);
	}

	/// <summary>Creates a continuation that executes according to the specified <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run according to the specified <paramref name="continuationOptions" />. When run, the delegate will be passed the completed task as an argument.</param>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created the token has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.-or-The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, scheduler, cancellationToken, continuationOptions, ref stackMark);
	}

	private Task ContinueWith(Action<Task> continuationAction, TaskScheduler scheduler, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, ref StackCrawlMark stackMark)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		CreationOptionsFromContinuationOptions(continuationOptions, out var creationOptions, out var internalOptions);
		Task task = new ContinuationTaskFromTask(this, continuationAction, null, creationOptions, internalOptions, ref stackMark);
		ContinueWithCore(task, scheduler, cancellationToken, continuationOptions);
		return task;
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be passed the completed task as and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation action.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task, object> continuationAction, object state)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, state, TaskScheduler.Current, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation action.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task, object> continuationAction, object state, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, state, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task" /> completes.  When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation action.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task, object> continuationAction, object state, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, state, scheduler, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation action.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task, object> continuationAction, object state, TaskContinuationOptions continuationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, state, TaskScheduler.Current, default(CancellationToken), continuationOptions, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task" />.</returns>
	/// <param name="continuationAction">An action to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation action.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its  execution.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationAction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task ContinueWith(Action<Task, object> continuationAction, object state, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationAction, state, scheduler, cancellationToken, continuationOptions, ref stackMark);
	}

	private Task ContinueWith(Action<Task, object> continuationAction, object state, TaskScheduler scheduler, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, ref StackCrawlMark stackMark)
	{
		if (continuationAction == null)
		{
			throw new ArgumentNullException("continuationAction");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		CreationOptionsFromContinuationOptions(continuationOptions, out var creationOptions, out var internalOptions);
		Task task = new ContinuationTaskFromTask(this, continuationAction, state, creationOptions, internalOptions, ref stackMark);
		ContinueWithCore(task, scheduler, cancellationToken, continuationOptions);
		return task;
	}

	/// <summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
	/// <typeparam name="TResult"> The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, TaskScheduler.Current, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <typeparam name="TResult"> The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created the token has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes asynchronously when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be passed the completed task as an argument.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
	/// <typeparam name="TResult"> The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.-or-The <paramref name="scheduler" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, scheduler, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes according to the condition specified in <paramref name="continuationOptions" />.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run according to the condition specified in <paramref name="continuationOptions" />. When run, the delegate will be passed the completed task as an argument.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <typeparam name="TResult"> The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction, TaskContinuationOptions continuationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, TaskScheduler.Current, default(CancellationToken), continuationOptions, ref stackMark);
	}

	/// <summary>Creates a continuation that executes according to the condition specified in <paramref name="continuationOptions" />.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run according to the specified <paramref name="continuationOptions." /> When run, the delegate will be passed the completed task as an argument.</param>
	/// <param name="cancellationToken">The <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
	/// <typeparam name="TResult"> The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created the token has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.-or-The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, scheduler, cancellationToken, continuationOptions, ref stackMark);
	}

	private Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction, TaskScheduler scheduler, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, ref StackCrawlMark stackMark)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		CreationOptionsFromContinuationOptions(continuationOptions, out var creationOptions, out var internalOptions);
		Task<TResult> task = new ContinuationResultTaskFromTask<TResult>(this, continuationFunction, null, creationOptions, internalOptions, ref stackMark);
		ContinueWithCore(task, scheduler, cancellationToken, continuationOptions);
		return task;
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation function.</param>
	/// <typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWith<TResult>(Func<Task, object, TResult> continuationFunction, object state)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, state, TaskScheduler.Current, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation function.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWith<TResult>(Func<Task, object, TResult> continuationFunction, object state, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, state, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task" /> completes.  When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation function.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its execution.</param>
	/// <typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWith<TResult>(Func<Task, object, TResult> continuationFunction, object state, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, state, scheduler, default(CancellationToken), TaskContinuationOptions.None, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation function.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWith<TResult>(Func<Task, object, TResult> continuationFunction, object state, TaskContinuationOptions continuationOptions)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, state, TaskScheduler.Current, default(CancellationToken), continuationOptions, ref stackMark);
	}

	/// <summary>Creates a continuation that executes when the target <see cref="T:System.Threading.Tasks.Task" /> completes.</summary>
	/// <returns>A new continuation <see cref="T:System.Threading.Tasks.Task`1" />.</returns>
	/// <param name="continuationFunction">A function to run when the <see cref="T:System.Threading.Tasks.Task" /> completes. When run, the delegate will be  passed the completed task and the caller-supplied state object as arguments.</param>
	/// <param name="state">An object representing data to be used by the continuation function.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> that will be assigned to the new continuation task.</param>
	/// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves. This includes criteria, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled" />, as well as execution options, such as <see cref="F:System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously" />.</param>
	/// <param name="scheduler">The <see cref="T:System.Threading.Tasks.TaskScheduler" /> to associate with the continuation task and to use for its  execution.</param>
	/// <typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuationFunction" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="continuationOptions" /> argument specifies an invalid value for <see cref="T:System.Threading.Tasks.TaskContinuationOptions" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="scheduler" /> argument is null.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The provided <see cref="T:System.Threading.CancellationToken" /> has already been disposed.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public Task<TResult> ContinueWith<TResult>(Func<Task, object, TResult> continuationFunction, object state, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return ContinueWith(continuationFunction, state, scheduler, cancellationToken, continuationOptions, ref stackMark);
	}

	private Task<TResult> ContinueWith<TResult>(Func<Task, object, TResult> continuationFunction, object state, TaskScheduler scheduler, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, ref StackCrawlMark stackMark)
	{
		if (continuationFunction == null)
		{
			throw new ArgumentNullException("continuationFunction");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		CreationOptionsFromContinuationOptions(continuationOptions, out var creationOptions, out var internalOptions);
		Task<TResult> task = new ContinuationResultTaskFromTask<TResult>(this, continuationFunction, state, creationOptions, internalOptions, ref stackMark);
		ContinueWithCore(task, scheduler, cancellationToken, continuationOptions);
		return task;
	}

	internal static void CreationOptionsFromContinuationOptions(TaskContinuationOptions continuationOptions, out TaskCreationOptions creationOptions, out InternalTaskOptions internalOptions)
	{
		TaskContinuationOptions taskContinuationOptions = TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.NotOnRanToCompletion;
		TaskContinuationOptions taskContinuationOptions2 = TaskContinuationOptions.PreferFairness | TaskContinuationOptions.LongRunning | TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.HideScheduler | TaskContinuationOptions.RunContinuationsAsynchronously;
		TaskContinuationOptions taskContinuationOptions3 = TaskContinuationOptions.LongRunning | TaskContinuationOptions.ExecuteSynchronously;
		if ((continuationOptions & taskContinuationOptions3) == taskContinuationOptions3)
		{
			throw new ArgumentOutOfRangeException("continuationOptions", Environment.GetResourceString("The specified TaskContinuationOptions combined LongRunning and ExecuteSynchronously.  Synchronous continuations should not be long running."));
		}
		if ((continuationOptions & ~(taskContinuationOptions2 | taskContinuationOptions | TaskContinuationOptions.LazyCancellation | TaskContinuationOptions.ExecuteSynchronously)) != 0)
		{
			throw new ArgumentOutOfRangeException("continuationOptions");
		}
		if ((continuationOptions & taskContinuationOptions) == taskContinuationOptions)
		{
			throw new ArgumentOutOfRangeException("continuationOptions", Environment.GetResourceString("The specified TaskContinuationOptions excluded all continuation kinds."));
		}
		creationOptions = (TaskCreationOptions)(continuationOptions & taskContinuationOptions2);
		internalOptions = InternalTaskOptions.ContinuationTask;
		if ((continuationOptions & TaskContinuationOptions.LazyCancellation) != 0)
		{
			internalOptions |= InternalTaskOptions.LazyCancellation;
		}
	}

	internal void ContinueWithCore(Task continuationTask, TaskScheduler scheduler, CancellationToken cancellationToken, TaskContinuationOptions options)
	{
		TaskContinuation taskContinuation = new StandardTaskContinuation(continuationTask, options, scheduler);
		if (cancellationToken.CanBeCanceled)
		{
			if (IsCompleted || cancellationToken.IsCancellationRequested)
			{
				continuationTask.AssignCancellationToken(cancellationToken, null, null);
			}
			else
			{
				continuationTask.AssignCancellationToken(cancellationToken, this, taskContinuation);
			}
		}
		if (!continuationTask.IsCompleted && !AddTaskContinuation(taskContinuation, addBeforeOthers: false))
		{
			taskContinuation.Run(this, bCanInlineContinuationTask: true);
		}
	}

	internal void AddCompletionAction(ITaskCompletionAction action)
	{
		AddCompletionAction(action, addBeforeOthers: false);
	}

	private void AddCompletionAction(ITaskCompletionAction action, bool addBeforeOthers)
	{
		if (!AddTaskContinuation(action, addBeforeOthers))
		{
			action.Invoke(this);
		}
	}

	private bool AddTaskContinuationComplex(object tc, bool addBeforeOthers)
	{
		object continuationObject = m_continuationObject;
		if (continuationObject != s_taskCompletionSentinel && !(continuationObject is List<object>))
		{
			List<object> list = new List<object>();
			list.Add(continuationObject);
			Interlocked.CompareExchange(ref m_continuationObject, list, continuationObject);
		}
		if (m_continuationObject is List<object> list2)
		{
			lock (list2)
			{
				if (m_continuationObject != s_taskCompletionSentinel)
				{
					if (list2.Count == list2.Capacity)
					{
						list2.RemoveAll(s_IsTaskContinuationNullPredicate);
					}
					if (addBeforeOthers)
					{
						list2.Insert(0, tc);
					}
					else
					{
						list2.Add(tc);
					}
					return true;
				}
			}
		}
		return false;
	}

	private bool AddTaskContinuation(object tc, bool addBeforeOthers)
	{
		if (IsCompleted)
		{
			return false;
		}
		if (m_continuationObject != null || Interlocked.CompareExchange(ref m_continuationObject, tc, null) != null)
		{
			return AddTaskContinuationComplex(tc, addBeforeOthers);
		}
		return true;
	}

	internal void RemoveContinuation(object continuationObject)
	{
		object continuationObject2 = m_continuationObject;
		if (continuationObject2 == s_taskCompletionSentinel)
		{
			return;
		}
		List<object> list = continuationObject2 as List<object>;
		if (list == null)
		{
			if (Interlocked.CompareExchange(ref m_continuationObject, new List<object>(), continuationObject) == continuationObject)
			{
				return;
			}
			list = m_continuationObject as List<object>;
		}
		if (list == null)
		{
			return;
		}
		lock (list)
		{
			if (m_continuationObject != s_taskCompletionSentinel)
			{
				int num = list.IndexOf(continuationObject);
				if (num != -1)
				{
					list[num] = null;
				}
			}
		}
	}

	/// <summary>Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution.</summary>
	/// <param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
	/// <exception cref="T:System.ObjectDisposedException">One or more of the <see cref="T:System.Threading.Tasks.Task" /> objects in <paramref name="tasks" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is null.-or-The <paramref name="tasks" /> argument contains a null element.</exception>
	/// <exception cref="T:System.AggregateException">At least one of the <see cref="T:System.Threading.Tasks.Task" /> instances was canceled -or- an exception was thrown during the execution of at least one of the <see cref="T:System.Threading.Tasks.Task" /> instances. If a task was canceled, the <see cref="T:System.AggregateException" /> contains an <see cref="T:System.OperationCanceledException" /> in its <see cref="P:System.AggregateException.InnerExceptions" /> collection.</exception>
	[MethodImpl(MethodImplOptions.NoOptimization)]
	public static void WaitAll(params Task[] tasks)
	{
		WaitAll(tasks, -1);
	}

	/// <summary>Waits for all of the provided cancellable <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified time interval.</summary>
	/// <returns>true if all of the <see cref="T:System.Threading.Tasks.Task" /> instances completed execution within the allotted time; otherwise, false.</returns>
	/// <param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
	/// <exception cref="T:System.ObjectDisposedException">One or more of the <see cref="T:System.Threading.Tasks.Task" /> objects in <paramref name="tasks" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is null.</exception>
	/// <exception cref="T:System.AggregateException">At least one of the <see cref="T:System.Threading.Tasks.Task" /> instances was canceled -or- an exception was thrown during the execution of at least one of the <see cref="T:System.Threading.Tasks.Task" /> instances. If a task was canceled, the <see cref="T:System.AggregateException" /> contains an <see cref="T:System.OperationCanceledException" /> in its <see cref="P:System.AggregateException.InnerExceptions" /> collection.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
	[MethodImpl(MethodImplOptions.NoOptimization)]
	public static bool WaitAll(Task[] tasks, TimeSpan timeout)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout");
		}
		return WaitAll(tasks, (int)num);
	}

	/// <summary>Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified number of milliseconds.</summary>
	/// <returns>true if all of the <see cref="T:System.Threading.Tasks.Task" /> instances completed execution within the allotted time; otherwise, false.</returns>
	/// <param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
	/// <exception cref="T:System.ObjectDisposedException">One or more of the <see cref="T:System.Threading.Tasks.Task" /> objects in <paramref name="tasks" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is null.</exception>
	/// <exception cref="T:System.AggregateException">At least one of the <see cref="T:System.Threading.Tasks.Task" /> instances was canceled -or- an exception was thrown during the execution of at least one of the <see cref="T:System.Threading.Tasks.Task" /> instances. If a task was canceled, the <see cref="T:System.AggregateException" /> contains an <see cref="T:System.OperationCanceledException" /> in its <see cref="P:System.AggregateException.InnerExceptions" /> collection.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
	[MethodImpl(MethodImplOptions.NoOptimization)]
	public static bool WaitAll(Task[] tasks, int millisecondsTimeout)
	{
		return WaitAll(tasks, millisecondsTimeout, default(CancellationToken));
	}

	/// <summary>Waits for all of the provided cancellable <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution.</summary>
	/// <param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
	/// <param name="cancellationToken">A <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> to observe while waiting for the tasks to complete.</param>
	/// <exception cref="T:System.OperationCanceledException">The <paramref name="cancellationToken" /> was canceled.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is null.</exception>
	/// <exception cref="T:System.AggregateException">At least one of the <see cref="T:System.Threading.Tasks.Task" /> instances was canceled -or- an exception was thrown during the execution of at least one of the <see cref="T:System.Threading.Tasks.Task" /> instances. If a task was canceled, the <see cref="T:System.AggregateException" /> contains an <see cref="T:System.OperationCanceledException" /> in its <see cref="P:System.AggregateException.InnerExceptions" /> collection.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
	/// <exception cref="T:System.ObjectDisposedException">One or more of the <see cref="T:System.Threading.Tasks.Task" /> objects in <paramref name="tasks" /> has been disposed.</exception>
	[MethodImpl(MethodImplOptions.NoOptimization)]
	public static void WaitAll(Task[] tasks, CancellationToken cancellationToken)
	{
		WaitAll(tasks, -1, cancellationToken);
	}

	/// <summary>Waits for all of the provided cancellable <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified number of milliseconds.</summary>
	/// <returns>true if all of the <see cref="T:System.Threading.Tasks.Task" /> instances completed execution within the allotted time; otherwise, false.</returns>
	/// <param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
	/// <param name="cancellationToken">A <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> to observe while waiting for the tasks to complete.</param>
	/// <exception cref="T:System.ObjectDisposedException">One or more of the <see cref="T:System.Threading.Tasks.Task" /> objects in <paramref name="tasks" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is null.</exception>
	/// <exception cref="T:System.AggregateException">At least one of the <see cref="T:System.Threading.Tasks.Task" /> instances was canceled -or- an exception was thrown during the execution of at least one of the <see cref="T:System.Threading.Tasks.Task" /> instances. If a task was canceled, the <see cref="T:System.AggregateException" /> contains an <see cref="T:System.OperationCanceledException" /> in its <see cref="P:System.AggregateException.InnerExceptions" /> collection.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
	/// <exception cref="T:System.OperationCanceledException">The <paramref name="cancellationToken" /> was canceled.</exception>
	[MethodImpl(MethodImplOptions.NoOptimization)]
	public static bool WaitAll(Task[] tasks, int millisecondsTimeout, CancellationToken cancellationToken)
	{
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		if (millisecondsTimeout < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeout");
		}
		cancellationToken.ThrowIfCancellationRequested();
		List<Exception> exceptions = null;
		List<Task> list = null;
		List<Task> list2 = null;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = true;
		for (int num = tasks.Length - 1; num >= 0; num--)
		{
			Task task = tasks[num];
			if (task == null)
			{
				throw new ArgumentException(Environment.GetResourceString("The tasks array included at least one null element."), "tasks");
			}
			bool flag4 = task.IsCompleted;
			if (!flag4)
			{
				if (millisecondsTimeout != -1 || cancellationToken.CanBeCanceled)
				{
					AddToList(task, ref list, tasks.Length);
				}
				else
				{
					flag4 = task.WrappedTryRunInline() && task.IsCompleted;
					if (!flag4)
					{
						AddToList(task, ref list, tasks.Length);
					}
				}
			}
			if (flag4)
			{
				if (task.IsFaulted)
				{
					flag = true;
				}
				else if (task.IsCanceled)
				{
					flag2 = true;
				}
				if (task.IsWaitNotificationEnabled)
				{
					AddToList(task, ref list2, 1);
				}
			}
		}
		if (list != null)
		{
			flag3 = WaitAllBlockingCore(list, millisecondsTimeout, cancellationToken);
			if (flag3)
			{
				foreach (Task item in list)
				{
					if (item.IsFaulted)
					{
						flag = true;
					}
					else if (item.IsCanceled)
					{
						flag2 = true;
					}
					if (item.IsWaitNotificationEnabled)
					{
						AddToList(item, ref list2, 1);
					}
				}
			}
			GC.KeepAlive(tasks);
		}
		if (flag3 && list2 != null)
		{
			using List<Task>.Enumerator enumerator = list2.GetEnumerator();
			while (enumerator.MoveNext() && !enumerator.Current.NotifyDebuggerOfWaitCompletionIfNecessary())
			{
			}
		}
		if (flag3 && (flag || flag2))
		{
			if (!flag)
			{
				cancellationToken.ThrowIfCancellationRequested();
			}
			foreach (Task t in tasks)
			{
				AddExceptionsForCompletedTask(ref exceptions, t);
			}
			throw new AggregateException(exceptions);
		}
		return flag3;
	}

	private static void AddToList<T>(T item, ref List<T> list, int initSize)
	{
		if (list == null)
		{
			list = new List<T>(initSize);
		}
		list.Add(item);
	}

	private static bool WaitAllBlockingCore(List<Task> tasks, int millisecondsTimeout, CancellationToken cancellationToken)
	{
		bool flag = false;
		SetOnCountdownMres setOnCountdownMres = new SetOnCountdownMres(tasks.Count);
		try
		{
			foreach (Task task in tasks)
			{
				task.AddCompletionAction(setOnCountdownMres, addBeforeOthers: true);
			}
			flag = setOnCountdownMres.Wait(millisecondsTimeout, cancellationToken);
		}
		finally
		{
			if (!flag)
			{
				foreach (Task task2 in tasks)
				{
					if (!task2.IsCompleted)
					{
						task2.RemoveContinuation(setOnCountdownMres);
					}
				}
			}
		}
		return flag;
	}

	internal static void FastWaitAll(Task[] tasks)
	{
		List<Exception> exceptions = null;
		for (int num = tasks.Length - 1; num >= 0; num--)
		{
			if (!tasks[num].IsCompleted)
			{
				tasks[num].WrappedTryRunInline();
			}
		}
		for (int num2 = tasks.Length - 1; num2 >= 0; num2--)
		{
			Task task = tasks[num2];
			task.SpinThenBlockingWait(-1, default(CancellationToken));
			AddExceptionsForCompletedTask(ref exceptions, task);
		}
		if (exceptions != null)
		{
			throw new AggregateException(exceptions);
		}
	}

	internal static void AddExceptionsForCompletedTask(ref List<Exception> exceptions, Task t)
	{
		AggregateException exceptions2 = t.GetExceptions(includeTaskCanceledExceptions: true);
		if (exceptions2 != null)
		{
			t.UpdateExceptionObservedStatus();
			if (exceptions == null)
			{
				exceptions = new List<Exception>(exceptions2.InnerExceptions.Count);
			}
			exceptions.AddRange(exceptions2.InnerExceptions);
		}
	}

	/// <summary>Waits for any of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution.</summary>
	/// <returns>The index of the completed task in the <paramref name="tasks" /> array argument.</returns>
	/// <param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
	[MethodImpl(MethodImplOptions.NoOptimization)]
	public static int WaitAny(params Task[] tasks)
	{
		return WaitAny(tasks, -1);
	}

	/// <summary>Waits for any of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified time interval.</summary>
	/// <returns>The index of the completed task in the <paramref name="tasks" /> array argument, or -1 if the timeout occurred.</returns>
	/// <param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
	[MethodImpl(MethodImplOptions.NoOptimization)]
	public static int WaitAny(Task[] tasks, TimeSpan timeout)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout");
		}
		return WaitAny(tasks, (int)num);
	}

	/// <summary>Waits for any of the provided cancellable <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution.</summary>
	/// <returns>The index of the completed task in the <paramref name="tasks" /> array argument.</returns>
	/// <param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
	/// <param name="cancellationToken">A <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> to observe while waiting for a task to complete.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
	/// <exception cref="T:System.OperationCanceledException">The <paramref name="cancellationToken" /> was canceled.</exception>
	[MethodImpl(MethodImplOptions.NoOptimization)]
	public static int WaitAny(Task[] tasks, CancellationToken cancellationToken)
	{
		return WaitAny(tasks, -1, cancellationToken);
	}

	/// <summary>Waits for any of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified number of milliseconds.</summary>
	/// <returns>The index of the completed task in the <paramref name="tasks" /> array argument, or -1 if the timeout occurred.</returns>
	/// <param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
	[MethodImpl(MethodImplOptions.NoOptimization)]
	public static int WaitAny(Task[] tasks, int millisecondsTimeout)
	{
		return WaitAny(tasks, millisecondsTimeout, default(CancellationToken));
	}

	/// <summary>Waits for any of the provided cancellable <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution within a specified number of milliseconds.</summary>
	/// <returns>The index of the completed task in the <paramref name="tasks" /> array argument, or -1 if the timeout occurred.</returns>
	/// <param name="tasks">An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.</param>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
	/// <param name="cancellationToken">A <see cref="P:System.Threading.Tasks.TaskFactory.CancellationToken" /> to observe while waiting for a task to complete.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Tasks.Task" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> argument contains a null element.</exception>
	/// <exception cref="T:System.OperationCanceledException">The <paramref name="cancellationToken" /> was canceled.</exception>
	[MethodImpl(MethodImplOptions.NoOptimization)]
	public static int WaitAny(Task[] tasks, int millisecondsTimeout, CancellationToken cancellationToken)
	{
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		if (millisecondsTimeout < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeout");
		}
		cancellationToken.ThrowIfCancellationRequested();
		int num = -1;
		for (int i = 0; i < tasks.Length; i++)
		{
			Task task = tasks[i];
			if (task == null)
			{
				throw new ArgumentException(Environment.GetResourceString("The tasks array included at least one null element."), "tasks");
			}
			if (num == -1 && task.IsCompleted)
			{
				num = i;
			}
		}
		if (num == -1 && tasks.Length != 0)
		{
			Task<Task> task2 = TaskFactory.CommonCWAnyLogic(tasks);
			if (task2.Wait(millisecondsTimeout, cancellationToken))
			{
				num = Array.IndexOf(tasks, task2.Result);
			}
		}
		GC.KeepAlive(tasks);
		return num;
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that's completed successfully with the specified result.</summary>
	/// <returns>The successfully completed task.</returns>
	/// <param name="result">The result to store into the completed task.</param>
	/// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
	public static Task<TResult> FromResult<TResult>(TResult result)
	{
		return new Task<TResult>(result);
	}

	public static Task FromException(Exception exception)
	{
		return FromException<VoidTaskResult>(exception);
	}

	public static Task<TResult> FromException<TResult>(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		Task<TResult> task = new Task<TResult>();
		task.TrySetException(exception);
		return task;
	}

	[FriendAccessAllowed]
	internal static Task FromCancellation(CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			throw new ArgumentOutOfRangeException("cancellationToken");
		}
		return new Task(canceled: true, TaskCreationOptions.None, cancellationToken);
	}

	public static Task FromCanceled(CancellationToken cancellationToken)
	{
		return FromCancellation(cancellationToken);
	}

	[FriendAccessAllowed]
	internal static Task<TResult> FromCancellation<TResult>(CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			throw new ArgumentOutOfRangeException("cancellationToken");
		}
		return new Task<TResult>(canceled: true, default(TResult), TaskCreationOptions.None, cancellationToken);
	}

	public static Task<TResult> FromCanceled<TResult>(CancellationToken cancellationToken)
	{
		return FromCancellation<TResult>(cancellationToken);
	}

	internal static Task<TResult> FromCancellation<TResult>(OperationCanceledException exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		Task<TResult> task = new Task<TResult>();
		task.TrySetCanceled(exception.CancellationToken, exception);
		return task;
	}

	/// <summary>Queues the specified work to run on the ThreadPool and returns a task handle for that work.</summary>
	/// <returns>A task that represents the work queued to execute in the ThreadPool.</returns>
	/// <param name="action">The work to execute asynchronously</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="action" /> parameter was null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static Task Run(Action action)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return InternalStartNew(null, action, null, default(CancellationToken), TaskScheduler.Default, TaskCreationOptions.DenyChildAttach, InternalTaskOptions.None, ref stackMark);
	}

	/// <summary>Queues the specified work to run on the ThreadPool and returns a task handle for that work.</summary>
	/// <returns>A task that represents the work queued to execute in the ThreadPool.</returns>
	/// <param name="action">The work to execute asynchronously</param>
	/// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="action" /> parameter was null.</exception>
	/// <exception cref="T:System.Threading.Tasks.TaskCanceledException">The task has been canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> associated with <paramref name="cancellationToken" /> was disposed.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static Task Run(Action action, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return InternalStartNew(null, action, null, cancellationToken, TaskScheduler.Default, TaskCreationOptions.DenyChildAttach, InternalTaskOptions.None, ref stackMark);
	}

	/// <summary>Queues the specified work to run on the ThreadPool and returns a Task(TResult) handle for that work.</summary>
	/// <returns>A Task(TResult) that represents the work queued to execute in the ThreadPool.</returns>
	/// <param name="function">The work to execute asynchronously</param>
	/// <typeparam name="TResult">The result type of the task.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> parameter was null.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static Task<TResult> Run<TResult>(Func<TResult> function)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return Task<TResult>.StartNew(null, function, default(CancellationToken), TaskCreationOptions.DenyChildAttach, InternalTaskOptions.None, TaskScheduler.Default, ref stackMark);
	}

	/// <summary>Queues the specified work to run on the ThreadPool and returns a Task(TResult) handle for that work.</summary>
	/// <returns>A Task(TResult) that represents the work queued to execute in the ThreadPool.</returns>
	/// <param name="function">The work to execute asynchronously</param>
	/// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
	/// <typeparam name="TResult">The result type of the task.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> parameter was null.</exception>
	/// <exception cref="T:System.Threading.Tasks.TaskCanceledException">The task has been canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> associated with <paramref name="cancellationToken" /> was disposed.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static Task<TResult> Run<TResult>(Func<TResult> function, CancellationToken cancellationToken)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return Task<TResult>.StartNew(null, function, cancellationToken, TaskCreationOptions.DenyChildAttach, InternalTaskOptions.None, TaskScheduler.Default, ref stackMark);
	}

	/// <summary>Queues the specified work to run on the ThreadPool and returns a proxy for the  task returned by <paramref name="function" />.</summary>
	/// <returns>A task that represents a proxy for the task returned by <paramref name="function" />.</returns>
	/// <param name="function">The work to execute asynchronously</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> parameter was null.</exception>
	public static Task Run(Func<Task> function)
	{
		return Run(function, default(CancellationToken));
	}

	/// <summary>Queues the specified work to run on the ThreadPool and returns a proxy for the  task returned by <paramref name="function" />.</summary>
	/// <returns>A task that represents a proxy for the task returned by <paramref name="function" />.</returns>
	/// <param name="function">The work to execute asynchronously</param>
	/// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> parameter was null.</exception>
	/// <exception cref="T:System.Threading.Tasks.TaskCanceledException">The task has been canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> associated with <paramref name="cancellationToken" /> was disposed.</exception>
	public static Task Run(Func<Task> function, CancellationToken cancellationToken)
	{
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (AppContextSwitches.ThrowExceptionIfDisposedCancellationTokenSource)
		{
			cancellationToken.ThrowIfSourceDisposed();
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return FromCancellation(cancellationToken);
		}
		return new UnwrapPromise<VoidTaskResult>(Task<Task>.Factory.StartNew(function, cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default), lookForOce: true);
	}

	/// <summary>Queues the specified work to run on the ThreadPool and returns a proxy for the  Task(TResult) returned by <paramref name="function" />.</summary>
	/// <returns>A Task(TResult) that represents a proxy for the Task(TResult) returned by <paramref name="function" />.</returns>
	/// <param name="function">The work to execute asynchronously</param>
	/// <typeparam name="TResult">The type of the result returned by the proxy task.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> parameter was null.</exception>
	public static Task<TResult> Run<TResult>(Func<Task<TResult>> function)
	{
		return Run(function, default(CancellationToken));
	}

	/// <summary>Queues the specified work to run on the ThreadPool and returns a proxy for the  Task(TResult) returned by <paramref name="function" />.</summary>
	/// <returns>A Task(TResult) that represents a proxy for the Task(TResult) returned by <paramref name="function" />.</returns>
	/// <param name="function">The work to execute asynchronously</param>
	/// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
	/// <typeparam name="TResult">The type of the result returned by the proxy task.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="function" /> parameter was null.</exception>
	/// <exception cref="T:System.Threading.Tasks.TaskCanceledException">The task has been canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.CancellationTokenSource" /> associated with <paramref name="cancellationToken" /> was disposed.</exception>
	public static Task<TResult> Run<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken)
	{
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (AppContextSwitches.ThrowExceptionIfDisposedCancellationTokenSource)
		{
			cancellationToken.ThrowIfSourceDisposed();
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return FromCancellation<TResult>(cancellationToken);
		}
		return new UnwrapPromise<TResult>(Task<Task<TResult>>.Factory.StartNew(function, cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default), lookForOce: true);
	}

	/// <summary>Creates a task that will complete after a time delay.</summary>
	/// <returns>A task that represents the time delay</returns>
	/// <param name="delay">The time span to wait before completing the returned task</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="delay" /> represents a negative time interval or is greater than <see cref="F:System.TimeSpan.MaxValue" />.</exception>
	public static Task Delay(TimeSpan delay)
	{
		return Delay(delay, default(CancellationToken));
	}

	/// <summary>Creates a task that will complete after a time delay.</summary>
	/// <returns>A task that represents the time delay</returns>
	/// <param name="delay">The time span to wait before completing the returned task</param>
	/// <param name="cancellationToken">The cancellation token that will be checked prior to completing the returned task</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="delay" /> represents a negative time interval or is greater than <see cref="F:System.TimeSpan.MaxValue" />.</exception>
	/// <exception cref="T:System.Threading.Tasks.TaskCanceledException">The task has been canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The provided <paramref name="cancellationToken" /> has already been disposed.</exception>
	public static Task Delay(TimeSpan delay, CancellationToken cancellationToken)
	{
		long num = (long)delay.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("delay", Environment.GetResourceString("The value needs to translate in milliseconds to -1 (signifying an infinite timeout), 0 or a positive integer less than or equal to Int32.MaxValue."));
		}
		return Delay((int)num, cancellationToken);
	}

	/// <summary>Creates a task that will complete after a time delay.</summary>
	/// <returns>A task that represents the time delay</returns>
	/// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned task</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="millisecondsDelay" /> is less than -1.</exception>
	public static Task Delay(int millisecondsDelay)
	{
		return Delay(millisecondsDelay, default(CancellationToken));
	}

	/// <summary>Creates a task that will complete after a time delay.</summary>
	/// <returns>A task that represents the time delay</returns>
	/// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned task</param>
	/// <param name="cancellationToken">The cancellation token that will be checked prior to completing the returned task</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="millisecondsDelay" /> is less than -1.</exception>
	/// <exception cref="T:System.Threading.Tasks.TaskCanceledException">The task has been canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The provided <paramref name="cancellationToken" /> has already been disposed.</exception>
	public static Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
	{
		if (millisecondsDelay < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsDelay", Environment.GetResourceString("The value needs to be either -1 (signifying an infinite timeout), 0 or a positive integer."));
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return FromCancellation(cancellationToken);
		}
		if (millisecondsDelay == 0)
		{
			return CompletedTask;
		}
		DelayPromise delayPromise = new DelayPromise(cancellationToken);
		if (cancellationToken.CanBeCanceled)
		{
			delayPromise.Registration = cancellationToken.InternalRegisterWithoutEC(delegate(object state)
			{
				((DelayPromise)state).Complete();
			}, delayPromise);
		}
		if (millisecondsDelay != -1)
		{
			delayPromise.Timer = new Timer(delegate(object state)
			{
				((DelayPromise)state).Complete();
			}, delayPromise, millisecondsDelay, -1);
			delayPromise.Timer.KeepRootedWhileScheduled();
		}
		return delayPromise;
	}

	/// <summary>Creates a task that will complete when all of the supplied tasks have completed.</summary>
	/// <returns>A task that represents the completion of all of the supplied tasks.</returns>
	/// <param name="tasks">The tasks to wait on for completion.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> collection contained a null task.</exception>
	public static Task WhenAll(IEnumerable<Task> tasks)
	{
		if (tasks is Task[] tasks2)
		{
			return WhenAll(tasks2);
		}
		if (tasks is ICollection<Task> collection)
		{
			int num = 0;
			Task[] array = new Task[collection.Count];
			foreach (Task task in tasks)
			{
				if (task == null)
				{
					throw new ArgumentException(Environment.GetResourceString("The tasks argument included a null value."), "tasks");
				}
				array[num++] = task;
			}
			return InternalWhenAll(array);
		}
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		List<Task> list = new List<Task>();
		foreach (Task task2 in tasks)
		{
			if (task2 == null)
			{
				throw new ArgumentException(Environment.GetResourceString("The tasks argument included a null value."), "tasks");
			}
			list.Add(task2);
		}
		return InternalWhenAll(list.ToArray());
	}

	/// <summary>Creates a task that will complete when all of the supplied tasks have completed.</summary>
	/// <returns>A task that represents the completion of all of the supplied tasks.</returns>
	/// <param name="tasks">The tasks to wait on for completion.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contained a null task.</exception>
	public static Task WhenAll(params Task[] tasks)
	{
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		int num = tasks.Length;
		if (num == 0)
		{
			return InternalWhenAll(tasks);
		}
		Task[] array = new Task[num];
		for (int i = 0; i < num; i++)
		{
			Task task = tasks[i];
			if (task == null)
			{
				throw new ArgumentException(Environment.GetResourceString("The tasks argument included a null value."), "tasks");
			}
			array[i] = task;
		}
		return InternalWhenAll(array);
	}

	private static Task InternalWhenAll(Task[] tasks)
	{
		if (tasks.Length != 0)
		{
			return new WhenAllPromise(tasks);
		}
		return CompletedTask;
	}

	/// <summary>Creates a task that will complete when all of the supplied tasks have completed.</summary>
	/// <returns>A task that represents the completion of all of the supplied tasks.</returns>
	/// <param name="tasks">The tasks to wait on for completion.</param>
	/// <typeparam name="TResult">The type of the completed task.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> collection contained a null task.</exception>
	public static Task<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks)
	{
		if (tasks is Task<TResult>[] tasks2)
		{
			return WhenAll(tasks2);
		}
		if (tasks is ICollection<Task<TResult>> collection)
		{
			int num = 0;
			Task<TResult>[] array = new Task<TResult>[collection.Count];
			foreach (Task<TResult> task in tasks)
			{
				if (task == null)
				{
					throw new ArgumentException(Environment.GetResourceString("The tasks argument included a null value."), "tasks");
				}
				array[num++] = task;
			}
			return InternalWhenAll(array);
		}
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		List<Task<TResult>> list = new List<Task<TResult>>();
		foreach (Task<TResult> task2 in tasks)
		{
			if (task2 == null)
			{
				throw new ArgumentException(Environment.GetResourceString("The tasks argument included a null value."), "tasks");
			}
			list.Add(task2);
		}
		return InternalWhenAll(list.ToArray());
	}

	/// <summary>Creates a task that will complete when all of the supplied tasks have completed.</summary>
	/// <returns>A task that represents the completion of all of the supplied tasks.</returns>
	/// <param name="tasks">The tasks to wait on for completion.</param>
	/// <typeparam name="TResult">The type of the completed task.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contained a null task.</exception>
	public static Task<TResult[]> WhenAll<TResult>(params Task<TResult>[] tasks)
	{
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		int num = tasks.Length;
		if (num == 0)
		{
			return InternalWhenAll(tasks);
		}
		Task<TResult>[] array = new Task<TResult>[num];
		for (int i = 0; i < num; i++)
		{
			Task<TResult> task = tasks[i];
			if (task == null)
			{
				throw new ArgumentException(Environment.GetResourceString("The tasks argument included a null value."), "tasks");
			}
			array[i] = task;
		}
		return InternalWhenAll(array);
	}

	private static Task<TResult[]> InternalWhenAll<TResult>(Task<TResult>[] tasks)
	{
		if (tasks.Length != 0)
		{
			return new WhenAllPromise<TResult>(tasks);
		}
		return new Task<TResult[]>(canceled: false, new TResult[0], TaskCreationOptions.None, default(CancellationToken));
	}

	/// <summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
	/// <returns>A task that represents the completion of one of the supplied tasks.  The return task's Result is the task that completed.</returns>
	/// <param name="tasks">The tasks to wait on for completion.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contained a null task, or was empty.</exception>
	public static Task<Task> WhenAny(params Task[] tasks)
	{
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		if (tasks.Length == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("The tasks argument contains no tasks."), "tasks");
		}
		int num = tasks.Length;
		Task[] array = new Task[num];
		for (int i = 0; i < num; i++)
		{
			Task task = tasks[i];
			if (task == null)
			{
				throw new ArgumentException(Environment.GetResourceString("The tasks argument included a null value."), "tasks");
			}
			array[i] = task;
		}
		return TaskFactory.CommonCWAnyLogic(array);
	}

	/// <summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
	/// <returns>A task that represents the completion of one of the supplied tasks.  The return task's Result is the task that completed.</returns>
	/// <param name="tasks">The tasks to wait on for completion.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contained a null task, or was empty.</exception>
	public static Task<Task> WhenAny(IEnumerable<Task> tasks)
	{
		if (tasks == null)
		{
			throw new ArgumentNullException("tasks");
		}
		List<Task> list = new List<Task>();
		foreach (Task task in tasks)
		{
			if (task == null)
			{
				throw new ArgumentException(Environment.GetResourceString("The tasks argument included a null value."), "tasks");
			}
			list.Add(task);
		}
		if (list.Count == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("The tasks argument contains no tasks."), "tasks");
		}
		return TaskFactory.CommonCWAnyLogic(list);
	}

	/// <summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
	/// <returns>A task that represents the completion of one of the supplied tasks.  The return task's Result is the task that completed.</returns>
	/// <param name="tasks">The tasks to wait on for completion.</param>
	/// <typeparam name="TResult">The type of the completed task.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contained a null task, or was empty.</exception>
	public static Task<Task<TResult>> WhenAny<TResult>(params Task<TResult>[] tasks)
	{
		return WhenAny((Task[])tasks).ContinueWith(Task<TResult>.TaskWhenAnyCast, default(CancellationToken), TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
	}

	/// <summary>Creates a task that will complete when any of the supplied tasks have completed.</summary>
	/// <returns>A task that represents the completion of one of the supplied tasks.  The return task's Result is the task that completed.</returns>
	/// <param name="tasks">The tasks to wait on for completion.</param>
	/// <typeparam name="TResult">The type of the completed task.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="tasks" /> argument was null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="tasks" /> array contained a null task, or was empty.</exception>
	public static Task<Task<TResult>> WhenAny<TResult>(IEnumerable<Task<TResult>> tasks)
	{
		return WhenAny((IEnumerable<Task>)tasks).ContinueWith(Task<TResult>.TaskWhenAnyCast, default(CancellationToken), TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
	}

	[FriendAccessAllowed]
	internal static Task<TResult> CreateUnwrapPromise<TResult>(Task outerTask, bool lookForOce)
	{
		return new UnwrapPromise<TResult>(outerTask, lookForOce);
	}

	internal virtual Delegate[] GetDelegateContinuationsForDebugger()
	{
		if (m_continuationObject != this)
		{
			return GetDelegatesFromContinuationObject(m_continuationObject);
		}
		return null;
	}

	internal static Delegate[] GetDelegatesFromContinuationObject(object continuationObject)
	{
		if (continuationObject != null)
		{
			if (continuationObject is Action action)
			{
				return new Delegate[1] { AsyncMethodBuilderCore.TryGetStateMachineForDebugger(action) };
			}
			if (continuationObject is TaskContinuation taskContinuation)
			{
				return taskContinuation.GetDelegateContinuationsForDebugger();
			}
			if (continuationObject is Task task)
			{
				Delegate[] delegateContinuationsForDebugger = task.GetDelegateContinuationsForDebugger();
				if (delegateContinuationsForDebugger != null)
				{
					return delegateContinuationsForDebugger;
				}
			}
			if (continuationObject is ITaskCompletionAction taskCompletionAction)
			{
				return new Delegate[1]
				{
					new Action<Task>(taskCompletionAction.Invoke)
				};
			}
			if (continuationObject is List<object> list)
			{
				List<Delegate> list2 = new List<Delegate>();
				foreach (object item in list)
				{
					Delegate[] delegatesFromContinuationObject = GetDelegatesFromContinuationObject(item);
					if (delegatesFromContinuationObject == null)
					{
						continue;
					}
					Delegate[] array = delegatesFromContinuationObject;
					foreach (Delegate @delegate in array)
					{
						if ((object)@delegate != null)
						{
							list2.Add(@delegate);
						}
					}
				}
				return list2.ToArray();
			}
		}
		return null;
	}

	private static Task GetActiveTaskFromId(int taskId)
	{
		Task value = null;
		s_currentActiveTasks.TryGetValue(taskId, out value);
		return value;
	}

	private static Task[] GetActiveTasks()
	{
		return new List<Task>(s_currentActiveTasks.Values).ToArray();
	}
}
