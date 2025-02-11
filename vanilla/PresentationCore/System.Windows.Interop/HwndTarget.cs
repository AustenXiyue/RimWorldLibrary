using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Media;
using System.Windows.Media.Composition;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Automation;
using MS.Internal.Interop;
using MS.Internal.PresentationCore;
using MS.Utility;
using MS.Win32;

namespace System.Windows.Interop;

/// <summary>Represents a binding to a window handle that supports visual composition. </summary>
public class HwndTarget : CompositionTarget
{
	private class MonitorPowerEventArgs : EventArgs
	{
		public bool PowerOn { get; private set; }

		public MonitorPowerEventArgs(bool powerOn)
		{
			PowerOn = powerOn;
		}
	}

	private class NotificationWindowHelper : IDisposable
	{
		private HwndWrapper _notificationHwnd;

		private HwndWrapperHook _notificationHook;

		private int _hwndTargetCount;

		private bool _monitorOn = true;

		private nint _hPowerNotify;

		public event EventHandler<MonitorPowerEventArgs> MonitorPowerEvent;

		public unsafe NotificationWindowHelper()
		{
			if (Utilities.IsOSVistaOrNewer)
			{
				_notificationHook = NotificationFilterMessage;
				HwndWrapperHook[] hooks = new HwndWrapperHook[1] { _notificationHook };
				_notificationHwnd = new HwndWrapper(0, 0, 0, 0, 0, 0, 0, "", IntPtr.Zero, hooks);
				Guid guid = new Guid(MS.Win32.NativeMethods.GUID_MONITOR_POWER_ON.ToByteArray());
				_hPowerNotify = MS.Win32.UnsafeNativeMethods.RegisterPowerSettingNotification(_notificationHwnd.Handle, &guid, 0);
			}
		}

		public void Dispose()
		{
			if (_hPowerNotify != IntPtr.Zero)
			{
				MS.Win32.UnsafeNativeMethods.UnregisterPowerSettingNotification(_hPowerNotify);
				_hPowerNotify = IntPtr.Zero;
			}
			this.MonitorPowerEvent = null;
			_hwndTargetCount = 0;
			if (_notificationHwnd != null)
			{
				_notificationHwnd.Dispose();
				_notificationHwnd = null;
			}
		}

		public void AttachHwndTarget(HwndTarget hwndTarget)
		{
			MonitorPowerEvent += hwndTarget.OnMonitorPowerEvent;
			if (_hwndTargetCount > 0)
			{
				hwndTarget.OnMonitorPowerEvent(null, _monitorOn, paintOnWake: false);
			}
			_hwndTargetCount++;
		}

		public bool DetachHwndTarget(HwndTarget hwndTarget)
		{
			MonitorPowerEvent -= hwndTarget.OnMonitorPowerEvent;
			_hwndTargetCount--;
			return _hwndTargetCount == 0;
		}

		private unsafe nint NotificationFilterMessage(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
		{
			nint zero = IntPtr.Zero;
			if (msg == 536)
			{
				if (MS.Win32.NativeMethods.IntPtrToInt32(wParam) == 32787)
				{
					if (((MS.Win32.NativeMethods.POWERBROADCAST_SETTING*)lParam)->PowerSetting == MS.Win32.NativeMethods.GUID_MONITOR_POWER_ON)
					{
						if (((MS.Win32.NativeMethods.POWERBROADCAST_SETTING*)lParam)->Data == 0)
						{
							_monitorOn = false;
						}
						else
						{
							_monitorOn = true;
						}
						if (this.MonitorPowerEvent != null)
						{
							this.MonitorPowerEvent(null, new MonitorPowerEventArgs(_monitorOn));
							return zero;
						}
					}
				}
			}
			else
			{
				handled = false;
			}
			return zero;
		}
	}

	private static readonly object s_lockObject;

	private static WindowMessage s_updateWindowSettings;

	private static WindowMessage s_needsRePresentOnWake;

	private static WindowMessage s_DisplayDevicesAvailabilityChanged;

	private static readonly nint Handled;

	private static readonly nint Unhandled;

	private MatrixTransform _worldTransform;

	private MS.Internal.SecurityCriticalDataForSet<RenderMode> _renderModePreference = new MS.Internal.SecurityCriticalDataForSet<RenderMode>(RenderMode.Default);

	private MS.Win32.NativeMethods.HWND _hWnd;

	private MS.Win32.NativeMethods.RECT _hwndClientRectInScreenCoords;

	private MS.Win32.NativeMethods.RECT _hwndWindowRectInScreenCoords;

	private Color _backgroundColor = Color.FromRgb(0, 0, 0);

	private DUCE.MultiChannelResource _compositionTarget;

	private bool _isRenderTargetEnabled = true;

	private bool _usesPerPixelOpacity;

	private int _disableCookie;

	private bool _isMinimized;

	private bool _isSessionDisconnected;

	private bool _isSuspended;

	private bool _userInputResize;

	private bool _needsRePresentOnWake;

	private bool _hasRePresentedSinceWake;

	private bool _displayDevicesAvailable = MediaContext.ShouldRenderEvenWhenNoDisplayDevicesAreAvailable;

	private bool _wasWmPaintProcessingDeferred;

	private int? _sessionId;

	private DateTime _lastWakeOrUnlockEvent;

	private const double _allowedPresentFailureDelay = 10.0;

	private DispatcherTimer _restoreDT;

	private bool _windowPosChanging;

	[ThreadStatic]
	private static NotificationWindowHelper _notificationWindowHelper;

	/// <summary>Gets or sets the rendering mode for the window referenced by this <see cref="T:System.Windows.Interop.HwndTarget" />.</summary>
	/// <returns>One of the <see cref="T:System.Windows.Interop.RenderMode" /> values that specifies the current render mode. The default is <see cref="F:System.Windows.Interop.RenderMode.Default" />.</returns>
	public RenderMode RenderMode
	{
		get
		{
			return _renderModePreference.Value;
		}
		set
		{
			if (value != 0 && value != RenderMode.SoftwareOnly)
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(RenderMode));
			}
			_renderModePreference.Value = value;
			InvalidateRenderMode();
		}
	}

	private static MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS? ProcessDpiAwareness { get; set; }

	private static MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS? AppManifestProcessDpiAwareness { get; set; }

	private DpiAwarenessContextValue DpiAwarenessContext { get; set; }

	internal DpiScale2 CurrentDpiScale { get; private set; }

	internal static bool IsPerMonitorDpiScalingSupportedOnCurrentPlatform => OSVersionHelper.IsOsWindows10RS1OrGreater;

	internal static bool IsPerMonitorDpiScalingEnabled
	{
		get
		{
			if (!CoreAppContextSwitches.DoNotScaleForDpiChanges)
			{
				return IsPerMonitorDpiScalingSupportedOnCurrentPlatform;
			}
			return false;
		}
	}

	internal static bool? IsProcessPerMonitorDpiAware
	{
		get
		{
			if (ProcessDpiAwareness.HasValue)
			{
				return ProcessDpiAwareness.Value == MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE;
			}
			return null;
		}
	}

	internal static bool? IsProcessSystemAware
	{
		get
		{
			if (ProcessDpiAwareness.HasValue)
			{
				return ProcessDpiAwareness.Value == MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE;
			}
			return null;
		}
	}

	internal static bool? IsProcessUnaware
	{
		get
		{
			if (ProcessDpiAwareness.HasValue)
			{
				return ProcessDpiAwareness.Value == MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE;
			}
			return null;
		}
	}

	internal bool IsWindowPerMonitorDpiAware
	{
		get
		{
			if (DpiAwarenessContext != DpiAwarenessContextValue.PerMonitorAware)
			{
				return DpiAwarenessContext == DpiAwarenessContextValue.PerMonitorAwareVersion2;
			}
			return true;
		}
	}

	/// <summary>Gets or sets the root visual object of the page that is hosted by the window. </summary>
	/// <returns>The root visual object of the hosted page.</returns>
	public override Visual RootVisual
	{
		set
		{
			base.RootVisual = value;
			if (value != null)
			{
				if (IsProcessPerMonitorDpiAware == true)
				{
					DpiFlags dpiFlags = DpiUtil.UpdateDpiScalesAndGetIndex(CurrentDpiScale.PixelsPerInchX, CurrentDpiScale.PixelsPerInchY);
					DpiScale newDpiScale = new DpiScale(UIElement.DpiScaleXValues[dpiFlags.Index], UIElement.DpiScaleYValues[dpiFlags.Index]);
					RootVisual.RecursiveSetDpiScaleVisualFlags(new DpiRecursiveChangeArgs(dpiFlags, RootVisual.GetDpi(), newDpiScale));
				}
				MS.Win32.UnsafeNativeMethods.NotifyWinEvent(1879048191, _hWnd.MakeHandleRef(this), 0, 0);
			}
		}
	}

	/// <summary>Gets a matrix that transforms the coordinates of this target to the device that is associated with the rendering destination. </summary>
	/// <returns>The transform matrix.</returns>
	public override Matrix TransformToDevice
	{
		get
		{
			VerifyAPIReadOnly();
			Matrix identity = Matrix.Identity;
			identity.Scale(CurrentDpiScale.DpiScaleX, CurrentDpiScale.DpiScaleY);
			return identity;
		}
	}

	/// <summary>Gets a matrix that transforms the coordinates of the device that is associated with the rendering destination of this target. </summary>
	/// <returns>The transform matrix.</returns>
	public override Matrix TransformFromDevice
	{
		get
		{
			VerifyAPIReadOnly();
			Matrix identity = Matrix.Identity;
			identity.Scale(1.0 / CurrentDpiScale.DpiScaleX, 1.0 / CurrentDpiScale.DpiScaleY);
			return identity;
		}
	}

	/// <summary>Gets or sets the background color of the window referenced by this <see cref="T:System.Windows.Interop.HwndTarget" />. </summary>
	/// <returns>The background color, as a <see cref="T:System.Windows.Media.Color" /> value.</returns>
	public Color BackgroundColor
	{
		get
		{
			VerifyAPIReadOnly();
			return _backgroundColor;
		}
		set
		{
			VerifyAPIReadWrite();
			if (_backgroundColor != value)
			{
				_backgroundColor = value;
				MediaContext mediaContext = MediaContext.From(base.Dispatcher);
				DUCE.Channel channel = mediaContext.GetChannels().Channel;
				if (channel != null)
				{
					DUCE.CompositionTarget.SetClearColor(_compositionTarget.GetHandle(channel), _backgroundColor, channel);
					mediaContext.PostRender();
				}
			}
		}
	}

	/// <summary>Gets a value that declares whether the per-pixel opacity value of the source window content is used for rendering.</summary>
	/// <returns>true if using per-pixel opacity; otherwise, false.</returns>
	public bool UsesPerPixelOpacity
	{
		get
		{
			VerifyAPIReadOnly();
			return _usesPerPixelOpacity;
		}
		internal set
		{
			VerifyAPIReadWrite();
			if (_usesPerPixelOpacity != value)
			{
				_usesPerPixelOpacity = value;
				UpdateWindowSettings();
			}
		}
	}

	static HwndTarget()
	{
		s_lockObject = new object();
		Handled = new IntPtr(1);
		Unhandled = IntPtr.Zero;
		ProcessDpiAwareness = null;
		AppManifestProcessDpiAwareness = null;
		s_updateWindowSettings = MS.Win32.UnsafeNativeMethods.RegisterWindowMessage("UpdateWindowSettings");
		s_needsRePresentOnWake = MS.Win32.UnsafeNativeMethods.RegisterWindowMessage("NeedsRePresentOnWake");
		s_DisplayDevicesAvailabilityChanged = MS.Win32.UnsafeNativeMethods.RegisterWindowMessage("DisplayDevicesAvailabilityChanged");
	}

	public HwndTarget(nint hwnd)
	{
		bool flag = true;
		_sessionId = SafeNativeMethods.GetCurrentSessionId();
		_isSessionDisconnected = !SafeNativeMethods.IsCurrentSessionConnectStateWTSActive(_sessionId);
		if (_isSessionDisconnected)
		{
			_needsRePresentOnWake = true;
		}
		AttachToHwnd(hwnd);
		try
		{
			if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info))
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientCreateVisual, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, base.Dispatcher.GetHashCode(), ((IntPtr)hwnd).ToInt64());
			}
			_hWnd = MS.Win32.NativeMethods.HWND.Cast(hwnd);
			UpdateWindowAndClientCoordinates();
			_lastWakeOrUnlockEvent = DateTime.MinValue;
			InitializeDpiAwarenessAndDpiScales();
			_worldTransform = new MatrixTransform(new Matrix(CurrentDpiScale.DpiScaleX, 0.0, 0.0, CurrentDpiScale.DpiScaleY, 0.0, 0.0));
			MediaContext.RegisterICompositionTarget(base.Dispatcher, this);
			_restoreDT = new DispatcherTimer();
			_restoreDT.Tick += InvalidateSelf;
			_restoreDT.Interval = TimeSpan.FromMilliseconds(100.0);
			flag = false;
		}
		finally
		{
			if (flag)
			{
				VisualTarget_DetachFromHwnd(hwnd);
			}
		}
	}

	private void InitializeDpiAwarenessAndDpiScales()
	{
		lock (s_lockObject)
		{
			if (!AppManifestProcessDpiAwareness.HasValue)
			{
				GetProcessDpiAwareness((nint)_hWnd, out var appManifestProcessDpiAwareness, out var processDpiAwareness);
				AppManifestProcessDpiAwareness = appManifestProcessDpiAwareness;
				ProcessDpiAwareness = processDpiAwareness;
				DpiUtil.UpdateUIElementCacheForSystemDpi(DpiUtil.GetSystemDpi());
			}
		}
		DpiAwarenessContext = (DpiAwarenessContextValue)DpiUtil.GetDpiAwarenessContext((nint)_hWnd);
		CurrentDpiScale = GetDpiScaleForWindow((nint)_hWnd);
	}

	private static void GetProcessDpiAwareness(nint hWnd, out MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS appManifestProcessDpiAwareness, out MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS processDpiAwareness)
	{
		appManifestProcessDpiAwareness = DpiUtil.GetProcessDpiAwareness(hWnd);
		if (IsPerMonitorDpiScalingEnabled)
		{
			processDpiAwareness = appManifestProcessDpiAwareness;
		}
		else
		{
			processDpiAwareness = DpiUtil.GetLegacyProcessDpiAwareness();
		}
	}

	private static DpiScale2 GetDpiScaleForWindow(nint hWnd)
	{
		DpiScale2 dpiScale = null;
		if (IsPerMonitorDpiScalingEnabled)
		{
			dpiScale = DpiUtil.GetWindowDpi(hWnd, fallbackToNearestMonitorHeuristic: false);
		}
		else if (ProcessDpiAwareness.HasValue)
		{
			if (IsProcessSystemAware == true)
			{
				dpiScale = DpiUtil.GetSystemDpiFromUIElementCache();
			}
			else if (IsProcessUnaware == true)
			{
				dpiScale = DpiScale2.FromPixelsPerInch(96.0, 96.0);
			}
		}
		if (dpiScale == null)
		{
			dpiScale = DpiUtil.GetLegacyProcessDpiAwareness() switch
			{
				MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE => DpiUtil.GetSystemDpi(), 
				MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE => IsPerMonitorDpiScalingEnabled ? DpiUtil.GetWindowDpi(hWnd, fallbackToNearestMonitorHeuristic: false) : DpiUtil.GetSystemDpi(), 
				_ => DpiScale2.FromPixelsPerInch(96.0, 96.0), 
			};
		}
		return dpiScale;
	}

	private static HandleRef NormalizeWindow(HandleRef hWnd, bool normalizeChildWindows, bool normalizePopups)
	{
		HandleRef handleRef = hWnd;
		object wrapper = hWnd.Wrapper;
		int num = (normalizeChildWindows ? 1073741824 : 0) | (normalizePopups ? int.MinValue : 0);
		if ((MS.Win32.NativeMethods.IntPtrToInt32(SafeNativeMethods.GetWindowStyle(hWnd, exStyle: false)) & num) != 0)
		{
			nint zero = IntPtr.Zero;
			do
			{
				try
				{
					zero = MS.Win32.UnsafeNativeMethods.GetParent(handleRef);
				}
				catch (Win32Exception)
				{
					zero = MS.Win32.UnsafeNativeMethods.GetWindow(handleRef, 4);
				}
				if (zero != IntPtr.Zero)
				{
					handleRef = new HandleRef(wrapper, zero);
				}
			}
			while (zero != IntPtr.Zero);
		}
		return handleRef;
	}

	private void AttachToHwnd(nint hwnd)
	{
		int lpdwProcessId = 0;
		int windowThreadProcessId = MS.Win32.UnsafeNativeMethods.GetWindowThreadProcessId(new HandleRef(this, hwnd), out lpdwProcessId);
		if (!MS.Win32.UnsafeNativeMethods.IsWindow(new HandleRef(this, hwnd)))
		{
			throw new ArgumentException(SR.HwndTarget_InvalidWindowHandle, "hwnd");
		}
		if (lpdwProcessId != SafeNativeMethods.GetCurrentProcessId())
		{
			throw new ArgumentException(SR.HwndTarget_InvalidWindowProcess, "hwnd");
		}
		if (windowThreadProcessId != SafeNativeMethods.GetCurrentThreadId())
		{
			throw new ArgumentException(SR.HwndTarget_InvalidWindowThread, "hwnd");
		}
		int num = VisualTarget_AttachToHwnd(hwnd);
		if (MS.Internal.HRESULT.Failed(num))
		{
			if (num == -2147024891)
			{
				throw new InvalidOperationException(SR.HwndTarget_WindowAlreadyHasContent);
			}
			MS.Internal.HRESULT.Check(num);
		}
		EnsureNotificationWindow();
		_notificationWindowHelper.AttachHwndTarget(this);
		MS.Win32.UnsafeNativeMethods.WTSRegisterSessionNotification(hwnd, 0u);
	}

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MilVisualTarget_AttachToHwnd")]
	internal static extern int VisualTarget_AttachToHwnd(nint hwnd);

	[DllImport("wpfgfx_cor3.dll", EntryPoint = "MilVisualTarget_DetachFromHwnd")]
	internal static extern int VisualTarget_DetachFromHwnd(nint hwnd);

	internal void InvalidateRenderMode()
	{
		RenderingMode renderingMode = ((RenderMode == RenderMode.SoftwareOnly) ? RenderingMode.Software : RenderingMode.Default);
		if (MediaSystem.ForceSoftwareRendering)
		{
			if (renderingMode == RenderingMode.Hardware || renderingMode == RenderingMode.HardwareReference)
			{
				throw new InvalidOperationException(SR.HwndTarget_HardwareNotSupportDueToProtocolMismatch);
			}
			renderingMode = RenderingMode.Software;
		}
		bool? enableMultiMonitorDisplayClipping = CoreCompatibilityPreferences.EnableMultiMonitorDisplayClipping;
		if (enableMultiMonitorDisplayClipping.HasValue)
		{
			renderingMode |= RenderingMode.IsDisableMultimonDisplayClippingValid;
			if (!enableMultiMonitorDisplayClipping.Value)
			{
				renderingMode |= RenderingMode.DisableMultimonDisplayClipping;
			}
		}
		if (MediaSystem.DisableDirtyRectangles)
		{
			renderingMode |= RenderingMode.DisableDirtyRectangles;
		}
		DUCE.Channel channel = MediaContext.From(base.Dispatcher).GetChannels().Channel;
		DUCE.CompositionTarget.SetRenderingMode(_compositionTarget.GetHandle(channel), (MILRTInitializationFlags)renderingMode, channel);
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Windows.Interop.HwndTarget" />.</summary>
	public override void Dispose()
	{
		VerifyAccess();
		try
		{
			if (!base.IsDisposed)
			{
				RootVisual = null;
				MS.Internal.HRESULT.Check(VisualTarget_DetachFromHwnd((nint)_hWnd));
				MediaContext.UnregisterICompositionTarget(base.Dispatcher, this);
				if (_notificationWindowHelper != null && _notificationWindowHelper.DetachHwndTarget(this))
				{
					_notificationWindowHelper.Dispose();
					_notificationWindowHelper = null;
				}
				MS.Win32.UnsafeNativeMethods.WTSUnRegisterSessionNotification((nint)_hWnd);
			}
		}
		finally
		{
			base.Dispose();
			GC.SuppressFinalize(this);
		}
	}

	internal override void CreateUCEResources(DUCE.Channel channel, DUCE.Channel outOfBandChannel)
	{
		base.CreateUCEResources(channel, outOfBandChannel);
		_compositionTarget.CreateOrAddRefOnChannel(this, outOfBandChannel, DUCE.ResourceType.TYPE_HWNDRENDERTARGET);
		_compositionTarget.DuplicateHandle(outOfBandChannel, channel);
		outOfBandChannel.CloseBatch();
		outOfBandChannel.Commit();
		DUCE.CompositionTarget.HwndInitialize(_compositionTarget.GetHandle(channel), (nint)_hWnd, _hwndClientRectInScreenCoords.right - _hwndClientRectInScreenCoords.left, _hwndClientRectInScreenCoords.bottom - _hwndClientRectInScreenCoords.top, MediaSystem.ForceSoftwareRendering, (int)DpiAwarenessContext, CurrentDpiScale, channel);
		DUCE.ResourceHandle hTransform = ((DUCE.IResource)_worldTransform).AddRefOnChannel(channel);
		DUCE.CompositionNode.SetTransform(_contentRoot.GetHandle(channel), hTransform, channel);
		DUCE.CompositionTarget.SetClearColor(_compositionTarget.GetHandle(channel), _backgroundColor, channel);
		Rect rect = new Rect(0.0, 0.0, (float)Math.Ceiling((double)(_hwndClientRectInScreenCoords.right - _hwndClientRectInScreenCoords.left)), (float)Math.Ceiling((double)(_hwndClientRectInScreenCoords.bottom - _hwndClientRectInScreenCoords.top)));
		StateChangedCallback(new object[3]
		{
			(HostStateFlags)3u,
			_worldTransform.Matrix,
			rect
		});
		DUCE.CompositionTarget.SetRoot(_compositionTarget.GetHandle(channel), _contentRoot.GetHandle(channel), channel);
		_disableCookie = 0;
		DUCE.ChannelSet value = default(DUCE.ChannelSet);
		value.Channel = channel;
		value.OutOfBandChannel = outOfBandChannel;
		UpdateWindowSettings(_isRenderTargetEnabled, value);
	}

	internal override void ReleaseUCEResources(DUCE.Channel channel, DUCE.Channel outOfBandChannel)
	{
		if (_compositionTarget.IsOnChannel(channel))
		{
			DUCE.CompositionTarget.SetRoot(_compositionTarget.GetHandle(channel), DUCE.ResourceHandle.Null, channel);
			_compositionTarget.ReleaseOnChannel(channel);
		}
		if (_compositionTarget.IsOnChannel(outOfBandChannel))
		{
			_compositionTarget.ReleaseOnChannel(outOfBandChannel);
		}
		if (!((DUCE.IResource)_worldTransform).GetHandle(channel).IsNull)
		{
			((DUCE.IResource)_worldTransform).ReleaseOnChannel(channel);
		}
		base.ReleaseUCEResources(channel, outOfBandChannel);
	}

	private bool HandleDpiChangedMessage(nint wParam, nint lParam)
	{
		bool result = false;
		if (IsPerMonitorDpiScalingEnabled)
		{
			HwndSource hwndSource = HwndSource.FromHwnd((nint)_hWnd);
			if (hwndSource != null)
			{
				DpiScale2 currentDpiScale = CurrentDpiScale;
				DpiScale2 dpiScale = DpiScale2.FromPixelsPerInch(MS.Win32.NativeMethods.SignedLOWORD(wParam), MS.Win32.NativeMethods.SignedHIWORD(wParam));
				if (currentDpiScale != dpiScale)
				{
					MS.Win32.NativeMethods.RECT rECT = Marshal.PtrToStructure<MS.Win32.NativeMethods.RECT>(lParam);
					hwndSource.ChangeDpi(new HwndDpiChangedEventArgs(suggestedRect: new Rect(rECT.left, rECT.top, rECT.Width, rECT.Height), oldDpi: currentDpiScale, newDpi: dpiScale));
					result = true;
				}
			}
		}
		return result;
	}

	private bool HandleDpiChangedAfterParentMessage()
	{
		bool result = false;
		if (IsPerMonitorDpiScalingEnabled)
		{
			DpiScale2 currentDpiScale = CurrentDpiScale;
			DpiScale2 dpiScaleForWindow = GetDpiScaleForWindow((nint)_hWnd);
			if (currentDpiScale != dpiScaleForWindow)
			{
				HwndSource hwndSource = HwndSource.FromHwnd((nint)_hWnd);
				if (hwndSource != null)
				{
					MS.Win32.NativeMethods.RECT clientRect = SafeNativeMethods.GetClientRect(_hWnd.MakeHandleRef(this));
					hwndSource.ChangeDpi(new HwndDpiChangedAfterParentEventArgs(suggestedRect: new Rect(clientRect.left, clientRect.top, clientRect.right - clientRect.left, clientRect.bottom - clientRect.top), oldDpi: currentDpiScale, newDpi: dpiScaleForWindow));
					result = true;
				}
			}
		}
		return result;
	}

	internal unsafe nint HandleMessage(WindowMessage msg, nint wparam, nint lparam)
	{
		nint result = Unhandled;
		if (msg == s_DisplayDevicesAvailabilityChanged)
		{
			_displayDevicesAvailable = ((IntPtr)wparam).ToInt32() != 0;
			if (_displayDevicesAvailable && _wasWmPaintProcessingDeferred)
			{
				MS.Win32.UnsafeNativeMethods.InvalidateRect(_hWnd.MakeHandleRef(this), IntPtr.Zero, erase: true);
				DoPaint();
			}
		}
		else if (msg == s_updateWindowSettings)
		{
			if (SafeNativeMethods.IsWindowVisible(_hWnd.MakeHandleRef(this)))
			{
				UpdateWindowSettings(enableRenderTarget: true);
			}
		}
		else if (msg == s_needsRePresentOnWake)
		{
			bool flag = (DateTime.Now - _lastWakeOrUnlockEvent).TotalSeconds < 10.0;
			bool flag2 = _displayDevicesAvailable || MediaContext.ShouldRenderEvenWhenNoDisplayDevicesAreAvailable;
			if (_isSessionDisconnected || _isSuspended || (_hasRePresentedSinceWake && !flag) || !flag2)
			{
				_needsRePresentOnWake = true;
			}
			else if (!_hasRePresentedSinceWake || flag)
			{
				MS.Win32.UnsafeNativeMethods.InvalidateRect(_hWnd.MakeHandleRef(this), IntPtr.Zero, erase: true);
				DoPaint();
				_hasRePresentedSinceWake = true;
			}
			return Handled;
		}
		if (base.IsDisposed)
		{
			return result;
		}
		switch (msg)
		{
		case WindowMessage.WM_DPICHANGED:
			result = (HandleDpiChangedMessage(wparam, lparam) ? Handled : Unhandled);
			break;
		case WindowMessage.WM_DPICHANGED_AFTERPARENT:
			result = (HandleDpiChangedAfterParentMessage() ? Handled : Unhandled);
			break;
		case WindowMessage.WM_NCCREATE:
			if (IsProcessPerMonitorDpiAware == true)
			{
				MS.Win32.UnsafeNativeMethods.EnableNonClientDpiScaling(NormalizeWindow(new HandleRef(this, (nint)_hWnd), normalizeChildWindows: false, normalizePopups: true));
			}
			break;
		case WindowMessage.WM_ERASEBKGND:
			result = Handled;
			break;
		case WindowMessage.WM_PAINT:
			if (_displayDevicesAvailable || MediaContext.ShouldRenderEvenWhenNoDisplayDevicesAreAvailable)
			{
				_wasWmPaintProcessingDeferred = false;
				DoPaint();
				result = Handled;
			}
			else
			{
				_wasWmPaintProcessingDeferred = true;
			}
			break;
		case WindowMessage.WM_SIZE:
			if (MS.Win32.NativeMethods.IntPtrToInt32(wparam) != 1)
			{
				if (_isMinimized)
				{
					_restoreDT.Start();
				}
				_isMinimized = false;
				DoPaint();
				OnResize();
			}
			else
			{
				_isMinimized = true;
			}
			break;
		case WindowMessage.WM_WININICHANGE:
			if (OnSettingChange(MS.Win32.NativeMethods.IntPtrToInt32(wparam)))
			{
				MS.Win32.UnsafeNativeMethods.InvalidateRect(_hWnd.MakeHandleRef(this), IntPtr.Zero, erase: true);
			}
			break;
		case WindowMessage.WM_GETOBJECT:
			result = CriticalHandleWMGetobject(wparam, lparam, RootVisual, (nint)_hWnd);
			break;
		case WindowMessage.WM_WINDOWPOSCHANGING:
			OnWindowPosChanging(lparam);
			break;
		case WindowMessage.WM_WINDOWPOSCHANGED:
			OnWindowPosChanged(lparam);
			break;
		case WindowMessage.WM_SHOWWINDOW:
		{
			bool flag6 = wparam != IntPtr.Zero;
			OnShowWindow(flag6);
			if (flag6)
			{
				DoPaint();
			}
			break;
		}
		case WindowMessage.WM_ENTERSIZEMOVE:
			OnEnterSizeMove();
			break;
		case WindowMessage.WM_EXITSIZEMOVE:
			OnExitSizeMove();
			break;
		case WindowMessage.WM_STYLECHANGING:
		{
			MS.Win32.NativeMethods.STYLESTRUCT* ptr2 = (MS.Win32.NativeMethods.STYLESTRUCT*)lparam;
			if ((int)wparam == -20)
			{
				if (UsesPerPixelOpacity)
				{
					ptr2->styleNew |= 524288;
				}
				else
				{
					ptr2->styleNew &= -524289;
				}
			}
			break;
		}
		case WindowMessage.WM_STYLECHANGED:
		{
			bool flag3 = false;
			MS.Win32.NativeMethods.STYLESTRUCT* ptr = (MS.Win32.NativeMethods.STYLESTRUCT*)lparam;
			if ((int)wparam == -16)
			{
				bool num = (ptr->styleOld & 0x40000000) == 1073741824;
				bool flag4 = (ptr->styleNew & 0x40000000) == 1073741824;
				flag3 = num != flag4;
			}
			else
			{
				bool num2 = (ptr->styleOld & 0x400000) == 4194304;
				bool flag5 = (ptr->styleNew & 0x400000) == 4194304;
				flag3 = num2 != flag5;
			}
			if (flag3)
			{
				UpdateWindowSettings();
			}
			break;
		}
		case WindowMessage.WM_WTSSESSION_CHANGE:
			if (_sessionId.HasValue && _sessionId.Value != ((IntPtr)lparam).ToInt32())
			{
				break;
			}
			switch (MS.Win32.NativeMethods.IntPtrToInt32(wparam))
			{
			case 2:
			case 4:
			case 7:
				_hasRePresentedSinceWake = false;
				_isSessionDisconnected = true;
				_lastWakeOrUnlockEvent = DateTime.MinValue;
				break;
			case 1:
			case 3:
			case 8:
				_isSessionDisconnected = false;
				if (_needsRePresentOnWake || _wasWmPaintProcessingDeferred)
				{
					MS.Win32.UnsafeNativeMethods.InvalidateRect(_hWnd.MakeHandleRef(this), IntPtr.Zero, erase: true);
					_needsRePresentOnWake = false;
				}
				DoPaint();
				_lastWakeOrUnlockEvent = DateTime.Now;
				break;
			}
			break;
		case WindowMessage.WM_POWERBROADCAST:
			switch (MS.Win32.NativeMethods.IntPtrToInt32(wparam))
			{
			case 4:
				_isSuspended = true;
				_hasRePresentedSinceWake = false;
				_lastWakeOrUnlockEvent = DateTime.MinValue;
				break;
			case 6:
			case 7:
			case 18:
				_isSuspended = false;
				if (_needsRePresentOnWake)
				{
					MS.Win32.UnsafeNativeMethods.InvalidateRect(_hWnd.MakeHandleRef(this), IntPtr.Zero, erase: true);
					_needsRePresentOnWake = false;
				}
				DoPaint();
				_lastWakeOrUnlockEvent = DateTime.Now;
				break;
			}
			break;
		}
		return result;
	}

	private void OnMonitorPowerEvent(object sender, MonitorPowerEventArgs eventArgs)
	{
		OnMonitorPowerEvent(sender, eventArgs.PowerOn, paintOnWake: true);
	}

	private void OnMonitorPowerEvent(object sender, bool powerOn, bool paintOnWake)
	{
		if (powerOn)
		{
			_isSuspended = false;
			if (paintOnWake)
			{
				if (_needsRePresentOnWake)
				{
					MS.Win32.UnsafeNativeMethods.InvalidateRect(_hWnd.MakeHandleRef(this), IntPtr.Zero, erase: true);
					_needsRePresentOnWake = false;
				}
				DoPaint();
			}
			_lastWakeOrUnlockEvent = DateTime.Now;
		}
		else
		{
			_isSuspended = true;
			_hasRePresentedSinceWake = false;
			_lastWakeOrUnlockEvent = DateTime.MinValue;
		}
	}

	private void InvalidateSelf(object s, EventArgs args)
	{
		MS.Win32.UnsafeNativeMethods.InvalidateRect(_hWnd.MakeHandleRef(this), IntPtr.Zero, erase: true);
		((DispatcherTimer)s)?.Stop();
	}

	private void DoPaint()
	{
		MS.Win32.NativeMethods.PAINTSTRUCT lpPaint = default(MS.Win32.NativeMethods.PAINTSTRUCT);
		HandleRef handleRef = new HandleRef(this, (nint)_hWnd);
		MS.Win32.NativeMethods.HDC hDC = default(MS.Win32.NativeMethods.HDC);
		hDC.h = MS.Win32.UnsafeNativeMethods.BeginPaint(handleRef, ref lpPaint);
		int windowLong = MS.Win32.UnsafeNativeMethods.GetWindowLong(handleRef, -20);
		MS.Win32.NativeMethods.RECT rc = new MS.Win32.NativeMethods.RECT(lpPaint.rcPaint_left, lpPaint.rcPaint_top, lpPaint.rcPaint_right, lpPaint.rcPaint_bottom);
		if (rc.IsEmpty && (windowLong & 0x80000) != 0 && !MS.Win32.UnsafeNativeMethods.GetLayeredWindowAttributes(_hWnd.MakeHandleRef(this), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) && !_isSessionDisconnected && !_isMinimized && (!_isSuspended || MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.REMOTESESSION) != 0))
		{
			rc = new MS.Win32.NativeMethods.RECT(0, 0, _hwndClientRectInScreenCoords.right - _hwndClientRectInScreenCoords.left, _hwndClientRectInScreenCoords.bottom - _hwndClientRectInScreenCoords.top);
		}
		AdjustForRightToLeft(ref rc, handleRef);
		if (!rc.IsEmpty)
		{
			InvalidateRect(rc);
		}
		MS.Win32.UnsafeNativeMethods.EndPaint(_hWnd.MakeHandleRef(this), ref lpPaint);
	}

	internal AutomationPeer EnsureAutomationPeer(Visual root)
	{
		return EnsureAutomationPeer(root, (nint)_hWnd);
	}

	internal static AutomationPeer EnsureAutomationPeer(Visual root, nint handle)
	{
		AutomationPeer automationPeer = null;
		if (root.CheckFlagsAnd(VisualFlags.IsUIElement))
		{
			UIElement uIElement = (UIElement)root;
			automationPeer = UIElementAutomationPeer.CreatePeerForElement(uIElement);
			if (automationPeer == null)
			{
				automationPeer = uIElement.CreateGenericRootAutomationPeer();
			}
			if (automationPeer != null)
			{
				automationPeer.Hwnd = handle;
			}
		}
		if (automationPeer == null)
		{
			automationPeer = UIElementAutomationPeer.GetRootAutomationPeer(root, handle);
		}
		automationPeer?.AddToAutomationEventList();
		return automationPeer;
	}

	private static nint CriticalHandleWMGetobject(nint wparam, nint lparam, Visual root, nint handle)
	{
		try
		{
			if (root == null)
			{
				return IntPtr.Zero;
			}
			AutomationPeer automationPeer = EnsureAutomationPeer(root, handle);
			if (automationPeer == null)
			{
				return IntPtr.Zero;
			}
			IRawElementProviderSimple el = ElementProxy.StaticWrap(automationPeer, automationPeer);
			return AutomationInteropProvider.ReturnRawElementProvider(handle, wparam, lparam, el);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			return new IntPtr(Marshal.GetHRForException(ex));
		}
	}

	internal void AdjustForRightToLeft(ref MS.Win32.NativeMethods.RECT rc, HandleRef handleRef)
	{
		if ((SafeNativeMethods.GetWindowStyle(handleRef, exStyle: true) & 0x400000) == 4194304)
		{
			MS.Win32.NativeMethods.RECT rect = default(MS.Win32.NativeMethods.RECT);
			SafeNativeMethods.GetClientRect(handleRef, ref rect);
			int num = rc.right - rc.left;
			rc.right = rect.right - rc.left;
			rc.left = rc.right - num;
		}
	}

	private bool OnSettingChange(int firstParam)
	{
		if (firstParam == 75 || firstParam == 8203 || firstParam == 8205 || firstParam == 8211 || firstParam == 8213 || firstParam == 8215 || firstParam == 8217 || firstParam == 8219)
		{
			MS.Internal.HRESULT.Check(MILUpdateSystemParametersInfo.Update());
			return true;
		}
		return false;
	}

	private void InvalidateRect(MS.Win32.NativeMethods.RECT rcDirty)
	{
		DUCE.ChannelSet channels = MediaContext.From(base.Dispatcher).GetChannels();
		DUCE.Channel channel = channels.Channel;
		DUCE.Channel outOfBandChannel = channels.OutOfBandChannel;
		if (_compositionTarget.IsOnChannel(channel))
		{
			DUCE.CompositionTarget.Invalidate(_compositionTarget.GetHandle(outOfBandChannel), ref rcDirty, outOfBandChannel);
		}
	}

	private void OnResize()
	{
		if (_compositionTarget.IsOnAnyChannel)
		{
			MediaContext mediaContext = MediaContext.From(base.Dispatcher);
			UpdateWindowSettings();
			Rect rect = new Rect(0.0, 0.0, (float)Math.Ceiling((double)(_hwndClientRectInScreenCoords.right - _hwndClientRectInScreenCoords.left)), (float)Math.Ceiling((double)(_hwndClientRectInScreenCoords.bottom - _hwndClientRectInScreenCoords.top)));
			StateChangedCallback(new object[3]
			{
				HostStateFlags.ClipBounds,
				null,
				rect
			});
			mediaContext.Resize(this);
			int windowLong = MS.Win32.UnsafeNativeMethods.GetWindowLong(_hWnd.MakeHandleRef(this), -16);
			if (_userInputResize || _usesPerPixelOpacity || ((windowLong & 0x40000000) != 0 && Utilities.IsCompositionEnabled))
			{
				mediaContext.CompleteRender();
			}
		}
	}

	private void UpdateWindowAndClientCoordinates()
	{
		HandleRef hWnd = _hWnd.MakeHandleRef(this);
		SafeNativeMethods.GetWindowRect(hWnd, ref _hwndWindowRectInScreenCoords);
		MS.Win32.NativeMethods.RECT rect = default(MS.Win32.NativeMethods.RECT);
		SafeNativeMethods.GetClientRect(hWnd, ref rect);
		MS.Win32.NativeMethods.POINT pt = new MS.Win32.NativeMethods.POINT(rect.left, rect.top);
		MS.Win32.UnsafeNativeMethods.ClientToScreen(hWnd, ref pt);
		MS.Win32.NativeMethods.POINT pt2 = new MS.Win32.NativeMethods.POINT(rect.right, rect.bottom);
		MS.Win32.UnsafeNativeMethods.ClientToScreen(hWnd, ref pt2);
		if (pt2.x >= pt.x)
		{
			_hwndClientRectInScreenCoords.left = pt.x;
			_hwndClientRectInScreenCoords.right = pt2.x;
		}
		else
		{
			_hwndClientRectInScreenCoords.left = pt2.x;
			_hwndClientRectInScreenCoords.right = pt.x;
		}
		if (pt2.y >= pt.y)
		{
			_hwndClientRectInScreenCoords.top = pt.y;
			_hwndClientRectInScreenCoords.bottom = pt2.y;
		}
		else
		{
			_hwndClientRectInScreenCoords.top = pt2.y;
			_hwndClientRectInScreenCoords.bottom = pt.y;
		}
	}

	private void UpdateWorldTransform(DpiScale2 dpiScale)
	{
		_worldTransform = new MatrixTransform(new Matrix(dpiScale.DpiScaleX, 0.0, 0.0, dpiScale.DpiScaleY, 0.0, 0.0));
		DUCE.Channel channel = MediaContext.From(base.Dispatcher).GetChannels().Channel;
		DUCE.ResourceHandle hTransform = ((DUCE.IResource)_worldTransform).AddRefOnChannel(channel);
		DUCE.CompositionNode.SetTransform(_contentRoot.GetHandle(channel), hTransform, channel);
	}

	private void PropagateDpiChangeToRootVisual(DpiScale2 oldDpi, DpiScale2 newDpi)
	{
		DpiFlags dpiFlags = DpiUtil.UpdateDpiScalesAndGetIndex(newDpi.PixelsPerInchX, newDpi.PixelsPerInchY);
		if (RootVisual != null)
		{
			RecursiveUpdateDpiFlagAndInvalidateMeasure(RootVisual, new DpiRecursiveChangeArgs(dpiFlags, oldDpi, newDpi));
		}
	}

	private void NotifyListenersOfWorldTransformAndClipBoundsChanged()
	{
		Rect rect = new Rect(0.0, 0.0, _hwndClientRectInScreenCoords.right - _hwndClientRectInScreenCoords.left, _hwndClientRectInScreenCoords.bottom - _hwndClientRectInScreenCoords.top);
		StateChangedCallback(new object[3]
		{
			(HostStateFlags)3u,
			_worldTransform.Matrix,
			rect
		});
	}

	internal void OnDpiChanged(HwndDpiChangedEventArgs e)
	{
		DpiScale2 currentDpiScale = CurrentDpiScale;
		DpiScale2 dpiScale2 = (CurrentDpiScale = new DpiScale2(e.NewDpi));
		UpdateWorldTransform(dpiScale2);
		PropagateDpiChangeToRootVisual(currentDpiScale, dpiScale2);
		NotifyListenersOfWorldTransformAndClipBoundsChanged();
		NotifyRendererOfDpiChange(afterParent: false);
		MS.Win32.UnsafeNativeMethods.SetWindowPos(_hWnd.MakeHandleRef(this), new HandleRef(null, IntPtr.Zero), (int)e.SuggestedRect.Left, (int)e.SuggestedRect.Top, (int)e.SuggestedRect.Width, (int)e.SuggestedRect.Height, 16388);
	}

	internal void OnDpiChangedAfterParent(HwndDpiChangedAfterParentEventArgs e)
	{
		DpiScale2 currentDpiScale = CurrentDpiScale;
		DpiScale2 dpiScale2 = (CurrentDpiScale = new DpiScale2(e.NewDpi));
		UpdateWorldTransform(dpiScale2);
		PropagateDpiChangeToRootVisual(currentDpiScale, dpiScale2);
		NotifyListenersOfWorldTransformAndClipBoundsChanged();
		NotifyRendererOfDpiChange(afterParent: true);
		MS.Win32.UnsafeNativeMethods.SetWindowPos(_hWnd.MakeHandleRef(this), new HandleRef(null, IntPtr.Zero), (int)e.SuggestedRect.Left, (int)e.SuggestedRect.Top, (int)e.SuggestedRect.Width, (int)e.SuggestedRect.Height, 20);
		MS.Win32.UnsafeNativeMethods.InvalidateRect(new HandleRef(this, (nint)_hWnd), IntPtr.Zero, erase: true);
		DoPaint();
	}

	private void NotifyRendererOfDpiChange(bool afterParent)
	{
		DUCE.Channel channel = MediaContext.From(base.Dispatcher).GetChannels().Channel;
		DUCE.CompositionTarget.ProcessDpiChanged(_compositionTarget.GetHandle(channel), CurrentDpiScale, afterParent, channel);
	}

	private void RecursiveUpdateDpiFlagAndInvalidateMeasure(DependencyObject d, DpiRecursiveChangeArgs args)
	{
		int childrenCount = VisualTreeHelper.GetChildrenCount(d);
		for (int i = 0; i < childrenCount; i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(d, i);
			if (child != null)
			{
				RecursiveUpdateDpiFlagAndInvalidateMeasure(child, args);
			}
		}
		if (d is Visual visual)
		{
			visual.SetDpiScaleVisualFlags(args);
			(d as UIElement)?.InvalidateMeasure();
		}
	}

	private void OnWindowPosChanging(nint lParam)
	{
		_windowPosChanging = true;
		UpdateWindowPos(lParam);
	}

	private void OnWindowPosChanged(nint lParam)
	{
		_windowPosChanging = false;
		UpdateWindowPos(lParam);
	}

	private void UpdateWindowPos(nint lParam)
	{
		MS.Win32.NativeMethods.WINDOWPOS wINDOWPOS = Marshal.PtrToStructure<MS.Win32.NativeMethods.WINDOWPOS>(lParam);
		bool flag = (wINDOWPOS.flags & 2) == 0;
		bool flag2 = (wINDOWPOS.flags & 1) == 0;
		bool flag3 = flag || flag2;
		if (flag3)
		{
			if (!flag)
			{
				wINDOWPOS.x = (wINDOWPOS.y = 0);
			}
			if (!flag2)
			{
				wINDOWPOS.cx = (wINDOWPOS.cy = 0);
			}
			MS.Win32.NativeMethods.RECT rcWindowCoords = new MS.Win32.NativeMethods.RECT(wINDOWPOS.x, wINDOWPOS.y, wINDOWPOS.x + wINDOWPOS.cx, wINDOWPOS.y + wINDOWPOS.cy);
			nint parent = MS.Win32.UnsafeNativeMethods.GetParent(new HandleRef(null, wINDOWPOS.hwnd));
			if (parent != IntPtr.Zero)
			{
				SafeSecurityHelper.TransformLocalRectToScreen(new HandleRef(null, parent), ref rcWindowCoords);
			}
			if (!flag)
			{
				int num = rcWindowCoords.right - rcWindowCoords.left;
				int num2 = rcWindowCoords.bottom - rcWindowCoords.top;
				rcWindowCoords.left = _hwndWindowRectInScreenCoords.left;
				rcWindowCoords.right = rcWindowCoords.left + num;
				rcWindowCoords.top = _hwndWindowRectInScreenCoords.top;
				rcWindowCoords.bottom = rcWindowCoords.top + num2;
			}
			if (!flag2)
			{
				int num3 = _hwndWindowRectInScreenCoords.right - _hwndWindowRectInScreenCoords.left;
				int num4 = _hwndWindowRectInScreenCoords.bottom - _hwndWindowRectInScreenCoords.top;
				rcWindowCoords.right = rcWindowCoords.left + num3;
				rcWindowCoords.bottom = rcWindowCoords.top + num4;
			}
			flag3 = _hwndWindowRectInScreenCoords.left != rcWindowCoords.left || _hwndWindowRectInScreenCoords.top != rcWindowCoords.top || _hwndWindowRectInScreenCoords.right != rcWindowCoords.right || _hwndWindowRectInScreenCoords.bottom != rcWindowCoords.bottom;
		}
		bool flag4 = SafeNativeMethods.IsWindowVisible(_hWnd.MakeHandleRef(this));
		if (flag4 && _windowPosChanging && flag3)
		{
			flag4 = false;
		}
		if (flag3 || flag4 != _isRenderTargetEnabled)
		{
			UpdateWindowSettings(flag4);
		}
	}

	private void OnShowWindow(bool enableRenderTarget)
	{
		if (enableRenderTarget != _isRenderTargetEnabled)
		{
			UpdateWindowSettings(enableRenderTarget);
		}
	}

	private void OnEnterSizeMove()
	{
		_userInputResize = true;
	}

	private void OnExitSizeMove()
	{
		if (_windowPosChanging)
		{
			_windowPosChanging = false;
			UpdateWindowSettings(enableRenderTarget: true);
		}
		_userInputResize = false;
	}

	private void UpdateWindowSettings()
	{
		UpdateWindowSettings(_isRenderTargetEnabled, null);
	}

	private void UpdateWindowSettings(bool enableRenderTarget)
	{
		UpdateWindowSettings(enableRenderTarget, null);
	}

	private unsafe void UpdateWindowSettings(bool enableRenderTarget, DUCE.ChannelSet? channelSet)
	{
		MediaContext mediaContext = MediaContext.From(base.Dispatcher);
		bool flag = false;
		bool flag2 = false;
		if (_isRenderTargetEnabled != enableRenderTarget)
		{
			_isRenderTargetEnabled = enableRenderTarget;
			flag = !enableRenderTarget;
			flag2 = enableRenderTarget;
		}
		if (!_compositionTarget.IsOnAnyChannel)
		{
			return;
		}
		UpdateWindowAndClientCoordinates();
		int windowLong = MS.Win32.UnsafeNativeMethods.GetWindowLong(_hWnd.MakeHandleRef(this), -16);
		int windowLong2 = MS.Win32.UnsafeNativeMethods.GetWindowLong(_hWnd.MakeHandleRef(this), -20);
		bool flag3 = (windowLong2 & 0x80000) != 0;
		bool isChild = (windowLong & 0x40000000) != 0;
		bool isRTL = (windowLong2 & 0x400000) != 0;
		int num = _hwndClientRectInScreenCoords.right - _hwndClientRectInScreenCoords.left;
		int num2 = _hwndClientRectInScreenCoords.bottom - _hwndClientRectInScreenCoords.top;
		MILTransparencyFlags mILTransparencyFlags = MILTransparencyFlags.Opaque;
		if (_usesPerPixelOpacity)
		{
			mILTransparencyFlags |= MILTransparencyFlags.PerPixelAlpha;
		}
		if (!flag3 && mILTransparencyFlags != 0)
		{
			MS.Win32.UnsafeNativeMethods.SetWindowLong(_hWnd.MakeHandleRef(this), -20, new IntPtr(windowLong2 | 0x80000));
		}
		else if (flag3 && mILTransparencyFlags == MILTransparencyFlags.Opaque)
		{
			MS.Win32.UnsafeNativeMethods.SetWindowLong(_hWnd.MakeHandleRef(this), -20, new IntPtr(windowLong2 & -524289));
		}
		else if (flag3 && mILTransparencyFlags != 0 && _isRenderTargetEnabled && (num == 0 || num2 == 0))
		{
			MS.Win32.NativeMethods.BLENDFUNCTION pBlend = default(MS.Win32.NativeMethods.BLENDFUNCTION);
			pBlend.BlendOp = 0;
			pBlend.SourceConstantAlpha = 0;
			MS.Win32.UnsafeNativeMethods.UpdateLayeredWindow(_hWnd.h, IntPtr.Zero, null, null, IntPtr.Zero, null, 0, ref pBlend, 2);
		}
		flag3 = mILTransparencyFlags != MILTransparencyFlags.Opaque;
		if (!channelSet.HasValue)
		{
			channelSet = mediaContext.GetChannels();
		}
		DUCE.Channel outOfBandChannel = channelSet.Value.OutOfBandChannel;
		if (flag2)
		{
			outOfBandChannel.Commit();
			outOfBandChannel.SyncFlush();
		}
		if (!_isRenderTargetEnabled)
		{
			_disableCookie++;
		}
		DUCE.Channel channel = channelSet.Value.Channel;
		DUCE.CompositionTarget.UpdateWindowSettings(_isRenderTargetEnabled ? _compositionTarget.GetHandle(channel) : _compositionTarget.GetHandle(outOfBandChannel), _hwndClientRectInScreenCoords, Colors.Transparent, 1f, flag3 ? ((!_usesPerPixelOpacity) ? MILWindowLayerType.SystemManagedLayer : MILWindowLayerType.ApplicationManagedLayer) : MILWindowLayerType.NotLayered, mILTransparencyFlags, isChild, isRTL, _isRenderTargetEnabled, _disableCookie, _isRenderTargetEnabled ? channel : outOfBandChannel);
		if (_isRenderTargetEnabled)
		{
			mediaContext.PostRender();
			return;
		}
		if (flag)
		{
			outOfBandChannel.CloseBatch();
			outOfBandChannel.Commit();
			outOfBandChannel.SyncFlush();
		}
		MS.Win32.UnsafeNativeMethods.PostMessage(new HandleRef(this, (nint)_hWnd), s_updateWindowSettings, IntPtr.Zero, IntPtr.Zero);
	}

	private void EnsureNotificationWindow()
	{
		if (_notificationWindowHelper == null)
		{
			_notificationWindowHelper = new NotificationWindowHelper();
		}
	}
}
