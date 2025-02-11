using System;

namespace MS.Internal.TextFormatting;

[Flags]
internal enum LineFlags
{
	None = 0,
	BreakClassWide = 1,
	BreakClassStrict = 2,
	BreakAlways = 4,
	MinMax = 8,
	KeepState = 0x10
}
