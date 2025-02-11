using System;

namespace MS.Internal.Shaping;

[Flags]
internal enum TagInfoFlags : uint
{
	Substitution = 1u,
	Positioning = 2u,
	Both = 3u,
	None = 0u
}
