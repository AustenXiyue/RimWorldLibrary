using System;

namespace MS.Internal.TextFormatting;

internal struct LsPap
{
	[Flags]
	public enum Flags : uint
	{
		None = 0u,
		fFmiVisiCondHyphens = 1u,
		fFmiVisiParaMarks = 2u,
		fFmiVisiSpaces = 4u,
		fFmiVisiTabs = 8u,
		fFmiVisiSplats = 0x10u,
		fFmiVisiBreaks = 0x20u,
		fFmiApplyBreakingRules = 0x40u,
		fFmiApplyOpticalAlignment = 0x80u,
		fFmiPunctStartLine = 0x100u,
		fFmiHangingPunct = 0x200u,
		fFmiPresSuppressWiggle = 0x400u,
		fFmiPresExactSync = 0x800u,
		fFmiAnm = 0x1000u,
		fFmiAutoDecimalTab = 0x2000u,
		fFmiUnderlineTrailSpacesRM = 0x4000u,
		fFmiSpacesInfluenceHeight = 0x8000u,
		fFmiIgnoreSplatBreak = 0x10000u,
		fFmiLimSplat = 0x20000u,
		fFmiAllowSplatLine = 0x40000u,
		fFmiForceBreakAsNext = 0x80000u,
		fFmiAllowHyphenation = 0x100000u,
		fFmiDrawInCharCodes = 0x200000u,
		fFmiTreatHyphenAsRegular = 0x400000u,
		fFmiWrapTrailingSpaces = 0x800000u,
		fFmiWrapAllSpaces = 0x1000000u,
		fFmiFCheckTruncateBefore = 0x2000000u,
		fFmiForgetLastTabAlignment = 0x10000000u,
		fFmiIndentChangesHyphenZone = 0x20000000u,
		fFmiNoPunctAfterAutoNumber = 0x40000000u,
		fFmiResolveTabsAsWord97 = 0x80000000u
	}

	public int cpFirst;

	public int cpFirstContent;

	public Flags grpf;

	public LsBreakJust lsbrj;

	public LsKJust lskj;

	public int fJustify;

	public int durAutoDecimalTab;

	public LsKEOP lskeop;

	public LsTFlow lstflow;
}
