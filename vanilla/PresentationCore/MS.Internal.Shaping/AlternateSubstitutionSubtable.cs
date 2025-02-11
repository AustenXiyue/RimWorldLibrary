namespace MS.Internal.Shaping;

internal struct AlternateSubstitutionSubtable
{
	private struct AlternateSetTable
	{
		private const int offsetGlyphCount = 0;

		private const int offsetGlyphs = 2;

		private const int sizeGlyph = 2;

		private int offset;

		public ushort GlyphCount(FontTable Table)
		{
			return Table.GetUShort(offset);
		}

		public ushort Alternate(FontTable Table, uint FeatureParam)
		{
			Invariant.Assert(FeatureParam != 0);
			uint num = FeatureParam - 1;
			if (num >= GlyphCount(Table))
			{
				return ushort.MaxValue;
			}
			return Table.GetUShort(offset + 2 + (ushort)num * 2);
		}

		public AlternateSetTable(int Offset)
		{
			offset = Offset;
		}
	}

	private const int offsetFormat = 0;

	private const int offsetCoverage = 2;

	private const int offsetAlternateSetCount = 4;

	private const int offsetAlternateSets = 6;

	private const int sizeAlternateSetOffset = 2;

	private const ushort InvalidAlternateGlyph = ushort.MaxValue;

	private int offset;

	public ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private CoverageTable Coverage(FontTable Table)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + 2));
	}

	private AlternateSetTable AlternateSet(FontTable Table, int index)
	{
		return new AlternateSetTable(offset + Table.GetUShort(offset + 6 + index * 2));
	}

	public bool Apply(FontTable Table, GlyphInfoList GlyphInfo, uint FeatureParam, int FirstGlyph, out int NextGlyph)
	{
		NextGlyph = FirstGlyph + 1;
		if (Format(Table) != 1)
		{
			return false;
		}
		_ = GlyphInfo.Length;
		int glyphIndex = Coverage(Table).GetGlyphIndex(Table, GlyphInfo.Glyphs[FirstGlyph]);
		if (glyphIndex == -1)
		{
			return false;
		}
		ushort num = AlternateSet(Table, glyphIndex).Alternate(Table, FeatureParam);
		if (num != ushort.MaxValue)
		{
			GlyphInfo.Glyphs[FirstGlyph] = num;
			GlyphInfo.GlyphFlags[FirstGlyph] = 23;
			return true;
		}
		return false;
	}

	public bool IsLookupCovered(FontTable table, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId)
	{
		return Coverage(table).IsAnyGlyphCovered(table, glyphBits, minGlyphId, maxGlyphId);
	}

	public CoverageTable GetPrimaryCoverage(FontTable table)
	{
		return Coverage(table);
	}

	public AlternateSubstitutionSubtable(int Offset)
	{
		offset = Offset;
	}
}
