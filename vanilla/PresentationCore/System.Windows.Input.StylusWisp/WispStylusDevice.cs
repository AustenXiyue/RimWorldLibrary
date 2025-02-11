using System.ComponentModel;
using System.Globalization;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Interop;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace System.Windows.Input.StylusWisp;

internal class WispStylusDevice : StylusDeviceBase
{
	private WispTabletDevice _tabletDevice;

	private string _sName;

	private int _id;

	private bool _fInverted;

	private bool _fInRange;

	private StylusButtonCollection _stylusButtonCollection;

	private IInputElement _stylusOver;

	private IInputElement _stylusCapture;

	private CaptureMode _captureMode;

	private StylusPoint _rawPosition = new StylusPoint(0.0, 0.0);

	private Point _rawElementRelativePosition = new Point(0.0, 0.0);

	private StylusPointCollection _eventStylusPoints;

	private SecurityCriticalDataClass<PresentationSource> _inputSource;

	private SecurityCriticalDataClass<PenContext> _activePenContext;

	private bool _needToSendMouseDown;

	private Point _lastMouseScreenLocation = new Point(0.0, 0.0);

	private Point _lastScreenLocation = new Point(0.0, 0.0);

	private bool _fInAir = true;

	private bool _fLeftButtonDownTrigger = true;

	private bool _fGestureWasFired = true;

	private bool _fBlockMouseMoveChanges;

	private bool _fDetectedDrag;

	private MouseButtonState _promotedMouseState;

	private StylusPlugInCollection _nonVerifiedTarget;

	private StylusPlugInCollection _verifiedTarget;

	private object _rtiCaptureChanged = new object();

	private StylusPlugInCollection _stylusCapturePlugInCollection;

	private Point _lastTapXY = new Point(0.0, 0.0);

	private int _tapCount;

	private int _lastTapTime;

	private bool _lastTapBarrelDown;

	private bool _seenDoubleTapGesture;

	private bool _seenHoldEnterGesture;

	private bool _sawMouseButton1Down;

	private bool _ignoreStroke;

	private WispLogic _stylusLogic;

	private WispStylusTouchDevice _touchDevice;

	internal override IInputElement Target
	{
		get
		{
			VerifyAccess();
			return _stylusOver;
		}
	}

	internal override bool IsValid => _tabletDevice != null;

	internal override PresentationSource ActiveSource
	{
		get
		{
			if (_inputSource != null)
			{
				return _inputSource.Value;
			}
			return null;
		}
	}

	internal override PresentationSource CriticalActiveSource
	{
		get
		{
			if (_inputSource != null)
			{
				return _inputSource.Value;
			}
			return null;
		}
	}

	internal PenContext ActivePenContext
	{
		get
		{
			if (_activePenContext != null)
			{
				return _activePenContext.Value;
			}
			return null;
		}
	}

	internal StylusPlugInCollection CurrentNonVerifiedTarget
	{
		get
		{
			return _nonVerifiedTarget;
		}
		set
		{
			_nonVerifiedTarget = value;
		}
	}

	internal override StylusPlugInCollection CurrentVerifiedTarget
	{
		get
		{
			return _verifiedTarget;
		}
		set
		{
			_verifiedTarget = value;
		}
	}

	internal override IInputElement DirectlyOver
	{
		get
		{
			VerifyAccess();
			return _stylusOver;
		}
	}

	internal override IInputElement Captured
	{
		get
		{
			VerifyAccess();
			return _stylusCapture;
		}
	}

	internal override CaptureMode CapturedMode => _captureMode;

	internal override TabletDevice TabletDevice => _tabletDevice.TabletDevice;

	internal override string Name
	{
		get
		{
			VerifyAccess();
			return _sName;
		}
	}

	internal override int Id
	{
		get
		{
			VerifyAccess();
			return _id;
		}
	}

	internal override StylusButtonCollection StylusButtons
	{
		get
		{
			VerifyAccess();
			return _stylusButtonCollection;
		}
	}

	internal override StylusPoint RawStylusPoint => _rawPosition;

	internal override bool InAir
	{
		get
		{
			VerifyAccess();
			return _fInAir;
		}
	}

	internal override bool Inverted
	{
		get
		{
			VerifyAccess();
			return _fInverted;
		}
	}

	internal override bool InRange
	{
		get
		{
			VerifyAccess();
			return _fInRange;
		}
	}

	internal override int TapCount
	{
		get
		{
			return _tapCount;
		}
		set
		{
			_tapCount = value;
		}
	}

	internal int LastTapTime
	{
		get
		{
			return _lastTapTime;
		}
		set
		{
			_lastTapTime = value;
		}
	}

	internal Point LastTapPoint
	{
		get
		{
			return _lastTapXY;
		}
		set
		{
			_lastTapXY = value;
		}
	}

	internal bool LastTapBarrelDown
	{
		get
		{
			return _lastTapBarrelDown;
		}
		set
		{
			_lastTapBarrelDown = value;
		}
	}

	internal override int DoubleTapDeltaX => (int)_tabletDevice.DoubleTapSize.Width;

	internal override int DoubleTapDeltaY => (int)_tabletDevice.DoubleTapSize.Height;

	internal override int DoubleTapDeltaTime => _stylusLogic.DoubleTapDeltaTime;

	internal Point LastMouseScreenPoint
	{
		get
		{
			return _lastMouseScreenLocation;
		}
		set
		{
			_lastMouseScreenLocation = value;
		}
	}

	internal bool SeenDoubleTapGesture
	{
		get
		{
			return _seenDoubleTapGesture;
		}
		set
		{
			_seenDoubleTapGesture = value;
		}
	}

	internal bool SeenHoldEnterGesture => _seenHoldEnterGesture;

	internal bool GestureWasFired => _fGestureWasFired;

	internal bool SentMouseDown => _promotedMouseState == MouseButtonState.Pressed;

	internal bool DetectedDrag => _fDetectedDrag;

	internal bool LeftIsActiveMouseButton => _fLeftButtonDownTrigger;

	internal bool IgnoreStroke
	{
		get
		{
			return _ignoreStroke;
		}
		set
		{
			_ignoreStroke = value;
		}
	}

	internal WispStylusTouchDevice TouchDevice
	{
		get
		{
			if (_touchDevice == null)
			{
				_touchDevice = new WispStylusTouchDevice(this);
			}
			return _touchDevice;
		}
	}

	internal WispStylusDevice(WispTabletDevice tabletDevice, string sName, int id, bool fInverted, StylusButtonCollection stylusButtonCollection)
	{
		_tabletDevice = tabletDevice;
		_sName = sName;
		_id = id;
		_fInverted = fInverted;
		_fInRange = false;
		_stylusButtonCollection = stylusButtonCollection;
		if (_stylusButtonCollection != null)
		{
			foreach (StylusButton item in _stylusButtonCollection)
			{
				item.SetOwner(this);
			}
		}
		_stylusLogic = StylusLogic.GetCurrentStylusLogicAs<WispLogic>();
		_stylusLogic.RegisterStylusDeviceCore(base.StylusDevice);
	}

	protected override void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}
		if (disposing)
		{
			_stylusLogic.UnregisterStylusDeviceCore(base.StylusDevice);
			WispStylusTouchDevice touchDevice = _touchDevice;
			if (touchDevice != null && touchDevice.IsActive)
			{
				_touchDevice.OnDeactivate();
			}
			_inputSource = null;
			_stylusCapture = null;
			_stylusOver = null;
			_nonVerifiedTarget = null;
			_verifiedTarget = null;
			_rtiCaptureChanged = null;
			_stylusCapturePlugInCollection = null;
			_fBlockMouseMoveChanges = false;
			_tabletDevice = null;
			_stylusLogic = null;
			_fInRange = false;
			_touchDevice = null;
		}
		_disposed = true;
	}

	internal override bool Capture(IInputElement element, CaptureMode captureMode)
	{
		int tickCount = Environment.TickCount;
		VerifyAccess();
		if (captureMode != 0 && captureMode != CaptureMode.Element && captureMode != CaptureMode.SubTree)
		{
			throw new InvalidEnumArgumentException("captureMode", (int)captureMode, typeof(CaptureMode));
		}
		if (element == null)
		{
			captureMode = CaptureMode.None;
		}
		if (captureMode == CaptureMode.None)
		{
			element = null;
		}
		DependencyObject dependencyObject = element as DependencyObject;
		if (dependencyObject != null && !InputElement.IsValid(element))
		{
			throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, dependencyObject.GetType()));
		}
		dependencyObject?.VerifyAccess();
		bool flag = false;
		if (element is UIElement uIElement)
		{
			if (uIElement.IsVisible || uIElement.IsEnabled)
			{
				flag = true;
			}
		}
		else if (element is ContentElement contentElement)
		{
			if (contentElement.IsEnabled)
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			ChangeStylusCapture(element, captureMode, tickCount);
		}
		return flag;
	}

	internal override bool Capture(IInputElement element)
	{
		return Capture(element, CaptureMode.Element);
	}

	internal override StylusPlugInCollection GetCapturedPlugInCollection(ref bool elementHasCapture)
	{
		lock (_rtiCaptureChanged)
		{
			elementHasCapture = _stylusCapture != null;
			return _stylusCapturePlugInCollection;
		}
	}

	internal override void Synchronize()
	{
		if (!InRange || _inputSource == null || _inputSource.Value == null || _inputSource.Value.CompositionTarget == null || _inputSource.Value.CompositionTarget.IsDisposed)
		{
			return;
		}
		Point point = PointUtil.ScreenToClient(_lastScreenLocation, _inputSource.Value);
		IInputElement inputElement = StylusDevice.GlobalHitTest(_inputSource.Value, point);
		bool flag = false;
		if (_stylusOver == inputElement)
		{
			Point position = GetPosition(inputElement);
			flag = !DoubleUtil.AreClose(position.X, _rawElementRelativePosition.X) || !DoubleUtil.AreClose(position.Y, _rawElementRelativePosition.Y);
		}
		if (flag || _stylusOver != inputElement)
		{
			int tickCount = Environment.TickCount;
			PenContext stylusPenContextForHwnd = _stylusLogic.GetStylusPenContextForHwnd(_inputSource.Value, TabletDevice.Id);
			if (_eventStylusPoints != null && _eventStylusPoints.Count > 0 && StylusPointDescription.AreCompatible(stylusPenContextForHwnd.StylusPointDescription, _eventStylusPoints.Description))
			{
				int[] packetData = _eventStylusPoints[_eventStylusPoints.Count - 1].GetPacketData();
				Matrix tabletToScreen = _tabletDevice.TabletToScreen;
				tabletToScreen.Invert();
				Point point2 = point * tabletToScreen;
				packetData[0] = (int)point2.X;
				packetData[1] = (int)point2.Y;
				RawStylusInputReport rawStylusInputReport = new RawStylusInputReport(InputMode.Foreground, tickCount, _inputSource.Value, stylusPenContextForHwnd, InAir ? RawStylusActions.InAirMove : RawStylusActions.Move, TabletDevice.Id, Id, packetData);
				rawStylusInputReport.Synchronized = true;
				InputReportEventArgs inputReportEventArgs = new InputReportEventArgs(base.StylusDevice, rawStylusInputReport);
				inputReportEventArgs.RoutedEvent = InputManager.PreviewInputReportEvent;
				_stylusLogic.InputManagerProcessInputEventArgs(inputReportEventArgs);
			}
		}
	}

	internal void ChangeStylusCapture(IInputElement stylusCapture, CaptureMode captureMode, int timestamp)
	{
		if (stylusCapture == _stylusCapture)
		{
			return;
		}
		IInputElement stylusCapture2 = _stylusCapture;
		using (base.Dispatcher.DisableProcessing())
		{
			lock (_rtiCaptureChanged)
			{
				_stylusCapture = stylusCapture;
				_captureMode = captureMode;
				_stylusCapturePlugInCollection = null;
				if (stylusCapture != null && InputElement.GetContainingUIElement(stylusCapture as DependencyObject) is UIElement uIElement)
				{
					PresentationSource presentationSource = PresentationSource.CriticalFromVisual(uIElement);
					if (presentationSource != null)
					{
						PenContexts penContextsFromHwnd = _stylusLogic.GetPenContextsFromHwnd(presentationSource);
						_stylusCapturePlugInCollection = penContextsFromHwnd.FindPlugInCollection(uIElement);
					}
				}
			}
		}
		_stylusLogic.UpdateStylusCapture(this, stylusCapture2, _stylusCapture, timestamp);
		if (stylusCapture2 != null)
		{
			StylusEventArgs stylusEventArgs = new StylusEventArgs(base.StylusDevice, timestamp);
			stylusEventArgs.RoutedEvent = Stylus.LostStylusCaptureEvent;
			stylusEventArgs.Source = stylusCapture2;
			_stylusLogic.InputManagerProcessInputEventArgs(stylusEventArgs);
		}
		if (_stylusCapture != null)
		{
			StylusEventArgs stylusEventArgs2 = new StylusEventArgs(base.StylusDevice, timestamp);
			stylusEventArgs2.RoutedEvent = Stylus.GotStylusCaptureEvent;
			stylusEventArgs2.Source = _stylusCapture;
			_stylusLogic.InputManagerProcessInputEventArgs(stylusEventArgs2);
		}
		if (_stylusLogic.CurrentStylusDevice == this || InRange)
		{
			if (_stylusCapture != null)
			{
				IInputElement stylusOver = _stylusCapture;
				if (CapturedMode == CaptureMode.SubTree && _inputSource != null && _inputSource.Value != null)
				{
					Point position = _stylusLogic.DeviceUnitsFromMeasureUnits(_inputSource.Value, GetPosition(null));
					stylusOver = FindTarget(_inputSource.Value, position);
				}
				ChangeStylusOver(stylusOver);
			}
			else if (_inputSource != null && _inputSource.Value != null)
			{
				Point position2 = GetPosition(null);
				position2 = _stylusLogic.DeviceUnitsFromMeasureUnits(_inputSource.Value, position2);
				IInputElement stylusOver2 = StylusDevice.GlobalHitTest(_inputSource.Value, position2);
				ChangeStylusOver(stylusOver2);
			}
		}
		if (Mouse.Captured != _stylusCapture || Mouse.CapturedMode != _captureMode)
		{
			Mouse.Capture(_stylusCapture, _captureMode);
		}
	}

	internal void ChangeStylusOver(IInputElement stylusOver)
	{
		if (_stylusOver != stylusOver)
		{
			_stylusOver = stylusOver;
			_rawElementRelativePosition = GetPosition(_stylusOver);
		}
		else if (InRange)
		{
			_rawElementRelativePosition = GetPosition(_stylusOver);
		}
		_stylusLogic.UpdateOverProperty(this, _stylusOver);
	}

	internal IInputElement FindTarget(PresentationSource inputSource, Point position)
	{
		IInputElement inputElement = null;
		switch (_captureMode)
		{
		case CaptureMode.None:
			inputElement = StylusDevice.GlobalHitTest(inputSource, position);
			if (!InputElement.IsValid(inputElement))
			{
				inputElement = InputElement.GetContainingInputElement(inputElement as DependencyObject);
			}
			break;
		case CaptureMode.Element:
			inputElement = _stylusCapture;
			break;
		case CaptureMode.SubTree:
		{
			IInputElement containingInputElement = InputElement.GetContainingInputElement(_stylusCapture as DependencyObject);
			if (containingInputElement != null && inputSource != null)
			{
				inputElement = StylusDevice.GlobalHitTest(inputSource, position);
			}
			if (inputElement != null && !InputElement.IsValid(inputElement))
			{
				inputElement = InputElement.GetContainingInputElement(inputElement as DependencyObject);
			}
			if (inputElement != null)
			{
				IInputElement inputElement2 = inputElement;
				UIElement uIElement = null;
				while (inputElement2 != null && inputElement2 != containingInputElement)
				{
					inputElement2 = ((!(inputElement2 is UIElement uIElement2)) ? InputElement.GetContainingInputElement((inputElement2 as ContentElement).GetUIParent(continuePastVisualTree: true)) : InputElement.GetContainingInputElement(uIElement2.GetUIParent(continuePastVisualTree: true)));
				}
				if (inputElement2 != containingInputElement)
				{
					inputElement = _stylusCapture;
				}
			}
			else
			{
				inputElement = _stylusCapture;
			}
			break;
		}
		}
		return inputElement;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0}({1})", base.ToString(), Name);
	}

	internal override StylusPointCollection GetStylusPoints(IInputElement relativeTo)
	{
		VerifyAccess();
		if (_eventStylusPoints == null)
		{
			return new StylusPointCollection(_tabletDevice.StylusPointDescription);
		}
		return _eventStylusPoints.Clone(StylusDevice.GetElementTransform(relativeTo), _eventStylusPoints.Description);
	}

	internal override StylusPointCollection GetStylusPoints(IInputElement relativeTo, StylusPointDescription subsetToReformatTo)
	{
		if (subsetToReformatTo == null)
		{
			throw new ArgumentNullException("subsetToReformatTo");
		}
		if (_eventStylusPoints == null)
		{
			return new StylusPointCollection(subsetToReformatTo);
		}
		return _eventStylusPoints.Reformat(subsetToReformatTo, StylusDevice.GetElementTransform(relativeTo));
	}

	internal override Point GetPosition(IInputElement relativeTo)
	{
		VerifyAccess();
		if (relativeTo != null && !InputElement.IsValid(relativeTo))
		{
			throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, relativeTo.GetType()));
		}
		PresentationSource presentationSource = null;
		if (relativeTo != null)
		{
			DependencyObject containingVisual = InputElement.GetContainingVisual(relativeTo as DependencyObject);
			if (containingVisual != null)
			{
				presentationSource = PresentationSource.CriticalFromVisual(containingVisual);
			}
		}
		else if (_inputSource != null)
		{
			presentationSource = _inputSource.Value;
		}
		if (presentationSource == null || presentationSource.RootVisual == null)
		{
			return new Point(0.0, 0.0);
		}
		return InputElement.TranslatePoint(PointUtil.ClientToRoot(PointUtil.ScreenToClient(_lastScreenLocation, presentationSource), presentationSource), presentationSource.RootVisual, (DependencyObject)relativeTo);
	}

	internal Point GetRawPosition(IInputElement relativeTo)
	{
		StylusDevice.GetElementTransform(relativeTo).TryTransform((Point)_rawPosition, out var result);
		return result;
	}

	internal override MouseButtonState GetMouseButtonState(MouseButton mouseButton, MouseDevice mouseDevice)
	{
		return mouseButton switch
		{
			MouseButton.Left => _stylusLogic.GetMouseLeftOrRightButtonState(leftButton: true), 
			MouseButton.Right => _stylusLogic.GetMouseLeftOrRightButtonState(leftButton: false), 
			_ => mouseDevice.GetButtonStateFromSystem(mouseButton), 
		};
	}

	internal override Point GetMouseScreenPosition(MouseDevice mouseDevice)
	{
		return mouseDevice?.GetScreenPositionFromSystem() ?? _lastMouseScreenLocation;
	}

	private GeneralTransform GetTabletToElementTransform(PresentationSource source, IInputElement relativeTo)
	{
		return new GeneralTransformGroup
		{
			Children = 
			{
				(GeneralTransform)new MatrixTransform(_stylusLogic.GetTabletToViewTransform(source, _tabletDevice.TabletDevice)),
				StylusDevice.GetElementTransform(relativeTo)
			}
		};
	}

	internal override void UpdateEventStylusPoints(RawStylusInputReport report, bool resetIfNoOverride)
	{
		if (report.RawStylusInput != null && report.RawStylusInput.StylusPointsModified)
		{
			GeneralTransform inverse = report.RawStylusInput.Target.ViewToElement.Inverse;
			_eventStylusPoints = report.RawStylusInput.GetStylusPoints(inverse);
		}
		else if (resetIfNoOverride)
		{
			_eventStylusPoints = new StylusPointCollection(report.StylusPointDescription, report.GetRawPacketData(), GetTabletToElementTransform(report.InputSource, null), Matrix.Identity);
		}
	}

	internal void UpdateState(RawStylusInputReport report)
	{
		_eventStylusPoints = new StylusPointCollection(report.StylusPointDescription, report.GetRawPacketData(), GetTabletToElementTransform(report.InputSource, null), Matrix.Identity);
		PresentationSource presentationSource = DetermineValidSource(report.InputSource, _eventStylusPoints, report.PenContext.Contexts);
		if (presentationSource != null && presentationSource != report.InputSource)
		{
			Point measurePoint = PointUtil.ClientToScreen(new Point(0.0, 0.0), presentationSource);
			measurePoint = _stylusLogic.MeasureUnitsFromDeviceUnits(presentationSource, measurePoint);
			Point point = _stylusLogic.MeasureUnitsFromDeviceUnits(report.InputSource, report.PenContext.Contexts.DestroyedLocation);
			MatrixTransform transform = new MatrixTransform(new Matrix(1.0, 0.0, 0.0, 1.0, point.X - measurePoint.X, point.Y - measurePoint.Y));
			_eventStylusPoints = _eventStylusPoints.Reformat(report.StylusPointDescription, transform);
		}
		_rawPosition = _eventStylusPoints[_eventStylusPoints.Count - 1];
		_inputSource = new SecurityCriticalDataClass<PresentationSource>(presentationSource);
		if (presentationSource != null)
		{
			Point pointClient = _stylusLogic.DeviceUnitsFromMeasureUnits(presentationSource, (Point)_rawPosition);
			_lastScreenLocation = PointUtil.ClientToScreen(pointClient, presentationSource);
		}
		if (!_fBlockMouseMoveChanges)
		{
			_lastMouseScreenLocation = _lastScreenLocation;
		}
		if ((report.Actions & RawStylusActions.Down) != 0 || (report.Actions & RawStylusActions.Move) != 0)
		{
			_fInAir = false;
			if ((report.Actions & RawStylusActions.Down) != 0)
			{
				_needToSendMouseDown = true;
				_fGestureWasFired = false;
				_fDetectedDrag = false;
				_seenHoldEnterGesture = false;
				_tabletDevice.UpdateSizeDeltas(report.StylusPointDescription, _stylusLogic);
			}
			else if (presentationSource != null && _fBlockMouseMoveChanges && _seenDoubleTapGesture && !_fGestureWasFired && !_fDetectedDrag)
			{
				Size cancelSize = _tabletDevice.CancelSize;
				Point measurePoint2 = (Point)_eventStylusPoints[0];
				measurePoint2 = _stylusLogic.DeviceUnitsFromMeasureUnits(presentationSource, measurePoint2);
				measurePoint2 = PointUtil.ClientToScreen(measurePoint2, presentationSource);
				if (Math.Abs(_lastMouseScreenLocation.X - measurePoint2.X) > cancelSize.Width || Math.Abs(_lastMouseScreenLocation.Y - measurePoint2.Y) > cancelSize.Height)
				{
					_fDetectedDrag = true;
				}
			}
		}
		UpdateEventStylusPoints(report, resetIfNoOverride: false);
		if ((report.Actions & RawStylusActions.Up) != 0 || (report.Actions & RawStylusActions.InAirMove) != 0)
		{
			_fInAir = true;
			if ((report.Actions & RawStylusActions.Up) != 0)
			{
				_sawMouseButton1Down = false;
			}
		}
	}

	private PresentationSource DetermineValidSource(PresentationSource inputSource, StylusPointCollection stylusPoints, PenContexts penContextsOfPoints)
	{
		HwndSource hwndSource = (HwndSource)inputSource;
		if (inputSource.CompositionTarget == null || inputSource.CompositionTarget.IsDisposed || hwndSource == null || hwndSource.IsHandleNull)
		{
			PresentationSource presentationSource = null;
			if (_stylusCapture != null)
			{
				PresentationSource presentationSource2 = PresentationSource.CriticalFromVisual(InputElement.GetContainingVisual(_stylusCapture as DependencyObject));
				if (presentationSource2 != null && presentationSource2.CompositionTarget != null && !presentationSource2.CompositionTarget.IsDisposed)
				{
					presentationSource = presentationSource2;
				}
			}
			if (presentationSource == null && stylusPoints != null)
			{
				Point point;
				if (penContextsOfPoints?.InputSource?.CompositionTarget != null)
				{
					point = _stylusLogic.DeviceUnitsFromMeasureUnits(penContextsOfPoints.InputSource, (Point)stylusPoints[0]);
					point.Offset(penContextsOfPoints.DestroyedLocation.X, penContextsOfPoints.DestroyedLocation.Y);
				}
				else
				{
					point = _lastMouseScreenLocation;
				}
				nint num = MS.Win32.UnsafeNativeMethods.WindowFromPoint((int)point.X, (int)point.Y);
				if (num != IntPtr.Zero)
				{
					HwndSource hwndSource2 = HwndSource.CriticalFromHwnd(num);
					if (hwndSource2 != null && hwndSource2.Dispatcher == base.Dispatcher)
					{
						presentationSource = hwndSource2;
					}
				}
			}
			return presentationSource;
		}
		return inputSource;
	}

	internal void UpdateInRange(bool inRange, PenContext penContext)
	{
		_fInRange = inRange;
		if (inRange)
		{
			_activePenContext = new SecurityCriticalDataClass<PenContext>(penContext);
		}
		else
		{
			_activePenContext = null;
		}
	}

	internal void UpdateStateForSystemGesture(RawStylusSystemGestureInputReport report)
	{
		UpdateStateForSystemGesture(report.SystemGesture, report);
	}

	private void UpdateStateForSystemGesture(SystemGesture gesture, RawStylusSystemGestureInputReport report)
	{
		switch (gesture)
		{
		case SystemGesture.Tap:
		case SystemGesture.Drag:
			_fLeftButtonDownTrigger = true;
			_fGestureWasFired = true;
			break;
		case SystemGesture.RightTap:
		case SystemGesture.RightDrag:
			_fLeftButtonDownTrigger = false;
			_fGestureWasFired = true;
			break;
		case SystemGesture.HoldEnter:
			_seenHoldEnterGesture = true;
			break;
		case SystemGesture.Flick:
		{
			_fGestureWasFired = true;
			if (report == null || report.InputSource == null || _eventStylusPoints == null || _eventStylusPoints.Count <= 0)
			{
				break;
			}
			StylusPoint stylusPoint = _eventStylusPoints[_eventStylusPoints.Count - 1];
			stylusPoint.X = report.GestureX;
			stylusPoint.Y = report.GestureY;
			_eventStylusPoints = new StylusPointCollection(stylusPoint.Description, stylusPoint.GetPacketData(), GetTabletToElementTransform(report.InputSource, null), Matrix.Identity);
			PresentationSource presentationSource = DetermineValidSource(report.InputSource, _eventStylusPoints, report.PenContext.Contexts);
			if (presentationSource != null)
			{
				if (presentationSource != report.InputSource)
				{
					Point measurePoint = PointUtil.ClientToScreen(new Point(0.0, 0.0), presentationSource);
					measurePoint = _stylusLogic.MeasureUnitsFromDeviceUnits(presentationSource, measurePoint);
					Point point = _stylusLogic.MeasureUnitsFromDeviceUnits(report.InputSource, report.PenContext.Contexts.DestroyedLocation);
					MatrixTransform transform = new MatrixTransform(new Matrix(1.0, 0.0, 0.0, 1.0, point.X - measurePoint.X, point.Y - measurePoint.Y));
					_eventStylusPoints = _eventStylusPoints.Reformat(report.StylusPointDescription, transform);
				}
				_rawPosition = _eventStylusPoints[_eventStylusPoints.Count - 1];
				_inputSource = new SecurityCriticalDataClass<PresentationSource>(presentationSource);
				Point pointClient = _stylusLogic.DeviceUnitsFromMeasureUnits(presentationSource, (Point)_rawPosition);
				_lastScreenLocation = PointUtil.ClientToScreen(pointClient, presentationSource);
			}
			break;
		}
		}
	}

	internal void PlayBackCachedDownInputReport(int timestamp)
	{
		if (!_needToSendMouseDown)
		{
			return;
		}
		PresentationSource mousePresentationSource = GetMousePresentationSource();
		if (mousePresentationSource != null)
		{
			Point point = PointUtil.ScreenToClient(_lastMouseScreenLocation, mousePresentationSource);
			_needToSendMouseDown = false;
			_promotedMouseState = MouseButtonState.Pressed;
			RawMouseActions rawMouseActions = (_fLeftButtonDownTrigger ? RawMouseActions.Button1Press : RawMouseActions.Button2Press);
			if (_stylusLogic.UpdateMouseButtonState(rawMouseActions))
			{
				InputManager inputManager = (InputManager)base.Dispatcher.InputManager;
				if (inputManager != null && inputManager.PrimaryMouseDevice.CriticalActiveSource != mousePresentationSource)
				{
					rawMouseActions |= RawMouseActions.Activate;
				}
				RawMouseInputReport report = new RawMouseInputReport(InputMode.Foreground, timestamp, mousePresentationSource, rawMouseActions, (int)point.X, (int)point.Y, 0, IntPtr.Zero);
				InputReportEventArgs inputReportEventArgs = new InputReportEventArgs(base.StylusDevice, report);
				inputReportEventArgs.RoutedEvent = InputManager.PreviewInputReportEvent;
				_stylusLogic.InputManagerProcessInputEventArgs(inputReportEventArgs);
			}
		}
		_needToSendMouseDown = false;
	}

	internal PresentationSource GetMousePresentationSource()
	{
		InputManager inputManager = (InputManager)base.Dispatcher.InputManager;
		PresentationSource result = null;
		if (inputManager != null)
		{
			IInputElement captured = inputManager.PrimaryMouseDevice.Captured;
			if (captured != null)
			{
				DependencyObject containingVisual = InputElement.GetContainingVisual((DependencyObject)captured);
				if (containingVisual != null)
				{
					result = PresentationSource.CriticalFromVisual(containingVisual);
				}
			}
			else if (_stylusOver != null)
			{
				result = ((_inputSource != null && _inputSource.Value != null) ? DetermineValidSource(_inputSource.Value, _eventStylusPoints, null) : null);
			}
		}
		return result;
	}

	internal RawMouseActions GetMouseActionsFromStylusEventAndPlaybackCachedDown(RoutedEvent stylusEvent, StylusEventArgs stylusArgs)
	{
		if (stylusEvent == Stylus.StylusSystemGestureEvent)
		{
			StylusSystemGestureEventArgs stylusSystemGestureEventArgs = (StylusSystemGestureEventArgs)stylusArgs;
			if (stylusSystemGestureEventArgs.SystemGesture == SystemGesture.Tap || stylusSystemGestureEventArgs.SystemGesture == SystemGesture.RightTap || stylusSystemGestureEventArgs.SystemGesture == SystemGesture.Drag || stylusSystemGestureEventArgs.SystemGesture == SystemGesture.RightDrag || stylusSystemGestureEventArgs.SystemGesture == SystemGesture.Flick)
			{
				UpdateStateForSystemGesture(stylusSystemGestureEventArgs.SystemGesture, null);
				if (stylusSystemGestureEventArgs.SystemGesture == SystemGesture.Drag || stylusSystemGestureEventArgs.SystemGesture == SystemGesture.RightDrag || stylusSystemGestureEventArgs.SystemGesture == SystemGesture.Flick)
				{
					_fBlockMouseMoveChanges = false;
					TapCount = 1;
					if (stylusSystemGestureEventArgs.SystemGesture == SystemGesture.Flick)
					{
						_needToSendMouseDown = false;
					}
					else
					{
						PlayBackCachedDownInputReport(stylusSystemGestureEventArgs.Timestamp);
					}
				}
				else
				{
					PlayBackCachedDownInputReport(stylusSystemGestureEventArgs.Timestamp);
				}
			}
		}
		else
		{
			if (stylusEvent == Stylus.StylusInAirMoveEvent)
			{
				return RawMouseActions.AbsoluteMove;
			}
			if (stylusEvent == Stylus.StylusDownEvent)
			{
				_fLeftButtonDownTrigger = true;
				_fBlockMouseMoveChanges = true;
				if (_seenDoubleTapGesture || _sawMouseButton1Down)
				{
					PlayBackCachedDownInputReport(stylusArgs.Timestamp);
				}
			}
			else if (stylusEvent == Stylus.StylusMoveEvent)
			{
				if (!_fBlockMouseMoveChanges)
				{
					return RawMouseActions.AbsoluteMove;
				}
			}
			else if (stylusEvent == Stylus.StylusUpEvent)
			{
				MouseButtonState promotedMouseState = _promotedMouseState;
				ResetStateForStylusUp();
				if (promotedMouseState == MouseButtonState.Pressed)
				{
					RawMouseActions rawMouseActions = (_fLeftButtonDownTrigger ? RawMouseActions.Button1Release : RawMouseActions.Button2Release);
					if (_stylusLogic.UpdateMouseButtonState(rawMouseActions))
					{
						return rawMouseActions;
					}
				}
			}
		}
		return RawMouseActions.None;
	}

	internal void ResetStateForStylusUp()
	{
		_fBlockMouseMoveChanges = false;
		_seenDoubleTapGesture = false;
		_sawMouseButton1Down = false;
		if (_promotedMouseState == MouseButtonState.Pressed)
		{
			_promotedMouseState = MouseButtonState.Released;
		}
	}

	internal void SetSawMouseButton1Down(bool sawMouseButton1Down)
	{
		_sawMouseButton1Down = sawMouseButton1Down;
	}

	internal void UpdateTouchActiveSource()
	{
		if (_touchDevice != null)
		{
			PresentationSource criticalActiveSource = CriticalActiveSource;
			if (criticalActiveSource != null)
			{
				_touchDevice.ChangeActiveSource(criticalActiveSource);
			}
		}
	}
}
