namespace MS.Internal.Shaping;

internal struct ClassContextSubtable
{
	private class SubClassSet
	{
		private const int offsetRuleCount = 0;

		private const int offsetRuleArray = 2;

		private const int sizeRuleOffset = 2;

		private int offset;

		public bool IsNull => offset == int.MaxValue;

		public ushort RuleCount(FontTable Table)
		{
			return Table.GetUShort(offset);
		}

		public SubClassRule Rule(FontTable Table, ushort Index)
		{
			return new SubClassRule(offset + Table.GetUShort(offset + 2 + Index * 2));
		}

		public SubClassSet(int Offset)
		{
			offset = Offset;
		}
	}

	private class SubClassRule
	{
		private const int offsetGlyphCount = 0;

		private const int offsetSubstCount = 2;

		private const int offsetInputSequence = 4;

		private const int sizeCount = 2;

		private const int sizeClassId = 2;

		private int offset;

		public ushort GlyphCount(FontTable Table)
		{
			return Table.GetUShort(offset);
		}

		public ushort ClassId(FontTable Table, int Index)
		{
			return Table.GetUShort(offset + 4 + (Index - 1) * 2);
		}

		public ushort SubstCount(FontTable Table)
		{
			return Table.GetUShort(offset + 2);
		}

		public ContextualLookupRecords ContextualLookups(FontTable Table)
		{
			return new ContextualLookupRecords(offset + 4 + (GlyphCount(Table) - 1) * 2, SubstCount(Table));
		}

		public unsafe bool Apply(IOpenTypeFont Font, OpenTypeTags TableTag, FontTable Table, LayoutMetrics Metrics, ClassDefTable ClassDef, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets, ushort LookupFlags, int FirstGlyph, int AfterLastGlyph, uint Parameter, int nestingLevel, out int NextGlyph)
		{
			NextGlyph = FirstGlyph + 1;
			bool flag = true;
			int num = FirstGlyph;
			int num2 = GlyphCount(Table);
			ushort num3 = 1;
			while (num3 < num2 && flag)
			{
				num = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num + 1, LookupFlags, 1);
				if (num >= AfterLastGlyph)
				{
					flag = false;
				}
				else
				{
					ushort num4 = ClassId(Table, num3);
					flag = ClassDef.GetClass(Table, GlyphInfo.Glyphs[num]) == num4;
				}
				num3++;
			}
			if (flag)
			{
				ContextualLookups(Table).ApplyContextualLookups(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, LookupFlags, FirstGlyph, num + 1, Parameter, nestingLevel, out NextGlyph);
			}
			return flag;
		}

		public SubClassRule(int Offset)
		{
			offset = Offset;
		}
	}

	private const int offsetFormat = 0;

	private const int offsetCoverage = 2;

	private const int offsetClassDef = 4;

	private const int offsetSubClassSetCount = 6;

	private const int offsetSubClassSetArray = 8;

	private const int sizeClassSetOffset = 2;

	private int offset;

	public ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private CoverageTable Coverage(FontTable Table)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + 2));
	}

	private ClassDefTable ClassDef(FontTable Table)
	{
		return new ClassDefTable(offset + Table.GetUShort(offset + 4));
	}

	private ushort ClassSetCount(FontTable Table)
	{
		return Table.GetUShort(offset + 6);
	}

	private SubClassSet ClassSet(FontTable Table, ushort Index)
	{
		int uShort = Table.GetUShort(offset + 8 + Index * 2);
		if (uShort == 0)
		{
			return new SubClassSet(int.MaxValue);
		}
		return new SubClassSet(offset + uShort);
	}

	public unsafe bool Apply(IOpenTypeFont Font, OpenTypeTags TableTag, FontTable Table, LayoutMetrics Metrics, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets, ushort LookupFlags, int FirstGlyph, int AfterLastGlyph, uint Parameter, int nestingLevel, out int NextGlyph)
	{
		Invariant.Assert(Format(Table) == 2);
		NextGlyph = FirstGlyph + 1;
		_ = GlyphInfo.Length;
		ushort glyph = GlyphInfo.Glyphs[FirstGlyph];
		if (Coverage(Table).GetGlyphIndex(Table, glyph) < 0)
		{
			return false;
		}
		ClassDefTable classDef = ClassDef(Table);
		ushort @class = classDef.GetClass(Table, glyph);
		if (@class >= ClassSetCount(Table))
		{
			return false;
		}
		SubClassSet subClassSet = ClassSet(Table, @class);
		if (subClassSet.IsNull)
		{
			return false;
		}
		ushort num = subClassSet.RuleCount(Table);
		bool flag = false;
		ushort num2 = 0;
		while (!flag && num2 < num)
		{
			flag = subClassSet.Rule(Table, num2).Apply(Font, TableTag, Table, Metrics, classDef, CharCount, Charmap, GlyphInfo, Advances, Offsets, LookupFlags, FirstGlyph, AfterLastGlyph, Parameter, nestingLevel, out NextGlyph);
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

	public ClassContextSubtable(int Offset)
	{
		offset = Offset;
	}
}
