using System.Security;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

/// <summary>Provides an awaitable object that enables configured awaits on a task.</summary>
public struct ConfiguredTaskAwaitable
{
	/// <summary>Provides an awaiter for an awaitable (<see cref="T:System.Runtime.CompilerServices.ConfiguredTaskAwaitable" />) object.</summary>
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
	{
		private readonly Task m_task;

		private readonly bool m_continueOnCapturedContext;

		/// <summary>Gets a value that specifies whether the task being awaited is completed.</summary>
		/// <returns>true if the task being awaited is completed; otherwise, false.</returns>
		/// <exception cref="T:System.NullReferenceException">The awaiter was not properly initialized.</exception>
		public bool IsCompleted => m_task.IsCompleted;

		internal ConfiguredTaskAwaiter(Task task, bool continueOnCapturedContext)
		{
			m_task = task;
			m_continueOnCapturedContext = continueOnCapturedContext;
		}

		/// <summary>Schedules the continuation action for the task associated with this awaiter.</summary>
		/// <param name="continuation">The action to invoke when the await operation completes.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuation" /> argument is null.</exception>
		/// <exception cref="T:System.NullReferenceException">The awaiter was not properly initialized.</exception>
		[SecuritySafeCritical]
		public void OnCompleted(Action continuation)
		{
			TaskAwaiter.OnCompletedInternal(m_task, continuation, m_continueOnCapturedContext, flowExecutionContext: true);
		}

		/// <summary>Schedules the continuation action for the task associated with this awaiter. </summary>
		/// <param name="continuation">The action to invoke when the await operation completes.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuation" /> argument is null.</exception>
		/// <exception cref="T:System.NullReferenceException">The awaiter was not properly initialized.</exception>
		[SecurityCritical]
		public void UnsafeOnCompleted(Action continuation)
		{
			TaskAwaiter.OnCompletedInternal(m_task, continuation, m_continueOnCapturedContext, flowExecutionContext: false);
		}

		/// <summary>Ends the await on the completed task.</summary>
		/// <exception cref="T:System.NullReferenceException">The awaiter was not properly initialized.</exception>
		/// <exception cref="T:System.Threading.Tasks.TaskCanceledException">The task was canceled.</exception>
		/// <exception cref="T:System.Exception">The task completed in a faulted state.</exception>
		public void GetResult()
		{
			TaskAwaiter.ValidateEnd(m_task);
		}
	}

	private readonly ConfiguredTaskAwaiter m_configuredTaskAwaiter;

	internal ConfiguredTaskAwaitable(Task task, bool continueOnCapturedContext)
	{
		m_configuredTaskAwaiter = new ConfiguredTaskAwaiter(task, continueOnCapturedContext);
	}

	/// <summary>Returns an awaiter for this awaitable object.</summary>
	/// <returns>The awaiter.</returns>
	public ConfiguredTaskAwaiter GetAwaiter()
	{
		return m_configuredTaskAwaiter;
	}
}
/// <summary>Provides an awaitable object that enables configured awaits on a task.</summary>
/// <typeparam name="TResult">The type of the result produced by this <see cref="T:System.Threading.Tasks.Task`1" />.</typeparam>
public struct ConfiguredTaskAwaitable<TResult>
{
	/// <summary>Provides an awaiter for an awaitable object(<see cref="T:System.Runtime.CompilerServices.ConfiguredTaskAwaitable`1" />).</summary>
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
	{
		private readonly Task<TResult> m_task;

		private readonly bool m_continueOnCapturedContext;

		/// <summary>Gets a value that specifies whether the task being awaited has been completed.</summary>
		/// <returns>true if the task being awaited has been completed; otherwise, false.</returns>
		/// <exception cref="T:System.NullReferenceException">The awaiter was not properly initialized.</exception>
		public bool IsCompleted => m_task.IsCompleted;

		internal ConfiguredTaskAwaiter(Task<TResult> task, bool continueOnCapturedContext)
		{
			m_task = task;
			m_continueOnCapturedContext = continueOnCapturedContext;
		}

		/// <summary>Schedules the continuation action for the task associated with this awaiter.</summary>
		/// <param name="continuation">The action to invoke when the await operation completes.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuation" /> argument is null.</exception>
		/// <exception cref="T:System.NullReferenceException">The awaiter was not properly initialized.</exception>
		[SecuritySafeCritical]
		public void OnCompleted(Action continuation)
		{
			TaskAwaiter.OnCompletedInternal(m_task, continuation, m_continueOnCapturedContext, flowExecutionContext: true);
		}

		/// <summary>Schedules the continuation action for the task associated with this awaiter. </summary>
		/// <param name="continuation">The action to invoke when the await operation completes.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="continuation" /> argument is null.</exception>
		/// <exception cref="T:System.NullReferenceException">The awaiter was not properly initialized.</exception>
		[SecurityCritical]
		public void UnsafeOnCompleted(Action continuation)
		{
			TaskAwaiter.OnCompletedInternal(m_task, continuation, m_continueOnCapturedContext, flowExecutionContext: false);
		}

		/// <summary>Ends the await on the completed task.</summary>
		/// <returns>The result of the completed task.</returns>
		/// <exception cref="T:System.NullReferenceException">The awaiter was not properly initialized.</exception>
		/// <exception cref="T:System.Threading.Tasks.TaskCanceledException">The task was canceled.</exception>
		/// <exception cref="T:System.Exception">The task completed in a faulted state.</exception>
		public TResult GetResult()
		{
			TaskAwaiter.ValidateEnd(m_task);
			return m_task.ResultOnSuccess;
		}
	}

	private readonly ConfiguredTaskAwaiter m_configuredTaskAwaiter;

	internal ConfiguredTaskAwaitable(Task<TResult> task, bool continueOnCapturedContext)
	{
		m_configuredTaskAwaiter = new ConfiguredTaskAwaiter(task, continueOnCapturedContext);
	}

	/// <summary>Returns an awaiter for this awaitable object.</summary>
	/// <returns>The awaiter.</returns>
	public ConfiguredTaskAwaiter GetAwaiter()
	{
		return m_configuredTaskAwaiter;
	}
}
