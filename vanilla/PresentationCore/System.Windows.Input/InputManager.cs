using System.Collections;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Manages all the input systems in Windows Presentation Foundation (WPF).</summary>
public sealed class InputManager : DispatcherObject
{
	internal static readonly RoutedEvent PreviewInputReportEvent = GlobalEventManager.RegisterRoutedEvent("PreviewInputReport", RoutingStrategy.Tunnel, typeof(InputReportEventHandler), typeof(InputManager));

	[FriendAccessAllowed]
	internal static readonly RoutedEvent InputReportEvent = GlobalEventManager.RegisterRoutedEvent("InputReport", RoutingStrategy.Bubble, typeof(InputReportEventHandler), typeof(InputManager));

	private int _menuModeCount;

	private DispatcherOperationCallback _continueProcessingStagingAreaCallback;

	private bool _continueProcessingStagingArea;

	private NotifyInputEventArgs _notifyInputEventArgs;

	private ProcessInputEventArgs _processInputEventArgs;

	private PreProcessInputEventArgs _preProcessInputEventArgs;

	private Tuple<PreProcessInputEventHandler, Delegate[]> _preProcessInput;

	private Tuple<NotifyInputEventHandler, Delegate[]> _preNotifyInput;

	private Tuple<NotifyInputEventHandler, Delegate[]> _postNotifyInput;

	private Tuple<ProcessInputEventHandler, Delegate[]> _postProcessInput;

	private Hashtable _inputProviders = new Hashtable();

	private KeyboardDevice _primaryKeyboardDevice;

	private MouseDevice _primaryMouseDevice;

	private CommandDevice _primaryCommandDevice;

	private bool _inDragDrop;

	private DispatcherOperationCallback _hitTestInvalidatedAsyncCallback;

	private DispatcherOperation _hitTestInvalidatedAsyncOperation;

	private EventHandler _layoutUpdatedCallback;

	private Stack _stagingArea;

	private InputDevice _mostRecentInputDevice;

	private DispatcherTimer _inputTimer;

	private static bool _isSynchronizedInput;

	private static DependencyObject _listeningElement;

	private static RoutedEvent[] _synchronizedInputEvents;

	private static RoutedEvent[] _pairedSynchronizedInputEvents;

	private static SynchronizedInputType _synchronizedInputType;

	private static SynchronizedInputStates _synchronizedInputState = SynchronizedInputStates.NoOpportunity;

	private static DispatcherOperation _synchronizedInputAsyncClearOperation;

	private static readonly object _synchronizedInputLock = new object();

	/// <summary>Gets the <see cref="T:System.Windows.Input.InputManager" /> associated with the current thread.</summary>
	/// <returns>The input manager. </returns>
	public static InputManager Current => GetCurrentInputManagerImpl();

	internal static InputManager UnsecureCurrent
	{
		[FriendAccessAllowed]
		get
		{
			return GetCurrentInputManagerImpl();
		}
	}

	internal static bool IsSynchronizedInput => _isSynchronizedInput;

	internal static RoutedEvent[] SynchronizedInputEvents => _synchronizedInputEvents;

	internal static RoutedEvent[] PairedSynchronizedInputEvents => _pairedSynchronizedInputEvents;

	internal static SynchronizedInputType SynchronizeInputType => _synchronizedInputType;

	internal static DependencyObject ListeningElement => _listeningElement;

	internal static SynchronizedInputStates SynchronizedInputState
	{
		get
		{
			return _synchronizedInputState;
		}
		set
		{
			_synchronizedInputState = value;
		}
	}

	/// <summary>Gets a collection of <see cref="P:System.Windows.Input.InputManager.InputProviders" /> registered with the <see cref="T:System.Windows.Input.InputManager" />. </summary>
	/// <returns>The collection of input provides.</returns>
	public ICollection InputProviders => UnsecureInputProviders;

	internal ICollection UnsecureInputProviders => _inputProviders.Keys;

	/// <summary>Gets the primary keyboard device. </summary>
	/// <returns>The keyboard device.</returns>
	public KeyboardDevice PrimaryKeyboardDevice => _primaryKeyboardDevice;

	/// <summary>Gets the primary mouse device. </summary>
	/// <returns>The mouse device.</returns>
	public MouseDevice PrimaryMouseDevice => _primaryMouseDevice;

	internal StylusLogic StylusLogic => StylusLogic.CurrentStylusLogic;

	internal CommandDevice PrimaryCommandDevice => _primaryCommandDevice;

	internal bool InDragDrop
	{
		get
		{
			return _inDragDrop;
		}
		set
		{
			_inDragDrop = value;
		}
	}

	/// <summary>Gets a value that represents the input device associated with the most recent input event. </summary>
	/// <returns>The input device.</returns>
	public InputDevice MostRecentInputDevice
	{
		get
		{
			return _mostRecentInputDevice;
		}
		internal set
		{
			_mostRecentInputDevice = value;
		}
	}

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Windows.Interop.ComponentDispatcher" /> is in menu mode. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Interop.ComponentDispatcher" /> is in menu mode; otherwise, false. </returns>
	public bool IsInMenuMode => _menuModeCount > 0;

	/// <summary>Occurs when the <see cref="T:System.Windows.Input.InputManager" /> starts to process the input item.</summary>
	public event PreProcessInputEventHandler PreProcessInput
	{
		add
		{
			EventHelper.AddHandler(ref _preProcessInput, value);
		}
		remove
		{
			EventHelper.RemoveHandler(ref _preProcessInput, value);
		}
	}

	/// <summary>Occurs when the <see cref="E:System.Windows.Input.InputManager.PreProcessInput" /> handlers have finished processing the input, if the input was not canceled. </summary>
	public event NotifyInputEventHandler PreNotifyInput
	{
		add
		{
			EventHelper.AddHandler(ref _preNotifyInput, value);
		}
		remove
		{
			EventHelper.RemoveHandler(ref _preNotifyInput, value);
		}
	}

	/// <summary>Occurs after the <see cref="E:System.Windows.Input.InputManager.PreNotifyInput" /> handlers have finished processing the input and the corresponding Windows Presentation Foundation (WPF) events have been raised. </summary>
	public event NotifyInputEventHandler PostNotifyInput
	{
		add
		{
			EventHelper.AddHandler(ref _postNotifyInput, value);
		}
		remove
		{
			EventHelper.RemoveHandler(ref _postNotifyInput, value);
		}
	}

	/// <summary>Occurs after the <see cref="E:System.Windows.Input.InputManager.PreNotifyInput" /> handlers have finished processing the input.</summary>
	public event ProcessInputEventHandler PostProcessInput
	{
		add
		{
			EventHelper.AddHandler(ref _postProcessInput, value);
		}
		remove
		{
			EventHelper.RemoveHandler(ref _postProcessInput, value);
		}
	}

	internal event KeyEventHandler TranslateAccelerator
	{
		[FriendAccessAllowed]
		add
		{
			_translateAccelerator += value;
		}
		[FriendAccessAllowed]
		remove
		{
			_translateAccelerator -= value;
		}
	}

	/// <summary>Occurs when a control enters menu mode by calling the <see cref="M:System.Windows.Input.InputManager.PushMenuMode(System.Windows.PresentationSource)" /> method. </summary>
	public event EventHandler EnterMenuMode;

	/// <summary>Occurs when a control leaves menu mode by calling the <see cref="M:System.Windows.Input.InputManager.PopMenuMode(System.Windows.PresentationSource)" /> method.</summary>
	public event EventHandler LeaveMenuMode;

	/// <summary>Occurs when the result of a hit-test may have changed. </summary>
	public event EventHandler HitTestInvalidatedAsync;

	private event KeyEventHandler _translateAccelerator;

	private static InputManager GetCurrentInputManagerImpl()
	{
		InputManager inputManager = null;
		Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;
		inputManager = currentDispatcher.InputManager as InputManager;
		if (inputManager == null)
		{
			inputManager = (InputManager)(currentDispatcher.InputManager = new InputManager());
		}
		return inputManager;
	}

	private InputManager()
	{
		if (Thread.CurrentThread.GetApartmentState() != 0)
		{
			throw new InvalidOperationException(SR.RequiresSTA);
		}
		_stagingArea = new Stack();
		_primaryKeyboardDevice = new Win32KeyboardDevice(this);
		_primaryMouseDevice = new Win32MouseDevice(this);
		_primaryCommandDevice = new CommandDevice(this);
		_continueProcessingStagingAreaCallback = ContinueProcessingStagingArea;
		_hitTestInvalidatedAsyncOperation = null;
		_hitTestInvalidatedAsyncCallback = HitTestInvalidatedAsyncCallback;
		_layoutUpdatedCallback = OnLayoutUpdated;
		ContextLayoutManager.From(base.Dispatcher).LayoutEvents.Add(_layoutUpdatedCallback);
		_inputTimer = new DispatcherTimer(DispatcherPriority.Background);
		_inputTimer.Tick += ValidateInputDevices;
		_inputTimer.Interval = TimeSpan.FromMilliseconds(125.0);
	}

	internal void RaiseTranslateAccelerator(KeyEventArgs e)
	{
		if (this._translateAccelerator != null)
		{
			this._translateAccelerator(this, e);
		}
	}

	internal InputProviderSite RegisterInputProvider(IInputProvider inputProvider)
	{
		InputProviderSite inputProviderSite = new InputProviderSite(this, inputProvider);
		_inputProviders[inputProvider] = inputProviderSite;
		return inputProviderSite;
	}

	internal void UnregisterInputProvider(IInputProvider inputProvider)
	{
		_inputProviders.Remove(inputProvider);
	}

	/// <summary>Called by components to enter menu mode. </summary>
	/// <param name="menuSite">The menu to enter. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="menuSite" /> is null.</exception>
	public void PushMenuMode(PresentationSource menuSite)
	{
		if (menuSite == null)
		{
			throw new ArgumentNullException("menuSite");
		}
		menuSite.VerifyAccess();
		menuSite.PushMenuMode();
		_menuModeCount++;
		if (1 == _menuModeCount)
		{
			this.EnterMenuMode?.Invoke(null, EventArgs.Empty);
		}
	}

	/// <summary>Called by components to leave menu mode.</summary>
	/// <param name="menuSite">The menu to leave. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="menuSite" /> is null.</exception>
	public void PopMenuMode(PresentationSource menuSite)
	{
		if (menuSite == null)
		{
			throw new ArgumentNullException("menuSite");
		}
		menuSite.VerifyAccess();
		if (_menuModeCount <= 0)
		{
			throw new InvalidOperationException();
		}
		menuSite.PopMenuMode();
		_menuModeCount--;
		if (_menuModeCount == 0)
		{
			this.LeaveMenuMode?.Invoke(null, EventArgs.Empty);
		}
	}

	internal void NotifyHitTestInvalidated()
	{
		if (_hitTestInvalidatedAsyncOperation == null)
		{
			_hitTestInvalidatedAsyncOperation = base.Dispatcher.BeginInvoke(DispatcherPriority.Input, _hitTestInvalidatedAsyncCallback, null);
		}
		else if (_hitTestInvalidatedAsyncOperation.Priority == DispatcherPriority.Inactive)
		{
			ValidateInputDevices(this, EventArgs.Empty);
		}
	}

	internal static void SafeCurrentNotifyHitTestInvalidated()
	{
		UnsecureCurrent.NotifyHitTestInvalidated();
	}

	private object HitTestInvalidatedAsyncCallback(object arg)
	{
		_hitTestInvalidatedAsyncOperation = null;
		if (this.HitTestInvalidatedAsync != null)
		{
			this.HitTestInvalidatedAsync(this, EventArgs.Empty);
		}
		return null;
	}

	private void OnLayoutUpdated(object sender, EventArgs e)
	{
		NotifyHitTestInvalidated();
	}

	internal void InvalidateInputDevices()
	{
		if (_hitTestInvalidatedAsyncOperation == null)
		{
			_hitTestInvalidatedAsyncOperation = base.Dispatcher.BeginInvoke(DispatcherPriority.Inactive, _hitTestInvalidatedAsyncCallback, null);
			_inputTimer.IsEnabled = true;
		}
	}

	private void ValidateInputDevices(object sender, EventArgs e)
	{
		if (_hitTestInvalidatedAsyncOperation != null)
		{
			_hitTestInvalidatedAsyncOperation.Priority = DispatcherPriority.Input;
		}
		_inputTimer.IsEnabled = false;
	}

	/// <summary>Processes the specified input synchronously. </summary>
	/// <returns>true if all input events were handled; otherwise, false.</returns>
	/// <param name="input">The input to process.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is null.</exception>
	public bool ProcessInput(InputEventArgs input)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		PushMarker();
		PushInput(input, null);
		RequestContinueProcessingStagingArea();
		return ProcessStagingArea();
	}

	internal StagingAreaInputItem PushInput(StagingAreaInputItem inputItem)
	{
		_stagingArea.Push(inputItem);
		return inputItem;
	}

	internal StagingAreaInputItem PushInput(InputEventArgs input, StagingAreaInputItem promote)
	{
		StagingAreaInputItem stagingAreaInputItem = new StagingAreaInputItem(isMarker: false);
		stagingAreaInputItem.Reset(input, promote);
		return PushInput(stagingAreaInputItem);
	}

	internal StagingAreaInputItem PushMarker()
	{
		StagingAreaInputItem inputItem = new StagingAreaInputItem(isMarker: true);
		return PushInput(inputItem);
	}

	internal StagingAreaInputItem PopInput()
	{
		object obj = null;
		if (_stagingArea.Count > 0)
		{
			obj = _stagingArea.Pop();
		}
		return obj as StagingAreaInputItem;
	}

	internal StagingAreaInputItem PeekInput()
	{
		object obj = null;
		if (_stagingArea.Count > 0)
		{
			obj = _stagingArea.Peek();
		}
		return obj as StagingAreaInputItem;
	}

	internal object ContinueProcessingStagingArea(object unused)
	{
		_continueProcessingStagingArea = false;
		if (_stagingArea.Count > 0)
		{
			RequestContinueProcessingStagingArea();
			ProcessStagingArea();
		}
		return null;
	}

	internal static bool StartListeningSynchronizedInput(DependencyObject d, SynchronizedInputType inputType)
	{
		lock (_synchronizedInputLock)
		{
			if (_isSynchronizedInput)
			{
				return false;
			}
			_isSynchronizedInput = true;
			_synchronizedInputState = SynchronizedInputStates.NoOpportunity;
			_listeningElement = d;
			_synchronizedInputType = inputType;
			_synchronizedInputEvents = SynchronizedInputHelper.MapInputTypeToRoutedEvents(inputType);
			_pairedSynchronizedInputEvents = SynchronizedInputHelper.MapInputTypeToRoutedEvents(SynchronizedInputHelper.GetPairedInputType(inputType));
			return true;
		}
	}

	internal static void CancelSynchronizedInput()
	{
		lock (_synchronizedInputLock)
		{
			_isSynchronizedInput = false;
			_synchronizedInputState = SynchronizedInputStates.NoOpportunity;
			_listeningElement = null;
			_synchronizedInputEvents = null;
			_pairedSynchronizedInputEvents = null;
			if (_synchronizedInputAsyncClearOperation != null)
			{
				_synchronizedInputAsyncClearOperation.Abort();
				_synchronizedInputAsyncClearOperation = null;
			}
		}
	}

	private bool ProcessStagingArea()
	{
		bool result = false;
		NotifyInputEventArgs notifyInputEventArgs = ((_notifyInputEventArgs != null) ? _notifyInputEventArgs : new NotifyInputEventArgs());
		ProcessInputEventArgs processInputEventArgs = ((_processInputEventArgs != null) ? _processInputEventArgs : new ProcessInputEventArgs());
		PreProcessInputEventArgs preProcessInputEventArgs = ((_preProcessInputEventArgs != null) ? _preProcessInputEventArgs : new PreProcessInputEventArgs());
		_notifyInputEventArgs = null;
		_processInputEventArgs = null;
		_preProcessInputEventArgs = null;
		StagingAreaInputItem stagingAreaInputItem = null;
		while ((stagingAreaInputItem = PopInput()) != null && !stagingAreaInputItem.IsMarker)
		{
			if (_preProcessInput != null)
			{
				preProcessInputEventArgs.Reset(stagingAreaInputItem, this);
				Delegate[] item = _preProcessInput.Item2;
				for (int num = item.Length - 1; num >= 0; num--)
				{
					((PreProcessInputEventHandler)item[num])(this, preProcessInputEventArgs);
				}
			}
			if (preProcessInputEventArgs.Canceled)
			{
				continue;
			}
			if (_preNotifyInput != null)
			{
				notifyInputEventArgs.Reset(stagingAreaInputItem, this);
				Delegate[] item2 = _preNotifyInput.Item2;
				for (int num2 = item2.Length - 1; num2 >= 0; num2--)
				{
					((NotifyInputEventHandler)item2[num2])(this, notifyInputEventArgs);
				}
			}
			InputEventArgs input = stagingAreaInputItem.Input;
			DependencyObject dependencyObject = input.Source as DependencyObject;
			if ((dependencyObject == null || !InputElement.IsValid(dependencyObject as IInputElement)) && input.Device != null)
			{
				dependencyObject = input.Device.Target as DependencyObject;
			}
			if (_isSynchronizedInput && SynchronizedInputHelper.IsMappedEvent(input) && Array.IndexOf(SynchronizedInputEvents, input.RoutedEvent) < 0 && Array.IndexOf(PairedSynchronizedInputEvents, input.RoutedEvent) < 0)
			{
				if (!SynchronizedInputHelper.ShouldContinueListening(input))
				{
					_synchronizedInputState = SynchronizedInputStates.Discarded;
					SynchronizedInputHelper.RaiseAutomationEvents();
					CancelSynchronizedInput();
				}
				else
				{
					_synchronizedInputAsyncClearOperation = base.Dispatcher.BeginInvoke((Action)delegate
					{
						_synchronizedInputState = SynchronizedInputStates.Discarded;
						SynchronizedInputHelper.RaiseAutomationEvents();
						CancelSynchronizedInput();
					}, DispatcherPriority.Background);
				}
			}
			else if (dependencyObject != null)
			{
				if (dependencyObject is UIElement uIElement)
				{
					uIElement.RaiseEvent(input, trusted: true);
				}
				else if (dependencyObject is ContentElement contentElement)
				{
					contentElement.RaiseEvent(input, trusted: true);
				}
				else if (dependencyObject is UIElement3D uIElement3D)
				{
					uIElement3D.RaiseEvent(input, trusted: true);
				}
				if (_isSynchronizedInput && SynchronizedInputHelper.IsListening(_listeningElement, input))
				{
					if (!SynchronizedInputHelper.ShouldContinueListening(input))
					{
						SynchronizedInputHelper.RaiseAutomationEvents();
						CancelSynchronizedInput();
					}
					else
					{
						_synchronizedInputAsyncClearOperation = base.Dispatcher.BeginInvoke((Action)delegate
						{
							SynchronizedInputHelper.RaiseAutomationEvents();
							CancelSynchronizedInput();
						}, DispatcherPriority.Background);
					}
				}
			}
			if (_postNotifyInput != null)
			{
				notifyInputEventArgs.Reset(stagingAreaInputItem, this);
				Delegate[] item3 = _postNotifyInput.Item2;
				for (int num3 = item3.Length - 1; num3 >= 0; num3--)
				{
					((NotifyInputEventHandler)item3[num3])(this, notifyInputEventArgs);
				}
			}
			if (_postProcessInput != null)
			{
				processInputEventArgs.Reset(stagingAreaInputItem, this);
				RaiseProcessInputEventHandlers(_postProcessInput, processInputEventArgs);
				if (stagingAreaInputItem.Input.RoutedEvent == PreviewInputReportEvent && !stagingAreaInputItem.Input.Handled)
				{
					InputReportEventArgs inputReportEventArgs = (InputReportEventArgs)stagingAreaInputItem.Input;
					InputReportEventArgs inputReportEventArgs2 = new InputReportEventArgs(inputReportEventArgs.Device, inputReportEventArgs.Report);
					inputReportEventArgs2.RoutedEvent = InputReportEvent;
					PushInput(inputReportEventArgs2, stagingAreaInputItem);
				}
			}
			if (input.Handled)
			{
				result = true;
			}
		}
		_notifyInputEventArgs = notifyInputEventArgs;
		_processInputEventArgs = processInputEventArgs;
		_preProcessInputEventArgs = preProcessInputEventArgs;
		_notifyInputEventArgs.Reset(null, null);
		_processInputEventArgs.Reset(null, null);
		_preProcessInputEventArgs.Reset(null, null);
		return result;
	}

	private void RaiseProcessInputEventHandlers(Tuple<ProcessInputEventHandler, Delegate[]> postProcessInput, ProcessInputEventArgs processInputEventArgs)
	{
		processInputEventArgs.StagingItem.Input.MarkAsUserInitiated();
		try
		{
			Delegate[] item = postProcessInput.Item2;
			for (int num = item.Length - 1; num >= 0; num--)
			{
				((ProcessInputEventHandler)item[num])(this, processInputEventArgs);
			}
		}
		finally
		{
			processInputEventArgs.StagingItem.Input.ClearUserInitiated();
		}
	}

	private void RequestContinueProcessingStagingArea()
	{
		if (!_continueProcessingStagingArea)
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Input, _continueProcessingStagingAreaCallback, null);
			_continueProcessingStagingArea = true;
		}
	}
}
