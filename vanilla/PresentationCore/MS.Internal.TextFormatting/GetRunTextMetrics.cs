namespace MS.Internal.TextFormatting;

internal delegate LsErr GetRunTextMetrics(nint pols, Plsrun plsrun, LsDevice lsDevice, LsTFlow lstFlow, ref LsTxM lstTextMetrics);
