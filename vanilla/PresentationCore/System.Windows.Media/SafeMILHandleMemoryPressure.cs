using System.Threading;

namespace System.Windows.Media;

internal class SafeMILHandleMemoryPressure
{
	private long _gcPressure;

	private int _refCount;

	internal SafeMILHandleMemoryPressure(long gcPressure)
	{
		_gcPressure = gcPressure;
		_refCount = 0;
		GC.AddMemoryPressure(_gcPressure);
	}

	internal void AddRef()
	{
		Interlocked.Increment(ref _refCount);
	}

	internal void Release()
	{
		if (Interlocked.Decrement(ref _refCount) == 0)
		{
			GC.RemoveMemoryPressure(_gcPressure);
			_gcPressure = 0L;
		}
	}
}
