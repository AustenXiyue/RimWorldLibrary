using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Threading;

namespace System.Windows.Input;

internal abstract class TabletDeviceBase : DispatcherObject, IDisposable
{
	private const uint DefaultPropertyValue = 1000u;

	protected bool _disposed;

	protected Size _doubleTapSize = Size.Empty;

	protected bool _forceUpdateSizeDeltas;

	private MultiTouchSystemGestureLogic _multiTouchSystemGestureLogic;

	protected TabletDeviceInfo _tabletInfo;

	protected StylusPointDescription _stylusPointDescription;

	internal TabletDevice TabletDevice { get; private set; }

	internal abstract IInputElement Target { get; }

	internal abstract PresentationSource ActiveSource { get; }

	internal int Id
	{
		get
		{
			VerifyAccess();
			return _tabletInfo.Id;
		}
	}

	internal string Name
	{
		get
		{
			VerifyAccess();
			return _tabletInfo.Name;
		}
	}

	internal string ProductId
	{
		get
		{
			VerifyAccess();
			return _tabletInfo.PlugAndPlayId;
		}
	}

	internal TabletHardwareCapabilities TabletHardwareCapabilities
	{
		get
		{
			VerifyAccess();
			return _tabletInfo.HardwareCapabilities;
		}
	}

	internal ReadOnlyCollection<StylusPointProperty> SupportedStylusPointProperties
	{
		get
		{
			VerifyAccess();
			return _tabletInfo.StylusPointProperties;
		}
	}

	internal TabletDeviceType Type
	{
		get
		{
			VerifyAccess();
			return _tabletInfo.DeviceType;
		}
	}

	internal abstract StylusDeviceCollection StylusDevices { get; }

	internal Matrix TabletToScreen => new Matrix(_tabletInfo.SizeInfo.ScreenSize.Width / _tabletInfo.SizeInfo.TabletSize.Width, 0.0, 0.0, _tabletInfo.SizeInfo.ScreenSize.Height / _tabletInfo.SizeInfo.TabletSize.Height, 0.0, 0.0);

	internal Size TabletSize => _tabletInfo.SizeInfo.TabletSize;

	internal Size ScreenSize => _tabletInfo.SizeInfo.ScreenSize;

	internal abstract Size DoubleTapSize { get; }

	internal StylusPointDescription StylusPointDescription
	{
		get
		{
			if (_stylusPointDescription == null)
			{
				ReadOnlyCollection<StylusPointProperty> supportedStylusPointProperties = SupportedStylusPointProperties;
				List<StylusPointPropertyInfo> list = new List<StylusPointPropertyInfo>();
				foreach (StylusPointProperty item in supportedStylusPointProperties)
				{
					list.Add((item is StylusPointPropertyInfo) ? ((StylusPointPropertyInfo)item) : new StylusPointPropertyInfo(item));
				}
				_stylusPointDescription = new StylusPointDescription(list, _tabletInfo.PressureIndex);
			}
			return _stylusPointDescription;
		}
	}

	internal T As<T>() where T : TabletDeviceBase
	{
		return this as T;
	}

	protected TabletDeviceBase(TabletDeviceInfo info)
	{
		TabletDevice = new TabletDevice(this);
		_tabletInfo = info;
		if (_tabletInfo.DeviceType == TabletDeviceType.Touch)
		{
			_multiTouchSystemGestureLogic = new MultiTouchSystemGestureLogic();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		_disposed = true;
	}

	~TabletDeviceBase()
	{
		Dispose(disposing: false);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0}({1})", base.ToString(), Name);
	}

	internal SystemGesture? GenerateStaticGesture(RawStylusInputReport stylusInputReport)
	{
		return _multiTouchSystemGestureLogic?.GenerateStaticGesture(stylusInputReport);
	}

	protected static uint GetPropertyValue(StylusPointPropertyInfo propertyInfo)
	{
		uint result = 1000u;
		switch (propertyInfo.Unit)
		{
		case StylusPointPropertyUnit.Inches:
			if (propertyInfo.Resolution != 0f)
			{
				result = (uint)((float)((propertyInfo.Maximum - propertyInfo.Minimum) * 254) / propertyInfo.Resolution);
			}
			break;
		case StylusPointPropertyUnit.Centimeters:
			if (propertyInfo.Resolution != 0f)
			{
				result = (uint)((float)((propertyInfo.Maximum - propertyInfo.Minimum) * 100) / propertyInfo.Resolution);
			}
			break;
		}
		return result;
	}
}
