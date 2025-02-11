namespace MS.Internal.Shaping;

internal struct MultipleSubstitutionSubtable
{
	private const int offsetFormat = 0;

	private const int offsetCoverage = 2;

	private const int offsetSequenceCount = 4;

	private const int offsetSequenceArray = 6;

	private const int sizeSequenceOffset = 2;

	private int offset;

	private ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private CoverageTable Coverage(FontTable Table)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + 2));
	}

	private MultipleSubstitutionSequenceTable Sequence(FontTable Table, int Index)
	{
		return new MultipleSubstitutionSequenceTable(offset + Table.GetUShort(offset + 6 + Index * 2));
	}

	public bool Apply(IOpenTypeFont Font, FontTable Table, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, ushort LookupFlags, int FirstGlyph, int AfterLastGlyph, out int NextGlyph)
	{
		NextGlyph = FirstGlyph + 1;
		if (Format(Table) != 1)
		{
			return false;
		}
		_ = GlyphInfo.Length;
		ushort glyph = GlyphInfo.Glyphs[FirstGlyph];
		int glyphIndex = Coverage(Table).GetGlyphIndex(Table, glyph);
		if (glyphIndex == -1)
		{
			return false;
		}
		MultipleSubstitutionSequenceTable multipleSubstitutionSequenceTable = Sequence(Table, glyphIndex);
		ushort num = multipleSubstitutionSequenceTable.GlyphCount(Table);
		int num2 = num - 1;
		if (num == 0)
		{
			GlyphInfo.Remove(FirstGlyph, 1);
		}
		else
		{
			ushort value = GlyphInfo.FirstChars[FirstGlyph];
			ushort value2 = GlyphInfo.LigatureCounts[FirstGlyph];
			if (num2 > 0)
			{
				GlyphInfo.Insert(FirstGlyph, num2);
			}
			for (ushort num3 = 0; num3 < num; num3++)
			{
				GlyphInfo.Glyphs[FirstGlyph + num3] = multipleSubstitutionSequenceTable.Glyph(Table, num3);
				GlyphInfo.GlyphFlags[FirstGlyph + num3] = 23;
				GlyphInfo.FirstChars[FirstGlyph + num3] = value;
				GlyphInfo.LigatureCounts[FirstGlyph + num3] = value2;
			}
		}
		for (int i = 0; i < CharCount; i++)
		{
			if (Charmap[i] > FirstGlyph)
			{
				Charmap[i] = (ushort)(Charmap[i] + num2);
			}
		}
		NextGlyph = FirstGlyph + num2 + 1;
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

	public MultipleSubstitutionSubtable(int Offset)
	{
		offset = Offset;
	}
}
