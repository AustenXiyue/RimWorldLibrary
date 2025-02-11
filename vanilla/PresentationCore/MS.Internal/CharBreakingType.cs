namespace MS.Internal;

internal enum CharBreakingType : byte
{
	NoBreak = 0,
	ControlBreak = 1,
	DigitBreak = 2,
	PairMirrorBreak = 4,
	SingleMirrorBreak = 8
}
