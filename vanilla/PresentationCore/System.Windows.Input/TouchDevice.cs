using System.Collections.Generic;
using System.Windows.Input.Tracing;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Input;

/// <summary>Represents a single touch input produced by a finger on a touchscreen.</summary>
public abstract class TouchDevice : InputDevice, IManipulator
{
	private int _deviceId;

	private IInputElement _directlyOver;

	private IInputElement _captured;

	private CaptureMode _captureMode;

	private bool _isDown;

	private DispatcherOperation _reevaluateCapture;

	private DispatcherOperation _reevaluateOver;

	private DeferredElementTreeState _directlyOverTreeState;

	private DeferredElementTreeState _capturedWithinTreeState;

	private bool _isPrimary;

	private bool _isActive;

	private Action<DependencyObject, bool> _raiseTouchEnterOrLeaveAction;

	private bool _lastDownHandled;

	private bool _lastUpHandled;

	private bool _lastMoveHandled;

	private PresentationSource _activeSource;

	private InputManager _inputManager;

	private WeakReference _manipulatingElement;

	[ThreadStatic]
	private static List<TouchDevice> _activeDevices;

	/// <summary>Gets the unique identifier of the <see cref="T:System.Windows.Input.TouchDevice" />, as provided by the operating system.</summary>
	/// <returns>The unique identifier of the <see cref="T:System.Windows.Input.TouchDevice" />.</returns>
	public int Id => _deviceId;

	/// <summary>Gets a value that indicates whether the device is active.</summary>
	/// <returns>true if the device is active; otherwise, false.</returns>
	public bool IsActive => _isActive;

	/// <summary>Gets the element that receives input from the <see cref="T:System.Windows.Input.TouchDevice" />.</summary>
	/// <returns>The element that receives input from the <see cref="T:System.Windows.Input.TouchDevice" />.</returns>
	public sealed override IInputElement Target => _directlyOver;

	/// <summary>Gets the <see cref="T:System.Windows.PresentationSource" /> that is reporting input for this device.</summary>
	/// <returns>The source that is reporting input for this device.</returns>
	public sealed override PresentationSource ActiveSource => _activeSource;

	/// <summary>Gets the element that the touch contact point is directly over.</summary>
	/// <returns>The element that the touch contact point is directly over.</returns>
	public IInputElement DirectlyOver => _directlyOver;

	/// <summary>Gets the element that captured the <see cref="T:System.Windows.Input.TouchDevice" />.</summary>
	/// <returns>The element that captured the <see cref="T:System.Windows.Input.TouchDevice" />.</returns>
	public IInputElement Captured => _captured;

	/// <summary>Gets the capture policy of the <see cref="T:System.Windows.Input.TouchDevice" />.</summary>
	/// <returns>The capture policy of the <see cref="T:System.Windows.Input.TouchDevice" />.</returns>
	public CaptureMode CaptureMode => _captureMode;

	private Action<DependencyObject, bool> RaiseTouchEnterOrLeaveAction
	{
		get
		{
			if (_raiseTouchEnterOrLeaveAction == null)
			{
				_raiseTouchEnterOrLeaveAction = RaiseTouchEnterOrLeave;
			}
			return _raiseTouchEnterOrLeaveAction;
		}
	}

	internal bool PromotingToManipulation { get; private set; }

	/// <summary>Gets the unique identifier of the <see cref="T:System.Windows.Input.TouchDevice" /> as provided by the operating system.</summary>
	/// <returns>The unique identifier of the <see cref="T:System.Windows.Input.TouchDevice" />.</returns>
	int IManipulator.Id => Id;

	/// <summary>Occurs when the <see cref="T:System.Windows.Input.TouchDevice" /> is added to the input messaging system.</summary>
	public event EventHandler Activated;

	/// <summary>Occurs when the <see cref="T:System.Windows.Input.TouchDevice" /> is removed from the input messaging system.</summary>
	public event EventHandler Deactivated;

	/// <summary>Occurs when a touch message is sent.</summary>
	public event EventHandler Updated;

	/// <summary>Called from constructors in derived classes to initialize the <see cref="T:System.Windows.Input.TouchDevice" /> class. </summary>
	/// <param name="deviceId">A unique identifier for the touch device.</param>
	protected TouchDevice(int deviceId)
	{
		_deviceId = deviceId;
		_inputManager = InputManager.UnsecureCurrent;
		StylusLogic currentStylusLogic = StylusLogic.CurrentStylusLogic;
		if (currentStylusLogic != null && !(this is StylusTouchDeviceBase))
		{
			currentStylusLogic.Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.CustomTouchDeviceUsed;
		}
	}

	private void AttachTouchDevice()
	{
		_inputManager.PostProcessInput += PostProcessInput;
		_inputManager.HitTestInvalidatedAsync += OnHitTestInvalidatedAsync;
	}

	private void DetachTouchDevice()
	{
		_inputManager.PostProcessInput -= PostProcessInput;
		_inputManager.HitTestInvalidatedAsync -= OnHitTestInvalidatedAsync;
	}

	/// <summary>Sets the <see cref="T:System.Windows.PresentationSource" /> that is reporting input for this device.</summary>
	/// <param name="activeSource">The source that reports input for this device.</param>
	protected void SetActiveSource(PresentationSource activeSource)
	{
		_activeSource = activeSource;
	}

	/// <summary>Returns the current position of the touch device relative to the specified element.</summary>
	/// <returns>The current position of the touch device relative to the specified element.</returns>
	/// <param name="relativeTo">The element that defines the coordinate space.</param>
	public abstract TouchPoint GetTouchPoint(IInputElement relativeTo);

	/// <summary>When overridden in a derived class, returns all touch points that are collected between the most recent and previous touch events.</summary>
	/// <returns>All touch points that were collected between the most recent and previous touch events.</returns>
	/// <param name="relativeTo">The element that defines the coordinate space.</param>
	public abstract TouchPointCollection GetIntermediateTouchPoints(IInputElement relativeTo);

	private IInputElement CriticalHitTest(Point point, bool isSynchronize)
	{
		IInputElement element = null;
		if (_activeSource != null)
		{
			switch (_captureMode)
			{
			case CaptureMode.None:
				if (_isDown)
				{
					element = ((!isSynchronize) ? LocalHitTest(point, _activeSource) : GlobalHitTest(point, _activeSource));
					EnsureValid(ref element);
				}
				break;
			case CaptureMode.Element:
				element = _captured;
				break;
			case CaptureMode.SubTree:
				if (InputElement.GetContainingInputElement(_captured as DependencyObject) != null)
				{
					element = GlobalHitTest(point, _activeSource);
				}
				EnsureValid(ref element);
				if (element != null)
				{
					IInputElement inputElement = element;
					while (inputElement != null && inputElement != _captured)
					{
						inputElement = ((inputElement is UIElement uIElement) ? InputElement.GetContainingInputElement(uIElement.GetUIParent(continuePastVisualTree: true)) : ((!(inputElement is ContentElement contentElement)) ? InputElement.GetContainingInputElement(((UIElement3D)inputElement).GetUIParent(continuePastVisualTree: true)) : InputElement.GetContainingInputElement(contentElement.GetUIParent(continuePastVisualTree: true))));
					}
					if (inputElement != _captured)
					{
						element = _captured;
					}
				}
				else
				{
					element = _captured;
				}
				break;
			}
		}
		return element;
	}

	private static void EnsureValid(ref IInputElement element)
	{
		if (element != null && !InputElement.IsValid(element))
		{
			element = InputElement.GetContainingInputElement(element as DependencyObject);
		}
	}

	private static IInputElement GlobalHitTest(Point pt, PresentationSource inputSource)
	{
		return MouseDevice.GlobalHitTest(clientUnits: false, pt, inputSource);
	}

	private static IInputElement LocalHitTest(Point pt, PresentationSource inputSource)
	{
		return MouseDevice.LocalHitTest(clientUnits: false, pt, inputSource);
	}

	/// <summary>Captures a touch to the specified element by using the <see cref="F:System.Windows.Input.CaptureMode.Element" /> capture mode.</summary>
	/// <returns>true if the element was able to capture the touch; otherwise, false.</returns>
	/// <param name="element">The element that captures the touch input.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="element" /> is not a <see cref="T:System.Windows.UIElement" />, <see cref="T:System.Windows.UIElement3D" />, or <see cref="T:System.Windows.ContentElement" />.</exception>
	public bool Capture(IInputElement element)
	{
		return Capture(element, CaptureMode.Element);
	}

	/// <summary>Captures a touch to the specified element by using the specified <see cref="T:System.Windows.Input.CaptureMode" />.</summary>
	/// <returns>true if the element was able to capture the touch; otherwise, false.</returns>
	/// <param name="element">The element that captures the touch.</param>
	/// <param name="captureMode">The capture policy to use.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="element" /> is not a <see cref="T:System.Windows.UIElement" />, <see cref="T:System.Windows.UIElement3D" />, or <see cref="T:System.Windows.ContentElement" />.</exception>
	public bool Capture(IInputElement element, CaptureMode captureMode)
	{
		VerifyAccess();
		if (element == null || captureMode == CaptureMode.None)
		{
			element = null;
			captureMode = CaptureMode.None;
		}
		CastInputElement(element, out var uiElement, out var contentElement, out var uiElement3D);
		if (element != null && uiElement == null && contentElement == null && uiElement3D == null)
		{
			throw new ArgumentException(SR.Format(SR.Invalid_IInputElement, element.GetType()), "element");
		}
		if (_captured != element)
		{
			if (element == null || (uiElement != null && uiElement.IsVisible && uiElement.IsEnabled) || (contentElement != null && contentElement.IsEnabled) || (uiElement3D != null && uiElement3D.IsVisible && uiElement3D.IsEnabled))
			{
				IInputElement captured = _captured;
				_captured = element;
				_captureMode = captureMode;
				CastInputElement(captured, out var uiElement2, out var contentElement2, out var uiElement3D2);
				if (uiElement2 != null)
				{
					uiElement2.IsEnabledChanged -= OnReevaluateCapture;
					uiElement2.IsVisibleChanged -= OnReevaluateCapture;
					uiElement2.IsHitTestVisibleChanged -= OnReevaluateCapture;
				}
				else if (contentElement2 != null)
				{
					contentElement2.IsEnabledChanged -= OnReevaluateCapture;
				}
				else if (uiElement3D2 != null)
				{
					uiElement3D2.IsEnabledChanged -= OnReevaluateCapture;
					uiElement3D2.IsVisibleChanged -= OnReevaluateCapture;
					uiElement3D2.IsHitTestVisibleChanged -= OnReevaluateCapture;
				}
				if (uiElement != null)
				{
					uiElement.IsEnabledChanged += OnReevaluateCapture;
					uiElement.IsVisibleChanged += OnReevaluateCapture;
					uiElement.IsHitTestVisibleChanged += OnReevaluateCapture;
				}
				else if (contentElement != null)
				{
					contentElement.IsEnabledChanged += OnReevaluateCapture;
				}
				else if (uiElement3D != null)
				{
					uiElement3D.IsEnabledChanged += OnReevaluateCapture;
					uiElement3D.IsVisibleChanged += OnReevaluateCapture;
					uiElement3D.IsHitTestVisibleChanged += OnReevaluateCapture;
				}
				UpdateReverseInheritedProperty(capture: true, captured, _captured);
				if (captured != null)
				{
					(captured as DependencyObject).SetValue(UIElement.AreAnyTouchesCapturedPropertyKey, BooleanBoxes.Box(AreAnyTouchesCapturedOrDirectlyOver(captured, isCapture: true)));
				}
				if (_captured != null)
				{
					(_captured as DependencyObject).SetValue(UIElement.AreAnyTouchesCapturedPropertyKey, BooleanBoxes.TrueBox);
				}
				if (captured != null)
				{
					RaiseLostCapture(captured);
				}
				if (_captured != null)
				{
					RaiseGotCapture(_captured);
				}
				OnCapture(element, captureMode);
				Synchronize();
				return true;
			}
			return false;
		}
		return true;
	}

	private void UpdateReverseInheritedProperty(bool capture, IInputElement oldElement, IInputElement newElement)
	{
		List<DependencyObject> list = null;
		int num = ((_activeDevices != null) ? _activeDevices.Count : 0);
		if (num > 0)
		{
			list = new List<DependencyObject>(num);
		}
		for (int i = 0; i < num; i++)
		{
			TouchDevice touchDevice = _activeDevices[i];
			if (touchDevice != this)
			{
				DependencyObject dependencyObject = (capture ? (touchDevice._captured as DependencyObject) : (touchDevice._directlyOver as DependencyObject));
				if (dependencyObject != null)
				{
					list.Add(dependencyObject);
				}
			}
		}
		ReverseInheritProperty obj = (capture ? ((ReverseInheritProperty)UIElement.TouchesCapturedWithinProperty) : ((ReverseInheritProperty)UIElement.TouchesOverProperty));
		DeferredElementTreeState oldTreeState = (capture ? _capturedWithinTreeState : _directlyOverTreeState);
		Action<DependencyObject, bool> originChangedAction = (capture ? null : RaiseTouchEnterOrLeaveAction);
		obj.OnOriginValueChanged(oldElement as DependencyObject, newElement as DependencyObject, list, ref oldTreeState, originChangedAction);
		if (capture)
		{
			_capturedWithinTreeState = oldTreeState;
		}
		else
		{
			_directlyOverTreeState = oldTreeState;
		}
	}

	internal static void ReevaluateCapturedWithin(DependencyObject element, DependencyObject oldParent, bool isCoreParent)
	{
		int num = ((_activeDevices != null) ? _activeDevices.Count : 0);
		for (int i = 0; i < num; i++)
		{
			_activeDevices[i].ReevaluateCapturedWithinAsync(element, oldParent, isCoreParent);
		}
	}

	private void ReevaluateCapturedWithinAsync(DependencyObject element, DependencyObject oldParent, bool isCoreParent)
	{
		if (element != null)
		{
			if (_capturedWithinTreeState == null)
			{
				_capturedWithinTreeState = new DeferredElementTreeState();
			}
			if (isCoreParent)
			{
				_capturedWithinTreeState.SetCoreParent(element, oldParent);
			}
			else
			{
				_capturedWithinTreeState.SetLogicalParent(element, oldParent);
			}
		}
		if (_reevaluateCapture == null)
		{
			_reevaluateCapture = base.Dispatcher.BeginInvoke(DispatcherPriority.Input, (DispatcherOperationCallback)delegate(object args)
			{
				TouchDevice obj = (TouchDevice)args;
				obj._reevaluateCapture = null;
				obj.OnReevaluateCapturedWithinAsync();
				return (object)null;
			}, this);
		}
	}

	private void OnReevaluateCapturedWithinAsync()
	{
		if (_captured != null)
		{
			bool flag = false;
			CastInputElement(_captured, out var uiElement, out var contentElement, out var uiElement3D);
			flag = ((uiElement != null) ? (!uiElement.IsEnabled || !uiElement.IsVisible || !uiElement.IsHitTestVisible) : ((contentElement != null) ? (!contentElement.IsEnabled) : (uiElement3D == null || !uiElement3D.IsEnabled || !uiElement3D.IsVisible || !uiElement3D.IsHitTestVisible)));
			if (!flag)
			{
				DependencyObject containingVisual = InputElement.GetContainingVisual(_captured as DependencyObject);
				flag = !ValidateVisualForCapture(containingVisual);
			}
			if (flag)
			{
				Capture(null);
			}
			if (_capturedWithinTreeState != null && !_capturedWithinTreeState.IsEmpty)
			{
				UpdateReverseInheritedProperty(capture: true, _captured, _captured);
			}
		}
	}

	private bool ValidateVisualForCapture(DependencyObject visual)
	{
		if (visual == null)
		{
			return false;
		}
		PresentationSource presentationSource = PresentationSource.CriticalFromVisual(visual);
		if (presentationSource != null)
		{
			return presentationSource == _activeSource;
		}
		return false;
	}

	private void OnReevaluateCapture(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (!(bool)e.NewValue && _reevaluateCapture == null)
		{
			_reevaluateCapture = base.Dispatcher.BeginInvoke(DispatcherPriority.Input, (DispatcherOperationCallback)delegate(object args)
			{
				TouchDevice obj = (TouchDevice)args;
				obj._reevaluateCapture = null;
				obj.Capture(null);
				return (object)null;
			}, this);
		}
	}

	private static void CastInputElement(IInputElement element, out UIElement uiElement, out ContentElement contentElement, out UIElement3D uiElement3D)
	{
		uiElement = element as UIElement;
		contentElement = ((uiElement == null) ? (element as ContentElement) : null);
		uiElement3D = ((uiElement == null && contentElement == null) ? (element as UIElement3D) : null);
	}

	private void RaiseLostCapture(IInputElement oldCapture)
	{
		TouchEventArgs touchEventArgs = CreateEventArgs(Touch.LostTouchCaptureEvent);
		touchEventArgs.Source = oldCapture;
		_inputManager.ProcessInput(touchEventArgs);
	}

	private void RaiseGotCapture(IInputElement captured)
	{
		TouchEventArgs touchEventArgs = CreateEventArgs(Touch.GotTouchCaptureEvent);
		touchEventArgs.Source = captured;
		_inputManager.ProcessInput(touchEventArgs);
	}

	/// <summary>Called when a touch is captured to an element.</summary>
	/// <param name="element">The element that captures the touch input.</param>
	/// <param name="captureMode">The capture policy.</param>
	protected virtual void OnCapture(IInputElement element, CaptureMode captureMode)
	{
	}

	/// <summary>Reports that a touch is pressed on an element.</summary>
	/// <returns>true if the <see cref="E:System.Windows.UIElement.TouchDown" /> event was handled; otherwise, false.</returns>
	protected bool ReportDown()
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordInput, EventTrace.Level.Info, EventTrace.Event.TouchDownReported, _deviceId);
		_isDown = true;
		UpdateDirectlyOver(isSynchronize: false);
		bool result = RaiseTouchDown();
		OnUpdated();
		Touch.ReportFrame();
		return result;
	}

	/// <summary>Reports that a touch is moving across an element.</summary>
	/// <returns>true if the <see cref="E:System.Windows.UIElement.TouchMove" /> event was handled; otherwise, false.</returns>
	protected bool ReportMove()
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordInput, EventTrace.Level.Info, EventTrace.Event.TouchMoveReported, _deviceId);
		UpdateDirectlyOver(isSynchronize: false);
		bool result = RaiseTouchMove();
		OnUpdated();
		Touch.ReportFrame();
		return result;
	}

	/// <summary>Reports that a touch was lifted from an element.</summary>
	/// <returns>true if the <see cref="E:System.Windows.UIElement.TouchUp" /> event was handled; otherwise, false.</returns>
	protected bool ReportUp()
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordInput, EventTrace.Level.Info, EventTrace.Event.TouchUpReported, _deviceId);
		if (_reevaluateOver != null)
		{
			_reevaluateOver = null;
			OnHitTestInvalidatedAsync(this, EventArgs.Empty);
		}
		bool result = RaiseTouchUp();
		_isDown = false;
		UpdateDirectlyOver(isSynchronize: false);
		OnUpdated();
		Touch.ReportFrame();
		return result;
	}

	/// <summary>Adds the <see cref="T:System.Windows.Input.TouchDevice" /> to the input messaging system.</summary>
	/// <exception cref="T:System.InvalidOperationException">The device is already activated.</exception>
	protected void Activate()
	{
		if (_isActive)
		{
			throw new InvalidOperationException(SR.Touch_DeviceAlreadyActivated);
		}
		PromotingToManipulation = false;
		AddActiveDevice(this);
		AttachTouchDevice();
		Synchronize();
		if (_activeDevices.Count == 1)
		{
			_isPrimary = true;
		}
		_isActive = true;
		if (this.Activated != null)
		{
			this.Activated(this, EventArgs.Empty);
		}
	}

	/// <summary>Removes the <see cref="T:System.Windows.Input.TouchDevice" /> from the input messaging system.</summary>
	/// <exception cref="T:System.InvalidOperationException">The device is not activated.</exception>
	protected void Deactivate()
	{
		if (!_isActive)
		{
			throw new InvalidOperationException(SR.Touch_DeviceNotActivated);
		}
		Capture(null);
		DetachTouchDevice();
		RemoveActiveDevice(this);
		_isActive = false;
		_manipulatingElement = null;
		if (this.Deactivated != null)
		{
			this.Deactivated(this, EventArgs.Empty);
		}
	}

	/// <summary>Forces the <see cref="T:System.Windows.Input.TouchDevice" /> to synchronize the user interface with underlying touch points.</summary>
	public void Synchronize()
	{
		if (_activeSource != null && _activeSource.CompositionTarget != null && !_activeSource.CompositionTarget.IsDisposed && UpdateDirectlyOver(isSynchronize: true))
		{
			OnUpdated();
			Touch.ReportFrame();
		}
	}

	/// <summary>Called when a manipulation has ended.</summary>
	/// <param name="cancel">true to cancel the action; otherwise, false.</param>
	protected virtual void OnManipulationEnded(bool cancel)
	{
		if (GetManipulatableElement() != null && PromotingToManipulation)
		{
			Capture(null);
		}
	}

	/// <summary>Called when a manipulation is started.</summary>
	protected virtual void OnManipulationStarted()
	{
	}

	private void OnHitTestInvalidatedAsync(object sender, EventArgs e)
	{
		Synchronize();
		if (_directlyOverTreeState != null && !_directlyOverTreeState.IsEmpty)
		{
			UpdateReverseInheritedProperty(capture: false, _directlyOver, _directlyOver);
		}
	}

	private bool UpdateDirectlyOver(bool isSynchronize)
	{
		IInputElement inputElement = null;
		TouchPoint touchPoint = GetTouchPoint(null);
		if (touchPoint != null)
		{
			Point position = touchPoint.Position;
			inputElement = CriticalHitTest(position, isSynchronize);
		}
		if (inputElement != _directlyOver)
		{
			ChangeDirectlyOver(inputElement);
			return true;
		}
		return false;
	}

	private void OnReevaluateDirectlyOver(object sender, DependencyPropertyChangedEventArgs e)
	{
		ReevaluateDirectlyOverAsync(null, null, isCoreParent: true);
	}

	internal static void ReevaluateDirectlyOver(DependencyObject element, DependencyObject oldParent, bool isCoreParent)
	{
		int num = ((_activeDevices != null) ? _activeDevices.Count : 0);
		for (int i = 0; i < num; i++)
		{
			_activeDevices[i].ReevaluateDirectlyOverAsync(element, oldParent, isCoreParent);
		}
	}

	private void ReevaluateDirectlyOverAsync(DependencyObject element, DependencyObject oldParent, bool isCoreParent)
	{
		if (element != null)
		{
			if (_directlyOverTreeState == null)
			{
				_directlyOverTreeState = new DeferredElementTreeState();
			}
			if (isCoreParent)
			{
				_directlyOverTreeState.SetCoreParent(element, oldParent);
			}
			else
			{
				_directlyOverTreeState.SetLogicalParent(element, oldParent);
			}
		}
		if (_reevaluateOver == null)
		{
			_reevaluateOver = base.Dispatcher.BeginInvoke(DispatcherPriority.Input, (DispatcherOperationCallback)delegate(object args)
			{
				TouchDevice obj = (TouchDevice)args;
				obj._reevaluateOver = null;
				obj.OnHitTestInvalidatedAsync(this, EventArgs.Empty);
				return (object)null;
			}, this);
		}
	}

	private void ChangeDirectlyOver(IInputElement newDirectlyOver)
	{
		IInputElement directlyOver = _directlyOver;
		_directlyOver = newDirectlyOver;
		CastInputElement(directlyOver, out var uiElement, out var contentElement, out var uiElement3D);
		CastInputElement(newDirectlyOver, out var uiElement2, out var contentElement2, out var uiElement3D2);
		if (uiElement != null)
		{
			uiElement.IsEnabledChanged -= OnReevaluateDirectlyOver;
			uiElement.IsVisibleChanged -= OnReevaluateDirectlyOver;
			uiElement.IsHitTestVisibleChanged -= OnReevaluateDirectlyOver;
		}
		else if (contentElement != null)
		{
			contentElement.IsEnabledChanged -= OnReevaluateDirectlyOver;
		}
		else if (uiElement3D != null)
		{
			uiElement3D.IsEnabledChanged -= OnReevaluateDirectlyOver;
			uiElement3D.IsVisibleChanged -= OnReevaluateDirectlyOver;
			uiElement3D.IsHitTestVisibleChanged -= OnReevaluateDirectlyOver;
		}
		if (uiElement2 != null)
		{
			uiElement2.IsEnabledChanged += OnReevaluateDirectlyOver;
			uiElement2.IsVisibleChanged += OnReevaluateDirectlyOver;
			uiElement2.IsHitTestVisibleChanged += OnReevaluateDirectlyOver;
		}
		else if (contentElement2 != null)
		{
			contentElement2.IsEnabledChanged += OnReevaluateDirectlyOver;
		}
		else if (uiElement3D2 != null)
		{
			uiElement3D2.IsEnabledChanged += OnReevaluateDirectlyOver;
			uiElement3D2.IsVisibleChanged += OnReevaluateDirectlyOver;
			uiElement3D2.IsHitTestVisibleChanged += OnReevaluateDirectlyOver;
		}
		UpdateReverseInheritedProperty(capture: false, directlyOver, newDirectlyOver);
		if (directlyOver != null)
		{
			(directlyOver as DependencyObject).SetValue(UIElement.AreAnyTouchesDirectlyOverPropertyKey, BooleanBoxes.Box(AreAnyTouchesCapturedOrDirectlyOver(directlyOver, isCapture: false)));
		}
		if (newDirectlyOver != null)
		{
			(newDirectlyOver as DependencyObject).SetValue(UIElement.AreAnyTouchesDirectlyOverPropertyKey, BooleanBoxes.TrueBox);
		}
	}

	private void RaiseTouchEnterOrLeave(DependencyObject element, bool isLeave)
	{
		TouchEventArgs touchEventArgs = CreateEventArgs(isLeave ? Touch.TouchLeaveEvent : Touch.TouchEnterEvent);
		touchEventArgs.Source = element;
		_inputManager.ProcessInput(touchEventArgs);
	}

	private TouchEventArgs CreateEventArgs(RoutedEvent routedEvent)
	{
		return new TouchEventArgs(this, Environment.TickCount)
		{
			RoutedEvent = routedEvent
		};
	}

	private bool RaiseTouchDown()
	{
		TouchEventArgs input = CreateEventArgs(Touch.PreviewTouchDownEvent);
		_lastDownHandled = false;
		_inputManager.ProcessInput(input);
		return _lastDownHandled;
	}

	private bool RaiseTouchMove()
	{
		TouchEventArgs input = CreateEventArgs(Touch.PreviewTouchMoveEvent);
		_lastMoveHandled = false;
		_inputManager.ProcessInput(input);
		return _lastMoveHandled;
	}

	private bool RaiseTouchUp()
	{
		TouchEventArgs input = CreateEventArgs(Touch.PreviewTouchUpEvent);
		_lastUpHandled = false;
		_inputManager.ProcessInput(input);
		return _lastUpHandled;
	}

	private void PostProcessInput(object sender, ProcessInputEventArgs e)
	{
		InputEventArgs input = e.StagingItem.Input;
		if (input == null || input.Device != this)
		{
			return;
		}
		if (input.Handled)
		{
			RoutedEvent routedEvent = input.RoutedEvent;
			if (routedEvent == Touch.PreviewTouchMoveEvent || routedEvent == Touch.TouchMoveEvent)
			{
				_lastMoveHandled = true;
			}
			else if (routedEvent == Touch.PreviewTouchDownEvent || routedEvent == Touch.TouchDownEvent)
			{
				_lastDownHandled = true;
			}
			else if (routedEvent == Touch.PreviewTouchUpEvent || routedEvent == Touch.TouchUpEvent)
			{
				_lastUpHandled = true;
			}
			return;
		}
		bool forManipulation;
		RoutedEvent routedEvent2 = PromotePreviewToMain(input.RoutedEvent, out forManipulation);
		if (routedEvent2 != null)
		{
			TouchEventArgs input2 = CreateEventArgs(routedEvent2);
			e.PushInput(input2, e.StagingItem);
		}
		else if (forManipulation)
		{
			UIElement manipulatableElement = GetManipulatableElement();
			if (manipulatableElement != null)
			{
				PromoteMainToManipulation(manipulatableElement, (TouchEventArgs)input);
			}
		}
	}

	private RoutedEvent PromotePreviewToMain(RoutedEvent routedEvent, out bool forManipulation)
	{
		forManipulation = false;
		if (routedEvent == Touch.PreviewTouchMoveEvent)
		{
			return Touch.TouchMoveEvent;
		}
		if (routedEvent == Touch.PreviewTouchDownEvent)
		{
			return Touch.TouchDownEvent;
		}
		if (routedEvent == Touch.PreviewTouchUpEvent)
		{
			return Touch.TouchUpEvent;
		}
		forManipulation = routedEvent == Touch.TouchMoveEvent || routedEvent == Touch.TouchDownEvent || routedEvent == Touch.TouchUpEvent || routedEvent == Touch.GotTouchCaptureEvent || routedEvent == Touch.LostTouchCaptureEvent;
		return null;
	}

	private UIElement GetManipulatableElement()
	{
		UIElement uIElement = InputElement.GetContainingUIElement(_directlyOver as DependencyObject) as UIElement;
		if (uIElement != null)
		{
			uIElement = Manipulation.FindManipulationParent(uIElement);
		}
		return uIElement;
	}

	private void PromoteMainToManipulation(UIElement manipulatableElement, TouchEventArgs touchEventArgs)
	{
		RoutedEvent routedEvent = touchEventArgs.RoutedEvent;
		if (routedEvent == Touch.TouchDownEvent)
		{
			Capture(manipulatableElement);
		}
		else if (routedEvent == Touch.TouchUpEvent && PromotingToManipulation)
		{
			Capture(null);
		}
		else if (routedEvent == Touch.GotTouchCaptureEvent && !PromotingToManipulation)
		{
			if (_captured is UIElement { IsManipulationEnabled: not false } uIElement)
			{
				_manipulatingElement = new WeakReference(uIElement);
				Manipulation.AddManipulator(uIElement, this);
				PromotingToManipulation = true;
				OnManipulationStarted();
			}
		}
		else if (routedEvent == Touch.LostTouchCaptureEvent && PromotingToManipulation && _manipulatingElement != null)
		{
			UIElement uIElement2 = _manipulatingElement.Target as UIElement;
			_manipulatingElement = null;
			if (uIElement2 != null)
			{
				Manipulation.TryRemoveManipulator(uIElement2, this);
				PromotingToManipulation = false;
			}
		}
	}

	private static void AddActiveDevice(TouchDevice device)
	{
		if (_activeDevices == null)
		{
			_activeDevices = new List<TouchDevice>(2);
		}
		_activeDevices.Add(device);
	}

	private static void RemoveActiveDevice(TouchDevice device)
	{
		if (_activeDevices != null)
		{
			_activeDevices.Remove(device);
		}
	}

	internal static TouchPointCollection GetTouchPoints(IInputElement relativeTo)
	{
		TouchPointCollection touchPointCollection = new TouchPointCollection();
		if (_activeDevices != null)
		{
			int count = _activeDevices.Count;
			for (int i = 0; i < count; i++)
			{
				TouchDevice touchDevice = _activeDevices[i];
				touchPointCollection.Add(touchDevice.GetTouchPoint(relativeTo));
			}
		}
		return touchPointCollection;
	}

	internal static TouchPoint GetPrimaryTouchPoint(IInputElement relativeTo)
	{
		if (_activeDevices != null && _activeDevices.Count > 0)
		{
			TouchDevice touchDevice = _activeDevices[0];
			if (touchDevice._isPrimary)
			{
				return touchDevice.GetTouchPoint(relativeTo);
			}
		}
		return null;
	}

	internal static void ReleaseAllCaptures(IInputElement element)
	{
		if (_activeDevices == null)
		{
			return;
		}
		int count = _activeDevices.Count;
		for (int i = 0; i < count; i++)
		{
			TouchDevice touchDevice = _activeDevices[i];
			if (touchDevice.Captured == element)
			{
				touchDevice.Capture(null);
			}
		}
	}

	internal static IEnumerable<TouchDevice> GetCapturedTouches(IInputElement element, bool includeWithin)
	{
		return GetCapturedOrOverTouches(element, includeWithin, isCapture: true);
	}

	internal static IEnumerable<TouchDevice> GetTouchesOver(IInputElement element, bool includeWithin)
	{
		return GetCapturedOrOverTouches(element, includeWithin, isCapture: false);
	}

	private static bool IsWithin(IInputElement parent, IInputElement child)
	{
		DependencyObject dependencyObject = child as DependencyObject;
		while (dependencyObject != null && dependencyObject != parent)
		{
			dependencyObject = ((!(dependencyObject is Visual) && !(dependencyObject is Visual3D)) ? ((ContentElement)dependencyObject).Parent : VisualTreeHelper.GetParent(dependencyObject));
		}
		return dependencyObject == parent;
	}

	private static IEnumerable<TouchDevice> GetCapturedOrOverTouches(IInputElement element, bool includeWithin, bool isCapture)
	{
		List<TouchDevice> list = new List<TouchDevice>();
		if (_activeDevices != null)
		{
			int count = _activeDevices.Count;
			for (int i = 0; i < count; i++)
			{
				TouchDevice touchDevice = _activeDevices[i];
				IInputElement inputElement = (isCapture ? touchDevice.Captured : touchDevice.DirectlyOver);
				if (inputElement != null && (inputElement == element || (includeWithin && IsWithin(element, inputElement))))
				{
					list.Add(touchDevice);
				}
			}
		}
		return list;
	}

	private static bool AreAnyTouchesCapturedOrDirectlyOver(IInputElement element, bool isCapture)
	{
		if (_activeDevices != null)
		{
			int count = _activeDevices.Count;
			for (int i = 0; i < count; i++)
			{
				TouchDevice touchDevice = _activeDevices[i];
				IInputElement inputElement = (isCapture ? touchDevice.Captured : touchDevice.DirectlyOver);
				if (inputElement != null && inputElement == element)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>Returns the position of the <see cref="T:System.Windows.Input.IManipulator" /> object.</summary>
	/// <returns>The position of the <see cref="T:System.Windows.Input.IManipulator" /> object.</returns>
	/// <param name="relativeTo">The element to use as the frame of reference for calculating the position of the <see cref="T:System.Windows.Input.IManipulator" />.</param>
	Point IManipulator.GetPosition(IInputElement relativeTo)
	{
		return GetTouchPoint(relativeTo).Position;
	}

	private void OnUpdated()
	{
		if (this.Updated != null)
		{
			this.Updated(this, EventArgs.Empty);
		}
	}

	/// <summary>Occurs when a manipulation has ended.</summary>
	/// <param name="cancel">true to cancel the action; otherwise, false.</param>
	void IManipulator.ManipulationEnded(bool cancel)
	{
		OnManipulationEnded(cancel);
	}
}
