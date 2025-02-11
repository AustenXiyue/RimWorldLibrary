namespace MS.Internal.Shaping;

internal struct ContextSubtable
{
	private const int offsetFormat = 0;

	private int offset;

	private ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	public unsafe bool Apply(IOpenTypeFont Font, OpenTypeTags TableTag, FontTable Table, LayoutMetrics Metrics, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets, ushort LookupFlags, int FirstGlyph, int AfterLastGlyph, uint Parameter, int nestingLevel, out int NextGlyph)
	{
		switch (Format(Table))
		{
		case 1:
			return new GlyphContextSubtable(offset).Apply(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, LookupFlags, FirstGlyph, AfterLastGlyph, Parameter, nestingLevel, out NextGlyph);
		case 2:
			return new ClassContextSubtable(offset).Apply(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, LookupFlags, FirstGlyph, AfterLastGlyph, Parameter, nestingLevel, out NextGlyph);
		case 3:
			return new CoverageContextSubtable(offset).Apply(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, LookupFlags, FirstGlyph, AfterLastGlyph, Parameter, nestingLevel, out NextGlyph);
		default:
			NextGlyph = FirstGlyph + 1;
			return false;
		}
	}

	public bool IsLookupCovered(FontTable table, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId)
	{
		return Format(table) switch
		{
			1 => new GlyphContextSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId), 
			2 => new ClassContextSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId), 
			3 => new CoverageContextSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId), 
			_ => true, 
		};
	}

	public CoverageTable GetPrimaryCoverage(FontTable table)
	{
		return Format(table) switch
		{
			1 => new GlyphContextSubtable(offset).GetPrimaryCoverage(table), 
			2 => new ClassContextSubtable(offset).GetPrimaryCoverage(table), 
			3 => new CoverageContextSubtable(offset).GetPrimaryCoverage(table), 
			_ => CoverageTable.InvalidCoverage, 
		};
	}

	public ContextSubtable(int Offset)
	{
		offset = Offset;
	}
}
