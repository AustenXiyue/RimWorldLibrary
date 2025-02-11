using System;

namespace MS.Internal.Shaping;

[Flags]
internal enum GlyphFlags : ushort
{
	Unassigned = 0,
	Base = 1,
	Ligature = 2,
	Mark = 3,
	Component = 4,
	Unresolved = 7,
	GlyphTypeMask = 7,
	Substituted = 0x10,
	Positioned = 0x20,
	NotChanged = 0,
	CursiveConnected = 0x40,
	ClusterStart = 0x100,
	Diacritic = 0x200,
	ZeroWidth = 0x400,
	Missing = 0x800,
	InvalidBase = 0x1000
}
