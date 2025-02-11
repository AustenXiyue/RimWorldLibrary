namespace MS.Internal.Shaping;

internal struct ClassChainingSubtable
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
		private const int sizeCount = 2;

		private const int sizeClassId = 2;

		private int offset;

		public static ushort GlyphCount(FontTable Table, int Offset)
		{
			return Table.GetUShort(Offset);
		}

		public static ushort ClassId(FontTable Table, int Offset)
		{
			return Table.GetUShort(Offset);
		}

		public ContextualLookupRecords ContextualLookups(FontTable Table, int CurrentOffset)
		{
			return new ContextualLookupRecords(CurrentOffset + 2, Table.GetUShort(CurrentOffset));
		}

		public unsafe bool Apply(IOpenTypeFont Font, OpenTypeTags TableTag, FontTable Table, LayoutMetrics Metrics, ClassDefTable inputClassDef, ClassDefTable backtrackClassDef, ClassDefTable lookaheadClassDef, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets, ushort LookupFlags, int FirstGlyph, int AfterLastGlyph, uint Parameter, int nestingLevel, out int NextGlyph)
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
					ushort num5 = ClassId(Table, num);
					num += 2;
					flag = backtrackClassDef.GetClass(Table, GlyphInfo.Glyphs[num3]) == num5;
				}
				num4++;
			}
			if (!flag)
			{
				return false;
			}
			int num6 = GlyphCount(Table, num);
			num += 2;
			num3 = FirstGlyph;
			ushort num7 = 1;
			while (num7 < num6 && flag)
			{
				num3 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num3 + 1, LookupFlags, 1);
				if (num3 >= AfterLastGlyph)
				{
					flag = false;
				}
				else
				{
					ushort num8 = ClassId(Table, num);
					num += 2;
					flag = inputClassDef.GetClass(Table, GlyphInfo.Glyphs[num3]) == num8;
				}
				num7++;
			}
			if (!flag)
			{
				return false;
			}
			int afterLastGlyph = num3 + 1;
			int num9 = GlyphCount(Table, num);
			num += 2;
			ushort num10 = 0;
			while (num10 < num9 && flag)
			{
				num3 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num3 + 1, LookupFlags, 1);
				if (num3 >= GlyphInfo.Length)
				{
					flag = false;
				}
				else
				{
					ushort num11 = ClassId(Table, num);
					num += 2;
					flag = lookaheadClassDef.GetClass(Table, GlyphInfo.Glyphs[num3]) == num11;
				}
				num10++;
			}
			if (flag)
			{
				ContextualLookups(Table, num).ApplyContextualLookups(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, LookupFlags, FirstGlyph, afterLastGlyph, Parameter, nestingLevel, out NextGlyph);
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

	private const int offsetBacktrackClassDef = 4;

	private const int offsetInputClassDef = 6;

	private const int offsetLookaheadClassDef = 8;

	private const int offsetSubClassSetCount = 10;

	private const int offsetSubClassSetArray = 12;

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

	private ClassDefTable BacktrackClassDef(FontTable Table)
	{
		return new ClassDefTable(offset + Table.GetUShort(offset + 4));
	}

	private ClassDefTable InputClassDef(FontTable Table)
	{
		return new ClassDefTable(offset + Table.GetUShort(offset + 6));
	}

	private ClassDefTable LookaheadClassDef(FontTable Table)
	{
		return new ClassDefTable(offset + Table.GetUShort(offset + 8));
	}

	private ushort ClassSetCount(FontTable Table)
	{
		return Table.GetUShort(offset + 10);
	}

	private SubClassSet ClassSet(FontTable Table, ushort Index)
	{
		int uShort = Table.GetUShort(offset + 12 + Index * 2);
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
		ClassDefTable inputClassDef = InputClassDef(Table);
		ClassDefTable backtrackClassDef = BacktrackClassDef(Table);
		ClassDefTable lookaheadClassDef = LookaheadClassDef(Table);
		ushort @class = inputClassDef.GetClass(Table, glyph);
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
			flag = subClassSet.Rule(Table, num2).Apply(Font, TableTag, Table, Metrics, inputClassDef, backtrackClassDef, lookaheadClassDef, CharCount, Charmap, GlyphInfo, Advances, Offsets, LookupFlags, FirstGlyph, AfterLastGlyph, Parameter, nestingLevel, out NextGlyph);
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

	public ClassChainingSubtable(int Offset)
	{
		offset = Offset;
	}
}
