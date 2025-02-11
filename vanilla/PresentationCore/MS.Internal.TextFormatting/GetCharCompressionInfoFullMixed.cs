namespace MS.Internal.TextFormatting;

internal unsafe delegate LsErr GetCharCompressionInfoFullMixed(nint pols, LsDevice device, LsTFlow textFlow, LsCharRunInfo* plscharrunInfo, LsNeighborInfo* plsneighborInfoLeft, LsNeighborInfo* plsneighborInfoRight, int maxPriorityLevel, int** pplscompressionLeft, int** pplscompressionRight);
