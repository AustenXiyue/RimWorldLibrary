using System.Runtime.InteropServices;

namespace Standard;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal class SHARDAPPIDINFOIDLIST
{
	private nint pidl;

	[MarshalAs(UnmanagedType.LPWStr)]
	private string pszAppID;
}
