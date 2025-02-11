namespace MS.Internal.Shaping;

internal struct GSUBHeader
{
	private const int offsetScriptList = 4;

	private const int offsetFeatureList = 6;

	private const int offsetLookupList = 8;

	private int offset;

	public ScriptList GetScriptList(FontTable Table)
	{
		return new ScriptList(offset + Table.GetOffset(offset + 4));
	}

	public FeatureList GetFeatureList(FontTable Table)
	{
		return new FeatureList(offset + Table.GetOffset(offset + 6));
	}

	public LookupList GetLookupList(FontTable Table)
	{
		return new LookupList(offset + Table.GetOffset(offset + 8));
	}

	public GSUBHeader(int Offset)
	{
		offset = Offset;
	}
}
