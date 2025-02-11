namespace MS.Internal.Shaping;

internal struct GlyphContextSubtable
{
	private class SubRuleSet
	{
		private const int offsetRuleCount = 0;

		private const int offsetRuleArray = 2;

		private const int sizeRuleOffset = 2;

		private int offset;

		public ushort RuleCount(FontTable Table)
		{
			return Table.GetUShort(offset);
		}

		public SubRule Rule(FontTable Table, ushort Index)
		{
			return new SubRule(offset + Table.GetUShort(offset + 2 + Index * 2));
		}

		public SubRuleSet(int Offset)
		{
			offset = Offset;
		}
	}

	private class SubRule
	{
		private const int offsetGlyphCount = 0;

		private const int offsetSubstCount = 2;

		private const int offsetInput = 4;

		private const int sizeCount = 2;

		private const int sizeGlyphId = 2;

		private int offset;

		public ushort GlyphCount(FontTable Table)
		{
			return Table.GetUShort(offset);
		}

		public ushort SubstCount(FontTable Table)
		{
			return Table.GetUShort(offset + 2);
		}

		public ushort GlyphId(FontTable Table, int Index)
		{
			return Table.GetUShort(offset + 4 + (Index - 1) * 2);
		}

		public ContextualLookupRecords ContextualLookups(FontTable Table)
		{
			return new ContextualLookupRecords(offset + 4 + (GlyphCount(Table) - 1) * 2, SubstCount(Table));
		}

		public unsafe bool Apply(IOpenTypeFont Font, OpenTypeTags TableTag, FontTable Table, LayoutMetrics Metrics, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets, ushort LookupFlags, int FirstGlyph, int AfterLastGlyph, uint Parameter, int nestingLevel, out int NextGlyph)
		{
			bool flag = true;
			NextGlyph = FirstGlyph + 1;
			int num = GlyphCount(Table);
			int num2 = FirstGlyph;
			ushort num3 = 1;
			while (num3 < num && flag)
			{
				num2 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num2 + 1, LookupFlags, 1);
				flag = num2 < AfterLastGlyph && GlyphId(Table, num3) == GlyphInfo.Glyphs[num2];
				num3++;
			}
			if (flag)
			{
				ContextualLookups(Table).ApplyContextualLookups(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, LookupFlags, FirstGlyph, num2 + 1, Parameter, nestingLevel, out NextGlyph);
			}
			return flag;
		}

		public SubRule(int Offset)
		{
			offset = Offset;
		}
	}

	private const int offsetFormat = 0;

	private const int offsetCoverage = 2;

	private const int offsetSubRuleSetCount = 4;

	private const int offsetSubRuleSetArray = 6;

	private const int sizeRuleSetOffset = 2;

	private int offset;

	public ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private CoverageTable Coverage(FontTable Table)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + 2));
	}

	private SubRuleSet RuleSet(FontTable Table, int Index)
	{
		return new SubRuleSet(offset + Table.GetUShort(offset + 6 + Index * 2));
	}

	public unsafe bool Apply(IOpenTypeFont Font, OpenTypeTags TableTag, FontTable Table, LayoutMetrics Metrics, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets, ushort LookupFlags, int FirstGlyph, int AfterLastGlyph, uint Parameter, int nestingLevel, out int NextGlyph)
	{
		Invariant.Assert(Format(Table) == 1);
		NextGlyph = FirstGlyph + 1;
		_ = GlyphInfo.Length;
		ushort glyph = GlyphInfo.Glyphs[FirstGlyph];
		int glyphIndex = Coverage(Table).GetGlyphIndex(Table, glyph);
		if (glyphIndex < 0)
		{
			return false;
		}
		SubRuleSet subRuleSet = RuleSet(Table, glyphIndex);
		ushort num = subRuleSet.RuleCount(Table);
		bool flag = false;
		ushort num2 = 0;
		while (!flag && num2 < num)
		{
			flag = subRuleSet.Rule(Table, num2).Apply(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, LookupFlags, FirstGlyph, AfterLastGlyph, Parameter, nestingLevel, out NextGlyph);
			num2++;
		}
		return flag;
	}

	public bool IsLookupCovered(FontTable table, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId)
	{
		return true;
	}

	public CoverageTable GetPrimaryCoverage(FontTable table)
	{
		return Coverage(table);
	}

	public GlyphContextSubtable(int Offset)
	{
		offset = Offset;
	}
}
