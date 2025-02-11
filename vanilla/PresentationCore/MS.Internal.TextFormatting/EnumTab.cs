namespace MS.Internal.TextFormatting;

internal unsafe delegate LsErr EnumTab(nint pols, Plsrun plsrun, int cpFirst, char* pwchText, char tabLeader, LsTFlow lstFlow, int fReverseOrder, int fGeometryProvided, ref LSPOINT pptStart, ref LsHeights heights, int dupRun);
