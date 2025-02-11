namespace MS.Internal.Shaping;

internal struct ExtensionLookupTable
{
	private const int offsetFormat = 0;

	private const int offsetLookupType = 2;

	private const int offsetExtensionOffset = 4;

	private int offset;

	internal ushort LookupType(FontTable Table)
	{
		return Table.GetUShort(offset + 2);
	}

	internal int LookupSubtableOffset(FontTable Table)
	{
		return offset + (int)Table.GetUInt(offset + 4);
	}

	public ExtensionLookupTable(int Offset)
	{
		offset = Offset;
	}
}
