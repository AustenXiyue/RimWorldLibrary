using Microsoft.Win32.SafeHandles;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

internal class SafeProfileHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	internal SafeProfileHandle()
		: base(ownsHandle: true)
	{
	}

	internal SafeProfileHandle(nint profile)
		: base(ownsHandle: true)
	{
		SetHandle(profile);
	}

	protected override bool ReleaseHandle()
	{
		return UnsafeNativeMethods.Mscms.CloseColorProfile(handle);
	}
}
