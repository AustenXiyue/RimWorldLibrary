using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Security.RightsManagement;

internal sealed class SafeRightsManagementQueryHandle : SafeHandle
{
	public override bool IsInvalid => ((IntPtr)handle).Equals(IntPtr.Zero);

	private SafeRightsManagementQueryHandle()
		: base(IntPtr.Zero, ownsHandle: true)
	{
	}

	internal SafeRightsManagementQueryHandle(uint handle)
		: base((nint)handle, ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		int num = 0;
		if (!IsInvalid)
		{
			num = SafeNativeMethods.DRMCloseQueryHandle((uint)handle);
			SetHandle(IntPtr.Zero);
		}
		return num >= 0;
	}
}
