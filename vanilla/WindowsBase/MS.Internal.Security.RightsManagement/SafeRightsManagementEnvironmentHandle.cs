using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Security.RightsManagement;

internal sealed class SafeRightsManagementEnvironmentHandle : SafeHandle
{
	public override bool IsInvalid => ((IntPtr)handle).Equals(IntPtr.Zero);

	private SafeRightsManagementEnvironmentHandle()
		: base(IntPtr.Zero, ownsHandle: true)
	{
	}

	internal SafeRightsManagementEnvironmentHandle(uint handle)
		: base((nint)handle, ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		int num = 0;
		if (!IsInvalid)
		{
			num = SafeNativeMethods.DRMCloseEnvironmentHandle((uint)handle);
			SetHandle(IntPtr.Zero);
		}
		return num >= 0;
	}
}
