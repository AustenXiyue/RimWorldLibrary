using MS.Win32.Penimc;

namespace System.Windows.Input;

internal sealed class PenThread
{
	private PenThreadWorker _penThreadWorker;

	internal PenThread()
	{
		_penThreadWorker = new PenThreadWorker();
	}

	internal void Dispose()
	{
		DisposeHelper();
	}

	~PenThread()
	{
		DisposeHelper();
	}

	private void DisposeHelper()
	{
		if (_penThreadWorker != null)
		{
			_penThreadWorker.Dispose();
		}
		GC.KeepAlive(this);
	}

	internal bool AddPenContext(PenContext penContext)
	{
		return _penThreadWorker.WorkerAddPenContext(penContext);
	}

	internal bool RemovePenContext(PenContext penContext)
	{
		return _penThreadWorker.WorkerRemovePenContext(penContext);
	}

	internal TabletDeviceInfo[] WorkerGetTabletsInfo()
	{
		return _penThreadWorker.WorkerGetTabletsInfo();
	}

	internal PenContextInfo WorkerCreateContext(nint hwnd, IPimcTablet3 pimcTablet)
	{
		return _penThreadWorker.WorkerCreateContext(hwnd, pimcTablet);
	}

	internal bool WorkerAcquireTabletLocks(IPimcTablet3 tablet, uint wispTabletKey)
	{
		return _penThreadWorker.WorkerAcquireTabletLocks(tablet, wispTabletKey);
	}

	internal bool WorkerReleaseTabletLocks(IPimcTablet3 tablet, uint wispTabletKey)
	{
		return _penThreadWorker.WorkerReleaseTabletLocks(tablet, wispTabletKey);
	}

	internal StylusDeviceInfo[] WorkerRefreshCursorInfo(IPimcTablet3 pimcTablet)
	{
		return _penThreadWorker.WorkerRefreshCursorInfo(pimcTablet);
	}

	internal TabletDeviceInfo WorkerGetTabletInfo(uint index)
	{
		return _penThreadWorker.WorkerGetTabletInfo(index);
	}

	internal TabletDeviceSizeInfo WorkerGetUpdatedSizes(IPimcTablet3 pimcTablet)
	{
		return _penThreadWorker.WorkerGetUpdatedSizes(pimcTablet);
	}
}
