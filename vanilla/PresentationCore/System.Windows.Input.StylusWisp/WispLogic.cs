using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Input.Tracing;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Interop;
using MS.Internal.PresentationCore;
using MS.Utility;
using MS.Win32;

namespace System.Windows.Input.StylusWisp;

internal class WispLogic : StylusLogic
{
	internal class StagingAreaInputItemList : List<StagingAreaInputItem>
	{
		private long _version;

		internal long Version => _version;

		internal void AddItem(StagingAreaInputItem item)
		{
			Add(item);
			IncrementVersion();
		}

		internal long IncrementVersion()
		{
			return ++_version;
		}
	}

	private SecurityCriticalData<InputManager> _inputManager;

	private DispatcherOperationCallback _dlgInputManagerProcessInput;

	private object _stylusEventQueueLock = new object();

	private Queue<RawStylusInputReport> _queueStylusEvents = new Queue<RawStylusInputReport>();

	private int _lastStylusDeviceId;

	private bool _lastMouseMoveFromStylus = true;

	private MouseButtonState _mouseLeftButtonState;

	private MouseButtonState _mouseRightButtonState;

	private StylusPlugInCollection _activeMousePlugInCollection;

	private StylusPointDescription _mousePointDescription;

	private EventHandler _shutdownHandler;

	private bool _tabletDeviceCollectionDisposed;

	private WispTabletDeviceCollection _tabletDeviceCollection;

	private WispStylusDevice _currentStylusDevice;

	private int _lastInRangeTime;

	private bool _triedDeferringMouseMove;

	private RawMouseInputReport _deferredMouseMove;

	private DispatcherOperationCallback _processDeferredMouseMove;

	private RawMouseInputReport _mouseDeactivateInputReport;

	private bool _inputEnabled;

	private bool _updatingScreenMeasurements;

	private DispatcherOperationCallback _processDisplayChanged;

	private readonly object __penContextsLock = new object();

	private Dictionary<object, PenContexts> __penContextsMap = new Dictionary<object, PenContexts>(2);

	private readonly object __stylusDeviceLock = new object();

	private Dictionary<int, StylusDevice> __stylusDeviceMap = new Dictionary<int, StylusDevice>(2);

	private bool _inDragDrop;

	private bool _leavingDragDrop;

	private bool _processingQueuedEvent;

	private bool _stylusDeviceInRange;

	private bool _seenRealMouseActivate;

	private int _lastKnownDeviceCount = -1;

	private Dictionary<StylusDeviceBase, RawStylusInputReport> _lastMovesQueued = new Dictionary<StylusDeviceBase, RawStylusInputReport>();

	private Dictionary<StylusDeviceBase, RawStylusInputReport> _coalescedMoves = new Dictionary<StylusDeviceBase, RawStylusInputReport>();

	private readonly object _coalesceLock = new object();

	private IInputElement _stylusCapture;

	private IInputElement _stylusOver;

	private DeferredElementTreeState _stylusOverTreeState;

	private DeferredElementTreeState _stylusCaptureWithinTreeState;

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

	internal object CurrentMousePromotionStylusDevice { get; set; }

	internal StylusPointDescription GetMousePointDescription
	{
		get
		{
			if (_mousePointDescription == null)
			{
				_mousePointDescription = new StylusPointDescription(new StylusPointPropertyInfo[6]
				{
					StylusPointPropertyInfoDefaults.X,
					StylusPointPropertyInfoDefaults.Y,
					StylusPointPropertyInfoDefaults.NormalPressure,
					StylusPointPropertyInfoDefaults.PacketStatus,
					StylusPointPropertyInfoDefaults.TipButton,
					StylusPointPropertyInfoDefaults.BarrelButton
				}, -1);
			}
			return _mousePointDescription;
		}
	}

	internal int DoubleTapDelta
	{
		get
		{
			if (!IsTouchStylusDevice(_currentStylusDevice))
			{
				return _stylusDoubleTapDelta;
			}
			return _touchDoubleTapDelta;
		}
	}

	internal int DoubleTapDeltaTime
	{
		get
		{
			if (!IsTouchStylusDevice(_currentStylusDevice))
			{
				return _stylusDoubleTapDeltaTime;
			}
			return _touchDoubleTapDeltaTime;
		}
	}

	internal int CancelDelta => _cancelDelta;

	internal override TabletDeviceCollection TabletDevices => WispTabletDevices;

	internal WispTabletDeviceCollection WispTabletDevices
	{
		get
		{
			if (_tabletDeviceCollection == null)
			{
				_tabletDeviceCollection = new WispTabletDeviceCollection();
				_inputManager.Value.Dispatcher.ShutdownFinished += _shutdownHandler;
			}
			return _tabletDeviceCollection;
		}
	}

	internal override StylusDeviceBase CurrentStylusDevice => _currentStylusDevice;

	internal bool Enabled => _inputEnabled;

	private DeferredElementTreeState StylusOverTreeState
	{
		get
		{
			if (_stylusOverTreeState == null)
			{
				_stylusOverTreeState = new DeferredElementTreeState();
			}
			return _stylusOverTreeState;
		}
	}

	private DeferredElementTreeState StylusCaptureWithinTreeState
	{
		get
		{
			if (_stylusCaptureWithinTreeState == null)
			{
				_stylusCaptureWithinTreeState = new DeferredElementTreeState();
			}
			return _stylusCaptureWithinTreeState;
		}
	}

	internal WispLogic(InputManager inputManager)
	{
		base.Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.WispStackEnabled;
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
		_shutdownHandler = OnDispatcherShutdown;
		_processDisplayChanged = ProcessDisplayChanged;
		_processDeferredMouseMove = ProcessDeferredMouseMove;
		ReadSystemConfig();
		_dlgInputManagerProcessInput = InputManagerProcessInput;
	}

	private void OnDispatcherShutdown(object sender, EventArgs e)
	{
		if (_shutdownHandler != null)
		{
			_inputManager.Value.Dispatcher.ShutdownFinished -= _shutdownHandler;
		}
		if (_tabletDeviceCollection != null)
		{
			_tabletDeviceCollection.DisposeTablets();
			_tabletDeviceCollection = null;
			_tabletDeviceCollectionDisposed = true;
		}
		_currentStylusDevice = null;
	}

	internal void ProcessSystemEvent(PenContext penContext, int tabletDeviceId, int stylusDeviceId, int timestamp, SystemGesture systemGesture, int gestureX, int gestureY, int buttonState, PresentationSource inputSource)
	{
		if (systemGesture == SystemGesture.Tap || systemGesture == SystemGesture.RightTap || systemGesture == SystemGesture.Drag || systemGesture == SystemGesture.RightDrag || systemGesture == SystemGesture.HoldEnter || systemGesture == SystemGesture.HoldLeave || systemGesture == SystemGesture.HoverEnter || systemGesture == SystemGesture.HoverLeave || systemGesture == SystemGesture.Flick || systemGesture == (SystemGesture)17 || systemGesture == SystemGesture.None)
		{
			RawStylusSystemGestureInputReport inputReport = new RawStylusSystemGestureInputReport(InputMode.Foreground, timestamp, inputSource, penContext, tabletDeviceId, stylusDeviceId, systemGesture, gestureX, gestureY, buttonState);
			ProcessInputReport(inputReport);
		}
	}

	internal void ProcessInput(RawStylusActions actions, PenContext penContext, int tabletDeviceId, int stylusDeviceId, int[] data, int timestamp, PresentationSource inputSource)
	{
		RawStylusInputReport inputReport = new RawStylusInputReport(InputMode.Foreground, timestamp, inputSource, penContext, actions, tabletDeviceId, stylusDeviceId, data);
		ProcessInputReport(inputReport);
	}

	private void CoalesceAndQueueStylusEvent(RawStylusInputReport inputReport)
	{
		StylusDeviceBase stylusDeviceBase = inputReport?.StylusDevice?.StylusDeviceImpl;
		if (stylusDeviceBase == null)
		{
			return;
		}
		RawStylusInputReport value = null;
		RawStylusInputReport value2 = null;
		lock (_coalesceLock)
		{
			_lastMovesQueued.TryGetValue(stylusDeviceBase, out value);
			_coalescedMoves.TryGetValue(stylusDeviceBase, out value2);
			if (inputReport.Actions == RawStylusActions.Move)
			{
				if (value2 == null)
				{
					_coalescedMoves[stylusDeviceBase] = inputReport;
					value2 = inputReport;
				}
				else
				{
					int[] rawPacketData = value2.GetRawPacketData();
					int[] rawPacketData2 = inputReport.GetRawPacketData();
					int[] array = new int[rawPacketData.Length + rawPacketData2.Length];
					rawPacketData.CopyTo(array, 0);
					rawPacketData2.CopyTo(array, rawPacketData.Length);
					value2 = new RawStylusInputReport(value2.Mode, value2.Timestamp, value2.InputSource, value2.PenContext, value2.Actions, value2.TabletDeviceId, value2.StylusDeviceId, array);
					value2.StylusDevice = stylusDeviceBase.StylusDevice;
					_coalescedMoves[stylusDeviceBase] = value2;
				}
				if (value != null && value.IsQueued)
				{
					return;
				}
			}
			if (value2 != null)
			{
				QueueStylusEvent(value2);
				_lastMovesQueued[stylusDeviceBase] = value2;
				_coalescedMoves.Remove(stylusDeviceBase);
			}
			if (inputReport.Actions != RawStylusActions.Move)
			{
				QueueStylusEvent(inputReport);
				_lastMovesQueued.Remove(stylusDeviceBase);
			}
		}
	}

	private void ProcessInputReport(RawStylusInputReport inputReport)
	{
		inputReport.StylusDevice = FindStylusDeviceWithLock(inputReport.StylusDeviceId)?.StylusDevice;
		if (!_inDragDrop || !inputReport.PenContext.Contexts.IsWindowDisabled)
		{
			InvokeStylusPluginCollection(inputReport);
		}
		CoalesceAndQueueStylusEvent(inputReport);
	}

	private void QueueStylusEvent(RawStylusInputReport report)
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordInput, EventTrace.Level.Info, EventTrace.Event.StylusEventQueued, report.StylusDeviceId);
		report.IsQueued = true;
		lock (_stylusEventQueueLock)
		{
			if (report.StylusDevice != null)
			{
				WispTabletDevice wispTabletDevice = report.StylusDevice.TabletDevice.As<WispTabletDevice>();
				if (wispTabletDevice != null)
				{
					wispTabletDevice.QueuedEventCount++;
				}
			}
			_queueStylusEvents.Enqueue(report);
		}
		base.Dispatcher.BeginInvoke(DispatcherPriority.Input, _dlgInputManagerProcessInput, null);
	}

	internal object InputManagerProcessInput(object oInput)
	{
		RawStylusInputReport rawStylusInputReport = null;
		WispTabletDevice wispTabletDevice = null;
		lock (_stylusEventQueueLock)
		{
			if (_queueStylusEvents.Count > 0)
			{
				rawStylusInputReport = _queueStylusEvents.Dequeue();
				wispTabletDevice = rawStylusInputReport?.StylusDevice?.TabletDevice?.As<WispTabletDevice>();
				if (wispTabletDevice != null)
				{
					wispTabletDevice.QueuedEventCount--;
				}
			}
		}
		if (rawStylusInputReport != null && rawStylusInputReport.StylusDevice != null && rawStylusInputReport.StylusDevice.IsValid)
		{
			rawStylusInputReport.IsQueued = false;
			PenContext penContext = rawStylusInputReport.PenContext;
			if (wispTabletDevice != null && penContext.UpdateScreenMeasurementsPending)
			{
				bool num = wispTabletDevice.AreSizeDeltasValid();
				penContext.UpdateScreenMeasurementsPending = false;
				wispTabletDevice.UpdateScreenMeasurements();
				if (num)
				{
					wispTabletDevice.UpdateSizeDeltas(penContext.StylusPointDescription, this);
				}
			}
			InputReportEventArgs inputReportEventArgs = new InputReportEventArgs(null, rawStylusInputReport);
			inputReportEventArgs.RoutedEvent = InputManager.PreviewInputReportEvent;
			_processingQueuedEvent = true;
			try
			{
				InputManagerProcessInputEventArgs(inputReportEventArgs);
			}
			finally
			{
				_processingQueuedEvent = false;
			}
		}
		return null;
	}

	internal void InputManagerProcessInputEventArgs(InputEventArgs input)
	{
		_inputManager.Value.ProcessInput(input);
	}

	private bool DeferMouseMove(RawMouseInputReport mouseInputReport)
	{
		if (!_triedDeferringMouseMove)
		{
			if (_deferredMouseMove != null)
			{
				return false;
			}
			_deferredMouseMove = mouseInputReport;
			base.Dispatcher.BeginInvoke(DispatcherPriority.Background, _processDeferredMouseMove, null);
			return true;
		}
		return false;
	}

	internal object ProcessDeferredMouseMove(object oInput)
	{
		if (_deferredMouseMove != null)
		{
			if (CurrentStylusDevice == null || !CurrentStylusDevice.InRange)
			{
				SendDeferredMouseEvent(sendInput: true);
			}
			else
			{
				SendDeferredMouseEvent(sendInput: false);
			}
		}
		return null;
	}

	private void SendDeferredMouseEvent(bool sendInput)
	{
		if (sendInput)
		{
			_triedDeferringMouseMove = true;
			if (_deferredMouseMove != null && _deferredMouseMove.InputSource != null && _deferredMouseMove.InputSource.CompositionTarget != null && !_deferredMouseMove.InputSource.CompositionTarget.IsDisposed)
			{
				InputReportEventArgs inputReportEventArgs = new InputReportEventArgs(_inputManager.Value.PrimaryMouseDevice, _deferredMouseMove);
				inputReportEventArgs.RoutedEvent = InputManager.PreviewInputReportEvent;
				_deferredMouseMove = null;
				_inputManager.Value.ProcessInput(inputReportEventArgs);
			}
		}
		_deferredMouseMove = null;
	}

	private void PreProcessInput(object sender, PreProcessInputEventArgs e)
	{
		if (!_inputEnabled || e.StagingItem.Input.RoutedEvent != InputManager.PreviewInputReportEvent || !(e.StagingItem.Input is InputReportEventArgs { Handled: false } inputReportEventArgs))
		{
			return;
		}
		if (_inDragDrop != _inputManager.Value.InDragDrop)
		{
			_inDragDrop = _inputManager.Value.InDragDrop;
			if (!_inDragDrop && _stylusDeviceInRange)
			{
				UpdateMouseState();
				_leavingDragDrop = true;
			}
		}
		if (inputReportEventArgs.Report.Type == InputType.Mouse)
		{
			if (!(inputReportEventArgs.Device is StylusDevice))
			{
				if (!_tabletDeviceCollectionDisposed && TabletDevices.Count != 0)
				{
					RawMouseInputReport rawMouseInputReport = (RawMouseInputReport)inputReportEventArgs.Report;
					RawMouseActions actions = rawMouseInputReport.Actions;
					int num = MS.Win32.NativeMethods.IntPtrToInt32(rawMouseInputReport.ExtraInformation);
					bool flag = StylusLogic.IsPromotedMouseEvent(rawMouseInputReport);
					if (flag)
					{
						_lastMouseMoveFromStylus = true;
						_lastStylusDeviceId = num & 0xFF;
					}
					if ((actions & RawMouseActions.Deactivate) == RawMouseActions.Deactivate)
					{
						_seenRealMouseActivate = false;
						if (CurrentStylusDevice != null)
						{
							PenContexts penContextsFromHwnd = GetPenContextsFromHwnd(rawMouseInputReport.InputSource);
							if (_stylusDeviceInRange && !_inDragDrop && (penContextsFromHwnd == null || !penContextsFromHwnd.IsWindowDisabled))
							{
								_mouseDeactivateInputReport = rawMouseInputReport;
								e.Cancel();
								inputReportEventArgs.Handled = true;
							}
							else if (CurrentStylusDevice.DirectlyOver != null && _inputManager.Value.PrimaryMouseDevice.CriticalActiveSource == rawMouseInputReport.InputSource)
							{
								_currentStylusDevice.ChangeStylusOver(null);
							}
						}
					}
					else if ((actions & RawMouseActions.CancelCapture) != 0)
					{
						if (CurrentStylusDevice != null && CurrentStylusDevice.InRange)
						{
							RawMouseInputReport report = new RawMouseInputReport(rawMouseInputReport.Mode, rawMouseInputReport.Timestamp, rawMouseInputReport.InputSource, rawMouseInputReport.Actions, 0, 0, 0, IntPtr.Zero);
							InputReportEventArgs inputReportEventArgs2 = new InputReportEventArgs(CurrentStylusDevice.StylusDevice, report);
							inputReportEventArgs2.RoutedEvent = InputManager.PreviewInputReportEvent;
							e.Cancel();
							_inputManager.Value.ProcessInput(inputReportEventArgs2);
						}
					}
					else if ((actions & RawMouseActions.Activate) != 0)
					{
						_mouseDeactivateInputReport = null;
						WispStylusDevice wispStylusDevice = null;
						_seenRealMouseActivate = true;
						if (CurrentStylusDevice != null && CurrentStylusDevice.InRange)
						{
							wispStylusDevice = _currentStylusDevice;
						}
						else if (flag || ShouldConsiderStylusInRange(rawMouseInputReport))
						{
							wispStylusDevice = FindStylusDevice(_lastStylusDeviceId);
						}
						if (wispStylusDevice != null)
						{
							if (rawMouseInputReport.InputSource != _inputManager.Value.PrimaryMouseDevice.CriticalActiveSource)
							{
								Point lastMouseScreenPoint = wispStylusDevice.LastMouseScreenPoint;
								lastMouseScreenPoint = PointUtil.ScreenToClient(lastMouseScreenPoint, rawMouseInputReport.InputSource);
								RawMouseInputReport report2 = new RawMouseInputReport(rawMouseInputReport.Mode, rawMouseInputReport.Timestamp, rawMouseInputReport.InputSource, RawMouseActions.Activate, (int)lastMouseScreenPoint.X, (int)lastMouseScreenPoint.Y, rawMouseInputReport.Wheel, rawMouseInputReport.ExtraInformation);
								InputReportEventArgs inputReportEventArgs3 = new InputReportEventArgs(wispStylusDevice.StylusDevice, report2);
								inputReportEventArgs3.RoutedEvent = InputManager.PreviewInputReportEvent;
								_inputManager.Value.ProcessInput(inputReportEventArgs3);
							}
							e.Cancel();
						}
					}
					else if ((actions & (RawMouseActions.AbsoluteMove | RawMouseActions.Button1Press | RawMouseActions.Button1Release | RawMouseActions.Button2Press | RawMouseActions.Button2Release | RawMouseActions.QueryCursor)) != 0)
					{
						if ((actions & RawMouseActions.Button1Press) != 0 && CurrentStylusDevice != null && !CurrentStylusDevice.InAir)
						{
							HwndSource hwndSource = rawMouseInputReport.InputSource as HwndSource;
							nint num2 = hwndSource?.CriticalHandle ?? IntPtr.Zero;
							if (num2 != IntPtr.Zero && _inputManager.Value.PrimaryMouseDevice.Captured != null && MS.Win32.UnsafeNativeMethods.GetParent(new HandleRef(this, num2)) == IntPtr.Zero && num2 != MS.Win32.UnsafeNativeMethods.GetForegroundWindow() && (MS.Win32.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, num2), -20) & 0x8000000) == 0)
							{
								MS.Win32.UnsafeNativeMethods.SetForegroundWindow(new HandleRef(this, hwndSource.Handle));
							}
							if (!_currentStylusDevice.SentMouseDown && flag && ShouldPromoteToMouse(_currentStylusDevice))
							{
								WispStylusTouchDevice touchDevice = _currentStylusDevice.TouchDevice;
								if (touchDevice.PromotingToManipulation)
								{
									touchDevice.StoredStagingAreaItems.AddItem(e.StagingItem);
								}
								else if (touchDevice.PromotingToOther)
								{
									_currentStylusDevice.PlayBackCachedDownInputReport(rawMouseInputReport.Timestamp);
								}
							}
						}
						if (flag)
						{
							bool flag2 = true;
							Point ptClient = new Point(rawMouseInputReport.X, rawMouseInputReport.Y);
							if ((CurrentStylusDevice == null || CurrentStylusDevice.InAir) && Mouse.Captured != null && !InWindowClientRect(ptClient, rawMouseInputReport.InputSource))
							{
								flag2 = false;
							}
							if (flag2)
							{
								if ((actions & (RawMouseActions.Button1Press | RawMouseActions.Button2Press)) == 0)
								{
									inputReportEventArgs.Handled = true;
								}
								e.Cancel();
								if ((actions & RawMouseActions.Button1Press) != 0 && CurrentStylusDevice != null && CurrentStylusDevice.InAir)
								{
									_currentStylusDevice.SetSawMouseButton1Down(sawMouseButton1Down: true);
								}
								if (!_processingQueuedEvent)
								{
									InputManagerProcessInput(null);
								}
							}
							return;
						}
						bool flag3 = false;
						bool flag4 = true;
						if (_stylusDeviceInRange)
						{
							flag3 = true;
							if ((actions & (RawMouseActions.Button1Press | RawMouseActions.Button2Press)) == 0)
							{
								flag4 = false;
							}
						}
						else if ((actions & ~(RawMouseActions.AbsoluteMove | RawMouseActions.QueryCursor)) == 0)
						{
							if (DeferMouseMove(rawMouseInputReport))
							{
								flag3 = true;
							}
							else if (_lastMouseMoveFromStylus && ShouldConsiderStylusInRange(rawMouseInputReport))
							{
								SendDeferredMouseEvent(sendInput: false);
								flag3 = true;
							}
							else
							{
								_lastMouseMoveFromStylus = false;
								if (!_triedDeferringMouseMove)
								{
									SendDeferredMouseEvent(sendInput: true);
								}
								if (CurrentStylusDevice != null)
								{
									SelectStylusDevice(null, null, updateOver: true);
								}
							}
						}
						else
						{
							_lastMouseMoveFromStylus = false;
							SendDeferredMouseEvent(sendInput: true);
							if (CurrentStylusDevice != null)
							{
								SelectStylusDevice(null, null, updateOver: true);
							}
						}
						if (flag3)
						{
							e.Cancel();
							if (flag4)
							{
								inputReportEventArgs.Handled = true;
							}
						}
					}
					else if (!_stylusDeviceInRange)
					{
						_lastMouseMoveFromStylus = false;
						SendDeferredMouseEvent(sendInput: true);
						if (CurrentStylusDevice != null)
						{
							SelectStylusDevice(null, null, updateOver: true);
						}
					}
					else
					{
						SendDeferredMouseEvent(sendInput: true);
					}
				}
				else
				{
					_lastMouseMoveFromStylus = false;
				}
			}
			else
			{
				_lastMouseMoveFromStylus = true;
				RawMouseInputReport rawMouseInputReport2 = (RawMouseInputReport)inputReportEventArgs.Report;
				if (!((StylusDevice)inputReportEventArgs.Device).InRange && rawMouseInputReport2._isSynchronize)
				{
					e.Cancel();
					inputReportEventArgs.Handled = true;
				}
			}
		}
		else
		{
			if (inputReportEventArgs.Report.Type != InputType.Stylus)
			{
				return;
			}
			RawStylusInputReport rawStylusInputReport = (RawStylusInputReport)inputReportEventArgs.Report;
			WispStylusDevice wispStylusDevice2 = rawStylusInputReport?.StylusDevice?.As<WispStylusDevice>();
			bool flag5 = true;
			if (rawStylusInputReport.InputSource != null && rawStylusInputReport.PenContext != null)
			{
				if (wispStylusDevice2 == null)
				{
					wispStylusDevice2 = FindStylusDevice(rawStylusInputReport.StylusDeviceId);
					if (wispStylusDevice2 == null)
					{
						wispStylusDevice2 = WispTabletDevices.UpdateStylusDevices(rawStylusInputReport.TabletDeviceId, rawStylusInputReport.StylusDeviceId);
					}
					rawStylusInputReport.StylusDevice = wispStylusDevice2.StylusDevice;
				}
				_triedDeferringMouseMove = false;
				if (rawStylusInputReport.Actions == RawStylusActions.InRange && rawStylusInputReport.Data == null)
				{
					rawStylusInputReport.PenContext.DecrementQueuedInRangeCount();
					e.Cancel();
					inputReportEventArgs.Handled = true;
					_lastInRangeTime = Environment.TickCount;
					return;
				}
				if (rawStylusInputReport.Actions == RawStylusActions.SystemGesture && wispStylusDevice2 != null && ((RawStylusSystemGestureInputReport)rawStylusInputReport).SystemGesture == (SystemGesture)17)
				{
					wispStylusDevice2.SeenDoubleTapGesture = true;
					e.Cancel();
					inputReportEventArgs.Handled = true;
					return;
				}
				if (wispStylusDevice2 != null && IsValidStylusAction(rawStylusInputReport))
				{
					flag5 = false;
					WispTabletDevice wispTabletDevice = wispStylusDevice2.TabletDevice?.As<WispTabletDevice>();
					if (wispTabletDevice != null)
					{
						SystemGesture? systemGesture = wispTabletDevice.GenerateStaticGesture(rawStylusInputReport);
						if (systemGesture.HasValue)
						{
							GenerateGesture(rawStylusInputReport, systemGesture.Value);
						}
					}
					if (rawStylusInputReport.Actions == RawStylusActions.Up)
					{
						if (!wispStylusDevice2.GestureWasFired)
						{
							GenerateGesture(rawStylusInputReport, wispStylusDevice2.LastTapBarrelDown ? SystemGesture.RightTap : SystemGesture.Tap);
						}
						if (!_inDragDrop && !rawStylusInputReport.PenContext.Contexts.IsWindowDisabled)
						{
							ProcessMouseMove(wispStylusDevice2, rawStylusInputReport.Timestamp, isSynchronize: false);
						}
					}
					inputReportEventArgs.Device = wispStylusDevice2.StylusDevice;
				}
			}
			if (flag5)
			{
				e.Cancel();
			}
		}
	}

	private void PreNotifyInput(object sender, NotifyInputEventArgs e)
	{
		if (e.StagingItem.Input.RoutedEvent == InputManager.PreviewInputReportEvent)
		{
			InputReportEventArgs inputReportEventArgs = e.StagingItem.Input as InputReportEventArgs;
			if (!inputReportEventArgs.Handled && inputReportEventArgs.Report.Type == InputType.Stylus)
			{
				RawStylusInputReport rawStylusInputReport = (RawStylusInputReport)inputReportEventArgs.Report;
				WispStylusDevice wispStylusDevice = rawStylusInputReport.StylusDevice?.As<WispStylusDevice>();
				if (wispStylusDevice != null && wispStylusDevice.IsValid)
				{
					switch (rawStylusInputReport.Actions)
					{
					case RawStylusActions.SystemGesture:
						wispStylusDevice.UpdateStateForSystemGesture((RawStylusSystemGestureInputReport)rawStylusInputReport);
						break;
					case RawStylusActions.OutOfRange:
						_lastInRangeTime = Environment.TickCount;
						wispStylusDevice.UpdateInRange(inRange: false, rawStylusInputReport.PenContext);
						UpdateIsStylusInRange(forceInRange: false);
						break;
					case RawStylusActions.InRange:
						_lastInRangeTime = Environment.TickCount;
						wispStylusDevice.UpdateInRange(inRange: true, rawStylusInputReport.PenContext);
						wispStylusDevice.UpdateState(rawStylusInputReport);
						UpdateIsStylusInRange(forceInRange: true);
						break;
					default:
						wispStylusDevice.UpdateState(rawStylusInputReport);
						break;
					}
					if (!_inDragDrop && !rawStylusInputReport.PenContext.Contexts.IsWindowDisabled && !wispStylusDevice.IgnoreStroke)
					{
						Point rawPosition = wispStylusDevice.GetRawPosition(null);
						rawPosition = DeviceUnitsFromMeasureUnits(wispStylusDevice.CriticalActiveSource, rawPosition);
						IInputElement newOver = wispStylusDevice.FindTarget(wispStylusDevice.CriticalActiveSource, rawPosition);
						SelectStylusDevice(wispStylusDevice, newOver, updateOver: true);
					}
					else
					{
						SelectStylusDevice(wispStylusDevice, null, updateOver: false);
					}
					if (rawStylusInputReport.Actions == RawStylusActions.Down && wispStylusDevice.Target == null)
					{
						wispStylusDevice.IgnoreStroke = true;
					}
					_inputManager.Value.MostRecentInputDevice = wispStylusDevice.StylusDevice;
					VerifyStylusPlugInCollectionTarget(rawStylusInputReport);
				}
			}
		}
		if (e.StagingItem.Input.RoutedEvent != Stylus.PreviewStylusDownEvent)
		{
			return;
		}
		StylusEventArgs stylusEventArgs = e.StagingItem.Input as StylusDownEventArgs;
		WispStylusDevice wispStylusDevice2 = stylusEventArgs.StylusDeviceImpl.As<WispStylusDevice>();
		if (wispStylusDevice2 != null && wispStylusDevice2.IsValid)
		{
			Point rawPosition2 = wispStylusDevice2.GetRawPosition(null);
			WispTabletDevice wispTabletDevice = wispStylusDevice2.TabletDevice.As<WispTabletDevice>();
			bool flag = false;
			int buttonBitPosition = wispTabletDevice.StylusPointDescription.GetButtonBitPosition(StylusPointProperties.BarrelButton);
			if (buttonBitPosition != -1 && wispStylusDevice2.StylusButtons[buttonBitPosition].StylusButtonState == StylusButtonState.Down)
			{
				flag = true;
			}
			Point point = DeviceUnitsFromMeasureUnits(wispStylusDevice2.CriticalActiveSource, rawPosition2);
			Point point2 = DeviceUnitsFromMeasureUnits(wispStylusDevice2.CriticalActiveSource, wispStylusDevice2.LastTapPoint);
			int num = Math.Abs(stylusEventArgs.Timestamp - wispStylusDevice2.LastTapTime);
			Size doubleTapSize = wispTabletDevice.DoubleTapSize;
			bool flag2 = Math.Abs(point.X - point2.X) < doubleTapSize.Width && Math.Abs(point.Y - point2.Y) < doubleTapSize.Height;
			if (num < DoubleTapDeltaTime && flag2 && flag == wispStylusDevice2.LastTapBarrelDown)
			{
				wispStylusDevice2.TapCount++;
			}
			else
			{
				wispStylusDevice2.TapCount = 1;
				wispStylusDevice2.LastTapPoint = new Point(rawPosition2.X, rawPosition2.Y);
				wispStylusDevice2.LastTapTime = stylusEventArgs.Timestamp;
				wispStylusDevice2.LastTapBarrelDown = flag;
			}
			ProcessMouseMove(wispStylusDevice2, stylusEventArgs.Timestamp, isSynchronize: true);
		}
	}

	private void PostProcessInput(object sender, ProcessInputEventArgs e)
	{
		if (_inputEnabled && (e.StagingItem.Input.RoutedEvent == Mouse.LostMouseCaptureEvent || e.StagingItem.Input.RoutedEvent == Mouse.GotMouseCaptureEvent))
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
				WispStylusDevice wispStylusDevice = rawStylusInputReport.StylusDevice.As<WispStylusDevice>();
				if (!_inDragDrop)
				{
					if (!rawStylusInputReport.PenContext.Contexts.IsWindowDisabled)
					{
						PromoteRawToPreview(rawStylusInputReport, e);
						if (rawStylusInputReport.Actions == RawStylusActions.Up)
						{
							wispStylusDevice.IgnoreStroke = false;
						}
					}
					else if ((rawStylusInputReport.Actions & RawStylusActions.Up) != 0 && wispStylusDevice != null)
					{
						wispStylusDevice.ResetStateForStylusUp();
						WispStylusTouchDevice touchDevice = wispStylusDevice.TouchDevice;
						if (touchDevice.IsActive)
						{
							touchDevice.OnDeactivate();
						}
					}
				}
				else if (wispStylusDevice != null && wispStylusDevice != CurrentMousePromotionStylusDevice && (rawStylusInputReport.Actions & RawStylusActions.Up) != 0)
				{
					WispStylusTouchDevice touchDevice2 = wispStylusDevice.TouchDevice;
					if (touchDevice2.IsActive)
					{
						touchDevice2.OnDeactivate();
					}
				}
			}
		}
		if (e.StagingItem.Input.RoutedEvent == Stylus.StylusOutOfRangeEvent)
		{
			RawMouseInputReport mouseDeactivateInputReport = _mouseDeactivateInputReport;
			_mouseDeactivateInputReport = null;
			StylusEventArgs stylusEventArgs = (StylusEventArgs)e.StagingItem.Input;
			PresentationSource criticalActiveSource = _inputManager.Value.PrimaryMouseDevice.CriticalActiveSource;
			if (mouseDeactivateInputReport != null || (!_seenRealMouseActivate && criticalActiveSource != null))
			{
				WispStylusDevice wispStylusDevice2 = stylusEventArgs.StylusDeviceImpl.As<WispStylusDevice>();
				wispStylusDevice2.ChangeStylusOver(null);
				InputReportEventArgs inputReportEventArgs2 = new InputReportEventArgs(report: (mouseDeactivateInputReport != null) ? new RawMouseInputReport(mouseDeactivateInputReport.Mode, stylusEventArgs.Timestamp, mouseDeactivateInputReport.InputSource, mouseDeactivateInputReport.Actions, mouseDeactivateInputReport.X, mouseDeactivateInputReport.Y, mouseDeactivateInputReport.Wheel, mouseDeactivateInputReport.ExtraInformation) : new RawMouseInputReport(InputMode.Foreground, stylusEventArgs.Timestamp, criticalActiveSource, RawMouseActions.Deactivate, 0, 0, 0, IntPtr.Zero), inputDevice: wispStylusDevice2.StylusDevice);
				inputReportEventArgs2.RoutedEvent = InputManager.PreviewInputReportEvent;
				_inputManager.Value.ProcessInput(inputReportEventArgs2);
			}
		}
		CallPlugInsForMouse(e);
		PromotePreviewToMain(e);
		UpdateButtonStates(e);
		PromoteMainToOther(e);
		if (e.StagingItem.Input.RoutedEvent == Stylus.StylusMoveEvent)
		{
			StylusEventArgs stylusEventArgs2 = (StylusEventArgs)e.StagingItem.Input;
			WispStylusDevice wispStylusDevice3 = stylusEventArgs2.StylusDeviceImpl.As<WispStylusDevice>();
			if (wispStylusDevice3.SeenDoubleTapGesture && !wispStylusDevice3.GestureWasFired && wispStylusDevice3.DetectedDrag)
			{
				GenerateGesture(stylusEventArgs2.InputReport, SystemGesture.Drag);
			}
		}
		if (e.StagingItem.Input.RoutedEvent == Stylus.StylusSystemGestureEvent)
		{
			StylusSystemGestureEventArgs stylusSystemGestureEventArgs = (StylusSystemGestureEventArgs)e.StagingItem.Input;
			if (stylusSystemGestureEventArgs.SystemGesture == SystemGesture.Flick)
			{
				HandleFlick(stylusSystemGestureEventArgs.ButtonState, stylusSystemGestureEventArgs.StylusDevice.DirectlyOver);
			}
		}
		if (e.StagingItem.Input.RoutedEvent == Stylus.StylusOutOfRangeEvent)
		{
			WispTabletDevice wispTabletDevice = (e.StagingItem.Input as StylusEventArgs)?.StylusDeviceImpl?.TabletDevice.As<WispTabletDevice>();
			if (wispTabletDevice.IsDisposalPending && wispTabletDevice.CanDispose)
			{
				RefreshTablets();
			}
		}
	}

	private void PromoteRawToPreview(RawStylusInputReport report, ProcessInputEventArgs e)
	{
		RoutedEvent previewEventFromRawStylusActions = StylusLogic.GetPreviewEventFromRawStylusActions(report.Actions);
		if (previewEventFromRawStylusActions != null && report.StylusDevice != null && !report.StylusDevice.As<WispStylusDevice>().IgnoreStroke)
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
		else
		{
			StylusEventArgs stylusEventArgs3 = e.StagingItem.Input as StylusEventArgs;
			if (stylusEventArgs3?.RoutedEvent == Stylus.PreviewStylusUpEvent && stylusEventArgs3.StylusDeviceImpl.As<WispStylusDevice>().TouchDevice.IsActive)
			{
				stylusEventArgs3.StylusDeviceImpl.As<WispStylusDevice>().TouchDevice.OnDeactivate();
			}
		}
	}

	private void PromoteMainToOther(ProcessInputEventArgs e)
	{
		StagingAreaInputItem stagingItem = e.StagingItem;
		if (!(stagingItem.Input is StylusEventArgs stylusEventArgs))
		{
			return;
		}
		WispStylusDevice wispStylusDevice = stylusEventArgs.StylusDeviceImpl.As<WispStylusDevice>();
		WispStylusTouchDevice touchDevice = wispStylusDevice.TouchDevice;
		bool flag = ShouldPromoteToMouse(wispStylusDevice);
		if (IsTouchPromotionEvent(stylusEventArgs))
		{
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
		else if (e.StagingItem.Input.RoutedEvent == Stylus.StylusSystemGestureEvent)
		{
			if (flag)
			{
				if (touchDevice.PromotingToManipulation)
				{
					touchDevice.StoredStagingAreaItems.AddItem(stagingItem);
				}
				else if (touchDevice.PromotingToOther)
				{
					PromoteMainToMouse(stagingItem);
				}
			}
		}
		else if (flag && touchDevice.PromotingToOther)
		{
			PromoteMainToMouse(stagingItem);
		}
	}

	private static bool IsTouchPromotionEvent(StylusEventArgs stylusEventArgs)
	{
		if (stylusEventArgs != null)
		{
			RoutedEvent routedEvent = stylusEventArgs.RoutedEvent;
			if (IsTouchStylusDevice(stylusEventArgs.StylusDeviceImpl.As<WispStylusDevice>()))
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

	private static bool IsTouchStylusDevice(WispStylusDevice stylusDevice)
	{
		if (stylusDevice != null && stylusDevice.TabletDevice != null)
		{
			return stylusDevice.TabletDevice.Type == TabletDeviceType.Touch;
		}
		return false;
	}

	private void PromoteMainToTouch(ProcessInputEventArgs e, StylusEventArgs stylusEventArgs)
	{
		WispStylusDevice wispStylusDevice = stylusEventArgs.StylusDeviceImpl.As<WispStylusDevice>();
		wispStylusDevice.UpdateTouchActiveSource();
		if (stylusEventArgs.RoutedEvent == Stylus.StylusMoveEvent)
		{
			PromoteMainMoveToTouch(wispStylusDevice, e.StagingItem);
		}
		else if (stylusEventArgs.RoutedEvent == Stylus.StylusDownEvent)
		{
			PromoteMainDownToTouch(wispStylusDevice, e.StagingItem);
		}
		else if (stylusEventArgs.RoutedEvent == Stylus.StylusUpEvent)
		{
			PromoteMainUpToTouch(wispStylusDevice, e.StagingItem);
		}
	}

	private void PromoteMainDownToTouch(WispStylusDevice stylusDevice, StagingAreaInputItem stagingItem)
	{
		WispStylusTouchDevice touchDevice = stylusDevice.TouchDevice;
		if (touchDevice.IsActive)
		{
			touchDevice.OnDeactivate();
		}
		touchDevice.OnActivate();
		bool flag = ShouldPromoteToMouse(stylusDevice);
		if (!touchDevice.OnDown() && flag)
		{
			if (touchDevice.PromotingToManipulation)
			{
				touchDevice.StoredStagingAreaItems.AddItem(stagingItem);
			}
			else if (touchDevice.PromotingToOther)
			{
				PromoteMainToMouse(stagingItem);
			}
		}
	}

	private void PromoteMainMoveToTouch(WispStylusDevice stylusDevice, StagingAreaInputItem stagingItem)
	{
		WispStylusTouchDevice touchDevice = stylusDevice.TouchDevice;
		bool flag = ShouldPromoteToMouse(stylusDevice);
		if (touchDevice.IsActive)
		{
			if (!(!touchDevice.OnMove() && flag))
			{
				return;
			}
			if (touchDevice.PromotingToManipulation)
			{
				StagingAreaInputItemList storedStagingAreaItems = touchDevice.StoredStagingAreaItems;
				int count = storedStagingAreaItems.Count;
				if (count > 0 && storedStagingAreaItems[count - 1].Input.RoutedEvent == Stylus.StylusMoveEvent)
				{
					storedStagingAreaItems[count - 1] = stagingItem;
					storedStagingAreaItems.IncrementVersion();
				}
				else
				{
					touchDevice.StoredStagingAreaItems.AddItem(stagingItem);
				}
			}
			else if (touchDevice.PromotingToOther)
			{
				PromoteMainToMouse(stagingItem);
			}
		}
		else if (flag)
		{
			PromoteMainToMouse(stagingItem);
		}
	}

	private void PromoteMainUpToTouch(WispStylusDevice stylusDevice, StagingAreaInputItem stagingItem)
	{
		WispStylusTouchDevice touchDevice = stylusDevice.TouchDevice;
		bool flag = ShouldPromoteToMouse(stylusDevice);
		if (touchDevice.IsActive)
		{
			touchDevice.OnUp();
			bool promotingToOther = touchDevice.PromotingToOther;
			if (touchDevice.IsActive)
			{
				touchDevice.OnDeactivate();
			}
			if (flag && promotingToOther && (_mouseLeftButtonState == MouseButtonState.Pressed || _mouseRightButtonState == MouseButtonState.Pressed || _leavingDragDrop))
			{
				PromoteMainToMouse(stagingItem);
			}
		}
		else if (flag)
		{
			PromoteMainToMouse(stagingItem);
		}
		_leavingDragDrop = false;
	}

	internal void PromoteStoredItemsToMouse(WispStylusTouchDevice touchDevice)
	{
		if (!ShouldPromoteToMouse(touchDevice.StylusDevice.As<WispStylusDevice>()))
		{
			return;
		}
		int count = touchDevice.StoredStagingAreaItems.Count;
		if (count <= 0)
		{
			return;
		}
		StagingAreaInputItemList storedStagingAreaItems = touchDevice.StoredStagingAreaItems;
		StagingAreaInputItem[] array = new StagingAreaInputItem[count];
		storedStagingAreaItems.CopyTo(array, 0);
		storedStagingAreaItems.Clear();
		long num = storedStagingAreaItems.IncrementVersion();
		for (int i = 0; i < count; i++)
		{
			if (num != storedStagingAreaItems.Version)
			{
				break;
			}
			StagingAreaInputItem stagingAreaInputItem = array[i];
			if (stagingAreaInputItem.Input is InputReportEventArgs inputReportEventArgs && inputReportEventArgs.Report.Type == InputType.Mouse && !(inputReportEventArgs.Device is StylusDevice))
			{
				touchDevice.StylusDevice.As<WispStylusDevice>().PlayBackCachedDownInputReport(inputReportEventArgs.Report.Timestamp);
			}
			else
			{
				PromoteMainToMouse(stagingAreaInputItem);
			}
		}
	}

	private bool ShouldPromoteToMouse(WispStylusDevice stylusDevice)
	{
		if (CurrentMousePromotionStylusDevice == null || CurrentMousePromotionStylusDevice == stylusDevice)
		{
			return true;
		}
		return false;
	}

	private void PromoteMainToMouse(StagingAreaInputItem stagingItem)
	{
		if (stagingItem.Input.Handled || !(stagingItem.Input is StylusEventArgs stylusEventArgs))
		{
			return;
		}
		WispStylusDevice wispStylusDevice = stylusEventArgs.StylusDevice.As<WispStylusDevice>();
		if (wispStylusDevice == null || IgnoreGestureToMousePromotion(stylusEventArgs as StylusSystemGestureEventArgs, wispStylusDevice.TouchDevice))
		{
			return;
		}
		RawMouseActions rawMouseActions = wispStylusDevice.GetMouseActionsFromStylusEventAndPlaybackCachedDown(stagingItem.Input.RoutedEvent, stylusEventArgs);
		if (rawMouseActions == RawMouseActions.None)
		{
			return;
		}
		PresentationSource mousePresentationSource = wispStylusDevice.GetMousePresentationSource();
		if (mousePresentationSource != null)
		{
			Point point = PointUtil.ScreenToClient(wispStylusDevice.LastMouseScreenPoint, mousePresentationSource);
			if (_inputManager.Value.PrimaryMouseDevice.CriticalActiveSource != mousePresentationSource)
			{
				rawMouseActions |= RawMouseActions.Activate;
			}
			RawMouseInputReport report = new RawMouseInputReport(InputMode.Foreground, stylusEventArgs.Timestamp, mousePresentationSource, rawMouseActions, (int)point.X, (int)point.Y, 0, IntPtr.Zero);
			InputReportEventArgs inputReportEventArgs = new InputReportEventArgs(wispStylusDevice.StylusDevice, report);
			inputReportEventArgs.RoutedEvent = InputManager.PreviewInputReportEvent;
			_inputManager.Value.ProcessInput(inputReportEventArgs);
		}
	}

	private bool IgnoreGestureToMousePromotion(StylusSystemGestureEventArgs gestureArgs, WispStylusTouchDevice touchDevice)
	{
		if (gestureArgs != null && touchDevice.DownHandled)
		{
			SystemGesture systemGesture = gestureArgs.SystemGesture;
			if (systemGesture == SystemGesture.Tap || systemGesture == SystemGesture.Drag)
			{
				return true;
			}
		}
		return false;
	}

	private void CallPlugInsForMouse(ProcessInputEventArgs e)
	{
		if (e.StagingItem.Input.Handled || (e.StagingItem.Input.RoutedEvent != Mouse.PreviewMouseDownEvent && e.StagingItem.Input.RoutedEvent != Mouse.PreviewMouseUpEvent && e.StagingItem.Input.RoutedEvent != Mouse.PreviewMouseMoveEvent && e.StagingItem.Input.RoutedEvent != InputManager.InputReportEvent))
		{
			return;
		}
		RawStylusActions actions = RawStylusActions.None;
		MouseDevice mouseDevice;
		bool flag;
		bool flag2;
		int timestamp;
		PresentationSource presentationSource;
		if (e.StagingItem.Input.RoutedEvent == InputManager.InputReportEvent)
		{
			if (_activeMousePlugInCollection == null || _activeMousePlugInCollection.Element == null)
			{
				return;
			}
			InputReportEventArgs inputReportEventArgs = e.StagingItem.Input as InputReportEventArgs;
			if (inputReportEventArgs.Report.Type != InputType.Mouse)
			{
				return;
			}
			RawMouseInputReport rawMouseInputReport = (RawMouseInputReport)inputReportEventArgs.Report;
			if ((rawMouseInputReport.Actions & RawMouseActions.Deactivate) != RawMouseActions.Deactivate)
			{
				return;
			}
			mouseDevice = _inputManager.Value.PrimaryMouseDevice;
			if (mouseDevice == null || mouseDevice.DirectlyOver != null)
			{
				return;
			}
			flag = mouseDevice.LeftButton == MouseButtonState.Pressed;
			flag2 = mouseDevice.RightButton == MouseButtonState.Pressed;
			timestamp = rawMouseInputReport.Timestamp;
			presentationSource = PresentationSource.CriticalFromVisual(_activeMousePlugInCollection.Element);
		}
		else
		{
			MouseEventArgs mouseEventArgs = e.StagingItem.Input as MouseEventArgs;
			mouseDevice = mouseEventArgs.MouseDevice;
			flag = mouseDevice.LeftButton == MouseButtonState.Pressed;
			flag2 = mouseDevice.RightButton == MouseButtonState.Pressed;
			if (mouseEventArgs.StylusDevice != null && e.StagingItem.Input.RoutedEvent != Mouse.PreviewMouseUpEvent)
			{
				return;
			}
			if (e.StagingItem.Input.RoutedEvent == Mouse.PreviewMouseMoveEvent)
			{
				if (!flag)
				{
					return;
				}
				actions = RawStylusActions.Move;
			}
			if (e.StagingItem.Input.RoutedEvent == Mouse.PreviewMouseDownEvent)
			{
				if ((mouseEventArgs as MouseButtonEventArgs).ChangedButton != 0)
				{
					return;
				}
				actions = RawStylusActions.Down;
			}
			if (e.StagingItem.Input.RoutedEvent == Mouse.PreviewMouseUpEvent)
			{
				if ((mouseEventArgs as MouseButtonEventArgs).ChangedButton != 0)
				{
					return;
				}
				actions = RawStylusActions.Up;
			}
			timestamp = mouseEventArgs.Timestamp;
			if (!(mouseDevice.DirectlyOver is Visual v))
			{
				return;
			}
			presentationSource = PresentationSource.CriticalFromVisual(v);
		}
		PenContexts penContextsFromHwnd = GetPenContextsFromHwnd(presentationSource);
		if (penContextsFromHwnd != null && presentationSource != null && presentationSource.CompositionTarget != null && !presentationSource.CompositionTarget.IsDisposed)
		{
			IInputElement directlyOver = mouseDevice.DirectlyOver;
			int num = (flag ? 1 : 0) | (flag2 ? 9 : 0);
			Point position = mouseDevice.GetPosition(presentationSource.RootVisual as IInputElement);
			position = presentationSource.CompositionTarget.TransformToDevice.Transform(position);
			int num2 = (flag ? 1 : 0) | (flag2 ? 3 : 0);
			int[] data = new int[4]
			{
				(int)position.X,
				(int)position.Y,
				num,
				num2
			};
			RawStylusInputReport inputReport = new RawStylusInputReport(InputMode.Foreground, timestamp, presentationSource, actions, () => GetMousePointDescription, 0, 0, data);
			using (base.Dispatcher.DisableProcessing())
			{
				_activeMousePlugInCollection = penContextsFromHwnd.InvokeStylusPluginCollectionForMouse(inputReport, directlyOver, _activeMousePlugInCollection);
			}
		}
	}

	internal MouseButtonState GetMouseLeftOrRightButtonState(bool leftButton)
	{
		if (leftButton)
		{
			return _mouseLeftButtonState;
		}
		return _mouseRightButtonState;
	}

	internal bool UpdateMouseButtonState(RawMouseActions actions)
	{
		bool result = false;
		switch (actions)
		{
		case RawMouseActions.Button1Press:
			if (_mouseLeftButtonState != MouseButtonState.Pressed)
			{
				result = true;
				_mouseLeftButtonState = MouseButtonState.Pressed;
			}
			break;
		case RawMouseActions.Button1Release:
			if (_mouseLeftButtonState != 0)
			{
				result = true;
				_mouseLeftButtonState = MouseButtonState.Released;
			}
			break;
		case RawMouseActions.Button2Press:
			if (_mouseRightButtonState != MouseButtonState.Pressed)
			{
				result = true;
				_mouseRightButtonState = MouseButtonState.Pressed;
			}
			break;
		case RawMouseActions.Button2Release:
			if (_mouseRightButtonState != 0)
			{
				result = true;
				_mouseRightButtonState = MouseButtonState.Released;
			}
			break;
		}
		return result;
	}

	private void UpdateMouseState()
	{
		MouseDevice primaryMouseDevice = _inputManager.Value.PrimaryMouseDevice;
		_mouseLeftButtonState = primaryMouseDevice.GetButtonStateFromSystem(MouseButton.Left);
		_mouseRightButtonState = primaryMouseDevice.GetButtonStateFromSystem(MouseButton.Right);
	}

	private void UpdateIsStylusInRange(bool forceInRange)
	{
		bool flag = false;
		if (forceInRange)
		{
			flag = true;
		}
		else
		{
			foreach (TabletDevice item in (IEnumerable)Tablet.TabletDevices)
			{
				foreach (StylusDevice stylusDevice in item.StylusDevices)
				{
					if (stylusDevice.InRange)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		_stylusDeviceInRange = flag;
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

	internal override void ReevaluateStylusOver(DependencyObject element, DependencyObject oldParent, bool isCoreParent)
	{
		if (element != null)
		{
			if (isCoreParent)
			{
				StylusOverTreeState.SetCoreParent(element, oldParent);
			}
			else
			{
				StylusOverTreeState.SetLogicalParent(element, oldParent);
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

	internal override void ReevaluateCapture(DependencyObject element, DependencyObject oldParent, bool isCoreParent)
	{
		if (element != null)
		{
			if (isCoreParent)
			{
				StylusCaptureWithinTreeState.SetCoreParent(element, oldParent);
			}
			else
			{
				StylusCaptureWithinTreeState.SetLogicalParent(element, oldParent);
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

	private bool IsValidStylusAction(RawStylusInputReport rawStylusInputReport)
	{
		bool flag = true;
		WispStylusDevice wispStylusDevice = rawStylusInputReport.StylusDevice.As<WispStylusDevice>();
		switch (rawStylusInputReport.Actions)
		{
		case RawStylusActions.InRange:
			flag = !wispStylusDevice.InRange && !rawStylusInputReport.InputSource.IsDisposed;
			break;
		case RawStylusActions.InAirMove:
			if (!wispStylusDevice.InRange && !rawStylusInputReport.InputSource.IsDisposed)
			{
				GenerateInRange(rawStylusInputReport);
			}
			else
			{
				flag = rawStylusInputReport.PenContext == wispStylusDevice.ActivePenContext;
			}
			break;
		case RawStylusActions.Down:
			if (!wispStylusDevice.InRange)
			{
				GenerateInRange(rawStylusInputReport);
			}
			else
			{
				flag = rawStylusInputReport.PenContext == wispStylusDevice.ActivePenContext;
			}
			break;
		case RawStylusActions.Move:
			flag = rawStylusInputReport.PenContext == wispStylusDevice.ActivePenContext;
			break;
		case RawStylusActions.Up:
			flag = rawStylusInputReport.PenContext == wispStylusDevice.ActivePenContext;
			break;
		case RawStylusActions.SystemGesture:
			flag = rawStylusInputReport.PenContext == wispStylusDevice.ActivePenContext;
			if (flag && ((RawStylusSystemGestureInputReport)rawStylusInputReport).SystemGesture == SystemGesture.Tap && wispStylusDevice.InAir)
			{
				flag = false;
			}
			break;
		case RawStylusActions.OutOfRange:
			flag = rawStylusInputReport.PenContext == wispStylusDevice.ActivePenContext;
			break;
		}
		return flag;
	}

	private void GenerateInRange(RawStylusInputReport rawStylusInputReport)
	{
		StylusDevice stylusDevice = rawStylusInputReport.StylusDevice;
		RawStylusInputReport report = new RawStylusInputReport(rawStylusInputReport.Mode, rawStylusInputReport.Timestamp, rawStylusInputReport.InputSource, rawStylusInputReport.PenContext, RawStylusActions.InRange, stylusDevice.TabletDevice.Id, stylusDevice.Id, rawStylusInputReport.Data);
		InputReportEventArgs inputReportEventArgs = new InputReportEventArgs(stylusDevice, report);
		inputReportEventArgs.RoutedEvent = InputManager.PreviewInputReportEvent;
		_inputManager.Value.ProcessInput(inputReportEventArgs);
	}

	internal override void HandleMessage(WindowMessage msg, nint wParam, nint lParam)
	{
		switch (msg)
		{
		case WindowMessage.WM_DEVICECHANGE:
			if (!_inputEnabled && MS.Win32.NativeMethods.IntPtrToInt32(wParam) == 7)
			{
				OnDeviceChange();
			}
			break;
		case WindowMessage.WM_DISPLAYCHANGE:
			OnScreenMeasurementsChanged();
			break;
		case WindowMessage.WM_WININICHANGE:
			ReadSystemConfig();
			if (_tabletDeviceCollection == null)
			{
				break;
			}
			{
				foreach (TabletDevice item in (IEnumerable)_tabletDeviceCollection)
				{
					item.As<WispTabletDevice>().InvalidateSizeDeltas();
				}
				break;
			}
		case WindowMessage.WM_TABLET_ADDED:
			OnTabletAdded((uint)MS.Win32.NativeMethods.IntPtrToInt32(wParam));
			break;
		case WindowMessage.WM_TABLET_DELETED:
			OnTabletRemovedImpl((uint)MS.Win32.NativeMethods.IntPtrToInt32(wParam), isInternalCall: true);
			break;
		}
	}

	internal void InvokeStylusPluginCollection(RawStylusInputReport inputReport)
	{
		if (inputReport.StylusDevice != null)
		{
			inputReport.PenContext.Contexts.InvokeStylusPluginCollection(inputReport);
		}
	}

	private void VerifyStylusPlugInCollectionTarget(RawStylusInputReport rawStylusInputReport)
	{
		RawStylusActions actions = rawStylusInputReport.Actions;
		if (actions != RawStylusActions.Down && actions != RawStylusActions.Up && actions != RawStylusActions.Move)
		{
			return;
		}
		RawStylusInput rawStylusInput = rawStylusInputReport.RawStylusInput;
		StylusPlugInCollection stylusPlugInCollection = null;
		StylusPlugInCollection stylusPlugInCollection2 = rawStylusInput?.Target;
		bool flag = false;
		if (InputElement.GetContainingUIElement(rawStylusInputReport.StylusDevice.DirectlyOver as DependencyObject) is UIElement element)
		{
			stylusPlugInCollection = rawStylusInputReport.PenContext.Contexts.FindPlugInCollection(element);
		}
		using (base.Dispatcher.DisableProcessing())
		{
			if (stylusPlugInCollection2 != null && stylusPlugInCollection2 != stylusPlugInCollection && rawStylusInput != null)
			{
				foreach (RawStylusInputCustomData customData in rawStylusInput.CustomDataList)
				{
					customData.Owner.FireCustomData(customData.Data, rawStylusInputReport.Actions, targetVerified: false);
				}
				flag = rawStylusInput.StylusPointsModified;
				rawStylusInputReport.RawStylusInput = null;
			}
			WispStylusDevice wispStylusDevice = rawStylusInputReport.StylusDevice.As<WispStylusDevice>();
			bool flag2 = false;
			if (stylusPlugInCollection != null && rawStylusInputReport.RawStylusInput == null)
			{
				GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
				generalTransformGroup.Children.Add(new MatrixTransform(GetTabletToViewTransform(wispStylusDevice.CriticalActiveSource, wispStylusDevice.TabletDevice)));
				generalTransformGroup.Children.Add(stylusPlugInCollection.ViewToElement);
				generalTransformGroup.Freeze();
				RawStylusInput rawStylusInput2 = new RawStylusInput(rawStylusInputReport, generalTransformGroup, stylusPlugInCollection);
				rawStylusInputReport.RawStylusInput = rawStylusInput2;
				flag2 = true;
			}
			StylusPlugInCollection currentVerifiedTarget = wispStylusDevice.CurrentVerifiedTarget;
			if (stylusPlugInCollection != currentVerifiedTarget)
			{
				if (currentVerifiedTarget != null)
				{
					if (rawStylusInput == null)
					{
						GeneralTransformGroup generalTransformGroup2 = new GeneralTransformGroup();
						generalTransformGroup2.Children.Add(new MatrixTransform(GetTabletToViewTransform(wispStylusDevice.CriticalActiveSource, wispStylusDevice.TabletDevice)));
						generalTransformGroup2.Children.Add(currentVerifiedTarget.ViewToElement);
						generalTransformGroup2.Freeze();
						rawStylusInput = new RawStylusInput(rawStylusInputReport, generalTransformGroup2, currentVerifiedTarget);
					}
					currentVerifiedTarget.FireEnterLeave(isEnter: false, rawStylusInput, confirmed: true);
				}
				if (stylusPlugInCollection != null)
				{
					stylusPlugInCollection.FireEnterLeave(isEnter: true, rawStylusInputReport.RawStylusInput, confirmed: true);
					base.Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.StylusPluginsUsed;
				}
				wispStylusDevice.CurrentVerifiedTarget = stylusPlugInCollection;
			}
			if (flag2)
			{
				stylusPlugInCollection.FireRawStylusInput(rawStylusInputReport.RawStylusInput);
				flag = flag || rawStylusInputReport.RawStylusInput.StylusPointsModified;
				base.Statistics.FeaturesUsed |= StylusTraceLogger.FeatureFlags.StylusPluginsUsed;
			}
			if (stylusPlugInCollection != null)
			{
				foreach (RawStylusInputCustomData customData2 in rawStylusInputReport.RawStylusInput.CustomDataList)
				{
					customData2.Owner.FireCustomData(customData2.Data, rawStylusInputReport.Actions, targetVerified: true);
				}
			}
			if (flag)
			{
				rawStylusInputReport.StylusDevice.As<WispStylusDevice>().UpdateEventStylusPoints(rawStylusInputReport, resetIfNoOverride: true);
			}
		}
	}

	private void GenerateGesture(RawStylusInputReport rawStylusInputReport, SystemGesture gesture)
	{
		StylusDevice stylusDevice = rawStylusInputReport.StylusDevice;
		RawStylusSystemGestureInputReport rawStylusSystemGestureInputReport = new RawStylusSystemGestureInputReport(InputMode.Foreground, rawStylusInputReport.Timestamp, rawStylusInputReport.InputSource, rawStylusInputReport.PenContext, rawStylusInputReport.TabletDeviceId, rawStylusInputReport.StylusDeviceId, gesture, 0, 0, 0);
		rawStylusSystemGestureInputReport.StylusDevice = stylusDevice;
		InputReportEventArgs inputReportEventArgs = new InputReportEventArgs(stylusDevice, rawStylusSystemGestureInputReport);
		inputReportEventArgs.RoutedEvent = InputManager.PreviewInputReportEvent;
		InputManagerProcessInputEventArgs(inputReportEventArgs);
	}

	private void ProcessMouseMove(WispStylusDevice stylusDevice, int timestamp, bool isSynchronize)
	{
		if (!ShouldPromoteToMouse(stylusDevice) || !stylusDevice.TouchDevice.PromotingToOther)
		{
			return;
		}
		PresentationSource mousePresentationSource = stylusDevice.GetMousePresentationSource();
		if (mousePresentationSource != null)
		{
			RawMouseActions rawMouseActions = RawMouseActions.AbsoluteMove;
			if (!isSynchronize && _inputManager.Value.PrimaryMouseDevice.CriticalActiveSource != mousePresentationSource)
			{
				rawMouseActions |= RawMouseActions.Activate;
			}
			Point lastMouseScreenPoint = stylusDevice.LastMouseScreenPoint;
			lastMouseScreenPoint = PointUtil.ScreenToClient(lastMouseScreenPoint, mousePresentationSource);
			RawMouseInputReport rawMouseInputReport = new RawMouseInputReport(InputMode.Foreground, timestamp, mousePresentationSource, rawMouseActions, (int)lastMouseScreenPoint.X, (int)lastMouseScreenPoint.Y, 0, IntPtr.Zero);
			if (isSynchronize)
			{
				rawMouseInputReport._isSynchronize = true;
			}
			InputReportEventArgs inputReportEventArgs = new InputReportEventArgs(stylusDevice.StylusDevice, rawMouseInputReport);
			inputReportEventArgs.RoutedEvent = InputManager.PreviewInputReportEvent;
			InputManagerProcessInputEventArgs(inputReportEventArgs);
		}
	}

	private void UpdateButtonStates(ProcessInputEventArgs e)
	{
		if (e.StagingItem.Input.Handled)
		{
			return;
		}
		RoutedEvent routedEvent = e.StagingItem.Input.RoutedEvent;
		if (routedEvent == null || (routedEvent != Stylus.StylusDownEvent && routedEvent != Stylus.StylusUpEvent && routedEvent != Stylus.StylusMoveEvent && routedEvent != Stylus.StylusInAirMoveEvent))
		{
			return;
		}
		RawStylusInputReport inputReport = ((StylusEventArgs)e.StagingItem.Input).InputReport;
		StylusDevice stylusDevice = inputReport.StylusDevice;
		StylusPointCollection stylusPoints = stylusDevice.GetStylusPoints(null);
		StylusPoint stylusPoint = stylusPoints[stylusPoints.Count - 1];
		foreach (StylusButton stylusButton in stylusDevice.StylusButtons)
		{
			StylusButtonState propertyValue = (StylusButtonState)stylusPoint.GetPropertyValue(new StylusPointProperty(stylusButton.Guid, isButton: true));
			if (propertyValue != stylusButton.CachedButtonState)
			{
				stylusButton.CachedButtonState = propertyValue;
				StylusButtonEventArgs stylusButtonEventArgs = new StylusButtonEventArgs(stylusDevice, inputReport.Timestamp, stylusButton);
				stylusButtonEventArgs.InputReport = inputReport;
				if (propertyValue == StylusButtonState.Down)
				{
					stylusButtonEventArgs.RoutedEvent = Stylus.PreviewStylusButtonDownEvent;
				}
				else
				{
					stylusButtonEventArgs.RoutedEvent = Stylus.PreviewStylusButtonUpEvent;
				}
				InputManagerProcessInputEventArgs(stylusButtonEventArgs);
			}
		}
	}

	private static bool InWindowClientRect(Point ptClient, PresentationSource inputSource)
	{
		bool result = false;
		if (inputSource is HwndSource { CompositionTarget: not null, IsHandleNull: false } hwndSource)
		{
			Point pointScreen = PointUtil.ClientToScreen(ptClient, hwndSource);
			nint zero = IntPtr.Zero;
			HwndSource hwndSource2 = null;
			Point point = new Point(0.0, 0.0);
			zero = MS.Win32.UnsafeNativeMethods.WindowFromPoint((int)pointScreen.X, (int)pointScreen.Y);
			if (zero != IntPtr.Zero)
			{
				hwndSource2 = HwndSource.CriticalFromHwnd(zero);
				if (hwndSource2 != null)
				{
					point = PointUtil.ScreenToClient(pointScreen, hwndSource2);
					MS.Win32.NativeMethods.RECT rect = default(MS.Win32.NativeMethods.RECT);
					SafeNativeMethods.GetClientRect(new HandleRef(hwndSource2, zero), ref rect);
					result = (int)point.X >= rect.left && (int)point.X < rect.right && (int)point.Y >= rect.top && (int)point.Y < rect.bottom;
				}
			}
		}
		return result;
	}

	internal void RegisterStylusDeviceCore(StylusDevice stylusDevice)
	{
		lock (__stylusDeviceLock)
		{
			int id = stylusDevice.Id;
			if (__stylusDeviceMap.ContainsKey(id))
			{
				throw new InvalidOperationException
				{
					Data = { 
					{
						(object)"System.Windows.Input.StylusLogic",
						(object?)""
					} }
				};
			}
			__stylusDeviceMap[id] = stylusDevice;
		}
	}

	internal void UnregisterStylusDeviceCore(StylusDevice stylusDevice)
	{
		lock (__stylusDeviceLock)
		{
			__stylusDeviceMap.Remove(stylusDevice.Id);
		}
	}

	internal WispStylusDevice FindStylusDevice(int stylusDeviceId)
	{
		__stylusDeviceMap.TryGetValue(stylusDeviceId, out var value);
		return value?.As<WispStylusDevice>();
	}

	internal WispStylusDevice FindStylusDeviceWithLock(int stylusDeviceId)
	{
		StylusDevice value;
		lock (__stylusDeviceLock)
		{
			__stylusDeviceMap.TryGetValue(stylusDeviceId, out value);
		}
		return value?.As<WispStylusDevice>();
	}

	internal void SelectStylusDevice(WispStylusDevice wispStylusDevice, IInputElement newOver, bool updateOver)
	{
		bool flag = _currentStylusDevice != wispStylusDevice;
		WispStylusDevice currentStylusDevice = _currentStylusDevice;
		if (updateOver && wispStylusDevice == null && flag)
		{
			_currentStylusDevice.ChangeStylusOver(newOver);
		}
		_currentStylusDevice = wispStylusDevice;
		if (updateOver && wispStylusDevice != null)
		{
			wispStylusDevice.ChangeStylusOver(newOver);
			if (flag && currentStylusDevice != null && !currentStylusDevice.InRange)
			{
				currentStylusDevice.ChangeStylusOver(null);
			}
		}
	}

	internal void EnableCore()
	{
		lock (__penContextsLock)
		{
			foreach (PenContexts value in __penContextsMap.Values)
			{
				value.Enable();
			}
			_inputEnabled = true;
		}
		StylusTraceLogger.LogStartup();
		base.ShutdownListener = new StylusLogicShutDownListener(this, ShutDownEvents.DispatcherShutdown);
	}

	internal void RegisterHwndForInput(InputManager inputManager, PresentationSource inputSource)
	{
		HwndSource hwndSource = (HwndSource)inputSource;
		GetAndCacheTransformToDeviceMatrix(hwndSource);
		bool flag = _tabletDeviceCollection == null;
		WispTabletDeviceCollection wispTabletDevices = WispTabletDevices;
		lock (__penContextsLock)
		{
			if (__penContextsMap.ContainsKey(inputSource))
			{
				throw new InvalidOperationException(SR.PenService_WindowAlreadyRegistered);
			}
			PenContexts penContexts = new PenContexts(StylusLogic.GetCurrentStylusLogicAs<WispLogic>(), inputSource);
			__penContextsMap[inputSource] = penContexts;
			if (__penContextsMap.Count == 1 && !flag && wispTabletDevices.Count > 0)
			{
				wispTabletDevices.UpdateTablets();
				_lastKnownDeviceCount = GetDeviceCount();
			}
			if ((MS.Win32.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, hwndSource.CriticalHandle), -16) & 0x8000000) != 0)
			{
				penContexts.IsWindowDisabled = true;
			}
			if (_inputEnabled)
			{
				penContexts.Enable();
			}
		}
	}

	internal void UnRegisterHwndForInput(HwndSource hwndSource)
	{
		bool hasShutdownStarted = base.Dispatcher.HasShutdownStarted;
		if (hasShutdownStarted)
		{
			OnDispatcherShutdown(null, null);
		}
		lock (__penContextsLock)
		{
			if (__penContextsMap.TryGetValue(hwndSource, out var value))
			{
				__penContextsMap.Remove(hwndSource);
				value.Disable(hasShutdownStarted);
				if (MS.Win32.UnsafeNativeMethods.IsWindow(new HandleRef(hwndSource, hwndSource.CriticalHandle)))
				{
					value.DestroyedLocation = PointUtil.ClientToScreen(new Point(0.0, 0.0), hwndSource);
				}
			}
			if (value == null)
			{
				throw new InvalidOperationException(SR.PenService_WindowNotRegistered);
			}
		}
	}

	internal PenContexts GetPenContextsFromHwnd(PresentationSource presentationSource)
	{
		PenContexts value = null;
		if (presentationSource != null)
		{
			__penContextsMap.TryGetValue(presentationSource, out value);
		}
		return value;
	}

	internal bool ShouldConsiderStylusInRange(RawMouseInputReport mouseInputReport)
	{
		int timestamp = mouseInputReport.Timestamp;
		if (Math.Abs(timestamp - _lastInRangeTime) <= 500)
		{
			return true;
		}
		if (mouseInputReport.InputSource is HwndSource presentationSource)
		{
			PenContexts penContextsFromHwnd = GetPenContextsFromHwnd(presentationSource);
			if (penContextsFromHwnd != null)
			{
				return penContextsFromHwnd.ConsiderInRange(timestamp);
			}
		}
		return false;
	}

	internal PenContext GetStylusPenContextForHwnd(PresentationSource presentationSource, int tabletDeviceId)
	{
		if (presentationSource != null)
		{
			__penContextsMap.TryGetValue(presentationSource, out var value);
			if (value != null)
			{
				return value.GetTabletDeviceIDPenContext(tabletDeviceId);
			}
		}
		return null;
	}

	private void OnDeviceChange()
	{
		if (!_inputEnabled && WispTabletDeviceCollection.ShouldEnableTablets())
		{
			WispTabletDevices.UpdateTablets();
			EnableCore();
			_lastKnownDeviceCount = GetDeviceCount();
		}
	}

	private void OnTabletAdded(uint wisptisIndex)
	{
		lock (__penContextsLock)
		{
			WispTabletDeviceCollection wispTabletDevices = WispTabletDevices;
			if (!_inputEnabled)
			{
				wispTabletDevices.UpdateTablets();
				EnableCore();
				_lastKnownDeviceCount = GetDeviceCount();
				return;
			}
			_lastKnownDeviceCount = GetDeviceCount();
			uint tabletIndexChanged = uint.MaxValue;
			if (!wispTabletDevices.HandleTabletAdded(wisptisIndex, ref tabletIndexChanged))
			{
				return;
			}
			if (tabletIndexChanged != uint.MaxValue)
			{
				foreach (PenContexts value in __penContextsMap.Values)
				{
					value.AddContext(tabletIndexChanged);
				}
				return;
			}
			RefreshTablets();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	protected override void OnTabletRemoved(uint wisptisIndex)
	{
		OnTabletRemovedImpl(wisptisIndex, isInternalCall: false);
	}

	private void OnTabletRemovedImpl(uint wisptisIndex, bool isInternalCall)
	{
		if (!_inputEnabled)
		{
			return;
		}
		lock (__penContextsLock)
		{
			if (_tabletDeviceCollection == null)
			{
				return;
			}
			int deviceCount = GetDeviceCount();
			if (isInternalCall && (_lastKnownDeviceCount < 0 || deviceCount != _lastKnownDeviceCount - 1 || wisptisIndex >= TabletDevices.Count))
			{
				RefreshTablets();
				if (!_inputEnabled)
				{
					OnTabletRemoved(uint.MaxValue);
				}
			}
			else
			{
				int count = _tabletDeviceCollection.DeferredTablets.Count;
				uint num = _tabletDeviceCollection.HandleTabletRemoved(wisptisIndex);
				if (num != uint.MaxValue && _tabletDeviceCollection.DeferredTablets.Count == count)
				{
					foreach (PenContexts value in __penContextsMap.Values)
					{
						value.RemoveContext(num);
					}
				}
			}
			_lastKnownDeviceCount = deviceCount;
		}
	}

	private void RefreshTablets()
	{
		foreach (PenContexts value in __penContextsMap.Values)
		{
			value.Disable(shutdownWorkerThread: false);
		}
		WispTabletDevices.UpdateTablets();
		foreach (PenContexts value2 in __penContextsMap.Values)
		{
			value2.Enable();
		}
	}

	private int GetDeviceCount()
	{
		PenThread penThread = null;
		TabletDeviceCollection tabletDevices = TabletDevices;
		if (tabletDevices != null && tabletDevices.Count > 0)
		{
			penThread = tabletDevices[0].As<WispTabletDevice>().PenThread;
		}
		if (penThread != null)
		{
			return penThread.WorkerGetTabletsInfo().Length;
		}
		return -1;
	}

	private void OnScreenMeasurementsChanged()
	{
		if (!_updatingScreenMeasurements)
		{
			_updatingScreenMeasurements = true;
			base.Dispatcher.BeginInvoke(DispatcherPriority.Background, _processDisplayChanged, null);
		}
	}

	internal void OnWindowEnableChanged(nint hwnd, bool disabled)
	{
		HwndSource hwndSource = HwndSource.CriticalFromHwnd(hwnd);
		if (hwndSource != null)
		{
			PenContexts penContextsFromHwnd = GetPenContextsFromHwnd(hwndSource);
			if (penContextsFromHwnd != null)
			{
				penContextsFromHwnd.IsWindowDisabled = disabled;
			}
		}
		if (!disabled && _currentStylusDevice != null)
		{
			if (_currentStylusDevice.InAir || !_currentStylusDevice.GestureWasFired)
			{
				_mouseLeftButtonState = MouseButtonState.Released;
				_mouseRightButtonState = MouseButtonState.Released;
			}
			else
			{
				_mouseLeftButtonState = (_currentStylusDevice.LeftIsActiveMouseButton ? MouseButtonState.Pressed : MouseButtonState.Released);
				_mouseRightButtonState = ((!_currentStylusDevice.LeftIsActiveMouseButton) ? MouseButtonState.Pressed : MouseButtonState.Released);
			}
		}
	}

	internal object ProcessDisplayChanged(object oInput)
	{
		_updatingScreenMeasurements = false;
		if (_tabletDeviceCollection != null)
		{
			foreach (TabletDevice item in (IEnumerable)_tabletDeviceCollection)
			{
				item.As<WispTabletDevice>()?.UpdateScreenMeasurements();
			}
		}
		return null;
	}

	internal Matrix GetTabletToViewTransform(PresentationSource source, TabletDevice tabletDevice)
	{
		Matrix andCacheTransformToDeviceMatrix = GetAndCacheTransformToDeviceMatrix(source);
		andCacheTransformToDeviceMatrix.Invert();
		return andCacheTransformToDeviceMatrix * tabletDevice.As<TabletDeviceBase>().TabletToScreen;
	}

	internal override Point DeviceUnitsFromMeasureUnits(PresentationSource source, Point measurePoint)
	{
		Point result = measurePoint * GetAndCacheTransformToDeviceMatrix(source);
		result.X = (int)Math.Round(result.X);
		result.Y = (int)Math.Round(result.Y);
		return result;
	}

	internal override Point MeasureUnitsFromDeviceUnits(PresentationSource source, Point measurePoint)
	{
		Matrix andCacheTransformToDeviceMatrix = GetAndCacheTransformToDeviceMatrix(source);
		andCacheTransformToDeviceMatrix.Invert();
		return measurePoint * andCacheTransformToDeviceMatrix;
	}
}
