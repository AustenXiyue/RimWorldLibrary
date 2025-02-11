using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace System.Threading;

/// <summary>A lightweight alternative to <see cref="T:System.Threading.Semaphore" /> that limits the number of threads that can access a resource or pool of resources concurrently.</summary>
[DebuggerDisplay("Current Count = {m_currentCount}")]
[ComVisible(false)]
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public class SemaphoreSlim : IDisposable
{
	private sealed class TaskNode : Task<bool>, IThreadPoolWorkItem
	{
		internal TaskNode Prev;

		internal TaskNode Next;

		internal TaskNode()
		{
		}

		[SecurityCritical]
		void IThreadPoolWorkItem.ExecuteWorkItem()
		{
			TrySetResult(result: true);
		}

		[SecurityCritical]
		void IThreadPoolWorkItem.MarkAborted(ThreadAbortException tae)
		{
		}
	}

	private volatile int m_currentCount;

	private readonly int m_maxCount;

	private volatile int m_waitCount;

	private object m_lockObj;

	private volatile ManualResetEvent m_waitHandle;

	private TaskNode m_asyncHead;

	private TaskNode m_asyncTail;

	private static readonly Task<bool> s_trueTask = new Task<bool>(canceled: false, result: true, (TaskCreationOptions)16384, default(CancellationToken));

	private const int NO_MAXIMUM = int.MaxValue;

	private static Action<object> s_cancellationTokenCanceledEventHandler = CancellationTokenCanceledEventHandler;

	/// <summary>Gets the number of threads that will be allowed to enter the <see cref="T:System.Threading.SemaphoreSlim" />.</summary>
	/// <returns>The current count of the <see cref="T:System.Threading.SemaphoreSlim" />.</returns>
	public int CurrentCount => m_currentCount;

	/// <summary>Returns a <see cref="T:System.Threading.WaitHandle" /> that can be used to wait on the semaphore.</summary>
	/// <returns>A <see cref="T:System.Threading.WaitHandle" /> that can be used to wait on the semaphore.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.SemaphoreSlim" /> has been disposed.</exception>
	public WaitHandle AvailableWaitHandle
	{
		get
		{
			CheckDispose();
			if (m_waitHandle != null)
			{
				return m_waitHandle;
			}
			lock (m_lockObj)
			{
				if (m_waitHandle == null)
				{
					m_waitHandle = new ManualResetEvent(m_currentCount != 0);
				}
			}
			return m_waitHandle;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.SemaphoreSlim" /> class, specifying the initial number of requests that can be granted concurrently.</summary>
	/// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="initialCount" /> is less than 0.</exception>
	public SemaphoreSlim(int initialCount)
		: this(initialCount, int.MaxValue)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.SemaphoreSlim" /> class, specifying the initial and maximum number of requests that can be granted concurrently.</summary>
	/// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
	/// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="initialCount" /> is less than 0, or <paramref name="initialCount" /> is greater than <paramref name="maxCount" />, or <paramref name="maxCount" /> is equal to or less than 0.</exception>
	public SemaphoreSlim(int initialCount, int maxCount)
	{
		if (initialCount < 0 || initialCount > maxCount)
		{
			throw new ArgumentOutOfRangeException("initialCount", initialCount, GetResourceString("The initialCount argument must be non-negative and less than or equal to the maximumCount."));
		}
		if (maxCount <= 0)
		{
			throw new ArgumentOutOfRangeException("maxCount", maxCount, GetResourceString("The maximumCount argument must be a positive number. If a maximum is not required, use the constructor without a maxCount parameter."));
		}
		m_maxCount = maxCount;
		m_lockObj = new object();
		m_currentCount = initialCount;
	}

	/// <summary>Blocks the current thread until it can enter the <see cref="T:System.Threading.SemaphoreSlim" />.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	public void Wait()
	{
		Wait(-1, default(CancellationToken));
	}

	/// <summary>Blocks the current thread until it can enter the <see cref="T:System.Threading.SemaphoreSlim" />, while observing a <see cref="T:System.Threading.CancellationToken" />.</summary>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> token to observe.</param>
	/// <exception cref="T:System.OperationCanceledException">
	///   <paramref name="cancellationToken" /> was canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.-or-The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	public void Wait(CancellationToken cancellationToken)
	{
		Wait(-1, cancellationToken);
	}

	/// <summary>Blocks the current thread until it can enter the <see cref="T:System.Threading.SemaphoreSlim" />, using a <see cref="T:System.TimeSpan" /> to specify the timeout.</summary>
	/// <returns>true if the current thread successfully entered the <see cref="T:System.Threading.SemaphoreSlim" />; otherwise, false.</returns>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The semaphoreSlim instance has been disposed<paramref name="." /></exception>
	public bool Wait(TimeSpan timeout)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout", timeout, GetResourceString("The timeout must represent a value between -1 and Int32.MaxValue, inclusive."));
		}
		return Wait((int)timeout.TotalMilliseconds, default(CancellationToken));
	}

	/// <summary>Blocks the current thread until it can enter the <see cref="T:System.Threading.SemaphoreSlim" />, using a <see cref="T:System.TimeSpan" /> that specifies the timeout, while observing a <see cref="T:System.Threading.CancellationToken" />.</summary>
	/// <returns>true if the current thread successfully entered the <see cref="T:System.Threading.SemaphoreSlim" />; otherwise, false.</returns>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
	/// <exception cref="T:System.OperationCanceledException">
	///   <paramref name="cancellationToken" /> was canceled.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The semaphoreSlim instance has been disposed<paramref name="." /><paramref name="-or-" />The <see cref="T:System.Threading.CancellationTokenSource" /> that created<paramref name=" cancellationToken" /> has already been disposed.</exception>
	public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout", timeout, GetResourceString("The timeout must represent a value between -1 and Int32.MaxValue, inclusive."));
		}
		return Wait((int)timeout.TotalMilliseconds, cancellationToken);
	}

	/// <summary>Blocks the current thread until it can enter the <see cref="T:System.Threading.SemaphoreSlim" />, using a 32-bit signed integer that specifies the timeout.</summary>
	/// <returns>true if the current thread successfully entered the <see cref="T:System.Threading.SemaphoreSlim" />; otherwise, false.</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" />(-1) to wait indefinitely.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	public bool Wait(int millisecondsTimeout)
	{
		return Wait(millisecondsTimeout, default(CancellationToken));
	}

	/// <summary>Blocks the current thread until it can enter the <see cref="T:System.Threading.SemaphoreSlim" />, using a 32-bit signed integer that specifies the timeout, while observing a <see cref="T:System.Threading.CancellationToken" />.</summary>
	/// <returns>true if the current thread successfully entered the <see cref="T:System.Threading.SemaphoreSlim" />; otherwise, false.</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" />(-1) to wait indefinitely.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
	/// <exception cref="T:System.OperationCanceledException">
	///   <paramref name="cancellationToken" /> was canceled.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.SemaphoreSlim" /> instance has been disposed, or the <see cref="T:System.Threading.CancellationTokenSource" />that created<paramref name=" cancellationToken" /> has been disposed.</exception>
	public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
	{
		CheckDispose();
		if (millisecondsTimeout < -1)
		{
			throw new ArgumentOutOfRangeException("totalMilliSeconds", millisecondsTimeout, GetResourceString("The timeout must represent a value between -1 and Int32.MaxValue, inclusive."));
		}
		cancellationToken.ThrowIfCancellationRequested();
		uint startTime = 0u;
		if (millisecondsTimeout != -1 && millisecondsTimeout > 0)
		{
			startTime = TimeoutHelper.GetTime();
		}
		bool flag = false;
		Task<bool> task = null;
		bool lockTaken = false;
		CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.InternalRegisterWithoutEC(s_cancellationTokenCanceledEventHandler, this);
		try
		{
			SpinWait spinWait = default(SpinWait);
			while (m_currentCount == 0 && !spinWait.NextSpinWillYield)
			{
				spinWait.SpinOnce();
			}
			try
			{
			}
			finally
			{
				Monitor.Enter(m_lockObj, ref lockTaken);
				if (lockTaken)
				{
					m_waitCount++;
				}
			}
			if (m_asyncHead != null)
			{
				task = WaitAsync(millisecondsTimeout, cancellationToken);
			}
			else
			{
				OperationCanceledException ex = null;
				if (m_currentCount == 0)
				{
					if (millisecondsTimeout == 0)
					{
						return false;
					}
					try
					{
						flag = WaitUntilCountOrTimeout(millisecondsTimeout, startTime, cancellationToken);
					}
					catch (OperationCanceledException ex2)
					{
						ex = ex2;
					}
				}
				if (m_currentCount > 0)
				{
					flag = true;
					m_currentCount--;
				}
				else if (ex != null)
				{
					throw ex;
				}
				if (m_waitHandle != null && m_currentCount == 0)
				{
					m_waitHandle.Reset();
				}
			}
		}
		finally
		{
			if (lockTaken)
			{
				m_waitCount--;
				Monitor.Exit(m_lockObj);
			}
			cancellationTokenRegistration.Dispose();
		}
		return task?.GetAwaiter().GetResult() ?? flag;
	}

	private bool WaitUntilCountOrTimeout(int millisecondsTimeout, uint startTime, CancellationToken cancellationToken)
	{
		int num = -1;
		while (m_currentCount == 0)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (millisecondsTimeout != -1)
			{
				num = TimeoutHelper.UpdateTimeOut(startTime, millisecondsTimeout);
				if (num <= 0)
				{
					return false;
				}
			}
			if (!Monitor.Wait(m_lockObj, num))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Asynchronously waits to enter the <see cref="T:System.Threading.SemaphoreSlim" />.</summary>
	/// <returns>A task that will complete when the semaphore has been entered.</returns>
	public Task WaitAsync()
	{
		return WaitAsync(-1, default(CancellationToken));
	}

	/// <summary>Asynchronously waits to enter the <see cref="T:System.Threading.SemaphoreSlim" />, while observing a <see cref="T:System.Threading.CancellationToken" />.</summary>
	/// <returns>A task that will complete when the semaphore has been entered.</returns>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> token to observe.</param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	public Task WaitAsync(CancellationToken cancellationToken)
	{
		return WaitAsync(-1, cancellationToken);
	}

	/// <summary>Asynchronously waits to enter the <see cref="T:System.Threading.SemaphoreSlim" />, using a 32-bit signed integer to measure the time interval.</summary>
	/// <returns>A task that will complete with a result of true if the current thread successfully entered the <see cref="T:System.Threading.SemaphoreSlim" />, otherwise with a result of false.</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	public Task<bool> WaitAsync(int millisecondsTimeout)
	{
		return WaitAsync(millisecondsTimeout, default(CancellationToken));
	}

	/// <summary>Asynchronously waits to enter the <see cref="T:System.Threading.SemaphoreSlim" />, using a <see cref="T:System.TimeSpan" /> to measure the time interval.</summary>
	/// <returns>A task that will complete with a result of true if the current thread successfully entered the <see cref="T:System.Threading.SemaphoreSlim" />, otherwise with a result of false.</returns>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	public Task<bool> WaitAsync(TimeSpan timeout)
	{
		return WaitAsync(timeout, default(CancellationToken));
	}

	/// <summary>Asynchronously waits to enter the <see cref="T:System.Threading.SemaphoreSlim" />, using a <see cref="T:System.TimeSpan" /> to measure the time interval, while observing a <see cref="T:System.Threading.CancellationToken" />.</summary>
	/// <returns>A task that will complete with a result of true if the current thread successfully entered the <see cref="T:System.Threading.SemaphoreSlim" />, otherwise with a result of false.</returns>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> token to observe.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out-or-timeout is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout", timeout, GetResourceString("The timeout must represent a value between -1 and Int32.MaxValue, inclusive."));
		}
		return WaitAsync((int)timeout.TotalMilliseconds, cancellationToken);
	}

	/// <summary>Asynchronously waits to enter the <see cref="T:System.Threading.SemaphoreSlim" />, using a 32-bit signed integer to measure the time interval, while observing a <see cref="T:System.Threading.CancellationToken" />.</summary>
	/// <returns>A task that will complete with a result of true if the current thread successfully entered the <see cref="T:System.Threading.SemaphoreSlim" />, otherwise with a result of false.</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	public Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken)
	{
		CheckDispose();
		if (millisecondsTimeout < -1)
		{
			throw new ArgumentOutOfRangeException("totalMilliSeconds", millisecondsTimeout, GetResourceString("The timeout must represent a value between -1 and Int32.MaxValue, inclusive."));
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCancellation<bool>(cancellationToken);
		}
		lock (m_lockObj)
		{
			if (m_currentCount > 0)
			{
				m_currentCount--;
				if (m_waitHandle != null && m_currentCount == 0)
				{
					m_waitHandle.Reset();
				}
				return s_trueTask;
			}
			TaskNode taskNode = CreateAndAddAsyncWaiter();
			return (millisecondsTimeout == -1 && !cancellationToken.CanBeCanceled) ? taskNode : WaitUntilCountOrTimeoutAsync(taskNode, millisecondsTimeout, cancellationToken);
		}
	}

	private TaskNode CreateAndAddAsyncWaiter()
	{
		TaskNode taskNode = new TaskNode();
		if (m_asyncHead == null)
		{
			m_asyncHead = taskNode;
			m_asyncTail = taskNode;
		}
		else
		{
			m_asyncTail.Next = taskNode;
			taskNode.Prev = m_asyncTail;
			m_asyncTail = taskNode;
		}
		return taskNode;
	}

	private bool RemoveAsyncWaiter(TaskNode task)
	{
		bool result = m_asyncHead == task || task.Prev != null;
		if (task.Next != null)
		{
			task.Next.Prev = task.Prev;
		}
		if (task.Prev != null)
		{
			task.Prev.Next = task.Next;
		}
		if (m_asyncHead == task)
		{
			m_asyncHead = task.Next;
		}
		if (m_asyncTail == task)
		{
			m_asyncTail = task.Prev;
		}
		task.Next = (task.Prev = null);
		return result;
	}

	private async Task<bool> WaitUntilCountOrTimeoutAsync(TaskNode asyncWaiter, int millisecondsTimeout, CancellationToken cancellationToken)
	{
		using (CancellationTokenSource cts = (cancellationToken.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, default(CancellationToken)) : new CancellationTokenSource()))
		{
			if (asyncWaiter == await Task.WhenAny(asyncWaiter, Task.Delay(millisecondsTimeout, cts.Token)).ConfigureAwait(continueOnCapturedContext: false))
			{
				cts.Cancel();
				return true;
			}
		}
		lock (m_lockObj)
		{
			if (RemoveAsyncWaiter(asyncWaiter))
			{
				cancellationToken.ThrowIfCancellationRequested();
				return false;
			}
		}
		return await asyncWaiter.ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>Exits the <see cref="T:System.Threading.SemaphoreSlim" /> once.</summary>
	/// <returns>The previous count of the <see cref="T:System.Threading.SemaphoreSlim" />.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.Threading.SemaphoreFullException">The <see cref="T:System.Threading.SemaphoreSlim" /> has already reached its maximum size.</exception>
	public int Release()
	{
		return Release(1);
	}

	/// <summary>Exits the <see cref="T:System.Threading.SemaphoreSlim" /> a specified number of times.</summary>
	/// <returns>The previous count of the <see cref="T:System.Threading.SemaphoreSlim" />.</returns>
	/// <param name="releaseCount">The number of times to exit the semaphore.</param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="releaseCount" /> is less than 1.</exception>
	/// <exception cref="T:System.Threading.SemaphoreFullException">The <see cref="T:System.Threading.SemaphoreSlim" /> has already reached its maximum size.</exception>
	public int Release(int releaseCount)
	{
		CheckDispose();
		if (releaseCount < 1)
		{
			throw new ArgumentOutOfRangeException("releaseCount", releaseCount, GetResourceString("The releaseCount argument must be greater than zero."));
		}
		int num;
		lock (m_lockObj)
		{
			int currentCount = m_currentCount;
			num = currentCount;
			if (m_maxCount - currentCount < releaseCount)
			{
				throw new SemaphoreFullException();
			}
			currentCount += releaseCount;
			int waitCount = m_waitCount;
			if (currentCount == 1 || waitCount == 1)
			{
				Monitor.Pulse(m_lockObj);
			}
			else if (waitCount > 1)
			{
				Monitor.PulseAll(m_lockObj);
			}
			if (m_asyncHead != null)
			{
				int num2 = currentCount - waitCount;
				while (num2 > 0 && m_asyncHead != null)
				{
					currentCount--;
					num2--;
					TaskNode asyncHead = m_asyncHead;
					RemoveAsyncWaiter(asyncHead);
					QueueWaiterTask(asyncHead);
				}
			}
			m_currentCount = currentCount;
			if (m_waitHandle != null && num == 0 && currentCount > 0)
			{
				m_waitHandle.Set();
			}
		}
		return num;
	}

	[SecuritySafeCritical]
	private static void QueueWaiterTask(TaskNode waiterTask)
	{
		ThreadPool.UnsafeQueueCustomWorkItem(waiterTask, forceGlobal: false);
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.SemaphoreSlim" /> class.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Threading.SemaphoreSlim" />, and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (m_waitHandle != null)
			{
				m_waitHandle.Close();
				m_waitHandle = null;
			}
			m_lockObj = null;
			m_asyncHead = null;
			m_asyncTail = null;
		}
	}

	private static void CancellationTokenCanceledEventHandler(object obj)
	{
		SemaphoreSlim semaphoreSlim = obj as SemaphoreSlim;
		lock (semaphoreSlim.m_lockObj)
		{
			Monitor.PulseAll(semaphoreSlim.m_lockObj);
		}
	}

	private void CheckDispose()
	{
		if (m_lockObj == null)
		{
			throw new ObjectDisposedException(null, GetResourceString("The semaphore has been disposed."));
		}
	}

	private static string GetResourceString(string str)
	{
		return Environment.GetResourceString(str);
	}
}
