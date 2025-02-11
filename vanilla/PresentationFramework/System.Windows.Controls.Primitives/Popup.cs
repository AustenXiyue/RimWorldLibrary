using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Accessibility;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.Interop;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;
using MS.Win32;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents a pop-up window that has content.</summary>
[DefaultEvent("Opened")]
[DefaultProperty("Child")]
[Localizability(LocalizationCategory.None)]
[ContentProperty("Child")]
public class Popup : FrameworkElement, IAddChild
{
	private class PopupModelTreeEnumerator : ModelTreeEnumerator
	{
		private Popup _popup;

		protected override bool IsUnchanged => base.Content == _popup.Child;

		internal PopupModelTreeEnumerator(Popup popup, object child)
			: base(child)
		{
			_popup = popup;
		}
	}

	private enum InterestPoint
	{
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
		Center
	}

	private struct PointCombination
	{
		public InterestPoint TargetInterestPoint;

		public InterestPoint ChildInterestPoint;

		public PointCombination(InterestPoint targetInterestPoint, InterestPoint childInterestPoint)
		{
			TargetInterestPoint = targetInterestPoint;
			ChildInterestPoint = childInterestPoint;
		}
	}

	private class PositionInfo
	{
		public int X;

		public int Y;

		public Size ChildSize;

		public Rect MouseRect = Rect.Empty;
	}

	private enum CacheBits
	{
		CaptureEngaged = 1,
		IsTransparent = 2,
		OnClosedHandlerReopen = 4,
		DropOppositeSet = 8,
		DropOpposite = 0x10,
		AnimateFromRight = 0x20,
		AnimateFromBottom = 0x40,
		HitTestable = 0x80,
		IsDragDropActive = 0x100,
		IsIgnoringMouseEvents = 0x200
	}

	private class PopupSecurityHelper
	{
		private bool _isChildPopup;

		private bool _isChildPopupInitialized;

		private SecurityCriticalDataClass<HwndSource> _window;

		private const string WebOCWindowClassName = "Shell Embedding";

		internal bool IsChildPopup
		{
			get
			{
				if (!_isChildPopupInitialized)
				{
					_isChildPopup = false;
					_isChildPopupInitialized = true;
				}
				return _isChildPopup;
			}
		}

		private nint Handle => GetHandle(_window.Value);

		private nint ParentHandle => GetParentHandle(_window.Value);

		internal PopupSecurityHelper()
		{
		}

		internal bool IsWindowAlive()
		{
			if (_window != null)
			{
				HwndSource value = _window.Value;
				if (value != null)
				{
					return !value.IsDisposed;
				}
				return false;
			}
			return false;
		}

		internal Point ClientToScreen(Visual rootVisual, Point clientPoint)
		{
			if (GetPresentationSource(rootVisual) is HwndSource hwnd)
			{
				return PointUtil.ToPoint(ClientToScreen(hwnd, clientPoint));
			}
			return clientPoint;
		}

		private MS.Win32.NativeMethods.POINT ClientToScreen(HwndSource hwnd, Point clientPt)
		{
			bool isChildPopup = IsChildPopup;
			HwndSource hwndSource = null;
			if (isChildPopup)
			{
				hwndSource = HwndSource.CriticalFromHwnd(ParentHandle);
			}
			Point pointScreen = clientPt;
			if (!isChildPopup || hwndSource != hwnd)
			{
				pointScreen = PointUtil.ClientToScreen(clientPt, hwnd);
			}
			if (isChildPopup && hwndSource != hwnd)
			{
				pointScreen = PointUtil.ScreenToClient(pointScreen, hwndSource);
			}
			return new MS.Win32.NativeMethods.POINT((int)pointScreen.X, (int)pointScreen.Y);
		}

		internal MS.Win32.NativeMethods.POINT GetMouseCursorPos(Visual targetVisual)
		{
			if (Mouse.DirectlyOver != null)
			{
				HwndSource hwndSource = null;
				if (targetVisual != null)
				{
					hwndSource = GetPresentationSource(targetVisual) as HwndSource;
				}
				if (targetVisual is IInputElement relativeTo)
				{
					Point result = Mouse.GetPosition(relativeTo);
					if (hwndSource != null && !hwndSource.IsDisposed)
					{
						Visual rootVisual = hwndSource.RootVisual;
						CompositionTarget compositionTarget = hwndSource.CompositionTarget;
						if (rootVisual != null && compositionTarget != null)
						{
							GeneralTransform generalTransform = targetVisual.TransformToAncestor(rootVisual);
							Matrix matrix = PointUtil.GetVisualTransform(rootVisual) * compositionTarget.TransformToDevice;
							generalTransform.TryTransform(result, out result);
							result = matrix.Transform(result);
							return ClientToScreen(hwndSource, result);
						}
					}
				}
			}
			MS.Win32.NativeMethods.POINT pt = new MS.Win32.NativeMethods.POINT(0, 0);
			MS.Win32.UnsafeNativeMethods.TryGetCursorPos(ref pt);
			return pt;
		}

		internal void SetPopupPos(bool position, int x, int y, bool size, int width, int height)
		{
			int num = 20;
			if (!position)
			{
				num |= 2;
			}
			if (!size)
			{
				num |= 1;
			}
			MS.Win32.UnsafeNativeMethods.SetWindowPos(new HandleRef(null, Handle), new HandleRef(null, IntPtr.Zero), x, y, width, height, num);
		}

		internal Rect GetParentWindowRect()
		{
			MS.Win32.NativeMethods.RECT rect = new MS.Win32.NativeMethods.RECT(0, 0, 0, 0);
			nint parentHandle = ParentHandle;
			if (parentHandle != IntPtr.Zero)
			{
				SafeNativeMethods.GetClientRect(new HandleRef(null, parentHandle), ref rect);
			}
			return PointUtil.ToRect(rect);
		}

		internal Rect GetWindowRect()
		{
			MS.Win32.NativeMethods.RECT rect = new MS.Win32.NativeMethods.RECT(0, 0, 0, 0);
			nint handle = Handle;
			if (handle != IntPtr.Zero)
			{
				SafeNativeMethods.GetWindowRect(new HandleRef(null, handle), ref rect);
			}
			return PointUtil.ToRect(rect);
		}

		internal Matrix GetTransformToDevice()
		{
			CompositionTarget compositionTarget = _window.Value.CompositionTarget;
			if (compositionTarget != null && !compositionTarget.IsDisposed)
			{
				return compositionTarget.TransformToDevice;
			}
			return Matrix.Identity;
		}

		internal static Matrix GetTransformToDevice(Visual targetVisual)
		{
			HwndSource hwndSource = null;
			if (targetVisual != null)
			{
				hwndSource = GetPresentationSource(targetVisual) as HwndSource;
			}
			if (hwndSource != null)
			{
				CompositionTarget compositionTarget = hwndSource.CompositionTarget;
				if (compositionTarget != null && !compositionTarget.IsDisposed)
				{
					return compositionTarget.TransformToDevice;
				}
			}
			return Matrix.Identity;
		}

		internal Matrix GetTransformFromDevice()
		{
			CompositionTarget compositionTarget = _window.Value.CompositionTarget;
			if (compositionTarget != null && !compositionTarget.IsDisposed)
			{
				return compositionTarget.TransformFromDevice;
			}
			return Matrix.Identity;
		}

		internal void SetWindowRootVisual(Visual v)
		{
			_window.Value.RootVisual = v;
		}

		internal static bool IsVisualPresentationSourceNull(Visual visual)
		{
			return GetPresentationSource(visual) == null;
		}

		internal void ShowWindow()
		{
			if (IsChildPopup)
			{
				nint lastWebOCHwnd = GetLastWebOCHwnd();
				MS.Win32.UnsafeNativeMethods.SetWindowPos(new HandleRef(null, Handle), (lastWebOCHwnd == IntPtr.Zero) ? MS.Win32.NativeMethods.HWND_TOP : new HandleRef(null, lastWebOCHwnd), 0, 0, 0, 0, 83);
			}
			else if (!FrameworkCompatibilityPreferences.GetUseSetWindowPosForTopmostWindows())
			{
				MS.Win32.UnsafeNativeMethods.ShowWindow(new HandleRef(null, Handle), 8);
			}
			else
			{
				MS.Win32.UnsafeNativeMethods.SetWindowPos(new HandleRef(null, Handle), MS.Win32.NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, 595);
			}
		}

		internal void HideWindow()
		{
			MS.Win32.UnsafeNativeMethods.ShowWindow(new HandleRef(null, Handle), 0);
		}

		private nint GetLastWebOCHwnd()
		{
			nint window = MS.Win32.UnsafeNativeMethods.GetWindow(new HandleRef(null, Handle), 1);
			StringBuilder stringBuilder = new StringBuilder(260);
			while (window != IntPtr.Zero)
			{
				if (MS.Win32.UnsafeNativeMethods.GetClassName(new HandleRef(null, window), stringBuilder, 260) != 0)
				{
					if (string.Compare(stringBuilder.ToString(), "Shell Embedding", StringComparison.OrdinalIgnoreCase) == 0)
					{
						break;
					}
					window = MS.Win32.UnsafeNativeMethods.GetWindow(new HandleRef(null, window), 3);
					continue;
				}
				throw new Win32Exception();
			}
			return window;
		}

		internal void SetHitTestable(bool hitTestable)
		{
			nint handle = Handle;
			int windowLong = MS.Win32.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, handle), -20);
			int num = windowLong;
			if ((num & 0x20) == 0 != hitTestable)
			{
				windowLong = ((!hitTestable) ? (num | 0x20) : (num & -33));
				MS.Win32.UnsafeNativeMethods.CriticalSetWindowLong(new HandleRef(null, handle), -20, windowLong);
			}
		}

		private static Visual FindMainTreeVisual(Visual v)
		{
			DependencyObject dependencyObject = null;
			DependencyObject dependencyObject2 = v;
			while (dependencyObject2 != null)
			{
				dependencyObject = dependencyObject2;
				if (dependencyObject2 is PopupRoot popupRoot)
				{
					dependencyObject2 = popupRoot.Parent;
					if (dependencyObject2 is Popup { PlacementTarget: { } placementTarget })
					{
						dependencyObject2 = placementTarget;
					}
				}
				else
				{
					dependencyObject2 = VisualTreeHelper.GetParent(dependencyObject2);
				}
			}
			return dependencyObject as Visual;
		}

		internal void BuildWindow(int x, int y, Visual placementTarget, bool transparent, HwndSourceHook hook, AutoResizedEventHandler handler, HwndDpiChangedEventHandler dpiChangedHandler)
		{
			transparent = transparent && !IsChildPopup;
			Visual visual = placementTarget;
			if (IsChildPopup)
			{
				visual = FindMainTreeVisual(placementTarget);
			}
			HwndSource hwndSource = GetPresentationSource(visual) as HwndSource;
			nint num = IntPtr.Zero;
			if (hwndSource != null)
			{
				num = GetHandle(hwndSource);
			}
			int windowClassStyle = 0;
			int num2 = 67108864;
			int num3 = 134217856;
			if (IsChildPopup)
			{
				num2 |= 0x40000000;
			}
			else
			{
				num2 |= int.MinValue;
				num3 |= 8;
			}
			HwndSourceParameters parameters = new HwndSourceParameters(string.Empty);
			parameters.WindowClassStyle = windowClassStyle;
			parameters.WindowStyle = num2;
			parameters.ExtendedWindowStyle = num3;
			parameters.SetPosition(x, y);
			if (IsChildPopup)
			{
				if (num != IntPtr.Zero)
				{
					parameters.ParentWindow = num;
				}
			}
			else
			{
				parameters.UsesPerPixelOpacity = transparent;
				if (num != IntPtr.Zero && ConnectedToForegroundWindow(num))
				{
					parameters.ParentWindow = num;
				}
			}
			HwndSource hwndSource2 = new HwndSource(parameters);
			hwndSource2.AddHook(hook);
			_window = new SecurityCriticalDataClass<HwndSource>(hwndSource2);
			hwndSource2.CompositionTarget.BackgroundColor = (transparent ? Colors.Transparent : Colors.Black);
			hwndSource2.AutoResized += handler;
			hwndSource2.DpiChanged += dpiChangedHandler;
		}

		private static bool ConnectedToForegroundWindow(nint window)
		{
			nint foregroundWindow = MS.Win32.UnsafeNativeMethods.GetForegroundWindow();
			while (window != IntPtr.Zero)
			{
				if (window == foregroundWindow)
				{
					return true;
				}
				window = MS.Win32.UnsafeNativeMethods.GetParent(new HandleRef(null, window));
			}
			return false;
		}

		private static nint GetHandle(HwndSource hwnd)
		{
			return hwnd?.CriticalHandle ?? IntPtr.Zero;
		}

		private static nint GetParentHandle(HwndSource hwnd)
		{
			if (hwnd != null)
			{
				nint handle = GetHandle(hwnd);
				if (handle != IntPtr.Zero)
				{
					return MS.Win32.UnsafeNativeMethods.GetParent(new HandleRef(null, handle));
				}
			}
			return IntPtr.Zero;
		}

		private static PresentationSource GetPresentationSource(Visual visual)
		{
			if (visual == null)
			{
				return null;
			}
			return PresentationSource.CriticalFromVisual(visual);
		}

		internal void ForceMsaaToUiaBridge(PopupRoot popupRoot)
		{
			if (Handle == IntPtr.Zero || (!MS.Win32.UnsafeNativeMethods.IsWinEventHookInstalled(32773) && !MS.Win32.UnsafeNativeMethods.IsWinEventHookInstalled(32778)) || !(UIElementAutomationPeer.CreatePeerForElement(popupRoot) is PopupRootAutomationPeer popupRootAutomationPeer))
			{
				return;
			}
			if (popupRootAutomationPeer.Hwnd == IntPtr.Zero)
			{
				popupRootAutomationPeer.Hwnd = Handle;
			}
			IRawElementProviderSimple el = popupRootAutomationPeer.ProviderFromPeer(popupRootAutomationPeer);
			nint num = AutomationInteropProvider.ReturnRawElementProvider(Handle, IntPtr.Zero, new IntPtr(-4), el);
			if (num != IntPtr.Zero)
			{
				IAccessible ppvObject = null;
				Guid iid = new Guid("618736e0-3c3d-11cf-810c-00aa00389b71");
				if (MS.Win32.UnsafeNativeMethods.ObjectFromLresult(num, ref iid, IntPtr.Zero, ref ppvObject) != 0)
				{
				}
			}
		}

		internal void DestroyWindow(HwndSourceHook hook, AutoResizedEventHandler onAutoResizedEventHandler, HwndDpiChangedEventHandler onDpiChagnedEventHandler)
		{
			HwndSource value = _window.Value;
			_window = null;
			if (!value.IsDisposed)
			{
				value.AutoResized -= onAutoResizedEventHandler;
				value.DpiChanged -= onDpiChagnedEventHandler;
				value.RemoveHook(hook);
				value.RootVisual = null;
				value.Dispose();
			}
		}
	}

	private static class PopupInitialPlacementHelper
	{
		internal static bool IsPerMonitorDpiScalingActive
		{
			get
			{
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

		private static MS.Win32.NativeMethods.POINT? GetPlacementTargetOriginInScreenCoordinates(Popup popup)
		{
			if (popup?.GetTarget() is UIElement uIElement)
			{
				Visual rootVisual = GetRootVisual(uIElement);
				if (TransformToClient(uIElement, rootVisual).TryTransform(new Point(0.0, 0.0), out var result))
				{
					Point point = popup._secHelper.ClientToScreen(rootVisual, result);
					return new MS.Win32.NativeMethods.POINT((int)point.X, (int)point.Y);
				}
			}
			return null;
		}

		internal static MS.Win32.NativeMethods.POINT GetPlacementOrigin(Popup popup)
		{
			MS.Win32.NativeMethods.POINT result = default(MS.Win32.NativeMethods.POINT);
			if (IsPerMonitorDpiScalingActive)
			{
				MS.Win32.NativeMethods.POINT? placementTargetOriginInScreenCoordinates = GetPlacementTargetOriginInScreenCoordinates(popup);
				if (placementTargetOriginInScreenCoordinates.HasValue)
				{
					try
					{
						nint handle = SafeNativeMethods.MonitorFromPoint(placementTargetOriginInScreenCoordinates.Value, 2);
						MS.Win32.NativeMethods.MONITORINFOEX mONITORINFOEX = new MS.Win32.NativeMethods.MONITORINFOEX();
						SafeNativeMethods.GetMonitorInfo(new HandleRef(null, handle), mONITORINFOEX);
						result.x = mONITORINFOEX.rcMonitor.left;
						result.y = mONITORINFOEX.rcMonitor.top;
					}
					catch (Win32Exception)
					{
					}
				}
			}
			return result;
		}
	}

	internal static readonly DependencyProperty TreatMousePlacementAsBottomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Popup.Child" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Popup.Child" /> dependency property.</returns>
	public static readonly DependencyProperty ChildProperty;

	internal static readonly UncommonField<List<Popup>> RegisteredPopupsField;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Popup.IsOpen" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Popup.IsOpen" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty IsOpenProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Popup.Placement" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Popup.Placement" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty PlacementProperty;

	/// <summary>Identifies the <see cref="T:System.Windows.Controls.Primitives.CustomPopupPlacementCallback" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="T:System.Windows.Controls.Primitives.CustomPopupPlacementCallback" /> dependency property.</returns>
	public static readonly DependencyProperty CustomPopupPlacementCallbackProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Popup.StaysOpen" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Popup.StaysOpen" /> dependency property.</returns>
	public static readonly DependencyProperty StaysOpenProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Popup.HorizontalOffset" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Popup.HorizontalOffset" /> dependency property.</returns>
	public static readonly DependencyProperty HorizontalOffsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Popup.VerticalOffset" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Popup.VerticalOffset" /> dependency property.</returns>
	public static readonly DependencyProperty VerticalOffsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Popup.PlacementTarget" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Popup.PlacementTarget" /> dependency property.</returns>
	public static readonly DependencyProperty PlacementTargetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Popup.PlacementRectangle" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Popup.PlacementRectangle" /> dependency property.</returns>
	public static readonly DependencyProperty PlacementRectangleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Popup.PopupAnimation" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Popup.PopupAnimation" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty PopupAnimationProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Popup.AllowsTransparency" /> dependency property. </summary>
	public static readonly DependencyProperty AllowsTransparencyProperty;

	private static readonly DependencyPropertyKey HasDropShadowPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Popup.HasDropShadow" /> dependency property. </summary>
	public static readonly DependencyProperty HasDropShadowProperty;

	private static readonly EventPrivateKey OpenedKey;

	private static readonly EventPrivateKey ClosedKey;

	private static readonly UncommonField<Exception> SavedExceptionField;

	internal const double Tolerance = 0.01;

	private const int AnimationDelay = 150;

	internal static TimeSpan AnimationDelayTime;

	internal static RoutedEventHandler CloseOnUnloadedHandler;

	private static readonly UncommonField<PopupRoot> ParentPopupRootField;

	private PositionInfo _positionInfo;

	private MS.Internal.SecurityCriticalDataForSet<PopupRoot> _popupRoot;

	private DispatcherOperation _asyncCreate;

	private DispatcherTimer _asyncDestroy;

	private PopupSecurityHelper _secHelper;

	private BitVector32 _cacheValid = new BitVector32(0);

	private const double RestrictPercentage = 0.75;

	internal bool TreatMousePlacementAsBottom
	{
		get
		{
			return (bool)GetValue(TreatMousePlacementAsBottomProperty);
		}
		set
		{
			SetValue(TreatMousePlacementAsBottomProperty, value);
		}
	}

	/// <summary>Gets or sets the content of the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control.  </summary>
	/// <returns>The <see cref="T:System.Windows.UIElement" /> content of the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control. The default is null.</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	public UIElement Child
	{
		get
		{
			return (UIElement)GetValue(ChildProperty);
		}
		set
		{
			SetValue(ChildProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.Primitives.Popup" /> is visible.  </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.Primitives.Popup" /> is visible; otherwise, false. The default is false.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public bool IsOpen
	{
		get
		{
			return (bool)GetValue(IsOpenProperty);
		}
		set
		{
			SetValue(IsOpenProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets the orientation of the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control when the control opens, and specifies the behavior of the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control when it overlaps screen boundaries.  </summary>
	/// <returns>A <see cref="T:System.Windows.Controls.Primitives.PlacementMode" /> enumeration value that determines the orientation of the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control when the control opens, and that specifies how the control interacts with screen boundaries. The default is <see cref="F:System.Windows.Controls.Primitives.PlacementMode.Bottom" />. </returns>
	[Bindable(true)]
	[Category("Layout")]
	public PlacementMode Placement
	{
		get
		{
			return (PlacementMode)GetValue(PlacementProperty);
		}
		set
		{
			SetValue(PlacementProperty, value);
		}
	}

	internal PlacementMode PlacementInternal
	{
		get
		{
			PlacementMode placementMode = Placement;
			if ((placementMode == PlacementMode.Mouse || placementMode == PlacementMode.MousePoint) && TreatMousePlacementAsBottom)
			{
				placementMode = PlacementMode.Bottom;
			}
			return placementMode;
		}
	}

	/// <summary>Gets or sets a delegate handler method that positions the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control.  </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.Primitives.CustomPopupPlacementCallback" /> delegate method that provides placement information for the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control. The default is null.</returns>
	[Bindable(false)]
	[Category("Layout")]
	public CustomPopupPlacementCallback CustomPopupPlacementCallback
	{
		get
		{
			return (CustomPopupPlacementCallback)GetValue(CustomPopupPlacementCallbackProperty);
		}
		set
		{
			SetValue(CustomPopupPlacementCallbackProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control closes when the control is no longer in focus.  </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control closes when <see cref="P:System.Windows.Controls.Primitives.Popup.IsOpen" /> property is set to false; false if the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control closes when a mouse or keyboard event occurs outside the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control. The default is true.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public bool StaysOpen
	{
		get
		{
			return (bool)GetValue(StaysOpenProperty);
		}
		set
		{
			SetValue(StaysOpenProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Get or sets the horizontal distance between the target origin and the popup alignment point. </summary>
	/// <returns>The horizontal distance between the target origin and the popup alignment point. For information about the target origin and popup alignment point, see Popup Placement Behavior. The default is 0.</returns>
	[Bindable(true)]
	[Category("Layout")]
	[TypeConverter(typeof(LengthConverter))]
	public double HorizontalOffset
	{
		get
		{
			return (double)GetValue(HorizontalOffsetProperty);
		}
		set
		{
			SetValue(HorizontalOffsetProperty, value);
		}
	}

	/// <summary>Gets or sets the vertical distance between the target origin and the popup alignment point.  </summary>
	/// <returns>The vertical distance between the target origin and the popup alignment point. For information about the target origin and popup alignment point, see Popup Placement Behavior. The default is 0.</returns>
	[Bindable(true)]
	[Category("Layout")]
	[TypeConverter(typeof(LengthConverter))]
	public double VerticalOffset
	{
		get
		{
			return (double)GetValue(VerticalOffsetProperty);
		}
		set
		{
			SetValue(VerticalOffsetProperty, value);
		}
	}

	/// <summary>Gets or sets the element relative to which the <see cref="T:System.Windows.Controls.Primitives.Popup" /> is positioned when it opens.  </summary>
	/// <returns>The <see cref="T:System.Windows.UIElement" /> that is the logical parent of the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control. The default is null.</returns>
	[Bindable(true)]
	[Category("Layout")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public UIElement PlacementTarget
	{
		get
		{
			return (UIElement)GetValue(PlacementTargetProperty);
		}
		set
		{
			SetValue(PlacementTargetProperty, value);
		}
	}

	/// <summary>Gets or sets the rectangle relative to which the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control is positioned when it opens.  </summary>
	/// <returns>The rectangle that is used to position the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control. The default is null.</returns>
	[Bindable(true)]
	[Category("Layout")]
	public Rect PlacementRectangle
	{
		get
		{
			return (Rect)GetValue(PlacementRectangleProperty);
		}
		set
		{
			SetValue(PlacementRectangleProperty, value);
		}
	}

	internal bool DropOpposite
	{
		get
		{
			bool result = false;
			if (_cacheValid[8])
			{
				result = _cacheValid[16];
			}
			else
			{
				DependencyObject dependencyObject = this;
				do
				{
					dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
					if (dependencyObject is PopupRoot popupRoot)
					{
						Popup popup = popupRoot.Parent as Popup;
						dependencyObject = popup;
						if (popup != null && popup._cacheValid[8])
						{
							result = popup._cacheValid[16];
							break;
						}
					}
				}
				while (dependencyObject != null);
			}
			return result;
		}
		set
		{
			_cacheValid[16] = value;
			_cacheValid[8] = true;
		}
	}

	/// <summary>Gets or sets an animation for the opening and closing of a <see cref="T:System.Windows.Controls.Primitives.Popup" /> control.  </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.Primitives.PopupAnimation" /> enumeration value that defines an animation to open and close a <see cref="T:System.Windows.Controls.Primitives.Popup" /> control. The default is <see cref="F:System.Windows.Controls.Primitives.PopupAnimation.None" />.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public PopupAnimation PopupAnimation
	{
		get
		{
			return (PopupAnimation)GetValue(PopupAnimationProperty);
		}
		set
		{
			SetValue(PopupAnimationProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a <see cref="T:System.Windows.Controls.Primitives.Popup" /> control can contain transparent content.  </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control can contain transparent content; otherwise, false. The default is false.</returns>
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

	/// <summary>Gets a value that indicates whether a <see cref="T:System.Windows.Controls.Primitives.Popup" /> is displayed with a drop shadow effect.  </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.Primitives.Popup" /> is displayed with a drop shadow; otherwise, false.</returns>
	public bool HasDropShadow => (bool)GetValue(HasDropShadowProperty);

	/// <summary>Gets an enumerator that you can use to access the logical child elements of the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that you can use to access the logical child elements of a <see cref="T:System.Windows.Controls.Primitives.Popup" /> control. The default is null.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			object child = Child;
			if (child == null)
			{
				return EmptyEnumerator.Instance;
			}
			return new PopupModelTreeEnumerator(this, child);
		}
	}

	internal Exception SavedException
	{
		get
		{
			return SavedExceptionField.GetValue(this);
		}
		set
		{
			SavedExceptionField.SetValue(this, value);
		}
	}

	private bool IsTransparent
	{
		get
		{
			return _cacheValid[2];
		}
		set
		{
			_cacheValid[2] = value;
		}
	}

	private bool AnimateFromRight
	{
		get
		{
			return _cacheValid[32];
		}
		set
		{
			_cacheValid[32] = value;
		}
	}

	private bool AnimateFromBottom
	{
		get
		{
			return _cacheValid[64];
		}
		set
		{
			_cacheValid[64] = value;
		}
	}

	internal bool HitTestable
	{
		get
		{
			return !_cacheValid[128];
		}
		set
		{
			_cacheValid[128] = !value;
		}
	}

	private bool IsDragDropActive
	{
		get
		{
			return _cacheValid[256];
		}
		set
		{
			_cacheValid[256] = value;
		}
	}

	internal override int EffectiveValuesInitialSize => 19;

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.Primitives.Popup.IsOpen" /> property changes to true. </summary>
	public event EventHandler Opened
	{
		add
		{
			EventHandlersStoreAdd(OpenedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(OpenedKey, value);
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.Primitives.Popup.IsOpen" /> property changes to false. </summary>
	public event EventHandler Closed
	{
		add
		{
			EventHandlersStoreAdd(ClosedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(ClosedKey, value);
		}
	}

	internal event EventHandler PopupCouldClose;

	static Popup()
	{
		TreatMousePlacementAsBottomProperty = DependencyProperty.Register("TreatMousePlacementAsBottom", typeof(bool), typeof(Popup), new FrameworkPropertyMetadata(false));
		ChildProperty = DependencyProperty.Register("Child", typeof(UIElement), typeof(Popup), new FrameworkPropertyMetadata(null, OnChildChanged));
		RegisteredPopupsField = new UncommonField<List<Popup>>();
		IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(Popup), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged, CoerceIsOpen));
		PlacementProperty = DependencyProperty.Register("Placement", typeof(PlacementMode), typeof(Popup), new FrameworkPropertyMetadata(PlacementMode.Bottom, OnPlacementChanged), IsValidPlacementMode);
		CustomPopupPlacementCallbackProperty = DependencyProperty.Register("CustomPopupPlacementCallback", typeof(CustomPopupPlacementCallback), typeof(Popup), new FrameworkPropertyMetadata((object)null));
		StaysOpenProperty = DependencyProperty.Register("StaysOpen", typeof(bool), typeof(Popup), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, OnStaysOpenChanged));
		HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(Popup), new FrameworkPropertyMetadata(0.0, OnOffsetChanged));
		VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(double), typeof(Popup), new FrameworkPropertyMetadata(0.0, OnOffsetChanged));
		PlacementTargetProperty = DependencyProperty.Register("PlacementTarget", typeof(UIElement), typeof(Popup), new FrameworkPropertyMetadata(null, OnPlacementTargetChanged));
		PlacementRectangleProperty = DependencyProperty.Register("PlacementRectangle", typeof(Rect), typeof(Popup), new FrameworkPropertyMetadata(Rect.Empty, OnOffsetChanged));
		PopupAnimationProperty = DependencyProperty.Register("PopupAnimation", typeof(PopupAnimation), typeof(Popup), new FrameworkPropertyMetadata(PopupAnimation.None, null, CoercePopupAnimation), IsValidPopupAnimation);
		AllowsTransparencyProperty = Window.AllowsTransparencyProperty.AddOwner(typeof(Popup), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, OnAllowsTransparencyChanged, CoerceAllowsTransparency));
		HasDropShadowPropertyKey = DependencyProperty.RegisterReadOnly("HasDropShadow", typeof(bool), typeof(Popup), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, null, CoerceHasDropShadow));
		HasDropShadowProperty = HasDropShadowPropertyKey.DependencyProperty;
		OpenedKey = new EventPrivateKey();
		ClosedKey = new EventPrivateKey();
		SavedExceptionField = new UncommonField<Exception>();
		AnimationDelayTime = new TimeSpan(0, 0, 0, 0, 150);
		ParentPopupRootField = new UncommonField<PopupRoot>();
		EventManager.RegisterClassHandler(typeof(Popup), Mouse.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));
		EventManager.RegisterClassHandler(typeof(Popup), DragDrop.DragDropStartedEvent, new RoutedEventHandler(OnDragDropStarted), handledEventsToo: true);
		EventManager.RegisterClassHandler(typeof(Popup), DragDrop.DragDropCompletedEvent, new RoutedEventHandler(OnDragDropCompleted), handledEventsToo: true);
		UIElement.VisibilityProperty.OverrideMetadata(typeof(Popup), new FrameworkPropertyMetadata(VisibilityBoxes.CollapsedBox, null, CoerceVisibility));
	}

	private static object CoerceVisibility(DependencyObject d, object value)
	{
		return VisibilityBoxes.CollapsedBox;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.Popup" /> class. </summary>
	public Popup()
	{
		_secHelper = new PopupSecurityHelper();
	}

	private static void OnChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Popup popup = (Popup)d;
		UIElement child = (UIElement)e.OldValue;
		UIElement child2 = (UIElement)e.NewValue;
		if (popup._popupRoot.Value != null && (popup.IsOpen || popup._popupRoot.Value.Child != null))
		{
			popup._popupRoot.Value.Child = child2;
		}
		popup.RemoveLogicalChild(child);
		popup.AddLogicalChild(child2);
		popup.Reposition();
		popup.pushTextRenderingMode();
	}

	internal override void pushTextRenderingMode()
	{
		if (Child != null && DependencyPropertyHelper.GetValueSource(Child, TextOptions.TextRenderingModeProperty).BaseValueSource <= BaseValueSource.Inherited)
		{
			Child.VisualTextRenderingMode = TextOptions.GetTextRenderingMode(this);
		}
	}

	private static void RegisterPopupWithPlacementTarget(Popup popup, UIElement placementTarget)
	{
		List<Popup> value = RegisteredPopupsField.GetValue(placementTarget);
		if (value == null)
		{
			value = new List<Popup> { popup };
			RegisteredPopupsField.SetValue(placementTarget, value);
		}
		else if (!value.Contains(popup))
		{
			value.Add(popup);
		}
	}

	private static void UnregisterPopupFromPlacementTarget(Popup popup, UIElement placementTarget)
	{
		List<Popup> value = RegisteredPopupsField.GetValue(placementTarget);
		if (value != null)
		{
			value.Remove(popup);
			if (value.Count == 0)
			{
				RegisteredPopupsField.SetValue(placementTarget, null);
			}
		}
	}

	private void UpdatePlacementTargetRegistration(UIElement oldValue, UIElement newValue)
	{
		if (oldValue != null)
		{
			UnregisterPopupFromPlacementTarget(this, oldValue);
			if (newValue == null && VisualTreeHelper.GetParent(this) == null)
			{
				TreeWalkHelper.InvalidateOnTreeChange(this, null, oldValue, isAddOperation: false);
			}
		}
		if (newValue != null && VisualTreeHelper.GetParent(this) == null)
		{
			RegisterPopupWithPlacementTarget(this, newValue);
			if (!base.IsSelfInheritanceParent)
			{
				SetIsSelfInheritanceParent();
			}
			TreeWalkHelper.InvalidateOnTreeChange(this, null, newValue, isAddOperation: true);
		}
	}

	private static object CoerceIsOpen(DependencyObject d, object value)
	{
		if ((bool)value)
		{
			Popup popup = (Popup)d;
			if (!popup.IsLoaded && VisualTreeHelper.GetParent(popup) != null)
			{
				popup.RegisterToOpenOnLoad();
				return BooleanBoxes.FalseBox;
			}
		}
		return value;
	}

	private void RegisterToOpenOnLoad()
	{
		base.Loaded += OpenOnLoad;
	}

	private void OpenOnLoad(object sender, RoutedEventArgs e)
	{
		base.Dispatcher.BeginInvoke(DispatcherPriority.Input, (DispatcherOperationCallback)delegate
		{
			CoerceValue(IsOpenProperty);
			return (object)null;
		}, null);
	}

	private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Popup popup = (Popup)d;
		bool flag = (popup._secHelper.IsWindowAlive() && popup._asyncDestroy == null) || popup._asyncCreate != null;
		bool flag2 = (bool)e.NewValue;
		if (flag2 == flag)
		{
			return;
		}
		if (flag2)
		{
			if (popup._cacheValid[4])
			{
				throw new InvalidOperationException(SR.PopupReopeningNotAllowed);
			}
			popup.CancelAsyncDestroy();
			popup.CancelAsyncCreate();
			popup.CreateWindow(asyncCall: false);
			if (popup._secHelper.IsWindowAlive())
			{
				if (CloseOnUnloadedHandler == null)
				{
					CloseOnUnloadedHandler = CloseOnUnloaded;
				}
				popup.Unloaded += CloseOnUnloadedHandler;
			}
			return;
		}
		popup.CancelAsyncCreate();
		if (popup._secHelper.IsWindowAlive() && popup._asyncDestroy == null)
		{
			popup.HideWindow();
			if (CloseOnUnloadedHandler != null)
			{
				popup.Unloaded -= CloseOnUnloadedHandler;
			}
		}
	}

	/// <summary>Responds to the condition in which the value of the <see cref="P:System.Windows.Controls.Primitives.Popup.IsOpen" /> property changes from false to true. </summary>
	/// <param name="e">The event arguments.</param>
	protected virtual void OnOpened(EventArgs e)
	{
		RaiseClrEvent(OpenedKey, e);
	}

	/// <summary>Responds when the value of the <see cref="P:System.Windows.Controls.Primitives.Popup.IsOpen" /> property changes from to true to false.</summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnClosed(EventArgs e)
	{
		_cacheValid[4] = true;
		try
		{
			RaiseClrEvent(ClosedKey, e);
		}
		finally
		{
			_cacheValid[4] = false;
		}
	}

	private static void CloseOnUnloaded(object sender, RoutedEventArgs e)
	{
		((Popup)sender).SetCurrentValueInternal(IsOpenProperty, BooleanBoxes.FalseBox);
	}

	private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Popup)d).Reposition();
	}

	private static bool IsValidPlacementMode(object o)
	{
		PlacementMode placementMode = (PlacementMode)o;
		if (placementMode != 0 && placementMode != PlacementMode.AbsolutePoint && placementMode != PlacementMode.Bottom && placementMode != PlacementMode.Center && placementMode != PlacementMode.Mouse && placementMode != PlacementMode.MousePoint && placementMode != PlacementMode.Relative && placementMode != PlacementMode.RelativePoint && placementMode != PlacementMode.Right && placementMode != PlacementMode.Left && placementMode != PlacementMode.Top)
		{
			return placementMode == PlacementMode.Custom;
		}
		return true;
	}

	private static void OnStaysOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Popup popup = (Popup)d;
		if (popup.IsOpen)
		{
			if ((bool)e.NewValue)
			{
				popup.ReleasePopupCapture();
			}
			else
			{
				popup.EstablishPopupCapture();
			}
		}
	}

	private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Popup)d).Reposition();
	}

	private static void OnPlacementTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Popup popup = (Popup)d;
		if (popup.IsOpen)
		{
			popup.UpdatePlacementTargetRegistration((UIElement)e.OldValue, (UIElement)e.NewValue);
		}
		else if (e.OldValue != null)
		{
			UnregisterPopupFromPlacementTarget(popup, (UIElement)e.OldValue);
		}
	}

	private void ClearDropOpposite()
	{
		_cacheValid[8] = false;
	}

	private static object CoercePopupAnimation(DependencyObject o, object value)
	{
		if (!((Popup)o).AllowsTransparency)
		{
			return PopupAnimation.None;
		}
		return value;
	}

	private static bool IsValidPopupAnimation(object o)
	{
		PopupAnimation popupAnimation = (PopupAnimation)o;
		if (popupAnimation != 0 && popupAnimation != PopupAnimation.Fade && popupAnimation != PopupAnimation.Slide)
		{
			return popupAnimation == PopupAnimation.Scroll;
		}
		return true;
	}

	private static void OnAllowsTransparencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		d.CoerceValue(PopupAnimationProperty);
	}

	private static object CoerceAllowsTransparency(DependencyObject d, object value)
	{
		if (!((Popup)d)._secHelper.IsChildPopup)
		{
			return value;
		}
		return BooleanBoxes.FalseBox;
	}

	private static object CoerceHasDropShadow(DependencyObject d, object value)
	{
		return BooleanBoxes.Box(SystemParameters.DropShadow && ((Popup)d).AllowsTransparency);
	}

	/// <summary>Attaches a child element to a <see cref="T:System.Windows.Controls.Primitives.Popup" /> control. </summary>
	/// <param name="popup">The <see cref="T:System.Windows.Controls.Primitives.Popup" /> to which to add child content. </param>
	/// <param name="child">The <see cref="T:System.Windows.UIElement" /> child content. </param>
	public static void CreateRootPopup(Popup popup, UIElement child)
	{
		CreateRootPopupInternal(popup, child, bindTreatMousePlacementAsBottomProperty: false);
	}

	internal static void CreateRootPopupInternal(Popup popup, UIElement child, bool bindTreatMousePlacementAsBottomProperty)
	{
		if (popup == null)
		{
			throw new ArgumentNullException("popup");
		}
		if (child == null)
		{
			throw new ArgumentNullException("child");
		}
		object obj = null;
		if ((obj = LogicalTreeHelper.GetParent(child)) != null)
		{
			throw new InvalidOperationException(SR.Format(SR.CreateRootPopup_ChildHasLogicalParent, child, obj));
		}
		if ((obj = VisualTreeHelper.GetParent(child)) != null)
		{
			throw new InvalidOperationException(SR.Format(SR.CreateRootPopup_ChildHasVisualParent, child, obj));
		}
		Binding binding = new Binding("PlacementTarget");
		binding.Mode = BindingMode.OneWay;
		binding.Source = child;
		popup.SetBinding(PlacementTargetProperty, binding);
		popup.Child = child;
		binding = new Binding("VerticalOffset");
		binding.Mode = BindingMode.OneWay;
		binding.Source = child;
		popup.SetBinding(VerticalOffsetProperty, binding);
		binding = new Binding("HorizontalOffset");
		binding.Mode = BindingMode.OneWay;
		binding.Source = child;
		popup.SetBinding(HorizontalOffsetProperty, binding);
		binding = new Binding("PlacementRectangle");
		binding.Mode = BindingMode.OneWay;
		binding.Source = child;
		popup.SetBinding(PlacementRectangleProperty, binding);
		binding = new Binding("Placement");
		binding.Mode = BindingMode.OneWay;
		binding.Source = child;
		popup.SetBinding(PlacementProperty, binding);
		binding = new Binding("StaysOpen");
		binding.Mode = BindingMode.OneWay;
		binding.Source = child;
		popup.SetBinding(StaysOpenProperty, binding);
		binding = new Binding("CustomPopupPlacementCallback");
		binding.Mode = BindingMode.OneWay;
		binding.Source = child;
		popup.SetBinding(CustomPopupPlacementCallbackProperty, binding);
		if (bindTreatMousePlacementAsBottomProperty)
		{
			binding = new Binding("FromKeyboard");
			binding.Mode = BindingMode.OneWay;
			binding.Source = child;
			popup.SetBinding(TreatMousePlacementAsBottomProperty, binding);
		}
		binding = new Binding("IsOpen");
		binding.Mode = BindingMode.OneWay;
		binding.Source = child;
		popup.SetBinding(IsOpenProperty, binding);
	}

	internal static bool IsRootedInPopup(Popup parentPopup, UIElement element)
	{
		object parent = LogicalTreeHelper.GetParent(element);
		if (parent == null && VisualTreeHelper.GetParent(element) != null)
		{
			return false;
		}
		if (parent != parentPopup)
		{
			return false;
		}
		return true;
	}

	private void FirePopupCouldClose()
	{
		if (this.PopupCouldClose != null)
		{
			this.PopupCouldClose(this, EventArgs.Empty);
		}
	}

	/// <summary>Determines the required size of the <see cref="T:System.Windows.Controls.Primitives.Popup" /> content within the visual tree of the logical parent.</summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> structure that has the <see cref="P:System.Windows.Size.Height" /> and <see cref="P:System.Windows.Size.Width" /> properties both equal to zero (0).</returns>
	/// <param name="availableSize">The available size that this element can give to the child. You can use infinity as a value to indicate that the element can size to whatever content is available.</param>
	protected override Size MeasureOverride(Size availableSize)
	{
		return default(Size);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.PreviewMouseLeftButtonDown" /> event.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		OnPreviewMouseButton(e);
		base.OnPreviewMouseLeftButtonDown(e);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.PreviewMouseRightButtonUp" /> event.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
	{
		base.OnPreviewMouseRightButtonDown(e);
		OnPreviewMouseButton(e);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.PreviewMouseLeftButtonUp" /> event.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		OnPreviewMouseButton(e);
		base.OnPreviewMouseLeftButtonUp(e);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.PreviewMouseRightButtonDown" /> event.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
	{
		base.OnPreviewMouseRightButtonUp(e);
		OnPreviewMouseButton(e);
	}

	private void OnPreviewMouseButton(MouseButtonEventArgs e)
	{
		if (_cacheValid[1] && !StaysOpen && !_cacheValid[512] && _popupRoot.Value != null && e.OriginalSource == _popupRoot.Value && _popupRoot.Value.InputHitTest(e.GetPosition(_popupRoot.Value)) == null)
		{
			SetCurrentValueInternal(IsOpenProperty, BooleanBoxes.FalseBox);
		}
		if (_cacheValid[512] && e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
		{
			_cacheValid[512] = false;
		}
	}

	private void EstablishPopupCapture(bool isRestoringCapture = false)
	{
		if (_cacheValid[1] || _popupRoot.Value == null || StaysOpen)
		{
			return;
		}
		IInputElement inputElement = Mouse.Captured;
		if (inputElement is PopupRoot value)
		{
			if (isRestoringCapture)
			{
				if (Mouse.LeftButton != 0 || Mouse.RightButton != 0)
				{
					_cacheValid[512] = true;
				}
			}
			else
			{
				ParentPopupRootField.SetValue(this, value);
			}
			inputElement = null;
		}
		if (inputElement == null)
		{
			Mouse.Capture(_popupRoot.Value, CaptureMode.SubTree);
			_cacheValid[1] = true;
		}
	}

	private void ReleasePopupCapture()
	{
		if (!_cacheValid[1])
		{
			return;
		}
		PopupRoot value = ParentPopupRootField.GetValue(this);
		ParentPopupRootField.ClearValue(this);
		if (Mouse.Captured == _popupRoot.Value)
		{
			if (value == null)
			{
				Mouse.Capture(null);
			}
			else if (value.Parent is Popup popup)
			{
				popup.EstablishPopupCapture(isRestoringCapture: true);
			}
		}
		_cacheValid[1] = false;
	}

	private static void OnLostMouseCapture(object sender, MouseEventArgs e)
	{
		Popup popup = sender as Popup;
		if (popup.StaysOpen)
		{
			return;
		}
		PopupRoot value = popup._popupRoot.Value;
		if (e.OriginalSource != value && Mouse.Captured == null && SafeNativeMethods.GetCapture() == IntPtr.Zero)
		{
			popup.EstablishPopupCapture();
			e.Handled = true;
			return;
		}
		if (Mouse.Captured != value)
		{
			popup._cacheValid[1] = false;
		}
		Popup popup2 = ((!(Mouse.Captured is PopupRoot popupRoot)) ? null : (popupRoot.Parent as Popup));
		if ((popup2 == null || value == null || value != ParentPopupRootField.GetValue(popup2)) && (Mouse.Captured == null || !MenuBase.IsDescendant(value, Mouse.Captured as DependencyObject)) && Mouse.Captured != value && !popup.IsDragDropActive)
		{
			popup.SetCurrentValueInternal(IsOpenProperty, BooleanBoxes.FalseBox);
		}
	}

	private static void OnDragDropStarted(object sender, RoutedEventArgs e)
	{
		((Popup)sender).IsDragDropActive = true;
	}

	private static void OnDragDropCompleted(object sender, RoutedEventArgs e)
	{
		Popup popup = (Popup)sender;
		popup.IsDragDropActive = false;
		if (!popup.StaysOpen)
		{
			popup.EstablishPopupCapture();
		}
	}

	/// <summary>This member supports the WPF infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="value">An object to add as a child. </param>
	void IAddChild.AddChild(object value)
	{
		UIElement uIElement = value as UIElement;
		if (uIElement == null && value != null)
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(UIElement)), "value");
		}
		Child = uIElement;
	}

	/// <summary>This member supports the WPF infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="text">A string to add to the object. </param>
	void IAddChild.AddText(string text)
	{
		TextBlock textBlock = new TextBlock();
		textBlock.Text = text;
		Child = textBlock;
	}

	internal override void OnThemeChanged()
	{
		if (_popupRoot.Value != null)
		{
			TreeWalkHelper.InvalidateOnResourcesChange(_popupRoot.Value, null, ResourcesChangeInfo.ThemeChangeInfo);
		}
	}

	internal override bool BlockReverseInheritance()
	{
		return base.TemplatedParent == null;
	}

	/// <summary>Returns the logical parent of a <see cref="T:System.Windows.Controls.Primitives.Popup" />. </summary>
	/// <returns>If the <see cref="T:System.Windows.Controls.Primitives.Popup" /> does not have a defined parent and the <see cref="P:System.Windows.Controls.Primitives.Popup.PlacementTarget" /> is not null, the <see cref="P:System.Windows.Controls.Primitives.Popup.PlacementTarget" /> is returned. Otherwise, the return values are the same as <see cref="M:System.Windows.FrameworkElement.GetUIParentCore" />.</returns>
	protected internal override DependencyObject GetUIParentCore()
	{
		if (base.Parent == null)
		{
			UIElement placementTarget = PlacementTarget;
			if (placementTarget != null && (IsOpen || _secHelper.IsWindowAlive()))
			{
				return placementTarget;
			}
		}
		return base.GetUIParentCore();
	}

	internal override bool IgnoreModelParentBuildRoute(RoutedEventArgs e)
	{
		if (base.Parent == null)
		{
			return e.RoutedEvent != Mouse.LostMouseCaptureEvent;
		}
		return false;
	}

	private static Visual GetRootVisual(Visual child)
	{
		DependencyObject dependencyObject = child;
		DependencyObject parent;
		while ((parent = VisualTreeHelper.GetParent(dependencyObject)) != null)
		{
			dependencyObject = parent;
		}
		return dependencyObject as Visual;
	}

	private Visual GetTarget()
	{
		Visual visual = PlacementTarget;
		if (visual == null)
		{
			visual = VisualTreeHelper.GetContainingVisual2D(VisualTreeHelper.GetParent(this));
		}
		return visual;
	}

	private void SetHitTestable(bool hitTestable)
	{
		_popupRoot.Value.IsHitTestVisible = hitTestable;
		if (IsTransparent)
		{
			_secHelper.SetHitTestable(hitTestable);
		}
	}

	private static object AsyncCreateWindow(object arg)
	{
		Popup obj = (Popup)arg;
		obj._asyncCreate = null;
		obj.CreateWindow(asyncCall: true);
		return null;
	}

	private void CreateNewPopupRoot()
	{
		if (_popupRoot.Value == null)
		{
			_popupRoot.Value = new PopupRoot();
			AddLogicalChild(_popupRoot.Value);
			_popupRoot.Value.SetupLayoutBindings(this);
		}
	}

	private void CreateWindow(bool asyncCall)
	{
		ClearDropOpposite();
		Visual target = GetTarget();
		if (target != null && PopupSecurityHelper.IsVisualPresentationSourceNull(target))
		{
			if (!asyncCall)
			{
				_asyncCreate = base.Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(AsyncCreateWindow), this);
			}
			else
			{
				FirePopupCouldClose();
			}
			return;
		}
		if (_positionInfo != null)
		{
			_positionInfo.MouseRect = Rect.Empty;
			_positionInfo.ChildSize = Size.Empty;
		}
		bool flag = !_secHelper.IsWindowAlive();
		if (PopupInitialPlacementHelper.IsPerMonitorDpiScalingActive)
		{
			DestroyWindowImpl();
			_positionInfo = null;
			flag = true;
		}
		if (flag)
		{
			BuildWindow(target);
			CreateNewPopupRoot();
		}
		UIElement child = Child;
		if (_popupRoot.Value.Child != child)
		{
			_popupRoot.Value.Child = child;
		}
		UpdatePlacementTargetRegistration(null, PlacementTarget);
		UpdateTransform();
		bool flag2 = true;
		if (flag)
		{
			SetRootVisualToPopupRoot();
			flag2 = _secHelper.IsWindowAlive();
			if (flag2)
			{
				_secHelper.ForceMsaaToUiaBridge(_popupRoot.Value);
			}
		}
		else
		{
			UpdatePosition();
			flag2 = _secHelper.IsWindowAlive();
		}
		if (flag2)
		{
			ShowWindow();
			OnOpened(EventArgs.Empty);
		}
	}

	private void SetRootVisualToPopupRoot()
	{
		if (PopupAnimation != 0 && IsTransparent)
		{
			_popupRoot.Value.Opacity = 0.0;
		}
		_secHelper.SetWindowRootVisual(_popupRoot.Value);
	}

	private void BuildWindow(Visual targetVisual)
	{
		CoerceValue(AllowsTransparencyProperty);
		CoerceValue(HasDropShadowProperty);
		IsTransparent = AllowsTransparency;
		MS.Win32.NativeMethods.POINT pOINT = ((_positionInfo != null) ? new MS.Win32.NativeMethods.POINT(_positionInfo.X, _positionInfo.Y) : PopupInitialPlacementHelper.GetPlacementOrigin(this));
		_secHelper.BuildWindow(pOINT.x, pOINT.y, targetVisual, IsTransparent, PopupFilterMessage, OnWindowResize, OnDpiChanged);
	}

	private bool DestroyWindowImpl()
	{
		if (_secHelper.IsWindowAlive())
		{
			_secHelper.DestroyWindow(PopupFilterMessage, OnWindowResize, OnDpiChanged);
			return true;
		}
		return false;
	}

	private void DestroyWindow()
	{
		if (_secHelper.IsWindowAlive() && DestroyWindowImpl())
		{
			ReleasePopupCapture();
			OnClosed(EventArgs.Empty);
			UpdatePlacementTargetRegistration(PlacementTarget, null);
		}
	}

	private void ShowWindow()
	{
		if (_secHelper.IsWindowAlive())
		{
			_popupRoot.Value.Opacity = 1.0;
			SetupAnimations(visible: true);
			SetHitTestable(HitTestable || !IsTransparent);
			EstablishPopupCapture();
			_secHelper.ShowWindow();
		}
	}

	private void HideWindow()
	{
		bool flag = SetupAnimations(visible: false);
		SetHitTestable(hitTestable: false);
		ReleasePopupCapture();
		_asyncDestroy = new DispatcherTimer(DispatcherPriority.Input);
		_asyncDestroy.Tick += delegate
		{
			_asyncDestroy.Stop();
			_asyncDestroy = null;
			DestroyWindow();
		};
		_asyncDestroy.Interval = (flag ? AnimationDelayTime : TimeSpan.Zero);
		_asyncDestroy.Start();
		if (!flag)
		{
			_secHelper.HideWindow();
		}
	}

	private bool SetupAnimations(bool visible)
	{
		PopupAnimation popupAnimation = PopupAnimation;
		_popupRoot.Value.StopAnimations();
		if (popupAnimation != 0 && IsTransparent)
		{
			if (popupAnimation == PopupAnimation.Fade)
			{
				_popupRoot.Value.SetupFadeAnimation(AnimationDelayTime, visible);
				return true;
			}
			if (visible)
			{
				_popupRoot.Value.SetupTranslateAnimations(popupAnimation, AnimationDelayTime, AnimateFromRight, AnimateFromBottom);
				return true;
			}
		}
		return false;
	}

	private void CancelAsyncCreate()
	{
		if (_asyncCreate != null)
		{
			_asyncCreate.Abort();
			_asyncCreate = null;
		}
	}

	private void CancelAsyncDestroy()
	{
		if (_asyncDestroy != null)
		{
			_asyncDestroy.Stop();
			_asyncDestroy = null;
		}
	}

	internal void ForceClose()
	{
		if (_asyncDestroy != null)
		{
			CancelAsyncDestroy();
			DestroyWindow();
		}
	}

	private unsafe nint PopupFilterMessage(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		switch ((WindowMessage)msg)
		{
		case WindowMessage.WM_MOUSEACTIVATE:
			handled = true;
			return new IntPtr(3);
		case WindowMessage.WM_ACTIVATEAPP:
			if (wParam == IntPtr.Zero)
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(HandleDeactivateApp), null);
			}
			break;
		case WindowMessage.WM_WINDOWPOSCHANGING:
			if (_secHelper.IsChildPopup)
			{
				((MS.Win32.NativeMethods.WINDOWPOS*)lParam)->flags |= 256;
			}
			break;
		}
		return IntPtr.Zero;
	}

	private object HandleDeactivateApp(object arg)
	{
		if (!StaysOpen)
		{
			SetCurrentValueInternal(IsOpenProperty, BooleanBoxes.FalseBox);
		}
		FirePopupCouldClose();
		return null;
	}

	private void UpdateTransform()
	{
		Matrix matrix = base.LayoutTransform.Value * base.RenderTransform.Value;
		DependencyObject parent = VisualTreeHelper.GetParent(this);
		Visual visual = ((parent == null) ? null : GetRootVisual(this));
		if (visual != null)
		{
			matrix = matrix * TransformToAncestor(visual).AffineTransform.Value * PointUtil.GetVisualTransform(visual);
		}
		if (IsTransparent)
		{
			if (parent != null && (FlowDirection)parent.GetValue(FrameworkElement.FlowDirectionProperty) == FlowDirection.RightToLeft)
			{
				matrix.Scale(-1.0, 1.0);
			}
		}
		else
		{
			Vector vector = matrix.Transform(new Vector(1.0, 0.0));
			Vector vector2 = matrix.Transform(new Vector(0.0, 1.0));
			matrix = default(Matrix);
			matrix.Scale(vector.Length, vector2.Length);
		}
		_popupRoot.Value.Transform = new MatrixTransform(matrix);
	}

	private void OnWindowResize(object sender, AutoResizedEventArgs e)
	{
		if (_positionInfo == null)
		{
			throw new NullReferenceException(new NullReferenceException().Message, SavedException);
		}
		SavedExceptionField.ClearValue(this);
		if (e.Size != _positionInfo.ChildSize)
		{
			_positionInfo.ChildSize = e.Size;
			Reposition();
		}
	}

	private void OnDpiChanged(object sender, HwndDpiChangedEventArgs e)
	{
		if (IsOpen)
		{
			e.Handled = true;
		}
	}

	internal void Reposition()
	{
		if (!IsOpen || !_secHelper.IsWindowAlive())
		{
			return;
		}
		if (CheckAccess())
		{
			UpdatePosition();
			return;
		}
		base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
		{
			Reposition();
			return (object)null;
		}, null);
	}

	private static bool IsAbsolutePlacementMode(PlacementMode placement)
	{
		if (placement == PlacementMode.Absolute || placement == PlacementMode.AbsolutePoint || (uint)(placement - 7) <= 1u)
		{
			return true;
		}
		return false;
	}

	private void UpdatePosition()
	{
		if (_popupRoot.Value == null)
		{
			return;
		}
		PlacementMode placementInternal = PlacementInternal;
		Point[] placementTargetInterestPoints = GetPlacementTargetInterestPoints(placementInternal);
		Point[] childInterestPoints = GetChildInterestPoints(placementInternal);
		Rect bounds = GetBounds(placementTargetInterestPoints);
		Rect rect = GetBounds(childInterestPoints);
		double num = rect.Width * rect.Height;
		int num2 = -1;
		Vector offsetVector = new Vector(_positionInfo.X, _positionInfo.Y);
		double num3 = -1.0;
		CustomPopupPlacement[] array = null;
		int num4;
		if (placementInternal == PlacementMode.Custom)
		{
			CustomPopupPlacementCallback customPopupPlacementCallback = CustomPopupPlacementCallback;
			if (customPopupPlacementCallback != null)
			{
				array = customPopupPlacementCallback(rect.Size, bounds.Size, new Point(HorizontalOffset, VerticalOffset));
			}
			num4 = ((array != null) ? array.Length : 0);
			if (!IsOpen)
			{
				return;
			}
		}
		else
		{
			num4 = GetNumberOfCombinations(placementInternal);
		}
		Rect screenBounds;
		for (int i = 0; i < num4; i++)
		{
			bool animateFromRight = false;
			bool animateFromBottom = false;
			Vector vector;
			PopupPrimaryAxis axis;
			if (placementInternal == PlacementMode.Custom)
			{
				vector = (Vector)placementTargetInterestPoints[0] + (Vector)array[i].Point;
				axis = array[i].PrimaryAxis;
			}
			else
			{
				PointCombination pointCombination = GetPointCombination(placementInternal, i, out axis);
				InterestPoint targetInterestPoint = pointCombination.TargetInterestPoint;
				InterestPoint childInterestPoint = pointCombination.ChildInterestPoint;
				vector = placementTargetInterestPoints[(int)targetInterestPoint] - childInterestPoints[(int)childInterestPoint];
				animateFromRight = childInterestPoint == InterestPoint.TopRight || childInterestPoint == InterestPoint.BottomRight;
				animateFromBottom = childInterestPoint == InterestPoint.BottomLeft || childInterestPoint == InterestPoint.BottomRight;
			}
			Rect rect2 = Rect.Offset(rect, vector);
			screenBounds = GetScreenBounds(bounds, placementTargetInterestPoints[0]);
			Rect rect3 = Rect.Intersect(screenBounds, rect2);
			double num5 = ((rect3 != Rect.Empty) ? (rect3.Width * rect3.Height) : 0.0);
			if (num5 - num3 > 0.01)
			{
				num2 = i;
				offsetVector = vector;
				num3 = num5;
				AnimateFromRight = animateFromRight;
				AnimateFromBottom = animateFromBottom;
				if (Math.Abs(num5 - num) < 0.01)
				{
					break;
				}
			}
		}
		if (num2 >= 2 && (placementInternal == PlacementMode.Right || placementInternal == PlacementMode.Left))
		{
			DropOpposite = !DropOpposite;
		}
		rect = new Rect((Size)_secHelper.GetTransformToDevice().Transform((Point)_popupRoot.Value.RenderSize));
		rect.Offset(offsetVector);
		screenBounds = GetScreenBounds(bounds, placementTargetInterestPoints[0]);
		Rect rect4 = Rect.Intersect(screenBounds, rect);
		if (Math.Abs(rect4.Width - rect.Width) > 0.01 || Math.Abs(rect4.Height - rect.Height) > 0.01)
		{
			Point point = placementTargetInterestPoints[0];
			Vector vector2 = placementTargetInterestPoints[1] - point;
			vector2.Normalize();
			if (!IsTransparent || double.IsNaN(vector2.Y) || Math.Abs(vector2.Y) < 0.01)
			{
				if (rect.Right > screenBounds.Right)
				{
					offsetVector.X = screenBounds.Right - rect.Width;
				}
				else if (rect.Left < screenBounds.Left)
				{
					offsetVector.X = screenBounds.Left;
				}
			}
			else if (IsTransparent && Math.Abs(vector2.X) < 0.01)
			{
				if (rect.Bottom > screenBounds.Bottom)
				{
					offsetVector.Y = screenBounds.Bottom - rect.Height;
				}
				else if (rect.Top < screenBounds.Top)
				{
					offsetVector.Y = screenBounds.Top;
				}
			}
			Point point2 = placementTargetInterestPoints[2];
			Vector vector3 = point - point2;
			vector3.Normalize();
			if (!IsTransparent || double.IsNaN(vector3.X) || Math.Abs(vector3.X) < 0.01)
			{
				if (rect.Bottom > screenBounds.Bottom)
				{
					offsetVector.Y = screenBounds.Bottom - rect.Height;
				}
				else if (rect.Top < screenBounds.Top)
				{
					offsetVector.Y = screenBounds.Top;
				}
			}
			else if (IsTransparent && Math.Abs(vector3.Y) < 0.01)
			{
				if (rect.Right > screenBounds.Right)
				{
					offsetVector.X = screenBounds.Right - rect.Width;
				}
				else if (rect.Left < screenBounds.Left)
				{
					offsetVector.X = screenBounds.Left;
				}
			}
		}
		int num6 = DoubleUtil.DoubleToInt(offsetVector.X);
		int num7 = DoubleUtil.DoubleToInt(offsetVector.Y);
		if (num6 != _positionInfo.X || num7 != _positionInfo.Y)
		{
			_positionInfo.X = num6;
			_positionInfo.Y = num7;
			_secHelper.SetPopupPos(position: true, num6, num7, size: false, 0, 0);
		}
	}

	private void GetPopupRootLimits(out Rect targetBounds, out Rect screenBounds, out Size limitSize)
	{
		PlacementMode placementInternal = PlacementInternal;
		Point[] placementTargetInterestPoints = GetPlacementTargetInterestPoints(placementInternal);
		targetBounds = GetBounds(placementTargetInterestPoints);
		screenBounds = GetScreenBounds(targetBounds, placementTargetInterestPoints[0]);
		PopupPrimaryAxis primaryAxis = GetPrimaryAxis(placementInternal);
		limitSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
		switch (primaryAxis)
		{
		case PopupPrimaryAxis.Horizontal:
		{
			Point point2 = placementTargetInterestPoints[0];
			Vector vector2 = placementTargetInterestPoints[2] - point2;
			vector2.Normalize();
			if (!IsTransparent || double.IsNaN(vector2.X) || Math.Abs(vector2.X) < 0.01)
			{
				limitSize.Height = Math.Max(0.0, Math.Max(screenBounds.Bottom - targetBounds.Bottom, targetBounds.Top - screenBounds.Top));
			}
			else if (IsTransparent && Math.Abs(vector2.Y) < 0.01)
			{
				limitSize.Width = Math.Max(0.0, Math.Max(screenBounds.Right - targetBounds.Right, targetBounds.Left - screenBounds.Left));
			}
			break;
		}
		case PopupPrimaryAxis.Vertical:
		{
			Point point = placementTargetInterestPoints[0];
			Vector vector = placementTargetInterestPoints[1] - point;
			vector.Normalize();
			if (!IsTransparent || double.IsNaN(vector.X) || Math.Abs(vector.Y) < 0.01)
			{
				limitSize.Width = Math.Max(0.0, Math.Max(screenBounds.Right - targetBounds.Right, targetBounds.Left - screenBounds.Left));
			}
			else if (IsTransparent && Math.Abs(vector.X) < 0.01)
			{
				limitSize.Height = Math.Max(0.0, Math.Max(screenBounds.Bottom - targetBounds.Bottom, targetBounds.Top - screenBounds.Top));
			}
			break;
		}
		}
	}

	private Point[] GetPlacementTargetInterestPoints(PlacementMode placement)
	{
		if (_positionInfo == null)
		{
			_positionInfo = new PositionInfo();
		}
		Rect rect = PlacementRectangle;
		UIElement uIElement = GetTarget() as UIElement;
		Vector vector = new Vector(HorizontalOffset, VerticalOffset);
		Point[] array;
		if (uIElement == null || IsAbsolutePlacementMode(placement))
		{
			if (placement == PlacementMode.Mouse || placement == PlacementMode.MousePoint)
			{
				if (_positionInfo.MouseRect == Rect.Empty)
				{
					_positionInfo.MouseRect = GetMouseRect(placement);
				}
				rect = _positionInfo.MouseRect;
			}
			else if (rect == Rect.Empty)
			{
				rect = default(Rect);
			}
			vector = _secHelper.GetTransformToDevice().Transform(vector);
			rect.Offset(vector);
			array = InterestPointsFromRect(rect);
		}
		else
		{
			if (rect == Rect.Empty)
			{
				rect = ((placement == PlacementMode.Relative || placement == PlacementMode.RelativePoint) ? default(Rect) : new Rect(0.0, 0.0, uIElement.RenderSize.Width, uIElement.RenderSize.Height));
			}
			rect.Offset(vector);
			array = InterestPointsFromRect(rect);
			Visual rootVisual = GetRootVisual(uIElement);
			GeneralTransform generalTransform = TransformToClient(uIElement, rootVisual);
			for (int i = 0; i < 5; i++)
			{
				generalTransform.TryTransform(array[i], out array[i]);
				array[i] = _secHelper.ClientToScreen(rootVisual, array[i]);
			}
		}
		return array;
	}

	private static void SwapPoints(ref Point p1, ref Point p2)
	{
		Point point = p1;
		p1 = p2;
		p2 = point;
	}

	private Point[] GetChildInterestPoints(PlacementMode placement)
	{
		UIElement child = Child;
		if (child == null)
		{
			return InterestPointsFromRect(default(Rect));
		}
		Point[] array = InterestPointsFromRect(new Rect(default(Point), child.RenderSize));
		if (GetTarget() is UIElement uIElement && !IsAbsolutePlacementMode(placement) && (FlowDirection)uIElement.GetValue(FrameworkElement.FlowDirectionProperty) != (FlowDirection)child.GetValue(FrameworkElement.FlowDirectionProperty))
		{
			SwapPoints(ref array[0], ref array[1]);
			SwapPoints(ref array[2], ref array[3]);
		}
		Vector animationOffset = _popupRoot.Value.AnimationOffset;
		GeneralTransform generalTransform = TransformToClient(child, _popupRoot.Value);
		for (int i = 0; i < 5; i++)
		{
			generalTransform.TryTransform(array[i] - animationOffset, out array[i]);
		}
		return array;
	}

	private static Point[] InterestPointsFromRect(Rect rect)
	{
		return new Point[5]
		{
			rect.TopLeft,
			rect.TopRight,
			rect.BottomLeft,
			rect.BottomRight,
			new Point(rect.Left + rect.Width / 2.0, rect.Top + rect.Height / 2.0)
		};
	}

	private static GeneralTransform TransformToClient(Visual visual, Visual rootVisual)
	{
		return new GeneralTransformGroup
		{
			Children = 
			{
				visual.TransformToAncestor(rootVisual),
				(GeneralTransform)new MatrixTransform(PointUtil.GetVisualTransform(rootVisual) * PopupSecurityHelper.GetTransformToDevice(rootVisual))
			}
		};
	}

	private Rect GetBounds(Point[] interestPoints)
	{
		double num;
		double num2 = (num = interestPoints[0].X);
		double num3;
		double num4 = (num3 = interestPoints[0].Y);
		for (int i = 1; i < interestPoints.Length; i++)
		{
			double x = interestPoints[i].X;
			double y = interestPoints[i].Y;
			if (x < num2)
			{
				num2 = x;
			}
			if (x > num)
			{
				num = x;
			}
			if (y < num4)
			{
				num4 = y;
			}
			if (y > num3)
			{
				num3 = y;
			}
		}
		return new Rect(num2, num4, num - num2, num3 - num4);
	}

	private static int GetNumberOfCombinations(PlacementMode placement)
	{
		switch (placement)
		{
		case PlacementMode.Bottom:
		case PlacementMode.Mouse:
		case PlacementMode.Top:
			return 2;
		case PlacementMode.Right:
		case PlacementMode.AbsolutePoint:
		case PlacementMode.RelativePoint:
		case PlacementMode.MousePoint:
		case PlacementMode.Left:
			return 4;
		case PlacementMode.Custom:
			return 0;
		default:
			return 1;
		}
	}

	private PointCombination GetPointCombination(PlacementMode placement, int i, out PopupPrimaryAxis axis)
	{
		bool menuDropAlignment = SystemParameters.MenuDropAlignment;
		switch (placement)
		{
		case PlacementMode.Bottom:
		case PlacementMode.Mouse:
			axis = PopupPrimaryAxis.Horizontal;
			if (menuDropAlignment)
			{
				switch (i)
				{
				case 0:
					return new PointCombination(InterestPoint.BottomRight, InterestPoint.TopRight);
				case 1:
					return new PointCombination(InterestPoint.TopRight, InterestPoint.BottomRight);
				}
			}
			else
			{
				switch (i)
				{
				case 0:
					return new PointCombination(InterestPoint.BottomLeft, InterestPoint.TopLeft);
				case 1:
					return new PointCombination(InterestPoint.TopLeft, InterestPoint.BottomLeft);
				}
			}
			break;
		case PlacementMode.Top:
			axis = PopupPrimaryAxis.Horizontal;
			if (menuDropAlignment)
			{
				switch (i)
				{
				case 0:
					return new PointCombination(InterestPoint.TopRight, InterestPoint.BottomRight);
				case 1:
					return new PointCombination(InterestPoint.BottomRight, InterestPoint.TopRight);
				}
			}
			else
			{
				switch (i)
				{
				case 0:
					return new PointCombination(InterestPoint.TopLeft, InterestPoint.BottomLeft);
				case 1:
					return new PointCombination(InterestPoint.BottomLeft, InterestPoint.TopLeft);
				}
			}
			break;
		case PlacementMode.Right:
		case PlacementMode.Left:
			axis = PopupPrimaryAxis.Vertical;
			menuDropAlignment |= DropOpposite;
			if ((menuDropAlignment && placement == PlacementMode.Right) || (!menuDropAlignment && placement == PlacementMode.Left))
			{
				switch (i)
				{
				case 0:
					return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopRight);
				case 1:
					return new PointCombination(InterestPoint.BottomLeft, InterestPoint.BottomRight);
				case 2:
					return new PointCombination(InterestPoint.TopRight, InterestPoint.TopLeft);
				case 3:
					return new PointCombination(InterestPoint.BottomRight, InterestPoint.BottomLeft);
				}
			}
			else
			{
				switch (i)
				{
				case 0:
					return new PointCombination(InterestPoint.TopRight, InterestPoint.TopLeft);
				case 1:
					return new PointCombination(InterestPoint.BottomRight, InterestPoint.BottomLeft);
				case 2:
					return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopRight);
				case 3:
					return new PointCombination(InterestPoint.BottomLeft, InterestPoint.BottomRight);
				}
			}
			break;
		case PlacementMode.Relative:
		case PlacementMode.AbsolutePoint:
		case PlacementMode.RelativePoint:
		case PlacementMode.MousePoint:
			axis = PopupPrimaryAxis.Horizontal;
			if (menuDropAlignment)
			{
				switch (i)
				{
				case 0:
					return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopRight);
				case 1:
					return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopLeft);
				case 2:
					return new PointCombination(InterestPoint.TopLeft, InterestPoint.BottomRight);
				case 3:
					return new PointCombination(InterestPoint.TopLeft, InterestPoint.BottomLeft);
				}
			}
			else
			{
				switch (i)
				{
				case 0:
					return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopLeft);
				case 1:
					return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopRight);
				case 2:
					return new PointCombination(InterestPoint.TopLeft, InterestPoint.BottomLeft);
				case 3:
					return new PointCombination(InterestPoint.TopLeft, InterestPoint.BottomRight);
				}
			}
			break;
		case PlacementMode.Center:
			axis = PopupPrimaryAxis.None;
			return new PointCombination(InterestPoint.Center, InterestPoint.Center);
		default:
			axis = PopupPrimaryAxis.None;
			return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopLeft);
		}
		return new PointCombination(InterestPoint.TopLeft, InterestPoint.TopRight);
	}

	private static PopupPrimaryAxis GetPrimaryAxis(PlacementMode placement)
	{
		switch (placement)
		{
		case PlacementMode.Right:
		case PlacementMode.Left:
			return PopupPrimaryAxis.Vertical;
		case PlacementMode.Bottom:
		case PlacementMode.AbsolutePoint:
		case PlacementMode.RelativePoint:
		case PlacementMode.Top:
			return PopupPrimaryAxis.Horizontal;
		default:
			return PopupPrimaryAxis.None;
		}
	}

	internal Size RestrictSize(Size desiredSize)
	{
		GetPopupRootLimits(out var _, out var screenBounds, out var limitSize);
		desiredSize = (Size)_secHelper.GetTransformToDevice().Transform((Point)desiredSize);
		desiredSize.Width = Math.Min(desiredSize.Width, screenBounds.Width);
		desiredSize.Width = Math.Min(desiredSize.Width, limitSize.Width);
		double val = 0.75 * screenBounds.Width * screenBounds.Height / desiredSize.Width;
		desiredSize.Height = Math.Min(desiredSize.Height, screenBounds.Height);
		desiredSize.Height = Math.Min(desiredSize.Height, val);
		desiredSize.Height = Math.Min(desiredSize.Height, limitSize.Height);
		desiredSize = (Size)_secHelper.GetTransformFromDevice().Transform((Point)desiredSize);
		return desiredSize;
	}

	private Rect GetScreenBounds(Rect boundingBox, Point p)
	{
		if (_secHelper.IsChildPopup)
		{
			return _secHelper.GetParentWindowRect();
		}
		MS.Win32.NativeMethods.RECT rc = new MS.Win32.NativeMethods.RECT(0, 0, 0, 0);
		MS.Win32.NativeMethods.RECT rect = PointUtil.FromRect(boundingBox);
		nint num = SafeNativeMethods.MonitorFromRect(ref rect, 2);
		if (num != IntPtr.Zero)
		{
			MS.Win32.NativeMethods.MONITORINFOEX mONITORINFOEX = new MS.Win32.NativeMethods.MONITORINFOEX();
			mONITORINFOEX.cbSize = Marshal.SizeOf(typeof(MS.Win32.NativeMethods.MONITORINFOEX));
			SafeNativeMethods.GetMonitorInfo(new HandleRef(null, num), mONITORINFOEX);
			rc = (((!(Child is MenuBase) && !(Child is ToolTip) && !(base.TemplatedParent is MenuItem)) || !(p.X >= (double)mONITORINFOEX.rcWork.left) || !(p.X <= (double)mONITORINFOEX.rcWork.right) || !(p.Y >= (double)mONITORINFOEX.rcWork.top) || !(p.Y <= (double)mONITORINFOEX.rcWork.bottom)) ? mONITORINFOEX.rcMonitor : mONITORINFOEX.rcWork);
		}
		return PointUtil.ToRect(rc);
	}

	private Rect GetMouseRect(PlacementMode placement)
	{
		MS.Win32.NativeMethods.POINT mouseCursorPos = _secHelper.GetMouseCursorPos(GetTarget());
		if (placement == PlacementMode.Mouse)
		{
			GetMouseCursorSize(out var width, out var height, out var hotX, out var hotY);
			return new Rect(mouseCursorPos.x, mouseCursorPos.y - 1, Math.Max(0, width - hotX), Math.Max(0, height - hotY + 2));
		}
		return new Rect(mouseCursorPos.x, mouseCursorPos.y, 0.0, 0.0);
	}

	private static void GetMouseCursorSize(out int width, out int height, out int hotX, out int hotY)
	{
		width = (height = (hotX = (hotY = 0)));
		nint cursor = SafeNativeMethods.GetCursor();
		if (cursor == IntPtr.Zero)
		{
			return;
		}
		width = (height = 16);
		MS.Win32.NativeMethods.ICONINFO piconinfo = new MS.Win32.NativeMethods.ICONINFO();
		bool flag = true;
		try
		{
			MS.Win32.UnsafeNativeMethods.GetIconInfo(new HandleRef(null, cursor), out piconinfo);
		}
		catch (Win32Exception)
		{
			flag = false;
		}
		if (!flag)
		{
			return;
		}
		MS.Win32.NativeMethods.BITMAP bITMAP = new MS.Win32.NativeMethods.BITMAP();
		if (MS.Win32.UnsafeNativeMethods.GetObject(piconinfo.hbmMask.MakeHandleRef(null), Marshal.SizeOf(typeof(MS.Win32.NativeMethods.BITMAP)), bITMAP) != 0)
		{
			int num = bITMAP.bmWidth * bITMAP.bmHeight / 8;
			byte[] array = new byte[num * 2];
			if (MS.Win32.UnsafeNativeMethods.GetBitmapBits(piconinfo.hbmMask.MakeHandleRef(null), array.Length, array) != 0)
			{
				bool flag2 = false;
				if (piconinfo.hbmColor.IsInvalid)
				{
					flag2 = true;
					num /= 2;
				}
				bool flag3 = true;
				int num2 = num;
				for (num2--; num2 >= 0; num2--)
				{
					if (array[num2] != byte.MaxValue || (flag2 && array[num2 + num] != 0))
					{
						flag3 = false;
						break;
					}
				}
				if (!flag3)
				{
					int i;
					for (i = 0; i < num && array[i] == byte.MaxValue && (!flag2 || array[i + num] == 0); i++)
					{
					}
					int num3 = bITMAP.bmWidth / 8;
					int num4 = num2 % num3 * 8;
					num2 /= num3;
					int num5 = i % num3 * 8;
					i /= num3;
					width = num4 - num5 + 1;
					height = num2 - i + 1;
					hotX = piconinfo.xHotspot - num5;
					hotY = piconinfo.yHotspot - i;
				}
				else
				{
					width = bITMAP.bmWidth;
					height = bITMAP.bmHeight;
					hotX = piconinfo.xHotspot;
					hotY = piconinfo.yHotspot;
				}
			}
		}
		piconinfo.hbmColor.Dispose();
		piconinfo.hbmMask.Dispose();
	}

	internal Rect GetParentWindowRect()
	{
		return _secHelper.GetParentWindowRect();
	}

	internal Rect GetWindowRect()
	{
		return _secHelper.GetWindowRect();
	}
}
