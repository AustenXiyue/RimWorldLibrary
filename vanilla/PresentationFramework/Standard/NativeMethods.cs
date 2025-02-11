using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using MS.Internal.WindowsBase;

namespace Standard;

internal static class NativeMethods
{
	[DllImport("user32.dll", EntryPoint = "AdjustWindowRectEx", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _AdjustWindowRectEx(ref RECT lpRect, WS dwStyle, [MarshalAs(UnmanagedType.Bool)] bool bMenu, WS_EX dwExStyle);

	public static RECT AdjustWindowRectEx(RECT lpRect, WS dwStyle, bool bMenu, WS_EX dwExStyle)
	{
		if (!_AdjustWindowRectEx(ref lpRect, dwStyle, bMenu, dwExStyle))
		{
			HRESULT.ThrowLastError();
		}
		return lpRect;
	}

	[DllImport("user32.dll", EntryPoint = "ChangeWindowMessageFilter", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _ChangeWindowMessageFilter(WM message, MSGFLT dwFlag);

	[DllImport("user32.dll", EntryPoint = "ChangeWindowMessageFilterEx", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _ChangeWindowMessageFilterEx(nint hwnd, WM message, MSGFLT action, [Optional][In][Out] ref CHANGEFILTERSTRUCT pChangeFilterStruct);

	public static HRESULT ChangeWindowMessageFilterEx(nint hwnd, WM message, MSGFLT action, out MSGFLTINFO filterInfo)
	{
		filterInfo = MSGFLTINFO.NONE;
		if (!Utility.IsOSVistaOrNewer)
		{
			return HRESULT.S_FALSE;
		}
		if (!Utility.IsOSWindows7OrNewer)
		{
			if (!_ChangeWindowMessageFilter(message, action))
			{
				return (HRESULT)Win32Error.GetLastError();
			}
			return HRESULT.S_OK;
		}
		CHANGEFILTERSTRUCT cHANGEFILTERSTRUCT = default(CHANGEFILTERSTRUCT);
		cHANGEFILTERSTRUCT.cbSize = (uint)Marshal.SizeOf(typeof(CHANGEFILTERSTRUCT));
		CHANGEFILTERSTRUCT pChangeFilterStruct = cHANGEFILTERSTRUCT;
		if (!_ChangeWindowMessageFilterEx(hwnd, message, action, ref pChangeFilterStruct))
		{
			return (HRESULT)Win32Error.GetLastError();
		}
		filterInfo = pChangeFilterStruct.ExtStatus;
		return HRESULT.S_OK;
	}

	[DllImport("gdi32.dll")]
	public static extern CombineRgnResult CombineRgn(nint hrgnDest, nint hrgnSrc1, nint hrgnSrc2, RGN fnCombineMode);

	[DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "CommandLineToArgvW")]
	private static extern nint _CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string cmdLine, out int numArgs);

	public static string[] CommandLineToArgvW(string cmdLine)
	{
		nint num = IntPtr.Zero;
		try
		{
			int numArgs = 0;
			num = _CommandLineToArgvW(cmdLine, out numArgs);
			if (num == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			string[] array = new string[numArgs];
			for (int i = 0; i < numArgs; i++)
			{
				nint ptr = Marshal.ReadIntPtr(num, i * Marshal.SizeOf(typeof(nint)));
				array[i] = Marshal.PtrToStringUni(ptr);
			}
			return array;
		}
		finally
		{
			_LocalFree(num);
		}
	}

	[DllImport("gdi32.dll", EntryPoint = "CreateDIBSection", SetLastError = true)]
	private static extern SafeHBITMAP _CreateDIBSection(SafeDC hdc, [In] ref BITMAPINFO bitmapInfo, int iUsage, out nint ppvBits, nint hSection, int dwOffset);

	[DllImport("gdi32.dll", EntryPoint = "CreateDIBSection", SetLastError = true)]
	private static extern SafeHBITMAP _CreateDIBSectionIntPtr(nint hdc, [In] ref BITMAPINFO bitmapInfo, int iUsage, out nint ppvBits, nint hSection, int dwOffset);

	public static SafeHBITMAP CreateDIBSection(SafeDC hdc, ref BITMAPINFO bitmapInfo, out nint ppvBits, nint hSection, int dwOffset)
	{
		SafeHBITMAP safeHBITMAP = null;
		safeHBITMAP = ((hdc != null) ? _CreateDIBSection(hdc, ref bitmapInfo, 0, out ppvBits, hSection, dwOffset) : _CreateDIBSectionIntPtr(IntPtr.Zero, ref bitmapInfo, 0, out ppvBits, hSection, dwOffset));
		if (safeHBITMAP.IsInvalid)
		{
			HRESULT.ThrowLastError();
		}
		return safeHBITMAP;
	}

	[DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn", SetLastError = true)]
	private static extern nint _CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

	public static nint CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse)
	{
		nint num = _CreateRoundRectRgn(nLeftRect, nTopRect, nRightRect, nBottomRect, nWidthEllipse, nHeightEllipse);
		if (IntPtr.Zero == num)
		{
			throw new Win32Exception();
		}
		return num;
	}

	[DllImport("gdi32.dll", EntryPoint = "CreateRectRgn", SetLastError = true)]
	private static extern nint _CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

	public static nint CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect)
	{
		nint num = _CreateRectRgn(nLeftRect, nTopRect, nRightRect, nBottomRect);
		if (IntPtr.Zero == num)
		{
			throw new Win32Exception();
		}
		return num;
	}

	[DllImport("gdi32.dll", EntryPoint = "CreateRectRgnIndirect", SetLastError = true)]
	private static extern nint _CreateRectRgnIndirect([In] ref RECT lprc);

	public static nint CreateRectRgnIndirect(RECT lprc)
	{
		nint num = _CreateRectRgnIndirect(ref lprc);
		if (IntPtr.Zero == num)
		{
			throw new Win32Exception();
		}
		return num;
	}

	[DllImport("gdi32.dll")]
	public static extern nint CreateSolidBrush(int crColor);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateWindowExW", SetLastError = true)]
	private static extern nint _CreateWindowEx(WS_EX dwExStyle, [MarshalAs(UnmanagedType.LPWStr)] string lpClassName, [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName, WS dwStyle, int x, int y, int nWidth, int nHeight, nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

	public static nint CreateWindowEx(WS_EX dwExStyle, string lpClassName, string lpWindowName, WS dwStyle, int x, int y, int nWidth, int nHeight, nint hWndParent, nint hMenu, nint hInstance, nint lpParam)
	{
		nint num = _CreateWindowEx(dwExStyle, lpClassName, lpWindowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
		if (IntPtr.Zero == num)
		{
			HRESULT.ThrowLastError();
		}
		return num;
	}

	[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "DefWindowProcW")]
	public static extern nint DefWindowProc(nint hWnd, WM Msg, nint wParam, nint lParam);

	[DllImport("gdi32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DeleteObject(nint hObject);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DestroyIcon(nint handle);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DestroyWindow(nint hwnd);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool IsWindow(nint hwnd);

	[DllImport("dwmapi.dll", PreserveSig = false)]
	public static extern void DwmExtendFrameIntoClientArea(nint hwnd, ref MARGINS pMarInset);

	[DllImport("dwmapi.dll", EntryPoint = "DwmIsCompositionEnabled", PreserveSig = false)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _DwmIsCompositionEnabled();

	[DllImport("dwmapi.dll", EntryPoint = "DwmGetColorizationColor")]
	private static extern HRESULT _DwmGetColorizationColor(out uint pcrColorization, [MarshalAs(UnmanagedType.Bool)] out bool pfOpaqueBlend);

	public static bool DwmGetColorizationColor(out uint pcrColorization, out bool pfOpaqueBlend)
	{
		if (Utility.IsOSVistaOrNewer && IsThemeActive() && _DwmGetColorizationColor(out pcrColorization, out pfOpaqueBlend).Succeeded)
		{
			return true;
		}
		pcrColorization = 4278190080u;
		pfOpaqueBlend = true;
		return false;
	}

	public static bool DwmIsCompositionEnabled()
	{
		if (!Utility.IsOSVistaOrNewer)
		{
			return false;
		}
		return _DwmIsCompositionEnabled();
	}

	[DllImport("dwmapi.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DwmDefWindowProc(nint hwnd, WM msg, nint wParam, nint lParam, out nint plResult);

	[DllImport("dwmapi.dll", EntryPoint = "DwmSetWindowAttribute")]
	private static extern void _DwmSetWindowAttribute(nint hwnd, DWMWA dwAttribute, ref int pvAttribute, int cbAttribute);

	public static void DwmSetWindowAttributeFlip3DPolicy(nint hwnd, DWMFLIP3D flip3dPolicy)
	{
		int pvAttribute = (int)flip3dPolicy;
		_DwmSetWindowAttribute(hwnd, DWMWA.FLIP3D_POLICY, ref pvAttribute, 4);
	}

	public static void DwmSetWindowAttributeDisallowPeek(nint hwnd, bool disallowPeek)
	{
		int pvAttribute = (disallowPeek ? 1 : 0);
		_DwmSetWindowAttribute(hwnd, DWMWA.DISALLOW_PEEK, ref pvAttribute, 4);
	}

	[DllImport("user32.dll", EntryPoint = "EnableMenuItem")]
	private static extern int _EnableMenuItem(nint hMenu, SC uIDEnableItem, MF uEnable);

	public static MF EnableMenuItem(nint hMenu, SC uIDEnableItem, MF uEnable)
	{
		return (MF)_EnableMenuItem(hMenu, uIDEnableItem, uEnable);
	}

	[DllImport("user32.dll", EntryPoint = "RemoveMenu", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _RemoveMenu(nint hMenu, uint uPosition, uint uFlags);

	public static void RemoveMenu(nint hMenu, SC uPosition, MF uFlags)
	{
		if (!_RemoveMenu(hMenu, (uint)uPosition, (uint)uFlags))
		{
			throw new Win32Exception();
		}
	}

	[DllImport("user32.dll", EntryPoint = "DrawMenuBar", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _DrawMenuBar(nint hWnd);

	public static void DrawMenuBar(nint hWnd)
	{
		if (!_DrawMenuBar(hWnd))
		{
			throw new Win32Exception();
		}
	}

	[DllImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool FindClose(nint handle);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern SafeFindHandle FindFirstFileW(string lpFileName, [In][Out][MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_DATAW lpFindFileData);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool FindNextFileW(SafeFindHandle hndFindFile, [In][Out][MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_DATAW lpFindFileData);

	[DllImport("user32.dll", EntryPoint = "GetClientRect", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _GetClientRect(nint hwnd, out RECT lpRect);

	public static RECT GetClientRect(nint hwnd)
	{
		if (!_GetClientRect(hwnd, out var lpRect))
		{
			HRESULT.ThrowLastError();
		}
		return lpRect;
	}

	[DllImport("uxtheme.dll", CharSet = CharSet.Unicode, EntryPoint = "GetCurrentThemeName")]
	private static extern HRESULT _GetCurrentThemeName(StringBuilder pszThemeFileName, int dwMaxNameChars, StringBuilder pszColorBuff, int cchMaxColorChars, StringBuilder pszSizeBuff, int cchMaxSizeChars);

	public static void GetCurrentThemeName(out string themeFileName, out string color, out string size)
	{
		StringBuilder stringBuilder = new StringBuilder(260);
		StringBuilder stringBuilder2 = new StringBuilder(260);
		StringBuilder stringBuilder3 = new StringBuilder(260);
		_GetCurrentThemeName(stringBuilder, stringBuilder.Capacity, stringBuilder2, stringBuilder2.Capacity, stringBuilder3, stringBuilder3.Capacity).ThrowIfFailed();
		themeFileName = stringBuilder.ToString();
		color = stringBuilder2.ToString();
		size = stringBuilder3.ToString();
	}

	[DllImport("uxtheme.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool IsThemeActive();

	[DllImport("gdi32.dll")]
	public static extern int GetDeviceCaps(SafeDC hdc, DeviceCap nIndex);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetModuleFileName", SetLastError = true)]
	private static extern int _GetModuleFileName(nint hModule, StringBuilder lpFilename, int nSize);

	public static string GetModuleFileName(nint hModule)
	{
		StringBuilder stringBuilder = new StringBuilder(260);
		while (true)
		{
			int num = _GetModuleFileName(hModule, stringBuilder, stringBuilder.Capacity);
			if (num == 0)
			{
				HRESULT.ThrowLastError();
			}
			if (num != stringBuilder.Capacity)
			{
				break;
			}
			stringBuilder.EnsureCapacity(stringBuilder.Capacity * 2);
		}
		return stringBuilder.ToString();
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetModuleHandleW", SetLastError = true)]
	private static extern nint _GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

	public static nint GetModuleHandle(string lpModuleName)
	{
		nint num = _GetModuleHandle(lpModuleName);
		if (num == IntPtr.Zero)
		{
			HRESULT.ThrowLastError();
		}
		return num;
	}

	[DllImport("user32.dll", EntryPoint = "GetMonitorInfo", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _GetMonitorInfo(nint hMonitor, [In][Out] MONITORINFO lpmi);

	public static MONITORINFO GetMonitorInfo(nint hMonitor)
	{
		MONITORINFO mONITORINFO = new MONITORINFO();
		if (!_GetMonitorInfo(hMonitor, mONITORINFO))
		{
			throw new Win32Exception();
		}
		return mONITORINFO;
	}

	[DllImport("gdi32.dll", EntryPoint = "GetStockObject", SetLastError = true)]
	private static extern nint _GetStockObject(StockObject fnObject);

	public static nint GetStockObject(StockObject fnObject)
	{
		nint num = _GetStockObject(fnObject);
		if (num == IntPtr.Zero)
		{
			HRESULT.ThrowLastError();
		}
		return num;
	}

	[DllImport("user32.dll")]
	public static extern nint GetSystemMenu(nint hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

	[DllImport("user32.dll")]
	public static extern int GetSystemMetrics(SM nIndex);

	public static nint GetWindowLongPtr(nint hwnd, GWL nIndex)
	{
		nint zero = IntPtr.Zero;
		zero = ((8 != IntPtr.Size) ? new IntPtr(NativeMethodsSetLastError.GetWindowLong(hwnd, (int)nIndex)) : NativeMethodsSetLastError.GetWindowLongPtr(hwnd, (int)nIndex));
		if (IntPtr.Zero == zero)
		{
			throw new Win32Exception();
		}
		return zero;
	}

	[DllImport("uxtheme.dll", PreserveSig = false)]
	public static extern void SetWindowThemeAttribute([In] nint hwnd, [In] WINDOWTHEMEATTRIBUTETYPE eAttribute, [In] ref WTA_OPTIONS pvAttribute, [In] uint cbAttribute);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetWindowPlacement(nint hwnd, WINDOWPLACEMENT lpwndpl);

	public static WINDOWPLACEMENT GetWindowPlacement(nint hwnd)
	{
		WINDOWPLACEMENT wINDOWPLACEMENT = new WINDOWPLACEMENT();
		if (GetWindowPlacement(hwnd, wINDOWPLACEMENT))
		{
			return wINDOWPLACEMENT;
		}
		throw new Win32Exception();
	}

	[DllImport("user32.dll", EntryPoint = "GetWindowRect", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _GetWindowRect(nint hWnd, out RECT lpRect);

	public static RECT GetWindowRect(nint hwnd)
	{
		if (!_GetWindowRect(hwnd, out var lpRect))
		{
			HRESULT.ThrowLastError();
		}
		return lpRect;
	}

	[DllImport("gdiplus.dll")]
	public static extern Status GdipCreateBitmapFromStream(IStream stream, out nint bitmap);

	[DllImport("gdiplus.dll")]
	public static extern Status GdipCreateHBITMAPFromBitmap(nint bitmap, out nint hbmReturn, int background);

	[DllImport("gdiplus.dll")]
	public static extern Status GdipCreateHICONFromBitmap(nint bitmap, out nint hbmReturn);

	[DllImport("gdiplus.dll")]
	public static extern Status GdipDisposeImage(nint image);

	[DllImport("gdiplus.dll")]
	public static extern Status GdipImageForceValidation(nint image);

	[DllImport("gdiplus.dll")]
	public static extern Status GdiplusStartup(out nint token, StartupInput input, out StartupOutput output);

	[DllImport("gdiplus.dll")]
	public static extern Status GdiplusShutdown(nint token);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool IsWindowVisible(nint hwnd);

	[DllImport("kernel32.dll", EntryPoint = "LocalFree", SetLastError = true)]
	private static extern nint _LocalFree(nint hMem);

	[DllImport("user32.dll")]
	public static extern nint MonitorFromWindow(nint hwnd, uint dwFlags);

	[DllImport("user32.dll", EntryPoint = "PostMessage", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _PostMessage(nint hWnd, WM Msg, nint wParam, nint lParam);

	public static void PostMessage(nint hWnd, WM Msg, nint wParam, nint lParam)
	{
		if (!_PostMessage(hWnd, Msg, wParam, lParam))
		{
			throw new Win32Exception();
		}
	}

	[DllImport("user32.dll", EntryPoint = "RegisterClassExW", SetLastError = true)]
	private static extern short _RegisterClassEx([In] ref WNDCLASSEX lpwcx);

	public static short RegisterClassEx(ref WNDCLASSEX lpwcx)
	{
		short num = _RegisterClassEx(ref lpwcx);
		if (num == 0)
		{
			HRESULT.ThrowLastError();
		}
		return num;
	}

	[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegisterWindowMessage", SetLastError = true)]
	private static extern uint _RegisterWindowMessage([MarshalAs(UnmanagedType.LPWStr)] string lpString);

	public static WM RegisterWindowMessage(string lpString)
	{
		uint num = _RegisterWindowMessage(lpString);
		if (num == 0)
		{
			HRESULT.ThrowLastError();
		}
		return (WM)num;
	}

	[DllImport("user32.dll", EntryPoint = "SetActiveWindow", SetLastError = true)]
	private static extern nint _SetActiveWindow(nint hWnd);

	public static nint SetActiveWindow(nint hwnd)
	{
		Verify.IsNotDefault<nint>(hwnd, "hwnd");
		nint num = _SetActiveWindow(hwnd);
		if (num == IntPtr.Zero)
		{
			HRESULT.ThrowLastError();
		}
		return num;
	}

	public static nint SetClassLongPtr(nint hwnd, GCLP nIndex, nint dwNewLong)
	{
		if (8 == IntPtr.Size)
		{
			return SetClassLongPtr64(hwnd, nIndex, dwNewLong);
		}
		return new IntPtr(SetClassLongPtr32(hwnd, nIndex, ((IntPtr)dwNewLong).ToInt32()));
	}

	[DllImport("user32.dll", EntryPoint = "SetClassLong", SetLastError = true)]
	private static extern int SetClassLongPtr32(nint hWnd, GCLP nIndex, int dwNewLong);

	[DllImport("user32.dll", EntryPoint = "SetClassLongPtr", SetLastError = true)]
	private static extern nint SetClassLongPtr64(nint hWnd, GCLP nIndex, nint dwNewLong);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern ErrorModes SetErrorMode(ErrorModes newMode);

	[DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _SetProcessWorkingSetSize(nint hProcess, nint dwMinimiumWorkingSetSize, nint dwMaximumWorkingSetSize);

	public static void SetProcessWorkingSetSize(nint hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize)
	{
		if (!_SetProcessWorkingSetSize(hProcess, new IntPtr(dwMinimumWorkingSetSize), new IntPtr(dwMaximumWorkingSetSize)))
		{
			throw new Win32Exception();
		}
	}

	public static nint SetWindowLongPtr(nint hwnd, GWL nIndex, nint dwNewLong)
	{
		if (8 == IntPtr.Size)
		{
			return NativeMethodsSetLastError.SetWindowLongPtr(hwnd, (int)nIndex, dwNewLong);
		}
		return new IntPtr(NativeMethodsSetLastError.SetWindowLong(hwnd, (int)nIndex, ((IntPtr)dwNewLong).ToInt32()));
	}

	[DllImport("user32.dll", EntryPoint = "SetWindowRgn", SetLastError = true)]
	private static extern int _SetWindowRgn(nint hWnd, nint hRgn, [MarshalAs(UnmanagedType.Bool)] bool bRedraw);

	public static void SetWindowRgn(nint hWnd, nint hRgn, bool bRedraw)
	{
		if (_SetWindowRgn(hWnd, hRgn, bRedraw) == 0)
		{
			throw new Win32Exception();
		}
	}

	[DllImport("user32.dll", EntryPoint = "SetWindowPos", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, SWP uFlags);

	public static bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, SWP uFlags)
	{
		if (!_SetWindowPos(hWnd, hWndInsertAfter, x, y, cx, cy, uFlags))
		{
			return false;
		}
		return true;
	}

	[DllImport("shell32.dll")]
	public static extern Win32Error SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ShowWindow(nint hwnd, SW nCmdShow);

	[DllImport("user32.dll", EntryPoint = "SystemParametersInfoW", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _SystemParametersInfo_String(SPI uiAction, int uiParam, [MarshalAs(UnmanagedType.LPWStr)] string pvParam, SPIF fWinIni);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SystemParametersInfoW", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _SystemParametersInfo_NONCLIENTMETRICS(SPI uiAction, int uiParam, [In][Out] ref NONCLIENTMETRICS pvParam, SPIF fWinIni);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SystemParametersInfoW", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _SystemParametersInfo_HIGHCONTRAST(SPI uiAction, int uiParam, [In][Out] ref HIGHCONTRAST pvParam, SPIF fWinIni);

	public static void SystemParametersInfo(SPI uiAction, int uiParam, string pvParam, SPIF fWinIni)
	{
		if (!_SystemParametersInfo_String(uiAction, uiParam, pvParam, fWinIni))
		{
			HRESULT.ThrowLastError();
		}
	}

	public static NONCLIENTMETRICS SystemParameterInfo_GetNONCLIENTMETRICS()
	{
		NONCLIENTMETRICS pvParam = (Utility.IsOSVistaOrNewer ? NONCLIENTMETRICS.VistaMetricsStruct : NONCLIENTMETRICS.XPMetricsStruct);
		if (!_SystemParametersInfo_NONCLIENTMETRICS(SPI.GETNONCLIENTMETRICS, pvParam.cbSize, ref pvParam, SPIF.None))
		{
			HRESULT.ThrowLastError();
		}
		return pvParam;
	}

	public static HIGHCONTRAST SystemParameterInfo_GetHIGHCONTRAST()
	{
		HIGHCONTRAST hIGHCONTRAST = default(HIGHCONTRAST);
		hIGHCONTRAST.cbSize = Marshal.SizeOf(typeof(HIGHCONTRAST));
		HIGHCONTRAST pvParam = hIGHCONTRAST;
		if (!_SystemParametersInfo_HIGHCONTRAST(SPI.GETHIGHCONTRAST, pvParam.cbSize, ref pvParam, SPIF.None))
		{
			HRESULT.ThrowLastError();
		}
		return pvParam;
	}

	[DllImport("user32.dll")]
	public static extern uint TrackPopupMenuEx(nint hmenu, uint fuFlags, int x, int y, nint hwnd, nint lptpm);

	[DllImport("gdi32.dll", EntryPoint = "SelectObject", SetLastError = true)]
	private static extern nint _SelectObject(SafeDC hdc, nint hgdiobj);

	public static nint SelectObject(SafeDC hdc, nint hgdiobj)
	{
		nint num = _SelectObject(hdc, hgdiobj);
		if (num == IntPtr.Zero)
		{
			HRESULT.ThrowLastError();
		}
		return num;
	}

	[DllImport("gdi32.dll", EntryPoint = "SelectObject", SetLastError = true)]
	private static extern nint _SelectObjectSafeHBITMAP(SafeDC hdc, SafeHBITMAP hgdiobj);

	public static nint SelectObject(SafeDC hdc, SafeHBITMAP hgdiobj)
	{
		nint num = _SelectObjectSafeHBITMAP(hdc, hgdiobj);
		if (num == IntPtr.Zero)
		{
			HRESULT.ThrowLastError();
		}
		return num;
	}

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int SendInput(int nInputs, ref INPUT pInputs, int cbSize);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern nint SendMessage(nint hWnd, WM Msg, nint wParam, nint lParam);

	[DllImport("user32.dll", EntryPoint = "UnregisterClass", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _UnregisterClassAtom(nint lpClassName, nint hInstance);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "UnregisterClass", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _UnregisterClassName(string lpClassName, nint hInstance);

	public static void UnregisterClass(short atom, nint hinstance)
	{
		if (!_UnregisterClassAtom(new IntPtr(atom), hinstance))
		{
			HRESULT.ThrowLastError();
		}
	}

	public static void UnregisterClass(string lpClassName, nint hInstance)
	{
		if (!_UnregisterClassName(lpClassName, hInstance))
		{
			HRESULT.ThrowLastError();
		}
	}

	[DllImport("user32.dll", EntryPoint = "UpdateLayeredWindow", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _UpdateLayeredWindow(nint hwnd, SafeDC hdcDst, ref POINT pptDst, ref SIZE psize, SafeDC hdcSrc, ref POINT pptSrc, int crKey, ref BLENDFUNCTION pblend, ULW dwFlags);

	[DllImport("user32.dll", EntryPoint = "UpdateLayeredWindow", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool _UpdateLayeredWindowIntPtr(nint hwnd, nint hdcDst, nint pptDst, nint psize, nint hdcSrc, nint pptSrc, int crKey, ref BLENDFUNCTION pblend, ULW dwFlags);

	public static void UpdateLayeredWindow(nint hwnd, SafeDC hdcDst, ref POINT pptDst, ref SIZE psize, SafeDC hdcSrc, ref POINT pptSrc, int crKey, ref BLENDFUNCTION pblend, ULW dwFlags)
	{
		if (!_UpdateLayeredWindow(hwnd, hdcDst, ref pptDst, ref psize, hdcSrc, ref pptSrc, crKey, ref pblend, dwFlags))
		{
			HRESULT.ThrowLastError();
		}
	}

	public static void UpdateLayeredWindow(nint hwnd, int crKey, ref BLENDFUNCTION pblend, ULW dwFlags)
	{
		if (!_UpdateLayeredWindowIntPtr(hwnd, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, crKey, ref pblend, dwFlags))
		{
			HRESULT.ThrowLastError();
		}
	}

	[DllImport("shell32.dll", EntryPoint = "SHAddToRecentDocs")]
	private static extern void _SHAddToRecentDocs_String(SHARD uFlags, [MarshalAs(UnmanagedType.LPWStr)] string pv);

	[DllImport("shell32.dll", EntryPoint = "SHAddToRecentDocs")]
	private static extern void _SHAddToRecentDocs_ShellLink(SHARD uFlags, IShellLinkW pv);

	public static void SHAddToRecentDocs(string path)
	{
		_SHAddToRecentDocs_String(SHARD.PATHW, path);
	}

	public static void SHAddToRecentDocs(IShellLinkW shellLink)
	{
		_SHAddToRecentDocs_ShellLink(SHARD.LINK, shellLink);
	}

	[DllImport("dwmapi.dll", EntryPoint = "DwmGetCompositionTimingInfo")]
	private static extern HRESULT _DwmGetCompositionTimingInfo(nint hwnd, ref DWM_TIMING_INFO pTimingInfo);

	public static DWM_TIMING_INFO? DwmGetCompositionTimingInfo(nint hwnd)
	{
		if (!Utility.IsOSVistaOrNewer)
		{
			return null;
		}
		DWM_TIMING_INFO dWM_TIMING_INFO = default(DWM_TIMING_INFO);
		dWM_TIMING_INFO.cbSize = Marshal.SizeOf(typeof(DWM_TIMING_INFO));
		DWM_TIMING_INFO pTimingInfo = dWM_TIMING_INFO;
		HRESULT hRESULT = _DwmGetCompositionTimingInfo(hwnd, ref pTimingInfo);
		if (hRESULT == HRESULT.E_PENDING)
		{
			return null;
		}
		hRESULT.ThrowIfFailed();
		return pTimingInfo;
	}

	[DllImport("dwmapi.dll", PreserveSig = false)]
	public static extern void DwmInvalidateIconicBitmaps(nint hwnd);

	[DllImport("dwmapi.dll", PreserveSig = false)]
	public static extern void DwmSetIconicThumbnail(nint hwnd, nint hbmp, DWM_SIT dwSITFlags);

	[DllImport("dwmapi.dll", PreserveSig = false)]
	public static extern void DwmSetIconicLivePreviewBitmap(nint hwnd, nint hbmp, RefPOINT pptClient, DWM_SIT dwSITFlags);

	[DllImport("shell32.dll", PreserveSig = false)]
	public static extern void SHGetItemFromDataObject(IDataObject pdtobj, DOGIF dwFlags, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);

	[DllImport("shell32.dll")]
	public static extern HRESULT SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IBindCtx pbc, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);

	[DllImport("shell32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool Shell_NotifyIcon(NIM dwMessage, [In] NOTIFYICONDATA lpdata);

	[DllImport("shell32.dll", PreserveSig = false)]
	public static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

	[DllImport("shell32.dll")]
	public static extern HRESULT GetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] out string AppID);
}
