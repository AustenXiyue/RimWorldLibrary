using System.Diagnostics;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

/// <summary>Represents a builder for asynchronous methods that return a task.</summary>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public struct AsyncTaskMethodBuilder
{
	private static readonly Task<VoidTaskResult> s_cachedCompleted = AsyncTaskMethodBuilder<VoidTaskResult>.s_defaultResultTask;

	private AsyncTaskMethodBuilder<VoidTaskResult> m_builder;

	/// <summary>Gets the task for this builder.</summary>
	/// <returns>The task for this builder.</returns>
	/// <exception cref="T:System.InvalidOperationException">The builder is not initialized.</exception>
	public Task Task => m_builder.Task;

	private object ObjectIdForDebugger => Task;

	/// <summary>Creates an instance of the <see cref="T:System.Runtime.CompilerServices.AsyncTaskMethodBuilder" /> class.</summary>
	/// <returns>A new instance of the builder.</returns>
	public static AsyncTaskMethodBuilder Create()
	{
		return default(AsyncTaskMethodBuilder);
	}

	/// <summary>Begins running the builder with the associated state machine.</summary>
	/// <param name="stateMachine">The state machine instance, passed by reference.</param>
	/// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stateMachine" /> is null.</exception>
	[DebuggerStepThrough]
	[SecuritySafeCritical]
	public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
	{
		if (stateMachine == null)
		{
			throw new ArgumentNullException("stateMachine");
		}
		ExecutionContextSwitcher ecsw = default(ExecutionContextSwitcher);
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			ExecutionContext.EstablishCopyOnWriteScope(ref ecsw);
			stateMachine.MoveNext();
		}
		finally
		{
			ecsw.Undo();
		}
	}

	/// <summary>Associates the builder with the specified state machine.</summary>
	/// <param name="stateMachine">The state machine instance to associate with the builder.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stateMachine" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The state machine was previously set.</exception>
	public void SetStateMachine(IAsyncStateMachine stateMachine)
	{
		m_builder.SetStateMachine(stateMachine);
	}

	/// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes.</summary>
	/// <param name="awaiter">The awaiter.</param>
	/// <param name="stateMachine">The state machine.</param>
	/// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
	/// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
	public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		m_builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
	}

	/// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes. This method can be called from partially trusted code.</summary>
	/// <param name="awaiter">The awaiter.</param>
	/// <param name="stateMachine">The state machine.</param>
	/// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
	/// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
	public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		m_builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
	}

	/// <summary>Marks the task as successfully completed.</summary>
	/// <exception cref="T:System.InvalidOperationException">The task has already completed.-or-The builder is not initialized.</exception>
	public void SetResult()
	{
		m_builder.SetResult(s_cachedCompleted);
	}

	/// <summary>Marks the task as failed and binds the specified exception to the task.</summary>
	/// <param name="exception">The exception to bind to the task.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="exception" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The task has already completed.-or-The builder is not initialized.</exception>
	public void SetException(Exception exception)
	{
		m_builder.SetException(exception);
	}

	internal void SetNotificationForWaitCompletion(bool enabled)
	{
		m_builder.SetNotificationForWaitCompletion(enabled);
	}
}
/// <summary>Represents a builder for asynchronous methods that returns a task and provides a parameter for the result.</summary>
/// <typeparam name="TResult">The result to use to complete the task.</typeparam>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public struct AsyncTaskMethodBuilder<TResult>
{
	internal static readonly Task<TResult> s_defaultResultTask = AsyncTaskCache.CreateCacheableTask(default(TResult));

	private AsyncMethodBuilderCore m_coreState;

	private Task<TResult> m_task;

	/// <summary>Gets the task for this builder.</summary>
	/// <returns>The task for this builder.</returns>
	public Task<TResult> Task
	{
		get
		{
			Task<TResult> task = m_task;
			if (task == null)
			{
				task = (m_task = new Task<TResult>());
			}
			return task;
		}
	}

	private object ObjectIdForDebugger => Task;

	/// <summary>Creates an instance of the <see cref="T:System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1" /> class.</summary>
	/// <returns>A new instance of the builder.</returns>
	public static AsyncTaskMethodBuilder<TResult> Create()
	{
		return default(AsyncTaskMethodBuilder<TResult>);
	}

	/// <summary>Begins running the builder with the associated state machine.</summary>
	/// <param name="stateMachine">The state machine instance, passed by reference.</param>
	/// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stateMachine" /> is null.</exception>
	[SecuritySafeCritical]
	[DebuggerStepThrough]
	public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
	{
		if (stateMachine == null)
		{
			throw new ArgumentNullException("stateMachine");
		}
		ExecutionContextSwitcher ecsw = default(ExecutionContextSwitcher);
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			ExecutionContext.EstablishCopyOnWriteScope(ref ecsw);
			stateMachine.MoveNext();
		}
		finally
		{
			ecsw.Undo();
		}
	}

	/// <summary>Associates the builder with the specified state machine.</summary>
	/// <param name="stateMachine">The state machine instance to associate with the builder.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stateMachine" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The state machine was previously set.</exception>
	public void SetStateMachine(IAsyncStateMachine stateMachine)
	{
		m_coreState.SetStateMachine(stateMachine);
	}

	/// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes.</summary>
	/// <param name="awaiter">The awaiter.</param>
	/// <param name="stateMachine">The state machine.</param>
	/// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
	/// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
	public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		try
		{
			AsyncMethodBuilderCore.MoveNextRunner runnerToInitialize = null;
			Action completionAction = m_coreState.GetCompletionAction(AsyncCausalityTracer.LoggingOn ? Task : null, ref runnerToInitialize);
			if (m_coreState.m_stateMachine == null)
			{
				Task<TResult> task = Task;
				m_coreState.PostBoxInitialization(stateMachine, runnerToInitialize, task);
			}
			awaiter.OnCompleted(completionAction);
		}
		catch (Exception exception)
		{
			AsyncMethodBuilderCore.ThrowAsync(exception, null);
		}
	}

	/// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes. This method can be called from partially trusted code.</summary>
	/// <param name="awaiter">The awaiter.</param>
	/// <param name="stateMachine">The state machine.</param>
	/// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
	/// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
	[SecuritySafeCritical]
	public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		try
		{
			AsyncMethodBuilderCore.MoveNextRunner runnerToInitialize = null;
			Action completionAction = m_coreState.GetCompletionAction(AsyncCausalityTracer.LoggingOn ? Task : null, ref runnerToInitialize);
			if (m_coreState.m_stateMachine == null)
			{
				Task<TResult> task = Task;
				m_coreState.PostBoxInitialization(stateMachine, runnerToInitialize, task);
			}
			awaiter.UnsafeOnCompleted(completionAction);
		}
		catch (Exception exception)
		{
			AsyncMethodBuilderCore.ThrowAsync(exception, null);
		}
	}

	/// <summary>Marks the task as successfully completed.</summary>
	/// <param name="result">The result to use to complete the task.</param>
	/// <exception cref="T:System.InvalidOperationException">The task has already completed.</exception>
	public void SetResult(TResult result)
	{
		Task<TResult> task = m_task;
		if (task == null)
		{
			m_task = GetTaskForResult(result);
			return;
		}
		if (AsyncCausalityTracer.LoggingOn)
		{
			AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, task.Id, AsyncCausalityStatus.Completed);
		}
		if (System.Threading.Tasks.Task.s_asyncDebuggingEnabled)
		{
			System.Threading.Tasks.Task.RemoveFromActiveTasks(task.Id);
		}
		if (task.TrySetResult(result))
		{
			return;
		}
		throw new InvalidOperationException(Environment.GetResourceString("An attempt was made to transition a task to a final state when it had already completed."));
	}

	internal void SetResult(Task<TResult> completedTask)
	{
		if (m_task == null)
		{
			m_task = completedTask;
		}
		else
		{
			SetResult(default(TResult));
		}
	}

	/// <summary>Marks the task as failed and binds the specified exception to the task.</summary>
	/// <param name="exception">The exception to bind to the task.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="exception" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The task has already completed.</exception>
	public void SetException(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		Task<TResult> task = m_task;
		if (task == null)
		{
			task = Task;
		}
		if (!((exception is OperationCanceledException ex) ? task.TrySetCanceled(ex.CancellationToken, ex) : task.TrySetException(exception)))
		{
			throw new InvalidOperationException(Environment.GetResourceString("An attempt was made to transition a task to a final state when it had already completed."));
		}
	}

	internal void SetNotificationForWaitCompletion(bool enabled)
	{
		Task.SetNotificationForWaitCompletion(enabled);
	}

	[SecuritySafeCritical]
	private Task<TResult> GetTaskForResult(TResult result)
	{
		if (default(TResult) != null)
		{
			if (typeof(TResult) == typeof(bool))
			{
				return JitHelpers.UnsafeCast<Task<TResult>>(((bool)(object)result) ? AsyncTaskCache.TrueTask : AsyncTaskCache.FalseTask);
			}
			if (typeof(TResult) == typeof(int))
			{
				int num = (int)(object)result;
				if (num < 9 && num >= -1)
				{
					return JitHelpers.UnsafeCast<Task<TResult>>(AsyncTaskCache.Int32Tasks[num - -1]);
				}
			}
			else if ((typeof(TResult) == typeof(uint) && (uint)(object)result == 0) || (typeof(TResult) == typeof(byte) && (byte)(object)result == 0) || (typeof(TResult) == typeof(sbyte) && (sbyte)(object)result == 0) || (typeof(TResult) == typeof(char) && (char)(object)result == '\0') || (typeof(TResult) == typeof(decimal) && 0m == (decimal)(object)result) || (typeof(TResult) == typeof(long) && (long)(object)result == 0L) || (typeof(TResult) == typeof(ulong) && (ulong)(object)result == 0L) || (typeof(TResult) == typeof(short) && (short)(object)result == 0) || (typeof(TResult) == typeof(ushort) && (ushort)(object)result == 0) || (typeof(TResult) == typeof(IntPtr) && (IntPtr)0 == (IntPtr)(object)result) || (typeof(TResult) == typeof(UIntPtr) && (UIntPtr)0u == (UIntPtr)(object)result))
			{
				return s_defaultResultTask;
			}
		}
		else if (result == null)
		{
			return s_defaultResultTask;
		}
		return new Task<TResult>(result);
	}
}
