using System;

namespace MonoMod.Core.Utils;

[Flags]
internal enum AddressKind
{
	Rel32 = 0,
	Rel64 = 2,
	Abs32 = 1,
	Abs64 = 3,
	PrecodeFixupThunkRel32 = 4,
	PrecodeFixupThunkRel64 = 6,
	PrecodeFixupThunkAbs32 = 5,
	PrecodeFixupThunkAbs64 = 7,
	Indirect = 8
}
