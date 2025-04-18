using System;
using System.Threading;

namespace Microsoft.Internal;

internal struct WriteLock : IDisposable
{
	private readonly Lock _lock;

	private int _isDisposed;

	public WriteLock(Lock @lock)
	{
		_isDisposed = 0;
		_lock = @lock;
		_lock.EnterWriteLock();
	}

	public void Dispose()
	{
		if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
		{
			_lock.ExitWriteLock();
		}
	}
}
