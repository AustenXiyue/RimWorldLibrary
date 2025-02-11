namespace MS.Internal.TextFormatting;

internal delegate LsErr InlineDraw(nint pols, Plsrun plsrun, ref LSPOINT runOrigin, LsTFlow textFlow, int runWidth);
