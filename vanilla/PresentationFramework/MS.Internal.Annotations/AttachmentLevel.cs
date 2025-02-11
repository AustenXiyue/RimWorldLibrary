using System;

namespace MS.Internal.Annotations;

[Flags]
internal enum AttachmentLevel
{
	Full = 7,
	StartPortion = 4,
	MiddlePortion = 2,
	EndPortion = 1,
	Incomplete = 0x100,
	Unresolved = 0
}
