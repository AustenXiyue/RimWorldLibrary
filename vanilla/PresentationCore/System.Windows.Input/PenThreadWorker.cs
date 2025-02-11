using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.Penimc;

namespace System.Windows.Input;

internal sealed class PenThreadWorker
{
	private abstract class WorkerOperation
	{
		private AutoResetEvent _doneEvent;

		internal AutoResetEvent DoneEvent => _doneEvent;

		internal WorkerOperation()
		{
			_doneEvent = new AutoResetEvent(initialState: false);
		}

		internal void DoWork()
		{
			try
			{
				OnDoWork();
			}
			finally
			{
				_doneEvent.Set();
			}
		}

		protected abstract void OnDoWork();
	}

	private class WorkerOperationThreadStart : WorkerOperation
	{
		protected override void OnDoWork()
		{
		}
	}

	private class WorkerOperationGetTabletsInfo : WorkerOperation
	{
		private TabletDeviceInfo[] _tabletDevicesInfo = Array.Empty<TabletDeviceInfo>();

		internal TabletDeviceInfo[] TabletDevicesInfo => _tabletDevicesInfo;

		protected override void OnDoWork()
		{
			try
			{
				IPimcManager3 pimcManager = UnsafeNativeMethods.PimcManager;
				pimcManager.GetTabletCount(out var count);
				TabletDeviceInfo[] array = new TabletDeviceInfo[count];
				for (uint num = 0u; num < count; num++)
				{
					pimcManager.GetTablet(num, out var IPimcTablet);
					array[num] = GetTabletInfoHelper(IPimcTablet);
				}
				_tabletDevicesInfo = array;
			}
			catch (Exception e) when (IsKnownException(e))
			{
			}
		}
	}

	private class WorkerOperationCreateContext : WorkerOperation
	{
		private nint _hwnd;

		private IPimcTablet3 _pimcTablet;

		private PenContextInfo _result;

		internal PenContextInfo Result => _result;

		internal WorkerOperationCreateContext(nint hwnd, IPimcTablet3 pimcTablet)
		{
			_hwnd = hwnd;
			_pimcTablet = pimcTablet;
		}

		protected override void OnDoWork()
		{
			try
			{
				_pimcTablet.CreateContext(_hwnd, fEnable: true, 250u, out var IPimcContext, out var key, out var commHandle);
				PenContextInfo result = default(PenContextInfo);
				result.ContextId = key;
				result.PimcContext = new SecurityCriticalDataClass<IPimcContext3>(IPimcContext);
				result.CommHandle = new SecurityCriticalDataClass<nint>((IntPtr.Size == 4) ? new IntPtr((int)commHandle) : new IntPtr(commHandle));
				result.WispContextKey = UnsafeNativeMethods.QueryWispContextKey(IPimcContext);
				_result = result;
			}
			catch (Exception e) when (IsKnownException(e))
			{
			}
		}
	}

	private class WorkerOperationAcquireTabletLocks : WorkerOperation
	{
		private IPimcTablet3 _tablet;

		private uint _wispTabletKey;

		internal bool Result { get; private set; }

		internal WorkerOperationAcquireTabletLocks(IPimcTablet3 tablet, uint wispTabletKey)
		{
			_tablet = tablet;
			_wispTabletKey = wispTabletKey;
		}

		protected override void OnDoWork()
		{
			UnsafeNativeMethods.AcquireTabletExternalLock(_tablet);
			UnsafeNativeMethods.CheckedLockWispObjectFromGit(_wispTabletKey);
			Result = true;
		}
	}

	private class WorkerOperationReleaseTabletLocks : WorkerOperation
	{
		private IPimcTablet3 _tablet;

		private uint _wispTabletKey;

		internal bool Result { get; private set; }

		internal WorkerOperationReleaseTabletLocks(IPimcTablet3 tablet, uint wispTabletKey)
		{
			_tablet = tablet;
			_wispTabletKey = wispTabletKey;
		}

		protected override void OnDoWork()
		{
			UnsafeNativeMethods.CheckedUnlockWispObjectFromGit(_wispTabletKey);
			UnsafeNativeMethods.ReleaseTabletExternalLock(_tablet);
			Result = true;
		}
	}

	private class WorkerOperationRefreshCursorInfo : WorkerOperation
	{
		private IPimcTablet3 _pimcTablet;

		private StylusDeviceInfo[] _stylusDevicesInfo = Array.Empty<StylusDeviceInfo>();

		internal StylusDeviceInfo[] StylusDevicesInfo => _stylusDevicesInfo;

		internal WorkerOperationRefreshCursorInfo(IPimcTablet3 pimcTablet)
		{
			_pimcTablet = pimcTablet;
		}

		protected override void OnDoWork()
		{
			try
			{
				_pimcTablet.RefreshCursorInfo();
				_stylusDevicesInfo = GetStylusDevicesInfo(_pimcTablet);
			}
			catch (Exception e) when (IsKnownException(e))
			{
			}
		}
	}

	private class WorkerOperationGetTabletInfo : WorkerOperation
	{
		private uint _index;

		private TabletDeviceInfo _tabletDeviceInfo = new TabletDeviceInfo();

		internal TabletDeviceInfo TabletDeviceInfo => _tabletDeviceInfo;

		internal WorkerOperationGetTabletInfo(uint index)
		{
			_index = index;
		}

		protected override void OnDoWork()
		{
			try
			{
				UnsafeNativeMethods.PimcManager.GetTablet(_index, out var IPimcTablet);
				_tabletDeviceInfo = GetTabletInfoHelper(IPimcTablet);
			}
			catch (Exception e) when (IsKnownException(e))
			{
			}
		}
	}

	private class WorkerOperationWorkerGetUpdatedSizes : WorkerOperation
	{
		private IPimcTablet3 _pimcTablet;

		private TabletDeviceSizeInfo _tabletDeviceSizeInfo = new TabletDeviceSizeInfo(new Size(1.0, 1.0), new Size(1.0, 1.0));

		internal TabletDeviceSizeInfo TabletDeviceSizeInfo => _tabletDeviceSizeInfo;

		internal WorkerOperationWorkerGetUpdatedSizes(IPimcTablet3 pimcTablet)
		{
			_pimcTablet = pimcTablet;
		}

		protected override void OnDoWork()
		{
			try
			{
				_pimcTablet.GetTabletAndDisplaySize(out var tabletWidth, out var tabletHeight, out var displayWidth, out var displayHeight);
				_tabletDeviceSizeInfo = new TabletDeviceSizeInfo(new Size(tabletWidth, tabletHeight), new Size(displayWidth, displayHeight));
			}
			catch (Exception e) when (IsKnownException(e))
			{
			}
		}
	}

	private class WorkerOperationAddContext : WorkerOperation
	{
		private PenContext _newPenContext;

		private PenThreadWorker _penThreadWorker;

		private bool _result;

		internal bool Result => _result;

		internal WorkerOperationAddContext(PenContext penContext, PenThreadWorker penThreadWorker)
		{
			_newPenContext = penContext;
			_penThreadWorker = penThreadWorker;
		}

		protected override void OnDoWork()
		{
			_result = _penThreadWorker.AddPenContext(_newPenContext);
		}
	}

	private class WorkerOperationRemoveContext : WorkerOperation
	{
		private PenContext _penContextToRemove;

		private PenThreadWorker _penThreadWorker;

		private bool _result;

		internal bool Result => _result;

		internal WorkerOperationRemoveContext(PenContext penContext, PenThreadWorker penThreadWorker)
		{
			_penContextToRemove = penContext;
			_penThreadWorker = penThreadWorker;
		}

		protected override void OnDoWork()
		{
			_result = _penThreadWorker.RemovePenContext(_penContextToRemove);
		}
	}

	private const int PenEventNone = 0;

	private const int PenEventTimeout = 1;

	private const int PenEventPenInRange = 707;

	private const int PenEventPenOutOfRange = 708;

	private const int PenEventPenDown = 709;

	private const int PenEventPenUp = 710;

	private const int PenEventPackets = 711;

	private const int PenEventSystem = 714;

	private const int MaxContextPerThread = 31;

	private const int EventsFrequency = 8;

	private nint[] _handles = Array.Empty<nint>();

	private WeakReference[] _penContexts = Array.Empty<WeakReference>();

	private IPimcContext3[] _pimcContexts = Array.Empty<IPimcContext3>();

	private uint[] _wispContextKeys = Array.Empty<uint>();

	private SecurityCriticalData<nint> _pimcResetHandle;

	private volatile bool __disposed;

	private List<WorkerOperation> _workerOperation = new List<WorkerOperation>();

	private object _workerOperationLock = new object();

	private PenContext _cachedMovePenContext;

	private int _cachedMoveStylusPointerId;

	private int _cachedMoveStartTimestamp;

	private int[] _cachedMoveData;

	internal PenThreadWorker()
	{
		UnsafeNativeMethods.CreateResetEvent(out var handle);
		_pimcResetHandle = new SecurityCriticalData<nint>(handle);
		WorkerOperationThreadStart workerOperationThreadStart = new WorkerOperationThreadStart();
		lock (_workerOperationLock)
		{
			_workerOperation.Add(workerOperationThreadStart);
		}
		Thread thread = new Thread(ThreadProc);
		thread.IsBackground = true;
		thread.Start();
		workerOperationThreadStart.DoneEvent.WaitOne();
		workerOperationThreadStart.DoneEvent.Close();
	}

	internal void Dispose()
	{
		if (!__disposed)
		{
			__disposed = true;
			UnsafeNativeMethods.RaiseResetEvent(_pimcResetHandle.Value);
		}
		GC.KeepAlive(this);
	}

	internal bool WorkerAddPenContext(PenContext penContext)
	{
		if (__disposed)
		{
			throw new ObjectDisposedException(null, SR.Penservice_Disposed);
		}
		WorkerOperationAddContext workerOperationAddContext = new WorkerOperationAddContext(penContext, this);
		lock (_workerOperationLock)
		{
			_workerOperation.Add(workerOperationAddContext);
		}
		UnsafeNativeMethods.RaiseResetEvent(_pimcResetHandle.Value);
		workerOperationAddContext.DoneEvent.WaitOne();
		workerOperationAddContext.DoneEvent.Close();
		return workerOperationAddContext.Result;
	}

	internal bool WorkerRemovePenContext(PenContext penContext)
	{
		if (__disposed)
		{
			return true;
		}
		WorkerOperationRemoveContext workerOperationRemoveContext = new WorkerOperationRemoveContext(penContext, this);
		lock (_workerOperationLock)
		{
			_workerOperation.Add(workerOperationRemoveContext);
		}
		UnsafeNativeMethods.RaiseResetEvent(_pimcResetHandle.Value);
		workerOperationRemoveContext.DoneEvent.WaitOne();
		workerOperationRemoveContext.DoneEvent.Close();
		return workerOperationRemoveContext.Result;
	}

	internal TabletDeviceInfo[] WorkerGetTabletsInfo()
	{
		WorkerOperationGetTabletsInfo workerOperationGetTabletsInfo = new WorkerOperationGetTabletsInfo();
		lock (_workerOperationLock)
		{
			_workerOperation.Add(workerOperationGetTabletsInfo);
		}
		UnsafeNativeMethods.RaiseResetEvent(_pimcResetHandle.Value);
		workerOperationGetTabletsInfo.DoneEvent.WaitOne();
		workerOperationGetTabletsInfo.DoneEvent.Close();
		return workerOperationGetTabletsInfo.TabletDevicesInfo;
	}

	internal PenContextInfo WorkerCreateContext(nint hwnd, IPimcTablet3 pimcTablet)
	{
		WorkerOperationCreateContext workerOperationCreateContext = new WorkerOperationCreateContext(hwnd, pimcTablet);
		lock (_workerOperationLock)
		{
			_workerOperation.Add(workerOperationCreateContext);
		}
		UnsafeNativeMethods.RaiseResetEvent(_pimcResetHandle.Value);
		workerOperationCreateContext.DoneEvent.WaitOne();
		workerOperationCreateContext.DoneEvent.Close();
		return workerOperationCreateContext.Result;
	}

	internal bool WorkerAcquireTabletLocks(IPimcTablet3 tablet, uint wispTabletKey)
	{
		WorkerOperationAcquireTabletLocks workerOperationAcquireTabletLocks = new WorkerOperationAcquireTabletLocks(tablet, wispTabletKey);
		lock (_workerOperationLock)
		{
			_workerOperation.Add(workerOperationAcquireTabletLocks);
		}
		UnsafeNativeMethods.RaiseResetEvent(_pimcResetHandle.Value);
		workerOperationAcquireTabletLocks.DoneEvent.WaitOne();
		workerOperationAcquireTabletLocks.DoneEvent.Close();
		return workerOperationAcquireTabletLocks.Result;
	}

	internal bool WorkerReleaseTabletLocks(IPimcTablet3 tablet, uint wispTabletKey)
	{
		WorkerOperationReleaseTabletLocks workerOperationReleaseTabletLocks = new WorkerOperationReleaseTabletLocks(tablet, wispTabletKey);
		lock (_workerOperationLock)
		{
			_workerOperation.Add(workerOperationReleaseTabletLocks);
		}
		UnsafeNativeMethods.RaiseResetEvent(_pimcResetHandle.Value);
		workerOperationReleaseTabletLocks.DoneEvent.WaitOne();
		workerOperationReleaseTabletLocks.DoneEvent.Close();
		return workerOperationReleaseTabletLocks.Result;
	}

	internal StylusDeviceInfo[] WorkerRefreshCursorInfo(IPimcTablet3 pimcTablet)
	{
		WorkerOperationRefreshCursorInfo workerOperationRefreshCursorInfo = new WorkerOperationRefreshCursorInfo(pimcTablet);
		lock (_workerOperationLock)
		{
			_workerOperation.Add(workerOperationRefreshCursorInfo);
		}
		UnsafeNativeMethods.RaiseResetEvent(_pimcResetHandle.Value);
		workerOperationRefreshCursorInfo.DoneEvent.WaitOne();
		workerOperationRefreshCursorInfo.DoneEvent.Close();
		return workerOperationRefreshCursorInfo.StylusDevicesInfo;
	}

	internal TabletDeviceInfo WorkerGetTabletInfo(uint index)
	{
		WorkerOperationGetTabletInfo workerOperationGetTabletInfo = new WorkerOperationGetTabletInfo(index);
		lock (_workerOperationLock)
		{
			_workerOperation.Add(workerOperationGetTabletInfo);
		}
		UnsafeNativeMethods.RaiseResetEvent(_pimcResetHandle.Value);
		workerOperationGetTabletInfo.DoneEvent.WaitOne();
		workerOperationGetTabletInfo.DoneEvent.Close();
		return workerOperationGetTabletInfo.TabletDeviceInfo;
	}

	internal TabletDeviceSizeInfo WorkerGetUpdatedSizes(IPimcTablet3 pimcTablet)
	{
		WorkerOperationWorkerGetUpdatedSizes workerOperationWorkerGetUpdatedSizes = new WorkerOperationWorkerGetUpdatedSizes(pimcTablet);
		lock (_workerOperationLock)
		{
			_workerOperation.Add(workerOperationWorkerGetUpdatedSizes);
		}
		UnsafeNativeMethods.RaiseResetEvent(_pimcResetHandle.Value);
		workerOperationWorkerGetUpdatedSizes.DoneEvent.WaitOne();
		workerOperationWorkerGetUpdatedSizes.DoneEvent.Close();
		return workerOperationWorkerGetUpdatedSizes.TabletDeviceSizeInfo;
	}

	private void FlushCache(bool goingOutOfRange)
	{
		if (_cachedMoveData != null)
		{
			if (!goingOutOfRange || _cachedMovePenContext.IsInRange(_cachedMoveStylusPointerId))
			{
				_cachedMovePenContext.FirePenInRange(_cachedMoveStylusPointerId, _cachedMoveData, _cachedMoveStartTimestamp);
				_cachedMovePenContext.FirePackets(_cachedMoveStylusPointerId, _cachedMoveData, _cachedMoveStartTimestamp);
			}
			_cachedMoveData = null;
			_cachedMovePenContext = null;
			_cachedMoveStylusPointerId = 0;
		}
	}

	private bool DoCacheEvent(int evt, PenContext penContext, int stylusPointerId, int[] data, int timestamp)
	{
		if (evt == 711)
		{
			if (_cachedMoveData == null)
			{
				_cachedMovePenContext = penContext;
				_cachedMoveStylusPointerId = stylusPointerId;
				_cachedMoveStartTimestamp = timestamp;
				_cachedMoveData = data;
				return true;
			}
			if (_cachedMovePenContext == penContext && stylusPointerId == _cachedMoveStylusPointerId)
			{
				int num = timestamp - _cachedMoveStartTimestamp;
				if (timestamp < _cachedMoveStartTimestamp)
				{
					num = int.MaxValue - _cachedMoveStartTimestamp + timestamp;
				}
				if (8 > num)
				{
					int[] cachedMoveData = _cachedMoveData;
					_cachedMoveData = new int[cachedMoveData.Length + data.Length];
					cachedMoveData.CopyTo(_cachedMoveData, 0);
					data.CopyTo(_cachedMoveData, cachedMoveData.Length);
					return true;
				}
			}
		}
		return false;
	}

	internal void FireEvent(PenContext penContext, int evt, int stylusPointerId, int cPackets, int cbPacket, nint pPackets)
	{
		if (__disposed)
		{
			return;
		}
		if (cbPacket % 4 != 0)
		{
			throw new InvalidOperationException(SR.PenService_InvalidPacketData);
		}
		int num = cPackets * (cbPacket / 4);
		int[] array = null;
		if (0 < num)
		{
			array = new int[num];
			Marshal.Copy(pPackets, array, 0, num);
			penContext.CheckForRectMappingChanged(array, cPackets);
		}
		else
		{
			array = null;
		}
		int tickCount = Environment.TickCount;
		if (!DoCacheEvent(evt, penContext, stylusPointerId, array, tickCount))
		{
			FlushCache(goingOutOfRange: false);
			switch (evt)
			{
			case 709:
				penContext.FirePenInRange(stylusPointerId, array, tickCount);
				penContext.FirePenDown(stylusPointerId, array, tickCount);
				break;
			case 710:
				penContext.FirePenInRange(stylusPointerId, array, tickCount);
				penContext.FirePenUp(stylusPointerId, array, tickCount);
				break;
			case 711:
				penContext.FirePenInRange(stylusPointerId, array, tickCount);
				penContext.FirePackets(stylusPointerId, array, tickCount);
				break;
			case 707:
				penContext.FirePenInRange(stylusPointerId, null, tickCount);
				break;
			case 708:
				penContext.FirePenOutOfRange(stylusPointerId, tickCount);
				break;
			case 714:
				penContext.FireSystemGesture(stylusPointerId, tickCount);
				break;
			case 712:
			case 713:
				break;
			}
		}
	}

	private static TabletDeviceInfo GetTabletInfoHelper(IPimcTablet3 pimcTablet)
	{
		TabletDeviceInfo tabletDeviceInfo = new TabletDeviceInfo();
		tabletDeviceInfo.PimcTablet = new SecurityCriticalDataClass<IPimcTablet3>(pimcTablet);
		pimcTablet.GetKey(out tabletDeviceInfo.Id);
		pimcTablet.GetName(out tabletDeviceInfo.Name);
		pimcTablet.GetPlugAndPlayId(out tabletDeviceInfo.PlugAndPlayId);
		pimcTablet.GetTabletAndDisplaySize(out var tabletWidth, out var tabletHeight, out var displayWidth, out var displayHeight);
		tabletDeviceInfo.SizeInfo = new TabletDeviceSizeInfo(new Size(tabletWidth, tabletHeight), new Size(displayWidth, displayHeight));
		pimcTablet.GetHardwareCaps(out var caps);
		tabletDeviceInfo.HardwareCapabilities = (TabletHardwareCapabilities)caps;
		pimcTablet.GetDeviceType(out var devType);
		tabletDeviceInfo.DeviceType = (TabletDeviceType)(devType - 1);
		InitializeSupportedStylusPointProperties(pimcTablet, tabletDeviceInfo);
		tabletDeviceInfo.StylusDevicesInfo = GetStylusDevicesInfo(pimcTablet);
		tabletDeviceInfo.WispTabletKey = UnsafeNativeMethods.QueryWispTabletKey(pimcTablet);
		UnsafeNativeMethods.SetWispManagerKey(pimcTablet);
		UnsafeNativeMethods.LockWispManager();
		return tabletDeviceInfo;
	}

	private static void InitializeSupportedStylusPointProperties(IPimcTablet3 pimcTablet, TabletDeviceInfo tabletInfo)
	{
		int num = -1;
		pimcTablet.GetPacketDescriptionInfo(out var cProps, out var cButtons);
		List<StylusPointProperty> list = new List<StylusPointProperty>(cProps + cButtons + 3);
		for (int i = 0; i < cProps; i++)
		{
			pimcTablet.GetPacketPropertyInfo(i, out var guid, out var _, out var _, out var _, out var _);
			if (num == -1 && guid == StylusPointPropertyIds.NormalPressure)
			{
				num = i;
			}
			StylusPointProperty item = new StylusPointProperty(guid, isButton: false);
			list.Add(item);
		}
		for (int j = 0; j < cButtons; j++)
		{
			pimcTablet.GetPacketButtonInfo(j, out var guid2);
			StylusPointProperty item2 = new StylusPointProperty(guid2, isButton: true);
			list.Add(item2);
		}
		if (num == -1)
		{
			list.Insert(2, StylusPointProperties.NormalPressure);
		}
		else
		{
			tabletInfo.HardwareCapabilities |= TabletHardwareCapabilities.SupportsPressure;
		}
		tabletInfo.StylusPointProperties = new ReadOnlyCollection<StylusPointProperty>(list);
		tabletInfo.PressureIndex = num;
	}

	private static StylusDeviceInfo[] GetStylusDevicesInfo(IPimcTablet3 pimcTablet)
	{
		pimcTablet.GetCursorCount(out var cCursors);
		StylusDeviceInfo[] array = new StylusDeviceInfo[cCursors];
		for (int i = 0; i < cCursors; i++)
		{
			pimcTablet.GetCursorInfo(i, out var sName, out var id, out var fInverted);
			pimcTablet.GetCursorButtonCount(i, out var cButtons);
			StylusButton[] array2 = new StylusButton[cButtons];
			for (int j = 0; j < cButtons; j++)
			{
				pimcTablet.GetCursorButtonInfo(i, j, out var sName2, out var guid);
				array2[j] = new StylusButton(sName2, guid);
			}
			StylusButtonCollection buttonCollection = new StylusButtonCollection(array2);
			array[i].CursorName = sName;
			array[i].CursorId = id;
			array[i].CursorInverted = fInverted;
			array[i].ButtonCollection = buttonCollection;
		}
		return array;
	}

	internal bool AddPenContext(PenContext penContext)
	{
		List<PenContext> list = new List<PenContext>();
		bool result = false;
		for (int i = 0; i < _penContexts.Length; i++)
		{
			if (_penContexts[i].IsAlive && _penContexts[i].Target is PenContext item)
			{
				list.Add(item);
			}
		}
		if (list.Count < 31)
		{
			list.Add(penContext);
			UnsafeNativeMethods.CheckedLockWispObjectFromGit(penContext.WispContextKey);
			_pimcContexts = new IPimcContext3[list.Count];
			_penContexts = new WeakReference[list.Count];
			_handles = new nint[list.Count];
			_wispContextKeys = new uint[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				PenContext penContext2 = list[i];
				_handles[i] = penContext2.CommHandle;
				_pimcContexts[i] = penContext2._pimcContext.Value;
				_penContexts[i] = new WeakReference(penContext2);
				_wispContextKeys[i] = penContext2.WispContextKey;
				penContext2 = null;
			}
			result = true;
		}
		list.Clear();
		list = null;
		return result;
	}

	internal bool RemovePenContext(PenContext penContext)
	{
		List<PenContext> list = new List<PenContext>();
		bool flag = false;
		for (int i = 0; i < _penContexts.Length; i++)
		{
			if (_penContexts[i].IsAlive && _penContexts[i].Target is PenContext penContext2 && (penContext2 != penContext || penContext2.IsInRange(0)))
			{
				list.Add(penContext2);
			}
		}
		flag = !list.Contains(penContext);
		_pimcContexts = new IPimcContext3[list.Count];
		_penContexts = new WeakReference[list.Count];
		_handles = new nint[list.Count];
		_wispContextKeys = new uint[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			PenContext penContext3 = list[i];
			_handles[i] = penContext3.CommHandle;
			_pimcContexts[i] = penContext3._pimcContext.Value;
			_penContexts[i] = new WeakReference(penContext3);
			_wispContextKeys[i] = penContext3.WispContextKey;
			penContext3 = null;
		}
		list.Clear();
		list = null;
		if (flag)
		{
			UnsafeNativeMethods.CheckedUnlockWispObjectFromGit(penContext.WispContextKey);
			penContext._pimcContext.Value.ShutdownComm();
			Marshal.ReleaseComObject(penContext._pimcContext.Value);
		}
		return flag;
	}

	private static bool IsKnownException(Exception e)
	{
		if (!(e is COMException) && !(e is ArgumentException) && !(e is UnauthorizedAccessException))
		{
			return e is InvalidCastException;
		}
		return true;
	}

	internal void ThreadProc()
	{
		Thread.CurrentThread.Name = "Stylus Input";
		try
		{
			while (!__disposed)
			{
				UnsafeNativeMethods.EnsurePenImcClassesActivated();
				WorkerOperation[] array = null;
				lock (_workerOperationLock)
				{
					if (_workerOperation.Count > 0)
					{
						array = _workerOperation.ToArray();
						_workerOperation.Clear();
					}
				}
				if (array != null)
				{
					for (int i = 0; i < array.Length; i++)
					{
						array[i].DoWork();
					}
					array = null;
				}
				while (true)
				{
					int evt;
					int stylusPointerId;
					int cPackets;
					int cbPacket;
					nint pPackets;
					int iHandle;
					if (_handles.Length == 1)
					{
						if (!UnsafeNativeMethods.GetPenEvent(_handles[0], _pimcResetHandle.Value, out evt, out stylusPointerId, out cPackets, out cbPacket, out pPackets))
						{
							break;
						}
						iHandle = 0;
					}
					else if (!UnsafeNativeMethods.GetPenEventMultiple(_handles.Length, _handles, _pimcResetHandle.Value, out iHandle, out evt, out stylusPointerId, out cPackets, out cbPacket, out pPackets))
					{
						break;
					}
					if (evt != 1)
					{
						if (_penContexts[iHandle].Target is PenContext penContext)
						{
							FireEvent(penContext, evt, stylusPointerId, cPackets, cbPacket, pPackets);
							PenContext penContext2 = null;
						}
						continue;
					}
					FlushCache(goingOutOfRange: true);
					for (int j = 0; j < _penContexts.Length; j++)
					{
						if (_penContexts[j].Target is PenContext penContext3)
						{
							penContext3.FirePenOutOfRange(0, Environment.TickCount);
							PenContext penContext4 = null;
						}
					}
				}
			}
		}
		finally
		{
			__disposed = true;
			UnsafeNativeMethods.DestroyResetEvent(_pimcResetHandle.Value);
			UnsafeNativeMethods.UnlockWispManager();
			UnsafeNativeMethods.ReleaseManagerExternalLock();
			for (int k = 0; k < _pimcContexts.Length; k++)
			{
				UnsafeNativeMethods.CheckedUnlockWispObjectFromGit(_wispContextKeys[k]);
				_pimcContexts[k].ShutdownComm();
			}
			UnsafeNativeMethods.DeactivatePenImcClasses();
			GC.KeepAlive(this);
		}
	}
}
