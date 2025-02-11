using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace System.Threading;

/// <summary>Enables multiple tasks to cooperatively work on an algorithm in parallel through multiple phases.</summary>
[DebuggerDisplay("Participant Count={ParticipantCount},Participants Remaining={ParticipantsRemaining}")]
[ComVisible(false)]
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public class Barrier : IDisposable
{
	private volatile int m_currentTotalCount;

	private const int CURRENT_MASK = 2147418112;

	private const int TOTAL_MASK = 32767;

	private const int SENSE_MASK = int.MinValue;

	private const int MAX_PARTICIPANTS = 32767;

	private long m_currentPhase;

	private bool m_disposed;

	private ManualResetEventSlim m_oddEvent;

	private ManualResetEventSlim m_evenEvent;

	private ExecutionContext m_ownerThreadContext;

	[SecurityCritical]
	private static ContextCallback s_invokePostPhaseAction;

	private Action<Barrier> m_postPhaseAction;

	private Exception m_exception;

	private int m_actionCallerID;

	/// <summary>Gets the number of participants in the barrier that haven’t yet signaled in the current phase.</summary>
	/// <returns>Returns the number of participants in the barrier that haven’t yet signaled in the current phase.</returns>
	public int ParticipantsRemaining
	{
		get
		{
			int currentTotalCount = m_currentTotalCount;
			int num = currentTotalCount & 0x7FFF;
			int num2 = (currentTotalCount & 0x7FFF0000) >> 16;
			return num - num2;
		}
	}

	/// <summary>Gets the total number of participants in the barrier.</summary>
	/// <returns>Returns the total number of participants in the barrier.</returns>
	public int ParticipantCount => m_currentTotalCount & 0x7FFF;

	/// <summary>Gets the number of the barrier's current phase.</summary>
	/// <returns>Returns the number of the barrier's current phase.</returns>
	public long CurrentPhaseNumber
	{
		get
		{
			return Volatile.Read(ref m_currentPhase);
		}
		internal set
		{
			Volatile.Write(ref m_currentPhase, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Barrier" /> class.</summary>
	/// <param name="participantCount">The number of participating threads.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="participantCount" /> is less than 0 or greater than 32,767.</exception>
	public Barrier(int participantCount)
		: this(participantCount, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Barrier" /> class.</summary>
	/// <param name="participantCount">The number of participating threads.</param>
	/// <param name="postPhaseAction">The <see cref="T:System.Action`1" /> to be executed after each phase. null (Nothing in Visual Basic) may be passed to indicate no action is taken.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="participantCount" /> is less than 0 or greater than 32,767.</exception>
	public Barrier(int participantCount, Action<Barrier> postPhaseAction)
	{
		if (participantCount < 0 || participantCount > 32767)
		{
			throw new ArgumentOutOfRangeException("participantCount", participantCount, global::SR.GetString("The participantCount argument must be non-negative and less than or equal to 32767."));
		}
		m_currentTotalCount = participantCount;
		m_postPhaseAction = postPhaseAction;
		m_oddEvent = new ManualResetEventSlim(initialState: true);
		m_evenEvent = new ManualResetEventSlim(initialState: false);
		if (postPhaseAction != null && !ExecutionContext.IsFlowSuppressed())
		{
			m_ownerThreadContext = ExecutionContext.Capture();
		}
		m_actionCallerID = 0;
	}

	private void GetCurrentTotal(int currentTotal, out int current, out int total, out bool sense)
	{
		total = currentTotal & 0x7FFF;
		current = (currentTotal & 0x7FFF0000) >> 16;
		sense = (currentTotal & int.MinValue) == 0;
	}

	private bool SetCurrentTotal(int currentTotal, int current, int total, bool sense)
	{
		int num = (current << 16) | total;
		if (!sense)
		{
			num |= int.MinValue;
		}
		return Interlocked.CompareExchange(ref m_currentTotalCount, num, currentTotal) == currentTotal;
	}

	/// <summary>Notifies the <see cref="T:System.Threading.Barrier" /> that there will be an additional participant.</summary>
	/// <returns>The phase number of the barrier in which the new participants will first participate.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">Adding a participant would cause the barrier's participant count to exceed 32,767.-or-The method was invoked from within a post-phase action.</exception>
	public long AddParticipant()
	{
		try
		{
			return AddParticipants(1);
		}
		catch (ArgumentOutOfRangeException)
		{
			throw new InvalidOperationException(global::SR.GetString("Adding participantCount participants would result in the number of participants exceeding the maximum number allowed."));
		}
	}

	/// <summary>Notifies the <see cref="T:System.Threading.Barrier" /> that there will be additional participants.</summary>
	/// <returns>The phase number of the barrier in which the new participants will first participate.</returns>
	/// <param name="participantCount">The number of additional participants to add to the barrier.</param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="participantCount" /> is less than 0.-or-Adding <paramref name="participantCount" /> participants would cause the barrier's participant count to exceed 32,767.</exception>
	/// <exception cref="T:System.InvalidOperationException">The method was invoked from within a post-phase action.</exception>
	public long AddParticipants(int participantCount)
	{
		ThrowIfDisposed();
		if (participantCount < 1)
		{
			throw new ArgumentOutOfRangeException("participantCount", participantCount, global::SR.GetString("The participantCount argument must be a positive value."));
		}
		if (participantCount > 32767)
		{
			throw new ArgumentOutOfRangeException("participantCount", global::SR.GetString("Adding participantCount participants would result in the number of participants exceeding the maximum number allowed."));
		}
		if (m_actionCallerID != 0 && Thread.CurrentThread.ManagedThreadId == m_actionCallerID)
		{
			throw new InvalidOperationException(global::SR.GetString("This method may not be called from within the postPhaseAction."));
		}
		SpinWait spinWait = default(SpinWait);
		long num = 0L;
		bool sense;
		while (true)
		{
			int currentTotalCount = m_currentTotalCount;
			GetCurrentTotal(currentTotalCount, out var current, out var total, out sense);
			if (participantCount + total > 32767)
			{
				throw new ArgumentOutOfRangeException("participantCount", global::SR.GetString("Adding participantCount participants would result in the number of participants exceeding the maximum number allowed."));
			}
			if (SetCurrentTotal(currentTotalCount, current, total + participantCount, sense))
			{
				break;
			}
			spinWait.SpinOnce();
		}
		long currentPhaseNumber = CurrentPhaseNumber;
		num = ((sense != (currentPhaseNumber % 2 == 0)) ? (currentPhaseNumber + 1) : currentPhaseNumber);
		if (num != currentPhaseNumber)
		{
			if (sense)
			{
				m_oddEvent.Wait();
			}
			else
			{
				m_evenEvent.Wait();
			}
		}
		else if (sense && m_evenEvent.IsSet)
		{
			m_evenEvent.Reset();
		}
		else if (!sense && m_oddEvent.IsSet)
		{
			m_oddEvent.Reset();
		}
		return num;
	}

	/// <summary>Notifies the <see cref="T:System.Threading.Barrier" /> that there will be one less participant.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The barrier already has 0 participants.-or-The method was invoked from within a post-phase action.</exception>
	public void RemoveParticipant()
	{
		RemoveParticipants(1);
	}

	/// <summary>Notifies the <see cref="T:System.Threading.Barrier" /> that there will be fewer participants.</summary>
	/// <param name="participantCount">The number of additional participants to remove from the barrier.</param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="participantCount" /> is less than 0.</exception>
	/// <exception cref="T:System.InvalidOperationException">The barrier already has 0 participants.-or-The method was invoked from within a post-phase action. -or-current participant count is less than the specified participantCount</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The total participant count is less than the specified<paramref name=" participantCount" /></exception>
	public void RemoveParticipants(int participantCount)
	{
		ThrowIfDisposed();
		if (participantCount < 1)
		{
			throw new ArgumentOutOfRangeException("participantCount", participantCount, global::SR.GetString("The participantCount argument must be a positive value."));
		}
		if (m_actionCallerID != 0 && Thread.CurrentThread.ManagedThreadId == m_actionCallerID)
		{
			throw new InvalidOperationException(global::SR.GetString("This method may not be called from within the postPhaseAction."));
		}
		SpinWait spinWait = default(SpinWait);
		while (true)
		{
			int currentTotalCount = m_currentTotalCount;
			GetCurrentTotal(currentTotalCount, out var current, out var total, out var sense);
			if (total < participantCount)
			{
				throw new ArgumentOutOfRangeException("participantCount", global::SR.GetString("The participantCount argument must be less than or equal the number of participants."));
			}
			if (total - participantCount < current)
			{
				throw new InvalidOperationException(global::SR.GetString("The participantCount argument is greater than the number of participants that haven't yet arrived at the barrier in this phase."));
			}
			int num = total - participantCount;
			if (num > 0 && current == num)
			{
				if (SetCurrentTotal(currentTotalCount, 0, total - participantCount, !sense))
				{
					FinishPhase(sense);
					break;
				}
			}
			else if (SetCurrentTotal(currentTotalCount, current, total - participantCount, sense))
			{
				break;
			}
			spinWait.SpinOnce();
		}
	}

	/// <summary>Signals that a participant has reached the barrier and waits for all other participants to reach the barrier as well.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The method was invoked from within a post-phase action, the barrier currently has 0 participants, or the barrier is signaled by more threads than are registered as participants.</exception>
	/// <exception cref="T:System.Threading.BarrierPostPhaseException">If an exception is thrown from the post phase action of a Barrier after all participating threads have called SignalAndWait, the exception will be wrapped in a BarrierPostPhaseException and be thrown on all participating threads.</exception>
	public void SignalAndWait()
	{
		SignalAndWait(default(CancellationToken));
	}

	/// <summary>Signals that a participant has reached the barrier and waits for all other participants to reach the barrier, while observing a cancellation token.</summary>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
	/// <exception cref="T:System.OperationCanceledException">
	///   <paramref name="cancellationToken" /> has been canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The method was invoked from within a post-phase action, the barrier currently has 0 participants, or the barrier is signaled by more threads than are registered as participants.</exception>
	public void SignalAndWait(CancellationToken cancellationToken)
	{
		SignalAndWait(-1, cancellationToken);
	}

	/// <summary>Signals that a participant has reached the barrier and waits for all other participants to reach the barrier as well, using a <see cref="T:System.TimeSpan" /> object to measure the time interval.</summary>
	/// <returns>true if all other participants reached the barrier; otherwise, false.</returns>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" />is a negative number other than -1 milliseconds, which represents an infinite time-out, or it is greater than 32,767.</exception>
	/// <exception cref="T:System.InvalidOperationException">The method was invoked from within a post-phase action, the barrier currently has 0 participants, or the barrier is signaled by more threads than are registered as participants.</exception>
	public bool SignalAndWait(TimeSpan timeout)
	{
		return SignalAndWait(timeout, default(CancellationToken));
	}

	/// <summary>Signals that a participant has reached the barrier and waits for all other participants to reach the barrier as well, using a <see cref="T:System.TimeSpan" /> object to measure the time interval, while observing a cancellation token.</summary>
	/// <returns>true if all other participants reached the barrier; otherwise, false.</returns>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
	/// <exception cref="T:System.OperationCanceledException">
	///   <paramref name="cancellationToken" /> has been canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" />is a negative number other than -1 milliseconds, which represents an infinite time-out.</exception>
	/// <exception cref="T:System.InvalidOperationException">The method was invoked from within a post-phase action, the barrier currently has 0 participants, or the barrier is signaled by more threads than are registered as participants.</exception>
	public bool SignalAndWait(TimeSpan timeout, CancellationToken cancellationToken)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout", timeout, global::SR.GetString("The specified timeout must represent a value between -1 and Int32.MaxValue, inclusive."));
		}
		return SignalAndWait((int)timeout.TotalMilliseconds, cancellationToken);
	}

	/// <summary>Signals that a participant has reached the barrier and waits for all other participants to reach the barrier as well, using a 32-bit signed integer to measure the timeout.</summary>
	/// <returns>if all participants reached the barrier within the specified time; otherwise false.</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" />(-1) to wait indefinitely.</param>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	/// <exception cref="T:System.InvalidOperationException">The method was invoked from within a post-phase action, the barrier currently has 0 participants, or the barrier is signaled by more threads than are registered as participants.</exception>
	/// <exception cref="T:System.Threading.BarrierPostPhaseException">If an exception is thrown from the post phase action of a Barrier after all participating threads have called SignalAndWait, the exception will be wrapped in a BarrierPostPhaseException and be thrown on all participating threads.</exception>
	public bool SignalAndWait(int millisecondsTimeout)
	{
		return SignalAndWait(millisecondsTimeout, default(CancellationToken));
	}

	/// <summary>Signals that a participant has reached the barrier and waits for all other participants to reach the barrier as well, using a 32-bit signed integer to measure the timeout, while observing a cancellation token.</summary>
	/// <returns>if all participants reached the barrier within the specified time; otherwise false</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" />(-1) to wait indefinitely.</param>
	/// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
	/// <exception cref="T:System.OperationCanceledException">
	///   <paramref name="cancellationToken" /> has been canceled.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
	/// <exception cref="T:System.InvalidOperationException">The method was invoked from within a post-phase action, the barrier currently has 0 participants, or the barrier is signaled by more threads than are registered as participants.</exception>
	public bool SignalAndWait(int millisecondsTimeout, CancellationToken cancellationToken)
	{
		ThrowIfDisposed();
		cancellationToken.ThrowIfCancellationRequested();
		if (millisecondsTimeout < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeout", millisecondsTimeout, global::SR.GetString("The specified timeout must represent a value between -1 and Int32.MaxValue, inclusive."));
		}
		if (m_actionCallerID != 0 && Thread.CurrentThread.ManagedThreadId == m_actionCallerID)
		{
			throw new InvalidOperationException(global::SR.GetString("This method may not be called from within the postPhaseAction."));
		}
		SpinWait spinWait = default(SpinWait);
		int current;
		int total;
		bool sense;
		long currentPhaseNumber;
		while (true)
		{
			int currentTotalCount = m_currentTotalCount;
			GetCurrentTotal(currentTotalCount, out current, out total, out sense);
			currentPhaseNumber = CurrentPhaseNumber;
			if (total == 0)
			{
				throw new InvalidOperationException(global::SR.GetString("The barrier has no registered participants."));
			}
			if (current == 0 && sense != (CurrentPhaseNumber % 2 == 0))
			{
				throw new InvalidOperationException(global::SR.GetString("The number of threads using the barrier exceeded the total number of registered participants."));
			}
			if (current + 1 == total)
			{
				if (SetCurrentTotal(currentTotalCount, 0, total, !sense))
				{
					FinishPhase(sense);
					return true;
				}
			}
			else if (SetCurrentTotal(currentTotalCount, current + 1, total, sense))
			{
				break;
			}
			spinWait.SpinOnce();
		}
		ManualResetEventSlim currentPhaseEvent = (sense ? m_evenEvent : m_oddEvent);
		bool flag = false;
		bool flag2 = false;
		try
		{
			flag2 = DiscontinuousWait(currentPhaseEvent, millisecondsTimeout, cancellationToken, currentPhaseNumber);
		}
		catch (OperationCanceledException)
		{
			flag = true;
		}
		catch (ObjectDisposedException)
		{
			if (currentPhaseNumber >= CurrentPhaseNumber)
			{
				throw;
			}
			flag2 = true;
		}
		if (!flag2)
		{
			spinWait.Reset();
			while (true)
			{
				int currentTotalCount = m_currentTotalCount;
				GetCurrentTotal(currentTotalCount, out current, out total, out var sense2);
				if (currentPhaseNumber < CurrentPhaseNumber || sense != sense2)
				{
					break;
				}
				if (SetCurrentTotal(currentTotalCount, current - 1, total, sense))
				{
					if (flag)
					{
						throw new OperationCanceledException(global::SR.GetString("The operation was canceled."), cancellationToken);
					}
					return false;
				}
				spinWait.SpinOnce();
			}
			WaitCurrentPhase(currentPhaseEvent, currentPhaseNumber);
		}
		if (m_exception != null)
		{
			throw new BarrierPostPhaseException(m_exception);
		}
		return true;
	}

	[SecuritySafeCritical]
	private void FinishPhase(bool observedSense)
	{
		if (m_postPhaseAction != null)
		{
			try
			{
				m_actionCallerID = Thread.CurrentThread.ManagedThreadId;
				if (m_ownerThreadContext != null)
				{
					ExecutionContext ownerThreadContext = m_ownerThreadContext;
					m_ownerThreadContext = m_ownerThreadContext.CreateCopy();
					ContextCallback callback = InvokePostPhaseAction;
					ExecutionContext.Run(ownerThreadContext, callback, this);
					ownerThreadContext.Dispose();
				}
				else
				{
					m_postPhaseAction(this);
				}
				m_exception = null;
				return;
			}
			catch (Exception exception)
			{
				m_exception = exception;
				return;
			}
			finally
			{
				m_actionCallerID = 0;
				SetResetEvents(observedSense);
				if (m_exception != null)
				{
					throw new BarrierPostPhaseException(m_exception);
				}
			}
		}
		SetResetEvents(observedSense);
	}

	[SecurityCritical]
	private static void InvokePostPhaseAction(object obj)
	{
		Barrier barrier = (Barrier)obj;
		barrier.m_postPhaseAction(barrier);
	}

	private void SetResetEvents(bool observedSense)
	{
		CurrentPhaseNumber++;
		if (observedSense)
		{
			m_oddEvent.Reset();
			m_evenEvent.Set();
		}
		else
		{
			m_evenEvent.Reset();
			m_oddEvent.Set();
		}
	}

	private void WaitCurrentPhase(ManualResetEventSlim currentPhaseEvent, long observedPhase)
	{
		SpinWait spinWait = default(SpinWait);
		while (!currentPhaseEvent.IsSet && CurrentPhaseNumber - observedPhase <= 1)
		{
			spinWait.SpinOnce();
		}
	}

	private bool DiscontinuousWait(ManualResetEventSlim currentPhaseEvent, int totalTimeout, CancellationToken token, long observedPhase)
	{
		int num = 100;
		int num2 = 10000;
		while (observedPhase == CurrentPhaseNumber)
		{
			int num3 = ((totalTimeout == -1) ? num : Math.Min(num, totalTimeout));
			if (currentPhaseEvent.Wait(num3, token))
			{
				return true;
			}
			if (totalTimeout != -1)
			{
				totalTimeout -= num3;
				if (totalTimeout <= 0)
				{
					return false;
				}
			}
			num = ((num >= num2) ? num2 : Math.Min(num << 1, num2));
		}
		WaitCurrentPhase(currentPhaseEvent, observedPhase);
		return true;
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.Barrier" /> class.</summary>
	/// <exception cref="T:System.InvalidOperationException">The method was invoked from within a post-phase action.</exception>
	public void Dispose()
	{
		if (m_actionCallerID != 0 && Thread.CurrentThread.ManagedThreadId == m_actionCallerID)
		{
			throw new InvalidOperationException(global::SR.GetString("This method may not be called from within the postPhaseAction."));
		}
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Threading.Barrier" />, and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (m_disposed)
		{
			return;
		}
		if (disposing)
		{
			m_oddEvent.Dispose();
			m_evenEvent.Dispose();
			if (m_ownerThreadContext != null)
			{
				m_ownerThreadContext.Dispose();
				m_ownerThreadContext = null;
			}
		}
		m_disposed = true;
	}

	private void ThrowIfDisposed()
	{
		if (m_disposed)
		{
			throw new ObjectDisposedException("Barrier", global::SR.GetString("The barrier has been disposed."));
		}
	}
}
