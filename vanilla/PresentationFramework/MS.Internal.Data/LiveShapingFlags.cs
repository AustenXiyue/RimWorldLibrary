using System;

namespace MS.Internal.Data;

[Flags]
internal enum LiveShapingFlags
{
	Sorting = 1,
	Filtering = 2,
	Grouping = 4
}
