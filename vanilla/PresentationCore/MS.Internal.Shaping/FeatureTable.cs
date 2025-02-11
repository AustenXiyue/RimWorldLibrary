namespace MS.Internal.Shaping;

internal struct FeatureTable
{
	private const int offsetLookupCount = 2;

	private const int offsetLookupIndexArray = 4;

	private const int sizeLookupIndex = 2;

	private int offset;

	public bool IsNull => offset == int.MaxValue;

	public ushort LookupCount(FontTable Table)
	{
		return Table.GetUShort(offset + 2);
	}

	public ushort LookupIndex(FontTable Table, ushort Index)
	{
		return Table.GetUShort(offset + 4 + Index * 2);
	}

	public FeatureTable(int Offset)
	{
		offset = Offset;
	}
}
