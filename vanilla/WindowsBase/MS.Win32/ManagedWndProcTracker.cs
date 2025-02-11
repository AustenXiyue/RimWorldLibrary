using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using MS.Internal;
using MS.Internal.Interop;

namespace MS.Win32;

internal static class ManagedWndProcTracker
{
	private sealed class ManagedWndProcTrackerShutDownListener : ShutDownListener
	{
		public ManagedWndProcTrackerShutDownListener()
			: base(null, ShutDownEvents.AppDomain)
		{
		}

		internal override void OnShutDown(object target, object sender, EventArgs e)
		{
			OnAppDomainProcessExit();
		}
	}

	private static nint _cachedDefWindowProcA;

	private static nint _cachedDefWindowProcW;

	private static Hashtable _hwndList;

	private static bool _exiting;

	static ManagedWndProcTracker()
	{
		_cachedDefWindowProcA = IntPtr.Zero;
		_cachedDefWindowProcW = IntPtr.Zero;
		_hwndList = new Hashtable(10);
		_exiting = false;
		new ManagedWndProcTrackerShutDownListener();
	}

	internal static void TrackHwndSubclass(HwndSubclass subclass, nint hwnd)
	{
		lock (_hwndList)
		{
			_hwndList[subclass] = hwnd;
		}
	}

	internal static void UnhookHwndSubclass(HwndSubclass subclass)
	{
		if (_exiting)
		{
			return;
		}
		lock (_hwndList)
		{
			_hwndList.Remove(subclass);
		}
	}

	private static void OnAppDomainProcessExit()
	{
		_exiting = true;
		lock (_hwndList)
		{
			foreach (DictionaryEntry hwnd in _hwndList)
			{
				nint num = (nint)hwnd.Value;
				if ((UnsafeNativeMethods.GetWindowLong(new HandleRef(null, num), -16) & 0x40000000) != 0)
				{
					UnsafeNativeMethods.SendMessage(num, HwndSubclass.DetachMessage, IntPtr.Zero, 2);
				}
				HookUpDefWindowProc(num);
			}
		}
	}

	private static void HookUpDefWindowProc(nint hwnd)
	{
		nint num = IntPtr.Zero;
		if (hwnd == IntPtr.Zero)
		{
			return;
		}
		nint defWindowProcAddress = GetDefWindowProcAddress(hwnd);
		if (defWindowProcAddress == IntPtr.Zero)
		{
			return;
		}
		try
		{
			num = UnsafeNativeMethods.SetWindowLong(new HandleRef(null, hwnd), -4, defWindowProcAddress);
		}
		catch (Win32Exception ex)
		{
			if (ex.NativeErrorCode != 1400)
			{
				throw;
			}
		}
		if (num != IntPtr.Zero)
		{
			UnsafeNativeMethods.PostMessage(new HandleRef(null, hwnd), WindowMessage.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
		}
	}

	private static nint GetDefWindowProcAddress(nint hwnd)
	{
		if (SafeNativeMethods.IsWindowUnicode(new HandleRef(null, hwnd)))
		{
			if (_cachedDefWindowProcW == IntPtr.Zero)
			{
				_cachedDefWindowProcW = GetUser32ProcAddress("DefWindowProcW");
			}
			return _cachedDefWindowProcW;
		}
		if (_cachedDefWindowProcA == IntPtr.Zero)
		{
			_cachedDefWindowProcA = GetUser32ProcAddress("DefWindowProcA");
		}
		return _cachedDefWindowProcA;
	}

	private static nint GetUser32ProcAddress(string export)
	{
		nint moduleHandle = UnsafeNativeMethods.GetModuleHandle("user32.dll");
		if (moduleHandle != IntPtr.Zero)
		{
			return UnsafeNativeMethods.GetProcAddress(new HandleRef(null, moduleHandle), export);
		}
		return IntPtr.Zero;
	}
}
