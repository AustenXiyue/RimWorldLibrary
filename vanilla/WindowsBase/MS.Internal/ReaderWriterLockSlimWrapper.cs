using System;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;

namespace MS.Internal;

internal class ReaderWriterLockSlimWrapper : IDisposable
{
	private ReaderWriterLockSlim _rwLock;

	private readonly LockRecursionPolicy _lockRecursionPolicy;

	private readonly bool _disableDispatcherProcessingWhenNoRecursion;

	private bool _disposed;

	internal ReaderWriterLockSlimWrapper(LockRecursionPolicy recursionPolicy = LockRecursionPolicy.NoRecursion, bool disableDispatcherProcessingWhenNoRecursion = true)
	{
		_lockRecursionPolicy = recursionPolicy;
		_disableDispatcherProcessingWhenNoRecursion = disableDispatcherProcessingWhenNoRecursion;
		_rwLock = new ReaderWriterLockSlim(_lockRecursionPolicy);
		_disposed = false;
		GC.SuppressFinalize(this);
	}

	internal bool WithReadLock(Action criticalAction)
	{
		object result = null;
		return ExecuteWithinLockInternal(_rwLock.EnterReadLock, _rwLock.ExitReadLock, ref result, criticalAction, null);
	}

	internal bool WithReadLock<T>(Action<T> criticalAction, T arg)
	{
		object result = null;
		return ExecuteWithinLockInternal(_rwLock.EnterReadLock, _rwLock.ExitReadLock, ref result, criticalAction, arg);
	}

	internal bool WithReadLock<T, TResult>(Func<T, TResult> criticalAction, T arg, out TResult result)
	{
		object result2 = null;
		bool flag = false;
		try
		{
			flag = ExecuteWithinLockInternal(_rwLock.EnterReadLock, _rwLock.ExitReadLock, ref result2, criticalAction, arg);
		}
		finally
		{
			result = (flag ? ((TResult)result2) : default(TResult));
		}
		return flag;
	}

	internal bool WithReadLock<T1, T2, TResult>(Func<T1, T2, TResult> criticalAction, T1 arg1, T2 arg2, out TResult result)
	{
		object result2 = null;
		bool flag = false;
		try
		{
			flag = ExecuteWithinLockInternal(_rwLock.EnterReadLock, _rwLock.ExitReadLock, ref result2, criticalAction, arg1, arg2);
		}
		finally
		{
			result = (flag ? ((TResult)result2) : default(TResult));
		}
		return flag;
	}

	internal bool WithWriteLock(Action criticalAction)
	{
		object result = null;
		return ExecuteWithinLockInternal(_rwLock.EnterWriteLock, _rwLock.ExitWriteLock, ref result, criticalAction, null);
	}

	internal bool WithWriteLock<T>(Action<T> criticalAction, T arg)
	{
		object result = null;
		return ExecuteWithinLockInternal(_rwLock.EnterWriteLock, _rwLock.ExitWriteLock, ref result, criticalAction, arg);
	}

	internal bool WithWriteLock<T, TResult>(Func<T, TResult> criticalAction, T arg, out TResult result)
	{
		bool flag = false;
		object result2 = null;
		try
		{
			flag = ExecuteWithinLockInternal(_rwLock.EnterWriteLock, _rwLock.ExitWriteLock, ref result2, criticalAction, arg);
		}
		finally
		{
			result = (flag ? ((TResult)result2) : default(TResult));
		}
		return flag;
	}

	internal bool WithWriteLock<T1, T2, TResult>(Func<T1, T2, TResult> criticalAction, T1 arg1, T2 arg2, out TResult result)
	{
		bool flag = false;
		object result2 = null;
		try
		{
			flag = ExecuteWithinLockInternal(_rwLock.EnterWriteLock, _rwLock.ExitWriteLock, ref result2, criticalAction, arg1, arg2);
		}
		finally
		{
			result = (flag ? ((TResult)result2) : default(TResult));
		}
		return flag;
	}

	private bool ExecuteWithinLockInternal(Action lockAcquire, Action lockRelease, ref object result, Delegate criticalAction, params object[] args)
	{
		if ((object)criticalAction == null)
		{
			throw new ArgumentNullException("criticalAction");
		}
		bool flag = false;
		DispatcherProcessingDisabled? dispatcherProcessingDisabled = null;
		if (_lockRecursionPolicy == LockRecursionPolicy.NoRecursion && _disableDispatcherProcessingWhenNoRecursion)
		{
			dispatcherProcessingDisabled = Dispatcher.FromThread(Thread.CurrentThread)?.DisableProcessing();
		}
		try
		{
			lockAcquire();
			flag = true;
		}
		catch (Exception ex) when (ex is ObjectDisposedException || ex is LockRecursionException)
		{
		}
		finally
		{
			try
			{
				if (flag)
				{
					result = criticalAction.DynamicInvoke(args);
				}
			}
			catch (TargetInvocationException ex2) when (ex2.InnerException != null)
			{
				throw ex2.InnerException;
			}
			finally
			{
				try
				{
					if (flag)
					{
						lockRelease();
					}
				}
				finally
				{
					if (dispatcherProcessingDisabled.HasValue)
					{
						dispatcherProcessingDisabled.Value.Dispose();
					}
				}
			}
		}
		return flag;
	}

	~ReaderWriterLockSlimWrapper()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("ReaderWriterLockSlimWrapper");
		}
		try
		{
			_rwLock.Dispose();
			_disposed = true;
			_rwLock = null;
		}
		catch (SynchronizationLockException) when (disposing)
		{
		}
		finally
		{
			if (!_disposed && disposing)
			{
				GC.ReRegisterForFinalize(this);
			}
		}
	}
}
