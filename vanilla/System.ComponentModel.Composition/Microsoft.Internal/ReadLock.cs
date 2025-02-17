using System;
using System.Threading;

namespace Microsoft.Internal;

internal struct ReadLock : IDisposable
{
	private readonly Lock _lock;

	private int _isDisposed;

	public ReadLock(Lock @lock)
	{
		_isDisposed = 0;
		_lock = @lock;
		_lock.EnterReadLock();
	}

	public void Dispose()
	{
		if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
		{
			_lock.ExitReadLock();
		}
	}
}
