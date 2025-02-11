using System.Collections;
using System.Collections.Generic;
using System.Windows.Input.Tracing;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Interop;
using MS.Internal.PresentationCore;

namespace System.Windows.Input.StylusPointer;

internal class PointerLogic : StylusLogic
{
	private bool _lastTapBarrelDown;

	private Point _lastTapPoint = new Point(0.0, 0.0);

	private int _lastTapTimeTicks;

	private IInputElement _stylusCapture;

	private IInputElement _stylusOver;

	private DeferredElementTreeState _stylusOverTreeState = new DeferredElementTreeState();

	private DeferredElementTreeState _stylusCaptureWithinTreeState = new DeferredElementTreeState();

	private DependencyPropertyChangedEventHandler _overIsEnabledChangedEventHandler;

	private DependencyPropertyChangedEventHandler _overIsVisibleChangedEventHandler;

	private DependencyPropertyChangedEventHandler _overIsHitTestVisibleChangedEventHandler;

	private DispatcherOperationCallback _reevaluateStylusOverDelegate;

	private DispatcherOperation _reevaluateStylusOverOperation;

	private DependencyPropertyChangedEventHandler _captureIsEnabledChangedEventHandler;

	private DependencyPropertyChangedEventHandler _captureIsVisibleChangedEventHandler;

	private DependencyPropertyChangedEventHandler _captureIsHitTestVisibleChangedEventHandler;

	private DispatcherOperationCallback _reevaluateCaptureDelegate;

	private DispatcherOperation _reevaluateCaptureOperation;

	private bool _initialDeviceRefreshDone;

	private PointerTabletDeviceCollection _pointerDevices = new PointerTabletDeviceCollection();

	private PointerStylusDevice _currentStylusDevice;

	private SecurityCriticalData<InputManager> _inputManager;

	private bool _inDragDrop;

	internal Dictionary<PresentationSource, PointerStylusPlugInManager> PlugInManagers { get; private set; } = new Dictionary<PresentationSource, PointerStylusPlugInManager>();

	internal bool InDragDrop => _inDragDrop;

	internal static bool IsEnabled { get; private set; } = true;

	internal override StylusDeviceBase CurrentStylusDevice => _currentStylusDevice;

	internal override TabletDeviceCollection TabletDevices
	{
		get
		{
			if (!_initialDeviceRefreshDone)
			{
				_pointerDevices.Refresh();
				_initialDeviceRefreshDone = true;
				StylusTraceLogger.LogStartup();
				base.ShutdownListener = new StylusLogicShutDownListener(this, ShutDownEvents.DispatcherShutdown);
			}
			return _pointerDevices;
		}
	}

	internal PointerLogic(InputManager inputManager)
	{
		base.Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.PointerStackEnabled;
		_inputManager = new SecurityCriticalData<InputManager>(inputManager);
		_inputManager.Value.PreProcessInput += PreProcessInput;
		_inputManager.Value.PreNotifyInput += PreNotifyInput;
		_inputManager.Value.PostProcessInput += PostProcessInput;
		_overIsEnabledChangedEventHandler = OnOverIsEnabledChanged;
		_overIsVisibleChangedEventHandler = OnOverIsVisibleChanged;
		_overIsHitTestVisibleChangedEventHandler = OnOverIsHitTestVisibleChanged;
		_reevaluateStylusOverDelegate = ReevaluateStylusOverAsync;
		_reevaluateStylusOverOperation = null;
		_captureIsEnabledChangedEventHandler = OnCaptureIsEnabledChanged;
		_captureIsVisibleChangedEventHandler = OnCaptureIsVisibleChanged;
		_captureIsHitTestVisibleChangedEventHandler = OnCaptureIsHitTestVisibleChanged;
		_reevaluateCaptureDelegate = ReevaluateCaptureAsync;
		_reevaluateCaptureOperation = null;
	}

	private void PreNotifyInput(object sender, NotifyInputEventArgs e)
	{
		if (e.StagingItem.Input.RoutedEvent == InputManager.PreviewInputReportEvent)
		{
			InputReportEventArgs inputReportEventArgs = e.StagingItem.Input as InputReportEventArgs;
			if (!inputReportEventArgs.Handled && inputReportEventArgs.Report.Type == InputType.Stylus)
			{
				RawStylusInputReport rawStylusInputReport = (RawStylusInputReport)inputReportEventArgs.Report;
				PointerStylusDevice pointerStylusDevice = rawStylusInputReport.StylusDevice.As<PointerStylusDevice>();
				if (!_inDragDrop && pointerStylusDevice.CurrentPointerProvider.IsWindowEnabled)
				{
					Point position = pointerStylusDevice.GetPosition(null);
					IInputElement newOver = pointerStylusDevice.FindTarget(pointerStylusDevice.CriticalActiveSource, position);
					SelectStylusDevice(pointerStylusDevice, newOver, updateOver: true);
				}
				else
				{
					SelectStylusDevice(pointerStylusDevice, null, updateOver: false);
				}
				_inputManager.Value.MostRecentInputDevice = pointerStylusDevice.StylusDevice;
				GetManagerForSource(pointerStylusDevice.ActiveSource)?.VerifyStylusPlugInCollectionTarget(rawStylusInputReport);
			}
		}
		UpdateTapCount(e);
	}

	private void PreProcessMouseInput(PreProcessInputEventArgs e, InputReportEventArgs input)
	{
		RawMouseInputReport rawMouseInputReport = (RawMouseInputReport)input.Report;
		bool flag = StylusLogic.IsPromotedMouseEvent(rawMouseInputReport);
		PointerStylusDevice pointerStylusDevice = (input.Device as StylusDevice)?.StylusDeviceImpl?.As<PointerStylusDevice>();
		if (flag && CurrentStylusDevice?.As<PointerStylusDevice>()?.TouchDevice?.PromotingToOther != true && CurrentStylusDevice?.As<PointerStylusDevice>()?.TouchDevice?.PromotingToManipulation == true)
		{
			if ((rawMouseInputReport.Actions & RawMouseActions.Activate) == RawMouseActions.Activate)
			{
				MouseDevice.PushActivateInputReport(e, input, rawMouseInputReport, clearExtraInformation: true);
			}
			input.Handled = true;
			e.Cancel();
		}
		else
		{
			if (flag || pointerStylusDevice != null)
			{
				return;
			}
			switch (rawMouseInputReport.Actions)
			{
			case RawMouseActions.AbsoluteMove:
			{
				StylusDeviceBase currentStylusDevice2 = CurrentStylusDevice;
				if (currentStylusDevice2 != null && currentStylusDevice2.InRange)
				{
					e.Cancel();
					input.Handled = true;
				}
				break;
			}
			case RawMouseActions.CancelCapture:
			{
				StylusDeviceBase currentStylusDevice = CurrentStylusDevice;
				if (currentStylusDevice != null && currentStylusDevice.InRange)
				{
					RawMouseInputReport report = new RawMouseInputReport(rawMouseInputReport.Mode, rawMouseInputReport.Timestamp, rawMouseInputReport.InputSource, rawMouseInputReport.Actions, 0, 0, 0, IntPtr.Zero);
					InputReportEventArgs inputReportEventArgs = new InputReportEventArgs(CurrentStylusDevice.StylusDevice, report);
					inputReportEventArgs.RoutedEvent = InputManager.PreviewInputReportEvent;
					_inputManager.Value.ProcessInput(inputReportEventArgs);
					e.Cancel();
				}
				break;
			}
			}
		}
	}

	private void PreProcessInput(object sender, PreProcessInputEventArgs e)
	{
		if (e.StagingItem.Input.RoutedEvent != InputManager.PreviewInputReportEvent || !(e.StagingItem.Input is InputReportEventArgs { Handled: false } inputReportEventArgs))
		{
			return;
		}
		if (_inDragDrop != _inputManager.Value.InDragDrop)
		{
			_inDragDrop = _inputManager.Value.InDragDrop;
		}
		if (inputReportEventArgs.Report.Type == InputType.Mouse)
		{
			PreProcessMouseInput(e, inputReportEventArgs);
		}
		else if (inputReportEventArgs.Report.Type == InputType.Stylus)
		{
			RawStylusInputReport rawStylusInputReport = (RawStylusInputReport)inputReportEventArgs.Report;
			SystemGesture? systemGesture = rawStylusInputReport.StylusDevice.TabletDevice.TabletDeviceImpl.GenerateStaticGesture(rawStylusInputReport);
			if (systemGesture.HasValue)
			{
				GenerateGesture(rawStylusInputReport, systemGesture.Value);
			}
		}
	}

	private void PostProcessInput(object sender, ProcessInputEventArgs e)
	{
		if (e.StagingItem.Input.RoutedEvent == Mouse.LostMouseCaptureEvent || e.StagingItem.Input.RoutedEvent == Mouse.GotMouseCaptureEvent)
		{
			foreach (TabletDevice item in (IEnumerable)TabletDevices)
			{
				foreach (StylusDevice stylusDevice in item.StylusDevices)
				{
					stylusDevice.Capture(Mouse.Captured, Mouse.CapturedMode);
				}
			}
		}
		if (e.StagingItem.Input.RoutedEvent == InputManager.InputReportEvent)
		{
			InputReportEventArgs inputReportEventArgs = e.StagingItem.Input as InputReportEventArgs;
			if (!inputReportEventArgs.Handled && inputReportEventArgs.Report.Type == InputType.Stylus)
			{
				RawStylusInputReport rawStylusInputReport = (RawStylusInputReport)inputReportEventArgs.Report;
				PointerStylusDevice pointerStylusDevice = rawStylusInputReport.StylusDevice.As<PointerStylusDevice>();
				if (!_inDragDrop)
				{
					if (pointerStylusDevice.CurrentPointerProvider.IsWindowEnabled)
					{
						PromoteRawToPreview(rawStylusInputReport, e);
					}
					else if ((rawStylusInputReport.Actions & RawStylusActions.Up) != 0 && pointerStylusDevice != null)
					{
						PointerTouchDevice touchDevice = pointerStylusDevice.TouchDevice;
						if (touchDevice.IsActive)
						{
							touchDevice.OnDeactivate();
						}
					}
				}
				else if ((pointerStylusDevice == null || !pointerStylusDevice.IsPrimary) && (rawStylusInputReport.Actions & RawStylusActions.Up) != 0)
				{
					PointerTouchDevice touchDevice2 = pointerStylusDevice.TouchDevice;
					if (touchDevice2.IsActive)
					{
						touchDevice2.OnDeactivate();
					}
				}
			}
		}
		PointerStylusPlugInManager.InvokePlugInsForMouse(e);
		PromotePreviewToMain(e);
		PromoteMainToOther(e);
	}

	internal override Point DeviceUnitsFromMeasureUnits(PresentationSource source, Point measurePoint)
	{
		Point point = measurePoint * GetAndCacheTransformToDeviceMatrix(source);
		return new Point(Math.Round(point.X), Math.Round(point.Y));
	}

	internal override Point MeasureUnitsFromDeviceUnits(PresentationSource source, Point devicePoint)
	{
		Point point = devicePoint * GetAndCacheTransformToDeviceMatrix(source);
		return new Point(Math.Round(point.X), Math.Round(point.Y));
	}

	internal override void UpdateStylusCapture(StylusDeviceBase stylusDevice, IInputElement oldStylusDeviceCapture, IInputElement newStylusDeviceCapture, int timestamp)
	{
		if (newStylusDeviceCapture == _stylusCapture)
		{
			return;
		}
		DependencyObject dependencyObject = null;
		IInputElement stylusCapture = _stylusCapture;
		_stylusCapture = newStylusDeviceCapture;
		if (stylusCapture != null)
		{
			dependencyObject = stylusCapture as DependencyObject;
			if (dependencyObject is UIElement uIElement)
			{
				uIElement.IsEnabledChanged -= _captureIsEnabledChangedEventHandler;
				uIElement.IsVisibleChanged -= _captureIsVisibleChangedEventHandler;
				uIElement.IsHitTestVisibleChanged -= _captureIsHitTestVisibleChangedEventHandler;
			}
			else if (dependencyObject is ContentElement contentElement)
			{
				contentElement.IsEnabledChanged -= _captureIsEnabledChangedEventHandler;
			}
			else
			{
				if (!(dependencyObject is UIElement3D uIElement3D))
				{
					throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, stylusCapture.GetType()));
				}
				uIElement3D.IsEnabledChanged -= _captureIsEnabledChangedEventHandler;
				uIElement3D.IsVisibleChanged -= _captureIsVisibleChangedEventHandler;
				uIElement3D.IsHitTestVisibleChanged -= _captureIsHitTestVisibleChangedEventHandler;
			}
		}
		if (_stylusCapture != null)
		{
			dependencyObject = _stylusCapture as DependencyObject;
			if (dependencyObject is UIElement uIElement2)
			{
				uIElement2.IsEnabledChanged += _captureIsEnabledChangedEventHandler;
				uIElement2.IsVisibleChanged += _captureIsVisibleChangedEventHandler;
				uIElement2.IsHitTestVisibleChanged += _captureIsHitTestVisibleChangedEventHandler;
			}
			else if (dependencyObject is ContentElement contentElement2)
			{
				contentElement2.IsEnabledChanged += _captureIsEnabledChangedEventHandler;
			}
			else
			{
				if (!(dependencyObject is UIElement3D uIElement3D2))
				{
					throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, _stylusCapture.GetType()));
				}
				uIElement3D2.IsEnabledChanged += _captureIsEnabledChangedEventHandler;
				uIElement3D2.IsVisibleChanged += _captureIsVisibleChangedEventHandler;
				uIElement3D2.IsHitTestVisibleChanged += _captureIsHitTestVisibleChangedEventHandler;
			}
		}
		UIElement.StylusCaptureWithinProperty.OnOriginValueChanged(stylusCapture as DependencyObject, _stylusCapture as DependencyObject, ref _stylusCaptureWithinTreeState);
		if (stylusCapture != null)
		{
			dependencyObject = stylusCapture as DependencyObject;
			dependencyObject.SetValue(UIElement.IsStylusCapturedPropertyKey, value: false);
		}
		if (_stylusCapture != null)
		{
			dependencyObject = _stylusCapture as DependencyObject;
			dependencyObject.SetValue(UIElement.IsStylusCapturedPropertyKey, value: true);
		}
	}

	internal override void UpdateOverProperty(StylusDeviceBase stylusDevice, IInputElement newOver)
	{
		if (stylusDevice != _currentStylusDevice || newOver == _stylusOver)
		{
			return;
		}
		DependencyObject dependencyObject = null;
		IInputElement stylusOver = _stylusOver;
		_stylusOver = newOver;
		if (stylusOver != null)
		{
			dependencyObject = stylusOver as DependencyObject;
			if (dependencyObject is UIElement uIElement)
			{
				uIElement.IsEnabledChanged -= _overIsEnabledChangedEventHandler;
				uIElement.IsVisibleChanged -= _overIsVisibleChangedEventHandler;
				uIElement.IsHitTestVisibleChanged -= _overIsHitTestVisibleChangedEventHandler;
			}
			else if (dependencyObject is ContentElement contentElement)
			{
				contentElement.IsEnabledChanged -= _overIsEnabledChangedEventHandler;
			}
			else
			{
				if (!(dependencyObject is UIElement3D uIElement3D))
				{
					throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, stylusOver.GetType()));
				}
				uIElement3D.IsEnabledChanged -= _overIsEnabledChangedEventHandler;
				uIElement3D.IsVisibleChanged -= _overIsVisibleChangedEventHandler;
				uIElement3D.IsHitTestVisibleChanged -= _overIsHitTestVisibleChangedEventHandler;
			}
		}
		if (_stylusOver != null)
		{
			dependencyObject = _stylusOver as DependencyObject;
			if (dependencyObject is UIElement uIElement2)
			{
				uIElement2.IsEnabledChanged += _overIsEnabledChangedEventHandler;
				uIElement2.IsVisibleChanged += _overIsVisibleChangedEventHandler;
				uIElement2.IsHitTestVisibleChanged += _overIsHitTestVisibleChangedEventHandler;
			}
			else if (dependencyObject is ContentElement contentElement2)
			{
				contentElement2.IsEnabledChanged += _overIsEnabledChangedEventHandler;
			}
			else
			{
				if (!(dependencyObject is UIElement3D uIElement3D2))
				{
					throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, _stylusOver.GetType()));
				}
				uIElement3D2.IsEnabledChanged += _overIsEnabledChangedEventHandler;
				uIElement3D2.IsVisibleChanged += _overIsVisibleChangedEventHandler;
				uIElement3D2.IsHitTestVisibleChanged += _overIsHitTestVisibleChangedEventHandler;
			}
		}
		UIElement.StylusOverProperty.OnOriginValueChanged(stylusOver as DependencyObject, _stylusOver as DependencyObject, ref _stylusOverTreeState);
		if (stylusOver != null)
		{
			dependencyObject = stylusOver as DependencyObject;
			dependencyObject.SetValue(UIElement.IsStylusDirectlyOverPropertyKey, value: false);
		}
		if (_stylusOver != null)
		{
			dependencyObject = _stylusOver as DependencyObject;
			dependencyObject.SetValue(UIElement.IsStylusDirectlyOverPropertyKey, value: true);
		}
	}

	private void OnOverIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		ReevaluateStylusOver(null, null, isCoreParent: true);
	}

	private void OnOverIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		ReevaluateStylusOver(null, null, isCoreParent: true);
	}

	private void OnOverIsHitTestVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		ReevaluateStylusOver(null, null, isCoreParent: true);
	}

	private void OnCaptureIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		ReevaluateCapture(null, null, isCoreParent: true);
	}

	private void OnCaptureIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		ReevaluateCapture(null, null, isCoreParent: true);
	}

	private void OnCaptureIsHitTestVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		ReevaluateCapture(null, null, isCoreParent: true);
	}

	internal override void ReevaluateCapture(DependencyObject element, DependencyObject oldParent, bool isCoreParent)
	{
		if (element != null)
		{
			if (isCoreParent)
			{
				_stylusCaptureWithinTreeState.SetCoreParent(element, oldParent);
			}
			else
			{
				_stylusCaptureWithinTreeState.SetLogicalParent(element, oldParent);
			}
		}
		if (_reevaluateCaptureOperation == null)
		{
			_reevaluateCaptureOperation = base.Dispatcher.BeginInvoke(DispatcherPriority.Input, _reevaluateCaptureDelegate, null);
		}
	}

	private object ReevaluateCaptureAsync(object arg)
	{
		_reevaluateCaptureOperation = null;
		if (_stylusCapture == null)
		{
			return null;
		}
		bool flag = false;
		DependencyObject dependencyObject = _stylusCapture as DependencyObject;
		if (dependencyObject is UIElement element)
		{
			flag = !ValidateUIElementForCapture(element);
		}
		else if (dependencyObject is ContentElement element2)
		{
			flag = !ValidateContentElementForCapture(element2);
		}
		else
		{
			if (!(dependencyObject is UIElement3D element3))
			{
				throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, _stylusCapture.GetType()));
			}
			flag = !ValidateUIElement3DForCapture(element3);
		}
		if (!flag)
		{
			DependencyObject containingVisual = InputElement.GetContainingVisual(dependencyObject);
			flag = !ValidateVisualForCapture(containingVisual, CurrentStylusDevice);
		}
		if (flag)
		{
			Stylus.Capture(null);
		}
		if (_stylusCaptureWithinTreeState != null && !_stylusCaptureWithinTreeState.IsEmpty)
		{
			UIElement.StylusCaptureWithinProperty.OnOriginValueChanged(_stylusCapture as DependencyObject, _stylusCapture as DependencyObject, ref _stylusCaptureWithinTreeState);
		}
		return null;
	}

	internal override void ReevaluateStylusOver(DependencyObject element, DependencyObject oldParent, bool isCoreParent)
	{
		if (element != null)
		{
			if (isCoreParent)
			{
				_stylusOverTreeState.SetCoreParent(element, oldParent);
			}
			else
			{
				_stylusOverTreeState.SetLogicalParent(element, oldParent);
			}
		}
		if (_reevaluateStylusOverOperation == null)
		{
			_reevaluateStylusOverOperation = base.Dispatcher.BeginInvoke(DispatcherPriority.Input, _reevaluateStylusOverDelegate, null);
		}
	}

	private object ReevaluateStylusOverAsync(object arg)
	{
		_reevaluateStylusOverOperation = null;
		if (_stylusOverTreeState != null && !_stylusOverTreeState.IsEmpty)
		{
			UIElement.StylusOverProperty.OnOriginValueChanged(_stylusOver as DependencyObject, _stylusOver as DependencyObject, ref _stylusOverTreeState);
		}
		return null;
	}

	protected override void OnTabletRemoved(uint wisptisIndex)
	{
		IsEnabled = false;
	}

	internal override void HandleMessage(WindowMessage msg, nint wParam, nint lParam)
	{
		switch (msg)
		{
		case WindowMessage.WM_DEVICECHANGE:
			_pointerDevices.Refresh();
			break;
		case WindowMessage.WM_DISPLAYCHANGE:
			_pointerDevices.Refresh();
			break;
		case WindowMessage.WM_WININICHANGE:
			ReadSystemConfig();
			_pointerDevices.Refresh();
			break;
		case WindowMessage.WM_TABLET_ADDED:
			_pointerDevices.Refresh();
			break;
		case WindowMessage.WM_TABLET_DELETED:
			_pointerDevices.Refresh();
			break;
		}
	}

	private bool IsTouchPromotionEvent(StylusEventArgs stylusEventArgs)
	{
		if (stylusEventArgs != null)
		{
			RoutedEvent routedEvent = stylusEventArgs.RoutedEvent;
			if (stylusEventArgs != null && stylusEventArgs.StylusDevice?.TabletDevice?.Type == TabletDeviceType.Touch)
			{
				if (routedEvent != Stylus.StylusMoveEvent && routedEvent != Stylus.StylusDownEvent)
				{
					return routedEvent == Stylus.StylusUpEvent;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	private void PromoteRawToPreview(RawStylusInputReport report, ProcessInputEventArgs e)
	{
		RoutedEvent previewEventFromRawStylusActions = StylusLogic.GetPreviewEventFromRawStylusActions(report.Actions);
		if (previewEventFromRawStylusActions != null && report.StylusDevice != null)
		{
			StylusEventArgs stylusEventArgs;
			if (previewEventFromRawStylusActions != Stylus.PreviewStylusSystemGestureEvent)
			{
				stylusEventArgs = ((previewEventFromRawStylusActions != Stylus.PreviewStylusDownEvent) ? new StylusEventArgs(report.StylusDevice, report.Timestamp) : new StylusDownEventArgs(report.StylusDevice, report.Timestamp));
			}
			else
			{
				RawStylusSystemGestureInputReport rawStylusSystemGestureInputReport = (RawStylusSystemGestureInputReport)report;
				stylusEventArgs = new StylusSystemGestureEventArgs(report.StylusDevice, report.Timestamp, rawStylusSystemGestureInputReport.SystemGesture, rawStylusSystemGestureInputReport.GestureX, rawStylusSystemGestureInputReport.GestureY, rawStylusSystemGestureInputReport.ButtonState);
			}
			stylusEventArgs.InputReport = report;
			stylusEventArgs.RoutedEvent = previewEventFromRawStylusActions;
			e.PushInput(stylusEventArgs, e.StagingItem);
		}
	}

	private void PromotePreviewToMain(ProcessInputEventArgs e)
	{
		if (!e.StagingItem.Input.Handled)
		{
			RoutedEvent mainEventFromPreviewEvent = StylusLogic.GetMainEventFromPreviewEvent(e.StagingItem.Input.RoutedEvent);
			if (mainEventFromPreviewEvent != null)
			{
				StylusEventArgs stylusEventArgs = (StylusEventArgs)e.StagingItem.Input;
				StylusDevice stylusDevice = stylusEventArgs.InputReport.StylusDevice;
				StylusEventArgs stylusEventArgs2;
				if (mainEventFromPreviewEvent == Stylus.StylusDownEvent || mainEventFromPreviewEvent == Stylus.PreviewStylusDownEvent)
				{
					_ = (StylusDownEventArgs)stylusEventArgs;
					stylusEventArgs2 = new StylusDownEventArgs(stylusDevice, stylusEventArgs.Timestamp);
				}
				else if (mainEventFromPreviewEvent == Stylus.StylusButtonDownEvent || mainEventFromPreviewEvent == Stylus.StylusButtonUpEvent)
				{
					StylusButtonEventArgs stylusButtonEventArgs = (StylusButtonEventArgs)stylusEventArgs;
					stylusEventArgs2 = new StylusButtonEventArgs(stylusDevice, stylusEventArgs.Timestamp, stylusButtonEventArgs.StylusButton);
				}
				else if (mainEventFromPreviewEvent != Stylus.StylusSystemGestureEvent)
				{
					stylusEventArgs2 = new StylusEventArgs(stylusDevice, stylusEventArgs.Timestamp);
				}
				else
				{
					StylusSystemGestureEventArgs stylusSystemGestureEventArgs = (StylusSystemGestureEventArgs)stylusEventArgs;
					stylusEventArgs2 = new StylusSystemGestureEventArgs(stylusDevice, stylusSystemGestureEventArgs.Timestamp, stylusSystemGestureEventArgs.SystemGesture, stylusSystemGestureEventArgs.GestureX, stylusSystemGestureEventArgs.GestureY, stylusSystemGestureEventArgs.ButtonState);
				}
				stylusEventArgs2.InputReport = stylusEventArgs.InputReport;
				stylusEventArgs2.RoutedEvent = mainEventFromPreviewEvent;
				e.PushInput(stylusEventArgs2, e.StagingItem);
			}
		}
		else if (e.StagingItem.Input is StylusEventArgs stylusEventArgs3 && stylusEventArgs3.RoutedEvent == Stylus.PreviewStylusUpEvent && stylusEventArgs3.StylusDeviceImpl.As<PointerStylusDevice>().TouchDevice.IsActive)
		{
			stylusEventArgs3.StylusDeviceImpl.As<PointerStylusDevice>().TouchDevice.OnDeactivate();
		}
	}

	private void PromoteMainToOther(ProcessInputEventArgs e)
	{
		if (!(e.StagingItem.Input is StylusEventArgs stylusEventArgs))
		{
			return;
		}
		PointerTouchDevice touchDevice = stylusEventArgs.StylusDeviceImpl.As<PointerStylusDevice>().TouchDevice;
		if (!IsTouchPromotionEvent(stylusEventArgs))
		{
			return;
		}
		if (e.StagingItem.Input.Handled)
		{
			if (stylusEventArgs.RoutedEvent == Stylus.StylusUpEvent && touchDevice.IsActive)
			{
				touchDevice.OnDeactivate();
			}
		}
		else
		{
			PromoteMainToTouch(e, stylusEventArgs);
		}
	}

	private void PromoteMainToTouch(ProcessInputEventArgs e, StylusEventArgs stylusEventArgs)
	{
		PointerStylusDevice stylusDevice = stylusEventArgs.StylusDeviceImpl.As<PointerStylusDevice>();
		if (stylusEventArgs.RoutedEvent == Stylus.StylusMoveEvent)
		{
			PromoteMainMoveToTouch(stylusDevice, e.StagingItem);
		}
		else if (stylusEventArgs.RoutedEvent == Stylus.StylusDownEvent)
		{
			PromoteMainDownToTouch(stylusDevice, e.StagingItem);
		}
		else if (stylusEventArgs.RoutedEvent == Stylus.StylusUpEvent)
		{
			PromoteMainUpToTouch(stylusDevice, e.StagingItem);
		}
	}

	private void PromoteMainDownToTouch(PointerStylusDevice stylusDevice, StagingAreaInputItem stagingItem)
	{
		PointerTouchDevice touchDevice = stylusDevice.TouchDevice;
		if (touchDevice.IsActive)
		{
			touchDevice.OnDeactivate();
		}
		touchDevice.OnActivate();
		touchDevice.OnDown();
	}

	private void PromoteMainMoveToTouch(PointerStylusDevice stylusDevice, StagingAreaInputItem stagingItem)
	{
		PointerTouchDevice touchDevice = stylusDevice.TouchDevice;
		if (touchDevice.IsActive)
		{
			touchDevice.OnMove();
		}
	}

	private void PromoteMainUpToTouch(PointerStylusDevice stylusDevice, StagingAreaInputItem stagingItem)
	{
		PointerTouchDevice touchDevice = stylusDevice.TouchDevice;
		if (touchDevice.IsActive)
		{
			touchDevice.OnUp();
		}
	}

	internal void SelectStylusDevice(PointerStylusDevice pointerStylusDevice, IInputElement newOver, bool updateOver)
	{
		bool flag = _currentStylusDevice != pointerStylusDevice;
		PointerStylusDevice currentStylusDevice = _currentStylusDevice;
		if (updateOver && pointerStylusDevice == null && flag && newOver == null)
		{
			_currentStylusDevice.ChangeStylusOver(newOver);
		}
		_currentStylusDevice = pointerStylusDevice;
		if (updateOver && pointerStylusDevice != null)
		{
			pointerStylusDevice.ChangeStylusOver(newOver);
			if (flag && currentStylusDevice != null && !currentStylusDevice.InRange)
			{
				currentStylusDevice.ChangeStylusOver(null);
			}
		}
	}

	internal PointerStylusPlugInManager GetManagerForSource(PresentationSource source)
	{
		if (source == null)
		{
			return null;
		}
		PointerStylusPlugInManager value = null;
		PlugInManagers.TryGetValue(source, out value);
		return value;
	}

	private void UpdateTapCount(NotifyInputEventArgs args)
	{
		if (args.StagingItem.Input.RoutedEvent == Stylus.PreviewStylusDownEvent)
		{
			StylusEventArgs stylusEventArgs = args.StagingItem.Input as StylusDownEventArgs;
			PointerStylusDevice pointerStylusDevice = stylusEventArgs.StylusDevice.As<PointerStylusDevice>();
			Point position = pointerStylusDevice.GetPosition(null);
			bool flag = (pointerStylusDevice.StylusButtons.GetStylusButtonByGuid(StylusPointPropertyIds.BarrelButton)?.StylusButtonState ?? StylusButtonState.Up) == StylusButtonState.Down;
			int num = Math.Abs(stylusEventArgs.Timestamp - _lastTapTimeTicks);
			Point lastTapPoint = DeviceUnitsFromMeasureUnits(pointerStylusDevice.CriticalActiveSource, position);
			Size doubleTapSize = pointerStylusDevice.PointerTabletDevice.DoubleTapSize;
			bool flag2 = Math.Abs(lastTapPoint.X - _lastTapPoint.X) < doubleTapSize.Width && Math.Abs(lastTapPoint.Y - _lastTapPoint.Y) < doubleTapSize.Height;
			if (num < pointerStylusDevice.PointerTabletDevice.DoubleTapDeltaTime && flag2 && flag == _lastTapBarrelDown)
			{
				pointerStylusDevice.TapCount++;
				return;
			}
			pointerStylusDevice.TapCount = 1;
			_lastTapPoint = lastTapPoint;
			_lastTapTimeTicks = stylusEventArgs.Timestamp;
			_lastTapBarrelDown = flag;
		}
		else if (args.StagingItem.Input.RoutedEvent == Stylus.PreviewStylusSystemGestureEvent)
		{
			StylusSystemGestureEventArgs stylusSystemGestureEventArgs = args.StagingItem.Input as StylusSystemGestureEventArgs;
			PointerStylusDevice pointerStylusDevice2 = stylusSystemGestureEventArgs.StylusDevice.As<PointerStylusDevice>();
			if (stylusSystemGestureEventArgs.SystemGesture == SystemGesture.Drag || stylusSystemGestureEventArgs.SystemGesture == SystemGesture.RightDrag)
			{
				pointerStylusDevice2.TapCount = 1;
			}
		}
	}

	private void GenerateGesture(RawStylusInputReport rawStylusInputReport, SystemGesture gesture)
	{
		PointerStylusDevice stylusDevice = rawStylusInputReport.StylusDevice.As<PointerStylusDevice>();
		RawStylusSystemGestureInputReport report = new RawStylusSystemGestureInputReport(InputMode.Foreground, rawStylusInputReport.Timestamp, rawStylusInputReport.InputSource, () => stylusDevice.PointerTabletDevice.StylusPointDescription, rawStylusInputReport.TabletDeviceId, rawStylusInputReport.StylusDeviceId, gesture, 0, 0, 0)
		{
			StylusDevice = stylusDevice.StylusDevice
		};
		InputReportEventArgs inputReportEventArgs = new InputReportEventArgs(stylusDevice.StylusDevice, report);
		inputReportEventArgs.RoutedEvent = InputManager.PreviewInputReportEvent;
		_inputManager.Value.ProcessInput(inputReportEventArgs);
	}
}
