using System;

namespace MS.Internal.TextFormatting;

internal struct LsChp
{
	[Flags]
	public enum Flags : uint
	{
		None = 0u,
		fApplyKern = 1u,
		fModWidthOnRun = 2u,
		fModWidthSpace = 4u,
		fModWidthPairs = 8u,
		fCompressOnRun = 0x10u,
		fCompressSpace = 0x20u,
		fCompressTable = 0x40u,
		fExpandOnRun = 0x80u,
		fExpandSpace = 0x100u,
		fExpandTable = 0x200u,
		fGlyphBased = 0x400u,
		fInvisible = 0x10000u,
		fUnderline = 0x20000u,
		fStrike = 0x40000u,
		fShade = 0x80000u,
		fBorder = 0x100000u,
		fSymbol = 0x200000u,
		fHyphen = 0x400000u,
		fCheckForReplaceChar = 0x800000u
	}

	public ushort idObj;

	public ushort dcpMaxContent;

	public uint effectsFlags;

	public Flags flags;

	public int dvpPos;
}
