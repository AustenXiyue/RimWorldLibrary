using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Win32;

namespace System.Windows.Controls;

internal sealed class PopupControlService
{
	private struct WeakRefWrapper<T> where T : class
	{
		private WeakReference<T> _storage;

		public T GetValue()
		{
			T target;
			if (_storage != null)
			{
				if (!_storage.TryGetTarget(out target))
				{
					_storage = null;
				}
			}
			else
			{
				target = null;
			}
			return target;
		}

		public void SetValue(T value)
		{
			if (value == null)
			{
				_storage = null;
			}
			else if (_storage == null)
			{
				_storage = new WeakReference<T>(value);
			}
			else
			{
				_storage.SetTarget(value);
			}
		}
	}

	private class ConvexHull
	{
		private enum Direction
		{
			Skew,
			Left,
			Right,
			Up,
			Down
		}

		[DebuggerDisplay("{X} {Y} {Direction}")]
		private struct Point
		{
			public int X { get; set; }

			public int Y { get; set; }

			public Direction Direction { get; set; }

			public Point(int x, int y, Direction d = Direction.Skew)
			{
				X = x;
				Y = y;
				Direction = d;
			}
		}

		private class PointList : List<Point>
		{
		}

		private Point[] _points;

		private PresentationSource _source;

		internal ConvexHull(PresentationSource source, List<MS.Win32.NativeMethods.RECT> rects)
		{
			_source = source;
			PointList pointList = new PointList();
			if (rects.Count == 1)
			{
				AddPoints(pointList, rects[0], rectIsHull: true);
				_points = pointList.ToArray();
				return;
			}
			foreach (MS.Win32.NativeMethods.RECT rect in rects)
			{
				AddPoints(pointList, rect);
			}
			SortPoints(pointList);
			BuildHullIncrementally(pointList);
		}

		private void SortPoints(PointList points)
		{
			int i = 1;
			for (int count = points.Count; i < count; i++)
			{
				Point value = points[i];
				int num;
				for (num = i - 1; num >= 0; num--)
				{
					int num2 = points[num].Y - value.Y;
					if (num2 <= 0 && (num2 != 0 || points[num].X <= value.X))
					{
						break;
					}
					points[num + 1] = points[num];
				}
				points[num + 1] = value;
			}
		}

		private void BuildHullIncrementally(PointList points)
		{
			int count = points.Count;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			while (num < count)
			{
				int y = points[num].Y;
				Point a = points[num];
				int i;
				for (i = num + 1; i < count && points[i].Y == y; i++)
				{
				}
				Point a2 = points[i - 1];
				int num5 = ((i == num + 1) ? 1 : 2);
				num = i;
				if (num2 == 0)
				{
					if (num5 == 2)
					{
						points[0] = a2;
						points[1] = a;
						num3 = 1;
					}
					else
					{
						points[0] = a;
						num3 = 0;
					}
					num4 = (num2 = num5);
					continue;
				}
				int num6 = num3;
				while (num6 > 0 && Cross(in a, points[num6], points[num6 - 1]) <= 0)
				{
					num6--;
				}
				int j;
				for (j = num4; j < num2; j++)
				{
					int num7 = j + 1;
					if (num7 == num2)
					{
						num7 = 0;
					}
					if (Cross(in a2, points[j], points[num7]) < 0)
					{
						break;
					}
				}
				int num8 = j - num6 - 1;
				int num9 = num5 - num8;
				if (num9 < 0)
				{
					for (int k = j; k < num2; k++)
					{
						points[k + num9] = points[k];
					}
				}
				else if (num9 > 0)
				{
					for (int num10 = num2 - 1; num10 >= j; num10--)
					{
						points[num10 + num9] = points[num10];
					}
				}
				points[num6 + 1] = a;
				num3 = (num4 = num6 + 1);
				if (num5 == 2)
				{
					points[num6 + 2] = a2;
					num4 = num6 + 2;
				}
				num2 += num9;
			}
			points.RemoveRange(num2, count - num2);
			_points = points.ToArray();
			SetDirections();
		}

		private void SetDirections()
		{
			int i = 0;
			for (int num = _points.Length; i < num; i++)
			{
				int num2 = i + 1;
				if (num2 == num)
				{
					num2 = 0;
				}
				if (_points[i].X == _points[num2].X)
				{
					_points[i].Direction = ((_points[i].Y >= _points[num2].Y) ? Direction.Up : Direction.Down);
				}
				else if (_points[i].Y == _points[num2].Y)
				{
					_points[i].Direction = ((_points[i].X >= _points[num2].X) ? Direction.Left : Direction.Right);
				}
				else
				{
					_points[i].Direction = Direction.Skew;
				}
			}
		}

		private void AddPoints(PointList points, in MS.Win32.NativeMethods.RECT rect, bool rectIsHull = false)
		{
			if (rectIsHull)
			{
				points.Add(new Point(rect.right, rect.top, Direction.Left));
				points.Add(new Point(rect.left, rect.top, Direction.Down));
				points.Add(new Point(rect.left, rect.bottom, Direction.Right));
				points.Add(new Point(rect.right, rect.bottom, Direction.Up));
			}
			else
			{
				points.Add(new Point(rect.left, rect.top));
				points.Add(new Point(rect.right, rect.top));
				points.Add(new Point(rect.left, rect.bottom));
				points.Add(new Point(rect.right, rect.bottom));
			}
		}

		internal bool ContainsMousePoint()
		{
			PresentationSource criticalActiveSource = Mouse.PrimaryDevice.CriticalActiveSource;
			System.Windows.Point pointClient = Mouse.PrimaryDevice.NonRelativePosition;
			if (criticalActiveSource != _source)
			{
				pointClient = PointUtil.ScreenToClient(PointUtil.ClientToScreen(pointClient, criticalActiveSource), _source);
			}
			return ContainsPoint(_source, (int)pointClient.X, (int)pointClient.Y);
		}

		internal bool ContainsPoint(PresentationSource source, int x, int y)
		{
			if (source != _source)
			{
				return false;
			}
			int i = 0;
			for (int num = _points.Length; i < num; i++)
			{
				switch (_points[i].Direction)
				{
				case Direction.Left:
					if (y < _points[i].Y)
					{
						return false;
					}
					break;
				case Direction.Right:
					if (y >= _points[i].Y)
					{
						return false;
					}
					break;
				case Direction.Up:
					if (x >= _points[i].X)
					{
						return false;
					}
					break;
				case Direction.Down:
					if (x < _points[i].X)
					{
						return false;
					}
					break;
				}
			}
			int j = 0;
			for (int num2 = _points.Length; j < num2; j++)
			{
				if (_points[j].Direction == Direction.Skew)
				{
					int num3 = j + 1;
					if (num3 == num2)
					{
						num3 = 0;
					}
					Point c = new Point(x, y);
					if (Cross(in _points[j], in _points[num3], in c) > 0)
					{
						return false;
					}
				}
			}
			return true;
		}

		private static int Cross(in Point a, in Point b, in Point c)
		{
			return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
		}
	}

	internal static readonly RoutedEvent ContextMenuOpenedEvent = EventManager.RegisterRoutedEvent("Opened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PopupControlService));

	internal static readonly RoutedEvent ContextMenuClosedEvent = EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PopupControlService));

	internal static readonly DependencyProperty ServiceOwnedProperty = DependencyProperty.RegisterAttached("ServiceOwned", typeof(bool), typeof(PopupControlService), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));

	internal static readonly DependencyProperty OwnerProperty = DependencyProperty.RegisterAttached("Owner", typeof(DependencyObject), typeof(PopupControlService), new FrameworkPropertyMetadata(null, OnOwnerChanged));

	private static int ShortDelay = 73;

	private ToolTip _pendingToolTip;

	private DispatcherTimer _pendingToolTipTimer;

	private ToolTip _sentinelToolTip;

	private ToolTip _currentToolTip;

	private DispatcherTimer _currentToolTipTimer;

	private DispatcherTimer _forceCloseTimer;

	private Key _lastCtrlKeyDown;

	private WeakRefWrapper<IInputElement> _lastMouseDirectlyOver;

	private WeakRefWrapper<DependencyObject> _lastMouseToolTipOwner;

	private bool _quickShow;

	private ToolTip PendingToolTip
	{
		get
		{
			return _pendingToolTip;
		}
		set
		{
			_pendingToolTip = value;
		}
	}

	private DispatcherTimer PendingToolTipTimer
	{
		get
		{
			return _pendingToolTipTimer;
		}
		set
		{
			_pendingToolTipTimer = value;
		}
	}

	internal ToolTip CurrentToolTip => _currentToolTip;

	private DispatcherTimer CurrentToolTipTimer
	{
		get
		{
			return _currentToolTipTimer;
		}
		set
		{
			_currentToolTipTimer = value;
		}
	}

	private IInputElement LastMouseDirectlyOver
	{
		get
		{
			return _lastMouseDirectlyOver.GetValue();
		}
		set
		{
			_lastMouseDirectlyOver.SetValue(value);
		}
	}

	private DependencyObject LastMouseToolTipOwner
	{
		get
		{
			return _lastMouseToolTipOwner.GetValue();
		}
		set
		{
			_lastMouseToolTipOwner.SetValue(value);
		}
	}

	private ConvexHull SafeArea { get; set; }

	internal static PopupControlService Current => FrameworkElement.PopupControlService;

	internal PopupControlService()
	{
		InputManager.Current.PostProcessInput += OnPostProcessInput;
	}

	private void OnPostProcessInput(object sender, ProcessInputEventArgs e)
	{
		if (e.StagingItem.Input.RoutedEvent == InputManager.InputReportEvent)
		{
			InputReportEventArgs inputReportEventArgs = (InputReportEventArgs)e.StagingItem.Input;
			if (inputReportEventArgs.Handled || inputReportEventArgs.Report.Type != InputType.Mouse)
			{
				return;
			}
			RawMouseInputReport rawMouseInputReport = (RawMouseInputReport)inputReportEventArgs.Report;
			if ((rawMouseInputReport.Actions & RawMouseActions.AbsoluteMove) == RawMouseActions.AbsoluteMove)
			{
				if (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
				{
					DismissToolTips();
					return;
				}
				IInputElement rawHit = Mouse.PrimaryDevice.RawDirectlyOver;
				if (rawHit == null)
				{
					return;
				}
				if (Mouse.CapturedMode != 0)
				{
					PresentationSource presentationSource = PresentationSource.CriticalFromVisual((DependencyObject)rawHit);
					UIElement uIElement = ((presentationSource != null) ? (presentationSource.RootVisual as UIElement) : null);
					if (uIElement != null)
					{
						Point position = Mouse.PrimaryDevice.GetPosition(uIElement);
						uIElement.InputHitTest(position, out var _, out rawHit);
					}
					else
					{
						rawHit = null;
					}
				}
				if (rawHit != null)
				{
					OnMouseMove(rawHit);
				}
			}
			else if ((rawMouseInputReport.Actions & RawMouseActions.Deactivate) == RawMouseActions.Deactivate)
			{
				DismissToolTips();
				LastMouseDirectlyOver = null;
				if (SafeNativeMethods.GetCapture() == IntPtr.Zero)
				{
					LastMouseToolTipOwner = null;
				}
			}
		}
		else if (e.StagingItem.Input.RoutedEvent == Keyboard.KeyDownEvent)
		{
			ProcessKeyDown(sender, (KeyEventArgs)e.StagingItem.Input);
		}
		else if (e.StagingItem.Input.RoutedEvent == Keyboard.KeyUpEvent)
		{
			ProcessKeyUp(sender, (KeyEventArgs)e.StagingItem.Input);
		}
		else if (e.StagingItem.Input.RoutedEvent == Mouse.MouseUpEvent)
		{
			ProcessMouseUp(sender, (MouseButtonEventArgs)e.StagingItem.Input);
		}
		else if (e.StagingItem.Input.RoutedEvent == Mouse.MouseDownEvent)
		{
			DismissToolTips();
		}
		else if (e.StagingItem.Input.RoutedEvent == Keyboard.GotKeyboardFocusEvent)
		{
			ProcessGotKeyboardFocus(sender, (KeyboardFocusChangedEventArgs)e.StagingItem.Input);
		}
		else if (e.StagingItem.Input.RoutedEvent == Keyboard.LostKeyboardFocusEvent)
		{
			ProcessLostKeyboardFocus(sender, (KeyboardFocusChangedEventArgs)e.StagingItem.Input);
		}
	}

	private void OnMouseMove(IInputElement directlyOver)
	{
		if (MouseHasLeftSafeArea())
		{
			DismissCurrentToolTip();
		}
		if (directlyOver != LastMouseDirectlyOver)
		{
			LastMouseDirectlyOver = directlyOver;
			DependencyObject o = FindToolTipOwner(directlyOver, ToolTipService.TriggerAction.Mouse);
			BeginShowToolTip(o, ToolTipService.TriggerAction.Mouse);
		}
		else if (PendingToolTipTimer?.Tag == BooleanBoxes.TrueBox)
		{
			if (CurrentToolTip == null)
			{
				PendingToolTipTimer.Stop();
				PromotePendingToolTipToCurrent(ToolTipService.TriggerAction.Mouse);
			}
			else
			{
				PendingToolTipTimer.Stop();
				PendingToolTipTimer.Start();
			}
		}
	}

	private void ProcessGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		DismissKeyboardToolTips();
		if (KeyboardNavigation.IsKeyboardMostRecentInputDevice())
		{
			IInputElement newFocus = e.NewFocus;
			DependencyObject o = FindToolTipOwner(newFocus, ToolTipService.TriggerAction.KeyboardFocus);
			BeginShowToolTip(o, ToolTipService.TriggerAction.KeyboardFocus);
		}
	}

	private void ProcessLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		DismissKeyboardToolTips();
	}

	private void ProcessMouseUp(object sender, MouseButtonEventArgs e)
	{
		DismissToolTips();
		if (e.Handled || e.ChangedButton != MouseButton.Right || e.RightButton != 0)
		{
			return;
		}
		IInputElement rawDirectlyOver = Mouse.PrimaryDevice.RawDirectlyOver;
		if (rawDirectlyOver != null)
		{
			Point position = Mouse.PrimaryDevice.GetPosition(rawDirectlyOver);
			if (RaiseContextMenuOpeningEvent(rawDirectlyOver, position.X, position.Y, e.UserInitiated))
			{
				e.Handled = true;
			}
		}
	}

	private void ProcessKeyDown(object sender, KeyEventArgs e)
	{
		if (!e.Handled)
		{
			ModifierKeys modifierKeys = Keyboard.Modifiers & (ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Windows);
			if (e.SystemKey == Key.F10 && modifierKeys == (ModifierKeys.Control | ModifierKeys.Shift))
			{
				e.Handled = OpenOrCloseToolTipViaShortcut();
			}
			else if (e.SystemKey == Key.F10 && modifierKeys == ModifierKeys.Shift)
			{
				RaiseContextMenuOpeningEvent(e);
			}
			_lastCtrlKeyDown = Key.None;
			ToolTip currentToolTip = CurrentToolTip;
			if (currentToolTip != null && currentToolTip.FromKeyboard && modifierKeys == ModifierKeys.Control && (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl))
			{
				_lastCtrlKeyDown = e.Key;
			}
		}
	}

	private void ProcessKeyUp(object sender, KeyEventArgs e)
	{
		if (e.Handled)
		{
			return;
		}
		if (e.Key == Key.Apps)
		{
			RaiseContextMenuOpeningEvent(e);
		}
		if (_lastCtrlKeyDown != 0 && e.Key == _lastCtrlKeyDown && Keyboard.Modifiers == ModifierKeys.None)
		{
			ToolTip currentToolTip = CurrentToolTip;
			if (currentToolTip != null && currentToolTip.FromKeyboard)
			{
				DismissCurrentToolTip();
			}
		}
		_lastCtrlKeyDown = Key.None;
	}

	private bool OpenOrCloseToolTipViaShortcut()
	{
		DependencyObject dependencyObject = FindToolTipOwner(Keyboard.FocusedElement, ToolTipService.TriggerAction.KeyboardShortcut);
		if (dependencyObject == null)
		{
			return false;
		}
		if (dependencyObject == GetOwner(CurrentToolTip))
		{
			DismissCurrentToolTip();
		}
		else
		{
			if (dependencyObject == GetOwner(PendingToolTip))
			{
				DismissPendingToolTip();
			}
			BeginShowToolTip(dependencyObject, ToolTipService.TriggerAction.KeyboardShortcut);
		}
		return true;
	}

	private void BeginShowToolTip(DependencyObject o, ToolTipService.TriggerAction triggerAction)
	{
		if (triggerAction == ToolTipService.TriggerAction.Mouse)
		{
			if (o == LastMouseToolTipOwner)
			{
				return;
			}
			LastMouseToolTipOwner = o;
			if (PendingToolTip != null && !PendingToolTip.FromKeyboard && o != GetOwner(PendingToolTip))
			{
				DismissPendingToolTip();
			}
		}
		if (o == null || o == GetOwner(PendingToolTip) || o == GetOwner(CurrentToolTip))
		{
			return;
		}
		DismissPendingToolTip();
		PendingToolTip = SentinelToolTip(o, triggerAction);
		bool flag = false;
		bool flag2 = _quickShow;
		if (!flag2)
		{
			ToolTip toolTip = CurrentToolTip;
			switch (triggerAction)
			{
			case ToolTipService.TriggerAction.Mouse:
				if (SafeArea != null)
				{
					flag = true;
				}
				break;
			case ToolTipService.TriggerAction.KeyboardFocus:
				flag2 = toolTip?.FromKeyboard ?? false;
				break;
			default:
				toolTip = null;
				flag2 = true;
				break;
			}
			if (toolTip != null && (flag2 || flag) && ToolTipService.GetBetweenShowDelay(GetOwner(toolTip)) == 0)
			{
				flag2 = false;
				flag = false;
			}
		}
		int num = ((!flag2) ? (flag ? ShortDelay : ToolTipService.GetInitialShowDelay(o)) : 0);
		if (num == 0)
		{
			PromotePendingToolTipToCurrent(triggerAction);
			return;
		}
		PendingToolTipTimer = new DispatcherTimer(DispatcherPriority.Normal);
		PendingToolTipTimer.Interval = TimeSpan.FromMilliseconds(num);
		PendingToolTipTimer.Tick += delegate
		{
			PromotePendingToolTipToCurrent(triggerAction);
		};
		PendingToolTipTimer.Tag = BooleanBoxes.Box(flag);
		PendingToolTipTimer.Start();
	}

	private void PromotePendingToolTipToCurrent(ToolTipService.TriggerAction triggerAction)
	{
		DependencyObject owner = GetOwner(PendingToolTip);
		DismissToolTips();
		if (owner != null)
		{
			ShowToolTip(owner, ToolTipService.IsFromKeyboard(triggerAction));
		}
	}

	private void ShowToolTip(DependencyObject o, bool fromKeyboard)
	{
		ResetCurrentToolTipTimer();
		OnForceClose(null, EventArgs.Empty);
		bool flag = true;
		if (o is IInputElement inputElement)
		{
			ToolTipEventArgs toolTipEventArgs = new ToolTipEventArgs(opening: true);
			inputElement.RaiseEvent(toolTipEventArgs);
			flag = !toolTipEventArgs.Handled && _currentToolTip == null;
		}
		if (flag)
		{
			if (ToolTipService.GetToolTip(o) is ToolTip currentToolTip)
			{
				_currentToolTip = currentToolTip;
			}
			else
			{
				_currentToolTip = new ToolTip();
				_currentToolTip.SetValue(ServiceOwnedProperty, BooleanBoxes.TrueBox);
				Binding binding = new Binding();
				binding.Path = new PropertyPath(ToolTipService.ToolTipProperty);
				binding.Mode = BindingMode.OneWay;
				binding.Source = o;
				_currentToolTip.SetBinding(ContentControl.ContentProperty, binding);
			}
			if (!_currentToolTip.StaysOpen)
			{
				throw new NotSupportedException(SR.ToolTipStaysOpenFalseNotAllowed);
			}
			_currentToolTip.SetValue(OwnerProperty, o);
			_currentToolTip.Closed += OnToolTipClosed;
			_currentToolTip.FromKeyboard = fromKeyboard;
			if (!_currentToolTip.IsOpen)
			{
				_currentToolTip.Opened += OnToolTipOpened;
				_currentToolTip.IsOpen = true;
			}
			else
			{
				SetSafeArea(_currentToolTip);
			}
			CurrentToolTipTimer = new DispatcherTimer(DispatcherPriority.Normal);
			CurrentToolTipTimer.Interval = TimeSpan.FromMilliseconds(ToolTipService.GetShowDuration(o));
			CurrentToolTipTimer.Tick += OnShowDurationTimerExpired;
			CurrentToolTipTimer.Start();
		}
	}

	private void OnShowDurationTimerExpired(object sender, EventArgs e)
	{
		DismissCurrentToolTip();
	}

	internal void ReplaceCurrentToolTip()
	{
		ToolTip currentToolTip = _currentToolTip;
		if (currentToolTip != null)
		{
			DependencyObject owner = GetOwner(currentToolTip);
			bool fromKeyboard = currentToolTip.FromKeyboard;
			DismissCurrentToolTip();
			ShowToolTip(owner, fromKeyboard);
		}
	}

	internal void DismissToolTipsForOwner(DependencyObject o)
	{
		if (o == GetOwner(PendingToolTip))
		{
			DismissPendingToolTip();
		}
		if (o == GetOwner(CurrentToolTip))
		{
			DismissCurrentToolTip();
		}
	}

	private void DismissToolTips()
	{
		DismissPendingToolTip();
		DismissCurrentToolTip();
	}

	private void DismissKeyboardToolTips()
	{
		ToolTip pendingToolTip = PendingToolTip;
		if (pendingToolTip != null && pendingToolTip.FromKeyboard)
		{
			DismissPendingToolTip();
		}
		ToolTip currentToolTip = CurrentToolTip;
		if (currentToolTip != null && currentToolTip.FromKeyboard)
		{
			DismissCurrentToolTip();
		}
	}

	private void DismissPendingToolTip()
	{
		if (PendingToolTipTimer != null)
		{
			PendingToolTipTimer.Stop();
			PendingToolTipTimer = null;
		}
		if (PendingToolTip != null)
		{
			PendingToolTip = null;
			_sentinelToolTip.SetValue(OwnerProperty, null);
		}
	}

	private void DismissCurrentToolTip()
	{
		ToolTip currentToolTip = _currentToolTip;
		_currentToolTip = null;
		CloseToolTip(currentToolTip);
	}

	private void CloseToolTip(ToolTip tooltip)
	{
		if (tooltip == null)
		{
			return;
		}
		SetSafeArea(null);
		ResetCurrentToolTipTimer();
		DependencyObject owner = GetOwner(tooltip);
		try
		{
			if (tooltip.IsOpen && owner is IInputElement inputElement)
			{
				inputElement.RaiseEvent(new ToolTipEventArgs(opening: false));
			}
		}
		finally
		{
			if (tooltip.IsOpen)
			{
				tooltip.IsOpen = false;
				_forceCloseTimer = new DispatcherTimer(DispatcherPriority.Normal);
				_forceCloseTimer.Interval = Popup.AnimationDelayTime;
				_forceCloseTimer.Tick += OnForceClose;
				_forceCloseTimer.Tag = tooltip;
				_forceCloseTimer.Start();
				int betweenShowDelay = ToolTipService.GetBetweenShowDelay(owner);
				_quickShow = betweenShowDelay > 0;
				if (_quickShow)
				{
					CurrentToolTipTimer = new DispatcherTimer(DispatcherPriority.Normal);
					CurrentToolTipTimer.Interval = TimeSpan.FromMilliseconds(betweenShowDelay);
					CurrentToolTipTimer.Tick += OnBetweenShowDelay;
					CurrentToolTipTimer.Start();
				}
			}
			else
			{
				ClearServiceProperties(tooltip);
			}
		}
	}

	private void ClearServiceProperties(ToolTip tooltip)
	{
		if (tooltip != null && !tooltip.IsOpen)
		{
			tooltip.ClearValue(OwnerProperty);
			tooltip.FromKeyboard = false;
			tooltip.Closed -= OnToolTipClosed;
			if ((bool)tooltip.GetValue(ServiceOwnedProperty))
			{
				BindingOperations.ClearBinding(tooltip, ContentControl.ContentProperty);
			}
		}
	}

	private DependencyObject FindToolTipOwner(IInputElement element, ToolTipService.TriggerAction triggerAction)
	{
		if (element == null)
		{
			return null;
		}
		DependencyObject dependencyObject = null;
		switch (triggerAction)
		{
		case ToolTipService.TriggerAction.Mouse:
		{
			FindToolTipEventArgs findToolTipEventArgs = new FindToolTipEventArgs(triggerAction);
			element.RaiseEvent(findToolTipEventArgs);
			dependencyObject = findToolTipEventArgs.TargetElement;
			break;
		}
		case ToolTipService.TriggerAction.KeyboardFocus:
		case ToolTipService.TriggerAction.KeyboardShortcut:
			dependencyObject = element as DependencyObject;
			if (dependencyObject != null && !ToolTipService.ToolTipIsEnabled(dependencyObject, triggerAction))
			{
				dependencyObject = null;
			}
			break;
		}
		if (WithinCurrentToolTip(dependencyObject))
		{
			dependencyObject = null;
		}
		return dependencyObject;
	}

	private bool WithinCurrentToolTip(DependencyObject o)
	{
		if (_currentToolTip == null)
		{
			return false;
		}
		DependencyObject dependencyObject = o as Visual;
		if (dependencyObject == null)
		{
			dependencyObject = ((!(o is ContentElement ce)) ? (o as Visual3D) : FindContentElementParent(ce));
		}
		if (dependencyObject != null)
		{
			if (!(dependencyObject is Visual) || !((Visual)dependencyObject).IsDescendantOf(_currentToolTip))
			{
				if (dependencyObject is Visual3D)
				{
					return ((Visual3D)dependencyObject).IsDescendantOf(_currentToolTip);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private void ResetCurrentToolTipTimer()
	{
		if (CurrentToolTipTimer != null)
		{
			CurrentToolTipTimer.Stop();
			CurrentToolTipTimer = null;
			_quickShow = false;
		}
	}

	private void OnToolTipOpened(object sender, EventArgs e)
	{
		ToolTip toolTip = (ToolTip)sender;
		toolTip.Opened -= OnToolTipOpened;
		SetSafeArea(toolTip);
	}

	private void OnToolTipClosed(object sender, EventArgs e)
	{
		ToolTip toolTip = (ToolTip)sender;
		if (toolTip != CurrentToolTip)
		{
			ClearServiceProperties(toolTip);
		}
	}

	private void OnForceClose(object sender, EventArgs e)
	{
		if (_forceCloseTimer != null)
		{
			_forceCloseTimer.Stop();
			ToolTip toolTip = (ToolTip)_forceCloseTimer.Tag;
			toolTip.ForceClose();
			ClearServiceProperties(toolTip);
			_forceCloseTimer = null;
		}
	}

	private void OnBetweenShowDelay(object source, EventArgs e)
	{
		ResetCurrentToolTipTimer();
	}

	private DependencyObject GetOwner(ToolTip t)
	{
		return t?.GetValue(OwnerProperty) as DependencyObject;
	}

	private ToolTip SentinelToolTip(DependencyObject o, ToolTipService.TriggerAction triggerAction)
	{
		if (_sentinelToolTip == null)
		{
			_sentinelToolTip = new ToolTip();
		}
		_sentinelToolTip.SetValue(OwnerProperty, o);
		_sentinelToolTip.FromKeyboard = ToolTipService.IsFromKeyboard(triggerAction);
		return _sentinelToolTip;
	}

	private void SetSafeArea(ToolTip tooltip)
	{
		SafeArea = null;
		if (tooltip == null || tooltip.FromKeyboard)
		{
			return;
		}
		DependencyObject owner = GetOwner(tooltip);
		PresentationSource presentationSource = ((owner != null) ? PresentationSource.CriticalFromVisual(owner) : null);
		if (presentationSource == null)
		{
			return;
		}
		List<MS.Win32.NativeMethods.RECT> list = new List<MS.Win32.NativeMethods.RECT>();
		if (owner is UIElement uIElement)
		{
			Rect rect = PointUtil.RootToClient(PointUtil.ElementToRoot(new Rect(new Point(0.0, 0.0), uIElement.RenderSize), uIElement, presentationSource), presentationSource);
			if (!rect.IsEmpty)
			{
				list.Add(PointUtil.FromRect(rect));
			}
		}
		else if (owner is ContentElement contentElement)
		{
			IContentHost ichParent = null;
			UIElement parentUIElementFromContentElement = KeyboardNavigation.GetParentUIElementFromContentElement(contentElement, ref ichParent);
			if (ichParent is Visual visual && parentUIElementFromContentElement != null)
			{
				ReadOnlyCollection<Rect> rectangles = ichParent.GetRectangles(contentElement);
				GeneralTransform generalTransform = visual.TransformToAncestor(presentationSource.RootVisual);
				CompositionTarget compositionTarget = presentationSource.CompositionTarget;
				Matrix visualTransform = PointUtil.GetVisualTransform(compositionTarget.RootVisual);
				Matrix transformToDevice = compositionTarget.TransformToDevice;
				foreach (Rect item in (IEnumerable<Rect>)rectangles)
				{
					Rect rect2 = Rect.Transform(Rect.Transform(generalTransform.TransformBounds(item), visualTransform), transformToDevice);
					list.Add(PointUtil.FromRect(rect2));
				}
			}
		}
		Rect screenRect = tooltip.GetScreenRect();
		Point location = PointUtil.ScreenToClient(screenRect.Location, presentationSource);
		Rect rect3 = new Rect(location, screenRect.Size);
		if (!rect3.IsEmpty)
		{
			list.Add(PointUtil.FromRect(rect3));
		}
		SafeArea = new ConvexHull(presentationSource, list);
	}

	private bool MouseHasLeftSafeArea()
	{
		if (SafeArea == null)
		{
			return false;
		}
		DependencyObject owner = GetOwner(CurrentToolTip);
		if (((owner != null) ? PresentationSource.CriticalFromVisual(owner) : null) == null)
		{
			return true;
		}
		return !(SafeArea?.ContainsMousePoint() ?? true);
	}

	private void RaiseContextMenuOpeningEvent(KeyEventArgs e)
	{
		if (e.OriginalSource is IInputElement source && RaiseContextMenuOpeningEvent(source, -1.0, -1.0, e.UserInitiated))
		{
			e.Handled = true;
		}
	}

	private bool RaiseContextMenuOpeningEvent(IInputElement source, double x, double y, bool userInitiated)
	{
		ContextMenuEventArgs contextMenuEventArgs = new ContextMenuEventArgs(source, opening: true, x, y);
		DependencyObject dependencyObject = source as DependencyObject;
		if (userInitiated && dependencyObject != null)
		{
			if (dependencyObject is UIElement uIElement)
			{
				uIElement.RaiseEvent(contextMenuEventArgs, userInitiated);
			}
			else if (dependencyObject is ContentElement contentElement)
			{
				contentElement.RaiseEvent(contextMenuEventArgs, userInitiated);
			}
			else if (dependencyObject is UIElement3D uIElement3D)
			{
				uIElement3D.RaiseEvent(contextMenuEventArgs, userInitiated);
			}
			else
			{
				source.RaiseEvent(contextMenuEventArgs);
			}
		}
		else
		{
			source.RaiseEvent(contextMenuEventArgs);
		}
		if (!contextMenuEventArgs.Handled)
		{
			DependencyObject targetElement = contextMenuEventArgs.TargetElement;
			if (targetElement != null && ContextMenuService.ContextMenuIsEnabled(targetElement))
			{
				ContextMenu contextMenu = ContextMenuService.GetContextMenu(targetElement) as ContextMenu;
				contextMenu.SetValue(OwnerProperty, targetElement);
				contextMenu.Closed += OnContextMenuClosed;
				if (x == -1.0 && y == -1.0)
				{
					contextMenu.Placement = PlacementMode.Center;
				}
				else
				{
					contextMenu.Placement = PlacementMode.MousePoint;
				}
				DismissToolTips();
				contextMenu.SetCurrentValueInternal(ContextMenu.IsOpenProperty, BooleanBoxes.TrueBox);
				return true;
			}
			return false;
		}
		DismissToolTips();
		return true;
	}

	private void OnContextMenuClosed(object source, RoutedEventArgs e)
	{
		if (!(source is ContextMenu contextMenu))
		{
			return;
		}
		contextMenu.Closed -= OnContextMenuClosed;
		DependencyObject dependencyObject = (DependencyObject)contextMenu.GetValue(OwnerProperty);
		if (dependencyObject == null)
		{
			return;
		}
		contextMenu.ClearValue(OwnerProperty);
		UIElement target = GetTarget(dependencyObject);
		if (target != null && !IsPresentationSourceNull(target))
		{
			object obj;
			if (!(dependencyObject is ContentElement) && !(dependencyObject is UIElement3D))
			{
				IInputElement inputElement = target;
				obj = inputElement;
			}
			else
			{
				obj = (IInputElement)dependencyObject;
			}
			ContextMenuEventArgs e2 = new ContextMenuEventArgs(obj, opening: false);
			((IInputElement)obj).RaiseEvent((RoutedEventArgs)e2);
		}
	}

	private static bool IsPresentationSourceNull(DependencyObject uie)
	{
		return PresentationSource.CriticalFromVisual(uie) == null;
	}

	internal static DependencyObject FindParent(DependencyObject o)
	{
		DependencyObject dependencyObject = o as Visual;
		if (dependencyObject == null)
		{
			dependencyObject = o as Visual3D;
		}
		ContentElement contentElement = ((dependencyObject == null) ? (o as ContentElement) : null);
		if (contentElement != null)
		{
			o = ContentOperations.GetParent(contentElement);
			if (o != null)
			{
				return o;
			}
			if (contentElement is FrameworkContentElement frameworkContentElement)
			{
				return frameworkContentElement.Parent;
			}
		}
		else if (dependencyObject != null)
		{
			return VisualTreeHelper.GetParent(dependencyObject);
		}
		return null;
	}

	internal static DependencyObject FindContentElementParent(ContentElement ce)
	{
		DependencyObject dependencyObject = null;
		DependencyObject dependencyObject2 = ce;
		while (dependencyObject2 != null)
		{
			dependencyObject = dependencyObject2 as Visual;
			if (dependencyObject != null)
			{
				break;
			}
			dependencyObject = dependencyObject2 as Visual3D;
			if (dependencyObject != null)
			{
				break;
			}
			ce = dependencyObject2 as ContentElement;
			if (ce == null)
			{
				break;
			}
			dependencyObject2 = ContentOperations.GetParent(ce);
			if (dependencyObject2 == null && ce is FrameworkContentElement frameworkContentElement)
			{
				dependencyObject2 = frameworkContentElement.Parent;
			}
		}
		return dependencyObject;
	}

	internal static bool IsElementEnabled(DependencyObject o)
	{
		bool result = true;
		UIElement uIElement = o as UIElement;
		ContentElement contentElement = ((uIElement == null) ? (o as ContentElement) : null);
		UIElement3D uIElement3D = ((uIElement == null && contentElement == null) ? (o as UIElement3D) : null);
		if (uIElement != null)
		{
			result = uIElement.IsEnabled;
		}
		else if (contentElement != null)
		{
			result = contentElement.IsEnabled;
		}
		else if (uIElement3D != null)
		{
			result = uIElement3D.IsEnabled;
		}
		return result;
	}

	private static UIElement GetTarget(DependencyObject o)
	{
		UIElement uIElement = o as UIElement;
		if (uIElement == null)
		{
			if (o is ContentElement ce)
			{
				DependencyObject dependencyObject = FindContentElementParent(ce);
				uIElement = dependencyObject as UIElement;
				if (uIElement == null && dependencyObject is UIElement3D reference)
				{
					uIElement = UIElementHelper.GetContainingUIElement2D(reference);
				}
			}
			else if (o is UIElement3D reference2)
			{
				uIElement = UIElementHelper.GetContainingUIElement2D(reference2);
			}
		}
		return uIElement;
	}

	private static void OnOwnerChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
	{
		if (o is ContextMenu)
		{
			o.CoerceValue(ContextMenu.HorizontalOffsetProperty);
			o.CoerceValue(ContextMenu.VerticalOffsetProperty);
			o.CoerceValue(ContextMenu.PlacementTargetProperty);
			o.CoerceValue(ContextMenu.PlacementRectangleProperty);
			o.CoerceValue(ContextMenu.PlacementProperty);
			o.CoerceValue(ContextMenu.HasDropShadowProperty);
		}
		else if (o is ToolTip)
		{
			o.CoerceValue(ToolTip.HorizontalOffsetProperty);
			o.CoerceValue(ToolTip.VerticalOffsetProperty);
			o.CoerceValue(ToolTip.PlacementTargetProperty);
			o.CoerceValue(ToolTip.PlacementRectangleProperty);
			o.CoerceValue(ToolTip.PlacementProperty);
			o.CoerceValue(ToolTip.HasDropShadowProperty);
		}
	}

	internal static object CoerceProperty(DependencyObject o, object value, DependencyProperty dp)
	{
		DependencyObject dependencyObject = (DependencyObject)o.GetValue(OwnerProperty);
		if (dependencyObject != null)
		{
			if (dependencyObject.GetValueSource(dp, null, out var hasModifiers) != BaseValueSourceInternal.Default || hasModifiers)
			{
				return dependencyObject.GetValue(dp);
			}
			if (dp == ToolTip.PlacementTargetProperty || dp == ContextMenu.PlacementTargetProperty)
			{
				UIElement target = GetTarget(dependencyObject);
				if (target != null)
				{
					return target;
				}
			}
		}
		return value;
	}
}
