namespace MS.Internal.TextFormatting;

internal unsafe delegate LsErr GetCharExpansionInfoFullMixed(nint pols, LsDevice device, LsTFlow textFlow, LsCharRunInfo* plscharrunInfo, LsNeighborInfo* plsneighborInfoLeft, LsNeighborInfo* plsneighborInfoRight, int maxPriorityLevel, int** pplsexpansionLeft, int** pplsexpansionRight);
