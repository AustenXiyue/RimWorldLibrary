using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Standard;

internal sealed class SafeDC : SafeHandleZeroOrMinusOneIsInvalid
{
	private static class NativeMethods
	{
		[DllImport("user32.dll")]
		public static extern int ReleaseDC(nint hWnd, nint hDC);

		[DllImport("user32.dll")]
		public static extern SafeDC GetDC(nint hwnd);

		[DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
		public static extern SafeDC CreateDC([MarshalAs(UnmanagedType.LPWStr)] string lpszDriver, [MarshalAs(UnmanagedType.LPWStr)] string lpszDevice, nint lpszOutput, nint lpInitData);

		[DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern SafeDC CreateCompatibleDC(nint hdc);

		[DllImport("gdi32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteDC(nint hdc);
	}

	private nint? _hwnd;

	private bool _created;

	public nint Hwnd
	{
		set
		{
			_hwnd = value;
		}
	}

	private SafeDC()
		: base(ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		if (_created)
		{
			return NativeMethods.DeleteDC(handle);
		}
		if (!_hwnd.HasValue || _hwnd.Value == IntPtr.Zero)
		{
			return true;
		}
		return NativeMethods.ReleaseDC(_hwnd.Value, handle) == 1;
	}

	public static SafeDC CreateDC(string deviceName)
	{
		SafeDC safeDC = null;
		try
		{
			safeDC = NativeMethods.CreateDC(deviceName, null, IntPtr.Zero, IntPtr.Zero);
		}
		finally
		{
			if (safeDC != null)
			{
				safeDC._created = true;
			}
		}
		if (safeDC.IsInvalid)
		{
			safeDC.Dispose();
			throw new SystemException("Unable to create a device context from the specified device information.");
		}
		return safeDC;
	}

	public static SafeDC CreateCompatibleDC(SafeDC hdc)
	{
		SafeDC safeDC = null;
		try
		{
			nint zero = IntPtr.Zero;
			if (hdc != null)
			{
				zero = hdc.handle;
			}
			safeDC = NativeMethods.CreateCompatibleDC(zero);
			if (safeDC == null)
			{
				HRESULT.ThrowLastError();
			}
		}
		finally
		{
			if (safeDC != null)
			{
				safeDC._created = true;
			}
		}
		if (safeDC.IsInvalid)
		{
			safeDC.Dispose();
			throw new SystemException("Unable to create a device context from the specified device information.");
		}
		return safeDC;
	}

	public static SafeDC GetDC(nint hwnd)
	{
		SafeDC safeDC = null;
		try
		{
			safeDC = NativeMethods.GetDC(hwnd);
		}
		finally
		{
			if (safeDC != null)
			{
				safeDC.Hwnd = hwnd;
			}
		}
		if (safeDC.IsInvalid)
		{
			HRESULT.E_FAIL.ThrowIfFailed();
		}
		return safeDC;
	}

	public static SafeDC GetDesktop()
	{
		return GetDC(IntPtr.Zero);
	}

	public static SafeDC WrapDC(nint hdc)
	{
		return new SafeDC
		{
			handle = hdc,
			_created = false,
			_hwnd = IntPtr.Zero
		};
	}
}
