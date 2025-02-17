using System;

namespace Standard;

[Flags]
internal enum WS : uint
{
	OVERLAPPED = 0u,
	POPUP = 0x80000000u,
	CHILD = 0x40000000u,
	MINIMIZE = 0x20000000u,
	VISIBLE = 0x10000000u,
	DISABLED = 0x8000000u,
	CLIPSIBLINGS = 0x4000000u,
	CLIPCHILDREN = 0x2000000u,
	MAXIMIZE = 0x1000000u,
	BORDER = 0x800000u,
	DLGFRAME = 0x400000u,
	VSCROLL = 0x200000u,
	HSCROLL = 0x100000u,
	SYSMENU = 0x80000u,
	THICKFRAME = 0x40000u,
	GROUP = 0x20000u,
	TABSTOP = 0x10000u,
	MINIMIZEBOX = 0x20000u,
	MAXIMIZEBOX = 0x10000u,
	CAPTION = 0xC00000u,
	TILED = 0u,
	ICONIC = 0x20000000u,
	SIZEBOX = 0x40000u,
	TILEDWINDOW = 0xCF0000u,
	OVERLAPPEDWINDOW = 0xCF0000u,
	POPUPWINDOW = 0x80880000u,
	CHILDWINDOW = 0x40000000u
}
