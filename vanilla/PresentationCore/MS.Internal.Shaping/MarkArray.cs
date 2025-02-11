namespace MS.Internal.Shaping;

internal struct MarkArray
{
	private const int offsetClassArray = 2;

	private const int sizeClassRecord = 4;

	private const int offsetClassRecordClass = 0;

	private const int offsetClassRecordAnchor = 2;

	private int offset;

	public ushort Class(FontTable Table, ushort Index)
	{
		return Table.GetUShort(offset + 2 + Index * 4);
	}

	public AnchorTable MarkAnchor(FontTable Table, ushort Index)
	{
		int uShort = Table.GetUShort(offset + 2 + Index * 4 + 2);
		if (uShort == 0)
		{
			return new AnchorTable(Table, 0);
		}
		return new AnchorTable(Table, offset + uShort);
	}

	public MarkArray(int Offset)
	{
		offset = Offset;
	}
}
