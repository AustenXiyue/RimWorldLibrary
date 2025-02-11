using Microsoft.Win32.SafeHandles;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

internal class ColorTransformHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	internal ColorTransformHandle()
		: base(ownsHandle: true)
	{
	}

	internal ColorTransformHandle(nint profile)
		: base(ownsHandle: true)
	{
		SetHandle(profile);
	}

	protected override bool ReleaseHandle()
	{
		return UnsafeNativeMethods.Mscms.DeleteColorTransform(handle);
	}
}
