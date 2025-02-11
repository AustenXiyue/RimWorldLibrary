using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Security.RightsManagement;

internal sealed class SafeRightsManagementHandle : SafeHandle
{
	private static readonly SafeRightsManagementHandle _invalidHandle = new SafeRightsManagementHandle(0u);

	public override bool IsInvalid => ((IntPtr)handle).Equals(IntPtr.Zero);

	internal static SafeRightsManagementHandle InvalidHandle => _invalidHandle;

	private SafeRightsManagementHandle()
		: base(IntPtr.Zero, ownsHandle: true)
	{
	}

	internal SafeRightsManagementHandle(uint handle)
		: base((nint)handle, ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		int num = 0;
		if (!IsInvalid)
		{
			num = SafeNativeMethods.DRMCloseHandle((uint)handle);
			SetHandle(IntPtr.Zero);
		}
		return num >= 0;
	}
}
