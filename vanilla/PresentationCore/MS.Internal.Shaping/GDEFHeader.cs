namespace MS.Internal.Shaping;

internal struct GDEFHeader
{
	private const int offsetGlyphClassDef = 4;

	private const int offsetGlyphAttachList = 6;

	private const int offsetLigaCaretList = 8;

	private const int offsetMarkAttachClassDef = 10;

	private int offset;

	public ClassDefTable GetGlyphClassDef(FontTable Table)
	{
		Invariant.Assert(Table.IsPresent);
		return new ClassDefTable(offset + Table.GetOffset(offset + 4));
	}

	public ClassDefTable GetMarkAttachClassDef(FontTable Table)
	{
		Invariant.Assert(Table.IsPresent);
		return new ClassDefTable(offset + Table.GetOffset(offset + 10));
	}

	public GDEFHeader(int Offset)
	{
		offset = Offset;
	}
}
