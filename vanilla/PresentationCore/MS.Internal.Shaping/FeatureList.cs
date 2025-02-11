namespace MS.Internal.Shaping;

internal struct FeatureList
{
	private const int offsetFeatureCount = 0;

	private const int offsetFeatureRecordArray = 2;

	private const int sizeFeatureRecord = 6;

	private const int offsetFeatureRecordTag = 0;

	private const int offsetFeatureRecordOffset = 4;

	private int offset;

	public ushort FeatureCount(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	public uint FeatureTag(FontTable Table, ushort Index)
	{
		return Table.GetUInt(offset + 2 + Index * 6);
	}

	public FeatureTable FeatureTable(FontTable Table, ushort Index)
	{
		return new FeatureTable(offset + Table.GetUShort(offset + 2 + Index * 6 + 4));
	}

	public FeatureList(int Offset)
	{
		offset = Offset;
	}
}
