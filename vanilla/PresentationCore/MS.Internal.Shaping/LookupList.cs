namespace MS.Internal.Shaping;

internal struct LookupList
{
	private const int offsetLookupCount = 0;

	private const int LookupOffsetArray = 2;

	private const int sizeLookupOffset = 2;

	private int offset;

	public ushort LookupCount(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	public LookupTable Lookup(FontTable Table, ushort Index)
	{
		return new LookupTable(Table, offset + Table.GetUShort(offset + 2 + Index * 2));
	}

	public LookupList(int Offset)
	{
		offset = Offset;
	}
}
