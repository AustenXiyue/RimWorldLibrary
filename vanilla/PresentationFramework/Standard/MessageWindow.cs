using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace Standard;

internal sealed class MessageWindow : CriticalFinalizerObject
{
	private static readonly WndProc s_WndProc;

	private static readonly Dictionary<nint, MessageWindow> s_windowLookup;

	private WndProc _wndProcCallback;

	private string _className;

	private bool _isDisposed;

	private Dispatcher _dispatcher;

	public nint Handle { get; private set; }

	static MessageWindow()
	{
		s_WndProc = _WndProc;
		s_windowLookup = new Dictionary<nint, MessageWindow>();
	}

	public MessageWindow(CS classStyle, WS style, WS_EX exStyle, Rect location, string name, WndProc callback)
	{
		_wndProcCallback = callback;
		_className = "MessageWindowClass+" + Guid.NewGuid();
		WNDCLASSEX lpwcx = new WNDCLASSEX
		{
			cbSize = Marshal.SizeOf(typeof(WNDCLASSEX)),
			style = classStyle,
			lpfnWndProc = s_WndProc,
			hInstance = NativeMethods.GetModuleHandle(null),
			hbrBackground = NativeMethods.GetStockObject(StockObject.NULL_BRUSH),
			lpszMenuName = "",
			lpszClassName = _className
		};
		NativeMethods.RegisterClassEx(ref lpwcx);
		GCHandle gCHandle = default(GCHandle);
		try
		{
			gCHandle = GCHandle.Alloc(this);
			nint lpParam = (nint)gCHandle;
			Handle = NativeMethods.CreateWindowEx(exStyle, _className, name, style, (int)location.X, (int)location.Y, (int)location.Width, (int)location.Height, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, lpParam);
		}
		finally
		{
			gCHandle.Free();
		}
		_dispatcher = Dispatcher.CurrentDispatcher;
	}

	~MessageWindow()
	{
		_Dispose(disposing: false, isHwndBeingDestroyed: false);
	}

	public void Release()
	{
		_Dispose(disposing: true, isHwndBeingDestroyed: false);
		GC.SuppressFinalize(this);
	}

	private void _Dispose(bool disposing, bool isHwndBeingDestroyed)
	{
		if (_isDisposed)
		{
			return;
		}
		_isDisposed = true;
		nint handle = Handle;
		string className = _className;
		if (isHwndBeingDestroyed)
		{
			_dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(_DestroyWindowCallback), new object[2]
			{
				IntPtr.Zero,
				className
			});
		}
		else if (Handle != IntPtr.Zero)
		{
			if (_dispatcher.CheckAccess())
			{
				_DestroyWindow(handle, className);
			}
			else
			{
				_dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(_DestroyWindowCallback), new object[2] { handle, className });
			}
		}
		s_windowLookup.Remove(handle);
		_className = null;
		Handle = IntPtr.Zero;
	}

	private object _DestroyWindowCallback(object arg)
	{
		object[] array = (object[])arg;
		_DestroyWindow((nint)array[0], (string)array[1]);
		return null;
	}

	private static nint _WndProc(nint hwnd, WM msg, nint wParam, nint lParam)
	{
		nint zero = IntPtr.Zero;
		MessageWindow value = null;
		if (msg == WM.CREATE)
		{
			value = (MessageWindow)GCHandle.FromIntPtr(Marshal.PtrToStructure<CREATESTRUCT>(lParam).lpCreateParams).Target;
			s_windowLookup.Add(hwnd, value);
		}
		else if (!s_windowLookup.TryGetValue(hwnd, out value))
		{
			return NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam);
		}
		zero = value._wndProcCallback?.Invoke(hwnd, msg, wParam, lParam) ?? NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam);
		if (msg == WM.NCDESTROY)
		{
			value._Dispose(disposing: true, isHwndBeingDestroyed: true);
			GC.SuppressFinalize(value);
		}
		return zero;
	}

	private static void _DestroyWindow(nint hwnd, string className)
	{
		Utility.SafeDestroyWindow(ref hwnd);
		NativeMethods.UnregisterClass(className, NativeMethods.GetModuleHandle(null));
	}
}
