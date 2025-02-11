using System.Runtime.InteropServices;

namespace Standard;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
internal struct SHFILEOPSTRUCT
{
	public nint hwnd;

	[MarshalAs(UnmanagedType.U4)]
	public FO wFunc;

	public string pFrom;

	public string pTo;

	[MarshalAs(UnmanagedType.U2)]
	public FOF fFlags;

	[MarshalAs(UnmanagedType.Bool)]
	public int fAnyOperationsAborted;

	public nint hNameMappings;

	public string lpszProgressTitle;
}
