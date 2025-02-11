namespace MS.Internal.TextFormatting;

internal struct LsQSubInfo
{
	public LsTFlow lstflowSubLine;

	public int lscpFirstSubLine;

	public int lsdcpSubLine;

	public LSPOINT pointUvStartSubLine;

	public LsHeights lsHeightsPresSubLine;

	public int dupSubLine;

	public uint idobj;

	public nint plsrun;

	public int lscpFirstRun;

	public int lsdcpRun;

	public LSPOINT pointUvStartRun;

	public LsHeights lsHeightsPresRun;

	public int dupRun;

	public int dvpPosRun;

	public int dupBorderBefore;

	public int dupBorderAfter;

	public LSPOINT pointUvStartObj;

	public LsHeights lsHeightsPresObj;

	public int dupObj;
}
