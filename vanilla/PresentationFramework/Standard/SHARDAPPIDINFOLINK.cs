using System.Runtime.InteropServices;

namespace Standard;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal class SHARDAPPIDINFOLINK
{
	private nint psl;

	[MarshalAs(UnmanagedType.LPWStr)]
	private string pszAppID;
}
