using System.Runtime.InteropServices;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

internal struct ColorContextHelper
{
	private SafeProfileHandle _profileHandle;

	internal bool IsInvalid
	{
		get
		{
			if (_profileHandle != null)
			{
				return _profileHandle.IsInvalid;
			}
			return true;
		}
	}

	internal SafeProfileHandle ProfileHandle => _profileHandle;

	internal void OpenColorProfile(ref MS.Win32.UnsafeNativeMethods.PROFILE profile)
	{
		_profileHandle = UnsafeNativeMethods.Mscms.OpenColorProfile(ref profile, 1u, 1u, 3u);
	}

	internal bool GetColorProfileHeader(out MS.Win32.UnsafeNativeMethods.PROFILEHEADER header)
	{
		if (IsInvalid)
		{
			throw new InvalidOperationException(SR.Image_ColorContextInvalid);
		}
		return UnsafeNativeMethods.Mscms.GetColorProfileHeader(_profileHandle, out header);
	}

	internal void GetColorProfileFromHandle(byte[] buffer, ref uint bufferSize)
	{
		Invariant.Assert(buffer == null || bufferSize <= buffer.Length);
		if (IsInvalid)
		{
			throw new InvalidOperationException(SR.Image_ColorContextInvalid);
		}
		if (!UnsafeNativeMethods.Mscms.GetColorProfileFromHandle(_profileHandle, buffer, ref bufferSize) && buffer != null)
		{
			HRESULT.Check(Marshal.GetHRForLastWin32Error());
		}
	}
}
