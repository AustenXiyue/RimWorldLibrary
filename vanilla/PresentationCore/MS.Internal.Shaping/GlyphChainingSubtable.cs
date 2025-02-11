namespace MS.Internal.Shaping;

internal struct GlyphChainingSubtable
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
		private const int sizeCount = 2;

		private const int sizeGlyphId = 2;

		private int offset;

		public static ushort GlyphCount(FontTable Table, int Offset)
		{
			return Table.GetUShort(Offset);
		}

		public static ushort GlyphId(FontTable Table, int Offset)
		{
			return Table.GetUShort(Offset);
		}

		public ContextualLookupRecords ContextualLookups(FontTable Table, int CurrentOffset)
		{
			return new ContextualLookupRecords(CurrentOffset + 2, Table.GetUShort(CurrentOffset));
		}

		public unsafe bool Apply(IOpenTypeFont Font, OpenTypeTags TableTag, FontTable Table, LayoutMetrics Metrics, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets, ushort LookupFlags, int FirstGlyph, int AfterLastGlyph, uint Parameter, int nestingLevel, out int NextGlyph)
		{
			bool flag = true;
			NextGlyph = FirstGlyph + 1;
			int num = offset;
			int num2 = GlyphCount(Table, num);
			num += 2;
			int num3 = FirstGlyph;
			ushort num4 = 0;
			while (num4 < num2 && flag)
			{
				num3 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num3 - 1, LookupFlags, -1);
				if (num3 < 0)
				{
					flag = false;
				}
				else
				{
					flag = GlyphId(Table, num) == GlyphInfo.Glyphs[num3];
					num += 2;
				}
				num4++;
			}
			if (!flag)
			{
				return false;
			}
			int num5 = GlyphCount(Table, num);
			num += 2;
			num3 = FirstGlyph;
			ushort num6 = 1;
			while (num6 < num5 && flag)
			{
				num3 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num3 + 1, LookupFlags, 1);
				if (num3 >= AfterLastGlyph)
				{
					flag = false;
				}
				else
				{
					flag = GlyphId(Table, num) == GlyphInfo.Glyphs[num3];
					num += 2;
				}
				num6++;
			}
			if (!flag)
			{
				return false;
			}
			int afterLastGlyph = num3 + 1;
			int num7 = GlyphCount(Table, num);
			num += 2;
			ushort num8 = 0;
			while (num8 < num7 && flag)
			{
				num3 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num3 + 1, LookupFlags, 1);
				if (num3 >= GlyphInfo.Length)
				{
					flag = false;
				}
				else
				{
					flag = GlyphId(Table, num) == GlyphInfo.Glyphs[num3];
					num += 2;
				}
				num8++;
			}
			if (flag)
			{
				ContextualLookups(Table, num).ApplyContextualLookups(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, LookupFlags, FirstGlyph, afterLastGlyph, Parameter, nestingLevel, out NextGlyph);
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

	public GlyphChainingSubtable(int Offset)
	{
		offset = Offset;
	}
}
