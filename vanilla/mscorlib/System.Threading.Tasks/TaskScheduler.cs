using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;

namespace System.Threading.Tasks;

/// <summary>Represents an object that handles the low-level work of queuing tasks onto threads.</summary>
[DebuggerTypeProxy(typeof(SystemThreadingTasks_TaskSchedulerDebugView))]
[DebuggerDisplay("Id={Id}")]
[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public abstract class TaskScheduler
{
	internal sealed class SystemThreadingTasks_TaskSchedulerDebugView
	{
		private readonly TaskScheduler m_taskScheduler;

		public int Id => m_taskScheduler.Id;

		public IEnumerable<Task> ScheduledTasks
		{
			[SecurityCritical]
			get
			{
				return m_taskScheduler.GetScheduledTasks();
			}
		}

		public SystemThreadingTasks_TaskSchedulerDebugView(TaskScheduler scheduler)
		{
			m_taskScheduler = scheduler;
		}
	}

	private static ConditionalWeakTable<TaskScheduler, object> s_activeTaskSchedulers;

	private static readonly TaskScheduler s_defaultTaskScheduler = new ThreadPoolTaskScheduler();

	internal static int s_taskSchedulerIdCounter;

	private volatile int m_taskSchedulerId;

	private static EventHandler<UnobservedTaskExceptionEventArgs> _unobservedTaskException;

	private static readonly object _unobservedTaskExceptionLockObject = new object();

	/// <summary>Indicates the maximum concurrency level this <see cref="T:System.Threading.Tasks.TaskScheduler" /> is able to support.</summary>
	/// <returns>Returns an integer that represents the maximum concurrency level. The default scheduler returns <see cref="F:System.Int32.MaxValue" />.</returns>
	public virtual int MaximumConcurrencyLevel => int.MaxValue;

	internal virtual bool RequiresAtomicStartTransition => true;

	/// <summary>Gets the default <see cref="T:System.Threading.Tasks.TaskScheduler" /> instance that is provided by the .NET Framework.</summary>
	/// <returns>Returns the default <see cref="T:System.Threading.Tasks.TaskScheduler" /> instance.</returns>
	public static TaskScheduler Default => s_defaultTaskScheduler;

	/// <summary>Gets the <see cref="T:System.Threading.Tasks.TaskScheduler" /> associated with the currently executing task.</summary>
	/// <returns>Returns the <see cref="T:System.Threading.Tasks.TaskScheduler" /> associated with the currently executing task.</returns>
	public static TaskScheduler Current => InternalCurrent ?? Default;

	internal static bool IsDefault => Current == Default;

	internal static TaskScheduler InternalCurrent
	{
		get
		{
			Task internalCurrent = Task.InternalCurrent;
			if (internalCurrent == null || (internalCurrent.CreationOptions & TaskCreationOptions.HideScheduler) != 0)
			{
				return null;
			}
			return internalCurrent.ExecutingTaskScheduler;
		}
	}

	/// <summary>Gets the unique ID for this <see cref="T:System.Threading.Tasks.TaskScheduler" />.</summary>
	/// <returns>Returns the unique ID for this <see cref="T:System.Threading.Tasks.TaskScheduler" />.</returns>
	public int Id
	{
		get
		{
			if (m_taskSchedulerId == 0)
			{
				int num = 0;
				do
				{
					num = Interlocked.Increment(ref s_taskSchedulerIdCounter);
				}
				while (num == 0);
				Interlocked.CompareExchange(ref m_taskSchedulerId, num, 0);
			}
			return m_taskSchedulerId;
		}
	}

	/// <summary>Occurs when a faulted task's unobserved exception is about to trigger exception escalation policy, which, by default, would terminate the process.</summary>
	public static event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException
	{
		[SecurityCritical]
		add
		{
			if (value != null)
			{
				RuntimeHelpers.PrepareContractedDelegate(value);
				lock (_unobservedTaskExceptionLockObject)
				{
					_unobservedTaskException = (EventHandler<UnobservedTaskExceptionEventArgs>)Delegate.Combine(_unobservedTaskException, value);
				}
			}
		}
		[SecurityCritical]
		remove
		{
			lock (_unobservedTaskExceptionLockObject)
			{
				_unobservedTaskException = (EventHandler<UnobservedTaskExceptionEventArgs>)Delegate.Remove(_unobservedTaskException, value);
			}
		}
	}

	/// <summary>Queues a <see cref="T:System.Threading.Tasks.Task" /> to the scheduler. </summary>
	/// <param name="task">The <see cref="T:System.Threading.Tasks.Task" /> to be queued.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="task" /> argument is null.</exception>
	[SecurityCritical]
	protected internal abstract void QueueTask(Task task);

	/// <summary>Determines whether the provided <see cref="T:System.Threading.Tasks.Task" /> can be executed synchronously in this call, and if it can, executes it.</summary>
	/// <returns>A Boolean value indicating whether the task was executed inline.</returns>
	/// <param name="task">The <see cref="T:System.Threading.Tasks.Task" /> to be executed.</param>
	/// <param name="taskWasPreviouslyQueued">A Boolean denoting whether or not task has previously been queued. If this parameter is True, then the task may have been previously queued (scheduled); if False, then the task is known not to have been queued, and this call is being made in order to execute the task inline without queuing it.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="task" /> argument is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="task" /> was already executed.</exception>
	[SecurityCritical]
	protected abstract bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued);

	/// <summary>For debugger support only, generates an enumerable of <see cref="T:System.Threading.Tasks.Task" /> instances currently queued to the scheduler waiting to be executed.</summary>
	/// <returns>An enumerable that allows a debugger to traverse the tasks currently queued to this scheduler.</returns>
	/// <exception cref="T:System.NotSupportedException">This scheduler is unable to generate a list of queued tasks at this time.</exception>
	[SecurityCritical]
	protected abstract IEnumerable<Task> GetScheduledTasks();

	[SecuritySafeCritical]
	internal bool TryRunInline(Task task, bool taskWasPreviouslyQueued)
	{
		TaskScheduler executingTaskScheduler = task.ExecutingTaskScheduler;
		if (executingTaskScheduler != this && executingTaskScheduler != null)
		{
			return executingTaskScheduler.TryRunInline(task, taskWasPreviouslyQueued);
		}
		StackGuard currentStackGuard;
		if (executingTaskScheduler == null || task.m_action == null || task.IsDelegateInvoked || task.IsCanceled || !(currentStackGuard = Task.CurrentStackGuard).TryBeginInliningScope())
		{
			return false;
		}
		bool flag = false;
		try
		{
			task.FireTaskScheduledIfNeeded(this);
			flag = TryExecuteTaskInline(task, taskWasPreviouslyQueued);
		}
		finally
		{
			currentStackGuard.EndInliningScope();
		}
		if (flag && !task.IsDelegateInvoked && !task.IsCanceled)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The TryExecuteTaskInline call to the underlying scheduler succeeded, but the task body was not invoked."));
		}
		return flag;
	}

	/// <summary>Attempts to dequeue a <see cref="T:System.Threading.Tasks.Task" /> that was previously queued to this scheduler.</summary>
	/// <returns>A Boolean denoting whether the <paramref name="task" /> argument was successfully dequeued.</returns>
	/// <param name="task">The <see cref="T:System.Threading.Tasks.Task" /> to be dequeued.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="task" /> argument is null.</exception>
	[SecurityCritical]
	protected internal virtual bool TryDequeue(Task task)
	{
		return false;
	}

	internal virtual void NotifyWorkItemProgress()
	{
	}

	[SecurityCritical]
	internal void InternalQueueTask(Task task)
	{
		task.FireTaskScheduledIfNeeded(this);
		QueueTask(task);
	}

	/// <summary>Initializes the <see cref="T:System.Threading.Tasks.TaskScheduler" />.</summary>
	protected TaskScheduler()
	{
		if (Debugger.IsAttached)
		{
			AddToActiveTaskSchedulers();
		}
	}

	private void AddToActiveTaskSchedulers()
	{
		ConditionalWeakTable<TaskScheduler, object> conditionalWeakTable = s_activeTaskSchedulers;
		if (conditionalWeakTable == null)
		{
			Interlocked.CompareExchange(ref s_activeTaskSchedulers, new ConditionalWeakTable<TaskScheduler, object>(), null);
			conditionalWeakTable = s_activeTaskSchedulers;
		}
		conditionalWeakTable.Add(this, null);
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.TaskScheduler" /> associated with the current <see cref="T:System.Threading.SynchronizationContext" />.</summary>
	/// <returns>A <see cref="T:System.Threading.Tasks.TaskScheduler" /> associated with the current <see cref="T:System.Threading.SynchronizationContext" />, as determined by <see cref="P:System.Threading.SynchronizationContext.Current" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The current SynchronizationContext may not be used as a TaskScheduler.</exception>
	public static TaskScheduler FromCurrentSynchronizationContext()
	{
		return new SynchronizationContextTaskScheduler();
	}

	/// <summary>Attempts to execute the provided <see cref="T:System.Threading.Tasks.Task" /> on this scheduler.</summary>
	/// <returns>A Boolean that is true if <paramref name="task" /> was successfully executed, false if it was not. A common reason for execution failure is that the task had previously been executed or is in the process of being executed by another thread.</returns>
	/// <param name="task">A <see cref="T:System.Threading.Tasks.Task" /> object to be executed.</param>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="task" /> is not associated with this scheduler.</exception>
	[SecurityCritical]
	protected bool TryExecuteTask(Task task)
	{
		if (task.ExecutingTaskScheduler != this)
		{
			throw new InvalidOperationException(Environment.GetResourceString("ExecuteTask may not be called for a task which was previously queued to a different TaskScheduler."));
		}
		return task.ExecuteEntry(bPreventDoubleExecution: true);
	}

	internal static void PublishUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs ueea)
	{
		lock (_unobservedTaskExceptionLockObject)
		{
			_unobservedTaskException?.Invoke(sender, ueea);
		}
	}

	[SecurityCritical]
	internal Task[] GetScheduledTasksForDebugger()
	{
		IEnumerable<Task> scheduledTasks = GetScheduledTasks();
		if (scheduledTasks == null)
		{
			return null;
		}
		Task[] array = scheduledTasks as Task[];
		if (array == null)
		{
			array = new List<Task>(scheduledTasks).ToArray();
		}
		Task[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			_ = array2[i].Id;
		}
		return array;
	}

	[SecurityCritical]
	internal static TaskScheduler[] GetTaskSchedulersForDebugger()
	{
		if (s_activeTaskSchedulers == null)
		{
			return new TaskScheduler[1] { s_defaultTaskScheduler };
		}
		ICollection<TaskScheduler> keys = s_activeTaskSchedulers.Keys;
		if (!keys.Contains(s_defaultTaskScheduler))
		{
			keys.Add(s_defaultTaskScheduler);
		}
		TaskScheduler[] array = new TaskScheduler[keys.Count];
		keys.CopyTo(array, 0);
		TaskScheduler[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			_ = array2[i].Id;
		}
		return array;
	}
}
