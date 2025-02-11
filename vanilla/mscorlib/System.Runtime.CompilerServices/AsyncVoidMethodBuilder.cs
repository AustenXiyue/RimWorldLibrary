using System.Diagnostics;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

/// <summary>Represents a builder for asynchronous methods that do not return a value.</summary>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public struct AsyncVoidMethodBuilder
{
	private SynchronizationContext m_synchronizationContext;

	private AsyncMethodBuilderCore m_coreState;

	private Task m_task;

	private Task Task
	{
		get
		{
			if (m_task == null)
			{
				m_task = new Task();
			}
			return m_task;
		}
	}

	private object ObjectIdForDebugger => Task;

	/// <summary>Creates an instance of the <see cref="T:System.Runtime.CompilerServices.AsyncVoidMethodBuilder" /> class.</summary>
	/// <returns>A new instance of the builder.</returns>
	public static AsyncVoidMethodBuilder Create()
	{
		SynchronizationContext currentNoFlow = SynchronizationContext.CurrentNoFlow;
		currentNoFlow?.OperationStarted();
		AsyncVoidMethodBuilder result = default(AsyncVoidMethodBuilder);
		result.m_synchronizationContext = currentNoFlow;
		return result;
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
				if (AsyncCausalityTracer.LoggingOn)
				{
					AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Required, Task.Id, "Async: " + stateMachine.GetType().Name, 0uL);
				}
				m_coreState.PostBoxInitialization(stateMachine, runnerToInitialize, null);
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
				if (AsyncCausalityTracer.LoggingOn)
				{
					AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Required, Task.Id, "Async: " + stateMachine.GetType().Name, 0uL);
				}
				m_coreState.PostBoxInitialization(stateMachine, runnerToInitialize, null);
			}
			awaiter.UnsafeOnCompleted(completionAction);
		}
		catch (Exception exception)
		{
			AsyncMethodBuilderCore.ThrowAsync(exception, null);
		}
	}

	/// <summary>Marks the method builder as successfully completed.</summary>
	/// <exception cref="T:System.InvalidOperationException">The builder is not initialized.</exception>
	public void SetResult()
	{
		if (AsyncCausalityTracer.LoggingOn)
		{
			AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, Task.Id, AsyncCausalityStatus.Completed);
		}
		if (m_synchronizationContext != null)
		{
			NotifySynchronizationContextOfCompletion();
		}
	}

	/// <summary>Binds an exception to the method builder.</summary>
	/// <param name="exception">The exception to bind.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="exception" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The builder is not initialized.</exception>
	public void SetException(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		if (AsyncCausalityTracer.LoggingOn)
		{
			AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, Task.Id, AsyncCausalityStatus.Error);
		}
		if (m_synchronizationContext != null)
		{
			try
			{
				AsyncMethodBuilderCore.ThrowAsync(exception, m_synchronizationContext);
				return;
			}
			finally
			{
				NotifySynchronizationContextOfCompletion();
			}
		}
		AsyncMethodBuilderCore.ThrowAsync(exception, null);
	}

	private void NotifySynchronizationContextOfCompletion()
	{
		try
		{
			m_synchronizationContext.OperationCompleted();
		}
		catch (Exception exception)
		{
			AsyncMethodBuilderCore.ThrowAsync(exception, null);
		}
	}
}
