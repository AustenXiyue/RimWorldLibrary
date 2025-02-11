using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace MS.Internal.FontCache;

[FriendAccessAllowed]
internal class FileMapping : UnmanagedMemoryStream
{
	private MS.Win32.UnsafeNativeMethods.SafeViewOfFileHandle _viewHandle;

	private MS.Win32.UnsafeNativeMethods.SafeFileMappingHandle _mappingHandle;

	private bool _disposed;

	~FileMapping()
	{
		Dispose(disposing: false);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (!_disposed)
		{
			if (disposing)
			{
				if (_viewHandle != null)
				{
					_viewHandle.Dispose();
				}
				if (_mappingHandle != null)
				{
					_mappingHandle.Dispose();
				}
			}
			Invariant.Assert(!CanWrite);
		}
		_disposed = true;
	}

	internal unsafe void OpenFile(string fileName)
	{
		MS.Win32.NativeMethods.SECURITY_ATTRIBUTES sECURITY_ATTRIBUTES = new MS.Win32.NativeMethods.SECURITY_ATTRIBUTES();
		try
		{
			long quadPart;
			using (SafeFileHandle safeFileHandle = MS.Win32.UnsafeNativeMethods.CreateFile(fileName, 2147483648u, 1u, null, 3, 0, IntPtr.Zero))
			{
				if (safeFileHandle.IsInvalid)
				{
					Util.ThrowWin32Exception(Marshal.GetLastWin32Error(), fileName);
				}
				MS.Win32.UnsafeNativeMethods.LARGE_INTEGER lpFileSize = default(MS.Win32.UnsafeNativeMethods.LARGE_INTEGER);
				if (!MS.Win32.UnsafeNativeMethods.GetFileSizeEx(safeFileHandle, ref lpFileSize))
				{
					throw new IOException(SR.Format(SR.IOExceptionWithFileName, fileName));
				}
				quadPart = lpFileSize.QuadPart;
				if (quadPart == 0L)
				{
					throw new FileFormatException(new Uri(fileName));
				}
				_mappingHandle = MS.Win32.UnsafeNativeMethods.CreateFileMapping(safeFileHandle, sECURITY_ATTRIBUTES, 2, 0u, 0u, null);
			}
			if (_mappingHandle.IsInvalid)
			{
				throw new IOException(SR.Format(SR.IOExceptionWithFileName, fileName));
			}
			_viewHandle = MS.Win32.UnsafeNativeMethods.MapViewOfFileEx(_mappingHandle, 4, 0, 0, IntPtr.Zero, IntPtr.Zero);
			if (_viewHandle.IsInvalid)
			{
				throw new IOException(SR.Format(SR.IOExceptionWithFileName, fileName));
			}
			Initialize((byte*)_viewHandle.Memory, quadPart, quadPart, FileAccess.Read);
		}
		finally
		{
			sECURITY_ATTRIBUTES.Release();
			sECURITY_ATTRIBUTES = null;
		}
	}
}
