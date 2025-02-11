using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Security;
using System.Security.Permissions;

namespace System.Threading;

/// <summary>Provides a mechanism that synchronizes access to objects.</summary>
/// <filterpriority>2</filterpriority>
[ComVisible(true)]
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public static class Monitor
{
	/// <summary>Acquires an exclusive lock on the specified object.</summary>
	/// <param name="obj">The object on which to acquire the monitor lock. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecuritySafeCritical]
	public static extern void Enter(object obj);

	/// <summary>Acquires an exclusive lock on the specified object, and atomically sets a value that indicates whether the lock was taken.</summary>
	/// <param name="obj">The object on which to wait. </param>
	/// <param name="lockTaken">The result of the attempt to acquire the lock, passed by reference. The input must be false. The output is true if the lock is acquired; otherwise, the output is false. The output is set even if an exception occurs during the attempt to acquire the lock. Note   If no exception occurs, the output of this method is always true.</param>
	/// <exception cref="T:System.ArgumentException">The input to <paramref name="lockTaken" /> is true.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	public static void Enter(object obj, ref bool lockTaken)
	{
		if (lockTaken)
		{
			ThrowLockTakenException();
		}
		ReliableEnter(obj, ref lockTaken);
	}

	private static void ThrowLockTakenException()
	{
		throw new ArgumentException(Environment.GetResourceString("Argument must be initialized to false"), "lockTaken");
	}

	/// <summary>Releases an exclusive lock on the specified object.</summary>
	/// <param name="obj">The object on which to release the lock. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <exception cref="T:System.Threading.SynchronizationLockException">The current thread does not own the lock for the specified object. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[SecuritySafeCritical]
	public static extern void Exit(object obj);

	/// <summary>Attempts to acquire an exclusive lock on the specified object.</summary>
	/// <returns>true if the current thread acquires the lock; otherwise, false.</returns>
	/// <param name="obj">The object on which to acquire the lock. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool TryEnter(object obj)
	{
		bool lockTaken = false;
		TryEnter(obj, 0, ref lockTaken);
		return lockTaken;
	}

	/// <summary>Attempts to acquire an exclusive lock on the specified object, and atomically sets a value that indicates whether the lock was taken.</summary>
	/// <param name="obj">The object on which to acquire the lock. </param>
	/// <param name="lockTaken">The result of the attempt to acquire the lock, passed by reference. The input must be false. The output is true if the lock is acquired; otherwise, the output is false. The output is set even if an exception occurs during the attempt to acquire the lock.</param>
	/// <exception cref="T:System.ArgumentException">The input to <paramref name="lockTaken" /> is true.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	public static void TryEnter(object obj, ref bool lockTaken)
	{
		if (lockTaken)
		{
			ThrowLockTakenException();
		}
		ReliableEnterTimeout(obj, 0, ref lockTaken);
	}

	/// <summary>Attempts, for the specified number of milliseconds, to acquire an exclusive lock on the specified object.</summary>
	/// <returns>true if the current thread acquires the lock; otherwise, false.</returns>
	/// <param name="obj">The object on which to acquire the lock. </param>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait for the lock. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is negative, and not equal to <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool TryEnter(object obj, int millisecondsTimeout)
	{
		bool lockTaken = false;
		TryEnter(obj, millisecondsTimeout, ref lockTaken);
		return lockTaken;
	}

	private static int MillisecondsTimeoutFromTimeSpan(TimeSpan timeout)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("Number must be either non-negative and less than or equal to Int32.MaxValue or -1."));
		}
		return (int)num;
	}

	/// <summary>Attempts, for the specified amount of time, to acquire an exclusive lock on the specified object.</summary>
	/// <returns>true if the current thread acquires the lock; otherwise, false.</returns>
	/// <param name="obj">The object on which to acquire the lock. </param>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> representing the amount of time to wait for the lock. A value of –1 millisecond specifies an infinite wait.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="timeout" /> in milliseconds is negative and is not equal to <see cref="F:System.Threading.Timeout.Infinite" /> (–1 millisecond), or is greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool TryEnter(object obj, TimeSpan timeout)
	{
		return TryEnter(obj, MillisecondsTimeoutFromTimeSpan(timeout));
	}

	/// <summary>Attempts, for the specified number of milliseconds, to acquire an exclusive lock on the specified object, and atomically sets a value that indicates whether the lock was taken.</summary>
	/// <param name="obj">The object on which to acquire the lock. </param>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait for the lock. </param>
	/// <param name="lockTaken">The result of the attempt to acquire the lock, passed by reference. The input must be false. The output is true if the lock is acquired; otherwise, the output is false. The output is set even if an exception occurs during the attempt to acquire the lock.</param>
	/// <exception cref="T:System.ArgumentException">The input to <paramref name="lockTaken" /> is true.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> is negative, and not equal to <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	public static void TryEnter(object obj, int millisecondsTimeout, ref bool lockTaken)
	{
		if (lockTaken)
		{
			ThrowLockTakenException();
		}
		ReliableEnterTimeout(obj, millisecondsTimeout, ref lockTaken);
	}

	/// <summary>Attempts, for the specified amount of time, to acquire an exclusive lock on the specified object, and atomically sets a value that indicates whether the lock was taken.</summary>
	/// <param name="obj">The object on which to acquire the lock. </param>
	/// <param name="timeout">The amount of time to wait for the lock. A value of –1 millisecond specifies an infinite wait.</param>
	/// <param name="lockTaken">The result of the attempt to acquire the lock, passed by reference. The input must be false. The output is true if the lock is acquired; otherwise, the output is false. The output is set even if an exception occurs during the attempt to acquire the lock.</param>
	/// <exception cref="T:System.ArgumentException">The input to <paramref name="lockTaken" /> is true.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="timeout" /> in milliseconds is negative and is not equal to <see cref="F:System.Threading.Timeout.Infinite" /> (–1 millisecond), or is greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	public static void TryEnter(object obj, TimeSpan timeout, ref bool lockTaken)
	{
		if (lockTaken)
		{
			ThrowLockTakenException();
		}
		ReliableEnterTimeout(obj, MillisecondsTimeoutFromTimeSpan(timeout), ref lockTaken);
	}

	/// <summary>Determines whether the current thread holds the lock on the specified object. </summary>
	/// <returns>true if the current thread holds the lock on <paramref name="obj" />; otherwise, false.</returns>
	/// <param name="obj">The object to test. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="obj" /> is null. </exception>
	[SecuritySafeCritical]
	public static bool IsEntered(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		return IsEnteredNative(obj);
	}

	/// <summary>Releases the lock on an object and blocks the current thread until it reacquires the lock. If the specified time-out interval elapses, the thread enters the ready queue. This method also specifies whether the synchronization domain for the context (if in a synchronized context) is exited before the wait and reacquired afterward.</summary>
	/// <returns>true if the lock was reacquired before the specified time elapsed; false if the lock was reacquired after the specified time elapsed. The method does not return until the lock is reacquired.</returns>
	/// <param name="obj">The object on which to wait. </param>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait before the thread enters the ready queue. </param>
	/// <param name="exitContext">true to exit and reacquire the synchronization domain for the context (if in a synchronized context) before the wait; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <exception cref="T:System.Threading.SynchronizationLockException">Wait is not invoked from within a synchronized block of code. </exception>
	/// <exception cref="T:System.Threading.ThreadInterruptedException">The thread that invokes Wait is later interrupted from the waiting state. This happens when another thread calls this thread's <see cref="M:System.Threading.Thread.Interrupt" /> method. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of the <paramref name="millisecondsTimeout" /> parameter is negative, and is not equal to <see cref="F:System.Threading.Timeout.Infinite" />.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static bool Wait(object obj, int millisecondsTimeout, bool exitContext)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		return ObjWait(exitContext, millisecondsTimeout, obj);
	}

	/// <summary>Releases the lock on an object and blocks the current thread until it reacquires the lock. If the specified time-out interval elapses, the thread enters the ready queue. Optionally exits the synchronization domain for the synchronized context before the wait and reacquires the domain afterward.</summary>
	/// <returns>true if the lock was reacquired before the specified time elapsed; false if the lock was reacquired after the specified time elapsed. The method does not return until the lock is reacquired.</returns>
	/// <param name="obj">The object on which to wait. </param>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> representing the amount of time to wait before the thread enters the ready queue. </param>
	/// <param name="exitContext">true to exit and reacquire the synchronization domain for the context (if in a synchronized context) before the wait; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <exception cref="T:System.Threading.SynchronizationLockException">Wait is not invoked from within a synchronized block of code. </exception>
	/// <exception cref="T:System.Threading.ThreadInterruptedException">The thread that invokes Wait is later interrupted from the waiting state. This happens when another thread calls this thread's <see cref="M:System.Threading.Thread.Interrupt" /> method. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="timeout" /> parameter is negative and does not represent <see cref="F:System.Threading.Timeout.Infinite" /> (–1 millisecond), or is greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool Wait(object obj, TimeSpan timeout, bool exitContext)
	{
		return Wait(obj, MillisecondsTimeoutFromTimeSpan(timeout), exitContext);
	}

	/// <summary>Releases the lock on an object and blocks the current thread until it reacquires the lock. If the specified time-out interval elapses, the thread enters the ready queue.</summary>
	/// <returns>true if the lock was reacquired before the specified time elapsed; false if the lock was reacquired after the specified time elapsed. The method does not return until the lock is reacquired.</returns>
	/// <param name="obj">The object on which to wait. </param>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait before the thread enters the ready queue. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <exception cref="T:System.Threading.SynchronizationLockException">The calling thread does not own the lock for the specified object. </exception>
	/// <exception cref="T:System.Threading.ThreadInterruptedException">The thread that invokes Wait is later interrupted from the waiting state. This happens when another thread calls this thread's <see cref="M:System.Threading.Thread.Interrupt" /> method. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of the <paramref name="millisecondsTimeout" /> parameter is negative, and is not equal to <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool Wait(object obj, int millisecondsTimeout)
	{
		return Wait(obj, millisecondsTimeout, exitContext: false);
	}

	/// <summary>Releases the lock on an object and blocks the current thread until it reacquires the lock. If the specified time-out interval elapses, the thread enters the ready queue.</summary>
	/// <returns>true if the lock was reacquired before the specified time elapsed; false if the lock was reacquired after the specified time elapsed. The method does not return until the lock is reacquired.</returns>
	/// <param name="obj">The object on which to wait. </param>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> representing the amount of time to wait before the thread enters the ready queue. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <exception cref="T:System.Threading.SynchronizationLockException">The calling thread does not own the lock for the specified object. </exception>
	/// <exception cref="T:System.Threading.ThreadInterruptedException">The thread that invokes Wait is later interrupted from the waiting state. This happens when another thread calls this thread's <see cref="M:System.Threading.Thread.Interrupt" /> method. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of the <paramref name="timeout" /> parameter in milliseconds is negative and does not represent <see cref="F:System.Threading.Timeout.Infinite" /> (–1 millisecond), or is greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool Wait(object obj, TimeSpan timeout)
	{
		return Wait(obj, MillisecondsTimeoutFromTimeSpan(timeout), exitContext: false);
	}

	/// <summary>Releases the lock on an object and blocks the current thread until it reacquires the lock.</summary>
	/// <returns>true if the call returned because the caller reacquired the lock for the specified object. This method does not return if the lock is not reacquired.</returns>
	/// <param name="obj">The object on which to wait. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <exception cref="T:System.Threading.SynchronizationLockException">The calling thread does not own the lock for the specified object. </exception>
	/// <exception cref="T:System.Threading.ThreadInterruptedException">The thread that invokes Wait is later interrupted from the waiting state. This happens when another thread calls this thread's <see cref="M:System.Threading.Thread.Interrupt" /> method. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool Wait(object obj)
	{
		return Wait(obj, -1, exitContext: false);
	}

	/// <summary>Notifies a thread in the waiting queue of a change in the locked object's state.</summary>
	/// <param name="obj">The object a thread is waiting for. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <exception cref="T:System.Threading.SynchronizationLockException">The calling thread does not own the lock for the specified object. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static void Pulse(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		ObjPulse(obj);
	}

	/// <summary>Notifies all waiting threads of a change in the object's state.</summary>
	/// <param name="obj">The object that sends the pulse. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <exception cref="T:System.Threading.SynchronizationLockException">The calling thread does not own the lock for the specified object. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static void PulseAll(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		ObjPulseAll(obj);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool Monitor_test_synchronised(object obj);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Monitor_pulse(object obj);

	private static void ObjPulse(object obj)
	{
		if (!Monitor_test_synchronised(obj))
		{
			throw new SynchronizationLockException("Object is not synchronized");
		}
		Monitor_pulse(obj);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Monitor_pulse_all(object obj);

	private static void ObjPulseAll(object obj)
	{
		if (!Monitor_test_synchronised(obj))
		{
			throw new SynchronizationLockException("Object is not synchronized");
		}
		Monitor_pulse_all(obj);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool Monitor_wait(object obj, int ms);

	private static bool ObjWait(bool exitContext, int millisecondsTimeout, object obj)
	{
		if (millisecondsTimeout < 0 && millisecondsTimeout != -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeout");
		}
		if (!Monitor_test_synchronised(obj))
		{
			throw new SynchronizationLockException("Object is not synchronized");
		}
		try
		{
			if (exitContext)
			{
				SynchronizationAttribute.ExitContext();
			}
			return Monitor_wait(obj, millisecondsTimeout);
		}
		finally
		{
			if (exitContext)
			{
				SynchronizationAttribute.EnterContext();
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void try_enter_with_atomic_var(object obj, int millisecondsTimeout, ref bool lockTaken);

	private static void ReliableEnterTimeout(object obj, int timeout, ref bool lockTaken)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (timeout < 0 && timeout != -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeout");
		}
		try_enter_with_atomic_var(obj, timeout, ref lockTaken);
	}

	private static void ReliableEnter(object obj, ref bool lockTaken)
	{
		ReliableEnterTimeout(obj, -1, ref lockTaken);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool Monitor_test_owner(object obj);

	private static bool IsEnteredNative(object obj)
	{
		return Monitor_test_owner(obj);
	}
}
