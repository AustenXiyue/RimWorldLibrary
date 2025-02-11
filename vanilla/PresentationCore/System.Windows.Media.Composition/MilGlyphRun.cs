namespace System.Windows.Media.Composition;

[Flags]
internal enum MilGlyphRun : ushort
{
	Sideways = 1,
	HasOffsets = 0x10,
	FORCE_WORD = ushort.MaxValue
}
