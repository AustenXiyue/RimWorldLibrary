using System;

namespace MS.Internal.Interop;

[Flags]
internal enum STGM_FLAGS
{
	CREATE = 0x1000,
	MODE = 0x1000,
	READ = 0,
	WRITE = 1,
	READWRITE = 2,
	ACCESS = 3,
	SHARE_DENY_NONE = 0x40,
	SHARE_DENY_READ = 0x30,
	SHARE_DENY_WRITE = 0x20,
	SHARE_EXCLUSIVE = 0x10,
	SHARING = 0x70
}
