namespace MS.Internal.TextFormatting;

internal delegate LsErr FetchLineProps(nint pols, int lscpFetch, int firstLineInPara, ref LsLineProps lsLineProps);
