namespace MS.Internal.TextFormatting;

internal unsafe delegate LsErr GetGlyphExpansionInfoFullMixed(nint pols, LsDevice device, LsTFlow textFlow, LsGlyphRunInfo* plsglyphrunInfo, LsNeighborInfo* plsneighborInfoLeft, LsNeighborInfo* plsneighborInfoRight, int maxPriorityLevel, int** pplsexpansionLeft, int** pplsexpansionRight, LsExpType* plsexptype, int* pduMinInk);
