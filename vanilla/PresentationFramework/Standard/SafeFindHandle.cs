using Microsoft.Win32.SafeHandles;

namespace Standard;

internal sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	private SafeFindHandle()
		: base(ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		return NativeMethods.FindClose(handle);
	}
}
