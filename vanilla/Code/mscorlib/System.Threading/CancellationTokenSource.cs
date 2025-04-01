using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Threading;

/// <summary>Signals to a <see cref="T:System.Threading.CancellationToken" /> that it should be canceled.</summary>
[ComVisible(false)]
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public class CancellationTokenSource : IDisposable
{
	private static readonly CancellationTokenSource _staticSource_Set = new CancellationTokenSource(set: true);

	private static readonly CancellationTokenSource _staticSource_NotCancelable = new CancellationTokenSource(set: false);

	private static readonly int s_nLists = ((PlatformHelper.ProcessorCount > 24) ? 24 : PlatformHelper.ProcessorCount);

	private volatile ManualResetEvent m_kernelEvent;

	private volatile SparselyPopulatedArray<CancellationCallbackInfo>[] m_registeredCallbacksLists;

	private const int CANNOT_BE_CANCELED = 0;

	private const int NOT_CANCELED = 1;

	private const int NOTIFYING = 2;

	private const int NOTIFYINGCOMPLETE = 3;

	private volatile int m_state;

	private volatile int m_threadIDExecutingCallbacks = -1;

	private bool m_disposed;

	private CancellationTokenRegistration[] m_linkingRegistrations;

	private static readonly Action<object> s_LinkedTokenCancelDelegate = LinkedTokenCancelDelegate;

	private volatile CancellationCallbackInfo m_executingCallback;

	private volatile Timer m_timer;

	private static readonly TimerCallback s_timerCallback = TimerCallbackLogic;

	/// <summary>Gets whether cancellation has been requested for this <see cref="T:System.Threading.CancellationTokenSource" />.</summary>
	/// <returns>Whether cancellation has been requested for this <see cref="T:System.Threading.CancellationTokenSource" />.</returns>
	public bool IsCancellationRequested => m_state >= 2;

	internal bool IsCancellationCompleted => m_state == 3;

	internal bool IsDisposed => m_disposed;

	internal int ThreadIDExecutingCallbacks
	{
		get
		{
			return m_threadIDExecutingCallbacks;
		}
		set
		{
			m_threadIDExecutingCallbacks = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Threading.CancellationToken" /> associated with this <see cref="T:System.Threading.CancellationTokenSource" />.</summary>
	/// <returns>The <see cref="T:System.Threading.CancellationToken" /> associated with this <see cref="T:System.Threading.CancellationTokenSource" />.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The token source has been disposed.</exception>
	public CancellationToken Token
	{
		get
		{
			ThrowIfDisposed();
			return new CancellationToken(this);
		}
	}

	internal bool CanBeCanceled => m_state != 0;

	internal WaitHandle WaitHandle
	{
		get
		{
			ThrowIfDisposed();
			if (m_kernelEvent != null)
			{
				return m_kernelEvent;
			}
			ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
			if (Interlocked.CompareExchange(ref m_kernelEvent, manualResetEvent, null) != null)
			{
				((IDisposable)manualResetEvent).Dispose();
			}
			if (IsCancellationRequested)
			{
				m_kernelEvent.Set();
			}
			return m_kernelEvent;
		}
	}

	internal CancellationCallbackInfo ExecutingCallback => m_executingCallback;

	private static void LinkedTokenCancelDelegate(object source)
	{
		(source as CancellationTokenSource).Cancel();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class.</summary>
	public CancellationTokenSource()
	{
		m_state = 1;
	}

	private CancellationTokenSource(bool set)
	{
		m_state = (set ? 3 : 0);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class that will be canceled after the specified time span.</summary>
	/// <param name="delay">The time span to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="delay" />.<see cref="P:System.TimeSpan.TotalMilliseconds" /> is less than -1 or greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	public CancellationTokenSource(TimeSpan delay)
	{
		long num = (long)delay.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("delay");
		}
		InitializeWithTimer((int)num);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class that will be canceled after the specified delay in milliseconds.</summary>
	/// <param name="millisecondsDelay">The time span to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when <paramref name="millisecondsDelay" /> is less than -1.</exception>
	public CancellationTokenSource(int millisecondsDelay)
	{
		if (millisecondsDelay < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsDelay");
		}
		InitializeWithTimer(millisecondsDelay);
	}

	private void InitializeWithTimer(int millisecondsDelay)
	{
		m_state = 1;
		m_timer = new Timer(s_timerCallback, this, millisecondsDelay, -1);
	}

	/// <summary>Communicates a request for cancellation.</summary>
	/// <exception cref="T:System.ObjectDisposedException">This <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	/// <exception cref="T:System.AggregateException">An aggregate exception containing all the exceptions thrown by the registered callbacks on the associated <see cref="T:System.Threading.CancellationToken" />.</exception>
	public void Cancel()
	{
		Cancel(throwOnFirstException: false);
	}

	/// <summary>Communicates a request for cancellation, and specifies whether remaining callbacks and cancelable operations should be processed.</summary>
	/// <param name="throwOnFirstException">true if exceptions should immediately propagate; otherwise, false.</param>
	/// <exception cref="T:System.ObjectDisposedException">This <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	/// <exception cref="T:System.AggregateException">An aggregate exception containing all the exceptions thrown by the registered callbacks on the associated <see cref="T:System.Threading.CancellationToken" />.</exception>
	public void Cancel(bool throwOnFirstException)
	{
		ThrowIfDisposed();
		NotifyCancellation(throwOnFirstException);
	}

	/// <summary>Schedules a cancel operation on this <see cref="T:System.Threading.CancellationTokenSource" /> after the specified time span.</summary>
	/// <param name="delay">The time span to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The exception thrown when this <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when <paramref name="delay" /> is less than -1 or greater than Int32.MaxValue.</exception>
	public void CancelAfter(TimeSpan delay)
	{
		long num = (long)delay.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("delay");
		}
		CancelAfter((int)num);
	}

	/// <summary>Schedules a cancel operation on this <see cref="T:System.Threading.CancellationTokenSource" /> after the specified number of milliseconds.</summary>
	/// <param name="millisecondsDelay">The time span to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The exception thrown when this <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception thrown when <paramref name="millisecondsDelay" /> is less than -1.</exception>
	public void CancelAfter(int millisecondsDelay)
	{
		ThrowIfDisposed();
		if (millisecondsDelay < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsDelay");
		}
		if (IsCancellationRequested)
		{
			return;
		}
		if (m_timer == null)
		{
			Timer timer = new Timer(s_timerCallback, this, -1, -1);
			if (Interlocked.CompareExchange(ref m_timer, timer, null) != null)
			{
				timer.Dispose();
			}
		}
		try
		{
			m_timer.Change(millisecondsDelay, -1);
		}
		catch (ObjectDisposedException)
		{
		}
	}

	private static void TimerCallbackLogic(object obj)
	{
		CancellationTokenSource cancellationTokenSource = (CancellationTokenSource)obj;
		if (cancellationTokenSource.IsDisposed)
		{
			return;
		}
		try
		{
			cancellationTokenSource.Cancel();
		}
		catch (ObjectDisposedException)
		{
			if (!cancellationTokenSource.IsDisposed)
			{
				throw;
			}
		}
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class.</summary>
	/// <exception cref="T:System.ObjectDisposedException">A linked <see cref="T:System.Threading.CancellationTokenSource" /> has already been disposed.</exception>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Threading.CancellationTokenSource" /> class and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposing || m_disposed)
		{
			return;
		}
		if (m_timer != null)
		{
			m_timer.Dispose();
		}
		CancellationTokenRegistration[] linkingRegistrations = m_linkingRegistrations;
		if (linkingRegistrations != null)
		{
			m_linkingRegistrations = null;
			for (int i = 0; i < linkingRegistrations.Length; i++)
			{
				linkingRegistrations[i].Dispose();
			}
		}
		m_registeredCallbacksLists = null;
		ManualResetEvent kernelEvent = m_kernelEvent;
		if (kernelEvent != null)
		{
			m_kernelEvent = null;
			kernelEvent.Close();
		}
		m_disposed = true;
	}

	internal void ThrowIfDisposed()
	{
		if (m_disposed)
		{
			ThrowObjectDisposedException();
		}
	}

	private static void ThrowObjectDisposedException()
	{
		throw new ObjectDisposedException(null, Environment.GetResourceString("The CancellationTokenSource has been disposed."));
	}

	internal static CancellationTokenSource InternalGetStaticSource(bool set)
	{
		if (!set)
		{
			return _staticSource_NotCancelable;
		}
		return _staticSource_Set;
	}

	internal CancellationTokenRegistration InternalRegister(Action<object> callback, object stateForCallback, SynchronizationContext targetSyncContext, ExecutionContext executionContext)
	{
		if (AppContextSwitches.ThrowExceptionIfDisposedCancellationTokenSource)
		{
			ThrowIfDisposed();
		}
		if (!IsCancellationRequested)
		{
			if (m_disposed && !AppContextSwitches.ThrowExceptionIfDisposedCancellationTokenSource)
			{
				return default(CancellationTokenRegistration);
			}
			int num = Thread.CurrentThread.ManagedThreadId % s_nLists;
			CancellationCallbackInfo cancellationCallbackInfo = new CancellationCallbackInfo(callback, stateForCallback, targetSyncContext, executionContext, this);
			SparselyPopulatedArray<CancellationCallbackInfo>[] array = m_registeredCallbacksLists;
			if (array == null)
			{
				SparselyPopulatedArray<CancellationCallbackInfo>[] array2 = new SparselyPopulatedArray<CancellationCallbackInfo>[s_nLists];
				array = Interlocked.CompareExchange(ref m_registeredCallbacksLists, array2, null);
				if (array == null)
				{
					array = array2;
				}
			}
			SparselyPopulatedArray<CancellationCallbackInfo> sparselyPopulatedArray = Volatile.Read(ref array[num]);
			if (sparselyPopulatedArray == null)
			{
				SparselyPopulatedArray<CancellationCallbackInfo> value = new SparselyPopulatedArray<CancellationCallbackInfo>(4);
				Interlocked.CompareExchange(ref array[num], value, null);
				sparselyPopulatedArray = array[num];
			}
			SparselyPopulatedArrayAddInfo<CancellationCallbackInfo> registrationInfo = sparselyPopulatedArray.Add(cancellationCallbackInfo);
			CancellationTokenRegistration result = new CancellationTokenRegistration(cancellationCallbackInfo, registrationInfo);
			if (!IsCancellationRequested)
			{
				return result;
			}
			if (!result.TryDeregister())
			{
				return result;
			}
		}
		callback(stateForCallback);
		return default(CancellationTokenRegistration);
	}

	private void NotifyCancellation(bool throwOnFirstException)
	{
		if (IsCancellationRequested || Interlocked.CompareExchange(ref m_state, 2, 1) != 1)
		{
			return;
		}
		m_timer?.Dispose();
		ThreadIDExecutingCallbacks = Thread.CurrentThread.ManagedThreadId;
		ManualResetEvent kernelEvent = m_kernelEvent;
		if (kernelEvent != null)
		{
			try
			{
				kernelEvent.Set();
			}
			catch (ObjectDisposedException)
			{
				if (m_kernelEvent != null)
				{
					throw;
				}
			}
		}
		ExecuteCallbackHandlers(throwOnFirstException);
	}

	private void ExecuteCallbackHandlers(bool throwOnFirstException)
	{
		List<Exception> list = null;
		SparselyPopulatedArray<CancellationCallbackInfo>[] registeredCallbacksLists = m_registeredCallbacksLists;
		if (registeredCallbacksLists == null)
		{
			Interlocked.Exchange(ref m_state, 3);
			return;
		}
		try
		{
			for (int i = 0; i < registeredCallbacksLists.Length; i++)
			{
				SparselyPopulatedArray<CancellationCallbackInfo> sparselyPopulatedArray = Volatile.Read(ref registeredCallbacksLists[i]);
				if (sparselyPopulatedArray == null)
				{
					continue;
				}
				for (SparselyPopulatedArrayFragment<CancellationCallbackInfo> sparselyPopulatedArrayFragment = sparselyPopulatedArray.Tail; sparselyPopulatedArrayFragment != null; sparselyPopulatedArrayFragment = sparselyPopulatedArrayFragment.Prev)
				{
					for (int num = sparselyPopulatedArrayFragment.Length - 1; num >= 0; num--)
					{
						m_executingCallback = sparselyPopulatedArrayFragment[num];
						if (m_executingCallback != null)
						{
							CancellationCallbackCoreWorkArguments cancellationCallbackCoreWorkArguments = new CancellationCallbackCoreWorkArguments(sparselyPopulatedArrayFragment, num);
							try
							{
								if (m_executingCallback.TargetSyncContext != null)
								{
									m_executingCallback.TargetSyncContext.Send(CancellationCallbackCoreWork_OnSyncContext, cancellationCallbackCoreWorkArguments);
									ThreadIDExecutingCallbacks = Thread.CurrentThread.ManagedThreadId;
								}
								else
								{
									CancellationCallbackCoreWork(cancellationCallbackCoreWorkArguments);
								}
							}
							catch (Exception item)
							{
								if (throwOnFirstException)
								{
									throw;
								}
								if (list == null)
								{
									list = new List<Exception>();
								}
								list.Add(item);
							}
						}
					}
				}
			}
		}
		finally
		{
			m_state = 3;
			m_executingCallback = null;
			Thread.MemoryBarrier();
		}
		if (list == null)
		{
			return;
		}
		throw new AggregateException(list);
	}

	private void CancellationCallbackCoreWork_OnSyncContext(object obj)
	{
		CancellationCallbackCoreWork((CancellationCallbackCoreWorkArguments)obj);
	}

	private void CancellationCallbackCoreWork(CancellationCallbackCoreWorkArguments args)
	{
		CancellationCallbackInfo cancellationCallbackInfo = args.m_currArrayFragment.SafeAtomicRemove(args.m_currArrayIndex, m_executingCallback);
		if (cancellationCallbackInfo == m_executingCallback)
		{
			if (cancellationCallbackInfo.TargetExecutionContext != null)
			{
				cancellationCallbackInfo.CancellationTokenSource.ThreadIDExecutingCallbacks = Thread.CurrentThread.ManagedThreadId;
			}
			cancellationCallbackInfo.ExecuteCallback();
		}
	}

	/// <summary>Creates a <see cref="T:System.Threading.CancellationTokenSource" /> that will be in the canceled state when any of the source tokens are in the canceled state.</summary>
	/// <returns>A <see cref="T:System.Threading.CancellationTokenSource" /> that is linked to the source tokens.</returns>
	/// <param name="token1">The first cancellation token to observe.</param>
	/// <param name="token2">The second cancellation token to observe.</param>
	/// <exception cref="T:System.ObjectDisposedException">A <see cref="T:System.Threading.CancellationTokenSource" /> associated with one of the source tokens has been disposed.</exception>
	public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token1, CancellationToken token2)
	{
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		bool canBeCanceled = token2.CanBeCanceled;
		if (token1.CanBeCanceled)
		{
			cancellationTokenSource.m_linkingRegistrations = new CancellationTokenRegistration[(!canBeCanceled) ? 1 : 2];
			cancellationTokenSource.m_linkingRegistrations[0] = token1.InternalRegisterWithoutEC(s_LinkedTokenCancelDelegate, cancellationTokenSource);
		}
		if (canBeCanceled)
		{
			int num = 1;
			if (cancellationTokenSource.m_linkingRegistrations == null)
			{
				cancellationTokenSource.m_linkingRegistrations = new CancellationTokenRegistration[1];
				num = 0;
			}
			cancellationTokenSource.m_linkingRegistrations[num] = token2.InternalRegisterWithoutEC(s_LinkedTokenCancelDelegate, cancellationTokenSource);
		}
		return cancellationTokenSource;
	}

	/// <summary>Creates a <see cref="T:System.Threading.CancellationTokenSource" /> that will be in the canceled state when any of the source tokens in the specified array are in the canceled state.</summary>
	/// <returns>A <see cref="T:System.Threading.CancellationTokenSource" /> that is linked to the source tokens.</returns>
	/// <param name="tokens">An array that contains the cancellation token instances to observe.</param>
	/// <exception cref="T:System.ObjectDisposedException">A <see cref="T:System.Threading.CancellationTokenSource" /> associated with one of the source tokens has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="tokens" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="tokens" /> is empty.</exception>
	public static CancellationTokenSource CreateLinkedTokenSource(params CancellationToken[] tokens)
	{
		if (tokens == null)
		{
			throw new ArgumentNullException("tokens");
		}
		if (tokens.Length == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("No tokens were supplied."));
		}
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.m_linkingRegistrations = new CancellationTokenRegistration[tokens.Length];
		for (int i = 0; i < tokens.Length; i++)
		{
			if (tokens[i].CanBeCanceled)
			{
				cancellationTokenSource.m_linkingRegistrations[i] = tokens[i].InternalRegisterWithoutEC(s_LinkedTokenCancelDelegate, cancellationTokenSource);
			}
		}
		return cancellationTokenSource;
	}

	internal void WaitForCallbackToComplete(CancellationCallbackInfo callbackInfo)
	{
		SpinWait spinWait = default(SpinWait);
		while (ExecutingCallback == callbackInfo)
		{
			spinWait.SpinOnce();
		}
	}
}
