using System.Runtime.InteropServices;

namespace MS.Internal.Security.RightsManagement;

[StructLayout(LayoutKind.Sequential)]
internal class ActivationServerInfo
{
	public uint Version;

	[MarshalAs(UnmanagedType.LPWStr)]
	internal string PubKey = "";

	[MarshalAs(UnmanagedType.LPWStr)]
	internal string Url = "";
}
