namespace MS.Internal.TextFormatting;

internal delegate LsErr DrawUnderline(nint pols, Plsrun plsrun, uint ulType, ref LSPOINT ptOrigin, int ulLength, int ulThickness, LsTFlow textFlow, uint displayMode, ref LSRECT clipRect);
