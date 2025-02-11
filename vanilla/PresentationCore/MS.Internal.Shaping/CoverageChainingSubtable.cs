namespace MS.Internal.Shaping;

internal struct CoverageChainingSubtable
{
	private const int offsetFormat = 0;

	private const int offsetBacktrackGlyphCount = 2;

	private const int offsetBacktrackCoverageArray = 4;

	private const int sizeGlyphCount = 2;

	private const int sizeCoverageOffset = 2;

	private int offset;

	private int offsetInputGlyphCount;

	private int offsetLookaheadGlyphCount;

	public ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	public ushort BacktrackGlyphCount(FontTable Table)
	{
		return Table.GetUShort(offset + 2);
	}

	public CoverageTable BacktrackCoverage(FontTable Table, ushort Index)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + 2 + 2 + Index * 2));
	}

	public ushort InputGlyphCount(FontTable Table)
	{
		return Table.GetUShort(offset + offsetInputGlyphCount);
	}

	public CoverageTable InputCoverage(FontTable Table, ushort Index)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + offsetInputGlyphCount + 2 + Index * 2));
	}

	public ushort LookaheadGlyphCount(FontTable Table)
	{
		return Table.GetUShort(offset + offsetLookaheadGlyphCount);
	}

	public CoverageTable LookaheadCoverage(FontTable Table, ushort Index)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + offsetLookaheadGlyphCount + 2 + Index * 2));
	}

	public ContextualLookupRecords ContextualLookups(FontTable Table)
	{
		int num = offset + offsetLookaheadGlyphCount + 2 + LookaheadGlyphCount(Table) * 2;
		return new ContextualLookupRecords(num + 2, Table.GetUShort(num));
	}

	public CoverageChainingSubtable(FontTable Table, int Offset)
	{
		offset = Offset;
		offsetInputGlyphCount = 4 + Table.GetUShort(offset + 2) * 2;
		offsetLookaheadGlyphCount = offsetInputGlyphCount + 2 + Table.GetUShort(offset + offsetInputGlyphCount) * 2;
	}

	public unsafe bool Apply(IOpenTypeFont Font, OpenTypeTags TableTag, FontTable Table, LayoutMetrics Metrics, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets, ushort LookupFlags, int FirstGlyph, int AfterLastGlyph, uint Parameter, int nestingLevel, out int NextGlyph)
	{
		Invariant.Assert(Format(Table) == 3);
		NextGlyph = FirstGlyph + 1;
		_ = GlyphInfo.Length;
		ushort num = BacktrackGlyphCount(Table);
		ushort num2 = InputGlyphCount(Table);
		ushort num3 = LookaheadGlyphCount(Table);
		if (FirstGlyph < num || FirstGlyph + num2 > AfterLastGlyph)
		{
			return false;
		}
		bool flag = true;
		int num4 = FirstGlyph;
		ushort num5 = 0;
		while (num5 < num && flag)
		{
			num4 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num4 - 1, LookupFlags, -1);
			if (num4 < 0 || BacktrackCoverage(Table, num5).GetGlyphIndex(Table, GlyphInfo.Glyphs[num4]) < 0)
			{
				flag = false;
			}
			num5++;
		}
		if (!flag)
		{
			return false;
		}
		num4 = FirstGlyph;
		ushort num6 = 0;
		while (num6 < num2 && flag)
		{
			if (num4 >= AfterLastGlyph || InputCoverage(Table, num6).GetGlyphIndex(Table, GlyphInfo.Glyphs[num4]) < 0)
			{
				flag = false;
			}
			else
			{
				num4 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num4 + 1, LookupFlags, 1);
			}
			num6++;
		}
		if (!flag)
		{
			return false;
		}
		int afterLastGlyph = num4;
		ushort num7 = 0;
		while (num7 < num3 && flag)
		{
			if (num4 >= GlyphInfo.Length || LookaheadCoverage(Table, num7).GetGlyphIndex(Table, GlyphInfo.Glyphs[num4]) < 0)
			{
				flag = false;
			}
			else
			{
				num4 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num4 + 1, LookupFlags, 1);
			}
			num7++;
		}
		if (flag)
		{
			ContextualLookups(Table).ApplyContextualLookups(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, LookupFlags, FirstGlyph, afterLastGlyph, Parameter, nestingLevel, out NextGlyph);
		}
		return flag;
	}

	public bool IsLookupCovered(FontTable table, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId)
	{
		ushort num = BacktrackGlyphCount(table);
		ushort num2 = InputGlyphCount(table);
		ushort num3 = LookaheadGlyphCount(table);
		for (ushort num4 = 0; num4 < num; num4++)
		{
			if (!BacktrackCoverage(table, num4).IsAnyGlyphCovered(table, glyphBits, minGlyphId, maxGlyphId))
			{
				return false;
			}
		}
		for (ushort num5 = 0; num5 < num2; num5++)
		{
			if (!InputCoverage(table, num5).IsAnyGlyphCovered(table, glyphBits, minGlyphId, maxGlyphId))
			{
				return false;
			}
		}
		for (ushort num6 = 0; num6 < num3; num6++)
		{
			if (!LookaheadCoverage(table, num6).IsAnyGlyphCovered(table, glyphBits, minGlyphId, maxGlyphId))
			{
				return false;
			}
		}
		return true;
	}

	public CoverageTable GetPrimaryCoverage(FontTable table)
	{
		if (InputGlyphCount(table) > 0)
		{
			return InputCoverage(table, 0);
		}
		return CoverageTable.InvalidCoverage;
	}
}
