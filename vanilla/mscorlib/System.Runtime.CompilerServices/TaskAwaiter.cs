using System.Collections.ObjectModel;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

/// <summary>Provides an object that waits for the completion of an asynchronous task.</summary>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public struct TaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
{
	private readonly Task m_task;

	/// <summary>Gets a value that indicates whether the asynchronous task has completed.</summary>
	/// <returns>true if the task has completed; otherwise, false.</returns>
	/// <exception cref="T:System.NullReferenceException">The <see cref="T:System.Runtime.CompilerServices.TaskAwaiter" /> object was not properly initialized.</exception>
	public bool IsCompleted => m_task.IsCompleted;

	internal TaskAwaiter(Task task)
	{
		m_task = task;
	}

	/// <summary>Sets the action to perform when the <see cref="T:System.Runtime.CompilerServices.TaskAwaiter" /> object stops waiting for the asynchronous task to complete.</summary>
	/// <param name="continuation">The action to perform when the wait operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="continuation" /> is null.</exception>
	/// <exception cref="T:System.NullReferenceException">The <see cref="T:System.Runtime.CompilerServices.TaskAwaiter" /> object was not properly initialized.</exception>
	[SecuritySafeCritical]
	public void OnCompleted(Action continuation)
	{
		OnCompletedInternal(m_task, continuation, continueOnCapturedContext: true, flowExecutionContext: true);
	}

	/// <summary>Schedules the continuation action for the asynchronous task that is associated with this awaiter.</summary>
	/// <param name="continuation">The action to invoke when the await operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="continuation" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The awaiter was not properly initialized.</exception>
	[SecurityCritical]
	public void UnsafeOnCompleted(Action continuation)
	{
		OnCompletedInternal(m_task, continuation, continueOnCapturedContext: true, flowExecutionContext: false);
	}

	/// <summary>Ends the wait for the completion of the asynchronous task.</summary>
	/// <exception cref="T:System.NullReferenceException">The <see cref="T:System.Runtime.CompilerServices.TaskAwaiter" /> object was not properly initialized.</exception>
	/// <exception cref="T:System.Threading.Tasks.TaskCanceledException">The task was canceled.</exception>
	/// <exception cref="T:System.Exception">The task completed in a <see cref="F:System.Threading.Tasks.TaskStatus.Faulted" /> state.</exception>
	public void GetResult()
	{
		ValidateEnd(m_task);
	}

	internal static void ValidateEnd(Task task)
	{
		if (task.IsWaitNotificationEnabledOrNotRanToCompletion)
		{
			HandleNonSuccessAndDebuggerNotification(task);
		}
	}

	private static void HandleNonSuccessAndDebuggerNotification(Task task)
	{
		if (!task.IsCompleted)
		{
			task.InternalWait(-1, default(CancellationToken));
		}
		task.NotifyDebuggerOfWaitCompletionIfNecessary();
		if (!task.IsRanToCompletion)
		{
			ThrowForNonSuccess(task);
		}
	}

	private static void ThrowForNonSuccess(Task task)
	{
		switch (task.Status)
		{
		case TaskStatus.Canceled:
			task.GetCancellationExceptionDispatchInfo()?.Throw();
			throw new TaskCanceledException(task);
		case TaskStatus.Faulted:
		{
			ReadOnlyCollection<ExceptionDispatchInfo> exceptionDispatchInfos = task.GetExceptionDispatchInfos();
			if (exceptionDispatchInfos.Count > 0)
			{
				exceptionDispatchInfos[0].Throw();
				break;
			}
			throw task.Exception;
		}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecurityCritical]
	internal static void OnCompletedInternal(Task task, Action continuation, bool continueOnCapturedContext, bool flowExecutionContext)
	{
		if (continuation == null)
		{
			throw new ArgumentNullException("continuation");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		task.SetContinuationForAwait(continuation, continueOnCapturedContext, flowExecutionContext, ref stackMark);
	}

	private static Action OutputWaitEtwEvents(Task task, Action continuation)
	{
		if (Task.s_asyncDebuggingEnabled)
		{
			Task.AddToActiveTasks(task);
		}
		return AsyncMethodBuilderCore.CreateContinuationWrapper(continuation, delegate
		{
			if (Task.s_asyncDebuggingEnabled)
			{
				Task.RemoveFromActiveTasks(task.Id);
			}
			continuation();
		});
	}
}
/// <summary>Represents an object that waits for the completion of an asynchronous task and provides a parameter for the result.</summary>
/// <typeparam name="TResult">The result for the task.</typeparam>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public struct TaskAwaiter<TResult> : ICriticalNotifyCompletion, INotifyCompletion
{
	private readonly Task<TResult> m_task;

	/// <summary>Gets a value that indicates whether the asynchronous task has completed.</summary>
	/// <returns>true if the task has completed; otherwise, false.</returns>
	/// <exception cref="T:System.NullReferenceException">The <see cref="T:System.Runtime.CompilerServices.TaskAwaiter`1" /> object was not properly initialized.</exception>
	public bool IsCompleted => m_task.IsCompleted;

	internal TaskAwaiter(Task<TResult> task)
	{
		m_task = task;
	}

	/// <summary>Sets the action to perform when the <see cref="T:System.Runtime.CompilerServices.TaskAwaiter`1" /> object stops waiting for the asynchronous task to complete.</summary>
	/// <param name="continuation">The action to perform when the wait operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="continuation" /> is null.</exception>
	/// <exception cref="T:System.NullReferenceException">The <see cref="T:System.Runtime.CompilerServices.TaskAwaiter`1" /> object was not properly initialized.</exception>
	[SecuritySafeCritical]
	public void OnCompleted(Action continuation)
	{
		TaskAwaiter.OnCompletedInternal(m_task, continuation, continueOnCapturedContext: true, flowExecutionContext: true);
	}

	/// <summary>Schedules the continuation action for the asynchronous task associated with this awaiter.</summary>
	/// <param name="continuation">The action to invoke when the await operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="continuation" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The awaiter was not properly initialized.</exception>
	[SecurityCritical]
	public void UnsafeOnCompleted(Action continuation)
	{
		TaskAwaiter.OnCompletedInternal(m_task, continuation, continueOnCapturedContext: true, flowExecutionContext: false);
	}

	/// <summary>Ends the wait for the completion of the asynchronous task.</summary>
	/// <returns>The result of the completed task.</returns>
	/// <exception cref="T:System.NullReferenceException">The <see cref="T:System.Runtime.CompilerServices.TaskAwaiter`1" /> object was not properly initialized.</exception>
	/// <exception cref="T:System.Threading.Tasks.TaskCanceledException">The task was canceled.</exception>
	/// <exception cref="T:System.Exception">The task completed in a <see cref="F:System.Threading.Tasks.TaskStatus.Faulted" /> state.</exception>
	public TResult GetResult()
	{
		TaskAwaiter.ValidateEnd(m_task);
		return m_task.ResultOnSuccess;
	}
}
