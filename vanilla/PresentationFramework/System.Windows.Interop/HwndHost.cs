#define TRACE
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Interop;
using MS.Win32;

namespace System.Windows.Interop;

/// <summary>Hosts a Win32 window as an element within Windows Presentation Foundation (WPF) content. </summary>
public abstract class HwndHost : FrameworkElement, IDisposable, IWin32Window, IKeyboardInputSink
{
	private class WeakEventDispatcherShutdown : WeakReference
	{
		private Dispatcher _that;

		public WeakEventDispatcherShutdown(HwndHost hwndHost, Dispatcher that)
			: base(hwndHost)
		{
			_that = that;
			_that.ShutdownFinished += OnShutdownFinished;
		}

		public void OnShutdownFinished(object sender, EventArgs e)
		{
			if (Target is HwndHost hwndHost)
			{
				hwndHost.OnDispatcherShutdown(sender, e);
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

	public static readonly RoutedEvent DpiChangedEvent;

	private DependencyPropertyChangedEventHandler _handlerEnabledChanged;

	private DependencyPropertyChangedEventHandler _handlerVisibleChanged;

	private EventHandler _handlerLayoutUpdated;

	private HwndSubclass _hwndSubclass;

	private HwndWrapperHook _hwndSubclassHook;

	private HandleRef _hwnd;

	private ArrayList _hooks;

	private Size _desiredSize;

	private bool _hasDpiAwarenessContextTransition;

	private MS.Internal.SecurityCriticalDataForSet<bool> _fTrusted;

	private bool _isBuildingWindow;

	private bool _isDisposed;

	private WeakEventDispatcherShutdown _weakEventDispatcherShutdown;

	/// <summary>Gets the window handle  of the hosted window. </summary>
	/// <returns>The window handle.</returns>
	public nint Handle => CriticalHandle;

	/// <summary>For a description of this member, see <see cref="P:System.Windows.Interop.IKeyboardInputSink.KeyboardInputSite" />.</summary>
	/// <returns>A reference to the container's <see cref="T:System.Windows.Interop.IKeyboardInputSite" /> interface.</returns>
	IKeyboardInputSite IKeyboardInputSink.KeyboardInputSite { get; set; }

	private double DpiParentToChildRatio
	{
		get
		{
			if (!_hasDpiAwarenessContextTransition)
			{
				return 1.0;
			}
			DpiScale2 windowDpi = DpiUtil.GetWindowDpi(Handle, fallbackToNearestMonitorHeuristic: false);
			DpiScale2 windowDpi2 = DpiUtil.GetWindowDpi(MS.Win32.UnsafeNativeMethods.GetParent(_hwnd), fallbackToNearestMonitorHeuristic: false);
			if (windowDpi == null || windowDpi2 == null)
			{
				return 1.0;
			}
			return windowDpi2.DpiScaleX / windowDpi.DpiScaleX;
		}
	}

	internal nint CriticalHandle
	{
		get
		{
			if (_hwnd.Handle != IntPtr.Zero && !MS.Win32.UnsafeNativeMethods.IsWindow(_hwnd))
			{
				_hwnd = new HandleRef(null, IntPtr.Zero);
			}
			return _hwnd.Handle;
		}
	}

	/// <summary>Occurs for each unhandled message that is received by the hosted window. </summary>
	public event HwndSourceHook MessageHook
	{
		add
		{
			if (_hooks == null)
			{
				_hooks = new ArrayList(8);
			}
			_hooks.Add(value);
		}
		remove
		{
			if (_hooks != null)
			{
				_hooks.Remove(value);
				if (_hooks.Count == 0)
				{
					_hooks = null;
				}
			}
		}
	}

	public event DpiChangedEventHandler DpiChanged
	{
		add
		{
			AddHandler(DpiChangedEvent, value);
		}
		remove
		{
			RemoveHandler(DpiChangedEvent, value);
		}
	}

	static HwndHost()
	{
		UIElement.FocusableProperty.OverrideMetadata(typeof(HwndHost), new FrameworkPropertyMetadata(true));
		DpiChangedEvent = Window.DpiChangedEvent.AddOwner(typeof(HwndHost));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Interop.HwndHost" /> class. </summary>
	protected HwndHost()
	{
		Initialize(fTrusted: false);
	}

	internal HwndHost(bool fTrusted)
	{
		Initialize(fTrusted);
	}

	/// <summary>Performs the final cleanup before the garbage collector destroys the object. </summary>
	~HwndHost()
	{
		Dispose(disposing: false);
	}

	/// <summary>Immediately frees any system resources that the object might hold. </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary> Called when the hosted window receives a WM_KEYUP message. </summary>
	/// <param name="e">The event data.</param>
	protected override void OnKeyUp(KeyEventArgs e)
	{
		MSG msg = ((!_fTrusted.Value) ? ComponentDispatcher.CurrentKeyboardMessage : ComponentDispatcher.UnsecureCurrentKeyboardMessage);
		ModifierKeys systemModifierKeys = HwndKeyboardInputProvider.GetSystemModifierKeys();
		bool flag = ((IKeyboardInputSink)this).TranslateAccelerator(ref msg, systemModifierKeys);
		if (flag)
		{
			e.Handled = flag;
		}
		base.OnKeyUp(e);
	}

	protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
	{
		RaiseEvent(new DpiChangedEventArgs(oldDpi, newDpi, DpiChangedEvent, this));
		UpdateWindowPos();
	}

	/// <summary>Called when the hosted window receives a WM_KEYDOWN message. </summary>
	/// <param name="e">The event data.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		MSG msg = ((!_fTrusted.Value) ? ComponentDispatcher.CurrentKeyboardMessage : ComponentDispatcher.UnsecureCurrentKeyboardMessage);
		ModifierKeys systemModifierKeys = HwndKeyboardInputProvider.GetSystemModifierKeys();
		bool flag = ((IKeyboardInputSink)this).TranslateAccelerator(ref msg, systemModifierKeys);
		if (flag)
		{
			e.Handled = flag;
		}
		base.OnKeyDown(e);
	}

	/// <summary>Registers the <see cref="T:System.Windows.Interop.IKeyboardInputSink" /> interface of a contained component.  </summary>
	/// <returns>The <see cref="T:System.Windows.Interop.IKeyboardInputSite" /> site of the contained component.</returns>
	/// <param name="sink">The <see cref="T:System.Windows.Interop.IKeyboardInputSink" /> sink of the contained component.</param>
	protected virtual IKeyboardInputSite RegisterKeyboardInputSinkCore(IKeyboardInputSink sink)
	{
		throw new InvalidOperationException(SR.HwndHostDoesNotSupportChildKeyboardSinks);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Interop.IKeyboardInputSink.RegisterKeyboardInputSink(System.Windows.Interop.IKeyboardInputSink)" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Interop.IKeyboardInputSite" /> site of the contained component.</returns>
	/// <param name="sink">The <see cref="T:System.Windows.Interop.IKeyboardInputSink" /> sink of the contained component.</param>
	IKeyboardInputSite IKeyboardInputSink.RegisterKeyboardInputSink(IKeyboardInputSink sink)
	{
		return RegisterKeyboardInputSinkCore(sink);
	}

	/// <summary>Processes keyboard input at the keydown message level.</summary>
	/// <returns>Always returns false.</returns>
	/// <param name="msg">The message and associated data. Do not modify this structure. It is passed by reference for performance reasons only.</param>
	/// <param name="modifiers">Modifier keys.</param>
	protected virtual bool TranslateAcceleratorCore(ref MSG msg, ModifierKeys modifiers)
	{
		return false;
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
	/// <returns>Always returns false.</returns>
	/// <param name="request">Specifies whether focus should be set to the first or the last tab stop.</param>
	protected virtual bool TabIntoCore(TraversalRequest request)
	{
		return false;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Interop.IKeyboardInputSink.TabInto(System.Windows.Input.TraversalRequest)" />.</summary>
	/// <returns>true if the focus has been set as requested; false, if there are no tab stops.</returns>
	/// <param name="request">Specifies whether focus should be set to the first or the last tab stop.</param>
	bool IKeyboardInputSink.TabInto(TraversalRequest request)
	{
		return TabIntoCore(request);
	}

	/// <summary>Called when one of the mnemonics (access keys) for this sink is invoked. </summary>
	/// <returns>Always returns false. </returns>
	/// <param name="msg">The message for the mnemonic and associated data.</param>
	/// <param name="modifiers">Modifier keys.</param>
	protected virtual bool OnMnemonicCore(ref MSG msg, ModifierKeys modifiers)
	{
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
	/// <returns>Always returns false.</returns>
	/// <param name="msg">The message and associated data. Do not modify this structure. It is passed by reference for performance reasons only.</param>
	/// <param name="modifiers">Modifier keys.</param>
	protected virtual bool TranslateCharCore(ref MSG msg, ModifierKeys modifiers)
	{
		return false;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Interop.IKeyboardInputSink.TranslateChar(System.Windows.Interop.MSG@,System.Windows.Input.ModifierKeys)" />.</summary>
	/// <returns>true if the message was processed and <see cref="M:System.Windows.Interop.IKeyboardInputSink.OnMnemonic(System.Windows.Interop.MSG@,System.Windows.Input.ModifierKeys)" /> should not be called; otherwise, false.</returns>
	/// <param name="msg">The message and associated data. Do not modify this structure. It is passed by reference for performance reasons only.</param>
	/// <param name="modifiers">Modifier keys.</param>
	bool IKeyboardInputSink.TranslateChar(ref MSG msg, ModifierKeys modifiers)
	{
		return TranslateCharCore(ref msg, modifiers);
	}

	/// <summary>Gets a value that indicates whether the sink or one of its contained components has focus.</summary>
	/// <returns>true if the sink or one of its contained components has focus; otherwise, false.</returns>
	protected virtual bool HasFocusWithinCore()
	{
		HandleRef hwnd = new HandleRef(this, MS.Win32.UnsafeNativeMethods.GetFocus());
		if (Handle != IntPtr.Zero && (hwnd.Handle == _hwnd.Handle || MS.Win32.UnsafeNativeMethods.IsChild(_hwnd, hwnd)))
		{
			return true;
		}
		return false;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Media.FamilyTypefaceCollection.System#Collections#IList#Remove(System.Object)" />.</summary>
	/// <returns>true if the sink or one of its contained components has focus; otherwise, false.</returns>
	bool IKeyboardInputSink.HasFocusWithin()
	{
		return HasFocusWithinCore();
	}

	/// <summary> Updates the child window's size, visibility, and position to reflect the current state of the element. </summary>
	public void UpdateWindowPos()
	{
		if (_isDisposed)
		{
			return;
		}
		PresentationSource presentationSource = null;
		CompositionTarget compositionTarget = null;
		if (CriticalHandle != IntPtr.Zero && base.IsVisible)
		{
			presentationSource = PresentationSource.CriticalFromVisual(this, enable2DTo3DTransition: false);
			if (presentationSource != null)
			{
				compositionTarget = presentationSource.CompositionTarget;
			}
		}
		if (compositionTarget != null && compositionTarget.RootVisual != null)
		{
			Rect rcBoundingBox = PointUtil.ToRect(CalculateAssignedRC(presentationSource));
			OnWindowPositionChanged(rcBoundingBox);
			MS.Win32.UnsafeNativeMethods.ShowWindowAsync(_hwnd, 5);
		}
		else
		{
			MS.Win32.UnsafeNativeMethods.ShowWindowAsync(_hwnd, 0);
		}
	}

	private MS.Win32.NativeMethods.RECT CalculateAssignedRC(PresentationSource source)
	{
		Rect rect = PointUtil.RootToClient(PointUtil.ElementToRoot(new Rect(base.RenderSize), this, source), source);
		MS.Win32.NativeMethods.RECT rECT = PointUtil.AdjustForRightToLeft(handleRef: new HandleRef(null, MS.Win32.UnsafeNativeMethods.GetParent(_hwnd)), rc: PointUtil.FromRect(rect));
		if (!CoreAppContextSwitches.DoNotUsePresentationDpiCapabilityTier2OrGreater)
		{
			rECT = AdjustRectForDpi(rECT);
		}
		return rECT;
	}

	private MS.Win32.NativeMethods.RECT AdjustRectForDpi(MS.Win32.NativeMethods.RECT rcRect)
	{
		if (_hasDpiAwarenessContextTransition)
		{
			double dpiParentToChildRatio = DpiParentToChildRatio;
			rcRect.left = (int)((double)rcRect.left / dpiParentToChildRatio);
			rcRect.top = (int)((double)rcRect.top / dpiParentToChildRatio);
			rcRect.right = (int)((double)rcRect.right / dpiParentToChildRatio);
			rcRect.bottom = (int)((double)rcRect.bottom / dpiParentToChildRatio);
		}
		return rcRect;
	}

	/// <summary>Immediately frees any system resources that the object might hold. </summary>
	/// <param name="disposing">Set to true if called from an explicit disposer and false otherwise.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (_isDisposed)
		{
			return;
		}
		if (disposing)
		{
			VerifyAccess();
			if (_hwndSubclass != null)
			{
				if (_fTrusted.Value)
				{
					_hwndSubclass.CriticalDetach(force: false);
				}
				else
				{
					_hwndSubclass.RequestDetach(force: false);
				}
				_hwndSubclass = null;
			}
			_hooks = null;
			PresentationSource.RemoveSourceChangedHandler(this, OnSourceChanged);
		}
		if (_weakEventDispatcherShutdown != null)
		{
			_weakEventDispatcherShutdown.Dispose();
			_weakEventDispatcherShutdown = null;
		}
		DestroyWindow();
		_isDisposed = true;
	}

	private void OnDispatcherShutdown(object sender, EventArgs e)
	{
		Dispose();
	}

	/// <summary>When overridden in a derived class, creates the window to be hosted. </summary>
	/// <returns>The handle to the child Win32 window to create.</returns>
	/// <param name="hwndParent">The window handle of the parent window.</param>
	protected abstract HandleRef BuildWindowCore(HandleRef hwndParent);

	/// <summary>When overridden in a derived class, destroys the hosted window. </summary>
	/// <param name="hwnd">A structure that contains the window handle.</param>
	protected abstract void DestroyWindowCore(HandleRef hwnd);

	protected unsafe virtual nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		DemandIfUntrusted();
		switch ((WindowMessage)msg)
		{
		case WindowMessage.WM_NCDESTROY:
			_hwnd = new HandleRef(null, IntPtr.Zero);
			break;
		case WindowMessage.WM_WINDOWPOSCHANGING:
		{
			PresentationSource presentationSource = PresentationSource.CriticalFromVisual(this, enable2DTo3DTransition: false);
			if (presentationSource != null)
			{
				MS.Win32.NativeMethods.RECT rECT = CalculateAssignedRC(presentationSource);
				((MS.Win32.NativeMethods.WINDOWPOS*)lParam)->cx = rECT.right - rECT.left;
				((MS.Win32.NativeMethods.WINDOWPOS*)lParam)->cy = rECT.bottom - rECT.top;
				((MS.Win32.NativeMethods.WINDOWPOS*)lParam)->flags &= -2;
				((MS.Win32.NativeMethods.WINDOWPOS*)lParam)->x = rECT.left;
				((MS.Win32.NativeMethods.WINDOWPOS*)lParam)->y = rECT.top;
				((MS.Win32.NativeMethods.WINDOWPOS*)lParam)->flags &= -3;
				((MS.Win32.NativeMethods.WINDOWPOS*)lParam)->flags |= 256;
			}
			break;
		}
		case WindowMessage.WM_GETOBJECT:
			handled = true;
			return OnWmGetObject(wParam, lParam);
		}
		return IntPtr.Zero;
	}

	/// <summary>Creates an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for <see cref="T:System.Windows.Interop.HwndHost" /> . </summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation. </returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new HwndHostAutomationPeer(this);
	}

	private nint OnWmGetObject(nint wparam, nint lparam)
	{
		nint result = IntPtr.Zero;
		AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(this);
		if (automationPeer != null)
		{
			IRawElementProviderSimple interopChild = automationPeer.GetInteropChild();
			result = AutomationInteropProvider.ReturnRawElementProvider(CriticalHandle, wparam, lparam, interopChild);
		}
		return result;
	}

	/// <summary> Called when the hosted window's position changes. </summary>
	/// <param name="rcBoundingBox">The window's position.</param>
	protected virtual void OnWindowPositionChanged(Rect rcBoundingBox)
	{
		if (!_isDisposed)
		{
			MS.Win32.UnsafeNativeMethods.SetWindowPos(_hwnd, new HandleRef(null, IntPtr.Zero), (int)rcBoundingBox.X, (int)rcBoundingBox.Y, (int)rcBoundingBox.Width, (int)rcBoundingBox.Height, 16660);
		}
	}

	/// <summary>Returns the size of the window represented by the <see cref="T:System.Windows.Interop.HwndHost" /> object, as requested by layout engine operations. </summary>
	/// <returns>The size of the <see cref="T:System.Windows.Interop.HwndHost" /> object.</returns>
	/// <param name="constraint">The size of the <see cref="T:System.Windows.Interop.HwndHost" /> object.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		DemandIfUntrusted();
		Size result = new Size(0.0, 0.0);
		if (CriticalHandle != IntPtr.Zero)
		{
			result.Width = Math.Min(_desiredSize.Width, constraint.Width);
			result.Height = Math.Min(_desiredSize.Height, constraint.Height);
		}
		return result;
	}

	internal override DrawingGroup GetDrawing()
	{
		return GetDrawingHelper();
	}

	internal override Rect GetContentBounds()
	{
		return new Rect(base.RenderSize);
	}

	private DrawingGroup GetDrawingHelper()
	{
		DrawingGroup drawingGroup = null;
		if (Handle != IntPtr.Zero)
		{
			MS.Win32.NativeMethods.RECT rect = default(MS.Win32.NativeMethods.RECT);
			SafeNativeMethods.GetWindowRect(_hwnd, ref rect);
			int num = rect.right - rect.left;
			int num2 = rect.bottom - rect.top;
			HandleRef hDC = new HandleRef(this, MS.Win32.UnsafeNativeMethods.GetDC(new HandleRef(this, IntPtr.Zero)));
			if (hDC.Handle != IntPtr.Zero)
			{
				HandleRef handleRef = new HandleRef(this, IntPtr.Zero);
				HandleRef hObject = new HandleRef(this, IntPtr.Zero);
				try
				{
					handleRef = new HandleRef(this, MS.Win32.UnsafeNativeMethods.CriticalCreateCompatibleDC(hDC));
					if (handleRef.Handle != IntPtr.Zero)
					{
						hObject = new HandleRef(this, MS.Win32.UnsafeNativeMethods.CriticalCreateCompatibleBitmap(hDC, num, num2));
						if (hObject.Handle != IntPtr.Zero)
						{
							nint obj = MS.Win32.UnsafeNativeMethods.CriticalSelectObject(handleRef, hObject.Handle);
							try
							{
								MS.Win32.NativeMethods.RECT rcFill = new MS.Win32.NativeMethods.RECT(0, 0, num, num2);
								nint brush = MS.Win32.UnsafeNativeMethods.CriticalGetStockObject(0);
								MS.Win32.UnsafeNativeMethods.CriticalFillRect(handleRef.Handle, ref rcFill, brush);
								if (!MS.Win32.UnsafeNativeMethods.CriticalPrintWindow(_hwnd, handleRef, 0))
								{
									MS.Win32.UnsafeNativeMethods.SendMessage(_hwnd.Handle, WindowMessage.WM_PRINT, handleRef.Handle, 30);
								}
								else
								{
									MS.Win32.UnsafeNativeMethods.CriticalRedrawWindow(_hwnd, IntPtr.Zero, IntPtr.Zero, 129);
								}
								drawingGroup = new DrawingGroup();
								BitmapSource imageSource = Imaging.CriticalCreateBitmapSourceFromHBitmap(hObject.Handle, IntPtr.Zero, Int32Rect.Empty, null, WICBitmapAlphaChannelOption.WICBitmapIgnoreAlpha);
								Rect rect2 = new Rect(base.RenderSize);
								drawingGroup.Children.Add(new ImageDrawing(imageSource, rect2));
								drawingGroup.Freeze();
							}
							finally
							{
								MS.Win32.UnsafeNativeMethods.CriticalSelectObject(handleRef, obj);
							}
						}
					}
				}
				finally
				{
					MS.Win32.UnsafeNativeMethods.ReleaseDC(new HandleRef(this, IntPtr.Zero), hDC);
					hDC = new HandleRef(null, IntPtr.Zero);
					if (hObject.Handle != IntPtr.Zero)
					{
						MS.Win32.UnsafeNativeMethods.DeleteObject(hObject);
						hObject = new HandleRef(this, IntPtr.Zero);
					}
					if (handleRef.Handle != IntPtr.Zero)
					{
						MS.Win32.UnsafeNativeMethods.CriticalDeleteDC(handleRef);
						handleRef = new HandleRef(this, IntPtr.Zero);
					}
				}
			}
		}
		return drawingGroup;
	}

	private void Initialize(bool fTrusted)
	{
		_fTrusted = new MS.Internal.SecurityCriticalDataForSet<bool>(fTrusted);
		_hwndSubclassHook = SubclassWndProc;
		_handlerLayoutUpdated = OnLayoutUpdated;
		_handlerEnabledChanged = OnEnabledChanged;
		_handlerVisibleChanged = OnVisibleChanged;
		PresentationSource.AddSourceChangedHandler(this, OnSourceChanged);
		_weakEventDispatcherShutdown = new WeakEventDispatcherShutdown(this, base.Dispatcher);
	}

	private void DemandIfUntrusted()
	{
		_ = _fTrusted.Value;
	}

	private void OnSourceChanged(object sender, SourceChangedEventArgs e)
	{
		IKeyboardInputSite keyboardInputSite = ((IKeyboardInputSink)this).KeyboardInputSite;
		if (keyboardInputSite != null)
		{
			((IKeyboardInputSink)this).KeyboardInputSite = null;
			keyboardInputSite.Unregister();
		}
		if (PresentationSource.CriticalFromVisual(this, enable2DTo3DTransition: false) is IKeyboardInputSink keyboardInputSink)
		{
			((IKeyboardInputSink)this).KeyboardInputSite = keyboardInputSink.RegisterKeyboardInputSink(this);
		}
		BuildOrReparentWindow();
	}

	private void OnLayoutUpdated(object sender, EventArgs e)
	{
		UpdateWindowPos();
	}

	private void OnEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (!_isDisposed)
		{
			bool enable = (bool)e.NewValue;
			MS.Win32.UnsafeNativeMethods.EnableWindow(_hwnd, enable);
		}
	}

	private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (!_isDisposed)
		{
			if ((bool)e.NewValue)
			{
				MS.Win32.UnsafeNativeMethods.ShowWindowAsync(_hwnd, 8);
			}
			else
			{
				MS.Win32.UnsafeNativeMethods.ShowWindowAsync(_hwnd, 0);
			}
		}
	}

	private void BuildOrReparentWindow()
	{
		DemandIfUntrusted();
		if (_isBuildingWindow || _isDisposed)
		{
			return;
		}
		_isBuildingWindow = true;
		nint num = IntPtr.Zero;
		PresentationSource presentationSource = PresentationSource.CriticalFromVisual(this, enable2DTo3DTransition: false);
		if (presentationSource != null)
		{
			if (presentationSource is HwndSource hwndSource)
			{
				num = hwndSource.CriticalHandle;
			}
		}
		else if (PresentationSource.CriticalFromVisual(this, enable2DTo3DTransition: true) != null && TraceHwndHost.IsEnabled)
		{
			TraceHwndHost.Trace(TraceEventType.Warning, TraceHwndHost.HwndHostIn3D);
		}
		try
		{
			if (num != IntPtr.Zero)
			{
				if (_hwnd.Handle == IntPtr.Zero)
				{
					BuildWindow(new HandleRef(null, num));
					base.LayoutUpdated += _handlerLayoutUpdated;
					base.IsEnabledChanged += _handlerEnabledChanged;
					base.IsVisibleChanged += _handlerVisibleChanged;
				}
				else if (num != MS.Win32.UnsafeNativeMethods.GetParent(_hwnd))
				{
					MS.Win32.UnsafeNativeMethods.SetParent(_hwnd, new HandleRef(null, num));
				}
			}
			else if (Handle != IntPtr.Zero)
			{
				HwndWrapper dpiAwarenessCompatibleNotificationWindow = SystemResources.GetDpiAwarenessCompatibleNotificationWindow(_hwnd);
				if (dpiAwarenessCompatibleNotificationWindow != null)
				{
					MS.Win32.UnsafeNativeMethods.SetParent(_hwnd, new HandleRef(null, dpiAwarenessCompatibleNotificationWindow.Handle));
					SystemResources.DelayHwndShutdown();
				}
				else
				{
					Trace.WriteLineIf(dpiAwarenessCompatibleNotificationWindow == null, "- Warning - Notification Window is null\n" + new StackTrace(fNeedFileInfo: true).ToString());
				}
			}
		}
		finally
		{
			_isBuildingWindow = false;
		}
	}

	private void BuildWindow(HandleRef hwndParent)
	{
		DemandIfUntrusted();
		_hwnd = BuildWindowCore(hwndParent);
		if (_hwnd.Handle == IntPtr.Zero || !MS.Win32.UnsafeNativeMethods.IsWindow(_hwnd))
		{
			throw new InvalidOperationException(SR.ChildWindowNotCreated);
		}
		if ((MS.Win32.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, _hwnd.Handle), -16) & 0x40000000) == 0)
		{
			throw new InvalidOperationException(SR.HostedWindowMustBeAChildWindow);
		}
		if (hwndParent.Handle != MS.Win32.UnsafeNativeMethods.GetParent(_hwnd))
		{
			throw new InvalidOperationException(SR.ChildWindowMustHaveCorrectParent);
		}
		if (DpiUtil.GetDpiAwarenessContext(_hwnd.Handle) != DpiUtil.GetDpiAwarenessContext(hwndParent.Handle))
		{
			_hasDpiAwarenessContextTransition = true;
		}
		if (MS.Win32.UnsafeNativeMethods.GetWindowThreadProcessId(_hwnd, out var lpdwProcessId) == SafeNativeMethods.GetCurrentThreadId() && lpdwProcessId == SafeNativeMethods.GetCurrentProcessId())
		{
			_hwndSubclass = new HwndSubclass(_hwndSubclassHook);
			_hwndSubclass.CriticalAttach(_hwnd.Handle);
		}
		MS.Win32.UnsafeNativeMethods.ShowWindowAsync(_hwnd, 0);
		MS.Win32.NativeMethods.RECT rect = default(MS.Win32.NativeMethods.RECT);
		SafeNativeMethods.GetWindowRect(_hwnd, ref rect);
		PresentationSource presentationSource = PresentationSource.CriticalFromVisual(this, enable2DTo3DTransition: false);
		Point point = new Point(rect.left, rect.top);
		Point point2 = new Point(rect.right, rect.bottom);
		point = presentationSource.CompositionTarget.TransformFromDevice.Transform(point);
		point2 = presentationSource.CompositionTarget.TransformFromDevice.Transform(point2);
		_desiredSize = new Size(point2.X - point.X, point2.Y - point.Y);
		InvalidateMeasure();
	}

	private void DestroyWindow()
	{
		if (CriticalHandle != IntPtr.Zero)
		{
			if (!CheckAccess())
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(AsyncDestroyWindow), null);
				return;
			}
			HandleRef hwnd = _hwnd;
			_hwnd = new HandleRef(null, IntPtr.Zero);
			DestroyWindowCore(hwnd);
		}
	}

	private object AsyncDestroyWindow(object arg)
	{
		DestroyWindow();
		return null;
	}

	private nint SubclassWndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		nint zero = IntPtr.Zero;
		zero = WndProc(hwnd, msg, wParam, lParam, ref handled);
		if (!handled && _hooks != null)
		{
			int i = 0;
			for (int count = _hooks.Count; i < count; i++)
			{
				zero = ((HwndSourceHook)_hooks[i])(hwnd, msg, wParam, lParam, ref handled);
				if (handled)
				{
					break;
				}
			}
		}
		return zero;
	}
}
