using MS.Internal.Text.TextInterface;

namespace MS.Internal.TextFormatting;

internal unsafe delegate LsErr EnumText(nint pols, Plsrun plsrun, int cpFirst, int dcp, char* pwchText, int cchText, LsTFlow lstFlow, int fReverseOrder, int fGeometryProvided, ref LSPOINT pptStart, ref LsHeights pheights, int dupRun, int glyphBaseRun, int* charWidths, ushort* pClusterMap, ushort* characterProperties, ushort* puglyphs, int* pGlyphAdvances, GlyphOffset* pGlyphOffsets, uint* pGlyphProperties, int glyphCount);
