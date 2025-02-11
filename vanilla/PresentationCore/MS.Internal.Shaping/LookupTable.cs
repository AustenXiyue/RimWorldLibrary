namespace MS.Internal.Shaping;

internal struct LookupTable
{
	private const int offsetLookupType = 0;

	private const int offsetLookupFlags = 2;

	private const int offsetSubtableCount = 4;

	private const int offsetSubtableArray = 6;

	private const int sizeSubtableOffset = 2;

	private int offset;

	private ushort lookupType;

	private ushort lookupFlags;

	private ushort subtableCount;

	public ushort LookupType()
	{
		return lookupType;
	}

	public ushort LookupFlags()
	{
		return lookupFlags;
	}

	public ushort SubTableCount()
	{
		return subtableCount;
	}

	public int SubtableOffset(FontTable Table, ushort Index)
	{
		return offset + Table.GetOffset(offset + 6 + Index * 2);
	}

	public LookupTable(FontTable table, int Offset)
	{
		offset = Offset;
		lookupType = table.GetUShort(offset);
		lookupFlags = table.GetUShort(offset + 2);
		subtableCount = table.GetUShort(offset + 4);
	}
}
