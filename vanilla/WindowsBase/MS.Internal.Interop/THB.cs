using System;

namespace MS.Internal.Interop;

[Flags]
internal enum THB : uint
{
	BITMAP = 1u,
	ICON = 2u,
	TOOLTIP = 4u,
	FLAGS = 8u
}
