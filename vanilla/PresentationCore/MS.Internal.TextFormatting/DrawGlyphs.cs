using MS.Internal.Text.TextInterface;

namespace MS.Internal.TextFormatting;

internal unsafe delegate LsErr DrawGlyphs(nint pols, Plsrun plsrun, char* pwchText, ushort* puClusterMap, ushort* puCharProperties, int cchText, ushort* puGlyphs, int* piJustifiedGlyphAdvances, int* puGlyphAdvances, GlyphOffset* piiGlyphOffsets, uint* piGlyphProperties, LsExpType* plsExpType, int glyphCount, LsTFlow textFlow, uint displayMode, ref LSPOINT origin, ref LsHeights lsHeights, int runWidth, ref LSRECT clippingRect);
