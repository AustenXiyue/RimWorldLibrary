using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.AppModel;
using MS.Internal.Interop;
using MS.Internal.KnownBoxes;
using MS.Win32;

namespace System.Windows;

/// <summary>Provides the ability to create, configure, show, and manage the lifetime of windows and dialog boxes.</summary>
[Localizability(LocalizationCategory.Ignore)]
public class Window : ContentControl, IWindowService
{
	internal struct WindowMinMax
	{
		internal double minWidth;

		internal double maxWidth;

		internal double minHeight;

		internal double maxHeight;

		internal WindowMinMax(double minSize, double maxSize)
		{
			minWidth = minSize;
			maxWidth = maxSize;
			minHeight = minSize;
			maxHeight = maxSize;
		}
	}

	internal class SourceWindowHelper
	{
		private HwndSource _sourceWindow;

		private HwndPanningFeedback _panningFeedback;

		internal bool IsSourceWindowNull => _sourceWindow == null;

		internal bool IsCompositionTargetInvalid => CompositionTarget == null;

		internal nint CriticalHandle
		{
			get
			{
				if (_sourceWindow != null)
				{
					return _sourceWindow.CriticalHandle;
				}
				return IntPtr.Zero;
			}
		}

		internal MS.Win32.NativeMethods.RECT WorkAreaBoundsForNearestMonitor
		{
			get
			{
				MS.Win32.NativeMethods.MONITORINFOEX mONITORINFOEX = new MS.Win32.NativeMethods.MONITORINFOEX();
				mONITORINFOEX.cbSize = Marshal.SizeOf(typeof(MS.Win32.NativeMethods.MONITORINFOEX));
				nint num = SafeNativeMethods.MonitorFromWindow(new HandleRef(this, CriticalHandle), 2);
				if (num != IntPtr.Zero)
				{
					SafeNativeMethods.GetMonitorInfo(new HandleRef(this, num), mONITORINFOEX);
				}
				return mONITORINFOEX.rcWork;
			}
		}

		private MS.Win32.NativeMethods.RECT ClientBounds
		{
			get
			{
				MS.Win32.NativeMethods.RECT rect = new MS.Win32.NativeMethods.RECT(0, 0, 0, 0);
				SafeNativeMethods.GetClientRect(new HandleRef(this, CriticalHandle), ref rect);
				return rect;
			}
		}

		internal MS.Win32.NativeMethods.RECT WindowBounds
		{
			get
			{
				MS.Win32.NativeMethods.RECT rect = new MS.Win32.NativeMethods.RECT(0, 0, 0, 0);
				SafeNativeMethods.GetWindowRect(new HandleRef(this, CriticalHandle), ref rect);
				return rect;
			}
		}

		internal SizeToContent HwndSourceSizeToContent
		{
			get
			{
				return _sourceWindow.SizeToContent;
			}
			set
			{
				_sourceWindow.SizeToContent = value;
			}
		}

		internal Visual RootVisual
		{
			set
			{
				_sourceWindow.RootVisual = value;
			}
		}

		internal bool IsActiveWindow => _sourceWindow.CriticalHandle == MS.Win32.UnsafeNativeMethods.GetActiveWindow();

		internal HwndSource HwndSourceWindow => _sourceWindow;

		internal HwndTarget CompositionTarget
		{
			get
			{
				if (_sourceWindow != null)
				{
					HwndTarget compositionTarget = _sourceWindow.CompositionTarget;
					if (compositionTarget != null && !compositionTarget.IsDisposed)
					{
						return compositionTarget;
					}
				}
				return null;
			}
		}

		internal Size WindowSize
		{
			get
			{
				MS.Win32.NativeMethods.RECT windowBounds = WindowBounds;
				return new Size(windowBounds.right - windowBounds.left, windowBounds.bottom - windowBounds.top);
			}
		}

		internal int StyleExFromHwnd => MS.Win32.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, CriticalHandle), -20);

		internal int StyleFromHwnd => MS.Win32.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, CriticalHandle), -16);

		internal SourceWindowHelper(HwndSource sourceWindow)
		{
			_sourceWindow = sourceWindow;
		}

		private MS.Win32.NativeMethods.POINT GetWindowScreenLocation(FlowDirection flowDirection)
		{
			MS.Win32.NativeMethods.POINT pt = default(MS.Win32.NativeMethods.POINT);
			if (flowDirection == FlowDirection.RightToLeft)
			{
				MS.Win32.NativeMethods.RECT rect = new MS.Win32.NativeMethods.RECT(0, 0, 0, 0);
				SafeNativeMethods.GetClientRect(new HandleRef(this, CriticalHandle), ref rect);
				pt = new MS.Win32.NativeMethods.POINT(rect.right, rect.top);
			}
			MS.Win32.UnsafeNativeMethods.ClientToScreen(new HandleRef(this, _sourceWindow.CriticalHandle), ref pt);
			return pt;
		}

		internal MS.Win32.NativeMethods.POINT GetPointRelativeToWindow(int x, int y, FlowDirection flowDirection)
		{
			MS.Win32.NativeMethods.POINT windowScreenLocation = GetWindowScreenLocation(flowDirection);
			return new MS.Win32.NativeMethods.POINT(x - windowScreenLocation.x, y - windowScreenLocation.y);
		}

		internal Size GetSizeFromHwndInMeasureUnits()
		{
			Point point = new Point(0.0, 0.0);
			MS.Win32.NativeMethods.RECT windowBounds = WindowBounds;
			point.X = windowBounds.right - windowBounds.left;
			point.Y = windowBounds.bottom - windowBounds.top;
			point = _sourceWindow.CompositionTarget.TransformFromDevice.Transform(point);
			return new Size(point.X, point.Y);
		}

		internal Size GetHwndNonClientAreaSizeInMeasureUnits()
		{
			MS.Win32.NativeMethods.RECT clientBounds = ClientBounds;
			MS.Win32.NativeMethods.RECT windowBounds = WindowBounds;
			Point point = new Point(windowBounds.right - windowBounds.left - (clientBounds.right - clientBounds.left), windowBounds.bottom - windowBounds.top - (clientBounds.bottom - clientBounds.top));
			point = _sourceWindow.CompositionTarget.TransformFromDevice.Transform(point);
			return new Size(Math.Max(0.0, point.X), Math.Max(0.0, point.Y));
		}

		internal void ClearRootVisual()
		{
			if (_sourceWindow.RootVisual != null)
			{
				_sourceWindow.RootVisual = null;
			}
		}

		internal void AddDisposedHandler(EventHandler theHandler)
		{
			if (_sourceWindow != null)
			{
				_sourceWindow.Disposed += theHandler;
			}
		}

		internal void RemoveDisposedHandler(EventHandler theHandler)
		{
			if (_sourceWindow != null)
			{
				_sourceWindow.Disposed -= theHandler;
			}
		}

		internal void UpdatePanningFeedback(Vector totalOverpanOffset, bool animate)
		{
			if (_panningFeedback == null && _sourceWindow != null)
			{
				_panningFeedback = new HwndPanningFeedback(_sourceWindow);
			}
			if (_panningFeedback != null)
			{
				_panningFeedback.UpdatePanningFeedback(totalOverpanOffset, animate);
			}
		}

		internal void EndPanningFeedback(bool animateBack)
		{
			if (_panningFeedback != null)
			{
				_panningFeedback.EndPanningFeedback(animateBack);
				_panningFeedback = null;
			}
		}
	}

	internal class HwndStyleManager : IDisposable
	{
		private Window _window;

		private int _refCount;

		private bool _fDirty;

		internal bool Dirty
		{
			get
			{
				return _fDirty;
			}
			set
			{
				_fDirty = value;
			}
		}

		internal static HwndStyleManager StartManaging(Window w, int Style, int StyleEx)
		{
			if (w.Manager == null)
			{
				return new HwndStyleManager(w, Style, StyleEx);
			}
			w.Manager._refCount++;
			return w.Manager;
		}

		private HwndStyleManager(Window w, int Style, int StyleEx)
		{
			_window = w;
			_window.Manager = this;
			if (!w.IsSourceWindowNull)
			{
				_window._Style = Style;
				_window._StyleEx = StyleEx;
				Dirty = false;
			}
			_refCount = 1;
		}

		void IDisposable.Dispose()
		{
			_refCount--;
			if (_refCount == 0)
			{
				_window.Flush();
				if (_window.Manager == this)
				{
					_window.Manager = null;
				}
			}
		}
	}

	private class WindowStartupTopLeftPointHelper
	{
		internal Point LogicalTopLeft { get; }

		internal Point? ScreenTopLeft { get; private set; }

		private bool IsHelperNeeded
		{
			get
			{
				if (CoreAppContextSwitches.DoNotUsePresentationDpiCapabilityTier2OrGreater)
				{
					return false;
				}
				if (!HwndTarget.IsPerMonitorDpiScalingEnabled)
				{
					return false;
				}
				if (HwndTarget.IsProcessPerMonitorDpiAware.HasValue)
				{
					return HwndTarget.IsProcessPerMonitorDpiAware.Value;
				}
				return DpiUtil.GetProcessDpiAwareness(IntPtr.Zero) == MS.Win32.NativeMethods.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE;
			}
		}

		internal WindowStartupTopLeftPointHelper(Point topLeft)
		{
			LogicalTopLeft = topLeft;
			if (IsHelperNeeded)
			{
				IdentifyScreenTopLeft();
			}
		}

		private void IdentifyScreenTopLeft()
		{
			HandleRef hWnd = new HandleRef(null, IntPtr.Zero);
			nint dC = MS.Win32.UnsafeNativeMethods.GetDC(hWnd);
			MS.Win32.UnsafeNativeMethods.EnumDisplayMonitors(dC, IntPtr.Zero, MonitorEnumProc, IntPtr.Zero);
			MS.Win32.UnsafeNativeMethods.ReleaseDC(hWnd, new HandleRef(null, dC));
		}

		private bool MonitorEnumProc(nint hMonitor, nint hdcMonitor, ref MS.Win32.NativeMethods.RECT lprcMonitor, nint dwData)
		{
			bool result = true;
			if (MS.Win32.UnsafeNativeMethods.GetDpiForMonitor(new HandleRef(null, hMonitor), MS.Win32.NativeMethods.MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out var dpiX, out var dpiY) == 0)
			{
				double num = (double)dpiX * 1.0 / 96.0;
				double num2 = (double)dpiY * 1.0 / 96.0;
				Rect rect = default(Rect);
				rect.X = (double)lprcMonitor.left / num;
				rect.Y = (double)lprcMonitor.top / num2;
				rect.Width = (double)(lprcMonitor.right - lprcMonitor.left) / num;
				rect.Height = (double)(lprcMonitor.bottom - lprcMonitor.top) / num2;
				Rect rect2 = rect;
				if (rect2.Contains(LogicalTopLeft))
				{
					ScreenTopLeft = new Point
					{
						X = LogicalTopLeft.X * num,
						Y = LogicalTopLeft.Y * num2
					};
					result = false;
				}
			}
			return result;
		}
	}

	private enum TransformType
	{
		WorkAreaToScreenArea,
		ScreenAreaToWorkArea
	}

	private enum BoundsSpecified
	{
		Height,
		Width,
		Top,
		Left
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Window.TaskbarItemInfo" /> dependency property.</summary>
	/// <returns>The <see cref="P:System.Windows.Window.TaskbarItemInfo" /> dependency property.</returns>
	public static readonly DependencyProperty TaskbarItemInfoProperty;

	public static readonly RoutedEvent DpiChangedEvent;

	/// <summary>Identifies the <see cref="P:System.Windows.Window.AllowsTransparency" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Window.AllowsTransparency" /> dependency property.</returns>
	public static readonly DependencyProperty AllowsTransparencyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Window.Title" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Window.Title" /> dependency property.</returns>
	public static readonly DependencyProperty TitleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Window.Icon" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Window.Icon" /> dependency property.</returns>
	public static readonly DependencyProperty IconProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Window.SizeToContent" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Window.SizeToContent" /> dependency property.</returns>
	public static readonly DependencyProperty SizeToContentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Window.Top" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Window.Top" /> dependency property.</returns>
	public static readonly DependencyProperty TopProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Window.Left" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Window.Left" /> dependency property.</returns>
	public static readonly DependencyProperty LeftProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Window.ShowInTaskbar" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Window.ShowInTaskbar" /> dependency property.</returns>
	public static readonly DependencyProperty ShowInTaskbarProperty;

	private static readonly DependencyPropertyKey IsActivePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Window.IsActive" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Window.IsActive" /> dependency property.</returns>
	public static readonly DependencyProperty IsActiveProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Window.WindowStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Window.WindowStyle" /> dependency property.</returns>
	public static readonly DependencyProperty WindowStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Window.WindowState" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Window.WindowState" /> dependency property.</returns>
	public static readonly DependencyProperty WindowStateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Window.ResizeMode" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Window.ResizeMode" /> dependency property.</returns>
	public static readonly DependencyProperty ResizeModeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Window.Topmost" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Window.Topmost" /> dependency property.</returns>
	public static readonly DependencyProperty TopmostProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Window.ShowActivated" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Window.ShowActivated" /> dependency property.</returns>
	public static readonly DependencyProperty ShowActivatedProperty;

	internal static readonly RoutedCommand DialogCancelCommand;

	private SourceWindowHelper _swh;

	private Window _ownerWindow;

	private nint _ownerHandle = IntPtr.Zero;

	private WindowCollection _ownedWindows;

	private ArrayList _threadWindowHandles;

	private bool _updateHwndSize = true;

	private bool _updateHwndLocation = true;

	private bool _updateStartupLocation;

	private bool _isVisible;

	private bool _isVisibilitySet;

	private bool _resetKeyboardCuesProperty;

	private bool _previousKeyboardCuesProperty;

	private static bool _dialogCommandAdded;

	private bool _postContentRenderedFromLoadedHandler;

	private bool _disposed;

	private bool _appShuttingDown;

	private bool _ignoreCancel;

	private bool _showingAsDialog;

	private bool _isClosing;

	private bool _visibilitySetInternally;

	private bool _hwndCreatedButNotShown;

	private double _trackMinWidthDeviceUnits;

	private double _trackMinHeightDeviceUnits;

	private double _trackMaxWidthDeviceUnits = double.PositiveInfinity;

	private double _trackMaxHeightDeviceUnits = double.PositiveInfinity;

	private double _windowMaxWidthDeviceUnits = double.PositiveInfinity;

	private double _windowMaxHeightDeviceUnits = double.PositiveInfinity;

	private double _actualTop = double.NaN;

	private double _actualLeft = double.NaN;

	private bool _inTrustedSubWindow;

	private ImageSource _icon;

	private MS.Win32.NativeMethods.IconHandle _defaultLargeIconHandle;

	private MS.Win32.NativeMethods.IconHandle _defaultSmallIconHandle;

	private MS.Win32.NativeMethods.IconHandle _currentLargeIconHandle;

	private MS.Win32.NativeMethods.IconHandle _currentSmallIconHandle;

	private bool? _dialogResult;

	private nint _dialogOwnerHandle = IntPtr.Zero;

	private nint _dialogPreviousActiveHandle;

	private DispatcherFrame _dispatcherFrame;

	private WindowStartupLocation _windowStartupLocation;

	private WindowState _previousWindowState;

	private HwndWrapper _hiddenWindow;

	private EventHandlerList _events;

	private MS.Internal.SecurityCriticalDataForSet<int> _styleDoNotUse;

	private MS.Internal.SecurityCriticalDataForSet<int> _styleExDoNotUse;

	private HwndStyleManager _manager;

	private Control _resizeGripControl;

	private Point _prePanningLocation = new Point(double.NaN, double.NaN);

	private static readonly object EVENT_SOURCEINITIALIZED;

	private static readonly object EVENT_CLOSING;

	private static readonly object EVENT_CLOSED;

	private static readonly object EVENT_ACTIVATED;

	private static readonly object EVENT_DEACTIVATED;

	private static readonly object EVENT_STATECHANGED;

	private static readonly object EVENT_LOCATIONCHANGED;

	private static readonly object EVENT_CONTENTRENDERED;

	private static readonly object EVENT_VISUALCHILDRENCHANGED;

	private static readonly WindowMessage WM_TASKBARBUTTONCREATED;

	private static readonly WindowMessage WM_APPLYTASKBARITEMINFO;

	private const int c_MaximumThumbButtons = 7;

	private ITaskbarList3 _taskbarList;

	private DispatcherTimer _taskbarRetryTimer;

	private Size _overlaySize;

	internal static readonly DependencyProperty IWindowServiceProperty;

	private DispatcherOperation _contentRenderedCallback;

	private WeakReference _currentPanningTarget;

	private static DependencyObjectType _dType;

	/// <summary>Gets an enumerator for a window's logical child elements.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> a window's logical child elements.</returns>
	protected internal override IEnumerator LogicalChildren => new SingleChildEnumerator(base.Content);

	/// <summary>Gets or sets the Windows 7 taskbar thumbnail for the <see cref="T:System.Windows.Window" />.</summary>
	/// <returns>The Windows 7 taskbar thumbnail for the <see cref="T:System.Windows.Window" />.</returns>
	public TaskbarItemInfo TaskbarItemInfo
	{
		get
		{
			VerifyContextAndObjectState();
			return (TaskbarItemInfo)GetValue(TaskbarItemInfoProperty);
		}
		set
		{
			VerifyContextAndObjectState();
			SetValue(TaskbarItemInfoProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a window's client area supports transparency. </summary>
	/// <returns>true if the window supports transparency; otherwise, false.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Window.AllowsTransparency" /> is changed after a window has been shown.</exception>
	/// <exception cref="T:System.InvalidOperationException">A window that has a <see cref="P:System.Windows.Window.WindowStyle" /> value of anything other than <see cref="F:System.Windows.WindowStyle.None" />.</exception>
	public bool AllowsTransparency
	{
		get
		{
			return (bool)GetValue(AllowsTransparencyProperty);
		}
		set
		{
			SetValue(AllowsTransparencyProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a window's title. </summary>
	/// <returns>A <see cref="T:System.String" /> that contains the window's title.</returns>
	[Localizability(LocalizationCategory.Title)]
	public string Title
	{
		get
		{
			VerifyContextAndObjectState();
			return (string)GetValue(TitleProperty);
		}
		set
		{
			VerifyContextAndObjectState();
			SetValue(TitleProperty, value);
		}
	}

	/// <summary>Gets or sets a window's icon.  </summary>
	/// <returns>An <see cref="T:System.Windows.Media.ImageSource" /> object that represents the icon.</returns>
	public ImageSource Icon
	{
		get
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			return (ImageSource)GetValue(IconProperty);
		}
		set
		{
			VerifyApiSupported();
			VerifyContextAndObjectState();
			SetValue(IconProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a window will automatically size itself to fit the size of its content. </summary>
	/// <returns>A <see cref="T:System.Windows.SizeToContent" /> value. The default is <see cref="F:System.Windows.SizeToContent.Manual" />.</returns>
	public SizeToContent SizeToContent
	{
		get
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			return (SizeToContent)GetValue(SizeToContentProperty);
		}
		set
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			SetValue(SizeToContentProperty, value);
		}
	}

	/// <summary>Gets or sets the position of the window's top edge, in relation to the desktop. </summary>
	/// <returns>The position of the window's top, in logical units (1/96").</returns>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	public double Top
	{
		get
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			return (double)GetValue(TopProperty);
		}
		set
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			SetValue(TopProperty, value);
		}
	}

	/// <summary>Gets or sets the position of the window's left edge, in relation to the desktop. </summary>
	/// <returns>The position of the window's left edge, in logical units (1/96th of an inch).</returns>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	public double Left
	{
		get
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			return (double)GetValue(LeftProperty);
		}
		set
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			SetValue(LeftProperty, value);
		}
	}

	/// <summary>Gets the size and location of a window before being either minimized or maximized.</summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> that specifies the size and location of a window before being either minimized or maximized.</returns>
	public Rect RestoreBounds
	{
		get
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			if (IsSourceWindowNull || IsCompositionTargetInvalid)
			{
				return Rect.Empty;
			}
			return GetNormalRectLogicalUnits(CriticalHandle);
		}
	}

	/// <summary>Gets or sets the position of the window when first shown.</summary>
	/// <returns>A <see cref="T:System.Windows.WindowStartupLocation" /> value that specifies the top/left position of a window when first shown. The default is <see cref="F:System.Windows.WindowStartupLocation.Manual" />.</returns>
	[DefaultValue(WindowStartupLocation.Manual)]
	public WindowStartupLocation WindowStartupLocation
	{
		get
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			return _windowStartupLocation;
		}
		set
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			if (!IsValidWindowStartupLocation(value))
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(WindowStartupLocation));
			}
			_windowStartupLocation = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the window has a task bar button. </summary>
	/// <returns>true if the window has a task bar button; otherwise, false. Does not apply when the window is hosted in a browser.</returns>
	public bool ShowInTaskbar
	{
		get
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			return (bool)GetValue(ShowInTaskbarProperty);
		}
		set
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			SetValue(ShowInTaskbarProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets a value that indicates whether the window is active. </summary>
	/// <returns>true if the window is active; otherwise, false. The default is false.</returns>
	public bool IsActive
	{
		get
		{
			VerifyContextAndObjectState();
			return (bool)GetValue(IsActiveProperty);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Window" /> that owns this <see cref="T:System.Windows.Window" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Window" /> object that represents the owner of this <see cref="T:System.Windows.Window" />.</returns>
	/// <exception cref="T:System.ArgumentException">A window tries to own itself-or-Two windows try to own each other.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Window.Owner" /> property is set on a visible window shown using <see cref="M:System.Windows.Window.ShowDialog" />-or-The <see cref="P:System.Windows.Window.Owner" /> property is set with a window that has not been previously shown.</exception>
	[DefaultValue(null)]
	public Window Owner
	{
		get
		{
			VerifyApiSupported();
			VerifyContextAndObjectState();
			return _ownerWindow;
		}
		set
		{
			VerifyApiSupported();
			VerifyContextAndObjectState();
			if (value == this)
			{
				throw new ArgumentException(SR.CannotSetOwnerToItself);
			}
			if (_showingAsDialog)
			{
				throw new InvalidOperationException(SR.CantSetOwnerAfterDialogIsShown);
			}
			if (value != null && value.IsSourceWindowNull)
			{
				if (value._disposed)
				{
					throw new InvalidOperationException(SR.CantSetOwnerToClosedWindow);
				}
				throw new InvalidOperationException(SR.CantSetOwnerWhosHwndIsNotCreated);
			}
			if (_ownerWindow == value)
			{
				return;
			}
			if (!_disposed)
			{
				if (value != null)
				{
					WindowCollection ownedWindows = OwnedWindows;
					for (int i = 0; i < ownedWindows.Count; i++)
					{
						if (ownedWindows[i] == value)
						{
							throw new ArgumentException(SR.Format(SR.CircularOwnerChild, value, this));
						}
					}
				}
				if (_ownerWindow != null)
				{
					_ownerWindow.OwnedWindowsInternal.Remove(this);
				}
			}
			_ownerWindow = value;
			if (!_disposed)
			{
				SetOwnerHandle((_ownerWindow != null) ? _ownerWindow.CriticalHandle : IntPtr.Zero);
				if (_ownerWindow != null)
				{
					_ownerWindow.OwnedWindowsInternal.Add(this);
				}
			}
		}
	}

	private bool IsOwnerNull => _ownerWindow == null;

	/// <summary>Gets a collection of windows for which this window is the owner.</summary>
	/// <returns>A <see cref="T:System.Windows.WindowCollection" /> that contains references to the windows for which this window is the owner.</returns>
	public WindowCollection OwnedWindows
	{
		get
		{
			VerifyContextAndObjectState();
			return OwnedWindowsInternal.Clone();
		}
	}

	internal bool IsShowingAsDialog => _showingAsDialog;

	/// <summary>Gets or sets the dialog result value, which is the value that is returned from the <see cref="M:System.Windows.Window.ShowDialog" /> method.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> value of type <see cref="T:System.Boolean" />. The default is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Window.DialogResult" /> is set before a window is opened by calling <see cref="M:System.Windows.Window.ShowDialog" />. -or-<see cref="P:System.Windows.Window.DialogResult" /> is set on a window that is opened by calling <see cref="M:System.Windows.Window.Show" />.</exception>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[TypeConverter(typeof(DialogResultConverter))]
	public bool? DialogResult
	{
		get
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			return _dialogResult;
		}
		set
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			if (_showingAsDialog)
			{
				if (_dialogResult != value)
				{
					_dialogResult = value;
					if (!_isClosing)
					{
						Close();
					}
				}
				return;
			}
			throw new InvalidOperationException(SR.DialogResultMustBeSetAfterShowDialog);
		}
	}

	/// <summary>Gets or sets a window's border style. </summary>
	/// <returns>A <see cref="T:System.Windows.WindowStyle" /> that specifies a window's border style. The default is <see cref="F:System.Windows.WindowStyle.SingleBorderWindow" />.</returns>
	public WindowStyle WindowStyle
	{
		get
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			return (WindowStyle)GetValue(WindowStyleProperty);
		}
		set
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			SetValue(WindowStyleProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a window is restored, minimized, or maximized. </summary>
	/// <returns>A <see cref="T:System.Windows.WindowState" /> that determines whether a window is restored, minimized, or maximized. The default is <see cref="F:System.Windows.WindowState.Normal" /> (restored).</returns>
	public WindowState WindowState
	{
		get
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			return (WindowState)GetValue(WindowStateProperty);
		}
		set
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			SetValue(WindowStateProperty, value);
		}
	}

	/// <summary>Gets or sets the resize mode. </summary>
	/// <returns>A <see cref="T:System.Windows.ResizeMode" /> value specifying the resize mode.</returns>
	public ResizeMode ResizeMode
	{
		get
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			return (ResizeMode)GetValue(ResizeModeProperty);
		}
		set
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			SetValue(ResizeModeProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a window appears in the topmost z-order. </summary>
	/// <returns>true if the window is topmost; otherwise, false.</returns>
	public bool Topmost
	{
		get
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			return (bool)GetValue(TopmostProperty);
		}
		set
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			SetValue(TopmostProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a value that indicates whether a window is activated when first shown. </summary>
	/// <returns>true if a window is activated when first shown; otherwise, false. The default is true.</returns>
	public bool ShowActivated
	{
		get
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			return (bool)GetValue(ShowActivatedProperty);
		}
		set
		{
			VerifyContextAndObjectState();
			VerifyApiSupported();
			SetValue(ShowActivatedProperty, BooleanBoxes.Box(value));
		}
	}

	internal bool IsSourceWindowNull
	{
		get
		{
			if (_swh != null)
			{
				return _swh.IsSourceWindowNull;
			}
			return true;
		}
	}

	internal bool IsCompositionTargetInvalid
	{
		get
		{
			if (_swh != null)
			{
				return _swh.IsCompositionTargetInvalid;
			}
			return true;
		}
	}

	internal MS.Win32.NativeMethods.RECT WorkAreaBoundsForNearestMonitor => _swh.WorkAreaBoundsForNearestMonitor;

	internal Size WindowSize => _swh.WindowSize;

	internal HwndSource HwndSourceWindow
	{
		get
		{
			if (_swh != null)
			{
				return _swh.HwndSourceWindow;
			}
			return null;
		}
	}

	internal bool HwndCreatedButNotShown => _hwndCreatedButNotShown;

	internal bool IsDisposed => _disposed;

	internal bool IsVisibilitySet
	{
		get
		{
			VerifyContextAndObjectState();
			return _isVisibilitySet;
		}
	}

	internal nint CriticalHandle
	{
		get
		{
			VerifyContextAndObjectState();
			if (_swh != null)
			{
				return _swh.CriticalHandle;
			}
			return IntPtr.Zero;
		}
	}

	internal nint OwnerHandle
	{
		get
		{
			VerifyContextAndObjectState();
			return _ownerHandle;
		}
		set
		{
			VerifyContextAndObjectState();
			if (_showingAsDialog)
			{
				throw new InvalidOperationException(SR.CantSetOwnerAfterDialogIsShown);
			}
			SetOwnerHandle(value);
		}
	}

	internal int Win32Style
	{
		get
		{
			VerifyContextAndObjectState();
			return _Style;
		}
		set
		{
			VerifyContextAndObjectState();
			_Style = value;
		}
	}

	internal int _Style
	{
		get
		{
			if (Manager != null)
			{
				return _styleDoNotUse.Value;
			}
			if (IsSourceWindowNull)
			{
				return _styleDoNotUse.Value;
			}
			return _swh.StyleFromHwnd;
		}
		set
		{
			_styleDoNotUse = new MS.Internal.SecurityCriticalDataForSet<int>(value);
			Manager.Dirty = true;
		}
	}

	internal int _StyleEx
	{
		get
		{
			if (Manager != null)
			{
				return _styleExDoNotUse.Value;
			}
			if (IsSourceWindowNull)
			{
				return _styleExDoNotUse.Value;
			}
			return _swh.StyleExFromHwnd;
		}
		set
		{
			_styleExDoNotUse = new MS.Internal.SecurityCriticalDataForSet<int>(value);
			Manager.Dirty = true;
		}
	}

	internal HwndStyleManager Manager
	{
		get
		{
			return _manager;
		}
		set
		{
			_manager = value;
		}
	}

	bool IWindowService.UserResized => false;

	private bool CanCenterOverWPFOwner
	{
		get
		{
			if (Owner == null)
			{
				return false;
			}
			if (Owner.IsSourceWindowNull && (double.IsNaN(Owner.Width) || double.IsNaN(Owner.Height)))
			{
				return false;
			}
			if (double.IsNaN(Owner.Left) || double.IsNaN(Owner.Top))
			{
				return false;
			}
			return true;
		}
	}

	private SizeToContent HwndSourceSizeToContent
	{
		get
		{
			return _swh.HwndSourceSizeToContent;
		}
		set
		{
			_swh.HwndSourceSizeToContent = value;
		}
	}

	private MS.Win32.NativeMethods.RECT WindowBounds => _swh.WindowBounds;

	private int StyleFromHwnd
	{
		get
		{
			if (_swh == null)
			{
				return 0;
			}
			return _swh.StyleFromHwnd;
		}
	}

	private int StyleExFromHwnd
	{
		get
		{
			if (_swh == null)
			{
				return 0;
			}
			return _swh.StyleExFromHwnd;
		}
	}

	private WindowCollection OwnedWindowsInternal
	{
		get
		{
			if (_ownedWindows == null)
			{
				_ownedWindows = new WindowCollection();
			}
			return _ownedWindows;
		}
	}

	private Application App => Application.Current;

	private bool IsInsideApp => Application.Current != null;

	private EventHandlerList Events
	{
		get
		{
			if (_events == null)
			{
				_events = new EventHandlerList();
			}
			return _events;
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>This event is raised to support interoperation with Win32. See <see cref="T:System.Windows.Interop.HwndSource" />.</summary>
	public event EventHandler SourceInitialized
	{
		add
		{
			Events.AddHandler(EVENT_SOURCEINITIALIZED, value);
		}
		remove
		{
			Events.RemoveHandler(EVENT_SOURCEINITIALIZED, value);
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

	/// <summary>Occurs when a window becomes the foreground window.</summary>
	public event EventHandler Activated
	{
		add
		{
			Events.AddHandler(EVENT_ACTIVATED, value);
		}
		remove
		{
			Events.RemoveHandler(EVENT_ACTIVATED, value);
		}
	}

	/// <summary>Occurs when a window becomes a background window.</summary>
	public event EventHandler Deactivated
	{
		add
		{
			Events.AddHandler(EVENT_DEACTIVATED, value);
		}
		remove
		{
			Events.RemoveHandler(EVENT_DEACTIVATED, value);
		}
	}

	/// <summary>Occurs when the window's <see cref="P:System.Windows.Window.WindowState" /> property changes.</summary>
	public event EventHandler StateChanged
	{
		add
		{
			Events.AddHandler(EVENT_STATECHANGED, value);
		}
		remove
		{
			Events.RemoveHandler(EVENT_STATECHANGED, value);
		}
	}

	/// <summary>Occurs when the window's location changes.</summary>
	public event EventHandler LocationChanged
	{
		add
		{
			Events.AddHandler(EVENT_LOCATIONCHANGED, value);
		}
		remove
		{
			Events.RemoveHandler(EVENT_LOCATIONCHANGED, value);
		}
	}

	/// <summary>Occurs directly after <see cref="M:System.Windows.Window.Close" /> is called, and can be handled to cancel window closure.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.UIElement.Visibility" /> is set, or <see cref="M:System.Windows.Window.Show" />, <see cref="M:System.Windows.Window.ShowDialog" />, or <see cref="M:System.Windows.Window.Close" /> is called while a window is closing.</exception>
	public event CancelEventHandler Closing
	{
		add
		{
			Events.AddHandler(EVENT_CLOSING, value);
		}
		remove
		{
			Events.RemoveHandler(EVENT_CLOSING, value);
		}
	}

	/// <summary>Occurs when the window is about to close.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.UIElement.Visibility" /> is set, or <see cref="M:System.Windows.Window.Show" />, <see cref="M:System.Windows.Window.ShowDialog" />, or <see cref="M:System.Windows.Window.Hide" /> is called while a window is closing.</exception>
	public event EventHandler Closed
	{
		add
		{
			Events.AddHandler(EVENT_CLOSED, value);
		}
		remove
		{
			Events.RemoveHandler(EVENT_CLOSED, value);
		}
	}

	/// <summary>Occurs after a window's content has been rendered.</summary>
	public event EventHandler ContentRendered
	{
		add
		{
			Events.AddHandler(EVENT_CONTENTRENDERED, value);
		}
		remove
		{
			Events.RemoveHandler(EVENT_CONTENTRENDERED, value);
		}
	}

	internal event EventHandler<EventArgs> VisualChildrenChanged
	{
		add
		{
			Events.AddHandler(EVENT_VISUALCHILDRENCHANGED, value);
		}
		remove
		{
			Events.RemoveHandler(EVENT_VISUALCHILDRENCHANGED, value);
		}
	}

	static Window()
	{
		TaskbarItemInfoProperty = DependencyProperty.Register("TaskbarItemInfo", typeof(TaskbarItemInfo), typeof(Window), new PropertyMetadata(null, delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((Window)d).OnTaskbarItemInfoChanged(e);
		}, VerifyAccessCoercion));
		AllowsTransparencyProperty = DependencyProperty.Register("AllowsTransparency", typeof(bool), typeof(Window), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, OnAllowsTransparencyChanged, CoerceAllowsTransparency));
		TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(Window), new FrameworkPropertyMetadata(string.Empty, _OnTitleChanged), _ValidateText);
		IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(Window), new FrameworkPropertyMetadata(_OnIconChanged, VerifyAccessCoercion));
		SizeToContentProperty = DependencyProperty.Register("SizeToContent", typeof(SizeToContent), typeof(Window), new FrameworkPropertyMetadata(SizeToContent.Manual, _OnSizeToContentChanged), _ValidateSizeToContentCallback);
		TopProperty = Canvas.TopProperty.AddOwner(typeof(Window), new FrameworkPropertyMetadata(double.NaN, _OnTopChanged, CoerceTop));
		LeftProperty = Canvas.LeftProperty.AddOwner(typeof(Window), new FrameworkPropertyMetadata(double.NaN, _OnLeftChanged, CoerceLeft));
		ShowInTaskbarProperty = DependencyProperty.Register("ShowInTaskbar", typeof(bool), typeof(Window), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, _OnShowInTaskbarChanged, VerifyAccessCoercion));
		IsActivePropertyKey = DependencyProperty.RegisterReadOnly("IsActive", typeof(bool), typeof(Window), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsActiveProperty = IsActivePropertyKey.DependencyProperty;
		WindowStyleProperty = DependencyProperty.Register("WindowStyle", typeof(WindowStyle), typeof(Window), new FrameworkPropertyMetadata(WindowStyle.SingleBorderWindow, _OnWindowStyleChanged, CoerceWindowStyle), _ValidateWindowStyleCallback);
		WindowStateProperty = DependencyProperty.Register("WindowState", typeof(WindowState), typeof(Window), new FrameworkPropertyMetadata(WindowState.Normal, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, _OnWindowStateChanged, VerifyAccessCoercion), _ValidateWindowStateCallback);
		ResizeModeProperty = DependencyProperty.Register("ResizeMode", typeof(ResizeMode), typeof(Window), new FrameworkPropertyMetadata(ResizeMode.CanResize, FrameworkPropertyMetadataOptions.AffectsMeasure, _OnResizeModeChanged, VerifyAccessCoercion), _ValidateResizeModeCallback);
		TopmostProperty = DependencyProperty.Register("Topmost", typeof(bool), typeof(Window), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, _OnTopmostChanged, VerifyAccessCoercion));
		ShowActivatedProperty = DependencyProperty.Register("ShowActivated", typeof(bool), typeof(Window), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, null, VerifyAccessCoercion));
		DialogCancelCommand = new RoutedCommand("DialogCancel", typeof(Window));
		EVENT_SOURCEINITIALIZED = new object();
		EVENT_CLOSING = new object();
		EVENT_CLOSED = new object();
		EVENT_ACTIVATED = new object();
		EVENT_DEACTIVATED = new object();
		EVENT_STATECHANGED = new object();
		EVENT_LOCATIONCHANGED = new object();
		EVENT_CONTENTRENDERED = new object();
		EVENT_VISUALCHILDRENCHANGED = new object();
		IWindowServiceProperty = DependencyProperty.RegisterAttached("IWindowService", typeof(IWindowService), typeof(Window), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior));
		FrameworkElement.HeightProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(_OnHeightChanged));
		FrameworkElement.MinHeightProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(_OnMinHeightChanged));
		FrameworkElement.MaxHeightProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(_OnMaxHeightChanged));
		FrameworkElement.WidthProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(_OnWidthChanged));
		FrameworkElement.MinWidthProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(_OnMinWidthChanged));
		FrameworkElement.MaxWidthProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(_OnMaxWidthChanged));
		UIElement.VisibilityProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(Visibility.Collapsed, _OnVisibilityChanged, CoerceVisibility));
		Control.IsTabStopProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
		KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
		FocusManager.IsFocusScopeProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(typeof(Window)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(Window));
		FrameworkElement.FlowDirectionProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(_OnFlowDirectionChanged));
		UIElement.RenderTransformProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(Transform.Identity, _OnRenderTransformChanged, CoerceRenderTransform));
		UIElement.ClipToBoundsProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, _OnClipToBoundsChanged, CoerceClipToBounds));
		WM_TASKBARBUTTONCREATED = MS.Win32.UnsafeNativeMethods.RegisterWindowMessage("TaskbarButtonCreated");
		WM_APPLYTASKBARITEMINFO = MS.Win32.UnsafeNativeMethods.RegisterWindowMessage("WPF_ApplyTaskbarItemInfo");
		EventManager.RegisterClassHandler(typeof(Window), UIElement.ManipulationCompletedEvent, new EventHandler<ManipulationCompletedEventArgs>(OnStaticManipulationCompleted), handledEventsToo: true);
		EventManager.RegisterClassHandler(typeof(Window), UIElement.ManipulationInertiaStartingEvent, new EventHandler<ManipulationInertiaStartingEventArgs>(OnStaticManipulationInertiaStarting), handledEventsToo: true);
		DpiChangedEvent = EventManager.RegisterRoutedEvent("DpiChanged", RoutingStrategy.Bubble, typeof(DpiChangedEventHandler), typeof(Window));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Window" /> class. </summary>
	public Window()
	{
		_inTrustedSubWindow = false;
		Initialize();
	}

	internal Window(bool inRbw)
	{
		if (inRbw)
		{
			_inTrustedSubWindow = true;
		}
		else
		{
			_inTrustedSubWindow = false;
		}
		Initialize();
	}

	/// <summary>Opens a window and returns without waiting for the newly opened window to close.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Windows.Window.Show" /> is called on a window that is closing (<see cref="E:System.Windows.Window.Closing" />) or has been closed (<see cref="E:System.Windows.Window.Closed" />).</exception>
	public void Show()
	{
		VerifyContextAndObjectState();
		VerifyCanShow();
		VerifyNotClosing();
		VerifyConsistencyWithAllowsTransparency();
		UpdateVisibilityProperty(Visibility.Visible);
		ShowHelper(BooleanBoxes.TrueBox);
	}

	/// <summary>Makes a window invisible.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Windows.Window.Hide" /> is called on a window that is closing (<see cref="E:System.Windows.Window.Closing" />) or has been closed (<see cref="E:System.Windows.Window.Closed" />).</exception>
	public void Hide()
	{
		VerifyContextAndObjectState();
		if (!_disposed)
		{
			UpdateVisibilityProperty(Visibility.Hidden);
			ShowHelper(BooleanBoxes.FalseBox);
		}
	}

	/// <summary>Manually closes a <see cref="T:System.Windows.Window" />.</summary>
	public void Close()
	{
		VerifyApiSupported();
		VerifyContextAndObjectState();
		InternalClose(shutdown: false, ignoreCancel: false);
	}

	/// <summary>Allows a window to be dragged by a mouse with its left button down over an exposed area of the window's client area.</summary>
	/// <exception cref="T:System.InvalidOperationException">The left mouse button is not down.</exception>
	public void DragMove()
	{
		VerifyApiSupported();
		VerifyContextAndObjectState();
		VerifyHwndCreateShowState();
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			if (Mouse.LeftButton != MouseButtonState.Pressed)
			{
				throw new InvalidOperationException(SR.DragMoveFail);
			}
			if (WindowState == WindowState.Normal)
			{
				MS.Win32.UnsafeNativeMethods.SendMessage(CriticalHandle, WindowMessage.WM_SYSCOMMAND, 61458, IntPtr.Zero);
				MS.Win32.UnsafeNativeMethods.SendMessage(CriticalHandle, WindowMessage.WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);
			}
		}
	}

	/// <summary>Opens a window and returns only when the newly opened window is closed.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> value of type <see cref="T:System.Boolean" /> that specifies whether the activity was accepted (true) or canceled (false). The return value is the value of the <see cref="P:System.Windows.Window.DialogResult" /> property before a window closes.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Windows.Window.ShowDialog" /> is called on a <see cref="T:System.Windows.Window" /> that is visible-or-<see cref="M:System.Windows.Window.ShowDialog" /> is called on a visible <see cref="T:System.Windows.Window" /> that was opened by calling <see cref="M:System.Windows.Window.ShowDialog" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Windows.Window.ShowDialog" /> is called on a window that is closing (<see cref="E:System.Windows.Window.Closing" />) or has been closed (<see cref="E:System.Windows.Window.Closed" />).</exception>
	public bool? ShowDialog()
	{
		VerifyApiSupported();
		VerifyContextAndObjectState();
		VerifyCanShow();
		VerifyNotClosing();
		VerifyConsistencyWithAllowsTransparency();
		if (_isVisible)
		{
			throw new InvalidOperationException(SR.ShowDialogOnVisible);
		}
		if (_showingAsDialog)
		{
			throw new InvalidOperationException(SR.ShowDialogOnModal);
		}
		_dialogOwnerHandle = _ownerHandle;
		if (!MS.Win32.UnsafeNativeMethods.IsWindow(new HandleRef(null, _dialogOwnerHandle)))
		{
			_dialogOwnerHandle = IntPtr.Zero;
		}
		_dialogPreviousActiveHandle = MS.Win32.UnsafeNativeMethods.GetActiveWindow();
		if (_dialogOwnerHandle == IntPtr.Zero)
		{
			_dialogOwnerHandle = _dialogPreviousActiveHandle;
		}
		if (_dialogOwnerHandle != IntPtr.Zero && _dialogOwnerHandle == MS.Win32.UnsafeNativeMethods.GetDesktopWindow())
		{
			_dialogOwnerHandle = IntPtr.Zero;
		}
		if (_dialogOwnerHandle != IntPtr.Zero)
		{
			while (_dialogOwnerHandle != IntPtr.Zero && (MS.Win32.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, _dialogOwnerHandle), -16) & 0x40000000) == 1073741824)
			{
				_dialogOwnerHandle = MS.Win32.UnsafeNativeMethods.GetParent(new HandleRef(null, _dialogOwnerHandle));
			}
		}
		_threadWindowHandles = new ArrayList();
		MS.Win32.UnsafeNativeMethods.EnumThreadWindows(SafeNativeMethods.GetCurrentThreadId(), ThreadWindowsCallback, MS.Win32.NativeMethods.NullHandleRef);
		EnableThreadWindows(state: false);
		if (SafeNativeMethods.GetCapture() != IntPtr.Zero)
		{
			SafeNativeMethods.ReleaseCapture();
		}
		EnsureDialogCommand();
		try
		{
			_showingAsDialog = true;
			Show();
		}
		catch
		{
			if (_threadWindowHandles != null)
			{
				EnableThreadWindows(state: true);
			}
			if (_dialogPreviousActiveHandle != IntPtr.Zero && MS.Win32.UnsafeNativeMethods.IsWindow(new HandleRef(null, _dialogPreviousActiveHandle)))
			{
				MS.Win32.UnsafeNativeMethods.TrySetFocus(new HandleRef(null, _dialogPreviousActiveHandle), ref _dialogPreviousActiveHandle);
			}
			ClearShowKeyboardCueState();
			_showingAsDialog = false;
			throw;
		}
		finally
		{
			_showingAsDialog = false;
		}
		return _dialogResult;
	}

	/// <summary>Attempts to bring the window to the foreground and activates it.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Window" /> was successfully activated; otherwise, false.</returns>
	public bool Activate()
	{
		VerifyApiSupported();
		VerifyContextAndObjectState();
		VerifyHwndCreateShowState();
		if (IsSourceWindowNull || IsCompositionTargetInvalid)
		{
			return false;
		}
		return MS.Win32.UnsafeNativeMethods.SetForegroundWindow(new HandleRef(null, CriticalHandle));
	}

	/// <summary>Returns a reference to the <see cref="T:System.Windows.Window" /> object that hosts the content tree within which the dependency object is located.</summary>
	/// <returns>A <see cref="T:System.Windows.Window" /> reference to the host window.</returns>
	/// <param name="dependencyObject">The dependency object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dependencyObject" /> is null.</exception>
	public static Window GetWindow(DependencyObject dependencyObject)
	{
		if (dependencyObject == null)
		{
			throw new ArgumentNullException("dependencyObject");
		}
		return dependencyObject.GetValue(IWindowServiceProperty) as Window;
	}

	private void OnTaskbarItemInfoChanged(DependencyPropertyChangedEventArgs e)
	{
		TaskbarItemInfo taskbarItemInfo = (TaskbarItemInfo)e.OldValue;
		TaskbarItemInfo taskbarItemInfo2 = (TaskbarItemInfo)e.NewValue;
		if (Utilities.IsOSWindows7OrNewer && !e.IsASubPropertyChange)
		{
			if (taskbarItemInfo != null)
			{
				taskbarItemInfo.PropertyChanged -= OnTaskbarItemInfoSubPropertyChanged;
			}
			if (taskbarItemInfo2 != null)
			{
				taskbarItemInfo2.PropertyChanged += OnTaskbarItemInfoSubPropertyChanged;
			}
			ApplyTaskbarItemInfo();
		}
	}

	private void HandleTaskbarListError(MS.Internal.Interop.HRESULT hr)
	{
		if (!hr.Failed)
		{
			return;
		}
		if (hr == (MS.Internal.Interop.HRESULT)Win32Error.ERROR_TIMEOUT)
		{
			if (TraceShell.IsEnabled)
			{
				TraceShell.Trace(TraceEventType.Error, TraceShell.ExplorerTaskbarTimeout);
				TraceShell.Trace(TraceEventType.Warning, TraceShell.ExplorerTaskbarRetrying);
			}
			_taskbarRetryTimer.Start();
		}
		else if (hr == (MS.Internal.Interop.HRESULT)Win32Error.ERROR_INVALID_WINDOW_HANDLE || hr == MS.Internal.Interop.HRESULT.E_NOTIMPL)
		{
			if (TraceShell.IsEnabled)
			{
				TraceShell.Trace(TraceEventType.Warning, TraceShell.ExplorerTaskbarNotRunning);
			}
			Utilities.SafeRelease(ref _taskbarList);
		}
		else if (TraceShell.IsEnabled)
		{
			TraceShell.Trace(TraceEventType.Error, TraceShell.NativeTaskbarError(hr.ToString()));
		}
	}

	private void OnTaskbarItemInfoSubPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender == TaskbarItemInfo && _taskbarList != null && (_taskbarRetryTimer == null || !_taskbarRetryTimer.IsEnabled))
		{
			DependencyProperty property = e.Property;
			MS.Internal.Interop.HRESULT hr = MS.Internal.Interop.HRESULT.S_OK;
			if (property == TaskbarItemInfo.ProgressStateProperty)
			{
				hr = UpdateTaskbarProgressState();
			}
			else if (property == TaskbarItemInfo.ProgressValueProperty)
			{
				hr = UpdateTaskbarProgressValue();
			}
			else if (property == TaskbarItemInfo.OverlayProperty)
			{
				hr = UpdateTaskbarOverlay();
			}
			else if (property == TaskbarItemInfo.DescriptionProperty)
			{
				hr = UpdateTaskbarDescription();
			}
			else if (property == TaskbarItemInfo.ThumbnailClipMarginProperty)
			{
				hr = UpdateTaskbarThumbnailClipping();
			}
			else if (property == TaskbarItemInfo.ThumbButtonInfosProperty)
			{
				hr = UpdateTaskbarThumbButtons();
			}
			HandleTaskbarListError(hr);
		}
	}

	private static void OnAllowsTransparencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
	}

	private static object CoerceAllowsTransparency(DependencyObject d, object value)
	{
		value = VerifyAccessCoercion(d, value);
		if (!((Window)d).IsSourceWindowNull)
		{
			throw new InvalidOperationException(SR.ChangeNotAllowedAfterShow);
		}
		return value;
	}

	private static object CoerceWindowStyle(DependencyObject d, object value)
	{
		value = VerifyAccessCoercion(d, value);
		if (!((Window)d).IsSourceWindowNull)
		{
			((Window)d).VerifyConsistencyWithAllowsTransparency((WindowStyle)value);
		}
		return value;
	}

	/// <summary>Creates and returns a <see cref="T:System.Windows.Automation.Peers.WindowAutomationPeer" /> object for this <see cref="T:System.Windows.Window" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.WindowAutomationPeer" /> object for this <see cref="T:System.Windows.Window" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new WindowAutomationPeer(this);
	}

	protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
	{
		RaiseEvent(new DpiChangedEventArgs(oldDpi, newDpi, DpiChangedEvent, this));
	}

	/// <summary>Called when the parent of the window is changed. </summary>
	/// <param name="oldParent">The previous parent. Set to null if the <see cref="T:System.Windows.DependencyObject" /> did not have a previous parent.</param>
	protected internal sealed override void OnVisualParentChanged(DependencyObject oldParent)
	{
		VerifyContextAndObjectState();
		base.OnVisualParentChanged(oldParent);
		if (VisualTreeHelper.GetParent(this) != null)
		{
			throw new InvalidOperationException(SR.WindowMustBeRoot);
		}
	}

	/// <summary>Override this method to measure the size of a window.</summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> that reflects the size that this window determines it needs during layout, based on its calculations of children's sizes.</returns>
	/// <param name="availableSize">A <see cref="T:System.Windows.Size" /> that reflects the available size that this window can give to the child. Infinity can be given as a value to indicate that the window will size to whatever content is available.</param>
	protected override Size MeasureOverride(Size availableSize)
	{
		VerifyContextAndObjectState();
		Size constraint = new Size(availableSize.Width, availableSize.Height);
		WindowMinMax windowMinMax = GetWindowMinMax();
		constraint.Width = Math.Max(windowMinMax.minWidth, Math.Min(constraint.Width, windowMinMax.maxWidth));
		constraint.Height = Math.Max(windowMinMax.minHeight, Math.Min(constraint.Height, windowMinMax.maxHeight));
		Size size = MeasureOverrideHelper(constraint);
		return new Size(Math.Max(size.Width, windowMinMax.minWidth), Math.Max(size.Height, windowMinMax.minHeight));
	}

	/// <summary>Override this method to arrange and size a window and its child elements. </summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> that reflects the actual size that was used.</returns>
	/// <param name="arrangeBounds">A <see cref="T:System.Windows.Size" /> that reflects the final size that the window should use to arrange itself and its children.</param>
	protected override Size ArrangeOverride(Size arrangeBounds)
	{
		VerifyContextAndObjectState();
		WindowMinMax windowMinMax = GetWindowMinMax();
		arrangeBounds.Width = Math.Max(windowMinMax.minWidth, Math.Min(arrangeBounds.Width, windowMinMax.maxWidth));
		arrangeBounds.Height = Math.Max(windowMinMax.minHeight, Math.Min(arrangeBounds.Height, windowMinMax.maxHeight));
		if (IsSourceWindowNull || IsCompositionTargetInvalid)
		{
			return arrangeBounds;
		}
		if (VisualChildrenCount > 0 && GetVisualChild(0) is UIElement uIElement)
		{
			Size hwndNonClientAreaSizeInMeasureUnits = GetHwndNonClientAreaSizeInMeasureUnits();
			Size size = default(Size);
			size.Width = Math.Max(0.0, arrangeBounds.Width - hwndNonClientAreaSizeInMeasureUnits.Width);
			size.Height = Math.Max(0.0, arrangeBounds.Height - hwndNonClientAreaSizeInMeasureUnits.Height);
			uIElement.Arrange(new Rect(size));
			if (base.FlowDirection == FlowDirection.RightToLeft)
			{
				FrameworkElement.InternalSetLayoutTransform(uIElement, new MatrixTransform(-1.0, 0.0, 0.0, 1.0, size.Width, 0.0));
			}
		}
		return arrangeBounds;
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property changes.</summary>
	/// <param name="oldContent">A reference to the root of the old content tree.</param>
	/// <param name="newContent">A reference to the root of the new content tree.</param>
	protected override void OnContentChanged(object oldContent, object newContent)
	{
		base.OnContentChanged(oldContent, newContent);
		SetIWindowService();
		if (base.IsLoaded)
		{
			PostContentRendered();
		}
		else if (!_postContentRenderedFromLoadedHandler)
		{
			base.Loaded += LoadedHandler;
			_postContentRenderedFromLoadedHandler = true;
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Window.SourceInitialized" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	protected virtual void OnSourceInitialized(EventArgs e)
	{
		VerifyContextAndObjectState();
		((EventHandler)Events[EVENT_SOURCEINITIALIZED])?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Window.Activated" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	protected virtual void OnActivated(EventArgs e)
	{
		VerifyContextAndObjectState();
		((EventHandler)Events[EVENT_ACTIVATED])?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Window.Deactivated" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	protected virtual void OnDeactivated(EventArgs e)
	{
		VerifyContextAndObjectState();
		((EventHandler)Events[EVENT_DEACTIVATED])?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Window.StateChanged" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	protected virtual void OnStateChanged(EventArgs e)
	{
		VerifyContextAndObjectState();
		((EventHandler)Events[EVENT_STATECHANGED])?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Window.LocationChanged" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	protected virtual void OnLocationChanged(EventArgs e)
	{
		VerifyContextAndObjectState();
		((EventHandler)Events[EVENT_LOCATIONCHANGED])?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Window.Closing" /> event.</summary>
	/// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
	protected virtual void OnClosing(CancelEventArgs e)
	{
		VerifyContextAndObjectState();
		((CancelEventHandler)Events[EVENT_CLOSING])?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Window.Closed" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	protected virtual void OnClosed(EventArgs e)
	{
		VerifyContextAndObjectState();
		((EventHandler)Events[EVENT_CLOSED])?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Window.ContentRendered" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	protected virtual void OnContentRendered(EventArgs e)
	{
		VerifyContextAndObjectState();
		if (base.Content is DependencyObject element)
		{
			FocusManager.GetFocusedElement(element)?.Focus();
		}
		((EventHandler)Events[EVENT_CONTENTRENDERED])?.Invoke(this, e);
	}

	protected internal override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
	{
		VerifyContextAndObjectState();
		(Events[EVENT_VISUALCHILDRENCHANGED] as EventHandler<EventArgs>)?.Invoke(this, new EventArgs());
	}

	internal Point DeviceToLogicalUnits(Point ptDeviceUnits)
	{
		Invariant.Assert(!IsCompositionTargetInvalid, "IsCompositionTargetInvalid is supposed to be false here");
		return _swh.CompositionTarget.TransformFromDevice.Transform(ptDeviceUnits);
	}

	internal Point LogicalToDeviceUnits(Point ptLogicalUnits)
	{
		Invariant.Assert(!IsCompositionTargetInvalid, "IsCompositionTargetInvalid is supposed to be false here");
		return _swh.CompositionTarget.TransformToDevice.Transform(ptLogicalUnits);
	}

	internal static bool VisibilityToBool(Visibility v)
	{
		switch (v)
		{
		case Visibility.Visible:
			return true;
		case Visibility.Hidden:
		case Visibility.Collapsed:
			return false;
		default:
			return false;
		}
	}

	internal virtual void SetResizeGripControl(Control ctrl)
	{
		_resizeGripControl = ctrl;
	}

	internal virtual void ClearResizeGripControl(Control oldCtrl)
	{
		if (oldCtrl == _resizeGripControl)
		{
			_resizeGripControl = null;
		}
	}

	internal virtual void TryClearingMainWindow()
	{
		if (IsInsideApp && this == App.MainWindow)
		{
			App.MainWindow = null;
		}
	}

	internal void InternalClose(bool shutdown, bool ignoreCancel)
	{
		VerifyNotClosing();
		if (_disposed)
		{
			return;
		}
		_appShuttingDown = shutdown;
		_ignoreCancel = ignoreCancel;
		if (IsSourceWindowNull)
		{
			_isClosing = true;
			CancelEventArgs cancelEventArgs = new CancelEventArgs(cancel: false);
			try
			{
				OnClosing(cancelEventArgs);
			}
			catch
			{
				CloseWindowBeforeShow();
				throw;
			}
			if (ShouldCloseWindow(cancelEventArgs.Cancel))
			{
				CloseWindowBeforeShow();
			}
			else
			{
				_isClosing = false;
			}
		}
		else
		{
			MS.Win32.UnsafeNativeMethods.UnsafeSendMessage(CriticalHandle, WindowMessage.WM_CLOSE, 0, 0);
		}
	}

	private void CloseWindowBeforeShow()
	{
		InternalDispose();
		OnClosed(EventArgs.Empty);
	}

	private void InternalDispose()
	{
		_disposed = true;
		UpdateWindowListsOnClose();
		if (_taskbarRetryTimer != null)
		{
			_taskbarRetryTimer.Stop();
			_taskbarRetryTimer = null;
		}
		try
		{
			ClearSourceWindow();
			Utilities.SafeDispose(ref _hiddenWindow);
			Utilities.SafeDispose(ref _defaultLargeIconHandle);
			Utilities.SafeDispose(ref _defaultSmallIconHandle);
			Utilities.SafeDispose(ref _currentLargeIconHandle);
			Utilities.SafeDispose(ref _currentSmallIconHandle);
			Utilities.SafeRelease(ref _taskbarList);
		}
		finally
		{
			_isClosing = false;
		}
	}

	internal override void OnAncestorChanged()
	{
		base.OnAncestorChanged();
		if (base.Parent != null)
		{
			throw new InvalidOperationException(SR.WindowMustBeRoot);
		}
	}

	internal virtual void CreateAllStyle()
	{
		_Style = 34078720;
		_StyleEx = 0;
		CreateWindowStyle();
		CreateWindowState();
		if (_isVisible)
		{
			_Style |= 268435456;
		}
		SetTaskbarStatus();
		CreateTopmost();
		CreateResizibility();
		CreateRtl();
	}

	internal virtual void CreateSourceWindowDuringShow()
	{
		CreateSourceWindow(duringShow: true);
	}

	internal void CreateSourceWindow(bool duringShow)
	{
		VerifyContextAndObjectState();
		VerifyCanShow();
		VerifyNotClosing();
		VerifyConsistencyWithAllowsTransparency();
		if (!duringShow)
		{
			VerifyApiSupported();
		}
		double requestedTop = 0.0;
		double requestedLeft = 0.0;
		double requestedWidth = 0.0;
		double requestedHeight = 0.0;
		GetRequestedDimensions(ref requestedLeft, ref requestedTop, ref requestedWidth, ref requestedHeight);
		WindowStartupTopLeftPointHelper windowStartupTopLeftPointHelper = new WindowStartupTopLeftPointHelper(new Point(requestedLeft, requestedTop));
		using (HwndStyleManager hwndStyleManager = HwndStyleManager.StartManaging(this, StyleFromHwnd, StyleExFromHwnd))
		{
			CreateAllStyle();
			HwndSourceParameters parameters = CreateHwndSourceParameters();
			if (windowStartupTopLeftPointHelper.ScreenTopLeft.HasValue)
			{
				Point value = windowStartupTopLeftPointHelper.ScreenTopLeft.Value;
				parameters.SetPosition((int)value.X, (int)value.Y);
			}
			HwndSource hwndSource = new HwndSource(parameters);
			_swh = new SourceWindowHelper(hwndSource);
			hwndSource.SizeToContentChanged += OnSourceSizeToContentChanged;
			hwndStyleManager.Dirty = false;
			CorrectStyleForBorderlessWindowCase();
		}
		_swh.AddDisposedHandler(OnSourceWindowDisposed);
		_hwndCreatedButNotShown = !duringShow;
		if (Utilities.IsOSWindows7OrNewer)
		{
			MS.Win32.UnsafeNativeMethods.ChangeWindowMessageFilterEx(_swh.CriticalHandle, WM_TASKBARBUTTONCREATED, MSGFLT.ALLOW, out var extStatus);
			MS.Win32.UnsafeNativeMethods.ChangeWindowMessageFilterEx(_swh.CriticalHandle, WindowMessage.WM_COMMAND, MSGFLT.ALLOW, out extStatus);
		}
		SetupInitialState(requestedTop, requestedLeft, requestedWidth, requestedHeight);
		OnSourceInitialized(EventArgs.Empty);
	}

	internal virtual HwndSourceParameters CreateHwndSourceParameters()
	{
		HwndSourceParameters result = new HwndSourceParameters(Title, int.MinValue, int.MinValue);
		result.UsesPerPixelOpacity = AllowsTransparency;
		result.WindowStyle = _Style;
		result.ExtendedWindowStyle = _StyleEx;
		result.ParentWindow = _ownerHandle;
		result.AdjustSizingForNonClientArea = true;
		result.HwndSourceHook = WindowFilterMessage;
		return result;
	}

	private void OnSourceSizeToContentChanged(object sender, EventArgs args)
	{
		SizeToContent = HwndSourceSizeToContent;
	}

	internal virtual void CorrectStyleForBorderlessWindowCase()
	{
		using (HwndStyleManager.StartManaging(this, StyleFromHwnd, StyleExFromHwnd))
		{
			if (WindowStyle == WindowStyle.None)
			{
				_Style = _swh.StyleFromHwnd;
				_Style &= -12582913;
			}
		}
	}

	internal virtual void GetRequestedDimensions(ref double requestedLeft, ref double requestedTop, ref double requestedWidth, ref double requestedHeight)
	{
		requestedTop = Top;
		requestedLeft = Left;
		requestedWidth = base.Width;
		requestedHeight = base.Height;
	}

	internal virtual void SetupInitialState(double requestedTop, double requestedLeft, double requestedWidth, double requestedHeight)
	{
		HwndSourceSizeToContent = (SizeToContent)GetValue(SizeToContentProperty);
		UpdateIcon();
		MS.Win32.NativeMethods.RECT windowBounds = WindowBounds;
		Size currentSizeDeviceUnits = new Size(windowBounds.right - windowBounds.left, windowBounds.bottom - windowBounds.top);
		double leftDeviceUnits = windowBounds.left;
		double topDeviceUnits = windowBounds.top;
		bool flag = false;
		Point point = DeviceToLogicalUnits(new Point(leftDeviceUnits, topDeviceUnits));
		_actualLeft = point.X;
		_actualTop = point.Y;
		try
		{
			_updateHwndLocation = false;
			CoerceValue(TopProperty);
			CoerceValue(LeftProperty);
		}
		finally
		{
			_updateHwndLocation = true;
		}
		Point point2 = LogicalToDeviceUnits(new Point(requestedWidth, requestedHeight));
		Point point3 = LogicalToDeviceUnits(new Point(requestedLeft, requestedTop));
		if (!double.IsNaN(requestedWidth) && !DoubleUtil.AreClose(currentSizeDeviceUnits.Width, point2.X))
		{
			flag = true;
			currentSizeDeviceUnits.Width = point2.X;
			if (WindowState != 0)
			{
				UpdateHwndRestoreBounds(requestedWidth, BoundsSpecified.Width);
			}
		}
		if (!double.IsNaN(requestedHeight) && !DoubleUtil.AreClose(currentSizeDeviceUnits.Height, point2.Y))
		{
			flag = true;
			currentSizeDeviceUnits.Height = point2.Y;
			if (WindowState != 0)
			{
				UpdateHwndRestoreBounds(requestedHeight, BoundsSpecified.Height);
			}
		}
		if (!double.IsNaN(requestedLeft) && !DoubleUtil.AreClose(leftDeviceUnits, point3.X))
		{
			flag = true;
			leftDeviceUnits = point3.X;
			if (WindowState != 0)
			{
				UpdateHwndRestoreBounds(requestedLeft, BoundsSpecified.Left);
			}
		}
		if (!double.IsNaN(requestedTop) && !DoubleUtil.AreClose(topDeviceUnits, point3.Y))
		{
			flag = true;
			topDeviceUnits = point3.Y;
			if (WindowState != 0)
			{
				UpdateHwndRestoreBounds(requestedTop, BoundsSpecified.Top);
			}
		}
		Point point4 = LogicalToDeviceUnits(new Point(base.MinWidth, base.MinHeight));
		Point point5 = LogicalToDeviceUnits(new Point(base.MaxWidth, base.MaxHeight));
		if (!double.IsPositiveInfinity(point5.X) && currentSizeDeviceUnits.Width > point5.X)
		{
			flag = true;
			currentSizeDeviceUnits.Width = point5.X;
		}
		if (!double.IsPositiveInfinity(point4.Y) && currentSizeDeviceUnits.Height > point5.Y)
		{
			flag = true;
			currentSizeDeviceUnits.Height = point5.Y;
		}
		if (currentSizeDeviceUnits.Width < point4.X)
		{
			flag = true;
			currentSizeDeviceUnits.Width = point4.X;
		}
		if (currentSizeDeviceUnits.Height < point4.Y)
		{
			flag = true;
			currentSizeDeviceUnits.Height = point4.Y;
		}
		if ((CalculateWindowLocation(ref leftDeviceUnits, ref topDeviceUnits, currentSizeDeviceUnits) || flag) && WindowState == WindowState.Normal)
		{
			MS.Win32.UnsafeNativeMethods.SetWindowPos(new HandleRef(this, CriticalHandle), new HandleRef(null, IntPtr.Zero), DoubleUtil.DoubleToInt(leftDeviceUnits), DoubleUtil.DoubleToInt(topDeviceUnits), DoubleUtil.DoubleToInt(currentSizeDeviceUnits.Width), DoubleUtil.DoubleToInt(currentSizeDeviceUnits.Height), 20);
			try
			{
				_updateHwndLocation = false;
				_updateStartupLocation = true;
				CoerceValue(TopProperty);
				CoerceValue(LeftProperty);
			}
			finally
			{
				_updateHwndLocation = true;
				_updateStartupLocation = false;
			}
		}
		if (!HwndCreatedButNotShown)
		{
			SetRootVisualAndUpdateSTC();
		}
	}

	internal void SetRootVisual()
	{
		SetIWindowService();
		if (!IsSourceWindowNull)
		{
			_swh.RootVisual = this;
		}
	}

	internal void SetRootVisualAndUpdateSTC()
	{
		SetRootVisual();
		if (IsSourceWindowNull || (SizeToContent == SizeToContent.Manual && !HwndCreatedButNotShown))
		{
			return;
		}
		MS.Win32.NativeMethods.RECT windowBounds = WindowBounds;
		double leftDeviceUnits = windowBounds.left;
		double topDeviceUnits = windowBounds.top;
		Point point = LogicalToDeviceUnits(new Point(base.ActualWidth, base.ActualHeight));
		if (CalculateWindowLocation(ref leftDeviceUnits, ref topDeviceUnits, new Size(point.X, point.Y)) && WindowState == WindowState.Normal)
		{
			MS.Win32.UnsafeNativeMethods.SetWindowPos(new HandleRef(this, CriticalHandle), new HandleRef(null, IntPtr.Zero), DoubleUtil.DoubleToInt(leftDeviceUnits), DoubleUtil.DoubleToInt(topDeviceUnits), 0, 0, 21);
			try
			{
				_updateHwndLocation = false;
				_updateStartupLocation = true;
				CoerceValue(TopProperty);
				CoerceValue(LeftProperty);
			}
			finally
			{
				_updateHwndLocation = true;
				_updateStartupLocation = false;
			}
		}
	}

	private void CreateWindowStyle()
	{
		_Style &= -12582913;
		_StyleEx &= -641;
		switch (WindowStyle)
		{
		case WindowStyle.None:
			_Style &= -12582913;
			break;
		case WindowStyle.SingleBorderWindow:
			_Style |= 12582912;
			break;
		case WindowStyle.ThreeDBorderWindow:
			_Style |= 12582912;
			_StyleEx |= 512;
			break;
		case WindowStyle.ToolWindow:
			_Style |= 12582912;
			_StyleEx |= 128;
			break;
		}
	}

	internal virtual void UpdateTitle(string title)
	{
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			MS.Win32.UnsafeNativeMethods.SetWindowText(new HandleRef(this, CriticalHandle), title);
		}
	}

	private void UpdateHwndSizeOnWidthHeightChange(double widthLogicalUnits, double heightLogicalUnits)
	{
		_ = _inTrustedSubWindow;
		Point point = LogicalToDeviceUnits(new Point(widthLogicalUnits, heightLogicalUnits));
		MS.Win32.UnsafeNativeMethods.SetWindowPos(new HandleRef(this, CriticalHandle), new HandleRef(null, IntPtr.Zero), 0, 0, DoubleUtil.DoubleToInt(point.X), DoubleUtil.DoubleToInt(point.Y), 22);
	}

	internal void HandleActivate(bool windowActivated)
	{
		if (windowActivated && !IsActive)
		{
			SetValue(IsActivePropertyKey, BooleanBoxes.TrueBox);
			OnActivated(EventArgs.Empty);
		}
		else if (!windowActivated && IsActive)
		{
			SetValue(IsActivePropertyKey, BooleanBoxes.FalseBox);
			OnDeactivated(EventArgs.Empty);
		}
	}

	internal virtual void UpdateHeight(double newHeight)
	{
		if (WindowState == WindowState.Normal)
		{
			UpdateHwndSizeOnWidthHeightChange(DeviceToLogicalUnits(new Point(WindowBounds.Width, 0.0)).X, newHeight);
		}
		else
		{
			UpdateHwndRestoreBounds(newHeight, BoundsSpecified.Height);
		}
	}

	internal virtual void UpdateWidth(double newWidth)
	{
		if (WindowState == WindowState.Normal)
		{
			UpdateHwndSizeOnWidthHeightChange(newWidth, DeviceToLogicalUnits(new Point(0.0, WindowBounds.Height)).Y);
		}
		else
		{
			UpdateHwndRestoreBounds(newWidth, BoundsSpecified.Width);
		}
	}

	internal virtual void VerifyApiSupported()
	{
	}

	private Size MeasureOverrideHelper(Size constraint)
	{
		if (IsSourceWindowNull || IsCompositionTargetInvalid)
		{
			return new Size(0.0, 0.0);
		}
		if (VisualChildrenCount > 0 && GetVisualChild(0) is UIElement uIElement)
		{
			Size hwndNonClientAreaSizeInMeasureUnits = GetHwndNonClientAreaSizeInMeasureUnits();
			Size availableSize = default(Size);
			availableSize.Width = ((constraint.Width == double.PositiveInfinity) ? double.PositiveInfinity : Math.Max(0.0, constraint.Width - hwndNonClientAreaSizeInMeasureUnits.Width));
			availableSize.Height = ((constraint.Height == double.PositiveInfinity) ? double.PositiveInfinity : Math.Max(0.0, constraint.Height - hwndNonClientAreaSizeInMeasureUnits.Height));
			uIElement.Measure(availableSize);
			Size desiredSize = uIElement.DesiredSize;
			return new Size(desiredSize.Width + hwndNonClientAreaSizeInMeasureUnits.Width, desiredSize.Height + hwndNonClientAreaSizeInMeasureUnits.Height);
		}
		return _swh.GetSizeFromHwndInMeasureUnits();
	}

	internal virtual WindowMinMax GetWindowMinMax()
	{
		WindowMinMax result = default(WindowMinMax);
		Invariant.Assert(!IsCompositionTargetInvalid, "IsCompositionTargetInvalid is supposed to be false here");
		double x = _trackMaxWidthDeviceUnits;
		double y = _trackMaxHeightDeviceUnits;
		if (WindowState == WindowState.Maximized)
		{
			x = Math.Max(_trackMaxWidthDeviceUnits, _windowMaxWidthDeviceUnits);
			y = Math.Max(_trackMaxHeightDeviceUnits, _windowMaxHeightDeviceUnits);
		}
		Point point = DeviceToLogicalUnits(new Point(x, y));
		Point point2 = DeviceToLogicalUnits(new Point(_trackMinWidthDeviceUnits, _trackMinHeightDeviceUnits));
		result.minWidth = Math.Max(base.MinWidth, point2.X);
		if (base.MinWidth > base.MaxWidth)
		{
			result.maxWidth = Math.Min(base.MinWidth, point.X);
		}
		else if (!double.IsPositiveInfinity(base.MaxWidth))
		{
			result.maxWidth = Math.Min(base.MaxWidth, point.X);
		}
		else
		{
			result.maxWidth = point.X;
		}
		result.minHeight = Math.Max(base.MinHeight, point2.Y);
		if (base.MinHeight > base.MaxHeight)
		{
			result.maxHeight = Math.Min(base.MinHeight, point.Y);
		}
		else if (!double.IsPositiveInfinity(base.MaxHeight))
		{
			result.maxHeight = Math.Min(base.MaxHeight, point.Y);
		}
		else
		{
			result.maxHeight = point.Y;
		}
		return result;
	}

	private void LoadedHandler(object sender, RoutedEventArgs e)
	{
		if (_postContentRenderedFromLoadedHandler)
		{
			PostContentRendered();
			_postContentRenderedFromLoadedHandler = false;
			base.Loaded -= LoadedHandler;
		}
	}

	private void PostContentRendered()
	{
		if (_contentRenderedCallback != null)
		{
			_contentRenderedCallback.Abort();
		}
		_contentRenderedCallback = base.Dispatcher.BeginInvoke(DispatcherPriority.Input, (DispatcherOperationCallback)delegate(object arg)
		{
			Window obj = (Window)arg;
			obj._contentRenderedCallback = null;
			obj.OnContentRendered(EventArgs.Empty);
			return (object)null;
		}, this);
	}

	private void EnsureDialogCommand()
	{
		if (!_dialogCommandAdded)
		{
			CommandBinding commandBinding = new CommandBinding(DialogCancelCommand);
			commandBinding.Executed += OnDialogCommand;
			CommandManager.RegisterClassCommandBinding(typeof(Window), commandBinding);
			_dialogCommandAdded = true;
		}
	}

	private static void OnDialogCommand(object target, ExecutedRoutedEventArgs e)
	{
		(target as Window).OnDialogCancelCommand();
	}

	private void OnDialogCancelCommand()
	{
		if (_showingAsDialog)
		{
			DialogResult = false;
		}
	}

	private bool ThreadWindowsCallback(nint hWnd, nint lparam)
	{
		if (SafeNativeMethods.IsWindowVisible(new HandleRef(null, hWnd)) && SafeNativeMethods.IsWindowEnabled(new HandleRef(null, hWnd)))
		{
			_threadWindowHandles.Add(hWnd);
		}
		return true;
	}

	private void EnableThreadWindows(bool state)
	{
		for (int i = 0; i < _threadWindowHandles.Count; i++)
		{
			nint handle = (nint)_threadWindowHandles[i];
			if (MS.Win32.UnsafeNativeMethods.IsWindow(new HandleRef(null, handle)))
			{
				MS.Win32.UnsafeNativeMethods.EnableWindowNoThrow(new HandleRef(null, handle), state);
			}
		}
		if (state)
		{
			_threadWindowHandles = null;
		}
	}

	private void Initialize()
	{
		base.BypassLayoutPolicies = true;
		if (!IsInsideApp)
		{
			return;
		}
		if (Application.Current.Dispatcher.Thread == Dispatcher.CurrentDispatcher.Thread)
		{
			App.WindowsInternal.Add(this);
			if (App.MainWindow == null)
			{
				App.MainWindow = this;
			}
		}
		else
		{
			App.NonAppWindowsInternal.Add(this);
		}
	}

	internal void VerifyContextAndObjectState()
	{
		VerifyAccess();
	}

	private void VerifyCanShow()
	{
		if (_disposed)
		{
			throw new InvalidOperationException(SR.ReshowNotAllowed);
		}
	}

	private void VerifyNotClosing()
	{
		if (_isClosing)
		{
			throw new InvalidOperationException(SR.InvalidOperationDuringClosing);
		}
		if (!IsSourceWindowNull && IsCompositionTargetInvalid)
		{
			throw new InvalidOperationException(SR.InvalidCompositionTarget);
		}
	}

	private void VerifyHwndCreateShowState()
	{
		if (HwndCreatedButNotShown)
		{
			throw new InvalidOperationException(SR.NotAllowedBeforeShow);
		}
	}

	private void SetIWindowService()
	{
		if (GetValue(IWindowServiceProperty) == null)
		{
			SetValue(IWindowServiceProperty, this);
		}
	}

	private nint GetCurrentMonitorFromMousePosition()
	{
		MS.Win32.NativeMethods.POINT pt = default(MS.Win32.NativeMethods.POINT);
		MS.Win32.UnsafeNativeMethods.TryGetCursorPos(ref pt);
		return SafeNativeMethods.MonitorFromPoint(pt, 2);
	}

	private bool CalculateWindowLocation(ref double leftDeviceUnits, ref double topDeviceUnits, Size currentSizeDeviceUnits)
	{
		double value = leftDeviceUnits;
		double value2 = topDeviceUnits;
		switch (_windowStartupLocation)
		{
		case WindowStartupLocation.CenterScreen:
		{
			nint zero = IntPtr.Zero;
			zero = ((_ownerHandle != IntPtr.Zero && (_hiddenWindow == null || _hiddenWindow.Handle != _ownerHandle)) ? MonitorFromWindow(_ownerHandle) : GetCurrentMonitorFromMousePosition());
			if (zero != IntPtr.Zero)
			{
				CalculateCenterScreenPosition(zero, currentSizeDeviceUnits, ref leftDeviceUnits, ref topDeviceUnits);
			}
			break;
		}
		case WindowStartupLocation.CenterOwner:
		{
			Rect rect = Rect.Empty;
			if (CanCenterOverWPFOwner)
			{
				if (Owner.WindowState == WindowState.Maximized || Owner.WindowState == WindowState.Minimized)
				{
					goto case WindowStartupLocation.CenterScreen;
				}
				Point point;
				if (Owner.CriticalHandle == IntPtr.Zero)
				{
					point = Owner.LogicalToDeviceUnits(new Point(Owner.Width, Owner.Height));
				}
				else
				{
					Size windowSize = Owner.WindowSize;
					point = new Point(windowSize.Width, windowSize.Height);
				}
				Point point2 = Owner.LogicalToDeviceUnits(new Point(Owner.Left, Owner.Top));
				rect = new Rect(point2.X, point2.Y, point.X, point.Y);
			}
			else if (_ownerHandle != IntPtr.Zero && MS.Win32.UnsafeNativeMethods.IsWindow(new HandleRef(null, _ownerHandle)))
			{
				rect = GetNormalRectDeviceUnits(_ownerHandle);
			}
			if (!rect.IsEmpty)
			{
				leftDeviceUnits = rect.X + (rect.Width - currentSizeDeviceUnits.Width) / 2.0;
				topDeviceUnits = rect.Y + (rect.Height - currentSizeDeviceUnits.Height) / 2.0;
				MS.Win32.NativeMethods.RECT rECT = WorkAreaBoundsForHwnd(_ownerHandle);
				leftDeviceUnits = Math.Min(leftDeviceUnits, (double)rECT.right - currentSizeDeviceUnits.Width);
				leftDeviceUnits = Math.Max(leftDeviceUnits, rECT.left);
				topDeviceUnits = Math.Min(topDeviceUnits, (double)rECT.bottom - currentSizeDeviceUnits.Height);
				topDeviceUnits = Math.Max(topDeviceUnits, rECT.top);
			}
			break;
		}
		}
		if (DoubleUtil.AreClose(value, leftDeviceUnits))
		{
			return !DoubleUtil.AreClose(value2, topDeviceUnits);
		}
		return true;
	}

	private static MS.Win32.NativeMethods.RECT WorkAreaBoundsForHwnd(nint hwnd)
	{
		return WorkAreaBoundsForMointor(MonitorFromWindow(hwnd));
	}

	private static MS.Win32.NativeMethods.RECT WorkAreaBoundsForMointor(nint hMonitor)
	{
		MS.Win32.NativeMethods.MONITORINFOEX mONITORINFOEX = new MS.Win32.NativeMethods.MONITORINFOEX();
		SafeNativeMethods.GetMonitorInfo(new HandleRef(null, hMonitor), mONITORINFOEX);
		return mONITORINFOEX.rcWork;
	}

	private static nint MonitorFromWindow(nint hwnd)
	{
		nint num = SafeNativeMethods.MonitorFromWindow(new HandleRef(null, hwnd), 2);
		if (num == IntPtr.Zero)
		{
			throw new Win32Exception();
		}
		return num;
	}

	internal static void CalculateCenterScreenPosition(nint hMonitor, Size currentSizeDeviceUnits, ref double leftDeviceUnits, ref double topDeviceUnits)
	{
		MS.Win32.NativeMethods.RECT rECT = WorkAreaBoundsForMointor(hMonitor);
		double num = rECT.right - rECT.left;
		double num2 = rECT.bottom - rECT.top;
		leftDeviceUnits = (double)rECT.left + (num - currentSizeDeviceUnits.Width) / 2.0;
		topDeviceUnits = (double)rECT.top + (num2 - currentSizeDeviceUnits.Height) / 2.0;
	}

	private Rect GetNormalRectDeviceUnits(nint hwndHandle)
	{
		int windowLong = MS.Win32.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, hwndHandle), -20);
		MS.Win32.NativeMethods.WINDOWPLACEMENT placement = new MS.Win32.NativeMethods.WINDOWPLACEMENT
		{
			length = Marshal.SizeOf(typeof(MS.Win32.NativeMethods.WINDOWPLACEMENT))
		};
		MS.Win32.UnsafeNativeMethods.GetWindowPlacement(new HandleRef(this, hwndHandle), ref placement);
		Point pt = new Point(placement.rcNormalPosition_left, placement.rcNormalPosition_top);
		if ((windowLong & 0x80) == 0)
		{
			pt = TransformWorkAreaScreenArea(pt, TransformType.WorkAreaToScreenArea);
		}
		Point point = new Point(placement.rcNormalPosition_right - placement.rcNormalPosition_left, placement.rcNormalPosition_bottom - placement.rcNormalPosition_top);
		return new Rect(pt.X, pt.Y, point.X, point.Y);
	}

	private Rect GetNormalRectLogicalUnits(nint hwndHandle)
	{
		Rect normalRectDeviceUnits = GetNormalRectDeviceUnits(hwndHandle);
		Point point = DeviceToLogicalUnits(new Point(normalRectDeviceUnits.Width, normalRectDeviceUnits.Height));
		Point point2 = DeviceToLogicalUnits(new Point(normalRectDeviceUnits.X, normalRectDeviceUnits.Y));
		return new Rect(point2.X, point2.Y, point.X, point.Y);
	}

	private void CreateWindowState()
	{
		switch (WindowState)
		{
		case WindowState.Maximized:
			_Style |= 16777216;
			break;
		case WindowState.Minimized:
			_Style |= 536870912;
			break;
		case WindowState.Normal:
			break;
		}
	}

	private void CreateTopmost()
	{
		if (Topmost)
		{
			_StyleEx |= 8;
		}
		else
		{
			_StyleEx &= -9;
		}
	}

	private void CreateResizibility()
	{
		_Style &= -458753;
		switch (ResizeMode)
		{
		case ResizeMode.CanMinimize:
			_Style |= 131072;
			break;
		case ResizeMode.CanResize:
		case ResizeMode.CanResizeWithGrip:
			_Style |= 458752;
			break;
		case ResizeMode.NoResize:
			break;
		}
	}

	private void UpdateIcon()
	{
		MS.Win32.NativeMethods.IconHandle largeIconHandle;
		MS.Win32.NativeMethods.IconHandle smallIconHandle;
		if (_icon != null)
		{
			IconHelper.GetIconHandlesFromImageSource(_icon, out largeIconHandle, out smallIconHandle);
		}
		else if (_defaultLargeIconHandle == null && _defaultSmallIconHandle == null)
		{
			IconHelper.GetDefaultIconHandles(out largeIconHandle, out smallIconHandle);
			_defaultLargeIconHandle = largeIconHandle;
			_defaultSmallIconHandle = smallIconHandle;
		}
		else
		{
			largeIconHandle = _defaultLargeIconHandle;
			smallIconHandle = _defaultSmallIconHandle;
		}
		HandleRef[] array = new HandleRef[2]
		{
			new HandleRef(this, CriticalHandle),
			default(HandleRef)
		};
		int num = 1;
		if (_hiddenWindow != null)
		{
			array[1] = new HandleRef(_hiddenWindow, _hiddenWindow.Handle);
			num++;
		}
		for (int i = 0; i < num; i++)
		{
			HandleRef hWnd = array[i];
			MS.Win32.UnsafeNativeMethods.SendMessage(hWnd, WindowMessage.WM_SETICON, 1, largeIconHandle);
			MS.Win32.UnsafeNativeMethods.SendMessage(hWnd, WindowMessage.WM_SETICON, 0, smallIconHandle);
		}
		if (_currentLargeIconHandle != null && _currentLargeIconHandle != _defaultLargeIconHandle)
		{
			_currentLargeIconHandle.Dispose();
		}
		if (_currentSmallIconHandle != null && _currentSmallIconHandle != _defaultSmallIconHandle)
		{
			_currentSmallIconHandle.Dispose();
		}
		_currentLargeIconHandle = largeIconHandle;
		_currentSmallIconHandle = smallIconHandle;
	}

	private void SetOwnerHandle(nint ownerHandle)
	{
		if (_ownerHandle == ownerHandle && _ownerHandle == IntPtr.Zero)
		{
			return;
		}
		_ownerHandle = ((IntPtr.Zero == ownerHandle && !ShowInTaskbar) ? EnsureHiddenWindow().Handle : ownerHandle);
		if (!IsSourceWindowNull)
		{
			MS.Win32.UnsafeNativeMethods.SetWindowLong(new HandleRef(null, CriticalHandle), -8, _ownerHandle);
			if (_ownerWindow != null && _ownerWindow.CriticalHandle != _ownerHandle)
			{
				_ownerWindow.OwnedWindowsInternal.Remove(this);
				_ownerWindow = null;
			}
		}
	}

	private void OnSourceWindowDisposed(object sender, EventArgs e)
	{
		if (!_disposed)
		{
			InternalDispose();
		}
	}

	private nint WindowFilterMessage(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		nint refInt = IntPtr.Zero;
		switch (msg)
		{
		case 36:
			handled = WmGetMinMaxInfo(lParam);
			break;
		case 5:
			handled = WmSizeChanged(wParam);
			break;
		}
		if (_swh != null && _swh.CompositionTarget != null)
		{
			if (msg == (int)WM_TASKBARBUTTONCREATED || msg == (int)WM_APPLYTASKBARITEMINFO)
			{
				if (_taskbarRetryTimer != null)
				{
					_taskbarRetryTimer.Stop();
				}
				ApplyTaskbarItemInfo();
			}
			else
			{
				switch (msg)
				{
				case 16:
					handled = WmClose();
					break;
				case 2:
					handled = WmDestroy();
					break;
				case 6:
					handled = WmActivate(wParam);
					break;
				case 3:
					handled = WmMoveChanged();
					break;
				case 132:
					handled = WmNcHitTest(lParam, ref refInt);
					break;
				case 24:
					handled = WmShowWindow(wParam, lParam);
					break;
				case 273:
					handled = WmCommand(wParam, lParam);
					break;
				default:
					handled = false;
					break;
				}
			}
		}
		return refInt;
	}

	private bool WmCommand(nint wParam, nint lParam)
	{
		if (MS.Win32.NativeMethods.SignedHIWORD(((IntPtr)wParam).ToInt32()) == 6144)
		{
			TaskbarItemInfo taskbarItemInfo = TaskbarItemInfo;
			if (taskbarItemInfo != null)
			{
				int num = MS.Win32.NativeMethods.SignedLOWORD(((IntPtr)wParam).ToInt32());
				if (num >= 0 && num < taskbarItemInfo.ThumbButtonInfos.Count)
				{
					taskbarItemInfo.ThumbButtonInfos[num].InvokeClick();
				}
			}
			return true;
		}
		return false;
	}

	private bool WmClose()
	{
		if (IsSourceWindowNull || IsCompositionTargetInvalid)
		{
			return false;
		}
		_isClosing = true;
		CancelEventArgs cancelEventArgs = new CancelEventArgs(cancel: false);
		try
		{
			OnClosing(cancelEventArgs);
		}
		catch
		{
			CloseWindowFromWmClose();
			throw;
		}
		if (ShouldCloseWindow(cancelEventArgs.Cancel))
		{
			CloseWindowFromWmClose();
			return false;
		}
		_isClosing = false;
		_dialogResult = null;
		return true;
	}

	private void CloseWindowFromWmClose()
	{
		if (_showingAsDialog)
		{
			DoDialogHide();
		}
		ClearRootVisual();
		ClearHiddenWindowIfAny();
	}

	private bool ShouldCloseWindow(bool cancelled)
	{
		if (cancelled && !_appShuttingDown)
		{
			return _ignoreCancel;
		}
		return true;
	}

	private void DoDialogHide()
	{
		if (_dispatcherFrame != null)
		{
			_dispatcherFrame.Continue = false;
			_dispatcherFrame = null;
		}
		if (!_dialogResult.HasValue)
		{
			_dialogResult = false;
		}
		_showingAsDialog = false;
		bool isActiveWindow = _swh.IsActiveWindow;
		EnableThreadWindows(state: true);
		if (isActiveWindow && _dialogPreviousActiveHandle != IntPtr.Zero && MS.Win32.UnsafeNativeMethods.IsWindow(new HandleRef(this, _dialogPreviousActiveHandle)))
		{
			MS.Win32.UnsafeNativeMethods.SetActiveWindow(new HandleRef(this, _dialogPreviousActiveHandle));
		}
	}

	private void UpdateWindowListsOnClose()
	{
		WindowCollection ownedWindowsInternal = OwnedWindowsInternal;
		while (ownedWindowsInternal.Count > 0)
		{
			ownedWindowsInternal[0].InternalClose(shutdown: false, ignoreCancel: true);
		}
		if (!IsOwnerNull)
		{
			Owner.OwnedWindowsInternal.Remove(this);
		}
		if (!IsInsideApp)
		{
			return;
		}
		if (Application.Current.Dispatcher.Thread == Dispatcher.CurrentDispatcher.Thread)
		{
			App.WindowsInternal.Remove(this);
			if (!_appShuttingDown && ((App.Windows.Count == 0 && App.ShutdownMode == ShutdownMode.OnLastWindowClose) || (App.MainWindow == this && App.ShutdownMode == ShutdownMode.OnMainWindowClose)))
			{
				App.CriticalShutdown(0);
			}
			TryClearingMainWindow();
		}
		else
		{
			App.NonAppWindowsInternal.Remove(this);
		}
	}

	private bool WmDestroy()
	{
		if (IsSourceWindowNull)
		{
			return false;
		}
		if (!_disposed)
		{
			InternalDispose();
		}
		OnClosed(EventArgs.Empty);
		return false;
	}

	private bool WmActivate(nint wParam)
	{
		if (IsSourceWindowNull || IsCompositionTargetInvalid)
		{
			return false;
		}
		bool windowActivated = ((MS.Win32.NativeMethods.SignedLOWORD(wParam) != 0) ? true : false);
		HandleActivate(windowActivated);
		return false;
	}

	private void UpdateDimensionsToRestoreBounds()
	{
		Rect restoreBounds = RestoreBounds;
		SetValue(LeftProperty, restoreBounds.Left);
		SetValue(TopProperty, restoreBounds.Top);
		SetValue(FrameworkElement.WidthProperty, restoreBounds.Width);
		SetValue(FrameworkElement.HeightProperty, restoreBounds.Height);
	}

	private bool WmSizeChanged(nint wParam)
	{
		if (IsSourceWindowNull || IsCompositionTargetInvalid)
		{
			return false;
		}
		MS.Win32.NativeMethods.RECT windowBounds = WindowBounds;
		Point ptDeviceUnits = new Point(windowBounds.right - windowBounds.left, windowBounds.bottom - windowBounds.top);
		Point point = DeviceToLogicalUnits(ptDeviceUnits);
		try
		{
			_updateHwndSize = false;
			SetValue(FrameworkElement.WidthProperty, point.X);
			SetValue(FrameworkElement.HeightProperty, point.Y);
		}
		finally
		{
			_updateHwndSize = true;
		}
		UpdateTaskbarThumbnailClipping();
		switch ((int)wParam)
		{
		case 2:
			if (_previousWindowState == WindowState.Maximized)
			{
				break;
			}
			if (WindowState != WindowState.Maximized)
			{
				try
				{
					_updateHwndLocation = false;
					_updateHwndSize = false;
					UpdateDimensionsToRestoreBounds();
				}
				finally
				{
					_updateHwndSize = true;
					_updateHwndLocation = true;
				}
				WindowState = WindowState.Maximized;
			}
			_windowMaxWidthDeviceUnits = Math.Max(_windowMaxWidthDeviceUnits, ptDeviceUnits.X);
			_windowMaxHeightDeviceUnits = Math.Max(_windowMaxHeightDeviceUnits, ptDeviceUnits.Y);
			_previousWindowState = WindowState.Maximized;
			OnStateChanged(EventArgs.Empty);
			break;
		case 1:
			if (_previousWindowState == WindowState.Minimized)
			{
				break;
			}
			if (WindowState != WindowState.Minimized)
			{
				try
				{
					_updateHwndSize = false;
					_updateHwndLocation = false;
					UpdateDimensionsToRestoreBounds();
				}
				finally
				{
					_updateHwndSize = true;
					_updateHwndLocation = true;
				}
				WindowState = WindowState.Minimized;
			}
			_previousWindowState = WindowState.Minimized;
			OnStateChanged(EventArgs.Empty);
			break;
		case 0:
			if (_previousWindowState != 0)
			{
				if (WindowState != 0)
				{
					WindowState = WindowState.Normal;
					WmMoveChangedHelper();
				}
				_previousWindowState = WindowState.Normal;
				OnStateChanged(EventArgs.Empty);
			}
			break;
		}
		return false;
	}

	private bool WmMoveChanged()
	{
		if (IsSourceWindowNull || IsCompositionTargetInvalid)
		{
			return false;
		}
		MS.Win32.NativeMethods.RECT windowBounds = WindowBounds;
		Point point = DeviceToLogicalUnits(new Point(windowBounds.left, windowBounds.top));
		if (!DoubleUtil.AreClose(_actualLeft, point.X) || !DoubleUtil.AreClose(_actualTop, point.Y))
		{
			_actualLeft = point.X;
			_actualTop = point.Y;
			WmMoveChangedHelper();
			UIElementAutomationPeer.FromElement(this)?.InvalidatePeer();
		}
		return false;
	}

	internal virtual void WmMoveChangedHelper()
	{
		if (WindowState == WindowState.Normal)
		{
			try
			{
				_updateHwndLocation = false;
				SetValue(LeftProperty, _actualLeft);
				SetValue(TopProperty, _actualTop);
			}
			finally
			{
				_updateHwndLocation = true;
			}
			OnLocationChanged(EventArgs.Empty);
		}
	}

	private unsafe bool WmGetMinMaxInfo(nint lParam)
	{
		MS.Win32.NativeMethods.MINMAXINFO mINMAXINFO = *(MS.Win32.NativeMethods.MINMAXINFO*)lParam;
		_trackMinWidthDeviceUnits = mINMAXINFO.ptMinTrackSize.x;
		_trackMinHeightDeviceUnits = mINMAXINFO.ptMinTrackSize.y;
		_trackMaxWidthDeviceUnits = mINMAXINFO.ptMaxTrackSize.x;
		_trackMaxHeightDeviceUnits = mINMAXINFO.ptMaxTrackSize.y;
		_windowMaxWidthDeviceUnits = mINMAXINFO.ptMaxSize.x;
		_windowMaxHeightDeviceUnits = mINMAXINFO.ptMaxSize.y;
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			WindowMinMax windowMinMax = GetWindowMinMax();
			Point point = LogicalToDeviceUnits(new Point(windowMinMax.minWidth, windowMinMax.minHeight));
			Point point2 = LogicalToDeviceUnits(new Point(windowMinMax.maxWidth, windowMinMax.maxHeight));
			mINMAXINFO.ptMinTrackSize.x = DoubleUtil.DoubleToInt(point.X);
			mINMAXINFO.ptMinTrackSize.y = DoubleUtil.DoubleToInt(point.Y);
			mINMAXINFO.ptMaxTrackSize.x = DoubleUtil.DoubleToInt(point2.X);
			mINMAXINFO.ptMaxTrackSize.y = DoubleUtil.DoubleToInt(point2.Y);
			*(MS.Win32.NativeMethods.MINMAXINFO*)lParam = mINMAXINFO;
		}
		return true;
	}

	private bool WmNcHitTest(nint lParam, ref nint refInt)
	{
		if (IsSourceWindowNull || IsCompositionTargetInvalid)
		{
			return false;
		}
		return HandleWmNcHitTestMsg(lParam, ref refInt);
	}

	internal virtual bool HandleWmNcHitTestMsg(nint lParam, ref nint refInt)
	{
		if (_resizeGripControl == null || ResizeMode != ResizeMode.CanResizeWithGrip)
		{
			return false;
		}
		int x = MS.Win32.NativeMethods.SignedLOWORD(lParam);
		int y = MS.Win32.NativeMethods.SignedHIWORD(lParam);
		MS.Win32.NativeMethods.POINT pointRelativeToWindow = GetPointRelativeToWindow(x, y);
		Point point = DeviceToLogicalUnits(new Point(pointRelativeToWindow.x, pointRelativeToWindow.y));
		GeneralTransform generalTransform = TransformToDescendant(_resizeGripControl);
		Point result = point;
		if (generalTransform == null || !generalTransform.TryTransform(point, out result))
		{
			return false;
		}
		if (result.X < 0.0 || result.Y < 0.0 || result.X > _resizeGripControl.RenderSize.Width || result.Y > _resizeGripControl.RenderSize.Height)
		{
			return false;
		}
		if (base.FlowDirection == FlowDirection.RightToLeft)
		{
			refInt = new IntPtr(16);
		}
		else
		{
			refInt = new IntPtr(17);
		}
		return true;
	}

	private bool WmShowWindow(nint wParam, nint lParam)
	{
		if (IsSourceWindowNull || IsCompositionTargetInvalid)
		{
			return false;
		}
		switch (MS.Win32.NativeMethods.IntPtrToInt32(lParam))
		{
		case 1:
			_isVisible = false;
			UpdateVisibilityProperty(Visibility.Hidden);
			break;
		case 3:
			_isVisible = true;
			UpdateVisibilityProperty(Visibility.Visible);
			break;
		}
		return false;
	}

	private static void _OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Window)d).OnIconChanged(e.NewValue as ImageSource);
	}

	private void OnIconChanged(ImageSource newIcon)
	{
		_icon = newIcon;
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			UpdateIcon();
		}
	}

	private static void _OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Window)d).OnTitleChanged();
	}

	private static bool _ValidateText(object value)
	{
		return value != null;
	}

	private void OnTitleChanged()
	{
		UpdateTitle(Title);
	}

	private static void _OnShowInTaskbarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Window)d).OnShowInTaskbarChanged();
	}

	private void OnShowInTaskbarChanged()
	{
		VerifyApiSupported();
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			bool flag = false;
			if (_isVisible)
			{
				MS.Win32.UnsafeNativeMethods.SetWindowPos(new HandleRef(this, CriticalHandle), MS.Win32.NativeMethods.NullHandleRef, 0, 0, 0, 0, 1175);
				flag = true;
			}
			using (HwndStyleManager.StartManaging(this, StyleFromHwnd, StyleExFromHwnd))
			{
				SetTaskbarStatus();
			}
			if (flag)
			{
				MS.Win32.UnsafeNativeMethods.SetWindowPos(new HandleRef(this, CriticalHandle), MS.Win32.NativeMethods.NullHandleRef, 0, 0, 0, 0, 1111);
			}
		}
	}

	private static bool _ValidateWindowStateCallback(object value)
	{
		return IsValidWindowState((WindowState)value);
	}

	private static void _OnWindowStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Window)d).OnWindowStateChanged((WindowState)e.NewValue);
	}

	private void OnWindowStateChanged(WindowState windowState)
	{
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			if (_isVisible)
			{
				HandleRef hWnd = new HandleRef(this, CriticalHandle);
				int style = _Style;
				switch (windowState)
				{
				case WindowState.Normal:
					if ((style & 0x1000000) == 16777216)
					{
						if (ShowActivated || IsActive)
						{
							MS.Win32.UnsafeNativeMethods.ShowWindow(hWnd, 9);
						}
						else
						{
							MS.Win32.UnsafeNativeMethods.ShowWindow(hWnd, 4);
						}
					}
					else if ((style & 0x20000000) == 536870912)
					{
						MS.Win32.NativeMethods.WINDOWPLACEMENT placement = default(MS.Win32.NativeMethods.WINDOWPLACEMENT);
						placement.length = Marshal.SizeOf(placement);
						MS.Win32.UnsafeNativeMethods.GetWindowPlacement(hWnd, ref placement);
						if ((placement.flags & 2) == 2)
						{
							MS.Win32.UnsafeNativeMethods.ShowWindow(hWnd, 9);
						}
						else if (ShowActivated)
						{
							MS.Win32.UnsafeNativeMethods.ShowWindow(hWnd, 9);
						}
						else
						{
							MS.Win32.UnsafeNativeMethods.ShowWindow(hWnd, 4);
						}
					}
					break;
				case WindowState.Maximized:
					if ((style & 0x1000000) != 16777216)
					{
						MS.Win32.UnsafeNativeMethods.ShowWindow(hWnd, 3);
					}
					break;
				case WindowState.Minimized:
					if ((style & 0x20000000) != 536870912)
					{
						MS.Win32.UnsafeNativeMethods.ShowWindow(hWnd, 6);
					}
					break;
				}
			}
		}
		else
		{
			_previousWindowState = windowState;
		}
		try
		{
			_updateHwndLocation = false;
			CoerceValue(TopProperty);
			CoerceValue(LeftProperty);
		}
		finally
		{
			_updateHwndLocation = true;
		}
	}

	private static bool _ValidateWindowStyleCallback(object value)
	{
		return IsValidWindowStyle((WindowStyle)value);
	}

	private static void _OnWindowStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Window)d).OnWindowStyleChanged((WindowStyle)e.NewValue);
	}

	private void OnWindowStyleChanged(WindowStyle windowStyle)
	{
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			using (HwndStyleManager.StartManaging(this, StyleFromHwnd, StyleExFromHwnd))
			{
				CreateWindowStyle();
			}
		}
	}

	private static void _OnTopmostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Window)d).OnTopmostChanged((bool)e.NewValue);
	}

	private void OnTopmostChanged(bool topmost)
	{
		VerifyApiSupported();
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			HandleRef hWndInsertAfter = (topmost ? MS.Win32.NativeMethods.HWND_TOPMOST : MS.Win32.NativeMethods.HWND_NOTOPMOST);
			MS.Win32.UnsafeNativeMethods.SetWindowPos(new HandleRef(null, CriticalHandle), hWndInsertAfter, 0, 0, 0, 0, 19);
		}
	}

	private static object CoerceVisibility(DependencyObject d, object value)
	{
		Window window = (Window)d;
		if ((Visibility)value == Visibility.Visible)
		{
			window.VerifyCanShow();
			window.VerifyConsistencyWithAllowsTransparency();
			window.VerifyNotClosing();
			window.VerifyConsistencyWithShowActivated();
		}
		return value;
	}

	private static void _OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Window window = (Window)d;
		window._isVisibilitySet = true;
		if (!window._visibilitySetInternally)
		{
			bool flag = VisibilityToBool((Visibility)e.NewValue);
			window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(window.ShowHelper), flag ? BooleanBoxes.TrueBox : BooleanBoxes.FalseBox);
		}
	}

	private void SafeCreateWindowDuringShow()
	{
		if (IsSourceWindowNull)
		{
			CreateSourceWindowDuringShow();
		}
		else if (HwndCreatedButNotShown)
		{
			SetRootVisualAndUpdateSTC();
			_hwndCreatedButNotShown = false;
		}
	}

	private void SetShowKeyboardCueState()
	{
		if (KeyboardNavigation.IsKeyboardMostRecentInputDevice())
		{
			_previousKeyboardCuesProperty = (bool)GetValue(KeyboardNavigation.ShowKeyboardCuesProperty);
			SetValue(KeyboardNavigation.ShowKeyboardCuesProperty, BooleanBoxes.TrueBox);
			_resetKeyboardCuesProperty = true;
		}
	}

	private void ClearShowKeyboardCueState()
	{
		if (_resetKeyboardCuesProperty)
		{
			_resetKeyboardCuesProperty = false;
			SetValue(KeyboardNavigation.ShowKeyboardCuesProperty, BooleanBoxes.Box(_previousKeyboardCuesProperty));
		}
	}

	private void UpdateVisibilityProperty(Visibility value)
	{
		try
		{
			_visibilitySetInternally = true;
			SetValue(UIElement.VisibilityProperty, value);
		}
		finally
		{
			_visibilitySetInternally = false;
		}
	}

	private object ShowHelper(object booleanBox)
	{
		if (_disposed)
		{
			return null;
		}
		bool flag = (bool)booleanBox;
		_isClosing = false;
		if (_isVisible == flag)
		{
			return null;
		}
		if (flag)
		{
			if (Application.IsShuttingDown)
			{
				return null;
			}
			SetShowKeyboardCueState();
			SafeCreateWindowDuringShow();
			_isVisible = true;
		}
		else
		{
			ClearShowKeyboardCueState();
			if (_showingAsDialog)
			{
				DoDialogHide();
			}
			_isVisible = false;
		}
		if (!IsSourceWindowNull)
		{
			int num = 0;
			num = (flag ? nCmdForShow() : 0);
			if ((bool)GetValue(TopmostProperty) && FrameworkCompatibilityPreferences.GetUseSetWindowPosForTopmostWindows() && (num == 5 || num == 8))
			{
				int num2 = ((num == 8) ? 16 : 0);
				MS.Win32.UnsafeNativeMethods.SetWindowPos(new HandleRef(this, CriticalHandle), MS.Win32.NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, num2 | 2 | 1 | 0x200 | 0x40);
			}
			else
			{
				MS.Win32.UnsafeNativeMethods.ShowWindow(new HandleRef(this, CriticalHandle), num);
			}
			SafeStyleSetter();
		}
		if (_showingAsDialog && _isVisible)
		{
			try
			{
				ComponentDispatcher.PushModal();
				_dispatcherFrame = new DispatcherFrame();
				Dispatcher.PushFrame(_dispatcherFrame);
			}
			finally
			{
				ComponentDispatcher.PopModal();
			}
		}
		return null;
	}

	internal virtual int nCmdForShow()
	{
		int num = 0;
		return WindowState switch
		{
			WindowState.Maximized => 3, 
			WindowState.Minimized => ShowActivated ? 2 : 7, 
			_ => ShowActivated ? 5 : 8, 
		};
	}

	private void SafeStyleSetter()
	{
		using (HwndStyleManager.StartManaging(this, StyleFromHwnd, StyleExFromHwnd))
		{
			_Style = (_isVisible ? (_Style | 0x10000000) : _Style);
		}
	}

	private static bool _ValidateSizeToContentCallback(object value)
	{
		return IsValidSizeToContent((SizeToContent)value);
	}

	private static object _SizeToContentGetValueOverride(DependencyObject d)
	{
		return (d as Window).SizeToContent;
	}

	private static void _OnSizeToContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as Window).OnSizeToContentChanged((SizeToContent)e.NewValue);
	}

	private void OnSizeToContentChanged(SizeToContent sizeToContent)
	{
		VerifyApiSupported();
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			HwndSourceSizeToContent = sizeToContent;
		}
	}

	private static void ValidateLengthForHeightWidth(double l)
	{
		if (!double.IsPositiveInfinity(l) && !double.IsNaN(l) && (l > 2147483647.0 || l < -2147483648.0))
		{
			throw new ArgumentException(SR.Format(SR.ValueNotBetweenInt32MinMax, l));
		}
	}

	private static void ValidateTopLeft(double length)
	{
		if (double.IsPositiveInfinity(length) || double.IsNegativeInfinity(length))
		{
			throw new ArgumentException(SR.Format(SR.InvalidValueForTopLeft, length));
		}
		if (length > 2147483647.0 || length < -2147483648.0)
		{
			throw new ArgumentException(SR.Format(SR.ValueNotBetweenInt32MinMax, length));
		}
	}

	private static void _OnHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Window window = d as Window;
		if (window._updateHwndSize)
		{
			window.OnHeightChanged((double)e.NewValue);
		}
	}

	private void OnHeightChanged(double height)
	{
		ValidateLengthForHeightWidth(height);
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid && !double.IsNaN(height))
		{
			UpdateHeight(height);
		}
	}

	private static void _OnMinHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as Window).OnMinHeightChanged((double)e.NewValue);
	}

	private void OnMinHeightChanged(double minHeight)
	{
		VerifyApiSupported();
		ValidateLengthForHeightWidth(minHeight);
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			MS.Win32.NativeMethods.RECT windowBounds = WindowBounds;
			Point point = DeviceToLogicalUnits(new Point(windowBounds.Width, windowBounds.Height));
			if (minHeight > point.Y && WindowState == WindowState.Normal)
			{
				UpdateHwndSizeOnWidthHeightChange(point.X, minHeight);
			}
		}
	}

	private static void _OnMaxHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as Window).OnMaxHeightChanged((double)e.NewValue);
	}

	private void OnMaxHeightChanged(double maxHeight)
	{
		VerifyApiSupported();
		ValidateLengthForHeightWidth(base.MaxHeight);
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			MS.Win32.NativeMethods.RECT windowBounds = WindowBounds;
			Point point = DeviceToLogicalUnits(new Point(windowBounds.Width, windowBounds.Height));
			if (maxHeight < point.Y && WindowState == WindowState.Normal)
			{
				UpdateHwndSizeOnWidthHeightChange(point.X, maxHeight);
			}
		}
	}

	private static void _OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Window window = d as Window;
		if (window._updateHwndSize)
		{
			window.OnWidthChanged((double)e.NewValue);
		}
	}

	private void OnWidthChanged(double width)
	{
		ValidateLengthForHeightWidth(width);
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid && !double.IsNaN(width))
		{
			UpdateWidth(width);
		}
	}

	private static void _OnMinWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as Window).OnMinWidthChanged((double)e.NewValue);
	}

	private void OnMinWidthChanged(double minWidth)
	{
		VerifyApiSupported();
		ValidateLengthForHeightWidth(minWidth);
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			MS.Win32.NativeMethods.RECT windowBounds = WindowBounds;
			Point point = DeviceToLogicalUnits(new Point(windowBounds.Width, windowBounds.Height));
			if (minWidth > point.X && WindowState == WindowState.Normal)
			{
				UpdateHwndSizeOnWidthHeightChange(minWidth, point.Y);
			}
		}
	}

	private static void _OnMaxWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as Window).OnMaxWidthChanged((double)e.NewValue);
	}

	private void OnMaxWidthChanged(double maxWidth)
	{
		VerifyApiSupported();
		ValidateLengthForHeightWidth(maxWidth);
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			MS.Win32.NativeMethods.RECT windowBounds = WindowBounds;
			Point point = DeviceToLogicalUnits(new Point(windowBounds.Width, windowBounds.Height));
			if (maxWidth < point.X && WindowState == WindowState.Normal)
			{
				UpdateHwndSizeOnWidthHeightChange(maxWidth, point.Y);
			}
		}
	}

	private void UpdateHwndRestoreBounds(double newValue, BoundsSpecified specifiedRestoreBounds)
	{
		MS.Win32.NativeMethods.WINDOWPLACEMENT placement = default(MS.Win32.NativeMethods.WINDOWPLACEMENT);
		placement.length = Marshal.SizeOf(typeof(MS.Win32.NativeMethods.WINDOWPLACEMENT));
		MS.Win32.UnsafeNativeMethods.GetWindowPlacement(new HandleRef(this, CriticalHandle), ref placement);
		double x = LogicalToDeviceUnits(new Point(newValue, 0.0)).X;
		switch (specifiedRestoreBounds)
		{
		case BoundsSpecified.Height:
			placement.rcNormalPosition_bottom = placement.rcNormalPosition_top + DoubleUtil.DoubleToInt(x);
			break;
		case BoundsSpecified.Width:
			placement.rcNormalPosition_right = placement.rcNormalPosition_left + DoubleUtil.DoubleToInt(x);
			break;
		case BoundsSpecified.Top:
		{
			double y = newValue;
			if ((StyleExFromHwnd & 0x80) == 0)
			{
				y = TransformWorkAreaScreenArea(new Point(0.0, y), TransformType.ScreenAreaToWorkArea).Y;
			}
			y = LogicalToDeviceUnits(new Point(0.0, y)).Y;
			int num2 = placement.rcNormalPosition_bottom - placement.rcNormalPosition_top;
			placement.rcNormalPosition_top = DoubleUtil.DoubleToInt(y);
			placement.rcNormalPosition_bottom = placement.rcNormalPosition_top + num2;
			break;
		}
		case BoundsSpecified.Left:
		{
			double x2 = newValue;
			if ((StyleExFromHwnd & 0x80) == 0)
			{
				x2 = TransformWorkAreaScreenArea(new Point(x2, 0.0), TransformType.ScreenAreaToWorkArea).X;
			}
			x2 = LogicalToDeviceUnits(new Point(x2, 0.0)).X;
			int num = placement.rcNormalPosition_right - placement.rcNormalPosition_left;
			placement.rcNormalPosition_left = DoubleUtil.DoubleToInt(x2);
			placement.rcNormalPosition_right = placement.rcNormalPosition_left + num;
			break;
		}
		}
		if (!_isVisible)
		{
			placement.showCmd = 0;
		}
		MS.Win32.UnsafeNativeMethods.SetWindowPlacement(new HandleRef(this, CriticalHandle), ref placement);
	}

	private Point TransformWorkAreaScreenArea(Point pt, TransformType transformType)
	{
		int num = 0;
		int num2 = 0;
		nint num3 = SafeNativeMethods.MonitorFromWindow(new HandleRef(this, CriticalHandle), 0);
		if (num3 != IntPtr.Zero)
		{
			MS.Win32.NativeMethods.MONITORINFOEX mONITORINFOEX = new MS.Win32.NativeMethods.MONITORINFOEX();
			mONITORINFOEX.cbSize = Marshal.SizeOf(typeof(MS.Win32.NativeMethods.MONITORINFOEX));
			SafeNativeMethods.GetMonitorInfo(new HandleRef(this, num3), mONITORINFOEX);
			MS.Win32.NativeMethods.RECT rcWork = mONITORINFOEX.rcWork;
			MS.Win32.NativeMethods.RECT rcMonitor = mONITORINFOEX.rcMonitor;
			num = rcWork.left - rcMonitor.left;
			num2 = rcWork.top - rcMonitor.top;
		}
		return (transformType != 0) ? new Point(pt.X - (double)num, pt.Y - (double)num2) : new Point(pt.X + (double)num, pt.Y + (double)num2);
	}

	private static object CoerceTop(DependencyObject d, object value)
	{
		Window window = d as Window;
		window.VerifyApiSupported();
		double num = (double)value;
		ValidateTopLeft(num);
		if (window.IsSourceWindowNull || window.IsCompositionTargetInvalid)
		{
			return value;
		}
		if (double.IsNaN(num))
		{
			return window._actualTop;
		}
		if (window.WindowState != 0)
		{
			return value;
		}
		if (window._updateStartupLocation && window.WindowStartupLocation != 0)
		{
			return window._actualTop;
		}
		return value;
	}

	private static void _OnTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Window window = d as Window;
		if (window._updateHwndLocation)
		{
			window.OnTopChanged((double)e.NewValue);
		}
	}

	private void OnTopChanged(double newTop)
	{
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			if (!double.IsNaN(newTop))
			{
				if (WindowState == WindowState.Normal)
				{
					Invariant.Assert(!double.IsNaN(_actualLeft), "_actualLeft cannot be NaN after show");
					UpdateHwndPositionOnTopLeftChange(double.IsNaN(Left) ? _actualLeft : Left, newTop);
				}
				else
				{
					UpdateHwndRestoreBounds(newTop, BoundsSpecified.Top);
				}
			}
		}
		else
		{
			_actualTop = newTop;
		}
	}

	private static object CoerceLeft(DependencyObject d, object value)
	{
		Window window = d as Window;
		window.VerifyApiSupported();
		double num = (double)value;
		ValidateTopLeft(num);
		if (window.IsSourceWindowNull || window.IsCompositionTargetInvalid)
		{
			return value;
		}
		if (double.IsNaN(num))
		{
			return window._actualLeft;
		}
		if (window.WindowState != 0)
		{
			return value;
		}
		if (window._updateStartupLocation && window.WindowStartupLocation != 0)
		{
			return window._actualLeft;
		}
		return value;
	}

	private static void _OnLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Window window = d as Window;
		if (window._updateHwndLocation)
		{
			window.OnLeftChanged((double)e.NewValue);
		}
	}

	private void OnLeftChanged(double newLeft)
	{
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			if (!double.IsNaN(newLeft))
			{
				if (WindowState == WindowState.Normal)
				{
					Invariant.Assert(!double.IsNaN(_actualTop), "_actualTop cannot be NaN after show");
					UpdateHwndPositionOnTopLeftChange(newLeft, double.IsNaN(Top) ? _actualTop : Top);
				}
				else
				{
					UpdateHwndRestoreBounds(newLeft, BoundsSpecified.Left);
				}
			}
		}
		else
		{
			_actualLeft = newLeft;
		}
	}

	private void UpdateHwndPositionOnTopLeftChange(double leftLogicalUnits, double topLogicalUnits)
	{
		Point point = LogicalToDeviceUnits(new Point(leftLogicalUnits, topLogicalUnits));
		MS.Win32.UnsafeNativeMethods.SetWindowPos(new HandleRef(this, CriticalHandle), new HandleRef(null, IntPtr.Zero), DoubleUtil.DoubleToInt(point.X), DoubleUtil.DoubleToInt(point.Y), 0, 0, 21);
	}

	private static bool _ValidateResizeModeCallback(object value)
	{
		return IsValidResizeMode((ResizeMode)value);
	}

	private static void _OnResizeModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as Window).OnResizeModeChanged();
	}

	private void OnResizeModeChanged()
	{
		VerifyApiSupported();
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			using (HwndStyleManager.StartManaging(this, StyleFromHwnd, StyleExFromHwnd))
			{
				CreateResizibility();
			}
		}
	}

	private static object VerifyAccessCoercion(DependencyObject d, object value)
	{
		((Window)d).VerifyApiSupported();
		return value;
	}

	private static void _OnFlowDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as Window).OnFlowDirectionChanged();
	}

	private void OnFlowDirectionChanged()
	{
		if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
		{
			using (HwndStyleManager.StartManaging(this, StyleFromHwnd, StyleExFromHwnd))
			{
				CreateRtl();
			}
		}
	}

	private static object CoerceRenderTransform(DependencyObject d, object value)
	{
		Transform transform = (Transform)value;
		if (value != null && (transform == null || !transform.Value.IsIdentity))
		{
			throw new InvalidOperationException(SR.TransformNotSupported);
		}
		return value;
	}

	private static void _OnRenderTransformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
	}

	private static object CoerceClipToBounds(DependencyObject d, object value)
	{
		if ((bool)value)
		{
			throw new InvalidOperationException(SR.ClipToBoundsNotSupported);
		}
		return value;
	}

	private static void _OnClipToBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
	}

	private HwndWrapper EnsureHiddenWindow()
	{
		if (_hiddenWindow == null)
		{
			_hiddenWindow = new HwndWrapper(0, 13565952, 0, int.MinValue, int.MinValue, int.MinValue, int.MinValue, "Hidden Window", IntPtr.Zero, null);
		}
		return _hiddenWindow;
	}

	private void SetTaskbarStatus()
	{
		if (!ShowInTaskbar)
		{
			EnsureHiddenWindow();
			if (_ownerHandle == IntPtr.Zero)
			{
				SetOwnerHandle(_hiddenWindow.Handle);
				if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
				{
					UpdateIcon();
				}
			}
			_StyleEx &= -262145;
		}
		else
		{
			_StyleEx |= 262144;
			if (!IsSourceWindowNull && _hiddenWindow != null && _ownerHandle == _hiddenWindow.Handle)
			{
				SetOwnerHandle(IntPtr.Zero);
			}
		}
	}

	private void OnTaskbarRetryTimerTick(object sender, EventArgs e)
	{
		MS.Win32.UnsafeNativeMethods.PostMessage(new HandleRef(this, CriticalHandle), WM_APPLYTASKBARITEMINFO, IntPtr.Zero, IntPtr.Zero);
	}

	private void ApplyTaskbarItemInfo()
	{
		if (!Utilities.IsOSWindows7OrNewer)
		{
			if (TraceShell.IsEnabled)
			{
				TraceShell.Trace(TraceEventType.Warning, TraceShell.NotOnWindows7);
			}
		}
		else
		{
			if (IsSourceWindowNull || IsCompositionTargetInvalid || (_taskbarRetryTimer != null && _taskbarRetryTimer.IsEnabled))
			{
				return;
			}
			MS.Internal.Interop.HRESULT s_OK = MS.Internal.Interop.HRESULT.S_OK;
			if (_taskbarList == null)
			{
				if (TaskbarItemInfo == null)
				{
					return;
				}
				ITaskbarList comObject = null;
				try
				{
					comObject = (ITaskbarList)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("56FDF344-FD6D-11d0-958A-006097C9A090")));
					s_OK = comObject.HrInit();
					if (s_OK != MS.Internal.Interop.HRESULT.S_OK)
					{
						HandleTaskbarListError(s_OK);
						return;
					}
					_taskbarList = (ITaskbarList3)comObject;
					comObject = null;
				}
				finally
				{
					Utilities.SafeRelease(ref comObject);
				}
				_overlaySize = new Size(MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.CXSMICON), MS.Win32.UnsafeNativeMethods.GetSystemMetrics(SM.CYSMICON));
				if (_taskbarRetryTimer == null)
				{
					_taskbarRetryTimer = new DispatcherTimer
					{
						Interval = new TimeSpan(0, 1, 0)
					};
					_taskbarRetryTimer.Tick += OnTaskbarRetryTimerTick;
				}
			}
			s_OK = RegisterTaskbarThumbButtons();
			if (s_OK.Succeeded)
			{
				s_OK = UpdateTaskbarProgressState();
			}
			if (s_OK.Succeeded)
			{
				s_OK = UpdateTaskbarOverlay();
			}
			if (s_OK.Succeeded)
			{
				s_OK = UpdateTaskbarDescription();
			}
			if (s_OK.Succeeded)
			{
				s_OK = UpdateTaskbarThumbnailClipping();
			}
			if (s_OK.Succeeded)
			{
				s_OK = UpdateTaskbarThumbButtons();
			}
			HandleTaskbarListError(s_OK);
		}
	}

	private MS.Internal.Interop.HRESULT UpdateTaskbarProgressState()
	{
		TaskbarItemInfo taskbarItemInfo = TaskbarItemInfo;
		TBPF tbpFlags = TBPF.NOPROGRESS;
		if (taskbarItemInfo != null)
		{
			tbpFlags = taskbarItemInfo.ProgressState switch
			{
				TaskbarItemProgressState.Error => TBPF.ERROR, 
				TaskbarItemProgressState.Indeterminate => TBPF.INDETERMINATE, 
				TaskbarItemProgressState.None => TBPF.NOPROGRESS, 
				TaskbarItemProgressState.Normal => TBPF.NORMAL, 
				TaskbarItemProgressState.Paused => TBPF.PAUSED, 
				_ => TBPF.NOPROGRESS, 
			};
		}
		MS.Internal.Interop.HRESULT result = _taskbarList.SetProgressState(CriticalHandle, tbpFlags);
		if (result.Succeeded)
		{
			result = UpdateTaskbarProgressValue();
		}
		return result;
	}

	private MS.Internal.Interop.HRESULT UpdateTaskbarProgressValue()
	{
		TaskbarItemInfo taskbarItemInfo = TaskbarItemInfo;
		if (taskbarItemInfo == null || taskbarItemInfo.ProgressState == TaskbarItemProgressState.None || taskbarItemInfo.ProgressState == TaskbarItemProgressState.Indeterminate)
		{
			return MS.Internal.Interop.HRESULT.S_OK;
		}
		ulong ullCompleted = (ulong)(taskbarItemInfo.ProgressValue * 1000.0);
		return _taskbarList.SetProgressValue(CriticalHandle, ullCompleted, 1000uL);
	}

	private MS.Internal.Interop.HRESULT UpdateTaskbarOverlay()
	{
		TaskbarItemInfo taskbarItemInfo = TaskbarItemInfo;
		MS.Win32.NativeMethods.IconHandle iconHandle = MS.Win32.NativeMethods.IconHandle.GetInvalidIcon();
		try
		{
			if (taskbarItemInfo != null && taskbarItemInfo.Overlay != null)
			{
				iconHandle = IconHelper.CreateIconHandleFromImageSource(taskbarItemInfo.Overlay, _overlaySize);
			}
			return _taskbarList.SetOverlayIcon(CriticalHandle, iconHandle, null);
		}
		finally
		{
			iconHandle.Dispose();
		}
	}

	private MS.Internal.Interop.HRESULT UpdateTaskbarDescription()
	{
		TaskbarItemInfo taskbarItemInfo = TaskbarItemInfo;
		string pszTip = "";
		if (taskbarItemInfo != null)
		{
			pszTip = taskbarItemInfo.Description ?? "";
		}
		return _taskbarList.SetThumbnailTooltip(CriticalHandle, pszTip);
	}

	private MS.Internal.Interop.HRESULT UpdateTaskbarThumbnailClipping()
	{
		if (_taskbarList == null)
		{
			return MS.Internal.Interop.HRESULT.S_OK;
		}
		if (_taskbarRetryTimer != null && _taskbarRetryTimer.IsEnabled)
		{
			return MS.Internal.Interop.HRESULT.S_FALSE;
		}
		if (MS.Win32.UnsafeNativeMethods.IsIconic(CriticalHandle))
		{
			return MS.Internal.Interop.HRESULT.S_FALSE;
		}
		TaskbarItemInfo taskbarItemInfo = TaskbarItemInfo;
		MS.Win32.NativeMethods.RefRECT prcClip = null;
		if (taskbarItemInfo != null && !taskbarItemInfo.ThumbnailClipMargin.IsZero)
		{
			Thickness thumbnailClipMargin = taskbarItemInfo.ThumbnailClipMargin;
			MS.Win32.NativeMethods.RECT rect = default(MS.Win32.NativeMethods.RECT);
			SafeNativeMethods.GetClientRect(new HandleRef(this, CriticalHandle), ref rect);
			Rect rect2 = new Rect(DeviceToLogicalUnits(new Point(rect.left, rect.top)), DeviceToLogicalUnits(new Point(rect.right, rect.bottom)));
			if (thumbnailClipMargin.Left + thumbnailClipMargin.Right >= rect2.Width || thumbnailClipMargin.Top + thumbnailClipMargin.Bottom >= rect2.Height)
			{
				prcClip = new MS.Win32.NativeMethods.RefRECT(0, 0, 0, 0);
			}
			else
			{
				Rect rect3 = new Rect(LogicalToDeviceUnits(new Point(thumbnailClipMargin.Left, thumbnailClipMargin.Top)), LogicalToDeviceUnits(new Point(rect2.Width - thumbnailClipMargin.Right, rect2.Height - thumbnailClipMargin.Bottom)));
				prcClip = new MS.Win32.NativeMethods.RefRECT((int)rect3.Left, (int)rect3.Top, (int)rect3.Right, (int)rect3.Bottom);
			}
		}
		return _taskbarList.SetThumbnailClip(CriticalHandle, prcClip);
	}

	private MS.Internal.Interop.HRESULT RegisterTaskbarThumbButtons()
	{
		THUMBBUTTON[] array = new THUMBBUTTON[7];
		for (int i = 0; i < 7; i++)
		{
			array[i] = new THUMBBUTTON
			{
				iId = (uint)i,
				dwFlags = (THBF.DISABLED | THBF.NOBACKGROUND | THBF.HIDDEN | THBF.NONINTERACTIVE),
				dwMask = (THB.ICON | THB.TOOLTIP | THB.FLAGS)
			};
		}
		MS.Internal.Interop.HRESULT hRESULT = _taskbarList.ThumbBarAddButtons(CriticalHandle, (uint)array.Length, array);
		if (hRESULT == MS.Internal.Interop.HRESULT.E_INVALIDARG)
		{
			hRESULT = MS.Internal.Interop.HRESULT.S_FALSE;
		}
		return hRESULT;
	}

	private MS.Internal.Interop.HRESULT UpdateTaskbarThumbButtons()
	{
		THUMBBUTTON[] array = new THUMBBUTTON[7];
		TaskbarItemInfo taskbarItemInfo = TaskbarItemInfo;
		ThumbButtonInfoCollection thumbButtonInfoCollection = null;
		if (taskbarItemInfo != null)
		{
			thumbButtonInfoCollection = taskbarItemInfo.ThumbButtonInfos;
		}
		List<MS.Win32.NativeMethods.IconHandle> list = new List<MS.Win32.NativeMethods.IconHandle>();
		try
		{
			uint num = 0u;
			if (thumbButtonInfoCollection != null)
			{
				foreach (ThumbButtonInfo item in thumbButtonInfoCollection)
				{
					THUMBBUTTON tHUMBBUTTON = default(THUMBBUTTON);
					tHUMBBUTTON.iId = num;
					tHUMBBUTTON.dwMask = THB.ICON | THB.TOOLTIP | THB.FLAGS;
					THUMBBUTTON tHUMBBUTTON2 = tHUMBBUTTON;
					switch (item.Visibility)
					{
					case Visibility.Collapsed:
						tHUMBBUTTON2.dwFlags = THBF.HIDDEN;
						break;
					case Visibility.Hidden:
						tHUMBBUTTON2.dwFlags = THBF.DISABLED | THBF.NOBACKGROUND;
						tHUMBBUTTON2.hIcon = IntPtr.Zero;
						break;
					default:
						tHUMBBUTTON2.szTip = item.Description ?? "";
						if (item.ImageSource != null)
						{
							MS.Win32.NativeMethods.IconHandle iconHandle = IconHelper.CreateIconHandleFromImageSource(item.ImageSource, _overlaySize);
							tHUMBBUTTON2.hIcon = iconHandle.CriticalGetHandle();
							list.Add(iconHandle);
						}
						if (!item.IsBackgroundVisible)
						{
							tHUMBBUTTON2.dwFlags |= THBF.NOBACKGROUND;
						}
						if (!item.IsEnabled)
						{
							tHUMBBUTTON2.dwFlags |= THBF.DISABLED;
						}
						else
						{
							tHUMBBUTTON2.dwFlags |= THBF.ENABLED;
						}
						if (!item.IsInteractive)
						{
							tHUMBBUTTON2.dwFlags |= THBF.NONINTERACTIVE;
						}
						if (item.DismissWhenClicked)
						{
							tHUMBBUTTON2.dwFlags |= THBF.DISMISSONCLICK;
						}
						break;
					}
					array[num] = tHUMBBUTTON2;
					num++;
					if (num == 7)
					{
						break;
					}
				}
			}
			for (; num < 7; num++)
			{
				array[num] = new THUMBBUTTON
				{
					iId = num,
					dwFlags = (THBF.DISABLED | THBF.NOBACKGROUND | THBF.HIDDEN),
					dwMask = (THB.ICON | THB.TOOLTIP | THB.FLAGS)
				};
			}
			return _taskbarList.ThumbBarUpdateButtons(CriticalHandle, (uint)array.Length, array);
		}
		finally
		{
			foreach (MS.Win32.NativeMethods.IconHandle item2 in list)
			{
				item2.Dispose();
			}
		}
	}

	private void CreateRtl()
	{
		if (base.FlowDirection == FlowDirection.LeftToRight)
		{
			_StyleEx &= -4194305;
			return;
		}
		if (base.FlowDirection == FlowDirection.RightToLeft)
		{
			_StyleEx |= 4194304;
			return;
		}
		throw new InvalidOperationException(SR.IncorrectFlowDirection);
	}

	internal void Flush()
	{
		HwndStyleManager manager = Manager;
		if (manager.Dirty && CriticalHandle != IntPtr.Zero)
		{
			MS.Win32.UnsafeNativeMethods.CriticalSetWindowLong(new HandleRef(this, CriticalHandle), -16, _styleDoNotUse.Value);
			MS.Win32.UnsafeNativeMethods.CriticalSetWindowLong(new HandleRef(this, CriticalHandle), -20, _styleExDoNotUse.Value);
			MS.Win32.UnsafeNativeMethods.SetWindowPos(new HandleRef(this, CriticalHandle), MS.Win32.NativeMethods.NullHandleRef, 0, 0, 0, 0, 55);
			manager.Dirty = false;
		}
	}

	private void ClearRootVisual()
	{
		if (_swh != null)
		{
			_swh.ClearRootVisual();
		}
	}

	private MS.Win32.NativeMethods.POINT GetPointRelativeToWindow(int x, int y)
	{
		return _swh.GetPointRelativeToWindow(x, y, base.FlowDirection);
	}

	private Size GetHwndNonClientAreaSizeInMeasureUnits()
	{
		if (!AllowsTransparency)
		{
			return _swh.GetHwndNonClientAreaSizeInMeasureUnits();
		}
		return new Size(0.0, 0.0);
	}

	private void ClearSourceWindow()
	{
		if (_swh == null)
		{
			return;
		}
		try
		{
			_swh.RemoveDisposedHandler(OnSourceWindowDisposed);
		}
		finally
		{
			HwndSource hwndSourceWindow = _swh.HwndSourceWindow;
			_swh = null;
			if (hwndSourceWindow != null)
			{
				hwndSourceWindow.SizeToContentChanged -= OnSourceSizeToContentChanged;
			}
		}
	}

	private void ClearHiddenWindowIfAny()
	{
		if (_hiddenWindow != null && _hiddenWindow.Handle == _ownerHandle)
		{
			SetOwnerHandle(IntPtr.Zero);
		}
	}

	private void VerifyConsistencyWithAllowsTransparency()
	{
		if (AllowsTransparency)
		{
			VerifyConsistencyWithAllowsTransparency(WindowStyle);
		}
	}

	private void VerifyConsistencyWithAllowsTransparency(WindowStyle style)
	{
		if (AllowsTransparency && style != 0)
		{
			throw new InvalidOperationException(SR.MustUseWindowStyleNone);
		}
	}

	private void VerifyConsistencyWithShowActivated()
	{
		if (!_inTrustedSubWindow && WindowState == WindowState.Maximized && !ShowActivated)
		{
			throw new InvalidOperationException(SR.ShowNonActivatedAndMaximized);
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

	private static bool IsValidResizeMode(ResizeMode value)
	{
		if (value != 0 && value != ResizeMode.CanMinimize && value != ResizeMode.CanResize)
		{
			return value == ResizeMode.CanResizeWithGrip;
		}
		return true;
	}

	private static bool IsValidWindowStartupLocation(WindowStartupLocation value)
	{
		if (value != WindowStartupLocation.CenterOwner && value != WindowStartupLocation.CenterScreen)
		{
			return value == WindowStartupLocation.Manual;
		}
		return true;
	}

	private static bool IsValidWindowState(WindowState value)
	{
		if (value != WindowState.Maximized && value != WindowState.Minimized)
		{
			return value == WindowState.Normal;
		}
		return true;
	}

	private static bool IsValidWindowStyle(WindowStyle value)
	{
		if (value != 0 && value != WindowStyle.SingleBorderWindow && value != WindowStyle.ThreeDBorderWindow)
		{
			return value == WindowStyle.ToolWindow;
		}
		return true;
	}

	/// <summary>Called when the <see cref="E:System.Windows.UIElement.ManipulationBoundaryFeedback" /> event occurs.</summary>
	/// <param name="e">The data for the event. </param>
	protected override void OnManipulationBoundaryFeedback(ManipulationBoundaryFeedbackEventArgs e)
	{
		base.OnManipulationBoundaryFeedback(e);
		if (PresentationSource.UnderSamePresentationSource(e.OriginalSource as DependencyObject, this) && !e.Handled)
		{
			if ((_currentPanningTarget == null || !_currentPanningTarget.IsAlive || _currentPanningTarget.Target != e.OriginalSource) && _swh != null)
			{
				MS.Win32.NativeMethods.RECT windowBounds = WindowBounds;
				_prePanningLocation = DeviceToLogicalUnits(new Point(windowBounds.left, windowBounds.top));
			}
			ManipulationDelta boundaryFeedback = e.BoundaryFeedback;
			UpdatePanningFeedback(boundaryFeedback.Translation, e.OriginalSource);
			e.CompensateForBoundaryFeedback = CompensateForPanningFeedback;
		}
	}

	private static void OnStaticManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
	{
		if (sender is Window window)
		{
			window.EndPanningFeedback(animateBack: true);
		}
	}

	private static void OnStaticManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
	{
		if (sender is Window window)
		{
			window.EndPanningFeedback(animateBack: false);
		}
	}

	private void UpdatePanningFeedback(Vector totalOverpanOffset, object originalSource)
	{
		if (_currentPanningTarget != null && !_currentPanningTarget.IsAlive)
		{
			_currentPanningTarget = null;
			EndPanningFeedback(animateBack: false);
		}
		if (_swh != null)
		{
			if (_currentPanningTarget == null)
			{
				_currentPanningTarget = new WeakReference(originalSource);
			}
			if (originalSource == _currentPanningTarget.Target)
			{
				_swh.UpdatePanningFeedback(totalOverpanOffset, animate: false);
			}
		}
	}

	private void EndPanningFeedback(bool animateBack)
	{
		if (_swh != null)
		{
			_swh.EndPanningFeedback(animateBack);
		}
		_currentPanningTarget = null;
		_prePanningLocation = new Point(double.NaN, double.NaN);
	}

	private Point CompensateForPanningFeedback(Point point)
	{
		if (!double.IsNaN(_prePanningLocation.X) && !double.IsNaN(_prePanningLocation.Y) && _swh != null)
		{
			MS.Win32.NativeMethods.RECT windowBounds = WindowBounds;
			Point point2 = DeviceToLogicalUnits(new Point(windowBounds.left, windowBounds.top));
			return new Point(point.X - (_prePanningLocation.X - point2.X), point.Y - (_prePanningLocation.Y - point2.Y));
		}
		return point;
	}
}
