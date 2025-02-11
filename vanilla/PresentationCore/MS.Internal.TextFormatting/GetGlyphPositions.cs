using MS.Internal.Text.TextInterface;

namespace MS.Internal.TextFormatting;

internal unsafe delegate LsErr GetGlyphPositions(nint pols, nint* plsplsruns, int* pcchPlsrun, int plsrunCount, LsDevice device, char* pwchText, ushort* puClusterMap, ushort* puCharProperties, int cchText, ushort* puGlyphs, uint* piGlyphProperties, int glyphCount, LsTFlow textFlow, int* piGlyphAdvances, GlyphOffset* piiGlyphOffsets);
