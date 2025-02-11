namespace MS.Internal.TextFormatting;

internal unsafe delegate LsErr FetchRunRedefined(nint pols, int lscpFetch, int fIsStyle, nint pstyle, char* pwchTextBuffer, int cchTextBuffer, ref int fIsBufferUsed, out char* pwchText, ref int cchText, ref int fIsHidden, ref LsChp lschp, ref nint lsplsrun);
