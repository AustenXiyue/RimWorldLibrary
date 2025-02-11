namespace MS.Internal.TextFormatting;

internal delegate LsErr GetRunUnderlineInfo(nint pols, Plsrun plsrun, ref LsHeights lsHeights, LsTFlow textFlow, ref LsULInfo ulInfo);
