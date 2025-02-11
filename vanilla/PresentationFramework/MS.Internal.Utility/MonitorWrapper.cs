using System;
using System.Threading;

namespace MS.Internal.Utility;

internal class MonitorWrapper
{
	private class MonitorHelper : IDisposable
	{
		private MonitorWrapper _monitorWrapper;

		public MonitorHelper(MonitorWrapper monitorWrapper)
		{
			_monitorWrapper = monitorWrapper;
		}

		public void Dispose()
		{
			if (_monitorWrapper != null)
			{
				_monitorWrapper.Exit();
				_monitorWrapper = null;
			}
			GC.SuppressFinalize(this);
		}
	}

	private int _enterCount;

	private object _syncRoot = new object();

	public bool Busy => _enterCount > 0;

	public IDisposable Enter()
	{
		Monitor.Enter(_syncRoot);
		Interlocked.Increment(ref _enterCount);
		return new MonitorHelper(this);
	}

	public void Exit()
	{
		Invariant.Assert(Interlocked.Decrement(ref _enterCount) >= 0, "unmatched call to MonitorWrapper.Exit");
		Monitor.Exit(_syncRoot);
	}
}
