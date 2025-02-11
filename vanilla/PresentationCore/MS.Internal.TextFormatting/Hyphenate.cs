namespace MS.Internal.TextFormatting;

internal delegate LsErr Hyphenate(nint pols, int fLastHyphenationFound, int lscpLastHyphenation, ref LsHyph lastHyphenation, int lscpBeginWord, int lscpExceed, ref int fHyphenFound, ref int lscpHyphen, ref LsHyph plsHyph);
