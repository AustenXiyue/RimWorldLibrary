namespace MS.Internal.TextFormatting;

internal delegate LsErr GetPrevHyphenOpp(nint pols, int lscpStartSearch, int lsdcpSearch, ref int fHyphenFound, ref int lscpHyphen, ref LsHyph lsHyph);
