using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Threading;

/// <summary>Represents a synchronization primitive that is signaled when its count reaches zero.</summary>
[DebuggerDisplay("Initial Count={InitialCount}, Current Count={CurrentCount}")]
[ComVisible(false)]
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public class CountdownEvent : IDisposable
{
	private int m_initialCount;

	private volatile int m_currentCount;

	private ManualResetEventSlim m_event;

	private volatile bool m_disposed;

	/// <summary>Gets the number of remaining signals required to set the event.</summary>
	/// <returns> The number of remaining signals required to set the event.</returns>
	public int CurrentCount
	{
		get
		{
			int currentCount = m_currentCount;
			if (currentCount >= 0)
			{
				return currentCount;
			}
			return 0;
		}
	}

	/// <summary>Gets the numbers of signals initially required to set the event.</summary>
	/// <returns> The number of signals initially required to set the event.</returns>
	public int InitialCount => m_initialCount;

	/// <summary>Determines whether the event is set.</summary>
	/// <returns>true if the event is set; otherwise, false.</returns>
	public bool IsSet => m_currentCount <= 0;

	/// <summary>Gets a <see cref="T:System.Threading.WaitHandle" /> that is used to wait for the event to be set.</summary>
	/// <returns>A <see cref="T:System.Threading.WaitHandle" /> that is used to wait for the event to be set.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	public WaitHandle WaitHandle
	{
		get
		{
			ThrowIfDisposed();
			return m_event.WaitHandle;
		}
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Threading.CountdownEvent" /> class with the specified count.</summary>
	/// <param name="initialCount">The number of signals initially required to set the <see cref="T:System.Threading.CountdownEvent" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="initialCount" /> is less than 0.</exception>
	public CountdownEvent(int initialCount)
	{
		if (initialCount < 0)
		{
			throw new ArgumentOutOfRangeException("initialCount");
		}
		m_initialCount = initialCount;
		m_currentCount = initialCount;
		m_event = new ManualResetEventSlim();
		if (initialCount == 0)
		{
			m_event.Set();
		}
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.CountdownEvent" /> class.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Threading.CountdownEvent" />, and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			m_event.Dispose();
			m_disposed = true;
		}
	}

	/// <summary>Registers a signal with the <see cref="T:System.Threading.CountdownEvent" />, decrementing the value of <see cref="P:System.Threading.CountdownEvent.CurrentCount" />.</summary>
	/// <returns>true if the signal caused the count to reach zero and the event was set; otherwise, false.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current instance is already set.</exception>
	public bool Signal()
	{
		ThrowIfDisposed();
		if (m_currentCount <= 0)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Invalid attempt made to decrement the event's count below zero."));
		}
		int num = Interlocked.Decrement(ref m_currentCount);
		if (num == 0)
		{
			m_event.Set();
			return true;
		}
		if (num < 0)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Invalid attempt made to decrement the event's count below zero."));
		}
		return false;
	}

	/// <summary>Registers multiple signals with the <see cref="T:System.Threading.CountdownEvent" />, decrementing the value of <see cref="P:System.Threading.CountdownEvent.CurrentCount" /> by the specified amount.</summary>
	/// <returns>true if the signals caused the count to reach zero and the event was set; otherwise, false.</returns>
	/// <param name="signalCount">The number of signals to register.</param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="signalCount" /> is less than 1.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current instance is already set. -or- Or <paramref name="signalCount" /> is greater than <see cref="P:System.Threading.CountdownEvent.CurrentCount" />.</exception>
	public bool Signal(int signalCount)
	{
		if (signalCount <= 0)
		{
			throw new ArgumentOutOfRangeException("signalCount");
		}
		ThrowIfDisposed();
		SpinWait spinWait = default(SpinWait);
		int currentCount;
		while (true)
		{
			currentCount = m_currentCount;
			if (currentCount < signalCount)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Invalid attempt made to decrement the event's count below zero."));
			}
			if (Interlocked.CompareExchange(ref m_currentCount, currentCount - signalCount, currentCount) == currentCount)
			{
				break;
			}
			spinWait.SpinOnce();
		}
		if (currentCount == signalCount)
		{
			m_event.Set();
			return true;
		}
		return false;
	}

	/// <summary>Increments the <see cref="T:System.Threading.CountdownEvent" />'s current count by one.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current instance is already set.-or-<see cref="P:System.Threading.CountdownEvent.CurrentCount" /> is equal to or greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	public void AddCount()
	{
		AddCount(1);
	}

	/// <summary>Attempts to increment <see cref="P:System.Threading.CountdownEvent.CurrentCount" /> by one.</summary>
	/// <returns>true if the increment succeeded; otherwise, false. If <see cref="P:System.Threading.CountdownEvent.CurrentCount" /> is already at zero, this method will return false.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Threading.CountdownEvent.CurrentCount" /> is equal to <see cref="F:System.Int32.MaxValue" />.</exception>
	public bool TryAddCount()
	{
		return TryAddCount(1);
	}

	/// <summary>Increments the <see cref="T:System.Threading.CountdownEvent" />'s current count by a specified value.</summary>
	/// <param name="signalCount">The value by which to increase <see cref="P:System.Threading.CountdownEvent.CurrentCount" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="signalCount" /> is less than or equal to 0.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current instance is already set.-or-<see cref="P:System.Threading.CountdownEvent.CurrentCount" /> is equal to or greater than <see cref="F:System.Int32.MaxValue" /> after count is incremented by <paramref name="signalCount." /></exception>
	public void AddCount(int signalCount)
	{
		if (!TryAddCount(signalCount))
		{
			throw new InvalidOperationException(Environment.GetResourceString("The event is already signaled and cannot be incremented."));
		}
	}

	/// <summary>Attempts to increment <see cref="P:System.Threading.CountdownEvent.CurrentCount" /> by a specified value.</summary>
	/// <returns>true if the increment succeeded; otherwise, false. If <see cref="P:System.Threading.CountdownEvent.CurrentCount" /> is already at zero this will return false.</returns>
	/// <param name="signalCount">The value by which to increase <see cref="P:System.Threading.CountdownEvent.CurrentCount" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="signalCount" /> is less than or equal to 0.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current instance is already set.-or-<see cref="P:System.Threading.CountdownEvent.CurrentCount" /> + <paramref name="signalCount" /> is equal to or greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	public bool TryAddCount(int signalCount)
	{
		if (signalCount <= 0)
		{
			throw new ArgumentOutOfRangeException("signalCount");
		}
		ThrowIfDisposed();
		SpinWait spinWait = default(SpinWait);
		while (true)
		{
			int currentCount = m_currentCount;
			if (currentCount <= 0)
			{
				return false;
			}
			if (currentCount > int.MaxValue - signalCount)
			{
				throw new InvalidOperationException(Environment.GetResourceString("The increment operation would cause the CurrentCount to overflow."));
			}
			if (Interlocked.CompareExchange(ref m_currentCount, currentCount + signalCount, currentCount) == currentCount)
			{
				break;
			}
			spinWait.SpinOnce();
		}
		return true;
	}

	/// <summary>Resets the <see cref="P:System.Threading.CountdownEvent.CurrentCount" /> to the value of <see cref="P:System.Threading.CountdownEvent.InitialCount" />.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed..</exception>
	public void Reset()
	{
		Reset(m_initialCount);
	}

	/// <summary>Resets the <see cref="P:System.Threading.CountdownEvent.InitialCount" /> property to a specified value.</summary>
	/// <param name="count">The number of signals required to set the <see cref="T:System.Threading.CountdownEvent" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has alread been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than 0.</exception>
	public void Reset(int count)
	{
		ThrowIfDisposed();
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		m_currentCount = count;
		m_initialCount = count;
		if (count == 0)
		{
			m_event.Set();
		}
		else
		{
			m_event.Reset();
		}
	}

	/// <summary>Blocks the current thread until the <see cref="T:System.Threading.CountdownEvent" /> is set.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	public void Wait()
	{
		Wait(-1, default(CancellationToken));
	}

	/// <summary>Blocks the current thread until the <see cref="T:System.Threading.CountdownEvent" /> is set, while observing a <see cref="T:System.Threading.CancellationToken" />.</summary>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
	/// <exception cref="T:System.OperationCanceledException">
	///   <paramref name="cancellationToken" /> has been canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed. -or- The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
	public void Wait(CancellationToken cancellationToken)
	{
		Wait(-1, cancellationToken);
	}

	/// <summary>Blocks the current thread until the <see cref="T:System.Threading.CountdownEvent" /> is set, using a <see cref="T:System.TimeSpan" /> to measure the timeout.</summary>
	/// <returns>true if the <see cref="T:System.Threading.CountdownEvent" /> was set; otherwise, false.</returns>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	public bool Wait(TimeSpan timeout)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout");
		}
		return Wait((int)num, default(CancellationToken));
	}

	/// <summary>Blocks the current thread until the <see cref="T:System.Threading.CountdownEvent" /> is set, using a <see cref="T:System.TimeSpan" /> to measure the timeout, while observing a <see cref="T:System.Threading.CancellationToken" />.</summary>
	/// <returns>true if the <see cref="T:System.Threading.CountdownEvent" /> was set; otherwise, false.</returns>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
	/// <exception cref="T:System.OperationCanceledException">
	///   <paramref name="cancellationToken" /> has been canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed. -or- The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout");
		}
		return Wait((int)num, cancellationToken);
	}

	/// <summary>Blocks the current thread until the <see cref="T:System.Threading.CountdownEvent" /> is set, using a 32-bit signed integer to measure the timeout.</summary>
	/// <returns>true if the <see cref="T:System.Threading.CountdownEvent" /> was set; otherwise, false.</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" />(-1) to wait indefinitely.</param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	public bool Wait(int millisecondsTimeout)
	{
		return Wait(millisecondsTimeout, default(CancellationToken));
	}

	/// <summary>Blocks the current thread until the <see cref="T:System.Threading.CountdownEvent" /> is set, using a 32-bit signed integer to measure the timeout, while observing a <see cref="T:System.Threading.CancellationToken" />.</summary>
	/// <returns>true if the <see cref="T:System.Threading.CountdownEvent" /> was set; otherwise, false.</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" />(-1) to wait indefinitely.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
	/// <exception cref="T:System.OperationCanceledException">
	///   <paramref name="cancellationToken" /> has been canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed. -or- The <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
	{
		if (millisecondsTimeout < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeout");
		}
		ThrowIfDisposed();
		cancellationToken.ThrowIfCancellationRequested();
		bool flag = IsSet;
		if (!flag)
		{
			flag = m_event.Wait(millisecondsTimeout, cancellationToken);
		}
		return flag;
	}

	private void ThrowIfDisposed()
	{
		if (m_disposed)
		{
			throw new ObjectDisposedException("CountdownEvent");
		}
	}
}
