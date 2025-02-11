namespace MS.Internal.TextFormatting;

internal struct LsBreaks
{
	public int cBreaks;

	public unsafe LsLInfo* plslinfoArray;

	public unsafe nint* plinepenaltyArray;

	public unsafe nint* pplolineArray;
}
