using System.Runtime.CompilerServices;
using System.Security.Permissions;

namespace System.Threading;

/// <summary>Represents a lock that is used to manage access to a resource, allowing multiple threads for reading or exclusive access for writing.</summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class ReaderWriterLockSlim : IDisposable
{
	private struct TimeoutTracker
	{
		private int m_total;

		private int m_start;

		public int RemainingMilliseconds
		{
			get
			{
				if (m_total == -1 || m_total == 0)
				{
					return m_total;
				}
				int num = Environment.TickCount - m_start;
				if (num < 0 || num >= m_total)
				{
					return 0;
				}
				return m_total - num;
			}
		}

		public bool IsExpired => RemainingMilliseconds == 0;

		public TimeoutTracker(TimeSpan timeout)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1 || num > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("timeout");
			}
			m_total = (int)num;
			if (m_total != -1 && m_total != 0)
			{
				m_start = Environment.TickCount;
			}
			else
			{
				m_start = 0;
			}
		}

		public TimeoutTracker(int millisecondsTimeout)
		{
			if (millisecondsTimeout < -1)
			{
				throw new ArgumentOutOfRangeException("millisecondsTimeout");
			}
			m_total = millisecondsTimeout;
			if (m_total != -1 && m_total != 0)
			{
				m_start = Environment.TickCount;
			}
			else
			{
				m_start = 0;
			}
		}
	}

	private bool fIsReentrant;

	private int myLock;

	private const int LockSpinCycles = 20;

	private const int LockSpinCount = 10;

	private const int LockSleep0Count = 5;

	private uint numWriteWaiters;

	private uint numReadWaiters;

	private uint numWriteUpgradeWaiters;

	private uint numUpgradeWaiters;

	private bool fNoWaiters;

	private int upgradeLockOwnerId;

	private int writeLockOwnerId;

	private EventWaitHandle writeEvent;

	private EventWaitHandle readEvent;

	private EventWaitHandle upgradeEvent;

	private EventWaitHandle waitUpgradeEvent;

	private static long s_nextLockID;

	private long lockID;

	[ThreadStatic]
	private static ReaderWriterCount t_rwc;

	private bool fUpgradeThreadHoldingRead;

	private const int MaxSpinCount = 20;

	private uint owners;

	private const uint WRITER_HELD = 2147483648u;

	private const uint WAITING_WRITERS = 1073741824u;

	private const uint WAITING_UPGRADER = 536870912u;

	private const uint MAX_READER = 268435454u;

	private const uint READER_MASK = 268435455u;

	private bool fDisposed;

	/// <summary>Gets a value that indicates whether the current thread has entered the lock in read mode.</summary>
	/// <returns>true if the current thread has entered read mode; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsReadLockHeld
	{
		get
		{
			if (RecursiveReadCount > 0)
			{
				return true;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether the current thread has entered the lock in upgradeable mode. </summary>
	/// <returns>true if the current thread has entered upgradeable mode; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsUpgradeableReadLockHeld
	{
		get
		{
			if (RecursiveUpgradeCount > 0)
			{
				return true;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether the current thread has entered the lock in write mode.</summary>
	/// <returns>true if the current thread has entered write mode; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsWriteLockHeld
	{
		get
		{
			if (RecursiveWriteCount > 0)
			{
				return true;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates the recursion policy for the current <see cref="T:System.Threading.ReaderWriterLockSlim" /> object.</summary>
	/// <returns>One of the enumeration values that specifies the lock recursion policy.</returns>
	public LockRecursionPolicy RecursionPolicy
	{
		get
		{
			if (fIsReentrant)
			{
				return LockRecursionPolicy.SupportsRecursion;
			}
			return LockRecursionPolicy.NoRecursion;
		}
	}

	/// <summary>Gets the total number of unique threads that have entered the lock in read mode.</summary>
	/// <returns>The number of unique threads that have entered the lock in read mode.</returns>
	public int CurrentReadCount
	{
		get
		{
			int numReaders = (int)GetNumReaders();
			if (upgradeLockOwnerId != -1)
			{
				return numReaders - 1;
			}
			return numReaders;
		}
	}

	/// <summary>Gets the number of times the current thread has entered the lock in read mode, as an indication of recursion.</summary>
	/// <returns>0 (zero) if the current thread has not entered read mode, 1 if the thread has entered read mode but has not entered it recursively, or n if the thread has entered the lock recursively n - 1 times.</returns>
	/// <filterpriority>2</filterpriority>
	public int RecursiveReadCount
	{
		get
		{
			int result = 0;
			ReaderWriterCount threadRWCount = GetThreadRWCount(dontAllocate: true);
			if (threadRWCount != null)
			{
				result = threadRWCount.readercount;
			}
			return result;
		}
	}

	/// <summary>Gets the number of times the current thread has entered the lock in upgradeable mode, as an indication of recursion.</summary>
	/// <returns>0 if the current thread has not entered upgradeable mode, 1 if the thread has entered upgradeable mode but has not entered it recursively, or n if the thread has entered upgradeable mode recursively n - 1 times.</returns>
	/// <filterpriority>2</filterpriority>
	public int RecursiveUpgradeCount
	{
		get
		{
			if (fIsReentrant)
			{
				int result = 0;
				ReaderWriterCount threadRWCount = GetThreadRWCount(dontAllocate: true);
				if (threadRWCount != null)
				{
					result = threadRWCount.upgradecount;
				}
				return result;
			}
			if (Thread.CurrentThread.ManagedThreadId == upgradeLockOwnerId)
			{
				return 1;
			}
			return 0;
		}
	}

	/// <summary>Gets the number of times the current thread has entered the lock in write mode, as an indication of recursion.</summary>
	/// <returns>0 if the current thread has not entered write mode, 1 if the thread has entered write mode but has not entered it recursively, or n if the thread has entered write mode recursively n - 1 times.</returns>
	/// <filterpriority>2</filterpriority>
	public int RecursiveWriteCount
	{
		get
		{
			if (fIsReentrant)
			{
				int result = 0;
				ReaderWriterCount threadRWCount = GetThreadRWCount(dontAllocate: true);
				if (threadRWCount != null)
				{
					result = threadRWCount.writercount;
				}
				return result;
			}
			if (Thread.CurrentThread.ManagedThreadId == writeLockOwnerId)
			{
				return 1;
			}
			return 0;
		}
	}

	/// <summary>Gets the total number of threads that are waiting to enter the lock in read mode.</summary>
	/// <returns>The total number of threads that are waiting to enter read mode.</returns>
	/// <filterpriority>2</filterpriority>
	public int WaitingReadCount => (int)numReadWaiters;

	/// <summary>Gets the total number of threads that are waiting to enter the lock in upgradeable mode.</summary>
	/// <returns>The total number of threads that are waiting to enter upgradeable mode.</returns>
	/// <filterpriority>2</filterpriority>
	public int WaitingUpgradeCount => (int)numUpgradeWaiters;

	/// <summary>Gets the total number of threads that are waiting to enter the lock in write mode.</summary>
	/// <returns>The total number of threads that are waiting to enter write mode.</returns>
	/// <filterpriority>2</filterpriority>
	public int WaitingWriteCount => (int)numWriteWaiters;

	private void InitializeThreadCounts()
	{
		upgradeLockOwnerId = -1;
		writeLockOwnerId = -1;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.ReaderWriterLockSlim" /> class with default property values.</summary>
	public ReaderWriterLockSlim()
		: this(LockRecursionPolicy.NoRecursion)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.ReaderWriterLockSlim" /> class, specifying the lock recursion policy.</summary>
	/// <param name="recursionPolicy">One of the enumeration values that specifies the lock recursion policy. </param>
	public ReaderWriterLockSlim(LockRecursionPolicy recursionPolicy)
	{
		if (recursionPolicy == LockRecursionPolicy.SupportsRecursion)
		{
			fIsReentrant = true;
		}
		InitializeThreadCounts();
		fNoWaiters = true;
		lockID = Interlocked.Increment(ref s_nextLockID);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsRWEntryEmpty(ReaderWriterCount rwc)
	{
		if (rwc.lockID == 0L)
		{
			return true;
		}
		if (rwc.readercount == 0 && rwc.writercount == 0 && rwc.upgradecount == 0)
		{
			return true;
		}
		return false;
	}

	private bool IsRwHashEntryChanged(ReaderWriterCount lrwc)
	{
		return lrwc.lockID != lockID;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private ReaderWriterCount GetThreadRWCount(bool dontAllocate)
	{
		ReaderWriterCount next = t_rwc;
		ReaderWriterCount readerWriterCount = null;
		while (next != null)
		{
			if (next.lockID == lockID)
			{
				return next;
			}
			if (!dontAllocate && readerWriterCount == null && IsRWEntryEmpty(next))
			{
				readerWriterCount = next;
			}
			next = next.next;
		}
		if (dontAllocate)
		{
			return null;
		}
		if (readerWriterCount == null)
		{
			readerWriterCount = new ReaderWriterCount();
			readerWriterCount.next = t_rwc;
			t_rwc = readerWriterCount;
		}
		readerWriterCount.lockID = lockID;
		return readerWriterCount;
	}

	/// <summary>Tries to enter the lock in read mode.</summary>
	/// <exception cref="T:System.Threading.LockRecursionException">The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" /> and the current thread has already entered read mode. -or-The recursion number would exceed the capacity of the counter. This limit is so large that applications should never encounter it.</exception>
	public void EnterReadLock()
	{
		TryEnterReadLock(-1);
	}

	/// <summary>Tries to enter the lock in read mode, with an optional time-out.</summary>
	/// <returns>true if the calling thread entered read mode, otherwise, false.</returns>
	/// <param name="timeout">The interval to wait, or -1 milliseconds to wait indefinitely. </param>
	/// <exception cref="T:System.Threading.LockRecursionException">The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" /> and the current thread has already entered the lock. -or-The recursion number would exceed the capacity of the counter. The limit is so large that applications should never encounter it.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="timeout" /> is negative, but it is not equal to -1 milliseconds, which is the only negative value allowed.-or-The value of <paramref name="timeout" /> is greater than <see cref="F:System.Int32.MaxValue" /> milliseconds. </exception>
	public bool TryEnterReadLock(TimeSpan timeout)
	{
		return TryEnterReadLock(new TimeoutTracker(timeout));
	}

	/// <summary>Tries to enter the lock in read mode, with an optional integer time-out.</summary>
	/// <returns>true if the calling thread entered read mode, otherwise, false.</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or -1 (<see cref="F:System.Threading.Timeout.Infinite" />) to wait indefinitely.</param>
	/// <exception cref="T:System.Threading.LockRecursionException">The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" /> and the current thread has already entered the lock. -or-The recursion number would exceed the capacity of the counter. The limit is so large that applications should never encounter it.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="millisecondsTimeout" /> is negative, but it is not equal to <see cref="F:System.Threading.Timeout.Infinite" /> (-1), which is the only negative value allowed. </exception>
	public bool TryEnterReadLock(int millisecondsTimeout)
	{
		return TryEnterReadLock(new TimeoutTracker(millisecondsTimeout));
	}

	private bool TryEnterReadLock(TimeoutTracker timeout)
	{
		return TryEnterReadLockCore(timeout);
	}

	private bool TryEnterReadLockCore(TimeoutTracker timeout)
	{
		if (fDisposed)
		{
			throw new ObjectDisposedException(null);
		}
		ReaderWriterCount readerWriterCount = null;
		int managedThreadId = Thread.CurrentThread.ManagedThreadId;
		if (!fIsReentrant)
		{
			if (managedThreadId == writeLockOwnerId)
			{
				throw new LockRecursionException(global::SR.GetString("A read lock may not be acquired with the write lock held in this mode."));
			}
			EnterMyLock();
			readerWriterCount = GetThreadRWCount(dontAllocate: false);
			if (readerWriterCount.readercount > 0)
			{
				ExitMyLock();
				throw new LockRecursionException(global::SR.GetString("Recursive read lock acquisitions not allowed in this mode."));
			}
			if (managedThreadId == upgradeLockOwnerId)
			{
				readerWriterCount.readercount++;
				owners++;
				ExitMyLock();
				return true;
			}
		}
		else
		{
			EnterMyLock();
			readerWriterCount = GetThreadRWCount(dontAllocate: false);
			if (readerWriterCount.readercount > 0)
			{
				readerWriterCount.readercount++;
				ExitMyLock();
				return true;
			}
			if (managedThreadId == upgradeLockOwnerId)
			{
				readerWriterCount.readercount++;
				owners++;
				ExitMyLock();
				fUpgradeThreadHoldingRead = true;
				return true;
			}
			if (managedThreadId == writeLockOwnerId)
			{
				readerWriterCount.readercount++;
				owners++;
				ExitMyLock();
				return true;
			}
		}
		bool flag = true;
		int num = 0;
		while (true)
		{
			if (owners < 268435454)
			{
				owners++;
				readerWriterCount.readercount++;
				ExitMyLock();
				return flag;
			}
			if (num < 20)
			{
				ExitMyLock();
				if (timeout.IsExpired)
				{
					return false;
				}
				num++;
				SpinWait(num);
				EnterMyLock();
				if (IsRwHashEntryChanged(readerWriterCount))
				{
					readerWriterCount = GetThreadRWCount(dontAllocate: false);
				}
			}
			else if (readEvent == null)
			{
				LazyCreateEvent(ref readEvent, makeAutoResetEvent: false);
				if (IsRwHashEntryChanged(readerWriterCount))
				{
					readerWriterCount = GetThreadRWCount(dontAllocate: false);
				}
			}
			else
			{
				flag = WaitOnEvent(readEvent, ref numReadWaiters, timeout, isWriteWaiter: false);
				if (!flag)
				{
					break;
				}
				if (IsRwHashEntryChanged(readerWriterCount))
				{
					readerWriterCount = GetThreadRWCount(dontAllocate: false);
				}
			}
		}
		return false;
	}

	/// <summary>Tries to enter the lock in write mode.</summary>
	/// <exception cref="T:System.Threading.LockRecursionException">The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" /> and the current thread has already entered the lock in any mode. -or-The current thread has entered read mode, so trying to enter the lock in write mode would create the possibility of a deadlock. -or-The recursion number would exceed the capacity of the counter. The limit is so large that applications should never encounter it.</exception>
	public void EnterWriteLock()
	{
		TryEnterWriteLock(-1);
	}

	/// <summary>Tries to enter the lock in write mode, with an optional time-out.</summary>
	/// <returns>true if the calling thread entered write mode, otherwise, false.</returns>
	/// <param name="timeout">The interval to wait, or -1 milliseconds to wait indefinitely.</param>
	/// <exception cref="T:System.Threading.LockRecursionException">The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" /> and the current thread has already entered the lock. -or-The current thread initially entered the lock in read mode, and therefore trying to enter write mode would create the possibility of a deadlock. -or-The recursion number would exceed the capacity of the counter. The limit is so large that applications should never encounter it.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="timeout" /> is negative, but it is not equal to -1 milliseconds, which is the only negative value allowed.-or-The value of <paramref name="timeout" /> is greater than <see cref="F:System.Int32.MaxValue" /> milliseconds. </exception>
	public bool TryEnterWriteLock(TimeSpan timeout)
	{
		return TryEnterWriteLock(new TimeoutTracker(timeout));
	}

	/// <summary>Tries to enter the lock in write mode, with an optional time-out.</summary>
	/// <returns>true if the calling thread entered write mode, otherwise, false.</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or -1 (<see cref="F:System.Threading.Timeout.Infinite" />) to wait indefinitely.</param>
	/// <exception cref="T:System.Threading.LockRecursionException">The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" /> and the current thread has already entered the lock. -or-The current thread initially entered the lock in read mode, and therefore trying to enter write mode would create the possibility of a deadlock. -or-The recursion number would exceed the capacity of the counter. The limit is so large that applications should never encounter it.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="millisecondsTimeout" /> is negative, but it is not equal to <see cref="F:System.Threading.Timeout.Infinite" /> (-1), which is the only negative value allowed. </exception>
	public bool TryEnterWriteLock(int millisecondsTimeout)
	{
		return TryEnterWriteLock(new TimeoutTracker(millisecondsTimeout));
	}

	private bool TryEnterWriteLock(TimeoutTracker timeout)
	{
		return TryEnterWriteLockCore(timeout);
	}

	private bool TryEnterWriteLockCore(TimeoutTracker timeout)
	{
		if (fDisposed)
		{
			throw new ObjectDisposedException(null);
		}
		int managedThreadId = Thread.CurrentThread.ManagedThreadId;
		bool flag = false;
		ReaderWriterCount threadRWCount;
		if (!fIsReentrant)
		{
			if (managedThreadId == writeLockOwnerId)
			{
				throw new LockRecursionException(global::SR.GetString("Recursive write lock acquisitions not allowed in this mode."));
			}
			if (managedThreadId == upgradeLockOwnerId)
			{
				flag = true;
			}
			EnterMyLock();
			threadRWCount = GetThreadRWCount(dontAllocate: true);
			if (threadRWCount != null && threadRWCount.readercount > 0)
			{
				ExitMyLock();
				throw new LockRecursionException(global::SR.GetString("Write lock may not be acquired with read lock held. This pattern is prone to deadlocks. Please ensure that read locks are released before taking a write lock. If an upgrade is necessary, use an upgrade lock in place of the read lock."));
			}
		}
		else
		{
			EnterMyLock();
			threadRWCount = GetThreadRWCount(dontAllocate: false);
			if (managedThreadId == writeLockOwnerId)
			{
				threadRWCount.writercount++;
				ExitMyLock();
				return true;
			}
			if (managedThreadId == upgradeLockOwnerId)
			{
				flag = true;
			}
			else if (threadRWCount.readercount > 0)
			{
				ExitMyLock();
				throw new LockRecursionException(global::SR.GetString("Write lock may not be acquired with read lock held. This pattern is prone to deadlocks. Please ensure that read locks are released before taking a write lock. If an upgrade is necessary, use an upgrade lock in place of the read lock."));
			}
		}
		int num = 0;
		while (true)
		{
			if (IsWriterAcquired())
			{
				SetWriterAcquired();
				break;
			}
			if (flag)
			{
				uint numReaders = GetNumReaders();
				if (numReaders == 1)
				{
					SetWriterAcquired();
					break;
				}
				if (numReaders == 2 && threadRWCount != null)
				{
					if (IsRwHashEntryChanged(threadRWCount))
					{
						threadRWCount = GetThreadRWCount(dontAllocate: false);
					}
					if (threadRWCount.readercount > 0)
					{
						SetWriterAcquired();
						break;
					}
				}
			}
			if (num < 20)
			{
				ExitMyLock();
				if (timeout.IsExpired)
				{
					return false;
				}
				num++;
				SpinWait(num);
				EnterMyLock();
			}
			else if (flag)
			{
				if (waitUpgradeEvent == null)
				{
					LazyCreateEvent(ref waitUpgradeEvent, makeAutoResetEvent: true);
				}
				else if (!WaitOnEvent(waitUpgradeEvent, ref numWriteUpgradeWaiters, timeout, isWriteWaiter: true))
				{
					return false;
				}
			}
			else if (writeEvent == null)
			{
				LazyCreateEvent(ref writeEvent, makeAutoResetEvent: true);
			}
			else if (!WaitOnEvent(writeEvent, ref numWriteWaiters, timeout, isWriteWaiter: true))
			{
				return false;
			}
		}
		if (fIsReentrant)
		{
			if (IsRwHashEntryChanged(threadRWCount))
			{
				threadRWCount = GetThreadRWCount(dontAllocate: false);
			}
			threadRWCount.writercount++;
		}
		ExitMyLock();
		writeLockOwnerId = managedThreadId;
		return true;
	}

	/// <summary>Tries to enter the lock in upgradeable mode.</summary>
	/// <exception cref="T:System.Threading.LockRecursionException">The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" /> and the current thread has already entered the lock in any mode. -or-The current thread has entered read mode, so trying to enter upgradeable mode would create the possibility of a deadlock. -or-The recursion number would exceed the capacity of the counter. The limit is so large that applications should never encounter it.</exception>
	public void EnterUpgradeableReadLock()
	{
		TryEnterUpgradeableReadLock(-1);
	}

	/// <summary>Tries to enter the lock in upgradeable mode, with an optional time-out.</summary>
	/// <returns>true if the calling thread entered upgradeable mode, otherwise, false.</returns>
	/// <param name="timeout">The interval to wait, or -1 milliseconds to wait indefinitely.</param>
	/// <exception cref="T:System.Threading.LockRecursionException">The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" /> and the current thread has already entered the lock. -or-The current thread initially entered the lock in read mode, and therefore trying to enter upgradeable mode would create the possibility of a deadlock. -or-The recursion number would exceed the capacity of the counter. The limit is so large that applications should never encounter it.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="timeout" /> is negative, but it is not equal to -1 milliseconds, which is the only negative value allowed.-or-The value of <paramref name="timeout" /> is greater than <see cref="F:System.Int32.MaxValue" /> milliseconds. </exception>
	public bool TryEnterUpgradeableReadLock(TimeSpan timeout)
	{
		return TryEnterUpgradeableReadLock(new TimeoutTracker(timeout));
	}

	/// <summary>Tries to enter the lock in upgradeable mode, with an optional time-out.</summary>
	/// <returns>true if the calling thread entered upgradeable mode, otherwise, false.</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait, or -1 (<see cref="F:System.Threading.Timeout.Infinite" />) to wait indefinitely.</param>
	/// <exception cref="T:System.Threading.LockRecursionException">The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" /> and the current thread has already entered the lock. -or-The current thread initially entered the lock in read mode, and therefore trying to enter upgradeable mode would create the possibility of a deadlock. -or-The recursion number would exceed the capacity of the counter. The limit is so large that applications should never encounter it.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="millisecondsTimeout" /> is negative, but it is not equal to <see cref="F:System.Threading.Timeout.Infinite" /> (-1), which is the only negative value allowed. </exception>
	public bool TryEnterUpgradeableReadLock(int millisecondsTimeout)
	{
		return TryEnterUpgradeableReadLock(new TimeoutTracker(millisecondsTimeout));
	}

	private bool TryEnterUpgradeableReadLock(TimeoutTracker timeout)
	{
		return TryEnterUpgradeableReadLockCore(timeout);
	}

	private bool TryEnterUpgradeableReadLockCore(TimeoutTracker timeout)
	{
		if (fDisposed)
		{
			throw new ObjectDisposedException(null);
		}
		int managedThreadId = Thread.CurrentThread.ManagedThreadId;
		ReaderWriterCount threadRWCount;
		if (!fIsReentrant)
		{
			if (managedThreadId == upgradeLockOwnerId)
			{
				throw new LockRecursionException(global::SR.GetString("Recursive upgradeable lock acquisitions not allowed in this mode."));
			}
			if (managedThreadId == writeLockOwnerId)
			{
				throw new LockRecursionException(global::SR.GetString("Upgradeable lock may not be acquired with write lock held in this mode. Acquiring Upgradeable lock gives the ability to read along with an option to upgrade to a writer."));
			}
			EnterMyLock();
			threadRWCount = GetThreadRWCount(dontAllocate: true);
			if (threadRWCount != null && threadRWCount.readercount > 0)
			{
				ExitMyLock();
				throw new LockRecursionException(global::SR.GetString("Upgradeable lock may not be acquired with read lock held."));
			}
		}
		else
		{
			EnterMyLock();
			threadRWCount = GetThreadRWCount(dontAllocate: false);
			if (managedThreadId == upgradeLockOwnerId)
			{
				threadRWCount.upgradecount++;
				ExitMyLock();
				return true;
			}
			if (managedThreadId == writeLockOwnerId)
			{
				owners++;
				upgradeLockOwnerId = managedThreadId;
				threadRWCount.upgradecount++;
				if (threadRWCount.readercount > 0)
				{
					fUpgradeThreadHoldingRead = true;
				}
				ExitMyLock();
				return true;
			}
			if (threadRWCount.readercount > 0)
			{
				ExitMyLock();
				throw new LockRecursionException(global::SR.GetString("Upgradeable lock may not be acquired with read lock held."));
			}
		}
		int num = 0;
		while (true)
		{
			if (upgradeLockOwnerId == -1 && owners < 268435454)
			{
				owners++;
				upgradeLockOwnerId = managedThreadId;
				if (fIsReentrant)
				{
					if (IsRwHashEntryChanged(threadRWCount))
					{
						threadRWCount = GetThreadRWCount(dontAllocate: false);
					}
					threadRWCount.upgradecount++;
				}
				break;
			}
			if (num < 20)
			{
				ExitMyLock();
				if (timeout.IsExpired)
				{
					return false;
				}
				num++;
				SpinWait(num);
				EnterMyLock();
			}
			else if (upgradeEvent == null)
			{
				LazyCreateEvent(ref upgradeEvent, makeAutoResetEvent: true);
			}
			else if (!WaitOnEvent(upgradeEvent, ref numUpgradeWaiters, timeout, isWriteWaiter: false))
			{
				return false;
			}
		}
		ExitMyLock();
		return true;
	}

	/// <summary>Reduces the recursion count for read mode, and exits read mode if the resulting count is 0 (zero).</summary>
	/// <exception cref="T:System.Threading.SynchronizationLockException">The current thread has not entered the lock in read mode.</exception>
	public void ExitReadLock()
	{
		ReaderWriterCount readerWriterCount = null;
		EnterMyLock();
		readerWriterCount = GetThreadRWCount(dontAllocate: true);
		if (readerWriterCount == null || readerWriterCount.readercount < 1)
		{
			ExitMyLock();
			throw new SynchronizationLockException(global::SR.GetString("The read lock is being released without being held."));
		}
		if (fIsReentrant)
		{
			if (readerWriterCount.readercount > 1)
			{
				readerWriterCount.readercount--;
				ExitMyLock();
				return;
			}
			if (Thread.CurrentThread.ManagedThreadId == upgradeLockOwnerId)
			{
				fUpgradeThreadHoldingRead = false;
			}
		}
		owners--;
		readerWriterCount.readercount--;
		ExitAndWakeUpAppropriateWaiters();
	}

	/// <summary>Reduces the recursion count for write mode, and exits write mode if the resulting count is 0 (zero).</summary>
	/// <exception cref="T:System.Threading.SynchronizationLockException">The current thread has not entered the lock in write mode.</exception>
	public void ExitWriteLock()
	{
		if (!fIsReentrant)
		{
			if (Thread.CurrentThread.ManagedThreadId != writeLockOwnerId)
			{
				throw new SynchronizationLockException(global::SR.GetString("The write lock is being released without being held."));
			}
			EnterMyLock();
		}
		else
		{
			EnterMyLock();
			ReaderWriterCount threadRWCount = GetThreadRWCount(dontAllocate: false);
			if (threadRWCount == null)
			{
				ExitMyLock();
				throw new SynchronizationLockException(global::SR.GetString("The write lock is being released without being held."));
			}
			if (threadRWCount.writercount < 1)
			{
				ExitMyLock();
				throw new SynchronizationLockException(global::SR.GetString("The write lock is being released without being held."));
			}
			threadRWCount.writercount--;
			if (threadRWCount.writercount > 0)
			{
				ExitMyLock();
				return;
			}
		}
		ClearWriterAcquired();
		writeLockOwnerId = -1;
		ExitAndWakeUpAppropriateWaiters();
	}

	/// <summary>Reduces the recursion count for upgradeable mode, and exits upgradeable mode if the resulting count is 0 (zero).</summary>
	/// <exception cref="T:System.Threading.SynchronizationLockException">The current thread has not entered the lock in upgradeable mode.</exception>
	public void ExitUpgradeableReadLock()
	{
		if (!fIsReentrant)
		{
			if (Thread.CurrentThread.ManagedThreadId != upgradeLockOwnerId)
			{
				throw new SynchronizationLockException(global::SR.GetString("The upgradeable lock is being released without being held."));
			}
			EnterMyLock();
		}
		else
		{
			EnterMyLock();
			ReaderWriterCount threadRWCount = GetThreadRWCount(dontAllocate: true);
			if (threadRWCount == null)
			{
				ExitMyLock();
				throw new SynchronizationLockException(global::SR.GetString("The upgradeable lock is being released without being held."));
			}
			if (threadRWCount.upgradecount < 1)
			{
				ExitMyLock();
				throw new SynchronizationLockException(global::SR.GetString("The upgradeable lock is being released without being held."));
			}
			threadRWCount.upgradecount--;
			if (threadRWCount.upgradecount > 0)
			{
				ExitMyLock();
				return;
			}
			fUpgradeThreadHoldingRead = false;
		}
		owners--;
		upgradeLockOwnerId = -1;
		ExitAndWakeUpAppropriateWaiters();
	}

	private void LazyCreateEvent(ref EventWaitHandle waitEvent, bool makeAutoResetEvent)
	{
		ExitMyLock();
		EventWaitHandle eventWaitHandle = ((!makeAutoResetEvent) ? ((EventWaitHandle)new ManualResetEvent(initialState: false)) : ((EventWaitHandle)new AutoResetEvent(initialState: false)));
		EnterMyLock();
		if (waitEvent == null)
		{
			waitEvent = eventWaitHandle;
		}
		else
		{
			eventWaitHandle.Close();
		}
	}

	private bool WaitOnEvent(EventWaitHandle waitEvent, ref uint numWaiters, TimeoutTracker timeout, bool isWriteWaiter)
	{
		waitEvent.Reset();
		numWaiters++;
		fNoWaiters = false;
		if (numWriteWaiters == 1)
		{
			SetWritersWaiting();
		}
		if (numWriteUpgradeWaiters == 1)
		{
			SetUpgraderWaiting();
		}
		bool flag = false;
		ExitMyLock();
		try
		{
			flag = waitEvent.WaitOne(timeout.RemainingMilliseconds);
		}
		finally
		{
			EnterMyLock();
			numWaiters--;
			if (numWriteWaiters == 0 && numWriteUpgradeWaiters == 0 && numUpgradeWaiters == 0 && numReadWaiters == 0)
			{
				fNoWaiters = true;
			}
			if (numWriteWaiters == 0)
			{
				ClearWritersWaiting();
			}
			if (numWriteUpgradeWaiters == 0)
			{
				ClearUpgraderWaiting();
			}
			if (!flag)
			{
				if (isWriteWaiter)
				{
					ExitAndWakeUpAppropriateReadWaiters();
				}
				else
				{
					ExitMyLock();
				}
			}
		}
		return flag;
	}

	private void ExitAndWakeUpAppropriateWaiters()
	{
		if (fNoWaiters)
		{
			ExitMyLock();
		}
		else
		{
			ExitAndWakeUpAppropriateWaitersPreferringWriters();
		}
	}

	private void ExitAndWakeUpAppropriateWaitersPreferringWriters()
	{
		uint numReaders = GetNumReaders();
		if (fIsReentrant && numWriteUpgradeWaiters != 0 && fUpgradeThreadHoldingRead && numReaders == 2)
		{
			ExitMyLock();
			waitUpgradeEvent.Set();
		}
		else if (numReaders == 1 && numWriteUpgradeWaiters != 0)
		{
			ExitMyLock();
			waitUpgradeEvent.Set();
		}
		else if (numReaders == 0 && numWriteWaiters != 0)
		{
			ExitMyLock();
			writeEvent.Set();
		}
		else
		{
			ExitAndWakeUpAppropriateReadWaiters();
		}
	}

	private void ExitAndWakeUpAppropriateReadWaiters()
	{
		if (numWriteWaiters != 0 || numWriteUpgradeWaiters != 0 || fNoWaiters)
		{
			ExitMyLock();
			return;
		}
		bool flag = numReadWaiters != 0;
		bool num = numUpgradeWaiters != 0 && upgradeLockOwnerId == -1;
		ExitMyLock();
		if (flag)
		{
			readEvent.Set();
		}
		if (num)
		{
			upgradeEvent.Set();
		}
	}

	private bool IsWriterAcquired()
	{
		return (owners & 0xBFFFFFFFu) == 0;
	}

	private void SetWriterAcquired()
	{
		owners |= 2147483648u;
	}

	private void ClearWriterAcquired()
	{
		owners &= 2147483647u;
	}

	private void SetWritersWaiting()
	{
		owners |= 1073741824u;
	}

	private void ClearWritersWaiting()
	{
		owners &= 3221225471u;
	}

	private void SetUpgraderWaiting()
	{
		owners |= 536870912u;
	}

	private void ClearUpgraderWaiting()
	{
		owners &= 3758096383u;
	}

	private uint GetNumReaders()
	{
		return owners & 0xFFFFFFF;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnterMyLock()
	{
		if (Interlocked.CompareExchange(ref myLock, 1, 0) != 0)
		{
			EnterMyLockSpin();
		}
	}

	private void EnterMyLockSpin()
	{
		int processorCount = Environment.ProcessorCount;
		int num = 0;
		while (true)
		{
			if (num < 10 && processorCount > 1)
			{
				Thread.SpinWait(20 * (num + 1));
			}
			else if (num < 15)
			{
				Thread.Sleep(0);
			}
			else
			{
				Thread.Sleep(1);
			}
			if (myLock == 0 && Interlocked.CompareExchange(ref myLock, 1, 0) == 0)
			{
				break;
			}
			num++;
		}
	}

	private void ExitMyLock()
	{
		Volatile.Write(ref myLock, 0);
	}

	private static void SpinWait(int SpinCount)
	{
		if (SpinCount < 5 && Environment.ProcessorCount > 1)
		{
			Thread.SpinWait(20 * SpinCount);
		}
		else if (SpinCount < 17)
		{
			Thread.Sleep(0);
		}
		else
		{
			Thread.Sleep(1);
		}
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.ReaderWriterLockSlim" /> class.</summary>
	/// <filterpriority>2</filterpriority>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	private void Dispose(bool disposing)
	{
		if (disposing && !fDisposed)
		{
			if (WaitingReadCount > 0 || WaitingUpgradeCount > 0 || WaitingWriteCount > 0)
			{
				throw new SynchronizationLockException(global::SR.GetString("The lock is being disposed while still being used. It either is being held by a thread and/or has active waiters waiting to acquire the lock."));
			}
			if (IsReadLockHeld || IsUpgradeableReadLockHeld || IsWriteLockHeld)
			{
				throw new SynchronizationLockException(global::SR.GetString("The lock is being disposed while still being used. It either is being held by a thread and/or has active waiters waiting to acquire the lock."));
			}
			if (writeEvent != null)
			{
				writeEvent.Close();
				writeEvent = null;
			}
			if (readEvent != null)
			{
				readEvent.Close();
				readEvent = null;
			}
			if (upgradeEvent != null)
			{
				upgradeEvent.Close();
				upgradeEvent = null;
			}
			if (waitUpgradeEvent != null)
			{
				waitUpgradeEvent.Close();
				waitUpgradeEvent = null;
			}
			fDisposed = true;
		}
	}
}
