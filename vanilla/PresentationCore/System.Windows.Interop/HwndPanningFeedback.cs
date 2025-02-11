using System.Runtime.InteropServices;
using System.Windows.Threading;
using MS.Win32;

namespace System.Windows.Interop;

internal class HwndPanningFeedback
{
	private int _deviceOffsetX;

	private int _deviceOffsetY;

	private bool _inInertia;

	private DispatcherOperation _updatePanningOperation;

	private bool _isProvidingPanningFeedback;

	private HwndSource _hwndSource;

	private static bool IsSupported => OperatingSystemVersionCheck.IsVersionOrLater(OperatingSystemVersion.Windows7);

	private HandleRef Handle
	{
		get
		{
			if (_hwndSource != null)
			{
				nint criticalHandle = _hwndSource.CriticalHandle;
				if (criticalHandle != IntPtr.Zero)
				{
					return new HandleRef(_hwndSource, criticalHandle);
				}
			}
			return default(HandleRef);
		}
	}

	public HwndPanningFeedback(HwndSource hwndSource)
	{
		if (hwndSource == null)
		{
			throw new ArgumentNullException("hwndSource");
		}
		_hwndSource = hwndSource;
	}

	public void UpdatePanningFeedback(Vector totalOverpanOffset, bool inInertia)
	{
		if (_hwndSource == null || !IsSupported)
		{
			return;
		}
		if (!_isProvidingPanningFeedback)
		{
			_isProvidingPanningFeedback = MS.Win32.UnsafeNativeMethods.BeginPanningFeedback(Handle);
		}
		if (_isProvidingPanningFeedback)
		{
			Point point = _hwndSource.TransformToDevice((Point)totalOverpanOffset);
			_deviceOffsetX = (int)point.X;
			_deviceOffsetY = (int)point.Y;
			_inInertia = inInertia;
			if (_updatePanningOperation == null)
			{
				_updatePanningOperation = _hwndSource.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(OnUpdatePanningFeedback), this);
			}
		}
	}

	private object OnUpdatePanningFeedback(object args)
	{
		HwndPanningFeedback hwndPanningFeedback = (HwndPanningFeedback)args;
		_updatePanningOperation = null;
		MS.Win32.UnsafeNativeMethods.UpdatePanningFeedback(hwndPanningFeedback.Handle, hwndPanningFeedback._deviceOffsetX, hwndPanningFeedback._deviceOffsetY, hwndPanningFeedback._inInertia);
		return null;
	}

	public void EndPanningFeedback(bool animateBack)
	{
		if (_hwndSource != null && _isProvidingPanningFeedback)
		{
			_isProvidingPanningFeedback = false;
			if (_updatePanningOperation != null)
			{
				_updatePanningOperation.Abort();
				_updatePanningOperation = null;
			}
			MS.Win32.UnsafeNativeMethods.EndPanningFeedback(Handle, animateBack);
		}
	}
}
