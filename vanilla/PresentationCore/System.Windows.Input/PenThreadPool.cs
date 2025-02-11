using System.Collections.Generic;
using System.Windows.Input.Tracing;

namespace System.Windows.Input;

internal class PenThreadPool
{
	private const int MAX_PENTHREAD_RETRIES = 10;

	[ThreadStatic]
	private static PenThreadPool _penThreadPool;

	private List<WeakReference<PenThread>> _penThreadWeakRefList;

	static PenThreadPool()
	{
	}

	internal static PenThread GetPenThreadForPenContext(PenContext penContext)
	{
		if (_penThreadPool == null)
		{
			_penThreadPool = new PenThreadPool();
		}
		return _penThreadPool.GetPenThreadForPenContextHelper(penContext);
	}

	internal PenThreadPool()
	{
		_penThreadWeakRefList = new List<WeakReference<PenThread>>();
	}

	private PenThread GetPenThreadForPenContextHelper(PenContext penContext)
	{
		List<PenThread> list = new List<PenThread>();
		PenThread penThread = null;
		while (list.Count < 10)
		{
			for (int num = _penThreadWeakRefList.Count - 1; num >= 0; num--)
			{
				PenThread target = null;
				if (_penThreadWeakRefList[num].TryGetTarget(out target) && !list.Contains(target))
				{
					penThread = target;
				}
				else if (target == null)
				{
					_penThreadWeakRefList.RemoveAt(num);
				}
			}
			if (penThread == null)
			{
				penThread = new PenThread();
				_penThreadWeakRefList.Add(new WeakReference<PenThread>(penThread));
			}
			if (penContext == null || penThread.AddPenContext(penContext))
			{
				break;
			}
			list.Add(penThread);
			penThread = null;
			StylusTraceLogger.LogReentrancy("GetPenThreadForPenContextHelper");
		}
		if (penThread == null)
		{
			StylusTraceLogger.LogReentrancyRetryLimitReached();
		}
		return penThread;
	}
}
