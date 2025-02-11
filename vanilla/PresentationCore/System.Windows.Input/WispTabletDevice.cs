using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Input.StylusWisp;
using System.Windows.Input.Tracing;
using MS.Win32;
using MS.Win32.Penimc;

namespace System.Windows.Input;

internal class WispTabletDevice : TabletDeviceBase
{
	private PenThread _penThread;

	protected Size _cancelSize = Size.Empty;

	private StylusDeviceCollection _stylusDeviceCollection;

	private bool _isDisposalPending;

	private int _queuedEventCount;

	internal override IInputElement Target
	{
		get
		{
			VerifyAccess();
			return Stylus.CurrentStylusDevice?.Target;
		}
	}

	internal override PresentationSource ActiveSource
	{
		get
		{
			VerifyAccess();
			return Stylus.CurrentStylusDevice?.ActiveSource;
		}
	}

	internal override StylusDeviceCollection StylusDevices
	{
		get
		{
			VerifyAccess();
			return _stylusDeviceCollection;
		}
	}

	internal PenThread PenThread => _penThread;

	internal override Size DoubleTapSize => _doubleTapSize;

	internal Size CancelSize => _cancelSize;

	internal bool IsDisposalPending => _isDisposalPending;

	internal bool CanDispose => _queuedEventCount == 0;

	internal int QueuedEventCount
	{
		get
		{
			return _queuedEventCount;
		}
		set
		{
			_queuedEventCount = value;
		}
	}

	internal uint WispTabletKey => _tabletInfo.WispTabletKey;

	internal WispTabletDevice(TabletDeviceInfo tabletInfo, PenThread penThread)
		: base(tabletInfo)
	{
		_penThread = penThread;
		_penThread.WorkerAcquireTabletLocks(tabletInfo.PimcTablet.Value, tabletInfo.WispTabletKey);
		int num = tabletInfo.StylusDevicesInfo.Length;
		WispStylusDevice[] array = new WispStylusDevice[num];
		for (int i = 0; i < num; i++)
		{
			StylusDeviceInfo stylusDeviceInfo = tabletInfo.StylusDevicesInfo[i];
			array[i] = new WispStylusDevice(this, stylusDeviceInfo.CursorName, stylusDeviceInfo.CursorId, stylusDeviceInfo.CursorInverted, stylusDeviceInfo.ButtonCollection);
		}
		_stylusDeviceCollection = new StylusDeviceCollection(array);
		StylusTraceLogger.LogDeviceConnect(new StylusTraceLogger.StylusDeviceInfo(base.Id, base.Name, base.ProductId, base.TabletHardwareCapabilities, base.TabletSize, base.ScreenSize, _tabletInfo.DeviceType, StylusDevices.Count));
	}

	protected override void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				DisposeOrDeferDisposal();
			}
			else
			{
				_disposed = true;
			}
		}
	}

	internal WispStylusDevice UpdateStylusDevices(int stylusId)
	{
		StylusDeviceInfo[] array = _penThread.WorkerRefreshCursorInfo(_tabletInfo.PimcTablet.Value);
		int num = array.Length;
		if (num > StylusDevices.Count)
		{
			for (int i = 0; i < num; i++)
			{
				StylusDeviceInfo stylusDeviceInfo = array[i];
				if (stylusDeviceInfo.CursorId == stylusId)
				{
					WispStylusDevice wispStylusDevice = new WispStylusDevice(this, stylusDeviceInfo.CursorName, stylusDeviceInfo.CursorId, stylusDeviceInfo.CursorInverted, stylusDeviceInfo.ButtonCollection);
					StylusDevices.AddStylusDevice(i, wispStylusDevice);
					return wispStylusDevice;
				}
			}
		}
		return null;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0}({1})", base.ToString(), base.Name);
	}

	internal PenContext CreateContext(nint hwnd, PenContexts contexts)
	{
		bool supportInRange = (base.TabletHardwareCapabilities & TabletHardwareCapabilities.HardProximity) != 0;
		bool isIntegrated = (base.TabletHardwareCapabilities & TabletHardwareCapabilities.Integrated) != 0;
		PenContextInfo penContextInfo = _penThread.WorkerCreateContext(hwnd, _tabletInfo.PimcTablet.Value);
		return new PenContext((penContextInfo.PimcContext != null) ? penContextInfo.PimcContext.Value : null, hwnd, contexts, supportInRange, isIntegrated, penContextInfo.ContextId, (penContextInfo.CommHandle != null) ? penContextInfo.CommHandle.Value : IntPtr.Zero, base.Id, penContextInfo.WispContextKey);
	}

	internal void UpdateScreenMeasurements()
	{
		_cancelSize = Size.Empty;
		_doubleTapSize = Size.Empty;
		_tabletInfo.SizeInfo = _penThread.WorkerGetUpdatedSizes(_tabletInfo.PimcTablet.Value);
	}

	internal void InvalidateSizeDeltas()
	{
		_forceUpdateSizeDeltas = true;
	}

	internal bool AreSizeDeltasValid()
	{
		if (!_doubleTapSize.IsEmpty)
		{
			return !_cancelSize.IsEmpty;
		}
		return false;
	}

	internal void UpdateSizeDeltas(StylusPointDescription description, WispLogic stylusLogic)
	{
		Size cancelSize = new Size(Math.Max(1, SafeSystemMetrics.DragDeltaX / 2), Math.Max(1, SafeSystemMetrics.DragDeltaY / 2));
		Size doubleTapSize = new Size(Math.Max(1, SafeSystemMetrics.DoubleClickDeltaX / 2), Math.Max(1, SafeSystemMetrics.DoubleClickDeltaY / 2));
		StylusPointPropertyInfo propertyInfo = description.GetPropertyInfo(StylusPointProperties.X);
		StylusPointPropertyInfo propertyInfo2 = description.GetPropertyInfo(StylusPointProperties.Y);
		uint propertyValue = TabletDeviceBase.GetPropertyValue(propertyInfo);
		uint propertyValue2 = TabletDeviceBase.GetPropertyValue(propertyInfo2);
		if (propertyValue != 0 && propertyValue2 != 0)
		{
			_cancelSize = new Size((int)Math.Round(base.ScreenSize.Width * (double)stylusLogic.CancelDelta / (double)propertyValue), (int)Math.Round(base.ScreenSize.Height * (double)stylusLogic.CancelDelta / (double)propertyValue2));
			_cancelSize.Width = Math.Max(cancelSize.Width, _cancelSize.Width);
			_cancelSize.Height = Math.Max(cancelSize.Height, _cancelSize.Height);
			_doubleTapSize = new Size((int)Math.Round(base.ScreenSize.Width * (double)stylusLogic.DoubleTapDelta / (double)propertyValue), (int)Math.Round(base.ScreenSize.Height * (double)stylusLogic.DoubleTapDelta / (double)propertyValue2));
			_doubleTapSize.Width = Math.Max(doubleTapSize.Width, _doubleTapSize.Width);
			_doubleTapSize.Height = Math.Max(doubleTapSize.Height, _doubleTapSize.Height);
		}
		else
		{
			_doubleTapSize = doubleTapSize;
			_cancelSize = cancelSize;
		}
		_forceUpdateSizeDeltas = false;
	}

	internal void DisposeOrDeferDisposal()
	{
		if (CanDispose)
		{
			if (Tablet.CurrentTabletDevice == base.TabletDevice)
			{
				StylusLogic.GetCurrentStylusLogicAs<WispLogic>().SelectStylusDevice(null, null, updateOver: true);
			}
			StylusTraceLogger.LogDeviceDisconnect(_tabletInfo.Id);
			IPimcTablet3 pimcTablet = _tabletInfo.PimcTablet?.Value;
			_tabletInfo.PimcTablet = null;
			if (pimcTablet != null)
			{
				PenThread.WorkerReleaseTabletLocks(pimcTablet, _tabletInfo.WispTabletKey);
				Marshal.ReleaseComObject(pimcTablet);
			}
			StylusDeviceCollection stylusDeviceCollection = _stylusDeviceCollection;
			_stylusDeviceCollection = null;
			stylusDeviceCollection?.Dispose();
			_penThread = null;
			_isDisposalPending = false;
			_disposed = true;
			GC.SuppressFinalize(this);
		}
		else
		{
			_isDisposalPending = true;
		}
	}
}
