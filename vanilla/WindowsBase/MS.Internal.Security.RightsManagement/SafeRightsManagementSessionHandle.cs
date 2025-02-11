using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Security.RightsManagement;

internal sealed class SafeRightsManagementSessionHandle : SafeHandle
{
	public override bool IsInvalid => ((IntPtr)handle).Equals(IntPtr.Zero);

	private SafeRightsManagementSessionHandle()
		: base(IntPtr.Zero, ownsHandle: true)
	{
	}

	internal SafeRightsManagementSessionHandle(uint handle)
		: base((nint)handle, ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		int num = 0;
		if (!IsInvalid)
		{
			num = SafeNativeMethods.DRMCloseSession((uint)handle);
			SetHandle(IntPtr.Zero);
		}
		return num >= 0;
	}
}
