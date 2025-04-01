using System.Collections;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace System.Threading;

/// <summary>Defines a lock that supports single writers and multiple readers. </summary>
/// <filterpriority>2</filterpriority>
[ComVisible(true)]
public sealed class ReaderWriterLock : CriticalFinalizerObject
{
	private int seq_num = 1;

	private int state;

	private int readers;

	private int writer_lock_owner;

	private LockQueue writer_queue;

	private Hashtable reader_locks;

	/// <summary>Gets a value indicating whether the current thread holds a reader lock.</summary>
	/// <returns>true if the current thread holds a reader lock; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsReaderLockHeld
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		get
		{
			lock (this)
			{
				return reader_locks.ContainsKey(Thread.CurrentThreadId);
			}
		}
	}

	/// <summary>Gets a value indicating whether the current thread holds the writer lock.</summary>
	/// <returns>true if the current thread holds the writer lock; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsWriterLockHeld
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		get
		{
			lock (this)
			{
				return state < 0 && Thread.CurrentThreadId == writer_lock_owner;
			}
		}
	}

	/// <summary>Gets the current sequence number.</summary>
	/// <returns>The current sequence number.</returns>
	/// <filterpriority>2</filterpriority>
	public int WriterSeqNum
	{
		get
		{
			lock (this)
			{
				return seq_num;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.ReaderWriterLock" /> class.</summary>
	public ReaderWriterLock()
	{
		writer_queue = new LockQueue(this);
		reader_locks = new Hashtable();
		GC.SuppressFinalize(this);
	}

	~ReaderWriterLock()
	{
	}

	/// <summary>Acquires a reader lock, using an <see cref="T:System.Int32" /> value for the time-out.</summary>
	/// <param name="millisecondsTimeout">The time-out in milliseconds. </param>
	/// <exception cref="T:System.ApplicationException">
	///   <paramref name="millisecondsTimeout" /> expires before the lock request is granted. </exception>
	/// <filterpriority>2</filterpriority>
	public void AcquireReaderLock(int millisecondsTimeout)
	{
		AcquireReaderLock(millisecondsTimeout, 1);
	}

	private void AcquireReaderLock(int millisecondsTimeout, int initialLockCount)
	{
		lock (this)
		{
			if (HasWriterLock())
			{
				AcquireWriterLock(millisecondsTimeout, initialLockCount);
				return;
			}
			object obj = reader_locks[Thread.CurrentThreadId];
			if (obj == null)
			{
				readers++;
				try
				{
					if (state < 0 || !writer_queue.IsEmpty)
					{
						do
						{
							if (!Monitor.Wait(this, millisecondsTimeout))
							{
								throw new ApplicationException("Timeout expired");
							}
						}
						while (state < 0);
					}
				}
				finally
				{
					readers--;
				}
				reader_locks[Thread.CurrentThreadId] = initialLockCount;
				state += initialLockCount;
			}
			else
			{
				reader_locks[Thread.CurrentThreadId] = (int)obj + 1;
				state++;
			}
		}
	}

	/// <summary>Acquires a reader lock, using a <see cref="T:System.TimeSpan" /> value for the time-out.</summary>
	/// <param name="timeout">A TimeSpan specifying the time-out period. </param>
	/// <exception cref="T:System.ApplicationException">
	///   <paramref name="timeout" /> expires before the lock request is granted. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> specifies a negative value other than -1 milliseconds. </exception>
	/// <filterpriority>2</filterpriority>
	public void AcquireReaderLock(TimeSpan timeout)
	{
		int millisecondsTimeout = CheckTimeout(timeout);
		AcquireReaderLock(millisecondsTimeout, 1);
	}

	/// <summary>Acquires the writer lock, using an <see cref="T:System.Int32" /> value for the time-out.</summary>
	/// <param name="millisecondsTimeout">The time-out in milliseconds. </param>
	/// <exception cref="T:System.ApplicationException">
	///   <paramref name="timeout" /> expires before the lock request is granted. </exception>
	/// <filterpriority>2</filterpriority>
	public void AcquireWriterLock(int millisecondsTimeout)
	{
		AcquireWriterLock(millisecondsTimeout, 1);
	}

	private void AcquireWriterLock(int millisecondsTimeout, int initialLockCount)
	{
		lock (this)
		{
			if (HasWriterLock())
			{
				state--;
				return;
			}
			if (state != 0 || !writer_queue.IsEmpty)
			{
				do
				{
					if (!writer_queue.Wait(millisecondsTimeout))
					{
						throw new ApplicationException("Timeout expired");
					}
				}
				while (state != 0);
			}
			state = -initialLockCount;
			writer_lock_owner = Thread.CurrentThreadId;
			seq_num++;
		}
	}

	/// <summary>Acquires the writer lock, using a <see cref="T:System.TimeSpan" /> value for the time-out.</summary>
	/// <param name="timeout">The TimeSpan specifying the time-out period. </param>
	/// <exception cref="T:System.ApplicationException">
	///   <paramref name="timeout" /> expires before the lock request is granted. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> specifies a negative value other than -1 milliseconds. </exception>
	/// <filterpriority>2</filterpriority>
	public void AcquireWriterLock(TimeSpan timeout)
	{
		int millisecondsTimeout = CheckTimeout(timeout);
		AcquireWriterLock(millisecondsTimeout, 1);
	}

	/// <summary>Indicates whether the writer lock has been granted to any thread since the sequence number was obtained.</summary>
	/// <returns>true if the writer lock has been granted to any thread since the sequence number was obtained; otherwise, false.</returns>
	/// <param name="seqNum">The sequence number. </param>
	/// <filterpriority>2</filterpriority>
	public bool AnyWritersSince(int seqNum)
	{
		lock (this)
		{
			return seq_num > seqNum;
		}
	}

	/// <summary>Restores the lock status of the thread to what it was before <see cref="M:System.Threading.ReaderWriterLock.UpgradeToWriterLock(System.Int32)" /> was called.</summary>
	/// <param name="lockCookie">A <see cref="T:System.Threading.LockCookie" /> returned by <see cref="M:System.Threading.ReaderWriterLock.UpgradeToWriterLock(System.Int32)" />. </param>
	/// <exception cref="T:System.ApplicationException">The thread does not have the writer lock. </exception>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="lockCookie" /> is a null pointer. </exception>
	/// <filterpriority>2</filterpriority>
	public void DowngradeFromWriterLock(ref LockCookie lockCookie)
	{
		lock (this)
		{
			if (!HasWriterLock())
			{
				throw new ApplicationException("The thread does not have the writer lock.");
			}
			if (lockCookie.WriterLocks != 0)
			{
				state++;
				return;
			}
			state = lockCookie.ReaderLocks;
			reader_locks[Thread.CurrentThreadId] = state;
			if (readers > 0)
			{
				Monitor.PulseAll(this);
			}
		}
	}

	/// <summary>Releases the lock, regardless of the number of times the thread acquired the lock.</summary>
	/// <returns>A <see cref="T:System.Threading.LockCookie" /> value representing the released lock.</returns>
	/// <filterpriority>2</filterpriority>
	public LockCookie ReleaseLock()
	{
		LockCookie lockCookie;
		lock (this)
		{
			lockCookie = GetLockCookie();
			if (lockCookie.WriterLocks != 0)
			{
				ReleaseWriterLock(lockCookie.WriterLocks);
			}
			else if (lockCookie.ReaderLocks != 0)
			{
				ReleaseReaderLock(lockCookie.ReaderLocks, lockCookie.ReaderLocks);
			}
		}
		return lockCookie;
	}

	/// <summary>Decrements the lock count.</summary>
	/// <exception cref="T:System.ApplicationException">The thread does not have any reader or writer locks. </exception>
	/// <filterpriority>2</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public void ReleaseReaderLock()
	{
		lock (this)
		{
			if (HasWriterLock())
			{
				ReleaseWriterLock();
				return;
			}
			if (state > 0)
			{
				object obj = reader_locks[Thread.CurrentThreadId];
				if (obj != null)
				{
					ReleaseReaderLock((int)obj, 1);
					return;
				}
			}
			throw new ApplicationException("The thread does not have any reader or writer locks.");
		}
	}

	private void ReleaseReaderLock(int currentCount, int releaseCount)
	{
		int num = currentCount - releaseCount;
		if (num == 0)
		{
			reader_locks.Remove(Thread.CurrentThreadId);
		}
		else
		{
			reader_locks[Thread.CurrentThreadId] = num;
		}
		state -= releaseCount;
		if (state == 0 && !writer_queue.IsEmpty)
		{
			writer_queue.Pulse();
		}
	}

	/// <summary>Decrements the lock count on the writer lock.</summary>
	/// <exception cref="T:System.ApplicationException">The thread does not have the writer lock. </exception>
	/// <filterpriority>2</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public void ReleaseWriterLock()
	{
		lock (this)
		{
			if (!HasWriterLock())
			{
				throw new ApplicationException("The thread does not have the writer lock.");
			}
			ReleaseWriterLock(1);
		}
	}

	private void ReleaseWriterLock(int releaseCount)
	{
		state += releaseCount;
		if (state == 0)
		{
			if (readers > 0)
			{
				Monitor.PulseAll(this);
			}
			else if (!writer_queue.IsEmpty)
			{
				writer_queue.Pulse();
			}
		}
	}

	/// <summary>Restores the lock status of the thread to what it was before calling <see cref="M:System.Threading.ReaderWriterLock.ReleaseLock" />.</summary>
	/// <param name="lockCookie">A <see cref="T:System.Threading.LockCookie" /> returned by <see cref="M:System.Threading.ReaderWriterLock.ReleaseLock" />. </param>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="lockCookie" /> is a null pointer. </exception>
	/// <filterpriority>2</filterpriority>
	public void RestoreLock(ref LockCookie lockCookie)
	{
		lock (this)
		{
			if (lockCookie.WriterLocks != 0)
			{
				AcquireWriterLock(-1, lockCookie.WriterLocks);
			}
			else if (lockCookie.ReaderLocks != 0)
			{
				AcquireReaderLock(-1, lockCookie.ReaderLocks);
			}
		}
	}

	/// <summary>Upgrades a reader lock to the writer lock, using an Int32 value for the time-out.</summary>
	/// <returns>A <see cref="T:System.Threading.LockCookie" /> value.</returns>
	/// <param name="millisecondsTimeout">The time-out in milliseconds. </param>
	/// <exception cref="T:System.ApplicationException">
	///   <paramref name="millisecondsTimeout" /> expires before the lock request is granted. </exception>
	/// <filterpriority>2</filterpriority>
	public LockCookie UpgradeToWriterLock(int millisecondsTimeout)
	{
		LockCookie lockCookie;
		lock (this)
		{
			lockCookie = GetLockCookie();
			if (lockCookie.WriterLocks != 0)
			{
				state--;
				return lockCookie;
			}
			if (lockCookie.ReaderLocks != 0)
			{
				ReleaseReaderLock(lockCookie.ReaderLocks, lockCookie.ReaderLocks);
			}
		}
		AcquireWriterLock(millisecondsTimeout);
		return lockCookie;
	}

	/// <summary>Upgrades a reader lock to the writer lock, using a TimeSpan value for the time-out.</summary>
	/// <returns>A <see cref="T:System.Threading.LockCookie" /> value.</returns>
	/// <param name="timeout">The TimeSpan specifying the time-out period. </param>
	/// <exception cref="T:System.ApplicationException">
	///   <paramref name="timeout" /> expires before the lock request is granted. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> specifies a negative value other than -1 milliseconds. </exception>
	/// <filterpriority>2</filterpriority>
	public LockCookie UpgradeToWriterLock(TimeSpan timeout)
	{
		int millisecondsTimeout = CheckTimeout(timeout);
		return UpgradeToWriterLock(millisecondsTimeout);
	}

	private LockCookie GetLockCookie()
	{
		LockCookie result = new LockCookie(Thread.CurrentThreadId);
		if (HasWriterLock())
		{
			result.WriterLocks = -state;
		}
		else
		{
			object obj = reader_locks[Thread.CurrentThreadId];
			if (obj != null)
			{
				result.ReaderLocks = (int)obj;
			}
		}
		return result;
	}

	private bool HasWriterLock()
	{
		if (state < 0)
		{
			return Thread.CurrentThreadId == writer_lock_owner;
		}
		return false;
	}

	private int CheckTimeout(TimeSpan timeout)
	{
		int num = (int)timeout.TotalMilliseconds;
		if (num < -1)
		{
			throw new ArgumentOutOfRangeException("timeout", "Number must be either non-negative or -1");
		}
		return num;
	}
}
