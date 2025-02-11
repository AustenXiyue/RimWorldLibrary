using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Automation;
using MS.Internal.Interop;
using MS.Internal.PresentationCore;
using MS.Utility;
using MS.Win32;

namespace System.Windows.Interop;

/// <summary>Presents Windows Presentation Foundation (WPF) content in a Win32 window.</summary>
public class HwndSource : PresentationSource, IDisposable, IWin32Window, IKeyboardInputSink
{
	private class MSGDATA
	{
		public MSG msg;

		public bool handled;
	}

	private class ThreadDataBlob
	{
		public int TranslateAcceleratorCallDepth;
	}

	private class WeakEventDispatcherShutdown : WeakReference
	{
		private Dispatcher _that;

		public WeakEventDispatcherShutdown(HwndSource source, Dispatcher that)
			: base(source)
		{
			_that = that;
			_that.ShutdownFinished += OnShutdownFinished;
		}

		public void OnShutdownFinished(object sender, EventArgs e)
		{
			if (Target is HwndSource hwndSource)
			{
				hwndSource.OnShutdownFinished(sender, e);
			}
			else
			{
				Dispose();
			}
		}

		public void Dispose()
		{
			if (_that != null)
			{
				_that.ShutdownFinished -= OnShutdownFinished;
			}
		}
	}

	private class WeakEventPreprocessMessage : WeakReference
	{
		private bool _addToFront;

		private ThreadMessageEventHandler _handler;

		public WeakEventPreprocessMessage(HwndSource source, bool addToFront)
			: base(source)
		{
			_addToFront = addToFront;
			_handler = OnPreprocessMessage;
			if (addToFront)
			{
				ComponentDispatcher.CriticalAddThreadPreprocessMessageHandlerFirst(_handler);
			}
			else
			{
				ComponentDispatcher.ThreadPreprocessMessage += _handler;
			}
		}

		public void OnPreprocessMessage(ref MSG msg, ref bool handled)
		{
			if (Target is HwndSource hwndSource)
			{
				hwndSource.OnPreprocessMessageThunk(ref msg, ref handled);
			}
			else
			{
				Dispose();
			}
		}

		public void Dispose()
		{
			if (_addToFront)
			{
				ComponentDispatcher.CriticalRemoveThreadPreprocessMessageHandlerFirst(_handler);
			}
			else
			{
				ComponentDispatcher.ThreadPreprocessMessage -= _handler;
			}
			_handler = null;
		}
	}

	private object _constructionParameters;

	private bool _isDisposed;

	private bool _isDisposing;

	private bool _inRealHwndDispose;

	private bool _adjustSizingForNonClientArea;

	private bool _treatAncestorsAsNonClientArea;

	private bool _myOwnUpdate;

	private bool _isWindowInMinimizeState;

	private int _registeredDropTargetCount;

	private SizeToContent _sizeToContent;

	private Size? _previousSize;

	private HwndWrapper _hwndWrapper;

	private HwndTarget _hwndTarget;

	private MS.Internal.SecurityCriticalDataForSet<Visual> _rootVisual;

	private Tuple<HwndSourceHook, Delegate[]> _hooks;

	private SecurityCriticalDataClass<HwndMouseInputProvider> _mouse;

	private SecurityCriticalDataClass<HwndKeyboardInputProvider> _keyboard;

	private SecurityCriticalDataClass<IStylusInputProvider> _stylus;

	private SecurityCriticalDataClass<HwndAppCommandInputProvider> _appCommand;

	private WeakEventDispatcherShutdown _weakShutdownHandler;

	private WeakEventPreprocessMessage _weakPreprocessMessageHandler;

	private WeakEventPreprocessMessage _weakMenuModeMessageHandler;

	private static LocalDataStoreSlot _threadSlot;

	private RestoreFocusMode _restoreFocusMode;

	[ThreadStatic]
	private static bool? _defaultAcquireHwndFocusInMenuMode;

	private bool _acquireHwndFocusInMenuMode;

	private MSG _lastKeyboardMessage;

	private List<HwndSourceKeyboardInputSite> _keyboardInputSinkChildren;

	private IKeyboardInputSite _keyboardInputSite;

	private HwndWrapperHook _layoutHook;

	private HwndWrapperHook _inputHook;

	private HwndWrapperHook _hwndTargetHook;

	private HwndWrapperHook _publicHook;

	[ThreadStatic]
	internal static bool _eatCharMessages;

	/// <summary>Gets a value that indicates whether <see cref="M:System.Windows.Interop.HwndSource.Dispose" /> has been called on this <see cref="T:System.Windows.Interop.HwndSource" />. </summary>
	/// <returns>true if the object has had <see cref="M:System.Windows.Interop.HwndSource.Dispose" /> called on it; otherwise, false.</returns>
	public override bool IsDisposed => _isDisposed;

	/// <summary>Gets or sets the <see cref="P:System.Windows.Media.CompositionTarget.RootVisual" /> of the window. </summary>
	/// <returns>The root visual object of the window.</returns>
	public override Visual RootVisual
	{
		get
		{
			if (_isDisposed)
			{
				return null;
			}
			return _rootVisual.Value;
		}
		set
		{
			CheckDisposed(verifyAccess: true);
			RootVisualInternal = value;
		}
	}

	private Visual RootVisualInternal
	{
		set
		{
			if (_rootVisual.Value != value)
			{
				Visual value2 = _rootVisual.Value;
				if (value != null)
				{
					_rootVisual.Value = value;
					if (_rootVisual.Value is UIElement)
					{
						((UIElement)_rootVisual.Value).LayoutUpdated += OnLayoutUpdated;
					}
					if (_hwndTarget != null && !_hwndTarget.IsDisposed)
					{
						_hwndTarget.RootVisual = _rootVisual.Value;
					}
					UIElement.PropagateResumeLayout(null, value);
				}
				else
				{
					_rootVisual.Value = null;
					if (_hwndTarget != null && !_hwndTarget.IsDisposed)
					{
						_hwndTarget.RootVisual = null;
					}
				}
				if (value2 != null)
				{
					if (value2 is UIElement)
					{
						((UIElement)value2).LayoutUpdated -= OnLayoutUpdated;
					}
					UIElement.PropagateSuspendLayout(value2);
				}
				RootChanged(value2, _rootVisual.Value);
				if (IsLayoutActive())
				{
					SetLayoutSize();
					base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(PresentationSource.FireContentRendered), this);
				}
				else
				{
					InputManager.SafeCurrentNotifyHitTestInvalidated();
				}
				if (_keyboard != null)
				{
					_keyboard.Value.OnRootChanged(value2, _rootVisual.Value);
				}
			}
			if (value != null && _hwndTarget != null && !_hwndTarget.IsDisposed && EventMap.HasListeners)
			{
				_hwndTarget.EnsureAutomationPeer(value);
			}
		}
	}

	/// <summary>Gets a sequence of registered input sinks.</summary>
	/// <returns>An enumeration of keyboard input sinks.</returns>
	public IEnumerable<IKeyboardInputSink> ChildKeyboardInputSinks
	{
		get
		{
			if (_keyboardInputSinkChildren == null)
			{
				yield break;
			}
			foreach (HwndSourceKeyboardInputSite keyboardInputSinkChild in _keyboardInputSinkChildren)
			{
				yield return ((IKeyboardInputSite)keyboardInputSinkChild).Sink;
			}
		}
	}

	/// <summary>Gets the visual manager for the hosted window. </summary>
	/// <returns>The visual manager.</returns>
	public new HwndTarget CompositionTarget
	{
		get
		{
			if (_isDisposed)
			{
				return null;
			}
			if (_hwndTarget != null && _hwndTarget.IsDisposed)
			{
				return null;
			}
			return _hwndTarget;
		}
	}

	internal bool IsInExclusiveMenuMode { get; private set; }

	/// <summary>Gets the window handle for this <see cref="T:System.Windows.Interop.HwndSource" />. </summary>
	/// <returns>The window handle.</returns>
	public nint Handle => CriticalHandle;

	internal nint CriticalHandle
	{
		[FriendAccessAllowed]
		get
		{
			if (_hwndWrapper != null)
			{
				return _hwndWrapper.Handle;
			}
			return IntPtr.Zero;
		}
	}

	internal HwndWrapper HwndWrapper => _hwndWrapper;

	internal bool HasCapture => SafeNativeMethods.GetCapture() == CriticalHandle;

	internal bool IsHandleNull => _hwndWrapper.Handle == IntPtr.Zero;

	/// <summary>Get or sets whether and how the window is sized to its content. </summary>
	/// <returns>One of the enumeration values. The default value is <see cref="F:System.Windows.SizeToContent.Manual" />, which specifies that the window is not sized to its content.</returns>
	public SizeToContent SizeToContent
	{
		get
		{
			CheckDisposed(verifyAccess: true);
			return _sizeToContent;
		}
		set
		{
			CheckDisposed(verifyAccess: true);
			if (!IsValidSizeToContent(value))
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(SizeToContent));
			}
			if (_sizeToContent != value)
			{
				_sizeToContent = value;
				if (IsLayoutActive())
				{
					SetLayoutSize();
				}
			}
		}
	}

	/// <summary>Gets a value that declares whether the per-pixel opacity of the source window content is respected.</summary>
	/// <returns>true if the system uses per-pixel opacity; otherwise, false.</returns>
	public bool UsesPerPixelOpacity
	{
		get
		{
			CheckDisposed(verifyAccess: true);
			_ = CompositionTarget;
			if (_hwndTarget != null)
			{
				return _hwndTarget.UsesPerPixelOpacity;
			}
			return false;
		}
	}

	/// <summary>Gets or sets a reference to the component's container's <see cref="T:System.Windows.Interop.IKeyboardInputSite" /> interface. </summary>
	/// <returns>A reference to the container's <see cref="T:System.Windows.Interop.IKeyboardInputSite" /> interface; or null if no site is assigned. The default is null.</returns>
	protected IKeyboardInputSite KeyboardInputSiteCore
	{
		get
		{
			return _keyboardInputSite;
		}
		set
		{
			_keyboardInputSite = value;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Windows.Interop.IKeyboardInputSink.KeyboardInputSite" />.</summary>
	/// <returns>A reference to the container's <see cref="T:System.Windows.Interop.IKeyboardInputSite" /> interface.</returns>
	IKeyboardInputSite IKeyboardInputSink.KeyboardInputSite
	{
		get
		{
			return KeyboardInputSiteCore;
		}
		set
		{
			KeyboardInputSiteCore = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Input.RestoreFocusMode" /> for the window.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.RestoreFocusMode" /> for the window.</returns>
	public RestoreFocusMode RestoreFocusMode => _restoreFocusMode;

	/// <summary>Gets or sets the default <see cref="P:System.Windows.Interop.HwndSource.AcquireHwndFocusInMenuMode" /> value for new instances of <see cref="T:System.Windows.Interop.HwndSource" />. </summary>
	/// <returns>true to acquire Win32 focus for the WPF containing window when the user interacts with menus; otherwise, false. The default is true.</returns>
	public static bool DefaultAcquireHwndFocusInMenuMode
	{
		get
		{
			if (!_defaultAcquireHwndFocusInMenuMode.HasValue)
			{
				_defaultAcquireHwndFocusInMenuMode = true;
			}
			return _defaultAcquireHwndFocusInMenuMode.Value;
		}
		set
		{
			_defaultAcquireHwndFocusInMenuMode = value;
		}
	}

	/// <summary>Gets the value that determines whether to acquire Win32 focus for the WPF containing window for this <see cref="T:System.Windows.Interop.HwndSource" />.</summary>
	/// <returns>true to acquire Win32 focus for the WPF containing window when the user interacts with menus; otherwise, false.</returns>
	public bool AcquireHwndFocusInMenuMode => _acquireHwndFocusInMenuMode;

	private IKeyboardInputSink ChildSinkWithFocus
	{
		get
		{
			IKeyboardInputSink result = null;
			if (_keyboardInputSinkChildren == null)
			{
				return null;
			}
			foreach (HwndSourceKeyboardInputSite keyboardInputSinkChild in _keyboardInputSinkChildren)
			{
				if (((IKeyboardInputSite)keyboardInputSinkChild).Sink.HasFocusWithin())
				{
					result = ((IKeyboardInputSite)keyboardInputSinkChild).Sink;
					break;
				}
			}
			return result;
		}
	}

	private bool IsUsable
	{
		get
		{
			if (!_isDisposed && _hwndTarget != null)
			{
				return !_hwndTarget.IsDisposed;
			}
			return false;
		}
	}

	private bool HasFocus => MS.Win32.UnsafeNativeMethods.GetFocus() == CriticalHandle;

	private static ThreadDataBlob PerThreadData
	{
		get
		{
			object data = Thread.GetData(_threadSlot);
			ThreadDataBlob threadDataBlob;
			if (data == null)
			{
				threadDataBlob = new ThreadDataBlob();
				Thread.SetData(_threadSlot, threadDataBlob);
			}
			else
			{
				threadDataBlob = (ThreadDataBlob)data;
			}
			return threadDataBlob;
		}
	}

	/// <summary>Occurs when the <see cref="M:System.Windows.Interop.HwndSource.Dispose" /> method is called on this object.</summary>
	public event EventHandler Disposed;

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.Interop.HwndSource.SizeToContent" /> property changes.</summary>
	public event EventHandler SizeToContentChanged;

	public event HwndDpiChangedEventHandler DpiChanged;

	/// <summary>Occurs when layout causes the <see cref="T:System.Windows.Interop.HwndSource" /> to automatically resize. </summary>
	public event AutoResizedEventHandler AutoResized;

	static HwndSource()
	{
		_threadSlot = Thread.AllocateDataSlot();
	}

	public HwndSource(int classStyle, int style, int exStyle, int x, int y, string name, nint parent)
	{
		HwndSourceParameters parameters = new HwndSourceParameters(name);
		parameters.WindowClassStyle = classStyle;
		parameters.WindowStyle = style;
		parameters.ExtendedWindowStyle = exStyle;
		parameters.SetPosition(x, y);
		parameters.ParentWindow = parent;
		Initialize(parameters);
	}

	public HwndSource(int classStyle, int style, int exStyle, int x, int y, int width, int height, string name, nint parent, bool adjustSizingForNonClientArea)
	{
		HwndSourceParameters parameters = new HwndSourceParameters(name, width, height);
		parameters.WindowClassStyle = classStyle;
		parameters.WindowStyle = style;
		parameters.ExtendedWindowStyle = exStyle;
		parameters.SetPosition(x, y);
		parameters.ParentWindow = parent;
		parameters.AdjustSizingForNonClientArea = adjustSizingForNonClientArea;
		Initialize(parameters);
	}

	public HwndSource(int classStyle, int style, int exStyle, int x, int y, int width, int height, string name, nint parent)
	{
		HwndSourceParameters parameters = new HwndSourceParameters(name, width, height);
		parameters.WindowClassStyle = classStyle;
		parameters.WindowStyle = style;
		parameters.ExtendedWindowStyle = exStyle;
		parameters.SetPosition(x, y);
		parameters.ParentWindow = parent;
		Initialize(parameters);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Interop.HwndSource" /> class by using a structure that contains the initial settings. </summary>
	/// <param name="parameters">A structure that contains the parameters that are required to create the window.</param>
	public HwndSource(HwndSourceParameters parameters)
	{
		Initialize(parameters);
	}

	private void Initialize(HwndSourceParameters parameters)
	{
		_mouse = new SecurityCriticalDataClass<HwndMouseInputProvider>(new HwndMouseInputProvider(this));
		_keyboard = new SecurityCriticalDataClass<HwndKeyboardInputProvider>(new HwndKeyboardInputProvider(this));
		_layoutHook = LayoutFilterMessage;
		_inputHook = InputFilterMessage;
		_hwndTargetHook = HwndTargetFilterMessage;
		_publicHook = PublicHooksFilterMessage;
		HwndWrapperHook[] array = new HwndWrapperHook[4] { _hwndTargetHook, _layoutHook, _inputHook, null };
		if (parameters.HwndSourceHook != null)
		{
			Delegate[] invocationList = parameters.HwndSourceHook.GetInvocationList();
			for (int num = invocationList.Length - 1; num >= 0; num--)
			{
				EventHelper.AddHandler(ref _hooks, (HwndSourceHook)invocationList[num]);
			}
			array[3] = _publicHook;
		}
		_restoreFocusMode = parameters.RestoreFocusMode;
		_acquireHwndFocusInMenuMode = parameters.AcquireHwndFocusInMenuMode;
		if (parameters.EffectivePerPixelOpacity)
		{
			parameters.ExtendedWindowStyle |= 524288;
		}
		else
		{
			parameters.ExtendedWindowStyle &= -524289;
		}
		_constructionParameters = parameters;
		_hwndWrapper = new HwndWrapper(parameters.WindowClassStyle, parameters.WindowStyle, parameters.ExtendedWindowStyle, parameters.PositionX, parameters.PositionY, parameters.Width, parameters.Height, parameters.WindowName, parameters.ParentWindow, array);
		_hwndTarget = new HwndTarget(_hwndWrapper.Handle);
		_hwndTarget.UsesPerPixelOpacity = parameters.EffectivePerPixelOpacity;
		if (_hwndTarget.UsesPerPixelOpacity)
		{
			_hwndTarget.BackgroundColor = Colors.Transparent;
			MS.Win32.UnsafeNativeMethods.CriticalSetWindowTheme(new HandleRef(this, _hwndWrapper.Handle), "", "");
		}
		_constructionParameters = null;
		if (!parameters.HasAssignedSize)
		{
			_sizeToContent = SizeToContent.WidthAndHeight;
		}
		_adjustSizingForNonClientArea = parameters.AdjustSizingForNonClientArea;
		_treatAncestorsAsNonClientArea = parameters.TreatAncestorsAsNonClientArea;
		_weakShutdownHandler = new WeakEventDispatcherShutdown(this, base.Dispatcher);
		_hwndWrapper.Disposed += OnHwndDisposed;
		if (StylusLogic.IsStylusAndTouchSupportEnabled)
		{
			if (StylusLogic.IsPointerStackEnabled)
			{
				_stylus = new SecurityCriticalDataClass<IStylusInputProvider>(new HwndPointerInputProvider(this));
			}
			else
			{
				_stylus = new SecurityCriticalDataClass<IStylusInputProvider>(new HwndStylusInputProvider(this));
			}
		}
		_appCommand = new SecurityCriticalDataClass<HwndAppCommandInputProvider>(new HwndAppCommandInputProvider(this));
		if (parameters.TreatAsInputRoot)
		{
			_weakPreprocessMessageHandler = new WeakEventPreprocessMessage(this, addToFront: false);
		}
		AddSource();
		if (_hwndWrapper.Handle != IntPtr.Zero)
		{
			DragDrop.RegisterDropTarget(_hwndWrapper.Handle);
			_registeredDropTargetCount++;
		}
	}

	/// <summary>Releases all managed resources that are used by the <see cref="T:System.Windows.Interop.HwndSource" />, and raises the <see cref="E:System.Windows.Interop.HwndSource.Disposed" /> event.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Adds an event handler that receives all window messages. </summary>
	/// <param name="hook">The handler implementation (based on the <see cref="T:System.Windows.Interop.HwndSourceHook" /> delegate) that receives the window messages.</param>
	public void AddHook(HwndSourceHook hook)
	{
		Verify.IsNotNull(hook, "hook");
		CheckDisposed(verifyAccess: true);
		if (_hooks == null)
		{
			_hwndWrapper.AddHook(_publicHook);
		}
		EventHelper.AddHandler(ref _hooks, hook);
	}

	/// <summary>Removes the event handlers that were added by <see cref="M:System.Windows.Interop.HwndSource.AddHook(System.Windows.Interop.HwndSourceHook)" />. </summary>
	/// <param name="hook">The event handler to remove.</param>
	public void RemoveHook(HwndSourceHook hook)
	{
		EventHelper.RemoveHandler(ref _hooks, hook);
		if (_hooks == null)
		{
			_hwndWrapper.RemoveHook(_publicHook);
		}
	}

	internal override IInputProvider GetInputProvider(Type inputDevice)
	{
		if (inputDevice == typeof(MouseDevice))
		{
			if (_mouse == null)
			{
				return null;
			}
			return _mouse.Value;
		}
		if (inputDevice == typeof(KeyboardDevice))
		{
			if (_keyboard == null)
			{
				return null;
			}
			return _keyboard.Value;
		}
		if (inputDevice == typeof(StylusDevice))
		{
			if (_stylus == null)
			{
				return null;
			}
			return _stylus.Value;
		}
		return null;
	}

	internal void ChangeDpi(HwndDpiChangedEventArgs e)
	{
		OnDpiChanged(e);
	}

	internal void ChangeDpi(HwndDpiChangedAfterParentEventArgs e)
	{
		OnDpiChangedAfterParent(e);
	}

	protected virtual void OnDpiChanged(HwndDpiChangedEventArgs e)
	{
		this.DpiChanged?.Invoke(this, e);
		if (!e.Handled)
		{
			_hwndTarget?.OnDpiChanged(e);
			if (IsLayoutActive())
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(SetLayoutSize));
				base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(PresentationSource.FireContentRendered), this);
			}
			else
			{
				InputManager.SafeCurrentNotifyHitTestInvalidated();
			}
		}
	}

	private void OnDpiChangedAfterParent(HwndDpiChangedAfterParentEventArgs e)
	{
		if (_hwndTarget != null)
		{
			HwndDpiChangedEventArgs hwndDpiChangedEventArgs = (HwndDpiChangedEventArgs)e;
			this.DpiChanged?.Invoke(this, hwndDpiChangedEventArgs);
			if (!hwndDpiChangedEventArgs.Handled)
			{
				_hwndTarget?.OnDpiChangedAfterParent(e);
			}
			if (IsLayoutActive())
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(SetLayoutSize));
				base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(PresentationSource.FireContentRendered), this);
			}
			else
			{
				InputManager.SafeCurrentNotifyHitTestInvalidated();
			}
		}
	}

	public static HwndSource FromHwnd(nint hwnd)
	{
		return CriticalFromHwnd(hwnd);
	}

	internal static HwndSource CriticalFromHwnd(nint hwnd)
	{
		if (hwnd == IntPtr.Zero)
		{
			throw new ArgumentException(SR.NullHwnd);
		}
		HwndSource result = null;
		WeakReferenceListEnumerator enumerator = PresentationSource.CriticalCurrentSources.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if ((PresentationSource)enumerator.Current is HwndSource hwndSource && hwndSource.CriticalHandle == hwnd)
			{
				if (!hwndSource.IsDisposed)
				{
					result = hwndSource;
				}
				break;
			}
		}
		return result;
	}

	/// <summary>Gets the visual target of the window. </summary>
	/// <returns>Returns the visual target of the window.</returns>
	protected override CompositionTarget GetCompositionTargetCore()
	{
		return CompositionTarget;
	}

	internal override void OnEnterMenuMode()
	{
		IsInExclusiveMenuMode = !_acquireHwndFocusInMenuMode;
		if (IsInExclusiveMenuMode)
		{
			_weakMenuModeMessageHandler = new WeakEventPreprocessMessage(this, addToFront: true);
			MS.Win32.UnsafeNativeMethods.HideCaret(new HandleRef(this, IntPtr.Zero));
		}
	}

	internal override void OnLeaveMenuMode()
	{
		if (IsInExclusiveMenuMode)
		{
			_weakMenuModeMessageHandler.Dispose();
			_weakMenuModeMessageHandler = null;
			MS.Win32.UnsafeNativeMethods.ShowCaret(new HandleRef(this, IntPtr.Zero));
		}
		IsInExclusiveMenuMode = false;
	}

	private void OnLayoutUpdated(object obj, EventArgs args)
	{
		if (_rootVisual.Value is UIElement { RenderSize: var renderSize } && (!_previousSize.HasValue || !DoubleUtil.AreClose(_previousSize.Value.Width, renderSize.Width) || !DoubleUtil.AreClose(_previousSize.Value.Height, renderSize.Height)))
		{
			_previousSize = renderSize;
			if (_sizeToContent != 0 && !_isWindowInMinimizeState)
			{
				Resize(renderSize);
			}
		}
	}

	private void Resize(Size newSize)
	{
		try
		{
			_myOwnUpdate = true;
			if (IsUsable)
			{
				MS.Win32.NativeMethods.RECT rECT = AdjustWindowSize(newSize);
				int cx = rECT.right - rECT.left;
				int cy = rECT.bottom - rECT.top;
				MS.Win32.UnsafeNativeMethods.SetWindowPos(new HandleRef(this, _hwndWrapper.Handle), new HandleRef(null, IntPtr.Zero), 0, 0, cx, cy, 22);
				if (this.AutoResized != null)
				{
					this.AutoResized(this, new AutoResizedEventArgs(newSize));
				}
			}
		}
		finally
		{
			_myOwnUpdate = false;
		}
	}

	internal void ShowSystemMenu()
	{
		nint ancestor = MS.Win32.UnsafeNativeMethods.GetAncestor(new HandleRef(this, CriticalHandle), 2);
		MS.Win32.UnsafeNativeMethods.PostMessage(new HandleRef(this, ancestor), WindowMessage.WM_SYSCOMMAND, new IntPtr(61696), new IntPtr(32));
	}

	internal Point TransformToDevice(Point pt)
	{
		return _hwndTarget.TransformToDevice.Transform(pt);
	}

	internal Point TransformFromDevice(Point pt)
	{
		return _hwndTarget.TransformFromDevice.Transform(pt);
	}

	private MS.Win32.NativeMethods.RECT AdjustWindowSize(Size newSize)
	{
		Point size = TransformToDevice(new Point(newSize.Width, newSize.Height));
		RoundDeviceSize(ref size);
		MS.Win32.NativeMethods.RECT lpRect = new MS.Win32.NativeMethods.RECT(0, 0, (int)size.X, (int)size.Y);
		if (!_adjustSizingForNonClientArea && !UsesPerPixelOpacity)
		{
			int dwStyle = MS.Win32.NativeMethods.IntPtrToInt32(SafeNativeMethods.GetWindowStyle(new HandleRef(this, _hwndWrapper.Handle), exStyle: false));
			int dwExStyle = MS.Win32.NativeMethods.IntPtrToInt32(SafeNativeMethods.GetWindowStyle(new HandleRef(this, _hwndWrapper.Handle), exStyle: true));
			SafeNativeMethods.AdjustWindowRectEx(ref lpRect, dwStyle, bMenu: false, dwExStyle);
		}
		return lpRect;
	}

	private void RoundDeviceSize(ref Point size)
	{
		if (_rootVisual.Value is UIElement { SnapsToDevicePixels: not false })
		{
			size = new Point(DoubleUtil.DoubleToInt(size.X), DoubleUtil.DoubleToInt(size.Y));
		}
		else
		{
			size = new Point(Math.Ceiling(size.X), Math.Ceiling(size.Y));
		}
	}

	/// <summary>Gets the window handle for the <see cref="T:System.Windows.Interop.HwndSource" />. The window handle is packaged as part of a <see cref="T:System.Runtime.InteropServices.HandleRef" /> structure. </summary>
	/// <returns>A structure that contains the window handle for this <see cref="T:System.Windows.Interop.HwndSource" />.</returns>
	public HandleRef CreateHandleRef()
	{
		return new HandleRef(this, Handle);
	}

	private bool IsLayoutActive()
	{
		if (_rootVisual.Value is UIElement && _hwndTarget != null && !_hwndTarget.IsDisposed)
		{
			return true;
		}
		return false;
	}

	private void SetLayoutSize()
	{
		UIElement uIElement = null;
		if (!(_rootVisual.Value is UIElement uIElement2))
		{
			return;
		}
		uIElement2.InvalidateMeasure();
		bool flag = EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info);
		long num = 0L;
		if (_sizeToContent == SizeToContent.WidthAndHeight)
		{
			Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
			if (flag)
			{
				num = ((IntPtr)_hwndWrapper.Handle).ToInt64();
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientLayoutBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, num, EventTrace.LayoutSource.HwndSource_SetLayoutSize);
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientMeasureBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, num);
			}
			uIElement2.Measure(availableSize);
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientMeasureEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, 1);
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientArrangeBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, num);
			}
			uIElement2.Arrange(new Rect(default(Point), uIElement2.DesiredSize));
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientArrangeEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, 1);
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientLayoutEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info);
			}
		}
		else
		{
			Size sizeFromHwnd = GetSizeFromHwnd();
			Size size = new Size((_sizeToContent == SizeToContent.Width) ? double.PositiveInfinity : sizeFromHwnd.Width, (_sizeToContent == SizeToContent.Height) ? double.PositiveInfinity : sizeFromHwnd.Height);
			if (flag)
			{
				num = ((IntPtr)_hwndWrapper.Handle).ToInt64();
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientLayoutBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, num, EventTrace.LayoutSource.HwndSource_SetLayoutSize);
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientMeasureBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, num);
			}
			uIElement2.Measure(size);
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientMeasureEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, 1);
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientArrangeBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, num);
			}
			size = ((_sizeToContent == SizeToContent.Width) ? new Size(uIElement2.DesiredSize.Width, sizeFromHwnd.Height) : ((_sizeToContent != SizeToContent.Height) ? sizeFromHwnd : new Size(sizeFromHwnd.Width, uIElement2.DesiredSize.Height)));
			uIElement2.Arrange(new Rect(default(Point), size));
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientArrangeEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, 1);
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientLayoutEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info);
			}
		}
		uIElement2.UpdateLayout();
	}

	private Size GetSizeFromHwnd()
	{
		MS.Win32.NativeMethods.RECT rect = new MS.Win32.NativeMethods.RECT(0, 0, 0, 0);
		if (_adjustSizingForNonClientArea)
		{
			GetNonClientRect(ref rect);
		}
		else
		{
			SafeNativeMethods.GetClientRect(new HandleRef(this, _hwndWrapper.Handle), ref rect);
		}
		Point point = TransformFromDevice(new Point(rect.right - rect.left, rect.bottom - rect.top));
		return new Size(point.X, point.Y);
	}

	private nint HwndTargetFilterMessage(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		nint num = IntPtr.Zero;
		if (IsUsable)
		{
			HwndTarget hwndTarget = _hwndTarget;
			if (hwndTarget != null)
			{
				num = hwndTarget.HandleMessage((WindowMessage)msg, wParam, lParam);
				if (num != IntPtr.Zero)
				{
					handled = true;
				}
			}
		}
		return num;
	}

	private nint LayoutFilterMessage(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		nint zero = IntPtr.Zero;
		UIElement uIElement = null;
		uIElement = _rootVisual.Value as UIElement;
		if (IsUsable && uIElement != null)
		{
			switch (msg)
			{
			case 274:
			{
				int num = MS.Win32.NativeMethods.IntPtrToInt32(wParam) & 0xFFF0;
				if (num == 61488 || num == 61440)
				{
					DisableSizeToContent(uIElement, hwnd);
				}
				break;
			}
			case 532:
				DisableSizeToContent(uIElement, hwnd);
				break;
			case 70:
				Process_WM_WINDOWPOSCHANGING(uIElement, hwnd, (WindowMessage)msg, wParam, lParam);
				break;
			case 5:
				Process_WM_SIZE(uIElement, hwnd, (WindowMessage)msg, wParam, lParam);
				break;
			}
		}
		if (!handled && (_constructionParameters != null || IsUsable))
		{
			bool flag = ((_constructionParameters != null) ? ((HwndSourceParameters)_constructionParameters).EffectivePerPixelOpacity : _hwndTarget.UsesPerPixelOpacity);
			if (msg == 131 && flag)
			{
				if (wParam == IntPtr.Zero)
				{
					zero = IntPtr.Zero;
					handled = true;
				}
				else
				{
					zero = IntPtr.Zero;
					handled = true;
				}
			}
		}
		return zero;
	}

	private void Process_WM_WINDOWPOSCHANGING(UIElement rootUIElement, nint hwnd, WindowMessage msg, nint wParam, nint lParam)
	{
		if (_myOwnUpdate || SizeToContent == SizeToContent.Manual)
		{
			return;
		}
		MS.Win32.NativeMethods.RECT rECT = AdjustWindowSize(rootUIElement.RenderSize);
		int num = rECT.right - rECT.left;
		int num2 = rECT.bottom - rECT.top;
		MS.Win32.NativeMethods.WINDOWPOS structure = Marshal.PtrToStructure<MS.Win32.NativeMethods.WINDOWPOS>(lParam);
		bool flag = false;
		if ((structure.flags & 1) == 1)
		{
			MS.Win32.NativeMethods.RECT rect = new MS.Win32.NativeMethods.RECT(0, 0, 0, 0);
			SafeNativeMethods.GetWindowRect(new HandleRef(this, _hwndWrapper.Handle), ref rect);
			if (num != rect.right - rect.left || num2 != rect.bottom - rect.top)
			{
				structure.flags &= -2;
				structure.cx = num;
				structure.cy = num2;
				flag = true;
			}
		}
		else
		{
			bool num3 = SizeToContent != SizeToContent.Height;
			bool flag2 = SizeToContent != SizeToContent.Width;
			if (num3 && structure.cx != num)
			{
				structure.cx = num;
				flag = true;
			}
			if (flag2 && structure.cy != num2)
			{
				structure.cy = num2;
				flag = true;
			}
		}
		if (flag)
		{
			Marshal.StructureToPtr(structure, lParam, fDeleteOld: true);
		}
	}

	private void Process_WM_SIZE(UIElement rootUIElement, nint hwnd, WindowMessage msg, nint wParam, nint lParam)
	{
		int num = MS.Win32.NativeMethods.SignedLOWORD(lParam);
		int num2 = MS.Win32.NativeMethods.SignedHIWORD(lParam);
		Point point = new Point(num, num2);
		bool flag = EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info);
		long num3 = 0L;
		_isWindowInMinimizeState = MS.Win32.NativeMethods.IntPtrToInt32(wParam) == 1;
		if (!_myOwnUpdate && _sizeToContent != SizeToContent.WidthAndHeight && !_isWindowInMinimizeState)
		{
			Point pt = new Point(point.X, point.Y);
			if (_adjustSizingForNonClientArea)
			{
				MS.Win32.NativeMethods.RECT rc = new MS.Win32.NativeMethods.RECT(0, 0, (int)point.X, (int)point.Y);
				GetNonClientRect(ref rc);
				pt.X = rc.Width;
				pt.Y = rc.Height;
			}
			pt = TransformFromDevice(pt);
			Size size = new Size((_sizeToContent == SizeToContent.Width) ? double.PositiveInfinity : pt.X, (_sizeToContent == SizeToContent.Height) ? double.PositiveInfinity : pt.Y);
			if (_adjustSizingForNonClientArea)
			{
				rootUIElement.InvalidateMeasure();
			}
			if (flag)
			{
				num3 = ((IntPtr)_hwndWrapper.Handle).ToInt64();
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientLayoutBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, num3, EventTrace.LayoutSource.HwndSource_WMSIZE);
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientMeasureBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, num3);
			}
			rootUIElement.Measure(size);
			size = ((_sizeToContent == SizeToContent.Width) ? new Size(rootUIElement.DesiredSize.Width, pt.Y) : ((_sizeToContent != SizeToContent.Height) ? new Size(pt.X, pt.Y) : new Size(pt.X, rootUIElement.DesiredSize.Height)));
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientMeasureEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, 1);
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientArrangeBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, num3);
			}
			rootUIElement.Arrange(new Rect(default(Point), size));
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientArrangeEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info, 1);
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientLayoutEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordLayout, EventTrace.Level.Info);
			}
			rootUIElement.UpdateLayout();
		}
	}

	private void DisableSizeToContent(UIElement rootUIElement, nint hwnd)
	{
		if (_sizeToContent != 0)
		{
			_sizeToContent = SizeToContent.Manual;
			Size sizeFromHwnd = GetSizeFromHwnd();
			rootUIElement.Measure(sizeFromHwnd);
			rootUIElement.Arrange(new Rect(default(Point), sizeFromHwnd));
			rootUIElement.UpdateLayout();
			if (this.SizeToContentChanged != null)
			{
				this.SizeToContentChanged(this, EventArgs.Empty);
			}
		}
	}

	private void GetNonClientRect(ref MS.Win32.NativeMethods.RECT rc)
	{
		nint zero = IntPtr.Zero;
		zero = ((!_treatAncestorsAsNonClientArea) ? CriticalHandle : MS.Win32.UnsafeNativeMethods.GetAncestor(new HandleRef(this, CriticalHandle), 2));
		SafeNativeMethods.GetWindowRect(new HandleRef(this, zero), ref rc);
	}

	private nint InputFilterMessage(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		nint result = IntPtr.Zero;
		if (msg == 2)
		{
			DisposeStylusInputProvider();
		}
		if (!_isDisposed && _stylus != null && !handled)
		{
			result = _stylus.Value.FilterMessage(hwnd, (WindowMessage)msg, wParam, lParam, ref handled);
		}
		if (!_isDisposed && _mouse != null && !handled)
		{
			result = _mouse.Value.FilterMessage(hwnd, (WindowMessage)msg, wParam, lParam, ref handled);
		}
		if (!_isDisposed && _keyboard != null && !handled)
		{
			result = _keyboard.Value.FilterMessage(hwnd, (WindowMessage)msg, wParam, lParam, ref handled);
			if ((uint)(msg - 256) <= 7u)
			{
				_lastKeyboardMessage = default(MSG);
			}
		}
		if (!_isDisposed && _appCommand != null && !handled)
		{
			result = _appCommand.Value.FilterMessage(hwnd, (WindowMessage)msg, wParam, lParam, ref handled);
		}
		return result;
	}

	private nint PublicHooksFilterMessage(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		nint result = IntPtr.Zero;
		if (_hooks != null)
		{
			Delegate[] item = _hooks.Item2;
			for (int num = item.Length - 1; num >= 0; num--)
			{
				result = ((HwndSourceHook)item[num])(hwnd, msg, wParam, lParam, ref handled);
				if (handled)
				{
					break;
				}
			}
		}
		switch (msg)
		{
		case 130:
			OnNoMoreWindowMessages();
			break;
		case 2:
			DisposeStylusInputProvider();
			break;
		}
		return result;
	}

	private void DisposeStylusInputProvider()
	{
		if (_stylus != null)
		{
			SecurityCriticalDataClass<IStylusInputProvider> stylus = _stylus;
			_stylus = null;
			stylus.Value.Dispose();
		}
	}

	private void OnPreprocessMessageThunk(ref MSG msg, ref bool handled)
	{
		if (handled)
		{
			return;
		}
		WindowMessage message = (WindowMessage)msg.message;
		if ((uint)(message - 256) <= 7u)
		{
			MSGDATA mSGDATA = new MSGDATA();
			mSGDATA.msg = msg;
			mSGDATA.handled = handled;
			object obj = Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Send, new DispatcherOperationCallback(OnPreprocessMessage), mSGDATA);
			if (obj != null)
			{
				handled = (bool)obj;
			}
			msg = mSGDATA.msg;
		}
	}

	private object OnPreprocessMessage(object param)
	{
		MSGDATA mSGDATA = (MSGDATA)param;
		if (!((IKeyboardInputSink)this).HasFocusWithin() && !IsInExclusiveMenuMode)
		{
			return mSGDATA.handled;
		}
		ModifierKeys systemModifierKeys = HwndKeyboardInputProvider.GetSystemModifierKeys();
		switch ((WindowMessage)mSGDATA.msg.message)
		{
		case WindowMessage.WM_KEYFIRST:
		case WindowMessage.WM_SYSKEYDOWN:
		{
			_eatCharMessages = true;
			DispatcherOperation dispatcherOperation = base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(RestoreCharMessages), null);
			base.Dispatcher.CriticalRequestProcessing(force: true);
			mSGDATA.handled = CriticalTranslateAccelerator(ref mSGDATA.msg, systemModifierKeys);
			if (!mSGDATA.handled)
			{
				_eatCharMessages = false;
				dispatcherOperation.Abort();
			}
			if (IsInExclusiveMenuMode)
			{
				if (!mSGDATA.handled)
				{
					MS.Win32.UnsafeNativeMethods.TranslateMessage(ref mSGDATA.msg);
				}
				mSGDATA.handled = true;
			}
			break;
		}
		case WindowMessage.WM_KEYUP:
		case WindowMessage.WM_SYSKEYUP:
			mSGDATA.handled = CriticalTranslateAccelerator(ref mSGDATA.msg, systemModifierKeys);
			if (IsInExclusiveMenuMode)
			{
				mSGDATA.handled = true;
			}
			break;
		case WindowMessage.WM_CHAR:
		case WindowMessage.WM_DEADCHAR:
		case WindowMessage.WM_SYSCHAR:
		case WindowMessage.WM_SYSDEADCHAR:
			if (!_eatCharMessages)
			{
				mSGDATA.handled = ((IKeyboardInputSink)this).TranslateChar(ref mSGDATA.msg, systemModifierKeys);
				if (!mSGDATA.handled)
				{
					mSGDATA.handled = ((IKeyboardInputSink)this).OnMnemonic(ref mSGDATA.msg, systemModifierKeys);
				}
				if (!mSGDATA.handled)
				{
					_keyboard.Value.ProcessTextInputAction(mSGDATA.msg.hwnd, (WindowMessage)mSGDATA.msg.message, mSGDATA.msg.wParam, mSGDATA.msg.lParam, ref mSGDATA.handled);
				}
			}
			if (IsInExclusiveMenuMode)
			{
				if (!mSGDATA.handled)
				{
					SafeNativeMethods.MessageBeep(0);
				}
				mSGDATA.handled = true;
			}
			break;
		}
		return mSGDATA.handled;
	}

	/// <summary>Registers the <see cref="T:System.Windows.Interop.IKeyboardInputSink" /> interface of a contained component. </summary>
	/// <returns>The <see cref="T:System.Windows.Interop.IKeyboardInputSite" /> site of the contained component.</returns>
	/// <param name="sink">The <see cref="T:System.Windows.Interop.IKeyboardInputSink" /> sink of the contained component.</param>
	protected IKeyboardInputSite RegisterKeyboardInputSinkCore(IKeyboardInputSink sink)
	{
		CheckDisposed(verifyAccess: true);
		if (sink == null)
		{
			throw new ArgumentNullException("sink");
		}
		if (sink.KeyboardInputSite != null)
		{
			throw new ArgumentException(SR.KeyboardSinkAlreadyOwned);
		}
		HwndSourceKeyboardInputSite hwndSourceKeyboardInputSite = new HwndSourceKeyboardInputSite(this, sink);
		if (_keyboardInputSinkChildren == null)
		{
			_keyboardInputSinkChildren = new List<HwndSourceKeyboardInputSite>();
		}
		_keyboardInputSinkChildren.Add(hwndSourceKeyboardInputSite);
		return hwndSourceKeyboardInputSite;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Interop.IKeyboardInputSink.RegisterKeyboardInputSink(System.Windows.Interop.IKeyboardInputSink)" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Interop.IKeyboardInputSite" /> site of the contained component.</returns>
	/// <param name="sink">The <see cref="T:System.Windows.Interop.IKeyboardInputSink" /> sink of the contained component.</param>
	IKeyboardInputSite IKeyboardInputSink.RegisterKeyboardInputSink(IKeyboardInputSink sink)
	{
		return RegisterKeyboardInputSinkCore(sink);
	}

	/// <summary>Processes keyboard input at the key-down message level.</summary>
	/// <returns>true if the message was handled by the method implementation; otherwise, false.</returns>
	/// <param name="msg">The message and associated data. Do not modify this structure. It is passed by reference for performance reasons only.</param>
	/// <param name="modifiers">Modifier keys.</param>
	protected virtual bool TranslateAcceleratorCore(ref MSG msg, ModifierKeys modifiers)
	{
		return CriticalTranslateAccelerator(ref msg, modifiers);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Interop.IKeyboardInputSink.TranslateAccelerator(System.Windows.Interop.MSG@,System.Windows.Input.ModifierKeys)" />.</summary>
	/// <returns>true if the message was handled by the method implementation; otherwise, false.</returns>
	/// <param name="msg">The message and associated data. Do not modify this structure. It is passed by reference for performance reasons only.</param>
	/// <param name="modifiers">Modifier keys.</param>
	bool IKeyboardInputSink.TranslateAccelerator(ref MSG msg, ModifierKeys modifiers)
	{
		return TranslateAcceleratorCore(ref msg, modifiers);
	}

	/// <summary>Sets focus on either the first tab stop or the last tab stop of the sink.</summary>
	/// <returns>true if the focus has been set as requested; false, if there are no tab stops.</returns>
	/// <param name="request">Specifies whether focus should be set to the first or the last tab stop.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="request" /> is null.</exception>
	protected virtual bool TabIntoCore(TraversalRequest request)
	{
		bool result = false;
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (_rootVisual.Value is UIElement uIElement)
		{
			result = uIElement.MoveFocus(request);
		}
		return result;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Interop.IKeyboardInputSink.TabInto(System.Windows.Input.TraversalRequest)" />.</summary>
	/// <returns>true if the focus has been set as requested; false, if there are no tab stops.</returns>
	/// <param name="request">Specifies whether focus should be set to the first or the last tab stop.</param>
	bool IKeyboardInputSink.TabInto(TraversalRequest request)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		return TabIntoCore(request);
	}

	/// <summary>Called when one of the mnemonics (access keys) for this sink is invoked. </summary>
	/// <returns>true if the message was handled; otherwise, false.</returns>
	/// <param name="msg">The message for the mnemonic and associated data.</param>
	/// <param name="modifiers">Modifier keys.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="msg" /> is not WM_KEYDOWN, WM_SYSKEYDOWN, WM_CHAR, or WM_DEADCHAR.</exception>
	protected virtual bool OnMnemonicCore(ref MSG msg, ModifierKeys modifiers)
	{
		switch ((WindowMessage)msg.message)
		{
		case WindowMessage.WM_SYSCHAR:
		case WindowMessage.WM_SYSDEADCHAR:
		{
			string text = new string((char)msg.wParam, 1);
			if (text != null && text.Length > 0)
			{
				HwndSource hwndSource = ((!(Keyboard.FocusedElement is DependencyObject v)) ? null : (PresentationSource.CriticalFromVisual(v) as HwndSource));
				if (hwndSource != null && hwndSource != this && IsInExclusiveMenuMode)
				{
					return ((IKeyboardInputSink)hwndSource).OnMnemonic(ref msg, modifiers);
				}
				if (AccessKeyManager.IsKeyRegistered(this, text))
				{
					AccessKeyManager.ProcessKey(this, text, isMultiple: false);
					return true;
				}
			}
			break;
		}
		default:
			throw new ArgumentException(SR.OnlyAcceptsKeyMessages);
		case WindowMessage.WM_CHAR:
		case WindowMessage.WM_DEADCHAR:
			break;
		}
		_lastKeyboardMessage = msg;
		if (_keyboardInputSinkChildren != null && !IsInExclusiveMenuMode)
		{
			foreach (HwndSourceKeyboardInputSite keyboardInputSinkChild in _keyboardInputSinkChildren)
			{
				if (((IKeyboardInputSite)keyboardInputSinkChild).Sink.OnMnemonic(ref msg, modifiers))
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Interop.IKeyboardInputSink.OnMnemonic(System.Windows.Interop.MSG@,System.Windows.Input.ModifierKeys)" />.</summary>
	/// <returns>true if the message was handled; otherwise, false.</returns>
	/// <param name="msg">The message for the mnemonic and associated data. Do not modify this message structure. It is passed by reference for performance reasons only.</param>
	/// <param name="modifiers">Modifier keys.</param>
	bool IKeyboardInputSink.OnMnemonic(ref MSG msg, ModifierKeys modifiers)
	{
		return OnMnemonicCore(ref msg, modifiers);
	}

	/// <summary>Processes WM_CHAR, WM_SYSCHAR, WM_DEADCHAR, and WM_SYSDEADCHAR input messages before the <see cref="M:System.Windows.Interop.IKeyboardInputSink.OnMnemonic(System.Windows.Interop.MSG@,System.Windows.Input.ModifierKeys)" /> method is called.</summary>
	/// <returns>true if the message was processed and <see cref="M:System.Windows.Interop.IKeyboardInputSink.OnMnemonic(System.Windows.Interop.MSG@,System.Windows.Input.ModifierKeys)" /> should not be called; otherwise, false.</returns>
	/// <param name="msg">The message and associated data. Do not modify this structure. It is passed by reference for performance reasons only.</param>
	/// <param name="modifiers">Modifier keys.</param>
	protected virtual bool TranslateCharCore(ref MSG msg, ModifierKeys modifiers)
	{
		if (HasFocus || IsInExclusiveMenuMode)
		{
			return false;
		}
		return ChildSinkWithFocus?.TranslateChar(ref msg, modifiers) ?? false;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Interop.IKeyboardInputSink.TranslateChar(System.Windows.Interop.MSG@,System.Windows.Input.ModifierKeys)" />.</summary>
	/// <returns>true if the message was processed and <see cref="M:System.Windows.Interop.IKeyboardInputSink.OnMnemonic(System.Windows.Interop.MSG@,System.Windows.Input.ModifierKeys)" /> should not be called; otherwise, false.</returns>
	/// <param name="msg">The message and associated data. Do not modify this structure. It is passed by reference for performance reasons only.</param>
	/// <param name="modifiers">Modifier keys.</param>
	bool IKeyboardInputSink.TranslateChar(ref MSG msg, ModifierKeys modifiers)
	{
		return TranslateCharCore(ref msg, modifiers);
	}

	/// <summary>Gets a value that indicates whether the sink or one of its contained components has focus. </summary>
	/// <returns>true if the sink or one of its contained components has focus; otherwise, false.</returns>
	protected virtual bool HasFocusWithinCore()
	{
		if (HasFocus)
		{
			return true;
		}
		if (_keyboardInputSinkChildren == null)
		{
			return false;
		}
		foreach (HwndSourceKeyboardInputSite keyboardInputSinkChild in _keyboardInputSinkChildren)
		{
			if (((IKeyboardInputSite)keyboardInputSinkChild).Sink.HasFocusWithin())
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Media.FamilyTypefaceCollection.System#Collections#IList#Remove(System.Object)" />.</summary>
	/// <returns>true if the sink or one of its contained components has focus; otherwise, false.</returns>
	bool IKeyboardInputSink.HasFocusWithin()
	{
		return HasFocusWithinCore();
	}

	internal void CriticalUnregisterKeyboardInputSink(HwndSourceKeyboardInputSite site)
	{
		if (_isDisposed || _keyboardInputSinkChildren == null || _keyboardInputSinkChildren.Remove(site))
		{
			return;
		}
		throw new InvalidOperationException(SR.KeyboardSinkNotAChild);
	}

	internal bool CriticalTranslateAccelerator(ref MSG msg, ModifierKeys modifiers)
	{
		WindowMessage message = (WindowMessage)msg.message;
		if ((uint)(message - 256) > 1u && (uint)(message - 260) > 1u)
		{
			throw new ArgumentException(SR.OnlyAcceptsKeyMessages);
		}
		if (_keyboard == null)
		{
			return false;
		}
		bool handled = false;
		if (PerThreadData.TranslateAcceleratorCallDepth == 0)
		{
			_lastKeyboardMessage = msg;
			if (HasFocus || IsInExclusiveMenuMode)
			{
				_keyboard.Value.ProcessKeyAction(ref msg, ref handled);
			}
			else
			{
				IInputElement forceTarget = (IInputElement)ChildSinkWithFocus;
				try
				{
					PerThreadData.TranslateAcceleratorCallDepth++;
					Keyboard.PrimaryDevice.ForceTarget = forceTarget;
					_keyboard.Value.ProcessKeyAction(ref msg, ref handled);
				}
				finally
				{
					Keyboard.PrimaryDevice.ForceTarget = null;
					PerThreadData.TranslateAcceleratorCallDepth--;
				}
			}
		}
		else
		{
			int virtualKey = HwndKeyboardInputProvider.GetVirtualKey(msg.wParam, msg.lParam);
			int scanCode = HwndKeyboardInputProvider.GetScanCode(msg.wParam, msg.lParam);
			bool isExtendedKey = HwndKeyboardInputProvider.IsExtendedKey(msg.lParam);
			Key key = KeyInterop.KeyFromVirtualKey(virtualKey);
			RoutedEvent routedEvent = null;
			RoutedEvent routedEvent2 = null;
			switch ((WindowMessage)msg.message)
			{
			case WindowMessage.WM_KEYUP:
			case WindowMessage.WM_SYSKEYUP:
				routedEvent = Keyboard.PreviewKeyUpEvent;
				routedEvent2 = Keyboard.KeyUpEvent;
				break;
			case WindowMessage.WM_KEYFIRST:
			case WindowMessage.WM_SYSKEYDOWN:
				routedEvent = Keyboard.PreviewKeyDownEvent;
				routedEvent2 = Keyboard.KeyDownEvent;
				break;
			}
			bool hasFocus = HasFocus;
			IKeyboardInputSink keyboardInputSink = ((hasFocus || IsInExclusiveMenuMode) ? null : ChildSinkWithFocus);
			IInputElement inputElement = keyboardInputSink as IInputElement;
			if (inputElement == null && hasFocus)
			{
				inputElement = Keyboard.PrimaryDevice.FocusedElement;
				if (inputElement != null && PresentationSource.CriticalFromVisual((DependencyObject)inputElement) != this)
				{
					inputElement = null;
				}
			}
			try
			{
				Keyboard.PrimaryDevice.ForceTarget = keyboardInputSink as IInputElement;
				if (inputElement != null)
				{
					KeyEventArgs keyEventArgs = new KeyEventArgs(Keyboard.PrimaryDevice, this, msg.time, key);
					keyEventArgs.ScanCode = scanCode;
					keyEventArgs.IsExtendedKey = isExtendedKey;
					keyEventArgs.RoutedEvent = routedEvent;
					inputElement.RaiseEvent(keyEventArgs);
					handled = keyEventArgs.Handled;
				}
				if (!handled)
				{
					KeyEventArgs keyEventArgs2 = new KeyEventArgs(Keyboard.PrimaryDevice, this, msg.time, key);
					keyEventArgs2.ScanCode = scanCode;
					keyEventArgs2.IsExtendedKey = isExtendedKey;
					keyEventArgs2.RoutedEvent = routedEvent2;
					if (inputElement != null)
					{
						inputElement.RaiseEvent(keyEventArgs2);
						handled = keyEventArgs2.Handled;
					}
					if (!handled)
					{
						InputManager.UnsecureCurrent.RaiseTranslateAccelerator(keyEventArgs2);
						handled = keyEventArgs2.Handled;
					}
				}
			}
			finally
			{
				Keyboard.PrimaryDevice.ForceTarget = null;
			}
		}
		return handled;
	}

	internal static object RestoreCharMessages(object unused)
	{
		_eatCharMessages = false;
		return null;
	}

	internal bool IsRepeatedKeyboardMessage(nint hwnd, int msg, nint wParam, nint lParam)
	{
		if (msg != _lastKeyboardMessage.message)
		{
			return false;
		}
		if (hwnd != _lastKeyboardMessage.hwnd)
		{
			return false;
		}
		if (wParam != _lastKeyboardMessage.wParam)
		{
			return false;
		}
		if (lParam != _lastKeyboardMessage.lParam)
		{
			return false;
		}
		return true;
	}

	private void OnHwndDisposed(object sender, EventArgs args)
	{
		_inRealHwndDispose = true;
		Dispose();
	}

	private void OnNoMoreWindowMessages()
	{
		_hooks = null;
	}

	private void OnShutdownFinished(object sender, EventArgs args)
	{
		Dispose();
	}

	private void Dispose(bool disposing)
	{
		if (!disposing || _isDisposing)
		{
			return;
		}
		_isDisposing = true;
		if (this.Disposed != null)
		{
			try
			{
				this.Disposed(this, EventArgs.Empty);
			}
			catch
			{
			}
			this.Disposed = null;
		}
		ClearContentRenderedListeners();
		RootVisualInternal = null;
		RemoveSource();
		IKeyboardInputSite keyboardInputSite = ((IKeyboardInputSink)this).KeyboardInputSite;
		if (keyboardInputSite != null)
		{
			keyboardInputSite.Unregister();
			((IKeyboardInputSink)this).KeyboardInputSite = null;
		}
		_keyboardInputSinkChildren = null;
		if (!_inRealHwndDispose)
		{
			DisposeStylusInputProvider();
		}
		if (_hwndTarget != null)
		{
			_hwndTarget.Dispose();
			_hwndTarget = null;
		}
		if (_hwndWrapper != null)
		{
			if (_hwndWrapper.Handle != IntPtr.Zero && _registeredDropTargetCount > 0)
			{
				DragDrop.RevokeDropTarget(_hwndWrapper.Handle);
				_registeredDropTargetCount--;
			}
			_hwndWrapper.Disposed -= OnHwndDisposed;
			if (!_inRealHwndDispose)
			{
				_hwndWrapper.Dispose();
			}
		}
		if (_mouse != null)
		{
			_mouse.Value.Dispose();
			_mouse = null;
		}
		if (_keyboard != null)
		{
			_keyboard.Value.Dispose();
			_keyboard = null;
		}
		if (_appCommand != null)
		{
			_appCommand.Value.Dispose();
			_appCommand = null;
		}
		if (_weakShutdownHandler != null)
		{
			_weakShutdownHandler.Dispose();
			_weakShutdownHandler = null;
		}
		if (_weakPreprocessMessageHandler != null)
		{
			_weakPreprocessMessageHandler.Dispose();
			_weakPreprocessMessageHandler = null;
		}
		_isDisposed = true;
	}

	private void CheckDisposed(bool verifyAccess)
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException(null, SR.HwndSourceDisposed);
		}
	}

	private static bool IsValidSizeToContent(SizeToContent value)
	{
		if (value != 0 && value != SizeToContent.Width && value != SizeToContent.Height)
		{
			return value == SizeToContent.WidthAndHeight;
		}
		return true;
	}
}
