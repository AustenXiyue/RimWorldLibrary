using System;

namespace MS.Internal.Interop;

[Flags]
internal enum SLGP
{
	SHORTPATH = 1,
	UNCPRIORITY = 2,
	RAWPATH = 4
}
