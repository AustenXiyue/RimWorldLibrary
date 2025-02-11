using System;

namespace Standard;

[Flags]
internal enum CS : uint
{
	VREDRAW = 1u,
	HREDRAW = 2u,
	DBLCLKS = 8u,
	OWNDC = 0x20u,
	CLASSDC = 0x40u,
	PARENTDC = 0x80u,
	NOCLOSE = 0x200u,
	SAVEBITS = 0x800u,
	BYTEALIGNCLIENT = 0x1000u,
	BYTEALIGNWINDOW = 0x2000u,
	GLOBALCLASS = 0x4000u,
	IME = 0x10000u,
	DROPSHADOW = 0x20000u
}
