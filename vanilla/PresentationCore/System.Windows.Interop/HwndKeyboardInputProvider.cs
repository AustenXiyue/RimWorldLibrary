using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Interop;
using MS.Internal.PresentationCore;
using MS.Utility;
using MS.Win32;

namespace System.Windows.Interop;

internal sealed class HwndKeyboardInputProvider : DispatcherObject, IKeyboardInputProvider, IInputProvider, IDisposable
{
	private int _msgTime;

	private SecurityCriticalDataClass<HwndSource> _source;

	private SecurityCriticalDataClass<InputProviderSite> _site;

	private IInputElement _restoreFocus;

	private nint _restoreFocusWindow;

	private bool _active;

	private bool _partialActive;

	private bool _acquiringFocusOurselves;

	internal HwndKeyboardInputProvider(HwndSource source)
	{
		_site = new SecurityCriticalDataClass<InputProviderSite>(InputManager.Current.RegisterInputProvider(this));
		_source = new SecurityCriticalDataClass<HwndSource>(source);
	}

	public void Dispose()
	{
		if (_site != null)
		{
			_site.Value.Dispose();
			_site = null;
		}
		_source = null;
	}

	public void OnRootChanged(Visual oldRoot, Visual newRoot)
	{
		if (_active && newRoot != null)
		{
			Keyboard.Focus(null);
		}
	}

	bool IInputProvider.ProvidesInputForRootVisual(Visual v)
	{
		return _source.Value.RootVisual == v;
	}

	void IInputProvider.NotifyDeactivate()
	{
		_active = false;
		_partialActive = false;
	}

	bool IKeyboardInputProvider.AcquireFocus(bool checkOnly)
	{
		bool result = false;
		try
		{
			if (!checkOnly)
			{
				_acquiringFocusOurselves = true;
				_restoreFocusWindow = IntPtr.Zero;
				_restoreFocus = null;
			}
			HandleRef hWnd = new HandleRef(this, _source.Value.CriticalHandle);
			nint focus = MS.Win32.UnsafeNativeMethods.GetFocus();
			if ((MS.Win32.UnsafeNativeMethods.GetWindowLong(hWnd, -20) & 0x8000000) == 134217728 || _source.Value.IsInExclusiveMenuMode)
			{
				if (SafeNativeMethods.IsWindowEnabled(hWnd))
				{
					result = focus != IntPtr.Zero;
				}
			}
			else
			{
				if (!checkOnly)
				{
					if (!_active && focus == _source.Value.CriticalHandle)
					{
						OnSetFocus(focus);
					}
					else
					{
						MS.Win32.UnsafeNativeMethods.TrySetFocus(hWnd);
						focus = MS.Win32.UnsafeNativeMethods.GetFocus();
					}
				}
				result = focus == _source.Value.CriticalHandle;
			}
		}
		catch (Win32Exception)
		{
		}
		finally
		{
			_acquiringFocusOurselves = false;
		}
		return result;
	}

	internal nint FilterMessage(nint hwnd, WindowMessage message, nint wParam, nint lParam, ref bool handled)
	{
		nint zero = IntPtr.Zero;
		if (_source == null || _source.Value == null)
		{
			return zero;
		}
		_msgTime = 0;
		try
		{
			_msgTime = SafeNativeMethods.GetMessageTime();
		}
		catch (Win32Exception)
		{
		}
		switch (message)
		{
		case WindowMessage.WM_KEYFIRST:
		case WindowMessage.WM_SYSKEYDOWN:
			if (!_source.Value.IsRepeatedKeyboardMessage(hwnd, (int)message, wParam, lParam))
			{
				try
				{
					SafeNativeMethods.GetTickCount();
				}
				catch (Win32Exception)
				{
				}
				HwndSource._eatCharMessages = true;
				DispatcherOperation dispatcherOperation = base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(HwndSource.RestoreCharMessages), null);
				base.Dispatcher.CriticalRequestProcessing(force: true);
				MSG msg2 = new MSG(hwnd, (int)message, wParam, lParam, _msgTime, 0, 0);
				ProcessKeyAction(ref msg2, ref handled);
				if (!handled)
				{
					HwndSource._eatCharMessages = false;
					dispatcherOperation.Abort();
				}
			}
			break;
		case WindowMessage.WM_KEYUP:
		case WindowMessage.WM_SYSKEYUP:
			if (!_source.Value.IsRepeatedKeyboardMessage(hwnd, (int)message, wParam, lParam))
			{
				MSG msg = new MSG(hwnd, (int)message, wParam, lParam, _msgTime, 0, 0);
				ProcessKeyAction(ref msg, ref handled);
			}
			break;
		case WindowMessage.WM_CHAR:
		case WindowMessage.WM_DEADCHAR:
		case WindowMessage.WM_SYSCHAR:
		case WindowMessage.WM_SYSDEADCHAR:
			if (!_source.Value.IsRepeatedKeyboardMessage(hwnd, (int)message, wParam, lParam) && !HwndSource._eatCharMessages)
			{
				ProcessTextInputAction(hwnd, message, wParam, lParam, ref handled);
			}
			break;
		case WindowMessage.WM_EXITMENULOOP:
		case WindowMessage.WM_EXITSIZEMOVE:
			if (_active)
			{
				_partialActive = true;
				ReportInput(hwnd, InputMode.Foreground, _msgTime, RawKeyboardActions.Activate, 0, isExtendedKey: false, isSystemKey: false, 0);
			}
			break;
		case WindowMessage.WM_SETFOCUS:
			OnSetFocus(hwnd);
			handled = true;
			break;
		case WindowMessage.WM_KILLFOCUS:
			if (_active && wParam != _source.Value.CriticalHandle)
			{
				if (_source.Value.RestoreFocusMode == RestoreFocusMode.Auto)
				{
					_restoreFocusWindow = GetImmediateChildFor(wParam, _source.Value.CriticalHandle);
					_restoreFocus = null;
					if (_restoreFocusWindow == IntPtr.Zero && Keyboard.FocusedElement is DependencyObject dependencyObject && PresentationSource.CriticalFromVisual(dependencyObject) as HwndSource == _source.Value)
					{
						_restoreFocus = dependencyObject as IInputElement;
					}
				}
				PossiblyDeactivate(wParam);
			}
			handled = true;
			break;
		case WindowMessage.WM_UPDATEUISTATE:
		{
			RawUIStateInputReport inputReport = new RawUIStateInputReport(_source.Value, InputMode.Foreground, _msgTime, (RawUIStateActions)MS.Win32.NativeMethods.SignedLOWORD((int)wParam), (RawUIStateTargets)MS.Win32.NativeMethods.SignedHIWORD((int)wParam));
			_site.Value.ReportInput(inputReport);
			handled = true;
			break;
		}
		}
		if (handled && EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordInput, EventTrace.Level.Info))
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientInputMessage, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordInput, EventTrace.Level.Info, base.Dispatcher.GetHashCode(), ((IntPtr)hwnd).ToInt64(), message, (int)wParam, (int)lParam);
		}
		return zero;
	}

	private void OnSetFocus(nint hwnd)
	{
		_active = false;
		if (_active)
		{
			return;
		}
		HwndSource value = _source.Value;
		ReportInput(hwnd, InputMode.Foreground, _msgTime, RawKeyboardActions.Activate, 0, isExtendedKey: false, isSystemKey: false, 0);
		_partialActive = true;
		if (_acquiringFocusOurselves || value.RestoreFocusMode != 0)
		{
			return;
		}
		if (_restoreFocusWindow != IntPtr.Zero)
		{
			nint result = _restoreFocusWindow;
			_restoreFocusWindow = IntPtr.Zero;
			MS.Win32.UnsafeNativeMethods.TrySetFocus(new HandleRef(this, result), ref result);
			return;
		}
		DependencyObject dependencyObject = _restoreFocus as DependencyObject;
		_restoreFocus = null;
		if (dependencyObject != null && PresentationSource.CriticalFromVisual(dependencyObject) as HwndSource != value)
		{
			dependencyObject = null;
		}
		Keyboard.Focus(dependencyObject as IInputElement);
		if (MS.Win32.UnsafeNativeMethods.GetFocus() == value.CriticalHandle)
		{
			dependencyObject = (DependencyObject)Keyboard.FocusedElement;
			if (dependencyObject != null && PresentationSource.CriticalFromVisual(dependencyObject) as HwndSource != value)
			{
				Keyboard.ClearFocus();
			}
		}
	}

	internal void ProcessKeyAction(ref MSG msg, ref bool handled)
	{
		MSG unsecureCurrentKeyboardMessage = ComponentDispatcher.UnsecureCurrentKeyboardMessage;
		ComponentDispatcher.UnsecureCurrentKeyboardMessage = msg;
		try
		{
			int virtualKey = GetVirtualKey(msg.wParam, msg.lParam);
			int scanCode = GetScanCode(msg.wParam, msg.lParam);
			bool isExtendedKey = IsExtendedKey(msg.lParam);
			bool isSystemKey = msg.message == 260 || msg.message == 261;
			RawKeyboardActions keyUpKeyDown = GetKeyUpKeyDown((WindowMessage)msg.message);
			handled = ReportInput(msg.hwnd, InputMode.Foreground, _msgTime, keyUpKeyDown, scanCode, isExtendedKey, isSystemKey, virtualKey);
		}
		finally
		{
			ComponentDispatcher.UnsecureCurrentKeyboardMessage = unsecureCurrentKeyboardMessage;
		}
	}

	internal void ProcessTextInputAction(nint hwnd, WindowMessage msg, nint wParam, nint lParam, ref bool handled)
	{
		char c = (char)wParam;
		bool isDeadCharacter = msg == WindowMessage.WM_DEADCHAR || msg == WindowMessage.WM_SYSDEADCHAR;
		bool isSystemCharacter = msg == WindowMessage.WM_SYSCHAR || msg == WindowMessage.WM_SYSDEADCHAR;
		bool isControlCharacter = false;
		try
		{
			if ((MS.Win32.UnsafeNativeMethods.GetKeyState(17) & 0x8000) != 0 && (MS.Win32.UnsafeNativeMethods.GetKeyState(18) & 0x8000) == 0 && char.IsControl(c))
			{
				isControlCharacter = true;
			}
		}
		catch (Win32Exception)
		{
		}
		RawTextInputReport inputReport = new RawTextInputReport(_source.Value, InputMode.Foreground, _msgTime, isDeadCharacter, isSystemCharacter, isControlCharacter, c);
		handled = _site.Value.ReportInput(inputReport);
	}

	internal static int GetVirtualKey(nint wParam, nint lParam)
	{
		int num = MS.Win32.NativeMethods.IntPtrToInt32(wParam);
		int num2 = 0;
		int num3 = MS.Win32.NativeMethods.IntPtrToInt32(lParam);
		if (num == 16)
		{
			num2 = (num3 & 0xFF0000) >> 16;
			try
			{
				num = SafeNativeMethods.MapVirtualKey(num2, 3);
				if (num == 0)
				{
					num = 160;
				}
			}
			catch (Win32Exception)
			{
				num = 160;
			}
		}
		if (num == 18)
		{
			num = (((num3 & 0x1000000) >> 24 == 0) ? 164 : 165);
		}
		if (num == 17)
		{
			num = (((num3 & 0x1000000) >> 24 == 0) ? 162 : 163);
		}
		return num;
	}

	internal static int GetScanCode(nint wParam, nint lParam)
	{
		int num = (MS.Win32.NativeMethods.IntPtrToInt32(lParam) & 0xFF0000) >> 16;
		if (num == 0)
		{
			try
			{
				num = SafeNativeMethods.MapVirtualKey(GetVirtualKey(wParam, lParam), 0);
			}
			catch (Win32Exception)
			{
			}
		}
		return num;
	}

	internal static bool IsExtendedKey(nint lParam)
	{
		return (MS.Win32.NativeMethods.IntPtrToInt32(lParam) & 0x1000000) != 0;
	}

	[FriendAccessAllowed]
	internal static ModifierKeys GetSystemModifierKeys()
	{
		ModifierKeys modifierKeys = ModifierKeys.None;
		if ((MS.Win32.UnsafeNativeMethods.GetKeyState(16) & 0x8000) == 32768)
		{
			modifierKeys |= ModifierKeys.Shift;
		}
		if ((MS.Win32.UnsafeNativeMethods.GetKeyState(17) & 0x8000) == 32768)
		{
			modifierKeys |= ModifierKeys.Control;
		}
		if ((MS.Win32.UnsafeNativeMethods.GetKeyState(18) & 0x8000) == 32768)
		{
			modifierKeys |= ModifierKeys.Alt;
		}
		return modifierKeys;
	}

	private RawKeyboardActions GetKeyUpKeyDown(WindowMessage msg)
	{
		switch (msg)
		{
		case WindowMessage.WM_KEYFIRST:
		case WindowMessage.WM_SYSKEYDOWN:
			return RawKeyboardActions.KeyDown;
		case WindowMessage.WM_KEYUP:
		case WindowMessage.WM_SYSKEYUP:
			return RawKeyboardActions.KeyUp;
		default:
			throw new ArgumentException(SR.OnlyAcceptsKeyMessages);
		}
	}

	private void PossiblyDeactivate(nint hwndFocus)
	{
		bool num = !IsOurWindow(hwndFocus);
		_active = false;
		if (num)
		{
			ReportInput(_source.Value.CriticalHandle, InputMode.Foreground, _msgTime, RawKeyboardActions.Deactivate, 0, isExtendedKey: false, isSystemKey: false, 0);
		}
	}

	private bool IsOurWindow(nint hwnd)
	{
		bool flag = false;
		if (hwnd != IntPtr.Zero)
		{
			HwndSource hwndSource = HwndSource.CriticalFromHwnd(hwnd);
			if (hwndSource != null)
			{
				if (hwndSource.Dispatcher == _source.Value.Dispatcher)
				{
					return true;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	private nint GetImmediateChildFor(nint hwnd, nint hwndRoot)
	{
		while (hwnd != IntPtr.Zero && (MS.Win32.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, hwnd), -16) & 0x40000000) != 0)
		{
			nint parent = MS.Win32.UnsafeNativeMethods.GetParent(new HandleRef(this, hwnd));
			if (parent == hwndRoot)
			{
				return hwnd;
			}
			hwnd = parent;
		}
		return IntPtr.Zero;
	}

	private bool ReportInput(nint hwnd, InputMode mode, int timestamp, RawKeyboardActions actions, int scanCode, bool isExtendedKey, bool isSystemKey, int virtualKey)
	{
		if ((actions & RawKeyboardActions.Deactivate) == 0 && (!_active || _partialActive))
		{
			try
			{
				actions |= RawKeyboardActions.Activate;
				_active = true;
				_partialActive = false;
			}
			catch (Win32Exception)
			{
			}
		}
		nint extraInformation = IntPtr.Zero;
		try
		{
			extraInformation = MS.Win32.UnsafeNativeMethods.GetMessageExtraInfo();
		}
		catch (Win32Exception)
		{
		}
		RawKeyboardInputReport inputReport = new RawKeyboardInputReport(_source.Value, mode, timestamp, actions, scanCode, isExtendedKey, isSystemKey, virtualKey, extraInformation);
		return _site.Value.ReportInput(inputReport);
	}
}
