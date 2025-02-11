namespace MS.Internal.Shaping;

internal struct ReverseChainingSubtable
{
	private const int offsetFormat = 0;

	private const int offsetCoverage = 2;

	private const int offsetBacktrackGlyphCount = 4;

	private const int sizeCount = 2;

	private const int sizeOffset = 2;

	private const int sizeGlyphId = 2;

	private int offset;

	private ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private CoverageTable InputCoverage(FontTable Table)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + 2));
	}

	private CoverageTable Coverage(FontTable Table, int Offset)
	{
		return new CoverageTable(offset + Table.GetUShort(Offset));
	}

	private ushort GlyphCount(FontTable Table, int Offset)
	{
		return Table.GetUShort(Offset);
	}

	private static ushort Glyph(FontTable Table, int Offset)
	{
		return Table.GetUShort(Offset);
	}

	public unsafe bool Apply(IOpenTypeFont Font, OpenTypeTags TableTag, FontTable Table, LayoutMetrics Metrics, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets, ushort LookupFlags, int FirstGlyph, int AfterLastGlyph, uint Parameter, out int NextGlyph)
	{
		NextGlyph = AfterLastGlyph - 1;
		if (Format(Table) != 1)
		{
			return false;
		}
		bool flag = true;
		int num = AfterLastGlyph - 1;
		int glyphIndex = InputCoverage(Table).GetGlyphIndex(Table, GlyphInfo.Glyphs[num]);
		if (glyphIndex < 0)
		{
			return false;
		}
		int num2 = offset + 4;
		ushort num3 = GlyphCount(Table, num2);
		num2 += 2;
		int num4 = num;
		ushort num5 = 0;
		while (num5 < num3 && flag)
		{
			num4 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num4 - 1, LookupFlags, -1);
			if (num4 < 0)
			{
				flag = false;
			}
			else
			{
				flag = Coverage(Table, num2).GetGlyphIndex(Table, GlyphInfo.Glyphs[num4]) >= 0;
				num2 += 2;
			}
			num5++;
		}
		ushort num6 = GlyphCount(Table, num2);
		num2 += 2;
		num4 = num;
		ushort num7 = 0;
		while (num7 < num6 && flag)
		{
			num4 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num4 + 1, LookupFlags, 1);
			if (num4 >= GlyphInfo.Length)
			{
				flag = false;
			}
			else
			{
				flag = Coverage(Table, num2).GetGlyphIndex(Table, GlyphInfo.Glyphs[num4]) >= 0;
				num2 += 2;
			}
			num7++;
		}
		if (flag)
		{
			num2 += 2 + 2 * glyphIndex;
			GlyphInfo.Glyphs[num] = Glyph(Table, num2);
			GlyphInfo.GlyphFlags[num] = 23;
		}
		return flag;
	}

	public bool IsLookupCovered(FontTable table, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId)
	{
		return true;
	}

	public CoverageTable GetPrimaryCoverage(FontTable table)
	{
		return InputCoverage(table);
	}

	public ReverseChainingSubtable(int Offset)
	{
		offset = Offset;
	}
}
