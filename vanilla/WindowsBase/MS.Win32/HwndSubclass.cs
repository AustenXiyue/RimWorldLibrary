using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using MS.Internal.Interop;
using MS.Internal.WindowsBase;

namespace MS.Win32;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal class HwndSubclass : IDisposable
{
	private class DispatcherOperationCallbackParameter
	{
		internal nint hwnd;

		internal nint wParam;

		internal nint lParam;

		internal nint retVal;

		internal int msg;

		internal bool handled;
	}

	private enum Bond
	{
		Unattached,
		Attached,
		Detached,
		Orphaned
	}

	[ThreadStatic]
	private static DispatcherOperationCallbackParameter _paramDispatcherCallbackOperation;

	private DispatcherOperationCallback _dispatcherOperationCallback;

	internal static readonly WindowMessage DetachMessage;

	private static NativeMethods.WndProc DefWndProcStub;

	private static nint DefWndProc;

	private nint _hwndAttached;

	private HandleRef _hwndHandleRef;

	private NativeMethods.WndProc _attachedWndProc;

	private nint _oldWndProc;

	private Bond _bond;

	private GCHandle _gcHandle;

	private WeakReference _hook;

	static HwndSubclass()
	{
		DefWndProcStub = DefWndProcWrapper;
		DetachMessage = UnsafeNativeMethods.RegisterWindowMessage("HwndSubclass.DetachMessage");
		nint moduleHandle = UnsafeNativeMethods.GetModuleHandle("user32.dll");
		DefWndProc = UnsafeNativeMethods.GetProcAddress(new HandleRef(null, moduleHandle), "DefWindowProcW");
	}

	internal HwndSubclass(HwndWrapperHook hook)
	{
		if (hook == null)
		{
			throw new ArgumentNullException("hook");
		}
		_bond = Bond.Unattached;
		_hook = new WeakReference(hook);
		_gcHandle = GCHandle.Alloc(this);
	}

	public virtual void Dispose()
	{
		DisposeImpl(forceUnhook: false);
	}

	private bool DisposeImpl(bool forceUnhook)
	{
		_hook = null;
		return UnhookWindowProc(forceUnhook);
	}

	internal nint Attach(nint hwnd)
	{
		if (_bond != 0)
		{
			throw new InvalidOperationException(SR.HwndSubclassMultipleAttach);
		}
		return CriticalAttach(hwnd);
	}

	internal bool Detach(bool force)
	{
		return CriticalDetach(force);
	}

	internal bool CriticalDetach(bool force)
	{
		if (_bond == Bond.Detached || _bond == Bond.Unattached)
		{
			return true;
		}
		_bond = Bond.Orphaned;
		return DisposeImpl(force);
	}

	internal void RequestDetach(bool force)
	{
		if (_hwndAttached != IntPtr.Zero)
		{
			RequestDetach(_hwndAttached, (nint)_gcHandle, force);
		}
	}

	internal static void RequestDetach(nint hwnd, nint subclass, bool force)
	{
		if (hwnd == IntPtr.Zero)
		{
			throw new ArgumentNullException("hwnd");
		}
		if (subclass == IntPtr.Zero)
		{
			throw new ArgumentNullException("subclass");
		}
		int num = (force ? 1 : 0);
		UnsafeNativeMethods.UnsafeSendMessage(hwnd, DetachMessage, subclass, num);
	}

	internal nint SubclassWndProc(nint hwnd, int msg, nint wParam, nint lParam)
	{
		nint result = IntPtr.Zero;
		bool flag = false;
		if (_bond == Bond.Unattached)
		{
			HookWindowProc(hwnd, SubclassWndProc, Marshal.GetFunctionPointerForDelegate(DefWndProcStub));
		}
		else if (_bond == Bond.Detached)
		{
			throw new InvalidOperationException();
		}
		nint oldWndProc = _oldWndProc;
		if (msg == (int)DetachMessage)
		{
			if (wParam == IntPtr.Zero || wParam == (nint)_gcHandle)
			{
				int num = (int)lParam;
				bool force = num > 0;
				result = (CriticalDetach(force) ? new IntPtr(1) : IntPtr.Zero);
				flag = num < 2;
			}
		}
		else
		{
			Dispatcher dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
			if (dispatcher != null && !dispatcher.HasShutdownFinished)
			{
				if (_dispatcherOperationCallback == null)
				{
					_dispatcherOperationCallback = DispatcherCallbackOperation;
				}
				if (_paramDispatcherCallbackOperation == null)
				{
					_paramDispatcherCallbackOperation = new DispatcherOperationCallbackParameter();
				}
				DispatcherOperationCallbackParameter paramDispatcherCallbackOperation = _paramDispatcherCallbackOperation;
				_paramDispatcherCallbackOperation = null;
				paramDispatcherCallbackOperation.hwnd = hwnd;
				paramDispatcherCallbackOperation.msg = msg;
				paramDispatcherCallbackOperation.wParam = wParam;
				paramDispatcherCallbackOperation.lParam = lParam;
				if (dispatcher.Invoke(DispatcherPriority.Send, _dispatcherOperationCallback, paramDispatcherCallbackOperation) != null)
				{
					flag = paramDispatcherCallbackOperation.handled;
					result = paramDispatcherCallbackOperation.retVal;
				}
				_paramDispatcherCallbackOperation = paramDispatcherCallbackOperation;
			}
			if (msg == 130)
			{
				CriticalDetach(force: true);
				flag = false;
			}
		}
		if (!flag)
		{
			result = CallOldWindowProc(oldWndProc, hwnd, (WindowMessage)msg, wParam, lParam);
		}
		return result;
	}

	internal nint CriticalAttach(nint hwnd)
	{
		if (hwnd == IntPtr.Zero)
		{
			throw new ArgumentNullException("hwnd");
		}
		if (_bond != 0)
		{
			throw new InvalidOperationException();
		}
		NativeMethods.WndProc newWndProc = SubclassWndProc;
		nint windowLongPtr = UnsafeNativeMethods.GetWindowLongPtr(new HandleRef(this, hwnd), -4);
		HookWindowProc(hwnd, newWndProc, windowLongPtr);
		return (nint)_gcHandle;
	}

	private object DispatcherCallbackOperation(object o)
	{
		DispatcherOperationCallbackParameter dispatcherOperationCallbackParameter = (DispatcherOperationCallbackParameter)o;
		dispatcherOperationCallbackParameter.handled = false;
		dispatcherOperationCallbackParameter.retVal = IntPtr.Zero;
		if (_bond == Bond.Attached && _hook.Target is HwndWrapperHook hwndWrapperHook)
		{
			dispatcherOperationCallbackParameter.retVal = hwndWrapperHook(dispatcherOperationCallbackParameter.hwnd, dispatcherOperationCallbackParameter.msg, dispatcherOperationCallbackParameter.wParam, dispatcherOperationCallbackParameter.lParam, ref dispatcherOperationCallbackParameter.handled);
		}
		return dispatcherOperationCallbackParameter;
	}

	private nint CallOldWindowProc(nint oldWndProc, nint hwnd, WindowMessage msg, nint wParam, nint lParam)
	{
		return UnsafeNativeMethods.CallWindowProc(oldWndProc, hwnd, (int)msg, wParam, lParam);
	}

	private void HookWindowProc(nint hwnd, NativeMethods.WndProc newWndProc, nint oldWndProc)
	{
		_hwndAttached = hwnd;
		_hwndHandleRef = new HandleRef(null, _hwndAttached);
		_bond = Bond.Attached;
		_attachedWndProc = newWndProc;
		_oldWndProc = oldWndProc;
		UnsafeNativeMethods.CriticalSetWindowLong(_hwndHandleRef, -4, _attachedWndProc);
		ManagedWndProcTracker.TrackHwndSubclass(this, _hwndAttached);
	}

	private bool UnhookWindowProc(bool force)
	{
		if (_bond == Bond.Unattached || _bond == Bond.Detached)
		{
			return true;
		}
		if (!force)
		{
			force = UnsafeNativeMethods.GetWindowLongWndProc(new HandleRef(this, _hwndAttached)) == _attachedWndProc;
		}
		if (!force)
		{
			return false;
		}
		_bond = Bond.Orphaned;
		ManagedWndProcTracker.UnhookHwndSubclass(this);
		try
		{
			UnsafeNativeMethods.CriticalSetWindowLong(_hwndHandleRef, -4, _oldWndProc);
		}
		catch (Win32Exception ex)
		{
			if (ex.NativeErrorCode != 1400)
			{
				throw;
			}
		}
		_bond = Bond.Detached;
		_oldWndProc = IntPtr.Zero;
		_attachedWndProc = null;
		_hwndAttached = IntPtr.Zero;
		_hwndHandleRef = new HandleRef(null, IntPtr.Zero);
		if (_gcHandle.IsAllocated)
		{
			_gcHandle.Free();
		}
		return true;
	}

	private static nint DefWndProcWrapper(nint hwnd, int msg, nint wParam, nint lParam)
	{
		return UnsafeNativeMethods.CallWindowProc(DefWndProc, hwnd, msg, wParam, lParam);
	}
}
