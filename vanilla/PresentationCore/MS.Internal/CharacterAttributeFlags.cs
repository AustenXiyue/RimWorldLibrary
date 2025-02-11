namespace MS.Internal;

internal enum CharacterAttributeFlags : ushort
{
	CharacterComplex = 1,
	CharacterRTL = 2,
	CharacterLineBreak = 4,
	CharacterFormatAnchor = 8,
	CharacterFastText = 0x10,
	CharacterIdeo = 0x20,
	CharacterExtended = 0x40,
	CharacterSpace = 0x80,
	CharacterDigit = 0x100,
	CharacterParaBreak = 0x200,
	CharacterCRLF = 0x400,
	CharacterLetter = 0x800
}
