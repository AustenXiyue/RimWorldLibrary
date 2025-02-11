namespace MS.Internal.TextFormatting;

internal delegate LsErr GetRunStrikethroughInfo(nint pols, Plsrun plsrun, ref LsHeights lsHeights, LsTFlow textFlow, ref LsStInfo stInfo);
