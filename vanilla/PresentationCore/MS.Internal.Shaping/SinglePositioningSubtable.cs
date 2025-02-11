namespace MS.Internal.Shaping;

internal struct SinglePositioningSubtable
{
	private const int offsetFormat = 0;

	private const int offsetCoverage = 2;

	private const int offsetValueFormat = 4;

	private const int offsetFormat1Value = 6;

	private const int offsetFormat2ValueCount = 6;

	private const int offsetFormat2ValueArray = 8;

	private int offset;

	private ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private CoverageTable Coverage(FontTable Table)
	{
		return new CoverageTable(offset + Table.GetOffset(offset + 2));
	}

	private ushort ValueFormat(FontTable Table)
	{
		return Table.GetUShort(offset + 4);
	}

	private ValueRecordTable Format1ValueRecord(FontTable Table)
	{
		Invariant.Assert(Format(Table) == 1);
		return new ValueRecordTable(offset + 6, offset, ValueFormat(Table));
	}

	private ValueRecordTable Format2ValueRecord(FontTable Table, ushort Index)
	{
		Invariant.Assert(Format(Table) == 2);
		return new ValueRecordTable(offset + 8 + Index * ValueRecordTable.Size(ValueFormat(Table)), offset, ValueFormat(Table));
	}

	public unsafe bool Apply(FontTable Table, LayoutMetrics Metrics, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets, int FirstGlyph, int AfterLastGlyph, out int NextGlyph)
	{
		Invariant.Assert(FirstGlyph >= 0);
		Invariant.Assert(AfterLastGlyph <= GlyphInfo.Length);
		NextGlyph = FirstGlyph + 1;
		_ = GlyphInfo.Length;
		ushort glyph = GlyphInfo.Glyphs[FirstGlyph];
		int glyphIndex = Coverage(Table).GetGlyphIndex(Table, glyph);
		if (glyphIndex == -1)
		{
			return false;
		}
		ValueRecordTable valueRecordTable;
		switch (Format(Table))
		{
		case 1:
			valueRecordTable = Format1ValueRecord(Table);
			break;
		case 2:
			valueRecordTable = Format2ValueRecord(Table, (ushort)glyphIndex);
			break;
		default:
			return false;
		}
		valueRecordTable.AdjustPos(Table, Metrics, ref Offsets[FirstGlyph], ref Advances[FirstGlyph]);
		return true;
	}

	public bool IsLookupCovered(FontTable table, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId)
	{
		return Coverage(table).IsAnyGlyphCovered(table, glyphBits, minGlyphId, maxGlyphId);
	}

	public CoverageTable GetPrimaryCoverage(FontTable table)
	{
		return Coverage(table);
	}

	public SinglePositioningSubtable(int Offset)
	{
		offset = Offset;
	}
}
