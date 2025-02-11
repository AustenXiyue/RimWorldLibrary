namespace MS.Internal.TextFormatting;

internal struct LsCharRunInfo
{
	public Plsrun plsrun;

	public unsafe char* pwch;

	public unsafe int* rgduNominalWidth;

	public unsafe int* rgduChangeLeft;

	public unsafe int* rgduChangeRight;

	public int cwch;
}
