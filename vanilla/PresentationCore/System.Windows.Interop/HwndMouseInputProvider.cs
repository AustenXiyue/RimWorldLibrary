using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Interop;
using MS.Utility;
using MS.Win32;

namespace System.Windows.Interop;

internal sealed class HwndMouseInputProvider : DispatcherObject, IMouseInputProvider, IInputProvider, IDisposable
{
	private enum SetCursorState
	{
		SetCursorNotReceived,
		SetCursorReceived,
		SetCursorDisabled
	}

	private SecurityCriticalDataClass<HwndSource> _source;

	private SecurityCriticalDataClass<InputProviderSite> _site;

	private int _msgTime;

	private MS.Win32.NativeMethods.MOUSEMOVEPOINT _latestMovePoint;

	private MS.Win32.NativeMethods.MOUSEMOVEPOINT _previousMovePoint;

	private int _lastX;

	private int _lastY;

	private bool _tracking;

	private bool _active;

	private SetCursorState _setCursorState;

	private bool _haveCapture;

	private DispatcherOperation _queryCursorOperation;

	private bool _isDwmProcess;

	private MS.Win32.NativeMethods.TRACKMOUSEEVENT _tme = new MS.Win32.NativeMethods.TRACKMOUSEEVENT();

	private const string PresentationFrameworkAssemblyFullName = "PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

	private static DependencyProperty WindowChromeWorkerProperty;

	private static MethodInfo GetEffectiveClientAreaMI;

	internal HwndMouseInputProvider(HwndSource source)
	{
		_site = new SecurityCriticalDataClass<InputProviderSite>(InputManager.Current.RegisterInputProvider(this));
		_source = new SecurityCriticalDataClass<HwndSource>(source);
		_setCursorState = SetCursorState.SetCursorNotReceived;
		_haveCapture = false;
		_queryCursorOperation = null;
	}

	public void Dispose()
	{
		if (_site != null)
		{
			StopTracking(_source.Value.CriticalHandle);
			try
			{
				if (_source.Value.HasCapture)
				{
					SafeNativeMethods.ReleaseCapture();
				}
			}
			catch (Win32Exception)
			{
			}
			try
			{
				nint capture = SafeNativeMethods.GetCapture();
				PossiblyDeactivate(capture, stillActiveIfOverSelf: false);
			}
			catch (Win32Exception)
			{
			}
			_site.Value.Dispose();
			_site = null;
		}
		_source = null;
	}

	bool IInputProvider.ProvidesInputForRootVisual(Visual v)
	{
		return _source.Value.RootVisual == v;
	}

	void IInputProvider.NotifyDeactivate()
	{
		if (_active)
		{
			StopTracking(_source.Value.CriticalHandle);
			_active = false;
		}
	}

	bool IMouseInputProvider.SetCursor(Cursor cursor)
	{
		bool result = false;
		if (_setCursorState != SetCursorState.SetCursorDisabled)
		{
			try
			{
				SafeNativeMethods.SetCursor(cursor.Handle);
				result = true;
			}
			catch (Win32Exception)
			{
			}
		}
		return result;
	}

	bool IMouseInputProvider.CaptureMouse()
	{
		if (_isDwmProcess)
		{
			return true;
		}
		bool flag = true;
		try
		{
			SafeNativeMethods.SetCapture(new HandleRef(this, _source.Value.CriticalHandle));
			if (SafeNativeMethods.GetCapture() != _source.Value.CriticalHandle)
			{
				flag = false;
			}
		}
		catch (Win32Exception)
		{
			flag = false;
		}
		if (flag)
		{
			_haveCapture = true;
		}
		if (flag && !_active)
		{
			MS.Win32.NativeMethods.POINT pt = default(MS.Win32.NativeMethods.POINT);
			flag = MS.Win32.UnsafeNativeMethods.TryGetCursorPos(ref pt);
			if (flag)
			{
				try
				{
					SafeNativeMethods.ScreenToClient(new HandleRef(this, _source.Value.CriticalHandle), ref pt);
				}
				catch (Win32Exception)
				{
					flag = false;
				}
				if (flag)
				{
					ReportInput(_source.Value.CriticalHandle, InputMode.Foreground, _msgTime, RawMouseActions.AbsoluteMove, pt.x, pt.y, 0);
				}
			}
		}
		return flag;
	}

	void IMouseInputProvider.ReleaseMouseCapture()
	{
		_haveCapture = false;
		if (_isDwmProcess)
		{
			return;
		}
		try
		{
			SafeNativeMethods.ReleaseCapture();
		}
		catch (Win32Exception)
		{
		}
	}

	int IMouseInputProvider.GetIntermediatePoints(IInputElement relativeTo, Point[] points)
	{
		int num = -1;
		try
		{
			if (points != null && relativeTo != null)
			{
				DependencyObject containingVisual = InputElement.GetContainingVisual(relativeTo as DependencyObject);
				if (PresentationSource.FromDependencyObject(containingVisual) is HwndSource hwndSource)
				{
					int systemMetrics = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.CXVIRTUALSCREEN);
					int systemMetrics2 = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.CYVIRTUALSCREEN);
					int systemMetrics3 = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.XVIRTUALSCREEN);
					int systemMetrics4 = MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.YVIRTUALSCREEN);
					uint num2 = 1u;
					MS.Win32.NativeMethods.MOUSEMOVEPOINT pointsIn = default(MS.Win32.NativeMethods.MOUSEMOVEPOINT);
					MS.Win32.NativeMethods.MOUSEMOVEPOINT[] array = new MS.Win32.NativeMethods.MOUSEMOVEPOINT[64];
					pointsIn.x = _latestMovePoint.x;
					pointsIn.y = _latestMovePoint.y;
					pointsIn.time = 0;
					int mouseMovePointsEx = MS.Win32.UnsafeNativeMethods.GetMouseMovePointsEx((uint)Marshal.SizeOf(pointsIn), ref pointsIn, array, 64, num2);
					if (mouseMovePointsEx == -1)
					{
						throw new Win32Exception();
					}
					num = 0;
					bool flag = true;
					for (int i = 0; i < mouseMovePointsEx && num < points.Length; i++)
					{
						if (flag)
						{
							if (array[i].time >= _latestMovePoint.time && (array[i].time != _latestMovePoint.time || array[i].x != _latestMovePoint.x || array[i].y != _latestMovePoint.y))
							{
								continue;
							}
							flag = false;
						}
						if (array[i].time < _previousMovePoint.time || (array[i].time == _previousMovePoint.time && array[i].x == _previousMovePoint.x && array[i].y == _previousMovePoint.y))
						{
							break;
						}
						Point pointScreen = new Point(array[i].x, array[i].y);
						switch (num2)
						{
						case 1u:
							if (pointScreen.X > 32767.0)
							{
								pointScreen.X -= 65536.0;
							}
							if (pointScreen.Y > 32767.0)
							{
								pointScreen.Y -= 65536.0;
							}
							break;
						case 2u:
							pointScreen.X = (pointScreen.X * (double)(systemMetrics - 1) - (double)(systemMetrics3 * 65536)) / (double)systemMetrics;
							pointScreen.Y = (pointScreen.Y * (double)(systemMetrics2 - 1) - (double)(systemMetrics4 * 65536)) / (double)systemMetrics2;
							break;
						}
						pointScreen = PointUtil.ScreenToClient(pointScreen, hwndSource);
						pointScreen = PointUtil.ClientToRoot(pointScreen, hwndSource);
						hwndSource.RootVisual.TransformToDescendant(VisualTreeHelper.GetContainingVisual2D(containingVisual))?.TryTransform(pointScreen, out pointScreen);
						points[num++] = pointScreen;
					}
				}
			}
		}
		catch (Win32Exception)
		{
			num = -1;
		}
		return num;
	}

	internal unsafe nint FilterMessage(nint hwnd, WindowMessage msg, nint wParam, nint lParam, ref bool handled)
	{
		nint zero = IntPtr.Zero;
		if (_source == null || _source.Value == null)
		{
			return zero;
		}
		_msgTime = 0;
		try
		{
			_msgTime = SafeNativeMethods.GetMessageTime();
		}
		catch (Win32Exception)
		{
		}
		if (msg == WindowMessage.WM_MOUSEQUERY)
		{
			if (!_isDwmProcess)
			{
				_isDwmProcess = true;
			}
			MS.Win32.UnsafeNativeMethods.MOUSEQUERY* ptr = (MS.Win32.UnsafeNativeMethods.MOUSEQUERY*)lParam;
			if (ptr->uMsg == 512)
			{
				msg = (WindowMessage)ptr->uMsg;
				wParam = ptr->wParam;
				lParam = MakeLPARAM(ptr->ptX, ptr->ptY);
			}
		}
		switch (msg)
		{
		case WindowMessage.WM_NCDESTROY:
			Dispose();
			break;
		case WindowMessage.WM_MOUSEMOVE:
		{
			int x = MS.Win32.NativeMethods.SignedLOWORD(lParam);
			int y = MS.Win32.NativeMethods.SignedHIWORD(lParam);
			if (_queryCursorOperation != null)
			{
				_queryCursorOperation.Abort();
				_queryCursorOperation = null;
			}
			if (_haveCapture)
			{
				_setCursorState = SetCursorState.SetCursorReceived;
			}
			else if (_setCursorState == SetCursorState.SetCursorNotReceived)
			{
				_setCursorState = SetCursorState.SetCursorDisabled;
			}
			else if (_setCursorState == SetCursorState.SetCursorReceived)
			{
				_setCursorState = SetCursorState.SetCursorNotReceived;
			}
			handled = ReportInput(hwnd, InputMode.Foreground, _msgTime, RawMouseActions.AbsoluteMove, x, y, 0);
			break;
		}
		case WindowMessage.WM_MOUSEWHEEL:
		{
			int wheel = MS.Win32.NativeMethods.SignedHIWORD(wParam);
			int x10 = MS.Win32.NativeMethods.SignedLOWORD(lParam);
			int y10 = MS.Win32.NativeMethods.SignedHIWORD(lParam);
			MS.Win32.NativeMethods.POINT pt = new MS.Win32.NativeMethods.POINT(x10, y10);
			try
			{
				SafeNativeMethods.ScreenToClient(new HandleRef(this, hwnd), ref pt);
				x10 = pt.x;
				y10 = pt.y;
				handled = ReportInput(hwnd, InputMode.Foreground, _msgTime, RawMouseActions.VerticalWheelRotate, x10, y10, wheel);
			}
			catch (Win32Exception)
			{
			}
			break;
		}
		case WindowMessage.WM_LBUTTONDOWN:
		case WindowMessage.WM_LBUTTONDBLCLK:
		{
			int x3 = MS.Win32.NativeMethods.SignedLOWORD(lParam);
			int y3 = MS.Win32.NativeMethods.SignedHIWORD(lParam);
			handled = ReportInput(hwnd, InputMode.Foreground, _msgTime, RawMouseActions.Button1Press, x3, y3, 0);
			break;
		}
		case WindowMessage.WM_LBUTTONUP:
		{
			int x8 = MS.Win32.NativeMethods.SignedLOWORD(lParam);
			int y8 = MS.Win32.NativeMethods.SignedHIWORD(lParam);
			handled = ReportInput(hwnd, InputMode.Foreground, _msgTime, RawMouseActions.Button1Release, x8, y8, 0);
			break;
		}
		case WindowMessage.WM_RBUTTONDOWN:
		case WindowMessage.WM_RBUTTONDBLCLK:
		{
			int x7 = MS.Win32.NativeMethods.SignedLOWORD(lParam);
			int y7 = MS.Win32.NativeMethods.SignedHIWORD(lParam);
			handled = ReportInput(hwnd, InputMode.Foreground, _msgTime, RawMouseActions.Button2Press, x7, y7, 0);
			break;
		}
		case WindowMessage.WM_RBUTTONUP:
		{
			int x9 = MS.Win32.NativeMethods.SignedLOWORD(lParam);
			int y9 = MS.Win32.NativeMethods.SignedHIWORD(lParam);
			handled = ReportInput(hwnd, InputMode.Foreground, _msgTime, RawMouseActions.Button2Release, x9, y9, 0);
			break;
		}
		case WindowMessage.WM_MBUTTONDOWN:
		case WindowMessage.WM_MBUTTONDBLCLK:
		{
			int x5 = MS.Win32.NativeMethods.SignedLOWORD(lParam);
			int y5 = MS.Win32.NativeMethods.SignedHIWORD(lParam);
			handled = ReportInput(hwnd, InputMode.Foreground, _msgTime, RawMouseActions.Button3Press, x5, y5, 0);
			break;
		}
		case WindowMessage.WM_MBUTTONUP:
		{
			int x6 = MS.Win32.NativeMethods.SignedLOWORD(lParam);
			int y6 = MS.Win32.NativeMethods.SignedHIWORD(lParam);
			handled = ReportInput(hwnd, InputMode.Foreground, _msgTime, RawMouseActions.Button3Release, x6, y6, 0);
			break;
		}
		case WindowMessage.WM_XBUTTONDOWN:
		case WindowMessage.WM_XBUTTONDBLCLK:
		{
			int num2 = MS.Win32.NativeMethods.SignedHIWORD(wParam);
			int x4 = MS.Win32.NativeMethods.SignedLOWORD(lParam);
			int y4 = MS.Win32.NativeMethods.SignedHIWORD(lParam);
			RawMouseActions actions2 = RawMouseActions.None;
			switch (num2)
			{
			case 1:
				actions2 = RawMouseActions.Button4Press;
				break;
			case 2:
				actions2 = RawMouseActions.Button5Press;
				break;
			}
			handled = ReportInput(hwnd, InputMode.Foreground, _msgTime, actions2, x4, y4, 0);
			break;
		}
		case WindowMessage.WM_XBUTTONUP:
		{
			int num = MS.Win32.NativeMethods.SignedHIWORD(wParam);
			int x2 = MS.Win32.NativeMethods.SignedLOWORD(lParam);
			int y2 = MS.Win32.NativeMethods.SignedHIWORD(lParam);
			RawMouseActions actions = RawMouseActions.None;
			switch (num)
			{
			case 1:
				actions = RawMouseActions.Button4Release;
				break;
			case 2:
				actions = RawMouseActions.Button5Release;
				break;
			}
			handled = ReportInput(hwnd, InputMode.Foreground, _msgTime, actions, x2, y2, 0);
			break;
		}
		case WindowMessage.WM_MOUSELEAVE:
			StopTracking(hwnd);
			try
			{
				nint capture = SafeNativeMethods.GetCapture();
				nint criticalHandle = _source.Value.CriticalHandle;
				if (capture != criticalHandle)
				{
					PossiblyDeactivate(capture, stillActiveIfOverSelf: false);
				}
			}
			catch (Win32Exception)
			{
			}
			break;
		case WindowMessage.WM_CAPTURECHANGED:
			if (lParam != _source.Value.CriticalHandle)
			{
				_haveCapture = false;
				if (_setCursorState == SetCursorState.SetCursorReceived)
				{
					_setCursorState = SetCursorState.SetCursorNotReceived;
				}
				if (!IsOurWindow(lParam) && _active)
				{
					ReportInput(hwnd, InputMode.Foreground, _msgTime, RawMouseActions.CancelCapture, 0, 0, 0);
				}
				if (lParam != IntPtr.Zero || !_tracking)
				{
					PossiblyDeactivate(lParam, stillActiveIfOverSelf: true);
				}
			}
			break;
		case WindowMessage.WM_CANCELMODE:
			try
			{
				if (_source.Value.HasCapture)
				{
					SafeNativeMethods.ReleaseCapture();
				}
			}
			catch (Win32Exception)
			{
			}
			break;
		case WindowMessage.WM_SETCURSOR:
			if (_queryCursorOperation == null)
			{
				_queryCursorOperation = base.Dispatcher.BeginInvoke(DispatcherPriority.Input, (DispatcherOperationCallback)delegate(object sender)
				{
					HwndMouseInputProvider obj = (HwndMouseInputProvider)sender;
					if (obj._active)
					{
						Mouse.UpdateCursor();
					}
					obj._queryCursorOperation = null;
					return (object)null;
				}, this);
			}
			_setCursorState = SetCursorState.SetCursorReceived;
			if (MS.Win32.NativeMethods.SignedLOWORD((int)lParam) == 1)
			{
				handled = true;
			}
			break;
		}
		if (handled && EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordInput, EventTrace.Level.Info))
		{
			int num3 = 0;
			if (_source != null && !_source.Value.IsDisposed && _source.Value.CompositionTarget != null)
			{
				num3 = _source.Value.CompositionTarget.Dispatcher.GetHashCode();
			}
			int num4 = (int)wParam;
			int num5 = (int)lParam;
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientInputMessage, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordInput, EventTrace.Level.Info, num3, ((IntPtr)hwnd).ToInt64(), msg, num4, num5);
		}
		return zero;
	}

	private void PossiblyDeactivate(nint hwndCapture, bool stillActiveIfOverSelf)
	{
		if (_source == null || _source.Value == null || _isDwmProcess)
		{
			return;
		}
		nint num = hwndCapture;
		if (num == IntPtr.Zero)
		{
			MS.Win32.NativeMethods.POINT pt = default(MS.Win32.NativeMethods.POINT);
			int n = 0;
			try
			{
				n = SafeNativeMethods.GetMessagePos();
			}
			catch (Win32Exception)
			{
			}
			pt.x = MS.Win32.NativeMethods.SignedLOWORD(n);
			pt.y = MS.Win32.NativeMethods.SignedHIWORD(n);
			try
			{
				num = MS.Win32.UnsafeNativeMethods.WindowFromPoint(pt.x, pt.y);
			}
			catch (Win32Exception)
			{
			}
			if (!stillActiveIfOverSelf && num == _source.Value.CriticalHandle)
			{
				num = IntPtr.Zero;
			}
			if (num != IntPtr.Zero)
			{
				try
				{
					MS.Win32.NativeMethods.RECT effectiveClientRect = GetEffectiveClientRect(num);
					SafeNativeMethods.ScreenToClient(new HandleRef(this, num), ref pt);
					if (pt.x < effectiveClientRect.left || pt.x >= effectiveClientRect.right || pt.y < effectiveClientRect.top || pt.y >= effectiveClientRect.bottom)
					{
						num = IntPtr.Zero;
					}
				}
				catch (Win32Exception)
				{
				}
			}
		}
		if (!IsOurWindow(num))
		{
			ReportInput(_source.Value.CriticalHandle, InputMode.Foreground, _msgTime, RawMouseActions.Deactivate, 0, 0, 0);
		}
		else
		{
			_active = false;
		}
	}

	private MS.Win32.NativeMethods.RECT GetEffectiveClientRect(nint hwnd)
	{
		MS.Win32.NativeMethods.RECT rcClient = default(MS.Win32.NativeMethods.RECT);
		if (!IsOurWindowImpl(hwnd, out var hwndSource))
		{
			return rcClient;
		}
		if (HasCustomChrome(hwndSource, ref rcClient))
		{
			return rcClient;
		}
		SafeNativeMethods.GetClientRect(new HandleRef(this, hwnd), ref rcClient);
		return rcClient;
	}

	private bool HasCustomChrome(HwndSource hwndSource, ref MS.Win32.NativeMethods.RECT rcClient)
	{
		if (!EnsureFrameworkAccessors(hwndSource))
		{
			return false;
		}
		if (!(hwndSource.RootVisual?.GetValue(WindowChromeWorkerProperty) is DependencyObject obj))
		{
			return false;
		}
		object[] array = new object[1] { rcClient };
		if ((bool)GetEffectiveClientAreaMI.Invoke(obj, array))
		{
			rcClient = (MS.Win32.NativeMethods.RECT)array[0];
			return true;
		}
		return false;
	}

	private bool EnsureFrameworkAccessors(HwndSource hwndSource)
	{
		if (WindowChromeWorkerProperty != null)
		{
			return true;
		}
		Assembly presentationFrameworkFromHwndSource = GetPresentationFrameworkFromHwndSource(_source.Value);
		if (presentationFrameworkFromHwndSource == null)
		{
			presentationFrameworkFromHwndSource = GetPresentationFrameworkFromHwndSource(hwndSource);
		}
		if (presentationFrameworkFromHwndSource == null)
		{
			return false;
		}
		Type type = presentationFrameworkFromHwndSource.GetType("System.Windows.Shell.WindowChromeWorker");
		DependencyProperty dependencyProperty = (type?.GetField("WindowChromeWorkerProperty", BindingFlags.Static | BindingFlags.Public))?.GetValue(null) as DependencyProperty;
		GetEffectiveClientAreaMI = type?.GetMethod("GetEffectiveClientArea", BindingFlags.Instance | BindingFlags.NonPublic);
		if (dependencyProperty != null && GetEffectiveClientAreaMI != null)
		{
			WindowChromeWorkerProperty = dependencyProperty;
		}
		return WindowChromeWorkerProperty != null;
	}

	private Assembly GetPresentationFrameworkFromHwndSource(HwndSource hwndSource)
	{
		Type type = (hwndSource?.RootVisual)?.GetType();
		while (type != null && type.Assembly.FullName != "PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")
		{
			type = type.BaseType;
		}
		return type?.Assembly;
	}

	private void StartTracking(nint hwnd)
	{
		if (!_tracking && !_isDwmProcess)
		{
			_tme.hwndTrack = hwnd;
			_tme.dwFlags = 2;
			try
			{
				SafeNativeMethods.TrackMouseEvent(_tme);
				_tracking = true;
			}
			catch (Win32Exception)
			{
			}
		}
	}

	private void StopTracking(nint hwnd)
	{
		if (_tracking && !_isDwmProcess)
		{
			_tme.hwndTrack = hwnd;
			_tme.dwFlags = -2147483646;
			try
			{
				SafeNativeMethods.TrackMouseEvent(_tme);
				_tracking = false;
			}
			catch (Win32Exception)
			{
			}
		}
	}

	private nint MakeLPARAM(int high, int low)
	{
		return (high << 16) | (low & 0xFFFF);
	}

	private bool IsOurWindow(nint hwnd)
	{
		HwndSource hwndSource;
		return IsOurWindowImpl(hwnd, out hwndSource);
	}

	private bool IsOurWindowImpl(nint hwnd, out HwndSource hwndSource)
	{
		bool flag = false;
		hwndSource = null;
		if (hwnd != IntPtr.Zero)
		{
			hwndSource = HwndSource.CriticalFromHwnd(hwnd);
			if (hwndSource != null)
			{
				if (hwndSource.Dispatcher == _source.Value.Dispatcher)
				{
					return true;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	private bool ReportInput(nint hwnd, InputMode mode, int timestamp, RawMouseActions actions, int x, int y, int wheel)
	{
		if (_source == null || _source.Value == null)
		{
			return false;
		}
		PresentationSource presentationSource = _source.Value;
		CompositionTarget compositionTarget = presentationSource.CompositionTarget;
		if (_site == null || presentationSource.IsDisposed || compositionTarget == null)
		{
			if (!_active)
			{
				return false;
			}
			actions = RawMouseActions.Deactivate;
		}
		if ((actions & RawMouseActions.Deactivate) == RawMouseActions.Deactivate)
		{
			StopTracking(hwnd);
			_active = false;
		}
		else if ((actions & RawMouseActions.CancelCapture) != RawMouseActions.CancelCapture)
		{
			if (!_active && (actions & RawMouseActions.VerticalWheelRotate) == RawMouseActions.VerticalWheelRotate)
			{
				MouseDevice primaryMouseDevice = _site.Value.CriticalInputManager.PrimaryMouseDevice;
				if (primaryMouseDevice != null && primaryMouseDevice.CriticalActiveSource != null)
				{
					presentationSource = primaryMouseDevice.CriticalActiveSource;
				}
			}
			else
			{
				if (!_active)
				{
					nint num = SafeNativeMethods.GetCapture();
					if (hwnd != num)
					{
						MS.Win32.NativeMethods.POINT pt = default(MS.Win32.NativeMethods.POINT);
						try
						{
							MS.Win32.UnsafeNativeMethods.GetCursorPos(ref pt);
						}
						catch (Win32Exception)
						{
						}
						try
						{
							num = MS.Win32.UnsafeNativeMethods.WindowFromPoint(pt.x, pt.y);
						}
						catch (Win32Exception)
						{
						}
						if (hwnd != num)
						{
							return false;
						}
					}
					actions |= RawMouseActions.Activate;
					_active = true;
					_lastX = x;
					_lastY = y;
				}
				StartTracking(hwnd);
				if ((actions & RawMouseActions.AbsoluteMove) == 0)
				{
					if (x != _lastX || y != _lastY)
					{
						actions |= RawMouseActions.AbsoluteMove;
					}
				}
				else
				{
					_lastX = x;
					_lastY = y;
				}
				if ((actions & RawMouseActions.AbsoluteMove) != 0)
				{
					RecordMouseMove(x, y, _msgTime);
				}
				if ((actions & (RawMouseActions.Activate | RawMouseActions.AbsoluteMove)) != 0)
				{
					try
					{
						if ((SafeNativeMethods.GetWindowStyle(new HandleRef(this, _source.Value.CriticalHandle), exStyle: true) & 0x400000) == 4194304)
						{
							MS.Win32.NativeMethods.RECT rect = default(MS.Win32.NativeMethods.RECT);
							SafeNativeMethods.GetClientRect(new HandleRef(this, _source.Value.Handle), ref rect);
							x = rect.right - x;
						}
					}
					catch (Win32Exception)
					{
					}
				}
			}
		}
		nint extraInformation = IntPtr.Zero;
		try
		{
			extraInformation = MS.Win32.UnsafeNativeMethods.GetMessageExtraInfo();
		}
		catch (Win32Exception)
		{
		}
		RawMouseInputReport inputReport = new RawMouseInputReport(mode, timestamp, presentationSource, actions, x, y, wheel, extraInformation);
		return _site.Value.ReportInput(inputReport);
	}

	private void RecordMouseMove(int x, int y, int timestamp)
	{
		Point pointClient = new Point(x, y);
		pointClient = PointUtil.ClientToScreen(pointClient, _source.Value);
		_previousMovePoint = _latestMovePoint;
		_latestMovePoint.x = (int)pointClient.X & 0xFFFF;
		_latestMovePoint.y = (int)pointClient.Y & 0xFFFF;
		_latestMovePoint.time = timestamp;
	}
}
