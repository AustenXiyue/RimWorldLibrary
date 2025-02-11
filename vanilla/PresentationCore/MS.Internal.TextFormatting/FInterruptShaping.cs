namespace MS.Internal.TextFormatting;

internal delegate LsErr FInterruptShaping(nint pols, LsTFlow textFlow, Plsrun firstPlsrun, Plsrun secondPlsrun, ref int fIsInterruptOk);
