using MS.Internal;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

internal class SafeMediaHandle : SafeMILHandle
{
	internal SafeMediaHandle()
	{
	}

	internal SafeMediaHandle(nint handle)
	{
		SetHandle(handle);
	}

	protected override bool ReleaseHandle()
	{
		HRESULT.Check(MILMedia.Shutdown(handle));
		UnsafeNativeMethods.MILUnknown.ReleaseInterface(ref handle);
		return true;
	}
}
