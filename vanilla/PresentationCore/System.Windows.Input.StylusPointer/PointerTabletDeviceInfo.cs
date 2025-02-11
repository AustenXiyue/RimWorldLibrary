using System.Collections.Generic;
using System.Collections.ObjectModel;
using MS.Win32.Pointer;

namespace System.Windows.Input.StylusPointer;

internal class PointerTabletDeviceInfo : TabletDeviceInfo
{
	private UnsafeNativeMethods.POINTER_DEVICE_INFO _deviceInfo;

	internal UnsafeNativeMethods.POINTER_DEVICE_PROPERTY[] SupportedPointerProperties { get; private set; }

	internal int SupportedButtonPropertyIndex { get; private set; }

	internal StylusButtonCollection StylusButtons { get; private set; }

	internal nint Device => _deviceInfo.device;

	internal bool UsingFakePressure { get; private set; }

	internal UnsafeNativeMethods.RECT DeviceRect { get; private set; }

	internal UnsafeNativeMethods.RECT DisplayRect { get; private set; }

	internal PointerTabletDeviceInfo(int id, UnsafeNativeMethods.POINTER_DEVICE_INFO deviceInfo)
	{
		_deviceInfo = deviceInfo;
		Id = id;
		Name = _deviceInfo.productString;
		PlugAndPlayId = _deviceInfo.productString;
	}

	internal bool TryInitialize()
	{
		bool flag = true;
		InitializeDeviceType();
		flag = TryInitializeSupportedStylusPointProperties();
		if (flag)
		{
			flag = TryInitializeDeviceRects();
		}
		return flag;
	}

	private void InitializeDeviceType()
	{
		switch (_deviceInfo.pointerDeviceType)
		{
		case UnsafeNativeMethods.POINTER_DEVICE_TYPE.POINTER_DEVICE_TYPE_EXTERNAL_PEN:
			DeviceType = TabletDeviceType.Stylus;
			break;
		case UnsafeNativeMethods.POINTER_DEVICE_TYPE.POINTER_DEVICE_TYPE_INTEGRATED_PEN:
			DeviceType = TabletDeviceType.Stylus;
			HardwareCapabilities |= TabletHardwareCapabilities.Integrated;
			break;
		case UnsafeNativeMethods.POINTER_DEVICE_TYPE.POINTER_DEVICE_TYPE_TOUCH:
			DeviceType = TabletDeviceType.Touch;
			HardwareCapabilities |= TabletHardwareCapabilities.Integrated;
			break;
		case UnsafeNativeMethods.POINTER_DEVICE_TYPE.POINTER_DEVICE_TYPE_TOUCH_PAD:
			DeviceType = TabletDeviceType.Touch;
			break;
		}
		HardwareCapabilities |= TabletHardwareCapabilities.HardProximity;
	}

	private bool TryInitializeSupportedStylusPointProperties()
	{
		bool flag = false;
		uint propertyCount = 0u;
		PressureIndex = -1;
		UsingFakePressure = true;
		flag = UnsafeNativeMethods.GetPointerDeviceProperties(Device, ref propertyCount, null);
		if (flag)
		{
			SupportedPointerProperties = new UnsafeNativeMethods.POINTER_DEVICE_PROPERTY[propertyCount];
			flag = UnsafeNativeMethods.GetPointerDeviceProperties(Device, ref propertyCount, SupportedPointerProperties);
			if (flag)
			{
				List<StylusPointProperty> list = new List<StylusPointProperty>
				{
					StylusPointPropertyInfoDefaults.X,
					StylusPointPropertyInfoDefaults.Y,
					StylusPointPropertyInfoDefaults.NormalPressure
				};
				List<StylusPointProperty> list2 = new List<StylusPointProperty>();
				List<UnsafeNativeMethods.POINTER_DEVICE_PROPERTY> list3 = new List<UnsafeNativeMethods.POINTER_DEVICE_PROPERTY>
				{
					default(UnsafeNativeMethods.POINTER_DEVICE_PROPERTY),
					default(UnsafeNativeMethods.POINTER_DEVICE_PROPERTY)
				};
				List<UnsafeNativeMethods.POINTER_DEVICE_PROPERTY> list4 = new List<UnsafeNativeMethods.POINTER_DEVICE_PROPERTY>();
				bool flag2 = false;
				UnsafeNativeMethods.POINTER_DEVICE_PROPERTY[] supportedPointerProperties = SupportedPointerProperties;
				foreach (UnsafeNativeMethods.POINTER_DEVICE_PROPERTY pOINTER_DEVICE_PROPERTY in supportedPointerProperties)
				{
					StylusPointPropertyInfo stylusPointPropertyInfo = PointerStylusPointPropertyInfoHelper.CreatePropertyInfo(pOINTER_DEVICE_PROPERTY);
					if (stylusPointPropertyInfo != null)
					{
						if (stylusPointPropertyInfo.Id == StylusPointPropertyIds.NormalPressure)
						{
							flag2 = true;
							list[2] = stylusPointPropertyInfo;
							list3.Insert(2, pOINTER_DEVICE_PROPERTY);
						}
						else if (stylusPointPropertyInfo.Id == StylusPointPropertyIds.X)
						{
							list[0] = stylusPointPropertyInfo;
							list3[0] = pOINTER_DEVICE_PROPERTY;
						}
						else if (stylusPointPropertyInfo.Id == StylusPointPropertyIds.Y)
						{
							list[1] = stylusPointPropertyInfo;
							list3[1] = pOINTER_DEVICE_PROPERTY;
						}
						else if (stylusPointPropertyInfo.IsButton)
						{
							list2.Add(stylusPointPropertyInfo);
							list4.Add(pOINTER_DEVICE_PROPERTY);
						}
						else
						{
							list.Add(stylusPointPropertyInfo);
							list3.Add(pOINTER_DEVICE_PROPERTY);
						}
					}
				}
				if (flag2)
				{
					PressureIndex = 2;
					UsingFakePressure = false;
					HardwareCapabilities |= TabletHardwareCapabilities.SupportsPressure;
				}
				list.AddRange(list2);
				SupportedButtonPropertyIndex = list3.Count;
				list3.AddRange(list4);
				StylusPointProperties = new ReadOnlyCollection<StylusPointProperty>(list);
				SupportedPointerProperties = list3.ToArray();
			}
		}
		return flag;
	}

	private bool TryInitializeDeviceRects()
	{
		UnsafeNativeMethods.RECT pointerDeviceRect = default(UnsafeNativeMethods.RECT);
		UnsafeNativeMethods.RECT displayRect = default(UnsafeNativeMethods.RECT);
		bool pointerDeviceRects = UnsafeNativeMethods.GetPointerDeviceRects(_deviceInfo.device, ref pointerDeviceRect, ref displayRect);
		if (pointerDeviceRects)
		{
			DeviceRect = pointerDeviceRect;
			DisplayRect = displayRect;
			SizeInfo = new TabletDeviceSizeInfo(new Size(SupportedPointerProperties[0].logicalMax, SupportedPointerProperties[1].logicalMax), new Size(displayRect.right - displayRect.left, displayRect.bottom - displayRect.top));
		}
		return pointerDeviceRects;
	}
}
