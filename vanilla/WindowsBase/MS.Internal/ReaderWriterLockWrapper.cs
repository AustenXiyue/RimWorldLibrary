using System;
using System.Threading;
using MS.Internal.WindowsBase;
using MS.Win32;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal class ReaderWriterLockWrapper
{
	private struct AutoWriterRelease : IDisposable
	{
		private ReaderWriterLockWrapper _wrapper;

		public AutoWriterRelease(ReaderWriterLockWrapper wrapper)
		{
			_wrapper = wrapper;
		}

		public void Dispose()
		{
			_wrapper.ReleaseWriterLock();
		}
	}

	private struct AutoReaderRelease : IDisposable
	{
		private ReaderWriterLockWrapper _wrapper;

		public AutoReaderRelease(ReaderWriterLockWrapper wrapper)
		{
			_wrapper = wrapper;
		}

		public void Dispose()
		{
			_wrapper.ReleaseReaderLock();
		}
	}

	private class AutoWriterReleaseClass : IDisposable
	{
		private ReaderWriterLockWrapper _wrapper;

		public AutoWriterReleaseClass(ReaderWriterLockWrapper wrapper)
		{
			_wrapper = wrapper;
		}

		public void Dispose()
		{
			_wrapper.ReleaseWriterLock2();
		}
	}

	private class AutoReaderReleaseClass : IDisposable
	{
		private ReaderWriterLockWrapper _wrapper;

		public AutoReaderReleaseClass(ReaderWriterLockWrapper wrapper)
		{
			_wrapper = wrapper;
		}

		public void Dispose()
		{
			_wrapper.ReleaseReaderLock2();
		}
	}

	private class NonPumpingSynchronizationContext : SynchronizationContext
	{
		public SynchronizationContext Parent { get; set; }

		public NonPumpingSynchronizationContext()
		{
			SetWaitNotificationRequired();
		}

		public override int Wait(nint[] waitHandles, bool waitAll, int millisecondsTimeout)
		{
			return UnsafeNativeMethods.WaitForMultipleObjectsEx(waitHandles.Length, waitHandles, waitAll, millisecondsTimeout, bAlertable: false);
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			Parent.Send(d, state);
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			Parent.Post(d, state);
		}

		public override SynchronizationContext CreateCopy()
		{
			return this;
		}
	}

	private ReaderWriterLockSlim _rwLock;

	private AutoReaderRelease _arr;

	private AutoWriterRelease _awr;

	private AutoReaderReleaseClass _arrc;

	private AutoWriterReleaseClass _awrc;

	private Action _enterReadAction;

	private Action _exitReadAction;

	private Action _enterWriteAction;

	private Action _exitWriteAction;

	private NonPumpingSynchronizationContext _defaultSynchronizationContext;

	internal IDisposable WriteLock
	{
		get
		{
			if (!BaseAppContextSwitches.EnableWeakEventMemoryImprovements)
			{
				CallWithNonPumpingWait(delegate
				{
					_rwLock.EnterWriteLock();
				});
				return _awr;
			}
			CallWithNonPumpingWait(_enterWriteAction);
			return _awrc;
		}
	}

	internal IDisposable ReadLock
	{
		get
		{
			if (!BaseAppContextSwitches.EnableWeakEventMemoryImprovements)
			{
				CallWithNonPumpingWait(delegate
				{
					_rwLock.EnterReadLock();
				});
				return _arr;
			}
			CallWithNonPumpingWait(_enterReadAction);
			return _arrc;
		}
	}

	internal ReaderWriterLockWrapper()
	{
		_rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		_defaultSynchronizationContext = new NonPumpingSynchronizationContext();
		Initialize(!BaseAppContextSwitches.EnableWeakEventMemoryImprovements);
	}

	private void Initialize(bool useLegacyMemoryBehavior)
	{
		if (useLegacyMemoryBehavior)
		{
			_awr = new AutoWriterRelease(this);
			_arr = new AutoReaderRelease(this);
			return;
		}
		_awrc = new AutoWriterReleaseClass(this);
		_arrc = new AutoReaderReleaseClass(this);
		_enterReadAction = _rwLock.EnterReadLock;
		_exitReadAction = _rwLock.ExitReadLock;
		_enterWriteAction = _rwLock.EnterWriteLock;
		_exitWriteAction = _rwLock.ExitWriteLock;
	}

	private void ReleaseWriterLock()
	{
		CallWithNonPumpingWait(delegate
		{
			_rwLock.ExitWriteLock();
		});
	}

	private void ReleaseReaderLock()
	{
		CallWithNonPumpingWait(delegate
		{
			_rwLock.ExitReadLock();
		});
	}

	private void ReleaseWriterLock2()
	{
		CallWithNonPumpingWait(_exitWriteAction);
	}

	private void ReleaseReaderLock2()
	{
		CallWithNonPumpingWait(_exitReadAction);
	}

	private void CallWithNonPumpingWait(Action callback)
	{
		SynchronizationContext current = SynchronizationContext.Current;
		NonPumpingSynchronizationContext nonPumpingSynchronizationContext = Interlocked.Exchange(ref _defaultSynchronizationContext, null);
		bool flag = nonPumpingSynchronizationContext != null;
		if (!flag)
		{
			nonPumpingSynchronizationContext = new NonPumpingSynchronizationContext();
		}
		try
		{
			nonPumpingSynchronizationContext.Parent = current;
			SynchronizationContext.SetSynchronizationContext(nonPumpingSynchronizationContext);
			callback();
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(current);
			if (flag)
			{
				Interlocked.Exchange(ref _defaultSynchronizationContext, nonPumpingSynchronizationContext);
			}
		}
	}
}
