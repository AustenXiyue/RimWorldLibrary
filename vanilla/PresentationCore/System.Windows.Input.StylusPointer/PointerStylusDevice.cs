using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Interop;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.Pointer;

namespace System.Windows.Input.StylusPointer;

internal class PointerStylusDevice : StylusDeviceBase
{
	private int _tapCount = 1;

	private StylusButtonCollection _stylusButtons;

	private PointerInteractionEngine _interactionEngine;

	private StylusPlugInCollection _stylusCapturePlugInCollection;

	private PointerLogic _pointerLogic;

	private IInputElement _stylusCapture;

	private CaptureMode _captureMode;

	private IInputElement _stylusOver;

	private Point _rawElementRelativePosition = new Point(0.0, 0.0);

	private SecurityCriticalDataClass<PresentationSource> _inputSource;

	private int _lastEventTimeTicks;

	private PointerData _pointerData;

	private UnsafeNativeMethods.POINTER_DEVICE_CURSOR_INFO _cursorInfo;

	private PointerTabletDevice _tabletDevice;

	private StylusPointCollection _currentStylusPoints;

	internal override IInputElement Target => DirectlyOver;

	internal override PresentationSource ActiveSource => _inputSource.Value;

	internal UnsafeNativeMethods.POINTER_INFO CurrentPointerInfo => _pointerData.Info;

	internal HwndPointerInputProvider CurrentPointerProvider { get; private set; }

	internal uint CursorId => _cursorInfo.cursorId;

	internal bool IsNew
	{
		get
		{
			PointerData pointerData = _pointerData;
			if (pointerData == null)
			{
				return false;
			}
			return pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_NEW);
		}
	}

	internal bool IsInContact
	{
		get
		{
			PointerData pointerData = _pointerData;
			if (pointerData == null)
			{
				return false;
			}
			return pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_INCONTACT);
		}
	}

	internal bool IsPrimary
	{
		get
		{
			PointerData pointerData = _pointerData;
			if (pointerData == null)
			{
				return false;
			}
			return pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_PRIMARY);
		}
	}

	internal bool IsFirstButton
	{
		get
		{
			PointerData pointerData = _pointerData;
			if (pointerData == null)
			{
				return false;
			}
			return pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_FIRSTBUTTON);
		}
	}

	internal bool IsSecondButton
	{
		get
		{
			PointerData pointerData = _pointerData;
			if (pointerData == null)
			{
				return false;
			}
			return pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_SECONDBUTTON);
		}
	}

	internal bool IsThirdButton
	{
		get
		{
			PointerData pointerData = _pointerData;
			if (pointerData == null)
			{
				return false;
			}
			return pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_THIRDBUTTON);
		}
	}

	internal bool IsFourthButton
	{
		get
		{
			PointerData pointerData = _pointerData;
			if (pointerData == null)
			{
				return false;
			}
			return pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_FOURTHBUTTON);
		}
	}

	internal bool IsFifthButton
	{
		get
		{
			PointerData pointerData = _pointerData;
			if (pointerData == null)
			{
				return false;
			}
			return pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_FIFTHBUTTON);
		}
	}

	internal uint TimeStamp => _pointerData?.Info.dwTime ?? 0;

	internal bool IsDown
	{
		get
		{
			PointerData pointerData = _pointerData;
			if (pointerData == null)
			{
				return false;
			}
			return pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_DOWN);
		}
	}

	internal bool IsUpdate
	{
		get
		{
			PointerData pointerData = _pointerData;
			if (pointerData == null)
			{
				return false;
			}
			return pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_UPDATE);
		}
	}

	internal bool IsUp
	{
		get
		{
			PointerData pointerData = _pointerData;
			if (pointerData == null)
			{
				return false;
			}
			return pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_UP);
		}
	}

	internal bool HasCaptureChanged
	{
		get
		{
			PointerData pointerData = _pointerData;
			if (pointerData == null)
			{
				return false;
			}
			return pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_CAPTURECHANGED);
		}
	}

	internal bool HasTransform
	{
		get
		{
			PointerData pointerData = _pointerData;
			if (pointerData == null)
			{
				return false;
			}
			return pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_HASTRANSFORM);
		}
	}

	internal PointerTouchDevice TouchDevice { get; private set; }

	internal override StylusPlugInCollection CurrentVerifiedTarget { get; set; }

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

	internal override StylusButtonCollection StylusButtons => _stylusButtons;

	internal override StylusPoint RawStylusPoint => _currentStylusPoints[_currentStylusPoints.Count - 1];

	internal override bool IsValid => true;

	internal override IInputElement DirectlyOver => _stylusOver;

	internal override IInputElement Captured => _stylusCapture;

	internal override TabletDevice TabletDevice => _tabletDevice.TabletDevice;

	internal PointerTabletDevice PointerTabletDevice => _tabletDevice;

	internal override string Name
	{
		get
		{
			if (_cursorInfo.cursor != UnsafeNativeMethods.POINTER_DEVICE_CURSOR_TYPE.POINTER_DEVICE_CURSOR_TYPE_ERASER)
			{
				return "Stylus";
			}
			return "Eraser";
		}
	}

	internal override int Id => (int)CursorId;

	internal override bool InAir
	{
		get
		{
			PointerData pointerData = _pointerData;
			if (pointerData == null || !pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_INCONTACT))
			{
				PointerData pointerData2 = _pointerData;
				if (pointerData2 == null)
				{
					return false;
				}
				return pointerData2.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_INRANGE);
			}
			return false;
		}
	}

	internal override bool Inverted
	{
		get
		{
			if (_tabletDevice.Type == TabletDeviceType.Stylus)
			{
				PointerData pointerData = _pointerData;
				if (pointerData == null)
				{
					return false;
				}
				return pointerData.PenInfo.penFlags.HasFlag(UnsafeNativeMethods.PEN_FLAGS.PEN_FLAG_INVERTED);
			}
			return false;
		}
	}

	internal override bool InRange
	{
		get
		{
			PointerData pointerData = _pointerData;
			if (pointerData == null)
			{
				return false;
			}
			return pointerData.Info.pointerFlags.HasFlag(UnsafeNativeMethods.POINTER_FLAGS.POINTER_FLAG_INRANGE);
		}
	}

	internal override int DoubleTapDeltaX => (int)PointerTabletDevice.DoubleTapSize.Width;

	internal override int DoubleTapDeltaY => (int)PointerTabletDevice.DoubleTapSize.Height;

	internal override int DoubleTapDeltaTime => PointerTabletDevice.DoubleTapDeltaTime;

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

	internal override CaptureMode CapturedMode => _captureMode;

	internal PointerStylusDevice(PointerTabletDevice tabletDevice, UnsafeNativeMethods.POINTER_DEVICE_CURSOR_INFO cursorInfo)
	{
		_cursorInfo = cursorInfo;
		_tabletDevice = tabletDevice;
		_pointerLogic = StylusLogic.GetCurrentStylusLogicAs<PointerLogic>();
		if (tabletDevice.Type == TabletDeviceType.Touch)
		{
			TouchDevice = new PointerTouchDevice(this);
		}
		_interactionEngine = new PointerInteractionEngine(this);
		_interactionEngine.InteractionDetected += HandleInteraction;
		List<StylusButton> list = new List<StylusButton>();
		foreach (StylusPointProperty stylusPointProperty in _tabletDevice.DeviceInfo.StylusPointProperties)
		{
			if (stylusPointProperty.IsButton)
			{
				StylusButton stylusButton = new StylusButton(StylusPointPropertyIds.GetStringRepresentation(stylusPointProperty.Id), stylusPointProperty.Id);
				stylusButton.SetOwner(this);
				list.Add(stylusButton);
			}
		}
		_stylusButtons = new StylusButtonCollection(list);
	}

	protected override void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			_interactionEngine.Dispose();
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
		UIElement uIElement = element as UIElement;
		if ((uIElement != null && uIElement.IsVisible) || (uIElement != null && uIElement.IsEnabled))
		{
			flag = true;
		}
		else
		{
			ContentElement obj = element as ContentElement;
			flag = obj == null || !obj.IsEnabled || true;
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

	internal override void Synchronize()
	{
		if (!InRange || _inputSource == null || _inputSource.Value == null || _inputSource.Value.CompositionTarget == null || _inputSource.Value.CompositionTarget.IsDisposed)
		{
			return;
		}
		Point point = PointUtil.ScreenToClient(new Point(_pointerData.Info.ptPixelLocationRaw.X, _pointerData.Info.ptPixelLocationRaw.Y), _inputSource.Value);
		IInputElement inputElement = StylusDevice.GlobalHitTest(_inputSource.Value, point);
		bool flag = false;
		if (_stylusOver == inputElement)
		{
			Point position = GetPosition(inputElement);
			flag = !DoubleUtil.AreClose(position.X, _rawElementRelativePosition.X) || !DoubleUtil.AreClose(position.Y, _rawElementRelativePosition.Y);
		}
		if (!flag && _stylusOver == inputElement)
		{
			return;
		}
		int tickCount = Environment.TickCount;
		if (_currentStylusPoints != null && _currentStylusPoints.Count > 0 && StylusPointDescription.AreCompatible(PointerTabletDevice.StylusPointDescription, _currentStylusPoints.Description))
		{
			int[] packetData = _currentStylusPoints[_currentStylusPoints.Count - 1].GetPacketData();
			Matrix tabletToScreen = _tabletDevice.TabletToScreen;
			tabletToScreen.Invert();
			Point point2 = point * tabletToScreen;
			packetData[0] = (int)point2.X;
			packetData[1] = (int)point2.Y;
			RawStylusInputReport rawStylusInputReport = new RawStylusInputReport(InputMode.Foreground, tickCount, _inputSource.Value, InAir ? RawStylusActions.InAirMove : RawStylusActions.Move, () => PointerTabletDevice.StylusPointDescription, TabletDevice.Id, Id, packetData);
			rawStylusInputReport.Synchronized = true;
			InputReportEventArgs inputReportEventArgs = new InputReportEventArgs(base.StylusDevice, rawStylusInputReport);
			inputReportEventArgs.RoutedEvent = InputManager.PreviewInputReportEvent;
			InputManager.Current.ProcessInput(inputReportEventArgs);
		}
	}

	internal override StylusPointCollection GetStylusPoints(IInputElement relativeTo)
	{
		VerifyAccess();
		if (_currentStylusPoints == null)
		{
			return new StylusPointCollection(_tabletDevice.StylusPointDescription);
		}
		return _currentStylusPoints.Clone(StylusDevice.GetElementTransform(relativeTo), _currentStylusPoints.Description);
	}

	internal override StylusPointCollection GetStylusPoints(IInputElement relativeTo, StylusPointDescription subsetToReformatTo)
	{
		if (subsetToReformatTo == null)
		{
			throw new ArgumentNullException("subsetToReformatTo");
		}
		if (_currentStylusPoints == null)
		{
			return new StylusPointCollection(subsetToReformatTo);
		}
		return _currentStylusPoints.Reformat(subsetToReformatTo, StylusDevice.GetElementTransform(relativeTo));
	}

	internal override Point GetPosition(IInputElement relativeTo)
	{
		VerifyAccess();
		if (relativeTo != null && !InputElement.IsValid(relativeTo))
		{
			throw new InvalidOperationException();
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
		return InputElement.TranslatePoint(PointUtil.ClientToRoot(PointUtil.ScreenToClient(new Point(_pointerData.Info.ptPixelLocationRaw.X, _pointerData.Info.ptPixelLocationRaw.Y), presentationSource), presentationSource), presentationSource.RootVisual, (DependencyObject)relativeTo);
	}

	internal override Point GetMouseScreenPosition(MouseDevice mouseDevice)
	{
		return mouseDevice.GetScreenPositionFromSystem();
	}

	internal override MouseButtonState GetMouseButtonState(MouseButton mouseButton, MouseDevice mouseDevice)
	{
		return mouseDevice.GetButtonStateFromSystem(mouseButton);
	}

	internal void Update(HwndPointerInputProvider provider, PresentationSource inputSource, PointerData pointerData, RawStylusInputReport rsir)
	{
		_lastEventTimeTicks = Environment.TickCount;
		_inputSource = new SecurityCriticalDataClass<PresentationSource>(inputSource);
		_pointerData = pointerData;
		_currentStylusPoints = new StylusPointCollection(rsir.StylusPointDescription, rsir.GetRawPacketData(), GetTabletToElementTransform(null), Matrix.Identity);
		if (rsir?.RawStylusInput?.StylusPointsModified == true)
		{
			GeneralTransform inverse = rsir.RawStylusInput.Target.ViewToElement.Inverse;
			_currentStylusPoints = rsir.RawStylusInput.GetStylusPoints(inverse);
		}
		CurrentPointerProvider = provider;
		if (PointerTabletDevice.Type == TabletDeviceType.Touch)
		{
			TouchDevice.ChangeActiveSource(_inputSource.Value);
		}
	}

	internal void UpdateInteractions(RawStylusInputReport rsir)
	{
		_interactionEngine.Update(rsir);
	}

	private void HandleInteraction(object clientData, RawStylusSystemGestureInputReport originalReport)
	{
		RawStylusSystemGestureInputReport rawStylusSystemGestureInputReport = new RawStylusSystemGestureInputReport(InputMode.Foreground, Environment.TickCount, CriticalActiveSource, () => PointerTabletDevice.StylusPointDescription, TabletDevice.Id, Id, originalReport.SystemGesture, originalReport.GestureX, originalReport.GestureY, originalReport.ButtonState)
		{
			StylusDevice = base.StylusDevice
		};
		if (rawStylusSystemGestureInputReport.SystemGesture == SystemGesture.Flick)
		{
			StylusPoint stylusPoint = _currentStylusPoints[_currentStylusPoints.Count - 1];
			stylusPoint.X = rawStylusSystemGestureInputReport.GestureX;
			stylusPoint.Y = rawStylusSystemGestureInputReport.GestureY;
			_currentStylusPoints = new StylusPointCollection(stylusPoint.Description, stylusPoint.GetPacketData(), GetTabletToElementTransform(null), Matrix.Identity);
		}
		InputReportEventArgs input = new InputReportEventArgs(base.StylusDevice, rawStylusSystemGestureInputReport)
		{
			RoutedEvent = InputManager.PreviewInputReportEvent
		};
		InputManager.UnsecureCurrent.ProcessInput(input);
	}

	internal override StylusPlugInCollection GetCapturedPlugInCollection(ref bool elementHasCapture)
	{
		elementHasCapture = _stylusCapture != null;
		return _stylusCapturePlugInCollection;
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
		_pointerLogic.UpdateOverProperty(this, _stylusOver);
	}

	internal void ChangeStylusCapture(IInputElement stylusCapture, CaptureMode captureMode, int timestamp)
	{
		if (stylusCapture == _stylusCapture)
		{
			return;
		}
		IInputElement stylusCapture2 = _stylusCapture;
		_stylusCapture = stylusCapture;
		_captureMode = captureMode;
		_stylusCapturePlugInCollection = null;
		if (stylusCapture != null && InputElement.GetContainingUIElement(stylusCapture as DependencyObject) is UIElement uIElement)
		{
			PresentationSource presentationSource = PresentationSource.CriticalFromVisual(uIElement);
			if (presentationSource != null && _pointerLogic.PlugInManagers.TryGetValue(presentationSource, out var value))
			{
				_stylusCapturePlugInCollection = value.FindPlugInCollection(uIElement);
			}
		}
		_pointerLogic.UpdateStylusCapture(this, stylusCapture2, _stylusCapture, timestamp);
		if (stylusCapture2 != null)
		{
			StylusEventArgs stylusEventArgs = new StylusEventArgs(base.StylusDevice, timestamp);
			stylusEventArgs.RoutedEvent = Stylus.LostStylusCaptureEvent;
			stylusEventArgs.Source = stylusCapture2;
			InputManager.UnsecureCurrent.ProcessInput(stylusEventArgs);
		}
		if (_stylusCapture != null)
		{
			StylusEventArgs stylusEventArgs2 = new StylusEventArgs(base.StylusDevice, timestamp);
			stylusEventArgs2.RoutedEvent = Stylus.GotStylusCaptureEvent;
			stylusEventArgs2.Source = _stylusCapture;
			InputManager.UnsecureCurrent.ProcessInput(stylusEventArgs2);
		}
		if (_pointerLogic.CurrentStylusDevice == this || InRange)
		{
			if (_stylusCapture != null)
			{
				IInputElement stylusOver = _stylusCapture;
				if (CapturedMode == CaptureMode.SubTree && _inputSource != null && _inputSource.Value != null)
				{
					Point position = _pointerLogic.DeviceUnitsFromMeasureUnits(_inputSource.Value, GetPosition(null));
					stylusOver = FindTarget(_inputSource.Value, position);
				}
				ChangeStylusOver(stylusOver);
			}
			else if (_inputSource != null && _inputSource.Value != null)
			{
				Point position2 = GetPosition(null);
				position2 = _pointerLogic.DeviceUnitsFromMeasureUnits(_inputSource.Value, position2);
				IInputElement stylusOver2 = StylusDevice.GlobalHitTest(_inputSource.Value, position2);
				ChangeStylusOver(stylusOver2);
			}
		}
		if (Mouse.Captured != _stylusCapture || Mouse.CapturedMode != _captureMode)
		{
			Mouse.Capture(_stylusCapture, _captureMode);
		}
	}

	internal override void UpdateEventStylusPoints(RawStylusInputReport report, bool resetIfNoOverride)
	{
		if (report.RawStylusInput != null && report.RawStylusInput.StylusPointsModified)
		{
			GeneralTransform inverse = report.RawStylusInput.Target.ViewToElement.Inverse;
			_currentStylusPoints = report.RawStylusInput.GetStylusPoints(inverse);
		}
		else if (resetIfNoOverride)
		{
			_currentStylusPoints = new StylusPointCollection(report.StylusPointDescription, report.GetRawPacketData(), GetTabletToElementTransform(null), Matrix.Identity);
		}
	}

	internal GeneralTransform GetTabletToElementTransform(IInputElement relativeTo)
	{
		GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
		Matrix transformToDevice = _inputSource.Value.CompositionTarget.TransformToDevice;
		transformToDevice.Invert();
		generalTransformGroup.Children.Add(new MatrixTransform(PointerTabletDevice.TabletToScreen * transformToDevice));
		generalTransformGroup.Children.Add(StylusDevice.GetElementTransform(relativeTo));
		return generalTransformGroup;
	}
}
