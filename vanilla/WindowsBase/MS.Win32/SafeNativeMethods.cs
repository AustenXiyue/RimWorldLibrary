using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using MS.Internal.WindowsBase;
using MS.Utility;

namespace MS.Win32;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal static class SafeNativeMethods
{
	private static class SafeNativeMethodsPrivate
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern int GetCurrentProcessId();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ProcessIdToSessionId([In] int dwProcessId, out int pSessionId);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern int GetCurrentThreadId();

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern nint GetCapture();

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern bool IsWindowVisible(HandleRef hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern int GetMessagePos();

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "ReleaseCapture", ExactSpelling = true, SetLastError = true)]
		public static extern bool IntReleaseCapture();

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowRect", ExactSpelling = true, SetLastError = true)]
		public static extern bool IntGetWindowRect(HandleRef hWnd, [In][Out] ref NativeMethods.RECT rect);

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetClientRect", ExactSpelling = true, SetLastError = true)]
		public static extern bool IntGetClientRect(HandleRef hWnd, [In][Out] ref NativeMethods.RECT rect);

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "AdjustWindowRectEx", ExactSpelling = true, SetLastError = true)]
		public static extern bool IntAdjustWindowRectEx(ref NativeMethods.RECT lpRect, int dwStyle, bool bMenu, int dwExStyle);

		[DllImport("user32.dll", ExactSpelling = true)]
		public static extern nint MonitorFromRect(ref NativeMethods.RECT rect, int flags);

		[DllImport("user32.dll", ExactSpelling = true)]
		public static extern nint MonitorFromPoint(NativeMethods.POINT pt, int flags);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern nint ActivateKeyboardLayout(HandleRef hkl, int uFlags);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern nint GetKeyboardLayout(int dwLayout);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
		public static extern nint SetTimer(HandleRef hWnd, int nIDEvent, int uElapse, NativeMethods.TimerProc lpTimerFunc);

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SetTimer")]
		public static extern nint TrySetTimer(HandleRef hWnd, int nIDEvent, int uElapse, NativeMethods.TimerProc lpTimerFunc);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern bool KillTimer(HandleRef hwnd, int idEvent);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern bool IsWindowUnicode(HandleRef hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern int GetDoubleClickTime();

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern bool IsWindowEnabled(HandleRef hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern nint GetCursor();

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern int ShowCursor(bool show);

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetMonitorInfo", SetLastError = true)]
		public static extern bool IntGetMonitorInfo(HandleRef hmonitor, [In][Out] NativeMethods.MONITORINFOEX info);

		[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern nint MonitorFromWindow(HandleRef handle, int flags);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		internal static extern int MapVirtualKey(int nVirtKey, int nMapType);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern nint SetCapture(HandleRef hwnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern nint SetCursor(HandleRef hcursor);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern nint SetCursor(SafeHandle hcursor);

		[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern bool TrackMouseEvent(NativeMethods.TRACKMOUSEEVENT tme);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern NativeMethods.CursorHandle LoadCursor(HandleRef hInst, nint iconId);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern int GetTickCount();

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "ScreenToClient", ExactSpelling = true, SetLastError = true)]
		public static extern int IntScreenToClient(HandleRef hWnd, ref NativeMethods.POINT pt);

		[DllImport("user32.dll")]
		public static extern int MessageBeep(int uType);

		[DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool WTSQuerySessionInformation([In] nint hServer, [In] int SessionId, [In] NativeMethods.WTS_INFO_CLASS WTSInfoClass, out nint ppBuffer, out int BytesReturned);

		[DllImport("wtsapi32.dll", CharSet = CharSet.Auto)]
		public static extern bool WTSFreeMemory([In] nint pMemory);

		[DllImport("shcore.dll")]
		internal static extern uint GetProcessDpiAwareness([In] HandleRef hProcess, out nint awareness);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool AdjustWindowRectExForDpi([In][Out] ref NativeMethods.RECT lpRect, [In] int dwStyle, [In][MarshalAs(UnmanagedType.Bool)] bool bMenu, [In] int dwExStyle, [In] int dpi);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool PhysicalToLogicalPointForPerMonitorDPI([In] HandleRef hWnd, ref NativeMethods.POINT lpPoint);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool LogicalToPhysicalPointForPerMonitorDPI([In] HandleRef hWnd, ref NativeMethods.POINT lpPoint);

		[DllImport("user32.dll")]
		internal static extern DpiAwarenessContextHandle GetWindowDpiAwarenessContext([In] nint hwnd);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool AreDpiAwarenessContextsEqual([In] nint dpiContextA, [In] nint dpiContextB);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		internal static extern uint GetDpiForWindow([In] HandleRef hwnd);

		[DllImport("user32.dll")]
		internal static extern uint GetDpiForSystem();

		[DllImport("user32.dll")]
		internal static extern NativeMethods.DPI_HOSTING_BEHAVIOR GetWindowDpiHostingBehavior(nint hWnd);

		[DllImport("user32.dll")]
		internal static extern NativeMethods.DPI_HOSTING_BEHAVIOR GetThreadDpiHostingBehavior();

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		internal static extern bool InSendMessage();

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
		public static extern int IsThemeActive();

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool SetCaretPos(int x, int y);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool DestroyCaret();

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int GetCaretBlinkTime();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool GetStringTypeEx(uint locale, uint infoType, char[] sourceString, int count, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] ushort[] charTypes);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int GetSysColor(int nIndex);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern bool IsClipboardFormatAvailable(int format);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		internal static extern bool IsDebuggerPresent();

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool QueryPerformanceFrequency(out long lpFrequency);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		internal static extern int GetMessageTime();
	}

	[Flags]
	internal enum PlaySoundFlags
	{
		SND_SYNC = 0,
		SND_ASYNC = 1,
		SND_NODEFAULT = 2,
		SND_MEMORY = 4,
		SND_LOOP = 8,
		SND_NOSTOP = 0x10,
		SND_PURGE = 0x40,
		SND_APPLICATION = 0x80,
		SND_NOWAIT = 0x2000,
		SND_ALIAS = 0x10000,
		SND_FILENAME = 0x20000,
		SND_RESOURCE = 0x40000
	}

	public const uint CT_CTYPE1 = 1u;

	public const uint CT_CTYPE2 = 2u;

	public const uint CT_CTYPE3 = 4u;

	public const ushort C1_SPACE = 8;

	public const ushort C1_PUNCT = 16;

	public const ushort C1_BLANK = 64;

	public const ushort C3_NONSPACING = 1;

	public const ushort C3_DIACRITIC = 2;

	public const ushort C3_VOWELMARK = 4;

	public const ushort C3_KATAKANA = 16;

	public const ushort C3_HIRAGANA = 32;

	public const ushort C3_HALFWIDTH = 64;

	public const ushort C3_FULLWIDTH = 128;

	public const ushort C3_IDEOGRAPH = 256;

	public const ushort C3_KASHIDA = 512;

	public static int GetMessagePos()
	{
		return SafeNativeMethodsPrivate.GetMessagePos();
	}

	public static nint GetKeyboardLayout(int dwLayout)
	{
		return SafeNativeMethodsPrivate.GetKeyboardLayout(dwLayout);
	}

	public static nint ActivateKeyboardLayout(HandleRef hkl, int uFlags)
	{
		return SafeNativeMethodsPrivate.ActivateKeyboardLayout(hkl, uFlags);
	}

	public static int GetKeyboardLayoutList(int size, [Out][MarshalAs(UnmanagedType.LPArray)] nint[] hkls)
	{
		int keyboardLayoutList = NativeMethodsSetLastError.GetKeyboardLayoutList(size, hkls);
		if (keyboardLayoutList == 0)
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error != 0)
			{
				throw new Win32Exception(lastWin32Error);
			}
		}
		return keyboardLayoutList;
	}

	internal static void GetMonitorInfo(HandleRef hmonitor, [In][Out] NativeMethods.MONITORINFOEX info)
	{
		if (!SafeNativeMethodsPrivate.IntGetMonitorInfo(hmonitor, info))
		{
			throw new Win32Exception();
		}
	}

	public static nint MonitorFromPoint(NativeMethods.POINT pt, int flags)
	{
		return SafeNativeMethodsPrivate.MonitorFromPoint(pt, flags);
	}

	public static nint MonitorFromRect(ref NativeMethods.RECT rect, int flags)
	{
		return SafeNativeMethodsPrivate.MonitorFromRect(ref rect, flags);
	}

	public static nint MonitorFromWindow(HandleRef handle, int flags)
	{
		return SafeNativeMethodsPrivate.MonitorFromWindow(handle, flags);
	}

	public static NativeMethods.CursorHandle LoadCursor(HandleRef hInst, nint iconId)
	{
		NativeMethods.CursorHandle cursorHandle = SafeNativeMethodsPrivate.LoadCursor(hInst, iconId);
		if (cursorHandle == null || cursorHandle.IsInvalid)
		{
			throw new Win32Exception();
		}
		return cursorHandle;
	}

	public static nint GetCursor()
	{
		return SafeNativeMethodsPrivate.GetCursor();
	}

	public static int ShowCursor(bool show)
	{
		return SafeNativeMethodsPrivate.ShowCursor(show);
	}

	internal static bool AdjustWindowRectEx(ref NativeMethods.RECT lpRect, int dwStyle, bool bMenu, int dwExStyle)
	{
		bool num = SafeNativeMethodsPrivate.IntAdjustWindowRectEx(ref lpRect, dwStyle, bMenu, dwExStyle);
		if (!num)
		{
			throw new Win32Exception();
		}
		return num;
	}

	internal static void GetClientRect(HandleRef hWnd, [In][Out] ref NativeMethods.RECT rect)
	{
		if (!SafeNativeMethodsPrivate.IntGetClientRect(hWnd, ref rect))
		{
			throw new Win32Exception();
		}
	}

	internal static NativeMethods.RECT GetClientRect(HandleRef hWnd)
	{
		NativeMethods.RECT rect = default(NativeMethods.RECT);
		GetClientRect(hWnd, ref rect);
		return rect;
	}

	internal static void GetWindowRect(HandleRef hWnd, [In][Out] ref NativeMethods.RECT rect)
	{
		if (!SafeNativeMethodsPrivate.IntGetWindowRect(hWnd, ref rect))
		{
			throw new Win32Exception();
		}
	}

	public static int GetDoubleClickTime()
	{
		return SafeNativeMethodsPrivate.GetDoubleClickTime();
	}

	public static bool IsWindowEnabled(HandleRef hWnd)
	{
		return SafeNativeMethodsPrivate.IsWindowEnabled(hWnd);
	}

	public static bool IsWindowVisible(HandleRef hWnd)
	{
		return SafeNativeMethodsPrivate.IsWindowVisible(hWnd);
	}

	internal static bool ReleaseCapture()
	{
		bool num = SafeNativeMethodsPrivate.IntReleaseCapture();
		if (!num)
		{
			throw new Win32Exception();
		}
		return num;
	}

	public static bool TrackMouseEvent(NativeMethods.TRACKMOUSEEVENT tme)
	{
		bool num = SafeNativeMethodsPrivate.TrackMouseEvent(tme);
		int lastWin32Error = Marshal.GetLastWin32Error();
		if (!num && lastWin32Error != 0)
		{
			throw new Win32Exception(lastWin32Error);
		}
		return num;
	}

	public static void SetTimer(HandleRef hWnd, int nIDEvent, int uElapse)
	{
		if (SafeNativeMethodsPrivate.SetTimer(hWnd, nIDEvent, uElapse, null) == IntPtr.Zero)
		{
			throw new Win32Exception();
		}
	}

	public static bool TrySetTimer(HandleRef hWnd, int nIDEvent, int uElapse)
	{
		if (SafeNativeMethodsPrivate.TrySetTimer(hWnd, nIDEvent, uElapse, null) == IntPtr.Zero)
		{
			return false;
		}
		return true;
	}

	public static bool KillTimer(HandleRef hwnd, int idEvent)
	{
		return SafeNativeMethodsPrivate.KillTimer(hwnd, idEvent);
	}

	public static int GetTickCount()
	{
		return SafeNativeMethodsPrivate.GetTickCount();
	}

	public static int MessageBeep(int uType)
	{
		return SafeNativeMethodsPrivate.MessageBeep(uType);
	}

	public static bool IsWindowUnicode(HandleRef hWnd)
	{
		return SafeNativeMethodsPrivate.IsWindowUnicode(hWnd);
	}

	public static nint SetCursor(HandleRef hcursor)
	{
		return SafeNativeMethodsPrivate.SetCursor(hcursor);
	}

	public static nint SetCursor(SafeHandle hcursor)
	{
		return SafeNativeMethodsPrivate.SetCursor(hcursor);
	}

	public static void ScreenToClient(HandleRef hWnd, ref NativeMethods.POINT pt)
	{
		if (SafeNativeMethodsPrivate.IntScreenToClient(hWnd, ref pt) == 0)
		{
			throw new Win32Exception();
		}
	}

	public static int GetCurrentProcessId()
	{
		return SafeNativeMethodsPrivate.GetCurrentProcessId();
	}

	public static int GetCurrentThreadId()
	{
		return SafeNativeMethodsPrivate.GetCurrentThreadId();
	}

	public static int? GetCurrentSessionId()
	{
		int? result = null;
		if (SafeNativeMethodsPrivate.ProcessIdToSessionId(GetCurrentProcessId(), out var pSessionId))
		{
			result = pSessionId;
		}
		return result;
	}

	public static nint GetCapture()
	{
		return SafeNativeMethodsPrivate.GetCapture();
	}

	public static nint SetCapture(HandleRef hwnd)
	{
		return SafeNativeMethodsPrivate.SetCapture(hwnd);
	}

	internal static int MapVirtualKey(int nVirtKey, int nMapType)
	{
		return SafeNativeMethodsPrivate.MapVirtualKey(nVirtKey, nMapType);
	}

	public static bool IsCurrentSessionConnectStateWTSActive(int? SessionId = null, bool defaultResult = true)
	{
		nint ppBuffer = IntPtr.Zero;
		int sessionId = (SessionId.HasValue ? SessionId.Value : (-1));
		bool result = defaultResult;
		try
		{
			if (SafeNativeMethodsPrivate.WTSQuerySessionInformation(NativeMethods.WTS_CURRENT_SERVER_HANDLE, sessionId, NativeMethods.WTS_INFO_CLASS.WTSConnectState, out ppBuffer, out var BytesReturned) && BytesReturned >= 4)
			{
				int num = Marshal.ReadInt32(ppBuffer);
				if (Enum.IsDefined(typeof(NativeMethods.WTS_CONNECTSTATE_CLASS), num))
				{
					result = num == 0;
				}
			}
		}
		finally
		{
			try
			{
				if (ppBuffer != IntPtr.Zero)
				{
					SafeNativeMethodsPrivate.WTSFreeMemory(ppBuffer);
				}
			}
			catch (Exception ex) when (ex is Win32Exception || ex is SEHException)
			{
			}
		}
		return result;
	}

	internal static NativeMethods.PROCESS_DPI_AWARENESS GetProcessDpiAwareness(HandleRef hProcess)
	{
		nint awareness = IntPtr.Zero;
		int processDpiAwareness = (int)SafeNativeMethodsPrivate.GetProcessDpiAwareness(hProcess, out awareness);
		if (processDpiAwareness != 0)
		{
			Marshal.ThrowExceptionForHR(processDpiAwareness);
		}
		return (NativeMethods.PROCESS_DPI_AWARENESS)NativeMethods.IntPtrToInt32(awareness);
	}

	internal static DpiAwarenessContextHandle GetWindowDpiAwarenessContext(nint hwnd)
	{
		return SafeNativeMethodsPrivate.GetWindowDpiAwarenessContext(hwnd);
	}

	internal static bool AreDpiAwarenessContextsEqual(nint dpiContextA, nint dpiContextB)
	{
		return SafeNativeMethodsPrivate.AreDpiAwarenessContextsEqual(dpiContextA, dpiContextB);
	}

	internal static uint GetDpiForWindow(HandleRef hwnd)
	{
		return SafeNativeMethodsPrivate.GetDpiForWindow(hwnd);
	}

	internal static uint GetDpiForSystem()
	{
		return SafeNativeMethodsPrivate.GetDpiForSystem();
	}

	internal static NativeMethods.DPI_HOSTING_BEHAVIOR GetWindowDpiHostingBehavior(nint hWnd)
	{
		return SafeNativeMethodsPrivate.GetWindowDpiHostingBehavior(hWnd);
	}

	internal static NativeMethods.DPI_HOSTING_BEHAVIOR GetThreadDpiHostingBehavior()
	{
		return SafeNativeMethodsPrivate.GetThreadDpiHostingBehavior();
	}

	internal static bool AdjustWindowRectExForDpi(ref NativeMethods.RECT lpRect, int dwStyle, bool bMenu, int dwExStyle, int dpi)
	{
		return SafeNativeMethodsPrivate.AdjustWindowRectExForDpi(ref lpRect, dwStyle, bMenu, dwExStyle, dpi);
	}

	internal static bool LogicalToPhysicalPointForPerMonitorDPI(HandleRef hWnd, ref NativeMethods.POINT lpPoint)
	{
		return SafeNativeMethodsPrivate.LogicalToPhysicalPointForPerMonitorDPI(hWnd, ref lpPoint);
	}

	internal static bool PhysicalToLogicalPointForPerMonitorDPI(HandleRef hWnd, ref NativeMethods.POINT lpPoint)
	{
		return SafeNativeMethodsPrivate.PhysicalToLogicalPointForPerMonitorDPI(hWnd, ref lpPoint);
	}

	internal static bool InSendMessage()
	{
		return SafeNativeMethodsPrivate.InSendMessage();
	}

	public static bool IsUxThemeActive()
	{
		return SafeNativeMethodsPrivate.IsThemeActive() != 0;
	}

	public static bool SetCaretPos(int x, int y)
	{
		return SafeNativeMethodsPrivate.SetCaretPos(x, y);
	}

	public static bool DestroyCaret()
	{
		return SafeNativeMethodsPrivate.DestroyCaret();
	}

	public static int GetCaretBlinkTime()
	{
		return SafeNativeMethodsPrivate.GetCaretBlinkTime();
	}

	public static bool GetStringTypeEx(uint locale, uint infoType, char[] sourceString, int count, ushort[] charTypes)
	{
		bool stringTypeEx = SafeNativeMethodsPrivate.GetStringTypeEx(locale, infoType, sourceString, count, charTypes);
		int lastWin32Error = Marshal.GetLastWin32Error();
		if (!stringTypeEx)
		{
			throw new Win32Exception(lastWin32Error);
		}
		return stringTypeEx;
	}

	public static int GetSysColor(int nIndex)
	{
		return SafeNativeMethodsPrivate.GetSysColor(nIndex);
	}

	public static bool IsClipboardFormatAvailable(int format)
	{
		return SafeNativeMethodsPrivate.IsClipboardFormatAvailable(format);
	}

	public static bool IsDebuggerPresent()
	{
		return SafeNativeMethodsPrivate.IsDebuggerPresent();
	}

	public static void QueryPerformanceCounter(out long lpPerformanceCount)
	{
		if (!SafeNativeMethodsPrivate.QueryPerformanceCounter(out lpPerformanceCount))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}

	public static void QueryPerformanceFrequency(out long lpFrequency)
	{
		if (!SafeNativeMethodsPrivate.QueryPerformanceFrequency(out lpFrequency))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}

	internal static int GetMessageTime()
	{
		return SafeNativeMethodsPrivate.GetMessageTime();
	}

	internal static int GetWindowStyle(HandleRef hWnd, bool exStyle)
	{
		int nIndex = (exStyle ? (-20) : (-16));
		return UnsafeNativeMethods.GetWindowLong(hWnd, nIndex);
	}
}
