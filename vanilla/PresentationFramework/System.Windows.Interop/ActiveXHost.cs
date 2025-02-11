using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using MS.Internal;
using MS.Internal.Controls;
using MS.Win32;

namespace System.Windows.Interop;

/// <summary>Hosts an ActiveX control as an element within Windows Presentation Foundation (WPF) content. </summary>
public class ActiveXHost : HwndHost
{
	private delegate void PropertyInvalidator(ActiveXHost axhost);

	internal static readonly DependencyProperty TabIndexProperty;

	private static Hashtable invalidatorMap;

	private MS.Win32.NativeMethods.COMRECT _bounds = new MS.Win32.NativeMethods.COMRECT(0, 0, 0, 0);

	private Rect _boundRect = new Rect(0.0, 0.0, 0.0, 0.0);

	private Size _cachedSize = Size.Empty;

	private HandleRef _hwndParent;

	private bool _isDisposed;

	private MS.Internal.SecurityCriticalDataForSet<Guid> _clsid;

	private HandleRef _axWindow;

	private BitVector32 _axHostState;

	private ActiveXHelper.ActiveXState _axState;

	private ActiveXSite _axSite;

	private ActiveXContainer _axContainer;

	private object _axInstance;

	private MS.Win32.UnsafeNativeMethods.IOleObject _axOleObject;

	private MS.Win32.UnsafeNativeMethods.IOleInPlaceObject _axOleInPlaceObject;

	private MS.Win32.UnsafeNativeMethods.IOleInPlaceActiveObject _axOleInPlaceActiveObject;

	/// <summary>Gets a value that indicates whether the <see cref="M:System.Windows.Interop.ActiveXHost.Dispose(System.Boolean)" /> method has been called on the <see cref="T:System.Windows.Interop.ActiveXHost" /> instance. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Interop.ActiveXHost" /> instance has been disposed; otherwise, false. The default is false.</returns>
	protected bool IsDisposed => _isDisposed;

	internal ActiveXSite ActiveXSite
	{
		get
		{
			if (_axSite == null)
			{
				_axSite = CreateActiveXSite();
			}
			return _axSite;
		}
	}

	internal ActiveXContainer Container
	{
		get
		{
			if (_axContainer == null)
			{
				_axContainer = new ActiveXContainer(this);
			}
			return _axContainer;
		}
	}

	internal ActiveXHelper.ActiveXState ActiveXState
	{
		get
		{
			return _axState;
		}
		set
		{
			_axState = value;
		}
	}

	internal int TabIndex
	{
		get
		{
			return (int)GetValue(TabIndexProperty);
		}
		set
		{
			SetValue(TabIndexProperty, value);
		}
	}

	internal HandleRef ParentHandle
	{
		get
		{
			return _hwndParent;
		}
		set
		{
			_hwndParent = value;
		}
	}

	internal MS.Win32.NativeMethods.COMRECT Bounds
	{
		get
		{
			return _bounds;
		}
		set
		{
			_bounds = value;
		}
	}

	internal Rect BoundRect => _boundRect;

	internal HandleRef ControlHandle => _axWindow;

	internal object ActiveXInstance => _axInstance;

	internal MS.Win32.UnsafeNativeMethods.IOleInPlaceObject ActiveXInPlaceObject => _axOleInPlaceObject;

	internal MS.Win32.UnsafeNativeMethods.IOleInPlaceActiveObject ActiveXInPlaceActiveObject => _axOleInPlaceActiveObject;

	static ActiveXHost()
	{
		TabIndexProperty = Control.TabIndexProperty.AddOwner(typeof(ActiveXHost));
		invalidatorMap = new Hashtable();
		invalidatorMap[UIElement.VisibilityProperty] = new PropertyInvalidator(OnVisibilityInvalidated);
		invalidatorMap[UIElement.IsEnabledProperty] = new PropertyInvalidator(OnIsEnabledInvalidated);
		EventManager.RegisterClassHandler(typeof(ActiveXHost), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(OnAccessKeyPressed));
		Control.IsTabStopProperty.OverrideMetadata(typeof(ActiveXHost), new FrameworkPropertyMetadata(true));
		UIElement.FocusableProperty.OverrideMetadata(typeof(ActiveXHost), new FrameworkPropertyMetadata(true));
		EventManager.RegisterClassHandler(typeof(ActiveXHost), Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotFocus));
		EventManager.RegisterClassHandler(typeof(ActiveXHost), Keyboard.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnLostFocus));
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(ActiveXHost), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
	}

	internal ActiveXHost(Guid clsid, bool fTrusted)
		: base(fTrusted)
	{
		if (Thread.CurrentThread.ApartmentState != 0)
		{
			throw new ThreadStateException(SR.Format(SR.AxRequiresApartmentThread, clsid.ToString()));
		}
		_clsid.Value = clsid;
		base.Initialized += OnInitialized;
	}

	/// <summary>Invoked whenever the effective value of any dependency property on this <see cref="T:System.Windows.FrameworkElement" /> has been updated. The specific dependency property that changed is reported in the arguments parameter. Overrides <see cref="M:System.Windows.DependencyObject.OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs)" />.</summary>
	/// <param name="e">The event data that describes the property that changed, as well as old and new values.</param>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.IsAValueChange || e.IsASubPropertyChange)
		{
			DependencyProperty property = e.Property;
			if (property != null && invalidatorMap.ContainsKey(property))
			{
				((PropertyInvalidator)invalidatorMap[property])(this);
			}
		}
	}

	/// <summary>Creates the <see cref="T:System.Windows.Interop.ActiveXHost" /> window and assigns it to a parent.</summary>
	/// <returns>A <see cref="T:System.Runtime.InteropServices.HandleRef" /> to the <see cref="T:System.Windows.Interop.ActiveXHost" /> window.</returns>
	/// <param name="hwndParent">The parent window.</param>
	protected override HandleRef BuildWindowCore(HandleRef hwndParent)
	{
		ParentHandle = hwndParent;
		TransitionUpTo(ActiveXHelper.ActiveXState.InPlaceActive);
		Invariant.Assert(_axOleInPlaceActiveObject != null, "InPlace activation of ActiveX control failed");
		if (ControlHandle.Handle == IntPtr.Zero)
		{
			nint hwnd = IntPtr.Zero;
			_axOleInPlaceActiveObject.GetWindow(out hwnd);
			AttachWindow(hwnd);
		}
		return _axWindow;
	}

	/// <summary>Called when the hosted window's position changes. </summary>
	/// <param name="bounds">The window's position.</param>
	protected override void OnWindowPositionChanged(Rect bounds)
	{
		_boundRect = bounds;
		_bounds.left = (int)bounds.X;
		_bounds.top = (int)bounds.Y;
		_bounds.right = (int)(bounds.Width + bounds.X);
		_bounds.bottom = (int)(bounds.Height + bounds.Y);
		ActiveXSite.OnActiveXRectChange(_bounds);
	}

	/// <summary>Destroys the hosted window.</summary>
	/// <param name="hwnd">A structure that contains the window handle.</param>
	protected override void DestroyWindowCore(HandleRef hwnd)
	{
	}

	/// <summary>Returns the size of the window represented by the <see cref="T:System.Windows.Interop.HwndHost" /> object, as requested by layout engine operations. </summary>
	/// <returns>The size of the <see cref="T:System.Windows.Interop.HwndHost" /> object.</returns>
	/// <param name="swConstraint">The size of the <see cref="T:System.Windows.Interop.HwndHost" /> object.</param>
	protected override Size MeasureOverride(Size swConstraint)
	{
		base.MeasureOverride(swConstraint);
		double width = ((!double.IsPositiveInfinity(swConstraint.Width)) ? swConstraint.Width : 150.0);
		double height = ((!double.IsPositiveInfinity(swConstraint.Height)) ? swConstraint.Height : 150.0);
		return new Size(width, height);
	}

	/// <summary>Provides class handling for when an access key that is meaningful for this element is invoked. </summary>
	/// <param name="args">The event data to the access key event. The event data reports which key was invoked, and indicate whether the <see cref="T:System.Windows.Input.AccessKeyManager" /> object that controls the sending of these events also sent this access key invocation to other elements.</param>
	protected override void OnAccessKey(AccessKeyEventArgs args)
	{
	}

	/// <summary>Releases the unmanaged resources that are used by the <see cref="T:System.Windows.Interop.ActiveXHost" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && !_isDisposed)
			{
				TransitionDownTo(ActiveXHelper.ActiveXState.Passive);
				_isDisposed = true;
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	internal virtual ActiveXSite CreateActiveXSite()
	{
		return new ActiveXSite(this);
	}

	internal virtual object CreateActiveXObject(Guid clsid)
	{
		return Activator.CreateInstance(Type.GetTypeFromCLSID(clsid));
	}

	internal virtual void AttachInterfaces(object nativeActiveXObject)
	{
	}

	internal virtual void DetachInterfaces()
	{
	}

	internal virtual void CreateSink()
	{
	}

	internal virtual void DetachSink()
	{
	}

	internal virtual void OnActiveXStateChange(int oldState, int newState)
	{
	}

	internal void RegisterAccessKey(char key)
	{
		AccessKeyManager.Register(key.ToString(), this);
	}

	internal bool GetAxHostState(int mask)
	{
		return _axHostState[mask];
	}

	internal void SetAxHostState(int mask, bool value)
	{
		_axHostState[mask] = value;
	}

	internal void TransitionUpTo(ActiveXHelper.ActiveXState state)
	{
		if (GetAxHostState(ActiveXHelper.inTransition))
		{
			return;
		}
		SetAxHostState(ActiveXHelper.inTransition, value: true);
		try
		{
			while (state > ActiveXState)
			{
				ActiveXHelper.ActiveXState activeXState = ActiveXState;
				switch (ActiveXState)
				{
				case ActiveXHelper.ActiveXState.Passive:
					TransitionFromPassiveToLoaded();
					ActiveXState = ActiveXHelper.ActiveXState.Loaded;
					break;
				case ActiveXHelper.ActiveXState.Loaded:
					TransitionFromLoadedToRunning();
					ActiveXState = ActiveXHelper.ActiveXState.Running;
					break;
				case ActiveXHelper.ActiveXState.Running:
					TransitionFromRunningToInPlaceActive();
					ActiveXState = ActiveXHelper.ActiveXState.InPlaceActive;
					break;
				case ActiveXHelper.ActiveXState.InPlaceActive:
					TransitionFromInPlaceActiveToUIActive();
					ActiveXState = ActiveXHelper.ActiveXState.UIActive;
					break;
				default:
					ActiveXState++;
					break;
				}
				OnActiveXStateChange((int)activeXState, (int)ActiveXState);
			}
		}
		finally
		{
			SetAxHostState(ActiveXHelper.inTransition, value: false);
		}
	}

	internal void TransitionDownTo(ActiveXHelper.ActiveXState state)
	{
		if (GetAxHostState(ActiveXHelper.inTransition))
		{
			return;
		}
		SetAxHostState(ActiveXHelper.inTransition, value: true);
		try
		{
			while (state < ActiveXState)
			{
				ActiveXHelper.ActiveXState activeXState = ActiveXState;
				switch (ActiveXState)
				{
				case ActiveXHelper.ActiveXState.Open:
					ActiveXState = ActiveXHelper.ActiveXState.UIActive;
					break;
				case ActiveXHelper.ActiveXState.UIActive:
					TransitionFromUIActiveToInPlaceActive();
					ActiveXState = ActiveXHelper.ActiveXState.InPlaceActive;
					break;
				case ActiveXHelper.ActiveXState.InPlaceActive:
					TransitionFromInPlaceActiveToRunning();
					ActiveXState = ActiveXHelper.ActiveXState.Running;
					break;
				case ActiveXHelper.ActiveXState.Running:
					TransitionFromRunningToLoaded();
					ActiveXState = ActiveXHelper.ActiveXState.Loaded;
					break;
				case ActiveXHelper.ActiveXState.Loaded:
					TransitionFromLoadedToPassive();
					ActiveXState = ActiveXHelper.ActiveXState.Passive;
					break;
				default:
					ActiveXState--;
					break;
				}
				OnActiveXStateChange((int)activeXState, (int)ActiveXState);
			}
		}
		finally
		{
			SetAxHostState(ActiveXHelper.inTransition, value: false);
		}
	}

	internal bool DoVerb(int verb)
	{
		return _axOleObject.DoVerb(verb, IntPtr.Zero, ActiveXSite, 0, ParentHandle.Handle, _bounds) == 0;
	}

	internal void AttachWindow(nint hwnd)
	{
		if (_axWindow.Handle != hwnd)
		{
			_axWindow = new HandleRef(this, hwnd);
			if (ParentHandle.Handle != IntPtr.Zero)
			{
				MS.Win32.UnsafeNativeMethods.SetParent(_axWindow, ParentHandle);
			}
		}
	}

	private void StartEvents()
	{
		if (!GetAxHostState(ActiveXHelper.sinkAttached))
		{
			SetAxHostState(ActiveXHelper.sinkAttached, value: true);
			CreateSink();
		}
		ActiveXSite.StartEvents();
	}

	private void StopEvents()
	{
		if (GetAxHostState(ActiveXHelper.sinkAttached))
		{
			SetAxHostState(ActiveXHelper.sinkAttached, value: false);
			DetachSink();
		}
		ActiveXSite.StopEvents();
	}

	private void TransitionFromPassiveToLoaded()
	{
		if (ActiveXState == ActiveXHelper.ActiveXState.Passive)
		{
			_axInstance = CreateActiveXObject(_clsid.Value);
			ActiveXState = ActiveXHelper.ActiveXState.Loaded;
			AttachInterfacesInternal();
		}
	}

	private void TransitionFromLoadedToPassive()
	{
		if (ActiveXState == ActiveXHelper.ActiveXState.Loaded)
		{
			if (_axInstance != null)
			{
				DetachInterfacesInternal();
				Marshal.FinalReleaseComObject(_axInstance);
				_axInstance = null;
			}
			ActiveXState = ActiveXHelper.ActiveXState.Passive;
		}
	}

	private void TransitionFromLoadedToRunning()
	{
		if (ActiveXState == ActiveXHelper.ActiveXState.Loaded)
		{
			int misc = 0;
			if (MS.Win32.NativeMethods.Succeeded(_axOleObject.GetMiscStatus(1, out misc)) && (misc & 0x20000) != 0)
			{
				_axOleObject.SetClientSite(ActiveXSite);
			}
			StartEvents();
			ActiveXState = ActiveXHelper.ActiveXState.Running;
		}
	}

	private void TransitionFromRunningToLoaded()
	{
		if (ActiveXState == ActiveXHelper.ActiveXState.Running)
		{
			StopEvents();
			_axOleObject.SetClientSite(null);
			ActiveXState = ActiveXHelper.ActiveXState.Loaded;
		}
	}

	private void TransitionFromRunningToInPlaceActive()
	{
		if (ActiveXState != ActiveXHelper.ActiveXState.Running)
		{
			return;
		}
		try
		{
			DoVerb(-5);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw new TargetInvocationException(SR.Format(SR.AXNohWnd, GetType().Name), ex);
		}
		ActiveXState = ActiveXHelper.ActiveXState.InPlaceActive;
	}

	private void TransitionFromInPlaceActiveToRunning()
	{
		if (ActiveXState == ActiveXHelper.ActiveXState.InPlaceActive)
		{
			_axOleInPlaceObject.InPlaceDeactivate();
			ActiveXState = ActiveXHelper.ActiveXState.Running;
		}
	}

	private void TransitionFromInPlaceActiveToUIActive()
	{
		if (ActiveXState == ActiveXHelper.ActiveXState.InPlaceActive)
		{
			DoVerb(-4);
			ActiveXState = ActiveXHelper.ActiveXState.UIActive;
		}
	}

	private void TransitionFromUIActiveToInPlaceActive()
	{
		if (ActiveXState == ActiveXHelper.ActiveXState.UIActive)
		{
			_axOleInPlaceObject.UIDeactivate();
			ActiveXState = ActiveXHelper.ActiveXState.InPlaceActive;
		}
	}

	private void OnInitialized(object sender, EventArgs e)
	{
		base.Initialized -= OnInitialized;
	}

	private static void OnIsEnabledInvalidated(ActiveXHost axHost)
	{
	}

	private static void OnVisibilityInvalidated(ActiveXHost axHost)
	{
		if (axHost != null)
		{
			switch (axHost.Visibility)
			{
			}
		}
	}

	private static void OnGotFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		if (sender is ActiveXHost activeXHost)
		{
			Invariant.Assert(activeXHost.ActiveXState >= ActiveXHelper.ActiveXState.InPlaceActive, "Should at least be InPlaceActive when getting focus");
			if (activeXHost.ActiveXState < ActiveXHelper.ActiveXState.UIActive)
			{
				activeXHost.TransitionUpTo(ActiveXHelper.ActiveXState.UIActive);
			}
		}
	}

	private static void OnLostFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		if (sender is ActiveXHost activeXHost)
		{
			Invariant.Assert(activeXHost.ActiveXState >= ActiveXHelper.ActiveXState.UIActive, "Should at least be UIActive when losing focus");
			if (!activeXHost.IsKeyboardFocusWithin)
			{
				activeXHost.TransitionDownTo(ActiveXHelper.ActiveXState.InPlaceActive);
			}
		}
	}

	private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs args)
	{
		if (!args.Handled && args.Scope == null && args.Target == null)
		{
			args.Target = (UIElement)sender;
		}
	}

	private void AttachInterfacesInternal()
	{
		_axOleObject = (MS.Win32.UnsafeNativeMethods.IOleObject)_axInstance;
		_axOleInPlaceObject = (MS.Win32.UnsafeNativeMethods.IOleInPlaceObject)_axInstance;
		_axOleInPlaceActiveObject = (MS.Win32.UnsafeNativeMethods.IOleInPlaceActiveObject)_axInstance;
		AttachInterfaces(_axInstance);
	}

	private void DetachInterfacesInternal()
	{
		_axOleObject = null;
		_axOleInPlaceObject = null;
		_axOleInPlaceActiveObject = null;
		DetachInterfaces();
	}

	private MS.Win32.NativeMethods.SIZE SetExtent(int width, int height)
	{
		MS.Win32.NativeMethods.SIZE sIZE = new MS.Win32.NativeMethods.SIZE();
		sIZE.cx = width;
		sIZE.cy = height;
		bool flag = false;
		try
		{
			_axOleObject.SetExtent(1, sIZE);
		}
		catch (COMException)
		{
			flag = true;
		}
		if (flag)
		{
			_axOleObject.GetExtent(1, sIZE);
			try
			{
				_axOleObject.SetExtent(1, sIZE);
			}
			catch (COMException)
			{
			}
		}
		return GetExtent();
	}

	private MS.Win32.NativeMethods.SIZE GetExtent()
	{
		MS.Win32.NativeMethods.SIZE sIZE = new MS.Win32.NativeMethods.SIZE();
		_axOleObject.GetExtent(1, sIZE);
		return sIZE;
	}
}
