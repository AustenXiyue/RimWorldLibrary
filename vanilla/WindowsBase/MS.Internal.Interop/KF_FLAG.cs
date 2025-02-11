using System;

namespace MS.Internal.Interop;

[Flags]
internal enum KF_FLAG : uint
{
	DEFAULT = 0u,
	CREATE = 0x8000u,
	DONT_VERIFY = 0x4000u,
	DONT_UNEXPAND = 0x2000u,
	NO_ALIAS = 0x1000u,
	INIT = 0x800u,
	DEFAULT_PATH = 0x400u,
	NOT_PARENT_RELATIVE = 0x200u,
	SIMPLE_IDLIST = 0x100u,
	ALIAS_ONLY = 0x80000000u
}
