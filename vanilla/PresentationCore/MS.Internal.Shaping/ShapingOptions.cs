using System;

namespace MS.Internal.Shaping;

[Flags]
internal enum ShapingOptions
{
	None = 0,
	DisplayControlCode = 1,
	InhibitLigature = 2
}
