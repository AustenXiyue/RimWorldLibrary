using System;

namespace MS.Internal.Interop;

[Flags]
internal enum TBPF
{
	NOPROGRESS = 0,
	INDETERMINATE = 1,
	NORMAL = 2,
	ERROR = 4,
	PAUSED = 8
}
