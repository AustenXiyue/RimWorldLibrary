using System.Collections.Generic;
using System.IO;
using System.Windows.Input.StylusPointer;
using System.Windows.Input.StylusWisp;
using System.Windows.Input.Tracing;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using MS.Internal;
using MS.Internal.Interop;
using MS.Win32;

namespace System.Windows.Input;

internal abstract class StylusLogic : DispatcherObject
{
	internal enum FlickAction
	{
		GenericKey,
		Scroll,
		AppCommand,
		CustomKey,
		KeyModifier
	}

	internal enum FlickScrollDirection
	{
		Up,
		Down
	}

	protected class StylusLogicShutDownListener : ShutDownListener
	{
		public StylusLogicShutDownListener(StylusLogic target, ShutDownEvents events)
			: base(target, events)
		{
		}

		internal override void OnShutDown(object target, object sender, EventArgs e)
		{
			StylusTraceLogger.LogStatistics(((StylusLogic)target).Statistics);
			StylusTraceLogger.LogShutdown();
		}
	}

	private const int FlickCommandMask = 31;

	private const string WispKeyAssert = "HKEY_CURRENT_USER\\Software\\Microsoft\\Wisp\\";

	private const string WispRootKey = "Software\\Microsoft\\Wisp\\";

	private const string WispPenSystemEventParametersKey = "Software\\Microsoft\\Wisp\\Software\\Microsoft\\Wisp\\Pen\\SysEventParameters";

	private const string WispTouchConfigKey = "Software\\Microsoft\\Wisp\\Software\\Microsoft\\Wisp\\Touch";

	private const string WispDoubleTapDistanceValue = "DlbDist";

	private const string WispDoubleTapTimeValue = "DlbTime";

	private const string WispCancelDeltaValue = "Cancel";

	private const string WispTouchDoubleTapDistanceValue = "TouchModeN_DtapDist";

	private const string WispTouchDoubleTapTimeValue = "TouchModeN_DtapTime";

	private const string WpfPointerKeyAssert = "HKEY_CURRENT_USER\\Software\\Microsoft\\Avalon.Touch\\";

	private const string WpfPointerKey = "Software\\Microsoft\\Avalon.Touch\\";

	private const string WpfPointerValue = "EnablePointerSupport";

	private const uint PromotedMouseEventTag = 4283520768u;

	private const uint PromotedMouseEventMask = 4294967040u;

	private const byte PromotedMouseEventCursorIdMask = 127;

	protected int _stylusDoubleTapDeltaTime = 800;

	protected int _stylusDoubleTapDelta = 15;

	protected int _cancelDelta = 10;

	protected int _touchDoubleTapDeltaTime = 300;

	protected int _touchDoubleTapDelta = 45;

	protected const double DoubleTapMinFactor = 0.7;

	protected const double DoubleTapMaxFactor = 1.3;

	private static bool? _isPointerStackEnabled;

	private readonly Dictionary<DpiScale2, Matrix> _transformToDeviceMatrices = new Dictionary<DpiScale2, Matrix>();

	[ThreadStatic]
	private static SecurityCriticalDataClass<StylusLogic> _currentStylusLogic;

	internal static bool IsInstantiated => _currentStylusLogic?.Value != null;

	internal static bool IsStylusAndTouchSupportEnabled => !CoreAppContextSwitches.DisableStylusAndTouchSupport;

	internal static bool IsPointerStackEnabled
	{
		get
		{
			if (!_isPointerStackEnabled.HasValue)
			{
				_isPointerStackEnabled = IsStylusAndTouchSupportEnabled && (CoreAppContextSwitches.EnablePointerSupport || IsPointerEnabledInRegistry) && OSVersionHelper.IsOsWindows10RS2OrGreater;
			}
			return _isPointerStackEnabled.Value;
		}
	}

	internal static StylusLogic CurrentStylusLogic
	{
		get
		{
			if (_currentStylusLogic?.Value == null)
			{
				Initialize();
			}
			return _currentStylusLogic?.Value;
		}
	}

	private static bool IsPointerEnabledInRegistry
	{
		get
		{
			bool result = false;
			try
			{
				result = (int)(Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Avalon.Touch\\", RegistryKeyPermissionCheck.ReadSubTree)?.GetValue("EnablePointerSupport", 0) ?? ((object)0)) == 1;
			}
			catch (Exception ex) when (ex is IOException)
			{
			}
			return result;
		}
	}

	internal int StylusDoubleTapDelta => _stylusDoubleTapDelta;

	internal int TouchDoubleTapDelta => _touchDoubleTapDelta;

	internal int StylusDoubleTapDeltaTime => _stylusDoubleTapDeltaTime;

	internal int TouchDoubleTapDeltaTime => _touchDoubleTapDeltaTime;

	internal abstract StylusDeviceBase CurrentStylusDevice { get; }

	internal abstract TabletDeviceCollection TabletDevices { get; }

	protected StylusLogicShutDownListener ShutdownListener { get; set; }

	public StylusTraceLogger.StylusStatistics Statistics { get; protected set; } = new StylusTraceLogger.StylusStatistics();

	internal static T GetCurrentStylusLogicAs<T>() where T : StylusLogic
	{
		return CurrentStylusLogic as T;
	}

	private static void Initialize()
	{
		if (IsStylusAndTouchSupportEnabled)
		{
			if (IsPointerStackEnabled)
			{
				_currentStylusLogic = new SecurityCriticalDataClass<StylusLogic>(new PointerLogic(InputManager.UnsecureCurrent));
			}
			else
			{
				_currentStylusLogic = new SecurityCriticalDataClass<StylusLogic>(new WispLogic(InputManager.UnsecureCurrent));
			}
		}
	}

	protected void ReadSystemConfig()
	{
		RegistryKey registryKey = null;
		RegistryKey registryKey2 = null;
		try
		{
			registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Wisp\\Software\\Microsoft\\Wisp\\Pen\\SysEventParameters");
			if (registryKey != null)
			{
				object value = registryKey.GetValue("DlbDist");
				_stylusDoubleTapDelta = ((value == null) ? _stylusDoubleTapDelta : ((int)value));
				value = registryKey.GetValue("DlbTime");
				_stylusDoubleTapDeltaTime = ((value == null) ? _stylusDoubleTapDeltaTime : ((int)value));
				value = registryKey.GetValue("Cancel");
				_cancelDelta = ((value == null) ? _cancelDelta : ((int)value));
			}
			registryKey2 = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Wisp\\Software\\Microsoft\\Wisp\\Touch");
			if (registryKey2 != null)
			{
				object value = registryKey2.GetValue("TouchModeN_DtapDist");
				_touchDoubleTapDelta = ((value == null) ? _touchDoubleTapDelta : FitToCplCurve((double)_touchDoubleTapDelta * 0.7, _touchDoubleTapDelta, (double)_touchDoubleTapDelta * 1.3, (int)value));
				value = registryKey2.GetValue("TouchModeN_DtapTime");
				_touchDoubleTapDeltaTime = ((value == null) ? _touchDoubleTapDeltaTime : FitToCplCurve((double)_touchDoubleTapDeltaTime * 0.7, _touchDoubleTapDeltaTime, (double)_touchDoubleTapDeltaTime * 1.3, (int)value));
			}
		}
		finally
		{
			registryKey?.Close();
			registryKey2?.Close();
		}
	}

	internal abstract void UpdateOverProperty(StylusDeviceBase stylusDevice, IInputElement newOver);

	protected Matrix GetAndCacheTransformToDeviceMatrix(PresentationSource source)
	{
		HwndSource hwndSource = source as HwndSource;
		Matrix result = Matrix.Identity;
		if (hwndSource?.CompositionTarget != null)
		{
			if (!_transformToDeviceMatrices.ContainsKey(hwndSource.CompositionTarget.CurrentDpiScale))
			{
				_transformToDeviceMatrices[hwndSource.CompositionTarget.CurrentDpiScale] = hwndSource.CompositionTarget.TransformToDevice;
			}
			result = _transformToDeviceMatrices[hwndSource.CompositionTarget.CurrentDpiScale];
		}
		return result;
	}

	internal abstract Point DeviceUnitsFromMeasureUnits(PresentationSource source, Point measurePoint);

	internal abstract Point MeasureUnitsFromDeviceUnits(PresentationSource source, Point measurePoint);

	internal abstract void UpdateStylusCapture(StylusDeviceBase stylusDevice, IInputElement oldStylusDeviceCapture, IInputElement newStylusDeviceCapture, int timestamp);

	internal abstract void ReevaluateCapture(DependencyObject element, DependencyObject oldParent, bool isCoreParent);

	internal abstract void ReevaluateStylusOver(DependencyObject element, DependencyObject oldParent, bool isCoreParent);

	protected abstract void OnTabletRemoved(uint wisptisIndex);

	internal abstract void HandleMessage(WindowMessage msg, nint wParam, nint lParam);

	private static int FitToCplCurve(double vMin, double vMid, double vMax, int value)
	{
		if (value < 0)
		{
			return (int)vMin;
		}
		if (value > 100)
		{
			return (int)vMax;
		}
		double num = (double)value / 100.0;
		double num2 = 0.0;
		num2 = ((!(num <= 0.5)) ? (vMid + 2.0 * (num - 0.5) * (vMax - vMid)) : (vMin + 2.0 * num * (vMid - vMin)));
		return (int)num2;
	}

	internal static bool IsPromotedMouseEvent(RawMouseInputReport mouseInputReport)
	{
		return (MS.Win32.NativeMethods.IntPtrToInt32(mouseInputReport.ExtraInformation) & 0xFFFFFF00u) == 4283520768u;
	}

	internal static uint GetCursorIdFromMouseEvent(RawMouseInputReport mouseInputReport)
	{
		return (uint)(MS.Win32.NativeMethods.IntPtrToInt32(mouseInputReport.ExtraInformation) & 0x7F);
	}

	internal static void CurrentStylusLogicReevaluateStylusOver(DependencyObject element, DependencyObject oldParent, bool isCoreParent)
	{
		CurrentStylusLogic.ReevaluateStylusOver(element, oldParent, isCoreParent);
	}

	internal static void CurrentStylusLogicReevaluateCapture(DependencyObject element, DependencyObject oldParent, bool isCoreParent)
	{
		CurrentStylusLogic.ReevaluateCapture(element, oldParent, isCoreParent);
	}

	internal static RoutedEvent GetMainEventFromPreviewEvent(RoutedEvent routedEvent)
	{
		if (routedEvent == Stylus.PreviewStylusDownEvent)
		{
			return Stylus.StylusDownEvent;
		}
		if (routedEvent == Stylus.PreviewStylusUpEvent)
		{
			return Stylus.StylusUpEvent;
		}
		if (routedEvent == Stylus.PreviewStylusMoveEvent)
		{
			return Stylus.StylusMoveEvent;
		}
		if (routedEvent == Stylus.PreviewStylusInAirMoveEvent)
		{
			return Stylus.StylusInAirMoveEvent;
		}
		if (routedEvent == Stylus.PreviewStylusInRangeEvent)
		{
			return Stylus.StylusInRangeEvent;
		}
		if (routedEvent == Stylus.PreviewStylusOutOfRangeEvent)
		{
			return Stylus.StylusOutOfRangeEvent;
		}
		if (routedEvent == Stylus.PreviewStylusSystemGestureEvent)
		{
			return Stylus.StylusSystemGestureEvent;
		}
		if (routedEvent == Stylus.PreviewStylusButtonDownEvent)
		{
			return Stylus.StylusButtonDownEvent;
		}
		if (routedEvent == Stylus.PreviewStylusButtonUpEvent)
		{
			return Stylus.StylusButtonUpEvent;
		}
		return null;
	}

	internal static RoutedEvent GetPreviewEventFromRawStylusActions(RawStylusActions actions)
	{
		RoutedEvent result = null;
		switch (actions)
		{
		case RawStylusActions.Down:
			result = Stylus.PreviewStylusDownEvent;
			break;
		case RawStylusActions.Up:
			result = Stylus.PreviewStylusUpEvent;
			break;
		case RawStylusActions.Move:
			result = Stylus.PreviewStylusMoveEvent;
			break;
		case RawStylusActions.InAirMove:
			result = Stylus.PreviewStylusInAirMoveEvent;
			break;
		case RawStylusActions.InRange:
			result = Stylus.PreviewStylusInRangeEvent;
			break;
		case RawStylusActions.OutOfRange:
			result = Stylus.PreviewStylusOutOfRangeEvent;
			break;
		case RawStylusActions.SystemGesture:
			result = Stylus.PreviewStylusSystemGestureEvent;
			break;
		}
		return result;
	}

	protected bool ValidateUIElementForCapture(UIElement element)
	{
		if (element.IsEnabled && element.IsVisible)
		{
			return element.IsHitTestVisible;
		}
		return false;
	}

	protected bool ValidateContentElementForCapture(ContentElement element)
	{
		return element.IsEnabled;
	}

	protected bool ValidateUIElement3DForCapture(UIElement3D element)
	{
		if (element.IsEnabled && element.IsVisible)
		{
			return element.IsHitTestVisible;
		}
		return false;
	}

	protected bool ValidateVisualForCapture(DependencyObject visual, StylusDeviceBase currentStylusDevice)
	{
		if (visual == null)
		{
			return false;
		}
		PresentationSource presentationSource = PresentationSource.CriticalFromVisual(visual);
		if (presentationSource == null)
		{
			return false;
		}
		if (currentStylusDevice != null && currentStylusDevice.CriticalActiveSource != presentationSource && currentStylusDevice.Captured == null)
		{
			return false;
		}
		return true;
	}

	internal static FlickAction GetFlickAction(int flickData)
	{
		return (FlickAction)(flickData & 0x1F);
	}

	protected static bool GetIsScrollUp(int flickData)
	{
		return MS.Win32.NativeMethods.SignedHIWORD(flickData) == 0;
	}

	internal bool HandleFlick(int flickData, IInputElement element)
	{
		bool result = false;
		switch (GetFlickAction(flickData))
		{
		case FlickAction.Scroll:
		{
			RoutedUICommand routedUICommand = (GetIsScrollUp(flickData) ? ComponentCommands.ScrollPageUp : ComponentCommands.ScrollPageDown);
			if (element != null)
			{
				if (routedUICommand.CanExecute(null, element))
				{
					Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.FlickScrollingUsed;
					routedUICommand.Execute(null, element);
				}
				result = true;
			}
			break;
		}
		}
		return result;
	}
}
