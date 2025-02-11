namespace MS.Internal.TextFormatting;

internal unsafe delegate LsErr DrawTextRun(nint pols, Plsrun plsrun, ref LSPOINT ptText, char* runText, int* charWidths, int cchText, LsTFlow textFlow, uint displayMode, ref LSPOINT ptRun, ref LsHeights lsHeights, int dupRun, ref LSRECT clipRect);
