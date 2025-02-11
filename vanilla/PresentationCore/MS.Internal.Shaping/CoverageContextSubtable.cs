namespace MS.Internal.Shaping;

internal struct CoverageContextSubtable
{
	private const int offsetFormat = 0;

	private const int offsetGlyphCount = 2;

	private const int offsetSubstCount = 4;

	private const int offsetInputCoverage = 6;

	private const int sizeOffset = 2;

	private int offset;

	private ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private ushort GlyphCount(FontTable Table)
	{
		return Table.GetUShort(offset + 2);
	}

	private ushort SubstCount(FontTable Table)
	{
		return Table.GetUShort(offset + 4);
	}

	private CoverageTable InputCoverage(FontTable Table, ushort index)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + 6 + index * 2));
	}

	public ContextualLookupRecords ContextualLookups(FontTable Table)
	{
		return new ContextualLookupRecords(offset + 6 + GlyphCount(Table) * 2, SubstCount(Table));
	}

	public unsafe bool Apply(IOpenTypeFont Font, OpenTypeTags TableTag, FontTable Table, LayoutMetrics Metrics, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets, ushort LookupFlags, int FirstGlyph, int AfterLastGlyph, uint Parameter, int nestingLevel, out int NextGlyph)
	{
		Invariant.Assert(Format(Table) == 3);
		NextGlyph = FirstGlyph + 1;
		bool flag = true;
		int num = GlyphCount(Table);
		int num2 = FirstGlyph;
		ushort num3 = 0;
		while (num3 < num && flag)
		{
			if (num2 >= AfterLastGlyph || InputCoverage(Table, num3).GetGlyphIndex(Table, GlyphInfo.Glyphs[num2]) < 0)
			{
				flag = false;
			}
			else
			{
				num2 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num2 + 1, LookupFlags, 1);
			}
			num3++;
		}
		if (flag)
		{
			ContextualLookups(Table).ApplyContextualLookups(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, LookupFlags, FirstGlyph, num2, Parameter, nestingLevel, out NextGlyph);
		}
		return flag;
	}

	public bool IsLookupCovered(FontTable table, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId)
	{
		return true;
	}

	public CoverageTable GetPrimaryCoverage(FontTable table)
	{
		if (GlyphCount(table) > 0)
		{
			return InputCoverage(table, 0);
		}
		return CoverageTable.InvalidCoverage;
	}

	public CoverageContextSubtable(int Offset)
	{
		offset = Offset;
	}
}
