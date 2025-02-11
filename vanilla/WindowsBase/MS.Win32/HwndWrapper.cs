using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Interop;
using MS.Internal.WindowsBase;

namespace MS.Win32;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal class HwndWrapper : DispatcherObject, IDisposable
{
	internal class DestroyWindowArgs
	{
		private SecurityCriticalDataClass<nint> _handle;

		private ushort _classAtom;

		public SecurityCriticalDataClass<nint> Handle => _handle;

		public ushort ClassAtom => _classAtom;

		public DestroyWindowArgs(SecurityCriticalDataClass<nint> handle, ushort classAtom)
		{
			_handle = handle;
			_classAtom = classAtom;
		}
	}

	private SecurityCriticalDataClass<nint> _handle;

	private ushort _classAtom;

	private SecurityCriticalDataClass<WeakReferenceList> _hooks;

	private SecurityCriticalDataForSet<int> _ownerThreadID;

	private SecurityCriticalData<HwndWrapperHook> _wndProc;

	private bool _isDisposed;

	private bool _isInCreateWindow;

	private static WindowMessage s_msgGCMemory;

	public nint Handle => _handle?.Value ?? IntPtr.Zero;

	public event EventHandler Disposed;

	static HwndWrapper()
	{
		s_msgGCMemory = UnsafeNativeMethods.RegisterWindowMessage("HwndWrapper.GetGCMemMessage");
	}

	public HwndWrapper(int classStyle, int style, int exStyle, int x, int y, int width, int height, string name, nint parent, HwndWrapperHook[] hooks)
	{
		_ownerThreadID = new SecurityCriticalDataForSet<int>(Environment.CurrentManagedThreadId);
		if (hooks != null)
		{
			int i = 0;
			for (int num = hooks.Length; i < num; i++)
			{
				if (hooks[i] != null)
				{
					AddHook(hooks[i]);
				}
			}
		}
		_wndProc = new SecurityCriticalData<HwndWrapperHook>(WndProc);
		HwndSubclass hwndSubclass = new HwndSubclass(_wndProc.Value);
		NativeMethods.WNDCLASSEX_D wNDCLASSEX_D = new NativeMethods.WNDCLASSEX_D();
		nint num2 = UnsafeNativeMethods.CriticalGetStockObject(5);
		if (num2 == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		nint moduleHandle = UnsafeNativeMethods.GetModuleHandle(null);
		NativeMethods.WndProc wndProc = hwndSubclass.SubclassWndProc;
		string arg = ((AppDomain.CurrentDomain.FriendlyName == null || 128 > AppDomain.CurrentDomain.FriendlyName.Length) ? AppDomain.CurrentDomain.FriendlyName : AppDomain.CurrentDomain.FriendlyName.Substring(0, 128));
		string arg2 = ((Thread.CurrentThread.Name == null || 64 > Thread.CurrentThread.Name.Length) ? Thread.CurrentThread.Name : Thread.CurrentThread.Name.Substring(0, 64));
		_classAtom = 0;
		string arg3 = Guid.NewGuid().ToString();
		string lpszClassName = string.Format(CultureInfo.InvariantCulture, "HwndWrapper[{0};{1};{2}]", arg, arg2, arg3);
		wNDCLASSEX_D.cbSize = Marshal.SizeOf(typeof(NativeMethods.WNDCLASSEX_D));
		wNDCLASSEX_D.style = classStyle;
		wNDCLASSEX_D.lpfnWndProc = wndProc;
		wNDCLASSEX_D.cbClsExtra = 0;
		wNDCLASSEX_D.cbWndExtra = 0;
		wNDCLASSEX_D.hInstance = moduleHandle;
		wNDCLASSEX_D.hIcon = IntPtr.Zero;
		wNDCLASSEX_D.hCursor = IntPtr.Zero;
		wNDCLASSEX_D.hbrBackground = num2;
		wNDCLASSEX_D.lpszMenuName = "";
		wNDCLASSEX_D.lpszClassName = lpszClassName;
		wNDCLASSEX_D.hIconSm = IntPtr.Zero;
		_classAtom = UnsafeNativeMethods.RegisterClassEx(wNDCLASSEX_D);
		_isInCreateWindow = true;
		try
		{
			_handle = new SecurityCriticalDataClass<nint>(UnsafeNativeMethods.CreateWindowEx(exStyle, lpszClassName, name, style, x, y, width, height, new HandleRef(null, parent), new HandleRef(null, IntPtr.Zero), new HandleRef(null, IntPtr.Zero), null));
		}
		finally
		{
			_isInCreateWindow = false;
			if (_handle == null || _handle.Value == IntPtr.Zero)
			{
				hwndSubclass.Dispose();
			}
		}
		GC.KeepAlive(wndProc);
	}

	~HwndWrapper()
	{
		Dispose(disposing: false, isHwndBeingDestroyed: false);
	}

	public virtual void Dispose()
	{
		Dispose(disposing: true, isHwndBeingDestroyed: false);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing, bool isHwndBeingDestroyed)
	{
		if (_isDisposed)
		{
			return;
		}
		if (disposing && this.Disposed != null)
		{
			this.Disposed(this, EventArgs.Empty);
		}
		_isDisposed = true;
		if (isHwndBeingDestroyed)
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(UnregisterClass), _classAtom);
		}
		else if (_handle != null && _handle.Value != IntPtr.Zero)
		{
			if (Environment.CurrentManagedThreadId == _ownerThreadID.Value)
			{
				DestroyWindow(new DestroyWindowArgs(_handle, _classAtom));
			}
			else
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(DestroyWindow), new DestroyWindowArgs(_handle, _classAtom));
			}
		}
		_classAtom = 0;
		_handle = null;
	}

	public void AddHook(HwndWrapperHook hook)
	{
		if (_hooks == null)
		{
			_hooks = new SecurityCriticalDataClass<WeakReferenceList>(new WeakReferenceList());
		}
		_hooks.Value.Insert(0, hook);
	}

	internal void AddHookLast(HwndWrapperHook hook)
	{
		if (_hooks == null)
		{
			_hooks = new SecurityCriticalDataClass<WeakReferenceList>(new WeakReferenceList());
		}
		_hooks.Value.Add(hook);
	}

	public void RemoveHook(HwndWrapperHook hook)
	{
		if (_hooks != null)
		{
			_hooks.Value.Remove(hook);
		}
	}

	private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		nint result = IntPtr.Zero;
		if (_hooks != null)
		{
			WeakReferenceListEnumerator enumerator = _hooks.Value.GetEnumerator();
			while (enumerator.MoveNext())
			{
				result = ((HwndWrapperHook)enumerator.Current)(hwnd, msg, wParam, lParam, ref handled);
				CheckForCreateWindowFailure(result, handled);
				if (handled)
				{
					break;
				}
			}
		}
		if (msg == 130)
		{
			Dispose(disposing: true, isHwndBeingDestroyed: true);
			GC.SuppressFinalize(this);
			handled = false;
		}
		else if (msg == (int)s_msgGCMemory)
		{
			result = (nint)GC.GetTotalMemory(wParam == new IntPtr(1));
			handled = true;
		}
		CheckForCreateWindowFailure(result, handled: true);
		return result;
	}

	private void CheckForCreateWindowFailure(nint result, bool handled)
	{
		if (_isInCreateWindow && IntPtr.Zero != result && handled)
		{
			if (!Debugger.IsAttached)
			{
				throw new InvalidOperationException();
			}
			Debugger.Break();
		}
	}

	internal static object DestroyWindow(object args)
	{
		SecurityCriticalDataClass<nint> handle = ((DestroyWindowArgs)args).Handle;
		ushort classAtom = ((DestroyWindowArgs)args).ClassAtom;
		Invariant.Assert(handle != null && handle.Value != IntPtr.Zero, "Attempting to destroy an invalid hwnd");
		UnsafeNativeMethods.DestroyWindow(new HandleRef(null, handle.Value));
		UnregisterClass(classAtom);
		return null;
	}

	internal static object UnregisterClass(object arg)
	{
		ushort num = (ushort)arg;
		if (num != 0)
		{
			nint moduleHandle = UnsafeNativeMethods.GetModuleHandle(null);
			UnsafeNativeMethods.UnregisterClass(new IntPtr(num), moduleHandle);
		}
		return null;
	}
}
