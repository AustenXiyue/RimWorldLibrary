namespace MS.Internal.TextFormatting;

internal unsafe delegate LsErr GetGlyphsRedefined(nint pols, nint* plsplsruns, int* pcchPlsrun, int plsrunCount, char* pwchText, int cchText, LsTFlow textFlow, ushort* puGlyphsBuffer, uint* piGlyphPropsBuffer, int cgiGlyphBuffers, ref int fIsGlyphBuffersUsed, ushort* puClusterMap, ushort* puCharProperties, int* pfCanGlyphAlone, ref int glyphCount);
