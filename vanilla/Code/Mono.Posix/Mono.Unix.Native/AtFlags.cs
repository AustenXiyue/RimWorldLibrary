using System;

namespace Mono.Unix.Native;

[Map]
[Flags]
[CLSCompliant(false)]
public enum AtFlags
{
	AT_SYMLINK_NOFOLLOW = 0x100,
	AT_REMOVEDIR = 0x200,
	AT_SYMLINK_FOLLOW = 0x400,
	AT_NO_AUTOMOUNT = 0x800,
	AT_EMPTY_PATH = 0x1000
}
