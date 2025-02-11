using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Security.RightsManagement;

[StructLayout(LayoutKind.Sequential)]
internal class BoundLicenseParams
{
	internal uint uVersion;

	internal uint hEnablingPrincipal;

	internal uint hSecureStore;

	[MarshalAs(UnmanagedType.LPWStr)]
	public string wszRightsRequested;

	[MarshalAs(UnmanagedType.LPWStr)]
	public string wszRightsGroup;

	internal uint DRMIDuVersion;

	[MarshalAs(UnmanagedType.LPWStr)]
	public string DRMIDIdType;

	[MarshalAs(UnmanagedType.LPWStr)]
	public string DRMIDId;

	internal uint cAuthenticatorCount;

	internal nint rghAuthenticators = IntPtr.Zero;

	[MarshalAs(UnmanagedType.LPWStr)]
	public string wszDefaultEnablingPrincipalCredentials;

	internal uint dwFlags;
}
