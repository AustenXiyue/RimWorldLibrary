using System.Runtime.InteropServices;

namespace Standard;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct CREATESTRUCT
{
	public nint lpCreateParams;

	public nint hInstance;

	public nint hMenu;

	public nint hwndParent;

	public int cy;

	public int cx;

	public int y;

	public int x;

	public WS style;

	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpszName;

	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpszClass;

	public WS_EX dwExStyle;
}
