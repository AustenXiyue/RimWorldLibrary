using System;
using System.Runtime.InteropServices;

namespace Standard;

[StructLayout(LayoutKind.Sequential)]
internal class NOTIFYICONDATA
{
	public int cbSize;

	public nint hWnd;

	public int uID;

	public NIF uFlags;

	public int uCallbackMessage;

	public nint hIcon;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
	public char[] szTip = new char[128];

	public uint dwState;

	public uint dwStateMask;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
	public char[] szInfo = new char[256];

	public uint uVersion;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
	public char[] szInfoTitle = new char[64];

	public uint dwInfoFlags;

	public Guid guidItem;

	private nint hBalloonIcon;
}
