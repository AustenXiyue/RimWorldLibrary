using Microsoft.Win32.SafeHandles;

namespace Standard;

internal sealed class SafeHBITMAP : SafeHandleZeroOrMinusOneIsInvalid
{
	private SafeHBITMAP()
		: base(ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		return NativeMethods.DeleteObject(handle);
	}
}
