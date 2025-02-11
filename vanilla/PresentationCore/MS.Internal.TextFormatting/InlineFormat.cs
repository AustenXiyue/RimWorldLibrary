namespace MS.Internal.TextFormatting;

internal delegate LsErr InlineFormat(nint pols, Plsrun plsrun, int lscpInline, int currentPosition, int rightMargin, ref ObjDim pobjDim, out int fFirstRealOnLine, out int fPenPositionUsed, out LsBrkCond breakBefore, out LsBrkCond breakAfter);
