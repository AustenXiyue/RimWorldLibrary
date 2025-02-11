using System.Collections.Generic;
using MS.Internal;
using MS.Win32.Penimc;

namespace System.Windows.Input;

internal sealed class PenContext
{
	internal SecurityCriticalDataClass<IPimcContext3> _pimcContext;

	private SecurityCriticalData<nint> _hwnd;

	private SecurityCriticalData<nint> _commHandle;

	private PenContexts _contexts;

	private PenThread _penThreadPenContext;

	private int _id;

	private int _tabletDeviceId;

	private StylusPointPropertyInfo _infoX;

	private StylusPointPropertyInfo _infoY;

	private bool _supportInRange;

	private List<int> _stylusDevicesInRange;

	private bool _isIntegrated;

	private StylusPointDescription _stylusPointDescription;

	private int _statusPropertyIndex = -1;

	private int _lastInRangeTime;

	private int _queuedInRangeCount;

	internal PenContexts Contexts => _contexts;

	internal nint CommHandle => _commHandle.Value;

	internal int Id => _id;

	internal int TabletDeviceId => _tabletDeviceId;

	internal StylusPointDescription StylusPointDescription
	{
		get
		{
			if (_stylusPointDescription == null)
			{
				InitStylusPointDescription();
			}
			return _stylusPointDescription;
		}
	}

	internal bool SupportInRange => _supportInRange;

	internal bool UpdateScreenMeasurementsPending { get; set; }

	internal int LastInRangeTime => _lastInRangeTime;

	internal int QueuedInRangeCount => _queuedInRangeCount;

	internal uint WispContextKey { get; private set; }

	internal PenContext(IPimcContext3 pimcContext, nint hwnd, PenContexts contexts, bool supportInRange, bool isIntegrated, int id, nint commHandle, int tabletDeviceId, uint wispContextKey)
	{
		_contexts = contexts;
		_pimcContext = new SecurityCriticalDataClass<IPimcContext3>(pimcContext);
		_id = id;
		_tabletDeviceId = tabletDeviceId;
		_commHandle = new SecurityCriticalData<nint>(commHandle);
		_hwnd = new SecurityCriticalData<nint>(hwnd);
		_supportInRange = supportInRange;
		_isIntegrated = isIntegrated;
		WispContextKey = wispContextKey;
		UpdateScreenMeasurementsPending = false;
	}

	~PenContext()
	{
		TryRemove(shutdownWorkerThread: false);
		_pimcContext = null;
		_contexts = null;
		GC.KeepAlive(this);
	}

	private void InitStylusPointDescription()
	{
		int num = -1;
		_pimcContext.Value.GetPacketDescriptionInfo(out var cProps, out var cButtons);
		List<StylusPointPropertyInfo> list = new List<StylusPointPropertyInfo>(cProps + cButtons + 3);
		for (int i = 0; i < cProps; i++)
		{
			_pimcContext.Value.GetPacketPropertyInfo(i, out var guid, out var iMin, out var iMax, out var iUnits, out var flResolution);
			if (num == -1 && guid == StylusPointPropertyIds.NormalPressure)
			{
				num = i;
			}
			if (_statusPropertyIndex == -1 && guid == StylusPointPropertyIds.PacketStatus)
			{
				_statusPropertyIndex = i;
			}
			StylusPointPropertyInfo item = new StylusPointPropertyInfo(new StylusPointProperty(guid, isButton: false), iMin, iMax, (StylusPointPropertyUnit)iUnits, flResolution);
			list.Add(item);
		}
		if (list != null)
		{
			for (int j = 0; j < cButtons; j++)
			{
				_pimcContext.Value.GetPacketButtonInfo(j, out var guid2);
				StylusPointPropertyInfo item2 = new StylusPointPropertyInfo(new StylusPointProperty(guid2, isButton: true));
				list.Add(item2);
			}
			if (num == -1)
			{
				list.Insert(2, StylusPointPropertyInfoDefaults.NormalPressure);
			}
			_infoX = list[0];
			_infoY = list[1];
			_stylusPointDescription = new StylusPointDescription(list, num);
		}
	}

	internal void Enable()
	{
		if (_pimcContext != null && _pimcContext.Value != null)
		{
			_penThreadPenContext = PenThreadPool.GetPenThreadForPenContext(this);
		}
	}

	internal void Disable(bool shutdownWorkerThread)
	{
		if (TryRemove(shutdownWorkerThread))
		{
			GC.SuppressFinalize(this);
		}
	}

	private bool TryRemove(bool shutdownWorkerThread)
	{
		if (_penThreadPenContext != null && _penThreadPenContext.RemovePenContext(this))
		{
			if (shutdownWorkerThread)
			{
				_penThreadPenContext?.Dispose();
			}
			_penThreadPenContext = null;
			return true;
		}
		return false;
	}

	internal bool IsInRange(int stylusPointerId)
	{
		if (stylusPointerId == 0)
		{
			if (_stylusDevicesInRange != null)
			{
				return _stylusDevicesInRange.Count > 0;
			}
			return false;
		}
		if (_stylusDevicesInRange != null)
		{
			return _stylusDevicesInRange.Contains(stylusPointerId);
		}
		return false;
	}

	internal void FirePenDown(int stylusPointerId, int[] data, int timestamp)
	{
		timestamp = EnsureTimestampUnique(timestamp);
		_lastInRangeTime = timestamp;
		if (_stylusPointDescription == null)
		{
			InitStylusPointDescription();
		}
		_contexts.OnPenDown(this, _tabletDeviceId, stylusPointerId, data, timestamp);
	}

	internal void FirePenUp(int stylusPointerId, int[] data, int timestamp)
	{
		timestamp = EnsureTimestampUnique(timestamp);
		_lastInRangeTime = timestamp;
		if (_stylusPointDescription == null)
		{
			InitStylusPointDescription();
		}
		_contexts.OnPenUp(this, _tabletDeviceId, stylusPointerId, data, timestamp);
	}

	internal void FirePackets(int stylusPointerId, int[] data, int timestamp)
	{
		timestamp = EnsureTimestampUnique(timestamp);
		_lastInRangeTime = timestamp;
		if (_stylusPointDescription == null)
		{
			InitStylusPointDescription();
		}
		bool flag = false;
		if (_statusPropertyIndex != -1)
		{
			flag = (data[_statusPropertyIndex] & 1) != 0;
		}
		if (flag)
		{
			_contexts.OnPackets(this, _tabletDeviceId, stylusPointerId, data, timestamp);
		}
		else
		{
			_contexts.OnInAirPackets(this, _tabletDeviceId, stylusPointerId, data, timestamp);
		}
	}

	internal void FirePenInRange(int stylusPointerId, int[] data, int timestamp)
	{
		if (_stylusPointDescription == null)
		{
			InitStylusPointDescription();
		}
		if (data == null)
		{
			_lastInRangeTime = timestamp;
			_queuedInRangeCount++;
			_contexts.OnPenInRange(this, _tabletDeviceId, stylusPointerId, data, timestamp);
		}
		else if (!IsInRange(stylusPointerId))
		{
			_lastInRangeTime = timestamp;
			if (_stylusDevicesInRange == null)
			{
				_stylusDevicesInRange = new List<int>();
			}
			_stylusDevicesInRange.Add(stylusPointerId);
			_contexts.OnPenInRange(this, _tabletDeviceId, stylusPointerId, data, timestamp);
		}
	}

	internal void FirePenOutOfRange(int stylusPointerId, int timestamp)
	{
		if (stylusPointerId != 0)
		{
			if (IsInRange(stylusPointerId))
			{
				timestamp = EnsureTimestampUnique(timestamp);
				_lastInRangeTime = timestamp;
				if (_stylusPointDescription == null)
				{
					InitStylusPointDescription();
				}
				_stylusDevicesInRange.Remove(stylusPointerId);
				_contexts.OnPenOutOfRange(this, _tabletDeviceId, stylusPointerId, timestamp);
				if (_stylusDevicesInRange.Count == 0)
				{
					_stylusDevicesInRange = null;
				}
			}
		}
		else if (_stylusDevicesInRange != null)
		{
			timestamp = EnsureTimestampUnique(timestamp);
			_lastInRangeTime = timestamp;
			if (_stylusPointDescription == null)
			{
				InitStylusPointDescription();
			}
			for (int i = 0; i < _stylusDevicesInRange.Count; i++)
			{
				_contexts.OnPenOutOfRange(this, _tabletDeviceId, _stylusDevicesInRange[i], timestamp);
			}
			_stylusDevicesInRange = null;
		}
	}

	internal void FireSystemGesture(int stylusPointerId, int timestamp)
	{
		timestamp = EnsureTimestampUnique(timestamp);
		_lastInRangeTime = timestamp;
		if (_stylusPointDescription == null)
		{
			InitStylusPointDescription();
		}
		UnsafeNativeMethods.GetLastSystemEventData(_commHandle.Value, out var evt, out var _, out var _, out var x, out var y, out var _, out var buttonState);
		_contexts.OnSystemEvent(this, _tabletDeviceId, stylusPointerId, timestamp, (SystemGesture)evt, x, y, buttonState);
	}

	internal void CheckForRectMappingChanged(int[] data, int numPackets)
	{
		if (UpdateScreenMeasurementsPending)
		{
			return;
		}
		if (_stylusPointDescription == null)
		{
			InitStylusPointDescription();
		}
		if (_statusPropertyIndex == -1)
		{
			return;
		}
		int num = data.Length / numPackets;
		for (int i = 0; i < numPackets; i++)
		{
			if ((data[i * num + _statusPropertyIndex] & 0x10) != 0)
			{
				UpdateScreenMeasurementsPending = true;
				break;
			}
		}
	}

	private int EnsureTimestampUnique(int timestamp)
	{
		if (_lastInRangeTime - timestamp >= 0)
		{
			timestamp = _lastInRangeTime + 1;
		}
		return timestamp;
	}

	internal void DecrementQueuedInRangeCount()
	{
		_queuedInRangeCount--;
	}
}
