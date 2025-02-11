using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Abstract class that represents a keyboard device. </summary>
public abstract class KeyboardDevice : InputDevice
{
	private class ScanCode
	{
		private readonly int _code;

		private readonly bool _isExtended;

		internal int Code => _code;

		internal bool IsExtended => _isExtended;

		internal ScanCode(int code, bool isExtended)
		{
			_code = code;
			_isExtended = isExtended;
		}
	}

	private SecurityCriticalDataClass<InputManager> _inputManager;

	private SecurityCriticalDataClass<PresentationSource> _activeSource;

	private DependencyObject _focus;

	private DeferredElementTreeState _focusTreeState;

	private DependencyObject _forceTarget;

	private DependencyObject _focusRootVisual;

	private Key _previousKey;

	private DependencyPropertyChangedEventHandler _isEnabledChangedEventHandler;

	private DependencyPropertyChangedEventHandler _isVisibleChangedEventHandler;

	private DependencyPropertyChangedEventHandler _focusableChangedEventHandler;

	private DispatcherOperationCallback _reevaluateFocusCallback;

	private DispatcherOperation _reevaluateFocusOperation;

	private object _tagNonRedundantActions = new object();

	private object _tagKey = new object();

	private object _tagScanCode = new object();

	private SecurityCriticalData<TextCompositionManager> _textcompositionManager;

	private SecurityCriticalDataClass<TextServicesManager> _TsfManager;

	/// <summary>Gets the specified <see cref="T:System.Windows.IInputElement" /> that input from this device is sent to. </summary>
	/// <returns>The element that receives input.</returns>
	public override IInputElement Target
	{
		get
		{
			if (ForceTarget != null)
			{
				return ForceTarget;
			}
			return FocusedElement;
		}
	}

	internal IInputElement ForceTarget
	{
		get
		{
			return (IInputElement)_forceTarget;
		}
		set
		{
			_forceTarget = value as DependencyObject;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.PresentationSource" /> that is reporting input for this device. </summary>
	/// <returns>The source of input for this device.</returns>
	public override PresentationSource ActiveSource
	{
		get
		{
			if (_activeSource != null)
			{
				return _activeSource.Value;
			}
			return null;
		}
	}

	/// <summary>Gets or sets the behavior of Windows Presentation Foundation (WPF) when restoring focus.</summary>
	/// <returns>An enumeration value that specifies the behavior of WPF when restoring focus. The default in <see cref="F:System.Windows.Input.RestoreFocusMode.Auto" />.</returns>
	public RestoreFocusMode DefaultRestoreFocusMode { get; set; }

	/// <summary>Gets the element that has keyboard focus. </summary>
	/// <returns>The element with keyboard focus.</returns>
	public IInputElement FocusedElement => (IInputElement)_focus;

	/// <summary>Gets the set of <see cref="T:System.Windows.Input.ModifierKeys" /> which are currently pressed.</summary>
	/// <returns>The set of modifier keys.</returns>
	public ModifierKeys Modifiers
	{
		get
		{
			ModifierKeys modifierKeys = ModifierKeys.None;
			if (IsKeyDown_private(Key.LeftAlt) || IsKeyDown_private(Key.RightAlt))
			{
				modifierKeys |= ModifierKeys.Alt;
			}
			if (IsKeyDown_private(Key.LeftCtrl) || IsKeyDown_private(Key.RightCtrl))
			{
				modifierKeys |= ModifierKeys.Control;
			}
			if (IsKeyDown_private(Key.LeftShift) || IsKeyDown_private(Key.RightShift))
			{
				modifierKeys |= ModifierKeys.Shift;
			}
			return modifierKeys;
		}
	}

	internal TextServicesManager TextServicesManager => _TsfManager.Value;

	internal TextCompositionManager TextCompositionManager => _textcompositionManager.Value;

	internal bool IsActive
	{
		get
		{
			if (_activeSource != null)
			{
				return _activeSource.Value != null;
			}
			return false;
		}
	}

	private DeferredElementTreeState FocusTreeState
	{
		get
		{
			if (_focusTreeState == null)
			{
				_focusTreeState = new DeferredElementTreeState();
			}
			return _focusTreeState;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.KeyboardDevice" /> class. </summary>
	/// <param name="inputManager">The input manager associated with this <see cref="T:System.Windows.Input.KeyboardDevice" />.</param>
	protected KeyboardDevice(InputManager inputManager)
	{
		_inputManager = new SecurityCriticalDataClass<InputManager>(inputManager);
		_inputManager.Value.PreProcessInput += PreProcessInput;
		_inputManager.Value.PreNotifyInput += PreNotifyInput;
		_inputManager.Value.PostProcessInput += PostProcessInput;
		_isEnabledChangedEventHandler = OnIsEnabledChanged;
		_isVisibleChangedEventHandler = OnIsVisibleChanged;
		_focusableChangedEventHandler = OnFocusableChanged;
		_reevaluateFocusCallback = ReevaluateFocusCallback;
		_reevaluateFocusOperation = null;
		_TsfManager = new SecurityCriticalDataClass<TextServicesManager>(new TextServicesManager(inputManager));
		_textcompositionManager = new SecurityCriticalData<TextCompositionManager>(new TextCompositionManager(inputManager));
	}

	/// <summary>When overridden in a derived class, obtains the <see cref="T:System.Windows.Input.KeyStates" /> for the specified <see cref="T:System.Windows.Input.Key" />.</summary>
	/// <returns>The set of key states for the specified key.</returns>
	/// <param name="key">The key to check.</param>
	protected abstract KeyStates GetKeyStatesFromSystem(Key key);

	/// <summary>Clears focus. </summary>
	public void ClearFocus()
	{
		Focus(null, askOld: false, askNew: false, forceToNullIfFailed: false);
	}

	/// <summary>Sets keyboard focus on the specified <see cref="T:System.Windows.IInputElement" />.</summary>
	/// <returns>The element that has keyboard focus.</returns>
	/// <param name="element">The element to move focus to.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="element" /> is not a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />.</exception>
	public IInputElement Focus(IInputElement element)
	{
		DependencyObject dependencyObject = null;
		bool forceToNullIfFailed = false;
		if (element != null)
		{
			if (!InputElement.IsValid(element))
			{
				throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, element.GetType()));
			}
			dependencyObject = (DependencyObject)element;
		}
		if (dependencyObject == null && _activeSource != null)
		{
			dependencyObject = _activeSource.Value.RootVisual;
			forceToNullIfFailed = true;
		}
		Focus(dependencyObject, askOld: true, askNew: true, forceToNullIfFailed);
		return (IInputElement)_focus;
	}

	private void Focus(DependencyObject focus, bool askOld, bool askNew, bool forceToNullIfFailed)
	{
		bool flag = true;
		if (focus != null)
		{
			flag = Keyboard.IsFocusable(focus);
			if (!flag && forceToNullIfFailed)
			{
				focus = null;
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		IKeyboardInputProvider keyboardInputProvider = null;
		DependencyObject containingVisual = InputElement.GetContainingVisual(focus);
		if (containingVisual != null)
		{
			PresentationSource presentationSource = PresentationSource.CriticalFromVisual(containingVisual);
			if (presentationSource != null)
			{
				keyboardInputProvider = (IKeyboardInputProvider)presentationSource.GetInputProvider(typeof(KeyboardDevice));
			}
		}
		TryChangeFocus(focus, keyboardInputProvider, askOld, askNew, forceToNullIfFailed);
	}

	private void Validate_Key(Key key)
	{
		if ((Key)256 <= key || key <= Key.None)
		{
			throw new InvalidEnumArgumentException("key", (int)key, typeof(Key));
		}
	}

	private bool IsKeyDown_private(Key key)
	{
		return (GetKeyStatesFromSystem(key) & KeyStates.Down) == KeyStates.Down;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Input.Key" /> is in the down state.</summary>
	/// <returns>true if <paramref name="key" /> is in the down state; otherwise, false.</returns>
	/// <param name="key">The key to check.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="key" /> is not a valid key.</exception>
	public bool IsKeyDown(Key key)
	{
		Validate_Key(key);
		return IsKeyDown_private(key);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Input.Key" /> is in the up state.</summary>
	/// <returns>true if <paramref name="key" /> is in the up state; otherwise, false.</returns>
	/// <param name="key">The key to check.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="key" /> is not a valid key.</exception>
	public bool IsKeyUp(Key key)
	{
		Validate_Key(key);
		return !IsKeyDown_private(key);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Input.Key" /> is in the toggled state.</summary>
	/// <returns>true if <paramref name="key" /> is in the toggled state; otherwise, false.</returns>
	/// <param name="key">The key to check.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="key" /> is not a valid key.</exception>
	public bool IsKeyToggled(Key key)
	{
		Validate_Key(key);
		return (GetKeyStatesFromSystem(key) & KeyStates.Toggled) == KeyStates.Toggled;
	}

	/// <summary>Gets the set of key states for the specified <see cref="T:System.Windows.Input.Key" />.</summary>
	/// <returns>The set of key states for the specified key.</returns>
	/// <param name="key">The key to check.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="key" /> is not a valid key.</exception>
	public KeyStates GetKeyStates(Key key)
	{
		Validate_Key(key);
		return GetKeyStatesFromSystem(key);
	}

	private void TryChangeFocus(DependencyObject newFocus, IKeyboardInputProvider keyboardInputProvider, bool askOld, bool askNew, bool forceToNullIfFailed)
	{
		bool flag = true;
		int tickCount = Environment.TickCount;
		DependencyObject focus = _focus;
		if (newFocus == _focus)
		{
			return;
		}
		if (askOld && _focus != null)
		{
			KeyboardFocusChangedEventArgs keyboardFocusChangedEventArgs = new KeyboardFocusChangedEventArgs(this, tickCount, (IInputElement)_focus, (IInputElement)newFocus);
			keyboardFocusChangedEventArgs.RoutedEvent = Keyboard.PreviewLostKeyboardFocusEvent;
			keyboardFocusChangedEventArgs.Source = _focus;
			if (_inputManager != null)
			{
				_inputManager.Value.ProcessInput(keyboardFocusChangedEventArgs);
			}
			if (keyboardFocusChangedEventArgs.Handled)
			{
				flag = false;
			}
		}
		if (askNew && flag && newFocus != null)
		{
			KeyboardFocusChangedEventArgs keyboardFocusChangedEventArgs2 = new KeyboardFocusChangedEventArgs(this, tickCount, (IInputElement)_focus, (IInputElement)newFocus);
			keyboardFocusChangedEventArgs2.RoutedEvent = Keyboard.PreviewGotKeyboardFocusEvent;
			keyboardFocusChangedEventArgs2.Source = newFocus;
			if (_inputManager != null)
			{
				_inputManager.Value.ProcessInput(keyboardFocusChangedEventArgs2);
			}
			if (keyboardFocusChangedEventArgs2.Handled)
			{
				flag = false;
			}
		}
		if (flag && newFocus != null)
		{
			if (keyboardInputProvider != null && Keyboard.IsFocusable(newFocus))
			{
				KeyboardInputProviderAcquireFocusEventArgs keyboardInputProviderAcquireFocusEventArgs = new KeyboardInputProviderAcquireFocusEventArgs(this, tickCount, flag);
				keyboardInputProviderAcquireFocusEventArgs.RoutedEvent = Keyboard.PreviewKeyboardInputProviderAcquireFocusEvent;
				keyboardInputProviderAcquireFocusEventArgs.Source = newFocus;
				if (_inputManager != null)
				{
					_inputManager.Value.ProcessInput(keyboardInputProviderAcquireFocusEventArgs);
				}
				flag = keyboardInputProvider.AcquireFocus(checkOnly: false);
				keyboardInputProviderAcquireFocusEventArgs = new KeyboardInputProviderAcquireFocusEventArgs(this, tickCount, flag);
				keyboardInputProviderAcquireFocusEventArgs.RoutedEvent = Keyboard.KeyboardInputProviderAcquireFocusEvent;
				keyboardInputProviderAcquireFocusEventArgs.Source = newFocus;
				if (_inputManager != null)
				{
					_inputManager.Value.ProcessInput(keyboardInputProviderAcquireFocusEventArgs);
				}
			}
			else
			{
				flag = false;
			}
		}
		if (!flag && forceToNullIfFailed && focus == _focus && !(newFocus is IInputElement { IsKeyboardFocusWithin: not false }))
		{
			newFocus = null;
			flag = true;
		}
		if (flag)
		{
			ChangeFocus(newFocus, tickCount);
		}
	}

	private void ChangeFocus(DependencyObject focus, int timestamp)
	{
		DependencyObject dependencyObject = null;
		if (focus == _focus)
		{
			return;
		}
		DependencyObject focus2 = _focus;
		_focus = focus;
		_focusRootVisual = InputElement.GetRootVisual(focus);
		using (base.Dispatcher.DisableProcessing())
		{
			if (focus2 != null)
			{
				dependencyObject = focus2;
				if (dependencyObject is UIElement uIElement)
				{
					uIElement.IsEnabledChanged -= _isEnabledChangedEventHandler;
					uIElement.IsVisibleChanged -= _isVisibleChangedEventHandler;
					uIElement.FocusableChanged -= _focusableChangedEventHandler;
				}
				else if (dependencyObject is ContentElement contentElement)
				{
					contentElement.IsEnabledChanged -= _isEnabledChangedEventHandler;
					contentElement.FocusableChanged -= _focusableChangedEventHandler;
				}
				else
				{
					if (!(dependencyObject is UIElement3D uIElement3D))
					{
						throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, dependencyObject.GetType()));
					}
					uIElement3D.IsEnabledChanged -= _isEnabledChangedEventHandler;
					uIElement3D.IsVisibleChanged -= _isVisibleChangedEventHandler;
					uIElement3D.FocusableChanged -= _focusableChangedEventHandler;
				}
			}
			if (_focus != null)
			{
				dependencyObject = _focus;
				if (dependencyObject is UIElement uIElement2)
				{
					uIElement2.IsEnabledChanged += _isEnabledChangedEventHandler;
					uIElement2.IsVisibleChanged += _isVisibleChangedEventHandler;
					uIElement2.FocusableChanged += _focusableChangedEventHandler;
				}
				else if (dependencyObject is ContentElement contentElement2)
				{
					contentElement2.IsEnabledChanged += _isEnabledChangedEventHandler;
					contentElement2.FocusableChanged += _focusableChangedEventHandler;
				}
				else
				{
					if (!(dependencyObject is UIElement3D uIElement3D2))
					{
						throw new InvalidOperationException(SR.Format(SR.Invalid_IInputElement, dependencyObject.GetType()));
					}
					uIElement3D2.IsEnabledChanged += _isEnabledChangedEventHandler;
					uIElement3D2.IsVisibleChanged += _isVisibleChangedEventHandler;
					uIElement3D2.FocusableChanged += _focusableChangedEventHandler;
				}
			}
		}
		UIElement.FocusWithinProperty.OnOriginValueChanged(focus2, _focus, ref _focusTreeState);
		if (focus2 != null)
		{
			dependencyObject = focus2;
			dependencyObject.SetValue(UIElement.IsKeyboardFocusedPropertyKey, value: false);
		}
		if (_focus != null)
		{
			dependencyObject = _focus;
			dependencyObject.SetValue(UIElement.IsKeyboardFocusedPropertyKey, value: true);
		}
		if (_TsfManager != null)
		{
			_TsfManager.Value.Focus(_focus);
		}
		InputLanguageManager.Current.Focus(_focus, focus2);
		if (focus2 != null)
		{
			KeyboardFocusChangedEventArgs keyboardFocusChangedEventArgs = new KeyboardFocusChangedEventArgs(this, timestamp, (IInputElement)focus2, (IInputElement)focus);
			keyboardFocusChangedEventArgs.RoutedEvent = Keyboard.LostKeyboardFocusEvent;
			keyboardFocusChangedEventArgs.Source = focus2;
			if (_inputManager != null)
			{
				_inputManager.Value.ProcessInput(keyboardFocusChangedEventArgs);
			}
		}
		if (_focus != null)
		{
			KeyboardFocusChangedEventArgs keyboardFocusChangedEventArgs2 = new KeyboardFocusChangedEventArgs(this, timestamp, (IInputElement)focus2, (IInputElement)_focus);
			keyboardFocusChangedEventArgs2.RoutedEvent = Keyboard.GotKeyboardFocusEvent;
			keyboardFocusChangedEventArgs2.Source = _focus;
			if (_inputManager != null)
			{
				_inputManager.Value.ProcessInput(keyboardFocusChangedEventArgs2);
			}
		}
		InputMethod.Current.GotKeyboardFocus(_focus);
		AutomationPeer.RaiseFocusChangedEventHelper((IInputElement)_focus);
	}

	private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		ReevaluateFocusAsync(null, null, isCoreParent: false);
	}

	private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		ReevaluateFocusAsync(null, null, isCoreParent: false);
	}

	private void OnFocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		ReevaluateFocusAsync(null, null, isCoreParent: false);
	}

	internal void ReevaluateFocusAsync(DependencyObject element, DependencyObject oldParent, bool isCoreParent)
	{
		if (element != null)
		{
			if (isCoreParent)
			{
				FocusTreeState.SetCoreParent(element, oldParent);
			}
			else
			{
				FocusTreeState.SetLogicalParent(element, oldParent);
			}
		}
		if (_reevaluateFocusOperation == null)
		{
			_reevaluateFocusOperation = base.Dispatcher.BeginInvoke(DispatcherPriority.Input, _reevaluateFocusCallback, null);
		}
	}

	private object ReevaluateFocusCallback(object arg)
	{
		_reevaluateFocusOperation = null;
		if (_focus == null)
		{
			return null;
		}
		DependencyObject dependencyObject = _focus;
		while (dependencyObject != null && !Keyboard.IsFocusable(dependencyObject))
		{
			dependencyObject = DeferredElementTreeState.GetCoreParent(dependencyObject, null);
		}
		PresentationSource presentationSource = null;
		DependencyObject containingVisual = InputElement.GetContainingVisual(dependencyObject);
		if (containingVisual != null)
		{
			presentationSource = PresentationSource.CriticalFromVisual(containingVisual);
		}
		bool flag = true;
		DependencyObject dependencyObject2 = null;
		if (presentationSource != null && presentationSource.GetInputProvider(typeof(KeyboardDevice)) is IKeyboardInputProvider keyboardInputProvider && keyboardInputProvider.AcquireFocus(checkOnly: true))
		{
			if (dependencyObject == _focus)
			{
				flag = false;
			}
			else
			{
				flag = true;
				dependencyObject2 = dependencyObject;
			}
		}
		if (flag)
		{
			if (dependencyObject2 == null && _activeSource != null)
			{
				dependencyObject2 = _activeSource.Value.RootVisual;
			}
			Focus(dependencyObject2, askOld: false, askNew: true, forceToNullIfFailed: true);
		}
		else if (_focusTreeState != null && !_focusTreeState.IsEmpty)
		{
			UIElement.FocusWithinProperty.OnOriginValueChanged(_focus, _focus, ref _focusTreeState);
		}
		return null;
	}

	private void PreProcessInput(object sender, PreProcessInputEventArgs e)
	{
		if (ExtractRawKeyboardInputReport(e, InputManager.PreviewInputReportEvent) != null)
		{
			e.StagingItem.Input.Device = this;
		}
	}

	private void PreNotifyInput(object sender, NotifyInputEventArgs e)
	{
		RawKeyboardInputReport rawKeyboardInputReport = ExtractRawKeyboardInputReport(e, InputManager.PreviewInputReportEvent);
		if (rawKeyboardInputReport != null)
		{
			CheckForDisconnectedFocus();
			if ((rawKeyboardInputReport.Actions & RawKeyboardActions.Activate) == RawKeyboardActions.Activate)
			{
				if (_activeSource == null)
				{
					_activeSource = new SecurityCriticalDataClass<PresentationSource>(rawKeyboardInputReport.InputSource);
				}
				else if (_activeSource.Value != rawKeyboardInputReport.InputSource)
				{
					IKeyboardInputProvider keyboardInputProvider = _activeSource.Value.GetInputProvider(typeof(KeyboardDevice)) as IKeyboardInputProvider;
					_activeSource = new SecurityCriticalDataClass<PresentationSource>(rawKeyboardInputReport.InputSource);
					keyboardInputProvider?.NotifyDeactivate();
				}
			}
			if ((rawKeyboardInputReport.Actions & RawKeyboardActions.KeyDown) == RawKeyboardActions.KeyDown)
			{
				RawKeyboardActions nonRedundantActions = GetNonRedundantActions(e);
				nonRedundantActions |= RawKeyboardActions.KeyDown;
				e.StagingItem.SetData(_tagNonRedundantActions, nonRedundantActions);
				Key key = KeyInterop.KeyFromVirtualKey(rawKeyboardInputReport.VirtualKey);
				e.StagingItem.SetData(_tagKey, key);
				e.StagingItem.SetData(_tagScanCode, new ScanCode(rawKeyboardInputReport.ScanCode, rawKeyboardInputReport.IsExtendedKey));
				if (_inputManager != null)
				{
					_inputManager.Value.MostRecentInputDevice = this;
				}
			}
			if ((rawKeyboardInputReport.Actions & RawKeyboardActions.KeyUp) == RawKeyboardActions.KeyUp)
			{
				RawKeyboardActions nonRedundantActions2 = GetNonRedundantActions(e);
				nonRedundantActions2 |= RawKeyboardActions.KeyUp;
				e.StagingItem.SetData(_tagNonRedundantActions, nonRedundantActions2);
				Key key2 = KeyInterop.KeyFromVirtualKey(rawKeyboardInputReport.VirtualKey);
				e.StagingItem.SetData(_tagKey, key2);
				e.StagingItem.SetData(_tagScanCode, new ScanCode(rawKeyboardInputReport.ScanCode, rawKeyboardInputReport.IsExtendedKey));
				if (_inputManager != null)
				{
					_inputManager.Value.MostRecentInputDevice = this;
				}
			}
		}
		if (e.StagingItem.Input.RoutedEvent == Keyboard.PreviewKeyDownEvent)
		{
			CheckForDisconnectedFocus();
			KeyEventArgs keyEventArgs = (KeyEventArgs)e.StagingItem.Input;
			if (_previousKey == keyEventArgs.RealKey)
			{
				keyEventArgs.SetRepeat(newRepeatState: true);
				return;
			}
			_previousKey = keyEventArgs.RealKey;
			keyEventArgs.SetRepeat(newRepeatState: false);
		}
		else if (e.StagingItem.Input.RoutedEvent == Keyboard.PreviewKeyUpEvent)
		{
			CheckForDisconnectedFocus();
			((KeyEventArgs)e.StagingItem.Input).SetRepeat(newRepeatState: false);
			_previousKey = Key.None;
		}
	}

	private void PostProcessInput(object sender, ProcessInputEventArgs e)
	{
		if (e.StagingItem.Input.RoutedEvent == Keyboard.PreviewKeyDownEvent)
		{
			CheckForDisconnectedFocus();
			if (!e.StagingItem.Input.Handled)
			{
				KeyEventArgs keyEventArgs = (KeyEventArgs)e.StagingItem.Input;
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				Key key = keyEventArgs.Key;
				switch (key)
				{
				case Key.System:
					flag = true;
					key = keyEventArgs.RealKey;
					break;
				case Key.ImeProcessed:
					flag2 = true;
					key = keyEventArgs.RealKey;
					break;
				case Key.DeadCharProcessed:
					flag3 = true;
					key = keyEventArgs.RealKey;
					break;
				}
				KeyEventArgs keyEventArgs2 = new KeyEventArgs(this, keyEventArgs.UnsafeInputSource, keyEventArgs.Timestamp, key);
				keyEventArgs2.SetRepeat(keyEventArgs.IsRepeat);
				if (flag)
				{
					keyEventArgs2.MarkSystem();
				}
				else if (flag2)
				{
					keyEventArgs2.MarkImeProcessed();
				}
				else if (flag3)
				{
					keyEventArgs2.MarkDeadCharProcessed();
				}
				keyEventArgs2.RoutedEvent = Keyboard.KeyDownEvent;
				keyEventArgs2.ScanCode = keyEventArgs.ScanCode;
				keyEventArgs2.IsExtendedKey = keyEventArgs.IsExtendedKey;
				e.PushInput(keyEventArgs2, e.StagingItem);
			}
		}
		if (e.StagingItem.Input.RoutedEvent == Keyboard.PreviewKeyUpEvent)
		{
			CheckForDisconnectedFocus();
			if (!e.StagingItem.Input.Handled)
			{
				KeyEventArgs keyEventArgs3 = (KeyEventArgs)e.StagingItem.Input;
				bool flag4 = false;
				bool flag5 = false;
				bool flag6 = false;
				Key key2 = keyEventArgs3.Key;
				switch (key2)
				{
				case Key.System:
					flag4 = true;
					key2 = keyEventArgs3.RealKey;
					break;
				case Key.ImeProcessed:
					flag5 = true;
					key2 = keyEventArgs3.RealKey;
					break;
				case Key.DeadCharProcessed:
					flag6 = true;
					key2 = keyEventArgs3.RealKey;
					break;
				}
				KeyEventArgs keyEventArgs4 = new KeyEventArgs(this, keyEventArgs3.UnsafeInputSource, keyEventArgs3.Timestamp, key2);
				if (flag4)
				{
					keyEventArgs4.MarkSystem();
				}
				else if (flag5)
				{
					keyEventArgs4.MarkImeProcessed();
				}
				else if (flag6)
				{
					keyEventArgs4.MarkDeadCharProcessed();
				}
				keyEventArgs4.RoutedEvent = Keyboard.KeyUpEvent;
				keyEventArgs4.ScanCode = keyEventArgs3.ScanCode;
				keyEventArgs4.IsExtendedKey = keyEventArgs3.IsExtendedKey;
				e.PushInput(keyEventArgs4, e.StagingItem);
			}
		}
		RawKeyboardInputReport rawKeyboardInputReport = ExtractRawKeyboardInputReport(e, InputManager.InputReportEvent);
		if (rawKeyboardInputReport == null)
		{
			return;
		}
		CheckForDisconnectedFocus();
		if (!e.StagingItem.Input.Handled)
		{
			RawKeyboardActions nonRedundantActions = GetNonRedundantActions(e);
			if ((nonRedundantActions & RawKeyboardActions.KeyDown) == RawKeyboardActions.KeyDown)
			{
				Key key3 = (Key)e.StagingItem.GetData(_tagKey);
				if (key3 != 0)
				{
					KeyEventArgs keyEventArgs5 = new KeyEventArgs(this, rawKeyboardInputReport.InputSource, rawKeyboardInputReport.Timestamp, key3);
					ScanCode scanCode = (ScanCode)e.StagingItem.GetData(_tagScanCode);
					keyEventArgs5.ScanCode = scanCode.Code;
					keyEventArgs5.IsExtendedKey = scanCode.IsExtended;
					if (rawKeyboardInputReport.IsSystemKey)
					{
						keyEventArgs5.MarkSystem();
					}
					keyEventArgs5.RoutedEvent = Keyboard.PreviewKeyDownEvent;
					e.PushInput(keyEventArgs5, e.StagingItem);
				}
			}
			if ((nonRedundantActions & RawKeyboardActions.KeyUp) == RawKeyboardActions.KeyUp)
			{
				Key key4 = (Key)e.StagingItem.GetData(_tagKey);
				if (key4 != 0)
				{
					KeyEventArgs keyEventArgs6 = new KeyEventArgs(this, rawKeyboardInputReport.InputSource, rawKeyboardInputReport.Timestamp, key4);
					ScanCode scanCode2 = (ScanCode)e.StagingItem.GetData(_tagScanCode);
					keyEventArgs6.ScanCode = scanCode2.Code;
					keyEventArgs6.IsExtendedKey = scanCode2.IsExtended;
					if (rawKeyboardInputReport.IsSystemKey)
					{
						keyEventArgs6.MarkSystem();
					}
					keyEventArgs6.RoutedEvent = Keyboard.PreviewKeyUpEvent;
					e.PushInput(keyEventArgs6, e.StagingItem);
				}
			}
		}
		if ((rawKeyboardInputReport.Actions & RawKeyboardActions.Deactivate) == RawKeyboardActions.Deactivate && IsActive)
		{
			_activeSource = null;
			ChangeFocus(null, e.StagingItem.Input.Timestamp);
		}
	}

	private RawKeyboardInputReport ExtractRawKeyboardInputReport(NotifyInputEventArgs e, RoutedEvent Event)
	{
		RawKeyboardInputReport result = null;
		if (e.StagingItem.Input is InputReportEventArgs inputReportEventArgs && inputReportEventArgs.Report.Type == InputType.Keyboard && inputReportEventArgs.RoutedEvent == Event)
		{
			result = inputReportEventArgs.Report as RawKeyboardInputReport;
		}
		return result;
	}

	private RawKeyboardActions GetNonRedundantActions(NotifyInputEventArgs e)
	{
		object data = e.StagingItem.GetData(_tagNonRedundantActions);
		if (data != null)
		{
			return (RawKeyboardActions)data;
		}
		return RawKeyboardActions.None;
	}

	private bool CheckForDisconnectedFocus()
	{
		bool result = false;
		if (InputElement.GetRootVisual(_focus) != _focusRootVisual)
		{
			result = true;
			Focus(null);
		}
		return result;
	}
}
