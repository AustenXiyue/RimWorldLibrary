using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Threading;

/// <summary>Provides a slimmed down version of <see cref="T:System.Threading.ManualResetEvent" />.</summary>
[DebuggerDisplay("Set = {IsSet}")]
[ComVisible(false)]
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public class ManualResetEventSlim : IDisposable
{
	private const int DEFAULT_SPIN_SP = 1;

	private const int DEFAULT_SPIN_MP = 10;

	private volatile object m_lock;

	private volatile ManualResetEvent m_eventObj;

	private volatile int m_combinedState;

	private const int SignalledState_BitMask = int.MinValue;

	private const int SignalledState_ShiftCount = 31;

	private const int Dispose_BitMask = 1073741824;

	private const int SpinCountState_BitMask = 1073217536;

	private const int SpinCountState_ShiftCount = 19;

	private const int SpinCountState_MaxValue = 2047;

	private const int NumWaitersState_BitMask = 524287;

	private const int NumWaitersState_ShiftCount = 0;

	private const int NumWaitersState_MaxValue = 524287;

	private static Action<object> s_cancellationTokenCallback = CancellationTokenCallback;

	/// <summary>Gets the underlying <see cref="T:System.Threading.WaitHandle" /> object for this <see cref="T:System.Threading.ManualResetEventSlim" />.</summary>
	/// <returns>The underlying <see cref="T:System.Threading.WaitHandle" /> event object fore this <see cref="T:System.Threading.ManualResetEventSlim" />.</returns>
	public WaitHandle WaitHandle
	{
		get
		{
			ThrowIfDisposed();
			if (m_eventObj == null)
			{
				LazyInitializeEvent();
			}
			return m_eventObj;
		}
	}

	/// <summary>Gets whether the event is set.</summary>
	/// <returns>true if the event has is set; otherwise, false.</returns>
	public bool IsSet
	{
		get
		{
			return ExtractStatePortion(m_combinedState, int.MinValue) != 0;
		}
		private set
		{
			UpdateStateAtomically((value ? 1 : 0) << 31, int.MinValue);
		}
	}

	/// <summary>Gets the number of spin waits that will be occur before falling back to a kernel-based wait operation.</summary>
	/// <returns>Returns the number of spin waits that will be occur before falling back to a kernel-based wait operation.</returns>
	public int SpinCount
	{
		get
		{
			return ExtractStatePortionAndShiftRight(m_combinedState, 1073217536, 19);
		}
		private set
		{
			m_combinedState = (m_combinedState & -1073217537) | (value << 19);
		}
	}

	private int Waiters
	{
		get
		{
			return ExtractStatePortionAndShiftRight(m_combinedState, 524287, 0);
		}
		set
		{
			if (value >= 524287)
			{
				throw new InvalidOperationException(string.Format(Environment.GetResourceString("There are too many threads currently waiting on the event. A maximum of {0} waiting threads are supported."), 524287));
			}
			UpdateStateAtomically(value, 524287);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.ManualResetEventSlim" /> class with an initial state of nonsignaled.</summary>
	public ManualResetEventSlim()
		: this(initialState: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.ManualResetEventSlim" /> class with a Boolean value indicating whether to set the intial state to signaled.</summary>
	/// <param name="initialState">true to set the initial state signaled; false to set the initial state to nonsignaled.</param>
	public ManualResetEventSlim(bool initialState)
	{
		Initialize(initialState, 10);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.ManualResetEventSlim" /> class with a Boolean value indicating whether to set the intial state to signaled and a specified spin count.</summary>
	/// <param name="initialState">true to set the initial state to signaled; false to set the initial state to nonsignaled.</param>
	/// <param name="spinCount">The number of spin waits that will occur before falling back to a kernel-based wait operation.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="spinCount" /> is less than 0 or greater than the maximum allowed value.</exception>
	public ManualResetEventSlim(bool initialState, int spinCount)
	{
		if (spinCount < 0)
		{
			throw new ArgumentOutOfRangeException("spinCount");
		}
		if (spinCount > 2047)
		{
			throw new ArgumentOutOfRangeException("spinCount", string.Format(Environment.GetResourceString("The spinCount argument must be in the range 0 to {0}, inclusive."), 2047));
		}
		Initialize(initialState, spinCount);
	}

	private void Initialize(bool initialState, int spinCount)
	{
		m_combinedState = (initialState ? int.MinValue : 0);
		SpinCount = (PlatformHelper.IsSingleProcessor ? 1 : spinCount);
	}

	private void EnsureLockObjectCreated()
	{
		if (m_lock == null)
		{
			object value = new object();
			Interlocked.CompareExchange(ref m_lock, value, null);
		}
	}

	private bool LazyInitializeEvent()
	{
		bool isSet = IsSet;
		ManualResetEvent manualResetEvent = new ManualResetEvent(isSet);
		if (Interlocked.CompareExchange(ref m_eventObj, manualResetEvent, null) != null)
		{
			manualResetEvent.Close();
			return false;
		}
		if (IsSet != isSet)
		{
			lock (manualResetEvent)
			{
				if (m_eventObj == manualResetEvent)
				{
					manualResetEvent.Set();
				}
			}
		}
		return true;
	}

	/// <summary>Sets the state of the event to signaled, which allows one or more threads waiting on the event to proceed.</summary>
	public void Set()
	{
		Set(duringCancellation: false);
	}

	private void Set(bool duringCancellation)
	{
		IsSet = true;
		if (Waiters > 0)
		{
			lock (m_lock)
			{
				Monitor.PulseAll(m_lock);
			}
		}
		ManualResetEvent eventObj = m_eventObj;
		if (eventObj == null || duringCancellation)
		{
			return;
		}
		lock (eventObj)
		{
			if (m_eventObj != null)
			{
				m_eventObj.Set();
			}
		}
	}

	/// <summary>Sets the state of the event to nonsignaled, which causes threads to block.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
	public void Reset()
	{
		ThrowIfDisposed();
		if (m_eventObj != null)
		{
			m_eventObj.Reset();
		}
		IsSet = false;
	}

	/// <summary>Blocks the current thread until the current <see cref="T:System.Threading.ManualResetEventSlim" /> is set.</summary>
	/// <exception cref="T:System.InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
	public void Wait()
	{
		Wait(-1, default(CancellationToken));
	}

	/// <summary>Blocks the current thread until the current <see cref="T:System.Threading.ManualResetEventSlim" /> receives a signal, while observing a <see cref="T:System.Threading.CancellationToken" />.</summary>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
	/// <exception cref="T:System.InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
	/// <exception cref="T:System.OperationCanceledException">
	///   <paramref name="cancellationToken" /> was canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed or the <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has been disposed.</exception>
	public void Wait(CancellationToken cancellationToken)
	{
		Wait(-1, cancellationToken);
	}

	/// <summary>Blocks the current thread until the current <see cref="T:System.Threading.ManualResetEventSlim" /> is set, using a <see cref="T:System.TimeSpan" /> to measure the time interval.</summary>
	/// <returns>true if the <see cref="T:System.Threading.ManualResetEventSlim" /> was set; otherwise, false.</returns>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
	public bool Wait(TimeSpan timeout)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout");
		}
		return Wait((int)num, default(CancellationToken));
	}

	/// <summary>Blocks the current thread until the current <see cref="T:System.Threading.ManualResetEventSlim" /> is set, using a <see cref="T:System.TimeSpan" /> to measure the time interval, while observing a <see cref="T:System.Threading.CancellationToken" />.</summary>
	/// <returns>true if the <see cref="T:System.Threading.ManualResetEventSlim" /> was set; otherwise, false.</returns>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
	/// <exception cref="T:System.OperationCanceledException">
	///   <paramref name="cancellationToken" /> was canceled.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The maximum number of waiters has been exceeded. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed or the <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has been disposed.</exception>
	public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout");
		}
		return Wait((int)num, cancellationToken);
	}

	/// <summary>Blocks the current thread until the current <see cref="T:System.Threading.ManualResetEventSlim" /> is set, using a 32-bit signed integer to measure the time interval.</summary>
	/// <returns>true if the <see cref="T:System.Threading.ManualResetEventSlim" /> was set; otherwise, false.</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" />(-1) to wait indefinitely.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	/// <exception cref="T:System.InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
	public bool Wait(int millisecondsTimeout)
	{
		return Wait(millisecondsTimeout, default(CancellationToken));
	}

	/// <summary>Blocks the current thread until the current <see cref="T:System.Threading.ManualResetEventSlim" /> is set, using a 32-bit signed integer to measure the time interval, while observing a <see cref="T:System.Threading.CancellationToken" />.</summary>
	/// <returns>true if the <see cref="T:System.Threading.ManualResetEventSlim" /> was set; otherwise, false.</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" />(-1) to wait indefinitely.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
	/// <exception cref="T:System.OperationCanceledException">
	///   <paramref name="cancellationToken" /> was canceled.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	/// <exception cref="T:System.InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed or the <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has been disposed.</exception>
	public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
	{
		ThrowIfDisposed();
		cancellationToken.ThrowIfCancellationRequested();
		if (millisecondsTimeout < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeout");
		}
		if (!IsSet)
		{
			if (millisecondsTimeout == 0)
			{
				return false;
			}
			uint startTime = 0u;
			bool flag = false;
			int num = millisecondsTimeout;
			if (millisecondsTimeout != -1)
			{
				startTime = TimeoutHelper.GetTime();
				flag = true;
			}
			int num2 = 10;
			int num3 = 5;
			int num4 = 20;
			int spinCount = SpinCount;
			for (int i = 0; i < spinCount; i++)
			{
				if (IsSet)
				{
					return true;
				}
				if (i < num2)
				{
					if (i == num2 / 2)
					{
						Thread.Yield();
					}
					else
					{
						Thread.SpinWait(PlatformHelper.ProcessorCount * (4 << i));
					}
				}
				else if (i % num4 == 0)
				{
					Thread.Sleep(1);
				}
				else if (i % num3 == 0)
				{
					Thread.Sleep(0);
				}
				else
				{
					Thread.Yield();
				}
				if (i >= 100 && i % 10 == 0)
				{
					cancellationToken.ThrowIfCancellationRequested();
				}
			}
			EnsureLockObjectCreated();
			using (cancellationToken.InternalRegisterWithoutEC(s_cancellationTokenCallback, this))
			{
				lock (m_lock)
				{
					while (!IsSet)
					{
						cancellationToken.ThrowIfCancellationRequested();
						if (flag)
						{
							num = TimeoutHelper.UpdateTimeOut(startTime, millisecondsTimeout);
							if (num <= 0)
							{
								return false;
							}
						}
						Waiters++;
						if (IsSet)
						{
							Waiters--;
							return true;
						}
						try
						{
							if (!Monitor.Wait(m_lock, num))
							{
								return false;
							}
						}
						finally
						{
							Waiters--;
						}
					}
				}
			}
		}
		return true;
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.ManualResetEventSlim" /> class.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Threading.ManualResetEventSlim" />, and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if ((m_combinedState & 0x40000000) != 0)
		{
			return;
		}
		m_combinedState |= 1073741824;
		if (!disposing)
		{
			return;
		}
		ManualResetEvent eventObj = m_eventObj;
		if (eventObj == null)
		{
			return;
		}
		lock (eventObj)
		{
			eventObj.Close();
			m_eventObj = null;
		}
	}

	private void ThrowIfDisposed()
	{
		if ((m_combinedState & 0x40000000) != 0)
		{
			throw new ObjectDisposedException(Environment.GetResourceString("The event has been disposed."));
		}
	}

	private static void CancellationTokenCallback(object obj)
	{
		ManualResetEventSlim manualResetEventSlim = obj as ManualResetEventSlim;
		lock (manualResetEventSlim.m_lock)
		{
			Monitor.PulseAll(manualResetEventSlim.m_lock);
		}
	}

	private void UpdateStateAtomically(int newBits, int updateBitsMask)
	{
		SpinWait spinWait = default(SpinWait);
		while (true)
		{
			int combinedState = m_combinedState;
			int value = (combinedState & ~updateBitsMask) | newBits;
			if (Interlocked.CompareExchange(ref m_combinedState, value, combinedState) == combinedState)
			{
				break;
			}
			spinWait.SpinOnce();
		}
	}

	private static int ExtractStatePortionAndShiftRight(int state, int mask, int rightBitShiftCount)
	{
		return (state & mask) >>> rightBitShiftCount;
	}

	private static int ExtractStatePortion(int state, int mask)
	{
		return state & mask;
	}
}
