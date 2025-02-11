using System.Runtime.InteropServices;

namespace MS.Internal.TextFormatting;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct LsContextInfo
{
	public uint version;

	public int cInstalledHandlers;

	public nint plsimethods;

	public int cEstimatedCharsPerLine;

	public int cJustPriorityLim;

	public char wchUndef;

	public char wchNull;

	public char wchSpace;

	public char wchHyphen;

	public char wchTab;

	public char wchPosTab;

	public char wchEndPara1;

	public char wchEndPara2;

	public char wchAltEndPara;

	public char wchEndLineInPara;

	public char wchColumnBreak;

	public char wchSectionBreak;

	public char wchPageBreak;

	public char wchNonBreakSpace;

	public char wchNonBreakHyphen;

	public char wchNonReqHyphen;

	public char wchEmDash;

	public char wchEnDash;

	public char wchEmSpace;

	public char wchEnSpace;

	public char wchNarrowSpace;

	public char wchOptBreak;

	public char wchNoBreak;

	public char wchFESpace;

	public char wchJoiner;

	public char wchNonJoiner;

	public char wchToReplace;

	public char wchReplace;

	public char wchVisiNull;

	public char wchVisiAltEndPara;

	public char wchVisiEndLineInPara;

	public char wchVisiEndPara;

	public char wchVisiSpace;

	public char wchVisiNonBreakSpace;

	public char wchVisiNonBreakHyphen;

	public char wchVisiNonReqHyphen;

	public char wchVisiTab;

	public char wchVisiPosTab;

	public char wchVisiEmSpace;

	public char wchVisiEnSpace;

	public char wchVisiNarrowSpace;

	public char wchVisiOptBreak;

	public char wchVisiNoBreak;

	public char wchVisiFESpace;

	public char wchEscAnmRun;

	public char wchPad;

	public nint pols;

	public nint pfnNewPtr;

	public nint pfnDisposePtr;

	public nint pfnReallocPtr;

	public nint pfnFetchRun;

	public GetAutoNumberInfo pfnGetAutoNumberInfo;

	public nint pfnGetNumericSeparators;

	public nint pfnCheckForDigit;

	public FetchPap pfnFetchPap;

	public FetchLineProps pfnFetchLineProps;

	public nint pfnFetchTabs;

	public nint pfnReleaseTabsBuffer;

	public nint pfnGetBreakThroughTab;

	public nint pfnGetPosTabProps;

	public nint pfnFGetLastLineJustification;

	public nint pfnCheckParaBoundaries;

	public GetRunCharWidths pfnGetRunCharWidths;

	public nint pfnCheckRunKernability;

	public nint pfnGetRunCharKerning;

	public GetRunTextMetrics pfnGetRunTextMetrics;

	public GetRunUnderlineInfo pfnGetRunUnderlineInfo;

	public GetRunStrikethroughInfo pfnGetRunStrikethroughInfo;

	public nint pfnGetBorderInfo;

	public nint pfnReleaseRun;

	public nint pfnReleaseRunBuffer;

	public Hyphenate pfnHyphenate;

	public GetPrevHyphenOpp pfnGetPrevHyphenOpp;

	public GetNextHyphenOpp pfnGetNextHyphenOpp;

	public nint pfnGetHyphenInfo;

	public DrawUnderline pfnDrawUnderline;

	public DrawStrikethrough pfnDrawStrikethrough;

	public nint pfnDrawBorder;

	public nint pfnFInterruptUnderline;

	public nint pfnFInterruptShade;

	public nint pfnFInterruptBorder;

	public nint pfnShadeRectangle;

	public DrawTextRun pfnDrawTextRun;

	public nint pfnDrawSplatLine;

	public FInterruptShaping pfnFInterruptShaping;

	public nint pfnGetGlyphs;

	public GetGlyphPositions pfnGetGlyphPositions;

	public DrawGlyphs pfnDrawGlyphs;

	public nint pfnReleaseGlyphBuffers;

	public nint pfnGetGlyphExpansionInfo;

	public nint pfnGetGlyphExpansionInkInfo;

	public nint pfnGetGlyphRunInk;

	public nint pfnGetEms;

	public nint pfnPunctStartLine;

	public nint pfnModWidthOnRun;

	public nint pfnModWidthSpace;

	public nint pfnCompOnRun;

	public nint pfnCompWidthSpace;

	public nint pfnExpOnRun;

	public nint pfnExpWidthSpace;

	public nint pfnGetModWidthClasses;

	public nint pfnGetBreakingClasses;

	public nint pfnFTruncateBefore;

	public nint pfnCanBreakBeforeChar;

	public nint pfnCanBreakAfterChar;

	public nint pfnFHangingPunct;

	public nint pfnGetSnapGrid;

	public nint pfnDrawEffects;

	public nint pfnFCancelHangingPunct;

	public nint pfnModifyCompAtLastChar;

	public GetDurMaxExpandRagged pfnGetDurMaxExpandRagged;

	public GetCharExpansionInfoFullMixed pfnGetCharExpansionInfoFullMixed;

	public GetGlyphExpansionInfoFullMixed pfnGetGlyphExpansionInfoFullMixed;

	public GetCharCompressionInfoFullMixed pfnGetCharCompressionInfoFullMixed;

	public GetGlyphCompressionInfoFullMixed pfnGetGlyphCompressionInfoFullMixed;

	public nint pfnGetCharAlignmentStartLine;

	public nint pfnGetCharAlignmentEndLine;

	public nint pfnGetGlyphAlignmentStartLine;

	public nint pfnGetGlyphAlignmentEndLine;

	public nint pfnGetPriorityForGoodTypography;

	public EnumText pfnEnumText;

	public EnumTab pfnEnumTab;

	public nint pfnEnumPen;

	public GetObjectHandlerInfo pfnGetObjectHandlerInfo;

	public nint pfnAssertFailedPtr;

	public int fDontReleaseRuns;
}
