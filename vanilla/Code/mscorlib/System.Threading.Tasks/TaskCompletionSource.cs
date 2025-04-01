using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Security.Permissions;

namespace System.Threading.Tasks;

/// <summary>Represents the producer side of a <see cref="T:System.Threading.Tasks.Task`1" /> unbound to a delegate, providing access to the consumer side through the <see cref="P:System.Threading.Tasks.TaskCompletionSource`1.Task" /> property.</summary>
/// <typeparam name="TResult">The type of the result value assocatied with this <see cref="T:System.Threading.Tasks.TaskCompletionSource`1" />.</typeparam>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public class TaskCompletionSource<TResult>
{
	private readonly Task<TResult> m_task;

	/// <summary>Gets the <see cref="T:System.Threading.Tasks.Task`1" /> created by this <see cref="T:System.Threading.Tasks.TaskCompletionSource`1" />.</summary>
	/// <returns>Returns the <see cref="T:System.Threading.Tasks.Task`1" /> created by this <see cref="T:System.Threading.Tasks.TaskCompletionSource`1" />.</returns>
	public Task<TResult> Task => m_task;

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.TaskCompletionSource`1" />.</summary>
	public TaskCompletionSource()
	{
		m_task = new Task<TResult>();
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.TaskCompletionSource`1" /> with the specified options.</summary>
	/// <param name="creationOptions">The options to use when creating the underlying <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> represent options invalid for use with a <see cref="T:System.Threading.Tasks.TaskCompletionSource`1" />.</exception>
	public TaskCompletionSource(TaskCreationOptions creationOptions)
		: this((object)null, creationOptions)
	{
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.TaskCompletionSource`1" /> with the specified state.</summary>
	/// <param name="state">The state to use as the underlying <see cref="T:System.Threading.Tasks.Task`1" />'s AsyncState.</param>
	public TaskCompletionSource(object state)
		: this(state, TaskCreationOptions.None)
	{
	}

	/// <summary>Creates a <see cref="T:System.Threading.Tasks.TaskCompletionSource`1" /> with the specified state and options.</summary>
	/// <param name="state">The state to use as the underlying <see cref="T:System.Threading.Tasks.Task`1" />'s AsyncState.</param>
	/// <param name="creationOptions">The options to use when creating the underlying <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="creationOptions" /> represent options invalid for use with a <see cref="T:System.Threading.Tasks.TaskCompletionSource`1" />.</exception>
	public TaskCompletionSource(object state, TaskCreationOptions creationOptions)
	{
		m_task = new Task<TResult>(state, creationOptions);
	}

	private void SpinUntilCompleted()
	{
		SpinWait spinWait = default(SpinWait);
		while (!m_task.IsCompleted)
		{
			spinWait.SpinOnce();
		}
	}

	/// <summary>Attempts to transition the underlying <see cref="T:System.Threading.Tasks.Task`1" /> into the <see cref="F:System.Threading.Tasks.TaskStatus.Faulted" /> state.</summary>
	/// <returns>True if the operation was successful; otherwise, false.</returns>
	/// <param name="exception">The exception to bind to this <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="P:System.Threading.Tasks.TaskCompletionSource`1.Task" /> was disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="exception" /> argument is null.</exception>
	public bool TrySetException(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		bool num = m_task.TrySetException(exception);
		if (!num && !m_task.IsCompleted)
		{
			SpinUntilCompleted();
		}
		return num;
	}

	/// <summary>Attempts to transition the underlying <see cref="T:System.Threading.Tasks.Task`1" /> into the <see cref="F:System.Threading.Tasks.TaskStatus.Faulted" /> state.</summary>
	/// <returns>True if the operation was successful; otherwise, false.</returns>
	/// <param name="exceptions">The collection of exceptions to bind to this <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="P:System.Threading.Tasks.TaskCompletionSource`1.Task" /> was disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="exceptions" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">There are one or more null elements in <paramref name="exceptions" />.-or-The <paramref name="exceptions" /> collection is empty.</exception>
	public bool TrySetException(IEnumerable<Exception> exceptions)
	{
		if (exceptions == null)
		{
			throw new ArgumentNullException("exceptions");
		}
		List<Exception> list = new List<Exception>();
		foreach (Exception exception in exceptions)
		{
			if (exception == null)
			{
				throw new ArgumentException(Environment.GetResourceString("The exceptions collection included at least one null element."), "exceptions");
			}
			list.Add(exception);
		}
		if (list.Count == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("The exceptions collection was empty."), "exceptions");
		}
		bool num = m_task.TrySetException(list);
		if (!num && !m_task.IsCompleted)
		{
			SpinUntilCompleted();
		}
		return num;
	}

	internal bool TrySetException(IEnumerable<ExceptionDispatchInfo> exceptions)
	{
		bool num = m_task.TrySetException(exceptions);
		if (!num && !m_task.IsCompleted)
		{
			SpinUntilCompleted();
		}
		return num;
	}

	/// <summary>Transitions the underlying <see cref="T:System.Threading.Tasks.Task`1" /> into the <see cref="F:System.Threading.Tasks.TaskStatus.Faulted" /> state.</summary>
	/// <param name="exception">The exception to bind to this <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="P:System.Threading.Tasks.TaskCompletionSource`1.Task" /> was disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="exception" /> argument is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The underlying <see cref="T:System.Threading.Tasks.Task`1" /> is already in one of the three final states: <see cref="F:System.Threading.Tasks.TaskStatus.RanToCompletion" />, <see cref="F:System.Threading.Tasks.TaskStatus.Faulted" />, or <see cref="F:System.Threading.Tasks.TaskStatus.Canceled" />.</exception>
	public void SetException(Exception exception)
	{
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		if (!TrySetException(exception))
		{
			throw new InvalidOperationException(Environment.GetResourceString("An attempt was made to transition a task to a final state when it had already completed."));
		}
	}

	/// <summary>Transitions the underlying <see cref="T:System.Threading.Tasks.Task`1" /> into the <see cref="F:System.Threading.Tasks.TaskStatus.Faulted" /> state.</summary>
	/// <param name="exceptions">The collection of exceptions to bind to this <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="P:System.Threading.Tasks.TaskCompletionSource`1.Task" /> was disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="exceptions" /> argument is null.</exception>
	/// <exception cref="T:System.ArgumentException">There are one or more null elements in <paramref name="exceptions" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The underlying <see cref="T:System.Threading.Tasks.Task`1" /> is already in one of the three final states: <see cref="F:System.Threading.Tasks.TaskStatus.RanToCompletion" />, <see cref="F:System.Threading.Tasks.TaskStatus.Faulted" />, or <see cref="F:System.Threading.Tasks.TaskStatus.Canceled" />.</exception>
	public void SetException(IEnumerable<Exception> exceptions)
	{
		if (!TrySetException(exceptions))
		{
			throw new InvalidOperationException(Environment.GetResourceString("An attempt was made to transition a task to a final state when it had already completed."));
		}
	}

	/// <summary>Attempts to transition the underlying <see cref="T:System.Threading.Tasks.Task`1" /> into the <see cref="F:System.Threading.Tasks.TaskStatus.RanToCompletion" /> state.</summary>
	/// <returns>True if the operation was successful; otherwise, false. </returns>
	/// <param name="result">The result value to bind to this <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	public bool TrySetResult(TResult result)
	{
		bool num = m_task.TrySetResult(result);
		if (!num && !m_task.IsCompleted)
		{
			SpinUntilCompleted();
		}
		return num;
	}

	/// <summary>Transitions the underlying <see cref="T:System.Threading.Tasks.Task`1" /> into the <see cref="F:System.Threading.Tasks.TaskStatus.RanToCompletion" /> state.</summary>
	/// <param name="result">The result value to bind to this <see cref="T:System.Threading.Tasks.Task`1" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="P:System.Threading.Tasks.TaskCompletionSource`1.Task" /> was disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The underlying <see cref="T:System.Threading.Tasks.Task`1" /> is already in one of the three final states: <see cref="F:System.Threading.Tasks.TaskStatus.RanToCompletion" />, <see cref="F:System.Threading.Tasks.TaskStatus.Faulted" />, or <see cref="F:System.Threading.Tasks.TaskStatus.Canceled" />.</exception>
	public void SetResult(TResult result)
	{
		if (!TrySetResult(result))
		{
			throw new InvalidOperationException(Environment.GetResourceString("An attempt was made to transition a task to a final state when it had already completed."));
		}
	}

	/// <summary>Attempts to transition the underlying <see cref="T:System.Threading.Tasks.Task`1" /> into the <see cref="F:System.Threading.Tasks.TaskStatus.Canceled" /> state.</summary>
	/// <returns>True if the operation was successful; false if the operation was unsuccessful or the object has already been disposed.</returns>
	public bool TrySetCanceled()
	{
		return TrySetCanceled(default(CancellationToken));
	}

	public bool TrySetCanceled(CancellationToken cancellationToken)
	{
		bool num = m_task.TrySetCanceled(cancellationToken);
		if (!num && !m_task.IsCompleted)
		{
			SpinUntilCompleted();
		}
		return num;
	}

	/// <summary>Transitions the underlying <see cref="T:System.Threading.Tasks.Task`1" /> into the <see cref="F:System.Threading.Tasks.TaskStatus.Canceled" /> state.</summary>
	/// <exception cref="T:System.InvalidOperationException">The underlying <see cref="T:System.Threading.Tasks.Task`1" /> is already in one of the three final states: <see cref="F:System.Threading.Tasks.TaskStatus.RanToCompletion" />, <see cref="F:System.Threading.Tasks.TaskStatus.Faulted" />, or <see cref="F:System.Threading.Tasks.TaskStatus.Canceled" />, or if the underlying <see cref="T:System.Threading.Tasks.Task`1" /> has already been disposed.</exception>
	public void SetCanceled()
	{
		if (!TrySetCanceled())
		{
			throw new InvalidOperationException(Environment.GetResourceString("An attempt was made to transition a task to a final state when it had already completed."));
		}
	}
}
