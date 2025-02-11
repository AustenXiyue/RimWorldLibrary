namespace MS.Internal.TextFormatting;

internal delegate LsErr DrawStrikethrough(nint pols, Plsrun plsrun, uint stType, ref LSPOINT ptOrigin, int stLength, int stThickness, LsTFlow textFlow, uint displayMode, ref LSRECT clipRect);
