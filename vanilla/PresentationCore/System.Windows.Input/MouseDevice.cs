using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input.StylusPointer;
using System.Windows.Interop;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace System.Windows.Input;

/// <summary>Represents a mouse device. </summary>
public abstract class MouseDevice : InputDevice
{
	private SecurityCriticalDataClass<PresentationSource> _inputSource;

	private SecurityCriticalData<InputManager> _inputManager;

	private IInputElement _mouseOver;

	private DeferredElementTreeState _mouseOverTreeState;

	private bool _isPhysicallyOver;

	private WeakReference _rawMouseOver;

	private IInputElement _mouseCapture;

	private DeferredElementTreeState _mouseCaptureWithinTreeState;

	private SecurityCriticalDataClass<IMouseInputProvider> _providerCapture;

	private CaptureMode _captureMode;

	private bool _isCaptureMouseInProgress;

	private DependencyPropertyChangedEventHandler _overIsEnabledChangedEventHandler;

	private DependencyPropertyChangedEventHandler _overIsVisibleChangedEventHandler;

	private DependencyPropertyChangedEventHandler _overIsHitTestVisibleChangedEventHandler;

	private DispatcherOperationCallback _reevaluateMouseOverDelegate;

	private DispatcherOperation _reevaluateMouseOverOperation;

	private DependencyPropertyChangedEventHandler _captureIsEnabledChangedEventHandler;

	private DependencyPropertyChangedEventHandler _captureIsVisibleChangedEventHandler;

	private DependencyPropertyChangedEventHandler _captureIsHitTestVisibleChangedEventHandler;

	private DispatcherOperationCallback _reevaluateCaptureDelegate;

	private DispatcherOperation _reevaluateCaptureOperation;

	private Point _positionRelativeToOver;

	private Point _lastPosition;

	private bool _forceUpdateLastPosition;

	private object _tagNonRedundantActions = new object();

	private object _tagStylusDevice = new object();

	private object _tagRootPoint = new object();

	private Point _lastClick;

	private MouseButton _lastButton;

	private int _clickCount;

	private int _lastClickTime;

	private int _doubleClickDeltaTime;

	private int _doubleClickDeltaX;

	private int _doubleClickDeltaY;

	private Cursor _overrideCursor;

	private StylusDevice _stylusDevice;

	/// <summary>Gets the <see cref="T:System.Windows.IInputElement" /> that the input from this mouse device is sent to. </summary>
	/// <returns>The element that receives the input.</returns>
	public override IInputElement Target => _mouseOver;

	/// <summary>Gets the <see cref="T:System.Windows.PresentationSource" /> that is reporting input for this device.</summary>
	/// <returns>The source of input for this device.</returns>
	public override PresentationSource ActiveSource
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

	internal PresentationSource CriticalActiveSource
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

	/// <summary>Gets the element that the mouse pointer is directly over. </summary>
	/// <returns>The element the mouse pointer is over.</returns>
	public IInputElement DirectlyOver => _mouseOver;

	[FriendAccessAllowed]
	internal IInputElement RawDirectlyOver
	{
		get
		{
			if (_rawMouseOver != null)
			{
				IInputElement inputElement = (IInputElement)_rawMouseOver.Target;
				if (inputElement != null)
				{
					return inputElement;
				}
			}
			return DirectlyOver;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.IInputElement" /> that is captured by the mouse. </summary>
	/// <returns>The element which is captured by the mouse.</returns>
	public IInputElement Captured
	{
		get
		{
			if (_isCaptureMouseInProgress)
			{
				return null;
			}
			return _mouseCapture;
		}
	}

	internal CaptureMode CapturedMode => _captureMode;

	/// <summary>Gets or sets the cursor for the entire application. </summary>
	/// <returns>The override cursor or null if <see cref="P:System.Windows.Input.MouseDevice.OverrideCursor" /> is not set.</returns>
	public Cursor OverrideCursor
	{
		get
		{
			return _overrideCursor;
		}
		set
		{
			_overrideCursor = value;
			UpdateCursorPrivate();
		}
	}

	/// <summary>Gets the state of the left mouse button of this mouse device. </summary>
	/// <returns>The state of the button.</returns>
	public MouseButtonState LeftButton => GetButtonState(MouseButton.Left);

	/// <summary>Gets the state of the right button of this mouse device. </summary>
	/// <returns>The state of the button.</returns>
	public MouseButtonState RightButton => GetButtonState(MouseButton.Right);

	/// <summary> The state of the middle button of this mouse device. </summary>
	/// <returns>The state of the button.</returns>
	public MouseButtonState MiddleButton => GetButtonState(MouseButton.Middle);

	/// <summary>Gets the state of the first extended button on this mouse device. </summary>
	/// <returns>The state of the button.</returns>
	public MouseButtonState XButton1 => GetButtonState(MouseButton.XButton1);

	/// <summary>Gets the state of the second extended button of this mouse device. </summary>
	/// <returns>The state of the button.</returns>
	public MouseButtonState XButton2 => GetButtonState(MouseButton.XButton2);

	internal Point PositionRelativeToOver => _positionRelativeToOver;

	internal Point NonRelativePosition => _lastPosition;

	internal bool IsActive
	{
		get
		{
			if (_inputSource != null)
			{
				return _inputSource.Value != null;
			}
			return false;
		}
	}

	internal StylusDevice StylusDevice => _stylusDevice;

	private DeferredElementTreeState MouseOverTreeState
	{
		get
		{
			if (_mouseOverTreeState == null)
			{
				_mouseOverTreeState = new DeferredElementTreeState();
			}
			return _mouseOverTreeState;
		}
	}

	private DeferredElementTreeState MouseCaptureWithinTreeState
	{
		get
		{
			if (_mouseCaptureWithinTreeState == null)
			{
				_mouseCaptureWithinTreeState = new DeferredElementTreeState();
			}
			return _mouseCaptureWithinTreeState;
		}
	}

	internal MouseDevice(InputManager inputManager)
	{
		_inputManager = new SecurityCriticalData<InputManager>(inputManager);
		_inputManager.Value.PreProcessInput += PreProcessInput;
		_inputManager.Value.PreNotifyInput += PreNotifyInput;
		_inputManager.Value.PostProcessInput += PostProcessInput;
		_doubleClickDeltaX = SafeSystemMetrics.DoubleClickDeltaX;
		_doubleClickDeltaY = SafeSystemMetrics.DoubleClickDeltaY;
		_doubleClickDeltaTime = SafeNativeMethods.GetDoubleClickTime();
		_overIsEnabledChangedEventHandler = OnOverIsEnabledChanged;
		_overIsVisibleChangedEventHandler = OnOverIsVisibleChanged;
		_overIsHitTestVisibleChangedEventHandler = OnOverIsHitTestVisibleChanged;
		_reevaluateMouseOverDelegate = ReevaluateMouseOverAsync;
		_reevaluateMouseOverOperation = null;
		_captureIsEnabledChangedEventHandler = OnCaptureIsEnabledChanged;
		_captureIsVisibleChangedEventHandler = OnCaptureIsVisibleChanged;
		_captureIsHitTestVisibleChangedEventHandler = OnCaptureIsHitTestVisibleChanged;
		_reevaluateCaptureDelegate = ReevaluateCaptureAsync;
		_reevaluateCaptureOperation = null;
		_inputManager.Value.HitTestInvalidatedAsync += OnHitTestInvalidatedAsync;
	}

	/// <summary>Gets the state of the specified mouse button.</summary>
	/// <returns>The state of the button.</returns>
	/// <param name="mouseButton">The button which is being queried.</param>
	protected MouseButtonState GetButtonState(MouseButton mouseButton)
	{
		if (_stylusDevice != null && _stylusDevice.IsValid)
		{
			return _stylusDevice.GetMouseButtonState(mouseButton, this);
		}
		return GetButtonStateFromSystem(mouseButton);
	}

	/// <summary>Calculates the screen position of the mouse pointer.</summary>
	/// <returns>The position of the mouse pointer.</returns>
	protected Point GetScreenPosition()
	{
		if (_stylusDevice != null)
		{
			return _stylusDevice.GetMouseScreenPosition(this);
		}
		return GetScreenPositionFromSystem();
	}

	internal abstract MouseButtonState GetButtonStateFromSystem(MouseButton mouseButton);

	internal Point GetScreenPositionFromSystem()
	{
		Point result = new Point(0.0, 0.0);
		if (IsActive)
		{
			try
			{
				PresentationSource criticalActiveSource = CriticalActiveSource;
				if (criticalActiveSource != null)
				{
					return PointUtil.ClientToScreen(_lastPosition, criticalActiveSource);
				}
			}
			catch (Win32Exception)
			{
				result = new Point(0.0, 0.0);
			}
		}
		return result;
	}

	/// <summary>Calculates the position of the mouse pointer, in client coordinates.</summary>
	/// <returns>The position of the mouse pointer, in client coordinates.</returns>
	protected Point GetClientPosition()
	{
		Point result = new Point(0.0, 0.0);
		try
		{
			PresentationSource criticalActiveSource = CriticalActiveSource;
			if (criticalActiveSource != null)
			{
				return GetClientPosition(criticalActiveSource);
			}
		}
		catch (Win32Exception)
		{
			result = new Point(0.0, 0.0);
		}
		return result;
	}

	/// <summary>Calculates the position of the mouse pointer, in client coordinates, in the specified <see cref="T:System.Windows.PresentationSource" />.</summary>
	/// <returns>The position of the mouse pointer, in client coordinates, in the specified <see cref="T:System.Windows.PresentationSource" />.</returns>
	/// <param name="presentationSource">The source in which to obtain the mouse position.</param>
	protected Point GetClientPosition(PresentationSource presentationSource)
	{
		return PointUtil.ScreenToClient(GetScreenPosition(), presentationSource);
	}

	/// <summary>Captures mouse events to the specified element. </summary>
	/// <returns>true if the element was able to capture the mouse; otherwise, false.</returns>
	/// <param name="element">The element to capture the mouse.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="element" /> is not a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />.</exception>
	public bool Capture(IInputElement element)
	{
		return Capture(element, CaptureMode.Element);
	}

	/// <summary>Captures mouse input to the specified element using the specified <see cref="T:System.Windows.Input.CaptureMode" />.</summary>
	/// <returns>true if the element was able to capture the mouse; otherwise, false.</returns>
	/// <param name="element">The element to capture the mouse..</param>
	/// <param name="captureMode">The capture policy to use.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="element" /> is not a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="captureMode" /> is not a valid <see cref="T:System.Windows.Input.CaptureMode" />.</exception>
	public bool Capture(IInputElement element, CaptureMode captureMode)
	{
		int tickCount = Environment.TickCount;
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
		bool flag = false;
		if (element is UIElement)
		{
			UIElement uIElement = element as UIElement;
			if (uIElement.IsVisible && uIElement.IsEnabled)
			{
				flag = true;
			}
		}
		else if (element is ContentElement)
		{
			if ((element as ContentElement).IsEnabled)
			{
				flag = true;
			}
		}
		else if (element is UIElement3D)
		{
			UIElement3D uIElement3D = element as UIElement3D;
			if (uIElement3D.IsVisible && uIElement3D.IsEnabled)
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
			flag = false;
			IMouseInputProvider mouseInputProvider = null;
			if (element != null)
			{
				DependencyObject containingVisual = InputElement.GetContainingVisual(dependencyObject);
				if (containingVisual != null)
				{
					PresentationSource presentationSource = PresentationSource.CriticalFromVisual(containingVisual);
					if (presentationSource != null)
					{
						mouseInputProvider = presentationSource.GetInputProvider(typeof(MouseDevice)) as IMouseInputProvider;
					}
				}
			}
			else if (_mouseCapture != null)
			{
				mouseInputProvider = _providerCapture.Value;
			}
			if (mouseInputProvider != null)
			{
				if (element != null)
				{
					bool isCaptureMouseInProgress = _isCaptureMouseInProgress;
					_isCaptureMouseInProgress = true;
					flag = mouseInputProvider.CaptureMouse();
					_isCaptureMouseInProgress = isCaptureMouseInProgress;
					if (flag)
					{
						ChangeMouseCapture(element, mouseInputProvider, captureMode, tickCount);
					}
				}
				else
				{
					mouseInputProvider.ReleaseMouseCapture();
					flag = true;
				}
			}
		}
		return flag;
	}

	private IMouseInputProvider FindMouseInputProviderForCursor()
	{
		IMouseInputProvider result = null;
		IEnumerator enumerator = _inputManager.Value.UnsecureInputProviders.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is IMouseInputProvider mouseInputProvider)
			{
				result = mouseInputProvider;
				break;
			}
		}
		return result;
	}

	/// <summary>Sets the mouse pointer to the specified <see cref="T:System.Windows.Input.Cursor" /></summary>
	/// <returns>true if the mouse cursor is set; otherwise, false.</returns>
	/// <param name="cursor">The cursor to set the mouse pointer to.</param>
	public bool SetCursor(Cursor cursor)
	{
		if (_overrideCursor != null)
		{
			cursor = _overrideCursor;
		}
		if (cursor == null)
		{
			cursor = Cursors.None;
		}
		return FindMouseInputProviderForCursor()?.SetCursor(cursor) ?? false;
	}

	/// <summary>Gets the position of the mouse relative to a specified element.</summary>
	/// <returns>The position of the mouse relative to the parameter <paramref name="relativeTo" />.</returns>
	/// <param name="relativeTo">The frame of reference in which to calculate the position of the mouse.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="relativeTo" /> is null or is not a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />. </exception>
	public Point GetPosition(IInputElement relativeTo)
	{
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
		bool success;
		Point pt = PointUtil.TryClientToRoot(GetClientPosition(presentationSource), presentationSource, throwOnError: false, out success);
		if (!success)
		{
			return new Point(0.0, 0.0);
		}
		return InputElement.TranslatePoint(pt, presentationSource.RootVisual, (DependencyObject)relativeTo);
	}

	internal void ReevaluateMouseOver(DependencyObject element, DependencyObject oldParent, bool isCoreParent)
	{
		if (element != null)
		{
			if (isCoreParent)
			{
				MouseOverTreeState.SetCoreParent(element, oldParent);
			}
			else
			{
				MouseOverTreeState.SetLogicalParent(element, oldParent);
			}
		}
		if (_reevaluateMouseOverOperation == null)
		{
			_reevaluateMouseOverOperation = base.Dispatcher.BeginInvoke(DispatcherPriority.Input, _reevaluateMouseOverDelegate, null);
		}
	}

	private object ReevaluateMouseOverAsync(object arg)
	{
		_reevaluateMouseOverOperation = null;
		Synchronize();
		if (_mouseOverTreeState != null && !_mouseOverTreeState.IsEmpty)
		{
			UIElement.MouseOverProperty.OnOriginValueChanged(_mouseOver as DependencyObject, _mouseOver as DependencyObject, ref _mouseOverTreeState);
		}
		return null;
	}

	internal void ReevaluateCapture(DependencyObject element, DependencyObject oldParent, bool isCoreParent)
	{
		if (element != null)
		{
			if (isCoreParent)
			{
				MouseCaptureWithinTreeState.SetCoreParent(element, oldParent);
			}
			else
			{
				MouseCaptureWithinTreeState.SetLogicalParent(element, oldParent);
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
		if (_mouseCapture == null)
		{
			return null;
		}
		bool flag = false;
		DependencyObject dependencyObject = _mouseCapture as DependencyObject;
		if (dependencyObject is UIElement element)
		{
			flag = !ValidateUIElementForCapture(element);
		}
		else if (dependencyObject is ContentElement element2)
		{
			flag = !ValidateContentElementForCapture(element2);
		}
		else if (dependencyObject is UIElement3D element3)
		{
			flag = !ValidateUIElement3DForCapture(element3);
		}
		if (!flag)
		{
			DependencyObject containingVisual = InputElement.GetContainingVisual(dependencyObject);
			flag = !ValidateVisualForCapture(containingVisual);
		}
		if (flag)
		{
			Capture(null);
		}
		if (_mouseCaptureWithinTreeState != null && !_mouseCaptureWithinTreeState.IsEmpty)
		{
			UIElement.MouseCaptureWithinProperty.OnOriginValueChanged(_mouseCapture as DependencyObject, _mouseCapture as DependencyObject, ref _mouseCaptureWithinTreeState);
		}
		return null;
	}

	private bool ValidateUIElementForCapture(UIElement element)
	{
		if (!element.IsEnabled)
		{
			return false;
		}
		if (!element.IsVisible)
		{
			return false;
		}
		if (!element.IsHitTestVisible)
		{
			return false;
		}
		return true;
	}

	private bool ValidateUIElement3DForCapture(UIElement3D element)
	{
		if (!element.IsEnabled)
		{
			return false;
		}
		if (!element.IsVisible)
		{
			return false;
		}
		if (!element.IsHitTestVisible)
		{
			return false;
		}
		return true;
	}

	private bool ValidateContentElementForCapture(ContentElement element)
	{
		if (!element.IsEnabled)
		{
			return false;
		}
		return true;
	}

	private bool ValidateVisualForCapture(DependencyObject visual)
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
		if (presentationSource != CriticalActiveSource)
		{
			return false;
		}
		return true;
	}

	private void OnOverIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		ReevaluateMouseOver(null, null, isCoreParent: true);
	}

	private void OnOverIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		ReevaluateMouseOver(null, null, isCoreParent: true);
	}

	private void OnOverIsHitTestVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		ReevaluateMouseOver(null, null, isCoreParent: true);
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

	private void OnHitTestInvalidatedAsync(object sender, EventArgs e)
	{
		Synchronize();
	}

	/// <summary>Forces the mouse to resynchronize. </summary>
	public void Synchronize()
	{
		PresentationSource criticalActiveSource = CriticalActiveSource;
		if (criticalActiveSource != null && criticalActiveSource.CompositionTarget != null && !criticalActiveSource.CompositionTarget.IsDisposed)
		{
			int tickCount = Environment.TickCount;
			Point clientPosition = GetClientPosition();
			RawMouseInputReport rawMouseInputReport = new RawMouseInputReport(InputMode.Foreground, tickCount, criticalActiveSource, RawMouseActions.AbsoluteMove, (int)clientPosition.X, (int)clientPosition.Y, 0, IntPtr.Zero);
			rawMouseInputReport._isSynchronize = true;
			InputReportEventArgs inputReportEventArgs = ((_stylusDevice == null) ? new InputReportEventArgs(this, rawMouseInputReport) : new InputReportEventArgs(_stylusDevice, rawMouseInputReport));
			inputReportEventArgs.RoutedEvent = InputManager.PreviewInputReportEvent;
			_inputManager.Value.ProcessInput(inputReportEventArgs);
		}
	}

	/// <summary>Forces the mouse cursor to update. </summary>
	public void UpdateCursor()
	{
		UpdateCursorPrivate();
	}

	private bool UpdateCursorPrivate()
	{
		int tickCount = Environment.TickCount;
		QueryCursorEventArgs queryCursorEventArgs = new QueryCursorEventArgs(this, tickCount);
		queryCursorEventArgs.Cursor = Cursors.Arrow;
		queryCursorEventArgs.RoutedEvent = Mouse.QueryCursorEvent;
		_inputManager.Value.ProcessInput(queryCursorEventArgs);
		return queryCursorEventArgs.Handled;
	}

	private void ChangeMouseOver(IInputElement mouseOver, int timestamp)
	{
		DependencyObject dependencyObject = null;
		if (_mouseOver == mouseOver)
		{
			return;
		}
		IInputElement mouseOver2 = _mouseOver;
		_mouseOver = mouseOver;
		using (base.Dispatcher.DisableProcessing())
		{
			if (mouseOver2 != null)
			{
				dependencyObject = mouseOver2 as DependencyObject;
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
				else if (dependencyObject is UIElement3D uIElement3D)
				{
					uIElement3D.IsEnabledChanged -= _overIsEnabledChangedEventHandler;
					uIElement3D.IsVisibleChanged -= _overIsVisibleChangedEventHandler;
					uIElement3D.IsHitTestVisibleChanged -= _overIsHitTestVisibleChangedEventHandler;
				}
			}
			if (_mouseOver != null)
			{
				dependencyObject = _mouseOver as DependencyObject;
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
				else if (dependencyObject is UIElement3D uIElement3D2)
				{
					uIElement3D2.IsEnabledChanged += _overIsEnabledChangedEventHandler;
					uIElement3D2.IsVisibleChanged += _overIsVisibleChangedEventHandler;
					uIElement3D2.IsHitTestVisibleChanged += _overIsHitTestVisibleChangedEventHandler;
				}
			}
		}
		UIElement.MouseOverProperty.OnOriginValueChanged(mouseOver2 as DependencyObject, _mouseOver as DependencyObject, ref _mouseOverTreeState);
		if (mouseOver2 != null)
		{
			dependencyObject = mouseOver2 as DependencyObject;
			dependencyObject.SetValue(UIElement.IsMouseDirectlyOverPropertyKey, value: false);
		}
		if (_mouseOver != null)
		{
			dependencyObject = _mouseOver as DependencyObject;
			dependencyObject.SetValue(UIElement.IsMouseDirectlyOverPropertyKey, value: true);
		}
	}

	private void ChangeMouseCapture(IInputElement mouseCapture, IMouseInputProvider providerCapture, CaptureMode captureMode, int timestamp)
	{
		DependencyObject dependencyObject = null;
		if (mouseCapture == _mouseCapture)
		{
			return;
		}
		IInputElement mouseCapture2 = _mouseCapture;
		_mouseCapture = mouseCapture;
		if (_mouseCapture != null)
		{
			_providerCapture = new SecurityCriticalDataClass<IMouseInputProvider>(providerCapture);
		}
		else
		{
			_providerCapture = null;
		}
		_captureMode = captureMode;
		using (base.Dispatcher.DisableProcessing())
		{
			if (mouseCapture2 != null)
			{
				dependencyObject = mouseCapture2 as DependencyObject;
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
				else if (dependencyObject is UIElement3D uIElement3D)
				{
					uIElement3D.IsEnabledChanged -= _captureIsEnabledChangedEventHandler;
					uIElement3D.IsVisibleChanged -= _captureIsVisibleChangedEventHandler;
					uIElement3D.IsHitTestVisibleChanged -= _captureIsHitTestVisibleChangedEventHandler;
				}
			}
			if (_mouseCapture != null)
			{
				dependencyObject = _mouseCapture as DependencyObject;
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
				else if (dependencyObject is UIElement3D uIElement3D2)
				{
					uIElement3D2.IsEnabledChanged += _captureIsEnabledChangedEventHandler;
					uIElement3D2.IsVisibleChanged += _captureIsVisibleChangedEventHandler;
					uIElement3D2.IsHitTestVisibleChanged += _captureIsHitTestVisibleChangedEventHandler;
				}
			}
		}
		UIElement.MouseCaptureWithinProperty.OnOriginValueChanged(mouseCapture2 as DependencyObject, _mouseCapture as DependencyObject, ref _mouseCaptureWithinTreeState);
		if (mouseCapture2 != null)
		{
			dependencyObject = mouseCapture2 as DependencyObject;
			dependencyObject.SetValue(UIElement.IsMouseCapturedPropertyKey, value: false);
		}
		if (_mouseCapture != null)
		{
			dependencyObject = _mouseCapture as DependencyObject;
			dependencyObject.SetValue(UIElement.IsMouseCapturedPropertyKey, value: true);
		}
		if (mouseCapture2 != null)
		{
			MouseEventArgs mouseEventArgs = new MouseEventArgs(this, timestamp, _stylusDevice);
			mouseEventArgs.RoutedEvent = Mouse.LostMouseCaptureEvent;
			mouseEventArgs.Source = mouseCapture2;
			_inputManager.Value.ProcessInput(mouseEventArgs);
		}
		if (_mouseCapture != null)
		{
			MouseEventArgs mouseEventArgs2 = new MouseEventArgs(this, timestamp, _stylusDevice);
			mouseEventArgs2.RoutedEvent = Mouse.GotMouseCaptureEvent;
			mouseEventArgs2.Source = _mouseCapture;
			_inputManager.Value.ProcessInput(mouseEventArgs2);
		}
		Synchronize();
	}

	private void PreProcessInput(object sender, PreProcessInputEventArgs e)
	{
		if (e.StagingItem.Input.RoutedEvent == InputManager.PreviewInputReportEvent)
		{
			InputReportEventArgs inputReportEventArgs = e.StagingItem.Input as InputReportEventArgs;
			if (inputReportEventArgs.Handled || inputReportEventArgs.Report.Type != InputType.Mouse)
			{
				return;
			}
			RawMouseInputReport rawMouseInputReport = (RawMouseInputReport)inputReportEventArgs.Report;
			if ((rawMouseInputReport.Actions & RawMouseActions.Activate) == RawMouseActions.Activate)
			{
				if ((rawMouseInputReport.Actions & ~RawMouseActions.Activate) != 0)
				{
					e.Cancel();
					RawMouseInputReport report = new RawMouseInputReport(rawMouseInputReport.Mode, rawMouseInputReport.Timestamp, rawMouseInputReport.InputSource, rawMouseInputReport.Actions & ~RawMouseActions.Activate, rawMouseInputReport.X, rawMouseInputReport.Y, rawMouseInputReport.Wheel, rawMouseInputReport.ExtraInformation);
					InputReportEventArgs inputReportEventArgs2 = new InputReportEventArgs(inputReportEventArgs.Device, report);
					inputReportEventArgs2.RoutedEvent = InputManager.PreviewInputReportEvent;
					e.PushInput(inputReportEventArgs2, null);
					PushActivateInputReport(e, inputReportEventArgs, rawMouseInputReport, clearExtraInformation: false);
				}
			}
			else
			{
				if (_inputSource == null || rawMouseInputReport.InputSource != _inputSource.Value)
				{
					return;
				}
				InputDevice inputDevice = e.StagingItem.GetData(_tagStylusDevice) as StylusDevice;
				if (inputDevice == null)
				{
					if (StylusLogic.IsPointerStackEnabled && StylusLogic.IsPromotedMouseEvent(rawMouseInputReport))
					{
						uint cursorIdFromMouseEvent = StylusLogic.GetCursorIdFromMouseEvent(rawMouseInputReport);
						inputDevice = Tablet.TabletDevices.As<PointerTabletDeviceCollection>().GetStylusDeviceByCursorId(cursorIdFromMouseEvent)?.StylusDevice;
					}
					else
					{
						inputDevice = inputReportEventArgs.Device as StylusDevice;
					}
					if (inputDevice != null)
					{
						e.StagingItem.SetData(_tagStylusDevice, inputDevice);
					}
				}
				inputReportEventArgs.Device = this;
				if ((rawMouseInputReport.Actions & RawMouseActions.Deactivate) == RawMouseActions.Deactivate && _mouseOver != null)
				{
					e.PushInput(e.StagingItem);
					e.Cancel();
					_isPhysicallyOver = false;
					ChangeMouseOver(null, e.StagingItem.Input.Timestamp);
				}
				if ((rawMouseInputReport.Actions & RawMouseActions.AbsoluteMove) != RawMouseActions.AbsoluteMove)
				{
					return;
				}
				if ((rawMouseInputReport.Actions & ~(RawMouseActions.AbsoluteMove | RawMouseActions.QueryCursor)) != 0)
				{
					e.Cancel();
					RawMouseInputReport report2 = new RawMouseInputReport(rawMouseInputReport.Mode, rawMouseInputReport.Timestamp, rawMouseInputReport.InputSource, rawMouseInputReport.Actions & ~(RawMouseActions.AbsoluteMove | RawMouseActions.QueryCursor), 0, 0, rawMouseInputReport.Wheel, rawMouseInputReport.ExtraInformation);
					InputReportEventArgs inputReportEventArgs3 = new InputReportEventArgs(inputDevice, report2);
					inputReportEventArgs3.RoutedEvent = InputManager.PreviewInputReportEvent;
					e.PushInput(inputReportEventArgs3, null);
					RawMouseInputReport report3 = new RawMouseInputReport(rawMouseInputReport.Mode, rawMouseInputReport.Timestamp, rawMouseInputReport.InputSource, rawMouseInputReport.Actions & (RawMouseActions.AbsoluteMove | RawMouseActions.QueryCursor), rawMouseInputReport.X, rawMouseInputReport.Y, 0, IntPtr.Zero);
					InputReportEventArgs inputReportEventArgs4 = new InputReportEventArgs(inputDevice, report3);
					inputReportEventArgs4.RoutedEvent = InputManager.PreviewInputReportEvent;
					e.PushInput(inputReportEventArgs4, null);
				}
				else
				{
					bool success = true;
					Point point = PointUtil.TryClientToRoot(new Point(rawMouseInputReport.X, rawMouseInputReport.Y), rawMouseInputReport.InputSource, throwOnError: false, out success);
					if (success)
					{
						e.StagingItem.SetData(_tagRootPoint, point);
					}
					else
					{
						e.Cancel();
					}
				}
			}
		}
		else
		{
			if (_inputSource == null)
			{
				return;
			}
			if (e.StagingItem.Input.RoutedEvent == Mouse.PreviewMouseDownEvent)
			{
				MouseButtonEventArgs mouseButtonEventArgs = e.StagingItem.Input as MouseButtonEventArgs;
				if (_mouseCapture != null && !_isPhysicallyOver)
				{
					MouseButtonEventArgs mouseButtonEventArgs2 = new MouseButtonEventArgs(this, mouseButtonEventArgs.Timestamp, mouseButtonEventArgs.ChangedButton, GetStylusDevice(e.StagingItem));
					mouseButtonEventArgs2.RoutedEvent = Mouse.PreviewMouseDownOutsideCapturedElementEvent;
					_inputManager.Value.ProcessInput(mouseButtonEventArgs2);
				}
			}
			else if (e.StagingItem.Input.RoutedEvent == Mouse.PreviewMouseUpEvent)
			{
				MouseButtonEventArgs mouseButtonEventArgs3 = e.StagingItem.Input as MouseButtonEventArgs;
				if (_mouseCapture != null && !_isPhysicallyOver)
				{
					MouseButtonEventArgs mouseButtonEventArgs4 = new MouseButtonEventArgs(this, mouseButtonEventArgs3.Timestamp, mouseButtonEventArgs3.ChangedButton, GetStylusDevice(e.StagingItem));
					mouseButtonEventArgs4.RoutedEvent = Mouse.PreviewMouseUpOutsideCapturedElementEvent;
					_inputManager.Value.ProcessInput(mouseButtonEventArgs4);
				}
			}
		}
	}

	internal static void PushActivateInputReport(PreProcessInputEventArgs e, InputReportEventArgs inputReportEventArgs, RawMouseInputReport rawMouseInputReport, bool clearExtraInformation)
	{
		nint extraInformation = (clearExtraInformation ? IntPtr.Zero : rawMouseInputReport.ExtraInformation);
		RawMouseInputReport report = new RawMouseInputReport(rawMouseInputReport.Mode, rawMouseInputReport.Timestamp, rawMouseInputReport.InputSource, RawMouseActions.Activate, rawMouseInputReport.X, rawMouseInputReport.Y, rawMouseInputReport.Wheel, extraInformation);
		InputReportEventArgs inputReportEventArgs2 = new InputReportEventArgs(inputReportEventArgs.Device, report);
		inputReportEventArgs2.RoutedEvent = InputManager.PreviewInputReportEvent;
		e.PushInput(inputReportEventArgs2, null);
	}

	private unsafe void PreNotifyInput(object sender, NotifyInputEventArgs e)
	{
		if (e.StagingItem.Input.RoutedEvent == InputManager.PreviewInputReportEvent)
		{
			InputReportEventArgs inputReportEventArgs = e.StagingItem.Input as InputReportEventArgs;
			if (inputReportEventArgs.Handled || inputReportEventArgs.Report.Type != InputType.Mouse)
			{
				return;
			}
			RawMouseInputReport rawMouseInputReport = (RawMouseInputReport)inputReportEventArgs.Report;
			RawMouseActions rawMouseActions = GetNonRedundantActions(e);
			RawMouseActions rawMouseActions2 = rawMouseActions;
			_stylusDevice = GetStylusDevice(e.StagingItem);
			if ((rawMouseInputReport.Actions & RawMouseActions.Activate) == RawMouseActions.Activate)
			{
				rawMouseActions |= RawMouseActions.Activate;
				_positionRelativeToOver.X = 0.0;
				_positionRelativeToOver.Y = 0.0;
				_lastPosition.X = rawMouseInputReport.X;
				_lastPosition.Y = rawMouseInputReport.Y;
				_forceUpdateLastPosition = true;
				_stylusDevice = inputReportEventArgs.Device as StylusDevice;
				if (_inputSource == null)
				{
					_inputSource = new SecurityCriticalDataClass<PresentationSource>(rawMouseInputReport.InputSource);
				}
				else if (_inputSource.Value != rawMouseInputReport.InputSource)
				{
					IMouseInputProvider mouseInputProvider = _inputSource.Value.GetInputProvider(typeof(MouseDevice)) as IMouseInputProvider;
					_inputSource = new SecurityCriticalDataClass<PresentationSource>(rawMouseInputReport.InputSource);
					mouseInputProvider?.NotifyDeactivate();
				}
			}
			if (_inputSource != null && rawMouseInputReport.InputSource == _inputSource.Value)
			{
				if ((rawMouseInputReport.Actions & RawMouseActions.Deactivate) == RawMouseActions.Deactivate)
				{
					_inputSource = null;
					ChangeMouseCapture(null, null, CaptureMode.None, e.StagingItem.Input.Timestamp);
				}
				if ((rawMouseInputReport.Actions & RawMouseActions.CancelCapture) == RawMouseActions.CancelCapture)
				{
					ChangeMouseCapture(null, null, CaptureMode.None, e.StagingItem.Input.Timestamp);
				}
				if ((rawMouseInputReport.Actions & RawMouseActions.AbsoluteMove) == RawMouseActions.AbsoluteMove)
				{
					bool translated = false;
					Point point = new Point(rawMouseInputReport.X, rawMouseInputReport.Y);
					Point pt = (Point)e.StagingItem.GetData(_tagRootPoint);
					Point point2 = InputElement.TranslatePoint(pt, rawMouseInputReport.InputSource.RootVisual, (DependencyObject)_mouseOver, out translated);
					IInputElement enabledHit = _mouseOver;
					IInputElement originalHit = ((_rawMouseOver != null) ? ((IInputElement)_rawMouseOver.Target) : null);
					bool flag = _isPhysicallyOver;
					bool flag2 = !ArePointsClose(point, _lastPosition);
					if (flag2 || rawMouseInputReport._isSynchronize || !translated)
					{
						flag = true;
						switch (_captureMode)
						{
						case CaptureMode.None:
							if (rawMouseInputReport._isSynchronize)
							{
								GlobalHitTest(clientUnits: true, point, _inputSource.Value, out enabledHit, out originalHit);
							}
							else
							{
								LocalHitTest(clientUnits: true, point, _inputSource.Value, out enabledHit, out originalHit);
							}
							if (enabledHit == originalHit)
							{
								originalHit = null;
							}
							if (!InputElement.IsValid(enabledHit))
							{
								enabledHit = InputElement.GetContainingInputElement(enabledHit as DependencyObject);
							}
							if (originalHit != null && !InputElement.IsValid(originalHit))
							{
								originalHit = InputElement.GetContainingInputElement(originalHit as DependencyObject);
							}
							break;
						case CaptureMode.Element:
							enabledHit = ((!rawMouseInputReport._isSynchronize) ? LocalHitTest(clientUnits: true, point, _inputSource.Value) : GlobalHitTest(clientUnits: true, point, _inputSource.Value));
							originalHit = null;
							if (enabledHit != _mouseCapture)
							{
								enabledHit = _mouseCapture;
								flag = false;
							}
							break;
						case CaptureMode.SubTree:
						{
							IInputElement containingInputElement = InputElement.GetContainingInputElement(_mouseCapture as DependencyObject);
							if (containingInputElement != null)
							{
								GlobalHitTest(clientUnits: true, point, _inputSource.Value, out enabledHit, out originalHit);
							}
							if (enabledHit != null && !InputElement.IsValid(enabledHit))
							{
								enabledHit = InputElement.GetContainingInputElement(enabledHit as DependencyObject);
							}
							if (enabledHit != null)
							{
								IInputElement inputElement = enabledHit;
								UIElement uIElement = null;
								ContentElement contentElement = null;
								while (inputElement != null && inputElement != containingInputElement)
								{
									inputElement = ((inputElement is UIElement uIElement2) ? InputElement.GetContainingInputElement(uIElement2.GetUIParent(continuePastVisualTree: true)) : ((!(inputElement is ContentElement contentElement2)) ? InputElement.GetContainingInputElement((inputElement as UIElement3D).GetUIParent(continuePastVisualTree: true)) : InputElement.GetContainingInputElement(contentElement2.GetUIParent(continuePastVisualTree: true))));
								}
								if (inputElement != containingInputElement)
								{
									enabledHit = _mouseCapture;
									flag = false;
									originalHit = null;
								}
							}
							else
							{
								enabledHit = _mouseCapture;
								flag = false;
								originalHit = null;
							}
							if (originalHit != null)
							{
								if (enabledHit == originalHit)
								{
									originalHit = null;
								}
								else if (!InputElement.IsValid(originalHit))
								{
									originalHit = InputElement.GetContainingInputElement(originalHit as DependencyObject);
								}
							}
							break;
						}
						}
					}
					_isPhysicallyOver = enabledHit != null && flag;
					bool flag3 = enabledHit != _mouseOver;
					if (flag3)
					{
						point2 = InputElement.TranslatePoint(pt, rawMouseInputReport.InputSource.RootVisual, (DependencyObject)enabledHit);
					}
					bool flag4 = flag3 || !ArePointsClose(point2, _positionRelativeToOver);
					if (flag2 || flag4 || _forceUpdateLastPosition)
					{
						_forceUpdateLastPosition = false;
						_lastPosition = point;
						_positionRelativeToOver = point2;
						if (flag3)
						{
							ChangeMouseOver(enabledHit, e.StagingItem.Input.Timestamp);
						}
						if (_rawMouseOver == null && originalHit != null)
						{
							_rawMouseOver = new WeakReference(originalHit);
						}
						else if (_rawMouseOver != null)
						{
							_rawMouseOver.Target = originalHit;
						}
						rawMouseActions |= RawMouseActions.AbsoluteMove;
						rawMouseActions |= RawMouseActions.QueryCursor;
					}
				}
				if ((rawMouseInputReport.Actions & RawMouseActions.VerticalWheelRotate) == RawMouseActions.VerticalWheelRotate)
				{
					rawMouseActions |= RawMouseActions.VerticalWheelRotate;
					_inputManager.Value.MostRecentInputDevice = this;
				}
				if ((rawMouseInputReport.Actions & RawMouseActions.QueryCursor) == RawMouseActions.QueryCursor)
				{
					rawMouseActions |= RawMouseActions.QueryCursor;
				}
				byte* intPtr = stackalloc byte[20];
				ReadOnlySpan<RawMouseActions> readOnlySpan = RuntimeHelpers.CreateSpan<RawMouseActions>((RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
				// IL cpblk instruction
				Unsafe.CopyBlockUnaligned(intPtr, ref readOnlySpan[0], 20);
				ReadOnlySpan<RawMouseActions> readOnlySpan2 = new Span<RawMouseActions>(intPtr, 5);
				byte* intPtr2 = stackalloc byte[20];
				readOnlySpan = RuntimeHelpers.CreateSpan<RawMouseActions>((RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
				// IL cpblk instruction
				Unsafe.CopyBlockUnaligned(intPtr2, ref readOnlySpan[0], 20);
				ReadOnlySpan<RawMouseActions> readOnlySpan3 = new Span<RawMouseActions>(intPtr2, 5);
				for (int i = 0; i < 5; i++)
				{
					if ((rawMouseInputReport.Actions & readOnlySpan2[i]) == readOnlySpan2[i])
					{
						rawMouseActions |= readOnlySpan2[i];
						_inputManager.Value.MostRecentInputDevice = this;
					}
					if ((rawMouseInputReport.Actions & readOnlySpan3[i]) == readOnlySpan3[i])
					{
						rawMouseActions |= readOnlySpan3[i];
						_inputManager.Value.MostRecentInputDevice = this;
					}
				}
			}
			if (rawMouseActions != rawMouseActions2)
			{
				e.StagingItem.SetData(_tagNonRedundantActions, rawMouseActions);
			}
		}
		else if (_inputSource != null && e.StagingItem.Input.RoutedEvent == Mouse.PreviewMouseDownEvent)
		{
			MouseButtonEventArgs mouseButtonEventArgs = e.StagingItem.Input as MouseButtonEventArgs;
			StylusDevice stylusDevice = GetStylusDevice(e.StagingItem);
			Point clientPosition = GetClientPosition();
			_clickCount = CalculateClickCount(mouseButtonEventArgs.ChangedButton, mouseButtonEventArgs.Timestamp, stylusDevice, clientPosition);
			if (_clickCount == 1)
			{
				_lastClick = clientPosition;
				_lastButton = mouseButtonEventArgs.ChangedButton;
				_lastClickTime = mouseButtonEventArgs.Timestamp;
			}
			mouseButtonEventArgs.ClickCount = _clickCount;
		}
	}

	private bool ArePointsClose(Point A, Point B)
	{
		if (DoubleUtil.AreClose(A.X, B.X))
		{
			return DoubleUtil.AreClose(A.Y, B.Y);
		}
		return false;
	}

	private void PostProcessInput(object sender, ProcessInputEventArgs e)
	{
		if (e.StagingItem.Input.RoutedEvent == Mouse.PreviewMouseWheelEvent && !e.StagingItem.Input.Handled)
		{
			MouseWheelEventArgs mouseWheelEventArgs = (MouseWheelEventArgs)e.StagingItem.Input;
			MouseWheelEventArgs mouseWheelEventArgs2 = new MouseWheelEventArgs(this, mouseWheelEventArgs.Timestamp, mouseWheelEventArgs.Delta);
			mouseWheelEventArgs2.RoutedEvent = Mouse.MouseWheelEvent;
			e.PushInput(mouseWheelEventArgs2, e.StagingItem);
		}
		if (e.StagingItem.Input.RoutedEvent == Mouse.PreviewMouseDownEvent && !e.StagingItem.Input.Handled)
		{
			MouseButtonEventArgs mouseButtonEventArgs = (MouseButtonEventArgs)e.StagingItem.Input;
			MouseButtonEventArgs mouseButtonEventArgs2 = new MouseButtonEventArgs(this, mouseButtonEventArgs.Timestamp, mouseButtonEventArgs.ChangedButton, GetStylusDevice(e.StagingItem));
			mouseButtonEventArgs2.ClickCount = mouseButtonEventArgs.ClickCount;
			mouseButtonEventArgs2.RoutedEvent = Mouse.MouseDownEvent;
			e.PushInput(mouseButtonEventArgs2, e.StagingItem);
		}
		if (e.StagingItem.Input.RoutedEvent == Mouse.PreviewMouseUpEvent && !e.StagingItem.Input.Handled)
		{
			MouseButtonEventArgs mouseButtonEventArgs3 = (MouseButtonEventArgs)e.StagingItem.Input;
			MouseButtonEventArgs mouseButtonEventArgs4 = new MouseButtonEventArgs(this, mouseButtonEventArgs3.Timestamp, mouseButtonEventArgs3.ChangedButton, GetStylusDevice(e.StagingItem));
			mouseButtonEventArgs4.RoutedEvent = Mouse.MouseUpEvent;
			e.PushInput(mouseButtonEventArgs4, e.StagingItem);
		}
		if (e.StagingItem.Input.RoutedEvent == Mouse.PreviewMouseMoveEvent && !e.StagingItem.Input.Handled)
		{
			MouseEventArgs mouseEventArgs = (MouseEventArgs)e.StagingItem.Input;
			MouseEventArgs mouseEventArgs2 = new MouseEventArgs(this, mouseEventArgs.Timestamp, GetStylusDevice(e.StagingItem));
			mouseEventArgs2.RoutedEvent = Mouse.MouseMoveEvent;
			e.PushInput(mouseEventArgs2, e.StagingItem);
		}
		if (e.StagingItem.Input.RoutedEvent == Mouse.QueryCursorEvent)
		{
			QueryCursorEventArgs queryCursorEventArgs = (QueryCursorEventArgs)e.StagingItem.Input;
			SetCursor(queryCursorEventArgs.Cursor);
		}
		if (e.StagingItem.Input.RoutedEvent != InputManager.InputReportEvent)
		{
			return;
		}
		InputReportEventArgs inputReportEventArgs = e.StagingItem.Input as InputReportEventArgs;
		if (inputReportEventArgs.Handled || inputReportEventArgs.Report.Type != InputType.Mouse)
		{
			return;
		}
		RawMouseInputReport rawMouseInputReport = (RawMouseInputReport)inputReportEventArgs.Report;
		if (_inputSource != null && rawMouseInputReport.InputSource == _inputSource.Value)
		{
			RawMouseActions nonRedundantActions = GetNonRedundantActions(e);
			if ((nonRedundantActions & RawMouseActions.Activate) == RawMouseActions.Activate)
			{
				Synchronize();
			}
			if ((nonRedundantActions & RawMouseActions.VerticalWheelRotate) == RawMouseActions.VerticalWheelRotate)
			{
				e.PushInput(new MouseWheelEventArgs(this, rawMouseInputReport.Timestamp, rawMouseInputReport.Wheel)
				{
					RoutedEvent = Mouse.PreviewMouseWheelEvent
				}, e.StagingItem);
			}
			if ((nonRedundantActions & RawMouseActions.Button1Press) == RawMouseActions.Button1Press)
			{
				e.PushInput(new MouseButtonEventArgs(this, rawMouseInputReport.Timestamp, MouseButton.Left, GetStylusDevice(e.StagingItem))
				{
					RoutedEvent = Mouse.PreviewMouseDownEvent
				}, e.StagingItem);
			}
			if ((nonRedundantActions & RawMouseActions.Button1Release) == RawMouseActions.Button1Release)
			{
				e.PushInput(new MouseButtonEventArgs(this, rawMouseInputReport.Timestamp, MouseButton.Left, GetStylusDevice(e.StagingItem))
				{
					RoutedEvent = Mouse.PreviewMouseUpEvent
				}, e.StagingItem);
			}
			if ((nonRedundantActions & RawMouseActions.Button2Press) == RawMouseActions.Button2Press)
			{
				e.PushInput(new MouseButtonEventArgs(this, rawMouseInputReport.Timestamp, MouseButton.Right, GetStylusDevice(e.StagingItem))
				{
					RoutedEvent = Mouse.PreviewMouseDownEvent
				}, e.StagingItem);
			}
			if ((nonRedundantActions & RawMouseActions.Button2Release) == RawMouseActions.Button2Release)
			{
				e.PushInput(new MouseButtonEventArgs(this, rawMouseInputReport.Timestamp, MouseButton.Right, GetStylusDevice(e.StagingItem))
				{
					RoutedEvent = Mouse.PreviewMouseUpEvent
				}, e.StagingItem);
			}
			if ((nonRedundantActions & RawMouseActions.Button3Press) == RawMouseActions.Button3Press)
			{
				e.PushInput(new MouseButtonEventArgs(this, rawMouseInputReport.Timestamp, MouseButton.Middle, GetStylusDevice(e.StagingItem))
				{
					RoutedEvent = Mouse.PreviewMouseDownEvent
				}, e.StagingItem);
			}
			if ((nonRedundantActions & RawMouseActions.Button3Release) == RawMouseActions.Button3Release)
			{
				e.PushInput(new MouseButtonEventArgs(this, rawMouseInputReport.Timestamp, MouseButton.Middle, GetStylusDevice(e.StagingItem))
				{
					RoutedEvent = Mouse.PreviewMouseUpEvent
				}, e.StagingItem);
			}
			if ((nonRedundantActions & RawMouseActions.Button4Press) == RawMouseActions.Button4Press)
			{
				e.PushInput(new MouseButtonEventArgs(this, rawMouseInputReport.Timestamp, MouseButton.XButton1, GetStylusDevice(e.StagingItem))
				{
					RoutedEvent = Mouse.PreviewMouseDownEvent
				}, e.StagingItem);
			}
			if ((nonRedundantActions & RawMouseActions.Button4Release) == RawMouseActions.Button4Release)
			{
				e.PushInput(new MouseButtonEventArgs(this, rawMouseInputReport.Timestamp, MouseButton.XButton1, GetStylusDevice(e.StagingItem))
				{
					RoutedEvent = Mouse.PreviewMouseUpEvent
				}, e.StagingItem);
			}
			if ((nonRedundantActions & RawMouseActions.Button5Press) == RawMouseActions.Button5Press)
			{
				e.PushInput(new MouseButtonEventArgs(this, rawMouseInputReport.Timestamp, MouseButton.XButton2, GetStylusDevice(e.StagingItem))
				{
					RoutedEvent = Mouse.PreviewMouseDownEvent
				}, e.StagingItem);
			}
			if ((nonRedundantActions & RawMouseActions.Button5Release) == RawMouseActions.Button5Release)
			{
				e.PushInput(new MouseButtonEventArgs(this, rawMouseInputReport.Timestamp, MouseButton.XButton2, GetStylusDevice(e.StagingItem))
				{
					RoutedEvent = Mouse.PreviewMouseUpEvent
				}, e.StagingItem);
			}
			if ((nonRedundantActions & RawMouseActions.AbsoluteMove) == RawMouseActions.AbsoluteMove)
			{
				e.PushInput(new MouseEventArgs(this, rawMouseInputReport.Timestamp, GetStylusDevice(e.StagingItem))
				{
					RoutedEvent = Mouse.PreviewMouseMoveEvent
				}, e.StagingItem);
			}
			if ((nonRedundantActions & RawMouseActions.QueryCursor) == RawMouseActions.QueryCursor)
			{
				inputReportEventArgs.Handled = UpdateCursorPrivate();
			}
		}
	}

	private RawMouseActions GetNonRedundantActions(NotifyInputEventArgs e)
	{
		RawMouseActions result = RawMouseActions.None;
		object data = e.StagingItem.GetData(_tagNonRedundantActions);
		if (data != null)
		{
			result = (RawMouseActions)data;
		}
		return result;
	}

	internal static IInputElement GlobalHitTest(bool clientUnits, Point pt, PresentationSource inputSource)
	{
		GlobalHitTest(clientUnits, pt, inputSource, out var enabledHit, out var _);
		return enabledHit;
	}

	internal static IInputElement GlobalHitTest(Point ptClient, PresentationSource inputSource)
	{
		return GlobalHitTest(clientUnits: true, ptClient, inputSource);
	}

	private static void GlobalHitTest(bool clientUnits, Point pt, PresentationSource inputSource, out IInputElement enabledHit, out IInputElement originalHit)
	{
		enabledHit = (originalHit = null);
		Point pointClient = (clientUnits ? pt : PointUtil.RootToClient(pt, inputSource));
		if (inputSource is HwndSource { CompositionTarget: not null, IsHandleNull: false } hwndSource)
		{
			Point pointScreen = PointUtil.ClientToScreen(pointClient, hwndSource);
			nint zero = IntPtr.Zero;
			HwndSource hwndSource2 = null;
			zero = MS.Win32.UnsafeNativeMethods.WindowFromPoint((int)pointScreen.X, (int)pointScreen.Y);
			if (!SafeNativeMethods.IsWindowEnabled(new HandleRef(null, zero)))
			{
				zero = IntPtr.Zero;
			}
			if (zero != IntPtr.Zero)
			{
				hwndSource2 = HwndSource.CriticalFromHwnd(zero);
			}
			if (hwndSource2 != null && hwndSource2.Dispatcher == inputSource.CompositionTarget.Dispatcher)
			{
				Point pt2 = PointUtil.ScreenToClient(pointScreen, hwndSource2);
				LocalHitTest(clientUnits: true, pt2, hwndSource2, out enabledHit, out originalHit);
			}
		}
	}

	internal static IInputElement LocalHitTest(bool clientUnits, Point pt, PresentationSource inputSource)
	{
		LocalHitTest(clientUnits, pt, inputSource, out var enabledHit, out var _);
		return enabledHit;
	}

	internal static IInputElement LocalHitTest(Point ptClient, PresentationSource inputSource)
	{
		return LocalHitTest(clientUnits: true, ptClient, inputSource);
	}

	private static void LocalHitTest(bool clientUnits, Point pt, PresentationSource inputSource, out IInputElement enabledHit, out IInputElement originalHit)
	{
		enabledHit = (originalHit = null);
		if (inputSource != null && inputSource.RootVisual is UIElement uIElement)
		{
			Point pt2 = (clientUnits ? PointUtil.ClientToRoot(pt, inputSource) : pt);
			uIElement.InputHitTest(pt2, out enabledHit, out originalHit);
		}
	}

	internal bool IsSameSpot(Point newPosition, StylusDevice stylusDevice)
	{
		int num = stylusDevice?.DoubleTapDeltaX ?? _doubleClickDeltaX;
		int num2 = stylusDevice?.DoubleTapDeltaY ?? _doubleClickDeltaY;
		if (Math.Abs(newPosition.X - _lastClick.X) < (double)num)
		{
			return Math.Abs(newPosition.Y - _lastClick.Y) < (double)num2;
		}
		return false;
	}

	internal int CalculateClickCount(MouseButton button, int timeStamp, StylusDevice stylusDevice, Point downPt)
	{
		int num = timeStamp - _lastClickTime;
		int num2 = stylusDevice?.DoubleTapDeltaTime ?? _doubleClickDeltaTime;
		bool flag = IsSameSpot(downPt, stylusDevice);
		bool flag2 = _lastButton == button;
		if (num < num2 && flag && flag2)
		{
			return _clickCount + 1;
		}
		return 1;
	}

	private StylusDevice GetStylusDevice(StagingAreaInputItem stagingItem)
	{
		return stagingItem.GetData(_tagStylusDevice) as StylusDevice;
	}
}
