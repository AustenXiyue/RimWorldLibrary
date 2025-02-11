namespace MS.Internal.TextFormatting;

internal delegate LsErr GetAutoNumberInfo(nint pols, ref LsKAlign alignment, ref LsChp lschp, ref nint lsplsrun, ref ushort addedChar, ref LsChp lschpAddedChar, ref nint lsplsrunAddedChar, ref int fWord95Model, ref int offset, ref int width);
