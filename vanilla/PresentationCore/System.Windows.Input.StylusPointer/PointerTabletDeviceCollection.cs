using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input.Tracing;
using MS.Win32;
using MS.Win32.Pointer;

namespace System.Windows.Input.StylusPointer;

internal class PointerTabletDeviceCollection : TabletDeviceCollection
{
	private Dictionary<nint, PointerTabletDevice> _tabletDeviceMap = new Dictionary<nint, PointerTabletDevice>();

	internal bool IsValid { get; private set; }

	internal PointerTabletDevice GetByDeviceId(nint deviceId)
	{
		PointerTabletDevice value = null;
		_tabletDeviceMap.TryGetValue(deviceId, out value);
		return value;
	}

	internal PointerStylusDevice GetStylusDeviceByCursorId(uint cursorId)
	{
		PointerStylusDevice result = null;
		using (Dictionary<nint, PointerTabletDevice>.ValueCollection.Enumerator enumerator = _tabletDeviceMap.Values.GetEnumerator())
		{
			while (enumerator.MoveNext() && (result = enumerator.Current.GetStylusByCursorId(cursorId)) == null)
			{
			}
		}
		return result;
	}

	internal void Refresh()
	{
		try
		{
			Dictionary<nint, PointerTabletDevice> tabletDeviceMap = _tabletDeviceMap;
			_tabletDeviceMap = new Dictionary<nint, PointerTabletDevice>();
			base.TabletDevices.Clear();
			uint deviceCount = 0u;
			IsValid = UnsafeNativeMethods.GetPointerDevices(ref deviceCount, null);
			if (!IsValid)
			{
				return;
			}
			UnsafeNativeMethods.POINTER_DEVICE_INFO[] array = new UnsafeNativeMethods.POINTER_DEVICE_INFO[deviceCount];
			IsValid = UnsafeNativeMethods.GetPointerDevices(ref deviceCount, array);
			if (IsValid)
			{
				UnsafeNativeMethods.POINTER_DEVICE_INFO[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					UnsafeNativeMethods.POINTER_DEVICE_INFO deviceInfo = array2[i];
					PointerTabletDeviceInfo pointerTabletDeviceInfo = new PointerTabletDeviceInfo(MS.Win32.NativeMethods.IntPtrToInt32(deviceInfo.device), deviceInfo);
					if (pointerTabletDeviceInfo.TryInitialize())
					{
						PointerTabletDevice pointerTabletDevice = new PointerTabletDevice(pointerTabletDeviceInfo);
						if (!tabletDeviceMap.Remove(pointerTabletDevice.Device))
						{
							StylusTraceLogger.LogDeviceConnect(new StylusTraceLogger.StylusDeviceInfo(pointerTabletDevice.Id, pointerTabletDevice.Name, pointerTabletDevice.ProductId, pointerTabletDevice.TabletHardwareCapabilities, pointerTabletDevice.TabletSize, pointerTabletDevice.ScreenSize, pointerTabletDevice.Type, pointerTabletDevice.StylusDevices.Count));
						}
						_tabletDeviceMap[pointerTabletDevice.Device] = pointerTabletDevice;
						base.TabletDevices.Add(pointerTabletDevice.TabletDevice);
					}
				}
			}
			foreach (PointerTabletDevice value in tabletDeviceMap.Values)
			{
				StylusTraceLogger.LogDeviceDisconnect(value.Id);
			}
		}
		catch (Win32Exception)
		{
			IsValid = false;
		}
	}
}
