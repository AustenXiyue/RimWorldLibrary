namespace MS.Internal.Shaping;

internal struct LigatureAttachTable
{
	private const int offsetAnchorArray = 2;

	private const int sizeAnchorOffset = 2;

	private int offset;

	private int classCount;

	public AnchorTable LigatureAnchor(FontTable Table, ushort Component, ushort MarkClass)
	{
		int uShort = Table.GetUShort(offset + 2 + (Component * classCount + MarkClass) * 2);
		if (uShort == 0)
		{
			return new AnchorTable(Table, 0);
		}
		return new AnchorTable(Table, offset + uShort);
	}

	public LigatureAttachTable(int Offset, ushort ClassCount)
	{
		offset = Offset;
		classCount = ClassCount;
	}
}
