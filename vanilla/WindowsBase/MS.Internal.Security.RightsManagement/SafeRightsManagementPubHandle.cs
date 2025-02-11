using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Security.RightsManagement;

internal sealed class SafeRightsManagementPubHandle : SafeHandle
{
	private static readonly SafeRightsManagementPubHandle _invalidHandle = new SafeRightsManagementPubHandle(0u);

	public override bool IsInvalid => ((IntPtr)handle).Equals(IntPtr.Zero);

	internal static SafeRightsManagementPubHandle InvalidHandle => _invalidHandle;

	private SafeRightsManagementPubHandle()
		: base(IntPtr.Zero, ownsHandle: true)
	{
	}

	internal SafeRightsManagementPubHandle(uint handle)
		: base((nint)handle, ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		int num = 0;
		if (!IsInvalid)
		{
			num = SafeNativeMethods.DRMClosePubHandle((uint)handle);
			SetHandle(IntPtr.Zero);
		}
		return num >= 0;
	}
}
