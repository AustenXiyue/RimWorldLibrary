namespace MS.Internal.Shaping;

internal struct MultipleSubstitutionSequenceTable
{
	private const int offsetGlyphCount = 0;

	private const int offsetGlyphArray = 2;

	private const int sizeGlyphId = 2;

	private int offset;

	public ushort GlyphCount(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	public ushort Glyph(FontTable Table, ushort index)
	{
		return Table.GetUShort(offset + 2 + index * 2);
	}

	public MultipleSubstitutionSequenceTable(int Offset)
	{
		offset = Offset;
	}
}
