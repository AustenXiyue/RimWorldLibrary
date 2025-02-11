using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Input.StylusPointer;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Interop;
using MS.Win32;
using MS.Win32.Pointer;

namespace System.Windows.Interop;

internal sealed class HwndPointerInputProvider : DispatcherObject, IStylusInputProvider, IInputProvider, IDisposable
{
	private bool _disposed;

	private SecurityCriticalDataClass<HwndSource> _source;

	private SecurityCriticalDataClass<InputProviderSite> _site;

	private SecurityCriticalDataClass<PointerLogic> _pointerLogic;

	private PointerStylusDevice _currentStylusDevice;

	private PointerTabletDevice _currentTabletDevice;

	internal bool IsWindowEnabled { get; private set; }

	internal HwndPointerInputProvider(HwndSource source)
	{
		_site = new SecurityCriticalDataClass<InputProviderSite>(InputManager.Current.RegisterInputProvider(this));
		_source = new SecurityCriticalDataClass<HwndSource>(source);
		_pointerLogic = new SecurityCriticalDataClass<PointerLogic>(StylusLogic.GetCurrentStylusLogicAs<PointerLogic>());
		_pointerLogic.Value.PlugInManagers[_source.Value] = new PointerStylusPlugInManager(_source.Value);
		int windowLong = MS.Win32.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, source.CriticalHandle), -16);
		IsWindowEnabled = (windowLong & 0x8000000) == 0;
	}

	~HwndPointerInputProvider()
	{
		Dispose(disposing: false);
	}

	private void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			if (_site != null)
			{
				_site.Value.Dispose();
				_site = null;
			}
			_pointerLogic.Value.PlugInManagers.Remove(_source.Value);
		}
		_disposed = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private uint GetPointerId(nint wParam)
	{
		return (uint)MS.Win32.NativeMethods.SignedLOWORD(wParam);
	}

	private int[] GenerateRawStylusData(PointerData pointerData, PointerTabletDevice tabletDevice)
	{
		int num = tabletDevice.DeviceInfo.SupportedPointerProperties.Length;
		int[] array = new int[num * pointerData.Info.historyCount];
		int[] array2 = Array.Empty<int>();
		if (UnsafeNativeMethods.GetRawPointerDeviceData(pointerData.Info.pointerId, pointerData.Info.historyCount, (uint)num, tabletDevice.DeviceInfo.SupportedPointerProperties, array))
		{
			GetOriginOffsetsLogical(out var originOffsetX, out var originOffsetY);
			int num2 = tabletDevice.DeviceInfo.SupportedPointerProperties.Length - tabletDevice.DeviceInfo.SupportedButtonPropertyIndex;
			int num3 = ((num2 > 0) ? (num - num2 + 1) : num);
			array2 = new int[num3 * pointerData.Info.historyCount];
			int num4 = 0;
			int num5 = array.Length - num;
			while (num4 < array2.Length)
			{
				Array.Copy(array, num5, array2, num4, num3);
				array2[num4] -= originOffsetX;
				array2[num4 + 1] -= originOffsetY;
				if (num2 > 0)
				{
					int num6 = num4 + num3 - 1;
					array2[num6] = 0;
					for (int i = tabletDevice.DeviceInfo.SupportedButtonPropertyIndex; i < num; i++)
					{
						int num7 = array[num5 + i] << i - tabletDevice.DeviceInfo.SupportedButtonPropertyIndex;
						array2[num6] |= num7;
					}
				}
				num4 += num3;
				num5 -= num;
			}
		}
		return array2;
	}

	private bool ProcessMessage(uint pointerId, RawStylusActions action, int timestamp)
	{
		bool result = false;
		PointerData pointerData = new PointerData(pointerId);
		if (pointerData.IsValid && (pointerData.Info.pointerType == UnsafeNativeMethods.POINTER_INPUT_TYPE.PT_TOUCH || pointerData.Info.pointerType == UnsafeNativeMethods.POINTER_INPUT_TYPE.PT_PEN))
		{
			uint cursorId = 0u;
			if (UnsafeNativeMethods.GetPointerCursorId(pointerId, ref cursorId))
			{
				nint sourceDevice = pointerData.Info.sourceDevice;
				if (!UpdateCurrentTabletAndStylus(sourceDevice, cursorId))
				{
					return false;
				}
				if (action == RawStylusActions.Move && !pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_INCONTACT) && pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_INRANGE))
				{
					action = RawStylusActions.InAirMove;
				}
				RawStylusInputReport rawStylusInputReport = new RawStylusInputReport(InputMode.Foreground, timestamp, _source.Value, action, () => _currentTabletDevice.StylusPointDescription, _currentTabletDevice.Id, _currentStylusDevice.Id, GenerateRawStylusData(pointerData, _currentTabletDevice))
				{
					StylusDevice = _currentStylusDevice.StylusDevice
				};
				if (!_pointerLogic.Value.InDragDrop && IsWindowEnabled && _pointerLogic.Value.PlugInManagers.TryGetValue(_source.Value, out var value))
				{
					value.InvokeStylusPluginCollection(rawStylusInputReport);
				}
				_currentStylusDevice.Update(this, _source.Value, pointerData, rawStylusInputReport);
				_currentStylusDevice.UpdateInteractions(rawStylusInputReport);
				InputReportEventArgs input = new InputReportEventArgs(_currentStylusDevice.StylusDevice, rawStylusInputReport)
				{
					RoutedEvent = InputManager.PreviewInputReportEvent
				};
				InputManager.UnsecureCurrent.ProcessInput(input);
				result = !_currentStylusDevice.IsPrimary;
			}
		}
		return result;
	}

	private void GetOriginOffsetsLogical(out int originOffsetX, out int originOffsetY)
	{
		Point point = _source.Value.RootVisual.PointToScreen(new Point(0.0, 0.0));
		MatrixTransform matrixTransform = new MatrixTransform(_currentTabletDevice.TabletToScreen);
		matrixTransform = (MatrixTransform)matrixTransform.Inverse;
		Point point2 = point * matrixTransform.Matrix;
		originOffsetX = (int)Math.Round(point2.X);
		originOffsetY = (int)Math.Round(point2.Y);
	}

	private bool UpdateCurrentTabletAndStylus(nint deviceId, uint cursorId)
	{
		PointerTabletDeviceCollection pointerTabletDeviceCollection = Tablet.TabletDevices?.As<PointerTabletDeviceCollection>();
		if (!pointerTabletDeviceCollection.IsValid)
		{
			pointerTabletDeviceCollection.Refresh();
			if (!pointerTabletDeviceCollection.IsValid)
			{
				return false;
			}
		}
		_currentTabletDevice = pointerTabletDeviceCollection?.GetByDeviceId(deviceId);
		_currentStylusDevice = _currentTabletDevice?.GetStylusByCursorId(cursorId);
		if (_currentTabletDevice == null || _currentStylusDevice == null)
		{
			pointerTabletDeviceCollection.Refresh();
			_currentTabletDevice = pointerTabletDeviceCollection?.GetByDeviceId(deviceId);
			_currentStylusDevice = _currentTabletDevice?.GetStylusByCursorId(cursorId);
			if (_currentTabletDevice == null || _currentStylusDevice == null)
			{
				return false;
			}
		}
		return true;
	}

	nint IStylusInputProvider.FilterMessage(nint hwnd, WindowMessage msg, nint wParam, nint lParam, ref bool handled)
	{
		handled = false;
		if (PointerLogic.IsEnabled)
		{
			switch (msg)
			{
			case WindowMessage.WM_ENABLE:
				IsWindowEnabled = MS.Win32.NativeMethods.IntPtrToInt32(wParam) == 1;
				break;
			case WindowMessage.WM_POINTERENTER:
				handled = ProcessMessage(GetPointerId(wParam), RawStylusActions.InRange, Environment.TickCount);
				break;
			case WindowMessage.WM_POINTERUPDATE:
				handled = ProcessMessage(GetPointerId(wParam), RawStylusActions.Move, Environment.TickCount);
				break;
			case WindowMessage.WM_POINTERDOWN:
				handled = ProcessMessage(GetPointerId(wParam), RawStylusActions.Down, Environment.TickCount);
				break;
			case WindowMessage.WM_POINTERUP:
				handled = ProcessMessage(GetPointerId(wParam), RawStylusActions.Up, Environment.TickCount);
				break;
			case WindowMessage.WM_POINTERLEAVE:
				handled = ProcessMessage(GetPointerId(wParam), RawStylusActions.OutOfRange, Environment.TickCount);
				break;
			}
		}
		return IntPtr.Zero;
	}

	public bool ProvidesInputForRootVisual(Visual v)
	{
		return false;
	}

	public void NotifyDeactivate()
	{
	}
}
