using System.Collections.Generic;
using MS.Win32;
using MS.Win32.Pointer;

namespace System.Windows.Input.StylusPointer;

internal class PointerTabletDevice : TabletDeviceBase
{
	private PointerTabletDeviceInfo _deviceInfo;

	private StylusDeviceCollection _stylusDevices;

	private Dictionary<uint, PointerStylusDevice> _stylusDeviceMap = new Dictionary<uint, PointerStylusDevice>();

	internal PointerTabletDeviceInfo DeviceInfo => _deviceInfo;

	internal nint Device => _deviceInfo.Device;

	internal int DoubleTapDelta
	{
		get
		{
			if (_tabletInfo.DeviceType != TabletDeviceType.Touch)
			{
				return StylusLogic.CurrentStylusLogic.StylusDoubleTapDelta;
			}
			return StylusLogic.CurrentStylusLogic.TouchDoubleTapDelta;
		}
	}

	internal int DoubleTapDeltaTime
	{
		get
		{
			if (_tabletInfo.DeviceType != TabletDeviceType.Touch)
			{
				return StylusLogic.CurrentStylusLogic.StylusDoubleTapDeltaTime;
			}
			return StylusLogic.CurrentStylusLogic.TouchDoubleTapDeltaTime;
		}
	}

	internal override Size DoubleTapSize => _doubleTapSize;

	internal override StylusDeviceCollection StylusDevices => _stylusDevices;

	internal override IInputElement Target => Stylus.CurrentStylusDevice?.Target;

	internal override PresentationSource ActiveSource => Stylus.CurrentStylusDevice?.ActiveSource;

	internal PointerTabletDevice(PointerTabletDeviceInfo deviceInfo)
		: base(deviceInfo)
	{
		_deviceInfo = deviceInfo;
		_tabletInfo = deviceInfo;
		UpdateSizeDeltas();
		BuildStylusDevices();
	}

	private void BuildStylusDevices()
	{
		uint cursorCount = 0u;
		List<PointerStylusDevice> list = new List<PointerStylusDevice>();
		if (UnsafeNativeMethods.GetPointerDeviceCursors(_deviceInfo.Device, ref cursorCount, null))
		{
			UnsafeNativeMethods.POINTER_DEVICE_CURSOR_INFO[] array = new UnsafeNativeMethods.POINTER_DEVICE_CURSOR_INFO[cursorCount];
			if (UnsafeNativeMethods.GetPointerDeviceCursors(_deviceInfo.Device, ref cursorCount, array))
			{
				UnsafeNativeMethods.POINTER_DEVICE_CURSOR_INFO[] array2 = array;
				foreach (UnsafeNativeMethods.POINTER_DEVICE_CURSOR_INFO cursorInfo in array2)
				{
					PointerStylusDevice pointerStylusDevice = new PointerStylusDevice(this, cursorInfo);
					_stylusDeviceMap.Add(pointerStylusDevice.CursorId, pointerStylusDevice);
					list.Add(pointerStylusDevice);
				}
			}
		}
		_stylusDevices = new StylusDeviceCollection(list.ToArray());
	}

	internal void UpdateSizeDeltas()
	{
		Size doubleTapSize = new Size(Math.Max(1, SafeSystemMetrics.DoubleClickDeltaX / 2), Math.Max(1, SafeSystemMetrics.DoubleClickDeltaY / 2));
		StylusPointPropertyInfo propertyInfo = base.StylusPointDescription.GetPropertyInfo(StylusPointProperties.X);
		StylusPointPropertyInfo propertyInfo2 = base.StylusPointDescription.GetPropertyInfo(StylusPointProperties.Y);
		uint propertyValue = TabletDeviceBase.GetPropertyValue(propertyInfo);
		uint propertyValue2 = TabletDeviceBase.GetPropertyValue(propertyInfo2);
		if (propertyValue != 0 && propertyValue2 != 0)
		{
			_doubleTapSize = new Size((int)Math.Round(base.ScreenSize.Width * (double)DoubleTapDelta / (double)propertyValue), (int)Math.Round(base.ScreenSize.Height * (double)DoubleTapDelta / (double)propertyValue2));
			_doubleTapSize.Width = Math.Max(doubleTapSize.Width, _doubleTapSize.Width);
			_doubleTapSize.Height = Math.Max(doubleTapSize.Height, _doubleTapSize.Height);
		}
		else
		{
			_doubleTapSize = doubleTapSize;
		}
		_forceUpdateSizeDeltas = false;
	}

	internal PointerStylusDevice GetStylusByCursorId(uint cursorId)
	{
		PointerStylusDevice value = null;
		_stylusDeviceMap.TryGetValue(cursorId, out value);
		return value;
	}
}
