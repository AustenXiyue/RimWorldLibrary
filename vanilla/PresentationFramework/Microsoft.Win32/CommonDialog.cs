using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using MS.Win32;

namespace Microsoft.Win32;

/// <summary>An abstract base class for displaying common Win32 dialogs.</summary>
public abstract class CommonDialog
{
	private object _userData;

	private Thread _thread = Thread.CurrentThread;

	private nint _hwndOwnerWindow;

	/// <summary>Gets or sets an object associated with the dialog. This provides the ability to attach an arbitrary object to the dialog.</summary>
	/// <returns>A <see cref="T:System.Object" /> that is attached or associated with a dialog.</returns>
	public object Tag
	{
		get
		{
			return _userData;
		}
		set
		{
			_userData = value;
		}
	}

	/// <summary>When overridden in a derived class, resets the properties of a common dialog to their default values.</summary>
	public abstract void Reset();

	/// <summary>Displays a common dialog.</summary>
	/// <returns>If the user clicks the OK button of the dialog that is displayed (e.g. <see cref="T:Microsoft.Win32.OpenFileDialog" />, <see cref="T:Microsoft.Win32.SaveFileDialog" />), true is returned; otherwise, false.</returns>
	public virtual bool? ShowDialog()
	{
		CheckPermissionsToShowDialog();
		if (!Environment.UserInteractive)
		{
			throw new InvalidOperationException(SR.CantShowModalOnNonInteractive);
		}
		nint num = MS.Win32.UnsafeNativeMethods.GetActiveWindow();
		if (num == IntPtr.Zero && Application.Current != null)
		{
			num = Application.Current.ParkingHwnd;
		}
		HwndWrapper hwndWrapper = null;
		try
		{
			if (num == IntPtr.Zero)
			{
				hwndWrapper = new HwndWrapper(0, 0, 0, 0, 0, 0, 0, "", IntPtr.Zero, null);
				num = hwndWrapper.Handle;
			}
			_hwndOwnerWindow = num;
			try
			{
				ComponentDispatcher.CriticalPushModal();
				return RunDialog(num);
			}
			finally
			{
				ComponentDispatcher.CriticalPopModal();
			}
		}
		finally
		{
			hwndWrapper?.Dispose();
		}
	}

	/// <summary>Displays a common dialog.</summary>
	/// <returns>If the user clicks the OK button of the dialog that is displayed (e.g. <see cref="T:Microsoft.Win32.OpenFileDialog" />, <see cref="T:Microsoft.Win32.SaveFileDialog" />), true is returned; otherwise, false.</returns>
	/// <param name="owner">Handle to the window that owns the dialog. </param>
	public bool? ShowDialog(Window owner)
	{
		CheckPermissionsToShowDialog();
		if (owner == null)
		{
			return ShowDialog();
		}
		if (!Environment.UserInteractive)
		{
			throw new InvalidOperationException(SR.CantShowModalOnNonInteractive);
		}
		nint criticalHandle = new WindowInteropHelper(owner).CriticalHandle;
		if (criticalHandle == IntPtr.Zero)
		{
			throw new InvalidOperationException();
		}
		_hwndOwnerWindow = criticalHandle;
		try
		{
			ComponentDispatcher.CriticalPushModal();
			return RunDialog(criticalHandle);
		}
		finally
		{
			ComponentDispatcher.CriticalPopModal();
		}
	}

	protected virtual nint HookProc(nint hwnd, int msg, nint wParam, nint lParam)
	{
		if (msg == 272)
		{
			MoveToScreenCenter(new HandleRef(this, hwnd));
			return new IntPtr(1);
		}
		return IntPtr.Zero;
	}

	protected abstract bool RunDialog(nint hwndOwner);

	/// <summary>Determines whether sufficient permissions for displaying a dialog exist.</summary>
	/// <exception cref="T:System.Security.SecurityException">if sufficient permissions do not exist to display a dialog.</exception>
	protected virtual void CheckPermissionsToShowDialog()
	{
		if (_thread != Thread.CurrentThread)
		{
			throw new InvalidOperationException(SR.CantShowOnDifferentThread);
		}
	}

	private void MoveToScreenCenter(HandleRef hWnd)
	{
		nint zero = IntPtr.Zero;
		if (_hwndOwnerWindow != IntPtr.Zero)
		{
			zero = SafeNativeMethods.MonitorFromWindow(new HandleRef(this, _hwndOwnerWindow), 2);
			if (zero != IntPtr.Zero)
			{
				MS.Win32.NativeMethods.RECT rect = default(MS.Win32.NativeMethods.RECT);
				SafeNativeMethods.GetWindowRect(hWnd, ref rect);
				Size currentSizeDeviceUnits = new Size(rect.right - rect.left, rect.bottom - rect.top);
				double leftDeviceUnits = 0.0;
				double topDeviceUnits = 0.0;
				Window.CalculateCenterScreenPosition(zero, currentSizeDeviceUnits, ref leftDeviceUnits, ref topDeviceUnits);
				MS.Win32.UnsafeNativeMethods.SetWindowPos(hWnd, MS.Win32.NativeMethods.NullHandleRef, (int)Math.Round(leftDeviceUnits), (int)Math.Round(topDeviceUnits), 0, 0, 21);
			}
		}
	}

	/// <summary>Provides initialization for base class values when called by the constructor of a derived class.</summary>
	protected CommonDialog()
	{
	}
}
