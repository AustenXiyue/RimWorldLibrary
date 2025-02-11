namespace MS.Internal.TextFormatting;

internal unsafe delegate LsErr GetGlyphCompressionInfoFullMixed(nint pols, LsDevice device, LsTFlow textFlow, LsGlyphRunInfo* plsglyphrunInfo, LsNeighborInfo* plsneighborInfoLeft, LsNeighborInfo* plsneighborInfoRight, int maxPriorityLevel, int** pplscompressionLeft, int** pplscompressionRight);
