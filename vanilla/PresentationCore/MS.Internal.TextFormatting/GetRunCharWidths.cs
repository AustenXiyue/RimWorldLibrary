namespace MS.Internal.TextFormatting;

internal unsafe delegate LsErr GetRunCharWidths(nint pols, Plsrun plsrun, LsDevice device, char* runText, int cchRun, int maxWidth, LsTFlow textFlow, int* charWidths, ref int totalWidth, ref int cchProcessed);
