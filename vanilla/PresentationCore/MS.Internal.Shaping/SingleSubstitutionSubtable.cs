namespace MS.Internal.Shaping;

internal struct SingleSubstitutionSubtable
{
	private const int offsetFormat = 0;

	private const int offsetCoverage = 2;

	private const int offsetFormat1DeltaGlyphId = 4;

	private const int offsetFormat2GlyphCount = 4;

	private const int offsetFormat2SubstitutehArray = 6;

	private const int sizeFormat2SubstituteSize = 2;

	private int offset;

	private ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private CoverageTable Coverage(FontTable Table)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + 2));
	}

	private short Format1DeltaGlyphId(FontTable Table)
	{
		Invariant.Assert(Format(Table) == 1);
		return Table.GetShort(offset + 4);
	}

	private ushort Format2SubstituteGlyphId(FontTable Table, ushort Index)
	{
		Invariant.Assert(Format(Table) == 2);
		return Table.GetUShort(offset + 6 + Index * 2);
	}

	public bool Apply(FontTable Table, GlyphInfoList GlyphInfo, int FirstGlyph, out int NextGlyph)
	{
		Invariant.Assert(FirstGlyph >= 0);
		NextGlyph = FirstGlyph + 1;
		ushort num = GlyphInfo.Glyphs[FirstGlyph];
		int glyphIndex = Coverage(Table).GetGlyphIndex(Table, num);
		if (glyphIndex == -1)
		{
			return false;
		}
		switch (Format(Table))
		{
		case 1:
			GlyphInfo.Glyphs[FirstGlyph] = (ushort)(num + Format1DeltaGlyphId(Table));
			GlyphInfo.GlyphFlags[FirstGlyph] = 23;
			NextGlyph = FirstGlyph + 1;
			return true;
		case 2:
			GlyphInfo.Glyphs[FirstGlyph] = Format2SubstituteGlyphId(Table, (ushort)glyphIndex);
			GlyphInfo.GlyphFlags[FirstGlyph] = 23;
			NextGlyph = FirstGlyph + 1;
			return true;
		default:
			NextGlyph = FirstGlyph + 1;
			return false;
		}
	}

	public bool IsLookupCovered(FontTable table, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId)
	{
		return Coverage(table).IsAnyGlyphCovered(table, glyphBits, minGlyphId, maxGlyphId);
	}

	public CoverageTable GetPrimaryCoverage(FontTable table)
	{
		return Coverage(table);
	}

	public SingleSubstitutionSubtable(int Offset)
	{
		offset = Offset;
	}
}
